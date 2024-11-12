using TMPro;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class LogGUI : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _logText;

    [SerializeField]
    private GameObject _debugObject;

    private void Awake()
    {
        _logText.text = string.Empty;
        Application.logMessageReceived += HandleLog;
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

    private void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        _logText.text += $"[{type}] {logString} \n";
    }
}