using UnityEngine;

namespace SYSTEMESCAPE
{
    public enum Gender { Male, Female }

    public class GameManager : MonoBehaviour
    {
        private static GameManager _instance;

        // Auto-creates a GameManager if none exists in the scene.
        // This means the game works even if you press Play directly in Level0
        // instead of starting from the MainMenu scene.
        public static GameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<GameManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("GameManager (auto)");
                        _instance = go.AddComponent<GameManager>();
                    }
                }
                return _instance;
            }
        }

        // Character
        public Gender     SelectedGender   { get; set; }
        public GameObject PlayerCharacter  { get; set; }

        // Progress flags (persisted by SaveSystem)
        public bool HasBackpack      { get; set; }
        public bool BinaryPuzzleDone { get; set; }

        // Clears all in-game progress. Called by New Game, because this object
        // survives scene loads and would otherwise keep flags from the last playthrough.
        public void ResetProgress()
        {
            HasBackpack      = false;
            BinaryPuzzleDone = false;
        }

        // Settings (persisted separately)
        public float MasterVolume   { get; set; } = 1f;
        public float MouseSensitivity { get; set; } = 0.12f;

        private void Awake()
        {
            // If a DIFFERENT instance already exists, this one is a duplicate — destroy it.
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            // Otherwise this is the surviving instance. ALWAYS mark it DontDestroyOnLoad.
            // (Critical: the Instance getter may have already set _instance = this before
            //  Awake ran. The old code then skipped DontDestroyOnLoad, so the manager —
            //  and its SoundManager — got destroyed on scene load. That's fixed here.)
            _instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings();
        }

        public void LoadSettings()
        {
            MasterVolume    = PlayerPrefs.GetFloat("vol", 1f);
            MouseSensitivity = PlayerPrefs.GetFloat("sens", 0.12f);
        }

        public void SaveSettings()
        {
            PlayerPrefs.SetFloat("vol",  MasterVolume);
            PlayerPrefs.SetFloat("sens", MouseSensitivity);
            PlayerPrefs.Save();
        }
    }
}
