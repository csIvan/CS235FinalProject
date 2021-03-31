using UnityEngine;

public class Target : MonoBehaviour
{
    [SerializeField] protected GameObject bubbleObject;

    protected float radius = 5.0f;
    protected bool selected = false;

    public float Radius
    {
        get { return radius;  }

        set
        {
            radius = value;
            transform.localScale = new Vector2(radius*2, radius*2);
        }
    }

    public virtual bool Selected
    {
        get { return selected; } 

        set
        {
            if (bubbleObject != null)
                bubbleObject.SetActive(value);

            selected = value;
        }
    }
}
