using UnityEngine;

public class PropSelector : MonoBehaviour
{
    [SerializeField]
    private GameObject _prop;
    [SerializeField]
    private PlayerAssembler PA;

    public void ApplyProp()
    {
        PA ??= GameObject.FindAnyObjectByType<PlayerAssembler>();

        PA.PropSet(_prop);
    }
}
