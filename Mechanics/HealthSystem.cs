using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public struct HealthSettings {
    public float healthMax;
    public TMP_Text healthText;
    public Slider healthSlider;
    public float healthRegen;
    public float healthRegenDelay;
    public bool enableRegen;
    public bool infiniteHealth;
    public bool isInvincibility;
}
public class HealthSystem : MonoBehaviour {
    [SerializeField]
    private HealthSettings _healthSettings = new HealthSettings {
        healthMax = 200f,
        healthRegen = 8f,
        healthRegenDelay = 1.5f,
        enableRegen = true,
        infiniteHealth = false,
        isInvincibility = false,
    };
    [SerializeField] private float _healthCurrent;
    private bool isDead;

    private CancellationTokenSource _regenCts;

    public event System.Action OnDeath;
    public event System.Action<float> OnDamageTaken;


    public float CurrentHealth => _healthCurrent;
    public float MaxHealth => _healthSettings.healthMax;
    public float HealthPercent => _healthCurrent / _healthSettings.healthMax;
    public bool IsAlive => _healthCurrent > 0 && !isDead;

    #region Health
        private void Start() {
            _healthCurrent = _healthSettings.healthMax;
            // isDead = false;
            RenderHealth();
        }
        private void StartRegeneration() {
            _regenCts?.Cancel();
            _regenCts?.Dispose();
            _regenCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
            RegenerationProcess(_regenCts.Token).Forget();
        }

        private async UniTaskVoid RegenerationProcess(CancellationToken token) {
            try {
                await UniTask.Delay(TimeSpan.FromSeconds(_healthSettings.healthRegenDelay), cancellationToken: token);
                
                while (_healthCurrent < _healthSettings.healthMax && !isDead && !token.IsCancellationRequested) {
                    _healthCurrent += _healthSettings.healthRegen * Time.deltaTime;
                    _healthCurrent = Mathf.Min(_healthCurrent, _healthSettings.healthMax);
                    RenderHealth();
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }
            }
            catch (OperationCanceledException) {
                // Игнорируем отмену
            }
        }
    
        public void StopRegeneration() {
            _regenCts?.Cancel();
            _regenCts?.Dispose();
            _regenCts = null;
        }
        public void Heal(float amount) {
            if (isDead) return;
            _healthCurrent += amount;
            _healthCurrent = Mathf.Clamp(_healthCurrent, 0f, _healthSettings.healthMax);
            RenderHealth();
        }
        public void Damage(float amount) {
            if (_healthSettings.isInvincibility || isDead) return;
            _healthCurrent -= amount;
            _healthCurrent = Mathf.Clamp(_healthCurrent, 0f, _healthSettings.healthMax);
            if (_healthCurrent <= 0) Kill();
            if (_healthSettings.enableRegen && !isDead) StartRegeneration();
            OnDamageTaken?.Invoke(amount);
            RenderHealth();
        }
        private void SetHealthInfinite(bool enabled) {
            _healthSettings.infiniteHealth = enabled;
            if (enabled) _healthCurrent = _healthSettings.healthMax;
        }
        public void SetInvincibility(bool enabled) => _healthSettings.isInvincibility = enabled;
        public void EnableRegeneration(bool enabled) => _healthSettings.enableRegen = enabled;
        public void Kill() {
            isDead = true;
            OnDeath?.Invoke();
        }
        public void HealthReset() {
            _healthCurrent = _healthSettings.healthMax;
            // _isExhausted = false;
            // _staminaRegenTimer = 0f;
        }
        public void ToggleHealthInfinite() {
            SetHealthInfinite(!_healthSettings.infiniteHealth);
            Debug.Log($"Infinity: {_healthSettings.infiniteHealth}");
        }
        public void RenderHealth() {
            if (_healthSettings.healthText) _healthSettings.healthText.text = $"{_healthCurrent}";
            if (_healthSettings.healthSlider) {
                _healthSettings.healthSlider.maxValue = _healthSettings.healthMax;
                _healthSettings.healthSlider.value = _healthCurrent;
            }
        }
    #endregion
}
