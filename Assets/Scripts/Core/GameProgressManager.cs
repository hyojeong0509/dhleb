using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 스토리 진행도, NPC 호감도, 이벤트 플래그를 런타임에서 관리.
/// SaveData와 연동되며, EventManager/QuestManager 등에서 사용.
/// </summary>
public class GameProgressManager : MonoBehaviour
{
    public static GameProgressManager Instance { get; private set; }

    private int storyProgress;
    private Dictionary<string, int> npcAffections = new Dictionary<string, int>();
    private HashSet<string> eventFlags = new HashSet<string>();

    // ── StoryProgress ────────────────────────────────────────

    public int StoryProgress => storyProgress;

    public void SetStoryProgress(int value)
    {
        storyProgress = Mathf.Max(0, value);
    }

    public void AdvanceStoryProgress(int amount = 1)
    {
        storyProgress = Mathf.Max(0, storyProgress + amount);
    }

    /// <summary>튜토리얼(첫날 스토리) 이전인지</summary>
    public bool IsBeforeTutorialEnd => storyProgress < 7;

    // ── NPC Affection ────────────────────────────────────────

    public int GetAffection(string npcId)
    {
        return npcAffections.TryGetValue(npcId, out int v) ? v : 0;
    }

    public void SetAffection(string npcId, int value)
    {
        npcAffections[npcId] = Mathf.Clamp(value, 0, 100);
    }

    public void AddAffection(string npcId, int amount)
    {
        int current = GetAffection(npcId);
        SetAffection(npcId, current + amount);
    }

    // ── Event Flags ───────────────────────────────────────────

    public bool HasFlag(string flag)
    {
        return eventFlags.Contains(flag);
    }

    public void SetFlag(string flag)
    {
        eventFlags.Add(flag);
        QuestManager.Instance?.NotifyFlagSet(flag);
    }

    public void RemoveFlag(string flag)
    {
        eventFlags.Remove(flag);
    }

    public void ClearAllFlags()
    {
        eventFlags.Clear();
    }

    // ── Save/Load 연동 ────────────────────────────────────────

    /// <summary>SaveData에서 로드 (GameSaveHelper에서 호출)</summary>
    public void LoadFromSaveData(GameProgressData data)
    {
        if (data == null) return;

        storyProgress = data.storyProgress;
        npcAffections.Clear();
        foreach (var entry in data.npcAffections)
        {
            if (!string.IsNullOrEmpty(entry.npcId))
                npcAffections[entry.npcId] = entry.affection;
        }
        eventFlags.Clear();
        foreach (var flag in data.eventFlags)
        {
            if (!string.IsNullOrEmpty(flag))
                eventFlags.Add(flag);
        }
    }

    /// <summary>SaveData에 저장 (GameSaveHelper에서 호출)</summary>
    public void FillSaveData(GameProgressData data)
    {
        if (data == null) return;

        data.storyProgress = storyProgress;
        data.npcAffections.Clear();
        foreach (var kv in npcAffections)
            data.npcAffections.Add(new NpcAffectionSaveData { npcId = kv.Key, affection = kv.Value });
        data.eventFlags.Clear();
        data.eventFlags.AddRange(eventFlags);
    }

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
}
