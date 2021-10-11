using UnityEngine;
using System.Collections.Generic;
using JackSParrot.Utils;
using System;

namespace JackSParrot.Services.Audio
{
    public class SFXPlayer : IUpdatable, IDisposable
    {
        float _volume = 1f;
        public float Volume
        {
            get { return _volume; }
            set
            {
                _volume = value;
                foreach (var handler in _handlers)
                {
                    handler.Volume = _volume;
                }
            }
        }

        AudioClipsStorer               _clipStorer  = null;
        List<AudioClipHandler>         _handlers    = new List<AudioClipHandler>();
        Dictionary<AudioClipData, int> _loadedClips = new Dictionary<AudioClipData, int>();
        int                            _idGenerator = 0;

        internal SFXPlayer(AudioClipsStorer clipsStorer)
        {
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
            var new_handler = new GameObject("sfx_handler").AddComponent<AudioClipHandler>();
            new_handler.Reset();
            _handlers.Add(new_handler);
            new_handler.Id = _idGenerator++;
            new_handler.Volume = _volume;
            UnityEngine.Object.DontDestroyOnLoad(new_handler.gameObject);
            return new_handler;
        }

        AudioClipHandler GetFreeHandler()
        {
            foreach (var handler in _handlers)
            {
                if (!handler.IsAlive)
                {
                    handler.Id = _idGenerator++;
                    handler.Volume = _volume;
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
            ReleasePlayingClip(handler.data);
            handler.Reset();
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