using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RelayHostUI : MonoBehaviour
{
    [SerializeField]
    private RelayDropdown _relayDropdown;
    
    [SerializeField]
    private RelayManager _relayManager;
    
    [SerializeField]
    private Button _hostButton;
    
    [SerializeField]
    private TMP_Text _statusText;
    
    private void Awake()
    {
        _hostButton.onClick.AddListener(OnHostButtonClicked);
    }

    private void Start()
    {
        SetupRegionsAsync().Forget();
    }

    private async UniTaskVoid SetupRegionsAsync()
    {
        try
        {
            var regions = await _relayManager.GetRegionList(gameObject.GetCancellationTokenOnDestroy());
            _relayDropdown.SetupRegionDropdown(regions);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    private void OnHostButtonClicked()
    {
        HostGameAsync(_relayDropdown.RegionId, gameObject.GetCancellationTokenOnDestroy()).Forget();
    }
    
    private async UniTaskVoid HostGameAsync(string regionId, CancellationToken token)
    {
        try
        {
            var code = await _relayManager.HostGameAsync(_relayDropdown.RegionId, gameObject.GetCancellationTokenOnDestroy());
            _statusText.text = $"Hosting game with code: {code}";
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
}
