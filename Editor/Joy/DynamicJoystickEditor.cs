using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DynamicJoystick))]
public class DynamicJoystickEditor : JoystickEditor
{
    private SerializedProperty _moveThreshold;

    protected override void OnEnable()
    {
        base.OnEnable();

        _moveThreshold = serializedObject.FindProperty("_moveThreshold");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (_background.objectReferenceValue is RectTransform bgRect)
        {
            bool needsFix = bgRect.anchorMax != Vector2.zero || 
                            bgRect.anchorMin != Vector2.zero || 
                            bgRect.pivot != Center;

            if (needsFix)
            {
                Undo.RecordObject(bgRect, "Setup Dynamic Joystick Background");
                bgRect.anchorMax = Vector2.zero;
                bgRect.anchorMin = Vector2.zero;
                bgRect.pivot = Center;
                EditorUtility.SetDirty(bgRect);
            }
        }
    }

    protected override void DrawValues()
    {
        base.DrawValues();

        EditorGUILayout.PropertyField(_moveThreshold, new GUIContent("Move Threshold", "The distance away from the center input has to be before the joystick begins to move."));
    }
}