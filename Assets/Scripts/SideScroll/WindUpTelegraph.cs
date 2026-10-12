using UnityEngine;

namespace EchoesOfNeon.SideScroll
{
    /// <summary>
    /// Visual twin of an enemy's audio wind-up, so a deaf or hard-of-hearing player (or anyone with the sound off)
    /// can read an incoming attack. A marker appears above the enemy for the whole wind-up and grows toward the strike.
    /// Shape, not just colour, carries the meaning (colour-blind safe): a flat warning bar for attacks you can fend,
    /// a tall diamond for unblockable ones (jump or dodge). Warm amber/red so it never reads as the cyan ally colour.
    /// Watches SideScrollEnemy.State, so the enemy logic stays untouched; added automatically by SideScrollEnemy.
    /// </summary>
    [RequireComponent(typeof(SideScrollEnemy))]
    public class WindUpTelegraph : MonoBehaviour
    {
        public static readonly Color FendableColor = new Color(1f, 0.72f, 0.1f);    // amber
        public static readonly Color UnblockableColor = new Color(1f, 0.15f, 0.1f); // red
        public float height = 2.3f;

        private SideScrollEnemy _enemy;
        private Transform _marker;
        private float _elapsed;

        public bool IsShowing => _marker != null && _marker.gameObject.activeSelf;

        public static bool ShouldShow(EnemyState s) => s == EnemyState.WindUp;

        /// <summary>Marker size over the wind-up: starts readable (0.6) and reaches full size at the strike.</summary>
        public static float ScaleAt(float elapsed, float duration) =>
            Mathf.Lerp(0.6f, 1f, duration <= 0f ? 1f : Mathf.Clamp01(elapsed / duration));

        public static Vector3 ShapeFor(bool unblockable) =>
            unblockable ? new Vector3(0.35f, 0.7f, 0.35f) : new Vector3(0.8f, 0.22f, 0.22f);

        private void Awake()
        {
            _enemy = GetComponent<SideScrollEnemy>();
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "WindUpTelegraph";
            DestroyImmediate(go.GetComponent<Collider>());   // purely visual: must not block sonar, rays or movement
            go.transform.SetParent(transform, false);
            go.transform.localPosition = Vector3.up * height;
            if (_enemy.unblockable) go.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
            var r = go.GetComponent<Renderer>();
            var shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
            if (shader != null)
            {
                var m = new Material(shader);
                var c = _enemy.unblockable ? UnblockableColor : FendableColor;
                if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
                if (m.HasProperty("_Color")) m.SetColor("_Color", c);
                r.sharedMaterial = m;
            }
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _marker = go.transform;
            go.SetActive(false);
        }

        private void LateUpdate()
        {
            if (_marker == null || _enemy == null) return;
            bool show = ShouldShow(_enemy.State);
            if (show != _marker.gameObject.activeSelf) { _marker.gameObject.SetActive(show); _elapsed = 0f; }
            if (!show) return;
            _elapsed += Time.deltaTime;
            float duration = _enemy.windUpSeconds * EchoesOfNeon.Accessibility.DifficultyRules.CurrentEnemyTelegraph;
            _marker.localScale = ShapeFor(_enemy.unblockable) * ScaleAt(_elapsed, duration);
        }
    }
}
