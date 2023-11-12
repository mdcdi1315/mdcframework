// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.Versioning;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

#nullable enable
#pragma warning disable CS8767 , CS8769
#pragma warning disable CS8600 , CS8602 , CS8604 , CS8601 , CS8603

namespace System.Collections.Immutable
{

    internal static class AllocFreeConcurrentStack<T>
    {
        private const int MaxSize = 35;

        private static readonly Type s_typeOfT = typeof(T);

        private static Stack<RefAsValueType<T>> ThreadLocalStack
        {
            get
            {
                Dictionary<Type, object> dictionary = AllocFreeConcurrentStack.t_stacks ?? (AllocFreeConcurrentStack.t_stacks = new Dictionary<Type, object>());
                if (!dictionary.TryGetValue(s_typeOfT, out var value))
                {
                    value = new Stack<RefAsValueType<T>>(35);
                    dictionary.Add(s_typeOfT, value);
                }
                return (Stack<RefAsValueType<T>>)value;
            }
        }

        public static void TryAdd(T item)
        {
            Stack<RefAsValueType<T>> threadLocalStack = ThreadLocalStack;
            if (threadLocalStack.Count < 35)
            {
                threadLocalStack.Push(new RefAsValueType<T>(item));
            }
        }

        public static bool TryTake([MaybeNullWhen(false)] out T item)
        {
            Stack<RefAsValueType<T>> threadLocalStack = ThreadLocalStack;
            if (threadLocalStack != null && threadLocalStack.Count > 0)
            {
                item = threadLocalStack.Pop().Value;
                return true;
            }
            item = default(T);
            return false;
        }
    }
    
    internal static class AllocFreeConcurrentStack
    {
        [ThreadStatic]
        internal static Dictionary<Type, object>? t_stacks;
    }

    internal sealed class DictionaryEnumerator<TKey, TValue> : IDictionaryEnumerator, IEnumerator where TKey : notnull
    {
        private readonly IEnumerator<KeyValuePair<TKey, TValue>> _inner;

        public DictionaryEntry Entry => new DictionaryEntry(_inner.Current.Key, _inner.Current.Value);

        public object Key => _inner.Current.Key;

        public object? Value => _inner.Current.Value;

        public object Current => Entry;

        internal DictionaryEnumerator(IEnumerator<KeyValuePair<TKey, TValue>> inner)
        {
            Requires.NotNull(inner, "inner");
            _inner = inner;
        }

        public bool MoveNext()
        {
            return _inner.MoveNext();
        }

        public void Reset()
        {
            _inner.Reset();
        }
    }

    internal struct DisposableEnumeratorAdapter<T, TEnumerator> : IDisposable where TEnumerator : struct, IEnumerator<T>
    {
        private readonly IEnumerator<T> _enumeratorObject;

        private TEnumerator _enumeratorStruct;

        public T Current
        {
            get
            {
                if (_enumeratorObject == null)
                {
                    return _enumeratorStruct.Current;
                }
                return _enumeratorObject.Current;
            }
        }

        #pragma warning disable CS8618 , CS8625
        internal DisposableEnumeratorAdapter(TEnumerator enumerator)
        {
            _enumeratorStruct = enumerator;
            _enumeratorObject = null;
        }
        #pragma warning restore CS8618, CS8625

        internal DisposableEnumeratorAdapter(IEnumerator<T> enumerator)
        {
            _enumeratorStruct = default(TEnumerator);
            _enumeratorObject = enumerator;
        }

        public bool MoveNext()
        {
            if (_enumeratorObject == null)
            {
                return _enumeratorStruct.MoveNext();
            }
            return _enumeratorObject.MoveNext();
        }

        public void Dispose()
        {
            if (_enumeratorObject != null)
            {
                _enumeratorObject.Dispose();
            }
            else
            {
                _enumeratorStruct.Dispose();
            }
        }

        public DisposableEnumeratorAdapter<T, TEnumerator> GetEnumerator()
        {
            return this;
        }
    }

    internal interface IBinaryTree
    {
        int Height { get; }

        bool IsEmpty { get; }

        int Count { get; }

        IBinaryTree? Left { get; }

        IBinaryTree? Right { get; }
    }
   
    internal interface IBinaryTree<out T> : IBinaryTree
    {
        T Value { get; }

        new IBinaryTree<T>? Left { get; }

        new IBinaryTree<T>? Right { get; }
    }

    internal interface IImmutableArray
    {
        Array? Array { get; }
    }

    /// <summary>Represents an immutable collection of key/value pairs.  </summary>
    /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
    public interface IImmutableDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>, IReadOnlyCollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
    {
        /// <summary>Retrieves an empty dictionary that has the same ordering and key/value comparison rules as this dictionary instance.</summary>
        /// <returns>An empty dictionary with equivalent ordering and key/value comparison rules.</returns>
        IImmutableDictionary<TKey, TValue> Clear();

        /// <summary>Adds an element with the specified key and value to the dictionary.</summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add.</param>
        /// <exception cref="T:System.ArgumentException">The given key already exists in the dictionary but has a different value.</exception>
        /// <returns>A new immutable dictionary that contains the additional key/value pair.</returns>
        IImmutableDictionary<TKey, TValue> Add(TKey key, TValue value);

        /// <summary>Adds the specified key/value pairs to the dictionary.</summary>
        /// <param name="pairs">The key/value pairs to add.</param>
        /// <exception cref="T:System.ArgumentException">One of the given keys already exists in the dictionary but has a different value.</exception>
        /// <returns>A new immutable dictionary that contains the additional key/value pairs.</returns>
        IImmutableDictionary<TKey, TValue> AddRange(IEnumerable<KeyValuePair<TKey, TValue>> pairs);

        /// <summary>Sets the specified key and value in the immutable dictionary, possibly overwriting an existing value for the key.</summary>
        /// <param name="key">The key of the entry to add.</param>
        /// <param name="value">The key value to set.</param>
        /// <returns>A new immutable dictionary that contains the specified key/value pair.</returns>
        IImmutableDictionary<TKey, TValue> SetItem(TKey key, TValue value);

        /// <summary>Sets the specified key/value pairs in the immutable dictionary, possibly overwriting existing values for the keys.</summary>
        /// <param name="items">The key/value pairs to set in the dictionary. If any of the keys already exist in the dictionary, this method will overwrite their previous values.</param>
        /// <returns>A new immutable dictionary that contains the specified key/value pairs.</returns>
        IImmutableDictionary<TKey, TValue> SetItems(IEnumerable<KeyValuePair<TKey, TValue>> items);

        /// <summary>Removes the elements with the specified keys from the immutable dictionary.</summary>
        /// <param name="keys">The keys of the elements to remove.</param>
        /// <returns>A new immutable dictionary with the specified keys removed; or this instance if the specified keys cannot be found in the dictionary.</returns>
        IImmutableDictionary<TKey, TValue> RemoveRange(IEnumerable<TKey> keys);

        /// <summary>Removes the element with the specified key from the immutable dictionary.</summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>A new immutable dictionary with the specified element removed; or this instance if the specified key cannot be found in the dictionary.</returns>
        IImmutableDictionary<TKey, TValue> Remove(TKey key);

        /// <summary>Determines whether the immutable dictionary contains the specified key/value pair.</summary>
        /// <param name="pair">The key/value pair to locate.</param>
        /// <returns>
        ///   <see langword="true" /> if the specified key/value pair is found in the dictionary; otherwise, <see langword="false" />.</returns>
        bool Contains(KeyValuePair<TKey, TValue> pair);

        /// <summary>Determines whether this dictionary contains a specified key.</summary>
        /// <param name="equalKey">The key to search for.</param>
        /// <param name="actualKey">The matching key located in the dictionary if found, or <c>equalkey</c> if no match is found.</param>
        /// <returns>
        ///   <see langword="true" /> if a match for <paramref name="equalKey" /> is found; otherwise, <see langword="false" />.</returns>
        bool TryGetKey(TKey equalKey, out TKey actualKey);
    }

    internal interface IImmutableDictionaryInternal<TKey, TValue>
    {
        bool ContainsValue(TValue value);
    }

    /// <summary>Represents a list of elements that cannot be modified. </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    public interface IImmutableList<T> : IReadOnlyList<T>, IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable
    {
        /// <summary>Creates  a list with all the items removed, but with the same sorting and ordering semantics as this list.</summary>
        /// <returns>An empty list that has the same sorting and ordering semantics as this instance.</returns>
        IImmutableList<T> Clear();

        /// <summary>Searches for the specified object and returns the zero-based index of the first occurrence within the range of elements in the <see cref="T:System.Collections.Immutable.IImmutableList`1" /> that starts at the specified index and contains the specified number of elements.</summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Immutable.IImmutableList`1" />. This value can be null for reference types.</param>
        /// <param name="index">The zero-based starting indexes of the search. 0 (zero) is valid in an empty list.</param>
        /// <param name="count">The number of elements in the section to search.</param>
        /// <param name="equalityComparer">The equality comparer to use to locate <paramref name="item" />.</param>
        /// <returns>The zero-based index of the first occurrence of <paramref name="item" /> within the range of elements in the <see cref="T:System.Collections.Immutable.IImmutableList`1" /> that starts at <paramref name="index" /> and contains <paramref name="count" /> number of elements if found; otherwise -1.</returns>
        int IndexOf(T item, int index, int count, IEqualityComparer<T>? equalityComparer);

        /// <summary>Searches for the specified object and returns the zero-based index of the last occurrence within the range of elements in the <see cref="T:System.Collections.Immutable.IImmutableList`1" /> that contains the specified number of elements and ends at the specified index.</summary>
        /// <param name="item">The object to locate in the list. The value can be <see langword="null" /> for reference types.</param>
        /// <param name="index">The zero-based starting index of the search. 0 (zero) is valid in an empty list.</param>
        /// <param name="count">The number of elements in the section to search.</param>
        /// <param name="equalityComparer">The equality comparer to match <paramref name="item" />.</param>
        /// <returns>Returns <see cref="T:System.Int32" />.</returns>
        int LastIndexOf(T item, int index, int count, IEqualityComparer<T>? equalityComparer);

        /// <summary>Makes a copy of the list, and adds the specified object to the end of the copied list.</summary>
        /// <param name="value">The object to add to the list.</param>
        /// <returns>A new list with the object added.</returns>
        IImmutableList<T> Add(T value);

        /// <summary>Makes a copy of the list and adds the specified objects to the end of the copied list.</summary>
        /// <param name="items">The objects to add to the list.</param>
        /// <returns>A new list with the elements added.</returns>
        IImmutableList<T> AddRange(IEnumerable<T> items);

        /// <summary>Inserts the specified element at the specified index in the immutable list.</summary>
        /// <param name="index">The zero-based index at which to insert the value.</param>
        /// <param name="element">The object to insert.</param>
        /// <returns>A new immutable list that includes the specified element.</returns>
        IImmutableList<T> Insert(int index, T element);

        /// <summary>Inserts the specified elements at the specified index in the immutable list.</summary>
        /// <param name="index">The zero-based index at which the new elements should be inserted.</param>
        /// <param name="items">The elements to insert.</param>
        /// <returns>A new immutable list that includes the specified elements.</returns>
        IImmutableList<T> InsertRange(int index, IEnumerable<T> items);

        /// <summary>Removes the first occurrence of a specified object from this immutable list.</summary>
        /// <param name="value">The object to remove from the list.</param>
        /// <param name="equalityComparer">The equality comparer to use to locate <paramref name="value" />.</param>
        /// <returns>A new list with the specified object removed.</returns>
        IImmutableList<T> Remove(T value, IEqualityComparer<T>? equalityComparer);

        /// <summary>Removes all the elements that match the conditions defined by the specified predicate.</summary>
        /// <param name="match">The delegate that defines the conditions of the elements to remove.</param>
        /// <returns>A new immutable list with the elements removed.</returns>
        IImmutableList<T> RemoveAll(Predicate<T> match);

        /// <summary>Removes the specified object from the list.</summary>
        /// <param name="items">The objects to remove from the list.</param>
        /// <param name="equalityComparer">The equality comparer to use to determine if <paramref name="items" /> match any objects in the list.</param>
        /// <returns>A new immutable list with the specified objects removed, if <paramref name="items" /> matched objects in the list.</returns>
        IImmutableList<T> RemoveRange(IEnumerable<T> items, IEqualityComparer<T>? equalityComparer);

        /// <summary>Removes a range of elements from the <see cref="T:System.Collections.Immutable.IImmutableList`1" />.</summary>
        /// <param name="index">The zero-based starting index of the range of elements to remove.</param>
        /// <param name="count">The number of elements to remove.</param>
        /// <returns>A new immutable list with the elements removed.</returns>
        IImmutableList<T> RemoveRange(int index, int count);

        /// <summary>Removes the element at the specified index of the immutable list.</summary>
        /// <param name="index">The index of the element to remove.</param>
        /// <returns>A new list with the element removed.</returns>
        IImmutableList<T> RemoveAt(int index);

        /// <summary>Replaces an element in the list at a given position with the specified element.</summary>
        /// <param name="index">The position in the list of the element to replace.</param>
        /// <param name="value">The element to replace the old element with.</param>
        /// <returns>A new list that contains the new element, even if the element at the specified location is the same as the new element.</returns>
        IImmutableList<T> SetItem(int index, T value);

        /// <summary>Returns a new list with the first matching element in the list replaced with the specified element.</summary>
        /// <param name="oldValue">The element to be replaced.</param>
        /// <param name="newValue">The element to replace the first occurrence of <paramref name="oldValue" /> with.</param>
        /// <param name="equalityComparer">The equality comparer to use for matching <paramref name="oldValue" />.</param>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="oldValue" /> does not exist in the list.</exception>
        /// <returns>A new list that contains <paramref name="newValue" />, even if <paramref name="oldValue" /> is the same as <paramref name="newValue" />.</returns>
        IImmutableList<T> Replace(T oldValue, T newValue, IEqualityComparer<T>? equalityComparer);
    }

    internal interface IImmutableListQueries<T> : IReadOnlyList<T>, IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable
    {
        ImmutableList<TOutput> ConvertAll<TOutput>(Func<T, TOutput> converter);

        void ForEach(Action<T> action);

        ImmutableList<T> GetRange(int index, int count);

        void CopyTo(T[] array);

        void CopyTo(T[] array, int arrayIndex);

        void CopyTo(int index, T[] array, int arrayIndex, int count);

        bool Exists(Predicate<T> match);

        T? Find(Predicate<T> match);

        ImmutableList<T> FindAll(Predicate<T> match);

        int FindIndex(Predicate<T> match);

        int FindIndex(int startIndex, Predicate<T> match);

        int FindIndex(int startIndex, int count, Predicate<T> match);

        T? FindLast(Predicate<T> match);

        int FindLastIndex(Predicate<T> match);

        int FindLastIndex(int startIndex, Predicate<T> match);

        int FindLastIndex(int startIndex, int count, Predicate<T> match);

        bool TrueForAll(Predicate<T> match);

        int BinarySearch(T item);

        int BinarySearch(T item, IComparer<T>? comparer);

        int BinarySearch(int index, int count, T item, IComparer<T>? comparer);
    }

    /// <summary>Represents an immutable first-in, first-out collection of objects. </summary>
    /// <typeparam name="T">The type of elements in the queue.</typeparam>
    public interface IImmutableQueue<T> : IEnumerable<T>, IEnumerable
    {
        /// <summary>Gets a value that indicates whether this immutable queue is empty.</summary>
        /// <returns>
        ///   <see langword="true" /> if this queue is empty; otherwise, <see langword="false" />.</returns>
        bool IsEmpty { get; }

        /// <summary>Returns a new queue with all the elements removed.</summary>
        /// <returns>An empty immutable queue.</returns>
        IImmutableQueue<T> Clear();

        /// <summary>Returns the element at the beginning of the immutable queue without removing it.</summary>
        /// <exception cref="T:System.InvalidOperationException">The queue is empty.</exception>
        /// <returns>The element at the beginning of the queue.</returns>
        T Peek();

        /// <summary>Adds an element to the end of the immutable queue, and returns the new queue.</summary>
        /// <param name="value">The element to add.</param>
        /// <returns>The new immutable queue with the specified element added.</returns>
        IImmutableQueue<T> Enqueue(T value);

        /// <summary>Removes the first element in the immutable queue, and returns the new queue.</summary>
        /// <exception cref="T:System.InvalidOperationException">The queue is empty.</exception>
        /// <returns>The new immutable queue with the first element removed. This value is never <see langword="null" />.</returns>
        IImmutableQueue<T> Dequeue();
    }

    /// <summary>Represents a set of elements that can only be modified by creating a new instance of the set. </summary>
    /// <typeparam name="T">The type of element stored in the set.</typeparam>
    public interface IImmutableSet<T> : IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable
    {
        /// <summary>Retrieves an empty immutable set that has the same sorting and ordering semantics as this instance.</summary>
        /// <returns>An empty set that has the same sorting and ordering semantics as this instance.</returns>
        IImmutableSet<T> Clear();

        /// <summary>Determines whether this immutable set contains a specified element.</summary>
        /// <param name="value">The element to locate in the set.</param>
        /// <returns>
        ///   <see langword="true" /> if the set contains the specified value; otherwise, <see langword="false" />.</returns>
        bool Contains(T value);

        /// <summary>Adds the specified element to this immutable set.</summary>
        /// <param name="value">The element to add.</param>
        /// <returns>A new set with the element added, or this set if the element is already in the set.</returns>
        IImmutableSet<T> Add(T value);

        /// <summary>Removes the specified element from this immutable set.</summary>
        /// <param name="value">The element to remove.</param>
        /// <returns>A new set with the specified element removed, or the current set if the element cannot be found in the set.</returns>
        IImmutableSet<T> Remove(T value);

        /// <summary>Determines whether the set contains a specified value.</summary>
        /// <param name="equalValue">The value to search for.</param>
        /// <param name="actualValue">The matching value from the set, if found, or <c>equalvalue</c> if there are no matches.</param>
        /// <returns>
        ///   <see langword="true" /> if a matching value was found; otherwise, <see langword="false" />.</returns>
        bool TryGetValue(T equalValue, out T actualValue);

        /// <summary>Creates an immutable set that contains only elements that exist in this set and the specified set.</summary>
        /// <param name="other">The collection to compare to the current <see cref="T:System.Collections.Immutable.IImmutableSet`1" />.</param>
        /// <returns>A new immutable set that contains elements that exist in both sets.</returns>
        IImmutableSet<T> Intersect(IEnumerable<T> other);

        /// <summary>Removes the elements in the specified collection from the current immutable set.</summary>
        /// <param name="other">The collection of items to remove from this set.</param>
        /// <returns>A new set with the items removed; or the original set if none of the items were in the set.</returns>
        IImmutableSet<T> Except(IEnumerable<T> other);

        /// <summary>Creates an immutable set that contains only elements that are present either in the current set or in the specified collection, but not both.</summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>A new set that contains the elements that are present only in the current set or in the specified collection, but not both.</returns>
        IImmutableSet<T> SymmetricExcept(IEnumerable<T> other);

        /// <summary>Creates a new immutable set that contains all elements that are present in either the current set or in the specified collection.</summary>
        /// <param name="other">The collection to add elements from.</param>
        /// <returns>A new immutable set with the items added; or the original set if all the items were already in the set.</returns>
        IImmutableSet<T> Union(IEnumerable<T> other);

        /// <summary>Determines whether the current immutable set and the specified collection contain the same elements.</summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>
        ///   <see langword="true" /> if the sets are equal; otherwise, <see langword="false" />.</returns>
        bool SetEquals(IEnumerable<T> other);

        /// <summary>Determines whether the current immutable set is a proper (strict) subset of the specified collection.</summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>
        ///   <see langword="true" /> if the current set is a proper subset of the specified collection; otherwise, <see langword="false" />.</returns>
        bool IsProperSubsetOf(IEnumerable<T> other);

        /// <summary>Determines whether the current immutable set is a proper (strict) superset of the specified collection.</summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>
        ///   <see langword="true" /> if the current set is a proper superset of the specified collection; otherwise, <see langword="false" />.</returns>
        bool IsProperSupersetOf(IEnumerable<T> other);

        /// <summary>Determines whether the current immutable set is a subset of a specified collection.</summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>
        ///   <see langword="true" /> if the current set is a subset of the specified collection; otherwise, <see langword="false" />.</returns>
        bool IsSubsetOf(IEnumerable<T> other);

        /// <summary>Determines whether the current immutable set is a superset of a specified collection.</summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>
        ///   <see langword="true" /> if the current set is a superset of the specified collection; otherwise, <see langword="false" />.</returns>
        bool IsSupersetOf(IEnumerable<T> other);

        /// <summary>Determines whether the current immutable set overlaps with the specified collection.</summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>
        ///   <see langword="true" /> if the current set and the specified collection share at least one common element; otherwise, <see langword="false" />.</returns>
        bool Overlaps(IEnumerable<T> other);
    }

    /// <summary>Represents an immutable last-in-first-out (LIFO) collection. </summary>
    /// <typeparam name="T">The type of elements in the stack.</typeparam>
    public interface IImmutableStack<T> : IEnumerable<T>, IEnumerable
    {
        /// <summary>Gets a value that indicates whether this immutable stack is empty.</summary>
        /// <returns>
        ///   <see langword="true" /> if this stack is empty; otherwise,<see langword="false" />.</returns>
        bool IsEmpty { get; }

        /// <summary>Removes all objects from the immutable stack.</summary>
        /// <returns>An empty immutable stack.</returns>
        IImmutableStack<T> Clear();

        /// <summary>Inserts an element at the top of the immutable stack and returns the new stack.</summary>
        /// <param name="value">The element to push onto the stack.</param>
        /// <returns>The new stack.</returns>
        IImmutableStack<T> Push(T value);

        /// <summary>Removes the element at the top of the immutable stack and returns the new stack.</summary>
        /// <exception cref="T:System.InvalidOperationException">The stack is empty.</exception>
        /// <returns>The new stack; never <see langword="null" />.</returns>
        IImmutableStack<T> Pop();

        /// <summary>Returns the element at the top of the immutable stack without removing it.</summary>
        /// <exception cref="T:System.InvalidOperationException">The stack is empty.</exception>
        /// <returns>The element at the top of the stack.</returns>
        T Peek();
    }

    /// <summary>Provides methods for creating an array that is immutable; meaning it cannot be changed once it is created. </summary>
    public static class ImmutableArray
    {
        internal static readonly byte[] TwoElementArray = new byte[2];

        /// <summary>Creates an empty immutable array.</summary>
        /// <typeparam name="T">The type of elements stored in the array.</typeparam>
        /// <returns>An empty immutable array.</returns>
        public static ImmutableArray<T> Create<T>()
        {
            return ImmutableArray<T>.Empty;
        }

        /// <summary>Creates an immutable array that contains the specified object.</summary>
        /// <param name="item">The object to store in the array.</param>
        /// <typeparam name="T">The type of elements stored in the array.</typeparam>
        /// <returns>An immutable array that contains the specified object.</returns>
        public static ImmutableArray<T> Create<T>(T item)
        {
            T[] items = new T[1] { item };
            return new ImmutableArray<T>(items);
        }

        /// <summary>Creates an immutable array that contains the specified objects.</summary>
        /// <param name="item1">The first object to store in the array.</param>
        /// <param name="item2">The second object to store in the array.</param>
        /// <typeparam name="T">The type of elements stored in the array.</typeparam>
        /// <returns>An immutable array that contains the specified objects.</returns>
        public static ImmutableArray<T> Create<T>(T item1, T item2)
        {
            T[] items = new T[2] { item1, item2 };
            return new ImmutableArray<T>(items);
        }

        /// <summary>Creates an immutable array that contains the specified objects.</summary>
        /// <param name="item1">The first object to store in the array.</param>
        /// <param name="item2">The second object to store in the array.</param>
        /// <param name="item3">The third object to store in the array.</param>
        /// <typeparam name="T">The type of elements stored in the array.</typeparam>
        /// <returns>An immutable array that contains the specified objects.</returns>
        public static ImmutableArray<T> Create<T>(T item1, T item2, T item3)
        {
            T[] items = new T[3] { item1, item2, item3 };
            return new ImmutableArray<T>(items);
        }

        /// <summary>Creates an immutable array that contains the specified objects.</summary>
        /// <param name="item1">The first object to store in the array.</param>
        /// <param name="item2">The second object to store in the array.</param>
        /// <param name="item3">The third object to store in the array.</param>
        /// <param name="item4">The fourth object to store in the array.</param>
        /// <typeparam name="T">The type of elements stored in the array.</typeparam>
        /// <returns>An immutable array that contains the specified objects.</returns>
        public static ImmutableArray<T> Create<T>(T item1, T item2, T item3, T item4)
        {
            T[] items = new T[4] { item1, item2, item3, item4 };
            return new ImmutableArray<T>(items);
        }

        /// <summary>Creates an <see cref="T:System.Collections.Immutable.ImmutableArray`1" /> with the specified elements.</summary>
        /// <param name="items">The elements to store in the array.</param>
        /// <typeparam name="T">The type of element stored in the array.</typeparam>
        /// <returns>An immutable array containing the specified items.</returns>
        public static ImmutableArray<T> Create<T>(ReadOnlySpan<T> items)
        {
            if (items.IsEmpty)
            {
                return ImmutableArray<T>.Empty;
            }
            T[] items2 = items.ToArray();
            return new ImmutableArray<T>(items2);
        }

        /// <summary>Creates an <see cref="T:System.Collections.Immutable.ImmutableArray`1" /> with the specified elements.</summary>
        /// <param name="items">The elements to store in the array.</param>
        /// <typeparam name="T">The type of element stored in the array.</typeparam>
        /// <returns>An immutable array containing the specified items.</returns>
        public static ImmutableArray<T> Create<T>(Span<T> items)
        {
            return Create((ReadOnlySpan<T>)items);
        }

        /// <summary>Produce an immutable array of contents from specified elements.</summary>
        /// <param name="items">The elements to store in the array.</param>
        /// <typeparam name="T">The type of element in the list.</typeparam>
        /// <returns>An immutable array containing the items in the span.</returns>
        public static ImmutableArray<T> ToImmutableArray<T>(this ReadOnlySpan<T> items)
        {
            return Create(items);
        }

        /// <summary>Converts the span to an immutable array.</summary>
        /// <param name="items">The elements to store in the array.</param>
        /// <typeparam name="T">The type of element in the list.</typeparam>
        /// <returns>An immutable array containing the items in the span.</returns>
        public static ImmutableArray<T> ToImmutableArray<T>(this Span<T> items)
        {
            return Create((ReadOnlySpan<T>)items);
        }

        /// <summary>Creates a new <see cref="T:System.Collections.Immutable.ImmutableArray`1" /> populated with the specified items.</summary>
        /// <param name="items">The elements to add to the array.</param>
        /// <typeparam name="T">The type of element stored in the array.</typeparam>
        /// <returns>An immutable array that contains the specified items.</returns>
        public static ImmutableArray<T> CreateRange<T>(IEnumerable<T> items)
        {
            Requires.NotNull(items, "items");
            if (items is IImmutableArray immutableArray)
            {
                Array array = immutableArray.Array;
                if (array == null)
                {
                    throw new InvalidOperationException(MDCFR.Properties.Resources.InvalidOperationOnDefaultArray);
                }
                return new ImmutableArray<T>((T[])array);
            }
            if (items.TryGetCount(out var count))
            {
                return new ImmutableArray<T>(items.ToArray(count));
            }
            return new ImmutableArray<T>(items.ToArray());
        }

        /// <summary>Creates an immutable array from the specified array of objects.</summary>
        /// <param name="items">The array of objects to populate the array with.</param>
        /// <typeparam name="T">The type of elements stored in the array.</typeparam>
        /// <returns>An immutable array that contains the array of items.</returns>
        public static ImmutableArray<T> Create<T>(params T[]? items)
        {
            if (items == null || items.Length == 0)
            {
                return ImmutableArray<T>.Empty;
            }
            T[] array = new T[items.Length];
            Array.Copy(items, array, items.Length);
            return new ImmutableArray<T>(array);
        }

        /// <summary>Creates an immutable array with specified objects from another array.</summary>
        /// <param name="items">The source array of objects.</param>
        /// <param name="start">The index of the first element to copy from <paramref name="items" />.</param>
        /// <param name="length">The number of elements from <paramref name="items" /> to include in this immutable array.</param>
        /// <typeparam name="T">The type of elements stored in the array.</typeparam>
        /// <returns>An immutable array that contains the specified objects from the source array.</returns>
        public static ImmutableArray<T> Create<T>(T[] items, int start, int length)
        {
            Requires.NotNull(items, "items");
            Requires.Range(start >= 0 && start <= items.Length, "start");
            Requires.Range(length >= 0 && start + length <= items.Length, "length");
            if (length == 0)
            {
                return Create<T>();
            }
            T[] array = new T[length];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = items[start + i];
            }
            return new ImmutableArray<T>(array);
        }

        /// <summary>Creates an immutable array with the specified objects from another immutable array.</summary>
        /// <param name="items">The source array of objects.</param>
        /// <param name="start">The index of the first element to copy from <paramref name="items" />.</param>
        /// <param name="length">The number of elements from <paramref name="items" /> to include in this immutable array.</param>
        /// <typeparam name="T">The type of elements stored in the array.</typeparam>
        /// <returns>An immutable array that contains the specified objects from the source array.</returns>
        public static ImmutableArray<T> Create<T>(ImmutableArray<T> items, int start, int length)
        {
            Requires.Range(start >= 0 && start <= items.Length, "start");
            Requires.Range(length >= 0 && start + length <= items.Length, "length");
            if (length == 0)
            {
                return Create<T>();
            }
            if (start == 0 && length == items.Length)
            {
                return items;
            }
            T[] array = new T[length];
            Array.Copy(items.array, start, array, 0, length);
            return new ImmutableArray<T>(array);
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Collections.Immutable.ImmutableArray`1" /> struct.</summary>
        /// <param name="items">The source array to initialize the resulting array with.</param>
        /// <param name="selector">The function to apply to each element from the source array.</param>
        /// <typeparam name="TSource">The type of element stored in the source array.</typeparam>
        /// <typeparam name="TResult">The type of element to store in the target array.</typeparam>
        /// <returns>An immutable array that contains the specified items.</returns>
        public static ImmutableArray<TResult> CreateRange<TSource, TResult>(ImmutableArray<TSource> items, Func<TSource, TResult> selector)
        {
            Requires.NotNull(selector, "selector");
            int length = items.Length;
            if (length == 0)
            {
                return Create<TResult>();
            }
            TResult[] array = new TResult[length];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = selector(items[i]);
            }
            return new ImmutableArray<TResult>(array);
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Collections.Immutable.ImmutableArray`1" /> struct.</summary>
        /// <param name="items">The source array to initialize the resulting array with.</param>
        /// <param name="start">The index of the first element in the source array to include in the resulting array.</param>
        /// <param name="length">The number of elements from the source array to include in the resulting array.</param>
        /// <param name="selector">The function to apply to each element from the source array included in the resulting array.</param>
        /// <typeparam name="TSource">The type of element stored in the source array.</typeparam>
        /// <typeparam name="TResult">The type of element to store in the target array.</typeparam>
        /// <returns>An immutable array that contains the specified items.</returns>
        public static ImmutableArray<TResult> CreateRange<TSource, TResult>(ImmutableArray<TSource> items, int start, int length, Func<TSource, TResult> selector)
        {
            int length2 = items.Length;
            Requires.Range(start >= 0 && start <= length2, "start");
            Requires.Range(length >= 0 && start + length <= length2, "length");
            Requires.NotNull(selector, "selector");
            if (length == 0)
            {
                return Create<TResult>();
            }
            TResult[] array = new TResult[length];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = selector(items[i + start]);
            }
            return new ImmutableArray<TResult>(array);
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Collections.Immutable.ImmutableArray`1" /> struct.</summary>
        /// <param name="items">The source array to initialize the resulting array with.</param>
        /// <param name="selector">The function to apply to each element from the source array.</param>
        /// <param name="arg">An argument to be passed to the selector mapping function.</param>
        /// <typeparam name="TSource">The type of element stored in the source array.</typeparam>
        /// <typeparam name="TArg">The type of argument to pass to the selector mapping function.</typeparam>
        /// <typeparam name="TResult">The type of element to store in the target array.</typeparam>
        /// <returns>An immutable array that contains the specified items.</returns>
        public static ImmutableArray<TResult> CreateRange<TSource, TArg, TResult>(ImmutableArray<TSource> items, Func<TSource, TArg, TResult> selector, TArg arg)
        {
            Requires.NotNull(selector, "selector");
            int length = items.Length;
            if (length == 0)
            {
                return Create<TResult>();
            }
            TResult[] array = new TResult[length];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = selector(items[i], arg);
            }
            return new ImmutableArray<TResult>(array);
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Collections.Immutable.ImmutableArray`1" /> struct.</summary>
        /// <param name="items">The source array to initialize the resulting array with.</param>
        /// <param name="start">The index of the first element in the source array to include in the resulting array.</param>
        /// <param name="length">The number of elements from the source array to include in the resulting array.</param>
        /// <param name="selector">The function to apply to each element from the source array included in the resulting array.</param>
        /// <param name="arg">An argument to be passed to the selector mapping function.</param>
        /// <typeparam name="TSource">The type of element stored in the source array.</typeparam>
        /// <typeparam name="TArg">The type of argument to be passed to the selector mapping function.</typeparam>
        /// <typeparam name="TResult">The type of element to be stored in the target array.</typeparam>
        /// <returns>An immutable array that contains the specified items.</returns>
        public static ImmutableArray<TResult> CreateRange<TSource, TArg, TResult>(ImmutableArray<TSource> items, int start, int length, Func<TSource, TArg, TResult> selector, TArg arg)
        {
            int length2 = items.Length;
            Requires.Range(start >= 0 && start <= length2, "start");
            Requires.Range(length >= 0 && start + length <= length2, "length");
            Requires.NotNull(selector, "selector");
            if (length == 0)
            {
                return Create<TResult>();
            }
            TResult[] array = new TResult[length];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = selector(items[i + start], arg);
            }
            return new ImmutableArray<TResult>(array);
        }

        /// <summary>Creates a mutable array that can be converted to an <see cref="T:System.Collections.Immutable.ImmutableArray" /> without allocating new memory.</summary>
        /// <typeparam name="T">The type of elements stored in the builder.</typeparam>
        /// <returns>A mutable array of the specified type that can be efficiently converted to an immutable array.</returns>
        public static ImmutableArray<T>.Builder CreateBuilder<T>()
        {
            return Create<T>().ToBuilder();
        }

        /// <summary>Creates a mutable array that can be converted to an <see cref="T:System.Collections.Immutable.ImmutableArray" /> without allocating new memory.</summary>
        /// <param name="initialCapacity">The initial capacity of the builder.</param>
        /// <typeparam name="T">The type of elements stored in the builder.</typeparam>
        /// <returns>A mutable array of the specified type that can be efficiently converted to an immutable array.</returns>
        public static ImmutableArray<T>.Builder CreateBuilder<T>(int initialCapacity)
        {
            return new ImmutableArray<T>.Builder(initialCapacity);
        }

        /// <summary>Creates an immutable array from the specified collection.</summary>
        /// <param name="items">The collection of objects to copy to the immutable array.</param>
        /// <typeparam name="TSource">The type of elements contained in <paramref name="items" />.</typeparam>
        /// <returns>An immutable array that contains the specified collection of objects.</returns>
        public static ImmutableArray<TSource> ToImmutableArray<TSource>(this IEnumerable<TSource> items)
        {
            if (items is ImmutableArray<TSource>)
            {
                return (ImmutableArray<TSource>)(object)items;
            }
            return CreateRange(items);
        }

        /// <summary>Creates an immutable array from the current contents of the builder's array.</summary>
        /// <param name="builder">The builder to create the immutable array from.</param>
        /// <typeparam name="TSource">The type of elements contained in the immutable array.</typeparam>
        /// <returns>An immutable array that contains the current contents of the builder's array.</returns>
        public static ImmutableArray<TSource> ToImmutableArray<TSource>(this ImmutableArray<TSource>.Builder builder)
        {
            Requires.NotNull(builder, "builder");
            return builder.ToImmutable();
        }

        /// <summary>Searches the sorted immutable array for a specified element using the default comparer and returns the zero-based index of the element, if it's found.</summary>
        /// <param name="array">The sorted array to search.</param>
        /// <param name="value">The object to search for.</param>
        /// <typeparam name="T">The type of element stored in the array.</typeparam>
        /// <exception cref="T:System.InvalidOperationException">
        ///   <paramref name="value" /> does not implement <see cref="T:System.IComparable" /> or the search encounters an element that does not implement <see cref="T:System.IComparable" />.</exception>
        /// <returns>The zero-based index of the item in the array, if item is found; otherwise, a negative number that is the bitwise complement of the index of the next element that is larger than <paramref name="value" /> or, if there is no larger element, the bitwise complement of <see cref="P:System.Collections.Generic.ICollection`1.Count" />.</returns>
        public static int BinarySearch<T>(this ImmutableArray<T> array, T value)
        {
            return Array.BinarySearch<T>(array.array, value);
        }

        /// <summary>Searches a sorted immutable array for a specified element and returns the zero-based index of the element, if it's found.</summary>
        /// <param name="array">The sorted array to search.</param>
        /// <param name="value">The object to search for.</param>
        /// <param name="comparer">The comparer implementation to use when comparing elements, or null to use the default comparer.</param>
        /// <typeparam name="T">The type of element stored in the array.</typeparam>
        /// <exception cref="T:System.InvalidOperationException">
        ///   <paramref name="comparer" /> is null and <paramref name="value" /> does not implement <see cref="T:System.IComparable" /> or the search encounters an element that does not implement <see cref="T:System.IComparable" />.</exception>
        /// <returns>The zero-based index of the item in the array, if item is found; otherwise, a negative number that is the bitwise complement of the index of the next element that is larger than <paramref name="value" /> or, if there is no larger element, the bitwise complement of <see cref="P:System.Collections.Generic.ICollection`1.Count" />.</returns>
        public static int BinarySearch<T>(this ImmutableArray<T> array, T value, IComparer<T>? comparer)
        {
            return Array.BinarySearch(array.array, value, comparer);
        }

        /// <summary>Searches a sorted immutable array for a specified element and returns the zero-based index of the element, if it's found.</summary>
        /// <param name="array">The sorted array to search.</param>
        /// <param name="index">The starting index of the range to search.</param>
        /// <param name="length">The length of the range to search.</param>
        /// <param name="value">The object to search for.</param>
        /// <typeparam name="T">The type of element stored in the array.</typeparam>
        /// <exception cref="T:System.InvalidOperationException">
        ///   <paramref name="value" /> does not implement <see cref="T:System.IComparable" /> or the search encounters an element that does not implement <see cref="T:System.IComparable" />.</exception>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="index" /> and <paramref name="length" /> do not specify a valid range in <paramref name="array" />.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///   <paramref name="index" /> is less than the lower bound of <paramref name="array" />.
        ///
        /// -or-
        ///
        /// <paramref name="length" /> is less than zero.</exception>
        /// <returns>The zero-based index of the item in the array, if item is found; otherwise, a negative number that is the bitwise complement of the index of the next element that is larger than <paramref name="value" /> or, if there is no larger element, the bitwise complement of <see cref="P:System.Collections.Generic.ICollection`1.Count" />.</returns>
        public static int BinarySearch<T>(this ImmutableArray<T> array, int index, int length, T value)
        {
            return Array.BinarySearch<T>(array.array, index, length, value);
        }

        /// <summary>Searches a sorted immutable array for a specified element and returns the zero-based index of the element.</summary>
        /// <param name="array">The sorted array to search.</param>
        /// <param name="index">The starting index of the range to search.</param>
        /// <param name="length">The length of the range to search.</param>
        /// <param name="value">The object to search for.</param>
        /// <param name="comparer">The comparer to use when comparing elements for equality or <see langword="null" /> to use the default comparer.</param>
        /// <typeparam name="T">The type of element stored in the array.</typeparam>
        /// <exception cref="T:System.InvalidOperationException">
        ///   <paramref name="comparer" /> is null and <paramref name="value" /> does not implement <see cref="T:System.IComparable" /> or the search encounters an element that does not implement <see cref="T:System.IComparable" />.</exception>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="index" /> and <paramref name="length" /> do not specify a valid range in <paramref name="array" />.
        ///
        /// -or-
        ///
        /// <paramref name="comparer" /> is <see langword="null" />, and <paramref name="value" /> is of a type that is not compatible with the elements of <paramref name="array" />.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///   <paramref name="index" /> is less than the lower bound of <paramref name="array" />.
        ///
        /// -or-
        ///
        /// <paramref name="length" /> is less than zero.</exception>
        /// <returns>The zero-based index of the item in the array, if item is found; otherwise, a negative number that is the bitwise complement of the index of the next element that is larger than <paramref name="value" /> or, if there is no larger element, the bitwise complement of <see cref="P:System.Collections.Generic.ICollection`1.Count" />.</returns>
        public static int BinarySearch<T>(this ImmutableArray<T> array, int index, int length, T value, IComparer<T>? comparer)
        {
            return Array.BinarySearch(array.array, index, length, value, comparer);
        }
    }
    /// <summary>Represents an array that is immutable; meaning it cannot be changed once it is created.  </summary>
    /// <typeparam name="T">The type of element stored by the array.</typeparam>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    [NonVersionable]
    public readonly struct ImmutableArray<T> : IReadOnlyList<T>, IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable, IList<T>, ICollection<T>, IEquatable<ImmutableArray<T>>, IList, ICollection, IImmutableArray, IStructuralComparable, IStructuralEquatable, IImmutableList<T>
    {
        /// <summary>A writable array accessor that can be converted into an <see cref="System.Collections.Immutable.ImmutableArray{T}" /> instance without allocating extra memory. </summary>
        [DebuggerDisplay("Count = {Count}")]
        [DebuggerTypeProxy(typeof(ImmutableArrayBuilderDebuggerProxy<>))]
        public sealed class Builder : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IReadOnlyList<T>, IReadOnlyCollection<T>
        {
            private T[] _elements;

            private int _count;

            /// <summary>Gets or sets the length of the internal array. When set, the internal array is reallocated to the given capacity if it is not already the specified length.</summary>
            /// <returns>The length of the internal array.</returns>
            public int Capacity
            {
                get
                {
                    return _elements.Length;
                }
                set
                {
                    if (value < _count)
                    {
                        throw new ArgumentException(MDCFR.Properties.Resources.CapacityMustBeGreaterThanOrEqualToCount, "value");
                    }
                    if (value == _elements.Length)
                    {
                        return;
                    }
                    if (value > 0)
                    {
                        T[] array = new T[value];
                        if (_count > 0)
                        {
                            Array.Copy(_elements, array, _count);
                        }
                        _elements = array;
                    }
                    else
                    {
                        _elements = ImmutableArray<T>.Empty.array;
                    }
                }
            }

            /// <summary>Gets or sets the number of items in the array.</summary>
            /// <returns>The number of items in the array.</returns>
            public int Count
            {
                get
                {
                    return _count;
                }
                set
                {
                    Requires.Range(value >= 0, "value");
                    if (value < _count)
                    {
                        if (_count - value > 64)
                        {
                            Array.Clear(_elements, value, _count - value);
                        }
                        else
                        {
                            for (int i = value; i < Count; i++)
                            {
                                _elements[i] = default(T);
                            }
                        }
                    }
                    else if (value > _count)
                    {
                        EnsureCapacity(value);
                    }
                    _count = value;
                }
            }

            /// <summary>Gets or sets the item at the specified index.</summary>
            /// <param name="index">The index of the item to get or set.</param>
            /// <exception cref="T:System.IndexOutOfRangeException">The specified index is not in the array.</exception>
            /// <returns>The item at the specified index.</returns>
            public T this[int index]
            {
                get
                {
                    if (index >= Count)
                    {
                        ThrowIndexOutOfRangeException();
                    }
                    return _elements[index];
                }
                set
                {
                    if (index >= Count)
                    {
                        ThrowIndexOutOfRangeException();
                    }
                    _elements[index] = value;
                }
            }

            /// <summary>Gets a value that indicates whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</summary>
            /// <returns>
            ///   <see langword="true" /> if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, <see langword="false" />.</returns>
            bool ICollection<T>.IsReadOnly => false;

            internal Builder(int capacity)
            {
                Requires.Range(capacity >= 0, "capacity");
                _elements = new T[capacity];
                _count = 0;
            }

            internal Builder()
                : this(8)
            {
            }

            private static void ThrowIndexOutOfRangeException()
            {
                throw new IndexOutOfRangeException();
            }

            /// <summary>Gets a read-only reference to the element at the specified index.</summary>
            /// <param name="index">The item index.</param>
            /// <exception cref="T:System.IndexOutOfRangeException">
            ///   <paramref name="index" /> is greater or equal to the array count.</exception>
            /// <returns>The read-only reference to the element at the specified index.</returns>
            public ref readonly T ItemRef(int index)
            {
                if (index >= Count)
                {
                    ThrowIndexOutOfRangeException();
                }
                return ref _elements[index];
            }

            /// <summary>Returns an immutable array that contains the current contents of this <see cref="T:System.Collections.Immutable.ImmutableArray`1.Builder" />.</summary>
            /// <returns>An immutable array that contains the current contents of this <see cref="T:System.Collections.Immutable.ImmutableArray`1.Builder" />.</returns>
            public ImmutableArray<T> ToImmutable()
            {
                return new ImmutableArray<T>(ToArray());
            }

            /// <summary>Extracts the internal array as an <see cref="T:System.Collections.Immutable.ImmutableArray`1" /> and replaces it              with a zero length array.</summary>
            /// <exception cref="T:System.InvalidOperationException">When <see cref="P:System.Collections.Immutable.ImmutableArray`1.Builder.Count" /> doesn't              equal <see cref="P:System.Collections.Immutable.ImmutableArray`1.Builder.Capacity" />.</exception>
            /// <returns>An immutable array containing the elements of the builder.</returns>
            public ImmutableArray<T> MoveToImmutable()
            {
                if (Capacity != Count)
                {
                    throw new InvalidOperationException(MDCFR.Properties.Resources.CapacityMustEqualCountOnMove);
                }
                T[] elements = _elements;
                _elements = ImmutableArray<T>.Empty.array;
                _count = 0;
                return new ImmutableArray<T>(elements);
            }

            /// <summary>Removes all items from the array.</summary>
            public void Clear()
            {
                Count = 0;
            }

            /// <summary>Inserts an item in the array at the specified index.</summary>
            /// <param name="index">The zero-based index at which to insert the item.</param>
            /// <param name="item">The object to insert into the array.</param>
            public void Insert(int index, T item)
            {
                Requires.Range(index >= 0 && index <= Count, "index");
                EnsureCapacity(Count + 1);
                if (index < Count)
                {
                    Array.Copy(_elements, index, _elements, index + 1, Count - index);
                }
                _count++;
                _elements[index] = item;
            }

            /// <summary>Inserts the specified values at the specified index.</summary>
            /// <param name="index">The index at which to insert the value.</param>
            /// <param name="items">The elements to insert.</param>
            public void InsertRange(int index, IEnumerable<T> items)
            {
                Requires.Range(index >= 0 && index <= Count, "index");
                Requires.NotNull(items, "items");
                int count = ImmutableExtensions.GetCount(ref items);
                EnsureCapacity(Count + count);
                if (index != Count)
                {
                    Array.Copy(_elements, index, _elements, index + count, _count - index);
                }
                if (!items.TryCopyTo(_elements, index))
                {
                    foreach (T item in items)
                    {
                        _elements[index++] = item;
                    }
                }
                _count += count;
            }

            /// <summary>Inserts the specified values at the specified index.</summary>
            /// <param name="index">The index at which to insert the value.</param>
            /// <param name="items">The elements to insert.</param>
            public void InsertRange(int index, ImmutableArray<T> items)
            {
                Requires.Range(index >= 0 && index <= Count, "index");
                if (!items.IsEmpty)
                {
                    EnsureCapacity(Count + items.Length);
                    if (index != Count)
                    {
                        Array.Copy(_elements, index, _elements, index + items.Length, _count - index);
                    }
                    Array.Copy(items.array, 0, _elements, index, items.Length);
                    _count += items.Length;
                }
            }

            /// <summary>Adds the specified item to the array.</summary>
            /// <param name="item">The object to add to the array.</param>
            public void Add(T item)
            {
                int num = _count + 1;
                EnsureCapacity(num);
                _elements[_count] = item;
                _count = num;
            }

            /// <summary>Adds the specified items to the end of the array.</summary>
            /// <param name="items">The items to add to the array.</param>
            public void AddRange(IEnumerable<T> items)
            {
                Requires.NotNull(items, "items");
                if (items.TryGetCount(out var count))
                {
                    EnsureCapacity(Count + count);
                    if (items.TryCopyTo(_elements, _count))
                    {
                        _count += count;
                        return;
                    }
                }
                foreach (T item in items)
                {
                    Add(item);
                }
            }

            /// <summary>Adds the specified items to the end of the array.</summary>
            /// <param name="items">The items to add to the array.</param>
            public void AddRange(params T[] items)
            {
                Requires.NotNull(items, "items");
                int count = Count;
                Count += items.Length;
                Array.Copy(items, 0, _elements, count, items.Length);
            }

            /// <summary>Adds the specified items that derive from the type currently in the array, to the end of the array.</summary>
            /// <param name="items">The items to add to end of the array.</param>
            /// <typeparam name="TDerived">The type that derives from the type of item already in the array.</typeparam>
            public void AddRange<TDerived>(TDerived[] items) where TDerived : T
            {
                Requires.NotNull(items, "items");
                int count = Count;
                Count += items.Length;
                Array.Copy(items, 0, _elements, count, items.Length);
            }

            /// <summary>Adds the specified items to the end of the array.</summary>
            /// <param name="items">The items to add to the array.</param>
            /// <param name="length">The number of elements from the source array to add.</param>
            public void AddRange(T[] items, int length)
            {
                Requires.NotNull(items, "items");
                Requires.Range(length >= 0 && length <= items.Length, "length");
                int count = Count;
                Count += length;
                Array.Copy(items, 0, _elements, count, length);
            }

            /// <summary>Adds the specified items to the end of the array.</summary>
            /// <param name="items">The items to add to the array.</param>
            public void AddRange(ImmutableArray<T> items)
            {
                AddRange(items, items.Length);
            }

            /// <summary>Adds the specified items to the end of the array.</summary>
            /// <param name="items">The items to add to the array.</param>
            /// <param name="length">The number of elements from the source array to add.</param>
            public void AddRange(ImmutableArray<T> items, int length)
            {
                Requires.Range(length >= 0, "length");
                if (items.array != null)
                {
                    AddRange(items.array, length);
                }
            }

            /// <summary>Adds the specified items to the end of the array.</summary>
            /// <param name="items">The items to add at the end of the array.</param>
            public void AddRange(ReadOnlySpan<T> items)
            {
                int count = Count;
                Count += items.Length;
                items.CopyTo(new Span<T>(_elements, count, items.Length));
            }

            /// <summary>Adds the specified items to the end of the array.</summary>
            /// <param name="items">The items to add at the end of the array.</param>
            /// <typeparam name="TDerived">The type that derives from the type of item already in the array.</typeparam>
            public void AddRange<TDerived>(ReadOnlySpan<TDerived> items) where TDerived : T
            {
                int count = Count;
                Count += items.Length;
                Span<T> span = new Span<T>(_elements, count, items.Length);
                for (int i = 0; i < items.Length; i++)
                {
                    span[i] = (T)(object)items[i];
                }
            }

            /// <summary>Adds the specified items that derive from the type currently in the array, to the end of the array.</summary>
            /// <param name="items">The items to add to the end of the array.</param>
            /// <typeparam name="TDerived">The type that derives from the type of item already in the array.</typeparam>
            public void AddRange<TDerived>(ImmutableArray<TDerived> items) where TDerived : T
            {
                if (items.array != null)
                {
                    this.AddRange<TDerived>(items.array);
                }
            }

            /// <summary>Adds the specified items to the end of the array.</summary>
            /// <param name="items">The items to add to the array.</param>
            public void AddRange(Builder items)
            {
                Requires.NotNull(items, "items");
                AddRange(items._elements, items.Count);
            }

            /// <summary>Adds the specified items that derive from the type currently in the array, to the end of the array.</summary>
            /// <param name="items">The items to add to the end of the array.</param>
            /// <typeparam name="TDerived">The type that derives from the type of item already in the array.</typeparam>
            public void AddRange<TDerived>(ImmutableArray<TDerived>.Builder items) where TDerived : T
            {
                Requires.NotNull(items, "items");
                AddRange<TDerived>(items._elements, items.Count);
            }

            /// <summary>Removes the specified element.</summary>
            /// <param name="element">The item to remove.</param>
            /// <returns>
            ///   <see langword="true" /> if <paramref name="element" /> was found and removed; otherwise, <see langword="false" />.</returns>
            public bool Remove(T element)
            {
                int num = IndexOf(element);
                if (num >= 0)
                {
                    RemoveAt(num);
                    return true;
                }
                return false;
            }

            /// <summary>Removes the first occurrence of the specified element from the builder.
            ///       If no match is found, the builder remains unchanged.</summary>
            /// <param name="element">The element to remove.</param>
            /// <param name="equalityComparer">The equality comparer to use in the search.
            ///       If <see langword="null" />, <see cref="P:System.Collections.Generic.EqualityComparer`1.Default" /> is used.</param>
            /// <returns>A value indicating whether the specified element was found and removed from the collection.</returns>
            public bool Remove(T element, IEqualityComparer<T>? equalityComparer)
            {
                int num = IndexOf(element, 0, _count, equalityComparer);
                if (num >= 0)
                {
                    RemoveAt(num);
                    return true;
                }
                return false;
            }

            /// <summary>Removes all the elements that match the conditions defined by the specified predicate.</summary>
            /// <param name="match">The <see cref="T:System.Predicate`1" /> delegate that defines the conditions of the elements to remove.</param>
            public void RemoveAll(Predicate<T> match)
            {
                List<int> list = null;
                for (int i = 0; i < _count; i++)
                {
                    if (match(_elements[i]))
                    {
                        if (list == null)
                        {
                            list = new List<int>();
                        }
                        list.Add(i);
                    }
                }
                if (list != null)
                {
                    RemoveAtRange(list);
                }
            }

            /// <summary>Removes the item at the specified index from the array.</summary>
            /// <param name="index">The zero-based index of the item to remove.</param>
            public void RemoveAt(int index)
            {
                Requires.Range(index >= 0 && index < Count, "index");
                if (index < Count - 1)
                {
                    Array.Copy(_elements, index + 1, _elements, index, Count - index - 1);
                }
                Count--;
            }

            /// <summary>Removes the specified values from this list.</summary>
            /// <param name="index">The 0-based index into the array for the element to omit from the returned array.</param>
            /// <param name="length">The number of elements to remove.</param>
            public void RemoveRange(int index, int length)
            {
                Requires.Range(index >= 0 && index + length <= _count, "index");
                if (length != 0)
                {
                    if (index + length < _count)
                    {
                        Array.Copy(_elements, index + length, _elements, index, Count - index - length);
                    }
                    _count -= length;
                }
            }

            /// <summary>Removes the specified values from this list.</summary>
            /// <param name="items">The items to remove if matches are found in this list.</param>
            public void RemoveRange(IEnumerable<T> items)
            {
                RemoveRange(items, EqualityComparer<T>.Default);
            }

            /// <summary>Removes the specified values from this list.</summary>
            /// <param name="items">The items to remove if matches are found in this list.</param>
            /// <param name="equalityComparer">The equality comparer to use in the search.
            ///       If <see langword="null" />, <see cref="P:System.Collections.Generic.EqualityComparer`1.Default" /> is used.</param>
            public void RemoveRange(IEnumerable<T> items, IEqualityComparer<T>? equalityComparer)
            {
                Requires.NotNull(items, "items");
                SortedSet<int> sortedSet = new SortedSet<int>();
                foreach (T item in items)
                {
                    int num = IndexOf(item, 0, _count, equalityComparer);
                    while (num >= 0 && !sortedSet.Add(num) && num + 1 < _count)
                    {
                        num = IndexOf(item, num + 1, equalityComparer);
                    }
                }
                RemoveAtRange(sortedSet);
            }

            /// <summary>Replaces the first equal element in the list with the specified element.</summary>
            /// <param name="oldValue">The element to replace.</param>
            /// <param name="newValue">The element to replace the old element with.</param>
            public void Replace(T oldValue, T newValue)
            {
                Replace(oldValue, newValue, EqualityComparer<T>.Default);
            }

            /// <summary>Replaces the first equal element in the list with the specified element.</summary>
            /// <param name="oldValue">The element to replace.</param>
            /// <param name="newValue">The element to replace the old element with.</param>
            /// <param name="equalityComparer">The equality comparer to use in the search.
            ///       If <see langword="null" />, <see cref="P:System.Collections.Generic.EqualityComparer`1.Default" /> is used.</param>
            public void Replace(T oldValue, T newValue, IEqualityComparer<T>? equalityComparer)
            {
                int num = IndexOf(oldValue, 0, _count, equalityComparer);
                if (num >= 0)
                {
                    _elements[num] = newValue;
                }
            }

            /// <summary>Determines whether the array contains a specific value.</summary>
            /// <param name="item">The object to locate in the array.</param>
            /// <returns>
            ///   <see langword="true" /> if the object is found; otherwise, <see langword="false" />.</returns>
            public bool Contains(T item)
            {
                return IndexOf(item) >= 0;
            }

            /// <summary>Creates a new array with the current contents of this <see cref="T:System.Collections.Immutable.ImmutableArray`1.Builder" />.</summary>
            /// <returns>A new array with the contents of this <see cref="T:System.Collections.Immutable.ImmutableArray`1.Builder" />.</returns>
            public T[] ToArray()
            {
                if (Count == 0)
                {
                    return ImmutableArray<T>.Empty.array;
                }
                T[] array = new T[Count];
                Array.Copy(_elements, array, Count);
                return array;
            }

            /// <summary>Copies the current contents to the specified array.</summary>
            /// <param name="array">The array to copy to.</param>
            /// <param name="index">The index to start the copy operation.</param>
            public void CopyTo(T[] array, int index)
            {
                Requires.NotNull(array, "array");
                Requires.Range(index >= 0 && index + Count <= array.Length, "index");
                Array.Copy(_elements, 0, array, index, Count);
            }

            /// <summary>Copies the contents of this array to the specified array.</summary>
            /// <param name="destination">The array to copy to.</param>
            public void CopyTo(T[] destination)
            {
                Requires.NotNull(destination, "destination");
                Array.Copy(_elements, 0, destination, 0, Count);
            }

            /// <summary>Copies the contents of this array to the specified array.</summary>
            /// <param name="sourceIndex">The index into this collection of the first element to copy.</param>
            /// <param name="destination">The array to copy to.</param>
            /// <param name="destinationIndex">The index into the destination array to which the first copied element is written.</param>
            /// <param name="length">The number of elements to copy.</param>
            public void CopyTo(int sourceIndex, T[] destination, int destinationIndex, int length)
            {
                Requires.NotNull(destination, "destination");
                Requires.Range(length >= 0, "length");
                Requires.Range(sourceIndex >= 0 && sourceIndex + length <= Count, "sourceIndex");
                Requires.Range(destinationIndex >= 0 && destinationIndex + length <= destination.Length, "destinationIndex");
                Array.Copy(_elements, sourceIndex, destination, destinationIndex, length);
            }

            private void EnsureCapacity(int capacity)
            {
                if (_elements.Length < capacity)
                {
                    int newSize = Math.Max(_elements.Length * 2, capacity);
                    Array.Resize(ref _elements, newSize);
                }
            }

            /// <summary>Determines the index of a specific item in the array.</summary>
            /// <param name="item">The item to locate in the array.</param>
            /// <returns>The index of <paramref name="item" /> if it's found in the list; otherwise, -1.</returns>
            public int IndexOf(T item)
            {
                return IndexOf(item, 0, _count, EqualityComparer<T>.Default);
            }

            /// <summary>Determines the index of the specified item.</summary>
            /// <param name="item">The item to locate in the array.</param>
            /// <param name="startIndex">The starting position of the search.</param>
            /// <returns>The index of <paramref name="item" /> if it's found in the list; otherwise, -1.</returns>
            public int IndexOf(T item, int startIndex)
            {
                return IndexOf(item, startIndex, Count - startIndex, EqualityComparer<T>.Default);
            }

            /// <summary>Determines the index of the specified item.</summary>
            /// <param name="item">The item to locate in the array.</param>
            /// <param name="startIndex">The starting position of the search.</param>
            /// <param name="count">The number of elements to search.</param>
            /// <returns>The index of <paramref name="item" /> if it's found in the list; otherwise, -1.</returns>
            public int IndexOf(T item, int startIndex, int count)
            {
                return IndexOf(item, startIndex, count, EqualityComparer<T>.Default);
            }

            /// <summary>Determines the index for the specified item.</summary>
            /// <param name="item">The item to locate in the array.</param>
            /// <param name="startIndex">The index at which to begin the search.</param>
            /// <param name="count">The starting position of the search.</param>
            /// <param name="equalityComparer">The equality comparer to use in the search.</param>
            /// <returns>The index of <paramref name="item" /> if it's found in the list; otherwise, -1.</returns>
            public int IndexOf(T item, int startIndex, int count, IEqualityComparer<T>? equalityComparer)
            {
                if (count == 0 && startIndex == 0)
                {
                    return -1;
                }
                Requires.Range(startIndex >= 0 && startIndex < Count, "startIndex");
                Requires.Range(count >= 0 && startIndex + count <= Count, "count");
                if (equalityComparer == null)
                {
                    equalityComparer = EqualityComparer<T>.Default;
                }
                if (equalityComparer == EqualityComparer<T>.Default)
                {
                    return Array.IndexOf<T>(_elements, item, startIndex, count);
                }
                for (int i = startIndex; i < startIndex + count; i++)
                {
                    if (equalityComparer.Equals(_elements[i], item))
                    {
                        return i;
                    }
                }
                return -1;
            }

            /// <summary>Searches the array for the specified item.</summary>
            /// <param name="item">The item to search for.</param>
            /// <param name="startIndex">The index at which to begin the search.</param>
            /// <param name="equalityComparer">The equality comparer to use in the search.
            ///       If <see langword="null" />, <see cref="P:System.Collections.Generic.EqualityComparer`1.Default" /> is used.</param>
            /// <returns>The 0-based index into the array where the item was found; or -1 if it could not be found.</returns>
            public int IndexOf(T item, int startIndex, IEqualityComparer<T>? equalityComparer)
            {
                return IndexOf(item, startIndex, Count - startIndex, equalityComparer);
            }

            /// <summary>Determines the 0-based index of the last occurrence of the specified item in this array.</summary>
            /// <param name="item">The item to search for.</param>
            /// <returns>The 0-based index where the item was found; or -1 if it could not be found.</returns>
            public int LastIndexOf(T item)
            {
                if (Count == 0)
                {
                    return -1;
                }
                return LastIndexOf(item, Count - 1, Count, EqualityComparer<T>.Default);
            }

            /// <summary>Determines the 0-based index of the last occurrence of the specified item in this array.</summary>
            /// <param name="item">The item to search for.</param>
            /// <param name="startIndex">The starting position of the search.</param>
            /// <returns>The 0-based index into the array where the item was found; or -1 if it could not be found.</returns>
            public int LastIndexOf(T item, int startIndex)
            {
                if (Count == 0 && startIndex == 0)
                {
                    return -1;
                }
                Requires.Range(startIndex >= 0 && startIndex < Count, "startIndex");
                return LastIndexOf(item, startIndex, startIndex + 1, EqualityComparer<T>.Default);
            }

            /// <summary>Determines the 0-based index of the last occurrence of the specified item in this array.</summary>
            /// <param name="item">The item to search for.</param>
            /// <param name="startIndex">The starting position of the search.</param>
            /// <param name="count">The number of elements to search.</param>
            /// <returns>The 0-based index into the array where the item was found; or -1 if it could not be found.</returns>
            public int LastIndexOf(T item, int startIndex, int count)
            {
                return LastIndexOf(item, startIndex, count, EqualityComparer<T>.Default);
            }

            /// <summary>Determines the 0-based index of the last occurrence of the specified item in this array.</summary>
            /// <param name="item">The item to search for.</param>
            /// <param name="startIndex">The starting position of the search.</param>
            /// <param name="count">The number of elements to search.</param>
            /// <param name="equalityComparer">The equality comparer to use in the search.</param>
            /// <returns>The 0-based index into the array where the item was found; or -1 if it could not be found.</returns>
            public int LastIndexOf(T item, int startIndex, int count, IEqualityComparer<T>? equalityComparer)
            {
                if (count == 0 && startIndex == 0)
                {
                    return -1;
                }
                Requires.Range(startIndex >= 0 && startIndex < Count, "startIndex");
                Requires.Range(count >= 0 && startIndex - count + 1 >= 0, "count");
                if (equalityComparer == null)
                {
                    equalityComparer = EqualityComparer<T>.Default;
                }
                if (equalityComparer == EqualityComparer<T>.Default)
                {
                    return Array.LastIndexOf<T>(_elements, item, startIndex, count);
                }
                for (int num = startIndex; num >= startIndex - count + 1; num--)
                {
                    if (equalityComparer.Equals(item, _elements[num]))
                    {
                        return num;
                    }
                }
                return -1;
            }

            /// <summary>Reverses the order of elements in the collection.</summary>
            public void Reverse()
            {
                int num = 0;
                int num2 = _count - 1;
                T[] elements = _elements;
                while (num < num2)
                {
                    T val = elements[num];
                    elements[num] = elements[num2];
                    elements[num2] = val;
                    num++;
                    num2--;
                }
            }

            /// <summary>Sorts the contents of the array.</summary>
            public void Sort()
            {
                if (Count > 1)
                {
                    Array.Sort(_elements, 0, Count, Comparer<T>.Default);
                }
            }

            /// <summary>Sorts the elements in the entire array using the specified <see cref="T:System.Comparison`1" />.</summary>
            /// <param name="comparison">The <see cref="T:System.Comparison`1" /> to use when comparing elements.</param>
            /// <exception cref="T:System.ArgumentNullException">
            ///   <paramref name="comparison" /> is null.</exception>
            public void Sort(Comparison<T> comparison)
            {
                Requires.NotNull(comparison, "comparison");
                if (Count > 1)
                {
                    Array.Sort(_elements, 0, _count, Comparer<T>.Create(comparison));
                }
            }

            /// <summary>Sorts the contents of the array.</summary>
            /// <param name="comparer">The comparer to use for sorting. If comparer is <see langword="null" />, the default comparer for the elements type in the array is used.</param>
            public void Sort(IComparer<T>? comparer)
            {
                if (Count > 1)
                {
                    Array.Sort(_elements, 0, _count, comparer);
                }
            }

            /// <summary>Sorts the contents of the array.</summary>
            /// <param name="index">The starting index for the sort.</param>
            /// <param name="count">The number of elements to include in the sort.</param>
            /// <param name="comparer">The comparer to use for sorting. If comparer is <see langword="null" />, the default comparer for the elements type in the array is used.</param>
            public void Sort(int index, int count, IComparer<T>? comparer)
            {
                Requires.Range(index >= 0, "index");
                Requires.Range(count >= 0 && index + count <= Count, "count");
                if (count > 1)
                {
                    Array.Sort(_elements, index, count, comparer);
                }
            }

            /// <summary>Copies the current contents to the specified <see cref="T:System.Span`1" />.</summary>
            /// <param name="destination">The <see cref="T:System.Span`1" /> to copy to.</param>
            public void CopyTo(Span<T> destination)
            {
                Requires.Range(Count <= destination.Length, "destination");
                new ReadOnlySpan<T>(_elements, 0, Count).CopyTo(destination);
            }

            /// <summary>Gets an object that can be used to iterate through the collection.</summary>
            /// <returns>An object that can be used to iterate through the collection.</returns>
            public IEnumerator<T> GetEnumerator()
            {
                for (int i = 0; i < Count; i++)
                {
                    yield return this[i];
                }
            }

            /// <summary>Returns an enumerator that iterates through the array.</summary>
            /// <returns>An enumerator that iterates through the array.</returns>
            IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                return GetEnumerator();
            }

            /// <summary>Returns an enumerator that iterates through the array.</summary>
            /// <returns>An enumerator that iterates through the array.</returns>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            private void AddRange<TDerived>(TDerived[] items, int length) where TDerived : T
            {
                EnsureCapacity(Count + length);
                int count = Count;
                Count += length;
                T[] elements = _elements;
                for (int i = 0; i < length; i++)
                {
                    elements[count + i] = (T)(object)items[i];
                }
            }

            private void RemoveAtRange(ICollection<int> indicesToRemove)
            {
                Requires.NotNull(indicesToRemove, "indicesToRemove");
                if (indicesToRemove.Count == 0)
                {
                    return;
                }
                int num = 0;
                int num2 = 0;
                int num3 = -1;
                foreach (int item in indicesToRemove)
                {
                    int num4 = ((num3 == -1) ? item : (item - num3 - 1));
                    Array.Copy(_elements, num + num2, _elements, num, num4);
                    num2++;
                    num += num4;
                    num3 = item;
                }
                Array.Copy(_elements, num + num2, _elements, num, _elements.Length - (num + num2));
                _count -= indicesToRemove.Count;
            }
        }

        /// <summary>An array enumerator. </summary>
        public struct Enumerator
        {
            private readonly T[] _array;

            private int _index;

            /// <summary>Gets the current item.</summary>
            /// <returns>The current item.</returns>
            public T Current => _array[_index];

            internal Enumerator(T[] array)
            {
                _array = array;
                _index = -1;
            }

            /// <summary>Advances to the next value in the array.</summary>
            /// <returns>
            ///   <see langword="true" /> if another item exists in the array; otherwise, <see langword="false" />.</returns>
            public bool MoveNext()
            {
                return ++_index < _array.Length;
            }
        }

        private sealed class EnumeratorObject : IEnumerator<T>, IDisposable, IEnumerator
        {
            private static readonly IEnumerator<T> s_EmptyEnumerator = new EnumeratorObject(ImmutableArray<T>.Empty.array);

            private readonly T[] _array;

            private int _index;

            public T Current
            {
                get
                {
                    if ((uint)_index < (uint)_array.Length)
                    {
                        return _array[_index];
                    }
                    throw new InvalidOperationException();
                }
            }

            object IEnumerator.Current => Current;

            private EnumeratorObject(T[] array)
            {
                _index = -1;
                _array = array;
            }

            public bool MoveNext()
            {
                int num = _index + 1;
                int num2 = _array.Length;
                if ((uint)num <= (uint)num2)
                {
                    _index = num;
                    return (uint)num < (uint)num2;
                }
                return false;
            }

            void IEnumerator.Reset()
            {
                _index = -1;
            }

            public void Dispose()
            {
            }

            internal static IEnumerator<T> Create(T[] array)
            {
                if (array.Length != 0)
                {
                    return new EnumeratorObject(array);
                }
                return s_EmptyEnumerator;
            }
        }

        /// <summary>Gets an empty immutable array.</summary>
        public static readonly ImmutableArray<T> Empty = new ImmutableArray<T>(new T[0]);

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        internal readonly T[]? array;

        /// <summary>Gets or sets the element at the specified index in the read-only list.</summary>
        /// <param name="index">The zero-based index of the element to get.</param>
        /// <exception cref="T:System.NotSupportedException">Always thrown from the setter.</exception>
        /// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Collections.Immutable.ImmutableArray`1.IsDefault" /> property returns <see langword="true" />.</exception>
        /// <returns>The element at the specified index in the read-only list.</returns>
        T IList<T>.this[int index]
        {
            get
            {
                ImmutableArray<T> immutableArray = this;
                immutableArray.ThrowInvalidOperationIfNotInitialized();
                return immutableArray[index];
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>Gets a value indicating whether this instance is read only.</summary>
        /// <returns>
        ///   <see langword="true" /> if this instance is read only; otherwise, <see langword="false" />.</returns>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool ICollection<T>.IsReadOnly => true;

        /// <summary>Gets the number of items in the collection.</summary>
        /// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Collections.Immutable.ImmutableArray`1.IsDefault" /> property returns <see langword="true" />.</exception>
        /// <returns>Number of items in the collection.</returns>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        int ICollection<T>.Count
        {
            get
            {
                ImmutableArray<T> immutableArray = this;
                immutableArray.ThrowInvalidOperationIfNotInitialized();
                return immutableArray.Length;
            }
        }

        /// <summary>Gets the number of items in the collection.</summary>
        /// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Collections.Immutable.ImmutableArray`1.IsDefault" /> property returns <see langword="true" />.</exception>
        /// <returns>The number of items in the collection.</returns>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        int IReadOnlyCollection<T>.Count
        {
            get
            {
                ImmutableArray<T> immutableArray = this;
                immutableArray.ThrowInvalidOperationIfNotInitialized();
                return immutableArray.Length;
            }
        }

        /// <summary>Gets the element at the specified index.</summary>
        /// <param name="index">The index.</param>
        /// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Collections.Immutable.ImmutableArray`1.IsDefault" /> property returns <see langword="true" />.</exception>
        /// <returns>The element.</returns>
        T IReadOnlyList<T>.this[int index]
        {
            get
            {
                ImmutableArray<T> immutableArray = this;
                immutableArray.ThrowInvalidOperationIfNotInitialized();
                return immutableArray[index];
            }
        }

        /// <summary>Gets a value indicating whether this instance is fixed size.</summary>
        /// <returns>
        ///   <see langword="true" /> if this instance is fixed size; otherwise, <see langword="false" />.</returns>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool IList.IsFixedSize => true;

        /// <summary>Gets a value indicating whether this instance is read only.</summary>
        /// <returns>
        ///   <see langword="true" /> if this instance is read only; otherwise, <see langword="false" />.</returns>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool IList.IsReadOnly => true;

        /// <summary>Gets the size of the array.</summary>
        /// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Collections.Immutable.ImmutableArray`1.IsDefault" /> property returns <see langword="true" />.</exception>
        /// <returns>The number of items in the collection.</returns>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        int ICollection.Count
        {
            get
            {
                ImmutableArray<T> immutableArray = this;
                immutableArray.ThrowInvalidOperationIfNotInitialized();
                return immutableArray.Length;
            }
        }

        /// <summary>See the <see cref="T:System.Collections.ICollection" /> interface. Always returns <see langword="true" /> since since immutable collections are thread-safe.</summary>
        /// <returns>Boolean value determining whether the collection is thread-safe.</returns>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool ICollection.IsSynchronized => true;

        /// <summary>Gets the sync root.</summary>
        /// <returns>An object for synchronizing access to the collection.</returns>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        object ICollection.SyncRoot
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>Gets or sets the <see cref="T:System.Object" /> at the specified index.</summary>
        /// <param name="index">The index.</param>
        /// <exception cref="T:System.NotSupportedException">Always thrown from the setter.</exception>
        /// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Collections.Immutable.ImmutableArray`1.IsDefault" /> property returns <see langword="true" />.</exception>
        /// <returns>The object at the specified index.</returns>
        object? IList.this[int index]
        {
            get
            {
                ImmutableArray<T> immutableArray = this;
                immutableArray.ThrowInvalidOperationIfNotInitialized();
                return immutableArray[index];
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>Gets the element at the specified index in the immutable array.</summary>
        /// <param name="index">The zero-based index of the element to get.</param>
        /// <returns>The element at the specified index in the immutable array.</returns>
        public T this[int index]
        {
            [System.Runtime.Versioning.NonVersionable]
            get
            {
                return array[index];
            }
        }

        /// <summary>Gets a value indicating whether this <see cref="T:System.Collections.Immutable.ImmutableArray`1" /> is empty.</summary>
        /// <returns>
        ///   <see langword="true" /> if the <see cref="T:System.Collections.Immutable.ImmutableArray`1" /> is empty; otherwise, <see langword="false" />.</returns>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public bool IsEmpty
        {
            [System.Runtime.Versioning.NonVersionable]
            get
            {
                return array.Length == 0;
            }
        }

        /// <summary>Gets the number of elements in the array.</summary>
        /// <returns>The number of elements in the array.</returns>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public int Length
        {
            [System.Runtime.Versioning.NonVersionable]
            get
            {
                return array.Length;
            }
        }

        /// <summary>Gets a value indicating whether this array was declared but not initialized.</summary>
        /// <returns>
        ///   <see langword="true" /> if the <see cref="T:System.Collections.Immutable.ImmutableArray`1" /> is <see langword="null" />; otherwise, <see langword="false" />.</returns>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public bool IsDefault => array == null;

        /// <summary>Gets a value indicating whether this <see cref="T:System.Collections.Immutable.ImmutableArray`1" /> is empty or is not initialized.</summary>
        /// <returns>
        ///   <see langword="true" /> if the <see cref="T:System.Collections.Immutable.ImmutableArray`1" /> is <see langword="null" /> or <see cref="F:System.Collections.Immutable.ImmutableArray`1.Empty" />; otherwise, <see langword="false" />.</returns>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public bool IsDefaultOrEmpty
        {
            get
            {
                ImmutableArray<T> immutableArray = this;
                if (immutableArray.array != null)
                {
                    return immutableArray.array.Length == 0;
                }
                return true;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        Array? IImmutableArray.Array => array;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay
        {
            get
            {
                ImmutableArray<T> immutableArray = this;
                if (!immutableArray.IsDefault)
                {
                    return $"Length = {immutableArray.Length}";
                }
                return "Uninitialized";
            }
        }

        /// <summary>Creates a new read-only span over this immutable array.</summary>
        /// <returns>The read-only span representation of this immutable array.</returns>
        public ReadOnlySpan<T> AsSpan()
        {
            return new ReadOnlySpan<T>(array);
        }

        /// <summary>Creates a new read-only memory region over this immutable array.</summary>
        /// <returns>The read-only memory representation of this immutable array.</returns>
        public ReadOnlyMemory<T> AsMemory()
        {
            return new ReadOnlyMemory<T>(array);
        }

        /// <summary>Searches the array for the specified item.</summary>
        /// <param name="item">The item to search for.</param>
        /// <returns>The zero-based index position of the item if it is found, or -1 if it is not.</returns>
        public int IndexOf(T item)
        {
            ImmutableArray<T> immutableArray = this;
            return immutableArray.IndexOf(item, 0, immutableArray.Length, EqualityComparer<T>.Default);
        }

        /// <summary>Searches the array for the specified item.</summary>
        /// <param name="item">The item to search for.</param>
        /// <param name="startIndex">The index at which to begin the search.</param>
        /// <param name="equalityComparer">The equality comparer to use in the search.</param>
        /// <returns>The zero-based index position of the item if it is found, or -1 if it is not.</returns>
        public int IndexOf(T item, int startIndex, IEqualityComparer<T>? equalityComparer)
        {
            ImmutableArray<T> immutableArray = this;
            return immutableArray.IndexOf(item, startIndex, immutableArray.Length - startIndex, equalityComparer);
        }

        /// <summary>Searches the array for the specified item.</summary>
        /// <param name="item">The item to search for.</param>
        /// <param name="startIndex">The index at which to begin the search.</param>
        /// <returns>The zero-based index position of the item if it is found, or -1 if it is not.</returns>
        public int IndexOf(T item, int startIndex)
        {
            ImmutableArray<T> immutableArray = this;
            return immutableArray.IndexOf(item, startIndex, immutableArray.Length - startIndex, EqualityComparer<T>.Default);
        }

        /// <summary>Searches the array for the specified item.</summary>
        /// <param name="item">The item to search for.</param>
        /// <param name="startIndex">The index at which to begin the search.</param>
        /// <param name="count">The number of elements to search.</param>
        /// <returns>The zero-based index position of the item if it is found, or -1 if it is not.</returns>
        public int IndexOf(T item, int startIndex, int count)
        {
            return IndexOf(item, startIndex, count, EqualityComparer<T>.Default);
        }

        /// <summary>Searches the array for the specified item.</summary>
        /// <param name="item">The item to search for.</param>
        /// <param name="startIndex">The index at which to begin the search.</param>
        /// <param name="count">The number of elements to search.</param>
        /// <param name="equalityComparer">The equality comparer to use in the search.</param>
        /// <returns>The zero-based index position of the item if it is found, or -1 if it is not.</returns>
        public int IndexOf(T item, int startIndex, int count, IEqualityComparer<T>? equalityComparer)
        {
            ImmutableArray<T> immutableArray = this;
            immutableArray.ThrowNullRefIfNotInitialized();
            if (count == 0 && startIndex == 0)
            {
                return -1;
            }
            Requires.Range(startIndex >= 0 && startIndex < immutableArray.Length, "startIndex");
            Requires.Range(count >= 0 && startIndex + count <= immutableArray.Length, "count");
            if (equalityComparer == null)
            {
                equalityComparer = EqualityComparer<T>.Default;
            }
            if (equalityComparer == EqualityComparer<T>.Default)
            {
                return Array.IndexOf<T>(immutableArray.array, item, startIndex, count);
            }
            for (int i = startIndex; i < startIndex + count; i++)
            {
                if (equalityComparer.Equals(immutableArray.array[i], item))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>Searches the array for the specified item; starting at the end of the array.</summary>
        /// <param name="item">The item to search for.</param>
        /// <returns>The 0-based index into the array where the item was found; or -1 if it could not be found.</returns>
        public int LastIndexOf(T item)
        {
            ImmutableArray<T> immutableArray = this;
            if (immutableArray.IsEmpty)
            {
                return -1;
            }
            return immutableArray.LastIndexOf(item, immutableArray.Length - 1, immutableArray.Length, EqualityComparer<T>.Default);
        }

        /// <summary>Searches the array for the specified item; starting at the end of the array.</summary>
        /// <param name="item">The item to search for.</param>
        /// <param name="startIndex">The index at which to begin the search.</param>
        /// <returns>The 0-based index into the array where the item was found; or -1 if it could not be found.</returns>
        public int LastIndexOf(T item, int startIndex)
        {
            ImmutableArray<T> immutableArray = this;
            if (immutableArray.IsEmpty && startIndex == 0)
            {
                return -1;
            }
            return immutableArray.LastIndexOf(item, startIndex, startIndex + 1, EqualityComparer<T>.Default);
        }

        /// <summary>Searches the array for the specified item; starting at the end of the array.</summary>
        /// <param name="item">The item to search for.</param>
        /// <param name="startIndex">The index at which to begin the search.</param>
        /// <param name="count">The number of elements to search.</param>
        /// <returns>The 0-based index into the array where the item was found; or -1 if it could not be found.</returns>
        public int LastIndexOf(T item, int startIndex, int count)
        {
            return LastIndexOf(item, startIndex, count, EqualityComparer<T>.Default);
        }

        /// <summary>Searches the array for the specified item; starting at the end of the array.</summary>
        /// <param name="item">The item to search for.</param>
        /// <param name="startIndex">The index at which to begin the search.</param>
        /// <param name="count">The number of elements to search.</param>
        /// <param name="equalityComparer">The equality comparer to use in the search.</param>
        /// <returns>The 0-based index into the array where the item was found; or -1 if it could not be found.</returns>
        public int LastIndexOf(T item, int startIndex, int count, IEqualityComparer<T>? equalityComparer)
        {
            ImmutableArray<T> immutableArray = this;
            immutableArray.ThrowNullRefIfNotInitialized();
            if (startIndex == 0 && count == 0)
            {
                return -1;
            }
            Requires.Range(startIndex >= 0 && startIndex < immutableArray.Length, "startIndex");
            Requires.Range(count >= 0 && startIndex - count + 1 >= 0, "count");
            if (equalityComparer == null)
            {
                equalityComparer = EqualityComparer<T>.Default;
            }
            if (equalityComparer == EqualityComparer<T>.Default)
            {
                return Array.LastIndexOf<T>(immutableArray.array, item, startIndex, count);
            }
            for (int num = startIndex; num >= startIndex - count + 1; num--)
            {
                if (equalityComparer.Equals(item, immutableArray.array[num]))
                {
                    return num;
                }
            }
            return -1;
        }

        /// <summary>Determines whether the specified item exists in the array.</summary>
        /// <param name="item">The item to search for.</param>
        /// <returns>
        ///   <see langword="true" /> if the specified item was found in the array; otherwise <see langword="false" />.</returns>
        public bool Contains(T item)
        {
            return IndexOf(item) >= 0;
        }

        /// <summary>Returns a new array with the specified value inserted at the specified position.</summary>
        /// <param name="index">The 0-based index into the array at which the new item should be added.</param>
        /// <param name="item">The item to insert at the start of the array.</param>
        /// <returns>A new array with the item inserted at the specified index.</returns>
        public ImmutableArray<T> Insert(int index, T item)
        {
            ImmutableArray<T> immutableArray = this;
            immutableArray.ThrowNullRefIfNotInitialized();
            Requires.Range(index >= 0 && index <= immutableArray.Length, "index");
            if (immutableArray.IsEmpty)
            {
                return ImmutableArray.Create(item);
            }
            T[] array = new T[immutableArray.Length + 1];
            array[index] = item;
            if (index != 0)
            {
                Array.Copy(immutableArray.array, array, index);
            }
            if (index != immutableArray.Length)
            {
                Array.Copy(immutableArray.array, index, array, index + 1, immutableArray.Length - index);
            }
            return new ImmutableArray<T>(array);
        }

        /// <summary>Inserts the specified values at the specified index.</summary>
        /// <param name="index">The index at which to insert the value.</param>
        /// <param name="items">The elements to insert.</param>
        /// <returns>A new immutable array with the items inserted at the specified index.</returns>
        public ImmutableArray<T> InsertRange(int index, IEnumerable<T> items)
        {
            ImmutableArray<T> result = this;
            result.ThrowNullRefIfNotInitialized();
            Requires.Range(index >= 0 && index <= result.Length, "index");
            Requires.NotNull(items, "items");
            if (result.IsEmpty)
            {
                return ImmutableArray.CreateRange(items);
            }
            int count = ImmutableExtensions.GetCount(ref items);
            if (count == 0)
            {
                return result;
            }
            T[] array = new T[result.Length + count];
            if (index != 0)
            {
                Array.Copy(result.array, array, index);
            }
            if (index != result.Length)
            {
                Array.Copy(result.array, index, array, index + count, result.Length - index);
            }
            if (!items.TryCopyTo(array, index))
            {
                int num = index;
                foreach (T item in items)
                {
                    array[num++] = item;
                }
            }
            return new ImmutableArray<T>(array);
        }

        /// <summary>Inserts the specified values at the specified index.</summary>
        /// <param name="index">The index at which to insert the value.</param>
        /// <param name="items">The elements to insert.</param>
        /// <returns>A new immutable array with the items inserted at the specified index.</returns>
        public ImmutableArray<T> InsertRange(int index, ImmutableArray<T> items)
        {
            ImmutableArray<T> result = this;
            result.ThrowNullRefIfNotInitialized();
            items.ThrowNullRefIfNotInitialized();
            Requires.Range(index >= 0 && index <= result.Length, "index");
            if (result.IsEmpty)
            {
                return items;
            }
            if (items.IsEmpty)
            {
                return result;
            }
            return result.InsertSpanRangeInternal(index, items.AsSpan());
        }

        /// <summary>Returns a copy of the original array with the specified item added to the end.</summary>
        /// <param name="item">The item to be added to the end of the array.</param>
        /// <returns>A new array with the specified item added to the end.</returns>
        public ImmutableArray<T> Add(T item)
        {
            ImmutableArray<T> immutableArray = this;
            if (immutableArray.IsEmpty)
            {
                return ImmutableArray.Create(item);
            }
            return immutableArray.Insert(immutableArray.Length, item);
        }

        /// <summary>Returns a copy of the original array with the specified elements added to the end of the array.</summary>
        /// <param name="items">The elements to add to the array.</param>
        /// <returns>A new array with the elements added.</returns>
        public ImmutableArray<T> AddRange(IEnumerable<T> items)
        {
            ImmutableArray<T> immutableArray = this;
            return immutableArray.InsertRange(immutableArray.Length, items);
        }

        /// <summary>Adds the specified items to the end of the array.</summary>
        /// <param name="items">The values to add.</param>
        /// <param name="length">The number of elements from the source array to add.</param>
        /// <returns>A new list with the elements added.</returns>
        public ImmutableArray<T> AddRange(T[] items, int length)
        {
            ImmutableArray<T> result = this;
            result.ThrowNullRefIfNotInitialized();
            Requires.NotNull(items, "items");
            Requires.Range(length >= 0 && length <= items.Length, "length");
            if (items.Length == 0 || length == 0)
            {
                return result;
            }
            if (result.IsEmpty)
            {
                return ImmutableArray.Create(items, 0, length);
            }
            T[] array = new T[result.Length + length];
            Array.Copy(result.array, array, result.Length);
            Array.Copy(items, 0, array, result.Length, length);
            return new ImmutableArray<T>(array);
        }

        /// <summary>Adds the specified items to the end of the array.</summary>
        /// <param name="items">The values to add.</param>
        /// <typeparam name="TDerived">The type that derives from the type of item already in the array.</typeparam>
        /// <returns>A new list with the elements added.</returns>
        public ImmutableArray<T> AddRange<TDerived>(TDerived[] items) where TDerived : T
        {
            ImmutableArray<T> result = this;
            result.ThrowNullRefIfNotInitialized();
            Requires.NotNull(items, "items");
            if (items.Length == 0)
            {
                return result;
            }
            T[] array = new T[result.Length + items.Length];
            Array.Copy(result.array, array, result.Length);
            Array.Copy(items, 0, array, result.Length, items.Length);
            return new ImmutableArray<T>(array);
        }

        /// <summary>Adds the specified items to the end of the array.</summary>
        /// <param name="items">The values to add.</param>
        /// <param name="length">The number of elements from the source array to add.</param>
        /// <returns>A new list with the elements added.</returns>
        public ImmutableArray<T> AddRange(ImmutableArray<T> items, int length)
        {
            ImmutableArray<T> result = this;
            Requires.Range(length >= 0, "length");
            if (items.array != null)
            {
                return result.AddRange(items.array, length);
            }
            return result;
        }

        /// <summary>Adds the specified items to the end of the array.</summary>
        /// <param name="items">The values to add.</param>
        /// <typeparam name="TDerived">The type that derives from the type of item already in the array.</typeparam>
        /// <returns>A new list with the elements added.</returns>
        public ImmutableArray<T> AddRange<TDerived>(ImmutableArray<TDerived> items) where TDerived : T
        {
            ImmutableArray<T> result = this;
            if (items.array != null)
            {
                return result.AddRange<TDerived>(items.array);
            }
            return result;
        }

        /// <summary>Returns a copy of the original array with the specified elements added to the end of the array.</summary>
        /// <param name="items">The elements to add to the array.</param>
        /// <returns>A new array with the elements added.</returns>
        public ImmutableArray<T> AddRange(ImmutableArray<T> items)
        {
            ImmutableArray<T> immutableArray = this;
            return immutableArray.InsertRange(immutableArray.Length, items);
        }

        /// <summary>Replaces the item at the specified index with the specified item.</summary>
        /// <param name="index">The index of the item to replace.</param>
        /// <param name="item">The item to add to the list.</param>
        /// <returns>The new array that contains <paramref name="item" /> at the specified index.</returns>
        public ImmutableArray<T> SetItem(int index, T item)
        {
            ImmutableArray<T> immutableArray = this;
            immutableArray.ThrowNullRefIfNotInitialized();
            Requires.Range(index >= 0 && index < immutableArray.Length, "index");
            T[] array = new T[immutableArray.Length];
            Array.Copy(immutableArray.array, array, immutableArray.Length);
            array[index] = item;
            return new ImmutableArray<T>(array);
        }

        /// <summary>Finds the first element in the array equal to the specified value and replaces the value with the specified new value.</summary>
        /// <param name="oldValue">The value to find and replace in the array.</param>
        /// <param name="newValue">The value to replace the <c>oldvalue</c> with.</param>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="oldValue" /> is not found in the array.</exception>
        /// <returns>A new array that contains <paramref name="newValue" /> even if the new and old values are the same.</returns>
        public ImmutableArray<T> Replace(T oldValue, T newValue)
        {
            return Replace(oldValue, newValue, EqualityComparer<T>.Default);
        }

        /// <summary>Finds the first element in the array equal to the specified value and replaces the value with the specified new value.</summary>
        /// <param name="oldValue">The value to find and replace in the array.</param>
        /// <param name="newValue">The value to replace the <c>oldvalue</c> with.</param>
        /// <param name="equalityComparer">The equality comparer to use to compare values.</param>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="oldValue" /> is not found in the array.</exception>
        /// <returns>A new array that contains <paramref name="newValue" /> even if the new and old values are the same.</returns>
        public ImmutableArray<T> Replace(T oldValue, T newValue, IEqualityComparer<T>? equalityComparer)
        {
            ImmutableArray<T> immutableArray = this;
            int num = immutableArray.IndexOf(oldValue, 0, immutableArray.Length, equalityComparer);
            if (num < 0)
            {
                throw new ArgumentException(MDCFR.Properties.Resources.CannotFindOldValue, "oldValue");
            }
            return immutableArray.SetItem(num, newValue);
        }

        /// <summary>Returns an array with the first occurrence of the specified element removed from the array. If no match is found, the current array is returned.</summary>
        /// <param name="item">The item to remove.</param>
        /// <returns>A new array with the item removed.</returns>
        public ImmutableArray<T> Remove(T item)
        {
            return Remove(item, EqualityComparer<T>.Default);
        }

        /// <summary>Returns an array with the first occurrence of the specified element removed from the array.  
        ///
        ///  If no match is found, the current array is returned.</summary>
        /// <param name="item">The item to remove.</param>
        /// <param name="equalityComparer">The equality comparer to use in the search.</param>
        /// <returns>A new array with the specified item removed.</returns>
        public ImmutableArray<T> Remove(T item, IEqualityComparer<T>? equalityComparer)
        {
            ImmutableArray<T> result = this;
            result.ThrowNullRefIfNotInitialized();
            int num = result.IndexOf(item, 0, result.Length, equalityComparer);
            if (num >= 0)
            {
                return result.RemoveAt(num);
            }
            return result;
        }

        /// <summary>Returns an array with the element at the specified position removed.</summary>
        /// <param name="index">The 0-based index of the element to remove from the returned array.</param>
        /// <returns>A new array with the item at the specified index removed.</returns>
        public ImmutableArray<T> RemoveAt(int index)
        {
            return RemoveRange(index, 1);
        }

        /// <summary>Returns an array with the elements at the specified position removed.</summary>
        /// <param name="index">The 0-based index of the starting element to remove from the array.</param>
        /// <param name="length">The number of elements to remove from the array.</param>
        /// <returns>The new array with the specified elements removed.</returns>
        public ImmutableArray<T> RemoveRange(int index, int length)
        {
            ImmutableArray<T> result = this;
            result.ThrowNullRefIfNotInitialized();
            Requires.Range(index >= 0 && index <= result.Length, "index");
            Requires.Range(length >= 0 && index + length <= result.Length, "length");
            if (length == 0)
            {
                return result;
            }
            T[] array = new T[result.Length - length];
            Array.Copy(result.array, array, index);
            Array.Copy(result.array, index + length, array, index, result.Length - index - length);
            return new ImmutableArray<T>(array);
        }

        /// <summary>Removes the specified items from this array.</summary>
        /// <param name="items">The items to remove if matches are found in this list.</param>
        /// <returns>A new array with the elements removed.</returns>
        public ImmutableArray<T> RemoveRange(IEnumerable<T> items)
        {
            return RemoveRange(items, EqualityComparer<T>.Default);
        }

        /// <summary>Removes the specified items from this array.</summary>
        /// <param name="items">The items to remove if matches are found in this list.</param>
        /// <param name="equalityComparer">The equality comparer to use in the search.</param>
        /// <returns>A new array with the elements removed.</returns>
        public ImmutableArray<T> RemoveRange(IEnumerable<T> items, IEqualityComparer<T>? equalityComparer)
        {
            ImmutableArray<T> immutableArray = this;
            immutableArray.ThrowNullRefIfNotInitialized();
            Requires.NotNull(items, "items");
            SortedSet<int> sortedSet = new SortedSet<int>();
            foreach (T item in items)
            {
                int num = -1;
                do
                {
                    num = immutableArray.IndexOf(item, num + 1, equalityComparer);
                }
                while (num >= 0 && !sortedSet.Add(num) && num < immutableArray.Length - 1);
            }
            return immutableArray.RemoveAtRange(sortedSet);
        }

        /// <summary>Removes the specified values from this list.</summary>
        /// <param name="items">The items to remove if matches are found in this list.</param>
        /// <returns>A new list with the elements removed.</returns>
        public ImmutableArray<T> RemoveRange(ImmutableArray<T> items)
        {
            return RemoveRange(items, EqualityComparer<T>.Default);
        }

        /// <summary>Removes the specified items from this list.</summary>
        /// <param name="items">The items to remove if matches are found in this list.</param>
        /// <param name="equalityComparer">The equality comparer to use in the search.</param>
        /// <returns>A new array with the elements removed.</returns>
        public ImmutableArray<T> RemoveRange(ImmutableArray<T> items, IEqualityComparer<T>? equalityComparer)
        {
#pragma warning disable CS8634 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'class' constraint.
            Requires.NotNull(items.array, "items");
#pragma warning restore CS8634 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'class' constraint.
            return RemoveRange(items.AsSpan(), equalityComparer);
        }

        /// <summary>Removes all the items from the array that meet the specified condition.</summary>
        /// <param name="match">The delegate that defines the conditions of the elements to remove.</param>
        /// <returns>A new array with items that meet the specified condition removed.</returns>
        public ImmutableArray<T> RemoveAll(Predicate<T> match)
        {
            ImmutableArray<T> result = this;
            result.ThrowNullRefIfNotInitialized();
            Requires.NotNull(match, "match");
            if (result.IsEmpty)
            {
                return result;
            }
            List<int> list = null;
            for (int i = 0; i < result.array.Length; i++)
            {
                if (match(result.array[i]))
                {
                    if (list == null)
                    {
                        list = new List<int>();
                    }
                    list.Add(i);
                }
            }
            if (list == null)
            {
                return result;
            }
            return result.RemoveAtRange(list);
        }

        /// <summary>Returns an array with all the elements removed.</summary>
        /// <returns>An array with all of the elements removed.</returns>
        public ImmutableArray<T> Clear()
        {
            return Empty;
        }

        /// <summary>Sorts the elements in the immutable array using the default comparer.</summary>
        /// <returns>A new immutable array that contains the items in this array, in sorted order.</returns>
        public ImmutableArray<T> Sort()
        {
            ImmutableArray<T> immutableArray = this;
            return immutableArray.Sort(0, immutableArray.Length, Comparer<T>.Default);
        }

        /// <summary>Sorts the elements in the entire <see cref="T:System.Collections.Immutable.ImmutableArray`1" /> using             the specified <see cref="T:System.Comparison`1" />.</summary>
        /// <param name="comparison">The <see cref="T:System.Comparison`1" /> to use when comparing elements.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="comparison" /> is null.</exception>
        /// <returns>The sorted list.</returns>
        public ImmutableArray<T> Sort(Comparison<T> comparison)
        {
            Requires.NotNull(comparison, "comparison");
            ImmutableArray<T> immutableArray = this;
            return immutableArray.Sort(Comparer<T>.Create(comparison));
        }

        /// <summary>Sorts the elements in the immutable array using the specified comparer.</summary>
        /// <param name="comparer">The implementation to use when comparing elements, or <see langword="null" /> to use the default comparer.</param>
        /// <returns>A new immutable array that contains the items in this array, in sorted order.</returns>
        public ImmutableArray<T> Sort(IComparer<T>? comparer)
        {
            ImmutableArray<T> immutableArray = this;
            return immutableArray.Sort(0, immutableArray.Length, comparer);
        }

        /// <summary>Sorts the specified elements in the immutable array using the specified comparer.</summary>
        /// <param name="index">The index of the first element to sort.</param>
        /// <param name="count">The number of elements to include in the sort.</param>
        /// <param name="comparer">The implementation to use when comparing elements, or <see langword="null" /> to use the default comparer.</param>
        /// <returns>A new immutable array that contains the items in this array, in sorted order.</returns>
        public ImmutableArray<T> Sort(int index, int count, IComparer<T>? comparer)
        {
            ImmutableArray<T> result = this;
            result.ThrowNullRefIfNotInitialized();
            Requires.Range(index >= 0, "index");
            Requires.Range(count >= 0 && index + count <= result.Length, "count");
            if (count > 1)
            {
                if (comparer == null)
                {
                    comparer = Comparer<T>.Default;
                }
                bool flag = false;
                for (int i = index + 1; i < index + count; i++)
                {
                    if (comparer.Compare(result.array[i - 1], result.array[i]) > 0)
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag)
                {
                    T[] array = new T[result.Length];
                    Array.Copy(result.array, array, result.Length);
                    Array.Sort(array, index, count, comparer);
                    return new ImmutableArray<T>(array);
                }
            }
            return result;
        }

        /// <summary>Filters the elements of this array to those assignable to the specified type.</summary>
        /// <typeparam name="TResult">The type to filter the elements of the sequence on.</typeparam>
        /// <returns>An <see cref="System.Collections.IEnumerable" /> that contains elements from the input sequence of type of TResult.</returns>
        public IEnumerable<TResult> OfType<TResult>()
        {
            ImmutableArray<T> immutableArray = this;
            if (immutableArray.array == null || immutableArray.array.Length == 0)
            {
                return Enumerable.Empty<TResult>();
            }
            return immutableArray.array.OfType<TResult>();
        }

        /// <summary>Adds the specified values to this list.</summary>
        /// <param name="items">The values to add.</param>
        /// <returns>A new list with the elements added.</returns>
        public ImmutableArray<T> AddRange(ReadOnlySpan<T> items)
        {
            ImmutableArray<T> immutableArray = this;
            return immutableArray.InsertRange(immutableArray.Length, items);
        }

        /// <summary>Adds the specified values to this list.</summary>
        /// <param name="items">The values to add.</param>
        /// <returns>A new list with the elements added.</returns>
        public ImmutableArray<T> AddRange(params T[] items)
        {
            ImmutableArray<T> immutableArray = this;
            return immutableArray.InsertRange(immutableArray.Length, items);
        }

        /// <summary>Creates a <see cref="T:System.ReadOnlySpan`1" /> over the portion of the current <see cref="T:System.Collections.Immutable.ImmutableArray`1" />, beginning at a specified position for a specified length.</summary>
        /// <param name="start">The index at which to begin the span.</param>
        /// <param name="length">The number of items in the span.</param>
        /// <returns>The <see cref="T:System.ReadOnlySpan`1" /> representation of the <see cref="T:System.Collections.Immutable.ImmutableArray`1" />.</returns>
        public ReadOnlySpan<T> AsSpan(int start, int length)
        {
            return new ReadOnlySpan<T>(array, start, length);
        }

        /// <summary>Copies the elements of current <see cref="T:System.Collections.Immutable.ImmutableArray`1" /> to a <see cref="T:System.Span`1" />.</summary>
        /// <param name="destination">The <see cref="T:System.Span`1" /> that is the destination of the elements copied from current <see cref="T:System.Collections.Immutable.ImmutableArray`1" />.</param>
        public void CopyTo(Span<T> destination)
        {
            ImmutableArray<T> immutableArray = this;
            immutableArray.ThrowNullRefIfNotInitialized();
            Requires.Range(immutableArray.Length <= destination.Length, "destination");
            immutableArray.AsSpan().CopyTo(destination);
        }

        /// <summary>Inserts the specified values at the specified index.</summary>
        /// <param name="index">The index at which to insert the value.</param>
        /// <param name="items">The elements to insert.</param>
        /// <returns>The new immutable collection.</returns>
        public ImmutableArray<T> InsertRange(int index, T[] items)
        {
            ImmutableArray<T> result = this;
            result.ThrowNullRefIfNotInitialized();
            Requires.Range(index >= 0 && index <= result.Length, "index");
            Requires.NotNull(items, "items");
            if (items.Length == 0)
            {
                return result;
            }
            if (result.IsEmpty)
            {
                return new ImmutableArray<T>(items);
            }
            return result.InsertSpanRangeInternal(index, items);
        }

        /// <summary>Inserts the specified values at the specified index.</summary>
        /// <param name="index">The index at which to insert the value.</param>
        /// <param name="items">The elements to insert.</param>
        /// <returns>The new immutable collection.</returns>
        public ImmutableArray<T> InsertRange(int index, ReadOnlySpan<T> items)
        {
            ImmutableArray<T> result = this;
            result.ThrowNullRefIfNotInitialized();
            Requires.Range(index >= 0 && index <= result.Length, "index");
            if (items.IsEmpty)
            {
                return result;
            }
            if (result.IsEmpty)
            {
                return items.ToImmutableArray();
            }
            return result.InsertSpanRangeInternal(index, items);
        }

        /// <summary>Removes the specified values from this list.</summary>
        /// <param name="items">The items to remove if matches are found in this list.</param>
        /// <param name="equalityComparer">The equality comparer to use in the search.</param>
        /// <returns>A new list with the elements removed.</returns>
        public ImmutableArray<T> RemoveRange(ReadOnlySpan<T> items, IEqualityComparer<T>? equalityComparer = null)
        {
            ImmutableArray<T> result = this;
            result.ThrowNullRefIfNotInitialized();
            if (items.IsEmpty || result.IsEmpty)
            {
                return result;
            }
            if (items.Length == 1)
            {
                return result.Remove(items[0], equalityComparer);
            }
            SortedSet<int> sortedSet = new SortedSet<int>();
            ReadOnlySpan<T> readOnlySpan = items;
            for (int i = 0; i < readOnlySpan.Length; i++)
            {
                T item = readOnlySpan[i];
                int num = -1;
                do
                {
                    num = result.IndexOf(item, num + 1, equalityComparer);
                }
                while (num >= 0 && !sortedSet.Add(num) && num < result.Length - 1);
            }
            return result.RemoveAtRange(sortedSet);
        }

        /// <summary>Removes the specified values from this list.</summary>
        /// <param name="items">The items to remove if matches are found in this list.</param>
        /// <param name="equalityComparer">The equality comparer to use in the search.</param>
        /// <returns>A new list with the elements removed.</returns>
        public ImmutableArray<T> RemoveRange(T[] items, IEqualityComparer<T>? equalityComparer = null)
        {
            ImmutableArray<T> immutableArray = this;
            immutableArray.ThrowNullRefIfNotInitialized();
            Requires.NotNull(items, "items");
            return immutableArray.RemoveRange(new ReadOnlySpan<T>(items), equalityComparer);
        }

        /// <summary>Forms a slice out of the current <see cref="T:System.Collections.Immutable.ImmutableArray`1" /> starting at a specified index for a specified length.</summary>
        /// <param name="start">The index at which to begin this slice.</param>
        /// <param name="length">The desired length for the slice.</param>
        /// <returns>An <see cref="T:System.Collections.Immutable.ImmutableArray`1" /> that consists of <paramref name="length" /> elements from the current <see cref="T:System.Collections.Immutable.ImmutableArray`1" />, starting at <paramref name="start" />.</returns>
        public ImmutableArray<T> Slice(int start, int length)
        {
            ImmutableArray<T> items = this;
            items.ThrowNullRefIfNotInitialized();
            return ImmutableArray.Create(items, start, length);
        }

        /// <summary>Throws <see cref="T:System.NotSupportedException" /> in all cases.</summary>
        /// <param name="index">The index of the location to insert the item.</param>
        /// <param name="item">The item to insert.</param>
        void IList<T>.Insert(int index, T item)
        {
            throw new NotSupportedException();
        }

        /// <summary>Throws <see cref="T:System.NotSupportedException" /> in all cases.</summary>
        /// <param name="index">The index.</param>
        void IList<T>.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        /// <summary>Throws <see cref="T:System.NotSupportedException" /> in all cases.</summary>
        /// <param name="item">The item to add to the end of the array.</param>
        void ICollection<T>.Add(T item)
        {
            throw new NotSupportedException();
        }

        /// <summary>Throws <see cref="T:System.NotSupportedException" /> in all cases.</summary>
        void ICollection<T>.Clear()
        {
            throw new NotSupportedException();
        }

        /// <summary>Throws <see cref="T:System.NotSupportedException" /> in all cases.</summary>
        /// <param name="item">The object to remove from the array.</param>
        /// <returns>Throws <see cref="T:System.NotSupportedException" /> in all cases.</returns>
        bool ICollection<T>.Remove(T item)
        {
            throw new NotSupportedException();
        }

        /// <summary>Returns an array with all the elements removed.</summary>
        /// <returns>An array with all the elements removed.</returns>
        IImmutableList<T> IImmutableList<T>.Clear()
        {
            ImmutableArray<T> immutableArray = this;
            immutableArray.ThrowInvalidOperationIfNotInitialized();
            return immutableArray.Clear();
        }

        /// <summary>Returns a copy of the original array with the specified item added to the end.</summary>
        /// <param name="value">The value to add to the end of the array.</param>
        /// <returns>A new array with the specified item added to the end.</returns>
        IImmutableList<T> IImmutableList<T>.Add(T value)
        {
            ImmutableArray<T> immutableArray = this;
            immutableArray.ThrowInvalidOperationIfNotInitialized();
            return immutableArray.Add(value);
        }

        /// <summary>Returns a copy of the original array with the specified elements added to the end of the array.</summary>
        /// <param name="items">The elements to add to the end of the array.</param>
        /// <returns>A new array with the elements added to the end.</returns>
        IImmutableList<T> IImmutableList<T>.AddRange(IEnumerable<T> items)
        {
            ImmutableArray<T> immutableArray = this;
            immutableArray.ThrowInvalidOperationIfNotInitialized();
            return immutableArray.AddRange(items);
        }

        /// <summary>Returns a new array with the specified value inserted at the specified position.</summary>
        /// <param name="index">The 0-based index into the array at which the new item should be added.</param>
        /// <param name="element">The item to insert at the start of the array.</param>
        /// <returns>A new array with the specified value inserted.</returns>
        IImmutableList<T> IImmutableList<T>.Insert(int index, T element)
        {
            ImmutableArray<T> immutableArray = this;
            immutableArray.ThrowInvalidOperationIfNotInitialized();
            return immutableArray.Insert(index, element);
        }

        /// <summary>Inserts the specified values at the specified index.</summary>
        /// <param name="index">The index at which to insert the value.</param>
        /// <param name="items">The elements to insert.</param>
        /// <returns>A new array with the specified values inserted.</returns>
        IImmutableList<T> IImmutableList<T>.InsertRange(int index, IEnumerable<T> items)
        {
            ImmutableArray<T> immutableArray = this;
            immutableArray.ThrowInvalidOperationIfNotInitialized();
            return immutableArray.InsertRange(index, items);
        }

        /// <summary>Returns an array with the first occurrence of the specified element removed from the array; if no match is found, the current array is returned.</summary>
        /// <param name="value">The value to remove from the array.</param>
        /// <param name="equalityComparer">The equality comparer to use in the search.</param>
        /// <returns>A new array with the value removed.</returns>
        IImmutableList<T> IImmutableList<T>.Remove(T value, IEqualityComparer<T> equalityComparer)
        {
            ImmutableArray<T> immutableArray = this;
            immutableArray.ThrowInvalidOperationIfNotInitialized();
            return immutableArray.Remove(value, equalityComparer);
        }

        /// <summary>Removes all the items from the array that meet the specified condition.</summary>
        /// <param name="match">The delegate that defines the conditions of the elements to remove.</param>
        /// <returns>A new array with items that meet the specified condition removed.</returns>
        IImmutableList<T> IImmutableList<T>.RemoveAll(Predicate<T> match)
        {
            ImmutableArray<T> immutableArray = this;
            immutableArray.ThrowInvalidOperationIfNotInitialized();
            return immutableArray.RemoveAll(match);
        }

        /// <summary>Removes the specified items from this array.</summary>
        /// <param name="items">The items to remove if matches are found in this list.</param>
        /// <param name="equalityComparer">The equality comparer to use in the search.</param>
        /// <returns>A new array with the elements removed.</returns>
        IImmutableList<T> IImmutableList<T>.RemoveRange(IEnumerable<T> items, IEqualityComparer<T> equalityComparer)
        {
            ImmutableArray<T> immutableArray = this;
            immutableArray.ThrowInvalidOperationIfNotInitialized();
            return immutableArray.RemoveRange(items, equalityComparer);
        }

        /// <summary>Returns an array with the elements at the specified position removed.</summary>
        /// <param name="index">The 0-based index of the starting element to remove from the array.</param>
        /// <param name="count">The number of elements to remove from the array.</param>
        /// <returns>The new array with the specified elements removed.</returns>
        IImmutableList<T> IImmutableList<T>.RemoveRange(int index, int count)
        {
            ImmutableArray<T> immutableArray = this;
            immutableArray.ThrowInvalidOperationIfNotInitialized();
            return immutableArray.RemoveRange(index, count);
        }

        /// <summary>Returns an array with the element at the specified position removed.</summary>
        /// <param name="index">The 0-based index of the element to remove from the returned array.</param>
        /// <returns>A new array with the specified item removed.</returns>
        IImmutableList<T> IImmutableList<T>.RemoveAt(int index)
        {
            ImmutableArray<T> immutableArray = this;
            immutableArray.ThrowInvalidOperationIfNotInitialized();
            return immutableArray.RemoveAt(index);
        }

        /// <summary>Replaces the item at the specified index with the specified item.</summary>
        /// <param name="index">The index of the item to replace.</param>
        /// <param name="value">The value to add to the list.</param>
        /// <returns>The new array that contains the item at the specified index.</returns>
        IImmutableList<T> IImmutableList<T>.SetItem(int index, T value)
        {
            ImmutableArray<T> immutableArray = this;
            immutableArray.ThrowInvalidOperationIfNotInitialized();
            return immutableArray.SetItem(index, value);
        }

        /// <summary>Finds the first element in the array equal to the specified value and replaces the value with the specified new value.</summary>
        /// <param name="oldValue">The value to find and replace in the array.</param>
        /// <param name="newValue">The value to replace the <c>oldvalue</c> with.</param>
        /// <param name="equalityComparer">The equality comparer to use to compare values.</param>
        /// <exception cref="System.ArgumentException">
        ///   <paramref name="oldValue" /> is not found in the array.</exception>
        /// <returns>A new array that contains <paramref name="newValue" /> even if the new and old values are the same.</returns>
        IImmutableList<T> IImmutableList<T>.Replace(T oldValue, T newValue, IEqualityComparer<T> equalityComparer)
        {
            ImmutableArray<T> immutableArray = this;
            immutableArray.ThrowInvalidOperationIfNotInitialized();
            return immutableArray.Replace(oldValue, newValue, equalityComparer);
        }

        /// <summary>Throws <see cref="T:System.NotSupportedException" /> in all cases.</summary>
        /// <param name="value">The value to add to the array.</param>
        /// <exception cref="T:System.NotSupportedException">Thrown in all cases.</exception>
        /// <returns>Throws <see cref="T:System.NotSupportedException" /> in all cases.</returns>
        int IList.Add(object value)
        {
            throw new NotSupportedException();
        }

        /// <summary>Throws <see cref="T:System.NotSupportedException" /> in all cases.</summary>
        /// <exception cref="T:System.NotSupportedException">Thrown in all cases.</exception>
        void IList.Clear()
        {
            throw new NotSupportedException();
        }

        /// <summary>Throws <see cref="T:System.NotSupportedException" /> in all cases.</summary>
        /// <param name="value">The value to check for.</param>
        /// <returns>Throws <see cref="T:System.NotSupportedException" /> in all cases.</returns>
        bool IList.Contains(object value)
        {
            ImmutableArray<T> immutableArray = this;
            immutableArray.ThrowInvalidOperationIfNotInitialized();
            return immutableArray.Contains((T)value);
        }

        /// <summary>Gets the value at the specified index.</summary>
        /// <param name="value">The value to return the index of.</param>
        /// <returns>The value of the element at the specified index.</returns>
        int IList.IndexOf(object value)
        {
            ImmutableArray<T> immutableArray = this;
            immutableArray.ThrowInvalidOperationIfNotInitialized();
            return immutableArray.IndexOf((T)value);
        }

        /// <summary>Throws <see cref="T:System.NotSupportedException" /> in all cases.</summary>
        /// <param name="index">Index that indicates where to insert the item.</param>
        /// <param name="value">The value to insert.</param>
        /// <exception cref="T:System.NotSupportedException">Thrown in all cases.</exception>
        void IList.Insert(int index, object value)
        {
            throw new NotSupportedException();
        }

        /// <summary>Throws <see cref="T:System.NotSupportedException" /> in all cases.</summary>
        /// <param name="value">The value to remove from the array.</param>
        /// <exception cref="T:System.NotSupportedException">Thrown in all cases.</exception>
        void IList.Remove(object value)
        {
            throw new NotSupportedException();
        }

        /// <summary>Throws <see cref="T:System.NotSupportedException" /> in all cases.</summary>
        /// <param name="index">The index of the item to remove.</param>
        /// <exception cref="T:System.NotSupportedException">Thrown in all cases.</exception>
        void IList.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        /// <summary>Copies this array to another array starting at the specified index.</summary>
        /// <param name="array">The array to copy this array to.</param>
        /// <param name="index">The index in the destination array to start the copy operation.</param>
        void ICollection.CopyTo(Array array, int index)
        {
            ImmutableArray<T> immutableArray = this;
            immutableArray.ThrowInvalidOperationIfNotInitialized();
            Array.Copy(immutableArray.array, 0, array, index, immutableArray.Length);
        }

        /// <summary>Determines whether this array is structurally equal to the specified array.</summary>
        /// <param name="other">The array to compare with the current instance.</param>
        /// <param name="comparer">An object that determines whether the current instance and other are structurally equal.</param>
        /// <returns>
        ///   <see langword="true" /> if the two arrays are structurally equal; otherwise, <see langword="false" />.</returns>
        bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
        {
            ImmutableArray<T> immutableArray = this;
            Array array = other as Array;
            if (array == null && other is IImmutableArray immutableArray2)
            {
                array = immutableArray2.Array;
                if (immutableArray.array == null && array == null)
                {
                    return true;
                }
                if (immutableArray.array == null)
                {
                    return false;
                }
            }
            IStructuralEquatable structuralEquatable = immutableArray.array;
            return structuralEquatable.Equals(array, comparer);
        }

        /// <summary>Returns a hash code for the current instance.</summary>
        /// <param name="comparer">An object that computes the hash code of the current object.</param>
        /// <returns>The hash code for the current instance.</returns>
        int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
        {
            ImmutableArray<T> immutableArray = this;
            return ((IStructuralEquatable)immutableArray.array)?.GetHashCode(comparer) ?? immutableArray.GetHashCode();
        }

        /// <summary>Determines whether the current collection element precedes, occurs in the same position as, or follows another element in the sort order.</summary>
        /// <param name="other">The element to compare with the current instance.</param>
        /// <param name="comparer">The object used to compare members of the current array with the corresponding members of other array.</param>
        /// <exception cref="T:System.ArgumentException">The arrays are not the same length.</exception>
        /// <returns>An integer that indicates whether the current element precedes, is in the same position or follows the other element.</returns>
        int IStructuralComparable.CompareTo(object other, IComparer comparer)
        {
            ImmutableArray<T> immutableArray = this;
            Array array = other as Array;
            if (array == null && other is IImmutableArray immutableArray2)
            {
                array = immutableArray2.Array;
                if (immutableArray.array == null && array == null)
                {
                    return 0;
                }
                if ((immutableArray.array == null) ^ (array == null))
                {
                    throw new ArgumentException(MDCFR.Properties.Resources.ArrayInitializedStateNotEqual, "other");
                }
            }
            if (array != null)
            {
                IStructuralComparable structuralComparable = immutableArray.array;
                if (structuralComparable == null)
                {
                    throw new ArgumentException(MDCFR.Properties.Resources.ArrayInitializedStateNotEqual, "other");
                }
                return structuralComparable.CompareTo(array, comparer);
            }
            throw new ArgumentException(MDCFR.Properties.Resources.ArrayLengthsNotEqual, "other");
        }

        private ImmutableArray<T> RemoveAtRange(ICollection<int> indicesToRemove)
        {
            ImmutableArray<T> result = this;
            result.ThrowNullRefIfNotInitialized();
            Requires.NotNull(indicesToRemove, "indicesToRemove");
            if (indicesToRemove.Count == 0)
            {
                return result;
            }
            T[] array = new T[result.Length - indicesToRemove.Count];
            int num = 0;
            int num2 = 0;
            int num3 = -1;
            foreach (int item in indicesToRemove)
            {
                int num4 = ((num3 == -1) ? item : (item - num3 - 1));
                Array.Copy(result.array, num + num2, array, num, num4);
                num2++;
                num += num4;
                num3 = item;
            }
            Array.Copy(result.array, num + num2, array, num, result.Length - (num + num2));
            return new ImmutableArray<T>(array);
        }

        private ImmutableArray<T> InsertSpanRangeInternal(int index, ReadOnlySpan<T> items)
        {
            T[] array = new T[Length + items.Length];
            if (index != 0)
            {
                Array.Copy(this.array, array, index);
            }
            items.CopyTo(new Span<T>(array, index, items.Length));
            if (index != Length)
            {
                Array.Copy(this.array, index, array, index + items.Length, Length - index);
            }
            return new ImmutableArray<T>(array);
        }

        internal ImmutableArray(T[]? items)
        {
            array = items;
        }

        /// <summary>Returns a value that indicates if two arrays are equal.</summary>
        /// <param name="left">The array to the left of the operator.</param>
        /// <param name="right">The array to the right of the operator.</param>
        /// <returns>
        ///   <see langword="true" /> if the arrays are equal; otherwise, <see langword="false" />.</returns>
        [System.Runtime.Versioning.NonVersionable]
        public static bool operator ==(ImmutableArray<T> left, ImmutableArray<T> right)
        {
            return left.Equals(right);
        }

        /// <summary>Returns a value that indicates whether two arrays are not equal.</summary>
        /// <param name="left">The array to the left of the operator.</param>
        /// <param name="right">The array to the right of the operator.</param>
        /// <returns>
        ///   <see langword="true" /> if the arrays are not equal; otherwise, <see langword="false" />.</returns>
        [System.Runtime.Versioning.NonVersionable]
        public static bool operator !=(ImmutableArray<T> left, ImmutableArray<T> right)
        {
            return !left.Equals(right);
        }

        /// <summary>Returns a value that indicates if two arrays are equal.</summary>
        /// <param name="left">The array to the left of the operator.</param>
        /// <param name="right">The array to the right of the operator.</param>
        /// <returns>
        ///   <see langword="true" /> if the arrays are equal; otherwise, <see langword="false" />.</returns>
        public static bool operator ==(ImmutableArray<T>? left, ImmutableArray<T>? right)
        {
            return left.GetValueOrDefault().Equals(right.GetValueOrDefault());
        }

        /// <summary>Checks for inequality between two array.</summary>
        /// <param name="left">The object to the left of the operator.</param>
        /// <param name="right">The object to the right of the operator.</param>
        /// <returns>
        ///   <see langword="true" /> if the two arrays are not equal; otherwise, <see langword="false" />.</returns>
        public static bool operator !=(ImmutableArray<T>? left, ImmutableArray<T>? right)
        {
            return !left.GetValueOrDefault().Equals(right.GetValueOrDefault());
        }

        /// <summary>Gets a read-only reference to the element at the specified <paramref name="index" /> in the read-only list.</summary>
        /// <param name="index">The zero-based index of the element to get a reference to.</param>
        /// <returns>A read-only reference to the element at the specified <paramref name="index" /> in the read-only list.</returns>
        public ref readonly T ItemRef(int index)
        {
            return ref array[index];
        }

        /// <summary>Copies the contents of this array to the specified array.</summary>
        /// <param name="destination">The array to copy to.</param>
        public void CopyTo(T[] destination)
        {
            ImmutableArray<T> immutableArray = this;
            immutableArray.ThrowNullRefIfNotInitialized();
            Array.Copy(immutableArray.array, destination, immutableArray.Length);
        }

        /// <summary>Copies the contents of this array to the specified array starting at the specified destination index.</summary>
        /// <param name="destination">The array to copy to.</param>
        /// <param name="destinationIndex">The index in the array where copying begins.</param>
        public void CopyTo(T[] destination, int destinationIndex)
        {
            ImmutableArray<T> immutableArray = this;
            immutableArray.ThrowNullRefIfNotInitialized();
            Array.Copy(immutableArray.array, 0, destination, destinationIndex, immutableArray.Length);
        }

        /// <summary>Copies the specified items in this array to the specified array at the specified starting index.</summary>
        /// <param name="sourceIndex">The index of this array where copying begins.</param>
        /// <param name="destination">The array to copy to.</param>
        /// <param name="destinationIndex">The index in <paramref name="destination" /> where copying begins.</param>
        /// <param name="length">The number of elements to copy from this array.</param>
        public void CopyTo(int sourceIndex, T[] destination, int destinationIndex, int length)
        {
            ImmutableArray<T> immutableArray = this;
            immutableArray.ThrowNullRefIfNotInitialized();
            Array.Copy(immutableArray.array, sourceIndex, destination, destinationIndex, length);
        }

        /// <summary>Creates a mutable array that has the same contents as this array and can be efficiently mutated across multiple operations using standard mutable interfaces.</summary>
        /// <returns>The new builder with the same contents as this array.</returns>
        public ImmutableArray<T>.Builder ToBuilder()
        {
            ImmutableArray<T> items = this;
            if (items.Length == 0)
            {
                return new Builder();
            }
            Builder builder = new Builder(items.Length);
            builder.AddRange(items);
            return builder;
        }

        /// <summary>Returns an enumerator that iterates through the contents of the array.</summary>
        /// <returns>An enumerator.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator()
        {
            ImmutableArray<T> immutableArray = this;
            immutableArray.ThrowNullRefIfNotInitialized();
            return new Enumerator(immutableArray.array);
        }

        /// <summary>Returns a hash code for this instance.</summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            ImmutableArray<T> immutableArray = this;
            if (immutableArray.array != null)
            {
                return immutableArray.array.GetHashCode();
            }
            return 0;
        }

        /// <summary>Determines if this array is equal to the specified object.</summary>
        /// <param name="obj">The <see cref="T:System.Object" /> to compare with this array.</param>
        /// <returns>
        ///   <see langword="true" /> if this array is equal to <paramref name="obj" />; otherwise, <see langword="false" />.</returns>
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj is IImmutableArray immutableArray)
            {
                return array == immutableArray.Array;
            }
            return false;
        }

        /// <summary>Indicates whether specified array is equal to this array.</summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <see langword="true" /> if <paramref name="other" /> is equal to this array; otherwise, <see langword="false" />.</returns>
        [System.Runtime.Versioning.NonVersionable]
        public bool Equals(ImmutableArray<T> other)
        {
            return array == other.array;
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Collections.Immutable.ImmutableArray`1" /> struct based on the contents of an existing instance, allowing a covariant static cast to efficiently reuse the existing array.</summary>
        /// <param name="items">The array to initialize the array with. No copy is made.</param>
        /// <typeparam name="TDerived">The type of array element to return.</typeparam>
        /// <returns>An immutable array instance with elements cast to the new type.</returns>
        public static ImmutableArray<T> CastUp<TDerived>(ImmutableArray<TDerived> items) where TDerived : class?, T
        {
            T[] items2 = (T[])(object)items.array;
            return new ImmutableArray<T>(items2);
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Collections.Immutable.ImmutableArray`1" /> struct by casting the underlying array to an array of type <typeparamref name="TOther" />.</summary>
        /// <typeparam name="TOther">The type of array element to return.</typeparam>
        /// <exception cref="T:System.InvalidCastException">The cast is illegal.</exception>
        /// <returns>An immutable array instance with elements cast to the new type.</returns>
        public ImmutableArray<TOther> CastArray<TOther>() where TOther : class?
        {
            return new ImmutableArray<TOther>((TOther[])(object)array);
        }

        /// <summary>Returns a new immutable array that contains the elements of this array cast to a different type.</summary>
        /// <typeparam name="TOther">The type of array element to return.</typeparam>
        /// <returns>An immutable array that contains the elements of this array, cast to a different type. If the cast fails, returns an array whose <see cref="P:System.Collections.Immutable.ImmutableArray`1.IsDefault" /> property returns <see langword="true" />.</returns>
        public ImmutableArray<TOther> As<TOther>() where TOther : class?
        {
            return new ImmutableArray<TOther>(array as TOther[]);
        }

        /// <summary>Returns an enumerator that iterates through the array.</summary>
        /// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Collections.Immutable.ImmutableArray`1.IsDefault" /> property returns <see langword="true" />.</exception>
        /// <returns>An enumerator that can be used to iterate through the array.</returns>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            ImmutableArray<T> immutableArray = this;
            immutableArray.ThrowInvalidOperationIfNotInitialized();
            return EnumeratorObject.Create(immutableArray.array);
        }

        /// <summary>Returns an enumerator that iterates through the immutable array.</summary>
        /// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Collections.Immutable.ImmutableArray`1.IsDefault" /> property returns <see langword="true" />.</exception>
        /// <returns>An enumerator that iterates through the immutable array.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            ImmutableArray<T> immutableArray = this;
            immutableArray.ThrowInvalidOperationIfNotInitialized();
            return EnumeratorObject.Create(immutableArray.array);
        }

        internal void ThrowNullRefIfNotInitialized()
        {
            _ = array.Length;
        }

        private void ThrowInvalidOperationIfNotInitialized()
        {
            if (IsDefault)
            {
                throw new InvalidOperationException(MDCFR.Properties.Resources.InvalidOperationOnDefaultArray);
            }
        }
    }

    internal sealed class ImmutableArrayBuilderDebuggerProxy<T>
    {
        private readonly ImmutableArray<T>.Builder _builder;

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] A => _builder.ToArray();

        public ImmutableArrayBuilderDebuggerProxy(ImmutableArray<T>.Builder builder)
        {
            Requires.NotNull(builder, "builder");
            _builder = builder;
        }
    }

    /// <summary>Provides a set of initialization methods for instances of the <see cref="System.Collections.Immutable.ImmutableDictionary{TKey , TValue}" /> class.  </summary>
    public static class ImmutableDictionary
    {
        /// <summary>Creates an empty immutable dictionary.</summary>
        /// <typeparam name="TKey">The type of keys stored by the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of values stored by the dictionary.</typeparam>
        /// <returns>An empty immutable dictionary.</returns>
        public static ImmutableDictionary<TKey, TValue> Create<TKey, TValue>() where TKey : notnull
        {
            return ImmutableDictionary<TKey, TValue>.Empty;
        }

        /// <summary>Creates an empty immutable dictionary that uses the specified key comparer.</summary>
        /// <param name="keyComparer">The implementation to use to determine the equality of keys in the dictionary.</param>
        /// <typeparam name="TKey">The type of keys stored by the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of values stored by the dictionary.</typeparam>
        /// <returns>An empty immutable dictionary.</returns>
        public static ImmutableDictionary<TKey, TValue> Create<TKey, TValue>(IEqualityComparer<TKey>? keyComparer) where TKey : notnull
        {
            return ImmutableDictionary<TKey, TValue>.Empty.WithComparers(keyComparer);
        }

        /// <summary>Creates an empty immutable dictionary that uses the specified key and value comparers.</summary>
        /// <param name="keyComparer">The implementation to use to determine the equality of keys in the dictionary.</param>
        /// <param name="valueComparer">The implementation to use to determine the equality of values in the dictionary.</param>
        /// <typeparam name="TKey">The type of keys stored by the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of values stored by the dictionary.</typeparam>
        /// <returns>An empty immutable dictionary.</returns>
        public static ImmutableDictionary<TKey, TValue> Create<TKey, TValue>(IEqualityComparer<TKey>? keyComparer, IEqualityComparer<TValue>? valueComparer) where TKey : notnull
        {
            return ImmutableDictionary<TKey, TValue>.Empty.WithComparers(keyComparer, valueComparer);
        }

        /// <summary>Creates a new immutable dictionary that contains the specified items.</summary>
        /// <param name="items">The items used to populate the dictionary before it's immutable.</param>
        /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
        /// <returns>A new immutable dictionary that contains the specified items.</returns>
        public static ImmutableDictionary<TKey, TValue> CreateRange<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> items) where TKey : notnull
        {
            return ImmutableDictionary<TKey, TValue>.Empty.AddRange(items);
        }

        /// <summary>Creates a new immutable dictionary that contains the specified items and uses the specified key comparer.</summary>
        /// <param name="keyComparer">The comparer implementation to use to compare keys for equality.</param>
        /// <param name="items">The items to add to the dictionary before it's immutable.</param>
        /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
        /// <returns>A new immutable dictionary that contains the specified items and uses the specified comparer.</returns>
        public static ImmutableDictionary<TKey, TValue> CreateRange<TKey, TValue>(IEqualityComparer<TKey>? keyComparer, IEnumerable<KeyValuePair<TKey, TValue>> items) where TKey : notnull
        {
            return ImmutableDictionary<TKey, TValue>.Empty.WithComparers(keyComparer).AddRange(items);
        }

        /// <summary>Creates a new immutable dictionary that contains the specified items and uses the specified key comparer.</summary>
        /// <param name="keyComparer">The comparer implementation to use to compare keys for equality.</param>
        /// <param name="valueComparer">The comparer implementation to use to compare values for equality.</param>
        /// <param name="items">The items to add to the dictionary before it's immutable.</param>
        /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
        /// <returns>A new immutable dictionary that contains the specified items and uses the specified comparer.</returns>
        public static ImmutableDictionary<TKey, TValue> CreateRange<TKey, TValue>(IEqualityComparer<TKey>? keyComparer, IEqualityComparer<TValue>? valueComparer, IEnumerable<KeyValuePair<TKey, TValue>> items) where TKey : notnull
        {
            return ImmutableDictionary<TKey, TValue>.Empty.WithComparers(keyComparer, valueComparer).AddRange(items);
        }

        /// <summary>Creates a new immutable dictionary builder.</summary>
        /// <typeparam name="TKey">The type of keys stored by the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of values stored by the dictionary.</typeparam>
        /// <returns>The new builder.</returns>
        public static ImmutableDictionary<TKey, TValue>.Builder CreateBuilder<TKey, TValue>() where TKey : notnull
        {
            return Create<TKey, TValue>().ToBuilder();
        }

        /// <summary>Creates a new immutable dictionary builder.</summary>
        /// <param name="keyComparer">The key comparer.</param>
        /// <typeparam name="TKey">The type of keys stored by the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of values stored by the dictionary.</typeparam>
        /// <returns>The new builder.</returns>
        public static ImmutableDictionary<TKey, TValue>.Builder CreateBuilder<TKey, TValue>(IEqualityComparer<TKey>? keyComparer) where TKey : notnull
        {
            return Create<TKey, TValue>(keyComparer).ToBuilder();
        }

        /// <summary>Creates a new immutable dictionary builder.</summary>
        /// <param name="keyComparer">The key comparer.</param>
        /// <param name="valueComparer">The value comparer.</param>
        /// <typeparam name="TKey">The type of keys stored by the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of values stored by the dictionary.</typeparam>
        /// <returns>The new builder.</returns>
        public static ImmutableDictionary<TKey, TValue>.Builder CreateBuilder<TKey, TValue>(IEqualityComparer<TKey>? keyComparer, IEqualityComparer<TValue>? valueComparer) where TKey : notnull
        {
            return Create(keyComparer, valueComparer).ToBuilder();
        }

        /// <summary>Enumerates and transforms a sequence, and produces an immutable dictionary of its contents by using the specified key and value comparers.</summary>
        /// <param name="source">The sequence to enumerate to generate the dictionary.</param>
        /// <param name="keySelector">The function that will produce the key for the dictionary from each sequence element.</param>
        /// <param name="elementSelector">The function that will produce the value for the dictionary from each sequence element.</param>
        /// <param name="keyComparer">The key comparer to use for the dictionary.</param>
        /// <param name="valueComparer">The value comparer to use for the dictionary.</param>
        /// <typeparam name="TSource">The type of the elements in the sequence.</typeparam>
        /// <typeparam name="TKey">The type of the keys in the resulting dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the resulting dictionary.</typeparam>
        /// <returns>An immutable dictionary that contains the items in the specified sequence.</returns>
        public static ImmutableDictionary<TKey, TValue> ToImmutableDictionary<TSource, TKey, TValue>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> elementSelector, IEqualityComparer<TKey>? keyComparer, IEqualityComparer<TValue>? valueComparer) where TKey : notnull
        {
            Func<TSource, TKey> keySelector2 = keySelector;
            Func<TSource, TValue> elementSelector2 = elementSelector;
            Requires.NotNull(source, "source");
            Requires.NotNull(keySelector2, "keySelector");
            Requires.NotNull(elementSelector2, "elementSelector");
            return ImmutableDictionary<TKey, TValue>.Empty.WithComparers(keyComparer, valueComparer).AddRange(source.Select<TSource, KeyValuePair<TKey, TValue>>((TSource element) => new KeyValuePair<TKey, TValue>(keySelector2(element), elementSelector2(element))));
        }

        /// <summary>Creates an immutable dictionary from the current contents of the builder's dictionary.</summary>
        /// <param name="builder">The builder to create the immutable dictionary from.</param>
        /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
        /// <returns>An immutable dictionary that contains the current contents in the builder's dictionary.</returns>
        public static ImmutableDictionary<TKey, TValue> ToImmutableDictionary<TKey, TValue>(this ImmutableDictionary<TKey, TValue>.Builder builder) where TKey : notnull
        {
            Requires.NotNull(builder, "builder");
            return builder.ToImmutable();
        }

        /// <summary>Enumerates and transforms a sequence, and produces an immutable dictionary of its contents by using the specified key comparer.</summary>
        /// <param name="source">The sequence to enumerate to generate the dictionary.</param>
        /// <param name="keySelector">The function that will produce the key for the dictionary from each sequence element.</param>
        /// <param name="elementSelector">The function that will produce the value for the dictionary from each sequence element.</param>
        /// <param name="keyComparer">The key comparer to use for the dictionary.</param>
        /// <typeparam name="TSource">The type of the elements in the sequence.</typeparam>
        /// <typeparam name="TKey">The type of the keys in the resulting dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the resulting dictionary.</typeparam>
        /// <returns>An immutable dictionary that contains the items in the specified sequence.</returns>
        public static ImmutableDictionary<TKey, TValue> ToImmutableDictionary<TSource, TKey, TValue>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> elementSelector, IEqualityComparer<TKey>? keyComparer) where TKey : notnull
        {
            return source.ToImmutableDictionary(keySelector, elementSelector, keyComparer, null);
        }

        /// <summary>Constructs an immutable dictionary from an existing collection of elements, applying a transformation function to the source keys.</summary>
        /// <param name="source">The source collection used to generate the immutable dictionary.</param>
        /// <param name="keySelector">The function used to transform keys for the immutable dictionary.</param>
        /// <typeparam name="TSource">The type of element in the source collection.</typeparam>
        /// <typeparam name="TKey">The type of key in the resulting immutable dictionary.</typeparam>
        /// <returns>The immutable dictionary that contains elements from <paramref name="source" />, with keys transformed by applying <paramref name="keySelector" />.</returns>
        public static ImmutableDictionary<TKey, TSource> ToImmutableDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) where TKey : notnull
        {
            return source.ToImmutableDictionary(keySelector, (TSource v) => v, null, null);
        }

        /// <summary>Constructs an immutable dictionary based on some transformation of a sequence.</summary>
        /// <param name="source">The source collection used to generate the immutable dictionary.</param>
        /// <param name="keySelector">The function used to transform keys for the immutable dictionary.</param>
        /// <param name="keyComparer">The key comparer to use for the dictionary.</param>
        /// <typeparam name="TSource">The type of element in the source collection.</typeparam>
        /// <typeparam name="TKey">The type of key in the resulting immutable dictionary.</typeparam>
        /// <returns>The immutable dictionary that contains elements from <paramref name="source" />, with keys transformed by applying <paramref name="keySelector" />.</returns>
        public static ImmutableDictionary<TKey, TSource> ToImmutableDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? keyComparer) where TKey : notnull
        {
            return source.ToImmutableDictionary(keySelector, (TSource v) => v, keyComparer, null);
        }

        /// <summary>Enumerates and transforms a sequence, and produces an immutable dictionary of its contents.</summary>
        /// <param name="source">The sequence to enumerate to generate the dictionary.</param>
        /// <param name="keySelector">The function that will produce the key for the dictionary from each sequence element.</param>
        /// <param name="elementSelector">The function that will produce the value for the dictionary from each sequence element.</param>
        /// <typeparam name="TSource">The type of the elements in the sequence.</typeparam>
        /// <typeparam name="TKey">The type of the keys in the resulting dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the resulting dictionary.</typeparam>
        /// <returns>An immutable dictionary that contains the items in the specified sequence.</returns>
        public static ImmutableDictionary<TKey, TValue> ToImmutableDictionary<TSource, TKey, TValue>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> elementSelector) where TKey : notnull
        {
            return source.ToImmutableDictionary(keySelector, elementSelector, null, null);
        }

        /// <summary>Enumerates a sequence of key/value pairs and produces an immutable dictionary of its contents by using the specified key and value comparers.</summary>
        /// <param name="source">The sequence of key/value pairs to enumerate.</param>
        /// <param name="keyComparer">The key comparer to use when building the immutable dictionary.</param>
        /// <param name="valueComparer">The value comparer to use for the immutable dictionary.</param>
        /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
        /// <returns>An immutable dictionary that contains the key/value pairs in the specified sequence.</returns>
        public static ImmutableDictionary<TKey, TValue> ToImmutableDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source, IEqualityComparer<TKey>? keyComparer, IEqualityComparer<TValue>? valueComparer) where TKey : notnull
        {
            Requires.NotNull(source, "source");
            if (source is ImmutableDictionary<TKey, TValue> immutableDictionary)
            {
                return immutableDictionary.WithComparers(keyComparer, valueComparer);
            }
            return ImmutableDictionary<TKey, TValue>.Empty.WithComparers(keyComparer, valueComparer).AddRange(source);
        }

        /// <summary>Enumerates a sequence of key/value pairs and produces an immutable dictionary of its contents by using the specified key comparer.</summary>
        /// <param name="source">The sequence of key/value pairs to enumerate.</param>
        /// <param name="keyComparer">The key comparer to use when building the immutable dictionary.</param>
        /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
        /// <returns>An immutable dictionary that contains the key/value pairs in the specified sequence.</returns>
        public static ImmutableDictionary<TKey, TValue> ToImmutableDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source, IEqualityComparer<TKey>? keyComparer) where TKey : notnull
        {
            return source.ToImmutableDictionary(keyComparer, null);
        }

        /// <summary>Enumerates a sequence of key/value pairs and produces an immutable dictionary of its contents.</summary>
        /// <param name="source">The sequence of key/value pairs to enumerate.</param>
        /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
        /// <returns>An immutable dictionary that contains the key/value pairs in the specified sequence.</returns>
        public static ImmutableDictionary<TKey, TValue> ToImmutableDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source) where TKey : notnull
        {
            return source.ToImmutableDictionary(null, null);
        }

        /// <summary>Determines whether the specified immutable dictionary contains the specified key/value pair.</summary>
        /// <param name="map">The immutable dictionary to search.</param>
        /// <param name="key">The key to locate in the immutable dictionary.</param>
        /// <param name="value">The value to locate on the specified key, if the key is found.</param>
        /// <typeparam name="TKey">The type of the keys in the immutable dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the immutable dictionary.</typeparam>
        /// <returns>
        ///   <see langword="true" /> if this map contains the specified key/value pair; otherwise, <see langword="false" />.</returns>
        public static bool Contains<TKey, TValue>(this IImmutableDictionary<TKey, TValue> map, TKey key, TValue value) where TKey : notnull
        {
            Requires.NotNull(map, "map");
            Requires.NotNullAllowStructs(key, "key");
            return map.Contains(new KeyValuePair<TKey, TValue>(key, value));
        }

        /// <summary>Gets the value for a given key if a matching key exists in the dictionary.</summary>
        /// <param name="dictionary">The dictionary to retrieve the value from.</param>
        /// <param name="key">The key to search for.</param>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <returns>The value for the key, or <c>default(TValue)</c> if no matching key was found.</returns>
        public static TValue? GetValueOrDefault<TKey, TValue>(this IImmutableDictionary<TKey, TValue> dictionary, TKey key) where TKey : notnull
        {
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
            return dictionary.GetValueOrDefault(key, default(TValue));
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
        }

        /// <summary>Gets the value for a given key if a matching key exists in the dictionary.</summary>
        /// <param name="dictionary">The dictionary to retrieve the value from.</param>
        /// <param name="key">The key to search for.</param>
        /// <param name="defaultValue">The default value to return if no matching key is found in the dictionary.</param>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <returns>The value for the key, or <paramref name="defaultValue" /> if no matching key was found.</returns>
        public static TValue GetValueOrDefault<TKey, TValue>(this IImmutableDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue) where TKey : notnull
        {
            Requires.NotNull(dictionary, "dictionary");
            Requires.NotNullAllowStructs(key, "key");
            if (dictionary.TryGetValue(key, out TValue value))
            {
                return value;
            }
            return defaultValue;
        }
    }
    
    /// <summary>Represents an immutable, unordered collection of keys and values. </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(ImmutableDictionaryDebuggerProxy<,>))]
    public sealed class ImmutableDictionary<TKey, TValue> : IImmutableDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>, IReadOnlyCollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, IImmutableDictionaryInternal<TKey, TValue>, IHashKeyCollection<TKey>, IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IDictionary, ICollection where TKey : notnull
    {
        /// <summary>Represents a hash map that mutates with little or no memory allocations and that can produce or build on immutable hash map instances very efficiently. </summary>
        [DebuggerDisplay("Count = {Count}")]
        [DebuggerTypeProxy(typeof(ImmutableDictionaryBuilderDebuggerProxy<,>))]
        public sealed class Builder : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, IReadOnlyDictionary<TKey, TValue>, IReadOnlyCollection<KeyValuePair<TKey, TValue>>, IDictionary, ICollection
        {
            private SortedInt32KeyNode<HashBucket> _root = SortedInt32KeyNode<HashBucket>.EmptyNode;

            private Comparers _comparers;

            private int _count;

            private ImmutableDictionary<TKey, TValue> _immutable;

            private int _version;

            private object _syncRoot;

            /// <summary>Gets or sets the key comparer.</summary>
            /// <returns>The key comparer.</returns>
            public IEqualityComparer<TKey> KeyComparer
            {
                get
                {
                    return _comparers.KeyComparer;
                }
                set
                {
                    Requires.NotNull(value, "value");
                    if (value != KeyComparer)
                    {
                        Comparers comparers = Comparers.Get(value, ValueComparer);
                        MutationInput origin = new MutationInput(SortedInt32KeyNode<HashBucket>.EmptyNode, comparers);
                        MutationResult mutationResult = ImmutableDictionary<TKey, TValue>.AddRange((IEnumerable<KeyValuePair<TKey, TValue>>)this, origin, KeyCollisionBehavior.ThrowIfValueDifferent);
                        _immutable = null;
                        _comparers = comparers;
                        _count = mutationResult.CountAdjustment;
                        Root = mutationResult.Root;
                    }
                }
            }

            /// <summary>Gets or sets the value comparer.</summary>
            /// <returns>The value comparer.</returns>
            public IEqualityComparer<TValue> ValueComparer
            {
                get
                {
                    return _comparers.ValueComparer;
                }
                set
                {
                    Requires.NotNull(value, "value");
                    if (value != ValueComparer)
                    {
                        _comparers = _comparers.WithValueComparer(value);
                        _immutable = null;
                    }
                }
            }

            /// <summary>Gets the number of elements contained in the immutable dictionary.</summary>
            /// <returns>The number of elements contained in the immutable dictionary.</returns>
            public int Count => _count;

            bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

            /// <summary>Gets a collection that contains the keys of the immutable dictionary.</summary>
            /// <returns>A collection that contains the keys of the object that implements the immutable dictionary.</returns>
            public IEnumerable<TKey> Keys
            {
                get
                {
                    using Enumerator enumerator = GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        yield return enumerator.Current.Key;
                    }
                }
            }

            ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys.ToArray(Count);

            /// <summary>Gets a collection that contains the values of the immutable dictionary.</summary>
            /// <returns>A collection that contains the values of the object that implements the dictionary.</returns>
            public IEnumerable<TValue> Values
            {
                get
                {
                    using Enumerator enumerator = GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        yield return enumerator.Current.Value;
                    }
                }
            }

            ICollection<TValue> IDictionary<TKey, TValue>.Values => Values.ToArray(Count);

            /// <summary>Gets a value that indicates whether the <see cref="T:System.Collections.IDictionary" /> object has a fixed size.</summary>
            /// <returns>
            ///   <see langword="true" /> if the <see cref="T:System.Collections.IDictionary" /> object has a fixed size; otherwise, <see langword="false" />.</returns>
            bool IDictionary.IsFixedSize => false;

            /// <summary>Gets a value that indicates whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</summary>
            /// <returns>
            ///   <see langword="true" /> if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, <see langword="false" />.</returns>
            bool IDictionary.IsReadOnly => false;

            /// <summary>Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2" />.</summary>
            /// <returns>An <see cref="T:System.Collections.Generic.ICollection`1" /> containing the keys of the object that implements <see cref="T:System.Collections.Generic.IDictionary`2" />.</returns>
            ICollection IDictionary.Keys => Keys.ToArray(Count);

            /// <summary>Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2" />.</summary>
            /// <returns>An <see cref="T:System.Collections.Generic.ICollection`1" /> containing the values in the object that implements <see cref="T:System.Collections.Generic.IDictionary`2" />.</returns>
            ICollection IDictionary.Values => Values.ToArray(Count);

            /// <summary>Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.</summary>
            /// <returns>An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.</returns>
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            object ICollection.SyncRoot
            {
                get
                {
                    if (_syncRoot == null)
                    {
                        Interlocked.CompareExchange<object>(ref _syncRoot, new object(), (object)null);
                    }
                    return _syncRoot;
                }
            }

            /// <summary>Gets a value that indicates whether access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe).</summary>
            /// <returns>
            ///   <see langword="true" /> if access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe); otherwise, <see langword="false" />.</returns>
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            bool ICollection.IsSynchronized => false;

            /// <summary>Gets or sets the element with the specified key.</summary>
            /// <param name="key">The key.</param>
            /// <returns>Value stored under specified key.</returns>
            object? IDictionary.this[object key]
            {
                get
                {
                    return this[(TKey)key];
                }
                set
                {
                    this[(TKey)key] = (TValue)value;
                }
            }

            internal int Version => _version;

            private MutationInput Origin => new MutationInput(Root, _comparers);

            private SortedInt32KeyNode<HashBucket> Root
            {
                get
                {
                    return _root;
                }
                set
                {
                    _version++;
                    if (_root != value)
                    {
                        _root = value;
                        _immutable = null;
                    }
                }
            }

            /// <summary>Gets or sets the element with the specified key.</summary>
            /// <param name="key">The element to get or set.</param>
            /// <exception cref="T:System.ArgumentNullException">
            ///   <paramref name="key" /> is <see langword="null" />.</exception>
            /// <exception cref="T:System.Collections.Generic.KeyNotFoundException">The property is being retrieved, and <paramref name="key" /> is not found.</exception>
            /// <exception cref="T:System.NotSupportedException">The property is being set, and the <see cref="T:System.Collections.Generic.IDictionary`2" /> is read-only.</exception>
            /// <returns>The element that has the specified key.</returns>
            public TValue this[TKey key]
            {
                get
                {
                    if (TryGetValue(key, out var value))
                    {
                        return value;
                    }
                    throw new KeyNotFoundException(System.SR.Format(MDCFR.Properties.Resources.Arg_KeyNotFoundWithKey, key.ToString()));
                }
                set
                {
                    MutationResult result = ImmutableDictionary<TKey, TValue>.Add(key, value, KeyCollisionBehavior.SetValue, Origin);
                    Apply(result);
                }
            }

            internal Builder(ImmutableDictionary<TKey, TValue> map)
            {
                Requires.NotNull(map, "map");
                _root = map._root;
                _count = map._count;
                _comparers = map._comparers;
                _immutable = map;
            }

            /// <summary>Adds an element with the provided key and value to the dictionary object.</summary>
            /// <param name="key">The key of the element to add.</param>
            /// <param name="value">The value of the element to add.</param>
            void IDictionary.Add(object key, object value)
            {
                Add((TKey)key, (TValue)value);
            }

            /// <summary>Determines whether the dictionary object contains an element with the specified key.</summary>
            /// <param name="key">The key to locate.</param>
            /// <returns>
            ///   <see langword="true" /> if the dictionary contains an element with the key; otherwise, <see langword="false" />.</returns>
            bool IDictionary.Contains(object key)
            {
                return ContainsKey((TKey)key);
            }

            /// <summary>Returns an <see cref="T:System.Collections.IDictionaryEnumerator" /> object for the dictionary.</summary>
            /// <exception cref="T:System.NotImplementedException" />
            /// <returns>An <see cref="T:System.Collections.IDictionaryEnumerator" /> object for the dictionary.</returns>
            IDictionaryEnumerator IDictionary.GetEnumerator()
            {
                return new DictionaryEnumerator<TKey, TValue>(GetEnumerator());
            }

            /// <summary>Removes the element with the specified key from the dictionary.</summary>
            /// <param name="key">The key of the element to remove.</param>
            void IDictionary.Remove(object key)
            {
                Remove((TKey)key);
            }

            /// <summary>Copies the elements of the dictionary to an array of type <see cref="T:System.Collections.Generic.KeyValuePair`2" />, starting at the specified array index.</summary>
            /// <param name="array">The one-dimensional array of type <see cref="T:System.Collections.Generic.KeyValuePair`2" /> that is the destination of the elements copied from the dictionary. The array must have zero-based indexing.</param>
            /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
            void ICollection.CopyTo(Array array, int arrayIndex)
            {
                Requires.NotNull(array, "array");
                Requires.Range(arrayIndex >= 0, "arrayIndex");
                Requires.Range(array.Length >= arrayIndex + Count, "arrayIndex");
                using Enumerator enumerator = GetEnumerator();
                while (enumerator.MoveNext())
                {
                    KeyValuePair<TKey, TValue> current = enumerator.Current;
                    array.SetValue(new DictionaryEntry(current.Key, current.Value), arrayIndex++);
                }
            }

            /// <summary>Adds a sequence of values to this collection.</summary>
            /// <param name="items">The items to add to this collection.</param>
            public void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> items)
            {
                MutationResult result = ImmutableDictionary<TKey, TValue>.AddRange(items, Origin, KeyCollisionBehavior.ThrowIfValueDifferent);
                Apply(result);
            }

            /// <summary>Removes any entries with keys that match those found in the specified sequence from the immutable dictionary.</summary>
            /// <param name="keys">The keys for entries to remove from the dictionary.</param>
            public void RemoveRange(IEnumerable<TKey> keys)
            {
                Requires.NotNull(keys, "keys");
                foreach (TKey key in keys)
                {
                    Remove(key);
                }
            }

            /// <summary>Returns an enumerator that iterates through the immutable dictionary.</summary>
            /// <returns>An enumerator that can be used to iterate through the collection.</returns>
            public Enumerator GetEnumerator()
            {
                return new Enumerator(_root, this);
            }

            /// <summary>Gets the value for a given key if a matching key exists in the dictionary.</summary>
            /// <param name="key">The key to search for.</param>
            /// <returns>The value for the key, or <c>default(TValue)</c> if no matching key was found.</returns>
            public TValue? GetValueOrDefault(TKey key)
            {
                return GetValueOrDefault(key, default(TValue));
            }

            /// <summary>Gets the value for a given key if a matching key exists in the dictionary.</summary>
            /// <param name="key">The key to search for.</param>
            /// <param name="defaultValue">The default value to return if no matching key is found in the dictionary.</param>
            /// <returns>The value for the key, or <paramref name="defaultValue" /> if no matching key was found.</returns>
            public TValue GetValueOrDefault(TKey key, TValue defaultValue)
            {
                Requires.NotNullAllowStructs(key, "key");
                if (TryGetValue(key, out var value))
                {
                    return value;
                }
                return defaultValue;
            }

            /// <summary>Creates an immutable dictionary based on the contents of this instance.</summary>
            /// <returns>An immutable dictionary.</returns>
            public ImmutableDictionary<TKey, TValue> ToImmutable()
            {
                return _immutable ?? (_immutable = ImmutableDictionary<TKey, TValue>.Wrap(_root, _comparers, _count));
            }

            /// <summary>Adds an element that has the specified key and value to the immutable dictionary.</summary>
            /// <param name="key">The key of the element to add.</param>
            /// <param name="value">The value of the element to add.</param>
            /// <exception cref="T:System.ArgumentNullException">
            ///   <paramref name="key" /> is null.</exception>
            /// <exception cref="T:System.ArgumentException">An element with the same key already exists in the dictionary.</exception>
            /// <exception cref="T:System.NotSupportedException">The dictionary is read-only.</exception>
            public void Add(TKey key, TValue value)
            {
                MutationResult result = ImmutableDictionary<TKey, TValue>.Add(key, value, KeyCollisionBehavior.ThrowIfValueDifferent, Origin);
                Apply(result);
            }

            /// <summary>Determines whether the immutable dictionary contains an element that has the specified key.</summary>
            /// <param name="key">The key to locate in the dictionary.</param>
            /// <exception cref="T:System.ArgumentNullException">
            ///   <paramref name="key" /> is null.</exception>
            /// <returns>
            ///   <see langword="true" /> if the dictionary contains an element with the key; otherwise, <see langword="false" />.</returns>
            public bool ContainsKey(TKey key)
            {
                return ImmutableDictionary<TKey, TValue>.ContainsKey(key, Origin);
            }

            /// <summary>Determines whether the immutable dictionary contains an element that has the specified value.</summary>
            /// <param name="value">The value to locate in the immutable dictionary. The value can be <see langword="null" /> for reference types.</param>
            /// <returns>
            ///   <see langword="true" /> if the dictionary contains an element with the specified value; otherwise, <see langword="false" />.</returns>
            public bool ContainsValue(TValue value)
            {
                using (Enumerator enumerator = GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        KeyValuePair<TKey, TValue> current = enumerator.Current;
                        if (ValueComparer.Equals(value, current.Value))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            /// <summary>Removes the element with the specified key from the immutable dictionary.</summary>
            /// <param name="key">The key of the element to remove.</param>
            /// <exception cref="T:System.ArgumentNullException">
            ///   <paramref name="key" /> is null.</exception>
            /// <exception cref="T:System.NotSupportedException">The dictionary is read-only.</exception>
            /// <returns>
            ///   <see langword="true" /> if the element is successfully removed; otherwise, <see langword="false" />.  This method also returns <see langword="false" /> if <paramref name="key" /> was not found in the dictionary.</returns>
            public bool Remove(TKey key)
            {
                MutationResult result = ImmutableDictionary<TKey, TValue>.Remove(key, Origin);
                return Apply(result);
            }

            /// <summary>Returns the value associated with the specified key.</summary>
            /// <param name="key">The key whose value will be retrieved.</param>
            /// <param name="value">When this method returns, contains the value associated with the specified key, if the key is found; otherwise, returns the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.</param>
            /// <exception cref="T:System.ArgumentNullException">
            ///   <paramref name="key" /> is null.</exception>
            /// <returns>
            ///   <see langword="true" /> if the object that implements the immutable dictionary contains an element with the specified key; otherwise, <see langword="false" />.</returns>
            public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
            {
                return ImmutableDictionary<TKey, TValue>.TryGetValue(key, Origin, out value);
            }

            /// <summary>Determines whether this dictionary contains a specified key.</summary>
            /// <param name="equalKey">The key to search for.</param>
            /// <param name="actualKey">The matching key located in the dictionary if found, or <c>equalkey</c> if no match is found.</param>
            /// <returns>
            ///   <see langword="true" /> if a match for <paramref name="equalKey" /> is found; otherwise, <see langword="false" />.</returns>
            public bool TryGetKey(TKey equalKey, out TKey actualKey)
            {
                return ImmutableDictionary<TKey, TValue>.TryGetKey(equalKey, Origin, out actualKey);
            }

            /// <summary>Adds the specified item to the immutable dictionary.</summary>
            /// <param name="item">The object to add to the dictionary.</param>
            /// <exception cref="T:System.NotSupportedException">The dictionary is read-only.</exception>
            public void Add(KeyValuePair<TKey, TValue> item)
            {
                Add(item.Key, item.Value);
            }

            /// <summary>Removes all items from the immutable dictionary.</summary>
            /// <exception cref="T:System.NotSupportedException">The dictionary is read-only.</exception>
            public void Clear()
            {
                Root = SortedInt32KeyNode<HashBucket>.EmptyNode;
                _count = 0;
            }

            /// <summary>Determines whether the immutable dictionary contains a specific value.</summary>
            /// <param name="item">The object to locate in the dictionary.</param>
            /// <returns>
            ///   <see langword="true" /> if <paramref name="item" /> is found in the dictionary; otherwise, <see langword="false" />.</returns>
            public bool Contains(KeyValuePair<TKey, TValue> item)
            {
                return ImmutableDictionary<TKey, TValue>.Contains(item, Origin);
            }

            void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
            {
                Requires.NotNull(array, "array");
                using Enumerator enumerator = GetEnumerator();
                while (enumerator.MoveNext())
                {
                    KeyValuePair<TKey, TValue> current = enumerator.Current;
                    array[arrayIndex++] = current;
                }
            }

            /// <summary>Removes the first occurrence of a specific object from the immutable dictionary.</summary>
            /// <param name="item">The object to remove from the dictionary.</param>
            /// <exception cref="T:System.NotSupportedException">The dictionary is read-only.</exception>
            /// <returns>
            ///   <see langword="true" /> if <paramref name="item" /> was successfully removed from the dictionary; otherwise, <see langword="false" />. This method also returns false if <paramref name="item" /> is not found in the dictionary.</returns>
            public bool Remove(KeyValuePair<TKey, TValue> item)
            {
                if (Contains(item))
                {
                    return Remove(item.Key);
                }
                return false;
            }

            IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
            {
                return GetEnumerator();
            }

            /// <summary>Returns an enumerator that iterates through a collection.</summary>
            /// <returns>An enumerator object that can be used to iterate through the collection.</returns>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            private bool Apply(MutationResult result)
            {
                Root = result.Root;
                _count += result.CountAdjustment;
                return result.CountAdjustment != 0;
            }
        }

        internal sealed class Comparers : IEqualityComparer<HashBucket>, IEqualityComparer<KeyValuePair<TKey, TValue>>
        {
            internal static readonly Comparers Default = new Comparers(EqualityComparer<TKey>.Default, EqualityComparer<TValue>.Default);

            private readonly IEqualityComparer<TKey> _keyComparer;

            private readonly IEqualityComparer<TValue> _valueComparer;

            internal IEqualityComparer<TKey> KeyComparer => _keyComparer;

            internal IEqualityComparer<KeyValuePair<TKey, TValue>> KeyOnlyComparer => this;

            internal IEqualityComparer<TValue> ValueComparer => _valueComparer;

            internal IEqualityComparer<HashBucket> HashBucketEqualityComparer => this;

            internal Comparers(IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
            {
                Requires.NotNull(keyComparer, "keyComparer");
                Requires.NotNull(valueComparer, "valueComparer");
                _keyComparer = keyComparer;
                _valueComparer = valueComparer;
            }

            public bool Equals(HashBucket x, HashBucket y)
            {
                if (x.AdditionalElements == y.AdditionalElements && KeyComparer.Equals(x.FirstValue.Key, y.FirstValue.Key))
                {
                    return ValueComparer.Equals(x.FirstValue.Value, y.FirstValue.Value);
                }
                return false;
            }

            public int GetHashCode(HashBucket obj)
            {
                return KeyComparer.GetHashCode(obj.FirstValue.Key);
            }

            bool IEqualityComparer<KeyValuePair<TKey, TValue>>.Equals(KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y)
            {
                return _keyComparer.Equals(x.Key, y.Key);
            }

            int IEqualityComparer<KeyValuePair<TKey, TValue>>.GetHashCode(KeyValuePair<TKey, TValue> obj)
            {
                return _keyComparer.GetHashCode(obj.Key);
            }

            internal static Comparers Get(IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
            {
                Requires.NotNull(keyComparer, "keyComparer");
                Requires.NotNull(valueComparer, "valueComparer");
                if (keyComparer != Default.KeyComparer || valueComparer != Default.ValueComparer)
                {
                    return new Comparers(keyComparer, valueComparer);
                }
                return Default;
            }

            internal Comparers WithValueComparer(IEqualityComparer<TValue> valueComparer)
            {
                Requires.NotNull(valueComparer, "valueComparer");
                if (_valueComparer != valueComparer)
                {
                    return Get(KeyComparer, valueComparer);
                }
                return this;
            }
        }

        /// <summary>Enumerates the contents of the immutable dictionary without allocating any memory. </summary>
        public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IDisposable, IEnumerator
        {
            private readonly Builder _builder;

            private SortedInt32KeyNode<HashBucket>.Enumerator _mapEnumerator;

            private HashBucket.Enumerator _bucketEnumerator;

            private int _enumeratingBuilderVersion;

            /// <summary>Gets the element at the current position of the enumerator.</summary>
            /// <returns>The element in the dictionary at the current position of the enumerator.</returns>
            public KeyValuePair<TKey, TValue> Current
            {
                get
                {
                    _mapEnumerator.ThrowIfDisposed();
                    return _bucketEnumerator.Current;
                }
            }

            /// <summary>Gets the current element.</summary>
            /// <returns>Current element in enumeration.</returns>
            object IEnumerator.Current => Current;

            internal Enumerator(SortedInt32KeyNode<HashBucket> root, Builder? builder = null)
            {
                _builder = builder;
                _mapEnumerator = new SortedInt32KeyNode<HashBucket>.Enumerator(root);
                _bucketEnumerator = default(HashBucket.Enumerator);
                _enumeratingBuilderVersion = builder?.Version ?? (-1);
            }

            /// <summary>Advances the enumerator to the next element of the immutable dictionary.</summary>
            /// <exception cref="T:System.InvalidOperationException">The dictionary was modified after the enumerator was created.</exception>
            /// <returns>
            ///   <see langword="true" /> if the enumerator was successfully advanced to the next element; <see langword="false" /> if the enumerator has passed the end of the dictionary.</returns>
            public bool MoveNext()
            {
                ThrowIfChanged();
                if (_bucketEnumerator.MoveNext())
                {
                    return true;
                }
                if (_mapEnumerator.MoveNext())
                {
                    _bucketEnumerator = new HashBucket.Enumerator(_mapEnumerator.Current.Value);
                    return _bucketEnumerator.MoveNext();
                }
                return false;
            }

            /// <summary>Sets the enumerator to its initial position, which is before the first element in the dictionary.</summary>
            /// <exception cref="T:System.InvalidOperationException">The dictionary was modified after the enumerator was created.</exception>
            public void Reset()
            {
                _enumeratingBuilderVersion = ((_builder != null) ? _builder.Version : (-1));
                _mapEnumerator.Reset();
                _bucketEnumerator.Dispose();
                _bucketEnumerator = default(HashBucket.Enumerator);
            }

            /// <summary>Releases the resources used by the current instance of the <see cref="T:System.Collections.Immutable.ImmutableDictionary`2.Enumerator" /> class.</summary>
            public void Dispose()
            {
                _mapEnumerator.Dispose();
                _bucketEnumerator.Dispose();
            }

            private void ThrowIfChanged()
            {
                if (_builder != null && _builder.Version != _enumeratingBuilderVersion)
                {
                    throw new InvalidOperationException(MDCFR.Properties.Resources.CollectionModifiedDuringEnumeration);
                }
            }
        }

        internal readonly struct HashBucket : IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
        {
            internal struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IDisposable, IEnumerator
            {
                private enum Position
                {
                    BeforeFirst,
                    First,
                    Additional,
                    End
                }

                private readonly HashBucket _bucket;

                private Position _currentPosition;

                private ImmutableList<KeyValuePair<TKey, TValue>>.Enumerator _additionalEnumerator;

                object IEnumerator.Current => Current;

                public KeyValuePair<TKey, TValue> Current => _currentPosition switch
                {
                    Position.First => _bucket._firstValue,
                    Position.Additional => _additionalEnumerator.Current,
                    _ => throw new InvalidOperationException(),
                };

                internal Enumerator(HashBucket bucket)
                {
                    _bucket = bucket;
                    _currentPosition = Position.BeforeFirst;
                    _additionalEnumerator = default(ImmutableList<KeyValuePair<TKey, TValue>>.Enumerator);
                }

                public bool MoveNext()
                {
                    if (_bucket.IsEmpty)
                    {
                        _currentPosition = Position.End;
                        return false;
                    }
                    switch (_currentPosition)
                    {
                        case Position.BeforeFirst:
                            _currentPosition = Position.First;
                            return true;
                        case Position.First:
                            if (_bucket._additionalElements.IsEmpty)
                            {
                                _currentPosition = Position.End;
                                return false;
                            }
                            _currentPosition = Position.Additional;
                            _additionalEnumerator = new ImmutableList<KeyValuePair<TKey, TValue>>.Enumerator(_bucket._additionalElements);
                            return _additionalEnumerator.MoveNext();
                        case Position.Additional:
                            return _additionalEnumerator.MoveNext();
                        case Position.End:
                            return false;
                        default:
                            throw new InvalidOperationException();
                    }
                }

                public void Reset()
                {
                    _additionalEnumerator.Dispose();
                    _currentPosition = Position.BeforeFirst;
                }

                public void Dispose()
                {
                    _additionalEnumerator.Dispose();
                }
            }

            private readonly KeyValuePair<TKey, TValue> _firstValue;

            private readonly ImmutableList<KeyValuePair<TKey, TValue>>.Node _additionalElements;

            internal bool IsEmpty => _additionalElements == null;

            internal KeyValuePair<TKey, TValue> FirstValue
            {
                get
                {
                    if (IsEmpty)
                    {
                        throw new InvalidOperationException();
                    }
                    return _firstValue;
                }
            }

            internal ImmutableList<KeyValuePair<TKey, TValue>>.Node AdditionalElements => _additionalElements;

            private HashBucket(KeyValuePair<TKey, TValue> firstElement, ImmutableList<KeyValuePair<TKey, TValue>>.Node additionalElements = null)
            {
                _firstValue = firstElement;
                _additionalElements = additionalElements ?? ImmutableList<KeyValuePair<TKey, TValue>>.Node.EmptyNode;
            }

            public Enumerator GetEnumerator()
            {
                return new Enumerator(this);
            }

            IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
            {
                return GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public override bool Equals(object? obj)
            {
                throw new NotSupportedException();
            }

            public override int GetHashCode()
            {
                throw new NotSupportedException();
            }

            internal HashBucket Add(TKey key, TValue value, IEqualityComparer<KeyValuePair<TKey, TValue>> keyOnlyComparer, IEqualityComparer<TValue> valueComparer, KeyCollisionBehavior behavior, out OperationResult result)
            {
                KeyValuePair<TKey, TValue> keyValuePair = new KeyValuePair<TKey, TValue>(key, value);
                if (IsEmpty)
                {
                    result = OperationResult.SizeChanged;
                    return new HashBucket(keyValuePair);
                }
                if (keyOnlyComparer.Equals(keyValuePair, _firstValue))
                {
                    switch (behavior)
                    {
                        case KeyCollisionBehavior.SetValue:
                            result = OperationResult.AppliedWithoutSizeChange;
                            return new HashBucket(keyValuePair, _additionalElements);
                        case KeyCollisionBehavior.Skip:
                            result = OperationResult.NoChangeRequired;
                            return this;
                        case KeyCollisionBehavior.ThrowIfValueDifferent:
                            if (!valueComparer.Equals(_firstValue.Value, value))
                            {
                                throw new ArgumentException(System.SR.Format(MDCFR.Properties.Resources.DuplicateKey, key));
                            }
                            result = OperationResult.NoChangeRequired;
                            return this;
                        case KeyCollisionBehavior.ThrowAlways:
                            throw new ArgumentException(System.SR.Format(MDCFR.Properties.Resources.DuplicateKey, key));
                        default:
                            throw new InvalidOperationException();
                    }
                }
                int num = _additionalElements.IndexOf(keyValuePair, keyOnlyComparer);
                if (num < 0)
                {
                    result = OperationResult.SizeChanged;
                    return new HashBucket(_firstValue, _additionalElements.Add(keyValuePair));
                }
                switch (behavior)
                {
                    case KeyCollisionBehavior.SetValue:
                        result = OperationResult.AppliedWithoutSizeChange;
                        return new HashBucket(_firstValue, _additionalElements.ReplaceAt(num, keyValuePair));
                    case KeyCollisionBehavior.Skip:
                        result = OperationResult.NoChangeRequired;
                        return this;
                    case KeyCollisionBehavior.ThrowIfValueDifferent:
                        if (!valueComparer.Equals(_additionalElements.ItemRef(num).Value, value))
                        {
                            throw new ArgumentException(System.SR.Format(MDCFR.Properties.Resources.DuplicateKey, key));
                        }
                        result = OperationResult.NoChangeRequired;
                        return this;
                    case KeyCollisionBehavior.ThrowAlways:
                        throw new ArgumentException(System.SR.Format(MDCFR.Properties.Resources.DuplicateKey, key));
                    default:
                        throw new InvalidOperationException();
                }
            }

            internal HashBucket Remove(TKey key, IEqualityComparer<KeyValuePair<TKey, TValue>> keyOnlyComparer, out OperationResult result)
            {
                if (IsEmpty)
                {
                    result = OperationResult.NoChangeRequired;
                    return this;
                }
                KeyValuePair<TKey, TValue> keyValuePair = new KeyValuePair<TKey, TValue>(key, default(TValue));
                if (keyOnlyComparer.Equals(_firstValue, keyValuePair))
                {
                    if (_additionalElements.IsEmpty)
                    {
                        result = OperationResult.SizeChanged;
                        return default(HashBucket);
                    }
                    int count = _additionalElements.Left.Count;
                    result = OperationResult.SizeChanged;
                    return new HashBucket(_additionalElements.Key, _additionalElements.RemoveAt(count));
                }
                int num = _additionalElements.IndexOf(keyValuePair, keyOnlyComparer);
                if (num < 0)
                {
                    result = OperationResult.NoChangeRequired;
                    return this;
                }
                result = OperationResult.SizeChanged;
                return new HashBucket(_firstValue, _additionalElements.RemoveAt(num));
            }

            internal bool TryGetValue(TKey key, Comparers comparers, [MaybeNullWhen(false)] out TValue value)
            {
                if (IsEmpty)
                {
                    value = default(TValue);
                    return false;
                }
                if (comparers.KeyComparer.Equals(_firstValue.Key, key))
                {
                    value = _firstValue.Value;
                    return true;
                }
                KeyValuePair<TKey, TValue> item = new KeyValuePair<TKey, TValue>(key, default(TValue));
                int num = _additionalElements.IndexOf(item, comparers.KeyOnlyComparer);
                if (num < 0)
                {
                    value = default(TValue);
                    return false;
                }
                value = _additionalElements.ItemRef(num).Value;
                return true;
            }

            internal bool TryGetKey(TKey equalKey, Comparers comparers, out TKey actualKey)
            {
                if (IsEmpty)
                {
                    actualKey = equalKey;
                    return false;
                }
                if (comparers.KeyComparer.Equals(_firstValue.Key, equalKey))
                {
                    actualKey = _firstValue.Key;
                    return true;
                }
                KeyValuePair<TKey, TValue> item = new KeyValuePair<TKey, TValue>(equalKey, default(TValue));
                int num = _additionalElements.IndexOf(item, comparers.KeyOnlyComparer);
                if (num < 0)
                {
                    actualKey = equalKey;
                    return false;
                }
                actualKey = _additionalElements.ItemRef(num).Key;
                return true;
            }

            internal void Freeze()
            {
                _additionalElements?.Freeze();
            }
        }

        private readonly struct MutationInput
        {
            private readonly SortedInt32KeyNode<HashBucket> _root;

            private readonly Comparers _comparers;

            internal SortedInt32KeyNode<HashBucket> Root => _root;

            internal Comparers Comparers => _comparers;

            internal IEqualityComparer<TKey> KeyComparer => _comparers.KeyComparer;

            internal IEqualityComparer<KeyValuePair<TKey, TValue>> KeyOnlyComparer => _comparers.KeyOnlyComparer;

            internal IEqualityComparer<TValue> ValueComparer => _comparers.ValueComparer;

            internal IEqualityComparer<HashBucket> HashBucketComparer => _comparers.HashBucketEqualityComparer;

            internal MutationInput(SortedInt32KeyNode<HashBucket> root, Comparers comparers)
            {
                _root = root;
                _comparers = comparers;
            }

            internal MutationInput(ImmutableDictionary<TKey, TValue> map)
            {
                _root = map._root;
                _comparers = map._comparers;
            }
        }

        private readonly struct MutationResult
        {
            private readonly SortedInt32KeyNode<HashBucket> _root;

            private readonly int _countAdjustment;

            internal SortedInt32KeyNode<HashBucket> Root => _root;

            internal int CountAdjustment => _countAdjustment;

            internal MutationResult(MutationInput unchangedInput)
            {
                _root = unchangedInput.Root;
                _countAdjustment = 0;
            }

            internal MutationResult(SortedInt32KeyNode<HashBucket> root, int countAdjustment)
            {
                Requires.NotNull(root, "root");
                _root = root;
                _countAdjustment = countAdjustment;
            }

            internal ImmutableDictionary<TKey, TValue> Finalize(ImmutableDictionary<TKey, TValue> priorMap)
            {
                Requires.NotNull(priorMap, "priorMap");
                return priorMap.Wrap(Root, priorMap._count + CountAdjustment);
            }
        }

        internal enum KeyCollisionBehavior
        {
            SetValue,
            Skip,
            ThrowIfValueDifferent,
            ThrowAlways
        }

        internal enum OperationResult
        {
            AppliedWithoutSizeChange,
            SizeChanged,
            NoChangeRequired
        }

        /// <summary>Gets an empty immutable dictionary.</summary>
        public static readonly ImmutableDictionary<TKey, TValue> Empty = new ImmutableDictionary<TKey, TValue>();

        private static readonly Action<KeyValuePair<int, HashBucket>> s_FreezeBucketAction = delegate (KeyValuePair<int, HashBucket> kv)
        {
            kv.Value.Freeze();
        };

        private readonly int _count;

        private readonly SortedInt32KeyNode<HashBucket> _root;

        private readonly Comparers _comparers;

        /// <summary>Gets the number of key/value pairs in the immutable dictionary.</summary>
        /// <returns>The number of key/value pairs in the dictionary.</returns>
        public int Count => _count;

        /// <summary>Gets a value that indicates whether this instance of the immutable dictionary is empty.</summary>
        /// <returns>
        ///   <see langword="true" /> if this instance is empty; otherwise, <see langword="false" />.</returns>
        public bool IsEmpty => Count == 0;

        /// <summary>Gets the key comparer for the immutable dictionary.</summary>
        /// <returns>The key comparer.</returns>
        public IEqualityComparer<TKey> KeyComparer => _comparers.KeyComparer;

        /// <summary>Gets the value comparer used to determine whether values are equal.</summary>
        /// <returns>The value comparer used to determine whether values are equal.</returns>
        public IEqualityComparer<TValue> ValueComparer => _comparers.ValueComparer;

        /// <summary>Gets the keys in the immutable dictionary.</summary>
        /// <returns>The keys in the immutable dictionary.</returns>
        public IEnumerable<TKey> Keys
        {
            get
            {
                foreach (KeyValuePair<int, HashBucket> item in _root)
                {
                    foreach (KeyValuePair<TKey, TValue> item2 in item.Value)
                    {
                        yield return item2.Key;
                    }
                }
            }
        }

        /// <summary>Gets the values in the immutable dictionary.</summary>
        /// <returns>The values in the immutable dictionary.</returns>
        public IEnumerable<TValue> Values
        {
            get
            {
                foreach (KeyValuePair<int, HashBucket> item in _root)
                {
                    foreach (KeyValuePair<TKey, TValue> item2 in item.Value)
                    {
                        yield return item2.Value;
                    }
                }
            }
        }

        ICollection<TKey> IDictionary<TKey, TValue>.Keys => new KeysCollectionAccessor<TKey, TValue>(this);

        ICollection<TValue> IDictionary<TKey, TValue>.Values => new ValuesCollectionAccessor<TKey, TValue>(this);

        private MutationInput Origin => new MutationInput(this);

        /// <summary>Gets the TValue associated with the specified key.</summary>
        /// <param name="key">The type of the key.</param>
        /// <returns>The value associated with the specified key. If no results are found, the operation throws an exception.</returns>
        public TValue this[TKey key]
        {
            get
            {
                Requires.NotNullAllowStructs(key, "key");
                if (TryGetValue(key, out var value))
                {
                    return value;
                }
                throw new KeyNotFoundException(System.SR.Format(MDCFR.Properties.Resources.Arg_KeyNotFoundWithKey, key.ToString()));
            }
        }

        TValue IDictionary<TKey, TValue>.this[TKey key]
        {
            get
            {
                return this[key];
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => true;

        /// <summary>Gets a value indicating whether the <see cref="T:System.Collections.IDictionary" /> object has a fixed size.</summary>
        /// <returns>
        ///   <see langword="true" /> if the <see cref="T:System.Collections.IDictionary" /> object has a fixed size; otherwise, <see langword="false" />.</returns>
        bool IDictionary.IsFixedSize => true;

        /// <summary>Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</summary>
        /// <returns>
        ///   <see langword="true" /> if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, <see langword="false" />.</returns>
        bool IDictionary.IsReadOnly => true;

        /// <summary>Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2" />.</summary>
        /// <returns>An <see cref="T:System.Collections.Generic.ICollection`1" /> containing the keys of the object that implements <see cref="T:System.Collections.Generic.IDictionary`2" />.</returns>
        ICollection IDictionary.Keys => new KeysCollectionAccessor<TKey, TValue>(this);

        /// <summary>Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2" />.</summary>
        /// <returns>An <see cref="T:System.Collections.Generic.ICollection`1" /> containing the values in the object that implements <see cref="T:System.Collections.Generic.IDictionary`2" />.</returns>
        ICollection IDictionary.Values => new ValuesCollectionAccessor<TKey, TValue>(this);

        internal SortedInt32KeyNode<HashBucket> Root => _root;

        /// <summary>Gets or sets the element with the specified key.</summary>
        /// <param name="key">The key.</param>
        /// <returns>The value stored under the specified key.</returns>
        object? IDictionary.this[object key]
        {
            get
            {
                return this[(TKey)key];
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.</summary>
        /// <returns>An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.</returns>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        object ICollection.SyncRoot => this;

        /// <summary>Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe).</summary>
        /// <returns>
        ///   <see langword="true" /> if access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe); otherwise, <see langword="false" />.</returns>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool ICollection.IsSynchronized => true;

        private ImmutableDictionary(SortedInt32KeyNode<HashBucket> root, Comparers comparers, int count)
            : this(Requires.NotNullPassthrough(comparers, "comparers"))
        {
            Requires.NotNull(root, "root");
            root.Freeze(s_FreezeBucketAction);
            _root = root;
            _count = count;
        }

        private ImmutableDictionary(Comparers comparers = null)
        {
            _comparers = comparers ?? Comparers.Get(EqualityComparer<TKey>.Default, EqualityComparer<TValue>.Default);
            _root = SortedInt32KeyNode<HashBucket>.EmptyNode;
        }

        /// <summary>Retrieves an empty immutable dictionary that has the same ordering and key/value comparison rules as this dictionary instance.</summary>
        /// <returns>An empty dictionary with equivalent ordering and key/value comparison rules.</returns>
        public ImmutableDictionary<TKey, TValue> Clear()
        {
            if (!IsEmpty)
            {
                return EmptyWithComparers(_comparers);
            }
            return this;
        }

        IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.Clear()
        {
            return Clear();
        }

        /// <summary>Creates an immutable dictionary with the same contents as this dictionary that can be efficiently mutated across multiple operations by using standard mutable interfaces.</summary>
        /// <returns>A collection with the same contents as this dictionary that can be efficiently mutated across multiple operations by using standard mutable interfaces.</returns>
        public Builder ToBuilder()
        {
            return new Builder(this);
        }

        /// <summary>Adds an element with the specified key and value to the immutable dictionary.</summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add.</param>
        /// <exception cref="T:System.ArgumentException">The given key already exists in the dictionary but has a different value.</exception>
        /// <returns>A new immutable dictionary that contains the additional key/value pair.</returns>
        public ImmutableDictionary<TKey, TValue> Add(TKey key, TValue value)
        {
            Requires.NotNullAllowStructs(key, "key");
            return Add(key, value, KeyCollisionBehavior.ThrowIfValueDifferent, Origin).Finalize(this);
        }

        /// <summary>Adds the specified key/value pairs to the immutable dictionary.</summary>
        /// <param name="pairs">The key/value pairs to add.</param>
        /// <exception cref="T:System.ArgumentException">One of the given keys already exists in the dictionary but has a different value.</exception>
        /// <returns>A new immutable dictionary that contains the additional key/value pairs.</returns>
        public ImmutableDictionary<TKey, TValue> AddRange(IEnumerable<KeyValuePair<TKey, TValue>> pairs)
        {
            Requires.NotNull(pairs, "pairs");
            return AddRange(pairs, avoidToHashMap: false);
        }

        /// <summary>Sets the specified key and value in the immutable dictionary, possibly overwriting an existing value for the key.</summary>
        /// <param name="key">The key of the entry to add.</param>
        /// <param name="value">The key value to set.</param>
        /// <returns>A new immutable dictionary that contains the specified key/value pair.</returns>
        public ImmutableDictionary<TKey, TValue> SetItem(TKey key, TValue value)
        {
            Requires.NotNullAllowStructs(key, "key");
            return Add(key, value, KeyCollisionBehavior.SetValue, Origin).Finalize(this);
        }

        /// <summary>Sets the specified key/value pairs in the immutable dictionary, possibly overwriting existing values for the keys.</summary>
        /// <param name="items">The key/value pairs to set in the dictionary. If any of the keys already exist in the dictionary, this method will overwrite their previous values.</param>
        /// <returns>A new immutable dictionary that contains the specified key/value pairs.</returns>
        public ImmutableDictionary<TKey, TValue> SetItems(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            Requires.NotNull(items, "items");
            return AddRange(items, Origin, KeyCollisionBehavior.SetValue).Finalize(this);
        }

        /// <summary>Removes the element with the specified key from the immutable dictionary.</summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>A new immutable dictionary with the specified element removed; or this instance if the specified key cannot be found in the dictionary.</returns>
        public ImmutableDictionary<TKey, TValue> Remove(TKey key)
        {
            Requires.NotNullAllowStructs(key, "key");
            return Remove(key, Origin).Finalize(this);
        }

        /// <summary>Removes the elements with the specified keys from the immutable dictionary.</summary>
        /// <param name="keys">The keys of the elements to remove.</param>
        /// <returns>A new immutable dictionary with the specified keys removed; or this instance if the specified keys cannot be found in the dictionary.</returns>
        public ImmutableDictionary<TKey, TValue> RemoveRange(IEnumerable<TKey> keys)
        {
            Requires.NotNull(keys, "keys");
            int num = _count;
            SortedInt32KeyNode<HashBucket> sortedInt32KeyNode = _root;
            foreach (TKey key in keys)
            {
                int hashCode = KeyComparer.GetHashCode(key);
                if (sortedInt32KeyNode.TryGetValue(hashCode, out var value))
                {
                    OperationResult result;
                    HashBucket newBucket = value.Remove(key, _comparers.KeyOnlyComparer, out result);
                    sortedInt32KeyNode = UpdateRoot(sortedInt32KeyNode, hashCode, newBucket, _comparers.HashBucketEqualityComparer);
                    if (result == OperationResult.SizeChanged)
                    {
                        num--;
                    }
                }
            }
            return Wrap(sortedInt32KeyNode, num);
        }

        /// <summary>Determines whether the immutable dictionary contains an element with the specified key.</summary>
        /// <param name="key">The key to locate.</param>
        /// <returns>
        ///   <see langword="true" /> if the immutable dictionary contains an element with the specified key; otherwise, <see langword="false" />.</returns>
        public bool ContainsKey(TKey key)
        {
            Requires.NotNullAllowStructs(key, "key");
            return ContainsKey(key, Origin);
        }

        /// <summary>Determines whether this immutable dictionary contains the specified key/value pair.</summary>
        /// <param name="pair">The key/value pair to locate.</param>
        /// <returns>
        ///   <see langword="true" /> if the specified key/value pair is found in the dictionary; otherwise, <see langword="false" />.</returns>
        public bool Contains(KeyValuePair<TKey, TValue> pair)
        {
            return Contains(pair, Origin);
        }

        /// <summary>Gets the value associated with the specified key.</summary>
        /// <param name="key">The key whose value will be retrieved.</param>
        /// <param name="value">When this method returns, contains the value associated with the specified key, if the key is found; otherwise, contains the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="key" /> is null.</exception>
        /// <returns>
        ///   <see langword="true" /> if the object that implements the dictionary contains an element with the specified key; otherwise, <see langword="false" />.</returns>
        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
        {
            Requires.NotNullAllowStructs(key, "key");
            return TryGetValue(key, Origin, out value);
        }

        /// <summary>Determines whether this dictionary contains a specified key.</summary>
        /// <param name="equalKey">The key to search for.</param>
        /// <param name="actualKey">The matching key located in the dictionary if found, or <c>equalkey</c> if no match is found.</param>
        /// <returns>
        ///   <see langword="true" /> if a match for <paramref name="equalKey" /> is found; otherwise, <see langword="false" />.</returns>
        public bool TryGetKey(TKey equalKey, out TKey actualKey)
        {
            Requires.NotNullAllowStructs(equalKey, "equalKey");
            return TryGetKey(equalKey, Origin, out actualKey);
        }

        /// <summary>Gets an instance of the immutable dictionary that uses the specified key and value comparers.</summary>
        /// <param name="keyComparer">The key comparer to use.</param>
        /// <param name="valueComparer">The value comparer to use.</param>
        /// <returns>An instance of the immutable dictionary that uses the given comparers.</returns>
        public ImmutableDictionary<TKey, TValue> WithComparers(IEqualityComparer<TKey>? keyComparer, IEqualityComparer<TValue>? valueComparer)
        {
            if (keyComparer == null)
            {
                keyComparer = EqualityComparer<TKey>.Default;
            }
            if (valueComparer == null)
            {
                valueComparer = EqualityComparer<TValue>.Default;
            }
            if (KeyComparer == keyComparer)
            {
                if (ValueComparer == valueComparer)
                {
                    return this;
                }
                Comparers comparers = _comparers.WithValueComparer(valueComparer);
                return new ImmutableDictionary<TKey, TValue>(_root, comparers, _count);
            }
            Comparers comparers2 = Comparers.Get(keyComparer, valueComparer);
            ImmutableDictionary<TKey, TValue> immutableDictionary = new ImmutableDictionary<TKey, TValue>(comparers2);
            return immutableDictionary.AddRange(this, avoidToHashMap: true);
        }

        /// <summary>Gets an instance of the immutable dictionary that uses the specified key comparer.</summary>
        /// <param name="keyComparer">The key comparer to use.</param>
        /// <returns>An instance of the immutable dictionary that uses the given comparer.</returns>
        public ImmutableDictionary<TKey, TValue> WithComparers(IEqualityComparer<TKey>? keyComparer)
        {
            return WithComparers(keyComparer, _comparers.ValueComparer);
        }

        /// <summary>Determines whether the immutable dictionary contains an element with the specified value.</summary>
        /// <param name="value">The value to locate. The value can be <see langword="null" /> for reference types.</param>
        /// <returns>
        ///   <see langword="true" /> if the dictionary contains an element with the specified value; otherwise, <see langword="false" />.</returns>
        public bool ContainsValue(TValue value)
        {
            using (Enumerator enumerator = GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    KeyValuePair<TKey, TValue> current = enumerator.Current;
                    if (ValueComparer.Equals(value, current.Value))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>Returns an enumerator that iterates through the immutable dictionary.</summary>
        /// <returns>An enumerator that can be used to iterate through the dictionary.</returns>
        public Enumerator GetEnumerator()
        {
            return new Enumerator(_root);
        }

        IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.Add(TKey key, TValue value)
        {
            return Add(key, value);
        }

        IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.SetItem(TKey key, TValue value)
        {
            return SetItem(key, value);
        }

        IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.SetItems(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            return SetItems(items);
        }

        IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.AddRange(IEnumerable<KeyValuePair<TKey, TValue>> pairs)
        {
            return AddRange(pairs);
        }

        IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.RemoveRange(IEnumerable<TKey> keys)
        {
            return RemoveRange(keys);
        }

        IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.Remove(TKey key)
        {
            return Remove(key);
        }

        void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
        {
            throw new NotSupportedException();
        }

        bool IDictionary<TKey, TValue>.Remove(TKey key)
        {
            throw new NotSupportedException();
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            throw new NotSupportedException();
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Clear()
        {
            throw new NotSupportedException();
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            throw new NotSupportedException();
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            Requires.NotNull(array, "array");
            Requires.Range(arrayIndex >= 0, "arrayIndex");
            Requires.Range(array.Length >= arrayIndex + Count, "arrayIndex");
            using Enumerator enumerator = GetEnumerator();
            while (enumerator.MoveNext())
            {
                KeyValuePair<TKey, TValue> current = enumerator.Current;
                array[arrayIndex++] = current;
            }
        }

        /// <summary>Adds an element with the provided key and value to the immutable dictionary object.</summary>
        /// <param name="key">The object to use as the key of the element to add.</param>
        /// <param name="value">The object to use as the value of the element to add.</param>
        void IDictionary.Add(object key, object value)
        {
            throw new NotSupportedException();
        }

        /// <summary>Determines whether the immutable dictionary object contains an element with the specified key.</summary>
        /// <param name="key">The key to locate in the dictionary object.</param>
        /// <returns>
        ///   <see langword="true" /> if the dictionary contains an element with the key; otherwise, <see langword="false" />.</returns>
        bool IDictionary.Contains(object key)
        {
            return ContainsKey((TKey)key);
        }

        /// <summary>Returns an <see cref="T:System.Collections.IDictionaryEnumerator" /> object for the immutable dictionary object.</summary>
        /// <returns>An enumerator object for the dictionary object.</returns>
        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return new DictionaryEnumerator<TKey, TValue>(GetEnumerator());
        }

        /// <summary>Removes the element with the specified key from the immutable dictionary object.</summary>
        /// <param name="key">The key of the element to remove.</param>
        void IDictionary.Remove(object key)
        {
            throw new NotSupportedException();
        }

        /// <summary>Clears this instance.</summary>
        /// <exception cref="T:System.NotSupportedException">The dictionary object is read-only.</exception>
        void IDictionary.Clear()
        {
            throw new NotSupportedException();
        }

        /// <summary>Copies the elements of the dictionary to an array, starting at a particular array index.</summary>
        /// <param name="array">The one-dimensional array that is the destination of the elements copied from the dictionary. The array must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        void ICollection.CopyTo(Array array, int arrayIndex)
        {
            Requires.NotNull(array, "array");
            Requires.Range(arrayIndex >= 0, "arrayIndex");
            Requires.Range(array.Length >= arrayIndex + Count, "arrayIndex");
            using Enumerator enumerator = GetEnumerator();
            while (enumerator.MoveNext())
            {
                KeyValuePair<TKey, TValue> current = enumerator.Current;
                array.SetValue(new DictionaryEntry(current.Key, current.Value), arrayIndex++);
            }
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            if (!IsEmpty)
            {
                return GetEnumerator();
            }
            return Enumerable.Empty<KeyValuePair<TKey, TValue>>().GetEnumerator();
        }

        /// <summary>Returns an enumerator that iterates through a collection.</summary>
        /// <returns>An enumerator object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private static ImmutableDictionary<TKey, TValue> EmptyWithComparers(Comparers comparers)
        {
            Requires.NotNull(comparers, "comparers");
            if (Empty._comparers != comparers)
            {
                return new ImmutableDictionary<TKey, TValue>(comparers);
            }
            return Empty;
        }

        private static bool TryCastToImmutableMap(IEnumerable<KeyValuePair<TKey, TValue>> sequence, [NotNullWhen(true)] out ImmutableDictionary<TKey, TValue> other)
        {
            other = sequence as ImmutableDictionary<TKey, TValue>;
            if (other != null)
            {
                return true;
            }
            if (sequence is Builder builder)
            {
                other = builder.ToImmutable();
                return true;
            }
            return false;
        }

        private static bool ContainsKey(TKey key, MutationInput origin)
        {
            int hashCode = origin.KeyComparer.GetHashCode(key);
            TValue value2;
            if (origin.Root.TryGetValue(hashCode, out var value))
            {
                return value.TryGetValue(key, origin.Comparers, out value2);
            }
            return false;
        }

        private static bool Contains(KeyValuePair<TKey, TValue> keyValuePair, MutationInput origin)
        {
            int hashCode = origin.KeyComparer.GetHashCode(keyValuePair.Key);
            if (origin.Root.TryGetValue(hashCode, out var value))
            {
                if (value.TryGetValue(keyValuePair.Key, origin.Comparers, out var value2))
                {
                    return origin.ValueComparer.Equals(value2, keyValuePair.Value);
                }
                return false;
            }
            return false;
        }

        private static bool TryGetValue(TKey key, MutationInput origin, [MaybeNullWhen(false)] out TValue value)
        {
            int hashCode = origin.KeyComparer.GetHashCode(key);
            if (origin.Root.TryGetValue(hashCode, out var value2))
            {
                return value2.TryGetValue(key, origin.Comparers, out value);
            }
            value = default(TValue);
            return false;
        }

        private static bool TryGetKey(TKey equalKey, MutationInput origin, out TKey actualKey)
        {
            int hashCode = origin.KeyComparer.GetHashCode(equalKey);
            if (origin.Root.TryGetValue(hashCode, out var value))
            {
                return value.TryGetKey(equalKey, origin.Comparers, out actualKey);
            }
            actualKey = equalKey;
            return false;
        }

        private static MutationResult Add(TKey key, TValue value, KeyCollisionBehavior behavior, MutationInput origin)
        {
            Requires.NotNullAllowStructs(key, "key");
            int hashCode = origin.KeyComparer.GetHashCode(key);
            OperationResult result;
            HashBucket newBucket = origin.Root.GetValueOrDefault(hashCode).Add(key, value, origin.KeyOnlyComparer, origin.ValueComparer, behavior, out result);
            if (result == OperationResult.NoChangeRequired)
            {
                return new MutationResult(origin);
            }
            SortedInt32KeyNode<HashBucket> root = UpdateRoot(origin.Root, hashCode, newBucket, origin.HashBucketComparer);
            return new MutationResult(root, (result == OperationResult.SizeChanged) ? 1 : 0);
        }

        private static MutationResult AddRange(IEnumerable<KeyValuePair<TKey, TValue>> items, MutationInput origin, KeyCollisionBehavior collisionBehavior = KeyCollisionBehavior.ThrowIfValueDifferent)
        {
            Requires.NotNull(items, "items");
            int num = 0;
            SortedInt32KeyNode<HashBucket> sortedInt32KeyNode = origin.Root;
            foreach (KeyValuePair<TKey, TValue> item in items)
            {
                Requires.NotNullAllowStructs(item.Key, "Key");
                int hashCode = origin.KeyComparer.GetHashCode(item.Key);
                OperationResult result;
                HashBucket newBucket = sortedInt32KeyNode.GetValueOrDefault(hashCode).Add(item.Key, item.Value, origin.KeyOnlyComparer, origin.ValueComparer, collisionBehavior, out result);
                sortedInt32KeyNode = UpdateRoot(sortedInt32KeyNode, hashCode, newBucket, origin.HashBucketComparer);
                if (result == OperationResult.SizeChanged)
                {
                    num++;
                }
            }
            return new MutationResult(sortedInt32KeyNode, num);
        }

        private static MutationResult Remove(TKey key, MutationInput origin)
        {
            int hashCode = origin.KeyComparer.GetHashCode(key);
            if (origin.Root.TryGetValue(hashCode, out var value))
            {
                OperationResult result;
                SortedInt32KeyNode<HashBucket> root = UpdateRoot(origin.Root, hashCode, value.Remove(key, origin.KeyOnlyComparer, out result), origin.HashBucketComparer);
                return new MutationResult(root, (result == OperationResult.SizeChanged) ? (-1) : 0);
            }
            return new MutationResult(origin);
        }

        private static SortedInt32KeyNode<HashBucket> UpdateRoot(SortedInt32KeyNode<HashBucket> root, int hashCode, HashBucket newBucket, IEqualityComparer<HashBucket> hashBucketComparer)
        {
            bool mutated;
            if (newBucket.IsEmpty)
            {
                return root.Remove(hashCode, out mutated);
            }
            bool mutated2;
            return root.SetItem(hashCode, newBucket, hashBucketComparer, out mutated, out mutated2);
        }

        private static ImmutableDictionary<TKey, TValue> Wrap(SortedInt32KeyNode<HashBucket> root, Comparers comparers, int count)
        {
            Requires.NotNull(root, "root");
            Requires.NotNull(comparers, "comparers");
            Requires.Range(count >= 0, "count");
            return new ImmutableDictionary<TKey, TValue>(root, comparers, count);
        }

        private ImmutableDictionary<TKey, TValue> Wrap(SortedInt32KeyNode<HashBucket> root, int adjustedCountIfDifferentRoot)
        {
            if (root == null)
            {
                return Clear();
            }
            if (_root != root)
            {
                if (!root.IsEmpty)
                {
                    return new ImmutableDictionary<TKey, TValue>(root, _comparers, adjustedCountIfDifferentRoot);
                }
                return Clear();
            }
            return this;
        }

        private ImmutableDictionary<TKey, TValue> AddRange(IEnumerable<KeyValuePair<TKey, TValue>> pairs, bool avoidToHashMap)
        {
            Requires.NotNull(pairs, "pairs");
            if (IsEmpty && !avoidToHashMap && TryCastToImmutableMap(pairs, out var other))
            {
                return other.WithComparers(KeyComparer, ValueComparer);
            }
            return AddRange(pairs, Origin).Finalize(this);
        }
    }

    internal sealed class ImmutableDictionaryBuilderDebuggerProxy<TKey, TValue> where TKey : notnull
    {
        private readonly ImmutableDictionary<TKey, TValue>.Builder _map;

        private KeyValuePair<TKey, TValue>[] _contents;

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public KeyValuePair<TKey, TValue>[] Contents => _contents ?? (_contents = _map.ToArray(_map.Count));

        public ImmutableDictionaryBuilderDebuggerProxy(ImmutableDictionary<TKey, TValue>.Builder map)
        {
            Requires.NotNull(map, "map");
            _map = map;
        }
    }

    internal sealed class ImmutableDictionaryDebuggerProxy<TKey, TValue> : ImmutableEnumerableDebuggerProxy<KeyValuePair<TKey, TValue>> where TKey : notnull
    {
        public ImmutableDictionaryDebuggerProxy(IImmutableDictionary<TKey, TValue> dictionary)
            : base((IEnumerable<KeyValuePair<TKey, TValue>>)dictionary)
        {
        }
    }

    internal class ImmutableEnumerableDebuggerProxy<T>
    {
        private readonly IEnumerable<T> _enumerable;

        private T[] _cachedContents;

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Contents => _cachedContents ?? (_cachedContents = _enumerable.ToArray());

        public ImmutableEnumerableDebuggerProxy(IEnumerable<T> enumerable)
        {
            Requires.NotNull(enumerable, "enumerable");
            _enumerable = enumerable;
        }
    }

    internal static class ImmutableExtensions
    {
        private sealed class ListOfTWrapper<T> : IOrderedCollection<T>, IEnumerable<T>, IEnumerable
        {
            private readonly IList<T> _collection;

            public int Count => _collection.Count;

            public T this[int index] => _collection[index];

            internal ListOfTWrapper(IList<T> collection)
            {
                Requires.NotNull(collection, "collection");
                _collection = collection;
            }

            public IEnumerator<T> GetEnumerator()
            {
                return _collection.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        private sealed class FallbackWrapper<T> : IOrderedCollection<T>, IEnumerable<T>, IEnumerable
        {
            private readonly IEnumerable<T> _sequence;

            private IList<T> _collection;

            public int Count
            {
                get
                {
                    if (_collection == null)
                    {
                        if (_sequence.TryGetCount(out var count))
                        {
                            return count;
                        }
                        _collection = _sequence.ToArray();
                    }
                    return _collection.Count;
                }
            }

            public T this[int index]
            {
                get
                {
                    if (_collection == null)
                    {
                        _collection = _sequence.ToArray();
                    }
                    return _collection[index];
                }
            }

            internal FallbackWrapper(IEnumerable<T> sequence)
            {
                Requires.NotNull(sequence, "sequence");
                _sequence = sequence;
            }

            public IEnumerator<T> GetEnumerator()
            {
                return _sequence.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        internal static bool IsValueType<T>()
        {
            if (default(T) != null)
            {
                return true;
            }
            Type typeFromHandle = typeof(T);
            if (typeFromHandle.IsConstructedGenericType && typeFromHandle.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return true;
            }
            return false;
        }

        internal static IOrderedCollection<T> AsOrderedCollection<T>(this IEnumerable<T> sequence)
        {
            Requires.NotNull(sequence, "sequence");
            if (sequence is IOrderedCollection<T> result)
            {
                return result;
            }
            if (sequence is IList<T> collection)
            {
                return new ListOfTWrapper<T>(collection);
            }
            return new FallbackWrapper<T>(sequence);
        }

        internal static void ClearFastWhenEmpty<T>(this Stack<T> stack)
        {
            if (stack.Count > 0)
            {
                stack.Clear();
            }
        }

        internal static DisposableEnumeratorAdapter<T, TEnumerator> GetEnumerableDisposable<T, TEnumerator>(this IEnumerable<T> enumerable) where TEnumerator : struct, IStrongEnumerator<T>, IEnumerator<T>
        {
            Requires.NotNull(enumerable, "enumerable");
            if (enumerable is IStrongEnumerable<T, TEnumerator> strongEnumerable)
            {
                return new DisposableEnumeratorAdapter<T, TEnumerator>(strongEnumerable.GetEnumerator());
            }
            return new DisposableEnumeratorAdapter<T, TEnumerator>(enumerable.GetEnumerator());
        }

        internal static bool TryGetCount<T>(this IEnumerable<T> sequence, out int count)
        {
            return ((IEnumerable)sequence).TryGetCount<T>(out count);
        }

        internal static bool TryGetCount<T>(this IEnumerable sequence, out int count)
        {
            if (sequence is ICollection collection)
            {
                count = collection.Count;
                return true;
            }
            if (sequence is ICollection<T> collection2)
            {
                count = collection2.Count;
                return true;
            }
            if (sequence is IReadOnlyCollection<T> readOnlyCollection)
            {
                count = readOnlyCollection.Count;
                return true;
            }
            count = 0;
            return false;
        }

        internal static int GetCount<T>(ref IEnumerable<T> sequence)
        {
            if (!sequence.TryGetCount(out var count))
            {
                List<T> list = sequence.ToList();
                count = list.Count;
                sequence = list;
            }
            return count;
        }

        internal static bool TryCopyTo<T>(this IEnumerable<T> sequence, T[] array, int arrayIndex)
        {
            if (sequence is IList<T>)
            {
                if (sequence is List<T> list)
                {
                    list.CopyTo(array, arrayIndex);
                    return true;
                }
                if (sequence.GetType() == typeof(T[]))
                {
                    T[] array2 = (T[])sequence;
                    Array.Copy(array2, 0, array, arrayIndex, array2.Length);
                    return true;
                }
                if (sequence is ImmutableArray<T> immutableArray)
                {
                    Array.Copy(immutableArray.array, 0, array, arrayIndex, immutableArray.Length);
                    return true;
                }
            }
            return false;
        }

        internal static T[] ToArray<T>(this IEnumerable<T> sequence, int count)
        {
            Requires.NotNull(sequence, "sequence");
            Requires.Range(count >= 0, "count");
            if (count == 0)
            {
                return ImmutableArray<T>.Empty.array;
            }
            T[] array = new T[count];
            if (!sequence.TryCopyTo(array, 0))
            {
                int num = 0;
                foreach (T item in sequence)
                {
                    Requires.Argument(num < count);
                    array[num++] = item;
                }
                Requires.Argument(num == count);
            }
            return array;
        }
    }

    /// <summary>Represents an immutable, unordered hash set.  </summary>
    /// <typeparam name="T">The type of elements in the hash set.</typeparam>
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(ImmutableEnumerableDebuggerProxy<>))]
    public sealed class ImmutableHashSet<T> : IImmutableSet<T>, IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable, IHashKeyCollection<T>, ICollection<T>, ISet<T>, ICollection, IStrongEnumerable<T, ImmutableHashSet<T>.Enumerator>
    {
        private sealed class HashBucketByValueEqualityComparer : IEqualityComparer<HashBucket>
        {
            private static readonly IEqualityComparer<HashBucket> s_defaultInstance = new HashBucketByValueEqualityComparer(EqualityComparer<T>.Default);

            private readonly IEqualityComparer<T> _valueComparer;

            internal static IEqualityComparer<HashBucket> DefaultInstance => s_defaultInstance;

            internal HashBucketByValueEqualityComparer(IEqualityComparer<T> valueComparer)
            {
                Requires.NotNull(valueComparer, "valueComparer");
                _valueComparer = valueComparer;
            }

            public bool Equals(HashBucket x, HashBucket y)
            {
                return x.EqualsByValue(y, _valueComparer);
            }

            public int GetHashCode(HashBucket obj)
            {
                throw new NotSupportedException();
            }
        }

        private sealed class HashBucketByRefEqualityComparer : IEqualityComparer<HashBucket>
        {
            private static readonly IEqualityComparer<HashBucket> s_defaultInstance = new HashBucketByRefEqualityComparer();

            internal static IEqualityComparer<HashBucket> DefaultInstance => s_defaultInstance;

            private HashBucketByRefEqualityComparer()
            {
            }

            public bool Equals(HashBucket x, HashBucket y)
            {
                return x.EqualsByRef(y);
            }

            public int GetHashCode(HashBucket obj)
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>Represents a hash set that mutates with little or no memory allocations and that can produce or build on immutable hash set instances very efficiently. </summary>
        [DebuggerDisplay("Count = {Count}")]
        public sealed class Builder : IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable, ISet<T>, ICollection<T>
        {
            private SortedInt32KeyNode<HashBucket> _root = SortedInt32KeyNode<HashBucket>.EmptyNode;

            private IEqualityComparer<T> _equalityComparer;

            private readonly IEqualityComparer<HashBucket> _hashBucketEqualityComparer;

            private int _count;

            private ImmutableHashSet<T> _immutable;

            private int _version;

            /// <summary>Gets the number of elements contained in the immutable hash set.</summary>
            /// <returns>The number of elements contained in the immutable hash set.</returns>
            public int Count => _count;

            /// <summary>Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</summary>
            /// <returns>
            ///   <see langword="true" /> if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, <see langword="false" />.</returns>
            bool ICollection<T>.IsReadOnly => false;

            /// <summary>Gets or sets the key comparer.</summary>
            /// <returns>The key comparer.</returns>
            public IEqualityComparer<T> KeyComparer
            {
                get
                {
                    return _equalityComparer;
                }
                set
                {
                    Requires.NotNull(value, "value");
                    if (value != _equalityComparer)
                    {
                        MutationResult mutationResult = ImmutableHashSet<T>.Union((IEnumerable<T>)this, new MutationInput(SortedInt32KeyNode<HashBucket>.EmptyNode, value, _hashBucketEqualityComparer, 0));
                        _immutable = null;
                        _equalityComparer = value;
                        Root = mutationResult.Root;
                        _count = mutationResult.Count;
                    }
                }
            }

            internal int Version => _version;

            private MutationInput Origin => new MutationInput(Root, _equalityComparer, _hashBucketEqualityComparer, _count);

            private SortedInt32KeyNode<HashBucket> Root
            {
                get
                {
                    return _root;
                }
                set
                {
                    _version++;
                    if (_root != value)
                    {
                        _root = value;
                        _immutable = null;
                    }
                }
            }

            internal Builder(ImmutableHashSet<T> set)
            {
                Requires.NotNull(set, "set");
                _root = set._root;
                _count = set._count;
                _equalityComparer = set._equalityComparer;
                _hashBucketEqualityComparer = set._hashBucketEqualityComparer;
                _immutable = set;
            }

            /// <summary>Returns an enumerator that iterates through the immutable hash set.</summary>
            /// <returns>An enumerator that can be used to iterate through the set.</returns>
            public Enumerator GetEnumerator()
            {
                return new Enumerator(_root, this);
            }

            /// <summary>Creates an immutable hash set based on the contents of this instance.</summary>
            /// <returns>An immutable set.</returns>
            public ImmutableHashSet<T> ToImmutable()
            {
                return _immutable ?? (_immutable = ImmutableHashSet<T>.Wrap(_root, _equalityComparer, _count));
            }

            /// <summary>Searches the set for a given value and returns the equal value it finds, if any.</summary>
            /// <param name="equalValue">The value for which to search.</param>
            /// <param name="actualValue">The value from the set that the search found, or the original value if the search yielded no match.</param>
            /// <returns>A value indicating whether the search was successful.</returns>
            public bool TryGetValue(T equalValue, out T actualValue)
            {
                int key = ((equalValue != null) ? _equalityComparer.GetHashCode(equalValue) : 0);
                if (_root.TryGetValue(key, out var value))
                {
                    return value.TryExchange(equalValue, _equalityComparer, out actualValue);
                }
                actualValue = equalValue;
                return false;
            }

            /// <summary>Adds the specified item to the immutable hash set.</summary>
            /// <param name="item">The item to add.</param>
            /// <returns>
            ///   <see langword="true" /> if the item did not already belong to the collection; otherwise, <see langword="false" />.</returns>
            public bool Add(T item)
            {
                MutationResult result = ImmutableHashSet<T>.Add(item, Origin);
                Apply(result);
                return result.Count != 0;
            }

            /// <summary>Removes the first occurrence of a specific object from the immutable hash set.</summary>
            /// <param name="item">The object to remove from the set.</param>
            /// <exception cref="T:System.NotSupportedException">The set is read-only.</exception>
            /// <returns>
            ///   <see langword="true" /> if <paramref name="item" /> was successfully removed from the set ; otherwise, <see langword="false" />. This method also returns <see langword="false" /> if <paramref name="item" /> is not found in the original set.</returns>
            public bool Remove(T item)
            {
                MutationResult result = ImmutableHashSet<T>.Remove(item, Origin);
                Apply(result);
                return result.Count != 0;
            }

            /// <summary>Determines whether the immutable hash set contains a specific value.</summary>
            /// <param name="item">The object to locate in the hash set.</param>
            /// <returns>
            ///   <see langword="true" /> if <paramref name="item" /> is found in the hash set ; otherwise, <see langword="false" />.</returns>
            public bool Contains(T item)
            {
                return ImmutableHashSet<T>.Contains(item, Origin);
            }

            /// <summary>Removes all items from the immutable hash set.</summary>
            /// <exception cref="T:System.NotSupportedException">The hash set is read-only.</exception>
            public void Clear()
            {
                _count = 0;
                Root = SortedInt32KeyNode<HashBucket>.EmptyNode;
            }

            /// <summary>Removes all elements in the specified collection from the current hash set.</summary>
            /// <param name="other">The collection of items to remove from the set.</param>
            public void ExceptWith(IEnumerable<T> other)
            {
                MutationResult result = ImmutableHashSet<T>.Except(other, _equalityComparer, _hashBucketEqualityComparer, _root);
                Apply(result);
            }

            /// <summary>Modifies the current set so that it contains only elements that are also in a specified collection.</summary>
            /// <param name="other">The collection to compare to the current set.</param>
            public void IntersectWith(IEnumerable<T> other)
            {
                MutationResult result = ImmutableHashSet<T>.Intersect(other, Origin);
                Apply(result);
            }

            /// <summary>Determines whether the current set is a proper (strict) subset of a specified collection.</summary>
            /// <param name="other">The collection to compare to the current set.</param>
            /// <returns>
            ///   <see langword="true" /> if the current set is a proper subset of <paramref name="other" />; otherwise, <see langword="false" />.</returns>
            public bool IsProperSubsetOf(IEnumerable<T> other)
            {
                return ImmutableHashSet<T>.IsProperSubsetOf(other, Origin);
            }

            /// <summary>Determines whether the current set is a proper (strict) superset of a specified collection.</summary>
            /// <param name="other">The collection to compare to the current set.</param>
            /// <returns>
            ///   <see langword="true" /> if the current set is a proper superset of <paramref name="other" />; otherwise, <see langword="false" />.</returns>
            public bool IsProperSupersetOf(IEnumerable<T> other)
            {
                return ImmutableHashSet<T>.IsProperSupersetOf(other, Origin);
            }

            /// <summary>Determines whether the current set is a subset of a specified collection.</summary>
            /// <param name="other">The collection to compare to the current set.</param>
            /// <returns>
            ///   <see langword="true" /> if the current set is a subset of <paramref name="other" />; otherwise, <see langword="false" />.</returns>
            public bool IsSubsetOf(IEnumerable<T> other)
            {
                return ImmutableHashSet<T>.IsSubsetOf(other, Origin);
            }

            /// <summary>Determines whether the current set is a superset of a specified collection.</summary>
            /// <param name="other">The collection to compare to the current set.</param>
            /// <returns>
            ///   <see langword="true" /> if the current set is a superset of <paramref name="other" />; otherwise, <see langword="false" />.</returns>
            public bool IsSupersetOf(IEnumerable<T> other)
            {
                return ImmutableHashSet<T>.IsSupersetOf(other, Origin);
            }

            /// <summary>Determines whether the current set overlaps with the specified collection.</summary>
            /// <param name="other">The collection to compare to the current set.</param>
            /// <returns>
            ///   <see langword="true" /> if the current set and <paramref name="other" /> share at least one common element; otherwise, <see langword="false" />.</returns>
            public bool Overlaps(IEnumerable<T> other)
            {
                return ImmutableHashSet<T>.Overlaps(other, Origin);
            }

            /// <summary>Determines whether the current set and the specified collection contain the same elements.</summary>
            /// <param name="other">The collection to compare to the current set.</param>
            /// <returns>
            ///   <see langword="true" /> if the current set is equal to <paramref name="other" />; otherwise, <see langword="false" />.</returns>
            public bool SetEquals(IEnumerable<T> other)
            {
                if (this == other)
                {
                    return true;
                }
                return ImmutableHashSet<T>.SetEquals(other, Origin);
            }

            /// <summary>Modifies the current set so that it contains only elements that are present either in the current set or in the specified collection, but not both.</summary>
            /// <param name="other">The collection to compare to the current set.</param>
            public void SymmetricExceptWith(IEnumerable<T> other)
            {
                MutationResult result = ImmutableHashSet<T>.SymmetricExcept(other, Origin);
                Apply(result);
            }

            /// <summary>Modifies the current set so that it contains all elements that are present in both the current set and in the specified collection.</summary>
            /// <param name="other">The collection to compare to the current set.</param>
            public void UnionWith(IEnumerable<T> other)
            {
                MutationResult result = ImmutableHashSet<T>.Union(other, Origin);
                Apply(result);
            }

            /// <summary>Adds an item to the hash set.</summary>
            /// <param name="item">The object to add to the set.</param>
            /// <exception cref="T:System.NotSupportedException">The set is read-only.</exception>
            void ICollection<T>.Add(T item)
            {
                Add(item);
            }

            /// <summary>Copies the elements of the hash set to an array, starting at a particular array index.</summary>
            /// <param name="array">The one-dimensional array that is the destination of the elements copied from the hash set. The array must have zero-based indexing.</param>
            /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
            void ICollection<T>.CopyTo(T[] array, int arrayIndex)
            {
                Requires.NotNull(array, "array");
                Requires.Range(arrayIndex >= 0, "arrayIndex");
                Requires.Range(array.Length >= arrayIndex + Count, "arrayIndex");
                using Enumerator enumerator = GetEnumerator();
                while (enumerator.MoveNext())
                {
                    T current = enumerator.Current;
                    array[arrayIndex++] = current;
                }
            }

            /// <summary>Returns an enumerator that iterates through the collection.</summary>
            /// <returns>An enumerator that can be used to iterate through the collection.</returns>
            IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                return GetEnumerator();
            }

            /// <summary>Returns an enumerator that iterates through a collection.</summary>
            /// <returns>An enumerator that can be used to iterate through the collection.</returns>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            private void Apply(MutationResult result)
            {
                Root = result.Root;
                if (result.CountType == CountType.Adjustment)
                {
                    _count += result.Count;
                }
                else
                {
                    _count = result.Count;
                }
            }
        }

        /// <summary>Enumerates the contents of the immutable hash set without allocating any memory. </summary>
        public struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator, IStrongEnumerator<T>
        {
            private readonly Builder _builder;

            private SortedInt32KeyNode<HashBucket>.Enumerator _mapEnumerator;

            private HashBucket.Enumerator _bucketEnumerator;

            private int _enumeratingBuilderVersion;

            /// <summary>Gets the element at the current position of the enumerator.</summary>
            /// <returns>The element at the current position of the enumerator.</returns>
            public T Current
            {
                get
                {
                    _mapEnumerator.ThrowIfDisposed();
                    return _bucketEnumerator.Current;
                }
            }

            /// <summary>Gets the current element.</summary>
            /// <returns>The element in the collection at the current position of the enumerator.</returns>
            object? IEnumerator.Current => Current;

            internal Enumerator(SortedInt32KeyNode<HashBucket> root, Builder? builder = null)
            {
                _builder = builder;
                _mapEnumerator = new SortedInt32KeyNode<HashBucket>.Enumerator(root);
                _bucketEnumerator = default(HashBucket.Enumerator);
                _enumeratingBuilderVersion = builder?.Version ?? (-1);
            }

            /// <summary>Advances the enumerator to the next element of the immutable hash set.</summary>
            /// <exception cref="T:System.InvalidOperationException">The hash set was modified after the enumerator was created.</exception>
            /// <returns>
            ///   <see langword="true" /> if the enumerator was successfully advanced to the next element; <see langword="false" /> if the enumerator has passed the end of the hash set.</returns>
            public bool MoveNext()
            {
                ThrowIfChanged();
                if (_bucketEnumerator.MoveNext())
                {
                    return true;
                }
                if (_mapEnumerator.MoveNext())
                {
                    _bucketEnumerator = new HashBucket.Enumerator(_mapEnumerator.Current.Value);
                    return _bucketEnumerator.MoveNext();
                }
                return false;
            }

            /// <summary>Sets the enumerator to its initial position, which is before the first element in the hash set.</summary>
            /// <exception cref="T:System.InvalidOperationException">The hash set was modified after the enumerator was created.</exception>
            public void Reset()
            {
                _enumeratingBuilderVersion = ((_builder != null) ? _builder.Version : (-1));
                _mapEnumerator.Reset();
                _bucketEnumerator.Dispose();
                _bucketEnumerator = default(HashBucket.Enumerator);
            }

            /// <summary>Releases the resources used by the current instance of the <see cref="T:System.Collections.Immutable.ImmutableHashSet`1.Enumerator" /> class.</summary>
            public void Dispose()
            {
                _mapEnumerator.Dispose();
                _bucketEnumerator.Dispose();
            }

            private void ThrowIfChanged()
            {
                if (_builder != null && _builder.Version != _enumeratingBuilderVersion)
                {
                    throw new InvalidOperationException(MDCFR.Properties.Resources.CollectionModifiedDuringEnumeration);
                }
            }
        }

        internal enum OperationResult
        {
            SizeChanged,
            NoChangeRequired
        }

        internal readonly struct HashBucket
        {
            internal struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator
            {
                private enum Position
                {
                    BeforeFirst,
                    First,
                    Additional,
                    End
                }

                private readonly HashBucket _bucket;

                private bool _disposed;

                private Position _currentPosition;

                private ImmutableList<T>.Enumerator _additionalEnumerator;

                object? IEnumerator.Current => Current;

                public T Current
                {
                    get
                    {
                        ThrowIfDisposed();
                        return _currentPosition switch
                        {
                            Position.First => _bucket._firstValue,
                            Position.Additional => _additionalEnumerator.Current,
                            _ => throw new InvalidOperationException(),
                        };
                    }
                }

                internal Enumerator(HashBucket bucket)
                {
                    _disposed = false;
                    _bucket = bucket;
                    _currentPosition = Position.BeforeFirst;
                    _additionalEnumerator = default(ImmutableList<T>.Enumerator);
                }

                public bool MoveNext()
                {
                    ThrowIfDisposed();
                    if (_bucket.IsEmpty)
                    {
                        _currentPosition = Position.End;
                        return false;
                    }
                    switch (_currentPosition)
                    {
                        case Position.BeforeFirst:
                            _currentPosition = Position.First;
                            return true;
                        case Position.First:
                            if (_bucket._additionalElements.IsEmpty)
                            {
                                _currentPosition = Position.End;
                                return false;
                            }
                            _currentPosition = Position.Additional;
                            _additionalEnumerator = new ImmutableList<T>.Enumerator(_bucket._additionalElements);
                            return _additionalEnumerator.MoveNext();
                        case Position.Additional:
                            return _additionalEnumerator.MoveNext();
                        case Position.End:
                            return false;
                        default:
                            throw new InvalidOperationException();
                    }
                }

                public void Reset()
                {
                    ThrowIfDisposed();
                    _additionalEnumerator.Dispose();
                    _currentPosition = Position.BeforeFirst;
                }

                public void Dispose()
                {
                    _disposed = true;
                    _additionalEnumerator.Dispose();
                }

                private void ThrowIfDisposed()
                {
                    if (_disposed)
                    {
                        Requires.FailObjectDisposed(this);
                    }
                }
            }

            private readonly T _firstValue;

            private readonly ImmutableList<T>.Node _additionalElements;

            internal bool IsEmpty => _additionalElements == null;

            private HashBucket(T firstElement, ImmutableList<T>.Node additionalElements = null)
            {
                _firstValue = firstElement;
                _additionalElements = additionalElements ?? ImmutableList<T>.Node.EmptyNode;
            }

            public Enumerator GetEnumerator()
            {
                return new Enumerator(this);
            }

            public override bool Equals(object? obj)
            {
                throw new NotSupportedException();
            }

            public override int GetHashCode()
            {
                throw new NotSupportedException();
            }

            internal bool EqualsByRef(HashBucket other)
            {
                if ((object)_firstValue == (object)other._firstValue)
                {
                    return _additionalElements == other._additionalElements;
                }
                return false;
            }

            internal bool EqualsByValue(HashBucket other, IEqualityComparer<T> valueComparer)
            {
                if (valueComparer.Equals(_firstValue, other._firstValue))
                {
                    return _additionalElements == other._additionalElements;
                }
                return false;
            }

            internal HashBucket Add(T value, IEqualityComparer<T> valueComparer, out OperationResult result)
            {
                if (IsEmpty)
                {
                    result = OperationResult.SizeChanged;
                    return new HashBucket(value);
                }
                if (valueComparer.Equals(value, _firstValue) || _additionalElements.IndexOf(value, valueComparer) >= 0)
                {
                    result = OperationResult.NoChangeRequired;
                    return this;
                }
                result = OperationResult.SizeChanged;
                return new HashBucket(_firstValue, _additionalElements.Add(value));
            }

            internal bool Contains(T value, IEqualityComparer<T> valueComparer)
            {
                if (IsEmpty)
                {
                    return false;
                }
                if (!valueComparer.Equals(value, _firstValue))
                {
                    return _additionalElements.IndexOf(value, valueComparer) >= 0;
                }
                return true;
            }

            internal bool TryExchange(T value, IEqualityComparer<T> valueComparer, out T existingValue)
            {
                if (!IsEmpty)
                {
                    if (valueComparer.Equals(value, _firstValue))
                    {
                        existingValue = _firstValue;
                        return true;
                    }
                    int num = _additionalElements.IndexOf(value, valueComparer);
                    if (num >= 0)
                    {
                        existingValue = _additionalElements.ItemRef(num);
                        return true;
                    }
                }
                existingValue = value;
                return false;
            }

            internal HashBucket Remove(T value, IEqualityComparer<T> equalityComparer, out OperationResult result)
            {
                if (IsEmpty)
                {
                    result = OperationResult.NoChangeRequired;
                    return this;
                }
                if (equalityComparer.Equals(_firstValue, value))
                {
                    if (_additionalElements.IsEmpty)
                    {
                        result = OperationResult.SizeChanged;
                        return default(HashBucket);
                    }
                    int count = _additionalElements.Left.Count;
                    result = OperationResult.SizeChanged;
                    return new HashBucket(_additionalElements.Key, _additionalElements.RemoveAt(count));
                }
                int num = _additionalElements.IndexOf(value, equalityComparer);
                if (num < 0)
                {
                    result = OperationResult.NoChangeRequired;
                    return this;
                }
                result = OperationResult.SizeChanged;
                return new HashBucket(_firstValue, _additionalElements.RemoveAt(num));
            }

            internal void Freeze()
            {
                _additionalElements?.Freeze();
            }
        }

        private readonly struct MutationInput
        {
            private readonly SortedInt32KeyNode<HashBucket> _root;

            private readonly IEqualityComparer<T> _equalityComparer;

            private readonly int _count;

            private readonly IEqualityComparer<HashBucket> _hashBucketEqualityComparer;

            internal SortedInt32KeyNode<HashBucket> Root => _root;

            internal IEqualityComparer<T> EqualityComparer => _equalityComparer;

            internal int Count => _count;

            internal IEqualityComparer<HashBucket> HashBucketEqualityComparer => _hashBucketEqualityComparer;

            internal MutationInput(ImmutableHashSet<T> set)
            {
                Requires.NotNull(set, "set");
                _root = set._root;
                _equalityComparer = set._equalityComparer;
                _count = set._count;
                _hashBucketEqualityComparer = set._hashBucketEqualityComparer;
            }

            internal MutationInput(SortedInt32KeyNode<HashBucket> root, IEqualityComparer<T> equalityComparer, IEqualityComparer<HashBucket> hashBucketEqualityComparer, int count)
            {
                Requires.NotNull(root, "root");
                Requires.NotNull(equalityComparer, "equalityComparer");
                Requires.Range(count >= 0, "count");
                Requires.NotNull(hashBucketEqualityComparer, "hashBucketEqualityComparer");
                _root = root;
                _equalityComparer = equalityComparer;
                _count = count;
                _hashBucketEqualityComparer = hashBucketEqualityComparer;
            }
        }

        private enum CountType
        {
            Adjustment,
            FinalValue
        }

        private readonly struct MutationResult
        {
            private readonly SortedInt32KeyNode<HashBucket> _root;

            private readonly int _count;

            private readonly CountType _countType;

            internal SortedInt32KeyNode<HashBucket> Root => _root;

            internal int Count => _count;

            internal CountType CountType => _countType;

            internal MutationResult(SortedInt32KeyNode<HashBucket> root, int count, CountType countType = CountType.Adjustment)
            {
                Requires.NotNull(root, "root");
                _root = root;
                _count = count;
                _countType = countType;
            }

            internal ImmutableHashSet<T> Finalize(ImmutableHashSet<T> priorSet)
            {
                Requires.NotNull(priorSet, "priorSet");
                int num = Count;
                if (CountType == CountType.Adjustment)
                {
                    num += priorSet._count;
                }
                return priorSet.Wrap(Root, num);
            }
        }

        private readonly struct NodeEnumerable : IEnumerable<T>, IEnumerable
        {
            private readonly SortedInt32KeyNode<HashBucket> _root;

            internal NodeEnumerable(SortedInt32KeyNode<HashBucket> root)
            {
                Requires.NotNull(root, "root");
                _root = root;
            }

            public Enumerator GetEnumerator()
            {
                return new Enumerator(_root);
            }

            [ExcludeFromCodeCoverage]
            IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                return GetEnumerator();
            }

            [ExcludeFromCodeCoverage]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        /// <summary>Gets an immutable hash set for this type that uses the default <see cref="T:System.Collections.Generic.IEqualityComparer`1" />.</summary>
        public static readonly ImmutableHashSet<T> Empty = new ImmutableHashSet<T>(SortedInt32KeyNode<HashBucket>.EmptyNode, EqualityComparer<T>.Default, 0);

        private static readonly Action<KeyValuePair<int, HashBucket>> s_FreezeBucketAction = delegate (KeyValuePair<int, HashBucket> kv)
        {
            kv.Value.Freeze();
        };

        private readonly IEqualityComparer<T> _equalityComparer;

        private readonly int _count;

        private readonly SortedInt32KeyNode<HashBucket> _root;

        private readonly IEqualityComparer<HashBucket> _hashBucketEqualityComparer;

        /// <summary>Gets the number of elements in the immutable hash set.</summary>
        /// <returns>The number of elements in the hash set.</returns>
        public int Count => _count;

        /// <summary>Gets a value that indicates whether the current immutable hash set is empty.</summary>
        /// <returns>
        ///   <see langword="true" /> if this instance is empty; otherwise, <see langword="false" />.</returns>
        public bool IsEmpty => Count == 0;

        /// <summary>Gets the object that is used to obtain hash codes for the keys and to check the equality of values in the immutable hash set.</summary>
        /// <returns>The comparer used to obtain hash codes for the keys and check equality.</returns>
        public IEqualityComparer<T> KeyComparer => _equalityComparer;

        /// <summary>See <see cref="T:System.Collections.ICollection" />.</summary>
        /// <returns>An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.</returns>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        object ICollection.SyncRoot => this;

        /// <summary>See the <see cref="T:System.Collections.ICollection" /> interface.</summary>
        /// <returns>
        ///   <see langword="true" /> if access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe); otherwise, <see langword="false" />.</returns>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool ICollection.IsSynchronized => true;

        internal IBinaryTree Root => _root;

        private MutationInput Origin => new MutationInput(this);

        /// <summary>See the <see cref="T:System.Collections.Generic.ICollection`1" /> interface.</summary>
        /// <returns>
        ///   <see langword="true" /> if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, <see langword="false" />.</returns>
        bool ICollection<T>.IsReadOnly => true;

        internal ImmutableHashSet(IEqualityComparer<T> equalityComparer)
            : this(SortedInt32KeyNode<HashBucket>.EmptyNode, equalityComparer, 0)
        {
        }

        private ImmutableHashSet(SortedInt32KeyNode<HashBucket> root, IEqualityComparer<T> equalityComparer, int count)
        {
            Requires.NotNull(root, "root");
            Requires.NotNull(equalityComparer, "equalityComparer");
            root.Freeze(s_FreezeBucketAction);
            _root = root;
            _count = count;
            _equalityComparer = equalityComparer;
            _hashBucketEqualityComparer = GetHashBucketEqualityComparer(equalityComparer);
        }

        /// <summary>Retrieves an empty immutable hash set that has the same sorting and ordering semantics as this instance.</summary>
        /// <returns>An empty hash set that has the same sorting and ordering semantics as this instance.</returns>
        public ImmutableHashSet<T> Clear()
        {
            if (!IsEmpty)
            {
                return Empty.WithComparer(_equalityComparer);
            }
            return this;
        }

        /// <summary>Retrieves an empty set that has the same sorting and ordering semantics as this instance.</summary>
        /// <returns>An empty set that has the same sorting or ordering semantics as this instance.</returns>
        IImmutableSet<T> IImmutableSet<T>.Clear()
        {
            return Clear();
        }

        /// <summary>Creates an immutable hash set that has the same contents as this set and can be efficiently mutated across multiple operations by using standard mutable interfaces.</summary>
        /// <returns>A set with the same contents as this set that can be efficiently mutated across multiple operations by using standard mutable interfaces.</returns>
        public Builder ToBuilder()
        {
            return new Builder(this);
        }

        /// <summary>Adds the specified element to the hash set.</summary>
        /// <param name="item">The element to add to the set.</param>
        /// <returns>A hash set that contains the added value and any values previously held by the  <see cref="T:System.Collections.Immutable.ImmutableHashSet`1" /> object.</returns>
        public ImmutableHashSet<T> Add(T item)
        {
            return Add(item, Origin).Finalize(this);
        }

        /// <summary>Removes the specified element from this immutable hash set.</summary>
        /// <param name="item">The element to remove.</param>
        /// <returns>A new set with the specified element removed, or the current set if the element cannot be found in the set.</returns>
        public ImmutableHashSet<T> Remove(T item)
        {
            return Remove(item, Origin).Finalize(this);
        }

        /// <summary>Searches the set for a given value and returns the equal value it finds, if any.</summary>
        /// <param name="equalValue">The value to search for.</param>
        /// <param name="actualValue">The value from the set that the search found, or the original value if the search yielded no match.</param>
        /// <returns>A value indicating whether the search was successful.</returns>
        public bool TryGetValue(T equalValue, out T actualValue)
        {
            int key = ((equalValue != null) ? _equalityComparer.GetHashCode(equalValue) : 0);
            if (_root.TryGetValue(key, out var value))
            {
                return value.TryExchange(equalValue, _equalityComparer, out actualValue);
            }
            actualValue = equalValue;
            return false;
        }

        /// <summary>Creates a new immutable hash set that contains all elements that are present in either the current set or in the specified collection.</summary>
        /// <param name="other">The collection to add elements from.</param>
        /// <returns>A new immutable hash set with the items added; or the original set if all the items were already in the set.</returns>
        public ImmutableHashSet<T> Union(IEnumerable<T> other)
        {
            Requires.NotNull(other, "other");
            return Union(other, avoidWithComparer: false);
        }

        /// <summary>Creates an immutable hash set that contains elements that exist in both this set and the specified set.</summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>A new immutable set that contains any elements that exist in both sets.</returns>
        public ImmutableHashSet<T> Intersect(IEnumerable<T> other)
        {
            Requires.NotNull(other, "other");
            return Intersect(other, Origin).Finalize(this);
        }

        /// <summary>Removes the elements in the specified collection from the current immutable hash set.</summary>
        /// <param name="other">The collection of items to remove from this set.</param>
        /// <returns>A new set with the items removed; or the original set if none of the items were in the set.</returns>
        public ImmutableHashSet<T> Except(IEnumerable<T> other)
        {
            Requires.NotNull(other, "other");
            return Except(other, _equalityComparer, _hashBucketEqualityComparer, _root).Finalize(this);
        }

        /// <summary>Creates an immutable hash set that contains only elements that are present either in the current set or in the specified collection, but not both.</summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>A new set that contains the elements that are present only in the current set or in the specified collection, but not both.</returns>
        public ImmutableHashSet<T> SymmetricExcept(IEnumerable<T> other)
        {
            Requires.NotNull(other, "other");
            return SymmetricExcept(other, Origin).Finalize(this);
        }

        /// <summary>Determines whether the current immutable hash set and the specified collection contain the same elements.</summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>
        ///   <see langword="true" /> if the sets are equal; otherwise, <see langword="false" />.</returns>
        public bool SetEquals(IEnumerable<T> other)
        {
            Requires.NotNull(other, "other");
            if (this == other)
            {
                return true;
            }
            return SetEquals(other, Origin);
        }

        /// <summary>Determines whether the current immutable hash set is a proper (strict) subset of a specified collection.</summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>
        ///   <see langword="true" /> if the current set is a proper subset of the specified collection; otherwise, <see langword="false" />.</returns>
        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            Requires.NotNull(other, "other");
            return IsProperSubsetOf(other, Origin);
        }

        /// <summary>Determines whether the current immutable hash set is a proper (strict) superset of a specified collection.</summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>
        ///   <see langword="true" /> if the current set is a proper superset of the specified collection; otherwise, <see langword="false" />.</returns>
        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            Requires.NotNull(other, "other");
            return IsProperSupersetOf(other, Origin);
        }

        /// <summary>Determines whether the current immutable hash set is a subset of a specified collection.</summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>
        ///   <see langword="true" /> if the current set is a subset of the specified collection; otherwise, <see langword="false" />.</returns>
        public bool IsSubsetOf(IEnumerable<T> other)
        {
            Requires.NotNull(other, "other");
            return IsSubsetOf(other, Origin);
        }

        /// <summary>Determines whether the current immutable hash set is a superset of a specified collection.</summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>
        ///   <see langword="true" /> if the current set is a superset of the specified collection; otherwise, <see langword="false" />.</returns>
        public bool IsSupersetOf(IEnumerable<T> other)
        {
            Requires.NotNull(other, "other");
            return IsSupersetOf(other, Origin);
        }

        /// <summary>Determines whether the current immutable hash set overlaps with the specified collection.</summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>
        ///   <see langword="true" /> if the current set and the specified collection share at least one common element; otherwise, <see langword="false" />.</returns>
        public bool Overlaps(IEnumerable<T> other)
        {
            Requires.NotNull(other, "other");
            return Overlaps(other, Origin);
        }

        /// <summary>Adds the specified element to this immutable set.</summary>
        /// <param name="item">The element to add.</param>
        /// <returns>A new set with the element added, or this set if the element is already in the set.</returns>
        IImmutableSet<T> IImmutableSet<T>.Add(T item)
        {
            return Add(item);
        }

        /// <summary>Removes the specified element from this immutable set.</summary>
        /// <param name="item">The element to remove.</param>
        /// <returns>A new set with the specified element removed, or the current set if the element cannot be found in the set.</returns>
        IImmutableSet<T> IImmutableSet<T>.Remove(T item)
        {
            return Remove(item);
        }

        /// <summary>Creates a new immutable set that contains all elements that are present in either the current set or in the specified collection.</summary>
        /// <param name="other">The collection to add elements from.</param>
        /// <returns>A new immutable set with the items added; or the original set if all the items were already in the set.</returns>
        IImmutableSet<T> IImmutableSet<T>.Union(IEnumerable<T> other)
        {
            return Union(other);
        }

        /// <summary>Creates an immutable set that contains elements that exist in both this set and the specified set.</summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>A new immutable set that contains any elements that exist in both sets.</returns>
        IImmutableSet<T> IImmutableSet<T>.Intersect(IEnumerable<T> other)
        {
            return Intersect(other);
        }

        /// <summary>Removes the elements in the specified collection from the current set.</summary>
        /// <param name="other">The collection of items to remove from this set.</param>
        /// <returns>A new set with the items removed; or the original set if none of the items were in the set.</returns>
        IImmutableSet<T> IImmutableSet<T>.Except(IEnumerable<T> other)
        {
            return Except(other);
        }

        /// <summary>Creates an immutable set that contains only elements that are present either in the current set or in the specified collection, but not both.</summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>A new set that contains the elements that are present only in the current set or in the specified collection, but not both.</returns>
        IImmutableSet<T> IImmutableSet<T>.SymmetricExcept(IEnumerable<T> other)
        {
            return SymmetricExcept(other);
        }

        /// <summary>Determines whether this immutable hash set contains the specified element.</summary>
        /// <param name="item">The object to locate in the immutable hash set.</param>
        /// <returns>
        ///   <see langword="true" /> if <paramref name="item" /> is found in the <see cref="T:System.Collections.Immutable.ImmutableHashSet`1" />; otherwise, <see langword="false" />.</returns>
        public bool Contains(T item)
        {
            return Contains(item, Origin);
        }

        /// <summary>Gets an instance of the immutable hash set that uses the specified equality comparer for its search methods.</summary>
        /// <param name="equalityComparer">The equality comparer to use.</param>
        /// <returns>An instance of this immutable hash set that uses the given comparer.</returns>
        public ImmutableHashSet<T> WithComparer(IEqualityComparer<T>? equalityComparer)
        {
            if (equalityComparer == null)
            {
                equalityComparer = EqualityComparer<T>.Default;
            }
            if (equalityComparer == _equalityComparer)
            {
                return this;
            }
            ImmutableHashSet<T> immutableHashSet = new ImmutableHashSet<T>(equalityComparer);
            return immutableHashSet.Union(this, avoidWithComparer: true);
        }

        /// <summary>Adds an element to the current set and returns a value that indicates whether the element was successfully added.</summary>
        /// <param name="item">The element to add to the collection.</param>
        /// <returns>
        ///   <see langword="true" /> if the element is added to the set; <see langword="false" /> if the element is already in the set.</returns>
        bool ISet<T>.Add(T item)
        {
            throw new NotSupportedException();
        }

        /// <summary>Removes all elements in the specified collection from the current set.</summary>
        /// <param name="other">The collection of items to remove.</param>
        void ISet<T>.ExceptWith(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }

        /// <summary>Modifies the current set so that it contains only elements that are also in a specified collection.</summary>
        /// <param name="other">The collection to compare to the current collection.</param>
        void ISet<T>.IntersectWith(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }

        /// <summary>Modifies the current set so that it contains only elements that are present either in the current set or in the specified collection, but not both.</summary>
        /// <param name="other">The collection to compare to the current set.</param>
        void ISet<T>.SymmetricExceptWith(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }

        /// <summary>Modifies the current set so that it contains all elements that are present in either the current set or in the specified collection.</summary>
        /// <param name="other">The collection to compare to the current set.</param>
        void ISet<T>.UnionWith(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }

        /// <summary>Copies the elements of the set to an array, starting at a particular index.</summary>
        /// <param name="array">The one-dimensional array that is the destination of the elements copied from the set. The array must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            Requires.NotNull(array, "array");
            Requires.Range(arrayIndex >= 0, "arrayIndex");
            Requires.Range(array.Length >= arrayIndex + Count, "arrayIndex");
            using Enumerator enumerator = GetEnumerator();
            while (enumerator.MoveNext())
            {
                T current = enumerator.Current;
                array[arrayIndex++] = current;
            }
        }

        /// <summary>Adds an item to the set.</summary>
        /// <param name="item">The object to add to the set.</param>
        /// <exception cref="T:System.NotSupportedException">The set is read-only.</exception>
        void ICollection<T>.Add(T item)
        {
            throw new NotSupportedException();
        }

        /// <summary>Removes all items from this set.</summary>
        /// <exception cref="T:System.NotSupportedException">The set is read-only.</exception>
        void ICollection<T>.Clear()
        {
            throw new NotSupportedException();
        }

        /// <summary>Removes the first occurrence of a specific object from the set.</summary>
        /// <param name="item">The object to remove from the set.</param>
        /// <returns>
        ///   <see langword="true" /> if the element is successfully removed; otherwise, <see langword="false" />.</returns>
        bool ICollection<T>.Remove(T item)
        {
            throw new NotSupportedException();
        }

        /// <summary>Copies the elements of the set to an array, starting at a particular index.</summary>
        /// <param name="array">The one-dimensional array that is the destination of the elements copied from the set. The array must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        void ICollection.CopyTo(Array array, int arrayIndex)
        {
            Requires.NotNull(array, "array");
            Requires.Range(arrayIndex >= 0, "arrayIndex");
            Requires.Range(array.Length >= arrayIndex + Count, "arrayIndex");
            using Enumerator enumerator = GetEnumerator();
            while (enumerator.MoveNext())
            {
                T current = enumerator.Current;
                array.SetValue(current, arrayIndex++);
            }
        }

        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public Enumerator GetEnumerator()
        {
            return new Enumerator(_root);
        }

        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator that iterates through the collection.</returns>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            if (!IsEmpty)
            {
                return GetEnumerator();
            }
            return Enumerable.Empty<T>().GetEnumerator();
        }

        /// <summary>Returns an enumerator that iterates through a set.</summary>
        /// <returns>An enumerator that can be used to iterate through the set.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private static bool IsSupersetOf(IEnumerable<T> other, MutationInput origin)
        {
            Requires.NotNull(other, "other");
            foreach (T item in other.GetEnumerableDisposable<T, Enumerator>())
            {
                if (!Contains(item, origin))
                {
                    return false;
                }
            }
            return true;
        }

        private static MutationResult Add(T item, MutationInput origin)
        {
            int num = ((item != null) ? origin.EqualityComparer.GetHashCode(item) : 0);
            OperationResult result;
            HashBucket newBucket = origin.Root.GetValueOrDefault(num).Add(item, origin.EqualityComparer, out result);
            if (result == OperationResult.NoChangeRequired)
            {
                return new MutationResult(origin.Root, 0);
            }
            SortedInt32KeyNode<HashBucket> root = UpdateRoot(origin.Root, num, origin.HashBucketEqualityComparer, newBucket);
            return new MutationResult(root, 1);
        }

        private static MutationResult Remove(T item, MutationInput origin)
        {
            OperationResult result = OperationResult.NoChangeRequired;
            int num = ((item != null) ? origin.EqualityComparer.GetHashCode(item) : 0);
            SortedInt32KeyNode<HashBucket> root = origin.Root;
            if (origin.Root.TryGetValue(num, out var value))
            {
                HashBucket newBucket = value.Remove(item, origin.EqualityComparer, out result);
                if (result == OperationResult.NoChangeRequired)
                {
                    return new MutationResult(origin.Root, 0);
                }
                root = UpdateRoot(origin.Root, num, origin.HashBucketEqualityComparer, newBucket);
            }
            return new MutationResult(root, (result == OperationResult.SizeChanged) ? (-1) : 0);
        }

        private static bool Contains(T item, MutationInput origin)
        {
            int key = ((item != null) ? origin.EqualityComparer.GetHashCode(item) : 0);
            if (origin.Root.TryGetValue(key, out var value))
            {
                return value.Contains(item, origin.EqualityComparer);
            }
            return false;
        }

        private static MutationResult Union(IEnumerable<T> other, MutationInput origin)
        {
            Requires.NotNull(other, "other");
            int num = 0;
            SortedInt32KeyNode<HashBucket> sortedInt32KeyNode = origin.Root;
            foreach (T item in other.GetEnumerableDisposable<T, Enumerator>())
            {
                int num2 = ((item != null) ? origin.EqualityComparer.GetHashCode(item) : 0);
                OperationResult result;
                HashBucket newBucket = sortedInt32KeyNode.GetValueOrDefault(num2).Add(item, origin.EqualityComparer, out result);
                if (result == OperationResult.SizeChanged)
                {
                    sortedInt32KeyNode = UpdateRoot(sortedInt32KeyNode, num2, origin.HashBucketEqualityComparer, newBucket);
                    num++;
                }
            }
            return new MutationResult(sortedInt32KeyNode, num);
        }

        private static bool Overlaps(IEnumerable<T> other, MutationInput origin)
        {
            Requires.NotNull(other, "other");
            if (origin.Root.IsEmpty)
            {
                return false;
            }
            foreach (T item in other.GetEnumerableDisposable<T, Enumerator>())
            {
                if (Contains(item, origin))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool SetEquals(IEnumerable<T> other, MutationInput origin)
        {
            Requires.NotNull(other, "other");
            HashSet<T> hashSet = new HashSet<T>(other, origin.EqualityComparer);
            if (origin.Count != hashSet.Count)
            {
                return false;
            }
            foreach (T item in hashSet)
            {
                if (!Contains(item, origin))
                {
                    return false;
                }
            }
            return true;
        }

        private static SortedInt32KeyNode<HashBucket> UpdateRoot(SortedInt32KeyNode<HashBucket> root, int hashCode, IEqualityComparer<HashBucket> hashBucketEqualityComparer, HashBucket newBucket)
        {
            bool mutated;
            if (newBucket.IsEmpty)
            {
                return root.Remove(hashCode, out mutated);
            }
            bool mutated2;
            return root.SetItem(hashCode, newBucket, hashBucketEqualityComparer, out mutated, out mutated2);
        }

        private static MutationResult Intersect(IEnumerable<T> other, MutationInput origin)
        {
            Requires.NotNull(other, "other");
            SortedInt32KeyNode<HashBucket> root = SortedInt32KeyNode<HashBucket>.EmptyNode;
            int num = 0;
            foreach (T item in other.GetEnumerableDisposable<T, Enumerator>())
            {
                if (Contains(item, origin))
                {
                    MutationResult mutationResult = Add(item, new MutationInput(root, origin.EqualityComparer, origin.HashBucketEqualityComparer, num));
                    root = mutationResult.Root;
                    num += mutationResult.Count;
                }
            }
            return new MutationResult(root, num, CountType.FinalValue);
        }

        private static MutationResult Except(IEnumerable<T> other, IEqualityComparer<T> equalityComparer, IEqualityComparer<HashBucket> hashBucketEqualityComparer, SortedInt32KeyNode<HashBucket> root)
        {
            Requires.NotNull(other, "other");
            Requires.NotNull(equalityComparer, "equalityComparer");
            Requires.NotNull(root, "root");
            int num = 0;
            SortedInt32KeyNode<HashBucket> sortedInt32KeyNode = root;
            foreach (T item in other.GetEnumerableDisposable<T, Enumerator>())
            {
                int num2 = ((item != null) ? equalityComparer.GetHashCode(item) : 0);
                if (sortedInt32KeyNode.TryGetValue(num2, out var value))
                {
                    OperationResult result;
                    HashBucket newBucket = value.Remove(item, equalityComparer, out result);
                    if (result == OperationResult.SizeChanged)
                    {
                        num--;
                        sortedInt32KeyNode = UpdateRoot(sortedInt32KeyNode, num2, hashBucketEqualityComparer, newBucket);
                    }
                }
            }
            return new MutationResult(sortedInt32KeyNode, num);
        }

        private static MutationResult SymmetricExcept(IEnumerable<T> other, MutationInput origin)
        {
            Requires.NotNull(other, "other");
            ImmutableHashSet<T> immutableHashSet = ImmutableHashSet.CreateRange(origin.EqualityComparer, other);
            int num = 0;
            SortedInt32KeyNode<HashBucket> root = SortedInt32KeyNode<HashBucket>.EmptyNode;
            foreach (T item in new NodeEnumerable(origin.Root))
            {
                if (!immutableHashSet.Contains(item))
                {
                    MutationResult mutationResult = Add(item, new MutationInput(root, origin.EqualityComparer, origin.HashBucketEqualityComparer, num));
                    root = mutationResult.Root;
                    num += mutationResult.Count;
                }
            }
            foreach (T item2 in immutableHashSet)
            {
                if (!Contains(item2, origin))
                {
                    MutationResult mutationResult2 = Add(item2, new MutationInput(root, origin.EqualityComparer, origin.HashBucketEqualityComparer, num));
                    root = mutationResult2.Root;
                    num += mutationResult2.Count;
                }
            }
            return new MutationResult(root, num, CountType.FinalValue);
        }

        private static bool IsProperSubsetOf(IEnumerable<T> other, MutationInput origin)
        {
            Requires.NotNull(other, "other");
            if (origin.Root.IsEmpty)
            {
                return other.Any();
            }
            HashSet<T> hashSet = new HashSet<T>(other, origin.EqualityComparer);
            if (origin.Count >= hashSet.Count)
            {
                return false;
            }
            int num = 0;
            bool flag = false;
            foreach (T item in hashSet)
            {
                if (Contains(item, origin))
                {
                    num++;
                }
                else
                {
                    flag = true;
                }
                if (num == origin.Count && flag)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsProperSupersetOf(IEnumerable<T> other, MutationInput origin)
        {
            Requires.NotNull(other, "other");
            if (origin.Root.IsEmpty)
            {
                return false;
            }
            int num = 0;
            foreach (T item in other.GetEnumerableDisposable<T, Enumerator>())
            {
                num++;
                if (!Contains(item, origin))
                {
                    return false;
                }
            }
            return origin.Count > num;
        }

        private static bool IsSubsetOf(IEnumerable<T> other, MutationInput origin)
        {
            Requires.NotNull(other, "other");
            if (origin.Root.IsEmpty)
            {
                return true;
            }
            HashSet<T> hashSet = new HashSet<T>(other, origin.EqualityComparer);
            int num = 0;
            foreach (T item in hashSet)
            {
                if (Contains(item, origin))
                {
                    num++;
                }
            }
            return num == origin.Count;
        }

        private static ImmutableHashSet<T> Wrap(SortedInt32KeyNode<HashBucket> root, IEqualityComparer<T> equalityComparer, int count)
        {
            Requires.NotNull(root, "root");
            Requires.NotNull(equalityComparer, "equalityComparer");
            Requires.Range(count >= 0, "count");
            return new ImmutableHashSet<T>(root, equalityComparer, count);
        }

        private static IEqualityComparer<HashBucket> GetHashBucketEqualityComparer(IEqualityComparer<T> valueComparer)
        {
            if (!ImmutableExtensions.IsValueType<T>())
            {
                return HashBucketByRefEqualityComparer.DefaultInstance;
            }
            if (valueComparer == EqualityComparer<T>.Default)
            {
                return HashBucketByValueEqualityComparer.DefaultInstance;
            }
            return new HashBucketByValueEqualityComparer(valueComparer);
        }

        private ImmutableHashSet<T> Wrap(SortedInt32KeyNode<HashBucket> root, int adjustedCountIfDifferentRoot)
        {
            if (root == _root)
            {
                return this;
            }
            return new ImmutableHashSet<T>(root, _equalityComparer, adjustedCountIfDifferentRoot);
        }

        private ImmutableHashSet<T> Union(IEnumerable<T> items, bool avoidWithComparer)
        {
            Requires.NotNull(items, "items");
            if (IsEmpty && !avoidWithComparer && items is ImmutableHashSet<T> immutableHashSet)
            {
                return immutableHashSet.WithComparer(KeyComparer);
            }
            return Union(items, Origin).Finalize(this);
        }
    }
    /// <summary>Provides a set of initialization methods for instances of the <see cref="System.Collections.Immutable.ImmutableHashSet{T}" /> class. </summary>
    public static class ImmutableHashSet
    {
        /// <summary>Creates an empty immutable hash set.</summary>
        /// <typeparam name="T">The type of items to be stored in the immutable hash set.</typeparam>
        /// <returns>An empty immutable hash set.</returns>
        public static ImmutableHashSet<T> Create<T>()
        {
            return ImmutableHashSet<T>.Empty;
        }

        /// <summary>Creates an empty immutable hash set that uses the specified equality comparer.</summary>
        /// <param name="equalityComparer">The object to use for comparing objects in the set for equality.</param>
        /// <typeparam name="T">The type of items in the immutable hash set.</typeparam>
        /// <returns>An empty immutable hash set.</returns>
        public static ImmutableHashSet<T> Create<T>(IEqualityComparer<T>? equalityComparer)
        {
            return ImmutableHashSet<T>.Empty.WithComparer(equalityComparer);
        }

        /// <summary>Creates a new immutable hash set that contains the specified item.</summary>
        /// <param name="item">The item to prepopulate the hash set with.</param>
        /// <typeparam name="T">The type of items in the immutable hash set.</typeparam>
        /// <returns>A new immutable hash set that contains the specified item.</returns>
        public static ImmutableHashSet<T> Create<T>(T item)
        {
            return ImmutableHashSet<T>.Empty.Add(item);
        }

        /// <summary>Creates a new immutable hash set that contains the specified item and uses the specified equality comparer for the set type.</summary>
        /// <param name="equalityComparer">The object to use for comparing objects in the set for equality.</param>
        /// <param name="item">The item to prepopulate the hash set with.</param>
        /// <typeparam name="T">The type of items in the immutable hash set.</typeparam>
        /// <returns>A new immutable hash set that contains the specified item.</returns>
        public static ImmutableHashSet<T> Create<T>(IEqualityComparer<T>? equalityComparer, T item)
        {
            return ImmutableHashSet<T>.Empty.WithComparer(equalityComparer).Add(item);
        }

        /// <summary>Creates a new immutable hash set prefilled with the specified items.</summary>
        /// <param name="items">The items to add to the hash set.</param>
        /// <typeparam name="T">The type of items stored by the collection.</typeparam>
        /// <returns>The new immutable hash set that contains the specified items.</returns>
        public static ImmutableHashSet<T> CreateRange<T>(IEnumerable<T> items)
        {
            return ImmutableHashSet<T>.Empty.Union(items);
        }

        /// <summary>Creates a new immutable hash set that contains the specified items and uses the specified equality comparer for the set type.</summary>
        /// <param name="equalityComparer">The object to use for comparing objects in the set for equality.</param>
        /// <param name="items">The items add to the collection before immutability is applied.</param>
        /// <typeparam name="T">The type of items stored in the collection.</typeparam>
        /// <returns>The new immutable hash set.</returns>
        public static ImmutableHashSet<T> CreateRange<T>(IEqualityComparer<T>? equalityComparer, IEnumerable<T> items)
        {
            return ImmutableHashSet<T>.Empty.WithComparer(equalityComparer).Union(items);
        }

        /// <summary>Creates a new immutable hash set that contains the specified array of items.</summary>
        /// <param name="items">An array that contains the items to prepopulate the hash set with.</param>
        /// <typeparam name="T">The type of items in the immutable hash set.</typeparam>
        /// <returns>A new immutable hash set that contains the specified items.</returns>
        public static ImmutableHashSet<T> Create<T>(params T[] items)
        {
            return ImmutableHashSet<T>.Empty.Union(items);
        }

        /// <summary>Creates a new immutable hash set that contains the items in the specified collection and uses the specified equality comparer for the set type.</summary>
        /// <param name="equalityComparer">The object to use for comparing objects in the set for equality.</param>
        /// <param name="items">An array that contains the items to prepopulate the hash set with.</param>
        /// <typeparam name="T">The type of items stored in the immutable hash set.</typeparam>
        /// <returns>A new immutable hash set that contains the specified items.</returns>
        public static ImmutableHashSet<T> Create<T>(IEqualityComparer<T>? equalityComparer, params T[] items)
        {
            return ImmutableHashSet<T>.Empty.WithComparer(equalityComparer).Union(items);
        }

        /// <summary>Creates a new immutable hash set builder.</summary>
        /// <typeparam name="T">The type of items stored by the collection.</typeparam>
        /// <returns>The immutable hash set builder.</returns>
        public static ImmutableHashSet<T>.Builder CreateBuilder<T>()
        {
            return Create<T>().ToBuilder();
        }

        /// <summary>Creates a new immutable hash set builder.</summary>
        /// <param name="equalityComparer">The object to use for comparing objects in the set for equality.</param>
        /// <typeparam name="T">The type of items stored by the collection.</typeparam>
        /// <returns>The new immutable hash set builder.</returns>
        public static ImmutableHashSet<T>.Builder CreateBuilder<T>(IEqualityComparer<T>? equalityComparer)
        {
            return Create(equalityComparer).ToBuilder();
        }

        /// <summary>Enumerates a sequence, produces an immutable hash set of its contents, and uses the specified equality comparer for the set type.</summary>
        /// <param name="source">The sequence to enumerate.</param>
        /// <param name="equalityComparer">The object to use for comparing objects in the set for equality.</param>
        /// <typeparam name="TSource">The type of the elements in the sequence.</typeparam>
        /// <returns>An immutable hash set that contains the items in the specified sequence and uses the specified equality comparer.</returns>
        public static ImmutableHashSet<TSource> ToImmutableHashSet<TSource>(this IEnumerable<TSource> source, IEqualityComparer<TSource>? equalityComparer)
        {
            if (source is ImmutableHashSet<TSource> immutableHashSet)
            {
                return immutableHashSet.WithComparer(equalityComparer);
            }
            return ImmutableHashSet<TSource>.Empty.WithComparer(equalityComparer).Union(source);
        }

        /// <summary>Creates an immutable hash set from the current contents of the builder's set.</summary>
        /// <param name="builder">The builder to create the immutable hash set from.</param>
        /// <typeparam name="TSource">The type of the elements in the hash set.</typeparam>
        /// <returns>An immutable hash set that contains the current contents in the builder's set.</returns>
        public static ImmutableHashSet<TSource> ToImmutableHashSet<TSource>(this ImmutableHashSet<TSource>.Builder builder)
        {
            Requires.NotNull(builder, "builder");
            return builder.ToImmutable();
        }

        /// <summary>Enumerates a sequence and produces an immutable hash set of its contents.</summary>
        /// <param name="source">The sequence to enumerate.</param>
        /// <typeparam name="TSource">The type of the elements in the sequence.</typeparam>
        /// <returns>An immutable hash set that contains the items in the specified sequence.</returns>
        public static ImmutableHashSet<TSource> ToImmutableHashSet<TSource>(this IEnumerable<TSource> source)
        {
            return source.ToImmutableHashSet(null);
        }
    }


    /// <summary>Contains interlocked exchange mechanisms for immutable collections. </summary>
    public static class ImmutableInterlocked
    {
        /// <summary>Mutates a value in-place with optimistic locking transaction semantics             via a specified transformation function.             The transformation is retried as many times as necessary to win the optimistic locking race.</summary>
        /// <param name="location">The variable or field to be changed, which may be accessed by multiple threads.</param>
        /// <param name="transformer">A function that mutates the value. This function should be side-effect free,              as it may run multiple times when races occur with other threads.</param>
        /// <typeparam name="T">The type of data.</typeparam>
        /// <returns>
        ///   <see langword="true" /> if the location's value is changed by applying the result of the <paramref name="transformer" /> function; <see langword="false" /> if the location's value remained the same because the last invocation of <paramref name="transformer" /> returned the existing value.</returns>
        public static bool Update<T>(ref T location, Func<T, T> transformer) where T : class?
        {
            Requires.NotNull(transformer, "transformer");
            T val = Volatile.Read(ref location);
            bool flag;
            do
            {
                T val2 = transformer(val);
                if (val == val2)
                {
                    return false;
                }
                T val3 = Interlocked.CompareExchange(ref location, val2, val);
                flag = val == val3;
                val = val3;
            }
            while (!flag);
            return true;
        }

        /// <summary>Mutates a value in-place with optimistic locking transaction semantics             via a specified transformation function.             The transformation is retried as many times as necessary to win the optimistic locking race.</summary>
        /// <param name="location">The variable or field to be changed, which may be accessed by multiple threads.</param>
        /// <param name="transformer">A function that mutates the value. This function should be side-effect free,              as it may run multiple times when races occur with other threads.</param>
        /// <param name="transformerArgument">The argument to pass to <paramref name="transformer" />.</param>
        /// <typeparam name="T">The type of data.</typeparam>
        /// <typeparam name="TArg">The type of argument passed to the <paramref name="transformer" />.</typeparam>
        /// <returns>
        ///   <see langword="true" /> if the location's value is changed by applying the result of the <paramref name="transformer" /> function; <see langword="false" /> if the location's value remained the same because the last invocation of <paramref name="transformer" /> returned the existing value.</returns>
        public static bool Update<T, TArg>(ref T location, Func<T, TArg, T> transformer, TArg transformerArgument) where T : class?
        {
            Requires.NotNull(transformer, "transformer");
            T val = Volatile.Read(ref location);
            bool flag;
            do
            {
                T val2 = transformer(val, transformerArgument);
                if (val == val2)
                {
                    return false;
                }
                T val3 = Interlocked.CompareExchange(ref location, val2, val);
                flag = val == val3;
                val = val3;
            }
            while (!flag);
            return true;
        }

        /// <summary>Mutates an immutable array in-place with optimistic locking transaction semantics via a specified transformation function.
        ///  The transformation is retried as many times as necessary to win the optimistic locking race.</summary>
        /// <param name="location">The immutable array to be changed.</param>
        /// <param name="transformer">A function that produces the new array from the old. This function should be side-effect free, as it may run multiple times when races occur with other threads.</param>
        /// <typeparam name="T">The type of data in the immutable array.</typeparam>
        /// <returns>
        ///   <see langword="true" /> if the location's value is changed by applying the result of the <paramref name="transformer" /> function; <see langword="false" /> if the location's value remained the same because the last invocation of <paramref name="transformer" /> returned the existing value.</returns>
        public static bool Update<T>(ref ImmutableArray<T> location, Func<ImmutableArray<T>, ImmutableArray<T>> transformer)
        {
            Requires.NotNull(transformer, "transformer");
            T[] array = Volatile.Read(ref Unsafe.AsRef(in location.array));
            bool flag;
            do
            {
                ImmutableArray<T> immutableArray = transformer(new ImmutableArray<T>(array));
                if (array == immutableArray.array)
                {
                    return false;
                }
                T[] array2 = Interlocked.CompareExchange(ref Unsafe.AsRef(in location.array), immutableArray.array, array);
                flag = array == array2;
                array = array2;
            }
            while (!flag);
            return true;
        }

        /// <summary>Mutates an immutable array in-place with optimistic locking transaction semantics via a specified transformation function.
        ///  The transformation is retried as many times as necessary to win the optimistic locking race.</summary>
        /// <param name="location">The immutable array to be changed.</param>
        /// <param name="transformer">A function that produces the new array from the old. This function should be side-effect free, as it may run multiple times when races occur with other threads.</param>
        /// <param name="transformerArgument">The argument to pass to <paramref name="transformer" />.</param>
        /// <typeparam name="T">The type of data in the immutable array.</typeparam>
        /// <typeparam name="TArg">The type of argument passed to the <paramref name="transformer" />.</typeparam>
        /// <returns>
        ///   <see langword="true" /> if the location's value is changed by applying the result of the <paramref name="transformer" /> function; <see langword="false" /> if the location's value remained the same because the last invocation of <paramref name="transformer" /> returned the existing value.</returns>
        public static bool Update<T, TArg>(ref ImmutableArray<T> location, Func<ImmutableArray<T>, TArg, ImmutableArray<T>> transformer, TArg transformerArgument)
        {
            Requires.NotNull(transformer, "transformer");
            T[] array = Volatile.Read(ref Unsafe.AsRef(in location.array));
            bool flag;
            do
            {
                ImmutableArray<T> immutableArray = transformer(new ImmutableArray<T>(array), transformerArgument);
                if (array == immutableArray.array)
                {
                    return false;
                }
                T[] array2 = Interlocked.CompareExchange(ref Unsafe.AsRef(in location.array), immutableArray.array, array);
                flag = array == array2;
                array = array2;
            }
            while (!flag);
            return true;
        }

        /// <summary>Sets an array to the specified array and returns a reference to the original array, as an atomic operation.</summary>
        /// <param name="location">The array to set to the specified value.</param>
        /// <param name="value">The value to which the <paramref name="location" /> parameter is set.</param>
        /// <typeparam name="T">The type of element stored by the array.</typeparam>
        /// <returns>The original value of <paramref name="location" />.</returns>
        public static ImmutableArray<T> InterlockedExchange<T>(ref ImmutableArray<T> location, ImmutableArray<T> value)
        {
            return new ImmutableArray<T>(Interlocked.Exchange(ref Unsafe.AsRef(in location.array), value.array));
        }

        /// <summary>Compares two immutable arrays for equality and, if they are equal, replaces one of the arrays.</summary>
        /// <param name="location">The destination, whose value is compared with <paramref name="comparand" /> and possibly replaced.</param>
        /// <param name="value">The value that replaces the destination value if the comparison results in equality.</param>
        /// <param name="comparand">The value that is compared to the value at <paramref name="location" />.</param>
        /// <typeparam name="T">The type of element stored by the array.</typeparam>
        /// <returns>The original value in <paramref name="location" />.</returns>
        public static ImmutableArray<T> InterlockedCompareExchange<T>(ref ImmutableArray<T> location, ImmutableArray<T> value, ImmutableArray<T> comparand)
        {
            return new ImmutableArray<T>(Interlocked.CompareExchange(ref Unsafe.AsRef(in location.array), value.array, comparand.array));
        }

        /// <summary>Sets an array to the specified array if the array has not been initialized.</summary>
        /// <param name="location">The array to set to the specified value.</param>
        /// <param name="value">The value to which the <paramref name="location" /> parameter is set, if it's not initialized.</param>
        /// <typeparam name="T">The type of element stored by the array.</typeparam>
        /// <returns>
        ///   <see langword="true" /> if the array was assigned the specified value;  otherwise, <see langword="false" />.</returns>
        public static bool InterlockedInitialize<T>(ref ImmutableArray<T> location, ImmutableArray<T> value)
        {
            return InterlockedCompareExchange(ref location, value, default(ImmutableArray<T>)).IsDefault;
        }

        /// <summary>Gets the value for the specified key from the dictionary, or if the key was not found, adds a new value to the dictionary.</summary>
        /// <param name="location">The variable or field to update if the specified is not in the dictionary.</param>
        /// <param name="key">The key for the value to retrieve or add.</param>
        /// <param name="valueFactory">The function to execute to obtain the value to insert into the dictionary if the key is not found.</param>
        /// <param name="factoryArgument">The argument to pass to the value factory.</param>
        /// <typeparam name="TKey">The type of the keys contained in the collection.</typeparam>
        /// <typeparam name="TValue">The type of the values contained in the collection.</typeparam>
        /// <typeparam name="TArg">The type of the argument supplied to the value factory.</typeparam>
        /// <returns>The value at the specified key or <paramref name="valueFactory" /> if the key was not present.</returns>
        public static TValue GetOrAdd<TKey, TValue, TArg>(ref ImmutableDictionary<TKey, TValue> location, TKey key, Func<TKey, TArg, TValue> valueFactory, TArg factoryArgument) where TKey : notnull
        {
            Requires.NotNull(valueFactory, "valueFactory");
            ImmutableDictionary<TKey, TValue> immutableDictionary = Volatile.Read(ref location);
            Requires.NotNull(immutableDictionary, "location");
            if (immutableDictionary.TryGetValue(key, out var value))
            {
                return value;
            }
            value = valueFactory(key, factoryArgument);
            return GetOrAdd(ref location, key, value);
        }

        /// <summary>Gets the value for the specified key from the dictionary, or if the key was not found, adds a new value to the dictionary.</summary>
        /// <param name="location">The variable or field to atomically update if the specified  is not in the dictionary.</param>
        /// <param name="key">The key for the value to retrieve or add.</param>
        /// <param name="valueFactory">The function to execute to obtain the value to insert into the dictionary if the key is not found. This delegate will not be invoked more than once.</param>
        /// <typeparam name="TKey">The type of the keys contained in the collection.</typeparam>
        /// <typeparam name="TValue">The type of the values contained in the collection.</typeparam>
        /// <returns>The value at the specified key or <paramref name="valueFactory" /> if the key was not present.</returns>
        public static TValue GetOrAdd<TKey, TValue>(ref ImmutableDictionary<TKey, TValue> location, TKey key, Func<TKey, TValue> valueFactory) where TKey : notnull
        {
            Requires.NotNull(valueFactory, "valueFactory");
            ImmutableDictionary<TKey, TValue> immutableDictionary = Volatile.Read(ref location);
            Requires.NotNull(immutableDictionary, "location");
            if (immutableDictionary.TryGetValue(key, out var value))
            {
                return value;
            }
            value = valueFactory(key);
            return GetOrAdd(ref location, key, value);
        }

        /// <summary>Gets the value for the specified key from the dictionary, or if the key was not found, adds a new value to the dictionary.</summary>
        /// <param name="location">The variable or field to atomically update if the specified key is not in the dictionary.</param>
        /// <param name="key">The key for the value to get or add.</param>
        /// <param name="value">The value to add to the dictionary the key is not found.</param>
        /// <typeparam name="TKey">The type of the keys contained in the collection.</typeparam>
        /// <typeparam name="TValue">The type of the values contained in the collection.</typeparam>
        /// <returns>The value at the specified key or <paramref name="value" /> if the key was not present.</returns>
        public static TValue GetOrAdd<TKey, TValue>(ref ImmutableDictionary<TKey, TValue> location, TKey key, TValue value) where TKey : notnull
        {
            ImmutableDictionary<TKey, TValue> immutableDictionary = Volatile.Read(ref location);
            bool flag;
            do
            {
                Requires.NotNull(immutableDictionary, "location");
                if (immutableDictionary.TryGetValue(key, out var value2))
                {
                    return value2;
                }
                ImmutableDictionary<TKey, TValue> value3 = immutableDictionary.Add(key, value);
                ImmutableDictionary<TKey, TValue> immutableDictionary2 = Interlocked.CompareExchange(ref location, value3, immutableDictionary);
                flag = immutableDictionary == immutableDictionary2;
                immutableDictionary = immutableDictionary2;
            }
            while (!flag);
            return value;
        }

        /// <summary>Obtains the value from a dictionary after having added it or updated an existing entry.</summary>
        /// <param name="location">The variable or field to atomically update if the specified  is not in the dictionary.</param>
        /// <param name="key">The key for the value to add or update.</param>
        /// <param name="addValueFactory">The function that receives the key and returns a new value to add to the dictionary when no value previously exists.</param>
        /// <param name="updateValueFactory">The function that receives the key and prior value and returns the new value with which to update the dictionary.</param>
        /// <typeparam name="TKey">The type of key stored by the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of value stored by the dictionary.</typeparam>
        /// <returns>The added or updated value.</returns>
        public static TValue AddOrUpdate<TKey, TValue>(ref ImmutableDictionary<TKey, TValue> location, TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory) where TKey : notnull
        {
            Requires.NotNull(addValueFactory, "addValueFactory");
            Requires.NotNull(updateValueFactory, "updateValueFactory");
            ImmutableDictionary<TKey, TValue> immutableDictionary = Volatile.Read(ref location);
            TValue val;
            bool flag;
            do
            {
                Requires.NotNull(immutableDictionary, "location");
                val = ((!immutableDictionary.TryGetValue(key, out var value)) ? addValueFactory(key) : updateValueFactory(key, value));
                ImmutableDictionary<TKey, TValue> immutableDictionary2 = immutableDictionary.SetItem(key, val);
                if (immutableDictionary == immutableDictionary2)
                {
                    return value;
                }
                ImmutableDictionary<TKey, TValue> immutableDictionary3 = Interlocked.CompareExchange(ref location, immutableDictionary2, immutableDictionary);
                flag = immutableDictionary == immutableDictionary3;
                immutableDictionary = immutableDictionary3;
            }
            while (!flag);
            return val;
        }

        /// <summary>Obtains the value from a dictionary after having added it or updated an existing entry.</summary>
        /// <param name="location">The variable or field to atomically update if the specified  is not in the dictionary.</param>
        /// <param name="key">The key for the value to add or update.</param>
        /// <param name="addValue">The value to use if no previous value exists.</param>
        /// <param name="updateValueFactory">The function that receives the key and prior value and returns the new value with which to update the dictionary.</param>
        /// <typeparam name="TKey">The type of key stored by the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of value stored by the dictionary.</typeparam>
        /// <returns>The added or updated value.</returns>
        public static TValue AddOrUpdate<TKey, TValue>(ref ImmutableDictionary<TKey, TValue> location, TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory) where TKey : notnull
        {
            Requires.NotNull(updateValueFactory, "updateValueFactory");
            ImmutableDictionary<TKey, TValue> immutableDictionary = Volatile.Read(ref location);
            TValue val;
            bool flag;
            do
            {
                Requires.NotNull(immutableDictionary, "location");
                val = (TValue)((!immutableDictionary.TryGetValue(key, out var value)) ? ((object)addValue) : ((object)updateValueFactory(key, value)));
                ImmutableDictionary<TKey, TValue> immutableDictionary2 = immutableDictionary.SetItem(key, val);
                if (immutableDictionary == immutableDictionary2)
                {
                    return value;
                }
                ImmutableDictionary<TKey, TValue> immutableDictionary3 = Interlocked.CompareExchange(ref location, immutableDictionary2, immutableDictionary);
                flag = immutableDictionary == immutableDictionary3;
                immutableDictionary = immutableDictionary3;
            }
            while (!flag);
            return val;
        }

        /// <summary>Adds the specified key and value to the dictionary if the key is not in the dictionary.</summary>
        /// <param name="location">The dictionary to update with the specified key and value.</param>
        /// <param name="key">The key to add, if is not already defined in the dictionary.</param>
        /// <param name="value">The value to add.</param>
        /// <typeparam name="TKey">The type of the keys contained in the collection.</typeparam>
        /// <typeparam name="TValue">The type of the values contained in the collection.</typeparam>
        /// <returns>
        ///   <see langword="true" /> if the key is not in the dictionary; otherwise, <see langword="false" />.</returns>
        public static bool TryAdd<TKey, TValue>(ref ImmutableDictionary<TKey, TValue> location, TKey key, TValue value) where TKey : notnull
        {
            ImmutableDictionary<TKey, TValue> immutableDictionary = Volatile.Read(ref location);
            bool flag;
            do
            {
                Requires.NotNull(immutableDictionary, "location");
                if (immutableDictionary.ContainsKey(key))
                {
                    return false;
                }
                ImmutableDictionary<TKey, TValue> value2 = immutableDictionary.Add(key, value);
                ImmutableDictionary<TKey, TValue> immutableDictionary2 = Interlocked.CompareExchange(ref location, value2, immutableDictionary);
                flag = immutableDictionary == immutableDictionary2;
                immutableDictionary = immutableDictionary2;
            }
            while (!flag);
            return true;
        }

        /// <summary>Sets the specified key to the specified value if the specified key already is set to a specific value.</summary>
        /// <param name="location">The dictionary to update.</param>
        /// <param name="key">The key to update.</param>
        /// <param name="newValue">The new value to set.</param>
        /// <param name="comparisonValue">The current value for <paramref name="key" /> in order for the update to succeed.</param>
        /// <typeparam name="TKey">The type of the keys contained in the collection.</typeparam>
        /// <typeparam name="TValue">The type of the values contained in the collection.</typeparam>
        /// <returns>
        ///   <see langword="true" /> if <paramref name="key" /> and <paramref name="comparisonValue" /> are present in the dictionary and comparison was updated to <paramref name="newValue" />; otherwise, <see langword="false" />.</returns>
        public static bool TryUpdate<TKey, TValue>(ref ImmutableDictionary<TKey, TValue> location, TKey key, TValue newValue, TValue comparisonValue) where TKey : notnull
        {
            EqualityComparer<TValue> @default = EqualityComparer<TValue>.Default;
            ImmutableDictionary<TKey, TValue> immutableDictionary = Volatile.Read(ref location);
            bool flag;
            do
            {
                Requires.NotNull(immutableDictionary, "location");
                if (!immutableDictionary.TryGetValue(key, out var value) || !@default.Equals(value, comparisonValue))
                {
                    return false;
                }
                ImmutableDictionary<TKey, TValue> value2 = immutableDictionary.SetItem(key, newValue);
                ImmutableDictionary<TKey, TValue> immutableDictionary2 = Interlocked.CompareExchange(ref location, value2, immutableDictionary);
                flag = immutableDictionary == immutableDictionary2;
                immutableDictionary = immutableDictionary2;
            }
            while (!flag);
            return true;
        }

        /// <summary>Removes the element with the specified key, if the key exists.</summary>
        /// <param name="location">The dictionary to update.</param>
        /// <param name="key">The key to remove.</param>
        /// <param name="value">Receives the value of the removed item, if the dictionary is not empty.</param>
        /// <typeparam name="TKey">The type of the keys contained in the collection.</typeparam>
        /// <typeparam name="TValue">The type of the values contained in the collection.</typeparam>
        /// <returns>
        ///   <see langword="true" /> if the key was found and removed; otherwise, <see langword="false" />.</returns>
        public static bool TryRemove<TKey, TValue>(ref ImmutableDictionary<TKey, TValue> location, TKey key, [MaybeNullWhen(false)] out TValue value) where TKey : notnull
        {
            ImmutableDictionary<TKey, TValue> immutableDictionary = Volatile.Read(ref location);
            bool flag;
            do
            {
                Requires.NotNull(immutableDictionary, "location");
                if (!immutableDictionary.TryGetValue(key, out value))
                {
                    return false;
                }
                ImmutableDictionary<TKey, TValue> value2 = immutableDictionary.Remove(key);
                ImmutableDictionary<TKey, TValue> immutableDictionary2 = Interlocked.CompareExchange(ref location, value2, immutableDictionary);
                flag = immutableDictionary == immutableDictionary2;
                immutableDictionary = immutableDictionary2;
            }
            while (!flag);
            return true;
        }

        /// <summary>Removes an element from the top of the stack, if there is an element to remove.</summary>
        /// <param name="location">The stack to update.</param>
        /// <param name="value">Receives the value removed from the stack, if the stack is not empty.</param>
        /// <typeparam name="T">The type of items in the stack.</typeparam>
        /// <returns>
        ///   <see langword="true" /> if an element is removed from the stack; otherwise, <see langword="false" />.</returns>
        public static bool TryPop<T>(ref ImmutableStack<T> location, [MaybeNullWhen(false)] out T value)
        {
            ImmutableStack<T> immutableStack = Volatile.Read(ref location);
            bool flag;
            do
            {
                Requires.NotNull(immutableStack, "location");
                if (immutableStack.IsEmpty)
                {
                    value = default(T);
                    return false;
                }
                ImmutableStack<T> value2 = immutableStack.Pop(out value);
                ImmutableStack<T> immutableStack2 = Interlocked.CompareExchange(ref location, value2, immutableStack);
                flag = immutableStack == immutableStack2;
                immutableStack = immutableStack2;
            }
            while (!flag);
            return true;
        }

        /// <summary>Pushes a new element onto the stack.</summary>
        /// <param name="location">The stack to update.</param>
        /// <param name="value">The value to push on the stack.</param>
        /// <typeparam name="T">The type of items in the stack.</typeparam>
        public static void Push<T>(ref ImmutableStack<T> location, T value)
        {
            ImmutableStack<T> immutableStack = Volatile.Read(ref location);
            bool flag;
            do
            {
                Requires.NotNull(immutableStack, "location");
                ImmutableStack<T> value2 = immutableStack.Push(value);
                ImmutableStack<T> immutableStack2 = Interlocked.CompareExchange(ref location, value2, immutableStack);
                flag = immutableStack == immutableStack2;
                immutableStack = immutableStack2;
            }
            while (!flag);
        }

        /// <summary>Atomically removes and returns the specified element at the head of the queue, if the queue is not empty.</summary>
        /// <param name="location">The variable or field to atomically update.</param>
        /// <param name="value">Set to the value from the head of the queue, if the queue not empty.</param>
        /// <typeparam name="T">The type of items in the queue.</typeparam>
        /// <returns>
        ///   <see langword="true" /> if the queue is not empty and the head element is removed; otherwise, <see langword="false" />.</returns>
        public static bool TryDequeue<T>(ref ImmutableQueue<T> location, [MaybeNullWhen(false)] out T value)
        {
            ImmutableQueue<T> immutableQueue = Volatile.Read(ref location);
            bool flag;
            do
            {
                Requires.NotNull(immutableQueue, "location");
                if (immutableQueue.IsEmpty)
                {
                    value = default(T);
                    return false;
                }
                ImmutableQueue<T> value2 = immutableQueue.Dequeue(out value);
                ImmutableQueue<T> immutableQueue2 = Interlocked.CompareExchange(ref location, value2, immutableQueue);
                flag = immutableQueue == immutableQueue2;
                immutableQueue = immutableQueue2;
            }
            while (!flag);
            return true;
        }

        /// <summary>Atomically enqueues an element to the end of a queue.</summary>
        /// <param name="location">The variable or field to atomically update.</param>
        /// <param name="value">The value to enqueue.</param>
        /// <typeparam name="T">The type of items contained in the collection.</typeparam>
        public static void Enqueue<T>(ref ImmutableQueue<T> location, T value)
        {
            ImmutableQueue<T> immutableQueue = Volatile.Read(ref location);
            bool flag;
            do
            {
                Requires.NotNull(immutableQueue, "location");
                ImmutableQueue<T> value2 = immutableQueue.Enqueue(value);
                ImmutableQueue<T> immutableQueue2 = Interlocked.CompareExchange(ref location, value2, immutableQueue);
                flag = immutableQueue == immutableQueue2;
                immutableQueue = immutableQueue2;
            }
            while (!flag);
        }
    }


    /// <summary>Provides a set of initialization methods for instances of the <see cref="System.Collections.Immutable.ImmutableList{T}" /> class. </summary>
    public static class ImmutableList
    {
        /// <summary>Creates an empty immutable list.</summary>
        /// <typeparam name="T">The type of items to be stored in the .</typeparam>
        /// <returns>An empty immutable list.</returns>
        public static ImmutableList<T> Create<T>()
        {
            return ImmutableList<T>.Empty;
        }

        /// <summary>Creates a new immutable list that contains the specified item.</summary>
        /// <param name="item">The item to prepopulate the list with.</param>
        /// <typeparam name="T">The type of items in the .</typeparam>
        /// <returns>A new  that contains the specified item.</returns>
        public static ImmutableList<T> Create<T>(T item)
        {
            return ImmutableList<T>.Empty.Add(item);
        }

        /// <summary>Creates a new immutable list that contains the specified items.</summary>
        /// <param name="items">The items to add to the list.</param>
        /// <typeparam name="T">The type of items in the .</typeparam>
        /// <returns>An immutable list that contains the specified items.</returns>
        public static ImmutableList<T> CreateRange<T>(IEnumerable<T> items)
        {
            return ImmutableList<T>.Empty.AddRange(items);
        }

        /// <summary>Creates a new immutable list that contains the specified array of items.</summary>
        /// <param name="items">An array that contains the items to prepopulate the list with.</param>
        /// <typeparam name="T">The type of items in the .</typeparam>
        /// <returns>A new immutable list that contains the specified items.</returns>
        public static ImmutableList<T> Create<T>(params T[] items)
        {
            return ImmutableList<T>.Empty.AddRange(items);
        }

        /// <summary>Creates a new immutable list builder.</summary>
        /// <typeparam name="T">The type of items stored by the collection.</typeparam>
        /// <returns>The immutable collection builder.</returns>
        public static ImmutableList<T>.Builder CreateBuilder<T>()
        {
            return Create<T>().ToBuilder();
        }

        /// <summary>Enumerates a sequence and produces an immutable list of its contents.</summary>
        /// <param name="source">The sequence to enumerate.</param>
        /// <typeparam name="TSource">The type of the elements in the sequence.</typeparam>
        /// <returns>An immutable list that contains the items in the specified sequence.</returns>
        public static ImmutableList<TSource> ToImmutableList<TSource>(this IEnumerable<TSource> source)
        {
            if (source is ImmutableList<TSource> result)
            {
                return result;
            }
            return ImmutableList<TSource>.Empty.AddRange(source);
        }

        /// <summary>Creates an immutable list from the current contents of the builder's collection.</summary>
        /// <param name="builder">The builder to create the immutable list from.</param>
        /// <typeparam name="TSource">The type of the elements in the list.</typeparam>
        /// <returns>An immutable list that contains the current contents in the builder's collection.</returns>
        public static ImmutableList<TSource> ToImmutableList<TSource>(this ImmutableList<TSource>.Builder builder)
        {
            Requires.NotNull(builder, "builder");
            return builder.ToImmutable();
        }

        /// <summary>Replaces the first equal element in the list with the specified element.</summary>
        /// <param name="list">The list to search.</param>
        /// <param name="oldValue">The element to replace.</param>
        /// <param name="newValue">The element to replace the old element with.</param>
        /// <typeparam name="T">The type of items in the list.</typeparam>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="oldValue" /> does not exist in the list.</exception>
        /// <returns>The new list -- even if the value being replaced is equal to the new value for that position.</returns>
        public static IImmutableList<T> Replace<T>(this IImmutableList<T> list, T oldValue, T newValue)
        {
            Requires.NotNull(list, "list");
            return list.Replace(oldValue, newValue, EqualityComparer<T>.Default);
        }

        /// <summary>Removes the specified value from this list.</summary>
        /// <param name="list">The list to search.</param>
        /// <param name="value">The value to remove.</param>
        /// <typeparam name="T">The type of items in the list.</typeparam>
        /// <returns>A new immutable list with the element removed, or this list if the element is not in this list.</returns>
        public static IImmutableList<T> Remove<T>(this IImmutableList<T> list, T value)
        {
            Requires.NotNull(list, "list");
            return list.Remove(value, EqualityComparer<T>.Default);
        }

        /// <summary>Removes the specified values from this list.</summary>
        /// <param name="list">The list to search.</param>
        /// <param name="items">The items to remove if matches are found in this list.</param>
        /// <typeparam name="T">The type of items in the list.</typeparam>
        /// <returns>A new immutable list with the elements removed.</returns>
        public static IImmutableList<T> RemoveRange<T>(this IImmutableList<T> list, IEnumerable<T> items)
        {
            Requires.NotNull(list, "list");
            return list.RemoveRange(items, EqualityComparer<T>.Default);
        }

        /// <summary>Searches for the specified object and returns the zero-based index of the first occurrence within the list.</summary>
        /// <param name="list">The list to search.</param>
        /// <param name="item">The object to locate in the list. The value can be null for reference types.</param>
        /// <typeparam name="T">The type of items in the list.</typeparam>
        /// <returns>The zero-based index of the first occurrence of item within the range of elements in the list that extends from index to the last element, if found; otherwise, -1.</returns>
        public static int IndexOf<T>(this IImmutableList<T> list, T item)
        {
            Requires.NotNull(list, "list");
            return list.IndexOf(item, 0, list.Count, EqualityComparer<T>.Default);
        }

        /// <summary>Searches for the specified object and returns the zero-based index of the first occurrence within the list.</summary>
        /// <param name="list">The list to search.</param>
        /// <param name="item">The object to locate in the Immutable list. The value can be null for reference types.</param>
        /// <param name="equalityComparer">The equality comparer to use in the search.</param>
        /// <typeparam name="T">The type of items in the list.</typeparam>
        /// <returns>The zero-based index of the first occurrence of item within the range of elements in the immutable list that extends from index to the last element, if found; otherwise, -1.</returns>
        public static int IndexOf<T>(this IImmutableList<T> list, T item, IEqualityComparer<T>? equalityComparer)
        {
            Requires.NotNull(list, "list");
            return list.IndexOf(item, 0, list.Count, equalityComparer);
        }

        /// <summary>Searches for the specified object and returns the zero-based index of the first occurrence within the range of elements in the immutable list that extends from the specified index to the last element.</summary>
        /// <param name="list">The list to search.</param>
        /// <param name="item">The object to locate in the Immutable list. The value can be null for reference types.</param>
        /// <param name="startIndex">The zero-based starting index of the search. 0 (zero) is valid in an empty list.</param>
        /// <typeparam name="T">The type of items in the list.</typeparam>
        /// <returns>The zero-based index of the first occurrence of item within the range of elements in the Immutable list that extends from index to the last element, if found; otherwise, -1.</returns>
        public static int IndexOf<T>(this IImmutableList<T> list, T item, int startIndex)
        {
            Requires.NotNull(list, "list");
            return list.IndexOf(item, startIndex, list.Count - startIndex, EqualityComparer<T>.Default);
        }

        /// <summary>Searches for the specified object and returns the zero-based index of the first occurrence within the range of elements in the immutable list that extends from the specified index to the last element.</summary>
        /// <param name="list">The list to search.</param>
        /// <param name="item">The object to locate in the Immutable list. The value can be null for reference types.</param>
        /// <param name="startIndex">The zero-based starting index of the search. 0 (zero) is valid in an empty list.</param>
        /// <param name="count">The number of elements in the section to search.</param>
        /// <typeparam name="T">The type of items in the list.</typeparam>
        /// <returns>The zero-based index of the first occurrence of item within the range of elements in the Immutable list that extends from index to the last element, if found; otherwise, -1.</returns>
        public static int IndexOf<T>(this IImmutableList<T> list, T item, int startIndex, int count)
        {
            Requires.NotNull(list, "list");
            return list.IndexOf(item, startIndex, count, EqualityComparer<T>.Default);
        }

        /// <summary>Searches for the specified object and returns the zero-based index of the last occurrence within the entire immutable list.</summary>
        /// <param name="list">The list to search.</param>
        /// <param name="item">The object to locate in the Immutable list. The value can be null for reference types.</param>
        /// <typeparam name="T">The type of items in the list.</typeparam>
        /// <returns>The zero-based index of the last occurrence of item within the entire the Immutable list, if found; otherwise, -1.</returns>
        public static int LastIndexOf<T>(this IImmutableList<T> list, T item)
        {
            Requires.NotNull(list, "list");
            if (list.Count == 0)
            {
                return -1;
            }
            return list.LastIndexOf(item, list.Count - 1, list.Count, EqualityComparer<T>.Default);
        }

        /// <summary>Searches for the specified object and returns the zero-based index of the last occurrence within the entire immutable list.</summary>
        /// <param name="list">The list to search.</param>
        /// <param name="item">The object to locate in the Immutable list. The value can be null for reference types.</param>
        /// <param name="equalityComparer">The equality comparer to use in the search.</param>
        /// <typeparam name="T">The type of items in the list.</typeparam>
        /// <returns>The zero-based index of the last occurrence of item within the entire the Immutable list, if found; otherwise, -1.</returns>
        public static int LastIndexOf<T>(this IImmutableList<T> list, T item, IEqualityComparer<T>? equalityComparer)
        {
            Requires.NotNull(list, "list");
            if (list.Count == 0)
            {
                return -1;
            }
            return list.LastIndexOf(item, list.Count - 1, list.Count, equalityComparer);
        }

        /// <summary>Searches for the specified object and returns the zero-based index of the last occurrence within the range of elements in the immutable list that extends from the first element to the specified index.</summary>
        /// <param name="list">The list to search.</param>
        /// <param name="item">The object to locate in the Immutable list. The value can be null for reference types.</param>
        /// <param name="startIndex">The zero-based starting index of the backward search.</param>
        /// <typeparam name="T">The type of items in the list.</typeparam>
        /// <returns>The zero-based index of the last occurrence of item within the range of elements in the Immutable list that extends from the first element to index, if found; otherwise, -1.</returns>
        public static int LastIndexOf<T>(this IImmutableList<T> list, T item, int startIndex)
        {
            Requires.NotNull(list, "list");
            if (list.Count == 0 && startIndex == 0)
            {
                return -1;
            }
            return list.LastIndexOf(item, startIndex, startIndex + 1, EqualityComparer<T>.Default);
        }

        /// <summary>Searches for the specified object and returns the zero-based index of the last occurrence within the range of elements in the immutable list that extends from the first element to the specified index.</summary>
        /// <param name="list">The list to search.</param>
        /// <param name="item">The object to locate in the Immutable list. The value can be null for reference types.</param>
        /// <param name="startIndex">The zero-based starting index of the backward search.</param>
        /// <param name="count">The number of elements in the section to search.</param>
        /// <typeparam name="T">The type of items in the list.</typeparam>
        /// <returns>The zero-based index of the last occurrence of item within the range of elements in the Immutable list that extends from the first element to index, if found; otherwise, -1.</returns>
        public static int LastIndexOf<T>(this IImmutableList<T> list, T item, int startIndex, int count)
        {
            Requires.NotNull(list, "list");
            return list.LastIndexOf(item, startIndex, count, EqualityComparer<T>.Default);
        }
    }
    /// <summary>Represents an immutable list, which is a strongly typed list of objects that can be accessed by index. </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(ImmutableEnumerableDebuggerProxy<>))]
    public sealed class ImmutableList<T> : IImmutableList<T>, IReadOnlyList<T>, IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable, IList<T>, ICollection<T>, IList, ICollection, IOrderedCollection<T>, IImmutableListQueries<T>, IStrongEnumerable<T, ImmutableList<T>.Enumerator>
    {
        /// <summary>Represents a list that mutates with little or no memory allocations and that can produce or build on immutable list instances very efficiently. </summary>
        [DebuggerDisplay("Count = {Count}")]
        [DebuggerTypeProxy(typeof(ImmutableListBuilderDebuggerProxy<>))]
        public sealed class Builder : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IList, ICollection, IOrderedCollection<T>, IImmutableListQueries<T>, IReadOnlyList<T>, IReadOnlyCollection<T>
        {
            private Node _root = Node.EmptyNode;

            private ImmutableList<T> _immutable;

            private int _version;

            private object _syncRoot;

            /// <summary>Gets the number of elements in this immutable list.</summary>
            /// <returns>The number of elements in this list.</returns>
            public int Count => Root.Count;

            /// <summary>Gets a value that indicates whether this instance is read-only.</summary>
            /// <returns>Always <see langword="false" />.</returns>
            bool ICollection<T>.IsReadOnly => false;

            internal int Version => _version;

            internal Node Root
            {
                get
                {
                    return _root;
                }
                private set
                {
                    _version++;
                    if (_root != value)
                    {
                        _root = value;
                        _immutable = null;
                    }
                }
            }

            /// <summary>Gets or sets the value for a given index in the list.</summary>
            /// <param name="index">The index of the item to get or set.</param>
            /// <returns>The value at the specified index.</returns>
            public T this[int index]
            {
                get
                {
                    return Root.ItemRef(index);
                }
                set
                {
                    Root = Root.ReplaceAt(index, value);
                }
            }

            T IOrderedCollection<T>.this[int index] => this[index];

            /// <summary>Gets a value that indicates whether the <see cref="T:System.Collections.IList" /> has a fixed size.</summary>
            /// <returns>
            ///   <see langword="true" /> if the <see cref="T:System.Collections.IList" /> has a fixed size; otherwise, <see langword="false" />.</returns>
            bool IList.IsFixedSize => false;

            /// <summary>Gets a value that indicates whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</summary>
            /// <returns>
            ///   <see langword="true" /> if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, <see langword="false" />.</returns>
            bool IList.IsReadOnly => false;

            /// <summary>Gets or sets the <see cref="T:System.Object" /> at the specified index.</summary>
            /// <param name="index">The index.</param>
            /// <returns>The object at the specified index.</returns>
            object? IList.this[int index]
            {
                get
                {
                    return this[index];
                }
                set
                {
                    this[index] = (T)value;
                }
            }

            /// <summary>Gets a value that indicates whether access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe).</summary>
            /// <returns>
            ///   <see langword="true" /> if access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe); otherwise, <see langword="false" />.</returns>
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            bool ICollection.IsSynchronized => false;

            /// <summary>Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.</summary>
            /// <returns>An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.</returns>
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            object ICollection.SyncRoot
            {
                get
                {
                    if (_syncRoot == null)
                    {
                        Interlocked.CompareExchange<object>(ref _syncRoot, new object(), (object)null);
                    }
                    return _syncRoot;
                }
            }

            internal Builder(ImmutableList<T> list)
            {
                Requires.NotNull(list, "list");
                _root = list._root;
                _immutable = list;
            }

            /// <summary>Gets a read-only reference to the value for a given <paramref name="index" /> into the list.</summary>
            /// <param name="index">The index of the desired element.</param>
            /// <returns>A read-only reference to the value at the specified <paramref name="index" />.</returns>
            public ref readonly T ItemRef(int index)
            {
                return ref Root.ItemRef(index);
            }

            /// <summary>Searches for the specified object and returns the zero-based index of the first occurrence within the range of elements in the immutable list.</summary>
            /// <param name="item">The object to locate in the immutable list. The value can be <see langword="null" /> for reference types.</param>
            /// <returns>The zero-based index of the first occurrence of <paramref name="item" /> within the range of elements in the immutable list, if found; otherwise, -1.</returns>
            public int IndexOf(T item)
            {
                return Root.IndexOf(item, EqualityComparer<T>.Default);
            }

            /// <summary>Inserts an item to the immutable list at the specified index.</summary>
            /// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
            /// <param name="item">The object to insert into the immutable list.</param>
            public void Insert(int index, T item)
            {
                Root = Root.Insert(index, item);
            }

            /// <summary>Removes the item at the specified index of the immutable list.</summary>
            /// <param name="index">The zero-based index of the item to remove from the list.</param>
            public void RemoveAt(int index)
            {
                Root = Root.RemoveAt(index);
            }

            /// <summary>Adds an item to the immutable list.</summary>
            /// <param name="item">The item to add to the list.</param>
            public void Add(T item)
            {
                Root = Root.Add(item);
            }

            /// <summary>Removes all items from the immutable list.</summary>
            public void Clear()
            {
                Root = Node.EmptyNode;
            }

            /// <summary>Determines whether the immutable list contains a specific value.</summary>
            /// <param name="item">The object to locate in the list.</param>
            /// <returns>
            ///   <see langword="true" /> if item is found in the list; otherwise, <see langword="false" />.</returns>
            public bool Contains(T item)
            {
                return IndexOf(item) >= 0;
            }

            /// <summary>Removes the first occurrence of a specific object from the immutable list.</summary>
            /// <param name="item">The object to remove from the list.</param>
            /// <returns>
            ///   <see langword="true" /> if item was successfully removed from the list; otherwise, <see langword="false" />. This method also returns <see langword="false" /> if item is not found in the list.</returns>
            public bool Remove(T item)
            {
                int num = IndexOf(item);
                if (num < 0)
                {
                    return false;
                }
                Root = Root.RemoveAt(num);
                return true;
            }

            /// <summary>Returns an enumerator that iterates through the collection.</summary>
            /// <returns>An enumerator that can be used to iterate through the list.</returns>
            public ImmutableList<T>.Enumerator GetEnumerator()
            {
                return Root.GetEnumerator(this);
            }

            /// <summary>Returns an enumerator that iterates through the collection.</summary>
            /// <returns>An enumerator that can be used to iterate through the collection.</returns>
            IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                return GetEnumerator();
            }

            /// <summary>Returns an enumerator that iterates through the collection.</summary>
            /// <returns>An enumerator that can be used to iterate through the collection.</returns>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            /// <summary>Performs the specified action on each element of the list.</summary>
            /// <param name="action">The delegate to perform on each element of the list.</param>
            public void ForEach(Action<T> action)
            {
                Requires.NotNull(action, "action");
                using Enumerator enumerator = GetEnumerator();
                while (enumerator.MoveNext())
                {
                    T current = enumerator.Current;
                    action(current);
                }
            }

            /// <summary>Copies the entire immutable list to a compatible one-dimensional array, starting at the beginning of the target array.</summary>
            /// <param name="array">The one-dimensional array that is the destination of the elements copied from the immutable list. The array must have zero-based indexing.</param>
            public void CopyTo(T[] array)
            {
                _root.CopyTo(array);
            }

            /// <summary>Copies the entire immutable list to a compatible one-dimensional array, starting at the specified index of the target array.</summary>
            /// <param name="array">The one-dimensional array that is the destination of the elements copied from the immutable list. The array must have zero-based indexing.</param>
            /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
            public void CopyTo(T[] array, int arrayIndex)
            {
                _root.CopyTo(array, arrayIndex);
            }

            /// <summary>Copies the entire immutable list to a compatible one-dimensional array, starting at the specified index of the target array.</summary>
            /// <param name="index">The zero-based index in the source immutable list at which copying begins.</param>
            /// <param name="array">The one-dimensional array that is the destination of the elements copied from the immutable list. The array must have zero-based indexing.</param>
            /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
            /// <param name="count">The number of elements to copy.</param>
            public void CopyTo(int index, T[] array, int arrayIndex, int count)
            {
                _root.CopyTo(index, array, arrayIndex, count);
            }

            /// <summary>Creates a shallow copy of a range of elements in the source immutable list.</summary>
            /// <param name="index">The zero-based index at which the range starts.</param>
            /// <param name="count">The number of elements in the range.</param>
            /// <returns>A shallow copy of a range of elements in the source immutable list.</returns>
            public ImmutableList<T> GetRange(int index, int count)
            {
                Requires.Range(index >= 0, "index");
                Requires.Range(count >= 0, "count");
                Requires.Range(index + count <= Count, "count");
                return ImmutableList<T>.WrapNode(Node.NodeTreeFromList(this, index, count));
            }

            /// <summary>Creates a new immutable list from the list represented by this builder by using the converter function.</summary>
            /// <param name="converter">The converter function.</param>
            /// <typeparam name="TOutput">The type of the output of the delegate converter function.</typeparam>
            /// <returns>A new immutable list from the list represented by this builder.</returns>
            public ImmutableList<TOutput> ConvertAll<TOutput>(Func<T, TOutput> converter)
            {
                Requires.NotNull(converter, "converter");
                return ImmutableList<TOutput>.WrapNode(_root.ConvertAll(converter));
            }

            /// <summary>Determines whether the immutable list contains elements that match the conditions defined by the specified predicate.</summary>
            /// <param name="match">The delegate that defines the conditions of the elements to search for.</param>
            /// <returns>
            ///   <see langword="true" /> if the immutable list contains one or more elements that match the conditions defined by the specified predicate; otherwise, <see langword="false" />.</returns>
            public bool Exists(Predicate<T> match)
            {
                return _root.Exists(match);
            }

            /// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the first occurrence within the entire immutable list.</summary>
            /// <param name="match">The delegate that defines the conditions of the element to search for.</param>
            /// <returns>The first element that matches the conditions defined by the specified predicate, if found; otherwise, the default value for type T.</returns>
            public T? Find(Predicate<T> match)
            {
                return _root.Find(match);
            }

            /// <summary>Retrieves all the elements that match the conditions defined by the specified predicate.</summary>
            /// <param name="match">The delegate that defines the conditions of the elements to search for.</param>
            /// <returns>An immutable list containing all the elements that match the conditions defined by the specified predicate, if found; otherwise, an empty immutable list.</returns>
            public ImmutableList<T> FindAll(Predicate<T> match)
            {
                return _root.FindAll(match);
            }

            /// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the entire immutable list.</summary>
            /// <param name="match">The delegate that defines the conditions of the element to search for.</param>
            /// <returns>The zero-based index of the first occurrence of an element that matches the conditions defined by <paramref name="match" />, if found; otherwise, -1.</returns>
            public int FindIndex(Predicate<T> match)
            {
                return _root.FindIndex(match);
            }

            /// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the range of elements in the immutable list that extends from the specified index to the last element.</summary>
            /// <param name="startIndex">The zero-based starting index of the search.</param>
            /// <param name="match">The delegate that defines the conditions of the element to search for.</param>
            /// <returns>The zero-based index of the first occurrence of an element that matches the conditions defined by <paramref name="match" />, if found; otherwise, -1.</returns>
            public int FindIndex(int startIndex, Predicate<T> match)
            {
                return _root.FindIndex(startIndex, match);
            }

            /// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the range of elements in the immutable list that starts at the specified index and contains the specified number of elements.</summary>
            /// <param name="startIndex">The zero-based starting index of the search.</param>
            /// <param name="count">The number of elements in the section to search.</param>
            /// <param name="match">The delegate that defines the conditions of the element to search for.</param>
            /// <returns>The zero-based index of the first occurrence of an element that matches the conditions defined by <paramref name="match" />, if found; otherwise, -1.</returns>
            public int FindIndex(int startIndex, int count, Predicate<T> match)
            {
                return _root.FindIndex(startIndex, count, match);
            }

            /// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the last occurrence within the entire immutable list.</summary>
            /// <param name="match">The delegate that defines the conditions of the element to search for.</param>
            /// <returns>The last element that matches the conditions defined by the specified predicate, found; otherwise, the default value for type T.</returns>
            public T? FindLast(Predicate<T> match)
            {
                return _root.FindLast(match);
            }

            /// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the entire immutable list.</summary>
            /// <param name="match">The delegate that defines the conditions of the element to search for.</param>
            /// <returns>The zero-based index of the last occurrence of an element that matches the conditions defined by <paramref name="match" />, if found; otherwise, -1.</returns>
            public int FindLastIndex(Predicate<T> match)
            {
                return _root.FindLastIndex(match);
            }

            /// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the range of elements in the immutable list that extends from the first element to the specified index.</summary>
            /// <param name="startIndex">The zero-based starting index of the backward search.</param>
            /// <param name="match">The delegate that defines the conditions of the element to search for.</param>
            /// <returns>The zero-based index of the last occurrence of an element that matches the conditions defined by <paramref name="match" />, if found; otherwise, -1.</returns>
            public int FindLastIndex(int startIndex, Predicate<T> match)
            {
                return _root.FindLastIndex(startIndex, match);
            }

            /// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the range of elements in the immutable list that contains the specified number of elements and ends at the specified index.</summary>
            /// <param name="startIndex">The zero-based starting index of the backward search.</param>
            /// <param name="count">The number of elements in the section to search.</param>
            /// <param name="match">The delegate that defines the conditions of the element to search for.</param>
            /// <returns>The zero-based index of the last occurrence of an element that matches the conditions defined by <paramref name="match" />, if found; otherwise, -1.</returns>
            public int FindLastIndex(int startIndex, int count, Predicate<T> match)
            {
                return _root.FindLastIndex(startIndex, count, match);
            }

            /// <summary>Searches for the specified object and returns the zero-based index of the first occurrence within the range of elements in the immutable list that extends from the specified index to the last element.</summary>
            /// <param name="item">The object to locate in the immutable list. The value can be <see langword="null" /> for reference types.</param>
            /// <param name="index">The zero-based starting index of the search. 0 (zero) is valid in an empty list.</param>
            /// <returns>The zero-based index of the first occurrence of item within the range of elements in the immutable list that extends from <paramref name="index" /> to the last element, if found; otherwise, -1.</returns>
            public int IndexOf(T item, int index)
            {
                return _root.IndexOf(item, index, Count - index, EqualityComparer<T>.Default);
            }

            /// <summary>Searches for the specified object and returns the zero-based index of the first occurrence within the range of elements in the immutable list that starts at the specified index and contains the specified number of elements.</summary>
            /// <param name="item">The object to locate in the immutable list. The value can be <see langword="null" /> for reference types.</param>
            /// <param name="index">The zero-based starting index of the search. 0 (zero) is valid in an empty list.</param>
            /// <param name="count">The number of elements in the section to search.</param>
            /// <returns>The zero-based index of the first occurrence of item within the range of elements in the immutable list that starts at <paramref name="index" /> and contains <paramref name="count" /> number of elements, if found; otherwise, -1.</returns>
            public int IndexOf(T item, int index, int count)
            {
                return _root.IndexOf(item, index, count, EqualityComparer<T>.Default);
            }

            /// <summary>Searches for the specified object and returns the zero-based index of the first occurrence within the range of elements in the <see cref="T:System.Collections.Immutable.ImmutableList`1.Builder" /> that starts at the specified index and contains the specified number of elements.</summary>
            /// <param name="item">The object to locate in the immutable list. The value can be <see langword="null" /> for reference types.</param>
            /// <param name="index">The zero-based starting index of the search. 0 (zero) is valid in an empty list.</param>
            /// <param name="count">The number of elements to search.</param>
            /// <param name="equalityComparer">The value comparer to use for comparing elements for equality.</param>
            /// <returns>The zero-based index of the first occurrence of item within the range of elements in the immutable list that starts at <paramref name="index" /> and contains <paramref name="count" /> number of elements, if found; otherwise, -1</returns>
            public int IndexOf(T item, int index, int count, IEqualityComparer<T>? equalityComparer)
            {
                return _root.IndexOf(item, index, count, equalityComparer);
            }

            /// <summary>Searches for the specified object and returns the zero-based index of the last occurrence within the entire immutable list.</summary>
            /// <param name="item">The object to locate in the immutable list. The value can be <see langword="null" /> for reference types.</param>
            /// <returns>The zero-based index of the last occurrence of <paramref name="item" /> within the entire immutable list, if found; otherwise, -1.</returns>
            public int LastIndexOf(T item)
            {
                if (Count == 0)
                {
                    return -1;
                }
                return _root.LastIndexOf(item, Count - 1, Count, EqualityComparer<T>.Default);
            }

            /// <summary>Searches for the specified object and returns the zero-based index of the last occurrence within the range of elements in the immutable list that extends from the first element to the specified index.</summary>
            /// <param name="item">The object to locate in the immutable list. The value can be <see langword="null" /> for reference types.</param>
            /// <param name="startIndex">The zero-based starting index of the backward search.</param>
            /// <returns>The zero-based index of the last occurrence of <paramref name="item" /> within the range of elements in the immutable list that extends from the first element to index, if found; otherwise, -1.</returns>
            public int LastIndexOf(T item, int startIndex)
            {
                if (Count == 0 && startIndex == 0)
                {
                    return -1;
                }
                return _root.LastIndexOf(item, startIndex, startIndex + 1, EqualityComparer<T>.Default);
            }

            /// <summary>Searches for the specified object and returns the zero-based index of the last occurrence within the range of elements in the immutable list that contains the specified number of elements and ends at the specified index.</summary>
            /// <param name="item">The object to locate in the immutable list. The value can be <see langword="null" /> for reference types.</param>
            /// <param name="startIndex">The zero-based starting index of the backward search.</param>
            /// <param name="count">The number of elements in the section to search.</param>
            /// <returns>The zero-based index of the last occurrence of <paramref name="item" /> within the range of elements in the immutable list that contains <paramref name="count" /> number of elements and ends at <paramref name="startIndex" />, if found; otherwise, -1.</returns>
            public int LastIndexOf(T item, int startIndex, int count)
            {
                return _root.LastIndexOf(item, startIndex, count, EqualityComparer<T>.Default);
            }

            /// <summary>Searches for the specified object and returns the zero-based index of the last occurrence within the range of elements in the immutable list that contains the specified number of elements and ends at the specified index.</summary>
            /// <param name="item">The object to locate in the immutable list. The value can be <see langword="null" /> for reference types.</param>
            /// <param name="startIndex">The zero-based starting index of the search. 0 (zero) is valid in an empty list.</param>
            /// <param name="count">The number of elements to search.</param>
            /// <param name="equalityComparer">The value comparer to use for comparing elements for equality.</param>
            /// <returns>The zero-based index of the first occurrence of item within the range of elements in the immutable list that starts at <paramref name="startIndex" /> and contains <paramref name="count" /> number of elements, if found; otherwise, -1</returns>
            public int LastIndexOf(T item, int startIndex, int count, IEqualityComparer<T>? equalityComparer)
            {
                return _root.LastIndexOf(item, startIndex, count, equalityComparer);
            }

            /// <summary>Determines whether every element in the immutable list matches the conditions defined by the specified predicate.</summary>
            /// <param name="match">The delegate that defines the conditions to check against the elements.</param>
            /// <returns>
            ///   <see langword="true" /> if every element in the immutable list matches the conditions defined by the specified predicate; otherwise, <see langword="false" />. If the list has no elements, the return value is <see langword="true" />.</returns>
            public bool TrueForAll(Predicate<T> match)
            {
                return _root.TrueForAll(match);
            }

            /// <summary>Adds a series of elements to the end of this list.</summary>
            /// <param name="items">The elements to add to the end of the list.</param>
            public void AddRange(IEnumerable<T> items)
            {
                Requires.NotNull(items, "items");
                Root = Root.AddRange(items);
            }

            /// <summary>Inserts the elements of a collection into the immutable list at the specified index.</summary>
            /// <param name="index">The zero-based index at which the new elements should be inserted.</param>
            /// <param name="items">The collection whose elements should be inserted into the immutable list. The collection itself cannot be <see langword="null" />, but it can contain elements that are null, if type <c>T</c> is a reference type.</param>
            public void InsertRange(int index, IEnumerable<T> items)
            {
                Requires.Range(index >= 0 && index <= Count, "index");
                Requires.NotNull(items, "items");
                Root = Root.InsertRange(index, items);
            }

            /// <summary>Removes all the elements that match the conditions defined by the specified predicate.</summary>
            /// <param name="match">The delegate that defines the conditions of the elements to remove.</param>
            /// <returns>The number of elements removed from the immutable list.</returns>
            public int RemoveAll(Predicate<T> match)
            {
                Requires.NotNull(match, "match");
                int count = Count;
                Root = Root.RemoveAll(match);
                return count - Count;
            }

            /// <summary>Removes the first occurrence matching the specified value from this list.</summary>
            /// <param name="item">The item to remove.</param>
            /// <param name="equalityComparer">The equality comparer to use in the search.
            ///       If <see langword="null" />, <see cref="P:System.Collections.Generic.EqualityComparer`1.Default" /> is used.</param>
            /// <returns>A value indicating whether the specified element was found and removed from the collection.</returns>
            public bool Remove(T item, IEqualityComparer<T>? equalityComparer)
            {
                int num = IndexOf(item, 0, Count, equalityComparer);
                if (num >= 0)
                {
                    RemoveAt(num);
                    return true;
                }
                return false;
            }

            /// <summary>Removes the specified range of values from this list.</summary>
            /// <param name="index">The starting index to begin removal.</param>
            /// <param name="count">The number of elements to remove.</param>
            public void RemoveRange(int index, int count)
            {
                Requires.Range(index >= 0 && index <= Count, "index");
                Requires.Range(count >= 0 && index + count <= Count, "count");
                int num = count;
                while (num-- > 0)
                {
                    RemoveAt(index);
                }
            }

            /// <summary>Removes any first occurrences of the specified values from this list.</summary>
            /// <param name="items">The items to remove if matches are found in this list.</param>
            /// <param name="equalityComparer">The equality comparer to use in the search.
            ///       If <see langword="null" />, <see cref="P:System.Collections.Generic.EqualityComparer`1.Default" /> is used.</param>
            public void RemoveRange(IEnumerable<T> items, IEqualityComparer<T>? equalityComparer)
            {
                Requires.NotNull(items, "items");
                foreach (T item in items.GetEnumerableDisposable<T, Enumerator>())
                {
                    int num = Root.IndexOf(item, equalityComparer);
                    if (num >= 0)
                    {
                        RemoveAt(num);
                    }
                }
            }

            /// <summary>Removes any first occurrences of the specified values from this list.</summary>
            /// <param name="items">The items to remove if matches are found in this list.</param>
            public void RemoveRange(IEnumerable<T> items)
            {
                RemoveRange(items, EqualityComparer<T>.Default);
            }

            /// <summary>Replaces the first equal element in the list with the specified element.</summary>
            /// <param name="oldValue">The element to replace.</param>
            /// <param name="newValue">The element to replace the old element with.</param>
            /// <exception cref="T:System.ArgumentException">The old value does not exist in the list.</exception>
            public void Replace(T oldValue, T newValue)
            {
                Replace(oldValue, newValue, EqualityComparer<T>.Default);
            }

            /// <summary>Replaces the first equal element in the list with the specified element.</summary>
            /// <param name="oldValue">The element to replace.</param>
            /// <param name="newValue">The element to replace the old element with.</param>
            /// <param name="equalityComparer">The equality comparer to use in the search.
            ///       If <see langword="null" />, <see cref="P:System.Collections.Generic.EqualityComparer`1.Default" /> is used.</param>
            /// <exception cref="T:System.ArgumentException">The old value does not exist in the list.</exception>
            public void Replace(T oldValue, T newValue, IEqualityComparer<T>? equalityComparer)
            {
                int num = IndexOf(oldValue, 0, Count, equalityComparer);
                if (num < 0)
                {
                    throw new ArgumentException(MDCFR.Properties.Resources.CannotFindOldValue, "oldValue");
                }
                Root = Root.ReplaceAt(num, newValue);
            }

            /// <summary>Reverses the order of the elements in the entire immutable list.</summary>
            public void Reverse()
            {
                Reverse(0, Count);
            }

            /// <summary>Reverses the order of the elements in the specified range of the immutable list.</summary>
            /// <param name="index">The zero-based starting index of the range to reverse.</param>
            /// <param name="count">The number of elements in the range to reverse.</param>
            public void Reverse(int index, int count)
            {
                Requires.Range(index >= 0, "index");
                Requires.Range(count >= 0, "count");
                Requires.Range(index + count <= Count, "count");
                Root = Root.Reverse(index, count);
            }

            /// <summary>Sorts the elements in the entire immutable list by using the default comparer.</summary>
            public void Sort()
            {
                Root = Root.Sort();
            }

            /// <summary>Sorts the elements in the entire immutable list by using the specified comparison object.</summary>
            /// <param name="comparison">The object to use when comparing elements.</param>
            /// <exception cref="T:System.ArgumentNullException">
            ///   <paramref name="comparison" /> is <see langword="null" />.</exception>
            public void Sort(Comparison<T> comparison)
            {
                Requires.NotNull(comparison, "comparison");
                Root = Root.Sort(comparison);
            }

            /// <summary>Sorts the elements in the entire immutable list by using the specified comparer.</summary>
            /// <param name="comparer">The implementation to use when comparing elements, or <see langword="null" /> to use the default comparer (<see cref="P:System.Collections.Generic.Comparer`1.Default" />).</param>
            public void Sort(IComparer<T>? comparer)
            {
                Root = Root.Sort(comparer);
            }

            /// <summary>Sorts the elements in a range of elements in the immutable list  by using the specified comparer.</summary>
            /// <param name="index">The zero-based starting index of the range to sort.</param>
            /// <param name="count">The length of the range to sort.</param>
            /// <param name="comparer">The implementation to use when comparing elements, or <see langword="null" /> to use the default comparer (<see cref="P:System.Collections.Generic.Comparer`1.Default" />).</param>
            public void Sort(int index, int count, IComparer<T>? comparer)
            {
                Requires.Range(index >= 0, "index");
                Requires.Range(count >= 0, "count");
                Requires.Range(index + count <= Count, "count");
                Root = Root.Sort(index, count, comparer);
            }

            /// <summary>Searches the entire <see cref="T:System.Collections.Immutable.ImmutableList`1.Builder" /> for an element using the default comparer and returns the zero-based index of the element.</summary>
            /// <param name="item">The object to locate. The value can be null for reference types.</param>
            /// <exception cref="T:System.InvalidOperationException">The default comparer <see cref="P:System.Collections.Generic.Comparer`1.Default" /> cannot find an implementation of the <see cref="T:System.IComparable`1" /> generic interface or the <see cref="T:System.IComparable" /> interface for type T.</exception>
            /// <returns>The zero-based index of item in the <see cref="T:System.Collections.Immutable.ImmutableList`1.Builder" />, if item is found; otherwise, a negative number that is the bitwise complement of the index of the next element that is larger than <paramref name="item" />.</returns>
            public int BinarySearch(T item)
            {
                return BinarySearch(item, null);
            }

            /// <summary>Searches the entire <see cref="T:System.Collections.Immutable.ImmutableList`1.Builder" /> for an element using the specified comparer and returns the zero-based index of the element.</summary>
            /// <param name="item">The object to locate. This value can be null for reference types.</param>
            /// <param name="comparer">The implementation to use when comparing elements, or <see langword="null" /> for the default comparer.</param>
            /// <exception cref="T:System.InvalidOperationException">
            ///   <paramref name="comparer" /> is <see langword="null" />, and the default comparer <see cref="P:System.Collections.Generic.Comparer`1.Default" /> cannot find an implementation of the <see cref="T:System.IComparable`1" /> generic interface or the <see cref="T:System.IComparable" /> interface for type T.</exception>
            /// <returns>The zero-based index of item in the <see cref="T:System.Collections.Immutable.ImmutableList`1.Builder" />, if item is found; otherwise, a negative number that is the bitwise complement of the index of the next element that is larger than <paramref name="item" />.</returns>
            public int BinarySearch(T item, IComparer<T>? comparer)
            {
                return BinarySearch(0, Count, item, comparer);
            }

            /// <summary>Searches the specified range of the <see cref="T:System.Collections.Immutable.ImmutableList`1.Builder" /> for an element using the specified comparer and returns the zero-based index of the element.</summary>
            /// <param name="index">The zero-based starting index of the range to search.</param>
            /// <param name="count">The length of the range to search.</param>
            /// <param name="item">The object to locate. This value can be null for reference types.</param>
            /// <param name="comparer">The implementation to use when comparing elements, or <see langword="null" /> for the default comparer.</param>
            /// <exception cref="T:System.ArgumentOutOfRangeException">
            ///   <paramref name="index" /> is less than 0.
            /// -or-
            ///
            /// <paramref name="count" /> is less than 0.</exception>
            /// <exception cref="T:System.ArgumentException">
            ///   <paramref name="index" /> and <paramref name="count" /> do not denote a valid range in the <see cref="T:System.Collections.Generic.List`1" />.</exception>
            /// <exception cref="T:System.InvalidOperationException">
            ///   <paramref name="comparer" /> is <see langword="null" />, and the default comparer <see cref="P:System.Collections.Generic.Comparer`1.Default" /> cannot find an implementation of the <see cref="T:System.IComparable`1" /> generic interface or the <see cref="T:System.IComparable" /> interface for type T.</exception>
            /// <returns>The zero-based index of item in the <see cref="T:System.Collections.Immutable.ImmutableList`1.Builder" />, if item is found; otherwise, a negative number that is the bitwise complement of the index of the next element that is larger than <paramref name="item" />.</returns>
            public int BinarySearch(int index, int count, T item, IComparer<T>? comparer)
            {
                return Root.BinarySearch(index, count, item, comparer);
            }

            /// <summary>Creates an immutable list based on the contents of this instance.</summary>
            /// <returns>An immutable list.</returns>
            public ImmutableList<T> ToImmutable()
            {
                return _immutable ?? (_immutable = ImmutableList<T>.WrapNode(Root));
            }

            /// <summary>Adds an item to the list.</summary>
            /// <param name="value">The object to add to the list.</param>
            /// <exception cref="T:System.NotImplementedException" />
            /// <returns>The position into which the new element was inserted, or -1 to indicate that the item was not inserted into the collection.</returns>
            int IList.Add(object value)
            {
                Add((T)value);
                return Count - 1;
            }

            /// <summary>Removes all items from the list.</summary>
            /// <exception cref="T:System.NotImplementedException" />
            void IList.Clear()
            {
                Clear();
            }

            /// <summary>Determines whether the list contains a specific value.</summary>
            /// <param name="value">The object to locate in the list.</param>
            /// <exception cref="T:System.NotImplementedException" />
            /// <returns>
            ///   <see langword="true" /> if the <see cref="T:System.Object" /> is found in the list; otherwise, <see langword="false" />.</returns>
            bool IList.Contains(object value)
            {
                if (ImmutableList<T>.IsCompatibleObject(value))
                {
                    return Contains((T)value);
                }
                return false;
            }

            /// <summary>Determines the index of a specific item in the list.</summary>
            /// <param name="value">The object to locate in the list.</param>
            /// <exception cref="T:System.NotImplementedException" />
            /// <returns>The index of <paramref name="value" /> if found in the list; otherwise, -1.</returns>
            int IList.IndexOf(object value)
            {
                if (ImmutableList<T>.IsCompatibleObject(value))
                {
                    return IndexOf((T)value);
                }
                return -1;
            }

            /// <summary>Inserts an item to the list at the specified index.</summary>
            /// <param name="index">The zero-based index at which <paramref name="value" /> should be inserted.</param>
            /// <param name="value">The object to insert into the list.</param>
            /// <exception cref="T:System.NotImplementedException" />
            void IList.Insert(int index, object value)
            {
                Insert(index, (T)value);
            }

            /// <summary>Removes the first occurrence of a specific object from the list.</summary>
            /// <param name="value">The object to remove from the list.</param>
            /// <exception cref="T:System.NotImplementedException" />
            void IList.Remove(object value)
            {
                if (ImmutableList<T>.IsCompatibleObject(value))
                {
                    Remove((T)value);
                }
            }

            /// <summary>Copies the elements of the list to an array, starting at a particular array index.</summary>
            /// <param name="array">The one-dimensional array that is the destination of the elements copied from the list. The array must have zero-based indexing.</param>
            /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
            /// <exception cref="T:System.NotImplementedException" />
            void ICollection.CopyTo(Array array, int arrayIndex)
            {
                Root.CopyTo(array, arrayIndex);
            }
        }

        /// <summary>Enumerates the contents of a binary tree.  </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator, ISecurePooledObjectUser, IStrongEnumerator<T>
        {
            private readonly Builder _builder;

            private readonly int _poolUserId;

            private readonly int _startIndex;

            private readonly int _count;

            private int _remainingCount;

            private readonly bool _reversed;

            private Node _root;

            private SecurePooledObject<Stack<RefAsValueType<Node>>> _stack;

            private Node _current;

            private int _enumeratingBuilderVersion;

            int ISecurePooledObjectUser.PoolUserId => _poolUserId;

            /// <summary>Gets the element at the current position of the enumerator.</summary>
            /// <returns>The element at the current position of the enumerator.</returns>
            public T Current
            {
                get
                {
                    ThrowIfDisposed();
                    if (_current != null)
                    {
                        return _current.Value;
                    }
                    throw new InvalidOperationException();
                }
            }

            /// <summary>The current element.</summary>
            /// <returns>The element in the collection at the current position of the enumerator.</returns>
            object? IEnumerator.Current => Current;

            internal Enumerator(Node root, Builder? builder = null, int startIndex = -1, int count = -1, bool reversed = false)
            {
                Requires.NotNull(root, "root");
                Requires.Range(startIndex >= -1, "startIndex");
                Requires.Range(count >= -1, "count");
                Requires.Argument(reversed || count == -1 || ((startIndex != -1) ? startIndex : 0) + count <= root.Count);
                Requires.Argument(!reversed || count == -1 || ((startIndex == -1) ? (root.Count - 1) : startIndex) - count + 1 >= 0);
                _root = root;
                _builder = builder;
                _current = null;
                _startIndex = ((startIndex >= 0) ? startIndex : (reversed ? (root.Count - 1) : 0));
                _count = ((count == -1) ? root.Count : count);
                _remainingCount = _count;
                _reversed = reversed;
                _enumeratingBuilderVersion = builder?.Version ?? (-1);
                _poolUserId = SecureObjectPool.NewId();
                _stack = null;
                if (_count > 0)
                {
                    if (!SecureObjectPool<Stack<RefAsValueType<Node>>, Enumerator>.TryTake(this, out _stack))
                    {
                        _stack = SecureObjectPool<Stack<RefAsValueType<Node>>, Enumerator>.PrepNew(this, new Stack<RefAsValueType<Node>>(root.Height));
                    }
                    ResetStack();
                }
            }

            /// <summary>Releases the resources used by the current instance of the <see cref="T:System.Collections.Immutable.ImmutableList`1.Enumerator" /> class.</summary>
            public void Dispose()
            {
                _root = null;
                _current = null;
                if (_stack != null && _stack.TryUse(ref this, out var value))
                {
                    value.ClearFastWhenEmpty();
                    SecureObjectPool<Stack<RefAsValueType<Node>>, Enumerator>.TryAdd(this, _stack);
                }
                _stack = null;
            }

            /// <summary>Advances enumeration to the next element of the immutable list.</summary>
            /// <returns>
            ///   <see langword="true" /> if the enumerator was successfully advanced to the next element; <see langword="false" /> if the enumerator has passed the end of the list.</returns>
            public bool MoveNext()
            {
                ThrowIfDisposed();
                ThrowIfChanged();
                if (_stack != null)
                {
                    Stack<RefAsValueType<Node>> stack = _stack.Use(ref this);
                    if (_remainingCount > 0 && stack.Count > 0)
                    {
                        PushNext(NextBranch(_current = stack.Pop().Value));
                        _remainingCount--;
                        return true;
                    }
                }
                _current = null;
                return false;
            }

            /// <summary>Sets the enumerator to its initial position, which is before the first element in the immutable list.</summary>
            public void Reset()
            {
                ThrowIfDisposed();
                _enumeratingBuilderVersion = ((_builder != null) ? _builder.Version : (-1));
                _remainingCount = _count;
                if (_stack != null)
                {
                    ResetStack();
                }
            }

            private void ResetStack()
            {
                Stack<RefAsValueType<Node>> stack = _stack.Use(ref this);
                stack.ClearFastWhenEmpty();
                Node node = _root;
                int num = (_reversed ? (_root.Count - _startIndex - 1) : _startIndex);
                while (!node.IsEmpty && num != PreviousBranch(node).Count)
                {
                    if (num < PreviousBranch(node).Count)
                    {
                        stack.Push(new RefAsValueType<Node>(node));
                        node = PreviousBranch(node);
                    }
                    else
                    {
                        num -= PreviousBranch(node).Count + 1;
                        node = NextBranch(node);
                    }
                }
                if (!node.IsEmpty)
                {
                    stack.Push(new RefAsValueType<Node>(node));
                }
            }

            private Node NextBranch(Node node)
            {
                if (!_reversed)
                {
                    return node.Right;
                }
                return node.Left;
            }

            private Node PreviousBranch(Node node)
            {
                if (!_reversed)
                {
                    return node.Left;
                }
                return node.Right;
            }

            private void ThrowIfDisposed()
            {
                if (_root == null || (_stack != null && !_stack.IsOwned(ref this)))
                {
                    Requires.FailObjectDisposed(this);
                }
            }

            private void ThrowIfChanged()
            {
                if (_builder != null && _builder.Version != _enumeratingBuilderVersion)
                {
                    throw new InvalidOperationException(MDCFR.Properties.Resources.CollectionModifiedDuringEnumeration);
                }
            }

            private void PushNext(Node node)
            {
                Requires.NotNull(node, "node");
                if (!node.IsEmpty)
                {
                    Stack<RefAsValueType<Node>> stack = _stack.Use(ref this);
                    while (!node.IsEmpty)
                    {
                        stack.Push(new RefAsValueType<Node>(node));
                        node = PreviousBranch(node);
                    }
                }
            }
        }

        [DebuggerDisplay("{_key}")]
        internal sealed class Node : IBinaryTree<T>, IBinaryTree, IEnumerable<T>, IEnumerable
        {
            internal static readonly Node EmptyNode = new Node();

            private T _key;

            private bool _frozen;

            private byte _height;

            private int _count;

            private Node _left;

            private Node _right;

            public bool IsEmpty => _left == null;

            public int Height => _height;

            public Node? Left => _left;

            IBinaryTree? IBinaryTree.Left => _left;

            public Node? Right => _right;

            IBinaryTree? IBinaryTree.Right => _right;

            IBinaryTree<T>? IBinaryTree<T>.Left => _left;

            IBinaryTree<T>? IBinaryTree<T>.Right => _right;

            public T Value => _key;

            public int Count => _count;

            internal T Key => _key;

            internal T this[int index]
            {
                get
                {
                    Requires.Range(index >= 0 && index < Count, "index");
                    if (index < _left._count)
                    {
                        return _left[index];
                    }
                    if (index > _left._count)
                    {
                        return _right[index - _left._count - 1];
                    }
                    return _key;
                }
            }

            private int BalanceFactor => _right._height - _left._height;

            private bool IsRightHeavy => BalanceFactor >= 2;

            private bool IsLeftHeavy => BalanceFactor <= -2;

            private bool IsBalanced => (uint)(BalanceFactor + 1) <= 2u;

            private Node()
            {
                _frozen = true;
            }

            private Node(T key, Node left, Node right, bool frozen = false)
            {
                Requires.NotNull(left, "left");
                Requires.NotNull(right, "right");
                _key = key;
                _left = left;
                _right = right;
                _height = ParentHeight(left, right);
                _count = ParentCount(left, right);
                _frozen = frozen;
            }

            internal ref readonly T ItemRef(int index)
            {
                Requires.Range(index >= 0 && index < Count, "index");
                return ref ItemRefUnchecked(index);
            }

            private ref readonly T ItemRefUnchecked(int index)
            {
                if (index < _left._count)
                {
                    return ref _left.ItemRefUnchecked(index);
                }
                if (index > _left._count)
                {
                    return ref _right.ItemRefUnchecked(index - _left._count - 1);
                }
                return ref _key;
            }

            public Enumerator GetEnumerator()
            {
                return new Enumerator(this);
            }

            [ExcludeFromCodeCoverage]
            IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                return GetEnumerator();
            }

            [ExcludeFromCodeCoverage]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            internal Enumerator GetEnumerator(Builder builder)
            {
                return new Enumerator(this, builder);
            }

            internal static Node NodeTreeFromList(IOrderedCollection<T> items, int start, int length)
            {
                Requires.NotNull(items, "items");
                Requires.Range(start >= 0, "start");
                Requires.Range(length >= 0, "length");
                if (length == 0)
                {
                    return EmptyNode;
                }
                int num = (length - 1) / 2;
                int num2 = length - 1 - num;
                Node left = NodeTreeFromList(items, start, num2);
                Node right = NodeTreeFromList(items, start + num2 + 1, num);
                return new Node(items[start + num2], left, right, frozen: true);
            }

            internal Node Add(T key)
            {
                if (IsEmpty)
                {
                    return CreateLeaf(key);
                }
                Node right = _right.Add(key);
                Node node = MutateRight(right);
                if (!node.IsBalanced)
                {
                    return node.BalanceRight();
                }
                return node;
            }

            internal Node Insert(int index, T key)
            {
                Requires.Range(index >= 0 && index <= Count, "index");
                if (IsEmpty)
                {
                    return CreateLeaf(key);
                }
                if (index <= _left._count)
                {
                    Node left = _left.Insert(index, key);
                    Node node = MutateLeft(left);
                    if (!node.IsBalanced)
                    {
                        return node.BalanceLeft();
                    }
                    return node;
                }
                Node right = _right.Insert(index - _left._count - 1, key);
                Node node2 = MutateRight(right);
                if (!node2.IsBalanced)
                {
                    return node2.BalanceRight();
                }
                return node2;
            }

            internal Node AddRange(IEnumerable<T> keys)
            {
                Requires.NotNull(keys, "keys");
                if (IsEmpty)
                {
                    return CreateRange(keys);
                }
                Node right = _right.AddRange(keys);
                Node node = MutateRight(right);
                return node.BalanceMany();
            }

            internal Node InsertRange(int index, IEnumerable<T> keys)
            {
                Requires.Range(index >= 0 && index <= Count, "index");
                Requires.NotNull(keys, "keys");
                if (IsEmpty)
                {
                    return CreateRange(keys);
                }
                Node node;
                if (index <= _left._count)
                {
                    Node left = _left.InsertRange(index, keys);
                    node = MutateLeft(left);
                }
                else
                {
                    Node right = _right.InsertRange(index - _left._count - 1, keys);
                    node = MutateRight(right);
                }
                return node.BalanceMany();
            }

            internal Node RemoveAt(int index)
            {
                Requires.Range(index >= 0 && index < Count, "index");
                Node node;
                if (index == _left._count)
                {
                    if (_right.IsEmpty && _left.IsEmpty)
                    {
                        node = EmptyNode;
                    }
                    else if (_right.IsEmpty && !_left.IsEmpty)
                    {
                        node = _left;
                    }
                    else if (!_right.IsEmpty && _left.IsEmpty)
                    {
                        node = _right;
                    }
                    else
                    {
                        Node node2 = _right;
                        while (!node2._left.IsEmpty)
                        {
                            node2 = node2._left;
                        }
                        Node right = _right.RemoveAt(0);
                        node = node2.MutateBoth(_left, right);
                    }
                }
                else if (index < _left._count)
                {
                    Node left = _left.RemoveAt(index);
                    node = MutateLeft(left);
                }
                else
                {
                    Node right2 = _right.RemoveAt(index - _left._count - 1);
                    node = MutateRight(right2);
                }
                if (!node.IsEmpty && !node.IsBalanced)
                {
                    return node.Balance();
                }
                return node;
            }

            internal Node RemoveAll(Predicate<T> match)
            {
                Requires.NotNull(match, "match");
                Node node = this;
                Enumerator enumerator = new Enumerator(node);
                try
                {
                    int num = 0;
                    while (enumerator.MoveNext())
                    {
                        if (match(enumerator.Current))
                        {
                            node = node.RemoveAt(num);
                            enumerator.Dispose();
                            enumerator = new Enumerator(node, null, num);
                        }
                        else
                        {
                            num++;
                        }
                    }
                    return node;
                }
                finally
                {
                    enumerator.Dispose();
                }
            }

            internal Node ReplaceAt(int index, T value)
            {
                Requires.Range(index >= 0 && index < Count, "index");
                if (index == _left._count)
                {
                    return MutateKey(value);
                }
                if (index < _left._count)
                {
                    Node left = _left.ReplaceAt(index, value);
                    return MutateLeft(left);
                }
                Node right = _right.ReplaceAt(index - _left._count - 1, value);
                return MutateRight(right);
            }

            internal Node Reverse()
            {
                return Reverse(0, Count);
            }

            internal Node Reverse(int index, int count)
            {
                Requires.Range(index >= 0, "index");
                Requires.Range(count >= 0, "count");
                Requires.Range(index + count <= Count, "index");
                Node node = this;
                int num = index;
                int num2 = index + count - 1;
                while (num < num2)
                {
                    T value = node.ItemRef(num);
                    T value2 = node.ItemRef(num2);
                    node = node.ReplaceAt(num2, value).ReplaceAt(num, value2);
                    num++;
                    num2--;
                }
                return node;
            }

            internal Node Sort()
            {
                return Sort(Comparer<T>.Default);
            }

            internal Node Sort(Comparison<T> comparison)
            {
                Requires.NotNull(comparison, "comparison");
                T[] array = new T[Count];
                CopyTo(array);
                Array.Sort(array, comparison);
                return NodeTreeFromList(array.AsOrderedCollection(), 0, Count);
            }

            internal Node Sort(IComparer<T>? comparer)
            {
                return Sort(0, Count, comparer);
            }

            internal Node Sort(int index, int count, IComparer<T>? comparer)
            {
                Requires.Range(index >= 0, "index");
                Requires.Range(count >= 0, "count");
                Requires.Argument(index + count <= Count);
                T[] array = new T[Count];
                CopyTo(array);
                Array.Sort(array, index, count, comparer);
                return NodeTreeFromList(array.AsOrderedCollection(), 0, Count);
            }

            internal int BinarySearch(int index, int count, T item, IComparer<T>? comparer)
            {
                Requires.Range(index >= 0, "index");
                Requires.Range(count >= 0, "count");
                if (comparer == null)
                {
                    comparer = Comparer<T>.Default;
                }
                if (IsEmpty || count <= 0)
                {
                    return ~index;
                }
                int count2 = _left.Count;
                if (index + count <= count2)
                {
                    return _left.BinarySearch(index, count, item, comparer);
                }
                if (index > count2)
                {
                    int num = _right.BinarySearch(index - count2 - 1, count, item, comparer);
                    int num2 = count2 + 1;
                    if (num >= 0)
                    {
                        return num + num2;
                    }
                    return num - num2;
                }
                int num3 = comparer.Compare(item, _key);
                if (num3 == 0)
                {
                    return count2;
                }
                if (num3 > 0)
                {
                    int num4 = count - (count2 - index) - 1;
                    int num5 = ((num4 < 0) ? (-1) : _right.BinarySearch(0, num4, item, comparer));
                    int num6 = count2 + 1;
                    if (num5 >= 0)
                    {
                        return num5 + num6;
                    }
                    return num5 - num6;
                }
                if (index == count2)
                {
                    return ~index;
                }
                return _left.BinarySearch(index, count, item, comparer);
            }

            internal int IndexOf(T item, IEqualityComparer<T>? equalityComparer)
            {
                return IndexOf(item, 0, Count, equalityComparer);
            }

            internal bool Contains(T item, IEqualityComparer<T> equalityComparer)
            {
                return Contains(this, item, equalityComparer);
            }

            internal int IndexOf(T item, int index, int count, IEqualityComparer<T>? equalityComparer)
            {
                Requires.Range(index >= 0, "index");
                Requires.Range(count >= 0, "count");
                Requires.Range(count <= Count, "count");
                Requires.Range(index + count <= Count, "count");
                if (equalityComparer == null)
                {
                    equalityComparer = EqualityComparer<T>.Default;
                }
                using (Enumerator enumerator = new Enumerator(this, null, index, count))
                {
                    while (enumerator.MoveNext())
                    {
                        if (equalityComparer.Equals(item, enumerator.Current))
                        {
                            return index;
                        }
                        index++;
                    }
                }
                return -1;
            }

            internal int LastIndexOf(T item, int index, int count, IEqualityComparer<T>? equalityComparer)
            {
                Requires.Range(index >= 0, "index");
                Requires.Range(count >= 0 && count <= Count, "count");
                Requires.Argument(index - count + 1 >= 0);
                if (equalityComparer == null)
                {
                    equalityComparer = EqualityComparer<T>.Default;
                }
                using (Enumerator enumerator = new Enumerator(this, null, index, count, reversed: true))
                {
                    while (enumerator.MoveNext())
                    {
                        if (equalityComparer.Equals(item, enumerator.Current))
                        {
                            return index;
                        }
                        index--;
                    }
                }
                return -1;
            }

            internal void CopyTo(T[] array)
            {
                Requires.NotNull(array, "array");
                Requires.Range(array.Length >= Count, "array");
                int num = 0;
                using Enumerator enumerator = GetEnumerator();
                while (enumerator.MoveNext())
                {
                    T current = enumerator.Current;
                    array[num++] = current;
                }
            }

            internal void CopyTo(T[] array, int arrayIndex)
            {
                Requires.NotNull(array, "array");
                Requires.Range(arrayIndex >= 0, "arrayIndex");
                Requires.Range(array.Length >= arrayIndex + Count, "arrayIndex");
                using Enumerator enumerator = GetEnumerator();
                while (enumerator.MoveNext())
                {
                    T current = enumerator.Current;
                    array[arrayIndex++] = current;
                }
            }

            internal void CopyTo(int index, T[] array, int arrayIndex, int count)
            {
                Requires.NotNull(array, "array");
                Requires.Range(index >= 0, "index");
                Requires.Range(count >= 0, "count");
                Requires.Range(index + count <= Count, "count");
                Requires.Range(arrayIndex >= 0, "arrayIndex");
                Requires.Range(arrayIndex + count <= array.Length, "arrayIndex");
                using Enumerator enumerator = new Enumerator(this, null, index, count);
                while (enumerator.MoveNext())
                {
                    array[arrayIndex++] = enumerator.Current;
                }
            }

            internal void CopyTo(Array array, int arrayIndex)
            {
                Requires.NotNull(array, "array");
                Requires.Range(arrayIndex >= 0, "arrayIndex");
                Requires.Range(array.Length >= arrayIndex + Count, "arrayIndex");
                using Enumerator enumerator = GetEnumerator();
                while (enumerator.MoveNext())
                {
                    T current = enumerator.Current;
                    array.SetValue(current, arrayIndex++);
                }
            }

            internal ImmutableList<TOutput>.Node ConvertAll<TOutput>(Func<T, TOutput> converter)
            {
                ImmutableList<TOutput>.Node emptyNode = ImmutableList<TOutput>.Node.EmptyNode;
                if (IsEmpty)
                {
                    return emptyNode;
                }
                return emptyNode.AddRange(this.Select<T, TOutput>(converter));
            }

            internal bool TrueForAll(Predicate<T> match)
            {
                Requires.NotNull(match, "match");
                using (Enumerator enumerator = GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        T current = enumerator.Current;
                        if (!match(current))
                        {
                            return false;
                        }
                    }
                }
                return true;
            }

            internal bool Exists(Predicate<T> match)
            {
                Requires.NotNull(match, "match");
                using (Enumerator enumerator = GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        T current = enumerator.Current;
                        if (match(current))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            internal T? Find(Predicate<T> match)
            {
                Requires.NotNull(match, "match");
                using (Enumerator enumerator = GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        T current = enumerator.Current;
                        if (match(current))
                        {
                            return current;
                        }
                    }
                }
                return default(T);
            }

            internal ImmutableList<T> FindAll(Predicate<T> match)
            {
                Requires.NotNull(match, "match");
                if (IsEmpty)
                {
                    return ImmutableList<T>.Empty;
                }
                List<T> list = null;
                using (Enumerator enumerator = GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        T current = enumerator.Current;
                        if (match(current))
                        {
                            if (list == null)
                            {
                                list = new List<T>();
                            }
                            list.Add(current);
                        }
                    }
                }
                if (list == null)
                {
                    return ImmutableList<T>.Empty;
                }
                return ImmutableList.CreateRange(list);
            }

            internal int FindIndex(Predicate<T> match)
            {
                Requires.NotNull(match, "match");
                return FindIndex(0, _count, match);
            }

            internal int FindIndex(int startIndex, Predicate<T> match)
            {
                Requires.NotNull(match, "match");
                Requires.Range(startIndex >= 0 && startIndex <= Count, "startIndex");
                return FindIndex(startIndex, Count - startIndex, match);
            }

            internal int FindIndex(int startIndex, int count, Predicate<T> match)
            {
                Requires.NotNull(match, "match");
                Requires.Range(startIndex >= 0, "startIndex");
                Requires.Range(count >= 0, "count");
                Requires.Range(startIndex + count <= Count, "count");
                using (Enumerator enumerator = new Enumerator(this, null, startIndex, count))
                {
                    int num = startIndex;
                    while (enumerator.MoveNext())
                    {
                        if (match(enumerator.Current))
                        {
                            return num;
                        }
                        num++;
                    }
                }
                return -1;
            }

            internal T? FindLast(Predicate<T> match)
            {
                Requires.NotNull(match, "match");
                using (Enumerator enumerator = new Enumerator(this, null, -1, -1, reversed: true))
                {
                    while (enumerator.MoveNext())
                    {
                        if (match(enumerator.Current))
                        {
                            return enumerator.Current;
                        }
                    }
                }
                return default(T);
            }

            internal int FindLastIndex(Predicate<T> match)
            {
                Requires.NotNull(match, "match");
                if (!IsEmpty)
                {
                    return FindLastIndex(Count - 1, Count, match);
                }
                return -1;
            }

            internal int FindLastIndex(int startIndex, Predicate<T> match)
            {
                Requires.NotNull(match, "match");
                Requires.Range(startIndex >= 0, "startIndex");
                Requires.Range(startIndex == 0 || startIndex < Count, "startIndex");
                if (!IsEmpty)
                {
                    return FindLastIndex(startIndex, startIndex + 1, match);
                }
                return -1;
            }

            internal int FindLastIndex(int startIndex, int count, Predicate<T> match)
            {
                Requires.NotNull(match, "match");
                Requires.Range(startIndex >= 0, "startIndex");
                Requires.Range(count <= Count, "count");
                Requires.Range(startIndex - count + 1 >= 0, "startIndex");
                using (Enumerator enumerator = new Enumerator(this, null, startIndex, count, reversed: true))
                {
                    int num = startIndex;
                    while (enumerator.MoveNext())
                    {
                        if (match(enumerator.Current))
                        {
                            return num;
                        }
                        num--;
                    }
                }
                return -1;
            }

            internal void Freeze()
            {
                if (!_frozen)
                {
                    _left.Freeze();
                    _right.Freeze();
                    _frozen = true;
                }
            }

            private Node RotateLeft()
            {
                return _right.MutateLeft(MutateRight(_right._left));
            }

            private Node RotateRight()
            {
                return _left.MutateRight(MutateLeft(_left._right));
            }

            private Node DoubleLeft()
            {
                Node right = _right;
                Node left = right._left;
                return left.MutateBoth(MutateRight(left._left), right.MutateLeft(left._right));
            }

            private Node DoubleRight()
            {
                Node left = _left;
                Node right = left._right;
                return right.MutateBoth(left.MutateRight(right._left), MutateLeft(right._right));
            }

            private Node Balance()
            {
                if (!IsLeftHeavy)
                {
                    return BalanceRight();
                }
                return BalanceLeft();
            }

            private Node BalanceLeft()
            {
                if (_left.BalanceFactor <= 0)
                {
                    return RotateRight();
                }
                return DoubleRight();
            }

            private Node BalanceRight()
            {
                if (_right.BalanceFactor >= 0)
                {
                    return RotateLeft();
                }
                return DoubleLeft();
            }

            private Node BalanceMany()
            {
                Node node = this;
                while (!node.IsBalanced)
                {
                    if (node.IsRightHeavy)
                    {
                        node = node.BalanceRight();
                        node.MutateLeft(node._left.BalanceMany());
                    }
                    else
                    {
                        node = node.BalanceLeft();
                        node.MutateRight(node._right.BalanceMany());
                    }
                }
                return node;
            }

            private Node MutateBoth(Node left, Node right)
            {
                Requires.NotNull(left, "left");
                Requires.NotNull(right, "right");
                if (_frozen)
                {
                    return new Node(_key, left, right);
                }
                _left = left;
                _right = right;
                _height = ParentHeight(left, right);
                _count = ParentCount(left, right);
                return this;
            }

            private Node MutateLeft(Node left)
            {
                Requires.NotNull(left, "left");
                if (_frozen)
                {
                    return new Node(_key, left, _right);
                }
                _left = left;
                _height = ParentHeight(left, _right);
                _count = ParentCount(left, _right);
                return this;
            }

            private Node MutateRight(Node right)
            {
                Requires.NotNull(right, "right");
                if (_frozen)
                {
                    return new Node(_key, _left, right);
                }
                _right = right;
                _height = ParentHeight(_left, right);
                _count = ParentCount(_left, right);
                return this;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static byte ParentHeight(Node left, Node right)
            {
                checked
                {
                    return (byte)(1 + unchecked((int)Math.Max(left._height, right._height)));
                }
            }

            private static int ParentCount(Node left, Node right)
            {
                return 1 + left._count + right._count;
            }

            private Node MutateKey(T key)
            {
                if (_frozen)
                {
                    return new Node(key, _left, _right);
                }
                _key = key;
                return this;
            }

            private static Node CreateRange(IEnumerable<T> keys)
            {
                if (ImmutableList<T>.TryCastToImmutableList(keys, out ImmutableList<T> other))
                {
                    return other._root;
                }
                IOrderedCollection<T> orderedCollection = keys.AsOrderedCollection();
                return NodeTreeFromList(orderedCollection, 0, orderedCollection.Count);
            }

            private static Node CreateLeaf(T key)
            {
                return new Node(key, EmptyNode, EmptyNode);
            }

            private static bool Contains(Node node, T value, IEqualityComparer<T> equalityComparer)
            {
                if (!node.IsEmpty)
                {
                    if (!equalityComparer.Equals(value, node._key) && !Contains(node._left, value, equalityComparer))
                    {
                        return Contains(node._right, value, equalityComparer);
                    }
                    return true;
                }
                return false;
            }
        }

        /// <summary>Gets an empty immutable list.</summary>
        public static readonly ImmutableList<T> Empty = new ImmutableList<T>();

        private readonly Node _root;

        /// <summary>Gets a value that indicates whether this list is empty.</summary>
        /// <returns>
        ///   <see langword="true" /> if the list is empty; otherwise, <see langword="false" />.</returns>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public bool IsEmpty => _root.IsEmpty;

        /// <summary>Gets the number of elements contained in the list.</summary>
        /// <returns>The number of elements in the list.</returns>
        public int Count => _root.Count;

        /// <summary>See <see cref="T:System.Collections.ICollection" />.</summary>
        /// <returns>Object used for synchronizing access to the collection.</returns>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        object ICollection.SyncRoot => this;

        /// <summary>This type is immutable, so it is always thread-safe. See the <see cref="T:System.Collections.ICollection" /> interface.</summary>
        /// <returns>Boolean value determining whether the collection is thread-safe.</returns>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool ICollection.IsSynchronized => true;

        /// <summary>Gets the element at the specified index of the list.</summary>
        /// <param name="index">The index of the element to retrieve.</param>
        /// <exception cref="System.IndexOutOfRangeException">In a get operation, <paramref name="index" /> is negative or not less than <see cref="System.Collections.Immutable.ImmutableList{T}.Count" />.</exception>
        /// <returns>The element at the specified index.</returns>
        public T this[int index] => _root.ItemRef(index);

        T IOrderedCollection<T>.this[int index] => this[index];

        /// <summary>Gets or sets the value at the specified index.</summary>
        /// <param name="index">The zero-based index of the item to access.</param>
        /// <exception cref="System.IndexOutOfRangeException">Thrown from getter when <paramref name="index" /> is negative or not less than <see cref="System.Collections.Immutable.ImmutableList{T}.Count" />.</exception>
        /// <exception cref="System.NotSupportedException">Always thrown from the setter.</exception>
        /// <returns>Value stored in the specified index.</returns>
        T IList<T>.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>Gets a value indicating whether the <see cref="System.Collections.Generic.ICollection{T}" /> is read-only.</summary>
        /// <returns>
        ///   <see langword="true" /> if the <see cref="System.Collections.Generic.ICollection{T}" /> is read-only; otherwise, <see langword="false" />.</returns>
        bool ICollection<T>.IsReadOnly => true;

        /// <summary>Gets a value indicating whether the <see cref="System.Collections.IList" /> has a fixed size.</summary>
        /// <returns>
        ///   <see langword="true" /> if the <see cref="System.Collections.IList" /> has a fixed size; otherwise, <see langword="false" />.</returns>
        bool IList.IsFixedSize => true;

        /// <summary>Gets a value indicating whether the <see cref="System.Collections.Generic.ICollection{T}" /> is read-only.</summary>
        /// <returns>
        ///   <see langword="true" /> if the <see cref="System.Collections.Generic.ICollection{T}" /> is read-only; otherwise, <see langword="false" />.</returns>
        bool IList.IsReadOnly => true;

        /// <summary>Gets or sets the <see cref="System.Object" /> at the specified index.</summary>
        /// <param name="index">The index.</param>
        /// <exception cref="System.IndexOutOfRangeException">Thrown from getter when <paramref name="index" /> is negative or not less than <see cref="P:System.Collections.Immutable.ImmutableList`1.Count" />.</exception>
        /// <exception cref="System.NotSupportedException">Always thrown from the setter.</exception>
        /// <returns>The value at the specified index.</returns>
        object? IList.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        internal Node Root => _root;

        internal ImmutableList()
        {
            _root = Node.EmptyNode;
        }

        private ImmutableList(Node root)
        {
            Requires.NotNull(root, "root");
            root.Freeze();
            _root = root;
        }

        /// <summary>Removes all elements from the immutable list.</summary>
        /// <returns>An empty list that retains the same sort or unordered semantics that this instance has.</returns>
        public ImmutableList<T> Clear()
        {
            return Empty;
        }

        /// <summary>Searches the entire sorted list for an element using the default comparer and returns the zero-based index of the element.</summary>
        /// <param name="item">The object to locate. The value can be <see langword="null" /> for reference types.</param>
        /// <exception cref="T:System.InvalidOperationException">The default comparer cannot find a comparer implementation of the for type T.</exception>
        /// <returns>The zero-based index of item in the sorted List, if item is found; otherwise, a negative number that is the bitwise complement of the index of the next element that is larger than item or, if there is no larger element, the bitwise complement of <see cref="P:System.Collections.ICollection.Count" />.</returns>
        public int BinarySearch(T item)
        {
            return BinarySearch(item, null);
        }

        /// <summary>Searches the entire sorted list for an element using the specified comparer and returns the zero-based index of the element.</summary>
        /// <param name="item">The object to locate. The value can be null for reference types.</param>
        /// <param name="comparer">The comparer implementation to use when comparing elements or null to use the default comparer.</param>
        /// <exception cref="T:System.InvalidOperationException">comparer is <see langword="null" />, and the default comparer cannot find an comparer implementation for type T.</exception>
        /// <returns>The zero-based index of item in the sorted List, if item is found; otherwise, a negative number that is the bitwise complement of the index of the next element that is larger than item or, if there is no larger element, the bitwise complement of <see cref="P:System.Collections.ICollection.Count" />.</returns>
        public int BinarySearch(T item, IComparer<T>? comparer)
        {
            return BinarySearch(0, Count, item, comparer);
        }

        /// <summary>Searches a range of elements in the sorted list for an element using the specified comparer and returns the zero-based index of the element.</summary>
        /// <param name="index">The zero-based starting index of the range to search.</param>
        /// <param name="count">The length of the range to search.</param>
        /// <param name="item">The object to locate. The value can be null for reference types.</param>
        /// <param name="comparer">The comparer implementation to use when comparing elements, or <see langword="null" /> to use the default comparer.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">index is less than 0 or <paramref name="count" /> is less than 0.</exception>
        /// <exception cref="T:System.ArgumentException">index and <paramref name="count" /> do not denote a valid range in the list.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        ///   <paramref name="comparer" /> is <see langword="null" />, and the default comparer cannot find an comparer implementation for type T.</exception>
        /// <returns>The zero-based index of item in the sorted list, if item is found; otherwise, a negative number that is the bitwise complement of the index of the next element that is larger than item or, if there is no larger element, the bitwise complement of <paramref name="count" />.</returns>
        public int BinarySearch(int index, int count, T item, IComparer<T>? comparer)
        {
            return _root.BinarySearch(index, count, item, comparer);
        }

        /// <summary>Retrieves an empty list that has the same sorting and ordering semantics as this instance.</summary>
        /// <returns>An empty list that has the same sorting and ordering semantics as this instance.</returns>
        IImmutableList<T> IImmutableList<T>.Clear()
        {
            return Clear();
        }

        /// <summary>Gets a read-only reference to the element of the set at the given <paramref name="index" />.</summary>
        /// <param name="index">The 0-based index of the element in the set to return.</param>
        /// <exception cref="System.IndexOutOfRangeException">
        ///  <paramref name="index" /> is negative or not less than <see cref="System.Collections.Immutable.ImmutableList{T}.Count" />.</exception>
        /// <returns>A read-only reference to the element at the given position.</returns>
        public ref readonly T ItemRef(int index)
        {
            return ref _root.ItemRef(index);
        }

        /// <summary>Creates a list that has the same contents as this list and can be efficiently mutated across multiple operations using standard mutable interfaces.</summary>
        /// <returns>The created list with the same contents as this list.</returns>
        public Builder ToBuilder()
        {
            return new Builder(this);
        }

        /// <summary>Adds the specified object to the end of the immutable list.</summary>
        /// <param name="value">The object to add.</param>
        /// <returns>A new immutable list with the object added.</returns>
        public ImmutableList<T> Add(T value)
        {
            Node root = _root.Add(value);
            return Wrap(root);
        }

        /// <summary>Adds the elements of the specified collection to the end of the immutable list.</summary>
        /// <param name="items">The collection whose elements will be added to the end of the list.</param>
        /// <returns>A new immutable list with the elements added.</returns>
        public ImmutableList<T> AddRange(IEnumerable<T> items)
        {
            Requires.NotNull(items, "items");
            if (IsEmpty)
            {
                return CreateRange(items);
            }
            Node root = _root.AddRange(items);
            return Wrap(root);
        }

        /// <summary>Inserts the specified object into the immutable list at the specified index.</summary>
        /// <param name="index">The zero-based index at which to insert the object.</param>
        /// <param name="item">The object to insert.</param>
        /// <returns>The new immutable list after the object is inserted.</returns>
        public ImmutableList<T> Insert(int index, T item)
        {
            Requires.Range(index >= 0 && index <= Count, "index");
            return Wrap(_root.Insert(index, item));
        }

        /// <summary>Inserts the elements of a collection into the immutable list at the specified index.</summary>
        /// <param name="index">The zero-based index at which to insert the elements.</param>
        /// <param name="items">The collection whose elements should be inserted.</param>
        /// <returns>The new immutable list after the elements are inserted.</returns>
        public ImmutableList<T> InsertRange(int index, IEnumerable<T> items)
        {
            Requires.Range(index >= 0 && index <= Count, "index");
            Requires.NotNull(items, "items");
            Node root = _root.InsertRange(index, items);
            return Wrap(root);
        }

        /// <summary>Removes the first occurrence of the specified object from this immutable list.</summary>
        /// <param name="value">The object to remove.</param>
        /// <returns>A new list with the object removed, or this list if the specified object is not in this list.</returns>
        public ImmutableList<T> Remove(T value)
        {
            return Remove(value, EqualityComparer<T>.Default);
        }

        /// <summary>Removes the first occurrence of the object that matches the specified value from this immutable list.</summary>
        /// <param name="value">The value of the element to remove from the list.</param>
        /// <param name="equalityComparer">The equality comparer to use in the search.</param>
        /// <returns>A new list with the object removed, or this list if the specified object is not in this list.</returns>
        public ImmutableList<T> Remove(T value, IEqualityComparer<T>? equalityComparer)
        {
            int num = this.IndexOf(value, equalityComparer);
            if (num >= 0)
            {
                return RemoveAt(num);
            }
            return this;
        }

        /// <summary>Removes a range of elements, starting from the specified index and containing the specified number of elements, from this immutable list.</summary>
        /// <param name="index">The starting index to begin removal.</param>
        /// <param name="count">The number of elements to remove.</param>
        /// <returns>A new list with the elements removed.</returns>
        public ImmutableList<T> RemoveRange(int index, int count)
        {
            Requires.Range(index >= 0 && index <= Count, "index");
            Requires.Range(count >= 0 && index + count <= Count, "count");
            Node node = _root;
            int num = count;
            while (num-- > 0)
            {
                node = node.RemoveAt(index);
            }
            return Wrap(node);
        }

        /// <summary>Removes a range of elements from this immutable list.</summary>
        /// <param name="items">The collection whose elements should be removed if matches are found in this list.</param>
        /// <returns>A new list with the elements removed.</returns>
        public ImmutableList<T> RemoveRange(IEnumerable<T> items)
        {
            return RemoveRange(items, EqualityComparer<T>.Default);
        }

        /// <summary>Removes the specified values from this list.</summary>
        /// <param name="items">The items to remove if matches are found in this list.</param>
        /// <param name="equalityComparer">The equality comparer to use in the search.</param>
        /// <returns>A new list with the elements removed.</returns>
        public ImmutableList<T> RemoveRange(IEnumerable<T> items, IEqualityComparer<T>? equalityComparer)
        {
            Requires.NotNull(items, "items");
            if (IsEmpty)
            {
                return this;
            }
            Node node = _root;
            foreach (T item in items.GetEnumerableDisposable<T, Enumerator>())
            {
                int num = node.IndexOf(item, equalityComparer);
                if (num >= 0)
                {
                    node = node.RemoveAt(num);
                }
            }
            return Wrap(node);
        }

        /// <summary>Removes the element at the specified index.</summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        /// <returns>A new list with the element removed.</returns>
        public ImmutableList<T> RemoveAt(int index)
        {
            Requires.Range(index >= 0 && index < Count, "index");
            Node root = _root.RemoveAt(index);
            return Wrap(root);
        }

        /// <summary>Removes all the elements that match the conditions defined by the specified predicate.</summary>
        /// <param name="match">The delegate that defines the conditions of the elements to remove.</param>
        /// <returns>The new list with the elements removed.</returns>
        public ImmutableList<T> RemoveAll(Predicate<T> match)
        {
            Requires.NotNull(match, "match");
            return Wrap(_root.RemoveAll(match));
        }

        /// <summary>Replaces an element at a given position in the immutable list with the specified element.</summary>
        /// <param name="index">The position in the list of the element to replace.</param>
        /// <param name="value">The element to replace the old element with.</param>
        /// <returns>The new list with the replaced element, even if it is equal to the old element at that position.</returns>
        public ImmutableList<T> SetItem(int index, T value)
        {
            return Wrap(_root.ReplaceAt(index, value));
        }

        /// <summary>Replaces the specified element in the immutable list with a new element.</summary>
        /// <param name="oldValue">The element to replace.</param>
        /// <param name="newValue">The element to replace <paramref name="oldValue" /> with.</param>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="oldValue" /> does not exist in the immutable list.</exception>
        /// <returns>The new list with the replaced element, even if it is equal to the old element.</returns>
        public ImmutableList<T> Replace(T oldValue, T newValue)
        {
            return Replace(oldValue, newValue, EqualityComparer<T>.Default);
        }

        /// <summary>Replaces the specified element in the immutable list with a new element.</summary>
        /// <param name="oldValue">The element to replace in the list.</param>
        /// <param name="newValue">The element to replace <paramref name="oldValue" /> with.</param>
        /// <param name="equalityComparer">The comparer to use to check for equality.</param>
        /// <returns>A new list with the object replaced, or this list if the specified object is not in this list.</returns>
        public ImmutableList<T> Replace(T oldValue, T newValue, IEqualityComparer<T>? equalityComparer)
        {
            int num = this.IndexOf(oldValue, equalityComparer);
            if (num < 0)
            {
                throw new ArgumentException(MDCFR.Properties.Resources.CannotFindOldValue, "oldValue");
            }
            return SetItem(num, newValue);
        }

        /// <summary>Reverses the order of the elements in the entire immutable list.</summary>
        /// <returns>The reversed list.</returns>
        public ImmutableList<T> Reverse()
        {
            return Wrap(_root.Reverse());
        }

        /// <summary>Reverses the order of the elements in the specified range of the immutable list.</summary>
        /// <param name="index">The zero-based starting index of the range to reverse.</param>
        /// <param name="count">The number of elements in the range to reverse.</param>
        /// <returns>The reversed list.</returns>
        public ImmutableList<T> Reverse(int index, int count)
        {
            return Wrap(_root.Reverse(index, count));
        }

        /// <summary>Sorts the elements in the entire immutable list using the default comparer.</summary>
        /// <returns>The sorted list.</returns>
        public ImmutableList<T> Sort()
        {
            return Wrap(_root.Sort());
        }

        /// <summary>Sorts the elements in the entire immutable list using the specified comparer.</summary>
        /// <param name="comparison">The delegate to use when comparing elements.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="comparison" /> is <see langword="null" />.</exception>
        /// <returns>The sorted list.</returns>
        public ImmutableList<T> Sort(Comparison<T> comparison)
        {
            Requires.NotNull(comparison, "comparison");
            return Wrap(_root.Sort(comparison));
        }

        /// <summary>Sorts the elements in the entire immutable list using the specified comparer.</summary>
        /// <param name="comparer">The implementation to use when comparing elements, or <see langword="null" /> to use the default comparer (<see cref="P:System.Collections.Generic.Comparer`1.Default" />).</param>
        /// <returns>The sorted list.</returns>
        public ImmutableList<T> Sort(IComparer<T>? comparer)
        {
            return Wrap(_root.Sort(comparer));
        }

        /// <summary>Sorts a range of elements in the immutable list using the specified comparer.</summary>
        /// <param name="index">The zero-based starting index of the range to sort.</param>
        /// <param name="count">The length of the range to sort.</param>
        /// <param name="comparer">The implementation to use when comparing elements, or <see langword="null" /> to use the default comparer (<see cref="P:System.Collections.Generic.Comparer`1.Default" />).</param>
        /// <returns>The sorted list.</returns>
        public ImmutableList<T> Sort(int index, int count, IComparer<T>? comparer)
        {
            Requires.Range(index >= 0, "index");
            Requires.Range(count >= 0, "count");
            Requires.Range(index + count <= Count, "count");
            return Wrap(_root.Sort(index, count, comparer));
        }

        /// <summary>Performs the specified action on each element of the immutable list.</summary>
        /// <param name="action">The delegate to perform on each element of the immutable list.</param>
        public void ForEach(Action<T> action)
        {
            Requires.NotNull(action, "action");
            using Enumerator enumerator = GetEnumerator();
            while (enumerator.MoveNext())
            {
                T current = enumerator.Current;
                action(current);
            }
        }

        /// <summary>Copies the entire immutable list to a compatible one-dimensional array, starting at the beginning of the target array.</summary>
        /// <param name="array">The one-dimensional array that is the destination of the elements copied from the immutable list. The array must have zero-based indexing.</param>
        public void CopyTo(T[] array)
        {
            _root.CopyTo(array);
        }

        /// <summary>Copies the entire immutable list to a compatible one-dimensional array, starting at the specified index of the target array.</summary>
        /// <param name="array">The one-dimensional array that is the destination of the elements copied from the immutable list. The array must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            _root.CopyTo(array, arrayIndex);
        }

        /// <summary>Copies a range of elements from the immutable list to a compatible one-dimensional array, starting at the specified index of the target array.</summary>
        /// <param name="index">The zero-based index in the source immutable list at which copying begins.</param>
        /// <param name="array">The one-dimensional array that is the destination of the elements copied from the immutable list. The array must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        /// <param name="count">The number of elements to copy.</param>
        public void CopyTo(int index, T[] array, int arrayIndex, int count)
        {
            _root.CopyTo(index, array, arrayIndex, count);
        }

        /// <summary>Creates a shallow copy of a range of elements in the source immutable list.</summary>
        /// <param name="index">The zero-based index at which the range starts.</param>
        /// <param name="count">The number of elements in the range.</param>
        /// <returns>A shallow copy of a range of elements in the source immutable list.</returns>
        public ImmutableList<T> GetRange(int index, int count)
        {
            Requires.Range(index >= 0, "index");
            Requires.Range(count >= 0, "count");
            Requires.Range(index + count <= Count, "count");
            return Wrap(Node.NodeTreeFromList(this, index, count));
        }

        /// <summary>Converts the elements in the current immutable list to another type, and returns a list containing the converted elements.</summary>
        /// <param name="converter">A delegate that converts each element from one type to another type.</param>
        /// <typeparam name="TOutput">The type of the elements of the target array.</typeparam>
        /// <returns>A list of the target type containing the converted elements from the current <see cref="T:System.Collections.Immutable.ImmutableList`1" />.</returns>
        public ImmutableList<TOutput> ConvertAll<TOutput>(Func<T, TOutput> converter)
        {
            Requires.NotNull(converter, "converter");
            return ImmutableList<TOutput>.WrapNode(_root.ConvertAll(converter));
        }

        /// <summary>Determines whether the immutable list contains elements that match the conditions defined by the specified predicate.</summary>
        /// <param name="match">The delegate that defines the conditions of the elements to search for.</param>
        /// <returns>
        ///   <see langword="true" /> if the immutable list contains one or more elements that match the conditions defined by the specified predicate; otherwise, <see langword="false" />.</returns>
        public bool Exists(Predicate<T> match)
        {
            return _root.Exists(match);
        }

        /// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the first occurrence within the entire immutable list.</summary>
        /// <param name="match">The delegate that defines the conditions of the element to search for.</param>
        /// <returns>The first element that matches the conditions defined by the specified predicate, if found; otherwise, the default value for type T.</returns>
        public T? Find(Predicate<T> match)
        {
            return _root.Find(match);
        }

        /// <summary>Retrieves all the elements that match the conditions defined by the specified predicate.</summary>
        /// <param name="match">The delegate that defines the conditions of the elements to search for.</param>
        /// <returns>An immutable list that contains all the elements that match the conditions defined by the specified predicate, if found; otherwise, an empty immutable list.</returns>
        public ImmutableList<T> FindAll(Predicate<T> match)
        {
            return _root.FindAll(match);
        }

        /// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the entire immutable list.</summary>
        /// <param name="match">The delegate that defines the conditions of the element to search for.</param>
        /// <returns>The zero-based index of the first occurrence of an element that matches the conditions defined by match, if found; otherwise, -1.</returns>
        public int FindIndex(Predicate<T> match)
        {
            return _root.FindIndex(match);
        }

        /// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the range of elements in the immutable list that extends from the specified index to the last element.</summary>
        /// <param name="startIndex">The zero-based starting index of the search.</param>
        /// <param name="match">The delegate that defines the conditions of the element to search for.</param>
        /// <returns>The zero-based index of the first occurrence of an element that matches the conditions defined by match, if found; otherwise, ?1.</returns>
        public int FindIndex(int startIndex, Predicate<T> match)
        {
            return _root.FindIndex(startIndex, match);
        }

        /// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the range of elements in the immutable list that starts at the specified index and contains the specified number of elements.</summary>
        /// <param name="startIndex">The zero-based starting index of the search.</param>
        /// <param name="count">The number of elements in the section to search.</param>
        /// <param name="match">The delegate that defines the conditions of the element to search for.</param>
        /// <returns>The zero-based index of the first occurrence of an element that matches the conditions defined by match, if found; otherwise, ?1.</returns>
        public int FindIndex(int startIndex, int count, Predicate<T> match)
        {
            return _root.FindIndex(startIndex, count, match);
        }

        /// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the last occurrence within the entire immutable list.</summary>
        /// <param name="match">The delegate that defines the conditions of the element to search for.</param>
        /// <returns>The last element that matches the conditions defined by the specified predicate, if found; otherwise, the default value for type T.</returns>
        public T? FindLast(Predicate<T> match)
        {
            return _root.FindLast(match);
        }

        /// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the entire immutable list.</summary>
        /// <param name="match">The delegate that defines the conditions of the element to search for.</param>
        /// <returns>The zero-based index of the last occurrence of an element that matches the conditions defined by <paramref name="match" />, if found; otherwise, ?1.</returns>
        public int FindLastIndex(Predicate<T> match)
        {
            return _root.FindLastIndex(match);
        }

        /// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the range of elements in the immutable list that extends from the first element to the specified index.</summary>
        /// <param name="startIndex">The zero-based starting index of the backward search.</param>
        /// <param name="match">The delegate that defines the conditions of the element to search for.</param>
        /// <returns>The zero-based index of the last occurrence of an element that matches the conditions defined by <paramref name="match" />, if found; otherwise, ?1.</returns>
        public int FindLastIndex(int startIndex, Predicate<T> match)
        {
            return _root.FindLastIndex(startIndex, match);
        }

        /// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the range of elements in the immutable list that contains the specified number of elements and ends at the specified index.</summary>
        /// <param name="startIndex">The zero-based starting index of the backward search.</param>
        /// <param name="count">The number of elements in the section to search.</param>
        /// <param name="match">The delegate that defines the conditions of the element to search for.</param>
        /// <returns>The zero-based index of the last occurrence of an element that matches the conditions defined by <paramref name="match" />, if found; otherwise, ?1.</returns>
        public int FindLastIndex(int startIndex, int count, Predicate<T> match)
        {
            return _root.FindLastIndex(startIndex, count, match);
        }

        /// <summary>Searches for the specified object and returns the zero-based index of the first occurrence within the range of elements in the list that starts at the specified index and contains the specified number of elements.</summary>
        /// <param name="item">The object to locate in the list The value can be null for reference types.</param>
        /// <param name="index">The zero-based starting index of the search. 0 (zero) is valid in an empty list.</param>
        /// <param name="count">The number of elements in the section to search.</param>
        /// <param name="equalityComparer">The equality comparer to use in the search.</param>
        /// <returns>The zero-based index of the first occurrence of item within the range of elements in the list that starts at index and contains count number of elements, if found; otherwise, -1.</returns>
        public int IndexOf(T item, int index, int count, IEqualityComparer<T>? equalityComparer)
        {
            return _root.IndexOf(item, index, count, equalityComparer);
        }

        /// <summary>Searches for the specified object and returns the zero-based index of the last occurrence within the range of elements in the list that contains the specified number of elements and ends at the specified index.</summary>
        /// <param name="item">The object to locate in the list. The value can be null for reference types.</param>
        /// <param name="index">The zero-based starting index of the backward search.</param>
        /// <param name="count">The number of elements in the section to search.</param>
        /// <param name="equalityComparer">The equality comparer to use in the search.</param>
        /// <returns>The zero-based index of the last occurrence of item within the range of elements in the list that contains count number of elements and ends at index, if found; otherwise, -1.</returns>
        public int LastIndexOf(T item, int index, int count, IEqualityComparer<T>? equalityComparer)
        {
            return _root.LastIndexOf(item, index, count, equalityComparer);
        }

        /// <summary>Determines whether every element in the immutable list matches the conditions defined by the specified predicate.</summary>
        /// <param name="match">The delegate that defines the conditions to check against the elements.</param>
        /// <returns>
        ///   <see langword="true" /> if every element in the immutable list matches the conditions defined by the specified predicate; otherwise, <see langword="false" />. If the list has no elements, the return value is <see langword="true" />.</returns>
        public bool TrueForAll(Predicate<T> match)
        {
            return _root.TrueForAll(match);
        }

        /// <summary>Determines whether this immutable list contains the specified value.</summary>
        /// <param name="value">The value to locate.</param>
        /// <returns>
        ///   <see langword="true" /> if the list contains the specified value; otherwise, <see langword="false" />.</returns>
        public bool Contains(T value)
        {
            return _root.Contains(value, EqualityComparer<T>.Default);
        }

        /// <summary>Searches for the specified object and returns the zero-based index of the first occurrence within the entire immutable list.</summary>
        /// <param name="value">The object to locate in the immutable list. The value can be <see langword="null" /> for reference types.</param>
        /// <returns>The zero-based index of the first occurrence of <paramref name="value" /> within the entire immutable list, if found; otherwise, ?1.</returns>
        public int IndexOf(T value)
        {
            return this.IndexOf(value, EqualityComparer<T>.Default);
        }

        /// <summary>Adds the specified value to this immutable list.</summary>
        /// <param name="value">The value to add.</param>
        /// <returns>A new list with the element added.</returns>
        IImmutableList<T> IImmutableList<T>.Add(T value)
        {
            return Add(value);
        }

        /// <summary>Adds the specified values to this immutable list.</summary>
        /// <param name="items">The values to add.</param>
        /// <returns>A new list with the elements added.</returns>
        IImmutableList<T> IImmutableList<T>.AddRange(IEnumerable<T> items)
        {
            return AddRange(items);
        }

        /// <summary>Inserts the specified element at the specified index in the immutable list.</summary>
        /// <param name="index">The index at which to insert the value.</param>
        /// <param name="item">The element to insert.</param>
        /// <returns>A new immutable list that includes the specified element.</returns>
        IImmutableList<T> IImmutableList<T>.Insert(int index, T item)
        {
            return Insert(index, item);
        }

        /// <summary>Inserts the specified elements at the specified index in the immutable list.</summary>
        /// <param name="index">The index at which to insert the elements.</param>
        /// <param name="items">The elements to insert.</param>
        /// <returns>A new immutable list that includes the specified elements.</returns>
        IImmutableList<T> IImmutableList<T>.InsertRange(int index, IEnumerable<T> items)
        {
            return InsertRange(index, items);
        }

        /// <summary>Removes the element with the specified value from the list.</summary>
        /// <param name="value">The value of the element to remove from the list.</param>
        /// <param name="equalityComparer">The comparer to use to compare elements for equality.</param>
        /// <returns>A new <see cref="T:System.Collections.Immutable.ImmutableList`1" /> with the specified element removed.</returns>
        IImmutableList<T> IImmutableList<T>.Remove(T value, IEqualityComparer<T> equalityComparer)
        {
            return Remove(value, equalityComparer);
        }

        /// <summary>Removes all the elements that match the conditions defined by the specified predicate.</summary>
        /// <param name="match">The delegate that defines the conditions of the elements to remove.</param>
        /// <returns>A new immutable list with the elements removed.</returns>
        IImmutableList<T> IImmutableList<T>.RemoveAll(Predicate<T> match)
        {
            return RemoveAll(match);
        }

        /// <summary>Removes a range of elements from this immutable list that match the items specified.</summary>
        /// <param name="items">The range of items to remove from the list, if found.</param>
        /// <param name="equalityComparer">The equality comparer to use to compare elements.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="items" /> or <paramref name="equalityComparer" /> is <see langword="null" />.</exception>
        /// <returns>An immutable list with the items removed.</returns>
        IImmutableList<T> IImmutableList<T>.RemoveRange(IEnumerable<T> items, IEqualityComparer<T> equalityComparer)
        {
            return RemoveRange(items, equalityComparer);
        }

        /// <summary>Removes the specified number of elements at the specified location from this list.</summary>
        /// <param name="index">The starting index of the range of elements to remove.</param>
        /// <param name="count">The number of elements to remove.</param>
        /// <returns>A new list with the elements removed.</returns>
        IImmutableList<T> IImmutableList<T>.RemoveRange(int index, int count)
        {
            return RemoveRange(index, count);
        }

        /// <summary>Removes the element at the specified index of the immutable list.</summary>
        /// <param name="index">The index of the element to remove.</param>
        /// <returns>A new list with the element removed.</returns>
        IImmutableList<T> IImmutableList<T>.RemoveAt(int index)
        {
            return RemoveAt(index);
        }

        /// <summary>Replaces an element in the list at a given position with the specified element.</summary>
        /// <param name="index">The position in the list of the element to replace.</param>
        /// <param name="value">The element to replace the old element with.</param>
        /// <returns>The new list.</returns>
        IImmutableList<T> IImmutableList<T>.SetItem(int index, T value)
        {
            return SetItem(index, value);
        }

        /// <summary>Replaces an element in the list with the specified element.</summary>
        /// <param name="oldValue">The element to replace.</param>
        /// <param name="newValue">The element to replace the old element with.</param>
        /// <param name="equalityComparer">The equality comparer to use in the search.</param>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="oldValue" /> does not exist in the list.</exception>
        /// <returns>The new list.</returns>
        IImmutableList<T> IImmutableList<T>.Replace(T oldValue, T newValue, IEqualityComparer<T> equalityComparer)
        {
            return Replace(oldValue, newValue, equalityComparer);
        }

        /// <summary>Returns an enumerator that iterates through the immutable list.</summary>
        /// <returns>An enumerator that can be used to iterate through the list.</returns>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            if (!IsEmpty)
            {
                return GetEnumerator();
            }
            return Enumerable.Empty<T>().GetEnumerator();
        }

        /// <summary>Returns an enumerator that iterates through the immutable list.</summary>
        /// <returns>An enumerator that can be used to iterate through the list.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>Inserts an object in the immutable list at the specified index.</summary>
        /// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
        /// <param name="item">The object to insert.</param>
        /// <exception cref="T:System.NotSupportedException" />
        void IList<T>.Insert(int index, T item)
        {
            throw new NotSupportedException();
        }

        /// <summary>Removes the value at the specified index.</summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <exception cref="T:System.NotSupportedException" />
        void IList<T>.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        /// <summary>Adds the specified item to the immutable list.</summary>
        /// <param name="item">The item to add.</param>
        /// <exception cref="T:System.NotSupportedException">Always thrown.</exception>
        void ICollection<T>.Add(T item)
        {
            throw new NotSupportedException();
        }

        /// <summary>Removes all items from the immutable list.</summary>
        /// <exception cref="T:System.NotSupportedException" />
        void ICollection<T>.Clear()
        {
            throw new NotSupportedException();
        }

        /// <summary>Removes the first occurrence of a specific object from the immutable list.</summary>
        /// <param name="item">The object to remove.</param>
        /// <exception cref="T:System.NotSupportedException">Always thrown.</exception>
        /// <returns>
        ///   <see langword="true" /> if <paramref name="item" /> was successfully removed from the list; otherwise, <see langword="false" />. This method also returns <see langword="false" /> if <paramref name="item" /> is not found in the original list.</returns>
        bool ICollection<T>.Remove(T item)
        {
            throw new NotSupportedException();
        }

        /// <summary>Copies the entire immutable list to a compatible one-dimensional array, starting at the specified array index.</summary>
        /// <param name="array">The one-dimensional array that is the destination of the elements copied from immutable list.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        void ICollection.CopyTo(Array array, int arrayIndex)
        {
            _root.CopyTo(array, arrayIndex);
        }

        /// <summary>Adds an item to the immutable list.</summary>
        /// <param name="value">The object to add to the list.</param>
        /// <exception cref="T:System.NotSupportedException">Always thrown.</exception>
        /// <returns>The position into which the new element was inserted, or -1 to indicate that the item was not inserted into the list.</returns>
        int IList.Add(object value)
        {
            throw new NotSupportedException();
        }

        /// <summary>Removes the item at the specified index of the immutable list.</summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <exception cref="T:System.NotSupportedException">Always thrown.</exception>
        void IList.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        /// <summary>Removes all items from the immutable list.</summary>
        /// <exception cref="T:System.NotSupportedException">Always thrown.</exception>
        void IList.Clear()
        {
            throw new NotSupportedException();
        }

        /// <summary>Determines whether the immutable list contains a specific value.</summary>
        /// <param name="value">The object to locate in the list.</param>
        /// <exception cref="T:System.NotImplementedException" />
        /// <returns>
        ///   <see langword="true" /> if the object is found in the list; otherwise, <see langword="false" />.</returns>
        bool IList.Contains(object value)
        {
            if (IsCompatibleObject(value))
            {
                return Contains((T)value);
            }
            return false;
        }

        /// <summary>Determines the index of a specific item in the immutable list.</summary>
        /// <param name="value">The object to locate in the list.</param>
        /// <exception cref="T:System.NotImplementedException" />
        /// <returns>The index of <paramref name="value" /> if found in the list; otherwise, -1.</returns>
        int IList.IndexOf(object value)
        {
            if (!IsCompatibleObject(value))
            {
                return -1;
            }
            return IndexOf((T)value);
        }

        /// <summary>Inserts an item into the immutable list at the specified index.</summary>
        /// <param name="index">The zero-based index at which <paramref name="value" /> should be inserted.</param>
        /// <param name="value">The object to insert into the list.</param>
        /// <exception cref="T:System.NotSupportedException">Always thrown.</exception>
        void IList.Insert(int index, object value)
        {
            throw new NotSupportedException();
        }

        /// <summary>Removes the first occurrence of a specific object from the immutable list.</summary>
        /// <param name="value">The object to remove from the list.</param>
        /// <exception cref="T:System.NotSupportedException">Always thrown.</exception>
        void IList.Remove(object value)
        {
            throw new NotSupportedException();
        }

        /// <summary>Returns an enumerator that iterates through the immutable list.</summary>
        /// <returns>An enumerator  that can be used to iterate through the immutable list.</returns>
        public Enumerator GetEnumerator()
        {
            return new Enumerator(_root);
        }

        private static ImmutableList<T> WrapNode(Node root)
        {
            if (!root.IsEmpty)
            {
                return new ImmutableList<T>(root);
            }
            return Empty;
        }

        private static bool TryCastToImmutableList(IEnumerable<T> sequence, [NotNullWhen(true)] out ImmutableList<T> other)
        {
            other = sequence as ImmutableList<T>;
            if (other != null)
            {
                return true;
            }
            if (sequence is Builder builder)
            {
                other = builder.ToImmutable();
                return true;
            }
            return false;
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

        private ImmutableList<T> Wrap(Node root)
        {
            if (root != _root)
            {
                if (!root.IsEmpty)
                {
                    return new ImmutableList<T>(root);
                }
                return Clear();
            }
            return this;
        }

        private static ImmutableList<T> CreateRange(IEnumerable<T> items)
        {
            if (TryCastToImmutableList(items, out var other))
            {
                return other;
            }
            IOrderedCollection<T> orderedCollection = items.AsOrderedCollection();
            if (orderedCollection.Count == 0)
            {
                return Empty;
            }
            Node root = Node.NodeTreeFromList(orderedCollection, 0, orderedCollection.Count);
            return new ImmutableList<T>(root);
        }
    }

    internal sealed class ImmutableListBuilderDebuggerProxy<T>
    {
        private readonly ImmutableList<T>.Builder _list;

        private T[] _cachedContents;

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Contents => _cachedContents ?? (_cachedContents = _list.ToArray(_list.Count));

        public ImmutableListBuilderDebuggerProxy(ImmutableList<T>.Builder builder)
        {
            Requires.NotNull(builder, "builder");
            _list = builder;
        }
    }

    /// <summary>Provides a set of initialization methods for instances of the <see cref="System.Collections.Immutable.ImmutableQueue{T}" /> class.  </summary>
    public static class ImmutableQueue
    {
        /// <summary>Creates an empty immutable queue.</summary>
        /// <typeparam name="T">The type of items to be stored in the immutable queue.</typeparam>
        /// <returns>An empty immutable queue.</returns>
        public static ImmutableQueue<T> Create<T>()
        {
            return ImmutableQueue<T>.Empty;
        }

        /// <summary>Creates a new immutable queue that contains the specified item.</summary>
        /// <param name="item">The item to prepopulate the queue with.</param>
        /// <typeparam name="T">The type of items in the immutable queue.</typeparam>
        /// <returns>A new immutable queue that contains the specified item.</returns>
        public static ImmutableQueue<T> Create<T>(T item)
        {
            return ImmutableQueue<T>.Empty.Enqueue(item);
        }

        /// <summary>Creates a new immutable queue that contains the specified items.</summary>
        /// <param name="items">The items to add to the queue before immutability is applied.</param>
        /// <typeparam name="T">The type of elements in the queue.</typeparam>
        /// <returns>An immutable queue that contains the specified items.</returns>
        public static ImmutableQueue<T> CreateRange<T>(IEnumerable<T> items)
        {
            Requires.NotNull(items, "items");
            if (items is T[] items2)
            {
                return Create(items2);
            }
            using IEnumerator<T> enumerator = items.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                return ImmutableQueue<T>.Empty;
            }
            ImmutableStack<T> forwards = ImmutableStack.Create(enumerator.Current);
            ImmutableStack<T> immutableStack = ImmutableStack<T>.Empty;
            while (enumerator.MoveNext())
            {
                immutableStack = immutableStack.Push(enumerator.Current);
            }
            return new ImmutableQueue<T>(forwards, immutableStack);
        }

        /// <summary>Creates a new immutable queue that contains the specified array of items.</summary>
        /// <param name="items">An array that contains the items to prepopulate the queue with.</param>
        /// <typeparam name="T">The type of items in the immutable queue.</typeparam>
        /// <returns>A new immutable queue that contains the specified items.</returns>
        public static ImmutableQueue<T> Create<T>(params T[] items)
        {
            Requires.NotNull(items, "items");
            if (items.Length == 0)
            {
                return ImmutableQueue<T>.Empty;
            }
            ImmutableStack<T> immutableStack = ImmutableStack<T>.Empty;
            for (int num = items.Length - 1; num >= 0; num--)
            {
                immutableStack = immutableStack.Push(items[num]);
            }
            return new ImmutableQueue<T>(immutableStack, ImmutableStack<T>.Empty);
        }

        /// <summary>Removes the item at the beginning of the immutable queue, and returns the new queue.</summary>
        /// <param name="queue">The queue to remove the item from.</param>
        /// <param name="value">When this method returns, contains the item from the beginning of the queue.</param>
        /// <typeparam name="T">The type of elements in the immutable queue.</typeparam>
        /// <exception cref="T:System.InvalidOperationException">The stack is empty.</exception>
        /// <returns>The new queue with the item removed.</returns>
        public static IImmutableQueue<T> Dequeue<T>(this IImmutableQueue<T> queue, out T value)
        {
            Requires.NotNull(queue, "queue");
            value = queue.Peek();
            return queue.Dequeue();
        }
    }
    
    /// <summary>Represents an immutable queue. </summary>
    /// <typeparam name="T">The type of elements in the queue.</typeparam>
    [DebuggerDisplay("IsEmpty = {IsEmpty}")]
    [DebuggerTypeProxy(typeof(ImmutableEnumerableDebuggerProxy<>))]
    public sealed class ImmutableQueue<T> : IImmutableQueue<T>, IEnumerable<T>, IEnumerable
    {
        /// <summary>Enumerates the contents of an immutable queue without allocating any memory.  </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public struct Enumerator
        {
            private readonly ImmutableQueue<T> _originalQueue;

            private ImmutableStack<T> _remainingForwardsStack;

            private ImmutableStack<T> _remainingBackwardsStack;

            /// <summary>Gets the element at the current position of the enumerator.</summary>
            /// <returns>The element at the current position of the enumerator.</returns>
            public T Current
            {
                get
                {
                    if (_remainingForwardsStack == null)
                    {
                        throw new InvalidOperationException();
                    }
                    if (!_remainingForwardsStack.IsEmpty)
                    {
                        return _remainingForwardsStack.Peek();
                    }
                    if (!_remainingBackwardsStack.IsEmpty)
                    {
                        return _remainingBackwardsStack.Peek();
                    }
                    throw new InvalidOperationException();
                }
            }

            internal Enumerator(ImmutableQueue<T> queue)
            {
                _originalQueue = queue;
                _remainingForwardsStack = null;
                _remainingBackwardsStack = null;
            }

            /// <summary>Advances the enumerator to the next element of the immutable queue.</summary>
            /// <returns>
            ///   <see langword="true" /> if the enumerator was successfully advanced to the next element; <see langword="false" /> if the enumerator has passed the end of the queue.</returns>
            public bool MoveNext()
            {
                if (_remainingForwardsStack == null)
                {
                    _remainingForwardsStack = _originalQueue._forwards;
                    _remainingBackwardsStack = _originalQueue.BackwardsReversed;
                }
                else if (!_remainingForwardsStack.IsEmpty)
                {
                    _remainingForwardsStack = _remainingForwardsStack.Pop();
                }
                else if (!_remainingBackwardsStack.IsEmpty)
                {
                    _remainingBackwardsStack = _remainingBackwardsStack.Pop();
                }
                if (_remainingForwardsStack.IsEmpty)
                {
                    return !_remainingBackwardsStack.IsEmpty;
                }
                return true;
            }
        }

        private sealed class EnumeratorObject : IEnumerator<T>, IDisposable, IEnumerator
        {
            private readonly ImmutableQueue<T> _originalQueue;

            private ImmutableStack<T> _remainingForwardsStack;

            private ImmutableStack<T> _remainingBackwardsStack;

            private bool _disposed;

            public T Current
            {
                get
                {
                    ThrowIfDisposed();
                    if (_remainingForwardsStack == null)
                    {
                        throw new InvalidOperationException();
                    }
                    if (!_remainingForwardsStack.IsEmpty)
                    {
                        return _remainingForwardsStack.Peek();
                    }
                    if (!_remainingBackwardsStack.IsEmpty)
                    {
                        return _remainingBackwardsStack.Peek();
                    }
                    throw new InvalidOperationException();
                }
            }

            object IEnumerator.Current => Current;

            internal EnumeratorObject(ImmutableQueue<T> queue)
            {
                _originalQueue = queue;
            }

            public bool MoveNext()
            {
                ThrowIfDisposed();
                if (_remainingForwardsStack == null)
                {
                    _remainingForwardsStack = _originalQueue._forwards;
                    _remainingBackwardsStack = _originalQueue.BackwardsReversed;
                }
                else if (!_remainingForwardsStack.IsEmpty)
                {
                    _remainingForwardsStack = _remainingForwardsStack.Pop();
                }
                else if (!_remainingBackwardsStack.IsEmpty)
                {
                    _remainingBackwardsStack = _remainingBackwardsStack.Pop();
                }
                if (_remainingForwardsStack.IsEmpty)
                {
                    return !_remainingBackwardsStack.IsEmpty;
                }
                return true;
            }

            public void Reset()
            {
                ThrowIfDisposed();
                _remainingBackwardsStack = null;
                _remainingForwardsStack = null;
            }

            public void Dispose()
            {
                _disposed = true;
            }

            private void ThrowIfDisposed()
            {
                if (_disposed)
                {
                    Requires.FailObjectDisposed(this);
                }
            }
        }

        private static readonly ImmutableQueue<T> s_EmptyField = new ImmutableQueue<T>(ImmutableStack<T>.Empty, ImmutableStack<T>.Empty);

        private readonly ImmutableStack<T> _backwards;

        private readonly ImmutableStack<T> _forwards;

        private ImmutableStack<T> _backwardsReversed;

        /// <summary>Gets a value that indicates whether this immutable queue is empty.  
        ///
        ///  NuGet package: System.Collections.Immutable (about immutable collections and how to install)</summary>
        /// <returns>
        ///   <see langword="true" /> if this queue is empty; otherwise, <see langword="false" />.</returns>
        public bool IsEmpty => _forwards.IsEmpty;

        /// <summary>Gets an empty immutable queue.</summary>
        /// <returns>An empty immutable queue.</returns>
        public static ImmutableQueue<T> Empty => s_EmptyField;

        private ImmutableStack<T> BackwardsReversed
        {
            get
            {
                if (_backwardsReversed == null)
                {
                    _backwardsReversed = _backwards.Reverse();
                }
                return _backwardsReversed;
            }
        }

        internal ImmutableQueue(ImmutableStack<T> forwards, ImmutableStack<T> backwards)
        {
            _forwards = forwards;
            _backwards = backwards;
        }

        /// <summary>Removes all objects from the immutable queue.</summary>
        /// <returns>The empty immutable queue.</returns>
        public ImmutableQueue<T> Clear()
        {
            return Empty;
        }

        /// <summary>Removes all elements from the immutable queue.</summary>
        /// <returns>The empty immutable queue.</returns>
        IImmutableQueue<T> IImmutableQueue<T>.Clear()
        {
            return Clear();
        }

        /// <summary>Returns the element at the beginning of the immutable queue without removing it.</summary>
        /// <exception cref="T:System.InvalidOperationException">The queue is empty.</exception>
        /// <returns>The element at the beginning of the queue.</returns>
        public T Peek()
        {
            if (IsEmpty)
            {
                throw new InvalidOperationException(MDCFR.Properties.Resources.InvalidEmptyOperation);
            }
            return _forwards.Peek();
        }

        /// <summary>Gets a read-only reference to the element at the front of the queue.</summary>
        /// <exception cref="T:System.InvalidOperationException">The queue is empty.</exception>
        /// <returns>Read-only reference to the element at the front of the queue.</returns>
        public ref readonly T PeekRef()
        {
            if (IsEmpty)
            {
                throw new InvalidOperationException(MDCFR.Properties.Resources.InvalidEmptyOperation);
            }
            return ref _forwards.PeekRef();
        }

        /// <summary>Adds an element to the end of the immutable queue, and returns the new queue.</summary>
        /// <param name="value">The element to add.</param>
        /// <returns>The new immutable queue.</returns>
        public ImmutableQueue<T> Enqueue(T value)
        {
            if (IsEmpty)
            {
                return new ImmutableQueue<T>(ImmutableStack.Create(value), ImmutableStack<T>.Empty);
            }
            return new ImmutableQueue<T>(_forwards, _backwards.Push(value));
        }

        /// <summary>Adds an element to the end of the immutable queue, and returns the new queue.</summary>
        /// <param name="value">The element to add.</param>
        /// <returns>The new immutable queue.</returns>
        IImmutableQueue<T> IImmutableQueue<T>.Enqueue(T value)
        {
            return Enqueue(value);
        }

        /// <summary>Removes the element at the beginning of the immutable queue, and returns the new queue.</summary>
        /// <exception cref="T:System.InvalidOperationException">The queue is empty.</exception>
        /// <returns>The new immutable queue; never <see langword="null" />.</returns>
        public ImmutableQueue<T> Dequeue()
        {
            if (IsEmpty)
            {
                throw new InvalidOperationException(MDCFR.Properties.Resources.InvalidEmptyOperation);
            }
            ImmutableStack<T> immutableStack = _forwards.Pop();
            if (!immutableStack.IsEmpty)
            {
                return new ImmutableQueue<T>(immutableStack, _backwards);
            }
            if (_backwards.IsEmpty)
            {
                return Empty;
            }
            return new ImmutableQueue<T>(BackwardsReversed, ImmutableStack<T>.Empty);
        }

        /// <summary>Removes the item at the beginning of the immutable queue, and returns the new queue.</summary>
        /// <param name="value">When this method returns, contains the element from the beginning of the queue.</param>
        /// <exception cref="T:System.InvalidOperationException">The queue is empty.</exception>
        /// <returns>The new immutable queue with the beginning element removed.</returns>
        public ImmutableQueue<T> Dequeue(out T value)
        {
            value = Peek();
            return Dequeue();
        }

        /// <summary>Removes the element at the beginning of the immutable queue, and returns the new queue.</summary>
        /// <exception cref="T:System.InvalidOperationException">The queue is empty.</exception>
        /// <returns>The new immutable queue; never <see langword="null" />.</returns>
        IImmutableQueue<T> IImmutableQueue<T>.Dequeue()
        {
            return Dequeue();
        }

        /// <summary>Returns an enumerator that iterates through the immutable queue.</summary>
        /// <returns>An enumerator that can be used to iterate through the queue.</returns>
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator  that can be used to iterate through the collection.</returns>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            if (!IsEmpty)
            {
                return new EnumeratorObject(this);
            }
            return Enumerable.Empty<T>().GetEnumerator();
        }

        /// <summary>Returns an enumerator that iterates through a collection.</summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new EnumeratorObject(this);
        }
    }


    /// <summary>Provides a set of initialization methods for instances of the <see cref="T:System.Collections.Immutable.ImmutableSortedDictionary`2" /> class.</summary>
    public static class ImmutableSortedDictionary
    {
        /// <summary>Creates an empty immutable sorted dictionary.</summary>
        /// <typeparam name="TKey">The type of keys stored by the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of values stored by the dictionary.</typeparam>
        /// <returns>An empty immutable sorted dictionary.</returns>
        public static ImmutableSortedDictionary<TKey, TValue> Create<TKey, TValue>() where TKey : notnull
        {
            return ImmutableSortedDictionary<TKey, TValue>.Empty;
        }

        /// <summary>Creates an empty immutable sorted dictionary that uses the specified key comparer.</summary>
        /// <param name="keyComparer">The implementation to use to determine the equality of keys in the dictionary.</param>
        /// <typeparam name="TKey">The type of keys stored by the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of values stored by the dictionary.</typeparam>
        /// <returns>An empty immutable sorted dictionary.</returns>
        public static ImmutableSortedDictionary<TKey, TValue> Create<TKey, TValue>(IComparer<TKey>? keyComparer) where TKey : notnull
        {
            return ImmutableSortedDictionary<TKey, TValue>.Empty.WithComparers(keyComparer);
        }

        /// <summary>Creates an empty immutable sorted dictionary that uses the specified key and value comparers.</summary>
        /// <param name="keyComparer">The implementation to use to determine the equality of keys in the dictionary.</param>
        /// <param name="valueComparer">The implementation to use to determine the equality of values in the dictionary.</param>
        /// <typeparam name="TKey">The type of keys stored by the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of values stored by the dictionary.</typeparam>
        /// <returns>An empty immutable sorted dictionary.</returns>
        public static ImmutableSortedDictionary<TKey, TValue> Create<TKey, TValue>(IComparer<TKey>? keyComparer, IEqualityComparer<TValue>? valueComparer) where TKey : notnull
        {
            return ImmutableSortedDictionary<TKey, TValue>.Empty.WithComparers(keyComparer, valueComparer);
        }

        /// <summary>Creates an immutable sorted dictionary that contains the specified items and uses the default comparer.</summary>
        /// <param name="items">The items to add to the sorted dictionary before it's immutable.</param>
        /// <typeparam name="TKey">The type of keys stored in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of values stored in the dictionary.</typeparam>
        /// <returns>An immutable sorted dictionary that contains the specified items.</returns>
        public static ImmutableSortedDictionary<TKey, TValue> CreateRange<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> items) where TKey : notnull
        {
            return ImmutableSortedDictionary<TKey, TValue>.Empty.AddRange(items);
        }

        /// <summary>Creates a new immutable sorted dictionary from the specified range of items with the specified key comparer.</summary>
        /// <param name="keyComparer">The comparer implementation to use to evaluate keys for equality and sorting.</param>
        /// <param name="items">The items to add to the sorted dictionary.</param>
        /// <typeparam name="TKey">The type of keys stored in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of values stored in the dictionary.</typeparam>
        /// <returns>The new immutable sorted dictionary that contains the specified items and uses the specified key comparer.</returns>
        public static ImmutableSortedDictionary<TKey, TValue> CreateRange<TKey, TValue>(IComparer<TKey>? keyComparer, IEnumerable<KeyValuePair<TKey, TValue>> items) where TKey : notnull
        {
            return ImmutableSortedDictionary<TKey, TValue>.Empty.WithComparers(keyComparer).AddRange(items);
        }

        /// <summary>Creates a new immutable sorted dictionary from the specified range of items with the specified key and value comparers.</summary>
        /// <param name="keyComparer">The comparer implementation to use to compare keys for equality and sorting.</param>
        /// <param name="valueComparer">The comparer implementation to use to compare values for equality.</param>
        /// <param name="items">The items to add to the sorted dictionary before it's immutable.</param>
        /// <typeparam name="TKey">The type of keys stored in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of values stored in the dictionary.</typeparam>
        /// <returns>An immutable sorted dictionary that contains the specified items and uses the specified comparers.</returns>
        public static ImmutableSortedDictionary<TKey, TValue> CreateRange<TKey, TValue>(IComparer<TKey>? keyComparer, IEqualityComparer<TValue>? valueComparer, IEnumerable<KeyValuePair<TKey, TValue>> items) where TKey : notnull
        {
            return ImmutableSortedDictionary<TKey, TValue>.Empty.WithComparers(keyComparer, valueComparer).AddRange(items);
        }

        /// <summary>Creates a new immutable sorted dictionary builder.</summary>
        /// <typeparam name="TKey">The type of keys stored by the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of values stored by the dictionary.</typeparam>
        /// <returns>The immutable collection builder.</returns>
        public static ImmutableSortedDictionary<TKey, TValue>.Builder CreateBuilder<TKey, TValue>() where TKey : notnull
        {
            return Create<TKey, TValue>().ToBuilder();
        }

        /// <summary>Creates a new immutable sorted dictionary builder.</summary>
        /// <param name="keyComparer">The key comparer.</param>
        /// <typeparam name="TKey">The type of keys stored by the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of values stored by the dictionary.</typeparam>
        /// <returns>The immutable collection builder.</returns>
        public static ImmutableSortedDictionary<TKey, TValue>.Builder CreateBuilder<TKey, TValue>(IComparer<TKey>? keyComparer) where TKey : notnull
        {
            return Create<TKey, TValue>(keyComparer).ToBuilder();
        }

        /// <summary>Creates a new immutable sorted dictionary builder.</summary>
        /// <param name="keyComparer">The key comparer.</param>
        /// <param name="valueComparer">The value comparer.</param>
        /// <typeparam name="TKey">The type of keys stored by the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of values stored by the dictionary.</typeparam>
        /// <returns>The immutable collection builder.</returns>
        public static ImmutableSortedDictionary<TKey, TValue>.Builder CreateBuilder<TKey, TValue>(IComparer<TKey>? keyComparer, IEqualityComparer<TValue>? valueComparer) where TKey : notnull
        {
            return Create(keyComparer, valueComparer).ToBuilder();
        }

        /// <summary>Enumerates and transforms a sequence, and produces an immutable sorted dictionary of its contents by using the specified key and value comparers.</summary>
        /// <param name="source">The sequence to enumerate to generate the dictionary.</param>
        /// <param name="keySelector">The function that will produce the key for the dictionary from each sequence element.</param>
        /// <param name="elementSelector">The function that will produce the value for the dictionary from each sequence element.</param>
        /// <param name="keyComparer">The key comparer to use for the dictionary.</param>
        /// <param name="valueComparer">The value comparer to use for the dictionary.</param>
        /// <typeparam name="TSource">The type of the elements in the sequence.</typeparam>
        /// <typeparam name="TKey">The type of the keys in the resulting dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the resulting dictionary.</typeparam>
        /// <returns>An immutable sorted dictionary that contains the items in the specified sequence.</returns>
        public static ImmutableSortedDictionary<TKey, TValue> ToImmutableSortedDictionary<TSource, TKey, TValue>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> elementSelector, IComparer<TKey>? keyComparer, IEqualityComparer<TValue>? valueComparer) where TKey : notnull
        {
            Func<TSource, TKey> keySelector2 = keySelector;
            Func<TSource, TValue> elementSelector2 = elementSelector;
            Requires.NotNull(source, "source");
            Requires.NotNull(keySelector2, "keySelector");
            Requires.NotNull(elementSelector2, "elementSelector");
            return ImmutableSortedDictionary<TKey, TValue>.Empty.WithComparers(keyComparer, valueComparer).AddRange(source.Select<TSource, KeyValuePair<TKey, TValue>>((TSource element) => new KeyValuePair<TKey, TValue>(keySelector2(element), elementSelector2(element))));
        }

        /// <summary>Creates an immutable sorted dictionary from the current contents of the builder's dictionary.</summary>
        /// <param name="builder">The builder to create the immutable sorted dictionary from.</param>
        /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
        /// <returns>An immutable sorted dictionary that contains the current contents in the builder's dictionary.</returns>
        public static ImmutableSortedDictionary<TKey, TValue> ToImmutableSortedDictionary<TKey, TValue>(this ImmutableSortedDictionary<TKey, TValue>.Builder builder) where TKey : notnull
        {
            Requires.NotNull(builder, "builder");
            return builder.ToImmutable();
        }

        /// <summary>Enumerates and transforms a sequence, and produces an immutable sorted dictionary of its contents by using the specified key comparer.</summary>
        /// <param name="source">The sequence to enumerate to generate the dictionary.</param>
        /// <param name="keySelector">The function that will produce the key for the dictionary from each sequence element.</param>
        /// <param name="elementSelector">The function that will produce the value for the dictionary from each sequence element.</param>
        /// <param name="keyComparer">The key comparer to use for the dictionary.</param>
        /// <typeparam name="TSource">The type of the elements in the sequence.</typeparam>
        /// <typeparam name="TKey">The type of the keys in the resulting dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the resulting dictionary.</typeparam>
        /// <returns>An immutable dictionary that contains the items in the specified sequence.</returns>
        public static ImmutableSortedDictionary<TKey, TValue> ToImmutableSortedDictionary<TSource, TKey, TValue>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> elementSelector, IComparer<TKey>? keyComparer) where TKey : notnull
        {
            return source.ToImmutableSortedDictionary(keySelector, elementSelector, keyComparer, null);
        }

        /// <summary>Enumerates and transforms a sequence, and produces an immutable sorted dictionary of its contents.</summary>
        /// <param name="source">The sequence to enumerate to generate the dictionary.</param>
        /// <param name="keySelector">The function that will produce the key for the dictionary from each sequence element.</param>
        /// <param name="elementSelector">The function that will produce the value for the dictionary from each sequence element.</param>
        /// <typeparam name="TSource">The type of the elements in the sequence.</typeparam>
        /// <typeparam name="TKey">The type of the keys in the resulting dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the resulting dictionary.</typeparam>
        /// <returns>An immutable sorted dictionary that contains the items in the specified sequence.</returns>
        public static ImmutableSortedDictionary<TKey, TValue> ToImmutableSortedDictionary<TSource, TKey, TValue>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> elementSelector) where TKey : notnull
        {
            return source.ToImmutableSortedDictionary(keySelector, elementSelector, null, null);
        }

        /// <summary>Enumerates a sequence of key/value pairs and produces an immutable sorted dictionary of its contents by using the specified key and value comparers.</summary>
        /// <param name="source">The sequence of key/value pairs to enumerate.</param>
        /// <param name="keyComparer">The key comparer to use when building the immutable dictionary.</param>
        /// <param name="valueComparer">The value comparer to use for the immutable dictionary.</param>
        /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
        /// <returns>An immutable sorted dictionary that contains the key/value pairs in the specified sequence.</returns>
        public static ImmutableSortedDictionary<TKey, TValue> ToImmutableSortedDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source, IComparer<TKey>? keyComparer, IEqualityComparer<TValue>? valueComparer) where TKey : notnull
        {
            Requires.NotNull(source, "source");
            if (source is ImmutableSortedDictionary<TKey, TValue> immutableSortedDictionary)
            {
                return immutableSortedDictionary.WithComparers(keyComparer, valueComparer);
            }
            return ImmutableSortedDictionary<TKey, TValue>.Empty.WithComparers(keyComparer, valueComparer).AddRange(source);
        }

        /// <summary>Enumerates a sequence of key/value pairs and produces an immutable dictionary of its contents by using the specified key comparer.</summary>
        /// <param name="source">The sequence of key/value pairs to enumerate.</param>
        /// <param name="keyComparer">The key comparer to use when building the immutable dictionary.</param>
        /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
        /// <returns>An immutable sorted dictionary that contains the key/value pairs in the specified sequence.</returns>
        public static ImmutableSortedDictionary<TKey, TValue> ToImmutableSortedDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source, IComparer<TKey>? keyComparer) where TKey : notnull
        {
            return source.ToImmutableSortedDictionary(keyComparer, null);
        }

        /// <summary>Enumerates a sequence of key/value pairs and produces an immutable sorted dictionary of its contents.</summary>
        /// <param name="source">The sequence of key/value pairs to enumerate.</param>
        /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
        /// <returns>An immutable sorted dictionary that contains the key/value pairs in the specified sequence.</returns>
        public static ImmutableSortedDictionary<TKey, TValue> ToImmutableSortedDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source) where TKey : notnull
        {
            return source.ToImmutableSortedDictionary(null, null);
        }
    }
   
    /// <summary>Represents an immutable sorted dictionary.  </summary>
    /// <typeparam name="TKey">The type of the key contained in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the value contained in the dictionary.</typeparam>
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(ImmutableDictionaryDebuggerProxy<,>))]
    public sealed class ImmutableSortedDictionary<TKey, TValue> : IImmutableDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>, IReadOnlyCollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, ISortKeyCollection<TKey>, IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IDictionary, ICollection where TKey : notnull
    {
        /// <summary>Represents a sorted dictionary that mutates with little or no memory allocations and that can produce or build on immutable sorted dictionary instances very efficiently. </summary>
        [DebuggerDisplay("Count = {Count}")]
        [DebuggerTypeProxy(typeof(ImmutableSortedDictionaryBuilderDebuggerProxy<,>))]
        public sealed class Builder : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, IReadOnlyDictionary<TKey, TValue>, IReadOnlyCollection<KeyValuePair<TKey, TValue>>, IDictionary, ICollection
        {
            private Node _root = Node.EmptyNode;

            private IComparer<TKey> _keyComparer = Comparer<TKey>.Default;

            private IEqualityComparer<TValue> _valueComparer = EqualityComparer<TValue>.Default;

            private int _count;

            private ImmutableSortedDictionary<TKey, TValue> _immutable;

            private int _version;

            private object _syncRoot;

            ICollection<TKey> IDictionary<TKey, TValue>.Keys => Root.Keys.ToArray(Count);

            /// <summary>Gets a strongly typed, read-only collection of elements.</summary>
            /// <returns>A strongly typed, read-only collection of elements.</returns>
            public IEnumerable<TKey> Keys => Root.Keys;

            ICollection<TValue> IDictionary<TKey, TValue>.Values => Root.Values.ToArray(Count);

            /// <summary>Gets a collection that contains the values of the immutable sorted dictionary.</summary>
            /// <returns>A collection that contains the values of the object that implements the dictionary.</returns>
            public IEnumerable<TValue> Values => Root.Values;

            /// <summary>Gets the number of elements in this immutable sorted dictionary.</summary>
            /// <returns>The number of elements in this dictionary.</returns>
            public int Count => _count;

            bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

            internal int Version => _version;

            private Node Root
            {
                get
                {
                    return _root;
                }
                set
                {
                    _version++;
                    if (_root != value)
                    {
                        _root = value;
                        _immutable = null;
                    }
                }
            }

            /// <summary>Gets or sets the value for a specified key in the immutable sorted dictionary.</summary>
            /// <param name="key">The key to retrieve the value for.</param>
            /// <returns>The value associated with the given key.</returns>
            public TValue this[TKey key]
            {
                get
                {
                    if (TryGetValue(key, out var value))
                    {
                        return value;
                    }
                    throw new KeyNotFoundException(System.SR.Format(MDCFR.Properties.Resources.Arg_KeyNotFoundWithKey, key.ToString()));
                }
                set
                {
                    Root = _root.SetItem(key, value, _keyComparer, _valueComparer, out var replacedExistingValue, out var mutated);
                    if (mutated && !replacedExistingValue)
                    {
                        _count++;
                    }
                }
            }

            /// <summary>Gets a value that indicates whether the <see cref="T:System.Collections.IDictionary" /> object has a fixed size.</summary>
            /// <returns>
            ///   <see langword="true" /> if the <see cref="T:System.Collections.IDictionary" /> object has a fixed size; otherwise, <see langword="false" />.</returns>
            bool IDictionary.IsFixedSize => false;

            /// <summary>Gets a value that indicates whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</summary>
            /// <returns>
            ///   <see langword="true" /> if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, <see langword="false" />.</returns>
            bool IDictionary.IsReadOnly => false;

            /// <summary>Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2" />.</summary>
            /// <returns>An <see cref="T:System.Collections.Generic.ICollection`1" /> containing the keys of the object that implements <see cref="T:System.Collections.Generic.IDictionary`2" />.</returns>
            ICollection IDictionary.Keys => Keys.ToArray(Count);

            /// <summary>Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2" />.</summary>
            /// <returns>An <see cref="T:System.Collections.Generic.ICollection`1" /> containing the values in the object that implements <see cref="T:System.Collections.Generic.IDictionary`2" />.</returns>
            ICollection IDictionary.Values => Values.ToArray(Count);

            /// <summary>Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.</summary>
            /// <returns>An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.</returns>
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            object ICollection.SyncRoot
            {
                get
                {
                    if (_syncRoot == null)
                    {
                        Interlocked.CompareExchange<object>(ref _syncRoot, new object(), (object)null);
                    }
                    return _syncRoot;
                }
            }

            /// <summary>Gets a value that indicates whether access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe).</summary>
            /// <returns>
            ///   <see langword="true" /> if access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe); otherwise, <see langword="false" />.</returns>
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            bool ICollection.IsSynchronized => false;

            /// <summary>Gets or sets the key comparer.</summary>
            /// <returns>The key comparer.</returns>
            public IComparer<TKey> KeyComparer
            {
                get
                {
                    return _keyComparer;
                }
                set
                {
                    Requires.NotNull(value, "value");
                    if (value == _keyComparer)
                    {
                        return;
                    }
                    Node node = Node.EmptyNode;
                    int num = 0;
                    using (Enumerator enumerator = GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<TKey, TValue> current = enumerator.Current;
                            node = node.Add(current.Key, current.Value, value, _valueComparer, out var mutated);
                            if (mutated)
                            {
                                num++;
                            }
                        }
                    }
                    _keyComparer = value;
                    Root = node;
                    _count = num;
                }
            }

            /// <summary>Gets or sets the value comparer.</summary>
            /// <returns>The value comparer.</returns>
            public IEqualityComparer<TValue> ValueComparer
            {
                get
                {
                    return _valueComparer;
                }
                set
                {
                    Requires.NotNull(value, "value");
                    if (value != _valueComparer)
                    {
                        _valueComparer = value;
                        _immutable = null;
                    }
                }
            }

            /// <summary>Gets or sets the element with the specified key.</summary>
            /// <param name="key">The key.</param>
            /// <returns>The value associated with the specified key.</returns>
            object? IDictionary.this[object key]
            {
                get
                {
                    return this[(TKey)key];
                }
                set
                {
                    this[(TKey)key] = (TValue)value;
                }
            }

            internal Builder(ImmutableSortedDictionary<TKey, TValue> map)
            {
                Requires.NotNull(map, "map");
                _root = map._root;
                _keyComparer = map.KeyComparer;
                _valueComparer = map.ValueComparer;
                _count = map.Count;
                _immutable = map;
            }

            /// <summary>Returns a read-only reference to the value associated with the provided <paramref name="key" />.</summary>
            /// <param name="key">Key of the entry to be looked up.</param>
            /// <exception cref="T:System.Collections.Generic.KeyNotFoundException">The <paramref name="key" /> is not present.</exception>
            /// <returns>A read-only reference to the value associated with the provided <paramref name="key" />.</returns>
            public ref readonly TValue ValueRef(TKey key)
            {
                Requires.NotNullAllowStructs(key, "key");
                return ref _root.ValueRef(key, _keyComparer);
            }

            /// <summary>Adds an element with the provided key and value to the dictionary object.</summary>
            /// <param name="key">The key of the element to add.</param>
            /// <param name="value">The value of the element to add.</param>
            void IDictionary.Add(object key, object value)
            {
                Add((TKey)key, (TValue)value);
            }

            /// <summary>Determines whether the dictionary object contains an element with the specified key.</summary>
            /// <param name="key">The key to locate.</param>
            /// <returns>
            ///   <see langword="true" /> if the dictionary contains an element with the key; otherwise, <see langword="false" />.</returns>
            bool IDictionary.Contains(object key)
            {
                return ContainsKey((TKey)key);
            }

            /// <summary>Returns an <see cref="T:System.Collections.IDictionaryEnumerator" /> object for the dictionary.</summary>
            /// <returns>An <see cref="T:System.Collections.IDictionaryEnumerator" /> object for the dictionary.</returns>
            IDictionaryEnumerator IDictionary.GetEnumerator()
            {
                return new DictionaryEnumerator<TKey, TValue>(GetEnumerator());
            }

            /// <summary>Removes the element with the specified key from the dictionary.</summary>
            /// <param name="key">The key of the element to remove.</param>
            void IDictionary.Remove(object key)
            {
                Remove((TKey)key);
            }

            /// <summary>Copies the elements of the dictionary to an array, starting at a particular array index.  
            ///
            ///  NuGet package: System.Collections.Immutable (about immutable collections and how to install)</summary>
            /// <param name="array">The one-dimensional array that is the destination of the elements copied from the dictionary. The array must have zero-based indexing.</param>
            /// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
            void ICollection.CopyTo(Array array, int index)
            {
                Root.CopyTo(array, index, Count);
            }

            /// <summary>Adds an element that has the specified key and value to the immutable sorted dictionary.</summary>
            /// <param name="key">The key of the element to add.</param>
            /// <param name="value">The value of the element to add.</param>
            public void Add(TKey key, TValue value)
            {
                Root = Root.Add(key, value, _keyComparer, _valueComparer, out var mutated);
                if (mutated)
                {
                    _count++;
                }
            }

            /// <summary>Determines whether the immutable sorted dictionary contains an element with the specified key.</summary>
            /// <param name="key">The key to locate in the dictionary.</param>
            /// <returns>
            ///   <see langword="true" /> if the dictionary contains an element with the key; otherwise, <see langword="false" />.</returns>
            public bool ContainsKey(TKey key)
            {
                return Root.ContainsKey(key, _keyComparer);
            }

            /// <summary>Removes the element with the specified key from the immutable sorted dictionary.</summary>
            /// <param name="key">The key of the element to remove.</param>
            /// <returns>
            ///   <see langword="true" /> if the element is successfully removed; otherwise, <see langword="false" />. This method also returns <see langword="false" /> if <paramref name="key" /> was not found in the original dictionary.</returns>
            public bool Remove(TKey key)
            {
                Root = Root.Remove(key, _keyComparer, out var mutated);
                if (mutated)
                {
                    _count--;
                }
                return mutated;
            }

            /// <summary>Gets the value associated with the specified key.</summary>
            /// <param name="key">The key whose value will be retrieved.</param>
            /// <param name="value">When this method returns, contains the value associated with the specified key, if the key is found; otherwise, contains the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.</param>
            /// <returns>
            ///   <see langword="true" /> if the object that implements the dictionary contains an element with the specified key; otherwise, <see langword="false" />.</returns>
            public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
            {
                return Root.TryGetValue(key, _keyComparer, out value);
            }

            /// <summary>Determines whether this dictionary contains a specified key.</summary>
            /// <param name="equalKey">The key to search for.</param>
            /// <param name="actualKey">The matching key located in the dictionary if found, or <c>equalkey</c> if no match is found.</param>
            /// <returns>
            ///   <see langword="true" /> if a match for <paramref name="equalKey" /> is found; otherwise, <see langword="false" />.</returns>
            public bool TryGetKey(TKey equalKey, out TKey actualKey)
            {
                Requires.NotNullAllowStructs(equalKey, "equalKey");
                return Root.TryGetKey(equalKey, _keyComparer, out actualKey);
            }

            /// <summary>Adds the specified item to the immutable sorted dictionary.</summary>
            /// <param name="item">The object to add to the dictionary.</param>
            public void Add(KeyValuePair<TKey, TValue> item)
            {
                Add(item.Key, item.Value);
            }

            /// <summary>Removes all items from the immutable sorted dictionary.</summary>
            public void Clear()
            {
                Root = Node.EmptyNode;
                _count = 0;
            }

            /// <summary>Determines whether the immutable sorted dictionary contains a specific value.</summary>
            /// <param name="item">The object to locate in the dictionary.</param>
            /// <returns>
            ///   <see langword="true" /> if <paramref name="item" /> is found in the dictionary; otherwise, <see langword="false" />.</returns>
            public bool Contains(KeyValuePair<TKey, TValue> item)
            {
                return Root.Contains(item, _keyComparer, _valueComparer);
            }

            void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
            {
                Root.CopyTo(array, arrayIndex, Count);
            }

            /// <summary>Removes the first occurrence of a specific object from the immutable sorted dictionary.</summary>
            /// <param name="item">The object to remove from the dictionary.</param>
            /// <returns>
            ///   <see langword="true" /> if <paramref name="item" /> was successfully removed from the dictionary; otherwise, <see langword="false" />. This method also returns <see langword="false" /> if <paramref name="item" /> is not found in the dictionary.</returns>
            public bool Remove(KeyValuePair<TKey, TValue> item)
            {
                if (Contains(item))
                {
                    return Remove(item.Key);
                }
                return false;
            }

            /// <summary>Returns an enumerator that iterates through the immutable sorted dictionary.</summary>
            /// <returns>An enumerator that can be used to iterate through the dictionary.</returns>
            public ImmutableSortedDictionary<TKey, TValue>.Enumerator GetEnumerator()
            {
                return Root.GetEnumerator(this);
            }

            IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
            {
                return GetEnumerator();
            }

            /// <summary>Returns an enumerator that iterates through a collection.</summary>
            /// <returns>An enumerator object that can be used to iterate through the collection.</returns>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            /// <summary>Determines whether the immutable sorted dictionary contains an element with the specified value.</summary>
            /// <param name="value">The value to locate in the dictionary. The value can be <see langword="null" /> for reference types.</param>
            /// <returns>
            ///   <see langword="true" /> if the immutable sorted dictionary contains an element with the specified value; otherwise, <see langword="false" />.</returns>
            public bool ContainsValue(TValue value)
            {
                return _root.ContainsValue(value, _valueComparer);
            }

            /// <summary>Adds a sequence of values to the immutable sorted dictionary.</summary>
            /// <param name="items">The items to add to the dictionary.</param>
            public void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> items)
            {
                Requires.NotNull(items, "items");
                foreach (KeyValuePair<TKey, TValue> item in items)
                {
                    Add(item);
                }
            }

            /// <summary>Removes any entries with keys that match those found in the specified sequence from the immutable sorted dictionary.</summary>
            /// <param name="keys">The keys for entries to remove from the dictionary.</param>
            public void RemoveRange(IEnumerable<TKey> keys)
            {
                Requires.NotNull(keys, "keys");
                foreach (TKey key in keys)
                {
                    Remove(key);
                }
            }

            /// <summary>Gets the value for a given key if a matching key exists in the dictionary; otherwise the default value.</summary>
            /// <param name="key">The key to search for.</param>
            /// <returns>The value for the key, or <c>default(TValue)</c> if no matching key was found.</returns>
            public TValue? GetValueOrDefault(TKey key)
            {
                return GetValueOrDefault(key, default(TValue));
            }

            /// <summary>Gets the value for a given key if a matching key exists in the dictionary; otherwise the default value.</summary>
            /// <param name="key">The key to search for.</param>
            /// <param name="defaultValue">The default value to return if no matching key is found in the dictionary.</param>
            /// <returns>The value for the key, or <paramref name="defaultValue" /> if no matching key was found.</returns>
            public TValue GetValueOrDefault(TKey key, TValue defaultValue)
            {
                Requires.NotNullAllowStructs(key, "key");
                if (TryGetValue(key, out var value))
                {
                    return value;
                }
                return defaultValue;
            }

            /// <summary>Creates an immutable sorted dictionary based on the contents of this instance.</summary>
            /// <returns>An immutable sorted dictionary.</returns>
            public ImmutableSortedDictionary<TKey, TValue> ToImmutable()
            {
                return _immutable ?? (_immutable = ImmutableSortedDictionary<TKey, TValue>.Wrap(Root, _count, _keyComparer, _valueComparer));
            }
        }

        /// <summary>Enumerates the contents of a binary tree.  </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IDisposable, IEnumerator, ISecurePooledObjectUser
        {
            private readonly Builder _builder;

            private readonly int _poolUserId;

            private Node _root;

            private SecurePooledObject<Stack<RefAsValueType<Node>>> _stack;

            private Node _current;

            private int _enumeratingBuilderVersion;

            /// <summary>Gets the element at the current position of the enumerator.</summary>
            /// <returns>The element at the current position of the enumerator.</returns>
            public KeyValuePair<TKey, TValue> Current
            {
                get
                {
                    ThrowIfDisposed();
                    if (_current != null)
                    {
                        return _current.Value;
                    }
                    throw new InvalidOperationException();
                }
            }

            int ISecurePooledObjectUser.PoolUserId => _poolUserId;

            /// <summary>The current element.</summary>
            /// <returns>The element in the collection at the current position of the enumerator.</returns>
            object IEnumerator.Current => Current;

            internal Enumerator(Node root, Builder? builder = null)
            {
                Requires.NotNull(root, "root");
                _root = root;
                _builder = builder;
                _current = null;
                _enumeratingBuilderVersion = builder?.Version ?? (-1);
                _poolUserId = SecureObjectPool.NewId();
                _stack = null;
                if (!_root.IsEmpty)
                {
                    if (!SecureObjectPool<Stack<RefAsValueType<Node>>, Enumerator>.TryTake(this, out _stack))
                    {
                        _stack = SecureObjectPool<Stack<RefAsValueType<Node>>, Enumerator>.PrepNew(this, new Stack<RefAsValueType<Node>>(root.Height));
                    }
                    PushLeft(_root);
                }
            }

            /// <summary>Releases the resources used by the current instance of the <see cref="T:System.Collections.Immutable.ImmutableSortedDictionary`2.Enumerator" /> class.</summary>
            public void Dispose()
            {
                _root = null;
                _current = null;
                if (_stack != null && _stack.TryUse(ref this, out var value))
                {
                    value.ClearFastWhenEmpty();
                    SecureObjectPool<Stack<RefAsValueType<Node>>, Enumerator>.TryAdd(this, _stack);
                }
                _stack = null;
            }

            /// <summary>Advances the enumerator to the next element of the immutable sorted dictionary.</summary>
            /// <returns>
            ///   <see langword="true" /> if the enumerator was successfully advanced to the next element; <see langword="false" /> if the enumerator has passed the end of the sorted dictionary.</returns>
            public bool MoveNext()
            {
                ThrowIfDisposed();
                ThrowIfChanged();
                if (_stack != null)
                {
                    Stack<RefAsValueType<Node>> stack = _stack.Use(ref this);
                    if (stack.Count > 0)
                    {
                        PushLeft((_current = stack.Pop().Value).Right);
                        return true;
                    }
                }
                _current = null;
                return false;
            }

            /// <summary>Sets the enumerator to its initial position, which is before the first element in the immutable sorted dictionary.</summary>
            public void Reset()
            {
                ThrowIfDisposed();
                _enumeratingBuilderVersion = ((_builder != null) ? _builder.Version : (-1));
                _current = null;
                if (_stack != null)
                {
                    Stack<RefAsValueType<Node>> stack = _stack.Use(ref this);
                    stack.ClearFastWhenEmpty();
                    PushLeft(_root);
                }
            }

            internal void ThrowIfDisposed()
            {
                if (_root == null || (_stack != null && !_stack.IsOwned(ref this)))
                {
                    Requires.FailObjectDisposed(this);
                }
            }

            private void ThrowIfChanged()
            {
                if (_builder != null && _builder.Version != _enumeratingBuilderVersion)
                {
                    throw new InvalidOperationException(MDCFR.Properties.Resources.CollectionModifiedDuringEnumeration);
                }
            }

            private void PushLeft(Node node)
            {
                Requires.NotNull(node, "node");
                Stack<RefAsValueType<Node>> stack = _stack.Use(ref this);
                while (!node.IsEmpty)
                {
                    stack.Push(new RefAsValueType<Node>(node));
                    node = node.Left;
                }
            }
        }

        [DebuggerDisplay("{_key} = {_value}")]
        internal sealed class Node : IBinaryTree<KeyValuePair<TKey, TValue>>, IBinaryTree, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
        {
            internal static readonly Node EmptyNode = new Node();

            private readonly TKey _key;

            private readonly TValue _value;

            private bool _frozen;

            private byte _height;

            private Node _left;

            private Node _right;

            public bool IsEmpty => _left == null;

            IBinaryTree<KeyValuePair<TKey, TValue>>? IBinaryTree<KeyValuePair<TKey, TValue>>.Left => _left;

            IBinaryTree<KeyValuePair<TKey, TValue>>? IBinaryTree<KeyValuePair<TKey, TValue>>.Right => _right;

            public int Height => _height;

            public Node? Left => _left;

            IBinaryTree? IBinaryTree.Left => _left;

            public Node? Right => _right;

            IBinaryTree? IBinaryTree.Right => _right;

            public KeyValuePair<TKey, TValue> Value => new KeyValuePair<TKey, TValue>(_key, _value);

            int IBinaryTree.Count
            {
                get
                {
                    throw new NotSupportedException();
                }
            }

            internal IEnumerable<TKey> Keys => this.Select((KeyValuePair<TKey, TValue> p) => p.Key);

            internal IEnumerable<TValue> Values => this.Select((KeyValuePair<TKey, TValue> p) => p.Value);

            private Node()
            {
                _frozen = true;
            }

            private Node(TKey key, TValue value, Node left, Node right, bool frozen = false)
            {
                Requires.NotNullAllowStructs(key, "key");
                Requires.NotNull(left, "left");
                Requires.NotNull(right, "right");
                _key = key;
                _value = value;
                _left = left;
                _right = right;
                checked
                {
                    _height = (byte)(1 + unchecked((int)Math.Max(left._height, right._height)));
                    _frozen = frozen;
                }
            }

            public Enumerator GetEnumerator()
            {
                return new Enumerator(this);
            }

            IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
            {
                return GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            internal Enumerator GetEnumerator(Builder builder)
            {
                return new Enumerator(this, builder);
            }

            internal void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex, int dictionarySize)
            {
                Requires.NotNull(array, "array");
                Requires.Range(arrayIndex >= 0, "arrayIndex");
                Requires.Range(array.Length >= arrayIndex + dictionarySize, "arrayIndex");
                using Enumerator enumerator = GetEnumerator();
                while (enumerator.MoveNext())
                {
                    KeyValuePair<TKey, TValue> current = enumerator.Current;
                    array[arrayIndex++] = current;
                }
            }

            internal void CopyTo(Array array, int arrayIndex, int dictionarySize)
            {
                Requires.NotNull(array, "array");
                Requires.Range(arrayIndex >= 0, "arrayIndex");
                Requires.Range(array.Length >= arrayIndex + dictionarySize, "arrayIndex");
                using Enumerator enumerator = GetEnumerator();
                while (enumerator.MoveNext())
                {
                    KeyValuePair<TKey, TValue> current = enumerator.Current;
                    array.SetValue(new DictionaryEntry(current.Key, current.Value), arrayIndex++);
                }
            }

            internal static Node NodeTreeFromSortedDictionary(SortedDictionary<TKey, TValue> dictionary)
            {
                Requires.NotNull(dictionary, "dictionary");
                IOrderedCollection<KeyValuePair<TKey, TValue>> orderedCollection = dictionary.AsOrderedCollection();
                return NodeTreeFromList(orderedCollection, 0, orderedCollection.Count);
            }

            internal Node Add(TKey key, TValue value, IComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer, out bool mutated)
            {
                Requires.NotNullAllowStructs(key, "key");
                Requires.NotNull(keyComparer, "keyComparer");
                Requires.NotNull(valueComparer, "valueComparer");
                bool replacedExistingValue;
                return SetOrAdd(key, value, keyComparer, valueComparer, overwriteExistingValue: false, out replacedExistingValue, out mutated);
            }

            internal Node SetItem(TKey key, TValue value, IComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer, out bool replacedExistingValue, out bool mutated)
            {
                Requires.NotNullAllowStructs(key, "key");
                Requires.NotNull(keyComparer, "keyComparer");
                Requires.NotNull(valueComparer, "valueComparer");
                return SetOrAdd(key, value, keyComparer, valueComparer, overwriteExistingValue: true, out replacedExistingValue, out mutated);
            }

            internal Node Remove(TKey key, IComparer<TKey> keyComparer, out bool mutated)
            {
                Requires.NotNullAllowStructs(key, "key");
                Requires.NotNull(keyComparer, "keyComparer");
                return RemoveRecursive(key, keyComparer, out mutated);
            }

            internal ref readonly TValue ValueRef(TKey key, IComparer<TKey> keyComparer)
            {
                Requires.NotNullAllowStructs(key, "key");
                Requires.NotNull(keyComparer, "keyComparer");
                Node node = Search(key, keyComparer);
                if (node.IsEmpty)
                {
                    throw new KeyNotFoundException(System.SR.Format(MDCFR.Properties.Resources.Arg_KeyNotFoundWithKey, key.ToString()));
                }
                return ref node._value;
            }

            internal bool TryGetValue(TKey key, IComparer<TKey> keyComparer, [MaybeNullWhen(false)] out TValue value)
            {
                Requires.NotNullAllowStructs(key, "key");
                Requires.NotNull(keyComparer, "keyComparer");
                Node node = Search(key, keyComparer);
                if (node.IsEmpty)
                {
                    value = default(TValue);
                    return false;
                }
                value = node._value;
                return true;
            }

            internal bool TryGetKey(TKey equalKey, IComparer<TKey> keyComparer, out TKey actualKey)
            {
                Requires.NotNullAllowStructs(equalKey, "equalKey");
                Requires.NotNull(keyComparer, "keyComparer");
                Node node = Search(equalKey, keyComparer);
                if (node.IsEmpty)
                {
                    actualKey = equalKey;
                    return false;
                }
                actualKey = node._key;
                return true;
            }

            internal bool ContainsKey(TKey key, IComparer<TKey> keyComparer)
            {
                Requires.NotNullAllowStructs(key, "key");
                Requires.NotNull(keyComparer, "keyComparer");
                return !Search(key, keyComparer).IsEmpty;
            }

            internal bool ContainsValue(TValue value, IEqualityComparer<TValue> valueComparer)
            {
                Requires.NotNull(valueComparer, "valueComparer");
                using (Enumerator enumerator = GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        if (valueComparer.Equals(value, enumerator.Current.Value))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            internal bool Contains(KeyValuePair<TKey, TValue> pair, IComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
            {
                Requires.NotNullAllowStructs(pair.Key, "Key");
                Requires.NotNull(keyComparer, "keyComparer");
                Requires.NotNull(valueComparer, "valueComparer");
                Node node = Search(pair.Key, keyComparer);
                if (node.IsEmpty)
                {
                    return false;
                }
                return valueComparer.Equals(node._value, pair.Value);
            }

            internal void Freeze()
            {
                if (!_frozen)
                {
                    _left.Freeze();
                    _right.Freeze();
                    _frozen = true;
                }
            }

            private static Node RotateLeft(Node tree)
            {
                Requires.NotNull(tree, "tree");
                if (tree._right.IsEmpty)
                {
                    return tree;
                }
                Node right = tree._right;
                return right.Mutate(tree.Mutate(null, right._left));
            }

            private static Node RotateRight(Node tree)
            {
                Requires.NotNull(tree, "tree");
                if (tree._left.IsEmpty)
                {
                    return tree;
                }
                Node left = tree._left;
                return left.Mutate(null, tree.Mutate(left._right));
            }

            private static Node DoubleLeft(Node tree)
            {
                Requires.NotNull(tree, "tree");
                if (tree._right.IsEmpty)
                {
                    return tree;
                }
                Node tree2 = tree.Mutate(null, RotateRight(tree._right));
                return RotateLeft(tree2);
            }

            private static Node DoubleRight(Node tree)
            {
                Requires.NotNull(tree, "tree");
                if (tree._left.IsEmpty)
                {
                    return tree;
                }
                Node tree2 = tree.Mutate(RotateLeft(tree._left));
                return RotateRight(tree2);
            }

            private static int Balance(Node tree)
            {
                Requires.NotNull(tree, "tree");
                return tree._right._height - tree._left._height;
            }

            private static bool IsRightHeavy(Node tree)
            {
                Requires.NotNull(tree, "tree");
                return Balance(tree) >= 2;
            }

            private static bool IsLeftHeavy(Node tree)
            {
                Requires.NotNull(tree, "tree");
                return Balance(tree) <= -2;
            }

            private static Node MakeBalanced(Node tree)
            {
                Requires.NotNull(tree, "tree");
                if (IsRightHeavy(tree))
                {
                    if (Balance(tree._right) >= 0)
                    {
                        return RotateLeft(tree);
                    }
                    return DoubleLeft(tree);
                }
                if (IsLeftHeavy(tree))
                {
                    if (Balance(tree._left) <= 0)
                    {
                        return RotateRight(tree);
                    }
                    return DoubleRight(tree);
                }
                return tree;
            }

            private static Node NodeTreeFromList(IOrderedCollection<KeyValuePair<TKey, TValue>> items, int start, int length)
            {
                Requires.NotNull(items, "items");
                Requires.Range(start >= 0, "start");
                Requires.Range(length >= 0, "length");
                if (length == 0)
                {
                    return EmptyNode;
                }
                int num = (length - 1) / 2;
                int num2 = length - 1 - num;
                Node left = NodeTreeFromList(items, start, num2);
                Node right = NodeTreeFromList(items, start + num2 + 1, num);
                KeyValuePair<TKey, TValue> keyValuePair = items[start + num2];
                return new Node(keyValuePair.Key, keyValuePair.Value, left, right, frozen: true);
            }

            private Node SetOrAdd(TKey key, TValue value, IComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer, bool overwriteExistingValue, out bool replacedExistingValue, out bool mutated)
            {
                replacedExistingValue = false;
                if (IsEmpty)
                {
                    mutated = true;
                    return new Node(key, value, this, this);
                }
                Node node = this;
                int num = keyComparer.Compare(key, _key);
                if (num > 0)
                {
                    Node right = _right.SetOrAdd(key, value, keyComparer, valueComparer, overwriteExistingValue, out replacedExistingValue, out mutated);
                    if (mutated)
                    {
                        node = Mutate(null, right);
                    }
                }
                else if (num < 0)
                {
                    Node left = _left.SetOrAdd(key, value, keyComparer, valueComparer, overwriteExistingValue, out replacedExistingValue, out mutated);
                    if (mutated)
                    {
                        node = Mutate(left);
                    }
                }
                else
                {
                    if (valueComparer.Equals(_value, value))
                    {
                        mutated = false;
                        return this;
                    }
                    if (!overwriteExistingValue)
                    {
                        throw new ArgumentException(System.SR.Format(MDCFR.Properties.Resources.DuplicateKey, key));
                    }
                    mutated = true;
                    replacedExistingValue = true;
                    node = new Node(key, value, _left, _right);
                }
                if (!mutated)
                {
                    return node;
                }
                return MakeBalanced(node);
            }

            private Node RemoveRecursive(TKey key, IComparer<TKey> keyComparer, out bool mutated)
            {
                if (IsEmpty)
                {
                    mutated = false;
                    return this;
                }
                Node node = this;
                int num = keyComparer.Compare(key, _key);
                if (num == 0)
                {
                    mutated = true;
                    if (_right.IsEmpty && _left.IsEmpty)
                    {
                        node = EmptyNode;
                    }
                    else if (_right.IsEmpty && !_left.IsEmpty)
                    {
                        node = _left;
                    }
                    else if (!_right.IsEmpty && _left.IsEmpty)
                    {
                        node = _right;
                    }
                    else
                    {
                        Node node2 = _right;
                        while (!node2._left.IsEmpty)
                        {
                            node2 = node2._left;
                        }
                        bool mutated2;
                        Node right = _right.Remove(node2._key, keyComparer, out mutated2);
                        node = node2.Mutate(_left, right);
                    }
                }
                else if (num < 0)
                {
                    Node left = _left.Remove(key, keyComparer, out mutated);
                    if (mutated)
                    {
                        node = Mutate(left);
                    }
                }
                else
                {
                    Node right2 = _right.Remove(key, keyComparer, out mutated);
                    if (mutated)
                    {
                        node = Mutate(null, right2);
                    }
                }
                if (!node.IsEmpty)
                {
                    return MakeBalanced(node);
                }
                return node;
            }

            private Node Mutate(Node left = null, Node right = null)
            {
                if (_frozen)
                {
                    return new Node(_key, _value, left ?? _left, right ?? _right);
                }
                if (left != null)
                {
                    _left = left;
                }
                if (right != null)
                {
                    _right = right;
                }
                checked
                {
                    _height = (byte)(1 + unchecked((int)Math.Max(_left._height, _right._height)));
                    return this;
                }
            }

            private Node Search(TKey key, IComparer<TKey> keyComparer)
            {
                if (IsEmpty)
                {
                    return this;
                }
                int num = keyComparer.Compare(key, _key);
                if (num == 0)
                {
                    return this;
                }
                if (num > 0)
                {
                    return _right.Search(key, keyComparer);
                }
                return _left.Search(key, keyComparer);
            }
        }

        /// <summary>Gets an empty immutable sorted dictionary.</summary>
        public static readonly ImmutableSortedDictionary<TKey, TValue> Empty = new ImmutableSortedDictionary<TKey, TValue>();

        private readonly Node _root;

        private readonly int _count;

        private readonly IComparer<TKey> _keyComparer;

        private readonly IEqualityComparer<TValue> _valueComparer;

        /// <summary>Gets the value comparer used to determine whether values are equal.</summary>
        /// <returns>The value comparer used to determine whether values are equal.</returns>
        public IEqualityComparer<TValue> ValueComparer => _valueComparer;

        /// <summary>Gets a value that indicates whether this instance of the immutable sorted dictionary is empty.</summary>
        /// <returns>
        ///   <see langword="true" /> if this instance is empty; otherwise, <see langword="false" />.</returns>
        public bool IsEmpty => _root.IsEmpty;

        /// <summary>Gets the number of key/value pairs in the immutable sorted dictionary.</summary>
        /// <returns>The number of key/value pairs in the dictionary.</returns>
        public int Count => _count;

        /// <summary>Gets the keys in the immutable sorted dictionary.</summary>
        /// <returns>The keys in the immutable dictionary.</returns>
        public IEnumerable<TKey> Keys => _root.Keys;

        /// <summary>Gets the values in the immutable sorted dictionary.</summary>
        /// <returns>The values in the dictionary.</returns>
        public IEnumerable<TValue> Values => _root.Values;

        ICollection<TKey> IDictionary<TKey, TValue>.Keys => new KeysCollectionAccessor<TKey, TValue>(this);

        ICollection<TValue> IDictionary<TKey, TValue>.Values => new ValuesCollectionAccessor<TKey, TValue>(this);

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => true;

        /// <summary>Gets the key comparer for the immutable sorted dictionary.</summary>
        /// <returns>The key comparer for the dictionary.</returns>
        public IComparer<TKey> KeyComparer => _keyComparer;

        internal Node Root => _root;

        /// <summary>Gets the TValue associated with the specified key.</summary>
        /// <param name="key">The key to retrieve the value for.</param>
        /// <returns>The value associated with the specified key. If no results are found, the operation throws an exception.</returns>
        public TValue this[TKey key]
        {
            get
            {
                Requires.NotNullAllowStructs(key, "key");
                if (TryGetValue(key, out var value))
                {
                    return value;
                }
                throw new KeyNotFoundException(System.SR.Format(MDCFR.Properties.Resources.Arg_KeyNotFoundWithKey, key.ToString()));
            }
        }

        TValue IDictionary<TKey, TValue>.this[TKey key]
        {
            get
            {
                return this[key];
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>Gets a value indicating whether the <see cref="T:System.Collections.IDictionary" /> object has a fixed size.</summary>
        /// <returns>
        ///   <see langword="true" /> if the <see cref="T:System.Collections.IDictionary" /> object has a fixed size; otherwise, <see langword="false" />.</returns>
        bool IDictionary.IsFixedSize => true;

        /// <summary>Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</summary>
        /// <returns>
        ///   <see langword="true" /> if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, <see langword="false" />.</returns>
        bool IDictionary.IsReadOnly => true;

        /// <summary>Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2" />.</summary>
        /// <returns>An <see cref="T:System.Collections.Generic.ICollection`1" /> containing the keys of the object that implements <see cref="T:System.Collections.Generic.IDictionary`2" />.</returns>
        ICollection IDictionary.Keys => new KeysCollectionAccessor<TKey, TValue>(this);

        /// <summary>Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2" />.</summary>
        /// <returns>An <see cref="T:System.Collections.Generic.ICollection`1" /> containing the values in the object that implements <see cref="T:System.Collections.Generic.IDictionary`2" />.</returns>
        ICollection IDictionary.Values => new ValuesCollectionAccessor<TKey, TValue>(this);

        /// <summary>Gets or sets the element with the specified key.</summary>
        /// <param name="key">The key of the element to be accessed.</param>
        /// <returns>Value stored under the specified key.</returns>
        object? IDictionary.this[object key]
        {
            get
            {
                return this[(TKey)key];
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.</summary>
        /// <returns>An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.</returns>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        object ICollection.SyncRoot => this;

        /// <summary>Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe).</summary>
        /// <returns>
        ///   <see langword="true" /> if access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread-safe); otherwise, <see langword="false" />.</returns>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool ICollection.IsSynchronized => true;

        internal ImmutableSortedDictionary(IComparer<TKey>? keyComparer = null, IEqualityComparer<TValue>? valueComparer = null)
        {
            _keyComparer = keyComparer ?? Comparer<TKey>.Default;
            _valueComparer = valueComparer ?? EqualityComparer<TValue>.Default;
            _root = Node.EmptyNode;
        }

        private ImmutableSortedDictionary(Node root, int count, IComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
        {
            Requires.NotNull(root, "root");
            Requires.Range(count >= 0, "count");
            Requires.NotNull(keyComparer, "keyComparer");
            Requires.NotNull(valueComparer, "valueComparer");
            root.Freeze();
            _root = root;
            _count = count;
            _keyComparer = keyComparer;
            _valueComparer = valueComparer;
        }

        /// <summary>Retrieves an empty immutable sorted dictionary that has the same ordering and key/value comparison rules as this dictionary instance.</summary>
        /// <returns>An empty dictionary with equivalent ordering and key/value comparison rules.</returns>
        public ImmutableSortedDictionary<TKey, TValue> Clear()
        {
            if (!_root.IsEmpty)
            {
                return Empty.WithComparers(_keyComparer, _valueComparer);
            }
            return this;
        }

        IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.Clear()
        {
            return Clear();
        }

        /// <summary>Returns a read-only reference to the value associated with the provided <paramref name="key" />.</summary>
        /// <param name="key">Key of the entry to be looked up.</param>
        /// <exception cref="T:System.Collections.Generic.KeyNotFoundException">The <paramref name="key" /> is not present.</exception>
        /// <returns>A read-only reference to the value associated with the provided <paramref name="key" />.</returns>
        public ref readonly TValue ValueRef(TKey key)
        {
            Requires.NotNullAllowStructs(key, "key");
            return ref _root.ValueRef(key, _keyComparer);
        }

        /// <summary>Creates an immutable sorted dictionary with the same contents as this dictionary that can be efficiently mutated across multiple operations by using standard mutable interfaces.</summary>
        /// <returns>A collection with the same contents as this dictionary.</returns>
        public Builder ToBuilder()
        {
            return new Builder(this);
        }

        /// <summary>Adds an element with the specified key and value to the immutable sorted dictionary.</summary>
        /// <param name="key">The key of the entry to add.</param>
        /// <param name="value">The value of entry to add.</param>
        /// <exception cref="T:System.ArgumentException">The given key already exists in the dictionary but has a different value.</exception>
        /// <returns>A new immutable sorted dictionary that contains the additional key/value pair.</returns>
        public ImmutableSortedDictionary<TKey, TValue> Add(TKey key, TValue value)
        {
            Requires.NotNullAllowStructs(key, "key");
            bool mutated;
            Node root = _root.Add(key, value, _keyComparer, _valueComparer, out mutated);
            return Wrap(root, _count + 1);
        }

        /// <summary>Sets the specified key and value in the immutable sorted dictionary, possibly overwriting an existing value for the given key.</summary>
        /// <param name="key">The key of the entry to add.</param>
        /// <param name="value">The key value to set.</param>
        /// <returns>A new immutable sorted dictionary that contains the specified key/value pair.</returns>
        public ImmutableSortedDictionary<TKey, TValue> SetItem(TKey key, TValue value)
        {
            Requires.NotNullAllowStructs(key, "key");
            bool replacedExistingValue;
            bool mutated;
            Node root = _root.SetItem(key, value, _keyComparer, _valueComparer, out replacedExistingValue, out mutated);
            return Wrap(root, replacedExistingValue ? _count : (_count + 1));
        }

        /// <summary>Sets the specified key/value pairs in the immutable sorted dictionary, possibly overwriting existing values for the keys.</summary>
        /// <param name="items">The key/value pairs to set in the dictionary. If any of the keys already exist in the dictionary, this method will overwrite their previous values.</param>
        /// <returns>An immutable dictionary that contains the specified key/value pairs.</returns>
        public ImmutableSortedDictionary<TKey, TValue> SetItems(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            Requires.NotNull(items, "items");
            return AddRange(items, overwriteOnCollision: true, avoidToSortedMap: false);
        }

        /// <summary>Adds the specific key/value pairs to the immutable sorted dictionary.</summary>
        /// <param name="items">The key/value pairs to add.</param>
        /// <exception cref="T:System.ArgumentException">One of the given keys already exists in the dictionary but has a different value.</exception>
        /// <returns>A new immutable dictionary that contains the additional key/value pairs.</returns>
        public ImmutableSortedDictionary<TKey, TValue> AddRange(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            Requires.NotNull(items, "items");
            return AddRange(items, overwriteOnCollision: false, avoidToSortedMap: false);
        }

        /// <summary>Removes the element with the specified value from the immutable sorted dictionary.</summary>
        /// <param name="value">The value of the element to remove.</param>
        /// <returns>A new immutable dictionary with the specified element removed; or this instance if the specified value cannot be found in the dictionary.</returns>
        public ImmutableSortedDictionary<TKey, TValue> Remove(TKey value)
        {
            Requires.NotNullAllowStructs(value, "value");
            bool mutated;
            Node root = _root.Remove(value, _keyComparer, out mutated);
            return Wrap(root, _count - 1);
        }

        /// <summary>Removes the elements with the specified keys from the immutable sorted dictionary.</summary>
        /// <param name="keys">The keys of the elements to remove.</param>
        /// <returns>A new immutable dictionary with the specified keys removed; or this instance if the specified keys cannot be found in the dictionary.</returns>
        public ImmutableSortedDictionary<TKey, TValue> RemoveRange(IEnumerable<TKey> keys)
        {
            Requires.NotNull(keys, "keys");
            Node node = _root;
            int num = _count;
            foreach (TKey key in keys)
            {
                bool mutated;
                Node node2 = node.Remove(key, _keyComparer, out mutated);
                if (mutated)
                {
                    node = node2;
                    num--;
                }
            }
            return Wrap(node, num);
        }

        /// <summary>Gets an instance of the immutable sorted dictionary that uses the specified key and value comparers.</summary>
        /// <param name="keyComparer">The key comparer to use.</param>
        /// <param name="valueComparer">The value comparer to use.</param>
        /// <returns>An instance of the immutable dictionary that uses the given comparers.</returns>
        public ImmutableSortedDictionary<TKey, TValue> WithComparers(IComparer<TKey>? keyComparer, IEqualityComparer<TValue>? valueComparer)
        {
            if (keyComparer == null)
            {
                keyComparer = Comparer<TKey>.Default;
            }
            if (valueComparer == null)
            {
                valueComparer = EqualityComparer<TValue>.Default;
            }
            if (keyComparer == _keyComparer)
            {
                if (valueComparer == _valueComparer)
                {
                    return this;
                }
                return new ImmutableSortedDictionary<TKey, TValue>(_root, _count, _keyComparer, valueComparer);
            }
            ImmutableSortedDictionary<TKey, TValue> immutableSortedDictionary = new ImmutableSortedDictionary<TKey, TValue>(Node.EmptyNode, 0, keyComparer, valueComparer);
            return immutableSortedDictionary.AddRange(this, overwriteOnCollision: false, avoidToSortedMap: true);
        }

        /// <summary>Gets an instance of the immutable sorted dictionary that uses the specified key comparer.</summary>
        /// <param name="keyComparer">The key comparer to use.</param>
        /// <returns>An instance of the immutable dictionary that uses the given comparer.</returns>
        public ImmutableSortedDictionary<TKey, TValue> WithComparers(IComparer<TKey>? keyComparer)
        {
            return WithComparers(keyComparer, _valueComparer);
        }

        /// <summary>Determines whether the immutable sorted dictionary contains an element with the specified value.</summary>
        /// <param name="value">The value to locate. The value can be <see langword="null" /> for reference types.</param>
        /// <returns>
        ///   <see langword="true" /> if the dictionary contains an element with the specified value; otherwise, <see langword="false" />.</returns>
        public bool ContainsValue(TValue value)
        {
            return _root.ContainsValue(value, _valueComparer);
        }

        IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.Add(TKey key, TValue value)
        {
            return Add(key, value);
        }

        IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.SetItem(TKey key, TValue value)
        {
            return SetItem(key, value);
        }

        IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.SetItems(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            return SetItems(items);
        }

        IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.AddRange(IEnumerable<KeyValuePair<TKey, TValue>> pairs)
        {
            return AddRange(pairs);
        }

        IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.RemoveRange(IEnumerable<TKey> keys)
        {
            return RemoveRange(keys);
        }

        IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.Remove(TKey key)
        {
            return Remove(key);
        }

        /// <summary>Determines whether this immutable sorted map contains the specified key.</summary>
        /// <param name="key">The key to locate.</param>
        /// <returns>
        ///   <see langword="true" /> if the immutable dictionary contains the specified key; otherwise, <see langword="false" />.</returns>
        public bool ContainsKey(TKey key)
        {
            Requires.NotNullAllowStructs(key, "key");
            return _root.ContainsKey(key, _keyComparer);
        }

        /// <summary>Determines whether this immutable sorted dictionary contains the specified key/value pair.</summary>
        /// <param name="pair">The key/value pair to locate.</param>
        /// <returns>
        ///   <see langword="true" /> if the specified key/value pair is found in the dictionary; otherwise, <see langword="false" />.</returns>
        public bool Contains(KeyValuePair<TKey, TValue> pair)
        {
            return _root.Contains(pair, _keyComparer, _valueComparer);
        }

        /// <summary>Gets the value associated with the specified key.</summary>
        /// <param name="key">The key whose value will be retrieved.</param>
        /// <param name="value">When this method returns, contains the value associated with the specified key, if the key is found; otherwise, contains the default value for the type of the <paramref name="value" /> parameter.</param>
        /// <returns>
        ///   <see langword="true" /> if the dictionary contains an element with the specified key; otherwise, <see langword="false" />.</returns>
        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
        {
            Requires.NotNullAllowStructs(key, "key");
            return _root.TryGetValue(key, _keyComparer, out value);
        }

        /// <summary>Determines whether this dictionary contains a specified key.</summary>
        /// <param name="equalKey">The key to search for.</param>
        /// <param name="actualKey">The matching key located in the dictionary if found, or <c>equalkey</c> if no match is found.</param>
        /// <returns>
        ///   <see langword="true" /> if a match for <paramref name="equalKey" /> is found; otherwise, <see langword="false" />.</returns>
        public bool TryGetKey(TKey equalKey, out TKey actualKey)
        {
            Requires.NotNullAllowStructs(equalKey, "equalKey");
            return _root.TryGetKey(equalKey, _keyComparer, out actualKey);
        }

        void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
        {
            throw new NotSupportedException();
        }

        bool IDictionary<TKey, TValue>.Remove(TKey key)
        {
            throw new NotSupportedException();
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            throw new NotSupportedException();
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Clear()
        {
            throw new NotSupportedException();
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            throw new NotSupportedException();
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            Requires.NotNull(array, "array");
            Requires.Range(arrayIndex >= 0, "arrayIndex");
            Requires.Range(array.Length >= arrayIndex + Count, "arrayIndex");
            using Enumerator enumerator = GetEnumerator();
            while (enumerator.MoveNext())
            {
                KeyValuePair<TKey, TValue> current = enumerator.Current;
                array[arrayIndex++] = current;
            }
        }

        /// <summary>Adds an element with the provided key and value to the dictionary object.</summary>
        /// <param name="key">The object to use as the key of the element to add.</param>
        /// <param name="value">The object to use as the value of the element to add.</param>
        void IDictionary.Add(object key, object value)
        {
            throw new NotSupportedException();
        }

        /// <summary>Determines whether the immutable dictionary object contains an element with the specified key.</summary>
        /// <param name="key">The key to locate in the dictionary object.</param>
        /// <returns>
        ///   <see langword="true" /> if the dictionary contains an element with the key; otherwise, <see langword="false" />.</returns>
        bool IDictionary.Contains(object key)
        {
            return ContainsKey((TKey)key);
        }

        /// <summary>Returns an <see cref="T:System.Collections.IDictionaryEnumerator" /> object for the immutable dictionary object.</summary>
        /// <returns>An enumerator object for the dictionary object.</returns>
        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return new DictionaryEnumerator<TKey, TValue>(GetEnumerator());
        }

        /// <summary>Removes the element with the specified key from the immutable dictionary object.</summary>
        /// <param name="key">The key of the element to remove.</param>
        void IDictionary.Remove(object key)
        {
            throw new NotSupportedException();
        }

        /// <summary>Clears this instance.</summary>
        /// <exception cref="T:System.NotSupportedException">The dictionary object is read-only.</exception>
        void IDictionary.Clear()
        {
            throw new NotSupportedException();
        }

        /// <summary>Copies the elements of the dictionary to an array, starting at a particular array index.</summary>
        /// <param name="array">The one-dimensional array that is the destination of the elements copied from the dictionary. The array must have zero-based indexing.</param>
        /// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        void ICollection.CopyTo(Array array, int index)
        {
            _root.CopyTo(array, index, Count);
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            if (!IsEmpty)
            {
                return GetEnumerator();
            }
            return Enumerable.Empty<KeyValuePair<TKey, TValue>>().GetEnumerator();
        }

        /// <summary>Returns an enumerator that iterates through a collection.</summary>
        /// <returns>An enumerator object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>Returns an enumerator that iterates through the immutable sorted dictionary.</summary>
        /// <returns>An enumerator that can be used to iterate through the dictionary.</returns>
        public Enumerator GetEnumerator()
        {
            return _root.GetEnumerator();
        }

        private static ImmutableSortedDictionary<TKey, TValue> Wrap(Node root, int count, IComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
        {
            if (!root.IsEmpty)
            {
                return new ImmutableSortedDictionary<TKey, TValue>(root, count, keyComparer, valueComparer);
            }
            return Empty.WithComparers(keyComparer, valueComparer);
        }

        private static bool TryCastToImmutableMap(IEnumerable<KeyValuePair<TKey, TValue>> sequence, [NotNullWhen(true)] out ImmutableSortedDictionary<TKey, TValue> other)
        {
            other = sequence as ImmutableSortedDictionary<TKey, TValue>;
            if (other != null)
            {
                return true;
            }
            if (sequence is Builder builder)
            {
                other = builder.ToImmutable();
                return true;
            }
            return false;
        }

        private ImmutableSortedDictionary<TKey, TValue> AddRange(IEnumerable<KeyValuePair<TKey, TValue>> items, bool overwriteOnCollision, bool avoidToSortedMap)
        {
            Requires.NotNull(items, "items");
            if (IsEmpty && !avoidToSortedMap)
            {
                return FillFromEmpty(items, overwriteOnCollision);
            }
            Node node = _root;
            int num = _count;
            foreach (KeyValuePair<TKey, TValue> item in items)
            {
                bool replacedExistingValue = false;
                bool mutated;
                Node node2 = (overwriteOnCollision ? node.SetItem(item.Key, item.Value, _keyComparer, _valueComparer, out replacedExistingValue, out mutated) : node.Add(item.Key, item.Value, _keyComparer, _valueComparer, out mutated));
                if (mutated)
                {
                    node = node2;
                    if (!replacedExistingValue)
                    {
                        num++;
                    }
                }
            }
            return Wrap(node, num);
        }

        private ImmutableSortedDictionary<TKey, TValue> Wrap(Node root, int adjustedCountIfDifferentRoot)
        {
            if (_root != root)
            {
                if (!root.IsEmpty)
                {
                    return new ImmutableSortedDictionary<TKey, TValue>(root, adjustedCountIfDifferentRoot, _keyComparer, _valueComparer);
                }
                return Clear();
            }
            return this;
        }

        private ImmutableSortedDictionary<TKey, TValue> FillFromEmpty(IEnumerable<KeyValuePair<TKey, TValue>> items, bool overwriteOnCollision)
        {
            Requires.NotNull(items, "items");
            if (TryCastToImmutableMap(items, out var other))
            {
                return other.WithComparers(KeyComparer, ValueComparer);
            }
            SortedDictionary<TKey, TValue> sortedDictionary;
            if (items is IDictionary<TKey, TValue> dictionary)
            {
                sortedDictionary = new SortedDictionary<TKey, TValue>(dictionary, KeyComparer);
            }
            else
            {
                sortedDictionary = new SortedDictionary<TKey, TValue>(KeyComparer);
                foreach (KeyValuePair<TKey, TValue> item in items)
                {
                    TValue value;
                    if (overwriteOnCollision)
                    {
                        sortedDictionary[item.Key] = item.Value;
                    }
                    else if (sortedDictionary.TryGetValue(item.Key, out value))
                    {
                        if (!_valueComparer.Equals(value, item.Value))
                        {
                            throw new ArgumentException(System.SR.Format(MDCFR.Properties.Resources.DuplicateKey, item.Key));
                        }
                    }
                    else
                    {
                        sortedDictionary.Add(item.Key, item.Value);
                    }
                }
            }
            if (sortedDictionary.Count == 0)
            {
                return this;
            }
            Node root = Node.NodeTreeFromSortedDictionary(sortedDictionary);
            return new ImmutableSortedDictionary<TKey, TValue>(root, sortedDictionary.Count, KeyComparer, ValueComparer);
        }
    }

    internal sealed class ImmutableSortedDictionaryBuilderDebuggerProxy<TKey, TValue> where TKey : notnull
    {
        private readonly ImmutableSortedDictionary<TKey, TValue>.Builder _map;

        private KeyValuePair<TKey, TValue>[] _contents;

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public KeyValuePair<TKey, TValue>[] Contents => _contents ?? (_contents = _map.ToArray(_map.Count));

        public ImmutableSortedDictionaryBuilderDebuggerProxy(ImmutableSortedDictionary<TKey, TValue>.Builder map)
        {
            Requires.NotNull(map, "map");
            _map = map;
        }
    }


    /// <summary>Provides a set of initialization methods for instances of the <see cref="System.Collections.Immutable.ImmutableSortedSet{T}" /> class. </summary>
    public static class ImmutableSortedSet
    {
        /// <summary>Creates an empty immutable sorted set.</summary>
        /// <typeparam name="T">The type of items to be stored in the immutable set.</typeparam>
        /// <returns>An empty immutable sorted set.</returns>
        public static ImmutableSortedSet<T> Create<T>()
        {
            return ImmutableSortedSet<T>.Empty;
        }

        /// <summary>Creates an empty immutable sorted set that uses the specified comparer.</summary>
        /// <param name="comparer">The implementation to use when comparing items in the set.</param>
        /// <typeparam name="T">The type of items in the immutable set.</typeparam>
        /// <returns>An empty immutable set.</returns>
        public static ImmutableSortedSet<T> Create<T>(IComparer<T>? comparer)
        {
            return ImmutableSortedSet<T>.Empty.WithComparer(comparer);
        }

        /// <summary>Creates a new immutable sorted set that contains the specified item.</summary>
        /// <param name="item">The item to prepopulate the set with.</param>
        /// <typeparam name="T">The type of items in the immutable set.</typeparam>
        /// <returns>A new immutable set that contains the specified item.</returns>
        public static ImmutableSortedSet<T> Create<T>(T item)
        {
            return ImmutableSortedSet<T>.Empty.Add(item);
        }

        /// <summary>Creates a new immutable sorted set that contains the specified item and uses the specified comparer.</summary>
        /// <param name="comparer">The implementation to use when comparing items in the set.</param>
        /// <param name="item">The item to prepopulate the set with.</param>
        /// <typeparam name="T">The type of items stored in the immutable set.</typeparam>
        /// <returns>A new immutable set that contains the specified item.</returns>
        public static ImmutableSortedSet<T> Create<T>(IComparer<T>? comparer, T item)
        {
            return ImmutableSortedSet<T>.Empty.WithComparer(comparer).Add(item);
        }

        /// <summary>Creates a new immutable collection that contains the specified items.</summary>
        /// <param name="items">The items to add to the set with before it's immutable.</param>
        /// <typeparam name="T">The type of items stored by the collection.</typeparam>
        /// <returns>The new immutable set that contains the specified items.</returns>
        public static ImmutableSortedSet<T> CreateRange<T>(IEnumerable<T> items)
        {
            return ImmutableSortedSet<T>.Empty.Union(items);
        }

        /// <summary>Creates a new immutable collection that contains the specified items.</summary>
        /// <param name="comparer">The comparer to use to compare elements in this set.</param>
        /// <param name="items">The items to add to the set before it's immutable.</param>
        /// <typeparam name="T">The type of items stored by the collection.</typeparam>
        /// <returns>The new immutable set that contains the specified items.</returns>
        public static ImmutableSortedSet<T> CreateRange<T>(IComparer<T>? comparer, IEnumerable<T> items)
        {
            return ImmutableSortedSet<T>.Empty.WithComparer(comparer).Union(items);
        }

        /// <summary>Creates a new immutable sorted set that contains the specified array of items.</summary>
        /// <param name="items">An array that contains the items to prepopulate the set with.</param>
        /// <typeparam name="T">The type of items in the immutable set.</typeparam>
        /// <returns>A new immutable set that contains the specified items.</returns>
        public static ImmutableSortedSet<T> Create<T>(params T[] items)
        {
            return ImmutableSortedSet<T>.Empty.Union(items);
        }

        /// <summary>Creates a new immutable sorted set that contains the specified array of items and uses the specified comparer.</summary>
        /// <param name="comparer">The implementation to use when comparing items in the set.</param>
        /// <param name="items">An array that contains the items to prepopulate the set with.</param>
        /// <typeparam name="T">The type of items in the immutable set.</typeparam>
        /// <returns>A new immutable set that contains the specified items.</returns>
        public static ImmutableSortedSet<T> Create<T>(IComparer<T>? comparer, params T[] items)
        {
            return ImmutableSortedSet<T>.Empty.WithComparer(comparer).Union(items);
        }

        /// <summary>Returns a collection that can be used to build an immutable sorted set.</summary>
        /// <typeparam name="T">The type of items stored by the collection.</typeparam>
        /// <returns>The immutable collection builder.</returns>
        public static ImmutableSortedSet<T>.Builder CreateBuilder<T>()
        {
            return Create<T>().ToBuilder();
        }

        /// <summary>Returns a collection that can be used to build an immutable sorted set.</summary>
        /// <param name="comparer">The comparer used to compare items in the set for equality.</param>
        /// <typeparam name="T">The type of items stored by the collection.</typeparam>
        /// <returns>The immutable collection.</returns>
        public static ImmutableSortedSet<T>.Builder CreateBuilder<T>(IComparer<T>? comparer)
        {
            return Create(comparer).ToBuilder();
        }

        /// <summary>Enumerates a sequence, produces an immutable sorted set of its contents, and uses the specified comparer.</summary>
        /// <param name="source">The sequence to enumerate.</param>
        /// <param name="comparer">The comparer to use for initializing and adding members to the sorted set.</param>
        /// <typeparam name="TSource">The type of the elements in the sequence.</typeparam>
        /// <returns>An immutable sorted set that contains the items in the specified sequence.</returns>
        public static ImmutableSortedSet<TSource> ToImmutableSortedSet<TSource>(this IEnumerable<TSource> source, IComparer<TSource>? comparer)
        {
            if (source is ImmutableSortedSet<TSource> immutableSortedSet)
            {
                return immutableSortedSet.WithComparer(comparer);
            }
            return ImmutableSortedSet<TSource>.Empty.WithComparer(comparer).Union(source);
        }

        /// <summary>Enumerates a sequence and produces an immutable sorted set of its contents.</summary>
        /// <param name="source">The sequence to enumerate.</param>
        /// <typeparam name="TSource">The type of the elements in the sequence.</typeparam>
        /// <returns>An immutable sorted set that contains the items in the specified sequence.</returns>
        public static ImmutableSortedSet<TSource> ToImmutableSortedSet<TSource>(this IEnumerable<TSource> source)
        {
            return source.ToImmutableSortedSet(null);
        }

        /// <summary>Creates an immutable sorted set from the current contents of the builder's set.</summary>
        /// <param name="builder">The builder to create the immutable sorted set from.</param>
        /// <typeparam name="TSource">The type of the elements in the immutable sorted set.</typeparam>
        /// <returns>An immutable sorted set that contains the current contents in the builder's set.</returns>
        public static ImmutableSortedSet<TSource> ToImmutableSortedSet<TSource>(this ImmutableSortedSet<TSource>.Builder builder)
        {
            Requires.NotNull(builder, "builder");
            return builder.ToImmutable();
        }
    }
   
    /// <summary>Represents an immutable sorted set implementation. </summary>
    /// <typeparam name="T">The type of elements in the set.</typeparam>
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(ImmutableEnumerableDebuggerProxy<>))]
    public sealed class ImmutableSortedSet<T> : IImmutableSet<T>, IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable, ISortKeyCollection<T>, IReadOnlyList<T>, IList<T>, ICollection<T>, ISet<T>, IList, ICollection, IStrongEnumerable<T, ImmutableSortedSet<T>.Enumerator>
    {
        /// <summary>Represents a sorted set that enables changes with little or no memory allocations, and efficiently manipulates or builds immutable sorted sets. </summary>
        [DebuggerDisplay("Count = {Count}")]
        [DebuggerTypeProxy(typeof(ImmutableSortedSetBuilderDebuggerProxy<>))]
        public sealed class Builder : ISortKeyCollection<T>, IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable, ISet<T>, ICollection<T>, ICollection
        {
            private Node _root = Node.EmptyNode;

            private IComparer<T> _comparer = Comparer<T>.Default;

            private ImmutableSortedSet<T> _immutable;

            private int _version;

            private object _syncRoot;

            /// <summary>Gets the number of elements in the immutable sorted set.</summary>
            /// <returns>The number of elements in this set.</returns>
            public int Count => Root.Count;

            /// <summary>Gets a value that indicates whether this instance is read-only.</summary>
            /// <returns>Always <see langword="false" />.</returns>
            bool ICollection<T>.IsReadOnly => false;

            /// <summary>Gets the element of the set at the given index.</summary>
            /// <param name="index">The 0-based index of the element in the set to return.</param>
            /// <returns>The element at the given position.</returns>
            public T this[int index] => _root.ItemRef(index);

            /// <summary>Gets the maximum value in the immutable sorted set, as defined by the comparer.</summary>
            /// <returns>The maximum value in the set.</returns>
            public T? Max => _root.Max;

            /// <summary>Gets the minimum value in the immutable sorted set, as defined by the comparer.</summary>
            /// <returns>The minimum value in the set.</returns>
            public T? Min => _root.Min;

            /// <summary>Gets or sets the object that is used to determine equality for the values in the immutable sorted set.</summary>
            /// <returns>The comparer that is used to determine equality for the values in the set.</returns>
            public IComparer<T> KeyComparer
            {
                get
                {
                    return _comparer;
                }
                set
                {
                    Requires.NotNull(value, "value");
                    if (value == _comparer)
                    {
                        return;
                    }
                    Node node = Node.EmptyNode;
                    using (Enumerator enumerator = GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            T current = enumerator.Current;
                            node = node.Add(current, value, out var _);
                        }
                    }
                    _immutable = null;
                    _comparer = value;
                    Root = node;
                }
            }

            internal int Version => _version;

            private Node Root
            {
                get
                {
                    return _root;
                }
                set
                {
                    _version++;
                    if (_root != value)
                    {
                        _root = value;
                        _immutable = null;
                    }
                }
            }

            /// <summary>Gets a value that indicates whether access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread-safe).</summary>
            /// <returns>
            ///   <see langword="true" /> if access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread-safe); otherwise, <see langword="false" />.</returns>
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            bool ICollection.IsSynchronized => false;

            /// <summary>Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.</summary>
            /// <returns>An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.</returns>
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            object ICollection.SyncRoot
            {
                get
                {
                    if (_syncRoot == null)
                    {
                        Interlocked.CompareExchange<object>(ref _syncRoot, new object(), (object)null);
                    }
                    return _syncRoot;
                }
            }

            internal Builder(ImmutableSortedSet<T> set)
            {
                Requires.NotNull(set, "set");
                _root = set._root;
                _comparer = set.KeyComparer;
                _immutable = set;
            }

            /// <summary>Gets a read-only reference to the element of the set at the given <paramref name="index" />.</summary>
            /// <param name="index">The 0-based index of the element in the set to return.</param>
            /// <returns>A read-only reference to the element at the given position.</returns>
            public ref readonly T ItemRef(int index)
            {
                return ref _root.ItemRef(index);
            }

            /// <summary>Adds an element to the current set and returns a value to indicate whether the element was successfully added.</summary>
            /// <param name="item">The element to add to the set.</param>
            /// <returns>
            ///   <see langword="true" /> if the element is added to the set; <see langword="false" /> if the element is already in the set.</returns>
            public bool Add(T item)
            {
                Root = Root.Add(item, _comparer, out var mutated);
                return mutated;
            }

            /// <summary>Removes the specified set of items from the current set.</summary>
            /// <param name="other">The collection of items to remove from the set.</param>
            public void ExceptWith(IEnumerable<T> other)
            {
                Requires.NotNull(other, "other");
                foreach (T item in other)
                {
                    Root = Root.Remove(item, _comparer, out var _);
                }
            }

            /// <summary>Modifies the current set so that it contains only elements that are also in a specified collection.</summary>
            /// <param name="other">The collection to compare to the current set.</param>
            public void IntersectWith(IEnumerable<T> other)
            {
                Requires.NotNull(other, "other");
                Node node = Node.EmptyNode;
                foreach (T item in other)
                {
                    if (Contains(item))
                    {
                        node = node.Add(item, _comparer, out var _);
                    }
                }
                Root = node;
            }

            /// <summary>Determines whether the current set is a proper (strict) subset of a specified collection.</summary>
            /// <param name="other">The collection to compare to the current set.</param>
            /// <returns>
            ///   <see langword="true" /> if the current set is a proper subset of <paramref name="other" />; otherwise, <see langword="false" />.</returns>
            public bool IsProperSubsetOf(IEnumerable<T> other)
            {
                return ToImmutable().IsProperSubsetOf(other);
            }

            /// <summary>Determines whether the current set is a proper (strict) superset of a specified collection.</summary>
            /// <param name="other">The collection to compare to the current set.</param>
            /// <returns>
            ///   <see langword="true" /> if the current set is a proper superset of <paramref name="other" />; otherwise, <see langword="false" />.</returns>
            public bool IsProperSupersetOf(IEnumerable<T> other)
            {
                return ToImmutable().IsProperSupersetOf(other);
            }

            /// <summary>Determines whether the current set is a subset of a specified collection.</summary>
            /// <param name="other">The collection is compare to the current set.</param>
            /// <returns>
            ///   <see langword="true" /> if the current set is a subset of <paramref name="other" />; otherwise, <see langword="false" />.</returns>
            public bool IsSubsetOf(IEnumerable<T> other)
            {
                return ToImmutable().IsSubsetOf(other);
            }

            /// <summary>Determines whether the current set is a superset of a specified collection.</summary>
            /// <param name="other">The collection to compare to the current set.</param>
            /// <returns>
            ///   <see langword="true" /> if the current set is a superset of <paramref name="other" />; otherwise, <see langword="false" />.</returns>
            public bool IsSupersetOf(IEnumerable<T> other)
            {
                return ToImmutable().IsSupersetOf(other);
            }

            /// <summary>Determines whether the current set overlaps with the specified collection.</summary>
            /// <param name="other">The collection to compare to the current set.</param>
            /// <returns>
            ///   <see langword="true" /> if the current set and <paramref name="other" /> share at least one common element; otherwise, <see langword="false" />.</returns>
            public bool Overlaps(IEnumerable<T> other)
            {
                return ToImmutable().Overlaps(other);
            }

            /// <summary>Determines whether the current set and the specified collection contain the same elements.</summary>
            /// <param name="other">The collection to compare to the current set.</param>
            /// <returns>
            ///   <see langword="true" /> if the current set is equal to <paramref name="other" />; otherwise, <see langword="false" />.</returns>
            public bool SetEquals(IEnumerable<T> other)
            {
                return ToImmutable().SetEquals(other);
            }

            /// <summary>Modifies the current set so that it contains only elements that are present either in the current set or in the specified collection, but not both.</summary>
            /// <param name="other">The collection to compare to the current set.</param>
            public void SymmetricExceptWith(IEnumerable<T> other)
            {
                Root = ToImmutable().SymmetricExcept(other)._root;
            }

            /// <summary>Modifies the current set so that it contains all elements that are present in both the current set and in the specified collection.</summary>
            /// <param name="other">The collection to compare to the current state.</param>
            public void UnionWith(IEnumerable<T> other)
            {
                Requires.NotNull(other, "other");
                foreach (T item in other)
                {
                    Root = Root.Add(item, _comparer, out var _);
                }
            }

            /// <summary>Adds an element to the current set and returns a value to indicate whether the element was successfully added.</summary>
            /// <param name="item">The element to add to the set.</param>
            void ICollection<T>.Add(T item)
            {
                Add(item);
            }

            /// <summary>Removes all elements from this set.</summary>
            public void Clear()
            {
                Root = Node.EmptyNode;
            }

            /// <summary>Determines whether the set contains the specified object.</summary>
            /// <param name="item">The object to locate in the set.</param>
            /// <returns>
            ///   <see langword="true" /> if <paramref name="item" /> is found in the set; otherwise, <see langword="false" />.</returns>
            public bool Contains(T item)
            {
                return Root.Contains(item, _comparer);
            }

            /// <summary>Copies the elements of the collection to an array, starting at a particular array index.</summary>
            /// <param name="array">The one-dimensional array that is the destination of the elements copied from collection. The array must have zero-based indexing.</param>
            /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
            void ICollection<T>.CopyTo(T[] array, int arrayIndex)
            {
                _root.CopyTo(array, arrayIndex);
            }

            /// <summary>Removes the first occurrence of the specified object from the set.</summary>
            /// <param name="item">The object to remove from the set.</param>
            /// <returns>
            ///   <see langword="true" /> if <paramref name="item" /> was removed from the set; <see langword="false" /> if <paramref name="item" /> was not found in the set.</returns>
            public bool Remove(T item)
            {
                Root = Root.Remove(item, _comparer, out var mutated);
                return mutated;
            }

            /// <summary>Returns an enumerator that iterates through the set.</summary>
            /// <returns>A enumerator that can be used to iterate through the set.</returns>
            public ImmutableSortedSet<T>.Enumerator GetEnumerator()
            {
                return Root.GetEnumerator(this);
            }

            /// <summary>Returns an enumerator that iterates through the collection.</summary>
            /// <returns>A enumerator that can be used to iterate through the collection.</returns>
            IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                return Root.GetEnumerator();
            }

            /// <summary>Returns an enumerator that iterates through the collection.</summary>
            /// <returns>A enumerator that can be used to iterate through the collection.</returns>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            /// <summary>Searches for the first index within this set that the specified value is contained.</summary>
            /// <param name="item">The value to locate within the set.</param>
            /// <returns>The index of the specified <paramref name="item" /> in the sorted set, if <paramref name="item" /> is found.  If <paramref name="item" /> is not found and <paramref name="item" /> is less than one or more elements in this set, returns a negative number that is the bitwise complement of the index of the first element that's larger than <paramref name="item" />. If <paramref name="item" /> is not found and <paramref name="item" /> is greater than any of the elements in the set, returns a negative number that is the bitwise complement of (the index of the last element plus 1).</returns>
            public int IndexOf(T item)
            {
                return Root.IndexOf(item, _comparer);
            }

            /// <summary>Returns an enumerator that iterates over the immutable sorted set in reverse order.</summary>
            /// <returns>An enumerator that iterates over the set in reverse order.</returns>
            public IEnumerable<T> Reverse()
            {
                return new ReverseEnumerable(_root);
            }

            /// <summary>Creates an immutable sorted set based on the contents of this instance.</summary>
            /// <returns>An immutable set.</returns>
            public ImmutableSortedSet<T> ToImmutable()
            {
                return _immutable ?? (_immutable = ImmutableSortedSet<T>.Wrap(Root, _comparer));
            }

            /// <summary>Searches the set for a given value and returns the equal value it finds, if any.</summary>
            /// <param name="equalValue">The value for which to search.</param>
            /// <param name="actualValue">The value from the set that the search found, or the original value if the search yielded no match.</param>
            /// <returns>A value indicating whether the search was successful.</returns>
            public bool TryGetValue(T equalValue, out T actualValue)
            {
                Node node = _root.Search(equalValue, _comparer);
                if (!node.IsEmpty)
                {
                    actualValue = node.Key;
                    return true;
                }
                actualValue = equalValue;
                return false;
            }

            /// <summary>Copies the elements of the set to an array, starting at a particular array index.</summary>
            /// <param name="array">The one-dimensional array that is the destination of the elements copied from the set. The array must have zero-based indexing.</param>
            /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
            void ICollection.CopyTo(Array array, int arrayIndex)
            {
                Root.CopyTo(array, arrayIndex);
            }
        }

        private sealed class ReverseEnumerable : IEnumerable<T>, IEnumerable
        {
            private readonly Node _root;

            internal ReverseEnumerable(Node root)
            {
                Requires.NotNull(root, "root");
                _root = root;
            }

            public IEnumerator<T> GetEnumerator()
            {
                return _root.Reverse();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        /// <summary>Enumerates the contents of a binary tree.  </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator, ISecurePooledObjectUser, IStrongEnumerator<T>
        {
            private readonly Builder _builder;

            private readonly int _poolUserId;

            private readonly bool _reverse;

            private Node _root;

            private SecurePooledObject<Stack<RefAsValueType<Node>>> _stack;

            private Node _current;

            private int _enumeratingBuilderVersion;

            int ISecurePooledObjectUser.PoolUserId => _poolUserId;

            /// <summary>Gets the element at the current position of the enumerator.  
            ///
            ///  NuGet package: System.Collections.Immutable (about immutable collections and how to install)</summary>
            /// <returns>The element at the current position of the enumerator.</returns>
            public T Current
            {
                get
                {
                    ThrowIfDisposed();
                    if (_current != null)
                    {
                        return _current.Value;
                    }
                    throw new InvalidOperationException();
                }
            }

            /// <summary>The current element.</summary>
            /// <returns>The element in the collection at the current position of the enumerator.</returns>
            object? IEnumerator.Current => Current;

            internal Enumerator(Node root, Builder? builder = null, bool reverse = false)
            {
                Requires.NotNull(root, "root");
                _root = root;
                _builder = builder;
                _current = null;
                _reverse = reverse;
                _enumeratingBuilderVersion = builder?.Version ?? (-1);
                _poolUserId = SecureObjectPool.NewId();
                _stack = null;
                if (!SecureObjectPool<Stack<RefAsValueType<Node>>, Enumerator>.TryTake(this, out _stack))
                {
                    _stack = SecureObjectPool<Stack<RefAsValueType<Node>>, Enumerator>.PrepNew(this, new Stack<RefAsValueType<Node>>(root.Height));
                }
                PushNext(_root);
            }

            /// <summary>Releases the resources used by the current instance of the <see cref="T:System.Collections.Immutable.ImmutableSortedSet`1.Enumerator" /> class.  
            ///
            ///  NuGet package: System.Collections.Immutable (about immutable collections and how to install)</summary>
            public void Dispose()
            {
                _root = null;
                _current = null;
                if (_stack != null && _stack.TryUse(ref this, out var value))
                {
                    value.ClearFastWhenEmpty();
                    SecureObjectPool<Stack<RefAsValueType<Node>>, Enumerator>.TryAdd(this, _stack);
                    _stack = null;
                }
            }

            /// <summary>Advances the enumerator to the next element of the immutable sorted set.  
            ///
            ///  NuGet package: System.Collections.Immutable (about immutable collections and how to install)</summary>
            /// <returns>
            ///   <see langword="true" /> if the enumerator was successfully advanced to the next element; <see langword="false" /> if the enumerator has passed the end of the sorted set.</returns>
            public bool MoveNext()
            {
                ThrowIfDisposed();
                ThrowIfChanged();
                Stack<RefAsValueType<Node>> stack = _stack.Use(ref this);
                if (stack.Count > 0)
                {
                    Node node = (_current = stack.Pop().Value);
                    PushNext(_reverse ? node.Left : node.Right);
                    return true;
                }
                _current = null;
                return false;
            }

            /// <summary>Sets the enumerator to its initial position, which is before the first element in the immutable sorted set.  
            ///
            ///  NuGet package: System.Collections.Immutable (about immutable collections and how to install)</summary>
            public void Reset()
            {
                ThrowIfDisposed();
                _enumeratingBuilderVersion = ((_builder != null) ? _builder.Version : (-1));
                _current = null;
                Stack<RefAsValueType<Node>> stack = _stack.Use(ref this);
                stack.ClearFastWhenEmpty();
                PushNext(_root);
            }

            private void ThrowIfDisposed()
            {
                if (_root == null || (_stack != null && !_stack.IsOwned(ref this)))
                {
                    Requires.FailObjectDisposed(this);
                }
            }

            private void ThrowIfChanged()
            {
                if (_builder != null && _builder.Version != _enumeratingBuilderVersion)
                {
                    throw new InvalidOperationException(MDCFR.Properties.Resources.CollectionModifiedDuringEnumeration);
                }
            }

            private void PushNext(Node node)
            {
                Requires.NotNull(node, "node");
                Stack<RefAsValueType<Node>> stack = _stack.Use(ref this);
                while (!node.IsEmpty)
                {
                    stack.Push(new RefAsValueType<Node>(node));
                    node = (_reverse ? node.Right : node.Left);
                }
            }
        }

        [DebuggerDisplay("{_key}")]
        internal sealed class Node : IBinaryTree<T>, IBinaryTree, IEnumerable<T>, IEnumerable
        {
            internal static readonly Node EmptyNode = new Node();

            private readonly T _key;

            private bool _frozen;

            private byte _height;

            private int _count;

            private Node _left;

            private Node _right;

            public bool IsEmpty => _left == null;

            public int Height => _height;

            public Node? Left => _left;

            IBinaryTree? IBinaryTree.Left => _left;

            public Node? Right => _right;

            IBinaryTree? IBinaryTree.Right => _right;

            IBinaryTree<T>? IBinaryTree<T>.Left => _left;

            IBinaryTree<T>? IBinaryTree<T>.Right => _right;

            public T Value => _key;

            public int Count => _count;

            internal T Key => _key;

            internal T? Max
            {
                get
                {
                    if (IsEmpty)
                    {
                        return default(T);
                    }
                    Node node = this;
                    while (!node._right.IsEmpty)
                    {
                        node = node._right;
                    }
                    return node._key;
                }
            }

            internal T? Min
            {
                get
                {
                    if (IsEmpty)
                    {
                        return default(T);
                    }
                    Node node = this;
                    while (!node._left.IsEmpty)
                    {
                        node = node._left;
                    }
                    return node._key;
                }
            }

            internal T this[int index]
            {
                get
                {
                    Requires.Range(index >= 0 && index < Count, "index");
                    if (index < _left._count)
                    {
                        return _left[index];
                    }
                    if (index > _left._count)
                    {
                        return _right[index - _left._count - 1];
                    }
                    return _key;
                }
            }

            private Node()
            {
                _frozen = true;
            }

            private Node(T key, Node left, Node right, bool frozen = false)
            {
                Requires.NotNull(left, "left");
                Requires.NotNull(right, "right");
                _key = key;
                _left = left;
                _right = right;
                checked
                {
                    _height = (byte)(1 + unchecked((int)Math.Max(left._height, right._height)));
                }
                _count = 1 + left._count + right._count;
                _frozen = frozen;
            }

            internal ref readonly T ItemRef(int index)
            {
                Requires.Range(index >= 0 && index < Count, "index");
                return ref ItemRefUnchecked(index);
            }

            private ref readonly T ItemRefUnchecked(int index)
            {
                if (index < _left._count)
                {
                    return ref _left.ItemRefUnchecked(index);
                }
                if (index > _left._count)
                {
                    return ref _right.ItemRefUnchecked(index - _left._count - 1);
                }
                return ref _key;
            }

            public Enumerator GetEnumerator()
            {
                return new Enumerator(this);
            }

            [ExcludeFromCodeCoverage]
            IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                return GetEnumerator();
            }

            [ExcludeFromCodeCoverage]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            internal Enumerator GetEnumerator(Builder builder)
            {
                return new Enumerator(this, builder);
            }

            internal void CopyTo(T[] array, int arrayIndex)
            {
                Requires.NotNull(array, "array");
                Requires.Range(arrayIndex >= 0, "arrayIndex");
                Requires.Range(array.Length >= arrayIndex + Count, "arrayIndex");
                using Enumerator enumerator = GetEnumerator();
                while (enumerator.MoveNext())
                {
                    T current = enumerator.Current;
                    array[arrayIndex++] = current;
                }
            }

            internal void CopyTo(Array array, int arrayIndex)
            {
                Requires.NotNull(array, "array");
                Requires.Range(arrayIndex >= 0, "arrayIndex");
                Requires.Range(array.Length >= arrayIndex + Count, "arrayIndex");
                using Enumerator enumerator = GetEnumerator();
                while (enumerator.MoveNext())
                {
                    T current = enumerator.Current;
                    array.SetValue(current, arrayIndex++);
                }
            }

            internal Node Add(T key, IComparer<T> comparer, out bool mutated)
            {
                Requires.NotNull(comparer, "comparer");
                if (IsEmpty)
                {
                    mutated = true;
                    return new Node(key, this, this);
                }
                Node node = this;
                int num = comparer.Compare(key, _key);
                if (num > 0)
                {
                    Node right = _right.Add(key, comparer, out mutated);
                    if (mutated)
                    {
                        node = Mutate(null, right);
                    }
                }
                else
                {
                    if (num >= 0)
                    {
                        mutated = false;
                        return this;
                    }
                    Node left = _left.Add(key, comparer, out mutated);
                    if (mutated)
                    {
                        node = Mutate(left);
                    }
                }
                if (!mutated)
                {
                    return node;
                }
                return MakeBalanced(node);
            }

            internal Node Remove(T key, IComparer<T> comparer, out bool mutated)
            {
                Requires.NotNull(comparer, "comparer");
                if (IsEmpty)
                {
                    mutated = false;
                    return this;
                }
                Node node = this;
                int num = comparer.Compare(key, _key);
                if (num == 0)
                {
                    mutated = true;
                    if (_right.IsEmpty && _left.IsEmpty)
                    {
                        node = EmptyNode;
                    }
                    else if (_right.IsEmpty && !_left.IsEmpty)
                    {
                        node = _left;
                    }
                    else if (!_right.IsEmpty && _left.IsEmpty)
                    {
                        node = _right;
                    }
                    else
                    {
                        Node node2 = _right;
                        while (!node2._left.IsEmpty)
                        {
                            node2 = node2._left;
                        }
                        bool mutated2;
                        Node right = _right.Remove(node2._key, comparer, out mutated2);
                        node = node2.Mutate(_left, right);
                    }
                }
                else if (num < 0)
                {
                    Node left = _left.Remove(key, comparer, out mutated);
                    if (mutated)
                    {
                        node = Mutate(left);
                    }
                }
                else
                {
                    Node right2 = _right.Remove(key, comparer, out mutated);
                    if (mutated)
                    {
                        node = Mutate(null, right2);
                    }
                }
                if (!node.IsEmpty)
                {
                    return MakeBalanced(node);
                }
                return node;
            }

            internal bool Contains(T key, IComparer<T> comparer)
            {
                Requires.NotNull(comparer, "comparer");
                return !Search(key, comparer).IsEmpty;
            }

            internal void Freeze()
            {
                if (!_frozen)
                {
                    _left.Freeze();
                    _right.Freeze();
                    _frozen = true;
                }
            }

            internal Node Search(T key, IComparer<T> comparer)
            {
                Requires.NotNull(comparer, "comparer");
                if (IsEmpty)
                {
                    return this;
                }
                int num = comparer.Compare(key, _key);
                if (num == 0)
                {
                    return this;
                }
                if (num > 0)
                {
                    return _right.Search(key, comparer);
                }
                return _left.Search(key, comparer);
            }

            internal int IndexOf(T key, IComparer<T> comparer)
            {
                Requires.NotNull(comparer, "comparer");
                if (IsEmpty)
                {
                    return -1;
                }
                int num = comparer.Compare(key, _key);
                if (num == 0)
                {
                    return _left.Count;
                }
                if (num > 0)
                {
                    int num2 = _right.IndexOf(key, comparer);
                    bool flag = num2 < 0;
                    if (flag)
                    {
                        num2 = ~num2;
                    }
                    num2 = _left.Count + 1 + num2;
                    if (flag)
                    {
                        num2 = ~num2;
                    }
                    return num2;
                }
                return _left.IndexOf(key, comparer);
            }

            internal IEnumerator<T> Reverse()
            {
                return new Enumerator(this, null, reverse: true);
            }

            private static Node RotateLeft(Node tree)
            {
                Requires.NotNull(tree, "tree");
                if (tree._right.IsEmpty)
                {
                    return tree;
                }
                Node right = tree._right;
                return right.Mutate(tree.Mutate(null, right._left));
            }

            private static Node RotateRight(Node tree)
            {
                Requires.NotNull(tree, "tree");
                if (tree._left.IsEmpty)
                {
                    return tree;
                }
                Node left = tree._left;
                return left.Mutate(null, tree.Mutate(left._right));
            }

            private static Node DoubleLeft(Node tree)
            {
                Requires.NotNull(tree, "tree");
                if (tree._right.IsEmpty)
                {
                    return tree;
                }
                Node tree2 = tree.Mutate(null, RotateRight(tree._right));
                return RotateLeft(tree2);
            }

            private static Node DoubleRight(Node tree)
            {
                Requires.NotNull(tree, "tree");
                if (tree._left.IsEmpty)
                {
                    return tree;
                }
                Node tree2 = tree.Mutate(RotateLeft(tree._left));
                return RotateRight(tree2);
            }

            private static int Balance(Node tree)
            {
                Requires.NotNull(tree, "tree");
                return tree._right._height - tree._left._height;
            }

            private static bool IsRightHeavy(Node tree)
            {
                Requires.NotNull(tree, "tree");
                return Balance(tree) >= 2;
            }

            private static bool IsLeftHeavy(Node tree)
            {
                Requires.NotNull(tree, "tree");
                return Balance(tree) <= -2;
            }

            private static Node MakeBalanced(Node tree)
            {
                Requires.NotNull(tree, "tree");
                if (IsRightHeavy(tree))
                {
                    if (Balance(tree._right) >= 0)
                    {
                        return RotateLeft(tree);
                    }
                    return DoubleLeft(tree);
                }
                if (IsLeftHeavy(tree))
                {
                    if (Balance(tree._left) <= 0)
                    {
                        return RotateRight(tree);
                    }
                    return DoubleRight(tree);
                }
                return tree;
            }

            internal static Node NodeTreeFromList(IOrderedCollection<T> items, int start, int length)
            {
                Requires.NotNull(items, "items");
                if (length == 0)
                {
                    return EmptyNode;
                }
                int num = (length - 1) / 2;
                int num2 = length - 1 - num;
                Node left = NodeTreeFromList(items, start, num2);
                Node right = NodeTreeFromList(items, start + num2 + 1, num);
                return new Node(items[start + num2], left, right, frozen: true);
            }

            private Node Mutate(Node left = null, Node right = null)
            {
                if (_frozen)
                {
                    return new Node(_key, left ?? _left, right ?? _right);
                }
                if (left != null)
                {
                    _left = left;
                }
                if (right != null)
                {
                    _right = right;
                }
                checked
                {
                    _height = (byte)(1 + unchecked((int)Math.Max(_left._height, _right._height)));
                }
                _count = 1 + _left._count + _right._count;
                return this;
            }
        }

        private const float RefillOverIncrementalThreshold = 0.15f;

        /// <summary>Gets an empty immutable sorted set.</summary>
        public static readonly ImmutableSortedSet<T> Empty = new ImmutableSortedSet<T>();

        private readonly Node _root;

        private readonly IComparer<T> _comparer;

        /// <summary>Gets the maximum value in the immutable sorted set, as defined by the comparer.</summary>
        /// <returns>The maximum value in the set.</returns>
        public T? Max => _root.Max;

        /// <summary>Gets the minimum value in the immutable sorted set, as defined by the comparer.</summary>
        /// <returns>The minimum value in the set.</returns>
        public T? Min => _root.Min;

        /// <summary>Gets a value that indicates whether this immutable sorted set is empty.</summary>
        /// <returns>
        ///   <see langword="true" /> if this set is empty; otherwise, <see langword="false" />.</returns>
        public bool IsEmpty => _root.IsEmpty;

        /// <summary>Gets the number of elements in the immutable sorted set.</summary>
        /// <returns>The number of elements in the immutable sorted set.</returns>
        public int Count => _root.Count;

        /// <summary>Gets the comparer used to sort keys in the immutable sorted set.</summary>
        /// <returns>The comparer used to sort keys.</returns>
        public IComparer<T> KeyComparer => _comparer;

        internal IBinaryTree Root => _root;

        /// <summary>Gets the element of the immutable sorted set at the given index.</summary>
        /// <param name="index">The index of the element to retrieve from the sorted set.</param>
        /// <returns>The element at the given index.</returns>
        public T this[int index] => _root.ItemRef(index);

        /// <summary>Returns true, since immutable collections are always read-only. See the <see cref="T:System.Collections.Generic.ICollection`1" /> interface.</summary>
        /// <returns>A boolean value indicating whether the collection is read-only.</returns>
        bool ICollection<T>.IsReadOnly => true;

        /// <summary>See the <see cref="T:System.Collections.Generic.IList`1" /> interface.</summary>
        /// <param name="index">The zero-based index of the item to access.</param>
        /// <returns>The element stored at the specified index.</returns>
        T IList<T>.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>Gets a value that indicates whether the <see cref="T:System.Collections.IList" /> has a fixed size.</summary>
        /// <returns>
        ///   <see langword="true" /> if the <see cref="T:System.Collections.IList" /> has a fixed size; otherwise, <see langword="false" />.</returns>
        bool IList.IsFixedSize => true;

        /// <summary>Gets a value that indicates whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</summary>
        /// <returns>
        ///   <see langword="true" /> if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, <see langword="false" />.</returns>
        bool IList.IsReadOnly => true;

        /// <summary>See <see cref="T:System.Collections.ICollection" />.</summary>
        /// <returns>Object used for synchronizing access to the collection.</returns>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        object ICollection.SyncRoot => this;

        /// <summary>Returns true, since immutable collections are always thread-safe. See the <see cref="T:System.Collections.ICollection" /> interface.</summary>
        /// <returns>A boolean value indicating whether the collection is thread-safe.</returns>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool ICollection.IsSynchronized => true;

        /// <summary>Gets or sets the <see cref="T:System.Object" /> at the specified index.</summary>
        /// <param name="index">The index.</param>
        /// <exception cref="T:System.NotSupportedException" />
        /// <returns>The <see cref="T:System.Object" />.</returns>
        object? IList.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        internal ImmutableSortedSet(IComparer<T>? comparer = null)
        {
            _root = Node.EmptyNode;
            _comparer = comparer ?? Comparer<T>.Default;
        }

        private ImmutableSortedSet(Node root, IComparer<T> comparer)
        {
            Requires.NotNull(root, "root");
            Requires.NotNull(comparer, "comparer");
            root.Freeze();
            _root = root;
            _comparer = comparer;
        }

        /// <summary>Removes all elements from the immutable sorted set.</summary>
        /// <returns>An empty set with the elements removed.</returns>
        public ImmutableSortedSet<T> Clear()
        {
            if (!_root.IsEmpty)
            {
                return Empty.WithComparer(_comparer);
            }
            return this;
        }

        /// <summary>Gets a read-only reference of the element of the set at the given <paramref name="index" />.</summary>
        /// <param name="index">The 0-based index of the element in the set to return.</param>
        /// <returns>A read-only reference of the element at the given position.</returns>
        public ref readonly T ItemRef(int index)
        {
            return ref _root.ItemRef(index);
        }

        /// <summary>Creates a collection that has the same contents as this immutable sorted set that can be efficiently manipulated by using standard mutable interfaces.</summary>
        /// <returns>The sorted set builder.</returns>
        public Builder ToBuilder()
        {
            return new Builder(this);
        }

        /// <summary>Adds the specified value to this immutable sorted set.</summary>
        /// <param name="value">The value to add.</param>
        /// <returns>A new set with the element added, or this set if the element is already in this set.</returns>
        public ImmutableSortedSet<T> Add(T value)
        {
            bool mutated;
            return Wrap(_root.Add(value, _comparer, out mutated));
        }

        /// <summary>Removes the specified value from this immutable sorted set.</summary>
        /// <param name="value">The element to remove.</param>
        /// <returns>A new immutable sorted set with the element removed, or this set if the element was not found in the set.</returns>
        public ImmutableSortedSet<T> Remove(T value)
        {
            bool mutated;
            return Wrap(_root.Remove(value, _comparer, out mutated));
        }

        /// <summary>Searches the set for a given value and returns the equal value it finds, if any.</summary>
        /// <param name="equalValue">The value to search for.</param>
        /// <param name="actualValue">The value from the set that the search found, or the original value if the search yielded no match.</param>
        /// <returns>A value indicating whether the search was successful.</returns>
        public bool TryGetValue(T equalValue, out T actualValue)
        {
            Node node = _root.Search(equalValue, _comparer);
            if (node.IsEmpty)
            {
                actualValue = equalValue;
                return false;
            }
            actualValue = node.Key;
            return true;
        }

        /// <summary>Creates an immutable sorted set that contains elements that exist both in this set and in the specified set.</summary>
        /// <param name="other">The set to intersect with this one.</param>
        /// <returns>A new immutable sorted set that contains any elements that exist in both sets.</returns>
        public ImmutableSortedSet<T> Intersect(IEnumerable<T> other)
        {
            Requires.NotNull(other, "other");
            ImmutableSortedSet<T> immutableSortedSet = Clear();
            foreach (T item in other.GetEnumerableDisposable<T, Enumerator>())
            {
                if (Contains(item))
                {
                    immutableSortedSet = immutableSortedSet.Add(item);
                }
            }
            return immutableSortedSet;
        }

        /// <summary>Removes a specified set of items from this immutable sorted set.</summary>
        /// <param name="other">The items to remove from this set.</param>
        /// <returns>A new set with the items removed; or the original set if none of the items were in the set.</returns>
        public ImmutableSortedSet<T> Except(IEnumerable<T> other)
        {
            Requires.NotNull(other, "other");
            Node node = _root;
            foreach (T item in other.GetEnumerableDisposable<T, Enumerator>())
            {
                node = node.Remove(item, _comparer, out var _);
            }
            return Wrap(node);
        }

        /// <summary>Creates an immutable sorted set that contains elements that exist either in this set or in a given sequence, but not both.</summary>
        /// <param name="other">The other sequence of items.</param>
        /// <returns>The new immutable sorted set.</returns>
        public ImmutableSortedSet<T> SymmetricExcept(IEnumerable<T> other)
        {
            Requires.NotNull(other, "other");
            ImmutableSortedSet<T> immutableSortedSet = ImmutableSortedSet.CreateRange(_comparer, other);
            ImmutableSortedSet<T> immutableSortedSet2 = Clear();
            using (Enumerator enumerator = GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    T current = enumerator.Current;
                    if (!immutableSortedSet.Contains(current))
                    {
                        immutableSortedSet2 = immutableSortedSet2.Add(current);
                    }
                }
            }
            foreach (T item in immutableSortedSet)
            {
                if (!Contains(item))
                {
                    immutableSortedSet2 = immutableSortedSet2.Add(item);
                }
            }
            return immutableSortedSet2;
        }

        /// <summary>Adds a given set of items to this immutable sorted set.</summary>
        /// <param name="other">The items to add.</param>
        /// <returns>The new set with the items added; or the original set if all the items were already in the set.</returns>
        public ImmutableSortedSet<T> Union(IEnumerable<T> other)
        {
            Requires.NotNull(other, "other");
            if (TryCastToImmutableSortedSet(other, out var other2) && other2.KeyComparer == KeyComparer)
            {
                if (other2.IsEmpty)
                {
                    return this;
                }
                if (IsEmpty)
                {
                    return other2;
                }
                if (other2.Count > Count)
                {
                    return other2.Union(this);
                }
            }
            if (IsEmpty || (other.TryGetCount(out var count) && (float)(Count + count) * 0.15f > (float)Count))
            {
                return LeafToRootRefill(other);
            }
            return UnionIncremental(other);
        }

        /// <summary>Returns the immutable sorted set that has the specified key comparer.</summary>
        /// <param name="comparer">The comparer to check for.</param>
        /// <returns>The immutable sorted set that has the specified key comparer.</returns>
        public ImmutableSortedSet<T> WithComparer(IComparer<T>? comparer)
        {
            if (comparer == null)
            {
                comparer = Comparer<T>.Default;
            }
            if (comparer == _comparer)
            {
                return this;
            }
            ImmutableSortedSet<T> immutableSortedSet = new ImmutableSortedSet<T>(Node.EmptyNode, comparer);
            return immutableSortedSet.Union(this);
        }

        /// <summary>Determines whether the current immutable sorted set and the specified collection contain the same elements.</summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>
        ///   <see langword="true" /> if the sets are equal; otherwise, <see langword="false" />.</returns>
        public bool SetEquals(IEnumerable<T> other)
        {
            Requires.NotNull(other, "other");
            if (this == other)
            {
                return true;
            }
            SortedSet<T> sortedSet = new SortedSet<T>(other, KeyComparer);
            if (Count != sortedSet.Count)
            {
                return false;
            }
            int num = 0;
            foreach (T item in sortedSet)
            {
                if (!Contains(item))
                {
                    return false;
                }
                num++;
            }
            return num == Count;
        }

        /// <summary>Determines whether the current immutable sorted set is a proper (strict) subset of the specified collection.</summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>
        ///   <see langword="true" /> if the current set is a proper subset of <paramref name="other" />; otherwise, <see langword="false" />.</returns>
        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            Requires.NotNull(other, "other");
            if (IsEmpty)
            {
                return other.Any();
            }
            SortedSet<T> sortedSet = new SortedSet<T>(other, KeyComparer);
            if (Count >= sortedSet.Count)
            {
                return false;
            }
            int num = 0;
            bool flag = false;
            foreach (T item in sortedSet)
            {
                if (Contains(item))
                {
                    num++;
                }
                else
                {
                    flag = true;
                }
                if (num == Count && flag)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>Determines whether the current immutable sorted set is a proper superset of a specified collection.</summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>
        ///   <see langword="true" /> if the current set is a proper superset of <paramref name="other" />; otherwise, <see langword="false" />.</returns>
        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            Requires.NotNull(other, "other");
            if (IsEmpty)
            {
                return false;
            }
            int num = 0;
            foreach (T item in other.GetEnumerableDisposable<T, Enumerator>())
            {
                num++;
                if (!Contains(item))
                {
                    return false;
                }
            }
            return Count > num;
        }

        /// <summary>Determines whether the current immutable sorted set is a subset of a specified collection.</summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>
        ///   <see langword="true" /> if the current set is a subset of <paramref name="other" />; otherwise, <see langword="false" />.</returns>
        public bool IsSubsetOf(IEnumerable<T> other)
        {
            Requires.NotNull(other, "other");
            if (IsEmpty)
            {
                return true;
            }
            SortedSet<T> sortedSet = new SortedSet<T>(other, KeyComparer);
            int num = 0;
            foreach (T item in sortedSet)
            {
                if (Contains(item))
                {
                    num++;
                }
            }
            return num == Count;
        }

        /// <summary>Determines whether the current immutable sorted set is a superset of a specified collection.</summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>
        ///   <see langword="true" /> if the current set is a superset of <paramref name="other" />; otherwise, <see langword="false" />.</returns>
        public bool IsSupersetOf(IEnumerable<T> other)
        {
            Requires.NotNull(other, "other");
            foreach (T item in other.GetEnumerableDisposable<T, Enumerator>())
            {
                if (!Contains(item))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>Determines whether the current immutable sorted set and a specified collection share common elements.</summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>
        ///   <see langword="true" /> if the current set and <paramref name="other" /> share at least one common element; otherwise, <see langword="false" />.</returns>
        public bool Overlaps(IEnumerable<T> other)
        {
            Requires.NotNull(other, "other");
            if (IsEmpty)
            {
                return false;
            }
            foreach (T item in other.GetEnumerableDisposable<T, Enumerator>())
            {
                if (Contains(item))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>Returns an <see cref="T:System.Collections.Generic.IEnumerable`1" /> that iterates over this immutable sorted set in reverse order.</summary>
        /// <returns>An enumerator that iterates over the immutable sorted set in reverse order.</returns>
        public IEnumerable<T> Reverse()
        {
            return new ReverseEnumerable(_root);
        }

        /// <summary>Gets the position within this immutable sorted set that the specified value appears in.</summary>
        /// <param name="item">The value whose position is being sought.</param>
        /// <returns>The index of the specified <paramref name="item" /> in the sorted set, if <paramref name="item" /> is found. If <paramref name="item" /> is not found and is less than one or more elements in this set, this method returns a negative number that is the bitwise complement of the index of the first element that is larger than value. If <paramref name="item" /> is not found and is greater than any of the elements in the set, this method returns a negative number that is the bitwise complement of the index of the last element plus 1.</returns>
        public int IndexOf(T item)
        {
            return _root.IndexOf(item, _comparer);
        }

        /// <summary>Determines whether this immutable sorted set contains the specified value.</summary>
        /// <param name="value">The value to check for.</param>
        /// <returns>
        ///   <see langword="true" /> if the set contains the specified value; otherwise, <see langword="false" />.</returns>
        public bool Contains(T value)
        {
            return _root.Contains(value, _comparer);
        }

        /// <summary>Retrieves an empty immutable set that has the same sorting and ordering semantics as this instance.</summary>
        /// <returns>An empty set that has the same sorting and ordering semantics as this instance.</returns>
        IImmutableSet<T> IImmutableSet<T>.Clear()
        {
            return Clear();
        }

        /// <summary>Adds the specified element to this immutable set.</summary>
        /// <param name="value">The element to add.</param>
        /// <returns>A new set with the element added, or this set if the element is already in the set.</returns>
        IImmutableSet<T> IImmutableSet<T>.Add(T value)
        {
            return Add(value);
        }

        /// <summary>Removes the specified element from this immutable set.</summary>
        /// <param name="value">The element to remove.</param>
        /// <returns>A new set with the specified element removed, or the current set if the element cannot be found in the set.</returns>
        IImmutableSet<T> IImmutableSet<T>.Remove(T value)
        {
            return Remove(value);
        }

        /// <summary>Creates an immutable set that contains elements that exist in both this set and the specified set.</summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>A new immutable set that contains any elements that exist in both sets.</returns>
        IImmutableSet<T> IImmutableSet<T>.Intersect(IEnumerable<T> other)
        {
            return Intersect(other);
        }

        /// <summary>Removes the elements in the specified collection from the current immutable set.</summary>
        /// <param name="other">The items to remove from this set.</param>
        /// <returns>The new set with the items removed; or the original set if none of the items were in the set.</returns>
        IImmutableSet<T> IImmutableSet<T>.Except(IEnumerable<T> other)
        {
            return Except(other);
        }

        /// <summary>Creates an immutable set that contains only elements that are present either in the current set or in the specified collection, but not both.</summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>A new set that contains the elements that are present only in the current set or in the specified collection, but not both.</returns>
        IImmutableSet<T> IImmutableSet<T>.SymmetricExcept(IEnumerable<T> other)
        {
            return SymmetricExcept(other);
        }

        /// <summary>Creates a new immutable set that contains all elements that are present in either the current set or in the specified collection.</summary>
        /// <param name="other">The collection to add elements from.</param>
        /// <returns>A new immutable set with the items added; or the original set if all the items were already in the set.</returns>
        IImmutableSet<T> IImmutableSet<T>.Union(IEnumerable<T> other)
        {
            return Union(other);
        }

        /// <summary>Adds an element to the current set and returns a value to indicate if the element was successfully added.</summary>
        /// <param name="item">The element to add to the set.</param>
        /// <returns>
        ///   <see langword="true" /> if the element is added to the set; <see langword="false" /> if the element is already in the set.</returns>
        bool ISet<T>.Add(T item)
        {
            throw new NotSupportedException();
        }

        /// <summary>Removes all elements in the specified collection from the current set.</summary>
        /// <param name="other">The collection of items to remove from the set.</param>
        void ISet<T>.ExceptWith(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }

        /// <summary>Modifies the current set so that it contains only elements that are also in a specified collection.</summary>
        /// <param name="other">The collection to compare to the current set.</param>
        void ISet<T>.IntersectWith(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }

        /// <summary>Modifies the current set so that it contains only elements that are present either in the current set or in the specified collection, but not both.</summary>
        /// <param name="other">The collection to compare to the current set.</param>
        void ISet<T>.SymmetricExceptWith(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }

        /// <summary>Modifies the current set so that it contains all elements that are present in either the current set or the specified collection.</summary>
        /// <param name="other">The collection to compare to the current set.</param>
        void ISet<T>.UnionWith(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }

        /// <summary>Copies the elements of the collection to an array, starting at a particular array index.</summary>
        /// <param name="array">The one-dimensional array that is the destination of the elements copied from collection. The array must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            _root.CopyTo(array, arrayIndex);
        }

        /// <summary>Adds the specified value to the collection.</summary>
        /// <param name="item">The value to add.</param>
        void ICollection<T>.Add(T item)
        {
            throw new NotSupportedException();
        }

        /// <summary>Removes all the items from the collection.</summary>
        void ICollection<T>.Clear()
        {
            throw new NotSupportedException();
        }

        /// <summary>Removes the first occurrence of a specific object from the collection.</summary>
        /// <param name="item">The object to remove from the collection.</param>
        /// <returns>
        ///   <see langword="true" /> if <paramref name="item" /> was successfully removed from the collection; otherwise, <see langword="false" />.</returns>
        bool ICollection<T>.Remove(T item)
        {
            throw new NotSupportedException();
        }

        /// <summary>Inserts an item in the set at the specified index.</summary>
        /// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
        /// <param name="item">The object to insert into the set.</param>
        void IList<T>.Insert(int index, T item)
        {
            throw new NotSupportedException();
        }

        /// <summary>Removes the item at the specified index.</summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        void IList<T>.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        /// <summary>Adds an item to the set.</summary>
        /// <param name="value">The object to add to the set.</param>
        /// <exception cref="T:System.NotSupportedException">The set is read-only or has a fixed size.</exception>
        /// <returns>The position into which the new element was inserted, or -1 to indicate that the item was not inserted into the collection.</returns>
        int IList.Add(object value)
        {
            throw new NotSupportedException();
        }

        /// <summary>Removes all items from the set.</summary>
        /// <exception cref="T:System.NotSupportedException">Thrown in all cases.</exception>
        void IList.Clear()
        {
            throw new NotSupportedException();
        }

        /// <summary>Determines whether the set contains a specific value.</summary>
        /// <param name="value">The object to locate in the set.</param>
        /// <returns>
        ///   <see langword="true" /> if the object is found in the set; otherwise, <see langword="false" />.</returns>
        bool IList.Contains(object value)
        {
            return Contains((T)value);
        }

        /// <summary>Determines the index of a specific item in the set.</summary>
        /// <param name="value">The object to locate in the set.</param>
        /// <returns>The index of <paramref name="value" /> if found in the list; otherwise, -1.</returns>
        int IList.IndexOf(object value)
        {
            return IndexOf((T)value);
        }

        /// <summary>Inserts an item into the set at the specified index.</summary>
        /// <param name="index">The zero-based index at which <paramref name="value" /> should be inserted.</param>
        /// <param name="value">The object to insert into the set.</param>
        /// <exception cref="T:System.NotSupportedException">The set is read-only or has a fixed size.</exception>
        void IList.Insert(int index, object value)
        {
            throw new NotSupportedException();
        }

        /// <summary>Removes the first occurrence of a specific object from the set.</summary>
        /// <param name="value">The object to remove from the set.</param>
        /// <exception cref="T:System.NotSupportedException">The set is read-only or has a fixed size.</exception>
        void IList.Remove(object value)
        {
            throw new NotSupportedException();
        }

        /// <summary>Removes the item at the specified index of the set.</summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <exception cref="T:System.NotSupportedException">The set is read-only or has a fixed size.</exception>
        void IList.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        /// <summary>Copies the elements of the set to an array, starting at a particular array index.</summary>
        /// <param name="array">The one-dimensional array that is the destination of the elements copied from the set. The array must have zero-based indexing.</param>
        /// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        void ICollection.CopyTo(Array array, int index)
        {
            _root.CopyTo(array, index);
        }

        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            if (!IsEmpty)
            {
                return GetEnumerator();
            }
            return Enumerable.Empty<T>().GetEnumerator();
        }

        /// <summary>Returns an enumerator that iterates through a collection.</summary>
        /// <returns>An enumerator object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>Returns an enumerator that iterates through the immutable sorted set.</summary>
        /// <returns>An enumerator that can be used to iterate through the set.</returns>
        public Enumerator GetEnumerator()
        {
            return _root.GetEnumerator();
        }

        private static bool TryCastToImmutableSortedSet(IEnumerable<T> sequence, [NotNullWhen(true)] out ImmutableSortedSet<T> other)
        {
            other = sequence as ImmutableSortedSet<T>;
            if (other != null)
            {
                return true;
            }
            if (sequence is Builder builder)
            {
                other = builder.ToImmutable();
                return true;
            }
            return false;
        }

        private static ImmutableSortedSet<T> Wrap(Node root, IComparer<T> comparer)
        {
            if (!root.IsEmpty)
            {
                return new ImmutableSortedSet<T>(root, comparer);
            }
            return Empty.WithComparer(comparer);
        }

        private ImmutableSortedSet<T> UnionIncremental(IEnumerable<T> items)
        {
            Requires.NotNull(items, "items");
            Node node = _root;
            foreach (T item in items.GetEnumerableDisposable<T, Enumerator>())
            {
                node = node.Add(item, _comparer, out var _);
            }
            return Wrap(node);
        }

        private ImmutableSortedSet<T> Wrap(Node root)
        {
            if (root != _root)
            {
                if (!root.IsEmpty)
                {
                    return new ImmutableSortedSet<T>(root, _comparer);
                }
                return Clear();
            }
            return this;
        }

        private ImmutableSortedSet<T> LeafToRootRefill(IEnumerable<T> addedItems)
        {
            Requires.NotNull(addedItems, "addedItems");
            List<T> list;
            if (IsEmpty)
            {
                if (addedItems.TryGetCount(out var count) && count == 0)
                {
                    return this;
                }
                list = new List<T>(addedItems);
                if (list.Count == 0)
                {
                    return this;
                }
            }
            else
            {
                list = new List<T>(this);
                list.AddRange(addedItems);
            }
            IComparer<T> keyComparer = KeyComparer;
            list.Sort(keyComparer);
            int num = 1;
            for (int i = 1; i < list.Count; i++)
            {
                if (keyComparer.Compare(list[i], list[i - 1]) != 0)
                {
                    list[num++] = list[i];
                }
            }
            list.RemoveRange(num, list.Count - num);
            Node root = Node.NodeTreeFromList(list.AsOrderedCollection(), 0, list.Count);
            return Wrap(root);
        }
    }

    internal sealed class ImmutableSortedSetBuilderDebuggerProxy<T>
    {
        private readonly ImmutableSortedSet<T>.Builder _set;

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Contents => _set.ToArray(_set.Count);

        public ImmutableSortedSetBuilderDebuggerProxy(ImmutableSortedSet<T>.Builder builder)
        {
            Requires.NotNull(builder, "builder");
            _set = builder;
        }
    }


    /// <summary>Provides a set of initialization methods for instances of the <see cref="System.Collections.Immutable.ImmutableStack{T}" /> class. </summary>
    public static class ImmutableStack
    {
        /// <summary>Creates an empty immutable stack.</summary>
        /// <typeparam name="T">The type of items to be stored in the immutable stack.</typeparam>
        /// <returns>An empty immutable stack.</returns>
        public static ImmutableStack<T> Create<T>()
        {
            return ImmutableStack<T>.Empty;
        }

        /// <summary>Creates a new immutable stack that contains the specified item.</summary>
        /// <param name="item">The item to prepopulate the stack with.</param>
        /// <typeparam name="T">The type of items in the immutable stack.</typeparam>
        /// <returns>A new immutable collection that contains the specified item.</returns>
        public static ImmutableStack<T> Create<T>(T item)
        {
            return ImmutableStack<T>.Empty.Push(item);
        }

        /// <summary>Creates a new immutable stack that contains the specified items.</summary>
        /// <param name="items">The items to add to the stack before it's immutable.</param>
        /// <typeparam name="T">The type of items in the stack.</typeparam>
        /// <returns>An immutable stack that contains the specified items.</returns>
        public static ImmutableStack<T> CreateRange<T>(IEnumerable<T> items)
        {
            Requires.NotNull(items, "items");
            ImmutableStack<T> immutableStack = ImmutableStack<T>.Empty;
            foreach (T item in items)
            {
                immutableStack = immutableStack.Push(item);
            }
            return immutableStack;
        }

        /// <summary>Creates a new immutable stack that contains the specified array of items.</summary>
        /// <param name="items">An array that contains the items to prepopulate the stack with.</param>
        /// <typeparam name="T">The type of items in the immutable stack.</typeparam>
        /// <returns>A new immutable stack that contains the specified items.</returns>
        public static ImmutableStack<T> Create<T>(params T[] items)
        {
            Requires.NotNull(items, "items");
            ImmutableStack<T> immutableStack = ImmutableStack<T>.Empty;
            foreach (T value in items)
            {
                immutableStack = immutableStack.Push(value);
            }
            return immutableStack;
        }

        /// <summary>Removes the specified item from an immutable stack.</summary>
        /// <param name="stack">The stack to modify.</param>
        /// <param name="value">The item to remove from the stack.</param>
        /// <typeparam name="T">The type of items contained in the stack.</typeparam>
        /// <exception cref="T:System.InvalidOperationException">The stack is empty.</exception>
        /// <returns>A stack; never <see langword="null" />.</returns>
        public static IImmutableStack<T> Pop<T>(this IImmutableStack<T> stack, out T value)
        {
            Requires.NotNull(stack, "stack");
            value = stack.Peek();
            return stack.Pop();
        }
    }
    
    /// <summary>Represents an immutable stack.  </summary>
    /// <typeparam name="T">The type of element on the stack.</typeparam>
    [DebuggerDisplay("IsEmpty = {IsEmpty}; Top = {_head}")]
    [DebuggerTypeProxy(typeof(ImmutableEnumerableDebuggerProxy<>))]
    public sealed class ImmutableStack<T> : IImmutableStack<T>, IEnumerable<T>, IEnumerable
    {
        /// <summary>Enumerates the contents of an immutable stack without allocating any memory. </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public struct Enumerator
        {
            private readonly ImmutableStack<T> _originalStack;

            private ImmutableStack<T> _remainingStack;

            /// <summary>Gets the element at the current position of the enumerator.</summary>
            /// <returns>The element at the current position of the enumerator.</returns>
            public T Current
            {
                get
                {
                    if (_remainingStack == null || _remainingStack.IsEmpty)
                    {
                        throw new InvalidOperationException();
                    }
                    return _remainingStack.Peek();
                }
            }

            internal Enumerator(ImmutableStack<T> stack)
            {
                Requires.NotNull(stack, "stack");
                _originalStack = stack;
                _remainingStack = null;
            }

            /// <summary>Advances the enumerator to the next element of the immutable stack.</summary>
            /// <returns>
            ///   <see langword="true" /> if the enumerator was successfully advanced to the next element; <see langword="false" /> if the enumerator has passed the end of the stack.</returns>
            public bool MoveNext()
            {
                if (_remainingStack == null)
                {
                    _remainingStack = _originalStack;
                }
                else if (!_remainingStack.IsEmpty)
                {
                    _remainingStack = _remainingStack.Pop();
                }
                return !_remainingStack.IsEmpty;
            }
        }

        private sealed class EnumeratorObject : IEnumerator<T>, IDisposable, IEnumerator
        {
            private readonly ImmutableStack<T> _originalStack;

            private ImmutableStack<T> _remainingStack;

            private bool _disposed;

            public T Current
            {
                get
                {
                    ThrowIfDisposed();
                    if (_remainingStack == null || _remainingStack.IsEmpty)
                    {
                        throw new InvalidOperationException();
                    }
                    return _remainingStack.Peek();
                }
            }

            object IEnumerator.Current => Current;

            internal EnumeratorObject(ImmutableStack<T> stack)
            {
                Requires.NotNull(stack, "stack");
                _originalStack = stack;
            }

            public bool MoveNext()
            {
                ThrowIfDisposed();
                if (_remainingStack == null)
                {
                    _remainingStack = _originalStack;
                }
                else if (!_remainingStack.IsEmpty)
                {
                    _remainingStack = _remainingStack.Pop();
                }
                return !_remainingStack.IsEmpty;
            }

            public void Reset()
            {
                ThrowIfDisposed();
                _remainingStack = null;
            }

            public void Dispose()
            {
                _disposed = true;
            }

            private void ThrowIfDisposed()
            {
                if (_disposed)
                {
                    Requires.FailObjectDisposed(this);
                }
            }
        }

        private static readonly ImmutableStack<T> s_EmptyField = new ImmutableStack<T>();

        private readonly T _head;

        private readonly ImmutableStack<T> _tail;

        /// <summary>Gets an empty immutable stack.</summary>
        /// <returns>An empty immutable stack.</returns>
        public static ImmutableStack<T> Empty => s_EmptyField;

        /// <summary>Gets a value that indicates whether this instance of the immutable stack is empty.</summary>
        /// <returns>
        ///   <see langword="true" /> if this instance is empty; otherwise, <see langword="false" />.</returns>
        public bool IsEmpty => _tail == null;

        private ImmutableStack()
        {
        }

        private ImmutableStack(T head, ImmutableStack<T> tail)
        {
            _head = head;
            _tail = tail;
        }

        /// <summary>Removes all objects from the immutable stack.</summary>
        /// <returns>An empty immutable stack.</returns>
        public ImmutableStack<T> Clear()
        {
            return Empty;
        }

        /// <summary>Removes all elements from the immutable stack.</summary>
        /// <returns>The empty immutable stack.</returns>
        IImmutableStack<T> IImmutableStack<T>.Clear()
        {
            return Clear();
        }

        /// <summary>Returns the object at the top of the stack without removing it.</summary>
        /// <exception cref="T:System.InvalidOperationException">The stack is empty.</exception>
        /// <returns>The object at the top of the stack.</returns>
        public T Peek()
        {
            if (IsEmpty)
            {
                throw new InvalidOperationException(MDCFR.Properties.Resources.InvalidEmptyOperation);
            }
            return _head;
        }

        /// <summary>Gets a read-only reference to the element on the top of the stack.</summary>
        /// <exception cref="T:System.InvalidOperationException">The stack is empty.</exception>
        /// <returns>A read-only reference to the element on the top of the stack.</returns>
        public ref readonly T PeekRef()
        {
            if (IsEmpty)
            {
                throw new InvalidOperationException(MDCFR.Properties.Resources.InvalidEmptyOperation);
            }
            return ref _head;
        }

        /// <summary>Inserts an object at the top of the immutable stack and returns the new stack.</summary>
        /// <param name="value">The object to push onto the stack.</param>
        /// <returns>The new stack.</returns>
        public ImmutableStack<T> Push(T value)
        {
            return new ImmutableStack<T>(value, this);
        }

        /// <summary>Inserts an element at the top of the immutable stack and returns the new stack.</summary>
        /// <param name="value">The element to push onto the stack.</param>
        /// <returns>The new stack.</returns>
        IImmutableStack<T> IImmutableStack<T>.Push(T value)
        {
            return Push(value);
        }

        /// <summary>Removes the element at the top of the immutable stack and returns the stack after the removal.</summary>
        /// <exception cref="T:System.InvalidOperationException">The stack is empty.</exception>
        /// <returns>A stack; never <see langword="null" />.</returns>
        public ImmutableStack<T> Pop()
        {
            if (IsEmpty)
            {
                throw new InvalidOperationException(MDCFR.Properties.Resources.InvalidEmptyOperation);
            }
            return _tail;
        }

        /// <summary>Removes the specified element from the immutable stack and returns the stack after the removal.</summary>
        /// <param name="value">The value to remove from the stack.</param>
        /// <returns>A stack; never <see langword="null" />.</returns>
        public ImmutableStack<T> Pop(out T value)
        {
            value = Peek();
            return Pop();
        }

        /// <summary>Removes the element at the top of the immutable stack and returns the new stack.</summary>
        /// <exception cref="T:System.InvalidOperationException">The stack is empty.</exception>
        /// <returns>The new stack; never <see langword="null" />.</returns>
        IImmutableStack<T> IImmutableStack<T>.Pop()
        {
            return Pop();
        }

        /// <summary>Returns an enumerator that iterates through the immutable stack.</summary>
        /// <returns>An enumerator that can be used to iterate through the stack.</returns>
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator  that can be used to iterate through the collection.</returns>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            if (!IsEmpty)
            {
                return new EnumeratorObject(this);
            }
            return Enumerable.Empty<T>().GetEnumerator();
        }

        /// <summary>Returns an enumerator that iterates through a collection.</summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new EnumeratorObject(this);
        }

        internal ImmutableStack<T> Reverse()
        {
            ImmutableStack<T> immutableStack = Clear();
            ImmutableStack<T> immutableStack2 = this;
            while (!immutableStack2.IsEmpty)
            {
                immutableStack = immutableStack.Push(immutableStack2.Peek());
                immutableStack2 = immutableStack2.Pop();
            }
            return immutableStack;
        }
    }

    internal interface IOrderedCollection<out T> : IEnumerable<T>, IEnumerable
    {
        int Count { get; }

        T this[int index] { get; }
    }

    internal interface ISecurePooledObjectUser
    {
        int PoolUserId { get; }
    }

    internal interface IStrongEnumerable<out T, TEnumerator> where TEnumerator : struct, IStrongEnumerator<T>
    {
        TEnumerator GetEnumerator();
    }

    internal interface IStrongEnumerator<T>
    {
        T Current { get; }

        bool MoveNext();
    }

    internal sealed class KeysCollectionAccessor<TKey, TValue> : KeysOrValuesCollectionAccessor<TKey, TValue, TKey> where TKey : notnull
    {
        internal KeysCollectionAccessor(IImmutableDictionary<TKey, TValue> dictionary)
            : base(dictionary, dictionary.Keys)
        {
        }

        public override bool Contains(TKey item)
        {
            return base.Dictionary.ContainsKey(item);
        }
    }

    [DebuggerDisplay("{Value,nq}")]
    internal struct RefAsValueType<T>
    {
        internal T Value;

        internal RefAsValueType(T value)
        {
            Value = value;
        }
    }

    internal static class Requires
    {
        [DebuggerStepThrough]
        public static void NotNull<T>([ValidatedNotNull] T value, string? parameterName) where T : class
        {
            if (value == null)
            {
                FailArgumentNullException(parameterName);
            }
        }

        [DebuggerStepThrough]
        public static T NotNullPassthrough<T>([ValidatedNotNull] T value, string? parameterName) where T : class
        {
            NotNull(value, parameterName);
            return value;
        }

        [DebuggerStepThrough]
        public static void NotNullAllowStructs<T>([ValidatedNotNull] T value, string? parameterName)
        {
            if (value == null)
            {
                FailArgumentNullException(parameterName);
            }
        }

        [DebuggerStepThrough]
        private static void FailArgumentNullException(string parameterName)
        {
            throw new ArgumentNullException(parameterName);
        }

        [DebuggerStepThrough]
        public static void Range(bool condition, string? parameterName, string? message = null)
        {
            if (!condition)
            {
                FailRange(parameterName, message);
            }
        }

        [DebuggerStepThrough]
        public static void FailRange(string? parameterName, string? message = null)
        {
            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
            throw new ArgumentOutOfRangeException(parameterName, message);
        }

        [DebuggerStepThrough]
        public static void Argument(bool condition, string? parameterName, string? message)
        {
            if (!condition)
            {
                throw new ArgumentException(message, parameterName);
            }
        }

        [DebuggerStepThrough]
        public static void Argument(bool condition)
        {
            if (!condition)
            {
                throw new ArgumentException();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [DebuggerStepThrough]
        public static void FailObjectDisposed<TDisposed>(TDisposed disposed)
        {
            throw new ObjectDisposedException(disposed.GetType().FullName);
        }
    }

    internal static class SecureObjectPool
    {
        private static int s_poolUserIdCounter;

        internal const int UnassignedId = -1;

        internal static int NewId()
        {
            int num;
            do
            {
                num = Interlocked.Increment(ref s_poolUserIdCounter);
            }
            while (num == -1);
            return num;
        }
    }

    internal static class SecureObjectPool<T, TCaller> where TCaller : ISecurePooledObjectUser
    {
        public static void TryAdd(TCaller caller, SecurePooledObject<T> item)
        {
            if (caller.PoolUserId == item.Owner)
            {
                item.Owner = -1;
                AllocFreeConcurrentStack<SecurePooledObject<T>>.TryAdd(item);
            }
        }

        public static bool TryTake(TCaller caller, out SecurePooledObject<T>? item)
        {
            if (caller.PoolUserId != -1 && AllocFreeConcurrentStack<SecurePooledObject<T>>.TryTake(out item))
            {
                item.Owner = caller.PoolUserId;
                return true;
            }
            item = null;
            return false;
        }

        public static SecurePooledObject<T> PrepNew(TCaller caller, T newValue)
        {
            Requires.NotNullAllowStructs(newValue, "newValue");
            SecurePooledObject<T> securePooledObject = new SecurePooledObject<T>(newValue);
            securePooledObject.Owner = caller.PoolUserId;
            return securePooledObject;
        }
    }

    internal sealed class SecurePooledObject<T>
    {
        private readonly T _value;

        private int _owner;

        internal int Owner
        {
            get
            {
                return _owner;
            }
            set
            {
                _owner = value;
            }
        }

        internal SecurePooledObject(T newValue)
        {
            Requires.NotNullAllowStructs(newValue, "newValue");
            _value = newValue;
        }

        internal T Use<TCaller>(ref TCaller caller) where TCaller : struct, ISecurePooledObjectUser
        {
            if (!IsOwned(ref caller))
            {
                Requires.FailObjectDisposed(caller);
            }
            return _value;
        }

        internal bool TryUse<TCaller>(ref TCaller caller, [MaybeNullWhen(false)] out T value) where TCaller : struct, ISecurePooledObjectUser
        {
            if (IsOwned(ref caller))
            {
                value = _value;
                return true;
            }
            value = default(T);
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool IsOwned<TCaller>(ref TCaller caller) where TCaller : struct, ISecurePooledObjectUser
        {
            return caller.PoolUserId == _owner;
        }
    }


    [DebuggerDisplay("{_key} = {_value}")]
    internal sealed class SortedInt32KeyNode<TValue> : IBinaryTree
    {
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public struct Enumerator : IEnumerator<KeyValuePair<int, TValue>>, IDisposable, IEnumerator, ISecurePooledObjectUser
        {
            private readonly int _poolUserId;

            private SortedInt32KeyNode<TValue> _root;

            private SecurePooledObject<Stack<RefAsValueType<SortedInt32KeyNode<TValue>>>> _stack;

            private SortedInt32KeyNode<TValue> _current;

            public KeyValuePair<int, TValue> Current
            {
                get
                {
                    ThrowIfDisposed();
                    if (_current != null)
                    {
                        return _current.Value;
                    }
                    throw new InvalidOperationException();
                }
            }

            int ISecurePooledObjectUser.PoolUserId => _poolUserId;

            object IEnumerator.Current => Current;

            internal Enumerator(SortedInt32KeyNode<TValue> root)
            {
                Requires.NotNull(root, "root");
                _root = root;
                _current = null;
                _poolUserId = SecureObjectPool.NewId();
                _stack = null;
                if (!_root.IsEmpty)
                {
                    if (!SecureObjectPool<Stack<RefAsValueType<SortedInt32KeyNode<TValue>>>, Enumerator>.TryTake(this, out _stack))
                    {
                        _stack = SecureObjectPool<Stack<RefAsValueType<SortedInt32KeyNode<TValue>>>, Enumerator>.PrepNew(this, new Stack<RefAsValueType<SortedInt32KeyNode<TValue>>>(root.Height));
                    }
                    PushLeft(_root);
                }
            }

            public void Dispose()
            {
                _root = null;
                _current = null;
                if (_stack != null && _stack.TryUse(ref this, out var value))
                {
                    value.ClearFastWhenEmpty();
                    SecureObjectPool<Stack<RefAsValueType<SortedInt32KeyNode<TValue>>>, Enumerator>.TryAdd(this, _stack);
                }
                _stack = null;
            }

            public bool MoveNext()
            {
                ThrowIfDisposed();
                if (_stack != null)
                {
                    Stack<RefAsValueType<SortedInt32KeyNode<TValue>>> stack = _stack.Use(ref this);
                    if (stack.Count > 0)
                    {
                        PushLeft((_current = stack.Pop().Value).Right);
                        return true;
                    }
                }
                _current = null;
                return false;
            }

            public void Reset()
            {
                ThrowIfDisposed();
                _current = null;
                if (_stack != null)
                {
                    Stack<RefAsValueType<SortedInt32KeyNode<TValue>>> stack = _stack.Use(ref this);
                    stack.ClearFastWhenEmpty();
                    PushLeft(_root);
                }
            }

            internal void ThrowIfDisposed()
            {
                if (_root == null || (_stack != null && !_stack.IsOwned(ref this)))
                {
                    Requires.FailObjectDisposed(this);
                }
            }

            private void PushLeft(SortedInt32KeyNode<TValue> node)
            {
                Requires.NotNull(node, "node");
                Stack<RefAsValueType<SortedInt32KeyNode<TValue>>> stack = _stack.Use(ref this);
                while (!node.IsEmpty)
                {
                    stack.Push(new RefAsValueType<SortedInt32KeyNode<TValue>>(node));
                    node = node.Left;
                }
            }
        }

        internal static readonly SortedInt32KeyNode<TValue> EmptyNode = new SortedInt32KeyNode<TValue>();

        private readonly int _key;

        private readonly TValue _value;

        private bool _frozen;

        private byte _height;

        private SortedInt32KeyNode<TValue> _left;

        private SortedInt32KeyNode<TValue> _right;

        public bool IsEmpty => _left == null;

        public int Height => _height;

        public SortedInt32KeyNode<TValue>? Left => _left;

        public SortedInt32KeyNode<TValue>? Right => _right;

        IBinaryTree? IBinaryTree.Left => _left;

        IBinaryTree? IBinaryTree.Right => _right;

        int IBinaryTree.Count
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public KeyValuePair<int, TValue> Value => new KeyValuePair<int, TValue>(_key, _value);

        internal IEnumerable<TValue> Values
        {
            get
            {
                using Enumerator enumerator = GetEnumerator();
                while (enumerator.MoveNext())
                {
                    yield return enumerator.Current.Value;
                }
            }
        }

        private SortedInt32KeyNode()
        {
            _frozen = true;
        }

        private SortedInt32KeyNode(int key, TValue value, SortedInt32KeyNode<TValue> left, SortedInt32KeyNode<TValue> right, bool frozen = false)
        {
            Requires.NotNull(left, "left");
            Requires.NotNull(right, "right");
            _key = key;
            _value = value;
            _left = left;
            _right = right;
            _frozen = frozen;
            checked
            {
                _height = (byte)(1 + unchecked((int)Math.Max(left._height, right._height)));
            }
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        internal SortedInt32KeyNode<TValue> SetItem(int key, TValue value, IEqualityComparer<TValue> valueComparer, out bool replacedExistingValue, out bool mutated)
        {
            Requires.NotNull(valueComparer, "valueComparer");
            return SetOrAdd(key, value, valueComparer, overwriteExistingValue: true, out replacedExistingValue, out mutated);
        }

        internal SortedInt32KeyNode<TValue> Remove(int key, out bool mutated)
        {
            return RemoveRecursive(key, out mutated);
        }

        internal TValue? GetValueOrDefault(int key)
        {
            SortedInt32KeyNode<TValue> sortedInt32KeyNode = this;
            while (true)
            {
                if (sortedInt32KeyNode.IsEmpty)
                {
                    return default(TValue);
                }
                if (key == sortedInt32KeyNode._key)
                {
                    break;
                }
                sortedInt32KeyNode = ((key <= sortedInt32KeyNode._key) ? sortedInt32KeyNode._left : sortedInt32KeyNode._right);
            }
            return sortedInt32KeyNode._value;
        }

        internal bool TryGetValue(int key, [MaybeNullWhen(false)] out TValue value)
        {
            SortedInt32KeyNode<TValue> sortedInt32KeyNode = this;
            while (true)
            {
                if (sortedInt32KeyNode.IsEmpty)
                {
                    value = default(TValue);
                    return false;
                }
                if (key == sortedInt32KeyNode._key)
                {
                    break;
                }
                sortedInt32KeyNode = ((key <= sortedInt32KeyNode._key) ? sortedInt32KeyNode._left : sortedInt32KeyNode._right);
            }
            value = sortedInt32KeyNode._value;
            return true;
        }

        internal void Freeze(Action<KeyValuePair<int, TValue>>? freezeAction = null)
        {
            if (!_frozen)
            {
                freezeAction?.Invoke(new KeyValuePair<int, TValue>(_key, _value));
                _left.Freeze(freezeAction);
                _right.Freeze(freezeAction);
                _frozen = true;
            }
        }

        private static SortedInt32KeyNode<TValue> RotateLeft(SortedInt32KeyNode<TValue> tree)
        {
            Requires.NotNull(tree, "tree");
            if (tree._right.IsEmpty)
            {
                return tree;
            }
            SortedInt32KeyNode<TValue> right = tree._right;
            return right.Mutate(tree.Mutate(null, right._left));
        }

        private static SortedInt32KeyNode<TValue> RotateRight(SortedInt32KeyNode<TValue> tree)
        {
            Requires.NotNull(tree, "tree");
            if (tree._left.IsEmpty)
            {
                return tree;
            }
            SortedInt32KeyNode<TValue> left = tree._left;
            return left.Mutate(null, tree.Mutate(left._right));
        }

        private static SortedInt32KeyNode<TValue> DoubleLeft(SortedInt32KeyNode<TValue> tree)
        {
            Requires.NotNull(tree, "tree");
            if (tree._right.IsEmpty)
            {
                return tree;
            }
            SortedInt32KeyNode<TValue> tree2 = tree.Mutate(null, RotateRight(tree._right));
            return RotateLeft(tree2);
        }

        private static SortedInt32KeyNode<TValue> DoubleRight(SortedInt32KeyNode<TValue> tree)
        {
            Requires.NotNull(tree, "tree");
            if (tree._left.IsEmpty)
            {
                return tree;
            }
            SortedInt32KeyNode<TValue> tree2 = tree.Mutate(RotateLeft(tree._left));
            return RotateRight(tree2);
        }

        private static int Balance(SortedInt32KeyNode<TValue> tree)
        {
            Requires.NotNull(tree, "tree");
            return tree._right._height - tree._left._height;
        }

        private static bool IsRightHeavy(SortedInt32KeyNode<TValue> tree)
        {
            Requires.NotNull(tree, "tree");
            return Balance(tree) >= 2;
        }

        private static bool IsLeftHeavy(SortedInt32KeyNode<TValue> tree)
        {
            Requires.NotNull(tree, "tree");
            return Balance(tree) <= -2;
        }

        private static SortedInt32KeyNode<TValue> MakeBalanced(SortedInt32KeyNode<TValue> tree)
        {
            Requires.NotNull(tree, "tree");
            if (IsRightHeavy(tree))
            {
                if (Balance(tree._right) >= 0)
                {
                    return RotateLeft(tree);
                }
                return DoubleLeft(tree);
            }
            if (IsLeftHeavy(tree))
            {
                if (Balance(tree._left) <= 0)
                {
                    return RotateRight(tree);
                }
                return DoubleRight(tree);
            }
            return tree;
        }

        private SortedInt32KeyNode<TValue> SetOrAdd(int key, TValue value, IEqualityComparer<TValue> valueComparer, bool overwriteExistingValue, out bool replacedExistingValue, out bool mutated)
        {
            replacedExistingValue = false;
            if (IsEmpty)
            {
                mutated = true;
                return new SortedInt32KeyNode<TValue>(key, value, this, this);
            }
            SortedInt32KeyNode<TValue> sortedInt32KeyNode = this;
            if (key > _key)
            {
                SortedInt32KeyNode<TValue> right = _right.SetOrAdd(key, value, valueComparer, overwriteExistingValue, out replacedExistingValue, out mutated);
                if (mutated)
                {
                    sortedInt32KeyNode = Mutate(null, right);
                }
            }
            else if (key < _key)
            {
                SortedInt32KeyNode<TValue> left = _left.SetOrAdd(key, value, valueComparer, overwriteExistingValue, out replacedExistingValue, out mutated);
                if (mutated)
                {
                    sortedInt32KeyNode = Mutate(left);
                }
            }
            else
            {
                if (valueComparer.Equals(_value, value))
                {
                    mutated = false;
                    return this;
                }
                if (!overwriteExistingValue)
                {
                    throw new ArgumentException(System.SR.Format(MDCFR.Properties.Resources.DuplicateKey, key));
                }
                mutated = true;
                replacedExistingValue = true;
                sortedInt32KeyNode = new SortedInt32KeyNode<TValue>(key, value, _left, _right);
            }
            if (!mutated)
            {
                return sortedInt32KeyNode;
            }
            return MakeBalanced(sortedInt32KeyNode);
        }

        private SortedInt32KeyNode<TValue> RemoveRecursive(int key, out bool mutated)
        {
            if (IsEmpty)
            {
                mutated = false;
                return this;
            }
            SortedInt32KeyNode<TValue> sortedInt32KeyNode = this;
            if (key == _key)
            {
                mutated = true;
                if (_right.IsEmpty && _left.IsEmpty)
                {
                    sortedInt32KeyNode = EmptyNode;
                }
                else if (_right.IsEmpty && !_left.IsEmpty)
                {
                    sortedInt32KeyNode = _left;
                }
                else if (!_right.IsEmpty && _left.IsEmpty)
                {
                    sortedInt32KeyNode = _right;
                }
                else
                {
                    SortedInt32KeyNode<TValue> sortedInt32KeyNode2 = _right;
                    while (!sortedInt32KeyNode2._left.IsEmpty)
                    {
                        sortedInt32KeyNode2 = sortedInt32KeyNode2._left;
                    }
                    bool mutated2;
                    SortedInt32KeyNode<TValue> right = _right.Remove(sortedInt32KeyNode2._key, out mutated2);
                    sortedInt32KeyNode = sortedInt32KeyNode2.Mutate(_left, right);
                }
            }
            else if (key < _key)
            {
                SortedInt32KeyNode<TValue> left = _left.Remove(key, out mutated);
                if (mutated)
                {
                    sortedInt32KeyNode = Mutate(left);
                }
            }
            else
            {
                SortedInt32KeyNode<TValue> right2 = _right.Remove(key, out mutated);
                if (mutated)
                {
                    sortedInt32KeyNode = Mutate(null, right2);
                }
            }
            if (!sortedInt32KeyNode.IsEmpty)
            {
                return MakeBalanced(sortedInt32KeyNode);
            }
            return sortedInt32KeyNode;
        }

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        private SortedInt32KeyNode<TValue> Mutate(SortedInt32KeyNode<TValue> left = null, SortedInt32KeyNode<TValue> right = null)
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        {
            if (_frozen)
            {
                return new SortedInt32KeyNode<TValue>(_key, _value, left ?? _left, right ?? _right);
            }
            if (left != null)
            {
                _left = left;
            }
            if (right != null)
            {
                _right = right;
            }
            checked
            {
                _height = (byte)(1 + unchecked((int)Math.Max(_left._height, _right._height)));
                return this;
            }
        }
    }

    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    internal sealed class ValidatedNotNullAttribute : Attribute { }


    internal abstract class KeysOrValuesCollectionAccessor<TKey, TValue, T> : ICollection<T>, IEnumerable<T>, IEnumerable, ICollection where TKey : notnull
    {
        private readonly IImmutableDictionary<TKey, TValue> _dictionary;

        private readonly IEnumerable<T> _keysOrValues;

        public bool IsReadOnly => true;

        public int Count => _dictionary.Count;

        protected IImmutableDictionary<TKey, TValue> Dictionary => _dictionary;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool ICollection.IsSynchronized => true;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        object ICollection.SyncRoot => this;

        protected KeysOrValuesCollectionAccessor(IImmutableDictionary<TKey, TValue> dictionary, IEnumerable<T> keysOrValues)
        {
            Requires.NotNull(dictionary, "dictionary");
            Requires.NotNull(keysOrValues, "keysOrValues");
            _dictionary = dictionary;
            _keysOrValues = keysOrValues;
        }

        public void Add(T item)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public abstract bool Contains(T item);

        public void CopyTo(T[] array, int arrayIndex)
        {
            Requires.NotNull(array, "array");
            Requires.Range(arrayIndex >= 0, "arrayIndex");
            Requires.Range(array.Length >= arrayIndex + Count, "arrayIndex");
            using IEnumerator<T> enumerator = GetEnumerator();
            while (enumerator.MoveNext())
            {
                T current = enumerator.Current;
                array[arrayIndex++] = current;
            }
        }

        public bool Remove(T item)
        {
            throw new NotSupportedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _keysOrValues.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        void ICollection.CopyTo(Array array, int arrayIndex)
        {
            Requires.NotNull(array, "array");
            Requires.Range(arrayIndex >= 0, "arrayIndex");
            Requires.Range(array.Length >= arrayIndex + Count, "arrayIndex");
            using IEnumerator<T> enumerator = GetEnumerator();
            while (enumerator.MoveNext())
            {
                T current = enumerator.Current;
                array.SetValue(current, arrayIndex++);
            }
        }
    }


    internal sealed class ValuesCollectionAccessor<TKey, TValue> : KeysOrValuesCollectionAccessor<TKey, TValue, TValue> where TKey : notnull
    {
        internal ValuesCollectionAccessor(IImmutableDictionary<TKey, TValue> dictionary)
            : base(dictionary, dictionary.Values)
        {
        }

        public override bool Contains(TValue item)
        {
            if (base.Dictionary is ImmutableSortedDictionary<TKey, TValue> immutableSortedDictionary)
            {
                return immutableSortedDictionary.ContainsValue(item);
            }
            if (base.Dictionary is IImmutableDictionaryInternal<TKey, TValue> immutableDictionaryInternal)
            {
                return immutableDictionaryInternal.ContainsValue(item);
            }
            throw new NotSupportedException();
        }
    }

}

#pragma warning restore CS8767, CS8769
#pragma warning restore CS8600, CS8602, CS8618, CS8604, CS8601, CS8603
#nullable disable