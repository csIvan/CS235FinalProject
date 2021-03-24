using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Block
{
    // Experiment variables
    [SerializeField] private int numDistractors = 25;
    [SerializeField] private int numTrials = 5;
    [SerializeField] private int numClicks = 9;

    // Independent variables (Amplitude, Density, Width)
    [SerializeField] private List<float> A;
    [SerializeField] private List<float> D;
    [SerializeField] private List<float> W;
}