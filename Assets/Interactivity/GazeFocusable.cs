using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

public class GazeFocusable : MonoBehaviour
{
    private bool lookingAt;
    private bool focusObjectActive;
    private bool invoked;
    // Start is called before the first frame update
    void Awake()
    {
        lookingAt = false;
        focusObjectActive = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public virtual void InvokeGaze(Vector3 hitPoint, Vector3 hitForward)
    {
    }

    public void SetLookingAt(bool looking)
    {
        lookingAt = looking;
    }
    public bool IsGazed()
    {
        return lookingAt;
    }
    public void SetFocusObjectActive(bool active)
    {
        focusObjectActive = active;
    }
    public bool ActiveFocusObject()
    {
        return focusObjectActive;
    }
    public void invoke()
    {
        invoked = true;
    }
    public bool hasBeenInvoked()
    {
        return invoked;
    }
}
