using UnityEngine;
using System.Collections.Generic;
using JackSParrot.Utils;
using System;
using UnityEngine.Audio;

namespace JackSParrot.Services.Audio
{
    public class SFXPlayer : IUpdatable, IDisposable
    {
        float _volume = 1f;
        public float Volume
        {
            get => _volume;
            set
            {
                _volume = Mathf.Clamp(value, 0.0001f, 1f);
                _outputMixerGroup.audioMixer.SetFloat("sfxVolume", Mathf.Log10(_volume) * 20f);
            }
        }

        int                    _idGenerator      = 0;
        AudioClipsStorer       _clipStorer       = null;
        AudioMixerGroup        _outputMixerGroup = null;
        List<AudioClipHandler> _handlers         = new List<AudioClipHandler>();

        Dictionary<AudioClipData, int> _loadedClips = new Dictionary<AudioClipData, int>();


        internal SFXPlayer(AudioClipsStorer clipsStorer, AudioMixerGroup outputGroup)
        {
            _outputMixerGroup = outputGroup;

            var updater = SharedServices.GetService<IUpdateScheduler>();
            if (updater == null)
            {
                updater = new UnityUpdateScheduler();
                SharedServices.RegisterService(updater);
            }

            var dispatcher = SharedServices.GetService<EventDispatcher>();
            if (dispatcher == null)
            {
                dispatcher = new EventDispatcher();
                SharedServices.RegisterService(dispatcher);
            }

            updater.ScheduleUpdate(this);
            dispatcher.AddListener<SceneManagementService.SceneUnloadedEvent>(OnSceneUnloaded);
            _clipStorer = clipsStorer;
            for (int i = 0; i < 10; ++i)
            {
                CreateHandler();
            }
        }

        public void UpdateDelta(float deltaTime)
        {
            foreach (var handler in _handlers)
            {
                if (handler.IsAlive)
                {
                    handler.UpdateHandler(deltaTime);
                    if (!handler.IsAlive)
                    {
                        StopPlaying(handler);
                    }
                }
            }
        }

        public void ReleaseReferenceCache()
        {
            foreach (var kvp in _loadedClips)
            {
                if (kvp.Value < 1)
                {
                    if (kvp.Key.ReferencedClip.Asset != null && kvp.Key.AutoRelease)
                    {
                        kvp.Key.ReferencedClip.ReleaseAsset();
                    }
                }
            }
        }

        void OnSceneUnloaded(SceneManagementService.SceneUnloadedEvent e)
        {
            ReleaseReferenceCache();
        }

        AudioClipHandler CreateHandler()
        {
            var newHandler = new GameObject("sfx_handler").AddComponent<AudioClipHandler>();
            newHandler.ResetHandler();
            _handlers.Add(newHandler);
            newHandler.Id = _idGenerator++;
            newHandler.SetOutput(_outputMixerGroup);
            UnityEngine.Object.DontDestroyOnLoad(newHandler.gameObject);
            return newHandler;
        }

        AudioClipHandler GetFreeHandler()
        {
            foreach (var handler in _handlers)
            {
                if (!handler.IsAlive)
                {
                    handler.Id = _idGenerator++;
                    return handler;
                }
            }

            return CreateHandler();
        }

        AudioClipData GetClipToPlay(ClipId clipId)
        {
            foreach (var kvp in _loadedClips)
            {
                if (kvp.Key.ClipId == clipId)
                {
                    _loadedClips[kvp.Key] += 1;
                    return kvp.Key;
                }
            }

            var sfx = _clipStorer.GetClipById(clipId);
            _loadedClips.Add(sfx, 1);
            return sfx;
        }

        void ReleasePlayingClip(AudioClipData clip)
        {
            if (clip != null && _loadedClips.ContainsKey(clip))
            {
                _loadedClips[clip] = Mathf.Max(0, _loadedClips[clip]);
            }
        }

        public int Play(ClipId clipId)
        {
            var handler = GetFreeHandler();
            handler.Play(GetClipToPlay(clipId));
            return handler.Id;
        }

        public int Play(ClipId clipId, Transform toFollow)
        {
            var handler = GetFreeHandler();
            handler.Play(GetClipToPlay(clipId), toFollow);
            return handler.Id;
        }

        public int Play(ClipId clipId, Vector3 at)
        {
            var handler = GetFreeHandler();
            handler.Play(GetClipToPlay(clipId), at);
            return handler.Id;
        }

        public void StopPlaying(int id)
        {
            foreach (var handler in _handlers)
            {
                if (handler.Id == id)
                {
                    StopPlaying(handler);
                }
            }
        }

        void StopPlaying(AudioClipHandler handler)
        {
            ReleasePlayingClip(handler.Data);
            handler.ResetHandler();
        }

        public void Dispose()
        {
            SharedServices.GetService<IUpdateScheduler>().UnscheduleUpdate(this);
            foreach (var handler in _handlers)
            {
                UnityEngine.Object.Destroy(handler.gameObject);
            }

            SharedServices.GetService<EventDispatcher>()
                .RemoveListener<SceneManagementService.SceneUnloadedEvent>(OnSceneUnloaded);
        }
    }
}