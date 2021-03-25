using System;
using System.Collections.Generic;
using System.Linq;
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

    private static System.Random rng = new System.Random();
    private bool initialized = false;
    private System.Object[] combinations;
    private int[] randomIndices;

    public int NumTrials { get; private set; }
    public int CurrTrial { get; private set; }
    public float CurrA { get; private set; }
    public float CurrD { get; private set; }
    public float CurrW { get; private set; }


    private void genCombinations()
    {
        // Instantiate an array to contain all the combinations
        combinations = new System.Object[NumTrials];

        int combIndex = 0;

        // Iterate through each combination and populate the array
        for (int AIndex = 0; AIndex < A.Length; AIndex++)
            for (int DIndex = 0; DIndex < D.Length; DIndex++)
                for (int WIndex = 0; WIndex < W.Length; WIndex++)
                    combinations[combIndex++] = new float[3] { A[AIndex], D[DIndex], W[WIndex] };
    }

    private void genRandomIndices()
    {
        // Generate an array containing each index of the combinations array
        randomIndices = Enumerable.Range(0, NumTrials).ToArray();

        // Randomize the array using Fisher-Yates shuffle
        for (int index1 = randomIndices.Length - 1; index1 > 0; index1--)
        {
            index1--;
            int index2 = rng.Next(index1 + 1);
            int tempValue = randomIndices[index2];
            randomIndices[index2] = randomIndices[index1];
            randomIndices[index1] = tempValue;
        }
    }

    // Reset the random indices
    public void reset()
    {
        NumTrials = A.Length * D.Length * W.Length;
        CurrTrial = 0;
        genRandomIndices();
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
            genCombinations();
            initialized = true;
        }

        // Get the variables of the current trial
        int combIndex = randomIndices[CurrTrial];
        float[] variables = (float[])combinations[combIndex];

        // Store the variables
        CurrA = variables[0];
        CurrD = variables[1];
        CurrW = variables[2];

        NumTrials++;

        return true;
    }
}
