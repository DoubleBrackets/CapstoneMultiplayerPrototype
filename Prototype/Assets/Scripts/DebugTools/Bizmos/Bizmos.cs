using System.Collections.Generic;
using DebugTools;
using UnityEngine;

public class Bizmos : MonoBehaviour
{
    private struct BizmoInstance
    {
        public IBizmo Bizmo;
        public float Duration;
        public float CreateTime;

        public bool IsExpired => Time.time - CreateTime > Duration;
    }

    public struct GuiBizmoInstance
    {
        public IGuiBizmo Bizmo;
        public float Duration;
        public float CreateTime;

        public bool IsExpired => Time.time - CreateTime > Duration;
    }

    public static GUIStyle TextStyle;

    [SerializeField]
    private int _textSize;

    public static Bizmos Instance { get; private set; }

    private readonly List<BizmoInstance> _bizmos = new();
    private readonly List<GuiBizmoInstance> _guiBizmos = new();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        TextStyle = new GUIStyle
        {
            fontSize = _textSize,
            normal = { textColor = Color.white }
        };

        Instance = this;
    }

    private void OnGUI()
    {
        for (int i = _guiBizmos.Count - 1; i >= 0; i--)
        {
            if (_guiBizmos[i].IsExpired)
            {
                _guiBizmos.RemoveAt(i);
                continue;
            }

            _guiBizmos[i].Bizmo.DrawBizmo(Camera.main);
        }
    }

    private void OnDrawGizmos()
    {
        Camera cam = Camera.main;
        for (int i = _bizmos.Count - 1; i >= 0; i--)
        {
            if (_bizmos[i].IsExpired)
            {
                _bizmos.RemoveAt(i);
                continue;
            }

            _bizmos[i].Bizmo.DrawBizmo(cam);
        }
    }

    public void AddBizmo(IBizmo bizmo, float duration)
    {
        _bizmos.Add(new BizmoInstance
        {
            Bizmo = bizmo,
            Duration = duration,
            CreateTime = Time.time
        });
    }

    public void AddGuiBizmo(IGuiBizmo bizmo, float duration)
    {
        _guiBizmos.Add(new GuiBizmoInstance
        {
            Bizmo = bizmo,
            Duration = duration,
            CreateTime = Time.time
        });
    }
}