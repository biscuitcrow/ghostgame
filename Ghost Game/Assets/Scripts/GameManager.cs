using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EditorAttributes;
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
    public Transform playerStartPosition;
    private Transform NPCStartingPoint;
    private NPCBehaviour npcScript;
    [SerializeField] private GameObject level;
    [SerializeField] private List<GameObject> npcPrefabsList;
    [SerializeField] private GameObject exorcistNPC;
    [SerializeField] private Transform mainCamera;
    [SerializeField] private CinemachineVirtualCamera shopCamera;

    
    private int deathScore;
    private int livedScore;
    private int levelCount;
    public int objectsThrownScore;
    public bool isItemShop;
    [SerializeField] private int maxPeopleAllowedToLive = 3;
    public bool isScareLevelRunning;
    [SerializeField] private float maxLevelTime = 10f;
    private float levelTime;
    private float startingNPCMaxFear = 90f;
    private float currentNPCMaxFear;
    private float fearIncreasePerLevel = 20f;

    [Header("Tutorial Settings")]
    public bool isTutorialCompleted;
    public bool isToggleTutorialCompleted;
    private bool isPremiseTutorialCompleted;
    public bool isThrowTutorialCompleted;
    private bool isFirstThrowNotifOut;
    private bool isSecondThrowNotifOut;
    public bool isTutorialRunning;
    private Coroutine storedTutorialCoroutine;

    [Header("Exorcist")]
    [SerializeField] private int exorcistLevelInterval = 5;
    [SerializeField] private bool isExorcistLevel = false;

    #endregion

    void Start()
    {
        playerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        NPCStartingPoint = GameObject.FindWithTag("NPC Starting Point").transform;
        //ResetGame();
        StartTutorial();
    }

    #region // <------- TUTORIAL METHODS -------> //
    void StartTutorial()
    {
        print("tutorial started");
        // This UI toggling order of the shop and main matters
        ToggleShop(false);
        UIManager.Instance.ToggleMainGameplayUI(false);
        UIManager.Instance.ToggleSkipTutorialButton(true);

        ResetValues();
        playerController.ResetPlayer();
        
        isTutorialCompleted = false;
        isToggleTutorialCompleted = false;
        isPremiseTutorialCompleted = false;
        isThrowTutorialCompleted = false;
        isFirstThrowNotifOut = false;
        isSecondThrowNotifOut = false;
        isTutorialRunning = true;
        isScareLevelRunning = false;
        storedTutorialCoroutine = null;
        StartTeachMovement();
    }

    public void SkipTutorial()
    {
        isTutorialCompleted = true;
        isTutorialRunning = false;
        if (storedTutorialCoroutine != null)
        {
            StopCoroutine(storedTutorialCoroutine);
        }
        UIManager.Instance.DisplayTutorialNotification(false, "");
        UIManager.Instance.ToggleHouseAdvertisementPanel(false);
        UIManager.Instance.ToggleSkipTutorialButton(false);
        ResetGame();
    }

    void StartTeachMovement()
    {
        // Teach the player the premise that you control a ghost with movement keyboard input in a haunted house
        UIManager.Instance.DisplayTutorialNotification(true, "Wakey wakey! Time to turn on the TV and check the morning news. [Use arrow keys to move and E to toggle objects.]");
    }

    public void StartTeachPremise()
    {
        isPremiseTutorialCompleted = false;
        UIManager.Instance.DisplayTutorialNotification(false, "");

        // Disable player controls
        playerController.isPlayerMovementEnabled = false;

        // Pop up of the house advertisement
        UIManager.Instance.ToggleHouseAdvertisementPanel(true);
    }

    public void StartTeachThrow()
    {
        isThrowTutorialCompleted = false;
        // Reenable player controls
        playerController.isPlayerMovementEnabled = true;

        // Teach the player how to throw items
        UIManager.Instance.DisplayTutorialNotification(true, "Throw a tantrum. [Hold and release E to throw objects]");
    }

    public IEnumerator PrepareToFinishTutorial()
    {
            yield return new WaitForSeconds(3f);
            UIManager.Instance.DisplayTutorialNotification(true, "AHEM. OKAY I'M NORMAL NOW.");
            yield return new WaitForSeconds(2f);
            UIManager.Instance.DisplayTutorialNotification(true, "I hear people coming. Quick, I have to SCARE them before they leave! Keep up my haunted house reputation - MAKE MY HOUSE UNSELLABLE!");
            yield return new WaitForSeconds(3.5f);
            UIManager.Instance.ToggleMainGameplayUI(true);
            yield return new WaitForSeconds(1.5f);
            UIManager.Instance.DisplayTutorialNotification(false, "");
            isTutorialCompleted = true;
            isTutorialRunning = false;
            ResetGame();
            yield break;
    }

    #endregion


    private void Update()
    {
        LevelTimer();

        #region // <<--------- TUTORIAL STUFF --------- >>
        // Premise tutorial
        if (isTutorialRunning)
        {
            if (!isPremiseTutorialCompleted && isToggleTutorialCompleted)
            {
                if (Input.GetKeyDown(playerController.interactionKeyCode))
                {
                    // Dismiss house advertisement and progress to next stage of tutorial
                    UIManager.Instance.ToggleHouseAdvertisementPanel(false);
                    isPremiseTutorialCompleted = true;
                    StartTeachThrow();
                }
            }

            // Throw tutorial
            if (!isThrowTutorialCompleted)
            {
                if (!isFirstThrowNotifOut && objectsThrownScore == 1)
                {
                    UIManager.Instance.DisplayTutorialNotification(true, "I'LL SHOW THEM! I'LL SHOW THEM WHAT IT'S LIKE TO MESS WITH THE WRONG GHOST!");
                    isFirstThrowNotifOut = true;
                }
                else if (!isSecondThrowNotifOut && objectsThrownScore == 2)
                {
                    UIManager.Instance.DisplayTutorialNotification(true, "THEY WILL RUE THE DAY THEY CROSSED ME! MWAHAHAHA!");
                    isSecondThrowNotifOut = true;
                    isThrowTutorialCompleted = true;
                    storedTutorialCoroutine = StartCoroutine("PrepareToFinishTutorial");
                }
            }
        }
        // <<---------------------------------- >>
        #endregion  
    }

    [Button("Reset Game")]
    public void ResetGame() // Doesn't reset any tutorial values
    {
        UIManager.Instance.ToggleGameOverUIPanel(false);
        ResetValues();
        StartLevel();
        playerController.ResetPlayer();
    }

    private void ResetValues()
    {
        deathScore = 0;
        livedScore = 0;
        levelCount = 0;
        objectsThrownScore = 0;
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
        UIManager.Instance.UpdateGameStats(levelCount, deathScore, livedScore);

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
            UIManager.Instance.UpdatePhobiaText("Phobia: None");
        }
        else if (tagNames.Count == 1)
        {
            UIManager.Instance.UpdatePhobiaText("Phobia: " + whatPhobias);
        }
        else if (tagNames.Count > 1)
        {
            UIManager.Instance.UpdatePhobiaText("Phobias: " + whatPhobias);
        }
    }

    private void LevelTimer()
    {
        if (isScareLevelRunning && levelTime > 0)
        {
            // Subtract elapsed time every frame
            levelTime -= Time.deltaTime;
            UIManager.Instance.UpdateTimerUI(levelTime, maxLevelTime);
        }
        else 
        {
            levelTime = 0;
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
                UIManager.Instance.DisplayNotification("Don't mean to toot my own horn, but I've scared even the formidable exorcist to the point of death!");
                //Proof of my dedication to the art of spooking!
            }
            else
            {
                UIManager.Instance.DisplayNotification("HEHEHE! The potential buyer has died of fright! Cowards, all of them! Seems like I'll be holding on to the house a little longer.");
            }
            deathScore++;
            UIManager.Instance.DisplayNPCKilledPopUp();
            UIManager.Instance.DisplayObituaryUIPopUp();
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
                UIManager.Instance.DisplayNotification($"BALLS! The exorcist survived and reduced my {abilityname} ability!");
            }
            else
            {
                livedScore++;
                UIManager.Instance.UpdateNPCEscapedIndicator(livedScore);
                UIManager.Instance.DisplayNotification("Oh, snap... the potential buyer lived and left the house unscared. They're going to spread the word to their friends... MY HOUSE!!!");
            }
            LevelOver();
        }
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
        yield return new WaitForSeconds(3f);
        ToggleShop(true);
    }

    private void ToggleShop(bool displayShop)
    {
        UIManager.Instance.ToggleHauntAbilityIndicator(!displayShop);
        UIManager.Instance.ToggleShop(displayShop);
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
            UIManager.Instance.UpdateGameOverText("You were destroyed by the exorcist. Stay clear of them next time!");
        }
        else
        {
            UIManager.Instance.UpdateGameOverText("You lost the game. Too many people lived. Max people allowed to live: " + maxPeopleAllowedToLive.ToString());
        }
        
        UIManager.Instance.ToggleGameOverUIPanel(true);
    }



}
