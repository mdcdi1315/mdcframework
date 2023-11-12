// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Buffers;
using System.Numerics;
using System.Reflection;
using System.Diagnostics;
using System.Globalization;
using System.ComponentModel;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace System
{

    internal static class SpanHelpers
    {

        internal struct ComparerComparable<T, TComparer> : IComparable<T> where TComparer : IComparer<T>
        {
            private readonly T _value;

            private readonly TComparer _comparer;

            public ComparerComparable(T value, TComparer comparer)
            {
                _value = value;
                _comparer = comparer;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int CompareTo(T other)
            {
                TComparer comparer = _comparer;
                return comparer.Compare(_value, other);
            }
        }

        [StructLayout(LayoutKind.Sequential, Size = 64)]
        private struct Reg64
        {
        }

        [StructLayout(LayoutKind.Sequential, Size = 32)]
        private struct Reg32
        {
        }

        [StructLayout(LayoutKind.Sequential, Size = 16)]
        private struct Reg16
        {
        }

        public static class PerTypeValues<T>
        {
            public static readonly bool IsReferenceOrContainsReferences = IsReferenceOrContainsReferencesCore(typeof(T));

            public static readonly T[] EmptyArray = new T[0];

            public static readonly IntPtr ArrayAdjustment = MeasureArrayAdjustment();

            private static IntPtr MeasureArrayAdjustment()
            {
                T[] array = new T[1];
                return Unsafe.ByteOffset(ref Unsafe.As<Pinnable<T>>(array).Data, ref array[0]);
            }
        }

        private const ulong XorPowerOfTwoToHighByte = 283686952306184uL;

        private const ulong XorPowerOfTwoToHighChar = 4295098372uL;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BinarySearch<T, TComparable>(this ReadOnlySpan<T> span, TComparable comparable) where TComparable : IComparable<T>
        {
            if (comparable == null)
            {
                System.ThrowHelper.ThrowArgumentNullException(System.ExceptionArgument.comparable);
            }
            return BinarySearch(ref MemoryMarshal.GetReference(span), span.Length, comparable);
        }

        public static int BinarySearch<T, TComparable>(ref T spanStart, int length, TComparable comparable) where TComparable : IComparable<T>
        {
            int num = 0;
            int num2 = length - 1;
            while (num <= num2)
            {
                int num3 = (int)((uint)(num2 + num) >> 1);
                int num4 = comparable.CompareTo(Unsafe.Add(ref spanStart, num3));
                if (num4 == 0)
                {
                    return num3;
                }
                if (num4 > 0)
                {
                    num = num3 + 1;
                }
                else
                {
                    num2 = num3 - 1;
                }
            }
            return ~num;
        }

        public static int IndexOf(ref byte searchSpace, int searchSpaceLength, ref byte value, int valueLength)
        {
            if (valueLength == 0)
            {
                return 0;
            }
            byte value2 = value;
            ref byte second = ref Unsafe.Add(ref value, 1);
            int num = valueLength - 1;
            int num2 = 0;
            while (true)
            {
                int num3 = searchSpaceLength - num2 - num;
                if (num3 <= 0)
                {
                    break;
                }
                int num4 = IndexOf(ref Unsafe.Add(ref searchSpace, num2), value2, num3);
                if (num4 == -1)
                {
                    break;
                }
                num2 += num4;
                if (SequenceEqual(ref Unsafe.Add(ref searchSpace, num2 + 1), ref second, num))
                {
                    return num2;
                }
                num2++;
            }
            return -1;
        }

        public static int IndexOfAny(ref byte searchSpace, int searchSpaceLength, ref byte value, int valueLength)
        {
            if (valueLength == 0)
            {
                return 0;
            }
            int num = -1;
            for (int i = 0; i < valueLength; i++)
            {
                int num2 = IndexOf(ref searchSpace, Unsafe.Add(ref value, i), searchSpaceLength);
                if ((uint)num2 < (uint)num)
                {
                    num = num2;
                    searchSpaceLength = num2;
                    if (num == 0)
                    {
                        break;
                    }
                }
            }
            return num;
        }

        public static int LastIndexOfAny(ref byte searchSpace, int searchSpaceLength, ref byte value, int valueLength)
        {
            if (valueLength == 0)
            {
                return 0;
            }
            int num = -1;
            for (int i = 0; i < valueLength; i++)
            {
                int num2 = LastIndexOf(ref searchSpace, Unsafe.Add(ref value, i), searchSpaceLength);
                if (num2 > num)
                {
                    num = num2;
                }
            }
            return num;
        }

        public unsafe static int IndexOf(ref byte searchSpace, byte value, int length)
        {
            IntPtr intPtr = (IntPtr)0;
            IntPtr intPtr2 = (IntPtr)length;
            if (Vector.IsHardwareAccelerated && length >= Vector<byte>.Count * 2)
            {
                int num = (int)Unsafe.AsPointer(ref searchSpace) & (Vector<byte>.Count - 1);
                intPtr2 = (IntPtr)((Vector<byte>.Count - num) & (Vector<byte>.Count - 1));
            }
            while (true)
            {
                if ((nuint)(void*)intPtr2 >= (nuint)8u)
                {
                    intPtr2 -= 8;
                    if (value == Unsafe.AddByteOffset(ref searchSpace, intPtr))
                    {
                        goto IL_0242;
                    }
                    if (value == Unsafe.AddByteOffset(ref searchSpace, intPtr + 1))
                    {
                        goto IL_024a;
                    }
                    if (value == Unsafe.AddByteOffset(ref searchSpace, intPtr + 2))
                    {
                        goto IL_0258;
                    }
                    if (value != Unsafe.AddByteOffset(ref searchSpace, intPtr + 3))
                    {
                        if (value != Unsafe.AddByteOffset(ref searchSpace, intPtr + 4))
                        {
                            if (value != Unsafe.AddByteOffset(ref searchSpace, intPtr + 5))
                            {
                                if (value != Unsafe.AddByteOffset(ref searchSpace, intPtr + 6))
                                {
                                    if (value == Unsafe.AddByteOffset(ref searchSpace, intPtr + 7))
                                    {
                                        break;
                                    }
                                    intPtr += 8;
                                    continue;
                                }
                                return (int)(void*)(intPtr + 6);
                            }
                            return (int)(void*)(intPtr + 5);
                        }
                        return (int)(void*)(intPtr + 4);
                    }
                    goto IL_0266;
                }
                if ((nuint)(void*)intPtr2 >= (nuint)4u)
                {
                    intPtr2 -= 4;
                    if (value == Unsafe.AddByteOffset(ref searchSpace, intPtr))
                    {
                        goto IL_0242;
                    }
                    if (value == Unsafe.AddByteOffset(ref searchSpace, intPtr + 1))
                    {
                        goto IL_024a;
                    }
                    if (value == Unsafe.AddByteOffset(ref searchSpace, intPtr + 2))
                    {
                        goto IL_0258;
                    }
                    if (value == Unsafe.AddByteOffset(ref searchSpace, intPtr + 3))
                    {
                        goto IL_0266;
                    }
                    intPtr += 4;
                }
                while ((void*)intPtr2 != null)
                {
                    intPtr2 -= 1;
                    if (value != Unsafe.AddByteOffset(ref searchSpace, intPtr))
                    {
                        intPtr += 1;
                        continue;
                    }
                    goto IL_0242;
                }
                if (Vector.IsHardwareAccelerated && (int)(void*)intPtr < length)
                {
                    intPtr2 = (IntPtr)((length - (int)(void*)intPtr) & ~(Vector<byte>.Count - 1));
                    Vector<byte> vector = GetVector(value);
                    for (; (void*)intPtr2 > (void*)intPtr; intPtr += Vector<byte>.Count)
                    {
                        Vector<byte> vector2 = Vector.Equals(vector, Unsafe.ReadUnaligned<Vector<byte>>(ref Unsafe.AddByteOffset(ref searchSpace, intPtr)));
                        if (!Vector<byte>.Zero.Equals(vector2))
                        {
                            return (int)(void*)intPtr + LocateFirstFoundByte(vector2);
                        }
                    }
                    if ((int)(void*)intPtr < length)
                    {
                        intPtr2 = (IntPtr)(length - (int)(void*)intPtr);
                        continue;
                    }
                }
                return -1;
            IL_0266:
                return (int)(void*)(intPtr + 3);
            IL_0242:
                return (int)(void*)intPtr;
            IL_0258:
                return (int)(void*)(intPtr + 2);
            IL_024a:
                return (int)(void*)(intPtr + 1);
            }
            return (int)(void*)(intPtr + 7);
        }

        public static int LastIndexOf(ref byte searchSpace, int searchSpaceLength, ref byte value, int valueLength)
        {
            if (valueLength == 0)
            {
                return 0;
            }
            byte value2 = value;
            ref byte second = ref Unsafe.Add(ref value, 1);
            int num = valueLength - 1;
            int num2 = 0;
            while (true)
            {
                int num3 = searchSpaceLength - num2 - num;
                if (num3 <= 0)
                {
                    break;
                }
                int num4 = LastIndexOf(ref searchSpace, value2, num3);
                if (num4 == -1)
                {
                    break;
                }
                if (SequenceEqual(ref Unsafe.Add(ref searchSpace, num4 + 1), ref second, num))
                {
                    return num4;
                }
                num2 += num3 - num4;
            }
            return -1;
        }

        public unsafe static int LastIndexOf(ref byte searchSpace, byte value, int length)
        {
            IntPtr intPtr = (IntPtr)length;
            IntPtr intPtr2 = (IntPtr)length;
            if (Vector.IsHardwareAccelerated && length >= Vector<byte>.Count * 2)
            {
                int num = (int)Unsafe.AsPointer(ref searchSpace) & (Vector<byte>.Count - 1);
                intPtr2 = (IntPtr)(((length & (Vector<byte>.Count - 1)) + num) & (Vector<byte>.Count - 1));
            }
            while (true)
            {
                if ((nuint)(void*)intPtr2 >= (nuint)8u)
                {
                    intPtr2 -= 8;
                    intPtr -= 8;
                    if (value == Unsafe.AddByteOffset(ref searchSpace, intPtr + 7))
                    {
                        break;
                    }
                    if (value != Unsafe.AddByteOffset(ref searchSpace, intPtr + 6))
                    {
                        if (value != Unsafe.AddByteOffset(ref searchSpace, intPtr + 5))
                        {
                            if (value != Unsafe.AddByteOffset(ref searchSpace, intPtr + 4))
                            {
                                if (value != Unsafe.AddByteOffset(ref searchSpace, intPtr + 3))
                                {
                                    if (value != Unsafe.AddByteOffset(ref searchSpace, intPtr + 2))
                                    {
                                        if (value != Unsafe.AddByteOffset(ref searchSpace, intPtr + 1))
                                        {
                                            if (value != Unsafe.AddByteOffset(ref searchSpace, intPtr))
                                            {
                                                continue;
                                            }
                                            goto IL_0254;
                                        }
                                        goto IL_025c;
                                    }
                                    goto IL_026a;
                                }
                                goto IL_0278;
                            }
                            return (int)(void*)(intPtr + 4);
                        }
                        return (int)(void*)(intPtr + 5);
                    }
                    return (int)(void*)(intPtr + 6);
                }
                if ((nuint)(void*)intPtr2 >= (nuint)4u)
                {
                    intPtr2 -= 4;
                    intPtr -= 4;
                    if (value == Unsafe.AddByteOffset(ref searchSpace, intPtr + 3))
                    {
                        goto IL_0278;
                    }
                    if (value == Unsafe.AddByteOffset(ref searchSpace, intPtr + 2))
                    {
                        goto IL_026a;
                    }
                    if (value == Unsafe.AddByteOffset(ref searchSpace, intPtr + 1))
                    {
                        goto IL_025c;
                    }
                    if (value == Unsafe.AddByteOffset(ref searchSpace, intPtr))
                    {
                        goto IL_0254;
                    }
                }
                while ((void*)intPtr2 != null)
                {
                    intPtr2 -= 1;
                    intPtr -= 1;
                    if (value != Unsafe.AddByteOffset(ref searchSpace, intPtr))
                    {
                        continue;
                    }
                    goto IL_0254;
                }
                if (Vector.IsHardwareAccelerated && (void*)intPtr != null)
                {
                    intPtr2 = (IntPtr)((int)(void*)intPtr & ~(Vector<byte>.Count - 1));
                    Vector<byte> vector = GetVector(value);
                    for (; (nuint)(void*)intPtr2 > (nuint)(Vector<byte>.Count - 1); intPtr2 -= Vector<byte>.Count)
                    {
                        Vector<byte> vector2 = Vector.Equals(vector, Unsafe.ReadUnaligned<Vector<byte>>(ref Unsafe.AddByteOffset(ref searchSpace, intPtr - Vector<byte>.Count)));
                        if (Vector<byte>.Zero.Equals(vector2))
                        {
                            intPtr -= Vector<byte>.Count;
                            continue;
                        }
                        return (int)intPtr - Vector<byte>.Count + LocateLastFoundByte(vector2);
                    }
                    if ((void*)intPtr != null)
                    {
                        intPtr2 = intPtr;
                        continue;
                    }
                }
                return -1;
            IL_0254:
                return (int)(void*)intPtr;
            IL_026a:
                return (int)(void*)(intPtr + 2);
            IL_0278:
                return (int)(void*)(intPtr + 3);
            IL_025c:
                return (int)(void*)(intPtr + 1);
            }
            return (int)(void*)(intPtr + 7);
        }

        public unsafe static int IndexOfAny(ref byte searchSpace, byte value0, byte value1, int length)
        {
            IntPtr intPtr = (IntPtr)0;
            IntPtr intPtr2 = (IntPtr)length;
            if (Vector.IsHardwareAccelerated && length >= Vector<byte>.Count * 2)
            {
                int num = (int)Unsafe.AsPointer(ref searchSpace) & (Vector<byte>.Count - 1);
                intPtr2 = (IntPtr)((Vector<byte>.Count - num) & (Vector<byte>.Count - 1));
            }
            while (true)
            {
                if ((nuint)(void*)intPtr2 >= (nuint)8u)
                {
                    intPtr2 -= 8;
                    uint num2 = Unsafe.AddByteOffset(ref searchSpace, intPtr);
                    if (value0 == num2 || value1 == num2)
                    {
                        goto IL_02ff;
                    }
                    num2 = Unsafe.AddByteOffset(ref searchSpace, intPtr + 1);
                    if (value0 == num2 || value1 == num2)
                    {
                        goto IL_0307;
                    }
                    num2 = Unsafe.AddByteOffset(ref searchSpace, intPtr + 2);
                    if (value0 == num2 || value1 == num2)
                    {
                        goto IL_0315;
                    }
                    num2 = Unsafe.AddByteOffset(ref searchSpace, intPtr + 3);
                    if (value0 != num2 && value1 != num2)
                    {
                        num2 = Unsafe.AddByteOffset(ref searchSpace, intPtr + 4);
                        if (value0 != num2 && value1 != num2)
                        {
                            num2 = Unsafe.AddByteOffset(ref searchSpace, intPtr + 5);
                            if (value0 != num2 && value1 != num2)
                            {
                                num2 = Unsafe.AddByteOffset(ref searchSpace, intPtr + 6);
                                if (value0 != num2 && value1 != num2)
                                {
                                    num2 = Unsafe.AddByteOffset(ref searchSpace, intPtr + 7);
                                    if (value0 == num2 || value1 == num2)
                                    {
                                        break;
                                    }
                                    intPtr += 8;
                                    continue;
                                }
                                return (int)(void*)(intPtr + 6);
                            }
                            return (int)(void*)(intPtr + 5);
                        }
                        return (int)(void*)(intPtr + 4);
                    }
                    goto IL_0323;
                }
                if ((nuint)(void*)intPtr2 >= (nuint)4u)
                {
                    intPtr2 -= 4;
                    uint num2 = Unsafe.AddByteOffset(ref searchSpace, intPtr);
                    if (value0 == num2 || value1 == num2)
                    {
                        goto IL_02ff;
                    }
                    num2 = Unsafe.AddByteOffset(ref searchSpace, intPtr + 1);
                    if (value0 == num2 || value1 == num2)
                    {
                        goto IL_0307;
                    }
                    num2 = Unsafe.AddByteOffset(ref searchSpace, intPtr + 2);
                    if (value0 == num2 || value1 == num2)
                    {
                        goto IL_0315;
                    }
                    num2 = Unsafe.AddByteOffset(ref searchSpace, intPtr + 3);
                    if (value0 == num2 || value1 == num2)
                    {
                        goto IL_0323;
                    }
                    intPtr += 4;
                }
                while ((void*)intPtr2 != null)
                {
                    intPtr2 -= 1;
                    uint num2 = Unsafe.AddByteOffset(ref searchSpace, intPtr);
                    if (value0 != num2 && value1 != num2)
                    {
                        intPtr += 1;
                        continue;
                    }
                    goto IL_02ff;
                }
                if (Vector.IsHardwareAccelerated && (int)(void*)intPtr < length)
                {
                    intPtr2 = (IntPtr)((length - (int)(void*)intPtr) & ~(Vector<byte>.Count - 1));
                    Vector<byte> vector = GetVector(value0);
                    Vector<byte> vector2 = GetVector(value1);
                    for (; (void*)intPtr2 > (void*)intPtr; intPtr += Vector<byte>.Count)
                    {
                        Vector<byte> left = Unsafe.ReadUnaligned<Vector<byte>>(ref Unsafe.AddByteOffset(ref searchSpace, intPtr));
                        Vector<byte> vector3 = Vector.BitwiseOr(Vector.Equals(left, vector), Vector.Equals(left, vector2));
                        if (!Vector<byte>.Zero.Equals(vector3))
                        {
                            return (int)(void*)intPtr + LocateFirstFoundByte(vector3);
                        }
                    }
                    if ((int)(void*)intPtr < length)
                    {
                        intPtr2 = (IntPtr)(length - (int)(void*)intPtr);
                        continue;
                    }
                }
                return -1;
            IL_02ff:
                return (int)(void*)intPtr;
            IL_0315:
                return (int)(void*)(intPtr + 2);
            IL_0307:
                return (int)(void*)(intPtr + 1);
            IL_0323:
                return (int)(void*)(intPtr + 3);
            }
            return (int)(void*)(intPtr + 7);
        }

        public unsafe static int IndexOfAny(ref byte searchSpace, byte value0, byte value1, byte value2, int length)
        {
            IntPtr intPtr = (IntPtr)0;
            IntPtr intPtr2 = (IntPtr)length;
            if (Vector.IsHardwareAccelerated && length >= Vector<byte>.Count * 2)
            {
                int num = (int)Unsafe.AsPointer(ref searchSpace) & (Vector<byte>.Count - 1);
                intPtr2 = (IntPtr)((Vector<byte>.Count - num) & (Vector<byte>.Count - 1));
            }
            while (true)
            {
                if ((nuint)(void*)intPtr2 >= (nuint)8u)
                {
                    intPtr2 -= 8;
                    uint num2 = Unsafe.AddByteOffset(ref searchSpace, intPtr);
                    if (value0 == num2 || value1 == num2 || value2 == num2)
                    {
                        goto IL_0393;
                    }
                    num2 = Unsafe.AddByteOffset(ref searchSpace, intPtr + 1);
                    if (value0 == num2 || value1 == num2 || value2 == num2)
                    {
                        goto IL_039b;
                    }
                    num2 = Unsafe.AddByteOffset(ref searchSpace, intPtr + 2);
                    if (value0 == num2 || value1 == num2 || value2 == num2)
                    {
                        goto IL_03a9;
                    }
                    num2 = Unsafe.AddByteOffset(ref searchSpace, intPtr + 3);
                    if (value0 != num2 && value1 != num2 && value2 != num2)
                    {
                        num2 = Unsafe.AddByteOffset(ref searchSpace, intPtr + 4);
                        if (value0 != num2 && value1 != num2 && value2 != num2)
                        {
                            num2 = Unsafe.AddByteOffset(ref searchSpace, intPtr + 5);
                            if (value0 != num2 && value1 != num2 && value2 != num2)
                            {
                                num2 = Unsafe.AddByteOffset(ref searchSpace, intPtr + 6);
                                if (value0 != num2 && value1 != num2 && value2 != num2)
                                {
                                    num2 = Unsafe.AddByteOffset(ref searchSpace, intPtr + 7);
                                    if (value0 == num2 || value1 == num2 || value2 == num2)
                                    {
                                        break;
                                    }
                                    intPtr += 8;
                                    continue;
                                }
                                return (int)(void*)(intPtr + 6);
                            }
                            return (int)(void*)(intPtr + 5);
                        }
                        return (int)(void*)(intPtr + 4);
                    }
                    goto IL_03b7;
                }
                if ((nuint)(void*)intPtr2 >= (nuint)4u)
                {
                    intPtr2 -= 4;
                    uint num2 = Unsafe.AddByteOffset(ref searchSpace, intPtr);
                    if (value0 == num2 || value1 == num2 || value2 == num2)
                    {
                        goto IL_0393;
                    }
                    num2 = Unsafe.AddByteOffset(ref searchSpace, intPtr + 1);
                    if (value0 == num2 || value1 == num2 || value2 == num2)
                    {
                        goto IL_039b;
                    }
                    num2 = Unsafe.AddByteOffset(ref searchSpace, intPtr + 2);
                    if (value0 == num2 || value1 == num2 || value2 == num2)
                    {
                        goto IL_03a9;
                    }
                    num2 = Unsafe.AddByteOffset(ref searchSpace, intPtr + 3);
                    if (value0 == num2 || value1 == num2 || value2 == num2)
                    {
                        goto IL_03b7;
                    }
                    intPtr += 4;
                }
                while ((void*)intPtr2 != null)
                {
                    intPtr2 -= 1;
                    uint num2 = Unsafe.AddByteOffset(ref searchSpace, intPtr);
                    if (value0 != num2 && value1 != num2 && value2 != num2)
                    {
                        intPtr += 1;
                        continue;
                    }
                    goto IL_0393;
                }
                if (Vector.IsHardwareAccelerated && (int)(void*)intPtr < length)
                {
                    intPtr2 = (IntPtr)((length - (int)(void*)intPtr) & ~(Vector<byte>.Count - 1));
                    Vector<byte> vector = GetVector(value0);
                    Vector<byte> vector2 = GetVector(value1);
                    Vector<byte> vector3 = GetVector(value2);
                    for (; (void*)intPtr2 > (void*)intPtr; intPtr += Vector<byte>.Count)
                    {
                        Vector<byte> left = Unsafe.ReadUnaligned<Vector<byte>>(ref Unsafe.AddByteOffset(ref searchSpace, intPtr));
                        Vector<byte> vector4 = Vector.BitwiseOr(Vector.BitwiseOr(Vector.Equals(left, vector), Vector.Equals(left, vector2)), Vector.Equals(left, vector3));
                        if (!Vector<byte>.Zero.Equals(vector4))
                        {
                            return (int)(void*)intPtr + LocateFirstFoundByte(vector4);
                        }
                    }
                    if ((int)(void*)intPtr < length)
                    {
                        intPtr2 = (IntPtr)(length - (int)(void*)intPtr);
                        continue;
                    }
                }
                return -1;
            IL_0393:
                return (int)(void*)intPtr;
            IL_039b:
                return (int)(void*)(intPtr + 1);
            IL_03b7:
                return (int)(void*)(intPtr + 3);
            IL_03a9:
                return (int)(void*)(intPtr + 2);
            }
            return (int)(void*)(intPtr + 7);
        }

        public unsafe static int LastIndexOfAny(ref byte searchSpace, byte value0, byte value1, int length)
        {
            IntPtr intPtr = (IntPtr)length;
            IntPtr intPtr2 = (IntPtr)length;
            if (Vector.IsHardwareAccelerated && length >= Vector<byte>.Count * 2)
            {
                int num = (int)Unsafe.AsPointer(ref searchSpace) & (Vector<byte>.Count - 1);
                intPtr2 = (IntPtr)(((length & (Vector<byte>.Count - 1)) + num) & (Vector<byte>.Count - 1));
            }
            while (true)
            {
                if ((nuint)(void*)intPtr2 >= (nuint)8u)
                {
                    intPtr2 -= 8;
                    intPtr -= 8;
                    uint num2 = Unsafe.AddByteOffset(ref searchSpace, intPtr + 7);
                    if (value0 == num2 || value1 == num2)
                    {
                        break;
                    }
                    num2 = Unsafe.AddByteOffset(ref searchSpace, intPtr + 6);
                    if (value0 != num2 && value1 != num2)
                    {
                        num2 = Unsafe.AddByteOffset(ref searchSpace, intPtr + 5);
                        if (value0 != num2 && value1 != num2)
                        {
                            num2 = Unsafe.AddByteOffset(ref searchSpace, intPtr + 4);
                            if (value0 != num2 && value1 != num2)
                            {
                                num2 = Unsafe.AddByteOffset(ref searchSpace, intPtr + 3);
                                if (value0 != num2 && value1 != num2)
                                {
                                    num2 = Unsafe.AddByteOffset(ref searchSpace, intPtr + 2);
                                    if (value0 != num2 && value1 != num2)
                                    {
                                        num2 = Unsafe.AddByteOffset(ref searchSpace, intPtr + 1);
                                        if (value0 != num2 && value1 != num2)
                                        {
                                            num2 = Unsafe.AddByteOffset(ref searchSpace, intPtr);
                                            if (value0 != num2 && value1 != num2)
                                            {
                                                continue;
                                            }
                                            goto IL_0314;
                                        }
                                        goto IL_031c;
                                    }
                                    goto IL_032a;
                                }
                                goto IL_0338;
                            }
                            return (int)(void*)(intPtr + 4);
                        }
                        return (int)(void*)(intPtr + 5);
                    }
                    return (int)(void*)(intPtr + 6);
                }
                if ((nuint)(void*)intPtr2 >= (nuint)4u)
                {
                    intPtr2 -= 4;
                    intPtr -= 4;
                    uint num2 = Unsafe.AddByteOffset(ref searchSpace, intPtr + 3);
                    if (value0 == num2 || value1 == num2)
                    {
                        goto IL_0338;
                    }
                    num2 = Unsafe.AddByteOffset(ref searchSpace, intPtr + 2);
                    if (value0 == num2 || value1 == num2)
                    {
                        goto IL_032a;
                    }
                    num2 = Unsafe.AddByteOffset(ref searchSpace, intPtr + 1);
                    if (value0 == num2 || value1 == num2)
                    {
                        goto IL_031c;
                    }
                    num2 = Unsafe.AddByteOffset(ref searchSpace, intPtr);
                    if (value0 == num2 || value1 == num2)
                    {
                        goto IL_0314;
                    }
                }
                while ((void*)intPtr2 != null)
                {
                    intPtr2 -= 1;
                    intPtr -= 1;
                    uint num2 = Unsafe.AddByteOffset(ref searchSpace, intPtr);
                    if (value0 != num2 && value1 != num2)
                    {
                        continue;
                    }
                    goto IL_0314;
                }
                if (Vector.IsHardwareAccelerated && (void*)intPtr != null)
                {
                    intPtr2 = (IntPtr)((int)(void*)intPtr & ~(Vector<byte>.Count - 1));
                    Vector<byte> vector = GetVector(value0);
                    Vector<byte> vector2 = GetVector(value1);
                    for (; (nuint)(void*)intPtr2 > (nuint)(Vector<byte>.Count - 1); intPtr2 -= Vector<byte>.Count)
                    {
                        Vector<byte> left = Unsafe.ReadUnaligned<Vector<byte>>(ref Unsafe.AddByteOffset(ref searchSpace, intPtr - Vector<byte>.Count));
                        Vector<byte> vector3 = Vector.BitwiseOr(Vector.Equals(left, vector), Vector.Equals(left, vector2));
                        if (Vector<byte>.Zero.Equals(vector3))
                        {
                            intPtr -= Vector<byte>.Count;
                            continue;
                        }
                        return (int)intPtr - Vector<byte>.Count + LocateLastFoundByte(vector3);
                    }
                    if ((void*)intPtr != null)
                    {
                        intPtr2 = intPtr;
                        continue;
                    }
                }
                return -1;
            IL_0314:
                return (int)(void*)intPtr;
            IL_0338:
                return (int)(void*)(intPtr + 3);
            IL_031c:
                return (int)(void*)(intPtr + 1);
            IL_032a:
                return (int)(void*)(intPtr + 2);
            }
            return (int)(void*)(intPtr + 7);
        }

        public unsafe static int LastIndexOfAny(ref byte searchSpace, byte value0, byte value1, byte value2, int length)
        {
            IntPtr intPtr = (IntPtr)length;
            IntPtr intPtr2 = (IntPtr)length;
            if (Vector.IsHardwareAccelerated && length >= Vector<byte>.Count * 2)
            {
                int num = (int)Unsafe.AsPointer(ref searchSpace) & (Vector<byte>.Count - 1);
                intPtr2 = (IntPtr)(((length & (Vector<byte>.Count - 1)) + num) & (Vector<byte>.Count - 1));
            }
            while (true)
            {
                if ((nuint)(void*)intPtr2 >= (nuint)8u)
                {
                    intPtr2 -= 8;
                    intPtr -= 8;
                    uint num2 = Unsafe.AddByteOffset(ref searchSpace, intPtr + 7);
                    if (value0 == num2 || value1 == num2 || value2 == num2)
                    {
                        break;
                    }
                    num2 = Unsafe.AddByteOffset(ref searchSpace, intPtr + 6);
                    if (value0 != num2 && value1 != num2 && value2 != num2)
                    {
                        num2 = Unsafe.AddByteOffset(ref searchSpace, intPtr + 5);
                        if (value0 != num2 && value1 != num2 && value2 != num2)
                        {
                            num2 = Unsafe.AddByteOffset(ref searchSpace, intPtr + 4);
                            if (value0 != num2 && value1 != num2 && value2 != num2)
                            {
                                num2 = Unsafe.AddByteOffset(ref searchSpace, intPtr + 3);
                                if (value0 != num2 && value1 != num2 && value2 != num2)
                                {
                                    num2 = Unsafe.AddByteOffset(ref searchSpace, intPtr + 2);
                                    if (value0 != num2 && value1 != num2 && value2 != num2)
                                    {
                                        num2 = Unsafe.AddByteOffset(ref searchSpace, intPtr + 1);
                                        if (value0 != num2 && value1 != num2 && value2 != num2)
                                        {
                                            num2 = Unsafe.AddByteOffset(ref searchSpace, intPtr);
                                            if (value0 != num2 && value1 != num2 && value2 != num2)
                                            {
                                                continue;
                                            }
                                            goto IL_03ab;
                                        }
                                        goto IL_03b3;
                                    }
                                    goto IL_03c1;
                                }
                                goto IL_03cf;
                            }
                            return (int)(void*)(intPtr + 4);
                        }
                        return (int)(void*)(intPtr + 5);
                    }
                    return (int)(void*)(intPtr + 6);
                }
                if ((nuint)(void*)intPtr2 >= (nuint)4u)
                {
                    intPtr2 -= 4;
                    intPtr -= 4;
                    uint num2 = Unsafe.AddByteOffset(ref searchSpace, intPtr + 3);
                    if (value0 == num2 || value1 == num2 || value2 == num2)
                    {
                        goto IL_03cf;
                    }
                    num2 = Unsafe.AddByteOffset(ref searchSpace, intPtr + 2);
                    if (value0 == num2 || value1 == num2 || value2 == num2)
                    {
                        goto IL_03c1;
                    }
                    num2 = Unsafe.AddByteOffset(ref searchSpace, intPtr + 1);
                    if (value0 == num2 || value1 == num2 || value2 == num2)
                    {
                        goto IL_03b3;
                    }
                    num2 = Unsafe.AddByteOffset(ref searchSpace, intPtr);
                    if (value0 == num2 || value1 == num2 || value2 == num2)
                    {
                        goto IL_03ab;
                    }
                }
                while ((void*)intPtr2 != null)
                {
                    intPtr2 -= 1;
                    intPtr -= 1;
                    uint num2 = Unsafe.AddByteOffset(ref searchSpace, intPtr);
                    if (value0 != num2 && value1 != num2 && value2 != num2)
                    {
                        continue;
                    }
                    goto IL_03ab;
                }
                if (Vector.IsHardwareAccelerated && (void*)intPtr != null)
                {
                    intPtr2 = (IntPtr)((int)(void*)intPtr & ~(Vector<byte>.Count - 1));
                    Vector<byte> vector = GetVector(value0);
                    Vector<byte> vector2 = GetVector(value1);
                    Vector<byte> vector3 = GetVector(value2);
                    for (; (nuint)(void*)intPtr2 > (nuint)(Vector<byte>.Count - 1); intPtr2 -= Vector<byte>.Count)
                    {
                        Vector<byte> left = Unsafe.ReadUnaligned<Vector<byte>>(ref Unsafe.AddByteOffset(ref searchSpace, intPtr - Vector<byte>.Count));
                        Vector<byte> vector4 = Vector.BitwiseOr(Vector.BitwiseOr(Vector.Equals(left, vector), Vector.Equals(left, vector2)), Vector.Equals(left, vector3));
                        if (Vector<byte>.Zero.Equals(vector4))
                        {
                            intPtr -= Vector<byte>.Count;
                            continue;
                        }
                        return (int)intPtr - Vector<byte>.Count + LocateLastFoundByte(vector4);
                    }
                    if ((void*)intPtr != null)
                    {
                        intPtr2 = intPtr;
                        continue;
                    }
                }
                return -1;
            IL_03ab:
                return (int)(void*)intPtr;
            IL_03cf:
                return (int)(void*)(intPtr + 3);
            IL_03c1:
                return (int)(void*)(intPtr + 2);
            IL_03b3:
                return (int)(void*)(intPtr + 1);
            }
            return (int)(void*)(intPtr + 7);
        }

        public unsafe static bool SequenceEqual(ref byte first, ref byte second, NUInt length)
        {
            if (Unsafe.AreSame(ref first, ref second))
            {
                goto IL_013d;
            }
            IntPtr intPtr = (IntPtr)0;
            IntPtr intPtr2 = (IntPtr)(void*)length;
            if (Vector.IsHardwareAccelerated && (nuint)(void*)intPtr2 >= (nuint)Vector<byte>.Count)
            {
                intPtr2 -= Vector<byte>.Count;
                while (true)
                {
                    if ((void*)intPtr2 > (void*)intPtr)
                    {
                        if (Unsafe.ReadUnaligned<Vector<byte>>(ref Unsafe.AddByteOffset(ref first, intPtr)) != Unsafe.ReadUnaligned<Vector<byte>>(ref Unsafe.AddByteOffset(ref second, intPtr)))
                        {
                            break;
                        }
                        intPtr += Vector<byte>.Count;
                        continue;
                    }
                    return Unsafe.ReadUnaligned<Vector<byte>>(ref Unsafe.AddByteOffset(ref first, intPtr2)) == Unsafe.ReadUnaligned<Vector<byte>>(ref Unsafe.AddByteOffset(ref second, intPtr2));
                }
            }
            else
            {
                if ((nuint)(void*)intPtr2 < (nuint)sizeof(UIntPtr))
                {
                    while ((void*)intPtr2 > (void*)intPtr)
                    {
                        if (Unsafe.AddByteOffset(ref first, intPtr) == Unsafe.AddByteOffset(ref second, intPtr))
                        {
                            intPtr += 1;
                            continue;
                        }
                        goto IL_013f;
                    }
                    goto IL_013d;
                }
                intPtr2 -= sizeof(UIntPtr);
                while (true)
                {
                    if ((void*)intPtr2 > (void*)intPtr)
                    {
                        if (Unsafe.ReadUnaligned<UIntPtr>(ref Unsafe.AddByteOffset(ref first, intPtr)) != Unsafe.ReadUnaligned<UIntPtr>(ref Unsafe.AddByteOffset(ref second, intPtr)))
                        {
                            break;
                        }
                        intPtr += sizeof(UIntPtr);
                        continue;
                    }
                    return Unsafe.ReadUnaligned<UIntPtr>(ref Unsafe.AddByteOffset(ref first, intPtr2)) == Unsafe.ReadUnaligned<UIntPtr>(ref Unsafe.AddByteOffset(ref second, intPtr2));
                }
            }
            goto IL_013f;
        IL_013d:
            return true;
        IL_013f:
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int LocateFirstFoundByte(Vector<byte> match)
        {
            Vector<ulong> vector = Vector.AsVectorUInt64(match);
            ulong num = 0uL;
            int i;
            for (i = 0; i < Vector<ulong>.Count; i++)
            {
                num = vector[i];
                if (num != 0L)
                {
                    break;
                }
            }
            return i * 8 + LocateFirstFoundByte(num);
        }

        public unsafe static int SequenceCompareTo(ref byte first, int firstLength, ref byte second, int secondLength)
        {
            if (!Unsafe.AreSame(ref first, ref second))
            {
                IntPtr intPtr = (IntPtr)((firstLength < secondLength) ? firstLength : secondLength);
                IntPtr intPtr2 = (IntPtr)0;
                IntPtr intPtr3 = (IntPtr)(void*)intPtr;
                if (Vector.IsHardwareAccelerated && (nuint)(void*)intPtr3 > (nuint)Vector<byte>.Count)
                {
                    intPtr3 -= Vector<byte>.Count;
                    for (; (void*)intPtr3 > (void*)intPtr2 && !(Unsafe.ReadUnaligned<Vector<byte>>(ref Unsafe.AddByteOffset(ref first, intPtr2)) != Unsafe.ReadUnaligned<Vector<byte>>(ref Unsafe.AddByteOffset(ref second, intPtr2))); intPtr2 += Vector<byte>.Count)
                    {
                    }
                }
                else if ((nuint)(void*)intPtr3 > (nuint)sizeof(UIntPtr))
                {
                    intPtr3 -= sizeof(UIntPtr);
                    for (; (void*)intPtr3 > (void*)intPtr2 && !(Unsafe.ReadUnaligned<UIntPtr>(ref Unsafe.AddByteOffset(ref first, intPtr2)) != Unsafe.ReadUnaligned<UIntPtr>(ref Unsafe.AddByteOffset(ref second, intPtr2))); intPtr2 += sizeof(UIntPtr))
                    {
                    }
                }
                for (; (void*)intPtr > (void*)intPtr2; intPtr2 += 1)
                {
                    int num = Unsafe.AddByteOffset(ref first, intPtr2).CompareTo(Unsafe.AddByteOffset(ref second, intPtr2));
                    if (num != 0)
                    {
                        return num;
                    }
                }
            }
            return firstLength - secondLength;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int LocateLastFoundByte(Vector<byte> match)
        {
            Vector<ulong> vector = Vector.AsVectorUInt64(match);
            ulong num = 0uL;
            int num2;
            for (num2 = Vector<ulong>.Count - 1; num2 >= 0; num2--)
            {
                num = vector[num2];
                if (num != 0L)
                {
                    break;
                }
            }
            return num2 * 8 + LocateLastFoundByte(num);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int LocateFirstFoundByte(ulong match)
        {
            ulong num = match ^ (match - 1);
            return (int)(num * 283686952306184L >> 57);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int LocateLastFoundByte(ulong match)
        {
            int num = 7;
            while ((long)match > 0L)
            {
                match <<= 8;
                num--;
            }
            return num;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector<byte> GetVector(byte vectorByte)
        {
            return Vector.AsVectorByte(new Vector<uint>((uint)(vectorByte * 16843009)));
        }

        public unsafe static int SequenceCompareTo(ref char first, int firstLength, ref char second, int secondLength)
        {
            int result = firstLength - secondLength;
            if (!Unsafe.AreSame(ref first, ref second))
            {
                IntPtr intPtr = (IntPtr)((firstLength < secondLength) ? firstLength : secondLength);
                IntPtr intPtr2 = (IntPtr)0;
                if ((nuint)(void*)intPtr >= (nuint)(sizeof(UIntPtr) / 2))
                {
                    if (Vector.IsHardwareAccelerated && (nuint)(void*)intPtr >= (nuint)Vector<ushort>.Count)
                    {
                        IntPtr intPtr3 = intPtr - Vector<ushort>.Count;
                        while (!(Unsafe.ReadUnaligned<Vector<ushort>>(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref first, intPtr2))) != Unsafe.ReadUnaligned<Vector<ushort>>(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref second, intPtr2)))))
                        {
                            intPtr2 += Vector<ushort>.Count;
                            if ((void*)intPtr3 < (void*)intPtr2)
                            {
                                break;
                            }
                        }
                    }
                    for (; (void*)intPtr >= (void*)(intPtr2 + sizeof(UIntPtr) / 2) && !(Unsafe.ReadUnaligned<UIntPtr>(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref first, intPtr2))) != Unsafe.ReadUnaligned<UIntPtr>(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref second, intPtr2)))); intPtr2 += sizeof(UIntPtr) / 2)
                    {
                    }
                }
                if (sizeof(UIntPtr) > 4 && (void*)intPtr >= (void*)(intPtr2 + 2) && Unsafe.ReadUnaligned<int>(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref first, intPtr2))) == Unsafe.ReadUnaligned<int>(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref second, intPtr2))))
                {
                    intPtr2 += 2;
                }
                for (; (void*)intPtr2 < (void*)intPtr; intPtr2 += 1)
                {
                    int num = Unsafe.Add(ref first, intPtr2).CompareTo(Unsafe.Add(ref second, intPtr2));
                    if (num != 0)
                    {
                        return num;
                    }
                }
            }
            return result;
        }

        public unsafe static int IndexOf(ref char searchSpace, char value, int length)
        {
            fixed (char* ptr = &searchSpace)
            {
                char* ptr2 = ptr;
                char* ptr3 = ptr2 + length;
                if (Vector.IsHardwareAccelerated && length >= Vector<ushort>.Count * 2)
                {
                    int num = ((int)ptr2 & (Unsafe.SizeOf<Vector<ushort>>() - 1)) / 2;
                    length = (Vector<ushort>.Count - num) & (Vector<ushort>.Count - 1);
                }
                while (true)
                {
                    if (length >= 4)
                    {
                        length -= 4;
                        if (*ptr2 == value)
                        {
                            break;
                        }
                        if (ptr2[1] != value)
                        {
                            if (ptr2[2] != value)
                            {
                                if (ptr2[3] != value)
                                {
                                    ptr2 += 4;
                                    continue;
                                }
                                ptr2++;
                            }
                            ptr2++;
                        }
                        ptr2++;
                        break;
                    }
                    while (length > 0)
                    {
                        length--;
                        if (*ptr2 == value)
                        {
                            goto end_IL_0079;
                        }
                        ptr2++;
                    }
                    if (Vector.IsHardwareAccelerated && ptr2 < ptr3)
                    {
                        length = (int)((ptr3 - ptr2) & ~(Vector<ushort>.Count - 1));
                        Vector<ushort> left = new Vector<ushort>(value);
                        while (length > 0)
                        {
                            Vector<ushort> vector = Vector.Equals(left, Unsafe.Read<Vector<ushort>>(ptr2));
                            if (Vector<ushort>.Zero.Equals(vector))
                            {
                                ptr2 += Vector<ushort>.Count;
                                length -= Vector<ushort>.Count;
                                continue;
                            }
                            return (int)(ptr2 - ptr) + LocateFirstFoundChar(vector);
                        }
                        if (ptr2 < ptr3)
                        {
                            length = (int)(ptr3 - ptr2);
                            continue;
                        }
                    }
                    return -1;
                end_IL_0079:
                    break;
                }
                return (int)(ptr2 - ptr);
            }
        }

        public unsafe static int LastIndexOf(ref char searchSpace, char value, int length)
        {
            fixed (char* ptr = &searchSpace)
            {
                char* ptr2 = ptr + length;
                char* ptr3 = ptr;
                if (Vector.IsHardwareAccelerated && length >= Vector<ushort>.Count * 2)
                {
                    length = ((int)ptr2 & (Unsafe.SizeOf<Vector<ushort>>() - 1)) / 2;
                }
                while (true)
                {
                    if (length >= 4)
                    {
                        length -= 4;
                        ptr2 -= 4;
                        if (ptr2[3] == value)
                        {
                            break;
                        }
                        if (ptr2[2] != value)
                        {
                            if (ptr2[1] != value)
                            {
                                if (*ptr2 != value)
                                {
                                    continue;
                                }
                                goto IL_011d;
                            }
                            return (int)(ptr2 - ptr3) + 1;
                        }
                        return (int)(ptr2 - ptr3) + 2;
                    }
                    while (length > 0)
                    {
                        length--;
                        ptr2--;
                        if (*ptr2 != value)
                        {
                            continue;
                        }
                        goto IL_011d;
                    }
                    if (Vector.IsHardwareAccelerated && ptr2 > ptr3)
                    {
                        length = (int)((ptr2 - ptr3) & ~(Vector<ushort>.Count - 1));
                        Vector<ushort> left = new Vector<ushort>(value);
                        while (length > 0)
                        {
                            char* ptr4 = ptr2 - Vector<ushort>.Count;
                            Vector<ushort> vector = Vector.Equals(left, Unsafe.Read<Vector<ushort>>(ptr4));
                            if (Vector<ushort>.Zero.Equals(vector))
                            {
                                ptr2 -= Vector<ushort>.Count;
                                length -= Vector<ushort>.Count;
                                continue;
                            }
                            return (int)(ptr4 - ptr3) + LocateLastFoundChar(vector);
                        }
                        if (ptr2 > ptr3)
                        {
                            length = (int)(ptr2 - ptr3);
                            continue;
                        }
                    }
                    return -1;
                IL_011d:
                    return (int)(ptr2 - ptr3);
                }
                return (int)(ptr2 - ptr3) + 3;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int LocateFirstFoundChar(Vector<ushort> match)
        {
            Vector<ulong> vector = Vector.AsVectorUInt64(match);
            ulong num = 0uL;
            int i;
            for (i = 0; i < Vector<ulong>.Count; i++)
            {
                num = vector[i];
                if (num != 0L)
                {
                    break;
                }
            }
            return i * 4 + LocateFirstFoundChar(num);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int LocateFirstFoundChar(ulong match)
        {
            ulong num = match ^ (match - 1);
            return (int)(num * 4295098372L >> 49);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int LocateLastFoundChar(Vector<ushort> match)
        {
            Vector<ulong> vector = Vector.AsVectorUInt64(match);
            ulong num = 0uL;
            int num2;
            for (num2 = Vector<ulong>.Count - 1; num2 >= 0; num2--)
            {
                num = vector[num2];
                if (num != 0L)
                {
                    break;
                }
            }
            return num2 * 4 + LocateLastFoundChar(num);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int LocateLastFoundChar(ulong match)
        {
            int num = 3;
            while ((long)match > 0L)
            {
                match <<= 16;
                num--;
            }
            return num;
        }

        public static int IndexOf<T>(ref T searchSpace, int searchSpaceLength, ref T value, int valueLength) where T : IEquatable<T>
        {
            if (valueLength == 0)
            {
                return 0;
            }
            T value2 = value;
            ref T second = ref Unsafe.Add(ref value, 1);
            int num = valueLength - 1;
            int num2 = 0;
            while (true)
            {
                int num3 = searchSpaceLength - num2 - num;
                if (num3 <= 0)
                {
                    break;
                }
                int num4 = IndexOf(ref Unsafe.Add(ref searchSpace, num2), value2, num3);
                if (num4 == -1)
                {
                    break;
                }
                num2 += num4;
                if (SequenceEqual(ref Unsafe.Add(ref searchSpace, num2 + 1), ref second, num))
                {
                    return num2;
                }
                num2++;
            }
            return -1;
        }

        public unsafe static int IndexOf<T>(ref T searchSpace, T value, int length) where T : IEquatable<T>
        {
            IntPtr intPtr = (IntPtr)0;
            while (true)
            {
                if (length >= 8)
                {
                    length -= 8;
                    if (!value.Equals(Unsafe.Add(ref searchSpace, intPtr)))
                    {
                        if (value.Equals(Unsafe.Add(ref searchSpace, intPtr + 1)))
                        {
                            goto IL_020a;
                        }
                        if (value.Equals(Unsafe.Add(ref searchSpace, intPtr + 2)))
                        {
                            goto IL_0218;
                        }
                        if (!value.Equals(Unsafe.Add(ref searchSpace, intPtr + 3)))
                        {
                            if (!value.Equals(Unsafe.Add(ref searchSpace, intPtr + 4)))
                            {
                                if (!value.Equals(Unsafe.Add(ref searchSpace, intPtr + 5)))
                                {
                                    if (!value.Equals(Unsafe.Add(ref searchSpace, intPtr + 6)))
                                    {
                                        if (value.Equals(Unsafe.Add(ref searchSpace, intPtr + 7)))
                                        {
                                            break;
                                        }
                                        intPtr += 8;
                                        continue;
                                    }
                                    return (int)(void*)(intPtr + 6);
                                }
                                return (int)(void*)(intPtr + 5);
                            }
                            return (int)(void*)(intPtr + 4);
                        }
                        goto IL_0226;
                    }
                }
                else
                {
                    if (length >= 4)
                    {
                        length -= 4;
                        if (value.Equals(Unsafe.Add(ref searchSpace, intPtr)))
                        {
                            goto IL_0202;
                        }
                        if (value.Equals(Unsafe.Add(ref searchSpace, intPtr + 1)))
                        {
                            goto IL_020a;
                        }
                        if (value.Equals(Unsafe.Add(ref searchSpace, intPtr + 2)))
                        {
                            goto IL_0218;
                        }
                        if (value.Equals(Unsafe.Add(ref searchSpace, intPtr + 3)))
                        {
                            goto IL_0226;
                        }
                        intPtr += 4;
                    }
                    while (true)
                    {
                        if (length > 0)
                        {
                            if (value.Equals(Unsafe.Add(ref searchSpace, intPtr)))
                            {
                                break;
                            }
                            intPtr += 1;
                            length--;
                            continue;
                        }
                        return -1;
                    }
                }
                goto IL_0202;
            IL_0218:
                return (int)(void*)(intPtr + 2);
            IL_0202:
                return (int)(void*)intPtr;
            IL_020a:
                return (int)(void*)(intPtr + 1);
            IL_0226:
                return (int)(void*)(intPtr + 3);
            }
            return (int)(void*)(intPtr + 7);
        }

        public static int IndexOfAny<T>(ref T searchSpace, T value0, T value1, int length) where T : IEquatable<T>
        {
            int num = 0;
            while (true)
            {
                if (length - num >= 8)
                {
                    T other = Unsafe.Add(ref searchSpace, num);
                    if (!value0.Equals(other) && !value1.Equals(other))
                    {
                        other = Unsafe.Add(ref searchSpace, num + 1);
                        if (value0.Equals(other) || value1.Equals(other))
                        {
                            goto IL_02cb;
                        }
                        other = Unsafe.Add(ref searchSpace, num + 2);
                        if (value0.Equals(other) || value1.Equals(other))
                        {
                            goto IL_02cf;
                        }
                        other = Unsafe.Add(ref searchSpace, num + 3);
                        if (!value0.Equals(other) && !value1.Equals(other))
                        {
                            other = Unsafe.Add(ref searchSpace, num + 4);
                            if (!value0.Equals(other) && !value1.Equals(other))
                            {
                                other = Unsafe.Add(ref searchSpace, num + 5);
                                if (!value0.Equals(other) && !value1.Equals(other))
                                {
                                    other = Unsafe.Add(ref searchSpace, num + 6);
                                    if (!value0.Equals(other) && !value1.Equals(other))
                                    {
                                        other = Unsafe.Add(ref searchSpace, num + 7);
                                        if (value0.Equals(other) || value1.Equals(other))
                                        {
                                            break;
                                        }
                                        num += 8;
                                        continue;
                                    }
                                    return num + 6;
                                }
                                return num + 5;
                            }
                            return num + 4;
                        }
                        goto IL_02d3;
                    }
                }
                else
                {
                    if (length - num >= 4)
                    {
                        T other = Unsafe.Add(ref searchSpace, num);
                        if (value0.Equals(other) || value1.Equals(other))
                        {
                            goto IL_02c9;
                        }
                        other = Unsafe.Add(ref searchSpace, num + 1);
                        if (value0.Equals(other) || value1.Equals(other))
                        {
                            goto IL_02cb;
                        }
                        other = Unsafe.Add(ref searchSpace, num + 2);
                        if (value0.Equals(other) || value1.Equals(other))
                        {
                            goto IL_02cf;
                        }
                        other = Unsafe.Add(ref searchSpace, num + 3);
                        if (value0.Equals(other) || value1.Equals(other))
                        {
                            goto IL_02d3;
                        }
                        num += 4;
                    }
                    while (true)
                    {
                        if (num < length)
                        {
                            T other = Unsafe.Add(ref searchSpace, num);
                            if (value0.Equals(other) || value1.Equals(other))
                            {
                                break;
                            }
                            num++;
                            continue;
                        }
                        return -1;
                    }
                }
                goto IL_02c9;
            IL_02cf:
                return num + 2;
            IL_02cb:
                return num + 1;
            IL_02d3:
                return num + 3;
            IL_02c9:
                return num;
            }
            return num + 7;
        }

        public static int IndexOfAny<T>(ref T searchSpace, T value0, T value1, T value2, int length) where T : IEquatable<T>
        {
            int num = 0;
            while (true)
            {
                if (length - num >= 8)
                {
                    T other = Unsafe.Add(ref searchSpace, num);
                    if (!value0.Equals(other) && !value1.Equals(other) && !value2.Equals(other))
                    {
                        other = Unsafe.Add(ref searchSpace, num + 1);
                        if (value0.Equals(other) || value1.Equals(other) || value2.Equals(other))
                        {
                            goto IL_03c2;
                        }
                        other = Unsafe.Add(ref searchSpace, num + 2);
                        if (value0.Equals(other) || value1.Equals(other) || value2.Equals(other))
                        {
                            goto IL_03c6;
                        }
                        other = Unsafe.Add(ref searchSpace, num + 3);
                        if (!value0.Equals(other) && !value1.Equals(other) && !value2.Equals(other))
                        {
                            other = Unsafe.Add(ref searchSpace, num + 4);
                            if (!value0.Equals(other) && !value1.Equals(other) && !value2.Equals(other))
                            {
                                other = Unsafe.Add(ref searchSpace, num + 5);
                                if (!value0.Equals(other) && !value1.Equals(other) && !value2.Equals(other))
                                {
                                    other = Unsafe.Add(ref searchSpace, num + 6);
                                    if (!value0.Equals(other) && !value1.Equals(other) && !value2.Equals(other))
                                    {
                                        other = Unsafe.Add(ref searchSpace, num + 7);
                                        if (value0.Equals(other) || value1.Equals(other) || value2.Equals(other))
                                        {
                                            break;
                                        }
                                        num += 8;
                                        continue;
                                    }
                                    return num + 6;
                                }
                                return num + 5;
                            }
                            return num + 4;
                        }
                        goto IL_03ca;
                    }
                }
                else
                {
                    if (length - num >= 4)
                    {
                        T other = Unsafe.Add(ref searchSpace, num);
                        if (value0.Equals(other) || value1.Equals(other) || value2.Equals(other))
                        {
                            goto IL_03c0;
                        }
                        other = Unsafe.Add(ref searchSpace, num + 1);
                        if (value0.Equals(other) || value1.Equals(other) || value2.Equals(other))
                        {
                            goto IL_03c2;
                        }
                        other = Unsafe.Add(ref searchSpace, num + 2);
                        if (value0.Equals(other) || value1.Equals(other) || value2.Equals(other))
                        {
                            goto IL_03c6;
                        }
                        other = Unsafe.Add(ref searchSpace, num + 3);
                        if (value0.Equals(other) || value1.Equals(other) || value2.Equals(other))
                        {
                            goto IL_03ca;
                        }
                        num += 4;
                    }
                    while (true)
                    {
                        if (num < length)
                        {
                            T other = Unsafe.Add(ref searchSpace, num);
                            if (value0.Equals(other) || value1.Equals(other) || value2.Equals(other))
                            {
                                break;
                            }
                            num++;
                            continue;
                        }
                        return -1;
                    }
                }
                goto IL_03c0;
            IL_03c0:
                return num;
            IL_03c6:
                return num + 2;
            IL_03c2:
                return num + 1;
            IL_03ca:
                return num + 3;
            }
            return num + 7;
        }

        public static int IndexOfAny<T>(ref T searchSpace, int searchSpaceLength, ref T value, int valueLength) where T : IEquatable<T>
        {
            if (valueLength == 0)
            {
                return 0;
            }
            int num = -1;
            for (int i = 0; i < valueLength; i++)
            {
                int num2 = IndexOf(ref searchSpace, Unsafe.Add(ref value, i), searchSpaceLength);
                if ((uint)num2 < (uint)num)
                {
                    num = num2;
                    searchSpaceLength = num2;
                    if (num == 0)
                    {
                        break;
                    }
                }
            }
            return num;
        }

        public static int LastIndexOf<T>(ref T searchSpace, int searchSpaceLength, ref T value, int valueLength) where T : IEquatable<T>
        {
            if (valueLength == 0)
            {
                return 0;
            }
            T value2 = value;
            ref T second = ref Unsafe.Add(ref value, 1);
            int num = valueLength - 1;
            int num2 = 0;
            while (true)
            {
                int num3 = searchSpaceLength - num2 - num;
                if (num3 <= 0)
                {
                    break;
                }
                int num4 = LastIndexOf(ref searchSpace, value2, num3);
                if (num4 == -1)
                {
                    break;
                }
                if (SequenceEqual(ref Unsafe.Add(ref searchSpace, num4 + 1), ref second, num))
                {
                    return num4;
                }
                num2 += num3 - num4;
            }
            return -1;
        }

        public static int LastIndexOf<T>(ref T searchSpace, T value, int length) where T : IEquatable<T>
        {
            while (true)
            {
                if (length >= 8)
                {
                    length -= 8;
                    if (value.Equals(Unsafe.Add(ref searchSpace, length + 7)))
                    {
                        break;
                    }
                    if (value.Equals(Unsafe.Add(ref searchSpace, length + 6)))
                    {
                        return length + 6;
                    }
                    if (value.Equals(Unsafe.Add(ref searchSpace, length + 5)))
                    {
                        return length + 5;
                    }
                    if (value.Equals(Unsafe.Add(ref searchSpace, length + 4)))
                    {
                        return length + 4;
                    }
                    if (value.Equals(Unsafe.Add(ref searchSpace, length + 3)))
                    {
                        goto IL_01c2;
                    }
                    if (value.Equals(Unsafe.Add(ref searchSpace, length + 2)))
                    {
                        goto IL_01be;
                    }
                    if (value.Equals(Unsafe.Add(ref searchSpace, length + 1)))
                    {
                        goto IL_01ba;
                    }
                    if (!value.Equals(Unsafe.Add(ref searchSpace, length)))
                    {
                        continue;
                    }
                }
                else
                {
                    if (length >= 4)
                    {
                        length -= 4;
                        if (value.Equals(Unsafe.Add(ref searchSpace, length + 3)))
                        {
                            goto IL_01c2;
                        }
                        if (value.Equals(Unsafe.Add(ref searchSpace, length + 2)))
                        {
                            goto IL_01be;
                        }
                        if (value.Equals(Unsafe.Add(ref searchSpace, length + 1)))
                        {
                            goto IL_01ba;
                        }
                        if (value.Equals(Unsafe.Add(ref searchSpace, length)))
                        {
                            goto IL_01b8;
                        }
                    }
                    do
                    {
                        if (length > 0)
                        {
                            length--;
                            continue;
                        }
                        return -1;
                    }
                    while (!value.Equals(Unsafe.Add(ref searchSpace, length)));
                }
                goto IL_01b8;
            IL_01be:
                return length + 2;
            IL_01c2:
                return length + 3;
            IL_01ba:
                return length + 1;
            IL_01b8:
                return length;
            }
            return length + 7;
        }

        public static int LastIndexOfAny<T>(ref T searchSpace, T value0, T value1, int length) where T : IEquatable<T>
        {
            while (true)
            {
                if (length >= 8)
                {
                    length -= 8;
                    T other = Unsafe.Add(ref searchSpace, length + 7);
                    if (value0.Equals(other) || value1.Equals(other))
                    {
                        break;
                    }
                    other = Unsafe.Add(ref searchSpace, length + 6);
                    if (value0.Equals(other) || value1.Equals(other))
                    {
                        return length + 6;
                    }
                    other = Unsafe.Add(ref searchSpace, length + 5);
                    if (value0.Equals(other) || value1.Equals(other))
                    {
                        return length + 5;
                    }
                    other = Unsafe.Add(ref searchSpace, length + 4);
                    if (value0.Equals(other) || value1.Equals(other))
                    {
                        return length + 4;
                    }
                    other = Unsafe.Add(ref searchSpace, length + 3);
                    if (value0.Equals(other) || value1.Equals(other))
                    {
                        goto IL_02cd;
                    }
                    other = Unsafe.Add(ref searchSpace, length + 2);
                    if (value0.Equals(other) || value1.Equals(other))
                    {
                        goto IL_02c9;
                    }
                    other = Unsafe.Add(ref searchSpace, length + 1);
                    if (value0.Equals(other) || value1.Equals(other))
                    {
                        goto IL_02c5;
                    }
                    other = Unsafe.Add(ref searchSpace, length);
                    if (!value0.Equals(other) && !value1.Equals(other))
                    {
                        continue;
                    }
                }
                else
                {
                    T other;
                    if (length >= 4)
                    {
                        length -= 4;
                        other = Unsafe.Add(ref searchSpace, length + 3);
                        if (value0.Equals(other) || value1.Equals(other))
                        {
                            goto IL_02cd;
                        }
                        other = Unsafe.Add(ref searchSpace, length + 2);
                        if (value0.Equals(other) || value1.Equals(other))
                        {
                            goto IL_02c9;
                        }
                        other = Unsafe.Add(ref searchSpace, length + 1);
                        if (value0.Equals(other) || value1.Equals(other))
                        {
                            goto IL_02c5;
                        }
                        other = Unsafe.Add(ref searchSpace, length);
                        if (value0.Equals(other) || value1.Equals(other))
                        {
                            goto IL_02c3;
                        }
                    }
                    do
                    {
                        if (length > 0)
                        {
                            length--;
                            other = Unsafe.Add(ref searchSpace, length);
                            continue;
                        }
                        return -1;
                    }
                    while (!value0.Equals(other) && !value1.Equals(other));
                }
                goto IL_02c3;
            IL_02c9:
                return length + 2;
            IL_02c5:
                return length + 1;
            IL_02c3:
                return length;
            IL_02cd:
                return length + 3;
            }
            return length + 7;
        }

        public static int LastIndexOfAny<T>(ref T searchSpace, T value0, T value1, T value2, int length) where T : IEquatable<T>
        {
            while (true)
            {
                if (length >= 8)
                {
                    length -= 8;
                    T other = Unsafe.Add(ref searchSpace, length + 7);
                    if (value0.Equals(other) || value1.Equals(other) || value2.Equals(other))
                    {
                        break;
                    }
                    other = Unsafe.Add(ref searchSpace, length + 6);
                    if (value0.Equals(other) || value1.Equals(other) || value2.Equals(other))
                    {
                        return length + 6;
                    }
                    other = Unsafe.Add(ref searchSpace, length + 5);
                    if (value0.Equals(other) || value1.Equals(other) || value2.Equals(other))
                    {
                        return length + 5;
                    }
                    other = Unsafe.Add(ref searchSpace, length + 4);
                    if (value0.Equals(other) || value1.Equals(other) || value2.Equals(other))
                    {
                        return length + 4;
                    }
                    other = Unsafe.Add(ref searchSpace, length + 3);
                    if (value0.Equals(other) || value1.Equals(other) || value2.Equals(other))
                    {
                        goto IL_03da;
                    }
                    other = Unsafe.Add(ref searchSpace, length + 2);
                    if (value0.Equals(other) || value1.Equals(other) || value2.Equals(other))
                    {
                        goto IL_03d5;
                    }
                    other = Unsafe.Add(ref searchSpace, length + 1);
                    if (value0.Equals(other) || value1.Equals(other) || value2.Equals(other))
                    {
                        goto IL_03d0;
                    }
                    other = Unsafe.Add(ref searchSpace, length);
                    if (!value0.Equals(other) && !value1.Equals(other) && !value2.Equals(other))
                    {
                        continue;
                    }
                }
                else
                {
                    T other;
                    if (length >= 4)
                    {
                        length -= 4;
                        other = Unsafe.Add(ref searchSpace, length + 3);
                        if (value0.Equals(other) || value1.Equals(other) || value2.Equals(other))
                        {
                            goto IL_03da;
                        }
                        other = Unsafe.Add(ref searchSpace, length + 2);
                        if (value0.Equals(other) || value1.Equals(other) || value2.Equals(other))
                        {
                            goto IL_03d5;
                        }
                        other = Unsafe.Add(ref searchSpace, length + 1);
                        if (value0.Equals(other) || value1.Equals(other) || value2.Equals(other))
                        {
                            goto IL_03d0;
                        }
                        other = Unsafe.Add(ref searchSpace, length);
                        if (value0.Equals(other) || value1.Equals(other) || value2.Equals(other))
                        {
                            goto IL_03cd;
                        }
                    }
                    do
                    {
                        if (length > 0)
                        {
                            length--;
                            other = Unsafe.Add(ref searchSpace, length);
                            continue;
                        }
                        return -1;
                    }
                    while (!value0.Equals(other) && !value1.Equals(other) && !value2.Equals(other));
                }
                goto IL_03cd;
            IL_03d0:
                return length + 1;
            IL_03d5:
                return length + 2;
            IL_03da:
                return length + 3;
            IL_03cd:
                return length;
            }
            return length + 7;
        }

        public static int LastIndexOfAny<T>(ref T searchSpace, int searchSpaceLength, ref T value, int valueLength) where T : IEquatable<T>
        {
            if (valueLength == 0)
            {
                return 0;
            }
            int num = -1;
            for (int i = 0; i < valueLength; i++)
            {
                int num2 = LastIndexOf(ref searchSpace, Unsafe.Add(ref value, i), searchSpaceLength);
                if (num2 > num)
                {
                    num = num2;
                }
            }
            return num;
        }

        public static bool SequenceEqual<T>(ref T first, ref T second, int length) where T : IEquatable<T>
        {
            if (!Unsafe.AreSame(ref first, ref second))
            {
                IntPtr intPtr = (IntPtr)0;
                while (true)
                {
                    if (length >= 8)
                    {
                        length -= 8;
                        if (Unsafe.Add(ref first, intPtr).Equals(Unsafe.Add(ref second, intPtr)) && Unsafe.Add(ref first, intPtr + 1).Equals(Unsafe.Add(ref second, intPtr + 1)) && Unsafe.Add(ref first, intPtr + 2).Equals(Unsafe.Add(ref second, intPtr + 2)) && Unsafe.Add(ref first, intPtr + 3).Equals(Unsafe.Add(ref second, intPtr + 3)) && Unsafe.Add(ref first, intPtr + 4).Equals(Unsafe.Add(ref second, intPtr + 4)) && Unsafe.Add(ref first, intPtr + 5).Equals(Unsafe.Add(ref second, intPtr + 5)) && Unsafe.Add(ref first, intPtr + 6).Equals(Unsafe.Add(ref second, intPtr + 6)) && Unsafe.Add(ref first, intPtr + 7).Equals(Unsafe.Add(ref second, intPtr + 7)))
                        {
                            intPtr += 8;
                            continue;
                        }
                        goto IL_028b;
                    }
                    if (length >= 4)
                    {
                        length -= 4;
                        if (!Unsafe.Add(ref first, intPtr).Equals(Unsafe.Add(ref second, intPtr)) || !Unsafe.Add(ref first, intPtr + 1).Equals(Unsafe.Add(ref second, intPtr + 1)) || !Unsafe.Add(ref first, intPtr + 2).Equals(Unsafe.Add(ref second, intPtr + 2)) || !Unsafe.Add(ref first, intPtr + 3).Equals(Unsafe.Add(ref second, intPtr + 3)))
                        {
                            goto IL_028b;
                        }
                        intPtr += 4;
                    }
                    while (length > 0)
                    {
                        if (Unsafe.Add(ref first, intPtr).Equals(Unsafe.Add(ref second, intPtr)))
                        {
                            intPtr += 1;
                            length--;
                            continue;
                        }
                        goto IL_028b;
                    }
                    break;
                IL_028b:
                    return false;
                }
            }
            return true;
        }

        public static int SequenceCompareTo<T>(ref T first, int firstLength, ref T second, int secondLength) where T : IComparable<T>
        {
            int num = firstLength;
            if (num > secondLength)
            {
                num = secondLength;
            }
            for (int i = 0; i < num; i++)
            {
                int num2 = Unsafe.Add(ref first, i).CompareTo(Unsafe.Add(ref second, i));
                if (num2 != 0)
                {
                    return num2;
                }
            }
            return firstLength.CompareTo(secondLength);
        }

        public unsafe static void CopyTo<T>(ref T dst, int dstLength, ref T src, int srcLength)
        {
            IntPtr intPtr = Unsafe.ByteOffset(ref src, ref Unsafe.Add(ref src, srcLength));
            IntPtr intPtr2 = Unsafe.ByteOffset(ref dst, ref Unsafe.Add(ref dst, dstLength));
            IntPtr intPtr3 = Unsafe.ByteOffset(ref src, ref dst);
            bool num;
            if (sizeof(IntPtr) != 4)
            {
                if ((ulong)(long)intPtr3 >= (ulong)(long)intPtr)
                {
                    num = (ulong)(long)intPtr3 > (ulong)(-(long)intPtr2);
                    goto IL_006f;
                }
            }
            else if ((uint)(int)intPtr3 >= (uint)(int)intPtr)
            {
                num = (uint)(int)intPtr3 > (uint)(-(int)intPtr2);
                goto IL_006f;
            }
            goto IL_00de;
        IL_00de:
            bool flag = ((sizeof(IntPtr) == 4) ? ((uint)(int)intPtr3 > (uint)(-(int)intPtr2)) : ((ulong)(long)intPtr3 > (ulong)(-(long)intPtr2)));
            int num2 = (flag ? 1 : (-1));
            int num3 = ((!flag) ? (srcLength - 1) : 0);
            int i;
            for (i = 0; i < (srcLength & -8); i += 8)
            {
                Unsafe.Add(ref dst, num3) = Unsafe.Add(ref src, num3);
                Unsafe.Add(ref dst, num3 + num2) = Unsafe.Add(ref src, num3 + num2);
                Unsafe.Add(ref dst, num3 + num2 * 2) = Unsafe.Add(ref src, num3 + num2 * 2);
                Unsafe.Add(ref dst, num3 + num2 * 3) = Unsafe.Add(ref src, num3 + num2 * 3);
                Unsafe.Add(ref dst, num3 + num2 * 4) = Unsafe.Add(ref src, num3 + num2 * 4);
                Unsafe.Add(ref dst, num3 + num2 * 5) = Unsafe.Add(ref src, num3 + num2 * 5);
                Unsafe.Add(ref dst, num3 + num2 * 6) = Unsafe.Add(ref src, num3 + num2 * 6);
                Unsafe.Add(ref dst, num3 + num2 * 7) = Unsafe.Add(ref src, num3 + num2 * 7);
                num3 += num2 * 8;
            }
            if (i < (srcLength & -4))
            {
                Unsafe.Add(ref dst, num3) = Unsafe.Add(ref src, num3);
                Unsafe.Add(ref dst, num3 + num2) = Unsafe.Add(ref src, num3 + num2);
                Unsafe.Add(ref dst, num3 + num2 * 2) = Unsafe.Add(ref src, num3 + num2 * 2);
                Unsafe.Add(ref dst, num3 + num2 * 3) = Unsafe.Add(ref src, num3 + num2 * 3);
                num3 += num2 * 4;
                i += 4;
            }
            for (; i < srcLength; i++)
            {
                Unsafe.Add(ref dst, num3) = Unsafe.Add(ref src, num3);
                num3 += num2;
            }
            return;
        IL_006f:
            if (!num && !IsReferenceOrContainsReferences<T>())
            {
                ref byte source = ref Unsafe.As<T, byte>(ref dst);
                ref byte source2 = ref Unsafe.As<T, byte>(ref src);
                ulong num4 = (ulong)(long)intPtr;
                uint num6;
                for (ulong num5 = 0uL; num5 < num4; num5 += num6)
                {
                    num6 = (uint)((num4 - num5 > uint.MaxValue) ? uint.MaxValue : (num4 - num5));
                    Unsafe.CopyBlock(ref Unsafe.Add(ref source, (IntPtr)(long)num5), ref Unsafe.Add(ref source2, (IntPtr)(long)num5), num6);
                }
                return;
            }
            goto IL_00de;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static IntPtr Add<T>(this IntPtr start, int index)
        {
            if (sizeof(IntPtr) == 4)
            {
                uint num = (uint)(index * Unsafe.SizeOf<T>());
                return (IntPtr)((byte*)(void*)start + num);
            }
            ulong num2 = (ulong)index * (ulong)Unsafe.SizeOf<T>();
            return (IntPtr)((byte*)(void*)start + num2);
        }

        public static bool IsReferenceOrContainsReferences<T>()
        {
            return PerTypeValues<T>.IsReferenceOrContainsReferences;
        }

        private static bool IsReferenceOrContainsReferencesCore(Type type)
        {
            if (type.GetTypeInfo().IsPrimitive)
            {
                return false;
            }
            if (!type.GetTypeInfo().IsValueType)
            {
                return true;
            }
            Type underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType != null)
            {
                type = underlyingType;
            }
            if (type.GetTypeInfo().IsEnum)
            {
                return false;
            }
            foreach (FieldInfo declaredField in type.GetTypeInfo().DeclaredFields)
            {
                if (!declaredField.IsStatic && IsReferenceOrContainsReferencesCore(declaredField.FieldType))
                {
                    return true;
                }
            }
            return false;
        }

        public unsafe static void ClearLessThanPointerSized(byte* ptr, UIntPtr byteLength)
        {
            if (sizeof(UIntPtr) == 4)
            {
                Unsafe.InitBlockUnaligned(ptr, 0, (uint)byteLength);
                return;
            }
            ulong num = (ulong)byteLength;
            uint num2 = (uint)(num & 0xFFFFFFFFu);
            Unsafe.InitBlockUnaligned(ptr, 0, num2);
            num -= num2;
            ptr += num2;
            while (num != 0)
            {
                num2 = (uint)((num >= uint.MaxValue) ? uint.MaxValue : num);
                Unsafe.InitBlockUnaligned(ptr, 0, num2);
                ptr += num2;
                num -= num2;
            }
        }

        public unsafe static void ClearLessThanPointerSized(ref byte b, UIntPtr byteLength)
        {
            if (sizeof(UIntPtr) == 4)
            {
                Unsafe.InitBlockUnaligned(ref b, 0, (uint)byteLength);
                return;
            }
            ulong num = (ulong)byteLength;
            uint num2 = (uint)(num & 0xFFFFFFFFu);
            Unsafe.InitBlockUnaligned(ref b, 0, num2);
            num -= num2;
            long num3 = num2;
            while (num != 0)
            {
                num2 = (uint)((num >= uint.MaxValue) ? uint.MaxValue : num);
                Unsafe.InitBlockUnaligned(ref Unsafe.Add(ref b, (IntPtr)num3), 0, num2);
                num3 += num2;
                num -= num2;
            }
        }

        public unsafe static void ClearPointerSizedWithoutReferences(ref byte b, UIntPtr byteLength)
        {
            IntPtr zero;
            for (zero = IntPtr.Zero; zero.LessThanEqual(byteLength - sizeof(Reg64)); zero += sizeof(Reg64))
            {
                Unsafe.As<byte, Reg64>(ref Unsafe.Add(ref b, zero)) = default(Reg64);
            }
            if (zero.LessThanEqual(byteLength - sizeof(Reg32)))
            {
                Unsafe.As<byte, Reg32>(ref Unsafe.Add(ref b, zero)) = default(Reg32);
                zero += sizeof(Reg32);
            }
            if (zero.LessThanEqual(byteLength - sizeof(Reg16)))
            {
                Unsafe.As<byte, Reg16>(ref Unsafe.Add(ref b, zero)) = default(Reg16);
                zero += sizeof(Reg16);
            }
            if (zero.LessThanEqual(byteLength - 8))
            {
                Unsafe.As<byte, long>(ref Unsafe.Add(ref b, zero)) = 0L;
                zero += 8;
            }
            if (sizeof(IntPtr) == 4 && zero.LessThanEqual(byteLength - 4))
            {
                Unsafe.As<byte, int>(ref Unsafe.Add(ref b, zero)) = 0;
                zero += 4;
            }
        }

        public static void ClearPointerSizedWithReferences(ref IntPtr ip, UIntPtr pointerSizeLength)
        {
            IntPtr intPtr = IntPtr.Zero;
            IntPtr zero = IntPtr.Zero;
            while ((zero = intPtr + 8).LessThanEqual(pointerSizeLength))
            {
                Unsafe.Add(ref ip, intPtr + 0) = default(IntPtr);
                Unsafe.Add(ref ip, intPtr + 1) = default(IntPtr);
                Unsafe.Add(ref ip, intPtr + 2) = default(IntPtr);
                Unsafe.Add(ref ip, intPtr + 3) = default(IntPtr);
                Unsafe.Add(ref ip, intPtr + 4) = default(IntPtr);
                Unsafe.Add(ref ip, intPtr + 5) = default(IntPtr);
                Unsafe.Add(ref ip, intPtr + 6) = default(IntPtr);
                Unsafe.Add(ref ip, intPtr + 7) = default(IntPtr);
                intPtr = zero;
            }
            if ((zero = intPtr + 4).LessThanEqual(pointerSizeLength))
            {
                Unsafe.Add(ref ip, intPtr + 0) = default(IntPtr);
                Unsafe.Add(ref ip, intPtr + 1) = default(IntPtr);
                Unsafe.Add(ref ip, intPtr + 2) = default(IntPtr);
                Unsafe.Add(ref ip, intPtr + 3) = default(IntPtr);
                intPtr = zero;
            }
            if ((zero = intPtr + 2).LessThanEqual(pointerSizeLength))
            {
                Unsafe.Add(ref ip, intPtr + 0) = default(IntPtr);
                Unsafe.Add(ref ip, intPtr + 1) = default(IntPtr);
                intPtr = zero;
            }
            if ((intPtr + 1).LessThanEqual(pointerSizeLength))
            {
                Unsafe.Add(ref ip, intPtr) = default(IntPtr);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe static bool LessThanEqual(this IntPtr index, UIntPtr length)
        {
            if (sizeof(UIntPtr) != 4) { return (long)index <= (long)(ulong)length; }
            return (int)index <= (int)(uint)length;
        }
    }

    internal sealed class MemoryDebugView<T>
    {
        private readonly ReadOnlyMemory<T> _memory;

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items => _memory.ToArray();

        public MemoryDebugView(Memory<T> memory)
        {
            _memory = memory;
        }

        public MemoryDebugView(ReadOnlyMemory<T> memory)
        {
            _memory = memory;
        }
    }

    /// <summary>
    /// Provides extension methods for the memory- and span-related types, such as <see cref="Memory{T}"/>, <see cref="ReadOnlyMemory{T}"/>, <see cref="Span{T}"/>, and <see cref="ReadOnlySpan{T}"/>.
    /// </summary>
    public static class MemoryExtensions
    {
        internal static readonly IntPtr StringAdjustment = MeasureStringAdjustment();

        public static ReadOnlySpan<char> Trim(this ReadOnlySpan<char> span)
        {
            return span.TrimStart().TrimEnd();
        }

        public static ReadOnlySpan<char> TrimStart(this ReadOnlySpan<char> span)
        {
            System.Int32 i;
            for (i = 0; i < span.Length && char.IsWhiteSpace(span[i]); i++) { }
            return span.Slice(i);
        }

        public static ReadOnlySpan<char> TrimEnd(this ReadOnlySpan<char> span)
        {
            int num = span.Length - 1;
            while (num >= 0 && char.IsWhiteSpace(span[num])) { num--; }
            return span.Slice(0, num + 1);
        }

        public static ReadOnlySpan<char> Trim(this ReadOnlySpan<char> span, char trimChar) { return span.TrimStart(trimChar).TrimEnd(trimChar); }

        public static ReadOnlySpan<char> TrimStart(this ReadOnlySpan<char> span, char trimChar)
        {
            int i;
            for (i = 0; i < span.Length && span[i] == trimChar; i++) { }
            return span.Slice(i);
        }

        public static ReadOnlySpan<char> TrimEnd(this ReadOnlySpan<char> span, char trimChar)
        {
            int num = span.Length - 1;
            while (num >= 0 && span[num] == trimChar) { num--; }
            return span.Slice(0, num + 1);
        }

        public static ReadOnlySpan<char> Trim(this ReadOnlySpan<char> span, ReadOnlySpan<char> trimChars) { return span.TrimStart(trimChars).TrimEnd(trimChars); }

        public static ReadOnlySpan<char> TrimStart(this ReadOnlySpan<char> span, ReadOnlySpan<char> trimChars)
        {
            if (trimChars.IsEmpty) { return span.TrimStart(); }
            int i;
            for (i = 0; i < span.Length; i++)
            {
                int num = 0;
                while (num < trimChars.Length)
                {
                    if (span[i] != trimChars[num]) { num++; continue; }
                    goto Continue;
                }
                break;
                Continue:;
            }
            return span.Slice(i);
        }

        public static ReadOnlySpan<char> TrimEnd(this ReadOnlySpan<char> span, ReadOnlySpan<char> trimChars)
        {
            if (trimChars.IsEmpty) { return span.TrimEnd(); }
            int num;
            for (num = span.Length - 1; num >= 0; num--)
            {
                int num2 = 0;
                while (num2 < trimChars.Length)
                {
                    if (span[num] != trimChars[num2]) { num2++; continue; }
                    goto Continue;
                }
                break;
                Continue:;
            }
            return span.Slice(0, num + 1);
        }

        public static bool IsWhiteSpace(this ReadOnlySpan<char> span)
        {
            for (int i = 0; i < span.Length; i++) { if (char.IsWhiteSpace(span[i]) == false) { return false; } }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T>(this Span<T> span, T value) where T : IEquatable<T>
        {
            if (typeof(T) == typeof(byte))
            {
                return SpanHelpers.IndexOf(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), Unsafe.As<T, byte>(ref value), span.Length);
            }
            if (typeof(T) == typeof(char))
            {
                return SpanHelpers.IndexOf(ref Unsafe.As<T, char>(ref MemoryMarshal.GetReference(span)), Unsafe.As<T, char>(ref value), span.Length);
            }
            return SpanHelpers.IndexOf(ref MemoryMarshal.GetReference(span), value, span.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T>(this Span<T> span, ReadOnlySpan<T> value) where T : IEquatable<T>
        {
            if (typeof(T) == typeof(byte))
            {
                return SpanHelpers.IndexOf(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), span.Length, ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(value)), value.Length);
            }
            return SpanHelpers.IndexOf(ref MemoryMarshal.GetReference(span), span.Length, ref MemoryMarshal.GetReference(value), value.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int LastIndexOf<T>(this Span<T> span, T value) where T : IEquatable<T>
        {
            if (typeof(T) == typeof(byte))
            {
                return SpanHelpers.LastIndexOf(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), Unsafe.As<T, byte>(ref value), span.Length);
            }
            if (typeof(T) == typeof(char))
            {
                return SpanHelpers.LastIndexOf(ref Unsafe.As<T, char>(ref MemoryMarshal.GetReference(span)), Unsafe.As<T, char>(ref value), span.Length);
            }
            return SpanHelpers.LastIndexOf(ref MemoryMarshal.GetReference(span), value, span.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int LastIndexOf<T>(this Span<T> span, ReadOnlySpan<T> value) where T : IEquatable<T>
        {
            if (typeof(T) == typeof(byte))
            {
                return SpanHelpers.LastIndexOf(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), span.Length, ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(value)), value.Length);
            }
            return SpanHelpers.LastIndexOf(ref MemoryMarshal.GetReference(span), span.Length, ref MemoryMarshal.GetReference(value), value.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool SequenceEqual<T>(this Span<T> span, ReadOnlySpan<T> other) where T : IEquatable<T>
        {
            int length = span.Length;
            if (default(T) != null && IsTypeComparableAsBytes<T>(out var size))
            {
                if (length == other.Length)
                {
                    return SpanHelpers.SequenceEqual(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(other)), (NUInt)length * size);
                }
                return false;
            }
            if (length == other.Length)
            {
                return SpanHelpers.SequenceEqual(ref MemoryMarshal.GetReference(span), ref MemoryMarshal.GetReference(other), length);
            }
            return false;
        }

        public static int SequenceCompareTo<T>(this Span<T> span, ReadOnlySpan<T> other) where T : IComparable<T>
        {
            if (typeof(T) == typeof(byte))
            {
                return SpanHelpers.SequenceCompareTo(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), span.Length, ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(other)), other.Length);
            }
            if (typeof(T) == typeof(char))
            {
                return SpanHelpers.SequenceCompareTo(ref Unsafe.As<T, char>(ref MemoryMarshal.GetReference(span)), span.Length, ref Unsafe.As<T, char>(ref MemoryMarshal.GetReference(other)), other.Length);
            }
            return SpanHelpers.SequenceCompareTo(ref MemoryMarshal.GetReference(span), span.Length, ref MemoryMarshal.GetReference(other), other.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T>(this ReadOnlySpan<T> span, T value) where T : IEquatable<T>
        {
            if (typeof(T) == typeof(byte))
            {
                return SpanHelpers.IndexOf(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), Unsafe.As<T, byte>(ref value), span.Length);
            }
            if (typeof(T) == typeof(char))
            {
                return SpanHelpers.IndexOf(ref Unsafe.As<T, char>(ref MemoryMarshal.GetReference(span)), Unsafe.As<T, char>(ref value), span.Length);
            }
            return SpanHelpers.IndexOf(ref MemoryMarshal.GetReference(span), value, span.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> value) where T : IEquatable<T>
        {
            if (typeof(T) == typeof(byte))
            {
                return SpanHelpers.IndexOf(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), span.Length, ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(value)), value.Length);
            }
            return SpanHelpers.IndexOf(ref MemoryMarshal.GetReference(span), span.Length, ref MemoryMarshal.GetReference(value), value.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int LastIndexOf<T>(this ReadOnlySpan<T> span, T value) where T : IEquatable<T>
        {
            if (typeof(T) == typeof(byte))
            {
                return SpanHelpers.LastIndexOf(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), Unsafe.As<T, byte>(ref value), span.Length);
            }
            if (typeof(T) == typeof(char))
            {
                return SpanHelpers.LastIndexOf(ref Unsafe.As<T, char>(ref MemoryMarshal.GetReference(span)), Unsafe.As<T, char>(ref value), span.Length);
            }
            return SpanHelpers.LastIndexOf(ref MemoryMarshal.GetReference(span), value, span.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int LastIndexOf<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> value) where T : IEquatable<T>
        {
            if (typeof(T) == typeof(byte))
            {
                return SpanHelpers.LastIndexOf(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), span.Length, ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(value)), value.Length);
            }
            return SpanHelpers.LastIndexOf(ref MemoryMarshal.GetReference(span), span.Length, ref MemoryMarshal.GetReference(value), value.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOfAny<T>(this Span<T> span, T value0, T value1) where T : IEquatable<T>
        {
            if (typeof(T) == typeof(byte))
            {
                return SpanHelpers.IndexOfAny(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), Unsafe.As<T, byte>(ref value0), Unsafe.As<T, byte>(ref value1), span.Length);
            }
            return SpanHelpers.IndexOfAny(ref MemoryMarshal.GetReference(span), value0, value1, span.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOfAny<T>(this Span<T> span, T value0, T value1, T value2) where T : IEquatable<T>
        {
            if (typeof(T) == typeof(byte))
            {
                return SpanHelpers.IndexOfAny(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), Unsafe.As<T, byte>(ref value0), Unsafe.As<T, byte>(ref value1), Unsafe.As<T, byte>(ref value2), span.Length);
            }
            return SpanHelpers.IndexOfAny(ref MemoryMarshal.GetReference(span), value0, value1, value2, span.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOfAny<T>(this Span<T> span, ReadOnlySpan<T> values) where T : IEquatable<T>
        {
            if (typeof(T) == typeof(byte))
            {
                return SpanHelpers.IndexOfAny(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), span.Length, ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(values)), values.Length);
            }
            return SpanHelpers.IndexOfAny(ref MemoryMarshal.GetReference(span), span.Length, ref MemoryMarshal.GetReference(values), values.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOfAny<T>(this ReadOnlySpan<T> span, T value0, T value1) where T : IEquatable<T>
        {
            if (typeof(T) == typeof(byte))
            {
                return SpanHelpers.IndexOfAny(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), Unsafe.As<T, byte>(ref value0), Unsafe.As<T, byte>(ref value1), span.Length);
            }
            return SpanHelpers.IndexOfAny(ref MemoryMarshal.GetReference(span), value0, value1, span.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOfAny<T>(this ReadOnlySpan<T> span, T value0, T value1, T value2) where T : IEquatable<T>
        {
            if (typeof(T) == typeof(byte))
            {
                return SpanHelpers.IndexOfAny(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), Unsafe.As<T, byte>(ref value0), Unsafe.As<T, byte>(ref value1), Unsafe.As<T, byte>(ref value2), span.Length);
            }
            return SpanHelpers.IndexOfAny(ref MemoryMarshal.GetReference(span), value0, value1, value2, span.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOfAny<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> values) where T : IEquatable<T>
        {
            if (typeof(T) == typeof(byte))
            {
                return SpanHelpers.IndexOfAny(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), span.Length, ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(values)), values.Length);
            }
            return SpanHelpers.IndexOfAny(ref MemoryMarshal.GetReference(span), span.Length, ref MemoryMarshal.GetReference(values), values.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int LastIndexOfAny<T>(this Span<T> span, T value0, T value1) where T : IEquatable<T>
        {
            if (typeof(T) == typeof(byte))
            {
                return SpanHelpers.LastIndexOfAny(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), Unsafe.As<T, byte>(ref value0), Unsafe.As<T, byte>(ref value1), span.Length);
            }
            return SpanHelpers.LastIndexOfAny(ref MemoryMarshal.GetReference(span), value0, value1, span.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int LastIndexOfAny<T>(this Span<T> span, T value0, T value1, T value2) where T : IEquatable<T>
        {
            if (typeof(T) == typeof(byte))
            {
                return SpanHelpers.LastIndexOfAny(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), Unsafe.As<T, byte>(ref value0), Unsafe.As<T, byte>(ref value1), Unsafe.As<T, byte>(ref value2), span.Length);
            }
            return SpanHelpers.LastIndexOfAny(ref MemoryMarshal.GetReference(span), value0, value1, value2, span.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int LastIndexOfAny<T>(this Span<T> span, ReadOnlySpan<T> values) where T : IEquatable<T>
        {
            if (typeof(T) == typeof(byte))
            {
                return SpanHelpers.LastIndexOfAny(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), span.Length, ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(values)), values.Length);
            }
            return SpanHelpers.LastIndexOfAny(ref MemoryMarshal.GetReference(span), span.Length, ref MemoryMarshal.GetReference(values), values.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int LastIndexOfAny<T>(this ReadOnlySpan<T> span, T value0, T value1) where T : IEquatable<T>
        {
            if (typeof(T) == typeof(byte))
            {
                return SpanHelpers.LastIndexOfAny(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), Unsafe.As<T, byte>(ref value0), Unsafe.As<T, byte>(ref value1), span.Length);
            }
            return SpanHelpers.LastIndexOfAny(ref MemoryMarshal.GetReference(span), value0, value1, span.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int LastIndexOfAny<T>(this ReadOnlySpan<T> span, T value0, T value1, T value2) where T : IEquatable<T>
        {
            if (typeof(T) == typeof(byte))
            {
                return SpanHelpers.LastIndexOfAny(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), Unsafe.As<T, byte>(ref value0), Unsafe.As<T, byte>(ref value1), Unsafe.As<T, byte>(ref value2), span.Length);
            }
            return SpanHelpers.LastIndexOfAny(ref MemoryMarshal.GetReference(span), value0, value1, value2, span.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int LastIndexOfAny<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> values) where T : IEquatable<T>
        {
            if (typeof(T) == typeof(byte))
            {
                return SpanHelpers.LastIndexOfAny(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), span.Length, ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(values)), values.Length);
            }
            return SpanHelpers.LastIndexOfAny(ref MemoryMarshal.GetReference(span), span.Length, ref MemoryMarshal.GetReference(values), values.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool SequenceEqual<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> other) where T : IEquatable<T>
        {
            int length = span.Length;
            if (default(T) != null && IsTypeComparableAsBytes<T>(out var size))
            {
                if (length == other.Length)
                {
                    return SpanHelpers.SequenceEqual(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(other)), (NUInt)length * size);
                }
                return false;
            }
            if (length == other.Length)
            {
                return SpanHelpers.SequenceEqual(ref MemoryMarshal.GetReference(span), ref MemoryMarshal.GetReference(other), length);
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SequenceCompareTo<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> other) where T : IComparable<T>
        {
            if (typeof(T) == typeof(byte))
            {
                return SpanHelpers.SequenceCompareTo(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), span.Length, ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(other)), other.Length);
            }
            if (typeof(T) == typeof(char))
            {
                return SpanHelpers.SequenceCompareTo(ref Unsafe.As<T, char>(ref MemoryMarshal.GetReference(span)), span.Length, ref Unsafe.As<T, char>(ref MemoryMarshal.GetReference(other)), other.Length);
            }
            return SpanHelpers.SequenceCompareTo(ref MemoryMarshal.GetReference(span), span.Length, ref MemoryMarshal.GetReference(other), other.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool StartsWith<T>(this Span<T> span, ReadOnlySpan<T> value) where T : IEquatable<T>
        {
            int length = value.Length;
            if (default(T) != null && IsTypeComparableAsBytes<T>(out var size))
            {
                if (length <= span.Length)
                {
                    return SpanHelpers.SequenceEqual(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(value)), (NUInt)length * size);
                }
                return false;
            }
            if (length <= span.Length)
            {
                return SpanHelpers.SequenceEqual(ref MemoryMarshal.GetReference(span), ref MemoryMarshal.GetReference(value), length);
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool StartsWith<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> value) where T : IEquatable<T>
        {
            int length = value.Length;
            if (default(T) != null && IsTypeComparableAsBytes<T>(out var size))
            {
                if (length <= span.Length)
                {
                    return SpanHelpers.SequenceEqual(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(value)), (NUInt)length * size);
                }
                return false;
            }
            if (length <= span.Length)
            {
                return SpanHelpers.SequenceEqual(ref MemoryMarshal.GetReference(span), ref MemoryMarshal.GetReference(value), length);
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EndsWith<T>(this Span<T> span, ReadOnlySpan<T> value) where T : IEquatable<T>
        {
            int length = span.Length;
            int length2 = value.Length;
            if (default(T) != null && IsTypeComparableAsBytes<T>(out var size))
            {
                if (length2 <= length)
                {
                    return SpanHelpers.SequenceEqual(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref MemoryMarshal.GetReference(span), length - length2)), ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(value)), (NUInt)length2 * size);
                }
                return false;
            }
            if (length2 <= length)
            {
                return SpanHelpers.SequenceEqual(ref Unsafe.Add(ref MemoryMarshal.GetReference(span), length - length2), ref MemoryMarshal.GetReference(value), length2);
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EndsWith<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> value) where T : IEquatable<T>
        {
            int length = span.Length;
            int length2 = value.Length;
            if (default(T) != null && IsTypeComparableAsBytes<T>(out var size))
            {
                if (length2 <= length)
                {
                    return SpanHelpers.SequenceEqual(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref MemoryMarshal.GetReference(span), length - length2)), ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(value)), (NUInt)length2 * size);
                }
                return false;
            }
            if (length2 <= length)
            {
                return SpanHelpers.SequenceEqual(ref Unsafe.Add(ref MemoryMarshal.GetReference(span), length - length2), ref MemoryMarshal.GetReference(value), length2);
            }
            return false;
        }

        public static void Reverse<T>(this Span<T> span)
        {
            ref T reference = ref MemoryMarshal.GetReference(span);
            int num = 0; int num2 = span.Length - 1;
            while (num < num2)
            {
                T val = Unsafe.Add(ref reference, num);
                Unsafe.Add(ref reference, num) = Unsafe.Add(ref reference, num2);
                Unsafe.Add(ref reference, num2) = val; num++; num2--;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> AsSpan<T>(this T[] array) { return new Span<T>(array); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> AsSpan<T>(this T[] array, int start, int length) { return new Span<T>(array, start, length); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> AsSpan<T>(this ArraySegment<T> segment) { return new Span<T>(segment.Array, segment.Offset, segment.Count); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> AsSpan<T>(this ArraySegment<T> segment, int start)
        {
            if ((uint)start > segment.Count)
            {
                System.ThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.start);
            }
            return new Span<T>(segment.Array, segment.Offset + start, segment.Count - start);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> AsSpan<T>(this ArraySegment<T> segment, int start, int length)
        {
            if ((uint)start > segment.Count)
            {
                System.ThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.start);
            }
            if ((uint)length > segment.Count - start)
            {
                System.ThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.length);
            }
            return new Span<T>(segment.Array, segment.Offset + start, length);
        }

        public static Memory<T> AsMemory<T>(this T[] array) { return new Memory<T>(array); }

        public static Memory<T> AsMemory<T>(this T[] array, int start) { return new Memory<T>(array, start); }

        public static Memory<T> AsMemory<T>(this T[] array, int start, int length) { return new Memory<T>(array, start, length); }

        public static Memory<T> AsMemory<T>(this ArraySegment<T> segment) { return new Memory<T>(segment.Array, segment.Offset, segment.Count); }

        public static Memory<T> AsMemory<T>(this ArraySegment<T> segment, int start)
        {
            if ((uint)start > segment.Count)
            {
                System.ThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.start);
            }
            return new Memory<T>(segment.Array, segment.Offset + start, segment.Count - start);
        }

        public static Memory<T> AsMemory<T>(this ArraySegment<T> segment, int start, int length)
        {
            if ((uint)start > segment.Count)
            {
                System.ThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.start);
            }
            if ((uint)length > segment.Count - start)
            {
                System.ThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.length);
            }
            return new Memory<T>(segment.Array, segment.Offset + start, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo<T>(this T[] source, Span<T> destination) { new ReadOnlySpan<T>(source).CopyTo(destination); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo<T>(this T[] source, Memory<T> destination) { source.CopyTo(destination.Span); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Overlaps<T>(this Span<T> span, ReadOnlySpan<T> other) { return ((ReadOnlySpan<T>)span).Overlaps(other); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Overlaps<T>(this Span<T> span, ReadOnlySpan<T> other, out int elementOffset) { return ((ReadOnlySpan<T>)span).Overlaps(other, out elementOffset); }

        public static bool Overlaps<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> other)
        {
            if (span.IsEmpty || other.IsEmpty)
            {
                return false;
            }
            IntPtr intPtr = Unsafe.ByteOffset(ref MemoryMarshal.GetReference(span), ref MemoryMarshal.GetReference(other));
            if (Unsafe.SizeOf<IntPtr>() == 4)
            {
                if ((uint)(int)intPtr >= (uint)(span.Length * Unsafe.SizeOf<T>()))
                {
                    return (uint)(int)intPtr > (uint)(-(other.Length * Unsafe.SizeOf<T>()));
                }
                return true;
            }
            if ((ulong)(long)intPtr >= (ulong)((long)span.Length * (long)Unsafe.SizeOf<T>()))
            {
                return (ulong)(long)intPtr > (ulong)(-((long)other.Length * (long)Unsafe.SizeOf<T>()));
            }
            return true;
        }

        public static bool Overlaps<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> other, out int elementOffset)
        {
            if (span.IsEmpty || other.IsEmpty) { elementOffset = 0; return false; }
            IntPtr intPtr = Unsafe.ByteOffset(ref MemoryMarshal.GetReference(span), ref MemoryMarshal.GetReference(other));
            if (Unsafe.SizeOf<IntPtr>() == 4)
            {
                if ((uint)(int)intPtr < (uint)(span.Length * Unsafe.SizeOf<T>()) || (uint)(int)intPtr > (uint)(-(other.Length * Unsafe.SizeOf<T>())))
                {
                    if ((int)intPtr % Unsafe.SizeOf<T>() != 0) { System.ThrowHelper.ThrowArgumentException_OverlapAlignmentMismatch(); }
                    elementOffset = (int)intPtr / Unsafe.SizeOf<T>(); return true;
                }
                elementOffset = 0; return false;
            }
            if ((ulong)(long)intPtr < (ulong)((long)span.Length * (long)Unsafe.SizeOf<T>()) || (ulong)(long)intPtr > (ulong)(-((long)other.Length * (long)Unsafe.SizeOf<T>())))
            {
                if ((long)intPtr % Unsafe.SizeOf<T>() != 0L) { System.ThrowHelper.ThrowArgumentException_OverlapAlignmentMismatch(); }
                elementOffset = (int)((long)intPtr / Unsafe.SizeOf<T>()); return true;
            }
            elementOffset = 0; return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BinarySearch<T>(this Span<T> span, IComparable<T> comparable) { return span.BinarySearch<T, IComparable<T>>(comparable); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BinarySearch<T, TComparable>(this Span<T> span, TComparable comparable) where TComparable : IComparable<T> { return BinarySearch((ReadOnlySpan<T>)span, comparable); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BinarySearch<T, TComparer>(this Span<T> span, T value, TComparer comparer) where TComparer : IComparer<T> { return ((ReadOnlySpan<T>)span).BinarySearch(value, comparer); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BinarySearch<T>(this ReadOnlySpan<T> span, IComparable<T> comparable) { return BinarySearch<T, IComparable<T>>(span, comparable); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BinarySearch<T, TComparable>(this ReadOnlySpan<T> span, TComparable comparable) where TComparable : IComparable<T> { return SpanHelpers.BinarySearch(span, comparable); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BinarySearch<T, TComparer>(this ReadOnlySpan<T> span, T value, TComparer comparer) where TComparer : IComparer<T>
        {
            if (comparer == null) { System.ThrowHelper.ThrowArgumentNullException(System.ExceptionArgument.comparer); }
            SpanHelpers.ComparerComparable<T, TComparer> comparable = new SpanHelpers.ComparerComparable<T, TComparer>(value, comparer);
            return BinarySearch(span, comparable);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsTypeComparableAsBytes<T>(out NUInt size)
        {
            if (typeof(T) == typeof(byte) || typeof(T) == typeof(sbyte)) { size = (NUInt)1; return true; }
            if (typeof(T) == typeof(char) || typeof(T) == typeof(short) || typeof(T) == typeof(ushort)) { size = (NUInt)2; return true; }
            if (typeof(T) == typeof(int) || typeof(T) == typeof(uint)) { size = (NUInt)4; return true; }
            if (typeof(T) == typeof(long) || typeof(T) == typeof(ulong)) { size = (NUInt)8; return true; }
            size = default(NUInt);
            return false;
        }

        public static Span<T> AsSpan<T>(this T[] array, int start) { return Span<T>.Create(array, start); }

        public static bool Contains(this ReadOnlySpan<char> span, ReadOnlySpan<char> value, StringComparison comparisonType) { return span.IndexOf(value, comparisonType) >= 0; }

        public static bool Equals(this ReadOnlySpan<char> span, ReadOnlySpan<char> other, StringComparison comparisonType)
        {
            switch (comparisonType)
            {
                case StringComparison.Ordinal:
                    return span.SequenceEqual(other);
                case StringComparison.OrdinalIgnoreCase:
                    if (span.Length != other.Length) { return false; }
                    return EqualsOrdinalIgnoreCase(span, other);
                default:
                    return span.ToString().Equals(other.ToString(), comparisonType);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool EqualsOrdinalIgnoreCase(ReadOnlySpan<char> span, ReadOnlySpan<char> other)
        {
            if (other.Length == 0) { return true; }
            return CompareToOrdinalIgnoreCase(span, other) == 0;
        }

        public static int CompareTo(this ReadOnlySpan<char> span, ReadOnlySpan<char> other, StringComparison comparisonType)
        {
            return comparisonType switch
            {
                StringComparison.Ordinal => span.SequenceCompareTo(other),
                StringComparison.OrdinalIgnoreCase => CompareToOrdinalIgnoreCase(span, other),
                _ => string.Compare(span.ToString(), other.ToString(), comparisonType),
            };
        }

        private unsafe static int CompareToOrdinalIgnoreCase(ReadOnlySpan<char> strA, ReadOnlySpan<char> strB)
        {
            int num = Math.Min(strA.Length, strB.Length); int num2 = num;
            fixed (char* ptr = &MemoryMarshal.GetReference(strA))
            {
                fixed (char* ptr3 = &MemoryMarshal.GetReference(strB))
                {
                    char* ptr2 = ptr; char* ptr4 = ptr3;
                    while (num != 0 && *ptr2 <= '\u007f' && *ptr4 <= '\u007f')
                    {
                        int num3 = *ptr2; int num4 = *ptr4;
                        if (num3 == num4) { ptr2++; ptr4++; num--; continue; }
                        if ((uint)(num3 - 97) <= 25u) { num3 -= 32; }
                        if ((uint)(num4 - 97) <= 25u) { num4 -= 32; }
                        if (num3 != num4) { return num3 - num4; }
                        ptr2++; ptr4++; num--;
                    }
                    if (num == 0) { return strA.Length - strB.Length; }
                    num2 -= num;
                    return string.Compare(strA.Slice(num2).ToString(), strB.Slice(num2).ToString(), StringComparison.OrdinalIgnoreCase);
                }
            }
        }

        public static int IndexOf(this ReadOnlySpan<char> span, ReadOnlySpan<char> value, StringComparison comparisonType)
        {
            if (comparisonType == StringComparison.Ordinal) { return span.IndexOf(value); }
            return span.ToString().IndexOf(value.ToString(), comparisonType);
        }

        public static int ToLower(this ReadOnlySpan<char> source, Span<char> destination, CultureInfo culture)
        {
            if (culture == null) { System.ThrowHelper.ThrowArgumentNullException(System.ExceptionArgument.culture); }
            if (destination.Length < source.Length) { return -1; }
            string text = source.ToString(); string text2 = text.ToLower(culture);
            text2.AsSpan().CopyTo(destination); return source.Length;
        }

        public static int ToLowerInvariant(this ReadOnlySpan<char> source, Span<char> destination) { return source.ToLower(destination, CultureInfo.InvariantCulture); }

        public static int ToUpper(this ReadOnlySpan<char> source, Span<char> destination, CultureInfo culture)
        {
            if (culture == null) { System.ThrowHelper.ThrowArgumentNullException(System.ExceptionArgument.culture); }
            if (destination.Length < source.Length) { return -1; }
            string text = source.ToString(); string text2 = text.ToUpper(culture);
            text2.AsSpan().CopyTo(destination); return source.Length;
        }

        public static int ToUpperInvariant(this ReadOnlySpan<char> source, Span<char> destination) { return source.ToUpper(destination, CultureInfo.InvariantCulture); }

        public static bool EndsWith(this ReadOnlySpan<char> span, ReadOnlySpan<char> value, StringComparison comparisonType)
        {
            switch (comparisonType)
            {
                case StringComparison.Ordinal:
                    return span.EndsWith(value);
                case StringComparison.OrdinalIgnoreCase:
                    if (value.Length <= span.Length) { return EqualsOrdinalIgnoreCase(span.Slice(span.Length - value.Length), value); }
                    return false;
                default:
                    {
                        string text = span.ToString();
                        string value2 = value.ToString();
                        return text.EndsWith(value2, comparisonType);
                    }
            }
        }

        public static bool StartsWith(this ReadOnlySpan<char> span, ReadOnlySpan<char> value, StringComparison comparisonType)
        {
            switch (comparisonType)
            {
                case StringComparison.Ordinal:
                    return span.StartsWith(value);
                case StringComparison.OrdinalIgnoreCase:
                    if (value.Length <= span.Length) { return EqualsOrdinalIgnoreCase(span.Slice(0, value.Length), value); }
                    return false;
                default:
                    {
                        string text = span.ToString();
                        string value2 = value.ToString();
                        return text.StartsWith(value2, comparisonType);
                    }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<char> AsSpan(this string text)
        {
            if (text == null) { return default; }
            return new ReadOnlySpan<char>(Unsafe.As<Pinnable<char>>(text), StringAdjustment, text.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<char> AsSpan(this string text, int start)
        {
            if (text == null)
            {
                if (start != 0) { System.ThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.start); }
                return default;
            }
            if ((uint)start > (uint)text.Length)
            {
                System.ThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.start);
            }
            return new ReadOnlySpan<char>(Unsafe.As<Pinnable<char>>(text), StringAdjustment + start * 2, text.Length - start);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<char> AsSpan(this string text, int start, int length)
        {
            if (text == null)
            {
                if (start != 0 || length != 0) { System.ThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.start); }
                return default;
            }
            if ((uint)start > (uint)text.Length || (uint)length > (uint)(text.Length - start))
            {
                System.ThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.start);
            }
            return new ReadOnlySpan<char>(Unsafe.As<Pinnable<char>>(text), StringAdjustment + start * 2, length);
        }

        public static ReadOnlyMemory<char> AsMemory(this string text)
        {
            if (text == null) { return default; }
            return new ReadOnlyMemory<char>(text, 0, text.Length);
        }

        public static ReadOnlyMemory<char> AsMemory(this string text, int start)
        {
            if (text == null)
            {
                if (start != 0)
                {
                    System.ThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.start);
                }
                return default;
            }
            if ((uint)start > (uint)text.Length)
            {
                System.ThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.start);
            }
            return new ReadOnlyMemory<char>(text, start, text.Length - start);
        }

        public static ReadOnlyMemory<char> AsMemory(this string text, int start, int length)
        {
            if (text == null)
            {
                if (start != 0 || length != 0) { System.ThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.start); }
                return default;
            }
            if ((uint)start > (uint)text.Length || (uint)length > (uint)(text.Length - start))
            {
                System.ThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.start);
            }
            return new ReadOnlyMemory<char>(text, start, length);
        }

        private unsafe static IntPtr MeasureStringAdjustment()
        {
            string text = "a";
            fixed (char* source = text) { return Unsafe.ByteOffset(ref Unsafe.As<Pinnable<char>>(text).Data, ref Unsafe.AsRef<char>(source)); }
        }
    }

    internal sealed class SpanDebugView<T>
    {
        private readonly T[] _array;

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items => _array;

        public SpanDebugView(Span<T> span) { _array = span.ToArray(); }

        public SpanDebugView(ReadOnlySpan<T> span) { _array = span.ToArray(); }
    }

    /// <summary>
    /// Provides a type-safe and memory-safe representation of a contiguous region of arbitrary memory.
    /// </summary>
    /// <typeparam name="T">The type of items in the <see cref="Span{T}"/>.</typeparam>
    /// <remarks>
    /// <see cref="Span{T}"/> is a <see langword="ref struct"/> that is allocated on the stack rather than on the managed heap. 
    /// Ref struct types have a number of restrictions to ensure that they cannot be promoted to the managed heap, including 
    /// that they can't be boxed, they can't be assigned to variables of type <see cref="System.Object"/>, 
    /// <see langword="dynamic"/> or to any interface type, they can't be fields in a reference type, and they can't be used 
    /// across <see langword="await"/> and <see langword="yield"/> boundaries. 
    /// In addition, calls to two methods, <see cref="Equals(object)"/> and <see cref="GetHashCode"/>, throw a <see cref="NotSupportedException"/>.
    /// </remarks>
    [DebuggerTypeProxy(typeof(SpanDebugView<>))]
    [DebuggerDisplay("{ToString(),raw}")]
    [DebuggerTypeProxy(typeof(SpanDebugView<>))]
    [DebuggerDisplay("{ToString(),raw}")]
    public readonly ref struct Span<T>
    {

        /// <summary>
        /// Provides an enumerator for the elements of a <see cref="Span{T}"/>.
        /// </summary>
        ///
        /// <remarks>
        /// The C# <see langword="foreach"/> of the C# language and the <c>For Each...Next</c> construct in Visual Basic hides the complexity of enumerators. 
        /// Instead of directly manipulating the enumerator, using <see langword="foreach"/> or <c>For Each...Next</c> is recommended. <br />
        /// <br />
        /// Initially, the enumerator is positioned before the first element in the <see cref="Span{T}"/>. 
        /// At this position, <see cref="Current"/> is undefined.
        /// You must call <see cref="MoveNext()"/> to advance the enumerator to the first item in the 
        /// <see cref="Span{T}"/> before reading the value of <see cref="Current"/>. <br />
        /// <br />
        /// <see cref="Current"/> returns the same value until <see cref="MoveNext()"/> is called. <see cref="MoveNext()"/> sets 
        /// <see cref="Current"/> to the next item in the <see cref="Span{T}"/>.
        /// <br />
        /// <br />
        /// If <see cref="MoveNext()"/> passes the end of the <see cref="Span{T}"/>, <see cref="MoveNext()"/>
        /// returns <see langword="false"/>. When the enumerator is at this state, subsequent calls to <see cref="MoveNext()"/> 
        /// also return <see langword="false"/> and <see cref="Current"/> is undefined. You cannot set <see cref="Current"/> 
        /// to the first item in the <see cref="Span{T}"/> again; you must create a new enumerator instance instead. <br /> <br />
        /// The enumerator does not have exclusive access to the <see cref="Span{T}"/>. In addition, the underlying data 
        /// on which the span is based can also be modified. 
        /// Therefore, enumerating through a span is intrinsically not a thread-safe procedure. To guarantee thread 
        /// safety during enumeration, you must implement your own synchronization. For example, the following code has a 
        /// race condition. It does not ensure that the span will be enumerated before the ClearContents method executes. 
        /// As a result, the underlying array is cleared during enumeration of the span: <br />
        /// </remarks>
        /// <code>
        /// using System;
        /// using System.Threading.Tasks;
        ///
        ///    class Program
        ///    {
        ///            private static readonly byte[] _array = new byte[5];
        ///
        ///            static void Main()
        ///            {
        ///                new Random(42).NextBytes(_array);
        ///                Span&lt;byte&gt; span = _array;
        ///
        ///                Task.Run( () => ClearContents() );
        ///
        ///               EnumerateSpan(span);
        ///            }
        ///
        ///            public static void ClearContents()
        ///            {
        ///                Task.Delay(20).Wait();
        ///                lock (_array)
        ///                {
        ///                   Array.Clear(_array, 0, _array.Length);
        ///                }
        ///            }
        ///
        ///            public static void EnumerateSpan(Span&lt;byte&gt; span)
        ///            {
        ///                foreach (byte element in span)
        ///                {
        ///                    Console.WriteLine(element);
        ///                    Task.Delay(10).Wait();
        ///                }
        ///          }
        ///     }
        /// </code>
        public ref struct Enumerator
        {
            private readonly Span<T> _span;

            private int _index;

            /// <summary>
            /// Gets a reference to the item at the current position of the enumerator.
            /// </summary>
            /// <remarks>
            /// <see cref="Current"/> is undefined under either of the following conditions:
            /// <list type="bullet">
            /// <item>Immediately after the enumerator is created, the enumerator is positioned 
            /// before the first element in the span. <see cref="MoveNext()"/> must be called 
            /// to advance the enumerator to the first element of the span before reading 
            /// the value of <see cref="Current"/>.</item>
            /// <item>The last call to <see cref="MoveNext()"/> returned <see langword="false"/>, which indicates the end of the span.</item>
            /// </list>
            /// <see cref="Current"/> returns the same value until <see cref="MoveNext()"/> is called. <see cref="MoveNext()"/> sets <see cref="Current"/> to the next item in the span.
            /// </remarks>
            public ref T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get { return ref _span[_index]; }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(Span<T> span) { _span = span; _index = -1; }

            /// <summary>
            /// Advances the enumerator to the next item of the <see cref="Span{T}"/>.
            /// </summary>
            /// <returns><see langword="true"/> if the enumerator successfully 
            /// advanced to the next item; <see langword="false"/> if the end of the 
            /// span has been passed.</returns>
            /// <remarks>
            /// After an enumerator is created, it is positioned before the first element 
            /// in the span, and the first call to <see cref="MoveNext()"/> advances the 
            /// enumerator to the first item in the span. <br />
            /// <br /> 
            /// If <see cref="MoveNext()"/> passes the end of the span, 
            /// <see cref="MoveNext()"/> returns <see langword="false"/>. When the 
            /// enumerator is at this state, subsequent calls to <see cref="MoveNext()"/> 
            /// also return <see langword="false"/>.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                int num = _index + 1;
                if (num < _span.Length) { _index = num; return true; }
                return false;
            }
        }

        private readonly Pinnable<T> _pinnable;

        private readonly IntPtr _byteOffset;

        private readonly int _length;

        /// <summary>
        /// Returns the length of the current span.
        /// </summary>
        public int Length => _length;

        /// <summary>
        /// Returns a value that indicates whether the current <see cref="Span{T}"/> is empty.
        /// </summary>
        public bool IsEmpty => _length == 0;

        /// <summary>
        /// Returns an empty <see cref="Span{T}"/> object.
        /// </summary>
        public static Span<T> Empty => default(Span<T>);

        /// <summary>
        /// Gets the element at the specified zero-based index.
        /// </summary>
        /// <param name="index">The zero-based index of the element.</param>
        /// <returns>The element at the specified index.</returns>
        /// <exception cref="System.IndexOutOfRangeException"><paramref name="index"/> is less than zero or greater than or equal to <see cref="Length"/>.</exception>
        public unsafe ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if ((uint)index >= (uint)_length) { System.ThrowHelper.ThrowIndexOutOfRangeException(); }
                if (_pinnable == null) { return ref Unsafe.Add(ref Unsafe.AsRef<T>(_byteOffset.ToPointer()), index); }
                return ref Unsafe.Add(ref Unsafe.AddByteOffset(ref _pinnable.Data, _byteOffset), index);
            }
        }

        internal Pinnable<T> Pinnable => _pinnable;

        internal IntPtr ByteOffset => _byteOffset;

        /// <summary>
        /// Returns a value that indicates whether two <see cref="Span{T}"/> objects are not equal.
        /// </summary>
        /// <param name="left">The first span to compare.</param>
        /// <param name="right">The second span to compare.</param>
        /// <returns><see langword="true"/> if the two <see cref="Span{T}"/> objects are not equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(Span<T> left, Span<T> right) { return (left == right) == false; }

#pragma warning disable CS0809
        /// <inheritdoc />
        [Obsolete("Equals() on Span will always throw an exception. Use == instead." , false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj) { throw new NotSupportedException(MDCFR.Properties.Resources.NotSupported_CannotCallEqualsOnSpan); }

        /// <inheritdoc />
        [Obsolete("GetHashCode() on Span will always throw an exception." , false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() { throw new NotSupportedException(MDCFR.Properties.Resources.NotSupported_CannotCallGetHashCodeOnSpan); }
#pragma warning restore CS0809

        /// <summary>
        /// Defines an implicit conversion of an array to a <see cref="Span{T}"/>.
        /// </summary>
        /// <param name="array">The array to convert to a <see cref="Span{T}"/>.</param>
        public static implicit operator Span<T>(T[] array) { return new Span<T>(array); }

        /// <summary>
        /// Defines an implicit conversion of an <see cref="ArraySegment{T}"/> to a <see cref="Span{T}"/>.
        /// </summary>
        /// <param name="segment">The array segment to be converted to a <see cref="Span{T}"/>.</param>
        public static implicit operator Span<T>(ArraySegment<T> segment) { return new Span<T>(segment.Array, segment.Offset, segment.Count); }

        /// <summary>
        /// Returns an enumerator for this <see cref="Span{T}"/>.
        /// </summary>
        /// <returns>An enumerator for this span.</returns>
        /// <remarks>
        /// Instead of calling the <see cref="GetEnumerator"/> 
        /// method directly, you can use the C# <see langword="foreach"/> 
        /// statement and the Visual Basic <c>For Each...Next</c> 
        /// construct to enumerate a <see cref="Span{T}"/>.
        /// </remarks>
        public Enumerator GetEnumerator() { return new Enumerator(this); }

        /// <summary>
        /// Creates a new <see cref="Span{T}"/> object over the entirety of a specified array.
        /// </summary>
        /// <param name="array">The array from which to create the <see cref="Span{T}"/> object.</param>
        /// <exception cref="System.ArrayTypeMismatchException">
        /// <typeparamref name="T"/> is a reference type, and <paramref name="array"/> is not an array 
        /// of type <typeparamref name="T"/>.
        /// </exception>
        /// <remarks>
        /// If <paramref name="array"/> is <see langword="null"/>, this constructor returns a <see langword="null"/> <see cref="Span{T}"/>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span(T[] array)
        {
            if (array == null) { this = default; return; }
            if (default(T) == null && array.GetType() != typeof(T[]))
            {
                System.ThrowHelper.ThrowArrayTypeMismatchException();
            }
            _length = array.Length;
            _pinnable = Unsafe.As<Pinnable<T>>(array);
            _byteOffset = SpanHelpers.PerTypeValues<T>.ArrayAdjustment;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Span<T> Create(T[] array, int start)
        {
            if (array == null)
            {
                if (start != 0)
                {
                    System.ThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.start);
                }
                return default;
            }
            if (default(T) == null && array.GetType() != typeof(T[]))
            {
                System.ThrowHelper.ThrowArrayTypeMismatchException();
            }
            if ((uint)start > (uint)array.Length)
            {
                System.ThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.start);
            }
            IntPtr byteOffset = SpanHelpers.PerTypeValues<T>.ArrayAdjustment.Add<T>(start);
            int length = array.Length - start;
            return new Span<T>(Unsafe.As<Pinnable<T>>(array), byteOffset, length);
        }

        /// <summary>
        /// Creates a new <see cref="Span{T}"/> object that includes a specified number of elements of an array starting at a specified index.
        /// </summary>
        /// <param name="array">The source array.</param>
        /// <param name="start">The index of the first element to include in the new <see cref="Span{T}"/>.</param>
        /// <param name="length">The number of elements to include in the new <see cref="Span{T}"/>.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// <paramref name="array"/> is <see langword="null"/>, but <paramref name="start"/> or <paramref name="length"/> is non-zero.<br /> <br />
        /// -or- <br /> <br />
        /// <paramref name="start"/> is outside the bounds of the array.
        /// -or- <br /> <br />
        /// <paramref name="start"/> and <paramref name="length"/> exceeds the number of elements in the array.
        /// </exception>
        /// <exception cref="System.ArrayTypeMismatchException">
        /// <typeparamref name="T"/> is a reference type, and <paramref name="array"/> is not an array 
        /// of type <typeparamref name="T"/>.
        /// </exception>
        /// <remarks>This constructor returns <see langword="default"/> when <paramref name="array"/> is <see langword="null"/>.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span(T[] array, int start, int length)
        {
            if (array == null)
            {
                if (start != 0 || length != 0)
                {
                    System.ThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.start);
                }
                this = default; return;
            }
            if (default(T) == null && array.GetType() != typeof(T[])) { System.ThrowHelper.ThrowArrayTypeMismatchException(); }
            if ((uint)start > (uint)array.Length || (uint)length > (uint)(array.Length - start))
            {
                System.ThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.start);
            }
            _length = length;
            _pinnable = Unsafe.As<Pinnable<T>>(array);
            _byteOffset = SpanHelpers.PerTypeValues<T>.ArrayAdjustment.Add<T>(start);
        }

        /// <summary>
        /// Creates a new <see cref="Span{T}"/> object from a specified 
        /// number of <typeparamref name="T"/> elements starting at a specified memory address.
        /// </summary>
        /// <param name="pointer">A pointer to the starting address of a specified number of 
        /// <typeparamref name="T"/> elements in memory.</param>
        /// <param name="length">The number of <typeparamref name="T"/> elements to be included 
        /// in the <see cref="Span{T}"/>.</param>
        /// <exception cref="System.ArgumentException">
        /// <typeparamref name="T"/> is a reference type or contains pointers and therefore cannot be stored in unmanaged memory.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="length"/> is negative.</exception>
        /// <remarks>This constructor should be used with care, since it creates arbitrarily typed <typeparamref name="T"/>'s from a 
        /// <see cref="System.Void"/>*-typed block of memory, and length is not checked. However, if the constructor succeeds in 
        /// creating a new <see cref="Span{T}"/> object, all subsequent uses are correct.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public unsafe Span(void* pointer, int length)
        {
            if (SpanHelpers.IsReferenceOrContainsReferences<T>()) { System.ThrowHelper.ThrowArgumentException_InvalidTypeWithPointersNotSupported(typeof(T)); }
            if (length < 0) { System.ThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.start); }
            _length = length;
            _pinnable = null;
            _byteOffset = new IntPtr(pointer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Span(Pinnable<T> pinnable, IntPtr byteOffset, int length) { _length = length; _pinnable = pinnable; _byteOffset = byteOffset; }

        /// <summary>
        /// Returns a reference to an object of type <typeparamref name="T"/> that can be used for pinning. <br /> <br />
        /// This method is intended to support .NET compilers and is not intended to be called by user code.
        /// </summary>
        /// <returns>A reference to the element of the span at index 0, or <see langword="null"/> if the span is empty.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public unsafe ref T GetPinnableReference()
        {
            if (_length != 0)
            {
                if (_pinnable == null) { return ref Unsafe.AsRef<T>(_byteOffset.ToPointer()); }
                return ref Unsafe.AddByteOffset(ref _pinnable.Data, _byteOffset);
            }
            return ref Unsafe.AsRef<T>(null);
        }

        /// <summary>
        /// Clears the contents of this <see cref="Span{T}"/> object.
        /// </summary>
        /// <remarks>
        /// The <see cref="Clear"/> method sets the items 
        /// in the <see cref="Span{T}"/> object to their default values. <br />
        /// It does not remove items from the <see cref="Span{T}"/>.
        /// </remarks>
        public unsafe void Clear()
        {
            int length = _length;
            if (length == 0) { return; }
            UIntPtr byteLength = (UIntPtr)(ulong)((uint)length * Unsafe.SizeOf<T>());
            if ((Unsafe.SizeOf<T>() & (sizeof(IntPtr) - 1)) != 0)
            {
                if (_pinnable == null)
                {
                    byte* ptr = (byte*)_byteOffset.ToPointer();
                    SpanHelpers.ClearLessThanPointerSized(ptr, byteLength);
                }
                else
                {
                    SpanHelpers.ClearLessThanPointerSized(ref Unsafe.As<T, byte>(ref Unsafe.AddByteOffset(ref _pinnable.Data, _byteOffset)), byteLength);
                }
            }
            else if (SpanHelpers.IsReferenceOrContainsReferences<T>())
            {
                UIntPtr pointerSizeLength = (UIntPtr)(ulong)(length * Unsafe.SizeOf<T>() / sizeof(IntPtr));
                SpanHelpers.ClearPointerSizedWithReferences(ref Unsafe.As<T, IntPtr>(ref DangerousGetPinnableReference()), pointerSizeLength);
            }
            else
            {
                SpanHelpers.ClearPointerSizedWithoutReferences(ref Unsafe.As<T, byte>(ref DangerousGetPinnableReference()), byteLength);
            }
        }

        /// <summary>
        /// Fills the elements of this span with a specified value.
        /// </summary>
        /// <param name="value">The value to assign to each element of the span.</param>
        public unsafe void Fill(T value)
        {
            int length = _length;
            if (length == 0) { return; }
            if (Unsafe.SizeOf<T>() == 1)
            {
                byte value2 = Unsafe.As<T, byte>(ref value);
                if (_pinnable == null)
                {
                    Unsafe.InitBlockUnaligned(_byteOffset.ToPointer(), value2, (uint)length);
                }
                else
                {
                    Unsafe.InitBlockUnaligned(ref Unsafe.As<T, byte>(ref Unsafe.AddByteOffset(ref _pinnable.Data, _byteOffset)), value2, (uint)length);
                }
                return;
            }
            ref T source = ref DangerousGetPinnableReference();
            int i;
            for (i = 0; i < (length & -8); i += 8)
            {
                Unsafe.Add(ref source, i) = value;
                Unsafe.Add(ref source, i + 1) = value;
                Unsafe.Add(ref source, i + 2) = value;
                Unsafe.Add(ref source, i + 3) = value;
                Unsafe.Add(ref source, i + 4) = value;
                Unsafe.Add(ref source, i + 5) = value;
                Unsafe.Add(ref source, i + 6) = value;
                Unsafe.Add(ref source, i + 7) = value;
            }
            if (i < (length & -4))
            {
                Unsafe.Add(ref source, i) = value;
                Unsafe.Add(ref source, i + 1) = value;
                Unsafe.Add(ref source, i + 2) = value;
                Unsafe.Add(ref source, i + 3) = value;
                i += 4;
            }
            for (; i < length; i++) { Unsafe.Add(ref source, i) = value; }
        }

        /// <summary>
        /// Copies the contents of this <see cref="Span{T}"/> into a destination <see cref="Span{T}"/>.
        /// </summary>
        /// <param name="destination">The destination <see cref="Span{T}"/> object.</param>
        /// <exception cref="System.ArgumentException"><paramref name="destination"/> is shorter 
        /// than the source <see cref="Span{T}"/>.</exception>
        /// <remarks>This method copies all of source to <paramref name="destination"/> even if 
        /// source and <paramref name="destination"/> overlap.</remarks>
        public void CopyTo(Span<T> destination) { if (TryCopyTo(destination) == false) { System.ThrowHelper.ThrowArgumentException_DestinationTooShort(); } }

        /// <summary>
        /// Attempts to copy the current <see cref="Span{T}"/> to a destination 
        /// <see cref="Span{T}"/> and returns a value that indicates whether 
        /// the copy operation succeeded.
        /// </summary>
        /// <param name="destination">The target of the copy operation.</param>
        /// <returns><see langword="true"/> if the copy operation succeeded; otherwise, <see langword="false"/>.</returns>
        public bool TryCopyTo(Span<T> destination)
        {
            int length = _length;
            int length2 = destination._length;
            if (length == 0) { return true; }
            if ((uint)length > (uint)length2) { return false; }
            ref T src = ref DangerousGetPinnableReference();
            SpanHelpers.CopyTo(ref destination.DangerousGetPinnableReference(), length2, ref src, length);
            return true;
        }

        /// <summary>
        /// Returns a value that indicates whether two <see cref="Span{T}"/> objects are equal.
        /// </summary>
        /// <param name="left">The first span to compare.</param>
        /// <param name="right">The second span to compare.</param>
        /// <returns><see langword="true"/> if the two <see cref="Span{T}"/> objects are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(Span<T> left, Span<T> right)
        {
            if (left._length == right._length) { return Unsafe.AreSame(ref left.DangerousGetPinnableReference(), ref right.DangerousGetPinnableReference()); }
            return false;
        }

        /// <summary>
        /// Defines an implicit conversion of a <see cref="Span{T}"/> to a <see cref="ReadOnlySpan{T}"/>.
        /// </summary>
        /// <param name="span">The object to convert to a <see cref="ReadOnlySpan{T}"/>.</param>
        public static implicit operator ReadOnlySpan<T>(Span<T> span) { return new ReadOnlySpan<T>(span._pinnable, span._byteOffset, span._length); }

        /// <summary>
        /// Returns the string representation of this <see cref="Span{T}"/> object.
        /// </summary>
        /// <returns>The string representation of this <see cref="Span{T}"/> object.</returns>
        /// <remarks>
        /// For a Span&lt;<see cref="System.Char"/>&gt;, the <see cref="ToString"/> method returns a 
        /// <see cref="System.String"/> that contains the characters pointed to by the 
        /// <see cref="Span{T}"/>. Otherwise, it returns a <see cref="System.String"/> 
        /// with the name of the type and the number of elements that the <see cref="Span{T}"/> contains.
        /// </remarks>
        public unsafe override string ToString()
        {
            if (typeof(T) == typeof(char))
            {
                fixed (char* value = &Unsafe.As<T, char>(ref DangerousGetPinnableReference())) { return new string(value, 0, _length); }
            }
            return $"System.Span<{typeof(T).Name}>[{_length}]";
        }

        /// <summary>
        /// Forms a slice out of the current span that begins at a specified index.
        /// </summary>
        /// <param name="start">The index at which to begin the slice.</param>
        /// <returns>A span that consists of all elements of the current span from <paramref name="start"/> to the end of the span.</returns>
        /// <exception cref="System.ArgumentException"><paramref name="start"/> is less than zero or greater than <see cref="Length"/>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> Slice(int start)
        {
            if ((uint)start > (uint)_length)
            {
                System.ThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.start);
            }
            IntPtr byteOffset = _byteOffset.Add<T>(start);
            int length = _length - start;
            return new Span<T>(_pinnable, byteOffset, length);
        }

        /// <summary>
        /// Forms a slice out of the current span starting at a specified index for a specified length.
        /// </summary>
        /// <param name="start">The index at which to begin this slice.</param>
        /// <param name="length">The desired length for the slice.</param>
        /// <returns>A span that consists of <paramref name="length"/> elements
        /// from the current span starting at <paramref name="start"/>.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// <paramref name="start"/> or 
        /// <c><paramref name="start"/> + <paramref name="length"/></c> 
        /// is less than zero or greater than <see cref="Length"/>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> Slice(int start, int length)
        {
            if ((uint)start > (uint)_length || (uint)length > (uint)(_length - start))
            {
                System.ThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.start);
            }
            IntPtr byteOffset = _byteOffset.Add<T>(start);
            return new Span<T>(_pinnable, byteOffset, length);
        }

        /// <summary>
        /// Copies the contents of this span into a new array.
        /// </summary>
        /// <returns>An array containing the data in the current span.</returns>
        /// <remarks>
        /// This method performs a heap allocation and therefore should be avoided if possible. 
        /// Heap allocations are expected in APIs that work with arrays. 
        /// Using such APIs is unavoidable if an alternative API overload that takes a 
        /// <see cref="Span{T}"/> does not exist.
        /// </remarks>
        public T[] ToArray()
        {
            if (_length == 0) { return SpanHelpers.PerTypeValues<T>.EmptyArray; }
            T[] array = new T[_length];
            CopyTo(array);
            return array;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal unsafe ref T DangerousGetPinnableReference()
        {
            if (_pinnable == null) { return ref Unsafe.AsRef<T>(_byteOffset.ToPointer()); }
            return ref Unsafe.AddByteOffset(ref _pinnable.Data, _byteOffset);
        }
    }

    /// <summary>
    /// Represents a contiguous region of memory.
    /// </summary>
    /// <typeparam name="T">The type of items in the <see cref="Memory{T}"/>.</typeparam>
    /// <remarks>
    /// Like <see cref="Span{T}"/>, <see cref="Memory{T}"/> represents a contiguous region 
    /// of memory. Unlike <see cref="Span{T}"/>, however, <see cref="Memory{T}"/> is not a 
    /// ref struct. This means that <see cref="Memory{T}"/> can be placed on the managed heap,
    /// whereas <see cref="Span{T}"/> cannot. As a result, the <see cref="Memory{T}"/> 
    /// structure does not have the same restrictions as a <see cref="Span{T}"/> 
    /// instance. In particular:
    /// <list type="bullet">
    /// <item>It can be used as a field in a class.</item>
    /// <item>It can be used across <see langword="await"/> and <see langword="yield"/> boundaries.</item>
    /// </list>
    /// In addition to <see cref="Memory{T}"/>, you can use <see cref="System.ReadOnlyMemory{T}"/> to
    /// represent immutable or read-only memory.
    /// </remarks>
    [DebuggerTypeProxy(typeof(MemoryDebugView<>))]
    [DebuggerDisplay("{ToString(),raw}")]
    public readonly struct Memory<T>
    {
        private readonly object _object;

        private readonly int _index;

        private readonly int _length;

        private const int RemoveFlagsBitMask = int.MaxValue;

        /// <summary>
        /// Returns an empty <see cref="Memory{T}"/> object.
        /// </summary>
        public static Memory<T> Empty => default(Memory<T>);

        /// <summary>
        /// Gets the number of items in the current instance.
        /// </summary>
        public int Length => _length & 0x7FFFFFFF;

        /// <summary>
        /// Indicates whether the current instance is empty.
        /// </summary>
        public bool IsEmpty => (_length & 0x7FFFFFFF) == 0;

        /// <summary>
        /// Returns a span from the current instance.
        /// </summary>
        public Span<T> Span
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                Span<T> result;
                if (_index < 0)
                {
                    result = ((MemoryManager<T>)_object).GetSpan();
                    return result.Slice(_index & 0x7FFFFFFF, _length);
                }
                if (typeof(T) == typeof(char) && _object is string text)
                {
                    result = new Span<T>(Unsafe.As<Pinnable<T>>(text), MemoryExtensions.StringAdjustment, text.Length);
                    return result.Slice(_index, _length);
                }
                if (_object != null)
                {
                    return new Span<T>((T[])_object, _index, _length & 0x7FFFFFFF);
                }
                result = default(Span<T>);
                return result;
            }
        }

        /// <summary>
        /// Creates a new <see cref="Memory{T}"/> object over the entirety of a specified array.
        /// </summary>
        /// <param name="array">The array from which to create the <see cref="Memory{T}"/> object.</param>
        /// <exception cref="System.ArrayTypeMismatchException">
        /// <typeparamref name="T"/> is a reference type, 
        /// and <paramref name="array"/> 
        /// is not an array of type <typeparamref name="T"/>. <br /> <br />
        /// -or- <br /> <br />
        /// The array is <see href="https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/covariance-contravariance">covariant</see>.
        /// </exception>
        /// <remarks>If <paramref name="array"/> is <see langword="null"/>, this constructor returns a <see cref="Memory{T}"/> object with a <see langword="default&lt;T&gt;"/> value.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Memory(T[] array)
        {
            if (array == null)
            {
                this = default(Memory<T>);
                return;
            }
            if (default(T) == null && array.GetType() != typeof(T[]))
            {
                System.ThrowHelper.ThrowArrayTypeMismatchException();
            }
            _object = array;
            _index = 0;
            _length = array.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Memory(T[] array, int start)
        {
            if (array == null)
            {
                if (start != 0)
                {
                    System.ThrowHelper.ThrowArgumentOutOfRangeException();
                }
                this = default(Memory<T>);
                return;
            }
            if (default(T) == null && array.GetType() != typeof(T[]))
            {
                System.ThrowHelper.ThrowArrayTypeMismatchException();
            }
            if ((uint)start > (uint)array.Length)
            {
                System.ThrowHelper.ThrowArgumentOutOfRangeException();
            }
            _object = array;
            _index = start;
            _length = array.Length - start;
        }

        /// <summary>
        /// Creates a new <see cref="Memory{T}"/> object that includes a specified number of elements of an array beginning at a specified index.
        /// </summary>
        /// <param name="array">The source array.</param>
        /// <param name="start">The index of the first element to include in the new <see cref="Memory{T}"/>.</param>
        /// <param name="length">The number of elements to include in the new <see cref="Memory{T}"/>.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// <paramref name="array"/> is <see langword="null"/>, but <paramref name="start"/> or <paramref name="length"/> is non-zero. <br /> <br />
        /// -or- <br /> <br />
        /// <paramref name="start"/> is outside the bounds of the <paramref name="array"/>.
        /// -or- <br /> <br />
        /// <paramref name="start"/> and <paramref name="length"/> exceeds the number of elements in the <paramref name="array"/>.
        /// </exception>
        /// <exception cref="System.ArrayTypeMismatchException">
        /// <typeparamref name="T"/> is a reference type, and <paramref name="array"/> is not an array of type <typeparamref name="T"/>.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Memory(T[] array, int start, int length)
        {
            if (array == null)
            {
                if (start != 0 || length != 0)
                {
                    System.ThrowHelper.ThrowArgumentOutOfRangeException();
                }
                this = default(Memory<T>);
                return;
            }
            if (default(T) == null && array.GetType() != typeof(T[]))
            {
                System.ThrowHelper.ThrowArrayTypeMismatchException();
            }
            if ((uint)start > (uint)array.Length || (uint)length > (uint)(array.Length - start))
            {
                System.ThrowHelper.ThrowArgumentOutOfRangeException();
            }
            _object = array;
            _index = start;
            _length = length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Memory(MemoryManager<T> manager, int length)
        {
            if (length < 0)
            {
                System.ThrowHelper.ThrowArgumentOutOfRangeException();
            }
            _object = manager;
            _index = int.MinValue;
            _length = length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Memory(MemoryManager<T> manager, int start, int length)
        {
            if (length < 0 || start < 0)
            {
                System.ThrowHelper.ThrowArgumentOutOfRangeException();
            }
            _object = manager;
            _index = start | int.MinValue;
            _length = length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Memory(object obj, int start, int length)
        {
            _object = obj;
            _index = start;
            _length = length;
        }

        /// <summary>
        /// Defines an implicit conversion of an array to a <see cref="Memory{T}"/> object.
        /// </summary>
        /// <param name="array">The array to convert.</param>
        public static implicit operator Memory<T>(T[] array) { return new Memory<T>(array); }

        /// <summary>
        /// Defines an implicit conversion of an <see cref="ArraySegment{T}"/> object to a <see cref="Memory{T}"/> object.
        /// </summary>
        /// <param name="segment">The object to convert.</param>
        public static implicit operator Memory<T>(ArraySegment<T> segment) { return new Memory<T>(segment.Array, segment.Offset, segment.Count); }

        /// <summary>
        /// Defines an implicit conversion of a <see cref="Memory{T}"/> object to a <see cref="ReadOnlyMemory{T}"/> object.
        /// </summary>
        /// <param name="memory">The object to convert.</param>
        public static implicit operator ReadOnlyMemory<T>(Memory<T> memory) { return Unsafe.As<Memory<T>, ReadOnlyMemory<T>>(ref memory); }

        /// <summary>
        /// Returns the string representation of this <see cref="Memory{T}"/> object.
        /// </summary>
        /// <returns>The string representation of this <see cref="Memory{T}"/> object.</returns>
        /// <remarks>
        /// For a Memory&lt;<see cref="System.Char"/>&gt;, the <see cref="ToString"/> method returns a 
        /// <see cref="System.String"/> that contains the characters pointed to by the 
        /// <see cref="Memory{T}"/>. Otherwise, it returns a <see cref="System.String"/> 
        /// with the name of the type and the number of elements that the <see cref="Memory{T}"/> contains.
        /// </remarks>
        public override string ToString()
        {
            if (typeof(T) == typeof(char))
            {
                if (!(_object is string text)) { return Span.ToString(); }
                return text.Substring(_index, _length & 0x7FFFFFFF);
            }
            return $"System.Memory<{typeof(T).Name}>[{_length & 0x7FFFFFFF}]";
        }

        /// <summary>
        /// Forms a slice out of the current memory that begins at a specified index.
        /// </summary>
        /// <param name="start">The index at which to begin the slice.</param>
        /// <returns>An object that contains all elements of the current instance from <paramref name="start"/> to the end of the instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="start"/> is less than zero or greater than <see cref="Length"/>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Memory<T> Slice(int start)
        {
            int length = _length;
            int num = length & 0x7FFFFFFF;
            if ((uint)start > (uint)num)
            {
                System.ThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.start);
            }
            return new Memory<T>(_object, _index + start, length - start);
        }

        /// <summary>
        /// Forms a slice out of the current memory starting at a specified index for a specified length.
        /// </summary>
        /// <param name="start">The index at which to begin the slice.</param>
        /// <param name="length">The number of elements to include in the slice.</param>
        /// <returns>An object that contains <paramref name="length"/> elements from the current instance starting at <paramref name="start"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="start"/> is less than zero or greater than <see cref="Length"/>. <br /> <br />
        /// -or- <br /> <br />
        /// <paramref name="length"/> is greater than <c><see cref="Length"/> - <paramref name="start"/></c>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Memory<T> Slice(int start, int length)
        {
            int length2 = _length;
            int num = length2 & 0x7FFFFFFF;
            if ((uint)start > (uint)num || (uint)length > (uint)(num - start))
            {
                System.ThrowHelper.ThrowArgumentOutOfRangeException();
            }
            return new Memory<T>(_object, _index + start, length | (length2 & int.MinValue));
        }

        /// <summary>
        /// Copies the contents of a <see cref="Memory{T}"/> object into a destination <see cref="Memory{T}"/> object.
        /// </summary>
        /// <param name="destination">The destination <see cref="Memory{T}"/> object.</param>
        /// <exception cref="System.ArgumentException">The length of <paramref name="destination"/> is less than the length of the current instance.</exception>
        /// <remarks>This method copies all of the contents of the current <see cref="Memory{T}"/> instance to <paramref name="destination"/> even if the contents of the 
        /// current instance and <paramref name="destination"/> overlap.</remarks>
        public void CopyTo(Memory<T> destination) { Span.CopyTo(destination.Span); }

        /// <summary>
        /// Copies the contents of a <see cref="Memory{T}"/> object into a destination <see cref="Memory{T}"/> object.
        /// </summary>
        /// <param name="destination">The destination <see cref="Memory{T}"/> object.</param>
        /// <returns><see langword="true" /> if the copy operation succeeds; otherwise, <see langword="false" />.</returns>
        /// <exception cref="System.ArgumentException">The length of <paramref name="destination"/> is less than the length of the current instance.</exception>
        /// <remarks>This method copies all of the contents of the current <see cref="Memory{T}"/> instance to <paramref name="destination"/> even if the contents of the 
        /// current instance and <paramref name="destination"/> overlap.</remarks>
        public bool TryCopyTo(Memory<T> destination) { return Span.TryCopyTo(destination.Span); }

        /// <summary>
        /// Creates a handle for the <see cref="Memory{T}"/> object.
        /// </summary>
        /// <returns>A handle for the <see cref="Memory{T}"/> object.</returns>
        /// <exception cref="System.ArgumentException">An instance with non-primitive (non-blittable) members cannot be pinned.</exception>
        /// <remarks>
        /// The garbage collector will not move the memory until the returned <see cref="MemoryHandle"/> object is disposed. 
        /// This enables you to retrieve and use the memory's address. <br /> <br />
        /// See also: <seealso cref="System.Type.IsPrimitive"/>
        /// </remarks>
        public unsafe MemoryHandle Pin()
        {
            if (_index < 0) { return ((MemoryManager<T>)_object).Pin(_index & 0x7FFFFFFF); }
            if (typeof(T) == typeof(char) && _object is string value)
            {
                GCHandle handle = GCHandle.Alloc(value, GCHandleType.Pinned);
                void* pointer = Unsafe.Add<T>((void*)handle.AddrOfPinnedObject(), _index);
                return new MemoryHandle(pointer, handle);
            }
            if (_object is T[] array)
            {
                if (_length < 0)
                {
                    void* pointer2 = Unsafe.Add<T>(Unsafe.AsPointer(ref MemoryMarshal.GetReference<T>(array)), _index);
                    return new MemoryHandle(pointer2);
                }
                GCHandle handle2 = GCHandle.Alloc(array, GCHandleType.Pinned);
                void* pointer3 = Unsafe.Add<T>((void*)handle2.AddrOfPinnedObject(), _index);
                return new MemoryHandle(pointer3, handle2);
            }
            return default(MemoryHandle);
        }

        /// <summary>
        /// Copies the contents from the memory into a new array.
        /// </summary>
        /// <returns>An array containing the elements in the current memory.</returns>
        /// <remarks>
        /// This method performs a heap allocation and therefore should be avoided if possible. 
        /// However, it is sometimes necessary to take advantage of functionality that is only available for arrays. <br />
        /// Each call to the <see cref="ToArray"/> method returns a new array.
        /// </remarks>
        public T[] ToArray() { return Span.ToArray(); }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><see langword="true"/> if the current instance and other are equal; otherwise, <see langword="false"/>.</returns>
        /// <remarks>Two <see cref="Memory{T}"/> objects are equal if both objects point to the same array and have the same length. 
        /// Note that the test for equality does not check whether the contents are equal.</remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
        {
            if (obj is ReadOnlyMemory<T> readOnlyMemory) { return readOnlyMemory.Equals(this); }
            if (obj is Memory<T> other) { return Equals(other); }
            return false;
        }

        /// <summary>
        /// Determines whether the specified <see cref="Memory{T}"/> object is equal to the current object.
        /// </summary>
        /// <param name="other">The object to compare with the current instance.</param>
        /// <returns><see langword="true"/> if the current instance and other are equal; otherwise, <see langword="false"/>.</returns>
        public bool Equals(Memory<T> other)
        {
            if (_object == other._object && _index == other._index) { return _length == other._length; }
            return false;
        }

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            if (_object == null) { return 0; }
            return CombineHashCodes(_object.GetHashCode(), _index.GetHashCode(), _length.GetHashCode());
        }

        private static int CombineHashCodes(int left, int right) { return ((left << 5) + left) ^ right; }

        private static int CombineHashCodes(int h1, int h2, int h3) { return CombineHashCodes(CombineHashCodes(h1, h2), h3); }
    }

    [DebuggerTypeProxy(typeof(MemoryDebugView<>))]
    [DebuggerDisplay("{ToString(),raw}")]
    public readonly struct ReadOnlyMemory<T>
    {
        private readonly object _object;

        private readonly int _index;

        private readonly int _length;

        internal const int RemoveFlagsBitMask = int.MaxValue;

        public static ReadOnlyMemory<T> Empty => default(ReadOnlyMemory<T>);

        public int Length => _length & 0x7FFFFFFF;

        public bool IsEmpty => (_length & 0x7FFFFFFF) == 0;

        public ReadOnlySpan<T> Span
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (_index < 0)
                {
                    return ((MemoryManager<T>)_object).GetSpan().Slice(_index & 0x7FFFFFFF, _length);
                }
                ReadOnlySpan<T> result;
                if (typeof(T) == typeof(char) && _object is string text)
                {
                    result = new ReadOnlySpan<T>(Unsafe.As<Pinnable<T>>(text), MemoryExtensions.StringAdjustment, text.Length);
                    return result.Slice(_index, _length);
                }
                if (_object != null)
                {
                    return new ReadOnlySpan<T>((T[])_object, _index, _length & 0x7FFFFFFF);
                }
                result = default(ReadOnlySpan<T>);
                return result;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory(T[] array)
        {
            if (array == null)
            {
                this = default(ReadOnlyMemory<T>);
                return;
            }
            _object = array;
            _index = 0;
            _length = array.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory(T[] array, int start, int length)
        {
            if (array == null)
            {
                if (start != 0 || length != 0)
                {
                    System.ThrowHelper.ThrowArgumentOutOfRangeException();
                }
                this = default(ReadOnlyMemory<T>);
                return;
            }
            if ((uint)start > (uint)array.Length || (uint)length > (uint)(array.Length - start))
            {
                System.ThrowHelper.ThrowArgumentOutOfRangeException();
            }
            _object = array;
            _index = start;
            _length = length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ReadOnlyMemory(object obj, int start, int length)
        {
            _object = obj;
            _index = start;
            _length = length;
        }

        public static implicit operator ReadOnlyMemory<T>(T[] array)
        {
            return new ReadOnlyMemory<T>(array);
        }

        public static implicit operator ReadOnlyMemory<T>(ArraySegment<T> segment)
        {
            return new ReadOnlyMemory<T>(segment.Array, segment.Offset, segment.Count);
        }

        public override string ToString()
        {
            if (typeof(T) == typeof(char))
            {
                if (!(_object is string text))
                {
                    return Span.ToString();
                }
                return text.Substring(_index, _length & 0x7FFFFFFF);
            }
            return $"System.ReadOnlyMemory<{typeof(T).Name}>[{_length & 0x7FFFFFFF}]";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory<T> Slice(int start)
        {
            int length = _length;
            int num = length & 0x7FFFFFFF;
            if ((uint)start > (uint)num)
            {
                System.ThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.start);
            }
            return new ReadOnlyMemory<T>(_object, _index + start, length - start);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory<T> Slice(int start, int length)
        {
            int length2 = _length;
            int num = _length & 0x7FFFFFFF;
            if ((uint)start > (uint)num || (uint)length > (uint)(num - start))
            {
                System.ThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.start);
            }
            return new ReadOnlyMemory<T>(_object, _index + start, length | (length2 & int.MinValue));
        }

        public void CopyTo(Memory<T> destination)
        {
            Span.CopyTo(destination.Span);
        }

        public bool TryCopyTo(Memory<T> destination)
        {
            return Span.TryCopyTo(destination.Span);
        }

        public unsafe MemoryHandle Pin()
        {
            if (_index < 0)
            {
                return ((MemoryManager<T>)_object).Pin(_index & 0x7FFFFFFF);
            }
            if (typeof(T) == typeof(char) && _object is string value)
            {
                GCHandle handle = GCHandle.Alloc(value, GCHandleType.Pinned);
                void* pointer = Unsafe.Add<T>((void*)handle.AddrOfPinnedObject(), _index);
                return new MemoryHandle(pointer, handle);
            }
            if (_object is T[] array)
            {
                if (_length < 0)
                {
                    void* pointer2 = Unsafe.Add<T>(Unsafe.AsPointer(ref MemoryMarshal.GetReference<T>(array)), _index);
                    return new MemoryHandle(pointer2);
                }
                GCHandle handle2 = GCHandle.Alloc(array, GCHandleType.Pinned);
                void* pointer3 = Unsafe.Add<T>((void*)handle2.AddrOfPinnedObject(), _index);
                return new MemoryHandle(pointer3, handle2);
            }
            return default(MemoryHandle);
        }

        public T[] ToArray()
        {
            return Span.ToArray();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
        {
            if (obj is ReadOnlyMemory<T> other)
            {
                return Equals(other);
            }
            if (obj is Memory<T> memory)
            {
                return Equals(memory);
            }
            return false;
        }

        public bool Equals(ReadOnlyMemory<T> other)
        {
            if (_object == other._object && _index == other._index)
            {
                return _length == other._length;
            }
            return false;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            if (_object == null)
            {
                return 0;
            }
            return CombineHashCodes(_object.GetHashCode(), _index.GetHashCode(), _length.GetHashCode());
        }

        private static int CombineHashCodes(int left, int right)
        {
            return ((left << 5) + left) ^ right;
        }

        private static int CombineHashCodes(int h1, int h2, int h3)
        {
            return CombineHashCodes(CombineHashCodes(h1, h2), h3);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal object GetObjectStartLength(out int start, out int length)
        {
            start = _index;
            length = _length;
            return _object;
        }
    }

    /// <summary>
    /// Provides a type-safe and memory-safe read-only representation of a contiguous region of arbitrary memory.
    /// </summary>
    /// <typeparam name="T">The type of items in the <see cref="ReadOnlySpan{T}"/>.</typeparam>
    /// <remarks>
    /// <see cref="ReadOnlySpan{T}"/> is a <see langword="ref struct"/> that is allocated on the stack and can 
    /// never escape to the managed heap. Ref struct types have a number of restrictions to ensure that they 
    /// cannot be promoted to the managed heap, including that they can't be boxed, captured in lambda expressions, 
    /// assigned to variables of type <see cref="System.Object"/>, assigned to <see langword="dynamic"/> variables, 
    /// and they cannot implement any interface type. <br /> <br />
    /// A <see cref="ReadOnlySpan{T}"/> instance is often used to reference the elements of an array or a portion 
    /// of an array. Unlike an array, however, a <see cref="ReadOnlySpan{T}"/> instance can point to managed memory, 
    /// native memory, or memory managed on the stack.
    /// </remarks>
    [DebuggerTypeProxy(typeof(SpanDebugView<>))]
    [DebuggerDisplay("{ToString(),raw}")]
    [DebuggerTypeProxy(typeof(SpanDebugView<>))]
    [DebuggerDisplay("{ToString(),raw}")]
    public readonly ref struct ReadOnlySpan<T>
    {
        /// <summary>
        /// Provides an enumerator for the elements of a <see cref="ReadOnlySpan{T}"/>.
        /// </summary>
        public ref struct Enumerator
        {
            private readonly ReadOnlySpan<T> _span;

            private int _index;

            /// <summary>
            /// Gets a reference to the item at the current position of the enumerator.
            /// </summary>
            public ref readonly T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    return ref _span[_index];
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(ReadOnlySpan<T> span)
            {
                _span = span;
                _index = -1;
            }

            /// <summary>
            /// Advances the enumerator to the next item of the <see cref="ReadOnlySpan{T}"/>.
            /// </summary>
            /// <returns><see langword="true"/> if the enumerator successfully 
            /// advanced to the next item; <see langword="false"/> if the end of the 
            /// span has been passed.</returns>
            /// <remarks>
            /// After an enumerator is created, it is positioned before the first element 
            /// in the span, and the first call to <see cref="MoveNext()"/> advances the 
            /// enumerator to the first item in the span. <br />
            /// <br /> 
            /// If <see cref="MoveNext()"/> passes the end of the span, 
            /// <see cref="MoveNext()"/> returns <see langword="false"/>. When the 
            /// enumerator is at this state, subsequent calls to <see cref="MoveNext()"/> 
            /// also return <see langword="false"/>.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                int num = _index + 1;
                if (num < _span.Length)
                {
                    _index = num;
                    return true;
                }
                return false;
            }
        }

        private readonly Pinnable<T> _pinnable;

        private readonly IntPtr _byteOffset;

        private readonly int _length;

        /// <summary>
        /// Returns the length of the current read-only span.
        /// </summary>
        public int Length => _length;

        /// <summary>
        /// Returns a value that indicates whether the current <see cref="ReadOnlySpan{T}"/> is empty.
        /// </summary>
        public bool IsEmpty => _length == 0;

        /// <summary>
        /// Returns an empty <see cref="ReadOnlySpan{T}"/> object.
        /// </summary>
        public static ReadOnlySpan<T> Empty => default(ReadOnlySpan<T>);

        /// <summary>
        /// Gets the element at the specified zero-based index.
        /// </summary>
        /// <param name="index">The zero-based index of the element.</param>
        /// <returns>The element at the specified index.</returns>
        /// <exception cref="System.IndexOutOfRangeException"><paramref name="index"/> is less than zero or greater than or equal to <see cref="Length"/>.</exception>
        public unsafe ref readonly T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if ((uint)index >= (uint)_length)
                {
                    System.ThrowHelper.ThrowIndexOutOfRangeException();
                }
                if (_pinnable == null)
                {
                    return ref Unsafe.Add(ref Unsafe.AsRef<T>(_byteOffset.ToPointer()), index);
                }
                return ref Unsafe.Add(ref Unsafe.AddByteOffset(ref _pinnable.Data, _byteOffset), index);
            }
        }

        internal Pinnable<T> Pinnable => _pinnable;

        internal IntPtr ByteOffset => _byteOffset;

        /// <summary>
        /// Returns a value that indicates whether two <see cref="ReadOnlySpan{T}"/> objects are not equal.
        /// </summary>
        /// <param name="left">The first read-only span to compare.</param>
        /// <param name="right">The second read-only span to compare.</param>
        /// <returns><see langword="true"/> if the two <see cref="ReadOnlySpan{T}"/> objects are not equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(ReadOnlySpan<T> left, ReadOnlySpan<T> right) { return !(left == right); }

#pragma warning disable CS0809
        /// <inheritdoc />
        [Obsolete("Equals() on ReadOnlySpan will always throw an exception. Use == instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj) { throw new NotSupportedException(MDCFR.Properties.Resources.NotSupported_CannotCallEqualsOnSpan); }

        /// <inheritdoc />
        [Obsolete("GetHashCode() on ReadOnlySpan will always throw an exception.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() { throw new NotSupportedException(MDCFR.Properties.Resources.NotSupported_CannotCallGetHashCodeOnSpan); }
#pragma warning restore CS0809

        /// <summary>
        /// Defines an implicit conversion of an array to a <see cref="ReadOnlySpan{T}"/>.
        /// </summary>
        /// <param name="array">The array to convert to a <see cref="ReadOnlySpan{T}"/>.</param>
        public static implicit operator ReadOnlySpan<T>(T[] array) { return new ReadOnlySpan<T>(array); }

        /// <summary>
        /// Defines an implicit conversion of an <see cref="ArraySegment{T}"/> to a <see cref="ReadOnlySpan{T}"/>.
        /// </summary>
        /// <param name="segment">The array segment to be converted to a <see cref="ReadOnlySpan{T}"/>.</param>
        public static implicit operator ReadOnlySpan<T>(ArraySegment<T> segment) { return new ReadOnlySpan<T>(segment.Array, segment.Offset, segment.Count); }

        /// <summary>
        /// Returns an enumerator for this <see cref="Span{T}"/>.
        /// </summary>
        /// <returns>An enumerator for this span.</returns>
        /// <remarks>
        /// Instead of calling the <see cref="GetEnumerator"/> 
        /// method directly, you can use the C# <see langword="foreach"/> 
        /// statement and the Visual Basic <c>For Each...Next</c> 
        /// construct to enumerate a <see cref="Span{T}"/>.
        /// </remarks>
        public Enumerator GetEnumerator() { return new Enumerator(this); }

        /// <summary>
        /// Creates a new <see cref="ReadOnlySpan{T}"/> object over the entirety of a specified array.
        /// </summary>
        /// <param name="array">The array from which to create the <see cref="ReadOnlySpan{T}"/> object.</param>
        /// <exception cref="System.ArrayTypeMismatchException">
        /// <typeparamref name="T"/> is a reference type, and <paramref name="array"/> is not an array 
        /// of type <typeparamref name="T"/>.
        /// </exception>
        /// <remarks>
        /// If <paramref name="array"/> is <see langword="null"/>, this constructor returns a <see langword="null"/> <see cref="ReadOnlySpan{T}"/>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan(T[] array)
        {
            if (array == null)
            {
                this = default(ReadOnlySpan<T>);
                return;
            }
            _length = array.Length;
            _pinnable = Unsafe.As<Pinnable<T>>(array);
            _byteOffset = SpanHelpers.PerTypeValues<T>.ArrayAdjustment;
        }

        /// <summary>
        /// Creates a new <see cref="ReadOnlySpan{T}"/> object that includes a specified number of elements of an array starting at a specified index.
        /// </summary>
        /// <param name="array">The source array.</param>
        /// <param name="start">The index of the first element to include in the new <see cref="ReadOnlySpan{T}"/>.</param>
        /// <param name="length">The number of elements to include in the new <see cref="ReadOnlySpan{T}"/>.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// <paramref name="array"/> is <see langword="null"/>, but <paramref name="start"/> or <paramref name="length"/> is non-zero.<br /> <br />
        /// -or- <br /> <br />
        /// <paramref name="start"/> is outside the bounds of the array.
        /// -or- <br /> <br />
        /// <paramref name="start"/> and <paramref name="length"/> exceeds the number of elements in the array.
        /// </exception>
        /// <exception cref="System.ArrayTypeMismatchException">
        /// <typeparamref name="T"/> is a reference type, and <paramref name="array"/> is not an array 
        /// of type <typeparamref name="T"/>.
        /// </exception>
        /// <remarks>This constructor returns <see langword="default"/> when <paramref name="array"/> is <see langword="null"/>.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan(T[] array, int start, int length)
        {
            if (array == null)
            {
                if (start != 0 || length != 0)
                {
                    System.ThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.start);
                }
                this = default(ReadOnlySpan<T>);
                return;
            }
            if ((uint)start > (uint)array.Length || (uint)length > (uint)(array.Length - start))
            {
                System.ThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.start);
            }
            _length = length;
            _pinnable = Unsafe.As<Pinnable<T>>(array);
            _byteOffset = SpanHelpers.PerTypeValues<T>.ArrayAdjustment.Add<T>(start);
        }

        /// <summary>
        /// Creates a new <see cref="ReadOnlySpan{T}"/> object from a specified 
        /// number of <typeparamref name="T"/> elements starting at a specified memory address.
        /// </summary>
        /// <param name="pointer">A pointer to the starting address of a specified number of 
        /// <typeparamref name="T"/> elements in memory.</param>
        /// <param name="length">The number of <typeparamref name="T"/> elements to be included 
        /// in the <see cref="ReadOnlySpan{T}"/>.</param>
        /// <exception cref="System.ArgumentException">
        /// <typeparamref name="T"/> is a reference type or contains pointers and therefore cannot be stored in unmanaged memory.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="length"/> is negative.</exception>
        /// <remarks>This constructor should be used with care, since it creates arbitrarily typed <typeparamref name="T"/>'s from a 
        /// <see cref="System.Void"/>*-typed block of memory, and length is not checked. However, if the constructor succeeds in 
        /// creating a new <see cref="ReadOnlySpan{T}"/> object, all subsequent uses are correct.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CLSCompliant(false)]
        public unsafe ReadOnlySpan(void* pointer, int length)
        {
            if (SpanHelpers.IsReferenceOrContainsReferences<T>())
            {
                System.ThrowHelper.ThrowArgumentException_InvalidTypeWithPointersNotSupported(typeof(T));
            }
            if (length < 0)
            {
                System.ThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.start);
            }
            _length = length;
            _pinnable = null;
            _byteOffset = new IntPtr(pointer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ReadOnlySpan(Pinnable<T> pinnable, IntPtr byteOffset, int length)
        {
            _length = length;
            _pinnable = pinnable;
            _byteOffset = byteOffset;
        }

        /// <summary>
        /// Returns a reference to an object of type <typeparamref name="T"/> that can be used for pinning. <br /> <br />
        /// This method is intended to support .NET compilers and is not intended to be called by user code.
        /// </summary>
        /// <returns>A reference to the element of the span at index 0, or <see langword="null"/> if the span is empty.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public unsafe ref readonly T GetPinnableReference()
        {
            if (_length != 0)
            {
                if (_pinnable == null)
                {
                    return ref Unsafe.AsRef<T>(_byteOffset.ToPointer());
                }
                return ref Unsafe.AddByteOffset(ref _pinnable.Data, _byteOffset);
            }
            return ref Unsafe.AsRef<T>(null);
        }

        /// <summary>
        /// Copies the contents of this <see cref="ReadOnlySpan{T}"/> into a destination <see cref="Span{T}"/>.
        /// </summary>
        /// <param name="destination">The destination <see cref="Span{T}"/> object.</param>
        /// <exception cref="System.ArgumentException"><paramref name="destination"/> is shorter 
        /// than the source <see cref="ReadOnlySpan{T}"/>.</exception>
        /// <remarks>This method copies all of source to <paramref name="destination"/> even if 
        /// source and <paramref name="destination"/> overlap.</remarks>
        public void CopyTo(Span<T> destination) { if (TryCopyTo(destination) == false) { System.ThrowHelper.ThrowArgumentException_DestinationTooShort(); } }

        /// <summary>
        /// Attempts to copy the current <see cref="ReadOnlySpan{T}"/> to a destination 
        /// <see cref="Span{T}"/> and returns a value that indicates whether 
        /// the copy operation succeeded.
        /// </summary>
        /// <param name="destination">The target of the copy operation.</param>
        /// <returns><see langword="true"/> if the copy operation succeeded; otherwise, <see langword="false"/>.</returns>
        public bool TryCopyTo(Span<T> destination)
        {
            int length = _length;
            int length2 = destination.Length;
            if (length == 0)
            {
                return true;
            }
            if ((uint)length > (uint)length2)
            {
                return false;
            }
            ref T src = ref DangerousGetPinnableReference();
            SpanHelpers.CopyTo(ref destination.DangerousGetPinnableReference(), length2, ref src, length);
            return true;
        }

        /// <summary>
        /// Returns a value that indicates whether two <see cref="ReadOnlySpan{T}"/> objects are equal.
        /// </summary>
        /// <param name="left">The first span to compare.</param>
        /// <param name="right">The second span to compare.</param>
        /// <returns><see langword="true"/> if the two <see cref="ReadOnlySpan{T}"/> objects are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(ReadOnlySpan<T> left, ReadOnlySpan<T> right)
        {
            if (left._length == right._length)
            {
                return Unsafe.AreSame(ref left.DangerousGetPinnableReference(), ref right.DangerousGetPinnableReference());
            }
            return false;
        }

        /// <summary>
        /// Returns the string representation of this <see cref="ReadOnlySpan{T}"/> object.
        /// </summary>
        /// <returns>The string representation of this <see cref="ReadOnlySpan{T}"/> object.</returns>
        /// <remarks>
        /// For a ReadOnlySpan&lt;<see cref="System.Char"/>&gt;, the <see cref="ToString"/> method returns a 
        /// <see cref="System.String"/> that contains the characters pointed to by the 
        /// <see cref="ReadOnlySpan{T}"/>. Otherwise, it returns a <see cref="System.String"/> 
        /// with the name of the type and the number of elements that the <see cref="ReadOnlySpan{T}"/> contains.
        /// </remarks>
        public unsafe override string ToString()
        {
            if (typeof(T) == typeof(char))
            {
                if (_byteOffset == MemoryExtensions.StringAdjustment)
                {
                    object obj = Unsafe.As<object>(_pinnable);
                    if (obj is string text && _length == text.Length) { return text; }
                }
                fixed (char* value = &Unsafe.As<T, char>(ref DangerousGetPinnableReference()))
                {
                    return new string(value, 0, _length);
                }
            }
            return $"System.ReadOnlySpan<{typeof(T).Name}>[{_length}]";
        }

        /// <summary>
        /// Forms a slice out of the current span that begins at a specified index.
        /// </summary>
        /// <param name="start">The index at which to begin the slice.</param>
        /// <returns>A span that consists of all elements of the current span from <paramref name="start"/> to the end of the span.</returns>
        /// <exception cref="System.ArgumentException"><paramref name="start"/> is less than zero or greater than <see cref="Length"/>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> Slice(int start)
        {
            if ((uint)start > (uint)_length)
            {
                System.ThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.start);
            }
            IntPtr byteOffset = _byteOffset.Add<T>(start);
            int length = _length - start;
            return new ReadOnlySpan<T>(_pinnable, byteOffset, length);
        }

        /// <summary>
        /// Forms a slice out of the current span starting at a specified index for a specified length.
        /// </summary>
        /// <param name="start">The index at which to begin this slice.</param>
        /// <param name="length">The desired length for the slice.</param>
        /// <returns>A span that consists of <paramref name="length"/> elements
        /// from the current span starting at <paramref name="start"/>.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// <paramref name="start"/> or 
        /// <c><paramref name="start"/> + <paramref name="length"/></c> 
        /// is less than zero or greater than <see cref="Length"/>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> Slice(int start, int length)
        {
            if ((uint)start > (uint)_length || (uint)length > (uint)(_length - start))
            {
                System.ThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.start);
            }
            IntPtr byteOffset = _byteOffset.Add<T>(start);
            return new ReadOnlySpan<T>(_pinnable, byteOffset, length);
        }

        /// <summary>
        /// Copies the contents of this span into a new array.
        /// </summary>
        /// <returns>An array containing the data in the current span.</returns>
        /// <remarks>
        /// This method performs a heap allocation and therefore should be avoided if possible. 
        /// Heap allocations are expected in APIs that work with arrays. 
        /// Using such APIs is unavoidable if an alternative API overload that takes a 
        /// <see cref="ReadOnlySpan{T}"/> does not exist.
        /// </remarks>
        public T[] ToArray()
        {
            if (_length == 0)
            {
                return SpanHelpers.PerTypeValues<T>.EmptyArray;
            }
            T[] array = new T[_length];
            CopyTo(array);
            return array;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal unsafe ref T DangerousGetPinnableReference()
        {
            if (_pinnable == null)
            {
                return ref Unsafe.AsRef<T>(_byteOffset.ToPointer());
            }
            return ref Unsafe.AddByteOffset(ref _pinnable.Data, _byteOffset);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal sealed class Pinnable<T> { public T Data; }



}


