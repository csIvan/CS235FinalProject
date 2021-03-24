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
    [SerializeField] private GameObject targetRoot;
    [SerializeField] private Texture2D cursorTexture;
    [SerializeField] private Text text;

    // The parameters for the experiment
    [SerializeField] private Cursor[] cursorTypes;
    [SerializeField] private int numBlocks;
    [SerializeField] private Block block;

    // Target GameObjects
    private GameObject goalObject;
    private List<GameObject> distractorObjects;

    private int currentTrial = 1;

    void Awake()
    {
        // Set this script as the only instance of the ExperimentManager class
        if (Instance == null)
            Instance = this;
    }

    void Start()
    {
        startTrial();
    }

    private void startTrial()
    {
        // Initial Target
        Vector2 targetPosition = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width / 2, Screen.height / 2));
        goalObject = Instantiate(goalPrefab, targetPosition, Quaternion.identity);
        goalObject.transform.localScale = new Vector2(0.5f, 0.5f);
        
        text.gameObject.SetActive(true);
        text.text = "Trial " + currentTrial + " \nClick the target to start";
    }

    private void spawnTargets(float amplitude, float density, float width, int numDistractors)
    {

    }

    private void hit()
    {

    }

    private void miss()
    {

    }
}
