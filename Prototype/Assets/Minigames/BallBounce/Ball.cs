using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using GameKit.Dependencies.Utilities;
using PlatformController;
using UnityEngine;

namespace Minigames.BallBounce
{
    public class Ball : NetworkBehaviour
    {
        public struct ReplicateData : IReplicateData
        {
            public ReplicateData(bool wasBumped)
            {
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

        public struct ReconcileData : IReconcileData
        {
            public PredictionRigidbody2D predictionRb;

            public ReconcileData(PredictionRigidbody2D pr) : this()
            {
                predictionRb = pr;
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

        private const float LineLength = 0.25f;

        [SerializeField]
        private SpriteRenderer _halo;

        [SerializeField]
        private float _bizmoDuration;

        private PredictionRigidbody2D _predictionRigidbody;
        private Rigidbody2D _rb;

        private float _timeSinceLastBump;
        private float _loseOwnershipTime;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _predictionRigidbody = ObjectCaches<PredictionRigidbody2D>.Retrieve();
            _predictionRigidbody.Initialize(_rb);
        }

        private void Update()
        {
            if (Owner.IsValid)
            {
                Color ownerColor = ServerNetworkPlayerDataManager.Instance.GetPlayerData(Owner).UserColor;
                _halo.color = ownerColor;
            }
            else
            {
                _halo.color = Color.white;
            }

            _timeSinceLastBump += Time.deltaTime;
            if (IsServerStarted && _timeSinceLastBump > _loseOwnershipTime && Owner.IsValid)
            {
                BadLogger.LogDebug("Removing ownership due to inactivity", BadLogger.Actor.Server);
                RemoveOwnership();
            }
        }

        private void OnDestroy()
        {
            ObjectCaches<PredictionRigidbody2D>.StoreAndDefault(ref _predictionRigidbody);
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            var ground = other.gameObject.GetComponent<ScoreGround>();
            if (ground)
            {
                if (InstanceFinder.ServerManager.Started)
                {
                    BallBounceScoreManager.Instance.ResetScore();
                }
            }

            var protag = other.gameObject.GetComponent<NetworkProtag>();
            /*if (protag && !IsBehaviourReconciling)
            {
                _bumpState = new Rigidbody2DState(_rb);
                if (_bumpState.Velocity.y <= _verticalBumpVel)
                {
                    _bumpState.Velocity.y = _verticalBumpVel;
                }

                if (!IsServerStarted)
                {
                    InjectPredictedBump();
                    _rb.SetState(_bumpState);
                }
                else
                {
                    _wasBumped = true;
                }

                if (protag.IsOwner)
                {
                    BadLogger.LogDebug($"Bumped by protag, giving ownership to {protag.Owner.ClientId}",
                        BadLogger.Actor.Client);
                    // ServerRpc_ChangeOwners(0.25f, Owner);
                }
            }*/
        }

        private void OnCollisionStay2D(Collision2D other)
        {
            var ground = other.gameObject.GetComponent<ScoreGround>();
            if (ground)
            {
                if (InstanceFinder.ServerManager.Started)
                {
                    BallBounceScoreManager.Instance.ResetScore();
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void ServerRpc_ChangeOwners(float duration, NetworkConnection conn = null)
        {
            GiveOwnership(conn);
            _loseOwnershipTime = duration;
            _timeSinceLastBump = 0f;
        }

        //In this example we do not need to use OnTick, only OnPostTick.
        //Because input is not processed on this object you only
        //need to pass in default for RunInputs, which can safely
        //be done in OnPostTick.
        public override void OnStartNetwork()
        {
            TimeManager.OnTick += TimeManager_OnTick;
            TimeManager.OnPostTick += TimeManager_OnPostTick;
        }

        public override void OnStopNetwork()
        {
            TimeManager.OnTick -= TimeManager_OnTick;
            TimeManager.OnPostTick -= TimeManager_OnPostTick;
        }

        private void TimeManager_OnTick()
        {
            if (IsController)
            {
                var data = new ReplicateData(false);
                RunInputs(data);
            }
            else
            {
                RunInputs(default);
            }
        }

        private void TimeManager_OnPostTick()
        {
            if (!IsServerStarted)
            {
                return;
            }

            CreateReconcile();
        }

        [Replicate]
        private void RunInputs(ReplicateData data, ReplicateState state = ReplicateState.Invalid,
            Channel channel = Channel.Unreliable)
        {
            BadLogger.LogTrace(
                $"Replicating {state.ContainsTicked()} {state.ContainsReplayed()} {state.ContainsCreated()} tick {data.GetTick()} {name}");
        }

        public override void CreateReconcile()
        {
            var rd = new ReconcileData(_predictionRigidbody);
            ReconcileState(rd);
        }

        [Reconcile]
        private void ReconcileState(ReconcileData data, Channel channel = Channel.Unreliable)
        {
            BadLogger.LogTrace(
                $"Reconciled tick {data.GetTick()} {name}");
            _predictionRigidbody.Reconcile(data.predictionRb);
        }
    }
}