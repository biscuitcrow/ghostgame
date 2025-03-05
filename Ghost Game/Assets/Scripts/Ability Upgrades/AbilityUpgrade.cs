using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Ability Upgrade", menuName = "Abilities/New Ability Upgrade")]

public class AbilityUpgrade : ScriptableObject
{
    public string Name => name;
    public string displayName;
    [TextArea(5,10)]
    public string description;

    public Sprite shopSprite;

    public bool isAvailableInShop;
    public float valueIncrease;

    public void ExecuteAbilityUpgrade()
    {
        AbilitiesManager.Instance.ChangeAbilityByValue(Name, valueIncrease);
    }

    public void ExecuteAbilityDowngrade()
    {
        AbilitiesManager.Instance.ChangeAbilityByValue(Name, -valueIncrease);
    }
}
