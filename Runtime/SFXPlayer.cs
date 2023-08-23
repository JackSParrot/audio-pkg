using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.Audio;

namespace JackSParrot.Services.Audio
{
	public class SfxPlayer: IDisposable
	{
		float volume = 1f;
		public float Volume
		{
			get => volume;
			set
			{
				volume = Mathf.Clamp(value, 0.0001f, 1f);
				outputMixerGroup.audioMixer.SetFloat("sfxVolume", Mathf.Log10(volume) * 20f);
				PlayerPrefs.SetFloat("SfxVolume", volume);
			}
		}

		int                    idGenerator      = 0;
		AudioService           service          = null;
		AudioMixerGroup        outputMixerGroup = null;
		List<AudioClipHandler> handlers         = new List<AudioClipHandler>();


		internal SfxPlayer(AudioService service, AudioMixerGroup outputGroup)
		{
			outputMixerGroup = outputGroup;
			this.service = service;
			for (int i = 0; i < 10; ++i)
			{
				CreateHandler();
			}
		}

		AudioClipHandler CreateHandler()
		{
			AudioClipHandler newHandler = new GameObject("sfx_handler").AddComponent<AudioClipHandler>();
			newHandler.ResetHandler();
			handlers.Add(newHandler);
			newHandler.Id = idGenerator++;
			newHandler.SetOutput(outputMixerGroup);
			UnityEngine.Object.DontDestroyOnLoad(newHandler.gameObject);
			return newHandler;
		}

		AudioClipHandler GetFreeHandler()
		{
			foreach (AudioClipHandler handler in handlers)
			{
				if (!handler.IsAlive)
				{
					handler.ResetHandler();
					handler.Id = idGenerator++;
					return handler;
				}
			}

			return CreateHandler();
		}

		AudioClipData GetClipToPlay(ClipId clipId)
		{
			return service.GetClipById(clipId);
		}

		public int Play(ClipId clipId)
		{
			AudioClipHandler handler = GetFreeHandler();
			handler.Play(GetClipToPlay(clipId));
			return handler.Id;
		}

		public int Play(ClipId clipId, float volume)
		{
			AudioClipHandler handler = GetFreeHandler();
			handler.Play(GetClipToPlay(clipId), volume);
			return handler.Id;
		}

		public int Play(ClipId clipId, Transform toFollow)
		{
			AudioClipHandler handler = GetFreeHandler();
			handler.Play(GetClipToPlay(clipId), toFollow);
			return handler.Id;
		}

		public int Play(ClipId clipId, Vector3 at)
		{
			AudioClipHandler handler = GetFreeHandler();
			handler.Play(GetClipToPlay(clipId), at);
			return handler.Id;
		}

		public void StopPlayingAll()
		{
			handlers.ForEach(StopPlaying);
		}

		public void StopPlaying(int id)
		{
			foreach (AudioClipHandler handler in handlers)
			{
				if (handler.IsAlive && handler.Id == id)
				{
					StopPlaying(handler);
				}
			}
		}

		public void UpdatePlayingClipVolume(int id, float newVolume)
		{
			newVolume = Mathf.Clamp(newVolume, 0.0001f, 1f);
			foreach (AudioClipHandler handler in handlers)
			{
				if (handler.IsAlive && handler.Id == id)
				{
					handler.UpdateVolume(newVolume);
				}
			}
		}

		void StopPlaying(AudioClipHandler handler)
		{
			handler.ResetHandler();
		}

		public void Dispose()
		{
			foreach (AudioClipHandler handler in handlers)
			{
				if (handler != null)
				{
					UnityEngine.Object.Destroy(handler.gameObject);
				}
			}

			handlers.Clear();
		}
	}
}
