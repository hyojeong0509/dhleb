using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 씬 간 데이터 전달 및 저장을 담당하는 싱글톤 매니저
/// </summary>
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

    [Header("현재 커스터마이징 데이터")]
    public PlayerCustomizationData currentCustomization;

    [Header("설정")]
    [Tooltip("게임 시작 시 자동으로 저장된 데이터 로드")]
    public bool autoLoadOnStart = true;

    private const string SAVE_KEY_PLAYER_NAME = "PlayerName";
    private const string SAVE_KEY_SKIN_COLOR = "SkinColor";
    private const string SAVE_KEY_HAIR_COLOR = "HairColor";
    private const string SAVE_KEY_OUTFIT_COLOR = "OutfitColor";
    private const string SAVE_KEY_CHARACTER_PRESET = "CharacterPreset";

    void Awake()
    {
        // 싱글톤 패턴
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
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

    /// <summary>
    /// 커스터마이징 데이터를 저장 (PlayerPrefs 사용)
    /// </summary>
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
        
        PlayerPrefs.SetInt(SAVE_KEY_CHARACTER_PRESET, data.characterPreset);
        PlayerPrefs.Save();

        Debug.Log($"GameDataManager: 커스터마이징 데이터 저장 완료 - 이름: {data.playerName}");
    }

    /// <summary>
    /// 저장된 커스터마이징 데이터를 로드
    /// </summary>
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
        
        currentCustomization.characterPreset = PlayerPrefs.GetInt(SAVE_KEY_CHARACTER_PRESET, 0);

        Debug.Log($"GameDataManager: 커스터마이징 데이터 로드 완료 - 이름: {currentCustomization.playerName}");
    }

    /// <summary>
    /// 저장된 커스터마이징 데이터를 삭제
    /// </summary>
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
        PlayerPrefs.DeleteKey(SAVE_KEY_CHARACTER_PRESET);
        PlayerPrefs.Save();
        
        currentCustomization = new PlayerCustomizationData();
        Debug.Log("GameDataManager: 커스터마이징 데이터 삭제 완료");
    }

    /// <summary>
    /// Game 씬으로 이동 (커스터마이징 적용)
    /// </summary>
    public void LoadGameScene()
    {
        // 커스터마이징 데이터 저장
        if (currentCustomization != null)
        {
            SaveCustomization(currentCustomization);
        }

        // GameSceneManager를 통해 씬 전환 (로딩 씬 사용)
        if (GameSceneManager.Instance != null)
        {
            GameSceneManager.Instance.LoadGameScene();
        }
        else
        {
            // GameSceneManager가 없으면 직접 전환
            UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
        }
    }
}





