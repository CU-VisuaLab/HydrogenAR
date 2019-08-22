using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using DxR;
using UnityEngine.UI;

public class WIMMenu : MonoBehaviour
{
    private GameObject WIMModel;
    // Start is called before the first frame update
    void Awake()
    {
        WIMPiece[] pieces = Resources.FindObjectsOfTypeAll(typeof(WIMPiece)) as WIMPiece[];
        WIMModel = pieces[0].transform.root.gameObject;
        if (!WIMModel.GetComponentInChildren<Canvas>(true).gameObject.activeSelf) ToggleCheckbox(transform.Find("Labels"));
        if (WIMModel.GetComponentInChildren<NavigationLines>().transform.childCount > 0 && 
            WIMModel.GetComponentInChildren<NavigationLines>().transform.GetChild(0).gameObject.activeSelf) ToggleCheckbox(transform.Find("LinkToWorld"));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // TODO: Assumes only one DxR.Vis object in each focus cycle
    public void TimeSeriesClick()
    {
        WIMFocusObject[] allWIMFocusObjects = FindObjectsOfType<WIMFocusObject>();
        foreach (WIMFocusObject wimFocusObject in allWIMFocusObjects)
        {
            if (wimFocusObject.GetComponentInChildren<Vis>(true) == null) continue;
            while (!wimFocusObject.GetComponentInChildren<Vis>(true).gameObject.activeSelf)
                wimFocusObject.cycleFocusObject();
        }
    }

    // Assumes there is a canvas with a name containing "metric"; won't break if there isn't
    public void RealtimeClick()
    {
        WIMFocusObject[] allWIMFocusObjects = FindObjectsOfType<WIMFocusObject>();
        foreach (WIMFocusObject wimFocusObject in allWIMFocusObjects)
        {
            for (var i = 0; i < wimFocusObject.focusObjects.Length; i++)
            {
                if (wimFocusObject.GetComponentInChildren<Canvas>() != null &&
                    wimFocusObject.GetComponentInChildren<Canvas>().transform.name.ToLower().Contains("metric"))
                {
                    continue;
                }
                wimFocusObject.cycleFocusObject();
            }
        }
    }

    public void ToggleLabels()
    {
        Transform WIMLabelsParent = WIMModel.transform.Find("[Labels]");
        Canvas[] labelCanvases = WIMLabelsParent.GetComponentsInChildren<Canvas>(true);
        foreach (Canvas labelCanvas in labelCanvases)
        {
            GameObject.Find("DebugText").GetComponent<Text>().text = labelCanvas.transform.name;
            labelCanvas.gameObject.SetActive(!labelCanvas.gameObject.activeSelf);
        }
    }

    public void ToggleLines()
    {
        NavigationLines WIMNavigationLines = FindObjectOfType<NavigationLines>();
        foreach (Transform line in WIMNavigationLines.transform)
        {
            line.gameObject.SetActive(!line.gameObject.activeSelf);
        }
    }

    public void ToggleCheckbox(Transform buttonParent)
    {
        if (buttonParent.Find("Checkbox").GetComponent<Image>().color == Color.white)
        {
            buttonParent.Find("Checkbox").GetComponent<Image>().color = new Color(0.298f, .733f, .09f);
            buttonParent.Find("Checkmark").GetComponent<Image>().color = Color.white;
        }
        else
        {
            buttonParent.Find("Checkbox").GetComponent<Image>().color = Color.white;
            buttonParent.Find("Checkmark").GetComponent<Image>().color = new Color(0.8f,0.8f,0.8f);
        }
    }

    public void LoadMetricMenu()
    {
        GameObject.Find("LoadMetricMenu").GetComponent<OneDimensionalMenu>().Activate(FindObjectOfType<MLControllerRadialMenu>());
        Destroy(gameObject);
    }

    public void ToggleWIM()
    {
        WIMModel.SetActive(!WIMModel.activeSelf);
    }
}
