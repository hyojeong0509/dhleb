using UnityEngine;
using UnityEngine.Localization.Settings;
using TMPro;

public class LocalizationTest : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown;

    private void Start()
    {
        dropdown.onValueChanged.AddListener(UpdateLocale);
    }

    public void UpdateLocale(int index)
    {
        LocalizationSettings.SelectedLocale =
            LocalizationSettings.AvailableLocales.Locales[index];

        Debug.Log("언어 변경: " +
            LocalizationSettings.SelectedLocale.name);
    }
}
