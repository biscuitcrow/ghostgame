using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class ButtonHoverEffect : MonoBehaviour
{

    public void ScaleButton(float scaleValue)
    {
        gameObject.GetComponent<RectTransform>().DOScale(new Vector3(scaleValue, scaleValue, scaleValue), 0.1f).SetEase(Ease.OutQuad);
    }

}
