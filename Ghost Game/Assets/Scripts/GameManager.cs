using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EditorAttributes;
using TMPro;
using Cinemachine;

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

    private Transform NPCStartingPoint;
    private NPCBehaviour npcScript;
    [SerializeField] private GameObject level;
    [SerializeField] private List<GameObject> npcPrefabsList;
    [SerializeField] private CinemachineVirtualCamera shopCamera;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI phobiaText;
    [SerializeField] private TextMeshProUGUI levelNumberText;
    [SerializeField] private GameObject shopUIPanel;
    [SerializeField] private GameObject mainUIPanel;
    [SerializeField] private GameObject killedNPCUIPopup;
    private int deathScore;
    private int livedScore;
    private int levelCount;
    [SerializeField] private int maxPeopleAllowedToLive = 3;
    public bool isScareLevelRunning;
    [SerializeField] private float maxLevelTime = 10f;
    private float levelTime;
    public bool isPlayerControlsEnabled;
    private float startingNPCMaxFear = 100f;
    private float currentNPCMaxFear;

    #endregion

    void Start()
    {
        NPCStartingPoint = GameObject.FindWithTag("NPC Starting Point").transform;
        ResetGame();
    }

    private void Update()
    {
        LevelTimer(); 
    }

    [Button("Reset Game")]
    private void ResetGame()
    {
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
        levelNumberText.text = "Level: " + levelCount.ToString();

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
    }

    [Button("Spawn NPC")]
    private void SpawnNPC()
    {
        GameObject randomNPC = npcPrefabsList[Random.Range(0, npcPrefabsList.Count)];
        GameObject spawnedNPC = Instantiate(randomNPC, NPCStartingPoint.position, Quaternion.identity);
        npcScript = spawnedNPC.GetComponent<NPCBehaviour>();
        npcScript.maxFear = currentNPCMaxFear;

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
        deathScore++;
        killedNPCUIPopup.SetActive(true);
        print("The NPC died!");
        LevelOver();
    }

    public void NPCLived()
    {
        livedScore++;
        print("The NPC lived! The NPC has exited the house successfully unscared.");
        LevelOver();
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
        yield return new WaitForSeconds(1f);
        ToggleShop(true);

    }

    private void ToggleShop(bool displayShop)
    {
        shopUIPanel.SetActive(displayShop);
        mainUIPanel.SetActive(!displayShop);
        killedNPCUIPopup.SetActive(false);

        if (displayShop)
        {
            shopCamera.Priority = 20;
        }
        else
        {
            shopCamera.Priority = 0;
        }
        
    }

    [Button("Next Level")]
    public void ContinueToNextLevel()
    {
        StartLevel();
    }

    private bool CheckIfGameLost()
    {
        if (livedScore >= maxPeopleAllowedToLive)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void GameLost()
    {
        print ("You lost the full game. Too many people lived. Max people allowed to live: " + maxPeopleAllowedToLive);
    }



}
