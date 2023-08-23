using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using JackSParrot.Services.Audio;

namespace JackSParrot.Audio
{
    [CustomPropertyDrawer(typeof(ClipId))]
    public class ClipIdEditor : PropertyDrawer
    {
        private Dictionary<string, List<string>> _ids;

        private AudioService _service;
        private List<string>   _categories;

        private int _previouslySelectedCategory = -1;

        private void Initialize()
        {
            LoadAudioManager();
            if (_service == null)
                return;

            IReadOnlyList<AudioCategory> categories = _service.Categories;
            _ids = new Dictionary<string, List<string>> { ["NONE"] = new List<string> { "NONE" } };
            _categories = new List<string> { "NONE" };
            for (int i = 0; i < categories.Count; ++i)
            {
                List<string> clips = new List<string>();
                foreach (AudioClipData clipData in categories[i].Clips)
                {
                    clips.Add(clipData.ClipId);
                }

                _ids.Add(categories[i].Id, clips);
                _categories.Add(categories[i].Id);
            }
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            if (_service == null)
                Initialize();

            VisualElement root = new VisualElement();
            if (_service == null)
            {
                root.Add(new Label(
                    $"<color=red>Error: {nameof(AudioService)} is missing.</color>"));
                return root;
            }

            SerializedProperty clipId = property.FindPropertyRelative("Id");

            for (int i = 0; i < _ids.Count && _previouslySelectedCategory < 0; ++i)
            {
                if (_ids[_categories[i]].Contains(clipId.stringValue))
                {
                    _previouslySelectedCategory = i;
                }
            }

            if (_previouslySelectedCategory == -1)
                _previouslySelectedCategory = 0;

            int previouslySelectedClip = -1;
            for (int i = 0; i < _ids[_categories[_previouslySelectedCategory]].Count && previouslySelectedClip < 0; ++i)
            {
                previouslySelectedClip = _ids[_categories[_previouslySelectedCategory]].IndexOf(clipId.stringValue);
            }

            if (previouslySelectedClip == -1)
                previouslySelectedClip = 0;

            DropdownField clips = new DropdownField(_ids[_categories[_previouslySelectedCategory]],
                previouslySelectedClip);
            DropdownField categories = new DropdownField(_categories, _previouslySelectedCategory);

            categories.RegisterCallback<ChangeEvent<string>>(_ => OnCategorySelected());
            clips.RegisterCallback<ChangeEvent<string>>(_ => OnSelectedClip());

            root.Add(new Label("Category"));
            root.Add(categories);
            root.Add(new Label("Clip"));
            root.Add(clips);

            void OnCategorySelected()
            {
                int selectedCategoryIdx = categories.index;
                if (selectedCategoryIdx != _previouslySelectedCategory)
                {
                    clipId.stringValue = selectedCategoryIdx == 0
                        ? string.Empty
                        : _ids[_categories[selectedCategoryIdx]][0];
                    _previouslySelectedCategory = selectedCategoryIdx;

                    root.Remove(clips);
                    clips = new DropdownField(_ids[_categories[_previouslySelectedCategory]], 0);
                    clips.RegisterCallback<ChangeEvent<string>>(_ =>
                        OnSelectedClip());
                    root.Add(clips);
                    property.serializedObject.ApplyModifiedProperties();
                }
            }

            void OnSelectedClip()
            {
                int idx = clips.index;
                string clip = _ids[_categories[_previouslySelectedCategory]][idx];
                clipId.stringValue = clip;
                property.serializedObject.ApplyModifiedProperties();
            }

            return root;
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