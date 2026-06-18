using UnityEngine;

namespace SYSTEMESCAPE
{
    // Drop this on any object in a scene and assign a music track.
    // It tells the persistent SoundManager to play that track for this scene.
    // Different scenes can have different tracks (menu, classroom, lab, etc.).

    public class SceneMusic : MonoBehaviour
    {
        [SerializeField] private AudioClip musicForThisScene;

        private void Start()
        {
            if (musicForThisScene != null)
                SoundManager.Instance?.PlayMusic(musicForThisScene);
        }
    }
}
