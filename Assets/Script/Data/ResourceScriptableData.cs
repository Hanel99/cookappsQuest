using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "ResourceData", menuName = "Scriptable Object/ResourceData", order = int.MaxValue)]
public class ResourceScriptableData : ScriptableObject
{

    [Header("- Sprite")]
    public List<Sprite> blockImages = new();
    public List<Sprite> specialBlockImages = new();
}
