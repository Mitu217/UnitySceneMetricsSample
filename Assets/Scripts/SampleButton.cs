using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SampleButton : MonoBehaviour
{
    public void OnClickGoSceneA()
    {
        //var time = DateTime.Now;
        SceneManager.LoadSceneAsync("SceneA", LoadSceneMode.Single);
        //Debug.LogFormat("{0}: {1}ms", "TestA", (DateTime.Now - time).ToString());
    }

    public void OnClickGoSceneB()
    {
        //var time = DateTime.Now;
        SceneManager.LoadSceneAsync("SceneB", LoadSceneMode.Single);
        //Debug.LogFormat("{0}: {1}ms", "TestA", (DateTime.Now - time).ToString());
    }
}
