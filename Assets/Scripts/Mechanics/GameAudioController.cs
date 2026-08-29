using UnityEngine;

namespace Platformer.Mechanics
{
    public class GameAudioController : MonoBehaviour
    {
        public static GameAudioController Instance { get; private set; }

        public AudioClip musicClip;
        public AudioClip workerAssignClip;
        public AudioClip taskSuccessClip;
        public AudioClip taskFailClip;
        public AudioClip roundWinClip;
        public AudioClip roundLoseClip;
        public AudioClip randomEventClip;

        [Range(0f, 1f)]
        public float musicVolume = 0.15f;

        [Range(0f, 1f)]
        public float sfxVolume = 0.8f;

        AudioSource musicSource;
        AudioSource sfxSource;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            EnsureDefaultClips();
            EnsureSources();
            StartMusic();
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        void EnsureDefaultClips()
        {
#if UNITY_EDITOR
            musicClip ??= LoadClip("Assets/Audio/Music.wav");
            workerAssignClip ??= LoadClip("Assets/Audio/jump.wav");
            taskSuccessClip ??= LoadClip("Assets/Audio/Collectable.wav");
            taskFailClip ??= LoadClip("Assets/Audio/Hurt.wav");
            roundWinClip ??= LoadClip("Assets/Audio/Collectable.wav");
            roundLoseClip ??= LoadClip("Assets/Audio/Death.wav");
            randomEventClip ??= LoadClip("Assets/Audio/Hurt.wav");
#endif
        }

#if UNITY_EDITOR
        static AudioClip LoadClip(string path)
        {
            return UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }
#endif

        void EnsureSources()
        {
            var sources = GetComponents<AudioSource>();
            if (sources.Length > 0)
            {
                musicSource = sources[0];
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }
            else
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }

            sfxSource = sources.Length > 1 ? sources[1] : gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }

        public void StartMusic()
        {
            if (musicSource == null || musicClip == null)
                return;

            if (musicSource.clip != musicClip)
                musicSource.clip = musicClip;

            musicSource.volume = musicVolume;
            if (!musicSource.isPlaying)
                musicSource.Play();
        }

        public void PlayWorkerAssigned()
        {
            PlayOneShot(workerAssignClip);
        }

        public void PlayTaskSuccess()
        {
            PlayOneShot(taskSuccessClip);
        }

        public void PlayTaskFail()
        {
            PlayOneShot(taskFailClip);
        }

        public void PlayRoundWin()
        {
            PlayOneShot(roundWinClip);
        }

        public void PlayRoundLose()
        {
            PlayOneShot(roundLoseClip);
        }

        public void PlayRandomEvent()
        {
            PlayOneShot(randomEventClip);
        }

        void PlayOneShot(AudioClip clip)
        {
            if (sfxSource == null || clip == null)
                return;

            sfxSource.PlayOneShot(clip, sfxVolume);
        }
    }
}
