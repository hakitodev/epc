
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(VariableJoystick))]
public class VariableJoystickEditor : JoystickEditor
{
    private SerializedProperty _moveThreshold;
    private SerializedProperty _joystickType;

    protected override void OnEnable()
    {
        base.OnEnable();

        _moveThreshold = serializedObject.FindProperty("_moveThreshold");
        _joystickType = serializedObject.FindProperty("_joystickType");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (_background.objectReferenceValue is RectTransform bgRect)
        {
            if (bgRect.pivot != Center)
            {
                Undo.RecordObject(bgRect, "Setup Variable Joystick Background");
                bgRect.pivot = Center;
                EditorUtility.SetDirty(bgRect);
            }
        }
    }

    protected override void DrawValues()
    {
        base.DrawValues();
        
        EditorGUILayout.PropertyField(_moveThreshold, new GUIContent("Move Threshold", "The distance away from the center input has to be before the joystick begins to move."));
        EditorGUILayout.PropertyField(_joystickType, new GUIContent("Joystick Type", "The type of joystick the variable joystick is current using."));
    }
}