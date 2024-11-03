using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using FishNet;
using FishNet.Managing.Server;
using FishNet.Transporting.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
///     Handles setting up Unity Relay service and hosting/connecting
/// </summary>
public class RelayManager : MonoBehaviour
{
    private enum RelayState
    {
        Disconnected,
        Host,
        Client
    }

    [SerializeField]
    private UnityServiceManager _unityCloudManager;

    [SerializeField]
    private FishyUnityTransport _fishyUnityTransport;

    public string JoinCode { get; private set; }

    public string RegionId => _currentAllocation?.Region ?? _currentJoinAllocation?.Region;

    private Allocation _currentAllocation;
    private JoinAllocation _currentJoinAllocation;

    private RelayState _relayState = RelayState.Disconnected;

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
        BadLogger.LogDebug($"Trying to host on region {regionId}");
        try
        {
            await _unityCloudManager.WaitForInitialization(token);
            
            _currentAllocation = await RelayService.Instance.CreateAllocationAsync(4, regionId);
            
            token.ThrowIfCancellationRequested();

            string joinCode =
                await RelayService.Instance.GetJoinCodeAsync(_currentAllocation.AllocationId);
            
            BadLogger.LogDebug($"Created Relay with id {_currentAllocation.AllocationId} " +
                          $"and code {joinCode} in region {_currentAllocation.Region}");
            
            token.ThrowIfCancellationRequested();

            // Codes are case insensitive, leave as upper since it's easier to read
            JoinCode = joinCode;
            
            // Copy to clipboard
            GUIUtility.systemCopyBuffer = joinCode;
            
            SetupTransport(_currentAllocation);

            _relayState = RelayState.Host;

            // Host is both server and client
            if (InstanceFinder.ServerManager.StartConnection())
            {
                InstanceFinder.ClientManager.StartConnection();
            }

            return JoinCode;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            _fishyUnityTransport.Shutdown();
            throw;
        }
    }

    /// <summary>
    ///     Attempt to join a host using a join code and specified region through relay & fishnet
    ///     Note that join codes are cross region, and determined by the host.
    /// </summary>
    /// <param name="joinCode"></param>
    /// <param name="token"></param>
    public async UniTask JoinGameAsync(string joinCode, CancellationToken token)
    {
        try
        {
            await _unityCloudManager.WaitForInitialization(token);
            
            BadLogger.LogDebug($"Trying to join with code {joinCode}");

            _currentJoinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
        
            token.ThrowIfCancellationRequested();

            SetupTransport(_currentJoinAllocation);
        
            BadLogger.LogDebug(_currentJoinAllocation.ToString());

            BadLogger.LogDebug($"Joined Relay with id {_currentJoinAllocation.AllocationId} " +
                               $"and code {joinCode} in region {_currentJoinAllocation.Region}");

            JoinCode = joinCode;

            _relayState = RelayState.Client;

            InstanceFinder.ClientManager.StartConnection();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            _fishyUnityTransport.Shutdown();
            throw;
        }
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
#if UNITY_WEBGL && !UNITY_EDITOR
        Logg.LogTrace("WebGL; using wss");
        _fishyUnityTransport.UseWebSockets = true;
        connectionType = "wss";
#else
        BadLogger.LogDebug("Not webgl; using dtls");
        connectionType = "dtls";
#endif
    }
}