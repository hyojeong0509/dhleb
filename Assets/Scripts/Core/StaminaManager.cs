using UnityEngine;
using System;

/// <summary>
/// 스테미나(에너지) 관리
/// - 도구 사용 시 소모 (괭이, 물뿌리개 각 1)
/// - 침대에서 회복 (정상 100%, 탈진 후 60%)
/// - 0이 되면 탈진 (이동속도 저하)
/// </summary>
public class StaminaManager : MonoBehaviour
{
    public static StaminaManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<StaminaManager>();
                if (_instance == null)
                {
                    var go = new GameObject("StaminaManager");
                    _instance = go.AddComponent<StaminaManager>();
                }
            }
            return _instance;
        }
        private set => _instance = value;
    }
    private static StaminaManager _instance;

    [Header("스테미나 설정")]
    public int maxStamina = 100;
    public int exhaustedRecoveryAmount = 60;  // 탈진 후 Sleep 시 회복량

    private int currentStamina;
    private bool wasExhausted;  // 전날 0이 됐었는지 (Sleep 시 60% 회복용)

    public int CurrentStamina => currentStamina;
    public int MaxStamina => maxStamina;
    public bool IsExhausted => currentStamina <= 0;
    public float NormalizedStamina => maxStamina > 0 ? (float)currentStamina / maxStamina : 0f;

    public event Action OnStaminaChanged;
    public event Action OnExhausted;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        currentStamina = maxStamina;
    }

    /// <summary>
    /// 스테미나 소모 (도구 사용 시)
    /// </summary>
    /// <returns>소모 성공 여부</returns>
    public bool UseStamina(int amount = 1)
    {
        if (currentStamina < amount) return false;

        currentStamina = Mathf.Max(0, currentStamina - amount);
        OnStaminaChanged?.Invoke();

        if (currentStamina <= 0)
        {
            wasExhausted = true;
            OnExhausted?.Invoke();
        }

        return true;
    }

    /// <summary>
    /// 스테미나 소모 가능 여부
    /// </summary>
    public bool CanUseStamina(int amount = 1)
    {
        return currentStamina >= amount;
    }

    /// <summary>
    /// Sleep 시 회복 (침대에서 호출)
    /// currentGameMinutes: TimeManager.CurrentGameMinutes (26:00=02:00, 30:00=06:00)
    /// 02:00 이전: 100%, 03:00까지: 60%, 04:00까지: 40%, 05:00~05:59: 20%
    /// </summary>
    public void RestoreOnSleep(float currentGameMinutes)
    {
        float recoveryPercent;
        if (currentGameMinutes < 26f * 60f)      // 02:00 이전
            recoveryPercent = 1f;
        else if (currentGameMinutes < 27f * 60f) // 03:00까지
            recoveryPercent = 0.6f;
        else if (currentGameMinutes < 28f * 60f) // 04:00까지
            recoveryPercent = 0.4f;
        else                                     // 05:00~05:59
            recoveryPercent = 0.2f;

        currentStamina = Mathf.RoundToInt(maxStamina * recoveryPercent);
        wasExhausted = false;
        OnStaminaChanged?.Invoke();
    }

    /// <summary>
    /// 외부에서 값 설정 (로드 시)
    /// </summary>
    public void SetStamina(int value, bool setWasExhausted = false)
    {
        currentStamina = Mathf.Clamp(value, 0, maxStamina);
        if (setWasExhausted) wasExhausted = setWasExhausted;
        OnStaminaChanged?.Invoke();
    }

    /// <summary>
    /// wasExhausted 플래그 설정 (로드 시)
    /// </summary>
    public void SetWasExhausted(bool value)
    {
        wasExhausted = value;
    }
}
