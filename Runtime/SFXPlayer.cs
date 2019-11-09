using UnityEngine;
using System.Collections.Generic;
using JackSParrot.Utils;
using System;

namespace JackSParrot.Services.Audio
{
    public class SFXPlayer : IUpdatable, IDisposable
    {
        AudioClipsStorer _clipStorer = null;
        List<AudioClipHandler> _handlers = new List<AudioClipHandler>();
        int _idGenerator = 0;

        public SFXPlayer(AudioClipsStorer clipsStorer)
        {
            if(SharedServices.GetService<IUpdateScheduler>() == null)
            {
                SharedServices.RegisterService<IUpdateScheduler>(new UnityUpdateScheduler());
            }
            SharedServices.GetService<IUpdateScheduler>().ScheduleUpdate(this);
            _clipStorer = clipsStorer;
            for(int i = 0; i < 10; ++i)
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
                }
                else if(handler.gameObject.activeSelf)
                {
                    handler.IsAlive = false;
                }
            }
        }

        AudioClipHandler CreateHandler()
        {
            var new_handler = new GameObject("sfx_clip").AddComponent<AudioClipHandler>();
            new_handler.transform.position = Vector3.zero;
            new_handler.IsAlive = false;
            _handlers.Add(new_handler);
            new_handler.Id = _idGenerator++;
            UnityEngine.Object.DontDestroyOnLoad(new_handler.gameObject);
            return new_handler;
        }

        AudioClipHandler GetFreeHandler()
        {
            foreach (var handler in _handlers)
            {
                if(!handler.IsAlive)
                {
                    handler.Id = _idGenerator++;
                    return handler;
                }
            }
            return CreateHandler();
        }

        public int Play(string clipName)
        {
            var handler = GetFreeHandler();
            handler.Play(_clipStorer.GetClipByName(clipName));
            return handler.Id;
        }

        public int Play(string clipName, Transform toFollow)
        {
            var handler = GetFreeHandler();
            handler.Play(_clipStorer.GetClipByName(clipName), toFollow);
            return handler.Id;
        }

        public int Play(string clipName, Vector3 at)
        {
            var handler = GetFreeHandler();
            handler.Play(_clipStorer.GetClipByName(clipName), at);
            return handler.Id;
        }

        public void StopPlaying(int id)
        {
            foreach(var handler in _handlers)
            {
                if(handler.Id == id)
                {
                    handler.IsAlive = false;
                }
            }
        }

        public void Dispose()
        {
            SharedServices.GetService<IUpdateScheduler>().UnscheduleUpdate(this);
            foreach(var handler in _handlers)
            {
                UnityEngine.Object.Destroy(handler.gameObject);
            }
        }
    }
}

