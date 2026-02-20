using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MapMeta : MonoBehaviour {
    [SerializeField] private TMP_Text MapName;
    [SerializeField] private TMP_Text MapLocation;
    [SerializeField] private string MapVariation;
    [SerializeField] private Image MapIcon;
    
    private MapsController _cachedController;
    private string _fullMapName;

    private void Awake() {
        _cachedController = FindAnyObjectByType<MapsController>();
    }

    public void MapLoad() {
        if (_cachedController == null) {
            _cachedController = FindAnyObjectByType<MapsController>();
        }
        _cachedController?.StartMap(MapVariation, _fullMapName);
    }
    
    public void MapDelete() {
        if (_cachedController == null) {
            _cachedController = FindAnyObjectByType<MapsController>();
        }
        _cachedController?.WantToDeleteMap(_fullMapName);
    }
    
    public void MapSet(string mapName, string mapLocation, string mapVariation, Sprite mapIcon) {
        if (MapName != null) MapName.text = mapName;
        if (MapLocation != null) MapLocation.text = mapLocation;
        if (MapIcon != null) MapIcon.sprite = mapIcon;
        MapVariation = mapVariation;
        _fullMapName = $"{mapVariation}_{mapName}.mdm";
    }
}
