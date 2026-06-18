using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace SYSTEMESCAPE
{
    // Developer debug console. It ships HIDDEN in the build and only turns on with a
    // secret shortcut, so players never see it but you can debug a real build anytime.
    //
    //   SECRET ACTIVATION:  hold  Left Ctrl + Left Shift  and press  D
    //
    // Once active:
    //   `   — toggle the clickable menu
    //   F1  — Skip intro            F2  — Solve binary
    //   F3  — Key + backpack        F4  — All PC parts
    //   F5  — Toggle fast move      F6  — Mark logic solved
    //   F7  — Reveal cipher (log)   F8  — Trigger ending
    //   F9  — Open all doors        F10 — Give EVERYTHING
    //   F11 — Reload scene          F12 — Quit to Main Menu
    //
    // A FPS counter + audio status show while active.

    public class DebugTools : MonoBehaviour
    {
        [Tooltip("If false, the debug console can never be activated (fully off).")]
        [SerializeField] private bool allowDebug = true;

        [Tooltip("Item names handed out by F4 / F10. MUST match the names in your " +
                 "PCBuildStation parts list exactly (CPU, GPU, RAM, PSU, MB, HDD).")]
        [SerializeField] private string[] pcParts = { "CPU", "GPU", "RAM", "PSU", "MB", "HDD" };

        private bool  _active   = false;
        private bool  _menuOpen = false;
        private bool  _fast     = false;
        private float _fps;

        private void Update()
        {
            if (!allowDebug) return;
            var kb = Keyboard.current;
            if (kb == null) return;

            // Secret activation: Ctrl + Shift + D
            bool ctrl  = kb.leftCtrlKey.isPressed  || kb.rightCtrlKey.isPressed;
            bool shift = kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed;
            if (ctrl && shift && kb.dKey.wasPressedThisFrame)
            {
                _active = !_active;
                Debug.Log($"[DEBUG] Console {(_active ? "ENABLED" : "disabled")}.");
            }

            if (!_active) return;

            // Smoothed FPS
            _fps = Mathf.Lerp(_fps, 1f / Mathf.Max(0.0001f, Time.unscaledDeltaTime), 0.1f);

            if (kb.backquoteKey.wasPressedThisFrame) _menuOpen = !_menuOpen;

            if (kb.f1Key.wasPressedThisFrame)  SkipIntro();
            if (kb.f2Key.wasPressedThisFrame)  SolveBinary();
            if (kb.f3Key.wasPressedThisFrame)  GiveKeyAndBag();
            if (kb.f4Key.wasPressedThisFrame)  GiveParts();
            if (kb.f5Key.wasPressedThisFrame)  ToggleFastMove();
            if (kb.f6Key.wasPressedThisFrame)  MarkLogicSolved();
            if (kb.f7Key.wasPressedThisFrame)  RevealCipher();
            if (kb.f8Key.wasPressedThisFrame)  TriggerEnding();
            if (kb.f9Key.wasPressedThisFrame)  OpenAllDoors();
            if (kb.f10Key.wasPressedThisFrame) GiveEverything();
            if (kb.f11Key.wasPressedThisFrame) ReloadScene();
            if (kb.f12Key.wasPressedThisFrame) QuitToMenu();
        }

        // -- Actions -------------------------------------------------------------

        private void SkipIntro()
        {
            FPSController.Instance?.EnableMovement();
            var flow = ClassroomSceneFlow.Instance;
            if (flow != null) flow.StopAllCoroutines();
            flow?.HideDialogue(true);
            Debug.Log("[DEBUG] Intro skipped.");
        }

        private void SolveBinary()
        {
            BinaryPuzzleManager.Instance?.DebugForceSolve();
            Debug.Log("[DEBUG] Binary force-solved.");
        }

        private void GiveKeyAndBag()
        {
            if (GameManager.Instance != null) GameManager.Instance.HasBackpack = true;
            InventorySystem.Instance?.UnlockInventory();
            InventorySystem.Instance?.AddItem(new InventoryItem { itemName = "Key", icon = null });
            Debug.Log("[DEBUG] Gave Key + backpack.");
        }

        private void GiveParts()
        {
            InventorySystem.Instance?.UnlockInventory();
            foreach (var part in pcParts)
                InventorySystem.Instance?.AddItem(new InventoryItem { itemName = part, icon = null });
            Debug.Log("[DEBUG] Gave all PC parts.");
        }

        private void GiveEverything()
        {
            GiveKeyAndBag();
            GiveParts();
            Debug.Log("[DEBUG] Gave EVERYTHING.");
        }

        private void MarkLogicSolved()
        {
            AccessPanelManager.Instance?.MarkLogicSolved();
            Debug.Log("[DEBUG] Logic gate marked solved.");
        }

        private void RevealCipher()
        {
            Debug.Log("[DEBUG] Cipher answer = check PhoneCipherLock 'Target Code' in Inspector.");
        }

        private void TriggerEnding()
        {
            QuizManager.Instance?.StartEnding();
            Debug.Log("[DEBUG] Triggered ending / quiz.");
        }

        private void ToggleFastMove()
        {
            _fast = !_fast;
            Time.timeScale = _fast ? 2f : 1f;     // quick way to speed up testing
            Debug.Log($"[DEBUG] Fast mode: {_fast} (time x{(_fast ? 2 : 1)}).");
        }

        private void OpenAllDoors()
        {
            foreach (var door in FindObjectsByType<DoorController>(FindObjectsSortMode.None))
                door.Open();
            Debug.Log("[DEBUG] Opened all doors.");
        }

        private void ReloadScene()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void QuitToMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }

        // -- On-screen UI ---------------------------------------------------------

        private void OnGUI()
        {
            if (!allowDebug || !_active) return;

            // Scale the whole debug UI up on high-res / Retina screens so it's readable
            float scale = Mathf.Max(1f, Screen.height / 1080f) * 1.4f;
            Matrix4x4 prevMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));

            GUI.color = Color.yellow;
            GUI.Label(new Rect(10, 10, 800, 20),
                $"DEBUG ON  |  FPS {_fps:0}  |  press  `  for menu  (Ctrl+Shift+D to hide)");

            string sm = SoundManager.Instance != null ? "OK" : "NULL";
            string gm = GameManager.Instance != null ? "OK" : "NULL";
            GUI.color = SoundManager.Instance != null ? Color.green : Color.red;
            GUI.Label(new Rect(10, 28, 800, 20),
                $"Sound={sm}  Game={gm}  Vol={AudioListener.volume:0.00}  Scene={SceneManager.GetActiveScene().name}");

            if (!_menuOpen) { GUI.matrix = prevMatrix; return; }

            GUI.color = Color.white;
            GUILayout.BeginArea(new Rect(10, 50, 240, 430), GUI.skin.box);
            GUILayout.Label("— DEBUG MENU —");
            if (GUILayout.Button("Skip Intro (F1)"))        SkipIntro();
            if (GUILayout.Button("Solve Binary (F2)"))      SolveBinary();
            if (GUILayout.Button("Key + Bag (F3)"))         GiveKeyAndBag();
            if (GUILayout.Button("PC Parts (F4)"))          GiveParts();
            if (GUILayout.Button("Fast Mode (F5)"))         ToggleFastMove();
            if (GUILayout.Button("Logic Solved (F6)"))      MarkLogicSolved();
            if (GUILayout.Button("Reveal Cipher (F7)"))     RevealCipher();
            if (GUILayout.Button("Trigger Ending (F8)"))    TriggerEnding();
            if (GUILayout.Button("Open Doors (F9)"))        OpenAllDoors();
            if (GUILayout.Button("Give EVERYTHING (F10)"))  GiveEverything();
            if (GUILayout.Button("Reload Scene (F11)"))     ReloadScene();
            if (GUILayout.Button("Quit to Menu (F12)"))     QuitToMenu();
            GUILayout.EndArea();

            GUI.matrix = prevMatrix;   // restore so game UI isn't affected
        }
    }
}
