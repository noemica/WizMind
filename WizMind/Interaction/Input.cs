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

        AltCtrl = Alt | Ctrl,

        AltShift = Alt | Shift,

        CtrlShift = Ctrl | Shift,

        AltCtrlShift = Alt | Ctrl | Shift,
    }

    public enum MouseButton
    {
        LeftButton,
        RightButton,
        MiddleButton,
    }

    public class Input(CogmindProcess cogmindProcess)
    {
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

        private uint? altScanCode;
        private uint? ctrlScanCode;
        private uint? shiftScanCode;
        private readonly CogmindProcess cogmindProcess = cogmindProcess;

        private uint AltScanCode =>
            this.altScanCode ??= MapVirtualKeyA((uint)Keys.Menu, VirtualKeyMapType.MAPVK_VK_TO_VSC);

        private uint CtrlScanCode =>
            this.ctrlScanCode ??= MapVirtualKeyA(
                (uint)Keys.ControlKey,
                VirtualKeyMapType.MAPVK_VK_TO_VSC
            );

        private uint ShiftScanCode =>
            this.shiftScanCode ??= MapVirtualKeyA(
                (uint)Keys.ShiftKey,
                VirtualKeyMapType.MAPVK_VK_TO_VSC
            );

        /// <summary>
        /// Sends a single keystroke with optional modifiers.
        /// </summary>
        /// <param name="key">The key to send.</param>
        /// <param name="keyModifier">Any key modifiers to apply.</param>
        /// <param name="waitForResponse">Whether to wait for the key to be processed or not.</param>
        /// <remarks>
        /// It may speed some things up to not wait for a response but it adds
        /// some extra safety so this should probably always remain true unless
        /// we're entering a super long string. Also, mixing blocking
        /// SendMessage vs non-blocking PostMessage keypresses can result in
        /// out of order key processing.
        /// <</remarks>
        public void SendKeystroke(
            Keys key,
            KeyModifier keyModifier = KeyModifier.None,
            bool waitForResponse = true
        )
        {
            // Map the virtual keycode to the scancode
            var keyVal = (uint)key;
            var scanCode = MapVirtualKeyA(keyVal, VirtualKeyMapType.MAPVK_VK_TO_VSC);

            // Send modifier keys first
            this.SendModifierKeysDown(keyModifier, waitForResponse);

            // Press and unpress the actual key
            this.SendKeyDown(keyVal, scanCode, waitForResponse);
            this.SendKeyUp(keyVal, scanCode, waitForResponse);

            // Unpress modifier keys
            this.SendModifierKeysUp(keyModifier, waitForResponse);
        }

        /// <summary>
        /// Sends a mouse press for the specified button at the given coordinates.
        /// If no coordinates are provided, the current mouse position is used.
        /// </summary>
        /// <param name="button">The mouse button to press.</param>
        /// <param name="mousePosition">The position of the mouse, or <c>null</c> to default.</param>
        /// <param name="keyModifier">Any optional key modifiers to send for the mouse press.</param>
        public void SendMousepress(
            MouseButton button,
            (int X, int Y)? mousePosition = null,
            KeyModifier keyModifier = KeyModifier.None
        )
        {
            // Send modifier keys first
            this.SendModifierKeysDown(keyModifier, true);

            // Press and unpress the mouse button
            this.SendMouseDown(button, mousePosition);
            this.SendMouseUp(button, mousePosition);

            // Unpress modifier keys
            this.SendModifierKeysUp(keyModifier, true);
        }

        /// <summary>
        /// Sends a string as a series of keystrokes.
        /// </summary>
        /// <param name="value">The string to send.</param>
        /// <param name="waitForResponse">Whether to wait for the key to be processed or not.</param>
        public void SendString(string value, bool waitForResponse = true)
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
                Thread.Sleep(TimeDuration.EnterStringSleep);
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

        private void SendModifierKeysDown(KeyModifier keyModifier, bool waitForResponse)
        {
            var hasAlt = keyModifier.HasFlag(KeyModifier.Alt);
            var hasCtrl = keyModifier.HasFlag(KeyModifier.Ctrl);
            var hasShift = keyModifier.HasFlag(KeyModifier.Shift);

            if (hasAlt)
            {
                this.SendKeyDown((uint)Keys.Menu, this.AltScanCode, waitForResponse);
            }

            if (hasCtrl)
            {
                this.SendKeyDown((uint)Keys.ControlKey, this.CtrlScanCode, waitForResponse);
            }

            if (hasShift)
            {
                this.SendKeyDown((uint)Keys.ShiftKey, this.ShiftScanCode, waitForResponse);
            }
        }

        private void SendModifierKeysUp(KeyModifier keyModifier, bool waitForResponse)
        {
            var hasAlt = keyModifier.HasFlag(KeyModifier.Alt);
            var hasCtrl = keyModifier.HasFlag(KeyModifier.Ctrl);
            var hasShift = keyModifier.HasFlag(KeyModifier.Shift);

            if (hasAlt)
            {
                this.SendKeyUp((uint)Keys.Menu, this.AltScanCode, waitForResponse);
            }

            if (hasCtrl)
            {
                this.SendKeyUp((uint)Keys.ControlKey, this.CtrlScanCode, waitForResponse);
            }

            if (hasShift)
            {
                this.SendKeyUp((uint)Keys.ShiftKey, this.ShiftScanCode, waitForResponse);
            }
        }

        private void SendMouseDown(MouseButton button, (int X, int Y)? mousePosition)
        {
            var window = this.cogmindProcess.Process.MainWindowHandle;

            int x;
            int y;

            if (mousePosition == null)
            {
                if (!GetCursorPos(out var point))
                {
                    throw new Exception("Failed to get cursor position");
                }

                x = point.X;
                y = point.Y;
            }
            else
            {
                x = mousePosition.Value.X;
                y = mousePosition.Value.Y;
            }

            // Build lParam flags
            // See https://learn.microsoft.com/en-us/windows/win32/inputdev/about-mouse-input#message-parameters
            var lParam = (uint)x | ((uint)y << 16);

            // Determine mouse event
            var messageType = button switch
            {
                MouseButton.LeftButton => WindowMessage.WM_LBUTTONDOWN,
                MouseButton.RightButton => WindowMessage.WM_RBUTTONDOWN,
                MouseButton.MiddleButton => WindowMessage.WM_MBUTTONDOWN,
                _ => throw new ArgumentException("Invalid mouse button"),
            };

            // Technically should include extra flags in wparam to indicate
            // which mouse buttons are being pressed and ctrl/shift state but
            // they are not used by SDL so always send wParam of 0 for now
            SendMessage(window, messageType, 0, lParam);
        }

        private void SendMouseUp(MouseButton button, (int x, int y)? mousePosition)
        {
            var window = this.cogmindProcess.Process.MainWindowHandle;

            int x;
            int y;

            if (mousePosition == null)
            {
                if (!GetCursorPos(out var point))
                {
                    throw new Exception("Failed to get cursor position");
                }

                x = point.X;
                y = point.Y;
            }
            else
            {
                x = mousePosition.Value.x;
                y = mousePosition.Value.y;
            }

            // Build lParam flags
            // See https://learn.microsoft.com/en-us/windows/win32/inputdev/about-mouse-input#message-parameters
            var lParam = (uint)x | ((uint)y << 16);

            // Determine mouse event
            var messageType = button switch
            {
                MouseButton.LeftButton => WindowMessage.WM_LBUTTONUP,
                MouseButton.RightButton => WindowMessage.WM_RBUTTONUP,
                MouseButton.MiddleButton => WindowMessage.WM_MBUTTONUP,
                _ => throw new ArgumentException("Invalid mouse button"),
            };

            // Technically should include extra flags in wparam to indicate
            // which mouse buttons are being pressed and ctrl/shift state but
            // they are not used by SDL so always send wParam of 0 for now
            SendMessage(window, messageType, 0, lParam);
        }
    }
}
