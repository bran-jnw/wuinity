using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace WUInity
{
    public partial class WUInityGUI
    {
        string opticalDensity, stallSpeed, borderSize;
        bool trafficInputDirty = true;
        void TrafficMenu()
        {
            TrafficInput tO = WUInity.INPUT.traffic;
            ItineroInput iO = WUInity.INPUT.itinero;
            if (trafficInputDirty)
            {
                trafficInputDirty = false;
                stallSpeed = tO.stallSpeed.ToString();
                opticalDensity = tO.opticalDensity.ToString();
                borderSize = iO.osmBorderSize.ToString();
            }
            GUI.Box(new Rect(120, 0, columnWidth + 40, Screen.height - consoleHeight), "");
            int buttonIndex = 0;

            string loadText = "Load router Database";
            if (WUInity.INSTANCE.routerDbLoaded)
            {
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Status: RouterDb loaded");
                loadText = "Re-load router Database";
            }
            else
            {
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Status: RouterDb NOT loaded");
            }
            ++buttonIndex;

            //name
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "OSM file");
            ++buttonIndex;
            iO.osmFile = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), iO.osmFile);
            ++buttonIndex;
            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Load OSM file"))
            {
                OpenLoadOSM();
            }
            ++buttonIndex;

            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "OSM border size [m]");
            ++buttonIndex;
            borderSize = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), borderSize);
            ++buttonIndex;

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), loadText))
            {
                ParseTrafficInput();
                WUInity.SIM.LoadItineroDatabase();
            }
            ++buttonIndex;

            /*if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Load pre-calc routes"))
            {
                ParseTrafficInput();
                WUInity.WUINITY_SIM.LoadRouteCollections();
            }
            ++buttonIndex;*/

            //jam speed
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Capacity speed");
            ++buttonIndex;
            stallSpeed = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), stallSpeed);
            ++buttonIndex;

            //smoke stuff
            tO.visibilityAffectsSpeed = GUI.Toggle(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), tO.visibilityAffectsSpeed, "Speed is affected by smoke");
            ++buttonIndex;

            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Optical density [1/m]");
            ++buttonIndex;
            opticalDensity = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), opticalDensity);
            ++buttonIndex;

            if (WUInity.INSTANCE.developerMode)
            {
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Run traffic verification"))
                {
                    Traffic.MacroTrafficVerification.RunTrafficVerificationTests();
                }
                ++buttonIndex;
            }
        }

        void ParseTrafficInput()
        {
            if (trafficInputDirty)
            {
                return;
            }

            TrafficInput tO = WUInity.INPUT.traffic;
            ItineroInput iO = WUInity.INPUT.itinero;

            float.TryParse(stallSpeed, out tO.stallSpeed);
            float.TryParse(opticalDensity, out tO.opticalDensity);
            float.TryParse(borderSize, out iO.osmBorderSize);
        }
    }
}
