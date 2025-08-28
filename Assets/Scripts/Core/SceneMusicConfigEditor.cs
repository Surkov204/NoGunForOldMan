#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

[CustomEditor(typeof(SceneMusicConfig))]
public class SceneMusicConfigEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var config = (SceneMusicConfig)target;

        if (GUILayout.Button("Add Scene Entry"))
        {
            config.sceneMusicList.Add(new SceneMusicConfig.SceneMusicEntry());
        }

        EditorBuildSettingsScene[] buildScenes = EditorBuildSettings.scenes;
        string[] sceneNames = new string[buildScenes.Length];
        for (int i = 0; i < buildScenes.Length; i++)
        {
            sceneNames[i] = Path.GetFileNameWithoutExtension(buildScenes[i].path);
        }

        for (int i = 0; i < config.sceneMusicList.Count; i++)
        {
            EditorGUILayout.BeginVertical("box");

            int currentIndex = Mathf.Max(0, System.Array.IndexOf(sceneNames, config.sceneMusicList[i].sceneName));
            int newIndex = EditorGUILayout.Popup("Scene", currentIndex, sceneNames);

            if (newIndex >= 0 && newIndex < sceneNames.Length)
                config.sceneMusicList[i].sceneName = sceneNames[newIndex];

            config.sceneMusicList[i].musicClip = (AudioClip)EditorGUILayout.ObjectField("Music Clip", config.sceneMusicList[i].musicClip, typeof(AudioClip), false);

            if (GUILayout.Button("Remove"))
            {
                config.sceneMusicList.RemoveAt(i);
            }

            EditorGUILayout.EndVertical();
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(config);
        }
    }
}
#endif
