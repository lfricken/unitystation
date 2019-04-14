using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class BuildingTools : Editor
{
    [MenuItem("Tools/Build Tools/Build Headless & 2 Clients")]
    public static void BuildHeadlessAndClients()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Lobby.unity");
        var gameData = FindObjectOfType<GameData>();
        gameData.testServer = true;
        EditorUtility.SetDirty(gameData);
        PlayerPrefs.SetInt("buildprocess_01", 1);
        PlayerPrefs.Save();
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new [] { "Assets/scenes/Lobby.unity", "Assets/scenes/OutpostStation.unity" };
        buildPlayerOptions.locationPathName = "../Builds/Headless";
        buildPlayerOptions.target = EditorUserBuildSettings.activeBuildTarget;
        BuildPipeline.BuildPlayer(buildPlayerOptions);
    }

    public static async void PerformClientBuildForHeadless()
    {
        await Task.Delay(2000);
        PlayerPrefs.SetInt("buildprocess_01", 2);
        PlayerPrefs.Save();
        var gameData = FindObjectOfType<GameData>();
        gameData.testServer = false;
        EditorUtility.SetDirty(gameData);
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new [] { "Assets/scenes/Lobby.unity", "Assets/scenes/OutpostStation.unity" };
        buildPlayerOptions.locationPathName = "../Builds/Client";
        buildPlayerOptions.target = EditorUserBuildSettings.activeBuildTarget;
        BuildPipeline.BuildPlayer(buildPlayerOptions);
    }
}