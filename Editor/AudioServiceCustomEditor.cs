using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace JackSParrot.Services.Audio
{
	[CustomEditor(typeof(AudioService))]
	public class AudioServiceCustomEditor: UnityEditor.Editor
	{
		private AudioService _service;

		private ReorderableList _list;

		private void OnEnable()
		{
			_service = (AudioService)target;
		}

		private void OnDisable()
		{
			_service.DisposePreview();
		}

		public override VisualElement CreateInspectorGUI()
		{
			if (_service == null)
				Initialize();

			VisualElement root = new VisualElement();
			if (_service == null)
			{
				root.Add(new Label($"<color=red>Error: {nameof(AudioService)} is missing.</color>"));
				return root;
			}

			VisualElement inspectorFoldout = new Foldout();
			InspectorElement.FillDefaultInspector(inspectorFoldout, serializedObject, this);
			Button previewButton = new Button { text = "Enable Previews" };
			inspectorFoldout.Add(previewButton);
			root.Add(inspectorFoldout);
			previewButton.clicked += () =>
			{
				_service.PreparePreview();
				root.Clear();
				VisualElement inspector = new Foldout();
				InspectorElement.FillDefaultInspector(inspector, serializedObject, this);
				root.Add(inspector);
				root.Bind(serializedObject);
			};
			return root;
		}

		private void Initialize()
		{
			LoadAudioManager();
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
