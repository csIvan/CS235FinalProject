using UnityEngine;

public class CursorManager : MonoBehaviour
{
    // A self-reference to the singleton instance of this script
    public static CursorManager Instance { get; private set; }

    [SerializeField] private GameObject pointCursor;
    [SerializeField] private GameObject bubbleCursor;
    [SerializeField] private GameObject ellipseCursor;
    [SerializeField] private Texture2D cursorTexture;

    private GameObject currentCursor;
    private CursorType currentCursorType;

    public CursorType cursorType
    {
        set { setCursor(value);  }
        get { return currentCursorType; }
    }

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

    private void setCursor(CursorType cursorType)
    {
        currentCursor.SetActive(false);

        switch (cursorType)
        {
            case CursorType.Point:
                currentCursor = pointCursor;
                break;

            case CursorType.Bubble:
                currentCursor = bubbleCursor;
                break;

            case CursorType.Ellipse:
                currentCursor = ellipseCursor;
                break;
        }

        currentCursor.SetActive(true);
        currentCursorType = cursorType;
    }

    public Vector2 getCursorPos()
    {
        return currentCursor.transform.localPosition;
    }
}
