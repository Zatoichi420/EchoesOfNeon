using EchoesOfNeon.Accessibility;
using UnityEngine;
using UnityEngine.Accessibility;

namespace EchoesOfNeon.UI
{
    /// <summary>Builds the sentence spoken when keyboard/gamepad focus lands
    /// on a control: "label, role, value, dimmed, hint".</summary>
    public static class AccessibleDescription
    {
        public static string Describe(GameObject go)
        {
            if (go == null) return null;
            var node = NodeOf(go);
            if (node == null) return null;
            return Compose(node.label, node.role, node.value, node.state, node.hint);
        }

        public static string Compose(string label, AccessibilityRole role, string value, AccessibilityState state, string hint)
        {
            var parts = new System.Collections.Generic.List<string>();
            if (!string.IsNullOrEmpty(label)) parts.Add(label);
            var roleWord = RoleWord(role);
            if (roleWord != null) parts.Add(roleWord);
            if (!string.IsNullOrEmpty(value)) parts.Add(value);
            if ((state & AccessibilityState.Disabled) != 0) parts.Add("dimmed");
            else if (role != AccessibilityRole.Toggle && (state & AccessibilityState.Selected) != 0) parts.Add("selected");
            if (!string.IsNullOrEmpty(hint)) parts.Add(hint);
            return string.Join(", ", parts);
        }

        private static string RoleWord(AccessibilityRole role)
        {
            switch (role)
            {
                case AccessibilityRole.Button: return "button";
                case AccessibilityRole.Toggle: return "toggle";
                case AccessibilityRole.Slider: return "slider";
                case AccessibilityRole.TabButton: return "tab";
                case AccessibilityRole.Header: return "heading";
                default: return null;
            }
        }

        private static AccessibilityNode NodeOf(GameObject go)
        {
            var b = go.GetComponent<AccessibleButton>(); if (b != null && b.Node != null) return b.Node;
            var t = go.GetComponent<AccessibleToggle>(); if (t != null && t.Node != null) return t.Node;
            var s = go.GetComponent<AccessibleSlider>(); if (s != null && s.Node != null) return s.Node;
            return null;
        }
    }
}
