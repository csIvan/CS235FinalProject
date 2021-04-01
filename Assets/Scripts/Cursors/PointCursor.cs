using UnityEngine;

public class PointCursor : MonoBehaviour
{
    private GameObject selectedObject = null;

    void Update()
    {
        updateSelected();

        if (Input.GetMouseButtonDown(0))
            ExperimentManager.Instance.targetHit(selectedObject);
    }

    void updateSelected()
    {
        RaycastHit2D hit = checkHit();

        // Cursor is over a target
        if (hit && hit.collider != null)
            setSelected(hit.transform.gameObject, hit.point);
        // Cursor is not over a target
        else
            setSelected(null, Vector2.zero);
    }

    private RaycastHit2D checkHit()
    {
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        int layerMask = ~(LayerMask.GetMask("Target"));

        return Physics2D.Raycast(mouseRay.origin, mouseRay.direction, layerMask);
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
