using System;
using FishNet.Component.Prediction;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using UnityEngine;

public class NetworkProtag : NetworkBehaviour
{
    [Serializable]
    public struct MoveStats
    {
        public float MoveSpeed;
        public float MoveAccel;
        public float JumpHeight;
        public float Gravity;
        public Vector2 GroundCheckOffset;
        public Vector2 GroundCheckSize;
        public LayerMask GroundLayer;
    }

    private struct MovementData : IReplicateData
    {
        public readonly float Horizontal;
        public readonly bool Jump;

        public MovementData(float horizontal, bool jump)
        {
            Horizontal = horizontal;
            Jump = jump;
            _tick = 0;
        }

        private uint _tick;

        public uint GetTick()
        {
            return _tick;
        }

        public void SetTick(uint value)
        {
            _tick = value;
        }

        public void Dispose()
        {
            // Internal to Fishnet
        }
    }

    private struct ReconcileData : IReconcileData
    {
        public readonly PredictionRigidbody2D PredictionRigidbody;

        private uint _tick;

        public ReconcileData(PredictionRigidbody2D predictionRigidbody)
        {
            PredictionRigidbody = predictionRigidbody;
            _tick = 0;
        }

        public uint GetTick()
        {
            return _tick;
        }

        public void SetTick(uint value)
        {
            _tick = value;
        }

        public void Dispose()
        {
        }
    }

    [SerializeField]
    private Rigidbody2D _rb;

    [SerializeField]
    private Transform _bodyAnchor;

    [SerializeField]
    private Animator _animator;

    [SerializeField]
    private SpriteRenderer _spriteRenderer;

    [SerializeField]
    private MoveStats _moveStats;

    private float _horizontalInput;
    private bool _jumpInput;
    
    private PredictionRigidbody2D _predictionRigidbody;
    
    private Rigidbody2DState _rbState;
    private bool _frozen;
    
    private int _lastHorizontal;

    private void Awake()
    {
        _predictionRigidbody = new PredictionRigidbody2D();
        _predictionRigidbody.Initialize(_rb);
        Debug.Log($"Initialized PredictionRigidbody for {name}");
    }

    private void Update()
    {
        if (IsOwner)
        {
            _horizontalInput = Input.GetAxisRaw("Horizontal");
            _jumpInput = _jumpInput || Input.GetButtonDown("Jump");
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube((Vector2)_bodyAnchor.position + _moveStats.GroundCheckOffset, _moveStats.GroundCheckSize);
    }

    public override void OnStartNetwork()
    {
        TimeManager.OnTick += TimeManager_OnTick;
        TimeManager.OnPostTick += TimeManager_PostTick;
        PredictionManager.OnPrePhysicsTransformSync +=  PredictionManager_OnPrePhysicsTransformSync;

        gameObject.name = "NetworkProtag";
        if (Owner.IsHost)
        {
            gameObject.name += "[Host]";
        }
        else
        {
            gameObject.name += "[Client]";
        }

        gameObject.name += $"[Owner={OwnerId}]";

        Color c = Color.red;
        if (OwnerId == 0)
        {
            c = Color.green;
        }
        else if (OwnerId == 1)
        {
            c = Color.blue;
        }

        GetComponentInChildren<SpriteRenderer>().color = c;
    }

    public override void OnStopNetwork()
    {
        TimeManager.OnTick -= TimeManager_OnTick;
        TimeManager.OnPostTick -= TimeManager_PostTick;
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
            var data = new MovementData(_horizontalInput, _jumpInput);
            Replicate(data);
        }
        else
        {
            Replicate(default);
        }
    }

    private void TimeManager_PostTick()
    {
        _jumpInput = false;
        CreateReconcile();
    }

    [Replicate]
    private void Replicate(
        MovementData data,
        ReplicateState replicateState = ReplicateState.Invalid,
        Channel channel = Channel.Unreliable)
    {
        BadLogger.LogTrace($"Replicating {replicateState} {data.Horizontal} {data.Jump} tick {data.GetTick()} {name}");
        var delta = (float)TimeManager.TickDelta;

        float horizontal = data.Horizontal;
        if (replicateState == ReplicateState.ReplayedFuture && !IsServerInitialized)
        {
            Freeze();
            return;
        }

        // If we're missing a packet, we'll just use the last horizontal input
        // This is to smooth packet loss out on observers, since player movement is generally continuous
        if (replicateState == ReplicateState.CurrentFuture)
        {
            horizontal = _lastHorizontal;
            Debug.DrawLine(_rb.position, _rb.position + Vector2.up, Color.red, 2f);
        }
        else if (replicateState == ReplicateState.CurrentCreated)
        {
            _lastHorizontal = (int)horizontal;
            Debug.DrawLine(_rb.position, _rb.position + Vector2.up, Color.green, 2f);
        }

        Vector2 currentVel = _rb.linearVelocity;

        Vector2 desiredVel = currentVel;
        desiredVel.x = Mathf.MoveTowards(
            currentVel.x,
            horizontal * _moveStats.MoveSpeed,
            _moveStats.MoveAccel * delta);

        bool isGrounded = UpdateGroundCheck();
        if (isGrounded)
        {
            if (data.Jump)
            {
                float jumpVel = Mathf.Sqrt(2 * -_moveStats.Gravity * _moveStats.JumpHeight);
                _predictionRigidbody.AddForce(Vector2.up * jumpVel, ForceMode2D.Impulse);
                // desiredVel.y = jumpVel;
            }
        }
        else
        {
            _predictionRigidbody.AddForce(Vector2.up * _moveStats.Gravity);
           // desiredVel.y += _moveStats.Gravity * delta;
        }

        _predictionRigidbody.AddForce(Vector3.right * (desiredVel.x - currentVel.x), ForceMode2D.Impulse);
        // _predictionRigidbody.Velocity(desiredVel);
        
        _predictionRigidbody.Simulate();

        if (data.Horizontal > 0)
        {
            _spriteRenderer.flipX = false;
        }
        else if (data.Horizontal < 0)
        {
            _spriteRenderer.flipX = true;
        }

        if (replicateState != ReplicateState.ReplayedFuture)
        {
            _animator.SetFloat("Speed", Mathf.Abs(_rb.linearVelocity.x) * Mathf.Abs(horizontal));
            _animator.SetBool("Air", !isGrounded);
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
        if (!IsServerStarted)
        {
            return;
        }

        var data = new ReconcileData(_predictionRigidbody);
        Reconcile(data);
    }

    [Reconcile]
    private void Reconcile(ReconcileData data, Channel channel = Channel.Unreliable)
    {
        BadLogger.LogTrace($"Reconciling {name} tick {data.GetTick()}", BadLogger.Actor.Client);
        Unfreeze();
        _predictionRigidbody.Reconcile(data.PredictionRigidbody);
        Debug.DrawLine(_rb.position, _rb.position + Vector2.up, Color.yellow, 2f);
    }

    private bool UpdateGroundCheck()
    {
        Vector2 checkPos = _rb.position + _moveStats.GroundCheckOffset;
        Collider2D hit = Physics2D.OverlapBox(checkPos, _moveStats.GroundCheckSize, 0, _moveStats.GroundLayer);

        return hit != null;
    }
}