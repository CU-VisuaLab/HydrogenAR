// ---------------------------------------------------------------------
//
// Copyright (c) 2018 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/creator-terms
//
// ---------------------------------------------------------------------

using UnityEngine;

namespace MagicLeap.Utilities
{
    ///<summary>
    /// Rotates a transform to be camera facing.
    ///</summary>
    public class Billboard : MonoBehaviour
    {
        //----------- Private Members -----------

        [SerializeField] private bool _lookAway = false;
        [SerializeField] private bool _billboardOnUpdate = true;
        private enum Axis
        {
            Y,
            XY,
            XYZ
        }

        [SerializeField] private Axis _axis;

        private Camera _mainCam;

        //----------- MonoBehaviour Methods -----------

        private void Start()
        {
            _mainCam = Camera.main;
        }

        private void Update()
        {
            if (_billboardOnUpdate)
            {
                Adjust();
            }
        }

        //----------- Public Methods -----------

        ///<summary>
        ///  This function will orient the gameobject based on the axis and look direction
        ///</summary> 
        public void Adjust()
        {
            Vector3 lookDir = (_mainCam.transform.position - transform.position).normalized;
            if(_lookAway)
            {
                lookDir = -lookDir;
            }
            Vector3 up = Vector3.up;

            switch (_axis)
            {
                case Axis.Y:
                    // zero out the y component of the look direction.
                    lookDir = new Vector3(lookDir.x, 0, lookDir.z);
                    break;
                case Axis.XY:
                    break;
                case Axis.XYZ:
                    up = _mainCam.transform.up;
                    break;
            };
            transform.rotation = Quaternion.LookRotation(lookDir, up);
        }
    }
}

