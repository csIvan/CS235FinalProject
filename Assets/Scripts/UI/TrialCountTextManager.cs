using UnityEngine;
using UnityEngine.UI;

public class TrialCountTextManager : MonoBehaviour
{
    Text trialText;
    int currTrial = 0;
    int numTrials = 0;

    void Awake()
    {
        trialText = gameObject.GetComponent<Text>();
    }

    private void updateText()
    {
        int remTrials = numTrials - currTrial;
        trialText.text = remTrials + " Remaining Trials";
    }

    public void setNumTrials(int numTrials)
    {
        this.numTrials = numTrials;
        updateText();
    }

    public void nextTrial()
    {
        currTrial++;
        updateText();
    }

    public void resetTrials()
    {
        currTrial = 1;
        updateText();
    }
}
