using UnityEngine;

public class ConditionedFieldsE : MonoBehaviour
{
    public enum FieldType { Int, Float, Bool, String, Slider }
    public FieldType fieldType;
    
    // Поля для Bool
    public bool toggleValue;
    public string boolLabel = "Enable Feature";
    
    // Поля для Slider
    [Range(0, 100)] public float sliderValue = 50f;
    public float minValue = 0f;
    public float maxValue = 100f;
}