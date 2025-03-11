using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RadialProgress : MonoBehaviour
{
    // Remember to set the Image type to Filled in the inspector

    [SerializeField] Image image;

    public void UpdateRadialProgress(float fraction)
    {
        image.fillAmount = fraction;
    }
}
