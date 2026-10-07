namespace EchoesOfNeon.Core
{
    /// <summary>Single flag for "the pause menu is open". Systems that
    /// drive Time.timeScale themselves (Neural Dilate) check it so they
    /// don't un-freeze the game behind the menu.</summary>
    public static class PauseState
    {
        public static bool IsPaused { get; set; }

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnDomainReload() => IsPaused = false;
    }
}
