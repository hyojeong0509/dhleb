using UnityEngine;

/// <summary>
/// T 키로 샘플 대화 재생 (테스트용). 배치 후 확인용.
/// </summary>
public class DialogueTestTrigger : MonoBehaviour
{
    [Tooltip("테스트할 대화 데이터 (비우면 런타임 샘플 생성)")]
    public DialogueData testDialogue;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T) && DialogueManager.Instance != null && !DialogueManager.Instance.IsPlaying)
        {
            var data = testDialogue != null ? testDialogue : CreateSampleDialogue();
            DialogueManager.Instance.Play(data, () => Debug.Log("[DialogueTest] 대화 종료"));
        }
    }

    static DialogueData CreateSampleDialogue()
    {
        var data = ScriptableObject.CreateInstance<DialogueData>();
        if (data.nodes == null) data.nodes = new System.Collections.Generic.List<DialogueNode>();
        data.startNodeId = "start";
        data.nodes.Add(new DialogueNode
        {
            nodeId = "start",
            speakerName = "트리오네스",
            text = "여기는 루멘시아 행성이고, 나는 AI 인공비서 트리오네스야.",
            nextNodeId = "node2",
            choices = null
        });
        data.nodes.Add(new DialogueNode
        {
            nodeId = "node2",
            speakerName = "트리오네스",
            text = "일단 농사부터 시작해봐. 씨앗을 심어보렴.",
            nextNodeId = "",
            choices = null
        });
        return data;
    }
}
