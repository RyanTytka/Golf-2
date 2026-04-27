using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using TMPro;

public class DiscordBugManager : MonoBehaviour
{
    private string webhookURL = "https://discord.com/api/webhooks/1498158328889737407/EQDvb6pGZm0jC8qx4hLK4GLkFHBxBSwwzufYIm1jw6RaoIDefepZ9r_uTwnL6c4fTNSy";

    public TMP_InputField inputField; //the input that user types bug into
    public tutorialManager tutorialManager;
    public GameObject feedkbacInputPanel, feedbackSentPanel;

    public void SendBug()
    {
        StartCoroutine(SendToDiscord(inputField.text));
    }

    IEnumerator SendToDiscord(string bugText)
    {
        string message = bugText;
        //"**Bug Report**\n" +
        //bugText + "\n\n" +
        //"**Device:** " + SystemInfo.deviceModel + "\n" +
        //"**OS:** " + SystemInfo.operatingSystem + "\n" +
        //"**Unity:** " + Application.unityVersion;

        string json = JsonUtility.ToJson(new DiscordMessage(message));

        UnityWebRequest request = new UnityWebRequest(webhookURL, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        inputField.text = "";
        feedkbacInputPanel.SetActive(false);
        feedbackSentPanel.SetActive(true);
        //tutorialManager.CloseTutorial();

        //if (request.result == UnityWebRequest.Result.Success)
        //{
        //    Debug.Log("Bug sent to Discord!");
        //}
        //else
        //{
        //    Debug.LogError(request.error);
        //}
    }

    [System.Serializable]
    public class DiscordMessage
    {
        public string content;

        public DiscordMessage(string content)
        {
            this.content = content;
        }
    }

    public string storeURL = "https://store.steampowered.com/app/3995620/Golf_2/";

    public void OpenStore()
    {
        Application.OpenURL(storeURL);
    }

    public string discordURL = "https://discord.gg/KJcs6wKU6";

    public void OpenDiscord()
    {
        Application.OpenURL(discordURL);
    }

}