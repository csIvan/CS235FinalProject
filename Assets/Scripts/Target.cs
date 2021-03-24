using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    [SerializeField] private GameObject bubbleSprite;
    [SerializeField] private GameObject selectionSprite;

    private float radius = 1.0f;
    private bool selected = false;

    public float Radius
    {
        get { return radius;  }

        set
        {
            transform.localScale = new Vector2(radius*2, radius*2);
            radius = value;
        }
    }

    public bool Selected
    {
        get { return selected;  }

        set
        {
            if (bubbleSprite != null)
                bubbleSprite.SetActive(value);

            if (selectionSprite != null)
                selectionSprite.SetActive(value);
        }
    }
}
