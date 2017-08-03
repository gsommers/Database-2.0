using Mapbox.Unity.Map;
using Mapbox.Unity.MeshGeneration.Data;
using Mapbox.Unity.Utilities;
using Mapbox.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// detects when user clicks on or around a tileset

public class Raycasting : MonoBehaviour
{
    public Slider radiusSlide; // determines radius of search area

    public Camera cam; // renders everything except the geology map

    private Ray ray; // imaginary line directed from mouse position

    public TextMeshProUGUI materialsList; // displays which species are
    public GameObject materialsPanel; // at this location
    public Window materialsWindow; // scroll view of materialsPanel
    public GameObject infoPanel; // profile of selected species/region
    public GameObject mapPanel; // map of selected species
    public GameObject geoCam; // map of geology

    // what to search for
    public Toggle timber;
    public Toggle stone;

    // handles double clicks
    private bool clicked = false;
    private float firstClickTime;
    public float interval; // how long to wait between clicks

    public float loadTime; // how long to wait for tilesets
    private WaitForSeconds delay;
    private bool loading = false; // am I waiting for tilesets to load after zooming?

    public ZoomAndPan map; // interactive map of world
    public AbstractMap geoMap; // interactive map of geology

    private Vector2d click; // where user clicked, in meters
    private SortedDictionary<int, List<string>> display;
    private Dictionary<string, double> found;
    Vector2d point; // where the collision is

    private void Start()
    {
        display = new SortedDictionary<int, List<string>>(); // sorts species names by categories of distance from click
        found = new Dictionary<string, double>(); // contains the name of and distance to all of the species within range
        delay = new WaitForSeconds(loadTime);
        VisibilityController.Instance.Awake();
    }

    // if user just zoomed in/out, they can't immediately look for species
    IEnumerator DelayForLoad()
    {
        loading = true;
        yield return delay;
        loading = false;

        // close window displaying loading message
        materialsPanel.SetActive(false);
    }

    // called when user zooms
    public void DelayLoad()
    {
        StartCoroutine(DelayForLoad());
    }

    private void Update()
    {
        // only if user double clicks on map
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            // first click
            if (!clicked)
            {
                clicked = true;
                firstClickTime = Time.time;
            }

            // second click
            else
            {
                clicked = false; // reset clicks

                // show materials panel
                materialsPanel.SetActive(true);
                infoPanel.SetActive(false);
                mapPanel.SetActive(false);
                geoCam.SetActive(false);
                materialsWindow.SetVisibility("open"); // resets scroll position

                // detects if mouse clicks intersects with any tileset
                ray = cam.ScreenPointToRay(Input.mousePosition);
                // Debug.Log(Input.mousePosition);

                Vector2d clickCoords = (cam.ScreenToWorldPoint(Input.mousePosition) - new Vector3(0, cam.transform.position.y, 0))
                       .GetGeoPosition(map.CenterMercator, map.WorldRelativeScale); // geographic location of point click (in long lat)
                click = Conversions.LatLonToMeters(clickCoords); // in meters
                geoMap.Search(clickCoords);


                // assuming no materials selected
                materialsList.text = "\n<size=30pt>No materials selected. Please select stone and/or timber to retrieve data.</size>";

                // layermask - which materials to display? 
                if (timber.isOn || stone.isOn)
                {
                    materialsList.text = ""; // reset text
                    if (timber.isOn)
                        ShowList(8);
                    if (stone.isOn)
                        ShowList(9);
                }

            }
        }

        // user didn't click, time for double click has passed
        else if (clicked && Time.time - firstClickTime > interval)
        {
            clicked = false;
        }
    }

    private void ShowList(int layerMask)
    {
        RaycastHit[] hits = Physics.SphereCastAll(ray, radiusSlide.value * 1000 * map.WorldRelativeScale, 120, 1 << layerMask);

        //reset dictionaries for new search
        found.Clear();
        display.Clear();


        // no tilesets found
        if (hits.Length == 0)
        {
            if (loading) // something might be here, it just hasn't loaded
                materialsList.text = "\n<size=30pt>Map is loading. Please click again when this panel closes.</size>";
            else // nothing is here
                SetMissingText(LayerMask.LayerToName(layerMask));
            // Debug.Log("Nothing here...");
        }

        else // found tilesets!
        {
            double d;
            foreach (RaycastHit hit in hits)
            {
                // geographic location of collision with shapefile (in meters)
                point = Conversions.LatLonToMeters(hit.point.GetGeoPosition(map.CenterMercator, map.WorldRelativeScale));

                d = Vector2d.Distance(point / 1000, click / 1000); // distance from click location to collision in km

                // within range (I think it should always be, but just in case, here's another check)
                if (d < radiusSlide.value)
                {
                    // GetComponent is expensive. Better way?
                    string name = hit.transform.gameObject.GetComponent<Mapbox.Unity.MeshGeneration.Components.FeatureBehaviour>().DataString;

                    // already found a collision with this shapefile; keep the closer one
                    if (found.ContainsKey(name))
                    {
                        double distance = found[name];
                        if (d < distance)
                        {
                            found.Remove(name);
                            found.Add(name, d);
                        }
                    }

                    else
                        found.Add(name, d);
                }
            }

            // Debug.Log(found.Count + "vs." + hits.Length);
            if (found.Count == 0) // no tilesets within range
            {
                SetMissingText(LayerMask.LayerToName(layerMask));
            }

            else
            {
                // go through kv pairs and assign to categories
                foreach (KeyValuePair<string, double> pair in found)
                {
                    int category = Category(pair.Value);

                    // other tilesets are in the same distance range
                    if (display.ContainsKey(category))
                    {
                        display[category].Add(pair.Key);
                    }

                    else // this is the first hit at this distance
                    {
                        List<string> thisList = new List<string>();
                        thisList.Add(pair.Key);
                        display.Add(category, thisList);
                    }
                }

                // display by distance categories
                StringBuilder text = new StringBuilder();
                foreach (KeyValuePair<int, List<string>> pair in display)
                {
                    // category label
                    text.Append(string.Format("\n<i>Within a {0}-km radius:</i>\n", pair.Key));

                    if (layerMask == 8) // species list for wood
                    {
                        foreach (string species in pair.Value)
                        {

                            text.Append("<size=90%><link=\"id_tree\"><color=\"blue\"><u>" + species + "</color></u></link></size>\n");
                        }

                    }
                    else // stone
                    {
                        foreach (string id in pair.Value)
                        {
                            text.Append("<size=90%><link=\"" + id + "\"><color=\"blue\"><u>Find stone</color></u></link></size>\n");
                        }

                    }

                }
                SetListText(layerMask, text.ToString());
            }

        }
    }

    // sets the text listing materials found in region
    private void SetListText(int layerMask, string list)
    {
        if (layerMask == 8) // timber
            materialsList.text += "\n<size=120%><b>Click on a species of wood for more info.</b></size> \n" + list;
        else // stone
            materialsList.text += "\n<size=120%><b>Click on one of the links below to learn about stone in nearby regions.</b></size> \n" + list;
    }

    // this displays when there are no species in the region
    private void SetMissingText(string layerName)
    {
        materialsList.text += String.Format("<size=30pt>\nNo {0} found here. Click somewhere else or broaden your search.</size>\n", layerName);
    }

    // groups distances into intervals for display
    // double gets rounded up to nearest interval mark
    private int Category(double distance)
    {
        if (distance < 5)
            return 5;
        else if (distance < 50)
            return 50;
        else if (distance < 100)
            return 100;
        else if (distance < 200)
            return 200;
        else if (distance < 300)
            return 300;
        else if (distance < 400)
            return 400;
        else
            return 500;
    }

}