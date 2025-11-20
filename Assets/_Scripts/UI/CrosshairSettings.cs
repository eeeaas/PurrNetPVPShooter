using UnityEngine;
using UnityEngine.UI;

public class CrosshairSettings : MonoBehaviour {
    [SerializeField] private SettingsView settingsView;
    [SerializeField] private RectTransform u, d, l, r;
    private Image[] imgs;

    private Vector2 uBasePos, dBasePos, lBasePos, rBasePos;
    private Vector2 uBaseSize, dBaseSize, lBaseSize, rBaseSize;

    private void Awake() {
        uBasePos = u.anchoredPosition;
        dBasePos = d.anchoredPosition;
        lBasePos = l.anchoredPosition;
        rBasePos = r.anchoredPosition;

        uBaseSize = u.sizeDelta;
        dBaseSize = d.sizeDelta;
        lBaseSize = l.sizeDelta;
        rBaseSize = r.sizeDelta;
        // Подписываемся на события
        settingsView.gapSlider.onValueChanged.AddListener(_ => UpdateCrosshair());
        settingsView.longSlider.onValueChanged.AddListener(_ => UpdateCrosshair());
        settingsView.thickSlider.onValueChanged.AddListener(_ => UpdateCrosshair());
        imgs = new Image[] { u.GetComponent<Image>(), d.GetComponent<Image>(), l.GetComponent<Image>(), r.GetComponent<Image>() };

        settingsView.R.onValueChanged.AddListener(_ => UpdateColor());
        settingsView.G.onValueChanged.AddListener(_ => UpdateColor());
        settingsView.B.onValueChanged.AddListener(_ => UpdateColor());
        settingsView.A.onValueChanged.AddListener(_ => UpdateColor());
        

        // Инициализация
        UpdateCrosshair();
        UpdateColor();
    }

    private void UpdateCrosshair() {
        float distance = settingsView.GetCrosshairSliders(0);
        float longe = settingsView.GetCrosshairSliders(1);
        float thick = settingsView.GetCrosshairSliders(2);

        u.anchoredPosition = new Vector2(0, distance);
        d.anchoredPosition = new Vector2(0, -distance);
        l.anchoredPosition = new Vector2(-distance, 0);
        r.anchoredPosition = new Vector2(distance, 0);

        u.sizeDelta = new Vector2(uBaseSize.x * thick, uBaseSize.y * longe);
        d.sizeDelta = new Vector2(dBaseSize.x * thick, dBaseSize.y * longe);
        l.sizeDelta = new Vector2(lBaseSize.x * thick, lBaseSize.y * longe);
        r.sizeDelta = new Vector2(rBaseSize.x * thick, rBaseSize.y * longe);
    }

    private void UpdateColor() {
        Color col = new Color(settingsView.R.value, settingsView.G.value, settingsView.B.value, settingsView.A.value);
        foreach (var img in imgs)
            img.color = col;
    }

}