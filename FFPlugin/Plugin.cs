using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace FreezeFrame
{
    [DefaultExecutionOrder(-10000)]
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public partial class Plugin : BaseUnityPlugin
    {
        private PipeController pipe = new PipeController();
        private FrameController frame;
        private SettingsController settings = new SettingsController();
        private VirtualInput input;
        private bool isLoading = false;

        private void Awake()
        {
            Application.runInBackground = true;
            var harmony = new Harmony("com.deweydot.freezeframe");
            harmony.CreateClassProcessor(typeof(TimePatch)).Patch();
            harmony.CreateClassProcessor(typeof(DeltaTimePatch)).Patch();
            harmony.CreateClassProcessor(typeof(SaveBindingsPatch)).Patch();
            harmony.CreateClassProcessor(typeof(CommitPrefsPatch)).Patch();
            frame = new FrameController(harmony);
            pipe.RunAsync();
        }

        private void Update()
        {
            if (input == null) input = new VirtualInput();
            frame.Update();
            if (isLoading) return;
            PipeController.Message? msg = pipe.Read();
            if (msg != null) this.Parse(msg.Value);
        }

        private void LateUpdate()
        {
            frame.LateUpdate();
        }

        public void Enable()
        {
            frame.Enable();
            input.Enable();
            settings.Apply();
        }

        public void Disable()
        {
            frame.Disable();
            input.Disable();
            settings.Revert();
        }

        public void Advance(bool fixedUpdate, InputState? state)
        {
            frame.Advance(fixedUpdate);
            if (state != null) input.queueState(state.Value.keyboard, state.Value.mouse);
        }

        public void LoadScene(string sceneName)
        {
            StartCoroutine(LoadAndSuspend(sceneName));
        }
    }
}
