using UnityEngine;
using SimpleFileBrowser;
using System.IO;
using PREACT.IO;
using PREACT.Tools;
using PREACT.Population;
using PREACT.Math;

namespace WUInity.UI
{
    public partial class WUInityGUI
    {
        private bool populationMenuDirty = true;
        private bool _reScaling = false, _filteringOSM = false, _creatingPopulationMap = false;
        private string _desiredPopulation, _xBorder, _yBorder, _populationMapCellSize, _minHouseholdSize, _maxHouseholdSize, _latitude, _longitude, _domainSizeX, _domainSizeY;
        bool success;

        bool ParseVector2d(string x, string y, out Vector2d v)
        {
            double a, b;
            if(double.TryParse(x, out a) && double.TryParse(y, out b))
            {
                v = new Vector2d(a, b);
                return true;
            }
            else
            {
                v = Vector2d.zero;
                return false;
            }            
        }

        void ToolsMenu()
        {
            PopulationInput popIn = _input.Population;
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

            if (_input == null)
            {
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Lower left lat/lon");
                ++buttonIndex;
                _latitude = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth * 0.45f, buttonHeight), _latitude);
                _longitude = GUI.TextField(new Rect(buttonColumnStart + columnWidth * 0.55f, buttonIndex * (buttonHeight + 5) + 10, columnWidth * 0.45f, buttonHeight), _longitude);
                ++buttonIndex;

                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Domain size x/y");
                ++buttonIndex;
                _domainSizeX = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth * 0.45f, buttonHeight), _domainSizeX);
                _domainSizeY = GUI.TextField(new Rect(buttonColumnStart + columnWidth * 0.55f, buttonIndex * (buttonHeight + 5) + 10, columnWidth * 0.45f, buttonHeight), _domainSizeY);
                ++buttonIndex;

                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Border x/y");
                ++buttonIndex;
                _xBorder = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth * 0.45f, buttonHeight), _xBorder);
                _yBorder = GUI.TextField(new Rect(buttonColumnStart + columnWidth * 0.55f, buttonIndex * (buttonHeight + 5) + 10, columnWidth * 0.45f, buttonHeight), _yBorder);
                ++buttonIndex;

                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Apply"))
                {
                    Vector2d latLon, domainSize;
                    if(ParseVector2d(_latitude, _longitude, out latLon) && ParseVector2d(_domainSizeX, _domainSizeY, out domainSize))
                    {
                        _workingData.SetSimulatonData(latLon, domainSize);
                    }                    
                }
                ++buttonIndex;
            }

            //GPW stuff
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "GPW tools");
            ++buttonIndex;            

            if (_workingData.HaveLocalGPW)
            {
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Population:" + _workingData.LocalGPWData.TotalPopulation);
                ++buttonIndex;
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Show/hide GPW data"))
                {                    
                    _wuinityManager.SimulationDomainVisualizer.ToggleGPWVisibility();
                    if(_wuinityManager.SimulationDomainVisualizer.IsGPWPlaneVisible())
                    {
                        _wuinityManager.SetSampleMode(DataSampleMode.LocalGPW);
                    }
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
            if (_workingData.HavePopulationMap)
            {
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Population:" + _workingData.PopulationMap.TotalPopulation);
                ++buttonIndex;
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Show/hide population map"))
                {
                    _wuinityManager.SetSampleMode(DataSampleMode.PopulationMap);
                    _wuinityManager.SimulationDomainVisualizer.ToggleVisibility();

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
            if (_workingData.HavePopulationMap)
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
                        ScalePopulation();
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

            //Population stuff
            ++buttonIndex;
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Population tools");
            ++buttonIndex;
            if(_workingData.HavePopulationMap && _workingData.PopulationMapCorrectedForRoadAccess)
            {
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Min/max household size");
                ++buttonIndex;
                _minHouseholdSize = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth * 0.45f, buttonHeight), _minHouseholdSize);
                _maxHouseholdSize = GUI.TextField(new Rect(buttonColumnStart + columnWidth * 0.55f, buttonIndex * (buttonHeight + 5) + 10, columnWidth * 0.45f, buttonHeight), _maxHouseholdSize);
                ++buttonIndex;                               

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
            if(_input.TriggerBuffer.kPERILInput != null)
            {
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Run k-PERIL"))
                {
                    //float[,] tB = PREACT.kPERIL.RunPERIL(_input.TriggerBuffer.kPERILInput.MidflameWindspeed);
                    //_engine.Simulation.SetTriggerBufferData(tB);
                    //_engine.Simulation.DisplayTriggerBuffer();
                    PREACT.Engine.Message(null, PREACT.Engine.LogType.Debug, "Not yet implemented.");
                }
            }   
        }        

        //GPW
        void OpenCreateAndSaveLocalGPW()
        {
            string initialPath = Path.GetDirectoryName(_input.RootFolder);
            FileBrowser.ShowLoadDialog(CreateLocalGPW, CancelSaveLoad, FileBrowser.PickMode.Folders, false, initialPath, null, "Select global GPW folder", "Set");
        }
        void CreateLocalGPW(string[] paths)
        {
            LocalGPWData data = PopulationTools.CreateLocalGPWData(_input.Simulation.LowerLeftLatLon, _input.Simulation.DomainSize, paths[0], out success);
            if (success)
            {
                _wuinityManager.SimulationDomainVisualizer.SetAndDisplayLocalGPW(data, _workingData);
            }
            
        }
        void OpenLoadLocalGPW()
        {
            FileBrowser.SetFilters(false, gpwFilter);
            string initialPath = Path.GetDirectoryName(_input.RootFolder);
            FileBrowser.ShowLoadDialog(LoadLocalGPW, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Select local GPW data", "Set");
        }
        void LoadLocalGPW(string[] paths)
        {
            bool success;
            LocalGPWData localGPWData = PopulationTools.LoadLocalGPWData(paths[0], out success);
            if(success)
            {
                _wuinityManager.SimulationDomainVisualizer.SetAndDisplayLocalGPW(localGPWData, _workingData);
            }
        }

        //Interpolated GPW
        void OpenCreateAndSavePopulationMap()
        {
            FileBrowser.SetFilters(false, gpwFilter);
            string initialPath = Path.GetDirectoryName(_input.RootFolder);
            FileBrowser.ShowLoadDialog(CreateAndSavePopulationMap, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Select local GPW data", "Set");
        }        
        void CreateAndSavePopulationMap(string[] paths)
        {
            string filePath = Path.Combine(Path.GetDirectoryName(paths[0]), Path.GetFileNameWithoutExtension(paths[0]) + ".pop");
            PopulationMap pMap = PopulationTools.CreateAndSavePopulationMap(_workingData.SimulationInput, paths[0], _populationMapCellSize, filePath, out success);
            if (success)
            {
                _workingData.PopulationMap = pMap;
                _wuinityManager.SimulationDomainVisualizer.SetAndDisplayPopulationMapTexture(_workingData.PopulationMap, _workingData);
            }
        }
        void OpenLoadPopulationMap()
        {
            FileBrowser.SetFilters(false, populationMapFilter);
            string initialPath = Path.GetDirectoryName(_engine.WorkingFolder);
            FileBrowser.ShowLoadDialog(LoadPopulationMap, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Select population map", "Set");
        }
        void LoadPopulationMap(string[] paths)
        {
            PopulationMap pMap = PopulationTools.LoadPopulationMap(paths[0], out success);
            if(success)
            {
                _workingData.PopulationMap = pMap;
                _wuinityManager.SimulationDomainVisualizer.SetAndDisplayPopulationMapTexture(_workingData.PopulationMap, _workingData);
            }            
        }

        void ScalePopulation()
        {
            PopulationTools.ScaleTotalPopulation(_workingData.PopulationMap, _desiredPopulation, out success);
            _wuinityManager.SimulationDomainVisualizer.SetAndDisplayPopulationMapTexture(_workingData.PopulationMap, _workingData);
            _wuinityManager.SimulationDomainVisualizer.ToggleVisibility();
        }

        //Filtering of OSM        
        void OpenFilterOSM()
        {
            FileBrowser.SetFilters(false, osmFilter);
            string initialPath = Path.GetDirectoryName(_engine.WorkingFolder);
            FileBrowser.ShowLoadDialog(FilterOSM, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Select OSM data to filter spatially", "Set");
        }
        void FilterOSM(string[] paths)
        {
            PopulationTools.FilterOsmData(paths[0], _xBorder, _yBorder, _latitude, _longitude, _domainSizeX, _domainSizeY);            
        }

        //Router Db
        string _firstFileInSequence;
        void OpenCreateAndSaveRouterDb()
        {
            FileBrowser.SetFilters(false, osmFilter);
            string initialPath = Path.GetDirectoryName(_input.RootFolder);
            FileBrowser.ShowLoadDialog(OpenSelectNewRouterDbFile, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Select OSM file to build routerDb from", "Set");
        }
        void OpenSelectNewRouterDbFile(string[] paths)
        {
            _firstFileInSequence = paths[0];
            FileBrowser.SetFilters(false, routerDbFilter);
            string initialPath = Path.GetDirectoryName(_input.RootFolder);
            FileBrowser.ShowLoadDialog(CreateAndSaveRouterDb, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Specify filename of new routerDb", "Set");
        }
        void CreateAndSaveRouterDb(string[] paths)
        {
            PopulationTools.CreateAndSaveRouterDb(_firstFileInSequence, paths[0]);
        }
        /*void OpenLoadRouterDb()
        {
            FileBrowser.SetFilters(false, routerDbFilter);
            string initialPath = Path.GetDirectoryName(WUI_input.RootFolder);
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
            string initialPath = Path.GetDirectoryName(_input.RootFolder);
            FileBrowser.ShowLoadDialog(RoadAccessCorrectPopulationMap, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Select routerDb to use for road access correction", "Set");
        }
        void RoadAccessCorrectPopulationMap(string[] paths)
        {
            _workingData.SimulationInput.Data.UpdateData(_latitude, _longitude, out success);
            if(success)
            {
                PopulationTools.RoadAccessCorrectPopulationMap(_workingData.PopulationMap, _workingData.SimulationInput.Data, paths[0], out success);
            }            
        }

        void OpenApplyMaskOnPopulationMap()
        {
            FileBrowser.SetFilters(false, maskFilter);
            string initialPath = Path.GetDirectoryName(_input.RootFolder);
            FileBrowser.ShowLoadDialog(ApplyPopulationMapMask, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Select population map mask to apply", "Set");
        }
        void ApplyPopulationMapMask(string[] paths)
        {
            PopulationTools.ApplyPopulationMapMask(_workingData.PopulationMap, paths[0]);
        }

        //create population
        /*void OpenCreatePopulation()
        {
            FileBrowser.SetFilters(false, populationMapFilter);
            string initialPath = Path.GetDirectoryName(WUI_input.RootFolder);
            FileBrowser.ShowLoadDialog(CreatePopulation, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Select routerDb to use for road access correction", "Set");
        }*/
        private void OpenCreatePopulation() //string[] paths
        {
            FileBrowser.SetFilters(false, csvFilter);
            string initialPath = Path.GetDirectoryName(_input.RootFolder);
            FileBrowser.ShowLoadDialog(CreatePopulation, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Specify population output file", "Create");
        }
        private void CreatePopulation(string[] paths) //string[] paths
        {
            if(_input == null)
            {
                _workingData.SimulationInput.Data.UpdateData(_latitude, _longitude, out success);
                PopulationTools.CreatePopulation(_minHouseholdSize, _maxHouseholdSize, _workingData.PopulationMap, _workingData.SimulationInput.Data, paths[0], out success);
            }
            else
            {
                PopulationTools.CreatePopulation(_minHouseholdSize, _maxHouseholdSize, _workingData.PopulationMap, _input.Simulation.Data, paths[0], out success);
            }
        }

        private void OpenSavePopulationMask() //string[] paths
        {
            FileBrowser.SetFilters(false, maskFilter);
            string initialPath = Path.GetDirectoryName(_input.RootFolder);
            FileBrowser.ShowLoadDialog(SavePopulationMask, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Specify population output file", "Create");
        }
        private void SavePopulationMask(string[] paths) //string[] paths
        {
            PopulationTools.SavePopulationMask(_workingData.PopulationMap, paths[0]);
        }
        
    }
}