//The unique System.Resources.Extensions could be fully ported to .NET Framework 4.8. !!!
// So , it was included here.
// It is only a 3000-line code that makes it work.
// And the best: Compiler is not complaining about the missing System.Resources.Extensions DLL , 
// because it IS the DLL itself.

// License excerpt by .NET Foundation: 

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


using System.IO;
using System.Text;
using System.Reflection;
using System.Threading;
using System.Collections;
using System.Diagnostics;
using System.Buffers.Binary;
using System.ComponentModel;
using System.Numerics.Hashing;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization.Formatters.Binary;


namespace System.Resources.Extensions
{

    using ResourceReader = System.Resources.Extensions.DeserializingResourceReader;

    #pragma warning disable CS1591 , CS8602
    #nullable enable

    public partial class DeserializingResourceReader
    {
        private bool _assumeBinaryFormatter;
        private BinaryFormatter? _formatter;

        private bool ValidateReaderType(string readerType)
        {
            // our format?
            if (TypeNameComparer.Instance.Equals(readerType, PreserializedResourceWriter.DeserializingResourceReaderFullyQualifiedName))
            {
                return true;
            }

            // default format?
            if (TypeNameComparer.Instance.Equals(readerType, PreserializedResourceWriter.ResourceReaderFullyQualifiedName))
            {
                // we can read the default format, we just assume BinaryFormatter and don't
                // read the SerializationFormat
                _assumeBinaryFormatter = true;
                return true;
            }

            return false;
        }

        // Issue https://github.com/dotnet/runtime/issues/39292 tracks finding an alternative to BinaryFormatter
        private object ReadBinaryFormattedObject()
        {
            _formatter ??= new BinaryFormatter()
            {
                Binder = new UndoTruncatedTypeNameSerializationBinder()
            };

            return _formatter.Deserialize(_store.BaseStream);
        }

        internal sealed class UndoTruncatedTypeNameSerializationBinder : SerializationBinder
        {
            public override Type? BindToType(string assemblyName, string typeName)
            {
                Type? type = null;

                // determine if we have a mangled generic type name
                if (typeName != null && assemblyName != null && !AreBracketsBalanced(typeName))
                {
                    // undo the mangling that may have happened with .NETFramework's
                    // incorrect ResXSerialization binder.
                    typeName = typeName + ", " + assemblyName;

                    type = Type.GetType(typeName, throwOnError: false, ignoreCase: false);
                }

                // if type is null we'll fall back to the default type binder which is preferable
                // since it is backed by a cache
                return type;
            }

            private static bool AreBracketsBalanced(string typeName)
            {
                // make sure brackets are balanced
                int firstBracket = typeName.IndexOf('[');

                if (firstBracket == -1)
                {
                    return true;
                }

                int brackets = 1;
                for (int i = firstBracket + 1; i < typeName.Length; i++)
                {
                    if (typeName[i] == '[')
                    {
                        brackets++;
                    }
                    else if (typeName[i] == ']')
                    {
                        brackets--;

                        if (brackets < 0)
                        {
                            // unbalanced, closing bracket without opening
                            break;
                        }
                    }
                }

                return brackets == 0;
            }

        }

        private object DeserializeObject(int typeIndex)
        {
            Type type = FindType(typeIndex);

            if (_assumeBinaryFormatter)
            {
                return ReadBinaryFormattedObject();
            }

            // read type
            SerializationFormat format = (SerializationFormat)_store.Read7BitEncodedInt();

            object value;

            // read data
            switch (format)
            {
                case SerializationFormat.BinaryFormatter:
                    {
                        // read length
                        int length = _store.Read7BitEncodedInt();
                        if (length < 0)
                        {
                            throw new BadImageFormatException(SR.Format(MDCFR.Properties.Resources.BadImageFormat_ResourceDataLengthInvalid, length));
                        }

                        long originalPosition = _store.BaseStream.Position;

                        value = ReadBinaryFormattedObject();

                        if (type == typeof(UnknownType))
                        {
                            // type information was omitted at the time of writing
                            // allow the payload to define the type
                            type = value.GetType();
                        }

                        long bytesRead = _store.BaseStream.Position - originalPosition;

                        // Ensure BF read what we expected.
                        if (bytesRead != length)
                        {
                            throw new BadImageFormatException(SR.Format(MDCFR.Properties.Resources.BadImageFormat_ResourceDataLengthInvalid, length));
                        }
                        break;
                    }
                case SerializationFormat.TypeConverterByteArray:
                    {
                        // read length
                        int length = _store.Read7BitEncodedInt();
                        if (length < 0)
                        {
                            throw new BadImageFormatException(SR.Format(MDCFR.Properties.Resources.BadImageFormat_ResourceDataLengthInvalid, length));
                        }

                        byte[] data = _store.ReadBytes(length);

                        TypeConverter converter = TypeDescriptor.GetConverter(type);

                        if (converter == null)
                        {
                            throw new TypeLoadException(SR.Format(MDCFR.Properties.Resources.TypeLoadException_CannotLoadConverter, type));
                        }

                        value = converter.ConvertFrom(data)!;
                        break;
                    }
                case SerializationFormat.TypeConverterString:
                    {
                        string stringData = _store.ReadString();

                        TypeConverter converter = TypeDescriptor.GetConverter(type);

                        if (converter == null)
                        {
                            throw new TypeLoadException(SR.Format(MDCFR.Properties.Resources.TypeLoadException_CannotLoadConverter, type));
                        }

                        value = converter.ConvertFromInvariantString(stringData)!;
                        break;
                    }
                case SerializationFormat.ActivatorStream:
                    {
                        // read length
                        int length = _store.Read7BitEncodedInt();
                        if (length < 0)
                        {
                            throw new BadImageFormatException(SR.Format(MDCFR.Properties.Resources.BadImageFormat_ResourceDataLengthInvalid, length));
                        }
                        Stream stream;

                        if (_store.BaseStream is UnmanagedMemoryStream ums)
                        {
                            // For the case that we've memory mapped in the .resources
                            // file, just return a Stream pointing to that block of memory.
                            unsafe
                            {
                                stream = new UnmanagedMemoryStream(ums.PositionPointer, length, length, FileAccess.Read);
                            }
                        }
                        else
                        {

                            byte[] bytes = _store.ReadBytes(length);
                            // Lifetime of memory == lifetime of this stream.
                            stream = new MemoryStream(bytes, false);
                        }

                        value = Activator.CreateInstance(type, new object[] { stream })!;
                        break;
                    }
                default:
                    throw new BadImageFormatException(MDCFR.Properties.Resources.BadImageFormat_TypeMismatch);
            }

            // Make sure we deserialized the type that we expected.
            // This protects against bad typeconverters or bad binaryformatter payloads.
            if (value.GetType() != type)
                throw new BadImageFormatException(SR.Format(MDCFR.Properties.Resources.BadImageFormat_ResType_SerBlobMismatch, type.FullName, value.GetType().FullName));

            return value;
        }

    }

    internal sealed class UnknownType { }

    internal class PrecannedResource
    {
        public System.Object Data;
        public System.String TypeName;
        
        public PrecannedResource(System.String typeName ,  System.Object data)
        {
            Data = data;
            TypeName = typeName;
        }
    }

    internal struct ResourceDataRecord
    {
        public System.Boolean CloseAfterWrite;
        public System.Resources.Extensions.SerializationFormat Format;
        public System.Object Data;

        public ResourceDataRecord(System.Resources.Extensions.SerializationFormat format ,
            System.Object data , System.Boolean closeAfterWrite = false)
        {
            Format = format;
            Data = data;
            CloseAfterWrite = closeAfterWrite;
        }
    }

    internal struct StreamWrapper
    {
        public System.Boolean CloseAfterWrite;
        public System.IO.Stream Stream;

        public StreamWrapper(System.IO.Stream stream ,  System.Boolean closeAfterWrite)
        {
            CloseAfterWrite = closeAfterWrite;
            Stream = stream;
        }
    }

    public partial class PreserializedResourceWriter : IResourceWriter , IDisposable
    {

        // An initial size for our internal sorted list, to avoid extra resizes.
        private const int AverageNameSize = 20 * 2;  // chars in little endian Unicode
        internal const string ResourceReaderFullyQualifiedName = "System.Resources.ResourceReader, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
        private const string ResSetTypeName = "System.Resources.RuntimeResourceSet";
        private const int ResSetVersion = 2;

        private SortedDictionary<string, object?>? _resourceList;
        private Stream _output;
        private Dictionary<string, object?> _caseInsensitiveDups;
        private Dictionary<string, PrecannedResource>? _preserializedData;

        PreserializedResourceWriter(string fileName)
        {
            if (fileName is null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            _output = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
            _resourceList = new SortedDictionary<string, object?>(FastResourceComparer.Default);
            _caseInsensitiveDups = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        }

        PreserializedResourceWriter(Stream stream)
        {
            if (stream is null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (!stream.CanWrite)
            {
                throw new ArgumentException(MDCFR.Properties.Resources.Argument_StreamNotWritable);
            }

            _output = stream;
            _resourceList = new SortedDictionary<string, object?>(FastResourceComparer.Default);
            _caseInsensitiveDups = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Adds a <see cref="System.String"/> resource to the list of resources to be written to a file.
        /// They aren't written until <see cref="Generate()"/> is called.
        /// </summary>
        public void AddResource(string name, string? value)
        {
            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (_resourceList == null)
            {
                throw new InvalidOperationException(MDCFR.Properties.Resources.InvalidOperation_ResourceWriterSaved);
            }

            // Check for duplicate resources whose names vary only by case.
            _caseInsensitiveDups.Add(name, null);
            _resourceList.Add(name, value);
        }

        /// <summary>
        /// Adds a <see cref="System.Object"/> resource to the list of resources to be written to a file.
        /// They aren't written until <see cref="Generate()"/> is called.
        /// </summary>
        public void AddResource(string name, object? value)
        {
            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (_resourceList == null)
            {
                throw new InvalidOperationException(MDCFR.Properties.Resources.InvalidOperation_ResourceWriterSaved);
            }

            // needed for binary compat
            if (value != null && value is Stream)
            {
                AddResourceInternal(name, (Stream)value, false);
            }
            else
            {
                // Check for duplicate resources whose names vary only by case.
                _caseInsensitiveDups.Add(name, null);
                _resourceList.Add(name, value);
            }
        }

        /// <summary>
        /// Adds a named <see cref="System.Byte"/>[] resource to the list of resources to be written to a file.
        /// They aren't written until <see cref="Generate()"/> is called.
        /// </summary>
        public void AddResource(string name, byte[]? value)
        {
            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (_resourceList == null)
            {
                throw new InvalidOperationException(MDCFR.Properties.Resources.InvalidOperation_ResourceWriterSaved);
            }

            // Check for duplicate resources whose names vary only by case.
            _caseInsensitiveDups.Add(name, null);
            _resourceList.Add(name, value);
        }

        /// <summary>
        /// Adds a resource of type Stream to the list of resources to be
        /// written to a file.  They aren't written until Generate() is called.
        /// closeAfterWrite parameter indicates whether to close the stream when done.
        /// </summary>
        public void AddResource(string name, Stream? value, bool closeAfterWrite = false)
        {
            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (_resourceList == null)
            {
                throw new InvalidOperationException(MDCFR.Properties.Resources.InvalidOperation_ResourceWriterSaved);
            }

            AddResourceInternal(name, value, closeAfterWrite);
        }

        private void AddResourceInternal(string name, Stream? value, bool closeAfterWrite)
        {
            Debug.Assert(_resourceList != null);

            if (value == null)
            {
                // Check for duplicate resources whose names vary only by case.
                _caseInsensitiveDups.Add(name, null);
                _resourceList.Add(name, value);
            }
            else
            {
                // make sure the Stream is seekable
                if (!value.CanSeek)
                    throw new ArgumentException(MDCFR.Properties.Resources.NotSupported_UnseekableStream);

                // Check for duplicate resources whose names vary only by case.
                _caseInsensitiveDups.Add(name, null);
                _resourceList.Add(name, new StreamWrapper(value, closeAfterWrite));
            }
        }

        public void Close()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_resourceList != null)
                {
                    Generate();
                }
                _output?.Dispose();
            }

            _output = null!;
            _caseInsensitiveDups = null!;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// After calling AddResource, Generate() writes out all resources to the
        /// output stream in the system default format.
        /// If an exception occurs during object serialization or during IO,
        /// the .resources file is closed and deleted, since it is most likely
        /// invalid.
        /// </summary>
        public void Generate()
        {
            if (_resourceList == null)
                throw new InvalidOperationException(MDCFR.Properties.Resources.InvalidOperation_ResourceWriterSaved);

            BinaryWriter bw = new BinaryWriter(_output, Encoding.UTF8);
            List<string> typeNames = new List<string>();

            // Write out the ResourceManager header
            // Write out magic number
            bw.Write(ResourceManager.MagicNumber);

            // Write out ResourceManager header version number
            bw.Write(ResourceManager.HeaderVersionNumber);

            MemoryStream resMgrHeaderBlob = new MemoryStream(240);
            BinaryWriter resMgrHeaderPart = new BinaryWriter(resMgrHeaderBlob);

            // Write out class name of IResourceReader capable of handling
            // this file.
            resMgrHeaderPart.Write(ResourceReaderTypeName);

            // Write out class name of the ResourceSet class best suited to
            // handling this file.
            // This needs to be the same even with multi-targeting. It's the
            // full name -- not the assembly qualified name.
            resMgrHeaderPart.Write(ResourceSetTypeName);
            resMgrHeaderPart.Flush();

            // Write number of bytes to skip over to get past ResMgr header
            bw.Write((int)resMgrHeaderBlob.Length);

            // Write the rest of the ResMgr header
            Debug.Assert(resMgrHeaderBlob.Length > 0, "ResourceWriter: Expected non empty header");
            resMgrHeaderBlob.Seek(0, SeekOrigin.Begin);
            resMgrHeaderBlob.CopyTo(bw.BaseStream, (int)resMgrHeaderBlob.Length);
            // End ResourceManager header


            // Write out the RuntimeResourceSet header
            // Version number
            bw.Write(ResSetVersion);

            // number of resources
            int numResources = _resourceList.Count;
            if (_preserializedData != null)
                numResources += _preserializedData.Count;
            bw.Write(numResources);

            // Store values in temporary streams to write at end of file.
            int[] nameHashes = new int[numResources];
            int[] namePositions = new int[numResources];
            int curNameNumber = 0;
            MemoryStream nameSection = new MemoryStream(numResources * AverageNameSize);
            BinaryWriter names = new BinaryWriter(nameSection, Encoding.Unicode);

            Stream dataSection = new MemoryStream();  // Either a FileStream or a MemoryStream

            using (dataSection)
            {
                BinaryWriter data = new BinaryWriter(dataSection, Encoding.UTF8);

                if (_preserializedData != null)
                {
                    foreach (KeyValuePair<string, PrecannedResource> entry in _preserializedData)
                    {
                        _resourceList.Add(entry.Key, entry.Value);
                    }
                }

                // Write resource name and position to the file, and the value
                // to our temporary buffer.  Save Type as well.
                foreach (var item in _resourceList)
                {
                    nameHashes[curNameNumber] = FastResourceComparer.HashFunction(item.Key);
                    namePositions[curNameNumber++] = (int)names.Seek(0, SeekOrigin.Current);
                    names.Write(item.Key); // key
                    names.Write((int)data.Seek(0, SeekOrigin.Current)); // virtual offset of value.

                    object? value = item.Value;
                    ResourceTypeCode typeCode = FindTypeCode(value, typeNames);

                    // Write out type code
                    data.Write7BitEncodedInt((int)typeCode);

                    var userProvidedResource = value as PrecannedResource;
                    if (userProvidedResource != null)
                    {
                        WriteData(data, userProvidedResource.Data);
                    }
                    else
                    {
                        WriteValue(typeCode, value, data);
                    }
                }

                // At this point, the ResourceManager header has been written.
                // Finish RuntimeResourceSet header
                // The reader expects a list of user defined type names
                // following the size of the list, write 0 for this
                // writer implementation
                bw.Write(typeNames.Count);
                foreach (var typeName in typeNames)
                {
                    bw.Write(typeName);
                }

                // Write out the name-related items for lookup.
                //  Note that the hash array and the namePositions array must
                //  be sorted in parallel.
                Array.Sort(nameHashes, namePositions);


                //  Prepare to write sorted name hashes (alignment fixup)
                //   Note: For 64-bit machines, these MUST be aligned on 8 byte
                //   boundaries!  Pointers on IA64 must be aligned!  And we'll
                //   run faster on X86 machines too.
                bw.Flush();
                int alignBytes = ((int)bw.BaseStream.Position) & 7;
                if (alignBytes > 0)
                {
                    for (int i = 0; i < 8 - alignBytes; i++)
                        bw.Write("PAD"[i % 3]);
                }

                //  Write out sorted name hashes.
                //   Align to 8 bytes.
                Debug.Assert((bw.BaseStream.Position & 7) == 0, "ResourceWriter: Name hashes array won't be 8 byte aligned!  Ack!");

                foreach (int hash in nameHashes)
                {
                    bw.Write(hash);
                }

                //  Write relative positions of all the names in the file.
                //   Note: this data is 4 byte aligned, occurring immediately
                //   after the 8 byte aligned name hashes (whose length may
                //   potentially be odd).
                Debug.Assert((bw.BaseStream.Position & 3) == 0, "ResourceWriter: Name positions array won't be 4 byte aligned!  Ack!");

                foreach (int pos in namePositions)
                {
                    bw.Write(pos);
                }

                // Flush all BinaryWriters to their underlying streams.
                bw.Flush();
                names.Flush();
                data.Flush();

                // Write offset to data section
                int startOfDataSection = (int)(bw.Seek(0, SeekOrigin.Current) + nameSection.Length);
                startOfDataSection += 4;  // We're writing an int to store this data, adding more bytes to the header
                bw.Write(startOfDataSection);

                // Write name section.
                if (nameSection.Length > 0)
                {
                    nameSection.Seek(0, SeekOrigin.Begin);
                    nameSection.CopyTo(bw.BaseStream, (int)nameSection.Length);
                }
                names.Dispose();

                // Write data section.
                Debug.Assert(startOfDataSection == bw.Seek(0, SeekOrigin.Current), "ResourceWriter::Generate - start of data section is wrong!");
                dataSection.Position = 0;
                dataSection.CopyTo(bw.BaseStream);
                data.Dispose();
            } // using(dataSection)  <--- Closes dataSection, which was opened w/ FileOptions.DeleteOnClose
            bw.Flush();

            // Indicate we've called Generate
            _resourceList = null;
        }

        private static void WriteValue(ResourceTypeCode typeCode, object? value, BinaryWriter writer)
        {
            Debug.Assert(writer != null);

            switch (typeCode)
            {
                case ResourceTypeCode.Null:
                    break;

                case ResourceTypeCode.String:
                    writer.Write((string)value!);
                    break;

                case ResourceTypeCode.Boolean:
                    writer.Write((bool)value!);
                    break;

                case ResourceTypeCode.Char:
                    writer.Write((ushort)(char)value!);
                    break;

                case ResourceTypeCode.Byte:
                    writer.Write((byte)value!);
                    break;

                case ResourceTypeCode.SByte:
                    writer.Write((sbyte)value!);
                    break;

                case ResourceTypeCode.Int16:
                    writer.Write((short)value!);
                    break;

                case ResourceTypeCode.UInt16:
                    writer.Write((ushort)value!);
                    break;

                case ResourceTypeCode.Int32:
                    writer.Write((int)value!);
                    break;

                case ResourceTypeCode.UInt32:
                    writer.Write((uint)value!);
                    break;

                case ResourceTypeCode.Int64:
                    writer.Write((long)value!);
                    break;

                case ResourceTypeCode.UInt64:
                    writer.Write((ulong)value!);
                    break;

                case ResourceTypeCode.Single:
                    writer.Write((float)value!);
                    break;

                case ResourceTypeCode.Double:
                    writer.Write((double)value!);
                    break;

                case ResourceTypeCode.Decimal:
                    writer.Write((decimal)value!);
                    break;

                case ResourceTypeCode.DateTime:
                    // Use DateTime's ToBinary & FromBinary.
                    long data = ((DateTime)value!).ToBinary();
                    writer.Write(data);
                    break;

                case ResourceTypeCode.TimeSpan:
                    writer.Write(((TimeSpan)value!).Ticks);
                    break;

                // Special Types
                case ResourceTypeCode.ByteArray:
                    {
                        byte[] bytes = (byte[])value!;
                        writer.Write(bytes.Length);
                        writer.Write(bytes, 0, bytes.Length);
                        break;
                    }

                case ResourceTypeCode.Stream:
                    {
                        StreamWrapper sw = (StreamWrapper)value!;
                        if (sw.Stream.GetType() == typeof(MemoryStream))
                        {
                            MemoryStream ms = (MemoryStream)sw.Stream;
                            if (ms.Length > int.MaxValue)
                                throw new ArgumentException(MDCFR.Properties.Resources.ArgumentOutOfRange_StreamLength);
                            byte[] arr = ms.ToArray();
                            writer.Write(arr.Length);
                            writer.Write(arr, 0, arr.Length);
                        }
                        else
                        {
                            Stream s = sw.Stream;
                            // we've already verified that the Stream is seekable
                            if (s.Length > int.MaxValue)
                                throw new ArgumentException(MDCFR.Properties.Resources.ArgumentOutOfRange_StreamLength);

                            s.Position = 0;
                            writer.Write((int)s.Length);
                            byte[] buffer = new byte[4096];
                            int read;
                            while ((read = s.Read(buffer, 0, buffer.Length)) != 0)
                            {
                                writer.Write(buffer, 0, read);
                            }
                            if (sw.CloseAfterWrite)
                            {
                                s.Close();
                            }
                        }
                        break;
                    }

                default:
                    Debug.Assert(typeCode >= ResourceTypeCode.StartOfUserTypes, $"ResourceReader: Unsupported ResourceTypeCode in .resources file!  {typeCode}");
                    throw new PlatformNotSupportedException(MDCFR.Properties.Resources.NotSupported_BinarySerializedResources);
            }
        }

        // indicates if the types of resources saved will require the DeserializingResourceReader
        // in order to read them.
        private bool _requiresDeserializingResourceReader;

        // use hard-coded strings rather than typeof so that the version doesn't leak into resources files
        internal const string DeserializingResourceReaderFullyQualifiedName = "System.Resources.Extensions.DeserializingResourceReader, System.Resources.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51";
        internal const string RuntimeResourceSetFullyQualifiedName = "System.Resources.Extensions.RuntimeResourceSet, System.Resources.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51";

        internal const string ResReaderTypeName = "System.Resources.ResourceReader";
        internal const string ResFileExtension = ".resources";

        // an internal type name used to represent an unknown resource type, explicitly omit version to save
        // on size and avoid changes in user resources.  This works since we only ever load this type name
        // from calls to GetType from this assembly.
        private static readonly string UnknownObjectTypeName = typeof(UnknownType).FullName!;

        private string ResourceReaderTypeName => _requiresDeserializingResourceReader ?
            DeserializingResourceReaderFullyQualifiedName :
            ResourceReaderFullyQualifiedName;

        private string ResourceSetTypeName => _requiresDeserializingResourceReader ?
            RuntimeResourceSetFullyQualifiedName :
            ResSetTypeName;

        // a collection of primitive types in a dictionary, indexed by type name
        // using a comparer which handles type name comparisons similar to what
        // is done by reflection
        private static readonly IReadOnlyDictionary<string, Type> s_primitiveTypes = new Dictionary<string, Type>(16, TypeNameComparer.Instance)
        {
            { typeof(string).FullName!, typeof(string) },
            { typeof(int).FullName!, typeof(int) },
            { typeof(bool).FullName!, typeof(bool) },
            { typeof(char).FullName!, typeof(char) },
            { typeof(byte).FullName!, typeof(byte) },
            { typeof(sbyte).FullName!, typeof(sbyte) },
            { typeof(short).FullName!, typeof(short) },
            { typeof(long).FullName!, typeof(long) },
            { typeof(ushort).FullName!, typeof(ushort) },
            { typeof(uint).FullName!, typeof(uint) },
            { typeof(ulong).FullName!, typeof(ulong) },
            { typeof(float).FullName!, typeof(float) },
            { typeof(double).FullName!, typeof(double) },
            { typeof(decimal).FullName!, typeof(decimal) },
            { typeof(DateTime).FullName!, typeof(DateTime) },
            { typeof(TimeSpan).FullName!, typeof(TimeSpan) }
            // byte[] and Stream are primitive types but do not define a conversion from string
        };

        private void AddResourceData(string name, string typeName, object data)
        {
            if (_resourceList == null)
                throw new InvalidOperationException(MDCFR.Properties.Resources.InvalidOperation_ResourceWriterSaved);

            // Check for duplicate resources whose names vary only by case.
            _caseInsensitiveDups.Add(name, null);
            _preserializedData ??= new Dictionary<string, PrecannedResource>(FastResourceComparer.Default);

            _preserializedData.Add(name, new PrecannedResource(typeName, data));
        }

        /// <summary>
        /// Adds a resource of specified type represented by a string value.
        /// If the type is a primitive type, the value will be converted using TypeConverter by the writer
        /// to that primitive type and stored in the resources in binary format.
        /// If the type is not a primitive type, the string value will be stored in the resources as a
        /// string and converted with a TypeConverter for the type when reading the resource.
        /// This is done to avoid activating arbitrary types during resource writing.
        /// </summary>
        /// <param name="name">Resource name</param>
        /// <param name="value">Value of the resource in string form understood by the type's TypeConverter</param>
        /// <param name="typeName">Assembly qualified type name of the resource</param>
        public void AddResource(string name, string value, string typeName)
        {
            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            if (typeName is null)
            {
                throw new ArgumentNullException(nameof(typeName));
            }

            // determine if the type is a primitive type
            if (s_primitiveTypes.TryGetValue(typeName, out Type? primitiveType))
            {
                // directly add strings
                if (primitiveType == typeof(string))
                {
                    AddResource(name, value);
                }
                else
                {
                    // for primitive types that are not strings, convert the string value to the
                    // primitive type value.
                    // we intentionally avoid calling GetType on the user provided type name
                    // and instead will only ever convert to one of the known types.
                    TypeConverter converter = TypeDescriptor.GetConverter(primitiveType);

                    if (converter == null)
                    {
                        throw new TypeLoadException(SR.Format(MDCFR.Properties.Resources.TypeLoadException_CannotLoadConverter, primitiveType));
                    }

                    object primitiveValue = converter.ConvertFromInvariantString(value)!;

                    Debug.Assert(primitiveValue.GetType() == primitiveType);

                    AddResource(name, primitiveValue);
                }
            }
            else
            {
                AddResourceData(name, typeName, new ResourceDataRecord(SerializationFormat.TypeConverterString, value));
                _requiresDeserializingResourceReader = true;
            }
        }

        /// <summary>
        /// Adds a resource of specified type represented by a byte[] value which will be
        /// passed to the type's TypeConverter when reading the resource.
        /// </summary>
        /// <param name="name">Resource name</param>
        /// <param name="value">Value of the resource in byte[] form understood by the type's TypeConverter</param>
        /// <param name="typeName">Assembly qualified type name of the resource</param>
        public void AddTypeConverterResource(string name, byte[] value, string typeName)
        {
            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            if (typeName is null)
            {
                throw new ArgumentNullException(nameof(typeName));
            }

            AddResourceData(name, typeName, new ResourceDataRecord(SerializationFormat.TypeConverterByteArray, value));

            _requiresDeserializingResourceReader = true;
        }

        /// <summary>
        /// Adds a resource of specified type represented by a byte[] value which will be
        /// passed to BinaryFormatter when reading the resource.
        /// </summary>
        /// <param name="name">Resource name</param>
        /// <param name="value">Value of the resource in byte[] form understood by BinaryFormatter</param>
        /// <param name="typeName">Assembly qualified type name of the resource</param>
        public void AddBinaryFormattedResource(string name, byte[] value, string? typeName = null)
        {
            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            // Some resx-files are missing type information for binary-formatted resources.
            // These would have previously been handled by deserializing once, capturing the type
            // and reserializing when writing the resources.  We don't want to do that so instead
            // we just omit the type.
            typeName ??= UnknownObjectTypeName;

            AddResourceData(name, typeName, new ResourceDataRecord(SerializationFormat.BinaryFormatter, value));

            // Even though ResourceReader can handle BinaryFormatted resources, the resource may contain
            // type names that were mangled by the ResXWriter's SerializationBinder, which we need to fix

            _requiresDeserializingResourceReader = true;
        }

        /// <summary>
        /// Adds a resource of specified type represented by a Stream value which will be
        /// passed to the type's constructor when reading the resource.
        /// </summary>
        /// <param name="name">Resource name</param>
        /// <param name="value">Value of the resource in Stream form understood by the types constructor</param>
        /// <param name="typeName">Assembly qualified type name of the resource</param>
        /// <param name="closeAfterWrite">Indicates that the stream should be closed after resources have been written</param>
        public void AddActivatorResource(string name, Stream value, string typeName, bool closeAfterWrite = false)
        {
            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            if (typeName is null)
            {
                throw new ArgumentNullException(nameof(typeName));
            }

            if (!value.CanSeek)
                throw new ArgumentException(MDCFR.Properties.Resources.NotSupported_UnseekableStream);

            AddResourceData(name, typeName, new ResourceDataRecord(SerializationFormat.ActivatorStream, value, closeAfterWrite));

            _requiresDeserializingResourceReader = true;
        }

        private sealed class ResourceDataRecord
        {
            internal readonly SerializationFormat Format;
            internal readonly object Data;
            internal readonly bool CloseAfterWrite;

            internal ResourceDataRecord(SerializationFormat format, object data, bool closeAfterWrite = false)
            {
                Format = format;
                Data = data;
                CloseAfterWrite = closeAfterWrite;
            }
        }

        private void WriteData(BinaryWriter writer, object dataContext)
        {
            ResourceDataRecord? record = dataContext as ResourceDataRecord;

            Debug.Assert(record != null);

            // Only write the format if we resources are in DeserializingResourceReader format
            if (_requiresDeserializingResourceReader)
            {
                writer.Write7BitEncodedInt((int)record.Format);
            }

            try
            {
                switch (record.Format)
                {
                    case SerializationFormat.BinaryFormatter:
                        {
                            byte[] data = (byte[])record.Data;

                            // only write length if using DeserializingResourceReader, ResourceReader
                            // doesn't constrain binaryFormatter
                            if (_requiresDeserializingResourceReader)
                            {
                                writer.Write7BitEncodedInt(data.Length);
                            }

                            writer.Write(data);
                            break;
                        }
                    case SerializationFormat.ActivatorStream:
                        {
                            Stream stream = (Stream)record.Data;

                            if (stream.Length > int.MaxValue)
                                throw new ArgumentException(MDCFR.Properties.Resources.ArgumentOutOfRange_StreamLength);

                            stream.Position = 0;

                            writer.Write7BitEncodedInt((int)stream.Length);

                            stream.CopyTo(writer.BaseStream);

                            break;
                        }
                    case SerializationFormat.TypeConverterByteArray:
                        {
                            byte[] data = (byte[])record.Data;
                            writer.Write7BitEncodedInt(data.Length);
                            writer.Write(data);
                            break;
                        }
                    case SerializationFormat.TypeConverterString:
                        {
                            string data = (string)record.Data;
                            writer.Write(data);
                            break;
                        }
                    default:
                        // unreachable: indicates inconsistency in this class
                        throw new ArgumentException(nameof(ResourceDataRecord.Format));
                }
            }
            finally
            {
                if (record.Data is IDisposable disposable && record.CloseAfterWrite)
                {
                    disposable.Dispose();
                }
            }
        }

        // Finds the ResourceTypeCode for a type, or adds this type to the
        // types list.
        private static ResourceTypeCode FindTypeCode(object? value, List<string> types)
        {
            if (value == null)
                return ResourceTypeCode.Null;

            Type type = value.GetType();
            if (type == typeof(string))
                return ResourceTypeCode.String;
            else if (type == typeof(int))
                return ResourceTypeCode.Int32;
            else if (type == typeof(bool))
                return ResourceTypeCode.Boolean;
            else if (type == typeof(char))
                return ResourceTypeCode.Char;
            else if (type == typeof(byte))
                return ResourceTypeCode.Byte;
            else if (type == typeof(sbyte))
                return ResourceTypeCode.SByte;
            else if (type == typeof(short))
                return ResourceTypeCode.Int16;
            else if (type == typeof(long))
                return ResourceTypeCode.Int64;
            else if (type == typeof(ushort))
                return ResourceTypeCode.UInt16;
            else if (type == typeof(uint))
                return ResourceTypeCode.UInt32;
            else if (type == typeof(ulong))
                return ResourceTypeCode.UInt64;
            else if (type == typeof(float))
                return ResourceTypeCode.Single;
            else if (type == typeof(double))
                return ResourceTypeCode.Double;
            else if (type == typeof(decimal))
                return ResourceTypeCode.Decimal;
            else if (type == typeof(DateTime))
                return ResourceTypeCode.DateTime;
            else if (type == typeof(TimeSpan))
                return ResourceTypeCode.TimeSpan;
            else if (type == typeof(byte[]))
                return ResourceTypeCode.ByteArray;
            else if (type == typeof(StreamWrapper))
                return ResourceTypeCode.Stream;


            // This is a user type, or a precanned resource.  Find type
            // table index.  If not there, add new element.
            string typeName;
            if (type == typeof(PrecannedResource))
            {
                typeName = ((PrecannedResource)value).TypeName;
                if (typeName.StartsWith("ResourceTypeCode.", StringComparison.Ordinal))
                {
                    typeName = typeName.Substring(17);  // Remove through '.'
                    ResourceTypeCode typeCode = (ResourceTypeCode)Enum.Parse(typeof(ResourceTypeCode), typeName);
                    return typeCode;
                }
            }
            else
            {
                // not a preserialized resource
                throw new PlatformNotSupportedException(MDCFR.Properties.Resources.NotSupported_BinarySerializedResources);
            }

            int typeIndex = types.IndexOf(typeName);
            if (typeIndex == -1)
            {
                typeIndex = types.Count;
                types.Add(typeName);
            }

            return (ResourceTypeCode)(typeIndex + ResourceTypeCode.StartOfUserTypes);
        }

    }

    // Internal Enum that's shared between reader and writer to indicate the
    // deserialization method for a resource.
    internal enum SerializationFormat
    {
        BinaryFormatter = 1,
        TypeConverterByteArray = 2,
        TypeConverterString = 3,
        ActivatorStream = 4
    }

    /// <summary>
    /// Compares type names as strings, ignoring version.
    /// When type names are missing, mscorlib is assumed.
    /// This comparer is not meant to capture all scenarios (eg: TypeForwards)
    /// but is meant to serve as a best effort, avoiding false positives, in the
    /// absence of real type metadata.
    /// </summary>
    internal sealed class TypeNameComparer : IEqualityComparer<string>
    {
        public static TypeNameComparer Instance { get; } = new TypeNameComparer();

        // these match the set of whitespace characters allowed by the runtime's type parser
        private static readonly char[] s_whiteSpaceChars =
        {
            ' ', '\n', '\r', '\t'
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ReadOnlySpan<char> ReadTypeName(ReadOnlySpan<char> assemblyQualifiedTypeName)
        {
            // the runtime doesn't tolerate anything between type name and comma
            int comma = assemblyQualifiedTypeName.IndexOf(',');

            return comma == -1 ? assemblyQualifiedTypeName : assemblyQualifiedTypeName.Slice(0, comma);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ReadOnlySpan<char> ReadAssemblySimpleName(ReadOnlySpan<char> assemblyName)
        {
            int comma = assemblyName.IndexOf(',');

            return comma == -1 ? assemblyName : assemblyName.Slice(0, comma).TrimEnd(s_whiteSpaceChars);
        }

        private static bool IsMscorlib(ReadOnlySpan<char> assemblyName)
        {
            // to match IsMscorlib() in VM, which will ignore public key token and culture for corelib
            return assemblyName.Equals("mscorlib".AsSpan(), StringComparison.OrdinalIgnoreCase);
        }

        // Compare two type names ignoring version
        // If a type name is missing assembly, we assume it came from mscorlib
        // since this is what Type.GetType will do.
#pragma warning disable CS8767 // This API member has [AllowNull] implemented interface, but we don't want to accept nulls here.
        public bool Equals(string assemblyQualifiedTypeName1, string assemblyQualifiedTypeName2)
#pragma warning restore CS8767
        {
            if (assemblyQualifiedTypeName1 is null)
            {
                throw new ArgumentNullException(nameof(assemblyQualifiedTypeName1));
            }
            if (assemblyQualifiedTypeName2 is null)
            {
                throw new ArgumentNullException(nameof(assemblyQualifiedTypeName2));
            }

            if (ReferenceEquals(assemblyQualifiedTypeName1, assemblyQualifiedTypeName2))
                return true;

            ReadOnlySpan<char> typeSpan1 = assemblyQualifiedTypeName1.AsSpan().TrimStart(s_whiteSpaceChars);
            ReadOnlySpan<char> typeSpan2 = assemblyQualifiedTypeName2.AsSpan().TrimStart(s_whiteSpaceChars);

            // First, compare type names
            ReadOnlySpan<char> type1 = ReadTypeName(typeSpan1);
            ReadOnlySpan<char> type2 = ReadTypeName(typeSpan2);
            if (!type1.Equals(type2, StringComparison.Ordinal))
                return false;

            // skip separator and whitespace
            typeSpan1 = typeSpan1.Length > type1.Length ? typeSpan1.Slice(type1.Length + 1).TrimStart(s_whiteSpaceChars) : ReadOnlySpan<char>.Empty;
            typeSpan2 = typeSpan2.Length > type2.Length ? typeSpan2.Slice(type2.Length + 1).TrimStart(s_whiteSpaceChars) : ReadOnlySpan<char>.Empty;

            // Now, compare assembly simple names ignoring case
            ReadOnlySpan<char> simpleName1 = ReadAssemblySimpleName(typeSpan1);
            ReadOnlySpan<char> simpleName2 = ReadAssemblySimpleName(typeSpan2);

            // Don't allow assembly name without simple name portion
            if (simpleName1.IsEmpty && !typeSpan1.IsEmpty ||
                simpleName2.IsEmpty && !typeSpan2.IsEmpty)
                return false;

            // if both are missing assembly name, or either is missing
            // assembly name and the other is mscorlib
            if (simpleName1.IsEmpty)
                return (simpleName2.IsEmpty || IsMscorlib(simpleName2));
            if (simpleName2.IsEmpty)
                return IsMscorlib(simpleName1);

            if (!simpleName1.Equals(simpleName2, StringComparison.OrdinalIgnoreCase))
                return false;

            // both are mscorlib, ignore culture and key
            if (IsMscorlib(simpleName1))
                return true;

            // both have a matching assembly name parse it to get remaining portions
            // to compare culture and public key token
            // the following results in allocations.
            AssemblyName an1 = new AssemblyName(typeSpan1.ToString());
            AssemblyName an2 = new AssemblyName(typeSpan2.ToString());

            if (an1.CultureInfo?.LCID != an2.CultureInfo?.LCID)
                return false;

            byte[]? pkt1 = an1.GetPublicKeyToken();
            byte[]? pkt2 = an2.GetPublicKeyToken();
            return pkt1.AsSpan().SequenceEqual(pkt2);
        }

        public int GetHashCode(string assemblyQualifiedTypeName)
        {
            // non-allocating GetHashCode that hashes the type name portion of the string
            ReadOnlySpan<char> typeSpan = assemblyQualifiedTypeName.AsSpan().TrimStart(s_whiteSpaceChars);
            ReadOnlySpan<char> typeName = ReadTypeName(typeSpan);

            int hashCode = 0;
            for (int i = 0; i < typeName.Length; i++)
            {
                hashCode = HashHelpers.Combine(hashCode, typeName[i].GetHashCode());
            }

            return hashCode;
        }
    }

    // A RuntimeResourceSet stores all the resources defined in one
    // particular CultureInfo, with some loading optimizations.
    //
    // It is expected that nearly all the runtime's users will be satisfied with the
    // default resource file format, and it will be more efficient than most simple
    // implementations.  Users who would consider creating their own ResourceSets and/or
    // ResourceReaders and ResourceWriters are people who have to interop with a
    // legacy resource file format, are creating their own resource file format
    // (using XML, for instance), or require doing resource lookups at runtime over
    // the network.  This group will hopefully be small, but all the infrastructure
    // should be in place to let these users write & plug in their own tools.
    //
    // The Default Resource File Format
    //
    // The fundamental problems addressed by the resource file format are:
    //
    // * Versioning - A ResourceReader could in theory support many different
    // file format revisions.
    // * Storing intrinsic datatypes (ie, ints, Strings, DateTimes, etc) in a compact
    // format
    // * Support for user-defined classes - Accomplished using Serialization
    // * Resource lookups should not require loading an entire resource file - If you
    // look up a resource, we only load the value for that resource, minimizing working set.
    //
    //
    // There are four sections to the default file format.  The first
    // is the Resource Manager header, which consists of a magic number
    // that identifies this as a Resource file, and a ResourceSet class name.
    // The class name is written here to allow users to provide their own
    // implementation of a ResourceSet (and a matching ResourceReader) to
    // control policy.  If objects greater than a certain size or matching a
    // certain naming scheme shouldn't be stored in memory, users can tweak that
    // with their own subclass of ResourceSet.
    //
    // The second section in the system default file format is the
    // RuntimeResourceSet specific header.  This contains a version number for
    // the .resources file, the number of resources in this file, the number of
    // different types contained in the file, followed by a list of fully
    // qualified type names.  After this, we include an array of hash values for
    // each resource name, then an array of virtual offsets into the name section
    // of the file.  The hashes allow us to do a binary search on an array of
    // integers to find a resource name very quickly without doing many string
    // compares (except for once we find the real type, of course).  If a hash
    // matches, the index into the array of hash values is used as the index
    // into the name position array to find the name of the resource.  The type
    // table allows us to read multiple different classes from the same file,
    // including user-defined types, in a more efficient way than using
    // Serialization, at least when your .resources file contains a reasonable
    // proportion of base data types such as Strings or ints.  We use
    // Serialization for all the non-intrinsic types.
    //
    // The third section of the file is the name section.  It contains a
    // series of resource names, written out as byte-length prefixed little
    // endian Unicode strings (UTF-16).  After each name is a four byte virtual
    // offset into the data section of the file, pointing to the relevant
    // string or serialized blob for this resource name.
    //
    // The fourth section in the file is the data section, which consists
    // of a type and a blob of bytes for each item in the file.  The type is
    // an integer index into the type table.  The data is specific to that type,
    // but may be a number written in binary format, a String, or a serialized
    // Object.
    //
    // The system default file format (V1) is as follows:
    //
    //     What                                               Type of Data
    // ====================================================   ===========
    //
    //                        Resource Manager header
    // Magic Number (0xBEEFCACE)                              Int32
    // Resource Manager header version                        Int32
    // Num bytes to skip from here to get past this header    Int32
    // Class name of IResourceReader to parse this file       String
    // Class name of ResourceSet to parse this file           String
    //
    //                       RuntimeResourceReader header
    // ResourceReader version number                          Int32
    // [Only in debug V2 builds - "***DEBUG***"]              String
    // Number of resources in the file                        Int32
    // Number of types in the type table                      Int32
    // Name of each type                                      Set of Strings
    // Padding bytes for 8-byte alignment (use PAD)           Bytes (0-7)
    // Hash values for each resource name                     Int32 array, sorted
    // Virtual offset of each resource name                   Int32 array, coupled with hash values
    // Absolute location of Data section                      Int32
    //
    //                     RuntimeResourceReader Name Section
    // Name & virtual offset of each resource                 Set of (UTF-16 String, Int32) pairs
    //
    //                     RuntimeResourceReader Data Section
    // Type and Value of each resource                Set of (Int32, blob of bytes) pairs
    //
    // This implementation, when used with the default ResourceReader class,
    // loads only the strings that you look up for.  It can do string comparisons
    // without having to create a new String instance due to some memory mapped
    // file optimizations in the ResourceReader and FastResourceComparer
    // classes.  This keeps the memory we touch to a minimum when loading
    // resources.
    //
    // If you use a different IResourceReader class to read a file, or if you
    // do case-insensitive lookups (and the case-sensitive lookup fails) then
    // we will load all the names of each resource and each resource value.
    // This could probably use some optimization.
    //
    // In addition, this supports object serialization in a similar fashion.
    // We build an array of class types contained in this file, and write it
    // to RuntimeResourceReader header section of the file.  Every resource
    // will contain its type (as an index into the array of classes) with the data
    // for that resource.  We will use the Runtime's serialization support for this.
    //
    // All strings in the file format are written with BinaryReader and
    // BinaryWriter, which writes out the length of the String in bytes as an
    // Int32 then the contents as Unicode chars encoded in UTF-8.  In the name
    // table though, each resource name is written in UTF-16 so we can do a
    // string compare byte by byte against the contents of the file, without
    // allocating objects.  Ideally we'd have a way of comparing UTF-8 bytes
    // directly against a String object, but that may be a lot of work.
    //
    // The offsets of each resource string are relative to the beginning
    // of the Data section of the file.  This way, if a tool decided to add
    // one resource to a file, it would only need to increment the number of
    // resources, add the hash &amp; location of last byte in the name section
    // to the array of resource hashes and resource name positions (carefully
    // keeping these arrays sorted), add the name to the end of the name &amp;
    // offset list, possibly add the type list of types (and increase
    // the number of items in the type table), and add the resource value at
    // the end of the file.  The other offsets wouldn't need to be updated to
    // reflect the longer header section.
    //
    // Resource files are currently limited to 2 gigabytes due to these
    // design parameters.  A future version may raise the limit to 4 gigabytes
    // by using unsigned integers, or may use negative numbers to load items
    // out of an assembly manifest.  Also, we may try sectioning the resource names
    // into smaller chunks, each of size sqrt(n), would be substantially better for
    // resource files containing thousands of resources.
    //

    public sealed class RuntimeResourceSet : ResourceSet, IEnumerable
    {
        // Cache for resources.  Key is the resource name, which can be cached
        // for arbitrarily long times, since the object is usually a string
        // literal that will live for the lifetime of the appdomain.  The
        // value is a ResourceLocator instance, which might cache the object.
        private Dictionary<string, ResourceLocator>? _resCache;


        // For our special load-on-demand reader. The
        // RuntimeResourceSet's implementation knows how to treat this reader specially.
        private DeserializingResourceReader? _defaultReader;

        // This is a lookup table for case-insensitive lookups, and may be null.
        // Consider always using a case-insensitive resource cache, as we don't
        // want to fill this out if we can avoid it.  The problem is resource
        // fallback will somewhat regularly cause us to look up resources that
        // don't exist.
        private Dictionary<string, ResourceLocator>? _caseInsensitiveTable;

        // explicitly do not call IResourceReader constructor since it caches all resources
        // the purpose of RuntimeResourceSet is to lazily load and cache.
        internal RuntimeResourceSet(IResourceReader reader) : base()
        {
            if (reader is null) { throw new ArgumentNullException(nameof(reader)); }

            _defaultReader = reader as DeserializingResourceReader ?? throw new ArgumentException(SR.Format(MDCFR.Properties.Resources.NotSupported_WrongResourceReader_Type, reader.GetType()), nameof(reader));
            _resCache = new Dictionary<string, ResourceLocator>(FastResourceComparer.Default);

            // in the CoreLib version RuntimeResourceSet creates ResourceReader and passes this in,
            // in the custom case ManifestBasedResourceReader creates the ResourceReader and passes it in
            // so we must initialize the cache here.
            _defaultReader._resCache = _resCache;
        }

        protected override void Dispose(bool disposing)
        {
            if (_defaultReader is null) { return; }

            if (disposing) { _defaultReader?.Close(); }

            _defaultReader = null;
            _resCache = null;
            _caseInsensitiveTable = null;
            base.Dispose(disposing);
        }

        public override IDictionaryEnumerator GetEnumerator()
        {
            return GetEnumeratorHelper();
        }

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumeratorHelper(); }

        private IDictionaryEnumerator GetEnumeratorHelper()
        {
            DeserializingResourceReader? reader = _defaultReader;
            if (reader is null)
                throw new ObjectDisposedException(null, MDCFR.Properties.Resources.ObjectDisposed_ResourceSet);

            return reader.GetEnumerator();
        }

        public override string? GetString(string key)
        {
            object? o = GetObject(key, false, true);
            return (string?)o;
        }

        public override string? GetString(string key, bool ignoreCase)
        {
            object? o = GetObject(key, ignoreCase, true);
            return (string?)o;
        }

        public override object? GetObject(string key)
        {
            return GetObject(key, false, false);
        }

        public override object? GetObject(string key, bool ignoreCase)
        {
            return GetObject(key, ignoreCase, false);
        }

        private object? GetObject(string key, bool ignoreCase, bool isString)
        {
            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            DeserializingResourceReader? reader = _defaultReader;
            Dictionary<string, ResourceLocator>? cache = _resCache;
            if (reader is null || cache is null)
                throw new ObjectDisposedException(null, MDCFR.Properties.Resources.ObjectDisposed_ResourceSet);

            object? value;
            ResourceLocator resEntry;

            // Lock the cache first, then the reader (reader locks implicitly through its methods).
            // Lock order MUST match ResourceReader.ResourceEnumerator.Entry to avoid deadlock.
            Debug.Assert(!Monitor.IsEntered(reader));
            lock (cache)
            {
                // Find the offset within the data section
                int dataPos;
                if (cache.TryGetValue(key, out resEntry))
                {
                    value = resEntry.Value;
                    if (value != null)
                        return value;

                    // When data type cannot be cached
                    dataPos = resEntry.DataPosition;
                    return isString ? reader.LoadString(dataPos) : reader.LoadObject(dataPos);
                }

                dataPos = reader.FindPosForResource(key);
                if (dataPos >= 0)
                {
                    value = ReadValue(reader, dataPos, isString, out resEntry);
                    cache[key] = resEntry;
                    return value;
                }
            }

            if (!ignoreCase)
            {
                return null;
            }

            // We haven't found the particular resource we're looking for
            // and may have to search for it in a case-insensitive way.
            bool initialize = false;
            Dictionary<string, ResourceLocator>? caseInsensitiveTable = _caseInsensitiveTable;
            if (caseInsensitiveTable == null)
            {
                caseInsensitiveTable = new Dictionary<string, ResourceLocator>(StringComparer.OrdinalIgnoreCase);
                initialize = true;
            }

            lock (caseInsensitiveTable)
            {
                if (initialize)
                {
                    DeserializingResourceReader.ResourceEnumerator en = reader.GetEnumeratorInternal();
                    while (en.MoveNext())
                    {
                        // The resource key must be read before the data position.
                        string currentKey = (string)en.Key;
                        ResourceLocator resLoc = new ResourceLocator(en.DataPosition, null);
                        caseInsensitiveTable.Add(currentKey, resLoc);
                    }

                    _caseInsensitiveTable = caseInsensitiveTable;
                }

                if (!caseInsensitiveTable.TryGetValue(key, out resEntry))
                    return null;

                if (resEntry.Value != null)
                    return resEntry.Value;

                value = ReadValue(reader, resEntry.DataPosition, isString, out resEntry);

                if (resEntry.Value != null)
                    caseInsensitiveTable[key] = resEntry;
            }

            return value;
        }

        private static object? ReadValue(DeserializingResourceReader reader, int dataPos, bool isString, out ResourceLocator locator)
        {
            object? value;
            ResourceTypeCode typeCode;

            if (isString)
            {
                value = reader.LoadString(dataPos);
                typeCode = ResourceTypeCode.String;
            }
            else
            {
                value = reader.LoadObject(dataPos, out typeCode);
            }

            locator = new ResourceLocator(dataPos, ResourceLocator.CanCache(typeCode) ? value : null);
            return value;
        }
    }

    // Provides the default implementation of IResourceReader, reading
    // .resources file from the system default binary format.  This class
    // can be treated as an enumerator once.
    //
    // See the RuntimeResourceSet overview for details on the system
    // default file format.
    //

    public class ResourceSet : IDisposable, IEnumerable
    {
        protected IResourceReader Reader = null!;

        private Dictionary<object, object?>? _table;
        private Dictionary<string, object?>? _caseInsensitiveTable;  // For case-insensitive lookups.

        protected ResourceSet()
        {
            // To not inconvenience people subclassing us, we should allocate a new
            // hashtable here just so that Table is set to something.
            _table = new Dictionary<object, object?>();
        }

        // For RuntimeResourceSet, ignore the Table parameter - it's a wasted
        // allocation.
        internal ResourceSet(bool _)
        {
        }

        // Creates a ResourceSet using the system default ResourceReader
        // implementation.  Use this constructor to open & read from a file
        // on disk.
        //
        public ResourceSet(string fileName)
            : this()
        {
            Reader = new ResourceReader(fileName);
            ReadResources();
        }

        // Creates a ResourceSet using the system default ResourceReader
        // implementation.  Use this constructor to read from an open stream
        // of data.
        //
        public ResourceSet(Stream stream)
            : this()
        {
            Reader = new ResourceReader(stream);
            ReadResources();
        }

        public ResourceSet(IResourceReader reader)
            : this()
        {
            if (reader == null) { throw new ArgumentNullException(nameof(reader)); }

            Reader = reader;
            ReadResources();
        }

        // Closes and releases any resources used by this ResourceSet, if any.
        // All calls to methods on the ResourceSet after a call to close may
        // fail.  Close is guaranteed to be safely callable multiple times on a
        // particular ResourceSet, and all subclasses must support these semantics.
        public virtual void Close()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Close the Reader in a thread-safe way.
                IResourceReader? copyOfReader = Reader;
                Reader = null!;
                copyOfReader?.Close();
            }
            Reader = null!;
            _caseInsensitiveTable = null;
            _table = null;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        // Returns the preferred IResourceReader class for this kind of ResourceSet.
        // Subclasses of ResourceSet using their own Readers &; should override
        // GetDefaultReader and GetDefaultWriter.
        public virtual Type GetDefaultReader()
        {
            return typeof(ResourceReader);
        }

        // Returns the preferred IResourceWriter class for this kind of ResourceSet.
        // Subclasses of ResourceSet using their own Readers &; should override
        // GetDefaultReader and GetDefaultWriter.
        public virtual Type GetDefaultWriter()
        {
            return Type.GetType("System.Resources.ResourceWriter, System.Resources.Writer", throwOnError: true)!;
        }

        public virtual IDictionaryEnumerator GetEnumerator()
        {
            return GetEnumeratorHelper();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumeratorHelper();
        }

        private IDictionaryEnumerator GetEnumeratorHelper()
        {
            IDictionary? copyOfTableAsIDictionary = _table;  // Avoid a race with Dispose
            if (copyOfTableAsIDictionary == null)
                throw new ObjectDisposedException(null, MDCFR.Properties.Resources.ObjectDisposed_ResourceSet);

            // Use IDictionary.GetEnumerator() for backward compatibility. Callers expect the enumerator to return DictionaryEntry instances.
            return copyOfTableAsIDictionary.GetEnumerator();
        }

        // Look up a string value for a resource given its name.
        //
        public virtual string? GetString(string name)
        {
            object? obj = GetObjectInternal(name);
            if (obj is string s)
                return s;

            if (obj is null)
                return null;

            throw new InvalidOperationException(SR.Format(MDCFR.Properties.Resources.InvalidOperation_ResourceNotString_Name, name));
        }

        public virtual string? GetString(string name, bool ignoreCase)
        {
            // Case-sensitive lookup
            object? obj = GetObjectInternal(name);
            if (obj is string s)
                return s;

            if (obj is not null)
                throw new InvalidOperationException(SR.Format(MDCFR.Properties.Resources.InvalidOperation_ResourceNotString_Name, name));

            if (!ignoreCase)
                return null;

            // Try doing a case-insensitive lookup
            obj = GetCaseInsensitiveObjectInternal(name);
            if (obj is string si)
                return si;

            if (obj is null)
                return null;

            throw new InvalidOperationException(SR.Format(MDCFR.Properties.Resources.InvalidOperation_ResourceNotString_Name, name));
        }

        // Look up an object value for a resource given its name.
        //
        public virtual object? GetObject(string name)
        {
            return GetObjectInternal(name);
        }

        public virtual object? GetObject(string name, bool ignoreCase)
        {
            object? obj = GetObjectInternal(name);

            if (obj != null || !ignoreCase)
                return obj;

            return GetCaseInsensitiveObjectInternal(name);
        }

        protected virtual void ReadResources()
        {
            Debug.Assert(_table != null);
            Debug.Assert(Reader != null);
            IDictionaryEnumerator en = Reader.GetEnumerator();
            while (en.MoveNext())
            {
                _table.Add(en.Key, en.Value);
            }
            // While technically possible to close the Reader here, don't close it
            // to help with some WinRes lifetime issues.
        }

        private object? GetObjectInternal(string name)
        {
            if (name == null) { throw new ArgumentNullException(name); }

            Dictionary<object, object?>? copyOfTable = _table;  // Avoid a race with Dispose

            if (copyOfTable == null)
                throw new ObjectDisposedException(null, MDCFR.Properties.Resources.ObjectDisposed_ResourceSet);

            copyOfTable.TryGetValue(name, out object? value);
            return value;
        }

        private object? GetCaseInsensitiveObjectInternal(string name)
        {
            Dictionary<object, object?>? copyOfTable = _table;  // Avoid a race with Dispose

            if (copyOfTable == null)
                throw new ObjectDisposedException(null, MDCFR.Properties.Resources.ObjectDisposed_ResourceSet);

            Dictionary<string, object?>? caseTable = _caseInsensitiveTable;  // Avoid a race condition with Close
            if (caseTable == null)
            {
                caseTable = new Dictionary<string, object?>(copyOfTable.Count, StringComparer.OrdinalIgnoreCase);
                foreach (var item in copyOfTable)
                {
                    if (item.Key is not string s)
                        continue;

                    caseTable.Add(s, item.Value);
                }
                _caseInsensitiveTable = caseTable;
            }

            caseTable.TryGetValue(name, out object? value);
            return value;
        }
    }


    internal readonly struct ResourceLocator
    {
        internal ResourceLocator(int dataPos, object? value)
        {
            DataPosition = dataPos;
            Value = value;
        }

        internal int DataPosition { get; }
        internal object? Value { get; }

        internal static bool CanCache(ResourceTypeCode value)
        {
            Debug.Assert(value >= 0, "negative ResourceTypeCode.  What?");
            return value <= ResourceTypeCode.LastPrimitive;
        }
    }

    public sealed partial class DeserializingResourceReader : IResourceReader
    {
        // A reasonable default buffer size for reading from files, especially
        // when we will likely be seeking frequently.  Could be smaller, but does
        // it make sense to use anything less than one page?
        private const int DefaultFileStreamBufferSize = 4096;

        // Backing store we're reading from. Usages outside of constructor
        // initialization must be protected by lock (this).
        private BinaryReader _store;
        // Used by RuntimeResourceSet and this class's enumerator.
        // Accesses must be protected by lock(_resCache).
        internal Dictionary<string, ResourceLocator>? _resCache;
        private long _nameSectionOffset;  // Offset to name section of file.
        private long _dataSectionOffset;  // Offset to Data section of file.

        // Note this class is tightly coupled with UnmanagedMemoryStream.
        // At runtime when getting an embedded resource from an assembly,
        // we're given an UnmanagedMemoryStream referring to the mmap'ed portion
        // of the assembly.  The pointers here are pointers into that block of
        // memory controlled by the OS's loader.
        private int[]? _nameHashes;    // hash values for all names.
        private unsafe int* _nameHashesPtr;  // In case we're using UnmanagedMemoryStream
        private int[]? _namePositions; // relative locations of names
        private unsafe int* _namePositionsPtr;  // If we're using UnmanagedMemoryStream
        private Type?[] _typeTable;    // Lazy array of Types for resource values.
        private int[] _typeNamePositions;  // To delay initialize type table
        private int _numResources;    // Num of resources files, in case arrays aren't allocated.

        // We'll include a separate code path that uses UnmanagedMemoryStream to
        // avoid allocating String objects and the like.
        private UnmanagedMemoryStream? _ums;

        // Version number of .resources file, for compatibility
        private int _version;

        public DeserializingResourceReader(string fileName)
        {
            _resCache = new Dictionary<string, ResourceLocator>(FastResourceComparer.Default);
            _store = new BinaryReader(new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, DefaultFileStreamBufferSize, FileOptions.RandomAccess), Encoding.UTF8);

            try
            {
                ReadResources();
            }
            catch
            {
                _store.Close(); // If we threw an exception, close the file.
                throw;
            }
        }

        public DeserializingResourceReader(Stream stream)
        {
            if (stream is null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (!stream.CanRead)
            {
                throw new ArgumentException(MDCFR.Properties.Resources.Argument_StreamNotReadable);
            }

            _resCache = new Dictionary<string, ResourceLocator>(FastResourceComparer.Default);
            _store = new BinaryReader(stream, Encoding.UTF8);
            // We have a faster code path for reading resource files from an assembly.
            _ums = stream as UnmanagedMemoryStream;

            ReadResources();
        }

        internal static bool AllowCustomResourceTypes { get; } = AppContext.TryGetSwitch("System.Resources.ResourceManager.AllowCustomResourceTypes", out bool allowReflection) ? allowReflection : true;

        public void Close() { Dispose(true); }

        public void Dispose() { Close(); }

        private unsafe void Dispose(bool disposing)
        {
            if (_store != null)
            {
                _resCache = null;
                if (disposing)
                {
                    // Close the stream in a thread-safe way.  This fix means
                    // that we may call Close n times, but that's safe.
                    BinaryReader copyOfStore = _store;
                    _store = null!;
                    copyOfStore?.Close();
                }
                _store = null!;
                _namePositions = null;
                _nameHashes = null;
                _ums = null;
                _namePositionsPtr = null;
                _nameHashesPtr = null;
            }
        }

        private static unsafe int ReadUnalignedI4(int* p)
        {
            return BinaryPrimitives.ReadInt32LittleEndian(new ReadOnlySpan<byte>(p, sizeof(int)));
        }

        private void SkipString()
        {
            // Note: this method assumes that it is called either during object
            // construction or within another method that locks on this.

            int stringLength = _store.Read7BitEncodedInt();
            if (stringLength < 0)
            {
                throw new BadImageFormatException(MDCFR.Properties.Resources.BadImageFormat_NegativeStringLength);
            }
            _store.BaseStream.Seek(stringLength, SeekOrigin.Current);
        }

        private unsafe System.Int32 GetNameHash(int index)
        {
            Debug.Assert(index >= 0 && index < _numResources, $"Bad index into hash array.  index: {index}");

            if (_ums == null)
            {
                Debug.Assert(_nameHashes != null && _nameHashesPtr == null, "Internal state mangled.");
                return _nameHashes[index];
            }
            else
            {
                Debug.Assert(_nameHashes == null && _nameHashesPtr != null, "Internal state mangled.");
                return ReadUnalignedI4(&_nameHashesPtr[index]);
            }
        }

        private unsafe int GetNamePosition(int index)
        {
            Debug.Assert(index >= 0 && index < _numResources, $"Bad index into name position array.  index: {index}");
            int r;
            if (_ums == null)
            {
                Debug.Assert(_namePositions != null && _namePositionsPtr == null, "Internal state mangled.");
                r = _namePositions[index];
            }
            else
            {
                Debug.Assert(_namePositions == null && _namePositionsPtr != null, "Internal state mangled.");
                r = ReadUnalignedI4(&_namePositionsPtr[index]);
            }

            if (r < 0 || r > _dataSectionOffset - _nameSectionOffset)
            {
                throw new FormatException(SR.Format(MDCFR.Properties.Resources.BadImageFormat_ResourcesNameInvalidOffset, r));
            }
            return r;
        }

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        public IDictionaryEnumerator GetEnumerator()
        {
            if (_resCache == null) { throw new InvalidOperationException(MDCFR.Properties.Resources.ResourceReaderIsClosed); }
            return new ResourceEnumerator(this);
        }

        // Called from RuntimeResourceSet
        internal ResourceEnumerator GetEnumeratorInternal() { return new ResourceEnumerator(this); }

        // From a name, finds the associated virtual offset for the data.
        // To read the data, seek to _dataSectionOffset + dataPos, then
        // read the resource type & data.
        // This does a binary search through the names.
        // Called from RuntimeResourceSet
        internal System.Int32 FindPosForResource(string name)
        {
            Debug.Assert(_store != null, "ResourceReader is closed!");
            int hash = FastResourceComparer.HashFunction(name);

            // Binary search over the hashes.  Use the _namePositions array to
            // determine where they exist in the underlying stream.
            int lo = 0;
            int hi = _numResources - 1;
            int index = -1;
            bool success = false;
            while (lo <= hi)
            {
                index = (lo + hi) >> 1;
                // Do NOT use subtraction here, since it will wrap for large
                // negative numbers.
                int currentHash = GetNameHash(index);
                int c;
                if (currentHash == hash)
                    c = 0;
                else if (currentHash < hash)
                    c = -1;
                else
                    c = 1;

                if (c == 0)
                {
                    success = true;
                    break;
                }
                if (c < 0)
                    lo = index + 1;
                else
                    hi = index - 1;
            }
            if (!success)
            {
                return -1;
            }

            // index is the location in our hash array that corresponds with a
            // value in the namePositions array.
            // There could be collisions in our hash function.  Check on both sides
            // of index to find the range of hash values that are equal to the
            // target hash value.
            if (lo != index)
            {
                lo = index;
                while (lo > 0 && GetNameHash(lo - 1) == hash)
                    lo--;
            }
            if (hi != index)
            {
                hi = index;
                while (hi < _numResources - 1 && GetNameHash(hi + 1) == hash)
                    hi++;
            }

            lock (this)
            {
                for (int i = lo; i <= hi; i++)
                {
                    _store.BaseStream.Seek(_nameSectionOffset + GetNamePosition(i), SeekOrigin.Begin);
                    if (CompareStringEqualsName(name))
                    {
                        int dataPos = _store.ReadInt32();
                        if (dataPos < 0 || dataPos >= _store.BaseStream.Length - _dataSectionOffset)
                        {
                            throw new FormatException(SR.Format(MDCFR.Properties.Resources.BadImageFormat_ResourcesDataInvalidOffset, dataPos));
                        }
                        return dataPos;
                    }
                }
            }
            return -1;
        }

        // This compares the String in the .resources file at the current position
        // with the string you pass in.
        // Whoever calls this method should make sure that they take a lock
        // so no one else can cause us to seek in the stream.
        private unsafe bool CompareStringEqualsName(string name)
        {
            Debug.Assert(_store != null, "ResourceReader is closed!");
            Debug.Assert(Monitor.IsEntered(this)); // uses _store

            int byteLen = _store.Read7BitEncodedInt();
            if (byteLen < 0)
            {
                throw new BadImageFormatException(MDCFR.Properties.Resources.BadImageFormat_NegativeStringLength);
            }
            if (_ums != null)
            {
                byte* bytes = _ums.PositionPointer;
                // Skip over the data in the Stream, positioning ourselves right after it.
                _ums.Seek(byteLen, SeekOrigin.Current);
                if (_ums.Position > _ums.Length)
                {
                    throw new BadImageFormatException(MDCFR.Properties.Resources.BadImageFormat_ResourcesNameTooLong);
                }

                // On 64-bit machines, these char*'s may be misaligned.  Use a
                // byte-by-byte comparison instead.
                return FastResourceComparer.CompareOrdinal(bytes, byteLen, name) == 0;
            }
            else
            {
                // This code needs to be fast
                byte[] bytes = new byte[byteLen];
                int numBytesToRead = byteLen;
                while (numBytesToRead > 0)
                {
                    int n = _store.Read(bytes, byteLen - numBytesToRead, numBytesToRead);
                    if (n == 0)
                        throw new BadImageFormatException(MDCFR.Properties.Resources.BadImageFormat_ResourceNameCorrupted);
                    numBytesToRead -= n;
                }
                return FastResourceComparer.CompareOrdinal(bytes, byteLen / 2, name) == 0;
            }
        }

        // This is used in the enumerator.  The enumerator iterates from 0 to n
        // of our resources and this returns the resource name for a particular
        // index.  The parameter is NOT a virtual offset.
        private unsafe string AllocateStringForNameIndex(int index, out int dataOffset)
        {
            Debug.Assert(_store != null, "ResourceReader is closed!");
            byte[] bytes;
            int byteLen;
            long nameVA = GetNamePosition(index);
            lock (this)
            {
                _store.BaseStream.Seek(nameVA + _nameSectionOffset, SeekOrigin.Begin);
                // Can't use _store.ReadString, since it's using UTF-8!
                byteLen = _store.Read7BitEncodedInt();
                if (byteLen < 0)
                {
                    throw new BadImageFormatException(MDCFR.Properties.Resources.BadImageFormat_NegativeStringLength);
                }

                if (_ums != null)
                {
                    if (_ums.Position > _ums.Length - byteLen)
                        throw new BadImageFormatException(SR.Format(MDCFR.Properties.Resources.BadImageFormat_ResourcesIndexTooLong, index));

                    string? s = null;
                    char* charPtr = (char*)_ums.PositionPointer;

                    if (BitConverter.IsLittleEndian)
                    {
                        s = new string(charPtr, 0, byteLen / 2);
                    }
                    else
                    {
                        char[] arr = new char[byteLen / 2];
                        for (int i = 0; i < arr.Length; i++)
                        {
                            arr[i] = (char)BinaryPrimitives.ReverseEndianness((short)charPtr[i]);
                        }
                        s = new string(arr);
                    }

                    _ums.Position += byteLen;
                    dataOffset = _store.ReadInt32();
                    if (dataOffset < 0 || dataOffset >= _store.BaseStream.Length - _dataSectionOffset)
                    {
                        throw new FormatException(SR.Format(MDCFR.Properties.Resources.BadImageFormat_ResourcesDataInvalidOffset, dataOffset));
                    }
                    return s;
                }

                bytes = new byte[byteLen];
                // We must read byteLen bytes, or we have a corrupted file.
                // Use a blocking read in case the stream doesn't give us back
                // everything immediately.
                int count = byteLen;
                while (count > 0)
                {
                    int n = _store.Read(bytes, byteLen - count, count);
                    if (n == 0)
                        throw new EndOfStreamException(SR.Format(MDCFR.Properties.Resources.BadImageFormat_ResourceNameCorrupted_NameIndex, index));
                    count -= n;
                }
                dataOffset = _store.ReadInt32();
                if (dataOffset < 0 || dataOffset >= _store.BaseStream.Length - _dataSectionOffset)
                {
                    throw new FormatException(SR.Format(MDCFR.Properties.Resources.BadImageFormat_ResourcesDataInvalidOffset, dataOffset));
                }
            }
            return Encoding.Unicode.GetString(bytes, 0, byteLen);
        }

        // This is used in the enumerator.  The enumerator iterates from 0 to n
        // of our resources and this returns the resource value for a particular
        // index.  The parameter is NOT a virtual offset.
        private object? GetValueForNameIndex(int index)
        {
            Debug.Assert(_store != null, "ResourceReader is closed!");
            long nameVA = GetNamePosition(index);
            lock (this)
            {
                _store.BaseStream.Seek(nameVA + _nameSectionOffset, SeekOrigin.Begin);
                SkipString();

                int dataPos = _store.ReadInt32();
                if (dataPos < 0 || dataPos >= _store.BaseStream.Length - _dataSectionOffset)
                {
                    throw new FormatException(SR.Format(MDCFR.Properties.Resources.BadImageFormat_ResourcesDataInvalidOffset, dataPos));
                }

                if (_version == 1)
                    return LoadObjectV1(dataPos);
                else
                    return LoadObjectV2(dataPos, out _);
            }
        }

        // This takes a virtual offset into the data section and reads a String
        // from that location. Called from RuntimeResourceSet
        internal string? LoadString(int pos)
        {
            Debug.Assert(_store != null, "ResourceReader is closed!");

            lock (this)
            {
                _store.BaseStream.Seek(_dataSectionOffset + pos, SeekOrigin.Begin);
                string? s = null;
                int typeIndex = _store.Read7BitEncodedInt();
                if (_version == 1)
                {
                    if (typeIndex == -1)
                        return null;
                    if (FindType(typeIndex) != typeof(string))
                        throw new InvalidOperationException(SR.Format(MDCFR.Properties.Resources.InvalidOperation_ResourceNotString_Type, FindType(typeIndex).FullName));
                    s = _store.ReadString();
                }
                else
                {
                    ResourceTypeCode typeCode = (ResourceTypeCode)typeIndex;
                    if (typeCode != ResourceTypeCode.String && typeCode != ResourceTypeCode.Null)
                    {
                        string? typeString;
                        if (typeCode < ResourceTypeCode.StartOfUserTypes)
                            typeString = typeCode.ToString();
                        else
                            typeString = FindType(typeCode - ResourceTypeCode.StartOfUserTypes).FullName;
                        throw new InvalidOperationException(SR.Format(MDCFR.Properties.Resources.InvalidOperation_ResourceNotString_Type, typeString));
                    }
                    if (typeCode == ResourceTypeCode.String) // ignore Null
                        s = _store.ReadString();
                }
                return s;
            }
        }

        // Called from RuntimeResourceSet
        internal object? LoadObject(int pos)
        {
            lock (this)
            {
                return _version == 1 ? LoadObjectV1(pos) : LoadObjectV2(pos, out _);
            }
        }

        // Called from RuntimeResourceSet
        internal object? LoadObject(int pos, out ResourceTypeCode typeCode)
        {
            lock (this)
            {
                if (_version == 1)
                {
                    object? o = LoadObjectV1(pos);
                    typeCode = (o is string) ? ResourceTypeCode.String : ResourceTypeCode.StartOfUserTypes;
                    return o;
                }
                return LoadObjectV2(pos, out typeCode);
            }
        }

        // This takes a virtual offset into the data section and reads an Object
        // from that location.
        private object? LoadObjectV1(int pos)
        {
            Debug.Assert(_store != null, "ResourceReader is closed!");
            Debug.Assert(_version == 1, ".resources file was not a V1 .resources file!");
            Debug.Assert(Monitor.IsEntered(this)); // uses _store

            try
            {
                // mega try-catch performs exceptionally bad on x64; factored out body into
                // _LoadObjectV1 and wrap here.
                return _LoadObjectV1(pos);
            }
            catch (EndOfStreamException eof)
            {
                throw new BadImageFormatException(MDCFR.Properties.Resources.BadImageFormat_TypeMismatch, eof);
            }
            catch (ArgumentOutOfRangeException e)
            {
                throw new BadImageFormatException(MDCFR.Properties.Resources.BadImageFormat_TypeMismatch, e);
            }
        }

        private object? _LoadObjectV1(int pos)
        {
            Debug.Assert(Monitor.IsEntered(this)); // uses _store

            _store.BaseStream.Seek(_dataSectionOffset + pos, SeekOrigin.Begin);
            int typeIndex = _store.Read7BitEncodedInt();
            if (typeIndex == -1)
                return null;
            Type type = FindType(typeIndex);
            // Consider putting in logic to see if this type is a
            // primitive or a value type first, so we can reach the
            // deserialization code faster for arbitrary objects.

            if (type == typeof(string))
                return _store.ReadString();
            else if (type == typeof(int))
                return _store.ReadInt32();
            else if (type == typeof(byte))
                return _store.ReadByte();
            else if (type == typeof(sbyte))
                return _store.ReadSByte();
            else if (type == typeof(short))
                return _store.ReadInt16();
            else if (type == typeof(long))
                return _store.ReadInt64();
            else if (type == typeof(ushort))
                return _store.ReadUInt16();
            else if (type == typeof(uint))
                return _store.ReadUInt32();
            else if (type == typeof(ulong))
                return _store.ReadUInt64();
            else if (type == typeof(float))
                return _store.ReadSingle();
            else if (type == typeof(double))
                return _store.ReadDouble();
            else if (type == typeof(DateTime))
            {
                // Ideally we should use DateTime's ToBinary & FromBinary,
                // but we can't for compatibility reasons.
                return new DateTime(_store.ReadInt64());
            }
            else if (type == typeof(TimeSpan))
                return new TimeSpan(_store.ReadInt64());
            else if (type == typeof(decimal))
            {
                int[] bits = new int[4];
                for (int i = 0; i < bits.Length; i++)
                    bits[i] = _store.ReadInt32();
                return new decimal(bits);
            }
            else { return DeserializeObject(typeIndex); }
        }

        private object? LoadObjectV2(int pos, out ResourceTypeCode typeCode)
        {
            Debug.Assert(_store != null, "ResourceReader is closed!");
            Debug.Assert(_version >= 2, ".resources file was not a V2 (or higher) .resources file!");
            Debug.Assert(Monitor.IsEntered(this)); // uses _store

            try
            {
                // mega try-catch performs exceptionally bad on x64; factored out body into
                // _LoadObjectV2 and wrap here.
                return _LoadObjectV2(pos, out typeCode);
            }
            catch (EndOfStreamException eof)
            {
                throw new BadImageFormatException(MDCFR.Properties.Resources.BadImageFormat_TypeMismatch, eof);
            }
            catch (ArgumentOutOfRangeException e)
            {
                throw new BadImageFormatException(MDCFR.Properties.Resources.BadImageFormat_TypeMismatch, e);
            }
        }

        private object? _LoadObjectV2(int pos, out ResourceTypeCode typeCode)
        {
            Debug.Assert(Monitor.IsEntered(this)); // uses _store

            _store.BaseStream.Seek(_dataSectionOffset + pos, SeekOrigin.Begin);
            typeCode = (ResourceTypeCode)_store.Read7BitEncodedInt();

            switch (typeCode)
            {
                case ResourceTypeCode.Null:
                    return null;

                case ResourceTypeCode.String:
                    return _store.ReadString();

                case ResourceTypeCode.Boolean:
                    return _store.ReadBoolean();

                case ResourceTypeCode.Char:
                    return (char)_store.ReadUInt16();

                case ResourceTypeCode.Byte:
                    return _store.ReadByte();

                case ResourceTypeCode.SByte:
                    return _store.ReadSByte();

                case ResourceTypeCode.Int16:
                    return _store.ReadInt16();

                case ResourceTypeCode.UInt16:
                    return _store.ReadUInt16();

                case ResourceTypeCode.Int32:
                    return _store.ReadInt32();

                case ResourceTypeCode.UInt32:
                    return _store.ReadUInt32();

                case ResourceTypeCode.Int64:
                    return _store.ReadInt64();

                case ResourceTypeCode.UInt64:
                    return _store.ReadUInt64();

                case ResourceTypeCode.Single:
                    return _store.ReadSingle();

                case ResourceTypeCode.Double:
                    return _store.ReadDouble();

                case ResourceTypeCode.Decimal:
                    return _store.ReadDecimal();

                case ResourceTypeCode.DateTime:
                    // Use DateTime's ToBinary & FromBinary.
                    long data = _store.ReadInt64();
                    return DateTime.FromBinary(data);

                case ResourceTypeCode.TimeSpan:
                    long ticks = _store.ReadInt64();
                    return new TimeSpan(ticks);

                // Special types
                case ResourceTypeCode.ByteArray:
                    {
                        int len = _store.ReadInt32();
                        if (len < 0)
                        {
                            throw new BadImageFormatException(SR.Format(MDCFR.Properties.Resources.BadImageFormat_ResourceDataLengthInvalid, len));
                        }

                        if (_ums == null)
                        {
                            if (len > _store.BaseStream.Length)
                            {
                                throw new BadImageFormatException(SR.Format(MDCFR.Properties.Resources.BadImageFormat_ResourceDataLengthInvalid, len));
                            }
                            return _store.ReadBytes(len);
                        }

                        if (len > _ums.Length - _ums.Position)
                        {
                            throw new BadImageFormatException(SR.Format(MDCFR.Properties.Resources.BadImageFormat_ResourceDataLengthInvalid, len));
                        }

                        byte[] bytes = new byte[len];
                        int r = _ums.Read(bytes, 0, len);
                        Debug.Assert(r == len, "ResourceReader needs to use a blocking read here.  (Call _store.ReadBytes(len)?)");
                        return bytes;
                    }

                case ResourceTypeCode.Stream:
                    {
                        int len = _store.ReadInt32();
                        if (len < 0)
                        {
                            throw new BadImageFormatException(SR.Format(MDCFR.Properties.Resources.BadImageFormat_ResourceDataLengthInvalid, len));
                        }
                        if (_ums == null)
                        {
                            byte[] bytes = _store.ReadBytes(len);
                            // Lifetime of memory == lifetime of this stream.
                            return new PinnedBufferMemoryStream(bytes);
                        }

                        // make sure we don't create an UnmanagedMemoryStream that is longer than the resource stream.
                        if (len > _ums.Length - _ums.Position)
                        {
                            throw new BadImageFormatException(SR.Format(MDCFR.Properties.Resources.BadImageFormat_ResourceDataLengthInvalid, len));
                        }

                        // For the case that we've memory mapped in the .resources
                        // file, just return a Stream pointing to that block of memory.
                        unsafe
                        {
                            return new UnmanagedMemoryStream(_ums.PositionPointer, len, len, FileAccess.Read);
                        }
                    }

                default:
                    if (typeCode < ResourceTypeCode.StartOfUserTypes)
                    {
                        throw new BadImageFormatException(MDCFR.Properties.Resources.BadImageFormat_TypeMismatch);
                    }
                    break;
            }

            // Normal serialized objects
            int typeIndex = typeCode - ResourceTypeCode.StartOfUserTypes;
            return DeserializeObject(typeIndex);
        }

        // Reads in the header information for a .resources file.  Verifies some
        // of the assumptions about this resource set, and builds the class table
        // for the default resource file format.
        [MemberNotNull(nameof(_typeTable))]
        [MemberNotNull(nameof(_typeNamePositions))]
        private void ReadResources()
        {
            Debug.Assert(!Monitor.IsEntered(this)); // only called during init
            Debug.Assert(_store != null, "ResourceReader is closed!");

            try
            {
                // mega try-catch performs exceptionally bad on x64; factored out body into
                // _ReadResources and wrap here.
                _ReadResources();
            }
            catch (EndOfStreamException eof)
            {
                throw new BadImageFormatException(MDCFR.Properties.Resources.BadImageFormat_ResourcesHeaderCorrupted, eof);
            }
            catch (IndexOutOfRangeException e)
            {
                throw new BadImageFormatException(MDCFR.Properties.Resources.BadImageFormat_ResourcesHeaderCorrupted, e);
            }
        }

        [MemberNotNull(nameof(_typeTable))]
        [MemberNotNull(nameof(_typeNamePositions))]
        private void _ReadResources()
        {
            Debug.Assert(!Monitor.IsEntered(this)); // only called during init

            // Read ResourceManager header
            // Check for magic number
            int magicNum = _store.ReadInt32();
            if (magicNum != ResourceManager.MagicNumber)
                throw new ArgumentException(MDCFR.Properties.Resources.Resources_StreamNotValid);
            // Assuming this is ResourceManager header V1 or greater, hopefully
            // after the version number there is a number of bytes to skip
            // to bypass the rest of the ResMgr header. For V2 or greater, we
            // use this to skip to the end of the header
            int resMgrHeaderVersion = _store.ReadInt32();
            int numBytesToSkip = _store.ReadInt32();
            if (numBytesToSkip < 0 || resMgrHeaderVersion < 0)
            {
                throw new BadImageFormatException(MDCFR.Properties.Resources.BadImageFormat_ResourcesHeaderCorrupted);
            }
            if (resMgrHeaderVersion > 1)
            {
                _store.BaseStream.Seek(numBytesToSkip, SeekOrigin.Current);
            }
            else
            {
                // We don't care about numBytesToSkip; read the rest of the header

                // Read in type name for a suitable ResourceReader
                // Note ResourceWriter & InternalResGen use different Strings.
                string readerType = _store.ReadString();

                if (!ValidateReaderType(readerType))
                    throw new NotSupportedException(SR.Format(MDCFR.Properties.Resources.NotSupported_WrongResourceReader_Type, readerType));

                // Skip over type name for a suitable ResourceSet
                SkipString();
            }

            // Read RuntimeResourceSet header
            // Do file version check
            int version = _store.ReadInt32();

            // File format version number
            const int CurrentVersion = 2;

            if (version != CurrentVersion && version != 1)
                throw new ArgumentException(SR.Format(MDCFR.Properties.Resources.Arg_ResourceFileUnsupportedVersion, CurrentVersion, version));
            _version = version;

            _numResources = _store.ReadInt32();
            if (_numResources < 0)
            {
                throw new BadImageFormatException(MDCFR.Properties.Resources.BadImageFormat_ResourcesHeaderCorrupted);
            }

            // Read type positions into type positions array.
            // But delay initialize the type table.
            int numTypes = _store.ReadInt32();
            if (numTypes < 0)
            {
                throw new BadImageFormatException(MDCFR.Properties.Resources.BadImageFormat_ResourcesHeaderCorrupted);
            }
            _typeTable = new Type[numTypes];
            _typeNamePositions = new int[numTypes];
            for (int i = 0; i < numTypes; i++)
            {
                _typeNamePositions[i] = (int)_store.BaseStream.Position;

                // Skip over the Strings in the file.  Don't create types.
                SkipString();
            }

            // Prepare to read in the array of name hashes
            //  Note that the name hashes array is aligned to 8 bytes so
            //  we can use pointers into it on 64 bit machines. (4 bytes
            //  may be sufficient, but let's plan for the future)
            //  Skip over alignment stuff.  All public .resources files
            //  should be aligned   No need to verify the byte values.
            long pos = _store.BaseStream.Position;
            int alignBytes = ((int)pos) & 7;
            if (alignBytes != 0)
            {
                for (int i = 0; i < 8 - alignBytes; i++)
                {
                    _store.ReadByte();
                }
            }

            // Read in the array of name hashes
            if (_ums == null)
            {
                _nameHashes = new int[_numResources];
                for (int i = 0; i < _numResources; i++)
                {
                    _nameHashes[i] = _store.ReadInt32();
                }
            }
            else
            {
                int seekPos = unchecked(4 * _numResources);
                if (seekPos < 0)
                {
                    throw new BadImageFormatException(MDCFR.Properties.Resources.BadImageFormat_ResourcesHeaderCorrupted);
                }
                unsafe
                {
                    _nameHashesPtr = (int*)_ums.PositionPointer;
                    // Skip over the array of nameHashes.
                    _ums.Seek(seekPos, SeekOrigin.Current);
                    // get the position pointer once more to check that the whole table is within the stream
                    _ = _ums.PositionPointer;
                }
            }

            // Read in the array of relative positions for all the names.
            if (_ums == null)
            {
                _namePositions = new int[_numResources];
                for (int i = 0; i < _numResources; i++)
                {
                    int namePosition = _store.ReadInt32();
                    if (namePosition < 0)
                    {
                        throw new BadImageFormatException(MDCFR.Properties.Resources.BadImageFormat_ResourcesHeaderCorrupted);
                    }

                    _namePositions[i] = namePosition;
                }
            }
            else
            {
                int seekPos = unchecked(4 * _numResources);
                if (seekPos < 0)
                {
                    throw new BadImageFormatException(MDCFR.Properties.Resources.BadImageFormat_ResourcesHeaderCorrupted);
                }
                unsafe
                {
                    _namePositionsPtr = (int*)_ums.PositionPointer;
                    // Skip over the array of namePositions.
                    _ums.Seek(seekPos, SeekOrigin.Current);
                    // get the position pointer once more to check that the whole table is within the stream
                    _ = _ums.PositionPointer;
                }
            }

            // Read location of data section.
            _dataSectionOffset = _store.ReadInt32();
            if (_dataSectionOffset < 0)
            {
                throw new BadImageFormatException(MDCFR.Properties.Resources.BadImageFormat_ResourcesHeaderCorrupted);
            }

            // Store current location as start of name section
            _nameSectionOffset = _store.BaseStream.Position;

            // _nameSectionOffset should be <= _dataSectionOffset; if not, it's corrupt
            if (_dataSectionOffset < _nameSectionOffset)
            {
                throw new BadImageFormatException(MDCFR.Properties.Resources.BadImageFormat_ResourcesHeaderCorrupted);
            }
        }

        // This allows us to delay-initialize the Type[].  This might be a
        // good startup time savings, since we might have to load assemblies
        // and initialize Reflection.
        [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026:RequiresUnreferencedCode",
            Justification = "UseReflectionToGetType will get trimmed out when AllowCustomResourceTypes is set to false. " +
            "When set to true, we will already throw a warning for this feature switch, so we suppress this one in order for" +
            "the user to only get one error.")]
        private Type FindType(int typeIndex)
        {
            if (!AllowCustomResourceTypes)
            {
                throw new NotSupportedException(MDCFR.Properties.Resources.ResourceManager_ReflectionNotAllowed);
            }

            if (typeIndex < 0 || typeIndex >= _typeTable.Length)
            {
                throw new BadImageFormatException(MDCFR.Properties.Resources.BadImageFormat_InvalidType);
            }

            return _typeTable[typeIndex] ?? UseReflectionToGetType(typeIndex);
        }

        [RequiresUnreferencedCode("The CustomResourceTypesSupport feature switch has been enabled for this app which is being trimmed. " +
            "Custom readers as well as custom objects on the resources file are not observable by the trimmer and so required assemblies, types and members may be removed.")]
        private Type UseReflectionToGetType(int typeIndex)
        {
            Debug.Assert(Monitor.IsEntered(this)); // uses _store

            long oldPos = _store.BaseStream.Position;
            try
            {
                _store.BaseStream.Position = _typeNamePositions[typeIndex];
                string typeName = _store.ReadString();
                _typeTable[typeIndex] = Type.GetType(typeName, true);
                Debug.Assert(_typeTable[typeIndex] != null, "Should have found a type!");
                return _typeTable[typeIndex]!;
            }
            // If-defing this coud out from Resources Extensions since they will by definition always support deserialization
            // So we shouldn't attempt to wrap the original exception with a NotSupportedException since that can be misleading.
            // For that reason, the bellow code is only relevant when building CoreLib's ResourceReader.
            finally
            {
                _store.BaseStream.Position = oldPos;
            }
        }

        // Finds the ResourceTypeCode for a type, or adds this type to the
        // types list.
        private static ResourceTypeCode FindTypeCode(object? value, List<string> types)
        {
            if (value == null)
                return ResourceTypeCode.Null;

            Type type = value.GetType();
            if (type == typeof(string))
                return ResourceTypeCode.String;
            else if (type == typeof(int))
                return ResourceTypeCode.Int32;
            else if (type == typeof(bool))
                return ResourceTypeCode.Boolean;
            else if (type == typeof(char))
                return ResourceTypeCode.Char;
            else if (type == typeof(byte))
                return ResourceTypeCode.Byte;
            else if (type == typeof(sbyte))
                return ResourceTypeCode.SByte;
            else if (type == typeof(short))
                return ResourceTypeCode.Int16;
            else if (type == typeof(long))
                return ResourceTypeCode.Int64;
            else if (type == typeof(ushort))
                return ResourceTypeCode.UInt16;
            else if (type == typeof(uint))
                return ResourceTypeCode.UInt32;
            else if (type == typeof(ulong))
                return ResourceTypeCode.UInt64;
            else if (type == typeof(float))
                return ResourceTypeCode.Single;
            else if (type == typeof(double))
                return ResourceTypeCode.Double;
            else if (type == typeof(decimal))
                return ResourceTypeCode.Decimal;
            else if (type == typeof(DateTime))
                return ResourceTypeCode.DateTime;
            else if (type == typeof(TimeSpan))
                return ResourceTypeCode.TimeSpan;
            else if (type == typeof(byte[]))
                return ResourceTypeCode.ByteArray;
            else if (type == typeof(StreamWrapper))
                return ResourceTypeCode.Stream;


            // This is a user type, or a precanned resource.  Find type
            // table index.  If not there, add new element.
            string typeName;
            if (type == typeof(PrecannedResource))
            {
                typeName = ((PrecannedResource)value).TypeName;
                if (typeName.StartsWith("ResourceTypeCode.", StringComparison.Ordinal))
                {
                    typeName = typeName.Substring(17);  // Remove through '.'
                    ResourceTypeCode typeCode = (ResourceTypeCode)Enum.Parse(typeof(ResourceTypeCode), typeName);
                    return typeCode;
                }
            }
            else
            {
                // not a preserialized resource
                throw new PlatformNotSupportedException(MDCFR.Properties.Resources.NotSupported_BinarySerializedResources);
            }

            int typeIndex = types.IndexOf(typeName);
            if (typeIndex == -1)
            {
                typeIndex = types.Count;
                types.Add(typeName);
            }

            return (ResourceTypeCode) (typeIndex + ResourceTypeCode.StartOfUserTypes);
        }

        private string TypeNameFromTypeCode(ResourceTypeCode typeCode)
        {
            Debug.Assert(typeCode >= 0, "can't be negative");
            Debug.Assert(Monitor.IsEntered(this)); // uses _store

            if (typeCode < ResourceTypeCode.StartOfUserTypes)
            {
                Debug.Assert(!string.Equals(typeCode.ToString(), "LastPrimitive"), "Change ResourceTypeCode metadata order so LastPrimitive isn't what Enum.ToString prefers.");
                return "ResourceTypeCode." + typeCode.ToString();
            }
            else
            {
                int typeIndex = typeCode - ResourceTypeCode.StartOfUserTypes;
                Debug.Assert(typeIndex >= 0 && typeIndex < _typeTable.Length, "TypeCode is broken or corrupted!");
                long oldPos = _store.BaseStream.Position;
                try
                {
                    _store.BaseStream.Position = _typeNamePositions[typeIndex];
                    return _store.ReadString();
                }
                finally
                {
                    _store.BaseStream.Position = oldPos;
                }
            }
        }

        internal sealed class ResourceEnumerator : IDictionaryEnumerator
        {
            private const int ENUM_DONE = int.MinValue;
            private const int ENUM_NOT_STARTED = -1;

            private readonly ResourceReader _reader;
            private bool _currentIsValid;
            private int _currentName;
            private int _dataPosition; // cached for case-insensitive table

            internal ResourceEnumerator(ResourceReader reader)
            {
                _currentName = ENUM_NOT_STARTED;
                _reader = reader;
                _dataPosition = -2;
            }

            public bool MoveNext()
            {
                if (_currentName == _reader._numResources - 1 || _currentName == ENUM_DONE)
                {
                    _currentIsValid = false;
                    _currentName = ENUM_DONE;
                    return false;
                }
                _currentIsValid = true;
                _currentName++;
                return true;
            }

            public object Key
            {
                get
                {
                    if (_currentName == ENUM_DONE) throw new InvalidOperationException(MDCFR.Properties.Resources.InvalidOperation_EnumEnded);
                    if (!_currentIsValid) throw new InvalidOperationException(MDCFR.Properties.Resources.InvalidOperation_EnumNotStarted);
                    if (_reader._resCache == null) throw new InvalidOperationException(MDCFR.Properties.Resources.ResourceReaderIsClosed);

                    return _reader.AllocateStringForNameIndex(_currentName, out _dataPosition);
                }
            }

            public object Current => Entry;

            // Warning: This requires that you call the Key or Entry property FIRST before calling it!
            internal int DataPosition => _dataPosition;

            public DictionaryEntry Entry
            {
                get
                {
                    if (_currentName == ENUM_DONE) throw new InvalidOperationException(MDCFR.Properties.Resources.InvalidOperation_EnumEnded);
                    if (!_currentIsValid) throw new InvalidOperationException(MDCFR.Properties.Resources.InvalidOperation_EnumNotStarted);
                    if (_reader._resCache == null) throw new InvalidOperationException(MDCFR.Properties.Resources.ResourceReaderIsClosed);

                    string key = _reader.AllocateStringForNameIndex(_currentName, out _dataPosition); // AllocateStringForNameIndex could lock on _reader

                    object? value = null;
                    // Lock the cache first, then the reader (in this case, we don't actually need to lock the reader and cache at the same time).
                    // Lock order MUST match RuntimeResourceSet.GetObject to avoid deadlock.
                    Debug.Assert(!Monitor.IsEntered(_reader));
                    lock (_reader._resCache)
                    {
                        if (_reader._resCache.TryGetValue(key, out ResourceLocator locator))
                        {
                            value = locator.Value;
                        }
                    }
                    if (value is null)
                    {
                        if (_dataPosition == -1)
                            value = _reader.GetValueForNameIndex(_currentName);
                        else
                            value = _reader.LoadObject(_dataPosition);
                        // If enumeration and subsequent lookups happen very
                        // frequently in the same process, add a ResourceLocator
                        // to _resCache here (we'll also need to extend the lock block!).
                        // But WinForms enumerates and just about everyone else does lookups.
                        // So caching here may bloat working set.
                    }
                    return new DictionaryEntry(key, value);
                }
            }

            public object? Value
            {
                get
                {
                    if (_currentName == ENUM_DONE) throw new InvalidOperationException(MDCFR.Properties.Resources.InvalidOperation_EnumEnded);
                    if (!_currentIsValid) throw new InvalidOperationException(MDCFR.Properties.Resources.InvalidOperation_EnumNotStarted);
                    if (_reader._resCache == null) throw new InvalidOperationException(MDCFR.Properties.Resources.ResourceReaderIsClosed);

                    // Consider using _resCache here, eventually, if
                    // this proves to be an interesting perf scenario.
                    // But mixing lookups and enumerators shouldn't be
                    // particularly compelling.
                    return _reader.GetValueForNameIndex(_currentName);
                }
            }

            public void Reset()
            {
                if (_reader._resCache == null) throw new InvalidOperationException(MDCFR.Properties.Resources.ResourceReaderIsClosed);
                _currentIsValid = false;
                _currentName = ENUM_NOT_STARTED;
            }
        }
    
    }

}


namespace System.Resources
{

    internal sealed class FastResourceComparer : IComparer, IEqualityComparer, IComparer<string?>, IEqualityComparer<string?>
    {
        internal static readonly FastResourceComparer Default = new FastResourceComparer();

        // Implements IHashCodeProvider too, due to Hashtable requirements.
        public int GetHashCode(object key)
        {
            string s = (string) key;
            return FastResourceComparer.HashFunction(s);
        }
#pragma warning disable CS8767
        public int GetHashCode([DisallowNull] string? key)
        {
            return FastResourceComparer.HashFunction(key!);
        }
#pragma warning restore CS8767

        // This hash function MUST be publicly documented with the resource
        // file format, AND we may NEVER change this hash function's return
        // value (without changing the file format).
        internal static int HashFunction(string key)
        {
            // Never change this hash function.  We must standardize it so that
            // others can read & write our .resources files.  Additionally, we
            // have a copy of it in InternalResGen as well.
            uint hash = 5381;
            for (int i = 0; i < key.Length; i++)
                hash = ((hash << 5) + hash) ^ key[i];
            return (int)hash;
        }

        // Compares Strings quickly in a case-sensitive way
        public int Compare(object? a, object? b)
        {
            if (a == b) return 0;
            string? sa = (string?)a;
            string? sb = (string?)b;
            return string.CompareOrdinal(sa, sb);
        }

        public int Compare(string? a, string? b)
        {
            return string.CompareOrdinal(a, b);
        }

        public bool Equals(string? a, string? b)
        {
            return string.Equals(a, b);
        }

        public new bool Equals(object? a, object? b)
        {
            if (a == b) return true;
            string? sa = (string?)a;
            string? sb = (string?)b;
            return string.Equals(sa, sb);
        }

        // Input is one string to compare with, and a byte[] containing chars in
        // little endian unicode.  Pass in the number of valid chars.
        public static unsafe int CompareOrdinal(string a, byte[] bytes, int bCharLength)
        {
            Debug.Assert(a != null && bytes != null, "FastResourceComparer::CompareOrdinal must have non-null params");
            Debug.Assert(bCharLength * 2 <= bytes.Length, "FastResourceComparer::CompareOrdinal - numChars is too big!");
            // This is a managed version of strcmp, but I can't take advantage
            // of a terminating 0, unlike strcmp in C.
            int i = 0;
            int r = 0;
            // Compare the min length # of characters, then return length diffs.
            int numChars = a.Length;
            if (numChars > bCharLength)
                numChars = bCharLength;
            if (bCharLength == 0)   // Can't use fixed on a 0-element array.
                return (a.Length == 0) ? 0 : -1;
            fixed (byte* pb = bytes)
            {
                byte* pChar = pb;
                while (i < numChars && r == 0)
                {
                    // little endian format
                    int b = pChar[0] | pChar[1] << 8;
                    r = a[i++] - b;
                    pChar += sizeof(char);
                }
            }
            if (r != 0) return r;
            return a.Length - bCharLength;
        }

        public static int CompareOrdinal(byte[] bytes, int aCharLength, string b)
        {
            return -CompareOrdinal(b, bytes, aCharLength);
        }

        // This method is to handle potentially misaligned data accesses.
        // The byte* must point to little endian Unicode characters.
        internal static unsafe int CompareOrdinal(byte* a, int byteLen, string b)
        {
            Debug.Assert((byteLen & 1) == 0, "CompareOrdinal is expecting a UTF-16 string length, which must be even!");
            Debug.Assert(a != null && b != null, "Null args not allowed.");
            Debug.Assert(byteLen >= 0, "byteLen must be non-negative.");

            int r = 0;
            int i = 0;
            // Compare the min length # of characters, then return length diffs.
            int numChars = byteLen >> 1;
            if (numChars > b.Length)
                numChars = b.Length;
            while (i < numChars && r == 0)
            {
                // Must compare character by character, not byte by byte.
                char aCh = (char)(*a++ | (*a++ << 8));
                r = aCh - b[i++];
            }
            if (r != 0) return r;
            return byteLen - b.Length * 2;
        }
    }


    /* An internal implementation detail for .resources files, describing
       what type an object is.
       Ranges:
       0 - 0x1F     Primitives and reserved values
       0x20 - 0x3F  Specially recognized types, like byte[] and Streams

       Note this data must be included in any documentation describing the
       internals of .resources files.
    */
    internal enum ResourceTypeCode
    {
        // Primitives
        Null = 0,
        String = 1,
        Boolean = 2,
        Char = 3,
        Byte = 4,
        SByte = 5,
        Int16 = 6,
        UInt16 = 7,
        Int32 = 8,
        UInt32 = 9,
        Int64 = 0xa,
        UInt64 = 0xb,
        Single = 0xc,
        Double = 0xd,
        Decimal = 0xe,
        DateTime = 0xf,
        TimeSpan = 0x10,

        // A meta-value - change this if you add new primitives
        LastPrimitive = TimeSpan,

        // Types with a special representation, like byte[] and Stream
        ByteArray = 0x20,
        Stream = 0x21,

        // User types - serialized using the binary formatter.
        StartOfUserTypes = 0x40
    }

}
#pragma warning restore CS1591, CS8602
#nullable disable