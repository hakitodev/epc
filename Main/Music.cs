// using System;
// using System.IO;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.Audio;
// using TMPro;

using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using TMPro;

namespace EPC {
    [Serializable]
    public class ClipData {
        public string name;
        public string author;
        public bool can_play = true;
        public AudioClip audio;
    }

    public class Music : MonoBehaviour {
        [SerializeField] private TMP_Text clipNameTxt, clipAuthorTxt;
        [SerializeField] private List<ClipData> clips = new List<ClipData>();
        [SerializeField] private int clipCur = 0;
        [SerializeField] private AudioSource source;
        
        private CancellationTokenSource _cts;

        private void OnDestroy() {
            _cts?.Cancel();
            _cts?.Dispose();
        }

        private void Start() {
            if (clips.Count > 0) PlayTrack(clipCur).Forget();
        }

        public void ClipChange(int input) {
            clipCur += input;
            ValidateIndex();

            int attempts = 0;
            while (!clips[clipCur].can_play && attempts < clips.Count) {
                clipCur = (clipCur + 1) % clips.Count;
                attempts++;
            }

            if (clips[clipCur].can_play) PlayTrack(clipCur).Forget();
            else Debug.LogWarning("No Tracks!");
        }

        private async UniTaskVoid PlayTrack(int index) {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
            
            ClipData track = clips[index];
            source.clip = track.audio;
            source.Play();

            if (clipNameTxt != null) clipNameTxt.text = track.name;
            if (clipAuthorTxt != null) clipAuthorTxt.text = $"- {track.author} -";

            await WaitForTrackEnd(_cts.Token);
            if (!_cts.Token.IsCancellationRequested) ClipChange(1);
        }

        private async UniTask WaitForTrackEnd(CancellationToken token) {
            if (source.clip == null) return;
            
            float checkInterval = 0.1f;
            while (source.clip != null && source.time < source.clip.length - 0.1f) {
                await UniTask.Delay(TimeSpan.FromSeconds(checkInterval), cancellationToken: token);
            }
            await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: token);
        }

        private void ValidateIndex() {
            if (clipCur >= clips.Count) clipCur = 0;
            if (clipCur < 0) clipCur = clips.Count - 1;
        }
    }
}
