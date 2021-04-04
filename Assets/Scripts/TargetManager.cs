using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetManager : MonoBehaviour
{
    // A self-reference to the singleton instance of this script
    public static TargetManager Instance { get; private set; }

    // Prefab references
    [SerializeField] private GameObject goalPrefab;
    [SerializeField] private GameObject distractorPrefab;
    // Target generation settings
    [SerializeField] private Vector2 experimentArea = new Vector2(1000.0f, 1500.0f);
    [SerializeField] private float targetRadius = 5.0f;
    [SerializeField] private float targetMargin = 5.0f;

    // Target Game Objects
    public GameObject GoalObject { get; private set; }
    public List<GameObject> Targets { get; private set; }

    // Distractor generation variables
    private Vector2 lastGoalPos;
    private Vector2 sliceDir;
    private Vector2 sliceOrigin;
    private float sliceAng = 20.0f;


    void Awake()
    {
        // Set this script as the only instance of the TargetManager class
        if (Instance == null)
            Instance = this;

        Targets = new List<GameObject>();
    }

    // Spawn one target in the center of the screen
    public void spawnStartTarget()
    {
        clearTargets();

        Vector2 targetPosition = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width / 2.0f, Screen.height / 2.0f));
        GoalObject = InstantiateTarget(goalPrefab, targetPosition, targetRadius);
        lastGoalPos = GoalObject.transform.localPosition;
    }

    // Spawn the targets for a trial given a set of variables
    public void spawnTrialTargets(TrialVars variables, int numDistractors)
    {
        clearTargets();
        SpawnGoal(variables);
        SpawnDistractors(variables, numDistractors);
    }

    public void clearTargets()
    {
        foreach (GameObject target in Targets)
            Destroy(target);

        GoalObject = null;
        Targets = new List<GameObject>();
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
        for (int i = 0; i < 4; i++)
        {
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
        for (int i = 0; i < intermediateDistractors; i++)
        {
            GameObject target = InstantiateTarget(distractorPrefab, new Vector2(0.0f, 0.0f), targetRadius);

            float newSpacing = (spacing / variables.D) * (i + 1);
            Vector2 perpVec = ((i + 1) % 2 == 0) ? Vector2.Perpendicular(sliceDir) : Vector2.Perpendicular(-sliceDir);

            Vector2 x = sliceOrigin + (sliceDir * newSpacing);
            Vector2 y = x + (perpVec.normalized * Random.Range(targetRadius / 2.0f, (newSpacing * Mathf.Tan(Mathf.Deg2Rad * (sliceAng / 2.0f)))));
            target.transform.localPosition = y;
        }

        // Remaining distractors outside cone
        numDistractors = (int)(numDistractors * variables.D);
        for (int i = 0; i < numDistractors; i++)
        {
            GameObject target = InstantiateTarget(distractorPrefab, new Vector2(0.0f, 0.0f), targetRadius);

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
        target.transform.SetParent(gameObject.transform);
        target.GetComponent<Target>().Radius = radius;
        Targets.Add(target);

        return target;
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
}
