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
        private PipeServer pipe;

        private void Awake()
        {
            Application.runInBackground = true;
            var harmony = new Harmony("com.deweydot.freezeframe");
            FrameController.Init(harmony);
            pipe = new PipeServer();
            pipe.RunAsync();
            isLoading = false;
        }

        private void Update()
        {
            FrameController.Update();
            if (isLoading) return;
            PipeServer.Message? msg = pipe.Read();
            if (msg != null) this.Parse(msg.Value);
        }

        private void LateUpdate()
        {
            FrameController.LateUpdate();
        }

        public bool isLoading { private get; set; }
    }

    public enum FrameState
    {
        Suspended,
        UpdateOnlyStep,
        UpdateBothStep,
        Continous
    }
}
