using UnityEngine;

public class PropSelector : MonoBehaviour {
    [SerializeField] private GameObject Prop;
    [SerializeField] private PlayerAssembler PA;

    public void ApplyProp() {
        if (PA == null) PA = GameObject.FindAnyObjectByType<PlayerAssembler>();
        PA.PropSet(Prop);
    }
}
