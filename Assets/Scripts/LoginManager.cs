using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField usernameField;
    public TextMeshProUGUI fingerprintStatusText;
    public Button enterButton;

    private string capturedFingerprintID = "";

    void Update()
    {
        // Check if Arduino sent a fingerprint scan
        if (FingerprintReader.NewFingerprintReceived)
        {
            capturedFingerprintID = FingerprintReader.LastFingerprintID;
            FingerprintReader.ClearFingerprint(); // reset flag
            fingerprintStatusText.text = "✔ Fingerprint Captured (ID: " + capturedFingerprintID + ")";
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

        // TODO: Validate against stored accounts
        Debug.Log("Logging in: " + username + " | Fingerprint: " + capturedFingerprintID);
        fingerprintStatusText.text = "Logging in...";
    }
}