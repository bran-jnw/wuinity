using UnityEngine;
using SimpleFileBrowser;
using System.IO;
using WUIPlatform.IO;

namespace WUIPlatform.WUInity.UI
{
    public partial class WUInityGUI
    {
        private bool populationMenuDirty = true;
        private bool _reScaling = false, _filteringOSM = false, _creatingPopulationMap = false;
        private string _desiredPopulation, _xBorder, _yBorder, _populationMapCellSize;    

        void ToolsMenu()
        {
            PopulationInput popIn = WUIEngine.INPUT.Population;
            if (populationMenuDirty)
            {
                populationMenuDirty = false;
            }
            GUI.Box(new Rect(120, 0, columnWidth + 40, Screen.height - consoleHeight), "");
            int buttonIndex = 0;
            int buttonColumnStart = 140;

            /*string localPopStatus = "Population data NOT loaded";
            if (WUIEngine.POPULATION.IsPopulationLoaded())
            {
                localPopStatus = "Population data loaded";
            }
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), localPopStatus);
            ++buttonIndex;*/

            //GPW stuff
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "GPW tools");
            ++buttonIndex;
            if(Tools.PopulationTools.HaveLocalGPW)
            {
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Population:" + WUIEngine.RUNTIME_DATA.Population.LocalGPWData.totalPopulation);
                ++buttonIndex;
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Show/hide GPW data"))
                {
                    WUInityEngine.INSTANCE.SetSampleMode(WUInityEngine.DataSampleMode.LocalGPW);
                    WUIEngine.RUNTIME_DATA.Population.Visualizer.ToggleLocalGPWVisibility();
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
            if (Tools.PopulationTools.HavePopulationMap)
            {
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Population:" + WUIEngine.RUNTIME_DATA.Population.PopulationMap._totalPopulation);
                ++buttonIndex;
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Show/hide population map"))
                {
                    WUInityEngine.INSTANCE.SetSampleMode(WUInityEngine.DataSampleMode.PopulationMap);
                    WUInityEngine.INSTANCE.DisplayPopulationMap();
                    WUInityEngine.INSTANCE.ToggleEvacDataPlane();

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
            if (Tools.PopulationTools.HavePopulationMap)
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
                        Tools.PopulationTools.ScaleTotalPopulation(_desiredPopulation);
                        WUInityEngine.INSTANCE.DisplayPopulationMap();
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
            if(Tools.PopulationTools.HavePopulationMap)
            {
                if (!WUInityEngine.INSTANCE.IsPainterActive())
                {
                    if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Create population mask"))
                    {
                        WUInityEngine.INSTANCE.StartPainter(Painter.PaintMode.PopulationMask);
                    }
                    ++buttonIndex;
                }
                else
                {
                    GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Population mask edit");
                    ++buttonIndex;
                    if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Add area"))
                    {
                        WUInityEngine.Painter.SetMaskGPWColor(true);
                    }
                    ++buttonIndex;
                    if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Remove area"))
                    {
                        WUInityEngine.Painter.SetMaskGPWColor(false);
                    }
                    ++buttonIndex;

                    if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Close & save"))
                    {
                        Tools.PopulationTools.SavePopulationMask();
                        WUInityEngine.INSTANCE.StopPainter();
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
            if(Tools.PopulationTools.HavePopulationMap && Tools.PopulationTools.PopulationMapCorrectedForRoadAccess)
            {
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Create & load population"))
                {
                    CreateAndLoadPopulation();
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
            if(WUIEngine.INPUT.TriggerBuffer.kPERILInput != null)
            {
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Run k-PERIL"))
                {
                    float[,] tB = WUIPlatformPERIL.RunPERIL(WUIEngine.INPUT.TriggerBuffer.kPERILInput.MidflameWindspeed);
                    WUIEngine.SIM.SetTriggerBufferOutput(tB);
                    WUIEngine.SIM.DisplayTriggerBuffer();
                }
            }   
        }        

        //GPW
        void OpenCreateAndSaveLocalGPW()
        {
            string initialPath = Path.GetDirectoryName(WUIEngine.WORKING_FOLDER);
            FileBrowser.ShowLoadDialog(CreateAndSaveLocalGPW, CancelSaveLoad, FileBrowser.PickMode.Folders, false, initialPath, null, "Select global GPW folder", "Set");
        }
        void CreateAndSaveLocalGPW(string[] paths)
        {
            Tools.PopulationTools.CreateAndSaveLocalGPWData(paths[0]);
            WUIEngine.RUNTIME_DATA.Population.Visualizer.SetDataPlane(true);
        }
        void OpenLoadLocalGPW()
        {
            FileBrowser.SetFilters(false, gpwFilter);
            string initialPath = Path.GetDirectoryName(WUIEngine.WORKING_FOLDER);
            FileBrowser.ShowLoadDialog(LoadLocalGPW, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Select local GPW data", "Set");
        }
        void LoadLocalGPW(string[] paths)
        {
            Tools.PopulationTools.LoadLocalGPWData(paths[0]);
            WUIEngine.RUNTIME_DATA.Population.Visualizer.SetDataPlane(true);
        }

        //Interpolated GPW
        void OpenCreateAndSavePopulationMap()
        {
            FileBrowser.SetFilters(false, gpwFilter);
            string initialPath = Path.GetDirectoryName(WUIEngine.WORKING_FOLDER);
            FileBrowser.ShowLoadDialog(CreateAndSavePopulationMap, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Select local GPW data", "Set");
        }        
        void CreateAndSavePopulationMap(string[] paths)
        {
            Tools.PopulationTools.CreateAndSavePopulationMap(paths[0], _populationMapCellSize);
            WUInityEngine.INSTANCE.DisplayPopulationMap();
            WUInityEngine.INSTANCE.SetPopulationDataPlane(true);           
        }
        void OpenLoadPopulationMap()
        {
            FileBrowser.SetFilters(false, populationMapFilter);
            string initialPath = Path.GetDirectoryName(WUIEngine.WORKING_FOLDER);
            FileBrowser.ShowLoadDialog(LoadPopulationMap, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Select population map", "Set");
        }
        void LoadPopulationMap(string[] paths)
        {
            Tools.PopulationTools.LoadPopulationMap(paths[0]);
            WUInityEngine.INSTANCE.DisplayPopulationMap();
            WUInityEngine.INSTANCE.SetPopulationDataPlane(true);
        }

        //Filtering of OSM        
        void OpenFilterOSM()
        {
            FileBrowser.SetFilters(false, osmFilter);
            string initialPath = Path.GetDirectoryName(WUIEngine.WORKING_FOLDER);
            FileBrowser.ShowLoadDialog(FilterOSM, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Select OSM data to filter spacially", "Set");
        }
        void FilterOSM(string[] paths)
        {
            Tools.PopulationTools.FilterOsmData(paths[0], _xBorder, _yBorder);            
        }

        //Router Db
        void OpenCreateAndSaveRouterDb()
        {
            FileBrowser.SetFilters(false, osmFilter);
            string initialPath = Path.GetDirectoryName(WUIEngine.WORKING_FOLDER);
            FileBrowser.ShowLoadDialog(CreateAndSaveRouterDb, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Select OSM file to build routerDb from", "Set");
        }
        void CreateAndSaveRouterDb(string[] paths)
        {
            Tools.PopulationTools.CreateAndSaveRouterDb(paths[0]);
        }
        /*void OpenLoadRouterDb()
        {
            FileBrowser.SetFilters(false, routerDbFilter);
            string initialPath = Path.GetDirectoryName(WUIEngine.WORKING_FOLDER);
            FileBrowser.ShowLoadDialog(LoadRouterDb, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Select local GPW data", "Set");
        }
        void LoadRouterDb(string[] paths)
        {
            Tools.PopulationTools.LoadRouterDb(paths[0]);
        }*/

        //filter population map
        void OpenRoadAccessCorrectPopulationMap()
        {
            FileBrowser.SetFilters(false, routerDbFilter);
            string initialPath = Path.GetDirectoryName(WUIEngine.WORKING_FOLDER);
            FileBrowser.ShowLoadDialog(RoadAccessCorrectPopulationMap, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Select routerDb to use for road access correction", "Set");
        }
        void RoadAccessCorrectPopulationMap(string[] paths)
        {
            Tools.PopulationTools.RoadAccessCorrectPopulationMap(paths[0]);
        }

        void OpenApplyMaskOnPopulationMap()
        {
            FileBrowser.SetFilters(false, maskFilter);
            string initialPath = Path.GetDirectoryName(WUIEngine.WORKING_FOLDER);
            FileBrowser.ShowLoadDialog(ApplyPopulationMapMask, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Select population map mask to apply", "Set");
        }
        void ApplyPopulationMapMask(string[] paths)
        {
            Tools.PopulationTools.ApplyPopulationMapMask(paths[0]);
        }

        //create population
        /*void OpenCreatePopulation()
        {
            FileBrowser.SetFilters(false, populationMapFilter);
            string initialPath = Path.GetDirectoryName(WUIEngine.WORKING_FOLDER);
            FileBrowser.ShowLoadDialog(CreatePopulation, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Select routerDb to use for road access correction", "Set");
        }*/
        void CreateAndLoadPopulation() //string[] paths
        {
            Tools.PopulationTools.CreateAndLoadPopulation(); //paths[0]
        }
    }
}