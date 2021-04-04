using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExperimentManager : MonoBehaviour
{
    // A self-reference to the singleton instance of this script
    public static ExperimentManager Instance { get; private set; }

    // Resource references
    [SerializeField] private Text trialStartText;

    // The parameters for the experiment
    [SerializeField] private float timer = 5.0f;
    [SerializeField] private CursorType[] cursorTypes;
    [SerializeField] private BlockVariables trainingBlock;
    [SerializeField] private BlockVariables experimentBlock;

    // Trial Management variables
    private IEnumerator ITrainingTrials;
    private IEnumerator IExperimentTrials;
    private IEnumerator ICurrentTrial;
    private int currentClick = 0;

    // Timer variables
    private float movementTime = 0;

    void Awake()
    {
        // Set this script as the only instance of the ExperimentManager class
        if (Instance == null)
            Instance = this;
    }

    void Start()
    {
        ITrainingTrials = trainingBlock.GetEnumerator();
        IExperimentTrials = experimentBlock.GetEnumerator();
        ICurrentTrial = IExperimentTrials;
        startTrial();

        CursorManager.Instance.setCursor(CursorType.Ellipse);

        /// JSON Test
        Click testClick = new Click();
        testClick.startPos = new Vector2(0.0f, 0.0f);
        testClick.endPos = new Vector2(10.0f, 10.0f);
        testClick.movementTime = 1.0f;

        Trial testTrial = new Trial();
        testTrial.amplitude = 5.0f;
        testTrial.distractorDesnity = 0.5f;
        testTrial.effectiveWidth = 20.0f;
        testTrial.clicks = new List<Click>();
        testTrial.clicks.Add(testClick);

        string json = JsonUtility.ToJson(testTrial);

        DatabaseManager.Instance.submitJSON(json);
    }

    void Update()
    {
        // Update the timer
        movementTime += Time.deltaTime;

        // If the timer exceeds the limit, move on to the next trial
        if (movementTime >= timer)
            Instance.targetHit(null);
    }

    private void startTrial()
    {
        TargetManager.Instance.spawnStartTarget();

        ICurrentTrial.Reset();
        ICurrentTrial.MoveNext();

        trialStartText.gameObject.SetActive(true);
        trialStartText.text = "Trial " + 1 + " \nClick the target to start";
    }

    public void targetHit(GameObject hitObject)
    {
        if (hitObject == TargetManager.Instance.GoalObject || movementTime >= timer)
        {
            if (currentClick >= experimentBlock.ClicksPerTrial)
            {
                if (ICurrentTrial.MoveNext())
                    startTrial();
                else
                    ICurrentTrial.Reset();

                currentClick = 0;
            }

            // Movement Time
            if (currentClick > 0)
                Debug.Log(movementTime);

            // Reset the timer
            movementTime = 0.0f;


            // Check if trial is in the initial state
            bool initialState = (TargetManager.Instance.Targets.Count == 1);

            // If initial target is clicked
            if (initialState && hitObject != null)        
                initialState = false;

            
            if (!initialState) {
                TargetManager.Instance.spawnTrialTargets((TrialVars)ICurrentTrial.Current, 25);
                currentClick++;
            }
        }
        else
        {
            Debug.Log("Miss");
        }
    }
}
