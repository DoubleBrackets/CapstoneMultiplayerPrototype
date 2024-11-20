using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Minigames.DebugMinigame
{
    public class OwnedCspTester : NetworkBehaviour, INetworkParentable
    {
        public struct ReplicateData : IReplicateData
        {
            public int SomeData;

            public ReplicateData(int someData)
            {
                SomeData = someData;
                _tick = 0;
            }

            private uint _tick;

            public void Dispose()
            {
            }

            public uint GetTick()
            {
                return _tick;
            }

            public void SetTick(uint value)
            {
                _tick = value;
            }
        }

        //Reconcile structure.
        public struct ReconcileData : IReconcileData
        {
            public int SomeData;

            public ReconcileData(int someData)
            {
                SomeData = someData;
                _tick = 0;
            }

            private uint _tick;

            public void Dispose()
            {
            }

            public uint GetTick()
            {
                return _tick;
            }

            public void SetTick(uint value)
            {
                _tick = value;
            }
        }

        [SerializeField]
        private Toggle _logToggle;

        [SerializeField]
        private TMP_Text _toggleLabel;

        private bool _log;

        private int _data;

        private void OnEnable()
        {
            _logToggle.onValueChanged.AddListener(OnLogToggle);
        }

        private void OnDisable()
        {
            _logToggle.onValueChanged.RemoveListener(OnLogToggle);
        }

        [Server]
        public void SetParent(string parent)
        {
            ObserversRpc_SetParent(parent);
        }

        public override void OnStartClient()
        {
            _log = false;
            _logToggle.isOn = false;
            _toggleLabel.text = "Log OwnedCspTester: " + OwnerId;
        }

        private void OnLogToggle(bool value)
        {
            _log = value;
        }

        public override void OnStartNetwork()
        {
            BadLogger.LogInfo($"{OwnerId} OnStartNetwork");
            TimeManager.OnTick += TimeManager_OnTick;
            TimeManager.OnPostTick += TimeManager_OnPostTick;
        }

        public override void OnStopNetwork()
        {
            BadLogger.LogInfo($"{OwnerId} OnStopNetwork");
            TimeManager.OnTick -= TimeManager_OnTick;
            TimeManager.OnPostTick -= TimeManager_OnPostTick;
        }

        private void TimeManager_OnTick()
        {
            LocalLog("OnTick | " +
                     "\n" +
                     $"Current Tick {TimeManager.Tick} | " +
                     $"Current LocalTick {TimeManager.LocalTick}");

            if (HasAuthority)
            {
                var data = new ReplicateData((int)TimeManager.Tick);
                RunInputs(data);
            }
            else
            {
                RunInputs(default);
            }
        }

        [Replicate]
        private void RunInputs(ReplicateData data, ReplicateState state = ReplicateState.Invalid,
            Channel channel = Channel.Unreliable)
        {
            LocalLog("RunInputs | " +
                     $"{state} | " +
                     $"Data: {data.SomeData}" +
                     "\n" +
                     $"Data Tick {data.GetTick()} | " +
                     $"Current Tick {TimeManager.Tick} | " +
                     $"Current LocalTick {TimeManager.LocalTick}");

            _data = data.SomeData;
        }

        private void TimeManager_OnPostTick()
        {
            LocalLog("OnPostTick | " +
                     "\n" +
                     $"Current Tick {TimeManager.Tick} | " +
                     $"Current LocalTick {TimeManager.LocalTick}");

            if (ServerManager.Started)
            {
                CreateReconcile();
            }
        }

        public override void CreateReconcile()
        {
            var recData = new ReconcileData(_data);
            LocalLog("CreateReconcile | " +
                     $"Data: {recData.SomeData}" +
                     "\n" +
                     $"Data Tick {recData.GetTick()} | " +
                     $"Current Tick {TimeManager.Tick} | " +
                     $"Current LocalTick {TimeManager.LocalTick}");
            ReconcileState(recData);
        }

        [Reconcile]
        private void ReconcileState(ReconcileData data, Channel channel = Channel.Unreliable)
        {
            LocalLog("Reconciling | " +
                     $"Data: {data.SomeData}" +
                     "\n" +
                     $"Data Tick {data.GetTick()} | " +
                     $"Current Tick {TimeManager.Tick} | " +
                     $"Current LocalTick {TimeManager.LocalTick}");
        }

        private void LocalLog(string message)
        {
            if (_log)
            {
                BadLogger.LogInfo(OwnerId + " " + message);
            }
        }

        [ObserversRpc(BufferLast = true)]
        private void ObserversRpc_SetParent(string parent)
        {
            transform.SetParent(GameObject.Find(parent).transform);
            transform.localScale = Vector3.one;
        }
    }
}