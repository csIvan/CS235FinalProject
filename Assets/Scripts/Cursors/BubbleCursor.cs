using UnityEngine;

public class BubbleCursor : Cursor
{
    [SerializeField] private float maxRadius = 100.0f;
    [SerializeField] private float margin = 1.0f;

    private float bubbleRadius = -1.0f;

    // Implementation of Bubble Cursor
    protected override void updateSelected()
    {
        GameObject closestTarget = null;
        float firstClosest = maxRadius;
        float secondClosest = maxRadius;
        float firstClosestRadius = 0.0f;
        
        // Find closest and second closest targets
        foreach (GameObject target in ExperimentManager.Instance.Targets)
        {
            float targetRadius = target.GetComponent<Target>().Radius;
            Vector2 diffVector = transform.localPosition - target.transform.localPosition;
            float distance = diffVector.magnitude - targetRadius;

            // Current target is the closest
            if (distance < firstClosest)
            {
                // The previous closest target is not the second target
                secondClosest = firstClosest;

                // The current target is the new closest
                closestTarget = target;
                firstClosest = distance;
                firstClosestRadius = targetRadius;
            }
            // Current target is the second closest
            else if (distance < secondClosest)
            {
                secondClosest = distance;
            }
        }

        // Bubble fully encapsulates the closest target
        if (firstClosest + (firstClosestRadius * 2) < secondClosest - margin)
            bubbleRadius = firstClosest + (firstClosestRadius * 2);
        // Bubble extends until the margin of the second closest target
        else if (secondClosest - margin > firstClosest + margin)
            bubbleRadius = secondClosest - margin;
        // Bubble extends until it touches the closest target plus some margin
        else if (firstClosest + margin < secondClosest)
            bubbleRadius = firstClosest + margin;
        // Bubble extends until it touches the second closest target
        else
            bubbleRadius = secondClosest;

        // Set the scale of the cursor
        transform.localScale = new Vector2(bubbleRadius * 2, bubbleRadius * 2);

        // Set the new selected target
        setSelected(closestTarget, transform.position);
    }
}
