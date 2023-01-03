using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using SimpleFileBrowser;
using System.IO;

namespace WUInity
{
    public class menu : MonoBehaviour
    {
        public UIDocument Document;

        bool newUIMenuDirty = false;
        private int iGPWfolderLength = 80;

        bool mainMenuDirty = true, creatingNewFile = false;

        // Associated variables with location/size/zoom level of map
        string mapLLLatLong, mapSizeXY, mapZoomLevel;

        public enum FireFile { fuelModelsFile, initialFuelMoistureFile, weatherFile, windFile, ignitionPointsFile }

        // example code of an implementation of stored variable, wrapped with a get/set property protocol
        private int iTestMemberVar = 4;
        public int iTestProperty
        {
            get => iTestMemberVar;
            set => iTestMemberVar = Math.Min(0, value);
        }
        // Can now be called as: iTestProperty = 2; (to set it),
        // and the "get" is: int n = menu::iTestProperty; // after the class definition, using the menu:: class object

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
            UnityEngine.Debug.Log("menu::Start();}");
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
            //UnityEngine.Debug.Log("menu::Update;}");
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// My custom methods for hadling GUI contriols setup and actions
        /// 

        /// <summary>
        /// Manually created methods for invoking workflow actions
        /// </summary>
        private void InitialiseWorkflow()
        {
            // Reference the root visual element for the UI "document"
            var root = Document.rootVisualElement;

            // Dock new GUI box to the right edge.
            UnityEngine.UIElements.VisualElement newGUI = root.Q<UnityEngine.UIElements.VisualElement>("root");
            newGUI.style.left = Screen.width - 500;
            newGUI.style.height = Screen.height - 20 - 160;
            newGUI.style.top = 20;

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
                SetGlobalGPWFolder(root);
                PopulateFromLocalGPWFile(root);
                PopulateFromLocalPopFile(root);

                UnityEngine.UIElements.Button btnShowHideLocalGPW = root.Q<UnityEngine.UIElements.Button>("ShowHideLocalGPW");
                if (btnShowHideLocalGPW != null)
                    btnShowHideLocalGPW.clicked += BtnShowHideLocalGPW_clicked;

                UnityEngine.UIElements.Button btnShowHideLocalPOP = root.Q<UnityEngine.UIElements.Button>("ShowHideLocalPOP");
                if (btnShowHideLocalPOP != null)
                    btnShowHideLocalPOP.clicked += BtnShowHideLocalPOP_clicked;

                // 4. Evacuation goals ----------------------------------------------------------------------------------------------
                SetupAddRemoveEvacGoalButtons(root);
                InitEvacGoalList(root);

                // 5. Evacuation group(s) and response ------------------------------------------------------------------------------
                InitResponseCurveList(root);
                InitEvacGroupList(root);

                SetupEvacGroupRespButtons(root);

                // 6. Evacuation settings -------------------------------------------------------------------------------------------

                // 7. Routing data --------------------------------------------------------------------------------------------------

                // 8. Traffic

                // 9. Landscape data

                // 10. Fire characteriestics
                SetupLoadLCPFile(root);
                SetupLoadFuelModelFile(root);
                SetupLoadFuelMoistureFile(root);

                SetupVewFireFileButtons(root);

                // 11. Check external fire references

                // 12. Running simulation settings
                RegisterSimulationToggles(root);

                // 13. Execute simulation
                // Activate "Run simulation" callback for button-click event
                SetupRunSimulationTask(root);

                UnityEngine.UIElements.Button switchGUIButton = root.Q<UnityEngine.UIElements.Button>("SwitchGUI");
                switchGUIButton.clicked += () => BtnSwitchGUIButton_clicked(100);

                // Add the handler to the quit button
                UnityEngine.UIElements.Button quitButton = root.Q<UnityEngine.UIElements.Button>("QuitButton");
                quitButton.clicked += () => Application.Quit();

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

        private void InitFoldoutSwitch(VisualElement root)
        {
            UnityEngine.UIElements.Button btnFoldoutSwitchButton = root.Q<UnityEngine.UIElements.Button>("FoldoutSwitchButton");

            if (btnFoldoutSwitchButton != null)
                btnFoldoutSwitchButton.clicked += BtnFoldoutSwitchButton;



            UnityEngine.UIElements.Button btnLogSwitchButton = root.Q<UnityEngine.UIElements.Button>("LogSwitchButton");
            if (btnLogSwitchButton != null)
                btnLogSwitchButton.clicked += BtnbtnLogSwitchButton_clicked;
        }

        private void BtnbtnLogSwitchButton_clicked()
        {
            var root = Document.rootVisualElement;
            UnityEngine.UIElements.VisualElement testGUI = root.Q<UnityEngine.UIElements.VisualElement>("TestME");

            testGUI.visible = !testGUI.visible;
        }

        private void BtnFoldoutSwitchButton()
        {
            var foldouts = Document.rootVisualElement.Query<Foldout>();
            foldouts.ForEach(SwitchFoldouts);
        }

        private void SwitchFoldouts(Foldout foldout)
        {
            foldout.value = !foldout.value;
        }

        private void BtnSwitchGUIButton_clicked(int clicknumber)
        {
            UnityEngine.Debug.Log($"Switch GUI = {clicknumber}");

            var root = Document.rootVisualElement;
            UnityEngine.UIElements.VisualElement newGUI = root.Q<UnityEngine.UIElements.VisualElement>("root");

            if (newGUI.style.left == 0) { 
                newGUI.style.left = Screen.width - 500;
                newGUI.style.top = 20;
                newGUI.style.height = Screen.height - 20;
                WUInity.GUI.enabled = true;
            }
            else { 
                newGUI.style.left = 0;
                newGUI.style.top = 0;
                newGUI.style.height = Screen.height;
                WUInity.GUI.enabled = false;
            }
            //newGUI.SetEnabled(false);
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
                btnEditGoalButton.clicked += BtnEditGoalButton;
        }

        private void BtnEditGoalButton()
        {
            UnityEngine.UIElements.DropdownField dfDfEvacutionDestination = Document.rootVisualElement.Q<UnityEngine.UIElements.DropdownField>("DfEvacutionDestination");

            if (dfDfEvacutionDestination != null && WUInity.DATA_STATUS.HaveInput && WUInity.RUNTIME_DATA.Evacuation.EvacuationGoals.Count > 0)
            {
                string initialPath = Path.GetDirectoryName(WUInity.WORKING_FOLDER);

                initialPath = Path.Combine(initialPath, WUInity.INPUT.Simulation.SimulationID);
                initialPath = Path.Combine(initialPath, WUInity.INPUT.Traffic.evacuationGoalFiles[dfDfEvacutionDestination.index] + ".ed");

                System.Diagnostics.Process.Start("Notepad.exe", initialPath);

                string message= string.Concat("Goal file [", Path.GetFileName(initialPath), "] is opened in Notepad.");
                EditorUtility.DisplayDialog(message, "Please remember to reload this goal file if you make and save any changes to the file in Notepad.", "Close");
            }            
            else
            {
                EditorUtility.DisplayDialog("No goal file is found", "Please create a new goal file and then load in Notepad.", "Close");
            }
        }

        private void BtnNewGoalButton_clicked()
        {
            // Add code here to create a new goal
        }

        private void BtnRemoveGoalButton_clicked()
        {
            //Update dropdown 

            if (EditorUtility.DisplayDialog("Remove current goal", "Do you want to remove the current goal?", "Confirm","Cancel")) {

                UnityEngine.UIElements.DropdownField dfDfEvacutionDestination = Document.rootVisualElement.Q<UnityEngine.UIElements.DropdownField>("DfEvacutionDestination");
                if (dfDfEvacutionDestination != null && dfDfEvacutionDestination.choices.Count >= 1)
                {
                    if (WUInity.RUNTIME_DATA.Evacuation.EvacuationGoals.Count > 1)
                    {
                        string[] newGoalList = new string[WUInity.INPUT.Traffic.evacuationGoalFiles.Length - 1];

                        for (int i = 0; i < dfDfEvacutionDestination.index; i++)
                        {
                            if (i != dfDfEvacutionDestination.index) newGoalList[i] = WUInity.INPUT.Traffic.evacuationGoalFiles[i];
                            else break;
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
        }

        private void BtnAddGoalButton_clicked()
        {
            string[] edFilter = new string[] { ".ed" };

            FileBrowser.SetFilters(false, edFilter);
            string initialPath = WUInity.DATA_FOLDER;

            if (WUInity.DATA_STATUS.HaveInput)
            {
                initialPath = Path.GetDirectoryName(WUInity.WORKING_FOLDER);
                initialPath = Path.Combine(initialPath, WUInity.INPUT.Simulation.SimulationID);
            }

            FileBrowser.ShowLoadDialog(LoadAEvacGoalFile, null, FileBrowser.PickMode.Files, false, initialPath, null, "Load evacuation goal file (.ed)", "Load");

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
                Color color = Color.white;

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
                        dfDfEvacutionDestination.choices.Contains(name);

                        dfDfEvacutionDestination.index = WUInity.INPUT.Traffic.evacuationGoalFiles.Length - 1;    // Show the most resent goal
                    }
                }
            }
        }

        private void SetupEvacGroupRespButtons(VisualElement root)
        {
            UnityEngine.UIElements.Button btnEditRespCurveButton = root.Q<UnityEngine.UIElements.Button>("EditRespCurveButton");

            if (btnEditRespCurveButton != null)
                btnEditRespCurveButton.clicked += BtnEditRespCurveButton;

            UnityEngine.UIElements.Button btnEditEvacGroupButton = root.Q<UnityEngine.UIElements.Button>("EditEvacGroupButton");

            if (btnEditEvacGroupButton != null)
                btnEditEvacGroupButton.clicked += BtnEditEvacGroupButton;
        }

        private void BtnVewFuelModelFile(string fileType)
        {
            if (WUInity.DATA_STATUS.HaveInput && WUInity.INPUT.Fire.fuelModelsFile.Length > 0)
            {
                string initialPath = Path.GetDirectoryName(WUInity.WORKING_FOLDER);

                initialPath = Path.Combine(initialPath, WUInity.INPUT.Simulation.SimulationID);
                initialPath = Path.Combine(initialPath, WUInity.INPUT.Fire.fuelModelsFile);

                System.Diagnostics.Process.Start("Notepad.exe", initialPath);

                string message = string.Concat("Fuel model file [", Path.GetFileName(initialPath), "] is opened in Notepad.");
                EditorUtility.DisplayDialog(message, "Please remember to reload this fuel model file if you make and save any changes to the file in Notepad.", "Close");
            }
            else
            {
                EditorUtility.DisplayDialog("No fuel model file is found", "Please create a new fuel model file and then load in Notepad.", "Close");
            }
        }

        private void BtnViewFireFile(FireFile fileType)
        {
            string fileName="";

            switch(fileType)
            {
                case FireFile.fuelModelsFile:
                    fileName = WUInity.INPUT.Fire.fuelModelsFile;            break;
                case FireFile.initialFuelMoistureFile:
                    fileName = WUInity.INPUT.Fire.initialFuelMoistureFile;   break;
                case FireFile.weatherFile:
                    fileName = WUInity.INPUT.Fire.weatherFile;               break;
                case FireFile.windFile:
                    fileName = WUInity.INPUT.Fire.windFile;                  break;
                case FireFile.ignitionPointsFile:
                    fileName = WUInity.INPUT.Fire.ignitionPointsFile;        break;
            }

            if (WUInity.DATA_STATUS.HaveInput && fileName.Length > 0)
            {
                string initialPath = Path.Combine(Path.GetDirectoryName(WUInity.WORKING_FOLDER), WUInity.INPUT.Simulation.SimulationID);
                initialPath = Path.Combine(initialPath, fileName);

                System.Diagnostics.Process.Start("Notepad.exe", initialPath);

                string message = "Fire model file ["+ Path.GetFileName(initialPath) + "] is opened in Notepad.";
                EditorUtility.DisplayDialog(message, "Please remember to reload this file into WUINITY if you make any changes in Notepad.", "Close");
            }
            else
            {
                EditorUtility.DisplayDialog("No fire model file is found", "Please create a new fire model file and then open in Notepad.", "Close");
            }
        }

        private void BtnEditRespCurveButton()
        {
            UnityEngine.UIElements.DropdownField dfDfResponseCurve = Document.rootVisualElement.Q<UnityEngine.UIElements.DropdownField>("DfResponseCurve");

            if (dfDfResponseCurve != null && WUInity.DATA_STATUS.HaveInput && WUInity.RUNTIME_DATA.Evacuation.ResponseCurves.Length > 0)
            {
                string initialPath = Path.GetDirectoryName(WUInity.WORKING_FOLDER);

                initialPath = Path.Combine(initialPath, WUInity.INPUT.Simulation.SimulationID);
                initialPath = Path.Combine(initialPath, WUInity.RUNTIME_DATA.Evacuation.ResponseCurves[dfDfResponseCurve.index].name + ".rsp");

                System.Diagnostics.Process.Start("Notepad.exe", initialPath);

                string message = string.Concat("Response curve file [", Path.GetFileName(initialPath), "] is opened in Notepad.");
                EditorUtility.DisplayDialog(message, "Please remember to reload this response curve file if you make and save any changes to the file in Notepad.", "Close");
            }
            else
            {
                EditorUtility.DisplayDialog("No response curve file is found", "Please create a new response curve file and then load in Notepad.", "Close");
            }
        }

        private void BtnEditEvacGroupButton()
        {
            UnityEngine.UIElements.DropdownField dfDfEvacuationGroup = Document.rootVisualElement.Q<UnityEngine.UIElements.DropdownField>("DfEvacuationGroup");

            if (dfDfEvacuationGroup != null && WUInity.DATA_STATUS.HaveInput && WUInity.RUNTIME_DATA.Evacuation.EvacuationGroups.Length > 0)
            {
                string initialPath = Path.GetDirectoryName(WUInity.WORKING_FOLDER);

                initialPath = Path.Combine(initialPath, WUInity.INPUT.Simulation.SimulationID);
                initialPath = Path.Combine(initialPath, WUInity.INPUT.Evacuation.EvacGroupFiles[dfDfEvacuationGroup.index] + ".eg");

                System.Diagnostics.Process.Start("Notepad.exe", initialPath);

                //UnityEngine.Debug.Log($"Edit evac group {initialPath}.");

                string message = string.Concat("Evacuation group file [", Path.GetFileName(initialPath), "] is opened in Notepad.");
                EditorUtility.DisplayDialog(message, "Please remember to reload this evacuation group file if you make and save any changes to the file in Notepad.", "Close");
            }
            else
            {
                EditorUtility.DisplayDialog("No evacuation group file is found", "Please create a new evacuation group file and then load in Notepad.", "Close");
            }
        }

        private void InitEvacGoalList(VisualElement root)
        {   
            // Evacuation Destination list initialisation starts:
            UnityEngine.UIElements.DropdownField dfDfEvacutionDestination = root.Q<UnityEngine.UIElements.DropdownField>("DfEvacutionDestination");
            if (dfDfEvacutionDestination != null)
            {
                dfDfEvacutionDestination.RegisterValueChangedCallback((evt) =>
                {
                    UnityEngine.Debug.Log($"The Evacuation Destination dropdown selection has changed to {evt.newValue}.");

                    // I need to add more fields to allow user see the detailed information about evacuation destinations.

                    UnityEngine.UIElements.TextField tfTxEvacDestName = root.Q<UnityEngine.UIElements.TextField>("TxEvacDestName");
                    if (tfTxEvacDestName != null)
                    {
                        tfTxEvacDestName.SetValueWithoutNotify(WUInity.RUNTIME_DATA.Evacuation.EvacuationGoals[dfDfEvacutionDestination.index].name);
                    }

                    UnityEngine.UIElements.TextField tfTxEvacDestLatLong = root.Q<UnityEngine.UIElements.TextField>("TxEvacDestLatLong");
                    if (tfTxEvacDestLatLong != null)
                    {
                        tfTxEvacDestLatLong.SetValueWithoutNotify(WUInity.RUNTIME_DATA.Evacuation.EvacuationGoals[dfDfEvacutionDestination.index].latLong.x.ToString() + ", " +
                                                                  WUInity.RUNTIME_DATA.Evacuation.EvacuationGoals[dfDfEvacutionDestination.index].latLong.y.ToString());
                    }

                    UnityEngine.UIElements.TextField tfTxEvacDestType = root.Q<UnityEngine.UIElements.TextField>("TxEvacDestType");
                    if (tfTxEvacDestType != null)
                    {
                        EvacGoalType evacGoalType = WUInity.RUNTIME_DATA.Evacuation.EvacuationGoals[dfDfEvacutionDestination.index].goalType;

                        if (evacGoalType == EvacGoalType.Refugee)
                            tfTxEvacDestType.SetValueWithoutNotify("Refugee");
                        else
                            tfTxEvacDestType.SetValueWithoutNotify("Exit");
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
                dfDfResponseCurve.RegisterValueChangedCallback((evt) =>
                {
                    UnityEngine.Debug.Log($"The Response curve dropdown selection has changed to {evt.newValue}.");

                    // I need to add more fields to allow user see the detailed information about evacuation destinations.

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
                dfDfEvacuationGroup.RegisterValueChangedCallback((evt) =>
                {
                    UnityEngine.Debug.Log($"The Evacuation group dropdown selection has changed to {evt.newValue}.");

                    // I need to add more fields to allow user see the detailed information about evacuation destinations.

                });
            }
            // Evacuation group list initialisation end.
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
            // Add event handler for the Project Open button being clicked
            UnityEngine.UIElements.Button btnProjectNew = root.Q<UnityEngine.UIElements.Button>("ProjectNew");
            if (btnProjectNew != null)
                btnProjectNew.clicked += BtnProjectNew_clicked; // Note: the += is "shorthand" for adding a handler to an event

            // Add event handler for the Project Open button being clicked
            UnityEngine.UIElements.Button btnProjectOpen = root.Q<UnityEngine.UIElements.Button>("ProjectOpen");
            if (btnProjectOpen != null)
                btnProjectOpen.clicked += BtnProjectOpen_clicked; // Note: the += is "shorthand" for adding a handler to an event

            // Add event handler for the Project Open button being clicked
            UnityEngine.UIElements.Button btnProjectSave = root.Q<UnityEngine.UIElements.Button>("ProjectSave");
            if (btnProjectSave != null)
                btnProjectSave.clicked += BtnProjectSave_clicked; // Note: the += is "shorthand" for adding a handler to an event

        }


        void OnGUI()
        {
            var root = Document.rootVisualElement;
            UnityEngine.UIElements.VisualElement newGUI = root.Q<UnityEngine.UIElements.VisualElement>("root");
            UnityEngine.UIElements.VisualElement testGUI = root.Q<UnityEngine.UIElements.VisualElement>("TestME");

            testGUI.style.left = 500;
            testGUI.style.top = Screen.height-160;

            Label togLabel = Document.rootVisualElement.Q<Label>("TestOUTPUT");

            if (togLabel != null)
            {
                togLabel.text = ""; // "Happy new year from WUINITY! \n\r";
                List<string> log = WUInity.GetLog();

                //foreach (string logItem in log)
                //    togLabel.text += logItem;
                togLabel.text += log[log.Count - 1]+ "\n\r" + log[log.Count - 2] + "\n\r" + log[log.Count - 3];

            }


            if (newGUI.style.left == 0)
            {
                newGUI.style.left = Screen.width - 500;
                newGUI.style.top = 20;
                newGUI.style.height = Screen.height - 20 - 160;
                WUInity.GUI.enabled = true;

                //testGUI.visible = false;
            }
            else
            {
                newGUI.style.left = 0;
                newGUI.style.top = 0;
                newGUI.style.height = Screen.height;
                WUInity.GUI.enabled = false;

                //testGUI.visible = true;
            }
        }

        //const int consoleHeight = 160;
        //Vector2 scrollPosition;
        void UpdateNewConsole()
        {
            //console
            //GUI.Box(new Rect(500, Screen.height - consoleHeight, Screen.width, consoleHeight), "");
            //GUI.BeginGroup(new Rect(500, Screen.height - consoleHeight, Screen.width, consoleHeight), "");
            //scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(Screen.width), GUILayout.Height(consoleHeight));
            
            List<string> log = WUInity.GetLog();
            
            /*
            for (int i = log.Count - 1; i >= 0; i--)
            {
                GUILayout.Label(log[i]);
            }
            */

            //UnityEngine.Debug.Log($"LOGME count = {log.Count}");

            //GUILayout.EndScrollView();
            //GUI.EndGroup();
        }

        private void BtnProjectNew_clicked()
        {
            /*
            if (EditorUtility.DisplayDialog("Function implementation required", "Call To ProjectNew", "Yes", "No"))
                print("Pressed Yes.");
            else
                print("Pressed No.");
            */
        }
        private void BtnProjectOpen_clicked()
        {
            //WUInity.GUI.OpenLoadInput(); // Avoid calling old GUI functions

            OpenLoadInput();

            /* The following code doesn't work right here. It is moved to OnGUI().
            if (newUIMenuDirty) {
                UpdateMenu();
                newUIMenuDirty = false;
            }
            */

        }
        private void BtnUpdateMap_clicked()
        {
            WUInityInput wO = WUInity.INPUT;

            string[] floatNumbers= mapLLLatLong.Split(',');

            double.TryParse(floatNumbers[0], out wO.Simulation.LowerLeftLatLong.x);
            double.TryParse(floatNumbers[1], out wO.Simulation.LowerLeftLatLong.y);
            
            floatNumbers = mapSizeXY.Split(',');

            double.TryParse(floatNumbers[0], out wO.Simulation.Size.x);
            double.TryParse(floatNumbers[1], out wO.Simulation.Size.y);
            int.TryParse(mapZoomLevel, out wO.Map.ZoomLevel);

            WUInity.INSTANCE.UpdateMapResourceStatus();
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

                // 2. Map section -------------------------------------------------------------------------------------------------------------
                UnityEngine.UIElements.TextField tfTxTSetLatLong = root.Q<UnityEngine.UIElements.TextField>("TxTSetLatLong");
                if (tfTxTSetLatLong != null)
                {
                    tfTxTSetLatLong.value = wO.Simulation.LowerLeftLatLong.x.ToString() + ", " + wO.Simulation.LowerLeftLatLong.y.ToString();
                }

                UnityEngine.UIElements.TextField tfTxTSetMapSize = root.Q<UnityEngine.UIElements.TextField>("TxTSetMapSize");
                if (tfTxTSetMapSize != null)
                {
                    tfTxTSetMapSize.value = wO.Simulation.Size.x.ToString() + ", " + wO.Simulation.Size.y.ToString();
                }

                UnityEngine.UIElements.TextField tfTxTSetMapZoomLevel = root.Q<UnityEngine.UIElements.TextField>("TxTSetMapZoomLevel");
                if (tfTxTSetMapZoomLevel != null)
                {
                    tfTxTSetMapZoomLevel.value = wO.Map.ZoomLevel.ToString();
                    //tfTxTSetMapZoomLevel.SetValueWithoutNotify(wO.Map.ZoomLevel.ToString()); // This function works too.
                }


                // 3. Population section 

                Toggle togTogSetGPWFolder = root.Q<Toggle>("TogSetGPWFolder");
                if (togTogSetGPWFolder != null)
                {
                    string filePath = "No valid global GPW data. Please select correct GPW folder.";

                    if (WUInity.DATA_STATUS.GlobalGPWAvailable && WUInity.INPUT.Population.gpwDataFolder.Length > 0)
                    {
                        togTogSetGPWFolder.SetValueWithoutNotify(true);
                        
                        if (WUInity.INPUT.Population.gpwDataFolder.Length < iGPWfolderLength)
                            filePath = "GPW folder: "+ WUInity.INPUT.Population.gpwDataFolder;
                        else
                            filePath = "GPW folder: ..."+ WUInity.INPUT.Population.gpwDataFolder.Substring(WUInity.INPUT.Population.gpwDataFolder.Length - iGPWfolderLength);
                    }
                    else
                    {
                        togTogSetGPWFolder.SetValueWithoutNotify(false);
                    }

                    Label togTxtGPWFolder = root.Q<Label>("TxtGPWFolder");
                    if (togTxtGPWFolder != null) togTxtGPWFolder.text = filePath;
                }

                SetLocalGPWFile();
                SetPopulationFile();

                SetLocalGPWNumber();
                SetPopulationAndCellNumber();


                // Load evacuation destination -------------------------------------------------------------------------------------------------------------
                UnityEngine.UIElements.DropdownField dfDfEvacutionDestination= root.Q<UnityEngine.UIElements.DropdownField>("DfEvacutionDestination");

                if (dfDfEvacutionDestination != null)
                {
                    List<string> m_DropOptions = new List<string> {};
                    for (int i = 0; i < WUInity.INPUT.Traffic.evacuationGoalFiles.Length; i++)
                    {
                        m_DropOptions.Add(WUInity.RUNTIME_DATA.Evacuation.EvacuationGoals[i].name);
                    }

                    dfDfEvacutionDestination.choices = m_DropOptions;
                    dfDfEvacutionDestination.index = 0;
                }


                // Load response curve -------------------------------------------------------------------------------------------------------------
                UnityEngine.UIElements.DropdownField dfDfResponseCurve = root.Q<UnityEngine.UIElements.DropdownField>("DfResponseCurve");

                if (dfDfResponseCurve != null)
                {
                    List<string> m_DropOptions = new List<string> { };
                    for (int i = 0; i < WUInity.RUNTIME_DATA.Evacuation.ResponseCurves.Length; i++)
                    {
                        m_DropOptions.Add(WUInity.RUNTIME_DATA.Evacuation.ResponseCurves[i].name);
                    }

                    dfDfResponseCurve.choices = m_DropOptions;
                    dfDfResponseCurve.index = 0;
                }

                // Load evacuation group -------------------------------------------------------------------------------------------------------------
                UnityEngine.UIElements.DropdownField dfDfEvacuationGroup = root.Q<UnityEngine.UIElements.DropdownField>("DfEvacuationGroup");

                if (dfDfEvacuationGroup != null)
                {
                    List<string> m_DropOptions = new List<string> { };
                    for (int i = 0; i < WUInity.RUNTIME_DATA.Evacuation.EvacuationGroups.Length; i++)
                    {
                        m_DropOptions.Add(WUInity.RUNTIME_DATA.Evacuation.EvacuationGroups[i].Name);
                    }

                    dfDfEvacuationGroup.choices = m_DropOptions;
                    dfDfEvacuationGroup.index = 0;
                }

                // Traffic section -------------------------------------------------------------------------------------------------------------
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

                // Evacuation section -------------------------------------------------------------------------------------------------------------
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

                UnityEngine.UIElements.TextField tfTxTSetmodWalkDist = root.Q<UnityEngine.UIElements.TextField>("TxTSetmodWalkDist");
                if (tfTxTSetmodWalkDist != null)
                {
                    tfTxTSetmodWalkDist.value = eO.walkingDistanceModifier.ToString();
                }

                UnityEngine.UIElements.TextField tfTxTEvaOrderTime = root.Q<UnityEngine.UIElements.TextField>("TxTEvaOrderTime");
                if (tfTxTEvaOrderTime != null)
                {
                    tfTxTEvaOrderTime.value = eO.EvacuationOrderStart.ToString();
                }

                // Fire characteristics -----------------------------------------------------------------------------------------------------------
                SetLCPFile();
                SetFuelModelFile();
                SetFuelMoistureFile();

                // Similation section -------------------------------------------------------------------------------------------------------------
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

        void SetLocalGPWFile()
        {
            var root = Document.rootVisualElement;
            Toggle togTogPopulateFromGPW = root.Q<Toggle>("TogPopulateFromGPW");

            if (togTogPopulateFromGPW != null)
            {
                Label togTxtGPWFile = root.Q<Label>("TxtGPWFile");
                if (togTxtGPWFile != null)
                {
                    string filePath;
                    if (WUInity.DATA_STATUS.LocalGPWLoaded && WUInity.INPUT.Population.localGPWFile.Length > 0)
                    {
                        filePath = "GPW file: " + WUInity.INPUT.Population.localGPWFile;
                        togTogPopulateFromGPW.SetValueWithoutNotify(true);
                    }
                    else
                    {
                        filePath = "GPW file: not set";
                        togTogPopulateFromGPW.SetValueWithoutNotify(false);
                    }

                    togTxtGPWFile.text = filePath;
                }
            }
        }

        void SetPopulationFile()
        {
            var root = Document.rootVisualElement;
            Toggle togTogPopulateFromPOP = root.Q<Toggle>("TogPopulateFromPOP");

            if (togTogPopulateFromPOP != null)
            {
                Label togTxtPOPFile = root.Q<Label>("TxtPOPFile");
                if (togTxtPOPFile != null)
                {
                    string filePath;
                    if (WUInity.DATA_STATUS.PopulationLoaded && WUInity.INPUT.Population.populationFile.Length > 0)
                    {
                        filePath = "POP file: " + WUInity.INPUT.Population.populationFile;
                        togTogPopulateFromPOP.SetValueWithoutNotify(true);
                    }
                    else
                    {
                        filePath = "POP file: not set";
                        togTogPopulateFromPOP.SetValueWithoutNotify(false);
                    }

                    togTxtPOPFile.text = filePath;
                }
            }
        }

        void SetLCPFile()
        {
            var root = Document.rootVisualElement;
            Toggle togTogLoadLCPFile = root.Q<Toggle>("TogLoadLCPFile");

            if (togTogLoadLCPFile != null)
            {
                Label togTxtLCPFile = root.Q<Label>("TxtLCPFile");
                if (togTxtLCPFile != null)
                {
                    string filePath;
                    if (WUInity.INPUT.Fire.lcpFile.Length > 0)
                    {
                        filePath = "LCP file: " + WUInity.INPUT.Fire.lcpFile;
                        togTogLoadLCPFile.SetValueWithoutNotify(true);
                    }
                    else
                    {
                        filePath = "LCP file: not set";
                        togTogLoadLCPFile.SetValueWithoutNotify(false);
                    }

                    togTxtLCPFile.text = filePath;
                }
            }
        }

        void SetFuelModelFile()
        {
            var root = Document.rootVisualElement;
            Toggle togTogLoadFuelModelFile = root.Q<Toggle>("TogLoadFuelModelFile");

            if (togTogLoadFuelModelFile != null)
            {
                Label togTxtFuelModelFile = root.Q<Label>("TxtFuelModelFile");
                if (togTxtFuelModelFile != null)
                {
                    string filePath;
                    if (WUInity.INPUT.Fire.fuelModelsFile.Length > 0)
                    {
                        filePath = "Fuel model file: " + WUInity.INPUT.Fire.fuelModelsFile;
                        togTogLoadFuelModelFile.SetValueWithoutNotify(true);
                    }
                    else
                    {
                        filePath = "Fuel model file: not set";
                        togTogLoadFuelModelFile.SetValueWithoutNotify(false);
                    }

                    togTxtFuelModelFile.text = filePath;
                }
            }
        }

        void SetFuelMoistureFile()
        {
            var root = Document.rootVisualElement;
            Toggle togTogLoadFuelMoistureFile = root.Q<Toggle>("TogLoadFuelMoistureFile");

            if (togTogLoadFuelMoistureFile != null)
            {
                Label togTxtFuelMoistureFile = root.Q<Label>("TxtFuelMoistureFile");
                if (togTxtFuelMoistureFile != null)
                {
                    string filePath;
                    if (WUInity.INPUT.Fire.initialFuelMoistureFile.Length > 0)
                    {
                        filePath = "Fuel moisture file: " + WUInity.INPUT.Fire.initialFuelMoistureFile;
                        togTogLoadFuelMoistureFile.SetValueWithoutNotify(true);
                    }
                    else
                    {
                        filePath = "Fuel moisture file: not set";
                        togTogLoadFuelMoistureFile.SetValueWithoutNotify(false);
                    }

                    togTxtFuelMoistureFile.text = filePath;
                }
            }
        }

        void OpenLoadInput()
        {
            string[] wuiFilter = new string[] { ".wui" };

            FileBrowser.SetFilters(false, wuiFilter);
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
        }

        /// <summary>
        // Initialise the callback for when "run simulation" is clicked
        /// </summary>
        private void BtnProjectSave_clicked()
        {
            string[] wuiFilter = new string[] { ".wui" };

            if (WUInity.WORKING_FILE == null)
            {
                //OpenSaveInput(); --- port 4 lines of code below
                FileBrowser.SetFilters(false, wuiFilter);
                WUInityInput wO = WUInity.INPUT;
                string initialPath = Path.GetDirectoryName(WUInity.WORKING_FILE);
                FileBrowser.ShowSaveDialog(SaveInput, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, wO.Simulation.SimulationID + ".wui", "Save file", "Save");
            }
            else
            {
                //ParseMainData(wO);    // Need to port code later
                WUInityInput.SaveInput();
            }

            //WUInity.GUI.enabled = true;

            /*
            if (EditorUtility.DisplayDialog("Function implementation required", "Call To ProjectSave", "Yes", "No"))
                print("Pressed Yes.");
            else
                print("Pressed No.");
            */
        }

        void SaveInput(string[] paths)
        {
            WUInityInput wO = WUInity.INPUT;

            WUInity.WORKING_FILE = paths[0];
            if (creatingNewFile)
            {
                mainMenuDirty = true;
                WUInity.INSTANCE.CreateNewInputData();
                wO = WUInity.INPUT; //have to update this since we are creating a new one
            }
            else
            {
                //ParseMainData(wO);
            }
            creatingNewFile = false;
            string name = Path.GetFileNameWithoutExtension(paths[0]);
            wO.Simulation.SimulationID = name;

            WUInityInput.SaveInput();
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

        private void BtnPauseSim_clicked()
        {
            // Start and stop running simulation.
 
            WUInity.INSTANCE.TogglePause();

        }

        private void BtnStartSim_clicked()
        {
            // Start and stop running simulation.
            if (!WUInity.SIM.IsRunning)
            {
                WUInityInput wO = WUInity.INPUT;

                WUInity.GUI.ParseMainData(wO);
                if (!WUInity.DATA_STATUS.CanRunSimulation())
                {
                    WUInity.LOG("ERROR: Could not start simulation, see error log.");
                }
                else
                {
                    //menuChoice = ActiveMenu.Output;
                    WUInity.INSTANCE.StartSimulation();
                }
            }
            else
            {
                if (WUInity.INSTANCE.IsPaused()) WUInity.INSTANCE.TogglePause();
                WUInity.INSTANCE.StopSimulation();
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
                    UnityEngine.Debug.Log($"MapZoomLevel has changed to {evt.newValue}.");
                    mapLLLatLong = evt.newValue;
                });

            UnityEngine.UIElements.TextField tfTxTSetMapSize = root.Q<UnityEngine.UIElements.TextField>("TxTSetMapSize");
            if (tfTxTSetMapSize != null)
                tfTxTSetMapSize.RegisterValueChangedCallback((evt) =>
                {
                    UnityEngine.Debug.Log($"MapZoomLevel has changed to {evt.newValue}.");
                    mapSizeXY = evt.newValue;
                });

            UnityEngine.UIElements.TextField tfTxTSetMapZoomLevel = root.Q<UnityEngine.UIElements.TextField>("TxTSetMapZoomLevel");
            if (tfTxTSetMapZoomLevel != null)
                tfTxTSetMapZoomLevel.RegisterValueChangedCallback((evt) =>
                {
                    UnityEngine.Debug.Log($"MapZoomLevel has changed to {evt.newValue}.");
                    mapZoomLevel = evt.newValue;
                });

            UnityEngine.UIElements.Button btnUpdateMap = root.Q<UnityEngine.UIElements.Button>("UpdateMap");
            if (btnUpdateMap != null)
                btnUpdateMap.clicked += BtnUpdateMap_clicked;
        }

        private void SetGlobalGPWFolder(VisualElement root)
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
            Toggle togTogSetGPWFolder = Document.rootVisualElement.Q<Toggle>("TogSetGPWFolder");
            if (togTogSetGPWFolder != null) togTogSetGPWFolder.value = false; // Check also if "gpwDataFolder" is null string.
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
                        string[] gpwFilter = new string[] { ".gpw" };
                        FileBrowser.SetFilters(false, gpwFilter);
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

        private void SetupLoadLCPFile(VisualElement root)
        {
            // Catch toggle-click "Download gridded population of the world" data
            Toggle togLoadLCP = root.Q<Toggle>("TogLoadLCPFile");
            if (togLoadLCP != null)
            {
                togLoadLCP.RegisterValueChangedCallback(evt =>
                {
                    bool loadLCP = evt.newValue;
                    if (loadLCP)
                    {
                        string[] lcpFilter = new string[] { ".lcp" };
                        FileBrowser.SetFilters(false, lcpFilter);
                        string initialPath = WUInity.DATA_FOLDER;

                        if (WUInity.DATA_STATUS.HaveInput)
                        {
                            initialPath = Path.GetDirectoryName(WUInity.WORKING_FOLDER);
                            initialPath = Path.Combine(initialPath, WUInity.INPUT.Simulation.SimulationID);
                        }

                        FileBrowser.ShowLoadDialog(LoadLCPFile, CancelLoadLCPFile, FileBrowser.PickMode.Files, false, initialPath, null, "Load LCP file", "Load");
                    }
                    else
                    {
                        Label togLabel = Document.rootVisualElement.Q<Label>("TxtLCPFile");
                        if (togLabel != null) togLabel.text = "LCP file: not set";
                    }

                    UnityEngine.Debug.Log($"Load local GPW file = {loadLCP}");
                });
            }

        }

        void LoadLCPFile(string[] paths)
        {
            string filePath = "LCP file load error.";

            if (WUInity.RUNTIME_DATA.Fire.LoadLCPFile(paths[0], true))
            {
                filePath = "LCP file: " + Path.GetFileName(paths[0])+ " is loaded successfully.";
            }
            else
            {
                Toggle togLoadLCP = Document.rootVisualElement.Q<Toggle>("TogLoadLCPFile");
                if (togLoadLCP != null) togLoadLCP.value = false;
            }

            Label togLabel = Document.rootVisualElement.Q<Label>("TxtLCPFile");
            if (togLabel != null) togLabel.text = filePath;

        }

        void CancelLoadLCPFile()
        {
            SetLCPFile();
        }

        private void SetupLoadFuelModelFile(VisualElement root)
        {
            // Catch toggle-click "Download gridded population of the world" data
            Toggle togLoadFuel = root.Q<Toggle>("TogLoadFuelModelFile");
            if (togLoadFuel != null)
            {
                togLoadFuel.RegisterValueChangedCallback(evt =>
                {
                    bool loadFuel = evt.newValue;
                    if (loadFuel)
                    {
                        string[] lcpFilter = new string[] { ".fuel" };
                        FileBrowser.SetFilters(false, lcpFilter);
                        string initialPath = WUInity.DATA_FOLDER;

                        if (WUInity.DATA_STATUS.HaveInput)
                        {
                            initialPath = Path.GetDirectoryName(WUInity.WORKING_FOLDER);
                            initialPath = Path.Combine(initialPath, WUInity.INPUT.Simulation.SimulationID);
                        }

                        FileBrowser.ShowLoadDialog(LoadFuelModelFile, CancelLoadFuelModelFile, FileBrowser.PickMode.Files, false, initialPath, null, "Load fuel model file", "Load");
                    }
                    else
                    {
                        Label togLabel = Document.rootVisualElement.Q<Label>("TxtFuelModelFile");
                        if (togLabel != null) togLabel.text = "Fuel model file: not set";
                    }

                    UnityEngine.Debug.Log($"Load local GPW file = {loadFuel}");
                });
            }
        }

        void LoadFuelModelFile(string[] paths)
        {
            string filePath = "Fuel model file load error.";

            if (WUInity.RUNTIME_DATA.Fire.LoadFuelModelsInput(paths[0], true))
            {
                filePath = "Fuel model file: " + Path.GetFileName(paths[0]) + " is loaded successfully.";
            }
            else
            {
                Toggle togTogLoadFuelModelFile = Document.rootVisualElement.Q<Toggle>("TogLoadFuelModelFile");
                if (togTogLoadFuelModelFile != null) togTogLoadFuelModelFile.value = false;
            }

            Label togLabel = Document.rootVisualElement.Q<Label>("TxtFuelModelFile");
            if (togLabel != null) togLabel.text = filePath;

        }

        void CancelLoadFuelModelFile()
        {
            SetFuelModelFile();
        }

        private void SetupLoadFuelMoistureFile(VisualElement root)
        {
            // Catch toggle-click "Download gridded population of the world" data
            Toggle togLoadFuelMoisture = root.Q<Toggle>("TogLoadFuelMoistureFile");
            if (togLoadFuelMoisture != null)
            {
                togLoadFuelMoisture.RegisterValueChangedCallback(evt =>
                {
                    bool loadFuelMoisture = evt.newValue;
                    if (loadFuelMoisture)
                    {
                        string[] fmcFilter = new string[] { ".fmc" };
                        FileBrowser.SetFilters(false, fmcFilter);
                        string initialPath = WUInity.DATA_FOLDER;

                        if (WUInity.DATA_STATUS.HaveInput)
                        {
                            initialPath = Path.GetDirectoryName(WUInity.WORKING_FOLDER);
                            initialPath = Path.Combine(initialPath, WUInity.INPUT.Simulation.SimulationID);
                        }

                        FileBrowser.ShowLoadDialog(LoadFuelMoistureFile, CancelLoadFuelMoistureFile, FileBrowser.PickMode.Files, false, initialPath, null, "Load fuel moisture file", "Load");
                    }
                    else
                    {
                        Label togLabel = Document.rootVisualElement.Q<Label>("TxtFuelMoistureFile");
                        if (togLabel != null) togLabel.text = "Fuel moisture file: not set";
                    }

                    UnityEngine.Debug.Log($"Load fuel moisture file = {loadFuelMoisture}");
                });
            }
        }

        void LoadFuelMoistureFile(string[] paths)
        {
            string filePath = "Fuel moisture file load error.";

            if (WUInity.RUNTIME_DATA.Fire.LoadInitialFuelMoistureData(paths[0], true))
            {
                filePath = "Fuel moisture file: " + Path.GetFileName(paths[0]) + " is loaded successfully.";
            }
            else
            {
                Toggle togTogLoadFuelMoistureFile = Document.rootVisualElement.Q<Toggle>("TogLoadFuelMoistureFile");
                if (togTogLoadFuelMoistureFile != null) togTogLoadFuelMoistureFile.value = false;
            }

            Label togLabel = Document.rootVisualElement.Q<Label>("TxtFuelMoistureFile");
            if (togLabel != null) togLabel.text = filePath;

        }

        void CancelLoadFuelMoistureFile()
        {
            SetFuelMoistureFile();
        }

        private void SetupVewFireFileButtons(VisualElement root)
        {
            UnityEngine.UIElements.Button btnVewFuelModelFile = root.Q<UnityEngine.UIElements.Button>("VewFuelModelFile");
            if (btnVewFuelModelFile != null) 
                btnVewFuelModelFile.clicked += () => BtnViewFireFile(FireFile.fuelModelsFile);

            UnityEngine.UIElements.Button btnVewInitialFuelMoistureFile = root.Q<UnityEngine.UIElements.Button>("VewInitialFuelMoistureFile");
            if (btnVewInitialFuelMoistureFile != null) 
                btnVewInitialFuelMoistureFile.clicked += () => BtnViewFireFile(FireFile.initialFuelMoistureFile);

            UnityEngine.UIElements.Button btnVewWeatherFile = root.Q<UnityEngine.UIElements.Button>("VewWeatherFile");
            if (btnVewWeatherFile != null) 
                btnVewWeatherFile.clicked += () => BtnViewFireFile(FireFile.weatherFile);

            UnityEngine.UIElements.Button btnVewWindFile = root.Q<UnityEngine.UIElements.Button>("VewWindFile");
            if (btnVewWindFile != null) 
                btnVewWindFile.clicked += () => BtnViewFireFile(FireFile.windFile);

            UnityEngine.UIElements.Button btnVewIgnitionPointsFile = root.Q<UnityEngine.UIElements.Button>("VewIgnitionPointsFile");
            if (btnVewIgnitionPointsFile != null)
                btnVewIgnitionPointsFile.clicked += () => BtnViewFireFile(FireFile.ignitionPointsFile);
        }

        /// <summary>
        /// Initialise the basic workflow controls
        /// </summary>
        /// <param name="root"></param>
        private void InitWorkflowControl(VisualElement root)
        {
            //  Populate the foldout control
            VisualElement workflowControl = root.Q<VisualElement>("WorkflowFoldout");
            if (workflowControl != null)
            {
                workflowControl.AddToClassList("show");
            }

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

                    var root = Document.rootVisualElement;
                    Label tipsLabel = root.Q<Label>("TipsLabel");
                    if (tipsLabel != null)
                    {
                        if (showString.Length > 0)
                        {
                            tipsLabel.text = showString;
                        }
                    }


                    // debugging only:
                    UnityEngine.Debug.Log($"Mouse is now over element = {targetElement.name}:{targetElement.name.Length}:{showString}");
                }
                else UnityEngine.Debug.Log("Mouse is over unnamed element");
            }
            else UnityEngine.Debug.Log("Mouse is over null element");
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