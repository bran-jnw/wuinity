using SimpleFileBrowser;
using System;
using System.IO;

namespace Assets.WUInity.GUI.DearIMGUI
{
    public static class FileBrowser
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
            SimpleFileBrowser.FileBrowser.SetFilters(false, wuiFilter);
            string initialPath = PreactGUI.Engine.WorkingFolder;
            SimpleFileBrowser.FileBrowser.ShowLoadDialog(LoadInput, CancelSaveLoad, SimpleFileBrowser.FileBrowser.PickMode.Files, false, initialPath, null, "Load WUI file", "Load");
        }
        private static void LoadInput(string[] paths)
        {
            bool success;
            PreactGUI.Engine.LoadInputFromFile(paths[0], out success);
        }

        public static void OpenSaveInput()
        {
            SimpleFileBrowser.FileBrowser.SetFilters(false, wuiFilter);
            string initialPath = PreactGUI.Engine.WorkingFolder;
            SimpleFileBrowser.FileBrowser.ShowSaveDialog(ScenarioEditorWindow.SaveNewInput, CancelSaveLoad, SimpleFileBrowser.FileBrowser.PickMode.Files, false, initialPath, ".wui", "Save file", "Save");
        }

        private static Action<string> _onFileSet;
        public static void OpenSetFilePath(Action<string> onFileSet)
        {
            _onFileSet = onFileSet;
            SimpleFileBrowser.FileBrowser.SetFilters(true);
            string initialPath = PreactGUI.Engine.WorkingFolder;
            SimpleFileBrowser.FileBrowser.ShowLoadDialog(SetFilePath, CancelSaveLoad, SimpleFileBrowser.FileBrowser.PickMode.Files, false, initialPath, null, "Set file", "Set");
        }
        private static void SetFilePath(string[] paths)
        {
            string relativePath = Path.GetRelativePath(PreactGUI.Engine.WorkingFolder, paths[0]);
            _onFileSet?.Invoke(relativePath);
            _onFileSet = null;
        }

        /*public static void OpenCreateBaseData()
        {
            FileBrowser.ShowSaveDialog(NewScenarioWindow.CreateBaseData, CancelSaveLoad, FileBrowser.PickMode.Folders, false, null, null, "Select root folder", "Create data");
        }*/

    }
}
