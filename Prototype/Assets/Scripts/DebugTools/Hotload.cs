using FishNet;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Hotload : MonoBehaviour
{
    [SerializeField]
    private MinigameListSO _minigameList;

    private void Start()
    {
        if (InstanceFinder.ClientManager == null)
        {
            SceneManager.LoadScene(_minigameList.MainMenuScene);
        }
    }
}