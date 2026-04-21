using UnityEngine;
using UnityEngine.UI;
using LightSide;

public class MapMeta : MonoBehaviour
{
    [SerializeField] private UniText MapName;
    [SerializeField] private UniText MapLocation;
    [SerializeField] private string MapVariation;
    [SerializeField] private Image MapIcon;
    
    private MapsController _controller;
    private string _fullMapName;

    private void Start()
    {
        _controller ??= FindAnyObjectByType<MapsController>();
    }

    public void MapLoad()
    {
        _controller ??= FindAnyObjectByType<MapsController>();

        string mapName = MapName != null ? MapName.Text : string.Empty;
        _controller?.StartMap(MapVariation, mapName);
    }
    
    public void MapDelete()
    {
        _controller ??= FindAnyObjectByType<MapsController>();

        _controller?.WantToDeleteMap(_fullMapName);
    }
    
    public void MapSet(string mapName, string mapLocation, string mapVariation, Sprite mapIcon)
    {
        MapName.Text = mapName;
        MapLocation.Text = mapLocation;
        MapIcon.sprite = mapIcon;
        MapVariation = mapVariation;
        _fullMapName = $"{mapVariation}_{mapName}.mdm";
    }
}
