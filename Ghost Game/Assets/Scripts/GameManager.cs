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
        UIManager.Instance.ToggleGameOverUIPanel(false);
        ResetValues();
        playerController.ResetPlayer();
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
                UIManager.Instance.DisplayNotification($"You've scared the exorcist to the point of death. A formidable feat indeed!");
            }
            deathScore++;
            UIManager.Instance.DisplayNPCKilledPopUp();
            UIManager.Instance.DisplayNotification("The potential buyer has died of fright! Seems like you'll be holding on to the house a little longer.");
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
                UIManager.Instance.DisplayNotification($"The exorcist survived and managed to reduce your {abilityname} ability.");
            }
            else
            {
                livedScore++;
                UIManager.Instance.UpdateNPCEscapedIndicator(livedScore);
                UIManager.Instance.DisplayNotification("The potential buyer lived! The client has exited the house unscared.");
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
        yield return new WaitForSeconds(2f);
        ToggleShop(true);
    }

    private void ToggleShop(bool displayShop)
    {
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
        string text;
        if (isExorcistLevel)
        {
            text = "You were killed by the exorcist. Stay clear of them next time!";
        }
        else
        {
            text = "You lost the game. Too many people lived. Max people allowed to live: " + maxPeopleAllowedToLive.ToString();
        }

        UIManager.Instance.ToggleGameOverUIPanel(true);
        UIManager.Instance.UpdateGameOverText(text);
    }



}
