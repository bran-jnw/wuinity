using System.IO;
using SimpleFileBrowser;

namespace Assets.WUInity.GUI.DearIMGUI
{
    public static class FileBrowserGUI
    {
        //filters
        static string[] wuiFilter = new string[] { ".wui" };
        static string[] lcpFilter = new string[] { ".lcp", ".tif", ".tiff" };
        static string[] geoTiffFilter = new string[] { ".tif", ".tiff" };
        static string[] fuelModelsFilter = new string[] { ".fuel" };

        static void CancelSaveLoad()
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
            FileBrowser.ShowSaveDialog(ScenarioEditorGUI.SaveNewInput, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, ".wui", "Save file", "Save");
        }
       

        /*public static void OpenCreateBaseData()
        {
            FileBrowser.ShowSaveDialog(NewScenarioWindow.CreateBaseData, CancelSaveLoad, FileBrowser.PickMode.Folders, false, null, null, "Select root folder", "Create data");
        }*/

    }
}
