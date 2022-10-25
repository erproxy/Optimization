using UnityEngine;
using UnityEngine.UI;
using System.Net.Http;

namespace FeedRESTAPIDataHttpClient
{
    public class GetWeaponsData : MonoBehaviour
    {
        [SerializeField] private Dropdown _namesDropDown;
        [SerializeField] private Slider _attackSlider;
        [SerializeField] private Slider _defenceSlider;
        [SerializeField] private Slider _speedSlider;
 
        WeaponsData _weaponsData = null;
 
        async void Awake()
        {
            string url = "https://hostspace.github.io/mockapi/weapons.json";
            using(var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    _weaponsData = JsonUtility.FromJson<WeaponsData>(json);
                }
                else
                    Debug.LogError(response.ReasonPhrase);
            }
 
            if(_weaponsData != null && _weaponsData.Weapons.Count > 0)
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
 
        void WeaponSelectEvent(int index)
        {
            var stats = _weaponsData.Weapons[index].Stats;
            _attackSlider.value = stats.Attack;
            _defenceSlider.value = stats.Defence;
            _speedSlider.value = stats.Speed;
        }
    }
}