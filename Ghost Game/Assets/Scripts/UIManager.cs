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

    [Header("UI Elements")]
    [SerializeField] private RadialProgress radialProgress;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Transform clockHandPivot;
    [SerializeField] private TextMeshProUGUI phobiaText;
    [SerializeField] private TextMeshProUGUI levelNumberText;
    [SerializeField] private TextMeshProUGUI NPCsKilledNumberText;
    [SerializeField] private TextMeshProUGUI NPCsLivedNumberText;



    [SerializeField] private GameObject skipTutorialButton;
    [SerializeField] private GameObject shopUIPanel;
    [SerializeField] private GameObject mainUIPanel;
    [SerializeField] private GameObject houseAdvertisementPanel;
    [SerializeField] private GameObject gameOverUIPanel;
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private GameObject notificationUIPopup;
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private GameObject obituaryUIPopup;
    [SerializeField] private GameObject killedNPCUIPopup;
    [SerializeField] private Transform livedGroupObj;

    [Header ("Notifications UI Settings")]
    [SerializeField] private Ease inEase;
    [SerializeField] private Ease outEase;

    #endregion

    private void Start()
    {

        
    }

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
    }

    public void ToggleGameOverUIPanel(bool isActive)
    {
        gameOverUIPanel.SetActive(isActive);
    }

    public void UpdateGameOverText(string text)
    {
        gameOverText.text = text;
    }

    public void ToggleShop(bool displayShop)
    {
        shopUIPanel.SetActive(displayShop);
        ToggleMainGameplayUI(!displayShop);
    }

    public void UpdateGameStats(int levelCount, int deathScore, int livedScore)
    {
        levelNumberText.text = "Level: " + levelCount.ToString();
        NPCsKilledNumberText.text = "Killed: " + deathScore.ToString();
        NPCsLivedNumberText.text = "Escaped: " + livedScore.ToString();
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

        // Updates clock images
        radialProgress.UpdateRadialProgress(1 - timeFraction);
        clockHandPivot.eulerAngles = new Vector3(0, 0, (timeFraction * 360));
    }


    public void DisplayNotification(string message)
    {
        StartCoroutine("DisplayNotificationRoutine", message);
    }

    
    public void DisplayTutorialNotification(bool isActive, string message)
    {
        notificationText.text = message;

        if (isActive)
        {
            notificationUIPopup.SetActive(true);

            // Animate notification
            notificationUIPopup.GetComponent<CanvasGroup>().alpha = 0f;
            notificationUIPopup.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            notificationUIPopup.GetComponent<CanvasGroup>().DOFade(1f, 0.2f);
            notificationUIPopup.transform.DOScale(Vector3.one, 0.2f).SetEase(inEase);
        }
        else
        {
            notificationUIPopup.GetComponent<CanvasGroup>().DOFade(0f, 0.2f);
            notificationUIPopup.transform.DOScale(new Vector3(0.1f, 0.1f, 0.1f), 0.2f).SetEase(outEase);
        }

    }
    

    public IEnumerator DisplayNotificationRoutine(string message)
    {
        notificationText.text = message;
        notificationUIPopup.SetActive(true);

        // Animate notification
        notificationUIPopup.GetComponent<CanvasGroup>().alpha = 0f;
        notificationUIPopup.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        notificationUIPopup.GetComponent<CanvasGroup>().DOFade(1f, 0.2f);
        notificationUIPopup.transform.DOScale(Vector3.one, 0.2f).SetEase(inEase);

        yield return new WaitForSeconds(2f);

        notificationUIPopup.GetComponent<CanvasGroup>().DOFade(0f, 0.2f);
        notificationUIPopup.transform.DOScale(new Vector3(0.1f, 0.1f, 0.1f), 0.2f).SetEase(outEase);
        yield return new WaitForSeconds(0.2f);
        notificationUIPopup.SetActive(false);
    }

    public void DisplayObituaryUIPopUp()
    {
        DisplayObituaryUIPopUpRoutine();
    }


    public IEnumerator DisplayObituaryUIPopUpRoutine()
    {
        obituaryUIPopup.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        obituaryUIPopup.SetActive(false);

    }

    public void DisplayNPCKilledPopUp()
    {
        StartCoroutine("DisplayNPCKilledPopUpRoutine");
    }

    public IEnumerator DisplayNPCKilledPopUpRoutine()
    {
        killedNPCUIPopup.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        killedNPCUIPopup.SetActive(false);

    }

    public void UpdateNPCEscapedIndicator(int livedScore)
    {
        livedGroupObj.GetChild(livedScore - 1).GetComponent<Image>().DOFade(0.2f, 0.5f);
    }
}
