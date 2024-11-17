using FishNet;
using FishNet.Component.Prediction;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using GameKit.Dependencies.Utilities;
using Minigames.BallBounce;
using UnityEngine;

public class Ball : NetworkBehaviour
{
    //Replicate structure.
    public struct ReplicateData : IReplicateData
    {
        public BasicRigidbody2DState Rigidbody2DState;
        public bool WasBumped;

        public ReplicateData(BasicRigidbody2DState rigidbody2DState, bool wasBumped)
        {
            WasBumped = wasBumped;
            Rigidbody2DState = rigidbody2DState;
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
        public Rigidbody2DState RbState;

        public ReconcileData(Rigidbody2DState pr) : this()
        {
            RbState = pr;
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
    private SpriteRenderer _halo;

    //Forces are not applied in this example but you
    //could definitely still apply forces to the PredictionRigidbody
    //even with no controller, such as if you wanted to bump it
    //with a player.
    private PredictionRigidbody2D _predictionRigidbody;
    private Rigidbody2D _rb;
    private bool _wasBumped;
    
    private Rigidbody2DState _rbState;
    private bool _frozen;

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
        if (protag)
        {
            if (ServerManager.Started)
            {
                BadLogger.LogDebug($"Bumped by protag, giving ownership to {protag.Owner.ClientId}",
                    BadLogger.Actor.Server);
                GiveOwnership(protag.Owner);
            }
            _wasBumped = true;
        }
    }

    //In this example we do not need to use OnTick, only OnPostTick.
    //Because input is not processed on this object you only
    //need to pass in default for RunInputs, which can safely
    //be done in OnPostTick.
    public override void OnStartNetwork()
    {
        TimeManager.OnTick += TimeManager_OnTick;
        TimeManager.OnPostTick += TimeManager_OnPostTick;
        PredictionManager.OnPrePhysicsTransformSync += PredictionManager_OnPrePhysicsTransformSync;
    }

    public override void OnStopNetwork()
    {
        TimeManager.OnTick -= TimeManager_OnTick;
        TimeManager.OnPostTick -= TimeManager_OnPostTick;
        PredictionManager.OnPrePhysicsTransformSync -= PredictionManager_OnPrePhysicsTransformSync;
    }
    
    private void PredictionManager_OnPrePhysicsTransformSync(uint clienttick, uint servertick)
    {
        // Prevent strange collision behaviors during reconciliation when one body is not replicate replaying
        if (!IsBehaviourReconciling)
        {
            Freeze();
        }
    }

    private void TimeManager_OnTick()
    {
        Unfreeze();

        if (HasAuthority)
        {
            var data = new ReplicateData(_rb.GetBasicState(), _wasBumped);
            RunInputs(data);
        }
        else
        {
            RunInputs(default);
        }

        _wasBumped = false;
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
        if (state == ReplicateState.ReplayedFuture && !IsServerInitialized)
        {
            Freeze();
            return;
        }
        
        if (data.WasBumped)
        {
            Vector2 lPos = _rb.position;
            _rb.SetBasicState(data.Rigidbody2DState);
            _rb.position = lPos;
            if (_rb.linearVelocity.y <= 5f)
            {
                _rb.linearVelocityY = 5f;
            }
            BadLogger.LogDebug($"Replicate Ball: {data.WasBumped} {state} {TimeManager.Tick}");
        }
    }
    
    
    private void Freeze()
    {
        if (_frozen)
        {
            return;
        }

        BadLogger.LogTrace($"Freezing {name}", BadLogger.Actor.Client);
        _frozen = true;
        _rbState = new Rigidbody2DState(_rb);
        _rb.bodyType = RigidbodyType2D.Static;
    }

    private void Unfreeze()
    {
        if (!_frozen)
        {
            return;
        }

        BadLogger.LogTrace($"Unfreeze {name}", BadLogger.Actor.Client);

        _frozen = false;
        _rb.bodyType = RigidbodyType2D.Dynamic;
        _rb.SetState(_rbState);
    }

    public override void CreateReconcile()
    {
        var rd = new ReconcileData(_rb.GetState());
        ReconcileState(rd);
    }

    [Reconcile]
    private void ReconcileState(ReconcileData data, Channel channel = Channel.Unreliable)
    {
        //Call reconcile on your PredictionRigidbody field passing in
        //values from data.
        if (HasAuthority)
        {
        }
        Unfreeze();

        BadLogger.LogDebug($"Reconcile Ball: {TimeManager.Tick}");

        _rb.SetState(data.RbState);
    }
}