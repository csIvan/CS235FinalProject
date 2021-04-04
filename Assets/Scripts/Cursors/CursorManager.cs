using UnityEngine;

public class CursorManager : MonoBehaviour
{
    [SerializeField] private GameObject pointCursor;
    [SerializeField] private GameObject bubbleCursor;
    [SerializeField] private GameObject ellipseCursor;
    [SerializeField] private Texture2D cursorTexture;

    private GameObject currentCursor;

    // A self-reference to the singleton instance of this script
    public static CursorManager Instance { get; private set; }


    void Awake()
    {
        // Set this script as the only instance of the CursorManager class
        if (Instance == null)
            Instance = this;

        // The default cursor is the point cursor
        pointCursor.SetActive(true);
        bubbleCursor.SetActive(false);
        ellipseCursor.SetActive(false);

        currentCursor = pointCursor;
    }

    public void setCursor(CursorType cursorType)
    {
        currentCursor.SetActive(false);

        switch (cursorType)
        {
            case CursorType.Point:
                pointCursor.SetActive(true);
                break;

            case CursorType.Bubble:
                bubbleCursor.SetActive(true);
                break;

            case CursorType.Ellipse:
                ellipseCursor.SetActive(true);
                break;
        }
    }
}
