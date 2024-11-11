using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
///     Simple logging utility
/// </summary>
public class BadLogger : MonoBehaviour
{
    public enum Priority
    {
        Trace,
        Debug,
        Info,
        Warning,
        Error
    }

    public enum Actor
    {
        None,
        Client,
        Server
    }

    public static Priority LogLevel = Priority.Trace;
    public static bool ShowTrace = true;

    private void Awake()
    {
        if (ShowTrace)
        {
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.ScriptOnly);
        }
        else
        {
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
        }

        if (!Application.isEditor)
        {
            LogLevel = Priority.Debug;
        }
    }

    public static void LogTrace(string message, Actor actor = Actor.None, Object context = null)
    {
        Log(message, Priority.Trace, actor, context);
    }

    public static void LogDebug(string message, Actor actor = Actor.None, Object context = null)
    {
        Log(message, Priority.Debug, actor, context);
    }

    public static void LogInfo(string message, Actor actor = Actor.None, Object context = null)
    {
        Log(message, Priority.Info, actor, context);
    }

    public static void LogWarning(string message, Actor actor = Actor.None, Object context = null)
    {
        Log(message, Priority.Warning, actor, context);
    }

    public static void LogError(string message, Actor actor = Actor.None, Object context = null)
    {
        Log(message, Priority.Error, actor, context);
    }

    public static void Log(string message, Priority priority = Priority.Debug, Actor actor = Actor.None,
        Object context = null)
    {
        if (priority < LogLevel)
        {
            return;
        }

        string priorityString = priority.ToString().ToUpper();
        string actorString = actor.ToString().ToUpper();
        var prefix = $"[{priorityString}][{actorString}]";

        if (priority == Priority.Trace || priority == Priority.Debug || priority == Priority.Info)
        {
            Debug.Log($"{prefix} {message}", context);
        }
        else if (priority == Priority.Warning)
        {
            Debug.LogWarning($"{prefix} {message}", context);
        }
        else if (priority == Priority.Error)
        {
            Debug.LogError($"{prefix} {message}", context);
        }
    }
}