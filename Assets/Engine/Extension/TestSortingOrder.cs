using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TestSortingOrder : MonoBehaviour 
{
    public int order = 0;

    Renderer[] renderers;
	// Use this for initialization
	void Start () 
	{
        renderers = gameObject.GetComponentsInChildren<Renderer>();


        for (int i = 0; i < renderers.Length; ++i)
        {
            renderers[i].sortingOrder = order;
        }
    }


    private void OnValidate()
    {
        if (renderers == null)
        {
            return;
        }
        for (int i = 0; i < renderers.Length; ++i)
        {
            renderers[i].sortingOrder = order;
        }
    }
}
