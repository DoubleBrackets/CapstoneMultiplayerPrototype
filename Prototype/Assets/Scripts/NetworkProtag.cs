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
        public float JumpHeight;
        public float Gravity;
        public Vector2 GroundCheckOffset;
        public Vector2 GroundCheckSize;
        public LayerMask GroundLayer;
    }
    
    private struct MovementData : IReplicateData
    {
        public readonly float Horizontal;
        
        public MovementData(float horizontal)
        {
            Horizontal = horizontal;
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
        public readonly Vector2 Position;
        
        private uint _tick;
        
        public ReconcileData(Vector2 position)
        {
            Position = position;
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

    public override void OnStartClient()
    {
        gameObject.name = "NetworkProtag";
        if (Owner.IsHost)
            gameObject.name += "[Host]";
        else
            gameObject.name += "[Client]";
        gameObject.name += $"[Owner={OwnerId}]";
    }

    public override void OnStartNetwork()
    {
        TimeManager.OnTick += TimeManagerTickEventHandler;
        
        TimeManager.OnPostTick  += TimeManagerPostTickEventHandler;
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
            _horizontalInput = Input.GetAxis("Horizontal");
            _jumpInput = Input.GetButton("Jump");
        }
    }

    private void TimeManagerTickEventHandler()
    {
        if (IsOwner)
        {
            MovementData data = new MovementData(_horizontalInput);
            Replicate(data);
        }
        else
        {
            Replicate(default);
        }
    }

    private void TimeManagerPostTickEventHandler()
    {
        if (!IsServerStarted) return;
        
        CreateReconcile();
    }
    
    [Replicate]
    private void Replicate(
        MovementData data, 
        ReplicateState replicateState = ReplicateState.Invalid, 
        Channel channel = Channel.Unreliable)
    {
        Vector2 linearVel = Vector2.zero;
        
        linearVel.x = data.Horizontal * _moveStats.MoveSpeed;

        _bodyAnchor.position += (Vector3)linearVel * Time.fixedDeltaTime;

        Debug.Log($"Replicating for {name} to {_bodyAnchor.position}");
    }
    
    public override void CreateReconcile()
    {
        ReconcileData data = new ReconcileData(_bodyAnchor.position);
        Reconcile(data);
    }
    
    [Reconcile]
    private void Reconcile(ReconcileData data,  Channel channel = Channel.Unreliable)
    {
        _bodyAnchor.position = data.Position;
        
        Debug.Log($"Reconciled to {data.Position} for {name}");
    }

    private bool UpdateGroundCheck()
    {
        Vector2 checkPos = (Vector2)_bodyAnchor.position + _moveStats.GroundCheckOffset;
        Collider2D hit = Physics2D.OverlapBox(checkPos, _moveStats.GroundCheckSize, 0, _moveStats.GroundLayer);
        
        return hit != null;
    }
    
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube((Vector2)_bodyAnchor.position + _moveStats.GroundCheckOffset, _moveStats.GroundCheckSize);
    }
}
