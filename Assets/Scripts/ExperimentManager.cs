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
    [SerializeField] private float targetRadius;
    [SerializeField] private float targetMargin;
    [SerializeField] private Cursor[] cursorTypes;
    [SerializeField] private Block trainingBlock;
    [SerializeField] private Block experimentBlock;

    // Target GameObjects
    public GameObject GoalObject { get; private set; }
    public List<GameObject> Targets { get; private set; }

    private IEnumerator ITrainingVars;
    private IEnumerator IExperimentVars;
    private IEnumerator ICurrentVars;
    private int currentTrial = 0;
    private Vector2 lastGoalPos;
    Vector2 cameraSize;

    void Awake()
    {
        // Set this script as the only instance of the ExperimentManager class
        if (Instance == null)
            Instance = this;
    }

    void Start()
    {
        float screenAspect = (float)Screen.width / (float)Screen.height;
        float cameraHeight = Camera.main.orthographicSize * 2;
        cameraSize = new Vector2((cameraHeight * screenAspect) / 2, cameraHeight / 2);

        bubbleCursor.SetActive(true);
        //pointCursor.SetActive(true);
        Targets = new List<GameObject>();
        ITrainingVars = trainingBlock.GetEnumerator();
        IExperimentVars = experimentBlock.GetEnumerator();
        ICurrentVars = IExperimentVars;
        startTrial();
    }

    private void startTrial()
    {
        Vector2 targetPosition = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width / 2.0f, Screen.height / 2.0f));
        GoalObject = spawnTarget(goalPrefab, targetPosition, targetRadius);
        ICurrentVars.MoveNext();

        trialStartText.gameObject.SetActive(true);
        trialStartText.text = "Trial " + 1 + " \nClick the target to start";
    }

    void SpawnTargets(TrialVars variables, int numDistractors)
    {
        clearTargets();
        Targets = new List<GameObject>(numDistractors + 5);

        SpawnGoal(variables);

        for (int i = 0; i < numDistractors; i++)
        {
            GameObject target = spawnTarget(distractorPrefab, new Vector2(0.0f, 0.0f), targetRadius);
            float ScreenOffset = targetRadius * 100.0f;

            do
            {
                Vector2 randomPosition = new Vector2(Random.Range(0 + ScreenOffset, Screen.width - ScreenOffset), Random.Range(0 + ScreenOffset, Screen.height - ScreenOffset));
                target.transform.position = (Vector2)Camera.main.ScreenToWorldPoint(randomPosition);
            } while (!checkOverlap(target));
        }
    }

    void SpawnGoal(TrialVars variables)
    {
        GoalObject = spawnTarget(goalPrefab, new Vector2(0.0f, 0.0f), targetRadius);

        // Set the location of the goal target
        do
        {
            float angle = Random.Range(0, Mathf.PI * 2.0f);
            GoalObject.transform.position = new Vector2(Mathf.Sin(angle) * 6.0f, Mathf.Cos(angle) * 6.0f) + lastGoalPos;

        } while (checkBounds(GoalObject));


        // Four Distractors to control Goal Target's Effective Width
        for (int i = 0; i < 4; i++)
        {
            Vector2 dir = lastGoalPos - (Vector2)GoalObject.transform.position;
            float sign = (GoalObject.transform.position.y < lastGoalPos.y) ? -1.0f : 1.0f;
            float dist = (targetRadius == variables.W / 2.0f) ? 1.0f : 2.0f;

            float angle = Mathf.Deg2Rad * (Vector2.Angle(Vector2.right, dir) * sign + (90 * i));
            Vector2 pos2d = new Vector2(Mathf.Sin(angle) * dist, Mathf.Cos(angle) * dist) + (Vector2)GoalObject.transform.position;
            spawnTarget(distractorPrefab, pos2d, targetRadius);
        }
    }

    private GameObject spawnTarget(GameObject targetPrefab, Vector2 position, float radius)
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
    }

    private bool checkOverlap(GameObject target1)
    {
        foreach (GameObject target2 in Targets)
        {
            if (target1 == target2)
                continue;

            Vector2 distVec = target1.transform.position - target2.transform.position;
            float radius1 = target1.GetComponent<Target>().Radius;
            float radius2 = target2.GetComponent<Target>().Radius;

            if (distVec.magnitude < (radius1 + radius2) + targetMargin)
                return false;
        }

        return true;
    }

    private bool checkBounds(GameObject target)
    {
        Vector2 pos = target.transform.position;
        float radius = target.GetComponent<Target>().Radius;

        if (pos.x < (-cameraSize.x + radius + targetMargin) || pos.x > (cameraSize.x - radius - targetMargin) ||
            pos.y < (-cameraSize.y + radius + targetMargin) || pos.y > (cameraSize.y - radius - targetMargin))
            return true;

        return false;
    }

    public void targetHit(GameObject hitObject)
    {
        if (hitObject == GoalObject)
        {
            if (currentTrial >= experimentBlock.ClicksPerTrial)
            {
                if (ICurrentVars.MoveNext())
                    startTrial();
                else
                    ICurrentVars.Reset();

                currentTrial = 0;
            }

            SpawnTargets((TrialVars)ICurrentVars.Current, 25);
            currentTrial++;
        }
        else
        {
            Debug.Log("Miss");
        }
    }
}
