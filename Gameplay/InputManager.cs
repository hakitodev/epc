using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    
    public InputSystem_Actions Actions { get; private set; }

    [SerializeField]
    private string _activeMapsDebug;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Actions = new InputSystem_Actions();
        
        ChangeToSingleMap(Actions.Player);
    }

    public void ChangeToSingleMap(InputActionMap mapToEnable)
    {
        foreach (var map in Actions.asset.actionMaps)
        {
            map.Disable();
        }
        mapToEnable.Enable();
        UpdateDebugInfo();
    }

    public void ApplyMultipleMaps(params InputActionMap[] actionMaps)
    {
        Actions.Disable();

        foreach (var map in actionMaps)
        {
            map.Enable();
        }

        UpdateDebugInfo();
    }

    public void SwitchMapByName(string mapName)
    {
        var targetMap = Actions.asset.FindActionMap(mapName);

        if (targetMap != null)
        {
            ChangeToSingleMap(targetMap);
        }
    }

    private void UpdateDebugInfo()
    {
        _activeMapsDebug = "";

        foreach (var map in Actions.asset.actionMaps)
        {
            if (map.enabled)
            {
                _activeMapsDebug += $"[{map.name}] ";
            }
        }
    }

    private void OnEnable() => Actions?.Enable();
    private void OnDisable() => Actions?.Disable();
    private void OnDestroy() => Actions?.Dispose();
}