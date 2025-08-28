#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

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

        for (int i = 0; i < config.sceneMusicList.Count; i++)
        {
            EditorGUILayout.BeginVertical("box");

            // Dropdown danh sách scene trong Build Settings
            string[] scenes = new string[UnityEditor.SceneManagement.EditorSceneManager.sceneCountInBuildSettings];
            for (int j = 0; j < scenes.Length; j++)
            {
                scenes[j] = System.IO.Path.GetFileNameWithoutExtension(UnityEditor.SceneManagement.EditorSceneManager.GetSceneByBuildIndex(j).path);
            }

            int currentIndex = Mathf.Max(0, System.Array.IndexOf(scenes, config.sceneMusicList[i].sceneName));
            int newIndex = EditorGUILayout.Popup("Scene", currentIndex, scenes);

            if (newIndex >= 0 && newIndex < scenes.Length)
                config.sceneMusicList[i].sceneName = scenes[newIndex];

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
