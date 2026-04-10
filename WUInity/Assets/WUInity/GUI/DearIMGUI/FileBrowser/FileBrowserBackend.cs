using SimpleFileBrowser;
using System;

namespace Assets.WUInity.GUI.DearIMGUI
{
    public static class FileBrowserBackend
    {
        //filters
        static string[] wuiFilter = new string[] { ".wui" };
        static string[] lcpFilter = new string[] { ".lcp", ".tif", ".tiff" };
        static string[] geoTiffFilter = new string[] { ".tif", ".tiff" };
        static string[] fuelModelsFilter = new string[] { ".fuel" };

        public static void CancelSaveLoad()
        {

        }

        public static void OpenLoadInput()
        {
            FileBrowser.SetFilters(false, wuiFilter);
            string initialPath = PreactGUI.Engine.WorkingFolder;
            FileBrowser.ShowLoadDialog(LoadInput, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Load WUI file", "Load");
        }
        private static void LoadInput(string[] paths)
        {
            bool success;
            PreactGUI.Engine.LoadInputFromFile(paths[0], out success);
        }

        public static void OpenSaveInput()
        {
            FileBrowser.SetFilters(false, wuiFilter);
            string initialPath = PreactGUI.Engine.WorkingFolder;
            FileBrowser.ShowSaveDialog(ScenarioEditorWindow.SaveNewInput, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, ".wui", "Save file", "Save");
        }

        private static Action<string> _onFileSet;
        public static void OpenSetFilePath(Action<string> onFileSet)
        {
            _onFileSet = onFileSet;

            FileBrowser.SetFilters(true);
            string initialPath = PreactGUI.Engine.WorkingFolder;
            FileBrowser.ShowLoadDialog(SetFilePath, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Set file", "Set");
        }
        private static void SetFilePath(string[] paths)
        {
            _onFileSet?.Invoke(paths[0]);
        }

        /*public static void OpenCreateBaseData()
        {
            FileBrowser.ShowSaveDialog(NewScenarioWindow.CreateBaseData, CancelSaveLoad, FileBrowser.PickMode.Folders, false, null, null, "Select root folder", "Create data");
        }*/

    }
}
