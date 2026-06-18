using UnityEngine;

namespace SYSTEMESCAPE
{
    // Simple save system using PlayerPrefs.
    // Call SaveSystem.Save() any time the game state changes.
    // Call SaveSystem.Load() on scene start to restore state.

    public static class SaveSystem
    {
        private const string KEY_HAS_SAVE    = "hasSave";
        private const string KEY_SCENE       = "savedScene";
        private const string KEY_GENDER      = "savedGender";
        private const string KEY_LEVEL       = "savedLevel";
        private const string KEY_HAS_PACK    = "hasBackpack";
        private const string KEY_PUZZLE_DONE = "binaryDone";

        public static bool HasSave() => PlayerPrefs.GetInt(KEY_HAS_SAVE, 0) == 1;

        public static void Save()
        {
            if (GameManager.Instance == null) return;

            PlayerPrefs.SetInt(KEY_HAS_SAVE,    1);
            PlayerPrefs.SetString(KEY_SCENE,    UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            PlayerPrefs.SetInt(KEY_GENDER,      (int)GameManager.Instance.SelectedGender);
            PlayerPrefs.SetInt(KEY_HAS_PACK,    GameManager.Instance.HasBackpack ? 1 : 0);
            PlayerPrefs.SetInt(KEY_PUZZLE_DONE, GameManager.Instance.BinaryPuzzleDone ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static void Load()
        {
            if (!HasSave() || GameManager.Instance == null) return;

            GameManager.Instance.SelectedGender    = (Gender)PlayerPrefs.GetInt(KEY_GENDER, 0);
            GameManager.Instance.HasBackpack       = PlayerPrefs.GetInt(KEY_HAS_PACK, 0) == 1;
            GameManager.Instance.BinaryPuzzleDone  = PlayerPrefs.GetInt(KEY_PUZZLE_DONE, 0) == 1;
        }

        public static string GetSavedScene() => PlayerPrefs.GetString(KEY_SCENE, "MainMenu");

        public static void DeleteSave()
        {
            PlayerPrefs.DeleteKey(KEY_HAS_SAVE);
            PlayerPrefs.DeleteKey(KEY_SCENE);
            PlayerPrefs.DeleteKey(KEY_GENDER);
            PlayerPrefs.DeleteKey(KEY_HAS_PACK);
            PlayerPrefs.DeleteKey(KEY_PUZZLE_DONE);
            PlayerPrefs.Save();
        }
    }
}
