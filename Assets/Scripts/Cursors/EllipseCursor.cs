using UnityEngine;

public class EllipseCursor : Cursor
{
    [SerializeField] private float maxRadius = 100.0f;
    [SerializeField] private float margin = 10f;
    [SerializeField] private float epsilon = 0.001f;

    private Vector2 dimensions = new Vector2(1.0f, 1.0f);

    // Implementation of Bubble Cursor
    protected override void updateSelected()
    {
        float closestDist = getClosestDist();

        while (Mathf.Abs(closestDist) > epsilon)
        {
            dimensions.x += closestDist;
            dimensions.y = dimensions.x * 0.5f;

            transform.localScale = dimensions;

            closestDist = getClosestDist();

            if (dimensions.magnitude > 100000)
                Debug.Log("test");
        }
    }

    private float getClosestDist()
    {
        float closestDist = maxRadius;

        foreach (GameObject target in ExperimentManager.Instance.Targets)
        {
            float targetRadius = target.GetComponent<Target>().Radius;

            Matrix4x4 worldToLocalMatrix = Matrix4x4.TRS(transform.localPosition, transform.localRotation, Vector3.one).inverse;
            Vector2 localPos = worldToLocalMatrix.MultiplyPoint3x4(target.transform.localPosition);

            float dist = ellipseSDF(localPos, dimensions / 2.0f) + targetRadius;

            if (dist < closestDist)
            {
                closestDist = dist;
                setSelected(target, transform.position);
            }
        }

        return closestDist;
    }

    // A signed distance function defining an ellipse
    // Code from: https://iquilezles.org/www/articles/ellipsoids/ellipsoids.htm
    private float ellipseSDF(Vector2 point, Vector2 radii)
    {
        float k1 = (point / radii).magnitude;
        float k2 = (point / (radii * radii)).magnitude;
        float dist = k1 * (k1 - 1.0f) / k2;

        if (dist != float.NaN)
            return dist;
        else
            return radii.magnitude;
    }
}
