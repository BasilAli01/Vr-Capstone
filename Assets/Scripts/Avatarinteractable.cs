using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class AvatarInteractable : MonoBehaviour
{
    public AccountManager accountManager;

    private XRSimpleInteractable interactable;

    void Start()
    {
        interactable = GetComponent<XRSimpleInteractable>();
        interactable.selectEntered.AddListener(OnAvatarClicked);
    }

    void OnAvatarClicked(SelectEnterEventArgs args)
    {
        accountManager.OnAvatarClicked();
    }

    void OnDestroy()
    {
        interactable.selectEntered.RemoveListener(OnAvatarClicked);
    }
}