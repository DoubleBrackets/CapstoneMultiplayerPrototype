using System;
using FishNet.Object;
using UnityEngine;

public class NetworkSceneDebug : NetworkBehaviour
{
    private GUIStyle _style;

    private void Awake()
    {
        _style = new GUIStyle();
        _style.fontSize = 20;
        _style.normal.textColor = Color.black;
    }

    private void OnGUI()
    {
        if (ServerManager)
        {
            var sceneConnections = SceneManager.SceneConnections;

            foreach (var pair in sceneConnections)
            {
                var scene = pair.Key;
                var conns = pair.Value;
                
                GUILayout.Label($"Scene: {scene.name}", _style);
                
                foreach (var conn in conns)
                {
                    GUILayout.Label($"Connection: {conn}", _style);
                }
            }
        }

        if (IsClientInitialized)
        {
            GUILayout.Label($"Local Client: {LocalConnection.ClientId}" , _style);
        }
    }
}
 