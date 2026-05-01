using System;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Hakito;

public class MapsController : MonoBehaviour
{
    [SerializeField] private GameObject mapPrefab;
    [SerializeField] private RectTransform mapList;
    [SerializeField] private string _mapsPath;
    [SerializeField] private List<Sprite> mapImages = new List<Sprite>();
    [SerializeField]
    private GameObject _loadingPanel;
    [SerializeField]
    private GameObject _deletePanel;
    private string _currentMapName;
    private string _currentMapLoc;
    private string _mapToDelete;
    private static readonly byte[] Key = Encoding.UTF8.GetBytes("7A5f92CpQ0mN10W1uR6qY7pL02XjS4e1"); // 32 bytes
    private static readonly byte[] IV = Encoding.UTF8.GetBytes("nU82XyZpQ1mN90W1");  // 16 bytes
    // public static MapsController Instance { get; private set; }

#region Initialization

    // private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    // private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;
    // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    // public static void Init()  {
    //     if (Instance != null) return;

    //     GameObject _go = new GameObject("MapManager");
    //     Instance = _go.AddComponent<MapsController>();
    //     DontDestroyOnLoad(_go);
    // }

    private void Awake()
    {
        // if (Instance != null && Instance != this) {
        //     Destroy(gameObject);
        //     return;
        // }

        // Instance = this;
        // DontDestroyOnLoad(gameObject);
        _mapsPath = Path.Combine(Application.persistentDataPath, "Maps");
    }

    private void Start()
    {
        if (!Directory.Exists(_mapsPath))
            Directory.CreateDirectory(_mapsPath);

        RefreshMapsList();
    }

#endregion

    public void RefreshMapsList()
    {
        Scene scene = SceneManager.GetActiveScene();

        if (scene.name != "MainMenu") return;

        ClearMapList();

        if (!Directory.Exists(_mapsPath)) return;

        string[] mapFiles = Directory.GetFiles(_mapsPath, "*.mdm");
        
        if (mapFiles.Length == 0) return;

        foreach (string filePath in mapFiles)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);

            if (string.IsNullOrEmpty(fileName)) continue;

            int underscore = fileName.IndexOf('_');

            if (underscore <= 0 || underscore >= fileName.Length - 1) continue;

            string mapVariation = fileName.Substring(0, underscore);
            string mapName = fileName.Substring(underscore + 1);

            if (!ValidateMapFile(filePath))
            {
                Debug.LogWarning($"Invalid or corrupted map file: {fileName}");
                continue;
            }

            LoadMapEntry(mapVariation, mapName);
        }
    }

    private bool ValidateMapFile(string filePath)
    {
        try
        {
            using FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using Aes aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;

            using ICryptoTransform decryptor = aes.CreateDecryptor();
            using CryptoStream cryptoStream = new CryptoStream(fs, decryptor, CryptoStreamMode.Read);
            using GZipStream gz = new GZipStream(cryptoStream, CompressionMode.Decompress);
            using BinaryReader reader = new BinaryReader(gz, Encoding.UTF8);

            string version = reader.ReadString();
            return !string.IsNullOrEmpty(version);
        }
        catch (Exception e)
        {
            Debug.LogError($"File validation error: {e.Message}");

            return false;
        }
    }

    private void LoadMapEntry(string mapVariation, string mapName)
    {
        string loc = VariationToLoaction(mapVariation);
        
        if (!int.TryParse(mapVariation, out int index))
        {
            index = 0;
        }

        if (index < 0 || index >= mapImages.Count) return;

        GameObject mapObject = Instantiate(mapPrefab, mapList);
        MapMeta mapComponent = mapObject.GetComponent<MapMeta>();

        if (mapComponent == null)
        {
            Destroy(mapObject);

            return;
        }

        mapComponent.MapSet(mapName, loc, mapVariation, mapImages[index]);
    }
    private string VariationToLoaction(string _mapVariation)
    {
        return _mapVariation switch
        {
            "000" => "Devs room",
            "001" => "Ground",
            "002" => "Lake",
            _ => "Unknown"
        };
    }

#region Delete
    private void ClearMapList()
    {
        if (mapList == null) return;

        for (int i = mapList.childCount - 1; i >= 0; i--)
        {
            Destroy(mapList.GetChild(i).gameObject);
        }
    }

    public void WantToDeleteMap(string mapToDeleteName)
    {
        _mapToDelete = mapToDeleteName;

        if (_deletePanel != null) _deletePanel.SetActive(true);
    }

    public void DeleteMap()
    {
        DeleteMapAsync().Forget();
    }

    private async UniTaskVoid DeleteMapAsync()
    {
        if (string.IsNullOrEmpty(_mapToDelete)) return;
        
        await UniTask.SwitchToThreadPool();

        string mapFullPath = Path.Combine(_mapsPath, _mapToDelete);

        if (File.Exists(mapFullPath))
        {
            File.Delete(mapFullPath);
        }

        await UniTask.SwitchToMainThread();

        RefreshMapsList();

        if (_deletePanel != null) _deletePanel.SetActive(false);
    }
#endregion

    public void StartMap(string mapLocation, string mapName)
    {
        _currentMapLoc = mapLocation;
        _currentMapName = mapName;

        _loadingPanel.SetActive(true);

        SceneLoading(mapLocation).Forget();
    }

    private async UniTask SceneLoading(string mapLocation)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(1f));

        await Hakito.MainControl.LoadSceneAsync(mapLocation);
        
        if (!string.IsNullOrEmpty(_currentMapName))
        {
            var loader = FindAnyObjectByType<SaveLoadManager>();

            if (loader != null)
            {
                string fileName = $"{SceneManager.GetActiveScene().name}_{_currentMapName}.mdm";
                loader.LoadMap(_currentMapName);
            }
        }
    }
}