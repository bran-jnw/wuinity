using UnityEngine;
using SimpleFileBrowser;
using System.IO;
using WUIPlatform.IO;
using WUIPlatform;
using WUIPlatform.Visualization;
using WUIPlatform.WUInity;

namespace WUIPlatform.WUInity.UI
{
    public partial class WUInityGUI
    {
        string fireEditMode;
        string lcpCurrentInfo;
        void FireMenu()
        {
            FireInput fI = WUIEngine.INPUT.Fire;

            GUI.Box(new Rect(120, 0, columnWidth + 40, Screen.height - consoleHeight), "");
            int buttonIndex = 0;

            string lcpExistsStatus = "LCP file NOT found"; 
            if(WUIEngine.DATA_STATUS.LcpLoaded)
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

            if(WUIEngine.RUNTIME_DATA.Fire.LCPData != null)
            {
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "LCP DATA");
                ++buttonIndex;
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Cells (x, y): " + WUIEngine.RUNTIME_DATA.Fire.LCPData.GetCellCountX() + ", " + WUIEngine.RUNTIME_DATA.Fire.LCPData.GetCellCountY());
                ++buttonIndex;
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Cell size (x, y): " + UnityEngine.Mathf.RoundToInt((float)WUIEngine.RUNTIME_DATA.Fire.LCPData.RasterCellResolutionX) + ", " + UnityEngine.Mathf.RoundToInt((float)WUIEngine.RUNTIME_DATA.Fire.LCPData.RasterCellResolutionY));
                ++buttonIndex;

                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Toggle LCP display"))
                {
                    WUIEngine.RUNTIME_DATA.Fire.ToggleLCPDataPlane();   
                }
                ++buttonIndex;


                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Fuel model"))
                {
                    WUIEngine.RUNTIME_DATA.Fire.Visualizer.SetLCPViewMode(FireDataVisualizer.LcpViewMode.FuelModel);
                }
                ++buttonIndex;
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Elevation"))
                {
                    WUIEngine.RUNTIME_DATA.Fire.Visualizer.SetLCPViewMode(FireDataVisualizer.LcpViewMode.Elevation);
                    lcpCurrentInfo = "Elevation range: " + WUIEngine.RUNTIME_DATA.Fire.LCPData.GetElevationMin() + "-" + WUIEngine.RUNTIME_DATA.Fire.LCPData.GetElevationMax() + " [m]";
                }
                ++buttonIndex;
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Slope"))
                {
                    WUIEngine.RUNTIME_DATA.Fire.Visualizer.SetLCPViewMode(FireDataVisualizer.LcpViewMode.Slope);
                    lcpCurrentInfo = "Slope range: " + WUIEngine.RUNTIME_DATA.Fire.LCPData.GetSlopeMin() + "-" + WUIEngine.RUNTIME_DATA.Fire.LCPData.GetSlopeMax() + " [-]";
                }
                ++buttonIndex;
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Aspect"))
                {
                    WUIEngine.RUNTIME_DATA.Fire.Visualizer.SetLCPViewMode(FireDataVisualizer.LcpViewMode.Aspect);
                    lcpCurrentInfo = "Aspect range: " + WUIEngine.RUNTIME_DATA.Fire.LCPData.GetAspectMin() + "-" + WUIEngine.RUNTIME_DATA.Fire.LCPData.GetAspectMax() + " [°]";
                }
                ++buttonIndex;

                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), lcpCurrentInfo);
                ++buttonIndex;
            }            

            if (WUIEngine.DATA_STATUS.FuelModelsLoaded)
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
            if (!WUInityEngine.INSTANCE.IsPainterActive())
            {
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Edit WUI area"))
                {
                    fireEditMode = "WUI area";
                    WUInityEngine.INSTANCE.StartPainter(Painter.PaintMode.WUIArea);
                }
                ++buttonIndex;

                //WUIEngine.INPUT.Fire.FireCellInput.UseRandomIgnitionMap = GUI.Toggle(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), WUIEngine.INPUT.Fire.FireCellInput.UseRandomIgnitionMap, "Use random ignition");
                //++buttonIndex;
                //if (WUIEngine.INPUT.Fire.FireCellInput.UseRandomIgnitionMap)
                //{
                    //WUIEngine.INPUT.Fire.FireCellInput.UseInitialIgnitionMap = false;
                    if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Edit random ignition area"))
                    {
                        fireEditMode = "Random ignition";
                        WUInityEngine.INSTANCE.StartPainter(Painter.PaintMode.RandomIgnitionArea);
                    }
                    ++buttonIndex;
                //}

                //WUIEngine.INPUT.Fire.FireCellInput.UseInitialIgnitionMap = GUI.Toggle(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), WUIEngine.INPUT.Fire.FireCellInput.UseInitialIgnitionMap, "Use initial ignition");
                //++buttonIndex;
                //if(WUIEngine.INPUT.Fire.FireCellInput.UseInitialIgnitionMap)
                //{
                    //WUIEngine.INPUT.Fire.FireCellInput.UseRandomIgnitionMap = false;
                    if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Edit initial ignition"))
                    {
                        fireEditMode = "Initial ignition";
                        WUInityEngine.INSTANCE.StartPainter(Painter.PaintMode.InitialIgnition);
                    }
                    ++buttonIndex;
                //}      
            }
            else
            {
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), fireEditMode);
                ++buttonIndex;

                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Add cells"))
                {
                    WUInityEngine.Painter.SetWUIAreaColor(true);
                }
                ++buttonIndex;

                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Remove cells"))
                {
                    WUInityEngine.Painter.SetWUIAreaColor(false);
                }
                ++buttonIndex;

                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Finish editing"))
                {                    
                    WUInityEngine.INSTANCE.StopPainter();
                    GraphicalFireInput.SaveGraphicalFireInput();
                }
                ++buttonIndex;                
            }

            ++buttonIndex;
            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Farsite import"))
            {
                menuChoice = ActiveMenu.Farsite;
                WUInityEngine.INSTANCE.SetSampleMode(WUInityEngine.DataSampleMode.Farsite);
            }
            ++buttonIndex;
        }

        void OpenLoadLCP()
        {
            FileBrowser.SetFilters(false, lcpFilter);
            string initialPath = Path.GetDirectoryName(WUIEngine.WORKING_FILE);
            FileBrowser.ShowLoadDialog(LoadLCP, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Load LCP file", "Load");
        }

        void LoadLCP(string[] paths)
        {
            WUIEngine.RUNTIME_DATA.Fire.LoadLCPFile(paths[0], true);
        }

        void OpenLoadFuelsModelFile()
        {
            FileBrowser.SetFilters(false, fuelModelsFilter);
            string initialPath = Path.GetDirectoryName(WUIEngine.WORKING_FILE);
            FileBrowser.ShowLoadDialog(LoadFuelModelsFile, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Load fuel models", "Load");
        }

        void LoadFuelModelsFile(string[] paths)
        {
            WUIEngine.RUNTIME_DATA.Fire.LoadFuelModelsInput(paths[0], true);
        }

        void ResetFireGUI()
        {
            WUIEngine.RUNTIME_DATA.Fire.SetLCPDataPlane(false);
        }
    }
}
