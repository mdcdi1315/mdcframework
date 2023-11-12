// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


using System.Threading;
using System.ComponentModel;
using System.Diagnostics.Tracing;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace System
{
    namespace Buffers
    {
        using Binary;

        // This code is located from the official NUGET package (System.Buffers)
        // and was decompiled. That code is located here:

        // Change: BufferAllocatedReason enum was moved to the namespace.
        // Change: Text.FormattingHelpers.HexCasing enum was moved to Text.

        namespace Text
        {

            /// <summary>
            /// Converts between binary data and UTF-8 encoded text that is represented in base 64.
            /// </summary>
            public static class Base64
            {
                private static readonly sbyte[] s_decodingMap = new sbyte[256]
                {
                    -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
                    -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
                    -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
                    -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
                    -1, -1, -1, 62, -1, -1, -1, 63, 52, 53,
                    54, 55, 56, 57, 58, 59, 60, 61, -1, -1,
                    -1, -1, -1, -1, -1, 0, 1, 2, 3, 4,
                    5, 6, 7, 8, 9, 10, 11, 12, 13, 14,
                    15, 16, 17, 18, 19, 20, 21, 22, 23, 24,
                    25, -1, -1, -1, -1, -1, -1, 26, 27, 28,
                    29, 30, 31, 32, 33, 34, 35, 36, 37, 38,
                    39, 40, 41, 42, 43, 44, 45, 46, 47, 48,
                    49, 50, 51, -1, -1, -1, -1, -1, -1, -1,
                    -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
                    -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
                    -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
                    -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
                    -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
                    -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
                    -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
                    -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
                    -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
                    -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
                    -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
                    -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
                    -1, -1, -1, -1, -1, -1
                };

                private static readonly byte[] s_encodingMap = new byte[64]
                {
                    65, 66, 67, 68, 69, 70, 71, 72, 73, 74,
                    75, 76, 77, 78, 79, 80, 81, 82, 83, 84,
                    85, 86, 87, 88, 89, 90, 97, 98, 99, 100,
                    101, 102, 103, 104, 105, 106, 107, 108, 109, 110,
                    111, 112, 113, 114, 115, 116, 117, 118, 119, 120,
                    121, 122, 48, 49, 50, 51, 52, 53, 54, 55,
                    56, 57, 43, 47
                };

                private const byte EncodingPad = 61;

                private const int MaximumEncodeLength = 1610612733;

                /// <summary>
                /// Decodes the span of UTF-8 encoded text represented as base 64 into binary data. 
                /// If the input is not a multiple of 4, it will decode as much as it can, to the closest multiple of 4.
                /// </summary>
                /// <param name="utf8">The input span that contains UTF-8 encoded text in base 64 that needs to be decoded.</param>
                /// <param name="bytes">The output span that contains the result of the operation, that is, the decoded binary data.</param>
                /// <param name="bytesConsumed">When this method returns, contains the number of input bytes consumed during the operation. 
                /// This can be used to slice the input for subsequent calls, if necessary.</param>
                /// <param name="bytesWritten">When this method returns, contains the number of bytes written into the output span. 
                /// This can be used to slice the output for subsequent calls, if necessary.</param>
                /// <param name="isFinalBlock"><c>true</c> (default) to indicate that the input span contains the entire data to decode. 
                /// <c>false</c> to indicate that the input span contains partial data with more data to follow.</param>
                /// <returns>One of the enumeration values that indicates the status of the decoding operation.</returns>
                /// <remarks>
                /// <para>
                /// The return value can be as follows:
                /// <para><see cref="OperationStatus.Done"/>: Processing of the entire input span succeeded. </para>
                /// <para><see cref="OperationStatus.DestinationTooSmall"/>: 
                /// There is not enough space in the output span to write the decoded input. </para>
                /// <para><see cref="OperationStatus.NeedMoreData"/>: 
                /// isFinalBlock is false and the input is not a multiple of 4. Otherwise, the partial input is considered InvalidData. </para>
                /// <para><see cref="OperationStatus.InvalidData"/>: 
                /// The input contains bytes outside of the expected base 64 range, or is incomplete (that is, not a multiple of 4) and isFinalBlock is true. 
                /// In .NET 7 and earlier versions, this value can also indicate that the input has invalid or more than two padding characters. </para>
                /// </para>
                /// </remarks>
                public static OperationStatus DecodeFromUtf8(ReadOnlySpan<byte> utf8, Span<byte> bytes, out int bytesConsumed, out int bytesWritten, bool isFinalBlock = true)
                {
                    ref byte reference = ref MemoryMarshal.GetReference(utf8);
                    ref byte reference2 = ref MemoryMarshal.GetReference(bytes);
                    int num = utf8.Length & -4;
                    int length = bytes.Length;
                    int num2 = 0;
                    int num3 = 0;
                    if (utf8.Length != 0)
                    {
                        ref sbyte reference3 = ref s_decodingMap[0];
                        int num4 = (isFinalBlock ? 4 : 0);
                        int num5 = 0;
                        num5 = ((length < GetMaxDecodedFromUtf8Length(num)) ? (length / 3 * 4) : (num - num4));
                        while (true)
                        {
                            if (num2 < num5)
                            {
                                int num6 = Decode(ref Unsafe.Add(ref reference, num2), ref reference3);
                                if (num6 >= 0)
                                {
                                    WriteThreeLowOrderBytes(ref Unsafe.Add(ref reference2, num3), num6);
                                    num3 += 3;
                                    num2 += 4;
                                    continue;
                                }
                            }
                            else
                            {
                                if (num5 != num - num4)
                                {
                                    goto IL_0205;
                                }
                                if (num2 == num)
                                {
                                    if (!isFinalBlock)
                                    {
                                        bytesConsumed = num2;
                                        bytesWritten = num3;
                                        return OperationStatus.NeedMoreData;
                                    }
                                }
                                else
                                {
                                    int elementOffset = Unsafe.Add(ref reference, num - 4);
                                    int elementOffset2 = Unsafe.Add(ref reference, num - 3);
                                    int num7 = Unsafe.Add(ref reference, num - 2);
                                    int num8 = Unsafe.Add(ref reference, num - 1);
                                    elementOffset = Unsafe.Add(ref reference3, elementOffset);
                                    elementOffset2 = Unsafe.Add(ref reference3, elementOffset2);
                                    elementOffset <<= 18;
                                    elementOffset2 <<= 12;
                                    elementOffset |= elementOffset2;
                                    if (num8 != 61)
                                    {
                                        num7 = Unsafe.Add(ref reference3, num7);
                                        num8 = Unsafe.Add(ref reference3, num8);
                                        num7 <<= 6;
                                        elementOffset |= num8;
                                        elementOffset |= num7;
                                        if (elementOffset >= 0)
                                        {
                                            if (num3 <= length - 3)
                                            {
                                                WriteThreeLowOrderBytes(ref Unsafe.Add(ref reference2, num3), elementOffset);
                                                num3 += 3;
                                                goto IL_01eb;
                                            }
                                            goto IL_0205;
                                        }
                                    }
                                    else if (num7 != 61)
                                    {
                                        num7 = Unsafe.Add(ref reference3, num7);
                                        num7 <<= 6;
                                        elementOffset |= num7;
                                        if (elementOffset >= 0)
                                        {
                                            if (num3 <= length - 2)
                                            {
                                                Unsafe.Add(ref reference2, num3) = (byte)(elementOffset >> 16);
                                                Unsafe.Add(ref reference2, num3 + 1) = (byte)(elementOffset >> 8);
                                                num3 += 2;
                                                goto IL_01eb;
                                            }
                                            goto IL_0205;
                                        }
                                    }
                                    else if (elementOffset >= 0)
                                    {
                                        if (num3 <= length - 1)
                                        {
                                            Unsafe.Add(ref reference2, num3) = (byte)(elementOffset >> 16);
                                            num3++;
                                            goto IL_01eb;
                                        }
                                        goto IL_0205;
                                    }
                                }
                            }
                            goto IL_022b;
                        IL_01eb:
                            num2 += 4;
                            if (num == utf8.Length)
                            {
                                break;
                            }
                            goto IL_022b;
                        IL_022b:
                            bytesConsumed = num2;
                            bytesWritten = num3;
                            return OperationStatus.InvalidData;
                        IL_0205:
                            if (!(num != utf8.Length && isFinalBlock))
                            {
                                bytesConsumed = num2;
                                bytesWritten = num3;
                                return OperationStatus.DestinationTooSmall;
                            }
                            goto IL_022b;
                        }
                    }
                    bytesConsumed = num2;
                    bytesWritten = num3;
                    return OperationStatus.Done;
                }

                /// <summary>
                /// Returns the maximum length (in bytes) of the result if you were to decode base-64 encoded text within a byte span with the specified length.
                /// </summary>
                /// <param name="length">The size of the byte span.</param>
                /// <returns>The maximum length (in bytes) of the result.</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static int GetMaxDecodedFromUtf8Length(int length)
                {
                    if (length < 0)
                    {
                        System.ThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.length);
                    }
                    return (length >> 2) * 3;
                }

                /// <summary>
                /// Encodes the span of binary data (in-place) into UTF-8 encoded text represented as base 64. 
                /// The encoded text output is larger than the binary data contained in the input (the operation inflates the data).
                /// </summary>
                /// <param name="buffer">The input span that contains binary data that needs to be encoded.
                /// Because the method performs an in-place conversion, it needs to be large enough to store the result of the operation.
                /// </param>
                /// <param name="bytesWritten">The number of bytes of binary data contained within the buffer that needs to be encoded. 
                /// This value must be smaller than the buffer length.</param>
                /// <returns>When this method returns, contains the number of bytes written into the buffer.</returns>
                /// <remarks>
                /// <para>The return value can be as follows:</para>
                /// <para><see cref="OperationStatus.Done"/>: Processing of the entire input span succeeded. </para>
                /// <para><see cref="OperationStatus.DestinationTooSmall"/>: 
                /// There is not enough space in the output span to write the decoded input. </para>
                /// <para>This method cannot return OperationStatus.NeedMoreData and OperationStatus.InvalidData.</para>
                /// </remarks>
                public static OperationStatus DecodeFromUtf8InPlace(Span<byte> buffer, out int bytesWritten)
                {
                    int length = buffer.Length;
                    int num = 0;
                    int num2 = 0;
                    if (length == (length >> 2) * 4)
                    {
                        if (length == 0)
                        {
                            goto IL_016d;
                        }
                        ref byte reference = ref MemoryMarshal.GetReference(buffer);
                        ref sbyte reference2 = ref s_decodingMap[0];
                        while (num < length - 4)
                        {
                            int num3 = Decode(ref Unsafe.Add(ref reference, num), ref reference2);
                            if (num3 >= 0)
                            {
                                WriteThreeLowOrderBytes(ref Unsafe.Add(ref reference, num2), num3);
                                num2 += 3;
                                num += 4;
                                continue;
                            }
                            goto IL_0172;
                        }
                        int elementOffset = Unsafe.Add(ref reference, length - 4);
                        int elementOffset2 = Unsafe.Add(ref reference, length - 3);
                        int num4 = Unsafe.Add(ref reference, length - 2);
                        int num5 = Unsafe.Add(ref reference, length - 1);
                        elementOffset = Unsafe.Add(ref reference2, elementOffset);
                        elementOffset2 = Unsafe.Add(ref reference2, elementOffset2);
                        elementOffset <<= 18;
                        elementOffset2 <<= 12;
                        elementOffset |= elementOffset2;
                        if (num5 != 61)
                        {
                            num4 = Unsafe.Add(ref reference2, num4);
                            num5 = Unsafe.Add(ref reference2, num5);
                            num4 <<= 6;
                            elementOffset |= num5;
                            elementOffset |= num4;
                            if (elementOffset >= 0)
                            {
                                WriteThreeLowOrderBytes(ref Unsafe.Add(ref reference, num2), elementOffset);
                                num2 += 3;
                                goto IL_016d;
                            }
                        }
                        else if (num4 != 61)
                        {
                            num4 = Unsafe.Add(ref reference2, num4);
                            num4 <<= 6;
                            elementOffset |= num4;
                            if (elementOffset >= 0)
                            {
                                Unsafe.Add(ref reference, num2) = (byte)(elementOffset >> 16);
                                Unsafe.Add(ref reference, num2 + 1) = (byte)(elementOffset >> 8);
                                num2 += 2;
                                goto IL_016d;
                            }
                        }
                        else if (elementOffset >= 0)
                        {
                            Unsafe.Add(ref reference, num2) = (byte)(elementOffset >> 16);
                            num2++;
                            goto IL_016d;
                        }
                    }
                    goto IL_0172;
                IL_016d:
                    bytesWritten = num2;
                    return OperationStatus.Done;
                IL_0172:
                    bytesWritten = num2;
                    return OperationStatus.InvalidData;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                private static int Decode(ref byte encodedBytes, ref sbyte decodingMap)
                {
                    int elementOffset = encodedBytes;
                    int elementOffset2 = Unsafe.Add(ref encodedBytes, 1);
                    int elementOffset3 = Unsafe.Add(ref encodedBytes, 2);
                    int elementOffset4 = Unsafe.Add(ref encodedBytes, 3);
                    elementOffset = Unsafe.Add(ref decodingMap, elementOffset);
                    elementOffset2 = Unsafe.Add(ref decodingMap, elementOffset2);
                    elementOffset3 = Unsafe.Add(ref decodingMap, elementOffset3);
                    elementOffset4 = Unsafe.Add(ref decodingMap, elementOffset4);
                    elementOffset <<= 18;
                    elementOffset2 <<= 12;
                    elementOffset3 <<= 6;
                    elementOffset |= elementOffset4;
                    elementOffset2 |= elementOffset3;
                    return elementOffset | elementOffset2;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                private static void WriteThreeLowOrderBytes(ref byte destination, int value)
                {
                    destination = (byte)(value >> 16);
                    Unsafe.Add(ref destination, 1) = (byte)(value >> 8);
                    Unsafe.Add(ref destination, 2) = (byte)value;
                }

                /// <summary>
                /// Encodes the span of binary data into UTF-8 encoded text represented as base 64.
                /// </summary>
                /// <param name="bytes">The input span that contains binary data that needs to be encoded.</param>
                /// <param name="utf8">The output span that contains the result of the operation, that is, the UTF-8 encoded text in base 64.</param>
                /// <param name="bytesConsumed">When this method returns, contains the number of input bytes consumed during the operation. 
                /// This can be used to slice the input for subsequent calls, if necessary.</param>
                /// <param name="bytesWritten">When this method returns, contains the number of bytes written into the output span. 
                /// This can be used to slice the output for subsequent calls, if necessary.
                /// </param>
                /// <param name="isFinalBlock"><c>true</c> (the default) to indicate that the input span contains the entire data to encode. 
                /// <c>false</c> to indicate that the input span contains partial data with more data to follow.</param>
                /// <returns>One of the enumeration values that indicates the status of the encoding operation.</returns>
                /// <remarks>
                /// <para>
                /// This method cannot return <see cref="OperationStatus.InvalidData"/> since that is not possible for base-64 encoding.
                /// </para>
                /// <para><see cref="OperationStatus.Done"/>: Processing of the entire input span succeeded. </para>
                /// <para><see cref="OperationStatus.DestinationTooSmall"/>: 
                /// There is not enough space in the output span to write the decoded input. </para>
                /// <para><see cref="OperationStatus.NeedMoreData"/>: 
                /// isFinalBlock is false and the input is not a multiple of 4. Otherwise, the partial input is considered InvalidData. </para>
                /// </remarks>
                public static OperationStatus EncodeToUtf8(ReadOnlySpan<byte> bytes, Span<byte> utf8, out int bytesConsumed, out int bytesWritten, bool isFinalBlock = true)
                {
                    ref byte reference = ref MemoryMarshal.GetReference(bytes);
                    ref byte reference2 = ref MemoryMarshal.GetReference(utf8);
                    int length = bytes.Length;
                    int length2 = utf8.Length;
                    int num = 0;
                    num = ((length > 1610612733 || length2 < GetMaxEncodedToUtf8Length(length)) ? ((length2 >> 2) * 3 - 2) : (length - 2));
                    int i = 0;
                    int num2 = 0;
                    int num3 = 0;
                    ref byte encodingMap = ref s_encodingMap[0];
                    for (; i < num; i += 3)
                    {
                        num3 = Encode(ref Unsafe.Add(ref reference, i), ref encodingMap);
                        Unsafe.WriteUnaligned(ref Unsafe.Add(ref reference2, num2), num3);
                        num2 += 4;
                    }
                    if (num == length - 2)
                    {
                        if (isFinalBlock)
                        {
                            if (i == length - 1)
                            {
                                num3 = EncodeAndPadTwo(ref Unsafe.Add(ref reference, i), ref encodingMap);
                                Unsafe.WriteUnaligned(ref Unsafe.Add(ref reference2, num2), num3);
                                num2 += 4;
                                i++;
                            }
                            else if (i == length - 2)
                            {
                                num3 = EncodeAndPadOne(ref Unsafe.Add(ref reference, i), ref encodingMap);
                                Unsafe.WriteUnaligned(ref Unsafe.Add(ref reference2, num2), num3);
                                num2 += 4;
                                i += 2;
                            }
                            bytesConsumed = i;
                            bytesWritten = num2;
                            return OperationStatus.Done;
                        }
                        bytesConsumed = i;
                        bytesWritten = num2;
                        return OperationStatus.NeedMoreData;
                    }
                    bytesConsumed = i;
                    bytesWritten = num2;
                    return OperationStatus.DestinationTooSmall;
                }

                /// <summary>
                /// Returns the maximum length (in bytes) of the result if you were to encode binary data within a byte span with the specified length.
                /// </summary>
                /// <param name="length">The size of the byte span.</param>
                /// <returns>The maximum length (in bytes) of the result.</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static int GetMaxEncodedToUtf8Length(int length)
                {
                    if ((uint)length > 1610612733u)
                    {
                        System.ThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.length);
                    }
                    return (length + 2) / 3 * 4;
                }

                /// <summary>
                /// Encodes the span of binary data (in-place) into UTF-8 encoded text represented as base 64. 
                /// The encoded text output is larger than the binary data contained in the input (the operation inflates the data).
                /// </summary>
                /// <param name="buffer">The input span that contains binary data that needs to be encoded. 
                /// Because the method performs an in-place conversion, it needs to be large enough to store the result of the operation. </param>
                /// <param name="dataLength">The number of bytes of binary data contained within the buffer that needs to be encoded. 
                /// This value must be smaller than the buffer length.</param>
                /// <param name="bytesWritten">When this method returns, contains the number of bytes written into the buffer.</param>
                /// <returns>One of the enumeration values that indicates the status of the encoding operation.</returns>
                /// <remarks>
                /// <para>The return value can be as follows:</para>
                /// <para><see cref="OperationStatus.Done"/>: Processing of the entire input span succeeded. </para>
                /// <para><see cref="OperationStatus.DestinationTooSmall"/>: 
                /// There is not enough space in the output span to write the decoded input. </para>
                /// <para>This method cannot return OperationStatus.NeedMoreData and OperationStatus.InvalidData.</para>
                /// </remarks>
                public static OperationStatus EncodeToUtf8InPlace(Span<byte> buffer, int dataLength, out int bytesWritten)
                {
                    int maxEncodedToUtf8Length = GetMaxEncodedToUtf8Length(dataLength);
                    if (buffer.Length >= maxEncodedToUtf8Length)
                    {
                        int num = dataLength - dataLength / 3 * 3;
                        int num2 = maxEncodedToUtf8Length - 4;
                        int num3 = dataLength - num;
                        int num4 = 0;
                        ref byte encodingMap = ref s_encodingMap[0];
                        ref byte reference = ref MemoryMarshal.GetReference(buffer);
                        switch (num)
                        {
                            case 1:
                                num4 = EncodeAndPadTwo(ref Unsafe.Add(ref reference, num3), ref encodingMap);
                                Unsafe.WriteUnaligned(ref Unsafe.Add(ref reference, num2), num4);
                                num2 -= 4;
                                break;
                            default:
                                num4 = EncodeAndPadOne(ref Unsafe.Add(ref reference, num3), ref encodingMap);
                                Unsafe.WriteUnaligned(ref Unsafe.Add(ref reference, num2), num4);
                                num2 -= 4;
                                break;
                            case 0:
                                break;
                        }
                        for (num3 -= 3; num3 >= 0; num3 -= 3)
                        {
                            num4 = Encode(ref Unsafe.Add(ref reference, num3), ref encodingMap);
                            Unsafe.WriteUnaligned(ref Unsafe.Add(ref reference, num2), num4);
                            num2 -= 4;
                        }
                        bytesWritten = maxEncodedToUtf8Length;
                        return OperationStatus.Done;
                    }
                    bytesWritten = 0;
                    return OperationStatus.DestinationTooSmall;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                private static int Encode(ref byte threeBytes, ref byte encodingMap)
                {
                    int num = (threeBytes << 16) | (Unsafe.Add(ref threeBytes, 1) << 8) | Unsafe.Add(ref threeBytes, 2);
                    int num2 = Unsafe.Add(ref encodingMap, num >> 18);
                    int num3 = Unsafe.Add(ref encodingMap, (num >> 12) & 0x3F);
                    int num4 = Unsafe.Add(ref encodingMap, (num >> 6) & 0x3F);
                    int num5 = Unsafe.Add(ref encodingMap, num & 0x3F);
                    return num2 | (num3 << 8) | (num4 << 16) | (num5 << 24);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                private static int EncodeAndPadOne(ref byte twoBytes, ref byte encodingMap)
                {
                    int num = (twoBytes << 16) | (Unsafe.Add(ref twoBytes, 1) << 8);
                    int num2 = Unsafe.Add(ref encodingMap, num >> 18);
                    int num3 = Unsafe.Add(ref encodingMap, (num >> 12) & 0x3F);
                    int num4 = Unsafe.Add(ref encodingMap, (num >> 6) & 0x3F);
                    return num2 | (num3 << 8) | (num4 << 16) | 0x3D000000;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                private static int EncodeAndPadTwo(ref byte oneByte, ref byte encodingMap)
                {
                    int num = oneByte << 8;
                    int num2 = Unsafe.Add(ref encodingMap, num >> 10);
                    int num3 = Unsafe.Add(ref encodingMap, (num >> 4) & 0x3F);
                    return num2 | (num3 << 8) | 0x3D0000 | 0x3D000000;
                }
            }

            internal enum HexCasing : System.UInt32
            {
                Uppercase = 0u,
                Lowercase = 8224u
            }

            internal static class FormattingHelpers
            {

                internal const string HexTableLower = "0123456789abcdef";

                internal const string HexTableUpper = "0123456789ABCDEF";

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static char GetSymbolOrDefault(in StandardFormat format, char defaultSymbol)
                {
                    char c = format.Symbol;
                    if (c == '\0' && format.Precision == 0)
                    {
                        c = defaultSymbol;
                    }
                    return c;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static void FillWithAsciiZeros(Span<byte> buffer)
                {
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        buffer[i] = 48;
                    }
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static void WriteHexByte(byte value, Span<byte> buffer, int startingIndex = 0, HexCasing casing = HexCasing.Uppercase)
                {
                    uint num = (uint)(((value & 0xF0) << 4) + (value & 0xF) - 35209);
                    uint num2 = ((((0 - num) & 0x7070) >> 4) + num + 47545) | (uint)casing;
                    buffer[startingIndex + 1] = (byte)num2;
                    buffer[startingIndex] = (byte)(num2 >> 8);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static void WriteDigits(ulong value, Span<byte> buffer)
                {
                    for (int num = buffer.Length - 1; num >= 1; num--)
                    {
                        ulong num2 = 48 + value;
                        value /= 10uL;
                        buffer[num] = (byte)(num2 - value * 10);
                    }
                    buffer[0] = (byte)(48 + value);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static void WriteDigitsWithGroupSeparator(ulong value, Span<byte> buffer)
                {
                    int num = 0;
                    for (int num2 = buffer.Length - 1; num2 >= 1; num2--)
                    {
                        ulong num3 = 48 + value;
                        value /= 10uL;
                        buffer[num2] = (byte)(num3 - value * 10);
                        if (num == 2)
                        {
                            buffer[--num2] = 44;
                            num = 0;
                        }
                        else
                        {
                            num++;
                        }
                    }
                    buffer[0] = (byte)(48 + value);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static void WriteDigits(uint value, Span<byte> buffer)
                {
                    for (int num = buffer.Length - 1; num >= 1; num--)
                    {
                        uint num2 = 48 + value;
                        value /= 10u;
                        buffer[num] = (byte)(num2 - value * 10);
                    }
                    buffer[0] = (byte)(48 + value);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static void WriteFourDecimalDigits(uint value, Span<byte> buffer, int startingIndex = 0)
                {
                    uint num = 48 + value;
                    value /= 10u;
                    buffer[startingIndex + 3] = (byte)(num - value * 10);
                    num = 48 + value;
                    value /= 10u;
                    buffer[startingIndex + 2] = (byte)(num - value * 10);
                    num = 48 + value;
                    value /= 10u;
                    buffer[startingIndex + 1] = (byte)(num - value * 10);
                    buffer[startingIndex] = (byte)(48 + value);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static void WriteTwoDecimalDigits(uint value, Span<byte> buffer, int startingIndex = 0)
                {
                    uint num = 48 + value;
                    value /= 10u;
                    buffer[startingIndex + 1] = (byte)(num - value * 10);
                    buffer[startingIndex] = (byte)(48 + value);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static ulong DivMod(ulong numerator, ulong denominator, out ulong modulo)
                {
                    ulong num = numerator / denominator;
                    modulo = numerator - num * denominator;
                    return num;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static uint DivMod(uint numerator, uint denominator, out uint modulo)
                {
                    uint num = numerator / denominator;
                    modulo = numerator - num * denominator;
                    return num;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static int CountDecimalTrailingZeros(uint value, out uint valueWithoutTrailingZeros)
                {
                    int num = 0;
                    if (value != 0)
                    {
                        while (true)
                        {
                            uint modulo;
                            uint num2 = DivMod(value, 10u, out modulo);
                            if (modulo != 0)
                            {
                                break;
                            }
                            value = num2;
                            num++;
                        }
                    }
                    valueWithoutTrailingZeros = value;
                    return num;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static int CountDigits(ulong value)
                {
                    int num = 1;
                    uint num2;
                    if (value >= 10000000)
                    {
                        if (value >= 100000000000000L)
                        {
                            num2 = (uint)(value / 100000000000000uL);
                            num += 14;
                        }
                        else
                        {
                            num2 = (uint)(value / 10000000uL);
                            num += 7;
                        }
                    }
                    else
                    {
                        num2 = (uint)value;
                    }
                    if (num2 >= 10)
                    {
                        num = ((num2 < 100) ? (num + 1) : ((num2 < 1000) ? (num + 2) : ((num2 < 10000) ? (num + 3) : ((num2 < 100000) ? (num + 4) : ((num2 >= 1000000) ? (num + 6) : (num + 5))))));
                    }
                    return num;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static int CountDigits(uint value)
                {
                    int num = 1;
                    if (value >= 100000)
                    {
                        value /= 100000u;
                        num += 5;
                    }
                    if (value >= 10)
                    {
                        num = ((value < 100) ? (num + 1) : ((value < 1000) ? (num + 2) : ((value >= 10000) ? (num + 4) : (num + 3))));
                    }
                    return num;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static int CountHexDigits(ulong value)
                {
                    int num = 1;
                    if (value > uint.MaxValue)
                    {
                        num += 8;
                        value >>= 32;
                    }
                    if (value > 65535)
                    {
                        num += 4;
                        value >>= 16;
                    }
                    if (value > 255)
                    {
                        num += 2;
                        value >>= 8;
                    }
                    if (value > 15)
                    {
                        num++;
                    }
                    return num;
                }
            }

            internal static class ParserHelpers
            {
                public const int ByteOverflowLength = 3;

                public const int ByteOverflowLengthHex = 2;

                public const int UInt16OverflowLength = 5;

                public const int UInt16OverflowLengthHex = 4;

                public const int UInt32OverflowLength = 10;

                public const int UInt32OverflowLengthHex = 8;

                public const int UInt64OverflowLength = 20;

                public const int UInt64OverflowLengthHex = 16;

                public const int SByteOverflowLength = 3;

                public const int SByteOverflowLengthHex = 2;

                public const int Int16OverflowLength = 5;

                public const int Int16OverflowLengthHex = 4;

                public const int Int32OverflowLength = 10;

                public const int Int32OverflowLengthHex = 8;

                public const int Int64OverflowLength = 19;

                public const int Int64OverflowLengthHex = 16;

                public static readonly byte[] s_hexLookup = new byte[256]
                {
                    255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
                    255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
                    255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
                    255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
                    255, 255, 255, 255, 255, 255, 255, 255, 0, 1,
                    2, 3, 4, 5, 6, 7, 8, 9, 255, 255,
                    255, 255, 255, 255, 255, 10, 11, 12, 13, 14,
                    15, 255, 255, 255, 255, 255, 255, 255, 255, 255,
                    255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
                    255, 255, 255, 255, 255, 255, 255, 10, 11, 12,
                    13, 14, 15, 255, 255, 255, 255, 255, 255, 255,
                    255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
                    255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
                    255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
                    255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
                    255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
                    255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
                    255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
                    255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
                    255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
                    255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
                    255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
                    255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
                    255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
                    255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
                    255, 255, 255, 255, 255, 255
                };

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static bool IsDigit(int i)
                {
                    return (uint)(i - 48) <= 9u;
                }
            }

            internal static class Utf8Constants
            {
                public const byte Colon = 58;

                public const byte Comma = 44;

                public const byte Minus = 45;

                public const byte Period = 46;

                public const byte Plus = 43;

                public const byte Slash = 47;

                public const byte Space = 32;

                public const byte Hyphen = 45;

                public const byte Separator = 44;

                public const int GroupSize = 3;

                public static readonly TimeSpan s_nullUtcOffset = TimeSpan.MinValue;

                public const int DateTimeMaxUtcOffsetHours = 14;

                public const int DateTimeNumFractionDigits = 7;

                public const int MaxDateTimeFraction = 9999999;

                public const ulong BillionMaxUIntValue = 4294967295000000000uL;

                public const uint Billion = 1000000000u;
            }

            [StructLayout(LayoutKind.Explicit)]
            internal struct DecomposedGuid
            {
                [FieldOffset(0)]
                public Guid Guid;

                [FieldOffset(0)]
                public byte Byte00;

                [FieldOffset(1)]
                public byte Byte01;

                [FieldOffset(2)]
                public byte Byte02;

                [FieldOffset(3)]
                public byte Byte03;

                [FieldOffset(4)]
                public byte Byte04;

                [FieldOffset(5)]
                public byte Byte05;

                [FieldOffset(6)]
                public byte Byte06;

                [FieldOffset(7)]
                public byte Byte07;

                [FieldOffset(8)]
                public byte Byte08;

                [FieldOffset(9)]
                public byte Byte09;

                [FieldOffset(10)]
                public byte Byte10;

                [FieldOffset(11)]
                public byte Byte11;

                [FieldOffset(12)]
                public byte Byte12;

                [FieldOffset(13)]
                public byte Byte13;

                [FieldOffset(14)]
                public byte Byte14;

                [FieldOffset(15)]
                public byte Byte15;
            }

            /// <summary>
            /// Provides static methods to format common data types as UTF-8 strings.
            /// </summary>
            public static class Utf8Formatter
            {

                private const byte TimeMarker = 84;

                private const byte UtcMarker = 90;

                private const byte GMT1 = 71;

                private const byte GMT2 = 77;

                private const byte GMT3 = 84;

                private const byte GMT1Lowercase = 103;

                private const byte GMT2Lowercase = 109;

                private const byte GMT3Lowercase = 116;

                private static readonly uint[] DayAbbreviations = new uint[7] { 7238995u, 7237453u, 6649172u, 6579543u, 7694420u, 6910534u, 7627091u };

                private static readonly uint[] DayAbbreviationsLowercase = new uint[7] { 7239027u, 7237485u, 6649204u, 6579575u, 7694452u, 6910566u, 7627123u };

                private static readonly uint[] MonthAbbreviations = new uint[12]
                {
                    7233866u, 6448454u, 7496013u, 7499841u, 7954765u, 7238986u, 7107914u, 6780225u, 7365971u, 7627599u,
                    7761742u, 6513988u
                };

                private static readonly uint[] MonthAbbreviationsLowercase = new uint[12]
                {
                    7233898u, 6448486u, 7496045u, 7499873u, 7954797u, 7239018u, 7107946u, 6780257u, 7366003u, 7627631u,
                    7761774u, 6514020u
                };

                private const byte OpenBrace = 123;

                private const byte CloseBrace = 125;

                private const byte OpenParen = 40;

                private const byte CloseParen = 41;

                private const byte Dash = 45;

                /// <summary>
                /// Formats a <see cref="System.Boolean"/> as a UTF-8 <see cref="System.String"/> .
                /// </summary>
                /// <param name="value">The value to format.</param>
                /// <param name="destination">The buffer to write the UTF8-formatted value to.</param>
                /// <param name="bytesWritten">When the method returns, contains the length of the formatted text in bytes.</param>
                /// <param name="format">The standard format to use.</param>
                /// <returns><c>true</c> if the formatting operation succeeds; 
                /// <c>false</c> if <paramref name="destination"/> is too small.</returns>
                /// <remarks>
                /// Format Supported:
                /// <list type="table">
                ///     <listheader>
                ///         Format <see cref="System.String"/> Example result <see cref="System.String"/>
                ///     </listheader>
                ///     <item>G(default)    True/False</item>
                ///     <item>I                    true/false</item>
                /// </list>
                /// If the method fails, iteratively increase the size of the buffer and retry until it succeeds.
                /// </remarks>
                public static bool TryFormat(bool value, Span<byte> destination, out int bytesWritten, StandardFormat format = default(StandardFormat))
                {
                    char symbolOrDefault = FormattingHelpers.GetSymbolOrDefault(in format, 'G');
                    if (value)
                    {
                        if (symbolOrDefault == 'G')
                        {
                            if (BinaryPrimitives.TryWriteUInt32BigEndian(destination, 1416787301u))
                            {
                                goto IL_0033;
                            }
                        }
                        else
                        {
                            if (symbolOrDefault != 'l')
                            {
                                goto IL_0083;
                            }
                            if (BinaryPrimitives.TryWriteUInt32BigEndian(destination, 1953658213u))
                            {
                                goto IL_0033;
                            }
                        }
                    }
                    else if (symbolOrDefault == 'G')
                    {
                        if (4u < (uint)destination.Length)
                        {
                            BinaryPrimitives.WriteUInt32BigEndian(destination, 1180789875u);
                            goto IL_006e;
                        }
                    }
                    else
                    {
                        if (symbolOrDefault != 'l')
                        {
                            goto IL_0083;
                        }
                        if (4u < (uint)destination.Length)
                        {
                            BinaryPrimitives.WriteUInt32BigEndian(destination, 1717660787u);
                            goto IL_006e;
                        }
                    }
                    bytesWritten = 0;
                    return false;
                IL_0033:
                    bytesWritten = 4;
                    return true;
                IL_0083:
                    return System.ThrowHelper.TryFormatThrowFormatException(out bytesWritten);
                IL_006e:
                    destination[4] = 101;
                    bytesWritten = 5;
                    return true;
                }

                /// <summary>
                /// Formats a <see cref="System.Boolean"/> as a UTF-8 <see cref="System.String"/> .
                /// </summary>
                /// <param name="value">The value to format.</param>
                /// <param name="destination">The buffer to write the UTF8-formatted value to.</param>
                /// <param name="bytesWritten">When the method returns, contains the length of the formatted text in bytes.</param>
                /// <param name="format">The standard format to use.</param>
                /// <returns><c>true</c> if the formatting operation succeeds; 
                /// <c>false</c> if <paramref name="destination"/> is too small.</returns>
                public static bool TryFormat(DateTimeOffset value, Span<byte> destination, out int bytesWritten, StandardFormat format = default(StandardFormat))
                {
                    TimeSpan offset = Utf8Constants.s_nullUtcOffset;
                    char c = format.Symbol;
                    if (format.IsDefault)
                    {
                        c = 'G';
                        offset = value.Offset;
                    }
                    return c switch
                    {
                        'R' => TryFormatDateTimeR(value.UtcDateTime, destination, out bytesWritten),
                        'l' => TryFormatDateTimeL(value.UtcDateTime, destination, out bytesWritten),
                        'O' => TryFormatDateTimeO(value.DateTime, value.Offset, destination, out bytesWritten),
                        'G' => TryFormatDateTimeG(value.DateTime, offset, destination, out bytesWritten),
                        _ => System.ThrowHelper.TryFormatThrowFormatException(out bytesWritten),
                    };
                }

                /// <summary>
                /// Formats a <see cref="System.Boolean"/> as a UTF-8 <see cref="System.String"/> .
                /// </summary>
                /// <param name="value">The value to format.</param>
                /// <param name="destination">The buffer to write the UTF8-formatted value to.</param>
                /// <param name="bytesWritten">When the method returns, contains the length of the formatted text in bytes.</param>
                /// <param name="format">The standard format to use.</param>
                /// <returns><c>true</c> if the formatting operation succeeds; 
                /// <c>false</c> if <paramref name="destination"/> is too small.</returns>
                /// <remarks>
                /// Format Supported:
                /// <list type="table">
                ///     <listheader>
                ///         Format <see cref="System.String"/> Example result <see cref="System.String"/>  Comments
                ///     </listheader>
                ///     <item>G(default)    05/25/2017 10:30:15</item>
                ///     <item>R	Tue, 03 Jan 2017 08:08:05 GMT	(RFC 1123)</item>
                ///     <item>l	tue, 03 jan 2017 08:08:05 gmt	(Lowercase RFC 1123)</item>
                ///     <item>O	2017-06-12T05:30:45.7680000-07:00	(Round-trippable)</item>
                /// </list>
                /// If the method fails, iteratively increase the size of the buffer and retry until it succeeds.
                /// </remarks>
                public static bool TryFormat(DateTime value, Span<byte> destination, out int bytesWritten, StandardFormat format = default(StandardFormat))
                {
                    return FormattingHelpers.GetSymbolOrDefault(in format, 'G') switch
                    {
                        'R' => TryFormatDateTimeR(value, destination, out bytesWritten),
                        'l' => TryFormatDateTimeL(value, destination, out bytesWritten),
                        'O' => TryFormatDateTimeO(value, Utf8Constants.s_nullUtcOffset, destination, out bytesWritten),
                        'G' => TryFormatDateTimeG(value, Utf8Constants.s_nullUtcOffset, destination, out bytesWritten),
                        _ => System.ThrowHelper.TryFormatThrowFormatException(out bytesWritten),
                    };
                }

                private static bool TryFormatDateTimeG(DateTime value, TimeSpan offset, Span<byte> destination, out int bytesWritten)
                {
                    int num = 19;
                    if (offset != Utf8Constants.s_nullUtcOffset)
                    {
                        num += 7;
                    }
                    if (destination.Length < num)
                    {
                        bytesWritten = 0;
                        return false;
                    }
                    bytesWritten = num;
                    byte b = destination[18];
                    FormattingHelpers.WriteTwoDecimalDigits((uint)value.Month, destination);
                    destination[2] = 47;
                    FormattingHelpers.WriteTwoDecimalDigits((uint)value.Day, destination, 3);
                    destination[5] = 47;
                    FormattingHelpers.WriteFourDecimalDigits((uint)value.Year, destination, 6);
                    destination[10] = 32;
                    FormattingHelpers.WriteTwoDecimalDigits((uint)value.Hour, destination, 11);
                    destination[13] = 58;
                    FormattingHelpers.WriteTwoDecimalDigits((uint)value.Minute, destination, 14);
                    destination[16] = 58;
                    FormattingHelpers.WriteTwoDecimalDigits((uint)value.Second, destination, 17);
                    if (offset != Utf8Constants.s_nullUtcOffset)
                    {
                        byte b2;
                        if (offset < default(TimeSpan))
                        {
                            b2 = 45;
                            offset = TimeSpan.FromTicks(-offset.Ticks);
                        }
                        else
                        {
                            b2 = 43;
                        }
                        FormattingHelpers.WriteTwoDecimalDigits((uint)offset.Minutes, destination, 24);
                        destination[23] = 58;
                        FormattingHelpers.WriteTwoDecimalDigits((uint)offset.Hours, destination, 21);
                        destination[20] = b2;
                        destination[19] = 32;
                    }
                    return true;
                }

                private static bool TryFormatDateTimeO(DateTime value, TimeSpan offset, Span<byte> destination, out int bytesWritten)
                {
                    int num = 27;
                    DateTimeKind dateTimeKind = DateTimeKind.Local;
                    if (offset == Utf8Constants.s_nullUtcOffset)
                    {
                        dateTimeKind = value.Kind;
                        switch (dateTimeKind)
                        {
                            case DateTimeKind.Local:
                                offset = TimeZoneInfo.Local.GetUtcOffset(value);
                                num += 6;
                                break;
                            case DateTimeKind.Utc:
                                num++;
                                break;
                        }
                    }
                    else
                    {
                        num += 6;
                    }
                    if (destination.Length < num)
                    {
                        bytesWritten = 0;
                        return false;
                    }
                    bytesWritten = num;
                    byte b = destination[26];
                    FormattingHelpers.WriteFourDecimalDigits((uint)value.Year, destination);
                    destination[4] = 45;
                    FormattingHelpers.WriteTwoDecimalDigits((uint)value.Month, destination, 5);
                    destination[7] = 45;
                    FormattingHelpers.WriteTwoDecimalDigits((uint)value.Day, destination, 8);
                    destination[10] = 84;
                    FormattingHelpers.WriteTwoDecimalDigits((uint)value.Hour, destination, 11);
                    destination[13] = 58;
                    FormattingHelpers.WriteTwoDecimalDigits((uint)value.Minute, destination, 14);
                    destination[16] = 58;
                    FormattingHelpers.WriteTwoDecimalDigits((uint)value.Second, destination, 17);
                    destination[19] = 46;
                    FormattingHelpers.WriteDigits((uint)((ulong)value.Ticks % 10000000uL), destination.Slice(20, 7));
                    switch (dateTimeKind)
                    {
                        case DateTimeKind.Local:
                            {
                                byte b2;
                                if (offset < default(TimeSpan))
                                {
                                    b2 = 45;
                                    offset = TimeSpan.FromTicks(-offset.Ticks);
                                }
                                else
                                {
                                    b2 = 43;
                                }
                                FormattingHelpers.WriteTwoDecimalDigits((uint)offset.Minutes, destination, 31);
                                destination[30] = 58;
                                FormattingHelpers.WriteTwoDecimalDigits((uint)offset.Hours, destination, 28);
                                destination[27] = b2;
                                break;
                            }
                        case DateTimeKind.Utc:
                            destination[27] = 90;
                            break;
                    }
                    return true;
                }

                private static bool TryFormatDateTimeR(DateTime value, Span<byte> destination, out int bytesWritten)
                {
                    if (28u >= (uint)destination.Length)
                    {
                        bytesWritten = 0;
                        return false;
                    }
                    uint num = DayAbbreviations[(int)value.DayOfWeek];
                    destination[0] = (byte)num;
                    num >>= 8;
                    destination[1] = (byte)num;
                    num >>= 8;
                    destination[2] = (byte)num;
                    destination[3] = 44;
                    destination[4] = 32;
                    FormattingHelpers.WriteTwoDecimalDigits((uint)value.Day, destination, 5);
                    destination[7] = 32;
                    uint num2 = MonthAbbreviations[value.Month - 1];
                    destination[8] = (byte)num2;
                    num2 >>= 8;
                    destination[9] = (byte)num2;
                    num2 >>= 8;
                    destination[10] = (byte)num2;
                    destination[11] = 32;
                    FormattingHelpers.WriteFourDecimalDigits((uint)value.Year, destination, 12);
                    destination[16] = 32;
                    FormattingHelpers.WriteTwoDecimalDigits((uint)value.Hour, destination, 17);
                    destination[19] = 58;
                    FormattingHelpers.WriteTwoDecimalDigits((uint)value.Minute, destination, 20);
                    destination[22] = 58;
                    FormattingHelpers.WriteTwoDecimalDigits((uint)value.Second, destination, 23);
                    destination[25] = 32;
                    destination[26] = 71;
                    destination[27] = 77;
                    destination[28] = 84;
                    bytesWritten = 29;
                    return true;
                }

                private static bool TryFormatDateTimeL(DateTime value, Span<byte> destination, out int bytesWritten)
                {
                    if (28u >= (uint)destination.Length)
                    {
                        bytesWritten = 0;
                        return false;
                    }
                    uint num = DayAbbreviationsLowercase[(int)value.DayOfWeek];
                    destination[0] = (byte)num;
                    num >>= 8;
                    destination[1] = (byte)num;
                    num >>= 8;
                    destination[2] = (byte)num;
                    destination[3] = 44;
                    destination[4] = 32;
                    FormattingHelpers.WriteTwoDecimalDigits((uint)value.Day, destination, 5);
                    destination[7] = 32;
                    uint num2 = MonthAbbreviationsLowercase[value.Month - 1];
                    destination[8] = (byte)num2;
                    num2 >>= 8;
                    destination[9] = (byte)num2;
                    num2 >>= 8;
                    destination[10] = (byte)num2;
                    destination[11] = 32;
                    FormattingHelpers.WriteFourDecimalDigits((uint)value.Year, destination, 12);
                    destination[16] = 32;
                    FormattingHelpers.WriteTwoDecimalDigits((uint)value.Hour, destination, 17);
                    destination[19] = 58;
                    FormattingHelpers.WriteTwoDecimalDigits((uint)value.Minute, destination, 20);
                    destination[22] = 58;
                    FormattingHelpers.WriteTwoDecimalDigits((uint)value.Second, destination, 23);
                    destination[25] = 32;
                    destination[26] = 103;
                    destination[27] = 109;
                    destination[28] = 116;
                    bytesWritten = 29;
                    return true;
                }

                /// <summary>
                /// Formats a <see cref="System.Boolean"/> as a UTF-8 <see cref="System.String"/> .
                /// </summary>
                /// <param name="value">The value to format.</param>
                /// <param name="destination">The buffer to write the UTF8-formatted value to.</param>
                /// <param name="bytesWritten">When the method returns, contains the length of the formatted text in bytes.</param>
                /// <param name="format">The standard format to use.</param>
                /// <returns><c>true</c> if the formatting operation succeeds; 
                /// <c>false</c> if <paramref name="destination"/> is too small.</returns>
                /// <remarks>
                /// Format Supported:
                /// <list type="table">
                ///     <listheader>
                ///         Format <see cref="System.String"/> Example result <see cref="System.String"/> Comments
                ///     </listheader>
                ///     <item>G/g(default)    </item>
                ///     <item>F/f	12.45	Fixed point</item>
                ///     <item>E/e	1.245000e1	Exponential</item>
                /// </list>
                /// If the method fails, iteratively increase the size of the buffer and retry until it succeeds.
                /// </remarks>
                public static bool TryFormat(decimal value, Span<byte> destination, out int bytesWritten, StandardFormat format = default(StandardFormat))
                {
                    if (format.IsDefault)
                    {
                        format = 'G';
                    }
                    switch (format.Symbol)
                    {
                        case 'G':
                        case 'g':
                            {
                                if (format.Precision != byte.MaxValue)
                                {
                                    throw new NotSupportedException(MDCFR.Properties.Resources.Argument_GWithPrecisionNotSupported);
                                }
                                NumberBuffer number3 = default(NumberBuffer);
                                System.Number.DecimalToNumber(value, ref number3);
                                if (number3.Digits[0] == 0)
                                {
                                    number3.IsNegative = false;
                                }
                                return TryFormatDecimalG(ref number3, destination, out bytesWritten);
                            }
                        case 'F':
                        case 'f':
                            {
                                NumberBuffer number2 = default(NumberBuffer);
                                System.Number.DecimalToNumber(value, ref number2);
                                byte b2 = (byte)((format.Precision == byte.MaxValue) ? 2 : format.Precision);
                                System.Number.RoundNumber(ref number2, number2.Scale + b2);
                                return TryFormatDecimalF(ref number2, destination, out bytesWritten, b2);
                            }
                        case 'E':
                        case 'e':
                            {
                                NumberBuffer number = default(NumberBuffer);
                                System.Number.DecimalToNumber(value, ref number);
                                byte b = (byte)((format.Precision == byte.MaxValue) ? 6 : format.Precision);
                                System.Number.RoundNumber(ref number, b + 1);
                                return TryFormatDecimalE(ref number, destination, out bytesWritten, b, (byte)format.Symbol);
                            }
                        default:
                            return System.ThrowHelper.TryFormatThrowFormatException(out bytesWritten);
                    }
                }

                private static bool TryFormatDecimalE(ref NumberBuffer number, Span<byte> destination, out int bytesWritten, byte precision, byte exponentSymbol)
                {
                    int scale = number.Scale;
                    ReadOnlySpan<byte> readOnlySpan = number.Digits;
                    int num = (number.IsNegative ? 1 : 0) + 1 + ((precision != 0) ? (precision + 1) : 0) + 2 + 3;
                    if (destination.Length < num)
                    {
                        bytesWritten = 0;
                        return false;
                    }
                    int num2 = 0;
                    int num3 = 0;
                    if (number.IsNegative)
                    {
                        destination[num2++] = 45;
                    }
                    byte b = readOnlySpan[num3];
                    int num4;
                    if (b == 0)
                    {
                        destination[num2++] = 48;
                        num4 = 0;
                    }
                    else
                    {
                        destination[num2++] = b;
                        num3++;
                        num4 = scale - 1;
                    }
                    if (precision > 0)
                    {
                        destination[num2++] = 46;
                        for (int i = 0; i < precision; i++)
                        {
                            byte b2 = readOnlySpan[num3];
                            if (b2 == 0)
                            {
                                while (i++ < precision)
                                {
                                    destination[num2++] = 48;
                                }
                                break;
                            }
                            destination[num2++] = b2;
                            num3++;
                        }
                    }
                    destination[num2++] = exponentSymbol;
                    if (num4 >= 0)
                    {
                        destination[num2++] = 43;
                    }
                    else
                    {
                        destination[num2++] = 45;
                        num4 = -num4;
                    }
                    destination[num2++] = 48;
                    destination[num2++] = (byte)(num4 / 10 + 48);
                    destination[num2++] = (byte)(num4 % 10 + 48);
                    bytesWritten = num;
                    return true;
                }

                private static bool TryFormatDecimalF(ref NumberBuffer number, Span<byte> destination, out int bytesWritten, byte precision)
                {
                    int scale = number.Scale;
                    ReadOnlySpan<byte> readOnlySpan = number.Digits;
                    int num = (number.IsNegative ? 1 : 0) + ((scale <= 0) ? 1 : scale) + ((precision != 0) ? (precision + 1) : 0);
                    if (destination.Length < num)
                    {
                        bytesWritten = 0;
                        return false;
                    }
                    int i = 0;
                    int num2 = 0;
                    if (number.IsNegative)
                    {
                        destination[num2++] = 45;
                    }
                    if (scale <= 0)
                    {
                        destination[num2++] = 48;
                    }
                    else
                    {
                        for (; i < scale; i++)
                        {
                            byte b = readOnlySpan[i];
                            if (b == 0)
                            {
                                int num3 = scale - i;
                                for (int j = 0; j < num3; j++)
                                {
                                    destination[num2++] = 48;
                                }
                                break;
                            }
                            destination[num2++] = b;
                        }
                    }
                    if (precision > 0)
                    {
                        destination[num2++] = 46;
                        int k = 0;
                        if (scale < 0)
                        {
                            int num4 = Math.Min(precision, -scale);
                            for (int l = 0; l < num4; l++)
                            {
                                destination[num2++] = 48;
                            }
                            k += num4;
                        }
                        for (; k < precision; k++)
                        {
                            byte b2 = readOnlySpan[i];
                            if (b2 == 0)
                            {
                                while (k++ < precision)
                                {
                                    destination[num2++] = 48;
                                }
                                break;
                            }
                            destination[num2++] = b2;
                            i++;
                        }
                    }
                    bytesWritten = num;
                    return true;
                }

                private static bool TryFormatDecimalG(ref NumberBuffer number, Span<byte> destination, out int bytesWritten)
                {
                    int scale = number.Scale;
                    ReadOnlySpan<byte> readOnlySpan = number.Digits;
                    int numDigits = number.NumDigits;
                    bool flag = scale < numDigits;
                    int num;
                    if (flag)
                    {
                        num = numDigits + 1;
                        if (scale <= 0)
                        {
                            num += 1 + -scale;
                        }
                    }
                    else
                    {
                        num = ((scale <= 0) ? 1 : scale);
                    }
                    if (number.IsNegative)
                    {
                        num++;
                    }
                    if (destination.Length < num)
                    {
                        bytesWritten = 0;
                        return false;
                    }
                    int i = 0;
                    int num2 = 0;
                    if (number.IsNegative)
                    {
                        destination[num2++] = 45;
                    }
                    if (scale <= 0)
                    {
                        destination[num2++] = 48;
                    }
                    else
                    {
                        for (; i < scale; i++)
                        {
                            byte b = readOnlySpan[i];
                            if (b == 0)
                            {
                                int num3 = scale - i;
                                for (int j = 0; j < num3; j++)
                                {
                                    destination[num2++] = 48;
                                }
                                break;
                            }
                            destination[num2++] = b;
                        }
                    }
                    if (flag)
                    {
                        destination[num2++] = 46;
                        if (scale < 0)
                        {
                            int num4 = -scale;
                            for (int k = 0; k < num4; k++)
                            {
                                destination[num2++] = 48;
                            }
                        }
                        byte b2;
                        while ((b2 = readOnlySpan[i++]) != 0)
                        {
                            destination[num2++] = b2;
                        }
                    }
                    bytesWritten = num;
                    return true;
                }

                /// <summary>
                /// Formats a <see cref="System.Boolean"/> as a UTF-8 <see cref="System.String"/> .
                /// </summary>
                /// <param name="value">The value to format.</param>
                /// <param name="destination">The buffer to write the UTF8-formatted value to.</param>
                /// <param name="bytesWritten">When the method returns, contains the length of the formatted text in bytes.</param>
                /// <param name="format">The standard format to use.</param>
                /// <returns><c>true</c> if the formatting operation succeeds; 
                /// <c>false</c> if <paramref name="destination"/> is too small.</returns>
                /// <remarks>
                /// Format Supported:
                /// <list type="table">
                ///     <listheader>
                ///         Format <see cref="System.String"/> Example result <see cref="System.String"/> Comments
                ///     </listheader>
                ///     <item>G/g(default)    </item>
                ///     <item>F/f	12.45	Fixed point</item>
                ///     <item>E/e	1.245000e1	Exponential</item>
                /// </list>
                /// If the method fails, iteratively increase the size of the buffer and retry until it succeeds.
                /// </remarks>
                public static bool TryFormat(double value, Span<byte> destination, out int bytesWritten, StandardFormat format = default(StandardFormat))
                {
                    return TryFormatFloatingPoint(value, destination, out bytesWritten, format);
                }

                /// <summary>
                /// Formats a <see cref="System.Boolean"/> as a UTF-8 <see cref="System.String"/> .
                /// </summary>
                /// <param name="value">The value to format.</param>
                /// <param name="destination">The buffer to write the UTF8-formatted value to.</param>
                /// <param name="bytesWritten">When the method returns, contains the length of the formatted text in bytes.</param>
                /// <param name="format">The standard format to use.</param>
                /// <returns><c>true</c> if the formatting operation succeeds; 
                /// <c>false</c> if <paramref name="destination"/> is too small.</returns>
                /// <remarks>
                /// Format Supported:
                /// <list type="table">
                ///     <listheader>
                ///         Format <see cref="System.String"/> Example result <see cref="System.String"/> Comments
                ///     </listheader>
                ///     <item>G/g(default)    </item>
                ///     <item>F/f	12.45	Fixed point</item>
                ///     <item>E/e	1.245000e1	Exponential</item>
                /// </list>
                /// If the method fails, iteratively increase the size of the buffer and retry until it succeeds.
                /// </remarks>
                public static bool TryFormat(float value, Span<byte> destination, out int bytesWritten, StandardFormat format = default(StandardFormat))
                {
                    return TryFormatFloatingPoint(value, destination, out bytesWritten, format);
                }

                private static bool TryFormatFloatingPoint<T>(T value, Span<byte> destination, out int bytesWritten, StandardFormat format) where T : IFormattable
                {
                    if (format.IsDefault)
                    {
                        format = 'G';
                    }
                    switch (format.Symbol)
                    {
                        case 'G':
                        case 'g':
                            if (format.Precision != byte.MaxValue)
                            {
                                throw new NotSupportedException(MDCFR.Properties.Resources.Argument_GWithPrecisionNotSupported);
                            }
                            break;
                        default:
                            return System.ThrowHelper.TryFormatThrowFormatException(out bytesWritten);
                        case 'E':
                        case 'F':
                        case 'e':
                        case 'f':
                            break;
                    }
                    string text = format.ToString();
                    string text2 = value.ToString(text, System.Globalization.CultureInfo.InvariantCulture);
                    int length = text2.Length;
                    if (length > destination.Length)
                    {
                        bytesWritten = 0;
                        return false;
                    }
                    for (int i = 0; i < length; i++)
                    {
                        destination[i] = (byte)text2[i];
                    }
                    bytesWritten = length;
                    return true;
                }

                /// <summary>
                /// Formats a <see cref="System.Boolean"/> as a UTF-8 <see cref="System.String"/> .
                /// </summary>
                /// <param name="value">The value to format.</param>
                /// <param name="destination">The buffer to write the UTF8-formatted value to.</param>
                /// <param name="bytesWritten">When the method returns, contains the length of the formatted text in bytes.</param>
                /// <param name="format">The standard format to use.</param>
                /// <returns><c>true</c> if the formatting operation succeeds; 
                /// <c>false</c> if <paramref name="destination"/> is too small.</returns>
                /// <remarks>
                /// Format Supported:
                /// <list type="table">
                ///     <listheader>
                ///         Format <see cref="System.String"/> Example result <see cref="System.String"/>
                ///     </listheader>
                ///     <item>D (default)	nnnnnnnn-nnnn-nnnn-nnnn-nnnnnnnnnnnn</item>
                ///     <item>B	{nnnnnnnn-nnnn-nnnn-nnnn-nnnnnnnnnnnn}</item>
                ///     <item>P	(nnnnnnnn-nnnn-nnnn-nnnn-nnnnnnnnnnnn)</item>
                ///     <item>N	nnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnn</item>
                /// </list>
                /// If the method fails, iteratively increase the size of the buffer and retry until it succeeds.
                /// </remarks>
                public static bool TryFormat(Guid value, Span<byte> destination, out int bytesWritten, StandardFormat format = default(StandardFormat))
                {
                    int num;
                    switch (FormattingHelpers.GetSymbolOrDefault(in format, 'D'))
                    {
                        case 'D':
                            num = -2147483612;
                            break;
                        case 'B':
                            num = -2139260122;
                            break;
                        case 'P':
                            num = -2144786394;
                            break;
                        case 'N':
                            num = 32;
                            break;
                        default:
                            return System.ThrowHelper.TryFormatThrowFormatException(out bytesWritten);
                    }
                    if ((byte)num > destination.Length)
                    {
                        bytesWritten = 0;
                        return false;
                    }
                    bytesWritten = (byte)num;
                    num >>= 8;
                    if ((byte)num != 0)
                    {
                        destination[0] = (byte)num;
                        destination = destination.Slice(1);
                    }
                    num >>= 8;
                    DecomposedGuid decomposedGuid = default(DecomposedGuid);
                    decomposedGuid.Guid = value;
                    byte b = destination[8];
                    FormattingHelpers.WriteHexByte(decomposedGuid.Byte03, destination, 0, HexCasing.Lowercase);
                    FormattingHelpers.WriteHexByte(decomposedGuid.Byte02, destination, 2, HexCasing.Lowercase);
                    FormattingHelpers.WriteHexByte(decomposedGuid.Byte01, destination, 4, HexCasing.Lowercase);
                    FormattingHelpers.WriteHexByte(decomposedGuid.Byte00, destination, 6, HexCasing.Lowercase);
                    if (num < 0)
                    {
                        destination[8] = 45;
                        destination = destination.Slice(9);
                    }
                    else
                    {
                        destination = destination.Slice(8);
                    }
                    byte b2 = destination[4];
                    FormattingHelpers.WriteHexByte(decomposedGuid.Byte05, destination, 0, HexCasing.Lowercase);
                    FormattingHelpers.WriteHexByte(decomposedGuid.Byte04, destination, 2, HexCasing.Lowercase);
                    if (num < 0)
                    {
                        destination[4] = 45;
                        destination = destination.Slice(5);
                    }
                    else
                    {
                        destination = destination.Slice(4);
                    }
                    byte b3 = destination[4];
                    FormattingHelpers.WriteHexByte(decomposedGuid.Byte07, destination, 0, HexCasing.Lowercase);
                    FormattingHelpers.WriteHexByte(decomposedGuid.Byte06, destination, 2, HexCasing.Lowercase);
                    if (num < 0)
                    {
                        destination[4] = 45;
                        destination = destination.Slice(5);
                    }
                    else
                    {
                        destination = destination.Slice(4);
                    }
                    byte b4 = destination[4];
                    FormattingHelpers.WriteHexByte(decomposedGuid.Byte08, destination, 0, HexCasing.Lowercase);
                    FormattingHelpers.WriteHexByte(decomposedGuid.Byte09, destination, 2, HexCasing.Lowercase);
                    if (num < 0)
                    {
                        destination[4] = 45;
                        destination = destination.Slice(5);
                    }
                    else
                    {
                        destination = destination.Slice(4);
                    }
                    byte b5 = destination[11];
                    FormattingHelpers.WriteHexByte(decomposedGuid.Byte10, destination, 0, HexCasing.Lowercase);
                    FormattingHelpers.WriteHexByte(decomposedGuid.Byte11, destination, 2, HexCasing.Lowercase);
                    FormattingHelpers.WriteHexByte(decomposedGuid.Byte12, destination, 4, HexCasing.Lowercase);
                    FormattingHelpers.WriteHexByte(decomposedGuid.Byte13, destination, 6, HexCasing.Lowercase);
                    FormattingHelpers.WriteHexByte(decomposedGuid.Byte14, destination, 8, HexCasing.Lowercase);
                    FormattingHelpers.WriteHexByte(decomposedGuid.Byte15, destination, 10, HexCasing.Lowercase);
                    if ((byte)num != 0)
                    {
                        destination[12] = (byte)num;
                    }
                    return true;
                }

                /// <summary>
                /// Formats a <see cref="System.Boolean"/> as a UTF-8 <see cref="System.String"/> .
                /// </summary>
                /// <param name="value">The value to format.</param>
                /// <param name="destination">The buffer to write the UTF8-formatted value to.</param>
                /// <param name="bytesWritten">When the method returns, contains the length of the formatted text in bytes.</param>
                /// <param name="format">The standard format to use.</param>
                /// <returns><c>true</c> if the formatting operation succeeds; 
                /// <c>false</c> if <paramref name="destination"/> is too small.</returns>
                /// <remarks>
                /// Format Supported:
                /// <list type="table">
                ///     <listheader>
                ///         Format <see cref="System.String"/> Example result <see cref="System.String"/>
                ///     </listheader>
                ///     <item>G/g(default)    </item>
                ///     <item>D/d                   122</item>
                ///     <item>N/n                   122</item>
                ///     <item>X/x                   7a  </item>
                /// </list>
                /// If the method fails, iteratively increase the size of the buffer and retry until it succeeds.
                /// </remarks>
                public static bool TryFormat(byte value, Span<byte> destination, out int bytesWritten, StandardFormat format = default(StandardFormat))
                {
                    return TryFormatUInt64(value, destination, out bytesWritten, format);
                }

                /// <summary>
                /// Formats a <see cref="System.Boolean"/> as a UTF-8 <see cref="System.String"/> .
                /// </summary>
                /// <param name="value">The value to format.</param>
                /// <param name="destination">The buffer to write the UTF8-formatted value to.</param>
                /// <param name="bytesWritten">When the method returns, contains the length of the formatted text in bytes.</param>
                /// <param name="format">The standard format to use.</param>
                /// <returns><c>true</c> if the formatting operation succeeds; 
                /// <c>false</c> if <paramref name="destination"/> is too small.</returns>
                /// <remarks>
                /// Format Supported:
                /// <list type="table">
                ///     <listheader>
                ///         Format <see cref="System.String"/> Example result <see cref="System.String"/>
                ///     </listheader>
                ///     <item>G/g(default)    </item>
                ///     <item>D/d                   122</item>
                ///     <item>N/n                   122</item>
                ///     <item>X/x                   7a  </item>
                /// </list>
                /// If the method fails, iteratively increase the size of the buffer and retry until it succeeds.
                /// </remarks>
                [CLSCompliant(false)]
                public static bool TryFormat(sbyte value, Span<byte> destination, out int bytesWritten, StandardFormat format = default(StandardFormat))
                {
                    return TryFormatInt64(value, 255uL, destination, out bytesWritten, format);
                }

                /// <summary>
                /// Formats a <see cref="System.Boolean"/> as a UTF-8 <see cref="System.String"/> .
                /// </summary>
                /// <param name="value">The value to format.</param>
                /// <param name="destination">The buffer to write the UTF8-formatted value to.</param>
                /// <param name="bytesWritten">When the method returns, contains the length of the formatted text in bytes.</param>
                /// <param name="format">The standard format to use.</param>
                /// <returns><c>true</c> if the formatting operation succeeds; 
                /// <c>false</c> if <paramref name="destination"/> is too small.</returns>
                /// <remarks>
                /// Format Supported:
                /// <list type="table">
                ///     <listheader>
                ///         Format <see cref="System.String"/>|||Example result <see cref="System.String"/>
                ///     </listheader>
                ///     <item>G/g(default)    </item>
                ///     <item>D/d                   32767</item>
                ///     <item>N/n                   32,767</item>
                ///     <item>X/x                   7fff  </item>
                /// </list>
                /// If the method fails, iteratively increase the size of the buffer and retry until it succeeds.
                /// </remarks>
                [CLSCompliant(false)]
                public static bool TryFormat(ushort value, Span<byte> destination, out int bytesWritten, StandardFormat format = default(StandardFormat))
                {
                    return TryFormatUInt64(value, destination, out bytesWritten, format);
                }

                /// <summary>
                /// Formats a <see cref="System.Boolean"/> as a UTF-8 <see cref="System.String"/> .
                /// </summary>
                /// <param name="value">The value to format.</param>
                /// <param name="destination">The buffer to write the UTF8-formatted value to.</param>
                /// <param name="bytesWritten">When the method returns, contains the length of the formatted text in bytes.</param>
                /// <param name="format">The standard format to use.</param>
                /// <returns><c>true</c> if the formatting operation succeeds; 
                /// <c>false</c> if <paramref name="destination"/> is too small.</returns>
                /// <remarks>
                /// Format Supported:
                /// <list type="table">
                ///     <listheader>
                ///         Format <see cref="System.String"/> Example result <see cref="System.String"/>
                ///     </listheader>
                ///     <item>G/g(default)    </item>
                ///     <item>D/d                   32767</item>
                ///     <item>N/n                   32,767</item>
                ///     <item>X/x                   7fff  </item>
                /// </list>
                /// If the method fails, iteratively increase the size of the buffer and retry until it succeeds.
                /// </remarks>
                public static bool TryFormat(short value, Span<byte> destination, out int bytesWritten, StandardFormat format = default(StandardFormat))
                {
                    return TryFormatInt64(value, 65535uL, destination, out bytesWritten, format);
                }

                /// <summary>
                /// Formats a <see cref="System.Boolean"/> as a UTF-8 <see cref="System.String"/> .
                /// </summary>
                /// <param name="value">The value to format.</param>
                /// <param name="destination">The buffer to write the UTF8-formatted value to.</param>
                /// <param name="bytesWritten">When the method returns, contains the length of the formatted text in bytes.</param>
                /// <param name="format">The standard format to use.</param>
                /// <returns><c>true</c> if the formatting operation succeeds; 
                /// <c>false</c> if <paramref name="destination"/> is too small.</returns>
                /// <remarks>
                /// Format Supported:
                /// <list type="table">
                ///     <listheader>
                ///         Format <see cref="System.String"/>|||Example result <see cref="System.String"/>
                ///     </listheader>
                ///     <item>G/g(default)    </item>
                ///     <item>D/d                   32767</item>
                ///     <item>N/n                   32,767</item>
                ///     <item>X/x                   7fff  </item>
                /// </list>
                /// If the method fails, iteratively increase the size of the buffer and retry until it succeeds.
                /// </remarks>
                [CLSCompliant(false)]
                public static bool TryFormat(uint value, Span<byte> destination, out int bytesWritten, StandardFormat format = default(StandardFormat))
                {
                    return TryFormatUInt64(value, destination, out bytesWritten, format);
                }

                /// <summary>
                /// Formats a <see cref="System.Boolean"/> as a UTF-8 <see cref="System.String"/> .
                /// </summary>
                /// <param name="value">The value to format.</param>
                /// <param name="destination">The buffer to write the UTF8-formatted value to.</param>
                /// <param name="bytesWritten">When the method returns, contains the length of the formatted text in bytes.</param>
                /// <param name="format">The standard format to use.</param>
                /// <returns><c>true</c> if the formatting operation succeeds; 
                /// <c>false</c> if <paramref name="destination"/> is too small.</returns>
                /// <remarks>
                /// Format Supported:
                /// <list type="table">
                ///     <listheader>
                ///         Format <see cref="System.String"/>|||Example result <see cref="System.String"/>
                ///     </listheader>
                ///     <item>G/g(default)    </item>
                ///     <item>D/d                   32767</item>
                ///     <item>N/n                   32,767</item>
                ///     <item>X/x                   7fff  </item>
                /// </list>
                /// If the method fails, iteratively increase the size of the buffer and retry until it succeeds.
                /// </remarks>
                public static bool TryFormat(int value, Span<byte> destination, out int bytesWritten, StandardFormat format = default(StandardFormat))
                {
                    return TryFormatInt64(value, 4294967295uL, destination, out bytesWritten, format);
                }


                /// <summary>
                /// Formats a <see cref="System.Boolean"/> as a UTF-8 <see cref="System.String"/> .
                /// </summary>
                /// <param name="value">The value to format.</param>
                /// <param name="destination">The buffer to write the UTF8-formatted value to.</param>
                /// <param name="bytesWritten">When the method returns, contains the length of the formatted text in bytes.</param>
                /// <param name="format">The standard format to use.</param>
                /// <returns><c>true</c> if the formatting operation succeeds; 
                /// <c>false</c> if <paramref name="destination"/> is too small.</returns>
                /// <remarks>
                /// Format Supported:
                /// <list type="table">
                ///     <listheader>
                ///         Format <see cref="System.String"/>|||Example result <see cref="System.String"/>
                ///     </listheader>
                ///     <item>G/g(default)    </item>
                ///     <item>D/d                   32767</item>
                ///     <item>N/n                   32,767</item>
                ///     <item>X/x                   7fff  </item>
                /// </list>
                /// If the method fails, iteratively increase the size of the buffer and retry until it succeeds.
                /// </remarks>
                [CLSCompliant(false)]
                public static bool TryFormat(ulong value, Span<byte> destination, out int bytesWritten, StandardFormat format = default(StandardFormat))
                {
                    return TryFormatUInt64(value, destination, out bytesWritten, format);
                }

                /// <summary>
                /// Formats a <see cref="System.Boolean"/> as a UTF-8 <see cref="System.String"/> .
                /// </summary>
                /// <param name="value">The value to format.</param>
                /// <param name="destination">The buffer to write the UTF8-formatted value to.</param>
                /// <param name="bytesWritten">When the method returns, contains the length of the formatted text in bytes.</param>
                /// <param name="format">The standard format to use.</param>
                /// <returns><c>true</c> if the formatting operation succeeds; 
                /// <c>false</c> if <paramref name="destination"/> is too small.</returns>
                /// <remarks>
                /// Format Supported:
                /// <list type="table">
                ///     <listheader>
                ///         Format <see cref="System.String"/> Example result <see cref="System.String"/>
                ///     </listheader>
                ///     <item>G/g(default)    </item>
                ///     <item>D/d                   32767</item>
                ///     <item>N/n                   32,767</item>
                ///     <item>X/x                   7fff  </item>
                /// </list>
                /// If the method fails, iteratively increase the size of the buffer and retry until it succeeds.
                /// </remarks>
                public static bool TryFormat(long value, Span<byte> destination, out int bytesWritten, StandardFormat format = default(StandardFormat))
                {
                    return TryFormatInt64(value, ulong.MaxValue, destination, out bytesWritten, format);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                private static bool TryFormatInt64(long value, ulong mask, Span<byte> destination, out int bytesWritten, StandardFormat format)
                {
                    if (format.IsDefault)
                    {
                        return TryFormatInt64Default(value, destination, out bytesWritten);
                    }
                    switch (format.Symbol)
                    {
                        case 'G':
                        case 'g':
                            if (format.HasPrecision)
                            {
                                throw new NotSupportedException(MDCFR.Properties.Resources.Argument_GWithPrecisionNotSupported);
                            }
                            return TryFormatInt64D(value, format.Precision, destination, out bytesWritten);
                        case 'D':
                        case 'd':
                            return TryFormatInt64D(value, format.Precision, destination, out bytesWritten);
                        case 'N':
                        case 'n':
                            return TryFormatInt64N(value, format.Precision, destination, out bytesWritten);
                        case 'x':
                            return TryFormatUInt64X((ulong)value & mask, format.Precision, useLower: true, destination, out bytesWritten);
                        case 'X':
                            return TryFormatUInt64X((ulong)value & mask, format.Precision, useLower: false, destination, out bytesWritten);
                        default:
                            return System.ThrowHelper.TryFormatThrowFormatException(out bytesWritten);
                    }
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                private static bool TryFormatInt64D(long value, byte precision, Span<byte> destination, out int bytesWritten)
                {
                    bool insertNegationSign = false;
                    if (value < 0)
                    {
                        insertNegationSign = true;
                        value = -value;
                    }
                    return TryFormatUInt64D((ulong)value, precision, destination, insertNegationSign, out bytesWritten);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                private static bool TryFormatInt64Default(long value, Span<byte> destination, out int bytesWritten)
                {
                    if ((ulong)value < 10uL)
                    {
                        return TryFormatUInt32SingleDigit((uint)value, destination, out bytesWritten);
                    }
                    if (IntPtr.Size == 8)
                    {
                        return TryFormatInt64MultipleDigits(value, destination, out bytesWritten);
                    }
                    if (value <= int.MaxValue && value >= int.MinValue)
                    {
                        return TryFormatInt32MultipleDigits((int)value, destination, out bytesWritten);
                    }
                    if (value <= 4294967295000000000L && value >= -4294967295000000000L)
                    {
                        if (value >= 0)
                        {
                            return TryFormatUInt64LessThanBillionMaxUInt((ulong)value, destination, out bytesWritten);
                        }
                        return TryFormatInt64MoreThanNegativeBillionMaxUInt(-value, destination, out bytesWritten);
                    }
                    if (value >= 0)
                    {
                        return TryFormatUInt64MoreThanBillionMaxUInt((ulong)value, destination, out bytesWritten);
                    }
                    return TryFormatInt64LessThanNegativeBillionMaxUInt(-value, destination, out bytesWritten);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                private static bool TryFormatInt32Default(int value, Span<byte> destination, out int bytesWritten)
                {
                    if ((uint)value < 10u)
                    {
                        return TryFormatUInt32SingleDigit((uint)value, destination, out bytesWritten);
                    }
                    return TryFormatInt32MultipleDigits(value, destination, out bytesWritten);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                private static bool TryFormatInt32MultipleDigits(int value, Span<byte> destination, out int bytesWritten)
                {
                    if (value < 0)
                    {
                        value = -value;
                        int num = FormattingHelpers.CountDigits((uint)value);
                        if (num >= destination.Length)
                        {
                            bytesWritten = 0;
                            return false;
                        }
                        destination[0] = 45;
                        bytesWritten = num + 1;
                        FormattingHelpers.WriteDigits((uint)value, destination.Slice(1, num));
                        return true;
                    }
                    return TryFormatUInt32MultipleDigits((uint)value, destination, out bytesWritten);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                private static bool TryFormatInt64MultipleDigits(long value, Span<byte> destination, out int bytesWritten)
                {
                    if (value < 0)
                    {
                        value = -value;
                        int num = FormattingHelpers.CountDigits((ulong)value);
                        if (num >= destination.Length)
                        {
                            bytesWritten = 0;
                            return false;
                        }
                        destination[0] = 45;
                        bytesWritten = num + 1;
                        FormattingHelpers.WriteDigits((ulong)value, destination.Slice(1, num));
                        return true;
                    }
                    return TryFormatUInt64MultipleDigits((ulong)value, destination, out bytesWritten);
                }

                private static bool TryFormatInt64MoreThanNegativeBillionMaxUInt(long value, Span<byte> destination, out int bytesWritten)
                {
                    uint num = (uint)(value / 1000000000);
                    uint value2 = (uint)(value - num * 1000000000);
                    int num2 = FormattingHelpers.CountDigits(num);
                    int num3 = num2 + 9;
                    if (num3 >= destination.Length)
                    {
                        bytesWritten = 0;
                        return false;
                    }
                    destination[0] = 45;
                    bytesWritten = num3 + 1;
                    FormattingHelpers.WriteDigits(num, destination.Slice(1, num2));
                    FormattingHelpers.WriteDigits(value2, destination.Slice(num2 + 1, 9));
                    return true;
                }

                private static bool TryFormatInt64LessThanNegativeBillionMaxUInt(long value, Span<byte> destination, out int bytesWritten)
                {
                    ulong num = (ulong)value / 1000000000uL;
                    uint value2 = (uint)((ulong)value - num * 1000000000);
                    uint num2 = (uint)(num / 1000000000uL);
                    uint value3 = (uint)(num - num2 * 1000000000);
                    int num3 = FormattingHelpers.CountDigits(num2);
                    int num4 = num3 + 18;
                    if (num4 >= destination.Length)
                    {
                        bytesWritten = 0;
                        return false;
                    }
                    destination[0] = 45;
                    bytesWritten = num4 + 1;
                    FormattingHelpers.WriteDigits(num2, destination.Slice(1, num3));
                    FormattingHelpers.WriteDigits(value3, destination.Slice(num3 + 1, 9));
                    FormattingHelpers.WriteDigits(value2, destination.Slice(num3 + 1 + 9, 9));
                    return true;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                private static bool TryFormatInt64N(long value, byte precision, Span<byte> destination, out int bytesWritten)
                {
                    bool insertNegationSign = false;
                    if (value < 0)
                    {
                        insertNegationSign = true;
                        value = -value;
                    }
                    return TryFormatUInt64N((ulong)value, precision, destination, insertNegationSign, out bytesWritten);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                private static bool TryFormatUInt64(ulong value, Span<byte> destination, out int bytesWritten, StandardFormat format)
                {
                    if (format.IsDefault)
                    {
                        return TryFormatUInt64Default(value, destination, out bytesWritten);
                    }
                    switch (format.Symbol)
                    {
                        case 'G':
                        case 'g':
                            if (format.HasPrecision)
                            {
                                throw new NotSupportedException(MDCFR.Properties.Resources.Argument_GWithPrecisionNotSupported);
                            }
                            return TryFormatUInt64D(value, format.Precision, destination, insertNegationSign: false, out bytesWritten);
                        case 'D':
                        case 'd':
                            return TryFormatUInt64D(value, format.Precision, destination, insertNegationSign: false, out bytesWritten);
                        case 'N':
                        case 'n':
                            return TryFormatUInt64N(value, format.Precision, destination, insertNegationSign: false, out bytesWritten);
                        case 'x':
                            return TryFormatUInt64X(value, format.Precision, useLower: true, destination, out bytesWritten);
                        case 'X':
                            return TryFormatUInt64X(value, format.Precision, useLower: false, destination, out bytesWritten);
                        default:
                            return System.ThrowHelper.TryFormatThrowFormatException(out bytesWritten);
                    }
                }

                private static bool TryFormatUInt64D(ulong value, byte precision, Span<byte> destination, bool insertNegationSign, out int bytesWritten)
                {
                    int num = FormattingHelpers.CountDigits(value);
                    int num2 = ((precision != byte.MaxValue) ? precision : 0) - num;
                    if (num2 < 0)
                    {
                        num2 = 0;
                    }
                    int num3 = num + num2;
                    if (insertNegationSign)
                    {
                        num3++;
                    }
                    if (num3 > destination.Length)
                    {
                        bytesWritten = 0;
                        return false;
                    }
                    bytesWritten = num3;
                    if (insertNegationSign)
                    {
                        destination[0] = 45;
                        destination = destination.Slice(1);
                    }
                    if (num2 > 0)
                    {
                        FormattingHelpers.FillWithAsciiZeros(destination.Slice(0, num2));
                    }
                    FormattingHelpers.WriteDigits(value, destination.Slice(num2, num));
                    return true;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                private static bool TryFormatUInt64Default(ulong value, Span<byte> destination, out int bytesWritten)
                {
                    if (value < 10)
                    {
                        return TryFormatUInt32SingleDigit((uint)value, destination, out bytesWritten);
                    }
                    if (IntPtr.Size == 8)
                    {
                        return TryFormatUInt64MultipleDigits(value, destination, out bytesWritten);
                    }
                    if (value <= uint.MaxValue)
                    {
                        return TryFormatUInt32MultipleDigits((uint)value, destination, out bytesWritten);
                    }
                    if (value <= 4294967295000000000L)
                    {
                        return TryFormatUInt64LessThanBillionMaxUInt(value, destination, out bytesWritten);
                    }
                    return TryFormatUInt64MoreThanBillionMaxUInt(value, destination, out bytesWritten);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                private static bool TryFormatUInt32Default(uint value, Span<byte> destination, out int bytesWritten)
                {
                    if (value < 10)
                    {
                        return TryFormatUInt32SingleDigit(value, destination, out bytesWritten);
                    }
                    return TryFormatUInt32MultipleDigits(value, destination, out bytesWritten);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                private static bool TryFormatUInt32SingleDigit(uint value, Span<byte> destination, out int bytesWritten)
                {
                    if (destination.Length == 0)
                    {
                        bytesWritten = 0;
                        return false;
                    }
                    destination[0] = (byte)(48 + value);
                    bytesWritten = 1;
                    return true;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                private static bool TryFormatUInt32MultipleDigits(uint value, Span<byte> destination, out int bytesWritten)
                {
                    int num = FormattingHelpers.CountDigits(value);
                    if (num > destination.Length)
                    {
                        bytesWritten = 0;
                        return false;
                    }
                    bytesWritten = num;
                    FormattingHelpers.WriteDigits(value, destination.Slice(0, num));
                    return true;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                private static bool TryFormatUInt64SingleDigit(ulong value, Span<byte> destination, out int bytesWritten)
                {
                    if (destination.Length == 0)
                    {
                        bytesWritten = 0;
                        return false;
                    }
                    destination[0] = (byte)(48 + value);
                    bytesWritten = 1;
                    return true;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                private static bool TryFormatUInt64MultipleDigits(ulong value, Span<byte> destination, out int bytesWritten)
                {
                    int num = FormattingHelpers.CountDigits(value);
                    if (num > destination.Length)
                    {
                        bytesWritten = 0;
                        return false;
                    }
                    bytesWritten = num;
                    FormattingHelpers.WriteDigits(value, destination.Slice(0, num));
                    return true;
                }

                private static bool TryFormatUInt64LessThanBillionMaxUInt(ulong value, Span<byte> destination, out int bytesWritten)
                {
                    uint num = (uint)(value / 1000000000uL);
                    uint value2 = (uint)(value - num * 1000000000);
                    int num2 = FormattingHelpers.CountDigits(num);
                    int num3 = num2 + 9;
                    if (num3 > destination.Length)
                    {
                        bytesWritten = 0;
                        return false;
                    }
                    bytesWritten = num3;
                    FormattingHelpers.WriteDigits(num, destination.Slice(0, num2));
                    FormattingHelpers.WriteDigits(value2, destination.Slice(num2, 9));
                    return true;
                }

                private static bool TryFormatUInt64MoreThanBillionMaxUInt(ulong value, Span<byte> destination, out int bytesWritten)
                {
                    ulong num = value / 1000000000uL;
                    uint value2 = (uint)(value - num * 1000000000);
                    uint num2 = (uint)(num / 1000000000uL);
                    uint value3 = (uint)(num - num2 * 1000000000);
                    int num3 = FormattingHelpers.CountDigits(num2);
                    int num4 = num3 + 18;
                    if (num4 > destination.Length)
                    {
                        bytesWritten = 0;
                        return false;
                    }
                    bytesWritten = num4;
                    FormattingHelpers.WriteDigits(num2, destination.Slice(0, num3));
                    FormattingHelpers.WriteDigits(value3, destination.Slice(num3, 9));
                    FormattingHelpers.WriteDigits(value2, destination.Slice(num3 + 9, 9));
                    return true;
                }

                private static bool TryFormatUInt64N(ulong value, byte precision, Span<byte> destination, bool insertNegationSign, out int bytesWritten)
                {
                    int num = FormattingHelpers.CountDigits(value);
                    int num2 = (num - 1) / 3;
                    int num3 = ((precision == byte.MaxValue) ? 2 : precision);
                    int num4 = num + num2;
                    if (num3 > 0)
                    {
                        num4 += num3 + 1;
                    }
                    if (insertNegationSign)
                    {
                        num4++;
                    }
                    if (num4 > destination.Length)
                    {
                        bytesWritten = 0;
                        return false;
                    }
                    bytesWritten = num4;
                    if (insertNegationSign)
                    {
                        destination[0] = 45;
                        destination = destination.Slice(1);
                    }
                    FormattingHelpers.WriteDigitsWithGroupSeparator(value, destination.Slice(0, num + num2));
                    if (num3 > 0)
                    {
                        destination[num + num2] = 46;
                        FormattingHelpers.FillWithAsciiZeros(destination.Slice(num + num2 + 1, num3));
                    }
                    return true;
                }

                private static bool TryFormatUInt64X(ulong value, byte precision, bool useLower, Span<byte> destination, out int bytesWritten)
                {
                    int num = FormattingHelpers.CountHexDigits(value);
                    int num2 = ((precision == byte.MaxValue) ? num : Math.Max(precision, num));
                    if (destination.Length < num2)
                    {
                        bytesWritten = 0;
                        return false;
                    }
                    bytesWritten = num2;
                    string text = (useLower ? "0123456789abcdef" : "0123456789ABCDEF");
                    while ((uint)(--num2) < (uint)destination.Length)
                    {
                        destination[num2] = (byte)text[(int)value & 0xF];
                        value >>= 4;
                    }
                    return true;
                }

                /// <summary>
                /// Formats a <see cref="System.Boolean"/> as a UTF-8 <see cref="System.String"/> .
                /// </summary>
                /// <param name="value">The value to format.</param>
                /// <param name="destination">The buffer to write the UTF8-formatted value to.</param>
                /// <param name="bytesWritten">When the method returns, contains the length of the formatted text in bytes.</param>
                /// <param name="format">The standard format to use.</param>
                /// <returns><c>true</c> if the formatting operation succeeds; 
                /// <c>false</c> if <paramref name="destination"/> is too small.</returns>
                /// <remarks>
                /// Format Supported:
                /// <list type="table">
                ///     <listheader>
                ///         Format <see cref="System.String"/>| Example result <see cref="System.String"/>| Comments
                ///     </listheader>
                ///     <item>c/t/T (default)	[-][d.]hh:mm:ss[.fffffff]	(constant format)	</item>
                ///     <item>G	[-]d:hh:mm:ss.fffffff		(general long)</item>
                ///     <item>g	[-][d:][h]h:mm:ss[.f[f[f[f[f[f[f]]]]]]	(general short)	</item>
                /// </list>
                /// If the method fails, iteratively increase the size of the buffer and retry until it succeeds.
                /// </remarks>
                public static bool TryFormat(TimeSpan value, Span<byte> destination, out int bytesWritten, StandardFormat format = default(StandardFormat))
                {
                    char c = FormattingHelpers.GetSymbolOrDefault(in format, 'c');
                    switch (c)
                    {
                        case 'T':
                        case 't':
                            c = 'c';
                            break;
                        default:
                            return System.ThrowHelper.TryFormatThrowFormatException(out bytesWritten);
                        case 'G':
                        case 'c':
                        case 'g':
                            break;
                    }
                    int num = 8;
                    long ticks = value.Ticks;
                    uint valueWithoutTrailingZeros;
                    ulong num2;
                    if (ticks < 0)
                    {
                        ticks = -ticks;
                        if (ticks < 0)
                        {
                            valueWithoutTrailingZeros = 4775808u;
                            num2 = 922337203685uL;
                            goto IL_0082;
                        }
                    }
                    num2 = FormattingHelpers.DivMod((ulong)Math.Abs(value.Ticks), 10000000uL, out var modulo);
                    valueWithoutTrailingZeros = (uint)modulo;
                    goto IL_0082;
                IL_0082:
                    int num3 = 0;
                    switch (c)
                    {
                        case 'c':
                            if (valueWithoutTrailingZeros != 0)
                            {
                                num3 = 7;
                            }
                            break;
                        case 'G':
                            num3 = 7;
                            break;
                        default:
                            if (valueWithoutTrailingZeros != 0)
                            {
                                num3 = 7 - FormattingHelpers.CountDecimalTrailingZeros(valueWithoutTrailingZeros, out valueWithoutTrailingZeros);
                            }
                            break;
                    }
                    if (num3 != 0)
                    {
                        num += num3 + 1;
                    }
                    ulong num4 = 0uL;
                    ulong modulo2 = 0uL;
                    if (num2 != 0)
                    {
                        num4 = FormattingHelpers.DivMod(num2, 60uL, out modulo2);
                    }
                    ulong num5 = 0uL;
                    ulong modulo3 = 0uL;
                    if (num4 != 0)
                    {
                        num5 = FormattingHelpers.DivMod(num4, 60uL, out modulo3);
                    }
                    uint num6 = 0u;
                    uint modulo4 = 0u;
                    if (num5 != 0)
                    {
                        num6 = FormattingHelpers.DivMod((uint)num5, 24u, out modulo4);
                    }
                    int num7 = 2;
                    if (modulo4 < 10 && c == 'g')
                    {
                        num7--;
                        num--;
                    }
                    int num8 = 0;
                    if (num6 == 0)
                    {
                        if (c == 'G')
                        {
                            num += 2;
                            num8 = 1;
                        }
                    }
                    else
                    {
                        num8 = FormattingHelpers.CountDigits(num6);
                        num += num8 + 1;
                    }
                    if (value.Ticks < 0)
                    {
                        num++;
                    }
                    if (destination.Length < num)
                    {
                        bytesWritten = 0;
                        return false;
                    }
                    bytesWritten = num;
                    int num9 = 0;
                    if (value.Ticks < 0)
                    {
                        destination[num9++] = 45;
                    }
                    if (num8 > 0)
                    {
                        FormattingHelpers.WriteDigits(num6, destination.Slice(num9, num8));
                        num9 += num8;
                        destination[num9++] = (byte)((c == 'c') ? 46 : 58);
                    }
                    FormattingHelpers.WriteDigits(modulo4, destination.Slice(num9, num7));
                    num9 += num7;
                    destination[num9++] = 58;
                    FormattingHelpers.WriteDigits((uint)modulo3, destination.Slice(num9, 2));
                    num9 += 2;
                    destination[num9++] = 58;
                    FormattingHelpers.WriteDigits((uint)modulo2, destination.Slice(num9, 2));
                    num9 += 2;
                    if (num3 > 0)
                    {
                        destination[num9++] = 46;
                        FormattingHelpers.WriteDigits(valueWithoutTrailingZeros, destination.Slice(num9, num3));
                        num9 += num3;
                    }
                    return true;
                }
            }

            /// <summary>
            /// Provides static methods to parse UTF-8 strings to common data types.
            /// </summary>
            public static class Utf8Parser
            {

                [Flags]
                private enum ParseNumberOptions
                {
                    AllowExponent = 1
                }

                private enum ComponentParseResult : byte
                {
                    NoMoreData,
                    Colon,
                    Period,
                    ParseFailure
                }

                private struct TimeSpanSplitter
                {
                    public uint V1;

                    public uint V2;

                    public uint V3;

                    public uint V4;

                    public uint V5;

                    public bool IsNegative;

                    public uint Separators;

                    public bool TrySplitTimeSpan(ReadOnlySpan<byte> source, bool periodUsedToSeparateDay, out int bytesConsumed)
                    {
                        int i = 0;
                        byte b = 0;
                        for (; i != source.Length; i++)
                        {
                            b = source[i];
                            if (b != 32 && b != 9)
                            {
                                break;
                            }
                        }
                        if (i == source.Length)
                        {
                            bytesConsumed = 0;
                            return false;
                        }
                        if (b == 45)
                        {
                            IsNegative = true;
                            i++;
                            if (i == source.Length)
                            {
                                bytesConsumed = 0;
                                return false;
                            }
                        }
                        if (!TryParseUInt32D(source.Slice(i), out V1, out var bytesConsumed2))
                        {
                            bytesConsumed = 0;
                            return false;
                        }
                        i += bytesConsumed2;
                        ComponentParseResult componentParseResult = ParseComponent(source, periodUsedToSeparateDay, ref i, out V2);
                        switch (componentParseResult)
                        {
                            case ComponentParseResult.ParseFailure:
                                bytesConsumed = 0;
                                return false;
                            case ComponentParseResult.NoMoreData:
                                bytesConsumed = i;
                                return true;
                            default:
                                Separators |= (uint)componentParseResult << 24;
                                componentParseResult = ParseComponent(source, neverParseAsFraction: false, ref i, out V3);
                                switch (componentParseResult)
                                {
                                    case ComponentParseResult.ParseFailure:
                                        bytesConsumed = 0;
                                        return false;
                                    case ComponentParseResult.NoMoreData:
                                        bytesConsumed = i;
                                        return true;
                                    default:
                                        Separators |= (uint)componentParseResult << 16;
                                        componentParseResult = ParseComponent(source, neverParseAsFraction: false, ref i, out V4);
                                        switch (componentParseResult)
                                        {
                                            case ComponentParseResult.ParseFailure:
                                                bytesConsumed = 0;
                                                return false;
                                            case ComponentParseResult.NoMoreData:
                                                bytesConsumed = i;
                                                return true;
                                            default:
                                                Separators |= (uint)componentParseResult << 8;
                                                componentParseResult = ParseComponent(source, neverParseAsFraction: false, ref i, out V5);
                                                switch (componentParseResult)
                                                {
                                                    case ComponentParseResult.ParseFailure:
                                                        bytesConsumed = 0;
                                                        return false;
                                                    case ComponentParseResult.NoMoreData:
                                                        bytesConsumed = i;
                                                        return true;
                                                    default:
                                                        Separators |= (uint)componentParseResult;
                                                        if (i != source.Length && (source[i] == 46 || source[i] == 58))
                                                        {
                                                            bytesConsumed = 0;
                                                            return false;
                                                        }
                                                        bytesConsumed = i;
                                                        return true;
                                                }
                                        }
                                }
                        }
                    }

                    private static ComponentParseResult ParseComponent(ReadOnlySpan<byte> source, bool neverParseAsFraction, ref int srcIndex, out uint value)
                    {
                        if (srcIndex == source.Length)
                        {
                            value = 0u;
                            return ComponentParseResult.NoMoreData;
                        }
                        byte b = source[srcIndex];
                        if (b == 58 || (b == 46 && neverParseAsFraction))
                        {
                            srcIndex++;
                            if (!TryParseUInt32D(source.Slice(srcIndex), out value, out var bytesConsumed))
                            {
                                value = 0u;
                                return ComponentParseResult.ParseFailure;
                            }
                            srcIndex += bytesConsumed;
                            if (b != 58)
                            {
                                return ComponentParseResult.Period;
                            }
                            return ComponentParseResult.Colon;
                        }
                        if (b == 46)
                        {
                            srcIndex++;
                            if (!TryParseTimeSpanFraction(source.Slice(srcIndex), out value, out var bytesConsumed2))
                            {
                                value = 0u;
                                return ComponentParseResult.ParseFailure;
                            }
                            srcIndex += bytesConsumed2;
                            return ComponentParseResult.Period;
                        }
                        value = 0u;
                        return ComponentParseResult.NoMoreData;
                    }
                }

                private const uint FlipCase = 32u;

                private const uint NoFlipCase = 0u;

                private static readonly int[] s_daysToMonth365 = new int[13]
                {
                    0, 31, 59, 90, 120, 151, 181, 212, 243, 273,
                    304, 334, 365
                };

                private static readonly int[] s_daysToMonth366 = new int[13]
                {
                    0, 31, 60, 91, 121, 152, 182, 213, 244, 274,
                    305, 335, 366
                };

                /// <summary>
                /// Parses a <see cref="System.Boolean" /> at the start of a UTF-8 string.
                /// </summary>
                /// <param name="source">The UTF-8 string to parse.</param>
                /// <param name="value">When the method returns, contains the value parsed from source, if the parsing operation succeeded.</param>
                /// <param name="bytesConsumed">If the parsing operation was successful, contains the length in bytes of the parsed substring when the method returns. 
                /// If the method fails, bytesConsumed is set to 0.</param>
                /// <param name="standardFormat">The expected format of the UTF-8 string.</param>
                /// <returns><c>true</c> for success; 
                /// <c>false</c> if the string was not syntactically valid or an overflow or underflow occurred.</returns>
                /// <remarks>
                /// Format Supported:
                /// <list type="table">
                ///     <listheader>
                ///         Format <see cref="System.String"/>|||Example expected format
                ///     </listheader>
                ///     <item>G(default)    True/False</item>
                ///     <item>I                    true/false</item>
                /// </list>
                /// </remarks>
                public static bool TryParse(ReadOnlySpan<byte> source, out bool value, out int bytesConsumed, char standardFormat = '\0')
                {
                    if (standardFormat != 0 && standardFormat != 'G' && standardFormat != 'l')
                    {
                        return System.ThrowHelper.TryParseThrowFormatException<bool>(out value, out bytesConsumed);
                    }
                    if (source.Length >= 4)
                    {
                        if ((source[0] == 84 || source[0] == 116) && (source[1] == 82 || source[1] == 114) && (source[2] == 85 || source[2] == 117) && (source[3] == 69 || source[3] == 101))
                        {
                            bytesConsumed = 4;
                            value = true;
                            return true;
                        }
                        if (source.Length >= 5 && (source[0] == 70 || source[0] == 102) && (source[1] == 65 || source[1] == 97) && (source[2] == 76 || source[2] == 108) && (source[3] == 83 || source[3] == 115) && (source[4] == 69 || source[4] == 101))
                        {
                            bytesConsumed = 5;
                            value = false;
                            return true;
                        }
                    }
                    bytesConsumed = 0;
                    value = false;
                    return false;
                }

                /// <summary>
                /// Parses a <see cref="System.DateTime" /> at the start of a UTF-8 string.
                /// </summary>
                /// <param name="source">The UTF-8 string to parse.</param>
                /// <param name="value">When the method returns, contains the value parsed from source, if the parsing operation succeeded.</param>
                /// <param name="bytesConsumed">If the parsing operation was successful, contains the length in bytes of the parsed substring when the method returns. 
                /// If the method fails, bytesConsumed is set to 0.</param>
                /// <param name="standardFormat">The expected format of the UTF-8 string.</param>
                /// <returns><c>true</c> for success; 
                /// <c>false</c> if the string was not syntactically valid or an overflow or underflow occurred.</returns>
                /// <remarks>
                /// Format Supported:
                /// <list type="table">
                ///     <listheader>
                ///         Format <see cref="System.String"/>|||Example expected format|||Comments
                ///     </listheader>
                ///     <item>G(default)    05/25/2017 10:30:15</item>
                ///     <item>R	Tue, 03 Jan 2017 08:08:05 GMT	(RFC 1123)</item>
                ///     <item>l	tue, 03 jan 2017 08:08:05 gmt	(Lowercase RFC 1123)</item>
                ///     <item>O	2017-06-12T05:30:45.7680000-07:00	(Round-trippable)</item>
                /// </list>
                /// </remarks>
                public static bool TryParse(ReadOnlySpan<byte> source, out DateTime value, out int bytesConsumed, char standardFormat = '\0')
                {
                    switch (standardFormat)
                    {
                        case 'R':
                            {
                                if (!TryParseDateTimeOffsetR(source, 0u, out var dateTimeOffset, out bytesConsumed))
                                {
                                    value = default(DateTime);
                                    return false;
                                }
                                value = dateTimeOffset.DateTime;
                                return true;
                            }
                        case 'l':
                            {
                                if (!TryParseDateTimeOffsetR(source, 32u, out var dateTimeOffset2, out bytesConsumed))
                                {
                                    value = default(DateTime);
                                    return false;
                                }
                                value = dateTimeOffset2.DateTime;
                                return true;
                            }
                        case 'O':
                            {
                                if (!TryParseDateTimeOffsetO(source, out var value2, out bytesConsumed, out var kind))
                                {
                                    value = default(DateTime);
                                    bytesConsumed = 0;
                                    return false;
                                }
                                switch (kind)
                                {
                                    case DateTimeKind.Local:
                                        value = value2.LocalDateTime;
                                        break;
                                    case DateTimeKind.Utc:
                                        value = value2.UtcDateTime;
                                        break;
                                    default:
                                        value = value2.DateTime;
                                        break;
                                }
                                return true;
                            }
                        case '\0':
                        case 'G':
                            {
                                DateTimeOffset valueAsOffset;
                                return TryParseDateTimeG(source, out value, out valueAsOffset, out bytesConsumed);
                            }
                        default:
                            return System.ThrowHelper.TryParseThrowFormatException<DateTime>(out value, out bytesConsumed);
                    }
                }

                /// <summary>
                /// Parses a <see cref="System.DateTimeOffset" /> at the start of a UTF-8 string.
                /// </summary>
                /// <param name="source">The UTF-8 string to parse.</param>
                /// <param name="value">When the method returns, contains the value parsed from source, if the parsing operation succeeded.</param>
                /// <param name="bytesConsumed">If the parsing operation was successful, contains the length in bytes of the parsed substring when the method returns. 
                /// If the method fails, bytesConsumed is set to 0.</param>
                /// <param name="standardFormat">The expected format of the UTF-8 string.</param>
                /// <returns><c>true</c> for success; 
                /// <c>false</c> if the string was not syntactically valid or an overflow or underflow occurred.</returns>
                /// <remarks>
                /// Format Supported:
                /// <list type="table">
                ///     <listheader>
                ///         Format <see cref="System.String"/>|||Example expected format|||Comments
                ///     </listheader>
                ///     <item>G(default)    05/25/2017 10:30:15</item>
                ///     <item>R	Tue, 03 Jan 2017 08:08:05 GMT	(RFC 1123)</item>
                ///     <item>l	tue, 03 jan 2017 08:08:05 gmt	(Lowercase RFC 1123)</item>
                ///     <item>O	2017-06-12T05:30:45.7680000-07:00	(Round-trippable)</item>
                /// </list>
                /// </remarks>
                public static bool TryParse(ReadOnlySpan<byte> source, out DateTimeOffset value, out int bytesConsumed, char standardFormat = '\0')
                {
                    DateTimeKind kind;
                    DateTime value2;
                    return standardFormat switch
                    {
                        'R' => TryParseDateTimeOffsetR(source, 0u, out value, out bytesConsumed),
                        'l' => TryParseDateTimeOffsetR(source, 32u, out value, out bytesConsumed),
                        'O' => TryParseDateTimeOffsetO(source, out value, out bytesConsumed, out kind),
                        '\0' => TryParseDateTimeOffsetDefault(source, out value, out bytesConsumed),
                        'G' => TryParseDateTimeG(source, out value2, out value, out bytesConsumed),
                        _ => System.ThrowHelper.TryParseThrowFormatException<DateTimeOffset>(out value, out bytesConsumed),
                    };
                }

                private static bool TryParseDateTimeOffsetDefault(ReadOnlySpan<byte> source, out DateTimeOffset value, out int bytesConsumed)
                {
                    if (source.Length < 26)
                    {
                        bytesConsumed = 0;
                        value = default(DateTimeOffset);
                        return false;
                    }
                    if (!TryParseDateTimeG(source, out var value2, out var _, out var _))
                    {
                        bytesConsumed = 0;
                        value = default(DateTimeOffset);
                        return false;
                    }
                    if (source[19] != 32)
                    {
                        bytesConsumed = 0;
                        value = default(DateTimeOffset);
                        return false;
                    }
                    byte b = source[20];
                    if (b != 43 && b != 45)
                    {
                        bytesConsumed = 0;
                        value = default(DateTimeOffset);
                        return false;
                    }
                    uint num = (uint)(source[21] - 48);
                    uint num2 = (uint)(source[22] - 48);
                    if (num > 9 || num2 > 9)
                    {
                        bytesConsumed = 0;
                        value = default(DateTimeOffset);
                        return false;
                    }
                    int num3 = (int)(num * 10 + num2);
                    if (source[23] != 58)
                    {
                        bytesConsumed = 0;
                        value = default(DateTimeOffset);
                        return false;
                    }
                    uint num4 = (uint)(source[24] - 48);
                    uint num5 = (uint)(source[25] - 48);
                    if (num4 > 9 || num5 > 9)
                    {
                        bytesConsumed = 0;
                        value = default(DateTimeOffset);
                        return false;
                    }
                    int num6 = (int)(num4 * 10 + num5);
                    TimeSpan timeSpan = new TimeSpan(num3, num6, 0);
                    if (b == 45)
                    {
                        timeSpan = -timeSpan;
                    }
                    if (!TryCreateDateTimeOffset(value2, b == 45, num3, num6, out value))
                    {
                        bytesConsumed = 0;
                        value = default(DateTimeOffset);
                        return false;
                    }
                    bytesConsumed = 26;
                    return true;
                }

                private static bool TryParseDateTimeG(ReadOnlySpan<byte> source, out DateTime value, out DateTimeOffset valueAsOffset, out int bytesConsumed)
                {
                    if (source.Length < 19)
                    {
                        bytesConsumed = 0;
                        value = default(DateTime);
                        valueAsOffset = default(DateTimeOffset);
                        return false;
                    }
                    uint num = (uint)(source[0] - 48);
                    uint num2 = (uint)(source[1] - 48);
                    if (num > 9 || num2 > 9)
                    {
                        bytesConsumed = 0;
                        value = default(DateTime);
                        valueAsOffset = default(DateTimeOffset);
                        return false;
                    }
                    int month = (int)(num * 10 + num2);
                    if (source[2] != 47)
                    {
                        bytesConsumed = 0;
                        value = default(DateTime);
                        valueAsOffset = default(DateTimeOffset);
                        return false;
                    }
                    uint num3 = (uint)(source[3] - 48);
                    uint num4 = (uint)(source[4] - 48);
                    if (num3 > 9 || num4 > 9)
                    {
                        bytesConsumed = 0;
                        value = default(DateTime);
                        valueAsOffset = default(DateTimeOffset);
                        return false;
                    }
                    int day = (int)(num3 * 10 + num4);
                    if (source[5] != 47)
                    {
                        bytesConsumed = 0;
                        value = default(DateTime);
                        valueAsOffset = default(DateTimeOffset);
                        return false;
                    }
                    uint num5 = (uint)(source[6] - 48);
                    uint num6 = (uint)(source[7] - 48);
                    uint num7 = (uint)(source[8] - 48);
                    uint num8 = (uint)(source[9] - 48);
                    if (num5 > 9 || num6 > 9 || num7 > 9 || num8 > 9)
                    {
                        bytesConsumed = 0;
                        value = default(DateTime);
                        valueAsOffset = default(DateTimeOffset);
                        return false;
                    }
                    int year = (int)(num5 * 1000 + num6 * 100 + num7 * 10 + num8);
                    if (source[10] != 32)
                    {
                        bytesConsumed = 0;
                        value = default(DateTime);
                        valueAsOffset = default(DateTimeOffset);
                        return false;
                    }
                    uint num9 = (uint)(source[11] - 48);
                    uint num10 = (uint)(source[12] - 48);
                    if (num9 > 9 || num10 > 9)
                    {
                        bytesConsumed = 0;
                        value = default(DateTime);
                        valueAsOffset = default(DateTimeOffset);
                        return false;
                    }
                    int hour = (int)(num9 * 10 + num10);
                    if (source[13] != 58)
                    {
                        bytesConsumed = 0;
                        value = default(DateTime);
                        valueAsOffset = default(DateTimeOffset);
                        return false;
                    }
                    uint num11 = (uint)(source[14] - 48);
                    uint num12 = (uint)(source[15] - 48);
                    if (num11 > 9 || num12 > 9)
                    {
                        bytesConsumed = 0;
                        value = default(DateTime);
                        valueAsOffset = default(DateTimeOffset);
                        return false;
                    }
                    int minute = (int)(num11 * 10 + num12);
                    if (source[16] != 58)
                    {
                        bytesConsumed = 0;
                        value = default(DateTime);
                        valueAsOffset = default(DateTimeOffset);
                        return false;
                    }
                    uint num13 = (uint)(source[17] - 48);
                    uint num14 = (uint)(source[18] - 48);
                    if (num13 > 9 || num14 > 9)
                    {
                        bytesConsumed = 0;
                        value = default(DateTime);
                        valueAsOffset = default(DateTimeOffset);
                        return false;
                    }
                    int second = (int)(num13 * 10 + num14);
                    if (!TryCreateDateTimeOffsetInterpretingDataAsLocalTime(year, month, day, hour, minute, second, 0, out valueAsOffset))
                    {
                        bytesConsumed = 0;
                        value = default(DateTime);
                        valueAsOffset = default(DateTimeOffset);
                        return false;
                    }
                    bytesConsumed = 19;
                    value = valueAsOffset.DateTime;
                    return true;
                }

                private static bool TryCreateDateTimeOffset(DateTime dateTime, bool offsetNegative, int offsetHours, int offsetMinutes, out DateTimeOffset value)
                {
                    if ((uint)offsetHours > 14u)
                    {
                        value = default(DateTimeOffset);
                        return false;
                    }
                    if ((uint)offsetMinutes > 59u)
                    {
                        value = default(DateTimeOffset);
                        return false;
                    }
                    if (offsetHours == 14 && offsetMinutes != 0)
                    {
                        value = default(DateTimeOffset);
                        return false;
                    }
                    long num = ((long)offsetHours * 3600L + (long)offsetMinutes * 60L) * 10000000;
                    if (offsetNegative)
                    {
                        num = -num;
                    }
                    try
                    {
                        value = new DateTimeOffset(dateTime.Ticks, new TimeSpan(num));
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        value = default(DateTimeOffset);
                        return false;
                    }
                    return true;
                }

                private static bool TryCreateDateTimeOffset(int year, int month, int day, int hour, int minute, int second, int fraction, bool offsetNegative, int offsetHours, int offsetMinutes, out DateTimeOffset value)
                {
                    if (!TryCreateDateTime(year, month, day, hour, minute, second, fraction, DateTimeKind.Unspecified, out var value2))
                    {
                        value = default(DateTimeOffset);
                        return false;
                    }
                    if (!TryCreateDateTimeOffset(value2, offsetNegative, offsetHours, offsetMinutes, out value))
                    {
                        value = default(DateTimeOffset);
                        return false;
                    }
                    return true;
                }

                private static bool TryCreateDateTimeOffsetInterpretingDataAsLocalTime(int year, int month, int day, int hour, int minute, int second, int fraction, out DateTimeOffset value)
                {
                    if (!TryCreateDateTime(year, month, day, hour, minute, second, fraction, DateTimeKind.Local, out var value2))
                    {
                        value = default(DateTimeOffset);
                        return false;
                    }
                    try
                    {
                        value = new DateTimeOffset(value2);
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        value = default(DateTimeOffset);
                        return false;
                    }
                    return true;
                }

                private static bool TryCreateDateTime(int year, int month, int day, int hour, int minute, int second, int fraction, DateTimeKind kind, out DateTime value)
                {
                    if (year == 0)
                    {
                        value = default(DateTime);
                        return false;
                    }
                    if ((uint)(month - 1) >= 12u)
                    {
                        value = default(DateTime);
                        return false;
                    }
                    uint num = (uint)(day - 1);
                    if (num >= 28 && num >= DateTime.DaysInMonth(year, month))
                    {
                        value = default(DateTime);
                        return false;
                    }
                    if ((uint)hour > 23u)
                    {
                        value = default(DateTime);
                        return false;
                    }
                    if ((uint)minute > 59u)
                    {
                        value = default(DateTime);
                        return false;
                    }
                    if ((uint)second > 59u)
                    {
                        value = default(DateTime);
                        return false;
                    }
                    int[] array = (DateTime.IsLeapYear(year) ? s_daysToMonth366 : s_daysToMonth365);
                    int num2 = year - 1;
                    int num3 = num2 * 365 + num2 / 4 - num2 / 100 + num2 / 400 + array[month - 1] + day - 1;
                    long num4 = num3 * 864000000000L;
                    int num5 = hour * 3600 + minute * 60 + second;
                    num4 += (long)num5 * 10000000L;
                    num4 += fraction;
                    value = new DateTime(num4, kind);
                    return true;
                }

                private static bool TryParseDateTimeOffsetO(ReadOnlySpan<byte> source, out DateTimeOffset value, out int bytesConsumed, out DateTimeKind kind)
                {
                    if (source.Length < 27)
                    {
                        value = default(DateTimeOffset);
                        bytesConsumed = 0;
                        kind = DateTimeKind.Unspecified;
                        return false;
                    }
                    uint num = (uint)(source[0] - 48);
                    uint num2 = (uint)(source[1] - 48);
                    uint num3 = (uint)(source[2] - 48);
                    uint num4 = (uint)(source[3] - 48);
                    if (num > 9 || num2 > 9 || num3 > 9 || num4 > 9)
                    {
                        value = default(DateTimeOffset);
                        bytesConsumed = 0;
                        kind = DateTimeKind.Unspecified;
                        return false;
                    }
                    int year = (int)(num * 1000 + num2 * 100 + num3 * 10 + num4);
                    if (source[4] != 45)
                    {
                        value = default(DateTimeOffset);
                        bytesConsumed = 0;
                        kind = DateTimeKind.Unspecified;
                        return false;
                    }
                    uint num5 = (uint)(source[5] - 48);
                    uint num6 = (uint)(source[6] - 48);
                    if (num5 > 9 || num6 > 9)
                    {
                        value = default(DateTimeOffset);
                        bytesConsumed = 0;
                        kind = DateTimeKind.Unspecified;
                        return false;
                    }
                    int month = (int)(num5 * 10 + num6);
                    if (source[7] != 45)
                    {
                        value = default(DateTimeOffset);
                        bytesConsumed = 0;
                        kind = DateTimeKind.Unspecified;
                        return false;
                    }
                    uint num7 = (uint)(source[8] - 48);
                    uint num8 = (uint)(source[9] - 48);
                    if (num7 > 9 || num8 > 9)
                    {
                        value = default(DateTimeOffset);
                        bytesConsumed = 0;
                        kind = DateTimeKind.Unspecified;
                        return false;
                    }
                    int day = (int)(num7 * 10 + num8);
                    if (source[10] != 84)
                    {
                        value = default(DateTimeOffset);
                        bytesConsumed = 0;
                        kind = DateTimeKind.Unspecified;
                        return false;
                    }
                    uint num9 = (uint)(source[11] - 48);
                    uint num10 = (uint)(source[12] - 48);
                    if (num9 > 9 || num10 > 9)
                    {
                        value = default(DateTimeOffset);
                        bytesConsumed = 0;
                        kind = DateTimeKind.Unspecified;
                        return false;
                    }
                    int hour = (int)(num9 * 10 + num10);
                    if (source[13] != 58)
                    {
                        value = default(DateTimeOffset);
                        bytesConsumed = 0;
                        kind = DateTimeKind.Unspecified;
                        return false;
                    }
                    uint num11 = (uint)(source[14] - 48);
                    uint num12 = (uint)(source[15] - 48);
                    if (num11 > 9 || num12 > 9)
                    {
                        value = default(DateTimeOffset);
                        bytesConsumed = 0;
                        kind = DateTimeKind.Unspecified;
                        return false;
                    }
                    int minute = (int)(num11 * 10 + num12);
                    if (source[16] != 58)
                    {
                        value = default(DateTimeOffset);
                        bytesConsumed = 0;
                        kind = DateTimeKind.Unspecified;
                        return false;
                    }
                    uint num13 = (uint)(source[17] - 48);
                    uint num14 = (uint)(source[18] - 48);
                    if (num13 > 9 || num14 > 9)
                    {
                        value = default(DateTimeOffset);
                        bytesConsumed = 0;
                        kind = DateTimeKind.Unspecified;
                        return false;
                    }
                    int second = (int)(num13 * 10 + num14);
                    if (source[19] != 46)
                    {
                        value = default(DateTimeOffset);
                        bytesConsumed = 0;
                        kind = DateTimeKind.Unspecified;
                        return false;
                    }
                    uint num15 = (uint)(source[20] - 48);
                    uint num16 = (uint)(source[21] - 48);
                    uint num17 = (uint)(source[22] - 48);
                    uint num18 = (uint)(source[23] - 48);
                    uint num19 = (uint)(source[24] - 48);
                    uint num20 = (uint)(source[25] - 48);
                    uint num21 = (uint)(source[26] - 48);
                    if (num15 > 9 || num16 > 9 || num17 > 9 || num18 > 9 || num19 > 9 || num20 > 9 || num21 > 9)
                    {
                        value = default(DateTimeOffset);
                        bytesConsumed = 0;
                        kind = DateTimeKind.Unspecified;
                        return false;
                    }
                    int fraction = (int)(num15 * 1000000 + num16 * 100000 + num17 * 10000 + num18 * 1000 + num19 * 100 + num20 * 10 + num21);
                    byte b = (byte)((source.Length > 27) ? source[27] : 0);
                    if (b != 90 && b != 43 && b != 45)
                    {
                        if (!TryCreateDateTimeOffsetInterpretingDataAsLocalTime(year, month, day, hour, minute, second, fraction, out value))
                        {
                            value = default(DateTimeOffset);
                            bytesConsumed = 0;
                            kind = DateTimeKind.Unspecified;
                            return false;
                        }
                        bytesConsumed = 27;
                        kind = DateTimeKind.Unspecified;
                        return true;
                    }
                    if (b == 90)
                    {
                        if (!TryCreateDateTimeOffset(year, month, day, hour, minute, second, fraction, offsetNegative: false, 0, 0, out value))
                        {
                            value = default(DateTimeOffset);
                            bytesConsumed = 0;
                            kind = DateTimeKind.Unspecified;
                            return false;
                        }
                        bytesConsumed = 28;
                        kind = DateTimeKind.Utc;
                        return true;
                    }
                    if (source.Length < 33)
                    {
                        value = default(DateTimeOffset);
                        bytesConsumed = 0;
                        kind = DateTimeKind.Unspecified;
                        return false;
                    }
                    uint num22 = (uint)(source[28] - 48);
                    uint num23 = (uint)(source[29] - 48);
                    if (num22 > 9 || num23 > 9)
                    {
                        value = default(DateTimeOffset);
                        bytesConsumed = 0;
                        kind = DateTimeKind.Unspecified;
                        return false;
                    }
                    int offsetHours = (int)(num22 * 10 + num23);
                    if (source[30] != 58)
                    {
                        value = default(DateTimeOffset);
                        bytesConsumed = 0;
                        kind = DateTimeKind.Unspecified;
                        return false;
                    }
                    uint num24 = (uint)(source[31] - 48);
                    uint num25 = (uint)(source[32] - 48);
                    if (num24 > 9 || num25 > 9)
                    {
                        value = default(DateTimeOffset);
                        bytesConsumed = 0;
                        kind = DateTimeKind.Unspecified;
                        return false;
                    }
                    int offsetMinutes = (int)(num24 * 10 + num25);
                    if (!TryCreateDateTimeOffset(year, month, day, hour, minute, second, fraction, b == 45, offsetHours, offsetMinutes, out value))
                    {
                        value = default(DateTimeOffset);
                        bytesConsumed = 0;
                        kind = DateTimeKind.Unspecified;
                        return false;
                    }
                    bytesConsumed = 33;
                    kind = DateTimeKind.Local;
                    return true;
                }

                private static bool TryParseDateTimeOffsetR(ReadOnlySpan<byte> source, uint caseFlipXorMask, out DateTimeOffset dateTimeOffset, out int bytesConsumed)
                {
                    if (source.Length < 29)
                    {
                        bytesConsumed = 0;
                        dateTimeOffset = default(DateTimeOffset);
                        return false;
                    }
                    uint num = source[0] ^ caseFlipXorMask;
                    uint num2 = source[1];
                    uint num3 = source[2];
                    uint num4 = source[3];
                    DayOfWeek dayOfWeek;
                    switch ((num << 24) | (num2 << 16) | (num3 << 8) | num4)
                    {
                        case 1400204844u:
                            dayOfWeek = DayOfWeek.Sunday;
                            break;
                        case 1299148332u:
                            dayOfWeek = DayOfWeek.Monday;
                            break;
                        case 1416979756u:
                            dayOfWeek = DayOfWeek.Tuesday;
                            break;
                        case 1466262572u:
                            dayOfWeek = DayOfWeek.Wednesday;
                            break;
                        case 1416131884u:
                            dayOfWeek = DayOfWeek.Thursday;
                            break;
                        case 1181903148u:
                            dayOfWeek = DayOfWeek.Friday;
                            break;
                        case 1398895660u:
                            dayOfWeek = DayOfWeek.Saturday;
                            break;
                        default:
                            bytesConsumed = 0;
                            dateTimeOffset = default(DateTimeOffset);
                            return false;
                    }
                    if (source[4] != 32)
                    {
                        bytesConsumed = 0;
                        dateTimeOffset = default(DateTimeOffset);
                        return false;
                    }
                    uint num5 = (uint)(source[5] - 48);
                    uint num6 = (uint)(source[6] - 48);
                    if (num5 > 9 || num6 > 9)
                    {
                        bytesConsumed = 0;
                        dateTimeOffset = default(DateTimeOffset);
                        return false;
                    }
                    int day = (int)(num5 * 10 + num6);
                    if (source[7] != 32)
                    {
                        bytesConsumed = 0;
                        dateTimeOffset = default(DateTimeOffset);
                        return false;
                    }
                    uint num7 = source[8] ^ caseFlipXorMask;
                    uint num8 = source[9];
                    uint num9 = source[10];
                    uint num10 = source[11];
                    int month;
                    switch ((num7 << 24) | (num8 << 16) | (num9 << 8) | num10)
                    {
                        case 1247899168u:
                            month = 1;
                            break;
                        case 1181049376u:
                            month = 2;
                            break;
                        case 1298231840u:
                            month = 3;
                            break;
                        case 1097888288u:
                            month = 4;
                            break;
                        case 1298233632u:
                            month = 5;
                            break;
                        case 1249209888u:
                            month = 6;
                            break;
                        case 1249209376u:
                            month = 7;
                            break;
                        case 1098213152u:
                            month = 8;
                            break;
                        case 1399156768u:
                            month = 9;
                            break;
                        case 1331917856u:
                            month = 10;
                            break;
                        case 1315927584u:
                            month = 11;
                            break;
                        case 1147495200u:
                            month = 12;
                            break;
                        default:
                            bytesConsumed = 0;
                            dateTimeOffset = default(DateTimeOffset);
                            return false;
                    }
                    uint num11 = (uint)(source[12] - 48);
                    uint num12 = (uint)(source[13] - 48);
                    uint num13 = (uint)(source[14] - 48);
                    uint num14 = (uint)(source[15] - 48);
                    if (num11 > 9 || num12 > 9 || num13 > 9 || num14 > 9)
                    {
                        bytesConsumed = 0;
                        dateTimeOffset = default(DateTimeOffset);
                        return false;
                    }
                    int year = (int)(num11 * 1000 + num12 * 100 + num13 * 10 + num14);
                    if (source[16] != 32)
                    {
                        bytesConsumed = 0;
                        dateTimeOffset = default(DateTimeOffset);
                        return false;
                    }
                    uint num15 = (uint)(source[17] - 48);
                    uint num16 = (uint)(source[18] - 48);
                    if (num15 > 9 || num16 > 9)
                    {
                        bytesConsumed = 0;
                        dateTimeOffset = default(DateTimeOffset);
                        return false;
                    }
                    int hour = (int)(num15 * 10 + num16);
                    if (source[19] != 58)
                    {
                        bytesConsumed = 0;
                        dateTimeOffset = default(DateTimeOffset);
                        return false;
                    }
                    uint num17 = (uint)(source[20] - 48);
                    uint num18 = (uint)(source[21] - 48);
                    if (num17 > 9 || num18 > 9)
                    {
                        bytesConsumed = 0;
                        dateTimeOffset = default(DateTimeOffset);
                        return false;
                    }
                    int minute = (int)(num17 * 10 + num18);
                    if (source[22] != 58)
                    {
                        bytesConsumed = 0;
                        dateTimeOffset = default(DateTimeOffset);
                        return false;
                    }
                    uint num19 = (uint)(source[23] - 48);
                    uint num20 = (uint)(source[24] - 48);
                    if (num19 > 9 || num20 > 9)
                    {
                        bytesConsumed = 0;
                        dateTimeOffset = default(DateTimeOffset);
                        return false;
                    }
                    int second = (int)(num19 * 10 + num20);
                    uint num21 = source[25];
                    uint num22 = source[26] ^ caseFlipXorMask;
                    uint num23 = source[27] ^ caseFlipXorMask;
                    uint num24 = source[28] ^ caseFlipXorMask;
                    uint num25 = (num21 << 24) | (num22 << 16) | (num23 << 8) | num24;
                    if (num25 != 541543764)
                    {
                        bytesConsumed = 0;
                        dateTimeOffset = default(DateTimeOffset);
                        return false;
                    }
                    if (!TryCreateDateTimeOffset(year, month, day, hour, minute, second, 0, offsetNegative: false, 0, 0, out dateTimeOffset))
                    {
                        bytesConsumed = 0;
                        dateTimeOffset = default(DateTimeOffset);
                        return false;
                    }
                    if (dayOfWeek != dateTimeOffset.DayOfWeek)
                    {
                        bytesConsumed = 0;
                        dateTimeOffset = default(DateTimeOffset);
                        return false;
                    }
                    bytesConsumed = 29;
                    return true;
                }

                /// <summary>
                /// Parses a <see cref="System.Decimal"/> at the start of a UTF-8 string.
                /// </summary>
                /// <param name="source">The UTF-8 string to parse.</param>
                /// <param name="value">When the method returns, contains the value parsed from source, if the parsing operation succeeded.</param>
                /// <param name="bytesConsumed">If the parsing operation was successful, contains the length in bytes of the parsed substring when the method returns. 
                /// <param name="standardFormat">The expected format of the UTF-8 string.</param>
                /// If the method fails, bytesConsumed is set to 0.</param>
                /// <returns><c>true</c> for success; 
                /// <c>false</c> if the string was not syntactically valid or an overflow or underflow occurred.</returns>
                /// <remarks>
                /// Format Supported:
                /// <list type="table">
                ///     <listheader>
                ///         Format <see cref="System.String"/>|||Example expected format|||Comments
                ///     </listheader>
                ///     <item>G/g(default)    </item>
                ///     <item>F/f	12.45	Fixed point</item>
                ///     <item>E/e	1.245000e1	Exponential</item>
                /// </list>
                /// </remarks>
                public static bool TryParse(ReadOnlySpan<byte> source, out decimal value, out int bytesConsumed, char standardFormat = '\0')
                {
                    ParseNumberOptions options;
                    switch (standardFormat)
                    {
                        case '\0':
                        case 'E':
                        case 'G':
                        case 'e':
                        case 'g':
                            options = ParseNumberOptions.AllowExponent;
                            break;
                        case 'F':
                        case 'f':
                            options = (ParseNumberOptions)0;
                            break;
                        default:
                            return System.ThrowHelper.TryParseThrowFormatException<decimal>(out value, out bytesConsumed);
                    }
                    NumberBuffer number = default(NumberBuffer);
                    if (!TryParseNumber(source, ref number, out bytesConsumed, options, out var textUsedExponentNotation))
                    {
                        value = default(decimal);
                        return false;
                    }
                    if (!textUsedExponentNotation && (standardFormat == 'E' || standardFormat == 'e'))
                    {
                        value = default(decimal);
                        bytesConsumed = 0;
                        return false;
                    }
                    if (number.Digits[0] == 0 && number.Scale == 0)
                    {
                        number.IsNegative = false;
                    }
                    value = default(decimal);
                    if (!System.Number.NumberBufferToDecimal(ref number, ref value))
                    {
                        value = default(decimal);
                        bytesConsumed = 0;
                        return false;
                    }
                    return true;
                }

                /// <summary>
                /// Parses a <see cref="System.Single"/> at the start of a UTF-8 string.
                /// </summary>
                /// <param name="source">The UTF-8 string to parse.</param>
                /// <param name="value">When the method returns, contains the value parsed from source, if the parsing operation succeeded.</param>
                /// <param name="bytesConsumed">If the parsing operation was successful, contains the length in bytes of the parsed substring when the method returns. 
                /// <param name="standardFormat">The expected format of the UTF-8 string.</param>
                /// If the method fails, bytesConsumed is set to 0.</param>
                /// <returns><c>true</c> for success; 
                /// <c>false</c> if the string was not syntactically valid or an overflow or underflow occurred.</returns>
                /// <remarks>
                /// Format Supported:
                /// <list type="table">
                ///     <listheader>
                ///         Format <see cref="System.String"/>|||Example expected format|||Comments
                ///     </listheader>
                ///     <item>G/g(default)    </item>
                ///     <item>F/f	12.45	Fixed point</item>
                ///     <item>E/e	1.245000e1	Exponential</item>
                /// </list>
                /// </remarks>
                public static bool TryParse(ReadOnlySpan<byte> source, out float value, out int bytesConsumed, char standardFormat = '\0')
                {
                    if (TryParseNormalAsFloatingPoint(source, out var value2, out bytesConsumed, standardFormat))
                    {
                        value = (float)value2;
                        if (float.IsInfinity(value))
                        {
                            value = 0f;
                            bytesConsumed = 0;
                            return false;
                        }
                        return true;
                    }
                    return TryParseAsSpecialFloatingPoint(source, float.PositiveInfinity, float.NegativeInfinity, float.NaN, out value, out bytesConsumed);
                }

                /// <summary>
                /// Parses a <see cref="System.Double"/> at the start of a UTF-8 string.
                /// </summary>
                /// <param name="source">The UTF-8 string to parse.</param>
                /// <param name="value">When the method returns, contains the value parsed from source, if the parsing operation succeeded.</param>
                /// <param name="bytesConsumed">If the parsing operation was successful, contains the length in bytes of the parsed substring when the method returns. 
                /// <param name="standardFormat">The expected format of the UTF-8 string.</param>
                /// If the method fails, bytesConsumed is set to 0.</param>
                /// <returns><c>true</c> for success; 
                /// <c>false</c> if the string was not syntactically valid or an overflow or underflow occurred.</returns>
                /// <remarks>
                /// Format Supported:
                /// <list type="table">
                ///     <listheader>
                ///         Format <see cref="System.String"/>|||Example expected format|||Comments
                ///     </listheader>
                ///     <item>G/g(default)    </item>
                ///     <item>F/f	12.45	Fixed point</item>
                ///     <item>E/e	1.245000e1	Exponential</item>
                /// </list>
                /// </remarks>
                public static bool TryParse(ReadOnlySpan<byte> source, out double value, out int bytesConsumed, char standardFormat = '\0')
                {
                    if (TryParseNormalAsFloatingPoint(source, out value, out bytesConsumed, standardFormat))
                    {
                        return true;
                    }
                    return TryParseAsSpecialFloatingPoint(source, double.PositiveInfinity, double.NegativeInfinity, double.NaN, out value, out bytesConsumed);
                }

                private static bool TryParseNormalAsFloatingPoint(ReadOnlySpan<byte> source, out double value, out int bytesConsumed, char standardFormat)
                {
                    ParseNumberOptions options;
                    switch (standardFormat)
                    {
                        case '\0':
                        case 'E':
                        case 'G':
                        case 'e':
                        case 'g':
                            options = ParseNumberOptions.AllowExponent;
                            break;
                        case 'F':
                        case 'f':
                            options = (ParseNumberOptions)0;
                            break;
                        default:
                            return System.ThrowHelper.TryParseThrowFormatException<double>(out value, out bytesConsumed);
                    }
                    NumberBuffer number = default(NumberBuffer);
                    if (!TryParseNumber(source, ref number, out bytesConsumed, options, out var textUsedExponentNotation))
                    {
                        value = 0.0;
                        return false;
                    }
                    if (!textUsedExponentNotation && (standardFormat == 'E' || standardFormat == 'e'))
                    {
                        value = 0.0;
                        bytesConsumed = 0;
                        return false;
                    }
                    if (number.Digits[0] == 0)
                    {
                        number.IsNegative = false;
                    }
                    if (!System.Number.NumberBufferToDouble(ref number, out value))
                    {
                        value = 0.0;
                        bytesConsumed = 0;
                        return false;
                    }
                    return true;
                }

                private static bool TryParseAsSpecialFloatingPoint<T>(ReadOnlySpan<byte> source, T positiveInfinity, T negativeInfinity, T nan, out T value, out int bytesConsumed)
                {
                    if (source.Length >= 8 && source[0] == 73 && source[1] == 110 && source[2] == 102 && source[3] == 105 && source[4] == 110 && source[5] == 105 && source[6] == 116 && source[7] == 121)
                    {
                        value = positiveInfinity;
                        bytesConsumed = 8;
                        return true;
                    }
                    if (source.Length >= 9 && source[0] == 45 && source[1] == 73 && source[2] == 110 && source[3] == 102 && source[4] == 105 && source[5] == 110 && source[6] == 105 && source[7] == 116 && source[8] == 121)
                    {
                        value = negativeInfinity;
                        bytesConsumed = 9;
                        return true;
                    }
                    if (source.Length >= 3 && source[0] == 78 && source[1] == 97 && source[2] == 78)
                    {
                        value = nan;
                        bytesConsumed = 3;
                        return true;
                    }
                    value = default(T);
                    bytesConsumed = 0;
                    return false;
                }

                /// <summary>
                /// Parses a <see cref="System.Guid"/> at the start of a UTF-8 string.
                /// </summary>
                /// <param name="source">The UTF-8 string to parse.</param>
                /// <param name="value">When the method returns, contains the value parsed from source, if the parsing operation succeeded.</param>
                /// <param name="bytesConsumed">If the parsing operation was successful, contains the length in bytes of the parsed substring when the method returns. 
                /// <param name="standardFormat">The expected format of the UTF-8 string.</param>
                /// If the method fails, bytesConsumed is set to 0.</param>
                /// <returns><c>true</c> for success; 
                /// <c>false</c> if the string was not syntactically valid or an overflow or underflow occurred.</returns>
                /// <remarks>
                /// Format Supported:
                /// <list type="table">
                ///     <listheader>
                ///         Format <see cref="System.String"/>|||Example expected format|||Comments
                ///     </listheader>
                ///     <item>D (default)	nnnnnnnn-nnnn-nnnn-nnnn-nnnnnnnnnnnn</item>
                ///     <item>B	{nnnnnnnn-nnnn-nnnn-nnnn-nnnnnnnnnnnn}</item>
                ///     <item>P	(nnnnnnnn-nnnn-nnnn-nnnn-nnnnnnnnnnnn)</item>
                ///     <item>N	nnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnn</item>
                /// </list>
                /// </remarks>
                public static bool TryParse(ReadOnlySpan<byte> source, out Guid value, out int bytesConsumed, char standardFormat = '\0')
                {
                    switch (standardFormat)
                    {
                        case '\0':
                        case 'D':
                            return TryParseGuidCore(source, ends: false, ' ', ' ', out value, out bytesConsumed);
                        case 'B':
                            return TryParseGuidCore(source, ends: true, '{', '}', out value, out bytesConsumed);
                        case 'P':
                            return TryParseGuidCore(source, ends: true, '(', ')', out value, out bytesConsumed);
                        case 'N':
                            return TryParseGuidN(source, out value, out bytesConsumed);
                        default:
                            return System.ThrowHelper.TryParseThrowFormatException<Guid>(out value, out bytesConsumed);
                    }
                }

                private static bool TryParseGuidN(ReadOnlySpan<byte> text, out Guid value, out int bytesConsumed)
                {
                    if (text.Length < 32)
                    {
                        value = default(Guid);
                        bytesConsumed = 0;
                        return false;
                    }
                    if (!TryParseUInt32X(text.Slice(0, 8), out var value2, out var bytesConsumed2) || bytesConsumed2 != 8)
                    {
                        value = default(Guid);
                        bytesConsumed = 0;
                        return false;
                    }
                    if (!TryParseUInt16X(text.Slice(8, 4), out var value3, out bytesConsumed2) || bytesConsumed2 != 4)
                    {
                        value = default(Guid);
                        bytesConsumed = 0;
                        return false;
                    }
                    if (!TryParseUInt16X(text.Slice(12, 4), out var value4, out bytesConsumed2) || bytesConsumed2 != 4)
                    {
                        value = default(Guid);
                        bytesConsumed = 0;
                        return false;
                    }
                    if (!TryParseUInt16X(text.Slice(16, 4), out var value5, out bytesConsumed2) || bytesConsumed2 != 4)
                    {
                        value = default(Guid);
                        bytesConsumed = 0;
                        return false;
                    }
                    if (!TryParseUInt64X(text.Slice(20), out var value6, out bytesConsumed2) || bytesConsumed2 != 12)
                    {
                        value = default(Guid);
                        bytesConsumed = 0;
                        return false;
                    }
                    bytesConsumed = 32;
                    value = new Guid((int)value2, (short)value3, (short)value4, (byte)(value5 >> 8), (byte)value5, (byte)(value6 >> 40), (byte)(value6 >> 32), (byte)(value6 >> 24), (byte)(value6 >> 16), (byte)(value6 >> 8), (byte)value6);
                    return true;
                }

                private static bool TryParseGuidCore(ReadOnlySpan<byte> source, bool ends, char begin, char end, out Guid value, out int bytesConsumed)
                {
                    int num = 36 + (ends ? 2 : 0);
                    if (source.Length < num)
                    {
                        value = default(Guid);
                        bytesConsumed = 0;
                        return false;
                    }
                    if (ends)
                    {
                        if (source[0] != begin)
                        {
                            value = default(Guid);
                            bytesConsumed = 0;
                            return false;
                        }
                        source = source.Slice(1);
                    }
                    if (!TryParseUInt32X(source, out var value2, out var bytesConsumed2))
                    {
                        value = default(Guid);
                        bytesConsumed = 0;
                        return false;
                    }
                    if (bytesConsumed2 != 8)
                    {
                        value = default(Guid);
                        bytesConsumed = 0;
                        return false;
                    }
                    if (source[bytesConsumed2] != 45)
                    {
                        value = default(Guid);
                        bytesConsumed = 0;
                        return false;
                    }
                    source = source.Slice(9);
                    if (!TryParseUInt16X(source, out var value3, out bytesConsumed2))
                    {
                        value = default(Guid);
                        bytesConsumed = 0;
                        return false;
                    }
                    if (bytesConsumed2 != 4)
                    {
                        value = default(Guid);
                        bytesConsumed = 0;
                        return false;
                    }
                    if (source[bytesConsumed2] != 45)
                    {
                        value = default(Guid);
                        bytesConsumed = 0;
                        return false;
                    }
                    source = source.Slice(5);
                    if (!TryParseUInt16X(source, out var value4, out bytesConsumed2))
                    {
                        value = default(Guid);
                        bytesConsumed = 0;
                        return false;
                    }
                    if (bytesConsumed2 != 4)
                    {
                        value = default(Guid);
                        bytesConsumed = 0;
                        return false;
                    }
                    if (source[bytesConsumed2] != 45)
                    {
                        value = default(Guid);
                        bytesConsumed = 0;
                        return false;
                    }
                    source = source.Slice(5);
                    if (!TryParseUInt16X(source, out var value5, out bytesConsumed2))
                    {
                        value = default(Guid);
                        bytesConsumed = 0;
                        return false;
                    }
                    if (bytesConsumed2 != 4)
                    {
                        value = default(Guid);
                        bytesConsumed = 0;
                        return false;
                    }
                    if (source[bytesConsumed2] != 45)
                    {
                        value = default(Guid);
                        bytesConsumed = 0;
                        return false;
                    }
                    source = source.Slice(5);
                    if (!TryParseUInt64X(source, out var value6, out bytesConsumed2))
                    {
                        value = default(Guid);
                        bytesConsumed = 0;
                        return false;
                    }
                    if (bytesConsumed2 != 12)
                    {
                        value = default(Guid);
                        bytesConsumed = 0;
                        return false;
                    }
                    if (ends && source[bytesConsumed2] != end)
                    {
                        value = default(Guid);
                        bytesConsumed = 0;
                        return false;
                    }
                    bytesConsumed = num;
                    value = new Guid((int)value2, (short)value3, (short)value4, (byte)(value5 >> 8), (byte)value5, (byte)(value6 >> 40), (byte)(value6 >> 32), (byte)(value6 >> 24), (byte)(value6 >> 16), (byte)(value6 >> 8), (byte)value6);
                    return true;
                }

                /// <summary>
                /// Parses a <see cref="System.Byte"/> at the start of a UTF-8 string.
                /// </summary>
                /// <param name="source">The UTF-8 string to parse.</param>
                /// <param name="value">When the method returns, contains the value parsed from source, if the parsing operation succeeded.</param>
                /// <param name="bytesConsumed">If the parsing operation was successful, contains the length in bytes of the parsed substring when the method returns. 
                /// <param name="standardFormat">The expected format of the UTF-8 string.</param>
                /// If the method fails, bytesConsumed is set to 0.</param>
                /// <returns><c>true</c> for success; 
                /// <c>false</c> if the string was not syntactically valid or an overflow or underflow occurred.</returns>
                /// <remarks>
                /// Format Supported:
                /// <list type="table">
                ///     <listheader>
                ///         Format <see cref="System.String"/>|||Example expected format
                ///     </listheader>
                ///     <item>G/g(default)    </item>
                ///     <item>D/d                   122</item>
                ///     <item>N/n                   122</item>
                ///     <item>X/x                   7a  </item>
                /// </list>
                /// </remarks>
                [CLSCompliant(false)]
                public static bool TryParse(ReadOnlySpan<byte> source, out sbyte value, out int bytesConsumed, char standardFormat = '\0')
                {
                    switch (standardFormat)
                    {
                        case '\0':
                        case 'D':
                        case 'G':
                        case 'd':
                        case 'g':
                            return TryParseSByteD(source, out value, out bytesConsumed);
                        case 'N':
                        case 'n':
                            return TryParseSByteN(source, out value, out bytesConsumed);
                        case 'X':
                        case 'x':
                            value = 0;
                            return TryParseByteX(source, out Unsafe.As<sbyte, byte>(ref value), out bytesConsumed);
                        default:
                            return System.ThrowHelper.TryParseThrowFormatException<sbyte>(out value, out bytesConsumed);
                    }
                }

                /// <summary>
                /// Parses a <see cref="System.Int16"/> at the start of a UTF-8 string.
                /// </summary>
                /// <param name="source">The UTF-8 string to parse.</param>
                /// <param name="value">When the method returns, contains the value parsed from source, if the parsing operation succeeded.</param>
                /// <param name="bytesConsumed">If the parsing operation was successful, contains the length in bytes of the parsed substring when the method returns. 
                /// <param name="standardFormat">The expected format of the UTF-8 string.</param>
                /// If the method fails, bytesConsumed is set to 0.</param>
                /// <returns><c>true</c> for success; 
                /// <c>false</c> if the string was not syntactically valid or an overflow or underflow occurred.</returns>
                /// <remarks>
                /// Format Supported:
                /// <list type="table">
                ///     <listheader>
                ///         Format <see cref="System.String"/>|||Example expected format|||Comments
                ///     </listheader>
                ///     <item>G/g(default)    </item>
                ///     <item>D/d                   32767</item>
                ///     <item>N/n                   32,767</item>
                ///     <item>X/x                   7fff  </item>
                /// </list>
                /// </remarks>
                public static bool TryParse(ReadOnlySpan<byte> source, out short value, out int bytesConsumed, char standardFormat = '\0')
                {
                    switch (standardFormat)
                    {
                        case '\0':
                        case 'D':
                        case 'G':
                        case 'd':
                        case 'g':
                            return TryParseInt16D(source, out value, out bytesConsumed);
                        case 'N':
                        case 'n':
                            return TryParseInt16N(source, out value, out bytesConsumed);
                        case 'X':
                        case 'x':
                            value = 0;
                            return TryParseUInt16X(source, out Unsafe.As<short, ushort>(ref value), out bytesConsumed);
                        default:
                            return System.ThrowHelper.TryParseThrowFormatException<short>(out value, out bytesConsumed);
                    }
                }

                /// <summary>
                /// Parses a <see cref="System.Int32"/> at the start of a UTF-8 string.
                /// </summary>
                /// <param name="source">The UTF-8 string to parse.</param>
                /// <param name="value">When the method returns, contains the value parsed from source, if the parsing operation succeeded.</param>
                /// <param name="bytesConsumed">If the parsing operation was successful, contains the length in bytes of the parsed substring when the method returns. 
                /// <param name="standardFormat">The expected format of the UTF-8 string.</param>
                /// If the method fails, bytesConsumed is set to 0.</param>
                /// <returns><c>true</c> for success; 
                /// <c>false</c> if the string was not syntactically valid or an overflow or underflow occurred.</returns>
                /// <remarks>
                /// Format Supported:
                /// <list type="table">
                ///     <listheader>
                ///         Format <see cref="System.String"/>|||Example expected format|||Comments
                ///     </listheader>
                ///     <item>G/g(default)    </item>
                ///     <item>D/d                   32767</item>
                ///     <item>N/n                   32,767</item>
                ///     <item>X/x                   7fff  </item>
                /// </list>
                /// </remarks>
                public static bool TryParse(ReadOnlySpan<byte> source, out int value, out int bytesConsumed, char standardFormat = '\0')
                {
                    switch (standardFormat)
                    {
                        case '\0':
                        case 'D':
                        case 'G':
                        case 'd':
                        case 'g':
                            return TryParseInt32D(source, out value, out bytesConsumed);
                        case 'N':
                        case 'n':
                            return TryParseInt32N(source, out value, out bytesConsumed);
                        case 'X':
                        case 'x':
                            value = 0;
                            return TryParseUInt32X(source, out Unsafe.As<int, uint>(ref value), out bytesConsumed);
                        default:
                            return System.ThrowHelper.TryParseThrowFormatException<int>(out value, out bytesConsumed);
                    }
                }

                /// <summary>
                /// Parses a <see cref="System.Int16"/> at the start of a UTF-8 string.
                /// </summary>
                /// <param name="source">The UTF-8 string to parse.</param>
                /// <param name="value">When the method returns, contains the value parsed from source, if the parsing operation succeeded.</param>
                /// <param name="bytesConsumed">If the parsing operation was successful, contains the length in bytes of the parsed substring when the method returns. 
                /// <param name="standardFormat">The expected format of the UTF-8 string.</param>
                /// If the method fails, bytesConsumed is set to 0.</param>
                /// <returns><c>true</c> for success; 
                /// <c>false</c> if the string was not syntactically valid or an overflow or underflow occurred.</returns>
                /// <remarks>
                /// Format Supported:
                /// <list type="table">
                ///     <listheader>
                ///         Format <see cref="System.String"/>|||Example expected format|||Comments
                ///     </listheader>
                ///     <item>G/g(default)    </item>
                ///     <item>D/d                   32767</item>
                ///     <item>N/n                   32,767</item>
                ///     <item>X/x                   7fff  </item>
                /// </list>
                /// </remarks>
                public static bool TryParse(ReadOnlySpan<byte> source, out long value, out int bytesConsumed, char standardFormat = '\0')
                {
                    switch (standardFormat)
                    {
                        case '\0':
                        case 'D':
                        case 'G':
                        case 'd':
                        case 'g':
                            return TryParseInt64D(source, out value, out bytesConsumed);
                        case 'N':
                        case 'n':
                            return TryParseInt64N(source, out value, out bytesConsumed);
                        case 'X':
                        case 'x':
                            value = 0L;
                            return TryParseUInt64X(source, out Unsafe.As<long, ulong>(ref value), out bytesConsumed);
                        default:
                            return System.ThrowHelper.TryParseThrowFormatException<long>(out value, out bytesConsumed);
                    }
                }

                private static bool TryParseSByteD(ReadOnlySpan<byte> source, out sbyte value, out int bytesConsumed)
                {
                    int num;
                    int num2;
                    int num4;
                    int num3;
                    if (source.Length >= 1)
                    {
                        num = 1;
                        num2 = 0;
                        num3 = source[num2];
                        if (num3 == 45)
                        {
                            num = -1;
                            num2++;
                            if ((uint)num2 >= (uint)source.Length)
                            {
                                goto IL_0123;
                            }
                            num3 = source[num2];
                        }
                        else if (num3 == 43)
                        {
                            num2++;
                            if ((uint)num2 >= (uint)source.Length)
                            {
                                goto IL_0123;
                            }
                            num3 = source[num2];
                        }
                        num4 = 0;
                        if (ParserHelpers.IsDigit(num3))
                        {
                            if (num3 != 48)
                            {
                                goto IL_009c;
                            }
                            while (true)
                            {
                                num2++;
                                if ((uint)num2 >= (uint)source.Length)
                                {
                                    break;
                                }
                                num3 = source[num2];
                                if (num3 == 48)
                                {
                                    continue;
                                }
                                goto IL_0091;
                            }
                            goto IL_012b;
                        }
                    }
                    goto IL_0123;
                IL_0123:
                    bytesConsumed = 0;
                    value = 0;
                    return false;
                IL_0091:
                    if (ParserHelpers.IsDigit(num3))
                    {
                        goto IL_009c;
                    }
                    goto IL_012b;
                IL_012b:
                    bytesConsumed = num2;
                    value = (sbyte)(num4 * num);
                    return true;
                IL_009c:
                    num4 = num3 - 48;
                    num2++;
                    if ((uint)num2 < (uint)source.Length)
                    {
                        num3 = source[num2];
                        if (ParserHelpers.IsDigit(num3))
                        {
                            num2++;
                            num4 = 10 * num4 + num3 - 48;
                            if ((uint)num2 < (uint)source.Length)
                            {
                                num3 = source[num2];
                                if (ParserHelpers.IsDigit(num3))
                                {
                                    num2++;
                                    num4 = num4 * 10 + num3 - 48;
                                    if ((uint)num4 > 127L + (long)((-1 * num + 1) / 2) || ((uint)num2 < (uint)source.Length && ParserHelpers.IsDigit(source[num2])))
                                    {
                                        goto IL_0123;
                                    }
                                }
                            }
                        }
                    }
                    goto IL_012b;
                }

                private static bool TryParseInt16D(ReadOnlySpan<byte> source, out short value, out int bytesConsumed)
                {
                    int num;
                    int num2;
                    int num4;
                    int num3;
                    if (source.Length >= 1)
                    {
                        num = 1;
                        num2 = 0;
                        num3 = source[num2];
                        if (num3 == 45)
                        {
                            num = -1;
                            num2++;
                            if ((uint)num2 >= (uint)source.Length)
                            {
                                goto IL_0186;
                            }
                            num3 = source[num2];
                        }
                        else if (num3 == 43)
                        {
                            num2++;
                            if ((uint)num2 >= (uint)source.Length)
                            {
                                goto IL_0186;
                            }
                            num3 = source[num2];
                        }
                        num4 = 0;
                        if (ParserHelpers.IsDigit(num3))
                        {
                            if (num3 != 48)
                            {
                                goto IL_009c;
                            }
                            while (true)
                            {
                                num2++;
                                if ((uint)num2 >= (uint)source.Length)
                                {
                                    break;
                                }
                                num3 = source[num2];
                                if (num3 == 48)
                                {
                                    continue;
                                }
                                goto IL_0091;
                            }
                            goto IL_018e;
                        }
                    }
                    goto IL_0186;
                IL_0186:
                    bytesConsumed = 0;
                    value = 0;
                    return false;
                IL_0091:
                    if (ParserHelpers.IsDigit(num3))
                    {
                        goto IL_009c;
                    }
                    goto IL_018e;
                IL_018e:
                    bytesConsumed = num2;
                    value = (short)(num4 * num);
                    return true;
                IL_009c:
                    num4 = num3 - 48;
                    num2++;
                    if ((uint)num2 < (uint)source.Length)
                    {
                        num3 = source[num2];
                        if (ParserHelpers.IsDigit(num3))
                        {
                            num2++;
                            num4 = 10 * num4 + num3 - 48;
                            if ((uint)num2 < (uint)source.Length)
                            {
                                num3 = source[num2];
                                if (ParserHelpers.IsDigit(num3))
                                {
                                    num2++;
                                    num4 = 10 * num4 + num3 - 48;
                                    if ((uint)num2 < (uint)source.Length)
                                    {
                                        num3 = source[num2];
                                        if (ParserHelpers.IsDigit(num3))
                                        {
                                            num2++;
                                            num4 = 10 * num4 + num3 - 48;
                                            if ((uint)num2 < (uint)source.Length)
                                            {
                                                num3 = source[num2];
                                                if (ParserHelpers.IsDigit(num3))
                                                {
                                                    num2++;
                                                    num4 = num4 * 10 + num3 - 48;
                                                    if ((uint)num4 > 32767L + (long)((-1 * num + 1) / 2) || ((uint)num2 < (uint)source.Length && ParserHelpers.IsDigit(source[num2])))
                                                    {
                                                        goto IL_0186;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    goto IL_018e;
                }

                private static bool TryParseInt32D(ReadOnlySpan<byte> source, out int value, out int bytesConsumed)
                {
                    int num;
                    int num2;
                    int num4;
                    int num3;
                    if (source.Length >= 1)
                    {
                        num = 1;
                        num2 = 0;
                        num3 = source[num2];
                        if (num3 == 45)
                        {
                            num = -1;
                            num2++;
                            if ((uint)num2 >= (uint)source.Length)
                            {
                                goto IL_0281;
                            }
                            num3 = source[num2];
                        }
                        else if (num3 == 43)
                        {
                            num2++;
                            if ((uint)num2 >= (uint)source.Length)
                            {
                                goto IL_0281;
                            }
                            num3 = source[num2];
                        }
                        num4 = 0;
                        if (ParserHelpers.IsDigit(num3))
                        {
                            if (num3 != 48)
                            {
                                goto IL_009c;
                            }
                            while (true)
                            {
                                num2++;
                                if ((uint)num2 >= (uint)source.Length)
                                {
                                    break;
                                }
                                num3 = source[num2];
                                if (num3 == 48)
                                {
                                    continue;
                                }
                                goto IL_0091;
                            }
                            goto IL_0289;
                        }
                    }
                    goto IL_0281;
                IL_0281:
                    bytesConsumed = 0;
                    value = 0;
                    return false;
                IL_0091:
                    if (ParserHelpers.IsDigit(num3))
                    {
                        goto IL_009c;
                    }
                    goto IL_0289;
                IL_0289:
                    bytesConsumed = num2;
                    value = num4 * num;
                    return true;
                IL_009c:
                    num4 = num3 - 48;
                    num2++;
                    if ((uint)num2 < (uint)source.Length)
                    {
                        num3 = source[num2];
                        if (ParserHelpers.IsDigit(num3))
                        {
                            num2++;
                            num4 = 10 * num4 + num3 - 48;
                            if ((uint)num2 < (uint)source.Length)
                            {
                                num3 = source[num2];
                                if (ParserHelpers.IsDigit(num3))
                                {
                                    num2++;
                                    num4 = 10 * num4 + num3 - 48;
                                    if ((uint)num2 < (uint)source.Length)
                                    {
                                        num3 = source[num2];
                                        if (ParserHelpers.IsDigit(num3))
                                        {
                                            num2++;
                                            num4 = 10 * num4 + num3 - 48;
                                            if ((uint)num2 < (uint)source.Length)
                                            {
                                                num3 = source[num2];
                                                if (ParserHelpers.IsDigit(num3))
                                                {
                                                    num2++;
                                                    num4 = 10 * num4 + num3 - 48;
                                                    if ((uint)num2 < (uint)source.Length)
                                                    {
                                                        num3 = source[num2];
                                                        if (ParserHelpers.IsDigit(num3))
                                                        {
                                                            num2++;
                                                            num4 = 10 * num4 + num3 - 48;
                                                            if ((uint)num2 < (uint)source.Length)
                                                            {
                                                                num3 = source[num2];
                                                                if (ParserHelpers.IsDigit(num3))
                                                                {
                                                                    num2++;
                                                                    num4 = 10 * num4 + num3 - 48;
                                                                    if ((uint)num2 < (uint)source.Length)
                                                                    {
                                                                        num3 = source[num2];
                                                                        if (ParserHelpers.IsDigit(num3))
                                                                        {
                                                                            num2++;
                                                                            num4 = 10 * num4 + num3 - 48;
                                                                            if ((uint)num2 < (uint)source.Length)
                                                                            {
                                                                                num3 = source[num2];
                                                                                if (ParserHelpers.IsDigit(num3))
                                                                                {
                                                                                    num2++;
                                                                                    num4 = 10 * num4 + num3 - 48;
                                                                                    if ((uint)num2 < (uint)source.Length)
                                                                                    {
                                                                                        num3 = source[num2];
                                                                                        if (ParserHelpers.IsDigit(num3))
                                                                                        {
                                                                                            num2++;
                                                                                            if (num4 <= 214748364)
                                                                                            {
                                                                                                num4 = num4 * 10 + num3 - 48;
                                                                                                if ((uint)num4 <= 2147483647L + (long)((-1 * num + 1) / 2) && ((uint)num2 >= (uint)source.Length || !ParserHelpers.IsDigit(source[num2])))
                                                                                                {
                                                                                                    goto IL_0289;
                                                                                                }
                                                                                            }
                                                                                            goto IL_0281;
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    goto IL_0289;
                }

                private static bool TryParseInt64D(ReadOnlySpan<byte> source, out long value, out int bytesConsumed)
                {
                    if (source.Length < 1)
                    {
                        bytesConsumed = 0;
                        value = 0L;
                        return false;
                    }
                    int num = 0;
                    int num2 = 1;
                    if (source[0] == 45)
                    {
                        num = 1;
                        num2 = -1;
                        if (source.Length <= num)
                        {
                            bytesConsumed = 0;
                            value = 0L;
                            return false;
                        }
                    }
                    else if (source[0] == 43)
                    {
                        num = 1;
                        if (source.Length <= num)
                        {
                            bytesConsumed = 0;
                            value = 0L;
                            return false;
                        }
                    }
                    int num3 = 19 + num;
                    long num4 = source[num] - 48;
                    if (num4 < 0 || num4 > 9)
                    {
                        bytesConsumed = 0;
                        value = 0L;
                        return false;
                    }
                    ulong num5 = (ulong)num4;
                    if (source.Length < num3)
                    {
                        for (int i = num + 1; i < source.Length; i++)
                        {
                            long num6 = source[i] - 48;
                            if (num6 < 0 || num6 > 9)
                            {
                                bytesConsumed = i;
                                value = (long)num5 * (long)num2;
                                return true;
                            }
                            num5 = num5 * 10 + (ulong)num6;
                        }
                    }
                    else
                    {
                        for (int j = num + 1; j < num3 - 1; j++)
                        {
                            long num7 = source[j] - 48;
                            if (num7 < 0 || num7 > 9)
                            {
                                bytesConsumed = j;
                                value = (long)num5 * (long)num2;
                                return true;
                            }
                            num5 = num5 * 10 + (ulong)num7;
                        }
                        for (int k = num3 - 1; k < source.Length; k++)
                        {
                            long num8 = source[k] - 48;
                            if (num8 < 0 || num8 > 9)
                            {
                                bytesConsumed = k;
                                value = (long)num5 * (long)num2;
                                return true;
                            }
                            bool flag = num2 > 0;
                            bool flag2 = num8 > 8 || (flag && num8 > 7);
                            if (num5 > 922337203685477580L || (num5 == 922337203685477580L && flag2))
                            {
                                bytesConsumed = 0;
                                value = 0L;
                                return false;
                            }
                            num5 = num5 * 10 + (ulong)num8;
                        }
                    }
                    bytesConsumed = source.Length;
                    value = (long)num5 * (long)num2;
                    return true;
                }

                private static bool TryParseSByteN(ReadOnlySpan<byte> source, out sbyte value, out int bytesConsumed)
                {
                    int num;
                    int num2;
                    int num4;
                    int num3;
                    if (source.Length >= 1)
                    {
                        num = 1;
                        num2 = 0;
                        num3 = source[num2];
                        if (num3 == 45)
                        {
                            num = -1;
                            num2++;
                            if ((uint)num2 >= (uint)source.Length)
                            {
                                goto IL_00f9;
                            }
                            num3 = source[num2];
                        }
                        else if (num3 == 43)
                        {
                            num2++;
                            if ((uint)num2 >= (uint)source.Length)
                            {
                                goto IL_00f9;
                            }
                            num3 = source[num2];
                        }
                        if (num3 != 46)
                        {
                            if (ParserHelpers.IsDigit(num3))
                            {
                                num4 = num3 - 48;
                                while (true)
                                {
                                    num2++;
                                    if ((uint)num2 >= (uint)source.Length)
                                    {
                                        break;
                                    }
                                    num3 = source[num2];
                                    if (num3 == 44)
                                    {
                                        continue;
                                    }
                                    if (num3 == 46)
                                    {
                                        goto IL_00d4;
                                    }
                                    if (!ParserHelpers.IsDigit(num3))
                                    {
                                        break;
                                    }
                                    num4 = num4 * 10 + num3 - 48;
                                    if (num4 <= 127 + (-1 * num + 1) / 2)
                                    {
                                        continue;
                                    }
                                    goto IL_00f9;
                                }
                                goto IL_0101;
                            }
                        }
                        else
                        {
                            num4 = 0;
                            num2++;
                            if ((uint)num2 < (uint)source.Length && source[num2] == 48)
                            {
                                goto IL_00d4;
                            }
                        }
                    }
                    goto IL_00f9;
                IL_00f9:
                    bytesConsumed = 0;
                    value = 0;
                    return false;
                IL_0101:
                    bytesConsumed = num2;
                    value = (sbyte)(num4 * num);
                    return true;
                IL_00d4:
                    while (true)
                    {
                        num2++;
                        if ((uint)num2 >= (uint)source.Length)
                        {
                            break;
                        }
                        num3 = source[num2];
                        if (num3 == 48)
                        {
                            continue;
                        }
                        goto IL_00f1;
                    }
                    goto IL_0101;
                IL_00f1:
                    if (ParserHelpers.IsDigit(num3))
                    {
                        goto IL_00f9;
                    }
                    goto IL_0101;
                }

                private static bool TryParseInt16N(ReadOnlySpan<byte> source, out short value, out int bytesConsumed)
                {
                    int num;
                    int num2;
                    int num4;
                    int num3;
                    if (source.Length >= 1)
                    {
                        num = 1;
                        num2 = 0;
                        num3 = source[num2];
                        if (num3 == 45)
                        {
                            num = -1;
                            num2++;
                            if ((uint)num2 >= (uint)source.Length)
                            {
                                goto IL_00ff;
                            }
                            num3 = source[num2];
                        }
                        else if (num3 == 43)
                        {
                            num2++;
                            if ((uint)num2 >= (uint)source.Length)
                            {
                                goto IL_00ff;
                            }
                            num3 = source[num2];
                        }
                        if (num3 != 46)
                        {
                            if (ParserHelpers.IsDigit(num3))
                            {
                                num4 = num3 - 48;
                                while (true)
                                {
                                    num2++;
                                    if ((uint)num2 >= (uint)source.Length)
                                    {
                                        break;
                                    }
                                    num3 = source[num2];
                                    if (num3 == 44)
                                    {
                                        continue;
                                    }
                                    if (num3 == 46)
                                    {
                                        goto IL_00da;
                                    }
                                    if (!ParserHelpers.IsDigit(num3))
                                    {
                                        break;
                                    }
                                    num4 = num4 * 10 + num3 - 48;
                                    if (num4 <= 32767 + (-1 * num + 1) / 2)
                                    {
                                        continue;
                                    }
                                    goto IL_00ff;
                                }
                                goto IL_0107;
                            }
                        }
                        else
                        {
                            num4 = 0;
                            num2++;
                            if ((uint)num2 < (uint)source.Length && source[num2] == 48)
                            {
                                goto IL_00da;
                            }
                        }
                    }
                    goto IL_00ff;
                IL_00ff:
                    bytesConsumed = 0;
                    value = 0;
                    return false;
                IL_0107:
                    bytesConsumed = num2;
                    value = (short)(num4 * num);
                    return true;
                IL_00da:
                    while (true)
                    {
                        num2++;
                        if ((uint)num2 >= (uint)source.Length)
                        {
                            break;
                        }
                        num3 = source[num2];
                        if (num3 == 48)
                        {
                            continue;
                        }
                        goto IL_00f7;
                    }
                    goto IL_0107;
                IL_00f7:
                    if (ParserHelpers.IsDigit(num3))
                    {
                        goto IL_00ff;
                    }
                    goto IL_0107;
                }

                private static bool TryParseInt32N(ReadOnlySpan<byte> source, out int value, out int bytesConsumed)
                {
                    int num;
                    int num2;
                    int num4;
                    int num3;
                    if (source.Length >= 1)
                    {
                        num = 1;
                        num2 = 0;
                        num3 = source[num2];
                        if (num3 == 45)
                        {
                            num = -1;
                            num2++;
                            if ((uint)num2 >= (uint)source.Length)
                            {
                                goto IL_010a;
                            }
                            num3 = source[num2];
                        }
                        else if (num3 == 43)
                        {
                            num2++;
                            if ((uint)num2 >= (uint)source.Length)
                            {
                                goto IL_010a;
                            }
                            num3 = source[num2];
                        }
                        if (num3 != 46)
                        {
                            if (ParserHelpers.IsDigit(num3))
                            {
                                num4 = num3 - 48;
                                while (true)
                                {
                                    num2++;
                                    if ((uint)num2 >= (uint)source.Length)
                                    {
                                        break;
                                    }
                                    num3 = source[num2];
                                    if (num3 == 44)
                                    {
                                        continue;
                                    }
                                    if (num3 == 46)
                                    {
                                        goto IL_00e5;
                                    }
                                    if (!ParserHelpers.IsDigit(num3))
                                    {
                                        break;
                                    }
                                    if ((uint)num4 <= 214748364u)
                                    {
                                        num4 = num4 * 10 + num3 - 48;
                                        if ((uint)num4 <= 2147483647L + (long)((-1 * num + 1) / 2))
                                        {
                                            continue;
                                        }
                                    }
                                    goto IL_010a;
                                }
                                goto IL_0112;
                            }
                        }
                        else
                        {
                            num4 = 0;
                            num2++;
                            if ((uint)num2 < (uint)source.Length && source[num2] == 48)
                            {
                                goto IL_00e5;
                            }
                        }
                    }
                    goto IL_010a;
                IL_010a:
                    bytesConsumed = 0;
                    value = 0;
                    return false;
                IL_0112:
                    bytesConsumed = num2;
                    value = num4 * num;
                    return true;
                IL_00e5:
                    while (true)
                    {
                        num2++;
                        if ((uint)num2 >= (uint)source.Length)
                        {
                            break;
                        }
                        num3 = source[num2];
                        if (num3 == 48)
                        {
                            continue;
                        }
                        goto IL_0102;
                    }
                    goto IL_0112;
                IL_0102:
                    if (ParserHelpers.IsDigit(num3))
                    {
                        goto IL_010a;
                    }
                    goto IL_0112;
                }

                private static bool TryParseInt64N(ReadOnlySpan<byte> source, out long value, out int bytesConsumed)
                {
                    int num;
                    int num2;
                    long num4;
                    int num3;
                    if (source.Length >= 1)
                    {
                        num = 1;
                        num2 = 0;
                        num3 = source[num2];
                        if (num3 == 45)
                        {
                            num = -1;
                            num2++;
                            if ((uint)num2 >= (uint)source.Length)
                            {
                                goto IL_0115;
                            }
                            num3 = source[num2];
                        }
                        else if (num3 == 43)
                        {
                            num2++;
                            if ((uint)num2 >= (uint)source.Length)
                            {
                                goto IL_0115;
                            }
                            num3 = source[num2];
                        }
                        if (num3 != 46)
                        {
                            if (ParserHelpers.IsDigit(num3))
                            {
                                num4 = num3 - 48;
                                while (true)
                                {
                                    num2++;
                                    if ((uint)num2 >= (uint)source.Length)
                                    {
                                        break;
                                    }
                                    num3 = source[num2];
                                    if (num3 == 44)
                                    {
                                        continue;
                                    }
                                    if (num3 == 46)
                                    {
                                        goto IL_00f0;
                                    }
                                    if (!ParserHelpers.IsDigit(num3))
                                    {
                                        break;
                                    }
                                    if ((ulong)num4 <= 922337203685477580uL)
                                    {
                                        num4 = num4 * 10 + num3 - 48;
                                        if ((ulong)num4 <= (ulong)(long.MaxValue + (-1 * num + 1) / 2))
                                        {
                                            continue;
                                        }
                                    }
                                    goto IL_0115;
                                }
                                goto IL_011e;
                            }
                        }
                        else
                        {
                            num4 = 0L;
                            num2++;
                            if ((uint)num2 < (uint)source.Length && source[num2] == 48)
                            {
                                goto IL_00f0;
                            }
                        }
                    }
                    goto IL_0115;
                IL_0115:
                    bytesConsumed = 0;
                    value = 0L;
                    return false;
                IL_011e:
                    bytesConsumed = num2;
                    value = num4 * num;
                    return true;
                IL_00f0:
                    while (true)
                    {
                        num2++;
                        if ((uint)num2 >= (uint)source.Length)
                        {
                            break;
                        }
                        num3 = source[num2];
                        if (num3 == 48)
                        {
                            continue;
                        }
                        goto IL_010d;
                    }
                    goto IL_011e;
                IL_010d:
                    if (ParserHelpers.IsDigit(num3))
                    {
                        goto IL_0115;
                    }
                    goto IL_011e;
                }

                /// <summary>
                /// Parses a <see cref="System.Byte"/> at the start of a UTF-8 string.
                /// </summary>
                /// <param name="source">The UTF-8 string to parse.</param>
                /// <param name="value">When the method returns, contains the value parsed from source, if the parsing operation succeeded.</param>
                /// <param name="bytesConsumed">If the parsing operation was successful, contains the length in bytes of the parsed substring when the method returns. 
                /// <param name="standardFormat">The expected format of the UTF-8 string.</param>
                /// If the method fails, bytesConsumed is set to 0.</param>
                /// <returns><c>true</c> for success; 
                /// <c>false</c> if the string was not syntactically valid or an overflow or underflow occurred.</returns>
                /// <remarks>
                /// Format Supported:
                /// <list type="table">
                ///     <listheader>
                ///         Format <see cref="System.String"/>|||Example expected format
                ///     </listheader>
                ///     <item>G/g(default)    </item>
                ///     <item>D/d                   122</item>
                ///     <item>N/n                   122</item>
                ///     <item>X/x                   7a  </item>
                /// </list>
                /// </remarks>
                public static bool TryParse(ReadOnlySpan<byte> source, out byte value, out int bytesConsumed, char standardFormat = '\0')
                {
                    switch (standardFormat)
                    {
                        case '\0':
                        case 'D':
                        case 'G':
                        case 'd':
                        case 'g':
                            return TryParseByteD(source, out value, out bytesConsumed);
                        case 'N':
                        case 'n':
                            return TryParseByteN(source, out value, out bytesConsumed);
                        case 'X':
                        case 'x':
                            return TryParseByteX(source, out value, out bytesConsumed);
                        default:
                            return System.ThrowHelper.TryParseThrowFormatException<byte>(out value, out bytesConsumed);
                    }
                }

                /// <summary>
                /// Parses a <see cref="System.UInt16"/> at the start of a UTF-8 string.
                /// </summary>
                /// <param name="source">The UTF-8 string to parse.</param>
                /// <param name="value">When the method returns, contains the value parsed from source, if the parsing operation succeeded.</param>
                /// <param name="bytesConsumed">If the parsing operation was successful, contains the length in bytes of the parsed substring when the method returns. 
                /// <param name="standardFormat">The expected format of the UTF-8 string.</param>
                /// If the method fails, bytesConsumed is set to 0.</param>
                /// <returns><c>true</c> for success; 
                /// <c>false</c> if the string was not syntactically valid or an overflow or underflow occurred.</returns>
                /// <remarks>
                /// Format Supported:
                /// <list type="table">
                ///     <listheader>
                ///         Format <see cref="System.String"/>|||Example expected format|||Comments
                ///     </listheader>
                ///     <item>G/g(default)    </item>
                ///     <item>D/d                   32767</item>
                ///     <item>N/n                   32,767</item>
                ///     <item>X/x                   7fff  </item>
                /// </list>
                /// </remarks>
                [CLSCompliant(false)]
                public static bool TryParse(ReadOnlySpan<byte> source, out ushort value, out int bytesConsumed, char standardFormat = '\0')
                {
                    switch (standardFormat)
                    {
                        case '\0':
                        case 'D':
                        case 'G':
                        case 'd':
                        case 'g':
                            return TryParseUInt16D(source, out value, out bytesConsumed);
                        case 'N':
                        case 'n':
                            return TryParseUInt16N(source, out value, out bytesConsumed);
                        case 'X':
                        case 'x':
                            return TryParseUInt16X(source, out value, out bytesConsumed);
                        default:
                            return System.ThrowHelper.TryParseThrowFormatException<ushort>(out value, out bytesConsumed);
                    }
                }

                /// <summary>
                /// Parses a <see cref="System.UInt32"/> at the start of a UTF-8 string.
                /// </summary>
                /// <param name="source">The UTF-8 string to parse.</param>
                /// <param name="value">When the method returns, contains the value parsed from source, if the parsing operation succeeded.</param>
                /// <param name="bytesConsumed">If the parsing operation was successful, contains the length in bytes of the parsed substring when the method returns. 
                /// <param name="standardFormat">The expected format of the UTF-8 string.</param>
                /// If the method fails, bytesConsumed is set to 0.</param>
                /// <returns><c>true</c> for success; 
                /// <c>false</c> if the string was not syntactically valid or an overflow or underflow occurred.</returns>
                /// <remarks>
                /// Format Supported:
                /// <list type="table">
                ///     <listheader>
                ///         Format <see cref="System.String"/>|||Example expected format|||Comments
                ///     </listheader>
                ///     <item>G/g(default)    </item>
                ///     <item>D/d                   32767</item>
                ///     <item>N/n                   32,767</item>
                ///     <item>X/x                   7fff  </item>
                /// </list>
                /// </remarks>
                [CLSCompliant(false)]
                public static bool TryParse(ReadOnlySpan<byte> source, out uint value, out int bytesConsumed, char standardFormat = '\0')
                {
                    switch (standardFormat)
                    {
                        case '\0':
                        case 'D':
                        case 'G':
                        case 'd':
                        case 'g':
                            return TryParseUInt32D(source, out value, out bytesConsumed);
                        case 'N':
                        case 'n':
                            return TryParseUInt32N(source, out value, out bytesConsumed);
                        case 'X':
                        case 'x':
                            return TryParseUInt32X(source, out value, out bytesConsumed);
                        default:
                            return System.ThrowHelper.TryParseThrowFormatException<uint>(out value, out bytesConsumed);
                    }
                }

                /// <summary>
                /// Parses a <see cref="System.UInt64"/> at the start of a UTF-8 string.
                /// </summary>
                /// <param name="source">The UTF-8 string to parse.</param>
                /// <param name="value">When the method returns, contains the value parsed from source, if the parsing operation succeeded.</param>
                /// <param name="bytesConsumed">If the parsing operation was successful, contains the length in bytes of the parsed substring when the method returns. 
                /// <param name="standardFormat">The expected format of the UTF-8 string.</param>
                /// If the method fails, bytesConsumed is set to 0.</param>
                /// <returns><c>true</c> for success; 
                /// <c>false</c> if the string was not syntactically valid or an overflow or underflow occurred.</returns>
                /// <remarks>
                /// Format Supported:
                /// <list type="table">
                ///     <listheader>
                ///         Format <see cref="System.String"/>|||Example expected format|||Comments
                ///     </listheader>
                ///     <item>G/g(default)    </item>
                ///     <item>D/d                   32767</item>
                ///     <item>N/n                   32,767</item>
                ///     <item>X/x                   7fff  </item>
                /// </list>
                /// </remarks>
                [CLSCompliant(false)]
                public static bool TryParse(ReadOnlySpan<byte> source, out ulong value, out int bytesConsumed, char standardFormat = '\0')
                {
                    switch (standardFormat)
                    {
                        case '\0':
                        case 'D':
                        case 'G':
                        case 'd':
                        case 'g':
                            return TryParseUInt64D(source, out value, out bytesConsumed);
                        case 'N':
                        case 'n':
                            return TryParseUInt64N(source, out value, out bytesConsumed);
                        case 'X':
                        case 'x':
                            return TryParseUInt64X(source, out value, out bytesConsumed);
                        default:
                            return System.ThrowHelper.TryParseThrowFormatException<ulong>(out value, out bytesConsumed);
                    }
                }

                private static bool TryParseByteD(ReadOnlySpan<byte> source, out byte value, out int bytesConsumed)
                {
                    int num;
                    int num3;
                    int num2;
                    if (source.Length >= 1)
                    {
                        num = 0;
                        num2 = source[num];
                        num3 = 0;
                        if (ParserHelpers.IsDigit(num2))
                        {
                            if (num2 != 48)
                            {
                                goto IL_0056;
                            }
                            while (true)
                            {
                                num++;
                                if ((uint)num >= (uint)source.Length)
                                {
                                    break;
                                }
                                num2 = source[num];
                                if (num2 == 48)
                                {
                                    continue;
                                }
                                goto IL_004b;
                            }
                            goto IL_00dd;
                        }
                    }
                    goto IL_00d5;
                IL_004b:
                    if (ParserHelpers.IsDigit(num2))
                    {
                        goto IL_0056;
                    }
                    goto IL_00dd;
                IL_0056:
                    num3 = num2 - 48;
                    num++;
                    if ((uint)num < (uint)source.Length)
                    {
                        num2 = source[num];
                        if (ParserHelpers.IsDigit(num2))
                        {
                            num++;
                            num3 = 10 * num3 + num2 - 48;
                            if ((uint)num < (uint)source.Length)
                            {
                                num2 = source[num];
                                if (ParserHelpers.IsDigit(num2))
                                {
                                    num++;
                                    num3 = num3 * 10 + num2 - 48;
                                    if ((uint)num3 > 255u || ((uint)num < (uint)source.Length && ParserHelpers.IsDigit(source[num])))
                                    {
                                        goto IL_00d5;
                                    }
                                }
                            }
                        }
                    }
                    goto IL_00dd;
                IL_00dd:
                    bytesConsumed = num;
                    value = (byte)num3;
                    return true;
                IL_00d5:
                    bytesConsumed = 0;
                    value = 0;
                    return false;
                }

                private static bool TryParseUInt16D(ReadOnlySpan<byte> source, out ushort value, out int bytesConsumed)
                {
                    int num;
                    int num3;
                    int num2;
                    if (source.Length >= 1)
                    {
                        num = 0;
                        num2 = source[num];
                        num3 = 0;
                        if (ParserHelpers.IsDigit(num2))
                        {
                            if (num2 != 48)
                            {
                                goto IL_0056;
                            }
                            while (true)
                            {
                                num++;
                                if ((uint)num >= (uint)source.Length)
                                {
                                    break;
                                }
                                num2 = source[num];
                                if (num2 == 48)
                                {
                                    continue;
                                }
                                goto IL_004b;
                            }
                            goto IL_013d;
                        }
                    }
                    goto IL_0135;
                IL_004b:
                    if (ParserHelpers.IsDigit(num2))
                    {
                        goto IL_0056;
                    }
                    goto IL_013d;
                IL_0056:
                    num3 = num2 - 48;
                    num++;
                    if ((uint)num < (uint)source.Length)
                    {
                        num2 = source[num];
                        if (ParserHelpers.IsDigit(num2))
                        {
                            num++;
                            num3 = 10 * num3 + num2 - 48;
                            if ((uint)num < (uint)source.Length)
                            {
                                num2 = source[num];
                                if (ParserHelpers.IsDigit(num2))
                                {
                                    num++;
                                    num3 = 10 * num3 + num2 - 48;
                                    if ((uint)num < (uint)source.Length)
                                    {
                                        num2 = source[num];
                                        if (ParserHelpers.IsDigit(num2))
                                        {
                                            num++;
                                            num3 = 10 * num3 + num2 - 48;
                                            if ((uint)num < (uint)source.Length)
                                            {
                                                num2 = source[num];
                                                if (ParserHelpers.IsDigit(num2))
                                                {
                                                    num++;
                                                    num3 = num3 * 10 + num2 - 48;
                                                    if ((uint)num3 > 65535u || ((uint)num < (uint)source.Length && ParserHelpers.IsDigit(source[num])))
                                                    {
                                                        goto IL_0135;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    goto IL_013d;
                IL_013d:
                    bytesConsumed = num;
                    value = (ushort)num3;
                    return true;
                IL_0135:
                    bytesConsumed = 0;
                    value = 0;
                    return false;
                }

                private static bool TryParseUInt32D(ReadOnlySpan<byte> source, out uint value, out int bytesConsumed)
                {
                    int num;
                    int num3;
                    int num2;
                    if (source.Length >= 1)
                    {
                        num = 0;
                        num2 = source[num];
                        num3 = 0;
                        if (ParserHelpers.IsDigit(num2))
                        {
                            if (num2 != 48)
                            {
                                goto IL_0056;
                            }
                            while (true)
                            {
                                num++;
                                if ((uint)num >= (uint)source.Length)
                                {
                                    break;
                                }
                                num2 = source[num];
                                if (num2 == 48)
                                {
                                    continue;
                                }
                                goto IL_004b;
                            }
                            goto IL_023d;
                        }
                    }
                    goto IL_0235;
                IL_004b:
                    if (ParserHelpers.IsDigit(num2))
                    {
                        goto IL_0056;
                    }
                    goto IL_023d;
                IL_0056:
                    num3 = num2 - 48;
                    num++;
                    if ((uint)num < (uint)source.Length)
                    {
                        num2 = source[num];
                        if (ParserHelpers.IsDigit(num2))
                        {
                            num++;
                            num3 = 10 * num3 + num2 - 48;
                            if ((uint)num < (uint)source.Length)
                            {
                                num2 = source[num];
                                if (ParserHelpers.IsDigit(num2))
                                {
                                    num++;
                                    num3 = 10 * num3 + num2 - 48;
                                    if ((uint)num < (uint)source.Length)
                                    {
                                        num2 = source[num];
                                        if (ParserHelpers.IsDigit(num2))
                                        {
                                            num++;
                                            num3 = 10 * num3 + num2 - 48;
                                            if ((uint)num < (uint)source.Length)
                                            {
                                                num2 = source[num];
                                                if (ParserHelpers.IsDigit(num2))
                                                {
                                                    num++;
                                                    num3 = 10 * num3 + num2 - 48;
                                                    if ((uint)num < (uint)source.Length)
                                                    {
                                                        num2 = source[num];
                                                        if (ParserHelpers.IsDigit(num2))
                                                        {
                                                            num++;
                                                            num3 = 10 * num3 + num2 - 48;
                                                            if ((uint)num < (uint)source.Length)
                                                            {
                                                                num2 = source[num];
                                                                if (ParserHelpers.IsDigit(num2))
                                                                {
                                                                    num++;
                                                                    num3 = 10 * num3 + num2 - 48;
                                                                    if ((uint)num < (uint)source.Length)
                                                                    {
                                                                        num2 = source[num];
                                                                        if (ParserHelpers.IsDigit(num2))
                                                                        {
                                                                            num++;
                                                                            num3 = 10 * num3 + num2 - 48;
                                                                            if ((uint)num < (uint)source.Length)
                                                                            {
                                                                                num2 = source[num];
                                                                                if (ParserHelpers.IsDigit(num2))
                                                                                {
                                                                                    num++;
                                                                                    num3 = 10 * num3 + num2 - 48;
                                                                                    if ((uint)num < (uint)source.Length)
                                                                                    {
                                                                                        num2 = source[num];
                                                                                        if (ParserHelpers.IsDigit(num2))
                                                                                        {
                                                                                            num++;
                                                                                            if ((uint)num3 <= 429496729u && (num3 != 429496729 || num2 <= 53))
                                                                                            {
                                                                                                num3 = num3 * 10 + num2 - 48;
                                                                                                if ((uint)num >= (uint)source.Length || !ParserHelpers.IsDigit(source[num]))
                                                                                                {
                                                                                                    goto IL_023d;
                                                                                                }
                                                                                            }
                                                                                            goto IL_0235;
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    goto IL_023d;
                IL_023d:
                    bytesConsumed = num;
                    value = (uint)num3;
                    return true;
                IL_0235:
                    bytesConsumed = 0;
                    value = 0u;
                    return false;
                }

                private static bool TryParseUInt64D(ReadOnlySpan<byte> source, out ulong value, out int bytesConsumed)
                {
                    if (source.Length < 1)
                    {
                        bytesConsumed = 0;
                        value = 0uL;
                        return false;
                    }
                    ulong num = (uint)(source[0] - 48);
                    if (num > 9)
                    {
                        bytesConsumed = 0;
                        value = 0uL;
                        return false;
                    }
                    ulong num2 = num;
                    if (source.Length < 19)
                    {
                        for (int i = 1; i < source.Length; i++)
                        {
                            ulong num3 = (uint)(source[i] - 48);
                            if (num3 > 9)
                            {
                                bytesConsumed = i;
                                value = num2;
                                return true;
                            }
                            num2 = num2 * 10 + num3;
                        }
                    }
                    else
                    {
                        for (int j = 1; j < 18; j++)
                        {
                            ulong num4 = (uint)(source[j] - 48);
                            if (num4 > 9)
                            {
                                bytesConsumed = j;
                                value = num2;
                                return true;
                            }
                            num2 = num2 * 10 + num4;
                        }
                        for (int k = 18; k < source.Length; k++)
                        {
                            ulong num5 = (uint)(source[k] - 48);
                            if (num5 > 9)
                            {
                                bytesConsumed = k;
                                value = num2;
                                return true;
                            }
                            if (num2 > 1844674407370955161L || (num2 == 1844674407370955161L && num5 > 5))
                            {
                                bytesConsumed = 0;
                                value = 0uL;
                                return false;
                            }
                            num2 = num2 * 10 + num5;
                        }
                    }
                    bytesConsumed = source.Length;
                    value = num2;
                    return true;
                }

                private static bool TryParseByteN(ReadOnlySpan<byte> source, out byte value, out int bytesConsumed)
                {
                    int num;
                    int num3;
                    int num2;
                    if (source.Length >= 1)
                    {
                        num = 0;
                        num2 = source[num];
                        if (num2 == 43)
                        {
                            num++;
                            if ((uint)num >= (uint)source.Length)
                            {
                                goto IL_00ce;
                            }
                            num2 = source[num];
                        }
                        if (num2 != 46)
                        {
                            if (ParserHelpers.IsDigit(num2))
                            {
                                num3 = num2 - 48;
                                while (true)
                                {
                                    num++;
                                    if ((uint)num >= (uint)source.Length)
                                    {
                                        break;
                                    }
                                    num2 = source[num];
                                    if (num2 == 44)
                                    {
                                        continue;
                                    }
                                    if (num2 == 46)
                                    {
                                        goto IL_00a9;
                                    }
                                    if (!ParserHelpers.IsDigit(num2))
                                    {
                                        break;
                                    }
                                    num3 = num3 * 10 + num2 - 48;
                                    if (num3 <= 255)
                                    {
                                        continue;
                                    }
                                    goto IL_00ce;
                                }
                                goto IL_00d6;
                            }
                        }
                        else
                        {
                            num3 = 0;
                            num++;
                            if ((uint)num < (uint)source.Length && source[num] == 48)
                            {
                                goto IL_00a9;
                            }
                        }
                    }
                    goto IL_00ce;
                IL_00c6:
                    if (ParserHelpers.IsDigit(num2))
                    {
                        goto IL_00ce;
                    }
                    goto IL_00d6;
                IL_00ce:
                    bytesConsumed = 0;
                    value = 0;
                    return false;
                IL_00d6:
                    bytesConsumed = num;
                    value = (byte)num3;
                    return true;
                IL_00a9:
                    while (true)
                    {
                        num++;
                        if ((uint)num >= (uint)source.Length)
                        {
                            break;
                        }
                        num2 = source[num];
                        if (num2 == 48)
                        {
                            continue;
                        }
                        goto IL_00c6;
                    }
                    goto IL_00d6;
                }

                private static bool TryParseUInt16N(ReadOnlySpan<byte> source, out ushort value, out int bytesConsumed)
                {
                    int num;
                    int num3;
                    int num2;
                    if (source.Length >= 1)
                    {
                        num = 0;
                        num2 = source[num];
                        if (num2 == 43)
                        {
                            num++;
                            if ((uint)num >= (uint)source.Length)
                            {
                                goto IL_00ce;
                            }
                            num2 = source[num];
                        }
                        if (num2 != 46)
                        {
                            if (ParserHelpers.IsDigit(num2))
                            {
                                num3 = num2 - 48;
                                while (true)
                                {
                                    num++;
                                    if ((uint)num >= (uint)source.Length)
                                    {
                                        break;
                                    }
                                    num2 = source[num];
                                    if (num2 == 44)
                                    {
                                        continue;
                                    }
                                    if (num2 == 46)
                                    {
                                        goto IL_00a9;
                                    }
                                    if (!ParserHelpers.IsDigit(num2))
                                    {
                                        break;
                                    }
                                    num3 = num3 * 10 + num2 - 48;
                                    if (num3 <= 65535)
                                    {
                                        continue;
                                    }
                                    goto IL_00ce;
                                }
                                goto IL_00d6;
                            }
                        }
                        else
                        {
                            num3 = 0;
                            num++;
                            if ((uint)num < (uint)source.Length && source[num] == 48)
                            {
                                goto IL_00a9;
                            }
                        }
                    }
                    goto IL_00ce;
                IL_00c6:
                    if (ParserHelpers.IsDigit(num2))
                    {
                        goto IL_00ce;
                    }
                    goto IL_00d6;
                IL_00ce:
                    bytesConsumed = 0;
                    value = 0;
                    return false;
                IL_00d6:
                    bytesConsumed = num;
                    value = (ushort)num3;
                    return true;
                IL_00a9:
                    while (true)
                    {
                        num++;
                        if ((uint)num >= (uint)source.Length)
                        {
                            break;
                        }
                        num2 = source[num];
                        if (num2 == 48)
                        {
                            continue;
                        }
                        goto IL_00c6;
                    }
                    goto IL_00d6;
                }

                private static bool TryParseUInt32N(ReadOnlySpan<byte> source, out uint value, out int bytesConsumed)
                {
                    int num;
                    int num3;
                    int num2;
                    if (source.Length >= 1)
                    {
                        num = 0;
                        num2 = source[num];
                        if (num2 == 43)
                        {
                            num++;
                            if ((uint)num >= (uint)source.Length)
                            {
                                goto IL_00de;
                            }
                            num2 = source[num];
                        }
                        if (num2 != 46)
                        {
                            if (ParserHelpers.IsDigit(num2))
                            {
                                num3 = num2 - 48;
                                while (true)
                                {
                                    num++;
                                    if ((uint)num >= (uint)source.Length)
                                    {
                                        break;
                                    }
                                    num2 = source[num];
                                    if (num2 == 44)
                                    {
                                        continue;
                                    }
                                    if (num2 == 46)
                                    {
                                        goto IL_00b9;
                                    }
                                    if (!ParserHelpers.IsDigit(num2))
                                    {
                                        break;
                                    }
                                    if ((uint)num3 <= 429496729u && (num3 != 429496729 || num2 <= 53))
                                    {
                                        num3 = num3 * 10 + num2 - 48;
                                        continue;
                                    }
                                    goto IL_00de;
                                }
                                goto IL_00e6;
                            }
                        }
                        else
                        {
                            num3 = 0;
                            num++;
                            if ((uint)num < (uint)source.Length && source[num] == 48)
                            {
                                goto IL_00b9;
                            }
                        }
                    }
                    goto IL_00de;
                IL_00d6:
                    if (ParserHelpers.IsDigit(num2))
                    {
                        goto IL_00de;
                    }
                    goto IL_00e6;
                IL_00de:
                    bytesConsumed = 0;
                    value = 0u;
                    return false;
                IL_00e6:
                    bytesConsumed = num;
                    value = (uint)num3;
                    return true;
                IL_00b9:
                    while (true)
                    {
                        num++;
                        if ((uint)num >= (uint)source.Length)
                        {
                            break;
                        }
                        num2 = source[num];
                        if (num2 == 48)
                        {
                            continue;
                        }
                        goto IL_00d6;
                    }
                    goto IL_00e6;
                }

                private static bool TryParseUInt64N(ReadOnlySpan<byte> source, out ulong value, out int bytesConsumed)
                {
                    int num;
                    long num3;
                    int num2;
                    if (source.Length >= 1)
                    {
                        num = 0;
                        num2 = source[num];
                        if (num2 == 43)
                        {
                            num++;
                            if ((uint)num >= (uint)source.Length)
                            {
                                goto IL_00eb;
                            }
                            num2 = source[num];
                        }
                        if (num2 != 46)
                        {
                            if (ParserHelpers.IsDigit(num2))
                            {
                                num3 = num2 - 48;
                                while (true)
                                {
                                    num++;
                                    if ((uint)num >= (uint)source.Length)
                                    {
                                        break;
                                    }
                                    num2 = source[num];
                                    if (num2 == 44)
                                    {
                                        continue;
                                    }
                                    if (num2 == 46)
                                    {
                                        goto IL_00c6;
                                    }
                                    if (!ParserHelpers.IsDigit(num2))
                                    {
                                        break;
                                    }
                                    if ((ulong)num3 <= 1844674407370955161uL && (num3 != 1844674407370955161L || num2 <= 53))
                                    {
                                        num3 = num3 * 10 + num2 - 48;
                                        continue;
                                    }
                                    goto IL_00eb;
                                }
                                goto IL_00f4;
                            }
                        }
                        else
                        {
                            num3 = 0L;
                            num++;
                            if ((uint)num < (uint)source.Length && source[num] == 48)
                            {
                                goto IL_00c6;
                            }
                        }
                    }
                    goto IL_00eb;
                IL_00e3:
                    if (ParserHelpers.IsDigit(num2))
                    {
                        goto IL_00eb;
                    }
                    goto IL_00f4;
                IL_00eb:
                    bytesConsumed = 0;
                    value = 0uL;
                    return false;
                IL_00f4:
                    bytesConsumed = num;
                    value = (ulong)num3;
                    return true;
                IL_00c6:
                    while (true)
                    {
                        num++;
                        if ((uint)num >= (uint)source.Length)
                        {
                            break;
                        }
                        num2 = source[num];
                        if (num2 == 48)
                        {
                            continue;
                        }
                        goto IL_00e3;
                    }
                    goto IL_00f4;
                }

                private static bool TryParseByteX(ReadOnlySpan<byte> source, out byte value, out int bytesConsumed)
                {
                    if (source.Length < 1)
                    {
                        bytesConsumed = 0;
                        value = 0;
                        return false;
                    }
                    byte[] s_hexLookup = ParserHelpers.s_hexLookup;
                    byte b = source[0];
                    byte b2 = s_hexLookup[b];
                    if (b2 == byte.MaxValue)
                    {
                        bytesConsumed = 0;
                        value = 0;
                        return false;
                    }
                    uint num = b2;
                    if (source.Length <= 2)
                    {
                        for (int i = 1; i < source.Length; i++)
                        {
                            b = source[i];
                            b2 = s_hexLookup[b];
                            if (b2 == byte.MaxValue)
                            {
                                bytesConsumed = i;
                                value = (byte)num;
                                return true;
                            }
                            num = (num << 4) + b2;
                        }
                    }
                    else
                    {
                        for (int j = 1; j < 2; j++)
                        {
                            b = source[j];
                            b2 = s_hexLookup[b];
                            if (b2 == byte.MaxValue)
                            {
                                bytesConsumed = j;
                                value = (byte)num;
                                return true;
                            }
                            num = (num << 4) + b2;
                        }
                        for (int k = 2; k < source.Length; k++)
                        {
                            b = source[k];
                            b2 = s_hexLookup[b];
                            if (b2 == byte.MaxValue)
                            {
                                bytesConsumed = k;
                                value = (byte)num;
                                return true;
                            }
                            if (num > 15)
                            {
                                bytesConsumed = 0;
                                value = 0;
                                return false;
                            }
                            num = (num << 4) + b2;
                        }
                    }
                    bytesConsumed = source.Length;
                    value = (byte)num;
                    return true;
                }

                private static bool TryParseUInt16X(ReadOnlySpan<byte> source, out ushort value, out int bytesConsumed)
                {
                    if (source.Length < 1)
                    {
                        bytesConsumed = 0;
                        value = 0;
                        return false;
                    }
                    byte[] s_hexLookup = ParserHelpers.s_hexLookup;
                    byte b = source[0];
                    byte b2 = s_hexLookup[b];
                    if (b2 == byte.MaxValue)
                    {
                        bytesConsumed = 0;
                        value = 0;
                        return false;
                    }
                    uint num = b2;
                    if (source.Length <= 4)
                    {
                        for (int i = 1; i < source.Length; i++)
                        {
                            b = source[i];
                            b2 = s_hexLookup[b];
                            if (b2 == byte.MaxValue)
                            {
                                bytesConsumed = i;
                                value = (ushort)num;
                                return true;
                            }
                            num = (num << 4) + b2;
                        }
                    }
                    else
                    {
                        for (int j = 1; j < 4; j++)
                        {
                            b = source[j];
                            b2 = s_hexLookup[b];
                            if (b2 == byte.MaxValue)
                            {
                                bytesConsumed = j;
                                value = (ushort)num;
                                return true;
                            }
                            num = (num << 4) + b2;
                        }
                        for (int k = 4; k < source.Length; k++)
                        {
                            b = source[k];
                            b2 = s_hexLookup[b];
                            if (b2 == byte.MaxValue)
                            {
                                bytesConsumed = k;
                                value = (ushort)num;
                                return true;
                            }
                            if (num > 4095)
                            {
                                bytesConsumed = 0;
                                value = 0;
                                return false;
                            }
                            num = (num << 4) + b2;
                        }
                    }
                    bytesConsumed = source.Length;
                    value = (ushort)num;
                    return true;
                }

                private static bool TryParseUInt32X(ReadOnlySpan<byte> source, out uint value, out int bytesConsumed)
                {
                    if (source.Length < 1)
                    {
                        bytesConsumed = 0;
                        value = 0u;
                        return false;
                    }
                    byte[] s_hexLookup = ParserHelpers.s_hexLookup;
                    byte b = source[0];
                    byte b2 = s_hexLookup[b];
                    if (b2 == byte.MaxValue)
                    {
                        bytesConsumed = 0;
                        value = 0u;
                        return false;
                    }
                    uint num = b2;
                    if (source.Length <= 8)
                    {
                        for (int i = 1; i < source.Length; i++)
                        {
                            b = source[i];
                            b2 = s_hexLookup[b];
                            if (b2 == byte.MaxValue)
                            {
                                bytesConsumed = i;
                                value = num;
                                return true;
                            }
                            num = (num << 4) + b2;
                        }
                    }
                    else
                    {
                        for (int j = 1; j < 8; j++)
                        {
                            b = source[j];
                            b2 = s_hexLookup[b];
                            if (b2 == byte.MaxValue)
                            {
                                bytesConsumed = j;
                                value = num;
                                return true;
                            }
                            num = (num << 4) + b2;
                        }
                        for (int k = 8; k < source.Length; k++)
                        {
                            b = source[k];
                            b2 = s_hexLookup[b];
                            if (b2 == byte.MaxValue)
                            {
                                bytesConsumed = k;
                                value = num;
                                return true;
                            }
                            if (num > 268435455)
                            {
                                bytesConsumed = 0;
                                value = 0u;
                                return false;
                            }
                            num = (num << 4) + b2;
                        }
                    }
                    bytesConsumed = source.Length;
                    value = num;
                    return true;
                }

                private static bool TryParseUInt64X(ReadOnlySpan<byte> source, out ulong value, out int bytesConsumed)
                {
                    if (source.Length < 1)
                    {
                        bytesConsumed = 0;
                        value = 0uL;
                        return false;
                    }
                    byte[] s_hexLookup = ParserHelpers.s_hexLookup;
                    byte b = source[0];
                    byte b2 = s_hexLookup[b];
                    if (b2 == byte.MaxValue)
                    {
                        bytesConsumed = 0;
                        value = 0uL;
                        return false;
                    }
                    ulong num = b2;
                    if (source.Length <= 16)
                    {
                        for (int i = 1; i < source.Length; i++)
                        {
                            b = source[i];
                            b2 = s_hexLookup[b];
                            if (b2 == byte.MaxValue)
                            {
                                bytesConsumed = i;
                                value = num;
                                return true;
                            }
                            num = (num << 4) + b2;
                        }
                    }
                    else
                    {
                        for (int j = 1; j < 16; j++)
                        {
                            b = source[j];
                            b2 = s_hexLookup[b];
                            if (b2 == byte.MaxValue)
                            {
                                bytesConsumed = j;
                                value = num;
                                return true;
                            }
                            num = (num << 4) + b2;
                        }
                        for (int k = 16; k < source.Length; k++)
                        {
                            b = source[k];
                            b2 = s_hexLookup[b];
                            if (b2 == byte.MaxValue)
                            {
                                bytesConsumed = k;
                                value = num;
                                return true;
                            }
                            if (num > 1152921504606846975L)
                            {
                                bytesConsumed = 0;
                                value = 0uL;
                                return false;
                            }
                            num = (num << 4) + b2;
                        }
                    }
                    bytesConsumed = source.Length;
                    value = num;
                    return true;
                }

                private static bool TryParseNumber(ReadOnlySpan<byte> source, ref NumberBuffer number, out int bytesConsumed, ParseNumberOptions options, out bool textUsedExponentNotation)
                {
                    textUsedExponentNotation = false;
                    if (source.Length == 0)
                    {
                        bytesConsumed = 0;
                        return false;
                    }
                    Span<byte> digits = number.Digits;
                    int i = 0;
                    int num = 0;
                    byte b = source[i];
                    if (b != 43)
                    {
                        if (b != 45)
                        {
                            goto IL_0055;
                        }
                        number.IsNegative = true;
                    }
                    i++;
                    if (i == source.Length)
                    {
                        bytesConsumed = 0;
                        return false;
                    }
                    b = source[i];
                    goto IL_0055;
                IL_0055:
                    int num2 = i;
                    for (; i != source.Length; i++)
                    {
                        b = source[i];
                        if (b != 48)
                        {
                            break;
                        }
                    }
                    if (i == source.Length)
                    {
                        digits[0] = 0;
                        number.Scale = 0;
                        bytesConsumed = i;
                        return true;
                    }
                    int num3 = i;
                    for (; i != source.Length; i++)
                    {
                        b = source[i];
                        if ((uint)(b - 48) > 9u)
                        {
                            break;
                        }
                    }
                    int num4 = i - num2;
                    int num5 = i - num3;
                    int num6 = Math.Min(num5, 50);
                    source.Slice(num3, num6).CopyTo(digits);
                    num = num6;
                    number.Scale = num5;
                    if (i == source.Length)
                    {
                        bytesConsumed = i;
                        return true;
                    }
                    int num7 = 0;
                    if (b == 46)
                    {
                        i++;
                        int num8 = i;
                        for (; i != source.Length; i++)
                        {
                            b = source[i];
                            if ((uint)(b - 48) > 9u)
                            {
                                break;
                            }
                        }
                        num7 = i - num8;
                        int j = num8;
                        if (num == 0)
                        {
                            for (; j < i && source[j] == 48; j++)
                            {
                                number.Scale--;
                            }
                        }
                        int num9 = Math.Min(i - j, 51 - num - 1);
                        source.Slice(j, num9).CopyTo(digits.Slice(num));
                        num += num9;
                        if (i == source.Length)
                        {
                            if (num4 == 0 && num7 == 0)
                            {
                                bytesConsumed = 0;
                                return false;
                            }
                            bytesConsumed = i;
                            return true;
                        }
                    }
                    if (num4 == 0 && num7 == 0)
                    {
                        bytesConsumed = 0;
                        return false;
                    }
                    if ((b & -33) != 69)
                    {
                        bytesConsumed = i;
                        return true;
                    }
                    textUsedExponentNotation = true;
                    i++;
                    if ((options & ParseNumberOptions.AllowExponent) == 0)
                    {
                        bytesConsumed = 0;
                        return false;
                    }
                    if (i == source.Length)
                    {
                        bytesConsumed = 0;
                        return false;
                    }
                    bool flag = false;
                    b = source[i];
                    if (b != 43)
                    {
                        if (b != 45)
                        {
                            goto IL_0229;
                        }
                        flag = true;
                    }
                    i++;
                    if (i == source.Length)
                    {
                        bytesConsumed = 0;
                        return false;
                    }
                    b = source[i];
                    goto IL_0229;
                IL_0229:
                    if (!TryParseUInt32D(source.Slice(i), out var value, out var bytesConsumed2))
                    {
                        bytesConsumed = 0;
                        return false;
                    }
                    i += bytesConsumed2;
                    if (flag)
                    {
                        if (number.Scale < int.MinValue + value)
                        {
                            number.Scale = int.MinValue;
                        }
                        else
                        {
                            number.Scale -= (int)value;
                        }
                    }
                    else
                    {
                        if (number.Scale > 2147483647L - (long)value)
                        {
                            bytesConsumed = 0;
                            return false;
                        }
                        number.Scale += (int)value;
                    }
                    bytesConsumed = i;
                    return true;
                }

                private static bool TryParseTimeSpanBigG(ReadOnlySpan<byte> source, out TimeSpan value, out int bytesConsumed)
                {
                    int i = 0;
                    byte b = 0;
                    for (; i != source.Length; i++)
                    {
                        b = source[i];
                        if (b != 32 && b != 9)
                        {
                            break;
                        }
                    }
                    if (i == source.Length)
                    {
                        value = default(TimeSpan);
                        bytesConsumed = 0;
                        return false;
                    }
                    bool isNegative = false;
                    if (b == 45)
                    {
                        isNegative = true;
                        i++;
                        if (i == source.Length)
                        {
                            value = default(TimeSpan);
                            bytesConsumed = 0;
                            return false;
                        }
                    }
                    if (!TryParseUInt32D(source.Slice(i), out var value2, out var bytesConsumed2))
                    {
                        value = default(TimeSpan);
                        bytesConsumed = 0;
                        return false;
                    }
                    i += bytesConsumed2;
                    if (i == source.Length || source[i++] != 58)
                    {
                        value = default(TimeSpan);
                        bytesConsumed = 0;
                        return false;
                    }
                    if (!TryParseUInt32D(source.Slice(i), out var value3, out bytesConsumed2))
                    {
                        value = default(TimeSpan);
                        bytesConsumed = 0;
                        return false;
                    }
                    i += bytesConsumed2;
                    if (i == source.Length || source[i++] != 58)
                    {
                        value = default(TimeSpan);
                        bytesConsumed = 0;
                        return false;
                    }
                    if (!TryParseUInt32D(source.Slice(i), out var value4, out bytesConsumed2))
                    {
                        value = default(TimeSpan);
                        bytesConsumed = 0;
                        return false;
                    }
                    i += bytesConsumed2;
                    if (i == source.Length || source[i++] != 58)
                    {
                        value = default(TimeSpan);
                        bytesConsumed = 0;
                        return false;
                    }
                    if (!TryParseUInt32D(source.Slice(i), out var value5, out bytesConsumed2))
                    {
                        value = default(TimeSpan);
                        bytesConsumed = 0;
                        return false;
                    }
                    i += bytesConsumed2;
                    if (i == source.Length || source[i++] != 46)
                    {
                        value = default(TimeSpan);
                        bytesConsumed = 0;
                        return false;
                    }
                    if (!TryParseTimeSpanFraction(source.Slice(i), out var value6, out bytesConsumed2))
                    {
                        value = default(TimeSpan);
                        bytesConsumed = 0;
                        return false;
                    }
                    i += bytesConsumed2;
                    if (!TryCreateTimeSpan(isNegative, value2, value3, value4, value5, value6, out value))
                    {
                        value = default(TimeSpan);
                        bytesConsumed = 0;
                        return false;
                    }
                    if (i != source.Length && (source[i] == 46 || source[i] == 58))
                    {
                        value = default(TimeSpan);
                        bytesConsumed = 0;
                        return false;
                    }
                    bytesConsumed = i;
                    return true;
                }

                private static bool TryParseTimeSpanC(ReadOnlySpan<byte> source, out TimeSpan value, out int bytesConsumed)
                {
                    TimeSpanSplitter timeSpanSplitter = default(TimeSpanSplitter);
                    if (!timeSpanSplitter.TrySplitTimeSpan(source, periodUsedToSeparateDay: true, out bytesConsumed))
                    {
                        value = default(TimeSpan);
                        return false;
                    }
                    bool isNegative = timeSpanSplitter.IsNegative;
                    bool flag;
                    switch (timeSpanSplitter.Separators)
                    {
                        case 0u:
                            flag = TryCreateTimeSpan(isNegative, timeSpanSplitter.V1, 0u, 0u, 0u, 0u, out value);
                            break;
                        case 16777216u:
                            flag = TryCreateTimeSpan(isNegative, 0u, timeSpanSplitter.V1, timeSpanSplitter.V2, 0u, 0u, out value);
                            break;
                        case 33619968u:
                            flag = TryCreateTimeSpan(isNegative, timeSpanSplitter.V1, timeSpanSplitter.V2, timeSpanSplitter.V3, 0u, 0u, out value);
                            break;
                        case 16842752u:
                            flag = TryCreateTimeSpan(isNegative, 0u, timeSpanSplitter.V1, timeSpanSplitter.V2, timeSpanSplitter.V3, 0u, out value);
                            break;
                        case 33620224u:
                            flag = TryCreateTimeSpan(isNegative, timeSpanSplitter.V1, timeSpanSplitter.V2, timeSpanSplitter.V3, timeSpanSplitter.V4, 0u, out value);
                            break;
                        case 16843264u:
                            flag = TryCreateTimeSpan(isNegative, 0u, timeSpanSplitter.V1, timeSpanSplitter.V2, timeSpanSplitter.V3, timeSpanSplitter.V4, out value);
                            break;
                        case 33620226u:
                            flag = TryCreateTimeSpan(isNegative, timeSpanSplitter.V1, timeSpanSplitter.V2, timeSpanSplitter.V3, timeSpanSplitter.V4, timeSpanSplitter.V5, out value);
                            break;
                        default:
                            value = default(TimeSpan);
                            flag = false;
                            break;
                    }
                    if (!flag)
                    {
                        bytesConsumed = 0;
                        return false;
                    }
                    return true;
                }

                /// <summary>
                /// Parses a <see cref="System.TimeSpan"/> at the start of a UTF-8 string.
                /// </summary>
                /// <param name="source">The UTF-8 string to parse.</param>
                /// <param name="value">When the method returns, contains the value parsed from source, if the parsing operation succeeded.</param>
                /// <param name="bytesConsumed">If the parsing operation was successful, contains the length in bytes of the parsed substring when the method returns. 
                /// <param name="standardFormat">The expected format of the UTF-8 string.</param>
                /// If the method fails, bytesConsumed is set to 0.</param>
                /// <returns><c>true</c> for success; 
                /// <c>false</c> if the string was not syntactically valid or an overflow or underflow occurred.</returns>
                /// <remarks>
                /// Format Supported:
                /// <list type="table">
                ///     <listheader>
                ///         Format <see cref="System.String"/>|||Expected format||| Comments
                ///     </listheader>
                ///     <item>c/t/T (default)	[-][d.]hh:mm:ss[.fffffff]	(constant format)	</item>
                ///     <item>G	[-]d:hh:mm:ss.fffffff		(general long)</item>
                ///     <item>g	[-][d:][h]h:mm:ss[.f[f[f[f[f[f[f]]]]]]	(general short)	</item>
                /// </list>
                /// </remarks>
                public static bool TryParse(ReadOnlySpan<byte> source, out TimeSpan value, out int bytesConsumed, char standardFormat = '\0')
                {
                    switch (standardFormat)
                    {
                        case '\0':
                        case 'T':
                        case 'c':
                        case 't':
                            return TryParseTimeSpanC(source, out value, out bytesConsumed);
                        case 'G':
                            return TryParseTimeSpanBigG(source, out value, out bytesConsumed);
                        case 'g':
                            return TryParseTimeSpanLittleG(source, out value, out bytesConsumed);
                        default:
                            return System.ThrowHelper.TryParseThrowFormatException<TimeSpan>(out value, out bytesConsumed);
                    }
                }

                private static bool TryParseTimeSpanFraction(ReadOnlySpan<byte> source, out uint value, out int bytesConsumed)
                {
                    int num = 0;
                    if (num == source.Length)
                    {
                        value = 0u;
                        bytesConsumed = 0;
                        return false;
                    }
                    uint num2 = (uint)(source[num] - 48);
                    if (num2 > 9)
                    {
                        value = 0u;
                        bytesConsumed = 0;
                        return false;
                    }
                    num++;
                    uint num3 = num2;
                    int num4 = 1;
                    while (num != source.Length)
                    {
                        num2 = (uint)(source[num] - 48);
                        if (num2 > 9)
                        {
                            break;
                        }
                        num++;
                        num4++;
                        if (num4 > 7)
                        {
                            value = 0u;
                            bytesConsumed = 0;
                            return false;
                        }
                        num3 = 10 * num3 + num2;
                    }
                    switch (num4)
                    {
                        case 6:
                            num3 *= 10;
                            break;
                        case 5:
                            num3 *= 100;
                            break;
                        case 4:
                            num3 *= 1000;
                            break;
                        case 3:
                            num3 *= 10000;
                            break;
                        case 2:
                            num3 *= 100000;
                            break;
                        default:
                            num3 *= 1000000;
                            break;
                        case 7:
                            break;
                    }
                    value = num3;
                    bytesConsumed = num;
                    return true;
                }

                private static bool TryCreateTimeSpan(bool isNegative, uint days, uint hours, uint minutes, uint seconds, uint fraction, out TimeSpan timeSpan)
                {
                    if (hours > 23 || minutes > 59 || seconds > 59)
                    {
                        timeSpan = default(TimeSpan);
                        return false;
                    }
                    long num = ((long)days * 3600L * 24 + (long)hours * 3600L + (long)minutes * 60L + seconds) * 1000;
                    long ticks;
                    if (isNegative)
                    {
                        num = -num;
                        if (num < -922337203685477L)
                        {
                            timeSpan = default(TimeSpan);
                            return false;
                        }
                        long num2 = num * 10000;
                        if (num2 < long.MinValue + fraction)
                        {
                            timeSpan = default(TimeSpan);
                            return false;
                        }
                        ticks = num2 - fraction;
                    }
                    else
                    {
                        if (num > 922337203685477L)
                        {
                            timeSpan = default(TimeSpan);
                            return false;
                        }
                        long num3 = num * 10000;
                        if (num3 > long.MaxValue - (long)fraction)
                        {
                            timeSpan = default(TimeSpan);
                            return false;
                        }
                        ticks = num3 + fraction;
                    }
                    timeSpan = new TimeSpan(ticks);
                    return true;
                }

                private static bool TryParseTimeSpanLittleG(ReadOnlySpan<byte> source, out TimeSpan value, out int bytesConsumed)
                {
                    TimeSpanSplitter timeSpanSplitter = default(TimeSpanSplitter);
                    if (!timeSpanSplitter.TrySplitTimeSpan(source, periodUsedToSeparateDay: false, out bytesConsumed))
                    {
                        value = default(TimeSpan);
                        return false;
                    }
                    bool isNegative = timeSpanSplitter.IsNegative;
                    bool flag;
                    switch (timeSpanSplitter.Separators)
                    {
                        case 0u:
                            flag = TryCreateTimeSpan(isNegative, timeSpanSplitter.V1, 0u, 0u, 0u, 0u, out value);
                            break;
                        case 16777216u:
                            flag = TryCreateTimeSpan(isNegative, 0u, timeSpanSplitter.V1, timeSpanSplitter.V2, 0u, 0u, out value);
                            break;
                        case 16842752u:
                            flag = TryCreateTimeSpan(isNegative, 0u, timeSpanSplitter.V1, timeSpanSplitter.V2, timeSpanSplitter.V3, 0u, out value);
                            break;
                        case 16843008u:
                            flag = TryCreateTimeSpan(isNegative, timeSpanSplitter.V1, timeSpanSplitter.V2, timeSpanSplitter.V3, timeSpanSplitter.V4, 0u, out value);
                            break;
                        case 16843264u:
                            flag = TryCreateTimeSpan(isNegative, 0u, timeSpanSplitter.V1, timeSpanSplitter.V2, timeSpanSplitter.V3, timeSpanSplitter.V4, out value);
                            break;
                        case 16843010u:
                            flag = TryCreateTimeSpan(isNegative, timeSpanSplitter.V1, timeSpanSplitter.V2, timeSpanSplitter.V3, timeSpanSplitter.V4, timeSpanSplitter.V5, out value);
                            break;
                        default:
                            value = default(TimeSpan);
                            flag = false;
                            break;
                    }
                    if (!flag)
                    {
                        bytesConsumed = 0;
                        return false;
                    }
                    return true;
                }

            }

            /// <summary>
            /// Represents the validity of a UTF code unit sequence.
            /// </summary>
            public enum SequenceValidity
            {
                /// <summary>
                /// The sequence is empty.
                /// </summary>
                Empty,
                /// <summary>
                /// The sequence is well-formed and unambiguously represents a proper Unicode scalar value.
                /// </summary>
                /// <remarks>
                /// [ 20 ] (U+0020 SPACE) is a well-formed UTF-8 sequence.
                /// [ C3 A9 ] (U+00E9 LATIN SMALL LETTER E WITH ACUTE) is a well-formed UTF-8 sequence.
                /// [ F0 9F 98 80 ] (U+1F600 GRINNING FACE) is a well-formed UTF-8 sequence.
                /// [ D83D DE00 ] (U+1F600 GRINNING FACE) is a well-formed UTF-16 sequence.
                /// </remarks>
                WellFormed,
                /// <summary>
                /// The sequence is not well-formed on its own, but it could appear as a prefix
                /// of a longer well-formed sequence. More code units are needed to make a proper
                /// determination as to whether this sequence is well-formed. Incomplete sequences
                /// can only appear at the end of a string.
                /// </summary>
                /// <remarks>
                /// [ C2 ] is an incomplete UTF-8 sequence if it is followed by nothing.
                /// [ F0 9F ] is an incomplete UTF-8 sequence if it is followed by nothing.
                /// [ D83D ] is an incomplete UTF-16 sequence if it is followed by nothing.
                /// </remarks>
                Incomplete,
                /// <summary>
                /// The sequence is never well-formed anywhere, or this sequence can never appear as a prefix
                /// of a longer well-formed sequence, or the sequence was improperly terminated by the code
                /// unit which appeared immediately after this sequence.
                /// </summary>
                /// <remarks>
                /// [ 80 ] is an invalid UTF-8 sequence (code unit cannot appear at start of sequence).
                /// [ FE ] is an invalid UTF-8 sequence (sequence is never well-formed anywhere in UTF-8 string).
                /// [ C2 ] is an invalid UTF-8 sequence if it is followed by [ 20 ] (sequence improperly terminated).
                /// [ ED A0 ] is an invalid UTF-8 sequence (sequence is never well-formed anywhere in UTF-8 string).
                /// [ DE00 ] is an invalid UTF-16 sequence (code unit cannot appear at start of sequence).
                /// </remarks>
                Invalid
            }

        }

        namespace Binary
        {
            /// <summary>
            /// Reads bytes as primitives with specific endianness.
            /// </summary>
#pragma warning disable CS1591
            public static class BinaryPrimitives
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                [CLSCompliant(false)]
                public static sbyte ReverseEndianness(sbyte value)
                {
                    return value;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static short ReverseEndianness(short value)
                {
                    return (short)(((value & 0xFF) << 8) | ((value & 0xFF00) >> 8));
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static int ReverseEndianness(int value)
                {
                    return (int)ReverseEndianness((uint)value);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static long ReverseEndianness(long value)
                {
                    return (long)ReverseEndianness((ulong)value);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static byte ReverseEndianness(byte value)
                {
                    return value;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                [CLSCompliant(false)]
                public static ushort ReverseEndianness(ushort value)
                {
                    return (ushort)((value >> 8) + (value << 8));
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                [CLSCompliant(false)]
                public static uint ReverseEndianness(uint value)
                {
                    uint num = value & 0xFF00FFu;
                    uint num2 = value & 0xFF00FF00u;
                    return ((num >> 8) | (num << 24)) + ((num2 << 8) | (num2 >> 24));
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                [CLSCompliant(false)]
                public static ulong ReverseEndianness(ulong value)
                {
                    return ((ulong)ReverseEndianness((uint)value) << 32) + ReverseEndianness((uint)(value >> 32));
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static short ReadInt16BigEndian(ReadOnlySpan<byte> source)
                {
                    short num = MemoryMarshal.Read<short>(source);
                    if (BitConverter.IsLittleEndian)
                    {
                        num = ReverseEndianness(num);
                    }
                    return num;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static int ReadInt32BigEndian(ReadOnlySpan<byte> source)
                {
                    int num = MemoryMarshal.Read<int>(source);
                    if (BitConverter.IsLittleEndian)
                    {
                        num = ReverseEndianness(num);
                    }
                    return num;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static long ReadInt64BigEndian(ReadOnlySpan<byte> source)
                {
                    long num = MemoryMarshal.Read<long>(source);
                    if (BitConverter.IsLittleEndian)
                    {
                        num = ReverseEndianness(num);
                    }
                    return num;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                [CLSCompliant(false)]
                public static ushort ReadUInt16BigEndian(ReadOnlySpan<byte> source)
                {
                    ushort num = MemoryMarshal.Read<ushort>(source);
                    if (BitConverter.IsLittleEndian)
                    {
                        num = ReverseEndianness(num);
                    }
                    return num;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                [CLSCompliant(false)]
                public static uint ReadUInt32BigEndian(ReadOnlySpan<byte> source)
                {
                    uint num = MemoryMarshal.Read<uint>(source);
                    if (BitConverter.IsLittleEndian)
                    {
                        num = ReverseEndianness(num);
                    }
                    return num;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                [CLSCompliant(false)]
                public static ulong ReadUInt64BigEndian(ReadOnlySpan<byte> source)
                {
                    ulong num = MemoryMarshal.Read<ulong>(source);
                    if (BitConverter.IsLittleEndian)
                    {
                        num = ReverseEndianness(num);
                    }
                    return num;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static bool TryReadInt16BigEndian(ReadOnlySpan<byte> source, out short value)
                {
                    bool result = MemoryMarshal.TryRead<short>(source, out value);
                    if (BitConverter.IsLittleEndian)
                    {
                        value = ReverseEndianness(value);
                    }
                    return result;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static bool TryReadInt32BigEndian(ReadOnlySpan<byte> source, out int value)
                {
                    bool result = MemoryMarshal.TryRead<int>(source, out value);
                    if (BitConverter.IsLittleEndian)
                    {
                        value = ReverseEndianness(value);
                    }
                    return result;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static bool TryReadInt64BigEndian(ReadOnlySpan<byte> source, out long value)
                {
                    bool result = MemoryMarshal.TryRead<long>(source, out value);
                    if (BitConverter.IsLittleEndian)
                    {
                        value = ReverseEndianness(value);
                    }
                    return result;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                [CLSCompliant(false)]
                public static bool TryReadUInt16BigEndian(ReadOnlySpan<byte> source, out ushort value)
                {
                    bool result = MemoryMarshal.TryRead<ushort>(source, out value);
                    if (BitConverter.IsLittleEndian)
                    {
                        value = ReverseEndianness(value);
                    }
                    return result;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                [CLSCompliant(false)]
                public static bool TryReadUInt32BigEndian(ReadOnlySpan<byte> source, out uint value)
                {
                    bool result = MemoryMarshal.TryRead<uint>(source, out value);
                    if (BitConverter.IsLittleEndian)
                    {
                        value = ReverseEndianness(value);
                    }
                    return result;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                [CLSCompliant(false)]
                public static bool TryReadUInt64BigEndian(ReadOnlySpan<byte> source, out ulong value)
                {
                    bool result = MemoryMarshal.TryRead<ulong>(source, out value);
                    if (BitConverter.IsLittleEndian)
                    {
                        value = ReverseEndianness(value);
                    }
                    return result;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static short ReadInt16LittleEndian(ReadOnlySpan<byte> source)
                {
                    short num = MemoryMarshal.Read<short>(source);
                    if (!BitConverter.IsLittleEndian)
                    {
                        num = ReverseEndianness(num);
                    }
                    return num;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static int ReadInt32LittleEndian(ReadOnlySpan<byte> source)
                {
                    int num = MemoryMarshal.Read<int>(source);
                    if (!BitConverter.IsLittleEndian)
                    {
                        num = ReverseEndianness(num);
                    }
                    return num;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static long ReadInt64LittleEndian(ReadOnlySpan<byte> source)
                {
                    long num = MemoryMarshal.Read<long>(source);
                    if (!BitConverter.IsLittleEndian)
                    {
                        num = ReverseEndianness(num);
                    }
                    return num;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                [CLSCompliant(false)]
                public static ushort ReadUInt16LittleEndian(ReadOnlySpan<byte> source)
                {
                    ushort num = MemoryMarshal.Read<ushort>(source);
                    if (!BitConverter.IsLittleEndian)
                    {
                        num = ReverseEndianness(num);
                    }
                    return num;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                [CLSCompliant(false)]
                public static uint ReadUInt32LittleEndian(ReadOnlySpan<byte> source)
                {
                    uint num = MemoryMarshal.Read<uint>(source);
                    if (!BitConverter.IsLittleEndian)
                    {
                        num = ReverseEndianness(num);
                    }
                    return num;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                [CLSCompliant(false)]
                public static ulong ReadUInt64LittleEndian(ReadOnlySpan<byte> source)
                {
                    ulong num = MemoryMarshal.Read<ulong>(source);
                    if (!BitConverter.IsLittleEndian)
                    {
                        num = ReverseEndianness(num);
                    }
                    return num;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static bool TryReadInt16LittleEndian(ReadOnlySpan<byte> source, out short value)
                {
                    bool result = MemoryMarshal.TryRead<short>(source, out value);
                    if (!BitConverter.IsLittleEndian)
                    {
                        value = ReverseEndianness(value);
                    }
                    return result;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static bool TryReadInt32LittleEndian(ReadOnlySpan<byte> source, out int value)
                {
                    bool result = MemoryMarshal.TryRead<int>(source, out value);
                    if (!BitConverter.IsLittleEndian)
                    {
                        value = ReverseEndianness(value);
                    }
                    return result;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static bool TryReadInt64LittleEndian(ReadOnlySpan<byte> source, out long value)
                {
                    bool result = MemoryMarshal.TryRead<long>(source, out value);
                    if (!BitConverter.IsLittleEndian)
                    {
                        value = ReverseEndianness(value);
                    }
                    return result;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                [CLSCompliant(false)]
                public static bool TryReadUInt16LittleEndian(ReadOnlySpan<byte> source, out ushort value)
                {
                    bool result = MemoryMarshal.TryRead<ushort>(source, out value);
                    if (!BitConverter.IsLittleEndian)
                    {
                        value = ReverseEndianness(value);
                    }
                    return result;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                [CLSCompliant(false)]
                public static bool TryReadUInt32LittleEndian(ReadOnlySpan<byte> source, out uint value)
                {
                    bool result = MemoryMarshal.TryRead<uint>(source, out value);
                    if (!BitConverter.IsLittleEndian)
                    {
                        value = ReverseEndianness(value);
                    }
                    return result;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                [CLSCompliant(false)]
                public static bool TryReadUInt64LittleEndian(ReadOnlySpan<byte> source, out ulong value)
                {
                    bool result = MemoryMarshal.TryRead<ulong>(source, out value);
                    if (!BitConverter.IsLittleEndian)
                    {
                        value = ReverseEndianness(value);
                    }
                    return result;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static void WriteInt16BigEndian(Span<byte> destination, short value)
                {
                    if (BitConverter.IsLittleEndian)
                    {
                        value = ReverseEndianness(value);
                    }
                    MemoryMarshal.Write(destination, ref value);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static void WriteInt32BigEndian(Span<byte> destination, int value)
                {
                    if (BitConverter.IsLittleEndian)
                    {
                        value = ReverseEndianness(value);
                    }
                    MemoryMarshal.Write(destination, ref value);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static void WriteInt64BigEndian(Span<byte> destination, long value)
                {
                    if (BitConverter.IsLittleEndian)
                    {
                        value = ReverseEndianness(value);
                    }
                    MemoryMarshal.Write(destination, ref value);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                [CLSCompliant(false)]
                public static void WriteUInt16BigEndian(Span<byte> destination, ushort value)
                {
                    if (BitConverter.IsLittleEndian)
                    {
                        value = ReverseEndianness(value);
                    }
                    MemoryMarshal.Write(destination, ref value);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                [CLSCompliant(false)]
                public static void WriteUInt32BigEndian(Span<byte> destination, uint value)
                {
                    if (BitConverter.IsLittleEndian)
                    {
                        value = ReverseEndianness(value);
                    }
                    MemoryMarshal.Write(destination, ref value);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                [CLSCompliant(false)]
                public static void WriteUInt64BigEndian(Span<byte> destination, ulong value)
                {
                    if (BitConverter.IsLittleEndian)
                    {
                        value = ReverseEndianness(value);
                    }
                    MemoryMarshal.Write(destination, ref value);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static bool TryWriteInt16BigEndian(Span<byte> destination, short value)
                {
                    if (BitConverter.IsLittleEndian)
                    {
                        value = ReverseEndianness(value);
                    }
                    return MemoryMarshal.TryWrite(destination, ref value);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static bool TryWriteInt32BigEndian(Span<byte> destination, int value)
                {
                    if (BitConverter.IsLittleEndian)
                    {
                        value = ReverseEndianness(value);
                    }
                    return MemoryMarshal.TryWrite(destination, ref value);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static bool TryWriteInt64BigEndian(Span<byte> destination, long value)
                {
                    if (BitConverter.IsLittleEndian)
                    {
                        value = ReverseEndianness(value);
                    }
                    return MemoryMarshal.TryWrite(destination, ref value);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                [CLSCompliant(false)]
                public static bool TryWriteUInt16BigEndian(Span<byte> destination, ushort value)
                {
                    if (BitConverter.IsLittleEndian)
                    {
                        value = ReverseEndianness(value);
                    }
                    return MemoryMarshal.TryWrite(destination, ref value);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                [CLSCompliant(false)]
                public static bool TryWriteUInt32BigEndian(Span<byte> destination, uint value)
                {
                    if (BitConverter.IsLittleEndian)
                    {
                        value = ReverseEndianness(value);
                    }
                    return MemoryMarshal.TryWrite(destination, ref value);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                [CLSCompliant(false)]
                public static bool TryWriteUInt64BigEndian(Span<byte> destination, ulong value)
                {
                    if (BitConverter.IsLittleEndian)
                    {
                        value = ReverseEndianness(value);
                    }
                    return MemoryMarshal.TryWrite(destination, ref value);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static void WriteInt16LittleEndian(Span<byte> destination, short value)
                {
                    if (!BitConverter.IsLittleEndian)
                    {
                        value = ReverseEndianness(value);
                    }
                    MemoryMarshal.Write(destination, ref value);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static void WriteInt32LittleEndian(Span<byte> destination, int value)
                {
                    if (!BitConverter.IsLittleEndian)
                    {
                        value = ReverseEndianness(value);
                    }
                    MemoryMarshal.Write(destination, ref value);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static void WriteInt64LittleEndian(Span<byte> destination, long value)
                {
                    if (!BitConverter.IsLittleEndian)
                    {
                        value = ReverseEndianness(value);
                    }
                    MemoryMarshal.Write(destination, ref value);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                [CLSCompliant(false)]
                public static void WriteUInt16LittleEndian(Span<byte> destination, ushort value)
                {
                    if (!BitConverter.IsLittleEndian)
                    {
                        value = ReverseEndianness(value);
                    }
                    MemoryMarshal.Write(destination, ref value);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                [CLSCompliant(false)]
                public static void WriteUInt32LittleEndian(Span<byte> destination, uint value)
                {
                    if (!BitConverter.IsLittleEndian)
                    {
                        value = ReverseEndianness(value);
                    }
                    MemoryMarshal.Write(destination, ref value);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                [CLSCompliant(false)]
                public static void WriteUInt64LittleEndian(Span<byte> destination, ulong value)
                {
                    if (!BitConverter.IsLittleEndian)
                    {
                        value = ReverseEndianness(value);
                    }
                    MemoryMarshal.Write(destination, ref value);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static bool TryWriteInt16LittleEndian(Span<byte> destination, short value)
                {
                    if (!BitConverter.IsLittleEndian)
                    {
                        value = ReverseEndianness(value);
                    }
                    return MemoryMarshal.TryWrite(destination, ref value);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static bool TryWriteInt32LittleEndian(Span<byte> destination, int value)
                {
                    if (!BitConverter.IsLittleEndian)
                    {
                        value = ReverseEndianness(value);
                    }
                    return MemoryMarshal.TryWrite(destination, ref value);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static bool TryWriteInt64LittleEndian(Span<byte> destination, long value)
                {
                    if (!BitConverter.IsLittleEndian)
                    {
                        value = ReverseEndianness(value);
                    }
                    return MemoryMarshal.TryWrite(destination, ref value);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                [CLSCompliant(false)]
                public static bool TryWriteUInt16LittleEndian(Span<byte> destination, ushort value)
                {
                    if (!BitConverter.IsLittleEndian)
                    {
                        value = ReverseEndianness(value);
                    }
                    return MemoryMarshal.TryWrite(destination, ref value);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                [CLSCompliant(false)]
                public static bool TryWriteUInt32LittleEndian(Span<byte> destination, uint value)
                {
                    if (!BitConverter.IsLittleEndian)
                    {
                        value = ReverseEndianness(value);
                    }
                    return MemoryMarshal.TryWrite(destination, ref value);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                [CLSCompliant(false)]
                public static bool TryWriteUInt64LittleEndian(Span<byte> destination, ulong value)
                {
                    if (!BitConverter.IsLittleEndian)
                    {
                        value = ReverseEndianness(value);
                    }
                    return MemoryMarshal.TryWrite(destination, ref value);
                }
            }
#pragma warning restore CS1591

        }

        internal sealed class ArrayMemoryPool<T> : MemoryPool<T>
        {
            private sealed class ArrayMemoryPoolBuffer : IMemoryOwner<T>, IDisposable
            {
                private T[] _array;

                public Memory<T> Memory
                {
                    get
                    {
                        T[] array = _array;
                        if (array == null)
                        {
                            System.ThrowHelper.ThrowObjectDisposedException_ArrayMemoryPoolBuffer();
                        }
                        return new Memory<T>(array);
                    }
                }

                public ArrayMemoryPoolBuffer(int size)
                {
                    _array = ArrayPool<T>.Shared.Rent(size);
                }

                public void Dispose()
                {
                    T[] array = _array;
                    if (array != null)
                    {
                        _array = null;
                        ArrayPool<T>.Shared.Return(array);
                    }
                }
            }

            private const int s_maxBufferSize = int.MaxValue;

            public sealed override int MaxBufferSize => int.MaxValue;

            public sealed override IMemoryOwner<T> Rent(int minimumBufferSize = -1)
            {
                if (minimumBufferSize == -1)
                {
                    minimumBufferSize = 1 + 4095 / Unsafe.SizeOf<T>();
                }
                else if ((uint)minimumBufferSize > 2147483647u)
                {
                    System.ThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.minimumBufferSize);
                }
                return new ArrayMemoryPoolBuffer(minimumBufferSize);
            }

            protected sealed override void Dispose(bool disposing)
            {
            }
        }

        /// <summary>
        /// Represents a pool of memory blocks.
        /// </summary>
        /// <typeparam name="T">The type of the items in the memory pool.</typeparam>
        public abstract class MemoryPool<T> : IDisposable
        {
            private static readonly MemoryPool<T> s_shared = new ArrayMemoryPool<T>();

            /// <summary>
            /// Gets a singleton instance of a memory pool based on arrays.
            /// </summary>
            public static MemoryPool<T> Shared => s_shared;

            /// <summary>
            /// The maximum buffer size supported by this pool.
            /// </summary>
            public abstract int MaxBufferSize { get; }

            /// <summary>
            /// Returns a memory block capable of holding at least <paramref name="minBufferSize"/> elements of T.
            /// </summary>
            /// <param name="minBufferSize">The minimum number of elements of <typeparamref name="T"/> that the memory pool can hold. A value of -1 returns a memory pool set to the default size for the pool.</param>
            /// <returns>A memory block capable of holding at least <paramref name="minBufferSize"/> elements of T.</returns>
            public abstract IMemoryOwner<T> Rent(int minBufferSize = -1);

            /// <summary>
            /// Frees all resources used by the memory pool.
            /// </summary>
            public void Dispose()
            {
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }

            /// <summary>
            /// Frees the unmanaged resources used by the memory pool and optionally releases the managed resources.
            /// </summary>
            /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.</param>
            protected abstract void Dispose(bool disposing);
        }

        /// <summary>
        /// This enum defines the various potential status that can be returned from Span-based operations
        /// that support processing of input contained in multiple discontiguous buffers.
        /// </summary>
        public enum OperationStatus
        {
            /// <summary>
            /// The entire input buffer has been processed and the operation is complete.
            /// </summary>
            Done,
            /// <summary>
            /// The input is partially processed, up to what could fit into the destination buffer.
            /// The caller can enlarge the destination buffer, slice the buffers appropriately, and retry.
            /// </summary>
            DestinationTooSmall,
            /// <summary>
            /// The input is partially processed, up to the last valid chunk of the input that could be consumed.
            /// The caller can stitch the remaining unprocessed input with more data, slice the buffers appropriately, and retry.
            /// </summary>
            NeedMoreData,
            /// <summary>
            /// The input contained invalid bytes which could not be processed. If the input is partially processed,
            /// the destination contains the partial result. This guarantees that no additional data appended to the input
            /// will make the invalid sequence valid.
            /// </summary>
            InvalidData,
        }


        /// <summary>
        /// Represents a standard format string without using an actual string.
        /// </summary>
        /// <remarks>A <see cref="StandardFormat"/> object consists of a single character 
        /// standard format specifier (such as 'G', 'D', or 'X') and an optional 
        /// precision specifier. The precision specifier can range from 0 to 9, 
        /// or it can be the special <see cref="StandardFormat.NoPrecision" /> value.</remarks>
        public readonly struct StandardFormat : IEquatable<StandardFormat>
        {
            /// <summary>
            /// Indicates that a format doesn't use a precision or that the precision is unspecified.
            /// </summary>
            public const byte NoPrecision = byte.MaxValue;

            /// <summary>
            /// Defines the maximum valid precision value.
            /// </summary>
            /// <remarks>The maximum valid precision is 99.</remarks>
            public const byte MaxPrecision = 99;

            private readonly byte _format;

            private readonly byte _precision;

            /// <summary>
            /// Gets the character component of the format.
            /// </summary>
            public char Symbol => (char)_format;

            /// <summary>
            /// Gets the precision component of the format.
            /// </summary>
            public byte Precision => _precision;

            /// <summary>
            /// Gets a value that indicates whether a format has a defined precision.
            /// </summary>
            public bool HasPrecision => _precision != NoPrecision;

            /// <summary>
            /// Gets a value that indicates whether the current instance is a default format.
            /// </summary>
            /// <remarks>A default format has a format specifier whose <see cref="System.Byte"/> value is 0 and whose precision is <see cref="NoPrecision"/>.</remarks>
            public bool IsDefault { get { if (_format == 0) { return _precision == 0; } return false; } }

            /// <summary>
            /// Initializes a new instance of the <see cref="StandardFormat"/> structure.
            /// </summary>
            /// <param name="symbol">A type-specific format specifier, such as 'G', 'D', or 'X'.</param>
            /// <param name="precision">An optional precision ranging from 0 to 99, or the special value <see cref="NoPrecision"/> (the default).</param>
            /// <exception cref="System.ArgumentOutOfRangeException">
            /// <paramref name="symbol"/> is not <see cref="NoPrecision"/>, and its value is greater than <see cref="MaxPrecision"/>. <br/>
            /// -or- <br />
            /// <paramref name="symbol"/> cannot be converted to a <see cref="System.Byte"/>.
            /// </exception>
            public StandardFormat(char symbol, byte precision = NoPrecision)
            {
                if (precision != NoPrecision && precision > 99)
                {
                    System.ThrowHelper.ThrowArgumentOutOfRangeException_PrecisionTooLarge();
                }
                if (symbol != (byte)symbol)
                {
                    System.ThrowHelper.ThrowArgumentOutOfRangeException_SymbolDoesNotFit();
                }
                _format = (byte)symbol;
                _precision = precision;
            }

            /// <summary>
            /// Converts a character to a <see cref="StandardFormat"/> instance using <see cref="NoPrecision"/> precision.
            /// </summary>
            /// <param name="symbol">The character to convert to a <see cref="StandardFormat"/> value.</param>
            public static implicit operator StandardFormat(char symbol) { return new StandardFormat(symbol); }

            /// <summary>
            /// Converts a <see cref="ReadOnlySpan{T}"/> (T is <see cref="System.Char"/>) into a <see cref="StandardFormat"/> instance using <see cref="NoPrecision"/> precision.
            /// </summary>
            /// <param name="format">A read-only span that contains the character to parse.</param>
            /// <returns>A value whose <see cref="Symbol"/> property value is the character in format and whose <see cref="Precision"/> property value is <see cref="NoPrecision"/> .</returns>
            /// <exception cref="FormatException">
            /// <paramref name="format"/> is not a valid standard character.
            /// </exception>
            public static StandardFormat Parse(ReadOnlySpan<char> format)
            {
                if (format.Length == 0)
                {
                    return default(StandardFormat);
                }
                char symbol = format[0];
                byte precision;
                if (format.Length == 1)
                {
                    precision = byte.MaxValue;
                }
                else
                {
                    uint num = 0u;
                    for (int i = 1; i < format.Length; i++)
                    {
                        uint num2 = (uint)(format[i] - 48);
                        if (num2 > 9)
                        {
                            throw new FormatException(System.SR.Format(
                                MDCFR.Properties.Resources.Argument_CannotParsePrecision, (byte)99));
                        }
                        num = num * 10 + num2;
                        if (num > 99)
                        {
                            throw new FormatException(System.SR.Format(
                                MDCFR.Properties.Resources.Argument_PrecisionTooLarge, (byte)99));
                        }
                    }
                    precision = (byte)num;
                }
                return new StandardFormat(symbol, precision);
            }

            /// <summary>
            /// Converts a classic .NET standard format string to a <see cref="StandardFormat"/> instance.
            /// </summary>
            /// <param name="format">A classic .NET standard format string.</param>
            /// <returns>A format.</returns>
            /// <exception cref="FormatException">
            /// <paramref name="format"/> is not a valid standard format string.
            /// </exception>
            public static StandardFormat Parse(string format)
            {
                if (format != null)
                {
                    return Parse(format.AsSpan());
                }
                return default(StandardFormat);
            }

            /// <summary>
            /// Returns a value that indicates whether the specified object is a <see cref="StandardFormat"/> object that is equal to the current instance.
            /// </summary>
            /// <param name="obj">An object to compare to the current instance.</param>
            /// <returns><see langword="true"/> if the two instances are equal; otherwise, <see langword="true"/>.</returns>
            public override bool Equals(object obj)
            {
                if (obj is StandardFormat other)
                {
                    return Equals(other);
                }
                return false;
            }

            /// <inheritdoc />
            public override int GetHashCode() { return _format.GetHashCode() ^ _precision.GetHashCode(); }

            /// <summary>
            /// Returns a value that indicates whether the specified <see cref="StandardFormat"/> is equal to the current instance.
            /// </summary>
            /// <param name="other">The format to compare to the current instance.</param>
            /// <returns><see langword="true"/> if the two instances are equal; otherwise, <see langword="false"/>.</returns>
            public bool Equals(StandardFormat other) { if (_format == other._format) { return _precision == other._precision; } return false; }

            /// <summary>
            /// Returns the string representation of this format.
            /// </summary>
            /// <returns>The string representation of this format.</returns>
            /// <remarks>The string representation of a <see cref="StandardFormat"/> instance is a standard .NET format string.</remarks>
            public unsafe override string ToString()
            {
                char* ptr = stackalloc char[4];
                int length = 0;
                char symbol = Symbol;
                if (symbol != 0)
                {
                    ptr[length++] = symbol;
                    byte b = Precision;
                    if (b != byte.MaxValue)
                    {
                        if (b >= 100)
                        {
                            ptr[length++] = (char)(48 + (int)b / 100 % 10);
                            b = (byte)((int)b % 100);
                        }
                        if (b >= 10)
                        {
                            ptr[length++] = (char)(48 + (int)b / 10 % 10);
                            b = (byte)((int)b % 10);
                        }
                        ptr[length++] = (char)(48 + b);
                    }
                }
                return new string(ptr, 0, length);
            }

            /// <summary>
            /// Returns a value that indicates whether two <see cref="StandardFormat"/> instances are equal.
            /// </summary>
            /// <param name="left">The first format to compare.</param>
            /// <param name="right">The second format to compare.</param>
            /// <returns><see langword="true"/> if the two instances are equal; otherwise, <see langword="false"/>.</returns>
            public static bool operator ==(StandardFormat left, StandardFormat right) { return left.Equals(right); }

            /// <summary>
            /// Returns a value that indicates whether two <see cref="StandardFormat"/> instances are unequal.
            /// </summary>
            /// <param name="left">The first format to compare.</param>
            /// <param name="right">The second format to compare.</param>
            /// <returns><see langword="true"/> if the two instances are equal; otherwise, <see langword="false"/>.</returns>
            /// <remarks>Two <see cref="StandardFormat"/> instances are unequal if their <see cref="Symbol"/> and <see cref="Precision"/> properties are not identical.</remarks>
            public static bool operator !=(StandardFormat left, StandardFormat right) { return !left.Equals(right); }
        }

        /// <summary>
        /// Identifies the owner of a block of memory who is responsible for disposing of the underlying memory appropriately.
        /// </summary>
        /// <typeparam name="T">The type of elements to store in memory.</typeparam>
        /// <remarks>
        /// The <see cref="IMemoryOwner{T}"/> interface is used to define the owner responsible for the lifetime management of a <see cref="Memory{T}"/> buffer. 
        /// An instance of the <see cref="IMemoryOwner{T}"/> interface is returned by the <see cref="MemoryPool{T}.Rent(int)"/> method. <br />
        /// While a buffer can have multiple consumers, it can only have a single owner at any given time. The owner can: <br /> <br />
        /// -> Create the buffer either directly or by calling a factory method. <br />
        /// -> Transfer ownership to another consumer. In this case, the previous owner should no longer use the buffer. <br />
        /// -> Destroy the buffer when it is no longer in use. <br /> <br />
        /// Because the <see cref="IMemoryOwner{T}"/> object implements the <see cref="IDisposable"/> interface, you should 
        /// call its <see cref="IDisposable.Dispose"/> method only after the memory buffer is no longer needed and you have 
        /// destroyed it.You should not dispose of the <see cref="IMemoryOwner{T}"/> object while a reference to its memory 
        /// is available.This means that the type in which <see cref="IMemoryOwner{T}"/> is declared should not have a Finalize method.
        /// </remarks>
        public interface IMemoryOwner<T> : IDisposable
        {
            /// <summary>
            /// Gets the memory belonging to this owner.
            /// </summary>
            Memory<T> Memory { get; }
        }

        /// <summary>
        /// Provides a mechanism for pinning and unpinning objects to prevent the garbage collector from moving them.
        /// </summary>
        /// <remarks>The <see cref="MemoryManager{T}"/> class implements the <see cref="IPinnable"/> interface.</remarks>
        public interface IPinnable
        {
            /// <summary>
            /// Pins a block of memory.
            /// </summary>
            /// <param name="elementIndex">The offset to the element within the memory buffer to which the returned <see cref="MemoryHandle"/> points.</param>
            /// <returns>A handle to the block of memory.</returns>
            /// <remarks>
            /// A developer can access an object that implements the <see cref="IPinnable"/> interface without pinning it only through managed APIs. 
            /// Pinning is required for access by unmanaged APIs. <br />
            /// Call this method to indicate that the <see cref="IPinnable"/> object cannot be moved by the garbage collector so that the 
            /// address of the pinned object can be used.
            /// </remarks>
            MemoryHandle Pin(int elementIndex);

            /// <summary>
            /// Frees a block of pinned memory.
            /// </summary>
            /// <remarks>Call this method to indicate that the <see cref="IPinnable"/> object no longer needs to be pinned, 
            /// and that the garbage collector can now move the object.
            /// </remarks>
            void Unpin();
        }

        /// <summary>
        /// Provides a memory handle for a block of memory.
        /// </summary>
        /// <remarks>
        /// A MemoryHandle instance represents a handle to a pinned block of memory. It is returned by the following methods:<br /> <br />
        /// <see cref="IPinnable.Pin(int)"/> <br />
        /// <see cref="Memory{T}.Pin"/> <br />
        /// <see cref="ReadOnlyMemory{T}.Pin"/> <br />
        /// <see cref="MemoryManager{T}.Pin(int)"/> <br />
        /// </remarks>
        public struct MemoryHandle : IDisposable
        {
            private unsafe void* _pointer;

            private GCHandle _handle;

            private IPinnable _pinnable;

            /// <summary>
            /// Returns a pointer to the memory block.
            /// </summary>
            /// <remarks>The memory is assumed to be pinned so that its address won't change.</remarks>
            [CLSCompliant(false)]
            public unsafe void* Pointer => _pointer;

            /// <summary>
            /// Creates a new memory handle for the block of memory.
            /// </summary>
            /// <param name="pointer">A pointer to memory.</param>
            /// <param name="handle">A handle used to pin array buffers.</param>
            /// <param name="pinnable">A reference to a manually managed object, or <see langword="default"/> if there is no memory manager.</param>
            [CLSCompliant(false)]
            public unsafe MemoryHandle(void* pointer, GCHandle handle = default(GCHandle), IPinnable pinnable = null)
            {
                _pointer = pointer;
                _handle = handle;
                _pinnable = pinnable;
            }

            /// <summary>
            /// Frees the pinned handle and releases the <see cref="IPinnable"/> instance.
            /// </summary>
            public unsafe void Dispose()
            {
                if (_handle.IsAllocated) { _handle.Free(); }
                if (_pinnable != null) { _pinnable.Unpin(); _pinnable = null; }
                _pointer = null;
            }
        }

        /// <summary>
        /// Provides extension methods for <see cref="ReadOnlySequence{T}"/>.
        /// </summary>
        public static class BuffersExtensions
        {
            /// <summary>
            /// Returns the position of the first occurrence of item in the <see cref="ReadOnlySequence{T}"/>.
            /// </summary>
            /// <typeparam name="T">The type of the items in the <see cref="ReadOnlySequence{T}"/>.</typeparam>
            /// <param name="source">The source <see cref="ReadOnlySequence{T}"/>.</param>
            /// <param name="value">The item to find in the <see cref="ReadOnlySequence{T}"/>.</param>
            /// <returns>An object whose <see cref="System.SequencePosition.GetInteger"/> method returns the position 
            /// of the first occurrence of item, or an object whose <see cref="System.Nullable{SequencePosition}.HasValue"/> property is false .</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static SequencePosition? PositionOf<T>(this in ReadOnlySequence<T> source, T value) where T : IEquatable<T>
            {
                if (source.IsSingleSegment)
                {
                    int num = source.First.Span.IndexOf(value);
                    if (num != -1)
                    {
                        return source.GetPosition(num);
                    }
                    return null;
                }
                return PositionOfMultiSegment(in source, value);
            }

            private static SequencePosition? PositionOfMultiSegment<T>(in ReadOnlySequence<T> source, T value) where T : IEquatable<T>
            {
                SequencePosition position = source.Start;
                SequencePosition origin = position;
                ReadOnlyMemory<T> memory;
                while (source.TryGet(ref position, out memory))
                {
                    int num = memory.Span.IndexOf(value);
                    if (num != -1)
                    {
                        return source.GetPosition(num, origin);
                    }
                    if (position.GetObject() == null)
                    {
                        break;
                    }
                    origin = position;
                }
                return null;
            }

            /// <summary>
            /// Copies the <see cref="ReadOnlySequence{T}"/> to the specified <see cref="Span{T}"/>.
            /// </summary>
            /// <typeparam name="T">The type of the items in the <see cref="ReadOnlySequence{T}"/>.</typeparam>
            /// <param name="source">The source <see cref="ReadOnlySequence{T}"/>.</param>
            /// <param name="destination">The destination <see cref="Span{T}"/>.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void CopyTo<T>(this in ReadOnlySequence<T> source, Span<T> destination)
            {
                if (source.Length > destination.Length)
                {
                    System.ThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.destination);
                }
                if (source.IsSingleSegment)
                {
                    source.First.Span.CopyTo(destination);
                }
                else
                {
                    CopyToMultiSegment(in source, destination);
                }
            }

            private static void CopyToMultiSegment<T>(in ReadOnlySequence<T> sequence, Span<T> destination)
            {
                SequencePosition position = sequence.Start;
                ReadOnlyMemory<T> memory;
                while (sequence.TryGet(ref position, out memory))
                {
                    ReadOnlySpan<T> span = memory.Span;
                    span.CopyTo(destination);
                    if (position.GetObject() != null)
                    {
                        destination = destination.Slice(span.Length);
                        continue;
                    }
                    break;
                }
            }

            /// <summary>
            /// Converts the <see cref="ReadOnlySequence{T}"/> to an array.
            /// </summary>
            /// <typeparam name="T">The type of the items in the <see cref="ReadOnlySequence{T}"/>.</typeparam>
            /// <param name="sequence">The read-only sequence to convert to an array.</param>
            /// <returns>An array containing the data in the current read-only sequence.</returns>
            public static T[] ToArray<T>(this in ReadOnlySequence<T> sequence)
            {
                T[] array = new T[sequence.Length];
                CopyTo(in sequence, array);
                return array;
            }

            /// <summary>
            /// Writes the contents of <paramref name="value"/> to <paramref name="writer"/>.
            /// </summary>
            /// <typeparam name="T">The type of the items in the <see cref="ReadOnlySpan{T}"/>.</typeparam>
            /// <param name="writer">The buffer writer to which to write <paramref name="value"/>.</param>
            /// <param name="value">The read-only span to be written to <paramref name="writer"/>.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void Write<T>(this IBufferWriter<T> writer, ReadOnlySpan<T> value)
            {
                Span<T> span = writer.GetSpan();
                if (value.Length <= span.Length)
                {
                    value.CopyTo(span);
                    writer.Advance(value.Length);
                }
                else
                {
                    WriteMultiSegment(writer, in value, span);
                }
            }

            private static void WriteMultiSegment<T>(IBufferWriter<T> writer, in ReadOnlySpan<T> source, Span<T> destination)
            {
                ReadOnlySpan<T> readOnlySpan = source;
                while (true)
                {
                    int num = Math.Min(destination.Length, readOnlySpan.Length);
                    readOnlySpan.Slice(0, num).CopyTo(destination);
                    writer.Advance(num);
                    readOnlySpan = readOnlySpan.Slice(num);
                    if (readOnlySpan.Length > 0)
                    {
                        destination = writer.GetSpan(readOnlySpan.Length);
                        continue;
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Represents an output sink into which <typeparamref name="T"/> data can be written.
        /// </summary>
        /// <typeparam name="T">The type of the items in the <see cref="IBufferWriter{T}"/>.</typeparam>
        public interface IBufferWriter<T>
        {
            /// <summary>
            /// Notifies the <see cref="IBufferWriter{T}"/> that <paramref name="count"/> data 
            /// items were written to the output <see cref="Span{T}"/> or <see cref="Memory{T}"/>.
            /// </summary>
            /// <param name="count">The number of data items written to the <see cref="Span{T}"/> 
            /// or <see cref="Memory{T}"/>.</param>
            /// <remarks>You must request a new buffer after calling <see cref="Advance(int)"/> to continue writing 
            /// more data; you cannot write to a previously acquired buffer.</remarks>
            void Advance(int count);

            /// <summary>
            /// Returns a <see cref="Memory{T}"/> to write to that is at least the requested size (specified by <paramref name="sizeHint"/>).
            /// </summary>
            /// <param name="sizeHint">The minimum length of the returned <see cref="Memory{T}"/>. If 0, a non-empty buffer is returned.</param>
            /// <returns>A <see cref="Memory{T}"/> of at least the size <paramref name="sizeHint"/>. If <paramref name="sizeHint"/> is 0, 
            /// returns a non-empty buffer.</returns>
            /// <exception cref="System.OutOfMemoryException">The requested buffer size is not available.</exception>
            /// <remarks>
            /// There is no guarantee that successive calls will return the same buffer or the same-sized buffer. <br />
            /// This must never return <see cref="Span{T}.Empty"/>, but it can throw if the requested buffer size 
            /// is not available. <br />
            /// You must request a new buffer after calling Advance to continue writing more data; you cannot write 
            /// to a previously acquired buffer.<br />
            /// </remarks>
            Memory<T> GetMemory(int sizeHint = 0);

            /// <summary>
            /// Returns a <see cref="Span{T}"/> to write to that is at least the requested size (specified by <paramref name="sizeHint"/>).
            /// </summary>
            /// <param name="sizeHint">The minimum length of the returned <see cref="Memory{T}"/>. If 0, a non-empty buffer is returned.</param>
            /// <returns>A <see cref="Span{T}"/> of at least the size <paramref name="sizeHint"/>. If <paramref name="sizeHint"/> is 0, 
            /// returns a non-empty buffer.</returns>
            /// <exception cref="System.OutOfMemoryException">The requested buffer size is not available.</exception>
            /// <remarks>
            /// There is no guarantee that successive calls will return the same buffer or the same-sized buffer. <br />
            /// This must never return <see cref="Span{T}.Empty"/>, but it can throw if the requested buffer size 
            /// is not available. <br />
            /// You must request a new buffer after calling Advance to continue writing more data; you cannot write 
            /// to a previously acquired buffer.<br />
            /// </remarks>
            Span<T> GetSpan(int sizeHint = 0);
        }

        internal static class Utilities
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static int SelectBucketIndex(int bufferSize)
            {
                uint num = (uint)(bufferSize - 1) >> 4;
                int num2 = 0;
                if (num > 65535)
                {
                    num >>= 16;
                    num2 = 16;
                }
                if (num > 255)
                {
                    num >>= 8;
                    num2 += 8;
                }
                if (num > 15)
                {
                    num >>= 4;
                    num2 += 4;
                }
                if (num > 3)
                {
                    num >>= 2;
                    num2 += 2;
                }
                if (num > 1)
                {
                    num >>= 1;
                    num2++;
                }
                return num2 + (int)num;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static int GetMaxSizeForBucket(int binIndex)
            {
                return 16 << binIndex;
            }
        }

        internal sealed class DefaultArrayPool<T> : ArrayPool<T>
        {
            private sealed class Bucket
            {
                internal readonly int _bufferLength;

                private readonly T[][] _buffers;

                private readonly int _poolId;

                private SpinLock _lock;

                private int _index;

                internal int Id => GetHashCode();

                internal Bucket(int bufferLength, int numberOfBuffers, int poolId)
                {
                    _lock = new SpinLock(System.Diagnostics.Debugger.IsAttached);
                    _buffers = new T[numberOfBuffers][];
                    _bufferLength = bufferLength;
                    _poolId = poolId;
                }

                internal T[] Rent()
                {
                    T[][] buffers = _buffers;
                    T[] array = null;
                    bool lockTaken = false;
                    bool flag = false;
                    try
                    {
                        _lock.Enter(ref lockTaken);
                        if (_index < buffers.Length)
                        {
                            array = buffers[_index];
                            buffers[_index++] = null;
                            flag = array == null;
                        }
                    }
                    finally
                    {
                        if (lockTaken)
                        {
                            _lock.Exit(useMemoryBarrier: false);
                        }
                    }
                    if (flag)
                    {
                        array = new T[_bufferLength];
                        ArrayPoolEventSource log = ArrayPoolEventSource.Log;
                        if (log.IsEnabled())
                        {
                            log.BufferAllocated(array.GetHashCode(), _bufferLength, _poolId, Id, BufferAllocatedReason.Pooled);
                        }
                    }
                    return array;
                }

                internal void Return(T[] array)
                {
                    if (array.Length != _bufferLength)
                    {
                        throw new ArgumentException(MDCFR.Properties.Resources.ArgumentException_BufferNotFromPool, "array");
                    }
                    bool lockTaken = false;
                    try
                    {
                        _lock.Enter(ref lockTaken);
                        if (_index != 0)
                        {
                            _buffers[--_index] = array;
                        }
                    }
                    finally
                    {
                        if (lockTaken)
                        {
                            _lock.Exit(useMemoryBarrier: false);
                        }
                    }
                }
            }

            private const int DefaultMaxArrayLength = 1048576;

            private const int DefaultMaxNumberOfArraysPerBucket = 50;

            private static T[] s_emptyArray;

            private readonly Bucket[] _buckets;

            private int Id => GetHashCode();

            internal DefaultArrayPool()
                : this(1048576, 50)
            {
            }

            internal DefaultArrayPool(int maxArrayLength, int maxArraysPerBucket)
            {
                if (maxArrayLength <= 0)
                {
                    throw new ArgumentOutOfRangeException("maxArrayLength");
                }
                if (maxArraysPerBucket <= 0)
                {
                    throw new ArgumentOutOfRangeException("maxArraysPerBucket");
                }
                if (maxArrayLength > 1073741824)
                {
                    maxArrayLength = 1073741824;
                }
                else if (maxArrayLength < 16)
                {
                    maxArrayLength = 16;
                }
                int id = Id;
                int num = Utilities.SelectBucketIndex(maxArrayLength);
                Bucket[] array = new Bucket[num + 1];
                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = new Bucket(Utilities.GetMaxSizeForBucket(i), maxArraysPerBucket, id);
                }
                _buckets = array;
            }

            public override T[] Rent(int minimumLength)
            {
                if (minimumLength < 0)
                {
                    throw new ArgumentOutOfRangeException("minimumLength");
                }
                if (minimumLength == 0)
                {
                    return s_emptyArray ?? (s_emptyArray = new T[0]);
                }
                ArrayPoolEventSource log = ArrayPoolEventSource.Log;
                T[] array = null;
                int num = Utilities.SelectBucketIndex(minimumLength);
                if (num < _buckets.Length)
                {
                    int num2 = num;
                    do
                    {
                        array = _buckets[num2].Rent();
                        if (array != null)
                        {
                            if (log.IsEnabled())
                            {
                                log.BufferRented(array.GetHashCode(), array.Length, Id, _buckets[num2].Id);
                            }
                            return array;
                        }
                    }
                    while (++num2 < _buckets.Length && num2 != num + 2);
                    array = new T[_buckets[num]._bufferLength];
                }
                else
                {
                    array = new T[minimumLength];
                }
                if (log.IsEnabled())
                {
                    int hashCode = array.GetHashCode();
                    int bucketId = -1;
                    log.BufferRented(hashCode, array.Length, Id, bucketId);
                    log.BufferAllocated(hashCode, array.Length, Id, bucketId, (num >= _buckets.Length) ? BufferAllocatedReason.OverMaximumSize : BufferAllocatedReason.PoolExhausted);
                }
                return array;
            }

            public override void Return(T[] array, bool clearArray = false)
            {
                if (array == null)
                {
                    throw new ArgumentNullException("array");
                }
                if (array.Length == 0)
                {
                    return;
                }
                int num = Utilities.SelectBucketIndex(array.Length);
                if (num < _buckets.Length)
                {
                    if (clearArray)
                    {
                        Array.Clear(array, 0, array.Length);
                    }
                    _buckets[num].Return(array);
                }
                ArrayPoolEventSource log = ArrayPoolEventSource.Log;
                if (log.IsEnabled())
                {
                    log.BufferReturned(array.GetHashCode(), array.Length, Id);
                }
            }
        }

        /// <summary>
        /// Provides a resource pool that enables reusing instances of arrays.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Renting and returning buffers with an <see cref="ArrayPool{T}"/> can increase performance
        /// in situations where arrays are created and destroyed frequently, resulting in significant
        /// memory pressure on the garbage collector.
        /// </para>
        /// <para>
        /// This class is thread-safe.  All members may be used by multiple threads concurrently.
        /// </para>
        /// </remarks>
        public abstract class ArrayPool<T>
        {
            private static ArrayPool<T> s_sharedInstance;

            /// <summary>
            /// Retrieves a shared <see cref="ArrayPool{T}"/> instance.
            /// </summary>
            /// <remarks>
            /// The shared pool provides a default implementation of <see cref="ArrayPool{T}"/>
            /// that's intended for general applicability.  It maintains arrays of multiple sizes, and
            /// may hand back a larger array than was actually requested, but will never hand back a smaller
            /// array than was requested. Renting a buffer from it with <see cref="Rent"/> will result in an
            /// existing buffer being taken from the pool if an appropriate buffer is available or in a new
            /// buffer being allocated if one is not available.
            /// byte[] and char[] are the most commonly pooled array types. For these we use a special pool type
            /// optimized for very fast access speeds, at the expense of more memory consumption.
            /// The shared pool instance is created lazily on first access.
            /// </remarks>
            public static ArrayPool<T> Shared
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    return Volatile.Read(ref s_sharedInstance) ?? EnsureSharedCreated();
                }
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            private static ArrayPool<T> EnsureSharedCreated()
            {
                Interlocked.CompareExchange(ref s_sharedInstance, Create(), null);
                return s_sharedInstance;
            }

            /// <summary>
            /// Creates a new <see cref="ArrayPool{T}"/> instance using default configuration options.
            /// </summary>
            /// <returns>A new <see cref="ArrayPool{T}"/> instance.</returns>
            public static ArrayPool<T> Create()
            {
                return new DefaultArrayPool<T>();
            }

            /// <summary>
            /// Creates a new <see cref="ArrayPool{T}"/> instance using custom configuration options.
            /// </summary>
            /// <param name="maxArrayLength">The maximum length of array instances that may be stored in the pool.</param>
            /// <param name="maxArraysPerBucket">
            /// The maximum number of array instances that may be stored in each bucket in the pool.  The pool
            /// groups arrays of similar lengths into buckets for faster access.
            /// </param>
            /// <returns>A new <see cref="ArrayPool{T}"/> instance with the specified configuration options.</returns>
            /// <remarks>
            /// The created pool will group arrays into buckets, with no more than <paramref name="maxArraysPerBucket"/>
            /// in each bucket and with those arrays not exceeding <paramref name="maxArrayLength"/> in length.
            /// </remarks>
            public static ArrayPool<T> Create(int maxArrayLength, int maxArraysPerBucket)
            {
                return new DefaultArrayPool<T>(maxArrayLength, maxArraysPerBucket);
            }

            /// <summary>
            /// Retrieves a buffer that is at least the requested length.
            /// </summary>
            /// <param name="minimumLength">The minimum length of the array needed.</param>
            /// <returns>
            /// An array that is at least <paramref name="minimumLength"/> in length.
            /// </returns>
            /// <remarks>
            /// This buffer is loaned to the caller and should be returned to the same pool via
            /// <see cref="Return"/> so that it may be reused in subsequent usage of <see cref="Rent"/>.
            /// It is not a fatal error to not return a rented buffer, but failure to do so may lead to
            /// decreased application performance, as the pool may need to create a new buffer to replace
            /// the one lost.
            /// </remarks>
            public abstract T[] Rent(int minimumLength);

            /// <summary>
            /// Returns to the pool an array that was previously obtained via <see cref="Rent"/> on the same
            /// <see cref="ArrayPool{T}"/> instance.
            /// </summary>
            /// <param name="array">
            /// The buffer previously obtained from <see cref="Rent"/> to return to the pool.
            /// </param>
            /// <param name="clearArray">
            /// If <c>true</c> and if the pool will store the buffer to enable subsequent reuse, <see cref="Return"/>
            /// will clear <paramref name="array"/> of its contents so that a subsequent consumer via <see cref="Rent"/>
            /// will not see the previous consumer's content.  If <c>false</c> or if the pool will release the buffer,
            /// the array's contents are left unchanged.
            /// </param>
            /// <remarks>
            /// Once a buffer has been returned to the pool, the caller gives up all ownership of the buffer
            /// and must not use it. The reference returned from a given call to <see cref="Rent"/> must only be
            /// returned via <see cref="Return"/> once.  The default <see cref="ArrayPool{T}"/>
            /// may hold onto the returned buffer in order to rent it again, or it may release the returned buffer
            /// if it's determined that the pool already has enough buffers stored.
            /// </remarks>
            public abstract void Return(T[] array, bool clearArray = false);
        }

        internal enum BufferAllocatedReason
        {
            Pooled,
            OverMaximumSize,
            PoolExhausted
        }

        [EventSource(Name = "System.Buffers.ArrayPoolEventSource")]
        internal sealed class ArrayPoolEventSource : EventSource
        {

            internal static readonly ArrayPoolEventSource Log = new ArrayPoolEventSource();

            [Event(1, Level = EventLevel.Verbose)]
            internal unsafe void BufferRented(int bufferId, int bufferSize, int poolId, int bucketId)
            {
                EventData* ptr = stackalloc EventData[4];
                *ptr = new EventData
                {
                    Size = 4,
                    DataPointer = (IntPtr)(&bufferId)
                };
                ptr[1] = new EventData
                {
                    Size = 4,
                    DataPointer = (IntPtr)(&bufferSize)
                };
                ptr[2] = new EventData
                {
                    Size = 4,
                    DataPointer = (IntPtr)(&poolId)
                };
                ptr[3] = new EventData
                {
                    Size = 4,
                    DataPointer = (IntPtr)(&bucketId)
                };
                WriteEventCore(1, 4, ptr);
            }

            [Event(2, Level = EventLevel.Informational)]
            internal unsafe void BufferAllocated(int bufferId, int bufferSize, int poolId, int bucketId, BufferAllocatedReason reason)
            {
                EventData* ptr = stackalloc EventData[5];
                *ptr = new EventData
                {
                    Size = 4,
                    DataPointer = (IntPtr)(&bufferId)
                };
                ptr[1] = new EventData
                {
                    Size = 4,
                    DataPointer = (IntPtr)(&bufferSize)
                };
                ptr[2] = new EventData
                {
                    Size = 4,
                    DataPointer = (IntPtr)(&poolId)
                };
                ptr[3] = new EventData
                {
                    Size = 4,
                    DataPointer = (IntPtr)(&bucketId)
                };
                ptr[4] = new EventData
                {
                    Size = 4,
                    DataPointer = (IntPtr)(&reason)
                };
                WriteEventCore(2, 5, ptr);
            }

            [Event(3, Level = EventLevel.Verbose)]
            internal void BufferReturned(int bufferId, int bufferSize, int poolId)
            {
                WriteEvent(3, bufferId, bufferSize, poolId);
            }
        }

        internal sealed class ReadOnlySequenceDebugView<T>
        {
            [System.Diagnostics.DebuggerDisplay("Count: {Segments.Length}", Name = "Segments")]
            public struct ReadOnlySequenceDebugViewSegments
            {
                [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.RootHidden)]
                public ReadOnlyMemory<T>[] Segments { get; set; }
            }

            private readonly T[] _array;

            private readonly ReadOnlySequenceDebugViewSegments _segments;

            public ReadOnlySequenceDebugViewSegments BufferSegments => _segments;

            [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.RootHidden)]
            public T[] Items => _array;

            public ReadOnlySequenceDebugView(ReadOnlySequence<T> sequence)
            {
                _array = BuffersExtensions.ToArray(in sequence);
                int num = 0;
                ReadOnlySequence<T>.Enumerator enumerator = sequence.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    ReadOnlyMemory<T> current = enumerator.Current;
                    num++;
                }
                ReadOnlyMemory<T>[] array = new ReadOnlyMemory<T>[num];
                int num2 = 0;
                ReadOnlySequence<T>.Enumerator enumerator2 = sequence.GetEnumerator();
                while (enumerator2.MoveNext())
                {
                    ReadOnlyMemory<T> current2 = enumerator2.Current;
                    array[num2] = current2;
                    num2++;
                }
                _segments = new ReadOnlySequenceDebugViewSegments
                {
                    Segments = array
                };
            }
        }

        /// <summary>
        /// Represents a linked list of <see cref="ReadOnlyMemory{T}"/> nodes.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the read-only sequence segment.</typeparam>
        public abstract class ReadOnlySequenceSegment<T>
        {
            /// <summary>
            /// Gets or sets a <see cref="ReadOnlyMemory{T}"/> value for the current node.
            /// </summary>
            public ReadOnlyMemory<T> Memory { get; protected set; }

            /// <summary>
            /// Gets or sets the next node.
            /// </summary>
            public ReadOnlySequenceSegment<T> Next { get; protected set; }

            /// <summary>
            /// Gets or sets the sum of node lengths before the current node.
            /// </summary>
            public long RunningIndex { get; protected set; }
        }

        /// <summary>
        /// Represents a sequence that can read a sequential series of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the read-only sequence.</typeparam>
        [System.Diagnostics.DebuggerTypeProxy(typeof(ReadOnlySequenceDebugView<>))]
        [System.Diagnostics.DebuggerDisplay("{ToString(),raw}")]
        public readonly struct ReadOnlySequence<T>
        {

            /// <summary>
            /// Represents an enumerator over a <see cref="ReadOnlySequence{T}"/>.
            /// </summary>
            public struct Enumerator
            {
                private readonly ReadOnlySequence<T> _sequence;

                private SequencePosition _next;

                private ReadOnlyMemory<T> _currentMemory;

                /// <summary>
                /// Gets the current <see cref="ReadOnlyMemory{T}"/>.
                /// </summary>
                public ReadOnlyMemory<T> Current => _currentMemory;

                /// <summary>
                /// Initializes the enumerator.
                /// </summary>
                /// <param name="sequence">The <see cref="ReadOnlySequence{T}"/> to enumerate.</param>
                public Enumerator(in ReadOnlySequence<T> sequence)
                {
                    _currentMemory = default(ReadOnlyMemory<T>);
                    _next = sequence.Start;
                    _sequence = sequence;
                }

                /// <summary>
                /// Moves to the next <see cref="ReadOnlyMemory{T}"/> in the <see cref="ReadOnlySequence{T}"/>.
                /// </summary>
                /// <returns><see langword="true"/> if the enumerator successfully advanced to the next item; 
                /// <see langword="false"/> if the end of the sequence has been reached.</returns>
                public bool MoveNext()
                {
                    if (_next.GetObject() == null)
                    {
                        return false;
                    }
                    return _sequence.TryGet(ref _next, out _currentMemory);
                }
            }

            private enum SequenceType
            {
                MultiSegment,
                Array,
                MemoryManager,
                String,
                Empty
            }

            private readonly SequencePosition _sequenceStart;

            private readonly SequencePosition _sequenceEnd;

            /// <summary>
            /// Returns an empty <see cref="ReadOnlySequence{T}"/>.
            /// </summary>
            public static readonly ReadOnlySequence<T> Empty = new ReadOnlySequence<T>(SpanHelpers.PerTypeValues<T>.EmptyArray);

            /// <summary>
            /// Gets the length of the <see cref="ReadOnlySequence{T}"/>.
            /// </summary>
            public long Length => GetLength();

            /// <summary>
            /// Gets a value that indicates whether the <see cref="ReadOnlySequence{T}"/> is empty.
            /// </summary>
            public bool IsEmpty => Length == 0;

            /// <summary>
            /// Gets a value that indicates whether the <see cref="ReadOnlySequence{T}"/> contains a single <see cref="ReadOnlyMemory{T}"/> segment.
            /// </summary>
            public bool IsSingleSegment
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get { return _sequenceStart.GetObject() == _sequenceEnd.GetObject(); }
            }

            /// <summary>
            /// Gets the <see cref="ReadOnlyMemory{T}"/> from the first segment.
            /// </summary>
            public ReadOnlyMemory<T> First => GetFirstBuffer();

            /// <summary>
            /// Gets the position to the start of the <see cref="ReadOnlySequence{T}"/>.
            /// </summary>
            public SequencePosition Start => _sequenceStart;

            /// <summary>
            /// Gets the position at the end of the <see cref="ReadOnlySequence{T}"/>.
            /// </summary>
            public SequencePosition End => _sequenceEnd;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private ReadOnlySequence(object startSegment, int startIndexAndFlags, object endSegment, int endIndexAndFlags)
            {
                _sequenceStart = new SequencePosition(startSegment, startIndexAndFlags);
                _sequenceEnd = new SequencePosition(endSegment, endIndexAndFlags);
            }

            /// <summary>
            /// Creates an instance of a <see cref="ReadOnlySequence{T}"/> from a linked memory list represented by start and end segments and the corresponding indexes in them.
            /// </summary>
            /// <param name="startSegment">The initial node of the linked memory list.</param>
            /// <param name="startIndex">The position to the start of the sequence inside <paramref name="startSegment"/>.</param>
            /// <param name="endSegment">The final node of the linked memory list.</param>
            /// <param name="endIndex">The position to the end of the sequence inside <paramref name="endSegment"/>.</param>
            /// <exception cref="System.ArgumentNullException"><paramref name="startSegment"/> or <paramref name="endSegment"/> is <see langword="null"/>.</exception>
            /// <exception cref="System.ArgumentOutOfRangeException">
            /// The running index of <paramref name="startSegment"/> is greater than the running index 
            /// of <paramref name="endSegment"/>, even though <paramref name="startSegment"/> is different to <paramref name="endSegment"/>.<br /> <br />
            /// -or- <br /> <br />
            /// <paramref name="startSegment"/> is equal to <paramref name="endSegment"/> but <paramref name="endIndex"/> is smaller than <paramref name="startIndex"/>. <br /> <br />
            /// -or- <br /> <br />
            /// <paramref name="startIndex"/> is greater than the length of the underlying memory block of <paramref name="startSegment"/>.
            /// </exception>
            public ReadOnlySequence(ReadOnlySequenceSegment<T> startSegment, int startIndex, ReadOnlySequenceSegment<T> endSegment, int endIndex)
            {
                if (startSegment == null || endSegment == null || (startSegment != endSegment && startSegment.RunningIndex > endSegment.RunningIndex) || (uint)startSegment.Memory.Length < (uint)startIndex || (uint)endSegment.Memory.Length < (uint)endIndex || (startSegment == endSegment && endIndex < startIndex))
                {
                    System.ThrowHelper.ThrowArgumentValidationException(startSegment, startIndex, endSegment);
                }
                _sequenceStart = new SequencePosition(startSegment, ReadOnlySequence.SegmentToSequenceStart(startIndex));
                _sequenceEnd = new SequencePosition(endSegment, ReadOnlySequence.SegmentToSequenceEnd(endIndex));
            }

            /// <summary>
            /// Creates an instance of <see cref="ReadOnlySequence{T}"/> from the <paramref name="array"/>.
            /// </summary>
            /// <param name="array">The array from which to create a read-only sequence.</param>
            public ReadOnlySequence(T[] array)
            {
                if (array == null)
                {
                    System.ThrowHelper.ThrowArgumentNullException(System.ExceptionArgument.array);
                }
                _sequenceStart = new SequencePosition(array, ReadOnlySequence.ArrayToSequenceStart(0));
                _sequenceEnd = new SequencePosition(array, ReadOnlySequence.ArrayToSequenceEnd(array.Length));
            }

            /// <summary>
            /// Creates an instance of <see cref="ReadOnlySequence{T}"/> from a section of the <paramref name="array"/>.
            /// </summary>
            /// <param name="array">The array from which to create a read-only sequence.</param>
            /// <param name="start">The zero-based index of the first element in the array to include in the read-only sequence.</param>
            /// <param name="length">The number of elements to include in the read-only sequence.</param>
            public ReadOnlySequence(T[] array, int start, int length)
            {
                if (array == null || (uint)start > (uint)array.Length || (uint)length > (uint)(array.Length - start))
                {
                    System.ThrowHelper.ThrowArgumentValidationException(array, start);
                }
                _sequenceStart = new SequencePosition(array, ReadOnlySequence.ArrayToSequenceStart(start));
                _sequenceEnd = new SequencePosition(array, ReadOnlySequence.ArrayToSequenceEnd(start + length));
            }

            /// <summary>
            /// Creates an instance of <see cref="ReadOnlySequence{T}"/> from a <see cref="ReadOnlyMemory{T}"/>.
            /// </summary>
            /// <param name="memory">A read-only block of memory of elements of type <typeparamref name="T"/>.</param>
            /// <remarks>The consumer is expected to manage the lifetime of memory until <see cref="ReadOnlySequence{T}"/> is not used anymore.</remarks>
            public ReadOnlySequence(ReadOnlyMemory<T> memory)
            {
                ArraySegment<T> segment;
                if (MemoryMarshal.TryGetMemoryManager<T, MemoryManager<T>>(memory, out var manager, out var start, out var length))
                {
                    _sequenceStart = new SequencePosition(manager, ReadOnlySequence.MemoryManagerToSequenceStart(start));
                    _sequenceEnd = new SequencePosition(manager, ReadOnlySequence.MemoryManagerToSequenceEnd(start + length));
                }
                else if (MemoryMarshal.TryGetArray(memory, out segment))
                {
                    T[] array = segment.Array;
                    int offset = segment.Offset;
                    _sequenceStart = new SequencePosition(array, ReadOnlySequence.ArrayToSequenceStart(offset));
                    _sequenceEnd = new SequencePosition(array, ReadOnlySequence.ArrayToSequenceEnd(offset + segment.Count));
                }
                else if (typeof(T) == typeof(char))
                {
                    if (!MemoryMarshal.TryGetString((ReadOnlyMemory<char>)(object)memory, out var text, out var start2, out length))
                    {
                        System.ThrowHelper.ThrowInvalidOperationException();
                    }
                    _sequenceStart = new SequencePosition(text, ReadOnlySequence.StringToSequenceStart(start2));
                    _sequenceEnd = new SequencePosition(text, ReadOnlySequence.StringToSequenceEnd(start2 + length));
                }
                else
                {
                    System.ThrowHelper.ThrowInvalidOperationException();
                    _sequenceStart = default(SequencePosition);
                    _sequenceEnd = default(SequencePosition);
                }
            }

            /// <summary>
            /// Forms a slice out of the current <see cref="ReadOnlySequence{T}"/>, beginning at <paramref name="start"/>, with <paramref name="length"/> items.
            /// </summary>
            /// <param name="start">The index at which to begin this slice.</param>
            /// <param name="length">The length of the slice.</param>
            /// <returns>A slice that consists of <paramref name="length"/> elements from the current instance starting at index <paramref name="start"/>.</returns>
            public ReadOnlySequence<T> Slice(long start, long length)
            {
                if (start < 0 || length < 0)
                {
                    System.ThrowHelper.ThrowStartOrEndArgumentValidationException(start);
                }
                int index = GetIndex(in _sequenceStart);
                int index2 = GetIndex(in _sequenceEnd);
                object @object = _sequenceStart.GetObject();
                object object2 = _sequenceEnd.GetObject();
                SequencePosition start2;
                SequencePosition end;
                if (@object != object2)
                {
                    ReadOnlySequenceSegment<T> readOnlySequenceSegment = (ReadOnlySequenceSegment<T>)@object;
                    int num = readOnlySequenceSegment.Memory.Length - index;
                    if (num > start)
                    {
                        index += (int)start;
                        start2 = new SequencePosition(@object, index);
                        end = GetEndPosition(readOnlySequenceSegment, @object, index, object2, index2, length);
                    }
                    else
                    {
                        if (num < 0)
                        {
                            System.ThrowHelper.ThrowArgumentOutOfRangeException_PositionOutOfRange();
                        }
                        start2 = SeekMultiSegment(readOnlySequenceSegment.Next, object2, index2, start - num, System.ExceptionArgument.start);
                        int index3 = GetIndex(in start2);
                        object object3 = start2.GetObject();
                        if (object3 != object2)
                        {
                            end = GetEndPosition((ReadOnlySequenceSegment<T>)object3, object3, index3, object2, index2, length);
                        }
                        else
                        {
                            if (index2 - index3 < length)
                            {
                                System.ThrowHelper.ThrowStartOrEndArgumentValidationException(0L);
                            }
                            end = new SequencePosition(object3, index3 + (int)length);
                        }
                    }
                }
                else
                {
                    if (index2 - index < start)
                    {
                        System.ThrowHelper.ThrowStartOrEndArgumentValidationException(-1L);
                    }
                    index += (int)start;
                    start2 = new SequencePosition(@object, index);
                    if (index2 - index < length)
                    {
                        System.ThrowHelper.ThrowStartOrEndArgumentValidationException(0L);
                    }
                    end = new SequencePosition(@object, index + (int)length);
                }
                return SliceImpl(in start2, in end);
            }

            /// <summary>
            /// Forms a slice out of the current <see cref="ReadOnlySequence{T}"/>, beginning at <paramref name="start"/> and ending at <paramref name="end"/> (exclusive).
            /// </summary>
            /// <param name="start">The index at which to begin this slice.</param>
            /// <param name="end">The end (exclusive) <see cref="SequencePosition"/> of the slice.</param>
            /// <returns>A slice that consists of items from the <paramref name="start"/> index to, but not including, the <paramref name="end"/> sequence position in the current read-only sequence.</returns>
            public ReadOnlySequence<T> Slice(long start, SequencePosition end)
            {
                if (start < 0)
                {
                    System.ThrowHelper.ThrowStartOrEndArgumentValidationException(start);
                }
                uint index = (uint)GetIndex(in end);
                object @object = end.GetObject();
                uint index2 = (uint)GetIndex(in _sequenceStart);
                object object2 = _sequenceStart.GetObject();
                uint index3 = (uint)GetIndex(in _sequenceEnd);
                object object3 = _sequenceEnd.GetObject();
                if (object2 == object3)
                {
                    if (!InRange(index, index2, index3))
                    {
                        System.ThrowHelper.ThrowArgumentOutOfRangeException_PositionOutOfRange();
                    }
                    if (index - index2 < start)
                    {
                        System.ThrowHelper.ThrowStartOrEndArgumentValidationException(-1L);
                    }
                }
                else
                {
                    ReadOnlySequenceSegment<T> readOnlySequenceSegment = (ReadOnlySequenceSegment<T>)object2;
                    ulong num = (ulong)(readOnlySequenceSegment.RunningIndex + index2);
                    ulong num2 = (ulong)(((ReadOnlySequenceSegment<T>)@object).RunningIndex + index);
                    if (!InRange(num2, num, (ulong)(((ReadOnlySequenceSegment<T>)object3).RunningIndex + index3)))
                    {
                        System.ThrowHelper.ThrowArgumentOutOfRangeException_PositionOutOfRange();
                    }
                    if ((ulong)((long)num + start) > num2)
                    {
                        System.ThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.start);
                    }
                    int num3 = readOnlySequenceSegment.Memory.Length - (int)index2;
                    if (num3 <= start)
                    {
                        if (num3 < 0)
                        {
                            System.ThrowHelper.ThrowArgumentOutOfRangeException_PositionOutOfRange();
                        }
                        SequencePosition start2 = SeekMultiSegment(readOnlySequenceSegment.Next, @object, (int)index, start - num3, System.ExceptionArgument.start);
                        return SliceImpl(in start2, in end);
                    }
                }
                SequencePosition start3 = new SequencePosition(object2, (int)index2 + (int)start);
                return SliceImpl(in start3, in end);
            }

            /// <summary>
            /// Forms a slice out of the current <see cref="ReadOnlySequence{T}"/>, beginning at <paramref name="start"/>, with <paramref name="length"/> items.
            /// </summary>
            /// <param name="start">The starting (inclusive) <see cref="SequencePosition"/> at which to begin this slice.</param>
            /// <param name="length">The length of the slice.</param>
            /// <returns>A slice that consists of <paramref name="length"/> elements from the current instance starting at sequence position <paramref name="start"/>.</returns>
            public ReadOnlySequence<T> Slice(SequencePosition start, long length)
            {
                uint index = (uint)GetIndex(in start);
                object @object = start.GetObject();
                uint index2 = (uint)GetIndex(in _sequenceStart);
                object object2 = _sequenceStart.GetObject();
                uint index3 = (uint)GetIndex(in _sequenceEnd);
                object object3 = _sequenceEnd.GetObject();
                if (object2 == object3)
                {
                    if (!InRange(index, index2, index3))
                    {
                        System.ThrowHelper.ThrowArgumentOutOfRangeException_PositionOutOfRange();
                    }
                    if (length < 0)
                    {
                        System.ThrowHelper.ThrowStartOrEndArgumentValidationException(0L);
                    }
                    if (index3 - index < length)
                    {
                        System.ThrowHelper.ThrowStartOrEndArgumentValidationException(0L);
                    }
                }
                else
                {
                    ReadOnlySequenceSegment<T> readOnlySequenceSegment = (ReadOnlySequenceSegment<T>)@object;
                    ulong num = (ulong)(readOnlySequenceSegment.RunningIndex + index);
                    ulong start2 = (ulong)(((ReadOnlySequenceSegment<T>)object2).RunningIndex + index2);
                    ulong num2 = (ulong)(((ReadOnlySequenceSegment<T>)object3).RunningIndex + index3);
                    if (!InRange(num, start2, num2))
                    {
                        System.ThrowHelper.ThrowArgumentOutOfRangeException_PositionOutOfRange();
                    }
                    if (length < 0)
                    {
                        System.ThrowHelper.ThrowStartOrEndArgumentValidationException(0L);
                    }
                    if ((ulong)((long)num + length) > num2)
                    {
                        System.ThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.length);
                    }
                    int num3 = readOnlySequenceSegment.Memory.Length - (int)index;
                    if (num3 < length)
                    {
                        if (num3 < 0)
                        {
                            System.ThrowHelper.ThrowArgumentOutOfRangeException_PositionOutOfRange();
                        }
                        SequencePosition end = SeekMultiSegment(readOnlySequenceSegment.Next, object3, (int)index3, length - num3, System.ExceptionArgument.length);
                        return SliceImpl(in start, in end);
                    }
                }
                SequencePosition end2 = new SequencePosition(@object, (int)index + (int)length);
                return SliceImpl(in start, in end2);
            }

            /// <summary>
            /// Forms a slice out of the current <see cref="ReadOnlySequence{T}"/>, beginning at <paramref name="start"/>, with <paramref name="length"/> items.
            /// </summary>
            /// <param name="start">The index at which to begin this slice.</param>
            /// <param name="length">The length of the slice.</param>
            /// <returns>A slice that consists of <paramref name="length"/> elements from the current instance starting at index <paramref name="start"/>.</returns>
            public ReadOnlySequence<T> Slice(int start, int length) { return Slice((long)start, (long)length); }

            /// <summary>
            /// Forms a slice out of the current <see cref="ReadOnlySequence{T}"/>, beginning at <paramref name="start"/> and ending at <paramref name="end"/> (exclusive).
            /// </summary>
            /// <param name="start">The index at which to begin this slice.</param>
            /// <param name="end">The end (exclusive) <see cref="SequencePosition"/> of the slice.</param>
            /// <returns>A slice that consists of items from the <paramref name="start"/> index to, but not including, the <paramref name="end"/> sequence position in the current read-only sequence.</returns>
            public ReadOnlySequence<T> Slice(int start, SequencePosition end) { return Slice((long)start, end); }

            /// <summary>
            /// Forms a slice out of the current <see cref="ReadOnlySequence{T}"/>, beginning at <paramref name="start"/>, with <paramref name="length"/> items.
            /// </summary>
            /// <param name="start">The starting (inclusive) <see cref="SequencePosition"/> at which to begin this slice.</param>
            /// <param name="length">The length of the slice.</param>
            /// <returns>A slice that consists of <paramref name="length"/> elements from the current instance starting at sequence position <paramref name="start"/>.</returns>
            public ReadOnlySequence<T> Slice(SequencePosition start, int length) { return Slice(start, (long)length); }

            /// <summary>
            /// Forms a slice out of the current <see cref="ReadOnlySequence{T}"/>, beginning at <paramref name="start"/> and ending at <paramref name="end"/> (exclusive). 
            /// </summary>
            /// <param name="start">The starting (inclusive) <see cref="SequencePosition"/> at which to begin this slice.</param>
            /// <param name="end">The ending (exclusive) <see cref="SequencePosition"/> of the slice. </param>
            /// <returns>A slice that consists of items from the <paramref name="start"/> sequence position to, but not including, the <paramref name="end"/> sequence position in the current read-only sequence.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnlySequence<T> Slice(SequencePosition start, SequencePosition end)
            {
                BoundsCheck((uint)GetIndex(in start), start.GetObject(), (uint)GetIndex(in end), end.GetObject());
                return SliceImpl(in start, in end);
            }

            /// <summary>
            /// Forms a slice out of the current <see cref="ReadOnlySequence{T}"/>, beginning at a specified sequence position and continuing to the end of the read-only sequence.
            /// </summary>
            /// <param name="start">The starting (inclusive) <see cref="SequencePosition"/> at which to begin this slice.</param>
            /// <returns>A slice starting at sequence position <paramref name="start"/> and continuing to the end of the current read-only sequence.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnlySequence<T> Slice(SequencePosition start) { BoundsCheck(in start); return SliceImpl(in start, in _sequenceEnd); }

            /// <summary>
            /// Forms a slice out of the current <see cref="ReadOnlySequence{T}"/>, beginning at a specified index and continuing to the end of the read-only sequence.
            /// </summary>
            /// <param name="start">The start index at which to begin this slice.</param>
            /// <returns>A slice starting at index <paramref name="start"/> and continuing to the end of the current read-only sequence.</returns>
            public ReadOnlySequence<T> Slice(long start)
            {
                if (start < 0)
                {
                    System.ThrowHelper.ThrowStartOrEndArgumentValidationException(start);
                }
                if (start == 0L)
                {
                    return this;
                }
                SequencePosition start2 = Seek(in _sequenceStart, in _sequenceEnd, start, System.ExceptionArgument.start);
                return SliceImpl(in start2, in _sequenceEnd);
            }

            /// <summary>
            /// Returns a string that represents the current sequence.
            /// </summary>
            /// <returns>A string that represents the current sequence.</returns>
            public override string ToString()
            {
                if (typeof(T) == typeof(char))
                {
                    ReadOnlySequence<T> source = this;
                    ReadOnlySequence<char> sequence = Unsafe.As<ReadOnlySequence<T>, ReadOnlySequence<char>>(ref source);
                    if (SequenceMarshal.TryGetString(sequence, out var text, out var start, out var length))
                    {
                        return text.Substring(start, length);
                    }
                    if (Length < int.MaxValue)
                    {
                        return new string(BuffersExtensions.ToArray(in sequence));
                    }
                }
                return $"System.Buffers.ReadOnlySequence<{typeof(T).Name}>[{Length}]";
            }

            /// <summary>
            /// Returns an enumerator over the <see cref="ReadOnlySequence{T}"/>.
            /// </summary>
            /// <returns>Returns an enumerator over the <see cref="ReadOnlySequence{T}"/>.</returns>
            public Enumerator GetEnumerator() { return new Enumerator(in this); }

            /// <summary>
            /// Returns a new <see cref="SequencePosition"/> at an <paramref name="offset"/> from the start of the sequence.
            /// </summary>
            /// <param name="offset">The offset from the start of the sequence.</param>
            /// <returns>An object representing the sequence position that starts at the specified <paramref name="offset"/> from the start of the sequence.</returns>
            public SequencePosition GetPosition(long offset) { return GetPosition(offset, _sequenceStart); }

            /// <summary>
            /// Returns a new SequencePosition starting at the specified offset from the <paramref name="origin"/> position.
            /// </summary>
            /// <param name="offset">The offset from the specified <paramref name="origin"/> sequence position.</param>
            /// <param name="origin">A sequence position representing the point from which to initiate the offset.</param>
            /// <returns>An object representing the sequence position that starts at the <paramref name="offset"/> position of the specified <paramref name="origin"/> position object.</returns>
            public SequencePosition GetPosition(long offset, SequencePosition origin)
            {
                if (offset < 0)
                {
                    System.ThrowHelper.ThrowArgumentOutOfRangeException_OffsetOutOfRange();
                }
                return Seek(in origin, in _sequenceEnd, offset, System.ExceptionArgument.offset);
            }

            /// <summary>
            /// Tries to retrieve the next segment after <paramref name="position"/> and returns a value that indicates whether the operation succeeded.
            /// </summary>
            /// <param name="position">The current sequence position.</param>
            /// <param name="memory">A read-only memory span that contains the next segment after <paramref name="position"/>.</param>
            /// <param name="advance"><see langword="true"/> if <paramref name="position"/> should to be in the beginning of next segment; otherwise, <see langword="false"/>.</param>
            /// <returns>Returns <see langword="true"/> if the method returned the next segment, or <see langword="false"/> if the end of the read-only sequence was reached.</returns>
            public bool TryGet(ref SequencePosition position, out ReadOnlyMemory<T> memory, bool advance = true)
            {
                SequencePosition next;
                bool result = TryGetBuffer(in position, out memory, out next);
                if (advance)
                {
                    position = next;
                }
                return result;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal bool TryGetBuffer(in SequencePosition position, out ReadOnlyMemory<T> memory, out SequencePosition next)
            {
                object @object = position.GetObject();
                next = default(SequencePosition);
                if (@object == null)
                {
                    memory = default(ReadOnlyMemory<T>);
                    return false;
                }
                SequenceType sequenceType = GetSequenceType();
                object object2 = _sequenceEnd.GetObject();
                int index = GetIndex(in position);
                int index2 = GetIndex(in _sequenceEnd);
                if (sequenceType == SequenceType.MultiSegment)
                {
                    ReadOnlySequenceSegment<T> readOnlySequenceSegment = (ReadOnlySequenceSegment<T>)@object;
                    if (readOnlySequenceSegment != object2)
                    {
                        ReadOnlySequenceSegment<T> next2 = readOnlySequenceSegment.Next;
                        if (next2 == null)
                        {
                            System.ThrowHelper.ThrowInvalidOperationException_EndPositionNotReached();
                        }
                        next = new SequencePosition(next2, 0);
                        memory = readOnlySequenceSegment.Memory.Slice(index);
                    }
                    else
                    {
                        memory = readOnlySequenceSegment.Memory.Slice(index, index2 - index);
                    }
                }
                else
                {
                    if (@object != object2)
                    {
                        System.ThrowHelper.ThrowInvalidOperationException_EndPositionNotReached();
                    }
                    if (sequenceType == SequenceType.Array)
                    {
                        memory = new ReadOnlyMemory<T>((T[])@object, index, index2 - index);
                    }
                    else if (typeof(T) == typeof(char) && sequenceType == SequenceType.String)
                    {
                        memory = (ReadOnlyMemory<T>)(object)((string)@object).AsMemory(index, index2 - index);
                    }
                    else
                    {
                        memory = ((MemoryManager<T>)@object).Memory.Slice(index, index2 - index);
                    }
                }
                return true;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private ReadOnlyMemory<T> GetFirstBuffer()
            {
                object @object = _sequenceStart.GetObject();
                if (@object == null)
                {
                    return default(ReadOnlyMemory<T>);
                }
                int integer = _sequenceStart.GetInteger();
                int integer2 = _sequenceEnd.GetInteger();
                bool flag = @object != _sequenceEnd.GetObject();
                if (integer >= 0)
                {
                    if (integer2 >= 0)
                    {
                        ReadOnlyMemory<T> memory = ((ReadOnlySequenceSegment<T>)@object).Memory;
                        if (flag)
                        {
                            return memory.Slice(integer);
                        }
                        return memory.Slice(integer, integer2 - integer);
                    }
                    if (flag)
                    {
                        System.ThrowHelper.ThrowInvalidOperationException_EndPositionNotReached();
                    }
                    return new ReadOnlyMemory<T>((T[])@object, integer, (integer2 & 0x7FFFFFFF) - integer);
                }
                if (flag)
                {
                    System.ThrowHelper.ThrowInvalidOperationException_EndPositionNotReached();
                }
                if (typeof(T) == typeof(char) && integer2 < 0)
                {
                    return (ReadOnlyMemory<T>)(object)((string)@object).AsMemory(integer & 0x7FFFFFFF, integer2 - integer);
                }
                integer &= 0x7FFFFFFF;
                return ((MemoryManager<T>)@object).Memory.Slice(integer, integer2 - integer);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private SequencePosition Seek(in SequencePosition start, in SequencePosition end, long offset, System.ExceptionArgument argument)
            {
                int index = GetIndex(in start);
                int index2 = GetIndex(in end);
                object @object = start.GetObject();
                object object2 = end.GetObject();
                if (@object != object2)
                {
                    ReadOnlySequenceSegment<T> readOnlySequenceSegment = (ReadOnlySequenceSegment<T>)@object;
                    int num = readOnlySequenceSegment.Memory.Length - index;
                    if (num <= offset)
                    {
                        if (num < 0)
                        {
                            System.ThrowHelper.ThrowArgumentOutOfRangeException_PositionOutOfRange();
                        }
                        return SeekMultiSegment(readOnlySequenceSegment.Next, object2, index2, offset - num, argument);
                    }
                }
                else if (index2 - index < offset)
                {
                    System.ThrowHelper.ThrowArgumentOutOfRangeException(argument);
                }
                return new SequencePosition(@object, index + (int)offset);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            private static SequencePosition SeekMultiSegment(ReadOnlySequenceSegment<T> currentSegment, object endObject, int endIndex, long offset, System.ExceptionArgument argument)
            {
                while (true)
                {
                    if (currentSegment != null && currentSegment != endObject)
                    {
                        int length = currentSegment.Memory.Length;
                        if (length > offset)
                        {
                            break;
                        }
                        offset -= length;
                        currentSegment = currentSegment.Next;
                        continue;
                    }
                    if (currentSegment == null || endIndex < offset)
                    {
                        System.ThrowHelper.ThrowArgumentOutOfRangeException(argument);
                    }
                    break;
                }
                return new SequencePosition(currentSegment, (int)offset);
            }

            private void BoundsCheck(in SequencePosition position)
            {
                uint index = (uint)GetIndex(in position);
                uint index2 = (uint)GetIndex(in _sequenceStart);
                uint index3 = (uint)GetIndex(in _sequenceEnd);
                object @object = _sequenceStart.GetObject();
                object object2 = _sequenceEnd.GetObject();
                if (@object == object2)
                {
                    if (!InRange(index, index2, index3))
                    {
                        System.ThrowHelper.ThrowArgumentOutOfRangeException_PositionOutOfRange();
                    }
                    return;
                }
                ulong start = (ulong)(((ReadOnlySequenceSegment<T>)@object).RunningIndex + index2);
                if (!InRange((ulong)(((ReadOnlySequenceSegment<T>)position.GetObject()).RunningIndex + index), start, (ulong)(((ReadOnlySequenceSegment<T>)object2).RunningIndex + index3)))
                {
                    System.ThrowHelper.ThrowArgumentOutOfRangeException_PositionOutOfRange();
                }
            }

            private void BoundsCheck(uint sliceStartIndex, object sliceStartObject, uint sliceEndIndex, object sliceEndObject)
            {
                uint index = (uint)GetIndex(in _sequenceStart);
                uint index2 = (uint)GetIndex(in _sequenceEnd);
                object @object = _sequenceStart.GetObject();
                object object2 = _sequenceEnd.GetObject();
                if (@object == object2)
                {
                    if (sliceStartObject != sliceEndObject || sliceStartObject != @object || sliceStartIndex > sliceEndIndex || sliceStartIndex < index || sliceEndIndex > index2)
                    {
                        System.ThrowHelper.ThrowArgumentOutOfRangeException_PositionOutOfRange();
                    }
                    return;
                }
                ulong num = (ulong)(((ReadOnlySequenceSegment<T>)sliceStartObject).RunningIndex + sliceStartIndex);
                ulong num2 = (ulong)(((ReadOnlySequenceSegment<T>)sliceEndObject).RunningIndex + sliceEndIndex);
                if (num > num2)
                {
                    System.ThrowHelper.ThrowArgumentOutOfRangeException_PositionOutOfRange();
                }
                if (num < (ulong)(((ReadOnlySequenceSegment<T>)@object).RunningIndex + index) || num2 > (ulong)(((ReadOnlySequenceSegment<T>)object2).RunningIndex + index2))
                {
                    System.ThrowHelper.ThrowArgumentOutOfRangeException_PositionOutOfRange();
                }
            }

            private static SequencePosition GetEndPosition(ReadOnlySequenceSegment<T> startSegment, object startObject, int startIndex, object endObject, int endIndex, long length)
            {
                int num = startSegment.Memory.Length - startIndex;
                if (num > length)
                {
                    return new SequencePosition(startObject, startIndex + (int)length);
                }
                if (num < 0)
                {
                    System.ThrowHelper.ThrowArgumentOutOfRangeException_PositionOutOfRange();
                }
                return SeekMultiSegment(startSegment.Next, endObject, endIndex, length - num, System.ExceptionArgument.length);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private SequenceType GetSequenceType() { return (SequenceType)(-(2 * (_sequenceStart.GetInteger() >> 31) + (_sequenceEnd.GetInteger() >> 31))); }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static int GetIndex(in SequencePosition position) { return position.GetInteger() & 0x7FFFFFFF; }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private ReadOnlySequence<T> SliceImpl(in SequencePosition start, in SequencePosition end)
            {
                return new ReadOnlySequence<T>(start.GetObject(), GetIndex(in start) | (_sequenceStart.GetInteger() & int.MinValue), end.GetObject(), GetIndex(in end) | (_sequenceEnd.GetInteger() & int.MinValue));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private long GetLength()
            {
                int index = GetIndex(in _sequenceStart);
                int index2 = GetIndex(in _sequenceEnd);
                object @object = _sequenceStart.GetObject();
                object object2 = _sequenceEnd.GetObject();
                if (@object != object2)
                {
                    ReadOnlySequenceSegment<T> readOnlySequenceSegment = (ReadOnlySequenceSegment<T>)@object;
                    ReadOnlySequenceSegment<T> readOnlySequenceSegment2 = (ReadOnlySequenceSegment<T>)object2;
                    return readOnlySequenceSegment2.RunningIndex + index2 - (readOnlySequenceSegment.RunningIndex + index);
                }
                return index2 - index;
            }

            internal bool TryGetReadOnlySequenceSegment(out ReadOnlySequenceSegment<T> startSegment, out int startIndex, out ReadOnlySequenceSegment<T> endSegment, out int endIndex)
            {
                object @object = _sequenceStart.GetObject();
                if (@object == null || GetSequenceType() != 0)
                {
                    startSegment = null;
                    startIndex = 0;
                    endSegment = null;
                    endIndex = 0;
                    return false;
                }
                startSegment = (ReadOnlySequenceSegment<T>)@object;
                startIndex = GetIndex(in _sequenceStart);
                endSegment = (ReadOnlySequenceSegment<T>)_sequenceEnd.GetObject();
                endIndex = GetIndex(in _sequenceEnd);
                return true;
            }

            internal bool TryGetArray(out ArraySegment<T> segment)
            {
                if (GetSequenceType() != SequenceType.Array)
                {
                    segment = default(ArraySegment<T>);
                    return false;
                }
                int index = GetIndex(in _sequenceStart);
                segment = new ArraySegment<T>((T[])_sequenceStart.GetObject(), index, GetIndex(in _sequenceEnd) - index);
                return true;
            }

            internal bool TryGetString(out string text, out int start, out int length)
            {
                if (typeof(T) != typeof(char) || GetSequenceType() != SequenceType.String)
                {
                    start = 0;
                    length = 0;
                    text = null;
                    return false;
                }
                start = GetIndex(in _sequenceStart);
                length = GetIndex(in _sequenceEnd) - start;
                text = (string)_sequenceStart.GetObject();
                return true;
            }

            private static bool InRange(uint value, uint start, uint end) { return value - start <= end - start; }

            private static bool InRange(ulong value, ulong start, ulong end) { return value - start <= end - start; }
        }

        internal static class ReadOnlySequence
        {
            public const int FlagBitMask = int.MinValue;

            public const int IndexBitMask = int.MaxValue;

            public const int SegmentStartMask = 0;

            public const int SegmentEndMask = 0;

            public const int ArrayStartMask = 0;

            public const int ArrayEndMask = int.MinValue;

            public const int MemoryManagerStartMask = int.MinValue;

            public const int MemoryManagerEndMask = 0;

            public const int StringStartMask = int.MinValue;

            public const int StringEndMask = int.MinValue;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int SegmentToSequenceStart(int startIndex)
            {
                return startIndex | 0;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int SegmentToSequenceEnd(int endIndex)
            {
                return endIndex | 0;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int ArrayToSequenceStart(int startIndex)
            {
                return startIndex | 0;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int ArrayToSequenceEnd(int endIndex)
            {
                return endIndex | int.MinValue;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int MemoryManagerToSequenceStart(int startIndex)
            {
                return startIndex | int.MinValue;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int MemoryManagerToSequenceEnd(int endIndex)
            {
                return endIndex | 0;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int StringToSequenceStart(int startIndex)
            {
                return startIndex | int.MinValue;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int StringToSequenceEnd(int endIndex)
            {
                return endIndex | int.MinValue;
            }
        }

        /// <summary>
        /// An abstract base class that is used to replace the implementation of <see cref="Memory{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of items in the memory buffer managed by this memory manager.</typeparam>
        public abstract class MemoryManager<T> : IMemoryOwner<T>, IDisposable, IPinnable
        {
            /// <summary>
            /// Gets the memory block handled by this <see cref="MemoryManager{T}"/>.
            /// </summary>
            public virtual Memory<T> Memory => new Memory<T>(this, GetSpan().Length);

            /// <summary>
            /// Returns a memory span that wraps the underlying memory buffer.
            /// </summary>
            /// <returns>A memory span that wraps the underlying memory buffer.</returns>
            public abstract Span<T> GetSpan();

            /// <summary>
            /// Returns a handle to the memory that has been pinned and whose address can be taken.
            /// </summary>
            /// <param name="elementIndex">The offset to the element in the memory buffer at which the returned <see cref="MemoryHandle"/> points.</param>
            /// <returns>A handle to the memory that has been pinned.</returns>
            public abstract MemoryHandle Pin(int elementIndex = 0);

            /// <summary>
            /// Unpins pinned memory so that the garbage collector is free to move it.
            /// </summary>
            public abstract void Unpin();

            /// <summary>
            /// Returns a memory buffer consisting of a specified number of elements from the memory managed by the current memory manager.
            /// </summary>
            /// <param name="length">The number of elements in the memory buffer, starting at offset 0.</param>
            /// <returns>A memory buffer.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected Memory<T> CreateMemory(int length) { return new Memory<T>(this, length); }

            /// <summary>
            /// Returns a memory buffer consisting of a specified number of elements starting at a specified offset from the memory managed by the current memory manager.
            /// </summary>
            /// <param name="start">The offset to the element at which the returned memory buffer starts.</param>
            /// <param name="length">The number of elements to include in the returned memory buffer.</param>
            /// <returns>A memory buffer that consists of <paramref name="length"/> elements starting at offset <paramref name="start"/>.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected Memory<T> CreateMemory(int start, int length) { return new Memory<T>(this, start, length); }

            /// <summary>
            /// Returns an array segment.
            /// </summary>
            /// <param name="segment">The array segment to write to.</param>
            /// <returns><see langword="true"/> if the method succeeded in retrieving the array segment; otherwise, <see langword="false"/>.</returns>
            /// <remarks>If this method is not overridden, it returns the default array segment.</remarks>
            protected internal virtual bool TryGetArray(out ArraySegment<T> segment)
            {
                segment = default(ArraySegment<T>);
                return false;
            }

            void IDisposable.Dispose()
            {
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }

            /// <summary>
            /// Releases all resources used by the current memory manager.
            /// </summary>
            /// <param name="disposing"><see langword="true"/> to release 
            /// both managed and unmanaged resources; 
            /// <see langword="false"/> to release only 
            /// unmanaged resources.</param>
            protected abstract void Dispose(bool disposing);
        }

        /// <summary>
        /// Represents a heap-based, array-backed output sink into which <typeparam name="T" /> data can be written.
        /// </summary>
        public sealed class ArrayBufferWriter<T> : IBufferWriter<T>
        {
            private const int ArrayMaxLength = 2147483591;

            private const int DefaultInitialBufferSize = 256;

            private T[] _buffer;

            private int _index;

            /// <summary>
            /// Returns the data written to the underlying buffer so far, as a <see cref="T:System.ReadOnlyMemory`1" />.
            /// </summary>
            public ReadOnlyMemory<T> WrittenMemory => _buffer.AsMemory(0, _index);

            /// <summary>
            /// Returns the data written to the underlying buffer so far, as a <see cref="T:System.ReadOnlySpan`1" />.
            /// </summary>
            public ReadOnlySpan<T> WrittenSpan => _buffer.AsSpan(0, _index);

            /// <summary>
            /// Returns the amount of data written to the underlying buffer so far.
            /// </summary>
            public int WrittenCount => _index;

            /// <summary>
            /// Returns the total amount of space within the underlying buffer.
            /// </summary>
            public int Capacity => _buffer.Length;

            /// <summary>
            /// Returns the amount of space available that can still be written into without forcing the underlying buffer to grow.
            /// </summary>
            public int FreeCapacity => _buffer.Length - _index;

            /// <summary>
            /// Creates an instance of an <see cref="System.Buffers.ArrayBufferWriter{T}" />, in which data can be written to,
            /// with the default initial capacity.
            /// </summary>
            public ArrayBufferWriter()
            {
                _buffer = Array.Empty<T>();
                _index = 0;
            }

            /// <summary>
            /// Creates an instance of an <see cref="System.Buffers.ArrayBufferWriter{T}" />, in which data can be written to,
            /// with an initial capacity specified.
            /// </summary>
            /// <param name="initialCapacity">The minimum capacity with which to initialize the underlying buffer.</param>
            /// <exception cref="System.ArgumentException">
            /// Thrown when <paramref name="initialCapacity" /> is not positive (i.e. less than or equal to 0).
            /// </exception>
            public ArrayBufferWriter(int initialCapacity)
            {
                if (initialCapacity <= 0)
                {
                    throw new ArgumentException(null, "initialCapacity");
                }
                _buffer = new T[initialCapacity];
                _index = 0;
            }

            /// <summary>
            /// Clears the data written to the underlying buffer.
            /// </summary>
            /// <remarks>
            /// You must clear the <see cref="T:System.Buffers.ArrayBufferWriter`1" /> before trying to re-use it.
            /// </remarks>
            public void Clear()
            {
                _buffer.AsSpan(0, _index).Clear();
                _index = 0;
            }

            /// <summary>
            /// Notifies <see cref="System.Buffers.IBufferWriter{T}" /> that <paramref name="count" /> amount of data was written to the output <see cref="T:System.Span`1" />/<see cref="T:System.Memory`1" />
            /// </summary>
            /// <exception cref="System.ArgumentException">
            /// Thrown when <paramref name="count" /> is negative.
            /// </exception>
            /// <exception cref="System.InvalidOperationException">
            /// Thrown when attempting to advance past the end of the underlying buffer.
            /// </exception>
            /// <remarks>
            /// You must request a new buffer after calling Advance to continue writing more data and cannot write to a previously acquired buffer.
            /// </remarks>
            public void Advance(int count)
            {
                if (count < 0)
                {
                    throw new ArgumentException(null, "count");
                }
                if (_index > _buffer.Length - count)
                {
                    ThrowInvalidOperationException_AdvancedTooFar(_buffer.Length);
                }
                _index += count;
            }

            /// <summary>
            /// Returns a <see cref="T:System.Memory`1" /> to write to that is at least the requested length (specified by <paramref name="sizeHint" />).
            /// If no <paramref name="sizeHint" /> is provided (or it's equal to <code>0</code>), some non-empty buffer is returned.
            /// </summary>
            /// <exception cref="T:System.ArgumentException">
            /// Thrown when <paramref name="sizeHint" /> is negative.
            /// </exception>
            /// <remarks>
            /// This will never return an empty <see cref="T:System.Memory`1" />.
            /// </remarks>
            /// <remarks>
            /// There is no guarantee that successive calls will return the same buffer or the same-sized buffer.
            /// </remarks>
            /// <remarks>
            /// You must request a new buffer after calling Advance to continue writing more data and cannot write to a previously acquired buffer.
            /// </remarks>
            public Memory<T> GetMemory(int sizeHint = 0)
            {
                CheckAndResizeBuffer(sizeHint);
                return _buffer.AsMemory(_index);
            }

            /// <summary>
            /// Returns a <see cref="T:System.Span`1" /> to write to that is at least the requested length (specified by <paramref name="sizeHint" />).
            /// If no <paramref name="sizeHint" /> is provided (or it's equal to <code>0</code>), some non-empty buffer is returned.
            /// </summary>
            /// <exception cref="T:System.ArgumentException">
            /// Thrown when <paramref name="sizeHint" /> is negative.
            /// </exception>
            /// <remarks>
            /// This will never return an empty <see cref="T:System.Span`1" />.
            /// </remarks>
            /// <remarks>
            /// There is no guarantee that successive calls will return the same buffer or the same-sized buffer.
            /// </remarks>
            /// <remarks>
            /// You must request a new buffer after calling Advance to continue writing more data and cannot write to a previously acquired buffer.
            /// </remarks>
            public Span<T> GetSpan(int sizeHint = 0)
            {
                CheckAndResizeBuffer(sizeHint);
                return _buffer.AsSpan(_index);
            }

            private void CheckAndResizeBuffer(int sizeHint)
            {
                if (sizeHint < 0)
                {
                    throw new ArgumentException("sizeHint");
                }
                if (sizeHint == 0)
                {
                    sizeHint = 1;
                }
                if (sizeHint <= FreeCapacity)
                {
                    return;
                }
                int num = _buffer.Length;
                int num2 = Math.Max(sizeHint, num);
                if (num == 0)
                {
                    num2 = Math.Max(num2, 256);
                }
                int num3 = num + num2;
                if ((uint)num3 > 2147483647u)
                {
                    uint num4 = (uint)(num - FreeCapacity + sizeHint);
                    if (num4 > 2147483591)
                    {
                        ThrowOutOfMemoryException(num4);
                    }
                    num3 = 2147483591;
                }
                Array.Resize(ref _buffer, num3);
            }

            private static void ThrowInvalidOperationException_AdvancedTooFar(int capacity)
            {
                throw new InvalidOperationException(System.SR.Format(MDCFR.Properties.Resources.BufferWriterAdvancedTooFar, capacity));
            }

            private static void ThrowOutOfMemoryException(uint capacity)
            {
                throw new OutOfMemoryException(System.SR.Format(MDCFR.Properties.Resources.BufferMaximumSizeExceeded, capacity));
            }
        }

    }

    /// <summary>
    /// Represents a position in a non-contiguous set of memory. 
    /// Properties of this type should not be interpreted by anything but the type that created it.
    /// </summary>
    public readonly struct SequencePosition : IEquatable<SequencePosition>
    {
        private readonly object _object;

        private readonly int _integer;

        /// <summary>
        /// Initializes a new instance of the <see cref="SequencePosition"/> struct.
        /// </summary>
        /// <param name="object">A non-contiguous set of memory.</param>
        /// <param name="integer">The position in <paramref name="object"/>.</param>
        public SequencePosition(object @object, int integer)
        {
            _object = @object;
            _integer = integer;
        }

        /// <summary>
        /// Returns the object part of this <see cref="SequencePosition"/> struct.
        /// </summary>
        /// <returns>The object part of this sequence position.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public object GetObject()
        {
            return _object;
        }

        /// <summary>
        /// Returns the integer part of this <see cref="SequencePosition"/> struct.
        /// </summary>
        /// <returns>The integer part of this sequence position.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public int GetInteger()
        {
            return _integer;
        }

        /// <summary>
        /// Indicates whether the current instance is equal to another <see cref="SequencePosition"/> struct.
        /// </summary>
        /// <param name="other">The sequence position to compare with the current instance.</param>
        /// <returns><c>true</c> if the two instances are equal; <c>false</c> otherwise.</returns>
        /// <remarks>
        /// Equality does not guarantee that the two instances point to the same location in a <see cref="Buffers.ReadOnlySequence{T}"/>.
        /// </remarks>
        public bool Equals(SequencePosition other)
        {
            if (_integer == other._integer)
            {
                return object.Equals(_object, other._object);
            }
            return false;
        }

        /// <summary>
        /// Returns a value that indicates whether the current instance is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><c>true</c> if <paramref name="obj"/> is of type <see cref="SequencePosition"/>
        /// and is equal to the current instance; otherwise, <c>false</c>.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
        {
            if (obj is SequencePosition other)
            {
                return Equals(other);
            }
            return false;
        }

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            return System.Numerics.Hashing.HashHelpers.Combine(_object?.GetHashCode() ?? 0, _integer);
        }
    }


}