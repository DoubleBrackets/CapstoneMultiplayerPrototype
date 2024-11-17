using UnityEngine;

/// <summary>
///     Manages local player's data when offline
/// </summary>
public class OfflinePlayerDataManager : MonoBehaviour
{
    public struct PublicPlayerData
    {
        public string Username;
        public Color UserColor;
    }

    private const string UsernamePlayerPrefsKey = "Username";
    private const string UserColorPlayerPrefsKey = "UserColor";

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

        LoadFromPrefs();
    }

    private void LoadFromPrefs()
    {
        string prefName = PlayerPrefs.GetString(UsernamePlayerPrefsKey, "");
        if (string.IsNullOrEmpty(prefName))
        {
            prefName = "Player" + Random.Range(1, 1000);
        }

        Color prefColor;
        string colorString = PlayerPrefs.GetString(UserColorPlayerPrefsKey, "#FFFFFF");

        ColorUtility.TryParseHtmlString("#" + colorString, out prefColor);

        OfflineLocalPlayerData = new PublicPlayerData
        {
            Username = prefName,
            UserColor = prefColor
        };
    }

    public void SetLocalPlayerName(string userName)
    {
        PublicPlayerData data = OfflineLocalPlayerData;
        data.Username = userName;
        OfflineLocalPlayerData = data;

        PlayerPrefs.SetString(UsernamePlayerPrefsKey, userName);
    }

    public void SetLocalPlayerColor(Color userColor)
    {
        PublicPlayerData data = OfflineLocalPlayerData;
        data.UserColor = userColor;
        OfflineLocalPlayerData = data;

        string colorStr = ColorUtility.ToHtmlStringRGB(userColor);
        PlayerPrefs.SetString(UserColorPlayerPrefsKey, colorStr);
    }
}