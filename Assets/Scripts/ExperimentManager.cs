using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExperimentManager : MonoBehaviour
{
    // A self-reference to the singleton instance of this script
    public static ExperimentManager Instance { get; private set; }

    // Resource references
    [SerializeField] private GameObject goalPrefab;
    [SerializeField] private GameObject distractorPrefab;
    [SerializeField] private GameObject targetsRoot;
    [SerializeField] private Texture2D cursorTexture;
    [SerializeField] private Text trialStartText;

    [SerializeField] private GameObject pointCursor;
    [SerializeField] private GameObject bubbleCursor;
    [SerializeField] private GameObject ellipseCursor;

    // The parameters for the experiment
    [SerializeField] private Vector2 experimentArea = new Vector2(1000.0f, 1500.0f);
    [SerializeField] private float targetRadius = 5.0f;
    [SerializeField] private float targetMargin = 5.0f;
    [SerializeField] private float timer = 5.0f;
    [SerializeField] private CursorType[] cursorTypes;
    [SerializeField] private BlockVariables trainingBlock;
    [SerializeField] private BlockVariables experimentBlock;

    // Target GameObjects
    public GameObject GoalObject { get; private set; }
    public List<GameObject> Targets { get; private set; }

    // Trial Management variables
    private IEnumerator ITrainingTrials;
    private IEnumerator IExperimentTrials;
    private IEnumerator ICurrentTrial;
    private int currentClick = 0;

    // Distractor generation variables
    private Vector2 lastGoalPos;
    private Vector2 sliceDir;
    private Vector2 sliceOrigin;
    private float sliceAng = 20.0f;

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
        //pointCursor.SetActive(true);
        //bubbleCursor.SetActive(true);
        ellipseCursor.SetActive(true);
        Targets = new List<GameObject>();
        ITrainingTrials = trainingBlock.GetEnumerator();
        IExperimentTrials = experimentBlock.GetEnumerator();
        ICurrentTrial = IExperimentTrials;
        startTrial();

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
        clearTargets();

        Vector2 targetPosition = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width / 2.0f, Screen.height / 2.0f));
        GoalObject = InstantiateTarget(goalPrefab, targetPosition, targetRadius);
        lastGoalPos = GoalObject.transform.localPosition;
        
        ICurrentTrial.Reset();
        ICurrentTrial.MoveNext();

        trialStartText.gameObject.SetActive(true);
        trialStartText.text = "Trial " + 1 + " \nClick the target to start";
    }

    private void spawnTargets(TrialVars variables, int numDistractors)
    {
        clearTargets();
        SpawnGoal(variables);
        SpawnDistractors(variables, numDistractors);
    }

    private void SpawnGoal(TrialVars variables)
    {
        GoalObject = InstantiateTarget(goalPrefab, new Vector2(0.0f, 0.0f), targetRadius);

        // Set the location of the goal target
        do
        {
            // Generate a random point in the experiment area
            Vector2 randomPos = randomTargetPos();
            // Get the vector from the last goal position to the random point
            Vector2 offset = lastGoalPos - randomPos;

            // Set the slice origin and direction
            sliceOrigin = lastGoalPos;
            sliceDir = offset.normalized;
            
            // Adjust the vector to have the correct amplitude
            offset = offset.normalized * variables.A;

            // Set the new goal position
            GoalObject.transform.localPosition = lastGoalPos + offset;

        } while (!checkInBounds(GoalObject));

        // Four Distractors to control Goal Target's Effective Width
        for (int i = 0; i < 4; i++) {
            GameObject target = InstantiateTarget(distractorPrefab, new Vector2(0.0f, 0.0f), targetRadius);

            Vector2 dir = lastGoalPos - (Vector2)GoalObject.transform.localPosition;
            float sign = (GoalObject.transform.localPosition.y < lastGoalPos.y) ? -1.0f : 1.0f;
            float offset = (targetRadius * 2f) + targetMargin;

            float angle = Mathf.Deg2Rad * (Vector2.Angle(Vector2.right, dir) * sign + (90 * i));
            Vector2 pos2d = new Vector2(Mathf.Sin(angle) * offset, Mathf.Cos(angle) * offset) + (Vector2)GoalObject.transform.localPosition;
            target.transform.localPosition = pos2d;
        }

        lastGoalPos = GoalObject.transform.localPosition;
    }

    private void SpawnDistractors(TrialVars variables, int numDistractors)
    {
        float dist = variables.A - ((targetRadius * 2.0f) + targetMargin / 4.0f) * 2.0f;
        float spacing = (targetRadius * 2.0f + (targetMargin / 2.0f));

        int intermediateDistractors = (variables.D != 0) ? (int)(dist / (spacing / variables.D)) : 0;

        // Spawn intermediate distractors within cone
        for (int i = 0; i < intermediateDistractors; i++) {
            GameObject target = InstantiateTarget(distractorPrefab, new Vector2(0.0f, 0.0f), targetRadius);
            
            float newSpacing = (spacing / variables.D) * (i + 1);
            Vector2 perpVec = ((i + 1) % 2 == 0) ? Vector2.Perpendicular(sliceDir) : Vector2.Perpendicular(-sliceDir);

            Vector2 x = sliceOrigin + (sliceDir * newSpacing);
            Vector2 y = x + (perpVec.normalized * Random.Range(targetRadius / 2.0f, (newSpacing * Mathf.Tan(Mathf.Deg2Rad * (sliceAng / 2.0f)))));
            target.transform.localPosition = y;
        }

        // Remaining distractors outside cone
        numDistractors = (int)(numDistractors * variables.D);
        for (int i = 0; i < numDistractors; i++) {
            GameObject target = InstantiateTarget(distractorPrefab, new Vector2(0.0f, 0.0f), targetRadius);

            do {
                target.transform.localPosition = randomTargetPos();
            } while (checkOverlap(target));
        }
    }

    private Vector2 randomTargetPos()
    {
        float halfWidth = experimentArea.x / 2.0f;
        float halfHeight = experimentArea.y / 2.0f;

        float randomX = Random.Range(-halfWidth, halfWidth);
        float randomY = Random.Range(-halfHeight, halfHeight);

        return new Vector2(randomX, randomY);
    }

    private GameObject InstantiateTarget(GameObject targetPrefab, Vector2 position, float radius)
    {
        GameObject target = Instantiate(targetPrefab, position, Quaternion.identity);
        target.transform.SetParent(targetsRoot.transform);
        target.GetComponent<Target>().Radius = radius;
        Targets.Add(target);

        return target;
    }

    private void clearTargets()
    {
        foreach (GameObject target in Targets)
            Destroy(target);

        GoalObject = null;
        Targets = new List<GameObject>();
    }

    private bool checkOverlap(GameObject target1)
    {
        foreach (GameObject target2 in Targets)
        {
            if (target1 == target2)
                continue;

            Vector2 distVec = target1.transform.localPosition - target2.transform.localPosition;
            float radius1 = target1.GetComponent<Target>().Radius;
            float radius2 = target2.GetComponent<Target>().Radius;

            if (distVec.magnitude < (radius1 + radius2) + targetMargin)
                return true;
        }

        return false;
    }

    private bool checkInBounds(GameObject target)
    {
        Vector2 pos = target.transform.localPosition;
        float radius = target.GetComponent<Target>().Radius;

        float halfWidth = experimentArea.x / 2.0f;
        float halfHeight = experimentArea.y / 2.0f;

        if (pos.x < (-halfWidth + radius + targetMargin) || pos.x > (halfWidth - radius - targetMargin) ||
            pos.y < (-halfHeight + radius + targetMargin) || pos.y > (halfHeight - radius - targetMargin))
            return false;

        return true;
    }

    public void targetHit(GameObject hitObject)
    {
        if (hitObject == GoalObject || movementTime >= timer)
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
            bool initialState = (Targets.Count == 1);

            // If initial target is clicked
            if (initialState && hitObject != null)        
                initialState = false;

            
            if (!initialState) {
                spawnTargets((TrialVars)ICurrentTrial.Current, 25);
                currentClick++;
            }
        }
        else
        {
            Debug.Log("Miss");
        }
    }
}
