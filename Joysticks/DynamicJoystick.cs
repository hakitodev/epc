using UnityEngine;
using UnityEngine.EventSystems;

public class DynamicJoystick : JoystickBase
{
    [SerializeField] private float _moveThreshold = 1f;

    public override void OnPointerDown(PointerEventData eventData)
    {
        _background.anchoredPosition = ScreenToAnchored(eventData.position);

        base.OnPointerDown(eventData);
    }

    protected override void HandleInput(float magnitude, Vector2 normalized)
    {
        if (magnitude > _moveThreshold)
        {
            Vector2 offset = normalized * (magnitude - _moveThreshold) * _radius;
            _background.anchoredPosition += offset;
        }
        
        base.HandleInput(magnitude, normalized);
    }
}