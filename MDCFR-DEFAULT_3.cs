
// An All-In-One framework abstracting the most important classes that are used in .NET
// that are more easily and more consistently to be used.
// The framework was designed to host many different operations , with the last goal 
// to be everything accessible for everyone.

// Global namespaces
using System;
using System.Runtime.Versioning;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace ROOT
{

    /// <summary>
    /// This class contains the internal console implementation extensions , which some of them are exposed publicly.
    /// </summary>
    public static class ConsoleExtensions
    {
        internal enum ConsoleHandleOptions : System.UInt32
        {
            Input = 0xFFFFFFF6,
            Output = 0xFFFFFFF5,
            Error = 0xFFFFFFF4
        }

        // This value indicates whether the console is detached , and therefore 
        // it notifies the console functions to 'not' actually call the console code.
        internal static System.Boolean Detached = false;

        // An internal buffer for the Title commands.
        private static System.String T = T2;
        private const System.String T2 = "\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0" +
            "\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0" +
            "\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0";

        /// <summary>
        /// Define the Buffer Size when using the function <see cref="ROOT.MAIN.ReadConsoleText(ConsoleReadBufferOptions)"/> .
        /// </summary>
        public enum ConsoleReadBufferOptions : System.Int32
        {
            /// <summary>
            /// Buffer size is set to 1024.
            /// </summary>
            Small = 1024,
            /// <summary>
            /// Buffer size is set to 2048.
            /// </summary>
            Default = 2048,
            /// <summary>
            /// Buffer size is set to 3072.
            /// </summary>
            Large = 3072,
            /// <summary>
            /// Buffer size is set to 4096.
            /// </summary>
            VeryLarge = 4096,
            /// <summary>
            /// Buffer size is set to 8192.
            /// </summary>
            ExtravagantlyLarge = 8192,
            /// <summary>
            /// Buffer size is set to 16384.
            /// </summary>
            DoubleLarge = 16384,
            /// <summary>
            /// Buffer size is set to 32768.
            /// </summary>
            TripleLarge = 32768,
        }

        /// <summary>
        /// Gets the underlying KERNEL32 handle which this implementation uses to write any kind of data to console.
        /// </summary>
        /// <returns>A new <see cref="System.IntPtr"/> handle which is the handle for writing data to the console.</returns>
        [System.Security.SecurityCritical]
        public static System.IntPtr GetOutputHandle()
        {
            if (ConsoleInterop.OutputHandle == System.IntPtr.Zero)
            {
                ConsoleInterop.OutputHandle = ConsoleInterop.GetConsoleStream(ConsoleHandleOptions.Output);
                return ConsoleInterop.OutputHandle;
            }
            else { return ConsoleInterop.OutputHandle; }
        }

        /// <summary>
        /// Gets the underlying KERNEL32 handle which this implementation uses to read from the console.
        /// </summary>
        /// <returns>A new <see cref="System.IntPtr"/> handle which is the handle for reading data 
        /// from the console.</returns>
        [System.Security.SecurityCritical]
        public static System.IntPtr GetInputHandle()
        {
            if (ConsoleInterop.InputHandle == System.IntPtr.Zero)
            {
                ConsoleInterop.InputHandle = ConsoleInterop.GetConsoleStream(ConsoleHandleOptions.Output);
                return ConsoleInterop.InputHandle;
            }
            else { return ConsoleInterop.InputHandle; }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [System.Security.SecurityCritical]
        internal static void InitIfNotInitOut()
        {
            if (ConsoleInterop.OutputHandle == System.IntPtr.Zero)
            {
                ConsoleInterop.OutputHandle = GetOutputHandle();
            }
            return;
        }

        /// <summary>
        /// Globally set or get the console foreground color. This property is used for the exported MDCFR functions , 
        /// and this is equivalent to <see cref="System.Console.ForegroundColor"/> property.
        /// </summary>
        public static System.ConsoleColor ForegroundColor
        {
            [System.Security.SecurityCritical]
            get
            {
                if (ROOT.ConsoleExtensions.Detached == true) { return System.ConsoleColor.Gray; }
                InitIfNotInitOut();
                ConsoleInterop.GetBufferInfo(ConsoleInterop.OutputHandle, out CONSOLE_SCREEN_BUFFER_INFO CSBI);
                return ColorAttributeToConsoleColor((System.Int16)(CSBI.wAttributes & (System.Int16)ConsoleControlChars.ForegroundMask));
            }
            [System.Security.SecurityCritical]
            set
            {
                InitIfNotInitOut();
                SetForeColor(value);
            }
        }

        /// <summary>
        /// Globally set or get the console background color. This property is used for the exported MDCFR functions , 
        /// and this is equivalent to <see cref="System.Console.BackgroundColor"/> property.
        /// </summary>
        public static System.ConsoleColor BackgroundColor
        {
            [System.Security.SecurityCritical]
            get
            {
                if (ROOT.ConsoleExtensions.Detached == true) { return System.ConsoleColor.Black; }
                InitIfNotInitOut();
                ConsoleInterop.GetBufferInfo(ConsoleInterop.OutputHandle, out CONSOLE_SCREEN_BUFFER_INFO CSBI);
                return ColorAttributeToConsoleColor((System.Int16)(CSBI.wAttributes & (System.Int16)ConsoleControlChars.BackgroundMask));
            }
            [System.Security.SecurityCritical]
            set
            {
                InitIfNotInitOut();
                SetBackColor(value);
            }
        }

        /// <summary>
        /// Get or Set the Console's Output encoding as an <see cref="System.Text.Encoding"/> class.
        /// </summary>
        /// <exception cref="AggregateException">
        /// Occurs when the Code Page defined to the 
        /// console does not exist as an <see cref="System.Text.Encoding"/> class.</exception>
        /// <exception cref="InvalidOperationException">
        /// Occurs when the specified Code Page is invalid for the console.</exception>
        public static System.Text.Encoding OutputEncoding
        {
            [System.Security.SecurityCritical]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                System.UInt32 CP = ConsoleInterop.GetOutputEnc();
                if (CP == 0)
                {
                    throw new AggregateException("Error occured while getting the current code page!!!");
                }
                System.Text.Encoding TI = null;
                TI = System.Text.CodePagesEncodingProvider.Instance.GetEncoding((System.Int32)CP);
                if (TI == null)
                {
                    try
                    {
                        TI = System.Text.Encoding.GetEncoding((System.Int32)CP);
                        return TI;
                    }
                    catch (System.Exception EX)
                    {
                        throw new AggregateException($"Could not get the codepage set to the console: {CP} .", EX);
                    }
                }
                else { return TI; }
            }

            [System.Security.SecurityCritical]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                System.UInt32 CP = (System.UInt32)value.CodePage;
                if (ConsoleInterop.SetOutputEnc(CP) == 0)
                {
                    throw new InvalidOperationException("Cannot apply the specific code page as a console output encoding. \n" +
                        $"Code Page Identifier: {CP} \n" +
                        $"Code Page Name: {value.BodyName} \n" +
                        $"Code Page Web Name: {value.WebName} \n" +
                        $"Code Page Windows Name: {value.WindowsCodePage}");
                }
            }

        }

        /// <summary>
        /// Get or Set the Console's Input encoding as an <see cref="System.Text.Encoding"/> class.
        /// </summary>
        /// <exception cref="AggregateException">
        /// Occurs when the Code Page defined to the 
        /// console does not exist as an <see cref="System.Text.Encoding"/> class.</exception>
        /// <exception cref="InvalidOperationException">
        /// Occurs when the specified Code Page is invalid for the console.</exception>
        public static System.Text.Encoding InputEncoding
        {
            [System.Security.SecurityCritical]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                System.UInt32 CP = ConsoleInterop.GetInputEnc();
                if (CP == 0)
                {
                    throw new AggregateException("Error occured while getting the current code page!!!");
                }
                System.Text.Encoding TI = null;
                TI = System.Text.CodePagesEncodingProvider.Instance.GetEncoding((System.Int32)CP);
                if (TI == null)
                {
                    try
                    {
                        TI = System.Text.Encoding.GetEncoding((System.Int32)CP);
                        return TI;
                    }
                    catch (System.Exception EX)
                    {
                        throw new AggregateException($"Could not get the codepage set to the console: {CP} .", EX);
                    }
                }
                else { return TI; }
            }

            [System.Security.SecurityCritical]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                System.UInt32 CP = (System.UInt32)value.CodePage;
                if (ConsoleInterop.SetInputEnc(CP) == 0)
                {
                    throw new InvalidOperationException("Cannot apply the specific code page as a console input encoding. \n" +
                        $"Code Page Identifier: {CP} \n" +
                        $"Code Page Name: {value.BodyName} \n" +
                        $"Code Page Web Name: {value.WebName} \n" +
                        $"Code Page Windows Name: {value.WindowsCodePage}");
                }
            }

        }

        /// <summary>
        /// Get or Set the current console title. This property is equivalent to <see cref="System.Console.Title"/> property.
        /// </summary>
        public static System.String Title
        {
            [System.Security.SecurityCritical]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ConsoleInterop.GetTitle(T, T.Length);
                System.String I = T;
                T = T2;
                return MAIN.RemoveDefinedChars(I, '\0');
            }
            [System.Security.SecurityCritical]
            set { ConsoleInterop.SetTitle(value); }
        }

        /// <summary>
        /// Gets the original title , when the application attached to the console.
        /// </summary>
        public static System.String OriginalTitle
        {
            [System.Security.SecurityCritical]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ConsoleInterop.OriginalTitle(T, T.Length);
                System.String I = T;
                T = T2;
                return MAIN.RemoveDefinedChars(I, '\0');
            }
        }

        // Global Windows C structure defining the coordinates of an window.
        // Interpreted as System.Int16 points.
        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        internal struct COORD
        {
            internal short X;

            internal short Y;
        }

        // Global Windows C structure defining the size of an window.
        // Interpreted as System.Int16 points.
        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        internal struct SMALL_RECT
        {
            internal short Left;

            internal short Top;

            internal short Right;

            internal short Bottom;
        }

        // The Console Info for this console session. This table is filled by specific functions only.
        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        internal struct CONSOLE_SCREEN_BUFFER_INFO
        {
            internal COORD dwSize;

            internal COORD dwCursorPosition;

            internal short wAttributes;

            internal SMALL_RECT srWindow;

            internal COORD dwMaximumWindowSize;
        }

        internal static void SetForeColor(System.ConsoleColor Color)
        {
            new System.Security.Permissions.UIPermission(System.Security.Permissions.UIPermissionWindow.SafeTopLevelWindows).Demand();
            CONSOLE_SCREEN_BUFFER_INFO INF = GetCBufferInfo(true, out System.Boolean SU);
            System.Int16 attrs = INF.wAttributes;
            attrs = (short)(attrs & -16);
            attrs = (short)((ushort)attrs | (ushort)ConsoleColorToColorAttribute(Color, false));
            ConsoleInterop.DefineNewAttributes(ConsoleInterop.OutputHandle, attrs);
        }

        internal static void SetBackColor(System.ConsoleColor Color)
        {
            new System.Security.Permissions.UIPermission(System.Security.Permissions.UIPermissionWindow.SafeTopLevelWindows).Demand();
            CONSOLE_SCREEN_BUFFER_INFO INF = GetCBufferInfo(true, out System.Boolean SU);
            System.Int16 attrs = INF.wAttributes;
            attrs = (short)(attrs & -241);
            attrs = (short)((ushort)attrs | (ushort)ConsoleColorToColorAttribute(Color, true));
            ConsoleInterop.DefineNewAttributes(ConsoleInterop.OutputHandle, attrs);
        }

        [System.Security.SecuritySafeCritical]
        private static CONSOLE_SCREEN_BUFFER_INFO GetCBufferInfo(bool throwOnNoConsole, out bool succeeded)
        {
            succeeded = false;
            IntPtr consoleOutputHandle = ConsoleInterop.OutputHandle;
            if (consoleOutputHandle == System.IntPtr.Zero)
            {
                if (!throwOnNoConsole)
                {
                    return default(CONSOLE_SCREEN_BUFFER_INFO);
                }
                throw new System.IO.IOException("There is not any Console Spawned! ");
            }
            if (!ConsoleInterop.GetBufferInfo(consoleOutputHandle, out var lpConsoleScreenBufferInfo))
            {
                System.Boolean consoleScreenBufferInfo = ConsoleInterop.GetBufferInfo(consoleOutputHandle, out lpConsoleScreenBufferInfo);
                if (!consoleScreenBufferInfo)
                {
                    consoleScreenBufferInfo = ConsoleInterop.GetBufferInfo(consoleOutputHandle, out lpConsoleScreenBufferInfo);
                }
                if (!consoleScreenBufferInfo)
                {
                    System.Int32 lastWin32Error = Marshal.GetLastWin32Error();
                    if (lastWin32Error == 6 && !throwOnNoConsole)
                    {
                        return default(CONSOLE_SCREEN_BUFFER_INFO);
                    }
                    throw new System.AggregateException($"Win32 Exception detected. HRESULT is {lastWin32Error} .");
                }
            }
            return lpConsoleScreenBufferInfo;
        }

        internal enum ConsoleControlChars : System.Int32
        {
            FOREGROUND_BLUE = 0x0001, // text color contains blue.
            FOREGROUND_GREEN = 0x0002, // text color contains green.
            FOREGROUND_RED = 0x0004, // text color contains red.
            FOREGROUND_INTENSITY = 0x0008, // text color is intensified.
            BACKGROUND_BLUE = 0x0010, // background color contains blue.
            BACKGROUND_GREEN = 0x0020, // background color contains green.
            BACKGROUND_RED = 0x0040, // background color contains red.
            BACKGROUND_INTENSITY = 0x0080, // background color is intensified.
            COMMON_LVB_LEADING_BYTE = 0x0100, // Leading Byte of DBCS
            COMMON_LVB_TRAILING_BYTE = 0x0200, // Trailing Byte of DBCS
            COMMON_LVB_GRID_HORIZONTAL = 0x0400, // DBCS: Grid attribute: top horizontal.
            COMMON_LVB_GRID_LVERTICAL = 0x0800, // DBCS: Grid attribute: left vertical.
            COMMON_LVB_GRID_RVERTICAL = 0x1000, // DBCS: Grid attribute: right vertical.
            COMMON_LVB_REVERSE_VIDEO = 0x4000, // DBCS: Reverse fore/back ground attribute.
            COMMON_LVB_UNDERSCORE = 0x8000, // DBCS: Underscore.
            ForegroundMask = 0xF,
            BackgroundMask = 0xF0,
            ColorMask = 0xFF
        }

        [System.Security.SecurityCritical]
        internal static ConsoleControlChars ConsoleColorToColorAttribute(ConsoleColor color, bool isBackground)
        {
            if (((uint)color & 0xFFFFFFF0u) != 0)
            {
                throw new ArgumentException($"The Console Color specified , {color} , is invalid.");
            }
            ConsoleControlChars color2 = (ConsoleControlChars)color;
            if (isBackground)
            {
                color2 = (ConsoleControlChars)((System.Int32)color2 << 4);
            }
            return color2;
        }

        [System.Security.SecurityCritical]
        internal static ConsoleColor ColorAttributeToConsoleColor(System.Int16 c)
        {
            if ((c & (System.Int16)ConsoleControlChars.BackgroundMask) != 0)
            {
                c = (System.Int16)((System.Int32)c >> 4);
            }
            return (ConsoleColor)c;
        }

        /// <summary>
        /// Revert the current implementation's back to default console colors , when it is initiated.
        /// </summary>
        public static void ToDefaultColors()
        {
            InitIfNotInitOut();
            ConsoleControlChars F = ConsoleColorToColorAttribute(System.ConsoleColor.Black, true);
            ConsoleInterop.DefineNewAttributes(ConsoleInterop.OutputHandle, (System.Int16)F);
            F = ConsoleColorToColorAttribute(System.ConsoleColor.Gray, false);
            ConsoleInterop.DefineNewAttributes(ConsoleInterop.OutputHandle, (System.Int16)F);
        }
    }

    namespace CryptographicOperations
    {
        /// <summary>
        /// MD2 Hasher Implementation class. Subsetted clone of 
        /// <see href="https://github.com/mono/mono/tree/main/mcs/class/Mono.Security/Mono.Security.Cryptography">
        /// Mono.Security.Cryptography</see> namespace.
        /// </summary>
        public class MD2Managed : MD2
        {
            //
            // MD2Managed.cs - Message Digest 2 Managed Implementation
            //
            // Author:
            //	Sebastien Pouliot (sebastien@ximian.com)
            //
            // (C) 2001-2003 Motus Technologies Inc. (http://www.motus.com)
            // Copyright (C) 2004-2005,2010 Novell, Inc (http://www.novell.com)
            //
            // Permission is hereby granted, free of charge, to any person obtaining
            // a copy of this software and associated documentation files (the
            // "Software"), to deal in the Software without restriction, including
            // without limitation the rights to use, copy, modify, merge, publish,
            // distribute, sublicense, and/or sell copies of the Software, and to
            // permit persons to whom the Software is furnished to do so, subject to
            // the following conditions:
            // 
            // The above copyright notice and this permission notice shall be
            // included in all copies or substantial portions of the Software.
            // 
            // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
            // EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
            // MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
            // NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
            // LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
            // OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
            // WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
            //

            private byte[] state;
            private byte[] checksum;
            private byte[] buffer;
            private int count;
            private byte[] x;

            /// <summary>
            /// Permutation of 0..255 constructed from the digits of pi. It gives a
            /// "random" nonlinear byte substitution operation.
            /// </summary>
            private static readonly byte[] PI_SUBST = {
            41, 46, 67, 201, 162, 216, 124, 1, 61, 54, 84, 161, 236, 240, 6,
            19, 98, 167, 5, 243, 192, 199, 115, 140, 152, 147, 43, 217, 188,
            76, 130, 202, 30, 155, 87, 60, 253, 212, 224, 22, 103, 66, 111, 24,
            138, 23, 229, 18, 190, 78, 196, 214, 218, 158, 222, 73, 160, 251,
            245, 142, 187, 47, 238, 122, 169, 104, 121, 145, 21, 178, 7, 63,
            148, 194, 16, 137, 11, 34, 95, 33, 128, 127, 93, 154, 90, 144, 50,
            39, 53, 62, 204, 231, 191, 247, 151, 3, 255, 25, 48, 179, 72, 165,
            181, 209, 215, 94, 146, 42, 172, 86, 170, 198, 79, 184, 56, 210,
            150, 164, 125, 182, 118, 252, 107, 226, 156, 116, 4, 241, 69, 157,
            112, 89, 100, 113, 135, 32, 134, 91, 207, 101, 230, 45, 168, 2, 27,
            96, 37, 173, 174, 176, 185, 246, 28, 70, 97, 105, 52, 64, 126, 15,
            85, 71, 163, 35, 221, 81, 175, 58, 195, 92, 249, 206, 186, 197,
            234, 38, 44, 83, 13, 110, 133, 40, 132, 9, 211, 223, 205, 244, 65,
            129, 77, 82, 106, 220, 55, 200, 108, 193, 171, 250, 36, 225, 123,
            8, 12, 189, 177, 74, 120, 136, 149, 139, 227, 99, 232, 109, 233,
            203, 213, 254, 59, 0, 29, 57, 242, 239, 183, 14, 102, 88, 208, 228,
            166, 119, 114, 248, 235, 117, 75, 10, 49, 68, 80, 180, 143, 237,
            31, 26, 219, 153, 141, 51, 159, 17, 131, 20 };

            private byte[] Padding(int nLength)
            {
                if (nLength > 0)
                {
                    byte[] padding = new byte[nLength];
                    for (int i = 0; i < padding.Length; i++)
                        padding[i] = (byte)nLength;
                    return padding;
                }
                return null;
            }

            //--- constructor -----------------------------------------------------------

            /// <summary>
            /// Creates a new instance of <see cref="MD2Managed"/> class.
            /// </summary>
            public MD2Managed() : base()
            {
                // we allocate the context memory
                state = new byte[16];
                checksum = new byte[16];
                buffer = new byte[16];
                x = new byte[48];
                // the initialize our context
                Initialize();
            }

            /// <summary>
            /// Initialise the hasher instance. Should not be called by callers except for custom constructors.
            /// </summary>
            public override void Initialize()
            {
                count = 0;
                Array.Clear(state, 0, 16);
                Array.Clear(checksum, 0, 16);
                Array.Clear(buffer, 0, 16);
                // Zeroize sensitive information
                Array.Clear(x, 0, 48);
            }

            /// <inheritdoc />
            protected override void HashCore(byte[] array, int ibStart, int cbSize)
            {
                int i;

                /* Update number of bytes mod 16 */
                int index = count;
                count = (int)(index + cbSize) & 0xf;

                int partLen = 16 - index;

                /* Transform as many times as possible. */
                if (cbSize >= partLen)
                {
                    // MD2_memcpy((POINTER)&context->buffer[index], (POINTER)input, partLen);
                    Buffer.BlockCopy(array, ibStart, buffer, index, partLen);
                    // MD2Transform (context->state, context->checksum, context->buffer);
                    MD2Transform(state, checksum, buffer, 0);

                    for (i = partLen; i + 15 < cbSize; i += 16)
                    {
                        // MD2Transform (context->state, context->checksum, &input[i]);
                        MD2Transform(state, checksum, array, ibStart + i);
                    }

                    index = 0;
                }
                else
                    i = 0;

                /* Buffer remaining input */
                // MD2_memcpy((POINTER)&context->buffer[index], (POINTER)&input[i], inputLen-i);
                Buffer.BlockCopy(array, ibStart + i, buffer, index, (cbSize - i));
            }

            /// <inheritdoc />
            protected override byte[] HashFinal()
            {
                // Pad out to multiple of 16. 
                int index = count;
                int padLen = 16 - index;

                // is padding needed ? required if length not a multiple of 16.
                if (padLen > 0)
                    HashCore(Padding(padLen), 0, padLen);

                // Extend with checksum 
                HashCore(checksum, 0, 16);

                // Store state in digest
                byte[] digest = (byte[])state.Clone();

                // Zeroize sensitive information.
                Initialize();

                return digest;
            }

            //--- private methods ---------------------------------------------------

            /// <summary>
            /// MD2 basic transformation. Transforms state and updates checksum
            /// based on block. 
            /// </summary>
            private void MD2Transform(byte[] state, byte[] checksum, byte[] block, int index)
            {
                /* Form encryption block from state, block, state ^ block. */
                // MD2_memcpy ((POINTER)x, (POINTER)state, 16);
                Buffer.BlockCopy(state, 0, x, 0, 16);
                // MD2_memcpy ((POINTER)x+16, (POINTER)block, 16);
                Buffer.BlockCopy(block, index, x, 16, 16);

                // for (i = 0; i < 16; i++) x[i+32] = state[i] ^ block[i];
                for (int i = 0; i < 16; i++)
                    x[i + 32] = (byte)((byte)state[i] ^ (byte)block[index + i]);

                /* Encrypt block (18 rounds). */
                int t = 0;
                for (int i = 0; i < 18; i++)
                {
                    for (int j = 0; j < 48; j++)
                        t = x[j] ^= PI_SUBST[t];
                    t = (t + i) & 0xff;
                }

                /* Save new state */
                // MD2_memcpy ((POINTER)state, (POINTER)x, 16);
                Buffer.BlockCopy(x, 0, state, 0, 16);

                /* Update checksum. */
                t = checksum[15];
                for (int i = 0; i < 16; i++)
                    t = checksum[i] ^= PI_SUBST[block[index + i] ^ t];
            }
        }

        /// <summary>
        /// MD2 Base Implementation class. Subsetted clone of 
        /// <see href="https://github.com/mono/mono/tree/main/mcs/class/Mono.Security/Mono.Security.Cryptography">
        /// Mono.Security.Cryptography</see> namespace.
        /// </summary>
        public abstract class MD2 : System.Security.Cryptography.HashAlgorithm
        {
            //
            // MD2.cs - Message Digest 2 Abstract class
            //
            // Author:
            //	Sebastien Pouliot (spouliot@motus.com)
            //
            // (C) 2001-2003 Motus Technologies Inc. (http://www.motus.com)
            //

            //
            // Permission is hereby granted, free of charge, to any person obtaining
            // a copy of this software and associated documentation files (the
            // "Software"), to deal in the Software without restriction, including
            // without limitation the rights to use, copy, modify, merge, publish,
            // distribute, sublicense, and/or sell copies of the Software, and to
            // permit persons to whom the Software is furnished to do so, subject to
            // the following conditions:
            // 
            // The above copyright notice and this permission notice shall be
            // included in all copies or substantial portions of the Software.
            // 
            // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
            // EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
            // MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
            // NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
            // LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
            // OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
            // WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
            //


            /// <summary>
            /// Initialises a new instance of <see cref="MD2"/> with default-preloaded settings.
            /// </summary>
            protected MD2()
            {
                // MD2 hash length are 128 bits long
                HashSizeValue = 128;
            }

            /// <summary>
            /// Creates a new <see cref="MD2Managed"/> hasher instance.
            /// </summary>
            /// <returns>The created instance.</returns>
            public static new MD2 Create() { return new MD2Managed(); }

        }

        /// <summary>
        /// MD4 Base Implementation class. Subsetted clone of 
        /// <see href="https://github.com/mono/mono/tree/main/mcs/class/Mono.Security/Mono.Security.Cryptography">
        /// Mono.Security.Cryptography</see> namespace.
        /// </summary>
        public abstract class MD4 : System.Security.Cryptography.HashAlgorithm
        {
            //
            // MD4.cs - Message Digest 4 Abstract class
            //
            // Author:
            //	Sebastien Pouliot (sebastien@xamarin.com)
            //
            // (C) 2003 Motus Technologies Inc. (http://www.motus.com)
            // Copyright 2013 Xamarin Inc. (http://www.xamarin.com)
            //

            //
            // Permission is hereby granted, free of charge, to any person obtaining
            // a copy of this software and associated documentation files (the
            // "Software"), to deal in the Software without restriction, including
            // without limitation the rights to use, copy, modify, merge, publish,
            // distribute, sublicense, and/or sell copies of the Software, and to
            // permit persons to whom the Software is furnished to do so, subject to
            // the following conditions:
            // 
            // The above copyright notice and this permission notice shall be
            // included in all copies or substantial portions of the Software.
            // 
            // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
            // EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
            // MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
            // NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
            // LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
            // OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
            // WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
            //

            /// <summary>
            /// Create a new instance of <see cref="MD4"/> class.
            /// </summary>
            protected MD4()
            {
                // MD4 hash length are 128 bits long
                HashSizeValue = 128;
            }

            /// <summary>
            /// Creates out the real MD4 Hasher.
            /// </summary>
            /// <returns>The created MD4 Hasher.</returns>
            public static new MD4 Create() { return new MD4Managed (); }
        }

        /// <summary>
        /// MD4 Hasher Implementation class. Subsetted clone of 
        /// <see href="https://github.com/mono/mono/tree/main/mcs/class/Mono.Security/Mono.Security.Cryptography">
        /// Mono.Security.Cryptography</see> namespace.
        /// </summary>
        public class MD4Managed : MD4
        {
            //
            // MD4Managed.cs - Message Digest 4 Managed Implementation
            //
            // Author:
            //	Sebastien Pouliot (sebastien@ximian.com)
            //
            // (C) 2003 Motus Technologies Inc. (http://www.motus.com)
            // Copyright (C) 2004-2005,2010 Novell, Inc (http://www.novell.com)
            //
            // Permission is hereby granted, free of charge, to any person obtaining
            // a copy of this software and associated documentation files (the
            // "Software"), to deal in the Software without restriction, including
            // without limitation the rights to use, copy, modify, merge, publish,
            // distribute, sublicense, and/or sell copies of the Software, and to
            // permit persons to whom the Software is furnished to do so, subject to
            // the following conditions:
            // 
            // The above copyright notice and this permission notice shall be
            // included in all copies or substantial portions of the Software.
            // 
            // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
            // EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
            // MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
            // NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
            // LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
            // OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
            // WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
            //


            private uint[] state;
            private byte[] buffer;
            private uint[] count;
            private uint[] x;

            private const int S11 = 3;
            private const int S12 = 7;
            private const int S13 = 11;
            private const int S14 = 19;
            private const int S21 = 3;
            private const int S22 = 5;
            private const int S23 = 9;
            private const int S24 = 13;
            private const int S31 = 3;
            private const int S32 = 9;
            private const int S33 = 11;
            private const int S34 = 15;

            private byte[] digest;

            //--- constructor -----------------------------------------------------------

            /// <summary>
            /// Initialises a new instance of <see cref="MD4Managed"/> class.
            /// </summary>
            public MD4Managed()
            {
                // we allocate the context memory
                state = new uint[4];
                count = new uint[2];
                buffer = new byte[64];
                digest = new byte[16];
                // temporary buffer in MD4Transform that we don't want to keep allocate on each iteration
                x = new uint[16];
                // the initialize our context
                Initialize();
            }

            /// <summary>
            /// Initialises an implementation of <see cref="MD4Managed"/> class.
            /// </summary>
            public override void Initialize()
            {
                count[0] = 0;
                count[1] = 0;
                state[0] = 0x67452301;
                state[1] = 0xefcdab89;
                state[2] = 0x98badcfe;
                state[3] = 0x10325476;
                // Zeroize sensitive information
                Array.Clear(buffer, 0, 64);
                Array.Clear(x, 0, 16);
            }

            /// <inheritdoc />
            protected override void HashCore(byte[] array, int ibStart, int cbSize)
            {
                /* Compute number of bytes mod 64 */
                int index = (int)((count[0] >> 3) & 0x3F);
                /* Update number of bits */
                count[0] += (uint)(cbSize << 3);
                if (count[0] < (cbSize << 3))
                    count[1]++;
                count[1] += (uint)(cbSize >> 29);

                int partLen = 64 - index;
                int i = 0;
                /* Transform as many times as possible. */
                if (cbSize >= partLen)
                {
                    //MD4_memcpy((POINTER)&context->buffer[index], (POINTER)input, partLen);
                    Buffer.BlockCopy(array, ibStart, buffer, index, partLen);
                    MD4Transform(state, buffer, 0);

                    for (i = partLen; i + 63 < cbSize; i += 64)
                    {
                        // MD4Transform (context->state, &input[i]);
                        MD4Transform(state, array, ibStart + i);
                    }

                    index = 0;
                }

                /* Buffer remaining input */
                //MD4_memcpy ((POINTER)&context->buffer[index], (POINTER)&input[i], inputLen-i);
                Buffer.BlockCopy(array, ibStart + i, buffer, index, (cbSize - i));
            }

            /// <inheritdoc />
            protected override byte[] HashFinal()
            {
                /* Save number of bits */
                byte[] bits = new byte[8];
                Encode(bits, count);

                /* Pad out to 56 mod 64. */
                uint index = ((count[0] >> 3) & 0x3f);
                int padLen = (int)((index < 56) ? (56 - index) : (120 - index));
                HashCore(Padding(padLen), 0, padLen);

                /* Append length (before padding) */
                HashCore(bits, 0, 8);

                /* Store state in digest */
                Encode(digest, state);

                // Zeroize sensitive information.
                Initialize();

                return digest;
            }

            //--- private methods ---------------------------------------------------

            private byte[] Padding(int nLength)
            {
                if (nLength > 0)
                {
                    byte[] padding = new byte[nLength];
                    padding[0] = 0x80;
                    return padding;
                }
                return null;
            }

            /* F, G and H are basic MD4 functions. */
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private uint F(uint x, uint y, uint z)
            {
                return (uint)(((x) & (y)) | ((~x) & (z)));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private uint G(uint x, uint y, uint z)
            {
                return (uint)(((x) & (y)) | ((x) & (z)) | ((y) & (z)));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private uint H(uint x, uint y, uint z)
            {
                return (uint)((x) ^ (y) ^ (z));
            }

            /* ROTATE_LEFT rotates x left n bits. */
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private uint ROL(uint x, byte n)
            {
                return (uint)(((x) << (n)) | ((x) >> (32 - (n))));
            }

            /* FF, GG and HH are transformations for rounds 1, 2 and 3 */
            /* Rotation is separate from addition to prevent recomputation */
            private void FF(ref uint a, uint b, uint c, uint d, uint x, byte s)
            {
                a += F(b, c, d) + x;
                a = ROL(a, s);
            }

            private void GG(ref uint a, uint b, uint c, uint d, uint x, byte s)
            {
                a += G(b, c, d) + x + 0x5a827999;
                a = ROL(a, s);
            }

            private void HH(ref uint a, uint b, uint c, uint d, uint x, byte s)
            {
                a += H(b, c, d) + x + 0x6ed9eba1;
                a = ROL(a, s);
            }

            private void Encode(byte[] output, uint[] input)
            {
                for (int i = 0, j = 0; j < output.Length; i++, j += 4)
                {
                    output[j] = (byte)(input[i]);
                    output[j + 1] = (byte)(input[i] >> 8);
                    output[j + 2] = (byte)(input[i] >> 16);
                    output[j + 3] = (byte)(input[i] >> 24);
                }
            }

            private void Decode(uint[] output, byte[] input, int index)
            {
                for (int i = 0, j = index; i < output.Length; i++, j += 4)
                {
                    output[i] = (uint)((input[j]) | (input[j + 1] << 8) | (input[j + 2] << 16) | (input[j + 3] << 24));
                }
            }

            private void MD4Transform(uint[] state, byte[] block, int index)
            {
                uint a = state[0];
                uint b = state[1];
                uint c = state[2];
                uint d = state[3];

                Decode(x, block, index);

                /* Round 1 */
                FF(ref a, b, c, d, x[0], S11); /* 1 */
                FF(ref d, a, b, c, x[1], S12); /* 2 */
                FF(ref c, d, a, b, x[2], S13); /* 3 */
                FF(ref b, c, d, a, x[3], S14); /* 4 */
                FF(ref a, b, c, d, x[4], S11); /* 5 */
                FF(ref d, a, b, c, x[5], S12); /* 6 */
                FF(ref c, d, a, b, x[6], S13); /* 7 */
                FF(ref b, c, d, a, x[7], S14); /* 8 */
                FF(ref a, b, c, d, x[8], S11); /* 9 */
                FF(ref d, a, b, c, x[9], S12); /* 10 */
                FF(ref c, d, a, b, x[10], S13); /* 11 */
                FF(ref b, c, d, a, x[11], S14); /* 12 */
                FF(ref a, b, c, d, x[12], S11); /* 13 */
                FF(ref d, a, b, c, x[13], S12); /* 14 */
                FF(ref c, d, a, b, x[14], S13); /* 15 */
                FF(ref b, c, d, a, x[15], S14); /* 16 */

                /* Round 2 */
                GG(ref a, b, c, d, x[0], S21); /* 17 */
                GG(ref d, a, b, c, x[4], S22); /* 18 */
                GG(ref c, d, a, b, x[8], S23); /* 19 */
                GG(ref b, c, d, a, x[12], S24); /* 20 */
                GG(ref a, b, c, d, x[1], S21); /* 21 */
                GG(ref d, a, b, c, x[5], S22); /* 22 */
                GG(ref c, d, a, b, x[9], S23); /* 23 */
                GG(ref b, c, d, a, x[13], S24); /* 24 */
                GG(ref a, b, c, d, x[2], S21); /* 25 */
                GG(ref d, a, b, c, x[6], S22); /* 26 */
                GG(ref c, d, a, b, x[10], S23); /* 27 */
                GG(ref b, c, d, a, x[14], S24); /* 28 */
                GG(ref a, b, c, d, x[3], S21); /* 29 */
                GG(ref d, a, b, c, x[7], S22); /* 30 */
                GG(ref c, d, a, b, x[11], S23); /* 31 */
                GG(ref b, c, d, a, x[15], S24); /* 32 */

                HH(ref a, b, c, d, x[0], S31); /* 33 */
                HH(ref d, a, b, c, x[8], S32); /* 34 */
                HH(ref c, d, a, b, x[4], S33); /* 35 */
                HH(ref b, c, d, a, x[12], S34); /* 36 */
                HH(ref a, b, c, d, x[2], S31); /* 37 */
                HH(ref d, a, b, c, x[10], S32); /* 38 */
                HH(ref c, d, a, b, x[6], S33); /* 39 */
                HH(ref b, c, d, a, x[14], S34); /* 40 */
                HH(ref a, b, c, d, x[1], S31); /* 41 */
                HH(ref d, a, b, c, x[9], S32); /* 42 */
                HH(ref c, d, a, b, x[5], S33); /* 43 */
                HH(ref b, c, d, a, x[13], S34); /* 44 */
                HH(ref a, b, c, d, x[3], S31); /* 45 */
                HH(ref d, a, b, c, x[11], S32); /* 46 */
                HH(ref c, d, a, b, x[7], S33); /* 47 */
                HH(ref b, c, d, a, x[15], S34); /* 48 */

                state[0] += a;
                state[1] += b;
                state[2] += c;
                state[3] += d;
            }
        }

        internal readonly struct SHAConstants
        {
            // SHA-224/256 Constants
            // Represent the first 32 bits of the fractional parts of the
            // cube roots of the first sixty-four prime numbers
            public readonly static uint[] K1 = {
                0x428A2F98, 0x71374491, 0xB5C0FBCF, 0xE9B5DBA5, 0x3956C25B, 0x59F111F1, 0x923F82A4, 0xAB1C5ED5,
                0xD807AA98, 0x12835B01, 0x243185BE, 0x550C7DC3, 0x72BE5D74, 0x80DEB1FE, 0x9BDC06A7, 0xC19BF174,
                0xE49B69C1, 0xEFBE4786, 0x0FC19DC6, 0x240CA1CC, 0x2DE92C6F, 0x4A7484AA, 0x5CB0A9DC, 0x76F988DA,
                0x983E5152, 0xA831C66D, 0xB00327C8, 0xBF597FC7, 0xC6E00BF3, 0xD5A79147, 0x06CA6351, 0x14292967,
                0x27B70A85, 0x2E1B2138, 0x4D2C6DFC, 0x53380D13, 0x650A7354, 0x766A0ABB, 0x81C2C92E, 0x92722C85,
                0xA2BFE8A1, 0xA81A664B, 0xC24B8B70, 0xC76C51A3, 0xD192E819, 0xD6990624, 0xF40E3585, 0x106AA070,
                0x19A4C116, 0x1E376C08, 0x2748774C, 0x34B0BCB5, 0x391C0CB3, 0x4ED8AA4A, 0x5B9CCA4F, 0x682E6FF3,
                0x748F82EE, 0x78A5636F, 0x84C87814, 0x8CC70208, 0x90BEFFFA, 0xA4506CEB, 0xBEF9A3F7, 0xC67178F2
            };
        }

        /// <summary>
        /// SHA224 Base Implementation class. Subsetted clone of 
        /// <see href="https://github.com/mono/mono/tree/main/mcs/class/Mono.Security/Mono.Security.Cryptography">
        /// Mono.Security.Cryptography</see> namespace.
        /// </summary>
        public abstract class SHA224 : System.Security.Cryptography.HashAlgorithm
        {
            //
            // Mono.Security.Cryptography SHA224 Class implementation
            //
            // Authors:
            //	Sebastien Pouliot <sebastien@ximian.com>
            //
            // Copyright (C) 2004 Novell, Inc (http://www.novell.com)
            //
            // Permission is hereby granted, free of charge, to any person obtaining
            // a copy of this software and associated documentation files (the
            // "Software"), to deal in the Software without restriction, including
            // without limitation the rights to use, copy, modify, merge, publish,
            // distribute, sublicense, and/or sell copies of the Software, and to
            // permit persons to whom the Software is furnished to do so, subject to
            // the following conditions:
            // 
            // The above copyright notice and this permission notice shall be
            // included in all copies or substantial portions of the Software.
            // 
            // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
            // EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
            // MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
            // NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
            // LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
            // OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
            // WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
            //

            /// <summary>
            /// Create a new instance of <see cref="SHA224"/> class.
            /// </summary>
            public SHA224()
            {
                // SHA-224 hash length are 224 bits long
                HashSizeValue = 224;
            }

            /// <summary>
            /// Creates out the real SHA224 Hasher.
            /// </summary>
            /// <returns>The created SHA224 Hasher.</returns>
            public static new SHA224 Create() { return new SHA224Managed(); }
        }

        /// <summary>
        /// SHA224 Hasher Implementation class. Subsetted clone of 
        /// <see href="https://github.com/mono/mono/tree/main/mcs/class/Mono.Security/Mono.Security.Cryptography">
        /// Mono.Security.Cryptography</see> namespace.
        /// </summary>
        public class SHA224Managed : SHA224
        {
            //
            // Mono.Security.Cryptography SHA224 class implementation
            //	based on SHA256Managed class implementation (mscorlib.dll)
            //
            // Authors:
            //	Matthew S. Ford (Matthew.S.Ford@Rose-Hulman.Edu)
            //	Sebastien Pouliot <sebastien@ximian.com>
            //
            // (C) 2001 
            // Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
            //
            // Permission is hereby granted, free of charge, to any person obtaining
            // a copy of this software and associated documentation files (the
            // "Software"), to deal in the Software without restriction, including
            // without limitation the rights to use, copy, modify, merge, publish,
            // distribute, sublicense, and/or sell copies of the Software, and to
            // permit persons to whom the Software is furnished to do so, subject to
            // the following conditions:
            // 
            // The above copyright notice and this permission notice shall be
            // included in all copies or substantial portions of the Software.
            // 
            // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
            // EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
            // MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
            // NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
            // LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
            // OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
            // WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
            //

            private const int BLOCK_SIZE_BYTES = 64;

            private uint[] _H;
            private ulong count;
            private byte[] _ProcessingBuffer;   // Used to start data when passed less than a block worth.
            private int _ProcessingBufferCount; // Counts how much data we have stored that still needs processed.
            private uint[] buff;

            /// <summary>
            /// Initialises a new instance of <see cref="SHA224Managed"/> class.
            /// </summary>
            public SHA224Managed()
            {
                _H = new uint[8];
                _ProcessingBuffer = new byte[BLOCK_SIZE_BYTES];
                buff = new uint[64];
                Initialize();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private uint Ch(uint u, uint v, uint w)
            {
                return (u & v) ^ (~u & w);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private uint Maj(uint u, uint v, uint w)
            {
                return (u & v) ^ (u & w) ^ (v & w);
            }

            private uint Ro0(uint x)
            {
                return ((x >> 7) | (x << 25))
                    ^ ((x >> 18) | (x << 14))
                    ^ (x >> 3);
            }

            private uint Ro1(uint x)
            {
                return ((x >> 17) | (x << 15))
                    ^ ((x >> 19) | (x << 13))
                    ^ (x >> 10);
            }

            private uint Sig0(uint x)
            {
                return ((x >> 2) | (x << 30))
                    ^ ((x >> 13) | (x << 19))
                    ^ ((x >> 22) | (x << 10));
            }

            private uint Sig1(uint x)
            {
                return ((x >> 6) | (x << 26))
                    ^ ((x >> 11) | (x << 21))
                    ^ ((x >> 25) | (x << 7));
            }

            /// <inheritdoc />
            protected override void HashCore(byte[] rgb, int start, int size)
            {
                int i;
                State = 1;

                if (_ProcessingBufferCount != 0)
                {
                    if (size < (BLOCK_SIZE_BYTES - _ProcessingBufferCount))
                    {
                        System.Buffer.BlockCopy(rgb, start, _ProcessingBuffer, _ProcessingBufferCount, size);
                        _ProcessingBufferCount += size;
                        return;
                    }
                    else
                    {
                        i = (BLOCK_SIZE_BYTES - _ProcessingBufferCount);
                        System.Buffer.BlockCopy(rgb, start, _ProcessingBuffer, _ProcessingBufferCount, i);
                        ProcessBlock(_ProcessingBuffer, 0);
                        _ProcessingBufferCount = 0;
                        start += i;
                        size -= i;
                    }
                }

                for (i = 0; i < size - size % BLOCK_SIZE_BYTES; i += BLOCK_SIZE_BYTES)
                {
                    ProcessBlock(rgb, start + i);
                }

                if (size % BLOCK_SIZE_BYTES != 0)
                {
                    System.Buffer.BlockCopy(rgb, size - size % BLOCK_SIZE_BYTES + start, _ProcessingBuffer, 0, size % BLOCK_SIZE_BYTES);
                    _ProcessingBufferCount = size % BLOCK_SIZE_BYTES;
                }
            }

            /// <inheritdoc />
            protected override byte[] HashFinal()
            {
                byte[] hash = new byte[28];
                int i, j;

                ProcessFinalBlock(_ProcessingBuffer, 0, _ProcessingBufferCount);

                for (i = 0; i < 7; i++)
                {
                    for (j = 0; j < 4; j++)
                    {
                        hash[i * 4 + j] = (byte)(_H[i] >> (24 - j * 8));
                    }
                }

                State = 0;
                return hash;
            }

            /// <inheritdoc cref="SHA224Managed.SHA224Managed"/>
            public override void Initialize()
            {
                count = 0;
                _ProcessingBufferCount = 0;

                _H[0] = 0xC1059ED8;
                _H[1] = 0x367CD507;
                _H[2] = 0x3070DD17;
                _H[3] = 0xF70E5939;
                _H[4] = 0xFFC00B31;
                _H[5] = 0x68581511;
                _H[6] = 0x64F98FA7;
                _H[7] = 0xBEFA4FA4;
            }

            private void ProcessBlock(byte[] inputBuffer, int inputOffset)
            {
                uint a, b, c, d, e, f, g, h;
                uint t1, t2;
                int i;
                uint[] K1 = SHAConstants.K1;
                uint[] buff = this.buff;

                count += BLOCK_SIZE_BYTES;

                for (i = 0; i < 16; i++)
                {
                    buff[i] = (uint)(((inputBuffer[inputOffset + 4 * i]) << 24)
                        | ((inputBuffer[inputOffset + 4 * i + 1]) << 16)
                        | ((inputBuffer[inputOffset + 4 * i + 2]) << 8)
                        | ((inputBuffer[inputOffset + 4 * i + 3])));
                }


                for (i = 16; i < 64; i++)
                {
                    t1 = buff[i - 15];
                    t1 = (((t1 >> 7) | (t1 << 25)) ^ ((t1 >> 18) | (t1 << 14)) ^ (t1 >> 3));

                    t2 = buff[i - 2];
                    t2 = (((t2 >> 17) | (t2 << 15)) ^ ((t2 >> 19) | (t2 << 13)) ^ (t2 >> 10));
                    buff[i] = t2 + buff[i - 7] + t1 + buff[i - 16];
                }

                a = _H[0];
                b = _H[1];
                c = _H[2];
                d = _H[3];
                e = _H[4];
                f = _H[5];
                g = _H[6];
                h = _H[7];

                for (i = 0; i < 64; i++)
                {
                    t1 = h + (((e >> 6) | (e << 26)) ^ ((e >> 11) | (e << 21)) ^ ((e >> 25) | (e << 7))) + ((e & f) ^ (~e & g)) + K1[i] + buff[i];

                    t2 = (((a >> 2) | (a << 30)) ^ ((a >> 13) | (a << 19)) ^ ((a >> 22) | (a << 10)));
                    t2 = t2 + ((a & b) ^ (a & c) ^ (b & c));
                    h = g;
                    g = f;
                    f = e;
                    e = d + t1;
                    d = c;
                    c = b;
                    b = a;
                    a = t1 + t2;
                }

                _H[0] += a;
                _H[1] += b;
                _H[2] += c;
                _H[3] += d;
                _H[4] += e;
                _H[5] += f;
                _H[6] += g;
                _H[7] += h;
            }

            private void ProcessFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
            {
                ulong total = count + (ulong)inputCount;
                int paddingSize = (56 - (int)(total % BLOCK_SIZE_BYTES));

                if (paddingSize < 1)
                    paddingSize += BLOCK_SIZE_BYTES;

                byte[] fooBuffer = new byte[inputCount + paddingSize + 8];

                for (int i = 0; i < inputCount; i++)
                {
                    fooBuffer[i] = inputBuffer[i + inputOffset];
                }

                fooBuffer[inputCount] = 0x80;
                for (int i = inputCount + 1; i < inputCount + paddingSize; i++)
                {
                    fooBuffer[i] = 0x00;
                }

                // I deal in bytes. The algorithm deals in bits.
                ulong size = total << 3;
                AddLength(size, fooBuffer, inputCount + paddingSize);
                ProcessBlock(fooBuffer, 0);

                if (inputCount + paddingSize + 8 == 128)
                {
                    ProcessBlock(fooBuffer, 64);
                }
            }

            internal void AddLength(ulong length, byte[] buffer, int position)
            {
                buffer[position++] = (byte)(length >> 56);
                buffer[position++] = (byte)(length >> 48);
                buffer[position++] = (byte)(length >> 40);
                buffer[position++] = (byte)(length >> 32);
                buffer[position++] = (byte)(length >> 24);
                buffer[position++] = (byte)(length >> 16);
                buffer[position++] = (byte)(length >> 8);
                buffer[position] = (byte)(length);
            }
        }

        /// <summary>
        /// RC4 Base Implementation class. Subsetted clone of 
        /// <see href="https://github.com/mono/mono/tree/main/mcs/class/Mono.Security/Mono.Security.Cryptography">
        /// Mono.Security.Cryptography</see> namespace.
        /// </summary>
        public abstract class RC4 : System.Security.Cryptography.SymmetricAlgorithm
        {
            //
            // RC4.cs: RC4(tm) symmetric stream cipher
            //	RC4 is a trademark of RSA Security
            //
            // Author:
            //	Sebastien Pouliot (sebastien@xamarin.com)
            //
            // (C) 2003 Motus Technologies Inc. (http://www.motus.com)
            // Copyright 2013 Xamarin Inc. (http://www.xamarin.com)
            //
            // Permission is hereby granted, free of charge, to any person obtaining
            // a copy of this software and associated documentation files (the
            // "Software"), to deal in the Software without restriction, including
            // without limitation the rights to use, copy, modify, merge, publish,
            // distribute, sublicense, and/or sell copies of the Software, and to
            // permit persons to whom the Software is furnished to do so, subject to
            // the following conditions:
            // 
            // The above copyright notice and this permission notice shall be
            // included in all copies or substantial portions of the Software.
            // 
            // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
            // EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
            // MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
            // NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
            // LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
            // OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
            // WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
            //

            private static System.Security.Cryptography.KeySizes[] s_legalBlockSizes = { new System.Security.Cryptography.KeySizes(64, 64, 0) };

            private static System.Security.Cryptography.KeySizes[] s_legalKeySizes = { new System.Security.Cryptography.KeySizes (40, 2048, 8) };

            /// <summary>
            /// Create a new instance of <see cref="RC4"/> class.
            /// </summary>
            public RC4()
            {
                KeySizeValue = 128;
                BlockSizeValue = 64;
                FeedbackSizeValue = BlockSizeValue;
                LegalBlockSizesValue = s_legalBlockSizes;
                LegalKeySizesValue = s_legalKeySizes;
            }

            /// <summary>
            /// This property is not used for this symmetric algorithm because it does not need 
            /// an Initialisation Vector.
            /// </summary>
            /// <remarks>Attempting to get it returns an empty <see cref="System.Byte"/>[] array and
            /// attempting to set it throws an <see cref="InvalidOperationException"/> exception.</remarks>
            // required for compatibility with .NET 2.0
            public override byte[] IV
            {
                get { return new byte[0]; }
                set { throw new InvalidOperationException("This property should not be called at any case."); }
            }

            /// <summary>
            /// Creates out the real RC4 Encryptor/Decryptor.
            /// </summary>
            /// <returns>The created RC4 Encryptor/Decryptor.</returns>
            new static public RC4 Create() { return new ARC4Managed (); }
        }

        /// <summary>
        /// Imaginary RC4 Stream Cipher class. Subsetted clone of 
        /// <see href="https://github.com/mono/mono/tree/main/mcs/class/Mono.Security/Mono.Security.Cryptography">
        /// Mono.Security.Cryptography</see> namespace.
        /// </summary>
        public class ARC4Managed : RC4, System.Security.Cryptography.ICryptoTransform
        {
            //
            // ARC4Managed.cs: Alleged RC4(tm) compatible symmetric stream cipher
            //	RC4 is a trademark of RSA Security
            //
            // Permission is hereby granted, free of charge, to any person obtaining
            // a copy of this software and associated documentation files (the
            // "Software"), to deal in the Software without restriction, including
            // without limitation the rights to use, copy, modify, merge, publish,
            // distribute, sublicense, and/or sell copies of the Software, and to
            // permit persons to whom the Software is furnished to do so, subject to
            // the following conditions:
            // 
            // The above copyright notice and this permission notice shall be
            // included in all copies or substantial portions of the Software.
            // 
            // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
            // EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
            // MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
            // NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
            // LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
            // OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
            // WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
            //


            private byte[] key;
            private byte[] state;
            private byte x;
            private byte y;
            private bool m_disposed;

            /// <summary>
            /// Creates a new instance of the <see cref="ARC4Managed"/> class.
            /// </summary>
            public ARC4Managed() : base() { state = new byte[256]; m_disposed = false; }

            /// <summary>
            /// This deconstuctor explicitly calls <see cref="Dispose(bool)"/> with the parameter value set to <see langword="true"/>.
            /// </summary>
            ~ARC4Managed() { Dispose(true); }

            /// <summary>
            /// Dispose the <see cref="ARC4Managed"/> class , with the option to release all the managed resources or not. 
            /// </summary>
            /// <param name="disposing">Determines whether to dispose all the managed resources.</param>
            protected override void Dispose(bool disposing)
            {
                if (!m_disposed)
                {
                    x = 0;
                    y = 0;
                    if (key != null)
                    {
                        Array.Clear(key, 0, key.Length);
                        key = null;
                    }
                    Array.Clear(state, 0, state.Length);
                    state = null;
                    GC.SuppressFinalize(this);
                    m_disposed = true;
                }
            }

            /// <summary>
            /// Gets or sets the current key for this instance.
            /// </summary>
            public override byte[] Key
            {
                get
                {
                    if (KeyValue == null) { GenerateKey(); }
                    return (byte[]) KeyValue.Clone();
                }
                set
                {
                    if (value == null) { throw new ArgumentNullException("Key"); }
                    KeyValue = key = (byte[]) value.Clone();
                    KeySetup(key);
                }
            }

            /// <inheritdoc />
            public bool CanReuseTransform { get { return false; } }

            /// <summary>
            /// Creates a symmetric encryptor of the <see cref="ARC4Managed"/> class with
            /// the specified <see cref="Key"/> property.
            /// </summary>
            /// <param name="rgbKey">The secret key to use for the symmetric algorithm.</param>
            /// <param name="rgvIV">This value should be ignored.</param>
            /// <returns>A symmetric encryptor of the <see cref="ARC4Managed"/> class.</returns>
            /// <remarks>The <paramref name="rgvIV"/> value is not used since <see cref="ARC4Managed"/> 
            /// does not make use of any Initialisation Vector.</remarks>

            public override System.Security.Cryptography.ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[] rgvIV)
            {
                Key = rgbKey;
                return this;
            }

            /// <summary>
            /// Creates a symmetric decryptor of the <see cref="ARC4Managed"/> class with
            /// the specified <see cref="Key"/> property.
            /// </summary>
            /// <param name="rgbKey">The secret key to use for the symmetric algorithm.</param>
            /// <param name="rgvIV">This value should be ignored.</param>
            /// <returns>A symmetric decryptor of the <see cref="ARC4Managed"/> class.</returns>
            /// <remarks>The <paramref name="rgvIV"/> value is not used since <see cref="ARC4Managed"/> 
            /// does not make use of any Initialisation Vector.</remarks>
            public override System.Security.Cryptography.ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[] rgvIV)
            {
                Key = rgbKey;
                return CreateEncryptor();
            }

            /// <summary>
            /// This function is not used and will always throw an <see cref="InvalidOperationException"/> exception.
            /// </summary>
            public override void GenerateIV()
            {
                // not used for a stream cipher
                IV = new byte[0];
                throw new InvalidOperationException("This method is not used.");
            }

            /// <inheritdoc />
            public override void GenerateKey() { System.Security.Cryptography.RNGCryptoServiceProvider.Create().GetNonZeroBytes(KeyValue); }

            /// <inheritdoc />
            public bool CanTransformMultipleBlocks { get { return true; } }

            /// <inheritdoc />
            public int InputBlockSize { get { return 1; } }

            /// <inheritdoc />
            public int OutputBlockSize { get { return 1; } }

            private void KeySetup(byte[] key)
            {
                byte index1 = 0;
                byte index2 = 0;

                for (int counter = 0; counter < 256; counter++)
                    state[counter] = (byte)counter;
                x = 0;
                y = 0;
                for (int counter = 0; counter < 256; counter++)
                {
                    index2 = (byte)(key[index1] + state[counter] + index2);
                    // swap byte
                    byte tmp = state[counter];
                    state[counter] = state[index2];
                    state[index2] = tmp;
                    index1 = (byte)((index1 + 1) % key.Length);
                }
            }

            private void CheckInput(byte[] inputBuffer, int inputOffset, int inputCount)
            {
                if (inputBuffer == null)
                    throw new ArgumentNullException("inputBuffer");
                if (inputOffset < 0)
                    throw new ArgumentOutOfRangeException("inputOffset", "< 0");
                if (inputCount < 0)
                    throw new ArgumentOutOfRangeException("inputCount", "< 0");
                // ordered to avoid possible integer overflow
                if (inputOffset > inputBuffer.Length - inputCount)
                    throw new ArgumentException("An overflow in the arguments occured.", "inputBuffer");
            }

            /// <inheritdoc />
            public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
            {
                CheckInput(inputBuffer, inputOffset, inputCount);
                // check output parameters
                if (outputBuffer == null)
                    throw new ArgumentNullException("outputBuffer");
                if (outputOffset < 0)
                    throw new ArgumentOutOfRangeException("outputOffset", "< 0");
                // ordered to avoid possible integer overflow
                if (outputOffset > outputBuffer.Length - inputCount)
                    throw new ArgumentException("An overflow in the arguments occured.", "outputBuffer");

                return InternalTransformBlock(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
            }

            private int InternalTransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
            {
                byte xorIndex;
                for (int counter = 0; counter < inputCount; counter++)
                {
                    x = (byte)(x + 1);
                    y = (byte)(state[x] + y);
                    // swap byte
                    byte tmp = state[x];
                    state[x] = state[y];
                    state[y] = tmp;

                    xorIndex = (byte)(state[x] + state[y]);
                    outputBuffer[outputOffset + counter] = (byte)(inputBuffer[inputOffset + counter] ^ state[xorIndex]);
                }
                return inputCount;
            }

            /// <inheritdoc />
            public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
            {
                CheckInput(inputBuffer, inputOffset, inputCount);

                byte[] output = new byte[inputCount];
                InternalTransformBlock(inputBuffer, inputOffset, inputCount, output, 0);
                return output;
            }
        }
    
        
    }

    internal static class Debugger
    {
        // Internal debugger tag that helps in debugging the library's functions.
        internal const System.String DBGINFOShow = "[MDCFRDBGINFO]";
        private static System.Boolean Executed = false;

        // This debugger constant will specify when to use the debugging services.
#if DEBUG
        private const System.Boolean UseDebugger = true;
#else
        private const System.Boolean UseDebugger = false;
#endif

        private static void EnsureConsoleOpen()
        {
            if (Executed == false)
            {
                if (ROOT.MAIN.CreateConsole() == false) { ConsoleExtensions.Detached = false; }
                if (ConsoleExtensions.Detached) { ROOT.MAIN.CreateConsole(); }
                Executed = true;
            }
        }

#if DEBUG == false
        #pragma warning disable CS0162
#endif
        internal static void DebuggingInfo(System.String Info)
        {
            if (UseDebugger) 
            {
                EnsureConsoleOpen();
                ROOT.MAIN.WriteConsoleText(DBGINFOShow + " " + Info);
            }
        }
#if DEBUG == false
        #pragma warning restore CS0162
#endif
    }

}

namespace ExternalHashCaculators
{
    //A Collection Namespace for computing hash values from external generated libraries.

    /// <summary>
    /// <para> xxHash is a really fast non-cryptographic hash digest. </para> 
	/// <para>
	/// This is a wrapper for the unmanaged library.
    /// Note that you can run this only on AMD64 machines only. </para>
    /// </summary>
    [SupportedOSPlatform("windows")]
    public static class XXHash
    {
        // xxHash Hash caculator system.
        // It is a fast , non-cryptographic algorithm , as described from Cyan4973.
        // It is also used by the zstd archiving protocol , so as to check and the file integrity.
        // The version imported here is 0.8.1.

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        private static System.Boolean _CheckDLLVer()
        {
            if (ROOT.MAIN.OSProcessorArchitecture() != "AMD64") { return false; }
            if (XXHASHMETHODS.XXH_versionNumber() < 00801) { return false; } else { return true; }
        }

        [System.Security.SuppressUnmanagedCodeSecurity]
        private sealed class XXHASHMETHODS
        {
            [System.Runtime.InteropServices.DllImport(Interop.Libraries.XXHash)]
            [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
            public static extern System.Int32 XXH32(System.Byte[] buffer,
            System.Int32 size, System.Int32 seed = 0);

            [System.Runtime.InteropServices.DllImport(Interop.Libraries.XXHash)]
            [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
            public static extern System.Int32 XXH64(System.Byte[] buffer,
            System.Int32 size, System.Int32 seed = 0);

            [System.Runtime.InteropServices.DllImport(Interop.Libraries.XXHash)]
            [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
            public static extern System.Int32 XXH_versionNumber();
        }

        /// <summary>
        /// Computes a file hash by using the XXH32 function.
        /// </summary>
        /// <param name="FileStream">The alive <see cref="System.IO.Stream"/> object from which the data will be collected.</param>
        /// <returns>A caculated xxHash32 value written as an hexadecimal <see cref="System.String"/>.</returns>
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static System.String xxHash_32(System.IO.Stream FileStream)
        {
            if (!(_CheckDLLVer())) { return "Error"; }
            if (FileStream.Length < 20) { return "Error"; }
            System.Byte[] FBM = new System.Byte[FileStream.Length];
            try
            {
                FileStream.Read(FBM, 0, FBM.Length);
            }
            catch (System.Exception)
            {
                FBM = null;
                return "Error";
            }
            System.Int32 EFR = XXHASHMETHODS.XXH32(FBM, FBM.Length, 0);
            FBM = null;
            return EFR.ToString("x2");
        }

        /// <summary>
        /// Computes a <see cref="System.Byte"/>[] array by using the XXH32 function.
        /// </summary>
        /// <param name="Data">The <see cref="System.Byte"/>[] array to get the hash from.</param>
        /// <param name="Length">The length of the <paramref name="Data"/> 
        /// <see cref="System.Byte"/>[] .</param>
        /// <param name="Seed">The Seed to use for calculating the hash. Can be 0.</param>
        /// <returns>A caculated xxHash32 value written as an hexadecimal <see cref="System.String"/>.</returns>
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static System.String xxHash_32(System.Byte[] Data, System.Int32 Length, System.Int32 Seed)
        {
            if (!(_CheckDLLVer())) { return "Error"; }
            System.Int32 EFR = XXHASHMETHODS.XXH32(Data, Length, Seed);
            return EFR.ToString("x2");
        }

        /// <summary>
        /// Computes a <see cref="System.Byte"/>[] array by using the XXH64 function.
        /// </summary>
        /// <param name="Data">The <see cref="System.Byte"/>[] array to get the hash from.</param>
        /// <param name="Length">The length of the <paramref name="Data"/> 
        /// <see cref="System.Byte"/>[] .</param>
        /// <param name="Seed">The Seed to use for calculating the hash. Can be 0.</param>
        /// <returns>A caculated xxHash64 value written as an hexadecimal <see cref="System.String"/>.</returns>
		/// <remarks>This function performs well only on AMD64 machines; it's performance is degraded when working on IA32.</remarks>
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static System.String xxHash_64(System.Byte[] Data, System.Int32 Length, System.Int32 Seed)
        {
            if (!(_CheckDLLVer())) { return "Error"; }
            System.Int32 EFR = XXHASHMETHODS.XXH64(Data, Length, Seed);
            return EFR.ToString("x2");
        }

        /// <summary>
        /// Computes a file hash by using the XXH64 function.
        /// </summary>
        /// <param name="FileStream">The alive <see cref="System.IO.Stream"/> object from which the data will be collected.</param>
        /// <returns>A caculated xxHash64 value written as an hexadecimal <see cref="System.String"/>.</returns>
        /// <remarks>This function performs well only on AMD64 machines; it's performance is degraded when working on IA32.</remarks>
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static System.String xxHash_64(System.IO.Stream FileStream)
        {
            if (!(_CheckDLLVer())) { return "Error"; }
            if (FileStream.Length < 20) { return "Error"; }
            System.Byte[] FBM = new System.Byte[FileStream.Length];
            try
            {
                FileStream.Read(FBM, 0, FBM.Length);
            }
            catch (System.Exception)
            {
                FBM = null;
                return "Error";
            }
            System.Int32 EFR = XXHASHMETHODS.XXH64(FBM, FBM.Length, 0);
            FBM = null;
            return EFR.ToString("x2");
        }
    }

}

namespace ExternalArchivingMethods
{
    // A Collection Namespace for making archives outside Microsoft's managed code.

    /// <summary>
    /// Zstandard archiving compression level.
    /// </summary>
    public enum ZSTDCMPLevel : System.Int32
    {
        /// <summary>
        /// Fast compression.
        /// </summary>
        Fast = 1,
        /// <summary>
        /// Fast compression , but compresses slightly better than <see cref="ZSTDCMPLevel.Fast"/>.
        /// </summary>
        Fast2 = 2,
        /// <summary>
        /// Good compression.
        /// </summary>
        Efficient = 3,
        /// <summary>
        /// A bit better from the <see cref="Efficient"/> compression level.
        /// </summary>
        Lazy = 4,
        /// <summary>
        /// An balanced compression level. This one is the most popular option.
        /// </summary>
        Lazy2 = 5,
        /// <summary>
        /// The files are compressed about an estimated 62-71% ratio.
        /// </summary>
        LazyOptimized = 6,
        /// <summary>
        /// The files are compressed about an estimated 71-80% ratio.
        /// </summary>
        Optimal = 7,
        /// <summary>
        /// The files are compressed about an estimated 80-85% ratio.
        /// </summary>
        Ultra = 8,
        /// <summary>
        /// The Zstandard algorithm will consume as much as possible resources to compress the target as much as possible.
        /// </summary>
        FullCompressPower = 9,
    }

    /// <summary>
    /// <para> zstd is a fast compression algorithm maintained by Facebook. </para> 
	/// <para>
	/// This is a wrapper for the unmanaged library.
    /// Note that you can run this only on AMD64 machines only. </para>
    /// </summary>
    [SupportedOSPlatform("windows")]
    public static class ZstandardArchives
    {
        // Yes , this patch adds (Patch Version 1.5.4.0) the Zstandard Archive format.
        // The runtime algorithm is built from the GitHub tree , version 1.5.2.0.
        // This has the following advantages: 
        //    1. The Dynamic-Link Library for the archive format is native C++.
        //    2. This format is very efficient. It can compress and decompress data very fast.
        //    3. The C algorithm that is comprised from is one of the most fast programming languages.
        //    4. Note that you cannot run earlier versions than 1.5.2.0.
        // NOTICE: the zstd.dll bundled with my library is being built by me.
        // Because actually this API calls the library via unmanaged way (Not very safe)
        // and requires the DLL path , use only updates which are either came from GitHub or other source that
        // is reliable. However , it is still very safe and stable , of course.

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        private static System.Boolean _CheckDLLVer()
        {
            if (ROOT.MAIN.OSProcessorArchitecture() != "AMD64") { return false; }
            if (ZSTD.ZSTD_versionNumber() < 10502) { return false; } else { return true; }
        }

        [System.Security.SuppressUnmanagedCodeSecurity]
        private sealed class ZSTD
        {
            // Proper API Calls defined in this class. DO NOT Modify.
            [System.Runtime.InteropServices.DllImport(Interop.Libraries.Zstd)]
            [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
            public static extern System.Int32 ZSTD_compress(System.Byte[] dst, System.Int32 dstCapacity,
            System.Byte[] src, System.Int32 srcCapacity, ZSTDCMPLevel compressionLevel);

            [System.Runtime.InteropServices.DllImport(Interop.Libraries.Zstd)]
            [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
            public static extern System.Int32 ZSTD_decompress(System.Byte[] dst, System.Int32 dstCapacity,
            System.Byte[] src, System.Int32 srcSize);

            [System.Runtime.InteropServices.DllImport(Interop.Libraries.Zstd)]
            [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
            public static extern System.Int64 ZSTD_getFrameContentSize(System.Byte[] src, System.Int32 srcSize);

            [System.Runtime.InteropServices.DllImport(Interop.Libraries.Zstd)]
            [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
            public static extern System.Int32 ZSTD_isError(System.Int32 code);

            [System.Runtime.InteropServices.DllImport(Interop.Libraries.Zstd)]
            [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
            public static extern System.Int32 ZSTD_findFrameCompressedSize(System.Byte[] src, System.Int32 srcSize);

            [System.Runtime.InteropServices.DllImport(Interop.Libraries.Zstd)]
            [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
            public static extern System.Int32 ZSTD_defaultCLevel();

            [System.Runtime.InteropServices.DllImport(Interop.Libraries.Zstd)]
            [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
            public static extern System.Int32 ZSTD_minCLevel();

            [System.Runtime.InteropServices.DllImport(Interop.Libraries.Zstd)]
            [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
            public static extern System.Int32 ZSTD_maxCLevel();

            [System.Runtime.InteropServices.DllImport(Interop.Libraries.Zstd)]
            [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
            public static extern System.Int32 ZSTD_versionNumber();

        }

        /// <summary>
        /// Compress an alive <see cref="System.IO.FileStream"/> object that contains decompressed data and store the 
        /// compressed data to another and empty <see cref="System.IO.FileStream"/> object.
        /// </summary>
        /// <param name="InputFile">The alive stream object containing the data to compress.</param>
        /// <param name="ArchiveStream">The alive output stream which will contain the compressed data.</param>
        /// <param name="CmpLevel">The compression level to apply.</param>
        /// <returns><c>true</c> if the compression was succeeded; otherwise, <c>false</c>.</returns>
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static System.Boolean CompressAsFileStreams(System.IO.FileStream InputFile,
        System.IO.FileStream ArchiveStream, ZSTDCMPLevel CmpLevel)
        {
            // Start first by checking the DLL version.
            if (!(_CheckDLLVer())) { return false; }
            // Check the filestreams.
            // The Input file must be more than 32 bytes (Just an average measure) , 
            // and the Output file (Result Archive) must be empty.
            if (InputFile.Length < 32) { return false; }
            if (ArchiveStream.Length > 0) { return false; }
            // Next step: Read all the file contents.
            // Initialise a new System.Byte[] object.
            System.Byte[] FSI = new System.Byte[InputFile.Length];
            try
            {
                InputFile.Read(FSI, 0, System.Convert.ToInt32(InputFile.Length));
            }
            catch (System.Exception)
            {
                // Any written contents to the buffer must be disposed.
                FSI = null;
                // Exit because the flushing was not happened sucessfully.
                return false;
            }
            // After the file was read , make the buffer Byte[] that 
            // will host the archive contents (compressed contents).
            // Becuase we cannot know from now which the compressed stream size is ,
            // a good strategy is to initialise a Byte[] same as the file one's.
            System.Byte[] FSO = new System.Byte[InputFile.Length];
            // Now , everything are ready to invoke the compression algorithm.
            // In parallel , set the compressed size as a variable , becuase it gonna be
            // needed to write the archived contents back to the other stream.
            System.Int32 BUFF = ZSTD.ZSTD_compress(FSO, FSO.Length, FSI, FSI.Length, CmpLevel);
            // Check in the meantime if the compression happened sucessfully. 
            // Test that by checking if the final buffer is less than of FSI object,
            if (BUFF >= FSI.Length)
            {
                FSI = null;
                FSO = null;
                return false;
            }
            else
            {
                FSI = null;
            }
            // Now write the data back to the archived file.
            // Here the compressed value is required; otherwise , we have an incorrect archive.
            try
            {
                ArchiveStream.Write(FSO, 0, BUFF);
            }
            catch (System.Exception)
            {
                // In any case that the data could not be written , give up again.
                return false;
            }
            finally
            {
                // Clear now the Archive buffer too , because we have done here sucessfully. 
                FSO = null;
            }
            // Sucessfull Operation.
            return true;
        }

        /// <summary>
        /// Decompress from an alive <see cref="System.IO.FileStream"/> object that contains the compressed data
        /// to another <see cref="System.IO.FileStream"/> object which the decompressed data will be stored.
        /// </summary>
        /// <param name="ArchiveFileStream">The stream object that contains the compressed data.</param>
        /// <param name="DecompressedFileStream">The output file stream to save the data to.</param>
        /// <returns><c>true</c> if the decompression was succeeded; otherwise, <c>false</c>.</returns>
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static System.Boolean DecompressFileStreams(System.IO.FileStream ArchiveFileStream, System.IO.FileStream DecompressedFileStream)
        {
            // Start first by checking the DLL version.
            if (!(_CheckDLLVer())) { return false; }
            // Check whether if the Archive is empty or not and check additionally whether the Output File Is empty.
            // A valid ZSTD archive header is around 62 bytes , adding more to be insured that it is okay.
            if (ArchiveFileStream.Length < 68) { return false; }
            if (DecompressedFileStream.Length > 0) { return false; }
            // Next step: Read all the file contents.
            // Initialise a new System.Byte[] object.
            System.Byte[] FSI = new System.Byte[ArchiveFileStream.Length];
            // Now pass the archive to the buffer.
            // It is controlled whether it was done sucessfully at any case.
            try
            {
                ArchiveFileStream.Read(FSI, 0, System.Convert.ToInt32(ArchiveFileStream.Length));
            }
            catch (System.Exception)
            {
                // If any error was found , close this instance and clear the buffer.
                FSI = null;
                return false;
            }
            // Here we cannot predict the Byte[] length of the output File.
            // We must predict it.
            // The ZSTD_getframeContentSize will help to predict that.
            System.Int32 BUFFOut = System.Convert.ToInt32(ZSTD.ZSTD_getFrameContentSize(FSI, FSI.Length));
            // The below procedure will get the actual file buffer. This will be used so as to initiate the output file buffer.
            System.Int32 BUFF = ZSTD.ZSTD_findFrameCompressedSize(FSI, FSI.Length);
            // Check for any errors / code inefficiencies. 
            if ((ZSTD.ZSTD_isError(BUFF) == 1) || (ZSTD.ZSTD_isError(BUFFOut) == 1))
            {
                FSI = null;
                return false;
            }
            // After this prediction , we can initialise the output file buffer.
            // Add some more bytes , will help it to the decompression process (*.*).
            System.Byte[] FSO = new System.Byte[BUFFOut + 12];
            // Decompression is ready. Call the procedure.
            System.Int32 BUFF1 = ZSTD.ZSTD_decompress(FSO, BUFFOut, FSI, BUFF);
            // Write the compressed data to the output file.
            try
            {
                DecompressedFileStream.Write(FSO, 0, BUFF1);
            }
            catch (System.Exception)
            {
                // If any error found , exit with an error.
                return false;
            }
            finally
            {
                // This is called in any case , even if it fails (Of Course before executing the Catch block.)
                FSO = null;
            }
            // Done.  All were sucessfully executed.
            return true;
        }

    }

    // System.IO.FileStream DM = System.IO.File.OpenRead(@".\ZSEX.zst");
    // System.IO.FileStream VA = System.IO.File.OpenWrite(@".\Out.txt");
    // System.Console.WriteLine(ZstandardArchives.DecompressFileStreams(DM , VA));
    // VA.Close();
    // VA.Dispose();
    // DM.Close();
    // DM.Dispose();
}


[System.Security.SecurityCritical]
[System.Security.SuppressUnmanagedCodeSecurity]
internal static class ConsoleInterop
{
    [DllImport(Interop.Libraries.Kernel32, EntryPoint = "FreeConsole", CallingConvention = CallingConvention.Winapi)]
    [System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        UI = true)]
    internal static extern System.Int32 DetachConsole();

    [DllImport(Interop.Libraries.Kernel32, EntryPoint = "GetConsoleScreenBufferInfo", CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    [System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        UI = true)]
    internal static extern System.Boolean GetBufferInfo(System.IntPtr ConsoleOutputHandle,
        out ROOT.ConsoleExtensions.CONSOLE_SCREEN_BUFFER_INFO ConsoleScreenBufferInfo);

    internal static volatile System.IntPtr InputHandle = System.IntPtr.Zero;
    internal static volatile System.IntPtr OutputHandle = System.IntPtr.Zero;

    [DllImport(Interop.Libraries.Kernel32, EntryPoint = "AttachConsole", CallingConvention = CallingConvention.Winapi)]
    [System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        UI = true)]
    internal static extern System.Int32 AttachToConsole(System.Int32 PID);

    [DllImport(Interop.Libraries.Kernel32, EntryPoint = "AllocConsole", CallingConvention = CallingConvention.Winapi)]
    [System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        UI = true)]
    public static extern System.Int32 CreateConsole();

    [DllImport(Interop.Libraries.Kernel32, EntryPoint = "GetStdHandle", CallingConvention = CallingConvention.Winapi)]
    [System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        UI = true)]
    internal static extern System.IntPtr GetConsoleStream(ROOT.ConsoleExtensions.ConsoleHandleOptions Stream);

    [DllImport(Interop.Libraries.Kernel32, EntryPoint = "WriteConsoleW", CallingConvention = CallingConvention.Winapi)]
    [System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        UI = true)]
    internal static extern System.Int32 WriteToConsoleUnmanaged(System.IntPtr Handle,
        [MarshalAs(UnmanagedType.LPWStr)] System.String Data,
        System.Int32 NChars, [OptionalAttribute] out System.Int32 CharsWritten, System.IntPtr MustBeNull);

    [DllImport(Interop.Libraries.Kernel32, CallingConvention = CallingConvention.Winapi, EntryPoint = "GetConsoleOriginalTitleW")]
    [System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        UI = true)]
    internal static extern System.Int32 OriginalTitle([MarshalAs(UnmanagedType.LPTStr)] System.String Title, System.Int32 Titlesize = 27500);

    [DllImport(Interop.Libraries.Kernel32, BestFitMapping = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi, EntryPoint = "SetConsoleTitleW")]
    [System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        UI = true)]
    internal static extern System.Int32 SetTitle([MarshalAs(UnmanagedType.LPTStr)] System.String Title);

    [DllImport(Interop.Libraries.Kernel32, CallingConvention = CallingConvention.Winapi, EntryPoint = "GetConsoleTitleW")]
    [System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        UI = true)]
    internal static extern System.Int32 GetTitle([MarshalAs(UnmanagedType.LPTStr)] System.String Title, System.Int32 Titlesize = 27500);

    [DllImport(Interop.Libraries.Kernel32, CallingConvention = CallingConvention.Winapi, EntryPoint = "SetConsoleOutputCP")]
    [System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        UI = true)]
    internal static extern System.Int32 SetOutputEnc(System.UInt32 OutputEnc);

    [DllImport(Interop.Libraries.Kernel32, CallingConvention = CallingConvention.Winapi, EntryPoint = "SetConsoleCP")]
    [System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        UI = true)]
    internal static extern System.Int32 SetInputEnc(System.UInt32 InputEnc);

    [DllImport(Interop.Libraries.Kernel32, CallingConvention = CallingConvention.Winapi, EntryPoint = "GetConsoleCP")]
    [System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        UI = true)]
    internal static extern System.UInt32 GetInputEnc();

    [DllImport(Interop.Libraries.Kernel32, CallingConvention = CallingConvention.Winapi, EntryPoint = "GetConsoleOutputCP")]
    [System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        UI = true)]
    internal static extern System.UInt32 GetOutputEnc();

    [DllImport(Interop.Libraries.Kernel32, EntryPoint = "ReadConsoleW", CallingConvention = CallingConvention.Winapi)]
    [System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        UI = true)]
    internal static extern System.Int32 ReadFromConsoleUnmanaged(System.IntPtr Handle,
        System.Byte[] Buffer, System.Int32 NumberOfCharsToRead,
        out System.UInt32 NumberOfCharsRead, System.IntPtr MustBeNull);

    [System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        UI = true)]
    private static void SetOutputHandle() { OutputHandle = GetConsoleStream(ROOT.ConsoleExtensions.ConsoleHandleOptions.Output); }

    [System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        UI = true)]
    private static void SetInputHandle() { InputHandle = GetConsoleStream(ROOT.ConsoleExtensions.ConsoleHandleOptions.Input); }

    [DllImport(Interop.Libraries.Kernel32, CallingConvention = CallingConvention.Winapi, EntryPoint = "SetConsoleTextAttribute")]
    [System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        UI = true)]
    internal static extern void DefineNewAttributes(System.IntPtr Handle, System.Int16 Attributes);

    [System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        UI = true)]
    public static System.Boolean WriteToConsole(System.Char[] data)
    {
        System.String DI = null;
        for (System.Int32 I = 0; I < data.Length; I++) { DI += data[I]; }
        return WriteToConsole(DI);
    }

    public static System.Boolean WriteToConsole(System.String data)
    {
        if (ROOT.ConsoleExtensions.Detached == true) { return true; }
        if (OutputHandle == System.IntPtr.Zero) { SetOutputHandle(); }
        if (WriteToConsoleUnmanaged(OutputHandle, data,
            data.Length, out System.Int32 CHARS, System.IntPtr.Zero) != 0)
        { return true; }
        else { return false; }
    }

    [System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        UI = true)]
    public static System.String ReadFromConsole(ROOT.ConsoleExtensions.ConsoleReadBufferOptions BufSize)
    {
        if (ROOT.ConsoleExtensions.Detached == true) { return ""; }
        if (InputHandle == System.IntPtr.Zero) { SetInputHandle(); }
        System.Byte[] RF = new System.Byte[(System.Int32)BufSize];
        if (ReadFromConsoleUnmanaged(InputHandle, RF, (System.Int32)BufSize,
            out System.UInt32 ED, System.IntPtr.Zero) == 0) { return "Error"; }
        System.String Result = null;
        for (System.Int32 I = 0; I < RF.Length; I++)
        {
            if (
                (RF[I] != (System.Byte)'\0') && (RF[I] != (System.Byte)'\r')
                && (RF[I] != (System.Byte)'\n')) { Result += (System.Char)RF[I]; }
        }
        return Result;
    }
}

[System.Security.SecurityCritical]
[System.Security.SuppressUnmanagedCodeSecurity]
internal static class FileInterop
{
    [DllImport(Interop.Libraries.Kernel32, 
        CallingConvention = CallingConvention.Winapi,
        EntryPoint = "GetFullPathNameW" , 
        SetLastError = true , CharSet = CharSet.Unicode)]
    [System.Security.Permissions.HostProtection(System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        MayLeakOnAbort = true)]
    public static extern System.Int32 GetFullPath(
        [In][MarshalAs(UnmanagedType.LPWStr)] System.String InputFileName,
        [In] System.Int32 InputFileNameLength,
        [Out][MarshalAs(UnmanagedType.LPWStr)] System.String ResultingPath,
        [Out][MarshalAs(UnmanagedType.LPWStr)] System.String DirOrFile);

    [DllImport(Interop.Libraries.Kernel32, CallingConvention = CallingConvention.Winapi,
        EntryPoint = "CreateDirectoryW", CharSet = CharSet.Unicode ,
        SetLastError = true)]
    [System.Security.Permissions.HostProtection(System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        MayLeakOnAbort = true, SecurityInfrastructure = true)]
    public static extern System.Int32 CreateDir([In] System.String Path, [Out][Optional] FileInterop_SECURITY_ATTRIBUTES Desc);

    [DllImport(Interop.Libraries.Kernel32, CallingConvention = CallingConvention.Winapi,
        EntryPoint = "CopyFileW", CharSet = CharSet.Unicode , SetLastError = true)]
    [System.Security.Permissions.HostProtection(System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        MayLeakOnAbort = true, SecurityInfrastructure = true)]
    public static extern System.Int32 CopyFile(
        [In][MarshalAs(UnmanagedType.LPWStr)] System.String PathToExistingFile,
        [In][MarshalAs(UnmanagedType.LPWStr)] System.String PathToCopy,
        [In] System.Boolean TerminateIfExists);

    [DllImport(Interop.Libraries.Kernel32, CallingConvention = CallingConvention.Winapi,
        CharSet = CharSet.Unicode, EntryPoint = "MoveFileW" , SetLastError = true)]
    [System.Security.Permissions.HostProtection(System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        MayLeakOnAbort = true, SecurityInfrastructure = true)]
    public static extern System.Int32 MoveFileOrDir(
        [In][MarshalAs(UnmanagedType.LPWStr)] System.String ExisitingFileOrDir,
        [In][MarshalAs(UnmanagedType.LPWStr)] System.String NewLocation);

    [DllImport(Interop.Libraries.Kernel32, CallingConvention = CallingConvention.Winapi,
        CharSet = CharSet.Unicode, EntryPoint = "CreateHardLinkW" , SetLastError = true)]
    [System.Security.Permissions.HostProtection(System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        MayLeakOnAbort = true, SecurityInfrastructure = true)]
    public static extern System.Int32 CreateHardLink(
        [In][MarshalAs(UnmanagedType.LPWStr)] System.String LinkLocation,
        [In][MarshalAs(UnmanagedType.LPWStr)] System.String FileToPointTo,
        [In][Optional] FileInterop_SECURITY_ATTRIBUTES MustBeNull);

    [DllImport(Interop.Libraries.Kernel32, CallingConvention = CallingConvention.Winapi,
        CharSet = CharSet.Unicode, EntryPoint = "RemoveDirectoryW" , SetLastError = true)]
    [System.Security.Permissions.HostProtection(System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        MayLeakOnAbort = true, SecurityInfrastructure = true)]
    public static extern System.Int32 RemoveDir([In][MarshalAs(UnmanagedType.LPWStr)] System.String PathToDir);

    [DllImport(Interop.Libraries.Kernel32, CallingConvention = CallingConvention.Winapi,
        CharSet = CharSet.Unicode, EntryPoint = "DeleteFileW" , SetLastError = true)]
    [System.Security.Permissions.HostProtection(System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        MayLeakOnAbort = true, SecurityInfrastructure = true)]
    public static extern System.Int32 DeleteFile([MarshalAs(UnmanagedType.LPWStr)] System.String Path);

    [DllImport(Interop.Libraries.Kernel32, CallingConvention = CallingConvention.Winapi,
        CharSet = CharSet.Unicode, EntryPoint = "CreateSymbolicLinkW" , SetLastError = true)]
    [System.Security.Permissions.HostProtection(System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        MayLeakOnAbort = true, SecurityInfrastructure = true)]
    public static extern System.Boolean CreateSymLink(
        [In][MarshalAs(UnmanagedType.LPWStr)] System.String LinkLocation,
        [In][MarshalAs(UnmanagedType.LPWStr)] System.String FileToPointTo,
        [In] FileInterop_SYMLINK_FLAGS Flags);

    [DllImport(Interop.Libraries.Kernel32, CallingConvention = CallingConvention.Winapi,
        CharSet = CharSet.Unicode, EntryPoint = "FindFirstFileW", SetLastError = true)]
    public static extern System.IntPtr FindFile(
        [In][MarshalAs(UnmanagedType.LPWStr)] System.String PathToSearch,
        ref FileInterop_WIN32_FIND_DATA_W Result);

    [DllImport(Interop.Libraries.Kernel32 , CallingConvention = CallingConvention.Winapi , 
        CharSet = CharSet.Unicode , EntryPoint = "FindClose"  , SetLastError = true)]
    public static extern System.Boolean CloseFind([In][Out] System.IntPtr PtrToClose);

    public static System.Boolean FileExists(System.String Path)
    {
        FileInterop_WIN32_FIND_DATA_W G = new();
        System.IntPtr FA = FindFile(Path, ref G);
        if (FA != (System.IntPtr)(-1)) 
        {
            CloseFind(FA);
            return true;
        } else { return false; }
    }

    public static System.Boolean CreateNewLink(System.String PathToSave , System.String PathToPointTo , 
        System.String WorkDir = "") 
    {
        try
        {
            ShellLinkObjectClass link = new();
            link.Path = PathToPointTo;
            link.WorkingDirectory = WorkDir;
            System.IO.FileStream FS = ROOT.MAIN.CreateANewFile(PathToSave);
            if (FS == null) { return false; }
            link.Save(FS);
            FS.Close();
            FS.Dispose();
            return true;
        } catch (System.Runtime.InteropServices.COMException E) 
        {
            if (E.ErrorCode == -2147221164) 
            {
                ROOT.RegEditor RG = new() { RootKey = ROOT.RegRootKeyValues.CurrentClassesRoot, SubKey = "CLSID" };
                if (RG.KeyExists("CLSID\\{11219420-1768-11D1-95BE-00609797EA4F}") == false) 
                {
                    RG.CreateNewKey("CLSID\\{11219420-1768-11D1-95BE-00609797EA4F}");
                }
                RG.Dispose();
                return false;
            } else { throw E; }
        }
    }

}

[Flags]
internal enum FileInterop_SYMLINK_FLAGS : System.UInt32
{
    SYMBOLIC_LINK_FLAG_FILE = 0x0,
    SYMBOLIC_LINK_FLAG_DIRECTORY = 0x1,
    SYMBOLIC_LINK_FLAG_ALLOW_UNPRIVILEGED_CREATE = 0x2
}

[StructLayout(LayoutKind.Explicit, Pack = 16)]
internal struct FileInterop_SECURITY_ATTRIBUTES
{
    [FieldOffset(0)]
    public System.UInt32 nLength;
    [FieldOffset(1)]
    public System.IntPtr lpSecurityDescriptor;
    [FieldOffset(2)]
    public System.Boolean bInheritHandle;
}

[StructLayoutAttribute(LayoutKind.Sequential)]
internal struct FileInterop_FILETIME
{
    public System.UInt32 dwLowDateTime;
    public System.UInt32 dwHighDateTime;
}

[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
internal unsafe struct FileInterop_WIN32_FIND_DATA_W
{
    public System.UInt32 dwFileAttributes;
    public FileInterop_FILETIME ftCreationTime;
    public FileInterop_FILETIME ftLastAccessTime;
    public FileInterop_FILETIME ftLastWriteTime;
    public System.UInt32 nFileSizeHigh;
    public System.UInt32 nFileSizeLow;
    public System.UInt32 dwReserved0;
    public System.UInt32 dwReserved1;
    public fixed System.Char cFileName[400];
    public fixed System.Char cAlternateFileName[14];
    public System.UInt32 dwFileType;
    public System.UInt32 dwCreatorType;
    public System.UInt32 wFinderFlags;

    public System.ReadOnlySpan<System.Char> FileName 
    {  get 
        { fixed (System.Char* ptr = cFileName)  { return new System.ReadOnlySpan<System.Char>(ptr, 400); } } 
    }

    public System.ReadOnlySpan<System.Char> AlternateFileName 
    {
        get { fixed (System.Char* ptr = cAlternateFileName) { return new System.ReadOnlySpan<System.Char>(ptr, 14); } }
    }
}

[ComImport]
[Guid("88A05C00-F000-11CE-8350-444553540000")]
[TypeLibType(TypeLibTypeFlags.FHidden | TypeLibTypeFlags.FDual | TypeLibTypeFlags.FDispatchable)]
internal interface IShellLinkDual
{
    [DispId(1610743808)]
    string Path
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(1610743808)]
        [return: MarshalAs(UnmanagedType.BStr)]
        get;
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(1610743808)]
        [param: In]
        [param: MarshalAs(UnmanagedType.BStr)]
        set;
    }

    [DispId(1610743810)]
    string Description
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(1610743810)]
        [return: MarshalAs(UnmanagedType.BStr)]
        get;
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(1610743810)]
        [param: In]
        [param: MarshalAs(UnmanagedType.BStr)]
        set;
    }

    [DispId(1610743812)]
    string WorkingDirectory
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(1610743812)]
        [return: MarshalAs(UnmanagedType.BStr)]
        get;
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(1610743812)]
        [param: In]
        [param: MarshalAs(UnmanagedType.BStr)]
        set;
    }

    [DispId(1610743814)]
    string Arguments
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(1610743814)]
        [return: MarshalAs(UnmanagedType.BStr)]
        get;
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(1610743814)]
        [param: In]
        [param: MarshalAs(UnmanagedType.BStr)]
        set;
    }

    [DispId(1610743816)]
    int Hotkey
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(1610743816)]
        get;
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(1610743816)]
        [param: In]
        set;
    }

    [DispId(1610743818)]
    int ShowCommand
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(1610743818)]
        get;
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(1610743818)]
        [param: In]
        set;
    }

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    [DispId(1610743820)]
    void Resolve([In] int fFlags);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    [DispId(1610743821)]
    int GetIconLocation([MarshalAs(UnmanagedType.BStr)] out string pbs);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    [DispId(1610743822)]
    void SetIconLocation([In][MarshalAs(UnmanagedType.BStr)] string bs, [In] int iIcon);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    [DispId(1610743823)]
    void Save([Optional][In][MarshalAs(UnmanagedType.Struct)] object vWhere);
}

[ComImport]
[TypeLibType(TypeLibTypeFlags.FHidden | TypeLibTypeFlags.FDual | TypeLibTypeFlags.FDispatchable)]
[Guid("317EE249-F12E-11D2-B1E4-00C04F8EEB3E")]
internal interface IShellLinkDual2 : IShellLinkDual
{
    [DispId(1610743808)]
    new string Path
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(1610743808)]
        [return: MarshalAs(UnmanagedType.BStr)]
        get;
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(1610743808)]
        [param: In]
        [param: MarshalAs(UnmanagedType.BStr)]
        set;
    }

    [DispId(1610743810)]
    new string Description
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(1610743810)]
        [return: MarshalAs(UnmanagedType.BStr)]
        get;
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(1610743810)]
        [param: In]
        [param: MarshalAs(UnmanagedType.BStr)]
        set;
    }

    [DispId(1610743812)]
    new string WorkingDirectory
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(1610743812)]
        [return: MarshalAs(UnmanagedType.BStr)]
        get;
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(1610743812)]
        [param: In]
        [param: MarshalAs(UnmanagedType.BStr)]
        set;
    }

    [DispId(1610743814)]
    new string Arguments
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(1610743814)]
        [return: MarshalAs(UnmanagedType.BStr)]
        get;
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(1610743814)]
        [param: In]
        [param: MarshalAs(UnmanagedType.BStr)]
        set;
    }

    [DispId(1610743816)]
    new int Hotkey
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(1610743816)]
        get;
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(1610743816)]
        [param: In]
        set;
    }

    [DispId(1610743818)]
    new int ShowCommand
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(1610743818)]
        get;
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(1610743818)]
        [param: In]
        set;
    }

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    [DispId(1610743820)]
    new void Resolve([In] int fFlags);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    [DispId(1610743821)]
    new int GetIconLocation([MarshalAs(UnmanagedType.BStr)] out string pbs);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    [DispId(1610743822)]
    new void SetIconLocation([In][MarshalAs(UnmanagedType.BStr)] string bs, [In] int iIcon);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    [DispId(1610743823)]
    new void Save([Optional][In][MarshalAs(UnmanagedType.Struct)] object vWhere);

    [DispId(1610809344)]
    FolderItem Target
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(1610809344)]
        [return: MarshalAs(UnmanagedType.Interface)]
        get;
    }
}

[ComImport]
[Guid("317EE249-F12E-11D2-B1E4-00C04F8EEB3E")]
[CoClass(typeof(ShellLinkObjectClass))]
internal interface ShellLinkObject : IShellLinkDual2 { }

[ComImport]
[Guid("11219420-1768-11D1-95BE-00609797EA4F")]
[ClassInterface(ClassInterfaceType.None)]
internal class ShellLinkObjectClass : IShellLinkDual2, ShellLinkObject
{
    [DispId(1610743808)]
    public virtual extern string Path
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(1610743808)]
        [return: MarshalAs(UnmanagedType.BStr)]
        get;
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(1610743808)]
        [param: In]
        [param: MarshalAs(UnmanagedType.BStr)]
        set;
    }

    [DispId(1610743810)]
    public virtual extern string Description
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(1610743810)]
        [return: MarshalAs(UnmanagedType.BStr)]
        get;
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(1610743810)]
        [param: In]
        [param: MarshalAs(UnmanagedType.BStr)]
        set;
    }

    [DispId(1610743812)]
    public virtual extern string WorkingDirectory
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(1610743812)]
        [return: MarshalAs(UnmanagedType.BStr)]
        get;
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(1610743812)]
        [param: In]
        [param: MarshalAs(UnmanagedType.BStr)]
        set;
    }

    [DispId(1610743814)]
    public virtual extern string Arguments
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(1610743814)]
        [return: MarshalAs(UnmanagedType.BStr)]
        get;
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(1610743814)]
        [param: In]
        [param: MarshalAs(UnmanagedType.BStr)]
        set;
    }

    [DispId(1610743816)]
    public virtual extern int Hotkey
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(1610743816)]
        get;
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(1610743816)]
        [param: In]
        set;
    }

    [DispId(1610743818)]
    public virtual extern int ShowCommand
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(1610743818)]
        get;
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(1610743818)]
        [param: In]
        set;
    }

    [DispId(1610809344)]
    public virtual extern FolderItem Target
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(1610809344)]
        [return: MarshalAs(UnmanagedType.Interface)]
        get;
    }

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    [DispId(1610743820)]
    public virtual extern void Resolve([In] int fFlags);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    [DispId(1610743821)]
    public virtual extern int GetIconLocation([MarshalAs(UnmanagedType.BStr)] out string pbs);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    [DispId(1610743822)]
    public virtual extern void SetIconLocation([In][MarshalAs(UnmanagedType.BStr)] string bs, [In] int iIcon);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    [DispId(1610743823)]
    public virtual extern void Save([Optional][In][MarshalAs(UnmanagedType.Struct)] object vWhere);
}

[ComImport]
[System.Reflection.DefaultMember("Name")]
[TypeLibType(TypeLibTypeFlags.FDual | TypeLibTypeFlags.FDispatchable)]
[Guid("FAC32C80-CBE4-11CE-8350-444553540000")]
internal interface FolderItem
{
    [DispId(1610743808)]
    object Application
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(1610743808)]
        [return: MarshalAs(UnmanagedType.IDispatch)]
        get;
    }

    [DispId(1610743809)]
    object Parent
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(1610743809)]
        [return: MarshalAs(UnmanagedType.IDispatch)]
        get;
    }

    [DispId(0)]
    string Name
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(0)]
        [return: MarshalAs(UnmanagedType.BStr)]
        get;
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(0)]
        [param: In]
        [param: MarshalAs(UnmanagedType.BStr)]
        set;
    }

    [DispId(1610743812)]
    string Path
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(1610743812)]
        [return: MarshalAs(UnmanagedType.BStr)]
        get;
    }

    [DispId(1610743813)]
    object GetLink
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(1610743813)]
        [return: MarshalAs(UnmanagedType.IDispatch)]
        get;
    }

    [DispId(1610743814)]
    object GetFolder
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(1610743814)]
        [return: MarshalAs(UnmanagedType.IDispatch)]
        get;
    }

    [DispId(1610743815)]
    bool IsLink
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(1610743815)]
        get;
    }

    [DispId(1610743816)]
    bool IsFolder
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(1610743816)]
        get;
    }

    [DispId(1610743817)]
    bool IsFileSystem
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(1610743817)]
        get;
    }

    [DispId(1610743818)]
    bool IsBrowsable
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(1610743818)]
        get;
    }

    [DispId(1610743819)]
    DateTime ModifyDate
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(1610743819)]
        get;
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(1610743819)]
        [param: In]
        set;
    }

    [DispId(1610743821)]
    int Size
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(1610743821)]
        get;
    }

    [DispId(1610743822)]
    string Type
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(1610743822)]
        [return: MarshalAs(UnmanagedType.BStr)]
        get;
    }

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    [DispId(1610743823)]
    [return: MarshalAs(UnmanagedType.Interface)]
    FolderItemVerbs Verbs();

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    [DispId(1610743824)]
    void InvokeVerb([Optional][In][MarshalAs(UnmanagedType.Struct)] object vVerb);
}

[ComImport]
[TypeLibType(TypeLibTypeFlags.FDual | TypeLibTypeFlags.FDispatchable)]
[Guid("1F8352C0-50B0-11CF-960C-0080C7F4EE85")]
internal interface FolderItemVerbs : System.Collections.IEnumerable
{
    [DispId(1610743808)]
    int Count
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(1610743808)]
        get;
    }

    [DispId(1610743809)]
    object Application
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(1610743809)]
        [return: MarshalAs(UnmanagedType.IDispatch)]
        get;
    }

    [DispId(1610743810)]
    object Parent
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(1610743810)]
        [return: MarshalAs(UnmanagedType.IDispatch)]
        get;
    }

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    [DispId(1610743811)]
    [return: MarshalAs(UnmanagedType.Interface)]
    FolderItemVerb Item([Optional][In][MarshalAs(UnmanagedType.Struct)] object index);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    [DispId(-4)]
    [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "System.Runtime.InteropServices.CustomMarshalers.EnumeratorToEnumVariantMarshaler, CustomMarshalers, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    new System.Collections.IEnumerator GetEnumerator();
}

[ComImport]
[System.Reflection.DefaultMember("Name")]
[Guid("08EC3E00-50B0-11CF-960C-0080C7F4EE85")]
[TypeLibType(TypeLibTypeFlags.FDual | TypeLibTypeFlags.FDispatchable)]
internal interface FolderItemVerb
{
    [DispId(1610743808)]
    object Application
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(1610743808)]
        [return: MarshalAs(UnmanagedType.IDispatch)]
        get;
    }

    [DispId(1610743809)]
    object Parent
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(1610743809)]
        [return: MarshalAs(UnmanagedType.IDispatch)]
        get;
    }

    [DispId(0)]
    string Name
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(0)]
        [return: MarshalAs(UnmanagedType.BStr)]
        get;
    }

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    [DispId(1610743811)]
    void DoIt();
}