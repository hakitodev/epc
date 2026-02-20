// using System;
// using System.IO;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.SceneManagement;
// using EPC;

// public class maps_con : MonoBehaviour {
//     [SerializeField] private GameObject map_prefab;
//     [SerializeField] private GameObject map_list;
//     [SerializeField] private EPC.parser parser;
//     private string path;
//     public string _map_name;
//     public string _map_loc;
//     public string map_del_name;
//     public GameObject panel_del;

//     private void Awake() => DontDestroyOnLoad(gameObject);

//     private void Start()
//     {
//         parser = GameObject.FindAnyObjectByType<parser>();
//         path = pathComb(Application.persistentDataPath, "Maps");
//         if (!Directory.Exists(path))
//             Directory.CreateDirectory(path);
//         maps_add();
//     }

//     private void maps_add() {
//         unRendMaps();
//         var Dirs = Directory.GetFiles(path, "*.mdm");
//         if (Dirs.Length != 0)
//         {
//             foreach (var file in Dirs)
//             {
//                 string content = File.ReadAllText(file);
//                 string resol = parser.decode_text(content.Substring(0, 14), 6);
//                 if (resol == "canBeREadEblEn") map_load(Path.GetFileName(file));
//             }
//         }
//     }

//     private void map_load(string map_name) {
//         var m_obj = Instantiate(map_prefab, map_list);
//         var m_com = m_obj.GetComponent<map_com>();
        
//         string m_name = Path.GetFileNameWithoutExtension(map_name);
//         string cleanName = m_name.Replace("_*", "");
//         string prefix = cleanName.Substring(0, 3);
//         Debug.Log(prefix);
        
//         m_com.map_name.text = cleanName.Substring(4);
//         m_com.map_loc.text = prefix == "000" ? "Devs room" : prefix == "001" ? "Ground" : "Unknown";
//         m_com.m_loc = $"00{int.Parse(prefix)}";
        
//         // _map_name = map_name;
//         // _map_loc = m_com.m_loc;
//     }
//     private void unRendMaps() {
//         for (int i = 0; i < map_list.childCount; i++) {
//             Destroy(map_list.GetChild(i).gameObject);
//         }
//     }

//     public void map_delete() {
//         StartCoroutine(panelOffer());
//     }
//     IEnumerator panelOffer() {
//         yield return StartCoroutine(fileDel());
//         maps_add();
//         panel_del.SetActive(false);
//     }
//     IEnumerator fileDel() {
//         if (map_del_name != "") File.Delete(pathComb(path, map_del_name));
//         yield return null;
//     }

//     private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
//         if (scene.name == "MainMenu" && map_list == null) Destroy(this.gameObject);
//         if (scene.name == _map_loc && _map_name != null) {
//             var loader = FindFirstObjectByType<SaveLoadManager>();
//             // loader.load_map(Path.Combine(path, $"{_map_loc}_{_map_name}.mdm"));
//         }
//     }
//     private string pathComb(string path, string name) {
//         string pth = Path.Combine(path, name);
//         return pth;
//     }

//     public void map_start(string m_loc, string m_name)
//     {
//         _map_loc = m_loc;
//         _map_name = m_name;
//         FindFirstObjectByType<MainControl>().scene_load(m_loc);
//     }

//     void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
//     void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;
// }
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
using TMPro;


public class MapsController : MonoBehaviour {
    [SerializeField] private GameObject mapPrefab;
    [SerializeField] private RectTransform mapList;
    // [SerializeField] private TMP_InputField passwordField;
    [SerializeField] private string mapsPath;
    [SerializeField] private List<Sprite> mapImages = new List<Sprite>();
    private string currentMapName;
    private string currentMapLoc;
    private string mapToDelete;
    private GameObject deletePanel;
    private static readonly byte[] Key = Encoding.UTF8.GetBytes("7A5f92CpQ0mN10W1uR6qY7pL02XjS4e1"); // 32 bytes
    private static readonly byte[] IV = Encoding.UTF8.GetBytes("nU82XyZpQ1mN90W1");  // 16 bytes
    // public static MapsController Instance { get; private set; }

    #region Initialization
        private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
        private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            if (scene.name == currentMapLoc && !string.IsNullOrEmpty(currentMapName)) {
                var loader = FindAnyObjectByType<SaveLoadManager>();
                if (loader != null) {
                    // Формат имени файла: SceneName_MapName.mdm
                    string fileName = $"{SceneManager.GetActiveScene().name}_{currentMapName}.mdm";
                    loader.LoadMap(currentMapName);
                }
            }
        }
        // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        // public static void Init()  {
        //     if (Instance != null) return;

        //     GameObject _go = new GameObject("MapManager");
        //     Instance = _go.AddComponent<MapsController>();
        //     DontDestroyOnLoad(_go);
        // }
        private void Awake() {
            // if (Instance != null && Instance != this) {
            //     Destroy(gameObject);
            //     return;
            // }

            // Instance = this;
            // DontDestroyOnLoad(gameObject);
            mapsPath = Path.Combine(Application.persistentDataPath, "Maps");
        }

        private void Start() {
            if (!Directory.Exists(mapsPath)) {
                Directory.CreateDirectory(mapsPath);
            }
            RefreshMapsList();
        }
    #endregion
    public void RefreshMapsList() {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.name != "MainMenu") return;

        ClearMapList();

        if (!Directory.Exists(mapsPath)) return;

        string[] mapFiles = Directory.GetFiles(mapsPath, "*.mdm");
        if (mapFiles.Length == 0) return;

        foreach (string filePath in mapFiles) {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            if (string.IsNullOrEmpty(fileName)) continue;

            int underscore = fileName.IndexOf('_');
            if (underscore <= 0 || underscore >= fileName.Length - 1) continue;

            string mapVariation = fileName.Substring(0, underscore);
            string mapName = fileName.Substring(underscore + 1);

            // Проверяем версию файла только для валидации
            if (!ValidateMapFile(filePath)) {
                Debug.LogWarning($"Invalid or corrupted map file: {fileName}");
                continue;
            }

            LoadMapEntry(mapVariation, mapName);
        }
    }

    private bool ValidateMapFile(string filePath) {
        try {
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
        catch (Exception e) {
            Debug.LogError($"File validation error: {e.Message}");
            return false;
        }
    }

    private void LoadMapEntry(string mapVariation, string mapName) {
        string loc = VariationToLoaction(mapVariation);
        
        if (!int.TryParse(mapVariation, out int index)) {
            index = 0;
        } 
        if (index < 0 || index >= mapImages.Count) return;

        GameObject mapObject = Instantiate(mapPrefab, mapList);
        MapMeta mapComponent = mapObject.GetComponent<MapMeta>();
        if (mapComponent == null) {
            Destroy(mapObject);
            return;
        }

        mapComponent.MapSet(mapName, loc, mapVariation, mapImages[index]);
    }
    private string VariationToLoaction(string _mapVariation) {
        return _mapVariation switch {
            "000" => "Devs room",
            "001" => "Ground",
            "002" => "Lake",
            _ => "Unknown"
        };
    }

    #region Delete
        private void ClearMapList() {
            if (mapList == null) return;
            for (int i = mapList.childCount - 1; i >= 0; i--) {
                Destroy(mapList.GetChild(i).gameObject);
            }
        }
        public void WantToDeleteMap(string mapToDeleteName) {
            mapToDelete = mapToDeleteName;
            if (deletePanel != null) deletePanel.SetActive(true);
        }
        public void DeleteMap() {
            DeleteMapAsync().Forget();
        }
        private async UniTaskVoid DeleteMapAsync() {
            await DeleteFileAsync();
            RefreshMapsList();
            if (deletePanel != null) deletePanel.SetActive(false);
        }
        private async UniTask DeleteFileAsync() {
            if (string.IsNullOrEmpty(mapToDelete)) return;
            
            await UniTask.SwitchToThreadPool();
            string mapFullPath = Path.Combine(mapsPath, mapToDelete);
            if (File.Exists(mapFullPath)) {
                File.Delete(mapFullPath);
            }
            await UniTask.SwitchToMainThread();
        }
    #endregion
    public void StartMap(string mapLocation, string mapName) {
        currentMapLoc = mapLocation;
        currentMapName = mapName;
        // Загружаем сцену по локации, а не по имени карты
        SceneManager.LoadScene(mapLocation);
    }
}