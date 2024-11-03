using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RelayJoinUI : MonoBehaviour
{
    [SerializeField]
    private RelayManager _relayManager;
    
    [SerializeField]
    private Button _joinButton;
    
    [SerializeField]
    private TMP_InputField _joinCodeInput;
    
    [SerializeField]
    private TMP_Text _statusText;
    
    private void Awake()
    {
        _joinButton.onClick.AddListener(OnJoinButtonClicked);
    }

    private void OnJoinButtonClicked()
    {
        _relayManager.JoinGameAsync(_joinCodeInput.text, gameObject.GetCancellationTokenOnDestroy()).Forget();
    }
}
