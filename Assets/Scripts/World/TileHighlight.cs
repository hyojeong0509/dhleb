using UnityEngine;
using UnityEngine.Tilemaps;

// 마우스 커서가 가리키는 타일을 하이라이트로 표시
public class TileHighlight : MonoBehaviour
{
    public static TileHighlight Instance { get; private set; }

    [Header("하이라이트 설정")]
    public SpriteRenderer highlightRenderer;

    [Header("색상")]
    public Color validColor   = new Color(0.3f, 1f, 0.3f, 0.4f);   // 초록 반투명
    public Color invalidColor = new Color(1f, 0.3f, 0.3f, 0.4f);   // 빨강 반투명

    private Tilemap referenceTilemap;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        gameObject.SetActive(false);
    }

    public void Init(Tilemap tilemap)
    {
        referenceTilemap = tilemap;
    }

    // 하이라이트를 타일 위치에 표시
    public void Show(Vector3 worldPos, bool isValid)
    {
        if (referenceTilemap == null) return;

        gameObject.SetActive(true);

        // 타일 격자에 딱 맞게 스냅
        Vector3Int cell = referenceTilemap.WorldToCell(worldPos);
        transform.position = referenceTilemap.GetCellCenterWorld(cell);

        highlightRenderer.color = isValid ? validColor : invalidColor;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
