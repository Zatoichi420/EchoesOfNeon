using System.Collections;
using EchoesOfNeon.Accessibility;
using EchoesOfNeon.Core;
using UnityEngine;
using UnityEngine.UI;

namespace EchoesOfNeon.UI
{
    /// <summary>
    /// Phase 5: visual complement to the Oculus Sensory Suite's audio-based
    /// sonar - a HUD compass ring that plots AcousticEventSystem events as
    /// directional blips, color-coded per MEMORY.md's design pillars
    /// (orange = gunfire/explosions, cyan = footsteps/movement, yellow =
    /// mechanical/alerts).
    ///
    /// This is the colorblind/low-vision-facing accessibility layer - the
    /// actual playable-without-sight cue is AcousticEventSystem's spatial
    /// audio, which fires independently of whether this compass exists.
    /// </summary>
    public class SoundVisualizerCompass : MonoBehaviour
    {
        [SerializeField] private Transform listenerTransform; // camera/head - bearing is relative to this
        [SerializeField] private float ringRadius = 120f;      // pixels from HUD center
        [SerializeField] private float blipLifetime = 1.5f;
        [SerializeField] private float blipSize = 18f;

        private Sprite _dotSprite;

        private void Awake()
        {
            _dotSprite = CreateDotSprite();
        }

        private void OnEnable()
        {
            if (AcousticEventSystem.Instance != null)
                AcousticEventSystem.Instance.OnAcousticEvent += HandleAcousticEvent;
        }

        private void OnDisable()
        {
            if (AcousticEventSystem.Instance != null)
                AcousticEventSystem.Instance.OnAcousticEvent -= HandleAcousticEvent;
        }

        private void HandleAcousticEvent(AcousticEvent evt)
        {
            if (listenerTransform == null) return;
            StartCoroutine(SpawnBlip(evt));
        }

        private IEnumerator SpawnBlip(AcousticEvent evt)
        {
            float bearing = SignedAngleToTarget(evt.Position);

            var blipGO = new GameObject("Blip", typeof(RectTransform), typeof(Image));
            blipGO.transform.SetParent(transform, false);
            var image = blipGO.GetComponent<Image>();
            image.sprite = _dotSprite;
            var settings = AccessibilityManager.Instance != null ? AccessibilityManager.Instance.Settings : null;
            var mode = settings != null ? settings.colorblindMode : ColorblindMode.None;
            bool highContrast = settings != null && settings.highContrastOutlines;
            image.color = CompassPalette.ColorFor(evt.Type, mode);

            // Size varies by sound type so the compass never relies on color
            // alone; high-contrast adds a bright outline and larger blips.
            float size = blipSize * CompassPalette.SizeScaleFor(evt.Type) * (highContrast ? 1.35f : 1f);
            if (highContrast)
            {
                var outline = blipGO.AddComponent<Outline>();
                outline.effectColor = Color.white;
                outline.effectDistance = new Vector2(3f, 3f);
            }

            var rt = (RectTransform)blipGO.transform;
            rt.sizeDelta = new Vector2(size, size);
            float rad = bearing * Mathf.Deg2Rad;
            rt.anchoredPosition = new Vector2(Mathf.Sin(rad), Mathf.Cos(rad)) * ringRadius;

            float elapsed = 0f;
            while (elapsed < blipLifetime)
            {
                elapsed += Time.unscaledDeltaTime;
                var c = image.color;
                c.a = 1f - (elapsed / blipLifetime);
                image.color = c;
                yield return null;
            }
            Destroy(blipGO);
        }

        private float SignedAngleToTarget(Vector3 worldPos)
        {
            Vector3 toTarget = worldPos - listenerTransform.position;
            toTarget.y = 0f;
            Vector3 forward = listenerTransform.forward;
            forward.y = 0f;
            return Vector3.SignedAngle(forward, toTarget, Vector3.up);
        }

        private static Sprite CreateDotSprite()
        {
            var texture = new Texture2D(8, 8);
            var pixels = new Color[8 * 8];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
            texture.SetPixels(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f));
        }
    }
}
