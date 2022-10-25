using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

namespace GetPostRESTAPIDataUsingUnityWebRequest
{
    public class GetMethod : MonoBehaviour
    {
        [SerializeField] private InputField _outputArea;
        [SerializeField] private Button _getButton;

        private void OnEnable()
        {
            _getButton.onClick.AddListener(GetData);
        }

        private void OnDisable()
        {
            _getButton.onClick.RemoveListener(GetData);
        }
 
        private void GetData() => StartCoroutine(GetData_Coroutine());
 
        private IEnumerator GetData_Coroutine()
        {
            _outputArea.text = "Loading...";
            string uri = "https://my-json-server.typicode.com/typicode/demo/posts";
            
            using(UnityWebRequest request = UnityWebRequest.Get(uri))
            {
                yield return request.SendWebRequest();
                if (request.isNetworkError || request.isHttpError)
                    _outputArea.text = request.error;
                else
                    _outputArea.text = request.downloadHandler.text;
            }
        }
    }
}