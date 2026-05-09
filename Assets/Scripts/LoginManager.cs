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
    private const string projectID = "vrbank-bba01";

    void Update()
    {
        if (FingerprintReader.NewFingerprintReceived)
        {
            capturedFingerprintID = FingerprintReader.LastFingerprintID;
            FingerprintReader.ClearFingerprint();
            fingerprintStatusText.text = "Fingerprint Captured (ID: " + capturedFingerprintID + ")";
            fingerprintStatusText.color = Color.green;
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
        if (enterButton != null) enterButton.interactable = false;

        string url = "https://firestore.googleapis.com/v1/projects/" + projectID +
                     "/databases/(default)/documents/accounts/" + username;

        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (enterButton != null) enterButton.interactable = true;

        if (request.result == UnityWebRequest.Result.Success)
        {
            FirestoreDoc doc = JsonUtility.FromJson<FirestoreDoc>(request.downloadHandler.text);
            string storedID = doc?.fields?.fingerprintID?.stringValue;

            if (!string.IsNullOrEmpty(storedID) && storedID == capturedFingerprintID)
            {
                fingerprintStatusText.text = "Login successful!";
                fingerprintStatusText.color = Color.green;
                SceneManager.LoadScene("SampleScene");
            }
            else
            {
                fingerprintStatusText.text = "Fingerprint does not match. Try again.";
                fingerprintStatusText.color = Color.red;
            }
        }
        else
        {
            if (request.responseCode == 404)
            {
                fingerprintStatusText.text = "Account not found.";
            }
            else
            {
                fingerprintStatusText.text = "Error connecting. Try again.";
                Debug.LogError("Firestore login error: " + request.error);
            }
            fingerprintStatusText.color = Color.red;
        }
    }

    [System.Serializable] private class StringValue { public string stringValue; }
    [System.Serializable] private class DocFields { public StringValue fingerprintID; }
    [System.Serializable] private class FirestoreDoc { public DocFields fields; }
}
