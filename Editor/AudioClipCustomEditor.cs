using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ClipId))]
public class AudioClipCustomEditor : PropertyDrawer
{
    private AudioClipsStorer _clips;
    private string[]         _ids;
    int                      _selected = 0;

    private void Initialize(string currentValue)
    {
        var assets = AssetDatabase.FindAssets("t:AudioClipsStorer");
        if (assets.Length < 1)
        {
            Debug.LogError("No AudioClipStorer created");
            return;
        }

        if (assets.Length > 1)
        {
            Debug.LogError("There are more than 1 AudioClipStorer created");
            return;
        }

        _clips = AssetDatabase.LoadAssetAtPath<AudioClipsStorer>(AssetDatabase.GUIDToAssetPath(assets[0]));
        var allClips = _clips.GetAllClips().ToArray();
        _ids = new string[allClips.Length + 1];
        _ids[0] = "NONE";
        for (int i = 0; i < allClips.Length; ++i)
        {
            _ids[i + 1] = allClips[i];
        }

        _selected = 0;
        if (!string.IsNullOrEmpty(currentValue))
        {
            for (int i = 0; i < _ids.Length; ++i)
            {
                if (_ids[i] == currentValue)
                {
                    _selected = i;
                    i = 99999;
                }
            }
        }
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty clipId = property.FindPropertyRelative("Id");
        if (_clips == null)
        {
            Initialize(clipId.stringValue);
        }

        int selected = EditorGUI.Popup(EditorGUI.PrefixLabel(position, label), _selected, _ids);

        if (selected != _selected)
        {
            _selected = selected;
            clipId.stringValue = selected == 0 ? string.Empty : _ids[selected];
        }
    }
}