using System;
using System.Runtime.InteropServices;
using UnityEngine.InputSystem.Utilities;

namespace UnityEngine.InputSystem.LowLevel
{
    /// <summary>
    /// Command to query the name of the current keyboard layout from a device.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = InputDeviceCommand.kBaseCommandSize + kMaxNameLength)]
    public unsafe struct QueryKeyboardLayoutCommand : IInputDeviceCommandInfo
    {
        public static FourCC Type { get { return new FourCC('K', 'B', 'L', 'T'); } }

        internal const int kMaxNameLength = 256;

        [FieldOffset(0)]
        public InputDeviceCommand baseCommand;

        [FieldOffset(InputDeviceCommand.kBaseCommandSize)]
        public fixed byte nameBuffer[kMaxNameLength];

        public string ReadLayoutName()
        {
            fixed(QueryKeyboardLayoutCommand * thisPtr = &this)
            {
                return StringHelpers.ReadStringFromBuffer(new IntPtr(thisPtr->nameBuffer), kMaxNameLength);
            }
        }

        public FourCC typeStatic
        {
            get { return Type; }
        }

        public static QueryKeyboardLayoutCommand Create()
        {
            return new QueryKeyboardLayoutCommand
            {
                baseCommand = new InputDeviceCommand(Type, InputDeviceCommand.kBaseCommandSize + kMaxNameLength)
            };
        }
    }
}
