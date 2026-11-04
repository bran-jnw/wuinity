using ImGuiNET;
using System.IO;

namespace Assets.WUInity.GUI.DearIMGUI
{
    public static class PopulationEditWindow
    {
        private static bool _isOpen, _folderSet, _setPopulationCountTarget = false;
        private static string _downloadFolder = string.Empty, _routerDbFilePath = string.Empty, _worldPopFilePath = string.Empty, _householdDemographicsFilePath = string.Empty;
        private static int _populationCountTarget;

        public static void Open()
        {
            if (!_isOpen)
            {
                PreactGUI.DrawWindow(Draw);
            }
            _isOpen = true;
        }

        public static void Close()
        {
            if (_isOpen)
            {
                PreactGUI.CloseWindow(Draw);
            }
            _isOpen = false;
        }

        public static void Draw()
        {
            if (!_isOpen)
            {
                return;
            }
            ImGui.Begin("Population edit tool", ref _isOpen, PreactGUI.NoDockingNoCollapse);

            if (ImGui.Button("Set working folder")) { OpenSetWorkingFolder(); }
            if (!_folderSet)
            {
                return;
            }
            ImGui.Text("Download folder set to:" + _downloadFolder);

            //ImGui.SeparatorText("Area of interest (AIO)");

            if (ImGui.Button("Create routerDb")) { FileBrowser.OpenSetFilePath(CreateRouterDb, "Select OSM file", false); }

            ImGui.Checkbox("Set population target", ref _setPopulationCountTarget);
            if (_setPopulationCountTarget)
            {
                ImGui.InputInt("Population target count", ref _populationCountTarget);
            }
            if (ImGui.Button("Create population from WorldPop")) { FileBrowser.OpenSetFilePath(SetHouseholdDemographics, "Select household demograhics file", false); }
            if (ImGui.Button("Scale population")) { }            

            ImGui.End();
            if (!_isOpen)
            {
                PreactGUI.CloseWindow(Draw);
            }
        }

        static void SetHouseholdDemographics(string householdDemographicsFilePath)
        {
            if(_populationCountTarget <= 0)
            {
                PREACT.Engine.Message(null, PREACT.Engine.LogType.Warning, "Please set a valid target population count.");
                return;
            }

            if(!_setPopulationCountTarget)
            {
                _populationCountTarget = 0;
            }

            _householdDemographicsFilePath = householdDemographicsFilePath;
            PREACT.Engine.Message(null, PREACT.Engine.LogType.Log, $"Household demographics file path set to {_householdDemographicsFilePath}");
            FileBrowser.OpenSetFilePath(SetRouterDb, "Select RouterDb file", false);
        }

        static void CreateRouterDb(string osmFilePath)
        {
            string osmFileName = Path.GetFileNameWithoutExtension(osmFilePath);
            string routerDbFilePath = Path.Combine(_downloadFolder, osmFileName + ".routerdb");
            PREACT.Tools.PopulationTools.CreateAndSaveRouterDb(osmFilePath, routerDbFilePath, out bool success);
        }

        static void SetRouterDb(string routerDbFilePath)
        {
            _routerDbFilePath = routerDbFilePath;
            PREACT.Engine.Message(null, PREACT.Engine.LogType.Log, $"RouterDb file path set to {_routerDbFilePath}");
            FileBrowser.OpenSetFilePath(SetWorldPop, "Select WorldPop file", false);
        }

        static void SetWorldPop(string worldPopFilePath)
        {
            _worldPopFilePath = worldPopFilePath;
            PREACT.Engine.Message(null, PREACT.Engine.LogType.Log, $"WorldPop file path set to {_worldPopFilePath}");
            string outputFilePath = Path.Combine(_downloadFolder, "population.csv");
            PREACT.Tools.PopulationTools.CreatePopulationFromWorldPop(_populationCountTarget, _householdDemographicsFilePath, _worldPopFilePath, _routerDbFilePath, outputFilePath, out bool success);
        }

        private static void OpenSetWorkingFolder()
        {
            string initialFolder = PreactGUI.Engine.WorkingFolder;
            SimpleFileBrowser.FileBrowser.ShowLoadDialog(SetRootFolder, FileBrowser.Cancel, SimpleFileBrowser.FileBrowser.PickMode.Folders, false, initialFolder, null, "Set download folder", "Set");
        }
        private static void SetRootFolder(string[] paths)
        {
            _folderSet = true;
            _downloadFolder = paths[0];
        }
    }
}
