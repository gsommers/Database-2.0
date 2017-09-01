using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class ReadText : MonoBehaviour
{
    public TextAsset stones;

    void Start()
    {
        /* string[] headers = {"Rock Type #1", "Rock Type #2", "Rock Type #3",  "Rock Type #4",  "Era Name",
            "Oldest Age (MYA)",  "Newest Age (MYA)", "Region Name"};*/
        string[] lines = stones.text.Split('\n');
        string[] headers = {"Common name(s)", "Alternative name(s)", "Petrographic Definition/Details", "Age", "Appearance and Composition",
            "General Density", "Elastic Modulus", "Poisson's Ratio", "Flexural Strength", "Compressive Strength", "Water Absorption",
            "Quarry Location(s)",  "Dates of Use", "Recorded Use", "Other qualities", "Primary sources"};
        for (int i = 0; i < lines.Length; i++)
        {
            string[] props = lines[i].Split('\t');
            StringBuilder sb = new StringBuilder();
            for (int j = 0; j < headers.Length && j < props.Length; j++)
            {
                if (props[j].Trim().Length > 0)
                {
                    String prop = props[j].Replace("^2", "<sup>2</sup>");
                    sb.Append(headers[j] + "::: " + prop.Replace("^3", "<sup>3</sup>") + ";;;");
                }
                else
                    sb.Append(headers[j] + "::: " + "no data available;;;");
            }
            lines[i] = sb.ToString();
        }
        System.IO.File.WriteAllLines(@"C:\Users\Grace Sommers\Desktop\pietra.txt", lines);
        
    }
}
