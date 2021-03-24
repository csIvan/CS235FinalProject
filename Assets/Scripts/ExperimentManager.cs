using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExperimentManager : MonoBehaviour
{
    private enum Cursor
    {
        Point,
        Bubble,
        Ellipse
    }

    // A self-reference to the singleton instance of this script
    public static ExperimentManager Instance { get; private set; }

    // Resource references
    [SerializeField] private GameObject goalPrefab;
    [SerializeField] private GameObject distractorPrefab;
    [SerializeField] private GameObject targetsRoot;
    [SerializeField] private Texture2D cursorTexture;
    [SerializeField] private Text text;

    [SerializeField] private GameObject pointCursor;
    [SerializeField] private GameObject bubbleCursor;
    [SerializeField] private GameObject ellipseCursor;

    // The parameters for the experiment
    [SerializeField] private Cursor[] cursorTypes;
    [SerializeField] private int numBlocks;
    [SerializeField] private Block block;

    // Target GameObjects
    public GameObject GoalObject { get; private set; }
    public List<GameObject> Targets { get; private set; }

    private int currentTrial = 1;

    void Awake()
    {
        // Set this script as the only instance of the ExperimentManager class
        if (Instance == null)
            Instance = this;
    }

    void Start()
    {
        bubbleCursor.SetActive(true);
        //pointCursor.SetActive(true);
        Targets = new List<GameObject>();
        startTrial();
    }

    private void startTrial()
    {
        // Initial Target
        Vector2 targetPosition = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width / 2, Screen.height / 2));
        GoalObject = Instantiate(goalPrefab, targetPosition, Quaternion.identity);
        GoalObject.GetComponent<Target>().Radius = 0.25f;
        GoalObject.transform.SetParent(targetsRoot.transform);
        Targets.Add(GoalObject);
        
        text.gameObject.SetActive(true);
        text.text = "Trial " + currentTrial + " \nClick the target to start";
    }

    private void spawnTargets(float amplitude, float density, float width, int numDistractors)
    {

    }

    public void targetHit(GameObject hitObject)
    {
        if (hitObject == GoalObject)
        {
            Debug.Log("Hit");
        }
        else
        {
            Debug.Log("Miss");
        }
    }
}
