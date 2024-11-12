using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
///     Handles displaying debug logs in-game
/// </summary>
public class InGameLogUI : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _text;

    [SerializeField]
    private RectTransform _container;

    [SerializeField]
    private ScrollRect _scrollRect;

    private string _lastTextContent;

    private void Awake()
    {
        _lastTextContent = _text.text;
    }

    private void Update()
    {
        if (_lastTextContent != _text.text)
        {
            _lastTextContent = _text.text;
            FitText();
            // Scroll to bottom
            _scrollRect.normalizedPosition = new Vector2(0, 0);
        }
    }

    private void FitText()
    {
        float preferredHeight = _text.preferredHeight;
        _container.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferredHeight + 15);
    }
}