using UnityEngine;
using SimpleFileBrowser;
using System.IO;

namespace WUInity
{
    public partial class WUInityGUI
    {
        string fireEditMode;
        void FireMenu()
        {
            FireInput fI = WUInity.INPUT.fire;

            GUI.Box(new Rect(120, 0, columnWidth + 40, Screen.height - consoleHeight), "");
            int buttonIndex = 0;

            string lcpExistsStatus = "LCP file NOT found"; 
            if(WUInity.DATA_STATUS.lcpLoaded)
            {
                lcpExistsStatus = "LCP file found";
            }
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), lcpExistsStatus);
            ++buttonIndex;

            //name
            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Load LCP file"))
            {
                OpenLoadLCP();
            }
            ++buttonIndex;
            ++buttonIndex;

            //edit maps
            if (!WUInity.INSTANCE.IsPainterActive())
            {
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Edit WUI area"))
                {
                    fireEditMode = "WUI area";
                    WUInity.INSTANCE.StartPainter(Painter.PaintMode.WUIArea);
                }
                ++buttonIndex;

                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Edit trigger buffer"))
                {
                    fireEditMode = "Trigger buffer";
                    WUInity.INSTANCE.StartPainter(Painter.PaintMode.TriggerBuffer);
                }
                ++buttonIndex;

                WUInity.INPUT.fire.useRandomIgnitionMap = GUI.Toggle(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), WUInity.INPUT.fire.useRandomIgnitionMap, "Use random ignition");
                ++buttonIndex;
                if (WUInity.INPUT.fire.useRandomIgnitionMap)
                {
                    WUInity.INPUT.fire.useInitialIgnitionMap = false;
                    if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Edit random ignition area"))
                    {
                        fireEditMode = "Random ignition";
                        WUInity.INSTANCE.StartPainter(Painter.PaintMode.RandomIgnitionArea);
                    }
                    ++buttonIndex;
                }

                WUInity.INPUT.fire.useInitialIgnitionMap = GUI.Toggle(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), WUInity.INPUT.fire.useInitialIgnitionMap, "Use initial ignition");
                ++buttonIndex;
                if(WUInity.INPUT.fire.useInitialIgnitionMap)
                {
                    WUInity.INPUT.fire.useRandomIgnitionMap = false;
                    if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Edit initial ignition"))
                    {
                        fireEditMode = "Initial ignition";
                        WUInity.INSTANCE.StartPainter(Painter.PaintMode.InitialIgnition);
                    }
                    ++buttonIndex;
                }      
            }
            else
            {
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), fireEditMode);
                ++buttonIndex;

                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Add cells"))
                {
                    WUInity.PAINTER.SetWUIAreaColor(true);
                }
                ++buttonIndex;

                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Remove cells"))
                {
                    WUInity.PAINTER.SetWUIAreaColor(false);
                }
                ++buttonIndex;

                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Stop editing"))
                {                    
                    WUInity.INSTANCE.StopPainter();
                    GraphicalFireInput.SaveGraphicalFireInput();
                }
                ++buttonIndex;                
            }

            ++buttonIndex;
            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Farsite import"))
            {
                menuChoice = ActiveMenu.Farsite;
                WUInity.INSTANCE.SetSampleMode(WUInity.DataSampleMode.Farsite);
            }
            ++buttonIndex;
        }

        void OpenLoadLCP()
        {
            FileBrowser.SetFilters(false, lcpFilter);
            string initialPath = Path.GetDirectoryName(WUInity.WORKING_FILE);
            FileBrowser.ShowLoadDialog(LoadLCP, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Load LCP file", "Load");
        }

        void LoadLCP(string[] paths)
        {
            WUInityInput wO = WUInity.INPUT;
            wO.fire.lcpFile = paths[0];
            WUInity.INSTANCE.UpdateFireResourceStatus();
        }
    }
}
