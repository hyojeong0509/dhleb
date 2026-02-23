using UnityEngine;
using System;

// 골드 관리
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

    // 골드 추가
    public void AddGold(int amount)
    {
        if (amount <= 0) return;
        currentGold += amount;
        OnGoldChanged?.Invoke();
    }


    // 골드 사용
    // <returns>지불 성공 여부</returns>
    public bool SpendGold(int amount)
    {
        if (amount <= 0) return true;
        if (currentGold < amount) return false;

        currentGold -= amount;
        OnGoldChanged?.Invoke();
        return true;
    }

    // 지불 가능 여부
    public bool CanAfford(int amount) => currentGold >= amount;

    // 강제 차감 (부족해도 0까지 차감, 응급실 비용 등 추후 설정 예정 )
    public void ForceSpend(int amount)
    {
        if (amount <= 0) return;
        currentGold = Mathf.Max(0, currentGold - amount);
        OnGoldChanged?.Invoke();
    }

    // 외부에서 값 설정 (로드 시)
    public void SetGold(int amount)
    {
        currentGold = Mathf.Max(0, amount);
        OnGoldChanged?.Invoke();
    }
}
