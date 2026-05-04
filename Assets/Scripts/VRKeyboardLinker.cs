using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_InputField))]
public class VRKeyboardLinker : MonoBehaviour
{
    private TMP_InputField _field;

    void Awake()
    {
        _field = GetComponent<TMP_InputField>();
        _field.shouldHideMobileInput = true;
        _field.shouldHideSoftKeyboard = true;
        _field.onSelect.AddListener(OnSelected);
    }

    void OnSelected(string _)
    {
        if (VRKeyboard.Instance != null)
            VRKeyboard.Instance.Show(_field);
    }

    void OnDestroy()
    {
        if (_field != null)
            _field.onSelect.RemoveListener(OnSelected);
    }
}
