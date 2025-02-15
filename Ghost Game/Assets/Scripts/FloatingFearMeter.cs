using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FloatingFearMeter : MonoBehaviour
{
    private Slider fearMeter;

    private void Start()
    {
        fearMeter = gameObject.GetComponent<Slider>();

        // Reset fear meter
        fearMeter.value = 0;
        
    }

    public void UpdateFearMeterUI(float currentValue, float maxValue)
    {
        fearMeter.value = currentValue / maxValue;
    }

}
