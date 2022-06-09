using UnityEngine;
using SimpleFileBrowser;
using System.IO;

namespace WUInity
{
    public partial class WUInityGUI
    {
        string opticalDensity, stallSpeed;
        bool trafficMenuDirty = true;
        string[] opticalDensityFilter = new string[] { ".odr" };        


        void TrafficMenu()
        {
            TrafficInput tO = WUInity.INPUT.traffic;
            if (trafficMenuDirty)
            {
                trafficMenuDirty = false;
                stallSpeed = tO.stallSpeed.ToString();
                opticalDensity = tO.opticalDensity.ToString();
            }
            GUI.Box(new Rect(120, 0, columnWidth + 40, Screen.height - consoleHeight), "");
            int buttonIndex = 0;
            
            //settings
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

                if (WUInity.DATA_STATUS.OpticalDensityLoaded)
                {
                    GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Optical density ramp loaded");
                    ++buttonIndex;
                }
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Load optical density"))
                {
                    OpenLoadOpticalDensityFile();
                }
                ++buttonIndex;
            } 

            if (WUInity.INSTANCE.DeveloperMode)
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

            float.TryParse(stallSpeed, out tO.stallSpeed);
            float.TryParse(opticalDensity, out tO.opticalDensity);
        }        

        void OpenLoadOpticalDensityFile()
        {
            FileBrowser.SetFilters(false, opticalDensityFilter);
            string initialPath = Path.GetDirectoryName(WUInity.WORKING_FILE);
            FileBrowser.ShowLoadDialog(LoadOpticalDensityFile, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Load optical density ramp file", "Load");
        }

        void LoadOpticalDensityFile(string[] paths)
        {
            WUInityInput wO = WUInity.INPUT;
            wO.traffic.opticalDensityFile = paths[0];
            WUInity.RUNTIME_DATA.LoadOpticalDensityFile();
        }
    }
}
