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
    [SerializeField] private Text trialStartText;

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

    private int currentBlock = 0;
    private int currentTrial = 0;

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
        block.reset();
        startTrial();
    }

    private void startTrial()
    {
        // Initial Target
        Vector2 targetPosition = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width / 2.0f, Screen.height / 2.0f));
        GoalObject = Instantiate(goalPrefab, targetPosition, Quaternion.identity);
        GoalObject.GetComponent<Target>().Radius = 0.5f;
        GoalObject.transform.SetParent(targetsRoot.transform);
        Targets.Add(GoalObject);
        
        // Test distractor
        GameObject distractor = Instantiate(distractorPrefab, new Vector2(2.0f, 2.0f), Quaternion.identity);
        distractor.GetComponent<Target>().Radius = 0.5f;
        distractor.transform.SetParent(targetsRoot.transform);
        Targets.Add(distractor);

        trialStartText.gameObject.SetActive(true);
        trialStartText.text = "Trial " + block.CurrTrial + " \nClick the target to start";
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
