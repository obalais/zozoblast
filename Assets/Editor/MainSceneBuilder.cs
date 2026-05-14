using System.IO;
using Blockblast.Controller;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace Blockblast.Editor
{
    public static class MainSceneBuilder
    {
        private const string ScenesFolder = "Assets/Scenes";
        private const string MainScenePath = "Assets/Scenes/Main.unity";

        [MenuItem("Tools/Blockblast/Build Main Scene")]
        public static void Build()
        {
            if (!Directory.Exists(ScenesFolder))
            {
                Directory.CreateDirectory(ScenesFolder);
            }

            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cameraGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraGo.tag = "MainCamera";
            cameraGo.transform.position = new Vector3(0f, 0f, -10f);
            SceneManager.MoveGameObjectToScene(cameraGo, newScene);

            var eventSystemGo = new GameObject("EventSystem", typeof(EventSystem));
            AttachInputModule(eventSystemGo);
            SceneManager.MoveGameObjectToScene(eventSystemGo, newScene);

            var gameControllerGo = new GameObject("GameController");
            gameControllerGo.AddComponent<GameController>();
            SceneManager.MoveGameObjectToScene(gameControllerGo, newScene);

            bool saved = EditorSceneManager.SaveScene(newScene, MainScenePath);
            if (!saved)
            {
                Debug.LogError($"[Blockblast] Failed to save scene at {MainScenePath}");
                return;
            }

            AddSceneToBuildSettingsAsFirst(MainScenePath);

            EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);

            Debug.Log($"[Blockblast] Main scene created at {MainScenePath}. Press Play to start the game.");
        }

        private static void AttachInputModule(GameObject eventSystemGo)
        {
#if ENABLE_INPUT_SYSTEM
            var module = eventSystemGo.AddComponent<InputSystemUIInputModule>();
            module.AssignDefaultActions();
#else
            eventSystemGo.AddComponent<StandaloneInputModule>();
#endif
        }

        private static void AddSceneToBuildSettingsAsFirst(string scenePath)
        {
            var existingScenes = EditorBuildSettings.scenes;

            int existingIndex = -1;
            for (int sceneIndex = 0; sceneIndex < existingScenes.Length; sceneIndex++)
            {
                if (existingScenes[sceneIndex].path == scenePath)
                {
                    existingIndex = sceneIndex;
                    break;
                }
            }

            var newSceneList = new System.Collections.Generic.List<EditorBuildSettingsScene>();
            newSceneList.Add(new EditorBuildSettingsScene(scenePath, true));
            for (int sceneIndex = 0; sceneIndex < existingScenes.Length; sceneIndex++)
            {
                if (sceneIndex == existingIndex) continue;
                newSceneList.Add(existingScenes[sceneIndex]);
            }
            EditorBuildSettings.scenes = newSceneList.ToArray();
        }
    }
}
