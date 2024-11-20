using UnityEngine;

namespace DebugTools
{
    public interface IBizmo
    {
        public void DrawBizmo(Camera cam);
    }

    public interface IGuiBizmo
    {
        public void DrawBizmo(Camera cam);
    }

    public struct TextBizmo : IGuiBizmo
    {
        private readonly string _text;
        private readonly Vector3 _position;
        private readonly Color _color;

        public TextBizmo(Vector3 position, string text, Color color)
        {
            _text = text;
            _position = position;
            _color = color;
        }

        public void DrawBizmo(Camera cam)
        {
            // project position to screen space
            Vector3 screenPos = cam.WorldToScreenPoint(_position);
            GUI.color = _color;

            int height = cam.pixelHeight;

            var size = new Vector2(100, 100);

            var pos = new Rect(screenPos.x, -screenPos.y + height, size.x, size.y);

            GUI.Label(pos, _text, Bizmos.TextStyle);

            GUI.color = Color.white;
        }
    }

    public struct LineBizmo : IBizmo
    {
        private readonly Vector3 _start;
        private readonly Vector3 _end;
        private readonly Color _color;

        public LineBizmo(Vector3 start, Vector3 end, Color color)
        {
            _start = start;
            _end = end;
            _color = color;
        }

        public void DrawBizmo(Camera camera)
        {
            Gizmos.color = _color;
            Gizmos.DrawLine(_start, _end);
        }
    }

    public struct CircleBizmo : IBizmo
    {
        private readonly Vector3 _pos;
        private readonly float _radius;
        private readonly Color _color;

        public CircleBizmo(Vector3 pos, float radius, Color color)
        {
            _pos = pos;
            _radius = radius;
            _color = color;
        }

        public void DrawBizmo(Camera camera)
        {
            Gizmos.color = _color;
            Gizmos.DrawWireSphere(_pos, _radius);
        }
    }
}