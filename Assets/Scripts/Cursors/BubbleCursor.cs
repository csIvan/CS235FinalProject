using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleCursor : MonoBehaviour
{
    [SerializeField] private float maxRadius = 100.0f;
    [SerializeField] private float margin = 10f;
    [SerializeField] private float epsilon = 0.1f;

    private float bubbleRadius = -1.0f;
    private GameObject selectedObject = null;
    private GameObject bubble = null;

    private float bubbleStartDist = 0.4f;
    private float bubbleMaxScale = 1.3f;
    private bool startBubble = false;
    private bool expandBubble = false;

    // Update is called once per frame
    void Update()
    {
        // Area Cursor - use mouse's location to update circle's position
        Vector2 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = mouse;

        // Apply Bubble Cursor Algorithm
        BubbleAlgorithm();
        AnimateBubble();

        if (Input.GetMouseButtonDown(0))
            ExperimentManager.Instance.targetHit(selectedObject);
    }

    // Implementation of Bubble Cursor
    private void BubbleAlgorithm()
    {
        float firstClosest = maxRadius;
        float secondClosest = maxRadius;
        float firstTargetRadius = 0.0f;
        float secondTargetRadius = 0.0f;
        
        // Find closest and second closest targets
        foreach (GameObject target in ExperimentManager.Instance.Targets)
        {
            float targetRadius = target.GetComponent<Target>().Radius;
            Vector2 diffVector = transform.localPosition - target.transform.localPosition;
            float distance = diffVector.magnitude - targetRadius;

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

        // Bubble fully encapsulates the closest target
        if (firstClosest + (firstTargetRadius * 2) < secondClosest - margin)
            bubbleRadius = firstClosest + (firstTargetRadius * 2);
        // Bubble extends until the margin of the second closest target
        else if (firstClosest + margin < secondClosest - margin)
            bubbleRadius = secondClosest - margin;
        // Bubble extends until it touches the closest target plus some margin
        else if (firstClosest + margin < secondClosest)
            bubbleRadius = firstClosest + margin;
        //Bubble extends until it almost touches the second closest target
        else
            bubbleRadius = secondClosest - epsilon;

        transform.localScale = new Vector2(bubbleRadius * 2, bubbleRadius * 2);
    }


    private void AnimateBubble() {
        // Initialize bubble position and scale
        if (startBubble) {
            bubble.transform.localScale = Vector2.zero;

            Vector2 dir = transform.localPosition - selectedObject.transform.localPosition;
            bubble.transform.localPosition = dir.normalized * bubbleStartDist;

            startBubble = false;
            expandBubble = true;
        }

        // Animate bubble into position and scale it up
        if (expandBubble) {
            if (selectedObject.transform.position != bubble.transform.position) {
                bubble.transform.localPosition = Vector3.MoveTowards(bubble.transform.localPosition, Vector2.zero, 2 * Time.deltaTime);

                float dist = (bubble.transform.localPosition - Vector3.zero).magnitude;
                bubble.transform.localScale = new Vector2(bubbleMaxScale - (dist / bubbleStartDist), bubbleMaxScale - (dist / bubbleStartDist));
            }
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Target"))
        {
            selectedObject = collision.gameObject;
            collision.gameObject.GetComponent<Target>().Selected = true;
            bubble = selectedObject.transform.Find("Bubble").gameObject;
            startBubble = true;
        }
    }

    private void OnTriggerStay2D(Collider2D collision) 
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Target")) 
        {
            selectedObject = collision.gameObject;
            collision.gameObject.GetComponent<Target>().Selected = true;
            bubble = selectedObject.transform.Find("Bubble").gameObject;
            expandBubble = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Target"))
        {
            collision.gameObject.GetComponent<Target>().Selected = false;
            selectedObject = null;
            bubble = null;
            expandBubble = false;
        }
    }
}
