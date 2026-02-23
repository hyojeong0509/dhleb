using UnityEngine;
using System;

/// <summary>
/// 골드(화폐) 관리
/// </summary>
public class GoldManager : MonoBehaviour
{
    public static GoldManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GoldManager>();
                if (_instance == null)
                {
                    var go = new GameObject("GoldManager");
                    _instance = go.AddComponent<GoldManager>();
                }
            }
            return _instance;
        }
        private set => _instance = value;
    }
    private static GoldManager _instance;

    private int currentGold;

    public int CurrentGold => currentGold;

    public event Action OnGoldChanged;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }

    /// <summary>
    /// 골드 추가
    /// </summary>
    public void AddGold(int amount)
    {
        if (amount <= 0) return;
        currentGold += amount;
        OnGoldChanged?.Invoke();
    }

    /// <summary>
    /// 골드 사용 (지불 가능 여부 확인)
    /// </summary>
    /// <returns>지불 성공 여부</returns>
    public bool SpendGold(int amount)
    {
        if (amount <= 0) return true;
        if (currentGold < amount) return false;

        currentGold -= amount;
        OnGoldChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// 지불 가능 여부
    /// </summary>
    public bool CanAfford(int amount) => currentGold >= amount;

    /// <summary>
    /// 강제 차감 (부족해도 0까지 차감, 응급실 비용 등)
    /// </summary>
    public void ForceSpend(int amount)
    {
        if (amount <= 0) return;
        currentGold = Mathf.Max(0, currentGold - amount);
        OnGoldChanged?.Invoke();
    }

    /// <summary>
    /// 외부에서 값 설정 (로드 시)
    /// </summary>
    public void SetGold(int amount)
    {
        currentGold = Mathf.Max(0, amount);
        OnGoldChanged?.Invoke();
    }
}
