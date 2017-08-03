
using Mapbox.Unity.Map;
using Mapbox.Unity.MeshGeneration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// not just for trees, also parses data for stone
public class TreeDictionary : MonoBehaviour
{
    // holds info about trees, keyed by their scientific names
    private Dictionary<string, string> species;
    public TextAsset trees; // from wood-database.com
    private TextMeshProUGUI treeInfo; // displays text held in species

    // holds info about stones, stored by area IDs
    private string[] stoneInfo;
    public TextAsset stones; // parse this to get array

    // panel for displaying text
    public GameObject treeDisplay;
    private Window treeWindow;

    // display shapefile
    public GameObject treeMapPanel;
    public Image treeMap;
    private AssetBundle mapBundle;

    // display geological map
    public GameObject geoMap;

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
                    // lots of magic numbers here, should probably fix at some point...
                    int length = line2.IndexOf("Distribution") - index - 22;
                    string key = "";
                    if (length > 0)
                        key = line2.Substring(index + 19, length); // scientific name
                    if (!species.ContainsKey(key))
                        species.Add(key, line2); // add this species to dictionary
                }

            }
        }

        // initialize the array of stones
            stoneInfo = stones.text.Split('\n');

        // text and window script of info panel
        treeInfo = treeDisplay.GetComponentInChildren<TextMeshProUGUI>();
        treeWindow = treeDisplay.GetComponent<Window>();

        // load bundle that holds map images
        mapBundle = AssetBundle.LoadFromFile(
            Path.Combine(Application.streamingAssetsPath, "tree maps"));

        if (mapBundle == null) // should not occur
        {
            Debug.Log("Failed to load AssetBundle!");
            return;
        }

    }

    // when this popup panel closes, close geology map, too
    private void OnDisable()
    {
        geoMap.SetActive(false);
    }
    // called when user clicks on a tree species name
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

            // properties
            SetInfoText(value);

            // map
            treeMapPanel.SetActive(true);
            treeMap.sprite = mapBundle.LoadAsset<Sprite>(name);
        }

    }


    // process stone when user clicks on it
    public void ReadStone(string areaId)
    {
        int id;
        Int32.TryParse(areaId, out id);
        treeWindow.SetVisibility("open");
        treeInfo.text = "\n\n"; // title?
        SetInfoText(stoneInfo[id - 1]); // area ID corresponds to info about the geology of the area

        // show stone map, not tree map
        geoMap.SetActive(true);
        treeMapPanel.SetActive(false);

        // set filter
        VisibilityController.Instance.ShowObject(id);
        
    }

    // lists properties in a formatted list
    private void SetInfoText(string info)
    {
        // split up properties
        string[] properties = info.Split(new string[] { ";;;" }, System.StringSplitOptions.None);
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


