using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using FishNet;
using FishNet.Transporting.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

/// <summary>
///     Handles setting up Unity Relay service and hosting/connecting
/// </summary>
public class RelayManager : MonoBehaviour
{
    public struct JoinAllocationEventData
    {
        public bool DidSucceed;
        public string JoinCode;
        public string FailureReason;

        public JoinAllocationEventData(bool didSucceed, string joinCode = "", string failureReason = "")
        {
            DidSucceed = didSucceed;
            JoinCode = joinCode;
            FailureReason = failureReason;
        }
    }

    public struct CreateAllocationEventData
    {
        public bool DidSucceed;
        public string FailureReason;

        public CreateAllocationEventData(bool result, string failureReason = "")
        {
            DidSucceed = result;
            FailureReason = failureReason;
        }
    }

    public const int MaxPlayers = 4;

    [SerializeField]
    private UnityServiceManager _unityCloudManager;

    [SerializeField]
    private FishyUnityTransport _fishyUnityTransport;

    public static List<RelayManager> Instances { get; } = new();

    public string JoinCode { get; private set; }

    public string RegionId => _currentCreatedAllocation?.Region ?? _currentJoinAllocation?.Region;

    public event Action<CreateAllocationEventData> OnCreateAllocation;

    public event Action<string> OnCreatedAllocationCodeRetrieved;
    public event Action<JoinAllocationEventData> OnJoinAllocation;

    private Allocation _currentCreatedAllocation;
    private JoinAllocation _currentJoinAllocation;

    private bool _inAllocationProgress;

    private void Awake()
    {
        Instances.Add(this);
    }

    private void OnDestroy()
    {
        Instances.Remove(this);
    }

    /// <summary>
    ///     Get a list of regions from the relay service
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public async UniTask<List<Region>> GetRegionList(CancellationToken token)
    {
        try
        {
            await _unityCloudManager.WaitForInitialization(token);

            token.ThrowIfCancellationRequested();
            List<Region> result = await RelayService.Instance.ListRegionsAsync();
            token.ThrowIfCancellationRequested();
            return result;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return null;
        }
    }

    /// <summary>
    ///     Attempt to start hosting a game server through the relay service and fishnet
    /// </summary>
    /// <param name="regionId">regionId, taken from region list</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async UniTask<string> HostGameAsync(string regionId, CancellationToken token)
    {
        Debug.Log("s");
        if (_inAllocationProgress || InstanceFinder.ServerManager.Started)
        {
            BadLogger.LogDebug("Already in allocation progress");
            return null;
        }

        BadLogger.LogDebug($"Trying to host on region {regionId}");

        _inAllocationProgress = true;
        try
        {
            await _unityCloudManager.WaitForInitialization(token);

            _currentCreatedAllocation =
                await RelayService.Instance.CreateAllocationAsync(MaxPlayers, regionId);

            token.ThrowIfCancellationRequested();

            OnCreateAllocation?.Invoke(new CreateAllocationEventData(true));

            BadLogger.LogDebug($"Created Relay with id {_currentCreatedAllocation.AllocationId} in region {regionId}");
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            _fishyUnityTransport.Shutdown();

            if (e is RelayServiceException relayServiceException)
            {
                OnCreateAllocation?.Invoke(
                    new CreateAllocationEventData(false, relayServiceException.Reason.ToString()));
            }
            else
            {
                OnCreateAllocation?.Invoke(new CreateAllocationEventData(false, "Exception thrown"));
            }

            _inAllocationProgress = false;
            throw;
        }

        string joinCode = null;
        try
        {
            joinCode =
                await RelayService.Instance.GetJoinCodeAsync(_currentCreatedAllocation.AllocationId);

            token.ThrowIfCancellationRequested();

            // Codes are case insensitive, leave as upper since it's easier to read
            JoinCode = joinCode;

            // Copy to clipboard
            GUIUtility.systemCopyBuffer = joinCode;

            OnCreatedAllocationCodeRetrieved?.Invoke(joinCode);
        }
        catch (Exception e)
        {
            OnCreatedAllocationCodeRetrieved?.Invoke("Failed to get join code");
            Debug.LogError(e);
            _fishyUnityTransport.Shutdown();
            _inAllocationProgress = false;

            throw;
        }

        try
        {
            SetupTransport(_currentCreatedAllocation);

            // Host is both server and client
            if (InstanceFinder.ServerManager.StartConnection())
            {
                InstanceFinder.ClientManager.StartConnection();
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            _fishyUnityTransport.Shutdown();
            _inAllocationProgress = false;
            throw;
        }

        _inAllocationProgress = false;
        return JoinCode;
    }

    /// <summary>
    ///     Attempt to join a host using a join code and specified region through relay & fishnet
    ///     Note that join codes are cross region, and determined by the host.
    /// </summary>
    /// <param name="joinCode"></param>
    /// <param name="token"></param>
    public async UniTask JoinGameAsync(string joinCode, CancellationToken token)
    {
        if (_inAllocationProgress || InstanceFinder.ClientManager.Started)
        {
            BadLogger.LogDebug("Already in allocation progress");
            return;
        }

        if (string.IsNullOrEmpty(joinCode) || joinCode.Length != 6)
        {
            OnJoinAllocation?.Invoke(
                new JoinAllocationEventData(false, joinCode, "Invalid Join Code!"));
            return;
        }

        _inAllocationProgress = true;

        try
        {
            await _unityCloudManager.WaitForInitialization(token);

            BadLogger.LogDebug($"Trying to join with code {joinCode}");

            _currentJoinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            token.ThrowIfCancellationRequested();

            BadLogger.LogDebug(_currentJoinAllocation.ToString());
            BadLogger.LogDebug($"Joined Relay with id {_currentJoinAllocation.AllocationId} " +
                               $"and code {joinCode} in region {_currentJoinAllocation.Region}");

            OnJoinAllocation?.Invoke(new JoinAllocationEventData(true, joinCode));
        }
        catch (Exception e)
        {
            Debug.LogError(e);

            if (e is RelayServiceException relayServiceException)
            {
                OnJoinAllocation?.Invoke(
                    new JoinAllocationEventData(false, failureReason: relayServiceException.Reason.ToString()));
            }
            else
            {
                OnJoinAllocation?.Invoke(new JoinAllocationEventData(false, "", "Exception thrown"));
            }

            _fishyUnityTransport.Shutdown();
            _inAllocationProgress = false;
            throw;
        }

        try
        {
            SetupTransport(_currentJoinAllocation);
            JoinCode = joinCode;
            InstanceFinder.ClientManager.StartConnection();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            _fishyUnityTransport.Shutdown();
            _inAllocationProgress = false;
            throw;
        }

        _inAllocationProgress = false;
    }

    private void SetupTransport(Allocation allocation)
    {
        ConfigureTransportType(out string connectionType);
        _fishyUnityTransport.SetRelayServerData(new RelayServerData(allocation, connectionType));
    }

    private void SetupTransport(JoinAllocation joinAllocation)
    {
        ConfigureTransportType(out string connectionType);
        _fishyUnityTransport.SetRelayServerData(new RelayServerData(joinAllocation, connectionType));
    }

    private void ConfigureTransportType(out string connectionType)
    {
        var isWebGL = false;
#if UNITY_WEBGL && !UNITY_EDITOR
        isWebGL = true;
#endif
        if (isWebGL)
        {
            BadLogger.LogTrace("WebGL; using wss");
            _fishyUnityTransport.UseWebSockets = true;
            connectionType = "wss";
        }
        else
        {
            BadLogger.LogDebug("Not webgl; using dtls");
            _fishyUnityTransport.UseWebSockets = false;
            connectionType = "dtls";
        }
    }
}