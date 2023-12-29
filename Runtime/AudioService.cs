using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Audio;

namespace JackSParrot.Services.Audio
{
	[CreateAssetMenu(fileName = "AudioService", menuName = "JackSParrot/Services/AudioService")]
	public class AudioService: AService
	{
		public AudioMixer OutputMixer = null;

		[SerializeField]
		List<AudioCategory> _categories = new List<AudioCategory>();

		private List<string> _categoriesLoaded = new List<string>();

		public IReadOnlyList<AudioCategory> Categories => _categories;

		private float           volume = 1f;
		private MusicPlayer     musicPlayer;
		private SfxPlayer       sfxPlayer;
		private AudioMixerGroup masterGroup;
		private UpdateRunner    updateRunner;

		public MusicPlayer Music => musicPlayer;
		public SfxPlayer Sfx => sfxPlayer;

		public float Volume
		{
			get => volume;
			set
			{
				volume = Mathf.Clamp(value, 0.001f, 1f);
				masterGroup.audioMixer.SetFloat("masterVolume", Mathf.Log10(volume) * 20f);
				PlayerPrefs.SetFloat("MasterVolume", volume);
			}
		}

		public float MusicVolume
		{
			get => musicPlayer.Volume;
			set => musicPlayer.Volume = value;
		}

		public float SfxVolume
		{
			get => sfxPlayer.Volume;
			set => sfxPlayer.Volume = value;
		}

		public override void Cleanup()
		{
			sfxPlayer.Dispose();
			musicPlayer.Dispose();
			Reset();
			if (updateRunner != null)
			{
				UnityEngine.Object.Destroy(updateRunner);
			}
		}

		public override List<Type> GetDependencies()
		{
			return null;
		}

		public override IEnumerator Initialize()
		{
			updateRunner = new GameObject("AudioServiceUpdater", typeof(UpdateRunner)).GetComponent<UpdateRunner>();
			UnityEngine.Object.DontDestroyOnLoad(updateRunner);
			updateRunner.OnDestroyed = Cleanup;

			AudioMixerGroup musicGroup = OutputMixer.FindMatchingGroups("Music")[0];
			AudioMixerGroup sfxGroup = OutputMixer.FindMatchingGroups("SFX")[0];

			masterGroup = OutputMixer.FindMatchingGroups("Master")[0];
			sfxPlayer = new SfxPlayer(this, sfxGroup);
			musicPlayer = new MusicPlayer(this, musicGroup, updateRunner);
			Volume = PlayerPrefs.GetFloat("MasterVolume", 1f);
			SfxVolume = PlayerPrefs.GetFloat("SfxVolume", 1f);
			MusicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
			Status = EServiceStatus.Initialized;
			yield return null;
		}

		public void PlayMusic(ClipId clipId, float fadeInSeconds = 1f) => musicPlayer.Play(clipId, fadeInSeconds);
		public void StopMusic(float fadeOutSeconds = 1f) => musicPlayer.Stop(fadeOutSeconds);

		public void CrossFadeMusic(ClipId clipId, float duration = 0.3f) => musicPlayer.CrossFade(clipId, duration);

		public int PlaySfx(ClipId clipId) => sfxPlayer.Play(clipId);
		public int PlaySfx(ClipId clipId, float volumeModifier) => sfxPlayer.Play(clipId, volumeModifier);

		public int PlaySfx(ClipId clipId, Transform toFollow) => sfxPlayer.Play(clipId, toFollow);

		public int PlaySfx(ClipId clipId, Vector3 at) => sfxPlayer.Play(clipId, at);

		public void UpdatePlayingClipVolume(int id, float newVolume) => sfxPlayer.UpdatePlayingClipVolume(id, newVolume);

		public void StopPlayingSfx(int id) => sfxPlayer.StopPlaying(id);
		public void StopPlayingAllSfx() => sfxPlayer.StopPlayingAll();

		public List<string> GetAllClips()
		{
			List<string> retVal = new List<string>();
			foreach (AudioCategory category in _categories)
			{
				foreach (AudioClipData clip in category.Clips)
				{
					retVal.Add(clip.ClipId);
				}
			}

			return retVal;
		}

		public AudioClipData GetClipById(ClipId clipId)
		{
			foreach (AudioCategory category in _categories)
			{
				foreach (AudioClipData clip in category.Clips)
				{
					if (clip.ClipId == clipId)
					{
						return clip;
					}
				}
			}

			Debug.Assert(false);
			return null;
		}

		public void LoadClipsForCategory(string categoryId, Action onLoaded = null)
		{
			AudioCategory category = GetCategoryById(categoryId);
			if (category == null)
			{
				Debug.LogError("Tried to load a category that doesn't exist: " + categoryId);
				onLoaded?.Invoke();
				return;
			}

			if (_categoriesLoaded.Contains(categoryId))
			{
				Debug.LogWarning("Trying to load an already loaded category: " + categoryId);
				onLoaded?.Invoke();
				return;
			}

			int tasks = 0;
			foreach (AudioClipData clip in category.Clips)
			{
				if (clip.ReferencedClip.Asset != null)
				{
					tasks++;
					if (tasks == category.Clips.Count)
					{
						_categoriesLoaded.Add(categoryId);
						onLoaded?.Invoke();
					}

					continue;
				}

				clip.ReferencedClip.LoadAssetAsync().Completed +=
					h =>
					{
						++tasks;
						if (tasks == category.Clips.Count)
						{
							_categoriesLoaded.Add(categoryId);
							onLoaded?.Invoke();
						}
					};
			}
		}

		public void UnloadClipsForCategory(string categoryId)
		{
			AudioCategory category = GetCategoryById(categoryId);
			if (category == null)
			{
				Debug.LogWarning("Tried to unload a category that doesn't exist: " + categoryId);
				return;
			}

			if (!_categoriesLoaded.Contains(categoryId))
			{
				Debug.LogWarning("Trying to unload a non loaded category: " + categoryId);
				return;
			}

			foreach (AudioClipData audioClipData in category.Clips)
			{
				audioClipData.ReferencedClip.ReleaseAsset();
			}

			_categoriesLoaded.Remove(categoryId);
		}

		public AudioCategory GetCategoryById(string categoryId)
		{
			foreach (AudioCategory category in _categories)
			{
				if (categoryId.Equals(category.Id))
				{
					return category;
				}
			}

			return null;
		}

		public void Reset()
		{
			foreach (AudioCategory category in _categories)
			{
				foreach (AudioClipData clip in category.Clips)
				{
					if (clip.ReferencedClip.Asset != null)
						clip.ReferencedClip.ReleaseAsset();
				}
			}

			_categoriesLoaded.Clear();
		}

		#if UNITY_EDITOR
		private AudioSource _source;

		public bool IsPreviewReady => _source != null;
		
		public void PreparePreview()
		{
			_source = new GameObject().AddComponent<AudioSource>();
		}

		public async System.Threading.Tasks.Task PlayPreview(Func<AudioClipData> getData)
		{
			if (!IsPreviewReady)
				return;
			
			AudioClipData clip = getData();
			_source.clip   = clip.ReferencedClip.editorAsset;
			_source.volume = clip.Volume;
			_source.Play();
			int remaining = (int)(_source.clip.length * 1000);
			while (remaining > 0 && _source != null && _source.isPlaying)
			{
				clip           = getData();
				_source.volume = clip.Volume;
				_source.pitch  = clip.Pitch;
				await System.Threading.Tasks.Task.Delay(50);
				remaining -= 50;
			}
		}

		public void StopPreview()
		{
			if(_source != null) _source.Stop();
		}

		public void DisposePreview()
		{
			if (!IsPreviewReady)
				return;
			
			DestroyImmediate(_source.gameObject);
			_source = null;
		}
		#endif

		internal class UpdateRunner: MonoBehaviour
		{
			public Action<float> OnUpdate    = t => { };
			public Action        OnDestroyed = () => { };
			private void Update() => OnUpdate(Time.deltaTime);

			private void OnDestroy() => OnDestroyed();
		}
	}
}
