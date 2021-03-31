using UnityEngine;

public class PointCursor : MonoBehaviour
{
    private GameObject selectedObject = null;

    void Update()
    {
        RaycastHit2D hit = checkHit();

        // Cursor is over a target
        if (hit && hit.collider != null)
        {
            selectedObject = hit.transform.gameObject;

            // Get the target script
            Target targetScript = selectedObject.GetComponent<Target>();

            // If the target is not selected, select and animate it
            if (!targetScript.Selected)
            {
                targetScript.Selected = true;
                targetScript.startAnimation(hit.point);
            }

            // If a target is clicked, report it to the Experiment manager
            if (Input.GetMouseButtonDown(0))
                ExperimentManager.Instance.targetHit(selectedObject);
        }
        // Cursor is not over a target
        else if (selectedObject)
        {
            selectedObject.GetComponent<Target>().Selected = false;
        }
    }

    private RaycastHit2D checkHit()
    {
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        int layerMask = ~(LayerMask.GetMask("Target"));

        return Physics2D.Raycast(mouseRay.origin, mouseRay.direction, layerMask);
    }
}
