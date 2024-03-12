using UnityEngine;
using SimpleFileBrowser;
using System.IO;

namespace WUInity.UI
{
    public partial class WUInityGUI
    {
        string fireEditMode;
        string lcpCurrentInfo;
        void FireMenu()
        {
            FireInput fI = WUInity.INPUT.Fire;

            GUI.Box(new Rect(120, 0, columnWidth + 40, Screen.height - consoleHeight), "");
            int buttonIndex = 0;

            string lcpExistsStatus = "LCP file NOT found"; 
            if(WUInity.DATA_STATUS.LcpLoaded)
            {
                lcpExistsStatus = "LCP file found";
            }
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), lcpExistsStatus);
            ++buttonIndex;

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Load LCP file"))
            {
                OpenLoadLCP();
            }
            ++buttonIndex;

            if(WUInity.RUNTIME_DATA.Fire.LCPData != null)
            {
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "LCP DATA");
                ++buttonIndex;
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Cells (x, y): " + WUInity.RUNTIME_DATA.Fire.LCPData.Header.numeast + ", " + WUInity.RUNTIME_DATA.Fire.LCPData.Header.numnorth);
                ++buttonIndex;
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Cell size (x, y): " + Mathf.RoundToInt((float)WUInity.RUNTIME_DATA.Fire.LCPData.RasterCellResolutionX) + ", " + Mathf.RoundToInt((float)WUInity.RUNTIME_DATA.Fire.LCPData.RasterCellResolutionY));
                ++buttonIndex;

                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Toggle LCP display"))
                {
                    WUInity.RUNTIME_DATA.Fire.ToggleLCPDataPlane();   
                }
                ++buttonIndex;


                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Fuel model"))
                {
                    WUInity.RUNTIME_DATA.Fire.Visualizer.SetLCPViewMode(Visualization.FireDataVisualizer.LcpViewMode.FuelModel);
                }
                ++buttonIndex;
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Elevation"))
                {
                    WUInity.RUNTIME_DATA.Fire.Visualizer.SetLCPViewMode(Visualization.FireDataVisualizer.LcpViewMode.Elevation);
                    lcpCurrentInfo = "Elevation range: " + WUInity.RUNTIME_DATA.Fire.LCPData.Header.loelev + "-" + WUInity.RUNTIME_DATA.Fire.LCPData.Header.hielev + " [m]";
                }
                ++buttonIndex;
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Slope"))
                {
                    WUInity.RUNTIME_DATA.Fire.Visualizer.SetLCPViewMode(Visualization.FireDataVisualizer.LcpViewMode.Slope);
                    lcpCurrentInfo = "Slope range: " + WUInity.RUNTIME_DATA.Fire.LCPData.Header.loslope + "-" + WUInity.RUNTIME_DATA.Fire.LCPData.Header.hislope + " [-]";
                }
                ++buttonIndex;
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Aspect"))
                {
                    WUInity.RUNTIME_DATA.Fire.Visualizer.SetLCPViewMode(Visualization.FireDataVisualizer.LcpViewMode.Aspect);
                    lcpCurrentInfo = "Aspect range: " + WUInity.RUNTIME_DATA.Fire.LCPData.Header.loaspect + "-" + WUInity.RUNTIME_DATA.Fire.LCPData.Header.hiaspect + " [°]";
                }
                ++buttonIndex;

                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), lcpCurrentInfo);
                ++buttonIndex;
            }            

            if (WUInity.DATA_STATUS.FuelModelsLoaded)
            {
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Custom fuel model set loaded");
                ++buttonIndex;
            }
            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Load fuel models"))
            {
                OpenLoadFuelsModelFile();
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

                WUInity.INPUT.Fire.useRandomIgnitionMap = GUI.Toggle(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), WUInity.INPUT.Fire.useRandomIgnitionMap, "Use random ignition");
                ++buttonIndex;
                if (WUInity.INPUT.Fire.useRandomIgnitionMap)
                {
                    WUInity.INPUT.Fire.useInitialIgnitionMap = false;
                    if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Edit random ignition area"))
                    {
                        fireEditMode = "Random ignition";
                        WUInity.INSTANCE.StartPainter(Painter.PaintMode.RandomIgnitionArea);
                    }
                    ++buttonIndex;
                }

                WUInity.INPUT.Fire.useInitialIgnitionMap = GUI.Toggle(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), WUInity.INPUT.Fire.useInitialIgnitionMap, "Use initial ignition");
                ++buttonIndex;
                if(WUInity.INPUT.Fire.useInitialIgnitionMap)
                {
                    WUInity.INPUT.Fire.useRandomIgnitionMap = false;
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
            WUInity.RUNTIME_DATA.Fire.LoadLCPFile(paths[0], true);
        }

        void OpenLoadFuelsModelFile()
        {
            FileBrowser.SetFilters(false, fuelModelsFilter);
            string initialPath = Path.GetDirectoryName(WUInity.WORKING_FILE);
            FileBrowser.ShowLoadDialog(LoadFuelModelsFile, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Load fuel models", "Load");
        }

        void LoadFuelModelsFile(string[] paths)
        {
            WUInity.RUNTIME_DATA.Fire.LoadFuelModelsInput(paths[0], true);
        }

        void ResetFireGUI()
        {
            WUInity.RUNTIME_DATA.Fire.SetLCPDataPlane(false);
        }
    }
}
