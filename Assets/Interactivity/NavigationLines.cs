using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationLines : MonoBehaviour
{
    public GameObject[] pieces;
    public GameObject[] associatedObjects;
    private List<GameObject> navigationLines;
    // Start is called before the first frame update
    void Start()
    {
        navigationLines = new List<GameObject>();
        foreach (GameObject piece in pieces)
        {
            GameObject navLine = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            navLine.transform.GetComponent<Renderer>().material = Resources.Load("Materials/TransparencyStripes") as Material;
            navLine.SetActive(false);
            navLine.transform.parent = transform;
            navigationLines.Add(navLine);
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (var i = 0; i < navigationLines.Count; i++)
        {
            GameObject navLine = navigationLines[i];
            navLine.transform.localScale = 
                new Vector3(0.02f, 0.5f * Vector3.Distance(pieces[i].transform.position, associatedObjects[i].transform.position), 0.02f);
            navLine.transform.position = (pieces[i].transform.position + associatedObjects[i].transform.position) / 2;
            navLine.transform.up = associatedObjects[i].transform.position - navLine.transform.position;
        }
    }
}
