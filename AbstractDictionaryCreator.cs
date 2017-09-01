using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class AbstractDictionaryCreator : ScriptableObject
{
    public TextAsset types;
    public SortedDictionary<string, string> materials;

    public abstract void OnEnable();
}
