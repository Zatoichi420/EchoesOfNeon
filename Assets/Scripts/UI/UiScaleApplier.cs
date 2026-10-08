using EchoesOfNeon.Accessibility;
using UnityEngine;
using UnityEngine.UI;

namespace EchoesOfNeon.UI
{
    /// <summary>Makes a ScaleWithScreenSize canvas follow the player's UI
    /// size setting by shrinking its reference resolution (smaller reference
    /// = bigger UI). Add next to a CanvasScaler.</summary>
    [RequireComponent(typeof(CanvasScaler))]
    public class UiScaleApplier : MonoBehaviour
    {
        private CanvasScaler _scaler;
        private Vector2 _baseResolution;
        private bool _subscribed;

        private void Awake()
        {
            _scaler = GetComponent<CanvasScaler>();
            _baseResolution = _scaler.referenceResolution;
        }

        private void OnEnable() { Subscribe(); Apply(); }
        private void Update() { if (!_subscribed) { Subscribe(); Apply(); } }

        private void OnDisable()
        {
            if (_subscribed && AccessibilityManager.Instance != null)
                AccessibilityManager.Instance.OnSettingsChanged -= HandleChanged;
            _subscribed = false;
        }

        private void Subscribe()
        {
            if (_subscribed || AccessibilityManager.Instance == null) return;
            AccessibilityManager.Instance.OnSettingsChanged += HandleChanged;
            _subscribed = true;
        }

        private void HandleChanged(AccessibilitySettings _) => Apply();

        private void Apply()
        {
            float scale = AccessibilityManager.Instance != null ? Mathf.Clamp(AccessibilityManager.Instance.Settings.uiScale, 1f, 1.5f) : 1f;
            _scaler.referenceResolution = _baseResolution / scale;
        }
    }
}
