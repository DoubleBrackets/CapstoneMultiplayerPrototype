using TMPro;
using UnityEngine;

public class LogGUI : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _logText;

    [SerializeField]
    private GameObject _debugObject;

    private void Awake()
    {
        _logText.text = string.Empty;
        BadLogger.LogDebug("Started up logging UI.");
        _debugObject.SetActive(Application.isEditor);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            _debugObject.SetActive(!_debugObject.activeSelf);
        }
    }

    private void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        _logText.text += $"[{type}] {logString} \n";
    }
}