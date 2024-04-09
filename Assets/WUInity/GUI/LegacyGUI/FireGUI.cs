using UnityEngine;
using SimpleFileBrowser;
using System.IO;
using WUIEngine.IO;
using WUIEngine;
using WUIEngine.Visualization;

namespace WUInity.UI
{
    public partial class WUInityGUI
    {
        string fireEditMode;
        string lcpCurrentInfo;
        void FireMenu()
        {
            FireInput fI = Engine.INPUT.Fire;

            GUI.Box(new Rect(120, 0, columnWidth + 40, Screen.height - consoleHeight), "");
            int buttonIndex = 0;

            string lcpExistsStatus = "LCP file NOT found"; 
            if(Engine.DATA_STATUS.LcpLoaded)
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

            if(Engine.RUNTIME_DATA.Fire.LCPData != null)
            {
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "LCP DATA");
                ++buttonIndex;
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Cells (x, y): " + Engine.RUNTIME_DATA.Fire.LCPData.Header.numeast + ", " + Engine.RUNTIME_DATA.Fire.LCPData.Header.numnorth);
                ++buttonIndex;
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Cell size (x, y): " + UnityEngine.Mathf.RoundToInt((float)Engine.RUNTIME_DATA.Fire.LCPData.RasterCellResolutionX) + ", " + UnityEngine.Mathf.RoundToInt((float)Engine.RUNTIME_DATA.Fire.LCPData.RasterCellResolutionY));
                ++buttonIndex;

                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Toggle LCP display"))
                {
                    Engine.RUNTIME_DATA.Fire.ToggleLCPDataPlane();   
                }
                ++buttonIndex;


                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Fuel model"))
                {
                    Engine.RUNTIME_DATA.Fire.Visualizer.SetLCPViewMode(FireDataVisualizer.LcpViewMode.FuelModel);
                }
                ++buttonIndex;
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Elevation"))
                {
                    Engine.RUNTIME_DATA.Fire.Visualizer.SetLCPViewMode(FireDataVisualizer.LcpViewMode.Elevation);
                    lcpCurrentInfo = "Elevation range: " + Engine.RUNTIME_DATA.Fire.LCPData.Header.loelev + "-" + Engine.RUNTIME_DATA.Fire.LCPData.Header.hielev + " [m]";
                }
                ++buttonIndex;
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Slope"))
                {
                    Engine.RUNTIME_DATA.Fire.Visualizer.SetLCPViewMode(FireDataVisualizer.LcpViewMode.Slope);
                    lcpCurrentInfo = "Slope range: " + Engine.RUNTIME_DATA.Fire.LCPData.Header.loslope + "-" + Engine.RUNTIME_DATA.Fire.LCPData.Header.hislope + " [-]";
                }
                ++buttonIndex;
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Aspect"))
                {
                    Engine.RUNTIME_DATA.Fire.Visualizer.SetLCPViewMode(FireDataVisualizer.LcpViewMode.Aspect);
                    lcpCurrentInfo = "Aspect range: " + Engine.RUNTIME_DATA.Fire.LCPData.Header.loaspect + "-" + Engine.RUNTIME_DATA.Fire.LCPData.Header.hiaspect + " [°]";
                }
                ++buttonIndex;

                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), lcpCurrentInfo);
                ++buttonIndex;
            }            

            if (Engine.DATA_STATUS.FuelModelsLoaded)
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

                Engine.INPUT.Fire.useRandomIgnitionMap = GUI.Toggle(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), Engine.INPUT.Fire.useRandomIgnitionMap, "Use random ignition");
                ++buttonIndex;
                if (Engine.INPUT.Fire.useRandomIgnitionMap)
                {
                    Engine.INPUT.Fire.useInitialIgnitionMap = false;
                    if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Edit random ignition area"))
                    {
                        fireEditMode = "Random ignition";
                        WUInity.INSTANCE.StartPainter(Painter.PaintMode.RandomIgnitionArea);
                    }
                    ++buttonIndex;
                }

                Engine.INPUT.Fire.useInitialIgnitionMap = GUI.Toggle(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), Engine.INPUT.Fire.useInitialIgnitionMap, "Use initial ignition");
                ++buttonIndex;
                if(Engine.INPUT.Fire.useInitialIgnitionMap)
                {
                    Engine.INPUT.Fire.useRandomIgnitionMap = false;
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
                    WUInity.Painter.SetWUIAreaColor(true);
                }
                ++buttonIndex;

                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Remove cells"))
                {
                    WUInity.Painter.SetWUIAreaColor(false);
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
            string initialPath = Path.GetDirectoryName(Engine.WORKING_FILE);
            FileBrowser.ShowLoadDialog(LoadLCP, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Load LCP file", "Load");
        }

        void LoadLCP(string[] paths)
        {
            Engine.RUNTIME_DATA.Fire.LoadLCPFile(paths[0], true);
        }

        void OpenLoadFuelsModelFile()
        {
            FileBrowser.SetFilters(false, fuelModelsFilter);
            string initialPath = Path.GetDirectoryName(Engine.WORKING_FILE);
            FileBrowser.ShowLoadDialog(LoadFuelModelsFile, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Load fuel models", "Load");
        }

        void LoadFuelModelsFile(string[] paths)
        {
            Engine.RUNTIME_DATA.Fire.LoadFuelModelsInput(paths[0], true);
        }

        void ResetFireGUI()
        {
            Engine.RUNTIME_DATA.Fire.SetLCPDataPlane(false);
        }
    }
}
