using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayDropdown : MonoBehaviour
{
    private const string AutoRegion = "Auto";
    private const string PreferredRegionKey = "PreferredRegion";

    [SerializeField]
    private TMP_Dropdown _regionDropdown;

    public string RegionId
    {
        get
        {
            string id = _regionDropdown.options[_regionDropdown.value].text;
            return id == AutoRegion ? string.Empty : id;
        }
    }

    public event Action<string> RegionSelected;

    private void Awake()
    {
        _regionDropdown.onValueChanged.AddListener(OnRegionDropdownValueChanged);
    }

    private void OnDestroy()
    {
        _regionDropdown.onValueChanged.RemoveListener(OnRegionDropdownValueChanged);
    }

    private void OnRegionDropdownValueChanged(int index)
    {
        string selectedRegion = _regionDropdown.options[index].text;
        BadLogger.LogDebug($"Selected region: {selectedRegion}");
        RegionSelected?.Invoke(selectedRegion);
        PlayerPrefs.SetString(PreferredRegionKey, selectedRegion);
    }

    public void SetupRegionDropdown(List<Region> regions)
    {
        try
        {
            List<TMP_Dropdown.OptionData> options =
                regions.Select(r => new TMP_Dropdown.OptionData(r.Id)).ToList();

            // QOS autodetect best region isn't available in WebGL
#if !UNITY_WEBGL || UNITY_EDITOR
            options.Insert(0, new TMP_Dropdown.OptionData(AutoRegion));
#endif

            _regionDropdown.ClearOptions();
            _regionDropdown.AddOptions(options);

            SelectPlayerPreferredRegion(_regionDropdown, options);
        }
        catch (Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }

    /// <summary>
    ///     Select player's preferred region from playerdata
    /// </summary>
    /// <param name="dropdown"></param>
    /// <param name="options"></param>
    private void SelectPlayerPreferredRegion(TMP_Dropdown dropdown, List<TMP_Dropdown.OptionData> options)
    {
        string preferredRegion = PlayerPrefs.GetString(PreferredRegionKey);
        if (!string.IsNullOrEmpty(preferredRegion))
        {
            int preferredRegionIndex = options.FindIndex(o => o.text == preferredRegion);
            if (preferredRegionIndex != -1)
            {
                dropdown.value = preferredRegionIndex;
            }
        }
    }
}