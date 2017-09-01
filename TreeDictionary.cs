
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
    private SortedDictionary<string, string> species;
    public TreeDictionaryCreator treeDictionary;
    private TextMeshProUGUI treeInfo; // displays text held in species

    // holds info about stone types, keyed by common names
    private SortedDictionary<string, string> rocks;
    public QuarryDictionaryCreator quarryDictionary;

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

    [SerializeField]
    Texture2D texture;
    private void Awake()
    {
        species = treeDictionary.materials;
        rocks = quarryDictionary.materials;

        // initialize the array of stones
        stoneInfo = stones.text.Split('\n');

        // text and window script of info panel
        treeInfo = treeDisplay.GetComponentInChildren<TextMeshProUGUI>();
        treeWindow = treeDisplay.GetComponent<Window>();

        var image = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "britain")).LoadAsset<TextAsset>("corsiTest");
        File.WriteAllBytes(Path.Combine(Application.persistentDataPath, "corsiTest2.jpg"), image.bytes);
        texture = new Texture2D(2, 2);
        texture.LoadImage(File.ReadAllBytes(Path.Combine(Application.persistentDataPath, "corsiTest2.jpg")));

        
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
        if (geoMap != null)
            geoMap.SetActive(false);
    }

    // called when user clicks on a tree species name
    public void Read(string name)
    {
        if (geoMap != null)
            geoMap.SetActive(false);
        string value;
        species.TryGetValue(name.ToLowerInvariant(), out value); // try to get this species from dictionary
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
        treeWindow.SetVisibility(true);
    }

    // called when user clicks on a stone type
    public void ReadQuarry(string name)
    {
        string value;
        rocks.TryGetValue(name.ToLowerInvariant(), out value); // try to get this rock type from dictionary
        if (value == null) // no rock with this name is in the dictionary
            treeInfo.text = "Rock type not in database";
        else // found rock!
        {
            // title
            treeInfo.text = "<color=#336666FF><size=130%><smallcaps><b>" + name + "</size></smallcaps></color></b>\n";

            // properties
            SetInfoText(value);
        }
        treeWindow.SetVisibility(true);

    }

    // process stone when user clicks on "Find stone"
    public void ReadStone(string areaId)
    {
        int id;
        Int32.TryParse(areaId, out id);
        treeWindow.SetVisibility(true);
        treeInfo.text = "\n\n"; // title?
        SetInfoText(stoneInfo[id - 1]); // area ID corresponds to info about the geology of the area

        // show stone map, not tree map
        geoMap.SetActive(true);
        geoMap.transform.position = new Vector3(0, 150, -50); // reset camera position 
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


