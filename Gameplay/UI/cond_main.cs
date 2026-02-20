using System.Collections.Generic;
using UnityEngine;

public class cond_main : MonoBehaviour {
    [SerializeField] private ConditionedFields[] fields;
    [SerializeField] private Transform parent;

    private void Start() {
        fields = parent.GetComponentsInChildren<ConditionedFields>();
    }
}