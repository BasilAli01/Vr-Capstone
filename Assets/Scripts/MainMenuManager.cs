using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject loginPanel;
    public GameObject createAccountPanel;
    public GameObject optionsPanel;
    public GameObject creditsPanel;

    void Start()
    {
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        SetAllPanels(false);
        mainMenuPanel.SetActive(true);
    }

    public void ShowLogin()
    {
        SetAllPanels(false);
        loginPanel.SetActive(true);
    }

    public void ShowCreateAccount()
    {
        SetAllPanels(false);
        createAccountPanel.SetActive(true);
    }

    public void ShowOptions()
    {
        SetAllPanels(false);
        optionsPanel.SetActive(true);
    }

    public void ShowCredits()
    {
        SetAllPanels(false);
        creditsPanel.SetActive(true);
    }

    private void SetAllPanels(bool state)
    {
        mainMenuPanel.SetActive(state);
        loginPanel.SetActive(state);
        createAccountPanel.SetActive(state);
        optionsPanel.SetActive(state);
        creditsPanel.SetActive(state);
    }
}