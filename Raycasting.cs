using Mapbox.Unity.Map;
using Mapbox.Unity.MeshGeneration.Data;
using Mapbox.Unity.Utilities;
using Mapbox.Utils;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// detects when user clicks on a shapefile
public class Raycasting : MonoBehaviour
{
    public Slider radiusSlide; // how wide of a radius to search
    private Ray ray; // imaginary line directed from mouse position

    public TextMeshProUGUI treeText; // displays which species are
    public GameObject speciesPanel; // at this location
    public GameObject treePanel; // profile of selected species
    public GameObject mapPanel; // map of selected species

    // what to search for
    public Toggle timber;
    public Toggle stone;

    // handles double clicks
    private bool clicked = false;
    private float firstClickTime;
    public float interval; // how long to wait between clicks

    public float loadTime; // how long to wait for tilesets
    private bool loading = false; // am I waiting for tilesets to load after zooming?

    public ZoomAndPan map;

    // if user just zoomed in/out, they can't immediately look for species
    IEnumerator DelayForLoad()
    {
        loading = true;
        yield return new WaitForSeconds(loadTime);
        loading = false;
        speciesPanel.SetActive(false);
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

                // show species panel
                speciesPanel.SetActive(true);
                treePanel.SetActive(false);
                mapPanel.SetActive(false);

                // reset info on panel
                treeText.text = "Click on a species of wood for more info. \n";

                // layermask - which materials to display? - doesn't currently work
                int layerMask = 0;
                if (timber.isOn)
                    layerMask = layerMask | (1 << 8);
                if (stone.isOn)
                    layerMask = layerMask | (1 << 9);
                // Debug.Log(layerMask);

                // detects if mouse clicks intersects with any tileset
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                // Debug.Log(Input.mousePosition);
                Vector2d click = Conversions.LatLonToMeters((Camera.main.ScreenToWorldPoint(Input.mousePosition) - new Vector3(0, 100, 0))
                       .GetGeoPosition(map.CenterMercator, map.WorldRelativeScale));
                RaycastHit[] hits = Physics.SphereCastAll(ray, 50);
                // RaycastHit[] hits = Physics.RaycastAll(ray);

                SortedDictionary<int, List<string>> display = new SortedDictionary<int, List<string>>();
                Dictionary<string, double> found = new Dictionary<string, double>();
                // no tilesets found
                if (hits.Length == 0)
                {
                    if (loading)
                        treeText.text = "<size=30pt>Map is loading. Please click again when this panel closes.</size>";
                    else
                        SetMissingText();
                }
                else // found tilesets!
                {
                    Debug.Log(hits.Length);
                    foreach (RaycastHit hit in hits)
                    {
                        Vector2d point = Conversions.LatLonToMeters(hit.point.GetGeoPosition(map.CenterMercator, map.WorldRelativeScale));
                        // Vector2d point = Conversions.LatLonToMeters(new Vector2d(40.3493, -74.6593));
                        // Vector2d click = Conversions.LatLonToMeters(new Vector2d(43.1573, -77.6152));
                        double d = Vector2d.Distance(point / 1000, click / 1000);

                        if (d < radiusSlide.value)
                        {
                            string name = hit.transform.gameObject.GetComponent<Mapbox.Unity.MeshGeneration.Components.FeatureBehaviour>().DataString;
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

                    if (found.Count == 0) // no tilesets within range
                    {
                        SetMissingText();
                    }
                    else
                    {
                        foreach (KeyValuePair<string, double> pair in found)
                        {
                            int category = Category(pair.Value);
                            if (display.ContainsKey(category))
                            {
                                display[category].Add(pair.Key);
                            }
                            else
                            {
                                List<string> thisList = new List<string>();
                                thisList.Add(pair.Key);
                                display.Add(category, thisList);
                            }
                        }

                        foreach (KeyValuePair<int, List<string>> pair in display)
                        {
                            treeText.text += string.Format("<i>Within a {0}-km radius:</i>\n", pair.Key);
                            foreach (string species in pair.Value)
                            {
                                treeText.text += "<size=90%><link=\"id_tree\"><color=\"blue\"><u>" + species + "</color></u></link></size>\n";
                            }
                        }
                        // Debug.Log(name);
                    }
                }
            }
        }

        // user didn't click, time for double click has passed
        else if (clicked && Time.time - firstClickTime > interval)
        {
            clicked = false;
        }
    }

    private void SetMissingText()
    {
        treeText.text = "<size=30pt>No material data found here. Click somewhere else or broaden your search.</size>";
    }

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
