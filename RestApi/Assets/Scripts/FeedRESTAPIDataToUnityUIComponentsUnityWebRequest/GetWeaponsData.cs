using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

namespace FeedRESTAPIDataToUnityUIComponentsUnityWebRequest
{
    public class GetWeaponsData : MonoBehaviour
    {
        [SerializeField] private Dropdown _namesDropDown;
        [SerializeField] private Slider _attackSlider;
        [SerializeField] private Slider _defenceSlider;
        [SerializeField] private Slider _speedSlider;
 
        private WeaponsData _weaponsData = null;
 
        private void Awake()
        {
            StartCoroutine(GetData());
        }
 
        private IEnumerator GetData()
        {
            string url = "https://hostspace.github.io/mockapi/weapons.json";
            using(var request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();
                if (request.isHttpError || request.isNetworkError)
                    Debug.LogError(request.error);
                else
                {
                    string json = request.downloadHandler.text;
                    _weaponsData = JsonUtility.FromJson<WeaponsData>(json);
                }
            }
 
            if(_weaponsData != null && _weaponsData.Weapons.Count>0)
            {
                _namesDropDown.options.Clear();
                foreach (var weapon in _weaponsData.Weapons)
                {
                    _namesDropDown.options.Add(new Dropdown.OptionData(weapon.Name));
                }
                _namesDropDown.value = 0;
                _namesDropDown.onValueChanged.AddListener(WeaponSelectEvent);
                WeaponSelectEvent(0);
            }
        }
 
        private void WeaponSelectEvent(int index)
        {
            var stats = _weaponsData.Weapons[index].Stats;
            _attackSlider.value = stats.Attack;
            _defenceSlider.value = stats.Defence;
            _speedSlider.value = stats.Speed;
        }
    }
}