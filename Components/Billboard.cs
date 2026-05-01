using UnityEngine;

public class Billboard : MonoBehaviour
{
    [SerializeField]
    private Transform point, _transform;
    private readonly Vector3 VECTOR_UP = Vector3.up;

    private void Start()
    {
        point ??= Camera.main.transform;
        _transform ??= transform;
    }

    private void LateUpdate()
    {
        _transform?.LookAt(point, VECTOR_UP);
    }
}
