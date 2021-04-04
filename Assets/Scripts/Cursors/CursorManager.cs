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

        switch (cursorType) {
            case CursorType.Point: {
                    currentCursor = pointCursor;
                    UnityEngine.Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);
                }
                break;

            case CursorType.Bubble: {
                    currentCursor = bubbleCursor;
                    UnityEngine.Cursor.SetCursor(cursorTexture, currentCursor.transform.position + new Vector3(cursorTexture.width / 2.0f, cursorTexture.width / 2.0f), CursorMode.ForceSoftware);
                }
                break;

            case CursorType.Ellipse: {
                    currentCursor = ellipseCursor;
                    UnityEngine.Cursor.SetCursor(cursorTexture, currentCursor.transform.position + new Vector3(cursorTexture.width / 2.0f, cursorTexture.width / 2.0f), CursorMode.ForceSoftware);
                }
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
