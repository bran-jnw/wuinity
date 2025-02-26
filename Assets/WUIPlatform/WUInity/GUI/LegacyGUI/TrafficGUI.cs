using UnityEngine;
using SimpleFileBrowser;
using System.IO;
using WUIPlatform.IO;

namespace WUIPlatform.WUInity.UI
{
    public partial class WUInityGUI
    {
        string stallSpeed;
        bool trafficMenuDirty = true;
        //string[] opticalDensityFilter = new string[] { ".odr" };        


        void TrafficMenu()
        {
            TrafficInput tO = WUIEngine.INPUT.Traffic;
            if (trafficMenuDirty)
            {
                trafficMenuDirty = false;
                stallSpeed = tO.macroTrafficSimInput.stallSpeed.ToString();
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

            /*if(tO.visibilityAffectsSpeed)
            {
                if (WUIEngine.DATA_STATUS.OpticalDensityLoaded)
                {
                    GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Optical density ramp loaded");
                    ++buttonIndex;
                }
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Load optical density"))
                {
                    OpenLoadOpticalDensityFile();
                }
                ++buttonIndex;
            } */
        }

        void ParseTrafficInput()
        {
            if (trafficMenuDirty)
            {
                return;
            }

            TrafficInput tO = WUIEngine.INPUT.Traffic;

            float.TryParse(stallSpeed, out tO.macroTrafficSimInput.stallSpeed);
            //float.TryParse(opticalDensity, out tO.opticalDensity);
        }        

        /*void OpenLoadOpticalDensityFile()
        {
            FileBrowser.SetFilters(false, opticalDensityFilter);
            string initialPath = Path.GetDirectoryName(WUIEngine.WORKING_FILE);
            FileBrowser.ShowLoadDialog(LoadOpticalDensityFile, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Load optical density ramp file", "Load");
        }

        void LoadOpticalDensityFile(string[] paths)
        {
            WUIEngine.RUNTIME_DATA.Traffic.LoadOpticalDensityFile(paths[0], true);
        }*/
    }
}
