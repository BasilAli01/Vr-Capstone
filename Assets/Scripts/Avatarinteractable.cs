using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class AvatarInteractable : MonoBehaviour
{
    public GameObject spatialPanel;

    private XRSimpleInteractable interactable;

    void Start()
    {
        spatialPanel.SetActive(false);
        interactable = GetComponent<XRSimpleInteractable>();
        interactable.selectEntered.AddListener(OnAvatarClicked);
    }

    void OnAvatarClicked(SelectEnterEventArgs args)
    {
        spatialPanel.SetActive(true);
    }

    void OnDestroy()
    {
        interactable.selectEntered.RemoveListener(OnAvatarClicked);
    }
}