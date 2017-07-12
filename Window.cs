using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// handles panels that can be hidden and displayed
public class Window : MonoBehaviour {

    // called by open and close buttons
    public void SetVisibility(string button)
    {
        if (button.CompareTo("open") == 0)
            gameObject.SetActive(true); // show window
        else if (button.CompareTo("close") == 0)
            gameObject.SetActive(false); // close window
    }
}
