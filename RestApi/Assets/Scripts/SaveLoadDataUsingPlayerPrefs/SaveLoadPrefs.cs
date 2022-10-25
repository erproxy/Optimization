using UnityEngine;
using UnityEngine.UI;

namespace SaveLoadDataUsingPlayerPrefs
{
    public class SaveLoadPrefs : MonoBehaviour
    {
        [SerializeField] private InputField _profileInputField;
        [SerializeField] private Slider _volumeSlider;
        [SerializeField] private Dropdown _qualityDropdown;
 
        public void SavePrefs()
        {
            PlayerPrefs.SetString("key_profile", _profileInputField.text);
            PlayerPrefs.SetFloat("key_volume", _volumeSlider.value);
            PlayerPrefs.SetInt("key_quality", _qualityDropdown.value);
        }
 
        public void LoadPrefs()
        {
            _profileInputField.text = PlayerPrefs.GetString("key_profile", "");  // "" => value if key not found.
            _volumeSlider.value = PlayerPrefs.GetFloat("key_volume", 0f);
            _qualityDropdown.value = PlayerPrefs.GetInt("key_quality", 0);
        }
    }
}