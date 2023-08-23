using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace JackSParrot.Services.Audio.Editor
{
    [CustomEditor(typeof(AudioClipsCategoryLoader))]
    public class AudioClipsCategoryLoaderEditor : UnityEditor.Editor
    {
        private List<string>             categoryIds = new List<string>();
        private AudioClipsCategoryLoader castedTarget;
        private int                      chosen  = 0;
        private bool                     foldOut = true;

        private void OnEnable()
        {
            castedTarget = (AudioClipsCategoryLoader)target;
            AudioService audioServiceSo = LoadAudioManager();
            foreach (AudioCategory category in audioServiceSo.Categories)
            {
                categoryIds.Add(category.Id);
            }

            foreach (string category in castedTarget.CategoriesToLoad)
            {
                categoryIds.Remove(category);
            }
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("serviceSo"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("loadOnEnable"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("unloadOnDisable"));
            foldOut = EditorGUILayout.BeginFoldoutHeaderGroup(foldOut, "Categories");
            SerializedProperty categories = serializedObject.FindProperty("categoriesToLoad");
            Color originalColor = GUI.color;
            if (foldOut)
            {
                EditorGUI.indentLevel++;
                if (categories.arraySize < 1)
                {
                    EditorGUILayout.LabelField("EMPTY");
                }

                for (int i = 0; i < categories.arraySize; ++i)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(categories.GetArrayElementAtIndex(i).stringValue);
                    EditorGUILayout.Space();
                    GUI.color = Color.red;
                    if (GUILayout.Button("-"))
                    {
                        categoryIds.Add(categories.GetArrayElementAtIndex(i).stringValue);
                        categories.DeleteArrayElementAtIndex(i--);
                    }

                    GUI.color = originalColor;
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUI.indentLevel--;
                EditorGUILayout.EndFoldoutHeaderGroup();
            }

            if (categoryIds.Count > 0)
            {
                EditorGUILayout.LabelField("Add New Category", EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();
                chosen = EditorGUILayout.Popup(chosen, categoryIds.ToArray());
                GUI.color = Color.green;
                if (GUILayout.Button("+"))
                {
                    categories.InsertArrayElementAtIndex(Mathf.Max(0, categories.arraySize - 1));
                    categories.GetArrayElementAtIndex(categories.arraySize - 1).stringValue = categoryIds[chosen];
                    categoryIds.Remove(categoryIds[chosen]);
                }

                GUI.color = originalColor;
                EditorGUILayout.EndHorizontal();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private AudioService LoadAudioManager()
        {
            string[] services = AssetDatabase.FindAssets($"t: {nameof(AudioService)}");
            if (services.Length > 1)
            {
                Debug.LogError($"There is a duplicate {nameof(AudioService)}");
                return null;
            }

            if (services.Length > 0)
            {
                return AssetDatabase.LoadAssetAtPath<AudioService>(AssetDatabase.GUIDToAssetPath(services[0]));
            }

            return null;
        }
    }
}