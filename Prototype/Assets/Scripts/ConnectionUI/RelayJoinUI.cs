using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RelayJoinUI : MonoBehaviour
{
    [SerializeField]
    private Button _joinButton;

    [SerializeField]
    private TMP_InputField _joinCodeInput;

    [SerializeField]
    private TMP_Text _statusText;

    private RelayManager _relayManager;

    private void Awake()
    {
        _joinButton.onClick.AddListener(OnJoinButtonClicked);
    }

    private void Start()
    {
        _relayManager = UnityServiceFinder.RelayManager;
        _relayManager.OnJoinAllocation += OnJoinAllocation;
    }

    private void OnDestroy()
    {
        _relayManager.OnJoinAllocation -= OnJoinAllocation;
    }

    private void OnJoinAllocation(RelayManager.JoinAllocationEventData data)
    {
        _statusText.text = data.DidSucceed
            ? "Joined!"
            : $"Failed to join: {data.FailureReason}";
    }

    private void OnJoinButtonClicked()
    {
        _relayManager.JoinGameAsync(_joinCodeInput.text, gameObject.GetCancellationTokenOnDestroy()).Forget();
    }
}