using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class UIManager : MonoBehaviour
{

    #region // <------- SINGLETON PATTERN -------> //
    private static UIManager _instance;
    public static UIManager Instance
    {
        get
        {
            // Create logic to create the instance
            if (_instance == null)
            {
                GameObject obj = new GameObject("UI Manager");
                obj.AddComponent<UIManager>();
            }

            return _instance;
        }
    }


    private void Awake()
    {
        _instance = this;
    }
    #endregion

    #region // <------- VARIABLE DEFINITIONS -------> //

    [Header("Tutorial UI Elements")]
    public GameObject televisionMarkerUI;
    public GameObject tableMarkerUI;


    [Header("HUD UI Elements")]
    [SerializeField] private FillProgress clockFillProgress;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Transform clockHandPivot;
    [SerializeField] private TextMeshProUGUI phobiaText;
    [SerializeField] private TextMeshProUGUI levelNumberText;
    [SerializeField] private TextMeshProUGUI NPCsKilledNumberText;
    [SerializeField] private TextMeshProUGUI NPCsLivedNumberText;


    [Header("Player UI Elements")]
    [SerializeField] private GameObject hauntAbilityIndicator;
    [SerializeField] private FillProgress hauntAbilityFillProgress;


    [Header("General UI Elements")]
    [SerializeField] private GameObject skipTutorialButton;
    [SerializeField] private GameObject mainUIPanel;
    [SerializeField] private GameObject clockUI;
    [SerializeField] private GameObject houseAdvertisementPanel;
    [SerializeField] private GameObject gameOverUIPanel;
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private GameObject obituaryUIPopup;
    [SerializeField] private GameObject killedNPCUIPopup;
    [SerializeField] private Transform livedGroupObj;

    [Header("Obituary UI Elements")]
    public float obituraryDelay = 3f;
    [SerializeField] private Image obituaryNPCProfile;

    [Header("Shop UI Elements")]
    [SerializeField] private GameObject shopUIPanel;
    [SerializeField] private GameObject shopButtonsUIPanel;
    [SerializeField] private GameObject shopNextClientUIPanel;
    [SerializeField] private Image shopUpcomingNPCProfile;
    [SerializeField] private Image shopUpcomingNPCPhobia;
    [SerializeField] private Tag unknownPhobiaTag;


    [Header("NPC Phobia ID Card Elements")]
    [SerializeField] private GameObject shopClientFilePanel;
    [SerializeField] private GameObject shopClientFileProfileCardPanel;
    [SerializeField] private TextMeshProUGUI shopClientFileNPCName;
    [SerializeField] private Image shopClientFileNPCProfile;
    [SerializeField] private Image shopClientFilePhobiaIcon;
    [SerializeField] private Button shopClientFileContinueButton;


    [Header("Notifications UI Settings")]
    [SerializeField] private Ease inEase;
    [SerializeField] private Ease outEase;
    [SerializeField] private GameObject notificationUIPopup;
    [SerializeField] private TextMeshProUGUI notificationText;

    [Header("Pulse UI Settings")]
    [SerializeField] private Ease pulseEase;

    private Coroutine notificationCoroutine;

    #endregion

    // <---------------------------------- GENERAL TWEENING METHODS ---------------------------------- > //
    public void ScaleandFadeUIGameObject(bool isActive, bool isScale, bool isFade, float endScale, GameObject gameObj, float duration)
    {
        if (isActive)
        {
            gameObj.SetActive(true);

            // Animate UI gameobject
            if (isScale) // Scaling is not compatible with TMP_Writer
            {
                gameObj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                gameObj.transform.DOScale(new Vector3(endScale, endScale, endScale), 0.2f).SetEase(inEase);
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
                gameObj.transform.DOScale(new Vector3(0.1f, 0.1f, 0.1f), duration).SetEase(outEase);
            }

            if (isFade)
            {
                gameObj.GetComponent<CanvasGroup>().DOFade(0f, duration);
            }
            
        }
    }

    public void ScalePulseUIGameObject(GameObject gameObj, float scalePulse = 0.005f, float duration = 0.2f)
    {
        // Animate UI gameobject
        gameObj.transform.DOPunchScale(new Vector3(scalePulse, scalePulse, scalePulse), duration).SetEase(pulseEase);
    }

    public void ScaleButton(float scaleValue)
    {
        gameObject.GetComponent<RectTransform>().DOScale(new Vector3(scaleValue, scaleValue, scaleValue), 0.1f).SetEase(Ease.OutQuad);
    }

    public void FadeUIGameObject(GameObject gameObj, float startAlpha = 0f, float endAlpha = 1f, float duration = 0.2f)
    {
        CanvasGroup canvasGroup = gameObj.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            gameObj.GetComponent<CanvasGroup>().alpha = startAlpha;
            gameObj.GetComponent<CanvasGroup>().DOFade(endAlpha, duration);
        }
    }

    public void TranslateUIGameObject(GameObject gameObj, Vector2 startPos, Vector2 endPos, float duration, Ease ease = Ease.InCubic)
    {
        RectTransform rectTransform = gameObj.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = startPos;
        DOTween.To(() => rectTransform.anchoredPosition, x => rectTransform.anchoredPosition = x, endPos, duration).SetEase(ease);
    }

    // <---------------------------------------------------------------------------------------------- > //


    // <---------------------------------- HAUNT ABILITY INDICATOR ---------------------------------- > //

    public void ToggleHauntAbilityIndicator(bool isActive)
    {
        ScaleandFadeUIGameObject(isActive, true, true, 1f, hauntAbilityIndicator, 0.2f);
    }

    // An instant update to the fill value
    public void UpdateHauntAbilityIndicator(float fraction)
    {
        hauntAbilityFillProgress.UpdateFillProgress(fraction);
    }

    public void TweenHauntAbilityIndicator(float end, float duration)
    {
        hauntAbilityFillProgress.TweenFillProgress(end, duration);
    }

    public void UseHauntAbilityIndicator()
    {
        print("haunt ability used");
        ScalePulseUIGameObject(hauntAbilityIndicator.transform.Find("Panel").gameObject, 0.4f, 0.15f);
    }

    public void ShakeHauntAbilityIndicator()
    {
        print("shaken not stirred.");
        hauntAbilityIndicator.transform.Find("Panel").DOShakePosition(0.1f, 0.1f);
    }

    // <---------------------------------------------------------------------------------------------- > //


    public void ToggleSkipTutorialButton(bool isActive)
    {
        skipTutorialButton.SetActive(isActive);
    }

    public void ToggleHouseAdvertisementPanel(bool isActive)
    {
        houseAdvertisementPanel.SetActive(isActive);
    }

    public void ToggleMainGameplayUI(bool isActive)
    {
        mainUIPanel.SetActive(isActive);

        if (isActive)
        {
            // Drops the clock UI in nicely (for juice)
            TranslateUIGameObject(clockUI, new Vector2(164.17f, 200f), new Vector2(164.17f, -155.6f), 0.5f, Ease.OutBounce);
            TranslateUIGameObject(levelNumberText.gameObject, new Vector2(-182.7f, 42f), new Vector2(-182.7f, -76.6f), 0.3f, Ease.InOutBack);
            TranslateUIGameObject(livedGroupObj.gameObject, new Vector2(193.8f, 230f), new Vector2(193.8f, 366.1f), 0.3f, Ease.InOutBack);
        }
    }

    public void ToggleGameOverUIPanel(bool isActive)
    {
        gameOverUIPanel.SetActive(isActive);
        if (isActive)
        {
            // Drops the game over UI panel in from the top
            TranslateUIGameObject(gameOverUIPanel, new Vector2(0f, 1080f), new Vector2(0f, 0f), 0.5f, Ease.OutBounce);
        }
    }

    public void UpdateGameOverText(string text)
    {
        gameOverText.text = text;
    }

    public void ToggleShop(bool displayShop)
    {
        UpdateUpcomingNPCInShop();
        shopUIPanel.SetActive(displayShop);
        if (displayShop)
        {
            ToggleShopClientFilePanel(false);
            TranslateUIGameObject(shopButtonsUIPanel, new Vector2(0, 800), new Vector2(0, 0), 0.5f, Ease.OutBounce);
            TranslateUIGameObject(shopNextClientUIPanel, new Vector2(211, -300), new Vector2(211, 112), 0.5f, Ease.OutBounce);

        }
        
        ToggleMainGameplayUI(!displayShop);
    }

    public void ToggleShopClientFilePanel(bool displayFile)
    {
        shopClientFilePanel.SetActive(displayFile);
        if (displayFile)
        {
            UpdateUpcomingNPCInShop();
            //Bounces the ID card in from the top
            TranslateUIGameObject(shopClientFileProfileCardPanel, new Vector2(0, 850), new Vector2(0, 0), 0.5f, Ease.OutBounce);
        }
    }

    private void UpdateUpcomingNPCInShop()
    {
        // Update to show the correct NPC information in the profile card and in the shop
        NPCBehaviour currentNPCScript = GameManager.Instance.currentlyChosenNPC.GetComponent<NPCBehaviour>();
        Tags currentNPCTags = GameManager.Instance.currentlyChosenNPC.GetComponent<Tags>();

        shopUpcomingNPCProfile.sprite = currentNPCScript.profileSprite;
        shopClientFileNPCProfile.sprite = currentNPCScript.profileSprite;
        shopClientFileNPCName.text = currentNPCScript.NPCName;

        if (currentNPCScript.isPhobiaRevealed)
        {
            // Currently only displays the first phobia in the tags list

            shopUpcomingNPCPhobia.sprite = currentNPCTags.allTags[0].phobiaIcon;
            shopClientFilePhobiaIcon.sprite = currentNPCTags.allTags[0].phobiaIcon;
        }
        else
        {
            // Set them to unknown phobia
            shopUpcomingNPCPhobia.sprite = unknownPhobiaTag.phobiaIcon;
            shopClientFilePhobiaIcon.sprite = unknownPhobiaTag.phobiaIcon;
        }
    }

    public void UpdateGameStats(int levelCount, int deathScore, int livedScore)
    {
        levelNumberText.text = "DAY " + levelCount.ToString();
        NPCsKilledNumberText.text = "DEATHS: " + deathScore.ToString();
        NPCsLivedNumberText.text = "ESCAPED: " + livedScore.ToString();
    }

    public void UpdatePhobiaText(string text)
    {
        phobiaText.text = text;
    }

    public void UpdateTimerUI(float levelTime, float maxLevelTime)
    {
        float timeFraction = levelTime / maxLevelTime;

        // Divide the time by 60
        float minutes = Mathf.FloorToInt(levelTime / 60);

        // Returns the remainder
        float seconds = Mathf.FloorToInt(levelTime % 60);

        // Set the text string
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        // Updates clock images to radial fill
        clockFillProgress.UpdateFillProgress(1 - timeFraction);
        clockHandPivot.eulerAngles = new Vector3(0, 0, (timeFraction * 360));
    }


    public void DisplayNotification(string message, float delay = 2f)
    {
        if(notificationCoroutine != null)
        {
            StopCoroutine(notificationCoroutine);
        }
        notificationCoroutine = StartCoroutine(DisplayNotificationRoutine(message, delay));
        AudioManager.instance.Play("Typewriter Sound");
    }

    
    public void DisplayTutorialNotification(bool isActive, string message)
    {
        notificationText.text = message;
   
        // Scale is not compatible with TMP_writer, choose 1
        ScaleandFadeUIGameObject(isActive, false, true, 1f, notificationUIPopup, 0.3f);
        AudioManager.instance.Play("Typewriter Sound");
    }
    

    public IEnumerator DisplayNotificationRoutine(string message, float delay)
    {
        notificationText.text = message;
        notificationUIPopup.SetActive(true);

        // Animate notification
        notificationUIPopup.GetComponent<CanvasGroup>().alpha = 0f;
        //notificationUIPopup.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        notificationUIPopup.GetComponent<CanvasGroup>().DOFade(1f, 0.2f);
        //notificationUIPopup.transform.DOScale(Vector3.one, 0.2f).SetEase(inEase);

        yield return new WaitForSeconds(delay);

        notificationUIPopup.GetComponent<CanvasGroup>().DOFade(0f, 0.2f);
        //notificationUIPopup.transform.DOScale(new Vector3(0.1f, 0.1f, 0.1f), 0.2f).SetEase(outEase);
        //yield return new WaitForSeconds(0.2f);
        //notificationUIPopup.SetActive(false);
    }

    public void DisplayObituaryUIPopUp()
    {
        StartCoroutine("DisplayObituaryUIPopUpRoutine");
    }


    public IEnumerator DisplayObituaryUIPopUpRoutine()
    {
        obituaryNPCProfile.sprite = GameManager.Instance.currentlyChosenNPC.GetComponent<NPCBehaviour>().profileSprite;
        obituaryUIPopup.SetActive(true);
        TranslateUIGameObject(obituaryUIPopup, new Vector2(0, 900), new Vector2(0, 0), 0.3f, Ease.InOutBack);
        yield return new WaitForSeconds(obituraryDelay);
        TranslateUIGameObject(obituaryUIPopup, new Vector2(0, 0), new Vector2(0, 900), 0.3f, Ease.InOutBack);
        yield return new WaitForSeconds(0.3f);
        obituaryUIPopup.SetActive(false);
    }

    public void DisplayNPCKilledPopUp()
    {
        StartCoroutine("DisplayNPCKilledPopUpRoutine");
    }

    public IEnumerator DisplayNPCKilledPopUpRoutine()
    {
        killedNPCUIPopup.SetActive(true);
        Vector2 originalPos = killedNPCUIPopup.GetComponent<RectTransform>().anchoredPosition;
        TranslateUIGameObject(killedNPCUIPopup, new Vector2(393, -340f), new Vector2(393, -173f), 0.3f, Ease.OutBounce);
        yield return new WaitForSeconds(1.5f);
        TranslateUIGameObject(killedNPCUIPopup, new Vector2(393, -173f), new Vector2(393, -340f), 0.3f, Ease.InBounce);
        yield return new WaitForSeconds(0.5f);
        killedNPCUIPopup.SetActive(false);
    }

    public void UpdateNPCEscapedIndicator(int livedScore)
    {
        ScalePulseUIGameObject(livedGroupObj.GetChild(livedScore - 1).gameObject, 0.5f);
        livedGroupObj.GetChild(livedScore - 1).GetComponent<Image>().DOFade(0.2f, 0.5f);
    }
}
