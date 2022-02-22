using UnityEngine;
using SimpleFileBrowser;
using System.IO;

namespace WUInity
{
    public partial class WUInityGUI
    {
        string dT, nrRuns, Lat, Long, sizeX, sizeY, zoom;
        bool mainInputDirty = true, creatingNewFile = false;

        void MainMenu()
        {
            WUInityInput wO = WUInity.INPUT;

            //whenever we load a file we need to set the new data for the GUI
            if (mainInputDirty)
            {
                mainInputDirty = false;
                dT = wO.deltaTime.ToString();
                nrRuns = wO.numberOfRuns.ToString();
                Lat = wO.lowerLeftLatLong.x.ToString();
                Long = wO.lowerLeftLatLong.y.ToString();
                sizeX = wO.size.x.ToString();
                sizeY = wO.size.y.ToString();
                zoom = wO.zoomLevel.ToString();
            }

            GUI.Box(new Rect(subMenuXOrigin, 0, columnWidth + 40, Screen.height - consoleHeight), "");
            int buttonIndex = 0;

            if (!WUInity.MAP.IsAccessTokenValid)
            {
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "ERROR: Mapbox token not valid.");
                return;
            }

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "New file"))
            {
                creatingNewFile = true;
                OpenSaveInput();
            }
            ++buttonIndex;

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Load file"))
            {
                OpenLoadInput();
            }
            ++buttonIndex;

            //will remove default and use example instead
            /*if (GUI.Button(new Rect(140, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Load defaults"))
            {
                SaveLoadWUI.LoadDefaultInputs();
                mainInputDirty = true;
            }
            ++buttonIndex;*/

            if (!WUInity.INSTANCE.haveInput)
            {
                return;
            }

            //name
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Simulation name:");
            ++buttonIndex;
            wO.simName = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), wO.simName);
            ++buttonIndex;

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Save input file"))
            {
                if (WUInity.WORKING_FILE == null)
                {
                    OpenSaveInput();
                }
                else
                {
                    ParseMainData(wO);
                    SaveLoadWUI.SaveInput();
                }
            }
            ++buttonIndex;

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Save as"))
            {
                OpenSaveInput();
            }
            ++buttonIndex;

            //LatLong
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Map LL Lat.:");
            ++buttonIndex;
            Lat = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), Lat);
            ++buttonIndex;

            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Map LL Long.:");
            ++buttonIndex;
            Long = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), Long);
            ++buttonIndex;

            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Map size x [m]:");
            ++buttonIndex;
            sizeX = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), sizeX);
            ++buttonIndex;

            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Map size y [m]:");
            ++buttonIndex;
            sizeY = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), sizeY);
            ++buttonIndex;

            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Map zoom level:");
            ++buttonIndex;
            zoom = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), zoom);
            ++buttonIndex;

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Update map"))
            {
                ParseMainData(wO);
                WUInity.INSTANCE.UpdateValidData();
                WUInity.INSTANCE.LoadMapbox();
            }

            buttonIndex += 2;
            //dT
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Time step [s]:");
            ++buttonIndex;
            dT = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), dT);
            ++buttonIndex;

            //number of runs
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Number of runs:");
            ++buttonIndex;
            nrRuns = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), nrRuns);
            ++buttonIndex;

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Start simulation"))
            {
                ParseMainData(wO);
                WUInity.INSTANCE.CompareValidData();
                if (WUInity.INSTANCE.IsMapDirty())
                {
                    WUInity.SIM.LogMessage("ERROR: Map is dirty, update before running simulation.");
                }

                if (WUInity.INSTANCE.IsGPWDirty())
                {
                    WUInity.SIM.LogMessage("ERROR: GPW data is dirty, update before running simulation.");
                }

                if (WUInity.INSTANCE.IsAnythingDirty())
                {
                    WUInity.SIM.LogMessage("ERROR: Could not start simulation, see error log.");
                }
                else
                {
                    WUInity.SIM.LogMessage("LOG: Simulation started, please wait.");
                    if (WUInity.SIM.StartSimFromGUI())
                    {
                        menuChoice = ActiveMenu.Output;
                    }
                }
            }
            ++buttonIndex;
        }

        void ParseMainData(WUInityInput wO)
        {
            ParseEvacInput();
            ParseTrafficInput();

            if (mainInputDirty)
            {
                return;
            }

            float.TryParse(dT, out wO.deltaTime);
            int.TryParse(nrRuns, out wO.numberOfRuns);
            double.TryParse(Lat, out wO.lowerLeftLatLong.x);
            double.TryParse(Long, out wO.lowerLeftLatLong.y);
            double.TryParse(sizeX, out wO.size.x);
            double.TryParse(sizeY, out wO.size.y);
            int.TryParse(zoom, out wO.zoomLevel);
        }

        void OpenSaveInput()
        {
            FileBrowser.SetFilters(false, wuiFilter);
            WUInityInput wO = WUInity.INPUT;
            string initialPath = Path.GetDirectoryName(WUInity.WORKING_FILE);
            FileBrowser.ShowSaveDialog(SaveInput, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, wO.simName + ".wui", "Save file", "Save");
        }

        void OpenLoadInput()
        {
            FileBrowser.SetFilters(false, wuiFilter);
            string initialPath = Path.GetDirectoryName(WUInity.DATA_FOLDER);
            FileBrowser.ShowLoadDialog(LoadInput, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Load WUI file", "Load");
        }

        void OpenLoadLCP()
        {
            FileBrowser.SetFilters(false, lcpFilter);
            string initialPath = Path.GetDirectoryName(WUInity.WORKING_FILE);
            FileBrowser.ShowLoadDialog(LoadLCP, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Load LCP file", "Load");
        }

        void OpenLoadOSM()
        {
            FileBrowser.SetFilters(false, osmFilter);
            string initialPath = Path.GetDirectoryName(WUInity.WORKING_FILE);
            FileBrowser.ShowLoadDialog(LoadOSM, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Load OSM file", "Load");
        }

        void SaveInput(string[] paths)
        {
            WUInityInput wO = WUInity.INPUT;

            WUInity.WORKING_FILE = paths[0];
            if (creatingNewFile)
            {
                mainInputDirty = true;
                WUInity.INSTANCE.NewInputData();
                wO = WUInity.INPUT; //have to update this since we are creating a new one
            }
            else
            {
                ParseMainData(wO);
            }
            creatingNewFile = false;
            string name = Path.GetFileNameWithoutExtension(paths[0]);
            wO.simName = name;

            SaveLoadWUI.SaveInput();
        }

        void LoadInput(string[] paths)
        {
            SaveLoadWUI.LoadInput(paths[0]);
            mainInputDirty = true;
        }

        void LoadLCP(string[] paths)
        {
            WUInityInput wO = WUInity.INPUT;
            wO.fire.lcpFile = paths[0];
            WUInity.SIM.UpdateLCPFile();
        }

        void LoadOSM(string[] paths)
        {
            WUInityInput wO = WUInity.INPUT;
            wO.itinero.osmFile = paths[0];
        }

        void CancelSaveLoad()
        {
            creatingNewFile = false;
        }
    }
}

