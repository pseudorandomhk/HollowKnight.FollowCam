using System;
using UnityEngine;

namespace FollowCam
{
    [Serializable]
    public class GlobalSettings
    {
        public static GlobalSettings instance = new();

        public float camProportion = 0.25f;

        public string toggleEnabled;
        public string toggleBlankerShown;
        public string followCamChangeHitboxState;
        public string mainCamChangeHitboxState;
        public string zoomIn;
        public string zoomOut;
    }
}