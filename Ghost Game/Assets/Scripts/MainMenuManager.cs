using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MainMenuManager : MonoBehaviour
{
    public GameObject mainButtonsUIPanel;
    public GameObject creditsUIPanel;


    public void ToggleCreditsWindowUI(bool isActive)
    {
        if (isActive)
        {
            ScaleandFadeUIGameObject(true, true, true, 1f, creditsUIPanel, 0.2f);
            ScaleandFadeUIGameObject(false, true, true, 0f, mainButtonsUIPanel, 0.2f);
        }
        else
        {

            ScaleandFadeUIGameObject(true, true, true, 1f, mainButtonsUIPanel, 0.2f);
            ScaleandFadeUIGameObject(false, true, true, 0f, creditsUIPanel, 0.2f);
        }
        
    }


    public void ScaleandFadeUIGameObject(bool isActive, bool isScale, bool isFade, float endScale, GameObject gameObj, float duration)
    {
        if (isActive)
        {
            gameObj.SetActive(true);

            // Animate UI gameobject
            if (isScale) // Scaling is not compatible with TMP_Writer
            {
                gameObj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                gameObj.transform.DOScale(new Vector3(endScale, endScale, endScale), 0.2f).SetEase(Ease.OutQuad);
            }
            if (isFade)
            {
                gameObj.GetComponent<CanvasGroup>().alpha = 0f;
                gameObj.GetComponent<CanvasGroup>().DOFade(1f, duration);
            }
        }
        else
        {
            if (isScale)
            {
                gameObj.transform.DOScale(new Vector3(0.1f, 0.1f, 0.1f), duration).SetEase(Ease.OutQuad);
            }

            if (isFade)
            {
                gameObj.GetComponent<CanvasGroup>().DOFade(0f, duration);
            }

        }
    }
}
