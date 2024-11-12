using TMPro;
using UnityEngine;

/// <summary>
///     Resizes text to fit TMP Text
/// </summary>
public class TextFitter : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _text;

    [SerializeField]
    private RectTransform _container;

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
        }
    }

    private void FitText()
    {
        float preferredHeight = _text.preferredHeight;
        _container.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferredHeight + 15);
    }
}