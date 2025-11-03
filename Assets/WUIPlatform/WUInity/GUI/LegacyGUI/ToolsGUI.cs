using UnityEngine;
using SimpleFileBrowser;
using System.IO;
using PREACT.IO;
using PREACT.Tools;

namespace WUInity.UI
{
    public partial class WUInityGUI
    {
        private bool populationMenuDirty = true;
        private bool _reScaling = false, _filteringOSM = false, _creatingPopulationMap = false;
        private string _desiredPopulation, _xBorder, _yBorder, _populationMapCellSize;

        private bool HaveLocalGPW { get => _engine.ScenarioData.Population.LocalGPWData.HavedData; }
        private bool HavePopulationMap { get => _engine.ScenarioData.Population.PopulationMap.HaveData; }
        private bool PopulationMapCorrectedForRoadAccess { get => _engine.ScenarioData.Population.PopulationMap.CorrectedForRoadAccess; }
        private bool HaveRouterDb { get => _engine.ScenarioData.Routing.RouterDb == null ? false : true; }

        void ToolsMenu()
        {
            PopulationInput popIn = _engine.Input.Population;
            if (populationMenuDirty)
            {
                populationMenuDirty = false;
            }
            GUI.Box(new Rect(120, 0, columnWidth + 40, Screen.height - consoleHeight), "");
            int buttonIndex = 0;
            int buttonColumnStart = 140;

            /*string localPopStatus = "Population data NOT loaded";
            if (WUI_engine.POPULATION.IsPopulationLoaded())
            {
                localPopStatus = "Population data loaded";
            }
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), localPopStatus);
            ++buttonIndex;*/

            //GPW stuff
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "GPW tools");
            ++buttonIndex;
            if(_engine.ScenarioData.Population.LocalGPWData.HavedData)
            {
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Population:" + _engine.ScenarioData.Population.LocalGPWData.totalPopulation);
                ++buttonIndex;
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Show/hide GPW data"))
                {
                    _wuinityManager.SetSampleMode(DataSampleMode.LocalGPW);
                    _engine.ScenarioData.Population.Visualizer.ToggleLocalGPWVisibility();
                }
                ++buttonIndex;
            }
            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Create local GPW data"))
            {
                OpenCreateAndSaveLocalGPW();
            }
            ++buttonIndex;
            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Load local GPW data"))
            {
                OpenLoadLocalGPW();
            }
            ++buttonIndex;

            //Router Db stuff
            ++buttonIndex;
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "RouterDb tools");
            ++buttonIndex;
            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Create routerDb"))
            {
                OpenCreateAndSaveRouterDb();
            }
            ++buttonIndex;
            /*if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Load routerDb"))
            {
                OpenLoadRouterDb();
            }
            ++buttonIndex;  */       

            //Population map stuff
            ++buttonIndex;
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Population map tools");
            ++buttonIndex;
            if (HavePopulationMap)
            {
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Population:" + _engine.ScenarioData.Population.PopulationMap._totalPopulation);
                ++buttonIndex;
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Show/hide population map"))
                {
                    _wuinityManager.SetSampleMode(DataSampleMode.PopulationMap);
                    _wuinityManager.DisplayPopulationMap();
                    _wuinityManager.ToggleDomainDataPlane();

                }
                ++buttonIndex;
            }
            if(!_creatingPopulationMap)
            {
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Create population map"))
                {
                    _creatingPopulationMap = true;
                }
                ++buttonIndex;
            }
            else
            {
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Cell size [m]:");
                ++buttonIndex;
                _populationMapCellSize = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight),_populationMapCellSize);
                ++buttonIndex;
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Select local GPW file"))
                {
                    OpenCreateAndSavePopulationMap();
                    _creatingPopulationMap = false;
                }
                ++buttonIndex;
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Cancel"))
                {
                    _creatingPopulationMap = false;
                }
                ++buttonIndex;
                ++buttonIndex;
            }
            
            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Load population map"))
            {
                OpenLoadPopulationMap();
            }
            ++buttonIndex;
            if (HavePopulationMap)
            {               
                //re-scaling
                if (!_reScaling)
                {
                    if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Re-scale total population"))
                    {
                        _reScaling = true;
                    }
                    ++buttonIndex;
                }
                else
                {
                    GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Desired population:");
                    ++buttonIndex;
                    _desiredPopulation = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), _desiredPopulation);
                    ++buttonIndex;
                    if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Apply re-scale"))
                    {
                        PREACT.Tools.PopulationTools.ScaleTotalPopulation(_engine, _desiredPopulation);
                        _wuinityManager.DisplayPopulationMap();
                        _reScaling = false;
                    }                    
                    ++buttonIndex;
                    if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Cancel"))
                    {
                        _reScaling = false;
                    }
                    ++buttonIndex;
                    ++buttonIndex;
                }
                //correct for road access
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Correct for network access"))
                {
                    OpenRoadAccessCorrectPopulationMap();
                }
                ++buttonIndex;                
            }

            //paint population mask
            if(HavePopulationMap)
            {
                if (!_wuinityManager.IsPainterActive())
                {
                    if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Create population mask"))
                    {
                        _wuinityManager.StartPainter(Painter.PaintMode.PopulationMask);
                    }
                    ++buttonIndex;
                }
                else
                {
                    GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Population mask edit");
                    ++buttonIndex;
                    if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Add area"))
                    {
                        _wuinityManager.Painter.SetMaskGPWColor(true);
                    }
                    ++buttonIndex;
                    if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Remove area"))
                    {
                        _wuinityManager.Painter.SetMaskGPWColor(false);
                    }
                    ++buttonIndex;

                    if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Close & save"))
                    {
                        
                        _wuinityManager.StopPainter();
                    }
                    ++buttonIndex;
                    ++buttonIndex;
                }

                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Apply population mask"))
                {
                    OpenApplyMaskOnPopulationMap();
                }
                ++buttonIndex;
            }            

            //Population stuff
            ++buttonIndex;
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Population tools");
            ++buttonIndex;
            if(HavePopulationMap && PopulationMapCorrectedForRoadAccess)
            {
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Create population"))
                {
                    OpenCreatePopulation();
                }
                ++buttonIndex;
            }
            else
            {
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "[Need data]");
                ++buttonIndex;
            }

            //OSM stuff
            ++buttonIndex;
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "OSM tools");
            ++buttonIndex;
            if(!_filteringOSM)
            {
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Filter OSM data"))
                {
                    _filteringOSM = true;
                }
            }
            else
            {
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Border x/y");
                ++buttonIndex;
                _xBorder = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth * 0.45f, buttonHeight), _xBorder);
                _yBorder = GUI.TextField(new Rect(buttonColumnStart + columnWidth * 0.55f, buttonIndex * (buttonHeight + 5) + 10, columnWidth * 0.45f, buttonHeight), _yBorder);
                ++buttonIndex;
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Select OSM data"))
                {
                    OpenFilterOSM();
                    _filteringOSM = false;
                }                
                ++buttonIndex;
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Cancel"))
                {
                    _filteringOSM = false;
                }
                ++buttonIndex;
                ++buttonIndex;
            }
            ++buttonIndex;

            //trigger buffer
            ++buttonIndex;
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Trigger buffer tools");
            ++buttonIndex;
            if(_engine.Input.TriggerBuffer.kPERILInput != null)
            {
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Run k-PERIL"))
                {
                    float[,] tB = PREACT.WUIPlatformPERIL.RunPERIL(_engine.Input.TriggerBuffer.kPERILInput.MidflameWindspeed);
                    _engine.Simulation.SetTriggerBufferData(tB);
                    _engine.Simulation.DisplayTriggerBuffer();
                }
            }   
        }        

        //GPW
        void OpenCreateAndSaveLocalGPW()
        {
            string initialPath = Path.GetDirectoryName(_engine.WorkingFolder);
            FileBrowser.ShowLoadDialog(CreateAndSaveLocalGPW, CancelSaveLoad, FileBrowser.PickMode.Folders, false, initialPath, null, "Select global GPW folder", "Set");
        }
        void CreateAndSaveLocalGPW(string[] paths)
        {
            PopulationTools.CreateAndSaveLocalGPWData(_engine, paths[0]);
            _engine.ScenarioData.Population.Visualizer.SetDataPlane(true);
        }
        void OpenLoadLocalGPW()
        {
            FileBrowser.SetFilters(false, gpwFilter);
            string initialPath = Path.GetDirectoryName(_engine.WorkingFolder);
            FileBrowser.ShowLoadDialog(LoadLocalGPW, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Select local GPW data", "Set");
        }
        void LoadLocalGPW(string[] paths)
        {
            PopulationTools.LoadLocalGPWData(_engine, paths[0]);
            _engine.ScenarioData.Population.Visualizer.SetDataPlane(true);
        }

        //Interpolated GPW
        void OpenCreateAndSavePopulationMap()
        {
            FileBrowser.SetFilters(false, gpwFilter);
            string initialPath = Path.GetDirectoryName(_engine.WorkingFolder);
            FileBrowser.ShowLoadDialog(CreateAndSavePopulationMap, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Select local GPW data", "Set");
        }        
        void CreateAndSavePopulationMap(string[] paths)
        {
            PopulationTools.CreateAndSavePopulationMap(_engine, paths[0], _populationMapCellSize);
            _wuinityManager.DisplayPopulationMap();
            _wuinityManager.SetDomainDataPlane(true);           
        }
        void OpenLoadPopulationMap()
        {
            FileBrowser.SetFilters(false, populationMapFilter);
            string initialPath = Path.GetDirectoryName(_engine.WorkingFolder);
            FileBrowser.ShowLoadDialog(LoadPopulationMap, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Select population map", "Set");
        }
        void LoadPopulationMap(string[] paths)
        {
            PopulationTools.LoadPopulationMap(_engine, paths[0]);
            _wuinityManager.DisplayPopulationMap();
            _wuinityManager.SetDomainDataPlane(true);
        }

        //Filtering of OSM        
        void OpenFilterOSM()
        {
            FileBrowser.SetFilters(false, osmFilter);
            string initialPath = Path.GetDirectoryName(_engine.WorkingFolder);
            FileBrowser.ShowLoadDialog(FilterOSM, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Select OSM data to filter spacially", "Set");
        }
        void FilterOSM(string[] paths)
        {
            PopulationTools.FilterOsmData(_engine, paths[0], _xBorder, _yBorder);            
        }

        //Router Db
        void OpenCreateAndSaveRouterDb()
        {
            FileBrowser.SetFilters(false, osmFilter);
            string initialPath = Path.GetDirectoryName(_engine.WorkingFolder);
            FileBrowser.ShowLoadDialog(CreateAndSaveRouterDb, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Select OSM file to build routerDb from", "Set");
        }
        void CreateAndSaveRouterDb(string[] paths)
        {
            PopulationTools.CreateAndSaveRouterDb(_engine, paths[0]);
        }
        /*void OpenLoadRouterDb()
        {
            FileBrowser.SetFilters(false, routerDbFilter);
            string initialPath = Path.GetDirectoryName(WUI_engine.WORKING_FOLDER);
            FileBrowser.ShowLoadDialog(LoadRouterDb, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Select local GPW data", "Set");
        }
        void LoadRouterDb(string[] paths)
        {
            LoadRouterDb(paths[0]);
        }*/

        //filter population map
        void OpenRoadAccessCorrectPopulationMap()
        {
            FileBrowser.SetFilters(false, routerDbFilter);
            string initialPath = Path.GetDirectoryName(_engine.WorkingFolder);
            FileBrowser.ShowLoadDialog(RoadAccessCorrectPopulationMap, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Select routerDb to use for road access correction", "Set");
        }
        void RoadAccessCorrectPopulationMap(string[] paths)
        {
            PopulationTools.RoadAccessCorrectPopulationMap(_engine, paths[0]);
        }

        void OpenApplyMaskOnPopulationMap()
        {
            FileBrowser.SetFilters(false, maskFilter);
            string initialPath = Path.GetDirectoryName(_engine.WorkingFolder);
            FileBrowser.ShowLoadDialog(ApplyPopulationMapMask, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Select population map mask to apply", "Set");
        }
        void ApplyPopulationMapMask(string[] paths)
        {
            PopulationTools.ApplyPopulationMapMask(_engine, paths[0]);
        }

        //create population
        /*void OpenCreatePopulation()
        {
            FileBrowser.SetFilters(false, populationMapFilter);
            string initialPath = Path.GetDirectoryName(WUI_engine.WORKING_FOLDER);
            FileBrowser.ShowLoadDialog(CreatePopulation, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Select routerDb to use for road access correction", "Set");
        }*/
        private void OpenCreatePopulation() //string[] paths
        {
            FileBrowser.SetFilters(false, csvFilter);
            string initialPath = Path.GetDirectoryName(_engine.WorkingFolder);
            FileBrowser.ShowLoadDialog(CreatePopulation, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Specify population output file", "Create");
        }
        private void CreatePopulation(string[] paths) //string[] paths
        {
            PopulationTools.CreatePopulation(_engine, paths[0]);
        }

        private void OpenSavePopulationMask() //string[] paths
        {
            FileBrowser.SetFilters(false, maskFilter);
            string initialPath = Path.GetDirectoryName(_engine.WorkingFolder);
            FileBrowser.ShowLoadDialog(SavePopulationMask, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Specify population output file", "Create");
        }
        private void SavePopulationMask(string[] paths) //string[] paths
        {
            PopulationTools.SavePopulationMask(_engine, paths[0]);
        }
        
    }
}