using BepInEx;
using HarmonyLib;
using System;
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
        private VirtualInput input;
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
            if (input == null) input = new VirtualInput();
            fc.Update();
            if (isLoading) return;
            PipeServer.Message? msg = pipe.Read();
            if (msg != null) this.Parse(msg.Value);
        }

        private void LateUpdate()
        {
            fc.LateUpdate();
        }

        public void Enable()
        {
            fc.Enable();
            input.Enable();
        }

        public void Disable()
        {
            fc.Disable();
            input.Disable();
        }

        public void Advance(bool fixedUpdate, InputState? state)
        {
            fc.Advance(fixedUpdate);
            if (state != null) input.queueState(state.Value.keyboard, state.Value.mouse);
        }

        public void LoadScene(string sceneName)
        {
            StartCoroutine(LoadAndSuspend(sceneName));
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
