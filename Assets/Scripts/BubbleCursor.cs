using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BubbleCursor : MonoBehaviour
{
    private GameObject GoalTarget;
    public GameObject[] Targets;
    public GameObject GoalRef;
    public GameObject DistractorRef;
    public Texture2D hiddenCursor;
    public Text text;

    Vector2 cameraSize;
    Vector2 lastGoalPos;
    Collider2D targetCollider;
    bool collided = false;

    // Independent variables (Distance, Density, Width)
    public float[] A = { 0.64f, 1.28f, 2.56f };
    public float[] D = { 0.25f, 0.5f, 1.0f };
    public float[] W = { 0.4f, 0.8f, 1.2f };
    float currentDist, currentRadius;
    int currentDensity;

    // Experiment variables
    public float maxRadius = 7.0f;
    public int numDistractors = 25;
    public int maxClicks = 9;
    public int maxTrials = 5;
    int clicks = -1;
    int trial = 0;


    void Start() {
        float screenAspect = (float)Screen.width / (float)Screen.height;
        float cameraHeight = Camera.main.orthographicSize * 2;
        cameraSize = new Vector2((cameraHeight * screenAspect) / 2, cameraHeight / 2);

        StartTrial();

        // Hide Cursor
        Cursor.SetCursor(hiddenCursor, Vector2.zero, CursorMode.ForceSoftware);
    }
    
    void Update() {
        // Area Cursor - use mouse's location to update circle's position
        Vector2 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = mouse;

        // Apply Bubble Cursor Algorithm
        BubbleAlgorithm();

        if (GoalTarget) {
            Debug.DrawLine(lastGoalPos, GoalTarget.transform.position, Color.blue);
        }

        // Check if goal target is selected
        TargetSelection();
    }

    //------------------------------------------------------| BUBBLE CURSOR ALGORITHM |--------------------------------------------------------

    // Implementation of Bubble Cursor
    private void BubbleAlgorithm() {
        float closest = float.MaxValue - 1;
        float secondClosest = float.MaxValue;
        float closestContainment = float.MaxValue;

        for (int i = 0; i < currentDensity; i++) {
            Vector2 targetDist = Targets[i].transform.position;
            Vector2 cursorCenter = transform.position;
            Vector2 dir = cursorCenter - targetDist;

            Vector2 closestPoint = targetDist + dir.normalized * currentRadius;
            Vector2 farthestPoint = targetDist + dir.normalized * -currentRadius;

            float intersectDist = Mathf.Sqrt(Mathf.Pow(cursorCenter.x - closestPoint.x, 2) + Mathf.Pow(cursorCenter.y - closestPoint.y, 2));
            float containmentDist = Mathf.Sqrt(Mathf.Pow(cursorCenter.x - farthestPoint.x, 2) + Mathf.Pow(cursorCenter.y - farthestPoint.y, 2));

            if (intersectDist <= closest) {
                secondClosest = closest;
                closest = intersectDist;
                closestContainment = containmentDist;
            }
            else if (intersectDist <= secondClosest) {
                secondClosest = intersectDist;
            }
        }

        float radius = Mathf.Min(Mathf.Min(closestContainment + 0.02f, secondClosest - 0.001f), maxRadius);

        // Update cursor size
        transform.localScale = new Vector2(radius * 2, radius * 2);

    }

    //-------------------------------------------------------| EXPERIMENT FUNCTIONS |-----------------------------------------------------

    private void StartTrial() {
        currentRadius = W[0] / 2.0f;
        currentDensity = 1;

        if (trial < maxTrials) {

            // Initial Target
            Targets = new GameObject[1];
            Vector2 targetPosition = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width / 2, Screen.height / 2));
            GoalTarget = Instantiate(GoalRef, targetPosition, Quaternion.identity);
            GoalTarget.transform.localScale = new Vector2(currentRadius, currentRadius);
            Targets[0] = GoalTarget;

            trial++;
            clicks = -1;
            text.gameObject.SetActive(true);
            text.text = "Trial " + trial + "\nClick the target to start";

        }
        else {
            // End Block
            currentDensity = 0;
            text.gameObject.SetActive(true);
            text.text = "Cursor Block Completed";
        }
    }

    //---------------------------------------------------------| TARGET MANAGEMENT |------------------------------------------------------

    void TargetSelection() {
        if (collided && Input.GetMouseButtonDown(0)) {
            // If Successful Click
            if ((GoalTarget != null && targetCollider.gameObject.Equals(GoalTarget))) {
                text.gameObject.SetActive(false);
                lastGoalPos = GoalTarget.transform.position;

                ClearTargets();

                clicks++;
                if (clicks < maxClicks) {
                    SpawnTargets();
                }
                else {
                    StartTrial();
                }
            }
        }
    }


    void SpawnTargets() {
        // Get W and D values every click
        // currentDist = A[clicks % 3];
        currentRadius = W[clicks % 3] / 2.0f;
        currentDensity = (int)(D[(int)(clicks / 3)] * numDistractors) + 5;

        // New Targets
        Targets = new GameObject[currentDensity];

        SpawnGoal();
        for (int i = 5; i < currentDensity; i++) {
            GameObject go;
            do {
                float ScreenOffset = currentRadius * 100;
                Vector2 randomPosition = new Vector2(Random.Range(0 + ScreenOffset, Screen.width - ScreenOffset), Random.Range(0 + ScreenOffset, Screen.height - ScreenOffset));
                Vector2 targetPosition = Camera.main.ScreenToWorldPoint(randomPosition);

                go = Instantiate(DistractorRef, targetPosition, Quaternion.identity) as GameObject;

            } while (!checkOverlap(go, i));

            go.transform.localScale = new Vector2(currentRadius, currentRadius);
            Targets[i] = go;
        }

        
    }

    void SpawnGoal() {

        // Create Goal Target
        do {
            float angle = Random.Range(0, Mathf.PI * 2);
            Vector2 pos2d = new Vector2(Mathf.Sin(angle) * 6.0f, Mathf.Cos(angle) * 6.0f) + lastGoalPos;
            GoalTarget = Instantiate(GoalRef, pos2d, Quaternion.identity) as GameObject;

        } while (checkBounds(GoalTarget));

        GoalTarget.transform.localScale = new Vector2(currentRadius, currentRadius);
        Targets[0] = GoalTarget;

        // Four Distractors to control Goal Target's Effective Width
        for (int i = 1; i < 5; i++) {
            Vector2 dir = lastGoalPos - (Vector2)GoalTarget.transform.position;
            float sign = (GoalTarget.transform.position.y < lastGoalPos.y) ? -1.0f : 1.0f;
            float dist = (currentRadius == W[0] / 2) ? 1.0f : 2.0f;

            float angle = Mathf.Deg2Rad * (Vector2.Angle(Vector2.right, dir) * sign + (90 * i));
            Vector2 pos2d = new Vector2(Mathf.Sin(angle) * dist, Mathf.Cos(angle) * dist) + (Vector2)GoalTarget.transform.position;
            GameObject go = Instantiate(DistractorRef, pos2d, Quaternion.identity) as GameObject;
            go.transform.localScale = new Vector2(currentRadius, currentRadius);
            Targets[i] = go;
        }
    }


    // Reset Screen
    void ClearTargets() {
        for (int i = 0; i < currentDensity; i++) {
            Destroy(Targets[i]);
        }
    }


    //--------------------------------------------------------| COLLISION DETECTION |------------------------------------------------------

    private bool checkOverlap(GameObject obj, int maxIndex) {
        for (int i = 0; i < maxIndex; i++) {
            Vector2 distVec = obj.transform.position - Targets[i].transform.position;
            if (distVec.magnitude < (currentRadius * 2) + 0.5f) {
                Destroy(obj);
                return false;
            }
        }
        return true;
    }

    private bool checkBounds(GameObject obj) {
        Vector2 pos = obj.transform.position;
        if (pos.x < (-cameraSize.x + currentRadius + 2.0f) || pos.x > (cameraSize.x - currentRadius - 2.0f) ||
            pos.y < (-cameraSize.y + currentRadius + 2.0f) || pos.y > (cameraSize.y - currentRadius - 2.0f)) {
            Destroy(obj);
            return true;
        }
        return false;
    }


    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Target")) {
            collided = true;
            targetCollider = collision;
            if (collision.gameObject.tag == "Distractor") {
                collision.gameObject.transform.GetChild(2).gameObject.SetActive(true);
            }
            collision.gameObject.transform.GetChild(0).gameObject.SetActive(true);
        }
    }


    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Target")) {
            collided = false;
            if (collision.gameObject.tag == "Distractor") {
                collision.gameObject.transform.GetChild(2).gameObject.SetActive(false);
            }
            collision.gameObject.transform.GetChild(0).gameObject.SetActive(false);
        }
    }
}