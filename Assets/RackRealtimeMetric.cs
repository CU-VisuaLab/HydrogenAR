using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RackRealtimeMetric : MonoBehaviour
{
    private string rackName;
    private WIMPiece[] wimPieces;
    // Start is called before the first frame update
    void Start()
    {
        rackName = transform.name.Split('_')[0];
        GameObject wimHost = GameObject.Find("WIM_Model/[Pieces]/" + rackName);
        wimPieces = GetComponentsInChildren<WIMPiece>();
    }

    // Update is called once per frame
    void Update()
    {
    }
}
