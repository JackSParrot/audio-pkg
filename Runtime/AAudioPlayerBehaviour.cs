using System.Collections;
using UnityEngine;

namespace JackSParrot.Services.Audio
{
    public abstract class AAudioPlayerBehaviour : MonoBehaviour
    {
        [Header("Play data")]
        [SerializeField]
        protected ClipId clipId;

        [SerializeField]
        protected bool playOnEnable = true;

        [SerializeField]
        protected bool stopOnDisable = false;
        
        protected AudioService audioService = null;

        public void Play()
        {
            if (audioService == null)
            {
                StopAllCoroutines();
                StartCoroutine(WaitForServiceCoroutine());
                return;
            }

            if (!clipId.IsValid())
            {
                Debug.LogWarning("Tried to play a sound clip not set");
                return;
            }

            DoPlay();
        }

        IEnumerator WaitForServiceCoroutine()
        {
            while (audioService == null)
            {
                yield return null;
                audioService = ServiceLocator.GetService<AudioService>();
            }

            Play();
        }

        public abstract void Stop();

        protected abstract void DoPlay();

        private void OnEnable()
        {
            if (playOnEnable)
            {
                Play();
            }
        }

        private void OnDisable()
        {
            if (stopOnDisable)
            {
                Stop();
            }
        }
    }
}