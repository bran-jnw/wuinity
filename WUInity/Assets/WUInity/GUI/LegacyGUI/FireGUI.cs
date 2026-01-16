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
            if(_input == null)
            {
                return;
            }

            WildfireModuleInput fI = _input.WildfireModule;

            GUI.Box(new Rect(120, 0, columnWidth + 40, Screen.height - consoleHeight), "");
            int buttonIndex = 0;

            string lcpExistsStatus = "LCP file NOT found"; 
            if(_input.WildfireModule.Data.LCPData != null)
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

            if(_input.WildfireModule.Data.LCPData != null)
            {
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "LCP DATA");
                ++buttonIndex;
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Cells (x, y): " + _input.WildfireModule.Data.LCPData.GetCellCountX() + ", " + _input.WildfireModule.Data.LCPData.GetCellCountY());
                ++buttonIndex;
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Cell size (x, y): " + UnityEngine.Mathf.RoundToInt((float)_input.WildfireModule.Data.LCPData.RasterCellResolutionX) + ", " + Mathf.RoundToInt((float)_input.WildfireModule.Data.LCPData.RasterCellResolutionY));
                ++buttonIndex;

                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Toggle LCP display"))
                {
                    _wuinityManager.FireDomainVisualizer.ToggleVisibility();
                }
                ++buttonIndex;


                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Fuel model"))
                {
                    _wuinityManager.FireDomainVisualizer.SetLCPViewMode(FireDomainVisualizer.LcpViewMode.FuelModel);
                }
                ++buttonIndex;
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Elevation"))
                {
                    _wuinityManager.FireDomainVisualizer.SetLCPViewMode(FireDomainVisualizer.LcpViewMode.Elevation);
                    lcpCurrentInfo = "Elevation range: " + _input.WildfireModule.Data.LCPData.GetElevationMin() + "-" + _input.WildfireModule.Data.LCPData.GetElevationMax() + " [m]";
                }
                ++buttonIndex;
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Slope"))
                {
                    _wuinityManager.FireDomainVisualizer.SetLCPViewMode(FireDomainVisualizer.LcpViewMode.Slope);
                    lcpCurrentInfo = "Slope range: " + _input.WildfireModule.Data.LCPData.GetSlopeMin() + "-" + _input.WildfireModule.Data.LCPData.GetSlopeMax() + " [-]";
                }
                ++buttonIndex;
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Aspect"))
                {
                    _wuinityManager.FireDomainVisualizer.SetLCPViewMode(FireDomainVisualizer.LcpViewMode.Aspect);
                    lcpCurrentInfo = "Aspect range: " + _input.WildfireModule.Data.LCPData.GetAspectMin() + "-" + _input.WildfireModule.Data.LCPData.GetAspectMax() + " [°]";
                }
                ++buttonIndex;

                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), lcpCurrentInfo);
                ++buttonIndex;
            }            

            if (_wuinityManager.Engine.DataStatus.FuelModelsLoaded)
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
            if (!_wuinityManager.IsPainterActive())
            {
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Edit WUI area"))
                {
                    fireEditMode = "WUI area";
                    _wuinityManager.StartPainter(Painter.PaintMode.WUIArea);
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
                        _wuinityManager.StartPainter(Painter.PaintMode.RandomIgnitionArea);
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
                        _wuinityManager.StartPainter(Painter.PaintMode.InitialIgnition);
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
                    _wuinityManager.Painter.SetWUIAreaColor(true);
                }
                ++buttonIndex;

                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Remove cells"))
                {
                    _wuinityManager.Painter.SetWUIAreaColor(false);
                }
                ++buttonIndex;

                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Finish editing"))
                {
                    FinishGraphicalFireInputEdit();
                }
                ++buttonIndex;                
            }

            /*++buttonIndex;
            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Farsite import"))
            {
                menuChoice = ActiveMenu.Farsite;
                _wuinityManager.SetSampleMode(DataSampleMode.Farsite);
            }
            ++buttonIndex;*/
        }

        void OpenLoadLCP()
        {
            FileBrowser.SetFilters(false, lcpFilter);
            string initialPath = Path.GetDirectoryName(_engine.WorkingFile);
            FileBrowser.ShowLoadDialog(LoadLCP, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Load LCP file", "Load");
        }

        void LoadLCP(string[] paths)
        {
            _input.WildfireModule.Data.LoadLCPFile(_input.WildfireModule, paths[0], _input.Simulation.Data.UTMOrigin, true, out success);
            _wuinityManager.FireDomainVisualizer.SetAndDisplayLCP(_input.WildfireModule.Data.LCPData);
        }

        void OpenLoadFuelsModelFile()
        {
            FileBrowser.SetFilters(false, fuelModelsFilter);
            string initialPath = Path.GetDirectoryName(_engine.WorkingFile);
            FileBrowser.ShowLoadDialog(LoadFuelModelsFile, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Load fuel models", "Load");
        }

        void LoadFuelModelsFile(string[] paths)
        {
            _input.WildfireModule.Data.LoadFuelModelsInput(_input.WildfireModule, paths[0], true, out success);
        }

        void FinishGraphicalFireInputEdit()
        {
            _wuinityManager.StopPainter();
            string filePath = Path.Combine(_engine.WorkingFolder, _input.WildfireModule.GraphicalFireInputFile);
            GraphicalFireInput.SaveGraphicalFireInput(filePath, _input.WildfireModule.Data);
        }

        void ResetFireGUI()
        {
            _wuinityManager.FireDomainVisualizer.SetVisibility(false);
        }
    }
}
