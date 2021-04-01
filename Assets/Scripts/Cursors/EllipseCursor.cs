using UnityEngine;

public class EllipseCursor : Cursor
{
    [SerializeField] private float maxRadius = 100.0f;
    [SerializeField] private float margin = 10f;
    [SerializeField] private float epsilon = 0.001f;

    private float minorRadius = 1.0f;
    private float majorRadius = 2.0f;

    // Implementation of Bubble Cursor
    protected override void updateSelected()
    {
        float closestDist = getClosestDist();

        while (Mathf.Abs(closestDist) > epsilon)
        {
            minorRadius += closestDist * 0.5f;
            majorRadius = minorRadius * 0.5f;

            transform.localScale = new Vector2(minorRadius * 2.0f, majorRadius * 2.0f);

            closestDist = getClosestDist();
        }
    }

    private float getClosestDist()
    {
        float closestDist = maxRadius;

        foreach (GameObject target in ExperimentManager.Instance.Targets)
        {
            float targetRadius = target.GetComponent<Target>().Radius;
            // Transform the point into the local space of the cursor
            Vector2 localPos = transform.InverseTransformPoint(target.transform.position);

            float magnitude = localPos.magnitude;

            // If the point is on the ellipse surface, return 0
            if (magnitude == 0.0f)
                return 0.0f;

            // Get the closest point to the target on the ellipse (in local space)
            localPos /= magnitude;

            // Transform the closest point into world space
            Vector2 closestPoint = transform.TransformPoint(localPos);
            closestPoint = transform.parent.InverseTransformPoint(closestPoint);

            //Get the distance between the closest point on the ellipse and the target
            Vector2 diffVector = (Vector2)target.transform.localPosition - closestPoint;
            closestDist = diffVector.magnitude;

            // If the point is inside the ellipse, negate the distance
            if (magnitude < 1.0f)
                closestDist = -closestDist;
        }

        return closestDist;
    }

    // A signed distance function defining an ellipse
    // Code from: https://iquilezles.org/www/articles/ellipsoids/ellipsoids.htm
    private float ellipseSDF(Vector2 point, Vector2 dimensions)
    {
        float k1 = (point / dimensions).magnitude;
        float k2 = (point / (dimensions * dimensions)).magnitude;
        return k1 * (k1 - 1.0f) / k2;
    }
}
