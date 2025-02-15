using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tags : MonoBehaviour
{
    [SerializeField]
    private List<Tag> _tags;
    public List<Tag> allTags => _tags;

    public bool HasTag(Tag t)
    {
        return _tags.Contains(t);
    } 
}
