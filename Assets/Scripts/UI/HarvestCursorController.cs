using UnityEngine;

/// <summary>
/// 수확 가능한 작물 위에 마우스를 올렸을 때만 커서를 harvestCursor로 변경.
/// 기본 커서는 Player Settings > Default Cursor 사용.
/// </summary>
public class HarvestCursorController : MonoBehaviour
{
    public static HarvestCursorController Instance { get; private set; }

    [Header("수확 커서")]
    [Tooltip("수확 가능할 때 표시할 커서 (Texture Type: Cursor 권장)")]
    public Texture2D harvestCursor;
    [Tooltip("클릭 기준점 (픽셀)")]
    public Vector2 harvestHotSpot = Vector2.zero;

    private bool isHarvestMode;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnDestroy()
    {
        if (Instance == this)
            ResetToDefault();
    }

    /// <summary>수확 커서로 전환</summary>
    public void SetHarvestCursor()
    {
        if (isHarvestMode) return;
        isHarvestMode = true;

        if (harvestCursor != null)
        {
            Cursor.SetCursor(harvestCursor, harvestHotSpot, CursorMode.Auto);
        }
    }

    /// <summary>Player Settings의 Default Cursor로 복원</summary>
    public void ResetToDefault()
    {
        if (!isHarvestMode) return;
        isHarvestMode = false;

        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    void LateUpdate()
    {
        if (isHarvestMode && harvestCursor != null)
            Cursor.SetCursor(harvestCursor, harvestHotSpot, CursorMode.Auto);
    }
}
