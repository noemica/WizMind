using System.Runtime.InteropServices;

namespace WizMind.Imports
{
    public static class User32
    {
        public enum MapType: uint
        {
            MAPVK_VK_TO_VSC = 0X00,
            MAPVK_VSC_TO_VK = 0X01,
            MAPVK_VK_TO_CHAR = 0X02,
            MAPVK_VSC_TO_VK_EX = 0X03,
            MAPVK_VK_TO_VSC_EX = 0X04,
        }

        public enum WindowMessage : uint
        {
            WM_KEYDOWN = 0x0100,
            WM_KEYUP = 0x0101,
        }

        [DllImport("user32.dll")]
        public static extern uint MapVirtualKeyA(uint uCode, MapType uMapType);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern nint SendMessage(nint hWnd, WindowMessage Msg, nuint wParam, nuint lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern nint PostMessage(nint hWnd, WindowMessage Msg, nuint wParam, nuint lParam);
    }
}
