using System.Collections.Generic;
using DebugTools;
using FishNet;
using FishNet.Component.Prediction;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using GameKit.Dependencies.Utilities;
using Minigames.BallBounce;
using UnityEngine;
using Random = UnityEngine.Random;

public class Ball : NetworkBehaviour
{
    //Replicate structure.
    public struct ReplicateData : IReplicateData
    {
        public Vector2 Vel;
        public float AngleVel;
        public bool WasBumped;

        public ReplicateData(Vector2 vel, float angleVel, bool wasBumped)
        {
            WasBumped = wasBumped;
            Vel = vel;
            AngleVel = angleVel;
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

    private const float LineLength = 0.25f;

    [SerializeField]
    private SpriteRenderer _halo;

    [SerializeField]
    private float _bizmoDuration;

    private readonly List<uint> _bumpTicks = new();

    //Forces are not applied in this example but you
    //could definitely still apply forces to the PredictionRigidbody
    //even with no controller, such as if you wanted to bump it
    //with a player.
    private PredictionRigidbody2D _predictionRigidbody;
    private Rigidbody2D _rb;
    private bool _wasBumped;

    private Rigidbody2DState _rbState;
    private bool _frozen;

    private Rigidbody2DState _bumpState;

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

        if (Input.GetKeyDown(KeyCode.Y))
        {
            _wasBumped = true;
            _rb.linearVelocity = Random.insideUnitCircle * 20f;
            _rb.angularVelocity = Random.Range(-360f, 360f);
            _bumpState = new Rigidbody2DState(_rb);
            ServerRpc_DoBump(IsOwner ? 0 : TimeManager.Tick, 0.25f, Owner);
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
            /*if (ServerManager.Started)
            {
                BadLogger.LogDebug($"Bumped by protag, giving ownership to {protag.Owner.ClientId}",
                    BadLogger.Actor.Server);
                GiveOwnership(protag.Owner);
                _timeSinceLastBump = 0f;
            }*/
            if (protag.IsOwner)
            {
                BadLogger.LogDebug($"Bumped by protag, giving ownership to {protag.Owner.ClientId}",
                    BadLogger.Actor.Client);
                _bumpState = new Rigidbody2DState(_rb);
                if (_bumpState.Velocity.y <= 5f)
                {
                    _bumpState.Velocity.y = 5f;
                }

                ServerRpc_DoBump(IsOwner ? 0 : TimeManager.Tick, 0.25f, Owner);
            }

            _wasBumped = true;
        }
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
    private void ServerRpc_DoBump(uint tick, float duration, NetworkConnection conn = null)
    {
        if (tick != 0)
        {
            _bumpTicks.Add(tick);
        }

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
        BadLogger.LogTrace(TimeManager.Tick.ToString());
        Unfreeze();

        if (HasAuthority)
        {
            var data = new ReplicateData(_bumpState.Velocity, _bumpState.AngularVelocity, _wasBumped);
            bool injectedBumpTick = IsServerInitialized && _bumpTicks.Contains(TimeManager.Tick);
            if (injectedBumpTick)
            {
                data.WasBumped = true;
            }

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
        bool doNotUseData = !IsServerInitialized && !IsOwner && !state.IsReplayed();

        if (!doNotUseData)
        {
            if (data.WasBumped)
            {
                _rb.linearVelocity = data.Vel;
                _rb.angularVelocity = data.AngleVel;

                if (IsServerInitialized && Owner != LocalConnection)
                {
                    BadLogger.LogDebug($"Replicate Ball: {data.WasBumped} {state} {TimeManager.Tick}");
                }
            }
        }

        // Debug bizmos
        Color c = Color.white;
        Vector2 dir = Vector2.up;
        var showVel = false;
        switch (state)
        {
            case ReplicateState.Invalid:
                c = Color.white;
                break;
            case ReplicateState.CurrentCreated:
                c = Color.green;
                dir = Vector2.one;
                showVel = true;
                break;
            case ReplicateState.ReplayedCreated:
                c = Color.cyan;
                dir = Vector2.right;
                break;
            case ReplicateState.CurrentFuture:
                c = Color.red;
                dir = Vector2.up;
                break;
            case ReplicateState.ReplayedFuture:
                dir = Vector2.right + Vector2.down;
                break;
        }

        Vector2 pos = _rb.position;
        Bizmos.Instance.AddBizmo(
            new LineBizmo(pos, pos + dir * LineLength, c),
            _bizmoDuration);

        if (showVel)
        {
            Bizmos.Instance.AddGuiBizmo(
                new TextBizmo(pos + dir * (LineLength * 3f), data.GetTick().ToString() + _rb.linearVelocity, c),
                _bizmoDuration);
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

        for (var i = 0; i < _bumpTicks.Count; i++)
        {
            if (_bumpTicks[i] < TimeManager.Tick)
            {
                _bumpTicks.RemoveAt(i);
                i--;
            }
        }

        Vector2 pos = rd.RbState.Position;
        Bizmos.Instance.AddBizmo(
            new LineBizmo(pos, pos + Vector2.left * LineLength, Color.blue),
            _bizmoDuration);

        Bizmos.Instance.AddGuiBizmo(
            new TextBizmo(pos + Vector2.left * (LineLength * 3f), TimeManager.Tick.ToString() + _rb.linearVelocity,
                Color.blue),
            _bizmoDuration);
    }

    [Reconcile]
    private void ReconcileState(ReconcileData data, Channel channel = Channel.Unreliable)
    {
        Unfreeze();

        BadLogger.LogTrace($"Reconcile Ball: {TimeManager.Tick}");

        _rb.SetState(data.RbState);

        Vector2 pos = data.RbState.Position;

        Bizmos.Instance.AddBizmo(
            new LineBizmo(pos, pos + -Vector2.one * LineLength, Color.magenta),
            _bizmoDuration);

        Bizmos.Instance.AddGuiBizmo(
            new TextBizmo(pos + -Vector2.one * (LineLength * 3f), data.GetTick().ToString() + _rb.linearVelocity,
                Color.magenta),
            _bizmoDuration);
    }
}