using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EchoesOfNeon.UI
{
    /// <summary>Finds the text a screen reader should speak for a control:
    /// TextMeshPro first, then legacy Text, then the object's name.</summary>
    public static class AccessibleLabel
    {
        public static string Resolve(Component control)
        {
            var tmp = control.GetComponentInChildren<TMP_Text>();
            if (tmp != null && !string.IsNullOrWhiteSpace(tmp.text)) return tmp.text;
            var text = control.GetComponentInChildren<Text>();
            if (text != null && !string.IsNullOrWhiteSpace(text.text)) return text.text;
            return control.gameObject.name;
        }
    }
}
