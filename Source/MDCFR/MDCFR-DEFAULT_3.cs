﻿
// An All-In-One framework abstracting the most important classes that are used in .NET
// that are more easily and more consistently to be used.
// The framework was designed to host many different operations , with the last goal 
// to be everything accessible for everyone.

// Global namespaces
using System;
using System.Runtime.Versioning;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ROOT
{

    /// <summary>
    /// This class contains the internal console implementation extensions , which some of them are exposed publicly. <br />
    /// This class cannot be inherited.
    /// </summary>
    public static class ConsoleExtensions
    {
        [Serializable]
        internal enum ConsoleHandleOptions : System.UInt32 { Input = 0xFFFFFFF6, Output = 0xFFFFFFF5, Error = 0xFFFFFFF4 }

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
        [Serializable]
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
        /// <exception cref="PlatformNotSupportedException">
		/// This exception will be thrown when MDCFR was built and used for other platforms , such as Unix.
		/// </exception>
        [System.Security.SecurityCritical]
        public static System.IntPtr GetOutputHandle()
        {
#if IsWindows
            if (ConsoleInterop.OutputHandle == System.IntPtr.Zero)
            {
                ConsoleInterop.OutputHandle = ConsoleInterop.GetConsoleStream(ConsoleHandleOptions.Output);  
            }
            return ConsoleInterop.OutputHandle;
#else
            throw new System.PlatformNotSupportedException("This API is not supported for non-Windows operating systems.");
#endif
        }

        /// <summary>
        /// Gets the underlying KERNEL32 handle which this implementation uses to read from the console.
        /// </summary>
        /// <returns>A new <see cref="System.IntPtr"/> handle which is the handle for reading data 
        /// from the console.</returns>
        /// <exception cref="PlatformNotSupportedException">
		/// This exception will be thrown when MDCFR was built and used for other platforms , such as Unix.
		/// </exception>
        [System.Security.SecurityCritical]
        public static System.IntPtr GetInputHandle()
        {
#if IsWindows
            if (ConsoleInterop.InputHandle == System.IntPtr.Zero)
            {
                ConsoleInterop.InputHandle = ConsoleInterop.GetConsoleStream(ConsoleHandleOptions.Output);
            }
            return ConsoleInterop.InputHandle;
#else
            throw new System.PlatformNotSupportedException("This API is not supported for non-Windows operating systems.");
#endif
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
        /// and this is equivalent to <see cref="System.Console.ForegroundColor"/> property. <br />
        /// On other platforms except Windows , this will fall back to the default <see cref="System.Console.ForegroundColor"/> property.
        /// </summary>
        public static System.ConsoleColor ForegroundColor
        {
#if IsWindows
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
#else
            [System.Security.SecurityCritical]
            get { return System.Console.ForegroundColor; }
            [System.Security.SecurityCritical]
            set { System.Console.ForegroundColor = value; }
#endif
        }

        /// <summary>
        /// Globally set or get the console background color. This property is used for the exported MDCFR functions , 
        /// and this is equivalent to <see cref="System.Console.BackgroundColor"/> property. <br />
        /// On other platforms except Windows , this will fall back to the default <see cref="System.Console.BackgroundColor"/> property.
        /// </summary>
        public static System.ConsoleColor BackgroundColor
        {
#if IsWindows
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
#else
            [System.Security.SecurityCritical]
            get { return System.Console.BackgroundColor; }
            [System.Security.SecurityCritical]
            set { System.Console.BackgroundColor = value; }
#endif
        }

        /// <summary>
        /// Get or Set the Console's Output encoding as an <see cref="System.Text.Encoding"/> class. <br />
        /// In case that this API was called from other platforms than Windows , then this will call the
        /// <see cref="System.Console.OutputEncoding"/> property.
        /// </summary>
        /// <exception cref="ExecutionException">
        /// Occurs when the Code Page defined to the 
        /// console does not exist as an <see cref="System.Text.Encoding"/> class.</exception>
        /// <exception cref="InvalidOperationException">
        /// Occurs when the specified Code Page is invalid for the console.</exception>
        public static System.Text.Encoding OutputEncoding
        {
#if IsWindows
            [System.Security.SecurityCritical]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                System.UInt32 CP = ConsoleInterop.GetOutputEnc();
                if (CP == 0)
                {
                    throw new ExecutionException("Error occured while getting the current code page: Native call returned 0.");
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
                        throw new ExecutionException($"Could not get the codepage set to the console: {CP} .", EX);
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
#else
            [System.Security.SecurityCritical]
            get { return System.Console.OutputEncoding; }
            [System.Security.SecurityCritical]
            set { System.Console.OutputEncoding = value; }
#endif
        }

        /// <summary>
        /// Get or Set the Console's Input encoding as an <see cref="System.Text.Encoding"/> class. <br />
        /// In case that this API was called from other platforms than Windows , then this will call the
        /// <see cref="System.Console.InputEncoding"/> property.
        /// </summary>
        /// <exception cref="ExecutionException">
        /// Occurs when the Code Page defined to the 
        /// console does not exist as an <see cref="System.Text.Encoding"/> class.</exception>
        /// <exception cref="InvalidOperationException">
        /// Occurs when the specified Code Page is invalid for the console.</exception>
        public static System.Text.Encoding InputEncoding
        {
#if IsWindows
            [System.Security.SecurityCritical]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                System.UInt32 CP = ConsoleInterop.GetInputEnc();
                if (CP == 0)
                {
                    throw new ExecutionException("Error occured while getting the current code page!!!");
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
                        throw new ExecutionException($"Could not get the codepage set to the console: {CP} .", EX);
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
                    throw new InvalidOperationException(
                        "Cannot apply the specific code page as a console input encoding. \n" +
                        $"Code Page Identifier: {CP} \n" +
                        $"Code Page Name: {value.BodyName} \n" +
                        $"Code Page Web Name: {value.WebName} \n" +
                        $"Code Page Windows Name: {value.WindowsCodePage}");
                }
            }
#else
            [System.Security.SecurityCritical]
            get { return System.Console.InputEncoding; }
            [System.Security.SecurityCritical]
            set { System.Console.InputEncoding = value; }
#endif
        }

        /// <summary>
        /// Get or Set the current console title. This property is equivalent to <see cref="System.Console.Title"/> property. <br />
        /// In case that you use this API in other platforms than Windows , this will fall back to the original <see cref="System.Console.Title"/> property.
        /// </summary>
        public static System.String Title
        {
#if IsWindows
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
#else
            get 
            {
                return System.Console.Title;
            }
            set { System.Console.Title = value; }
#endif
        }

        /// <summary>
        /// Gets the original title , when the application attached to the console. <br />
        /// Note: This API is only supported for Windows. On other platforms , such as Unix , 
        /// it will throw a new <see cref="PlatformNotSupportedException"/>.
        /// </summary>
        /// <exception cref="PlatformNotSupportedException">
        /// This API was used in other platform than Windows.
        /// </exception>
        public static System.String OriginalTitle
        {
#if IsWindows
            [System.Security.SecurityCritical]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ConsoleInterop.OriginalTitle(T, T.Length);
                System.String I = T;
                T = T2;
                return MAIN.RemoveDefinedChars(I, '\0');
            }
#else
            get
            {
                throw new System.PlatformNotSupportedException("This API is only supported for Windows.");
            }
#endif
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
#if NET7_0_OR_GREATER == false
            new System.Security.Permissions.UIPermission(System.Security.Permissions.UIPermissionWindow.SafeTopLevelWindows).Demand();
#endif
            CONSOLE_SCREEN_BUFFER_INFO INF = GetCBufferInfo(true, out System.Boolean SU);
            System.Int16 attrs = INF.wAttributes;
            attrs = (short)(attrs & -16);
            attrs = (short)((ushort)attrs | (ushort)ConsoleColorToColorAttribute(Color, false));
            ConsoleInterop.DefineNewAttributes(ConsoleInterop.OutputHandle, attrs);
        }

        internal static void SetBackColor(System.ConsoleColor Color)
        {
#if NET7_0_OR_GREATER == false
            new System.Security.Permissions.UIPermission(System.Security.Permissions.UIPermissionWindow.SafeTopLevelWindows).Demand();
#endif
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
        /// Revert the current implementation's back to default console colors , when it is initiated. <br />
        /// Note: This API is only for Windows. If it is called on other operating systems, it will throw a
        /// <see cref="PlatformNotSupportedException"/>.
        /// </summary>
        /// <exception cref="PlatformNotSupportedException">
        /// Thrown when the MDCFR is used in other platforms , such as Unix.
        /// </exception>
        public static void ToDefaultColors()
        {
#if IsWindows
            InitIfNotInitOut();
            ConsoleControlChars F = ConsoleColorToColorAttribute(System.ConsoleColor.Black, true);
            ConsoleInterop.DefineNewAttributes(ConsoleInterop.OutputHandle, (System.Int16)F);
            F = ConsoleColorToColorAttribute(System.ConsoleColor.Gray, false);
            ConsoleInterop.DefineNewAttributes(ConsoleInterop.OutputHandle, (System.Int16)F);
#else
            throw new System.PlatformNotSupportedException("This API is only supported for Windows.");
#endif
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
            public override void GenerateKey() 
            {
#if NET7_0_OR_GREATER
                default(System.Security.Cryptography.RandomNumberGenerator).GetBytes(KeyValue);
#else
                System.Security.Cryptography.RNGCryptoServiceProvider.Create().GetNonZeroBytes(KeyValue); 
#endif
            }

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

    [SpecialName]
    [CompilerGlobalScope]
    [CompilationRelaxations(CompilationRelaxations.NoStringInterning)]
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
#if IsWindows
            if (Executed == false)
            {
                if (ROOT.MAIN.CreateConsole() == false) { ConsoleExtensions.Detached = false; }
                if (ConsoleExtensions.Detached) { ROOT.MAIN.CreateConsole(); }
                Executed = true;
            }
#else
            if (Executed == false) { Executed = true; }
#endif
        }

#if DEBUG == false
#pragma warning disable CS0162
#endif
        internal static void DebuggingInfo(System.String Info)
        {
            if (UseDebugger) 
            {
                EnsureConsoleOpen();
#if IsWindows
                MAIN.WriteConsoleText(DBGINFOShow + " " + Info);
#else
                System.Console.WriteLine(DBGINFOShow + " " + Info);
#endif
            }
        }
#if DEBUG == false
#pragma warning restore CS0162
#endif
    }

    /// <summary>
    /// Passes additional information to use when the process will be launched.
    /// </summary>
    [Serializable]
    [RequiresPreviewFeatures]
    public struct ProcessCreatorData
    {
        /// <summary>
        /// Initialises a new instance of <see cref="ProcessCreatorData"/>.
        /// </summary>
        public ProcessCreatorData()
        {
            _Title = System.String.Empty;
            Path = null;
            Args = null;
            WorkingDirectory = null;
            Options = ProcessCreationOptions.NoOptions;
            X = 0;
            Y = 0;
            LaunchFullScreen = false;
        }

        private System.String _Title;

        /// <summary>
        /// The process path to launch.
        /// </summary>
        public System.String Path;
        /// <summary>
        /// The additional command-line arguments to pass.
        /// </summary>
        public System.String Args;
        /// <summary>
        /// The directory that the process will work in. 
        /// If not defined , it will be set to the application directory.
        /// </summary>
        public System.String WorkingDirectory;
        /// <summary>
        /// The additional launch options to use. Can be a bit-wise combination of the fields.
        /// </summary>
        public ProcessCreationOptions Options;

        /// <summary>
        /// Sets the console window title. This applies to console applications only.
        /// </summary>
        /// <remarks>
        /// This property works only with console applications , and therefore
        /// will throw <see cref="System.InvalidOperationException" /> when the
        /// <see cref="Options"/> are set either to <see cref="ProcessCreationOptions.NoConsoleWindow"/> or
        /// <see cref="ProcessCreationOptions.NoOptions"/>.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// The Title property cannot be set when the <see cref="ProcessCreationOptions.NoConsoleWindow"/> or
        /// <see cref="ProcessCreationOptions.NoOptions"/> are used.
        /// </exception>
        public System.String Title 
        {
            get { return _Title; }
            set 
            { 
                if (Options == ProcessCreationOptions.NoConsoleWindow || 
                    Options == ProcessCreationOptions.NoOptions) 
                {
                    _Title = "";
                    throw new InvalidOperationException(
                    "This option cannot be defined when NoConsoleWindow option is specified.\n" +
                    "Either set it to other value , or do not set it at all."); 
                }
                _Title = value; 
            }
        }

        /// <summary>
        /// Sets the X-Coordinate that the process will use as the X point in screen. <br />
        /// Set this property to zero so as this property is NOT considered when launching.
        /// </summary>
        /// <remarks>
        /// Be noted , though , that this property actually works when the app you want to run allows it. <br />
        /// Such example which allows to use this property is cmd.exe itself.
        /// </remarks>
        public System.UInt32 X { internal get; set; }

        /// <summary>
        /// Sets the Y-Coordinate that the process will use as the Y point in screen. <br />
        /// Set this property to zero so as this property is NOT considered when launching.
        /// </summary>
        /// <remarks>
        /// Be noted , though , that this property actually works when the app you want to run allows it. <br />
        /// Such example which allows to use this property is cmd.exe itself.
        /// </remarks>
        public System.UInt32 Y { internal get; set; }

        /// <summary>
        /// Launch the process and show it in the whole screen , if this is set to <see langword="true"/>.
        /// </summary>
        /// <remarks>
        /// Be noted , though , that this property actually works when the app you want to run allows it.
        /// </remarks>
        public System.Boolean LaunchFullScreen { internal get; set; }
    }

    /// <summary>
    /// Create a new process with the specified options of this enumeration. <br />
    /// You can also specify a bit-wise combination of these options.
    /// </summary>
    [Flags]
    [Serializable]
    [RequiresPreviewFeatures]
    public enum ProcessCreationOptions : System.UInt32
    {
        /// <summary>
        /// No more options than the needed ones will be passed to the process.
        /// </summary>
        NoOptions = 0,
        /// <summary>
        /// Creates a new console for the process.
        /// </summary>
        NewConsole = ProcessInterop_ProcessFLAGS.CREATE_NEW_CONSOLE,
        /// <summary>
        /// When in console applications , this option will not create a new console for that app. <br />
        /// However , if that app uses MDCFR , then the user can spawn a console using the 
        /// <see cref="MAIN.CreateConsole()"/> function.
        /// </summary>
        NoConsoleWindow = ProcessInterop_ProcessFLAGS.CREATE_NO_WINDOW,
        /// <summary>
        /// This will put the spawned process in a new process group containing this one. <br />
        /// Additionally , CTRL + C command is ignored if the spawned process is a console application.
        /// </summary>
        AsNewProcessGroup = ProcessInterop_ProcessFLAGS.CREATE_NEW_PROCESS_GROUP,
        /// <summary>
        /// Create the process with extra layers of security.
        /// </summary>
        AsNewSecureProcess = ProcessInterop_ProcessFLAGS.CREATE_SECURE_PROCESS,
        /// <summary>
        /// Spawn the process , then immediately put it into suspension mode.
        /// </summary>
        SpawnSuspended = ProcessInterop_ProcessFLAGS.CREATE_SUSPENDED,
    }

    /// <summary>
    /// Represents the opened process memory information data.<br />
    /// All measurements provided here are translated to memory kilobytes (KB's)
    /// </summary>
    [Serializable]
    [RequiresPreviewFeatures]
    public struct ProcessMemoryInfo
    {
        /// <summary>
        /// The freed and still unused memory from the process , and it is available to it.
        /// </summary>
        public System.Int64 AvailableMemoryCommit;
        /// <summary>
        /// The private commited memory for the process.
        /// </summary>
        public System.Int64 PrivateCommitUsage;
        /// <summary>
        /// The peak private commited memory caught for the process.
        /// </summary>
        public System.Int64 PeakPrivateCommitUsage;
        /// <summary>
        /// The total memory usage of the process.
        /// </summary>
        public System.Int64 TotalMemoryCommitUsage;
    }

    /// <summary>
    /// Represents the opened process timing information. <br />
    /// It contains time information related to when the process started , exited ,
    /// current CPU execution time and User context execution time.
    /// </summary>
    [Serializable]
    [RequiresPreviewFeatures]
    public struct ProcessTimesInfo 
    {
        /// <summary>
        /// Represents the CPU (kernel) execution time.
        /// </summary>
        public System.TimeSpan KernelTime;
        /// <summary>
        /// Represents the User context execution time.
        /// </summary>
        public System.TimeSpan UserTime;
        /// <summary>
        /// Represents the process time point that was started.
        /// </summary>
        public System.DateTime StartedTime;
        /// <summary>
        /// Represents the point when the process was exited. <br />
        /// Will be <see cref="System.DateTime.MinValue"/> in case that the process
        /// has not exited yet.
        /// </summary>
        public System.DateTime ExitedTime;
        /// <summary>
        /// Gets the current CPU clock execution cycles for the representing process.
        /// </summary>
        /// <remarks>In case that the cycles count have been larger than <see cref="System.UInt64.MaxValue"/> , then it returns zero.</remarks>
        public System.UInt64 ExecutionTimeNow;
    }

    /// <summary>
    /// Provides memory priority values for the opened process.
    /// </summary>
    [Serializable]
    [RequiresPreviewFeatures]
    public enum ProcessMemoryPriority : System.Int16
    {
        /// <summary>Reserved field value.</summary>
        Error = 0,
        /// <summary>Very low priority.</summary>
        VeryLow = (System.Int16)ProcessInterop_Memory_Priority_Levels.MEMORY_PRIORITY_VERY_LOW,
        /// <summary>Low priority.</summary>
        Low = (System.Int16)ProcessInterop_Memory_Priority_Levels.MEMORY_PRIORITY_LOW,
        /// <summary>Medium priority.</summary>
        Medium = (System.Int16)ProcessInterop_Memory_Priority_Levels.MEMORY_PRIORITY_MEDIUM,
        /// <summary>Normal priority. It is also and the default memory priority for all processes.</summary>
        Normal = (System.Int16)ProcessInterop_Memory_Priority_Levels.MEMORY_PRIORITY_NORMAL,
        /// <summary>Below Normal priority.</summary>
        BelowNormal = (System.Int16)ProcessInterop_Memory_Priority_Levels.MEMORY_PRIORITY_BELOW_NORMAL
    }

    /// <summary>
    /// Provides information about the status of the running process.
    /// </summary>
    [Serializable]
    [RequiresPreviewFeatures]
    public enum ProcessExecutionState
    {
        /// <summary>Reserved property.</summary>
        None = 0,
        /// <summary>
        /// The instance is not attached to a process yet.
        /// </summary>
        NotAttached = 1,
        /// <summary>
        /// The process that this instance refers to is still running.
        /// </summary>
        Running = 2,
        /// <summary>
        /// The process was either terminated by this instance or by itself.
        /// </summary>
        Terminated = 3
    }

    /// <summary>
    /// Creates and launches a new process context. <br />
    /// This class cannot be inherited.
    /// </summary>
    /// <remarks>Although that it is an alternative to 
    /// <see cref="System.Diagnostics.Process"/> class,
    /// this only works for Windows platforms. 
    /// </remarks>
    [RequiresPreviewFeatures]
    [SupportedOSPlatform("windows")]
    public sealed class ProcessCreator : System.IDisposable , System.IEquatable<ProcessCreator>
    {
        private ProcessInterop_StartupInfo startupInfo = new();
        private ProcessInterop.ProcessResult result = null;
        private System.Boolean iscurrentprocess = false;
        private System.Boolean hasnotstartedyet = true;
        private System.Boolean isdisposed = false;
        private System.IO.FileInfo fileinfo = null;
        private System.UInt32 ErrorCode = 0;

        /// <summary>
        /// Initialises a new instance of <see cref="ProcessCreator"/> class.
        /// </summary>
        public ProcessCreator() { }

        /// <summary>
        /// Initialises a new instance of <see cref="ProcessCreator"/> class and
        /// launches a new process , given the provided path, arguments and working directory.
        /// </summary>
        /// <param name="Path">The process path. The path supplied must exist.</param>
        /// <param name="Arguments">The arguments to pass.</param>
        /// <param name="WorkingDirectory">The process working directory.</param>
        public ProcessCreator(System.String Path, 
            System.String Arguments = " ",
            System.String WorkingDirectory = null)
        { Launch(Path, Arguments, WorkingDirectory); }

        /// <summary>
        /// Initialises a new instance of <see cref="ProcessCreator"/> class and
        /// launches a new process , given the process data provided.
        /// </summary>
        /// <param name="Data">The process data that will be used to launch the application.</param>
        public ProcessCreator(ProcessCreatorData Data) { LaunchInternal(Data); }

        /// <summary>
        /// Launches a new process , given the process data provided.
        /// </summary>
        /// <param name="Data">The process data that will be used to launch the application.</param>
        public void Launch(ProcessCreatorData Data) { LaunchInternal(Data); }

        /// <summary>
        /// Launches a new process , given the provided path, arguments and working directory.
        /// </summary>
        /// <param name="Path">The process path. The path supplied must exist.</param>
        /// <param name="Arguments">The arguments to pass.</param>
        /// <param name="WorkingDirectory">The process working directory.</param>
        public void Launch(System.String Path , System.String Arguments = " " , System.String WorkingDirectory = null)
        {
            ProcessCreatorData D = new();
            D.Path = Path;
            D.Args = Arguments;
            D.WorkingDirectory = WorkingDirectory;
            D.Options = ProcessCreationOptions.NoOptions;
            D.X = 0;
            D.Y = 0;
            D.LaunchFullScreen = false;
            LaunchInternal(D);
        }

        /// <summary>
        /// Gets the current process data and translates them to a new <see cref="ProcessCreator"/> class.
        /// </summary>
        [System.Security.SuppressUnmanagedCodeSecurity]
        public void GetFromCurrentProcess()
        {
            EnsureNotStarted();
            iscurrentprocess = true;
            result = new();
            result.ProcessH.ProcessHandle = ProcessInterop.GetThisProcess();
            result.ProcessH.ThreadHandle = ProcessInterop.GetThisThread();
            result.ProcessH.ProcessPID = (System.Int32)ProcessInterop.GetProcessPID(result.ProcessH.ProcessHandle);
            result.ProcessH.ProcessTID = (System.Int32)ProcessInterop.GetThreadID(result.ProcessH.ThreadHandle);
            result.MemPriorityInfo = ProcessInterop.GetMemPriorityInfo(result.ProcessH.ProcessHandle);
            result.MemInfo = ProcessInterop.GetMemoryInfo(result.ProcessH.ProcessHandle);
            result.Timing = ProcessInterop.GetProcessTimes(result.ProcessH.ProcessHandle);
            hasnotstartedyet = false;
            result.Success = true;
        }

        // This method translates the structure data given to proper native initialisation settings.
        [System.Security.SuppressUnmanagedCodeSecurity]
        private void LaunchInternal(ProcessCreatorData Data)
        {
            EnsureNotDisposed();
            EnsureNotStarted();
            result = new();
            fileinfo = new(Data.Path);
            if (fileinfo == null) { result.Success = false; return; }
            if (fileinfo?.Exists == false) { result.Success = false; return; }
            if (System.String.IsNullOrEmpty(Data.WorkingDirectory)) 
            { Data.WorkingDirectory = fileinfo.DirectoryName; }
            if (System.String.IsNullOrEmpty(Data.Title) && Data.X == 0 && Data.Y == 0)
            {
                LaunchInternalCore(fileinfo.FullName, " " + Data.Args, Data.WorkingDirectory, Data.Options);
            } else 
            {
                startupInfo.cb = 500;
                if (System.String.IsNullOrEmpty(Data.Title) == false) { startupInfo.lpTitle = Data.Title; }
                if (Data.LaunchFullScreen && Data.X > 0 && Data.Y > 0) 
                {
                    startupInfo.dwX = Data.X;
                    startupInfo.dwY = Data.Y;
                    startupInfo.dwFlags = ProcessInterop_PIFLAGS.STARTF_RUNFULLSCREEN | 
                        ProcessInterop_PIFLAGS.STARTF_USEPOSITION; 
                } else if (Data.LaunchFullScreen)
                {
                    startupInfo.dwFlags = ProcessInterop_PIFLAGS.STARTF_RUNFULLSCREEN;
                } else if (Data.X > 0 && Data.Y > 0)
                {
                    startupInfo.dwX = Data.X;
                    startupInfo.dwY = Data.Y;
                    startupInfo.dwFlags = ProcessInterop_PIFLAGS.STARTF_USEPOSITION;
                }
                LaunchInternalCore(fileinfo.FullName, " " + Data.Args, Data.WorkingDirectory, Data.Options , startupInfo);
            }
        }

        private void LaunchInternalCore(System.String Path, System.String Args, System.String Dir , ProcessCreationOptions CO)
        {
            if (CO == ProcessCreationOptions.NoOptions) { CO = (ProcessCreationOptions)0x00000400; }
            ProcessInterop.ProcessResult PR = ProcessInterop.LaunchProcess(Path, Args, (ProcessInterop_ProcessFLAGS)CO , Dir , false);
            ErrorCode = WindowsErrorCodes.LastErrorCode;
            result = PR;
            if (result.Success) { hasnotstartedyet = false; }
            return;
        }

        private void LaunchInternalCore(System.String Path, System.String Args, System.String Dir, ProcessCreationOptions CO , ProcessInterop_StartupInfo start)
        {
            if (CO == ProcessCreationOptions.NoOptions) { CO = (ProcessCreationOptions)0x00000400; }
            ProcessInterop.ProcessResult PR = ProcessInterop.LaunchProcess(Path, Args, (ProcessInterop_ProcessFLAGS)CO, Dir, false , start);
            ErrorCode = WindowsErrorCodes.LastErrorCode;
            result = PR;
            if (result.Success) { hasnotstartedyet = false; }
            return;
        }

        private void EnsureNotDisposed() { if (isdisposed) { throw new ObjectDisposedException(GetType().FullName ,"This instance is disposed and cannot be reused."); } }

        private void EnsureStarted() { if (hasnotstartedyet) { throw new InvalidOperationException("This instance has not been attached to a process yet."); } }

        private void EnsureNotStarted() { if (hasnotstartedyet == false && (result?.Success == true)) { throw new InvalidOperationException("This instance is already attached to a process."); } }

        /// <summary>
        /// Gets an <see cref="NativeCallErrorException"/> if the process was not executed sucessfully. 
        /// You can either throw the exception returned or do anything you need to get the error 
        /// that prevents the application from starting.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// Occurs when this instance is disposed. Disposed instances cannot 
        /// create or get processes or get their respective information.
        /// </exception>
        [CLSCompliant(false)]
        public NativeCallErrorException GetExceptionForLaunch
        {
            get
            {
                EnsureNotDisposed();
                if (ErrorCode == 0 || ExecutedSucessfully) { return null; }
                return new NativeCallErrorException(ErrorCode , $"Error detected while attempting to create the process:\n" +
                    $"{ROOT.WindowsErrorCodes.GetErrorStringFromWin32Code(ErrorCode)}");
            }
        }

        /// <summary>
        /// Determines whether two <see cref="ProcessCreator"/> instances are equal. <br />
        /// This is done by testing if the <see cref="PID"/> property is equal in both instances.
        /// </summary>
        /// <param name="other">The other instance to compare.</param>
        /// <returns><see langword="true"/> if both instances are equal; otherwise , <see langword="false"/>.</returns>
        /// <exception cref="InvalidOperationException">One of the instances compared is not bound to a process.</exception>
        /// <exception cref="ObjectDisposedException">One of the instances compared is disposed.</exception>
        public System.Boolean Equals(ProcessCreator other) { return PID == other.PID; }

        /// <summary>
        /// Gets the process execution state. <br />
        /// Use this value so as to determine if the process is running.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// Occurs when this instance is disposed. Disposed instances cannot 
        /// create or get processes or get their respective information.
        /// </exception>
        public ProcessExecutionState State
        {
            [System.Security.SuppressUnmanagedCodeSecurity]
            get {
                EnsureNotDisposed();
                if (hasnotstartedyet) { return ProcessExecutionState.NotAttached; }
                if (ExitCode == 259) { return ProcessExecutionState.Running; } else { return ProcessExecutionState.Terminated; }
            }
        }

        /// <summary>
        /// Gets the opened process information.
        /// </summary>
        /// <returns>Some of the opened process information , if are available.</returns>
        /// <exception cref="InvalidOperationException">This instance is not bound to a process.</exception>
        /// <exception cref="ObjectDisposedException">
        /// Occurs when this instance is disposed. Disposed instances cannot 
        /// create or get processes or get their respective information.
        /// </exception>
        public override string ToString()
        {
            EnsureNotDisposed();
            EnsureStarted();
            return $"{GetType().FullName}: \n" +
                $"ExecutedSucessfully={ExecutedSucessfully}\n" +
                $"PID={PID}\n" +
                $"TID={TID}\n";
        }

        /// <summary>
        /// Ensures to dispose the instance properly if an instance of this class is left orphaned.
        /// </summary>
        ~ProcessCreator() { Dispose(); }

        /// <summary>
        /// Gets a value whether the process was sucessfully launched.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// Thrown when this instance is disposed. Disposed instances cannot 
        /// create or get processes or get their respective information.
        /// </exception>
        public System.Boolean ExecutedSucessfully 
        {
            [System.Security.SecuritySafeCritical]
            get 
            { 
                EnsureNotDisposed();
                try { return result.Success; } catch (System.NullReferenceException) { return false; }
            }
        }

        /// <summary>
        /// Returns the native process handle of the spawned process.
        /// </summary>
        /// <exception cref="InvalidOperationException">This instance is not bound to a process.</exception>
        /// <exception cref="ObjectDisposedException">
        /// Thrown when this instance is disposed. Disposed instances cannot 
        /// create or get processes or get their respective information.
        /// </exception>
        public System.IntPtr NativeProcessHandle 
        {
            [System.Security.SecuritySafeCritical]
            [System.Security.SuppressUnmanagedCodeSecurity]
            get { EnsureNotDisposed(); EnsureStarted(); return result.ProcessH.ProcessHandle;  }
        }

        /// <summary>
        /// Returns the native process thread handle of the spawned process.
        /// </summary>
        /// <exception cref="InvalidOperationException">This instance is not bound to a process.</exception>
        /// <exception cref="ObjectDisposedException">
        /// Occurs when this instance is disposed. Disposed instances cannot 
        /// create or get processes or get their respective information.
        /// </exception>
        public System.IntPtr NativeThreadHandle 
        {
            [System.Security.SecuritySafeCritical]
            [System.Security.SuppressUnmanagedCodeSecurity]
            get { EnsureNotDisposed(); EnsureStarted(); return result.ProcessH.ThreadHandle; } 
        }

        /// <summary>
        /// Gets the attached process memory priority. <br />
        /// You can also set a memory priority target for the attached process by setting a value to this property.
        /// </summary>
        /// <exception cref="InvalidOperationException">This instance is not bound to a process.</exception>
        /// <exception cref="ObjectDisposedException">
        /// Occurs when this instance is disposed. Disposed instances cannot 
        /// create or get processes or get their respective information.
        /// </exception>
        /// <exception cref="NativeCallErrorException">
        /// Occurs while setting this property to an invalid value or when the native call fails.
        /// </exception>
        public ProcessMemoryPriority MemoryPriority
        {
            [System.Security.SecurityCritical]
            [System.Security.SuppressUnmanagedCodeSecurity]
            get 
            { 
                EnsureNotDisposed(); EnsureStarted();
                return (ProcessMemoryPriority)result.MemPriorityInfo.MemoryPriority;
            }
            [System.Security.SecurityCritical]
            [System.Security.SuppressUnmanagedCodeSecurity]
            set 
            {
                EnsureNotDisposed();
                EnsureStarted();
                unsafe {
                    ProcessInterop_Memory_Priority_Info temp = new()
                    {
                        MemoryPriority = (ProcessInterop_Memory_Priority_Levels)((System.Int16)value)
                    };
                    if (ProcessInterop.SetProcInfo(result.ProcessH.ProcessHandle,
                        ProcessInterop_Process_Information_Class.ProcessMemoryPriority,
                        &temp, (System.UInt32)Marshal.SizeOf(typeof(ProcessInterop_Memory_Priority_Info))) == 0)
                    {
                        WindowsErrorCodes.ThrowException(WindowsErrorCodes.LastErrorCode, 
                            $"Could not set new process memory priority value equal to {value}:");
                    }
                    result.MemPriorityInfo = temp;
                }
            }
        }

        /// <summary>
        /// Gets the memory information for this process.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// This instance is not bound to a process.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// Occurs when this instance is disposed. Disposed instances cannot 
        /// create or get processes or get their respective information.
        /// </exception>
        public ProcessMemoryInfo MemoryInfo 
        {
            [System.Security.SecuritySafeCritical]
            [System.Security.SuppressUnmanagedCodeSecurity]
            get
            {
                EnsureNotDisposed();
                EnsureStarted();
                return new ProcessMemoryInfo() 
                {
                    AvailableMemoryCommit = (System.Int64)result.MemInfo.AvailableCommit / 1024,
                    PeakPrivateCommitUsage = (System.Int64)result.MemInfo.PeakPrivateCommitUsage / 1024,
                    PrivateCommitUsage = (System.Int64)result.MemInfo.PrivateCommitUsage / 1024,
                    TotalMemoryCommitUsage = (System.Int64)result.MemInfo.TotalCommitUsage / 1024,
                }; 
            }
        }

        /// <summary>
        /// Gets the timing information for this process.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// This instance is not bound to a process.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// Occurs when this instance is disposed. Disposed instances cannot 
        /// create or get processes or get their repsective information.
        /// </exception>
        public ProcessTimesInfo TimingInfo
        {
            [System.Security.SecuritySafeCritical]
            [System.Security.SuppressUnmanagedCodeSecurity]
            get 
            {
                EnsureNotDisposed();
                EnsureStarted();
                return new()
                {
                    ExitedTime = result.Timing.ExitedTime.ToDateTimeUtc(),
                    StartedTime = result.Timing.CreatedTime.ToDateTimeUtc(),
                    KernelTime = new TimeSpan(result.Timing.KernelTime.ToTicks()),
                    UserTime = new TimeSpan(result.Timing.UserTime.ToTicks()),
                    ExecutionTimeNow = result.Timing.ExecutionTime
                };
            }
        }

        /// <summary>
        /// Unsuspends the current process created with the 
        /// <see cref="ProcessCreationOptions.SpawnSuspended"/> option.
        /// </summary>
        /// <returns><see langword="true"/> if the native function succeeded; otherwise , <see langword="false"/>.</returns>
        /// <exception cref="InvalidOperationException">
        /// This instance is not bound to a process.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// Occurs when this instance is disposed. Disposed instances cannot 
        /// create or get processes or get their repsective information.
        /// </exception>
        [System.Security.SecuritySafeCritical]
        [System.Security.SuppressUnmanagedCodeSecurity]
        public System.Boolean Unsuspend() { EnsureNotDisposed(); EnsureStarted(); return Interop.Kernel32.ResumeAppThread(result.ProcessH.ThreadHandle); }

        /// <summary>
        /// The process ID , if it is still available.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// This instance is not bound to a process.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// Occurs when this instance is disposed. Disposed instances cannot 
        /// create or get processes or get their respective information.
        /// </exception>
        public System.Int32 PID 
        {
            [System.Security.SecuritySafeCritical]
            get { EnsureNotDisposed(); EnsureStarted(); return result.ProcessH.ProcessPID; } 
        }

        /// <summary>
        /// The process thread ID , if it is still available.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// This instance is not bound to a process.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// Occurs when this instance is disposed. Disposed instances cannot 
        /// create or get processes or get their respective information.
        /// </exception>
        public System.Int32 TID 
        {
            [System.Security.SecuritySafeCritical]
            get { EnsureNotDisposed(); EnsureStarted(); return result.ProcessH.ProcessTID; } 
        }

        /// <summary>
        /// Returns the <see cref="System.IO.FileInfo"/> object used to launch the process.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// This exception will be returned when the 
        /// <see cref="GetFromCurrentProcess"/> 
        /// method was called , instead of using the <br />
        /// <see cref="Launch(ProcessCreatorData)"/> or
        /// <see cref="Launch(string, string, string)"/> methods.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// This instance is not bound to a process.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// Occurs when this instance is disposed. Disposed instances cannot 
        /// create or get processes or get their repsective information.
        /// </exception>
        public System.IO.FileInfo FileInfo 
        {
            [System.Security.SecuritySafeCritical]
            get 
            {
                EnsureNotDisposed();
                EnsureStarted();
                if (iscurrentprocess) { throw new InvalidOperationException("This operation is invalid for this instance."); } 
                return fileinfo; 
            } 
        }

        /// <summary>
        /// Returns a value whether the process launched is a critical process.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// This instance is not bound to a process.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// Occurs when this instance is disposed. Disposed instances cannot 
        /// create or get processes or get their respective information.
        /// </exception>
        public System.Boolean IsCritical { get { EnsureNotDisposed(); EnsureStarted(); return Interop.Kernel32.IsCritical(result.ProcessH.ProcessHandle); } }

        /// <summary>
        /// Terminates the current process with the specified error code which Windows will use to notify other processes.
        /// </summary>
        /// <param name="ExitCode">The exit error code to be assigned.</param>
        /// <param name="DisposeAfterTerminate">Set a value whether to dispose this instance after exiting.</param>
        /// <exception cref="InvalidOperationException">
        /// Attempting to terminate the current process handle is dangerous and therefore , prohibited. <br />
        /// Callers that want to terminate the current process can use instead the 
        /// <see cref="Environment.Exit(int)"/> <br />
        /// method, which shuts down safely the CLR and then calls the native method to terminate the process.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// This instance is not bound to a process.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// Occurs when this instance is disposed. Disposed instances cannot 
        /// create or get processes or get their respective information.
        /// </exception>
        public void Terminate(System.Boolean DisposeAfterTerminate, System.Int32 ExitCode = 0)
        {
            EnsureNotDisposed();
            EnsureStarted();
            if (iscurrentprocess) { throw new InvalidOperationException("Terminating the current process handle is " +
                "dangerous and therefore , it is prohibited."); }
            if (Interop.Kernel32.TermProc(result.ProcessH.ProcessHandle, 
                (System.UInt32)ExitCode) != 0)  { if (DisposeAfterTerminate) { Dispose(); } }
        }

        /// <summary>
        /// Suspends the thread where this method was called until the process launched
        /// terminated itself.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// This instance is not bound to a process.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// Occurs when this instance is disposed. Disposed instances cannot 
        /// create or get processes or get their respective information.
        /// </exception>
        public void WaitForExit() { EnsureNotDisposed(); EnsureStarted(); while (ExitCode == 259) { System.Threading.Thread.Sleep(50); } }

        /// <summary>
        /// The Exit code of the exited process , if any.
        /// </summary>
        /// <exception cref="NativeCallErrorException">
        /// The native method call failed.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// This instance is not bound to a process.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// Occurs when this instance is disposed. Disposed instances cannot 
        /// create or get processes or get their respective information.
        /// </exception>
        public System.UInt32 ExitCode 
        {
            [System.Security.SecurityCritical]
            [System.Security.SuppressUnmanagedCodeSecurity]
            get 
            {
                EnsureNotDisposed();
                EnsureStarted();
                System.UInt32 Code = Interop.Kernel32.GetExitCode(result.ProcessH.ProcessHandle); 
                if (Code == System.UInt32.MaxValue) 
                {
                    throw new NativeCallErrorException(Code , "Native method returned unexpected value.");
                } else { return Code; }
            }
        }

        /// <summary>
        /// Disposes the process data , if any. <br />
        /// However , after calling this method , you cannot re-use this class and you must construct a new one.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// This instance is not bound to a process.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// Occurs when this instance is disposed. Disposed instances cannot 
        /// create or get processes or get their respective information.
        /// </exception>
        [System.Security.SuppressUnmanagedCodeSecurity]
        public void Dispose() 
        {
            if (result != null)
            {
                result.ProcessH.TerminateHandles();
                result = null;
            }
            if (fileinfo != null) { fileinfo.Refresh(); fileinfo = null; }
            startupInfo = default;
            isdisposed = true;
        }

        /// <summary>
        /// Refreshes the instance state.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// This instance is not bound to a process.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// Occurs when this instance is disposed. Disposed instances cannot 
        /// create or get processes or get their respective information.
        /// </exception>
        [System.Security.SuppressUnmanagedCodeSecurity]
        public void Refresh()
        {
            EnsureNotDisposed();
            EnsureStarted();
            result.ProcessH.ProcessTID = (System.Int32)ProcessInterop.GetThreadID(result.ProcessH.ThreadHandle);
            result.MemPriorityInfo = ProcessInterop.GetMemPriorityInfo(result.ProcessH.ProcessHandle);
            result.MemInfo = ProcessInterop.GetMemoryInfo(result.ProcessH.ProcessHandle);
            result.Timing = ProcessInterop.GetProcessTimes(result.ProcessH.ProcessHandle);
            if (iscurrentprocess == false) { fileinfo.Refresh(); }
        }
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

[SpecialName]
[System.Security.SecurityCritical]
[System.Security.SuppressUnmanagedCodeSecurity]
internal static class ConsoleInterop
{
    internal static volatile System.IntPtr InputHandle;
    internal static volatile System.IntPtr OutputHandle;
    internal static System.UInt32 Win32Err;

    [DllImport(Interop.Libraries.Kernel32, EntryPoint = "FreeConsole", 
        CallingConvention = CallingConvention.Winapi , SetLastError = true)]
#if NET7_0_OR_GREATER == false
    [System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        UI = true)]
#endif
    private static extern System.Int32 DetachConsole_();

    internal static System.Int32 DetachConsole() 
    {
        System.Int32 D = DetachConsole_();
        Win32Err = ROOT.WindowsErrorCodes.LastErrorCode;
        return D;
    }

    public static System.Boolean UnallocThreads() { return Interop.Kernel32.CloseHandle(InputHandle) && Interop.Kernel32.CloseHandle(OutputHandle); }

    [DllImport(Interop.Libraries.Kernel32, EntryPoint = "GetConsoleScreenBufferInfo",
        CallingConvention = CallingConvention.Winapi, SetLastError = true)]
#if NET7_0_OR_GREATER == false
    [System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        UI = true)]
#endif
    private static extern System.Boolean GetBufferInfo_(System.IntPtr ConsoleOutputHandle,
        out ROOT.ConsoleExtensions.CONSOLE_SCREEN_BUFFER_INFO ConsoleScreenBufferInfo);

    internal static System.Boolean GetBufferInfo(System.IntPtr ConsoleOutputHandle,
        out ROOT.ConsoleExtensions.CONSOLE_SCREEN_BUFFER_INFO ConsoleScreenBufferInfo)
    {
        System.Boolean D = GetBufferInfo_(ConsoleOutputHandle, out ConsoleScreenBufferInfo);
        Win32Err = ROOT.WindowsErrorCodes.LastErrorCode;
        return D;
    }

    static ConsoleInterop()
    {
        InputHandle = System.IntPtr.Zero;
        OutputHandle = System.IntPtr.Zero;
        Win32Err = 0;
    }

    [DllImport(Interop.Libraries.Kernel32, EntryPoint = "AttachConsole", 
        CallingConvention = CallingConvention.Winapi , SetLastError = true)]
#if NET7_0_OR_GREATER == false
    [System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        UI = true)]
#endif
    private static extern System.Int32 AttachToConsole_(System.Int32 PID);

    public static System.Int32 AttachToConsole(System.Int32 PID)
    {
        System.Int32 D = AttachToConsole_(PID);
        Win32Err = ROOT.WindowsErrorCodes.LastErrorCode;
        return D;
    }

    [DllImport(Interop.Libraries.Kernel32, EntryPoint = "AllocConsole", 
        CallingConvention = CallingConvention.Winapi , SetLastError = true)]
#if NET7_0_OR_GREATER == false
    [System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        UI = true)]
#endif
    private static extern System.Int32 CreateConsole_();

    public static System.Int32 CreateConsole() 
    {
        System.Int32 D = CreateConsole_();
        Win32Err = ROOT.WindowsErrorCodes.LastErrorCode;
        return D;
    }

    [DllImport(Interop.Libraries.Kernel32, EntryPoint = "GetStdHandle", 
        CallingConvention = CallingConvention.Winapi , SetLastError = true)]
#if NET7_0_OR_GREATER == false
    [System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        UI = true)]
#endif
    private static extern System.IntPtr GetConsoleStream_(ROOT.ConsoleExtensions.ConsoleHandleOptions Stream);

    public static System.IntPtr GetConsoleStream(ROOT.ConsoleExtensions.ConsoleHandleOptions Stream)
    {
        System.IntPtr D = GetConsoleStream_(Stream);
        Win32Err = ROOT.WindowsErrorCodes.LastErrorCode;
        return D;
    }

    [DllImport(Interop.Libraries.Kernel32, EntryPoint = "WriteConsoleW", 
        CallingConvention = CallingConvention.Winapi , SetLastError = true)]
#if NET7_0_OR_GREATER == false
    [System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        UI = true)]
#endif
    internal static extern System.Int32 WriteToConsoleUnmanaged(System.IntPtr Handle,
        [MarshalAs(UnmanagedType.LPWStr)] System.String Data,
        System.Int32 NChars, [OptionalAttribute] out System.Int32 CharsWritten, System.IntPtr MustBeNull);

    [DllImport(Interop.Libraries.Kernel32, CallingConvention = CallingConvention.Winapi, EntryPoint = "GetConsoleOriginalTitleW")]
#if NET7_0_OR_GREATER == false
    [System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        UI = true)]
#endif
    internal static extern System.Int32 OriginalTitle([MarshalAs(UnmanagedType.LPTStr)] System.String Title, System.Int32 Titlesize = 27500);

    [DllImport(Interop.Libraries.Kernel32, BestFitMapping = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi, EntryPoint = "SetConsoleTitleW")]
#if NET7_0_OR_GREATER == false
    [System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        UI = true)]
#endif
    internal static extern System.Int32 SetTitle([MarshalAs(UnmanagedType.LPTStr)] System.String Title);

    [DllImport(Interop.Libraries.Kernel32, CallingConvention = CallingConvention.Winapi, EntryPoint = "GetConsoleTitleW")]
#if NET7_0_OR_GREATER == false
    [System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        UI = true)]
#endif
    internal static extern System.Int32 GetTitle([MarshalAs(UnmanagedType.LPTStr)] System.String Title, System.Int32 Titlesize = 27500);

    [DllImport(Interop.Libraries.Kernel32, CallingConvention = CallingConvention.Winapi, EntryPoint = "SetConsoleOutputCP")]
#if NET7_0_OR_GREATER == false
    [System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        UI = true)]
#endif
    internal static extern System.Int32 SetOutputEnc(System.UInt32 OutputEnc);

    [DllImport(Interop.Libraries.Kernel32, CallingConvention = CallingConvention.Winapi, EntryPoint = "SetConsoleCP")]
#if NET7_0_OR_GREATER == false
    [System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        UI = true)]
#endif
    internal static extern System.Int32 SetInputEnc(System.UInt32 InputEnc);

    [DllImport(Interop.Libraries.Kernel32, CallingConvention = CallingConvention.Winapi, EntryPoint = "GetConsoleCP")]
#if NET7_0_OR_GREATER == false
    [System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        UI = true)]
#endif
    internal static extern System.UInt32 GetInputEnc();

    [DllImport(Interop.Libraries.Kernel32, CallingConvention = CallingConvention.Winapi, EntryPoint = "GetConsoleOutputCP")]
#if NET7_0_OR_GREATER == false
    [System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        UI = true)]
#endif
    internal static extern System.UInt32 GetOutputEnc();

    [DllImport(Interop.Libraries.Kernel32, EntryPoint = "ReadConsoleW", CallingConvention = CallingConvention.Winapi)]
#if NET7_0_OR_GREATER == false
    [System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        UI = true)]
#endif
    internal static extern System.Int32 ReadFromConsoleUnmanaged(System.IntPtr Handle,
        System.Byte[] Buffer, System.Int32 NumberOfCharsToRead,
        out System.UInt32 NumberOfCharsRead, System.IntPtr MustBeNull);

#if NET7_0_OR_GREATER == false
    [System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        UI = true)]
#endif
    private static void SetOutputHandle() { OutputHandle = GetConsoleStream(ROOT.ConsoleExtensions.ConsoleHandleOptions.Output); }

#if NET7_0_OR_GREATER == false
    [System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        UI = true)]
#endif
    private static void SetInputHandle() { InputHandle = GetConsoleStream(ROOT.ConsoleExtensions.ConsoleHandleOptions.Input); }

    [DllImport(Interop.Libraries.Kernel32, CallingConvention = CallingConvention.Winapi, EntryPoint = "SetConsoleTextAttribute")]
#if NET7_0_OR_GREATER == false
    [System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        UI = true)]
#endif
    internal static extern void DefineNewAttributes(System.IntPtr Handle, System.Int16 Attributes);

#if NET7_0_OR_GREATER == false
    [System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        UI = true)]
#endif
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

#if NET7_0_OR_GREATER == false
    [System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        UI = true)]
#endif
    public static System.String ReadFromConsole(ROOT.ConsoleExtensions.ConsoleReadBufferOptions BufSize)
    {
        if (ROOT.ConsoleExtensions.Detached == true) { return null; }
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

[SpecialName]
[System.Security.SecurityCritical]
[System.Security.SuppressUnmanagedCodeSecurity]
internal static class FileInterop
{
    [DllImport(Interop.Libraries.Kernel32, 
        CallingConvention = CallingConvention.Winapi,
        EntryPoint = "GetFullPathNameW" , 
        SetLastError = true , CharSet = CharSet.Unicode)]
#if NET7_0_OR_GREATER == false
    [System.Security.Permissions.HostProtection(System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        MayLeakOnAbort = true)]
#endif
    public static extern System.Int32 GetFullPath(
        [In][MarshalAs(UnmanagedType.LPWStr)] System.String InputFileName,
        [In] System.Int32 InputFileNameLength,
        [Out][MarshalAs(UnmanagedType.LPWStr)] System.String ResultingPath,
        [Out][MarshalAs(UnmanagedType.LPWStr)] System.String DirOrFile);

    [DllImport(Interop.Libraries.Kernel32, CallingConvention = CallingConvention.Winapi,
        EntryPoint = "CreateDirectoryW", CharSet = CharSet.Unicode ,
        SetLastError = true)]
#if NET7_0_OR_GREATER == false
    [System.Security.Permissions.HostProtection(System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        MayLeakOnAbort = true, SecurityInfrastructure = true)]
#endif
    public static extern System.Int32 CreateDir([In] System.String Path, [Out][Optional] FileInterop_SECURITY_ATTRIBUTES Desc);

    [DllImport(Interop.Libraries.Kernel32, CallingConvention = CallingConvention.Winapi,
        EntryPoint = "CopyFileW", CharSet = CharSet.Unicode , SetLastError = true)]
#if NET7_0_OR_GREATER == false
    [System.Security.Permissions.HostProtection(System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        MayLeakOnAbort = true, SecurityInfrastructure = true)]
#endif
    public static extern System.Int32 CopyFile(
        [In][MarshalAs(UnmanagedType.LPWStr)] System.String PathToExistingFile,
        [In][MarshalAs(UnmanagedType.LPWStr)] System.String PathToCopy,
        [In] System.Boolean TerminateIfExists);

    [DllImport(Interop.Libraries.Kernel32, CallingConvention = CallingConvention.Winapi,
        CharSet = CharSet.Unicode, EntryPoint = "MoveFileW" , SetLastError = true)]
#if NET7_0_OR_GREATER == false
    [System.Security.Permissions.HostProtection(System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        MayLeakOnAbort = true, SecurityInfrastructure = true)]
#endif
    public static extern System.Int32 MoveFileOrDir(
        [In][MarshalAs(UnmanagedType.LPWStr)] System.String ExisitingFileOrDir,
        [In][MarshalAs(UnmanagedType.LPWStr)] System.String NewLocation);

    [DllImport(Interop.Libraries.Kernel32, CallingConvention = CallingConvention.Winapi,
        CharSet = CharSet.Unicode, EntryPoint = "CreateHardLinkW" , SetLastError = true)]
#if NET7_0_OR_GREATER == false
    [System.Security.Permissions.HostProtection(System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        MayLeakOnAbort = true, SecurityInfrastructure = true)]
#endif
    public static extern System.Int32 CreateHardLink(
        [In][MarshalAs(UnmanagedType.LPWStr)] System.String LinkLocation,
        [In][MarshalAs(UnmanagedType.LPWStr)] System.String FileToPointTo,
        [In][Optional] FileInterop_SECURITY_ATTRIBUTES MustBeNull);

    [DllImport(Interop.Libraries.Kernel32, CallingConvention = CallingConvention.Winapi,
        CharSet = CharSet.Unicode, EntryPoint = "RemoveDirectoryW" , SetLastError = true)]
#if NET7_0_OR_GREATER == false
    [System.Security.Permissions.HostProtection(System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        MayLeakOnAbort = true, SecurityInfrastructure = true)]
#endif
    public static extern System.Int32 RemoveDir([In][MarshalAs(UnmanagedType.LPWStr)] System.String PathToDir);

    [DllImport(Interop.Libraries.Kernel32, CallingConvention = CallingConvention.Winapi,
        CharSet = CharSet.Unicode, EntryPoint = "DeleteFileW" , SetLastError = true)]
#if NET7_0_OR_GREATER == false
    [System.Security.Permissions.HostProtection(System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        MayLeakOnAbort = true, SecurityInfrastructure = true)]
#endif
    public static extern System.Int32 DeleteFile([MarshalAs(UnmanagedType.LPWStr)] System.String Path);

    [DllImport(Interop.Libraries.Kernel32, CallingConvention = CallingConvention.Winapi,
        CharSet = CharSet.Unicode, EntryPoint = "CreateSymbolicLinkW" , SetLastError = true)]
#if NET7_0_OR_GREATER == false
    [System.Security.Permissions.HostProtection(System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        MayLeakOnAbort = true, SecurityInfrastructure = true)]
#endif
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
}

[SpecialName]
[System.Security.SecurityCritical]
[System.Security.SuppressUnmanagedCodeSecurity]
internal static class ProcessInterop
{
    [DllImport(Interop.Libraries.Kernel32, CallingConvention = CallingConvention.Winapi,
        CharSet = CharSet.Unicode, EntryPoint = "CreateProcessW" , SetLastError = true)]
#if NET7_0_OR_GREATER == false
    [System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        UI = true)]
#endif
    private static extern System.Int32 NewProcess(
        [In][Optional][MarshalAs(UnmanagedType.LPWStr)] System.String ApplicationPath,
        [Optional][MarshalAs(UnmanagedType.LPWStr)] System.String CmdLine,
        [In][Optional] in FileInterop_SECURITY_ATTRIBUTES ExeAttrs,
        [In][Optional] in FileInterop_SECURITY_ATTRIBUTES ThreadAttrs,
        [In][MarshalAs(UnmanagedType.Bool)] System.Boolean InheritHandles,
        [In] ProcessInterop_ProcessFLAGS CreationFlags,
        [In][Optional][MarshalAs(UnmanagedType.LPWStr)] System.String EnvironmentBlock,
        [In][Optional][MarshalAs(UnmanagedType.LPWStr)] System.String CurrentDirectory,
        ref ProcessInterop_StartupInfo StartupInfo,
        out ProcessInterop_Process_Information Information);

    [DllImport(Interop.Libraries.Kernel32, CallingConvention = CallingConvention.Winapi,
        CharSet = CharSet.Auto, EntryPoint = "GetProcessInformation", SetLastError = true)]
#if NET7_0_OR_GREATER == false
    [System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        UI = true)]
#endif
    public static extern unsafe System.Int32 GetProcInfo(
        [In] System.IntPtr ProcessHandle,
        [In] ProcessInterop_Process_Information_Class Class,
        void* Info,
        [In] System.UInt32 InfoSize);
    // Scheme for reading arbitrary data and translate them to structures (GetProcInfo or SetProcInfo methods):
    // 1. Pass to the method the process handle (either this will be the current one or another one).
    // 2. Pass the desired value for getting specific data (Consult ProcessInterop_Process_Information_Class
    // enum for more info).
    // 3. Before the method call , create a new variable that is the structure that will recieve the arbirary data
    // and initalise it as a new instance.
    // 4. The third parameter is a void* pointer. To correctly get the data from the pointer (This is the output
    // structure) add the structure variable you just initialised to that parameter with the de-reference
    // operator (&) in front of the variable name. This will sucessfully translate the data to the structure 
    // instance you created as a variable , because every class can be expressed as a void*.
    // 5. Finally , the fourth parameter of GetProcInfo() method requires the structure size.
    // Be CAREFUL: This method is implemented in native code ,so it requires the structure size as it was in
    // native code. To do that , just make sure that you have called InteropServices.Marshal.SizeOf()
    // method with it's parameter the structure instance variable you created.

    [DllImport(Interop.Libraries.Kernel32, CallingConvention = CallingConvention.Winapi,
        CharSet = CharSet.Auto, EntryPoint = "SetProcessInformation", SetLastError = true)]
#if NET7_0_OR_GREATER == false
    [System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        UI = true)]
#endif
    public static extern unsafe System.Int32 SetProcInfo(
        [In] System.IntPtr ProcessHandle,
        [In] ProcessInterop_Process_Information_Class Class,
        void* Info,
        [In] System.UInt32 InfoSize);

    public static unsafe ProcessInterop_App_Memory_Information GetMemoryInfo(System.IntPtr ProcessHandle)
    {
        ProcessInterop_App_Memory_Information mem = new();
        if (GetProcInfo(ProcessHandle, 
            ProcessInterop_Process_Information_Class.ProcessAppMemoryInfo,
             &mem, (System.UInt32)Marshal.SizeOf(mem)) != 0) { return mem; }
        return default;
    }

    public static unsafe ProcessInterop_Memory_Priority_Info GetMemPriorityInfo(System.IntPtr ProcessHandle)
    {
        ProcessInterop_Memory_Priority_Info pri = new();
        if (GetProcInfo(ProcessHandle ,
            ProcessInterop_Process_Information_Class.ProcessMemoryPriority,
            &pri , (System.UInt32)Marshal.SizeOf(pri)) != 0) { return pri; }
        return default;
    }

    [DllImport(Interop.Libraries.Kernel32 , CallingConvention = CallingConvention.Winapi ,
        CharSet = CharSet.Unicode , EntryPoint = "GetProcessTimes", SetLastError = true)]
    private static extern Interop.BOOL GetProcTimes(
        [In] System.IntPtr Handle,
        out FileInterop_FILETIME ProcCreationTime,
        out FileInterop_FILETIME ProcExitTime,
        out FileInterop_FILETIME KernelTime,
        out FileInterop_FILETIME UserTime);

    public static ProcessTimes GetProcessTimes(System.IntPtr ProcessHandle) 
    {
        System.UInt64 Cyc;
        if (GetProcTimes(ProcessHandle,
            out FileInterop_FILETIME one,
            out FileInterop_FILETIME two,
            out FileInterop_FILETIME three,
            out FileInterop_FILETIME four) == Interop.BOOL.FALSE) { return null; }
        try { if (GetCPUCycles(ProcessHandle, out Cyc) == Interop.BOOL.FALSE) { Cyc = 0; } } catch { Cyc = 0; }
        return new()
        {
            ExitedTime = two,
            CreatedTime = one,
            KernelTime = three,
            UserTime = four,
            ExecutionTime = Cyc
        };
    }

    [DllImport(Interop.Libraries.Kernel32, CallingConvention = CallingConvention.Winapi,
        CharSet = CharSet.Auto, EntryPoint = "GetCurrentProcess", SetLastError = true)]
#if NET7_0_OR_GREATER == false
    [System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        UI = true)]
#endif
    public static extern System.IntPtr GetThisProcess();

    [DllImport(Interop.Libraries.Kernel32, CallingConvention = CallingConvention.Winapi,
        CharSet = CharSet.Auto, EntryPoint = "GetProcessId", SetLastError = true)]
#if NET7_0_OR_GREATER == false
    [System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        UI = true)]
#endif
    public static extern System.UInt32 GetProcessPID([In] System.IntPtr ProcessHandle);

    [DllImport(Interop.Libraries.Kernel32, CallingConvention = CallingConvention.Winapi,
        CharSet = CharSet.Auto, EntryPoint = "GetThreadId", SetLastError = true)]
#if NET7_0_OR_GREATER == false
    [System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        UI = true)]
#endif
    public static extern System.UInt32 GetThreadID([In] System.IntPtr ProcessHandle);

    [DllImport(Interop.Libraries.Kernel32, CallingConvention = CallingConvention.Winapi,
        CharSet = CharSet.Auto, EntryPoint = "GetCurrentThread", SetLastError = true)]
#if NET7_0_OR_GREATER == false
    [System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        UI = true)]
#endif
    public static extern System.IntPtr GetThisThread();

    [DllImport(Interop.Libraries.Kernel32 , CallingConvention = CallingConvention.Winapi ,
        CharSet = CharSet.Auto , EntryPoint = "QueryProcessCycleTime", SetLastError = true)]
    private static extern Interop.BOOL GetCPUCycles(
        [In] System.IntPtr ProcessHandle,
        [Out] out System.UInt64 Cycles);

    internal class ProcessHandleD
    {
        public volatile System.IntPtr ProcessHandle = System.IntPtr.Zero;
        public volatile System.IntPtr ThreadHandle = System.IntPtr.Zero;
        public System.Int32 ProcessPID = -201;
        public System.Int32 ProcessTID = -201;

        public System.Boolean TerminateHandles() { return Interop.Kernel32.CloseHandle(ProcessHandle) && Interop.Kernel32.CloseHandle(ThreadHandle); }
    }

    // Internal unmanaged-managed transition data class.
    // Contains useful data which are consumed in the ProcessCreator class.
    internal class ProcessResult
    {
        public ProcessInterop_Memory_Priority_Info MemPriorityInfo = new();
        public ProcessInterop_App_Memory_Information MemInfo = new();
        public ProcessHandleD ProcessH = new();
        public System.Boolean Success = false;
        public ProcessTimes Timing = new();
    }

    internal class ProcessTimes
    {
        public FileInterop_FILETIME KernelTime;
        public FileInterop_FILETIME UserTime;
        public FileInterop_FILETIME ExitedTime;
        public FileInterop_FILETIME CreatedTime;
        public System.UInt64 ExecutionTime;
    }

    public static ProcessResult LaunchProcess(System.String Path, System.String Args,
        ProcessInterop_ProcessFLAGS CFlags, System.String CurrentDirectory,
        System.Boolean HandleInherit,
        [Optional] ProcessInterop_StartupInfo StartInfo)
    {
        ProcessResult PR = new();
        PR.Success = NewProcess(Path, Args, default, default, HandleInherit, CFlags,
            default, CurrentDirectory, ref StartInfo, out ProcessInterop_Process_Information PD) != 0;
        PR.ProcessH.ProcessTID = (System.Int32)PD.dwThreadId;
        PR.ProcessH.ProcessPID = (System.Int32)PD.dwProcessId;
        PR.ProcessH.ProcessHandle = PD.hProcess;
        PR.ProcessH.ThreadHandle = PD.hThread;
        if (PR.Success)
        {
            PR.Timing = GetProcessTimes(PD.hProcess);
            PR.MemInfo = GetMemoryInfo(PD.hProcess);
            PR.MemPriorityInfo = GetMemPriorityInfo(PD.hProcess);
        }
        return PR;
    }

}

internal static partial class Interop
{
    [System.Security.SuppressUnmanagedCodeSecurity]
    internal partial class Kernel32
    {
        [DllImport(Libraries.Kernel32  , CallingConvention = CallingConvention.Winapi , 
            CharSet = CharSet.Auto , EntryPoint = "CloseHandle")]
#if NET7_0_OR_GREATER == false
        [System.Security.Permissions.HostProtection(System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        MayLeakOnAbort = true)]
#endif
        private static extern System.Int32 CloseH([In] System.IntPtr nativeHandle);

        public static System.Boolean CloseHandle(System.IntPtr nativeHandle) { return CloseH(nativeHandle) != 0; }

        [DllImport(Libraries.Kernel32, CallingConvention = CallingConvention.Winapi,
            CharSet = CharSet.Auto, EntryPoint = "ResumeThread")]
#if NET7_0_OR_GREATER == false
        [System.Security.Permissions.HostProtection(System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        MayLeakOnAbort = true)]
#endif
        private static extern System.Int32 ResumeProcTh(System.IntPtr nativeHandle);

        public static System.Boolean ResumeAppThread(System.IntPtr handle) { return ResumeProcTh(handle) != -1; }

        [DllImport(Libraries.Kernel32 , CallingConvention = CallingConvention.Winapi ,
            CharSet = CharSet.Auto , EntryPoint = "IsProcessCritical")]
#if NET7_0_OR_GREATER == false
        [System.Security.Permissions.HostProtection(System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        MayLeakOnAbort = true)]
#endif
        private static extern BOOL GetIsCrit([In] System.IntPtr ProcessHandle , [MarshalAs(UnmanagedType.Bool)] out System.Boolean IsCrit);

        public static System.Boolean IsCritical(System.IntPtr ProcessH) { if (GetIsCrit(ProcessH, out bool E) == BOOL.FALSE) { return false; } else { return E; } }

        [DllImport(Libraries.Kernel32 , CallingConvention = CallingConvention.Winapi ,
            CharSet = CharSet.Auto , EntryPoint = "TerminateProcess")]
#if NET7_0_OR_GREATER == false
        [System.Security.Permissions.HostProtection(System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        MayLeakOnAbort = true)]
#endif
        public static extern System.Int32 TermProc([In] System.IntPtr ProcessHandle, [In] System.UInt32 ExitCode = 0);

        [DllImport(Libraries.Kernel32, CallingConvention = CallingConvention.Winapi,
            CharSet = CharSet.Auto, EntryPoint = "GetExitCodeProcess")]
#if NET7_0_OR_GREATER == false
        [System.Security.Permissions.HostProtection(System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        MayLeakOnAbort = true)]
#endif
        private static extern System.Int32 GetCodeExited([In] System.IntPtr handle , out System.UInt32 ExitCode);

        public static System.UInt32 GetExitCode(System.IntPtr ProcessHandle) { if (GetCodeExited(ProcessHandle, out uint EC) != 0) { return EC; } else { return System.UInt32.MaxValue; } }
    }

    [System.Security.SecurityCritical]
    [System.Security.SuppressUnmanagedCodeSecurity]
    internal static class Shell32
    {
        [DllImport(Libraries.Shell32, EntryPoint = "ShellAboutW", CallingConvention = CallingConvention.Winapi)]
#if NET7_0_OR_GREATER == false
        [System.Security.Permissions.HostProtection(
            Action = System.Security.Permissions.SecurityAction.Assert,
            Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
            UI = true)]
#endif
        private static extern System.Int32 Shownetframeworkinfo(System.IntPtr Handle,
            [MarshalAs(UnmanagedType.LPWStr)] System.String Title,
            [MarshalAs(UnmanagedType.LPWStr)] System.String desc, System.IntPtr IHandle);


        public static System.Boolean ShowDotNetFrameworkInfo()
        {
            if (Shownetframeworkinfo(
                System.IntPtr.Zero,
                "Microsoft ® .NET Framework",
                ".NET Framework is a product of Microsoft Corporation.\n" +
                $"Common Language Runtime Version: {ROOT.MAIN.GetRuntimeVersion()} \n" +
                $"Current Machine Architecture: {ROOT.MAIN.OSProcessorArchitecture()}",
                System.IntPtr.Zero) != 0) { return true; }
            else { return false; }
        }

        public static System.Boolean ShowDotNetFrameworkInfo(System.Windows.Forms.IWin32Window hwnd)
        {
            if (Shownetframeworkinfo(
                hwnd.Handle,
                "Microsoft ® .NET Framework",
                ".NET Framework is a product of Microsoft Corporation.\n" +
                $"Common Language Runtime Version: {ROOT.MAIN.GetRuntimeVersion()} \n" +
                $"Current Machine Architecture: {ROOT.MAIN.OSProcessorArchitecture()}",
                System.IntPtr.Zero) != 0) { return true; }
            else { return false; }
        }

        [DllImport(Libraries.Shell32, CallingConvention = CallingConvention.Winapi, EntryPoint = "ShellExecuteW")]
#if NET7_0_OR_GREATER == false
        [System.Security.Permissions.HostProtection(
            Action = System.Security.Permissions.SecurityAction.Assert,
            Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
            UI = true)]
#endif
        internal static extern System.Int16 ExecuteApp(System.IntPtr winHandle,
            [MarshalAs(UnmanagedType.LPWStr)] System.String Verb,
            [MarshalAs(UnmanagedType.LPWStr)] System.String Path,
            [MarshalAs(UnmanagedType.LPWStr)] System.String Parameters,
            [MarshalAs(UnmanagedType.LPWStr)] System.String WorkDir,
            System.Int32 WinShowArgs);

        internal struct ExecuteVerbs
        {
            public const System.String RunAs = "runas";

            public const System.String Print = "print";

            public const System.String Explore = "explore";

            public const System.String Find = "find";

            public const System.String Edit = "edit";

            public const System.String Open = "open";
        }


    }
}

[Serializable]
internal enum ProcessInterop_Process_Information_Class
{
    ProcessMemoryPriority,
    ProcessMemoryExhaustionInfo,
    ProcessAppMemoryInfo,
    ProcessInPrivateInfo,
    ProcessPowerThrottling,
    ProcessReservedValue1,           // Used to be for ProcessActivityThrottlePolicyInfo
    ProcessTelemetryCoverageInfo,
    ProcessProtectionLevelInfo,
    ProcessLeapSecondInfo,
    ProcessInformationClassMax
}

[Serializable]
[StructLayout(LayoutKind.Sequential)]
internal struct ProcessInterop_App_Memory_Information
{
    public System.UInt64 AvailableCommit;
    public System.UInt64 PrivateCommitUsage;
    public System.UInt64 PeakPrivateCommitUsage;
    public System.UInt64 TotalCommitUsage;
}

[StructLayout(LayoutKind.Sequential, Pack = 16)]
internal struct FileInterop_SECURITY_ATTRIBUTES
{
    public System.UInt32 nLength;
    public System.IntPtr lpSecurityDescriptor;
    public System.Boolean bInheritHandle;
}

[SpecialName]
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 64 , Size = 500)]
internal struct ProcessInterop_StartupInfo
{
    /// <summary>Must be set to 500.</summary>
    public System.UInt32 cb;

    [MarshalAs(UnmanagedType.LPWStr)]
    public System.String lpReserved;

    [MarshalAs(UnmanagedType.LPWStr)]
    public System.String lpDesktop;

    [MarshalAs(UnmanagedType.LPWStr)]
    public System.String lpTitle;

    public System.UInt32 dwX;
    public System.UInt32 dwY;
    public System.UInt32 dwXSize;
    public System.UInt32 dwYSize;
    public System.UInt32 dwXCountChars;
    public System.UInt32 dwYCountChars;
    public System.UInt32 dwFillAttribute;
    [MarshalAs(UnmanagedType.U4)]
    public ProcessInterop_PIFLAGS dwFlags;
    public System.UInt16 wShowWindow;
    // MSVCRT reserved fields <!--
    public System.UInt32 cbReserved2;
    public unsafe System.Byte* lpReserved2;
    // -->
    public System.IntPtr hStdInput;
    public System.IntPtr hStdOutput;
    public System.IntPtr hStdError;
}

[SpecialName]
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 64)]
internal struct ProcessInterop_Process_Information
{
    public System.IntPtr hProcess;
    public System.IntPtr hThread;
    public System.UInt32 dwProcessId;
    public System.UInt32 dwThreadId;
}

[Flags]
[Serializable]
internal enum ProcessInterop_ProcessFLAGS : System.UInt32
{
    CREATE_BREAKAWAY_FROM_JOB = 0x01000000,
    CREATE_DEFAULT_ERROR_MODE = 0x04000000,
    CREATE_NEW_CONSOLE = 0x00000010,
    CREATE_NEW_PROCESS_GROUP = 0x00000200,
    CREATE_NO_WINDOW = 0x08000000,
    CREATE_PROTECTED_PROCESS = 0x00040000,
    CREATE_PRESERVE_CODE_AUTHZ_LEVEL = 0x02000000,
    CREATE_SECURE_PROCESS = 0x00400000,
    /// <summary>
    /// For 16-bit old apps , deprecated after Windows 11 , since 11 run only on 64-bit , or 32-bit at least.
    /// </summary>
    CREATE_SEPARATE_WOW_VDM = 0x00000800,
    /// <summary>
    /// For 16-bit old apps , deprecated after Windows 11 , since 11 run only on 64-bit , or 32-bit at least.
    /// </summary>
    CREATE_SHARED_WOW_VDM = 0x00001000,
    CREATE_SUSPENDED = 0x00000004,
    CREATE_UNICODE_ENVIRONMENT = 0x00000400,
    DEBUG_ONLY_THIS_PROCESS = 0x00000002,
    DEBUG_PROCESS = 0x00000001,
    DETACHED_PROCESS = 0x00000008,
    EXTENDED_STARTUPINFO_PRESENT = 0x00080000,
    INHERIT_PARENT_AFFINITY = 0x00010000
}

[Flags]
[Serializable]
internal enum ProcessInterop_PIFLAGS : System.UInt32
{
    STARTF_FORCEONFEEDBACK = 0x00000040,
    STARTF_FORCEOFFFEEDBACK = 0x00000080,
    STARTF_PREVENTPINNING = 0x00002000,
    STARTF_RUNFULLSCREEN = 0x00000020,
    STARTF_TITLEISAPPID = 0x00001000,
    STARTF_TITLEISLINKNAME = 0x00000800,
    STARTF_UNTRUSTEDSOURCE = 0x00008000,
    STARTF_USECOUNTCHARS = 0x00000008,
    STARTF_USEFILLATTRIBUTE = 0x00000010,
    STARTF_USEHOTKEY = 0x00000200,
    STARTF_USEPOSITION = 0x00000004,
    STARTF_USESHOWWINDOW = 0x00000001,
    STARTF_USESIZE = 0x00000002,
    STARTF_USESTDHANDLES = 0x00000100
}

[Flags]
[Serializable]
internal enum FileInterop_SYMLINK_FLAGS : System.UInt32
{
    SYMBOLIC_LINK_FLAG_FILE = 0x0,
    SYMBOLIC_LINK_FLAG_DIRECTORY = 0x1,
    SYMBOLIC_LINK_FLAG_ALLOW_UNPRIVILEGED_CREATE = 0x2
}

[SpecialName]
[StructLayoutAttribute(LayoutKind.Sequential)]
internal struct FileInterop_FILETIME
{
    public System.UInt32 dwLowDateTime;
    public System.UInt32 dwHighDateTime;

    // The below methods are taken from the FileTime structure of Microsoft.IO.Redist package.

    public System.Int64 ToTicks()
    {
        return (System.Int64)(((ulong)dwHighDateTime << 32) + dwLowDateTime);
    }

    public DateTime ToDateTimeUtc()
    {
        return DateTime.FromFileTimeUtc(ToTicks());
    }

    public DateTimeOffset ToDateTimeOffset()
    {
        return DateTimeOffset.FromFileTime(ToTicks());
    }
}

[SpecialName]
[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
internal struct FileInterop_WIN32_FIND_DATA_W
{
    public System.UInt32 dwFileAttributes;
    public FileInterop_FILETIME ftCreationTime;
    public FileInterop_FILETIME ftLastAccessTime;
    public FileInterop_FILETIME ftLastWriteTime;
    public System.UInt32 nFileSizeHigh;
    public System.UInt32 nFileSizeLow;
    public System.UInt32 dwReserved0;
    public System.UInt32 dwReserved1;
    [MarshalAs(UnmanagedType.ByValTStr , SizeConst = 500)]
    public System.String cFileName;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
    public System.String cAlternateFileName;
    public System.UInt32 dwFileType;
    public System.UInt32 dwCreatorType;
    public System.UInt32 wFinderFlags;
}
