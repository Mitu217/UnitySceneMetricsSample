using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.Experimental.LowLevel;
using UnityEngine.SceneManagement;

public static class SceneProfiler
{

    /// <summary>
    /// Scene metrics process.
    /// </summary>
    private struct SceneMetricsProcess { }

    /// <summary>
    /// The initialization scene.
    /// </summary>
    private static ProfilerSystem initSceneProfileSystem;

    /// <summary>
    /// The scenes of application.
    /// </summary>
    private static ProfilerSystem[] sceneProfileSystems;

    /// <summary>
    /// Starts the profile for init scene.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void StartProfileForInitScene()
    {
        initSceneProfileSystem = new ProfilerSystem(SceneManager.GetActiveScene());
    }

    /// <summary>
    /// Ends the profile for init scene.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EndProfileForInitScene()
    {
        initSceneProfileSystem.Update();
    }

    /// <summary>
    /// Setups the profile.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void SetupProfile()
    {
        // Get profile systems of ascenes in build settings.
        var sceneCount = SceneManager.sceneCountInBuildSettings;
        sceneProfileSystems = new ProfilerSystem[sceneCount];
        for (var i = 0; i < sceneCount; i++)
        {
            var scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            var sceneName = Path.GetFileNameWithoutExtension(scenePath);
            if (sceneName == initSceneProfileSystem.SceneName)
            {
                sceneProfileSystems[i] = initSceneProfileSystem;
            }
            else
            {
                sceneProfileSystems[i] = new ProfilerSystem(sceneName, scenePath);
            }
        }

        InsertMetricsSystem(new PlayerLoopSystem()
        {
            type = typeof(SceneMetricsProcess),
            updateDelegate = () =>
            {
                foreach (var profilerSystem in sceneProfileSystems)
                {
                    profilerSystem.Update();
                }
            }
        });
    }

    /// <summary>
    /// Inserts the metrics system before initialization sub system.
    /// </summary>
    /// <param name="system">System.</param>
    private static void InsertMetricsSystem(PlayerLoopSystem system)
    {
        var playerLoop = PlayerLoop.GetDefaultPlayerLoop();

        for(var i=0; i<7; i++)
        {
            var updateSystem = playerLoop.subSystemList[i];
            var subPlayerLoop = new List<PlayerLoopSystem>(updateSystem.subSystemList);
            subPlayerLoop.Insert(0, system);
            updateSystem.subSystemList = subPlayerLoop.ToArray();
            playerLoop.subSystemList[i] = updateSystem;
        }

        PlayerLoop.SetPlayerLoop(playerLoop);
    }

    /// <summary>
    /// Profiler system class.
    /// </summary>
    private class ProfilerSystem
    {

        public enum LoadingState
        {
            NotLoaded = 0,
            Loading,
            Loaded,
        }

        public string SceneName { get; }
        public string ScenePath { get; }

        // Load profile
        private LoadingState currentLoadingState;
        private DateTime? latestLoadStartAt;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:SceneMetricsProps"/> class.
        /// </summary>
        /// <param name="sceneName">Name.</param>
        /// <param name="scenePath">Path.</param>
        public ProfilerSystem(string sceneName, string scenePath)
        {
            SceneName = sceneName;
            ScenePath = scenePath;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:SceneMetricsProps"/> class.
        /// </summary>
        /// <param name="scene">Scene.</param>
        public ProfilerSystem(Scene scene)
        {
            SceneName = scene.name;
            ScenePath = scene.path;
            currentLoadingState = GetLoadingStateByScene(scene);

            switch (currentLoadingState)
            {
                case LoadingState.Loading:
                    StartLoadProfile();
                    break;
            }
        }

        /// <summary>
        /// Update this instance.
        /// </summary>
        public void Update()
        {
            Scene scene = SceneManager.GetSceneByName(SceneName);
            if (scene.name == null)
            {
                return;
            }

            UpdateLoadProfile(scene);
        }

        /// <summary>
        /// Updates the load profile.
        /// </summary>
        /// <param name="scene">Scene.</param>
        private void UpdateLoadProfile(Scene scene)
        {
            var beforeLoadingState = currentLoadingState;
            var afterLoadingState = GetLoadingStateByScene(scene);
            currentLoadingState = afterLoadingState;

            if (beforeLoadingState == afterLoadingState)
            {
                return;
            }

            switch (afterLoadingState)
            {
                case LoadingState.Loading:
                    StartLoadProfile();
                    break;
                case LoadingState.Loaded:
                    EndLoadProfile();
                    break;
            }
        }

        /// <summary>
        /// Starts the profile.
        /// </summary>
        private void StartLoadProfile()
        {
            latestLoadStartAt = DateTime.Now;
        }

        /// <summary>
        /// Ends the profile.
        /// </summary>
        private void EndLoadProfile()
        {
            var diff = DateTime.Now - latestLoadStartAt;
            latestLoadStartAt = null;
            Debug.LogFormat("{0}: {1}ms", SceneName, diff.ToString());
        }

        /// <summary>
        /// Gets the state of the loading.
        /// </summary>
        /// <returns>The loading state.</returns>
        /// <param name="scene">Scene.</param>
        private LoadingState GetLoadingStateByScene(Scene scene)
        {
            Type type = scene.GetType();
            BindingFlags flags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic;
            MethodInfo method = type.GetMethod("GetLoadingStateInternal", flags);

            return (LoadingState)method.Invoke(scene, new object[1] { scene.handle });
        }

    }

}
