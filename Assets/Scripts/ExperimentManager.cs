using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ExperimentManager : MonoBehaviour
{
    // A self-reference to the singleton instance of this script
    public static ExperimentManager Instance { get; private set; }

    // UI References
    [SerializeField] private GameObject experimentStartText;
    [SerializeField] private GameObject trialStartText;

    // The parameters for the experiment
    [SerializeField] private Vector2 experimentBounds = new Vector2(250.0f, 150.0f);
    [SerializeField] private float timer = 5.0f;
    [SerializeField] private CursorType[] cursorTypes;
    [SerializeField] private BlockVariables trainingBlock;
    [SerializeField] private BlockVariables experimentBlock;

    public Vector2 ExperimentBounds { get { return experimentBounds; } }

    // Trial Management variables
    private IEnumerator ITrainingTrials;
    private IEnumerator IExperimentTrials;
    private IEnumerator ICurrentTrial;
    private bool firstTrial = true;
    private bool isTrainingBlock = true;
    private bool experimentFinished = false;
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

        // Store the current time
        experimentStartTime = DateTime.Now;


        // Set the first cursor type
        CursorType cursorType = (CursorType)randomCursors[currentCursor];
        CursorManager.Instance.cursorType = cursorType;

        // Get iterators to the trial blocks
        ITrainingTrials = trainingBlock.GetEnumerator();
        IExperimentTrials = experimentBlock.GetEnumerator();
        ICurrentTrial = ITrainingTrials;

        // Start the experiment
        ICurrentTrial.MoveNext();
        startTrial();
    }

    void Update()
    {
        // Update the timer
        movementTime += Time.deltaTime;

        // If the timer exceeds the limit, move onto the next trial
        if (currentClick > 0 && movementTime >= timer && !experimentFinished)
            Instance.targetTimeOut();
    }

    public void targetHit()
    {
        // Hide any text on-screen
        experimentStartText.SetActive(false);
        trialStartText.SetActive(false);

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

    private void targetTimeOut()
    {
        // Store the click data with a flag signifying that the timer ran out
        storeClick(false);
        nextTask();
        movementTime = 0.0f;
    }

    private void startTrial()
    {
        TargetManager.Instance.spawnStartTarget();

        // Don't show the trial start text on the first trial
        if (firstTrial)
            firstTrial = false;
        // On subsequent trials, show the trial start text
        else
            trialStartText.SetActive(true);
    }

    // Attempts to proceed to the next task in the experiment
    private bool nextTask()
    {
        // If the trial is not complete, move to the next task in the trial
        if (currentClick < experimentBlock.ClicksPerTrial)
        {
            // Spawn a new set of targets
            TargetManager.Instance.spawnTrialTargets((TrialVars)ICurrentTrial.Current);
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
                    if (currentCursor + 1 < randomCursors.Length)
                    {
                        // Change the cursor
                        currentCursor++;
                        CursorManager.Instance.cursorType = (CursorType)randomCursors[currentCursor];

                        // Start the training block
                        ICurrentTrial = ITrainingTrials;
                        isTrainingBlock = true;
                    }
                    // If this the last cursor, the experiment is complete
                    else
                        return false;
                }

                // Reset the block before using it
                ICurrentTrial.Reset();
                ICurrentTrial.MoveNext();
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
        experimentFinished = true;
    }

    private void storeClick(bool timeout = false)
    {
        clickData.startPos = cursorStartPos;
        clickData.goalPos = TargetManager.Instance.GoalObject.transform.localPosition;
        clickData.movementTime = movementTime;
        clickData.timeout = timeout;

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
