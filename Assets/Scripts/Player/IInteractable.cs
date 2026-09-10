using UnityEngine;

namespace EchoesOfNeon.Player
{
    /// <summary>Implemented by anything TacticalPlayerController's Interact
    /// raycast can hit (doors, terminals, pickups, etc.).</summary>
    public interface IInteractable
    {
        void Interact(GameObject instigator);
    }
}
