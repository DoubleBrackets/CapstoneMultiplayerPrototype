using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.UI;

public class RelayHostUI : MonoBehaviour
{
    [SerializeField]
    private RelayDropdown _relayDropdown;

    [SerializeField]
    private Button _hostButton;

    [SerializeField]
    private TMP_Text _statusText;

    private RelayManager _relayManager;

    private void Awake()
    {
        _hostButton.onClick.AddListener(OnHostButtonClicked);
    }

    private void Start()
    {
        _relayManager = UnityServiceFinder.RelayManager;
        _relayManager.OnCreateAllocation += OnCreateAllocation;
        SetupRegionsAsync().Forget();
    }

    private void OnDestroy()
    {
        _relayManager.OnCreateAllocation -= OnCreateAllocation;
    }

    private void OnCreateAllocation(RelayManager.CreateAllocationEventData data)
    {
        _statusText.text = data.DidSucceed
            ? "Successfully hosted"
            : $"Failed to host: {data.FailureReason}";
    }

    private async UniTaskVoid SetupRegionsAsync()
    {
        try
        {
            List<Region> regions = await _relayManager.GetRegionList(gameObject.GetCancellationTokenOnDestroy());
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
            await _relayManager.HostGameAsync(_relayDropdown.RegionId, gameObject.GetCancellationTokenOnDestroy());
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
}