using SimpleFileBrowser;
using System;
using System.IO;

namespace Assets.WUInity.GUI.DearIMGUI
{
    public static class FileBrowser
    {
        //filters
        public static readonly string[] wuiFilter = new string[] { ".wui" };
        public static readonly string[] lcpFilter = new string[] { ".lcp", ".tif", ".tiff" };
        public static readonly string[] geoTiffFilter = new string[] { ".tif", ".tiff" };
        public static readonly string[] fuelModelsFilter = new string[] { ".fuel" };

        public static void Cancel()
        {

        }

        public static void OpenLoadInput()
        {
            SimpleFileBrowser.FileBrowser.SetFilters(false, wuiFilter);
            string initialPath = PreactGUI.Engine.WorkingFolder;
            SimpleFileBrowser.FileBrowser.ShowLoadDialog(LoadInput, Cancel, SimpleFileBrowser.FileBrowser.PickMode.Files, false, initialPath, null, "Load WUI file", "Load");
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
            SimpleFileBrowser.FileBrowser.ShowSaveDialog(ScenarioEditorWindow.SaveNewInput, Cancel, SimpleFileBrowser.FileBrowser.PickMode.Files, false, initialPath, ".wui", "Save file", "Save");
        }

        private static bool _getRelativePath;
        private static Action<string> _onFileSet;
        public static void OpenSetFilePath(Action<string> onFileSet, string dialogHeader, bool getRelativePath, string[] fileFilter = null)
        {
            _onFileSet = onFileSet;
            _getRelativePath = getRelativePath;

            if(fileFilter == null)
            {
                SimpleFileBrowser.FileBrowser.SetFilters(true);
            }
            else
            {
                SimpleFileBrowser.FileBrowser.SetFilters(true, fileFilter);
            }
            string initialPath = PreactGUI.Engine.WorkingFolder;
            SimpleFileBrowser.FileBrowser.ShowLoadDialog(SetFilePath, Cancel, SimpleFileBrowser.FileBrowser.PickMode.Files, false, initialPath, null, dialogHeader, "Set");
        }

        private static void SetFilePath(string[] paths)
        {
            string filePath = paths[0];
            if (_getRelativePath)
            {
                filePath = Path.GetRelativePath(PreactGUI.Engine.WorkingFolder, paths[0]);
            }            
            _onFileSet?.Invoke(filePath);
            //_onFileSet = null;
        }

        /*public static void OpenCreateBaseData()
        {
            FileBrowser.ShowSaveDialog(NewScenarioWindow.CreateBaseData, CancelSaveLoad, FileBrowser.PickMode.Folders, false, null, null, "Select root folder", "Create data");
        }*/

    }
}
