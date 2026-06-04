using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class LoginManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField usernameField;
    public TextMeshProUGUI fingerprintStatusText;
    public Button enterButton;

    [Header("Panels")]
    public GameObject loginPanel;
    public GameObject loggedInPanel;
    public TextMeshProUGUI loggedInText;

    [Header("Fingerprint Reader")]
    public FingerprintReader fingerprintReader;

    private string capturedFingerprintID = "";
    private const string projectID = "vrbank-bba01";

    void Start()
    {
        if (fingerprintReader == null)
            fingerprintReader = FindObjectOfType<FingerprintReader>();

        StartFingerprintScan();
    }

    public void StartFingerprintScan()
    {
        capturedFingerprintID = "";
        FingerprintReader.ClearFingerprint();
        FingerprintReader.ClearAuthFailed();
        fingerprintStatusText.text = "Place your finger on the scanner...";
        fingerprintStatusText.color = Color.white;
        if (fingerprintReader != null)
            fingerprintReader.SendCommand("AUTH");
    }

    void Update()
    {
        if (FingerprintReader.NewFingerprintReceived)
        {
            capturedFingerprintID = FingerprintReader.LastFingerprintID;
            FingerprintReader.ClearFingerprint();
            fingerprintStatusText.text = "Fingerprint Captured";
            fingerprintStatusText.color = Color.green;
        }
        else if (FingerprintReader.AuthFailed)
        {
            FingerprintReader.ClearAuthFailed();
            capturedFingerprintID = "";
            fingerprintStatusText.text = "Scan failed. Please try again.";
            fingerprintStatusText.color = Color.red;
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
            string fullName = doc?.fields?.fullName?.stringValue;

            if (!string.IsNullOrEmpty(storedID) && storedID == capturedFingerprintID)
            {
                if (loginPanel != null) loginPanel.SetActive(false);

                if (loggedInPanel != null)
                {
                    loggedInPanel.SetActive(true);
                    if (loggedInText != null)
                        loggedInText.text = "Logged in as " + (string.IsNullOrEmpty(fullName) ? username : fullName);
                }
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
    [System.Serializable] private class DocFields { public StringValue fingerprintID; public StringValue fullName; }
    [System.Serializable] private class FirestoreDoc { public DocFields fields; }
}
