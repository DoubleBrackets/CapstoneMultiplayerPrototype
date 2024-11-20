using FishNet.Object;
using UnityEngine;
using UnityEngine.UI;

namespace Minigames.DebugMinigame
{
    public class RpcTester : NetworkBehaviour
    {
        [SerializeField]
        private Button _sendObserversRpcButton;

        [SerializeField]
        private Button _sendServerRpcButton;

        public override void OnStartServer()
        {
            _sendObserversRpcButton.onClick.AddListener(SendObserversRpc);
        }

        public override void OnStartClient()
        {
            if (!ServerManager.Started)
            {
                _sendObserversRpcButton.interactable = false;
            }

            _sendServerRpcButton.onClick.AddListener(SendServerRpc);
        }

        public override void OnStopServer()
        {
            _sendObserversRpcButton.onClick.RemoveListener(SendObserversRpc);
        }

        public override void OnStopClient()
        {
            _sendServerRpcButton.onClick.RemoveListener(SendServerRpc);
        }

        private void SendObserversRpc()
        {
            if (ServerManager.Started)
            {
                BadLogger.LogInfo(
                    "Sending Observers Test Rpc to Clients \n" +
                    $"Send Tick={TimeManager.Tick}, Send LocalTick={TimeManager.LocalTick}",
                    BadLogger.Actor.Server);
                ObserversRpc_TestRpc(TimeManager.Tick, TimeManager.LocalTick);
            }
        }

        [ObserversRpc]
        private void ObserversRpc_TestRpc(uint serverSendTick, uint serverSendLocalTick)
        {
            BadLogger.LogInfo(
                "Received Observers Test Rpc from Server \n" +
                $"Server send Tick={serverSendTick}, Server send LocalTick={serverSendLocalTick} |" +
                $"Receive Tick={TimeManager.Tick}, Receive LocalTick={TimeManager.LocalTick}",
                BadLogger.Actor.Client);
        }

        private void SendServerRpc()
        {
            if (ClientManager.Started)
            {
                BadLogger.LogInfo(
                    "Sending Server Test Rpc to Server \n" +
                    $"Send Tick={TimeManager.Tick}, Send LocalTick={TimeManager.LocalTick}",
                    BadLogger.Actor.Client);
                ServerRpc_TestRpc(TimeManager.Tick, TimeManager.LocalTick);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void ServerRpc_TestRpc(uint clientSendTick, uint clientSendLocalTick)
        {
            BadLogger.LogInfo(
                "Received Server Test Rpc from Client \n" +
                $"Client send Tick={clientSendTick}, Client send LocalTick={clientSendLocalTick} |" +
                $"Receive Tick={TimeManager.Tick}, Receive LocalTick={TimeManager.LocalTick}",
                BadLogger.Actor.Server);
        }
    }
}