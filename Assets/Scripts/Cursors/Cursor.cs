using UnityEngine;

public abstract class Cursor : MonoBehaviour
{
    protected GameObject selectedObject = null;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && selectedObject)
            selectedObject.GetComponent<Target>().action();
    }

    // Fixed Update is called 50 times per frame
    void FixedUpdate()
    {
        updatePosition();
        updateSelected();
    }

    // Move the cursor to the mouse position
    protected void updatePosition()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(mousePos.x, mousePos.y, transform.position.z);
    }

    // Calculate the currently selected target
    protected abstract void updateSelected();

    // Save the selected target and play the selection animation
    protected void setSelected(GameObject target, Vector2 cursorPos)
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
