using UnityEngine;

public enum InputType {
    Player,
    Vehicle,
    Game,
    UI
}

public class InputManager : MonoBehaviour {
    public static InputManager Instance { get; private set; }
    public InputSystem_Actions actions;
    private InputType _typeCurrent = InputType.Player;
    private InputType _typePrevious = InputType.Player;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Init()  {
        if (Instance != null) return;

        GameObject go = new GameObject("InputManager");
        Instance = go.AddComponent<InputManager>();
        DontDestroyOnLoad(go);
    }
    private void Awake()  {
        if (Instance != null && Instance != this)  {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        actions = new InputSystem_Actions();
        actions.Enable();

        ChangeInputMap(_typeCurrent);
    }
    private void OnDestroy() {
        if (Instance == this) {
            actions.Disable();
            actions = null;
            Instance = null;
        }
    }
    
    public void RevertToPrevios() {
        ChangeInputMap(_typePrevious);
    }

    public void ChangeInputMap(InputType _InputType)  {
        if (_typeCurrent == _InputType) return;

        _typePrevious = _typeCurrent;
        _typeCurrent = _InputType;

        actions.Player.Disable();
        actions.Vehicle.Disable();
        actions.Game.Disable();
        actions.UI.Disable();

        switch (_InputType) {
            case InputType.Player:
                actions.Player.Enable();
                break;
            case InputType.Vehicle:
                actions.Vehicle.Enable();
                break;
            case InputType.Game:
                actions.Game.Enable();
                break;
            case InputType.UI:
                actions.UI.Enable();
                break;
        }
    }
}