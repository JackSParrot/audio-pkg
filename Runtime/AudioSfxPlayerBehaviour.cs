using UnityEngine;

namespace JackSParrot.Services.Audio
{
    public class AudioSfxPlayerBehaviour : AAudioPlayerBehaviour
    {
        [Header("SFX settings")]
        [SerializeField]
        private bool PlayAtThisPosition = false;
        [SerializeField]
        private bool PlayFollowingThis = false;

        public float Volume
        {
            get => localVolume;
            set => UpdatePlayingClipVolume(value);
        }

        protected int   playingClip = -1;
        protected float localVolume = 1f;

        protected override void DoPlay()
        {
            if (PlayFollowingThis)
            {
                playingClip = audioService.PlaySfx(clipId, transform);
                return;
            }

            if (PlayAtThisPosition)
            {
                playingClip = audioService.PlaySfx(clipId, transform.position);
                return;
            }

            playingClip = audioService.PlaySfx(clipId);
        }

        public override void Stop()
        {
            if (playingClip >= 0)
            {
                audioService.StopPlayingSfx(playingClip);
                playingClip = -1;
            }
        }

        public void UpdatePlayingClipVolume(float volume)
        {
            localVolume = Mathf.Clamp(volume, 0f, 1f);
            if (playingClip >= 0)
            {
                audioService.UpdatePlayingClipVolume(playingClip, volume);
            }
        }
    }
}