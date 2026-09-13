using System.Collections;
using UnityEngine;

namespace EchoesOfNeon.Core
{
    /// <summary>Temporary verification aid for the test scene - fires its
    /// AcousticEmitter on a fixed interval so the compass/sonar have
    /// something to react to without needing a real weapon or AI system yet
    /// (Phase 6). Safe to delete once those exist.</summary>
    [RequireComponent(typeof(AcousticEmitter))]
    public class TestEmitterLoop : MonoBehaviour
    {
        [SerializeField] private float interval = 3f;

        private IEnumerator Start()
        {
            var emitter = GetComponent<AcousticEmitter>();
            var wait = new WaitForSeconds(interval);
            while (true)
            {
                yield return wait;
                emitter.Emit();
            }
        }
    }
}
