using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public class UsernameTextfield : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField _inputField;

    [SerializeField]
    private TMP_Text _usernameStatusText;

    private CancellationTokenSource _fadeStatusTextTokenSource = new();

    private void Start()
    {
        _usernameStatusText.text = "";
        string prefName = OfflinePlayerDataManager.Instance.OfflineLocalPlayerData.Username;

        if (!string.IsNullOrEmpty(prefName))
        {
            _inputField.text = prefName;
        }

        _inputField.onSubmit.AddListener(HandleUsernameSubmit);
        _inputField.onEndEdit.AddListener(HandleUsernameSubmit);
    }

    private void OnDestroy()
    {
        _inputField.onSubmit.RemoveListener(HandleUsernameSubmit);
        _inputField.onEndEdit.RemoveListener(HandleUsernameSubmit);
    }

    private void HandleUsernameSubmit(string username)
    {
        string currentUsername = OfflinePlayerDataManager.Instance.OfflineLocalPlayerData.Username;
        if (string.IsNullOrWhiteSpace(username))
        {
            SetUsernameStatusText("Username cannot be empty");
            _inputField.text = currentUsername;
            return;
        }

        if (currentUsername == username)
        {
            return;
        }

        username = username.Trim();

        OfflinePlayerDataManager.Instance.SetLocalPlayerName(username);

        SetUsernameStatusText("Username updated");
    }

    private void SetUsernameStatusText(string message)
    {
        _fadeStatusTextTokenSource.Cancel();
        _fadeStatusTextTokenSource = new CancellationTokenSource();
        FadeStatusText(message, _fadeStatusTextTokenSource.Token).Forget();
    }

    private async UniTaskVoid FadeStatusText(string message, CancellationToken token)
    {
        var fadeTime = 2f;
        _usernameStatusText.text = message;
        Color c = _usernameStatusText.color;
        c.a = 1f;
        for (float i = fadeTime; i >= 0f; i -= Time.deltaTime)
        {
            c.a = i;
            _usernameStatusText.color = c;
            await UniTask.Yield();

            if (token.IsCancellationRequested)
            {
                break;
            }
        }
    }
}