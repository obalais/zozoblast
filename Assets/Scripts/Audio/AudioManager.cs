using UnityEngine;
#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace Blockblast.Audio
{
    public class AudioManager : MonoBehaviour
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void ResumeUnityAudioContext();
#endif

        private AudioSource audioSource;
        private AudioClip placeClip;
        private AudioClip clearClip;
        private AudioClip invalidClip;
        private AudioClip gameOverClip;

        private void Awake()
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            audioSource.volume = 1f;
            audioSource.bypassListenerEffects = true;
            audioSource.bypassEffects = true;
            audioSource.bypassReverbZones = true;

            placeClip = Resources.Load<AudioClip>("Audio/place");
            clearClip = Resources.Load<AudioClip>("Audio/clear");
            invalidClip = Resources.Load<AudioClip>("Audio/invalid");
            gameOverClip = Resources.Load<AudioClip>("Audio/gameover");
        }

        public void PlayPlace() => PlayClip(placeClip);
        public void PlayClear() => PlayClip(clearClip);
        public void PlayInvalid() => PlayClip(invalidClip);
        public void PlayGameOver() => PlayClip(gameOverClip);

        private void PlayClip(AudioClip clip)
        {
            if (clip == null || audioSource == null) return;
#if UNITY_WEBGL && !UNITY_EDITOR
            ResumeUnityAudioContext();
#endif
            audioSource.PlayOneShot(clip);
        }
    }
}
