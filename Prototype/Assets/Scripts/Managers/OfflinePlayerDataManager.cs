using UnityEngine;

/// <summary>
///     Manages local player's data when offline
/// </summary>
public class OfflinePlayerDataManager : MonoBehaviour
{
    public struct PublicPlayerData
    {
        public string Username;
    }

    public static OfflinePlayerDataManager Instance { get; private set; }
    public PublicPlayerData OfflineLocalPlayerData { get; set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void SetLocalPlayerName(string userName)
    {
        PublicPlayerData data = OfflineLocalPlayerData;
        data.Username = userName;
        OfflineLocalPlayerData = data;
    }
}