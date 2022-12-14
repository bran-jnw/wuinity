using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Base class to store the data to contain button/foldout/label status information
/// </summary>
[System.Serializable]
public class VisualElementState 
{
    // Class should not be based on MonoBehaviour, otherwise it forces the wrong class hierarchy detection in JsonUtility
    // MUST be "serializable" in order for the automatic JsonUtility conversion processes to work within Unity
    // Use public member variables which cannot be declared with {get;set;} otherwise it blocks JsonUtility auto-detection
    public string ctrlName;
    public bool isDone;

    public VisualElementState(in string inName, in bool done)
    {
        this.ctrlName = inName;
        this.isDone = done;
    }

    public void showInConsole()
    {
        UnityEngine.Debug.Log($"JSON content: \n{this.ctrlName},{this.isDone}");
    }
}
/// <summary>
/// Container class for reading/writing the object states for the elementts in the workflow tree/foldout control.
/// </summary>
[System.Serializable]
public class WorkflowSettings : MonoBehaviour
{
    // All public variables in : MonoBehavior type class (declared as System.Serializable) can be auto-converted to/from JSON with JsonUtility
    // This also works when these classes are stored in a simple container like a List.
    // Note: this doesn't work for advanced collections like KeyPairValue
    public List<VisualElementState> controlStates;
   
    public WorkflowSettings()
    {
        controlStates = new List<VisualElementState>();
        controlStates.Add(new VisualElementState("btnOne", true));
        controlStates.Add(new VisualElementState("btnTwo", true));
        controlStates.Add(new VisualElementState("btnThree", false));
    }

    /// <summary>
    /// Save the workflow item settings to JSON data file in the defined path
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public bool SaveTo(in string path)
    {
        string strJSON = JsonUtility.ToJson(this);           // Auto-convert all public member vars for JSON processing
        StreamWriter writer = new StreamWriter(path, false); // Open the file stream, NOT in append mode
        if(writer!=null)
        {
            writer.Write(strJSON);
            writer.Close();
        }
        UnityEngine.Debug.Log($"JSON content: \n{strJSON}");
        return true;
    }

    /// <summary>
    /// Load the workflow tree control status values from the json data file
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public bool LoadFrom(in string path)
    {
        bool loadedOK = false;

        StreamReader reader = new StreamReader(path);
        if (reader != null)
        {
            string strJSON = reader.ReadToEnd();
            reader.Close();
            if(strJSON != null)
                JsonUtility.FromJsonOverwrite(strJSON, this);

            UnityEngine.Debug.Log($"JSON content: \n{strJSON}");
            loadedOK = true;
        }
       
        return loadedOK;
    }
   
}
