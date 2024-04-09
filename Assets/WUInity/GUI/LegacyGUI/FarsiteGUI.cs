using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WUIEngine;

namespace WUInity.UI
{
    public partial class WUInityGUI
    {
        float sliderValue = 0f;
        //Farsite.FarsiteViewer fV;
        void FarsiteMenu()
        {
            /*FarsiteInput fI = WUIEngine.Input.Fire.farsiteData;

            GUI.Box(new Rect(120, 0, columnWidth + 40, Screen.height - consoleHeight), "");
            int buttonIndex = 0;

            //name
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Output file name:");
            ++buttonIndex;
            fI.outputPrefix = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), fI.outputPrefix);
            ++buttonIndex;

            if (GUI.Button(new Rect(140, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Import Farsite Data"))
            {
                WUInity.INSTANCE.LoadFarsite();
            }
            ++buttonIndex;

            if (WUInity.FARSITE_VIEWER != null)
            {
                if (fV == null)
                {
                    fV = WUInity.FARSITE_VIEWER;
                }
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Time: " + (int)fV.actualTime + " hours after ignition");
                ++buttonIndex;
                sliderValue = GUI.HorizontalSlider(new Rect(140, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), sliderValue, 0.0F, 1.0f);
                fV.SetTime(sliderValue);
                ++buttonIndex;

                //TODO: add condition if farsite data loaded
                if (GUI.Button(new Rect(140, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Show/Hide Farsite Data"))
                {
                    fV.ToggleTerrain();
                }
                ++buttonIndex;
            }*/
        }
    }
}
