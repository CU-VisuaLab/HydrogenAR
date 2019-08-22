using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DetailHover : MonoBehaviour
{
    private string value;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void showDetail()
    {
        GameObject.Find("HostText").GetComponent<Text>().text = transform.name + ": " + value;
    }

    public void updateDetail(string newDetail)
    {
        value = newDetail;
    }
}
