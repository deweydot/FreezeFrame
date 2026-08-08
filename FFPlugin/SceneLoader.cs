using System.Collections;
using UnityEngine;

namespace FreezeFrame
{
    static class SceneLoader
    {
        public static void LoadScene(this Plugin plugin, string sceneName)
        {
            plugin.StartCoroutine(plugin.LoadAndSuspend(sceneName));
        }

        private static IEnumerator LoadAndSuspend(this Plugin plugin, string sceneName)
        {
            plugin.isLoading = true;
            Coroutine loadScene = SceneHelper.LoadSceneAsync(sceneName, false);
            yield return loadScene;
            FrameController.Enable();
            plugin.isLoading = false;
        }
    }
}
