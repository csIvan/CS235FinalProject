using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    [SerializeField] private GameObject bubbleSprite;
    [SerializeField] private GameObject selectionSprite;

    public void setSelected(bool enabled)
    {
        if (bubbleSprite != null)
            bubbleSprite.SetActive(enabled);

        if (selectionSprite != null)
            selectionSprite.SetActive(enabled);
    }
}
