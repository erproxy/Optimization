using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

namespace GetPostRESTAPIDataUsingUnityWebRequest
{
    public class PostMethod : MonoBehaviour
    {
        [SerializeField] private InputField _outputArea;
        [SerializeField] private Button _getButton;

        private void OnEnable()
        {
            _getButton.onClick.AddListener(PostData);
        }

        private void OnDisable()
        {
            _getButton.onClick.RemoveListener(PostData);
        }

 
        private void PostData() => StartCoroutine(PostData_Coroutine());
 
        private IEnumerator PostData_Coroutine()
        {
            _outputArea.text = "Loading...";
            string uri = "https://my-json-server.typicode.com/typicode/demo/posts";
            WWWForm form = new WWWForm();
            form.AddField("title", "test data");
            using(UnityWebRequest request = UnityWebRequest.Post(uri, form))
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