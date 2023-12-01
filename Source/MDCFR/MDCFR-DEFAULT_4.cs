
// An All-In-One framework abstracting the most important classes that are used in .NET
// that are more easily and more consistently to be used.
// The framework was designed to host many different operations , with the last goal 
// to be everything accessible for everyone.

// Global namespaces
using System;
using System.Runtime.Versioning;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

#if WPFExists == false
namespace System.Windows.Forms
{
    /// <summary>
    /// Specifies constants defining which buttons to display on a System.Windows.Forms.MessageBox.
    /// </summary>
    public enum MessageBoxButtons
    {
        /// <summary>
        /// The message box contains an OK button.
        /// </summary>
        OK,
        /// <summary>
        /// The message box contains OK and Cancel buttons.
        /// </summary>
        OKCancel,
        /// <summary>
        /// The message box contains Abort, Retry, and Ignore buttons.
        /// </summary>
        AbortRetryIgnore,
        /// <summary>
        /// The message box contains Yes, No, and Cancel buttons.
        /// </summary>
        YesNoCancel,
        /// <summary>
        /// The message box contains Yes and No buttons.
        /// </summary>
        YesNo,
        /// <summary>
        /// The message box contains Retry and Cancel buttons.
        /// </summary>
        RetryCancel
    }

    /// <summary>
    ///  Specifies constants defining which information to display.
    /// </summary>
    public enum MessageBoxIcon
    {
        /// <summary>
        /// The message box contains no symbols.
        /// </summary>
        None = 0,
        /// <summary>
        /// The message box contains a symbol consisting of a white X in a circle with a red background.
        /// </summary>
        Hand = 16,
        /// <summary>
        /// The message box contains a symbol consisting of a question mark in a circle.
        ///     The question mark message icon is no longer recommended because it does not clearly
        ///     represent a specific type of message and because the phrasing of a message as
        ///     a question could apply to any message type. In addition, users can confuse the
        ///     question mark symbol with a help information symbol. Therefore, do not use this
        ///     question mark symbol in your message boxes. The system continues to support its
        ///     inclusion only for backward compatibility.
        /// </summary>
        Question = 32,
        /// <summary>
        /// The message box contains a symbol consisting of an exclamation point in a triangle with a yellow background.
        /// </summary>
        Exclamation = 48,
        /// <summary>
        /// The message box contains a symbol consisting of a lowercase letter i in a circle.
        /// </summary>
        Asterisk = 64,
        /// <summary>
        /// The message box contains a symbol consisting of white X in a circle with a red background.
        /// </summary>
        Stop = 16,
        /// <summary>
        /// The message box contains a symbol consisting of white X in a circle with a red background.
        /// </summary>
        Error = 16,
        /// <summary>
        /// The message box contains a symbol consisting of an exclamation point in a triangle with a yellow background.
        /// </summary>
        Warning = 48,
        /// <summary>
        /// The message box contains a symbol consisting of a lowercase letter i in a circle.
        /// </summary>
        Information = 64
    }

    /// <summary>
    /// Provides an interface to expose Win32 HWND handles.
    /// </summary>
    [Guid("458AB8A2-A1EA-4d7b-8EBE-DEE5D3D9442C")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComVisible(true)]
    public interface IWin32Window
    {
        /// <summary>
        /// Gets the handle to the window represented by the implementer.
        /// </summary>
        /// <returns>A handle to the window represented by the implementer.</returns>
        IntPtr Handle { get; }
    }
}
#endif

namespace ROOT
{
    internal readonly struct HW31Mapper
    {
        // Kept for backwards compatibility.
        /*
		public static readonly System.Object[,] Mapper = { { 0 , "AA" } , { 1 , "AB" } , { 2 , "AC" } , { 3 , "AD" } , { 4 , "AE" } , { 5 , "AF" } ,
		{ 6 , "AG" } , { 7 , "AH" } , { 8 , "AI" } , { 9 , "AJ" } , {10 , "AK" } , {11 , "AL" } , {12 , "AM" } , { 13 , "AN" } , { 14 , "AO" } ,
		{ 15 , "AP" } , { 16 , "AQ" } , { 17 , "AR" } , { 18 , "AS" } , { 19 , "AT" } , { 20 , "AU" } , { 21 , "AV" } , { 22 , "AW" } , { 23 , "AX" },
		{ 24 , "AY" } , { 25 , "AZ" } , { 26 , "Aa" } , { 27 , "Ab" } , { 28 , "Ac" } , { 29 , "Ad" } , { 30 , "Ae" } , { 31 , "Af" } , { 32 , "Ag" },
		{ 33 , "Ah" } , { 34 , "Ai" } , { 35 , "Aj" } , { 36 , "Ak" } , { 37  , "Al" } , { 38 , "Am" } , { 39 , "An" } , { 40 , "Ao" } , { 41 , "Ap" },
		{ 42 , "Aq" } , { 43 , "Ar" } , { 44 , "As" } , { 45 , "At" } , { 46 , "Au" } , { 47 , "Av" } , { 48 , "Aw" } , { 49 , "Ax" } , { 50 , "Ay" },
		{ 51 , "Az" } , { 52 , "aA" } , { 53 , "aB" } , { 54 , "aC" } , { 55 , "aD" } , { 56 , "aE" } , { 57 , "aF" } , { 58 , "aG" } , { 59 , "aH" },
		{ 60 , "aI" } , { 61 , "aJ" } , { 62 , "aK" } , { 63 , "aL" } , { 64 , "aM" } , { 65 , "aN" } , { 66 , "aO" } , { 67 , "aP" } , { 68 , "aQ" },
		{ 69 , "aR" } , { 70 , "aS" } , { 71 , "aT" } , { 72 , "aU" } , { 73 , "aV" } , { 74 , "aW" } , { 75 , "aX" } , { 76 , "aY" } , { 77 , "aZ" },
		{ 78 , "aa" } , { 79 , "ab" } , { 80 , "ac" } , { 81 , "ad" } , { 82 , "ae" } , { 83 , "af" } , { 84 , "ag" } , { 85 , "ah" } , { 86 , "ai" },
		{ 87 , "aj" } , { 88 , "ak" } , { 89 , "al" } , { 90 , "am" } , { 91 , "an" } , { 92 , "ao" } , { 93 , "ap" } , { 94 , "aq" } , { 95 , "ar" },
		{ 96 , "as" } , { 97 , "at" } , { 98 , "au" } , { 99 , "av" } , { 100 , "aw" } , { 101 , "ax" } , { 102 , "ay" } , { 103 , "az" } ,
		{ 104 , "BA" } , { 105 , "BB" } , { 106 , "BC" } , { 107 , "BD" } , { 108 , "BE" } , { 109 , "BF" } , { 110 , "BG" } , { 111 , "BH" },
		{ 112 , "BI" } , { 113 , "BJ" } , { 114 , "BK" } , { 115 , "BL" } , { 116 , "BM" } , { 117 , "BO" } , { 118 , "BP" },
		{ 119 , "BQ" } , { 120 , "BR" } , { 121 , "BS" } , { 122 , "BT" } , { 123 , "BU" } , { 124 , "BV" } , { 125 , "BW" } , { 126 , "BX" },
		{ 127 , "BY" } , { 128 , "BZ" } , { 129 , "Ba" } , { 130 , "Bb" } , { 131 , "Bc" } , { 132 , "Bd" } , { 133 , "Be" } , { 134 , "Bf" },
		{ 135 , "Bg" } , { 136 , "Bh" } , { 137 , "Bi" } , { 138 , "Bj" } , { 139 , "Bk" } , { 140 , "Bl" } , { 141 , "Bm" } , { 142 , "Bn" } ,
		{ 143 , "Bo" } , { 144 , "Bp" } , { 145 , "Bq" } , { 146 , "Br" } , { 147 , "Bs" } , { 148 , "Bt" } , { 149 , "Bu" } , { 150 , "Bv" },
		{ 151 , "Bw" } , { 152 , "Bx" } , { 153 , "By" } , { 154 , "Bz" } , { 155 , "bA" } , { 156 , "bB" } , { 157 , "bC" } , { 158 , "bD" },
		{ 159 , "bE" } , { 160 , "bF" } , { 161 , "bG" } , { 162 , "bH" } , { 163 , "bI" } , { 164 , "bJ" } , { 165 , "bK" } , { 166 , "bL" },
		{ 167 , "bM" } , { 168 , "bN" } , { 169 , "bP" } , { 170 , "bQ" } , { 171 , "bR" } , { 172 , "bS" } , { 173 , "bT" },
		{ 174 , "bU" } , { 175 , "bV" } , { 176 , "bW" } , { 177 , "bX" } , { 178 , "bY" } , { 179 , "bZ" } , { 180 , "ba" } , { 181 , "bb" },
		{ 182 , "bc" } , { 183 , "bd" } , { 184 , "be" } , { 185 , "bf" } , { 186 , "bg" } , { 187 , "bh" } , { 188 , "bi" } , { 189 , "bj" },
		{ 190 , "bk" } , { 191 , "bl" } , { 192 , "bm" } , { 193 , "bn" } , { 194 , "bo" } , { 195 , "bp" } , { 196 , "bq" } , { 197 , "br" },
		{ 198 , "bs" } , { 199 , "bt" } , { 200 , "bu" } , { 201 , "bv" } , { 202 , "bw" } , { 203 , "bx" } , { 204 , "by" } , { 205 , "bz" },
		{ 206 , "CA" } , { 207 , "CB" } , { 208 , "CC" } , { 209 , "CD" } , { 210 , "CE" } , { 211 , "CF" } , { 212 , "CG" } , { 213 , "CH" },
		{ 214 , "CI" } , { 215 , "CJ" } , { 216 , "CK" } , { 217 , "CL" } , { 218 , "CM" } , { 219 , "CN" } , { 220 , "CO" } , { 221 , "CP" },
		{ 222 , "CQ" } , { 223 , "CR" } , { 224 , "CS" } , { 225 , "CT" } , { 226 , "CU" } , { 227 , "CV" } , { 228 , "CW" } , { 229 , "CX" },
		{ 230 , "CY" } , { 231 , "CZ" } ,  { 232 , "Ca" } , { 233 , "Cb" } , { 234 , "Cc" } , { 235 , "Cd" } , { 236 , "Cf" } , { 237 , "Cg" },
		{ 238 , "Ch" } , { 239 , "Ci" } , { 240 , "Cj" } , { 241 , "Ck" } , { 242 , "Cl" } , { 243 , "Cm" } , { 244 , "Cn" } , { 245 , "Co" },
		{ 246 , "Cp" } , { 247 , "Cq" } , { 248 , "Cr" } , { 249 , "Cs" } , { 250 , "Ct" } , { 251 , "Cu" } , { 252 , "Cv" } , { 253 , "Cw" },
		{ 254 , "Cx" } , { 255 , "Cy" } };
		*/

        // This field keeps all the valid byte numbers , range 0..255 . ,256 bytes in total.
        public static readonly System.Int32[] nums = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22,
            23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54,
            55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86,
            87, 88, 89, 90, 91, 92, 93, 94, 95, 96, 97, 98, 99, 100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113,
            114, 115, 116, 117, 118, 119, 120, 121, 122, 123, 124, 125, 126, 127, 128, 129, 130, 131, 132, 133, 134, 135, 136, 137,
            138, 139, 140, 141, 142, 143, 144, 145, 146, 147, 148, 149, 150, 151, 152, 153, 154, 155, 156, 157, 158, 159, 160, 161,
            162, 163, 164, 165, 166, 167, 168, 169, 170, 171, 172, 173, 174, 175, 176, 177, 178, 179, 180, 181, 182, 183, 184, 185,
            186, 187, 188, 189, 190, 191, 192, 193, 194, 195, 196, 197, 198, 199, 200, 201, 202, 203, 204, 205, 206, 207, 208, 209,
            210, 211, 212, 213, 214, 215, 216, 217, 218, 219, 220, 221, 222, 223, 224 , 225 , 226 , 227 , 228 ,229, 230, 231, 232,
            233, 234, 235, 236, 237, 238, 239, 240, 241, 242, 243, 244, 245, 246, 247, 248, 249, 250, 251, 252, 253, 254 , 255 };

        // This field is the matching case of every single byte of the above field. Each single byte is assigned in a single
        // unique string.
        public static readonly System.String[] chars = { "AA", "AB", "AC", "AD", "AE", "AF", "AG", "AH", "AI", "AJ", "AK", "AL",
            "AM", "AN", "AO", "AP", "AQ", "AR", "AS", "AT", "AU", "AV", "AW", "AX", "AY", "AZ", "Aa", "Ab", "Ac", "Ad", "Ae", "Af", "Ag",
            "Ah", "Ai", "Aj", "Ak", "Al", "Am", "An", "Ao", "Ap", "Aq", "Ar", "As", "At", "Au", "Av", "Aw", "Ax", "Ay", "Az", "aA", "aB",
            "aC", "aD", "aE", "aF", "aG", "aH", "aI", "aJ", "aK", "aL", "aM", "aN", "aO", "aP", "aQ", "aR", "aS", "aT", "aU", "aV", "aW",
            "aX", "aY", "aZ", "aa", "ab", "ac", "ad", "ae", "af", "ag", "ah", "ai", "aj", "ak", "al", "am", "an", "ao", "ap", "aq", "ar",
            "as", "at", "au", "av", "aw", "ax", "ay", "az", "BA", "BB", "BC", "BD", "BE", "BF", "BG", "BH", "BI", "BJ", "BK", "BL", "BM",
            "BO", "BP", "BQ", "BR", "BS", "BT", "BU", "BV", "BW", "BX", "BY", "BZ", "Ba", "Bb", "Bc", "Bd", "Be", "Bf", "Bg", "Bh", "Bi",
            "Bj", "Bk", "Bl", "Bm", "Bn", "Bo", "Bp", "Bq", "Br", "Bs", "Bt", "Bu", "Bv", "Bw", "Bx", "By", "Bz", "bA", "bB", "bC", "bD",
            "bE", "bF", "bG", "bH", "bI", "bJ", "bK", "bL", "bM", "bN", "bP", "bQ", "bR", "bS", "bT", "bU", "bV", "bW", "bX", "bY", "bZ",
            "ba", "bb", "bc", "bd", "be", "bf", "bg", "bh", "bi", "bj", "bk", "bl", "bm", "bn", "bo", "bp", "bq", "br", "bs", "bt", "bu",
            "bv", "bw", "bx", "by", "bz", "CA", "CB", "CC", "CD", "CE", "CF", "CG", "CH", "CI", "CJ", "CK", "CL", "CM", "CN", "CO", "CP",
            "CQ", "CR", "CS", "CT", "CU", "CV", "CW", "CX", "CY", "CZ", "Ca", "Cb", "Cc", "Cd", "Cf", "Cg", "Ch", "Ci", "Cj", "Ck", "Cl",
            "Cm", "Cn", "Co", "Cp", "Cq", "Cr", "Cs", "Ct", "Cu", "Cv", "Cw", "Cx", "Cy" };
    }

    /// <summary>
    /// A static class which constructs HW31 strings from <see cref="System.Byte"/>[] arrays.
    /// </summary>
    public static class HW31Strings
    {
        /*
		 * Where this class is useful? 
		 * > The class , like Base64 , creates a string representation of the byte array given.
		 *    However , Base64 and HW31 have differences:
		 *    HW31 allocates two unique characters representing each byte; Base64 creates the next character based on the last one and the next byte value.
		 *    Base64 leaves null characters at the end of the string(Interpreted as '=') , while HW31 leaves always only a space in the end of the string.
		 * > HW31 will also always produce the same result , no matter how, except in the case of corrupt string , which even then will return zero's.
		 * > HW31 has a dictionary which allows it to pick the appropriate byte each time and translate it into a HW31.
		 * Is it reliable to save byte data on an HW31 string?
		 * It depends on what you will use it. For data encryption keys , better is the Base64;
		 * For small binary data , HW31 will do the work fine.
		 * HW31 could be also used in small data dictionaries , where data precision is required.
		 */

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        private static System.String ByteToCorrespondingChars(System.Byte Value)
        {
            for (System.Int32 I = 0; I < HW31Mapper.nums.Length; I++) { if (HW31Mapper.nums[I] == Value) { return $"{HW31Mapper.chars[I]}"; } }
            return "Error";
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        private static System.Byte CharsToCorrespondingByte(System.String Chars)
        {
            if (Chars.Length != 2) { return 0; }
            for (System.Int32 I = 0; I < HW31Mapper.chars.Length; I++) { if (HW31Mapper.chars[I] == Chars) { return (System.Byte)HW31Mapper.nums[I]; } }
            return 0;
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design",
            "CA1031:Do not catch general exception types",
            Justification = "Used to catch several exceptions at once.")]
        private static System.Boolean TestIfItIsAnHW31String(System.String HW31)
        {
            if (HW31 == null) { return false; }
            if (HW31.Length < 3) { return false; }
            System.Char[] HW31Arr = HW31.ToCharArray();
            if (HW31Arr[2] != ' ') { return false; }
            if (HW31Arr[HW31Arr.Length - 1] != ' ') { return false; }
            for (System.Int32 I = 0; I < HW31Arr.Length; I++)
            {
                try
                {
                    if (IsDigit(System.Convert.ToInt32(HW31Arr[I])) == true) { return false; }
                }
                catch { continue; }
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsDigit(System.Int32 I) { return (System.UInt32)(I - '0') <= ('9' - '0'); }

        /// <summary>
        /// Converts a <see cref="System.Byte"/>[] array to a new HW31 <see cref="System.String"/>. 
        /// </summary>
        /// <param name="Array">The Byte array to get the data from.</param>
        /// <returns>A new HW31 <see cref="System.String"/> . </returns>
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static HW31 ByteArrayToHW31String(System.Byte[] Array)
        {
            HW31 DC = new HW31();
            if (Array.Length < 1) { DC.SetOrGetError = true; return DC; }
            System.String Result = null;
            System.String tmp = "";
            for (System.Int32 I = 0; I < Array.Length; I++)
            {
                tmp = ByteToCorrespondingChars((System.Byte)HW31Mapper.nums[Array[I]]);
                if (tmp != "Error") { Result += (tmp + " "); }
                else { DC.SetOrGetError = true; return DC; }
            }
            DC = new HW31(Result);
            return DC;
        }

        /// <summary>
        /// Converts a <see cref="System.Byte"/>[] array to a new HW31 <see langword="struct"/>. 
        /// </summary>
        /// <param name="Array">The Byte array to get the data from.</param>
        /// <param name="Count">How many iterations will happen to the array.</param>
        /// <param name="Start">From which point the iterator will start calculating.</param>
        /// <returns>A new HW31 <see langword="struct"/> .</returns>
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static HW31 ByteArrayToHW31String(System.Byte[] Array, System.Int32 Start, System.Int32 Count)
        {
            HW31 DC = new HW31();
            if (Array.Length < 1) { DC.SetOrGetError = true; return DC; }
            if (Start < 0) { DC.SetOrGetError = true; return DC; }
            if (Start > Count) { DC.SetOrGetError = true; return DC; }
            if (Count < 1) { DC.SetOrGetError = true; return DC; }
            System.String Result = null;
            System.String tmp = "";
            for (System.Int32 I = Start; I < Count; I++)
            {
                tmp = ByteToCorrespondingChars((System.Byte)HW31Mapper.nums[Array[I]]);
                if (tmp != "Error") { Result += (tmp + " "); }
                else { DC.SetOrGetError = true; return DC; }
            }
            DC = new HW31(Result);
            return DC;
        }

        /// <summary>
        /// Calculates the length of the HW31 string before it is created.
        /// </summary>
        /// <param name="Array">The <see cref="System.Byte"/>[] to calculate the data from.</param>
        /// <returns>The estimated HW31 <see langword="struct"/> containing length.</returns>
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static System.Int64 EstimateHW31StringLength([System.Diagnostics.CodeAnalysis.DisallowNull] System.Byte[] Array)
        { if (Array == null) { return -1; } return Array.Length * 3; }

        /// <summary>
        /// Converts a created HW31 <see cref="System.String"/> back to a <see cref="System.Byte"/>[] array.
        /// </summary>
        /// <param name="HW31String">The already created HW31 <see cref="System.String"/>. </param>
        /// <returns>A new <see cref="System.Byte"/>[] containing the byte data kept by the HW31 string.</returns>
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static System.Byte[] HW31StringToByteArray(HW31 HW31String)
        {
            if (TestIfItIsAnHW31String(HW31String.ReturnHW31) == false) { return null; }
            System.Char[] HW31Arr = HW31String.ReturnHW31.ToCharArray();
            if (HW31Arr[HW31Arr.Length - 1] != ' ') { return null; }
            System.Int32 BlanksToRemove = 0;
            for (System.Int32 I = 0; I < HW31Arr.Length; I++) { if (HW31Arr[I] == ' ') { BlanksToRemove += 2; } }
            System.Byte[] Result = new System.Byte[HW31Arr.Length - BlanksToRemove];
            System.String _Tmp = null;
            System.Int32 Count = 0;
            System.Int32 ArrCount = 0;
            try
            {
                for (System.Int32 I = 0; I < HW31Arr.Length; I++)
                {
                    if (HW31Arr[I] != ' ') { _Tmp += HW31Arr[I]; Count++; }
                    if (Count >= 2) { Result[ArrCount] = CharsToCorrespondingByte(_Tmp); Count = 0; ArrCount++; _Tmp = null; }
                }
            }
            catch (System.Exception) { return null; }
            return Result;
        }

    }

    /// <summary>
    /// The HW31 structure. HW31 is an intermediate storage to store binary data to <see cref="System.String"/>'s and the opposite.
    /// </summary>
#nullable enable
    [Serializable]
    public struct HW31 : IEquatable<HW31?>, IEquatable<HW31>
    {
        private System.String BackField;
        private System.Boolean Erro_r = false;

        internal System.String ReturnHW31 { get { return BackField; } }
        internal System.Boolean SetOrGetError { get { return Erro_r; } set { Erro_r = value; } }

        /// <summary>
        /// Returns a <see cref="System.Boolean"/> value , indicating that this HW31 is invalid and should be destroyed.
        /// </summary>
        /// <returns><c>true</c> if this structure is invalid; otherwise , <c>false</c> if it is usuable.</returns>
        public System.Boolean IsInvalid() { return Erro_r; }

        /// <summary>
        /// Create a new HW31 structure.
        /// </summary>
        /// <param name="HW31">The HW31 <see cref="System.String"/> to create from.</param>
        /// <exception cref="System.InvalidOperationException">
        /// The <see cref="System.String"/> attempted to set was not in the HW31 format.</exception>
        public HW31(System.String HW31)
        {
            if (TestIfItIsAnHW31String(HW31) == false)
            {
                throw new System.InvalidOperationException("Invalid attempt to set a string which is not an HW31 one.");
            }

            BackField = HW31;
        }

        /// <summary>Initialises a new instance of the <see cref="HW31"/> structure.</summary>
        public HW31() { BackField = ""; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design",
           "CA1031:Do not catch general exception types",
           Justification = "Used to catch several exceptions at once.")]
        private static System.Boolean TestIfItIsAnHW31String(System.String HW31)
        {
            if (HW31 == null) { return false; }
            if (HW31.Length < 3) { return false; }
            System.Char[] HW31Arr = HW31.ToCharArray();
            if (HW31Arr[2] != ' ') { return false; }
            if (HW31Arr[HW31Arr.Length - 1] != ' ') { return false; }
            for (System.Int32 I = 0; I < HW31Arr.Length; I++)
            {
                try
                {
                    if (IsDigit(System.Convert.ToInt32(HW31Arr[I])) == true) { return false; }
                }
                catch { continue; }
            }
            return true;
        }

        /// <summary>
        /// Detects if the specified <see cref="System.String"/> is an HW31 <see cref="System.String"/>.
        /// </summary>
        /// <param name="HW31">The HW31 <see cref="System.String"/> to test.</param>
        /// <returns><c>true</c> if the <paramref name="HW31"/> 
        /// can be an HW31 <see langword="struct"/>; otherwise , <c>false</c>.</returns>
        public static System.Boolean IsHW31(System.String HW31) { return TestIfItIsAnHW31String(HW31); }

        /// <summary>
        /// Test if an <see cref="HW31"/> structure that holds the HW31 <see cref="System.String"/> 
        /// is equal to a non-structured HW31 <see cref="System.String"/>.
        /// </summary>
        /// <param name="left">The <see cref="HW31"/> structure to take the <see cref="System.String"/> from.</param>
        /// <param name="right">The non-structured <see cref="System.String"/> to compare against.</param>
        /// <returns><c>true</c> if these two objects specified are equal; otherwise ,  <c>false</c>.</returns>
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static System.Boolean operator ==(HW31 left, System.String right) { return left.ReturnHW31.Equals(right); }

        /// <summary>
        /// Test if an <see cref="HW31"/> structure that holds the HW31 <see cref="System.String"/> 
        /// is NOT equal to a non-structured HW31 <see cref="System.String"/>.
        /// </summary>
        /// <param name="left">The <see cref="HW31"/> structure to take the <see cref="System.String"/> from.</param>
        /// <param name="right">The non-structured <see cref="System.String"/> to compare against.</param>
        /// <returns><c>true</c> if these two objects specified are NOT equal; otherwise ,  <c>false</c>.</returns>
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static System.Boolean operator !=(HW31 left, System.String right) { return left.ReturnHW31.Equals(right) == false; }

        /// <summary>
        /// Test if an <see cref="HW31"/> structure is equal to another.
        /// </summary>
        /// <param name="lhs">The first structure.</param>
        /// <param name="rhs">The second structure.</param>
        /// <returns><c>true</c> if the structures are equal; otherwise , <c>false</c>.</returns>
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static System.Boolean operator ==(HW31 lhs, HW31 rhs) { return lhs.Equals(rhs); }

        /// <summary>
        /// Test if an <see cref="HW31"/> structure is NOT equal to another.
        /// </summary>
        /// <param name="lhs">The first structure.</param>
        /// <param name="rhs">The second structure.</param>
        /// <returns><c>true</c> if the structures are NOT equal; otherwise , false.</returns>
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static System.Boolean operator !=(HW31 lhs, HW31 rhs) { return lhs.Equals(rhs) == false; }

        /// <summary>
        /// Test if an generic object is equal to this HW31 instance structure.
        /// </summary>
        /// <param name="obj">The generic object to compare.</param>
        /// <returns><c>true</c> if this structure is equal to the <paramref name="obj"/>;
        /// otherwise , <c>false</c>.</returns>
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public override System.Boolean Equals(object? obj) { return HW31.Equals(this, obj); }

        /// <summary>
        /// Test if another nullable HW31 construct is equal to this HW31 instance structure.
        /// </summary>
        /// <param name="other">The nullable HW31 construct to compare.</param>
        /// <returns><c>true</c> if this structure is equal to the <paramref name="other"/>;
        /// otherwise , <c>false</c>.</returns>
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public System.Boolean Equals(HW31? other) { if (other?.Equals(this) == true) { return true; } else { return false; } }

        /// <summary>
        /// Test if another HW31 construct is equal to this HW31 instance structure.
        /// </summary>
        /// <param name="other">The nullable HW31 construct to compare.</param>
        /// <returns><c>true</c> if this structure is equal to the <paramref name="other"/>;
        /// otherwise , <c>false</c>.</returns>
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public System.Boolean Equals(HW31 other) { if (other.Equals(this) == true) { return true; } else { return false; } }

        /// <inheritdoc />
        public override System.Int32 GetHashCode() { return BackField.GetHashCode(); }

        /// <summary>
        /// Gets the length of the HW31 <see cref="System.String"/>.
        /// </summary>
        /// <returns>The computed length.</returns>
        public System.Int32 Length() { return BackField.Length; }

        /// <summary>
        /// Gets the length of the HW31 <see cref="System.String"/> , but only the real interpreted characters.
        /// </summary>
        /// <returns>The computed length.</returns>
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public System.Int32 ClearLength()
        {
            System.Char[] Chars = BackField.ToCharArray();
            System.Int32 Remove = 0;
            for (System.Int32 I = 0; I < Chars.Length; I++) { if (Chars[I] == ' ') { Remove++; } }
            return Chars.Length - Remove;
        }

        // The below code is residing in the System.Buffers.Text namespace , which is a method for the internal mechanisms.
        // Here it is used for checking if the HW31 has digits , which is illegal.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsDigit(System.Int32 I) { return (System.UInt32)(I - '0') <= ('9' - '0'); }

        /// <summary>
        /// Returns the real HW31 <see cref="System.String"/> created.
        /// </summary>
        /// <returns>The HW31 <see cref="System.String"/> .</returns>
        public override System.String ToString()
        {
            if (Erro_r == true)
            {
                throw new InvalidOperationException("Cannot use this HW31 instance " +
                "because this structure is marked as invalid.");
            }
            return BackField.ToString();
        }

    }
#nullable disable

    /// <summary>
    /// The Registry Types that the user can use to set the data to a value.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming",
        "CA1720:Identifier contains type name",
        Justification = "String is also a Windows Registry type")]
    public enum RegTypes
    {
        /// <summary>
        /// Reserved property. Should not be used directly by your source code.
        /// </summary>
        ERROR = 0,
        /// <summary> RSVD </summary>
        RSVD0 = 1,
        /// <summary> RSVD </summary>
        RSVD1 = 2,
        /// <summary> RSVD </summary>
        RSVD2 = 3,
        /// <summary> RSVD </summary>
        RSVD3 = 4,
        /// <summary> RSVD </summary>
        RSVD4 = 5,
        /// <summary>
        /// The Registry type will be a string value.
        /// </summary>
        String = 6,
        /// <summary>
        /// The Registry type will be an environment variable string value.
        /// </summary>
        ExpandString = 7,
        /// <summary>
        /// The Registry type will be an quad-word byte array value.
        /// </summary>
        QuadWord = 8,
        /// <summary>
        /// The Registry type will be an double-word byte array value.
        /// </summary>
        DoubleWord = 9
    }

    /// <summary>
    /// The <see cref="RegEditor"/> instance class functions result after executing.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming",
        "CA1707:Identifiers should not contain underscores",
        Justification = "Removing underscores from a globally used field could be an breaking API change.")]
    public enum RegFunctionResult
    {
        /// <summary>
        /// Generic error.
        /// </summary>
        Error = 0,
        /// <summary> RSVD </summary>
        RSVD0 = 1,
        /// <summary> RSVD </summary>
        RSVD1 = 2,
        /// <summary>
        /// Incorrect Registry Path.
        /// </summary>
        /// <remarks>This is mostly returned when the path is incorrect or it is an registry error.</remarks>
        Misdefinition_Error = 3,
        /// <summary>
        /// The Root Key provided is invalid.
        /// </summary>
        InvalidRootKey = 4,
        /// <summary>
        /// Sucessfull execution.
        /// </summary>
        Success = 5
    }

    /// <summary>
    /// Valid root paths for the <see cref="RegEditor"/> to modify or create new values.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design",
        "CA1027:Mark enums with FlagsAttribute",
        Justification = "This enumeration represents fixed strings , not a set of bit values.")]
    public enum RegRootKeyValues
    {
        /// <summary>
        /// Reserved property for indicating a custom or unusuable root key.
        /// </summary>
        Inabsolute = 0,
        /// <summary> RSVD </summary>
        RSVD0 = 1,
        /// <summary> RSVD </summary>
        RSVD1 = 2,
        /// <summary> HKLM Path. </summary>
        HKLM = 3,
        /// <summary> HKCU Path. </summary>
        HKCU = 4,
        /// <summary> HKCC Path. </summary>
        HKCC = 5,
        /// <summary> HKPD Path. </summary>
        HKPD = 6,
        /// <summary> HKU Path. </summary>
        HKU = 7,
        /// <summary> HKCR Path. </summary>
        HKCR = 8,
        /// <summary>
        /// This provides the path to the Local Machine.
        /// </summary>
        LocalMachine = HKLM,
        /// <summary>
        /// This is the root path of the current user.
        /// </summary>
        CurrentUser = HKCU,
        /// <summary>
        /// This is the root path of the Current Config key.
        /// </summary>
        /// <remarks>This property is deprecated on the registry; use it with caution.</remarks>
        CurrentConfig = HKCC,
        /// <summary>
        /// This is the root path of the Performance Data key.
        /// </summary>
        /// <remarks>This property is deprecated on the registry; use it with caution.</remarks>
        PerfData = HKPD,
        /// <summary>
        /// This is the root path of the Users Data key.
        /// </summary>
        UsersStore = HKU,
        /// <summary>
        /// This is the root path of the Classes Data Root key.
        /// </summary>
        CurrentClassesRoot = HKCR
    }

    /// <summary>
    /// An easy to use Windows Registry Editor.
    /// </summary>
    [SupportedOSPlatform("windows")]
    public class RegEditor : System.IDisposable
    {
        private System.String _RootKey_;
        private System.String _SubKey_;
        private System.Boolean _DIAG_;

        /// <summary>
        /// The Registry Root Key. It accepts only specific values.
        /// </summary>
        public RegRootKeyValues RootKey
        {
            get
            {
                switch (_RootKey_)
                {
                    case "HKEY_LOCAL_MACHINE": return RegRootKeyValues.HKLM;
                    case "HKEY_CURRENT_USER": return RegRootKeyValues.HKCU;
                    case "HKEY_CURRENT_CONFIG": return RegRootKeyValues.HKCC;
                    case "HKEY_PERFORMANCE_DATA": return RegRootKeyValues.HKPD;
                    case "HKEY_USERS": return RegRootKeyValues.HKU;
                    case "HKEY_CLASSES_ROOT": return RegRootKeyValues.HKCR;
                    default: return RegRootKeyValues.Inabsolute;
                }
            }
            set
            {
                switch (value)
                {
                    case RegRootKeyValues.HKLM: _RootKey_ = "HKEY_LOCAL_MACHINE"; break;
                    case RegRootKeyValues.HKCU: _RootKey_ = "HKEY_CURRENT_USER"; break;
                    case RegRootKeyValues.HKCC: _RootKey_ = "HKEY_CURRENT_CONFIG"; break;
                    case RegRootKeyValues.HKPD: _RootKey_ = "HKEY_PERFORMANCE_DATA"; break;
                    case RegRootKeyValues.HKU: _RootKey_ = "HKEY_USERS"; break;
                    case RegRootKeyValues.HKCR: _RootKey_ = "HKEY_CLASSES_ROOT"; break;
                }
            }
        }

        /// <summary>
        /// The Registry sub-root key. Can be nested the one on the another.
        /// </summary>
        public System.String SubKey
        {
            get { return _SubKey_; }
            set { if (System.String.IsNullOrEmpty(value) == false) { _SubKey_ = value; } }
        }

        /// <summary>
        /// The default , classical and parameterless constructor.
        /// </summary>
        /// <remarks>You must set the required Registry Paths by the respective properties.</remarks>
        public RegEditor() { }

        /// <summary>
        /// Constructor which can be used to set the required Registry Paths on initialisation.
        /// </summary>
        /// <param name="KeyValue">One of the valid Root Keys. See the <see cref="RegRootKeyValues"/> <see cref="System.Enum"/> for more information. </param>
        /// <param name="SubKey">The Registry sub-root key. Can be nested the one on the another.</param>
        public RegEditor(RegRootKeyValues KeyValue, System.String SubKey)
        {
            switch (KeyValue)
            {
                case RegRootKeyValues.HKLM: _RootKey_ = "HKEY_LOCAL_MACHINE"; break;
                case RegRootKeyValues.HKCU: _RootKey_ = "HKEY_CURRENT_USER"; break;
                case RegRootKeyValues.HKCC: _RootKey_ = "HKEY_CURRENT_CONFIG"; break;
                case RegRootKeyValues.HKPD: _RootKey_ = "HKEY_PERFORMANCE_DATA"; break;
                case RegRootKeyValues.HKU: _RootKey_ = "HKEY_USERS"; break;
                case RegRootKeyValues.HKCR: _RootKey_ = "HKEY_CLASSES_ROOT"; break;
            }
            if (System.String.IsNullOrEmpty(SubKey) == false) { _SubKey_ = SubKey; }
        }

        /// <summary>
        /// Enable the Console Diagnostic debugging.
        /// </summary>
        public System.Boolean DiagnosticMessages { set { _DIAG_ = value; } }

        private System.Boolean _CheckPredefinedProperties()
        {
            if ((System.String.IsNullOrEmpty(_RootKey_) == false) &&
                (System.String.IsNullOrEmpty(_SubKey_) == false)) { return true; }
            else { return false; }
        }

        /// <summary>
        /// Gets the specified value from the key provided.
        /// </summary>
        /// <param name="VariableRegistryMember">The value name to retrieve the value data.</param>
        /// <returns>If it succeeded , a new <see cref="System.Object"/> instance containing the data; Otherwise , a <see cref="System.String"/> explaining the error.</returns>
        public System.Object GetEntry(System.String VariableRegistryMember)
        {
            if (System.String.IsNullOrEmpty(VariableRegistryMember)) { return "Error"; }
            if (!_CheckPredefinedProperties())
            {
                if (_DIAG_) { MAIN.WriteConsoleText("Error - Cannot initiate the Internal editor due to an error: Properties that point the searcher are undefined."); }
                return "UNDEF_ERR";
            }
            System.Object RegEntry = Microsoft.Win32.Registry.GetValue($"{_RootKey_}\\{_SubKey_}", VariableRegistryMember, "_ER_C_");
            if (System.Convert.ToString(RegEntry, default) == "_ER_C_") { return "Error"; }
            else
            {
                if (RegEntry is System.String[])
                {
                    return RegEntry;
                }
                else if (RegEntry is System.Byte[])
                {
                    return RegEntry;
                }
                else if (RegEntry is System.String) { return RegEntry; }
                else
                {
                    if (_DIAG_)
                    {
                        MAIN.WriteConsoleText("Error - Could not translate the object returned by the procedure.");
                        MAIN.WriteConsoleText("Please check that the entry is not broken , incorrect or in format that is not supported by this editor.");
                    }
                    return "Error";
                }
            }
        }

        /// <summary>
        /// Sets or creates the specified value.
        /// </summary>
        /// <param name="VariableRegistryMember">The value name whose data will be modified.</param>
        /// <param name="RegistryType">The value type that this value will have. Consult the <see cref="RegTypes"/> <see cref="System.Enum"/> for more information.</param>
        /// <param name="RegistryData">The new data that will be saved on the value; The type is depending upon the <paramref name="RegistryType"/> parameter.</param>
        /// <returns>A new <see cref="RegFunctionResult"/> <see cref="System.Enum"/> explaining if it succeeded.</returns>
        public RegFunctionResult SetEntry(System.String VariableRegistryMember, RegTypes RegistryType, System.Object RegistryData)
        {
            if (System.String.IsNullOrEmpty(VariableRegistryMember))
            {
                return RegFunctionResult.Error;
            }
            if (!_CheckPredefinedProperties())
            {
                if (_DIAG_) { MAIN.WriteConsoleText("Error - Cannot initiate the Internal editor due to an error: Properties that point the searcher are undefined."); }
                return RegFunctionResult.Misdefinition_Error;
            }
            if (RegistryData == null)
            {
                if (_DIAG_) { MAIN.WriteConsoleText("ERROR: 'null' value detected in RegistryData object. Maybe invalid definition?"); }
                return RegFunctionResult.Misdefinition_Error;
            }
            Microsoft.Win32.RegistryValueKind RegType_;
            if (RegistryType == RegTypes.String)
            {
                RegType_ = Microsoft.Win32.RegistryValueKind.String;
            }
            else if (RegistryType == RegTypes.ExpandString)
            {
                RegType_ = Microsoft.Win32.RegistryValueKind.ExpandString;
            }
            else if (RegistryType == RegTypes.QuadWord)
            {
                RegType_ = Microsoft.Win32.RegistryValueKind.QWord;
            }
            else if (RegistryType == RegTypes.DoubleWord)
            {
                RegType_ = Microsoft.Win32.RegistryValueKind.DWord;
            }
            else
            {
                if (_DIAG_)
                {
                    MAIN.WriteConsoleText($"ERROR: Unknown registry value type argument in the object creator was given: {RegistryType}");
                }
                return RegFunctionResult.InvalidRootKey;
            }
            try
            {
                Microsoft.Win32.Registry.SetValue($"{_RootKey_}\\{_SubKey_}", VariableRegistryMember, RegistryData, RegType_);
            }
            catch (System.Exception EX)
            {
                if (_DIAG_)
                {
                    MAIN.WriteConsoleText($"ERROR: Could not create key {VariableRegistryMember} . Invalid name maybe?");
                    MAIN.WriteConsoleText($"Error Raw Data: {EX}");
                }
                return RegFunctionResult.Error;
            }
            return RegFunctionResult.Success;
        }

        /// <summary>
        /// Deletes the specified value from the registry.
        /// </summary>
        /// <param name="VariableRegistryMember">The value which will be deleted.</param>
        /// <returns>A new <see cref="RegFunctionResult"/> <see cref="System.Enum"/> explaining if it succeeded.</returns>
        public RegFunctionResult DeleteEntry(System.String VariableRegistryMember)
        {
            if (System.String.IsNullOrEmpty(VariableRegistryMember)) { return RegFunctionResult.Error; }
            if (!_CheckPredefinedProperties())
            {
                if (_DIAG_) { MAIN.WriteConsoleText("Error - Cannot initiate the Internal editor due to an error: Properties that point the searcher are undefined."); }
                return RegFunctionResult.Misdefinition_Error;
            }
            Microsoft.Win32.RegistryKey ValueDelete;
            switch (_RootKey_)
            {
                case "HKEY_LOCAL_MACHINE":
                    ValueDelete = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(_SubKey_);
                    break;
                case "HKEY_CURRENT_USER":
                    ValueDelete = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(_SubKey_);
                    break;
                case "HKEY_CURRENT_CONFIG":
                    ValueDelete = Microsoft.Win32.Registry.CurrentConfig.OpenSubKey(_SubKey_);
                    break;
                case "HKEY_PERFORMANCE_DATA":
                    ValueDelete = Microsoft.Win32.Registry.PerformanceData.OpenSubKey(_SubKey_);
                    break;
                case "HKEY_USERS":
                    ValueDelete = Microsoft.Win32.Registry.Users.OpenSubKey(_SubKey_);
                    break;
                case "HKEY_CLASSES_ROOT":
                    ValueDelete = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(_SubKey_);
                    break;
                default:
                    if (_DIAG_)
                    {
                        MAIN.WriteConsoleText("Error - Registry root key could not be get. Incorrect Root Key Detected.");
                        MAIN.WriteConsoleText("Error while getting the root key: Root Key " + _RootKey_ + "Is invalid.");
                    }
                    return RegFunctionResult.Misdefinition_Error;
            }
            if (System.Convert.ToString(ValueDelete.GetValue(VariableRegistryMember, "_DNE_")) == "_DNE_")
            {
                ValueDelete.Close();
                return RegFunctionResult.Error;
            }
            ValueDelete.DeleteValue(VariableRegistryMember);
            ValueDelete.Close();
            return RegFunctionResult.Success;
        }

        /// <summary>
        /// Creates a new key inside a sub-key or the root key.
        /// </summary>
        /// <param name="KeyName">The sub-key to create. If this parameter is not defined , 
        /// then it will create the sub-key name defined in the <see cref="SubKey"/> property.</param>
        /// <returns>A new <see cref="RegFunctionResult"/> enumeration , which indicates success or not.</returns>
        public RegFunctionResult CreateNewKey(System.String KeyName = "")
        {
            if (System.String.IsNullOrEmpty(KeyName)) { KeyName = _SubKey_; }
            Microsoft.Win32.RegistryKey ValueCreate;
            try
            {
                switch (_RootKey_)
                {
                    case "HKEY_LOCAL_MACHINE":
                        ValueCreate = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(KeyName);
                        break;
                    case "HKEY_CURRENT_USER":
                        ValueCreate = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(KeyName);
                        break;
                    case "HKEY_CURRENT_CONFIG":
                        ValueCreate = Microsoft.Win32.Registry.CurrentConfig.CreateSubKey(KeyName);
                        break;
                    case "HKEY_PERFORMANCE_DATA":
                        ValueCreate = Microsoft.Win32.Registry.PerformanceData.CreateSubKey(KeyName);
                        break;
                    case "HKEY_USERS":
                        ValueCreate = Microsoft.Win32.Registry.Users.CreateSubKey(KeyName);
                        break;
                    case "HKEY_CLASSES_ROOT":
                        ValueCreate = Microsoft.Win32.Registry.ClassesRoot.CreateSubKey(KeyName);
                        break;
                    default:
                        if (_DIAG_)
                        {
                            MAIN.WriteConsoleText("Error - Registry root key could not be get. Incorrect Root Key Detected.");
                            MAIN.WriteConsoleText("Error while getting the root key: Root Key " + _RootKey_ + "Is invalid.");
                        }
                        return RegFunctionResult.Misdefinition_Error;
                }
                ValueCreate.Flush();
                ValueCreate.Close();
            }
            catch (Exception) { return RegFunctionResult.Error; }
            return RegFunctionResult.Success;
        }

        /// <summary>
        /// Determines whether a specified registry key exists , it's path depends by the <see cref="RootKey"/>
        /// property and the <paramref name="KeyName"/> parameter.
        /// </summary>
        /// <param name="KeyName">The sub-key to find. If this parameter is not defined , 
        /// then it will find the sub-key name defined in the <see cref="SubKey"/> property.</param>
        /// <returns><see langword="true"/> if the key exists , otherwise <see langword="false"/>. </returns>
        public System.Boolean KeyExists(System.String KeyName = "")
        {
            if (System.String.IsNullOrEmpty(KeyName)) { KeyName = _SubKey_; }
            Microsoft.Win32.RegistryKey ValueFind;
            try
            {
                switch (_RootKey_)
                {
                    case "HKEY_LOCAL_MACHINE":
                        ValueFind = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(KeyName);
                        break;
                    case "HKEY_CURRENT_USER":
                        ValueFind = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(KeyName);
                        break;
                    case "HKEY_CURRENT_CONFIG":
                        ValueFind = Microsoft.Win32.Registry.CurrentConfig.OpenSubKey(KeyName);
                        break;
                    case "HKEY_PERFORMANCE_DATA":
                        ValueFind = Microsoft.Win32.Registry.PerformanceData.OpenSubKey(KeyName);
                        break;
                    case "HKEY_USERS":
                        ValueFind = Microsoft.Win32.Registry.Users.OpenSubKey(KeyName);
                        break;
                    case "HKEY_CLASSES_ROOT":
                        ValueFind = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(KeyName);
                        break;
                    default:
                        return false;
                }
                ValueFind.Close();
            }
            catch (Exception) { return false; }
            return true;
        }

        /// <summary>
        /// Use this Dispose method to clear up the current key that the class is working on and make it possible to set a new path to work on.
        /// </summary>
        public void Dispose() { Dispose(true); GC.SuppressFinalize(this); }

        /// <summary>
        /// This Dispose method does the same thing as the parameterless <see cref="Dispose()"/> , 
        /// but has the option for a boolean to dispose all resources.
        /// Mainly provided for derivation reasons.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(System.Boolean disposing) { DisposeRes(); }

        private protected void DisposeRes()
        {
            // Delete any unused values.
            _RootKey_ = null;
            _SubKey_ = null;
            _DIAG_ = false;
        }
    }

    /// <summary>
    /// The exception that it is thrown when an unexpected result in an executing code block was found.
    /// </summary>
    public class ExecutionException : System.Exception
    {
        /// <summary>
        /// Creates a new instance of <see cref="ExecutionException"/> class.
        /// </summary>
        public ExecutionException() : base() { }

        /// <summary>
        /// Creates a new instance of <see cref="ExecutionException"/> class with the specified error message.
        /// </summary>
        /// <param name="message">The error message to show.</param>
        public ExecutionException(System.String message) : base(message) { }

        /// <summary>
        /// Creates a new instance of <see cref="ExecutionException"/> class with the specified error message
        /// and the <see cref="Exception"/> that caused this exception.
        /// </summary>
        /// <param name="message">The error message to show.</param>
        /// <param name="InnerException">The inner exception that it is the root cause of this exception.</param>
        public ExecutionException(System.String message , Exception InnerException) : base(message , InnerException) { }
    }

    /// <summary>
    /// The exception that is thrown when a native P/Invoke call failed to give correct results.
    /// </summary>
    public sealed class NativeCallErrorException : ExecutionException
    {
        /// <summary>
        /// Creates a new instance of <see cref="NativeCallErrorException"/> class.
        /// </summary>
        public NativeCallErrorException() : base() { }

        /// <summary>
        /// Creates a new instance of <see cref="NativeCallErrorException"/> 
        /// class with the specified error code.
        /// </summary>
        public NativeCallErrorException(System.Int64 code) : base() { ErrorCode = code; }

        /// <summary>
        /// Creates a new instance of <see cref="NativeCallErrorException"/> class with the specified error message.
        /// </summary>
        /// <param name="message">The error message to show.</param>
        public NativeCallErrorException(System.String message) : base(message) { }

        /// <summary>
        /// Creates a new instance of <see cref="NativeCallErrorException"/> class with the specified
        /// native error code and error message.
        /// </summary>
        /// <param name="message">The error message to show.</param>
        /// <param name="code">The error code that caused this exception.</param>
        public NativeCallErrorException(System.Int64 code , System.String message) : base(message) { ErrorCode = code; }

        /// <summary>
        /// Creates a new instance of <see cref="NativeCallErrorException"/> class with the specified error message
        /// and the <see cref="Exception"/> that caused this exception.
        /// </summary>
        /// <param name="message">The error message to show.</param>
        /// <param name="InnerException">The inner exception that it is the root cause of this exception.</param>
        public NativeCallErrorException(System.String message, Exception InnerException) : base(message, InnerException) { }

        /// <summary>
        /// The error code of the native call , if it is available.
        /// </summary>
        public System.Int64 ErrorCode { get; private set; }
    }

    /// <summary>
    /// Gets information on the last native Windows error code.
    /// </summary>
    [SupportedOSPlatform("windows")]
    public static class WindowsErrorCodes
    {
        [Flags]
        [Serializable]
        private enum FormatMsg_Flags : System.UInt32
        {
            FORMAT_MESSAGE_NO_RESTRICTIONS = 0,
            FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x00000100,
            FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x00002000,
            FORMAT_MESSAGE_FROM_HMODULE = 0x00000800,
            FORMAT_MESSAGE_FROM_STRING = 0x00000400,
            FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000,
            FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200,
            FORMAT_MESSAGE_MAX_WIDTH_MASK = 0x000000FF
        }

        [Flags]
        [Serializable]
        private enum FormatMsg_SourceType : System.UInt32
        {
            None = 0,
            FORMAT_MESSAGE_FROM_HMODULE = 0x00000800,
            FORMAT_MESSAGE_FROM_STRING = 0x00000400
        }

        [DllImport(Interop.Libraries.Kernel32, CallingConvention = CallingConvention.Winapi,
            CharSet = CharSet.Unicode, EntryPoint = "FormatMessageW")]
        private static extern System.UInt32 GetString(
            [In] FormatMsg_Flags Flags,
            [In] [Optional] FormatMsg_SourceType Source,
            [In] System.UInt32 MessageID,
            [In] System.UInt32 LanguageID,
            [Out] [MarshalAs(UnmanagedType.LPWStr)] System.String Buffer,
            [In] System.UInt32 Input,
            [In] [Optional] System.String Arguments);

        [DllImport(Interop.Libraries.Kernel32 , CallingConvention = CallingConvention.Winapi ,
            CharSet = CharSet.Auto , EntryPoint = "GetLastError")]
        private static extern System.UInt32 LastWinErrorCode();

        /// <summary>
        /// Gets the last Windows native error code.
        /// </summary>
        /// <remarks>
        /// Note: This error code is only specific for the thread it was called on. <br />
        /// To get the error code for another thread , use this property on that thread.
        /// </remarks>
        public static System.UInt32 LastErrorCode 
        {
            [System.Security.SecurityCritical]
            [System.Security.SuppressUnmanagedCodeSecurity]
            get { return LastWinErrorCode(); } 
        }

        /// <summary>
        /// Gets a string description from the last error code.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">
        /// No error was found , the property cannot return anything.
        /// </exception>
        public static System.String LastErrorString 
        {
            [System.Security.SecurityCritical]
            [System.Security.SuppressUnmanagedCodeSecurity]
            get 
            {
                if (LastErrorCode == 0) { throw new System.InvalidOperationException("No Error was found , nothing to show"); }
                System.Char[] D = new System.Char[700];
                System.String retval = new(D);
                GetString(FormatMsg_Flags.FORMAT_MESSAGE_ALLOCATE_BUFFER |
                    FormatMsg_Flags.FORMAT_MESSAGE_FROM_SYSTEM |
                    FormatMsg_Flags.FORMAT_MESSAGE_IGNORE_INSERTS,
                    FormatMsg_SourceType.None, LastErrorCode, 1033, retval, (System.UInt32)retval.Length);
                D = null;
                return retval;
            }
        }
    }
        
}

internal enum ProcessInterop_Memory_Priority_Levels : System.UInt64
{
    None = 0,
    MEMORY_PRIORITY_VERY_LOW,
    MEMORY_PRIORITY_LOW,
    MEMORY_PRIORITY_MEDIUM,
    MEMORY_PRIORITY_BELOW_NORMAL,
    MEMORY_PRIORITY_NORMAL
}

[StructLayout(LayoutKind.Explicit , CharSet = CharSet.Auto , Size = 64)]
internal struct ProcessInterop_Memory_Priority_Info
{
    [FieldOffset(0)]
    [MarshalAs(UnmanagedType.U8)]
    public ProcessInterop_Memory_Priority_Levels MemoryPriority;
}