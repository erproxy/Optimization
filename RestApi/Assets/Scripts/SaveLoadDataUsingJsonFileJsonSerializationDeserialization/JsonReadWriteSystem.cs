using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace SaveLoadDataUsingJsonFileJsonSerializationDeserialization
{
    public class JsonReadWriteSystem : MonoBehaviour
    {
        [SerializeField] private InputField _idInputField;
        [SerializeField] private InputField _nameInputField;
        [SerializeField] private InputField _infoInputField;
 
        public void SaveToJson()
        {
            WeaponData data = new WeaponData();
            data.Id = _idInputField.text;
            data.Name = _nameInputField.text;
            data.Information = _infoInputField.text;
 
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(Application.dataPath + "/WeaponDataFile.json", json);
        }
 
        public void LoadFromJson()
        {
            string json = File.ReadAllText(Application.dataPath + "/WeaponDataFile.json");
            WeaponData data = JsonUtility.FromJson<WeaponData>(json);
 
            _idInputField.text = data.Id;
            _nameInputField.text = data.Name;
            _infoInputField.text = data.Information;
        }
    }
}