using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleCursor : MonoBehaviour
{
    [SerializeField] private float maxRadius = 7.0f;
    private float currentRadius = -1.0f;

    Vector2 cameraSize;
    Vector2 lastGoalPos;
    Collider2D targetCollider;
    bool collided = false;

    void Start()
    {
        float screenAspect = (float)Screen.width / (float)Screen.height;
        float cameraHeight = Camera.main.orthographicSize * 2;
        cameraSize = new Vector2((cameraHeight * screenAspect) / 2, cameraHeight / 2);
    }

    // Update is called once per frame
    void Update()
    {
        // Area Cursor - use mouse's location to update circle's position
        Vector2 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = mouse;

        // Apply Bubble Cursor Algorithm
        BubbleAlgorithm();

        if (collided && Input.GetMouseButtonDown(0))
        {
            ExperimentManager.Instance.targetHit(targetCollider.gameObject);
        }
    }

    // Implementation of Bubble Cursor
    private void BubbleAlgorithm()
    {
        float closest = float.MaxValue - 1;
        float secondClosest = float.MaxValue;
        float closestContainment = float.MaxValue;

        foreach (GameObject target in ExperimentManager.Instance.Targets)
        {
            Vector2 targetDist = target.transform.position;
            Vector2 cursorCenter = transform.position;
            Vector2 dir = cursorCenter - targetDist;

            Vector2 closestPoint = targetDist + dir.normalized * currentRadius;
            Vector2 farthestPoint = targetDist + dir.normalized * -currentRadius;

            float intersectDist = Mathf.Sqrt(Mathf.Pow(cursorCenter.x - closestPoint.x, 2) + Mathf.Pow(cursorCenter.y - closestPoint.y, 2));
            float containmentDist = Mathf.Sqrt(Mathf.Pow(cursorCenter.x - farthestPoint.x, 2) + Mathf.Pow(cursorCenter.y - farthestPoint.y, 2));

            if (intersectDist <= closest)
            {
                secondClosest = closest;
                closest = intersectDist;
                closestContainment = containmentDist;
            }
            else if (intersectDist <= secondClosest)
            {
                secondClosest = intersectDist;
            }
        }

        float radius = Mathf.Min(Mathf.Min(closestContainment + 0.05f, secondClosest - 0.001f), maxRadius);

        // Update cursor size
        transform.localScale = new Vector2(radius * 2, radius * 2);
    }

    //--------------------------------------------------------| COLLISION DETECTION |------------------------------------------------------

    private bool checkOverlap(GameObject obj)
    {
        foreach (GameObject target in ExperimentManager.Instance.Targets)
        {
            Vector2 distVec = obj.transform.position - target.transform.position;
            if (distVec.magnitude < (currentRadius * 2) + 0.5f)
            {
                Destroy(obj);
                return false;
            }
        }
        return true;
    }

    private bool checkBounds(GameObject obj)
    {
        Vector2 pos = obj.transform.position;
        if (pos.x < (-cameraSize.x + currentRadius + 2.0f) || pos.x > (cameraSize.x - currentRadius - 2.0f) ||
            pos.y < (-cameraSize.y + currentRadius + 2.0f) || pos.y > (cameraSize.y - currentRadius - 2.0f))
        {
            Destroy(obj);
            return true;
        }
        return false;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Target"))
        {
            collided = true;
            targetCollider = collision;
            if (collision.gameObject.tag == "Distractor")
            {
                collision.gameObject.transform.GetChild(2).gameObject.SetActive(true);
            }
            collision.gameObject.transform.GetChild(0).gameObject.SetActive(true);
        }
    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Target"))
        {
            collided = false;
            if (collision.gameObject.tag == "Distractor")
            {
                collision.gameObject.transform.GetChild(2).gameObject.SetActive(false);
            }
            collision.gameObject.transform.GetChild(0).gameObject.SetActive(false);
        }
    }
}
