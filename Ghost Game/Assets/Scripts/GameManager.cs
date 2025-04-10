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
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    #region // <------- VARIABLE DEFINITIONS -------> //

    private PlayerController playerController;
    public Transform playerStartPosition;
    private Transform NPCStartingPoint;
    private NPCBehaviour npcScript;
    public GameObject currentlyChosenNPC;
    private GameObject spawnedNPC;
    [SerializeField] private GameObject level;
    [SerializeField] private List<GameObject> npcPrefabsList;
    [SerializeField] private GameObject exorcistNPC;

    [Header("Cameras")]
    [SerializeField] private Transform mainCamera;
    [SerializeField] private CinemachineVirtualCamera shopCamera;
    [SerializeField] private CinemachineVirtualCamera NPCCamera;
    [SerializeField] private CinemachineVirtualCamera NPCDeathCamera;
    [SerializeField] private float NPCCameraTrackingDuration;

    [Header("Scores and States")]
    public int objectsThrownScore;
    public bool isItemShop;
    [SerializeField] private int maxPeopleAllowedToLive = 3;
    public bool isScareLevelRunning;
    [SerializeField] private float maxLevelTime = 10f;
    public float levelTime;
    private float startingNPCMaxFear = 90f;
    private float currentNPCMaxFear;
    [SerializeField] private float fearIncreasePerLevel = 20f;
    private int deathScore;
    private int livedScore;
    private int levelCount;

    [Header("Tutorial Settings")]
    public bool isTutorialCompleted;
    public bool isToggleTutorialCompleted;
    private bool isPremiseTutorialCompleted;
    public bool isThrowTutorialCompleted;
    private bool isFirstThrowNotifOut;
    private bool isSecondThrowNotifOut;
    public bool isTutorialRunning;
    public bool isHauntTutorialRunning;
    private bool isHauntTutorialCompleted;
    private Coroutine storedTutorialCoroutine;

    [Header("Exorcist")]
    [SerializeField] private int exorcistLevelInterval = 5;
    [SerializeField] private bool isExorcistLevel = false;


    [Header("Audio")]
    int currentThemeIndex;
    [SerializeField] List<string> listOfAllLevelThemesNames = new List<string>();

    [Header("Main Door Trigger")]
    [SerializeField] MainDoorTrigger mainDoorTrigger;


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
        isHauntTutorialCompleted = false;
        isHauntTutorialRunning = false;
        isTutorialRunning = true;
        isScareLevelRunning = false;
        storedTutorialCoroutine = null;
        StartTeachMovement();
    }

    public void SkipTutorial()
    {
        isTutorialCompleted = true;
        isTutorialRunning = false;
        isToggleTutorialCompleted = true;
        isPremiseTutorialCompleted = true;
        isThrowTutorialCompleted = true;
        isFirstThrowNotifOut = true;
        isSecondThrowNotifOut = true;
        isScareLevelRunning = true;
        CompleteHauntTutorial();


        if (storedTutorialCoroutine != null)
        {
            StopCoroutine(storedTutorialCoroutine);
        }
        //UIManager.Instance.DisplayTutorialNotification(false, "");
        UIManager.Instance.ToggleHouseAdvertisementPanel(false);
        UIManager.Instance.ToggleSkipTutorialButton(false);
        UIManager.Instance.ScaleandFadeUIGameObject(false, true, true, 0, UIManager.Instance.televisionMarkerUI, 0.2f);
        UIManager.Instance.ScaleandFadeUIGameObject(false, true, true, 0, UIManager.Instance.tableMarkerUI, 0.2f);

        ResetGame();
    }

    void StartTeachMovement()
    {
        // Teach the player the premise that you control a ghost with movement keyboard input in a haunted house
        UIManager.Instance.DisplayTutorialNotification(true, DialogueManager.Instance.startTeachMovementText);
    }

    public void StartTeachPremise()
    {
        isPremiseTutorialCompleted = false;
        UIManager.Instance.DisplayTutorialNotification(false, "");

        // Disable player controls
        playerController.isPlayerMovementEnabled = false;

        // Pop up of the house advertisement
        UIManager.Instance.ToggleHouseAdvertisementPanel(true);

        UIManager.Instance.ScaleandFadeUIGameObject(false, true, true, 0, UIManager.Instance.televisionMarkerUI, 0.2f);
    }

    public void StartTeachThrow()
    {
        isThrowTutorialCompleted = false;
        // Reenable player controls
        playerController.isPlayerMovementEnabled = true;

        // Teach the player how to throw items
        UIManager.Instance.DisplayTutorialNotification(true, DialogueManager.Instance.startTeachThrowText);
        UIManager.Instance.ScaleandFadeUIGameObject(true, true, true, 1f, UIManager.Instance.tableMarkerUI, 0.2f);

    }

    public IEnumerator PrepareToFinishTutorial()
    {
            yield return new WaitForSeconds(3f);
            UIManager.Instance.DisplayTutorialNotification(true, DialogueManager.Instance.prepToFinishTutOne);
            yield return new WaitForSeconds(2f);
            UIManager.Instance.DisplayTutorialNotification(true, DialogueManager.Instance.prepToFinishTutTwo);
            yield return new WaitForSeconds(3.5f);
            //UIManager.Instance.ToggleMainGameplayUI(true);
            yield return new WaitForSeconds(1.5f);
            //UIManager.Instance.DisplayTutorialNotification(false, "");
            isTutorialCompleted = true;
            isTutorialRunning = false;
            UIManager.Instance.ToggleSkipTutorialButton(false);
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
                if (playerController.isObjectPickedUp)
                {
                    UIManager.Instance.ScaleandFadeUIGameObject(false, true, true, 0, UIManager.Instance.tableMarkerUI, 0.2f);
                }

                if (!isFirstThrowNotifOut && objectsThrownScore == 1)
                {
                    UIManager.Instance.DisplayTutorialNotification(true, DialogueManager.Instance.throwTextOne);
                    isFirstThrowNotifOut = true;
                }
                else if (!isSecondThrowNotifOut && objectsThrownScore == 2)
                {
                    UIManager.Instance.DisplayTutorialNotification(true, DialogueManager.Instance.throwTextTwo);
                    isSecondThrowNotifOut = true;
                    isThrowTutorialCompleted = true;
                    storedTutorialCoroutine = StartCoroutine("PrepareToFinishTutorial");
                }
            }
        }

        // Haunt tutorial
        CheckForTutorialHauntInput();
        // <<---------------------------------- >>
        #endregion  
    }

    [Button("Reset Game")]
    public void ResetGame() // Doesn't reset any tutorial values
    {
        playerController.gameObject.SetActive(true);
        UIManager.Instance.ToggleGameOverUIPanel(false);
        ResetValues();
        ChooseUpcomingNPC();
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
        currentThemeIndex = 0;
        
        AbilitiesManager.Instance.ResetAbilities();
        UIManager.Instance.ResetNPCEscapedIndicator();
    }

    private void RemoveAllNPCs()
    {
        // Remove all NPCs if any
        NPCBehaviour[] NPCs = GameObject.FindObjectsByType<NPCBehaviour>(FindObjectsSortMode.None);
        foreach (NPCBehaviour npc in NPCs)
        {
            VFXManager.Instance.InstantiateRemovalPS(npc.transform);
            Destroy(npc.fearMeterObj);
            Destroy(npc.gameObject);
        }
    }

    private void StartLevel()
    {
        // Needs to be before toggleshop so the right music can play
        levelCount++;
        isExorcistLevel = (levelCount % exorcistLevelInterval == 0) ? true : false;

        RemoveAllNPCs();
        ToggleShop(false);
        
        isItemShop = false;
        currentNPCMaxFear += fearIncreasePerLevel;
        UIManager.Instance.UpdateGameStats(levelCount, deathScore, livedScore);

        ResetLevel();
        SpawnNPC();

        if (isExorcistLevel)
        {
            UIManager.Instance.DisplayNotification("The exorcist has arrived! <shake maxx=0.1 maxy=0.1>Do not touch them</shake> or they will <shake maxx=0.1 maxy=0.1>instantly banish</shake> you from this world!");
            VFXManager.Instance.Invoke("ActivateLightning", 1f);
            StartCoroutine("LightningCoroutine");
        }
        else
        {
            UIManager.Instance.DisplayNotification("Potential client incoming!");
        }

        // Set NPC death camera to track the new NPC (for later)
        NPCDeathCamera.Follow = spawnedNPC.transform;
        StartCoroutine("TrackNPCWithCameraForDuration", NPCCameraTrackingDuration);
    }

    private void StartHauntTutorial()
    {
        isHauntTutorialRunning = true;
        // Freeze NPC movement
        npcScript.ToggleStopNavMeshAgent(true);
        // Freeze timer
        PauseLevelTimer(true);
        // Display haunt notification
        UIManager.Instance.DisplayTutorialNotification(true, "Go up to the client and press X to use my haunt ability to SCARE them!");

    }


    private void CheckForTutorialHauntInput()
    {
        if (isHauntTutorialRunning)
        {
            if (Input.GetKeyDown(playerController.hauntKeyCode)
            && npcScript.CheckNPCDistanceFromGhost() < AbilitiesManager.Instance.ghostScareVisibilityRadius)
            {
                // Display successful haunting notification
                UIManager.Instance.DisplayNotification("Successfully spooked the client with my haunting!");

                CompleteHauntTutorial();
            }
            else
            {
                playerController.ResetHauntCooldown();
            }
        }
    }

    private void CompleteHauntTutorial()
    {
        isHauntTutorialRunning = false;
        // Continue NPC movement
        if (npcScript != null)
        {
            npcScript.ToggleStopNavMeshAgent(false);
        }
        // Resume timer
        PauseLevelTimer(false);
        StartLevelTimer();
        isHauntTutorialCompleted = true;
        mainDoorTrigger.ToggleMainDoors(false);
    }

    private IEnumerator TrackNPCWithCameraForDuration(float duration)
    {
        // Set NPC camera to track the new NPC
        NPCCamera.Follow = spawnedNPC.transform;
        // Switch to the NPC with the camera for duration seconds
        NPCCamera.Priority = 30;
        playerController.isPlayerMovementEnabled = false;
        mainDoorTrigger.ToggleMainDoors(true);
        yield return new WaitForSeconds(duration);
        // Switch back to main camera
        NPCCamera.Priority = 0;
        playerController.isPlayerMovementEnabled = true;

        // Problematic area
        if (!isHauntTutorialCompleted)
        {
            yield return new WaitForSeconds(1f);
            StartHauntTutorial();
        }
        else
        {
            yield return new WaitForSeconds(0.15f);
            mainDoorTrigger.ToggleMainDoors(false);
            StartLevelTimer();
        }
        
        yield return null;
    }


    [Button("Reset Level")]
    private void ResetLevel()
    {
        if (GameObject.FindWithTag("Level") != null)
        {
            GameObject levelObj = GameObject.FindWithTag("Level");
            AudioManager.instance.StopAllTogglableFurnitureSoundEffects(level);
            Destroy(levelObj);
        }

        Vector3 originalPos = new Vector3(0, 9.6f, 0);
        Instantiate(level, originalPos, Quaternion.Euler(0, 180, 0));
        playerController.ResetPlayer();

    }


    private void ChooseUpcomingNPC()
    {
        GameObject randomNPC = npcPrefabsList[Random.Range(0, npcPrefabsList.Count)];
        //Chooses the exorcist NPC instead if its the boss level upcoming
        currentlyChosenNPC = CheckIfUpcomingLevelIsExorcistLevel() ? exorcistNPC : randomNPC;
    }

    private bool CheckIfUpcomingLevelIsExorcistLevel()
    {
        bool result = ((levelCount + 1) % exorcistLevelInterval == 0);
        return result;
    }

    [Button("Spawn NPC")]
    private void SpawnNPC()
    {

        // NPC Arriving SFX
        if (currentlyChosenNPC == exorcistNPC)
        {
            AudioManager.instance.Play("Exorcist Arrives");
        }
        else
        {
            AudioManager.instance.Play("NPC Arrives");
        }

        GameObject npcToSpawn = currentlyChosenNPC;
        spawnedNPC = Instantiate(npcToSpawn, NPCStartingPoint.position, Quaternion.identity);
        npcScript = spawnedNPC.GetComponent<NPCBehaviour>();
        npcScript.maxFear = currentNPCMaxFear;
        print("NPC spawned. npcScript.maxFear = " + npcScript.maxFear);

        FindandDisplayNPCPhobias(spawnedNPC);
        VFXManager.Instance.InstantiateSpawningPS(NPCStartingPoint, 0.3f).transform.parent = spawnedNPC.transform;
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


    private void StartLevelTimer()
    {
        isScareLevelRunning = true;
        levelTime = maxLevelTime;
    }

    private void PauseLevelTimer(bool isPaused)
    {
        isScareLevelRunning = !isPaused;
    }

    private void LevelTimer()
    {
        if (isScareLevelRunning)
        {
            if (levelTime > 0)
            {
                // Subtract elapsed time every frame
                levelTime -= Time.deltaTime;
                UIManager.Instance.UpdateTimerUI(levelTime, maxLevelTime);
            }
            else
            {
                levelTime = 0;
                NPCTimesUp();
                mainDoorTrigger.ToggleMainDoors(true);
            }
        }
    }


    [Button("NPC Times Up")]
    private void NPCTimesUp()
    {
        if (npcScript != null)
        {
            UIManager.Instance.DisplayNotification("Out of time! The client has started leaving the house!", 0.8f);
            npcScript.isNPCLeavingHouse = true;
            print("NPC is now starting to leave house.");
        }
    }

    [Button("NPC Died")]
    public void NPCDied()
    {
      
       if (isScareLevelRunning)
       {
            if (isExorcistLevel)
            {
                // Change shop to cool hat shop as a reward for killing the exorcist [WIP]
                isItemShop = true;
                //UIManager.Instance.DisplayNotification("Not to toot my own horn, but I've scared even the fearsome exorcist to the point of death!", UIManager.Instance.obituraryDelay);
                // Proof of my dedication to the art of spooking!
                AudioManager.instance.Play("Exorcist Died");
                AudioManager.instance.Stop("Exorcist Suck");
                // Play exorcist death animations
            }
            else
            {
                AudioManager.instance.Play("NPC Died");
                // Play NPC death animations
            }

            playerController.playerAnimator.SetBool("isGhostVictory",true);
            
            deathScore++;
            StartCoroutine("DisplayNPCDeathUI", 3f);
            LevelOver();
        }
    }

    private IEnumerator DisplayNPCDeathUI(float startingDelay)
    {
        NPCDeathCamera.Priority = 30;
        yield return new WaitForSeconds(startingDelay - 1);
        AudioManager.instance.Play("Victory Jingle");
        yield return new WaitForSeconds(1);
        NPCDeathCamera.Priority = 0;
        if (isExorcistLevel)
        {
            UIManager.Instance.DisplayNotification("Not to toot my own horn, but I've scared even the fearsome exorcist to the point of death!", UIManager.Instance.obituraryDelay);
        }
        else
        {
            UIManager.Instance.DisplayNotification("HEHEHE! The potential buyer has died of fright! Seems like I'll be holding on to the house a little longer.", UIManager.Instance.obituraryDelay);
        }

        
        UIManager.Instance.DisplayObituaryUIPopUp();
        UIManager.Instance.DisplayNPCKilledPopUp();
        yield return null;
    }

    [Button("NPC Lived")]
    public void NPCLived()
    {
        if (isScareLevelRunning)
        {
            if (isExorcistLevel)
            {
                // Random permanent debuff to stats, no life penalty
                string abilityname = AbilitiesManager.Instance.DebuffRandomAbility();
                UIManager.Instance.DisplayNotification($"<+shake>BALLS!</+shake> The exorcist survived and reduced my {abilityname} ability!");
                AudioManager.instance.Stop("Exorcist Suck");
            }
            else
            {
                livedScore++;
                UIManager.Instance.UpdateNPCEscapedIndicator(livedScore);
                UIManager.Instance.DisplayNotification(DialogueManager.Instance.NPCLivedText);
            }
            AudioManager.instance.Play("NPC Leaves");
            AudioManager.instance.Play("Level Fail");
            LevelOver(4f);
            playerController.playerAnimator.SetTrigger("isGhostSad");
        }
    }

    public void CameraShake()
    {
        mainCamera.DOShakeRotation(0.3f, 0.6f);
    }

    private void LevelOver(float delayBeforeShop = 6.5f)
    {
        isScareLevelRunning = false;
        print("The level is officially over.");
        if (CheckIfGameLost())
        {
            StartCoroutine("StartGameLostProcedure");
        }
        else
        {
            StartCoroutine(StartLevelOverProcedure(delayBeforeShop));
        }
    }

    IEnumerator StartLevelOverProcedure(float delayBeforeShop = 6f)
    {
        AudioManager.instance.Stop("Exorcist Suck");

        // Adds a delay so that any animations and stuff can play before the shop comes out
        yield return new WaitForSeconds(delayBeforeShop);

        // Choose the next NPC here so that the shop can display it, very important that it is chosen first before the shop is toggled
        ChooseUpcomingNPC();
        AbilitiesManager.Instance.ResetShop();
        ToggleShop(true);
    }

    private void ToggleShop(bool displayShop)
    {
        UIManager.Instance.ToggleHauntAbilityIndicator(!displayShop);
        UIManager.Instance.ToggleShop(displayShop);
        shopCamera.Priority = (displayShop) ? 20 : 0;

        if (displayShop)
        {
            AudioManager.instance.Play("Shop Music");
            AudioManager.instance.SetVolume("Shop Music", 0);
            AudioManager.instance.FadeVolume("Shop Music", 1f, 3f);
            AudioManager.instance.FadeVolume(listOfAllLevelThemesNames[currentThemeIndex], 0f, 2f);
            AudioManager.instance.FadeVolume("Exorcist Level Theme", 0f, 2f);

            VFXManager.Instance.ToggleMagicCirclePS(true);
        }
        else
        {
            ChangeAndPlayLevelMusic();
            AudioManager.instance.FadeVolume("Shop Music", 0f, 2f);

            VFXManager.Instance.ToggleMagicCirclePS(false);
        }
    }

    private void ChangeAndPlayLevelMusic()
    {
        AudioManager.instance.Play("Next Level");

        // Stops any currently playing level theme
        foreach (string theme in listOfAllLevelThemesNames)
        {
            if (AudioManager.instance.CheckIfPlaying(theme)){
                AudioManager.instance.Stop(theme);
            }
        }

        if (isExorcistLevel)
        {
            AudioManager.instance.Play("Exorcist Level Theme");
            AudioManager.instance.Play("Rain");
        }
        else
        {
            // Stops any currently playing exorcist theme
            if (AudioManager.instance.CheckIfPlaying("Exorcist Level Theme"))
            {
                AudioManager.instance.Stop("Exorcist Level Theme");
            }
            if (AudioManager.instance.CheckIfPlaying("Rain"))
            {
                AudioManager.instance.Stop("Rain");
            }

            // Increase the index, making sure that it loops around when it hits the max number of themes
            if (currentThemeIndex == listOfAllLevelThemesNames.Count - 1)
            {
                currentThemeIndex = 0;
            }
            else
            {
                currentThemeIndex++;
            }

            // Fade in the theme found at index
            AudioManager.instance.Play(listOfAllLevelThemesNames[currentThemeIndex]);
            AudioManager.instance.SetVolume(listOfAllLevelThemesNames[currentThemeIndex], 0);
            AudioManager.instance.FadeVolume(listOfAllLevelThemesNames[currentThemeIndex], 1f, 3f);
        }
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
        playerController.gameObject.SetActive(false);
        AudioManager.instance.Stop("Exorcist Suck");
        AudioManager.instance.Play("Level Fail");
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
            UIManager.Instance.UpdateGameOverText("You touched the exorcist, who instantly destroyed your ghostly form. Stay clear of them next time!");
        }
        else
        {
            //UIManager.Instance.UpdateGameOverText("You lost your house! " + maxPeopleAllowedToLive.ToString() + " people lived and one went on to buy your house.");
        }
        
        UIManager.Instance.ToggleGameOverUIPanel(true);
    }


    private IEnumerator LightningCoroutine()
    {
        while (true)
        {
            if (isExorcistLevel)
            {
                yield return new WaitForSeconds(Random.Range(5f, 8f));
                VFXManager.Instance.ActivateLightning();
            }
            else
            {
                yield break;
            }
        }
    }
}
