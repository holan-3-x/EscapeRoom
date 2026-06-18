using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using DG.Tweening;

namespace SYSTEMESCAPE
{
    // The empty PC case the player builds. Press E to install the next part.
    // The player must have collected the parts (PickupItem) into the backpack first.
    //
    // For each required part you set:
    //   - the item name (must match the PickupItem's Item Name, e.g. "CPU")
    //   - the part mesh INSIDE the case (disabled at start) that appears when installed
    //
    // When all parts are installed, OnPCComplete fires (wire it to open a door,
    // start the network minigame, ARIA dialogue, etc.).

    [System.Serializable]
    public class PCPart
    {
        [Tooltip("Must match the PickupItem's Item Name exactly.")]
        public string itemName = "CPU";
        [Tooltip("The mesh inside the case, disabled at start, shown when installed.")]
        public GameObject installedMesh;
        [Tooltip("What ARIA says when this part goes in.")]
        [TextArea] public string installLine = "CPU seated. That's the brain of the machine.";
    }

    public class PCBuildStation : MonoBehaviour, IInteractable
    {
        [Header("Parts to install (in order)")]
        [SerializeField] private List<PCPart> parts = new();

        [Header("Optional progress screen (3D or UI TMP)")]
        [SerializeField] private TMP_Text progressText;

        [Header("Sound")]
        [SerializeField] private AudioSource installSound;

        [Header("Discovery — ARIA's first line about the PC")]
        [TextArea]
        [SerializeField] private string introLine =
            "This is the lab's server PC — and it's been stripped for parts. " +
            "If we want into the network, we have to rebuild it. Find the components around the lab.";

        [Header("Cables — revealed once all parts are in (animate)")]
        [SerializeField] private GameObject[] cablesToReveal;

        [Header("Fires when assembly is done (BEFORE power-on)")]
        public UnityEvent OnAssemblyComplete;
        [Header("Fires when the power button boots the PC")]
        public UnityEvent OnPCPoweredOn;

        public bool ReadyToPowerOn { get; private set; }
        public bool PoweredOn { get; private set; }

        private int _installed = 0;
        private bool _complete = false;
        private bool _introShown = false;

        private void Start()
        {
            // Hide all part meshes + cables at the start
            foreach (var p in parts)
                if (p.installedMesh != null) p.installedMesh.SetActive(false);
            if (cablesToReveal != null)
                foreach (var c in cablesToReveal)
                    if (c != null) c.SetActive(false);
            UpdateScreen();
        }

        public string GetPrompt()
        {
            if (_complete) return "";
            if (_installed >= parts.Count) return "";
            return $"[E] Install {parts[_installed].itemName}";
        }

        public void Interact()
        {
            if (_complete || _installed >= parts.Count) return;

            // First time: ARIA explains what this PC is and why we build it
            if (!_introShown)
            {
                _introShown = true;
                if (!string.IsNullOrEmpty(introLine))
                {
                    ClassroomSceneFlow.Instance?.ShowDialogue("ARIA", introLine,
                        autoClose: true, autoCloseDelay: 5f);
                    return;   // let them read the intro before installing
                }
            }

            PCPart next = parts[_installed];

            // Does the player have this part in the backpack?
            if (InventorySystem.Instance == null || !InventorySystem.Instance.HasItem(next.itemName))
            {
                ClassroomSceneFlow.Instance?.ShowDialogue("ARIA",
                    $"You need the {next.itemName} for this slot. Look around the lab and the " +
                    $"hallway — check the desks, drawers, and shelves.",
                    autoClose: true, autoCloseDelay: 4f);
                return;
            }

            // Install it: consume from inventory, show the mesh
            InventorySystem.Instance.RemoveItem(next.itemName);
            if (next.installedMesh != null)
            {
                next.installedMesh.SetActive(true);
                next.installedMesh.transform.DOPunchScale(Vector3.one * 0.12f, 0.3f);
            }
            if (installSound != null) installSound.Play();
            SoundManager.Instance?.PlayPickup();

            if (!string.IsNullOrEmpty(next.installLine))
                ClassroomSceneFlow.Instance?.ShowDialogue("ARIA", next.installLine,
                    autoClose: true, autoCloseDelay: 3.5f);

            _installed++;
            UpdateScreen();

            if (_installed >= parts.Count)
                CompleteBuild();
        }

        private void CompleteBuild()
        {
            _complete = true;
            ReadyToPowerOn = true;
            SoundManager.Instance?.PlaySolve();

            // Reveal the internal cables with a little pop animation
            if (cablesToReveal != null)
            {
                foreach (var c in cablesToReveal)
                {
                    if (c == null) continue;
                    c.SetActive(true);
                    c.transform.localScale = Vector3.one * 0.8f;
                    c.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack);
                }
            }

            ClassroomSceneFlow.Instance?.ShowDialogue("ARIA",
                "All parts in, cables connected. The build's complete!\n" +
                "Now hit the power button on the case to boot it up.",
                autoClose: true, autoCloseDelay: 5f);

            OnAssemblyComplete?.Invoke();
        }

        // Called by the PowerButtonInteract once assembly is done
        public void PowerOn()
        {
            if (!ReadyToPowerOn || PoweredOn) return;
            PoweredOn = true;
            SoundManager.Instance?.PlayBoot();
            ClassroomSceneFlow.Instance?.ShowDialogue("ARIA",
                "There it goes — fans spinning, lights on. The server's alive again. " +
                "Now we can work on the network connections.",
                autoClose: true, autoCloseDelay: 5f);
            OnPCPoweredOn?.Invoke();
        }

        private void UpdateScreen()
        {
            if (progressText == null) return;
            var sb = new System.Text.StringBuilder("PC BUILD\n\n");
            for (int i = 0; i < parts.Count; i++)
            {
                bool done = i < _installed;
                string mark = done ? "<color=#3fd>[OK]</color>" : "[  ]";
                sb.AppendLine($"{mark} {parts[i].itemName}");
            }
            progressText.text = sb.ToString();
        }
    }
}
