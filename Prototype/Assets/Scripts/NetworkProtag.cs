using System;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using UnityEngine;
using UnityEngine.Serialization;

public class NetworkProtag : NetworkBehaviour
{
    [SerializeField]
    private Rigidbody2D _rb;

    [SerializeField]
    private Transform _bodyAnchor;

    [System.Serializable]
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
        public uint GetTick() => _tick;

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
        
        public uint GetTick() => _tick;

        public void SetTick(uint value) => _tick = value;

        public void Dispose()
        {
            
        }
    }
    
    [SerializeField]
    private MoveStats _moveStats;

    private float _horizontalInput;
    private bool _jumpInput;
    
    private PredictionRigidbody2D _predictionRigidbody;

    private void Awake()
    {
        _predictionRigidbody = new PredictionRigidbody2D();
        _predictionRigidbody.Initialize(_rb);
        Debug.Log($"Initialized PredictionRigidbody for {name}");
    }

    public override void OnStartNetwork()
    {
        TimeManager.OnTick += TimeManagerTickEventHandler;
        TimeManager.OnPostTick  += TimeManagerPostTickEventHandler;
        
        gameObject.name = "NetworkProtag";
        if (Owner.IsHost)
            gameObject.name += "[Host]";
        else
            gameObject.name += "[Client]";
        gameObject.name += $"[Owner={OwnerId}]";

        Color c = Color.red;
        if (OwnerId == 0)
            c = Color.green;
        else if (OwnerId == 1)
            c = Color.blue;
        
        GetComponentInChildren<SpriteRenderer>().color = c;
    }

    public override void OnStopNetwork()
    {
        TimeManager.OnTick -= TimeManagerTickEventHandler;
        
        TimeManager.OnPostTick  -= TimeManagerPostTickEventHandler;
    }


    private void Update()
    {
        if (IsOwner)
        {
            _horizontalInput = Input.GetAxisRaw("Horizontal");
            _jumpInput = _jumpInput || Input.GetButtonDown("Jump");
        }
    }

    private void TimeManagerTickEventHandler()
    {
        if (IsOwner)
        {
            MovementData data = new MovementData(_horizontalInput, _jumpInput);
            Replicate(data);
        }
        else
        {
            Replicate(default);
        }
    }

    private void TimeManagerPostTickEventHandler()
    {
        _jumpInput = false;
        
        // Only the server should be able to reconcile clients
        if (!IsServerStarted) return;

        CreateReconcile();
    }
    
    [Replicate]
    private void Replicate(
        MovementData data, 
        ReplicateState replicateState = ReplicateState.Invalid, 
        Channel channel = Channel.Unreliable)
    {
        var currentVel = _rb.linearVelocity;

        Vector2 desiredVel = currentVel;
        desiredVel.x = Mathf.MoveTowards(
            currentVel.x, 
            data.Horizontal * _moveStats.MoveSpeed, 
            _moveStats.MoveAccel * Time.fixedDeltaTime);
        
        desiredVel.y += _moveStats.Gravity * 0.5f * Time.fixedDeltaTime;
        

        if (data.Jump && UpdateGroundCheck())
        {
            var jumpVel = Mathf.Sqrt( 2 * -_moveStats.Gravity * _moveStats.JumpHeight);
            _predictionRigidbody.AddForce(Vector2.up * jumpVel, ForceMode2D.Impulse);
            desiredVel.y = jumpVel;
        }
        else
        {
            _predictionRigidbody.AddForce(Vector2.up * _moveStats.Gravity);
        }
        
        desiredVel.y += _moveStats.Gravity * 0.5f * Time.fixedDeltaTime;
        
        _predictionRigidbody.AddForce(Vector2.right * (desiredVel.x - currentVel.x), ForceMode2D.Impulse);

        // _predictionRigidbody.Velocity(desiredVel);
        _predictionRigidbody.Simulate();
    }
    
    public override void CreateReconcile()
    {
        ReconcileData data = new ReconcileData(_predictionRigidbody);
        Reconcile(data);
        BadLogger.LogTrace("Creating Reconcile for " + name, BadLogger.Actor.Server);
    }
    
    [Reconcile]
    private void Reconcile(ReconcileData data,  Channel channel = Channel.Unreliable)
    {
        BadLogger.LogTrace($"Reconciling {name}", BadLogger.Actor.Client);
        _predictionRigidbody.Reconcile(data.PredictionRigidbody);
    }

    private bool UpdateGroundCheck()
    {
        Vector2 checkPos = _rb.position + _moveStats.GroundCheckOffset;
        Collider2D hit = Physics2D.OverlapBox(checkPos, _moveStats.GroundCheckSize, 0, _moveStats.GroundLayer);
        
        return hit != null;
    }
    
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube((Vector2)_bodyAnchor.position + _moveStats.GroundCheckOffset, _moveStats.GroundCheckSize);
    }
}
