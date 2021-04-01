using UnityEngine;

public class BubbleCursor : Cursor
{
    [SerializeField] private float maxRadius = 100.0f;
    [SerializeField] private float margin = 10f;
    [SerializeField] private float epsilon = 0.1f;

    private float bubbleRadius = -1.0f;

    // Implementation of Bubble Cursor
    protected override void updateSelected()
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
}
