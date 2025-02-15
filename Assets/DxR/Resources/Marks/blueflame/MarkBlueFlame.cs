﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DxR
{
    /// <summary>
    /// This is the class for point mark which enables setting of channel
    /// values which may involve calling custom scripts. The idea is that 
    /// in order to add a custom channel, the developer simply has to implement
    /// a function that takes in the "channel" name and value in string format
    /// and performs the necessary changes under the SetChannelValue function.
    /// </summary>
    public class MarkBlueFlame : Mark
    {
        public MarkBlueFlame() : base()
        {
            
        }

        public override List<string> GetChannelsList()
        {
            List<string> myChannels = new List<string>() { "size" };
            myChannels.AddRange(base.GetChannelsList());

            return myChannels;
        }


        public override void SetChannelValue(string channel, string value)
        {
            switch (channel)
            {
                case "size":
                    SetSize(value);
                    break;
                default:
                    base.SetChannelValue(channel, value);
                    break;
            }
        }

        private void SetSize(string value)
        {
            ParticleSystem ps1 = gameObject.transform.Find("Fire_Frost_CampFire").gameObject.GetComponent<ParticleSystem>();
            if(ps1 != null)
            {
                var main = ps1.main;
                main.startSize = float.Parse(value);
            }
            /*
            ParticleSystem ps2 = gameObject.transform.Find("Fire_Orange_Living").gameObject.GetComponent<ParticleSystem>();
            if (ps2 != null)
            {
                var main = ps2.main;
                main.startSize = float.Parse(value);
            }

            
            */

            ParticleSystem ps3 = gameObject.transform.Find("Smoke_CampFire").gameObject.GetComponent<ParticleSystem>();
            if (ps3 != null)
            {
                var main = ps3.main;
                main.startSize = float.Parse(value);
            }
        }
    }
}
