using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Text;
using TMPro;

public class PropInfo : MonoBehaviour {
    public Vector3 SelfGravityVector = new Vector3(0, -9.81f, 0);
    public bool CanDelete = true;
    public bool CanGravity = true;
    public bool HasSelfGravity = false;
}

public class SaveLoadManager : MonoBehaviour {
    [SerializeField] private string currentMapName = "NewMap";
    [SerializeField] private Transform propsParent;
    [SerializeField] private string resourcesFolder = "Props";
    
    private Dictionary<short, GameObject> prefabCache = new Dictionary<short, GameObject>();
    [SerializeField] private TMP_InputField mapNameInput;
    private string saveDirectory;

    [SerializeField] private int loadBatchSize = 255;
    public float loadProgress = 0f;
    private static readonly byte[] Key = Encoding.UTF8.GetBytes("7A5f92CpQ0mN10W1uR6qY7pL02XjS4e1"); // 32 bytes
    private static readonly byte[] IV = Encoding.UTF8.GetBytes("nU82XyZpQ1mN90W1");  // 16 bytes

    void Start() => Initialize();

    private void Initialize() {
        saveDirectory = Path.Combine(Application.persistentDataPath, "Maps");
        if (!Directory.Exists(saveDirectory)) Directory.CreateDirectory(saveDirectory);
        CacheAllPrefabs();
    }

    private void CacheAllPrefabs() {
        GameObject[] prefabs = Resources.LoadAll<GameObject>(resourcesFolder);
        foreach (var p in prefabs) {
            if (short.TryParse(p.name, out short prefabId)) {
                prefabCache[prefabId] = p;
            } else {
                Debug.LogWarning($"Failed to parse prefab ID from name: {p.name}");
            }
        }
    }
    public void SaveCurrentMap() {
        if (mapNameInput != null && !string.IsNullOrEmpty(mapNameInput.text)) currentMapName = mapNameInput.text;
        if (string.IsNullOrEmpty(currentMapName)) return;
        
        string fileName = $"{SceneManager.GetActiveScene().name}_{currentMapName.Replace(".mdm", "")}.mdm";
        string filePath = Path.Combine(saveDirectory, fileName);

        try {
            using (FileStream fs = new FileStream(filePath, FileMode.Create)) {
                using (Aes aes = Aes.Create()) {
                    aes.Key = Key;
                    aes.IV = IV;

                    using (ICryptoTransform encryptor = aes.CreateEncryptor())
                    using (CryptoStream cryptoStream = new CryptoStream(fs, encryptor, CryptoStreamMode.Write))
                    using (GZipStream gz = new GZipStream(cryptoStream, System.IO.Compression.CompressionLevel.Optimal))
                    using (BinaryWriter writer = new BinaryWriter(gz, Encoding.UTF8)) {
                        // writer.Write(SceneManager.GetActiveScene().name);
                        // writer.Write(currentMapName);
                        writer.Write("0.0.1");
                        writer.Write(propsParent.childCount);
                        writer.Write((short)0);

                        int processed = 0;
                        foreach (Transform child in propsParent) {
                            GameObject obj = child.gameObject;
                            if (obj == null) continue;
                                        
                            string cleanName = obj.name.Replace("(Clone)", "").Trim();
                            if (!short.TryParse(cleanName, out short prefabId)) {
                                Debug.LogWarning($"Failed to parse prefab ID from object name: {obj.name}, skipping...");
                                continue;
                            }
                            writer.Write(prefabId);

                            writer.Write(obj.transform.position.x);
                            writer.Write(obj.transform.position.y);
                            writer.Write(obj.transform.position.z);

                            writer.Write(obj.transform.eulerAngles.x);
                            writer.Write(obj.transform.eulerAngles.y);
                            writer.Write(obj.transform.eulerAngles.z);

                            writer.Write(obj.transform.localScale.x);
                            writer.Write(obj.transform.localScale.y);
                            writer.Write(obj.transform.localScale.z);

                            PropInfo info = obj.GetComponent<PropInfo>();
                            if (info != null) {
                                writer.Write(info.SelfGravityVector.x);
                                writer.Write(info.SelfGravityVector.y);
                                writer.Write(info.SelfGravityVector.z);
                                writer.Write(info.CanDelete);
                                writer.Write(info.CanGravity);
                                writer.Write(info.HasSelfGravity);
                            } else {
                                writer.Write(0f);
                                writer.Write(-9.81f);
                                writer.Write(0f);
                                writer.Write(true);
                                writer.Write(true);
                                writer.Write(false);
                            }

                            Rigidbody rb = obj.GetComponent<Rigidbody>();
                            writer.Write(rb != null ? rb.isKinematic : false);
                            writer.Write(rb != null ? rb.useGravity : true);

                            processed++;
                        }

                        Debug.Log($"Saved Objects: {processed}, File: {filePath}");

                        writer.Flush();
                        gz.Flush();
                        
                        Debug.Log($"[Save] Успешно зашифровано объектов: {processed}. Путь: {filePath}");
                    }
                }
            }
        }
        catch (Exception e) {
            Debug.LogError($"Save Error: {e.Message}");
        }
    }

    public void LoadMap(string mapFileName) {
        if (string.IsNullOrEmpty(mapFileName)) return;
        
        string fileName = $"{SceneManager.GetActiveScene().name}_{mapFileName.Replace(".mdm", "")}.mdm";
        LoadMapAsync(fileName).Forget();
    }
    
    private async UniTaskVoid LoadMapAsync(string mapFileName) {
        string filePath = Path.Combine(saveDirectory, mapFileName);
        
        if (!File.Exists(filePath)) {
            Debug.LogError($"File Didn't Exist: {filePath}");
            return;
        }

        ClearAllProps();
        List<PropMeta> allData = new List<PropMeta>();

        try {
            await UniTask.SwitchToThreadPool();
            
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read)) {
                using (Aes aes = Aes.Create()) {
                    aes.Key = Key;
                    aes.IV = IV;

                    using (ICryptoTransform decryptor = aes.CreateDecryptor())
                    using (CryptoStream cryptoStream = new CryptoStream(fs, decryptor, CryptoStreamMode.Read))
                    using (GZipStream gz = new GZipStream(cryptoStream, CompressionMode.Decompress))
                    using (BinaryReader reader = new BinaryReader(gz, Encoding.UTF8)) {
                        string version = reader.ReadString();
                        int totalCount = reader.ReadInt32();
                        short timeOfDay = reader.ReadInt16(); 

                        allData.Capacity = totalCount;

                        for (int i = 0; i < totalCount; i++) {
                            PropMeta data = new PropMeta {
                                PrefabName = reader.ReadInt16(),

                                Position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
                                Rotation = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
                                Scale = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),

                                SelfGravityVector = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
                                CanDelete = reader.ReadBoolean(),
                                CanGravity = reader.ReadBoolean(),
                                HasSelfGravity = reader.ReadBoolean(),

                                IsKinematic = reader.ReadBoolean(),
                                HasGravity = reader.ReadBoolean()
                            };

                            allData.Add(data);
                        }
                    }
                }
            }
            
            await UniTask.SwitchToMainThread();
        }
        catch (Exception e) {
            Debug.LogError($"File Load/Decrypt Error: {e.Message}");
            return;
        }

        loadProgress = 0f;
        int loaded = 0;
        while (loaded < allData.Count) {
            int batchCount = Mathf.Min(loadBatchSize, allData.Count - loaded);
            for (int i = 0; i < batchCount; i++) {
                CreatePropFromData(allData[loaded]);
                loaded++;
            }
            loadProgress = (float)loaded / allData.Count;
            await UniTask.Yield();
        }

        allData.Clear();
        Debug.Log($"Successfully decrypted and loaded: {propsParent.childCount} objects");
    }

    public GameObject CreateNewProp(short _prefabName, Vector3 _pos, Quaternion _rot) {
        if (!prefabCache.TryGetValue(_prefabName, out GameObject prefab)) {
            prefab = Resources.Load<GameObject>($"{resourcesFolder}/{_prefabName.ToString()}");
            if (prefab == null) return null;
            prefabCache[_prefabName] = prefab;
        }
        
        GameObject obj = Instantiate(prefab, _pos, _rot, propsParent);
        obj.name = _prefabName.ToString();
        
        PropInfo info = obj.AddComponent<PropInfo>();

        RemoveBuildComponent(obj);
        return obj;
    }

    private void CreatePropFromData(PropMeta data) {
        if (!prefabCache.TryGetValue(data.PrefabName, out GameObject prefab)) return;
        
        GameObject obj = Instantiate(prefab, propsParent);
        obj.name = data.PrefabName.ToString();
        data.ApplyToGameObject(obj);
        PropInfo info = obj.AddComponent<PropInfo>();
        info.SelfGravityVector = data.SelfGravityVector;
        info.CanDelete = data.CanDelete;
        info.CanGravity = data.CanGravity;
        info.HasSelfGravity = data.HasSelfGravity;
        
        RemoveBuildComponent(obj);
    }
    public void EditProp(GameObject obj, PropMeta editData) {
        if (obj == null || !IsPropContains(obj)) {
            Debug.LogWarning("No Needed GO");
            return;
        }

        obj.transform.position = editData.Position;
        obj.transform.eulerAngles = editData.Rotation;
        obj.transform.localScale = editData.Scale;

        if (obj.TryGetComponent<Rigidbody>(out Rigidbody rb)) {
            rb.isKinematic = editData.IsKinematic;
            rb.useGravity = editData.HasGravity;
        }

        PropInfo info = obj.GetComponent<PropInfo>();
        if (info != null) {
            info.SelfGravityVector = editData.SelfGravityVector;
            info.CanDelete = editData.CanDelete;
            info.CanGravity = editData.CanGravity;
            info.HasSelfGravity = editData.HasSelfGravity;
        }

        Debug.Log($"GO {obj.name} Edited");
    }

    public PropMeta GetCurrentPropData(GameObject obj) {
        if (obj == null || !IsPropContains(obj)) return null;

        string cleanName = obj.name.Replace("(Clone)", "").Trim();
        if (!short.TryParse(cleanName, out short prefabId)) {
            Debug.LogWarning($"Failed to parse prefab ID from object name: {obj.name}");
            return null;
        }

        PropMeta data = new PropMeta {
            PrefabName = prefabId,
            Position = obj.transform.position,
            Rotation = obj.transform.eulerAngles,
            Scale = obj.transform.localScale
        };

        if (obj.TryGetComponent<Rigidbody>(out Rigidbody rb)) {
            data.IsKinematic = rb.isKinematic;
            data.HasGravity = rb.useGravity;
        }

        PropInfo info = obj.GetComponent<PropInfo>();
        if (info != null) {
            data.SelfGravityVector = info.SelfGravityVector;
            data.CanDelete = info.CanDelete;
            data.CanGravity = info.CanGravity;
            data.HasSelfGravity = info.HasSelfGravity;
        }

        return data;
    }

    public void RemoveProp(GameObject obj) {
        if (obj != null && obj.transform.parent == propsParent) {
            Destroy(obj);
        }
    }
    public void ClearAllProps() {
        while (propsParent.childCount > 0) {
            DestroyImmediate(propsParent.GetChild(0).gameObject);
        }
    }

    private void RemoveBuildComponent(GameObject obj) {
        if (obj.TryGetComponent<BuildPrefab>(out BuildPrefab bp)) Destroy(bp);
    }

    public bool IsPropContains(GameObject obj) => obj != null && obj.transform.parent == propsParent;

    private void OnApplicationQuit() => SaveCurrentMap();
}