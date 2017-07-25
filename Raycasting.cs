using Mapbox.Unity.MeshGeneration.Data;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// detects when user clicks on a shapefile
public class Raycasting : MonoBehaviour
{

    private Ray ray; // imaginary line directed from mouse position

    public TextMeshProUGUI treeText; // displays which species are
    public GameObject speciesPanel; // at this location
    public GameObject treePanel;

    // what to search for
    public Toggle timber;
    public Toggle stone;

    // handles double clicks
    private bool clicked = false;
    private float firstClickTime;
    public float interval; // how long to wait between clicks

    public float loadTime; // how long to wait for tilesets
    private bool loading = false; // am I waiting for tilesets to load after zooming?

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

                // reset info on panel
                treeText.text = "Click on a species for more info. \n";

                // layermask - which materials to display? - doesn't currently work
                int layerMask = 0;
                if (timber.isOn)
                    layerMask = layerMask | (1 << 8);
                if (stone.isOn)
                    layerMask = layerMask | (1 << 9);
                // Debug.Log(layerMask);

                // detects if mouse clicks intersects with any tileset
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit[] hits = Physics.RaycastAll(ray, layerMask);

                // no tilesets found
                if (hits.Length == 0)
                {
                    if (loading)
                        treeText.text = "<size=30pt>Map is loading. Please click again when this panel closes.</size>";
                    else
                        treeText.text = "<size=30pt>No material data found here. Click somewhere else or broaden your search.</size>";
                }
                else // found tilesets!
                    foreach (RaycastHit hit in hits)
                    {
                        string name = hit.transform.gameObject.GetComponent<Mapbox.Unity.MeshGeneration.Components.FeatureBehaviour>().DataString;
                        treeText.text += "<size=90%><link=\"id_tree\"><color=\"blue\"><u>" + name + "</color></u></link></size>\n";
                        // Debug.Log(name);
                    }

            }
        }

        // user didn't click, time for double click has passed
        else if (clicked && Time.time - firstClickTime > interval)
        {
            clicked = false;
        }
    }
}
