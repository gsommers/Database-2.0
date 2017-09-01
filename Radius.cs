using UnityEngine;
using System.Collections;
using TMPro;

public class Radius : MonoBehaviour
{
    public TextMeshProUGUI radiusText;
    // Use this for initialization
    void Start()
    {
        radiusText = gameObject.GetComponent<TextMeshProUGUI>();
    }

    public void SetRadius(float radius)
    {
        radiusText.text = radius.ToString();
    }
}
