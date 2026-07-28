using System;
using System.IO;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace FreezeFrame {
    public static class MessageHandler {
        public static void HandleMessage(this FreezeFrame ff, ManagedPipeServer pipe) {
            byte[] msg = pipe.Read();
            if (msg == null) return;
            if (msg.Length < 1 || (msg[0] > 127 && msg.Length < 4)) throw new InvalidDataException();
            switch (msg[0]) {
                // Payloadless (0 to 127)
                case 0: // Disable frame advance
                    FrameController.Disable();
                    break;
                case 1: // Enable frame advance
                    FrameController.Enable();
                    break;
                case 2: // Step frame (fixed update)
                    FrameController.Advance(true, null);
                    break;
                case 3: // Step frame (update only)
                    FrameController.Advance(false, null);
                    break;

                // Payloaded (128 to 255)
                case 130: // Step frame (fixed update)
                    FrameController.Advance(true, toInputState(msg[4..]));
                    break;
                case 131: // Step frame (update only)
                    FrameController.Advance(false, toInputState(msg[4..]));
                    break;
            }
            return;
        }

        private static InputState? toInputState(byte[] data) {
            // Protocol for input states
            // [4 bytes][4 bytes] - mouse position (x, y)
            // [4 bytes][4 bytes] - mouse delta (x, y)
            // [1 byte] - mouse buttons bitmask
            // [variable bytes] - keys pressed
            if (data.Length < 16) throw new InvalidDataException();
            MouseState m = new MouseState {
                position = new UnityEngine.Vector2(BitConverter.ToSingle(data, 0), BitConverter.ToSingle(data, 4)),
                delta = new UnityEngine.Vector2(BitConverter.ToSingle(data, 8), BitConverter.ToSingle(data, 12)),
                buttons = data[16]
            };
            KeyboardState kb = new KeyboardState();
            for (int i = 17; i < data.Length; i++) kb.Set((Key) data[i], true);
            return new InputState {
                keyboard = kb,
                mouse = m
            };
        }
    }
}