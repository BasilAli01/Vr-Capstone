using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.SceneManagement;

public class AccountManager : MonoBehaviour
{
    [Header("Spatial Panel")]
    public GameObject spatialPanel;

    [Header("Steps")]
    public GameObject formStep;
    public GameObject summaryStep;
    public GameObject successStep;

    [Header("Form Step")]
    public TMP_InputField fullNameField;
    public TMP_InputField usernameField;
    public TextMeshProUGUI fingerprintStatus;

    [Header("Summary Step")]
    public TextMeshProUGUI summaryFullName;
    public TextMeshProUGUI summaryUsername;
    public TextMeshProUGUI summaryFingerprint;

    [Header("Dialogue")]
    public TextMeshProUGUI dialogueText;

    private string capturedFingerprintID = "";
    private string projectID = "vrbank-bba01";
    private string firestoreURL;
    private bool _waitingForFingerprint = false;

    void Start()
    {
        firestoreURL = "https://firestore.googleapis.com/v1/projects/" + projectID + "/databases/(default)/documents/accounts/";
        spatialPanel.SetActive(false);

        usernameField.characterLimit = 10;
        usernameField.onValueChanged.AddListener(OnUsernameChanged);
    }

    void OnUsernameChanged(string value)
    {
        string filtered = "";
        foreach (char c in value)
            if (char.IsDigit(c) && filtered.Length < 10)
                filtered += c;

        if (filtered != value)
        {
            usernameField.SetTextWithoutNotify(filtered);
            usernameField.caretPosition = filtered.Length;
        }
    }

    // Called by AvatarInteractable when dummy is clicked
    public void OnAvatarClicked()
    {
        spatialPanel.SetActive(true);
        ShowFormStep();
        BeginFingerprintScan();
    }

    void BeginFingerprintScan()
    {
        capturedFingerprintID = "";
        _waitingForFingerprint = true;
        fingerprintStatus.text = "Place your finger on the scanner...";
        fingerprintStatus.color = Color.white;
    }

    void Update()
    {
        if (!_waitingForFingerprint) return;

        if (FingerprintReader.NewFingerprintReceived)
        {
            capturedFingerprintID = FingerprintReader.LastFingerprintID;
            FingerprintReader.ClearFingerprint();
            _waitingForFingerprint = false;
            fingerprintStatus.text = "Fingerprint Captured (ID: " + capturedFingerprintID + ")";
            fingerprintStatus.color = Color.green;
        }
        else if (FingerprintReader.AuthFailed)
        {
            FingerprintReader.ClearAuthFailed();
            fingerprintStatus.text = "Not recognized. Try again.";
            fingerprintStatus.color = Color.red;
        }
    }

    public void OnSubmitPressed()
    {
        string fullName = fullNameField.text.Trim();
        string username = usernameField.text.Trim();

        if (string.IsNullOrEmpty(fullName))
        {
            dialogueText.text = "Please enter your full name!";
            return;
        }
        if (string.IsNullOrEmpty(username))
        {
            dialogueText.text = "Please enter a username!";
            return;
        }
        foreach (char c in username)
        {
            if (!char.IsDigit(c))
            {
                dialogueText.text = "Username must contain digits only (0-9)!";
                return;
            }
        }
        if (string.IsNullOrEmpty(capturedFingerprintID))
        {
            dialogueText.text = "Please scan your fingerprint!";
            return;
        }

        summaryFullName.text = "Full Name: " + fullName;
        summaryUsername.text = "Username: " + username;
        summaryFingerprint.text = "Fingerprint ID: " + capturedFingerprintID;
        ShowSummaryStep();
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
            Debug.Log("Account saved!");
            ShowSuccessStep();
        }
        else
        {
            dialogueText.text = "Error saving account. Try again.";
            Debug.LogError("Firestore error: " + request.error);
        }
    }

    public void OnEditPressed()
    {
        ShowFormStep();
    }

    public void OnContinuePressed()
    {
        SceneManager.LoadScene("SampleScene");
    }

    private void ShowFormStep()
    {
        formStep.SetActive(true);
        summaryStep.SetActive(false);
        successStep.SetActive(false);
        dialogueText.text = "Hello! Please enter your details below.";
    }

    private void ShowSummaryStep()
    {
        formStep.SetActive(false);
        summaryStep.SetActive(true);
        successStep.SetActive(false);
        dialogueText.text = "Please confirm your details.";
    }

    private void ShowSuccessStep()
    {
        formStep.SetActive(false);
        summaryStep.SetActive(false);
        successStep.SetActive(true);
        dialogueText.text = "Your account has been created successfully!";
    }
}