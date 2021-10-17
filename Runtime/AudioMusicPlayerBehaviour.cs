using UnityEngine;

namespace JackSParrot.Audio
{
    public class AudioMusicPlayerBehaviour : AAudioPlayerBehaviour
    {
        [Header("Music settings")]
        [SerializeField]
        private float _changeMusicCrossfadeSeconds = 0f;
        [SerializeField]
        private float _fadeOutSeconds = 0f;

        protected override void DoPlay()
        {
            if (_changeMusicCrossfadeSeconds < 0.01f)
            {
                _audioService.PlayMusic(_clipId);
                return;
            }

            _audioService.CrossFadeMusic(_clipId, _changeMusicCrossfadeSeconds);
        }

        public override void Stop()
        {
            if (_audioService?.Music.PlayingClipId == _clipId)
            {
                _audioService?.StopMusic(_fadeOutSeconds);
            }
        }
    }
}