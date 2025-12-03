using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    private static SoundManager _instance;
    public static SoundManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<SoundManager>();
                
                if (_instance == null)
                {
                    GameObject go = new GameObject("SoundManager");
                    _instance = go.AddComponent<SoundManager>();
                }
            }
            return _instance;
        }
    }

    [Header("Audio Source 설정")]
    [Tooltip("배경음악용 AudioSource")]
    [SerializeField] private AudioSource bgmSource;
    
    [Tooltip("효과음용 AudioSource")]
    [SerializeField] private AudioSource sfxSource;

    [Header("볼륨 설정")]
    [Range(0f, 1f)]
    [Tooltip("전체 볼륨")]
    [SerializeField] private float masterVolume = 1.0f;
    
    [Range(0f, 1f)]
    [Tooltip("배경음악 볼륨")]
    [SerializeField] private float bgmVolume = 0.7f;
    
    [Range(0f, 1f)]
    [Tooltip("효과음 볼륨")]
    [SerializeField] private float sfxVolume = 0.8f;

    [Header("씬 이름 설정")]
    [Tooltip("타이틀 씬 이름")]
    [SerializeField] private string titleSceneName = "Title";
    
    [Tooltip("게임메인 씬 이름")]
    [SerializeField] private string gameSceneName = "Game";

    [Header("자동 재생 설정")]
    [Tooltip("씬 변경 시 자동으로 배경음악 재생")]
    [SerializeField] private bool autoPlayBGMOnSceneLoad = true;

    // Resources에서 로드할 오디오 클립
    private AudioClip titleBGM;
    private AudioClip gameMainBGM;
    private AudioClip clickSound;

    // PlayerPrefs 키
    private const string MASTER_VOLUME_KEY = "MasterVolume";
    private const string BGM_VOLUME_KEY = "BGMVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";

    void Awake()
    {
        // 싱글톤 패턴
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            // AudioSource 초기화
            InitializeAudioSources();
            
            // Resources에서 오디오 클립 로드
            LoadAudioClips();
            
            // 저장된 볼륨 설정 로드
            LoadVolumeSettings();
            
            // 씬 변경 이벤트 구독
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        // 이벤트 구독 해제
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 씬이 로드될 때 호출되는 콜백
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!autoPlayBGMOnSceneLoad)
            return;

        // 현재 씬 이름에 따라 배경음악 재생
        if (scene.name == titleSceneName)
        {
            PlayTitleBGM();
        }
        else if (scene.name == gameSceneName)
        {
            PlayGameMainBGM();
        }
    }

    // Resources 폴더에서 오디오 클립 로드
    private void LoadAudioClips()
    {
        // 타이틀 배경음악 로드
        titleBGM = Resources.Load<AudioClip>("Sounds/TitleBGM");
        if (titleBGM == null)
        {
            Debug.LogWarning("SoundManager: Resources/Sounds/TitleBGM을 찾을 수 없습니다.");
        }

        // 게임메인 배경음악 로드
        gameMainBGM = Resources.Load<AudioClip>("Sounds/MainBGM");
        if (gameMainBGM == null)
        {
            Debug.LogWarning("SoundManager: Resources/Sounds/MainBGM을 찾을 수 없습니다.");
        }

        // 클릭 효과음 로드
        clickSound = Resources.Load<AudioClip>("Sounds/ClickSound");
        if (clickSound == null)
        {
            Debug.LogWarning("SoundManager: Resources/Sounds/ClickSound를 찾을 수 없습니다.");
        }
    }

    // AudioSource 컴포넌트 초기화
    private void InitializeAudioSources()
    {
        // BGM AudioSource 초기화
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
            bgmSource.volume = bgmVolume * masterVolume;
        }

        // SFX AudioSource 초기화
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
            sfxSource.volume = sfxVolume * masterVolume;
        }
    }

    // 타이틀 배경음악 재생
    public void PlayTitleBGM()
    {
        if (titleBGM == null)
        {
            Debug.LogWarning("SoundManager: 타이틀 배경음악 클립이 설정되지 않았습니다.");
            return;
        }

        if (bgmSource == null)
        {
            InitializeAudioSources();
        }

        // 같은 음악이 재생 중이면 재생하지 않음
        if (bgmSource.clip == titleBGM && bgmSource.isPlaying)
        {
            return;
        }

        bgmSource.clip = titleBGM;
        bgmSource.volume = bgmVolume * masterVolume;
        bgmSource.Play();
    }

    // 게임메인 배경음악 재생
    public void PlayGameMainBGM()
    {
        if (gameMainBGM == null)
        {
            Debug.LogWarning("SoundManager: 게임메인 배경음악 클립이 설정되지 않았습니다.");
            return;
        }

        if (bgmSource == null)
        {
            InitializeAudioSources();
        }

        // 같은 음악이 재생 중이면 재생하지 않음
        if (bgmSource.clip == gameMainBGM && bgmSource.isPlaying)
        {
            return;
        }

        bgmSource.clip = gameMainBGM;
        bgmSource.volume = bgmVolume * masterVolume;
        bgmSource.Play();
    }

    // 효과음 재생
    /// <param name="clip">재생할 효과음 클립</param>
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("SoundManager: 효과음 클립이 null입니다.");
            return;
        }

        if (sfxSource == null)
        {
            InitializeAudioSources();
        }

        sfxSource.PlayOneShot(clip, sfxVolume * masterVolume);
    }

    // 클릭 효과음 재생
    public void PlayClickSound()
    {
        if (clickSound == null)
        {
            Debug.LogWarning("SoundManager: 클릭 효과음 클립이 설정되지 않았습니다.");
            return;
        }

        PlaySFX(clickSound);
    }

    // 배경음악 정지
    public void StopBGM()
    {
        if (bgmSource != null && bgmSource.isPlaying)
        {
            bgmSource.Stop();
        }
    }

    // 배경음악 일시정지
    public void PauseBGM()
    {
        if (bgmSource != null && bgmSource.isPlaying)
        {
            bgmSource.Pause();
        }
    }

    // 배경음악 재개
    public void ResumeBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.UnPause();
        }
    }

    // 전체 볼륨 설정
    /// <param name="volume">볼륨 값 (0.0 ~ 1.0)</param>
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        
        // 전체 볼륨 변경 시 BGM과 SFX에 즉시 반영
        if (bgmSource != null)
        {
            bgmSource.volume = bgmVolume * masterVolume;
        }
        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume * masterVolume;
        }
    }

    //배경음악 볼륨 설정
    /// <param name="volume">볼륨 값 (0.0 ~ 1.0)</param>
    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        if (bgmSource != null)
        {
            bgmSource.volume = bgmVolume * masterVolume;
        }
    }

    // 효과음 볼륨 설정
    /// <param name="volume">볼륨 값 (0.0 ~ 1.0)</param>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume * masterVolume;
        }
    }

    // 현재 전체 볼륨 반환
    public float GetMasterVolume()
    {
        return masterVolume;
    }

    // 현재 배경음악 볼륨 반환
    public float GetBGMVolume()
    {
        return bgmVolume;
    }

    // 현재 효과음 볼륨 반환
    public float GetSFXVolume()
    {
        return sfxVolume;
    }

    // 저장된 볼륨 설정 로드
    private void LoadVolumeSettings()
    {
        // PlayerPrefs에서 저장된 볼륨 값 로드
        if (PlayerPrefs.HasKey(MASTER_VOLUME_KEY))
        {
            masterVolume = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY);
        }

        if (PlayerPrefs.HasKey(BGM_VOLUME_KEY))
        {
            bgmVolume = PlayerPrefs.GetFloat(BGM_VOLUME_KEY);
        }

        if (PlayerPrefs.HasKey(SFX_VOLUME_KEY))
        {
            sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY);
        }

        // AudioSource가 이미 초기화되어 있다면 볼륨 적용
        if (bgmSource != null)
        {
            bgmSource.volume = bgmVolume * masterVolume;
        }

        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume * masterVolume;
        }
    }
}

