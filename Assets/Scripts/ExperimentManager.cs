using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExperimentManager : MonoBehaviour
{
    private enum Cursor
    {
        Point,
        Bubble,
        Ellipse
    }

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
    [SerializeField] private Cursor[] cursorTypes;
    [SerializeField] private Block trainingBlock;
    [SerializeField] private Block experimentBlock;

    // Target GameObjects
    public GameObject GoalObject { get; private set; }
    public List<GameObject> Targets { get; private set; }

    private IEnumerator ITrainingTrials;
    private IEnumerator IExperimentTrials;
    private IEnumerator ICurrentTrial;
    private int currentClick = 0;
    private Vector2 lastGoalPos;

    void Awake()
    {
        // Set this script as the only instance of the ExperimentManager class
        if (Instance == null)
            Instance = this;
    }

    void Start()
    {
        bubbleCursor.SetActive(true);
        //pointCursor.SetActive(true);
        Targets = new List<GameObject>();
        ITrainingTrials = trainingBlock.GetEnumerator();
        IExperimentTrials = experimentBlock.GetEnumerator();
        ICurrentTrial = IExperimentTrials;
        startTrial();
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
            // Adjust the vector to have the correct amplitude
            offset = offset.normalized * variables.A;

            // Set the new goal position
            GoalObject.transform.localPosition = lastGoalPos + offset;

        } while (!checkInBounds(GoalObject));

        // Four Distractors to control Goal Target's Effective Width
        for (int i = 0; i < 4; i++)
        {
            Vector2 dir = lastGoalPos - (Vector2)GoalObject.transform.localPosition;
            float sign = (GoalObject.transform.localPosition.y < lastGoalPos.y) ? -1.0f : 1.0f;
            float dist = (targetRadius == variables.W / 2.0f) ? 1.0f : 2.0f;

            float angle = Mathf.Deg2Rad * (Vector2.Angle(Vector2.right, dir) * sign + (90 * i));
            Vector2 pos2d = new Vector2(Mathf.Sin(angle) * dist, Mathf.Cos(angle) * dist) + (Vector2)GoalObject.transform.localPosition;
            InstantiateTarget(distractorPrefab, pos2d, targetRadius);
        }

        lastGoalPos = GoalObject.transform.localPosition;
    }

    private void SpawnDistractors(TrialVars variables, int numDistractors)
    {
        for (int i = 0; i < numDistractors; i++)
        {
            GameObject target = InstantiateTarget(distractorPrefab, new Vector2(0.0f, 0.0f), targetRadius);
            float ScreenOffset = targetRadius * 100.0f;

            do
            {
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

    public void targetHit(GameObject hitObject, float movementTime)
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
