using System;
using UnityEngine;

public enum CursorType
{
    Point,
    Bubble,
    Ellipse
}

[Serializable]
public class Block
{
    // Experiment variables
    [SerializeField] private CursorType cursorType = CursorType.Point;
    [SerializeField] private int numDistractors = 25;
    [SerializeField] private int numTrials = 5;
    [SerializeField] private int numClicks = 9;

    // Independent variables (Distance, Density, Width)
    [SerializeField] private float[] A = { 0.64f, 1.28f, 2.56f };
    [SerializeField] private float[] D = { 0.25f, 0.5f, 1.0f };
    [SerializeField] private float[] W = { 0.4f, 0.8f, 1.2f };
}
