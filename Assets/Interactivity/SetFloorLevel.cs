using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.MagicLeap;

public class SetFloorLevel: MonoBehaviour
{
    private float floorLevel;
    private bool initialized;
    // Start is called before the first frame update
    void Start()
    {
        initialized = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetLevel()
    {
        try
        {
            floorLevel = Mathf.Infinity;
            Transform meshParent = FindObjectOfType<MLSpatialMapper>().meshParent;
            foreach (Transform meshTransform in meshParent)
            {
                if (meshTransform.name.Contains("MLSpatialMapper") ||
                    meshTransform.GetComponent<MeshFilter>().mesh == null)
                {
                    continue;
                }
                for (var j = 0; j < meshTransform.GetComponent<MeshFilter>().mesh.vertexCount; j++)
                {
                    Vector3 meshVertex = meshTransform.GetComponent<MeshFilter>().mesh.vertices[j];
                    floorLevel = Mathf.Min(floorLevel, meshVertex.y);
                }
            }
            if (float.IsPositiveInfinity(floorLevel)) floorLevel = 0;
            initialized = true;
        }
        catch (Exception e) 
        {
            floorLevel = 0;
        }
    }

    public bool FloorInitialized()
    {
        return initialized;
    }

    public float GetFloorLevel()
    {
        return floorLevel + 0.1f;
    }
}
