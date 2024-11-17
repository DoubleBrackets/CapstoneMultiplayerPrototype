using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ColorPicker : MonoBehaviour
{
    private static readonly int Hue = Shader.PropertyToID("_Hue");
    private static readonly int SatValueCursor = Shader.PropertyToID("_SatValueCursor");

    [SerializeField]
    private Image _satValueImage;

    [SerializeField]
    private Image _hueBar;

    [SerializeField]
    private Image _currentColorImage;

    [SerializeField]
    private bool _gammaCorrectColor;

    private float _hue;
    private float _saturation;
    private float _value;

    private void Start()
    {
        Color c = OfflinePlayerDataManager.Instance.OfflineLocalPlayerData.UserColor;

        if (_gammaCorrectColor)
        {
            c = c.linear;
        }

        Debug.Log("Color: " + c);

        Color.RGBToHSV(c, out _hue, out _saturation, out _value);

        UpdateCurrentColor();
    }

    /// <summary>
    ///     Hooked up through inspector to event listener component
    /// </summary>
    /// <param name="eventData"></param>
    public void HandleSatValuePointerDrag(BaseEventData eventData)
    {
        var pointerEventData = (PointerEventData)eventData;
        Rect rect = _satValueImage.rectTransform.rect;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _satValueImage.rectTransform,
            pointerEventData.position,
            null,
            out localPoint);

        _saturation = Mathf.InverseLerp(rect.xMin, rect.xMax, localPoint.x);
        _value = Mathf.InverseLerp(rect.yMin, rect.yMax, localPoint.y);

        UpdateCurrentColor();
    }

    public void HandleHueBarPointerDrag(BaseEventData eventData)
    {
        var pointerEventData = (PointerEventData)eventData;
        Rect rect = _hueBar.rectTransform.rect;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _hueBar.rectTransform,
            pointerEventData.position,
            null,
            out localPoint);

        _hue = Mathf.InverseLerp(rect.xMin, rect.xMax, localPoint.x);

        UpdateCurrentColor();
    }

    private void UpdateCurrentColor()
    {
        _currentColorImage.color = Color.HSVToRGB(_hue, _saturation, _value);

        _satValueImage.materialForRendering.SetFloat(Hue, _hue);
        _hueBar.materialForRendering.SetFloat(Hue, _hue);
        _satValueImage.materialForRendering.SetVector(SatValueCursor, new Vector2(_saturation, _value));

        // Gamma correct the color
        if (_gammaCorrectColor)
        {
            _currentColorImage.color = _currentColorImage.color.gamma;
        }

        OfflinePlayerDataManager.Instance.SetLocalPlayerColor(_currentColorImage.color);
    }
}