using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneMetrics
{
    internal enum LoadingState
    {
        NotLoaded = 0,
        Loading,
        Loaded
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    internal static void Init()
    {
        Scene scene = SceneManager.GetActiveScene();
        Debug.Log(GetLoadingState(scene));
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    internal static void Second()
    {
        Scene scene = SceneManager.GetActiveScene();
        Debug.Log(GetLoadingState(scene));
    }

    internal static LoadingState GetLoadingState(Scene scene)
    {
        Type type = scene.GetType();
        BindingFlags flags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic;
        MethodInfo method = type.GetMethod("GetLoadingStateInternal", flags);

        return (LoadingState)method.Invoke(scene, new object[1] { scene.handle });
    }
}
