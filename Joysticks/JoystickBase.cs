using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public abstract class JoystickBase : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Settings")]
    [SerializeField] protected float _handleRange = 1f;
    [SerializeField] protected float _deadZone = 0.05f;
    [SerializeField] protected AxisOptions _axisOptions = AxisOptions.Both;
    [SerializeField] protected bool _snapX;
    [SerializeField] protected bool _snapY;

    [Header("References")]
    [SerializeField] protected RectTransform _background;
    [SerializeField] protected RectTransform _handle;

    protected Canvas _canvas;
    protected Camera _cam;
    protected Vector2 _input = Vector2.zero;
    protected Vector2 _radius;

    public float Horizontal => _snapX ? SnapAxis(_input.x, AxisOptions.Horizontal) : _input.x;
    public float Vertical   => _snapY ? SnapAxis(_input.y, AxisOptions.Vertical)   : _input.y;
    public Vector2 Direction => new Vector2(Horizontal, Vertical);

    protected virtual void Awake()
    {
        if (_background == null || _handle == null)
        {
            Debug.LogError($"[{name}] Missing Joystick references. Assign Background and Handle in Inspector.", this);
            enabled = false;
            return;
        }

        _canvas = GetComponentInParent<Canvas>();

        if (_canvas == null)
        {
            Debug.LogError($"[{name}] Joystick must be a child of a Canvas.", this);
            enabled = false;
            return;
        }

        _radius = _background.sizeDelta * 0.5f;
        _cam = (_canvas.renderMode == RenderMode.ScreenSpaceCamera || _canvas.renderMode == RenderMode.WorldSpace) 
               ? _canvas.worldCamera : null;
    }

    protected virtual void Start() { }
    protected virtual void OnDisable() => ResetInput();

    public virtual void OnPointerDown(PointerEventData eventData) => OnDrag(eventData);

    public virtual void OnDrag(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_background, eventData.position, _cam, out Vector2 localPoint))
        {
            Vector2 raw = localPoint / _radius;
            ApplyAxisConstraints(ref raw);

            float mag = raw.magnitude;
            Vector2 norm = mag > 0.001f ? raw / mag : Vector2.zero;

            HandleInput(mag, norm);
            _handle.anchoredPosition = _input * _radius * _handleRange;
        }
    }

    protected virtual void HandleInput(float magnitude, Vector2 normalized)
    {
        if (magnitude > _deadZone)
        {
            _input = magnitude > 1f ? normalized : normalized * magnitude;
        }
        else
        {
            _input = Vector2.zero;
        }
    }

    private void ApplyAxisConstraints(ref Vector2 v)
    {
        switch (_axisOptions)
        {
            case AxisOptions.Horizontal: 
                v.y = 0f;
                break;
            case AxisOptions.Vertical:
                v.x = 0f;
                break;
        }
    }

    private float SnapAxis(float value, AxisOptions snapAxis)
    {
        if (_input.sqrMagnitude < 0.001f) return 0f;

        if (_axisOptions != AxisOptions.Both) return Mathf.Sign(value);

        float angle = Vector2.Angle(Vector2.up, _input);

        if (snapAxis == AxisOptions.Horizontal)
            return (angle < 22.5f || angle > 157.5f) ? 0f : Mathf.Sign(value);

        return (angle > 67.5f && angle < 112.5f) ? 0f : Mathf.Sign(value);
    }

    public virtual void OnPointerUp(PointerEventData eventData) => ResetInput();

    protected virtual void ResetInput()
    {
        _input = Vector2.zero;

        if (_handle != null) _handle.anchoredPosition = Vector2.zero;
    }

    protected Vector2 ScreenToAnchored(Vector2 screenPos)
    {
        return RectTransformUtility.ScreenPointToLocalPointInRectangle(_background, screenPos, _cam, out Vector2 lp) ? lp : Vector2.zero;
    }

    public enum AxisOptions { Both, Horizontal, Vertical }
}