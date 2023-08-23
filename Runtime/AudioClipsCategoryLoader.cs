using System.Collections.Generic;
using UnityEngine;

namespace JackSParrot.Services.Audio
{
	public class AudioClipsCategoryLoader: MonoBehaviour
	{
		[SerializeField]
		private List<string> categoriesToLoad = new List<string>();
		[SerializeField]
		private bool loadOnEnable = true;
		[SerializeField]
		private bool unloadOnDisable = true;

		private AudioService _service;

		public IReadOnlyList<string> CategoriesToLoad => categoriesToLoad;

		private bool _loaded = false;

		public void Load()
		{
			if (_loaded)
			{
				return;
			}

			foreach (string category in categoriesToLoad)
			{
				_service.LoadClipsForCategory(category);
			}

			_loaded = true;
		}

		public void Unload()
		{
			if (!_loaded)
			{
				return;
			}

			foreach (string category in categoriesToLoad)
			{
				_service.UnloadClipsForCategory(category);
			}

			_loaded = false;
		}

		private void OnEnable()
		{
			if (_service == null)
			{
				_service = ServiceLocator.GetService<AudioService>();
			}

			if (loadOnEnable)
			{
				Load();
			}
		}

		private void OnDisable()
		{
			if (unloadOnDisable)
			{
				Unload();
			}
		}
	}
}
