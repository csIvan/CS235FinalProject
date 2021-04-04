using System.Runtime.InteropServices;
using UnityEngine;

public class DatabaseManager : MonoBehaviour
{

    // A self-reference to the singleton instance of this script
    public static DatabaseManager Instance { get; private set; }

    [SerializeField] private string path = "Experiment Data";
    [SerializeField] private GameObject displayObject;
    [SerializeField] private string callbackMethodName;
    [SerializeField] private string fallbackMethodName;

    void Awake()
    {
        // Set this script as the only instance of the DatabaseManager class
        if (Instance == null)
            Instance = this;
    }

    // Import the PushJSON method from the JavaScript library
    [DllImport("__Internal")]
    private static extern void PushJSON(string path, string value, string objectName, string callback, string fallback);

    public void submitJSON(string json)
    {
        if (!Application.isEditor)
            PushJSON(path, json, displayObject.name, callbackMethodName, fallbackMethodName);
    }
}
