using UnityEngine;
using FishNet.Object;
using FishNet.Connection;
using UnityEngine.InputSystem;


public class PlayerPickup : NetworkBehaviour
{
    
    private InputSystem_Actions _controls;
    private InputAction _interactAction;

    private GameObject _objectInHand;
    [SerializeField] private Transform _holdPosition;
    [SerializeField] private LayerMask _pickupLayer;
    [SerializeField] private Transform _bodyAnchor;



    public void OnInteract(InputAction.CallbackContext obj)
    {
        if (_objectInHand != null)
        {
            Debug.Log("dropping obj: " + _objectInHand.name);
            RPC_DropObject(_objectInHand);
            return;
        }

        Collider2D[] hits = Physics2D.OverlapBoxAll(_bodyAnchor.position, new Vector2(2,2), 0, _pickupLayer);
        if (hits.Length > 0)
        {
            Debug.Log("picking up obj: " + hits[0].gameObject.name);
            RPC_PickUpObject(hits[0].gameObject, _holdPosition.position, _holdPosition.rotation);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RPC_PickUpObject(GameObject obj, Vector2 position, Quaternion rotation, NetworkConnection conn = null)
    {

        if (obj == null || Vector2.Distance(_bodyAnchor.position, obj.transform.position) > 2)
        {
            Debug.Log("object is too far away or does not exist.");
            return;
        }
        obj.GetComponent<NetworkObject>().GiveOwnership(conn);
        RPC_PickUpObjectClient(obj, position, rotation);

    }
    
    [ObserversRpc]
    private void RPC_PickUpObjectClient(GameObject obj, Vector2 position, Quaternion rotation)
    {
        obj.transform.GetComponent<NetworkObject>().SetParent(_holdPosition.GetComponent<NetworkObject>());
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;

        
        if (obj.GetComponent<Rigidbody2D>() != null)
            obj.GetComponent<Rigidbody2D>().simulated = false;
        
        if (obj.GetComponent<Collider2D>() != null)
            obj.GetComponent<Collider2D>().enabled = false;
        
        _objectInHand = obj;
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void RPC_DropObject(GameObject obj)
    {
        obj.GetComponent<NetworkObject>().RemoveOwnership();
        RPC_DropObjectClient(_objectInHand);
    }
    
    [ObserversRpc]
    private void RPC_DropObjectClient(GameObject obj)
    {
        obj.transform.parent = null;
        
        if (obj.GetComponent<Rigidbody2D>() != null)
            obj.GetComponent<Rigidbody2D>().simulated = true;
        
        if (obj.GetComponent<Collider2D>() != null)
            obj.GetComponent<Collider2D>().enabled = true;
            
        _objectInHand = null;
    }
    
    
    
    
    
    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!base.IsOwner)
        {
            enabled = false;
            return;
        }

        InitializeControls();
    }
    
    public override void OnStopClient()
    {
        base.OnStopClient();
        
        DeInitializeControls();
    }
    
    
    private void InitializeControls()
    {
        _controls = new InputSystem_Actions();
        
        _interactAction = _controls.Player.Interact;
        _interactAction.Enable();
        _interactAction.performed += OnInteract;

    }
    
    private void DeInitializeControls()
    {
        _interactAction.Disable();
        _interactAction.performed -= OnInteract;

    }
}
