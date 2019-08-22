using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;
using System.Linq;
using UnityEngine.UI;
using System;

public class UpdateGazeFocus : MonoBehaviour
{
    private List<GazeFocusable> hitObjects;
    public int bufferLength = 20;
    // Start is called before the first frame update
    void Start()
    {
        hitObjects = new List<GazeFocusable>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnRaycastHit(MLWorldRays.MLWorldRaycastResultState state, RaycastHit result, float confidence)
    {       
        try
        {
            if (state != MLWorldRays.MLWorldRaycastResultState.RequestFailed &&
               state != MLWorldRays.MLWorldRaycastResultState.NoCollision &&
               result.transform != null &&
               result.transform.GetComponentInParent<GazeFocusable>() != null)
            {
                GazeFocusable[] focusables = FindObjectsOfType<GazeFocusable>();
                foreach (GazeFocusable focusable in focusables) focusable.SetLookingAt(false);
                result.transform.GetComponentInParent<GazeFocusable>().SetLookingAt(true);
                hitObjects.Add(result.transform.GetComponentInParent<GazeFocusable>());
                while (hitObjects.Count > bufferLength) hitObjects.RemoveAt(0);
                GazeFocusable focused = GetFocusedObject();
                if (focused != null && !focused.ActiveFocusObject() && !focused.hasBeenInvoked())
                {
                    hitObjects = new List<GazeFocusable>();
                    focused.InvokeGaze(result.point, result.normal);
                    focused.SetFocusObjectActive(true);
                }
            }
        }
        catch (Exception e)
        {

            GameObject.Find("DebugText").GetComponent<Text>().text = e.Message;
        }
    }

    private GazeFocusable GetFocusedObject()
    {
        if (hitObjects.Count < bufferLength) return null;
        for (var i = 0; i < hitObjects.Count / 4 + 1; i++)
        {
            if (hitObjects.Count(obj => obj == hitObjects[i]) >= (3 * hitObjects.Count / 4))
            {
                return hitObjects[i];
            }
        }
        return null;
     }
}
