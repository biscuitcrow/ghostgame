using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EditorAttributes;
using TMPro;
using Cinemachine;
using DG.Tweening;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    #region // <------- SINGLETON PATTERN -------> //
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            // Create logic to create the instance
            if (_instance == null)
            {
                GameObject obj = new GameObject("Game Manager");
                obj.AddComponent<GameManager>();
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

    private PlayerController playerController;
    private Transform NPCStartingPoint;
    private NPCBehaviour npcScript;
    [SerializeField] private GameObject level;
    [SerializeField] private List<GameObject> npcPrefabsList;
    [SerializeField] private GameObject exorcistNPC;
    [SerializeField] private Transform mainCamera;
    [SerializeField] private CinemachineVirtualCamera shopCamera;
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
    private int deathScore;
    private int livedScore;
    private int levelCount;
    public bool isItemShop;
    [SerializeField] private int maxPeopleAllowedToLive = 3;
    public bool isScareLevelRunning;
    [SerializeField] private float maxLevelTime = 10f;
    private float levelTime;
    public bool isPlayerControlsEnabled;
    private float startingNPCMaxFear = 90f;
    private float currentNPCMaxFear;
    private float fearIncreasePerLevel = 20f;

    [Header("Exorcist")]
    [SerializeField] private int exorcistLevelInterval = 5;
    [SerializeField] private bool isExorcistLevel = false;

    #endregion

    void Start()
    {
        playerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        NPCStartingPoint = GameObject.FindWithTag("NPC Starting Point").transform;
        ResetGame();
    }

    private void Update()
    {
        LevelTimer(); 
    }

    [Button("Reset Game")]
    public void ResetGame()
    {
        gameOverUIPanel.SetActive(false);
        ResetValues();
        StartLevel();
    }

    private void ResetValues()
    {
        deathScore = 0;
        livedScore = 0;
        levelCount = 0;
        maxPeopleAllowedToLive = 3;
        isScareLevelRunning = false;
        currentNPCMaxFear = startingNPCMaxFear;

        AbilitiesManager.Instance.ResetAbilities();
    }

    private void RemoveAllNPCs()
    {
        // Remove all NPCs if any
        NPCBehaviour[] NPCs = GameObject.FindObjectsByType<NPCBehaviour>(FindObjectsSortMode.None);
        foreach (NPCBehaviour npc in NPCs)
        {
            Destroy(npc.fearMeterObj);
            Destroy(npc.gameObject);
        }
    }

    private void StartLevel()
    {
        RemoveAllNPCs();
        AbilitiesManager.Instance.ResetShop();
        ToggleShop(false);

        levelCount++;

        isExorcistLevel = (levelCount % exorcistLevelInterval == 0)? true: false;
        isItemShop = false;
        currentNPCMaxFear += fearIncreasePerLevel;
        levelNumberText.text = "Level: " + levelCount.ToString();
        NPCsKilledNumberText.text = "Killed: " + deathScore.ToString();
        NPCsLivedNumberText.text = "Escaped: " + livedScore.ToString();


        ResetLevel();
        SpawnNPC();
        isScareLevelRunning = true;
        levelTime = maxLevelTime;
    }

    [Button("Reset Level")]
    private void ResetLevel()
    {
        if (GameObject.FindWithTag("Level") != null)
        {
            Destroy(GameObject.FindWithTag("Level"));
        } 
        Instantiate(level);
        playerController.ResetPlayer();
    }

    [Button("Spawn NPC")]
    private void SpawnNPC()
    {
        GameObject randomNPC = npcPrefabsList[Random.Range(0, npcPrefabsList.Count)];
        //Spawns the exorcist NPC instead if its the boss level
        GameObject npcToSpawn = isExorcistLevel? exorcistNPC : randomNPC;
        GameObject spawnedNPC = Instantiate(npcToSpawn, NPCStartingPoint.position, Quaternion.identity);
        npcScript = spawnedNPC.GetComponent<NPCBehaviour>();
        npcScript.maxFear = currentNPCMaxFear;
        print("npcScript.maxFear = " + npcScript.maxFear);

        FindandDisplayNPCPhobias(spawnedNPC);
    }

    private void FindandDisplayNPCPhobias(GameObject NPC)
    {
        string whatPhobias;
        List<string> tagNames = new List<string>();
        Tags NPCtagList = NPC.GetComponent<Tags>();
        foreach (Tag t in NPCtagList.allTags)
        {
            tagNames.Add(t.Name);
        }

        whatPhobias = string.Join(", ", tagNames);

        if (tagNames.Count == 0)
        {
            phobiaText.text = "Phobia: None";
        }
        else if (tagNames.Count == 1)
        {
            phobiaText.text = "Phobia: " + whatPhobias;
        }
        else if (tagNames.Count > 1)
        {
            phobiaText.text = "Phobias: " + whatPhobias;
        }
    }

    private void LevelTimer()
    {
        if (isScareLevelRunning && levelTime > 0)
        {
            // Subtract elapsed time every frame
            levelTime -= Time.deltaTime;

            // Divide the time by 60
            float minutes = Mathf.FloorToInt(levelTime / 60);

            // Returns the remainder
            float seconds = Mathf.FloorToInt(levelTime % 60);

            // Set the text string
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            float timefraction = levelTime / maxLevelTime;
            clockHandPivot.eulerAngles = new Vector3(0, 0, (timefraction * 360));

        }
        else 
        {
            levelTime = 0;
            timerText.text = string.Format("{0:00}:{1:00}", 0, 0);
            NPCTimesUp();
        }
    }

    [Button("NPC Times Up")]
    private void NPCTimesUp()
    {
        if (npcScript != null)
        {
            npcScript.isNPCLeavingHouse = true;
            print("NPC is now starting to leave house.");
        }
    }

    public void NPCDied()
    {
       if (isScareLevelRunning)
       {
            if (isExorcistLevel)
            {
                // Change shop to cool hat shop as a reward for killing the exorcist [WIP]
                isItemShop = true;
                StartCoroutine("DisplayNotification", $"You've scared the exorcist to the point of death. A formidable feat indeed!");
            }
            deathScore++;
            killedNPCUIPopup.SetActive(true);
            StartCoroutine("DisplayNotification", "The potential buyer has died of fright! Seems like you'll be holding on to the house a little longer.");
            LevelOver();
        }
    }

    public void NPCLived()
    {
        if (isScareLevelRunning)
        {
            if (isExorcistLevel)
            {
                // Random permanent debuff to stats, no life penalty
                string abilityname = AbilitiesManager.Instance.DebuffRandomAbility(); 
                StartCoroutine("DisplayNotification", $"The exorcist survived and managed to reduce your {abilityname} ability.");
            }
            else
            {
                livedScore++;
                livedGroupObj.GetChild(livedScore - 1).GetComponent<Image>().DOFade(0.2f, 0.5f);
                StartCoroutine("DisplayNotification", "The potential buyer lived! The client has exited the house unscared.");
            }
            LevelOver();
        }
    }

    IEnumerator DisplayNotification(string message)
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

    public void CameraShake()
    {
        mainCamera.DOShakeRotation(0.3f, 0.6f);
    }

    private void LevelOver()
    {
        isScareLevelRunning = false;
        print("The level is officially over.");
        if (CheckIfGameLost())
        {
            GameLost();
        }
        else
        {
            StartCoroutine("StartLevelOverProcedure");
        }
    }

    IEnumerator StartLevelOverProcedure()
    {
        // Adds a delay so that any animations and stuff can play before the shop comes out
        yield return new WaitForSeconds(2f);
        ToggleShop(true);
    }

    private void ToggleShop(bool displayShop)
    {
        shopUIPanel.SetActive(displayShop);
        mainUIPanel.SetActive(!displayShop);

        killedNPCUIPopup.SetActive(false);

        shopCamera.Priority = (displayShop) ? 20 : 0;
    }

    [Button("Next Level")]
    public void ContinueToNextLevel()
    {
        StartLevel();
    }

    private bool CheckIfGameLost()
    {
        return (livedScore >= maxPeopleAllowedToLive)? true: false;
    }


    public void ExorcistKilledGhost()
    {
        isScareLevelRunning = false;
        isExorcistLevel = true;
        StartCoroutine("StartGameLostProcedure");
    }

    IEnumerator StartGameLostProcedure()
    {
        // Adds a delay so that any animations and stuff can play before the end screen comes out
        yield return new WaitForSeconds(1f);
        GameLost();
    }

    public void GameLost()
    {
        if (isExorcistLevel)
        {
            gameOverText.text = "You were killed by the exorcist. Stay clear of them next time!";
        }
        else
        {
            gameOverText.text = "You lost the game. Too many people lived. Max people allowed to live: " + maxPeopleAllowedToLive;
        }
        gameOverUIPanel.SetActive(true);
    }



}
