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
    [Tooltip("Resources 경로 (예: TypingSound 또는 Sounds/TypingSound). 비우면 타이핑 효과음 없음")]
    public string typingSoundPath = "Sounds/TypingSound";

    [Header("UI 참조")]
    public GameObject panel;
    public TMP_Text txtSpeaker;
    public TMP_Text txtDialogue;
    [Tooltip("발화자 초상화 (플레이어 제외 NPC용, Resources/Portraits에서 로드)")]
    public Image imgPortrait;
    public Button btnNext;           // 선택지 없을 때 "다음" 버튼
    public Button btnClose;          // 닫기 버튼 (누르면 dialogueBoxToHide 비활성화)
    public GameObject dialogueBoxToHide; // CloseBtn 누르면 비활성화할 오브젝트 (예: DialogueBox)
    public Transform choiceContainer; // 선택지 부모 (자식으로 Button 생성)
    public GameObject choiceButtonPrefab; // 선택지 버튼 프리팹 (없으면 기본 Button 생성)

    [Header("초상화 (Resources 로드)")]
    [Tooltip("Resources 폴더 내 경로 (예: Portraits). 시작 시 이 경로의 이미지를 전부 로드 (파일명=speakerName)")]
    public string portraitFolder = "Portraits";
    [Tooltip("로드 실패 시 콘솔에 경로 출력 (원인 확인용)")]
    public bool logPortraitLoadFail;

    private System.Collections.Generic.Dictionary<string, Sprite> _portraitCache = new System.Collections.Generic.Dictionary<string, Sprite>();
    private Coroutine typewriterRoutine;
    private bool isTyping;
    private AudioClip _typingSoundClip;
    private AudioSource _typingAudioSource;

    void Start()
    {
        CachePortraits();
        LoadTypingSound();

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

    void OnDialogueEnded(DialogueData _, bool __)
    {
        Hide();
    }

    void OnNodeDisplayed(DialogueNode node)
    {
        UpdateNodeDisplay(node);
    }

    // 노드 표시
    public void ShowAndDisplayNode(DialogueNode node)
    {
        if (node == null) return;
        Show();
        UpdateNodeDisplay(node);
    }

    void UpdateNodeDisplay(DialogueNode node)
    {
        string speaker = node.hideSpeakerName ? "???" : GetDisplaySpeakerName(node.speakerName);
        if (txtSpeaker != null)
            txtSpeaker.text = speaker;

        UpdatePortrait(node.speakerName);

        if (typewriterRoutine != null)
        {
            StopCoroutine(typewriterRoutine);
            typewriterRoutine = null;
        }
        StopTypingSound();
        ClearChoices();

        if (txtDialogue != null && !string.IsNullOrEmpty(node.text))
        {
            if (typewriterDelay > 0)
            {
                isTyping = true;
                typewriterRoutine = StartCoroutine(TypewriterEffect(node.text, node));
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
            if (choiceContainer != null) choiceContainer.gameObject.SetActive(false);
            if (typewriterDelay <= 0 || string.IsNullOrEmpty(node.text))
                ShowChoicesForNode(node);
        }
        else
        {
            if (btnNext != null) btnNext.gameObject.SetActive(true);
            if (choiceContainer != null) choiceContainer.gameObject.SetActive(false);
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

    void LoadTypingSound()
    {
        _typingSoundClip = null;
        if (string.IsNullOrEmpty(typingSoundPath)) return;
        _typingSoundClip = Resources.Load<AudioClip>(typingSoundPath);
        if (_typingSoundClip == null)
            _typingSoundClip = Resources.Load<AudioClip>("Sounds/TypingSound");
    }

    AudioSource GetTypingAudioSource()
    {
        if (_typingAudioSource == null)
        {
            _typingAudioSource = gameObject.AddComponent<AudioSource>();
            _typingAudioSource.playOnAwake = false;
            _typingAudioSource.loop = false;
        }
        if (SoundManager.Instance != null)
            _typingAudioSource.volume = SoundManager.Instance.GetSFXVolume() * SoundManager.Instance.GetMasterVolume();
        return _typingAudioSource;
    }

    void StopTypingSound()
    {
        if (_typingAudioSource != null && _typingAudioSource.isPlaying)
            _typingAudioSource.Stop();
    }

    IEnumerator TypewriterEffect(string fullText, DialogueNode node)
    {
        if (_typingSoundClip != null && fullText.Length > 0)
        {
            var src = GetTypingAudioSource();
            src.clip = _typingSoundClip;
            src.Play();
        }

        for (int i = 0; i <= fullText.Length; i++)
        {
            if (txtDialogue == null) yield break;
            txtDialogue.text = fullText.Substring(0, i);
            if (i < fullText.Length)
                yield return new WaitForSecondsRealtime(typewriterDelay);
        }

        StopTypingSound();
        isTyping = false;
        typewriterRoutine = null;
        if (node != null && node.HasChoices)
            ShowChoicesForNode(node);
    }

    void ShowChoicesForNode(DialogueNode node)
    {
        if (node == null || !node.HasChoices || choiceContainer == null) return;
        if (DialogueManager.Instance == null || DialogueManager.Instance.CurrentNode != node) return;
        choiceContainer.gameObject.SetActive(true);
        foreach (var choice in node.choices)
        {
            var btn = CreateChoiceButton(choice);
            if (btn != null)
                btn.transform.SetParent(choiceContainer, false);
        }
    }

    void SkipTypewriter()
    {
        var node = DialogueManager.Instance?.CurrentNode;
        if (typewriterRoutine != null)
        {
            StopCoroutine(typewriterRoutine);
            typewriterRoutine = null;
        }
        StopTypingSound();
        if (txtDialogue != null && node != null)
            txtDialogue.text = node.text;
        isTyping = false;
        if (node != null && node.HasChoices)
            ShowChoicesForNode(node);
    }

    // Resources에서 초상화 로드 후 imgPortrait에 표시
    void UpdatePortrait(string speakerName)
    {
        if (imgPortrait == null) return;

        string resourceName = GetPortraitResourceName(speakerName);
        if (string.IsNullOrEmpty(resourceName))
        {
            imgPortrait.gameObject.SetActive(false);
            return;
        }

        string path = string.IsNullOrEmpty(portraitFolder)
            ? resourceName
            : $"{portraitFolder}/{resourceName}";
        var sprite = GetCachedPortrait(resourceName);
        if (sprite == null)
            sprite = Resources.Load<Sprite>(path);
        if (sprite != null)
        {
            imgPortrait.sprite = sprite;
            imgPortrait.gameObject.SetActive(true);
        }
        else
        {
            if (logPortraitLoadFail)
                Debug.LogWarning($"[DialogueUI] 초상화 로드 실패: speakerName=\"{speakerName}\" → Resources/{path} (imgPortrait 할당, Portrait Folder, Texture Type=Sprite 확인)");
            imgPortrait.gameObject.SetActive(false);
        }
    }

    // 시작 시 portraitFolder에서 이미지 전부 로드 (파일명=키)
    void CachePortraits()
    {
        _portraitCache.Clear();
        if (string.IsNullOrEmpty(portraitFolder)) return;

        var sprites = Resources.LoadAll<Sprite>(portraitFolder);
        foreach (var s in sprites)
        {
            if (s == null) continue;
            var key = s.texture != null ? s.texture.name : s.name;
            if (string.IsNullOrEmpty(key)) key = s.name;
            if (!string.IsNullOrEmpty(key) && !_portraitCache.ContainsKey(key))
                _portraitCache[key] = s;
        }
    }

    Sprite GetCachedPortrait(string resourceName)
    {
        if (string.IsNullOrEmpty(resourceName)) return null;
        return _portraitCache != null && _portraitCache.TryGetValue(resourceName, out var s) ? s : null;
    }

    // speakerName = 파일명
    string GetPortraitResourceName(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return "";
        if (raw == "{Player}" || raw == "{PlayerName}") return "Player";
        if (raw == "{npc}" && DialogueManager.Instance != null && !string.IsNullOrEmpty(DialogueManager.Instance.CurrentNpcIdForPortrait))
            return DialogueManager.Instance.CurrentNpcIdForPortrait;
        return raw;
    }

    // 발화자 이름 (플레이어는 저장된 이름으로 치환)
    string GetDisplaySpeakerName(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return "";
        if (raw == "{Player}" || raw == "{PlayerName}")
        {
            if (GameDataManager.Instance?.currentSaveData?.playerData != null)
                return GameDataManager.Instance.currentSaveData.playerData.playerName;
            return "나";
        }
        if (raw == "{npc}")
        {
            if (DialogueManager.Instance != null && !string.IsNullOrEmpty(DialogueManager.Instance.CurrentNpcDisplayName))
                return DialogueManager.Instance.CurrentNpcDisplayName;
            return "???";
        }
        return raw;
    }

    void OnCloseClicked()
    {
        if (dialogueBoxToHide != null)
            dialogueBoxToHide.SetActive(false);

        if (DialogueManager.Instance != null)
            DialogueManager.Instance.EndDialogue(completed: false);
    }

    Button CreateChoiceButton(DialogueChoice choice)
    {
        if (choiceButtonPrefab == null)
        {
            Debug.LogWarning("[DialogueUI] Choice Button Prefab이 할당되지 않았습니다. Inspector에서 할당해주세요.");
            return null;
        }

        var go = Instantiate(choiceButtonPrefab);
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
        choiceContainer.gameObject.SetActive(false);
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

    // 패널 숨기기
    public void HidePanel()
    {
        if (typewriterRoutine != null)
        {
            StopCoroutine(typewriterRoutine);
            typewriterRoutine = null;
        }
        StopTypingSound();
        isTyping = false;
        if (panel != null) panel.SetActive(false);
        if (dialogueBoxToHide != null) dialogueBoxToHide.SetActive(false);
        ClearChoices();
    }

}
