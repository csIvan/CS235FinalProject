using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointCursor : MonoBehaviour
{
    private GameObject selectedObject = null;
    private float movementTime = 0;
    private float timer = 5.0f;
    private bool resetTime = false;

    void Update()
    {
        // Measure movement time
        movementTime += Time.deltaTime;
        if (movementTime >= timer) {
            ExperimentManager.Instance.targetHit(null, movementTime);
            movementTime = 0f;
        }

        GameObject hitObject = checkHit();

        // Cursor is over a target
        if (hitObject)
        {
            selectedObject = hitObject;
            hitObject.GetComponent<Target>().Selected = true;

            if (Input.GetMouseButtonDown(0)) {
                // Reset movement time if selected object is a goal
                resetTime = (selectedObject && selectedObject.CompareTag("Goal"));

                ExperimentManager.Instance.targetHit(hitObject, movementTime);

                if (resetTime)
                    movementTime = 0f;
            }
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
