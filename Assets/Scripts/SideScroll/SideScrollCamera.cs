using UnityEngine;

namespace EchoesOfNeon.SideScroll
{
    /// <summary>Plain side-on follow camera (and the AudioListener's home). Because the listener sits on the
    /// plane looking down +Z, Unity's stereo panning maps world X directly to left/right in the headphones.</summary>
    public class SideScrollCamera : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset = new Vector3(0f, 2.5f, -14f);
        public float smoothing = 6f;

        private void LateUpdate()
        {
            if (target == null) return;
            var want = new Vector3(target.position.x, target.position.y, 0f) + offset;
            transform.position = Vector3.Lerp(transform.position, want, 1f - Mathf.Exp(-smoothing * Time.deltaTime));
            transform.rotation = Quaternion.identity;
        }
    }
}
