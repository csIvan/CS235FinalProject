using System;
using UnityEngine;

public class EllipseCursor : Cursor
{
    [SerializeField] private float maxRadius = 100.0f;
    [SerializeField] private float maxRadiusRatio = 5.0f;
    [SerializeField] private float maxSpeed = 3.0f;
    [SerializeField] private float margin = 1.0f;
    [SerializeField] private float maxError = 0.1f;

    private Vector2 dimensions = new Vector2(1.0f, 1.0f);
    private Vector2 prevPos, currPos;
    private float radiusRatio = 1.0f;
    private Matrix4x4 worldToLocalUnscaled;

    // On the first frame, the previous position is set to the current position
    void Start()
    {
        updatePosition();
        currPos = transform.position;
    }

    // Implementation of Bubble Cursor
    protected override void updateSelected()
    {
        updateTransform();
        updateMatrix();
        updateDimensions();
    }

    // Update the rotation and radius ratio of the ellipse
    private void updateTransform()
    {
        // Update the positions
        prevPos = currPos;
        currPos = transform.position;
        // Calculate the speed of the cursor
        Vector2 velocity = (currPos - prevPos);
        float speed = velocity.magnitude;

        // If the cursor isn't moving, then the cursor should be a sphere
        if (speed == 0.0f)
        {
            radiusRatio = 1.0f;
            return;
        }

        // Rotate the ellipse
        transform.right = velocity;
        // Set the ellipse radius ratio
        radiusRatio = mapf(speed, 0.0f, maxSpeed, 1.0f, maxRadiusRatio);
    }

    private float mapf(float value, float fromMin, float fromMax, float toMin, float toMax)
    {
        if (value < fromMin)
            return toMin;
        if (value > fromMax)
            return toMax;
        else
        {
            float range = (value - fromMin) / (fromMax - fromMin);
            return (range * (toMax - toMin)) + toMin;
        }
    }

    // Adjust the dimensions so that the ellipse covers the closest target
    private void updateDimensions()
    {
        // Calculate the initial distance
        Tuple<GameObject, float> closestTarget = getClosestDist();

        // Iteratively adjust the size of the ellipse until it reaches the target
        while (Mathf.Abs(closestTarget.Item2) > maxError)
        {
            dimensions.y += closestTarget.Item2 * 0.5f;
            dimensions.x = dimensions.y * radiusRatio;

            closestTarget = getClosestDist();
        }

        // Restrict the size if it is too large
        if (dimensions.y > maxRadius)
        {
            // Set the dimensions to the maximum
            dimensions.y = maxRadius;
            dimensions.x = dimensions.y * radiusRatio;

            // Compute the new distance to the closest target
            float newDist = sdf(closestTarget.Item1.transform.position);

            // If the target is not encapsulated, don't select it
            if (newDist > closestTarget.Item1.GetComponent<Target>().Radius)
                closestTarget = new Tuple<GameObject, float>(null, 0.0f);
        }

        // Set the scale of the ellipse
        transform.localScale = dimensions;
        // Select the closest target
        setSelected(closestTarget.Item1, transform.position);
    }

    // Recalculate the unscaled World-To-Local space transform matrix
    private void updateMatrix()
    {
        worldToLocalUnscaled = Matrix4x4.TRS(transform.localPosition, transform.localRotation, Vector3.one).inverse;
    }

    // Returns the maximum allowed distance between the closest target and the ellipse
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
            float distance = sdf(target.transform.localPosition) - targetRadius;

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
    private float sdf(Vector2 point)
    {
        Vector2 radii = dimensions * 0.5f;
        Vector2 localPoint = worldToLocalUnscaled.MultiplyPoint3x4(point);

        if (localPoint.magnitude < maxError)
            return Mathf.Min(-radii.x, -radii.y);

        float k1 = (localPoint / radii).magnitude;
        float k2 = (localPoint / (radii * radii)).magnitude;
        return k1 * (k1 - 1.0f) / k2;
    }
}
