using System;
using UnityEngine;

public class EllipseCursor : Cursor
{
    [SerializeField] private float maxRadius = 100.0f;
    [SerializeField] private float maxRadiusRatio = 5.0f;
    [SerializeField] private float maxSpeed = 3.0f;
    [SerializeField] private float margin = 1.0f;
    [SerializeField] private float maxError = 0.1f;

    [SerializeField] private float ratioLerpUpInterval;
    [SerializeField] private float ratioLerpDownInterval;

    private Vector2 dimensions = new Vector2(1.0f, 1.0f);
    private Vector2 prevPos, currPos;
    private float radiusRatio = 1.0f;
    private Matrix4x4 worldToLocalUnscaled;

    // On the first frame, the previous position is set to the current position
    void Start()
    {
        updatePosition();
        currPos = transform.localPosition;
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
        currPos = transform.localPosition;
        // Calculate the speed of the cursor
        Vector2 velocity = (currPos - prevPos);
        float speed = velocity.magnitude;

        float targetRadiusRatio;

        // Only update the rotation when the cursor is moving
        if (speed != 0.0f)
            transform.right = velocity;

        // Set the ellipse radius ratio
        targetRadiusRatio = mapf(speed, 0.0f, maxSpeed, 1.0f, maxRadiusRatio);

        // Lerp the radius ratio towards the target
        if (radiusRatio > targetRadiusRatio)
            radiusRatio = Mathf.Lerp(radiusRatio, targetRadiusRatio, ratioLerpDownInterval);
        else
            radiusRatio = Mathf.Lerp(radiusRatio, targetRadiusRatio, ratioLerpUpInterval);

        // Apply the ratio to the ellipse
        dimensions.x = dimensions.y * radiusRatio;
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

        // If there are no targets, set the dimensions to the maximum
        if (TargetManager.Instance.Targets.Count == 0)
        {
            dimensions.y = maxRadius;
            dimensions.x = dimensions.y * radiusRatio;
        }
        // Otherwise size the dimensions to encapsulate the closest target
        else
        {
            int iteration = 0;

            // Iteratively adjust the size of the ellipse until it reaches the target
            while (Mathf.Abs(closestTarget.Item2) > maxError)
            {
                // Prevent an infinite loop
                if (iteration > 100)
                {
                    dimensions.x = 1.0f;
                    dimensions.y = 1.0f;
                    break;
                }

                dimensions.y += closestTarget.Item2 * 0.5f;
                dimensions.x = dimensions.y * radiusRatio;

                closestTarget = getClosestDist();

                iteration++;
            }

            // Restrict the size if it is too large
            if (dimensions.x > maxRadius)
            {
                // Set the dimensions to the maximum
                dimensions.x = maxRadius;
                dimensions.y = dimensions.x / radiusRatio;

                // Compute the new distance to the closest target
                float newDist = sdf(closestTarget.Item1.transform.localPosition);

                // If the target is not encapsulated, don't select it
                if (newDist > closestTarget.Item1.GetComponent<Target>().Radius)
                    closestTarget = new Tuple<GameObject, float>(null, 0.0f);
            }
        }

        // Set the scale of the ellipse
        transform.localScale = dimensions;
        // Select the closest target
        setSelected(closestTarget.Item1, transform.localPosition);
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
        foreach (GameObject target in TargetManager.Instance.Targets)
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
        // Transform the point into the local space of the ellipse,
        // where the ellipse is a unit circle
        Vector2 offsetVec = worldToLocalUnscaled.MultiplyPoint3x4(point);
        offsetVec /= dimensions;

        // Subtract the radius of the unit circle from the
        // distance between the circle center and the point
        float distance = offsetVec.magnitude - 0.5f;
        // Store the sign of the magnitude
        float sign = Mathf.Sign(distance);

        // Scale the vector so that it points from the
        // closest point on the ellipse to the given point
        offsetVec = offsetVec.normalized * distance;

        // Transform the scale of the vector back into canvas space
        offsetVec *= dimensions;

        return offsetVec.magnitude * sign;
    }
}
