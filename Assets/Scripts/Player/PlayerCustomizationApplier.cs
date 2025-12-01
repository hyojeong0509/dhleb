using UnityEngine;

/// <summary>
/// Game 씬에서 플레이어에 커스터마이징을 적용하는 스크립트
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerCustomizationApplier : MonoBehaviour
{
    [Header("커스터마이징 적용 대상")]
    [Tooltip("피부색을 적용할 스프라이트 렌더러")]
    public SpriteRenderer skinRenderer;
    [Tooltip("머리색을 적용할 스프라이트 렌더러")]
    public SpriteRenderer hairRenderer;
    [Tooltip("옷 색상을 적용할 스프라이트 렌더러")]
    public SpriteRenderer outfitRenderer;

    [Header("설정")]
    [Tooltip("시작 시 자동으로 커스터마이징 적용")]
    public bool applyOnStart = true;

    private SpriteRenderer mainRenderer;

    void Start()
    {
        mainRenderer = GetComponent<SpriteRenderer>();

        // 커스터마이징 자동 적용
        if (applyOnStart)
        {
            ApplyCustomization();
        }
    }

    /// <summary>
    /// 저장된 커스터마이징 데이터를 플레이어에 적용
    /// </summary>
    public void ApplyCustomization()
    {
        if (GameDataManager.Instance == null || GameDataManager.Instance.currentCustomization == null)
        {
            Debug.LogWarning("PlayerCustomizationApplier: 커스터마이징 데이터가 없습니다. 기본값을 사용합니다.");
            return;
        }

        PlayerCustomizationData data = GameDataManager.Instance.currentCustomization;

        // 색상 적용
        ApplyColor(data.skinColor, skinRenderer);
        ApplyColor(data.hairColor, hairRenderer);
        ApplyColor(data.outfitColor, outfitRenderer);

        // 메인 렌더러에도 기본 색상 적용 (옵션)
        if (mainRenderer != null && skinRenderer == null)
        {
            mainRenderer.color = data.skinColor;
        }

        Debug.Log($"PlayerCustomizationApplier: 커스터마이징 적용 완료 - 이름: {data.playerName}");
    }

    void ApplyColor(Color color, SpriteRenderer renderer)
    {
        if (renderer != null)
        {
            renderer.color = color;
        }
    }

    /// <summary>
    /// 특정 커스터마이징 데이터를 적용
    /// </summary>
    public void ApplyCustomization(PlayerCustomizationData data)
    {
        if (data == null)
        {
            Debug.LogWarning("PlayerCustomizationApplier: 적용할 커스터마이징 데이터가 null입니다.");
            return;
        }

        ApplyColor(data.skinColor, skinRenderer);
        ApplyColor(data.hairColor, hairRenderer);
        ApplyColor(data.outfitColor, outfitRenderer);

        if (mainRenderer != null && skinRenderer == null)
        {
            mainRenderer.color = data.skinColor;
        }
    }
}





