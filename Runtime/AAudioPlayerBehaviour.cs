using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
using JackSParrot.Audio.Editor;
using UnityEditor;

#endif

namespace JackSParrot.Audio
{
    public abstract class AAudioPlayerBehaviour : MonoBehaviour
    {
        [SerializeField]
        protected AudioClipsStorer _clipsStorer;

        [Header("Play data")]
        [SerializeField]
        protected ClipId _clipId;

        [SerializeField]
        protected bool _playOnEnable = true;

        [SerializeField]
        protected bool _stopOnDisable = false;

        [SerializeField]
        protected bool _stopOnDestroy = false;

        protected AudioService _audioService = null;

        public void Play()
        {
            if (_audioService == null)
            {
                StopAllCoroutines();
                StartCoroutine(WaitForServiceCoroutine());
                return;
            }

            if (!_clipId.IsValid())
            {
                Debug.LogWarning("Tried to play a sound clip not set");
                return;
            }

            DoPlay();
        }

        IEnumerator WaitForServiceCoroutine()
        {
            _audioService = _clipsStorer.AudioService;
            while (_audioService == null)
            {
                _audioService = _clipsStorer.AudioService;
                yield return null;
            }

            Play();
        }

        public abstract void Stop();

        protected abstract void DoPlay();

        private void OnEnable()
        {
            if (_playOnEnable)
            {
                Play();
            }
        }

        private void OnDisable()
        {
            if (_stopOnDisable)
            {
                Stop();
            }
        }

        private void OnDestroy()
        {
            if (_stopOnDestroy)
            {
                Stop();
            }
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            _clipsStorer = EditorUtils.GetOrCreateAudioClipsStorer();
            EditorUtility.SetDirty(gameObject);
#endif
        }
    }
}