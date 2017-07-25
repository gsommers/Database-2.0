using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// handles panels that can be hidden and displayed
public class Window : MonoBehaviour {

    // called by open and close buttons
    public void SetVisibility(string button)
    {
        if (button.CompareTo("open") == 0)
        {
            gameObject.SetActive(true); // show window

            // reset scroll window to top
            ScrollRect scroll = gameObject.GetComponentInChildren<ScrollRect>();
            if (scroll != null)
                scroll.verticalNormalizedPosition = 1;
        }
        else if (button.CompareTo("close") == 0)
            gameObject.SetActive(false); // close window
        
    }
}
