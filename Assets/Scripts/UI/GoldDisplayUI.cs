using UnityEngine;
using TMPro;

/// <summary>
/// HUD 골드 표시
/// </summary>
public class GoldDisplayUI : MonoBehaviour
{
    public TMP_Text txtGold;

    [Tooltip("체크 시 오른쪽 정렬 (숫자가 오른쪽부터 채워짐)")]
    public bool rightAlign = true;

    void Start()
    {
        if (txtGold == null)
            txtGold = GetComponent<TMP_Text>();

        if (txtGold != null && rightAlign)
            txtGold.alignment = TextAlignmentOptions.Right;

        if (GoldManager.Instance != null)
        {
            GoldManager.Instance.OnGoldChanged += Refresh;
            Refresh();
        }
    }

    void OnDestroy()
    {
        if (GoldManager.Instance != null)
            GoldManager.Instance.OnGoldChanged -= Refresh;
    }

    void Refresh()
    {
        if (txtGold != null && GoldManager.Instance != null)
            txtGold.text = GoldManager.Instance.CurrentGold.ToString("N0");
    }
}
