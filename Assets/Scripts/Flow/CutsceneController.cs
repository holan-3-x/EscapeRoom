using UnityEngine;
using UnityEngine.Playables;

namespace SYSTEMESCAPE
{
    public class CutsceneController : MonoBehaviour
    {
        [SerializeField] private PlayableDirector timeline;

        public void PlayCutscene()
        {
            if (timeline != null)
            {
                timeline.Play();
                Debug.Log("Cutscene started.");
            }
            else
            {
                Debug.LogWarning("Cutscene timeline not assigned!");
            }
        }
    }
}
