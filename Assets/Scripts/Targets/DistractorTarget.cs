using UnityEngine;

public class DistractorTarget : Target
{
    [SerializeField] private GameObject centerObject;
    [SerializeField] private Color unselectedColor;
    [SerializeField] private Color selectedColor;

    private SpriteRenderer centerSpriteRenderer;

    void Start()
    {
        centerSpriteRenderer = centerObject.GetComponent<SpriteRenderer>();
    }

    public override bool Selected
    {
        get { return selected;  }

        set
        {
            if (value == true)
                centerSpriteRenderer.color = selectedColor;
            else
                centerSpriteRenderer.color = unselectedColor;

            bubbleObject.SetActive(value);
        }
    }
}
