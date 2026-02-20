using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EPC {
    public class MainControl : MonoBehaviour {
        public static MainControl I;
        private void Awake() => I = this;
        public void PanelRendTime(GameObject targetPanel, float duration = 1.25f) => 
            PanelTimerAsync(targetPanel, this.GetCancellationTokenOnDestroy(), duration).Forget();
        public void PanelRendTime(GameObject targetPanel) =>
            PanelTimerAsync(targetPanel, this.GetCancellationTokenOnDestroy()).Forget();
        
        private async UniTask PanelTimerAsync(GameObject panel, CancellationToken token, float duration = 1.25f) {
            if (panel == null) return;

            panel.SetActive(true);

            try {
                await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: token);
                if (panel != null) panel.SetActive(false);
            }
            catch (OperationCanceledException) {
                // Игнорируем отмену
            }
        }

        public void PanelToggle(GameObject panel) {
            if (panel != null) panel.SetActive(!panel.activeSelf);
        }

        public void LoadScene(string sceneName) {
            SceneManager.LoadScene(sceneName);
            Debug.Log($"Loaded Scene: {sceneName}");
        }

        // public async UniTask MoveAndRotateAsync(Transform target, Vector3 endPos, Quaternion endRot, float duration, CancellationToken token) {
        //     if (target == null) return;

        //     Vector3 startPos = target.position;
        //     Quaternion startRot = target.rotation;
        //     float elapsed = 0;

        //     try {
        //         while (elapsed < duration) {
        //             elapsed += Time.deltaTime;
        //             // float t = elapsed / duration;
        //             float t = Mathf.SmoothStep(0, 1, elapsed / duration);

        //             target.SetPositionAndRotation(
        //                 Vector3.Lerp(startPos, endPos, t),
        //                 Quaternion.Slerp(startRot, endRot, t)
        //             );

        //             await UniTask.Yield(PlayerLoopTiming.Update, token);
        //         }

        //         target.SetPositionAndRotation(endPos, endRot);
        //     }
        //     catch (OperationCanceledException) {
        //         Debug.Log("Pos & QRot canceled");
        //     }
        // }
        // Оставлено для обратной совместимости, но рекомендуется использовать твины напрямую
        public async UniTask MoveAndRotateAsync(Transform target, Vector3 endPos, Quaternion endRot, float duration, CancellationToken token) {
            if (target == null) return;

            if (duration <= 0f) {
                target.SetPositionAndRotation(endPos, endRot);
                return;
            }

            Vector3 startPos = target.position;
            Quaternion startRot = target.rotation;
            float elapsed = 0f;
            float invDuration = 1f / duration;

            while (elapsed < duration) {
                if (token.IsCancellationRequested || target == null) return;

                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed * invDuration);
                t = t * t * (3f - 2f * t); // SmoothStep

                target.SetPositionAndRotation(
                    Vector3.LerpUnclamped(startPos, endPos, t),
                    Quaternion.SlerpUnclamped(startRot, endRot, t)
                );

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            if (target != null) target.SetPositionAndRotation(endPos, endRot);
        }

        public void OpenUrl(string url) {
            if (!string.IsNullOrEmpty(url)) Application.OpenURL(url);
            else Debug.LogWarning("Link is Empty!");
        }
        public void OpenDiscord() => OpenUrl("https://discord.gg/DVPJV6TgVB");
        public void OpenTelegram() => OpenUrl("https://t.me/epicvoidcorp");

        public void GameExit() {
            Debug.Log("Game Extt");
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #endif
            Application.Quit();
        }
    }
}
