using UnityEngine;
using System.Collections;

public class DestroyTile : MonoBehaviour
{
    private int id; // area_id of this gameobject
    public int ID
    {
        set
        {
            id = value;
        }
    }


    private void OnDestroy()
    {
        VisibilityController.Instance.RemoveObject(id);
    }
}
