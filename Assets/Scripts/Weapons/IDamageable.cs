using UnityEngine;

namespace EchoesOfNeon.Weapons
{
    /// <summary>Decoupled seam for weapon hits, same pattern as
    /// ISonarPingable/IInteractable - no health/damage system exists yet,
    /// this is where a future one hooks in.</summary>
    public interface IDamageable
    {
        void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal);
    }
}
