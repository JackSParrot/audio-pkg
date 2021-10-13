using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(AudioClipsStorer))]
public class AudioClipsStorerCustomEditor : Editor
{
    private AudioClipsStorer _storer;
    private List<string>     _duplicatedCategories = new List<string>();
    private List<string>     _duplicatedClips      = new List<string>();

    private bool            _foldout = false;
    private ReorderableList _list;

    private void OnEnable()
    {
        _storer = (AudioClipsStorer) target;
        _list = new ReorderableList(serializedObject, serializedObject.FindProperty("_categories"), true, true,
            true, true);
        //_list.drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, "Categories");
        //_list.drawElementCallback = DrawElementCallback;
        //_list.elementHeightCallback = ElementHeightCallback;
    }

    private float ElementHeightCallback(int index)
    {
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float retVal = lineHeight;

        SerializedProperty element = _list.serializedProperty.GetArrayElementAtIndex(index);
        SerializedObject propertyObject = new SerializedObject(element.objectReferenceValue);
        SerializedProperty propertyIterator = propertyObject.GetIterator();

        while (propertyIterator.NextVisible(true))
        {
            retVal += lineHeight;
        }

        return retVal;
    }

    private void DrawElementCallback(Rect rect, int index, bool isactive, bool isfocused)
    {
        float lineHeight = EditorGUIUtility.singleLineHeight;

        SerializedProperty element = _list.serializedProperty.GetArrayElementAtIndex(index);
        SerializedObject propertyObject = new SerializedObject(element.objectReferenceValue);

        rect.height = lineHeight;
        EditorGUI.LabelField(rect, element.FindPropertyRelative("Id").stringValue);
        rect.y += lineHeight;

        SerializedProperty propertyIterator = propertyObject.GetIterator();
        while (propertyIterator.NextVisible(true))
        {
            EditorGUI.PropertyField(rect, propertyIterator);
            rect.y += lineHeight;
        }

        EditorGUI.PropertyField(rect, _list.serializedProperty.GetArrayElementAtIndex(index));
    }

    public override void OnInspectorGUI()
    {
        if (_duplicatedCategories.Count > 0)
        {
            EditorGUILayout.HelpBox($"There is a duplicated category id: {_duplicatedCategories[0]}",
                MessageType.Error);
        }

        if (_duplicatedClips.Count > 0)
        {
            EditorGUILayout.HelpBox($"There is a duplicated clip id: {_duplicatedClips[0]}", MessageType.Error);
        }

        base.OnInspectorGUI();
        //serializedObject.Update();
        //_list.DoLayoutList();
        //serializedObject.ApplyModifiedProperties();


        var categories = new List<string>();
        var clips = new List<string>();
        _duplicatedCategories.Clear();
        _duplicatedClips.Clear();
        foreach (AudioCategory storerCategory in _storer.Categories)
        {
            if (categories.Contains(storerCategory.Id))
            {
                _duplicatedCategories.Add(storerCategory.Id);
            }
            else
            {
                categories.Add(storerCategory.Id);
            }

            foreach (AudioClipData clip in storerCategory.Clips)
            {
                if (clips.Contains(clip.ClipId))
                {
                    _duplicatedClips.Add(clip.ClipId);
                }
                else
                {
                    clips.Add(clip.ClipId);
                }
            }
        }
    }
}