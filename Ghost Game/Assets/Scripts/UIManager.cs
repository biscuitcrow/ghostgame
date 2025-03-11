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
    [SerializeField] private GameObject shopUIPanel;
    [SerializeField] private GameObject mainUIPanel;
    [SerializeField] private GameObject gameOverUIPanel;
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private GameObject notificationUIPopup;
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private GameObject killedNPCUIPopup;
    [SerializeField] private Transform livedGroupObj;

    #endregion

    public void ToggleGameOverUIPanel(bool isActive)
    {
        gameOverUIPanel.SetActive(isActive);
    }

    public void UpdateGameOverText(string text)
    {

    }

    public void ToggleShop(bool displayShop)
    {
        shopUIPanel.SetActive(displayShop);
        mainUIPanel.SetActive(!displayShop);
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

    public IEnumerator DisplayNotificationRoutine(string message)
    {
        notificationText.text = message;
        notificationUIPopup.SetActive(true);
        yield return new WaitForSeconds(2f);
        notificationUIPopup.SetActive(false);

        /*
        Sequence mySequence = DOTween.Sequence();

        mySequence.Append(notificationUIPopup.DOAnchorPosY(105, 0.2f));
        mySequence.AppendInterval(1f);
        mySequence.Append(notificationUIPopup.DOAnchorPosY(-110, 0.2f));
        mySequence.Play();
        */

        //help i can't get this tween to work [wip]
        //notificationUIPopup.DOAnchorPosY(105, 0.2f);
        yield return new WaitForSeconds(1f);
        //notificationUIPopup.DOAnchorPosY(-110, 0.2f);

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
