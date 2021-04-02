using System;
using UnityEngine;

public class EllipseCursor : Cursor
{
    [SerializeField] private float maxRadius = 100.0f;
    [SerializeField] private float margin = 1.0f;
    [SerializeField] private float maxError = 0.1f;

    private Vector2 dimensions = new Vector2(1.0f, 1.0f);

    // Implementation of Bubble Cursor
    protected override void updateSelected()
    {
        Tuple<GameObject, float> closestTarget = getClosestDist();

        // Recursively adjust the size of the ellipse
        while (Mathf.Abs(closestTarget.Item2) > maxError)
        {
            dimensions.y += closestTarget.Item2;
            dimensions.x = dimensions.y * 1.5f;

            closestTarget = getClosestDist();
        }

        if (dimensions.y > maxRadius)
        {
            dimensions.y = maxRadius;
            dimensions.x = dimensions.y * 1.5f;

            Matrix4x4 worldToLocalMatrix = Matrix4x4.TRS(transform.localPosition, transform.localRotation, Vector3.one).inverse;
            Vector2 localPos = worldToLocalMatrix.MultiplyPoint3x4(closestTarget.Item1.transform.position);
            float newDist = ellipseSDF(localPos, dimensions * 0.5f);

            if (newDist > closestTarget.Item1.GetComponent<Target>().Radius)
                closestTarget = new Tuple<GameObject, float>(null, 0.0f);
        }

        // Set the scale of the ellipse
        transform.localScale = dimensions;
        // Select the closest target
        setSelected(closestTarget.Item1, transform.position);
    }

    private Tuple<GameObject, float> getClosestDist()
    {
        GameObject closestTarget = null;
        float firstClosest = float.PositiveInfinity;
        float secondClosest = float.PositiveInfinity;
        float firstClosestRadius = 0.0f;

        // Find closest and second closest targets
        foreach (GameObject target in ExperimentManager.Instance.Targets)
        {
            float targetRadius = target.GetComponent<Target>().Radius;

            Matrix4x4 worldToLocalMatrix = Matrix4x4.TRS(transform.localPosition, transform.localRotation, Vector3.one).inverse;
            Vector2 localPos = worldToLocalMatrix.MultiplyPoint3x4(target.transform.localPosition);

            float distance = ellipseSDF(localPos, dimensions * 0.5f) - targetRadius;

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

        // The distance that covers the closest target the most
        // without covering any of the other targets
        float adjustedDist;

        // Ellipse fully encapsulates the closest target
        if (firstClosest + (firstClosestRadius * 2) < secondClosest - margin)
            adjustedDist = firstClosest + (firstClosestRadius * 2);
        // Ellipse extends until the margin of the second closest target
        else if (secondClosest - margin > firstClosest + margin)
            adjustedDist = secondClosest - margin;
        // Ellipse extends until it touches the closest target plus some margin
        else if (firstClosest + margin < secondClosest)
            adjustedDist = firstClosest + margin;
        // Ellipse extends until it touches the second closest target
        else
            adjustedDist = secondClosest;

        return new Tuple<GameObject, float>(closestTarget, adjustedDist);
    }

    // A signed distance function defining an ellipse
    // Code from: https://iquilezles.org/www/articles/ellipsoids/ellipsoids.htm
    private float ellipseSDF(Vector2 point, Vector2 radii)
    {
        if (point.magnitude < maxError)
            return Mathf.Min(-radii.x, -radii.y);

        float k1 = (point / radii).magnitude;
        float k2 = (point / (radii * radii)).magnitude;
        return k1 * (k1 - 1.0f) / k2;
    }
}
