using System;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;

/// <summary>
///     Server owned manager that stores player data and syncs back to clients
/// </summary>
public class ServerNetworkPlayerDataManager : NetworkBehaviour
{
    public static ServerNetworkPlayerDataManager Instance { get; private set; }

    public event Action OnPlayerDataChange;

    private readonly SyncDictionary<NetworkConnection, OfflinePlayerDataManager.PublicPlayerData> _playerData = new();

    public override void OnStartNetwork()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        InstanceFinder.ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;

        _playerData.OnChange += PlayerDataOnChange;
        Instance = this;
    }

    public override void OnStopNetwork()
    {
        InstanceFinder.ServerManager.OnRemoteConnectionState -= OnRemoteConnectionState;

        _playerData.OnChange -= PlayerDataOnChange;
    }

    public override void OnStartClient()
    {
        // Send local client data to server
        BadLogger.LogDebug($"Sending local player data to server: Client {LocalConnection.ClientId}",
            BadLogger.Actor.Client);

        if (Instance.NetworkManager.IsHostStarted)
        {
            Server_SetPlayerData(OfflinePlayerDataManager.Instance.OfflineLocalPlayerData, LocalConnection);
        }
        else
        {
            ServerRPC_SetSelfPlayerData(OfflinePlayerDataManager.Instance.OfflineLocalPlayerData);
        }
    }

    public OfflinePlayerDataManager.PublicPlayerData GetPlayerData(NetworkConnection conn)
    {
        if (_playerData.ContainsKey(conn))
        {
            return _playerData[conn];
        }

        BadLogger.LogWarning("Player data not found for connection: " + conn);
        return new OfflinePlayerDataManager.PublicPlayerData();
    }

    private void PlayerDataOnChange(
        SyncDictionaryOperation op,
        NetworkConnection key,
        OfflinePlayerDataManager.PublicPlayerData value,
        bool asserver)
    {
        OnPlayerDataChange?.Invoke();
    }

    [Server]
    private void OnRemoteConnectionState(NetworkConnection conn, RemoteConnectionStateArgs args)
    {
        if (args.ConnectionState == RemoteConnectionState.Stopped)
        {
            // Remove from data
            _playerData.Remove(conn);
        }
    }

    [ServerRpc(RequireOwnership = false, RunLocally = true)]
    private void ServerRPC_SetSelfPlayerData(OfflinePlayerDataManager.PublicPlayerData data,
        NetworkConnection conn = null)
    {
        Server_SetPlayerData(data, conn);
    }

    [Server]
    private void Server_SetPlayerData(OfflinePlayerDataManager.PublicPlayerData data, NetworkConnection conn)
    {
        BadLogger.LogDebug("Updating player data for connection: " + conn, BadLogger.Actor.Server);
        _playerData[conn] = data;
    }
}