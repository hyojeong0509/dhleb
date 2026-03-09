using UnityEngine;

/// <summary>
/// 잔디 - 괭이 1회로 제거, 드롭 없음
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class GrassObject : NaturalObject
{
    public override string ObjectType => "Grass";
    public override int HitsRemaining => 1;

    public override bool CanHitWith(ToolType toolType) => toolType == ToolType.Hoe;

    public override bool OnHit(ToolType toolType)
    {
        if (!CanHitWith(toolType)) return false;
        if (NaturalObjectManager.Instance != null)
            NaturalObjectManager.Instance.ReturnToPool(this);
        return true;
    }

    public override NaturalObjectSaveData ToSaveData(Vector3Int cellPos)
    {
        return new NaturalObjectSaveData
        {
            cellX = cellPos.x,
            cellY = cellPos.y,
            cellZ = cellPos.z,
            objectType = ObjectType,
            hitsRemaining = 1
        };
    }
}
