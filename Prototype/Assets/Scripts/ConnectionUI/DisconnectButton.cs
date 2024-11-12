using FishNet;
using FishNet.Transporting;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DisconnectButton : MonoBehaviour
{
    [SerializeField]
    private Button _disconnectButton;

    [SerializeField]
    private TMP_Text _disconnectText;

    private void Start()
    {
        InstanceFinder.ServerManager.OnServerConnectionState += OnServerConnectionState;
        InstanceFinder.ClientManager.OnClientConnectionState += OnClientConnectionState;
        _disconnectButton.onClick.AddListener(OnDisconnectButtonClicked);
        _disconnectButton.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        InstanceFinder.ServerManager.OnServerConnectionState -= OnServerConnectionState;
        InstanceFinder.ClientManager.OnClientConnectionState -= OnClientConnectionState;
        _disconnectButton.onClick.RemoveListener(OnDisconnectButtonClicked);
    }

    private void OnServerConnectionState(ServerConnectionStateArgs state)
    {
        if (state.ConnectionState == LocalConnectionState.Started)
        {
            BadLogger.LogDebug("Showing end session button; server started");
            _disconnectButton.gameObject.SetActive(true);
            _disconnectText.text = "End Session";
        }
        else if (state.ConnectionState == LocalConnectionState.Stopped)
        {
            BadLogger.LogDebug("Hiding end session button; server stopped");
            _disconnectButton.gameObject.SetActive(false);
        }
    }

    private void OnClientConnectionState(ClientConnectionStateArgs state)
    {
        if (InstanceFinder.ServerManager.Started)
        {
            return;
        }

        if (state.ConnectionState == LocalConnectionState.Started)
        {
            BadLogger.LogDebug("Showing disconnect button; client started");
            _disconnectButton.gameObject.SetActive(true);
            _disconnectText.text = "Disconnect";
        }
        else if (state.ConnectionState == LocalConnectionState.Stopping)
        {
            BadLogger.LogDebug("Hiding disconnect button; client stopping");
            _disconnectButton.gameObject.SetActive(false);
        }
    }

    private void OnDisconnectButtonClicked()
    {
        if (InstanceFinder.ServerManager.Started)
        {
            InstanceFinder.ServerManager.StopConnection(true);
        }
        else if (InstanceFinder.ClientManager.Started)
        {
            InstanceFinder.ClientManager.StopConnection();
        }
    }
}