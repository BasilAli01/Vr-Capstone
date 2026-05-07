using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField usernameField;
    public TextMeshProUGUI fingerprintStatusText;
    public Button enterButton;

    private string capturedFingerprintID = "";
    private string projectID = "vrbank-bba01";

    void Update()
    {
        if (FingerprintReader.NewFingerprintReceived)
        {
            capturedFingerprintID = FingerprintReader.LastFingerprintID;
            FingerprintReader.ClearFingerprint();
            if (fingerprintStatusText != null)
            {
                fingerprintStatusText.text = "Fingerprint Captured (ID: " + capturedFingerprintID + ")";
                fingerprintStatusText.color = Color.green;
            }
            else
                Debug.LogError("LoginManager: fingerprintStatusText is not assigned in the Inspector.");
        }
        else if (FingerprintReader.AuthFailed)
        {
            FingerprintReader.ClearAuthFailed();
            if (fingerprintStatusText != null)
            {
                fingerprintStatusText.text = "Not recognized. Try again.";
                fingerprintStatusText.color = Color.red;
            }
        }
    }

    public void OnEnterPressed()
    {
        string username = usernameField.text.Trim();

        if (string.IsNullOrEmpty(username))
        {
            fingerprintStatusText.text = "Please enter a username.";
            fingerprintStatusText.color = Color.red;
            return;
        }

        if (string.IsNullOrEmpty(capturedFingerprintID))
        {
            fingerprintStatusText.text = "Please scan your fingerprint.";
            fingerprintStatusText.color = Color.red;
            return;
        }

        StartCoroutine(ValidateLogin(username));
    }

    IEnumerator ValidateLogin(string username)
    {
        fingerprintStatusText.text = "Validating...";
        fingerprintStatusText.color = Color.white;

        string url = "https://firestore.googleapis.com/v1/projects/" + projectID + "/databases/(default)/documents/accounts/" + username;
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            fingerprintStatusText.text = "Account not found.";
            fingerprintStatusText.color = Color.red;
            yield break;
        }

        string storedID = ExtractFingerprintID(request.downloadHandler.text);

        if (string.IsNullOrEmpty(storedID))
        {
            fingerprintStatusText.text = "Account data invalid.";
            fingerprintStatusText.color = Color.red;
            yield break;
        }

        if (storedID == capturedFingerprintID)
        {
            fingerprintStatusText.text = "Login successful!";
            fingerprintStatusText.color = Color.green;
            SceneManager.LoadScene("SampleScene");
        }
        else
        {
            fingerprintStatusText.text = "Fingerprint does not match. Access denied.";
            fingerprintStatusText.color = Color.red;
        }
    }

    string ExtractFingerprintID(string json)
    {
        string compact = System.Text.RegularExpressions.Regex.Replace(json, @"\s+", "");
        const string key = "\"fingerprintID\":{\"stringValue\":\"";
        int start = compact.IndexOf(key);
        if (start == -1) return "";
        start += key.Length;
        int end = compact.IndexOf("\"", start);
        if (end == -1) return "";
        return compact.Substring(start, end - start);
    }
}
