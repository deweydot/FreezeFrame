using System;
using System.IO;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace FreezeFrame.Pipe
{
    partial class Plugin
    {
        private static readonly Key[] KeyBitmaskOrder =
        {
            Key.W, Key.A, Key.S, Key.D, Key.LeftShift, Key.LeftCtrl, Key.Space, Key.Comma,
            Key.Period, Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4, Key.Digit5, Key.E, Key.Q,
            Key.G, Key.F, Key.V, Key.R
        };

        private void Parse(PipeServer.Message msg)
        {
            if ((msg.opcode & Protocol.Consts.PayloadFlag) == 0)
            {
                switch ((Protocol.Opcode)(msg.opcode & Protocol.Consts.OpcodeMask))
                {
                    case Protocol.Opcode.Enable:
                        Disable();
                        break;
                    case Protocol.Opcode.Disable:
                        Enable();
                        break;
                    case Protocol.Opcode.StepFixed:
                        Advance(true, null);
                        break;
                    case Protocol.Opcode.StepUpdate:
                        Advance(false, null);
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
                        Advance(true, toInputState(msg.payload));
                        break;
                    case Protocol.Opcode.StepUpdate:
                        Advance(false, toInputState(msg.payload));
                        break;
                    case Protocol.Opcode.LoadScene:
                        LoadScene(msg.payload.ToString());
                        break;
                    default:
                        throw new InvalidDataException("Unrecognized Opcode");
                }
            }
        }

        private static TAS.InputState? toInputState(byte[] data)
        {
            if (data.Length != 11) throw new InvalidDataException("Invalid Message Format");
            MouseState m = new MouseState
            {
                delta = new UnityEngine.Vector2(BitConverter.ToSingle(data, 0), BitConverter.ToSingle(data, 4))
            };
            uint mask = (uint)(data[8] | (data[9] << 8) | (data[10] << 16));
            KeyboardState kb = new KeyboardState();
            for (int i = 0; i < KeyBitmaskOrder.Length; i++)
                if ((mask & (1u << i)) != 0) kb.Set(KeyBitmaskOrder[i], true);
            return new TAS.InputState { keyboard = kb, mouse = m };
        }
    }
}