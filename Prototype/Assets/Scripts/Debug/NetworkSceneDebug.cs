using System;
using FishNet.Object;
using UnityEngine;

public class NetworkSceneDebug : NetworkBehaviour
{
    private void OnGUI()
    {
        if (ServerManager)
        {
            var sceneConnections = SceneManager.SceneConnections;

            foreach (var pair in sceneConnections)
            {
                var scene = pair.Key;
                var conns = pair.Value;
                
                GUILayout.Label($"Scene: {scene.name}");
                
                foreach (var conn in conns)
                {
                    GUILayout.Label($"Connection: {conn}");
                }
            }
        }
    }
}
