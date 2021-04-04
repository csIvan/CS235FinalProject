public class GoalTarget : Target
{
    public override bool Selected
    {
        get { return selected; }

        set
        {
            bubbleObject.SetActive(value);
            stopAnimation();
            selected = value;
        }
    }

    public override void action()
    {
        ExperimentManager.Instance.targetHit();
    }
}
