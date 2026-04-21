using System;
using UnityEngine;
using PrimeTween;
using EPC;

namespace EPC
{
    public class MainMenuMove : MonoBehaviour
    {
        [Serializable]
        private class MenuPoint 
        {
            public GameObject Panel;
            public Vector3 Pos;
            public Vector3 Rot;
            public float Duration;
            public Ease Ease;
        }

        [SerializeField]
        private Animator anim;
        [SerializeField]
        private MenuPoint Main, PlayMode, Settings, Shop;

        private GameObject _activePanel;

        private Sequence _moveTween;


        private void OnDestroy()
        {
            _moveTween.Stop();
        }

        private void Start()
        {
            SetMain();
        }

        private void SwitchPoint(MenuPoint point) 
        {
            if (point == null)
            {
                return;
            }

            if (_moveTween.isAlive)
            {
                _moveTween.Stop();
            }

            if (_activePanel != null)
            {
                _activePanel.SetActive(false);
            }

            if (anim != null && point != Main)
            {
                anim.SetBool("open", false);
            }

            _moveTween = Sequence.Create()
                .Group(Tween.Position(transform, point.Pos, point.Duration, Ease.InOutSine))
                .Group(Tween.Rotation(transform, Quaternion.Euler(point.Rot), point.Duration, Ease.InOutSine))
                .OnComplete(() =>
                {
                    if (this == null)
                    {
                        return;
                    }

                    if (point.Panel != null)
                    {
                        point.Panel.SetActive(true);
                        _activePanel = point.Panel;
                    }
                });
        }

        public void SetMain()
        {
            SwitchPoint(Main);
            if (anim != null)
            {
                anim.SetBool("open", true);
            }
        }

        public void SetPlayMode() => SwitchPoint(PlayMode);
        public void SetSettings() => SwitchPoint(Settings);
        public void SetShop() => SwitchPoint(Shop);
    }
}