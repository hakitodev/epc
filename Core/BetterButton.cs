using System;
using System.Threading;
using System.Collections;
using UnityEngine; // serializeField
using UnityEngine.UI; // Selectable
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using PrimeTween;
using Cysharp.Threading.Tasks;
// using Unity.Entities.UniversalDelegates;

// Need UniTask and PrimeTween, but there is coroutine (no yet, you can do it yourself by renaming Unitask to IEnumerator)
namespace Hakito.UI
{
    [AddComponentMenu("UI Hakito/Better Button", 30)]
    public class BetterButton : Selectable, ISubmitHandler, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler // IPointerClickHandler,
    {

#region Public API

        [Serializable]
        /// <summary>
        /// Function definition for a button perform event.
        /// </summary>
        public class ButtonPerformedEvent : UnityEvent {}

        [Serializable]
        /// <summary>
        /// Function definition for a button realize event.
        /// </summary>
        public class ButtonRealizedEvent : UnityEvent {}

        [Serializable]
        /// <summary>
        /// Function definition for a button hold event.
        /// </summary>
        public class ButtonHoldEvent : UnityEvent {}

        /// <summary>
        /// Event delegates triggered on Down / Perform.
        /// </summary>
        [FormerlySerializedAs("OnPerform")]
        [SerializeField]
        private ButtonPerformedEvent _onPerformEvent = new ButtonPerformedEvent();

        /// <summary>
        /// Event delegates triggered on Up / Realize.
        /// </summary>
        [FormerlySerializedAs("OnRealize")]
        [SerializeField]
        private ButtonRealizedEvent _onRealizeEvent = new ButtonRealizedEvent();

        /// <summary>
        /// Event delegates triggered on Hold.
        /// </summary>
        [FormerlySerializedAs("OnHold")]
        [SerializeField]
        private ButtonHoldEvent _onHoldEvent = new ButtonHoldEvent();

        public ButtonPerformedEvent OnPerformEvent
        {
            get { return _onPerformEvent; }
            set { _onPerformEvent = value; }
        }

        public ButtonRealizedEvent OnRealizeEvent
        {
            get { return _onRealizeEvent; }
            set { _onRealizeEvent = value; }
        }

        public ButtonHoldEvent OnHoldEvent
        {
            get { return _onHoldEvent; }
            set { _onHoldEvent = value; }
        }

#endregion

#region Settings

        [Header("Settings")]
        [Space(10)]
        [SerializeField]
        private bool _isChangeScaleOnPressed = false;
        [SerializeField]
        private bool _isHoldOnlyOnRelize = false;
        [SerializeField]
        private bool _useTimeBeforeHold = true;
        [SerializeField]
        private bool _usePulseByHolding = true;
        private bool _isPointerExited = true;
        private bool _isHolding = false;

        [Space(10)]
        [SerializeField]
        private float _scaleMultiplier = 0.85f;
        [SerializeField]
        private float _holdDuration = 0.5f;
        [SerializeField]
        private float _beforeHoldDuration = 0.25f;
        [SerializeField]
        private float _holdSpreadSpeed = 2f;
        [SerializeField]
        private float _holdSpreadAmount = 0.1f;

        private CancellationTokenSource _holdCts;

        RectTransform _rectTransform;
        Vector3 _initialScale;
        Vector3 _multipliedScale;

#endregion

        protected override void OnDisable()
        {
            base.OnDisable();
            _holdCts?.Cancel();
            _holdCts?.Dispose();
        }

        protected override void Start()
        {
            _rectTransform = GetComponent<RectTransform>();
            _initialScale = _rectTransform.localScale;
            _multipliedScale = _initialScale * _scaleMultiplier;
        }

        public override void OnPointerEnter(PointerEventData e)
        {
            base.OnPointerEnter(e);
            _isPointerExited = false;
        }
    
        public override void OnPointerExit(PointerEventData e)
        {
            base.OnPointerExit(e);
            _isPointerExited = true;
        }

#region Perform
        public override void OnPointerDown(PointerEventData e)
        {
            if (e.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            if (_isChangeScaleOnPressed)
            {
                _rectTransform.localScale = _multipliedScale;
            }

            _isPointerExited = false;

            Perform();
            StartHoldTracking();
        }

        private void Perform()
        {
            if (!IsActive() || !IsInteractable())
            {
                return;
            }

            UISystemProfilerApi.AddMarker("Button.OnPerform", this);
            _onPerformEvent.Invoke();
        }

#endregion

#region Realize

        public override void OnPointerUp(PointerEventData e)
        {
            if (e.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            _holdCts?.Cancel();

            

            if (!_isPointerExited && IsActive() && IsInteractable())
            {
                if (_isHoldOnlyOnRelize && _isHolding)
                {
                    TriggerHoldEvent();
                }
                Realize();
            }

            if (_isChangeScaleOnPressed)
            {
                _rectTransform.localScale = _initialScale;
            }

            _isPointerExited = false;
            _isHolding = false;
        }

        private void Realize()
        {
            if (!IsActive() || !IsInteractable())
            {
                return;
            }

            UISystemProfilerApi.AddMarker("Button.OnRealize", this);
            _onRealizeEvent.Invoke();
        }

#endregion

#region Click

        //     public virtual void OnPointerClick(PointerEventData e)
        //     {
        //         if (e.button != PointerEventData.InputButton.Left)
        //         {
        //             return;
        //         }

        //         Click();
        //     }

        private void Click()
        {
            if (!IsActive() || !IsInteractable())
            {
                return;
            }

            UISystemProfilerApi.AddMarker("Button.onClick", this);
            _onPerformEvent.Invoke();
            _onRealizeEvent.Invoke();
        }

#endregion

#region Hold

        private void StartHoldTracking()
        {
            _holdCts?.Cancel();
            _holdCts?.Dispose();

            _holdCts = new CancellationTokenSource();

            HoldAsync(_holdCts.Token).Forget();
        }

        private async UniTask HoldAsync(CancellationToken cts)
        {
            try
            {
                if (_useTimeBeforeHold)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(_beforeHoldDuration), cancellationToken: cts);
                }

                if (_usePulseByHolding)
                {
                    if (_holdSpreadAmount > 0)
                    {
                        var tween = Tween.PunchScale(transform, strength: Vector3.one * _holdSpreadAmount, duration: _holdDuration, frequency: _holdSpreadSpeed);

                        await tween.ToUniTask(cancellationToken: cts);
                    }
                }
                else
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(_holdDuration), cancellationToken: cts);
                }

                _isHolding = true;
                // transform.localScale = _initialScale;

                if (!_isHoldOnlyOnRelize)
                {
                    TriggerHoldEvent();
                }
            }
            catch (OperationCanceledException)
            {
                // transform.localScale = _initialScale;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[BetterButton] HoldAsync error: {ex.Message}");
            }
        }

        private void TriggerHoldEvent()
        {
            if (!IsActive() || !IsInteractable()) 
            {
                return;
            }

            UISystemProfilerApi.AddMarker("Button.OnHold", this);

            if (!_isPointerExited)
            {
                _onHoldEvent?.Invoke();
            }
        }

#endregion

        protected BetterButton() { }

        /// <summary>
        /// Call all registered ISubmitHandler.
        /// </summary>
        /// <param name="e">Associated data with the event. Typically by the event system.</param>
        /// <remarks>
        /// This detects when a Button has been selected via a "submit" key you specify (default is the return key).
        ///
        /// To change the submit key, either:
        ///
        /// 1. Go to Edit->Project Settings->Input.
        ///
        /// 2. Next, expand the Axes section and go to the Submit section if it exists.
        ///
        /// 3. If Submit doesn’t exist, add 1 number to the Size field. This creates a new section at the bottom. Expand the new section and change the Name field to “Submit”.
        ///
        /// 4. Change the Positive Button field to the key you want (e.g. space).
        ///
        ///
        /// Or:
        ///
        /// 1. Go to your EventSystem in your Project
        ///
        /// 2. Go to the Inspector window and change the Submit Button field to one of the sections in the Input Manager (e.g. "Submit"), or create your own by naming it what you like, then following the next few steps.
        ///
        /// 3. Go to Edit->Project Settings->Input to get to the Input Manager.
        ///
        /// 4. Expand the Axes section in the Inspector window. Add 1 to the number in the Size field. This creates a new section at the bottom.
        ///
        /// 5. Expand the new section and name it the same as the name you inserted in the Submit Button field in the EventSystem. Set the Positive Button field to the key you want (e.g. space)
        /// </remarks>

        public virtual void OnSubmit(BaseEventData e)
        {
            Click();

            if (!IsActive() || !IsInteractable())
            {
                return;
            }

            var cts = new CancellationTokenSource();

            DoStateTransition(SelectionState.Pressed, false);
            OnFinishSubmitAsync(cts.Token).Forget();
        }

        private async UniTask OnFinishSubmitAsync(CancellationToken cts)
        {
            var fadeTime = colors.fadeDuration;
            var elapsedTime = 0f;

            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.unscaledDeltaTime;
                await UniTask.Yield(cts);
            }

            DoStateTransition(currentSelectionState, false);
        }
    }
}
