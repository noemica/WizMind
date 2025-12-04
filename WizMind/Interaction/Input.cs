using System.ComponentModel.DataAnnotations;
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

        private readonly Dictionary<char, (Keys key, KeyModifier modifier)> charToKeyCode =
            new Dictionary<char, (Keys key, KeyModifier modifier)>
        {
            { ' ', (Keys.Space, KeyModifier.None) },
            { '"', (Keys.OemQuotes, KeyModifier.Shift) },
            { '\'', (Keys.OemQuotes, KeyModifier.None) },
            { '-', (Keys.OemMinus, KeyModifier.None) },
            { '.', (Keys.OemPeriod, KeyModifier.None) },
            { '/', (Keys.OemQuestion, KeyModifier.None) },
            { '0', (Keys.D0, KeyModifier.None) },
            { '1', (Keys.D1, KeyModifier.None) },
            { '2', (Keys.D2, KeyModifier.None) },
            { '3', (Keys.D3, KeyModifier.None) },
            { '4', (Keys.D4, KeyModifier.None) },
            { '5', (Keys.D5, KeyModifier.None) },
            { '6', (Keys.D6, KeyModifier.None) },
            { '7', (Keys.D7, KeyModifier.None) },
            { '8', (Keys.D8, KeyModifier.None) },
            { '9', (Keys.D9, KeyModifier.None) },
            { 'a', (Keys.A, KeyModifier.None) },
            { 'b', (Keys.B, KeyModifier.None) },
            { 'c', (Keys.C, KeyModifier.None) },
            { 'd', (Keys.D, KeyModifier.None) },
            { 'e', (Keys.E, KeyModifier.None) },
            { 'f', (Keys.F, KeyModifier.None) },
            { 'g', (Keys.G, KeyModifier.None) },
            { 'h', (Keys.H, KeyModifier.None) },
            { 'i', (Keys.I, KeyModifier.None) },
            { 'j', (Keys.J, KeyModifier.None) },
            { 'k', (Keys.K, KeyModifier.None) },
            { 'l', (Keys.L, KeyModifier.None) },
            { 'm', (Keys.M, KeyModifier.None) },
            { 'n', (Keys.N, KeyModifier.None) },
            { 'o', (Keys.O, KeyModifier.None) },
            { 'p', (Keys.P, KeyModifier.None) },
            { 'q', (Keys.Q, KeyModifier.None) },
            { 'r', (Keys.R, KeyModifier.None) },
            { 's', (Keys.S, KeyModifier.None) },
            { 't', (Keys.T, KeyModifier.None) },
            { 'u', (Keys.U, KeyModifier.None) },
            { 'v', (Keys.V, KeyModifier.None) },
            { 'w', (Keys.W, KeyModifier.None) },
            { 'x', (Keys.X, KeyModifier.None) },
            { 'y', (Keys.Y, KeyModifier.None) },
            { 'z', (Keys.Z, KeyModifier.None) },
            { '[', (Keys.OemOpenBrackets, KeyModifier.None) },
            { ']', (Keys.OemCloseBrackets, KeyModifier.None) },
        };

        public Input(CogmindProcess cogmindProcess)
        {
            this.cogmindProcess = cogmindProcess;
        }

        /// <summary>
        /// Sends a series of keystrokes.
        /// </summary>
        /// <param name="keys">The keys to send.</param>
        /// <param name="waitForResponse">Whether to wait for each key to be processed or not.</param>
        public void SendKeystrokes(ReadOnlySpan<Keys> keys, bool waitForResponse = false)
        {
            foreach (var key in keys)
            {
                this.SendKeystroke(key);
            }
        }

        /// <summary>
        /// Sends a single keystroke with optional modifiers.
        /// </summary>
        /// <param name="key">The key to send.</param>
        /// <param name="keyModifier">Any key modifiers to apply.</param>
        /// <param name="waitForResponse">Whether to wait for the key to be processed or not.</param>
        public void SendKeystroke(Keys key, KeyModifier keyModifier = KeyModifier.None, bool waitForResponse = false)
        {
            // Map the virtual keycode to the scancode
            var keyVal = (uint)key;
            var scanCode = MapVirtualKeyA(keyVal, MapType.MAPVK_VK_TO_VSC);

            var hasAlt = keyModifier.HasFlag(KeyModifier.Alt);
            var hasCtrl = keyModifier.HasFlag(KeyModifier.Ctrl);
            var hasShift = keyModifier.HasFlag(KeyModifier.Shift);

            uint? altKeyVal = null;
            uint? altScanCode = null;
            uint? ctrlKeyVal = null;
            uint? ctrlScanCode = null;
            uint? shiftKeyVal = null;
            uint? shiftScanCode = null;

            // Press alt/shift/ctrl modifiers first
            if (hasAlt)
            {
                altKeyVal = (uint)Keys.Menu;
                altScanCode = MapVirtualKeyA(altKeyVal.Value, MapType.MAPVK_VK_TO_VSC);

                this.SendKeyDown(altKeyVal.Value, altScanCode.Value, waitForResponse);
            }

            if (hasCtrl)
            {
                ctrlKeyVal = (uint)Keys.ControlKey;
                ctrlScanCode = MapVirtualKeyA(ctrlKeyVal.Value, MapType.MAPVK_VK_TO_VSC);

                this.SendKeyDown(ctrlKeyVal.Value, ctrlScanCode.Value, waitForResponse);
            }

            if (hasShift)
            {
                shiftKeyVal = (uint)Keys.ShiftKey;
                shiftScanCode = MapVirtualKeyA(shiftKeyVal.Value, MapType.MAPVK_VK_TO_VSC);

                this.SendKeyDown(shiftKeyVal.Value, shiftScanCode.Value, waitForResponse);
            }

            // Send the actual key
            this.SendKeyDown(keyVal, scanCode, waitForResponse);

            // Unpress key modifiers now
            if (hasAlt)
            {
                this.SendKeyUp(altKeyVal!.Value, altScanCode!.Value, waitForResponse);
            }

            if (hasCtrl)
            {
                this.SendKeyUp(ctrlKeyVal!.Value, ctrlScanCode!.Value, waitForResponse);
            }

            if (hasShift)
            {
                this.SendKeyUp(shiftKeyVal!.Value, shiftScanCode!.Value, waitForResponse);
            }

            // Unpress actual key
            this.SendKeyUp(keyVal, scanCode, waitForResponse);
        }

        /// <summary>
        /// Sends a string as a series of keystrokes.
        /// </summary>
        /// <param name="value">The string to send.</param>
        /// <param name="waitForResponse">Whether to wait for the key to be processed or not.</param>
        public void SendString(string value, bool waitForResponse = false)
        {
            var lowerString = value.ToLower();
            for (int i = 0; i < lowerString.Length; i++)
            {
                char keyChar = lowerString[i];
                if (!charToKeyCode.TryGetValue(keyChar, out var keyData))
                {
                    throw new Exception($"Tried to send unsupported character {keyChar}");
                }

                this.SendKeystroke(keyData.key, keyData.modifier, waitForResponse);
            }

            if (!waitForResponse)
            {
                // If we weren't waiting for a response, wait now for the whole
                // string to be processed anyway
                Thread.Sleep(SleepDuration.EnterString);
            }
        }

        private void SendKeyDown(uint key, uint scanCode, bool waitForResponse)
        {
            var window = this.cogmindProcess.Process.MainWindowHandle;

            // Build lParam flags
            // See https://learn.microsoft.com/en-us/windows/win32/inputdev/about-keyboard-input#keystroke-message-flags
            // Start with 1 key repeat (i.e. first press)
            var lParam = 0x00000001u;

            // Add scancode
            lParam |= scanCode << 16;

            if (waitForResponse)
            {
                SendMessage(window, WindowMessage.WM_KEYDOWN, key, lParam);
            }
            else
            {
                PostMessage(window, WindowMessage.WM_KEYDOWN, key, lParam);
            }
        }

        private void SendKeyUp(uint key, uint scanCode, bool waitForResponse)
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

            if (waitForResponse)
            {
                SendMessage(window, WindowMessage.WM_KEYUP, key, lParam);
            }
            else
            {
                PostMessage(window, WindowMessage.WM_KEYUP, key, lParam);
            }
        }
    }
}
