#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// CutsceneAction PropertyDrawer: 타입 선택 시 해당 타입에 필요한 필드만 표시
/// </summary>
[CustomPropertyDrawer(typeof(CutsceneAction))]
public class CutsceneActionDrawer : PropertyDrawer
{
    const float LINE_HEIGHT = 18f;
    const float SPACING = 2f;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var typeProp = property.FindPropertyRelative("type");
        var type = (CutsceneActionType)typeProp.enumValueIndex;
        float h = LINE_HEIGHT * 2 + SPACING * 2; // type + spacing
        h += GetFieldsHeight(property, type);
        return h + SPACING * 2;
    }

    float GetFieldsHeight(SerializedProperty property, CutsceneActionType type)
    {
        float h = 0;
        switch (type)
        {
            case CutsceneActionType.ShowDialogue:
                h += GetPropHeight(property, "dialogue");
                break;
            case CutsceneActionType.Wait:
                h += GetPropHeight(property, "waitDuration");
                break;
            case CutsceneActionType.FadeInFromBlack:
                h += GetPropHeight(property, "fadeInDuration");
                break;
            case CutsceneActionType.PlayerLookLeft:
            case CutsceneActionType.PlayerLookRight:
            case CutsceneActionType.PlayerLookUp:
                h += GetPropHeight(property, "lookDuration");
                break;
            case CutsceneActionType.SetAnimatorTrigger:
                h += GetPropHeight(property, "triggerName");
                break;
            case CutsceneActionType.SetFlag:
                h += GetPropHeight(property, "flagName");
                break;
            case CutsceneActionType.AdvanceStory:
                h += GetPropHeight(property, "storyAmount");
                break;
            case CutsceneActionType.AcceptQuest:
                h += GetPropHeight(property, "questId");
                break;
            case CutsceneActionType.CameraZoomOut:
                h += GetPropHeight(property, "zoomedInSize") + GetPropHeight(property, "zoomOutDuration");
                break;
            case CutsceneActionType.CameraZoomToTarget:
                h += GetPropHeight(property, "targetPosition") + GetPropHeight(property, "targetZoomedSize") + GetPropHeight(property, "zoomInDuration") + GetPropHeight(property, "holdDuration") + GetPropHeight(property, "zoomOutDurationTarget");
                break;
            case CutsceneActionType.PushPlayer:
                h += GetPropHeight(property, "pushDirection") + GetPropHeight(property, "pushDistance") + GetPropHeight(property, "pushDuration") + GetPropHeight(property, "pushPlayerTrigger") + GetPropHeight(property, "pushNpcId") + GetPropHeight(property, "pushNpcTrigger") + GetPropHeight(property, "pushNpcStateName");
                break;
            case CutsceneActionType.SetActive:
                h += GetPropHeight(property, "targetObject") + GetPropHeight(property, "setActive") + GetPropHeight(property, "npcId");
                break;
            case CutsceneActionType.AddAffection:
                h += GetPropHeight(property, "npcId") + GetPropHeight(property, "affectionAmount");
                break;
            case CutsceneActionType.NpcGroupMove:
                h += GetPropHeight(property, "npcIds") + GetPropHeight(property, "npcTargetPositions") + GetPropHeight(property, "usePlayerPositionAsOrigin") + GetPropHeight(property, "npcMoveDuration") + GetPropHeight(property, "npcSpawnAtCameraEdge") + GetPropHeight(property, "npcSpawnMargin");
                break;
            case CutsceneActionType.NpcGroupReturnToStart:
                h += GetPropHeight(property, "npcReturnIds") + GetPropHeight(property, "npcReturnDuration") + GetPropHeight(property, "npcReturnDurationMultiplier");
                break;
            case CutsceneActionType.NpcGroupMoveToPosition:
                h += GetPropHeight(property, "npcIds") + GetPropHeight(property, "npcTargetPositions") + GetPropHeight(property, "usePlayerPositionAsOrigin") + GetPropHeight(property, "npcMoveDuration");
                break;
            case CutsceneActionType.NpcTeleportToPosition:
                h += GetPropHeight(property, "npcIds") + GetPropHeight(property, "npcTargetPositions") + GetPropHeight(property, "usePlayerPositionAsOrigin");
                break;
            case CutsceneActionType.ShowNotification:
                h += GetPropHeight(property, "notificationText") + GetPropHeight(property, "notificationDuration") + GetPropHeight(property, "questId");
                break;
            case CutsceneActionType.PlayerMoveOffScreen:
                h += GetPropHeight(property, "playerWalkOffDuration") + GetPropHeight(property, "playerWalkOffMargin");
                break;
            case CutsceneActionType.TeleportPlayer:
                h += GetPropHeight(property, "teleportPosition");
                break;
            case CutsceneActionType.GiveItems:
                h += GetPropHeight(property, "giveItems");
                break;
            case CutsceneActionType.Custom:
            default:
                break;
        }
        return h;
    }

    float GetPropHeight(SerializedProperty parent, string name)
    {
        var prop = parent.FindPropertyRelative(name);
        if (prop == null) return 0;
        return EditorGUI.GetPropertyHeight(prop, true) + SPACING;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var typeProp = property.FindPropertyRelative("type");
        var type = (CutsceneActionType)typeProp.enumValueIndex;

        var rect = new Rect(position.x, position.y, position.width, LINE_HEIGHT);
        EditorGUI.PropertyField(rect, typeProp, new GUIContent("타입"));

        float y = position.y + LINE_HEIGHT + SPACING * 2;

        switch (type)
        {
            case CutsceneActionType.ShowDialogue:
                y = DrawProp(property, position, y, "dialogue");
                break;
            case CutsceneActionType.Wait:
                y = DrawProp(property, position, y, "waitDuration");
                break;
            case CutsceneActionType.FadeInFromBlack:
                y = DrawProp(property, position, y, "fadeInDuration");
                break;
            case CutsceneActionType.CameraZoomOut:
                y = DrawProp(property, position, y, "zoomedInSize");
                y = DrawProp(property, position, y, "zoomOutDuration");
                break;
            case CutsceneActionType.PlayerLookLeft:
            case CutsceneActionType.PlayerLookRight:
            case CutsceneActionType.PlayerLookUp:
                y = DrawProp(property, position, y, "lookDuration");
                break;
            case CutsceneActionType.CameraZoomToTarget:
                y = DrawProp(property, position, y, "targetPosition");
                y = DrawProp(property, position, y, "targetZoomedSize");
                y = DrawProp(property, position, y, "zoomInDuration");
                y = DrawProp(property, position, y, "holdDuration");
                y = DrawProp(property, position, y, "zoomOutDurationTarget");
                break;
            case CutsceneActionType.PushPlayer:
                y = DrawProp(property, position, y, "pushDirection");
                y = DrawProp(property, position, y, "pushDistance");
                y = DrawProp(property, position, y, "pushDuration");
                y = DrawProp(property, position, y, "pushPlayerTrigger");
                y = DrawProp(property, position, y, "pushNpcId");
                y = DrawProp(property, position, y, "pushNpcTrigger");
                y = DrawProp(property, position, y, "pushNpcStateName");
                break;
            case CutsceneActionType.SetAnimatorTrigger:
                y = DrawProp(property, position, y, "triggerName");
                break;
            case CutsceneActionType.SetActive:
                y = DrawProp(property, position, y, "targetObject");
                y = DrawProp(property, position, y, "setActive");
                y = DrawProp(property, position, y, "npcId");
                break;
            case CutsceneActionType.SetFlag:
                y = DrawProp(property, position, y, "flagName");
                break;
            case CutsceneActionType.AdvanceStory:
                y = DrawProp(property, position, y, "storyAmount");
                break;
            case CutsceneActionType.AddAffection:
                y = DrawProp(property, position, y, "npcId");
                y = DrawProp(property, position, y, "affectionAmount");
                break;
            case CutsceneActionType.NpcGroupMove:
                y = DrawProp(property, position, y, "npcIds");
                y = DrawProp(property, position, y, "npcTargetPositions");
                y = DrawProp(property, position, y, "usePlayerPositionAsOrigin");
                y = DrawProp(property, position, y, "npcMoveDuration");
                y = DrawProp(property, position, y, "npcSpawnAtCameraEdge");
                y = DrawProp(property, position, y, "npcSpawnMargin");
                break;
            case CutsceneActionType.NpcGroupReturnToStart:
                y = DrawProp(property, position, y, "npcReturnIds");
                y = DrawProp(property, position, y, "npcReturnDuration");
                y = DrawProp(property, position, y, "npcReturnDurationMultiplier");
                break;
            case CutsceneActionType.NpcGroupMoveToPosition:
                y = DrawProp(property, position, y, "npcIds");
                y = DrawProp(property, position, y, "npcTargetPositions");
                y = DrawProp(property, position, y, "usePlayerPositionAsOrigin");
                y = DrawProp(property, position, y, "npcMoveDuration");
                break;
            case CutsceneActionType.NpcTeleportToPosition:
                y = DrawProp(property, position, y, "npcIds");
                y = DrawProp(property, position, y, "npcTargetPositions");
                y = DrawProp(property, position, y, "usePlayerPositionAsOrigin");
                break;
            case CutsceneActionType.ShowNotification:
                y = DrawProp(property, position, y, "notificationText");
                y = DrawProp(property, position, y, "notificationDuration");
                y = DrawProp(property, position, y, "questId");
                break;
            case CutsceneActionType.AcceptQuest:
                y = DrawProp(property, position, y, "questId");
                break;
            case CutsceneActionType.PlayerMoveOffScreen:
                y = DrawProp(property, position, y, "playerWalkOffDuration");
                y = DrawProp(property, position, y, "playerWalkOffMargin");
                break;
            case CutsceneActionType.TeleportPlayer:
                y = DrawProp(property, position, y, "teleportPosition");
                break;
            case CutsceneActionType.GiveItems:
                y = DrawProp(property, position, y, "giveItems");
                break;
            case CutsceneActionType.Custom:
                break;
            default:
                break;
        }

        EditorGUI.EndProperty();
    }

    float DrawProp(SerializedProperty parent, Rect area, float y, string name)
    {
        var prop = parent.FindPropertyRelative(name);
        if (prop == null) return y;
        float h = EditorGUI.GetPropertyHeight(prop, true);
        var rect = new Rect(area.x, y, area.width, h);
        EditorGUI.PropertyField(rect, prop, true);
        return y + h + SPACING;
    }
}
#endif
