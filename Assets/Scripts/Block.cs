using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TrialVars
{
    public float A { get; set; }
    public float D { get; set; }
    public float W { get; set; }

    public TrialVars(float A, float D, float W)
    {
        this.A = A;
        this.D = D;
        this.W = W;
    }
}


[Serializable]
public class Block : IEnumerator, IEnumerable
{
    // Experiment variables
    [SerializeField] private int numDistractors = 25;
    [SerializeField] private int clicksPerTrial = 9;

    // Independent variables (Amplitude, Density, Width)
    [SerializeField] private float[] A;
    [SerializeField] private float[] D;
    [SerializeField] private float[] W;

    private static System.Random rng = new System.Random();

    private int numTrials = -1;
    private TrialVars[] combinations;
    private int[] randomIndices;

    public int NumDistractors { get { return numDistractors; } }
    public int ClicksPerTrial { get { return clicksPerTrial; } }

    public int NumTrials
    {
        get
        {
            if (numTrials < 0)
                numTrials = A.Length * D.Length * W.Length;

            return numTrials;
        }

        private set { numTrials = value; }
    }

    public TrialVars[] Combinations
    {
        get
        {
            if (combinations == null)
                genCombinations();

            return combinations;
        }

        private set { combinations = value; }
    }

    public int[] RandomIndices
    {
        get
        {
            if (randomIndices == null)
                genRandomIndices();

            return randomIndices;
        }

        private set { randomIndices = value; }
    }

    public int CurrTrial { get; private set; }
    public TrialVars CurrTrialVars { get; private set; }

    private void genCombinations()
    {
        // Instantiate an array to contain all the combinations
        Combinations = new TrialVars[NumTrials];

        int combIndex = 0;

        // Iterate through each combination and populate the array
        for (int AIndex = 0; AIndex < A.Length; AIndex++)
            for (int DIndex = 0; DIndex < D.Length; DIndex++)
                for (int WIndex = 0; WIndex < W.Length; WIndex++)
                    Combinations[combIndex++] = new TrialVars(A[AIndex], D[DIndex], W[WIndex]);
    }

    private void genRandomIndices()
    {
        // Generate an array containing each index of the combinations array
        RandomIndices = Enumerable.Range(0, NumTrials).ToArray();

        // Randomize the array using Fisher-Yates shuffle
        for (int index1 = RandomIndices.Length - 1; index1 > 0; index1--)
        {
            index1--;
            int index2 = rng.Next(index1 + 1);
            int tempValue = RandomIndices[index2];
            RandomIndices[index2] = RandomIndices[index1];
            RandomIndices[index1] = tempValue;
        }
    }

    // IEnumerator and IEnumerable
    public IEnumerator GetEnumerator()
    {
        return (IEnumerator)this;
    }

    // IEnumerable
    public void Reset()
    {
        CurrTrial = 0;
        genRandomIndices();
    }

    // IEnumerator
    public bool MoveNext()
    {
        // Return false if there is no next trial
        if (CurrTrial >= NumTrials)
            return false;

        // Move to the next trial
        CurrTrial++;

        return true;
    }

    // IEnumerable
    public object Current
    {
        get
        {
            int combIndex = RandomIndices[CurrTrial];
            return Combinations[combIndex];
        }
    }
}
