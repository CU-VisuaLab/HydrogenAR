using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AggregateSummary : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public virtual void UpdateValue(float value) { }
    public virtual void UpdatePlaybackSpeed(float frequency) { }
    public virtual void SetActive(bool activation) { }
}
