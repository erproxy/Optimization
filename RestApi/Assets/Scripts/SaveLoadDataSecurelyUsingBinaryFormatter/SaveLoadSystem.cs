using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;

namespace SaveLoadDataSecurelyUsingBinaryFormatter
{
    public class SaveLoadSystem : MonoBehaviour
    { 
        [SerializeField] private InputField _nameInputField;
        [SerializeField] private Slider _ageSlider;
        [SerializeField] private Dropdown _classDropdown;
 
        private string _filePath = String.Empty;
 
        private void Awake()
        {
            _filePath = Path.Combine(Application.dataPath, "playerData.dat");
        }
 
        public void SavePlayerData()
        {
            PlayerData playerData = new PlayerData();
            playerData.playerName = _nameInputField.text;
            playerData.playerAge = _ageSlider.value;
            playerData.playerClass = _classDropdown.value;
 
            Stream stream = new FileStream(_filePath, FileMode.Create);
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(stream, playerData);
            stream.Close();
        }
 
        public void LoadPlayerData()
        {
            if(File.Exists(_filePath))
            {
                Stream stream = new FileStream(_filePath, FileMode.Open);
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                PlayerData data = (PlayerData)binaryFormatter.Deserialize(stream);
                stream.Close();
 
                _nameInputField.text = data.playerName;
                _ageSlider.value = data.playerAge;
                _classDropdown.value = data.playerClass;
            }
        }
    }
}