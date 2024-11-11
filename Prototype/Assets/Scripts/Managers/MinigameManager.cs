using System.Collections.Generic;
using FishNet;
using FishNet.Managing.Scened;
using FishNet.Managing.Server;
using FishNet.Transporting;
using GameKit.Dependencies.Utilities.Types;
using UnityEngine;
using UnityEngine.SceneManagement;
using SceneManager = UnityEngine.SceneManagement.SceneManager;

public class MinigameManager : MonoBehaviour
{
    [SerializeField]
    [Scene]
    private List<string> _minigameScenes;

    [SerializeField]
    [Scene]
    private string _mainMenuScene;

    public static MinigameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InstanceFinder.ServerManager.OnServerConnectionState += OnServerConnectionState;
        InstanceFinder.ClientManager.OnClientConnectionState += OnClientConnectionState;
    }

    private void OnDestroy()
    {
        InstanceFinder.ServerManager.OnServerConnectionState -= OnServerConnectionState;
        InstanceFinder.ClientManager.OnClientConnectionState -= OnClientConnectionState;
    }

    private void OnServerConnectionState(ServerConnectionStateArgs state)
    {
        if (state.ConnectionState == LocalConnectionState.Stopped)
        {
            SceneManager.LoadScene(_mainMenuScene);
        }
        else if (state.ConnectionState == LocalConnectionState.Started)
        {
            InstanceFinder.SceneManager.OnLoadEnd += UnloadMainMenu;
            LoadMinigame(0);
        }
    }

    private void UnloadMainMenu(SceneLoadEndEventArgs args)
    {
        InstanceFinder.SceneManager.OnLoadEnd -= UnloadMainMenu;
        Scene scene = SceneManager.GetSceneByPath(_mainMenuScene);
        SceneManager.UnloadSceneAsync(scene);
    }

    private void OnClientConnectionState(ClientConnectionStateArgs state)
    {
        if (state.ConnectionState == LocalConnectionState.Stopped && !InstanceFinder.ServerManager.Started)
        {
            SceneManager.LoadScene(_mainMenuScene);
        }

        if (state.ConnectionState == LocalConnectionState.Started)
        {
            InstanceFinder.SceneManager.OnLoadEnd += UnloadMainMenu;
        }
    }

    public void ChangeMinigames(int index)
    {
        UnloadMinigame();
        LoadMinigame(index);
    }

    private void UnloadMinigame()
    {
        var sceneUnloadData = new SceneUnloadData(SceneManager.GetActiveScene());
        InstanceFinder.SceneManager.UnloadGlobalScenes(sceneUnloadData);
    }

    private void LoadMinigame(int index)
    {
        ServerManager serverManager = InstanceFinder.ServerManager;
        if (!serverManager.Started)
        {
            return;
        }

        string scene = _minigameScenes[index];
        var sceneLoadData = new SceneLoadData(scene);
        InstanceFinder.SceneManager.LoadGlobalScenes(sceneLoadData);
    }
}