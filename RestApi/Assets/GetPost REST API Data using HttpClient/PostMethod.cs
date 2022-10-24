using System.Collections.Generic;
using System.Net.Http;
using UnityEngine;
using UnityEngine.UI;

namespace GetPost_REST_API_Data_using_HttpClient
{
    public class PostMethod : MonoBehaviour
    {
        [SerializeField] private InputField _outputArea;
        [SerializeField] private Button _postData;
 
        private void OnEnable()
        {
            _postData.onClick.AddListener(PostData);
        }

        private void OnDisable()
        {            
            _postData.onClick.RemoveListener(PostData);
        }

 
        private async void PostData()
        {
            _outputArea.text = "Loading...";
            string url = "https://my-json-server.typicode.com/typicode/demo/posts";
            var postData = new Dictionary<string, string>();
            postData["title"] = "test data";
            using(var httpClient = new HttpClient())
            {
                var response = await httpClient.PostAsync(url, new FormUrlEncodedContent(postData));
                if (response.IsSuccessStatusCode)
                    _outputArea.text = await response.Content.ReadAsStringAsync();
                else
                    _outputArea.text = response.ReasonPhrase;
            }
        }
    }
}