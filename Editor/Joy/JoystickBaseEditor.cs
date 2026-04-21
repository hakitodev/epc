using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(JoystickBase), true)]
public class JoystickEditor : Editor
{
    private SerializedProperty _handleRange;
    private SerializedProperty _deadZone;
    private SerializedProperty _axisOptions;
    private SerializedProperty _snapX;
    private SerializedProperty _snapY;
    protected SerializedProperty _background;
    private SerializedProperty _handle;
    protected static readonly Vector2 Center = new Vector2(0.5f, 0.5f);

    protected virtual void OnEnable()
    {
        _handleRange = serializedObject.FindProperty("_handleRange");
        _deadZone = serializedObject.FindProperty("_deadZone");
        _axisOptions = serializedObject.FindProperty("_axisOptions");
        _snapX = serializedObject.FindProperty("_snapX");
        _snapY = serializedObject.FindProperty("_snapY");
        _background = serializedObject.FindProperty("_background");
        _handle = serializedObject.FindProperty("_handle");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawValues();
        EditorGUILayout.Space();
        DrawComponents();
        serializedObject.ApplyModifiedProperties();

        if (_handle.objectReferenceValue is RectTransform handleRect)
        {
            bool needsFix = handleRect.anchorMax != Center || 
                            handleRect.anchorMin != Center || 
                            handleRect.pivot != Center || 
                            handleRect.anchoredPosition != Vector2.zero;

            if (needsFix)
            {
                Undo.RecordObject(handleRect, "Setup Joystick Handle");
                handleRect.anchorMax = Center;
                handleRect.anchorMin = Center;
                handleRect.pivot = Center;
                handleRect.anchoredPosition = Vector2.zero;
                EditorUtility.SetDirty(handleRect);
            }
        }
    }

    protected virtual void DrawValues() {
        EditorGUILayout.PropertyField(_handleRange, new GUIContent("Handle Range", "The distance the visual handle can move from the center of the joystick."));
        EditorGUILayout.PropertyField(_deadZone, new GUIContent("Dead Zone", "The distance away from the center input has to be before registering."));
        EditorGUILayout.PropertyField(_axisOptions, new GUIContent("Axis Options", "Which axes the joystick uses."));
        EditorGUILayout.PropertyField(_snapX, new GUIContent("Snap X", "Snap the horizontal input to a whole value."));
        EditorGUILayout.PropertyField(_snapY, new GUIContent("Snap Y", "Snap the vertical input to a whole value."));
    }

    protected virtual void DrawComponents() {
        EditorGUILayout.ObjectField(_background, new GUIContent("Background", "The background's RectTransform component."));
        EditorGUILayout.ObjectField(_handle, new GUIContent("Handle", "The handle's RectTransform component."));
    }
}