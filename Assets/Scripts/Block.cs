using System;
using UnityEngine;

[Serializable]
public class Block
{
    // Experiment variables
    [SerializeField] private int numDistractors = 25;
    [SerializeField] private int clicksPerTrial = 9;

    // Independent variables (Amplitude, Density, Width)
    [SerializeField] private float[] A;
    [SerializeField] private float[] D;
    [SerializeField] private float[] W;

    private bool initialized = false;

    public int NumTrials { get; private set; }
    public int CurrTrial { get; private set; }




    // Reset the random variable combinations
    public void reset()
    {
        //Calculate the total number of trials in the block
        NumTrials = A.Length * D.Length * W.Length;

        CurrTrial = 0;

        // To-do: Generate variable combinations
    }

    public int remainingTrials()
    {
        return NumTrials - CurrTrial;
    }

    // Get the next combination of variables
    public bool nextTrial()
    {
        // Return false if there is no next trial
        if (CurrTrial >= NumTrials)
            return false;

        // Initialize the variable combinations if they aren't already
        if (!initialized)
        {
            reset();
            initialized = true;
        }

        // To-do: select the next variable combination

        NumTrials++;

        return true;
    }
}
