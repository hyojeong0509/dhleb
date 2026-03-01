using UnityEngine;

/// <summary>
/// 자연물 베이스 (잔디, 나무, 돌)
/// </summary>
public abstract class NaturalObject : MonoBehaviour
{
    public abstract string ObjectType { get; }
    public abstract int HitsRemaining { get; }
    public abstract bool CanHitWith(ToolType toolType);
    /// <summary>Hit 성공 시 true, 실패 시 false</summary>
    public abstract bool OnHit(ToolType toolType);
    public abstract NaturalObjectSaveData ToSaveData(Vector3Int cellPos);
}
