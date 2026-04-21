using UnityEngine;
using UnityEngine.EventSystems;

public enum JoystickType { Fixed, Floating, Dynamic }

public class VariableJoystick : JoystickBase
{
    [SerializeField] private float _moveThreshold = 1f;
    [SerializeField] private JoystickType _joystickType = JoystickType.Fixed;

    private Vector2 _fixedPosition;

    public float MoveThreshold { get => _moveThreshold; set => _moveThreshold = Mathf.Abs(value); }

    protected override void Start()
    {
        base.Start();

        _fixedPosition = _background.anchoredPosition;

        SetMode(_joystickType);
    }

    public void SetMode(JoystickType type)
    {
        _joystickType = type;
        bool isFixed = _joystickType == JoystickType.Fixed;
        
        _background.gameObject.SetActive(isFixed || type == JoystickType.Fixed); 

        if (isFixed) _background.anchoredPosition = _fixedPosition;
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (_joystickType != JoystickType.Fixed)
        {
            _background.anchoredPosition = ScreenToAnchored(eventData.position);
            _background.gameObject.SetActive(true);
        }

        base.OnPointerDown(eventData);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        if (_joystickType != JoystickType.Fixed)
            _background.gameObject.SetActive(false);

        base.OnPointerUp(eventData);
    }

    protected override void HandleInput(float magnitude, Vector2 normalized)
    {
        if (_joystickType == JoystickType.Dynamic && magnitude > _moveThreshold)
        {
            Vector2 offset = normalized * (magnitude - _moveThreshold) * _radius;
            _background.anchoredPosition += offset;
        }
        
        base.HandleInput(magnitude, normalized);
    }
}