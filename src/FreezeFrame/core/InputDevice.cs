using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem;

namespace FreezeFrame {
    class InputDevice {
        public Keyboard keyboard;
        public Mouse mouse;
        
        public InputDevice() {
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            keyboard = InputSystem.AddDevice<Keyboard>();
            mouse = InputSystem.AddDevice<Mouse>();
        }
        
        public void Enable() {
            InputSystem.EnableDevice(keyboard);
            InputSystem.EnableDevice(mouse);
        }
        
        public void Disable() {
            InputSystem.DisableDevice(keyboard);
            InputSystem.DisableDevice(mouse);
        }

        public void queueState(KeyboardState kb, MouseState m) {
            InputSystem.QueueStateEvent(keyboard, kb);
            InputSystem.QueueStateEvent(mouse, m);
        }
    }

    struct InputState {
        public KeyboardState keyboard;
        public MouseState mouse;
    }
}