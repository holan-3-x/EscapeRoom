using UnityEngine;
using DG.Tweening;

namespace SYSTEMESCAPE
{
    // Singleton sound manager.
    // Attach to the GameManager object (it persists between scenes).
    // Assign audio clips in Inspector then call SoundManager.Instance.Play(...)

    public class SoundManager : MonoBehaviour
    {
        private static SoundManager _instance;

        // Bulletproof access: find the live SoundManager; if none exists in the scene
        // (e.g. it didn't survive the scene load in a build), instantiate one from a
        // Resources prefab named "AudioManager" so audio ALWAYS works.
        public static SoundManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<SoundManager>();
                    if (_instance == null)
                    {
                        var prefab = Resources.Load<SoundManager>("AudioManager");
                        if (prefab != null)
                        {
                            _instance = Instantiate(prefab);
                            _instance.name = "AudioManager (auto)";
                        }
                    }
                }
                return _instance;
            }
            private set { _instance = value; }
        }

        [Header("UI Sounds")]
        [SerializeField] public AudioClip buttonClick;
        [SerializeField] public AudioClip panelOpen;
        [SerializeField] public AudioClip panelClose;

        [Header("Gameplay Sounds")]
        [SerializeField] public AudioClip switchFlip;       // generic switch toggle (fallback)
        [SerializeField] public AudioClip switchOn;         // flipping a bit to 1 (up)
        [SerializeField] public AudioClip switchOff;        // flipping a bit to 0 (down)
        [SerializeField] public AudioClip confirmPress;     // the CONFIRM button
        [SerializeField] public AudioClip puzzleSolve;      // correct answer
        [SerializeField] public AudioClip puzzleWrong;      // wrong answer
        [SerializeField] public AudioClip doorOpen;         // door unlocking
        [SerializeField] public AudioClip itemPickup;       // picking up any item
        [SerializeField] public AudioClip computerBoot;     // CRT / ARIA startup
        [SerializeField] public AudioClip dialogueBlip;     // soft blip per character of dialogue

        [Header("Ambient")]
        [SerializeField] public AudioClip ambientClassroom; // low hum, AC noise

        [Header("Background Music")]
        [SerializeField] public AudioClip defaultMusic;     // plays on start if no scene track set
        [Range(0f, 1f)]
        [SerializeField] private float musicVolume = 0.35f;

        private AudioSource _sfxSource;
        private AudioSource _ambientSource;
        private AudioSource _musicSource;

        private void Awake()
        {
            // If a DIFFERENT live instance already exists, this one is a duplicate.
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);   // survive scene loads on its own

            // Three audio sources: SFX (one-shot), ambient (loop), music (loop)
            _sfxSource     = gameObject.AddComponent<AudioSource>();
            _ambientSource = gameObject.AddComponent<AudioSource>();
            _musicSource   = gameObject.AddComponent<AudioSource>();

            _ambientSource.loop        = true;
            _ambientSource.volume      = 0.15f;
            _ambientSource.spatialBlend = 0f;

            _musicSource.loop          = true;
            _musicSource.volume        = musicVolume;
            _musicSource.spatialBlend  = 0f;
        }

        private void Start()
        {
            if (ambientClassroom != null)
            {
                _ambientSource.clip = ambientClassroom;
                _ambientSource.Play();
            }

            if (defaultMusic != null) PlayMusic(defaultMusic);

            // Apply saved volume — but never leave it muted by a stale 0 saved value
            float vol = GameManager.Instance != null ? GameManager.Instance.MasterVolume : 1f;
            if (vol <= 0.001f) vol = 1f;   // a saved 0 would silence everything
            AudioListener.volume = vol;

            // Diagnostic (shows in the Mac build's Player.log) — confirms the manager is alive
            int clips = 0;
            foreach (var c in new[] { switchOn, switchOff, confirmPress, puzzleSolve, puzzleWrong,
                                      doorOpen, itemPickup, computerBoot })
                if (c != null) clips++;
            Debug.Log($"[SoundManager] Alive. AudioListener.volume={AudioListener.volume}. " +
                      $"Assigned SFX clips={clips}/8.");
        }

        // Switch background music with a quick crossfade. Call from a per-scene script.
        public void PlayMusic(AudioClip track)
        {
            if (track == null || _musicSource == null) return;
            if (_musicSource.clip == track && _musicSource.isPlaying) return;

            // Quick fade: lower, swap, raise
            DOTween.To(() => _musicSource.volume, v => _musicSource.volume = v, 0f, 0.4f)
                .OnComplete(() =>
                {
                    _musicSource.clip = track;
                    _musicSource.Play();
                    DOTween.To(() => _musicSource.volume, v => _musicSource.volume = v,
                        musicVolume, 0.6f);
                });
        }

        public void StopMusic() => _musicSource?.Stop();

        // Stops BOTH music and ambient (e.g. the moment the dream ends)
        public void StopBackground()
        {
            if (_musicSource != null)   _musicSource.DOFade(0f, 0.8f).OnComplete(() => _musicSource.Stop());
            if (_ambientSource != null) _ambientSource.DOFade(0f, 0.8f).OnComplete(() => _ambientSource.Stop());
        }

        public void Play(AudioClip clip, float volume = 1f)
        {
            if (clip == null || _sfxSource == null) return;
            _sfxSource.PlayOneShot(clip, volume);
        }

        // Convenience helpers so other scripts can call one line
        public void PlayClick()       => Play(buttonClick);
        public void PlaySwitchFlip()  => Play(switchFlip);
        public void PlaySolve()       => Play(puzzleSolve);
        public void PlayWrong()       => Play(puzzleWrong);
        public void PlayDoor()        => Play(doorOpen);
        public void PlayPickup()      => Play(itemPickup);
        public void PlayBoot()        => Play(computerBoot);
        public void PlayConfirm()     => Play(confirmPress);

        // Bit switch with separate up (ON=1) and down (OFF=0) sounds.
        // Falls back to the generic switchFlip if a specific one isn't assigned.
        public void PlaySwitch(bool turningOn)
        {
            AudioClip clip = turningOn ? switchOn : switchOff;
            Play(clip != null ? clip : switchFlip);
        }
    }
}
