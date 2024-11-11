using UnityEngine;

/// <summary>
///     Instantiates singleton object containing services
/// </summary>
public class ServicesInstantiator : MonoBehaviour
{
    [SerializeField]
    private GameObject _singletonPrefab;

    public static ServicesInstantiator Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            SpawnSingleton(_singletonPrefab);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void SpawnSingleton(GameObject singleton)
    {
        Instantiate(singleton, transform);
        DontDestroyOnLoad(gameObject);
    }
}