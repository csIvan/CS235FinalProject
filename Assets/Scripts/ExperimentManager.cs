using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExperimentManager : MonoBehaviour
{
    //A self-reference to the singleton instance of this script
    public static ExperimentManager Instance { get; private set; }

    [SerializeField] private GameObject goalPrefab;
    [SerializeField] private GameObject distractorPrefab;
    [SerializeField] private Texture2D cursorTexture;
    [SerializeField] private Text text;

    // Experiment variables
    [SerializeField] private float maxRadius = 7.0f;
    [SerializeField] private int numDistractors = 25;
    [SerializeField] private int maxClicks = 9;
    [SerializeField] private int maxTrials = 5;
    [SerializeField] private int clicks = -1;
    [SerializeField] private int trial = 0;

    // Independent variables (Distance, Density, Width)
    [SerializeField] private float[] A = { 0.64f, 1.28f, 2.56f };
    [SerializeField] private float[] D = { 0.25f, 0.5f, 1.0f };
    [SerializeField] private float[] W = { 0.4f, 0.8f, 1.2f };
    [SerializeField] private float currentDist, currentRadius;
    [SerializeField] private int currentDensity;

    private GameObject goalTarget;
    private List<GameObject> targets;

    void Awake()
    {
        //Set this script as the only instance of the ObjectSelection script
        if (Instance == null)
            Instance = this;
    }

    void Start()
    {
        StartTrial();
    }

    void Update()
    {
        
    }

    //-------------------------------------------------------| EXPERIMENT FUNCTIONS |-----------------------------------------------------
    private void StartTrial()
    {
        currentRadius = W[0] / 2.0f;
        currentDensity = 1;

        if (trial < maxTrials)
        {

            // Initial Target
            targets = new List<GameObject>();
            Vector2 targetPosition = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width / 2, Screen.height / 2));
            goalTarget = Instantiate(goalPrefab, targetPosition, Quaternion.identity);
            goalTarget.transform.localScale = new Vector2(currentRadius, currentRadius);
            targets.Add(goalTarget);

            trial++;
            clicks = -1;
            text.gameObject.SetActive(true);
            text.text = "Trial " + trial + "\nClick the target to start";

        }
        else
        {
            // End Block
            currentDensity = 0;
            text.gameObject.SetActive(true);
            text.text = "Cursor Block Completed";
        }
    }
}
