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
public class ClickData
{
    public Vector2 startPos = Vector2.zero;
    public Vector2 endPos = Vector2.zero;
    public Vector2 goalPos = Vector2.zero;
    public List<Vector2> misClickPos = new List<Vector2>();
    public float movementTime = 0.0f;
}

[Serializable]
public class TrialData
{
    public float amplitude = 0.0f;
    public float distractorDesnity = 0.0f;
    public float effectiveWidth = 0.0f;
    public List<ClickData> clicks = new List<ClickData>();
}

[Serializable]
public class BlockData
{
    public CursorType cursorType;
    public List<TrialData> trials = new List<TrialData>();
}

[Serializable]
public class ExperimentData
{
    public string startTime = "";
    public string endTime = "";
    public List<BlockData> blocks = new List<BlockData>();
}