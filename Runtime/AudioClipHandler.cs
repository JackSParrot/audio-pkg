using JackSParrot.Utils;
using UnityEngine;

namespace JackSParrot.Services.Audio
{
    public class AudioClipHandler : MonoBehaviour
    {
        float _volume = 1f;
        public float Volume
        {
            get { return _volume; }
            set
            {
                _volume = value;
                if (data != null)
                {
                    _source.volume = data.Volume * _volume;
                }
            }
        }

        public bool IsAlive => _elapsed < _duration || _looping;
        public int Id = -1;
        public AudioClipData data { get; private set; } = null;

        Transform    _toFollow = null;
        Transform    _transform;
        AudioSource  _source            = null;
        float        _elapsed           = 0f;
        float        _duration          = 0f;
        bool         _looping           = false;
        private bool _isFollowingTarget = false;

        void Awake()
        {
            _transform = transform;
            if (_source == null)
            {
                _source = gameObject.AddComponent<AudioSource>();
            }
        }

        public void Reset()
        {
            data = null;
            _source.Stop();
            _looping = false;
            _elapsed = 0f;
            _duration = 0f;
            _toFollow = null;
            _isFollowingTarget = false;
            _transform.localPosition = Vector3.zero;
            gameObject.SetActive(false);
            Id = -1;
        }

        public void UpdateHandler(float deltaTime)
        {
            _elapsed += deltaTime;
            if (!_isFollowingTarget)
            {
                return;
            }

            if (_toFollow != null)
            {
                _transform.position = _toFollow.position;
                return;
            }

            _elapsed = _duration;
            _looping = false;
            _source.Stop();
        }

        public void Play(AudioClipData data)
        {
            this.data = data;
            _duration = 9999f;
            if (data.ReferencedClip.Asset != null)
            {
                OnLoaded(data.ReferencedClip.Asset as AudioClip);
                return;
            }

            if (data.ReferencedClip.OperationHandle.IsValid())
            {
                if (data.ReferencedClip.OperationHandle.IsDone)
                {
                    OnLoaded(data.ReferencedClip.OperationHandle.Result as AudioClip);
                    return;
                }

                data.ReferencedClip.OperationHandle.Completed += h => OnLoaded(h.Result as AudioClip);

                return;
            }

            data.ReferencedClip.LoadAssetAsync<AudioClip>().Completed += h => OnLoaded(h.Result);
        }

        void OnLoaded(AudioClip clip)
        {
            if (clip == null)
            {
                SharedServices.GetService<ICustomLogger>()?.LogError("Cannot load audio clip: " + data.ClipId);
                return;
            }

            gameObject.SetActive(true);
            gameObject.name = data.ClipId;
            _source.volume = data.Volume * _volume;
            _source.pitch = data.Pitch;
            _source.clip = clip;
            _source.loop = data.Loop;
            _source.spatialBlend = 0f;
            _source.Play();
            _toFollow = null;
            _looping = data.Loop;
            _duration = clip.length;
        }

        public void Play(AudioClipData data, Vector3 position)
        {
            Play(data);
            _source.spatialBlend = 1f;
            _transform.position = position;
        }

        public void Play(AudioClipData data, Transform parent)
        {
            Play(data);
            _source.spatialBlend = 1f;
            _transform.position = parent.position;
            _toFollow = parent;
            _isFollowingTarget = true;
        }
    }
}