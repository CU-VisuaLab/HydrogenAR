using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WIMPiece : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Renderer>().material = new Material(Resources.Load("Materials/Default") as Material);
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void setValue(float val)
    {
        string rackParent = transform.parent.name.Split('_')[0];
        if (GameObject.Find(rackParent + "_FocusObject") != null) 
            GameObject.Find(rackParent + "_FocusObject").GetComponent<WIMFocusObject>().UpdateValue(val);
    }
}
