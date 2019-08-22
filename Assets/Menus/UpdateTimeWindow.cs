using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateTimeWindow : MonoBehaviour
{
    MLControllerRadialMenu previousMenu;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void initializeButtons(GameObject menuObject)
    {
        Button[] buttons = menuObject.transform.GetComponentsInChildren<Button>();
        foreach (Button button in buttons)
        {
            if (button.transform.name.ToLower() != "back")
            {
                button.onClick.AddListener(() => useTimeWindow(button.transform.name));
            }
        }
    }

    private void useTimeWindow(string window)
    {
        // Manually account for time window names
        if (window.ToLower() == "realtime")
        {

        }
        else if (window.Replace(" ", "").ToLower() == "24hours")
        {

        }
        else if (window.ToLower() == "month")
        {

        }
        else if (window.ToLower() == "year")
        {

        }
    }
}
