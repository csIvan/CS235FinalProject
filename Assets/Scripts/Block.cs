using System;
using System.Collections.Generic;
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

    // Independent variables (Amplitude, Density, Width)
    [SerializeField] private List<float> A;
    [SerializeField] private List<float> D;
    [SerializeField] private List<float> W;
}
