using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "TreeDictionary")]
public class TreeDictionaryCreator : AbstractDictionaryCreator
{

    public override void OnEnable()
    {
        string[] lines = types.text.Split('\n');

        // initialize the dictionary of tree species
        materials = new SortedDictionary<string, string>();
        foreach (string line in lines)
        {
            string line2 = line;
            if (line.Length > 0)
            {
                int index = line.IndexOf("scientific name");

                // fix inconsistencies
                if (index >= 0)
                {
                    line2 = line.Replace("scientific name", ";;;Scientific Name:::");
                }

                index = line2.IndexOf("Scientific Name:::");
                if (index >= 0)
                {
                    // lots of magic numbers here, should probably fix at some point...
                    int length = line2.IndexOf("Distribution") - index - 22;
                    string key = "";
                    if (length > 0)
                        key = line2.Substring(index + 19, length); // scientific name
                    if (!materials.ContainsKey(key))
                        materials.Add(key.ToLowerInvariant(), line2); // add this species to dictionary
                }

            }
        }
    }
}
