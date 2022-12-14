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
            if (root != null)
            {
                // Initialise main controls
                InitWorkflowControl(root);
                InitOptionsControls(root);
                InitValueSlider(root);

                // Activate Button-click handlers
                SetupProjectTasks(root);

                // Activate Population data callbacks for workflow task items being selected
                SetupDownloadGPWTask(root);
                SetupPlaceGPWInCaseFolder(root);
                SetupDownloadMapboxTokenTask(root);
                SetupPlaceMapboxTokenInFolderTask(root);

                //Hui:load gpw and pop file
                PopulateFromLocalGPWFile(root);
                CheckPopOK(root);  // Will change the name later

                LoadLCP(root);

                // Activate "Run simulation" callback for button-click event
                SetupRunSimulationTask(root);

                // SaveTo/LoadFrom JSON test
                WorkflowSettings workflow;
                workflow = new WorkflowSettings();

                UnityEngine.Debug.Log($"Starting WokflowSettings class");
                bool bSaved = workflow.SaveTo("C:\\Temp\\test-save.json");
                UnityEngine.Debug.Log($"JSON saved OK? {bSaved}:");
                bool bLoaded = workflow.LoadFrom("C:\\Temp\\test-load.json");
                UnityEngine.Debug.Log($"JSON loaded OK? {bLoaded}:");

            }
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

        }


        private void BtnProjectNew_clicked()
        {

            if (EditorUtility.DisplayDialog("Function implementation required", "Call To ProjectNew", "Yes", "No"))
                print("Pressed Yes.");
            else
                print("Pressed No.");

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

        void UpdateMenu()
        {
            var root = Document.rootVisualElement;
            WUInityInput wO = WUInity.INPUT;
            EvacuationInput eO = WUInity.INPUT.Evacuation;

            if (root != null)
            {

                // Map section -------------------------------------------------------------------------------------------------------------
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


                // Traffic section -------------------------------------------------------------------------------------------------------------




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

                UnityEngine.UIElements.TextField tfTxTEvaOrderTime = root.Q<UnityEngine.UIElements.TextField>("TxTEvaOrderTime"); // Need to update it in menu-demo.uxml
                if (tfTxTEvaOrderTime != null)
                {
                    tfTxTEvaOrderTime.value = eO.EvacuationOrderStart.ToString();

                }


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

                UnityEngine.UIElements.TextField tfTxTSetNumSims = root.Q<UnityEngine.UIElements.TextField>("TxTSetNumSims");
                if (tfTxTSetNumSims != null)
                {
                    tfTxTSetNumSims.value = WUInity.RUNTIME_DATA.Simulation.NumberOfRuns.ToString();
                }


                UnityEngine.UIElements.Toggle tgTogMultipleSim = root.Q<UnityEngine.UIElements.Toggle>("TogMultipleSim"); // Need to add it into menu-demo.uxml
                if (tgTogMultipleSim != null)
                {
                    tgTogMultipleSim.SetValueWithoutNotify(WUInity.RUNTIME_DATA.Simulation.MultipleSimulations);
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
            //mainMenuDirty = true;
            newUIMenuDirty = true;
        }

        void CancelSaveLoad()
        {
            //creatingNewFile = false;
        }


        /// <summary>
        // Initialise the callback for when "run simulation" is clicked
        /// </summary>
        private void BtnProjectSave_clicked()
        {
            WUInity.GUI.enabled = true;
            
            this.enabled = false;

            /*
            if (EditorUtility.DisplayDialog("Function implementation required", "Call To ProjectSave", "Yes", "No"))
                print("Pressed Yes.");
            else
                print("Pressed No.");
            */
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
        /// Set up the callback for when "place mapbox token in folder" is called
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
                        String FilePath = "C:\\Projects\\WUI-NITY-project\\_input";
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
            // Catch toggle-click "Download gridded population of the world" data
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
            Toggle togPlaceInCase = root.Q<Toggle>("TogPlaceInCase");
            if (togPlaceInCase != null)
                togPlaceInCase.RegisterValueChangedCallback(evt =>
                {
                    bool placeInCaseFolder = evt.newValue;
                    if (placeInCaseFolder)
                    {
                        String FilePath = "C:\\Projects\\WUI-NITY-project\\_input";
                        System.Diagnostics.Process.Start("Explorer.exe", @"/select," + FilePath);
                    }
                    UnityEngine.Debug.Log($"Place in case folder = {evt.newValue}");
                });
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
                    if (downloadGPW)
                        System.Diagnostics.Process.Start("https://sedac.ciesin.columbia.edu/data/collection/gpw-v4");
                    UnityEngine.Debug.Log($"Download GPW file = {downloadGPW}");
                });
            }
        }


        //TogCheckPopOK
        private void CheckPopOK(VisualElement root) 
        {
            // Catch toggle-click "Download gridded population of the world" data
            Toggle togCheckPopOK = root.Q<Toggle>("TogCheckPopOK");
            if (togCheckPopOK != null)
            {
                togCheckPopOK.RegisterValueChangedCallback(evt =>
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
                        }

                        // Do nothing at this mo. Will be used to load local GPW file.
                        FileBrowser.ShowLoadDialog(null, null, FileBrowser.PickMode.Files, false, initialPath, null, "Load POP file", "Load");
                    }

                    UnityEngine.Debug.Log($"Load local GPW file = {checkPopOK}");
                });
            }

        }

        private void PopulateFromLocalGPWFile(VisualElement root)
        {
            // Catch toggle-click "Download gridded population of the world" data
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
                        }

                        // Do nothing at this mo. Will be used to load local GPW file.
                        FileBrowser.ShowLoadDialog(null, null, FileBrowser.PickMode.Files, false, initialPath, null, "Load GPW file", "Load");
                    }

                    UnityEngine.Debug.Log($"Load local GPW file = {populateFromGPW}");
                });
            }
        }


        private void LoadLCP(VisualElement root)
        {
            // Catch toggle-click "Download gridded population of the world" data
            Toggle togLoadLCP = root.Q<Toggle>("TogLoadLCP");
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
                        }

                        // Do nothing at this mo. Will be used to load local GPW file.
                        FileBrowser.ShowLoadDialog(null, null, FileBrowser.PickMode.Files, false, initialPath, null, "Load LCP file", "Load");
                    }

                    UnityEngine.Debug.Log($"Load local GPW file = {loadLCP}");
                });
            }

        }
        //TogLoadLCP


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