using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;
using TMPro;

public class VRKeyboard : MonoBehaviour
{
    public static VRKeyboard Instance { get; private set; }

    [Header("Layout")]
    [SerializeField] private float _keySize = 0.055f;
    [SerializeField] private float _keyGap = 0.008f;
    [SerializeField] private float _distanceFromCamera = 0.7f;
    [SerializeField] private float _verticalOffset = -0.15f;

    [Header("Colors")]
    [SerializeField] private Color _keyColor = new Color(0.18f, 0.18f, 0.22f, 1f);
    [SerializeField] private Color _keyHighlight = new Color(0.35f, 0.35f, 0.7f, 1f);
    [SerializeField] private Color _keyPressed = new Color(0.5f, 0.5f, 1f, 1f);
    [SerializeField] private Color _specialKeyColor = new Color(0.25f, 0.25f, 0.35f, 1f);
    [SerializeField] private Color _keyTextColor = Color.white;
    [SerializeField] private Color _bgColor = new Color(0.08f, 0.08f, 0.1f, 0.95f);

    private TMP_InputField _activeField;
    private bool _capsOn = false;
    private Canvas _canvas;
    private readonly List<(TextMeshProUGUI label, string value)> _letterKeys = new();

    private static readonly string[][] _rows =
    {
        new[] { "1","2","3","4","5","6","7","8","9","0" },
        new[] { "Q","W","E","R","T","Y","U","I","O","P" },
        new[] { "A","S","D","F","G","H","J","K","L" },
        new[] { "Z","X","C","V","B","N","M" },
        new[] { "CAPS","SPACE","DEL","OK" }
    };

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        BuildKeyboard();
        gameObject.SetActive(false);
    }

    public void Show(TMP_InputField field)
    {
        _activeField = field;
        PositionAtCamera();
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        _activeField = null;
    }

    public void PressKey(string key)
    {
        if (_activeField == null) return;

        switch (key)
        {
            case "DEL":
                if (_activeField.text.Length > 0)
                    _activeField.text = _activeField.text[..^1];
                break;
            case "SPACE":
                _activeField.text += " ";
                break;
            case "OK":
                _activeField.onSubmit.Invoke(_activeField.text);
                Hide();
                return;
            case "CAPS":
                _capsOn = !_capsOn;
                RefreshLetterLabels();
                break;
            default:
                _activeField.text += _capsOn ? key.ToUpper() : key.ToLower();
                break;
        }

        _activeField.caretPosition = _activeField.text.Length;
        _activeField.ActivateInputField();
    }

    // ── construction ───────────────────────────────────────────────

    void BuildKeyboard()
    {
        _canvas = gameObject.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.WorldSpace;
        _canvas.worldCamera = Camera.main;
        gameObject.AddComponent<TrackedDeviceGraphicRaycaster>();

        float step = _keySize + _keyGap;
        float maxRowKeys = 10;
        float boardW = maxRowKeys * step - _keyGap + 0.03f;
        float boardH = _rows.Length * step - _keyGap + 0.04f;

        var boardRt = GetComponent<RectTransform>();
        boardRt.sizeDelta = new Vector2(boardW, boardH);
        boardRt.localScale = Vector3.one;

        // Background
        var bg = CreateGO("Background", transform);
        var bgRt = bg.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = bgRt.offsetMax = Vector2.zero;
        bg.AddComponent<Image>().color = _bgColor;

        float topY = boardH / 2f - 0.02f;

        for (int r = 0; r < _rows.Length; r++)
        {
            string[] row = _rows[r];
            float y = topY - r * step - _keySize / 2f;

            if (r == _rows.Length - 1)
                BuildSpecialRow(row, y, boardW);
            else
                BuildUniformRow(row, y, step);
        }
    }

    void BuildUniformRow(string[] keys, float y, float step)
    {
        float rowW = keys.Length * step - _keyGap;
        float startX = -rowW / 2f + _keySize / 2f;

        for (int c = 0; c < keys.Length; c++)
        {
            float x = startX + c * step;
            CreateKey(keys[c], x, y, _keySize, false);
        }
    }

    void BuildSpecialRow(string[] keys, float y, float boardW)
    {
        // CAPS | SPACE (3x wide) | DEL | OK
        float capW   = _keySize * 1.4f;
        float spaceW = _keySize * 3.5f + _keyGap * 2f;
        float backW  = _keySize * 1.4f;
        float enterW = _keySize * 1.4f;

        float totalW = capW + _keyGap + spaceW + _keyGap + backW + _keyGap + enterW;
        float x = -totalW / 2f;

        PlaceSpecialKey("CAPS",  x + capW / 2f,             y, capW);   x += capW + _keyGap;
        PlaceSpecialKey("SPACE", x + spaceW / 2f,           y, spaceW); x += spaceW + _keyGap;
        PlaceSpecialKey("DEL",  x + backW / 2f,             y, backW);  x += backW + _keyGap;
        PlaceSpecialKey("OK",   x + enterW / 2f,            y, enterW);
    }

    void PlaceSpecialKey(string key, float x, float y, float w)
    {
        CreateKey(key, x, y, w, true);
    }

    void CreateKey(string key, float x, float y, float w, bool special)
    {
        var go = CreateGO(key, transform);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(w, _keySize);
        rt.anchoredPosition = new Vector2(x, y);

        var img = go.AddComponent<Image>();
        img.color = special ? _specialKeyColor : _keyColor;

        var btn = go.AddComponent<Button>();
        var cols = btn.colors;
        cols.normalColor    = special ? _specialKeyColor : _keyColor;
        cols.highlightedColor = _keyHighlight;
        cols.pressedColor   = _keyPressed;
        cols.selectedColor  = cols.normalColor;
        cols.fadeDuration   = 0.05f;
        btn.colors = cols;
        btn.targetGraphic = img;

        string captured = key;
        btn.onClick.AddListener(() => PressKey(captured));

        var labelGO = CreateGO("Label", go.transform);
        var lrt = labelGO.GetComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.offsetMin = lrt.offsetMax = Vector2.zero;

        var tmp = labelGO.AddComponent<TextMeshProUGUI>();
        tmp.text = DisplayLabel(key);
        tmp.color = _keyTextColor;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = 0.005f;
        tmp.fontSizeMax = 0.04f;
        tmp.margin = new Vector4(0.002f, 0.002f, 0.002f, 0.002f);

        if (key.Length == 1 && char.IsLetter(key[0]))
            _letterKeys.Add((tmp, key));
    }

    void RefreshLetterLabels()
    {
        foreach (var (label, value) in _letterKeys)
            label.text = _capsOn ? value.ToUpper() : value.ToLower();
    }

    string DisplayLabel(string key) => key switch
    {
        "SPACE" => "space",
        "CAPS"  => "caps",
        "DEL"  => "del",
        "OK"   => "ok",
        _       => _capsOn ? key.ToUpper() : key.ToLower()
    };

    static GameObject CreateGO(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    void PositionAtCamera()
    {
        var cam = Camera.main;
        if (cam == null) return;
        var camT = cam.transform;
        var fwd = new Vector3(camT.forward.x, 0f, camT.forward.z).normalized;
        if (fwd == Vector3.zero) fwd = Vector3.forward;
        transform.position = camT.position + fwd * _distanceFromCamera + Vector3.up * _verticalOffset;
        transform.rotation = Quaternion.LookRotation(fwd);
        _canvas.worldCamera = cam;
    }
}
