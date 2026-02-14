using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using TMPro;

public class LocalizationTest : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown;

    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += OnSelectedLocaleChanged;
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnSelectedLocaleChanged;
    }

    private void Start()
    {
        dropdown.onValueChanged.AddListener(UpdateLocale);
        SyncDropdownToCurrentLocale();
    }

    // 현재 선택된 로케일에 맞춰 드롭다운 인덱스만 갱신 (이벤트는 발생시키지 않음)
    private void SyncDropdownToCurrentLocale()
    {
        if (dropdown == null || LocalizationSettings.AvailableLocales == null ||
            LocalizationSettings.AvailableLocales.Locales == null ||
            LocalizationSettings.AvailableLocales.Locales.Count == 0)
            return;

        var current = LocalizationSettings.SelectedLocale;
        if (current == null) return;

        var locales = LocalizationSettings.AvailableLocales.Locales;
        for (int i = 0; i < locales.Count; i++)
        {
            if (locales[i] == current)
            {
                dropdown.SetValueWithoutNotify(i);
                dropdown.RefreshShownValue();
                return;
            }
        }
    }

    // Unity(또는 다른 코드)에서 로케일이 바뀌었을 때 호출됨 → 드롭다운만 동기화
    private void OnSelectedLocaleChanged(Locale newLocale)
    {
        SyncDropdownToCurrentLocale();
    }

    public void UpdateLocale(int index)
    {
        LocalizationSettings.SelectedLocale =
            LocalizationSettings.AvailableLocales.Locales[index];

        Debug.Log("언어 변경: " +
            LocalizationSettings.SelectedLocale.name);
    }
}
