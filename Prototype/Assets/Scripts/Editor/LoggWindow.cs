using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoggWindow : EditorWindow
{
    private BadLogger.Priority _logLevel = BadLogger.Priority.Trace;
    private bool _showTrace = true;

    private void OnGUI()
    {
        // log level dropdown
        _logLevel = (BadLogger.Priority)EditorGUILayout.EnumPopup("Log Level", _logLevel);
        BadLogger.LogLevel = _logLevel;
        
        _showTrace = EditorGUILayout.Toggle("Show Trace", _showTrace);
        BadLogger.ShowTrace = _showTrace;
    }

    [MenuItem("Tools/Logg")]
    public static void ShowWindow()
    {
        GetWindow<LoggWindow>("Logg");
    }
}