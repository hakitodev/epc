using UnityEngine;
using UnityEngine.UI;
using LightSide;

public class MapMeta : MonoBehaviour
{
    [SerializeField]
    private UniText _nameTxt;
    [SerializeField]
    private UniText _locationTxt;
    [SerializeField]
    private string _variation = "001";
    [SerializeField]
    private Image _icon;
    [SerializeField]
    private MapsController _controller;
    private string _fullName;

    private void Start()
    {
        _controller ??= FindAnyObjectByType<MapsController>();
    }

    public void MapLoad()
    {
        _controller ??= FindAnyObjectByType<MapsController>();

        string mapName = _nameTxt != null ? _nameTxt.Text : string.Empty;
        _controller?.StartMap(_variation, mapName);
    }
    
    public void MapDelete()
    {
        _controller ??= FindAnyObjectByType<MapsController>();

        _controller?.WantToDeleteMap(_fullName);
    }
    
    public void MapSet(string mapName, string mapLocation, string mapVariation, Sprite mapIcon)
    {
        if (_nameTxt != null) _nameTxt.Text = mapName;
        if (_nameTxt != null) _locationTxt.Text = mapLocation;
        if (_icon != null) _icon.sprite = mapIcon;
        _variation = mapVariation;
        _fullName = $"{mapVariation}_{mapName}.mdm";
    }
}
