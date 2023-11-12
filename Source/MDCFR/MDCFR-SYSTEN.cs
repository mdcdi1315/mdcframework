// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text;
using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;


namespace System.Numerics.Tensors
{
    /// <summary>
	/// Various methods for creating and manipulating Tensor&lt;T&gt;
	/// </summary>
	public static class Tensor
    {
        /// <summary>
        /// Creates an identity tensor of the specified size.  An identity tensor is a two dimensional tensor with 1s in the diagonal.
        /// </summary>
        /// <typeparam name="T">type contained within the Tensor.  Typically a value type such as int, double, float, etc.</typeparam>
        /// <param name="size">Width and height of the identity tensor to create.</param>
        /// <returns>a <paramref name="size" /> by <paramref name="size" /> with 1s along the diagonal and zeros elsewhere.</returns>
        public static Tensor<T> CreateIdentity<T>(int size)
        {
            return CreateIdentity(size, columMajor: false, Tensor<T>.One);
        }

        /// <summary>
        /// Creates an identity tensor of the specified size and layout (row vs column major).  An identity tensor is a two dimensional tensor with 1s in the diagonal.
        /// </summary>
        /// <typeparam name="T">type contained within the Tensor.  Typically a value type such as int, double, float, etc.</typeparam>
        /// <param name="size">Width and height of the identity tensor to create.</param>
        /// <param name="columMajor">&gt;False to indicate that the first dimension is most minor (closest) and the last dimension is most major (farthest): row-major.  True to indicate that the last dimension is most minor (closest together) and the first dimension is most major (farthest apart): column-major.</param>
        /// <returns>a <paramref name="size" /> by <paramref name="size" /> with 1s along the diagonal and zeros elsewhere.</returns>
        public static Tensor<T> CreateIdentity<T>(int size, bool columMajor)
        {
            return CreateIdentity(size, columMajor, Tensor<T>.One);
        }

        /// <summary>
        /// Creates an identity tensor of the specified size and layout (row vs column major) using the specified one value.  An identity tensor is a two dimensional tensor with 1s in the diagonal.  This may be used in case T is a type that doesn't have a known 1 value.
        /// </summary>
        /// <typeparam name="T">type contained within the Tensor.  Typically a value type such as int, double, float, etc.</typeparam>
        /// <param name="size">Width and height of the identity tensor to create.</param>
        /// <param name="columMajor">&gt;False to indicate that the first dimension is most minor (closest) and the last dimension is most major (farthest): row-major.  True to indicate that the last dimension is most minor (closest together) and the first dimension is most major (farthest apart): column-major.</param>
        /// <param name="oneValue">Value of <typeparamref name="T" /> that is used along the diagonal.</param>
        /// <returns>a <paramref name="size" /> by <paramref name="size" /> with 1s along the diagonal and zeros elsewhere.</returns>
        public static Tensor<T> CreateIdentity<T>(int size, bool columMajor, T oneValue)
        {
            Span<int> span = stackalloc int[2];
            span[0] = (span[1] = size);
            DenseTensor<T> denseTensor = new DenseTensor<T>(span, columMajor);
            for (int i = 0; i < size; i++)
            {
                denseTensor.SetValue(i * size + i, oneValue);
            }
            return denseTensor;
        }

        /// <summary>
        /// Creates a n+1-rank tensor using the specified n-rank diagonal.  Values not on the diagonal will be filled with zeros.
        /// </summary>
        /// <typeparam name="T">type contained within the Tensor.  Typically a value type such as int, double, float, etc.</typeparam>
        /// <param name="diagonal">Tensor representing the diagonal to build the new tensor from.</param>
        /// <returns>A new tensor of the same layout and order as <paramref name="diagonal" /> of one higher rank, with the values of <paramref name="diagonal" /> along the diagonal and zeros elsewhere.</returns>
        public static Tensor<T> CreateFromDiagonal<T>(Tensor<T> diagonal)
        {
            return CreateFromDiagonal(diagonal, 0);
        }

        /// <summary>
        /// Creates a n+1-dimension tensor using the specified n-dimension diagonal at the specified offset from the center.  Values not on the diagonal will be filled with zeros.
        /// </summary>
        /// <typeparam name="T">type contained within the Tensor.  Typically a value type such as int, double, float, etc.</typeparam>
        /// <param name="diagonal">Tensor representing the diagonal to build the new tensor from.</param>
        /// <param name="offset">Offset of diagonal to set in returned tensor.  0 for the main diagonal, less than zero for diagonals below, greater than zero from diagonals above.</param>
        /// <returns>A new tensor of the same layout and order as <paramref name="diagonal" /> of one higher rank, with the values of <paramref name="diagonal" /> along the specified diagonal and zeros elsewhere.</returns>
        public static Tensor<T> CreateFromDiagonal<T>(Tensor<T> diagonal, int offset)
        {
            if (diagonal.Rank < 1)
            {
                throw new ArgumentException(System.SR.Format(MDCFR.Properties.Resources.MustHaveAtLeastOneDimension, "diagonal"), "diagonal");
            }
            int num = diagonal.dimensions[0];
            int num2 = diagonal.dimensions.Length + 1;
            Span<int> span = ((num2 >= 16) ? ((Span<int>)new int[num2]) : stackalloc int[num2]);
            Span<int> span2 = span;
            int num3 = num + Math.Abs(offset);
            span2[0] = (span2[1] = num3);
            for (int i = 1; i < diagonal.dimensions.Length; i++)
            {
                span2[i + 1] = diagonal.dimensions[i];
            }
            Tensor<T> tensor = diagonal.CloneEmpty(span2);
            long num4 = diagonal.Length / num;
            int num5 = ((!diagonal.IsReversedStride || diagonal.Rank <= 1) ? 1 : diagonal.strides[1]);
            int num6 = ((!tensor.IsReversedStride || tensor.Rank <= 2) ? 1 : tensor.strides[2]);
            for (int j = 0; j < num; j++)
            {
                int num7 = ((offset < 0) ? (j - offset) : j);
                int num8 = ((offset > 0) ? (j + offset) : j);
                int num9 = num7 * tensor.strides[0] + num8 * tensor.strides[1];
                int num10 = j * diagonal.strides[0];
                for (int k = 0; k < num4; k++)
                {
                    tensor.SetValue(num9 + k * num6, diagonal.GetValue(num10 + k * num5));
                }
            }
            return tensor;
        }
    }
    
    /// <summary>
    /// Represents a multi-dimensional collection of objects of type T that can be accessed by indices.
    /// </summary>
    /// <typeparam name="T">Type contained within the Tensor.  Typically a value type such as int, double, float, etc.</typeparam>
    #nullable enable
    [DebuggerDisplay("{GetArrayString(false)}")]
    public abstract class Tensor<T> : IList, ICollection, IEnumerable, IList<T>, ICollection<T>, IEnumerable<T>, IReadOnlyList<T>, IReadOnlyCollection<T>, IStructuralComparable, IStructuralEquatable
    {
        /// <summary>
        /// The type that implements enumerators for <see cref="T:System.Numerics.Tensors.Tensor`1" /> instances.
        /// </summary>
        public struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator
        {
            private readonly Tensor<T> _tensor;

            private int _index;

            public T Current { get; private set; }

            object? IEnumerator.Current => Current;

            internal Enumerator(Tensor<T> tensor)
            {
                _tensor = tensor;
                _index = 0;
                Current = default(T);
            }

            public bool MoveNext()
            {
                if (_index < _tensor.Length)
                {
                    Current = _tensor.GetValue(_index);
                    _index++;
                    return true;
                }
                Current = default(T);
                return false;
            }

            /// <summary>
            /// Resets the enumerator to the beginning.
            /// </summary>
            public void Reset()
            {
                _index = 0;
                Current = default(T);
            }

            /// <summary>
            /// Disposes the enumerator.
            /// </summary>
            public void Dispose()
            {
            }
        }

        internal readonly int[] dimensions;

        internal readonly int[] strides;

        private readonly bool isReversedStride;

        private readonly long length;

        internal static T Zero
        {
            get
            {
                if (typeof(T) == typeof(bool))
                {
                    return (T)(object)false;
                }
                if (typeof(T) == typeof(byte))
                {
                    return (T)(object)(byte)0;
                }
                if (typeof(T) == typeof(char))
                {
                    return (T)(object)'\0';
                }
                if (typeof(T) == typeof(decimal))
                {
                    return (T)(object)0m;
                }
                if (typeof(T) == typeof(double))
                {
                    return (T)(object)0.0;
                }
                if (typeof(T) == typeof(float))
                {
                    return (T)(object)0f;
                }
                if (typeof(T) == typeof(int))
                {
                    return (T)(object)0;
                }
                if (typeof(T) == typeof(long))
                {
                    return (T)(object)0L;
                }
                if (typeof(T) == typeof(sbyte))
                {
                    return (T)(object)(sbyte)0;
                }
                if (typeof(T) == typeof(short))
                {
                    return (T)(object)(short)0;
                }
                if (typeof(T) == typeof(uint))
                {
                    return (T)(object)0u;
                }
                if (typeof(T) == typeof(ulong))
                {
                    return (T)(object)0uL;
                }
                if (typeof(T) == typeof(ushort))
                {
                    return (T)(object)(ushort)0;
                }
                throw new NotSupportedException();
            }
        }

        internal static T One
        {
            get
            {
                if (typeof(T) == typeof(bool))
                {
                    return (T)(object)true;
                }
                if (typeof(T) == typeof(byte))
                {
                    return (T)(object)(byte)1;
                }
                if (typeof(T) == typeof(char))
                {
                    return (T)(object)'\u0001';
                }
                if (typeof(T) == typeof(decimal))
                {
                    return (T)(object)1m;
                }
                if (typeof(T) == typeof(double))
                {
                    return (T)(object)1.0;
                }
                if (typeof(T) == typeof(float))
                {
                    return (T)(object)1f;
                }
                if (typeof(T) == typeof(int))
                {
                    return (T)(object)1;
                }
                if (typeof(T) == typeof(long))
                {
                    return (T)(object)1L;
                }
                if (typeof(T) == typeof(sbyte))
                {
                    return (T)(object)(sbyte)1;
                }
                if (typeof(T) == typeof(short))
                {
                    return (T)(object)(short)1;
                }
                if (typeof(T) == typeof(uint))
                {
                    return (T)(object)1u;
                }
                if (typeof(T) == typeof(ulong))
                {
                    return (T)(object)1uL;
                }
                if (typeof(T) == typeof(ushort))
                {
                    return (T)(object)(ushort)1;
                }
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Total length of the Tensor.
        /// </summary>
        public long Length => length;

        /// <summary>
        /// Rank of the tensor: number of dimensions.
        /// </summary>
        public int Rank => dimensions.Length;

        /// <summary>
        /// True if strides are reversed (AKA Column-major)
        /// </summary>
        public bool IsReversedStride => isReversedStride;

        /// <summary>
        /// Returns a readonly view of the dimensions of this tensor.
        /// </summary>
        public ReadOnlySpan<int> Dimensions => dimensions;

        /// <summary>
        /// Returns a readonly view of the strides of this tensor.
        /// </summary>
        public ReadOnlySpan<int> Strides => strides;

        /// <summary>
        /// Obtains the value at the specified indices
        /// </summary>
        /// <param name="indices">A one-dimensional array of integers that represent the indices specifying the position of the element to get.</param>
        /// <returns>The value at the specified position in this Tensor.</returns>
        public virtual T this[params int[] indices]
        {
            get
            {
                if (indices == null)
                {
                    throw new ArgumentNullException("indices");
                }
                ReadOnlySpan<int> indices2 = new ReadOnlySpan<int>(indices);
                return this[indices2];
            }
            set
            {
                if (indices == null)
                {
                    throw new ArgumentNullException("indices");
                }
                ReadOnlySpan<int> indices2 = new ReadOnlySpan<int>(indices);
                this[indices2] = value;
            }
        }

        /// <summary>
        /// Obtains the value at the specified indices
        /// </summary>
        /// <param name="indices">A span integers that represent the indices specifying the position of the element to get.</param>
        /// <returns>The value at the specified position in this Tensor.</returns>
        public virtual T this[ReadOnlySpan<int> indices]
        {
            get
            {
                return GetValue(ArrayUtilities.GetIndex(strides, indices));
            }
            set
            {
                SetValue(ArrayUtilities.GetIndex(strides, indices), value);
            }
        }

        int ICollection.Count => (int)Length;

        bool ICollection.IsSynchronized => false;

        object ICollection.SyncRoot => this;

        object? IList.this[int index]
        {
            get
            {
                return GetValue(index);
            }
            set
            {
                try
                {
                    SetValue(index, (T)value);
                }
                catch (InvalidCastException)
                {
                    throw new ArgumentException(System.SR.Format(MDCFR.Properties.Resources.ValueIsNotOfType, value, typeof(T)));
                }
            }
        }

        public bool IsFixedSize => true;

        public bool IsReadOnly => false;

        int ICollection<T>.Count => (int)Length;

        int IReadOnlyCollection<T>.Count => (int)Length;

        T IList<T>.this[int index]
        {
            get
            {
                return GetValue(index);
            }
            set
            {
                SetValue(index, value);
            }
        }

        T IReadOnlyList<T>.this[int index] => GetValue(index);

        /// <summary>
        /// Initialize a 1-dimensional tensor of the specified length
        /// </summary>
        /// <param name="length">Size of the 1-dimensional tensor</param>
        protected Tensor(int length)
        {
            dimensions = new int[1] { length };
            strides = new int[1] { 1 };
            isReversedStride = false;
            this.length = length;
        }

        /// <summary>
        /// Initialize an n-dimensional tensor with the specified dimensions and layout.  ReverseStride=true gives a stride of 1-element witdth to the first dimension (0).  ReverseStride=false gives a stride of 1-element width to the last dimension (n-1).
        /// </summary>
        /// <param name="dimensions">An span of integers that represent the size of each dimension of the Tensor to create.</param>
        /// <param name="reverseStride">False (default) to indicate that the first dimension is most major (farthest apart) and the last dimension is most minor (closest together): akin to row-major in a rank-2 tensor.  True to indicate that the last dimension is most major (farthest apart) and the first dimension is most minor (closest together): akin to column-major in a rank-2 tensor.</param>
        protected Tensor(ReadOnlySpan<int> dimensions, bool reverseStride)
        {
            if (dimensions.Length == 0)
            {
                throw new ArgumentException(MDCFR.Properties.Resources.DimensionsMustContainElements, "dimensions");
            }
            this.dimensions = new int[dimensions.Length];
            long num = 1L;
            for (int i = 0; i < dimensions.Length; i++)
            {
                if (dimensions[i] < 1)
                {
                    throw new ArgumentOutOfRangeException("dimensions", MDCFR.Properties.Resources.DimensionsMustBePositiveAndNonZero);
                }
                this.dimensions[i] = dimensions[i];
                num *= dimensions[i];
            }
            strides = ArrayUtilities.GetStrides(dimensions, reverseStride);
            isReversedStride = reverseStride;
            length = num;
        }

        /// <summary>
        /// Initializes tensor with same dimensions as array, content of array is ignored.  ReverseStride=true gives a stride of 1-element witdth to the first dimension (0).  ReverseStride=false gives a stride of 1-element width to the last dimension (n-1).
        /// </summary>
        /// <param name="fromArray">Array from which to derive dimensions.</param>
        /// <param name="reverseStride">False (default) to indicate that the first dimension is most major (farthest apart) and the last dimension is most minor (closest together): akin to row-major in a rank-2 tensor.  True to indicate that the last dimension is most major (farthest apart) and the first dimension is most minor (closest together): akin to column-major in a rank-2 tensor.</param>
        protected Tensor(Array fromArray, bool reverseStride)
        {
            if (fromArray == null)
            {
                throw new ArgumentNullException("fromArray");
            }
            if (fromArray.Rank == 0)
            {
                throw new ArgumentException(MDCFR.Properties.Resources.ArrayMustContainElements, "fromArray");
            }
            dimensions = new int[fromArray.Rank];
            long num = 1L;
            for (int i = 0; i < dimensions.Length; i++)
            {
                dimensions[i] = fromArray.GetLength(i);
                num *= dimensions[i];
            }
            strides = ArrayUtilities.GetStrides(dimensions, reverseStride);
            isReversedStride = reverseStride;
            length = num;
        }

        /// <summary>
        /// Sets all elements in Tensor to <paramref name="value" />.
        /// </summary>
        /// <param name="value">Value to fill</param>
        public virtual void Fill(T value)
        {
            for (int i = 0; i < Length; i++)
            {
                SetValue(i, value);
            }
        }

        /// <summary>
        /// Creates a shallow copy of this tensor, with new backing storage.
        /// </summary>
        /// <returns>A shallow copy of this tensor.</returns>
        public abstract Tensor<T> Clone();

        /// <summary>
        /// Creates a new Tensor with the same layout and dimensions as this tensor with elements initialized to their default value.
        /// </summary>
        /// <returns>A new Tensor with the same layout and dimensions as this tensor with elements initialized to their default value.</returns>
        public virtual Tensor<T> CloneEmpty()
        {
            return CloneEmpty<T>(dimensions);
        }

        /// <summary>
        /// Creates a new Tensor with the specified dimensions and the same layout as this tensor with elements initialized to their default value.
        /// </summary>
        /// <param name="dimensions">An span of integers that represent the size of each dimension of the DenseTensor to create.</param>
        /// <returns>A new Tensor with the same layout as this tensor and specified <paramref name="dimensions" /> with elements initialized to their default value.</returns>
        public virtual Tensor<T> CloneEmpty(ReadOnlySpan<int> dimensions)
        {
            return CloneEmpty<T>(dimensions);
        }

        /// <summary>
        /// Creates a new Tensor of a different type with the same layout and size as this tensor with elements initialized to their default value.
        /// </summary>
        /// <typeparam name="TResult">Type contained within the new Tensor.  Typically a value type such as int, double, float, etc.</typeparam>
        /// <returns>A new Tensor with the same layout and dimensions as this tensor with elements of <typeparamref name="TResult" /> type initialized to their default value.</returns>
        public virtual Tensor<TResult> CloneEmpty<TResult>()
        {
            return CloneEmpty<TResult>(dimensions);
        }

        /// <summary>
        /// Creates a new Tensor of a different type with the specified dimensions and the same layout as this tensor with elements initialized to their default value.
        /// </summary>
        /// <typeparam name="TResult">Type contained within the new Tensor.  Typically a value type such as int, double, float, etc.</typeparam>
        /// <param name="dimensions">An span of integers that represent the size of each dimension of the DenseTensor to create.</param>
        /// <returns>A new Tensor with the same layout as this tensor of specified <paramref name="dimensions" /> with elements of <typeparamref name="TResult" /> type initialized to their default value.</returns>
        public abstract Tensor<TResult> CloneEmpty<TResult>(ReadOnlySpan<int> dimensions);

        /// <summary>
        /// Gets the n-1 dimension diagonal from the n dimension tensor.
        /// </summary>
        /// <returns>An n-1 dimension tensor with the values from the main diagonal of this tensor.</returns>
        public Tensor<T> GetDiagonal()
        {
            return GetDiagonal(0);
        }

        /// <summary>
        /// Gets the n-1 dimension diagonal from the n dimension tensor at the specified offset from center.
        /// </summary>
        /// <param name="offset">Offset of diagonal to set in returned tensor.  0 for the main diagonal, less than zero for diagonals below, greater than zero from diagonals above.</param>
        /// <returns>An n-1 dimension tensor with the values from the specified diagonal of this tensor.</returns>
        public Tensor<T> GetDiagonal(int offset)
        {
            if (Rank < 2)
            {
                throw new InvalidOperationException(System.SR.Format(MDCFR.Properties.Resources.CannotComputeDiagonal, "Tensor"));
            }
            int num = dimensions[0];
            int num2 = dimensions[1];
            int val = ((offset < 0) ? (num + offset) : num);
            int val2 = ((offset > 0) ? (num2 - offset) : num2);
            int num3 = Math.Min(val, val2);
            if (num3 <= 0)
            {
                throw new ArgumentException(System.SR.Format(MDCFR.Properties.Resources.CannotComputeDiagonalWithOffset, offset), "offset");
            }
            int num4 = Rank - 1;
            Span<int> span = ((num4 >= 16) ? ((Span<int>)new int[num4]) : stackalloc int[num4]);
            Span<int> span2 = span;
            span2[0] = num3;
            for (int i = 2; i < dimensions.Length; i++)
            {
                span2[i - 1] = dimensions[i];
            }
            Tensor<T> tensor = CloneEmpty(span2);
            long num5 = tensor.Length / tensor.Dimensions[0];
            int num6 = ((!tensor.IsReversedStride || tensor.Rank <= 1) ? 1 : tensor.strides[1]);
            int num7 = ((!IsReversedStride || Rank <= 2) ? 1 : strides[2]);
            for (int j = 0; j < num3; j++)
            {
                int num8 = ((offset < 0) ? (j - offset) : j);
                int num9 = ((offset > 0) ? (j + offset) : j);
                int num10 = num8 * strides[0] + num9 * strides[1];
                int num11 = j * tensor.strides[0];
                for (int k = 0; k < num5; k++)
                {
                    tensor.SetValue(num11 + k * num6, GetValue(num10 + k * num7));
                }
            }
            return tensor;
        }

        /// <summary>
        /// Gets a tensor representing the elements below and including the diagonal, with the rest of the elements zero-ed.
        /// </summary>
        /// <returns>A tensor with the values from this tensor at and below the main diagonal and zeros elsewhere.</returns>
        public Tensor<T> GetTriangle()
        {
            return GetTriangle(0, upper: false);
        }

        /// <summary>
        /// Gets a tensor representing the elements below and including the specified diagonal, with the rest of the elements zero-ed.
        /// </summary>
        /// <param name="offset">Offset of diagonal to set in returned tensor.  0 for the main diagonal, less than zero for diagonals below, greater than zero from diagonals above.</param>
        /// <returns>A tensor with the values from this tensor at and below the specified diagonal and zeros elsewhere.</returns>
        public Tensor<T> GetTriangle(int offset)
        {
            return GetTriangle(offset, upper: false);
        }

        /// <summary>
        /// Gets a tensor representing the elements above and including the diagonal, with the rest of the elements zero-ed.
        /// </summary>
        /// <returns>A tensor with the values from this tensor at and above the main diagonal and zeros elsewhere.</returns>
        public Tensor<T> GetUpperTriangle()
        {
            return GetTriangle(0, upper: true);
        }

        /// <summary>
        /// Gets a tensor representing the elements above and including the specified diagonal, with the rest of the elements zero-ed.
        /// </summary>
        /// <param name="offset">Offset of diagonal to set in returned tensor.  0 for the main diagonal, less than zero for diagonals below, greater than zero from diagonals above.</param>
        /// <returns>A tensor with the values from this tensor at and above the specified diagonal and zeros elsewhere.</returns>
        public Tensor<T> GetUpperTriangle(int offset)
        {
            return GetTriangle(offset, upper: true);
        }

        private Tensor<T> GetTriangle(int offset, bool upper)
        {
            if (Rank < 2)
            {
                throw new InvalidOperationException(System.SR.Format(MDCFR.Properties.Resources.CannotComputeTriangle, "Tensor"));
            }
            int num = dimensions[0];
            int num2 = dimensions[1];
            int num3 = Math.Max(num, num2);
            Tensor<T> tensor = CloneEmpty();
            long num4 = Length / (num * num2);
            int num5 = ((!IsReversedStride || Rank <= 2) ? 1 : strides[2]);
            for (int i = 0; i < num3; i++)
            {
                int num6 = ((offset > 0) ? (i - offset) : i);
                int num7 = ((offset > 0) ? i : (i + offset));
                if (num6 < 0)
                {
                    if (upper)
                    {
                        continue;
                    }
                    num6 = 0;
                }
                if (num7 < 0)
                {
                    if (!upper)
                    {
                        continue;
                    }
                    num7 = 0;
                }
                while (num7 < num2 && num6 < num)
                {
                    int num8 = num6 * strides[0] + num7 * tensor.strides[1];
                    for (int j = 0; j < num4; j++)
                    {
                        int index = num8 + j * num5;
                        tensor.SetValue(index, GetValue(index));
                    }
                    if (upper)
                    {
                        num7++;
                    }
                    else
                    {
                        num6++;
                    }
                }
            }
            return tensor;
        }

        /// <summary>
        /// Reshapes the current tensor to new dimensions, using the same backing storage if possible.
        /// </summary>
        /// <param name="dimensions">An span of integers that represent the size of each dimension of the Tensor to create.</param>
        /// <returns>A new tensor that reinterprets this tensor with different dimensions.</returns>
        public abstract Tensor<T> Reshape(ReadOnlySpan<int> dimensions);

        /// <summary>
        /// Gets the value at the specified index, where index is a linearized version of n-dimension indices using strides.
        /// </summary>
        /// <param name="index">An integer index computed as a dot-product of indices.</param>
        /// <returns>The value at the specified position in this Tensor.</returns>
        public abstract T GetValue(int index);

        /// <summary>
        /// Sets the value at the specified index, where index is a linearized version of n-dimension indices using strides.
        /// </summary>
        /// <param name="index">An integer index computed as a dot-product of indices.</param>
        /// <param name="value">The new value to set at the specified position in this Tensor.</param>
        public abstract void SetValue(int index, T value);

        /// <summary>
        /// Gets an enumerator that enumerates the elements of the <see cref="T:System.Numerics.Tensors.Tensor`1" />.
        /// </summary>
        /// <returns>An enumerator for the current <see cref="T:System.Numerics.Tensors.Tensor`1" />.</returns>
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        /// <summary>
        /// Performs a value comparison of the content and shape of two tensors.  Two tensors are equal if they have the same shape and same value at every set of indices.  If not equal a tensor is greater or less than another tensor based on the first non-equal element when enumerating in linear order.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static int Compare(Tensor<T> left, Tensor<T> right)
        {
            return StructuralComparisons.StructuralComparer.Compare(left, right);
        }

        /// <summary>
        /// Performs a value equality comparison of the content of two tensors. Two tensors are equal if they have the same shape and same value at every set of indices.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool Equals(Tensor<T> left, Tensor<T> right)
        {
            return StructuralComparisons.StructuralEqualityComparer.Equals(left, right);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        void ICollection.CopyTo(Array array, int index)
        {
            if (array is T[] array2)
            {
                CopyTo(array2, index);
                return;
            }
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (array.Rank != 1)
            {
                throw new ArgumentException(MDCFR.Properties.Resources.OnlySingleDimensionalArraysSupported, "array");
            }
            if (array.Length < index + Length)
            {
                throw new ArgumentException(MDCFR.Properties.Resources.NumberGreaterThenAvailableSpace, "array");
            }
            for (int i = 0; i < length; i++)
            {
                array.SetValue(GetValue(i), index + i);
            }
        }

        int IList.Add(object value)
        {
            throw new InvalidOperationException();
        }

        void IList.Clear()
        {
            Fill(default(T));
        }

        bool IList.Contains(object value)
        {
            if (IsCompatibleObject(value))
            {
                return Contains((T)value);
            }
            return false;
        }

        int IList.IndexOf(object value)
        {
            if (IsCompatibleObject(value))
            {
                return IndexOf((T)value);
            }
            return -1;
        }

        void IList.Insert(int index, object value)
        {
            throw new InvalidOperationException();
        }

        void IList.Remove(object value)
        {
            throw new InvalidOperationException();
        }

        void IList.RemoveAt(int index)
        {
            throw new InvalidOperationException();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        void ICollection<T>.Add(T item)
        {
            throw new InvalidOperationException();
        }

        void ICollection<T>.Clear()
        {
            Fill(default(T));
        }

        bool ICollection<T>.Contains(T item)
        {
            return Contains(item);
        }

        /// <summary>
        /// Determines whether an element is in the Tensor&lt;T&gt;.
        /// </summary>
        /// <param name="item">
        /// The object to locate in the Tensor&lt;T&gt;. The value can be null for reference types.
        /// </param>
        /// <returns>
        /// true if item is found in the Tensor&lt;T&gt;; otherwise, false.
        /// </returns>
        protected virtual bool Contains(T item)
        {
            if (Length != 0L)
            {
                return IndexOf(item) != -1;
            }
            return false;
        }

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Copies the elements of the Tensor&lt;T&gt; to an Array, starting at a particular Array index.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional Array that is the destination of the elements copied from Tensor&lt;T&gt;. The Array must have zero-based indexing.
        /// </param>
        /// <param name="arrayIndex">
        /// The zero-based index in array at which copying begins.
        /// </param>
        protected virtual void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (array.Length < arrayIndex + Length)
            {
                throw new ArgumentException(MDCFR.Properties.Resources.NumberGreaterThenAvailableSpace, "array");
            }
            for (int i = 0; i < length; i++)
            {
                array[arrayIndex + i] = GetValue(i);
            }
        }

        bool ICollection<T>.Remove(T item)
        {
            throw new InvalidOperationException();
        }

        int IList<T>.IndexOf(T item)
        {
            return IndexOf(item);
        }

        /// <summary>
        /// Determines the index of a specific item in the Tensor&lt;T&gt;.
        /// </summary>
        /// <param name="item">The object to locate in the Tensor&lt;T&gt;.</param>
        /// <returns>The index of item if found in the tensor; otherwise, -1.</returns>
        protected virtual int IndexOf(T item)
        {
            for (int i = 0; i < Length; i++)
            {
                if (GetValue(i).Equals(item))
                {
                    return i;
                }
            }
            return -1;
        }

        void IList<T>.Insert(int index, T item)
        {
            throw new InvalidOperationException();
        }

        void IList<T>.RemoveAt(int index)
        {
            throw new InvalidOperationException();
        }

        int IStructuralComparable.CompareTo(object other, IComparer comparer)
        {
            if (other == null)
            {
                return 1;
            }
            if (other is Tensor<T>)
            {
                return CompareTo((Tensor<T>)other, comparer);
            }
            if (other is Array other2)
            {
                return CompareTo(other2, comparer);
            }
            throw new ArgumentException(System.SR.Format(MDCFR.Properties.Resources.CannotCompare, "Tensor", other.GetType()), "other");
        }

        private int CompareTo(Tensor<T> other, IComparer comparer)
        {
            if (Rank != other.Rank)
            {
                throw new ArgumentException(System.SR.Format(MDCFR.Properties.Resources.CannotCompareWithRank, "Tensor", Rank, "other", other.Rank), "other");
            }
            for (int i = 0; i < dimensions.Length; i++)
            {
                if (dimensions[i] != other.dimensions[i])
                {
                    throw new ArgumentException(System.SR.Format(MDCFR.Properties.Resources.CannotCompareWithDifferentDimension, "Tensor", i, dimensions[i], other.dimensions[i]), "other");
                }
            }
            int num = 0;
            if (IsReversedStride == other.IsReversedStride)
            {
                for (int j = 0; j < Length; j++)
                {
                    num = comparer.Compare(GetValue(j), other.GetValue(j));
                    if (num != 0)
                    {
                        break;
                    }
                }
            }
            else
            {
                Span<int> span = ((Rank >= 16) ? ((Span<int>)new int[Rank]) : stackalloc int[Rank]);
                Span<int> span2 = span;
                for (int k = 0; k < Length; k++)
                {
                    ArrayUtilities.GetIndices(strides, IsReversedStride, k, span2);
                    num = comparer.Compare(this[span2], other[span2]);
                    if (num != 0)
                    {
                        break;
                    }
                }
            }
            return num;
        }

        private int CompareTo(Array other, IComparer comparer)
        {
            if (Rank != other.Rank)
            {
                throw new ArgumentException(System.SR.Format(MDCFR.Properties.Resources.CannotCompareWithRank, "Tensor", Rank, "Array", other.Rank), "other");
            }
            for (int i = 0; i < dimensions.Length; i++)
            {
                int num = other.GetLength(i);
                if (dimensions[i] != num)
                {
                    throw new ArgumentException(System.SR.Format(MDCFR.Properties.Resources.CannotCompareToWithDifferentDimension, "Tensor", "Array", i, dimensions[i], num), "other");
                }
            }
            int num2 = 0;
            int[] indices = new int[Rank];
            for (int j = 0; j < Length; j++)
            {
                ArrayUtilities.GetIndices(strides, IsReversedStride, j, indices);
                num2 = comparer.Compare(GetValue(j), other.GetValue(indices));
                if (num2 != 0)
                {
                    break;
                }
            }
            return num2;
        }

        bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
        {
            if (other == null)
            {
                return false;
            }
            if (other is Tensor<T>)
            {
                return Equals((Tensor<T>)other, comparer);
            }
            if (other is Array other2)
            {
                return Equals(other2, comparer);
            }
            throw new ArgumentException(System.SR.Format(MDCFR.Properties.Resources.CannotCompare, "Tensor", other.GetType()), "other");
        }

        private bool Equals(Tensor<T> other, IEqualityComparer comparer)
        {
            if (Rank != other.Rank)
            {
                throw new ArgumentException(System.SR.Format(MDCFR.Properties.Resources.CannotCompareWithRank, "Tensor", Rank, "other", other.Rank), "other");
            }
            for (int i = 0; i < dimensions.Length; i++)
            {
                if (dimensions[i] != other.dimensions[i])
                {
                    throw new ArgumentException(System.SR.Format(MDCFR.Properties.Resources.CannotCompareWithDifferentDimension, "Tensor", i, dimensions[i], other.dimensions[i]), "other");
                }
            }
            if (IsReversedStride == other.IsReversedStride)
            {
                for (int j = 0; j < Length; j++)
                {
                    if (!comparer.Equals(GetValue(j), other.GetValue(j)))
                    {
                        return false;
                    }
                }
            }
            else
            {
                Span<int> span = ((Rank >= 16) ? ((Span<int>)new int[Rank]) : stackalloc int[Rank]);
                Span<int> span2 = span;
                for (int k = 0; k < Length; k++)
                {
                    ArrayUtilities.GetIndices(strides, IsReversedStride, k, span2);
                    if (!comparer.Equals(this[span2], other[span2]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private bool Equals(Array other, IEqualityComparer comparer)
        {
            if (Rank != other.Rank)
            {
                throw new ArgumentException(System.SR.Format(MDCFR.Properties.Resources.CannotCompareWithRank, "Tensor", Rank, "Array", other.Rank), "other");
            }
            for (int i = 0; i < dimensions.Length; i++)
            {
                int num = other.GetLength(i);
                if (dimensions[i] != num)
                {
                    throw new ArgumentException(System.SR.Format(MDCFR.Properties.Resources.CannotCompareToWithDifferentDimension, "Tensor", "Array", i, dimensions[i], num), "other");
                }
            }
            int[] indices = new int[Rank];
            for (int j = 0; j < Length; j++)
            {
                ArrayUtilities.GetIndices(strides, IsReversedStride, j, indices);
                if (!comparer.Equals(GetValue(j), other.GetValue(indices)))
                {
                    return false;
                }
            }
            return true;
        }

        int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
        {
            int num = 0;
            for (int i = 0; i < Length; i++)
            {
                num ^= comparer.GetHashCode(GetValue(i));
            }
            return num;
        }

        /// <summary>
        /// Creates a copy of this tensor as a DenseTensor&lt;T&gt;.  If this tensor is already a DenseTensor&lt;T&gt; calling this method is equivalent to calling Clone().
        /// </summary>
        /// <returns></returns>
        public virtual DenseTensor<T> ToDenseTensor()
        {
            DenseTensor<T> denseTensor = new DenseTensor<T>(Dimensions, IsReversedStride);
            for (int i = 0; i < Length; i++)
            {
                denseTensor.SetValue(i, GetValue(i));
            }
            return denseTensor;
        }

        /// <summary>
        /// Creates a copy of this tensor as a SparseTensor&lt;T&gt;.  If this tensor is already a SparseTensor&lt;T&gt; calling this method is equivalent to calling Clone().
        /// </summary>
        /// <returns></returns>
        public virtual SparseTensor<T> ToSparseTensor()
        {
            SparseTensor<T> sparseTensor = new SparseTensor<T>(Dimensions, IsReversedStride);
            for (int i = 0; i < Length; i++)
            {
                sparseTensor.SetValue(i, GetValue(i));
            }
            return sparseTensor;
        }

        /// <summary>
        /// Creates a copy of this tensor as a CompressedSparseTensor&lt;T&gt;.  If this tensor is already a CompressedSparseTensor&lt;T&gt; calling this method is equivalent to calling Clone().
        /// </summary>
        /// <returns></returns>
        public virtual CompressedSparseTensor<T> ToCompressedSparseTensor()
        {
            CompressedSparseTensor<T> compressedSparseTensor = new CompressedSparseTensor<T>(Dimensions, IsReversedStride);
            for (int i = 0; i < Length; i++)
            {
                compressedSparseTensor.SetValue(i, GetValue(i));
            }
            return compressedSparseTensor;
        }

        public string GetArrayString(bool includeWhitespace = true)
        {
            StringBuilder stringBuilder = new StringBuilder();
            int[] array = ArrayUtilities.GetStrides(dimensions);
            int[] array2 = new int[Rank];
            int num = Rank - 1;
            int num2 = dimensions[num];
            int num3 = 0;
            for (int i = 0; i < Length; i += num2)
            {
                ArrayUtilities.GetIndices(array, reverseStride: false, i, array2);
                while (num3 < num && array2[num3] == 0)
                {
                    if (includeWhitespace)
                    {
                        Indent(stringBuilder, num3);
                    }
                    num3++;
                    stringBuilder.Append('{');
                    if (includeWhitespace)
                    {
                        stringBuilder.AppendLine();
                    }
                }
                for (int j = 0; j < num2; j++)
                {
                    array2[num] = j;
                    if (j == 0)
                    {
                        if (includeWhitespace)
                        {
                            Indent(stringBuilder, num3);
                        }
                        stringBuilder.Append('{');
                    }
                    else
                    {
                        stringBuilder.Append(',');
                    }
                    stringBuilder.Append(this[array2]);
                }
                stringBuilder.Append('}');
                int num4 = Rank - 2;
                while (num4 >= 0)
                {
                    int num5 = dimensions[num4] - 1;
                    if (array2[num4] == num5)
                    {
                        num3--;
                        if (includeWhitespace)
                        {
                            stringBuilder.AppendLine();
                            Indent(stringBuilder, num3);
                        }
                        stringBuilder.Append('}');
                        num4--;
                        continue;
                    }
                    stringBuilder.Append(',');
                    if (includeWhitespace)
                    {
                        stringBuilder.AppendLine();
                    }
                    break;
                }
            }
            return stringBuilder.ToString();
        }

        private static void Indent(StringBuilder builder, int tabs, int spacesPerTab = 4)
        {
            for (int i = 0; i < tabs; i++)
            {
                for (int j = 0; j < spacesPerTab; j++)
                {
                    builder.Append(' ');
                }
            }
        }

        private static bool IsCompatibleObject(object value)
        {
            if (!(value is T))
            {
                if (value == null)
                {
                    return default(T) == null;
                }
                return false;
            }
            return true;
        }
    }
    #nullable disable
    /// <summary>
    /// Represents a tensor using compressed sparse format
    /// For a two dimensional tensor this is referred to as compressed sparse row (CSR, CRS, Yale), compressed sparse column (CSC, CCS)
    ///
    /// In this format, data that is in the same value for the compressed dimension has locality
    ///
    /// In standard layout of a dense tensor, data with the same value for first dimensions has locality.
    /// As such we'll use reverseStride = false (default) to mean that the first dimension is compressed (CSR)
    /// and reverseStride = true to mean that the last dimension is compressed (CSC)
    ///
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CompressedSparseTensor<T> : Tensor<T>
    {
        private Memory<T> values;

        private readonly Memory<int> compressedCounts;

        private Memory<int> indices;

        private int nonZeroCount;

        private readonly int[] nonCompressedStrides;

        private readonly int compressedDimension;

        private const int defaultCapacity = 64;

        /// <summary>
        /// Obtains the value at the specified indices
        /// </summary>
        /// <param name="indices">A span of integers that represent the indices specifying the position of the element to get.</param>
        /// <returns>The value at the specified position in this Tensor.</returns>
        public override T this[ReadOnlySpan<int> indices]
        {
            get
            {
                int compressedIndex = indices[compressedDimension];
                int index = ArrayUtilities.GetIndex(nonCompressedStrides, indices);
                if (TryFindIndex(compressedIndex, index, out var valueIndex))
                {
                    return values.Span[valueIndex];
                }
                return Tensor<T>.Zero;
            }
            set
            {
                int compressedIndex = indices[compressedDimension];
                int index = ArrayUtilities.GetIndex(nonCompressedStrides, indices);
                SetAt(value, compressedIndex, index);
            }
        }

        /// <summary>
        /// Gets the number of non-zero values this tensor can store without resizing.
        /// </summary>
        public int Capacity => values.Length;

        /// <summary>
        /// Get's the number on non-zero values currently being stored in this tensor.
        /// </summary>
        public int NonZeroCount => nonZeroCount;

        /// <summary>
        /// Memory storing non-zero values.
        /// </summary>
        public Memory<T> Values => values;

        /// <summary>
        /// Memory storing the counts of non-zero elements at each index of the compressed dimension.
        /// </summary>
        public Memory<int> CompressedCounts => compressedCounts;

        /// <summary>
        /// Memory storing the linearized index (excluding the compressed dimension) of non-zero elements.
        /// </summary>
        public Memory<int> Indices => indices;

        /// <summary>
        /// Constructs a new CompressedSparseTensor of the specified dimensions and stride ordering.
        /// </summary>
        /// <param name="dimensions">An span of integers that represent the size of each dimension of the CompressedSparseTensor to create.</param>
        /// <param name="reverseStride">False (default) to indicate that the first dimension is most major (farthest apart) and the last dimension is most minor (closest together): akin to row-major in a rank-2 tensor.  True to indicate that the last dimension is most major (farthest apart) and the first dimension is most minor (closest together): akin to column-major in a rank-2 tensor.</param>
        public CompressedSparseTensor(ReadOnlySpan<int> dimensions, bool reverseStride = false)
            : this(dimensions, 64, reverseStride)
        {
        }

        /// <summary>
        /// Constructs a new CompressedSparseTensor of the specified dimensions, initial capacity, and stride ordering.
        /// </summary>
        /// <param name="dimensions">An span of integers that represent the size of each dimension of the CompressedSparseTensor to create.</param>
        /// <param name="capacity">The number of non-zero values this tensor can store without resizing.</param>
        /// <param name="reverseStride">False (default) to indicate that the first dimension is most major (farthest apart) and the last dimension is most minor (closest together): akin to row-major in a rank-2 tensor.  True to indicate that the last dimension is most major (farthest apart) and the first dimension is most minor (closest together): akin to column-major in a rank-2 tensor.</param>
        public CompressedSparseTensor(ReadOnlySpan<int> dimensions, int capacity, bool reverseStride = false)
            : base(dimensions, reverseStride)
        {
            nonZeroCount = 0;
            compressedDimension = (reverseStride ? (base.Rank - 1) : 0);
            nonCompressedStrides = (int[])strides.Clone();
            nonCompressedStrides[compressedDimension] = 0;
            int num = dimensions[compressedDimension];
            compressedCounts = new int[num + 1];
            values = new T[capacity];
            indices = new int[capacity];
        }

        /// <summary>
        /// Constructs a new CompressedSparseTensor of the specified dimensions, wrapping existing backing memory for the contents.
        /// Growing this CompressedSparseTensor will re-allocate the backing memory.
        /// </summary>
        /// <param name="values">Memory storing non-zero values to construct this tensor with.</param>
        /// <param name="compressedCounts">Memory storing the counts of non-zero elements at each index of the compressed dimension.</param>
        /// <param name="indices">Memory storing the linearized index (excluding the compressed dimension) of non-zero elements.</param>
        /// <param name="nonZeroCount">The number of valid entries (eg: non-zero values) in <paramref name="values" /> and <paramref name="indices" />.</param>
        /// <param name="dimensions">An span of integers that represent the size of each dimension of the CompressedSparseTensor to create.</param>
        /// <param name="reverseStride">False (default) to indicate that the first dimension is most major (farthest apart) and the last dimension is most minor (closest together): akin to row-major in a rank-2 tensor.  True to indicate that the last dimension is most major (farthest apart) and the first dimension is most minor (closest together): akin to column-major in a rank-2 tensor.</param>
        public CompressedSparseTensor(Memory<T> values, Memory<int> compressedCounts, Memory<int> indices, int nonZeroCount, ReadOnlySpan<int> dimensions, bool reverseStride = false)
            : base(dimensions, reverseStride)
        {
            compressedDimension = (reverseStride ? (base.Rank - 1) : 0);
            nonCompressedStrides = (int[])strides.Clone();
            nonCompressedStrides[compressedDimension] = 0;
            this.values = values;
            this.compressedCounts = compressedCounts;
            this.indices = indices;
            this.nonZeroCount = nonZeroCount;
        }

        internal CompressedSparseTensor(Array fromArray, bool reverseStride = false)
            : base(fromArray, reverseStride)
        {
            nonZeroCount = 0;
            compressedDimension = (reverseStride ? (base.Rank - 1) : 0);
            nonCompressedStrides = (int[])strides.Clone();
            nonCompressedStrides[compressedDimension] = 0;
            int num = dimensions[compressedDimension];
            compressedCounts = new int[num + 1];
            int num2 = 0;
            if (reverseStride)
            {
                int[] sourceStrides = ArrayUtilities.GetStrides(dimensions);
                {
                    foreach (T item in fromArray)
                    {
                        if (!item.Equals(Tensor<T>.Zero))
                        {
                            int num3 = ArrayUtilities.TransformIndexByStrides(num2, sourceStrides, sourceReverseStride: false, strides);
                            int compressedIndex = num3 / strides[compressedDimension];
                            int nonCompressedIndex = num3 % strides[compressedDimension];
                            SetAt(item, compressedIndex, nonCompressedIndex);
                        }
                        num2++;
                    }
                    return;
                }
            }
            foreach (T item2 in fromArray)
            {
                if (!item2.Equals(Tensor<T>.Zero))
                {
                    int compressedIndex2 = num2 / strides[compressedDimension];
                    int nonCompressedIndex2 = num2 % strides[compressedDimension];
                    SetAt(item2, compressedIndex2, nonCompressedIndex2);
                }
                num2++;
            }
        }

        /// <summary>
        /// Gets the value at the specified index, where index is lineraized as a dot product between indices and strides.
        /// </summary>
        /// <param name="index">An integer index computed as a dot-product of indices.</param>
        /// <returns>The value at the specified position in this Tensor.</returns>
        public override T GetValue(int index)
        {
            int num = strides[compressedDimension];
            int compressedIndex = index / num;
            int nonCompressedIndex = index % num;
            if (TryFindIndex(compressedIndex, nonCompressedIndex, out var valueIndex))
            {
                return values.Span[valueIndex];
            }
            return Tensor<T>.Zero;
        }

        /// <summary>
        /// Sets the value at the specified index, where index is a linearized version of n-dimension indices using strides.
        /// </summary>
        /// <param name="index">An integer index computed as a dot-product of indices.</param>
        /// <param name="value">The new value to set at the specified position in this Tensor.</param>
        public override void SetValue(int index, T value)
        {
            int num = strides[compressedDimension];
            int compressedIndex = index / num;
            int nonCompressedIndex = index % num;
            SetAt(value, compressedIndex, nonCompressedIndex);
        }

        private void EnsureCapacity(int min, int allocateIndex = -1)
        {
            if (values.Length >= min)
            {
                return;
            }
            int num = ((values.Length == 0) ? 64 : (values.Length * 2));
            if (num > base.Length)
            {
                num = (int)base.Length;
            }
            if (num < min)
            {
                num = min;
            }
            Memory<T> memory = new T[num];
            Memory<int> memory2 = new int[num];
            if (nonZeroCount > 0)
            {
                Span<T> span;
                Span<int> span3;
                if (allocateIndex == -1)
                {
                    span = values.Span;
                    Span<T> span2 = span.Slice(0, nonZeroCount);
                    span3 = indices.Span;
                    Span<int> span4 = span3.Slice(0, nonZeroCount);
                    span2.CopyTo(memory.Span);
                    span4.CopyTo(memory2.Span);
                }
                else
                {
                    if (allocateIndex > 0)
                    {
                        span = values.Span;
                        Span<T> span5 = span.Slice(0, allocateIndex);
                        span3 = indices.Span;
                        Span<int> span6 = span3.Slice(0, allocateIndex);
                        span5.CopyTo(memory.Span);
                        span6.CopyTo(memory2.Span);
                    }
                    if (allocateIndex < nonZeroCount)
                    {
                        span = values.Span;
                        Span<T> span7 = span.Slice(allocateIndex, nonZeroCount - allocateIndex);
                        span3 = indices.Span;
                        Span<int> span8 = span3.Slice(allocateIndex, nonZeroCount - allocateIndex);
                        span = memory.Span;
                        Span<T> destination = span.Slice(allocateIndex + 1, nonZeroCount - allocateIndex);
                        span3 = memory2.Span;
                        Span<int> destination2 = span3.Slice(allocateIndex + 1, nonZeroCount - allocateIndex);
                        span7.CopyTo(destination);
                        span8.CopyTo(destination2);
                    }
                }
            }
            values = memory;
            indices = memory2;
        }

        private void InsertAt(int valueIndex, T value, int compressedIndex, int nonCompressedIndex)
        {
            Span<T> span;
            Span<int> span2;
            if (values.Length <= valueIndex)
            {
                EnsureCapacity(valueIndex + 1, valueIndex);
            }
            else if (nonZeroCount != valueIndex)
            {
                span = values.Span;
                span = span.Slice(valueIndex, nonZeroCount - valueIndex);
                span.CopyTo(values.Span.Slice(valueIndex + 1));
                span2 = indices.Span;
                span2 = span2.Slice(valueIndex, nonZeroCount - valueIndex);
                span2.CopyTo(indices.Span.Slice(valueIndex + 1));
            }
            span = values.Span;
            span[valueIndex] = value;
            span2 = indices.Span;
            span2[valueIndex] = nonCompressedIndex;
            span2 = compressedCounts.Span;
            Span<int> span3 = span2.Slice(compressedIndex + 1);
            for (int i = 0; i < span3.Length; i++)
            {
                span3[i]++;
            }
            nonZeroCount++;
        }

        private void RemoveAt(int valueIndex, int compressedIndex)
        {
            Span<T> span = values.Span;
            span = span.Slice(valueIndex + 1, nonZeroCount - valueIndex - 1);
            span.CopyTo(values.Span.Slice(valueIndex));
            Span<int> span2 = indices.Span;
            span2 = span2.Slice(valueIndex + 1, nonZeroCount - valueIndex - 1);
            span2.CopyTo(indices.Span.Slice(valueIndex));
            span2 = compressedCounts.Span;
            Span<int> span3 = span2.Slice(compressedIndex + 1);
            for (int i = 0; i < span3.Length; i++)
            {
                span3[i]--;
            }
            nonZeroCount--;
        }

        private void SetAt(T value, int compressedIndex, int nonCompressedIndex)
        {
            bool flag = value.Equals(Tensor<T>.Zero);
            if (TryFindIndex(compressedIndex, nonCompressedIndex, out var valueIndex))
            {
                if (flag)
                {
                    RemoveAt(valueIndex, compressedIndex);
                    return;
                }
                values.Span[valueIndex] = value;
                indices.Span[valueIndex] = nonCompressedIndex;
            }
            else if (!flag)
            {
                InsertAt(valueIndex, value, compressedIndex, nonCompressedIndex);
            }
        }

        /// <summary>
        /// Trys to find the place to store a value
        /// </summary>
        /// <param name="compressedIndex"></param>
        /// <param name="nonCompressedIndex"></param>
        /// <param name="valueIndex"></param>
        /// <returns>True if element is found at specific index, false if no specific index is found and insertion point is returned</returns>
        private bool TryFindIndex(int compressedIndex, int nonCompressedIndex, out int valueIndex)
        {
            if (nonZeroCount == 0)
            {
                valueIndex = 0;
                return false;
            }
            Span<int> span = compressedCounts.Span;
            int num = span[compressedIndex];
            int num2 = span[compressedIndex + 1];
            Span<int> span2 = indices.Span;
            for (valueIndex = num; valueIndex < num2; valueIndex++)
            {
                if (span2[valueIndex] == nonCompressedIndex)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Creates a shallow copy of this tensor, with new backing storage.
        /// </summary>
        /// <returns>A shallow copy of this tensor.</returns>
        public override Tensor<T> Clone()
        {
            return new CompressedSparseTensor<T>(values.ToArray(), compressedCounts.ToArray(), indices.ToArray(), nonZeroCount, dimensions, base.IsReversedStride);
        }

        /// <summary>
        /// Creates a new Tensor of a different type with the specified dimensions and the same layout as this tensor with elements initialized to their default value.
        /// </summary>
        /// <typeparam name="TResult">Type contained in the returned Tensor.</typeparam>
        /// <param name="dimensions">An span of integers that represent the size of each dimension of the CompressedSparseTensor to create.</param>
        /// <returns>A new tensor with the same layout as this tensor but different type and dimensions.</returns>
        public override Tensor<TResult> CloneEmpty<TResult>(ReadOnlySpan<int> dimensions)
        {
            return new CompressedSparseTensor<TResult>(dimensions, base.IsReversedStride);
        }

        /// <summary>
        /// Reshapes the current tensor to new dimensions. Unlike other Tensor implementations, CompressedSparseTensor&lt;T&gt; must allocate new backing storage to represent a reshaped Tensor.
        /// </summary>
        /// <param name="dimensions">An span of integers that represent the size of each dimension of the CompressedSparseTensor to create.</param>
        /// <returns>A new tensor that reinterprets the content of this tensor to new dimensions (assuming the same linear index for each element).</returns>
        public override Tensor<T> Reshape(ReadOnlySpan<int> dimensions)
        {
            int index = (base.IsReversedStride ? (dimensions.Length - 1) : 0);
            int num = dimensions[index];
            int num2 = (int)(base.Length / num);
            T[] array = values.ToArray();
            int[] array2 = new int[num + 1];
            int[] array3 = new int[indices.Length];
            int i = 0;
            Span<int> span = compressedCounts.Span;
            Span<int> span2 = indices.Span.Slice(0, nonZeroCount);
            for (int j = 0; j < span2.Length; j++)
            {
                for (; j >= span[i + 1]; i++)
                {
                }
                int num3 = span2[j] + i * strides[compressedDimension];
                array3[j] = num3 % num2;
                int num4 = num3 / num2;
                array2[num4 + 1] = j + 1;
            }
            return new CompressedSparseTensor<T>(array, array2, array3, nonZeroCount, dimensions, base.IsReversedStride);
        }

        /// <summary>
        /// Creates a copy of this tensor as a DenseTensor&lt;T&gt;.
        /// </summary>
        /// <returns>A copy of this tensor as a DenseTensor&lt;T&gt;</returns>
        public override DenseTensor<T> ToDenseTensor()
        {
            DenseTensor<T> denseTensor = new DenseTensor<T>(base.Dimensions, base.IsReversedStride);
            int i = 0;
            Span<int> span = compressedCounts.Span;
            Span<int> span2 = indices.Span.Slice(0, nonZeroCount);
            Span<T> span3 = values.Span.Slice(0, nonZeroCount);
            for (int j = 0; j < span3.Length; j++)
            {
                for (; j >= span[i + 1]; i++)
                {
                }
                int index = span2[j] + i * strides[compressedDimension];
                denseTensor.SetValue(index, span3[j]);
            }
            return denseTensor;
        }

        /// <summary>
        /// Creates a copy of this tensor as a new CompressedSparseTensor&lt;T&gt; eliminating any unused space in the backing storage.
        /// </summary>
        /// <returns>A copy of this tensor as a CompressedSparseTensor&lt;T&gt;.</returns>
        public override CompressedSparseTensor<T> ToCompressedSparseTensor()
        {
            T[] array = values.Slice(0, nonZeroCount).ToArray();
            int[] array2 = indices.Slice(0, nonZeroCount).ToArray();
            return new CompressedSparseTensor<T>(array, compressedCounts.ToArray(), array2, nonZeroCount, dimensions, base.IsReversedStride);
        }

        /// <summary>
        /// Creates a copy of this tensor as a SparseTensor&lt;T&gt;.
        /// </summary>
        /// <returns>A copy of this tensor as a SparseTensor&lt;T&gt;.</returns>
        public override SparseTensor<T> ToSparseTensor()
        {
            SparseTensor<T> sparseTensor = new SparseTensor<T>(dimensions, capacity: NonZeroCount, reverseStride: base.IsReversedStride);
            int i = 0;
            Span<int> span = compressedCounts.Span;
            Span<int> span2 = indices.Span.Slice(0, nonZeroCount);
            Span<T> span3 = values.Span.Slice(0, nonZeroCount);
            for (int j = 0; j < span3.Length; j++)
            {
                for (; j >= span[i + 1]; i++)
                {
                }
                int index = span2[j] + i * strides[compressedDimension];
                sparseTensor.SetValue(index, span3[j]);
            }
            return sparseTensor;
        }
    }

    public static class ArrayTensorExtensions
    {
        /// <summary>
        /// Creates a copy of this single-dimensional array as a DenseTensor&lt;T&gt;
        /// </summary>
        /// <typeparam name="T">Type contained in the array to copy to the DenseTensor&lt;T&gt;.</typeparam>
        /// <param name="array">The array to create a DenseTensor&lt;T&gt; from.</param>
        /// <returns>A 1-dimensional DenseTensor&lt;T&gt; with the same length and content as <paramref name="array" />.</returns>
        public static DenseTensor<T> ToTensor<T>(this T[] array)
        {
            return new DenseTensor<T>(array);
        }

        /// <summary>
        /// Creates a copy of this two-dimensional array as a DenseTensor&lt;T&gt;
        /// </summary>
        /// <typeparam name="T">Type contained in the array to copy to the DenseTensor&lt;T&gt;.</typeparam>
        /// <param name="array">The array to create a DenseTensor&lt;T&gt; from.</param>
        /// <param name="reverseStride">False (default) to indicate that the first dimension is most major (farthest apart) and the last dimension is most minor (closest together): row-major.  True to indicate that the last dimension is most major (farthest apart) and the first dimension is most minor (closest together): column-major.</param>
        /// <returns>A 2-dimensional DenseTensor&lt;T&gt; with the same dimensions and content as <paramref name="array" />.</returns>
        public static DenseTensor<T> ToTensor<T>(this T[,] array, bool reverseStride = false)
        {
            return new DenseTensor<T>(array, reverseStride);
        }

        /// <summary>
        /// Creates a copy of this three-dimensional array as a DenseTensor&lt;T&gt;
        /// </summary>
        /// <typeparam name="T">Type contained in the array to copy to the DenseTensor&lt;T&gt;.</typeparam>
        /// <param name="array">The array to create a DenseTensor&lt;T&gt; from.</param>
        /// <param name="reverseStride">False (default) to indicate that the first dimension is most major (farthest apart) and the last dimension is most minor (closest together): akin to row-major in a rank-2 tensor.  True to indicate that the last dimension is most major (farthest apart) and the first dimension is most minor (closest together): akin to column-major in a rank-2 tensor.</param>
        /// <returns>A 3-dimensional DenseTensor&lt;T&gt; with the same dimensions and content as <paramref name="array" />.</returns>
        public static DenseTensor<T> ToTensor<T>(this T[,,] array, bool reverseStride = false)
        {
            return new DenseTensor<T>(array, reverseStride);
        }

        /// <summary>
        /// Creates a copy of this n-dimensional array as a DenseTensor&lt;T&gt;
        /// </summary>
        /// <typeparam name="T">Type contained in the array to copy to the DenseTensor&lt;T&gt;.</typeparam>
        /// <param name="array">The array to create a DenseTensor&lt;T&gt; from.</param>
        /// <param name="reverseStride">False (default) to indicate that the first dimension is most major (farthest apart) and the last dimension is most minor (closest together): akin to row-major in a rank-2 tensor.  True to indicate that the last dimension is most major (farthest apart) and the first dimension is most minor (closest together): akin to column-major in a rank-2 tensor.</param>
        /// <returns>A n-dimensional DenseTensor&lt;T&gt; with the same dimensions and content as <paramref name="array" />.</returns>
        public static DenseTensor<T> ToTensor<T>(this Array array, bool reverseStride = false)
        {
            return new DenseTensor<T>(array, reverseStride);
        }

        /// <summary>
        /// Creates a copy of this single-dimensional array as a SparseTensor&lt;T&gt;
        /// </summary>
        /// <typeparam name="T">Type contained in the array to copy to the SparseTensor&lt;T&gt;.</typeparam>
        /// <param name="array">The array to create a SparseTensor&lt;T&gt; from.</param>
        /// <returns>A 1-dimensional SparseTensor&lt;T&gt; with the same length and content as <paramref name="array" />.</returns>
        public static SparseTensor<T> ToSparseTensor<T>(this T[] array)
        {
            return new SparseTensor<T>(array);
        }

        /// <summary>
        /// Creates a copy of this two-dimensional array as a SparseTensor&lt;T&gt;
        /// </summary>
        /// <typeparam name="T">Type contained in the array to copy to the SparseTensor&lt;T&gt;.</typeparam>
        /// <param name="array">The array to create a SparseTensor&lt;T&gt; from.</param>
        /// <param name="reverseStride">False (default) to indicate that the first dimension is most major (farthest apart) and the last dimension is most minor (closest together): row-major.  True to indicate that the last dimension is most major (farthest apart) and the first dimension is most minor (closest together): column-major.</param>
        /// <returns>A 2-dimensional SparseTensor&lt;T&gt; with the same dimensions and content as <paramref name="array" />.</returns>
        public static SparseTensor<T> ToSparseTensor<T>(this T[,] array, bool reverseStride = false)
        {
            return new SparseTensor<T>(array, reverseStride);
        }

        /// <summary>
        /// Creates a copy of this three-dimensional array as a SparseTensor&lt;T&gt;
        /// </summary>
        /// <typeparam name="T">Type contained in the array to copy to the SparseTensor&lt;T&gt;.</typeparam>
        /// <param name="array">The array to create a SparseTensor&lt;T&gt; from.</param>
        /// <param name="reverseStride">False (default) to indicate that the first dimension is most major (farthest apart) and the last dimension is most minor (closest together): akin to row-major in a rank-2 tensor.  True to indicate that the last dimension is most major (farthest apart) and the first dimension is most minor (closest together): akin to column-major in a rank-2 tensor.</param>
        /// <returns>A 3-dimensional SparseTensor&lt;T&gt; with the same dimensions and content as <paramref name="array" />.</returns>
        public static SparseTensor<T> ToSparseTensor<T>(this T[,,] array, bool reverseStride = false)
        {
            return new SparseTensor<T>(array, reverseStride);
        }

        /// <summary>
        /// Creates a copy of this n-dimensional array as a SparseTensor&lt;T&gt;
        /// </summary>
        /// <typeparam name="T">Type contained in the array to copy to the SparseTensor&lt;T&gt;.</typeparam>
        /// <param name="array">The array to create a SparseTensor&lt;T&gt; from.</param>
        /// <param name="reverseStride">False (default) to indicate that the first dimension is most major (farthest apart) and the last dimension is most minor (closest together): akin to row-major in a rank-2 tensor.  True to indicate that the last dimension is most major (farthest apart) and the first dimension is most minor (closest together): akin to column-major in a rank-2 tensor.</param>
        /// <returns>A n-dimensional SparseTensor&lt;T&gt; with the same dimensions and content as <paramref name="array" />.</returns>
        public static SparseTensor<T> ToSparseTensor<T>(this Array array, bool reverseStride = false)
        {
            return new SparseTensor<T>(array, reverseStride);
        }

        /// <summary>
        /// Creates a copy of this single-dimensional array as a CompressedSparseTensor&lt;T&gt;
        /// </summary>
        /// <typeparam name="T">Type contained in the array to copy to the CompressedSparseTensor&lt;T&gt;.</typeparam>
        /// <param name="array">The array to create a CompressedSparseTensor&lt;T&gt; from.</param>
        /// <returns>A 1-dimensional CompressedSparseTensor&lt;T&gt; with the same length and content as <paramref name="array" />.</returns>
        public static CompressedSparseTensor<T> ToCompressedSparseTensor<T>(this T[] array)
        {
            return new CompressedSparseTensor<T>(array);
        }

        /// <summary>
        /// Creates a copy of this two-dimensional array as a CompressedSparseTensor&lt;T&gt;
        /// </summary>
        /// <typeparam name="T">Type contained in the array to copy to the CompressedSparseTensor&lt;T&gt;.</typeparam>
        /// <param name="array">The array to create a CompressedSparseTensor&lt;T&gt; from.</param>
        /// <param name="reverseStride">False (default) to indicate that the first dimension is most major (farthest apart) and the last dimension is most minor (closest together): row-major.  True to indicate that the last dimension is most major (farthest apart) and the first dimension is most minor (closest together): column-major.</param>
        /// <returns>A 2-dimensional CompressedSparseTensor&lt;T&gt; with the same dimensions and content as <paramref name="array" />.</returns>
        public static CompressedSparseTensor<T> ToCompressedSparseTensor<T>(this T[,] array, bool reverseStride = false)
        {
            return new CompressedSparseTensor<T>(array, reverseStride);
        }

        /// <summary>
        /// Creates a copy of this three-dimensional array as a CompressedSparseTensor&lt;T&gt;
        /// </summary>
        /// <typeparam name="T">Type contained in the array to copy to the CompressedSparseTensor&lt;T&gt;.</typeparam>
        /// <param name="array">The array to create a CompressedSparseTensor&lt;T&gt; from.</param>
        /// <param name="reverseStride">False (default) to indicate that the first dimension is most major (farthest apart) and the last dimension is most minor (closest together): akin to row-major in a rank-2 tensor.  True to indicate that the last dimension is most major (farthest apart) and the first dimension is most minor (closest together): akin to column-major in a rank-2 tensor.</param>
        /// <returns>A 3-dimensional CompressedSparseTensor&lt;T&gt; with the same dimensions and content as <paramref name="array" />.</returns>
        public static CompressedSparseTensor<T> ToCompressedSparseTensor<T>(this T[,,] array, bool reverseStride = false)
        {
            return new CompressedSparseTensor<T>(array, reverseStride);
        }

        /// <summary>
        /// Creates a copy of this n-dimensional array as a CompressedSparseTensor&lt;T&gt;
        /// </summary>
        /// <typeparam name="T">Type contained in the array to copy to the CompressedSparseTensor&lt;T&gt;.</typeparam>
        /// <param name="array">The array to create a CompressedSparseTensor&lt;T&gt; from.</param>
        /// <param name="reverseStride">False (default) to indicate that the first dimension is most major (farthest apart) and the last dimension is most minor (closest together): akin to row-major in a rank-2 tensor.  True to indicate that the last dimension is most major (farthest apart) and the first dimension is most minor (closest together): akin to column-major in a rank-2 tensor.</param>
        /// <returns>A n-dimensional CompressedSparseTensor&lt;T&gt; with the same dimensions and content as <paramref name="array" />.</returns>
        public static CompressedSparseTensor<T> ToCompressedSparseTensor<T>(this Array array, bool reverseStride = false)
        {
            return new CompressedSparseTensor<T>(array, reverseStride);
        }
    }

    internal static class ArrayUtilities
    {
        public const int StackallocMax = 16;

        public static long GetProduct(ReadOnlySpan<int> dimensions, int startIndex = 0)
        {
            if (dimensions.Length == 0)
            {
                return 0L;
            }
            long num = 1L;
            for (int i = startIndex; i < dimensions.Length; i++)
            {
                if (dimensions[i] < 0)
                {
                    throw new ArgumentOutOfRangeException(string.Format("{0}[{1}]", "dimensions", i));
                }
                num = checked(num * dimensions[i]);
            }
            return num;
        }

        public static bool IsAscending(ReadOnlySpan<int> values)
        {
            for (int i = 1; i < values.Length; i++)
            {
                if (values[i] < values[i - 1])
                {
                    return false;
                }
            }
            return true;
        }

        public static bool IsDescending(ReadOnlySpan<int> values)
        {
            for (int i = 1; i < values.Length; i++)
            {
                if (values[i] > values[i - 1])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Gets the set of strides that can be used to calculate the offset of n-dimensions in a 1-dimensional layout
        /// </summary>
        /// <param name="dimensions"></param>
        /// <param name="reverseStride"></param>
        /// <returns></returns>
        public static int[] GetStrides(ReadOnlySpan<int> dimensions, bool reverseStride = false)
        {
            int[] array = new int[dimensions.Length];
            int num = 1;
            if (reverseStride)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = num;
                    num *= dimensions[i];
                }
            }
            else
            {
                for (int num2 = array.Length - 1; num2 >= 0; num2--)
                {
                    array[num2] = num;
                    num *= dimensions[num2];
                }
            }
            return array;
        }

        public static void SplitStrides(int[] strides, int[] splitAxes, int[] newStrides, int stridesOffset, int[] splitStrides, int splitStridesOffset)
        {
            int num = 0;
            for (int i = 0; i < strides.Length; i++)
            {
                int num2 = strides[i];
                bool flag = false;
                for (int j = 0; j < splitAxes.Length; j++)
                {
                    if (splitAxes[j] == i)
                    {
                        splitStrides[splitStridesOffset + j] = num2;
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    newStrides[stridesOffset + num++] = num2;
                }
            }
        }

        /// <summary>
        /// Calculates the 1-d index for n-d indices in layout specified by strides.
        /// </summary>
        /// <param name="strides"></param>
        /// <param name="indices"></param>
        /// <param name="startFromDimension"></param>
        /// <returns></returns>
        public static int GetIndex(int[] strides, ReadOnlySpan<int> indices, int startFromDimension = 0)
        {
            int num = 0;
            for (int i = startFromDimension; i < indices.Length; i++)
            {
                num += strides[i] * indices[i];
            }
            return num;
        }

        /// <summary>
        /// Calculates the n-d indices from the 1-d index in a layout specified by strides
        /// </summary>
        /// <param name="strides"></param>
        /// <param name="reverseStride"></param>
        /// <param name="index"></param>
        /// <param name="indices"></param>
        /// <param name="startFromDimension"></param>
        public static void GetIndices(ReadOnlySpan<int> strides, bool reverseStride, int index, int[] indices, int startFromDimension = 0)
        {
            int num = index;
            for (int i = startFromDimension; i < strides.Length; i++)
            {
                int num2 = (reverseStride ? (strides.Length - 1 - i) : i);
                int num3 = strides[num2];
                indices[num2] = num / num3;
                num %= num3;
            }
        }

        /// <summary>
        /// Calculates the n-d indices from the 1-d index in a layout specified by strides
        /// </summary>
        /// <param name="strides"></param>
        /// <param name="reverseStride"></param>
        /// <param name="index"></param>
        /// <param name="indices"></param>
        /// <param name="startFromDimension"></param>
        public static void GetIndices(ReadOnlySpan<int> strides, bool reverseStride, int index, Span<int> indices, int startFromDimension = 0)
        {
            int num = index;
            for (int i = startFromDimension; i < strides.Length; i++)
            {
                int index2 = (reverseStride ? (strides.Length - 1 - i) : i);
                int num2 = strides[index2];
                indices[index2] = num / num2;
                num %= num2;
            }
        }

        /// <summary>
        /// Takes an 1-d index over n-d sourceStrides and recalculates it assuming same n-d coordinates over a different n-d strides
        /// </summary>
        public static int TransformIndexByStrides(int index, int[] sourceStrides, bool sourceReverseStride, int[] transformStrides)
        {
            int num = 0;
            int num2 = index;
            for (int i = 0; i < sourceStrides.Length; i++)
            {
                int num3 = (sourceReverseStride ? (sourceStrides.Length - 1 - i) : i);
                int num4 = sourceStrides[num3];
                int num5 = transformStrides[num3];
                num += num5 * (num2 / num4);
                num2 %= num4;
            }
            return num;
        }
    }

    /// <summary>
	/// Represents a multi-dimensional collection of objects of type T that can be accessed by indices.  DenseTensor stores values in a contiguous sequential block of memory where all values are represented.
	/// </summary>
	/// <typeparam name="T">type contained within the Tensor.  Typically a value type such as int, double, float, etc.</typeparam>
	public class DenseTensor<T> : Tensor<T>
    {
        private readonly Memory<T> memory;

        /// <summary>
        /// Memory storing backing values of this tensor.
        /// </summary>
        public Memory<T> Buffer => memory;

        internal DenseTensor(Array fromArray, bool reverseStride = false)
            : base(fromArray, reverseStride)
        {
            T[] array = new T[fromArray.Length];
            int num = 0;
            if (reverseStride)
            {
                int[] sourceStrides = ArrayUtilities.GetStrides(dimensions);
                foreach (object item in fromArray)
                {
                    int num2 = ArrayUtilities.TransformIndexByStrides(num++, sourceStrides, sourceReverseStride: false, strides);
                    array[num2] = (T)item;
                }
            }
            else
            {
                foreach (object item2 in fromArray)
                {
                    array[num++] = (T)item2;
                }
            }
            memory = array;
        }

        /// <summary>
        /// Initializes a rank-1 Tensor using the specified <paramref name="length" />.
        /// </summary>
        /// <param name="length">Size of the 1-dimensional tensor</param>
        public DenseTensor(int length)
            : base(length)
        {
            memory = new T[length];
        }

        /// <summary>
        /// Initializes a rank-n Tensor using the dimensions specified in <paramref name="dimensions" />.
        /// </summary>
        /// <param name="dimensions">An span of integers that represent the size of each dimension of the DenseTensor to create.</param>
        /// <param name="reverseStride">False (default) to indicate that the first dimension is most major (farthest apart) and the last dimension is most minor (closest together): akin to row-major in a rank-2 tensor.  True to indicate that the last dimension is most major (farthest apart) and the first dimension is most minor (closest together): akin to column-major in a rank-2 tensor.</param>
        public DenseTensor(ReadOnlySpan<int> dimensions, bool reverseStride = false)
            : base(dimensions, reverseStride)
        {
            memory = new T[base.Length];
        }

        /// <summary>
        /// Constructs a new DenseTensor of the specified dimensions, wrapping existing backing memory for the contents.
        /// </summary>
        /// <param name="memory"></param>
        /// <param name="dimensions">An span of integers that represent the size of each dimension of the DenseTensor to create.</param>
        /// <param name="reverseStride">False (default) to indicate that the first dimension is most major (farthest apart) and the last dimension is most minor (closest together): akin to row-major in a rank-2 tensor.  True to indicate that the last dimension is most major (farthest apart) and the first dimension is most minor (closest together): akin to column-major in a rank-2 tensor.</param>
        public DenseTensor(Memory<T> memory, ReadOnlySpan<int> dimensions, bool reverseStride = false)
            : base(dimensions, reverseStride)
        {
            this.memory = memory;
            if (base.Length != memory.Length)
            {
                throw new ArgumentException(System.SR.Format(MDCFR.Properties.Resources.LengthMustMatch, "memory", memory.Length, "dimensions", base.Length));
            }
        }

        /// <summary>
        /// Gets the value at the specified index, where index is a linearized version of n-dimension indices using strides.
        /// </summary>
        /// <param name="index">An integer index computed as a dot-product of indices.</param>
        /// <returns>The value at the specified position in this Tensor.</returns>
        public override T GetValue(int index)
        {
            return Buffer.Span[index];
        }

        /// <summary>
        /// Sets the value at the specified index, where index is a linearized version of n-dimension indices using strides.
        /// </summary>
        /// <param name="index">An integer index computed as a dot-product of indices.</param>
        /// <param name="value">The new value to set at the specified position in this Tensor.</param>
        public override void SetValue(int index, T value)
        {
            Buffer.Span[index] = value;
        }

        protected override void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (array.Length < arrayIndex + base.Length)
            {
                throw new ArgumentException(MDCFR.Properties.Resources.NumberGreaterThenAvailableSpace, "array");
            }
            Buffer.Span.CopyTo(array.AsSpan(arrayIndex));
        }

        protected override int IndexOf(T item)
        {
            if (MemoryMarshal.TryGetArray((ReadOnlyMemory<T>)Buffer, out ArraySegment<T> segment))
            {
                int num = Array.IndexOf<T>(segment.Array, item, segment.Offset, segment.Count);
                if (num != -1)
                {
                    num -= segment.Offset;
                }
                return num;
            }
            return base.IndexOf(item);
        }

        /// <summary>
        /// Creates a shallow copy of this tensor, with new backing storage.
        /// </summary>
        /// <returns>A shallow copy of this tensor.</returns>
        public override Tensor<T> Clone()
        {
            return new DenseTensor<T>(Buffer.ToArray(), dimensions, base.IsReversedStride);
        }

        /// <summary>
        /// Creates a new Tensor of a different type with the specified dimensions and the same layout as this tensor with elements initialized to their default value.
        /// </summary>
        /// <typeparam name="TResult">Type contained in the returned Tensor.</typeparam>
        /// <param name="dimensions">An span of integers that represent the size of each dimension of the DenseTensor to create.</param>
        /// <returns>A new tensor with the same layout as this tensor but different type and dimensions.</returns>
        public override Tensor<TResult> CloneEmpty<TResult>(ReadOnlySpan<int> dimensions)
        {
            return new DenseTensor<TResult>(dimensions, base.IsReversedStride);
        }

        /// <summary>
        /// Reshapes the current tensor to new dimensions, using the same backing storage.
        /// </summary>
        /// <param name="dimensions">An span of integers that represent the size of each dimension of the DenseTensor to create.</param>
        /// <returns>A new tensor that reinterprets backing Buffer of this tensor with different dimensions.</returns>
        public override Tensor<T> Reshape(ReadOnlySpan<int> dimensions)
        {
            if (dimensions.Length == 0)
            {
                throw new ArgumentException(MDCFR.Properties.Resources.DimensionsMustContainElements, "dimensions");
            }
            long product = ArrayUtilities.GetProduct(dimensions);
            if (product != base.Length)
            {
                throw new ArgumentException(System.SR.Format(MDCFR.Properties.Resources.CannotReshapeArrayDueToMismatchInLengths, base.Length, product), "dimensions");
            }
            return new DenseTensor<T>(Buffer, dimensions, base.IsReversedStride);
        }
    }

    /// <summary>
	/// Represents a multi-dimensional collection of objects of type T that can be accessed by indices.  Unlike other Tensor&lt;T&gt; implementations SparseTensor&lt;T&gt; does not expose its backing storage.  It is meant as an intermediate to be used to build other Tensors, such as CompressedSparseTensor.  Unlike CompressedSparseTensor where insertions are O(n), insertions to SparseTensor&lt;T&gt; are nominally O(1).
	/// </summary>
	/// <typeparam name="T">type contained within the Tensor.  Typically a value type such as int, double, float, etc.</typeparam>
	public class SparseTensor<T> : Tensor<T>
    {
        private readonly Dictionary<int, T> values;

        /// <summary>
        /// Get's the number on non-zero values currently being stored in this tensor.
        /// </summary>
        public int NonZeroCount => values.Count;

        /// <summary>
        /// Constructs a new SparseTensor of the specified dimensions, initial capacity, and stride ordering.
        /// </summary>
        /// <param name="dimensions">An span of integers that represent the size of each dimension of the SparseTensor to create.</param>
        /// <param name="reverseStride">False (default) to indicate that the first dimension is most major (farthest apart) and the last dimension is most minor (closest together): akin to row-major in a rank-2 tensor.  True to indicate that the last dimension is most major (farthest apart) and the first dimension is most minor (closest together): akin to column-major in a rank-2 tensor.</param>
        /// <param name="capacity">The number of non-zero values this tensor can store without resizing.</param>
        public SparseTensor(ReadOnlySpan<int> dimensions, bool reverseStride = false, int capacity = 0)
            : base(dimensions, reverseStride)
        {
            values = new Dictionary<int, T>(capacity);
        }

        internal SparseTensor(Dictionary<int, T> values, ReadOnlySpan<int> dimensions, bool reverseStride = false)
            : base(dimensions, reverseStride)
        {
            this.values = values;
        }

        internal SparseTensor(Array fromArray, bool reverseStride = false)
            : base(fromArray, reverseStride)
        {
            values = new Dictionary<int, T>(fromArray.Length);
            int num = 0;
            if (reverseStride)
            {
                int[] sourceStrides = ArrayUtilities.GetStrides(dimensions);
                {
                    foreach (T item in fromArray)
                    {
                        if (!item.Equals(Tensor<T>.Zero))
                        {
                            int key = ArrayUtilities.TransformIndexByStrides(num, sourceStrides, sourceReverseStride: false, strides);
                            values[key] = item;
                        }
                        num++;
                    }
                    return;
                }
            }
            foreach (T item2 in fromArray)
            {
                if (!item2.Equals(Tensor<T>.Zero))
                {
                    values[num] = item2;
                }
                num++;
            }
        }

        /// <summary>
        /// Gets the value at the specified index, where index is a linearized version of n-dimension indices using strides.
        /// </summary>
        /// <param name="index">An integer index computed as a dot-product of indices.</param>
        /// <returns>The value at the specified position in this Tensor.</returns>
        public override T GetValue(int index)
        {
            if (!values.TryGetValue(index, out var value))
            {
                return Tensor<T>.Zero;
            }
            return value;
        }

        /// <summary>
        /// Sets the value at the specified index, where index is a linearized version of n-dimension indices using strides.
        /// </summary>
        /// <param name="index">An integer index computed as a dot-product of indices.</param>
        /// <param name="value">The new value to set at the specified position in this Tensor.</param>
        public override void SetValue(int index, T value)
        {
            if (value.Equals(Tensor<T>.Zero))
            {
                values.Remove(index);
            }
            else
            {
                values[index] = value;
            }
        }

        /// <summary>
        /// Creates a shallow copy of this tensor, with new backing storage.
        /// </summary>
        /// <returns>A shallow copy of this tensor.</returns>
        public override Tensor<T> Clone()
        {
            Dictionary<int, T> dictionary = new Dictionary<int, T>(values);
            return new SparseTensor<T>(dictionary, dimensions, base.IsReversedStride);
        }

        /// <summary>
        /// Creates a new Tensor of a different type with the specified dimensions and the same layout as this tensor with elements initialized to their default value.
        /// </summary>
        /// <typeparam name="TResult">Type contained in the returned Tensor.</typeparam>
        /// <param name="dimensions">An span of integers that represent the size of each dimension of the SparseTensor to create.</param>
        /// <returns>A new tensor with the same layout as this tensor but different type and dimensions.</returns>
        public override Tensor<TResult> CloneEmpty<TResult>(ReadOnlySpan<int> dimensions)
        {
            return new SparseTensor<TResult>(dimensions, base.IsReversedStride);
        }

        /// <summary>
        /// Reshapes the current tensor to new dimensions, using the same backing storage.
        /// </summary>
        /// <param name="dimensions">An span of integers that represent the size of each dimension of the SparseTensor to create.</param>
        /// <returns>A new tensor that reinterprets backing storage of this tensor with different dimensions.</returns>
        public override Tensor<T> Reshape(ReadOnlySpan<int> dimensions)
        {
            return new SparseTensor<T>(values, dimensions, base.IsReversedStride);
        }

        /// <summary>
        /// Creates a copy of this tensor as a DenseTensor&lt;T&gt;.
        /// </summary>
        /// <returns>A copy of this tensor as a DenseTensor&lt;T&gt;</returns>
        public override DenseTensor<T> ToDenseTensor()
        {
            DenseTensor<T> denseTensor = new DenseTensor<T>(base.Dimensions, base.IsReversedStride);
            foreach (KeyValuePair<int, T> value in values)
            {
                denseTensor.SetValue(value.Key, value.Value);
            }
            return denseTensor;
        }

        /// <summary>
        /// Creates a copy of this tensor as a new SparseTensor&lt;T&gt; eliminating any unused space in the backing storage.
        /// </summary>
        /// <returns>A copy of this tensor as a SparseTensor&lt;T&gt; eliminated any usused space in the backing storage.</returns>
        public override SparseTensor<T> ToSparseTensor()
        {
            Dictionary<int, T> dictionary = new Dictionary<int, T>(values);
            return new SparseTensor<T>(dictionary, dimensions, base.IsReversedStride);
        }

        /// <summary>
        /// Creates a copy of this tensor as a CompressedSparseTensor&lt;T&gt;.
        /// </summary>
        /// <returns>A copy of this tensor as a CompressedSparseTensor&lt;T&gt;.</returns>
        public override CompressedSparseTensor<T> ToCompressedSparseTensor()
        {
            CompressedSparseTensor<T> compressedSparseTensor = new CompressedSparseTensor<T>(dimensions, NonZeroCount, base.IsReversedStride);
            foreach (KeyValuePair<int, T> value in values)
            {
                compressedSparseTensor.SetValue(value.Key, value.Value);
            }
            return compressedSparseTensor;
        }
    }

}