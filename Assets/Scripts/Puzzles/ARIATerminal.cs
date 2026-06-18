using UnityEngine;

namespace SYSTEMESCAPE
{
    // The portable ARIA computer. Drives the binary lesson as a gated sequence:
    //   0) Meet ARIA — the door is locked
    //   1) What is binary — go try the light switch
    //   *) [player flips the light switch] -> lesson advances
    //   2) You just did 1 bit — here's how to combine bits (example 1)
    //   3) Example 2 (the decimal -> binary method)
    //   4) Here's the door code — puzzle UNLOCKS
    //   5+) Reminder of the code
    // The ARIA teaching brain. NOT an IInteractable — the PortableComputer drives it.
    // Lives on the same potato object; PortableComputer calls AdvanceLesson().
    public class ARIATerminal : MonoBehaviour
    {
        public static ARIATerminal Instance { get; private set; }
        private void Awake() { if (Instance == null) Instance = this; else Destroy(gameObject); }

        private int  _step = 0;
        private bool _lightFlipped = false;

        // Called by PortableComputer each time the player presses E to "Talk to ARIA".
        public void AdvanceLesson()
        {
            switch (_step)
            {
                case 0: StepMeetARIA();      _step = 1; break;
                case 1: StepWhatIsBinary();  break;   // stays on 1 until the light is flipped
                case 2: StepExample1();      _step = 3; break;
                case 3: StepExample2();      _step = 4; break;
                case 4: StepGiveChallenge(); _step = 5; break;
                default: StepReminder();     break;
            }
        }

        // -- Steps -------------------------------------------------------------

        private void StepMeetARIA()
        {
            ClassroomSceneFlow.Instance?.ShowDialogue("ARIA",
                "Booting... there we go. Hello! You slept right through the whole lesson, " +
                "didn't you?\n\nHere's the situation: the classroom door locked itself when " +
                "the building went into night mode. To open it, you'll need to enter a code " +
                "in binary — the computer's own language. Don't worry, I'll teach you. " +
                "Talk to me again when you're ready.",
                autoClose: false);
        }

        private void StepWhatIsBinary()
        {
            if (_lightFlipped) { StepExample1(); _step = 3; return; }

            ClassroomSceneFlow.Instance?.ShowDialogue("ARIA",
                "Computers only understand two states: ON and OFF. We write them as 1 and 0. " +
                "That's all binary is.\n\n" +
                "See the light switch on the wall? Go flip it a few times. " +
                "Watch what happens — that switch is a single 'bit'. Come back after you try it.",
                autoClose: false);
        }

        private void StepExample1()
        {
            ClassroomSceneFlow.Instance?.ShowDialogue("ARIA",
                "Perfect — you saw it. Light ON = 1, light OFF = 0. One switch is one bit.\n\n" +
                "Line up 8 bits and you can make any number from 0 to 255. Each switch is " +
                "worth double the one to its right: 128, 64, 32, 16, 8, 4, 2, 1.\n\n" +
                "Example: to make 5, turn on the 4 and the 1 (4 + 1 = 5). " +
                "So 5 is 0000 0101. Talk to me for one more.",
                autoClose: false);
        }

        private void StepExample2()
        {
            ClassroomSceneFlow.Instance?.ShowDialogue("ARIA",
                "One more — let's make 13.\n\n" +
                "Biggest value that fits 13 is 8 (13 - 8 = 5). Then 4 fits 5 (5 - 4 = 1). " +
                "Then 1 fits 1 (1 - 1 = 0).\n\n" +
                "So 13 = 8 + 4 + 1, which is 0000 1101. Subtract the biggest power of two, " +
                "repeat until zero. Ready? Talk to me for the real door code.",
                autoClose: false);
        }

        private void StepGiveChallenge()
        {
            if (BinaryPuzzleManager.Instance == null) return;

            int target = BinaryPuzzleManager.Instance.TargetValue;
            string binary = System.Convert.ToString(target, 2).PadLeft(8, '0');
            string binarySpaced = binary.Substring(0, 4) + " " + binary.Substring(4, 4);

            ClassroomSceneFlow.Instance?.ShowDialogue("ARIA",
                $"Here it is. The door code is  <b>{target}</b>.\n\n" +
                $"Work it out yourself if you want the practice — or here's the answer:\n" +
                $"  <b>{target}  =  {binarySpaced}</b>\n\n" +
                $"Go to the 8 switches on the wall. The leftmost is worth 128, the rightmost is 1. " +
                $"Set each one to 1 (green) or 0 (red) to match the code, then press CONFIRM. " +
                $"The panel at the bottom of your screen tracks your total as you go.",
                autoClose: false);

            // Open the 8 switches + confirm for input, show the tracker panel
            BinaryPuzzleManager.Instance.UnlockPuzzle();
        }

        private void StepReminder()
        {
            if (BinaryPuzzleManager.Instance == null) return;
            int target = BinaryPuzzleManager.Instance.TargetValue;
            string binary = System.Convert.ToString(target, 2).PadLeft(8, '0');
            string binarySpaced = binary.Substring(0, 4) + " " + binary.Substring(4, 4);

            ClassroomSceneFlow.Instance?.ShowDialogue("ARIA",
                $"The code is  <b>{target}</b>  =  <b>{binarySpaced}</b>.\n" +
                $"Leftmost switch = 128, rightmost = 1. Green is 1, red is 0. " +
                $"Match the code and press CONFIRM.",
                autoClose: false);
        }

        // Called by LightSwitch every time the player flips the room light.
        public void OnTeachingLightFlipped()
        {
            _lightFlipped = true;

            // If ARIA was waiting on the light lesson, advance her to the examples.
            if (_step == 1)
            {
                _step = 2;
                ClassroomSceneFlow.Instance?.ShowDialogue("ARIA",
                    "There it is — ON is 1, OFF is 0. You just read your first bit! " +
                    "Come back and talk to me, and I'll show you how to combine eight of them.",
                    autoClose: true, autoCloseDelay: 4.5f);
            }
        }
    }
}
