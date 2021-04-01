using UnityEngine;

public class PointCursor : Cursor
{
    protected override void updateSelected()
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
}
