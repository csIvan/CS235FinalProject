using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointCursor : MonoBehaviour
{
    private GameObject selectedObject = null;

    void Update()
    {

        GameObject hitObject = checkHit();

        // Cursor is over a target
        if (hitObject)
        {
            selectedObject = hitObject;
            hitObject.GetComponent<Target>().Selected = true;

            if (Input.GetMouseButtonDown(0))
                ExperimentManager.Instance.targetHit(hitObject);
        }
        // Cursor is not over a target
        else if (selectedObject)
        {
            selectedObject.GetComponent<Target>().Selected = false;
        }
    }

    private GameObject checkHit()
    {
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        int layerMask = ~(LayerMask.GetMask("Target"));

        RaycastHit2D hit = Physics2D.Raycast(mouseRay.origin, mouseRay.direction, layerMask);

        if (hit && hit.collider != null)
            return hit.transform.gameObject;

        return null;
    }
}
