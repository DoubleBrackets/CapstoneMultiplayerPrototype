using UnityEngine;
using UnityEngine.Events;

namespace Util
{
    public class CollisionEvents : MonoBehaviour
    {
        public UnityEvent<Collision2D> OnCollisionEnterEvent;
        public UnityEvent<Collision2D> OnCollisionStayEvent;
        public UnityEvent<Collider2D> OnTriggerEnterEvent;

        private void OnCollisionEnter2D(Collision2D other)
        {
            OnCollisionEnterEvent?.Invoke(other);
        }

        private void OnCollisionStay2D(Collision2D other)
        {
            OnCollisionStayEvent?.Invoke(other);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            OnTriggerEnterEvent?.Invoke(other);
        }
    }
}