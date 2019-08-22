using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ProxemicViewManagement : MonoBehaviour
{
    public bool distanceProxemics;
    public GameObject[] closeRangeObjects;
    public GameObject[] farRangeObjects;
    public float proximityDistanceThreshold = 1;

    private List<float> distances;
    private int movingAverageLength = 150;
    // Start is called before the first frame update
    void Start()
    {
        distances = new List<float>();
    }

    // Update is called once per frame
    void Update()
    {
        if (distanceProxemics) UpdateViewByDistance();
    }

    private void UpdateViewByDistance()
    {
        distances.Add(Vector3.Distance(Camera.main.transform.position, transform.position));
        if (distances.Count >= movingAverageLength) distances.RemoveAt(0);
        if (distances.Sum() / distances.Count < proximityDistanceThreshold)
        {
            foreach(GameObject closeRangeObject in closeRangeObjects) closeRangeObject.SetActive(true);
            foreach(GameObject farRangeObject in farRangeObjects) farRangeObject.SetActive(false);
        }
        else
        {
            foreach (GameObject closeRangeObject in closeRangeObjects) closeRangeObject.SetActive(false);
            foreach (GameObject farRangeObject in farRangeObjects) farRangeObject.SetActive(true);
        }
    }
}
