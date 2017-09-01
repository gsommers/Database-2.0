using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class FlipPage : MonoBehaviour
{
    public TextMeshProUGUI listText;
    private int numPages; // how many pages does this list take up?
    public Button pageNext, pagePrevious;

    private void Start()
    {
        pageNext.onClick.AddListener(NextPage);
        pagePrevious.onClick.AddListener(PreviousPage);
    }
    
    public void SetPages()
    {
            try
        {
            numPages = listText.GetTextInfo(listText.text).pageCount;
        }
        catch
        {
            Debug.Log("Something went wrong"); // seems to occur the first time I open tree info
        }
        listText.pageToDisplay = 1;
        pagePrevious.interactable = false; // set to first page, so can't go back
        if (numPages == 1)
            pageNext.interactable = false; // no second page
        else
            pageNext.interactable = true;
    }

    void NextPage()
    {
        listText.pageToDisplay++;
        if (listText.pageToDisplay == numPages) // last page
            pageNext.interactable = false;
        pagePrevious.interactable = true;
    }

    void PreviousPage()
    {
        listText.pageToDisplay--;
        if (listText.pageToDisplay == 1) // first page
            pagePrevious.interactable = false;
        pageNext.interactable = true;

    }
}
