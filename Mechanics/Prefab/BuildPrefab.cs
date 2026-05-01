using System.Collections;
using UnityEngine;

public class BuildPrefab : MonoBehaviour 
{
    private Collider[] _colliders;
    private Renderer[] _renderers;
    private Material[] _materials;
    [SerializeField] 
    private static Material _canMaterial;

    private void Start()
    {
        _colliders = GetComponentsInChildren<Collider>();
        _renderers = GetComponentsInChildren<Renderer>();
        
        _materials = new Material[_renderers.Length];

        for (int i = 0; i < _renderers.Length; i++)
        {
            _materials[i] = _renderers[i].material;
            _renderers[i].material = _canMaterial;
        }

        foreach (var collider in _colliders)
        {
            collider.enabled = false;
        }

        // for (int i = 0; i < _colliders.Length; i++)
        // {
        //     _colliders[i].enabled = false;
        // }
    }

    public bool IsPlaced(Vector3 pos, Vector3 rot)
    {
        transform.position = pos;
        transform.localEulerAngles = rot;
        
        foreach (var collider in _colliders)
        {
            collider.enabled = true;
        }

        // foreach (var renderer in _renderers)
        // {
        //     renderer.material = _materials[0];
        // }
        // for (int i = 0; i < _colliders.Length; i++)
        // {
        //     _colliders[i].enabled = true;
        // }

        for (int i = 0; i < _renderers.Length; i++)
        {
            _renderers[i].material = _materials[i];
        }
        
        Destroy(this);

        return true;
    }
}