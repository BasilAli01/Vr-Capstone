using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.SceneManagement;

public class AccountManager : MonoBehaviour
{
    [Header("Form Panel")]
    public GameObject formPanel;
    public TMP_InputField fullNameField;
    public TMP_InputField usernameField;
    public TextMeshProUGUI fingerprintStatus;
    public TextMeshProUGUI avatarPromptText;

    [Header("Summary Panel")]
    public GameObject summaryPanel;
    public TextMeshProUGUI summaryFullName;
    public TextMeshProUGUI summaryUsername;
    public TextMeshProUGUI summaryFingerprint;

    [Header("Success Panel")]
    public GameObject successPanel;

    // Replace with your actual Firebase project ID
    private string projectID = "vrbank-bba01";
    private string firestoreURL;
    private string capturedFingerprintID = "";

    void Start()
    {
        firestoreURL = "https://firestore.googleapis.com/v1/projects/" + projectID + "/databases/(default)/documents/accounts/";
        ShowFormPanel();
    }

    void Update()
    {
        if (FingerprintReader.NewFingerprintReceived)
        {
            capturedFingerprintID = FingerprintReader.LastFingerprintID;
            FingerprintReader.ClearFingerprint();
            fingerprintStatus.text = "Fingerprint Captured (ID: " + capturedFingerprintID + ")";
            fingerprintStatus.color = Color.green;
        }
    }

    public void OnSubmitPressed()
    {
        string fullName = fullNameField.text.Trim();
        string username = usernameField.text.Trim();

        if (string.IsNullOrEmpty(fullName))
        {
            avatarPromptText.text = "Please enter your full name!";
            avatarPromptText.color = Color.red;
            return;
        }

        if (string.IsNullOrEmpty(username))
        {
            avatarPromptText.text = "Please enter a username!";
            avatarPromptText.color = Color.red;
            return;
        }

        if (string.IsNullOrEmpty(capturedFingerprintID))
        {
            avatarPromptText.text = "Please scan your fingerprint!";
            avatarPromptText.color = Color.red;
            return;
        }

        summaryFullName.text = "Full Name: " + fullName;
        summaryUsername.text = "Username: " + username;
        summaryFingerprint.text = "Fingerprint ID: " + capturedFingerprintID;
        ShowSummaryPanel();
    }

    public void OnConfirmPressed()
    {
        StartCoroutine(SaveAccountToFirestore());
    }

    IEnumerator SaveAccountToFirestore()
    {
        string fullName = fullNameField.text.Trim();
        string username = usernameField.text.Trim();

        string json = "{ \"fields\": { " +
            "\"fullName\": { \"stringValue\": \"" + fullName + "\" }, " +
            "\"username\": { \"stringValue\": \"" + username + "\" }, " +
            "\"fingerprintID\": { \"stringValue\": \"" + capturedFingerprintID + "\" } " +
            "} }";

        string url = firestoreURL + username;

        UnityWebRequest request = new UnityWebRequest(url, "PATCH");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Account saved successfully!");
            ShowSuccessPanel();
        }
        else
        {
            avatarPromptText.text = "Error saving account. Try again.";
            avatarPromptText.color = Color.red;
            Debug.LogError("Firestore error: " + request.error);
            ShowFormPanel();
        }
    }

    public void OnEditPressed()
    {
        ShowFormPanel();
    }

    public void OnBackPressed()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void OnContinuePressed()
    {
        SceneManager.LoadScene("SampleScene");
    }

    private void ShowFormPanel()
    {
        formPanel.SetActive(true);
        summaryPanel.SetActive(false);
        successPanel.SetActive(false);
        avatarPromptText.text = "Please enter your details";
        avatarPromptText.color = Color.white;
    }

    private void ShowSummaryPanel()
    {
        formPanel.SetActive(false);
        summaryPanel.SetActive(true);
        successPanel.SetActive(false);
    }

    private void ShowSuccessPanel()
    {
        formPanel.SetActive(false);
        summaryPanel.SetActive(false);
        successPanel.SetActive(true);
    }
}