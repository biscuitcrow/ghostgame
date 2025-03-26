using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AbilitiesManager : MonoBehaviour
{
    #region // <------- SINGLETON PATTERN -------> //
    private static AbilitiesManager _instance;
    public static AbilitiesManager Instance
    {
        get
        {
            // Create logic to create the instance
            if (_instance == null)
            {
                GameObject obj = new GameObject("Abilities Manager");
                obj.AddComponent<AbilitiesManager>();
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

    public bool isVisibilityAbilityUnlocked;
    private bool isRerollUsed;

    public float movementSpeed
    {
        get { return abilityValues["movementSpeed"]; }
        set { }
    }
    public float throwForceMult
    {
        get { return abilityValues["throwForceMult"]; }
        set { }
    }
    public float maxWeightThrowable
    {
        get { return abilityValues["maxWeightThrowable"]; }
        set { }
    }

    public float ghostVisibilityScareValue
    {
        get { return abilityValues["ghostVisibilityScareValue"]; }
        set { }
    }

    public float ghostVisibilityScareMult
    {
        get { return abilityValues["ghostVisibilityScareMult"]; }
        set { }
    }

    public float ghostScareVisibilityRadius
    {
        get { return abilityValues["ghostScareVisibilityRadius"]; }
        set { }
    }

    public float ghostScareVisibilityDuration
    {
        get { return abilityValues["ghostScareVisibilityDuration"]; }
        set { }
    }

    public float ghostScareCooldown
    {
        get { return abilityValues["ghostScareCooldown"]; }
        set { }
    }


    public float bonusFearIncrease
    {
        get { return abilityValues["bonusFearIncrease"]; }
        set { }
    }
    public float phobiaMult
    {
        get { return abilityValues["phobiaMult"]; }
        set { }
    }

    [Header("Buttons")]
    [SerializeField] private GameObject rerollButton;
    [SerializeField] private List<Button> shopButtons = new List<Button>();


    [Header("Ability Upgrades")]
    [SerializeField] private AbilityUpgrade phobiaSleuth;
    [SerializeField] private List<AbilityUpgrade> allPossibleShopUpgrades = new List<AbilityUpgrade>();
    [SerializeField] private List<AbilityUpgrade> availableShopUpgrades = new List<AbilityUpgrade>();
    [SerializeField] private List<AbilityUpgrade> availableSkillUpgrades = new List<AbilityUpgrade>();
    [SerializeField] private List<AbilityUpgrade> remainingAvailableShopUpgrades = new List<AbilityUpgrade>();

    private Dictionary<string, float> abilityValues = new Dictionary<string, float>();

    #endregion

    private void Start()
    {
        foreach (AbilityUpgrade upgrade in availableShopUpgrades)
        {
            if (upgrade.isSkillUpgrade)
            {
                availableSkillUpgrades.Add(upgrade);
            }
        }

        ResetAbilities();
        ChooseRandomUpgradesForShop();
    }

    public void ResetAbilities()
    {
        abilityValues.Clear();
        

        // Movement
        EditAbilityDictionary("movementSpeed", 7f);

        // Throwables
        EditAbilityDictionary("throwForceMult", 10f);
        EditAbilityDictionary("maxWeightThrowable", 1f);

        // Ghost Visibility Scare Ability
        //Remember to set this to false afterwards after testing
        isVisibilityAbilityUnlocked = true;
        EditAbilityDictionary("ghostVisibilityScareValue", 20f);
        EditAbilityDictionary("ghostVisibilityScareMult", 2f); // Currently not using as an upgrade - 19 Mar 2025
        EditAbilityDictionary("ghostScareVisibilityRadius", 5f); 
        EditAbilityDictionary("ghostScareVisibilityDuration", 0.5f); // Currently not using as an upgrade (but it is being referenced)- 19 Mar 2025
        EditAbilityDictionary("ghostScareCooldown", 5f);

        // All Interactables
        EditAbilityDictionary("bonusFearIncrease", 10f);

        // Phobias
        EditAbilityDictionary("phobiaMult", 2f);
    }

    public void ResetShop()
    {
        isRerollUsed = false;
        rerollButton.SetActive(true);
        ChooseRandomUpgradesForShop();
    }

    public void RerollShop()
    {
        if (!isRerollUsed)
        {
            isRerollUsed = true;
            rerollButton.SetActive(false);
            ChooseRandomUpgradesForShop();
        }
    }

    private void ChooseRandomUpgradesForShop()
    {
        remainingAvailableShopUpgrades.Clear();
        remainingAvailableShopUpgrades.AddRange(availableShopUpgrades);

        // Remove Phobia Sleuth as an option in the random choosing if the upcoming NPC has already been Phobia Sleuthed 
        if (GameManager.Instance.currentlyChosenNPC.GetComponent<NPCBehaviour>().isPhobiaRevealed)
        {
            if (remainingAvailableShopUpgrades.Contains(phobiaSleuth))
            {
                remainingAvailableShopUpgrades.Remove(phobiaSleuth);
            }
        }


        for (int i = 0; i < shopButtons.Count; i++)
        {
            int rand = Random.Range(0, remainingAvailableShopUpgrades.Count);
            UpdateButtonDisplayUI(i, rand);
            // Add on click event listeners to the shop buttons with specific upgrade index as argument
            shopButtons[i].onClick.RemoveAllListeners();
            AbilityUpgrade ability = remainingAvailableShopUpgrades[rand];
            shopButtons[i].onClick.AddListener(delegate { ShopButtonChosen(ability); });
      

            // Prevents double-choosing of the upgrades
            remainingAvailableShopUpgrades.RemoveAt(rand);
        }

        void UpdateButtonDisplayUI(int buttonIndex, int upgradeIndex)
        {
            shopButtons[buttonIndex].gameObject.GetComponent<Image>().sprite = remainingAvailableShopUpgrades[upgradeIndex].shopSprite;
            shopButtons[buttonIndex].gameObject.GetComponentsInChildren<TextMeshProUGUI>(true)[0].text = remainingAvailableShopUpgrades[upgradeIndex].displayName;
            shopButtons[buttonIndex].gameObject.GetComponentsInChildren<TextMeshProUGUI>(true)[1].text = remainingAvailableShopUpgrades[upgradeIndex].description;
        }
    }

    private void ShopButtonChosen(AbilityUpgrade upgrade)
    {
        upgrade.ExecuteAbilityUpgrade();
        print("Shop upgrade chosen: " + upgrade.displayName);

        if (upgrade.isSkillUpgrade)
        {
            // Only continue the next level if the upgrade chosen was a skill upgrade (and not a special upgrade like Phobia Sleuth)
            GameManager.Instance.ContinueToNextLevel();
        }
    }

    private void EditAbilityDictionary(string key, float value)
    {
        if (!abilityValues.ContainsKey(key))
        {
            abilityValues.Add(key, value);
        }
        else {
            abilityValues[key] = value;
        }
    }

    public void ChangeAbilityByValue(string key, float value)
    {
        if (abilityValues.ContainsKey(key))
        {
            abilityValues[key] += value;
            print("New ability value: " + abilityValues[key]);
        }
    }

    public string DebuffRandomAbility()
    {
        AbilityUpgrade randomUpgrade = availableSkillUpgrades[Random.Range(0, availableSkillUpgrades.Count)];
        randomUpgrade.ExecuteSkillDowngrade();
        return randomUpgrade.displayName;
    }

    public void ActivatePhobiaSleuth()
    {
        // Change the phobia status to revealed on the relevant NPC
        GameManager.Instance.currentlyChosenNPC.GetComponent<NPCBehaviour>().isPhobiaRevealed = true;
        // Display ID card screen
        UIManager.Instance.ToggleShopClientFilePanel(true);
    }

    // When the shop opens
    // Add all the possibly unlockable ability upgrades to the shop pool
    // Choose 4 randomly from available pool of upgrade types
    // Access current values to and display relevant values in the UI
    // Whatever the player chooses, upgrade that particular boon
    // Every ability has a cap, if the value is reached, take it out of the pool. 

}
