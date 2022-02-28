using UnityEngine;
using SimpleFileBrowser;
using System.IO;

namespace WUInity
{
    public partial class WUInityGUI
    {
        string opticalDensity, stallSpeed, borderSize;
        bool trafficMenuDirty = true;
        void TrafficMenu()
        {
            TrafficInput tO = WUInity.INPUT.traffic;
            if (trafficMenuDirty)
            {
                trafficMenuDirty = false;
                stallSpeed = tO.stallSpeed.ToString();
                opticalDensity = tO.opticalDensity.ToString();
                borderSize = tO.osmBorderSize.ToString();
            }
            GUI.Box(new Rect(120, 0, columnWidth + 40, Screen.height - consoleHeight), "");
            int buttonIndex = 0;

            //router db
            string routerStatus = "RouterDb NOT loaded";
            string loadRouterText = "Load router database";
            if (WUInity.DATA_STATUS.routerDbLoaded)
            {
                routerStatus = "RouterDb loaded";
                loadRouterText = "Re-load router database";
            }
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), routerStatus);
            ++buttonIndex;

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), loadRouterText))
            {
                ParseTrafficInput();
                WUInity.SIM_DATA.LoadRouterDatabase();
            }
            ++buttonIndex;

            //route collection
            ++buttonIndex;
            string routeCollectionStatus = "Pre-calc routes NOT loaded";
            string loadRouteCollectionsText = "Load pre-calc routes";
            if (WUInity.DATA_STATUS.routeCollectionLoaded)
            {                
                routeCollectionStatus = "Pre-calc routes loaded";
                loadRouteCollectionsText = "Re-load pre-calc routes";
            }
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), routeCollectionStatus);
            ++buttonIndex;

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), loadRouteCollectionsText))
            {
                ParseTrafficInput();
                WUInity.SIM_DATA.LoadRouterDatabase();
            }
            ++buttonIndex;

            //OSM
            ++buttonIndex;
            string osmStatus = "OSM file NOT valid";
            if (WUInity.DATA_STATUS.osmFileValid)
            {
                osmStatus = "OSM file valid";
            }
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), osmStatus);
            ++buttonIndex;

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Select OSM source file"))
            {
                OpenSetOSMFile();
            }
            ++buttonIndex;            
            
            if(WUInity.DATA_STATUS.osmFileValid)
            {
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "OSM border size [m]");
                ++buttonIndex;
                borderSize = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), borderSize);
                ++buttonIndex;

                string buildRouterText = "Build router database";
                if(WUInity.DATA_STATUS.routerDbLoaded)
                {
                    buildRouterText = "Re-build router database";
                }
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), buildRouterText))
                {
                    
                }
                ++buttonIndex;
            }

            //settings
            ++buttonIndex;
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Capacity speed");
            ++buttonIndex;
            stallSpeed = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), stallSpeed);
            ++buttonIndex;

            //smoke stuff
            tO.visibilityAffectsSpeed = GUI.Toggle(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), tO.visibilityAffectsSpeed, "Speed is affected by smoke");
            ++buttonIndex;

            if(tO.visibilityAffectsSpeed)
            {
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Optical density [1/m]");
                ++buttonIndex;
                opticalDensity = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), opticalDensity);
                ++buttonIndex;
            }            

            if (WUInity.INSTANCE.developerMode)
            {
                ++buttonIndex;
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Run traffic verification"))
                {
                    Traffic.MacroTrafficVerification.RunTrafficVerificationTests();
                }
                ++buttonIndex;
            }
        }

        void ParseTrafficInput()
        {
            if (trafficMenuDirty)
            {
                return;
            }

            TrafficInput tO = WUInity.INPUT.traffic;
            ItineroInput iO = WUInity.INPUT.itinero;

            float.TryParse(stallSpeed, out tO.stallSpeed);
            float.TryParse(opticalDensity, out tO.opticalDensity);
            float.TryParse(borderSize, out tO.osmBorderSize);
        }

        void OpenSetOSMFile()
        {
            FileBrowser.SetFilters(false, osmFilter);
            string initialPath = Path.GetDirectoryName(WUInity.WORKING_FILE);
            FileBrowser.ShowLoadDialog(SetOSMFile, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Set OSM file", "Set");
        }

        void SetOSMFile(string[] paths)
        {
            WUInity.INPUT.traffic.osmFile = paths[0];
            WUInity.INSTANCE.UpdateOSMResourceStatus();
        }
    }
}
