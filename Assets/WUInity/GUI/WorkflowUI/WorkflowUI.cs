using System;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using SimpleFileBrowser;
using System.IO;
using static WUInity.TrafficInput;

namespace WUInity.UI
{
    public class WorkflowUI : MonoBehaviour
    {
        public UIDocument Document;

        // WorkflowUI control variables
        bool newUIMenuDirty = false;
        private readonly int _titleBarHeight = 29, _iMainBoxWidth = 450, _iSysLogBoxHeight=160, _iOutputBoxWidth=230, _iLogDisplayNum=6;
        bool mainMenuDirty = true, creatingNewFile = false;

        // WorkflowUI operational variables
        public enum FileType { lcpFile, fuelModelsFile, initialFuelMoistureFile, weatherFile, windFile, ignitionPointsFile, graphicalFireInputFile,
                               GPWFile, POPFile, evacuationGoalFile, evacuationGroupFile, responseCureveFile, OSMDataFile, routerDBFile, routeRCFile, wuiFile, syslogFile }

        string[] fileFilter = { ".lcp", ".fuel", ".fmc", ".wtr", ".wnd", ".ign", ".gfi", ".gpw", ".pop" , ".ed", ".eg", ".rsp", ".pbf", ".routerdb", ".rc", ".wui" ,".log"};

        //string[] togLoadFireFiles = { ".lcp", ".fuel", ".fmc", ".wtr", "TogLoadWindFile" };
        //string[] txtFireFiles = { ".lcp", ".fuel", ".fmc", ".wtr", "TxtWindFile" };

        private bool _bFoldout = true, _bHideOutput=false;
        private readonly int iGPWfolderLength = 42;   // Truncate the GPW folder string to display (normally the full string is too long).

        // Complementary variables for WUINITY scenario configuration
        private string _mapLLLatLong, _mapSizeXY, _mapZoomLevel, _rescalePop;   // Associated variables with location/size/zoom level of map

        private string _simulationID, _simTimeStep, _maxSimTime;

        private string _OSMDataFile="";

        private int iLogCount = 0;

        // example code of an implementation of stored variable, wrapped with a get/set property protocol
        private int iTestMemberVar = 4;
        public int iTestProperty
        {
            get => iTestMemberVar;
            set => iTestMemberVar = Math.Min(0, value);
        }
        // Can now be called as: iTestProperty = 2; (to set it),
        // and the "get" is: int n = menu::iTestProperty; // after the class definition, using the menu:: class object

        // WorkflowUI code starts from here -------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Add code to handle message catching in my dialog
        /// </summary>
        private void Awake()
        {
            InitialiseWorkflow();
            RunSomeBasicTestCode();
        }

        /// <summary>
        /// Start is called before the first frame update
        /// </summary>
        void Start()
        {
            WUInity.GUI.enabled = false;        // Turn off the original WUINITY 2.0 UI at the beginning by default
            Screen.fullScreen = true;           // Enter full screen mode at the beginning

            // Dock the main workflow GUI box to the left edge.
            var root = Document.rootVisualElement;
            UnityEngine.UIElements.VisualElement mainUIBox = root.Q<UnityEngine.UIElements.VisualElement>("MainUIBox");
            if (mainUIBox != null)
            {
                mainUIBox.style.left = 0;
                mainUIBox.style.top = _titleBarHeight +1;
                mainUIBox.style.height = Screen.height - _titleBarHeight -1;
            }

            UnityEngine.UIElements.VisualElement titleBar = root.Q<UnityEngine.UIElements.VisualElement>("TitleBar");
            if (titleBar != null)
            {
                titleBar.style.left = titleBar.style.top = 0;

                UnityEngine.UIElements.Button minimizeButton = root.Q<UnityEngine.UIElements.Button>("TBarMinimizeButton");
                minimizeButton.style.left = Screen.width - 120;
                minimizeButton.clicked += MinimizeButton_clicked;

                UnityEngine.UIElements.Button fullScreenButton = root.Q<UnityEngine.UIElements.Button>("TBarFullScreenButton");
                fullScreenButton.style.left = Screen.width - 80;
                fullScreenButton.clicked += FullScreenButton_clicked;

                UnityEngine.UIElements.Button quitButton = root.Q<UnityEngine.UIElements.Button>("TBarQuitButton");
                quitButton.style.left = Screen.width - 40;
                quitButton.clicked += () => Application.Quit();
            }

            // Hide horizontal scroll bar to have a little extra space
            UnityEngine.UIElements.ScrollView workflow = root.Q<UnityEngine.UIElements.ScrollView>("WorkflowUIElements");
            if (workflow != null) 
            {
                workflow.horizontalScrollerVisibility = ScrollerVisibility.Hidden; 
            }

            //UnityEngine.Debug.Log("menu::Start();}");
        }

        // For windows platform only.
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        private const int SW_MINIMIZE = 6;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        private void MinimizeButton_clicked()
        {
            ShowWindow(GetActiveWindow(), SW_MINIMIZE);
        }

        private void FullScreenButton_clicked()
        {
            Screen.fullScreen = !Screen.fullScreen;
        }

        /// <summary>
        /// Update is called once per frame
        /// </summary>
        void Update()
        {
            if (newUIMenuDirty)
            {
                UpdateMenu();
                newUIMenuDirty = false;
            }

            // Synchronize the count of log with the log window scroller count
            if (iLogCount != WUInity.GetLog().Count)
            {
                iLogCount=WUInity.GetLog().Count;

                UnityEngine.UIElements.Scroller scLogScroll = Document.rootVisualElement.Q<UnityEngine.UIElements.Scroller>("LogScroll");

                if (scLogScroll != null && iLogCount > 0)
                {
                    if(iLogCount> _iLogDisplayNum) 
                        scLogScroll.highValue = (float)(iLogCount - _iLogDisplayNum);
                    else 
                        scLogScroll.highValue = (float)iLogCount;
                }

                scLogScroll.value = scLogScroll.highValue;
            }

            //UnityEngine.Debug.Log("menu::Update;}");
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// My custom methods for handling GUI controls setup and actions
        /// 
        /// <summary>
        /// Manually created methods for invoking workflow actions
        /// </summary>
        private void InitialiseWorkflow()
        {
            // Reference the root visual element for the UI "document"
            var root = Document.rootVisualElement;

            if (root != null)
            {
                // Initialise main controls
                InitWorkflowControl(root);
                InitOptionsControls(root);
                InitValueSlider(root);
                InitFoldoutSwitch(root);
                
                // Start initialize menu items from here

                // 0. Activate three project Button-click handlers -----------------------------------------------------------------
                SetupProjectTasks(root);
                SetupOutputBoxTasks(root);

                // 1. Prepare population, map and traffic network data -------------------------------------------------------------

                // Activate Population data callbacks for workflow task items being selected
                SetupDownloadGPWTask(root);                             
                SetupPlaceGPWInCaseFolder(root);

                SetupDownloadMapboxTokenTask(root);
                SetupPlaceMapboxTokenInFolderTask(root);

                SetupDownloadOSMTask(root);
                SetupPlaceOSMInCaseFolder(root);

                // 2. Configure location and size of region ------------------------------------------------------------------------
                SetupMapConfiguration(root);

                // 3. Population data ----------------------------------------------------------------------------------------------
                SetupGlobalGPWFolder(root);
                PopulateFromLocalGPWFile(root);
                PopulateFromLocalPopFile(root);

                SetupPopulationControlButtons(root);

                // 4. Evacuation goals ----------------------------------------------------------------------------------------------
                SetupAddRemoveEvacGoalButtons(root);
                InitEvacGoalList(root);

                // 5. Evacuation group(s) and response ------------------------------------------------------------------------------
                InitResponseCurveList(root);
                InitEvacGroupList(root);

                SetupEvacGroupRespButtons(root);
                
                // 6. Evacuation settings -------------------------------------------------------------------------------------------

                // 7. Routing data --------------------------------------------------------------------------------------------------
                SetupLoadOSMDataFile(root);

                UnityEngine.UIElements.Button btnBuildRouterDB = root.Q<UnityEngine.UIElements.Button>("BuildRouterDB");
                if (btnBuildRouterDB != null)
                    btnBuildRouterDB.clicked += BtnBuildRouterDB_clicked;

                SetupLoadRouterDBFile(root);

                UnityEngine.UIElements.Button btnBuildRouteCollection = root.Q<UnityEngine.UIElements.Button>("BuildRouteCollection");
                if (btnBuildRouteCollection != null)
                    btnBuildRouteCollection.clicked += BtnbtnBuildRouteCollection_clicked;

                SetupLoadRouterRCFile(root);

                // 8. Traffic -------------------------------------------------------------------------------------------------------
                InitRouteChoiceList(root);
                InitRoadTypeList(root);

                UnityEngine.UIElements.TextField tfTxTSetMaxCapTrafSpeed = root.Q<UnityEngine.UIElements.TextField>("TxTSetMaxCapTrafSpeed");
                if (tfTxTSetMaxCapTrafSpeed != null)
                    tfTxTSetMaxCapTrafSpeed.RegisterValueChangedCallback((evt) =>
                    {
                        //UnityEngine.Debug.Log($"MapZoomLevel has changed to {evt.newValue}.");
                        int value;
                        int.TryParse(evt.newValue, out value);
                        if (value < 1 || value > 30)    // Set the stall speed range to be [1,30]. To be confirmed later.
                        {
                            tfTxTSetMaxCapTrafSpeed.SetValueWithoutNotify(WUInity.INPUT.Traffic.stallSpeed.ToString()); 
                            WUInity.LOG(WUInity.LogType.Warning, "The vehicle speed at max roadway capacity is not valid. Please choose between 1 and 30 (km/h).");
                        }
                        else
                            WUInity.INPUT.Traffic.stallSpeed = value;
                    });

                // 9. Landscape data ------------------------------------------------------------------------------------------------

                // 10. Fire characteriestics ----------------------------------------------------------------------------------------
                SetupLoadLCPFile(root);
                SetupLoadFuelModelFile(root);
                SetupLoadFuelMoistureFile(root);
                SetupLoadWeatherFile(root);
                SetupLoadWindFile(root);
                SetupLoadIgnitionPointsFile(root);
                SetupLoadGraphicalFireInputFile(root);

                SetupVewFireFileButtons(root);

                // 11. Check external fire references

                // 12. Running simulation settings
                SetupSimulationParameters(root);
                RegisterSimulationToggles(root);

                // 13. Execute simulation
                // Activate "Run simulation" callback for button-click event
                SetupRunSimulationTask(root);

                UnityEngine.UIElements.Button switchGUIButton = root.Q<UnityEngine.UIElements.Button>("SwitchGUI");
                switchGUIButton.clicked += () => BtnSwitchGUIButton_clicked(100);

                // Add the handler to the quit button
                //UnityEngine.UIElements.Button quitButton = root.Q<UnityEngine.UIElements.Button>("QuitButton");
                //quitButton.clicked += () => Application.Quit();

                // System logs windows controls
                UnityEngine.UIElements.Button clearLogsButton = root.Q<UnityEngine.UIElements.Button>("ClearLogsButton");
                clearLogsButton.clicked += BtnclearLogsButton_clicked;

                UnityEngine.UIElements.Button saveLogsButton = root.Q<UnityEngine.UIElements.Button>("SaveLogsButton");
                saveLogsButton.clicked += BtnSaveLogsButton_clicked;

                UnityEngine.UIElements.Scroller scLogScroll = root.Q<UnityEngine.UIElements.Scroller>("LogScroll");
                scLogScroll.valueChanged += OnSyslogScrollerValueChanged;
                scLogScroll.value = 0;

                // SaveTo/LoadFrom JSON test
                /*-----------------
                WorkflowSettings workflow;
                workflow = new WorkflowSettings();

                UnityEngine.Debug.Log($"Starting WokflowSettings class");
                bool bSaved = workflow.SaveTo("C:\\Temp\\test-save.json");
                UnityEngine.Debug.Log($"JSON saved OK? {bSaved}:");
                bool bLoaded = workflow.LoadFrom("C:\\Temp\\test-load.json");
                UnityEngine.Debug.Log($"JSON loaded OK? {bLoaded}:");
                -----------------*/
            }
        }

        private void OnSyslogScrollerValueChanged(float value)
        {
            UnityEngine.UIElements.VisualElement systemLogBox = Document.rootVisualElement.Q<UnityEngine.UIElements.VisualElement>("SystemLogBox");

            if (systemLogBox.visible)
            {
                List<string> log = WUInity.GetLog();

                int roll = (int)value, loopN;

                if (log.Count <= _iLogDisplayNum)
                {
                   roll = 0; loopN = log.Count;
                }
                else loopN = _iLogDisplayNum;

                Label togLabel = Document.rootVisualElement.Q<Label>("SysLogsText");
                if (togLabel != null)
                {
                    togLabel.text = "";

                    for (int i = 0; i < loopN; i++)
                    {
                        if (log[i + roll].Length < 150)
                            togLabel.text += log[i + roll] + "\n\r";
                        else
                            togLabel.text += log[i + roll].Substring(0, 150) + "\n\r";
                    }
                }
            }
        }

        string GetProjectPath()
        {
            if (WUInity.DATA_STATUS.HaveInput)
                return WUInity.WORKING_FOLDER;
            else
                return WUInity.DATA_FOLDER;
        }

        private void InitFoldoutSwitch(VisualElement root)
        {
            UnityEngine.UIElements.Button btnFoldoutSwitchButton = root.Q<UnityEngine.UIElements.Button>("FoldoutSwitchButton");
            if (btnFoldoutSwitchButton != null)
                btnFoldoutSwitchButton.clicked += BtnFoldoutSwitchButton_clicked;

            UnityEngine.UIElements.Button btnLogSwitchButton = root.Q<UnityEngine.UIElements.Button>("LogSwitchButton");
            if (btnLogSwitchButton != null)
                btnLogSwitchButton.clicked += BtnbtnLogSwitchButton_clicked;
        }

        private void BtnbtnLogSwitchButton_clicked()
        {
            var root = Document.rootVisualElement;
            UnityEngine.UIElements.VisualElement systemLogBox = root.Q<UnityEngine.UIElements.VisualElement>("SystemLogBox");
            systemLogBox.visible = !systemLogBox.visible;
        }

        private void BtnFoldoutSwitchButton_clicked()
        {
            _bFoldout = !_bFoldout;
            var foldouts = Document.rootVisualElement.Query<Foldout>();
            foldouts.ForEach(SwitchFoldouts);

            UnityEngine.UIElements.Button btnFoldoutSwitchButton = Document.rootVisualElement.Q<UnityEngine.UIElements.Button>("FoldoutSwitchButton");
            if (btnFoldoutSwitchButton != null)
            {
                if (_bFoldout)
                    //btnFoldoutSwitchButton.text = "<<"; 
                    btnFoldoutSwitchButton.text = "\u25B7/\u25BC";   //?/?
                    //btnFoldoutSwitchButton.text = System.Convert.ToChar("\u25B7")+"/"+ System.Convert.ToChar("\u25BC");
                    //btnFoldoutSwitchButton.text = Regex.Unescape("\u25B7/\u25BC");
                else
                    //btnFoldoutSwitchButton.text = ">>"; 
                    btnFoldoutSwitchButton.text = "\u25B6/\u25BD";             //?/?
                    //btnFoldoutSwitchButton.text = System.Convert.ToChar("\u25B6") + "/" + System.Convert.ToChar("\u25BD");
                    //btnFoldoutSwitchButton.text = Regex.Unescape("\u25B6/\u25BD");
            }
        }

        private void SwitchFoldouts(Foldout foldout)
        {
            foldout.value = _bFoldout;
        }

        private void BtnSwitchGUIButton_clicked(int clicknumber)
        {
            UnityEngine.Debug.Log($"Switch GUI = {clicknumber}");

            var root = Document.rootVisualElement;
            UnityEngine.UIElements.VisualElement mainUIBox = root.Q<UnityEngine.UIElements.VisualElement>("MainUIBox");
            UnityEngine.UIElements.VisualElement systemLogBox = root.Q<UnityEngine.UIElements.VisualElement>("SystemLogBox");     

            if (mainUIBox.style.left == 0) {
                mainUIBox.style.left = Screen.width - _iMainBoxWidth;
                mainUIBox.style.top = 20;
                mainUIBox.style.height = Screen.height - 20 - 160;     // 20 is the hight of data examine bar, 160 is system logs window height
                WUInity.GUI.enabled = true;
                systemLogBox.visible = false;
            }
            else {
                mainUIBox.style.left = 0;
                //mainUIBox.style.top = 0;
                //mainUIBox.style.height = Screen.height;

                mainUIBox.style.top = _titleBarHeight + 1;
                mainUIBox.style.height = Screen.height - _titleBarHeight - 1;

                WUInity.GUI.enabled = false;
                systemLogBox.visible = true;
            }
            //newGUI.SetEnabled(false);
        }

        private void BtnclearLogsButton_clicked()
        {
            WUInity.GetLog().Clear();
            Label togLabel = Document.rootVisualElement.Q<Label>("SysLogsText");
            if (togLabel != null) togLabel.text = ""; 
        }

        private void BtnSaveLogsButton_clicked()
        {
            FileBrowser.SetFilters(false, fileFilter[(int)FileType.syslogFile]);
            FileBrowser.ShowSaveDialog(SaveSysLogs, null, FileBrowser.PickMode.Files, false, GetProjectPath(), "SysLogs.log", "Save log file", "Save");
        }

        void SaveSysLogs(string[] paths)
        {
            DateTime localDate = DateTime.Now;
            var culture = new CultureInfo("en-GB");

            string logText= "Sys logs saved data and time: " + localDate.ToString(culture)+", "+ localDate.Kind + "\n\r"; 

            foreach (string logItem in WUInity.GetLog())
                logText += (logItem + "\n");

            System.IO.File.WriteAllText(paths[0], logText);
        }

        private void SetupPopulationControlButtons(VisualElement root)
        {
            UnityEngine.UIElements.Button btnBuildLocalGPW = root.Q<UnityEngine.UIElements.Button>("BuildLocalGPW");
            if (btnBuildLocalGPW != null)
                btnBuildLocalGPW.clicked += BtnBuildLocalGPW_clicked;

            UnityEngine.UIElements.Button btnShowHideLocalGPW = root.Q<UnityEngine.UIElements.Button>("ShowHideLocalGPW");
            if (btnShowHideLocalGPW != null)
                btnShowHideLocalGPW.clicked += BtnShowHideLocalGPW_clicked;

            UnityEngine.UIElements.Button btnShowHideLocalPOP = root.Q<UnityEngine.UIElements.Button>("ShowHideLocalPOP");
            if (btnShowHideLocalPOP != null)
                btnShowHideLocalPOP.clicked += BtnShowHideLocalPOP_clicked;

            UnityEngine.UIElements.Button btnCorrectPop = root.Q<UnityEngine.UIElements.Button>("CorrectPopButton");
            if (btnCorrectPop != null)
                btnCorrectPop.clicked += BtnCorrectPop_clicked;

            UnityEngine.UIElements.Button btnRescalePop = root.Q<UnityEngine.UIElements.Button>("RescalePopButton");
            if (btnRescalePop != null)
                btnRescalePop.clicked += BtnRescalePop_clicked;

            UnityEngine.UIElements.TextField tfTxTRescalePopNumber = root.Q<UnityEngine.UIElements.TextField>("TxTRescalePopNumber");
            if (tfTxTRescalePopNumber != null)
                tfTxTRescalePopNumber.RegisterValueChangedCallback((evt) =>
                {
                    UnityEngine.Debug.Log($"RescalePopNumber has changed to {evt.newValue}.");
                    _rescalePop = evt.newValue;
                });
        }

        private void SetupAddRemoveEvacGoalButtons(VisualElement root)
        {
            UnityEngine.UIElements.Button btnAddGoalButton = root.Q<UnityEngine.UIElements.Button>("AddGoalButton");
            if (btnAddGoalButton != null)
                btnAddGoalButton.clicked += BtnAddGoalButton_clicked; 

            UnityEngine.UIElements.Button btnRemoveGoalButton = root.Q<UnityEngine.UIElements.Button>("RemoveGoalButton");
            if (btnRemoveGoalButton != null)
                btnRemoveGoalButton.clicked += BtnRemoveGoalButton_clicked;

            UnityEngine.UIElements.Button btnNewGoalButton = root.Q<UnityEngine.UIElements.Button>("NewGoalButton");
            if (btnNewGoalButton != null)
                btnNewGoalButton.clicked += BtnNewGoalButton_clicked;

            UnityEngine.UIElements.Button btnEditGoalButton = root.Q<UnityEngine.UIElements.Button>("EditGoalButton");
            if (btnEditGoalButton != null)
                btnEditGoalButton.clicked += BtnEditGoalButton_clicked;
        }

        private void BtnEditGoalButton_clicked()
        {
            UnityEngine.UIElements.DropdownField dfDfEvacutionDestination = Document.rootVisualElement.Q<UnityEngine.UIElements.DropdownField>("DfEvacutionDestination");

            if (dfDfEvacutionDestination != null && WUInity.DATA_STATUS.HaveInput && WUInity.RUNTIME_DATA.Evacuation.EvacuationGoals.Count > 0)
            {
                string initialPath = Path.Combine(GetProjectPath(), WUInity.INPUT.Traffic.evacuationGoalFiles[dfDfEvacutionDestination.index] + ".ed");

                System.Diagnostics.Process.Start("Notepad.exe", initialPath);

                //string message= string.Concat("Goal file [", Path.GetFileName(initialPath), "] is opened in Notepad.");
                //EditorUtility.DisplayDialog(message, "Please remember to reload this goal file if you make and save any changes to the file in Notepad.", "Close");

                WUInity.LOG(WUInity.LogType.Log, "Edit goal file: " + Path.GetFileName(initialPath));
            }            
            else
            {
                //EditorUtility.DisplayDialog("No goal file is found", "Please create a new goal file and then load in Notepad.", "Close");

                WUInity.LOG(WUInity.LogType.Error, "No goal file is found! Please create a new goal file.");
            }
        }

        private void BtnNewEvacGroupButton_clicked()
        {
            // Add code here to create a new evacaution group file
            FileBrowser.SetFilters(false, fileFilter[(int)FileType.evacuationGroupFile]);
            FileBrowser.ShowSaveDialog(SaveNewEvacGroup, null, FileBrowser.PickMode.Files, false, GetProjectPath(), "groupX.eg", "Create a new evacuation group file", "Save");
        }

        void SaveNewEvacGroup(string[] paths)
        {
            string templateText = "Name: \"Add new evacuation group name here\"\n";
            templateText += "Response curves: \"Add the name(s) of response curve(s) here and the associalted cumulated probabilities below, separeted by comma\"\n";
            templateText += "Response curve probabilites:\n";
            templateText += "Destinations: \"Add evacaution goal(s) here and the associalted cumulated probabilities below, separeted by comma\"\n";
            templateText += "Destination probabilites: \n";
            templateText += "Color: 1.0, 1.0, 0.0";

            System.IO.File.WriteAllText(paths[0], templateText);
            System.Diagnostics.Process.Start("Notepad.exe", paths[0]);
        }

        private void BtnNewRespCurveButton_clicked()
        {
            // Add code here to create a new response curve file
            FileBrowser.SetFilters(false, fileFilter[(int)FileType.responseCureveFile]);
            FileBrowser.ShowSaveDialog(SaveNewEvacCurve, null, FileBrowser.PickMode.Files, false, GetProjectPath(), "responseX.rsp", "Create a new response curve file", "Save");
        }

        void SaveNewEvacCurve(string[] paths)
        {
            string templateText = "Time, Cumulative probability\n0, 0\n100, 1.0";

            System.IO.File.WriteAllText(paths[0], templateText);
            System.Diagnostics.Process.Start("Notepad.exe", paths[0]);
        }

        private void BtnNewGoalButton_clicked()
        {
            // Add code here to create a new goal
            FileBrowser.SetFilters(false, fileFilter[(int)FileType.evacuationGoalFile]);
            FileBrowser.ShowSaveDialog(SaveNewGoal, null, FileBrowser.PickMode.Files, false, GetProjectPath(), "goalX.ed", "Create a new evacuation goal file", "Save");
        }

        void SaveNewGoal(string[] paths)
        {
            string templateText = "Name: \"Add new goal name here\"\n";
            templateText += "Latitude:\nLongitude:\n";
            templateText += "Goal type: \"Exit\"\n";
            templateText += "Max flow: 3600.0\n";
            templateText += "Car capacity: -1\n";
            templateText += "People capacity: -1\n";
            templateText += "Initially blocked: \"false\"\n";
            templateText += "Color: 1.0, 0.0, 0.0";

            System.IO.File.WriteAllText(paths[0], templateText);
            System.Diagnostics.Process.Start("Notepad.exe", paths[0]);
        }

        private void BtnRemoveGoalButton_clicked()
        {
            // EditorUtility.DisplayDialog only works in editor mode. I need to remove the function later. 
            //if (EditorUtility.DisplayDialog("Remove current goal", "Do you want to remove the current goal?", "Confirm","Cancel")) 

            UnityEngine.UIElements.DropdownField dfDfEvacutionDestination = Document.rootVisualElement.Q<UnityEngine.UIElements.DropdownField>("DfEvacutionDestination");
            if (dfDfEvacutionDestination != null && WUInity.INPUT.Traffic.evacuationGoalFiles != null)
            {
                if (WUInity.RUNTIME_DATA.Evacuation.EvacuationGoals.Count > 0)
                {
                    string removeGoal = WUInity.INPUT.Traffic.evacuationGoalFiles[dfDfEvacutionDestination.index];
                    WUInity.LOG(WUInity.LogType.Log, "Goal file " + removeGoal + " is removed.");

                    if (WUInity.RUNTIME_DATA.Evacuation.EvacuationGoals.Count > 1)
                    {
                        string[] newGoalList = new string[WUInity.INPUT.Traffic.evacuationGoalFiles.Length - 1];

                        for (int i = 0; i < dfDfEvacutionDestination.index; i++)
                        {
                            newGoalList[i] = WUInity.INPUT.Traffic.evacuationGoalFiles[i];
                        }

                        for (int i = dfDfEvacutionDestination.index; i < WUInity.INPUT.Traffic.evacuationGoalFiles.Length - 1; i++)
                        {
                            newGoalList[i] = WUInity.INPUT.Traffic.evacuationGoalFiles[i + 1];
                        }

                        WUInity.INPUT.Traffic.evacuationGoalFiles = newGoalList;
                    }
                    else
                    {
                        WUInity.INPUT.Traffic.evacuationGoalFiles = null;
                    }

                    WUInity.RUNTIME_DATA.Evacuation.EvacuationGoals.RemoveAt(dfDfEvacutionDestination.index);
                    WUInityInput.SaveInput();

                    dfDfEvacutionDestination.choices.RemoveAt(dfDfEvacutionDestination.index);

                    if (dfDfEvacutionDestination.choices.Count > 0) dfDfEvacutionDestination.index = 0;
                    else dfDfEvacutionDestination.index = -1;
                }
            }
            else
            {
                WUInity.LOG(WUInity.LogType.Error, "Goal file list is empty.");
            }
        }

        private void BtnAddGoalButton_clicked()
        {
            if (WUInity.RUNTIME_DATA.Evacuation.EvacuationGoals != null) // Test to see if a project is opened. I need to find a better way!
            {
                FileBrowser.SetFilters(false, fileFilter[(int)FileType.evacuationGoalFile]);
                FileBrowser.ShowLoadDialog(LoadAEvacGoalFile, null, FileBrowser.PickMode.Files, false, GetProjectPath(), null, "Load evacuation goal file (.ed)", "Load");
            }
            else
            {
                WUInity.LOG(WUInity.LogType.Error, "Please create a new project or load an existing project before adding a goal file.");
            }
        }

        void LoadAEvacGoalFile(string[] paths)
        {
            UnityEngine.Debug.Log($"Load Goal file = {paths[0]}");

            string path = paths[0];

            // Code from EvacuationGoal.cs to load a single goal ----------------------------------------------------------------------------------
            bool fileExists = File.Exists(path);
            if (fileExists)
            {
                string[] dataLines = File.ReadAllLines(path);

                string name, exitType, blocked;
                double lati, longi;
                float maxFlow, r, g, b;
                int maxCars, maxPeople;
                bool initiallyBlocked;
                EvacGoalType evacGoalType;
                UnityEngine.Color color = Color.white;

                //name
                string[] data = dataLines[0].Split(':');
                name = data[1].Trim();
                name = name.Trim('"');

                //lat, long
                data = dataLines[1].Split(':');
                double.TryParse(data[1], out lati);

                data = dataLines[2].Split(':');
                double.TryParse(data[1], out longi);

                //goal type
                data = dataLines[3].Split(':');
                exitType = data[1].Trim();
                exitType = exitType.Trim('"');
                if (exitType == "Refugee")
                {
                    evacGoalType = EvacGoalType.Refugee;
                }
                else
                {
                    evacGoalType = EvacGoalType.Exit;
                }

                //max flow
                data = dataLines[4].Split(':');
                float.TryParse(data[1], out maxFlow);

                //car capacity
                data = dataLines[5].Split(':');
                int.TryParse(data[1], out maxCars);

                //max people
                data = dataLines[6].Split(':');
                int.TryParse(data[1], out maxPeople);

                //blocked initially?
                data = dataLines[7].Split(':');
                blocked = data[1].Trim();
                blocked = blocked.Trim('"');
                if (blocked == "false")
                {
                    initiallyBlocked = false;
                }
                else
                {
                    initiallyBlocked = true;
                }

                //color on marker
                data = dataLines[8].Split(':');
                data = data[1].Split(',');
                if (data.Length >= 3)
                {
                    float.TryParse(data[0], out r);
                    float.TryParse(data[1], out g);
                    float.TryParse(data[2], out b);
                    color = new Color(r, g, b);
                }

                bool findDuplicate = false;
                for (int i = 0; i < WUInity.RUNTIME_DATA.Evacuation.EvacuationGoals.Count; i++)
                {
                    if (WUInity.RUNTIME_DATA.Evacuation.EvacuationGoals[i].name == name) findDuplicate = true;
                }

                if (!findDuplicate) { 
                    EvacuationGoal eG = new EvacuationGoal(name, new Vector2D(lati, longi), color);
                    eG.goalType = evacGoalType;
                    eG.maxFlow = maxFlow;
                    eG.maxCars = maxCars;
                    eG.maxPeople = maxPeople;
                    eG.blocked = initiallyBlocked;

                    WUInity.RUNTIME_DATA.Evacuation.EvacuationGoals.Add(eG);
                    // Code from EvacuationGoal.cs ends ----------------------------------------------------------------------------------

                    //Save changes
                    string[] newGoalList = new string[WUInity.RUNTIME_DATA.Evacuation.EvacuationGoals.Count];

                    for (int i = 0; i < WUInity.RUNTIME_DATA.Evacuation.EvacuationGoals.Count - 1; i++)
                    {
                        newGoalList[i] = WUInity.INPUT.Traffic.evacuationGoalFiles[i];
                    }

                    string fileName = Path.GetFileName(path);
                    data = fileName.Split('.');

                    newGoalList[WUInity.RUNTIME_DATA.Evacuation.EvacuationGoals.Count - 1] = data[0];
                    WUInity.INPUT.Traffic.evacuationGoalFiles = newGoalList;

                    WUInityInput.SaveInput();

                    //Update dropdown 
                    UnityEngine.UIElements.DropdownField dfDfEvacutionDestination = Document.rootVisualElement.Q<UnityEngine.UIElements.DropdownField>("DfEvacutionDestination");
                    if (dfDfEvacutionDestination != null)
                    {
                        dfDfEvacutionDestination.choices.Add(name);
                        //dfDfEvacutionDestination.choices.Contains(name);

                        dfDfEvacutionDestination.index = dfDfEvacutionDestination.choices.Count - 1; // Show the most resent goal
                    }
                }
            }
        }

        private void SetupEvacGroupRespButtons(VisualElement root)
        {
            UnityEngine.UIElements.Button btnNewRespCurveButton = root.Q<UnityEngine.UIElements.Button>("NewRespCurveButton");
            if (btnNewRespCurveButton != null)
                btnNewRespCurveButton.clicked += BtnNewRespCurveButton_clicked;

            UnityEngine.UIElements.Button btnNewEvacGroupButton = root.Q<UnityEngine.UIElements.Button>("NewEvacGroupButton");
            if (btnNewEvacGroupButton != null)
                btnNewEvacGroupButton.clicked += BtnNewEvacGroupButton_clicked;

            UnityEngine.UIElements.Button btnAddEvacGroupButton = root.Q<UnityEngine.UIElements.Button>("AddEvacGroupButton");
            if (btnAddEvacGroupButton != null)
                btnAddEvacGroupButton.clicked += BtnAddEvacGroupButton_clicked;

            UnityEngine.UIElements.Button btnAddRespCurveButton = root.Q<UnityEngine.UIElements.Button>("AddRespCurveButton");
            if (btnAddRespCurveButton != null)
                btnAddRespCurveButton.clicked += BtnAddRespCurveButton_clicked;

            UnityEngine.UIElements.Button btnRemoveRespCurveButton = root.Q<UnityEngine.UIElements.Button>("RemoveRespCurveButton");
            if (btnRemoveRespCurveButton != null)
                btnRemoveRespCurveButton.clicked += BtnRemoveRespCurveButton_clicked;

            UnityEngine.UIElements.Button btnRemoveEvacGroupButton = root.Q<UnityEngine.UIElements.Button>("RemoveEvacGroupButton");
            if (btnRemoveEvacGroupButton != null)
                btnRemoveEvacGroupButton.clicked += BtnRemoveEvacGroupButton_clicked;

            UnityEngine.UIElements.Button btnEditRespCurveButton = root.Q<UnityEngine.UIElements.Button>("EditRespCurveButton");
            if (btnEditRespCurveButton != null)
                btnEditRespCurveButton.clicked += BtnEditRespCurveButton;

            UnityEngine.UIElements.Button btnEditEvacGroupButton = root.Q<UnityEngine.UIElements.Button>("EditEvacGroupButton");
            if (btnEditEvacGroupButton != null)
                btnEditEvacGroupButton.clicked += BtnEditEvacGroupButton;

            UnityEngine.UIElements.Button btnEditEvacGroupOnMap = root.Q<UnityEngine.UIElements.Button>("EditEvacGroupOnMapButton");
            if (btnEditEvacGroupOnMap != null)
                btnEditEvacGroupOnMap.clicked += BtnEditEvacGroupOnMap_clicked;
        }

        private void BtnRemoveRespCurveButton_clicked()
        {
            WUInity.LOG(WUInity.LogType.Warning, "To be implemented soon. Currently, please edit the project .WUI file to make any change to the response curve file list.");
            /*
            if (EditorUtility.DisplayDialog("Remove current response curve", "Do you want to remove the current response curve?", "Confirm", "Cancel"))
            {

                UnityEngine.UIElements.DropdownField dfResponseCurve = Document.rootVisualElement.Q<UnityEngine.UIElements.DropdownField>("DfResponseCurve");
                if (dfResponseCurve != null && dfResponseCurve.choices.Count >= 1)
                {
                    WUInity.RUNTIME_DATA.Evacuation.ResponseCurves.

                    if (WUInity.RUNTIME_DATA.Evacuation.ResponseCurves.Length > 1)
                    {
                        string[] newGoalList = new string[WUInity.RUNTIME_DATA.Evacuation.ResponseCurves.Length - 1];

                        for (int i = 0; i < dfResponseCurve.index; i++)
                        {
                            if (i != dfResponseCurve.index) newGoalList[i] = WUInity.RUNTIME_DATA.Evacuation.ResponseCurves[i].name;
                            else break;
                        }

                        for (int i = dfResponseCurve.index; i < WUInity.RUNTIME_DATA.Evacuation.ResponseCurves.Length - 1; i++)
                        {
                            newGoalList[i] = WUInity.RUNTIME_DATA.Evacuation.ResponseCurves[i + 1].name;
                        }

                        WUInity.INPUT.Traffic.evacuationGoalFiles = newGoalList;
                    }
                    else
                    {
                        WUInity.INPUT.Traffic.evacuationGoalFiles = null;
                    }

                    WUInity.RUNTIME_DATA.Evacuation.EvacuationGoals.RemoveAt(dfResponseCurve.index);
                    WUInityInput.SaveInput();

                    dfResponseCurve.choices.RemoveAt(dfResponseCurve.index);

                    if (dfResponseCurve.choices.Count > 0) dfResponseCurve.index = 0;
                    else dfResponseCurve.index = -1;
                }
            }
            */
        }
        private void BtnRemoveEvacGroupButton_clicked()
        {
            WUInity.LOG(WUInity.LogType.Warning, "To be implemented soon. Currently, please edit the project .WUI file to make any change to the evacuation group file list.");
        }

        private void BtnAddEvacGroupButton_clicked()
        {
            if (WUInity.INPUT.Evacuation.EvacGroupFiles != null ) {
                FileBrowser.SetFilters(false, fileFilter[(int)FileType.evacuationGroupFile]);
                FileBrowser.ShowLoadDialog(LoadAEvacGroupFile, null, FileBrowser.PickMode.Files, false, GetProjectPath(), null, "Load evacuation group file (.eg)", "Load");
            }
            else
            {
                WUInity.LOG(WUInity.LogType.Error, "Please create a new project or load an existing project before adding a evacuation group file.");
            }
        }

        private void LoadAEvacGroupFile(string[] paths)
        {
            string path = paths[0];

            if (File.Exists(path))
            {
                string[] dataLines = File.ReadAllLines(path);
                
                if (dataLines.Length >= 6)  //The file must have 6 lines to be valid.
                {
                    string name;
                    List<string> responseCurveNames = new List<string>(), destinationNames = new List<string>();
                    List<float> responseCurveProbabilities = new List<float>();
                    List<double> goalProbabilities = new List<double>();
                    float r, g, b;
                    Color color = Color.white;

                    //get name
                    string[] data = dataLines[0].Split(':');
                    data[1].Trim('"');
                    name = data[1].Trim(' ');

                    //response curve names
                    data = dataLines[1].Split(':');
                    data = data[1].Split(',');
                    for (int j = 0; j < data.Length; j++)
                    {
                        string value = data[j].Trim();
                        value = value.Trim('"');
                        responseCurveNames.Add(value);
                    }

                    //response curve probabilities
                    data = dataLines[2].Split(':');
                    data = data[1].Split(',');
                    for (int j = 0; j < data.Length; j++)
                    {
                        float value;
                        bool b1 = float.TryParse(data[j], out value);
                        if (b1)
                        {
                            responseCurveProbabilities.Add(value);
                        }
                    }

                    //goal names
                    data = dataLines[3].Split(':');
                    data = data[1].Split(',');
                    for (int j = 0; j < data.Length; j++)
                    {
                        string value = data[j].Trim();
                        value = value.Trim('"');
                        destinationNames.Add(value);
                    }

                    //goal probabilities
                    data = dataLines[4].Split(':');
                    data = data[1].Split(',');
                    for (int j = 0; j < data.Length; j++)
                    {
                        float value;
                        bool b1 = float.TryParse(data[j], out value);
                        if (b1)
                        {
                            goalProbabilities.Add(value);
                        }
                    }

                    //colors
                    data = dataLines[5].Split(':');
                    data = data[1].Split(',');
                    if (data.Length >= 3)
                    {
                        float.TryParse(data[0], out r);
                        float.TryParse(data[1], out g);
                        float.TryParse(data[2], out b);
                        color = new Color(r, g, b);
                    }

                    int[] goalIndices = new int[destinationNames.Count];
                    for (int j = 0; j < destinationNames.Count; j++)
                    {
                        goalIndices[j] = WUInity.RUNTIME_DATA.Evacuation.GetEvacGoalIndexFromName(destinationNames[j]);
                    }

                    int[] responseCurveIndices = new int[responseCurveNames.Count];
                    for (int j = 0; j < responseCurveNames.Count; j++)
                    {
                        responseCurveIndices[j] = WUInity.RUNTIME_DATA.Evacuation.GetResponseCurveIndexFromName(responseCurveNames[j]);
                    }

                    //TODO: check if input count and probabilities match

                    EvacGroup eG = new EvacGroup(name, goalIndices, goalProbabilities.ToArray(), responseCurveIndices, color);

                    // The code above is just to check if the group file contains valid data
                    //-----------------------------------------------------------------------------------------------------------------

                    List<String> evacGroupFiles = new List<String>();

                    for (int i = 0; i < WUInity.INPUT.Evacuation.EvacGroupFiles.Length; i++)
                    {
                        evacGroupFiles.Add(WUInity.INPUT.Evacuation.EvacGroupFiles[i]);
                    }

                    string fileName = Path.GetFileName(path);
                    data = fileName.Split('.');

                    if (!evacGroupFiles.Contains(data[0])) // Check if the evacuation group file has been added already.
                    {
                        evacGroupFiles.Add(data[0]);
                        WUInity.INPUT.Evacuation.EvacGroupFiles = evacGroupFiles.ToArray();

                        WUInity.RUNTIME_DATA.Evacuation.LoadEvacuationGroups(); // Reload all evacuation groups based on updated file list.
                        WUInity.RUNTIME_DATA.Evacuation.LoadEvacGroupIndices();

                        WUInity.LOG(WUInity.LogType.Log, "Loaded evacuation group from " + path + " named " + data[0]);

                        WUInityInput.SaveInput();

                        //Update dropdown 
                        UnityEngine.UIElements.DropdownField dfEvacuationGroup = Document.rootVisualElement.Q<UnityEngine.UIElements.DropdownField>("DfEvacuationGroup");
                        if (dfEvacuationGroup != null)
                        {
                            dfEvacuationGroup.choices.Add(name);
                            dfEvacuationGroup.index = dfEvacuationGroup.choices.Count - 1; // Show the most resent added response curve
                        }
                    }
                }
            }
        }

        private void BtnAddRespCurveButton_clicked()
        {
            if (WUInity.INPUT.Evacuation.ResponseCurveFiles != null) {
                FileBrowser.SetFilters(false, fileFilter[(int)FileType.responseCureveFile]);
                FileBrowser.ShowLoadDialog(LoadAResponseCurveFile, null, FileBrowser.PickMode.Files, false, GetProjectPath(), null, "Load response curve file (.rsp)", "Load");
            }
            else
            {
                WUInity.LOG(WUInity.LogType.Error, "Please create a new project or load an existing project before adding a response curve file.");
            }
        }

        private void LoadAResponseCurveFile(string[] paths)
        {
            string path = paths[0];

            if (File.Exists(path))
            {
                string[] dataLines = File.ReadAllLines(path);
                List<ResponseDataPoint> dataPoints = new List<ResponseDataPoint>();
                //skip first line (header)
                for (int j = 1; j < dataLines.Length; j++)
                {
                    string[] data = dataLines[j].Split(',');

                    if (data.Length >= 2)
                    {
                        float time, probability;

                        bool timeRead = float.TryParse(data[0], out time);
                        bool probabilityRead = float.TryParse(data[1], out probability);
                        if (timeRead && probabilityRead)
                        {
                            ResponseDataPoint dataPoint = new ResponseDataPoint(time, probability);
                            dataPoints.Add(dataPoint);
                        }
                    }
                }

                // need at least two to make a curve.
                // PS, read the contents of the response curve file just to validate it, so that it can be added into the array.
                if (dataPoints.Count >= 2) // A valid response curve file
                {
                    //List<ResponseCurve> responseCurves = new List<ResponseCurve>();
                    List<String> responseCurveFiles= new List<String>();

                    for (int i = 0; i < WUInity.INPUT.Evacuation.ResponseCurveFiles.Length; i++)
                    {
                        responseCurveFiles.Add(WUInity.INPUT.Evacuation.ResponseCurveFiles[i]);
                        //responseCurves.Add(WUInity.RUNTIME_DATA.Evacuation.ResponseCurves[i]);
                    }

                    string fileName = Path.GetFileName(path);
                    string[] data = fileName.Split('.');

                    if (!responseCurveFiles.Contains(data[0])) // Check if the curve file has been already added
                    {
                        responseCurveFiles.Add(data[0]);
                        WUInity.INPUT.Evacuation.ResponseCurveFiles = responseCurveFiles.ToArray();

                        // ResponseCurves could be simply updated by the following two lines, but I have to reload all curves using LoadResponseCurves();
                        //responseCurves.Add(new ResponseCurve(dataPoints, data[0]));
                        //WUInity.RUNTIME_DATA.Evacuation.ResponseCurves = responseCurves.ToArray();

                        WUInity.RUNTIME_DATA.Evacuation.LoadResponseCurves(); // Reload all response curves based on updated file list.

                        WUInity.LOG(WUInity.LogType.Log, " Loaded response curve from " + path + " named " + data[0]);

                        WUInityInput.SaveInput();

                        //Update dropdown 
                        UnityEngine.UIElements.DropdownField dfResponseCurve = Document.rootVisualElement.Q<UnityEngine.UIElements.DropdownField>("DfResponseCurve");
                        if (dfResponseCurve != null)
                        {
                            dfResponseCurve.choices.Add(data[0]);
                            dfResponseCurve.index = dfResponseCurve.choices.Count - 1; // Show the most resent added response curve
                        }
                    }
                }
            }
        }

        private void SetupLoadOSMDataFile(VisualElement root)
        {
            Toggle togSetOSMFile = root.Q<Toggle>("TogSetOSMFile");

            if (togSetOSMFile != null)
                togSetOSMFile.RegisterValueChangedCallback(evt =>
                {
                    if (evt.newValue) 
                    {
                        FileBrowser.SetFilters(false, fileFilter[(int)FileType.OSMDataFile]);
                        FileBrowser.ShowLoadDialog(SetOSMDataFile, CancelSetOSMDataFile, FileBrowser.PickMode.Files, false, GetProjectPath(), null, "Specify OSM data file", "Select");
                    }
                    else
                    {   // Clear GPW folder 
                        Label togLabel = root.Q<Label>("TxtOSMFile");
                        if (togLabel != null) togLabel.text = "OSM data file: not set";

                        _OSMDataFile = ""; //clear setting.
                    }
                });
        }

        void SetOSMDataFile(string[] paths)
        {
            string loadStatus = "";
            Toggle togSetOSMFile = Document.rootVisualElement.Q<Toggle>("TogSetOSMFile");

            if (togSetOSMFile != null)
            {
                if (File.Exists(paths[0]))  // There may be additional way of testing the valid of this file.
                {
                    _OSMDataFile = paths[0];
                    loadStatus = "OSM data file: " + Path.GetFileName(paths[0]) + " is loaded successfully.";
                    togSetOSMFile.SetValueWithoutNotify(true);
                }
                else
                {
                    loadStatus = "OSM data file load error.";
                    togSetOSMFile.SetValueWithoutNotify(false);
                }
            }

            Label togLabel = Document.rootVisualElement.Q<Label>("TxtOSMFile");
            if (togLabel != null) togLabel.text = loadStatus;
        }

        void CancelSetOSMDataFile()
        {
            Toggle togSetOSMFile = Document.rootVisualElement.Q<Toggle>("TogSetOSMFile");

            if (togSetOSMFile != null)
            {
                if (File.Exists(_OSMDataFile))
                    togSetOSMFile.value = true;
                else
                    togSetOSMFile.value = false;
            }
        }

        private void SetupLoadRouterDBFile(VisualElement root)
        {
            Toggle togSetRouterDB = root.Q<Toggle>("TogSetRouterDB");

            if (togSetRouterDB != null)
                togSetRouterDB.RegisterValueChangedCallback(evt =>
                {
                    if (evt.newValue)
                    {
                        FileBrowser.SetFilters(false, fileFilter[(int)FileType.routerDBFile]);
                        FileBrowser.ShowLoadDialog(LoadRouterDBFile, CancelLoadRouterDBFile, FileBrowser.PickMode.Files, false, GetProjectPath(), null, "Load routerDB data file", "Select");
                    }
                    else
                    {   // Clear GPW folder 
                        Label togLabel = root.Q<Label>("TxtRouterDBFile");
                        if (togLabel != null) togLabel.text = "RouterDB file: not set";
                    }
                });
        }

        void LoadRouterDBFile(string[] paths)
        {
            string loadStatus = "";
            Toggle togSetRouterDB = Document.rootVisualElement.Q<Toggle>("TogSetRouterDB");

            if (togSetRouterDB != null)
            {
                if (WUInity.RUNTIME_DATA.Routing.LoadRouterDb(paths[0], true))
                {
                    loadStatus = "RouterDB file: " + Path.GetFileName(paths[0]) + " is loaded successfully.";
                    togSetRouterDB.SetValueWithoutNotify(true);
                }
                else
                {
                    loadStatus = "RouterDB file load error.";
                    togSetRouterDB.SetValueWithoutNotify(false);
                }
            }

            Label togLabel = Document.rootVisualElement.Q<Label>("TxtRouterDBFile");
            if (togLabel != null) togLabel.text = loadStatus;
        }

        void CancelLoadRouterDBFile()
        {
            SetRouterDBFile();
        }

        void SetRouterDBFile()
        { 
            var root = Document.rootVisualElement;
            Toggle togSetRouterDB = root.Q<Toggle>("TogSetRouterDB");

            if (togSetRouterDB != null)
            {
                Label txtRouterDBFile = root.Q<Label>("TxtRouterDBFile");
                if (txtRouterDBFile != null)
                {
                    string filePath;
                    if (WUInity.INPUT.Routing.routerDbFile.Length > 0)
                    {
                        filePath = "RouterDB file: " + WUInity.INPUT.Routing.routerDbFile;
                        togSetRouterDB.SetValueWithoutNotify(true);
                    }
                    else
                    {
                        filePath = "RouterDB file: not set";
                        togSetRouterDB.SetValueWithoutNotify(false);
                    }

                    txtRouterDBFile.text = filePath;
                }
            }
        }

        private void SetupLoadRouterRCFile(VisualElement root)
        {
            Toggle togSetRouteRC = root.Q<Toggle>("TogSetRouteRC");

            if (togSetRouteRC != null)
                togSetRouteRC.RegisterValueChangedCallback(evt =>
                {
                    if (evt.newValue)
                    {
                        FileBrowser.SetFilters(false, fileFilter[(int)FileType.routeRCFile]);
                        FileBrowser.ShowLoadDialog(LoadRouteRCFile, CancelLoadRouteRCFile, FileBrowser.PickMode.Files, false, GetProjectPath(), null, "Load route collection file", "Select");
                    }
                    else
                    {   // Clear GPW folder 
                        Label togLabel = root.Q<Label>("TxtRCFile");
                        if (togLabel != null) togLabel.text = "Route collection file: not set";
                    }
                });
        }

        void LoadRouteRCFile(string[] paths)
        {
            string loadStatus = "";
            Toggle togSetRouteRC = Document.rootVisualElement.Q<Toggle>("TogSetRouteRC");

            if (togSetRouteRC != null)
            {
                if (WUInity.RUNTIME_DATA.Routing.LoadRouteCollection(paths[0], true))
                {
                    loadStatus = "Route collection file: " + Path.GetFileName(paths[0]) + " is loaded successfully.";
                    togSetRouteRC.SetValueWithoutNotify(true);
                }
                else
                {
                    loadStatus = "Route collection file load error.";
                    togSetRouteRC.SetValueWithoutNotify(false);
                }
            }

            Label togLabel = Document.rootVisualElement.Q<Label>("TxtRCFile");
            if (togLabel != null) togLabel.text = loadStatus;
        }

        void CancelLoadRouteRCFile()
        {
            SetRouteRCFile();
        }

        void SetRouteRCFile()
        {
            var root = Document.rootVisualElement;
            Toggle togSetRouteRC = root.Q<Toggle>("TogSetRouteRC");

            if (togSetRouteRC != null)
            {
                Label txtRCFile = root.Q<Label>("TxtRCFile");
                if (txtRCFile != null)
                {
                    string filePath;
                    if (WUInity.INPUT.Routing.routeCollectionFile.Length > 0)
                    {
                        filePath = "Route collection file: " + Path.GetFileName(WUInity.INPUT.Routing.routeCollectionFile); // There is a bug in WUInity.RUNTIME_DATA.Routing.LoadRouteCollection.
                        togSetRouteRC.SetValueWithoutNotify(true);
                    }
                    else
                    {
                        filePath = "Route collection file: not set";
                        togSetRouteRC.SetValueWithoutNotify(false);
                    }

                    txtRCFile.text = filePath;
                }
            }
        }

        private void BtnBuildRouterDB_clicked()
        {
            if (File.Exists(_OSMDataFile)) { 
                WUInity.RUNTIME_DATA.Routing.CreateRouterDatabaseFromOSM(_OSMDataFile);
                //EditorUtility.DisplayDialog("Build router database", "Router database is successfully created from OSM file.", "Close");
                WUInity.LOG(WUInity.LogType.Log, "Router database is successfully created from OSM file: "+ _OSMDataFile);
            }
            else {
                //EditorUtility.DisplayDialog("Build router database", "Could not create router database. Please set OSM data file first and also make sure the regional file and the location and size of region settings match each other.", "Close");
                WUInity.LOG(WUInity.LogType.Error, "Could not create router database. Please set OSM data file first and also make sure the regional file and the location and size of region settings match each other.");
            }
        }

        private void BtnbtnBuildRouteCollection_clicked()
        {
            if (WUInity.DATA_STATUS.RouterDbLoaded)
            {
                WUInity.RUNTIME_DATA.Routing.BuildAndSaveRouteCollection();
            }
        }

        private void BtnViewFireFile(FileType fileType)
        {
            string fileName="";

            switch(fileType)
            {
                case FileType.fuelModelsFile:
                    fileName = WUInity.INPUT.Fire.fuelModelsFile;            break;
                case FileType.initialFuelMoistureFile:
                    fileName = WUInity.INPUT.Fire.initialFuelMoistureFile;   break;
                case FileType.weatherFile:
                    fileName = WUInity.INPUT.Fire.weatherFile;               break;
                case FileType.windFile:
                    fileName = WUInity.INPUT.Fire.windFile;                  break;
                case FileType.ignitionPointsFile:
                    fileName = WUInity.INPUT.Fire.ignitionPointsFile;        break;
            }

            if (WUInity.DATA_STATUS.HaveInput && fileName.Length > 0)
            {
                string fullPath = Path.Combine(GetProjectPath(), fileName);
                System.Diagnostics.Process.Start("Notepad.exe", fullPath);

                //string message = "Fire characteristics file [" + fileName + "] is opened in Notepad.";
                //EditorUtility.DisplayDialog(message, "Please remember to reload this file into WUINITY if you make any changes to it in Notepad.", "Close");
                WUInity.LOG(WUInity.LogType.Log, "Open fire characteristics file: "+ fileName);
            }
            else
            {
                //EditorUtility.DisplayDialog("No fire characteristics file is found", "Please create a new fire characteristics file and then open in Notepad.", "Close");
                WUInity.LOG(WUInity.LogType.Error, "The fire characteristics file hasn't been specified.");
            }
        }

        private void BtnEditRespCurveButton()
        {
            UnityEngine.UIElements.DropdownField dfDfResponseCurve = Document.rootVisualElement.Q<UnityEngine.UIElements.DropdownField>("DfResponseCurve");

            if (dfDfResponseCurve != null && WUInity.DATA_STATUS.HaveInput && WUInity.RUNTIME_DATA.Evacuation.ResponseCurves.Length > 0)
            {
                string initialPath = Path.Combine(WUInity.WORKING_FOLDER, WUInity.RUNTIME_DATA.Evacuation.ResponseCurves[dfDfResponseCurve.index].name + ".rsp");

                System.Diagnostics.Process.Start("Notepad.exe", initialPath);

                //string message = string.Concat("Response curve file [", Path.GetFileName(initialPath), "] is opened in Notepad.");
                //EditorUtility.DisplayDialog(message, "Please remember to reload this response curve file if you make and save any changes to the file in Notepad.", "Close");
                WUInity.LOG(WUInity.LogType.Log, "Open response curve file: " + Path.GetFileName(initialPath));
            }
            else
            {
                //EditorUtility.DisplayDialog("No response curve file is found", "Please create a new response curve file and then load in Notepad.", "Close");
                WUInity.LOG(WUInity.LogType.Error, "The response curve file hasn't been specified.");
            }
        }

        private void BtnEditEvacGroupButton()
        {
            UnityEngine.UIElements.DropdownField dfDfEvacuationGroup = Document.rootVisualElement.Q<UnityEngine.UIElements.DropdownField>("DfEvacuationGroup");

            if (dfDfEvacuationGroup != null && WUInity.DATA_STATUS.HaveInput && WUInity.RUNTIME_DATA.Evacuation.EvacuationGroups.Length > 0)
            {
                string initialPath = Path.Combine(WUInity.WORKING_FOLDER, WUInity.INPUT.Evacuation.EvacGroupFiles[dfDfEvacuationGroup.index] + ".eg");

                System.Diagnostics.Process.Start("Notepad.exe", initialPath);

                //string message = string.Concat("Evacuation group file [", Path.GetFileName(initialPath), "] is opened in Notepad.");
                //EditorUtility.DisplayDialog(message, "Please remember to reload this evacuation group file if you make and save any changes to the file in Notepad.", "Close");
                WUInity.LOG(WUInity.LogType.Log, "Open evacuation group file: " + Path.GetFileName(initialPath));
            }
            else
            {
                //EditorUtility.DisplayDialog("No evacuation group file is found", "Please create a new evacuation group file and then load in Notepad.", "Close");
                WUInity.LOG(WUInity.LogType.Error, "The evacuation group file hasn't been specified.");
            }
        }

        private void BtnEditEvacGroupOnMap_clicked()
        {
            var root = Document.rootVisualElement;

            UnityEngine.UIElements.DropdownField dfDfEvacuationGroup = root.Q<UnityEngine.UIElements.DropdownField>("DfEvacuationGroup");
            UnityEngine.UIElements.Button btnEditEvacGroupOnMap = root.Q<UnityEngine.UIElements.Button>("EditEvacGroupOnMapButton");

            if (dfDfEvacuationGroup != null && WUInity.DATA_STATUS.HaveInput && WUInity.RUNTIME_DATA.Evacuation.EvacuationGroups.Length > 0)
            {
                if (!WUInity.INSTANCE.IsPainterActive())
                {
                    btnEditEvacGroupOnMap.text = "Stop editing";
                    WUInity.INSTANCE.StartPainter(Painter.PaintMode.EvacGroup);
                    WUInity.PAINTER.SetEvacGroupColor(dfDfEvacuationGroup.index);
                }
                else
                {
                    btnEditEvacGroupOnMap.text = "Edit evacuatoin group on map";
                    WUInity.INSTANCE.StopPainter();
                }
            }
        }

        private void InitEvacGoalList(VisualElement root)
        {   
            // Evacuation Destination list initialisation starts:
            UnityEngine.UIElements.DropdownField dfDfEvacutionDestination = root.Q<UnityEngine.UIElements.DropdownField>("DfEvacutionDestination");
            if (dfDfEvacutionDestination != null)
            {
                dfDfEvacutionDestination.choices.Clear();   //clear initial item if any.

                dfDfEvacutionDestination.RegisterValueChangedCallback((evt) =>
                {
                    UnityEngine.Debug.Log($"The Evacuation Destination dropdown selection has changed to {evt.newValue}.");

                    // Fields to allow user see the detailed information about evacuation destinations.

                    UnityEngine.UIElements.TextField tfTxEvacDestName = root.Q<UnityEngine.UIElements.TextField>("TxEvacDestName");
                    UnityEngine.UIElements.TextField tfTxEvacDestLatLong = root.Q<UnityEngine.UIElements.TextField>("TxEvacDestLatLong");
                    UnityEngine.UIElements.TextField tfTxEvacDestType = root.Q<UnityEngine.UIElements.TextField>("TxEvacDestType");

                    if (WUInity.RUNTIME_DATA.Evacuation.EvacuationGoals.Count > 0)
                    {
                        tfTxEvacDestName.SetValueWithoutNotify(WUInity.RUNTIME_DATA.Evacuation.EvacuationGoals[dfDfEvacutionDestination.index].name);
                        tfTxEvacDestLatLong.SetValueWithoutNotify(WUInity.RUNTIME_DATA.Evacuation.EvacuationGoals[dfDfEvacutionDestination.index].latLong.x.ToString() + ", " +
                                                                  WUInity.RUNTIME_DATA.Evacuation.EvacuationGoals[dfDfEvacutionDestination.index].latLong.y.ToString());

                        EvacGoalType evacGoalType = WUInity.RUNTIME_DATA.Evacuation.EvacuationGoals[dfDfEvacutionDestination.index].goalType;

                        if (evacGoalType == EvacGoalType.Refugee)
                            tfTxEvacDestType.SetValueWithoutNotify("Refugee");
                        else
                            tfTxEvacDestType.SetValueWithoutNotify("Exit");

                        WUInity.INSTANCE.SpawnMarkers();    // Update the goals on map.
                    }
                    else
                    {
                        tfTxEvacDestName.SetValueWithoutNotify("");
                        tfTxEvacDestLatLong.SetValueWithoutNotify("");
                        tfTxEvacDestType.SetValueWithoutNotify("");
                    }
                });
            }
            // Evacuation Destination list initialisation end.
        }

        private void InitResponseCurveList(VisualElement root)
        {
            // Response curve list initialisation starts:
            UnityEngine.UIElements.DropdownField dfDfResponseCurve = root.Q<UnityEngine.UIElements.DropdownField>("DfResponseCurve");
            if (dfDfResponseCurve != null)
            {
                dfDfResponseCurve.choices.Clear();  //clear initial item if any.

                dfDfResponseCurve.RegisterValueChangedCallback((evt) =>
                {
                    UnityEngine.Debug.Log($"The Response curve dropdown selection has changed to {evt.newValue}.");

                    // I need to add more fields to allow user see the detailed information about response curve.

                });
            }
            // Response curve list initialisation end.
        }

        private void InitEvacGroupList(VisualElement root)
        {
            // Evacuation group list initialisation starts:
            UnityEngine.UIElements.DropdownField dfDfEvacuationGroup = root.Q<UnityEngine.UIElements.DropdownField>("DfEvacuationGroup");
            if (dfDfEvacuationGroup != null)
            {
                dfDfEvacuationGroup.choices.Clear();    //clear initial item if any.

                dfDfEvacuationGroup.RegisterValueChangedCallback((evt) =>
                {
                    //UnityEngine.Debug.Log($"The Evacuation group dropdown selection has changed to {dfDfEvacuationGroup.index}, {evt.newValue}.");

                    WUInity.PAINTER.SetEvacGroupColor(dfDfEvacuationGroup.index);   //For editing evacuation group on map.

                    // I need to add more fields to allow user see the detailed information about evacuation groups.

                });
            }
            // Evacuation group list initialisation end.
        }

        private void InitRouteChoiceList(VisualElement root)
        {
            // Route choice list initialisation starts:
            UnityEngine.UIElements.DropdownField dfRouteChoice = root.Q<UnityEngine.UIElements.DropdownField>("DfRouteChoice");
            if (dfRouteChoice != null)
            {
                dfRouteChoice.RegisterValueChangedCallback((evt) =>
                {
                    WUInity.INPUT.Traffic.routeChoice = (RouteChoice) dfRouteChoice.index;
                });
            }
        }

        private void InitRoadTypeList(VisualElement root)
        {
            // Route choice list initialisation starts:
            UnityEngine.UIElements.DropdownField dfRoadType = root.Q<UnityEngine.UIElements.DropdownField>("DfRoadType");
            if (dfRoadType != null)
            {
                dfRoadType.RegisterValueChangedCallback((evt) =>
                {
                    /**
                    UnityEngine.UIElements.TextField tfTxRoadTypeName = root.Q<UnityEngine.UIElements.TextField>("TxRoadTypeName");
                    if (tfTxRoadTypeName != null) 
                        tfTxRoadTypeName.SetValueWithoutNotify(WUInity.RUNTIME_DATA.Traffic.RoadTypeData.roadData[dfRoadType.index].name);
                    ***/

                    UnityEngine.UIElements.TextField tfTxSpeedLimit = root.Q<UnityEngine.UIElements.TextField>("TxSpeedLimit");
                    if (tfTxSpeedLimit != null)
                        tfTxSpeedLimit.SetValueWithoutNotify(WUInity.RUNTIME_DATA.Traffic.RoadTypeData.roadData[dfRoadType.index].speedLimit.ToString());

                    UnityEngine.UIElements.TextField tfTxLanes = root.Q<UnityEngine.UIElements.TextField>("TxLanes");
                    if (tfTxLanes != null)
                        tfTxLanes.SetValueWithoutNotify(WUInity.RUNTIME_DATA.Traffic.RoadTypeData.roadData[dfRoadType.index].lanes.ToString());

                    UnityEngine.UIElements.TextField tfTxMaxCapacity = root.Q<UnityEngine.UIElements.TextField>("TxMaxCapacity");
                    if (tfTxMaxCapacity != null)
                        tfTxMaxCapacity.SetValueWithoutNotify(WUInity.RUNTIME_DATA.Traffic.RoadTypeData.roadData[dfRoadType.index].maxCapacity.ToString());

                    UnityEngine.UIElements.TextField tfTxCanBeReversed = root.Q<UnityEngine.UIElements.TextField>("TxCanBeReversed");
                    if (tfTxCanBeReversed != null)
                        tfTxCanBeReversed.SetValueWithoutNotify(WUInity.RUNTIME_DATA.Traffic.RoadTypeData.roadData[dfRoadType.index].canBeReversed.ToString());
             
                });
            }

            UnityEngine.UIElements.Button btnEditRoadTypeButton = root.Q<UnityEngine.UIElements.Button>("EditRoadTypeButton");
            if (btnEditRoadTypeButton != null)
                btnEditRoadTypeButton.clicked += BtnEditRoadTypeButton;
        }

        private void BtnEditRoadTypeButton()
        {
            if (WUInity.DATA_STATUS.HaveInput)
            {
                string initialPath1 = Path.Combine("default.roads");    // The file was not saved in a correct place in RoadTypeData.cs -> SaveRoadTypeData(RoadTypeData rTD)
                string initialPath2 = Path.Combine(GetProjectPath(), "default.roads");

                if (File.Exists(initialPath1))
                    System.Diagnostics.Process.Start("Notepad.exe", initialPath1);
                else if (File.Exists(initialPath2))
                    System.Diagnostics.Process.Start("Notepad.exe", initialPath2);
                else 
                    WUInity.LOG(WUInity.LogType.Error, "default.roads file is not found!");
            }
        }

        private void SetupSimulationParameters(VisualElement root)
        {
            UnityEngine.UIElements.TextField tfTxTSetSimID = root.Q<UnityEngine.UIElements.TextField>("TxTSetSimID");
            if (tfTxTSetSimID != null)
                tfTxTSetSimID.RegisterValueChangedCallback((evt) =>
                {
                    UnityEngine.Debug.Log($"TxTSetSimID has changed to {evt.newValue}.");
                    _simulationID = evt.newValue;
                });

            UnityEngine.UIElements.TextField tfTxTSetTimeStep = root.Q<UnityEngine.UIElements.TextField>("TxTSetTimeStep");
            if (tfTxTSetTimeStep != null)
                tfTxTSetTimeStep.RegisterValueChangedCallback((evt) =>
                {
                    UnityEngine.Debug.Log($"TxTSetTimeStep has changed to {evt.newValue}.");
                    _simTimeStep = evt.newValue;

                    float.TryParse(_simTimeStep, out WUInity.INPUT.Simulation.DeltaTime);             
                });

            UnityEngine.UIElements.TextField tfTxTSetMaxSimTime = root.Q<UnityEngine.UIElements.TextField>("TxTSetMaxSimTime");
            if (tfTxTSetMaxSimTime != null)
                tfTxTSetMaxSimTime.RegisterValueChangedCallback((evt) =>
                {
                    UnityEngine.Debug.Log($"TxTSetMaxSimTime has changed to {evt.newValue}.");
                    _maxSimTime = evt.newValue;

                    float.TryParse(_maxSimTime, out WUInity.INPUT.Simulation.MaxSimTime);
                });

        }
        private void RegisterSimulationToggles(VisualElement root) 
        {
            // Register simulation settings begins
            UnityEngine.UIElements.TextField tfTxTSetNumSims = root.Q<UnityEngine.UIElements.TextField>("TxTSetNumSims");
            if (tfTxTSetNumSims != null)
            {
                tfTxTSetNumSims.AddToClassList("hide");

                Toggle togTogMultipleSim = root.Q<Toggle>("TogMultipleSim");
                if (togTogMultipleSim != null)
                    togTogMultipleSim.RegisterValueChangedCallback(evt =>
                    {
                        WUInity.RUNTIME_DATA.Simulation.MultipleSimulations = evt.newValue;
                        tfTxTSetNumSims.ToggleInClassList("hide");

                        UnityEngine.Debug.Log($"TogMultipleSim = {evt.newValue}");
                    });
            }

            Toggle togTogSimPeds = root.Q<Toggle>("TogSimPeds");
            if (togTogSimPeds != null)
                togTogSimPeds.RegisterValueChangedCallback(evt =>
                {
                    WUInity.INPUT.Simulation.RunEvacSim = evt.newValue;
                    UnityEngine.Debug.Log($"TogSimPeds = {evt.newValue}");
                });

            Toggle togTogSimTraf = root.Q<Toggle>("TogSimTraf");
            if (togTogSimTraf != null)
                togTogSimTraf.RegisterValueChangedCallback(evt =>
                {
                    WUInity.INPUT.Simulation.RunTrafficSim = evt.newValue;
                    UnityEngine.Debug.Log($"TogSimTraf = {evt.newValue}");
                });

            Toggle togTogSimFire = root.Q<Toggle>("TogSimFire");
            if (togTogSimFire != null)
                togTogSimFire.RegisterValueChangedCallback(evt =>
                {
                    WUInity.INPUT.Simulation.RunFireSim = evt.newValue;
                    UnityEngine.Debug.Log($"TogSimFire = {evt.newValue}");
                });

            Toggle togTogSimSmoke = root.Q<Toggle>("TogSimSmoke");
            if (togTogSimSmoke != null)
                togTogSimSmoke.RegisterValueChangedCallback(evt =>
                {
                    WUInity.INPUT.Simulation.RunSmokeSim = evt.newValue;
                    UnityEngine.Debug.Log($"TogSimSmoke = {evt.newValue}");
                });

            // Register simulation settings ends       
        }

        /// <summary>
        // Initialise the callback for when "run simulation" is clicked
        /// </summary>
        private void SetupProjectTasks(VisualElement root)
        {
            // Add event handler for the Project New button being clicked
            UnityEngine.UIElements.Button btnProjectNew = root.Q<UnityEngine.UIElements.Button>("ProjectNew");
            if (btnProjectNew != null)
                btnProjectNew.clicked += BtnProjectNew_clicked; // Note: the += is "shorthand" for adding a handler to an event

            // Add event handler for the Project Open button being clicked
            UnityEngine.UIElements.Button btnProjectOpen = root.Q<UnityEngine.UIElements.Button>("ProjectOpen");
            if (btnProjectOpen != null)
                btnProjectOpen.clicked += BtnProjectOpen_clicked; // Note: the += is "shorthand" for adding a handler to an event

            // Add event handler for the Project Save button being clicked
            UnityEngine.UIElements.Button btnProjectSave = root.Q<UnityEngine.UIElements.Button>("ProjectSave");
            if (btnProjectSave != null)
                btnProjectSave.clicked += BtnProjectSave_clicked; // Note: the += is "shorthand" for adding a handler to an event
        }

        private void SetupOutputBoxTasks(VisualElement root)
        {
            
            UnityEngine.UIElements.Button btnClearOutputsButton = root.Q<UnityEngine.UIElements.Button>("ClearOutputsButton");
            if (btnClearOutputsButton != null) btnClearOutputsButton.clicked += BtnClearOutputsButton_clicked;

            UnityEngine.UIElements.Button btnHideOutputButton = root.Q<UnityEngine.UIElements.Button>("HideOutputButton");
            if (btnHideOutputButton != null) btnHideOutputButton.clicked += BtnHideOutputButton_clicked;

            UnityEngine.UIElements.Button btnVewHouseholds = root.Q<UnityEngine.UIElements.Button>("VewHouseholds");
            if (btnVewHouseholds != null) btnVewHouseholds.clicked += BtnVewHouseholds_clicked;

            UnityEngine.UIElements.Button btnVewTraffic = root.Q<UnityEngine.UIElements.Button>("VewTraffic");
            if (btnVewTraffic != null) btnVewTraffic.clicked += BtnVewTraffic_clicked;

            UnityEngine.UIElements.Button btnVewFireSpread = root.Q<UnityEngine.UIElements.Button>("VewFireSpread");
            if (btnVewFireSpread != null) btnVewFireSpread.clicked += BtnVewFireSpread_clicked;

            UnityEngine.UIElements.Button btnVewOpticalDensity = root.Q<UnityEngine.UIElements.Button>("VewOpticalDensity");
            if (btnVewOpticalDensity != null) btnVewOpticalDensity.clicked += BtnVewOpticalDensity;

            UnityEngine.UIElements.Button btnVewTrafficDensity = root.Q<UnityEngine.UIElements.Button>("VewTrafficDensity");
            if (btnVewTrafficDensity != null) btnVewTrafficDensity.clicked += BtnVewTrafficDensity_clicked;

            UnityEngine.UIElements.Button btnVewArrivalOutput = root.Q<UnityEngine.UIElements.Button>("VewArrivalOutput");
            if (btnVewArrivalOutput != null) btnVewArrivalOutput.clicked += BtnVewArrivalOutput_clicked;
        }

        private void ResetOutputDisplayOptions()
        {
            var root = Document.rootVisualElement;

            UnityEngine.UIElements.Button btnVewHouseholds = root.Q<UnityEngine.UIElements.Button>("VewHouseholds");
            if (btnVewHouseholds != null) btnVewHouseholds.text = "Households: on";

            UnityEngine.UIElements.Button btnVewTraffic = root.Q<UnityEngine.UIElements.Button>("VewTraffic");
            if (btnVewTraffic != null)    btnVewTraffic.text = "Traffic: on";

            UnityEngine.UIElements.Button btnVewFireSpread = root.Q<UnityEngine.UIElements.Button>("VewFireSpread");
            if (btnVewFireSpread != null) btnVewFireSpread.text = "Fire spread: on";

            UnityEngine.UIElements.Button btnVewOpticalDensity = root.Q<UnityEngine.UIElements.Button>("VewOpticalDensity");
            if (btnVewOpticalDensity != null) btnVewOpticalDensity.text = "Optical density: on";

            UnityEngine.UIElements.Button btnVewTrafficDensity = root.Q<UnityEngine.UIElements.Button>("VewTrafficDensity");
            if (btnVewTrafficDensity != null) btnVewTrafficDensity.text = "Traffic density: off";
        }

        private void BtnClearOutputsButton_clicked()
        {
            ClearOutput();
        }
        private void BtnHideOutputButton_clicked()
        {
            var root = Document.rootVisualElement;
            _bHideOutput = !_bHideOutput;

            /*
            UnityEngine.UIElements.Button btnHideOutputButton = root.Q<UnityEngine.UIElements.Button>("HideOutputButton");
            if(btnHideOutputButton != null)
            {
                if (_bHideOutput) btnHideOutputButton.text = "Unhide";
                else btnHideOutputButton.text = "Hide";
            }
            */
        }

        private void BtnVewHouseholds_clicked()
        {
            var root = Document.rootVisualElement;
            UnityEngine.UIElements.Button btnVewHouseholds = root.Q<UnityEngine.UIElements.Button>("VewHouseholds");

            if (btnVewHouseholds != null&& WUInity.SIM.IsRunning)
            {
                if(WUInity.INSTANCE.ToggleHouseholdRendering())
                    btnVewHouseholds.text = "Households: on";
                else
                    btnVewHouseholds.text = "Households: off";
            }
        }

        private void BtnVewTraffic_clicked()
        {
            var root = Document.rootVisualElement;
            UnityEngine.UIElements.Button btnVewTraffic = root.Q<UnityEngine.UIElements.Button>("VewTraffic");

            if (btnVewTraffic != null && WUInity.SIM.IsRunning)
            {
                if (WUInity.INSTANCE.ToggleTrafficRendering())
                    btnVewTraffic.text = "Traffic: on";
                else
                    btnVewTraffic.text = "Traffic: off";
            }
        }

        private void BtnVewFireSpread_clicked()
        {
            var root = Document.rootVisualElement;
            UnityEngine.UIElements.Button btnVewFireSpread = root.Q<UnityEngine.UIElements.Button>("VewFireSpread");

            if (btnVewFireSpread != null && WUInity.SIM.IsRunning)
            {
                if (WUInity.INSTANCE.ToggleFireSpreadRendering())
                    btnVewFireSpread.text = "Fire spread: on";
                else
                    btnVewFireSpread.text = "Fire spread: off";

                WUInity.INSTANCE.SetSampleMode(WUInity.DataSampleMode.None);
            }
        }

        private void BtnVewOpticalDensity()
        {
            var root = Document.rootVisualElement;
            UnityEngine.UIElements.Button btnVewOpticalDensity = root.Q<UnityEngine.UIElements.Button>("VewOpticalDensity");

            if (btnVewOpticalDensity != null && WUInity.SIM.IsRunning)
            {
                if (WUInity.INSTANCE.ToggleSootRendering())
                    btnVewOpticalDensity.text = "Optical density: on";
                else
                    btnVewOpticalDensity.text = "Optical density: off";

                WUInity.INSTANCE.SetSampleMode(WUInity.DataSampleMode.None);
            }
        }

        float sliderVtraffic = 1f;
        private void BtnVewTrafficDensity_clicked()
        {
            float timeRange = WUInity.OUTPUT.totalEvacTime - WUInity.SIM.StartTime;
            float time = sliderVtraffic * timeRange + WUInity.SIM.StartTime;

            var root = Document.rootVisualElement;
            UnityEngine.UIElements.Button btnVewTrafficDensity = root.Q<UnityEngine.UIElements.Button>("VewTrafficDensity");

            if (btnVewTrafficDensity != null && WUInity.SIM.IsRunning)
            {
                WUInity.INSTANCE.DisplayClosestDensityData(time);

                if (WUInity.INSTANCE.ToggleEvacDataPlane())
                    btnVewTrafficDensity.text = "Traffic density: on";
                else
                    btnVewTrafficDensity.text = "Traffic density: off";

                WUInity.INSTANCE.SetSampleMode(WUInity.DataSampleMode.TrafficDens);
            }
        }

        private void BtnVewArrivalOutput_clicked()
        {

        }

        void OnGUI()
        {
            var root = Document.rootVisualElement;
            UnityEngine.UIElements.VisualElement newGUI = root.Q<UnityEngine.UIElements.VisualElement>("MainUIBox");
            UnityEngine.UIElements.VisualElement systemLogBox = root.Q<UnityEngine.UIElements.VisualElement>("SystemLogBox");
            UnityEngine.UIElements.VisualElement simOutputsBox = root.Q<UnityEngine.UIElements.VisualElement>("SimOutputBox");
            UnityEngine.UIElements.VisualElement titleBar = root.Q<UnityEngine.UIElements.VisualElement>("TitleBar");

            if (WUInity.GUI.enabled)
            {
                newGUI.style.left = Screen.width - _iMainBoxWidth;
                newGUI.style.top = 20;
                newGUI.style.height = Screen.height - 20 - 160;
                
                simOutputsBox.visible = false;
                titleBar.visible = false;
            }
            else
            {
                newGUI.style.left = 0;
                newGUI.style.top = _titleBarHeight+1;
                newGUI.style.height = Screen.height - _titleBarHeight -1;
                
                simOutputsBox.visible = true;
                titleBar.visible = true;
                simOutputsBox.visible = true;

                UnityEngine.UIElements.Button minimizeButton = root.Q<UnityEngine.UIElements.Button>("TBarMinimizeButton");
                minimizeButton.style.left = Screen.width - 120;

                UnityEngine.UIElements.Button fullScreenButton = root.Q<UnityEngine.UIElements.Button>("TBarFullScreenButton");
                fullScreenButton.style.left = Screen.width - 80;

                UnityEngine.UIElements.Button quitButton = root.Q<UnityEngine.UIElements.Button>("TBarQuitButton");
                quitButton.style.left = Screen.width - 40;
            }

            if (systemLogBox.visible)
            {
                systemLogBox.style.left = _iMainBoxWidth+1;
                systemLogBox.style.top = Screen.height - _iSysLogBoxHeight;

                int logBoxWidth;
                if(simOutputsBox.visible)
                {
                    logBoxWidth = Screen.width - _iMainBoxWidth - _iOutputBoxWidth - 2;                    
                }
                else
                {
                    logBoxWidth = Screen.width - _iMainBoxWidth - _iMainBoxWidth - 2;
                }

                systemLogBox.style.width = logBoxWidth;

                Label togLabel = Document.rootVisualElement.Q<Label>("SysLogsText");

                if (togLabel != null)
                {
                    togLabel.style.width = logBoxWidth - 38;
                }
            }

            if (simOutputsBox.visible)
            {
                simOutputsBox.style.left = Screen.width - _iOutputBoxWidth;
                simOutputsBox.style.top = _titleBarHeight + 1;

                if (_bHideOutput)
                {
                    UnityEngine.UIElements.VisualElement outputButtonPanel = root.Q<UnityEngine.UIElements.VisualElement>("OutputButtonPanel");
                    if (outputButtonPanel != null) outputButtonPanel.visible = false;

                    UnityEngine.UIElements.VisualElement outputs = root.Q<UnityEngine.UIElements.VisualElement>("Outputs");
                    if (outputs != null) outputs.visible = false;

                    UnityEngine.UIElements.VisualElement displayControl = root.Q<UnityEngine.UIElements.VisualElement>("DisplayControl");
                    if (displayControl != null) displayControl.visible = false;

                    simOutputsBox.style.height = 1;//32;                    
                }
                else
                { 
                    simOutputsBox.style.height = Screen.height - _titleBarHeight - 1;

                    UnityEngine.UIElements.VisualElement outputButtonPanel = root.Q<UnityEngine.UIElements.VisualElement>("OutputButtonPanel");
                    if (outputButtonPanel != null) outputButtonPanel.visible = true;

                    UnityEngine.UIElements.VisualElement outputs = root.Q<UnityEngine.UIElements.VisualElement>("Outputs");
                    if (outputs != null) outputs.visible = true;

                    UnityEngine.UIElements.VisualElement displayControl = root.Q<UnityEngine.UIElements.VisualElement>("DisplayControl");
                    if (displayControl != null) displayControl.visible = true;
                    
                }

                if (WUInity.SIM.IsRunning)
                {
                    UpdateOutput();
                }
                else
                {
                    //ClearOutput();
                    UnityEngine.UIElements.Button btnStartSimulation = Document.rootVisualElement.Q<UnityEngine.UIElements.Button>("StartSimButton");
                    if (btnStartSimulation != null && btnStartSimulation.text== "Stop simulation") btnStartSimulation.text = "Start simulation";
                }
            }
            else
            {
                UnityEngine.UIElements.VisualElement outputButtonPanel = root.Q<UnityEngine.UIElements.VisualElement>("OutputButtonPanel");
                if (outputButtonPanel != null) outputButtonPanel.visible = false;

                UnityEngine.UIElements.VisualElement outputs = root.Q<UnityEngine.UIElements.VisualElement>("Outputs");
                if (outputs != null) outputs.visible = false;

                UnityEngine.UIElements.VisualElement displayControl = root.Q<UnityEngine.UIElements.VisualElement>("DisplayControl");
                if (displayControl != null) displayControl.visible = false;
            }
        }

        private void UpdateOutput()
        {
            var root = Document.rootVisualElement;

            Label label1 = Document.rootVisualElement.Q<Label>("TxtEvacID");
            label1.text = "Simulation ID: " + WUInity.INPUT.Simulation.SimulationID;

            Label label2 = Document.rootVisualElement.Q<Label>("TxtEvacTime");
            label2.text = "Sim. Clock: " + (int)WUInity.OUTPUT.totalEvacTime +" s\n\rdd:hh:mm:ss - " + TimeSpan.FromSeconds((int)WUInity.OUTPUT.totalEvacTime).ToString(@"dd\:hh\:mm\:ss");

            Label label3 = Document.rootVisualElement.Q<Label>("TxtTotalPop");
            label3.text = "Total population: " + WUInity.POPULATION.GetTotalPopulation();

            Label label4 = Document.rootVisualElement.Q<Label>("TxtPeopleStaying");
            label4.text = "People staying: " + WUInity.SIM.MacroHumanSim().GetPeopleStaying();

            Label label5 = Document.rootVisualElement.Q<Label>("TxtTotalCars");
            label5.text = "Total cars: " + WUInity.SIM.MacroHumanSim().GetTotalCars();

            if (WUInity.INPUT.Simulation.RunEvacSim && WUInity.SIM.MacroHumanSim() != null)
            {
                Label label6 = Document.rootVisualElement.Q<Label>("TxtPedLeft");
                label6.text = "Pedestrians left: " + WUInity.SIM.MacroHumanSim().GetPeopleLeft() + " (" + Math.Round((double)WUInity.SIM.MacroHumanSim().GetPeopleLeft() / (double)WUInity.POPULATION.GetTotalPopulation() * 100.0, 1) + "%)";

                Label label7 = Document.rootVisualElement.Q<Label>("TxtCarsReached");
                label7.text = "Cars reached by Peds: " + WUInity.SIM.MacroHumanSim().GetCarsReached();
            }

            if (WUInity.INPUT.Simulation.RunTrafficSim && WUInity.SIM.MacroTrafficSim() != null)
            {
                Label label8 = Document.rootVisualElement.Q<Label>("TxtCarsLeft");
                label8.text = "Cars left: " + WUInity.SIM.MacroTrafficSim().GetCarsInSystem() + " (" + Math.Round((double)WUInity.SIM.MacroTrafficSim().GetCarsInSystem() / (double)WUInity.SIM.MacroTrafficSim().GetTotalCarsSimulated() * 100.0, 1) + "%)";
            }

            int totalEvacuated = 0;
            string name="Evacuation goals reached:";
            for (int i = 0; i < WUInity.RUNTIME_DATA.Evacuation.EvacuationGoals.Count; i++)
            {
                totalEvacuated += WUInity.RUNTIME_DATA.Evacuation.EvacuationGoals[i].currentPeople;
                name += "\n\r" + WUInity.RUNTIME_DATA.Evacuation.EvacuationGoals[i].name;
                name += ": " + WUInity.RUNTIME_DATA.Evacuation.EvacuationGoals[i].currentPeople + " by " + WUInity.RUNTIME_DATA.Evacuation.EvacuationGoals[i].cars.Count+ " cars";
            }

            Label label9 = Document.rootVisualElement.Q<Label>("TxtEvacGoalsReached");
            label9.text = name;

            Label label10 = Document.rootVisualElement.Q<Label>("TxtTotalEvacuated");
            label10.text = "Total evacuated: " + totalEvacuated;

            if (WUInity.INPUT.Simulation.RunFireSim)
            {
                Label label11 = Document.rootVisualElement.Q<Label>("TxtWindSpeed");
                label11.text = "Wind speed: " + Math.Round(WUInity.SIM.GetFireWindSpeed(),1) + " m/s";

                Label label12 = Document.rootVisualElement.Q<Label>("TxtWindDirection");
                label12.text = "Wind direction: " + Math.Round(WUInity.SIM.GetFireWindDirection(),1) + " °";

                Label label13 = Document.rootVisualElement.Q<Label>("TxtActiveCells");
                label13.text = "Active cells: " + WUInity.SIM.FireMesh().GetActiveCellCount();
            }
        }

        private void ClearOutput()
        {
            var root = Document.rootVisualElement;

            Label label1 = Document.rootVisualElement.Q<Label>("TxtEvacID");
            label1.text = "Simulation ID:";

            Label label2 = Document.rootVisualElement.Q<Label>("TxtEvacTime");
            label2.text = "Sim. Clock:";

            Label label3 = Document.rootVisualElement.Q<Label>("TxtTotalPop");
            label3.text = "Total population:";

            Label label4 = Document.rootVisualElement.Q<Label>("TxtPeopleStaying");
            label4.text = "People staying:";

            Label label5 = Document.rootVisualElement.Q<Label>("TxtTotalCars");
            label5.text = "Total cars:";

            Label label6 = Document.rootVisualElement.Q<Label>("TxtPedLeft");
            label6.text = "Pedestrians left:";

            Label label7 = Document.rootVisualElement.Q<Label>("TxtCarsReached");
            label7.text = "Cars reached by Peds:";

            Label label8 = Document.rootVisualElement.Q<Label>("TxtCarsLeft");
            label8.text = "Cars left:";

            Label label9 = Document.rootVisualElement.Q<Label>("TxtEvacGoalsReached");
            label9.text = "Evacuation goals reached:";

            Label label10 = Document.rootVisualElement.Q<Label>("TxtTotalEvacuated");
            label10.text = "Total evacuated:";

            Label label11 = Document.rootVisualElement.Q<Label>("TxtWindSpeed");
            label11.text = "Wind speed:";

            Label label12 = Document.rootVisualElement.Q<Label>("TxtWindDirection");
            label12.text = "Wind direction:";

            Label label13 = Document.rootVisualElement.Q<Label>("TxtActiveCells");
            label13.text = "Active cells:";
        }

        private void BtnProjectNew_clicked()
        {
            FileBrowser.SetFilters(false, fileFilter[(int)FileType.wuiFile]);

            WUInityInput wO = WUInity.INPUT;
            string initialPath = Path.Combine(Directory.GetParent(Application.dataPath).ToString(), "Project");

            if (!File.Exists(initialPath))
                System.IO.Directory.CreateDirectory(initialPath);

            creatingNewFile=true;

            //initialPath = Path.GetDirectoryName(WUInity.WORKING_FILE);  //test code

            FileBrowser.ShowSaveDialog(SaveInput, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, "New_sim.wui", "Save file", "Save");

        }

        private void BtnProjectOpen_clicked()
        {
            OpenLoadInput();    // Equivalent to WUInity.GUI.OpenLoadInput(); 

            /* The following code doesn't work right here. It is moved to Update().
            if (newUIMenuDirty) {
                UpdateMenu();
                newUIMenuDirty = false;
            }  */
        }

        private void BtnUpdateMap_clicked()
        {
            WUInityInput wO = WUInity.INPUT;

            string[] floatNumbers= _mapLLLatLong.Split(',');

            double.TryParse(floatNumbers[0], out wO.Simulation.LowerLeftLatLong.x);
            double.TryParse(floatNumbers[1], out wO.Simulation.LowerLeftLatLong.y);
            
            floatNumbers = _mapSizeXY.Split(',');

            double.TryParse(floatNumbers[0], out wO.Simulation.Size.x);
            double.TryParse(floatNumbers[1], out wO.Simulation.Size.y);
            int.TryParse(_mapZoomLevel, out wO.Map.ZoomLevel);

            WUInity.INSTANCE.UpdateMapResourceStatus();
        }

        private void BtnBuildLocalGPW_clicked()
        {
            if (WUInity.DATA_STATUS.GlobalGPWAvailable && WUInity.INPUT.Population.gpwDataFolder.Length > 0)
            {
                WUInity.POPULATION.CreateLocalGPW();
                SetLocalGPWNumber();
            }
            else
                UnityEngine.Debug.Log($"Global GPW data is not avaible to build local GPW data.");
        }

        private void BtnShowHideLocalGPW_clicked()
        {
            if (WUInity.DATA_STATUS.LocalGPWLoaded) WUInity.POPULATION.ToggleLocalGPWVisibility();
        }

        private void BtnShowHideLocalPOP_clicked()
        {
            if (WUInity.POPULATION.IsPopulationLoaded())
            {
                WUInity.INSTANCE.SetSampleMode(WUInity.DataSampleMode.Population);
                WUInity.INSTANCE.DisplayPopulation();
                WUInity.INSTANCE.ToggleEvacDataPlane();
            }
        }

        private void BtnCorrectPop_clicked()
        {
            if (WUInity.POPULATION.IsPopulationLoaded())
            {
                WUInity.POPULATION.UpdatePopulationBasedOnRoutes(WUInity.RUNTIME_DATA.Routing.RouteCollections);
                WUInity.INSTANCE.DisplayPopulation();
                SetPopulationAndCellNumber();
            }
        }

        private void BtnRescalePop_clicked()
        {
            int newPop;
            if (int.TryParse(_rescalePop, out newPop))
            {
                WUInity.POPULATION.ScaleTotalPopulation(newPop);
                WUInity.INSTANCE.DisplayPopulation();
                SetPopulationAndCellNumber();
            }
            else
            {
                WUInity.LOG(WUInity.LogType.Error, " New population count not a number.");
            }
        }
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// 
        // This function is called after a project is loaded from a .wui file and uses information from the file to set the menu/statue of workflow.
        //
        void UpdateMenu()
        {
            var root = Document.rootVisualElement;
            WUInityInput wO = WUInity.INPUT;
            EvacuationInput eO = WUInity.INPUT.Evacuation;
            TrafficInput tO = WUInity.INPUT.Traffic;

            if (root != null)
            {
                // 2. Map section ------------------------------------------------------------------------------------------------------------------
                UnityEngine.UIElements.TextField tfTxTSetLatLong = root.Q<UnityEngine.UIElements.TextField>("TxTSetLatLong");
                if (tfTxTSetLatLong != null)
                    tfTxTSetLatLong.value = wO.Simulation.LowerLeftLatLong.x.ToString() + ", " + wO.Simulation.LowerLeftLatLong.y.ToString();

                UnityEngine.UIElements.TextField tfTxTSetMapSize = root.Q<UnityEngine.UIElements.TextField>("TxTSetMapSize");
                if (tfTxTSetMapSize != null)
                    tfTxTSetMapSize.value = wO.Simulation.Size.x.ToString() + ", " + wO.Simulation.Size.y.ToString();

                UnityEngine.UIElements.TextField tfTxTSetMapZoomLevel = root.Q<UnityEngine.UIElements.TextField>("TxTSetMapZoomLevel");
                if (tfTxTSetMapZoomLevel != null)
                    tfTxTSetMapZoomLevel.value = wO.Map.ZoomLevel.ToString();

                // 3. Population section -------------------------------------------------------------------------------------------------------------
                SetGlobalGPWFileFoler();
                SetLocalGPWFile();
                SetPopulationFile();

                SetLocalGPWNumber();
                SetPopulationAndCellNumber();

                // 4. Evacuation goals -------------------------------------------------------------------------------------------------------
                UnityEngine.UIElements.DropdownField dfDfEvacutionDestination= root.Q<UnityEngine.UIElements.DropdownField>("DfEvacutionDestination");

                if (dfDfEvacutionDestination != null && WUInity.INPUT.Traffic.evacuationGoalFiles.Length > 0 )
                {
                    List<string> m_DropOptions = new List<string> {};

                    foreach(EvacuationGoal eg in WUInity.RUNTIME_DATA.Evacuation.EvacuationGoals)
                        m_DropOptions.Add(eg.name);

                    dfDfEvacutionDestination.choices.Clear();
                    dfDfEvacutionDestination.choices = m_DropOptions;
                    dfDfEvacutionDestination.index = 0;
                }

                // 5A. Response curve -------------------------------------------------------------------------------------------------------------
                UnityEngine.UIElements.DropdownField dfDfResponseCurve = root.Q<UnityEngine.UIElements.DropdownField>("DfResponseCurve");

                if (dfDfResponseCurve != null && WUInity.INPUT.Evacuation.ResponseCurveFiles.Length > 0)
                {
                    List<string> m_DropOptions = new List<string> {};

                    foreach(ResponseCurve rc in WUInity.RUNTIME_DATA.Evacuation.ResponseCurves)
                        m_DropOptions.Add(rc.name);

                    dfDfResponseCurve.choices.Clear();
                    dfDfResponseCurve.choices = m_DropOptions;
                    dfDfResponseCurve.index = 0;
                }

                // 5B. Evacuation group -------------------------------------------------------------------------------------------------------------
                UnityEngine.UIElements.DropdownField dfDfEvacuationGroup = root.Q<UnityEngine.UIElements.DropdownField>("DfEvacuationGroup");

                if (dfDfEvacuationGroup != null && WUInity.INPUT.Evacuation.EvacGroupFiles.Length > 0)
                {
                    List<string> m_DropOptions = new List<string> {};

                    foreach (EvacGroup eg in WUInity.RUNTIME_DATA.Evacuation.EvacuationGroups)
                        m_DropOptions.Add(eg.Name);

                    dfDfEvacuationGroup.choices.Clear();
                    dfDfEvacuationGroup.choices = m_DropOptions;
                    dfDfEvacuationGroup.index = 0;
                }

                // 6. Evacuation section -------------------------------------------------------------------------------------------------------------
                UnityEngine.UIElements.TextField tfTxTSetEvacCellSize = root.Q<UnityEngine.UIElements.TextField>("TxTSetEvacCellSize");
                if (tfTxTSetEvacCellSize != null)
                {
                    tfTxTSetEvacCellSize.value = eO.RouteCellSize.ToString();
                }

                UnityEngine.UIElements.Toggle tgTogAllowMTOneCar = root.Q<UnityEngine.UIElements.Toggle>("TogAllowMTOneCar");
                if (tgTogAllowMTOneCar != null)
                {
                    tgTogAllowMTOneCar.SetValueWithoutNotify(eO.allowMoreThanOneCar);
                }

                UnityEngine.UIElements.TextField tfTxTSetMaxCarsPH = root.Q<UnityEngine.UIElements.TextField>("TxTSetMaxCarsPH");
                if (tfTxTSetMaxCarsPH != null)
                {
                    tfTxTSetMaxCarsPH.value = eO.maxCars.ToString();
                }

                UnityEngine.UIElements.TextField tfTxTSetProbMCPH = root.Q<UnityEngine.UIElements.TextField>("TxTSetProbMCPH");
                if (tfTxTSetProbMCPH != null)
                {
                    tfTxTSetProbMCPH.value = eO.maxCarsChance.ToString();
                }

                UnityEngine.UIElements.TextField tfTxTSetMinCarsPH = root.Q<UnityEngine.UIElements.TextField>("TxTSetMinCarsPH");
                if (tfTxTSetMinCarsPH != null)
                {
                    tfTxTSetMinCarsPH.value = eO.minHouseholdSize.ToString();
                }

                UnityEngine.UIElements.TextField tfTxTSetMaxPersPH = root.Q<UnityEngine.UIElements.TextField>("TxTSetMaxPersPH");
                if (tfTxTSetMaxPersPH != null)
                {
                    tfTxTSetMaxPersPH.value = eO.maxHouseholdSize.ToString();
                }

                UnityEngine.UIElements.TextField tfTxTSetMinWalkSpeed = root.Q<UnityEngine.UIElements.TextField>("TxTSetMinWalkSpeed");
                if (tfTxTSetMinWalkSpeed != null)
                {
                    tfTxTSetMinWalkSpeed.value = eO.walkingSpeedMinMax.x.ToString();
                }

                UnityEngine.UIElements.TextField tfTxTSetMaxWalkSpeed = root.Q<UnityEngine.UIElements.TextField>("TxTSetMaxWalkSpeed");
                if (tfTxTSetMaxWalkSpeed != null)
                {
                    tfTxTSetMaxWalkSpeed.value = eO.walkingSpeedMinMax.y.ToString();
                }

                UnityEngine.UIElements.TextField tfTxTSetModWalkSpeed = root.Q<UnityEngine.UIElements.TextField>("TxTSetModWalkSpeed");
                if (tfTxTSetModWalkSpeed != null)
                {
                    tfTxTSetModWalkSpeed.value = eO.walkingSpeedModifier.ToString();
                }

                UnityEngine.UIElements.TextField tfTxTSetmodWalkDist = root.Q<UnityEngine.UIElements.TextField>("TxTSetModWalkDist");
                if (tfTxTSetmodWalkDist != null)
                {
                    tfTxTSetmodWalkDist.value = eO.walkingDistanceModifier.ToString();
                }

                UnityEngine.UIElements.TextField tfTxTEvaOrderTime = root.Q<UnityEngine.UIElements.TextField>("TxTEvaOrderTime");
                if (tfTxTEvaOrderTime != null)
                {
                    tfTxTEvaOrderTime.value = eO.EvacuationOrderStart.ToString();
                }

                // 7. Routing section  ------------------------------------------------------------------------------------------------------------
                SetRouterDBFile();
                SetRouteRCFile();

                // 8. Traffic section -------------------------------------------------------------------------------------------------------------
                UnityEngine.UIElements.DropdownField dfRouteChoice = root.Q<UnityEngine.UIElements.DropdownField>("DfRouteChoice");

                if (dfRouteChoice != null)
                {
                    List<string> m_DropOptions = new List<string> {};

                    foreach(string s in Enum.GetNames(typeof(TrafficInput.RouteChoice)))
                        m_DropOptions.Add(s);

                    dfRouteChoice.choices.Clear();
                    dfRouteChoice.choices = m_DropOptions;
                    dfRouteChoice.index = (int)tO.routeChoice;

                    //UnityEngine.Debug.Log($"Current route choice ispppp = {(int)tO.routeChoice}");
                }

                UnityEngine.UIElements.DropdownField dfRoadType = root.Q<UnityEngine.UIElements.DropdownField>("DfRoadType");
                if (dfRoadType != null)
                {
                    List<string> m_DropOptions = new List<string> {};

                    foreach(Traffic.RoadData rd in WUInity.RUNTIME_DATA.Traffic.RoadTypeData.roadData)
                        m_DropOptions.Add(rd.name);

                    dfRoadType.choices.Clear();
                    dfRoadType.choices = m_DropOptions;
                    dfRoadType.index = 0;
                }

                UnityEngine.UIElements.TextField tfTxTSetMaxCapTrafSpeed = root.Q<UnityEngine.UIElements.TextField>("TxTSetMaxCapTrafSpeed");
                if (tfTxTSetMaxCapTrafSpeed != null)
                {
                    tfTxTSetMaxCapTrafSpeed.value = tO.stallSpeed.ToString();
                }

                UnityEngine.UIElements.Toggle tgTogSpeedAffectedBySmoke = root.Q<UnityEngine.UIElements.Toggle>("TogSpeedAffectedBySmoke"); // Need to add it into menu-demo.uxml
                if (tgTogSpeedAffectedBySmoke != null)
                {
                    tgTogSpeedAffectedBySmoke.SetValueWithoutNotify(tO.visibilityAffectsSpeed);
                }

                // 9. Landscape data section ------------------------------------------------------------------------------------------------------

                // 10. Fire characteristics -------------------------------------------------------------------------------------------------------
                SetLCPFile();
                SetFuelModelFile();
                SetFuelMoistureFile();
                SetWeatherFile();
                SetWindFile();
                SetIgnitionPointsFile();
                SetGraphicalFireInputFile();

                // 12. Similation section ---------------------------------------------------------------------------------------------------------
                UnityEngine.UIElements.TextField tfTxTSetSimID = root.Q<UnityEngine.UIElements.TextField>("TxTSetSimID");
                if (tfTxTSetSimID != null)
                {
                    tfTxTSetSimID.value = wO.Simulation.SimulationID;
                }

                UnityEngine.UIElements.TextField tfTxTSetTimeStep = root.Q<UnityEngine.UIElements.TextField>("TxTSetTimeStep");
                if (tfTxTSetTimeStep != null)
                {
                    tfTxTSetTimeStep.value = wO.Simulation.DeltaTime.ToString();
                }

                UnityEngine.UIElements.TextField tfTxTSetMaxSimTime = root.Q<UnityEngine.UIElements.TextField>("TxTSetMaxSimTime");
                if (tfTxTSetMaxSimTime != null)
                {
                    tfTxTSetMaxSimTime.value = wO.Simulation.MaxSimTime.ToString();
                }

                UnityEngine.UIElements.TextField tfTxTSetNumSims = root.Q<UnityEngine.UIElements.TextField>("TxTSetNumSims");
                if (tfTxTSetNumSims != null)
                {
                    tfTxTSetNumSims.value = WUInity.RUNTIME_DATA.Simulation.NumberOfRuns.ToString();
          
                    UnityEngine.UIElements.Toggle tgTogMultipleSim = root.Q<UnityEngine.UIElements.Toggle>("TogMultipleSim");
                    if (tgTogMultipleSim != null)
                    {
                        tgTogMultipleSim.SetValueWithoutNotify(WUInity.RUNTIME_DATA.Simulation.MultipleSimulations);

                        if(WUInity.RUNTIME_DATA.Simulation.MultipleSimulations)
                        {
                            tfTxTSetNumSims.ToggleInClassList("hide");
                        }
                    }
                }

                UnityEngine.UIElements.Toggle tgTogSimPeds = root.Q<UnityEngine.UIElements.Toggle>("TogSimPeds");
                if (tgTogSimPeds != null)
                {
                    tgTogSimPeds.SetValueWithoutNotify(WUInity.INPUT.Simulation.RunEvacSim);
                }

                UnityEngine.UIElements.Toggle tgTogSimTraf = root.Q<UnityEngine.UIElements.Toggle>("TogSimTraf");
                if (tgTogSimTraf != null)
                {
                    tgTogSimTraf.SetValueWithoutNotify(WUInity.INPUT.Simulation.RunTrafficSim);
                }

                UnityEngine.UIElements.Toggle tgTogSimFire = root.Q<UnityEngine.UIElements.Toggle>("TogSimFire");
                if (tgTogSimFire != null)
                {
                    tgTogSimFire.SetValueWithoutNotify(WUInity.INPUT.Simulation.RunFireSim);
                }

                UnityEngine.UIElements.Toggle tgTogSimSmoke = root.Q<UnityEngine.UIElements.Toggle>("TogSimSmoke");
                if (tgTogSimSmoke != null)
                {
                    tgTogSimSmoke.SetValueWithoutNotify(WUInity.INPUT.Simulation.RunSmokeSim);
                }
            }
        }

        void SetLocalGPWNumber()
        {
            var root = Document.rootVisualElement;

            Label togTxtGPWNumber = root.Q<Label>("TxtGPWNumber");

            if (togTxtGPWNumber != null) { 
                if(WUInity.DATA_STATUS.LocalGPWLoaded)
                    togTxtGPWNumber.text = "GPW total population: " + WUInity.POPULATION.GetLocalGPWTotalPopulation();
                else
                    togTxtGPWNumber.text = "GPW total population:";
            }
        }

        void SetPopulationAndCellNumber()
        {
            var root = Document.rootVisualElement;

            Label togTxtPOPNumber = root.Q<Label>("TxtPOPNumber");

            if (togTxtPOPNumber != null)
            {
                if (WUInity.DATA_STATUS.PopulationLoaded)
                    togTxtPOPNumber.text = "Total population (adjusted): " + WUInity.POPULATION.GetTotalPopulation();
                else
                    togTxtPOPNumber.text = "Total population (adjusted):";
            }

            Label togTxtPOPCellsNumber = root.Q<Label>("TxtPOPCellsNumber");

            if (togTxtPOPCellsNumber != null)
            {
                if (WUInity.DATA_STATUS.PopulationLoaded)
                    togTxtPOPCellsNumber.text = "Total avtive cells: " + WUInity.POPULATION.GetTotalActiveCells();
                else
                    togTxtPOPCellsNumber.text = "Total avtive cells:";
            }
        }

        void SetGlobalGPWFileFoler()
        {
            var root = Document.rootVisualElement;
            Toggle togTogSetGPWFolder = root.Q<Toggle>("TogSetGPWFolder");
            if (togTogSetGPWFolder != null)
            {
                string filePath = "No valid global GPW data. Please select correct GPW folder.";

                if (WUInity.DATA_STATUS.GlobalGPWAvailable && WUInity.INPUT.Population.gpwDataFolder.Length > 0)
                {
                    togTogSetGPWFolder.SetValueWithoutNotify(true);

                    if (WUInity.INPUT.Population.gpwDataFolder.Length < iGPWfolderLength)
                        filePath = "GPW folder: " + WUInity.INPUT.Population.gpwDataFolder;
                    else
                        filePath = "GPW folder: ..." + WUInity.INPUT.Population.gpwDataFolder.Substring(WUInity.INPUT.Population.gpwDataFolder.Length - iGPWfolderLength);
                }
                else
                {
                    togTogSetGPWFolder.SetValueWithoutNotify(false);
                }

                Label togTxtGPWFolder = root.Q<Label>("TxtGPWFolder");
                if (togTxtGPWFolder != null) togTxtGPWFolder.text = filePath;
            }
        }

        void SetLocalGPWFile()
        {
            var root = Document.rootVisualElement;
            Toggle togPopulateFromGPW = root.Q<Toggle>("TogPopulateFromGPW");

            if (togPopulateFromGPW != null)
            {
                Label txtGPWFile = root.Q<Label>("TxtGPWFile");
                if (txtGPWFile != null)
                {
                    string filePath;
                    if (WUInity.DATA_STATUS.LocalGPWLoaded && WUInity.INPUT.Population.localGPWFile.Length > 0)
                    {
                        filePath = "GPW file: " + WUInity.INPUT.Population.localGPWFile;
                        togPopulateFromGPW.SetValueWithoutNotify(true);
                    }
                    else
                    {
                        filePath = "GPW file: not set";
                        togPopulateFromGPW.SetValueWithoutNotify(false);
                    }

                    txtGPWFile.text = filePath;
                }
            }
        }

        void SetPopulationFile()
        {
            var root = Document.rootVisualElement;
            Toggle togPopulateFromPOP = root.Q<Toggle>("TogPopulateFromPOP");

            if (togPopulateFromPOP != null)
            {
                Label txtPOPFile = root.Q<Label>("TxtPOPFile");
                if (txtPOPFile != null)
                {
                    string filePath;
                    if (WUInity.DATA_STATUS.PopulationLoaded && WUInity.INPUT.Population.populationFile.Length > 0)
                    {
                        filePath = "POP file: " + WUInity.INPUT.Population.populationFile;
                        togPopulateFromPOP.SetValueWithoutNotify(true);
                    }
                    else
                    {
                        filePath = "POP file: not set";
                        togPopulateFromPOP.SetValueWithoutNotify(false);
                    }

                    txtPOPFile.text = filePath;
                }
            }
        }

        void SetLCPFile()
        {
            var root = Document.rootVisualElement;
            Toggle togLoadLCPFile = root.Q<Toggle>("TogLoadLCPFile");

            if (togLoadLCPFile != null)
            {
                Label txtLCPFile = root.Q<Label>("TxtLCPFile");
                if (txtLCPFile != null)
                {
                    string filePath;
                    if (WUInity.INPUT.Fire.lcpFile.Length > 0)
                    {
                        filePath = "LCP file: " + WUInity.INPUT.Fire.lcpFile;
                        togLoadLCPFile.SetValueWithoutNotify(true);
                    }
                    else
                    {
                        filePath = "LCP file: not set";
                        togLoadLCPFile.SetValueWithoutNotify(false);
                    }

                    txtLCPFile.text = filePath;
                }
            }
        }

        void SetFuelModelFile()
        {
            var root = Document.rootVisualElement;
            Toggle togLoadFuelModelFile = root.Q<Toggle>("TogLoadFuelModelFile");

            if (togLoadFuelModelFile != null)
            {
                Label txtFuelModelFile = root.Q<Label>("TxtFuelModelFile");
                if (txtFuelModelFile != null)
                {
                    string filePath;
                    if (WUInity.INPUT.Fire.fuelModelsFile.Length > 0)
                    {
                        filePath = "Fuel model file: " + WUInity.INPUT.Fire.fuelModelsFile;
                        togLoadFuelModelFile.SetValueWithoutNotify(true);
                    }
                    else
                    {
                        filePath = "Fuel model file: not set";
                        togLoadFuelModelFile.SetValueWithoutNotify(false);
                    }

                    txtFuelModelFile.text = filePath;
                }
            }
        }

        void SetFuelMoistureFile()
        {
            var root = Document.rootVisualElement;
            Toggle togLoadFuelMoistureFile = root.Q<Toggle>("TogLoadFuelMoistureFile");

            if (togLoadFuelMoistureFile != null)
            {
                Label txtFuelMoistureFile = root.Q<Label>("TxtFuelMoistureFile");
                if (txtFuelMoistureFile != null)
                {
                    string filePath;
                    if (WUInity.INPUT.Fire.initialFuelMoistureFile.Length > 0)
                    {
                        filePath = "Fuel moisture file: " + WUInity.INPUT.Fire.initialFuelMoistureFile;
                        togLoadFuelMoistureFile.SetValueWithoutNotify(true);
                    }
                    else
                    {
                        filePath = "Fuel moisture file: not set";
                        togLoadFuelMoistureFile.SetValueWithoutNotify(false);
                    }

                    txtFuelMoistureFile.text = filePath;
                }
            }
        }

        void SetWeatherFile()
        {
            var root = Document.rootVisualElement;
            Toggle togLoadWeatherFile = root.Q<Toggle>("TogLoadWeatherFile");

            if (togLoadWeatherFile != null)
            {
                Label txtWeatherFile = root.Q<Label>("TxtWeatherFile");
                if (txtWeatherFile != null)
                {
                    string filePath;
                    if (WUInity.INPUT.Fire.weatherFile.Length > 0)
                    {
                        filePath = "Weather file: " + WUInity.INPUT.Fire.weatherFile;
                        togLoadWeatherFile.SetValueWithoutNotify(true);
                    }
                    else
                    {
                        filePath = "Weather file: not set";
                        togLoadWeatherFile.SetValueWithoutNotify(false);
                    }

                    txtWeatherFile.text = filePath;
                }
            }
        }

        void SetWindFile()
        {
            var root = Document.rootVisualElement;
            Toggle togLoadWindFile = root.Q<Toggle>("TogLoadWindFile");

            if (togLoadWindFile != null)
            {
                Label txtWindFile = root.Q<Label>("TxtWindFile");
                if (txtWindFile != null)
                {
                    string filePath;
                    if (WUInity.INPUT.Fire.windFile.Length > 0)
                    {
                        filePath = "Wind file: " + WUInity.INPUT.Fire.windFile;
                        togLoadWindFile.SetValueWithoutNotify(true);
                    }
                    else
                    {
                        filePath = "Wind file: not set";
                        togLoadWindFile.SetValueWithoutNotify(false);
                    }

                    txtWindFile.text = filePath;
                }
            }
        }

        void SetIgnitionPointsFile()
        {
            var root = Document.rootVisualElement;
            Toggle togLoadIgnitionPointsFile = root.Q<Toggle>("TogLoadIgnitionPointsFile");

            if (togLoadIgnitionPointsFile != null)
            {
                Label txtIgnitionPointsFile = root.Q<Label>("TxtIgnitionPointsFile");
                if (txtIgnitionPointsFile != null)
                {
                    string filePath;
                    if (WUInity.INPUT.Fire.ignitionPointsFile.Length > 0)
                    {
                        filePath = "Ignition points file: " + WUInity.INPUT.Fire.ignitionPointsFile;
                        togLoadIgnitionPointsFile.SetValueWithoutNotify(true);
                    }
                    else
                    {
                        filePath = "Ignition points file: not set";
                        togLoadIgnitionPointsFile.SetValueWithoutNotify(false);
                    }

                    txtIgnitionPointsFile.text = filePath;
                }
            }
        }

        void SetGraphicalFireInputFile()
        {
            var root = Document.rootVisualElement;
            Toggle togLoadGraphicalFireInputFile = root.Q<Toggle>("TogLoadGraphicalFireInputFile");

            if (togLoadGraphicalFireInputFile != null)
            {
                Label txtGraphicalFireInputFile = root.Q<Label>("TxtGraphicalFireInputFile");
                if (txtGraphicalFireInputFile != null)
                {
                    string filePath;
                    if (WUInity.INPUT.Fire.graphicalFireInputFile.Length > 0)
                    {
                        filePath = "Graphical fire input file: " + WUInity.INPUT.Fire.graphicalFireInputFile;
                        togLoadGraphicalFireInputFile.SetValueWithoutNotify(true);
                    }
                    else
                    {
                        filePath = "Graphical fire input file: not set";
                        togLoadGraphicalFireInputFile.SetValueWithoutNotify(false);
                    }

                    txtGraphicalFireInputFile.text = filePath;
                }
            }
        }

        void OpenLoadInput()
        {
            FileBrowser.SetFilters(false, fileFilter[(int)FileType.wuiFile]);
            
            string initialPath = WUInity.DATA_FOLDER;

            if (WUInity.DATA_STATUS.HaveInput)
            {
                initialPath = Path.GetDirectoryName(WUInity.WORKING_FOLDER);
            }

            FileBrowser.ShowLoadDialog(LoadInput, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Load WUINITY project file (.WUI)", "Load");
        }

        void LoadInput(string[] paths)
        {
            WUInityInput.LoadInput(paths[0]);
            mainMenuDirty = true;
            newUIMenuDirty = true;

            LoadWorkflowUIStatus();
        }

        /// <summary>
        // Initialise the callback for when "run simulation" is clicked
        /// </summary>
        private void BtnProjectSave_clicked()
        {
            if (WUInity.WORKING_FILE == null)
            {
                //OpenSaveInput(); --- port 4 lines of code below
                FileBrowser.SetFilters(false, fileFilter[(int)FileType.wuiFile]);
                WUInityInput wO = WUInity.INPUT;
                string initialPath = Path.GetDirectoryName(WUInity.WORKING_FILE);
                FileBrowser.ShowSaveDialog(SaveInput, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, wO.Simulation.SimulationID + ".wui", "Save file", "Save");
            }
            else
            {
                //ParseMainData(wO);    // Need to port code later
                WUInityInput.SaveInput();
            }

            SaveWorkflowUIStatus();
        }

        private void SaveWorkflowUIStatus()
        {
            if (WUInity.DATA_STATUS.HaveInput)
            {
                var toggles = Document.rootVisualElement.Query<Toggle>();
                List<Toggle> tlist = toggles.ToList();

                string templateText = "";
                foreach (Toggle toggle in tlist)
                {
                    templateText += toggle.text + " = " + toggle.value + "\n";
                }

                string initFile = Path.Combine(GetProjectPath(), WUInity.INPUT.Simulation.SimulationID + ".ini");
                System.IO.File.WriteAllText(initFile, templateText);
            }
        }

        private void LoadWorkflowUIStatus()
        {
            var toggles = Document.rootVisualElement.Query<Toggle>();
            List<Toggle> tlist = toggles.ToList();

            string initFile = Path.Combine(GetProjectPath(), WUInity.INPUT.Simulation.SimulationID + ".ini");

            if (File.Exists(initFile))
            {
                string[] dataLines = File.ReadAllLines(initFile);

                int i = 0;
                if (tlist.Count == dataLines.Length)
                {
                    foreach (string line in dataLines)
                    {
                        string[] data = line.Split('=');

                        if (data.Length == 2)
                        {
                            bool setValue = false;
                            if (data[1].Contains("True")) setValue = true;
                            
                            tlist[i].SetValueWithoutNotify(setValue);
                        }

                        i++;
                    }
                }
                else
                {
                    WUInity.LOG(WUInity.LogType.Error, "Workflow status file length does not match the number of toggles!");
                }
            }
            else
            {
                WUInity.LOG(WUInity.LogType.Error, "Workflow status file does not exist!");
            }
        }

        void SaveInput(string[] paths)
        {

            WUInity.WORKING_FILE = paths[0];
            WUInityInput wO = WUInity.INPUT;

            if (creatingNewFile)
            {
                mainMenuDirty = true;

                //WUInity.INSTANCE.CreateNewInputData();

                WUInity.DATA_STATUS.Reset();
                WUInity.DATA_STATUS.HaveInput = true;

                WUInity.INPUT.Simulation = new SimulationInput();
                WUInity.INPUT.Map = new MapInput();
                WUInity.INPUT.Visualization = new VisualizationOptions();
                WUInity.INPUT.Population = new PopulationInput();
                WUInity.INPUT.Routing = new RoutingInput();
                WUInity.INPUT.Evacuation = new EvacuationInput();
                WUInity.INPUT.Traffic = new TrafficInput();
                WUInity.INPUT.Fire = new FireInput();
                WUInity.INPUT.Smoke = new SmokeInput();

                string json = JsonUtility.ToJson(WUInity.INPUT, true);
                System.IO.File.WriteAllText(WUInity.WORKING_FILE, json);

                WUInityInput.LoadInput(paths[0]); // The default constructors have problems. This is like an initialization process for WUInity.INSTANCE

                //WUInity.INPUT.Population.populationFile = "";
                //WUInity.INPUT.Population.localGPWFile = "";

                //transform input to actual data
                //WUInity.RUNTIME_DATA.Population.LoadAll();
                //WUInity.RUNTIME_DATA.Evacuation.LoadAll();
                //need to load evacuation goals before routing as they rely on evacuation goals
                //WUInity.RUNTIME_DATA.Routing.LoadAll();
                //WUInity.RUNTIME_DATA.Traffic.LoadAll();
                //WUInity.RUNTIME_DATA.Fire.LoadAll();

                //WUInity.INSTANCE.UpdateMapResourceStatus();

                //this needs map and evac goals

                //WUInity.INSTANCE.SpawnMarkers();

                //wO = WUInity.INPUT; //have to update this since we are creating a new one

                newUIMenuDirty = true;
            }
            else
            {
                //ParseMainData(wO);
                WUInityInput.SaveInput();
            }

            creatingNewFile = false;
            string name = Path.GetFileNameWithoutExtension(paths[0]);
            wO.Simulation.SimulationID = name;
        }

        void CancelSaveLoad()
        {
            creatingNewFile = false;
        }

        /// <summary>
        // Initialise the callback for when "run simulation" is clicked
        /// </summary>
        private void SetupRunSimulationTask(VisualElement root)
        {
            // Add event handler for the Run Simulation button being clicked
            UnityEngine.UIElements.Button btnStartSimulation = root.Q<UnityEngine.UIElements.Button>("StartSimButton");
            if (btnStartSimulation != null)
                btnStartSimulation.clicked += BtnStartSim_clicked; // Note: the += is "shorthand" for adding a handler to an event
                                                                   // Anonymous version: btnStartSimulation.clicked += () => MessageBox.Show("Function call required", "Call Jonathan's start simulation code entry point");

            UnityEngine.UIElements.Button btnPauseSimulation = root.Q<UnityEngine.UIElements.Button>("PauseSimButton");
            if (btnPauseSimulation != null)
                btnPauseSimulation.clicked += BtnPauseSim_clicked;

        }

        bool _bSimulationPaused = false;
        private void BtnPauseSim_clicked()
        {
            // Start and stop running simulation.
            UnityEngine.UIElements.Button btnPauseSimButton = Document.rootVisualElement.Q<UnityEngine.UIElements.Button>("PauseSimButton");

            if (WUInity.SIM.IsRunning)
            {
                WUInity.INSTANCE.TogglePause();

                _bSimulationPaused = !_bSimulationPaused;

                if (_bSimulationPaused)
                    btnPauseSimButton.text = "Continue simulation";
                else
                    btnPauseSimButton.text = "Pause simulation";
            }

        }

        private void BtnStartSim_clicked()
        {
            // Start and stop running simulation.
            if (!WUInity.SIM.IsRunning)
            {
                ResetOutputDisplayOptions();    // Reset display options:        

                WUInityInput wO = WUInity.INPUT;

                WUInity.GUI.ParseMainData(wO);
                if (!WUInity.DATA_STATUS.CanRunSimulation())
                {
                    WUInity.LOG(WUInity.LogType.Error, " Could not start simulation, see error log.");
                }
                else
                {
                    //menuChoice = ActiveMenu.Output;
                    UnityEngine.UIElements.Button btnStartSimulation = Document.rootVisualElement.Q<UnityEngine.UIElements.Button>("StartSimButton");
                    
                    if (btnStartSimulation != null) btnStartSimulation.text = "Stop simulation";

                    WUInity.INSTANCE.StartSimulation();
                }
            }
            else
            {
                if (WUInity.INSTANCE.IsPaused())
                {
                    WUInity.INSTANCE.TogglePause();

                    _bSimulationPaused = false;

                    UnityEngine.UIElements.Button btnPauseSimButton = Document.rootVisualElement.Q<UnityEngine.UIElements.Button>("PauseSimButton");
                    btnPauseSimButton.text = "Pause simulation";
                }

                WUInity.INSTANCE.StopSimulation();

                UnityEngine.UIElements.Button btnStartSimulation = Document.rootVisualElement.Q<UnityEngine.UIElements.Button>("StartSimButton");
                if (btnStartSimulation != null) btnStartSimulation.text = "Start simulation";
            }

            /*
            if (EditorUtility.DisplayDialog("Function call required", "Call Jonathan's start simulation code entry point", "Yes", "No"))
                print("Pressed Yes.");
            else
                print("Pressed No.");
            */
            //MessageBox.Show("Function call required", "Call Jonathan's start simulation code entry point");
        }

        /// <summary>
        /// Set up the callback for when "place Mapbox API access token in folder" is called.
        /// </summary>
        /// <param name="root"></param>
        private void SetupPlaceMapboxTokenInFolderTask(VisualElement root)
        {
            // Catch toggle-click
            Toggle togAddTokenToMapbox = root.Q<Toggle>("TogAddTokenToMapbox");
            if (togAddTokenToMapbox != null)
                togAddTokenToMapbox.RegisterValueChangedCallback(evt =>
                {
                    bool addTokenToMapbox = evt.newValue;
                    if (addTokenToMapbox)
                    {
                        string initialPath = WUInity.DATA_FOLDER;

                        if (WUInity.DATA_STATUS.HaveInput)
                        {
                            initialPath = Path.GetDirectoryName(WUInity.WORKING_FOLDER);
                            initialPath = Path.Combine(initialPath, WUInity.INPUT.Simulation.SimulationID);
                        }

                        String FilePath = initialPath;
                        System.Diagnostics.Process.Start("Explorer.exe", @"/select," + FilePath);
                    }
                    UnityEngine.Debug.Log($"Place in folder = {evt.newValue}");
                });
        }

        /// <summary>
        /// Set up the callback for loading the mapbox token task
        /// </summary>
        /// <param name="root"></param>
        private void SetupDownloadMapboxTokenTask(VisualElement root)
        {
            // Catch toggle-click "Download Mapbox API access token" data
            Toggle togGetMapboxToken = root.Q<Toggle>("TogGetMapboxToken");
            if (togGetMapboxToken != null)
                togGetMapboxToken.RegisterValueChangedCallback(evt =>
                {
                    bool getMapboxToken = evt.newValue;
                    if (getMapboxToken)
                        System.Diagnostics.Process.Start("https://account.mapbox.com/");
                    UnityEngine.Debug.Log($"Download GPW file = {getMapboxToken}");
                });
        }

        /// <summary>
        /// Setup the callback for selecting the 'place gpw in folder'task
        /// </summary>
        /// <param name="root"></param>
        private void SetupPlaceGPWInCaseFolder(VisualElement root)
        {
            // Catch toggle-click
            Toggle togTogPlaceGPWData = root.Q<Toggle>("TogPlaceGPWData");

            if (togTogPlaceGPWData != null) {

                togTogPlaceGPWData.SetEnabled(false);

                togTogPlaceGPWData.RegisterValueChangedCallback(evt =>
                {
                    bool placeGPWData = evt.newValue;
                    if (placeGPWData)
                    {
                        string initialPath = WUInity.DATA_FOLDER;

                        if (WUInity.DATA_STATUS.HaveInput)
                        {
                            initialPath = Path.GetDirectoryName(WUInity.WORKING_FOLDER);
                            initialPath = Path.Combine(initialPath, WUInity.INPUT.Simulation.SimulationID);
                        }

                        String FilePath = Path.Combine(initialPath, "GPW");

                        if (!File.Exists(FilePath))
                            System.IO.Directory.CreateDirectory(FilePath);

                        System.Diagnostics.Process.Start("Explorer.exe", @"/select," + FilePath);
                    }
                    UnityEngine.Debug.Log($"Place in case folder = {evt.newValue}");
                });
            }
        }

        /// <summary>
        /// Set up the callback for downloading the selected gridded population of the world
        /// </summary>
        /// <param name="root"></param>
        private void SetupDownloadGPWTask(VisualElement root)
        {
            // Catch toggle-click "Download gridded population of the world" data
            Toggle togDownloadGPW = root.Q<Toggle>("TogDownloadGPW");

            if (togDownloadGPW != null)
            {
                togDownloadGPW.RegisterValueChangedCallback(evt =>
                {
                    bool downloadGPW = evt.newValue;
                    Toggle togTogPlaceGPWData = root.Q<Toggle>("TogPlaceGPWData");

                    if (downloadGPW)
                    {
                        System.Diagnostics.Process.Start("https://sedac.ciesin.columbia.edu/data/collection/gpw-v4");
                        if (togTogPlaceGPWData != null) togTogPlaceGPWData.SetEnabled(true);
                    }
                    else
                    {
                        if (togTogPlaceGPWData != null)
                        {
                            togTogPlaceGPWData.SetValueWithoutNotify(false);
                            togTogPlaceGPWData.SetEnabled(false);
                        }
                    }
                    UnityEngine.Debug.Log($"Download GPW file = {downloadGPW}");
                });
            }
        }

        /// <summary>
        /// Setup the callback for selecting the 'place OSM in folder'task
        /// </summary>
        /// <param name="root"></param>
        private void SetupPlaceOSMInCaseFolder(VisualElement root)
        {
            // Catch toggle-click
            Toggle togTogPlaceOSMData = root.Q<Toggle>("TogPlaceOSMData");

            if (togTogPlaceOSMData != null)
            {

                togTogPlaceOSMData.SetEnabled(false);

                togTogPlaceOSMData.RegisterValueChangedCallback(evt =>
                {
                    bool placeOSMData = evt.newValue;
                    if (placeOSMData)
                    {
                        string initialPath = WUInity.DATA_FOLDER;

                        if (WUInity.DATA_STATUS.HaveInput)
                        {
                            initialPath = Path.GetDirectoryName(WUInity.WORKING_FOLDER);
                            initialPath = Path.Combine(initialPath, WUInity.INPUT.Simulation.SimulationID);
                        }

                        String FilePath = Path.Combine(initialPath, "OSM");

                        if (!File.Exists(FilePath))
                            System.IO.Directory.CreateDirectory(FilePath);

                        System.Diagnostics.Process.Start("Explorer.exe", @"/select," + FilePath);
                    }
                    UnityEngine.Debug.Log($"Place in case folder = {evt.newValue}");
                });
            }
        }

        /// <summary>
        /// Set up the callback for downloading the OpenStreetMap data for selected region
        /// </summary>
        /// <param name="root"></param>
        private void SetupDownloadOSMTask(VisualElement root)
        {
            // Catch toggle-click "Download gridded population of the world" data
            Toggle togTogDownloadOSM = root.Q<Toggle>("TogDownloadOSM");
            if (togTogDownloadOSM != null)
            {
                togTogDownloadOSM.RegisterValueChangedCallback(evt =>
                {
                    bool downloadOSM = evt.newValue;
                    Toggle togTogPlaceOSMData = root.Q<Toggle>("TogPlaceOSMData");

                    if (downloadOSM)
                    {
                        System.Diagnostics.Process.Start("https://download.geofabrik.de/index.html");
                        if (togTogPlaceOSMData != null) togTogPlaceOSMData.SetEnabled(true);
                    }
                    else
                    {
                        if (togTogPlaceOSMData != null)
                        {
                            togTogPlaceOSMData.SetValueWithoutNotify(false);
                            togTogPlaceOSMData.SetEnabled(false);
                        }
                    }
                    
                    UnityEngine.Debug.Log($"Download OSM data file = {downloadOSM}");
                });
            }
        }

        private void SetupMapConfiguration(VisualElement root)
        {
            
            UnityEngine.UIElements.TextField tfTxTSetLatLong = root.Q<UnityEngine.UIElements.TextField>("TxTSetLatLong");
            if (tfTxTSetLatLong != null)
                tfTxTSetLatLong.RegisterValueChangedCallback((evt) =>
                {
                    UnityEngine.Debug.Log($"MapLatLong has changed to {evt.newValue}.");
                    _mapLLLatLong = evt.newValue;
                });

            UnityEngine.UIElements.TextField tfTxTSetMapSize = root.Q<UnityEngine.UIElements.TextField>("TxTSetMapSize");
            if (tfTxTSetMapSize != null)
                tfTxTSetMapSize.RegisterValueChangedCallback((evt) =>
                {
                    UnityEngine.Debug.Log($"MapSize has changed to {evt.newValue}.");
                    _mapSizeXY = evt.newValue;
                });

            UnityEngine.UIElements.TextField tfTxTSetMapZoomLevel = root.Q<UnityEngine.UIElements.TextField>("TxTSetMapZoomLevel");
            if (tfTxTSetMapZoomLevel != null)
                tfTxTSetMapZoomLevel.RegisterValueChangedCallback((evt) =>
                {
                    UnityEngine.Debug.Log($"MapZoomLevel has changed to {evt.newValue}.");
                    _mapZoomLevel = evt.newValue;
                });

            UnityEngine.UIElements.Button btnUpdateMap = root.Q<UnityEngine.UIElements.Button>("UpdateMap");
            if (btnUpdateMap != null)
                btnUpdateMap.clicked += BtnUpdateMap_clicked;
        }

        private void SetupGlobalGPWFolder(VisualElement root)
        {
            Toggle togTogSetGPWFolder = root.Q<Toggle>("TogSetGPWFolder");

            if (togTogSetGPWFolder != null)
                togTogSetGPWFolder.RegisterValueChangedCallback(evt =>
                {
                    //togTogSetGPWFolder.value = WUInity.DATA_STATUS.GlobalGPWAvailable;
                    //if (Directory.Exists(WUInity.INPUT.Population.gpwDataFolder)) togTogSetGPWFolder.value = true;
                    //if (WUInity.INPUT.Population.gpwDataFolder.Length>0) togTogSetGPWFolder.value = true;

                    //UnityEngine.Debug.Log($"togTogSetGPWFolder = {evt.newValue}");

                    if (evt.newValue) // Set GPW folder 
                    {
                        string initialPath = WUInity.DATA_FOLDER;

                        if (WUInity.DATA_STATUS.HaveInput)
                        {
                            initialPath = Path.GetDirectoryName(WUInity.WORKING_FOLDER);
                            initialPath = Path.Combine(initialPath, WUInity.INPUT.Simulation.SimulationID);
                        }

                        FileBrowser.ShowLoadDialog(SetGlobalGPWFileFoler, CancelGlobalGPWFolderSelection, FileBrowser.PickMode.Folders, false, initialPath, null, "Specify global GPW files folder", "Select");
                    }
                    else
                    {   // Clear GPW folder 
                        Label togLabel = root.Q<Label>("TxtGPWFolder");
                        if (togLabel != null) togLabel.text = "GPW folder: not set";
                    }
                });
        }

        void SetGlobalGPWFileFoler(string[] paths)
        {
            UnityEngine.Debug.Log($"TogMultipleSim = {paths[0]}");
            string filePath = "The path does not contain valid GPW files. Please select again.";

            if (WUInity.RUNTIME_DATA.Population.LoadGlobalGPWFolder(paths[0], true))
            {
                if (paths[0].Length < iGPWfolderLength)
                    filePath = "GPW folder: " + paths[0];
                else
                    filePath = "GPW folder: ..." + paths[0].Substring(paths[0].Length - iGPWfolderLength);
            }
            else
            {
                Toggle togTogSetGPWFolder = Document.rootVisualElement.Q<Toggle>("TogSetGPWFolder");
                if (togTogSetGPWFolder != null) togTogSetGPWFolder.value = false;
            }

            Label togLabel = Document.rootVisualElement.Q<Label>("TxtGPWFolder");
            if (togLabel != null) togLabel.text = filePath;
        }

        void CancelGlobalGPWFolderSelection()
        {
            SetGlobalGPWFileFoler();
        }
        private void PopulateFromLocalPopFile(VisualElement root) //TogPopulateFromPOP
        {
            Toggle togTogPopulateFromPOP = root.Q<Toggle>("TogPopulateFromPOP");

            if (togTogPopulateFromPOP != null)
            {
                togTogPopulateFromPOP.RegisterValueChangedCallback(evt =>
                {
                    bool checkPopOK = evt.newValue;
                    if (checkPopOK)
                    {
                        string[] popFilter = new string[] { ".pop" };
                        FileBrowser.SetFilters(false, popFilter);
                        string initialPath = WUInity.DATA_FOLDER;

                        if (WUInity.DATA_STATUS.HaveInput)
                        {
                            initialPath = Path.GetDirectoryName(WUInity.WORKING_FOLDER);
                            initialPath = Path.Combine(initialPath, WUInity.INPUT.Simulation.SimulationID);
                        }

                        FileBrowser.ShowLoadDialog(LoadPopulationFile, CancelLoadPopulationFile, FileBrowser.PickMode.Files, false, initialPath, null, "Load POP file", "Load");
                    }
                    else
                    {
                        Label togLabel = root.Q<Label>("TxtPOPFile");
                        if (togLabel != null) togLabel.text = "POP file: not set";
                    }
                    
                    UnityEngine.Debug.Log($"Load POP file = {checkPopOK}");
                });
            }

        }

        void LoadPopulationFile(string[] paths)
        {
            string filePath = "POP file: Failed to load .pop file.";

            if (WUInity.POPULATION.LoadPopulationFromFile(paths[0], true))
            {
                filePath = "POP file: "+ Path.GetFileName(paths[0]) + " is loaded successfully.";
                
                WUInity.INPUT.Population.populationFile = Path.GetFileName(paths[0]);   // There is a bug in Population\EvacuationData.cs between lines 274-280 in saving inputs. 
                WUInityInput.SaveInput();                                               // This is a temp fix.
            }
            else
            {
                Toggle togTogPopulateFromPOP = Document.rootVisualElement.Q<Toggle>("TogPopulateFromPOP");
                if (togTogPopulateFromPOP != null) togTogPopulateFromPOP.value = false;
            }

            Label togLabel = Document.rootVisualElement.Q<Label>("TxtPOPFile");
            if (togLabel != null) togLabel.text = filePath;

            SetPopulationAndCellNumber();
        }

        void CancelLoadPopulationFile()
        {
            SetPopulationFile();    // Reset to the orignal POP file if there is one.
        }

        private void PopulateFromLocalGPWFile(VisualElement root) //TogPopulateFromGPW
        {
            Toggle togPopulateFromGPW = root.Q<Toggle>("TogPopulateFromGPW");
            if (togPopulateFromGPW != null)
            {
                togPopulateFromGPW.RegisterValueChangedCallback(evt =>
                {
                    bool populateFromGPW = evt.newValue;
                    if (populateFromGPW)
                    {
                        FileBrowser.SetFilters(false, fileFilter[(int)FileType.GPWFile]);
                        string initialPath = WUInity.DATA_FOLDER;

                        if (WUInity.DATA_STATUS.HaveInput)
                        {
                            initialPath = Path.GetDirectoryName(WUInity.WORKING_FOLDER);
                            initialPath = Path.Combine(initialPath, WUInity.INPUT.Simulation.SimulationID);
                        }

                        FileBrowser.ShowLoadDialog(LoadLocalGPWFile, CancelLoadLocalGPWFile, FileBrowser.PickMode.Files, false, initialPath, null, "Load GPW file", "Load");
                    }
                    else
                    {
                        Label togLabel = root.Q<Label>("TxtGPWFile");
                        if (togLabel != null) togLabel.text = "GPW file: not set";
                    }

                    UnityEngine.Debug.Log($"Load local GPW file = {populateFromGPW}");
                });
            }
        }

        void LoadLocalGPWFile(string[] paths)
        {
            string filePath = "GPW file: Failed to load local .gpw file.";

            if (WUInity.POPULATION.CreatePopulationFromLocalGPW(paths[0]))
            {
                filePath = "GPW file: " + Path.GetFileName(paths[0]) + " is loaded and a .pop is created.";

                WUInity.INPUT.Population.populationFile = Path.GetFileName(paths[0]);   // There is a bug in Population\EvacuationData.cs between lines 274-280 in saving inputs. 
                WUInityInput.SaveInput();                                               // This is a temp fix.
            }
            else
            {
                Toggle togTogPopulateFromGPW = Document.rootVisualElement.Q<Toggle>("TogPopulateFromGPW");
                if (togTogPopulateFromGPW != null) togTogPopulateFromGPW.value = false;
            }

            Label togLabel = Document.rootVisualElement.Q<Label>("TxtGPWFile");
            if (togLabel != null) togLabel.text = filePath;

            SetLocalGPWNumber();
        }

        void CancelLoadLocalGPWFile()
        {
            SetLocalGPWFile();    // Reset to the orignal local GPW file if there is one.
        }

        private void SetupLoadLCPFile(VisualElement root)   // Catch toggle-click "Load landscape file" 
        {
            Toggle togLoadLCP = root.Q<Toggle>("TogLoadLCPFile");
            if (togLoadLCP != null)
            {
                togLoadLCP.RegisterValueChangedCallback(evt =>
                {
                    if (evt.newValue)
                    {
                        FileBrowser.SetFilters(false, fileFilter[(int)FileType.lcpFile]);
                        FileBrowser.ShowLoadDialog(LoadLCPFile, CancelLoadLCPFile, FileBrowser.PickMode.Files, false, GetProjectPath(), null, "Load LCP file", "Load");
                    }
                    else
                    {
                        Label togLabel = Document.rootVisualElement.Q<Label>("TxtLCPFile");
                        if (togLabel != null) togLabel.text = "LCP file: not set";
                    }

                    UnityEngine.Debug.Log($"Load LCP file = {evt.newValue}");
                });
            }
        }

        void LoadLCPFile(string[] paths)
        {
            string loadStatus = "LCP file load error.";

            if (WUInity.RUNTIME_DATA.Fire.LoadLCPFile(paths[0], true))
            {
                loadStatus = "LCP file: " + Path.GetFileName(paths[0])+ " is loaded successfully.";
            }
            else
            {
                Toggle togLoadLCP = Document.rootVisualElement.Q<Toggle>("TogLoadLCPFile");
                if (togLoadLCP != null) togLoadLCP.value = false;
            }

            Label togLabel = Document.rootVisualElement.Q<Label>("TxtLCPFile");
            if (togLabel != null) togLabel.text = loadStatus;

        }

        void CancelLoadLCPFile()
        {
            SetLCPFile();
        }

        private void SetupLoadFuelModelFile(VisualElement root) // Catch toggle-click "Load fule model file
        {
            Toggle togLoadFuel = root.Q<Toggle>("TogLoadFuelModelFile");
            if (togLoadFuel != null)
            {
                togLoadFuel.RegisterValueChangedCallback(evt =>
                {
                    if (evt.newValue)
                    {
                        FileBrowser.SetFilters(false, fileFilter[(int)FileType.fuelModelsFile]);
                        FileBrowser.ShowLoadDialog(LoadFuelModelFile, CancelLoadFuelModelFile, FileBrowser.PickMode.Files, false, GetProjectPath(), null, "Load fuel model file", "Load");
                    }
                    else
                    {
                        Label togLabel = Document.rootVisualElement.Q<Label>("TxtFuelModelFile");
                        if (togLabel != null) togLabel.text = "Fuel model file: not set";
                    }

                    UnityEngine.Debug.Log($"Load fuel model file = {evt.newValue}");
                });
            }
        }

        void LoadFuelModelFile(string[] paths)
        {
            string loadStatus = "Fuel model file load error.";

            if (WUInity.RUNTIME_DATA.Fire.LoadFuelModelsInput(paths[0], true))
            {
                loadStatus = "Fuel model file: " + Path.GetFileName(paths[0]) + " is loaded successfully.";
            }
            else
            {
                Toggle togTogLoadFuelModelFile = Document.rootVisualElement.Q<Toggle>("TogLoadFuelModelFile");
                if (togTogLoadFuelModelFile != null) togTogLoadFuelModelFile.value = false;
            }

            Label togLabel = Document.rootVisualElement.Q<Label>("TxtFuelModelFile");
            if (togLabel != null) togLabel.text = loadStatus;
        }

        void CancelLoadFuelModelFile()
        {
            SetFuelModelFile();
        }

        private void SetupLoadFuelMoistureFile(VisualElement root) // Catch toggle-click ""
        {      
            Toggle togLoadFuelMoisture = root.Q<Toggle>("TogLoadFuelMoistureFile");
            if (togLoadFuelMoisture != null)
            {
                togLoadFuelMoisture.RegisterValueChangedCallback(evt =>
                {
                    if (evt.newValue)
                    {
                        FileBrowser.SetFilters(false, fileFilter[(int)FileType.initialFuelMoistureFile]);
                        FileBrowser.ShowLoadDialog(LoadFuelMoistureFile, CancelLoadFuelMoistureFile, FileBrowser.PickMode.Files, false, GetProjectPath(), null, "Load fuel moisture file", "Load");
                    }
                    else
                    {
                        Label togLabel = Document.rootVisualElement.Q<Label>("TxtFuelMoistureFile");
                        if (togLabel != null) togLabel.text = "Fuel moisture file: not set";
                    }

                    UnityEngine.Debug.Log($"Load fuel moisture file = {evt.newValue}");
                });
            }
        }

        void LoadFuelMoistureFile(string[] paths)
        {
            string loadStatus = "Fuel moisture file load error.";

            if (WUInity.RUNTIME_DATA.Fire.LoadInitialFuelMoistureData(paths[0], true))
            {
                loadStatus = "Fuel moisture file: " + Path.GetFileName(paths[0]) + " is loaded successfully.";
            }
            else
            {
                Toggle togTogLoadFuelMoistureFile = Document.rootVisualElement.Q<Toggle>("TogLoadFuelMoistureFile");
                if (togTogLoadFuelMoistureFile != null) togTogLoadFuelMoistureFile.value = false;
            }

            Label togLabel = Document.rootVisualElement.Q<Label>("TxtFuelMoistureFile");
            if (togLabel != null) togLabel.text = loadStatus;

        }

        void CancelLoadFuelMoistureFile()
        {
            SetFuelMoistureFile();
        }

        private void SetupLoadWeatherFile(VisualElement root) // Catch toggle-click ""
        {
            Toggle togLoadWeatherFile = root.Q<Toggle>("TogLoadWeatherFile");
            if (togLoadWeatherFile != null)
            {
                togLoadWeatherFile.RegisterValueChangedCallback(evt =>
                {
                    if (evt.newValue)
                    {
                        FileBrowser.SetFilters(false, fileFilter[(int)FileType.weatherFile]);
                        FileBrowser.ShowLoadDialog(LoadWeatherFile, CancelLoadWeatherFile, FileBrowser.PickMode.Files, false, GetProjectPath(), null, "Load weather file", "Load");
                    }
                    else
                    {
                        Label togLabel = Document.rootVisualElement.Q<Label>("TxtWeatherFile");
                        if (togLabel != null) togLabel.text = "Weather file: not set";
                    }

                    UnityEngine.Debug.Log($"Load weather file = {evt.newValue}");
                });
            }
        }

        void LoadWeatherFile(string[] paths)
        {
            string loadStatus = "Weather file load error.";

            if (WUInity.RUNTIME_DATA.Fire.LoadWeatherInput(paths[0], true))
            {
                loadStatus = "Weather file: " + Path.GetFileName(paths[0]) + " is loaded successfully.";
            }
            else
            {
                Toggle togLoadWeatherFile = Document.rootVisualElement.Q<Toggle>("TogLoadWeatherFile");
                if (togLoadWeatherFile != null) togLoadWeatherFile.value = false;
            }

            Label togLabel = Document.rootVisualElement.Q<Label>("TxtWeatherFile");
            if (togLabel != null) togLabel.text = loadStatus;

        }

        void CancelLoadWeatherFile()
        {
            SetWeatherFile();
        }

        private void SetupLoadWindFile(VisualElement root) // Catch toggle-click ""
        {
            Toggle togLoadWindFile = root.Q<Toggle>("TogLoadWindFile");
            if (togLoadWindFile != null)
            {
                togLoadWindFile.RegisterValueChangedCallback(evt =>
                {
                    if (evt.newValue)
                    {
                        FileBrowser.SetFilters(false, fileFilter[(int)FileType.windFile]);
                        FileBrowser.ShowLoadDialog(LoadWindFile, CancelLoadWindFile, FileBrowser.PickMode.Files, false, GetProjectPath(), null, "Load wind file", "Load");
                    }
                    else
                    {
                        Label togLabel = Document.rootVisualElement.Q<Label>("TxtWindFile");
                        if (togLabel != null) togLabel.text = "Wind file: not set";
                    }

                    UnityEngine.Debug.Log($"Load wind file = {evt.newValue}");
                });
            }
        }

        void LoadWindFile(string[] paths)
        {
            string loadStatus = "Wind file load error.";

            if (WUInity.RUNTIME_DATA.Fire.LoadWindInput(paths[0], true))
            {
                loadStatus = "Wind file: " + Path.GetFileName(paths[0]) + " is loaded successfully.";
            }
            else
            {
                Toggle togLoadWindFile = Document.rootVisualElement.Q<Toggle>("TogLoadWindFile");
                if (togLoadWindFile != null) togLoadWindFile.value = false;
            }

            Label togLabel = Document.rootVisualElement.Q<Label>("TxtWindFile");
            if (togLabel != null) togLabel.text = loadStatus;

        }

        void CancelLoadWindFile()
        {
            SetWindFile();
        }

        private void SetupLoadIgnitionPointsFile(VisualElement root)
        {
            Toggle togLoadIgnitionPointsFile = root.Q<Toggle>("TogLoadIgnitionPointsFile");
            if (togLoadIgnitionPointsFile != null)
            {
                togLoadIgnitionPointsFile.RegisterValueChangedCallback(evt =>
                {
                    if (evt.newValue)
                    {
                        FileBrowser.SetFilters(false, fileFilter[(int)FileType.ignitionPointsFile]);
                        FileBrowser.ShowLoadDialog(LoadIgnitionPointsFile, CancelIgnitionPointsFile, FileBrowser.PickMode.Files, false, GetProjectPath(), null, "Load ignition points file", "Load");
                    }
                    else
                    {
                        Label togLabel = Document.rootVisualElement.Q<Label>("TxtIgnitionPointsFile");
                        if (togLabel != null) togLabel.text = "Ignition points file: not set";
                    }

                    UnityEngine.Debug.Log($"Load ignition points file = {evt.newValue}");
                });
            }
        }

        void LoadIgnitionPointsFile(string[] paths)
        {
            string loadStatus = "Ignition points file load error.";

            if (WUInity.RUNTIME_DATA.Fire.LoadIgnitionPoints(paths[0], true))
            {
                loadStatus = "Ignition points file: " + Path.GetFileName(paths[0]) + " is loaded successfully.";
            }
            else
            {
                Toggle togLoadIgnitionPointsFile = Document.rootVisualElement.Q<Toggle>("TogLoadIgnitionPointsFile");
                if (togLoadIgnitionPointsFile != null) togLoadIgnitionPointsFile.value = false;
            }

            Label togLabel = Document.rootVisualElement.Q<Label>("TxtIgnitionPointsFile");
            if (togLabel != null) togLabel.text = loadStatus;
        }

        void CancelIgnitionPointsFile()
        {
            SetIgnitionPointsFile();
        }

        private void SetupLoadGraphicalFireInputFile(VisualElement root)
        {
            Toggle ogLoadGraphicalFireInputFile = root.Q<Toggle>("TogLoadGraphicalFireInputFile");
            if (ogLoadGraphicalFireInputFile != null)
            {
                ogLoadGraphicalFireInputFile.RegisterValueChangedCallback(evt =>
                {
                    if (evt.newValue)
                    {
                        FileBrowser.SetFilters(false, fileFilter[(int)FileType.graphicalFireInputFile]);
                        FileBrowser.ShowLoadDialog(LoadGraphicalFireInputFile, CancelGraphicalFireInputFile, FileBrowser.PickMode.Files, false, GetProjectPath(), null, "Load graphical fire input file", "Load");
                    }
                    else
                    {
                        Label togLabel = Document.rootVisualElement.Q<Label>("TxtGraphicalFireInputFile");
                        if (togLabel != null) togLabel.text = "Graphical fire input file: not set";
                    }

                    UnityEngine.Debug.Log($"Load graphical fire input file = {evt.newValue}");
                });
            }
        }

        void LoadGraphicalFireInputFile(string[] paths)
        {
            string loadStatus = "Graphical fire input file load error.";

            if (WUInity.RUNTIME_DATA.Fire.LoadGraphicalFireInput(paths[0], true))
            {
                loadStatus = "Graphical fire input file: " + Path.GetFileName(paths[0]) + " is loaded successfully.";
            }
            else
            {
                Toggle togLoadGraphicalFireInputFile = Document.rootVisualElement.Q<Toggle>("TogLoadGraphicalFireInputFile");
                if (togLoadGraphicalFireInputFile != null) togLoadGraphicalFireInputFile.value = false;
            }

            Label togLabel = Document.rootVisualElement.Q<Label>("TxtGraphicalFireInputFile");
            if (togLabel != null) togLabel.text = loadStatus;
        }

        void CancelGraphicalFireInputFile()
        {
            SetGraphicalFireInputFile();
        }
        private void SetupVewFireFileButtons(VisualElement root)
        {
            UnityEngine.UIElements.Button btnDisplayLCPFile = root.Q<UnityEngine.UIElements.Button>("DisplayLCPFile");
            if (btnDisplayLCPFile != null)
                btnDisplayLCPFile.clicked += BtnDisplayLCPFile;

            UnityEngine.UIElements.Button btnVewFuelModelFile = root.Q<UnityEngine.UIElements.Button>("VewFuelModelFile");
            if (btnVewFuelModelFile != null) 
                btnVewFuelModelFile.clicked += () => BtnViewFireFile(FileType.fuelModelsFile);

            UnityEngine.UIElements.Button btnVewInitialFuelMoistureFile = root.Q<UnityEngine.UIElements.Button>("VewInitialFuelMoistureFile");
            if (btnVewInitialFuelMoistureFile != null) 
                btnVewInitialFuelMoistureFile.clicked += () => BtnViewFireFile(FileType.initialFuelMoistureFile);

            UnityEngine.UIElements.Button btnVewWeatherFile = root.Q<UnityEngine.UIElements.Button>("VewWeatherFile");
            if (btnVewWeatherFile != null) 
                btnVewWeatherFile.clicked += () => BtnViewFireFile(FileType.weatherFile);

            UnityEngine.UIElements.Button btnVewWindFile = root.Q<UnityEngine.UIElements.Button>("VewWindFile");
            if (btnVewWindFile != null) 
                btnVewWindFile.clicked += () => BtnViewFireFile(FileType.windFile);

            UnityEngine.UIElements.Button btnVewIgnitionPointsFile = root.Q<UnityEngine.UIElements.Button>("VewIgnitionPointsFile");
            if (btnVewIgnitionPointsFile != null)
                btnVewIgnitionPointsFile.clicked += () => BtnViewFireFile(FileType.ignitionPointsFile);
        }

        void BtnDisplayLCPFile()
        {
            WUInity.RUNTIME_DATA.Fire.ToggleLCPDataPlane();
        }

        /// <summary>
        /// Initialise the basic workflow controls
        /// </summary>
        /// <param name="root"></param>
        private void InitWorkflowControl(VisualElement root)
        {
            //  Populate the foldout control --- This is a piece of test code. 
            VisualElement workflowControl = root.Q<VisualElement>("WorkflowFoldout");
            if (workflowControl != null)
            {
                workflowControl.AddToClassList("show");
            }

            // Register mouse hovering behaviour to show help tooltips
            root.RegisterCallback<MouseEnterEvent>(OnMouseEnter, TrickleDown.TrickleDown);
        }

        private void OnMouseEnter(MouseEnterEvent evt)
        {
            VisualElement targetElement = (VisualElement)evt.target;
            if (targetElement != null)
            {
                if (targetElement.name.Length > 0)
                {
                    // Now, populate a label control with the target element tooltip text
                    String showString = targetElement.tooltip;

                    Label tipsLabel = Document.rootVisualElement.Q<Label>("TipsLabel");
                    if (tipsLabel != null && showString.Length > 0 )
                    {
                        tipsLabel.text = showString;
                    }

                    // debugging only:
                    //UnityEngine.Debug.Log($"Mouse is now over element = {targetElement.name}:{targetElement.name.Length}:{showString}");
                }
                //else UnityEngine.Debug.Log("Mouse is over unnamed element");
            }
            //else UnityEngine.Debug.Log("Mouse is over null element");
        }

        /// <summary>
        /// Initialise the "Options" button and UI action (show/hide options controls)
        /// </summary>
        /// <param name="root"></param>
        private void InitOptionsControls(VisualElement root)
        {
            // Create a var for the options container element, which is the parent element for the slider control
            VisualElement optionsContainer = root.Q<VisualElement>("OptionsContainer");
            if (optionsContainer != null)
            {
                // Add the "hide" style to the list fo styles for the class
                optionsContainer.AddToClassList("hide");

                // Create  button var to access the options button
                UnityEngine.UIElements.Button optionsButton = root.Q<UnityEngine.UIElements.Button>("OptionsButton");

                // Add a handler to the .clicked event, toggling the hide class on/off for the button
                optionsButton.clicked += () => optionsContainer.ToggleInClassList("hide");
            }
        }

        /// <summary>
        /// Initialise the options value slider
        /// </summary>
        /// <param name="root"></param>
        private void InitValueSlider(VisualElement root)
        {
            // Create a SliderInt variable to access the VolumeSlider control
            var valueSlider = root.Q<SliderInt>("VolumeSlider");

            // Use a function to respond to the callback from the slider control value being changed, 
            valueSlider.RegisterValueChangedCallback(evt => myConsoleLog(evt));
            // Anonymous function version: volumeSlider.RegisterValueChangedCallback(evt =>{ Debug.Log(evt.newValue); });
        }

        /// <summary>
        /// Write out text to the event log in the console, tracking the slider value
        /// </summary>
        /// <param name="evt">slider value</param>
        private void myConsoleLog(ChangeEvent<int> evt)
        {
            UnityEngine.Debug.Log($"Value={evt.newValue}");
        }

        // sample test code methods for sand-boxing methods, events etc.
        /// <summary>
        /// SAMPLE: Testing value/ref code
        /// </summary>
        /// <param name="options"></param>
        /// <param name="n"></param>
        /// <param name="x"></param>
        void test(List<string> options, ref object n, out object x)
        {
            options.Add("Hello yourself");
            if (n == null)
                n = new();
            x = new();
        }

        /// <summary>
        /// SAMPLE: Some basic sample test code for sand-boxing some methods: to delete at the end of experimentation
        /// </summary>
        private void RunSomeBasicTestCode()
        {
            // Some test code to check passing by ref, value etc.
            List<string> options = new List<string>();
            options.Add("Hello");
            object n = null;
            // var c = n??n.GetType(); null coalescing operator
            object x;
            test(options, ref n, out x);
            string[] optionsArray = options.ToArray();
        }
    }
}