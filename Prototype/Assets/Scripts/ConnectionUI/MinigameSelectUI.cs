using System.Collections.Generic;
using System.Linq;
using FishNet;
using FishNet.Transporting;
using TMPro;
using UnityEngine;

public class MinigameSelectUI : MonoBehaviour
{
    [SerializeField]
    private TMP_Dropdown _minigameDropdown;

    private void Start()
    {
        InstanceFinder.ServerManager.OnServerConnectionState += OnServerConnectionState;

        SetupDropdown(MinigameManager.Instance.MinigameScenes.ToList());
        _minigameDropdown.onValueChanged.AddListener(OnMinigameDropdownValueChanged);
        _minigameDropdown.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        InstanceFinder.ServerManager.OnServerConnectionState -= OnServerConnectionState;
        _minigameDropdown.onValueChanged.RemoveListener(OnMinigameDropdownValueChanged);
    }

    private void OnServerConnectionState(ServerConnectionStateArgs args)
    {
        if (args.ConnectionState == LocalConnectionState.Started)
        {
            _minigameDropdown.gameObject.SetActive(true);
        }
        else if (args.ConnectionState == LocalConnectionState.Stopped)
        {
            _minigameDropdown.gameObject.SetActive(false);
        }
    }

    private void OnMinigameDropdownValueChanged(int index)
    {
        MinigameManager.Instance.OnMiniGameLoaded += OnMinigameLoaded;
        _minigameDropdown.interactable = false;
        MinigameManager.Instance.ChangeMinigames(index);
    }

    private void OnMinigameLoaded()
    {
        MinigameManager.Instance.OnMiniGameLoaded -= OnMinigameLoaded;
        _minigameDropdown.interactable = true;
    }

    private void SetupDropdown(List<string> minigameNames)
    {
        List<string> names = minigameNames.Select(scene => scene.Split('/').Last().Replace(".unity", "")).ToList();
        _minigameDropdown.ClearOptions();
        _minigameDropdown.AddOptions(names);
    }
}