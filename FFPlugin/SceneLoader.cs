using System.Collections;
using UnityEngine;

namespace FreezeFrame
{
    partial class Plugin
    {
        private IEnumerator LoadAndSuspend(string sceneName)
        {
            isLoading = true;
            Coroutine loadScene = SceneHelper.LoadSceneAsync(sceneName, false);
            yield return loadScene;
            fc.Enable();
            isLoading = false;
        }
    }
}
