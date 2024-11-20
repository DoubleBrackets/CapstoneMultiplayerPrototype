using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SyncVarTester : NetworkBehaviour
{
    [SerializeField]
    private Button _changeSyncVarButton;

    [SerializeField]
    private TMP_Text _syncVarText;

    private readonly SyncVar<int> _syncVar = new(0);

    public override void OnStartServer()
    {
        _changeSyncVarButton.onClick.AddListener(ChangeSyncVar);
    }

    public override void OnStartClient()
    {
        if (!ServerManager.Started)
        {
            _changeSyncVarButton.interactable = false;
        }

        _syncVar.OnChange += SyncVar_OnValueChanged;
    }

    public override void OnStopServer()
    {
        _changeSyncVarButton.onClick.RemoveListener(ChangeSyncVar);
    }

    public override void OnStopClient()
    {
        _syncVar.OnChange -= SyncVar_OnValueChanged;
    }

    private void SyncVar_OnValueChanged(int prev, int next, bool asserver)
    {
        BadLogger.LogInfo("SyncVar Changed" +
                          "\n" +
                          $"Received Tick: {TimeManager.Tick}, Received Local Tick {TimeManager.LocalTick}",
            BadLogger.Actor.Client);
        _syncVarText.text = next.ToString();
    }

    [Server]
    private void ChangeSyncVar()
    {
        BadLogger.LogInfo("Changing SyncVar" +
                          "\n" +
                          $"Change Tick: {TimeManager.Tick}, Change Local Tick {TimeManager.LocalTick}",
            BadLogger.Actor.Server);
        _syncVar.Value++;
    }
}