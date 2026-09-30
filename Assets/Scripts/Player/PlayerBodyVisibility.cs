using UnityEngine;
using UnityEngine.Rendering;

namespace EchoesOfNeon.Player
{
    /// <summary>
    /// The game is first-person, so Marcus's own body must not draw in the player's camera (the camera sits
    /// inside his head). In first-person mode every renderer on the body is shadows-only: he still casts a
    /// shadow and is still there for the physics/animation, but you never see the inside of your own head.
    /// Switch to third-person for cutscenes, debugging, or screenshots.
    /// </summary>
    public class PlayerBodyVisibility : MonoBehaviour
    {
        public GameObject body;
        [SerializeField] private bool firstPerson = true;

        public bool FirstPerson => firstPerson;

        private void Start() => Apply();
        public void SetFirstPerson(bool value) { firstPerson = value; Apply(); }

        public void Apply()
        {
            if (body == null) return;
            var mode = firstPerson ? ShadowCastingMode.ShadowsOnly : ShadowCastingMode.On;
            foreach (var r in body.GetComponentsInChildren<Renderer>(true)) r.shadowCastingMode = mode;
        }
    }
}
