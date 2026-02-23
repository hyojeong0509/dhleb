using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// 대화창 UI. DialogueManager와 연동.
/// 패널, 발화자 이름, 대사 텍스트, 다음/선택 버튼 필요.
/// </summary>
public class DialogueUI : MonoBehaviour
{
    [Header("타이핑 효과")]
    [Tooltip("한 글자당 대기 시간 (초). 0이면 효과 없음")]
    public float typewriterDelay = 0.05f;

    [Header("UI 참조")]
    public GameObject panel;
    public TMP_Text txtSpeaker;
    public TMP_Text txtDialogue;
    public Button btnNext;           // 선택지 없을 때 "다음" 버튼
    public Button btnClose;          // 닫기 버튼 (누르면 dialogueBoxToHide 비활성화)
    public GameObject dialogueBoxToHide; // CloseBtn 누르면 비활성화할 오브젝트 (예: DialogueBox)
    public Transform choiceContainer; // 선택지 부모 (자식으로 Button 생성)
    public GameObject choiceButtonPrefab; // 선택지 버튼 프리팹 (없으면 기본 Button 생성)

    private Coroutine typewriterRoutine;
    private bool isTyping;

    void Start()
    {
        if (DialogueManager.Instance == null) return;

        DialogueManager.Instance.OnDialogueStarted += OnDialogueStarted;
        DialogueManager.Instance.OnDialogueEnded += OnDialogueEnded;
        DialogueManager.Instance.OnNodeDisplayed += OnNodeDisplayed;

        if (btnNext != null)
            btnNext.onClick.AddListener(OnNextClicked);

        if (btnClose != null)
            btnClose.onClick.AddListener(OnCloseClicked);

        Hide();
    }

    void OnDestroy()
    {
        if (DialogueManager.Instance == null) return;

        DialogueManager.Instance.OnDialogueStarted -= OnDialogueStarted;
        DialogueManager.Instance.OnDialogueEnded -= OnDialogueEnded;
        DialogueManager.Instance.OnNodeDisplayed -= OnNodeDisplayed;
    }

    void OnDialogueStarted(DialogueData _)
    {
        Show();
    }

    void OnDialogueEnded(DialogueData _)
    {
        Hide();
    }

    void OnNodeDisplayed(DialogueNode node)
    {
        UpdateNodeDisplay(node);
    }

    /// <summary>노드 표시 (DialogueManager 폴백용 - 비활성 오브젝트에 있어도 호출됨)</summary>
    public void ShowAndDisplayNode(DialogueNode node)
    {
        if (node == null) return;
        Show();
        UpdateNodeDisplay(node);
    }

    void UpdateNodeDisplay(DialogueNode node)
    {
        string speaker = GetDisplaySpeakerName(node.speakerName);
        if (txtSpeaker != null)
            txtSpeaker.text = speaker;

        if (typewriterRoutine != null)
            StopCoroutine(typewriterRoutine);

        ClearChoices();

        if (txtDialogue != null && !string.IsNullOrEmpty(node.text))
        {
            if (typewriterDelay > 0)
            {
                isTyping = true;
                typewriterRoutine = StartCoroutine(TypewriterEffect(node.text));
            }
            else
            {
                txtDialogue.text = node.text;
                isTyping = false;
            }
        }
        else if (txtDialogue != null)
        {
            txtDialogue.text = "";
            isTyping = false;
        }

        if (node.HasChoices)
        {
            if (btnNext != null) btnNext.gameObject.SetActive(false);
            if (choiceContainer != null)
            {
                foreach (var choice in node.choices)
                {
                    var btn = CreateChoiceButton(choice);
                    if (btn != null) btn.transform.SetParent(choiceContainer, false);
                }
            }
        }
        else
        {
            if (btnNext != null) btnNext.gameObject.SetActive(true);
        }
    }

    void OnNextClicked()
    {
        if (DialogueManager.Instance == null || !DialogueManager.Instance.IsPlaying) return;
        var node = DialogueManager.Instance.CurrentNode;
        if (node == null) return;

        if (isTyping)
        {
            SkipTypewriter();
            return;
        }
        DialogueManager.Instance.Advance(node.nextNodeId);
    }

    IEnumerator TypewriterEffect(string fullText)
    {
        for (int i = 0; i <= fullText.Length; i++)
        {
            if (txtDialogue == null) yield break;
            txtDialogue.text = fullText.Substring(0, i);
            if (i < fullText.Length)
                yield return new WaitForSecondsRealtime(typewriterDelay);
        }
        isTyping = false;
        typewriterRoutine = null;
    }

    void SkipTypewriter()
    {
        if (typewriterRoutine != null)
        {
            StopCoroutine(typewriterRoutine);
            typewriterRoutine = null;
        }
        if (txtDialogue != null && DialogueManager.Instance != null && DialogueManager.Instance.CurrentNode != null)
            txtDialogue.text = DialogueManager.Instance.CurrentNode.text;
        isTyping = false;
    }

    /// <summary>발화자 이름 (플레이어는 저장된 이름으로 치환)</summary>
    string GetDisplaySpeakerName(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return "";
        if (raw == "{Player}" || raw == "{PlayerName}")
        {
            if (GameDataManager.Instance?.currentSaveData?.playerData != null)
                return GameDataManager.Instance.currentSaveData.playerData.playerName;
            return "나";
        }
        return raw;
    }

    void OnCloseClicked()
    {
        if (dialogueBoxToHide != null)
            dialogueBoxToHide.SetActive(false);

        if (DialogueManager.Instance != null)
            DialogueManager.Instance.EndDialogue();
    }

    Button CreateChoiceButton(DialogueChoice choice)
    {
        GameObject go;
        if (choiceButtonPrefab != null)
        {
            go = Instantiate(choiceButtonPrefab);
        }
        else
        {
            go = new GameObject("ChoiceButton");
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(400, 36);
            var img = go.AddComponent<Image>();
            img.color = new Color(0.25f, 0.25f, 0.3f, 0.95f);
            go.AddComponent<Button>();

            var child = new GameObject("Text");
            child.transform.SetParent(go.transform, false);
            var childRect = child.AddComponent<RectTransform>();
            childRect.anchorMin = Vector2.zero;
            childRect.anchorMax = Vector2.one;
            childRect.offsetMin = new Vector2(12, 4);
            childRect.offsetMax = new Vector2(-12, -4);
            var tmp = child.AddComponent<TextMeshProUGUI>();
            tmp.text = choice.text;
            tmp.fontSize = 16;
            tmp.alignment = TextAlignmentOptions.Left;
        }

        var button = go.GetComponent<Button>();
        if (button == null) button = go.AddComponent<Button>();

        var choiceTxt = go.GetComponentInChildren<TMP_Text>();
        if (choiceTxt != null) choiceTxt.text = choice.text;

        string nextId = choice.nextNodeId;
        button.onClick.AddListener(() => DialogueManager.Instance.Advance(nextId));

        return button;
    }

    void ClearChoices()
    {
        if (choiceContainer == null) return;
        for (int i = choiceContainer.childCount - 1; i >= 0; i--)
            Destroy(choiceContainer.GetChild(i).gameObject);
    }

    void Show()
    {
        if (panel != null) panel.SetActive(true);
        if (dialogueBoxToHide != null) dialogueBoxToHide.SetActive(true);
    }

    void Hide()
    {
        HidePanel();
    }

    /// <summary>패널 숨기기 (DialogueManager 폴백용)</summary>
    public void HidePanel()
    {
        if (typewriterRoutine != null)
        {
            StopCoroutine(typewriterRoutine);
            typewriterRoutine = null;
        }
        isTyping = false;
        if (panel != null) panel.SetActive(false);
        if (dialogueBoxToHide != null) dialogueBoxToHide.SetActive(false);
        ClearChoices();
    }

    void Update()
    {
        if (isTyping && Input.GetMouseButtonDown(0))
            SkipTypewriter();
    }
}
