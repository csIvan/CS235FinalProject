using System;
using System.Collections;
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

    // Cursor management variables
    private int[] randomCursors;
    private int currentCursor = 0;

    // Data tracking variables
    private DateTime experimentStartTime;
    private Vector2 cursorStartPos = Vector2.zero;
    private float movementTime = 0;

    // Data storage variables
    private ExperimentData experimentData = new ExperimentData();
    private BlockData blockData = new BlockData();
    private TrialData trialData = new TrialData();
    private ClickData clickData = new ClickData();


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
        CursorManager.Instance.cursorType = cursorType;

        // Store the current time
        experimentStartTime = DateTime.Now;
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
        // If this block isn't the training block and
        // this clicking task isn't the start button,
        // save the data from this clicking task
        if (!isTrainingBlock && currentClick > 0)
            storeClick();

        // Move onto the next task of the experiment
        // If there are no more trials left, end the experiment
        if (!nextTask())
        {
            storeExperiment();
            endExperiment();
        }


        // Reset the timer
        movementTime = 0.0f;
    }

    // Store the position of the misclick
    public void targetMiss()
    {
        clickData.misClickPos.Add(CursorManager.Instance.getCursorPos());
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
            // Store the completed experiment trial
            if (!isTrainingBlock)
                storeTrial();

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
                    // Store the completed experiment block
                    storeBlock();

                    // If this isn't the last cursor, go to the next cursor
                    if (currentCursor < randomCursors.Length - 1)
                    {
                        // Change the cursor
                        currentCursor++;
                        CursorManager.Instance.cursorType = (CursorType)currentCursor;

                        // Start the training block
                        ICurrentTrial = ITrainingTrials;
                        isTrainingBlock = true;
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
        TargetManager.Instance.clearTargets();

        // Submit the experiment data to the database
        string experimentJSON = JsonUtility.ToJson(experimentData);
        DatabaseManager.Instance.submitJSON(experimentJSON);
    }

    private void storeClick()
    {
        clickData.startPos = cursorStartPos;
        clickData.endPos = CursorManager.Instance.getCursorPos();
        clickData.goalPos = TargetManager.Instance.GoalObject.transform.localPosition;
        clickData.movementTime = movementTime;

        // If there were no misclicks, set the list to null
        if (clickData.misClickPos.Count == 0)
            clickData.misClickPos = null;

        // The end position of this click is the start
        // position of the next click
        cursorStartPos = clickData.endPos;

        trialData.clicks.Add(clickData);
        clickData = new ClickData();
    }

    private void storeTrial()
    {
        TrialVars trialVars = (TrialVars)ICurrentTrial.Current;

        trialData.amplitude = trialVars.A;
        trialData.distractorDesnity = trialVars.D;
        trialData.effectiveWidth = trialVars.W;

        blockData.trials.Add(trialData);
        trialData = new TrialData();
    }

    private void storeBlock()
    {
        blockData.cursorType = CursorManager.Instance.cursorType;

        experimentData.blocks.Add(blockData);
        blockData = new BlockData();
    }

    private void storeExperiment()
    {
        experimentData.startTime = experimentStartTime.ToString();
        experimentData.endTime = DateTime.Now.ToString();
    }
}
