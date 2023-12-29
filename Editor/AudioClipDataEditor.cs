using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using JackSParrot.Services.Audio;
using UnityEditor.UIElements;

namespace JackSParrot.Audio
{
	[CustomPropertyDrawer(typeof(AudioClipData))]
	public class AudioClipDataEditor : PropertyDrawer
	{
		private AudioService _service;

		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			if (_service == null)
				LoadAudioManager();

			VisualElement root = new VisualElement();
			if (_service == null)
			{
				root.Add(new Label($"<color=red>Error: {nameof(AudioService)} is missing.</color>"));
				return root;
			}

			PropertyField propertyField = new PropertyField(property);
			Button        btn           = new Button { text = "Play" };
			
			root.Add(propertyField);
			if(_service.IsPreviewReady)
				root.Add(btn);

			btn.clicked += PlayPreview;

			return root;

			async void PlayPreview()
			{
				if (btn.text == "Stop")
					return;

				btn.clicked -= PlayPreview;
				btn.clicked += StopPreview;
				btn.text    =  "Stop";
				await _service.PlayPreview(() => property.boxedValue as AudioClipData);
				StopPreview();
			}

			void StopPreview()
			{
				if (btn.text == "Play")
					return;

				btn.clicked -= StopPreview;
				btn.clicked += PlayPreview;
				btn.text    =  "Play";
				_service.StopPreview();
			}
		}

		private void LoadAudioManager()
		{
			string[] services = AssetDatabase.FindAssets($"t: {nameof(AudioService)}");
			if (services.Length > 1)
			{
				Debug.LogError($"There is a duplicate {nameof(AudioService)}");
				return;
			}

			if (services.Length > 0)
			{
				_service = AssetDatabase.LoadAssetAtPath<AudioService>(AssetDatabase.GUIDToAssetPath(services[0]));
			}
		}
	}
}
