using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Scripts.Biodiversity
{
    public class SpeciesAudio : MonoBehaviour
    {
        private Coroutine _periodicSoundCoroutine;
        private AudioSource _audioSource;
        private List<AudioClip> _periodicSounds;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        public void StartPeriodicSounds(List<AudioClip> sounds)
        {
            _periodicSounds = sounds;
            if (_periodicSoundCoroutine == null)
            {
                _periodicSoundCoroutine = StartCoroutine(PlayPeriodicSounds());
            }
        }

        public void StopPeriodicSounds()
        {
            if (_periodicSoundCoroutine != null)
            {
                StopCoroutine(_periodicSoundCoroutine);
                _periodicSoundCoroutine = null;
                _audioSource.Stop();
            }
        }

        private IEnumerator PlayPeriodicSounds()
        {
            while (true)
            {
                yield return new WaitForSeconds(15.0f); // Wait for 15 seconds interval

                if (_audioSource != null && _periodicSounds.Count > 0)
                {
                    AudioClip randomClip = _periodicSounds[Random.Range(0, _periodicSounds.Count)];
                    StartCoroutine(PlayPeriodicSoundWithFade(_audioSource, randomClip, 0.25f, 1.5f, 7.0f));
                }
            }
        }

        private IEnumerator PlayPeriodicSoundWithFade(AudioSource audioSource, AudioClip clip, float targetVolume,
            float fadeDuration, float playDuration)
        {
            audioSource.clip = clip;
            audioSource.volume = 0f; // Start with volume at 0
            audioSource.Play();

            float fadeOutStartTime = playDuration - fadeDuration;
            float elapsedTime = 0f;

            while (elapsedTime < playDuration)
            {
                elapsedTime += Time.deltaTime;

                if (elapsedTime < fadeDuration)
                {
                    audioSource.volume = Mathf.Clamp01(elapsedTime / fadeDuration) * targetVolume;
                }
                else if (elapsedTime > fadeOutStartTime)
                {
                    audioSource.volume = Mathf.Clamp01(1 - ((elapsedTime - fadeOutStartTime) / fadeDuration)) *
                                         targetVolume;
                }

                yield return null;
            }

            audioSource.Stop();
            audioSource.volume = 0f; // Ensure the volume is set back to 0 after stopping
        }
    }
}
