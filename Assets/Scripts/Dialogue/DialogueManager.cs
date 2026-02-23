using UnityEngine;
using System;

/// <summary>
/// 대화 재생 관리. 대화 중 시간 정지.
/// </summary>
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    public bool IsPlaying { get; private set; }

    public event Action<DialogueData> OnDialogueStarted;
    public event Action<DialogueData> OnDialogueEnded;
    public event Action<DialogueNode> OnNodeDisplayed;

    private DialogueData currentData;
    private DialogueNode currentNode;
    private Action onCompleteCallback;

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

    /// <summary>
    /// 대화 시작. 완료 시 onComplete 호출.
    /// </summary>
    public void Play(DialogueData data, Action onComplete = null)
    {
        if (data == null || data.nodes == null || data.nodes.Count == 0)
        {
            onComplete?.Invoke();
            return;
        }

        if (IsPlaying)
        {
            Debug.LogWarning("[DialogueManager] 이미 대화 진행 중");
            return;
        }

        currentData = data;
        onCompleteCallback = onComplete;
        IsPlaying = true;

        if (TimeManager.Instance != null)
            TimeManager.Instance.SetPause(true);

        OnDialogueStarted?.Invoke(data);
        ShowNode(data.startNodeId);
    }

    /// <summary>
    /// 다음 노드로 진행 (nextNodeId 또는 선택지의 nextNodeId)
    /// </summary>
    public void Advance(string nextNodeId)
    {
        if (!IsPlaying || currentData == null) return;

        if (string.IsNullOrEmpty(nextNodeId))
        {
            EndDialogue();
            return;
        }

        var node = currentData.GetNode(nextNodeId);
        if (node == null)
        {
            EndDialogue();
            return;
        }

        ShowNode(nextNodeId);
    }

    /// <summary>
    /// 현재 대화 강제 종료
    /// </summary>
    public void EndDialogue()
    {
        if (!IsPlaying) return;

        IsPlaying = false;
        var data = currentData;
        currentData = null;
        currentNode = null;

        if (TimeManager.Instance != null)
            TimeManager.Instance.SetPause(false);

        var all = FindObjectsOfType<DialogueUI>(true);
        foreach (var ui in all)
            ui.HidePanel();

        OnDialogueEnded?.Invoke(data);
        onCompleteCallback?.Invoke();
        onCompleteCallback = null;
    }

    void ShowNode(string nodeId)
    {
        var node = currentData.GetNode(nodeId);
        if (node == null)
        {
            EndDialogue();
            return;
        }

        currentNode = node;
        TryUpdateDialogueUI(node);
        OnNodeDisplayed?.Invoke(node);
    }

    /// <summary>DialogueUI가 비활성 오브젝트에 있어도 직접 UI 갱신 (폴백)</summary>
    void TryUpdateDialogueUI(DialogueNode node)
    {
        var all = FindObjectsOfType<DialogueUI>(true);
        DialogueUI ui = null;
        foreach (var u in all)
        {
            if (u.panel != null)
            {
                ui = u;
                break;
            }
        }
        if (ui == null && all.Length > 0) ui = all[0];
        if (ui != null && node != null)
            ui.ShowAndDisplayNode(node);
    }

    /// <summary>현재 표시 중인 노드</summary>
    public DialogueNode CurrentNode => currentNode;
}
