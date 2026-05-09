using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class SetupLoggedInPanel
{
    [MenuItem("Tools/Setup LoggedInPanel")]
    public static void Run()
    {
        // Find LoginPanel to use as reference for parent and size
        var loginPanelGO = GameObject.Find("LoginPanel");
        if (loginPanelGO == null) { Debug.LogError("LoginPanel not found"); return; }

        Transform canvasTransform = loginPanelGO.transform.parent;

        // Create the panel GameObject
        var panel = new GameObject("LoggedInPanel");
        panel.transform.SetParent(canvasTransform, false);

        var panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        var img = panel.AddComponent<Image>();
        img.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);

        // Create the text child
        var textGO = new GameObject("LoggedInText");
        textGO.transform.SetParent(panel.transform, false);

        var textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.sizeDelta = new Vector2(600, 100);
        textRect.anchoredPosition = Vector2.zero;

        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = "Logged In";
        tmp.fontSize = 48;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        // Start disabled
        panel.SetActive(false);

        // Wire to LoginManager
        var lm = Object.FindFirstObjectByType<LoginManager>();
        if (lm != null)
        {
            lm.loggedInPanel = panel;
            lm.loggedInText = tmp;
            EditorUtility.SetDirty(lm);
            Debug.Log("LoginManager wired: loggedInPanel and loggedInText set.");
        }
        else
        {
            Debug.LogWarning("LoginManager component not found in scene.");
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("LoggedInPanel created successfully.");
    }
}
