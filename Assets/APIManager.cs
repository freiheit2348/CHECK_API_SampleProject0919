using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System.IO;

public class APIManager : MonoBehaviour
{
    public InputField inputField;
    public Button sendButton;
    public string serverUrl = "http://34.30.2.117:5000"; // GCE�C���X�^���X�̊O��IP�A�h���X���g�p

    void Start()
    {
        sendButton.onClick.AddListener(SendRequest);
    }

    void SendRequest()
    {
        StartCoroutine(SendChatRequest(inputField.text));
    }

    IEnumerator SendChatRequest(string input)
    {
        // �`���b�g���N�G�X�g�𑗐M
        string jsonPayload = JsonUtility.ToJson(new ChatRequest { user_input = input });
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);

        using (UnityWebRequest www = new UnityWebRequest(serverUrl + "/chat", "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Chat Error: " + www.error);
            }
            else
            {
                string response = www.downloadHandler.text;
                Debug.Log("Chat Response: " + response);
                ChatResponse chatResponse = JsonUtility.FromJson<ChatResponse>(response);

                // �����������N�G�X�g�𑗐M
                StartCoroutine(GenerateAudio(chatResponse.ai_response));
            }
        }
    }

    IEnumerator GenerateAudio(string text)
    {
        string jsonPayload = JsonUtility.ToJson(new AudioRequest { text = text });
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);

        using (UnityWebRequest www = new UnityWebRequest(serverUrl + "/generate_audio", "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Audio Generation Error: " + www.error);
            }

                else
                {
                    Debug.Log("Audio received");
                    // �����t�@�C����ۑ�
                    string directory = Application.dataPath;
                    string fileName = "response.mp3";
                    string path = Path.Combine(directory, fileName);
                    File.WriteAllBytes(path, www.downloadHandler.data);
                    Debug.Log("Audio saved to: " + path);
                    // �����ŉ������Đ�����R�[�h��ǉ��ł��܂�
            }
        }
    }

    [System.Serializable]
    private class ChatRequest
    {
        public string user_input;
    }

    [System.Serializable]
    private class ChatResponse
    {
        public string ai_response;
    }

    [System.Serializable]
    private class AudioRequest
    {
        public string text;
    }
}