using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace FreezeFrame
{
    [DefaultExecutionOrder(-10000)]
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class FreezeFramePlugin : BaseUnityPlugin
    {
        public static FrameState state = FrameState.Continous;
        private PipeServer pipe;

        private void Awake()
        {
            Application.runInBackground = true;
            var harmony = new Harmony("com.deweydot.freezeframe");
            FrameController.Init(harmony);
            pipe = new PipeServer();
            pipe.Start();
        }

        private void Update()
        {
            FrameController.Update();
            this.HandleMessage(pipe);
        }

        private void LateUpdate()
        {
            FrameController.LateUpdate();
        }
    }

    public enum FrameState
    {
        Suspended,
        UpdateOnlyStep,
        UpdateBothStep,
        Continous
    }
}
