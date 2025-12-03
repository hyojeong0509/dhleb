using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SoundSettingsUI : MonoBehaviour
{
    [Header("UI 요소")]
    [Tooltip("전체 볼륨 슬라이더")]
    [SerializeField] private Slider masterSlider;
    
    [Tooltip("BGM 볼륨 슬라이더")]
    [SerializeField] private Slider bgmSlider;
    
    [Tooltip("SFX 볼륨 슬라이더")]
    [SerializeField] private Slider sfxSlider;

    [Header("볼륨 수치 텍스트")]
    [Tooltip("전체 볼륨 값 표시 텍스트")]
    [SerializeField] private TextMeshProUGUI masterVolumeText;
    
    [Tooltip("BGM 볼륨 값 표시 텍스트")]
    [SerializeField] private TextMeshProUGUI bgmVolumeText;
    
    [Tooltip("SFX 볼륨 값 표시 텍스트")]
    [SerializeField] private TextMeshProUGUI sfxVolumeText;

    [Header("볼륨 저장 설정")]
    [SerializeField] private bool saveVolumeSettings = true;

    private const string MASTER_VOLUME_KEY = "MasterVolume";
    private const string BGM_VOLUME_KEY = "BGMVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";

    void Start()
    {
        // 슬라이더 이벤트 연결
        if (masterSlider != null)
        {
            masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        }

        if (bgmSlider != null)
        {
            bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }

        // 저장된 볼륨 설정 로드
        LoadVolumeSettings();
    }

    // SoundManager에서 현재 볼륨을 가져와 UI에 표시
    private void LoadVolumeSettings()
    {
        if (SoundManager.Instance == null)
        {
            Debug.LogWarning("SoundSettingsUI: SoundManager가 없습니다.");
            return;
        }

        // SoundManager에서 현재 볼륨 값 가져오기 (이미 로드됨)
        float masterVolume = SoundManager.Instance.GetMasterVolume();
        float bgmVolume = SoundManager.Instance.GetBGMVolume();
        float sfxVolume = SoundManager.Instance.GetSFXVolume();

        // 슬라이더에 현재 볼륨 값 설정
        if (masterSlider != null)
        {
            masterSlider.value = masterVolume;
        }

        if (bgmSlider != null)
        {
            bgmSlider.value = bgmVolume;
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = sfxVolume;
        }

        // 텍스트 초기 업데이트
        UpdateVolumeTexts();
    }

    // 전체 볼륨 슬라이더 값 변경 시 호출
    private void OnMasterVolumeChanged(float value)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetMasterVolume(value);

            // 볼륨 설정 저장
            if (saveVolumeSettings)
            {
                PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, value);
                PlayerPrefs.Save();
            }

            // 텍스트 업데이트
            UpdateMasterVolumeText(value);
        }
    }

    // BGM 볼륨 슬라이더 값 변경 시 호출
    private void OnBGMVolumeChanged(float value)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetBGMVolume(value);

            // 볼륨 설정 저장
            if (saveVolumeSettings)
            {
                PlayerPrefs.SetFloat(BGM_VOLUME_KEY, value);
                PlayerPrefs.Save();
            }

            // 텍스트 업데이트
            UpdateBGMVolumeText(value);
        }
    }

    // SFX 볼륨 슬라이더 값 변경 시 호출
    private void OnSFXVolumeChanged(float value)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetSFXVolume(value);

            // 볼륨 설정 저장
            if (saveVolumeSettings)
            {
                PlayerPrefs.SetFloat(SFX_VOLUME_KEY, value);
                PlayerPrefs.Save();
            }

            // 텍스트 업데이트
            UpdateSFXVolumeText(value);
        }
    }

    // 전체 볼륨 텍스트 업데이트
    public void UpdateMasterVolumeText(float value)
    {
        if (masterVolumeText != null)
        {
            masterVolumeText.text = Mathf.RoundToInt(value * 100) + "%";
        }
    }

    // BGM 볼륨 텍스트 업데이트
    public void UpdateBGMVolumeText(float value)
    {
        if (bgmVolumeText != null)
        {
            bgmVolumeText.text = Mathf.RoundToInt(value * 100) + "%";
        }
    }

    // SFX 볼륨 텍스트 업데이트
    public void UpdateSFXVolumeText(float value)
    {
        if (sfxVolumeText != null)
        {
            sfxVolumeText.text = Mathf.RoundToInt(value * 100) + "%";
        }
    }

    // 모든 볼륨 텍스트 업데이트
    public void UpdateVolumeTexts()
    {
        if (masterSlider != null)
        {
            UpdateMasterVolumeText(masterSlider.value);
        }

        if (bgmSlider != null)
        {
            UpdateBGMVolumeText(bgmSlider.value);
        }

        if (sfxSlider != null)
        {
            UpdateSFXVolumeText(sfxSlider.value);
        }
    }

    void OnDestroy()
    {
        // 이벤트 구독 해제
        if (masterSlider != null)
        {
            masterSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
        }

        if (bgmSlider != null)
        {
            bgmSlider.onValueChanged.RemoveListener(OnBGMVolumeChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
        }
    }
}

