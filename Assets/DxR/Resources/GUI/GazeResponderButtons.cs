// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

// This example is built from HoloToolkit examples package.

using UnityEngine;

namespace DxR
{
    /// <summary>
    /// This class implements IFocusable to respond to gaze changes.
    /// It highlights the object being gazed at.
    /// </summary>

    #if UNITY_WSA_10_0	
    public class GazeResponderButtons : MonoBehaviour, HoloToolkit.Unity.InputModule.IFocusable
    #else
    public class GazeResponderButtons : MonoBehaviour
    #endif
    {
       
        private void Start()
        {
           
        }

        private void Update()
        {
            
        }
        
        public void OnFocusEnter()
        {
           
        }

        private void SetvisObjectVisibility(bool visib)
        {
        }

        public void OnFocusExit()
        {

        }

        private void OnDestroy()
        {
            
        }
    }
}