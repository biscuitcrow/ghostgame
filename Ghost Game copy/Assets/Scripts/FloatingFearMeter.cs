using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class FloatingFearMeter : MonoBehaviour
{
    private Slider fearMeter;
    private float pulseScale = 0.005f;
    public Image progressImage;
    public Gradient colorGradient;

    private void Start()
    {
        fearMeter = gameObject.GetComponent<Slider>();

        // Reset fear meter
        fearMeter.value = 0;
        
    }

    private void Update()
    {
        progressImage.color = colorGradient.Evaluate(fearMeter.value);
    }

    public void UpdateFearMeterUI(float currentValue, float maxValue)
    {
        UIManager.Instance.ScalePulseUIGameObject(gameObject, pulseScale, 0.2f);
        DOTween.To(() => fearMeter.value, x => fearMeter.value = x, currentValue / maxValue, 0.1f);
    }

}
