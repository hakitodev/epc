using System;
using UnityEngine;
using EPC;
#if PRIME_TWEEN
using PrimeTween;
#elif DOTWEEN
using DG.Tweening;
#endif

namespace EPC {
    public class MainMenuMove : MonoBehaviour {
        [SerializeField] private Animator anim;
        [SerializeField] private MenuPoint Main, PlayMode, Settings, Shop;

        private GameObject _activePanel;
#if PRIME_TWEEN
        private Tween _moveTween;
#elif DOTWEEN
        private Sequence _moveSequence;
#endif

        private void OnDestroy() {
#if PRIME_TWEEN
            _moveTween?.Stop();
#elif DOTWEEN
            _moveSequence?.Kill();
#endif
        }

        private void Start() {
            SetMain();
        }

        private void SwitchPoint(MenuPoint point) {
            if (point == null) return;

#if PRIME_TWEEN
            _moveTween?.Stop();
#elif DOTWEEN
            _moveSequence?.Kill();
#endif

            if (_activePanel != null)
                _activePanel.SetActive(false);

            if (anim != null)
                anim.SetBool("open", false);

#if PRIME_TWEEN
            _moveTween = Tween.PositionRotation(transform, point.Pos, Quaternion.Euler(point.Rot), point.Duration, Ease.InOutSine)
                .OnComplete(() => {
                    if (this == null) return;
                    if (point.Panel != null) {
                        point.Panel.SetActive(true);
                        _activePanel = point.Panel;
                    }
                });
#elif DOTWEEN
            _moveSequence = DOTween.Sequence()
                .Join(transform.DOMove(point.Pos, point.Duration).SetEase(Ease.InOutSine))
                .Join(transform.DORotateQuaternion(Quaternion.Euler(point.Rot), point.Duration).SetEase(Ease.InOutSine))
                .OnComplete(() => {
                    if (this == null) return;
                    if (point.Panel != null) {
                        point.Panel.SetActive(true);
                        _activePanel = point.Panel;
                    }
                });
#else
            // Fallback на простую анимацию через корутину
            StartCoroutine(MoveToPointCoroutine(point));
#endif
        }

#if !PRIME_TWEEN && !DOTWEEN
        private System.Collections.IEnumerator MoveToPointCoroutine(MenuPoint point) {
            Vector3 startPos = transform.position;
            Quaternion startRot = transform.rotation;
            Vector3 endPos = point.Pos;
            Quaternion endRot = Quaternion.Euler(point.Rot);
            float elapsed = 0f;

            while (elapsed < point.Duration) {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / point.Duration);
                transform.SetPositionAndRotation(
                    Vector3.Lerp(startPos, endPos, t),
                    Quaternion.Slerp(startRot, endRot, t)
                );
                yield return null;
            }

            transform.SetPositionAndRotation(endPos, endRot);
            if (point.Panel != null) {
                point.Panel.SetActive(true);
                _activePanel = point.Panel;
            }
        }
#endif

        public void SetMain() {
            SwitchPoint(Main);
            if (anim != null) anim.SetBool("open", true);
        }
        public void SetPlayMode() => SwitchPoint(PlayMode);
        public void SetSettings() => SwitchPoint(Settings);
        public void SetShop() => SwitchPoint(Shop);
    }

    [Serializable]
    public class MenuPoint {
        public GameObject Panel;
        public Vector3 Pos;
        public Vector3 Rot;
        public float Duration = 1.0f;
    }
}