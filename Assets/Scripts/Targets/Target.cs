using UnityEngine;

public class Target : MonoBehaviour
{
    [SerializeField] protected float radius = 1.0f;
    [SerializeField] protected float bubbleScale = 1.3f;
    [SerializeField] protected float animationSpeed = 5.0f;
    [SerializeField] protected GameObject bubbleObject;

    // Selection animation variables
    protected bool selected = false;
    protected bool animating = false;

    public float Radius
    {
        get { return radius;  }

        set
        {
            radius = value;
            float diameter = radius * 2.0f;
            transform.localScale = new Vector2(diameter, diameter);
        }
    }

    public virtual bool Selected
    {
        get { return selected; } 

        set
        {
            bubbleObject.SetActive(value);
            stopAnimation();
            selected = value;
        }
    }

    void Update()
    {
        if (animating)
        {
            if (bubbleObject.transform.position == transform.position)
                stopAnimation();
            
            animateBubble();
        }
    }

    public void startAnimation(Vector2 startPos)
    {
        // Move the bubble to the contact point
        Vector2 targetPos = transform.position;
        Vector2 direction = (targetPos - startPos).normalized;
        float bubbleRadius = bubbleScale * 0.5f;
        bubbleObject.transform.localPosition = -direction * bubbleRadius;

        // Start the animation
        animating = true;
    }

    public void stopAnimation()
    {
        // Reset the selection bubble to its default position and location
        bubbleObject.transform.position = transform.position;

        float diamter = bubbleScale * 2.0f;
        bubbleObject.transform.localScale = new Vector2(diamter, diamter);

        // Stop the animation
        animating = false;
    }

    protected void animateBubble()
    {
        Vector3 bubblePos = bubbleObject.transform.localPosition;

        bubbleObject.transform.localPosition = Vector3.MoveTowards(bubblePos, Vector2.zero, animationSpeed * Time.deltaTime);

        float dist = bubblePos.magnitude;
        float bubbleRadius = bubbleScale * 0.5f;
        float scale = bubbleScale - (dist / bubbleRadius);
        bubbleObject.transform.localScale = new Vector2(scale, scale);
    }
}
