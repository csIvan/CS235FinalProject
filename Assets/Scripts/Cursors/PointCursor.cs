using UnityEngine;

public class PointCursor : Cursor
{
    protected override void updateSelected()
    {
        // Check if the cursor is over any of the targets
        foreach (GameObject target in ExperimentManager.Instance.Targets)
        {
            Vector2 distVector = target.transform.localPosition - transform.localPosition;
            float distSquared = distVector.sqrMagnitude;
            float targetRadius = target.GetComponent<Target>().Radius;

            if (distSquared <= targetRadius * targetRadius)
            {
                setSelected(target, transform.localPosition);
                return;
            }
        }

        // If the cursor isn't over any targets, set the selected object to null
        setSelected(null, Vector2.zero);
    }
}
