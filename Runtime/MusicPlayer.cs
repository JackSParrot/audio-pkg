using System.Collections;
using UnityEngine;
using System;
using JackSParrot.AddressablesEssentials;
using UnityEngine.Audio;

namespace JackSParrot.Services.Audio
{
	public class MusicPlayer: IDisposable
	{
		float volume = 1f;
		public float Volume
		{
			get { return volume; }
			set
			{
				volume = Mathf.Clamp(value, 0.0001f, 1f);
				outputMixerGroup.audioMixer.SetFloat("musicVolume", Mathf.Log10(volume) * 20f);
				PlayerPrefs.SetFloat("MusicVolume", volume);
			}
		}

		public string PlayingClipId => playingClip?.ClipId ?? "";
		public bool IsPlaying => source != null && source.isPlaying;

		private AudioClipData             playingClip      = null;
		private AudioSource               source           = null;
		private AudioService              service          = null;
		private AudioService.UpdateRunner updateRunner     = null;
		private AudioMixerGroup           outputMixerGroup = null;
		private GameObject                requester        = null;

		internal MusicPlayer(AudioService audioService, AudioMixerGroup outputGroup,
							 AudioService.UpdateRunner updateRunner)
		{
			this.updateRunner = updateRunner;

			outputMixerGroup = outputGroup;
			service = audioService;
			source = new GameObject("MusicPlayer").AddComponent<AudioSource>();
			UnityEngine.Object.DontDestroyOnLoad(source.gameObject);
			source.playOnAwake = false;
			source.spatialBlend = 0f;
			source.outputAudioMixerGroup = outputMixerGroup;
		}

		public void Play(ClipId clipId)
		{
			if (playingClip != null && clipId == playingClip.ClipId && source.isPlaying)
			{
				return;
			}

			if (playingClip != null)
			{
				source.Stop();
				playingClip.ReferencedClip.ReleaseAsset();
				playingClip = null;
			}

			AudioClipData clip = service.GetClipById(clipId);

			if (clip == null)
			{
				Debug.Assert(false);
				return;
			}

			playingClip = clip;
			if (requester != null)
			{
				UnityEngine.Object.Destroy(requester);
			}

			requester = new GameObject("audioRequester");
			UnityEngine.Object.DontDestroyOnLoad(requester);
			AddressableAssetsUtility.LoadAsset<AudioClip>(clip.ReferencedClip, requester, OnClipLoaded);
		}

		public void Play(ClipId clipId, float fadeInSeconds)
		{
			Play(clipId);
			updateRunner.StopAllCoroutines();
			updateRunner.StartCoroutine(FadeInCoroutine(fadeInSeconds));
		}

		public void Stop(float fadeOutTime)
		{
			if (updateRunner == null)
			{
				return;
			}

			updateRunner.StopAllCoroutines();
			updateRunner.StartCoroutine(FadeOutCoroutine(fadeOutTime));
		}

		void OnClipLoaded(AudioClip clip)
		{
			if (clip == null)
			{
				Debug.Assert(false);
				return;
			}

			source.clip = clip;
			source.loop = playingClip.Loop;
			source.pitch = playingClip.Pitch;
			source.volume = playingClip.Volume;

			source.Play();
		}

		public void CrossFade(ClipId clipId, float duration)
		{
			if (updateRunner == null)
			{
				return;
			}

			updateRunner.StopAllCoroutines();
			if (playingClip != null)
			{
				updateRunner.StartCoroutine(CrossFadeCoroutine(clipId, duration));
			}
			else
			{
				Play(clipId);
				updateRunner.StartCoroutine(FadeInCoroutine(duration));
			}
		}

		IEnumerator CrossFadeCoroutine(ClipId fadeTo, float duration)
		{
			if (source == null)
			{
				yield break;
			}

			float halfDuration = duration * 0.5f;
			while (!source.isPlaying)
			{
				yield return null;
			}

			updateRunner.StartCoroutine(FadeOutCoroutine(halfDuration));
			yield return new WaitForSeconds(halfDuration);

			// If the source's time was set manually, then the time wouldn't be reset for the fadein coroutine. (throwing a silent null ref)
			// So you have to reset it manually.
			source.time = 0.0f;

			Play(fadeTo);
			updateRunner.StartCoroutine(FadeInCoroutine(halfDuration));
		}

		IEnumerator FadeOutCoroutine(float duration)
		{
			if (source == null)
			{
				yield break;
			}

			float remaining = duration;

			while (!source.isPlaying)
			{
				yield return null;
			}

			source.volume = 0f;
			while (remaining > 0f)
			{
				source.volume = playingClip.Volume * remaining / duration;
				yield return null;
				remaining -= Time.deltaTime;
			}

			source.Stop();
			playingClip.ReferencedClip.ReleaseAsset();
			playingClip = null;
		}

		IEnumerator FadeInCoroutine(float duration)
		{
			if (source == null)
			{
				yield break;
			}

			while (!source.isPlaying)
			{
				yield return null;
			}

			float remaining = duration;
			while (remaining > 0)
			{
				source.volume = playingClip.Volume * (1f - (remaining / duration));
				yield return null;
				remaining -= Time.deltaTime;
			}
		}

		public void Dispose()
		{
			if (playingClip != null)
			{
				if (playingClip.ReferencedClip.Asset != null)
				{
					playingClip.ReferencedClip.ReleaseAsset();
				}
			}

			if (updateRunner == null || source == null)
			{
				return;
			}

			source.Stop();
			updateRunner.StopAllCoroutines();
			UnityEngine.Object.Destroy(source.gameObject);
			if (requester != null)
			{
				UnityEngine.Object.Destroy(requester);
				requester = null;
			}
		}

		public void UpdateSourceTimer(float time)
		{
			source.time = time;
		}
	}
}
