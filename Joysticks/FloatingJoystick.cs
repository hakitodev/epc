using UnityEngine;
using UnityEngine.EventSystems;
using PrimeTween;

public class FloatingJoystick : JoystickBase
{
    [SerializeField] private float _appearanceDuration = 0.15f;
    private Tween _fadeTween;

    protected override void Awake()
    {
        base.Awake();
        
        if (_background != null) _background.localScale = Vector3.zero;
    }

    protected override void OnDisable()
    {
        _fadeTween.Stop();

        base.OnDisable();
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        _background.anchoredPosition = ScreenToAnchored(eventData.position);
        
        _fadeTween.Stop();
        _fadeTween = Tween.Scale(_background, Vector3.one, _appearanceDuration, ease: Ease.OutBack);
        
        base.OnPointerDown(eventData);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        
        _fadeTween.Stop();
        _fadeTween = Tween.Scale(_background, Vector3.zero, _appearanceDuration, ease: Ease.InQuad);
    }
}