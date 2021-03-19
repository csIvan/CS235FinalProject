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
        currentRadius = W[clicks % 3] / 2.0f;
        currentDensity = (int)(D[(int)(clicks / 3)] * numDistractors);

        // New Targets
        Targets = new GameObject[currentDensity];

        for (int i = 0; i < currentDensity; i++) {
            GameObject go;
            do {
                float ScreenOffset = currentRadius * 100;
                Vector2 randomPosition = new Vector2(Random.Range(0 + ScreenOffset, Screen.width - ScreenOffset), Random.Range(0 + ScreenOffset, Screen.height - ScreenOffset));
                Vector2 targetPosition = Camera.main.ScreenToWorldPoint(randomPosition);

                if (i == 0) {
                    go = Instantiate(GoalRef, targetPosition, Quaternion.identity) as GameObject;
                    GoalTarget = go;
                }
                else {
                    go = Instantiate(DistractorRef, targetPosition, Quaternion.identity) as GameObject;
                }

            } while (!checkOverlap(go, i));

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
            if (distVec.magnitude < currentRadius * 2) {
                Destroy(obj);
                return false;
            }
        }
        return true;
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