using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

// 씬 간 데이터 전달 및 저장을 담당하는 싱글톤 매니저
public class GameDataManager : MonoBehaviour
{
    private static GameDataManager _instance;
    public static GameDataManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameDataManager>();
                
                if (_instance == null)
                {
                    GameObject go = new GameObject("GameDataManager");
                    _instance = go.AddComponent<GameDataManager>();
                }
            }
            return _instance;
        }
    }

    [Header("현재 세이브 데이터")]
    public SaveData currentSaveData; // 현재 플레이 중인 슬롯의 전체 데이터

    [Header("플레이어 (저장 시 위치 수집용, 비우면 자동 탐색)")]
    public Transform playerTransform;

    [Header("현재 커스터마이징 데이터")]
    public PlayerCustomizationData currentCustomization;

    [Header("설정")]
    [Tooltip("게임 시작 시 자동으로 저장된 데이터 로드")]
    public bool autoLoadOnStart = true;

    private const string SAVE_KEY_PLAYER_NAME = "PlayerName";
    private const string SAVE_KEY_SKIN_COLOR = "SkinColor";
    private const string SAVE_KEY_HAIR_COLOR = "HairColor";
    private const string SAVE_KEY_OUTFIT_COLOR = "OutfitColor";
    private const string SAVE_KEY_SKIN_TONE_PRESET = "SkinTonePreset";

    void Awake()
    {
        // 싱글톤 패턴
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;

            // 커스터마이징 데이터 초기화
            currentCustomization = new PlayerCustomizationData();

            if (autoLoadOnStart)
            {
                LoadCustomization();
            }
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>ESC→타이틀 전에 호출. Game씬 전용 DontDestroyOnLoad 매니저 제거 → 재로드 시 깨끗한 상태로 시작</summary>
    public static void CleanupGameManagersBeforeTitle()
    {
        if (QuestManager.Instance != null) { Destroy(QuestManager.Instance.gameObject); }
        if (GameProgressManager.Instance != null) { Destroy(GameProgressManager.Instance.gameObject); }
        if (DialogueManager.Instance != null) { Destroy(DialogueManager.Instance.gameObject); }
        if (EventManager.Instance != null) { Destroy(EventManager.Instance.gameObject); }
        if (CutsceneManager.Instance != null) { Destroy(CutsceneManager.Instance.gameObject); }
    }

    /// <summary>
    /// Game 씬 로드 시 저장 데이터 적용. GameSceneInitializer와 병렬로 실행되며,
    /// 매니저 준비 후 ApplyLoadData 실행 (GameSceneInitializer 미실행 시 백업)
    /// </summary>
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "Game") return;
        if (currentSaveData == null) return;
        StartCoroutine(ApplyLoadDataWhenReady());
    }

    IEnumerator ApplyLoadDataWhenReady()
    {
        yield return new WaitForEndOfFrame();
        int slotIndex = currentSaveData.slotIndex;
        if (SaveManager.Instance != null)
        {
            var freshData = SaveManager.Instance.Load(slotIndex);
            if (freshData != null) currentSaveData = freshData;
        }
        int waitCount = 0;
        while (waitCount < 120)
        {
            bool inv = InventoryManager.Instance != null;
            bool tile = TileManager.Instance != null;
            bool natural = NaturalObjectManager.Instance != null;
            bool quest = QuestManager.Instance != null;
            if (inv && tile && natural && quest) break;
            if (waitCount >= 60 && inv && tile && natural) break;
            yield return null;
            waitCount++;
        }
        TryActivateInactiveManagers();
        var player = GameObject.FindGameObjectWithTag("Player")?.transform;
        GameSaveHelper.ApplyLoadData(currentSaveData, player);
    }

    void TryActivateInactiveManagers()
    {
        if (InventoryManager.Instance != null && TileManager.Instance != null && NaturalObjectManager.Instance != null) return;
        var inv = FindObjectOfType<InventoryManager>(true);
        if (inv != null && !inv.gameObject.activeInHierarchy)
            inv.gameObject.SetActive(true);
    }

    // 커스터마이징 데이터를 저장
    public void SaveCustomization(PlayerCustomizationData data)
    {
        if (data == null)
        {
            Debug.LogWarning("GameDataManager: 저장할 커스터마이징 데이터가 null입니다.");
            return;
        }

        currentCustomization = new PlayerCustomizationData(data);

        // PlayerPrefs에 저장
        PlayerPrefs.SetString(SAVE_KEY_PLAYER_NAME, data.playerName);
        PlayerPrefs.SetFloat(SAVE_KEY_SKIN_COLOR + "_R", data.skinColor.r);
        PlayerPrefs.SetFloat(SAVE_KEY_SKIN_COLOR + "_G", data.skinColor.g);
        PlayerPrefs.SetFloat(SAVE_KEY_SKIN_COLOR + "_B", data.skinColor.b);
        PlayerPrefs.SetFloat(SAVE_KEY_SKIN_COLOR + "_A", data.skinColor.a);
        
        PlayerPrefs.SetFloat(SAVE_KEY_HAIR_COLOR + "_R", data.hairColor.r);
        PlayerPrefs.SetFloat(SAVE_KEY_HAIR_COLOR + "_G", data.hairColor.g);
        PlayerPrefs.SetFloat(SAVE_KEY_HAIR_COLOR + "_B", data.hairColor.b);
        PlayerPrefs.SetFloat(SAVE_KEY_HAIR_COLOR + "_A", data.hairColor.a);
        
        PlayerPrefs.SetFloat(SAVE_KEY_OUTFIT_COLOR + "_R", data.outfitColor.r);
        PlayerPrefs.SetFloat(SAVE_KEY_OUTFIT_COLOR + "_G", data.outfitColor.g);
        PlayerPrefs.SetFloat(SAVE_KEY_OUTFIT_COLOR + "_B", data.outfitColor.b);
        PlayerPrefs.SetFloat(SAVE_KEY_OUTFIT_COLOR + "_A", data.outfitColor.a);
        
        PlayerPrefs.SetInt(SAVE_KEY_SKIN_TONE_PRESET, data.skinTonePreset);
        PlayerPrefs.Save();

        Debug.Log($"GameDataManager: 커스터마이징 데이터 저장 완료 - 이름: {data.playerName}");
    }

    // 저장된 커스터마이징 데이터를 로드
    public void LoadCustomization()
    {
        if (!PlayerPrefs.HasKey(SAVE_KEY_PLAYER_NAME))
        {
            Debug.Log("GameDataManager: 저장된 커스터마이징 데이터가 없습니다. 기본값을 사용합니다.");
            currentCustomization = new PlayerCustomizationData();
            return;
        }

        currentCustomization = new PlayerCustomizationData();
        currentCustomization.playerName = PlayerPrefs.GetString(SAVE_KEY_PLAYER_NAME, "플레이어");
        
        float r = PlayerPrefs.GetFloat(SAVE_KEY_SKIN_COLOR + "_R", 1f);
        float g = PlayerPrefs.GetFloat(SAVE_KEY_SKIN_COLOR + "_G", 1f);
        float b = PlayerPrefs.GetFloat(SAVE_KEY_SKIN_COLOR + "_B", 1f);
        float a = PlayerPrefs.GetFloat(SAVE_KEY_SKIN_COLOR + "_A", 1f);
        currentCustomization.skinColor = new Color(r, g, b, a);
        
        r = PlayerPrefs.GetFloat(SAVE_KEY_HAIR_COLOR + "_R", 1f);
        g = PlayerPrefs.GetFloat(SAVE_KEY_HAIR_COLOR + "_G", 1f);
        b = PlayerPrefs.GetFloat(SAVE_KEY_HAIR_COLOR + "_B", 1f);
        a = PlayerPrefs.GetFloat(SAVE_KEY_HAIR_COLOR + "_A", 1f);
        currentCustomization.hairColor = new Color(r, g, b, a);
        
        r = PlayerPrefs.GetFloat(SAVE_KEY_OUTFIT_COLOR + "_R", 1f);
        g = PlayerPrefs.GetFloat(SAVE_KEY_OUTFIT_COLOR + "_G", 1f);
        b = PlayerPrefs.GetFloat(SAVE_KEY_OUTFIT_COLOR + "_B", 1f);
        a = PlayerPrefs.GetFloat(SAVE_KEY_OUTFIT_COLOR + "_A", 1f);
        currentCustomization.outfitColor = new Color(r, g, b, a);
        
        currentCustomization.skinTonePreset = PlayerPrefs.GetInt(SAVE_KEY_SKIN_TONE_PRESET, 1); // 기본값: 1 (보통 톤)

        Debug.Log($"GameDataManager: 커스터마이징 데이터 로드 완료 - 이름: {currentCustomization.playerName}");
    }

    // 저장된 커스터마이징 데이터를 삭제
    public void ClearCustomization()
    {
        PlayerPrefs.DeleteKey(SAVE_KEY_PLAYER_NAME);
        PlayerPrefs.DeleteKey(SAVE_KEY_SKIN_COLOR + "_R");
        PlayerPrefs.DeleteKey(SAVE_KEY_SKIN_COLOR + "_G");
        PlayerPrefs.DeleteKey(SAVE_KEY_SKIN_COLOR + "_B");
        PlayerPrefs.DeleteKey(SAVE_KEY_SKIN_COLOR + "_A");
        PlayerPrefs.DeleteKey(SAVE_KEY_HAIR_COLOR + "_R");
        PlayerPrefs.DeleteKey(SAVE_KEY_HAIR_COLOR + "_G");
        PlayerPrefs.DeleteKey(SAVE_KEY_HAIR_COLOR + "_B");
        PlayerPrefs.DeleteKey(SAVE_KEY_HAIR_COLOR + "_A");
        PlayerPrefs.DeleteKey(SAVE_KEY_OUTFIT_COLOR + "_R");
        PlayerPrefs.DeleteKey(SAVE_KEY_OUTFIT_COLOR + "_G");
        PlayerPrefs.DeleteKey(SAVE_KEY_OUTFIT_COLOR + "_B");
        PlayerPrefs.DeleteKey(SAVE_KEY_OUTFIT_COLOR + "_A");
        PlayerPrefs.DeleteKey(SAVE_KEY_SKIN_TONE_PRESET);
        PlayerPrefs.Save();
        
        currentCustomization = new PlayerCustomizationData();
        Debug.Log("GameDataManager: 커스터마이징 데이터 삭제 완료");
    }

    void OnApplicationQuit()
    {
        if (currentSaveData != null)
            SaveGame();
    }

    /// <summary>
    /// 현재 게임 상태 저장 (침대 등에서 호출)
    /// </summary>
    public void SaveGame()
    {
        if (currentSaveData == null) return;

        Transform player = playerTransform != null ? playerTransform : GameObject.FindGameObjectWithTag("Player")?.transform;
        GameSaveHelper.CollectSaveData(currentSaveData, player);

        if (SaveManager.Instance != null)
            SaveManager.Instance.Save(currentSaveData);
    }

    // Game 씬으로 이동 (커스터마이징 적용)
    public void LoadGameScene()
    {
        if (currentCustomization != null)
            SaveCustomization(currentCustomization);

        if (GameSceneManager.Instance != null)
        {
            if (currentSaveData != null)
                GameSceneManager.Instance.LoadGameSceneDirect();
            else
                GameSceneManager.Instance.LoadGameScene();
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
        }
    }
}





