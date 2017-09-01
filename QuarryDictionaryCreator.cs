using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "QuarryDictionary")]
public class QuarryDictionaryCreator : AbstractDictionaryCreator
{
    public bool corsi; // formatting for corsi dictionary is slightly different
    // initialize the dictionary of stones/quarries
    public override void OnEnable()
    {
        string[] lines = types.text.Split('\n');
        // initialize the dictionary of tree species
        materials = new SortedDictionary<string, string>();
        foreach (string line in lines)
        {
            if (line.Length > 0) // this line contains data
            {

                int length = -1;
                string key = "";
                if (corsi)
                {
                    // 16 = length of "Stone Name:::" + ;;;
                    length = line.IndexOf("Alternate") - 16;
                    if (length >= 0)
                        key = line.Substring(13, length).ToLowerInvariant();
                }
                else
                {
                    // 21 = length of "Common Name(s)::: " + ";;;"
                    length = line.IndexOf("Alternative") - 21;
                    if (length >= 0)
                        key = line.Substring(18, length).ToLowerInvariant();
                }
                if (!materials.ContainsKey(key))
                    materials.Add(key, line); // add this stone to dictionary
                // Debug.Log(key);
            }
        }
         // System.IO.File.WriteAllLines(@"C:\Users\Grace Sommers\Desktop\pietri.txt", lines);
    }
}
