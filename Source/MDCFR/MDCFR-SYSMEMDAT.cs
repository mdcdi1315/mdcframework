/*
  * Licensed to the .NET Foundation under one or more agreements.
  * The .NET Foundation licenses this file to you under the MIT license.
  * See the LICENSE file in the project root for more information.
*/

using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

#nullable enable
namespace System
{
    /// <summary>
	/// A lightweight abstraction for a payload of bytes that supports converting between string, stream, JSON, and bytes.
	/// </summary>
	[JsonConverter(typeof(BinaryDataConverter))]
    public class BinaryData
    {
        private const string JsonSerializerRequiresUnreferencedCode = "JSON serialization and deserialization might require types that cannot be statically analyzed.";

        /// <summary>
        /// The backing store for the <see cref="T:System.BinaryData" /> instance.
        /// </summary>
        private readonly ReadOnlyMemory<byte> _bytes;

        /// <summary>
        /// Returns an empty BinaryData.
        /// </summary>
        public static BinaryData Empty { get; } = new BinaryData(ReadOnlyMemory<byte>.Empty);


        /// <summary>
        /// Creates a <see cref="T:System.BinaryData" /> instance by wrapping the
        /// provided byte array.
        /// </summary>
        /// <param name="data">The array to wrap.</param>
        public BinaryData(byte[] data)
        {
            if (data == null) { throw new ArgumentNullException("data"); }
            _bytes = (ReadOnlyMemory<byte>)data;
        }

        /// <summary>
        /// Creates a <see cref="T:System.BinaryData" /> instance by serializing the provided object to JSON
        /// using <see cref="T:System.Text.Json.JsonSerializer" />.
        /// </summary>
        /// <param name="jsonSerializable">The object that will be serialized to JSON using
        /// <see cref="T:System.Text.Json.JsonSerializer" />.</param>
        /// <param name="options">The options to use when serializing to JSON.</param>
        /// <param name="type">The type to use when serializing the data. If not specified, <see cref="M:System.Object.GetType" /> will
        /// be used to determine the type.</param>
        [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("JSON serialization and deserialization might require types that cannot be statically analyzed.")]
        public BinaryData(object? jsonSerializable, JsonSerializerOptions? options = null, Type? type = null)
        {
            if (type == null) { type = jsonSerializable?.GetType() ?? typeof(object); }
            _bytes = (ReadOnlyMemory<byte>)JsonSerializer.SerializeToUtf8Bytes(jsonSerializable, type, options);
        }

        /// <summary>
        /// Creates a <see cref="T:System.BinaryData" /> instance by serializing the provided object to JSON
        /// using <see cref="T:System.Text.Json.JsonSerializer" />.
        /// </summary>
        /// <param name="jsonSerializable">The object that will be serialized to JSON using
        /// <see cref="T:System.Text.Json.JsonSerializer" />.</param>
        /// <param name="context">The <see cref="T:System.Text.Json.Serialization.JsonSerializerContext" /> to use when serializing to JSON.</param>
        /// <param name="type">The type to use when serializing the data. If not specified, <see cref="M:System.Object.GetType" /> will
        /// be used to determine the type.</param>
        public BinaryData(object? jsonSerializable, JsonSerializerContext context, Type? type = null)
        {
            if (type == null) { type = jsonSerializable?.GetType() ?? typeof(object); }
            _bytes = (ReadOnlyMemory<byte>)JsonSerializer.SerializeToUtf8Bytes(jsonSerializable, type, context);
        }

        /// <summary>
        /// Creates a <see cref="System.BinaryData" /> instance by wrapping the
        /// provided bytes.
        /// </summary>
        /// <param name="data">Byte data to wrap.</param>
        public BinaryData(ReadOnlyMemory<byte> data) { _bytes = data; }

        /// <summary>
        /// Creates a <see cref="System.BinaryData" /> instance from a string by converting
        /// the string to bytes using the UTF-8 encoding.
        /// </summary>
        /// <param name="data">The string data.</param>
        public BinaryData(string data)
        {
            if (data == null) { throw new ArgumentNullException("data"); }
            _bytes = (ReadOnlyMemory<byte>)Encoding.UTF8.GetBytes(data);
        }

        /// <summary>
        /// Creates a <see cref="System.BinaryData" /> instance by wrapping the provided
        /// <see cref="T:System.ReadOnlyMemory`1" />.
        /// </summary>
        /// <param name="data">Byte data to wrap.</param>
        /// <returns>A wrapper over <paramref name="data" />.</returns>
        public static BinaryData FromBytes(ReadOnlyMemory<byte> data) { return new BinaryData(data); }

        /// <summary>
        /// Creates a <see cref="System.BinaryData" /> instance by wrapping the provided
        /// byte array.
        /// </summary>
        /// <param name="data">The array to wrap.</param>
        /// <returns>A wrapper over <paramref name="data" />.</returns>
        public static BinaryData FromBytes(byte[] data) { return new BinaryData(data); }

        /// <summary>
        /// Creates a <see cref="T:System.BinaryData" /> instance from a string by converting
        /// the string to bytes using the UTF-8 encoding.
        /// </summary>
        /// <param name="data">The string data.</param>
        /// <returns>A value representing the UTF-8 encoding of <paramref name="data" />.</returns>
        public static BinaryData FromString(string data) { return new BinaryData(data); }

        /// <summary>
        /// Creates a <see cref="T:System.BinaryData" /> instance from the specified stream.
        /// The stream is not disposed by this method.
        /// </summary>
        /// <param name="stream">Stream containing the data.</param>
        /// <returns>A value representing all of the data remaining in <paramref name="stream" />.</returns>
        public static BinaryData FromStream(Stream stream)
        {
            if (stream == null) { throw new ArgumentNullException("stream"); }
            return FromStreamAsync(stream, async: false).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Creates a <see cref="T:System.BinaryData" /> instance from the specified stream.
        /// The stream is not disposed by this method.
        /// </summary>
        /// <param name="stream">Stream containing the data.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        /// <returns>A value representing all of the data remaining in <paramref name="stream" />.</returns>
        public static Task<BinaryData> FromStreamAsync(Stream stream, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (stream == null) { throw new ArgumentNullException("stream"); }
            return FromStreamAsync(stream, async: true, cancellationToken);
        }

        private static async Task<BinaryData> FromStreamAsync(Stream stream, bool async, CancellationToken cancellationToken = default(CancellationToken))
        {
            int bufferSize = 81920;
            MemoryStream memoryStream;
            if (stream.CanSeek)
            {
                long num = stream.Length - stream.Position;
                if (num > int.MaxValue || num < 0)
                {
                    throw new ArgumentOutOfRangeException("stream", MDCFR.Properties.Resources.ArgumentOutOfRange_StreamLengthMustBeNonNegativeInt32);
                }
                bufferSize = ((num == 0L) ? 1 : Math.Min((int)num, 81920));
                memoryStream = new MemoryStream((int)num);
            }
            else
            {
                memoryStream = new MemoryStream();
            }
            using (memoryStream)
            {
                if (async)
                {
                    await stream.CopyToAsync(memoryStream, bufferSize, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                }
                else
                {
                    stream.CopyTo(memoryStream, bufferSize);
                }
                return new BinaryData(MemoryExtensions.AsMemory(memoryStream.GetBuffer(), 0, (int)memoryStream.Position));
            }
        }

        /// <summary>
        /// Creates a <see cref="T:System.BinaryData" /> instance by serializing the provided object using
        /// the <see cref="T:System.Text.Json.JsonSerializer" />.
        /// </summary>
        /// <typeparam name="T">The type to use when serializing the data.</typeparam>
        /// <param name="jsonSerializable">The data to use.</param>
        /// <param name="options">The options to use when serializing to JSON.</param>
        /// <returns>A value representing the UTF-8 encoding of the JSON representation of <paramref name="jsonSerializable" />.</returns>
        [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("JSON serialization and deserialization might require types that cannot be statically analyzed.")]
        public static BinaryData FromObjectAsJson<T>(T jsonSerializable, JsonSerializerOptions? options = null)
        {
            byte[] data = JsonSerializer.SerializeToUtf8Bytes(jsonSerializable, typeof(T), options);
            return new BinaryData(data);
        }

        /// <summary>
        /// Creates a <see cref="T:System.BinaryData" /> instance by serializing the provided object using
        /// the <see cref="T:System.Text.Json.JsonSerializer" />.
        /// </summary>
        /// <typeparam name="T">The type to use when serializing the data.</typeparam>
        /// <param name="jsonSerializable">The data to use.</param>
        /// <param name="jsonTypeInfo">The <see cref="T:System.Text.Json.Serialization.Metadata.JsonTypeInfo" /> to use when serializing to JSON.</param>
        /// <returns>A value representing the UTF-8 encoding of the JSON representation of <paramref name="jsonSerializable" />.</returns>
        public static BinaryData FromObjectAsJson<T>(T jsonSerializable, JsonTypeInfo<T> jsonTypeInfo)
        {
            return new BinaryData(JsonSerializer.SerializeToUtf8Bytes(jsonSerializable, jsonTypeInfo));
        }

        /// <summary>
        /// Converts the value of this instance to a string using UTF-8.
        /// </summary>
        /// <remarks>
        /// No special treatment is given to the contents of the data, it is merely decoded as a UTF-8 string.
        /// For a JPEG or other binary file format the string will largely be nonsense with many embedded NUL characters,
        /// and UTF-8 JSON values will look like their file/network representation,
        /// including starting and stopping quotes on a string.
        /// </remarks>
        /// <returns>
        /// A string from the value of this instance, using UTF-8 to decode the bytes.
        /// </returns>
        /// <seealso cref="M:System.BinaryData.ToObjectFromJson``1(System.Text.Json.JsonSerializerOptions)" />
        public unsafe override string ToString()
        {
            System.ReadOnlySpan<byte> span = _bytes.Span;
            if (span.IsEmpty)
            {
                return string.Empty;
            }
            fixed (byte* bytes = span)
            {
                return Encoding.UTF8.GetString(bytes, span.Length);
            }
        }

        /// <summary>
        /// Converts the <see cref="T:System.BinaryData" /> to a read-only stream.
        /// </summary>
        /// <returns>A stream representing the data.</returns>
        public Stream ToStream() { return new ReadOnlyMemoryStream(_bytes); }

        /// <summary>
        /// Gets the value of this instance as bytes without any further interpretation.
        /// </summary>
        /// <returns>The value of this instance as bytes without any further interpretation.</returns>
        public ReadOnlyMemory<byte> ToMemory() { return _bytes; }

        /// <summary>
        /// Converts the <see cref="BinaryData" /> to a byte array.
        /// </summary>
        /// <returns>A byte array representing the data.</returns>
        public byte[] ToArray() { return _bytes.ToArray(); }

        /// <summary>
        /// Converts the <see cref="BinaryData" /> to the specified type using
        /// <see cref="JsonSerializer" />.
        /// </summary>
        /// <typeparam name="T">The type that the data should be
        /// converted to.</typeparam>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use when serializing to JSON.</param>
        /// <returns>The data converted to the specified type.</returns>
        [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("JSON serialization and deserialization might require types that cannot be statically analyzed.")]
        public unsafe T? ToObjectFromJson<T>(JsonSerializerOptions? options = null)
        {
            System.ReadOnlySpan<byte> utf8Json = _bytes.Span;
            if (utf8Json.Length > 2 && *(byte*)utf8Json[0] == 239 && *(byte*)utf8Json[1] == 187 && *(byte*)utf8Json[2] == 191)
            {
                utf8Json = utf8Json.Slice(3);
            }
            return JsonSerializer.Deserialize<T>(utf8Json, options);
        }

        /// <summary>
        /// Converts the <see cref="BinaryData" /> to the specified type using
        /// <see cref="JsonSerializer" />.
        /// </summary>
        /// <typeparam name="T">The type that the data should be
        /// converted to.</typeparam>
        /// <param name="jsonTypeInfo">The <see cref="JsonTypeInfo" /> to use when serializing to JSON.</param>
        /// <returns>The data converted to the specified type.</returns>
        public T? ToObjectFromJson<T>(JsonTypeInfo<T> jsonTypeInfo)
        {
            return JsonSerializer.Deserialize(_bytes.Span, jsonTypeInfo);
        }

        /// <summary>
        /// Defines an implicit conversion from a <see cref="T:System.BinaryData" /> to a <see cref="T:System.ReadOnlyMemory`1" />.
        /// </summary>
        /// <param name="data">The value to be converted.</param>
        public static implicit operator ReadOnlyMemory<byte>(BinaryData? data) { return data?._bytes ?? default(ReadOnlyMemory<byte>); }

        /// <summary>
        /// Defines an implicit conversion from a <see cref="T:System.BinaryData" /> to a <see cref="T:System.ReadOnlySpan`1" />.
        /// </summary>
        /// <param name="data">The value to be converted.</param>
        public static implicit operator System.ReadOnlySpan<byte>(BinaryData? data)
        {
            if (data == null) { return default; }
            return data._bytes.Span;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>
        /// <see langword="true" /> if the specified object is equal to the current object; otherwise, <see langword="false" />.
        /// </returns>
        [ComponentModel.EditorBrowsable(ComponentModel.EditorBrowsableState.Never)]
        public override bool Equals([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] object? obj)
        {
            return this == obj;
        }

        /// <inheritdoc />
        [ComponentModel.EditorBrowsable(ComponentModel.EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    internal sealed class BinaryDataConverter : JsonConverter<BinaryData>
    {
        public sealed override BinaryData Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return BinaryData.FromBytes(reader.GetBytesFromBase64());
        }

        public sealed override void Write(Utf8JsonWriter writer, BinaryData value, JsonSerializerOptions options)
        {
            writer.WriteBase64StringValue(value.ToMemory().Span);
        }
    }

}

namespace System.Text.Json.Serialization
{
    /// <summary>
    /// Serializes <see cref="BinaryData"/> instances as Base64 JSON strings.
    /// </summary>
    public sealed class BinaryDataJsonConverter : JsonConverter<BinaryData>
    {
        /// <inheritdoc/>
        public override BinaryData? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return BinaryData.FromBytes(reader.GetBytesFromBase64());
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, BinaryData value, JsonSerializerOptions options)
        {
            writer.WriteBase64StringValue(value.ToMemory().Span);
        }
    }
}
