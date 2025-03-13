using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class FillProgress : MonoBehaviour
{
    // Remember to set the Image type to Filled in the inspector

    [SerializeField] Image image;

    public void UpdateFillProgress(float fraction)
    {
        image.fillAmount = fraction;
    }

    public void TweenFillProgress(float end, float duration)
    {
        DOTween.To(() => image.fillAmount, x => image.fillAmount = x, end, duration);
    }
}
