


using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Text
{
    using System.Text.Encodings.Web;

    internal readonly struct Rune : IEquatable<Rune>
    {
        private const int MaxUtf16CharsPerRune = 2;

        private const char HighSurrogateStart = '\ud800';

        private const char LowSurrogateStart = '\udc00';

        private const int HighSurrogateRange = 1023;

        private readonly uint _value;

        public bool IsAscii => UnicodeUtility.IsAsciiCodePoint(_value);

        public bool IsBmp => UnicodeUtility.IsBmpCodePoint(_value);

        public static Rune ReplacementChar => UnsafeCreate(65533u);

        public int Utf16SequenceLength => UnicodeUtility.GetUtf16SequenceLength(_value);

        public int Value => (int)_value;

        public Rune(uint value)
        {
            if (!UnicodeUtility.IsValidUnicodeScalar(value))
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.value);
            }
            _value = value;
        }

        public Rune(int value)
            : this((uint)value)
        {
        }

        private Rune(uint scalarValue, bool unused)
        {
            _value = scalarValue;
        }

        public static bool operator ==(Rune left, Rune right)
        {
            return left._value == right._value;
        }

        public static bool operator !=(Rune left, Rune right)
        {
            return left._value != right._value;
        }

        public static bool IsControl(Rune value)
        {
            return ((value._value + 1) & 0xFFFFFF7Fu) <= 32;
        }

        public static OperationStatus DecodeFromUtf16(ReadOnlySpan<char> source, out Rune result, out int charsConsumed)
        {
            if (!source.IsEmpty)
            {
                char c = source[0];
                if (TryCreate(c, out result))
                {
                    charsConsumed = 1;
                    return OperationStatus.Done;
                }
                if (1u < (uint)source.Length)
                {
                    char lowSurrogate = source[1];
                    if (TryCreate(c, lowSurrogate, out result))
                    {
                        charsConsumed = 2;
                        return OperationStatus.Done;
                    }
                }
                else if (char.IsHighSurrogate(c))
                {
                    goto IL_004c;
                }
                charsConsumed = 1;
                result = ReplacementChar;
                return OperationStatus.InvalidData;
            }
            goto IL_004c;
        IL_004c:
            charsConsumed = source.Length;
            result = ReplacementChar;
            return OperationStatus.NeedMoreData;
        }

        public static OperationStatus DecodeFromUtf8(ReadOnlySpan<byte> source, out Rune result, out int bytesConsumed)
        {
            int num = 0;
            uint num2;
            if ((uint)num < (uint)source.Length)
            {
                num2 = source[num];
                if (UnicodeUtility.IsAsciiCodePoint(num2))
                {
                    goto IL_0021;
                }
                if (UnicodeUtility.IsInRangeInclusive(num2, 194u, 244u))
                {
                    num2 = num2 - 194 << 6;
                    num++;
                    if ((uint)num >= (uint)source.Length)
                    {
                        goto IL_0163;
                    }
                    int num3 = (sbyte)source[num];
                    if (num3 < -64)
                    {
                        num2 += (uint)num3;
                        num2 += 128;
                        num2 += 128;
                        if (num2 < 2048)
                        {
                            goto IL_0021;
                        }
                        if (UnicodeUtility.IsInRangeInclusive(num2, 2080u, 3343u) && !UnicodeUtility.IsInRangeInclusive(num2, 2912u, 2943u) && !UnicodeUtility.IsInRangeInclusive(num2, 3072u, 3087u))
                        {
                            num++;
                            if ((uint)num >= (uint)source.Length)
                            {
                                goto IL_0163;
                            }
                            num3 = (sbyte)source[num];
                            if (num3 < -64)
                            {
                                num2 <<= 6;
                                num2 += (uint)num3;
                                num2 += 128;
                                num2 -= 131072;
                                if (num2 > 65535)
                                {
                                    num++;
                                    if ((uint)num >= (uint)source.Length)
                                    {
                                        goto IL_0163;
                                    }
                                    num3 = (sbyte)source[num];
                                    if (num3 >= -64)
                                    {
                                        goto IL_0153;
                                    }
                                    num2 <<= 6;
                                    num2 += (uint)num3;
                                    num2 += 128;
                                    num2 -= 4194304;
                                }
                                goto IL_0021;
                            }
                        }
                    }
                }
                else
                {
                    num = 1;
                }
                goto IL_0153;
            }
            goto IL_0163;
        IL_0021:
            bytesConsumed = num + 1;
            result = UnsafeCreate(num2);
            return OperationStatus.Done;
        IL_0153:
            bytesConsumed = num;
            result = ReplacementChar;
            return OperationStatus.InvalidData;
        IL_0163:
            bytesConsumed = num;
            result = ReplacementChar;
            return OperationStatus.NeedMoreData;
        }

        public override bool Equals([NotNullWhen(true)] object obj)
        {
            if (obj is Rune other)
            {
                return Equals(other);
            }
            return false;
        }

        public bool Equals(Rune other)
        {
            return this == other;
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public static bool TryCreate(char ch, out Rune result)
        {
            if (!UnicodeUtility.IsSurrogateCodePoint(ch))
            {
                result = UnsafeCreate(ch);
                return true;
            }
            result = default(Rune);
            return false;
        }

        public static bool TryCreate(char highSurrogate, char lowSurrogate, out Rune result)
        {
            uint num = (uint)(highSurrogate - 55296);
            uint num2 = (uint)(lowSurrogate - 56320);
            if ((num | num2) <= 1023)
            {
                result = UnsafeCreate((uint)((int)(num << 10) + (lowSurrogate - 56320) + 65536));
                return true;
            }
            result = default(Rune);
            return false;
        }

        public bool TryEncodeToUtf16(Span<char> destination, out int charsWritten)
        {
            if (destination.Length >= 1)
            {
                if (IsBmp)
                {
                    destination[0] = (char)_value;
                    charsWritten = 1;
                    return true;
                }
                if (destination.Length >= 2)
                {
                    UnicodeUtility.GetUtf16SurrogatesFromSupplementaryPlaneScalar(_value, out destination[0], out destination[1]);
                    charsWritten = 2;
                    return true;
                }
            }
            charsWritten = 0;
            return false;
        }

        public bool TryEncodeToUtf8(Span<byte> destination, out int bytesWritten)
        {
            if (destination.Length >= 1)
            {
                if (IsAscii)
                {
                    destination[0] = (byte)_value;
                    bytesWritten = 1;
                    return true;
                }
                if (destination.Length >= 2)
                {
                    if (_value <= 2047)
                    {
                        destination[0] = (byte)(_value + 12288 >> 6);
                        destination[1] = (byte)((_value & 0x3F) + 128);
                        bytesWritten = 2;
                        return true;
                    }
                    if (destination.Length >= 3)
                    {
                        if (_value <= 65535)
                        {
                            destination[0] = (byte)(_value + 917504 >> 12);
                            destination[1] = (byte)(((_value & 0xFC0) >> 6) + 128);
                            destination[2] = (byte)((_value & 0x3F) + 128);
                            bytesWritten = 3;
                            return true;
                        }
                        if (destination.Length >= 4)
                        {
                            destination[0] = (byte)(_value + 62914560 >> 18);
                            destination[1] = (byte)(((_value & 0x3F000) >> 12) + 128);
                            destination[2] = (byte)(((_value & 0xFC0) >> 6) + 128);
                            destination[3] = (byte)((_value & 0x3F) + 128);
                            bytesWritten = 4;
                            return true;
                        }
                    }
                }
            }
            bytesWritten = 0;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Rune UnsafeCreate(uint scalarValue)
        {
            return new Rune(scalarValue, unused: false);
        }
    }

    internal static class UnicodeDebug
    {
        [Conditional("DEBUG")]
        internal static void AssertIsBmpCodePoint(uint codePoint)
        {
            UnicodeUtility.IsBmpCodePoint(codePoint);
        }

        [Conditional("DEBUG")]
        internal static void AssertIsHighSurrogateCodePoint(uint codePoint)
        {
            UnicodeUtility.IsHighSurrogateCodePoint(codePoint);
        }

        [Conditional("DEBUG")]
        internal static void AssertIsLowSurrogateCodePoint(uint codePoint)
        {
            UnicodeUtility.IsLowSurrogateCodePoint(codePoint);
        }

        [Conditional("DEBUG")]
        internal static void AssertIsValidCodePoint(uint codePoint)
        {
            UnicodeUtility.IsValidCodePoint(codePoint);
        }

        [Conditional("DEBUG")]
        internal static void AssertIsValidScalar(uint scalarValue)
        {
            UnicodeUtility.IsValidUnicodeScalar(scalarValue);
        }

        [Conditional("DEBUG")]
        internal static void AssertIsValidSupplementaryPlaneScalar(uint scalarValue)
        {
            if (UnicodeUtility.IsValidUnicodeScalar(scalarValue))
            {
                UnicodeUtility.IsBmpCodePoint(scalarValue);
            }
        }

        private static string ToHexString(uint codePoint)
        {
            return FormattableString.Invariant($"U+{codePoint:X4}");
        }
    }

    internal static class UnicodeUtility
    {
        public const uint ReplacementChar = 65533u;

        public static int GetPlane(uint codePoint)
        {
            return (int)(codePoint >> 16);
        }

        public static uint GetScalarFromUtf16SurrogatePair(uint highSurrogateCodePoint, uint lowSurrogateCodePoint)
        {
            return (highSurrogateCodePoint << 10) + lowSurrogateCodePoint - 56613888;
        }

        public static int GetUtf16SequenceLength(uint value)
        {
            value -= 65536;
            value += 33554432;
            value >>= 24;
            return (int)value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetUtf16SurrogatesFromSupplementaryPlaneScalar(uint value, out char highSurrogateCodePoint, out char lowSurrogateCodePoint)
        {
            highSurrogateCodePoint = (char)(value + 56557568 >> 10);
            lowSurrogateCodePoint = (char)((value & 0x3FF) + 56320);
        }

        public static int GetUtf8SequenceLength(uint value)
        {
            int num = (int)(value - 2048) >> 31;
            value ^= 0xF800u;
            value -= 63616;
            value += 67108864;
            value >>= 24;
            return (int)value + num * 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAsciiCodePoint(uint value)
        {
            return value <= 127;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBmpCodePoint(uint value)
        {
            return value <= 65535;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsHighSurrogateCodePoint(uint value)
        {
            return IsInRangeInclusive(value, 55296u, 56319u);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInRangeInclusive(uint value, uint lowerBound, uint upperBound)
        {
            return value - lowerBound <= upperBound - lowerBound;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsLowSurrogateCodePoint(uint value)
        {
            return IsInRangeInclusive(value, 56320u, 57343u);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsSurrogateCodePoint(uint value)
        {
            return IsInRangeInclusive(value, 55296u, 57343u);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidCodePoint(uint codePoint)
        {
            return codePoint <= 1114111;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidUnicodeScalar(uint value)
        {
            return ((value - 1114112) ^ 0xD800) >= 4293855232u;
        }
    }

    internal ref struct ValueStringBuilder
    {
        private char[] _arrayToReturnToPool;

        private Span<char> _chars;

        private int _pos;

        public int Length
        {
            get
            {
                return _pos;
            }
            set
            {
                _pos = value;
            }
        }

        public int Capacity => _chars.Length;

        public ref char this[int index] => ref _chars[index];

        public Span<char> RawChars => _chars;

        public ValueStringBuilder(Span<char> initialBuffer)
        {
            _arrayToReturnToPool = null;
            _chars = initialBuffer;
            _pos = 0;
        }

        public ValueStringBuilder(int initialCapacity)
        {
            _arrayToReturnToPool = ArrayPool<char>.Shared.Rent(initialCapacity);
            _chars = _arrayToReturnToPool;
            _pos = 0;
        }

        public void EnsureCapacity(int capacity)
        {
            if ((uint)capacity > (uint)_chars.Length)
            {
                Grow(capacity - _pos);
            }
        }

        public ref char GetPinnableReference()
        {
            return ref MemoryMarshal.GetReference(_chars);
        }

        public ref char GetPinnableReference(bool terminate)
        {
            if (terminate)
            {
                EnsureCapacity(Length + 1);
                _chars[Length] = '\0';
            }
            return ref MemoryMarshal.GetReference(_chars);
        }

        public override string ToString()
        {
            string result = _chars.Slice(0, _pos).ToString();
            Dispose();
            return result;
        }

        public ReadOnlySpan<char> AsSpan(bool terminate)
        {
            if (terminate)
            {
                EnsureCapacity(Length + 1);
                _chars[Length] = '\0';
            }
            return _chars.Slice(0, _pos);
        }

        public ReadOnlySpan<char> AsSpan()
        {
            return _chars.Slice(0, _pos);
        }

        public ReadOnlySpan<char> AsSpan(int start)
        {
            return _chars.Slice(start, _pos - start);
        }

        public ReadOnlySpan<char> AsSpan(int start, int length)
        {
            return _chars.Slice(start, length);
        }

        public bool TryCopyTo(Span<char> destination, out int charsWritten)
        {
            if (_chars.Slice(0, _pos).TryCopyTo(destination))
            {
                charsWritten = _pos;
                Dispose();
                return true;
            }
            charsWritten = 0;
            Dispose();
            return false;
        }

        public void Insert(int index, char value, int count)
        {
            if (_pos > _chars.Length - count)
            {
                Grow(count);
            }
            int length = _pos - index;
            _chars.Slice(index, length).CopyTo(_chars.Slice(index + count));
            _chars.Slice(index, count).Fill(value);
            _pos += count;
        }

        public void Insert(int index, string s)
        {
            if (s != null)
            {
                int length = s.Length;
                if (_pos > _chars.Length - length)
                {
                    Grow(length);
                }
                int length2 = _pos - index;
                _chars.Slice(index, length2).CopyTo(_chars.Slice(index + length));
                s.AsSpan().CopyTo(_chars.Slice(index));
                _pos += length;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(char c)
        {
            int pos = _pos;
            if ((uint)pos < (uint)_chars.Length)
            {
                _chars[pos] = c;
                _pos = pos + 1;
            }
            else
            {
                GrowAndAppend(c);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(string s)
        {
            if (s != null)
            {
                int pos = _pos;
                if (s.Length == 1 && (uint)pos < (uint)_chars.Length)
                {
                    _chars[pos] = s[0];
                    _pos = pos + 1;
                }
                else
                {
                    AppendSlow(s);
                }
            }
        }

        private void AppendSlow(string s)
        {
            int pos = _pos;
            if (pos > _chars.Length - s.Length)
            {
                Grow(s.Length);
            }
            s.AsSpan().CopyTo(_chars.Slice(pos));
            _pos += s.Length;
        }

        public void Append(char c, int count)
        {
            if (_pos > _chars.Length - count)
            {
                Grow(count);
            }
            Span<char> span = _chars.Slice(_pos, count);
            for (int i = 0; i < span.Length; i++)
            {
                span[i] = c;
            }
            _pos += count;
        }

        public unsafe void Append(char* value, int length)
        {
            int pos = _pos;
            if (pos > _chars.Length - length)
            {
                Grow(length);
            }
            Span<char> span = _chars.Slice(_pos, length);
            for (int i = 0; i < span.Length; i++)
            {
                span[i] = *(value++);
            }
            _pos += length;
        }

        public void Append(ReadOnlySpan<char> value)
        {
            int pos = _pos;
            if (pos > _chars.Length - value.Length)
            {
                Grow(value.Length);
            }
            value.CopyTo(_chars.Slice(_pos));
            _pos += value.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<char> AppendSpan(int length)
        {
            int pos = _pos;
            if (pos > _chars.Length - length)
            {
                Grow(length);
            }
            _pos = pos + length;
            return _chars.Slice(pos, length);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void GrowAndAppend(char c)
        {
            Grow(1);
            Append(c);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void Grow(int additionalCapacityBeyondPos)
        {
            int minimumLength = (int)Math.Max((uint)(_pos + additionalCapacityBeyondPos), Math.Min((uint)(_chars.Length * 2), 2147483591u));
            char[] array = ArrayPool<char>.Shared.Rent(minimumLength);
            _chars.Slice(0, _pos).CopyTo(array);
            char[] arrayToReturnToPool = _arrayToReturnToPool;
            _chars = (_arrayToReturnToPool = array);
            if (arrayToReturnToPool != null)
            {
                ArrayPool<char>.Shared.Return(arrayToReturnToPool);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            char[] arrayToReturnToPool = _arrayToReturnToPool;
            this = default(ValueStringBuilder);
            if (arrayToReturnToPool != null)
            {
                ArrayPool<char>.Shared.Return(arrayToReturnToPool);
            }
        }
    }

    namespace Encodings.Web
    {
        using System.IO;
        using System.Buffers;
        using System.Numerics;
        using System.Text.Unicode;
        using System.Buffers.Binary;
        using System.ComponentModel;
        using System.Collections.Generic;
        using System.Diagnostics.CodeAnalysis;
        

        internal struct AllowedBmpCodePointsBitmap
        {
            private const int BitmapLengthInDWords = 2048;

            private unsafe fixed uint Bitmap[2048];

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe void AllowChar(char value)
            {
                _GetIndexAndOffset(value, out UIntPtr index, out int offset);
                ref uint reference = ref Bitmap[(ulong)index];
                reference |= (uint)(1 << offset);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe void ForbidChar(char value)
            {
                _GetIndexAndOffset(value, out UIntPtr index, out int offset);
                ref uint reference = ref Bitmap[(ulong)index];
                reference &= (uint)(~(1 << offset));
            }

            public void ForbidHtmlCharacters()
            {
                ForbidChar('<');
                ForbidChar('>');
                ForbidChar('&');
                ForbidChar('\'');
                ForbidChar('"');
                ForbidChar('+');
            }

            public unsafe void ForbidUndefinedCharacters()
            {
                fixed (uint* pointer = Bitmap)
                {
                    ReadOnlySpan<byte> definedBmpCodePointsBitmapLittleEndian = UnicodeHelpers.GetDefinedBmpCodePointsBitmapLittleEndian();
                    Span<uint> span = new Span<uint>(pointer, 2048);
                    for (int i = 0; i < span.Length; i++)
                    {
                        span[i] &= BinaryPrimitives.ReadUInt32LittleEndian(definedBmpCodePointsBitmapLittleEndian.Slice(i * 4));
                    }
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe readonly bool IsCharAllowed(char value)
            {
                _GetIndexAndOffset(value, out UIntPtr index, out int offset);
                if ((Bitmap[(ulong)index] & (uint)(1 << offset)) != 0)
                {
                    return true;
                }
                return false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe readonly bool IsCodePointAllowed(uint value)
            {
                if (!UnicodeUtility.IsBmpCodePoint(value))
                {
                    return false;
                }
                _GetIndexAndOffset(value, out UIntPtr index, out int offset);
                if ((Bitmap[(ulong)index] & (uint)(1 << offset)) != 0)
                {
                    return true;
                }
                return false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void _GetIndexAndOffset(uint value, out nuint index, out int offset)
            {
                index = value >> 5;
                offset = (int)(value & 0x1F);
            }
        }

        internal struct AsciiByteMap
        {
            private const int BufferSize = 128;

            private unsafe fixed byte Buffer[128];

            internal unsafe void InsertAsciiChar(char key, byte value)
            {
                if (key < '\u0080')
                {
                    Buffer[(uint)key] = value;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal unsafe readonly bool TryLookup(Rune key, out byte value)
            {
                if (key.IsAscii)
                {
                    byte b = Buffer[(uint)key.Value];
                    if (b != 0)
                    {
                        value = b;
                        return true;
                    }
                }
                value = 0;
                return false;
            }
        }

        internal sealed class DefaultHtmlEncoder : HtmlEncoder
        {
            private sealed class EscaperImplementation : ScalarEscaperBase
            {
                internal static readonly EscaperImplementation Singleton = new EscaperImplementation();

                private EscaperImplementation()
                {
                }

                internal override int EncodeUtf8(Rune value, Span<byte> destination)
                {
                    if (value.Value == 60)
                    {
                        if (SpanUtility.TryWriteBytes(destination, 38, 108, 116, 59))
                        {
                            return 4;
                        }
                    }
                    else if (value.Value == 62)
                    {
                        if (SpanUtility.TryWriteBytes(destination, 38, 103, 116, 59))
                        {
                            return 4;
                        }
                    }
                    else if (value.Value == 38)
                    {
                        if (SpanUtility.TryWriteBytes(destination, 38, 97, 109, 112, 59))
                        {
                            return 5;
                        }
                    }
                    else
                    {
                        if (value.Value != 34)
                        {
                            return TryEncodeScalarAsHex(this, (uint)value.Value, destination);
                        }
                        if (SpanUtility.TryWriteBytes(destination, 38, 113, 117, 111, 116, 59))
                        {
                            return 6;
                        }
                    }
                    return -1;
                    static int TryEncodeScalarAsHex(object @this, uint scalarValue, Span<byte> destination)
                    {
                        int num = (int)((uint)BitOperations.Log2(scalarValue) / 4u + 4);
                        if (SpanUtility.IsValidIndex(destination, num))
                        {
                            destination[num] = 59;
                            SpanUtility.TryWriteBytes(destination, 38, 35, 120, 48);
                            destination = destination.Slice(3, num - 3);
                            int num2 = destination.Length - 1;
                            while (SpanUtility.IsValidIndex(destination, num2))
                            {
                                char c = HexConverter.ToCharUpper((int)scalarValue);
                                destination[num2] = (byte)c;
                                scalarValue >>= 4;
                                num2--;
                            }
                            return destination.Length + 4;
                        }
                        return -1;
                    }
                }

                internal override int EncodeUtf16(Rune value, Span<char> destination)
                {
                    if (value.Value == 60)
                    {
                        if (SpanUtility.TryWriteChars(destination, '&', 'l', 't', ';'))
                        {
                            return 4;
                        }
                    }
                    else if (value.Value == 62)
                    {
                        if (SpanUtility.TryWriteChars(destination, '&', 'g', 't', ';'))
                        {
                            return 4;
                        }
                    }
                    else if (value.Value == 38)
                    {
                        if (SpanUtility.TryWriteChars(destination, '&', 'a', 'm', 'p', ';'))
                        {
                            return 5;
                        }
                    }
                    else
                    {
                        if (value.Value != 34)
                        {
                            return TryEncodeScalarAsHex(this, (uint)value.Value, destination);
                        }
                        if (SpanUtility.TryWriteChars(destination, '&', 'q', 'u', 'o', 't', ';'))
                        {
                            return 6;
                        }
                    }
                    return -1;
                    static int TryEncodeScalarAsHex(object @this, uint scalarValue, Span<char> destination)
                    {
                        int num = (int)((uint)BitOperations.Log2(scalarValue) / 4u + 4);
                        if (SpanUtility.IsValidIndex(destination, num))
                        {
                            destination[num] = ';';
                            SpanUtility.TryWriteChars(destination, '&', '#', 'x', '0');
                            destination = destination.Slice(3, num - 3);
                            int num2 = destination.Length - 1;
                            while (SpanUtility.IsValidIndex(destination, num2))
                            {
                                char c = HexConverter.ToCharUpper((int)scalarValue);
                                destination[num2] = c;
                                scalarValue >>= 4;
                                num2--;
                            }
                            return destination.Length + 4;
                        }
                        return -1;
                    }
                }
            }

            internal static readonly DefaultHtmlEncoder BasicLatinSingleton = new DefaultHtmlEncoder(new TextEncoderSettings(UnicodeRanges.BasicLatin));

            private readonly OptimizedInboxTextEncoder _innerEncoder;

            public override int MaxOutputCharactersPerInputCharacter => 8;

            internal DefaultHtmlEncoder(TextEncoderSettings settings)
            {
                if (settings == null)
                {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.settings);
                }
                _innerEncoder = new OptimizedInboxTextEncoder(EscaperImplementation.Singleton, in settings.GetAllowedCodePointsBitmap());
            }

            private protected override OperationStatus EncodeCore(ReadOnlySpan<char> source, Span<char> destination, out int charsConsumed, out int charsWritten, bool isFinalBlock)
            {
                return _innerEncoder.Encode(source, destination, out charsConsumed, out charsWritten, isFinalBlock);
            }

            private protected override OperationStatus EncodeUtf8Core(ReadOnlySpan<byte> utf8Source, Span<byte> utf8Destination, out int bytesConsumed, out int bytesWritten, bool isFinalBlock)
            {
                return _innerEncoder.EncodeUtf8(utf8Source, utf8Destination, out bytesConsumed, out bytesWritten, isFinalBlock);
            }

            private protected override int FindFirstCharacterToEncode(ReadOnlySpan<char> text)
            {
                return _innerEncoder.GetIndexOfFirstCharToEncode(text);
            }

            public unsafe override int FindFirstCharacterToEncode(char* text, int textLength)
            {
                return _innerEncoder.FindFirstCharacterToEncode(text, textLength);
            }

            public override int FindFirstCharacterToEncodeUtf8(ReadOnlySpan<byte> utf8Text)
            {
                return _innerEncoder.GetIndexOfFirstByteToEncode(utf8Text);
            }

            public unsafe override bool TryEncodeUnicodeScalar(int unicodeScalar, char* buffer, int bufferLength, out int numberOfCharactersWritten)
            {
                return _innerEncoder.TryEncodeUnicodeScalar(unicodeScalar, buffer, bufferLength, out numberOfCharactersWritten);
            }

            public override bool WillEncode(int unicodeScalar)
            {
                return !_innerEncoder.IsScalarValueAllowed(new Rune(unicodeScalar));
            }
        }

        internal sealed class DefaultJavaScriptEncoder : JavaScriptEncoder
        {
            private sealed class EscaperImplementation : ScalarEscaperBase
            {
                internal static readonly EscaperImplementation Singleton = new EscaperImplementation(allowMinimalEscaping: false);

                internal static readonly EscaperImplementation SingletonMinimallyEscaped = new EscaperImplementation(allowMinimalEscaping: true);

                private readonly AsciiByteMap _preescapedMap;

                private EscaperImplementation(bool allowMinimalEscaping)
                {
                    _preescapedMap.InsertAsciiChar('\b', 98);
                    _preescapedMap.InsertAsciiChar('\t', 116);
                    _preescapedMap.InsertAsciiChar('\n', 110);
                    _preescapedMap.InsertAsciiChar('\f', 102);
                    _preescapedMap.InsertAsciiChar('\r', 114);
                    _preescapedMap.InsertAsciiChar('\\', 92);
                    if (allowMinimalEscaping)
                    {
                        _preescapedMap.InsertAsciiChar('"', 34);
                    }
                }

                internal override int EncodeUtf8(Rune value, Span<byte> destination)
                {
                    if (_preescapedMap.TryLookup(value, out var value2))
                    {
                        if (SpanUtility.IsValidIndex(destination, 1))
                        {
                            destination[0] = 92;
                            destination[1] = value2;
                            return 2;
                        }
                        return -1;
                    }
                    return TryEncodeScalarAsHex(this, value, destination);
                    static int TryEncodeScalarAsHex(object @this, Rune value, Span<byte> destination)
                    {
                        if (value.IsBmp)
                        {
                            if (SpanUtility.IsValidIndex(destination, 5))
                            {
                                destination[0] = 92;
                                destination[1] = 117;
                                HexConverter.ToBytesBuffer((byte)value.Value, destination, 4);
                                HexConverter.ToBytesBuffer((byte)((uint)value.Value >> 8), destination, 2);
                                return 6;
                            }
                        }
                        else
                        {
                            UnicodeHelpers.GetUtf16SurrogatePairFromAstralScalarValue((uint)value.Value, out var highSurrogate, out var lowSurrogate);
                            if (SpanUtility.IsValidIndex(destination, 11))
                            {
                                destination[0] = 92;
                                destination[1] = 117;
                                HexConverter.ToBytesBuffer((byte)highSurrogate, destination, 4);
                                HexConverter.ToBytesBuffer((byte)((uint)highSurrogate >> 8), destination, 2);
                                destination[6] = 92;
                                destination[7] = 117;
                                HexConverter.ToBytesBuffer((byte)lowSurrogate, destination, 10);
                                HexConverter.ToBytesBuffer((byte)((uint)lowSurrogate >> 8), destination, 8);
                                return 12;
                            }
                        }
                        return -1;
                    }
                }

                internal override int EncodeUtf16(Rune value, Span<char> destination)
                {
                    if (_preescapedMap.TryLookup(value, out var value2))
                    {
                        if (SpanUtility.IsValidIndex(destination, 1))
                        {
                            destination[0] = '\\';
                            destination[1] = (char)value2;
                            return 2;
                        }
                        return -1;
                    }
                    return TryEncodeScalarAsHex(this, value, destination);
                    static int TryEncodeScalarAsHex(object @this, Rune value, Span<char> destination)
                    {
                        if (value.IsBmp)
                        {
                            if (SpanUtility.IsValidIndex(destination, 5))
                            {
                                destination[0] = '\\';
                                destination[1] = 'u';
                                HexConverter.ToCharsBuffer((byte)value.Value, destination, 4);
                                HexConverter.ToCharsBuffer((byte)((uint)value.Value >> 8), destination, 2);
                                return 6;
                            }
                        }
                        else
                        {
                            UnicodeHelpers.GetUtf16SurrogatePairFromAstralScalarValue((uint)value.Value, out var highSurrogate, out var lowSurrogate);
                            if (SpanUtility.IsValidIndex(destination, 11))
                            {
                                destination[0] = '\\';
                                destination[1] = 'u';
                                HexConverter.ToCharsBuffer((byte)highSurrogate, destination, 4);
                                HexConverter.ToCharsBuffer((byte)((uint)highSurrogate >> 8), destination, 2);
                                destination[6] = '\\';
                                destination[7] = 'u';
                                HexConverter.ToCharsBuffer((byte)lowSurrogate, destination, 10);
                                HexConverter.ToCharsBuffer((byte)((uint)lowSurrogate >> 8), destination, 8);
                                return 12;
                            }
                        }
                        return -1;
                    }
                }
            }

            internal static readonly DefaultJavaScriptEncoder BasicLatinSingleton = new DefaultJavaScriptEncoder(new TextEncoderSettings(UnicodeRanges.BasicLatin));

            internal static readonly DefaultJavaScriptEncoder UnsafeRelaxedEscapingSingleton = new DefaultJavaScriptEncoder(new TextEncoderSettings(UnicodeRanges.All), allowMinimalJsonEscaping: true);

            private readonly OptimizedInboxTextEncoder _innerEncoder;

            public override int MaxOutputCharactersPerInputCharacter => 6;

            internal DefaultJavaScriptEncoder(TextEncoderSettings settings)
                : this(settings, allowMinimalJsonEscaping: false)
            {
            }

            private DefaultJavaScriptEncoder(TextEncoderSettings settings, bool allowMinimalJsonEscaping)
            {
                if (settings == null)
                {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.settings);
                }
                OptimizedInboxTextEncoder innerEncoder;
                if (allowMinimalJsonEscaping)
                {
                    ScalarEscaperBase singletonMinimallyEscaped = EscaperImplementation.SingletonMinimallyEscaped;
                    ref readonly AllowedBmpCodePointsBitmap allowedCodePointsBitmap = ref settings.GetAllowedCodePointsBitmap();
                    Span<char> span = stackalloc char[2] { '"', '\\' };
                    innerEncoder = new OptimizedInboxTextEncoder(singletonMinimallyEscaped, in allowedCodePointsBitmap, forbidHtmlSensitiveCharacters: false, span);
                }
                else
                {
                    ScalarEscaperBase singleton = EscaperImplementation.Singleton;
                    ref readonly AllowedBmpCodePointsBitmap allowedCodePointsBitmap2 = ref settings.GetAllowedCodePointsBitmap();
                    Span<char> span = stackalloc char[2] { '\\', '`' };
                    innerEncoder = new OptimizedInboxTextEncoder(singleton, in allowedCodePointsBitmap2, forbidHtmlSensitiveCharacters: true, span);
                }
                _innerEncoder = innerEncoder;
            }

            private protected override OperationStatus EncodeCore(ReadOnlySpan<char> source, Span<char> destination, out int charsConsumed, out int charsWritten, bool isFinalBlock)
            {
                return _innerEncoder.Encode(source, destination, out charsConsumed, out charsWritten, isFinalBlock);
            }

            private protected override OperationStatus EncodeUtf8Core(ReadOnlySpan<byte> utf8Source, Span<byte> utf8Destination, out int bytesConsumed, out int bytesWritten, bool isFinalBlock)
            {
                return _innerEncoder.EncodeUtf8(utf8Source, utf8Destination, out bytesConsumed, out bytesWritten, isFinalBlock);
            }

            private protected override int FindFirstCharacterToEncode(ReadOnlySpan<char> text)
            {
                return _innerEncoder.GetIndexOfFirstCharToEncode(text);
            }

            public unsafe override int FindFirstCharacterToEncode(char* text, int textLength)
            {
                return _innerEncoder.FindFirstCharacterToEncode(text, textLength);
            }

            public override int FindFirstCharacterToEncodeUtf8(ReadOnlySpan<byte> utf8Text)
            {
                return _innerEncoder.GetIndexOfFirstByteToEncode(utf8Text);
            }

            public unsafe override bool TryEncodeUnicodeScalar(int unicodeScalar, char* buffer, int bufferLength, out int numberOfCharactersWritten)
            {
                return _innerEncoder.TryEncodeUnicodeScalar(unicodeScalar, buffer, bufferLength, out numberOfCharactersWritten);
            }

            public override bool WillEncode(int unicodeScalar)
            {
                return !_innerEncoder.IsScalarValueAllowed(new Rune(unicodeScalar));
            }
        }

        internal sealed class DefaultUrlEncoder : UrlEncoder
        {
            private sealed class EscaperImplementation : ScalarEscaperBase
            {
                internal static readonly EscaperImplementation Singleton = new EscaperImplementation();

                private EscaperImplementation()
                {
                }

                internal override int EncodeUtf8(Rune value, Span<byte> destination)
                {
                    uint utf8RepresentationForScalarValue = (uint)UnicodeHelpers.GetUtf8RepresentationForScalarValue((uint)value.Value);
                    if (SpanUtility.IsValidIndex(destination, 2))
                    {
                        destination[0] = 37;
                        HexConverter.ToBytesBuffer((byte)utf8RepresentationForScalarValue, destination, 1);
                        if ((utf8RepresentationForScalarValue >>= 8) == 0)
                        {
                            return 3;
                        }
                        if (SpanUtility.IsValidIndex(destination, 5))
                        {
                            destination[3] = 37;
                            HexConverter.ToBytesBuffer((byte)utf8RepresentationForScalarValue, destination, 4);
                            if ((utf8RepresentationForScalarValue >>= 8) == 0)
                            {
                                return 6;
                            }
                            if (SpanUtility.IsValidIndex(destination, 8))
                            {
                                destination[6] = 37;
                                HexConverter.ToBytesBuffer((byte)utf8RepresentationForScalarValue, destination, 7);
                                if ((utf8RepresentationForScalarValue >>= 8) == 0)
                                {
                                    return 9;
                                }
                                if (SpanUtility.IsValidIndex(destination, 11))
                                {
                                    destination[9] = 37;
                                    HexConverter.ToBytesBuffer((byte)utf8RepresentationForScalarValue, destination, 10);
                                    return 12;
                                }
                            }
                        }
                    }
                    return -1;
                }

                internal override int EncodeUtf16(Rune value, Span<char> destination)
                {
                    uint utf8RepresentationForScalarValue = (uint)UnicodeHelpers.GetUtf8RepresentationForScalarValue((uint)value.Value);
                    if (SpanUtility.IsValidIndex(destination, 2))
                    {
                        destination[0] = '%';
                        HexConverter.ToCharsBuffer((byte)utf8RepresentationForScalarValue, destination, 1);
                        if ((utf8RepresentationForScalarValue >>= 8) == 0)
                        {
                            return 3;
                        }
                        if (SpanUtility.IsValidIndex(destination, 5))
                        {
                            destination[3] = '%';
                            HexConverter.ToCharsBuffer((byte)utf8RepresentationForScalarValue, destination, 4);
                            if ((utf8RepresentationForScalarValue >>= 8) == 0)
                            {
                                return 6;
                            }
                            if (SpanUtility.IsValidIndex(destination, 8))
                            {
                                destination[6] = '%';
                                HexConverter.ToCharsBuffer((byte)utf8RepresentationForScalarValue, destination, 7);
                                if ((utf8RepresentationForScalarValue >>= 8) == 0)
                                {
                                    return 9;
                                }
                                if (SpanUtility.IsValidIndex(destination, 11))
                                {
                                    destination[9] = '%';
                                    HexConverter.ToCharsBuffer((byte)utf8RepresentationForScalarValue, destination, 10);
                                    return 12;
                                }
                            }
                        }
                    }
                    return -1;
                }
            }

            internal static readonly DefaultUrlEncoder BasicLatinSingleton = new DefaultUrlEncoder(new TextEncoderSettings(UnicodeRanges.BasicLatin));

            private readonly OptimizedInboxTextEncoder _innerEncoder;

            public override int MaxOutputCharactersPerInputCharacter => 9;

            internal DefaultUrlEncoder(TextEncoderSettings settings)
            {
                if (settings == null)
                {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.settings);
                }
                ScalarEscaperBase singleton = EscaperImplementation.Singleton;
                ref readonly AllowedBmpCodePointsBitmap allowedCodePointsBitmap = ref settings.GetAllowedCodePointsBitmap();
                Span<char> span = stackalloc char[31]
                {
                    ' ',
                    '#',
                    '%',
                    '/',
                    ':',
                    '=',
                    '?',
                    '[',
                    '\\',
                    ']',
                    '^',
                    '`',
                    '{',
                    '|',
                    '}',
                    '\ufff0',
                    '\ufff1',
                    '\ufff2',
                    '\ufff3',
                    '\ufff4',
                    '\ufff5',
                    '\ufff6',
                    '\ufff7',
                    '\ufff8',
                    '\ufff9',
                    '\ufffa',
                    '\ufffb',
                    '￼',
                    '\ufffd',
                    '\ufffe',
                    '\uffff'
                };
                _innerEncoder = new OptimizedInboxTextEncoder(singleton, in allowedCodePointsBitmap, forbidHtmlSensitiveCharacters: true, span);
            }

            private protected override OperationStatus EncodeCore(ReadOnlySpan<char> source, Span<char> destination, out int charsConsumed, out int charsWritten, bool isFinalBlock)
            {
                return _innerEncoder.Encode(source, destination, out charsConsumed, out charsWritten, isFinalBlock);
            }

            private protected override OperationStatus EncodeUtf8Core(ReadOnlySpan<byte> utf8Source, Span<byte> utf8Destination, out int bytesConsumed, out int bytesWritten, bool isFinalBlock)
            {
                return _innerEncoder.EncodeUtf8(utf8Source, utf8Destination, out bytesConsumed, out bytesWritten, isFinalBlock);
            }

            private protected override int FindFirstCharacterToEncode(ReadOnlySpan<char> text)
            {
                return _innerEncoder.GetIndexOfFirstCharToEncode(text);
            }

            public unsafe override int FindFirstCharacterToEncode(char* text, int textLength)
            {
                return _innerEncoder.FindFirstCharacterToEncode(text, textLength);
            }

            public override int FindFirstCharacterToEncodeUtf8(ReadOnlySpan<byte> utf8Text)
            {
                return _innerEncoder.GetIndexOfFirstByteToEncode(utf8Text);
            }

            public unsafe override bool TryEncodeUnicodeScalar(int unicodeScalar, char* buffer, int bufferLength, out int numberOfCharactersWritten)
            {
                return _innerEncoder.TryEncodeUnicodeScalar(unicodeScalar, buffer, bufferLength, out numberOfCharactersWritten);
            }

            public override bool WillEncode(int unicodeScalar)
            {
                return !_innerEncoder.IsScalarValueAllowed(new Rune(unicodeScalar));
            }
        }

        internal enum ExceptionArgument
        {
            value,
            settings,
            output,
            other,
            allowedRanges,
            characters,
            codePoints,
            range,
            ranges
        }

        /// <summary>Represents an HTML character encoding.</summary>
        public abstract class HtmlEncoder : TextEncoder
        {
            /// <summary>Gets a built-in instance of the <see cref="T:System.Text.Encodings.Web.HtmlEncoder" /> class.</summary>
            /// <returns>A built-in instance of the <see cref="T:System.Text.Encodings.Web.HtmlEncoder" /> class.</returns>
            public static HtmlEncoder Default => DefaultHtmlEncoder.BasicLatinSingleton;

            /// <summary>Creates a new instance of the HtmlEncoder class with the specified settings.</summary>
            /// <param name="settings">Settings that control how the <see cref="T:System.Text.Encodings.Web.HtmlEncoder" /> instance encodes, primarily which characters to encode.</param>
            /// <exception cref="T:System.ArgumentNullException">
            ///   <paramref name="settings" /> is <see langword="null" />.</exception>
            /// <returns>A new instance of the <see cref="T:System.Text.Encodings.Web.HtmlEncoder" /> class.</returns>
            public static HtmlEncoder Create(TextEncoderSettings settings)
            {
                return new DefaultHtmlEncoder(settings);
            }

            /// <summary>Creates a new instance of the HtmlEncoder class that specifies characters the encoder is allowed to not encode.</summary>
            /// <param name="allowedRanges">The set of characters that the encoder is allowed to not encode.</param>
            /// <exception cref="T:System.ArgumentNullException">
            ///   <paramref name="allowedRanges" /> is <see langword="null" />.</exception>
            /// <returns>A new instance of the <see cref="T:System.Text.Encodings.Web.HtmlEncoder" /> class.</returns>
            public static HtmlEncoder Create(params UnicodeRange[] allowedRanges)
            {
                return new DefaultHtmlEncoder(new TextEncoderSettings(allowedRanges));
            }

            /// <summary>Initializes a new instance of the <see cref="T:System.Text.Encodings.Web.HtmlEncoder" /> class.</summary>
            protected HtmlEncoder()
            {
            }
        }

        /// <summary>Represents a JavaScript character encoding.</summary>
        public abstract class JavaScriptEncoder : TextEncoder
        {
            /// <summary>Gets a built-in instance of the <see cref="T:System.Text.Encodings.Web.JavaScriptEncoder" /> class.</summary>
            /// <returns>A built-in instance of the <see cref="T:System.Text.Encodings.Web.JavaScriptEncoder" /> class.</returns>
            public static JavaScriptEncoder Default => DefaultJavaScriptEncoder.BasicLatinSingleton;

            /// <summary>Gets a built-in JavaScript encoder instance that is less strict about what is encoded.</summary>
            /// <returns>A JavaScript encoder instance.</returns>
            public static JavaScriptEncoder UnsafeRelaxedJsonEscaping => DefaultJavaScriptEncoder.UnsafeRelaxedEscapingSingleton;

            /// <summary>Creates a new instance of JavaScriptEncoder class with the specified settings.</summary>
            /// <param name="settings">Settings that control how the <see cref="T:System.Text.Encodings.Web.JavaScriptEncoder" /> instance encodes, primarily which characters to encode.</param>
            /// <exception cref="T:System.ArgumentNullException">
            ///   <paramref name="settings" /> is <see langword="null" />.</exception>
            /// <returns>A new instance of the <see cref="T:System.Text.Encodings.Web.JavaScriptEncoder" /> class.</returns>
            public static JavaScriptEncoder Create(TextEncoderSettings settings)
            {
                return new DefaultJavaScriptEncoder(settings);
            }

            /// <summary>Creates a new instance of the JavaScriptEncoder class that specifies characters the encoder is allowed to not encode.</summary>
            /// <param name="allowedRanges">The set of characters that the encoder is allowed to not encode.</param>
            /// <exception cref="T:System.ArgumentNullException">
            ///   <paramref name="allowedRanges" /> is <see langword="null" />.</exception>
            /// <returns>A new instance of the <see cref="T:System.Text.Encodings.Web.JavaScriptEncoder" /> class.</returns>
            public static JavaScriptEncoder Create(params UnicodeRange[] allowedRanges)
            {
                return new DefaultJavaScriptEncoder(new TextEncoderSettings(allowedRanges));
            }

            /// <summary>Initializes a new instance of the <see cref="T:System.Text.Encodings.Web.JavaScriptEncoder" /> class.</summary>
            protected JavaScriptEncoder()
            {
            }
        }

        internal sealed class OptimizedInboxTextEncoder
        {
            [StructLayout(LayoutKind.Explicit)]
            private struct AllowedAsciiCodePoints
            {
                [FieldOffset(0)]
                private unsafe fixed byte AsBytes[16];

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal unsafe readonly bool IsAllowedAsciiCodePoint(uint codePoint)
                {
                    if (codePoint > 127)
                    {
                        return false;
                    }
                    uint num = AsBytes[codePoint & 0xF];
                    if ((num & (uint)(1 << (int)(codePoint >> 4))) == 0)
                    {
                        return false;
                    }
                    return true;
                }

                internal unsafe void PopulateAllowedCodePoints(in AllowedBmpCodePointsBitmap allowedBmpCodePoints)
                {
                    this = default(AllowedAsciiCodePoints);
                    for (int i = 32; i < 127; i++)
                    {
                        if (allowedBmpCodePoints.IsCharAllowed((char)i))
                        {
                            ref byte reference = ref AsBytes[i & 0xF];
                            reference = (byte)(reference | (byte)(1 << (i >> 4)));
                        }
                    }
                }
            }

            private struct AsciiPreescapedData
            {
                private unsafe fixed ulong Data[128];

                internal unsafe void PopulatePreescapedData(in AllowedBmpCodePointsBitmap allowedCodePointsBmp, ScalarEscaperBase innerEncoder)
                {
                    this = default(AsciiPreescapedData);
                    byte* intPtr = stackalloc byte[16];
                    // IL initblk instruction
                    System.Runtime.CompilerServices.Unsafe.InitBlock(intPtr, 0, 16);
                    Span<char> span = new Span<char>(intPtr, 8);
                    Span<char> span2 = span;
                    for (int i = 0; i < 128; i++)
                    {
                        Rune value = new Rune(i);
                        ulong num;
                        int num2;
                        if (!Rune.IsControl(value) && allowedCodePointsBmp.IsCharAllowed((char)i))
                        {
                            num = (uint)i;
                            num2 = 1;
                        }
                        else
                        {
                            num2 = innerEncoder.EncodeUtf16(value, span2.Slice(0, 6));
                            num = 0uL;
                            span2.Slice(num2).Clear();
                            for (int num3 = num2 - 1; num3 >= 0; num3--)
                            {
                                uint num4 = span2[num3];
                                num = (num << 8) | num4;
                            }
                        }
                        Data[i] = num | ((ulong)(uint)num2 << 56);
                    }
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal unsafe readonly bool TryGetPreescapedData(uint codePoint, out ulong preescapedData)
                {
                    if (codePoint <= 127)
                    {
                        preescapedData = Data[codePoint];
                        return true;
                    }
                    preescapedData = 0uL;
                    return false;
                }
            }

            private readonly AllowedAsciiCodePoints _allowedAsciiCodePoints;

            private readonly AsciiPreescapedData _asciiPreescapedData;

            private readonly AllowedBmpCodePointsBitmap _allowedBmpCodePoints;

            private readonly ScalarEscaperBase _scalarEscaper;

            internal OptimizedInboxTextEncoder(ScalarEscaperBase scalarEscaper, in AllowedBmpCodePointsBitmap allowedCodePointsBmp, bool forbidHtmlSensitiveCharacters = true, ReadOnlySpan<char> extraCharactersToEscape = default(ReadOnlySpan<char>))
            {
                _scalarEscaper = scalarEscaper;
                _allowedBmpCodePoints = allowedCodePointsBmp;
                _allowedBmpCodePoints.ForbidUndefinedCharacters();
                if (forbidHtmlSensitiveCharacters)
                {
                    _allowedBmpCodePoints.ForbidHtmlCharacters();
                }
                ReadOnlySpan<char> readOnlySpan = extraCharactersToEscape;
                for (int i = 0; i < readOnlySpan.Length; i++)
                {
                    char value = readOnlySpan[i];
                    _allowedBmpCodePoints.ForbidChar(value);
                }
                _asciiPreescapedData.PopulatePreescapedData(in _allowedBmpCodePoints, scalarEscaper);
                _allowedAsciiCodePoints.PopulateAllowedCodePoints(in _allowedBmpCodePoints);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            [Obsolete("FindFirstCharacterToEncode has been deprecated. It should only be used by the TextEncoder adapter.")]
            public unsafe int FindFirstCharacterToEncode(char* text, int textLength)
            {
                return GetIndexOfFirstCharToEncode(new ReadOnlySpan<char>(text, textLength));
            }

            [Obsolete("TryEncodeUnicodeScalar has been deprecated. It should only be used by the TextEncoder adapter.")]
            public unsafe bool TryEncodeUnicodeScalar(int unicodeScalar, char* buffer, int bufferLength, out int numberOfCharactersWritten)
            {
                Span<char> destination = new Span<char>(buffer, bufferLength);
                if (_allowedBmpCodePoints.IsCodePointAllowed((uint)unicodeScalar))
                {
                    if (!destination.IsEmpty)
                    {
                        destination[0] = (char)unicodeScalar;
                        numberOfCharactersWritten = 1;
                        return true;
                    }
                }
                else
                {
                    int num = _scalarEscaper.EncodeUtf16(new Rune(unicodeScalar), destination);
                    if (num >= 0)
                    {
                        numberOfCharactersWritten = num;
                        return true;
                    }
                }
                numberOfCharactersWritten = 0;
                return false;
            }

            public OperationStatus Encode(ReadOnlySpan<char> source, Span<char> destination, out int charsConsumed, out int charsWritten, bool isFinalBlock)
            {
                _AssertThisNotNull();
                int num = 0;
                int num2 = 0;
                OperationStatus result2;
                while (true)
                {
                    int num3;
                    Rune result;
                    if (SpanUtility.IsValidIndex(source, num))
                    {
                        char c = source[num];
                        if (_asciiPreescapedData.TryGetPreescapedData(c, out var preescapedData))
                        {
                            if (SpanUtility.IsValidIndex(destination, num2))
                            {
                                destination[num2] = (char)(byte)preescapedData;
                                if (((int)preescapedData & 0xFF00) == 0)
                                {
                                    num2++;
                                    num++;
                                    continue;
                                }
                                preescapedData >>= 8;
                                num3 = num2 + 1;
                                while (SpanUtility.IsValidIndex(destination, num3))
                                {
                                    destination[num3++] = (char)(byte)preescapedData;
                                    if ((byte)(preescapedData >>= 8) != 0)
                                    {
                                        continue;
                                    }
                                    goto IL_0091;
                                }
                            }
                            goto IL_0148;
                        }
                        if (Rune.TryCreate(c, out result))
                        {
                            goto IL_00e1;
                        }
                        int index = num + 1;
                        if (SpanUtility.IsValidIndex(source, index))
                        {
                            if (Rune.TryCreate(c, source[index], out result))
                            {
                                goto IL_00e1;
                            }
                        }
                        else if (!isFinalBlock && char.IsHighSurrogate(c))
                        {
                            result2 = OperationStatus.NeedMoreData;
                            break;
                        }
                        result = Rune.ReplacementChar;
                        goto IL_010d;
                    }
                    result2 = OperationStatus.Done;
                    break;
                IL_0148:
                    result2 = OperationStatus.DestinationTooSmall;
                    break;
                IL_0091:
                    num2 = num3;
                    num++;
                    continue;
                IL_010d:
                    int num4 = _scalarEscaper.EncodeUtf16(result, destination.Slice(num2));
                    if (num4 >= 0)
                    {
                        num2 += num4;
                        num += result.Utf16SequenceLength;
                        continue;
                    }
                    goto IL_0148;
                IL_00e1:
                    if (!IsScalarValueAllowed(result))
                    {
                        goto IL_010d;
                    }
                    if (result.TryEncodeToUtf16(destination.Slice(num2), out var charsWritten2))
                    {
                        num2 += charsWritten2;
                        num += charsWritten2;
                        continue;
                    }
                    goto IL_0148;
                }
                charsConsumed = num;
                charsWritten = num2;
                return result2;
            }

            public OperationStatus EncodeUtf8(ReadOnlySpan<byte> source, Span<byte> destination, out int bytesConsumed, out int bytesWritten, bool isFinalBlock)
            {
                _AssertThisNotNull();
                int num = 0;
                int num2 = 0;
                OperationStatus result2;
                while (true)
                {
                    int num3;
                    if (SpanUtility.IsValidIndex(source, num))
                    {
                        uint codePoint = source[num];
                        if (_asciiPreescapedData.TryGetPreescapedData(codePoint, out var preescapedData))
                        {
                            if (SpanUtility.TryWriteUInt64LittleEndian(destination, num2, preescapedData))
                            {
                                num2 += (int)(preescapedData >> 56);
                                num++;
                                continue;
                            }
                            num3 = num2;
                            while (SpanUtility.IsValidIndex(destination, num3))
                            {
                                destination[num3++] = (byte)preescapedData;
                                if ((byte)(preescapedData >>= 8) != 0)
                                {
                                    continue;
                                }
                                goto IL_0076;
                            }
                        }
                        else
                        {
                            Rune result;
                            int bytesConsumed2;
                            OperationStatus operationStatus = Rune.DecodeFromUtf8(source.Slice(num), out result, out bytesConsumed2);
                            if (operationStatus != 0)
                            {
                                if (!isFinalBlock && operationStatus == OperationStatus.NeedMoreData)
                                {
                                    result2 = OperationStatus.NeedMoreData;
                                    break;
                                }
                            }
                            else if (IsScalarValueAllowed(result))
                            {
                                if (result.TryEncodeToUtf8(destination.Slice(num2), out var bytesWritten2))
                                {
                                    num2 += bytesWritten2;
                                    num += bytesWritten2;
                                    continue;
                                }
                                goto IL_0103;
                            }
                            int num4 = _scalarEscaper.EncodeUtf8(result, destination.Slice(num2));
                            if (num4 >= 0)
                            {
                                num2 += num4;
                                num += bytesConsumed2;
                                continue;
                            }
                        }
                        goto IL_0103;
                    }
                    result2 = OperationStatus.Done;
                    break;
                IL_0076:
                    num2 = num3;
                    num++;
                    continue;
                IL_0103:
                    result2 = OperationStatus.DestinationTooSmall;
                    break;
                }
                bytesConsumed = num;
                bytesWritten = num2;
                return result2;
            }

            public int GetIndexOfFirstByteToEncode(ReadOnlySpan<byte> data)
            {
                int length = data.Length;
                Rune result;
                int bytesConsumed;
                while (!data.IsEmpty && Rune.DecodeFromUtf8(data, out result, out bytesConsumed) == OperationStatus.Done && bytesConsumed < 4 && _allowedBmpCodePoints.IsCharAllowed((char)result.Value))
                {
                    data = data.Slice(bytesConsumed);
                }
                if (!data.IsEmpty)
                {
                    return length - data.Length;
                }
                return -1;
            }

            public unsafe int GetIndexOfFirstCharToEncode(ReadOnlySpan<char> data)
            {
                fixed (char* ptr = data)
                {
                    nuint num = (uint)data.Length;
                    nuint num2 = 0u;
                    if (num2 < num)
                    {
                        _AssertThisNotNull();
                        nint num3 = 0;
                        while (true)
                        {
                            if (num - num2 >= 8)
                            {
                                num3 = -1;
                                if (_allowedBmpCodePoints.IsCharAllowed(ptr[(nuint)((nint)num2 + ++num3)]) && _allowedBmpCodePoints.IsCharAllowed(ptr[(nuint)((nint)num2 + ++num3)]) && _allowedBmpCodePoints.IsCharAllowed(ptr[(nuint)((nint)num2 + ++num3)]) && _allowedBmpCodePoints.IsCharAllowed(ptr[(nuint)((nint)num2 + ++num3)]) && _allowedBmpCodePoints.IsCharAllowed(ptr[(nuint)((nint)num2 + ++num3)]) && _allowedBmpCodePoints.IsCharAllowed(ptr[(nuint)((nint)num2 + ++num3)]) && _allowedBmpCodePoints.IsCharAllowed(ptr[(nuint)((nint)num2 + ++num3)]) && _allowedBmpCodePoints.IsCharAllowed(ptr[(nuint)((nint)num2 + ++num3)]))
                                {
                                    num2 += 8;
                                    continue;
                                }
                                num2 += (nuint)num3;
                                break;
                            }
                            for (; num2 < num && _allowedBmpCodePoints.IsCharAllowed(ptr[num2]); num2++)
                            {
                            }
                            break;
                        }
                    }
                    int num4 = (int)num2;
                    if (num4 == (int)num)
                    {
                        num4 = -1;
                    }
                    return num4;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool IsScalarValueAllowed(Rune value)
            {
                return _allowedBmpCodePoints.IsCodePointAllowed((uint)value.Value);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void _AssertThisNotNull()
            {
                _ = GetType() == typeof(OptimizedInboxTextEncoder);
            }
        }

        internal abstract class ScalarEscaperBase
        {
            internal abstract int EncodeUtf16(Rune value, Span<char> destination);

            internal abstract int EncodeUtf8(Rune value, Span<byte> destination);
        }

        internal static class SpanUtility
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool IsValidIndex<T>(ReadOnlySpan<T> span, int index)
            {
                if ((uint)index >= (uint)span.Length)
                {
                    return false;
                }
                return true;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool IsValidIndex<T>(Span<T> span, int index)
            {
                if ((uint)index >= (uint)span.Length)
                {
                    return false;
                }
                return true;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool TryWriteBytes(Span<byte> span, byte a, byte b, byte c, byte d)
            {
                if (span.Length >= 4)
                {
                    uint num = (uint)((!BitConverter.IsLittleEndian) ? ((a << 24) | (b << 16) | (c << 8) | d) : ((d << 24) | (c << 16) | (b << 8) | a));
                    System.Runtime.CompilerServices.Unsafe.WriteUnaligned<uint>(ref MemoryMarshal.GetReference(span), num);
                    return true;
                }
                return false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool TryWriteBytes(Span<byte> span, byte a, byte b, byte c, byte d, byte e)
            {
                if (span.Length >= 5)
                {
                    uint num = (uint)((!BitConverter.IsLittleEndian) ? ((a << 24) | (b << 16) | (c << 8) | d) : ((d << 24) | (c << 16) | (b << 8) | a));
                    ref byte reference = ref MemoryMarshal.GetReference(span);
                    System.Runtime.CompilerServices.Unsafe.WriteUnaligned<uint>(ref reference, num);
                    System.Runtime.CompilerServices.Unsafe.Add<byte>(ref reference, 4) = e;
                    return true;
                }
                return false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool TryWriteBytes(Span<byte> span, byte a, byte b, byte c, byte d, byte e, byte f)
            {
                if (span.Length >= 6)
                {
                    uint num;
                    uint num2;
                    if (BitConverter.IsLittleEndian)
                    {
                        num = (uint)((d << 24) | (c << 16) | (b << 8) | a);
                        num2 = (uint)((f << 8) | e);
                    }
                    else
                    {
                        num = (uint)((a << 24) | (b << 16) | (c << 8) | d);
                        num2 = (uint)((e << 8) | f);
                    }
                    ref byte reference = ref MemoryMarshal.GetReference(span);
                    System.Runtime.CompilerServices.Unsafe.WriteUnaligned<uint>(ref reference, num);
                    System.Runtime.CompilerServices.Unsafe.WriteUnaligned<ushort>(ref System.Runtime.CompilerServices.Unsafe.Add<byte>(ref reference, 4), (ushort)num2);
                    return true;
                }
                return false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool TryWriteChars(Span<char> span, char a, char b, char c, char d)
            {
                if (span.Length >= 4)
                {
                    ulong num = ((!BitConverter.IsLittleEndian) ? (((ulong)a << 48) | ((ulong)b << 32) | ((ulong)c << 16) | d) : (((ulong)d << 48) | ((ulong)c << 32) | ((ulong)b << 16) | a));
                    System.Runtime.CompilerServices.Unsafe.WriteUnaligned<ulong>(ref System.Runtime.CompilerServices.Unsafe.As<char, byte>(ref MemoryMarshal.GetReference(span)), num);
                    return true;
                }
                return false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool TryWriteChars(Span<char> span, char a, char b, char c, char d, char e)
            {
                if (span.Length >= 5)
                {
                    ulong num = ((!BitConverter.IsLittleEndian) ? (((ulong)a << 48) | ((ulong)b << 32) | ((ulong)c << 16) | d) : (((ulong)d << 48) | ((ulong)c << 32) | ((ulong)b << 16) | a));
                    ref char reference = ref MemoryMarshal.GetReference(span);
                    System.Runtime.CompilerServices.Unsafe.WriteUnaligned<ulong>(ref System.Runtime.CompilerServices.Unsafe.As<char, byte>(ref reference), num);
                    System.Runtime.CompilerServices.Unsafe.Add<char>(ref reference, 4) = e;
                    return true;
                }
                return false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool TryWriteChars(Span<char> span, char a, char b, char c, char d, char e, char f)
            {
                if (span.Length >= 6)
                {
                    ulong num;
                    uint num2;
                    if (BitConverter.IsLittleEndian)
                    {
                        num = ((ulong)d << 48) | ((ulong)c << 32) | ((ulong)b << 16) | a;
                        num2 = ((uint)f << 16) | e;
                    }
                    else
                    {
                        num = ((ulong)a << 48) | ((ulong)b << 32) | ((ulong)c << 16) | d;
                        num2 = ((uint)e << 16) | f;
                    }
                    ref byte reference = ref System.Runtime.CompilerServices.Unsafe.As<char, byte>(ref MemoryMarshal.GetReference(span));
                    System.Runtime.CompilerServices.Unsafe.WriteUnaligned<ulong>(ref reference, num);
                    System.Runtime.CompilerServices.Unsafe.WriteUnaligned<uint>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset<byte>(ref reference, (IntPtr)8), num2);
                    return true;
                }
                return false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool TryWriteUInt64LittleEndian(Span<byte> span, int offset, ulong value)
            {
                if (AreValidIndexAndLength(span.Length, offset, 8))
                {
                    if (!BitConverter.IsLittleEndian)
                    {
                        value = BinaryPrimitives.ReverseEndianness(value);
                    }
                    System.Runtime.CompilerServices.Unsafe.WriteUnaligned<ulong>(ref System.Runtime.CompilerServices.Unsafe.Add<byte>(ref MemoryMarshal.GetReference(span), (IntPtr)(nint)(uint)offset), value);
                    return true;
                }
                return false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static bool AreValidIndexAndLength(int spanRealLength, int requestedOffset, int requestedLength)
            {
                if (IntPtr.Size == 4)
                {
                    if ((uint)requestedOffset > (uint)spanRealLength)
                    {
                        return false;
                    }
                    if ((uint)requestedLength > (uint)(spanRealLength - requestedOffset))
                    {
                        return false;
                    }
                }
                else if ((ulong)(uint)spanRealLength < (ulong)((long)(uint)requestedOffset + (long)(uint)requestedLength))
                {
                    return false;
                }
                return true;
            }
        }

        /// <summary>The base class of web encoders.</summary>
        public abstract class TextEncoder
        {
            private const int EncodeStartingOutputBufferSize = 1024;

            /// <summary>Gets the maximum number of characters that this encoder can generate for each input code point.</summary>
            /// <returns>The maximum number of characters.</returns>
            [EditorBrowsable(EditorBrowsableState.Never)]
            public abstract int MaxOutputCharactersPerInputCharacter { get; }

            /// <summary>Encodes a Unicode scalar value and writes it to a buffer.</summary>
            /// <param name="unicodeScalar">A Unicode scalar value.</param>
            /// <param name="buffer">A pointer to the buffer to which to write the encoded text.</param>
            /// <param name="bufferLength">The length of the destination <paramref name="buffer" /> in characters.</param>
            /// <param name="numberOfCharactersWritten">When the method returns, indicates the number of characters written to the <paramref name="buffer" />.</param>
            /// <returns>
            ///   <see langword="false" /> if <paramref name="bufferLength" /> is too small to fit the encoded text; otherwise, returns <see langword="true" />.</returns>
            [CLSCompliant(false)]
            [EditorBrowsable(EditorBrowsableState.Never)]
            public unsafe abstract bool TryEncodeUnicodeScalar(int unicodeScalar, char* buffer, int bufferLength, out int numberOfCharactersWritten);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private unsafe bool TryEncodeUnicodeScalar(uint unicodeScalar, Span<char> buffer, out int charsWritten)
            {
                fixed (char* buffer2 = &MemoryMarshal.GetReference(buffer))
                {
                    return TryEncodeUnicodeScalar((int)unicodeScalar, buffer2, buffer.Length, out charsWritten);
                }
            }

            private bool TryEncodeUnicodeScalarUtf8(uint unicodeScalar, Span<char> utf16ScratchBuffer, Span<byte> utf8Destination, out int bytesWritten)
            {
                if (!TryEncodeUnicodeScalar(unicodeScalar, utf16ScratchBuffer, out var charsWritten))
                {
                    ThrowArgumentException_MaxOutputCharsPerInputChar();
                }
                utf16ScratchBuffer = utf16ScratchBuffer.Slice(0, charsWritten);
                int num = 0;
                while (!utf16ScratchBuffer.IsEmpty)
                {
                    if (Rune.DecodeFromUtf16(utf16ScratchBuffer, out var result, out var charsConsumed) != 0)
                    {
                        ThrowArgumentException_MaxOutputCharsPerInputChar();
                    }
                    uint num2 = (uint)UnicodeHelpers.GetUtf8RepresentationForScalarValue((uint)result.Value);
                    do
                    {
                        if (SpanUtility.IsValidIndex(utf8Destination, num))
                        {
                            utf8Destination[num++] = (byte)num2;
                            continue;
                        }
                        bytesWritten = 0;
                        return false;
                    }
                    while ((num2 >>= 8) != 0);
                    utf16ScratchBuffer = utf16ScratchBuffer.Slice(charsConsumed);
                }
                bytesWritten = num;
                return true;
            }

            /// <summary>Finds the index of the first character to encode.</summary>
            /// <param name="text">The text buffer to search.</param>
            /// <param name="textLength">The number of characters in <paramref name="text" />.</param>
            /// <returns>The index of the first character to encode.</returns>
            [CLSCompliant(false)]
            [EditorBrowsable(EditorBrowsableState.Never)]
            public unsafe abstract int FindFirstCharacterToEncode(char* text, int textLength);

            /// <summary>Determines if a given Unicode scalar value will be encoded.</summary>
            /// <param name="unicodeScalar">A Unicode scalar value.</param>
            /// <returns>
            ///   <see langword="true" /> if the <paramref name="unicodeScalar" /> value will be encoded by this encoder; otherwise, returns <see langword="false" />.</returns>
            [EditorBrowsable(EditorBrowsableState.Never)]
            public abstract bool WillEncode(int unicodeScalar);

            /// <summary>Encodes the supplied string and returns the encoded text as a new string.</summary>
            /// <param name="value">The string to encode.</param>
            /// <exception cref="System.ArgumentNullException">
            ///   <paramref name="value" /> is <see langword="null" />.</exception>
            /// <exception cref="System.ArgumentException">The <see cref="TextEncoder.TryEncodeUnicodeScalar(int, char*, int, out int)" /> method failed. The encoder does not implement <see cref="System.Text.Encodings.Web.TextEncoder.MaxOutputCharactersPerInputCharacter" /> correctly.</exception>
            /// <returns>The encoded string.</returns>
            public virtual string Encode(string value)
            {
                if (value == null)
                {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value);
                }
                int num = FindFirstCharacterToEncode(value.AsSpan());
                if (num < 0)
                {
                    return value;
                }
                return EncodeToNewString(value.AsSpan(), num);
            }

            private string EncodeToNewString(ReadOnlySpan<char> value, int indexOfFirstCharToEncode)
            {
                ReadOnlySpan<char> source = value.Slice(indexOfFirstCharToEncode);
                Span<char> initialBuffer = stackalloc char[1024];
                ValueStringBuilder valueStringBuilder = new ValueStringBuilder(initialBuffer);
                valueStringBuilder.Append(value.Slice(0, indexOfFirstCharToEncode));
                int val = Math.Max(MaxOutputCharactersPerInputCharacter, 1024);
                do
                {
                    Span<char> destination = valueStringBuilder.AppendSpan(Math.Max(source.Length, val));
                    EncodeCore(source, destination, out var charsConsumed, out var charsWritten, isFinalBlock: true);
                    if (charsWritten == 0 || (uint)charsWritten > (uint)destination.Length)
                    {
                        ThrowArgumentException_MaxOutputCharsPerInputChar();
                    }
                    source = source.Slice(charsConsumed);
                    valueStringBuilder.Length -= destination.Length - charsWritten;
                }
                while (!source.IsEmpty);
                return valueStringBuilder.ToString();
            }

            /// <summary>Encodes the specified string to a <see cref="T:System.IO.TextWriter" /> object.</summary>
            /// <param name="output">The stream to which to write the encoded text.</param>
            /// <param name="value">The string to encode.</param>
            public void Encode(TextWriter output, string value)
            {
                Encode(output, value, 0, value.Length);
            }

            /// <summary>Encodes a substring and writes it to a <see cref="System.IO.TextWriter" /> object.</summary>
            /// <param name="output">The stream to which to write the encoded text.</param>
            /// <param name="value">The string whose substring is to be encoded.</param>
            /// <param name="startIndex">The index where the substring starts.</param>
            /// <param name="characterCount">The number of characters in the substring.</param>
            /// <exception cref="System.ArgumentNullException">
            ///   <paramref name="output" /> is <see langword="null" />.</exception>
            /// <exception cref="System.ArgumentException">The <see cref="TryEncodeUnicodeScalar(int, char*, int, out int)" /> method failed. 
            /// The encoder does not implement <see cref="MaxOutputCharactersPerInputCharacter" /> correctly.</exception>
            /// <exception cref="System.ArgumentNullException">
            ///   <paramref name="value" /> is <see langword="null" />.</exception>
            /// <exception cref="System.ArgumentOutOfRangeException">
            ///   <paramref name="startIndex" /> is out of range.</exception>
            /// <exception cref="System.ArgumentOutOfRangeException">
            ///   <paramref name="characterCount" /> is out of range.</exception>
            public virtual void Encode(TextWriter output, string value, int startIndex, int characterCount)
            {
                if (output == null)
                {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.output);
                }
                if (value == null)
                {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value);
                }
                ValidateRanges(startIndex, characterCount, value.Length);
                int num = FindFirstCharacterToEncode(value.AsSpan(startIndex, characterCount));
                if (num < 0)
                {
                    num = characterCount;
                }
                output.WritePartialString(value, startIndex, num);
                if (num != characterCount)
                {
                    EncodeCore(output, value.AsSpan(startIndex + num, characterCount - num));
                }
            }

            /// <summary>Encodes characters from an array and writes them to a <see cref="T:System.IO.TextWriter" /> object.</summary>
            /// <param name="output">The stream to which to write the encoded text.</param>
            /// <param name="value">The array of characters to encode.</param>
            /// <param name="startIndex">The array index of the first character to encode.</param>
            /// <param name="characterCount">The number of characters in the array to encode.</param>
            /// <exception cref="T:System.ArgumentNullException">
            ///   <paramref name="output" /> is <see langword="null" />.</exception>
            /// <exception cref="T:System.ArgumentException">The <see cref="M:System.Text.Encodings.Web.TextEncoder.TryEncodeUnicodeScalar(System.Int32,System.Char*,System.Int32,System.Int32@)" /> method failed. The encoder does not implement <see cref="P:System.Text.Encodings.Web.TextEncoder.MaxOutputCharactersPerInputCharacter" /> correctly.</exception>
            /// <exception cref="T:System.ArgumentNullException">
            ///   <paramref name="value" /> is <see langword="null" />.</exception>
            /// <exception cref="T:System.ArgumentOutOfRangeException">
            ///   <paramref name="startIndex" /> is out of range.</exception>
            /// <exception cref="T:System.ArgumentOutOfRangeException">
            ///   <paramref name="characterCount" /> is out of range.</exception>
            public virtual void Encode(TextWriter output, char[] value, int startIndex, int characterCount)
            {
                if (output == null)
                {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.output);
                }
                if (value == null)
                {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value);
                }
                ValidateRanges(startIndex, characterCount, value.Length);
                int num = FindFirstCharacterToEncode(value.AsSpan(startIndex, characterCount));
                if (num < 0)
                {
                    num = characterCount;
                }
                output.Write(value, startIndex, num);
                if (num != characterCount)
                {
                    EncodeCore(output, value.AsSpan(startIndex + num, characterCount - num));
                }
            }

            /// <summary>Encodes the supplied UTF-8 text.</summary>
            /// <param name="utf8Source">A source buffer containing the UTF-8 text to encode.</param>
            /// <param name="utf8Destination">The destination buffer to which the encoded form of <paramref name="utf8Source" /> will be written.</param>
            /// <param name="bytesConsumed">The number of bytes consumed from the <paramref name="utf8Source" /> buffer.</param>
            /// <param name="bytesWritten">The number of bytes written to the <paramref name="utf8Destination" /> buffer.</param>
            /// <param name="isFinalBlock">
            ///   <see langword="true" /> to indicate there is no further source data that needs to be encoded; otherwise, <see langword="false" />.</param>
            /// <returns>A status code that describes the result of the encoding operation.</returns>
            public virtual OperationStatus EncodeUtf8(ReadOnlySpan<byte> utf8Source, Span<byte> utf8Destination, out int bytesConsumed, out int bytesWritten, bool isFinalBlock = true)
            {
                ReadOnlySpan<byte> utf8Text = utf8Source;
                if (utf8Destination.Length < utf8Source.Length)
                {
                    utf8Text = utf8Source.Slice(0, utf8Destination.Length);
                }
                int num = FindFirstCharacterToEncodeUtf8(utf8Text);
                if (num < 0)
                {
                    num = utf8Text.Length;
                }
                utf8Source.Slice(0, num).CopyTo(utf8Destination);
                if (num == utf8Source.Length)
                {
                    bytesConsumed = utf8Source.Length;
                    bytesWritten = utf8Source.Length;
                    return OperationStatus.Done;
                }
                int bytesConsumed2;
                int bytesWritten2;
                OperationStatus result = EncodeUtf8Core(utf8Source.Slice(num), utf8Destination.Slice(num), out bytesConsumed2, out bytesWritten2, isFinalBlock);
                bytesConsumed = num + bytesConsumed2;
                bytesWritten = num + bytesWritten2;
                return result;
            }

            private protected virtual OperationStatus EncodeUtf8Core(ReadOnlySpan<byte> utf8Source, Span<byte> utf8Destination, out int bytesConsumed, out int bytesWritten, bool isFinalBlock)
            {
                int length = utf8Source.Length;
                int length2 = utf8Destination.Length;
                Span<char> utf16ScratchBuffer = stackalloc char[24];
                OperationStatus result2;
                while (true)
                {
                    int bytesConsumed2;
                    int num2;
                    if (!utf8Source.IsEmpty)
                    {
                        Rune result;
                        OperationStatus operationStatus = Rune.DecodeFromUtf8(utf8Source, out result, out bytesConsumed2);
                        if (operationStatus != 0)
                        {
                            if (!isFinalBlock && operationStatus == OperationStatus.NeedMoreData)
                            {
                                result2 = OperationStatus.NeedMoreData;
                                break;
                            }
                        }
                        else if (!WillEncode(result.Value))
                        {
                            uint num = (uint)UnicodeHelpers.GetUtf8RepresentationForScalarValue((uint)result.Value);
                            num2 = 0;
                            while ((uint)num2 < (uint)utf8Destination.Length)
                            {
                                utf8Destination[num2++] = (byte)num;
                                if ((num >>= 8) != 0)
                                {
                                    continue;
                                }
                                goto IL_008d;
                            }
                            goto IL_00f9;
                        }
                        if (TryEncodeUnicodeScalarUtf8((uint)result.Value, utf16ScratchBuffer, utf8Destination, out var bytesWritten2))
                        {
                            utf8Source = utf8Source.Slice(bytesConsumed2);
                            utf8Destination = utf8Destination.Slice(bytesWritten2);
                            continue;
                        }
                        goto IL_00f9;
                    }
                    result2 = OperationStatus.Done;
                    break;
                IL_008d:
                    utf8Source = utf8Source.Slice(bytesConsumed2);
                    utf8Destination = utf8Destination.Slice(num2);
                    continue;
                IL_00f9:
                    result2 = OperationStatus.DestinationTooSmall;
                    break;
                }
                bytesConsumed = length - utf8Source.Length;
                bytesWritten = length2 - utf8Destination.Length;
                return result2;
            }

            /// <summary>Encodes the supplied characters.</summary>
            /// <param name="source">A source buffer containing the characters to encode.</param>
            /// <param name="destination">The destination buffer to which the encoded form of <paramref name="source" /> will be written.</param>
            /// <param name="charsConsumed">The number of characters consumed from the <paramref name="source" /> buffer.</param>
            /// <param name="charsWritten">The number of characters written to the <paramref name="destination" /> buffer.</param>
            /// <param name="isFinalBlock">
            ///   <see langword="true" /> to indicate there is no further source data that needs to be encoded; otherwise, <see langword="false" />.</param>
            /// <returns>An enumeration value that describes the result of the encoding operation.</returns>
            public virtual OperationStatus Encode(ReadOnlySpan<char> source, Span<char> destination, out int charsConsumed, out int charsWritten, bool isFinalBlock = true)
            {
                ReadOnlySpan<char> text = source;
                if (destination.Length < source.Length)
                {
                    text = source.Slice(0, destination.Length);
                }
                int num = FindFirstCharacterToEncode(text);
                if (num < 0)
                {
                    num = text.Length;
                }
                source.Slice(0, num).CopyTo(destination);
                if (num == source.Length)
                {
                    charsConsumed = source.Length;
                    charsWritten = source.Length;
                    return OperationStatus.Done;
                }
                int charsConsumed2;
                int charsWritten2;
                OperationStatus result = EncodeCore(source.Slice(num), destination.Slice(num), out charsConsumed2, out charsWritten2, isFinalBlock);
                charsConsumed = num + charsConsumed2;
                charsWritten = num + charsWritten2;
                return result;
            }

            private protected virtual OperationStatus EncodeCore(ReadOnlySpan<char> source, Span<char> destination, out int charsConsumed, out int charsWritten, bool isFinalBlock)
            {
                int length = source.Length;
                int length2 = destination.Length;
                OperationStatus result2;
                while (true)
                {
                    if (!source.IsEmpty)
                    {
                        Rune result;
                        int charsConsumed2;
                        OperationStatus operationStatus = Rune.DecodeFromUtf16(source, out result, out charsConsumed2);
                        if (operationStatus != 0)
                        {
                            if (!isFinalBlock && operationStatus == OperationStatus.NeedMoreData)
                            {
                                result2 = OperationStatus.NeedMoreData;
                                break;
                            }
                        }
                        else if (!WillEncode(result.Value))
                        {
                            if (result.TryEncodeToUtf16(destination, out var _))
                            {
                                source = source.Slice(charsConsumed2);
                                destination = destination.Slice(charsConsumed2);
                                continue;
                            }
                            goto IL_00ad;
                        }
                        if (TryEncodeUnicodeScalar((uint)result.Value, destination, out var charsWritten3))
                        {
                            source = source.Slice(charsConsumed2);
                            destination = destination.Slice(charsWritten3);
                            continue;
                        }
                        goto IL_00ad;
                    }
                    result2 = OperationStatus.Done;
                    break;
                IL_00ad:
                    result2 = OperationStatus.DestinationTooSmall;
                    break;
                }
                charsConsumed = length - source.Length;
                charsWritten = length2 - destination.Length;
                return result2;
            }

            private void EncodeCore(TextWriter output, ReadOnlySpan<char> value)
            {
                int val = Math.Max(MaxOutputCharactersPerInputCharacter, 1024);
                char[] array = ArrayPool<char>.Shared.Rent(Math.Max(value.Length, val));
                Span<char> destination = array;
                do
                {
                    EncodeCore(value, destination, out var charsConsumed, out var charsWritten, isFinalBlock: true);
                    if (charsWritten == 0 || (uint)charsWritten > (uint)destination.Length)
                    {
                        ThrowArgumentException_MaxOutputCharsPerInputChar();
                    }
                    output.Write(array, 0, charsWritten);
                    value = value.Slice(charsConsumed);
                }
                while (!value.IsEmpty);
                ArrayPool<char>.Shared.Return(array);
            }

            private protected unsafe virtual int FindFirstCharacterToEncode(ReadOnlySpan<char> text)
            {
                fixed (char* text2 = &MemoryMarshal.GetReference(text))
                {
                    return FindFirstCharacterToEncode(text2, text.Length);
                }
            }

            /// <summary>Finds the first element in a UTF-8 text input buffer that would be escaped by the current encoder instance.</summary>
            /// <param name="utf8Text">The UTF-8 text input buffer to search.</param>
            /// <returns>The index of the first element in <paramref name="utf8Text" /> that would be escaped by the current encoder instance, or -1 if no data in <paramref name="utf8Text" /> requires escaping.</returns>
            [EditorBrowsable(EditorBrowsableState.Never)]
            public virtual int FindFirstCharacterToEncodeUtf8(ReadOnlySpan<byte> utf8Text)
            {
                int length = utf8Text.Length;
                Rune result;
                int bytesConsumed;
                while (!utf8Text.IsEmpty && Rune.DecodeFromUtf8(utf8Text, out result, out bytesConsumed) == OperationStatus.Done && !WillEncode(result.Value))
                {
                    utf8Text = utf8Text.Slice(bytesConsumed);
                }
                if (!utf8Text.IsEmpty)
                {
                    return length - utf8Text.Length;
                }
                return -1;
            }

            internal static bool TryCopyCharacters(string source, Span<char> destination, out int numberOfCharactersWritten)
            {
                if (destination.Length < source.Length)
                {
                    numberOfCharactersWritten = 0;
                    return false;
                }
                for (int i = 0; i < source.Length; i++)
                {
                    destination[i] = source[i];
                }
                numberOfCharactersWritten = source.Length;
                return true;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static bool TryWriteScalarAsChar(int unicodeScalar, Span<char> destination, out int numberOfCharactersWritten)
            {
                if (destination.IsEmpty)
                {
                    numberOfCharactersWritten = 0;
                    return false;
                }
                destination[0] = (char)unicodeScalar;
                numberOfCharactersWritten = 1;
                return true;
            }

            private static void ValidateRanges(int startIndex, int characterCount, int actualInputLength)
            {
                if (startIndex < 0 || startIndex > actualInputLength)
                {
                    throw new ArgumentOutOfRangeException("startIndex");
                }
                if (characterCount < 0 || characterCount > actualInputLength - startIndex)
                {
                    throw new ArgumentOutOfRangeException("characterCount");
                }
            }

            [DoesNotReturn]
            private static void ThrowArgumentException_MaxOutputCharsPerInputChar()
            {
                throw new ArgumentException(MDCFR.Properties.Resources.TextEncoderDoesNotImplementMaxOutputCharsPerInputChar);
            }

            /// <summary>Initializes a new instance of the <see cref="T:System.Text.Encodings.Web.TextEncoder" /> class.</summary>
            protected TextEncoder()
            {
            }
        }

        /// <summary>Represents a filter that allows only certain Unicode code points.</summary>
        public class TextEncoderSettings
        {
            private AllowedBmpCodePointsBitmap _allowedCodePointsBitmap;

            /// <summary>Instantiates an empty filter (allows no code points through by default).</summary>
            public TextEncoderSettings()
            {
            }

            /// <summary>Instantiates a filter by cloning the allowed list of another <see cref="T:System.Text.Encodings.Web.TextEncoderSettings" /> object.</summary>
            /// <param name="other">The other <see cref="T:System.Text.Encodings.Web.TextEncoderSettings" /> object to be cloned.</param>
            public TextEncoderSettings(TextEncoderSettings other)
            {
                if (other == null)
                {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.other);
                }
                _allowedCodePointsBitmap = other.GetAllowedCodePointsBitmap();
            }

            /// <summary>Instantiates a filter where only the character ranges specified by <paramref name="allowedRanges" /> are allowed by the filter.</summary>
            /// <param name="allowedRanges">The allowed character ranges.</param>
            /// <exception cref="T:System.ArgumentNullException">
            ///   <paramref name="allowedRanges" /> is <see langword="null" />.</exception>
            public TextEncoderSettings(params UnicodeRange[] allowedRanges)
            {
                if (allowedRanges == null)
                {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.allowedRanges);
                }
                AllowRanges(allowedRanges);
            }

            /// <summary>Allows the character specified by <paramref name="character" /> through the filter.</summary>
            /// <param name="character">The allowed character.</param>
            public virtual void AllowCharacter(char character)
            {
                _allowedCodePointsBitmap.AllowChar(character);
            }

            /// <summary>Allows all characters specified by <paramref name="characters" /> through the filter.</summary>
            /// <param name="characters">The allowed characters.</param>
            /// <exception cref="T:System.ArgumentNullException">
            ///   <paramref name="characters" /> is <see langword="null" />.</exception>
            public virtual void AllowCharacters(params char[] characters)
            {
                if (characters == null)
                {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.characters);
                }
                for (int i = 0; i < characters.Length; i++)
                {
                    _allowedCodePointsBitmap.AllowChar(characters[i]);
                }
            }

            /// <summary>Allows all code points specified by <paramref name="codePoints" />.</summary>
            /// <param name="codePoints">The allowed code points.</param>
            /// <exception cref="T:System.ArgumentNullException">
            ///   <paramref name="codePoints" /> is <see langword="null" />.</exception>
            public virtual void AllowCodePoints(IEnumerable<int> codePoints)
            {
                if (codePoints == null)
                {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.codePoints);
                }
                foreach (int codePoint in codePoints)
                {
                    if (UnicodeUtility.IsBmpCodePoint((uint)codePoint))
                    {
                        _allowedCodePointsBitmap.AllowChar((char)codePoint);
                    }
                }
            }

            /// <summary>Allows all characters specified by <paramref name="range" /> through the filter.</summary>
            /// <param name="range">The range of characters to be allowed.</param>
            /// <exception cref="T:System.ArgumentNullException">
            ///   <paramref name="range" /> is <see langword="null" />.</exception>
            public virtual void AllowRange(UnicodeRange range)
            {
                if (range == null)
                {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.range);
                }
                int firstCodePoint = range.FirstCodePoint;
                int length = range.Length;
                for (int i = 0; i < length; i++)
                {
                    int num = firstCodePoint + i;
                    _allowedCodePointsBitmap.AllowChar((char)num);
                }
            }

            /// <summary>Allows all characters specified by <paramref name="ranges" /> through the filter.</summary>
            /// <param name="ranges">The ranges of characters to be allowed.</param>
            /// <exception cref="T:System.ArgumentNullException">
            ///   <paramref name="ranges" /> is <see langword="null" />.</exception>
            public virtual void AllowRanges(params UnicodeRange[] ranges)
            {
                if (ranges == null)
                {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.ranges);
                }
                for (int i = 0; i < ranges.Length; i++)
                {
                    AllowRange(ranges[i]);
                }
            }

            /// <summary>Resets this object by disallowing all characters.</summary>
            public virtual void Clear()
            {
                _allowedCodePointsBitmap = default(AllowedBmpCodePointsBitmap);
            }

            /// <summary>Disallows the character <paramref name="character" /> through the filter.</summary>
            /// <param name="character">The disallowed character.</param>
            public virtual void ForbidCharacter(char character)
            {
                _allowedCodePointsBitmap.ForbidChar(character);
            }

            /// <summary>Disallows all characters specified by <paramref name="characters" /> through the filter.</summary>
            /// <param name="characters">The disallowed characters.</param>
            /// <exception cref="T:System.ArgumentNullException">
            ///   <paramref name="characters" /> is <see langword="null" />.</exception>
            public virtual void ForbidCharacters(params char[] characters)
            {
                if (characters == null)
                {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.characters);
                }
                for (int i = 0; i < characters.Length; i++)
                {
                    _allowedCodePointsBitmap.ForbidChar(characters[i]);
                }
            }

            /// <summary>Disallows all characters specified by <paramref name="range" /> through the filter.</summary>
            /// <param name="range">The range of characters to be disallowed.</param>
            /// <exception cref="T:System.ArgumentNullException">
            ///   <paramref name="range" /> is <see langword="null" />.</exception>
            public virtual void ForbidRange(UnicodeRange range)
            {
                if (range == null)
                {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.range);
                }
                int firstCodePoint = range.FirstCodePoint;
                int length = range.Length;
                for (int i = 0; i < length; i++)
                {
                    int num = firstCodePoint + i;
                    _allowedCodePointsBitmap.ForbidChar((char)num);
                }
            }

            /// <summary>Disallows all characters specified by <paramref name="ranges" /> through the filter.</summary>
            /// <param name="ranges">The ranges of characters to be disallowed.</param>
            /// <exception cref="T:System.ArgumentNullException">
            ///   <paramref name="ranges" /> is <see langword="null" />.</exception>
            public virtual void ForbidRanges(params UnicodeRange[] ranges)
            {
                if (ranges == null)
                {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.ranges);
                }
                for (int i = 0; i < ranges.Length; i++)
                {
                    ForbidRange(ranges[i]);
                }
            }

            /// <summary>Gets an enumerator of all allowed code points.</summary>
            /// <returns>The enumerator of allowed code points.</returns>
            public virtual IEnumerable<int> GetAllowedCodePoints()
            {
                for (int i = 0; i <= 65535; i++)
                {
                    if (_allowedCodePointsBitmap.IsCharAllowed((char)i))
                    {
                        yield return i;
                    }
                }
            }

            internal ref readonly AllowedBmpCodePointsBitmap GetAllowedCodePointsBitmap()
            {
                if (GetType() == typeof(TextEncoderSettings))
                {
                    return ref _allowedCodePointsBitmap;
                }
                StrongBox<AllowedBmpCodePointsBitmap> strongBox = new StrongBox<AllowedBmpCodePointsBitmap>();
                foreach (int allowedCodePoint in GetAllowedCodePoints())
                {
                    if ((uint)allowedCodePoint <= 65535u)
                    {
                        strongBox.Value.AllowChar((char)allowedCodePoint);
                    }
                }
                return ref strongBox.Value;
            }
        }

        internal static class ThrowHelper
        {
            [DoesNotReturn]
            internal static void ThrowArgumentNullException(ExceptionArgument argument)
            {
                throw new ArgumentNullException(GetArgumentName(argument));
            }

            [DoesNotReturn]
            internal static void ThrowArgumentOutOfRangeException(ExceptionArgument argument)
            {
                throw new ArgumentOutOfRangeException(GetArgumentName(argument));
            }

            private static string GetArgumentName(ExceptionArgument argument)
            {
                return argument.ToString();
            }
        }

        /// <summary>Represents a URL character encoding.</summary>
        public abstract class UrlEncoder : TextEncoder
        {
            /// <summary>Gets a built-in instance of the <see cref="T:System.Text.Encodings.Web.UrlEncoder" /> class.</summary>
            /// <returns>A built-in instance of the <see cref="T:System.Text.Encodings.Web.UrlEncoder" /> class.</returns>
            public static UrlEncoder Default => DefaultUrlEncoder.BasicLatinSingleton;

            /// <summary>Creates a new instance of UrlEncoder class with the specified settings.</summary>
            /// <param name="settings">Settings that control how the <see cref="T:System.Text.Encodings.Web.UrlEncoder" /> instance encodes, primarily which characters to encode.</param>
            /// <exception cref="T:System.ArgumentNullException">
            ///   <paramref name="settings" /> is <see langword="null" />.</exception>
            /// <returns>A new instance of the <see cref="T:System.Text.Encodings.Web.UrlEncoder" /> class.</returns>
            public static UrlEncoder Create(TextEncoderSettings settings)
            {
                return new DefaultUrlEncoder(settings);
            }

            /// <summary>Creates a new instance of the UrlEncoder class that specifies characters the encoder is allowed to not encode.</summary>
            /// <param name="allowedRanges">The set of characters that the encoder is allowed to not encode.</param>
            /// <exception cref="T:System.ArgumentNullException">
            ///   <paramref name="allowedRanges" /> is <see langword="null" />.</exception>
            /// <returns>A new instance of the <see cref="T:System.Text.Encodings.Web.UrlEncoder" /> class.</returns>
            public static UrlEncoder Create(params UnicodeRange[] allowedRanges)
            {
                return new DefaultUrlEncoder(new TextEncoderSettings(allowedRanges));
            }

            /// <summary>Initializes a new instance of the <see cref="T:System.Text.Encodings.Web.UrlEncoder" /> class.</summary>
            protected UrlEncoder()
            {
            }
        }

    }

    namespace Unicode
    {
        using System.Threading;

        internal static class UnicodeHelpers
        {
            internal const int UNICODE_LAST_CODEPOINT = 1114111;

            private static ReadOnlySpan<byte> DefinedCharsBitmapSpan => new byte[8192]
            {
            0, 0, 0, 0, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 127, 0, 0, 0, 0,
            254, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 252, 240, 215, 255, 255, 251, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 254, 255, 255, 255,
            127, 254, 255, 255, 255, 255, 255, 231, 254, 255,
            255, 255, 255, 255, 255, 0, 255, 255, 255, 135,
            31, 0, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 191, 255, 255, 255, 255,
            255, 255, 255, 231, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 3, 0, 255, 255,
            255, 255, 255, 255, 255, 231, 255, 255, 255, 255,
            255, 63, 255, 127, 255, 255, 255, 79, 255, 7,
            255, 255, 255, 127, 3, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 239, 159, 249, 255, 255, 253,
            197, 243, 159, 121, 128, 176, 207, 255, 255, 127,
            238, 135, 249, 255, 255, 253, 109, 211, 135, 57,
            2, 94, 192, 255, 127, 0, 238, 191, 251, 255,
            255, 253, 237, 243, 191, 59, 1, 0, 207, 255,
            3, 254, 238, 159, 249, 255, 255, 253, 237, 243,
            159, 57, 224, 176, 207, 255, 255, 0, 236, 199,
            61, 214, 24, 199, 255, 195, 199, 61, 129, 0,
            192, 255, 255, 7, 255, 223, 253, 255, 255, 253,
            255, 243, 223, 61, 96, 39, 207, 255, 128, 255,
            255, 223, 253, 255, 255, 253, 239, 243, 223, 61,
            96, 96, 207, 255, 6, 0, 255, 223, 253, 255,
            255, 255, 255, 255, 223, 253, 240, 255, 207, 255,
            255, 255, 238, 255, 127, 252, 255, 255, 251, 47,
            127, 132, 95, 255, 192, 255, 28, 0, 254, 255,
            255, 255, 255, 255, 255, 135, 255, 255, 255, 15,
            0, 0, 0, 0, 214, 247, 255, 255, 175, 255,
            255, 63, 95, 63, 255, 243, 0, 0, 0, 0,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 254,
            255, 255, 255, 31, 254, 255, 255, 255, 255, 254,
            255, 255, 255, 223, 255, 223, 255, 7, 0, 0,
            0, 0, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 191, 32, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 61, 127, 61, 255, 255,
            255, 255, 255, 61, 255, 255, 255, 255, 61, 127,
            61, 255, 127, 255, 255, 255, 255, 255, 255, 255,
            61, 255, 255, 255, 255, 255, 255, 255, 255, 231,
            255, 255, 255, 31, 255, 255, 255, 3, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 63, 63,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            254, 255, 255, 31, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 1, 255, 255, 63, 128,
            255, 255, 127, 0, 255, 255, 15, 0, 255, 223,
            13, 0, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 63, 255, 3, 255, 3, 255, 255,
            255, 3, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 1, 255, 255, 255, 255, 255, 7,
            255, 255, 255, 255, 255, 255, 255, 255, 63, 0,
            255, 255, 255, 127, 255, 15, 255, 15, 241, 255,
            255, 255, 255, 63, 31, 0, 255, 255, 255, 255,
            255, 15, 255, 255, 255, 3, 255, 199, 255, 255,
            255, 255, 255, 255, 255, 207, 255, 255, 255, 255,
            255, 255, 255, 127, 255, 255, 255, 159, 255, 3,
            255, 3, 255, 63, 255, 255, 255, 127, 0, 0,
            0, 0, 0, 0, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 31, 255, 255, 255, 255, 255, 127,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 15, 240, 255, 255, 255, 255,
            255, 255, 255, 248, 255, 227, 255, 255, 255, 255,
            255, 255, 255, 1, 255, 255, 255, 255, 255, 231,
            255, 0, 255, 255, 255, 255, 255, 7, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 63, 63, 255, 255, 255, 255,
            63, 63, 255, 170, 255, 255, 255, 63, 255, 255,
            255, 255, 255, 255, 223, 255, 223, 255, 207, 239,
            255, 255, 220, 127, 0, 248, 255, 255, 255, 124,
            255, 255, 255, 255, 255, 127, 223, 255, 243, 255,
            255, 127, 255, 31, 255, 255, 255, 255, 1, 0,
            255, 255, 255, 255, 1, 0, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 15, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 127, 0, 0, 0,
            255, 7, 0, 0, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            207, 255, 255, 255, 191, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 15, 254,
            255, 255, 255, 255, 191, 32, 255, 255, 255, 255,
            255, 255, 255, 128, 1, 128, 255, 255, 127, 0,
            127, 127, 127, 127, 127, 127, 127, 127, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 63, 0, 0, 0, 0, 255, 255,
            255, 251, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 15, 0, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            63, 0, 0, 0, 255, 15, 254, 255, 255, 255,
            255, 255, 255, 255, 254, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 127, 254, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 224, 255,
            255, 255, 255, 255, 254, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 127, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 15, 0, 255, 255,
            255, 255, 255, 127, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 31, 255, 255, 255, 255,
            255, 255, 127, 0, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 15, 0, 0,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 0, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 7,
            235, 3, 0, 0, 252, 255, 255, 255, 255, 255,
            255, 31, 255, 3, 255, 255, 255, 255, 255, 255,
            255, 0, 255, 255, 255, 255, 255, 255, 255, 255,
            63, 192, 255, 3, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 15, 128,
            255, 255, 255, 31, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 191, 255, 195, 255, 255, 255, 127,
            255, 255, 255, 255, 255, 255, 127, 0, 255, 63,
            255, 243, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 7, 0, 0, 248, 255, 255,
            127, 0, 126, 126, 126, 0, 127, 127, 255, 255,
            255, 255, 255, 255, 255, 15, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 63, 255, 3, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            15, 0, 255, 255, 127, 248, 255, 255, 255, 255,
            255, 15, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 63, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 3, 0, 0,
            0, 0, 127, 0, 248, 224, 255, 255, 127, 95,
            219, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 7, 0, 248, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 252, 255, 255, 255, 255, 255,
            255, 128, 0, 0, 0, 0, 255, 255, 255, 255,
            255, 3, 255, 255, 255, 255, 255, 255, 247, 255,
            127, 15, 223, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 31,
            254, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 127, 252, 252, 252, 28, 127, 127,
            0, 62
            };

            internal static ReadOnlySpan<byte> GetDefinedBmpCodePointsBitmapLittleEndian()
            {
                return DefinedCharsBitmapSpan;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static void GetUtf16SurrogatePairFromAstralScalarValue(uint scalar, out char highSurrogate, out char lowSurrogate)
            {
                highSurrogate = (char)(scalar + 56557568 >> 10);
                lowSurrogate = (char)((scalar & 0x3FF) + 56320);
            }

            internal static int GetUtf8RepresentationForScalarValue(uint scalar)
            {
                if (scalar <= 127)
                {
                    return (byte)scalar;
                }
                if (scalar <= 2047)
                {
                    byte b = (byte)(0xC0u | (scalar >> 6));
                    byte b2 = (byte)(0x80u | (scalar & 0x3Fu));
                    return (b2 << 8) | b;
                }
                if (scalar <= 65535)
                {
                    byte b3 = (byte)(0xE0u | (scalar >> 12));
                    byte b4 = (byte)(0x80u | ((scalar >> 6) & 0x3Fu));
                    byte b5 = (byte)(0x80u | (scalar & 0x3Fu));
                    return (((b5 << 8) | b4) << 8) | b3;
                }
                byte b6 = (byte)(0xF0u | (scalar >> 18));
                byte b7 = (byte)(0x80u | ((scalar >> 12) & 0x3Fu));
                byte b8 = (byte)(0x80u | ((scalar >> 6) & 0x3Fu));
                byte b9 = (byte)(0x80u | (scalar & 0x3Fu));
                return (((((b9 << 8) | b8) << 8) | b7) << 8) | b6;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static bool IsSupplementaryCodePoint(int scalar)
            {
                return (scalar & -65536) != 0;
            }
        }

        /// <summary>Represents a contiguous range of Unicode code points.</summary>
        public sealed class UnicodeRange
        {
            /// <summary>Gets the first code point in the range represented by this <see cref="T:System.Text.Unicode.UnicodeRange" /> instance.</summary>
            /// <returns>The first code point in the range.</returns>
            public int FirstCodePoint { get; private set; }

            /// <summary>Gets the number of code points in the range represented by this <see cref="T:System.Text.Unicode.UnicodeRange" /> instance.</summary>
            /// <returns>The number of code points in the range.</returns>
            public int Length { get; private set; }

            /// <summary>Creates a new <see cref="T:System.Text.Unicode.UnicodeRange" /> that includes a specified number of characters starting at a specified Unicode code point.</summary>
            /// <param name="firstCodePoint">The first code point in the range.</param>
            /// <param name="length">The number of code points in the range.</param>
            /// <exception cref="T:System.ArgumentOutOfRangeException">
            ///   <paramref name="firstCodePoint" /> is less than zero or greater than 0xFFFF. 
            ///
            /// -or-           
            ///
            /// <paramref name="length" /> is less than zero.
            ///
            /// -or-
            ///
            /// <paramref name="firstCodePoint" /> plus <paramref name="length" /> is greater than 0xFFFF.</exception>
            public UnicodeRange(int firstCodePoint, int length)
            {
                if (firstCodePoint < 0 || firstCodePoint > 65535)
                {
                    throw new ArgumentOutOfRangeException("firstCodePoint");
                }
                if (length < 0 || (long)firstCodePoint + (long)length > 65536)
                {
                    throw new ArgumentOutOfRangeException("length");
                }
                FirstCodePoint = firstCodePoint;
                Length = length;
            }

            /// <summary>Creates a new <see cref="T:System.Text.Unicode.UnicodeRange" /> instance from a span of characters.</summary>
            /// <param name="firstCharacter">The first character in the range.</param>
            /// <param name="lastCharacter">The last character in the range.</param>
            /// <exception cref="T:System.ArgumentOutOfRangeException">
            ///   <paramref name="lastCharacter" /> precedes <paramref name="firstCharacter" />.</exception>
            /// <returns>A range that includes all characters between <paramref name="firstCharacter" /> and <paramref name="lastCharacter" />.</returns>
            public static UnicodeRange Create(char firstCharacter, char lastCharacter)
            {
                if (lastCharacter < firstCharacter)
                {
                    throw new ArgumentOutOfRangeException("lastCharacter");
                }
                return new UnicodeRange(firstCharacter, 1 + (lastCharacter - firstCharacter));
            }
        }

        /// <summary>Provides static properties that return predefined <see cref="T:System.Text.Unicode.UnicodeRange" /> instances that correspond to blocks from the Unicode specification.</summary>
        public static class UnicodeRanges
        {
            private static UnicodeRange _none;

            private static UnicodeRange _all;

            private static UnicodeRange _u0000;

            private static UnicodeRange _u0080;

            private static UnicodeRange _u0100;

            private static UnicodeRange _u0180;

            private static UnicodeRange _u0250;

            private static UnicodeRange _u02B0;

            private static UnicodeRange _u0300;

            private static UnicodeRange _u0370;

            private static UnicodeRange _u0400;

            private static UnicodeRange _u0500;

            private static UnicodeRange _u0530;

            private static UnicodeRange _u0590;

            private static UnicodeRange _u0600;

            private static UnicodeRange _u0700;

            private static UnicodeRange _u0750;

            private static UnicodeRange _u0780;

            private static UnicodeRange _u07C0;

            private static UnicodeRange _u0800;

            private static UnicodeRange _u0840;

            private static UnicodeRange _u0860;

            private static UnicodeRange _u0870;

            private static UnicodeRange _u08A0;

            private static UnicodeRange _u0900;

            private static UnicodeRange _u0980;

            private static UnicodeRange _u0A00;

            private static UnicodeRange _u0A80;

            private static UnicodeRange _u0B00;

            private static UnicodeRange _u0B80;

            private static UnicodeRange _u0C00;

            private static UnicodeRange _u0C80;

            private static UnicodeRange _u0D00;

            private static UnicodeRange _u0D80;

            private static UnicodeRange _u0E00;

            private static UnicodeRange _u0E80;

            private static UnicodeRange _u0F00;

            private static UnicodeRange _u1000;

            private static UnicodeRange _u10A0;

            private static UnicodeRange _u1100;

            private static UnicodeRange _u1200;

            private static UnicodeRange _u1380;

            private static UnicodeRange _u13A0;

            private static UnicodeRange _u1400;

            private static UnicodeRange _u1680;

            private static UnicodeRange _u16A0;

            private static UnicodeRange _u1700;

            private static UnicodeRange _u1720;

            private static UnicodeRange _u1740;

            private static UnicodeRange _u1760;

            private static UnicodeRange _u1780;

            private static UnicodeRange _u1800;

            private static UnicodeRange _u18B0;

            private static UnicodeRange _u1900;

            private static UnicodeRange _u1950;

            private static UnicodeRange _u1980;

            private static UnicodeRange _u19E0;

            private static UnicodeRange _u1A00;

            private static UnicodeRange _u1A20;

            private static UnicodeRange _u1AB0;

            private static UnicodeRange _u1B00;

            private static UnicodeRange _u1B80;

            private static UnicodeRange _u1BC0;

            private static UnicodeRange _u1C00;

            private static UnicodeRange _u1C50;

            private static UnicodeRange _u1C80;

            private static UnicodeRange _u1C90;

            private static UnicodeRange _u1CC0;

            private static UnicodeRange _u1CD0;

            private static UnicodeRange _u1D00;

            private static UnicodeRange _u1D80;

            private static UnicodeRange _u1DC0;

            private static UnicodeRange _u1E00;

            private static UnicodeRange _u1F00;

            private static UnicodeRange _u2000;

            private static UnicodeRange _u2070;

            private static UnicodeRange _u20A0;

            private static UnicodeRange _u20D0;

            private static UnicodeRange _u2100;

            private static UnicodeRange _u2150;

            private static UnicodeRange _u2190;

            private static UnicodeRange _u2200;

            private static UnicodeRange _u2300;

            private static UnicodeRange _u2400;

            private static UnicodeRange _u2440;

            private static UnicodeRange _u2460;

            private static UnicodeRange _u2500;

            private static UnicodeRange _u2580;

            private static UnicodeRange _u25A0;

            private static UnicodeRange _u2600;

            private static UnicodeRange _u2700;

            private static UnicodeRange _u27C0;

            private static UnicodeRange _u27F0;

            private static UnicodeRange _u2800;

            private static UnicodeRange _u2900;

            private static UnicodeRange _u2980;

            private static UnicodeRange _u2A00;

            private static UnicodeRange _u2B00;

            private static UnicodeRange _u2C00;

            private static UnicodeRange _u2C60;

            private static UnicodeRange _u2C80;

            private static UnicodeRange _u2D00;

            private static UnicodeRange _u2D30;

            private static UnicodeRange _u2D80;

            private static UnicodeRange _u2DE0;

            private static UnicodeRange _u2E00;

            private static UnicodeRange _u2E80;

            private static UnicodeRange _u2F00;

            private static UnicodeRange _u2FF0;

            private static UnicodeRange _u3000;

            private static UnicodeRange _u3040;

            private static UnicodeRange _u30A0;

            private static UnicodeRange _u3100;

            private static UnicodeRange _u3130;

            private static UnicodeRange _u3190;

            private static UnicodeRange _u31A0;

            private static UnicodeRange _u31C0;

            private static UnicodeRange _u31F0;

            private static UnicodeRange _u3200;

            private static UnicodeRange _u3300;

            private static UnicodeRange _u3400;

            private static UnicodeRange _u4DC0;

            private static UnicodeRange _u4E00;

            private static UnicodeRange _uA000;

            private static UnicodeRange _uA490;

            private static UnicodeRange _uA4D0;

            private static UnicodeRange _uA500;

            private static UnicodeRange _uA640;

            private static UnicodeRange _uA6A0;

            private static UnicodeRange _uA700;

            private static UnicodeRange _uA720;

            private static UnicodeRange _uA800;

            private static UnicodeRange _uA830;

            private static UnicodeRange _uA840;

            private static UnicodeRange _uA880;

            private static UnicodeRange _uA8E0;

            private static UnicodeRange _uA900;

            private static UnicodeRange _uA930;

            private static UnicodeRange _uA960;

            private static UnicodeRange _uA980;

            private static UnicodeRange _uA9E0;

            private static UnicodeRange _uAA00;

            private static UnicodeRange _uAA60;

            private static UnicodeRange _uAA80;

            private static UnicodeRange _uAAE0;

            private static UnicodeRange _uAB00;

            private static UnicodeRange _uAB30;

            private static UnicodeRange _uAB70;

            private static UnicodeRange _uABC0;

            private static UnicodeRange _uAC00;

            private static UnicodeRange _uD7B0;

            private static UnicodeRange _uF900;

            private static UnicodeRange _uFB00;

            private static UnicodeRange _uFB50;

            private static UnicodeRange _uFE00;

            private static UnicodeRange _uFE10;

            private static UnicodeRange _uFE20;

            private static UnicodeRange _uFE30;

            private static UnicodeRange _uFE50;

            private static UnicodeRange _uFE70;

            private static UnicodeRange _uFF00;

            private static UnicodeRange _uFFF0;

            /// <summary>Gets an empty Unicode range.</summary>
            /// <returns>A Unicode range with no elements.</returns>
            public static UnicodeRange None => _none ?? CreateEmptyRange(ref _none);

            /// <summary>Gets a range that consists of the entire Basic Multilingual Plane (BMP), from U+0000 to U+FFFF).</summary>
            /// <returns>A range that consists of the entire BMP.</returns>
            public static UnicodeRange All => _all ?? CreateRange(ref _all, '\0', '\uffff');

            /// <summary>Gets the Basic Latin Unicode block (U+0021-U+007F).</summary>
            /// <returns>The Basic Latin Unicode block (U+0021-U+007F).</returns>
            public static UnicodeRange BasicLatin => _u0000 ?? CreateRange(ref _u0000, '\0', '\u007f');

            /// <summary>Gets the Latin-1 Supplement Unicode block (U+00A1-U+00FF).</summary>
            /// <returns>The Latin-1 Supplement Unicode block (U+00A1-U+00FF).</returns>
            public static UnicodeRange Latin1Supplement => _u0080 ?? CreateRange(ref _u0080, '\u0080', 'ÿ');

            /// <summary>Gets the Latin Extended-A Unicode block (U+0100-U+017F).</summary>
            /// <returns>The Latin Extended-A Unicode block (U+0100-U+017F).</returns>
            public static UnicodeRange LatinExtendedA => _u0100 ?? CreateRange(ref _u0100, 'Ā', 'ſ');

            /// <summary>Gets the Latin Extended-B Unicode block (U+0180-U+024F).</summary>
            /// <returns>The Latin Extended-B Unicode block (U+0180-U+024F).</returns>
            public static UnicodeRange LatinExtendedB => _u0180 ?? CreateRange(ref _u0180, 'ƀ', 'ɏ');

            /// <summary>Gets the IPA Extensions Unicode block (U+0250-U+02AF).</summary>
            /// <returns>The IPA Extensions Unicode block (U+0250-U+02AF).</returns>
            public static UnicodeRange IpaExtensions => _u0250 ?? CreateRange(ref _u0250, 'ɐ', 'ʯ');

            /// <summary>Gets the Spacing Modifier Letters Unicode block (U+02B0-U+02FF).</summary>
            /// <returns>The Spacing Modifier Letters Unicode block (U+02B0-U+02FF).</returns>
            public static UnicodeRange SpacingModifierLetters => _u02B0 ?? CreateRange(ref _u02B0, 'ʰ', '\u02ff');

            /// <summary>Gets the Combining Diacritical Marks Unicode block (U+0300-U+036F).</summary>
            /// <returns>The Combining Diacritical Marks Unicode block (U+0300-U+036F).</returns>
            public static UnicodeRange CombiningDiacriticalMarks => _u0300 ?? CreateRange(ref _u0300, '\u0300', '\u036f');

            /// <summary>Gets the Greek and Coptic Unicode block (U+0370-U+03FF).</summary>
            /// <returns>The Greek and Coptic Unicode block (U+0370-U+03FF).</returns>
            public static UnicodeRange GreekandCoptic => _u0370 ?? CreateRange(ref _u0370, 'Ͱ', 'Ͽ');

            /// <summary>Gets the Cyrillic Unicode block (U+0400-U+04FF).</summary>
            /// <returns>The Cyrillic Unicode block (U+0400-U+04FF).</returns>
            public static UnicodeRange Cyrillic => _u0400 ?? CreateRange(ref _u0400, 'Ѐ', 'ӿ');

            /// <summary>Gets the Cyrillic Supplement Unicode block (U+0500-U+052F).</summary>
            /// <returns>The Cyrillic Supplement Unicode block (U+0500-U+052F).</returns>
            public static UnicodeRange CyrillicSupplement => _u0500 ?? CreateRange(ref _u0500, 'Ԁ', 'ԯ');

            /// <summary>Gets the Armenian Unicode block (U+0530-U+058F).</summary>
            /// <returns>The Armenian Unicode block (U+0530-U+058F).</returns>
            public static UnicodeRange Armenian => _u0530 ?? CreateRange(ref _u0530, '\u0530', '֏');

            /// <summary>Gets the Hebrew Unicode block (U+0590-U+05FF).</summary>
            /// <returns>The Hebrew Unicode block (U+0590-U+05FF).</returns>
            public static UnicodeRange Hebrew => _u0590 ?? CreateRange(ref _u0590, '\u0590', '\u05ff');

            /// <summary>Gets the Arabic Unicode block (U+0600-U+06FF).</summary>
            /// <returns>The Arabic Unicode block (U+0600-U+06FF).</returns>
            public static UnicodeRange Arabic => _u0600 ?? CreateRange(ref _u0600, '\u0600', 'ۿ');

            /// <summary>Gets the Syriac Unicode block (U+0700-U+074F).</summary>
            /// <returns>The Syriac Unicode block (U+0700-U+074F).</returns>
            public static UnicodeRange Syriac => _u0700 ?? CreateRange(ref _u0700, '܀', 'ݏ');

            /// <summary>Gets the Arabic Supplement Unicode block (U+0750-U+077F).</summary>
            /// <returns>The Arabic Supplement Unicode block (U+0750-U+077F).</returns>
            public static UnicodeRange ArabicSupplement => _u0750 ?? CreateRange(ref _u0750, 'ݐ', 'ݿ');

            /// <summary>Gets the Thaana Unicode block (U+0780-U+07BF).</summary>
            /// <returns>The Thaana Unicode block (U+0780-U+07BF).</returns>
            public static UnicodeRange Thaana => _u0780 ?? CreateRange(ref _u0780, 'ހ', '\u07bf');

            /// <summary>Gets the NKo Unicode block (U+07C0-U+07FF).</summary>
            /// <returns>The NKo Unicode block (U+07C0-U+07FF).</returns>
            public static UnicodeRange NKo => _u07C0 ?? CreateRange(ref _u07C0, '߀', '߿');

            /// <summary>Gets the Samaritan Unicode block (U+0800-U+083F).</summary>
            /// <returns>The Samaritan Unicode block (U+0800-U+083F).</returns>
            public static UnicodeRange Samaritan => _u0800 ?? CreateRange(ref _u0800, 'ࠀ', '\u083f');

            /// <summary>Gets the Mandaic Unicode block (U+0840-U+085F).</summary>
            /// <returns>The Mandaic Unicode block (U+0840-U+085F).</returns>
            public static UnicodeRange Mandaic => _u0840 ?? CreateRange(ref _u0840, 'ࡀ', '\u085f');

            /// <summary>A <see cref="T:System.Text.Unicode.UnicodeRange" /> corresponding to the 'Syriac Supplement' Unicode block (U+0860..U+086F).</summary>
            public static UnicodeRange SyriacSupplement => _u0860 ?? CreateRange(ref _u0860, 'ࡠ', '\u086f');

            /// <summary>A <see cref="T:System.Text.Unicode.UnicodeRange" /> corresponding to the 'Arabic Extended-B' Unicode block (U+0870..U+089F).</summary>
            public static UnicodeRange ArabicExtendedB => _u0870 ?? CreateRange(ref _u0870, '\u0870', '\u089f');

            /// <summary>Gets the Arabic Extended-A Unicode block (U+08A0-U+08FF).</summary>
            /// <returns>The Arabic Extended-A Unicode block (U+08A0-U+08FF).</returns>
            public static UnicodeRange ArabicExtendedA => _u08A0 ?? CreateRange(ref _u08A0, 'ࢠ', '\u08ff');

            /// <summary>Gets the Devangari Unicode block (U+0900-U+097F).</summary>
            /// <returns>The Devangari Unicode block (U+0900-U+097F).</returns>
            public static UnicodeRange Devanagari => _u0900 ?? CreateRange(ref _u0900, '\u0900', 'ॿ');

            /// <summary>Gets the Bengali Unicode block (U+0980-U+09FF).</summary>
            /// <returns>The Bengali Unicode block (U+0980-U+09FF).</returns>
            public static UnicodeRange Bengali => _u0980 ?? CreateRange(ref _u0980, 'ঀ', '\u09ff');

            /// <summary>Gets the Gurmukhi Unicode block (U+0A01-U+0A7F).</summary>
            /// <returns>The Gurmukhi Unicode block (U+0A01-U+0A7F).</returns>
            public static UnicodeRange Gurmukhi => _u0A00 ?? CreateRange(ref _u0A00, '\u0a00', '\u0a7f');

            /// <summary>Gets the Gujarti Unicode block (U+0A81-U+0AFF).</summary>
            /// <returns>The Gujarti Unicode block (U+0A81-U+0AFF).</returns>
            public static UnicodeRange Gujarati => _u0A80 ?? CreateRange(ref _u0A80, '\u0a80', '\u0aff');

            /// <summary>Gets the Oriya Unicode block (U+0B00-U+0B7F).</summary>
            /// <returns>The Oriya Unicode block (U+0B00-U+0B7F).</returns>
            public static UnicodeRange Oriya => _u0B00 ?? CreateRange(ref _u0B00, '\u0b00', '\u0b7f');

            /// <summary>Gets the Tamil Unicode block (U+0B80-U+0BFF).</summary>
            /// <returns>The Tamil Unicode block (U+0B82-U+0BFA).</returns>
            public static UnicodeRange Tamil => _u0B80 ?? CreateRange(ref _u0B80, '\u0b80', '\u0bff');

            /// <summary>Gets the Telugu Unicode block (U+0C00-U+0C7F).</summary>
            /// <returns>The Telugu Unicode block (U+0C00-U+0C7F).</returns>
            public static UnicodeRange Telugu => _u0C00 ?? CreateRange(ref _u0C00, '\u0c00', '౿');

            /// <summary>Gets the Kannada Unicode block (U+0C81-U+0CFF).</summary>
            /// <returns>The Kannada Unicode block (U+0C81-U+0CFF).</returns>
            public static UnicodeRange Kannada => _u0C80 ?? CreateRange(ref _u0C80, 'ಀ', '\u0cff');

            /// <summary>Gets the Malayalam Unicode block (U+0D00-U+0D7F).</summary>
            /// <returns>The Malayalam Unicode block (U+0D00-U+0D7F).</returns>
            public static UnicodeRange Malayalam => _u0D00 ?? CreateRange(ref _u0D00, '\u0d00', 'ൿ');

            /// <summary>Gets the Sinhala Unicode block (U+0D80-U+0DFF).</summary>
            /// <returns>The Sinhala Unicode block (U+0D80-U+0DFF).</returns>
            public static UnicodeRange Sinhala => _u0D80 ?? CreateRange(ref _u0D80, '\u0d80', '\u0dff');

            /// <summary>Gets the Thai Unicode block (U+0E00-U+0E7F).</summary>
            /// <returns>The Thai Unicode block (U+0E00-U+0E7F).</returns>
            public static UnicodeRange Thai => _u0E00 ?? CreateRange(ref _u0E00, '\u0e00', '\u0e7f');

            /// <summary>Gets the Lao Unicode block (U+0E80-U+0EDF).</summary>
            /// <returns>The Lao Unicode block (U+0E80-U+0EDF).</returns>
            public static UnicodeRange Lao => _u0E80 ?? CreateRange(ref _u0E80, '\u0e80', '\u0eff');

            /// <summary>Gets the Tibetan Unicode block (U+0F00-U+0FFF).</summary>
            /// <returns>The Tibetan Unicode block (U+0F00-U+0FFF).</returns>
            public static UnicodeRange Tibetan => _u0F00 ?? CreateRange(ref _u0F00, 'ༀ', '\u0fff');

            /// <summary>Gets the Myanmar Unicode block (U+1000-U+109F).</summary>
            /// <returns>The Myanmar Unicode block (U+1000-U+109F).</returns>
            public static UnicodeRange Myanmar => _u1000 ?? CreateRange(ref _u1000, 'က', '႟');

            /// <summary>Gets the Georgian Unicode block (U+10A0-U+10FF).</summary>
            /// <returns>The Georgian Unicode block (U+10A0-U+10FF).</returns>
            public static UnicodeRange Georgian => _u10A0 ?? CreateRange(ref _u10A0, 'Ⴀ', 'ჿ');

            /// <summary>Gets the Hangul Jamo Unicode block (U+1100-U+11FF).</summary>
            /// <returns>The Hangul Jamo Unicode block (U+1100-U+11FF).</returns>
            public static UnicodeRange HangulJamo => _u1100 ?? CreateRange(ref _u1100, 'ᄀ', 'ᇿ');

            /// <summary>Gets the Ethiopic Unicode block (U+1200-U+137C).</summary>
            /// <returns>The Ethiopic Unicode block (U+1200-U+137C).</returns>
            public static UnicodeRange Ethiopic => _u1200 ?? CreateRange(ref _u1200, 'ሀ', '\u137f');

            /// <summary>Gets the Ethiopic Supplement Unicode block (U+1380-U+1399).</summary>
            /// <returns>The Ethiopic Supplement Unicode block (U+1380-U+1399).</returns>
            public static UnicodeRange EthiopicSupplement => _u1380 ?? CreateRange(ref _u1380, 'ᎀ', '\u139f');

            /// <summary>Gets the Cherokee Unicode block (U+13A0-U+13FF).</summary>
            /// <returns>The Cherokee Unicode block (U+13A0-U+13FF).</returns>
            public static UnicodeRange Cherokee => _u13A0 ?? CreateRange(ref _u13A0, 'Ꭰ', '\u13ff');

            /// <summary>Gets the Unified Canadian Aboriginal Syllabics Unicode block (U+1400-U+167F).</summary>
            /// <returns>The Unified Canadian Aboriginal Syllabics Unicode block (U+1400-U+167F).</returns>
            public static UnicodeRange UnifiedCanadianAboriginalSyllabics => _u1400 ?? CreateRange(ref _u1400, '᐀', 'ᙿ');

            /// <summary>Gets the Ogham Unicode block (U+1680-U+169F).</summary>
            /// <returns>The Ogham Unicode block (U+1680-U+169F).</returns>
            public static UnicodeRange Ogham => _u1680 ?? CreateRange(ref _u1680, '\u1680', '\u169f');

            /// <summary>Gets the Runic Unicode block (U+16A0-U+16FF).</summary>
            /// <returns>The Runic Unicode block (U+16A0-U+16FF).</returns>
            public static UnicodeRange Runic => _u16A0 ?? CreateRange(ref _u16A0, 'ᚠ', '\u16ff');

            /// <summary>Gets the Tagalog Unicode block (U+1700-U+171F).</summary>
            /// <returns>The Tagalog Unicode block (U+1700-U+171F).</returns>
            public static UnicodeRange Tagalog => _u1700 ?? CreateRange(ref _u1700, 'ᜀ', '\u171f');

            /// <summary>Gets the Hanunoo Unicode block (U+1720-U+173F).</summary>
            /// <returns>The Hanunoo Unicode block (U+1720-U+173F).</returns>
            public static UnicodeRange Hanunoo => _u1720 ?? CreateRange(ref _u1720, 'ᜠ', '\u173f');

            /// <summary>Gets the Buhid Unicode block (U+1740-U+175F).</summary>
            /// <returns>The Buhid Unicode block (U+1740-U+175F).</returns>
            public static UnicodeRange Buhid => _u1740 ?? CreateRange(ref _u1740, 'ᝀ', '\u175f');

            /// <summary>Gets the Tagbanwa Unicode block (U+1760-U+177F).</summary>
            /// <returns>The Tagbanwa Unicode block (U+1760-U+177F).</returns>
            public static UnicodeRange Tagbanwa => _u1760 ?? CreateRange(ref _u1760, 'ᝠ', '\u177f');

            /// <summary>Gets the Khmer Unicode block (U+1780-U+17FF).</summary>
            /// <returns>The Khmer Unicode block (U+1780-U+17FF).</returns>
            public static UnicodeRange Khmer => _u1780 ?? CreateRange(ref _u1780, 'ក', '\u17ff');

            /// <summary>Gets the Mongolian Unicode block (U+1800-U+18AF).</summary>
            /// <returns>The Mongolian Unicode block (U+1800-U+18AF).</returns>
            public static UnicodeRange Mongolian => _u1800 ?? CreateRange(ref _u1800, '᠀', '\u18af');

            /// <summary>Gets the Unified Canadian Aboriginal Syllabics Extended Unicode block (U+18B0-U+18FF).</summary>
            /// <returns>The Unified Canadian Aboriginal Syllabics Extended Unicode block (U+18B0-U+18FF).</returns>
            public static UnicodeRange UnifiedCanadianAboriginalSyllabicsExtended => _u18B0 ?? CreateRange(ref _u18B0, 'ᢰ', '\u18ff');

            /// <summary>Gets the Limbu Unicode block (U+1900-U+194F).</summary>
            /// <returns>The Limbu Unicode block (U+1900-U+194F).</returns>
            public static UnicodeRange Limbu => _u1900 ?? CreateRange(ref _u1900, 'ᤀ', '᥏');

            /// <summary>Gets the Tai Le Unicode block (U+1950-U+197F).</summary>
            /// <returns>The Tai Le Unicode block (U+1950-U+197F).</returns>
            public static UnicodeRange TaiLe => _u1950 ?? CreateRange(ref _u1950, 'ᥐ', '\u197f');

            /// <summary>Gets the New Tai Lue Unicode block (U+1980-U+19DF).</summary>
            /// <returns>The New Tai Lue Unicode block (U+1980-U+19DF).</returns>
            public static UnicodeRange NewTaiLue => _u1980 ?? CreateRange(ref _u1980, 'ᦀ', '᧟');

            /// <summary>Gets the Khmer Symbols Unicode block (U+19E0-U+19FF).</summary>
            /// <returns>The Khmer Symbols Unicode block (U+19E0-U+19FF).</returns>
            public static UnicodeRange KhmerSymbols => _u19E0 ?? CreateRange(ref _u19E0, '᧠', '᧿');

            /// <summary>Gets the Buginese Unicode block (U+1A00-U+1A1F).</summary>
            /// <returns>The Buginese Unicode block (U+1A00-U+1A1F).</returns>
            public static UnicodeRange Buginese => _u1A00 ?? CreateRange(ref _u1A00, 'ᨀ', '᨟');

            /// <summary>Gets the Tai Tham Unicode block (U+1A20-U+1AAF).</summary>
            /// <returns>The Tai Tham Unicode block (U+1A20-U+1AAF).</returns>
            public static UnicodeRange TaiTham => _u1A20 ?? CreateRange(ref _u1A20, 'ᨠ', '\u1aaf');

            /// <summary>Gets the Combining Diacritical Marks Extended Unicode block (U+1AB0-U+1AFF).</summary>
            /// <returns>The Combining Diacritical Marks Extended Unicode block (U+1AB0-U+1AFF).</returns>
            public static UnicodeRange CombiningDiacriticalMarksExtended => _u1AB0 ?? CreateRange(ref _u1AB0, '\u1ab0', '\u1aff');

            /// <summary>Gets the Balinese Unicode block (U+1B00-U+1B7F).</summary>
            /// <returns>The Balinese Unicode block (U+1B00-U+1B7F).</returns>
            public static UnicodeRange Balinese => _u1B00 ?? CreateRange(ref _u1B00, '\u1b00', '\u1b7f');

            /// <summary>Gets the Sundanese Unicode block (U+1B80-U+1BBF).</summary>
            /// <returns>The Sundanese Unicode block (U+1B80-U+1BBF).</returns>
            public static UnicodeRange Sundanese => _u1B80 ?? CreateRange(ref _u1B80, '\u1b80', 'ᮿ');

            /// <summary>Gets the Batak Unicode block (U+1BC0-U+1BFF).</summary>
            /// <returns>The Batak Unicode block (U+1BC0-U+1BFF).</returns>
            public static UnicodeRange Batak => _u1BC0 ?? CreateRange(ref _u1BC0, 'ᯀ', '᯿');

            /// <summary>Gets the Lepcha Unicode block (U+1C00-U+1C4F).</summary>
            /// <returns>The Lepcha Unicode block (U+1C00-U+1C4F).</returns>
            public static UnicodeRange Lepcha => _u1C00 ?? CreateRange(ref _u1C00, 'ᰀ', 'ᱏ');

            /// <summary>Gets the Ol Chiki Unicode block (U+1C50-U+1C7F).</summary>
            /// <returns>The Ol Chiki Unicode block (U+1C50-U+1C7F).</returns>
            public static UnicodeRange OlChiki => _u1C50 ?? CreateRange(ref _u1C50, '᱐', '᱿');

            /// <summary>A <see cref="T:System.Text.Unicode.UnicodeRange" /> corresponding to the 'Cyrillic Extended-C' Unicode block (U+1C80..U+1C8F).</summary>
            public static UnicodeRange CyrillicExtendedC => _u1C80 ?? CreateRange(ref _u1C80, 'ᲀ', '\u1c8f');

            /// <summary>A <see cref="T:System.Text.Unicode.UnicodeRange" /> corresponding to the 'Georgian Extended' Unicode block (U+1C90..U+1CBF).</summary>
            public static UnicodeRange GeorgianExtended => _u1C90 ?? CreateRange(ref _u1C90, 'Ა', 'Ჿ');

            /// <summary>Gets the Sundanese Supplement Unicode block (U+1CC0-U+1CCF).</summary>
            /// <returns>The Sundanese Supplement Unicode block (U+1CC0-U+1CCF).</returns>
            public static UnicodeRange SundaneseSupplement => _u1CC0 ?? CreateRange(ref _u1CC0, '᳀', '\u1ccf');

            /// <summary>Gets the Vedic Extensions Unicode block (U+1CD0-U+1CFF).</summary>
            /// <returns>The Vedic Extensions Unicode block (U+1CD0-U+1CFF).</returns>
            public static UnicodeRange VedicExtensions => _u1CD0 ?? CreateRange(ref _u1CD0, '\u1cd0', '\u1cff');

            /// <summary>Gets the Phonetic Extensions Unicode block (U+1D00-U+1D7F).</summary>
            /// <returns>The Phonetic Extensions Unicode block (U+1D00-U+1D7F).</returns>
            public static UnicodeRange PhoneticExtensions => _u1D00 ?? CreateRange(ref _u1D00, 'ᴀ', 'ᵿ');

            /// <summary>Gets the Phonetic Extensions Supplement Unicode block (U+1D80-U+1DBF).</summary>
            /// <returns>The Phonetic Extensions Supplement Unicode block (U+1D80-U+1DBF).</returns>
            public static UnicodeRange PhoneticExtensionsSupplement => _u1D80 ?? CreateRange(ref _u1D80, 'ᶀ', 'ᶿ');

            /// <summary>Gets the Combining Diacritical Marks Supplement Unicode block (U+1DC0-U+1DFF).</summary>
            /// <returns>The Combining Diacritical Marks Supplement Unicode block (U+1DC0-U+1DFF).</returns>
            public static UnicodeRange CombiningDiacriticalMarksSupplement => _u1DC0 ?? CreateRange(ref _u1DC0, '\u1dc0', '\u1dff');

            /// <summary>Gets the Latin Extended Additional Unicode block (U+1E00-U+1EFF).</summary>
            /// <returns>The Latin Extended Additional Unicode block (U+1E00-U+1EFF).</returns>
            public static UnicodeRange LatinExtendedAdditional => _u1E00 ?? CreateRange(ref _u1E00, 'Ḁ', 'ỿ');

            /// <summary>Gets the Greek Extended Unicode block (U+1F00-U+1FFF).</summary>
            /// <returns>The Greek Extended Unicode block (U+1F00-U+1FFF).</returns>
            public static UnicodeRange GreekExtended => _u1F00 ?? CreateRange(ref _u1F00, 'ἀ', '\u1fff');

            /// <summary>Gets the General Punctuation Unicode block (U+2000-U+206F).</summary>
            /// <returns>The General Punctuation Unicode block (U+2000-U+206F).</returns>
            public static UnicodeRange GeneralPunctuation => _u2000 ?? CreateRange(ref _u2000, '\u2000', '\u206f');

            /// <summary>Gets the Superscripts and Subscripts Unicode block (U+2070-U+209F).</summary>
            /// <returns>The Superscripts and Subscripts Unicode block (U+2070-U+209F).</returns>
            public static UnicodeRange SuperscriptsandSubscripts => _u2070 ?? CreateRange(ref _u2070, '⁰', '\u209f');

            /// <summary>Gets the Currency Symbols Unicode block (U+20A0-U+20CF).</summary>
            /// <returns>The Currency Symbols Unicode block (U+20A0-U+20CF).</returns>
            public static UnicodeRange CurrencySymbols => _u20A0 ?? CreateRange(ref _u20A0, '₠', '\u20cf');

            /// <summary>Gets the Combining Diacritical Marks for Symbols Unicode block (U+20D0-U+20FF).</summary>
            /// <returns>The Combining Diacritical Marks for Symbols Unicode block (U+20D0-U+20FF).</returns>
            public static UnicodeRange CombiningDiacriticalMarksforSymbols => _u20D0 ?? CreateRange(ref _u20D0, '\u20d0', '\u20ff');

            /// <summary>Gets the Letterlike Symbols Unicode block (U+2100-U+214F).</summary>
            /// <returns>The Letterlike Symbols Unicode block (U+2100-U+214F).</returns>
            public static UnicodeRange LetterlikeSymbols => _u2100 ?? CreateRange(ref _u2100, '℀', '⅏');

            /// <summary>Gets the Number Forms Unicode block (U+2150-U+218F).</summary>
            /// <returns>The Number Forms Unicode block (U+2150-U+218F).</returns>
            public static UnicodeRange NumberForms => _u2150 ?? CreateRange(ref _u2150, '⅐', '\u218f');

            /// <summary>Gets the Arrows Unicode block (U+2190-U+21FF).</summary>
            /// <returns>The Arrows Unicode block (U+2190-U+21FF).</returns>
            public static UnicodeRange Arrows => _u2190 ?? CreateRange(ref _u2190, '←', '⇿');

            /// <summary>Gets the Mathematical Operators Unicode block (U+2200-U+22FF).</summary>
            /// <returns>The Mathematical Operators Unicode block (U+2200-U+22FF).</returns>
            public static UnicodeRange MathematicalOperators => _u2200 ?? CreateRange(ref _u2200, '∀', '⋿');

            /// <summary>Gets the Miscellaneous Technical Unicode block (U+2300-U+23FF).</summary>
            /// <returns>The Miscellaneous Technical Unicode block (U+2300-U+23FF).</returns>
            public static UnicodeRange MiscellaneousTechnical => _u2300 ?? CreateRange(ref _u2300, '⌀', '⏿');

            /// <summary>Gets the Control Pictures Unicode block (U+2400-U+243F).</summary>
            /// <returns>The Control Pictures Unicode block (U+2400-U+243F).</returns>
            public static UnicodeRange ControlPictures => _u2400 ?? CreateRange(ref _u2400, '␀', '\u243f');

            /// <summary>Gets the Optical Character Recognition Unicode block (U+2440-U+245F).</summary>
            /// <returns>The Optical Character Recognition Unicode block (U+2440-U+245F).</returns>
            public static UnicodeRange OpticalCharacterRecognition => _u2440 ?? CreateRange(ref _u2440, '⑀', '\u245f');

            /// <summary>Gets the Enclosed Alphanumerics Unicode block (U+2460-U+24FF).</summary>
            /// <returns>The Enclosed Alphanumerics Unicode block (U+2460-U+24FF).</returns>
            public static UnicodeRange EnclosedAlphanumerics => _u2460 ?? CreateRange(ref _u2460, '①', '⓿');

            /// <summary>Gets the Box Drawing Unicode block (U+2500-U+257F).</summary>
            /// <returns>The Box Drawing Unicode block (U+2500-U+257F).</returns>
            public static UnicodeRange BoxDrawing => _u2500 ?? CreateRange(ref _u2500, '─', '╿');

            /// <summary>Gets the Block Elements Unicode block (U+2580-U+259F).</summary>
            /// <returns>The Block Elements Unicode block (U+2580-U+259F).</returns>
            public static UnicodeRange BlockElements => _u2580 ?? CreateRange(ref _u2580, '▀', '▟');

            /// <summary>Gets the Geometric Shapes Unicode block (U+25A0-U+25FF).</summary>
            /// <returns>The Geometric Shapes Unicode block (U+25A0-U+25FF).</returns>
            public static UnicodeRange GeometricShapes => _u25A0 ?? CreateRange(ref _u25A0, '■', '◿');

            /// <summary>Gets the Miscellaneous Symbols Unicode block (U+2600-U+26FF).</summary>
            /// <returns>The Miscellaneous Symbols Unicode block (U+2600-U+26FF).</returns>
            public static UnicodeRange MiscellaneousSymbols => _u2600 ?? CreateRange(ref _u2600, '☀', '⛿');

            /// <summary>Gets the Dingbats Unicode block (U+2700-U+27BF).</summary>
            /// <returns>The Dingbats Unicode block (U+2700-U+27BF).</returns>
            public static UnicodeRange Dingbats => _u2700 ?? CreateRange(ref _u2700, '✀', '➿');

            /// <summary>Gets the Miscellaneous Mathematical Symbols-A Unicode block (U+27C0-U+27EF).</summary>
            /// <returns>The Miscellaneous Mathematical Symbols-A Unicode block (U+27C0-U+27EF).</returns>
            public static UnicodeRange MiscellaneousMathematicalSymbolsA => _u27C0 ?? CreateRange(ref _u27C0, '⟀', '⟯');

            /// <summary>Gets the Supplemental Arrows-A Unicode block (U+27F0-U+27FF).</summary>
            /// <returns>The Supplemental Arrows-A Unicode block (U+27F0-U+27FF).</returns>
            public static UnicodeRange SupplementalArrowsA => _u27F0 ?? CreateRange(ref _u27F0, '⟰', '⟿');

            /// <summary>Gets the Braille Patterns Unicode block (U+2800-U+28FF).</summary>
            /// <returns>The Braille Patterns Unicode block (U+2800-U+28FF).</returns>
            public static UnicodeRange BraillePatterns => _u2800 ?? CreateRange(ref _u2800, '⠀', '⣿');

            /// <summary>Gets the Supplemental Arrows-B Unicode block (U+2900-U+297F).</summary>
            /// <returns>The Supplemental Arrows-B Unicode block (U+2900-U+297F).</returns>
            public static UnicodeRange SupplementalArrowsB => _u2900 ?? CreateRange(ref _u2900, '⤀', '⥿');

            /// <summary>Gets the Miscellaneous Mathematical Symbols-B Unicode block (U+2980-U+29FF).</summary>
            /// <returns>The Miscellaneous Mathematical Symbols-B Unicode block (U+2980-U+29FF).</returns>
            public static UnicodeRange MiscellaneousMathematicalSymbolsB => _u2980 ?? CreateRange(ref _u2980, '⦀', '⧿');

            /// <summary>Gets the Supplemental Mathematical Operators Unicode block (U+2A00-U+2AFF).</summary>
            /// <returns>The Supplemental Mathematical Operators Unicode block (U+2A00-U+2AFF).</returns>
            public static UnicodeRange SupplementalMathematicalOperators => _u2A00 ?? CreateRange(ref _u2A00, '⨀', '⫿');

            /// <summary>Gets the Miscellaneous Symbols and Arrows Unicode block (U+2B00-U+2BFF).</summary>
            /// <returns>The Miscellaneous Symbols and Arrows Unicode block (U+2B00-U+2BFF).</returns>
            public static UnicodeRange MiscellaneousSymbolsandArrows => _u2B00 ?? CreateRange(ref _u2B00, '⬀', '⯿');

            /// <summary>Gets the Glagolitic Unicode block (U+2C00-U+2C5F).</summary>
            /// <returns>The Glagolitic Unicode block (U+2C00-U+2C5F).</returns>
            public static UnicodeRange Glagolitic => _u2C00 ?? CreateRange(ref _u2C00, 'Ⰰ', '\u2c5f');

            /// <summary>Gets the Latin Extended-C Unicode block (U+2C60-U+2C7F).</summary>
            /// <returns>The Latin Extended-C Unicode block (U+2C60-U+2C7F).</returns>
            public static UnicodeRange LatinExtendedC => _u2C60 ?? CreateRange(ref _u2C60, 'Ⱡ', 'Ɀ');

            /// <summary>Gets the Coptic Unicode block (U+2C80-U+2CFF).</summary>
            /// <returns>The Coptic Unicode block (U+2C80-U+2CFF).</returns>
            public static UnicodeRange Coptic => _u2C80 ?? CreateRange(ref _u2C80, 'Ⲁ', '⳿');

            /// <summary>Gets the Georgian Supplement Unicode block (U+2D00-U+2D2F).</summary>
            /// <returns>The Georgian Supplement Unicode block (U+2D00-U+2D2F).</returns>
            public static UnicodeRange GeorgianSupplement => _u2D00 ?? CreateRange(ref _u2D00, 'ⴀ', '\u2d2f');

            /// <summary>Gets the Tifinagh Unicode block (U+2D30-U+2D7F).</summary>
            /// <returns>The Tifinagh Unicode block (U+2D30-U+2D7F).</returns>
            public static UnicodeRange Tifinagh => _u2D30 ?? CreateRange(ref _u2D30, 'ⴰ', '\u2d7f');

            /// <summary>Gets the Ethipic Extended Unicode block (U+2D80-U+2DDF).</summary>
            /// <returns>The Ethipic Extended Unicode block (U+2D80-U+2DDF).</returns>
            public static UnicodeRange EthiopicExtended => _u2D80 ?? CreateRange(ref _u2D80, 'ⶀ', '\u2ddf');

            /// <summary>Gets the Cyrillic Extended-A Unicode block (U+2DE0-U+2DFF).</summary>
            /// <returns>The Cyrillic Extended-A Unicode block (U+2DE0-U+2DFF).</returns>
            public static UnicodeRange CyrillicExtendedA => _u2DE0 ?? CreateRange(ref _u2DE0, '\u2de0', '\u2dff');

            /// <summary>Gets the Supplemental Punctuation Unicode block (U+2E00-U+2E7F).</summary>
            /// <returns>The Supplemental Punctuation Unicode block (U+2E00-U+2E7F).</returns>
            public static UnicodeRange SupplementalPunctuation => _u2E00 ?? CreateRange(ref _u2E00, '⸀', '\u2e7f');

            /// <summary>Gets the CJK Radicals Supplement Unicode block (U+2E80-U+2EFF).</summary>
            /// <returns>The CJK Radicals Supplement Unicode block (U+2E80-U+2EFF).</returns>
            public static UnicodeRange CjkRadicalsSupplement => _u2E80 ?? CreateRange(ref _u2E80, '⺀', '\u2eff');

            /// <summary>Gets the Kangxi Radicals Supplement Unicode block (U+2F00-U+2FDF).</summary>
            /// <returns>The Kangxi Radicals Supplement Unicode block (U+2F00-U+2FDF).</returns>
            public static UnicodeRange KangxiRadicals => _u2F00 ?? CreateRange(ref _u2F00, '⼀', '\u2fdf');

            /// <summary>Gets the Ideographic Description Characters Unicode block (U+2FF0-U+2FFF).</summary>
            /// <returns>The Ideographic Description Characters Unicode block (U+2FF0-U+2FFF).</returns>
            public static UnicodeRange IdeographicDescriptionCharacters => _u2FF0 ?? CreateRange(ref _u2FF0, '⿰', '\u2fff');

            /// <summary>Gets the CJK Symbols and Punctuation Unicode block (U+3000-U+303F).</summary>
            /// <returns>The CJK Symbols and Punctuation Unicode block (U+3000-U+303F).</returns>
            public static UnicodeRange CjkSymbolsandPunctuation => _u3000 ?? CreateRange(ref _u3000, '\u3000', '〿');

            /// <summary>Gets the Hiragana Unicode block (U+3040-U+309F).</summary>
            /// <returns>The Hiragana Unicode block (U+3040-U+309F).</returns>
            public static UnicodeRange Hiragana => _u3040 ?? CreateRange(ref _u3040, '\u3040', 'ゟ');

            /// <summary>Gets the Katakana Unicode block (U+30A0-U+30FF).</summary>
            /// <returns>The Katakana Unicode block (U+30A0-U+30FF).</returns>
            public static UnicodeRange Katakana => _u30A0 ?? CreateRange(ref _u30A0, '゠', 'ヿ');

            /// <summary>Gets the Bopomofo Unicode block (U+3100-U+312F).</summary>
            /// <returns>The Bopomofo Unicode block (U+3105-U+312F).</returns>
            public static UnicodeRange Bopomofo => _u3100 ?? CreateRange(ref _u3100, '\u3100', 'ㄯ');

            /// <summary>Gets the Hangul Compatibility Jamo Unicode block (U+3131-U+318F).</summary>
            /// <returns>The Hangul Compatibility Jamo Unicode block (U+3131-U+318F).</returns>
            public static UnicodeRange HangulCompatibilityJamo => _u3130 ?? CreateRange(ref _u3130, '\u3130', '\u318f');

            /// <summary>Gets the Kanbun Unicode block (U+3190-U+319F).</summary>
            /// <returns>The Kanbun Unicode block (U+3190-U+319F).</returns>
            public static UnicodeRange Kanbun => _u3190 ?? CreateRange(ref _u3190, '㆐', '㆟');

            /// <summary>Gets the Bopomofo Extended Unicode block (U+31A0-U+31BF).</summary>
            /// <returns>The Bopomofo Extended Unicode block (U+31A0-U+31BF).</returns>
            public static UnicodeRange BopomofoExtended => _u31A0 ?? CreateRange(ref _u31A0, 'ㆠ', 'ㆿ');

            /// <summary>Gets the CJK Strokes Unicode block (U+31C0-U+31EF).</summary>
            /// <returns>The CJK Strokes Unicode block (U+31C0-U+31EF).</returns>
            public static UnicodeRange CjkStrokes => _u31C0 ?? CreateRange(ref _u31C0, '㇀', '\u31ef');

            /// <summary>Gets the Katakana Phonetic Extensions Unicode block (U+31F0-U+31FF).</summary>
            /// <returns>The Katakana Phonetic Extensions Unicode block (U+31F0-U+31FF).</returns>
            public static UnicodeRange KatakanaPhoneticExtensions => _u31F0 ?? CreateRange(ref _u31F0, 'ㇰ', 'ㇿ');

            /// <summary>Gets the Enclosed CJK Letters and Months Unicode block (U+3200-U+32FF).</summary>
            /// <returns>The Enclosed CJK Letters and Months Unicode block (U+3200-U+32FF).</returns>
            public static UnicodeRange EnclosedCjkLettersandMonths => _u3200 ?? CreateRange(ref _u3200, '㈀', '㋿');

            /// <summary>Gets the CJK Compatibility Unicode block (U+3300-U+33FF).</summary>
            /// <returns>The CJK Compatibility Unicode block (U+3300-U+33FF).</returns>
            public static UnicodeRange CjkCompatibility => _u3300 ?? CreateRange(ref _u3300, '㌀', '㏿');

            /// <summary>Gets the CJK Unitied Ideographs Extension A Unicode block (U+3400-U+4DB5).</summary>
            /// <returns>The CJK Unitied Ideographs Extension A Unicode block (U+3400-U+4DB5).</returns>
            public static UnicodeRange CjkUnifiedIdeographsExtensionA => _u3400 ?? CreateRange(ref _u3400, '㐀', '䶿');

            /// <summary>Gets the Yijing Hexagram Symbols Unicode block (U+4DC0-U+4DFF).</summary>
            /// <returns>The Yijing Hexagram Symbols Unicode block (U+4DC0-U+4DFF).</returns>
            public static UnicodeRange YijingHexagramSymbols => _u4DC0 ?? CreateRange(ref _u4DC0, '䷀', '䷿');

            /// <summary>Gets the CJK Unified Ideographs Unicode block (U+4E00-U+9FCC).</summary>
            /// <returns>The CJK Unified Ideographs Unicode block (U+4E00-U+9FCC).</returns>
            public static UnicodeRange CjkUnifiedIdeographs => _u4E00 ?? CreateRange(ref _u4E00, '一', '\u9fff');

            /// <summary>Gets the Yi Syllables Unicode block (U+A000-U+A48F).</summary>
            /// <returns>The Yi Syllables Unicode block (U+A000-U+A48F).</returns>
            public static UnicodeRange YiSyllables => _uA000 ?? CreateRange(ref _uA000, 'ꀀ', '\ua48f');

            /// <summary>Gets the Yi Radicals Unicode block (U+A490-U+A4CF).</summary>
            /// <returns>The Yi Radicals Unicode block (U+A490-U+A4CF).</returns>
            public static UnicodeRange YiRadicals => _uA490 ?? CreateRange(ref _uA490, '꒐', '\ua4cf');

            /// <summary>Gets the Lisu Unicode block (U+A4D0-U+A4FF).</summary>
            /// <returns>The Lisu Unicode block (U+A4D0-U+A4FF).</returns>
            public static UnicodeRange Lisu => _uA4D0 ?? CreateRange(ref _uA4D0, 'ꓐ', '꓿');

            /// <summary>Gets the Vai Unicode block (U+A500-U+A63F).</summary>
            /// <returns>The Vai Unicode block (U+A500-U+A63F).</returns>
            public static UnicodeRange Vai => _uA500 ?? CreateRange(ref _uA500, 'ꔀ', '\ua63f');

            /// <summary>Gets the Cyrillic Extended-B Unicode block (U+A640-U+A69F).</summary>
            /// <returns>The Cyrillic Extended-B Unicode block (U+A640-U+A69F).</returns>
            public static UnicodeRange CyrillicExtendedB => _uA640 ?? CreateRange(ref _uA640, 'Ꙁ', '\ua69f');

            /// <summary>Gets the Bamum Unicode block (U+A6A0-U+A6FF).</summary>
            /// <returns>The Bamum Unicode block (U+A6A0-U+A6FF).</returns>
            public static UnicodeRange Bamum => _uA6A0 ?? CreateRange(ref _uA6A0, 'ꚠ', '\ua6ff');

            /// <summary>Gets the Modifier Tone Letters Unicode block (U+A700-U+A71F).</summary>
            /// <returns>The Modifier Tone Letters Unicode block (U+A700-U+A71F).</returns>
            public static UnicodeRange ModifierToneLetters => _uA700 ?? CreateRange(ref _uA700, '\ua700', 'ꜟ');

            /// <summary>Gets the Latin Extended-D Unicode block (U+A720-U+A7FF).</summary>
            /// <returns>The Latin Extended-D Unicode block (U+A720-U+A7FF).</returns>
            public static UnicodeRange LatinExtendedD => _uA720 ?? CreateRange(ref _uA720, '\ua720', 'ꟿ');

            /// <summary>Gets the Syloti Nagri Unicode block (U+A800-U+A82F).</summary>
            /// <returns>The Syloti Nagri Unicode block (U+A800-U+A82F).</returns>
            public static UnicodeRange SylotiNagri => _uA800 ?? CreateRange(ref _uA800, 'ꠀ', '\ua82f');

            /// <summary>Gets the Common Indic Number Forms Unicode block (U+A830-U+A83F).</summary>
            /// <returns>The Common Indic Number Forms Unicode block (U+A830-U+A83F).</returns>
            public static UnicodeRange CommonIndicNumberForms => _uA830 ?? CreateRange(ref _uA830, '꠰', '\ua83f');

            /// <summary>Gets the Phags-pa Unicode block (U+A840-U+A87F).</summary>
            /// <returns>The Phags-pa Unicode block (U+A840-U+A87F).</returns>
            public static UnicodeRange Phagspa => _uA840 ?? CreateRange(ref _uA840, 'ꡀ', '\ua87f');

            /// <summary>Gets the Saurashtra Unicode block (U+A880-U+A8DF).</summary>
            /// <returns>The Saurashtra Unicode block (U+A880-U+A8DF).</returns>
            public static UnicodeRange Saurashtra => _uA880 ?? CreateRange(ref _uA880, '\ua880', '\ua8df');

            /// <summary>Gets the Devanagari Extended Unicode block (U+A8E0-U+A8FF).</summary>
            /// <returns>The Devanagari Extended Unicode block (U+A8E0-U+A8FF).</returns>
            public static UnicodeRange DevanagariExtended => _uA8E0 ?? CreateRange(ref _uA8E0, '\ua8e0', '\ua8ff');

            /// <summary>Gets the Kayah Li Unicode block (U+A900-U+A92F).</summary>
            /// <returns>The Kayah Li Unicode block (U+A900-U+A92F).</returns>
            public static UnicodeRange KayahLi => _uA900 ?? CreateRange(ref _uA900, '꤀', '꤯');

            /// <summary>Gets the Rejang Unicode block (U+A930-U+A95F).</summary>
            /// <returns>The Rejang Unicode block (U+A930-U+A95F).</returns>
            public static UnicodeRange Rejang => _uA930 ?? CreateRange(ref _uA930, 'ꤰ', '꥟');

            /// <summary>Gets the Hangul Jamo Extended-A Unicode block (U+A960-U+A9F).</summary>
            /// <returns>The Hangul Jamo Extended-A Unicode block (U+A960-U+A97F).</returns>
            public static UnicodeRange HangulJamoExtendedA => _uA960 ?? CreateRange(ref _uA960, 'ꥠ', '\ua97f');

            /// <summary>Gets the Javanese Unicode block (U+A980-U+A9DF).</summary>
            /// <returns>The Javanese Unicode block (U+A980-U+A9DF).</returns>
            public static UnicodeRange Javanese => _uA980 ?? CreateRange(ref _uA980, '\ua980', '꧟');

            /// <summary>Gets the Myanmar Extended-B Unicode block (U+A9E0-U+A9FF).</summary>
            /// <returns>The Myanmar Extended-B Unicode block (U+A9E0-U+A9FF).</returns>
            public static UnicodeRange MyanmarExtendedB => _uA9E0 ?? CreateRange(ref _uA9E0, 'ꧠ', '\ua9ff');

            /// <summary>Gets the Cham Unicode block (U+AA00-U+AA5F).</summary>
            /// <returns>The Cham Unicode block (U+AA00-U+AA5F).</returns>
            public static UnicodeRange Cham => _uAA00 ?? CreateRange(ref _uAA00, 'ꨀ', '꩟');

            /// <summary>Gets the Myanmar Extended-A Unicode block (U+AA60-U+AA7F).</summary>
            /// <returns>The Myanmar Extended-A Unicode block (U+AA60-U+AA7F).</returns>
            public static UnicodeRange MyanmarExtendedA => _uAA60 ?? CreateRange(ref _uAA60, 'ꩠ', 'ꩿ');

            /// <summary>Gets the Tai Viet Unicode block (U+AA80-U+AADF).</summary>
            /// <returns>The Tai Viet Unicode block (U+AA80-U+AADF).</returns>
            public static UnicodeRange TaiViet => _uAA80 ?? CreateRange(ref _uAA80, 'ꪀ', '꫟');

            /// <summary>Gets the Meetei Mayek Extensions Unicode block (U+AAE0-U+AAFF).</summary>
            /// <returns>The Meetei Mayek Extensions Unicode block (U+AAE0-U+AAFF).</returns>
            public static UnicodeRange MeeteiMayekExtensions => _uAAE0 ?? CreateRange(ref _uAAE0, 'ꫠ', '\uaaff');

            /// <summary>Gets the Ethiopic Extended-A Unicode block (U+AB00-U+AB2F).</summary>
            /// <returns>The Ethiopic Extended-A Unicode block (U+AB00-U+AB2F).</returns>
            public static UnicodeRange EthiopicExtendedA => _uAB00 ?? CreateRange(ref _uAB00, '\uab00', '\uab2f');

            /// <summary>Gets the Latin Extended-E Unicode block (U+AB30-U+AB6F).</summary>
            /// <returns>The Latin Extended-E Unicode block (U+AB30-U+AB6F).</returns>
            public static UnicodeRange LatinExtendedE => _uAB30 ?? CreateRange(ref _uAB30, 'ꬰ', '\uab6f');

            /// <summary>Gets the Cherokee Supplement Unicode block (U+AB70-U+ABBF).</summary>
            /// <returns>The Cherokee Supplement Unicode block (U+AB70-U+ABBF).</returns>
            public static UnicodeRange CherokeeSupplement => _uAB70 ?? CreateRange(ref _uAB70, 'ꭰ', 'ꮿ');

            /// <summary>Gets the Meetei Mayek Unicode block (U+ABC0-U+ABFF).</summary>
            /// <returns>The Meetei Mayek Unicode block (U+ABC0-U+ABFF).</returns>
            public static UnicodeRange MeeteiMayek => _uABC0 ?? CreateRange(ref _uABC0, 'ꯀ', '\uabff');

            /// <summary>Gets the Hangul Syllables Unicode block (U+AC00-U+D7AF).</summary>
            /// <returns>The Hangul Syllables Unicode block (U+AC00-U+D7AF).</returns>
            public static UnicodeRange HangulSyllables => _uAC00 ?? CreateRange(ref _uAC00, '가', '\ud7af');

            /// <summary>Gets the Hangul Jamo Extended-B Unicode block (U+D7B0-U+D7FF).</summary>
            /// <returns>The Hangul Jamo Extended-B Unicode block (U+D7B0-U+D7FF).</returns>
            public static UnicodeRange HangulJamoExtendedB => _uD7B0 ?? CreateRange(ref _uD7B0, 'ힰ', '\ud7ff');

            /// <summary>Gets the CJK Compatibility Ideographs Unicode block (U+F900-U+FAD9).</summary>
            /// <returns>The CJK Compatibility Ideographs Unicode block (U+F900-U+FAD9).</returns>
            public static UnicodeRange CjkCompatibilityIdeographs => _uF900 ?? CreateRange(ref _uF900, '豈', '\ufaff');

            /// <summary>Gets the Alphabetic Presentation Forms Unicode block (U+FB00-U+FB4F).</summary>
            /// <returns>The Alphabetic Presentation Forms Unicode block (U+FB00-U+FB4F).</returns>
            public static UnicodeRange AlphabeticPresentationForms => _uFB00 ?? CreateRange(ref _uFB00, 'ﬀ', 'ﭏ');

            /// <summary>Gets the Arabic Presentation Forms-A Unicode block (U+FB50-U+FDFF).</summary>
            /// <returns>The Arabic Presentation Forms-A Unicode block (U+FB50-U+FDFF).</returns>
            public static UnicodeRange ArabicPresentationFormsA => _uFB50 ?? CreateRange(ref _uFB50, 'ﭐ', '\ufdff');

            /// <summary>Gets the Variation Selectors Unicode block (U+FE00-U+FE0F).</summary>
            /// <returns>The Variation Selectors Unicode block (U+FE00-U+FE0F).</returns>
            public static UnicodeRange VariationSelectors => _uFE00 ?? CreateRange(ref _uFE00, '\ufe00', '\ufe0f');

            /// <summary>Gets the Vertical Forms Unicode block (U+FE10-U+FE1F).</summary>
            /// <returns>The Vertical Forms Unicode block (U+FE10-U+FE1F).</returns>
            public static UnicodeRange VerticalForms => _uFE10 ?? CreateRange(ref _uFE10, '︐', '\ufe1f');

            /// <summary>Gets the Combining Half Marks Unicode block (U+FE20-U+FE2F).</summary>
            /// <returns>The Combining Half Marks Unicode block (U+FE20-U+FE2F).</returns>
            public static UnicodeRange CombiningHalfMarks => _uFE20 ?? CreateRange(ref _uFE20, '\ufe20', '\ufe2f');

            /// <summary>Gets the CJK Compatibility Forms Unicode block (U+FE30-U+FE4F).</summary>
            /// <returns>The CJK Compatibility Forms Unicode block (U+FE30-U+FE4F).</returns>
            public static UnicodeRange CjkCompatibilityForms => _uFE30 ?? CreateRange(ref _uFE30, '︰', '\ufe4f');

            /// <summary>Gets the Small Form Variants Unicode block (U+FE50-U+FE6F).</summary>
            /// <returns>The Small Form Variants Unicode block (U+FE50-U+FE6F).</returns>
            public static UnicodeRange SmallFormVariants => _uFE50 ?? CreateRange(ref _uFE50, '﹐', '\ufe6f');

            /// <summary>Gets the Arabic Presentation Forms-B Unicode block (U+FE70-U+FEFF).</summary>
            /// <returns>The Arabic Presentation Forms-B Unicode block (U+FE70-U+FEFF).</returns>
            public static UnicodeRange ArabicPresentationFormsB => _uFE70 ?? CreateRange(ref _uFE70, 'ﹰ', '\ufeff');

            /// <summary>Gets the Halfwidth and Fullwidth Forms Unicode block (U+FF00-U+FFEE).</summary>
            /// <returns>The Halfwidth and Fullwidth Forms Unicode block (U+FF00-U+FFEE).</returns>
            public static UnicodeRange HalfwidthandFullwidthForms => _uFF00 ?? CreateRange(ref _uFF00, '\uff00', '\uffef');

            /// <summary>Gets the Specials Unicode block (U+FFF0-U+FFFF).</summary>
            /// <returns>The Specials Unicode block (U+FFF0-U+FFFF).</returns>
            public static UnicodeRange Specials => _uFFF0 ?? CreateRange(ref _uFFF0, '\ufff0', '\uffff');

            [MethodImpl(MethodImplOptions.NoInlining)]
            private static UnicodeRange CreateEmptyRange([NotNull] ref UnicodeRange range)
            {
                Volatile.Write(ref range, new UnicodeRange(0, 0));
                return range;
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            private static UnicodeRange CreateRange([NotNull] ref UnicodeRange range, char first, char last)
            {
                Volatile.Write(ref range, UnicodeRange.Create(first, last));
                return range;
            }
        }

    }

}