using UnityEngine;

public class BubbleCursor : MonoBehaviour
{
    [SerializeField] private float maxRadius = 100.0f;
    [SerializeField] private float margin = 10f;
    [SerializeField] private float epsilon = 0.1f;

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

        if (Input.GetMouseButtonDown(0))
            ExperimentManager.Instance.targetHit(selectedObject);
    }

    // Implementation of Bubble Cursor
    private void BubbleAlgorithm()
    {
        GameObject closestTarget = null;
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
                // The previous closest target is not the second target
                secondClosest = firstClosest;
                secondTargetRadius = firstTargetRadius;

                // The current target is the new closest
                closestTarget = target;
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
        // Bubble extends until it almost touches the second closest target
        else
            bubbleRadius = secondClosest - epsilon;

        // Set the scale of the cursor
        transform.localScale = new Vector2(bubbleRadius * 2, bubbleRadius * 2);

        // Set the new selected target
        setSelected(closestTarget, transform.position);
    }

    private void setSelected(GameObject target, Vector2 cursorPos)
    {
        // If the given target is already selected, do nothing
        if (target == selectedObject)
            return;

        // Deselect the current target
        if (selectedObject)
            selectedObject.GetComponent<Target>().Selected = false;

        // Set the new selected target
        selectedObject = target;

        // If a selected target is not null, start its animation
        if (selectedObject)
        {
            // Get the target script of the closest object
            Target targetScript = selectedObject.GetComponent<Target>();

            // If the target is not selected, select and animate it
            if (!targetScript.Selected)
            {
                targetScript.Selected = true;
                targetScript.startAnimation(cursorPos);
            }
        }
    }
}
