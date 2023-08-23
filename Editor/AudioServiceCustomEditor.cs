using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace JackSParrot.Services.Audio
{
	[CustomEditor(typeof(AudioService))]
	public class AudioServiceCustomEditor: UnityEditor.Editor
	{
		private AudioService storer;
		private List<string> duplicatedCategories = new List<string>();
		private List<string> duplicatedClips      = new List<string>();

		private ReorderableList list;
		private List<string>    expandedClips           = new List<string>();
		private List<string>    expandedCategories      = new List<string>();
		private List<string>    expandedCategoriesClips = new List<string>();

		private void OnEnable()
		{
			storer = (AudioService)target;
			list = new ReorderableList(serializedObject, serializedObject.FindProperty("_categories"), true, true,
									   true, true);
			list.drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, "Categories");
			list.drawElementCallback = DrawElementCallback;
			list.elementHeightCallback = ElementHeightCallback;
			list.onAddCallback = OnAddCallback;
		}

		private void OnAddCallback(ReorderableList list)
		{
			SerializedProperty categoriesPropertyy = serializedObject.FindProperty("_categories");
			categoriesPropertyy.InsertArrayElementAtIndex(Mathf.Max(0, categoriesPropertyy.arraySize - 1));
			SerializedProperty categoryProperty =
				categoriesPropertyy.GetArrayElementAtIndex(categoriesPropertyy.arraySize - 1);
			categoryProperty.FindPropertyRelative("Id").stringValue =
				$"new Cateogry ({categoriesPropertyy.arraySize - 1})";
			categoryProperty.FindPropertyRelative("Clips").arraySize = 0;
			serializedObject.ApplyModifiedProperties();
		}

		private float ElementHeightCallback(int index)
		{
			float lineHeight = EditorGUIUtility.singleLineHeight;
			float retVal = lineHeight;
			AudioCategory category = storer.Categories[index];
			if (!expandedCategories.Contains(category.Id))
			{
				return retVal;
			}

			retVal += lineHeight;
			retVal += lineHeight;
			if (!expandedCategoriesClips.Contains(category.Id))
			{
				return retVal;
			}

			retVal += lineHeight;

			foreach (AudioClipData audioClipData in category.Clips)
			{
				retVal += expandedClips.Contains(audioClipData.ClipId) ? 7 * lineHeight : lineHeight;
			}

			return retVal;
		}

		private void DrawElementCallback(Rect rect, int index, bool isactive, bool isfocused)
		{
			float lineHeight = EditorGUIUtility.singleLineHeight;

			SerializedProperty category = list.serializedProperty.GetArrayElementAtIndex(index);
			rect.x += 10f;
			rect.width -= 10f;
			rect.height = lineHeight;
			SerializedProperty categoryIdProperty = category.FindPropertyRelative("Id");
			bool foldout = EditorGUI.Foldout(rect, expandedCategories.Contains(categoryIdProperty.stringValue),
											 categoryIdProperty.stringValue);
			rect.y += lineHeight;
			if (!foldout)
			{
				expandedCategories.Remove(categoryIdProperty.stringValue);
				return;
			}

			if (!expandedCategories.Contains(categoryIdProperty.stringValue))
			{
				expandedCategories.Add(categoryIdProperty.stringValue);
			}

			string newId = EditorGUI.TextField(rect, "Id", categoryIdProperty.stringValue);
			if (newId != categoryIdProperty.stringValue)
			{
				expandedCategories.Remove(categoryIdProperty.stringValue);
				expandedCategoriesClips.Remove(categoryIdProperty.stringValue);
				expandedCategories.Add(newId);
				expandedCategoriesClips.Add(newId);
				categoryIdProperty.stringValue = newId;
			}

			rect.y += lineHeight;
			bool clipsFoldout = EditorGUI.Foldout(rect, expandedCategoriesClips.Contains(newId), "Clips");
			if (!clipsFoldout)
			{
				expandedCategoriesClips.Remove(newId);
				return;
			}

			if (!expandedCategoriesClips.Contains(newId))
			{
				expandedCategoriesClips.Add(newId);
			}

			SerializedProperty clips = category.FindPropertyRelative("Clips");
			rect.y += lineHeight;
			rect.x += 10f;
			rect.width -= 10f;
			Color old = GUI.color;
			for (int i = 0; i < clips.arraySize; ++i)
			{
				SerializedProperty clip = clips.GetArrayElementAtIndex(i);
				SerializedProperty clipIdProperty = clip.FindPropertyRelative("ClipId");
				bool clipFoldout = EditorGUI.Foldout(new Rect(rect.x, rect.y, rect.width - 40f, rect.height),
													 expandedClips.Contains(clipIdProperty.stringValue),
													 clipIdProperty.stringValue);
				GUI.color = Color.red;
				if (GUI.Button(new Rect(rect.width - 40f, rect.y, 40f, rect.height), "-"))
				{
					//add item
					clips.DeleteArrayElementAtIndex(i--);
					continue;
				}

				GUI.color = i % 2 == 0 ? Color.black : Color.white;
				GUI.Box(new Rect(rect.x, rect.y, rect.width, lineHeight * (clipFoldout ? 7f : 1f)), "");
				GUI.color = old;
				rect.y += lineHeight;
				if (!clipFoldout)
				{
					expandedClips.Remove(clipIdProperty.stringValue);
					continue;
				}

				EditorGUI.PropertyField(rect, clipIdProperty);
				rect.y += lineHeight;
				EditorGUI.PropertyField(rect, clip.FindPropertyRelative("ReferencedClip"));
				rect.y += lineHeight;
				EditorGUI.PropertyField(rect, clip.FindPropertyRelative("Volume"));
				rect.y += lineHeight;
				EditorGUI.PropertyField(rect, clip.FindPropertyRelative("Pitch"));
				rect.y += lineHeight;
				EditorGUI.PropertyField(rect, clip.FindPropertyRelative("Loop"));
				rect.y += lineHeight;
				if (!expandedClips.Contains(clipIdProperty.stringValue))
				{
					expandedClips.Add(clipIdProperty.stringValue);
				}
			}

			GUI.color = Color.green;
			if (GUI.Button(rect, "+"))
			{
				clips.InsertArrayElementAtIndex(Mathf.Max(0, clips.arraySize - 1));
				SerializedProperty newClip = clips.GetArrayElementAtIndex(clips.arraySize - 1);
				newClip.FindPropertyRelative("ClipId").stringValue = $"new clip ({clips.arraySize - 1})";
				newClip.FindPropertyRelative("Volume").floatValue = 1f;
				newClip.FindPropertyRelative("Pitch").floatValue = 1f;
				newClip.FindPropertyRelative("Loop").boolValue = false;
			}

			GUI.color = old;
			rect.y += lineHeight;
		}

		public override void OnInspectorGUI()
		{
			if (duplicatedCategories.Count > 0)
			{
				EditorGUILayout.HelpBox($"There is a duplicated category id: {duplicatedCategories[0]}",
										MessageType.Error);
			}

			if (duplicatedClips.Count > 0)
			{
				EditorGUILayout.HelpBox($"There is a duplicated clip id: {duplicatedClips[0]}", MessageType.Error);
			}

			serializedObject.Update();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("OutputMixer"));
			list.DoLayoutList();
			serializedObject.ApplyModifiedProperties();


			List<string> categories = new List<string>();
			List<string> clips = new List<string>();
			duplicatedCategories.Clear();
			duplicatedClips.Clear();
			foreach (AudioCategory storerCategory in storer.Categories)
			{
				if (categories.Contains(storerCategory.Id))
				{
					duplicatedCategories.Add(storerCategory.Id);
				}
				else
				{
					categories.Add(storerCategory.Id);
				}

				foreach (AudioClipData clip in storerCategory.Clips)
				{
					if (clips.Contains(clip.ClipId))
					{
						duplicatedClips.Add(clip.ClipId);
					}
					else
					{
						clips.Add(clip.ClipId);
					}
				}
			}
		}
	}
}
