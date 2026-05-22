using UnityEngine;
using SimpleFileBrowser;
using System.IO;
using PREACT.Input;
using PREACT.Tools;
using PREACT.Population;
using PREACT.Math;

namespace WUInity.UI
{
    public partial class WUInityGUI
    {
        private bool populationMenuDirty = true;
        private bool _reScaling = false, _filteringOSM = false, _creatingPopulationMap = false;
        private string _desiredPopulation, _xBorder, _yBorder, _populationMapCellSize, _minHouseholdSize, _maxHouseholdSize, _latitude, _longitude, _domainSizeX, _domainSizeY, _scenarioId, _yearOfInterest;
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
            //PopulationInput popIn = _input.Population;
            if (populationMenuDirty)
            {
                populationMenuDirty = false;
            }
            GUI.Box(new Rect(120, 0, columnWidth + 40, Screen.height - consoleHeight), "");
            int buttonIndex = 0;
            int buttonColumnStart = 140;

            ++buttonIndex;
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Scenario Id");
            ++buttonIndex;
            _scenarioId = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), _scenarioId);
            ++buttonIndex;
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Year of interest");
            ++buttonIndex;
            _yearOfInterest = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), _yearOfInterest);
            ++buttonIndex;
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
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Min/max household size");
            ++buttonIndex;
            _minHouseholdSize = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth * 0.45f, buttonHeight), _minHouseholdSize);
            _maxHouseholdSize = GUI.TextField(new Rect(buttonColumnStart + columnWidth * 0.55f, buttonIndex * (buttonHeight + 5) + 10, columnWidth * 0.45f, buttonHeight), _maxHouseholdSize);
            ++buttonIndex;
            //base data creation
            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Create base data"))
            {
                OpenCreateBaseData();
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

            //Population stuff
            ++buttonIndex;
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Population tools");
            ++buttonIndex;

            

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Create population from WorldPop"))
            {
                OpenCreatePopulationFromWorldPop();
            }
            ++buttonIndex;   

            //OSM stuff
            ++buttonIndex;
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "OSM tools");
            ++buttonIndex;            
            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Download OSM"))
            {
                OpenDownloadOSM();
            }
            ++buttonIndex;

            if (!_filteringOSM)
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
            if(_input != null && _input.TriggerBufferModule.kPERILInput != null)
            {
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Run k-PERIL"))
                {
                    //float[,] tB = PREACT.kPERIL.RunPERIL(_input.TriggerBuffer.kPERILInput.MidflameWindspeed);
                    //_engine.Simulation.SetTriggerBufferData(tB);
                    //_engine.Simulation.DisplayTriggerBuffer();
                    PREACT.Engine.Message(null, PREACT.Engine.LogType.Debug, "Not yet implemented.");
                }
            }

            //Landscape tools
            ++buttonIndex;
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Landscape tools");
            ++buttonIndex;
            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Tiff->LCP"))
            {
                OpenSelectGeoTiff();
            }
        }

        void ScalePopulation()
        {
            int.TryParse(_desiredPopulation, out int desiredPop);
            PopulationTools.ScaleTotalPopulation(_workingData.PopulationMap, desiredPop, out success);
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
            string initialPath = _input == null ? null : Path.GetDirectoryName(_input.RootFolder);
            FileBrowser.ShowLoadDialog(OpenSelectNewRouterDbFile, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Select OSM file to build routerDb from", "Set");
        }
        void OpenSelectNewRouterDbFile(string[] paths)
        {
            _firstFileInSequence = paths[0];
            FileBrowser.SetFilters(false, routerDbFilter);
            string routerDbFilePath = Path.Combine(Path.GetDirectoryName(paths[0]), Path.GetFileNameWithoutExtension(paths[0]) + ".routerdb");
            PopulationTools.CreateAndSaveRouterDb(_firstFileInSequence, routerDbFilePath, out bool success);
        }

        //create population from world pop raster 
        private void OpenCreatePopulationFromWorldPop()
        {
            FileBrowser.SetFilters(false, geoTiffFilter);
            string initialPath = null;
            if (_input != null)
            {
                initialPath = Path.GetDirectoryName(_input.RootFolder);
            }
            FileBrowser.ShowLoadDialog(SaveGeoTiffLocation, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Select WorldPop GeoTiff", "Load");
        }
        string _worldPopFilePath;
        string _routerDbFilePath;
        private void SaveGeoTiffLocation(string[] paths)
        {
            _worldPopFilePath = paths[0];
            OpenSelectRouterDb();
        }
        private void OpenSelectRouterDb()
        {
            FileBrowser.SetFilters(false, routerDbFilter);
            string initialPath = Path.GetDirectoryName(_worldPopFilePath);
            FileBrowser.ShowLoadDialog(SaveRouterDbFilePath, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Select RouterDb", "Load");
        }
        private void SaveRouterDbFilePath(string[] paths)
        {
            _routerDbFilePath = paths[0];
            OpenSavePopulation();
        }
        private void OpenSavePopulation()
        {
            FileBrowser.SetFilters(false, csvFilter);
            string initialPath = Path.GetDirectoryName(_worldPopFilePath);
            FileBrowser.ShowSaveDialog(CreatePopulationFromWorldPop, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Specify population file name", "Save");
        }
        private void CreatePopulationFromWorldPop(string[] paths) //string[] paths
        {
            //PopulationTools.CreatePopulationFromWorldPop(_minHouseholdSize, _maxHouseholdSize, _worldPopFilePath, _routerDbFilePath, paths[0], out success);
        }

        //convert geotiff to lcp
        private void OpenSelectGeoTiff()
        {
            FileBrowser.SetFilters(false, geoTiffFilter);
            FileBrowser.ShowLoadDialog(SaveGeoTiffFilePath, CancelSaveLoad, FileBrowser.PickMode.Files, false, null, null, "Select Landscape GeoTiff", "Load");
        }
        string _geoTiffFilePath;
        private void SaveGeoTiffFilePath(string[] paths)
        {
            _geoTiffFilePath = paths[0];
            OpenSaveLCP();
        }
        private void OpenSaveLCP()
        {
            FileBrowser.SetFilters(false, lcpFilter);
            string initialPath = Path.GetDirectoryName(_geoTiffFilePath);
            FileBrowser.ShowSaveDialog(SaveLCPFile, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Specify LCP file name", "Save");
        }
        private void SaveLCPFile(string[] paths)
        {
            PREACT.Wildfire.LandscapeData landscape = new PREACT.Wildfire.LandscapeData(_geoTiffFilePath, Vector2d.zero);
            landscape.SaveLCP(paths[0]);
        }

        //OSM
        private void OpenDownloadOSM()
        {
            FileBrowser.SetFilters(false, ".osm.xml");
            FileBrowser.ShowSaveDialog(DownloadOSM, CancelSaveLoad, FileBrowser.PickMode.Files, false, null, null, "Specify OSM file name", "Save");
        }
        private async void DownloadOSM(string[] paths)
        {
            double.TryParse(_latitude, out double lat);
            double.TryParse(_longitude, out double lon);
            Vector2d latLon = new Vector2d(lat, lon);
            double.TryParse(_domainSizeX, out double xSize);
            double.TryParse(_domainSizeY, out double ySize);
            Vector2d domainSize = new Vector2d(xSize, ySize);
            _workingData.SetSimulatonData(latLon, domainSize);
            latLon = _workingData.SimulationInput.Data.GetWGS84FromSimulationPosition(new Vector2d(-1000.0, -1000.0));
            Vector2d upperLatLon = _workingData.SimulationInput.Data.GetWGS84FromSimulationPosition(new Vector2d(domainSize.x + 1000.0, domainSize.y + 1000.0));
            await OSMDownloader.Download(latLon, upperLatLon, paths[0]);
        }

        //one button data downlaoder
        private void OpenCreateBaseData()
        {
            FileBrowser.ShowSaveDialog(CreateBaseData, CancelSaveLoad, FileBrowser.PickMode.Folders, false, null, null, "Select root folder", "Create data");
        }
        private async void CreateBaseData(string[] paths)
        {
            double.TryParse(_latitude, out double lat);
            double.TryParse(_longitude, out double lon);
            Vector2d lowerLatLon = new Vector2d(lat, lon);
            double.TryParse(_domainSizeX, out double xSize);
            double.TryParse(_domainSizeY, out double ySize);
            Vector2d domainSize = new Vector2d(xSize, ySize);
            _workingData.SetSimulatonData(lowerLatLon, domainSize);
            Vector2d upperLatLon = _workingData.SimulationInput.Data.GetWGS84FromSimulationPosition(domainSize);

            int.TryParse(_minHouseholdSize, out int min);
            int.TryParse(_maxHouseholdSize, out int max);

            int.TryParse(_yearOfInterest, out int year);

            await PopulationTools.CreateBaseScenario(paths[0], _scenarioId, min, max, lowerLatLon, upperLatLon, year);
        }
    }
}