using UnityEngine;

public abstract class Target : MonoBehaviour
{
    protected float radius = 1.0f;
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

    public abstract bool Selected { get; set; }
}
