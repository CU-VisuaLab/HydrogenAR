using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewManagement : MonoBehaviour
{
    public GameObject[] views;
    private int viewIndex;
    // Start is called before the first frame update
    void Start()
    {
        viewIndex = 0;
        foreach (GameObject view in views)
        {
            view.SetActive(false);
        }
        views[0].SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void nextView()
    {
        views[viewIndex].SetActive(false);
        viewIndex = (++viewIndex) % views.Length;
        views[viewIndex].SetActive(true);
    }
}
