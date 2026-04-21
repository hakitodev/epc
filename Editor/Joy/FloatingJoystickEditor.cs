using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FloatingJoystick))]
public class FloatingJoystickEditor : JoystickEditor
{
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
                Undo.RecordObject(bgRect, "Setup Floating Joystick Background");
                bgRect.anchorMax = Vector2.zero;
                bgRect.anchorMin = Vector2.zero;
                bgRect.pivot = Center;
                EditorUtility.SetDirty(bgRect);
            }
        }
    }
}