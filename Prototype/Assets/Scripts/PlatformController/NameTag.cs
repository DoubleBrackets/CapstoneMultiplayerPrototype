using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using UnityEngine;

public class NameTag : NetworkBehaviour
{
    [SerializeField]
    private TMP_Text _nameText;

    private readonly SyncVar<string> _name = new();

    public override void OnStartClient()
    {
        _nameText.text = ServerNetworkPlayerDataManager.Instance.GetPlayerData(Owner).Username;
    }
}