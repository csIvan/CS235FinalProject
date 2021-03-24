using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleCursor : MonoBehaviour
{
    [SerializeField] private float maxRadius = 7.0f;
    [SerializeField] private float margin = 0.1f;

    private float bubbleRadius = -1.0f;
    private GameObject selectedObject = null;

    // Update is called once per frame
    void Update()
    {
        // Area Cursor - use mouse's location to update circle's position
        Vector2 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = mouse;

        // Apply Bubble Cursor Algorithm
        BubbleAlgorithm();

        if (selectedObject != null && Input.GetMouseButtonDown(0))
            ExperimentManager.Instance.targetHit(selectedObject);
    }

    // Implementation of Bubble Cursor
    private void BubbleAlgorithm()
    {
        float firstClosest = maxRadius;
        float secondClosest = maxRadius;
        float firstTargetRadius = 0.0f;
        float secondTargetRadius = 0.0f;
        
        foreach (GameObject target in ExperimentManager.Instance.Targets)
        {
            Vector2 diffVector = transform.position - target.transform.position;
            float distance = diffVector.magnitude;
            float targetRadius = target.GetComponent<Target>().Radius;

            if (distance < firstClosest)
            {
                secondClosest = firstClosest;
                secondTargetRadius = firstTargetRadius;
                firstClosest = distance;
                firstTargetRadius = targetRadius;
            }
            else if (distance < secondClosest)
            {
                secondClosest = distance;
                secondTargetRadius = targetRadius;
            }
        }

        float firstBubbleRadius = firstClosest + (firstTargetRadius * 2);
        float secondBubbleRadius = secondClosest + (secondTargetRadius * 2) + margin;
        float bubbleRadius = Mathf.Min(firstBubbleRadius, secondBubbleRadius);

        transform.localScale = new Vector2(bubbleRadius * 2, bubbleRadius * 2);
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Target"))
        {
            selectedObject = collision.gameObject;
            collision.gameObject.GetComponent<Target>().Selected = true;
        }
    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Target"))
        {
            collision.gameObject.GetComponent<Target>().Selected = false;
            selectedObject = null;
        }
    }
}
