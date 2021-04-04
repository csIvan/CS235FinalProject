using System;
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
    private bool isTrainingBlock = true;
    private int currentClick = 0;

    // A randomized list of cursor types
    private int[] randomCursors;
    // The current cursor
    private int currentCursor = 0;

    // Timer variables
    private float movementTime = 0;

    void Awake()
    {
        // Set this script as the only instance of the ExperimentManager class
        if (Instance == null)
            Instance = this;

        // Generate a random array of cursors
        int numCursors = Enum.GetNames(typeof(CursorType)).Length;
        randomCursors = Utility.randomIntArray(numCursors);
    }

    void Start()
    {
        // Get iterators to the trial blocks
        ITrainingTrials = trainingBlock.GetEnumerator();
        IExperimentTrials = experimentBlock.GetEnumerator();
        ICurrentTrial = ITrainingTrials;
        startTrial();

        // Set the first cursor type
        CursorType cursorType = (CursorType)randomCursors[currentCursor];
        CursorManager.Instance.setCursor(cursorType);
    }

    void Update()
    {
        // Update the timer
        movementTime += Time.deltaTime;

        // If the timer exceeds the limit, move onto the next trial
        if (movementTime >= timer)
            Instance.timeOut();
    }

    public void targetHit()
    {
        // Movement Time Test
        if (currentClick > 0)
            Debug.Log(movementTime);

        // Reset the timer
        movementTime = 0.0f;

        // Move onto the next task of the experiment
        // If there are no more trials left, end the experiment
        if (!nextTask())
            endExperiment();
    }

    public void targetMiss()
    {
        Debug.Log("Miss");
    }

    private void timeOut()
    {
        Debug.Log("Time Out");
    }

    private void startTrial()
    {
        TargetManager.Instance.spawnStartTarget();

        ICurrentTrial.Reset();
        ICurrentTrial.MoveNext();

        trialStartText.gameObject.SetActive(true);
        trialStartText.text = "Trial " + 1 + " \nClick the target to start";
    }

    // Attempts to proceed to the next task in the experiment
    private bool nextTask()
    {
        // If the trial is not complete, move to the next task in the trial
        if (currentClick < experimentBlock.ClicksPerTrial)
        {
            // Spawn a new set of targets
            TargetManager.Instance.spawnTrialTargets((TrialVars)ICurrentTrial.Current, 25);
            currentClick++;
        }
        // If the trial is complete, move to the next trial
        else
        {
            // Attempt to move to the next trial
            bool isNext = ICurrentTrial.MoveNext();

            // If this block is complete, move to the next block
            if (!isNext)
            {
                // If this is the training block, move onto the experiment block
                if (isTrainingBlock)
                {
                    ICurrentTrial = IExperimentTrials;
                    isTrainingBlock = false;
                }
                // If the experiment block is complete, move to the next cursor
                else
                {
                    // If this isn't the last cursor, go to the next cursor
                    if (currentCursor < randomCursors.Length)
                    {
                        currentCursor++;
                        CursorManager.Instance.setCursor((CursorType)currentCursor);
                    }
                    // If this the last cursor, the experiment is complete
                    else
                        return false;
                }
            }

            // Start the next trial
            startTrial();
            currentClick = 0;
        }

        return true;
    }

    private void endExperiment()
    {

    }
}
