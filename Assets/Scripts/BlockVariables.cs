using System;
using System.Collections;
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
public class BlockVariables : IEnumerator, IEnumerable
{
    [SerializeField] private int clicksPerTrial = 9;
    [SerializeField] private float[] A;
    [SerializeField] private float[] D;
    [SerializeField] private float[] W;

    private bool initialized = false;
    private int numTrials;
    // The default trial is one before the first element
    private int currTrial = -1;
    private TrialVars[] combinations;
    private int[] randomIndices;

    public int ClicksPerTrial { get { return clicksPerTrial; } }

    private void initialize()
    {
        numTrials = A.Length * D.Length * W.Length;
        genCombinations();
        randomIndices = Utility.randomIntArray(numTrials);
    }

    private void genCombinations()
    {
        // Instantiate an array to contain all the combinations
        combinations = new TrialVars[numTrials];

        int combIndex = 0;

        // Iterate through each combination and populate the array
        for (int AIndex = 0; AIndex < A.Length; AIndex++)
            for (int DIndex = 0; DIndex < D.Length; DIndex++)
                for (int WIndex = 0; WIndex < W.Length; WIndex++)
                    combinations[combIndex++] = new TrialVars(A[AIndex], D[DIndex], W[WIndex]);
    }

    // IEnumerator and IEnumerable
    public IEnumerator GetEnumerator()
    {
        return this;
    }

    // IEnumerable
    public void Reset()
    {
        currTrial = -1;
        randomIndices = Utility.randomIntArray(numTrials);
    }

    // IEnumerator
    public bool MoveNext()
    {
        if (!initialized)
        {
            initialize();
            initialized = true;
        }

        // Return false if there is no next trial
        if (currTrial >= numTrials - 1)
            return false;

        // Move to the next trial
        currTrial++;

        return true;
    }

    // IEnumerable
    public object Current
    {
        get
        {
            if (!initialized)
                throw new InvalidOperationException("Enumeration has not started. Call MoveNext.");
            else if (currTrial >= numTrials)
                throw new InvalidOperationException("Enumeration already finished.");

            int combIndex = randomIndices[currTrial];
            return combinations[combIndex];
        }
    }
}
