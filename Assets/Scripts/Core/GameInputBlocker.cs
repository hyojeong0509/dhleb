using UnityEngine;

/// <summary>
/// UI 패널이 열려있을 때 게임 입력(도구, 씨앗, 수확 등) 차단
/// </summary>
public static class GameInputBlocker
{
    private static int blockCount;
    public static bool IsBlocked => blockCount > 0;

    public static void Block()
    {
        blockCount = Mathf.Max(0, blockCount) + 1;
    }

    public static void Unblock()
    {
        blockCount = Mathf.Max(0, blockCount - 1);
    }
}
