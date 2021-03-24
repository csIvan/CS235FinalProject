using UnityEngine;

public class GoalTarget : Target
{
    [SerializeField] private GameObject bubbleObject;

    public override bool Selected
    {
        get { return selected;  }

        set
        {
            if (bubbleObject != null)
                bubbleObject.SetActive(value);
        }
    }
}
