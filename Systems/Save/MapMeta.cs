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
        // StartMap ожидает: mapLocation (имя сцены Unity) и mapName (имя карты без расширения)
        // MapLocation.text содержит локацию (например "Devs room"), которая должна быть именем сцены Unity
        // MapName.text содержит имя карты без расширения и префикса
        string mapName = MapName != null ? MapName.text : string.Empty;
        string mapLocation = MapLocation != null ? MapLocation.text : MapVariation;
        // Используем MapLocation.text как имя сцены (если оно установлено), иначе fallback на MapVariation
        _cachedController?.StartMap(mapLocation, mapName);
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
