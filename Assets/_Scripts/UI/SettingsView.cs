using System;
using PurrNet;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsView : View
{
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private TMP_Text sensitivityLabel;
    
    [SerializeField] public Slider gapSlider, longSlider, thickSlider;
    [SerializeField] public Slider R, G, B, A;
    [SerializeField] private TMP_Text gapText, longText,thickText;
    
    private void Awake() {
        InstanceHandler.RegisterInstance(this);

        // Подписка на события слайдеров
        sensitivitySlider.onValueChanged.AddListener(UpdateSensitivityText);
        gapSlider.onValueChanged.AddListener(UpdateGapText);
        longSlider.onValueChanged.AddListener(UpdateLongText);
        thickSlider.onValueChanged.AddListener(UpdateThickText);

        // Инициализация текста сразу
        UpdateSensitivityText(sensitivitySlider.value);
        UpdateGapText(gapSlider.value);
        UpdateLongText(longSlider.value);
        UpdateThickText(thickSlider.value);
    }

    private void OnDestroy() {
        InstanceHandler.UnregisterInstance<SettingsView>();
    }

// Методы обновления текста
    private void UpdateSensitivityText(float value) {
        sensitivityLabel.text = value.ToString("F2");
    }

    private void UpdateGapText(float value) {
        gapText.text = value.ToString("F2");
    }

    private void UpdateLongText(float value) {
        longText.text = value.ToString("F2");
    }

    private void UpdateThickText(float value) {
        thickText.text = value.ToString("F2");
    }


    public override void OnShow() {
        
    }

    public override void OnHide() {
        
    }

    public float GetCrosshairSliders(int a) {
        switch (a) {
           case 0:
               return gapSlider.value;
           case 1:
               return longSlider.value;
           case 2:
               return thickSlider.value;
        }
        return 0;
    }

    public float GetSensitivitySliderValue() {
        return sensitivitySlider.value;
    }
}
