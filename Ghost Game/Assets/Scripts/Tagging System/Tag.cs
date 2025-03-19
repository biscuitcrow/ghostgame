using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New Tag", menuName = "Tags/New Tag")]

public class Tag : ScriptableObject
{
    public string Name => name;
    public Sprite phobiaIcon;
}
