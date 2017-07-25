
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TreeDictionary : MonoBehaviour
{
    // holds info about trees, keyed by their scientific names
    private Dictionary<string, string> species;
    public TextAsset trees; // from wood-database.com
    private TextMeshProUGUI treeInfo; // displays text held in species

    // panel for displaying text
    public GameObject treeDisplay;
    private Window treeWindow;

    private void Awake()
    {
        // read trees.txt
        string[] lines = trees.text.Split('\n');

        // initialize the dictionary of tree species
        species = new Dictionary<string, string>();
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
                    // lots of magic numers here, should probably fix at some point...
                    int length = line2.IndexOf("Distribution") - index - 22;
                    string key = "";
                    if (length > 0)
                        key = line2.Substring(index + 19, length); // scientific name
                    if (!species.ContainsKey(key))
                        species.Add(key, line2); // add this species to dictionary
                }

            }
        }

        // text and window script of info panel
        treeInfo = treeDisplay.GetComponentInChildren<TextMeshProUGUI>();
        treeWindow = treeDisplay.GetComponent<Window>();
    }

    // process species name when user clicks on it
    public void Read(string name)
    {
        string value;
        species.TryGetValue(name, out value); // try to get this species from dictionary
        treeWindow.SetVisibility("open");
        if (value == null) // no species with this scientific name is in dictionary
            treeInfo.text = "Species not in database";
        else // found species!
        {
            // title
            treeInfo.text = "<color=#336666FF><size=130%><smallcaps><b>" + name + "</size></smallcaps></color></b>\n";

            // split up properties
            string[] properties = value.Split(new string[] { ";;;" }, System.StringSplitOptions.None);
            foreach (string prop in properties)
            {
                // split between property type and value
                string[] split = prop.Split(new string[] { "::" }, 2, System.StringSplitOptions.None);
                treeInfo.text += "<b><size=120%>" + split[0] + "</b></size>";
                if (split.Length > 1) // there actually is a value
                    treeInfo.text += split[1] + "\n";
            }
        }


    }
}


