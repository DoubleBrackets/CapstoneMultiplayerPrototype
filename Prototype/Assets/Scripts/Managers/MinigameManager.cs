using System;
using System.Collections.Generic;
using System.Linq;
using FishNet;
using FishNet.Managing.Scened;
using FishNet.Managing.Server;
using FishNet.Object;
using FishNet.Transporting;
using UnityEngine;
using UnityEngine.SceneManagement;
using SceneManager = UnityEngine.SceneManagement.SceneManager;

public class MinigameManager : MonoBehaviour
{
    [SerializeField]
    private MinigameListSO _minigameList;

    public static MinigameManager Instance { get; private set; }

    public IReadOnlyList<string> MinigameScenes => _minigameList.MinigameScenes;

    public event Action OnMiniGameLoaded;

    private bool _loading;

    private string _currentMinigameScene;

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
            SceneManager.LoadScene(_minigameList.MainMenuScene);
        }
        else if (state.ConnectionState == LocalConnectionState.Started)
        {
            InstanceFinder.SceneManager.OnLoadEnd += UnloadMainMenu;
            LoadMinigame(-1);
        }
    }

    private void UnloadMainMenu(SceneLoadEndEventArgs args)
    {
        InstanceFinder.SceneManager.OnLoadEnd -= UnloadMainMenu;
        Scene scene = SceneManager.GetSceneByPath(_minigameList.MainMenuScene);
        SceneManager.UnloadSceneAsync(scene);
    }

    private void OnClientConnectionState(ClientConnectionStateArgs state)
    {
        if (state.ConnectionState == LocalConnectionState.Stopped && !InstanceFinder.ServerManager.Started)
        {
            _loading = false;
            _currentMinigameScene = null;
            SceneManager.LoadScene(_minigameList.MainMenuScene);
        }

        if (state.ConnectionState == LocalConnectionState.Started)
        {
            InstanceFinder.SceneManager.OnLoadEnd += UnloadMainMenu;
        }
    }

    public void ChangeMinigames(int index)
    {
        if (!InstanceFinder.ServerManager.Started || _loading)
        {
            return;
        }

        _loading = true;

        UnloadMinigame();
        LoadMinigame(index);

        InstanceFinder.SceneManager.OnLoadEnd += OnMinigameLoaded;
    }

    private void OnMinigameLoaded(SceneLoadEndEventArgs arg)
    {
        InstanceFinder.SceneManager.OnLoadEnd -= OnMinigameLoaded;
        _loading = false;
        BadLogger.LogDebug("Minigame loaded!", BadLogger.Actor.Server);
        OnMiniGameLoaded?.Invoke();
    }

    private void UnloadMinigame()
    {
        if (string.IsNullOrEmpty(_currentMinigameScene))
        {
            return;
        }

        DespawnAllSceneObjects(_currentMinigameScene);

        var sceneUnloadData = new SceneUnloadData(_currentMinigameScene);

        BadLogger.LogInfo($"Unloading {_currentMinigameScene}");

        InstanceFinder.SceneManager.UnloadGlobalScenes(sceneUnloadData);
    }

    private void DespawnAllSceneObjects(string scene)
    {
        BadLogger.LogInfo($"Despawning all objects in {scene}");

        GameObject[] gameObjects = SceneManager.GetActiveScene().GetRootGameObjects();

        List<NetworkObject> nobs = gameObjects.SelectMany(a => a.GetComponentsInChildren<NetworkObject>()).ToList();

        foreach (NetworkObject no in nobs)
        {
            InstanceFinder.ServerManager.Despawn(no);
        }
    }

    private void LoadMinigame(int index)
    {
        ServerManager serverManager = InstanceFinder.ServerManager;
        if (!serverManager.Started)
        {
            return;
        }

        string scene = index == -1 ? _minigameList.StartScene : _minigameList.MinigameScenes[index];
        var sceneLoadData = new SceneLoadData(scene);
        sceneLoadData.Options.AutomaticallyUnload = true;
        sceneLoadData.PreferredActiveScene = new PreferredScene(new SceneLookupData(scene));

        InstanceFinder.SceneManager.LoadGlobalScenes(sceneLoadData);
        _currentMinigameScene = scene;
    }
}