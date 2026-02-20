using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConditionedFields : MonoBehaviour {
    public enum FieldType { Int, Float, Bool, String, Enum, Slider }
    public FieldType fieldType;
    // all
    public InputField in_field;
    // Int
    public int inte;
    // Float
    public float floate;
    // Bool
    public bool toggleValue;
    public string boolLabel = "Enable Feature";
    // String
    public List<string> whiteList = new List<string>();
    
    // Slider
    public Slider slider;
    [Range(0, 100)] public float curValue = 0f;
    public float minValue;
    public float maxValue;

    private void Start() {
        if (slider != null) {
            slider.minValue = minValue;
            slider.maxValue = maxValue;
            slider.onValueChanged.AddListener(OnSliderValidate);
        }
    }
    public void OnSliderValidate(float flt) {
        curValue = flt;
    }
    // private void OnValidate() {
    //     if (fieldType == FieldType.Slider && slider != null) {
    //         slider.value = curValue;
    //     }
    // }
}
