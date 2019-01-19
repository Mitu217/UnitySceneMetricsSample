using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneMetrics
{
    private readonly DateTime loadTime;

    public SceneMetrics(DateTime loadTime)
    {
        this.loadTime = loadTime;
    }
}
