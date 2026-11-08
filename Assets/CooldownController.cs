using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CooldownController : MonoBehaviour
{
    private PlayerController script;

    public GameObject playerController; // assign in inspector

    //Coldowns
    public Image alpha1CooldownImage; // assign in inspector
    public Image alpha2CooldownImage; // assign in inspector
    public Image alpha3CooldownImage; // assign in inspector
    public Image HP;

    // Ability-Slots (immer sichtbar)
    public Color slotBackgroundColor = new Color(0.12f, 0.12f, 0.18f, 0.85f);
    public Color labelReadyColor = new Color(0.98f, 1f, 0f, 1f);   // gelb = bereit
    public Color labelCooldownColor = new Color(0.6f, 0.6f, 0.6f, 1f); // grau = Cooldown

    private TMP_Text label1, label2, label3;

    void Start()
    {
        script = playerController.GetComponent<PlayerController>();

        // Alter HP-Kreis -> ersetzt durch Herzen im GameHUD
        if (HP != null) HP.gameObject.SetActive(false);

        label1 = SetupSlot(alpha1CooldownImage, "1");
        label2 = SetupSlot(alpha2CooldownImage, "2");
        label3 = SetupSlot(alpha3CooldownImage, "3");
    }

    // Update is called once per frame
    void Update()
    {
        if (script == null) return; // Spieler ist tot/zerstört

        alpha1CooldownImage.fillAmount = script.alpha1Cooldown / script.cd;
        alpha2CooldownImage.fillAmount = script.alpha2Cooldown / script.dashCd;
        alpha3CooldownImage.fillAmount = script.alpha3Cooldown / script.ballCooldown;

        UpdateLabel(label1, script.alpha1Cooldown);
        UpdateLabel(label2, script.alpha2Cooldown);
        UpdateLabel(label3, script.alpha3Cooldown);
    }

    // Legt einen festen Hintergrund hinter das Cooldown-Bild und macht die Zahl lesbar.
    private TMP_Text SetupSlot(Image cooldownImage, string key)
    {
        RectTransform src = cooldownImage.rectTransform;

        // Hintergrund als Geschwister direkt VOR dem Cooldown-Bild -> wird darunter gezeichnet
        GameObject bg = new GameObject("Slot BG " + key, typeof(RectTransform), typeof(Image));
        RectTransform bgRect = bg.GetComponent<RectTransform>();
        bgRect.SetParent(src.parent, false);
        bgRect.anchorMin = src.anchorMin;
        bgRect.anchorMax = src.anchorMax;
        bgRect.pivot = src.pivot;
        bgRect.anchoredPosition = src.anchoredPosition;
        bgRect.sizeDelta = src.sizeDelta;
        bgRect.SetSiblingIndex(src.GetSiblingIndex());

        Image bgImage = bg.GetComponent<Image>();
        bgImage.sprite = cooldownImage.sprite;
        bgImage.color = slotBackgroundColor;
        bgImage.type = Image.Type.Simple;
        bgImage.raycastTarget = false;

        // Vorhandenes Label nutzen oder eins anlegen
        TMP_Text label = cooldownImage.GetComponentInChildren<TMP_Text>(true);
        if (label == null)
        {
            GameObject go = new GameObject("Key " + key, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(cooldownImage.transform, false);
            label = go.GetComponent<TMP_Text>();
        }

        // Der World-Space-Canvas ist winzig (Slot ~0.01 Einheiten). TMP rundet Schriftgrößen beim
        // Auto-Sizing auf 0.05er-Schritte -> die nötige Größe (~0.01) wurde 0 = unsichtbar.
        // Deshalb: Label 10.000x größer anlegen und wieder runterskalieren -> normale Schriftgrößen.
        const float labelScale = 0.0001f;
        RectTransform lr = label.rectTransform;
        lr.anchorMin = lr.anchorMax = new Vector2(0.5f, 0.5f);
        lr.pivot = new Vector2(0.5f, 0.5f);
        lr.anchoredPosition = Vector2.zero;
        lr.sizeDelta = cooldownImage.rectTransform.rect.size / labelScale;
        lr.localScale = Vector3.one * labelScale;
        lr.localRotation = Quaternion.identity;

        label.gameObject.SetActive(true);
        label.text = key;
        label.enableAutoSizing = true;
        label.fontSizeMin = 1f;
        label.fontSizeMax = 1000f;
        label.margin = Vector4.zero;
        label.fontStyle = FontStyles.Bold;
        label.alignment = TextAlignmentOptions.Center;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.raycastTarget = false;
        label.color = labelReadyColor;
        return label;
    }

    private void UpdateLabel(TMP_Text label, float remaining)
    {
        if (label != null) label.color = remaining > 0f ? labelCooldownColor : labelReadyColor;
    }
}
