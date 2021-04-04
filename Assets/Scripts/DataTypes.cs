using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum CursorType
{
    Point,
    Bubble,
    Ellipse
}

[Serializable]
public class Click
{
    public Vector2 startPos = Vector2.zero;
    public Vector2 endPos = Vector2.zero;
    public List<Vector2> misClickPos = new List<Vector2>();
    public float movementTime = 0.0f;
}

[Serializable]
public class Trial
{
    public float amplitude = 0.0f;
    public float distractorDesnity = 0.0f;
    public float effectiveWidth = 0.0f;
    public List<Click> clicks = new List<Click>();
}

[Serializable]
public class Block
{
    public CursorType cursorType;
    public List<Trial> trials = new List<Trial>();
}

[Serializable]
public class ExperimentData
{
    public string startTime = "";
    public string endTime = "";
    public List<Block> blocks = new List<Block>();
}