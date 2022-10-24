using System;
using System.Net.Http;
using UnityEngine;
using UnityEngine.UI;

namespace GetPost_REST_API_Data_using_HttpClient
{
    public class GetMethod : MonoBehaviour
    {
        [SerializeField] private InputField _outputArea;
        [SerializeField] private Button _getData;
        private void OnEnable()
        {
            _getData.onClick.AddListener(GetData);
        }

        private void OnDisable()
        {            
            _getData.onClick.RemoveListener(GetData);
        }

        private async void GetData()
        {
            _outputArea.text = "Loading...";
            string url = "https://my-json-server.typicode.com/typicode/demo/posts";
            
            using(var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                    _outputArea.text = await response.Content.ReadAsStringAsync();
                else
                    _outputArea.text = response.ReasonPhrase;
            }
        }
    }
}
