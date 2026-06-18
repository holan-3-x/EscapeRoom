using UnityEngine;

namespace SYSTEMESCAPE
{
    public class LevelStartSpawner : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject malePrefab;
        [SerializeField] private GameObject femalePrefab;

        [Header("References")]
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private CutsceneController cutsceneController;

        private void Start()
        {
            if (GameManager.Instance == null)
            {
                Debug.LogWarning("No GameManager found! Did you start from the Menu scene?");
                return;
            }

            // Determine which prefab to spawn based on selection
            Gender pickedGender = GameManager.Instance.SelectedGender;
            GameObject prefabToSpawn = (pickedGender == Gender.Male) ? malePrefab : femalePrefab;

            if (prefabToSpawn != null && spawnPoint != null)
            {
                // Instantiate the character
                GameObject player = Instantiate(prefabToSpawn, spawnPoint.position, spawnPoint.rotation);
                
                // Store in GameManager for other systems
                GameManager.Instance.PlayerCharacter = player;

                Debug.Log($"Level0 started. Spawned {pickedGender} character.");
                
                // Start the level cutscene if present
                if (cutsceneController != null)
                {
                    cutsceneController.PlayCutscene();
                }
            }
            else
            {
                Debug.LogError("Spawn failed: Prefabs or SpawnPoint missing on LevelStartSpawner.");
            }
        }
    }
}
