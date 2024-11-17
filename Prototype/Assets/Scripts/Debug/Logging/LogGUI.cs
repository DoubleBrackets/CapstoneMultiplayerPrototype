using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-1)]
public class LogGUI : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _logText;

    [SerializeField]
    private GameObject _debugObject;

    [SerializeField]
    private RectTransform _container;

    [SerializeField]
    private ScrollRect _scrollRect;

    private void Awake()
    {
        _logText.text = string.Empty;
        Application.logMessageReceived += HandleLog;
        _debugObject.SetActive(true);
        BadLogger.LogDebug("Started up logging UI.");
    }

    private void Start()
    {
        // Disable in start; disabling in awake leads to incorrect height at start
        _debugObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.L))
        {
            _debugObject.SetActive(!_debugObject.activeSelf);
        }
    }

    private void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void FitText()
    {
        float preferredHeight = _logText.preferredHeight;
        _container.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferredHeight + 15);
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        _logText.text += $"[{type}] {logString} \n";
        FitText();
        // Scroll to bottom
        _scrollRect.normalizedPosition = new Vector2(0, 0);
    }
}