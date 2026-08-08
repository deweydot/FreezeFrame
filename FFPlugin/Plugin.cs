using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace FreezeFrame
{
    [DefaultExecutionOrder(-10000)]
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public partial class Plugin : BaseUnityPlugin
    {
        public static FrameState state = FrameState.Continous;
        private PipeServer pipe = new PipeServer();
        private FrameController fc;
        private bool isLoading = false;

        private void Awake()
        {
            Application.runInBackground = true;
            var harmony = new Harmony("com.deweydot.freezeframe");
            fc = new FrameController(harmony);
            pipe.RunAsync();
        }

        private void Update()
        {
            fc.Update();
            if (isLoading) return;
            PipeServer.Message? msg = pipe.Read();
            if (msg != null) this.Parse(msg.Value);
        }

        private void LateUpdate()
        {
            fc.LateUpdate();
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
