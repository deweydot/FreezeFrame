using System;
using System.IO;
using UnityEngine.InputSystem.LowLevel;
using static FreezeFrame.Protocol;

namespace FreezeFrame
{
    static class MessageHandler
    {
        public static void HandleMessage(this Plugin ff, PipeServer pipe)
        {
            PipeServer.Message? data = pipe.Read();
            if (data == null) return;
            PipeServer.Message msg = data.Value;
            if ((msg.opcode & PayloadFlag) == 0)
            {
                switch ((Opcode)(msg.opcode & OpcodeMask))
                {
                    case Opcode.Enable:
                        FrameController.Disable();
                        break;
                    case Opcode.Disable:
                        FrameController.Enable();
                        break;
                    case Opcode.StepFixed:
                        FrameController.Advance(true, null);
                        break;
                    case Opcode.StepUpdate:
                        FrameController.Advance(false, null);
                        break;
                    default:
                        throw new InvalidDataException("Unrecognized Opcode");
                }
            }
            else
            {
                if (msg.payload == null) throw new InvalidDataException("Invalid Message Format");
                switch ((Opcode)(msg.opcode & OpcodeMask))
                {
                    case Opcode.StepFixed:
                        FrameController.Advance(true, toInputState(msg.payload));
                        break;
                    case Opcode.StepUpdate:
                        FrameController.Advance(false, toInputState(msg.payload));
                        break;
                    default:
                        throw new InvalidDataException("Unrecognized Opcode");
                }
            }
            return;
        }

        private static InputState? toInputState(byte[] data)
        {
            if (data.Length < 8) throw new InvalidDataException("Invalid Message Format");
            MouseState m = new MouseState
            {
                delta = new UnityEngine.Vector2(BitConverter.ToSingle(data, 0), BitConverter.ToSingle(data, 4)),
                buttons = data[8]
            };
            KeyboardState kb = new KeyboardState();
            for (int i = 9; i < data.Length; i++) kb.Set((UnityEngine.InputSystem.Key)data[i], true);
            return new InputState
            {
                keyboard = kb,
                mouse = m
            };
        }
    }
}