using System;
using System.IO;
using UnityEngine.InputSystem.LowLevel;

namespace FreezeFrame
{
    partial class Plugin
    {
        private void Parse(PipeServer.Message msg)
        {
            if ((msg.opcode & Protocol.Consts.PayloadFlag) == 0)
            {
                switch ((Protocol.Opcode)(msg.opcode & Protocol.Consts.OpcodeMask))
                {
                    case Protocol.Opcode.Enable:
                        fc.Disable();
                        break;
                    case Protocol.Opcode.Disable:
                        fc.Enable();
                        break;
                    case Protocol.Opcode.StepFixed:
                        fc.Advance(true, null);
                        break;
                    case Protocol.Opcode.StepUpdate:
                        fc.Advance(false, null);
                        break;
                    default:
                        throw new InvalidDataException("Unrecognized Opcode");
                }
            }
            else
            {
                if (msg.payload == null) throw new InvalidDataException("Invalid Message Format");
                switch ((Protocol.Opcode)(msg.opcode & Protocol.Consts.OpcodeMask))
                {
                    case Protocol.Opcode.StepFixed:
                        fc.Advance(true, toInputState(msg.payload));
                        break;
                    case Protocol.Opcode.StepUpdate:
                        fc.Advance(false, toInputState(msg.payload));
                        break;
                    case Protocol.Opcode.LoadScene:
                        LoadScene(msg.payload.ToString());
                        break;
                    default:
                        throw new InvalidDataException("Unrecognized Opcode");
                }
            }
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