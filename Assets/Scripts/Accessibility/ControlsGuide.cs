using System.Collections.Generic;
using System.Text;

namespace EchoesOfNeon.Accessibility
{
    /// <summary>Builds the spoken "here are your controls" text from the
    /// player's CURRENT bindings, so it stays right after rebinding.</summary>
    public static class ControlsGuide
    {
        /// <summary>Action name (as InputManager knows it) and the words spoken for it.</summary>
        public static readonly (string action, string spoken)[] Entries =
        {
            ("Move:Up", "Move forward"), ("Move:Down", "Move back"), ("Move:Left", "Move left"), ("Move:Right", "Move right"),
            ("Jump", "Jump"), ("Interact", "Interact"), ("Fire", "Strike or fire"), ("Aim", "Aim"),
            ("Sprint", "Sprint"), ("Crouch", "Crouch"), ("Fend", "Fend off an attack"), ("Whistle", "Whistle for Cookie"),
            ("SonarPulse", "Sonar pulse"), ("NeuralDilate", "Neural Dilate slow motion"),
            ("EchoStep", "Echo step"), ("Special", "Special ability"), ("CycleSpecial", "Next special"), ("Howl", "Cookie's howl"),
            ("Journal", "Replay the latest clue"), ("Status", "Hear your status"), ("RepeatLast", "Repeat the last message"),
            ("Pause", "Pause"), ("Hint", "Hear these controls again"),
        };

        public static string Compose(System.Func<string, string> bindingFor)
        {
            var sb = new StringBuilder("Controls. ");
            foreach (var (action, spoken) in Entries)
            {
                var binding = bindingFor(action);
                if (string.IsNullOrEmpty(binding) || binding == "-") continue;
                sb.Append(spoken).Append(": ").Append(binding).Append(". ");
            }
            return sb.ToString().TrimEnd();
        }
    }
}
