using FishNet;
using FishNet.Managing.Server;
using FishNet.Transporting;
using TMPro;
using UnityEngine;

public class JoinCodeText : MonoBehaviour
{
    private const string NoGameRunning = "Join Code: No game running...";

    [SerializeField]
    private TMP_Text _joinCodeText;

    private RelayManager _relayManager;
    private ServerManager _serverManager;

    private void OnEnable()
    {
        _relayManager = UnityServiceFinder.RelayManager;
        _serverManager = InstanceFinder.ServerManager;
        _relayManager.OnCreatedAllocationCodeRetrieved += OnCreatedAllocationCodeRetrieved;
        _serverManager.OnServerConnectionState += OnServerConnectionState;
        InstanceFinder.ClientManager.OnClientConnectionState += OnClientConnectionState;
        UnityServiceFinder.RelayManager.OnJoinAllocation += OnJoinAllocation;

        _joinCodeText.text = NoGameRunning;
    }

    private void OnDisable()
    {
        _relayManager.OnCreatedAllocationCodeRetrieved -= OnCreatedAllocationCodeRetrieved;
        _serverManager.OnServerConnectionState -= OnServerConnectionState;
        InstanceFinder.ClientManager.OnClientConnectionState -= OnClientConnectionState;
        UnityServiceFinder.RelayManager.OnJoinAllocation -= OnJoinAllocation;
    }

    private void OnJoinAllocation(RelayManager.JoinAllocationEventData data)
    {
        if (data.DidSucceed)
        {
            _joinCodeText.text = $"Join Code: {data.JoinCode}";
        }
    }

    private void OnServerConnectionState(ServerConnectionStateArgs state)
    {
        if (state.ConnectionState == LocalConnectionState.Stopping)
        {
            _joinCodeText.text = NoGameRunning;
        }
    }

    private void OnClientConnectionState(ClientConnectionStateArgs state)
    {
        if (state.ConnectionState == LocalConnectionState.Stopped)
        {
            _joinCodeText.text = NoGameRunning;
        }
    }

    private void OnCreatedAllocationCodeRetrieved(string text)
    {
        _joinCodeText.text = $"Join Code: {text}";
    }
}