using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static WizMind.Imports.User32;

namespace WizMind.Interaction
{
    [Flags]
    public enum KeyModifier
    {
        None = 0,

        Alt = 0b1,

        Ctrl = 0b10,

        Shift = 0b100,

        AltShift = Alt | Shift,
    }

    public class Input
    {
        private readonly CogmindProcess cogmindProcess;

        public Input(CogmindProcess cogmindProcess)
        {
            this.cogmindProcess = cogmindProcess;
        }

        public void SendKeystroke(Keys key, KeyModifier keyModifier = KeyModifier.None)
        {
            var keyVal = (uint)key;
            var scanCode = MapVirtualKeyA(keyVal, MapType.MAPVK_VK_TO_VSC);

            var hasAlt = keyModifier.HasFlag(KeyModifier.Alt);
            var hasCtrl = keyModifier.HasFlag(KeyModifier.Ctrl);
            var hasShift = keyModifier.HasFlag(KeyModifier.Shift);

            uint? altKeyVal = null;
            uint? altScanCode = null;
            uint? shiftKeyVal = null;
            uint? shiftScanCode = null;

            if (hasAlt)
            {
                altKeyVal = (uint)Keys.Menu;
                altScanCode = MapVirtualKeyA(altKeyVal.Value, MapType.MAPVK_VK_TO_VSC);

                this.SendKeyDown(altKeyVal.Value, altScanCode.Value);
            }

            if (hasShift)
            {
                shiftKeyVal = (uint)Keys.ShiftKey;
                shiftScanCode = MapVirtualKeyA(shiftKeyVal.Value, MapType.MAPVK_VK_TO_VSC);

                this.SendKeyDown(shiftKeyVal.Value, shiftScanCode.Value);
            }

            this.SendKeyDown(keyVal, scanCode);

            if (hasAlt)
            {
                this.SendKeyUp(altKeyVal!.Value, altScanCode!.Value);
            }

            if (hasShift)
            {
                this.SendKeyUp(shiftKeyVal!.Value, shiftScanCode!.Value);
            }

            this.SendKeyUp(keyVal, scanCode);
        }

        private void SendKeyDown(uint key, uint scanCode)
        {
            var window = this.cogmindProcess.Process.MainWindowHandle;

            // Build lParam flags
            // See https://learn.microsoft.com/en-us/windows/win32/inputdev/about-keyboard-input#keystroke-message-flags
            // Start with 1 key repeat (i.e. first press)
            var lParam = 0x00000001u;

            // Add scancode
            lParam |= scanCode << 16;

            PostMessage(window, WindowMessage.WM_KEYDOWN, key, lParam);
        }

        private void SendKeyUp(uint key, uint scanCode)
        {
            var window = this.cogmindProcess.Process.MainWindowHandle;

            // Build lParam flags
            // See https://learn.microsoft.com/en-us/windows/win32/inputdev/about-keyboard-input#keystroke-message-flags
            // Start with 1 key repeat (i.e. first press)
            var lParam = 0x00000001u;

            // Add scancode
            lParam |= scanCode << 16;

            // Indicate previous key state was up
            lParam |= 0xC0000000;

            PostMessage(window, WindowMessage.WM_KEYUP, key, lParam);
        }
    }
}
