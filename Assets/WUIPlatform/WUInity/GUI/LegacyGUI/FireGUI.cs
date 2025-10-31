using UnityEngine;
using SimpleFileBrowser;
using System.IO;
using PREACT.IO;
using PREACT;
using PREACT.Visualization;

namespace WUInity.UI
{
    public partial class WUInityGUI
    {
        string fireEditMode;
        string lcpCurrentInfo;
        void FireMenu()
        {
            FireInput fI = _engine.Input.Fire;

            GUI.Box(new Rect(120, 0, columnWidth + 40, Screen.height - consoleHeight), "");
            int buttonIndex = 0;

            string lcpExistsStatus = "LCP file NOT found"; 
            if(_engine.DataStatus.LcpLoaded)
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

            if(_engine.RuntimeData.Fire.LCPData != null)
            {
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "LCP DATA");
                ++buttonIndex;
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Cells (x, y): " + _engine.RuntimeData.Fire.LCPData.GetCellCountX() + ", " + _engine.RuntimeData.Fire.LCPData.GetCellCountY());
                ++buttonIndex;
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Cell size (x, y): " + UnityEngine.Mathf.RoundToInt((float)_engine.RuntimeData.Fire.LCPData.RasterCellResolutionX) + ", " + Unity_engine.Mathf.RoundToInt((float)_engine.RuntimeData.Fire.LCPData.RasterCellResolutionY));
                ++buttonIndex;

                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Toggle LCP display"))
                {
                    _engine.RuntimeData.Fire.ToggleLCPDataPlane();   
                }
                ++buttonIndex;


                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Fuel model"))
                {
                    _engine.RuntimeData.Fire.Visualizer.SetLCPViewMode(FireDataVisualizer.LcpViewMode.FuelModel);
                }
                ++buttonIndex;
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Elevation"))
                {
                    _engine.RuntimeData.Fire.Visualizer.SetLCPViewMode(FireDataVisualizer.LcpViewMode.Elevation);
                    lcpCurrentInfo = "Elevation range: " + _engine.RuntimeData.Fire.LCPData.GetElevationMin() + "-" + _engine.RuntimeData.Fire.LCPData.GetElevationMax() + " [m]";
                }
                ++buttonIndex;
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Slope"))
                {
                    _engine.RuntimeData.Fire.Visualizer.SetLCPViewMode(FireDataVisualizer.LcpViewMode.Slope);
                    lcpCurrentInfo = "Slope range: " + _engine.RuntimeData.Fire.LCPData.GetSlopeMin() + "-" + _engine.RuntimeData.Fire.LCPData.GetSlopeMax() + " [-]";
                }
                ++buttonIndex;
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Aspect"))
                {
                    _engine.RuntimeData.Fire.Visualizer.SetLCPViewMode(FireDataVisualizer.LcpViewMode.Aspect);
                    lcpCurrentInfo = "Aspect range: " + _engine.RuntimeData.Fire.LCPData.GetAspectMin() + "-" + _engine.RuntimeData.Fire.LCPData.GetAspectMax() + " [°]";
                }
                ++buttonIndex;

                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), lcpCurrentInfo);
                ++buttonIndex;
            }            

            if (_engine.DataStatus.FuelModelsLoaded)
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
            if (!WUInityManager.INSTANCE.IsPainterActive())
            {
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Edit WUI area"))
                {
                    fireEditMode = "WUI area";
                    WUInityManager.INSTANCE.StartPainter(Painter.PaintMode.WUIArea);
                }
                ++buttonIndex;

                //WUI_engine.INPUT.Fire.FireCellInput.UseRandomIgnitionMap = GUI.Toggle(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), WUI_engine.INPUT.Fire.FireCellInput.UseRandomIgnitionMap, "Use random ignition");
                //++buttonIndex;
                //if (WUI_engine.INPUT.Fire.FireCellInput.UseRandomIgnitionMap)
                //{
                    //WUI_engine.INPUT.Fire.FireCellInput.UseInitialIgnitionMap = false;
                    if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Edit random ignition area"))
                    {
                        fireEditMode = "Random ignition";
                        WUInityManager.INSTANCE.StartPainter(Painter.PaintMode.RandomIgnitionArea);
                    }
                    ++buttonIndex;
                //}

                //WUI_engine.INPUT.Fire.FireCellInput.UseInitialIgnitionMap = GUI.Toggle(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), WUI_engine.INPUT.Fire.FireCellInput.UseInitialIgnitionMap, "Use initial ignition");
                //++buttonIndex;
                //if(WUI_engine.INPUT.Fire.FireCellInput.UseInitialIgnitionMap)
                //{
                    //WUI_engine.INPUT.Fire.FireCellInput.UseRandomIgnitionMap = false;
                    if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Edit initial ignition"))
                    {
                        fireEditMode = "Initial ignition";
                        WUInityManager.INSTANCE.StartPainter(Painter.PaintMode.InitialIgnition);
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
                    WUInityManager.Painter.SetWUIAreaColor(true);
                }
                ++buttonIndex;

                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Remove cells"))
                {
                    WUInityManager.Painter.SetWUIAreaColor(false);
                }
                ++buttonIndex;

                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Finish editing"))
                {                    
                    WUInityManager.INSTANCE.StopPainter();
                    GraphicalFireInput.SaveGraphicalFireInput();
                }
                ++buttonIndex;                
            }

            ++buttonIndex;
            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Farsite import"))
            {
                menuChoice = ActiveMenu.Farsite;
                WUInityManager.INSTANCE.SetSampleMode(WUInityManager.DataSampleMode.Farsite);
            }
            ++buttonIndex;
        }

        void OpenLoadLCP()
        {
            FileBrowser.SetFilters(false, lcpFilter);
            string initialPath = Path.GetDirectoryName(_engine.WorkingFile);
            FileBrowser.ShowLoadDialog(LoadLCP, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Load LCP file", "Load");
        }

        void LoadLCP(string[] paths)
        {
            _engine.RuntimeData.Fire.LoadLCPFile(paths[0], true);
        }

        void OpenLoadFuelsModelFile()
        {
            FileBrowser.SetFilters(false, fuelModelsFilter);
            string initialPath = Path.GetDirectoryName(_engine.WorkingFile);
            FileBrowser.ShowLoadDialog(LoadFuelModelsFile, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Load fuel models", "Load");
        }

        void LoadFuelModelsFile(string[] paths)
        {
            _engine.RuntimeData.Fire.LoadFuelModelsInput(paths[0], true);
        }

        void ResetFireGUI()
        {
            _engine.RuntimeData.Fire.SetLCPDataPlane(false);
        }
    }
}
