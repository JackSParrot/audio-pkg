using UnityEngine;

namespace JackSParrot.Services.Audio
{
    public class AudioMusicPlayerBehaviour : AAudioPlayerBehaviour
    {
        [Header("Music settings")]
        [SerializeField]
        private float changeMusicCrossfadeSeconds = 0f;
        [SerializeField]
        private float fadeOutSeconds = 0f;
        [SerializeField]
        private float fadeInSeconds = 0f;

        private bool  _playing = false;
        private float _delay   = 0f;

        public bool Playing => _playing;

        protected override void DoPlay()
        {
            _playing = true;
            if (string.IsNullOrEmpty(audioService?.Music?.PlayingClipId))
            {
                _delay = fadeInSeconds;
                audioService?.PlayMusic(clipId, fadeInSeconds);
                return;
            }

            _delay = changeMusicCrossfadeSeconds;
            audioService?.CrossFadeMusic(clipId, changeMusicCrossfadeSeconds);
        }

        public override void Stop()
        {
            if (audioService?.Music.PlayingClipId == clipId)
            {
                audioService?.StopMusic(fadeOutSeconds);
            }

            _playing = false;
        }

        private void Update()
        {
            _delay -= Time.deltaTime;
            if (_delay > 0f)
            {
                return;
            }

            if (_playing && audioService?.Music?.PlayingClipId != clipId)
            {
                gameObject.SetActive(false);
                _playing = false;
            }
        }
    }
}