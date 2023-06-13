// This CS file contains external archiving methods.
// First one: (Version 1.5.4.6) Tar Archives.
// Second one: (Version 1.5.5.0) Cabinet Archives.
// Third one: (Version 1.5.5.0) Snappy Archives.

// Global Preprocessor definitions
#define INTERNAL_NULLABLE_ATTRIBUTES

// Global namespaces
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Buffers;
using System.Security;
using System.Threading;
using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;
using System.Buffers.Binary;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
//Global namespaces when the code is compiled to >= .NET 6 
#if NET6_0_OR_GREATER
	using System.Runtime.Intrinsics;
	using System.Runtime.Intrinsics.X86;
	using static System.Runtime.Intrinsics.X86.Ssse3;
#endif

//Code required when the Snappy Archiving is compiled < .NET 6 .
#if !NET6_0_OR_GREATER
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace
namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    internal sealed class CallerArgumentExpressionAttribute : Attribute
    {
        public CallerArgumentExpressionAttribute(string parameterName)
        {
            ParameterName = parameterName;
        }

        public string ParameterName { get; }
    }
}
#endif

#if NETSTANDARD2_0 || NETCOREAPP2_0 || NETCOREAPP2_1 || NETCOREAPP2_2 || NET45 || NET451 || NET452 || NET6 || NET461 || NET462 || NET47 || NET471 || NET472 || NET48

// https://github.com/dotnet/corefx/blob/48363ac826ccf66fbe31a5dcb1dc2aab9a7dd768/src/Common/src/CoreLib/System/Diagnostics/CodeAnalysis/NullableAttributes.cs

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// ReSharper disable once CheckNamespace
namespace System.Diagnostics.CodeAnalysis
{
    /// <summary>Specifies that null is allowed as an input even if the corresponding type disallows it.</summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property, Inherited = false)]
		#if INTERNAL_NULLABLE_ATTRIBUTES
			internal
		#else
			public
		#endif
        sealed class AllowNullAttribute : Attribute { }

    /// <summary>Specifies that null is disallowed as an input even if the corresponding type allows it.</summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property, Inherited = false)]
		#if INTERNAL_NULLABLE_ATTRIBUTES
			internal
		#else
			public
		#endif
        sealed class DisallowNullAttribute : Attribute { }

    /// <summary>Specifies that an output may be null even if the corresponding type disallows it.</summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue, Inherited = false)]
		#if INTERNAL_NULLABLE_ATTRIBUTES
			internal
		#else
			public
		#endif
        sealed class MaybeNullAttribute : Attribute { }

    /// <summary>Specifies that an output will not be null even if the corresponding type allows it.</summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue, Inherited = false)]
		#if INTERNAL_NULLABLE_ATTRIBUTES
			internal
		#else
			public
		#endif
        sealed class NotNullAttribute : Attribute { }

    /// <summary>Specifies that when a method returns <see cref="ReturnValue"/>, the parameter may be null even if the corresponding type disallows it.</summary>
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
		#if INTERNAL_NULLABLE_ATTRIBUTES
			internal
		#else
			public
		#endif
        sealed class MaybeNullWhenAttribute : Attribute
		{
			/// <summary>Initializes the attribute with the specified return value condition.</summary>
			/// <param name="returnValue">
			/// The return value condition. If the method returns this value, the associated parameter may be null.
			/// </param>
			public MaybeNullWhenAttribute(bool returnValue) => ReturnValue = returnValue;

			/// <summary>Gets the return value condition.</summary>
			public bool ReturnValue { get; }
		}

    /// <summary>Specifies that when a method returns <see cref="ReturnValue"/>, the parameter will not be null even if the corresponding type allows it.</summary>
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
		#if INTERNAL_NULLABLE_ATTRIBUTES
			internal
		#else
			public
		#endif
        sealed class NotNullWhenAttribute : Attribute
		{
			/// <summary>Initializes the attribute with the specified return value condition.</summary>
			/// <param name="returnValue">
			/// The return value condition. If the method returns this value, the associated parameter will not be null.
			/// </param>
			public NotNullWhenAttribute(bool returnValue) => ReturnValue = returnValue;

			/// <summary>Gets the return value condition.</summary>
			public bool ReturnValue { get; }
		}

    /// <summary>Specifies that the output will be non-null if the named parameter is non-null.</summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue, AllowMultiple = true, Inherited = false)]
		#if INTERNAL_NULLABLE_ATTRIBUTES
			internal
		#else
	        public
		#endif
        sealed class NotNullIfNotNullAttribute : Attribute
		{
			/// <summary>Initializes the attribute with the associated parameter name.</summary>
			/// <param name="parameterName">
			/// The associated parameter name.  The output will be non-null if the argument to the parameter specified is non-null.
			/// </param>
			public NotNullIfNotNullAttribute(string parameterName) => ParameterName = parameterName;

			/// <summary>Gets the associated parameter name.</summary>
			public string ParameterName { get; }
		}

    /// <summary>Applied to a method that will never return under any circumstance.</summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
		#if INTERNAL_NULLABLE_ATTRIBUTES
			internal
		#else
			public
		#endif
        sealed class DoesNotReturnAttribute : Attribute { }

    /// <summary>Specifies that the method will not return if the associated Boolean parameter is passed the specified value.</summary>
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
		#if INTERNAL_NULLABLE_ATTRIBUTES
			internal
		#else
			public
		#endif
        sealed class DoesNotReturnIfAttribute : Attribute
		{
			/// <summary>Initializes the attribute with the specified parameter value.</summary>
			/// <param name="parameterValue">
			/// The condition parameter value. Code after the method will be considered unreachable by diagnostics if the argument to
			/// the associated parameter matches this value.
			/// </param>
			public DoesNotReturnIfAttribute(bool parameterValue) => ParameterValue = parameterValue;

			/// <summary>Gets the condition parameter value.</summary>
			public bool ParameterValue { get; }
		}
}

#endif

namespace ExternalArchivingMethods
{
	
	namespace Tars
	{
		/*
			Original License copy:
			BSD License

			Copyright (c) 2009, Vladimir Vasiltsov All rights reserved.

			Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

				* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
				* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
				* Names of its contributors may not be used to endorse or promote products derived from this software without specific prior written permission. 

			THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
			THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
			IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
			(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) 
			HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
			ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
		*/
		
		// DataWriter.cs file: <--
		internal class DataWriter : IArchiveDataWriter
		{
			private readonly long size;
			private long remainingBytes;
			private bool canWrite = true;
			private readonly Stream stream;

			public DataWriter(Stream data, long dataSizeInBytes)
			{
				size = dataSizeInBytes;
				remainingBytes = size;
				stream = data;
			}

			public int Write(byte[] buffer, int count)
			{
				if(remainingBytes == 0)
				{
					canWrite = false;
					return -1;
				}
				int bytesToWrite;
				if(remainingBytes - count < 0)
				{
					bytesToWrite = (int)remainingBytes;
				}
				else
				{
					bytesToWrite = count;
				}
				stream.Write(buffer,0,bytesToWrite);
				remainingBytes -= bytesToWrite;
				return bytesToWrite;
			}

			public bool CanWrite
			{
				get
				{
					return canWrite;
				}
			}
		}
		// Ends here: -->
		
		// IArchiveDataWriter.cs file: <--
		public interface IArchiveDataWriter
		{
			/// <summary>
			/// Write `length` bytes of data from `buffer` to corresponding archive.
			/// </summary>
			/// <param name="buffer">data storage</param>
			/// <param name="count">how many bytes to be written to the corresponding archive</param>
			int Write(byte[] buffer, int count);
			bool CanWrite { get; }
		}
		public delegate void WriteDataDelegate(IArchiveDataWriter writer);
		// Ends here: -->
		
		// ITarHeader.cs file: <--
		/// <summary>
		/// The Entry type that indicates the type of data saved to the Tar.
		/// </summary>
		public enum EntryType : byte
		{
			/// <summary>
			/// The Entry is a file.
			/// </summary>
			File = 0,
			FileObsolete = 0x30,
			/// <summary>
			/// The Entry is a hard-coded(strongly typed) link.
			/// </summary>
			HardLink = 0x31,
			/// <summary>
			/// The Entry is a symbolic link.
			/// </summary>
			SymLink = 0x32,
			CharDevice = 0x33,
			BlockDevice = 0x34,
			/// <summary>
			/// The Entry is a directory.
			/// </summary>
			Directory = 0x35,
			Fifo = 0x36,
		}

		public interface ITarHeader
		{
			string FileName { get; set; }
			int Mode { get; set; }
			int UserId { get; set; }
			string UserName { get; set; }
			int GroupId { get; set; }
			string GroupName { get; set; }
			long SizeInBytes { get; set; }
			DateTime LastModification { get; set; }
			int HeaderSize { get; }
			EntryType EntryType { get; set; }
		}
		// Ends here: -->
		
		
		// LegacyTarWriter.cs file: <--
		/// <summary>
		/// The Legacy Tar writer specified by it's original author. You should not use this class in your source code.
		/// </summary>
		public class LegacyTarWriter : IDisposable
		{
			private readonly Stream outStream;
			protected byte[] buffer = new byte[1024];
			private bool isClosed;
			public bool ReadOnZero = true;

			/// <summary>
			/// Writes tar (see GNU tar) archive to a stream
			/// </summary>
			/// <param name="writeStream">stream to write archive to</param>
			public LegacyTarWriter(Stream writeStream)
			{
				outStream = writeStream;
			}

			protected virtual Stream OutStream
			{
				get { return outStream; }
			}

			#region IDisposable Members

			public void Dispose()
			{
				Close();
			}

			#endregion


			public void WriteDirectoryEntry(string path,int userId, int groupId, int mode)
			{
				if (string.IsNullOrEmpty(path))
					throw new ArgumentNullException("path");
				if (path[path.Length - 1] != '/')
				{
					path += '/';
				}
				DateTime lastWriteTime;
				if (Directory.Exists(path))
				{
					lastWriteTime = Directory.GetLastWriteTime(path);
				}
				else
				{
					lastWriteTime = DateTime.Now;
				}
				WriteHeader(path, lastWriteTime, 0, userId, groupId, mode, EntryType.Directory);
			}

			public void WriteDirectoryEntry(string path)
			{
				WriteDirectoryEntry(path, 101, 101, 0777);
			}

			public void WriteDirectory(string directory, bool doRecursive)
			{
				if (string.IsNullOrEmpty(directory))
					throw new ArgumentNullException("directory");

				WriteDirectoryEntry(directory);

				string[] files = Directory.GetFiles(directory);
				foreach(var fileName in files)
				{
					Write(fileName);
				}

				string[] directories = Directory.GetDirectories(directory);
				foreach(var dirName in directories)
				{
					WriteDirectoryEntry(dirName);
					if(doRecursive)
					{
						WriteDirectory(dirName,true);
					}
				}
			}


			public void Write(string fileName)
			{
				if(string.IsNullOrEmpty(fileName))
					throw new ArgumentNullException("fileName");
				using (FileStream file = File.OpenRead(fileName))
				{
					Write(file, file.Length, fileName, 61, 61, 511, File.GetLastWriteTime(file.Name));
				}
			}

			public void Write(FileStream file)
			{
				string path = Path.GetFullPath(file.Name).Replace(Path.GetPathRoot(file.Name),string.Empty);
				path = path.Replace(Path.DirectorySeparatorChar, '/');
				Write(file, file.Length, path, 61, 61, 511, File.GetLastWriteTime(file.Name));
			}

			public void Write(Stream data, long dataSizeInBytes, string name)
			{
				Write(data, dataSizeInBytes, name, 61, 61, 511, DateTime.Now);
			}

			public virtual void Write(string name, long dataSizeInBytes, int userId, int groupId, int mode, DateTime lastModificationTime, WriteDataDelegate writeDelegate)
			{
				IArchiveDataWriter writer = new DataWriter(OutStream, dataSizeInBytes);
				WriteHeader(name, lastModificationTime, dataSizeInBytes, userId, groupId, mode, EntryType.File);
				while(writer.CanWrite)
				{
					writeDelegate(writer);
				}
				AlignTo512(dataSizeInBytes, false);
			}

			public virtual void Write(Stream data, long dataSizeInBytes, string name, int userId, int groupId, int mode,
									  DateTime lastModificationTime)
			{
				if(isClosed)
					throw new TarException("Can not write to the closed writer");
				WriteHeader(name, lastModificationTime, dataSizeInBytes, userId, groupId, mode, EntryType.File);
				WriteContent(dataSizeInBytes, data);
				AlignTo512(dataSizeInBytes,false);
			}

			protected void WriteContent(long count, Stream data)
			{
				while (count > 0 && count > buffer.Length)
				{
					int bytesRead = data.Read(buffer, 0, buffer.Length);
					if (bytesRead < 0)
						throw new IOException("LegacyTarWriter unable to read from provided stream");
					if (bytesRead == 0)
					{
						if (ReadOnZero)
							Thread.Sleep(100);
						else
							break;
					}
					OutStream.Write(buffer, 0, bytesRead);
					count -= bytesRead;
				}
				if (count > 0)
				{
					int bytesRead = data.Read(buffer, 0, (int) count);
					if (bytesRead < 0)
						throw new IOException("LegacyTarWriter unable to read from provided stream");
					if (bytesRead == 0)
					{
						while (count > 0)
						{
							OutStream.WriteByte(0);
							--count;
						}
					}
					else
						OutStream.Write(buffer, 0, bytesRead);
				}
			}

			protected virtual void WriteHeader(string name, DateTime lastModificationTime, long count, int userId, int groupId, int mode, EntryType entryType)
			{
				var header = new TarHeader
							 {
								 FileName = name,
								 LastModification = lastModificationTime,
								 SizeInBytes = count,
								 UserId = userId,
								 GroupId = groupId,
								 Mode = mode,
								 EntryType = entryType
							 };
				OutStream.Write(header.GetHeaderValue(), 0, header.HeaderSize);
			}


			public void AlignTo512(long size,bool acceptZero)
			{
				size = size%512;
				if (size == 0 && !acceptZero) return;
				while (size < 512)
				{
					OutStream.WriteByte(0);
					size++;
				}
			}

			public virtual void Close()
			{
				if (isClosed) return;
				AlignTo512(0,true);
				AlignTo512(0,true);
				isClosed = true;
			}
		}
		// Ends here: -->
		
		// TarException.cs file: <--
		/// <summary>
		/// It is a normal <see cref="System.Exception"/> that allows you to differentiate the the source of the exception (That is , the Tar Code) .
		/// </summary>
		public class TarException : Exception
		{
			public TarException(string message) : base(message)
			{
			}
		}
		// Ends here: -->
		
		// TarHeader.cs file: <--
		internal class TarHeader : ITarHeader
		{
			private readonly byte[] buffer = new byte[512];
			private long headerChecksum;

			public TarHeader()
			{
				// Default values
				Mode = 511; // 0777 dec
				UserId = 61; // 101 dec
				GroupId = 61; // 101 dec
			}

			private string fileName;
			protected readonly DateTime TheEpoch = new DateTime(1970, 1, 1, 0, 0, 0);
			public EntryType EntryType { get; set; }
			private static byte[] spaces = Encoding.ASCII.GetBytes("        ");

			public virtual string FileName
			{
				get
				{
					return fileName.Replace("\0",string.Empty);
				} 
				set
				{
					if(value.Length > 100)
					{
						throw new TarException("A file name can not be more than 100 chars long");
					}
					fileName = value;
				}
			}
			public int Mode { get; set; }

			public string ModeString
			{
				get { return Convert.ToString(Mode, 8).PadLeft(7, '0'); }
			}

			public int UserId { get; set; }
			public virtual string UserName
			{
				get { return UserId.ToString(); }
				set { UserId = Int32.Parse(value); }
			}

			public string UserIdString
			{
				get { return Convert.ToString(UserId, 8).PadLeft(7, '0'); }
			}

			public int GroupId { get; set; }
			public virtual string GroupName
			{
				get { return GroupId.ToString(); }
				set { GroupId = Int32.Parse(value); }
			}

			public string GroupIdString
			{
				get { return Convert.ToString(GroupId, 8).PadLeft(7, '0'); }
			}

			public long SizeInBytes { get; set; }

			public string SizeString
			{
				get { return Convert.ToString(SizeInBytes, 8).PadLeft(11, '0'); }
			}

			public DateTime LastModification { get; set; }

			public string LastModificationString
			{
				get
				{
					return Convert.ToString((long)(LastModification - TheEpoch).TotalSeconds, 8).PadLeft(11, '0');
				}
			}

			public string HeaderChecksumString
			{
				get { return Convert.ToString(headerChecksum, 8).PadLeft(6, '0'); }
			}


			public virtual int HeaderSize
			{
				get { return 512; }
			}

			public byte[] GetBytes()
			{
				return buffer;
			}

			public virtual bool UpdateHeaderFromBytes()
			{
				FileName = Encoding.ASCII.GetString(buffer, 0, 100);
				// thanks to Shasha Alperocivh. Trimming nulls.
				Mode = Convert.ToInt32(Encoding.ASCII.GetString(buffer, 100, 7).Trim(), 8);
				UserId = Convert.ToInt32(Encoding.ASCII.GetString(buffer, 108, 7).Trim(), 8);
				GroupId = Convert.ToInt32(Encoding.ASCII.GetString(buffer, 116, 7).Trim(), 8);

				EntryType = (EntryType)buffer[156];

				if((buffer[124] & 0x80) == 0x80) // if size in binary
				{
					long sizeBigEndian = BitConverter.ToInt64(buffer,0x80);
					SizeInBytes = IPAddress.NetworkToHostOrder(sizeBigEndian);
				}
				else
				{
					SizeInBytes = Convert.ToInt64(Encoding.ASCII.GetString(buffer, 124, 11), 8);
				}
				long unixTimeStamp = Convert.ToInt64(Encoding.ASCII.GetString(buffer,136,11),8);
				LastModification = TheEpoch.AddSeconds(unixTimeStamp);

				var storedChecksum = Convert.ToInt32(Encoding.ASCII.GetString(buffer,148,6));
				RecalculateChecksum(buffer);
				if (storedChecksum == headerChecksum)
				{
					return true;
				}

				RecalculateAltChecksum(buffer);
				return storedChecksum == headerChecksum;
			}

			private void RecalculateAltChecksum(byte[] buf)
			{
				spaces.CopyTo(buf, 148);
				headerChecksum = 0;
				foreach(byte b in buf)
				{
					if((b & 0x80) == 0x80)
					{
						headerChecksum -= b ^ 0x80;
					}
					else
					{
						headerChecksum += b;
					}
				}
			}

			public virtual byte[] GetHeaderValue()
			{
				// Clean old values
				Array.Clear(buffer,0, buffer.Length);

				if (string.IsNullOrEmpty(FileName)) throw new TarException("FileName can not be empty.");
				if (FileName.Length >= 100) throw new TarException("FileName is too long. It must be less than 100 bytes.");

				// Fill header
				Encoding.ASCII.GetBytes(FileName.PadRight(100, '\0')).CopyTo(buffer, 0);
				Encoding.ASCII.GetBytes(ModeString).CopyTo(buffer, 100);
				Encoding.ASCII.GetBytes(UserIdString).CopyTo(buffer, 108);
				Encoding.ASCII.GetBytes(GroupIdString).CopyTo(buffer, 116);
				Encoding.ASCII.GetBytes(SizeString).CopyTo(buffer, 124);
				Encoding.ASCII.GetBytes(LastModificationString).CopyTo(buffer, 136);

				//buffer[156] = 20;
				buffer[156] = ((byte) EntryType);


				RecalculateChecksum(buffer);

				// Write checksum
				Encoding.ASCII.GetBytes(HeaderChecksumString).CopyTo(buffer, 148);

				return buffer;
			}

			protected virtual void RecalculateChecksum(byte[] buf)
			{
				// Set default value for checksum. That is 8 spaces.
				spaces.CopyTo(buf, 148);

				// Calculate checksum
				headerChecksum = 0;
				foreach (byte b in buf)
				{
					headerChecksum += b;
				}
			}
		}
		// Ends here: -->
		
		// TarReader.cs file: <--
		
		/// <summary>
		/// Extract contents of a Tar file represented by a stream for the <see cref="TarReader"/> constructor
		/// </summary>
		public class TarReader
		{
			private readonly byte[] dataBuffer = new byte[512];
			private readonly UsTarHeader header;
			private readonly Stream inStream;
			private long remainingBytesInFile;

			/// <summary>
			/// Constructs TarReader object to read data from `tarredData` stream
			/// </summary>
			/// <param name="tarredData">A stream to read tar archive from</param>
			public TarReader(Stream tarredData)
			{
				inStream = tarredData;
				header = new UsTarHeader();
			}

			public ITarHeader FileInfo
			{
				get { return header; }
			}

            /// <summary>
            /// Read all files from an archive to a directory. It creates some child directories to
            /// reproduce a file structure from the archive.
            /// </summary>
            /// <param name="destDirectory">The out directory.</param>
            ///  <remarks>
            /// CAUTION! This method is not safe. It's not tar-bomb proof. 
            /// {see http://en.wikipedia.org/wiki/Tar_(file_format) }
            /// If you are not sure about the source of an archive you extracting,
            /// then use MoveNext and Read and handle paths like ".." and "../.." according
            /// to your business logic. </remarks>
            public void ReadToEnd(string destDirectory)
			{
				while (MoveNext(false))
				{
					string fileNameFromArchive = FileInfo.FileName;
					string totalPath = destDirectory + Path.DirectorySeparatorChar + fileNameFromArchive;
					if(UsTarHeader.IsPathSeparator(fileNameFromArchive[fileNameFromArchive.Length -1]) || FileInfo.EntryType == EntryType.Directory)
					{
						// Record is a directory
						Directory.CreateDirectory(totalPath);
						continue;
					}
					// If record is a file
					string fileName = Path.GetFileName(totalPath);
					string directory = totalPath.Remove(totalPath.Length - fileName.Length);
					Directory.CreateDirectory(directory);
					using (FileStream file = File.Create(totalPath))
					{
						Read(file);
					}
				}
			}
			
			/// <summary>
			/// Read data from a current file to a Stream.
			/// </summary>
			/// <param name="dataDestanation">A stream to read data to</param>
			/// 
			/// <seealso cref="MoveNext"/>
			public void Read(Stream dataDestanation)
			{
				Debug.WriteLine("tar stream position Read in: " + inStream.Position);
				int readBytes;
				byte[] read;
				while ((readBytes = Read(out read)) != -1)
				{
					Debug.WriteLine("tar stream position Read while(...) : " + inStream.Position);
					dataDestanation.Write(read, 0, readBytes);
				}
				Debug.WriteLine("tar stream position Read out: " + inStream.Position);
			}

			protected int Read(out byte[] buffer)
			{
				if(remainingBytesInFile == 0)
				{
					buffer = null;
					return -1;
				}
				int align512 = -1;
				long toRead = remainingBytesInFile - 512;

				if (toRead > 0) 
					toRead = 512;
				else
				{
					align512 = 512 - (int)remainingBytesInFile;
					toRead = remainingBytesInFile;
				}

				int bytesRead = 0;
				long bytesRemainingToRead = toRead;
				do
				{

					bytesRead = inStream.Read(dataBuffer, (int)(toRead-bytesRemainingToRead), (int)bytesRemainingToRead);
					bytesRemainingToRead -= bytesRead;
					remainingBytesInFile -= bytesRead;
				} while (bytesRead < toRead && bytesRemainingToRead > 0);
				
				if(inStream.CanSeek && align512 > 0)
				{
					inStream.Seek(align512, SeekOrigin.Current);
				}
				else
					while(align512 > 0)
					{
						inStream.ReadByte();
						--align512;
					}
					
				buffer = dataBuffer;
				return bytesRead;
			}

			/// <summary>
			/// Check if all bytes in buffer are zeroes
			/// </summary>
			/// <param name="buffer">buffer to check</param>
			/// <returns>true if all bytes are zeroes, otherwise false</returns>
			private static bool IsEmpty(IEnumerable<byte> buffer)
			{
				foreach(byte b in buffer)
				{
					if (b != 0) return false;
				}
				return true;
			}

			/// <summary>
			/// Move internal pointer to a next file in archive.
			/// </summary>
			/// <param name="skipData">Should be true if you want to read a header only, otherwise false</param>
			/// <returns>false on End Of File otherwise true</returns>
			/// <example>
			/// Example:
			/// while(MoveNext())
			/// { 
			///     Read(dataDestStream); 
			/// } </example>
			/// <seealso cref="Read(Stream)"/>
			public bool MoveNext(bool skipData)
			{
				Debug.WriteLine("tar stream position MoveNext in: " + inStream.Position);
				if (remainingBytesInFile > 0)
				{
					if (!skipData)
					{
						throw new TarException(
							"You are trying to change file while not all the data from the previous one was read. If you do want to skip files use skipData parameter set to true.");
					}
					// Skip to the end of file.
					if (inStream.CanSeek)
					{
						long remainer = (remainingBytesInFile%512);
						inStream.Seek(remainingBytesInFile + (512 - (remainer == 0 ? 512 : remainer) ), SeekOrigin.Current);
					}
					else
					{
						byte[] buffer;
						while (Read(out buffer) > 0)
						{
						}
					}
				}

				byte[] bytes = header.GetBytes();
				int headerRead;
				int bytesRemaining = header.HeaderSize;
				do
				{
					headerRead = inStream.Read(bytes, header.HeaderSize - bytesRemaining, bytesRemaining);
					bytesRemaining -= headerRead;
					if (headerRead <= 0 && bytesRemaining > 0)
					{
						throw new TarException("Can not read header");
					}
				} while (bytesRemaining > 0);

				if(IsEmpty(bytes))
				{
					bytesRemaining = header.HeaderSize;
					do
					{
						headerRead = inStream.Read(bytes, header.HeaderSize - bytesRemaining, bytesRemaining);
						bytesRemaining -= headerRead;
						if (headerRead <= 0 && bytesRemaining > 0)
						{
							throw new TarException("Broken archive");
						}
						
					} while (bytesRemaining > 0);
					if (bytesRemaining == 0 && IsEmpty(bytes))
					{
						Debug.WriteLine("tar stream position MoveNext  out(false): " + inStream.Position);
						return false;
					}
					throw new TarException("Broken archive");
				}

				if (header.UpdateHeaderFromBytes())
				{
					throw new TarException("Checksum check failed");
				}

				remainingBytesInFile = header.SizeInBytes;

				Debug.WriteLine("tar stream position MoveNext  out(true): " + inStream.Position);
				return true;
			}
		}
		// Ends here: -->
		
		// TarWriter.cs file: <--
		/// <summary>
		/// Add contents to Tar Archive and put them to the initialised Stream.
		/// </summary>
		public class TarWriter : LegacyTarWriter
		{
			/// <summary>
			/// Initialise the <see cref="TarWriter"/> class using an alive <see cref="System.IO.Stream"/> .
			/// </summary>
			/// <param name="writeStream"></param>
			public TarWriter(Stream writeStream) : base(writeStream)
			{
			}

			protected override void WriteHeader(string name, DateTime lastModificationTime, long count, int userId, int groupId, int mode, EntryType entryType)
			{
				var tarHeader = new UsTarHeader()
				{
					FileName = name,
					LastModification = lastModificationTime,
					SizeInBytes = count,
					UserId = userId,
					UserName = Convert.ToString(userId,8),
					GroupId = groupId,
					GroupName = Convert.ToString(groupId,8),
					Mode = mode,
					EntryType = entryType
				};
				OutStream.Write(tarHeader.GetHeaderValue(), 0, tarHeader.HeaderSize);
			}

			protected virtual void WriteHeader(string name, DateTime lastModificationTime, long count, string userName, string groupName, int mode)
			{
				var tarHeader = new UsTarHeader()
				{
					FileName = name,
					LastModification = lastModificationTime,
					SizeInBytes = count,
					UserId = userName.GetHashCode(),
					UserName = userName,
					GroupId = groupName.GetHashCode(),
					GroupName = groupName,
					Mode = mode
				};
				OutStream.Write(tarHeader.GetHeaderValue(), 0, tarHeader.HeaderSize);
			}

			/// <summary>
			/// Write a new and specified entry to the Tar archive.
			/// </summary>
			/// <param name="name">The file name to get data from.</param>
			/// <param name="dataSizeInBytes">The file's data size. Note: it is a <see cref="System.Int64"/> .</param>
			/// <param name="userName">The User Name (any) that owns this file.</param>
			/// <param name="groupName">The Group Name (any) that this file is grouped to. </param>
			/// <param name="mode">The Compression mode used?</param>
			/// <param name="lastModificationTime">A <see cref="System.DateTime"/> structure that indicates when the file was written in the last time.</param>
			/// <param name="writeDelegate">A custom <see cref="WriteDataDelegate"/> which writes the specified data to the Tar. It is usually used to enable other compression algorithms.</param>
			public virtual void Write(string name, long dataSizeInBytes, string userName, string groupName, int mode, DateTime lastModificationTime, WriteDataDelegate writeDelegate)
			{
				var writer = new DataWriter(OutStream,dataSizeInBytes);
				WriteHeader(name, lastModificationTime, dataSizeInBytes, userName, groupName, mode);
				while(writer.CanWrite)
				{
					writeDelegate(writer);
				}
				AlignTo512(dataSizeInBytes, false);
			}


            /// <summary>
            /// Write a new and specified entry to the Tar archive.
            /// </summary>
            /// <param name="data">The alive Stream to write data to.</param>
            /// <param name="dataSizeInBytes">The <see cref="System.IO.Stream"/> data size. Note: it is a <see cref="System.Int64"/> .</param>
            /// <param name="userId">The User Name (any) that owns this file.</param>
            /// <param name="groupId">The Group Name (any) that this file is grouped to. </param>
            /// <param name="mode">The Compression mode used?</param>
            /// <param name="lastModificationTime">A <see cref="System.DateTime"/> structure that indicates when the file was written in the last time.</param>
			/// <remarks>The data written are uncompressed; which means that you have to implement an algorithm to write the data in compressed format.</remarks>
            public void Write(Stream data, long dataSizeInBytes, string fileName, string userId, string groupId, int mode,
							  DateTime lastModificationTime)
			{
				WriteHeader(fileName,lastModificationTime,dataSizeInBytes,userId, groupId, mode);
				WriteContent(dataSizeInBytes,data);
				AlignTo512(dataSizeInBytes,false);
			}
		}
		// Ends here: -->
		
		
		
		// UsTarHeader.cs file: <--
		
		/// <summary>
		/// UsTar header implementation.
		/// </summary>
		internal class UsTarHeader : TarHeader
		{
			private const string magic = "ustar";
			private const string version = "  ";
			private string groupName;

			private string namePrefix = string.Empty;
			private string userName;

			public override string UserName
			{
				get { return userName.Replace("\0",string.Empty); }
				set
				{
					if (value.Length > 32)
					{
						throw new TarException("user name can not be longer than 32 chars");
					}
					userName = value;
				}
			}

			public override string GroupName
			{
				get { return groupName.Replace("\0",string.Empty); }
				set
				{
					if (value.Length > 32)
					{
						throw new TarException("group name can not be longer than 32 chars");
					}
					groupName = value;
				}
			}

			public override string FileName
			{
				get { return namePrefix.Replace("\0", string.Empty) + base.FileName.Replace("\0", string.Empty); }
				set
				{
					if (value.Length > 100)
					{
						if (value.Length > 255)
						{
							throw new TarException("UsTar Filename can not be longer than 255 chars");
						}
						int position = value.Length - 100;

						// Find first path separator in the remaining 100 chars of the file name
						while (!IsPathSeparator(value[position]))
						{
							++position;
							if (position == value.Length)
							{
								break;
							}
						}
						if (position == value.Length)
							position = value.Length - 100;
						namePrefix = value.Substring(0, position);
						base.FileName = value.Substring(position, value.Length - position);
					}
					else
					{
						base.FileName = value;
					}
				}
			}

			public override bool UpdateHeaderFromBytes()
			{
				byte[] bytes = GetBytes();
				UserName = Encoding.ASCII.GetString(bytes, 0x109, 32);
				GroupName = Encoding.ASCII.GetString(bytes, 0x129, 32);
				namePrefix = Encoding.ASCII.GetString(bytes, 347, 157);
				return base.UpdateHeaderFromBytes();
			}

			internal static bool IsPathSeparator(char ch)
			{
				return (ch == '\\' || ch == '/' || ch == '|'); // All the path separators I ever met.
			}

			public override byte[] GetHeaderValue()
			{
				byte[] header = base.GetHeaderValue();

				Encoding.ASCII.GetBytes(magic).CopyTo(header, 0x101); // Mark header as ustar
				Encoding.ASCII.GetBytes(version).CopyTo(header, 0x106);
				Encoding.ASCII.GetBytes(UserName).CopyTo(header, 0x109);
				Encoding.ASCII.GetBytes(GroupName).CopyTo(header, 0x129);
				Encoding.ASCII.GetBytes(namePrefix).CopyTo(header, 347);

				if (SizeInBytes >= 0x1FFFFFFFF)
				{
					byte[] bytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(SizeInBytes));
					SetMarker(AlignTo12(bytes)).CopyTo(header, 124);
				}

				RecalculateChecksum(header);
				Encoding.ASCII.GetBytes(HeaderChecksumString).CopyTo(header, 148);
				return header;
			}

			private static byte[] SetMarker(byte[] bytes)
			{
				bytes[0] |= 0x80;
				return bytes;
			}

			private static byte[] AlignTo12(byte[] bytes)
			{
				var retVal = new byte[12];
				bytes.CopyTo(retVal, 12 - bytes.Length);
				return retVal;
			}
		}
		// Ends here: -->
		
		// End of tar-cs specification. Ended at line 932. 
	}
	
	namespace Cabinets
	{
        // Cabinet Archive Format Support for .NET C# was found!!!
        // This code is taken from the Wix Toolset Foundation , which has made such a code 
        // to create Windows Installer packages. 
        // The code can create and extract a cabinet file.
        // Easy accessibile methods will be created in the ROOT.Archives namespace.

        /* Original License header: 
			
			// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.
		 
			This code license file is located to the LICENSE file of this Project.
		 */

        /// <summary>
        /// Base exception class for compression operations. Compression libraries should
        /// derive subclass exceptions with more specific error information relevant to the
        /// file format.
        /// </summary>
        [Serializable]
		public class ArchiveException : IOException
		{
			/// <summary>
			/// Creates a new ArchiveException with a specified error message and a reference to the
			/// inner exception that is the cause of this exception.
			/// </summary>
			/// <param name="message">The message that describes the error.</param>
			/// <param name="innerException">The exception that is the cause of the current exception. If the
			/// innerException parameter is not a null reference (Nothing in Visual Basic), the current exception
			/// is raised in a catch block that handles the inner exception.</param>
			public ArchiveException(string message, Exception innerException)
				: base(message, innerException)
			{
			}

			/// <summary>
			/// Creates a new ArchiveException with a specified error message.
			/// </summary>
			/// <param name="message">The message that describes the error.</param>
			public ArchiveException(string message)
				: this(message, null)
			{
			}

			/// <summary>
			/// Creates a new ArchiveException.
			/// </summary>
			public ArchiveException()
				: this(null, null)
			{
			}

			/// <summary>
			/// Initializes a new instance of the ArchiveException class with serialized data.
			/// </summary>
			/// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
			/// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
			protected ArchiveException(SerializationInfo info, StreamingContext context)
				: base(info, context)
			{
			}
		}

		/// <summary>
		/// Abstract object representing a compressed file within an archive;
		/// provides operations for getting the file properties and unpacking
		/// the file.
		/// </summary>
		[Serializable]
		public abstract class ArchiveFileInfo : FileSystemInfo
		{
			private ArchiveInfo archiveInfo;
			private string name;
			private string path;

			private bool initialized;
			private bool exists;
			private int archiveNumber;
			private FileAttributes attributes;
			private DateTime lastWriteTime;
			private long length;

			/// <summary>
			/// Creates a new ArchiveFileInfo object representing a file within
			/// an archive in a specified path.
			/// </summary>
			/// <param name="archiveInfo">An object representing the archive
			/// containing the file.</param>
			/// <param name="filePath">The path to the file within the archive.
			/// Usually, this is a simple file name, but if the archive contains
			/// a directory structure this may include the directory.</param>
			protected ArchiveFileInfo(ArchiveInfo archiveInfo, string filePath)
				: base()
			{
				if (filePath == null)
				{
					throw new ArgumentNullException("filePath");
				}

				this.Archive = archiveInfo;

				this.name = System.IO.Path.GetFileName(filePath);
				this.path = System.IO.Path.GetDirectoryName(filePath);

				this.attributes = FileAttributes.Normal;
				this.lastWriteTime = DateTime.MinValue;
			}

			/// <summary>
			/// Creates a new ArchiveFileInfo object with all parameters specified;
			/// used by subclasses when reading the metadata out of an archive.
			/// </summary>
			/// <param name="filePath">The internal path and name of the file in
			/// the archive.</param>
			/// <param name="archiveNumber">The archive number where the file
			/// starts.</param>
			/// <param name="attributes">The stored attributes of the file.</param>
			/// <param name="lastWriteTime">The stored last write time of the
			/// file.</param>
			/// <param name="length">The uncompressed size of the file.</param>
			protected ArchiveFileInfo(
				string filePath,
				int archiveNumber,
				FileAttributes attributes,
				DateTime lastWriteTime,
				long length)
				: this(null, filePath)
			{
				this.exists = true;
				this.archiveNumber = archiveNumber;
				this.attributes = attributes;
				this.lastWriteTime = lastWriteTime;
				this.length = length;
				this.initialized = true;
			}

			/// <summary>
			/// Initializes a new instance of the ArchiveFileInfo class with
			/// serialized data.
			/// </summary>
			/// <param name="info">The SerializationInfo that holds the serialized
			/// object data about the exception being thrown.</param>
			/// <param name="context">The StreamingContext that contains contextual
			/// information about the source or destination.</param>
			protected ArchiveFileInfo(SerializationInfo info, StreamingContext context)
				: base(info, context)
			{
				this.archiveInfo = (ArchiveInfo) info.GetValue(
					"archiveInfo", typeof(ArchiveInfo));
				this.name = info.GetString("name");
				this.path = info.GetString("path");
				this.initialized = info.GetBoolean("initialized");
				this.exists = info.GetBoolean("exists");
				this.archiveNumber = info.GetInt32("archiveNumber");
				this.attributes = (FileAttributes) info.GetValue(
					"attributes", typeof(FileAttributes));
				this.lastWriteTime = info.GetDateTime("lastWriteTime");
				this.length = info.GetInt64("length");
			}

			/// <summary>
			/// Gets the name of the file.
			/// </summary>
			/// <value>The name of the file, not including any path.</value>
			public override string Name
			{
				get
				{
					return this.name;
				}
			}

			/// <summary>
			/// Gets the internal path of the file in the archive.
			/// </summary>
			/// <value>The internal path of the file in the archive, not including
			/// the file name.</value>
			public string Path
			{
				get
				{
					return this.path;
				}
			}

			/// <summary>
			/// Gets the full path to the file.
			/// </summary>
			/// <value>The full path to the file, including the full path to the
			/// archive, the internal path in the archive, and the file name.</value>
			/// <remarks>
			/// For example, the path <c>"C:\archive.cab\file.txt"</c> refers to
			/// a file "file.txt" inside the archive "archive.cab".
			/// </remarks>
			public override string FullName
			{
				get
				{
					string fullName = System.IO.Path.Combine(this.Path, this.Name);
					
					if (this.Archive != null)
					{
						fullName = System.IO.Path.Combine(this.ArchiveName, fullName);
					}

					return fullName;
				}
			}

			/// <summary>
			/// Gets or sets the archive that contains this file.
			/// </summary>
			/// <value>
			/// The ArchiveInfo instance that retrieved this file information -- this
			/// may be null if the ArchiveFileInfo object was returned directly from
			/// a stream.
			/// </value>
			public ArchiveInfo Archive
			{
				get
				{
					return (ArchiveInfo) this.archiveInfo;
				}

				internal set
				{
					this.archiveInfo = value;

					// protected instance members inherited from FileSystemInfo:
					this.OriginalPath = (value != null ? value.FullName : null);
					this.FullPath = this.OriginalPath;
				}
			}

			/// <summary>
			/// Gets the full path of the archive that contains this file.
			/// </summary>
			/// <value>The full path of the archive that contains this file.</value>
			public string ArchiveName
			{
				get
				{
					return this.Archive != null ? this.Archive.FullName : null;
				}
			}

			/// <summary>
			/// Gets the number of the archive where this file starts.
			/// </summary>
			/// <value>The number of the archive where this file starts.</value>
			/// <remarks>A single archive or the first archive in a chain is
			/// numbered 0.</remarks>
			public int ArchiveNumber
			{
				get
				{
					return this.archiveNumber;
				}
			}

			/// <summary>
			/// Checks if the file exists within the archive.
			/// </summary>
			/// <value>True if the file exists, false otherwise.</value>
			public override bool Exists
			{
				get
				{
					if (!this.initialized)
					{
						this.Refresh();
					}

					return this.exists;
				}
			}

			/// <summary>
			/// Gets the uncompressed size of the file.
			/// </summary>
			/// <value>The uncompressed size of the file in bytes.</value>
			public long Length
			{
				get
				{
					if (!this.initialized)
					{
						this.Refresh();
					}

					return this.length;
				}
			}

			/// <summary>
			/// Gets the attributes of the file.
			/// </summary>
			/// <value>The attributes of the file as stored in the archive.</value>
			public new FileAttributes Attributes
			{
				get
				{
					if (!this.initialized)
					{
						this.Refresh();
					}

					return this.attributes;
				}
			}

			/// <summary>
			/// Gets the last modification time of the file.
			/// </summary>
			/// <value>The last modification time of the file as stored in the
			/// archive.</value>
			public new DateTime LastWriteTime
			{
				get
				{
					if (!this.initialized)
					{
						this.Refresh();
					}

					return this.lastWriteTime;
				}
			}

			/// <summary>
			/// Sets the SerializationInfo with information about the archive.
			/// </summary>
			/// <param name="info">The SerializationInfo that holds the serialized
			/// object data.</param>
			/// <param name="context">The StreamingContext that contains contextual
			/// information about the source or destination.</param>
			public override void GetObjectData(
				SerializationInfo info, StreamingContext context)
			{
				base.GetObjectData(info, context);
				info.AddValue("archiveInfo", this.archiveInfo);
				info.AddValue("name", this.name);
				info.AddValue("path", this.path);
				info.AddValue("initialized", this.initialized);
				info.AddValue("exists", this.exists);
				info.AddValue("archiveNumber", this.archiveNumber);
				info.AddValue("attributes", this.attributes);
				info.AddValue("lastWriteTime", this.lastWriteTime);
				info.AddValue("length", this.length);
			}

			/// <summary>
			/// Gets the full path to the file.
			/// </summary>
			/// <returns>The same as <see cref="FullName"/></returns>
			public override string ToString()
			{
				return this.FullName;
			}

			/// <summary>
			/// Deletes the file. NOT SUPPORTED.
			/// </summary>
			/// <exception cref="NotSupportedException">Files cannot be deleted
			/// from an existing archive.</exception>
			public override void Delete()
			{
				throw new NotSupportedException();
			}

			/// <summary>
			/// Refreshes the attributes and other cached information about the file,
			/// by re-reading the information from the archive.
			/// </summary>
			public new void Refresh()
			{
				base.Refresh();

				if (this.Archive != null)
				{
					string filePath = System.IO.Path.Combine(this.Path, this.Name);
					ArchiveFileInfo updatedFile = this.Archive.GetFile(filePath);
					if (updatedFile == null)
					{
						throw new FileNotFoundException(
								"File not found in archive.", filePath);
					}

					this.Refresh(updatedFile);
				}
			}

			/// <summary>
			/// Extracts the file.
			/// </summary>
			/// <param name="destFileName">The destination path where the file
			/// will be extracted.</param>
			[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "dest")]
			public void CopyTo(string destFileName)
			{
				this.CopyTo(destFileName, false);
			}

			/// <summary>
			/// Extracts the file, optionally overwriting any existing file.
			/// </summary>
			/// <param name="destFileName">The destination path where the file
			/// will be extracted.</param>
			/// <param name="overwrite">If true, <paramref name="destFileName"/>
			/// will be overwritten if it exists.</param>
			/// <exception cref="IOException"><paramref name="overwrite"/> is false
			/// and <paramref name="destFileName"/> exists.</exception>
			[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "dest")]
			public void CopyTo(string destFileName, bool overwrite)
			{
				if (destFileName == null)
				{
					throw new ArgumentNullException("destFileName");
				}

				if (!overwrite && File.Exists(destFileName))
				{
					throw new IOException();
				}

				if (this.Archive == null)
				{
					throw new InvalidOperationException();
				}

				this.Archive.UnpackFile(
					System.IO.Path.Combine(this.Path, this.Name), destFileName);
			}

			/// <summary>
			/// Opens the archive file for reading without actually extracting the
			/// file to disk.
			/// </summary>
			/// <returns>
			/// A stream for reading directly from the packed file. Like any stream
			/// this should be closed/disposed as soon as it is no longer needed.
			/// </returns>
			public Stream OpenRead()
			{
				return this.Archive.OpenRead(System.IO.Path.Combine(this.Path, this.Name));
			}

			/// <summary>
			/// Opens the archive file reading text with UTF-8 encoding without
			/// actually extracting the file to disk.
			/// </summary>
			/// <returns>
			/// A reader for reading text directly from the packed file. Like any reader
			/// this should be closed/disposed as soon as it is no longer needed.
			/// </returns>
			/// <remarks>
			/// To open an archived text file with different encoding, use the
			/// <see cref="OpenRead" /> method and pass the returned stream to one of
			/// the <see cref="StreamReader" /> constructor overloads.
			/// </remarks>
			public StreamReader OpenText()
			{
				return this.Archive.OpenText(System.IO.Path.Combine(this.Path, this.Name));
			}

			/// <summary>
			/// Refreshes the information in this object with new data retrieved
			/// from an archive.
			/// </summary>
			/// <param name="newFileInfo">Fresh instance for the same file just
			/// read from the archive.</param>
			/// <remarks>
			/// Subclasses may override this method to refresh sublcass fields.
			/// However they should always call the base implementation first.
			/// </remarks>
			protected virtual void Refresh(ArchiveFileInfo newFileInfo)
			{
				this.exists = newFileInfo.exists;
				this.length = newFileInfo.length;
				this.attributes = newFileInfo.attributes;
				this.lastWriteTime = newFileInfo.lastWriteTime;
			}
		}
		
		/// <summary>
		/// Provides a basic implementation of the archive pack and unpack stream context
		/// interfaces, based on a list of archive files, a default directory, and an
		/// optional mapping from internal to external file paths.
		/// </summary>
		/// <remarks>
		/// This class can also handle creating or extracting chained archive packages.
		/// </remarks>
		public class ArchiveFileStreamContext
			: IPackStreamContext, IUnpackStreamContext
		{
			private IList<string> archiveFiles;
			private string directory;
			private IDictionary<string, string> files;
			private bool extractOnlyNewerFiles;
			private bool enableOffsetOpen;

			#region Constructors

			/// <summary>
			/// Creates a new ArchiveFileStreamContext with a archive file and
			/// no default directory or file mapping.
			/// </summary>
			/// <param name="archiveFile">The path to a archive file that will be
			/// created or extracted.</param>
			public ArchiveFileStreamContext(string archiveFile)
				: this(archiveFile, null, null)
			{
			}

			/// <summary>
			/// Creates a new ArchiveFileStreamContext with a archive file, default
			/// directory and mapping from internal to external file paths.
			/// </summary>
			/// <param name="archiveFile">The path to a archive file that will be
			/// created or extracted.</param>
			/// <param name="directory">The default root directory where files will be
			/// located, optional.</param>
			/// <param name="files">A mapping from internal file paths to external file
			/// paths, optional.</param>
			/// <remarks>
			/// If the mapping is not null and a file is not included in the mapping,
			/// the file will be skipped.
			/// <para>If the external path in the mapping is a simple file name or
			/// relative file path, it will be concatenated onto the default directory,
			/// if one was specified.</para>
			/// <para>For more about how the default directory and files mapping are
			/// used, see <see cref="OpenFileReadStream"/> and
			/// <see cref="OpenFileWriteStream"/>.</para>
			/// </remarks>
			public ArchiveFileStreamContext(
				string archiveFile,
				string directory,
				IDictionary<string, string> files)
				: this(new string[] { archiveFile }, directory, files)
			{
				if (archiveFile == null)
				{
					throw new ArgumentNullException("archiveFile");
				}
			}

			/// <summary>
			/// Creates a new ArchiveFileStreamContext with a list of archive files,
			/// a default directory and a mapping from internal to external file paths.
			/// </summary>
			/// <param name="archiveFiles">A list of paths to archive files that will be
			/// created or extracted.</param>
			/// <param name="directory">The default root directory where files will be
			/// located, optional.</param>
			/// <param name="files">A mapping from internal file paths to external file
			/// paths, optional.</param>
			/// <remarks>
			/// When creating chained archives, the <paramref name="archiveFiles"/> list
			/// should include at least enough archives to handle the entire set of
			/// input files, based on the maximum archive size that is passed to the
			/// <see cref="CompressionEngine"/>.<see
			/// cref="CompressionEngine.Pack(IPackStreamContext,IEnumerable&lt;string&gt;,long)"/>.
			/// <para>If the mapping is not null and a file is not included in the mapping,
			/// the file will be skipped.</para>
			/// <para>If the external path in the mapping is a simple file name or
			/// relative file path, it will be concatenated onto the default directory,
			/// if one was specified.</para>
			/// <para>For more about how the default directory and files mapping are used,
			/// see <see cref="OpenFileReadStream"/> and
			/// <see cref="OpenFileWriteStream"/>.</para>
			/// </remarks>
			public ArchiveFileStreamContext(
				IList<string> archiveFiles,
				string directory,
				IDictionary<string, string> files)
			{
				if (archiveFiles == null || archiveFiles.Count == 0)
				{
					throw new ArgumentNullException("archiveFiles");
				}

				this.archiveFiles = archiveFiles;
				this.directory = directory;
				this.files = files;
			}

			#endregion

			#region Properties

			/// <summary>
			/// Gets or sets the list of archive files that are created or extracted.
			/// </summary>
			/// <value>The list of archive files that are created or extracted.</value>
			public IList<string> ArchiveFiles
			{
				get
				{
					return this.archiveFiles;
				}
			}

			/// <summary>
			/// Gets or sets the default root directory where files are located.
			/// </summary>
			/// <value>The default root directory where files are located.</value>
			/// <remarks>
			/// For details about how the default directory is used,
			/// see <see cref="OpenFileReadStream"/> and <see cref="OpenFileWriteStream"/>.
			/// </remarks>
			public string Directory
			{
				get
				{
					return this.directory;
				}
			}

			/// <summary>
			/// Gets or sets the mapping from internal file paths to external file paths.
			/// </summary>
			/// <value>A mapping from internal file paths to external file paths.</value>
			/// <remarks>
			/// For details about how the files mapping is used,
			/// see <see cref="OpenFileReadStream"/> and <see cref="OpenFileWriteStream"/>.
			/// </remarks>
			public IDictionary<string, string> Files
			{
				get
				{
					return this.files;
				}
			}

			/// <summary>
			/// Gets or sets a flag that can prevent extracted files from overwriting
			/// newer files that already exist.
			/// </summary>
			/// <value>True to prevent overwriting newer files that already exist
			/// during extraction; false to always extract from the archive regardless
			/// of existing files.</value>
			public bool ExtractOnlyNewerFiles
			{
				get
				{
					return this.extractOnlyNewerFiles;
				}

				set
				{
					this.extractOnlyNewerFiles = value;
				}
			}

			/// <summary>
			/// Gets or sets a flag that enables creating or extracting an archive
			/// at an offset within an existing file. (This is typically used to open
			/// archive-based self-extracting packages.)
			/// </summary>
			/// <value>True to search an existing package file for an archive offset
			/// or the end of the file;/ false to always create or open a plain
			/// archive file.</value>
			public bool EnableOffsetOpen
			{
				get
				{
					return this.enableOffsetOpen;
				}

				set
				{
					this.enableOffsetOpen = value;
				}
			}

			#endregion

			#region IPackStreamContext Members

			/// <summary>
			/// Gets the name of the archive with a specified number.
			/// </summary>
			/// <param name="archiveNumber">The 0-based index of the archive within
			/// the chain.</param>
			/// <returns>The name of the requested archive. May be an empty string
			/// for non-chained archives, but may never be null.</returns>
			/// <remarks>This method returns the file name of the archive from the
			/// <see cref="archiveFiles"/> list with the specified index, or an empty
			/// string if the archive number is outside the bounds of the list. The
			/// file name should not include any directory path.</remarks>
			public virtual string GetArchiveName(int archiveNumber)
			{
				if (archiveNumber < this.archiveFiles.Count)
				{
					return Path.GetFileName(this.archiveFiles[archiveNumber]);
				}

				return String.Empty;
			}

			/// <summary>
			/// Opens a stream for writing an archive.
			/// </summary>
			/// <param name="archiveNumber">The 0-based index of the archive within
			/// the chain.</param>
			/// <param name="archiveName">The name of the archive that was returned
			/// by <see cref="GetArchiveName"/>.</param>
			/// <param name="truncate">True if the stream should be truncated when
			/// opened (if it already exists); false if an existing stream is being
			/// re-opened for writing additional data.</param>
			/// <param name="compressionEngine">Instance of the compression engine
			/// doing the operations.</param>
			/// <returns>A writable Stream where the compressed archive bytes will be
			/// written, or null to cancel the archive creation.</returns>
			/// <remarks>
			/// This method opens the file from the <see cref="ArchiveFiles"/> list
			/// with the specified index. If the archive number is outside the bounds
			/// of the list, this method returns null.
			/// <para>If the <see cref="EnableOffsetOpen"/> flag is set, this method
			/// will seek to the start of any existing archive in the file, or to the
			/// end of the file if the existing file is not an archive.</para>
			/// </remarks>
			public virtual Stream OpenArchiveWriteStream(
				int archiveNumber,
				string archiveName,
				bool truncate,
				CompressionEngine compressionEngine)
			{
				if (archiveNumber >= this.archiveFiles.Count)
				{
					return null;
				}

				if (String.IsNullOrEmpty(archiveName))
				{
					throw new ArgumentNullException("archiveName");
				}

				// All archives must be in the same directory,
				// so always use the directory from the first archive.
				string archiveFile = Path.Combine(
					Path.GetDirectoryName(this.archiveFiles[0]), archiveName);
				Stream stream = File.Open(
					archiveFile,
					(truncate ? FileMode.OpenOrCreate : FileMode.Open),
					FileAccess.ReadWrite);

				if (this.enableOffsetOpen)
				{
					long offset = compressionEngine.FindArchiveOffset(
						new DuplicateStream(stream));

					// If this is not an archive file, append the archive to it.
					if (offset < 0)
					{
						offset = stream.Length;
					}

					if (offset > 0)
					{
						stream = new OffsetStream(stream, offset);
					}

					stream.Seek(0, SeekOrigin.Begin);
				}

				if (truncate)
				{
					// Truncate the stream, in case a larger old archive starts here.
					stream.SetLength(0);
				}
				
				return stream;
			}

			/// <summary>
			/// Closes a stream where an archive package was written.
			/// </summary>
			/// <param name="archiveNumber">The 0-based index of the archive within
			/// the chain.</param>
			/// <param name="archiveName">The name of the archive that was previously
			/// returned by <see cref="GetArchiveName"/>.</param>
			/// <param name="stream">A stream that was previously returned by
			/// <see cref="OpenArchiveWriteStream"/> and is now ready to be closed.</param>
			public virtual void CloseArchiveWriteStream(
				int archiveNumber,
				string archiveName,
				Stream stream)
			{
				if (stream != null)
				{
					stream.Close();

					FileStream fileStream = stream as FileStream;
					if (fileStream != null)
					{
						string streamFile = fileStream.Name;
						if (!String.IsNullOrEmpty(archiveName) &&
							archiveName != Path.GetFileName(streamFile))
						{
							string archiveFile = Path.Combine(
								Path.GetDirectoryName(this.archiveFiles[0]), archiveName);
							if (File.Exists(archiveFile))
							{
								File.Delete(archiveFile);
							}
							File.Move(streamFile, archiveFile);
						}
					}
				}
			}

			/// <summary>
			/// Opens a stream to read a file that is to be included in an archive.
			/// </summary>
			/// <param name="path">The path of the file within the archive.</param>
			/// <param name="attributes">The returned attributes of the opened file,
			/// to be stored in the archive.</param>
			/// <param name="lastWriteTime">The returned last-modified time of the
			/// opened file, to be stored in the archive.</param>
			/// <returns>A readable Stream where the file bytes will be read from
			/// before they are compressed, or null to skip inclusion of the file and
			/// continue to the next file.</returns>
			/// <remarks>
			/// This method opens a file using the following logic:
			/// <list>
			/// <item>If the <see cref="Directory"/> and the <see cref="Files"/> mapping
			/// are both null, the path is treated as relative to the current directory,
			/// and that file is opened.</item>
			/// <item>If the <see cref="Directory"/> is not null but the <see cref="Files"/>
			/// mapping is null, the path is treated as relative to that directory, and
			/// that file is opened.</item>
			/// <item>If the <see cref="Directory"/> is null but the <see cref="Files"/>
			/// mapping is not null, the path parameter is used as a key into the mapping,
			/// and the resulting value is the file path that is opened, relative to the
			/// current directory (or it may be an absolute path). If no mapping exists,
			/// the file is skipped.</item>
			/// <item>If both the <see cref="Directory"/> and the <see cref="Files"/>
			/// mapping are specified, the path parameter is used as a key into the
			/// mapping, and the resulting value is the file path that is opened, relative
			/// to the specified directory (or it may be an absolute path). If no mapping
			/// exists, the file is skipped.</item>
			/// </list>
			/// </remarks>
			public virtual Stream OpenFileReadStream(
				string path, out FileAttributes attributes, out DateTime lastWriteTime)
			{
				string filePath = this.TranslateFilePath(path);

				if (filePath == null)
				{
					attributes = FileAttributes.Normal;
					lastWriteTime = DateTime.Now;
					return null;
				}

				attributes = File.GetAttributes(filePath);
				lastWriteTime = File.GetLastWriteTime(filePath);
				return File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
			}

			/// <summary>
			/// Closes a stream that has been used to read a file.
			/// </summary>
			/// <param name="path">The path of the file within the archive; the same as
			/// the path provided when the stream was opened.</param>
			/// <param name="stream">A stream that was previously returned by
			/// <see cref="OpenFileReadStream"/> and is now ready to be closed.</param>
			public virtual void CloseFileReadStream(string path, Stream stream)
			{
				if (stream != null)
				{
					stream.Close();
				}
			}

			/// <summary>
			/// Gets extended parameter information specific to the compression format
			/// being used.
			/// </summary>
			/// <param name="optionName">Name of the option being requested.</param>
			/// <param name="parameters">Parameters for the option; for per-file options,
			/// the first parameter is typically the internal file path.</param>
			/// <returns>Option value, or null to use the default behavior.</returns>
			/// <remarks>
			/// This implementation does not handle any options. Subclasses may override
			/// this method to allow for non-default behavior.
			/// </remarks>
			public virtual object GetOption(string optionName, object[] parameters)
			{
				return null;
			}

			#endregion

			#region IUnpackStreamContext Members

			/// <summary>
			/// Opens the archive stream for reading.
			/// </summary>
			/// <param name="archiveNumber">The zero-based index of the archive to
			/// open.</param>
			/// <param name="archiveName">The name of the archive being opened.</param>
			/// <param name="compressionEngine">Instance of the compression engine
			/// doing the operations.</param>
			/// <returns>A stream from which archive bytes are read, or null to cancel
			/// extraction of the archive.</returns>
			/// <remarks>
			/// This method opens the file from the <see cref="ArchiveFiles"/> list with
			/// the specified index. If the archive number is outside the bounds of the
			/// list, this method returns null.
			/// <para>If the <see cref="EnableOffsetOpen"/> flag is set, this method will
			/// seek to the start of any existing archive in the file, or to the end of
			/// the file if the existing file is not an archive.</para>
			/// </remarks>
			public virtual Stream OpenArchiveReadStream(
				int archiveNumber, string archiveName, CompressionEngine compressionEngine)
			{
				if (archiveNumber >= this.archiveFiles.Count)
				{
					return null;
				}

				string archiveFile = this.archiveFiles[archiveNumber];
				Stream stream = File.Open(
					archiveFile, FileMode.Open, FileAccess.Read, FileShare.Read);

				if (this.enableOffsetOpen)
				{
					long offset = compressionEngine.FindArchiveOffset(
						new DuplicateStream(stream));
					if (offset > 0)
					{
						stream = new OffsetStream(stream, offset);
					}
					else
					{
						stream.Seek(0, SeekOrigin.Begin);
					}
				}

				return stream;
			}

			/// <summary>
			/// Closes a stream where an archive was read.
			/// </summary>
			/// <param name="archiveNumber">The archive number of the stream
			/// to close.</param>
			/// <param name="archiveName">The name of the archive being closed.</param>
			/// <param name="stream">The stream that was previously returned by
			/// <see cref="OpenArchiveReadStream"/> and is now ready to be closed.</param>
			public virtual void CloseArchiveReadStream(
				int archiveNumber, string archiveName, Stream stream)
			{
				if (stream != null)
				{
					stream.Close();
				}
			}

			/// <summary>
			/// Opens a stream for writing extracted file bytes.
			/// </summary>
			/// <param name="path">The path of the file within the archive.</param>
			/// <param name="fileSize">The uncompressed size of the file to be
			/// extracted.</param>
			/// <param name="lastWriteTime">The last write time of the file to be
			/// extracted.</param>
			/// <returns>A stream where extracted file bytes are to be written, or null
			/// to skip extraction of the file and continue to the next file.</returns>
			/// <remarks>
			/// This method opens a file using the following logic:
			/// <list>
			/// <item>If the <see cref="Directory"/> and the <see cref="Files"/> mapping
			/// are both null, the path is treated as relative to the current directory,
			/// and that file is opened.</item>
			/// <item>If the <see cref="Directory"/> is not null but the <see cref="Files"/>
			/// mapping is null, the path is treated as relative to that directory, and
			/// that file is opened.</item>
			/// <item>If the <see cref="Directory"/> is null but the <see cref="Files"/>
			/// mapping is not null, the path parameter is used as a key into the mapping,
			/// and the resulting value is the file path that is opened, relative to the
			/// current directory (or it may be an absolute path). If no mapping exists,
			/// the file is skipped.</item>
			/// <item>If both the <see cref="Directory"/> and the <see cref="Files"/>
			/// mapping are specified, the path parameter is used as a key into the
			/// mapping, and the resulting value is the file path that is opened,
			/// relative to the specified directory (or it may be an absolute path).
			/// If no mapping exists, the file is skipped.</item>
			/// </list>
			/// <para>If the <see cref="ExtractOnlyNewerFiles"/> flag is set, the file
			/// is skipped if a file currently exists in the same path with an equal
			/// or newer write time.</para>
			/// </remarks>
			public virtual Stream OpenFileWriteStream(
				string path,
				long fileSize,
				DateTime lastWriteTime)
			{
				string filePath = this.TranslateFilePath(path);

				if (filePath == null)
				{
					return null;
				}

				FileInfo file = new FileInfo(filePath);
				if (file.Exists)
				{
					if (this.extractOnlyNewerFiles && lastWriteTime != DateTime.MinValue)
					{
						if (file.LastWriteTime >= lastWriteTime)
						{
							return null;
						}
					}

					// Clear attributes that will prevent overwriting the file.
					// (The final attributes will be set after the file is unpacked.)
					FileAttributes attributesToClear =
						FileAttributes.ReadOnly | FileAttributes.Hidden | FileAttributes.System;
					if ((file.Attributes & attributesToClear) != 0)
					{
						file.Attributes &= ~attributesToClear;
					}
				}

				if (!file.Directory.Exists)
				{
					file.Directory.Create();
				}

				return File.Open(
					filePath, FileMode.Create, FileAccess.Write, FileShare.None);
			}

			/// <summary>
			/// Closes a stream where an extracted file was written.
			/// </summary>
			/// <param name="path">The path of the file within the archive.</param>
			/// <param name="stream">The stream that was previously returned by
			/// <see cref="OpenFileWriteStream"/> and is now ready to be closed.</param>
			/// <param name="attributes">The attributes of the extracted file.</param>
			/// <param name="lastWriteTime">The last write time of the file.</param>
			/// <remarks>
			/// After closing the extracted file stream, this method applies the date
			/// and attributes to that file.
			/// </remarks>
			public virtual void CloseFileWriteStream(
				string path,
				Stream stream,
				FileAttributes attributes,
				DateTime lastWriteTime)
			{
				if (stream != null)
				{
					stream.Close();
				}

				string filePath = this.TranslateFilePath(path);
				if (filePath != null)
				{
					FileInfo file = new FileInfo(filePath);

					if (lastWriteTime != DateTime.MinValue)
					{
						try
						{
							file.LastWriteTime = lastWriteTime;
						}
						catch (ArgumentException)
						{
						}
						catch (IOException)
						{
						}
					}

					try
					{
						file.Attributes = attributes;
					}
					catch (IOException)
					{
					}
				}
			}

			#endregion

			#region Private utility methods

			/// <summary>
			/// Translates an internal file path to an external file path using the
			/// <see cref="Directory"/> and the <see cref="Files"/> mapping, according to
			/// rules documented in <see cref="OpenFileReadStream"/> and
			/// <see cref="OpenFileWriteStream"/>.
			/// </summary>
			/// <param name="path">The path of the file with the archive.</param>
			/// <returns>The external path of the file, or null if there is no
			/// valid translation.</returns>
			private string TranslateFilePath(string path)
			{
				string filePath;
				if (this.files != null)
				{
					filePath = this.files[path];
				}
				else
				{
					this.ValidateArchivePath(path);

					filePath = path;
				}

				if (filePath != null)
				{
					if (this.directory != null)
					{
						filePath = Path.Combine(this.directory, filePath);
					}
				}

				return filePath;
			}

			private void ValidateArchivePath(string filePath)
			{
				string basePath = Path.GetFullPath(String.IsNullOrEmpty(this.directory) ? Environment.CurrentDirectory : this.directory);
				string path = Path.GetFullPath(Path.Combine(basePath, filePath));
				if (!path.StartsWith(basePath, StringComparison.InvariantCultureIgnoreCase))
				{
					throw new InvalidDataException("Archive cannot contain files with absolute or traversal paths.");
				}
			}

			#endregion
		}

		/// <summary>
		/// Abstract object representing a compressed archive on disk;
		/// provides access to file-based operations on the archive.
		/// </summary>
		[Serializable]
		public abstract class ArchiveInfo : FileSystemInfo
		{
			/// <summary>
			/// Creates a new ArchiveInfo object representing an archive in a
			/// specified path.
			/// </summary>
			/// <param name="path">The path to the archive. When creating an archive,
			/// this file does not necessarily exist yet.</param>
			protected ArchiveInfo(string path) : base()
			{
				if (path == null)
				{
					throw new ArgumentNullException("path");
				}

				// protected instance members inherited from FileSystemInfo:
				this.OriginalPath = path;
				this.FullPath = Path.GetFullPath(path);
			}

			/// <summary>
			/// Initializes a new instance of the ArchiveInfo class with serialized data.
			/// </summary>
			/// <param name="info">The SerializationInfo that holds the serialized object
			/// data about the exception being thrown.</param>
			/// <param name="context">The StreamingContext that contains contextual
			/// information about the source or destination.</param>
			protected ArchiveInfo(SerializationInfo info, StreamingContext context)
				: base(info, context)
			{
			}

			/// <summary>
			/// Gets the directory that contains the archive.
			/// </summary>
			/// <value>A DirectoryInfo object representing the parent directory of the
			/// archive.</value>
			public DirectoryInfo Directory
			{
				get
				{
					return new DirectoryInfo(Path.GetDirectoryName(this.FullName));
				}
			}

			/// <summary>
			/// Gets the full path of the directory that contains the archive.
			/// </summary>
			/// <value>The full path of the directory that contains the archive.</value>
			public string DirectoryName
			{
				get
				{
					return Path.GetDirectoryName(this.FullName);
				}
			}

			/// <summary>
			/// Gets the size of the archive.
			/// </summary>
			/// <value>The size of the archive in bytes.</value>
			public long Length
			{
				get
				{
					return new FileInfo(this.FullName).Length;
				}
			}

			/// <summary>
			/// Gets the file name of the archive.
			/// </summary>
			/// <value>The file name of the archive, not including any path.</value>
			public override string Name
			{
				get
				{
					return Path.GetFileName(this.FullName);
				}
			}

			/// <summary>
			/// Checks if the archive exists.
			/// </summary>
			/// <value>True if the archive exists; else false.</value>
			public override bool Exists
			{
				get
				{
					return File.Exists(this.FullName);
				}
			}

			/// <summary>
			/// Gets the full path of the archive.
			/// </summary>
			/// <returns>The full path of the archive.</returns>
			public override string ToString()
			{
				return this.FullName;
			}

			/// <summary>
			/// Deletes the archive.
			/// </summary>
			public override void Delete()
			{
				File.Delete(this.FullName);
			}

			/// <summary>
			/// Copies an existing archive to another location.
			/// </summary>
			/// <param name="destFileName">The destination file path.</param>
			[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "dest")]
			public void CopyTo(string destFileName)
			{
				File.Copy(this.FullName, destFileName);
			}

			/// <summary>
			/// Copies an existing archive to another location, optionally
			/// overwriting the destination file.
			/// </summary>
			/// <param name="destFileName">The destination file path.</param>
			/// <param name="overwrite">If true, the destination file will be
			/// overwritten if it exists.</param>
			[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "dest")]
			public void CopyTo(string destFileName, bool overwrite)
			{
				File.Copy(this.FullName, destFileName, overwrite);
			}
			
			/// <summary>
			/// Moves an existing archive to another location.
			/// </summary>
			/// <param name="destFileName">The destination file path.</param>
			[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "dest")]
			public void MoveTo(string destFileName)
			{
				File.Move(this.FullName, destFileName);
				this.FullPath = Path.GetFullPath(destFileName);
			}

			/// <summary>
			/// Checks if the archive contains a valid archive header.
			/// </summary>
			/// <returns>True if the file is a valid archive; false otherwise.</returns>
			public bool IsValid()
			{
				using (Stream stream = File.OpenRead(this.FullName))
				{
					using (CompressionEngine compressionEngine = this.CreateCompressionEngine())
					{
						return compressionEngine.FindArchiveOffset(stream) >= 0;
					}
				}
			}

			/// <summary>
			/// Gets information about the files contained in the archive.
			/// </summary>
			/// <returns>A list of <see cref="ArchiveFileInfo"/> objects, each
			/// containing information about a file in the archive.</returns>
			public IList<ArchiveFileInfo> GetFiles()
			{
				return this.InternalGetFiles((Predicate<string>) null);
			}

			/// <summary>
			/// Gets information about the certain files contained in the archive file.
			/// </summary>
			/// <param name="searchPattern">The search string, such as
			/// &quot;*.txt&quot;.</param>
			/// <returns>A list of <see cref="ArchiveFileInfo"/> objects, each containing
			/// information about a file in the archive.</returns>
			public IList<ArchiveFileInfo> GetFiles(string searchPattern)
			{
				if (searchPattern == null)
				{
					throw new ArgumentNullException("searchPattern");
				}

				string regexPattern = String.Format(
					CultureInfo.InvariantCulture,
					"^{0}$",
					Regex.Escape(searchPattern).Replace("\\*", ".*").Replace("\\?", "."));
				Regex regex = new Regex(
						regexPattern,
						RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

				return this.InternalGetFiles(
					delegate(string match)
					{
						return regex.IsMatch(match);
					});
			}

			/// <summary>
			/// Extracts all files from an archive to a destination directory.
			/// </summary>
			/// <param name="destDirectory">Directory where the files are to be
			/// extracted.</param>
			[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "dest")]
			public void Unpack(string destDirectory)
			{
				this.Unpack(destDirectory, null);
			}

			/// <summary>
			/// Extracts all files from an archive to a destination directory,
			/// optionally extracting only newer files.
			/// </summary>
			/// <param name="destDirectory">Directory where the files are to be
			/// extracted.</param>
			/// <param name="progressHandler">Handler for receiving progress
			/// information; this may be null if progress is not desired.</param>
			[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "dest")]
			public void Unpack(
				string destDirectory,
				EventHandler<ArchiveProgressEventArgs> progressHandler)
			{
				using (CompressionEngine compressionEngine = this.CreateCompressionEngine())
				{
					compressionEngine.Progress += progressHandler;
					ArchiveFileStreamContext streamContext =
						new ArchiveFileStreamContext(this.FullName, destDirectory, null);
					streamContext.EnableOffsetOpen = true;
					compressionEngine.Unpack(streamContext, null);
				}
			}

			/// <summary>
			/// Extracts a single file from the archive.
			/// </summary>
			/// <param name="fileName">The name of the file in the archive. Also
			/// includes the internal path of the file, if any. File name matching
			/// is case-insensitive.</param>
			/// <param name="destFileName">The path where the file is to be
			/// extracted on disk.</param>
			/// <remarks>If <paramref name="destFileName"/> already exists,
			/// it will be overwritten.</remarks>
			[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "dest")]
			public void UnpackFile(string fileName, string destFileName)
			{
				if (fileName == null)
				{
					throw new ArgumentNullException("fileName");
				}

				if (destFileName == null)
				{
					throw new ArgumentNullException("destFileName");
				}

				this.UnpackFiles(
					new string[] { fileName },
					null,
					new string[] { destFileName });
			}

			/// <summary>
			/// Extracts multiple files from the archive.
			/// </summary>
			/// <param name="fileNames">The names of the files in the archive.
			/// Each name includes the internal path of the file, if any. File name
			/// matching is case-insensitive.</param>
			/// <param name="destDirectory">This parameter may be null, but if
			/// specified it is the root directory for any relative paths in
			/// <paramref name="destFileNames"/>.</param>
			/// <param name="destFileNames">The paths where the files are to be
			/// extracted on disk. If this parameter is null, the files will be
			/// extracted with the names from the archive.</param>
			/// <remarks>
			/// If any extracted files already exist on disk, they will be overwritten.
			/// <p>The <paramref name="destDirectory"/> and
			/// <paramref name="destFileNames"/> parameters cannot both be null.</p>
			/// </remarks>
			[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "dest")]
			public void UnpackFiles(
				IList<string> fileNames,
				string destDirectory,
				IList<string> destFileNames)
			{
				this.UnpackFiles(fileNames, destDirectory, destFileNames, null);
			}

			/// <summary>
			/// Extracts multiple files from the archive, optionally extracting
			/// only newer files.
			/// </summary>
			/// <param name="fileNames">The names of the files in the archive.
			/// Each name includes the internal path of the file, if any. File name
			/// matching is case-insensitive.</param>
			/// <param name="destDirectory">This parameter may be null, but if
			/// specified it is the root directory for any relative paths in
			/// <paramref name="destFileNames"/>.</param>
			/// <param name="destFileNames">The paths where the files are to be
			/// extracted on disk. If this parameter is null, the files will be
			/// extracted with the names from the archive.</param>
			/// <param name="progressHandler">Handler for receiving progress information;
			/// this may be null if progress is not desired.</param>
			/// <remarks>
			/// If any extracted files already exist on disk, they will be overwritten.
			/// <p>The <paramref name="destDirectory"/> and
			/// <paramref name="destFileNames"/> parameters cannot both be null.</p>
			/// </remarks>
			[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "dest")]
			public void UnpackFiles(
				IList<string> fileNames,
				string destDirectory,
				IList<string> destFileNames,
				EventHandler<ArchiveProgressEventArgs> progressHandler)
			{
				if (fileNames == null)
				{
					throw new ArgumentNullException("fileNames");
				}

				if (destFileNames == null)
				{
					if (destDirectory == null)
					{
						throw new ArgumentNullException("destFileNames");
					}

					destFileNames = fileNames;
				}

				if (destFileNames.Count != fileNames.Count)
				{
					throw new ArgumentOutOfRangeException("destFileNames");
				}

				IDictionary<string, string> files =
					ArchiveInfo.CreateStringDictionary(fileNames, destFileNames);
				this.UnpackFileSet(files, destDirectory, progressHandler);
			}

			/// <summary>
			/// Extracts multiple files from the archive.
			/// </summary>
			/// <param name="fileNames">A mapping from internal file paths to
			/// external file paths. Case-senstivity when matching internal paths
			/// depends on the IDictionary implementation.</param>
			/// <param name="destDirectory">This parameter may be null, but if
			/// specified it is the root directory for any relative external paths
			/// in <paramref name="fileNames"/>.</param>
			/// <remarks>
			/// If any extracted files already exist on disk, they will be overwritten.
			/// </remarks>
			[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "dest")]
			public void UnpackFileSet(
				IDictionary<string, string> fileNames,
				string destDirectory)
			{
				this.UnpackFileSet(fileNames, destDirectory, null);
			}

			/// <summary>
			/// Extracts multiple files from the archive.
			/// </summary>
			/// <param name="fileNames">A mapping from internal file paths to
			/// external file paths. Case-senstivity when matching internal
			/// paths depends on the IDictionary implementation.</param>
			/// <param name="destDirectory">This parameter may be null, but if
			/// specified it is the root directory for any relative external
			/// paths in <paramref name="fileNames"/>.</param>
			/// <param name="progressHandler">Handler for receiving progress
			/// information; this may be null if progress is not desired.</param>
			/// <remarks>
			/// If any extracted files already exist on disk, they will be overwritten.
			/// </remarks>
			[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "dest")]
			public void UnpackFileSet(
				IDictionary<string, string> fileNames,
				string destDirectory,
				EventHandler<ArchiveProgressEventArgs> progressHandler)
			{
				if (fileNames == null)
				{
					throw new ArgumentNullException("fileNames");
				}

				using (CompressionEngine compressionEngine = this.CreateCompressionEngine())
				{
					compressionEngine.Progress += progressHandler;
					ArchiveFileStreamContext streamContext =
						new ArchiveFileStreamContext(this.FullName, destDirectory, fileNames);
					streamContext.EnableOffsetOpen = true;
					compressionEngine.Unpack(
						streamContext,
						delegate(string match)
						{
							return fileNames.ContainsKey(match);
						});
				}
			}

			/// <summary>
			/// Opens a file inside the archive for reading without actually
			/// extracting the file to disk.
			/// </summary>
			/// <param name="fileName">The name of the file in the archive. Also
			/// includes the internal path of the file, if any. File name matching
			/// is case-insensitive.</param>
			/// <returns>
			/// A stream for reading directly from the packed file. Like any stream
			/// this should be closed/disposed as soon as it is no longer needed.
			/// </returns>
			public Stream OpenRead(string fileName)
			{
				Stream archiveStream = File.OpenRead(this.FullName);
				CompressionEngine compressionEngine = this.CreateCompressionEngine();
				Stream fileStream = compressionEngine.Unpack(archiveStream, fileName);

				// Attach the archiveStream and compressionEngine to the
				// fileStream so they get disposed when the fileStream is disposed.
				return new CargoStream(fileStream, archiveStream, compressionEngine);
			}

			/// <summary>
			/// Opens a file inside the archive for reading text with UTF-8 encoding
			/// without actually extracting the file to disk.
			/// </summary>
			/// <param name="fileName">The name of the file in the archive. Also
			/// includes the internal path of the file, if any. File name matching
			/// is case-insensitive.</param>
			/// <returns>
			/// A reader for reading text directly from the packed file. Like any reader
			/// this should be closed/disposed as soon as it is no longer needed.
			/// </returns>
			/// <remarks>
			/// To open an archived text file with different encoding, use the
			/// <see cref="OpenRead" /> method and pass the returned stream to one of
			/// the <see cref="StreamReader" /> constructor overloads.
			/// </remarks>
			public StreamReader OpenText(string fileName)
			{
				return new StreamReader(this.OpenRead(fileName));
			}

			/// <summary>
			/// Compresses all files in a directory into the archive.
			/// Does not include subdirectories.
			/// </summary>
			/// <param name="sourceDirectory">The directory containing the
			/// files to be included.</param>
			/// <remarks>
			/// Uses maximum compression level.
			/// </remarks>
			public void Pack(string sourceDirectory)
			{
				this.Pack(sourceDirectory, false, CompressionLevel.Max, null);
			}

			/// <summary>
			/// Compresses all files in a directory into the archive, optionally
			/// including subdirectories.
			/// </summary>
			/// <param name="sourceDirectory">This is the root directory
			/// for to pack all files.</param>
			/// <param name="includeSubdirectories">If true, recursively include
			/// files in subdirectories.</param>
			/// <param name="compLevel">The compression level used when creating
			/// the archive.</param>
			/// <param name="progressHandler">Handler for receiving progress information;
			/// this may be null if progress is not desired.</param>
			/// <remarks>
			/// The files are stored in the archive using their relative file paths in
			/// the directory tree, if supported by the archive file format.
			/// </remarks>
			public void Pack(
				string sourceDirectory,
				bool includeSubdirectories,
				CompressionLevel compLevel,
				EventHandler<ArchiveProgressEventArgs> progressHandler)
			{
				IList<string> files = this.GetRelativeFilePathsInDirectoryTree(
					sourceDirectory, includeSubdirectories);
				this.PackFiles(sourceDirectory, files, files, compLevel, progressHandler);
			}

			/// <summary>
			/// Compresses files into the archive, specifying the names used to
			/// store the files in the archive.
			/// </summary>
			/// <param name="sourceDirectory">This parameter may be null, but
			/// if specified it is the root directory
			/// for any relative paths in <paramref name="sourceFileNames"/>.</param>
			/// <param name="sourceFileNames">The list of files to be included in
			/// the archive.</param>
			/// <param name="fileNames">The names of the files as they are stored
			/// in the archive. Each name
			/// includes the internal path of the file, if any. This parameter may
			/// be null, in which case the files are stored in the archive with their
			/// source file names and no path information.</param>
			/// <remarks>
			/// Uses maximum compression level.
			/// <p>Duplicate items in the <paramref name="fileNames"/> array will cause
			/// an <see cref="ArchiveException"/>.</p>
			/// </remarks>
			public void PackFiles(
				string sourceDirectory,
				IList<string> sourceFileNames,
				IList<string> fileNames)
			{
				this.PackFiles(
					sourceDirectory,
					sourceFileNames,
					fileNames,
					CompressionLevel.Max,
					null);
			}

			/// <summary>
			/// Compresses files into the archive, specifying the names used to
			/// store the files in the archive.
			/// </summary>
			/// <param name="sourceDirectory">This parameter may be null, but if
			/// specified it is the root directory
			/// for any relative paths in <paramref name="sourceFileNames"/>.</param>
			/// <param name="sourceFileNames">The list of files to be included in
			/// the archive.</param>
			/// <param name="fileNames">The names of the files as they are stored in
			/// the archive. Each name includes the internal path of the file, if any.
			/// This parameter may be null, in which case the files are stored in the
			/// archive with their source file names and no path information.</param>
			/// <param name="compLevel">The compression level used when creating the
			/// archive.</param>
			/// <param name="progressHandler">Handler for receiving progress information;
			/// this may be null if progress is not desired.</param>
			/// <remarks>
			/// Duplicate items in the <paramref name="fileNames"/> array will cause
			/// an <see cref="ArchiveException"/>.
			/// </remarks>
			public void PackFiles(
				string sourceDirectory,
				IList<string> sourceFileNames,
				IList<string> fileNames,
				CompressionLevel compLevel,
				EventHandler<ArchiveProgressEventArgs> progressHandler)
			{
				if (sourceFileNames == null)
				{
					throw new ArgumentNullException("sourceFileNames");
				}

				if (fileNames == null)
				{
					string[] fileNamesArray = new string[sourceFileNames.Count];
					for (int i = 0; i < sourceFileNames.Count; i++)
					{
						fileNamesArray[i] = Path.GetFileName(sourceFileNames[i]);
					}

					fileNames = fileNamesArray;
				}
				else if (fileNames.Count != sourceFileNames.Count)
				{
					throw new ArgumentOutOfRangeException("fileNames");
				}

				using (CompressionEngine compressionEngine = this.CreateCompressionEngine())
				{
					compressionEngine.Progress += progressHandler;
					IDictionary<string, string> contextFiles =
						ArchiveInfo.CreateStringDictionary(fileNames, sourceFileNames);
					ArchiveFileStreamContext streamContext = new ArchiveFileStreamContext(
							this.FullName, sourceDirectory, contextFiles);
					streamContext.EnableOffsetOpen = true;
					compressionEngine.CompressionLevel = compLevel;
					compressionEngine.Pack(streamContext, fileNames);
				}
			}

			/// <summary>
			/// Compresses files into the archive, specifying the names used
			/// to store the files in the archive.
			/// </summary>
			/// <param name="sourceDirectory">This parameter may be null, but if
			/// specified it is the root directory
			/// for any relative paths in <paramref name="fileNames"/>.</param>
			/// <param name="fileNames">A mapping from internal file paths to
			/// external file paths.</param>
			/// <remarks>
			/// Uses maximum compression level.
			/// </remarks>
			public void PackFileSet(
				string sourceDirectory,
				IDictionary<string, string> fileNames)
			{
				this.PackFileSet(sourceDirectory, fileNames, CompressionLevel.Max, null);
			}

			/// <summary>
			/// Compresses files into the archive, specifying the names used to
			/// store the files in the archive.
			/// </summary>
			/// <param name="sourceDirectory">This parameter may be null, but if
			/// specified it is the root directory
			/// for any relative paths in <paramref name="fileNames"/>.</param>
			/// <param name="fileNames">A mapping from internal file paths to
			/// external file paths.</param>
			/// <param name="compLevel">The compression level used when creating
			/// the archive.</param>
			/// <param name="progressHandler">Handler for receiving progress information;
			/// this may be null if progress is not desired.</param>
			public void PackFileSet(
				string sourceDirectory,
				IDictionary<string, string> fileNames,
				CompressionLevel compLevel,
				EventHandler<ArchiveProgressEventArgs> progressHandler)
			{
				if (fileNames == null)
				{
					throw new ArgumentNullException("fileNames");
				}

				string[] fileNamesArray = new string[fileNames.Count];
				fileNames.Keys.CopyTo(fileNamesArray, 0);

				using (CompressionEngine compressionEngine = this.CreateCompressionEngine())
				{
					compressionEngine.Progress += progressHandler;
					ArchiveFileStreamContext streamContext = new ArchiveFileStreamContext(
							this.FullName, sourceDirectory, fileNames);
					streamContext.EnableOffsetOpen = true;
					compressionEngine.CompressionLevel = compLevel;
					compressionEngine.Pack(streamContext, fileNamesArray);
				}
			}

			/// <summary>
			/// Given a directory, gets the relative paths of all files in the
			/// directory, optionally including all subdirectories.
			/// </summary>
			/// <param name="dir">The directory to search.</param>
			/// <param name="includeSubdirectories">True to include subdirectories
			/// in the search.</param>
			/// <returns>A list of file paths relative to the directory.</returns>
			internal IList<string> GetRelativeFilePathsInDirectoryTree(
				string dir, bool includeSubdirectories)
			{
				IList<string> fileList = new List<string>();
				this.RecursiveGetRelativeFilePathsInDirectoryTree(
					dir, String.Empty, includeSubdirectories, fileList);
				return fileList;
			}

			/// <summary>
			/// Retrieves information about one file from this archive.
			/// </summary>
			/// <param name="path">Path of the file in the archive.</param>
			/// <returns>File information, or null if the file was not found
			/// in the archive.</returns>
			internal ArchiveFileInfo GetFile(string path)
			{
				IList<ArchiveFileInfo> files = this.InternalGetFiles(
					delegate(string match)
					{
						return String.Compare(
							match, path, true, CultureInfo.InvariantCulture) == 0;
					});
				return (files != null && files.Count > 0 ? files[0] : null);
			}

			/// <summary>
			/// Creates a compression engine that does the low-level work for
			/// this object.
			/// </summary>
			/// <returns>A new compression engine instance that matches the specific
			/// subclass of archive.</returns>
			/// <remarks>
			/// Each instance will be <see cref="CompressionEngine.Dispose()"/>d
			/// immediately after use.
			/// </remarks>
			protected abstract CompressionEngine CreateCompressionEngine();

			/// <summary>
			/// Creates a case-insensitive dictionary mapping from one list of
			/// strings to the other.
			/// </summary>
			/// <param name="keys">List of keys.</param>
			/// <param name="values">List of values that are mapped 1-to-1 to
			/// the keys.</param>
			/// <returns>A filled dictionary of the strings.</returns>
			private static IDictionary<string, string> CreateStringDictionary(
				IList<string> keys, IList<string> values)
			{
				IDictionary<string, string> stringDict =
					new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
				for (int i = 0; i < keys.Count; i++)
				{
					stringDict.Add(keys[i], values[i]);
				}

				return stringDict;
			}

			/// <summary>
			/// Recursive-descent helper function for
			/// GetRelativeFilePathsInDirectoryTree.
			/// </summary>
			/// <param name="dir">The root directory of the search.</param>
			/// <param name="relativeDir">The relative directory to be
			/// processed now.</param>
			/// <param name="includeSubdirectories">True to descend into
			/// subdirectories.</param>
			/// <param name="fileList">List of files found so far.</param>
			private void RecursiveGetRelativeFilePathsInDirectoryTree(
				string dir,
				string relativeDir,
				bool includeSubdirectories,
				IList<string> fileList)
			{
				foreach (string file in System.IO.Directory.GetFiles(dir))
				{
					string fileName = Path.GetFileName(file);
					fileList.Add(Path.Combine(relativeDir, fileName));
				}

				if (includeSubdirectories)
				{
					foreach (string subDir in System.IO.Directory.GetDirectories(dir))
					{
						string subDirName = Path.GetFileName(subDir);
						this.RecursiveGetRelativeFilePathsInDirectoryTree(
							Path.Combine(dir, subDirName),
							Path.Combine(relativeDir, subDirName),
							includeSubdirectories,
							fileList);
					}
				}
			}

			/// <summary>
			/// Uses a CompressionEngine to get ArchiveFileInfo objects from this
			/// archive, and then associates them with this ArchiveInfo instance.
			/// </summary>
			/// <param name="fileFilter">Optional predicate that can determine
			/// which files to process.</param>
			/// <returns>A list of <see cref="ArchiveFileInfo"/> objects, each
			/// containing information about a file in the archive.</returns>
			private IList<ArchiveFileInfo> InternalGetFiles(Predicate<string> fileFilter)
			{
				using (CompressionEngine compressionEngine = this.CreateCompressionEngine())
				{
					ArchiveFileStreamContext streamContext =
						new ArchiveFileStreamContext(this.FullName, null, null);
					streamContext.EnableOffsetOpen = true;
					IList<ArchiveFileInfo> files =
						compressionEngine.GetFileInfo(streamContext, fileFilter);
					for (int i = 0; i < files.Count; i++)
					{
						files[i].Archive = this;
					}

					return files;
				}
			}
		}

		/// <summary>
		/// Contains the data reported in an archive progress event.
		/// </summary>
		public class ArchiveProgressEventArgs : EventArgs
		{
			private ArchiveProgressType progressType;

			private string currentFileName;
			private int currentFileNumber;
			private int totalFiles;
			private long currentFileBytesProcessed;
			private long currentFileTotalBytes;

			private string currentArchiveName;
			private short currentArchiveNumber;
			private short totalArchives;
			private long currentArchiveBytesProcessed;
			private long currentArchiveTotalBytes;

			private long fileBytesProcessed;
			private long totalFileBytes;

			/// <summary>
			/// Creates a new ArchiveProgressEventArgs object from specified event parameters.
			/// </summary>
			/// <param name="progressType">type of status message</param>
			/// <param name="currentFileName">name of the file being processed</param>
			/// <param name="currentFileNumber">number of the current file being processed</param>
			/// <param name="totalFiles">total number of files to be processed</param>
			/// <param name="currentFileBytesProcessed">number of bytes processed so far when compressing or extracting a file</param>
			/// <param name="currentFileTotalBytes">total number of bytes in the current file</param>
			/// <param name="currentArchiveName">name of the current Archive</param>
			/// <param name="currentArchiveNumber">current Archive number, when processing a chained set of Archives</param>
			/// <param name="totalArchives">total number of Archives in a chained set</param>
			/// <param name="currentArchiveBytesProcessed">number of compressed bytes processed so far during an extraction</param>
			/// <param name="currentArchiveTotalBytes">total number of compressed bytes to be processed during an extraction</param>
			/// <param name="fileBytesProcessed">number of uncompressed file bytes processed so far</param>
			/// <param name="totalFileBytes">total number of uncompressed file bytes to be processed</param>
			public ArchiveProgressEventArgs(
				ArchiveProgressType progressType,
				string currentFileName,
				int currentFileNumber,
				int totalFiles,
				long currentFileBytesProcessed,
				long currentFileTotalBytes,
				string currentArchiveName,
				int currentArchiveNumber,
				int totalArchives,
				long currentArchiveBytesProcessed,
				long currentArchiveTotalBytes,
				long fileBytesProcessed,
				long totalFileBytes)
			{
				this.progressType = progressType;
				this.currentFileName = currentFileName;
				this.currentFileNumber = currentFileNumber;
				this.totalFiles = totalFiles;
				this.currentFileBytesProcessed = currentFileBytesProcessed;
				this.currentFileTotalBytes = currentFileTotalBytes;
				this.currentArchiveName = currentArchiveName;
				this.currentArchiveNumber = (short)currentArchiveNumber;
				this.totalArchives = (short)totalArchives;
				this.currentArchiveBytesProcessed = currentArchiveBytesProcessed;
				this.currentArchiveTotalBytes = currentArchiveTotalBytes;
				this.fileBytesProcessed = fileBytesProcessed;
				this.totalFileBytes = totalFileBytes;
			}

			/// <summary>
			/// Gets the type of status message.
			/// </summary>
			/// <value>A <see cref="ArchiveProgressType"/> value indicating what type of progress event occurred.</value>
			/// <remarks>
			/// The handler may choose to ignore some types of progress events.
			/// For example, if the handler will only list each file as it is
			/// compressed/extracted, it can ignore events that
			/// are not of type <see cref="ArchiveProgressType.FinishFile"/>.
			/// </remarks>
			public ArchiveProgressType ProgressType
			{
				get
				{
					return this.progressType;
				}
			}

			/// <summary>
			/// Gets the name of the file being processed. (The name of the file within the Archive; not the external
			/// file path.) Also includes the internal path of the file, if any.  Valid for
			/// <see cref="ArchiveProgressType.StartFile"/>, <see cref="ArchiveProgressType.PartialFile"/>,
			/// and <see cref="ArchiveProgressType.FinishFile"/> messages.
			/// </summary>
			/// <value>The name of the file currently being processed, or null if processing
			/// is currently at the stream or archive level.</value>
			public string CurrentFileName
			{
				get
				{
					return this.currentFileName;
				}
			}

			/// <summary>
			/// Gets the number of the current file being processed. The first file is number 0, and the last file
			/// is <see cref="TotalFiles"/>-1. Valid for <see cref="ArchiveProgressType.StartFile"/>,
			/// <see cref="ArchiveProgressType.PartialFile"/>, and <see cref="ArchiveProgressType.FinishFile"/> messages.
			/// </summary>
			/// <value>The number of the file currently being processed, or the most recent
			/// file processed if processing is currently at the stream or archive level.</value>
			public int CurrentFileNumber
			{
				get
				{
					return this.currentFileNumber;
				}
			}

			/// <summary>
			/// Gets the total number of files to be processed.  Valid for all message types.
			/// </summary>
			/// <value>The total number of files to be processed that are known so far.</value>
			public int TotalFiles
			{
				get
				{
					return this.totalFiles;
				}
			}

			/// <summary>
			/// Gets the number of bytes processed so far when compressing or extracting a file.  Valid for
			/// <see cref="ArchiveProgressType.StartFile"/>, <see cref="ArchiveProgressType.PartialFile"/>,
			/// and <see cref="ArchiveProgressType.FinishFile"/> messages.
			/// </summary>
			/// <value>The number of uncompressed bytes processed so far for the current file,
			/// or 0 if processing is currently at the stream or archive level.</value>
			public long CurrentFileBytesProcessed
			{
				get
				{
					return this.currentFileBytesProcessed;
				}
			}

			/// <summary>
			/// Gets the total number of bytes in the current file.  Valid for <see cref="ArchiveProgressType.StartFile"/>,
			/// <see cref="ArchiveProgressType.PartialFile"/>, and <see cref="ArchiveProgressType.FinishFile"/> messages.
			/// </summary>
			/// <value>The uncompressed size of the current file being processed,
			/// or 0 if processing is currently at the stream or archive level.</value>
			public long CurrentFileTotalBytes
			{
				get
				{
					return this.currentFileTotalBytes;
				}
			}

			/// <summary>
			/// Gets the name of the current archive.  Not necessarily the name of the archive on disk.
			/// Valid for all message types.
			/// </summary>
			/// <value>The name of the current archive, or an empty string if no name was specified.</value>
			public string CurrentArchiveName
			{
				get
				{
					return this.currentArchiveName;
				}
			}

			/// <summary>
			/// Gets the current archive number, when processing a chained set of archives. Valid for all message types.
			/// </summary>
			/// <value>The number of the current archive.</value>
			/// <remarks>The first archive is number 0, and the last archive is
			/// <see cref="TotalArchives"/>-1.</remarks>
			public int CurrentArchiveNumber
			{
				get
				{
					return this.currentArchiveNumber;
				}
			}

			/// <summary>
			/// Gets the total number of known archives in a chained set. Valid for all message types.
			/// </summary>
			/// <value>The total number of known archives in a chained set.</value>
			/// <remarks>
			/// When using the compression option to auto-split into multiple archives based on data size,
			/// this value will not be accurate until the end.
			/// </remarks>
			public int TotalArchives
			{
				get
				{
					return this.totalArchives;
				}
			}

			/// <summary>
			/// Gets the number of compressed bytes processed so far during extraction
			/// of the current archive. Valid for all extraction messages.
			/// </summary>
			/// <value>The number of compressed bytes processed so far during extraction
			/// of the current archive.</value>
			public long CurrentArchiveBytesProcessed
			{
				get
				{
					return this.currentArchiveBytesProcessed;
				}
			}

			/// <summary>
			/// Gets the total number of compressed bytes to be processed during extraction
			/// of the current archive. Valid for all extraction messages.
			/// </summary>
			/// <value>The total number of compressed bytes to be processed during extraction
			/// of the current archive.</value>
			public long CurrentArchiveTotalBytes
			{
				get
				{
					return this.currentArchiveTotalBytes;
				}
			}

			/// <summary>
			/// Gets the number of uncompressed bytes processed so far among all files. Valid for all message types.  
			/// </summary>
			/// <value>The number of uncompressed file bytes processed so far among all files.</value>
			/// <remarks>
			/// When compared to <see cref="TotalFileBytes"/>, this can be used as a measure of overall progress.
			/// </remarks>
			public long FileBytesProcessed
			{
				get
				{
					return this.fileBytesProcessed;
				}
			}

			/// <summary>
			/// Gets the total number of uncompressed file bytes to be processed.  Valid for all message types.
			/// </summary>
			/// <value>The total number of uncompressed bytes to be processed among all files.</value>
			public long TotalFileBytes
			{
				get
				{
					return this.totalFileBytes;
				}
			}

	#if DEBUG

		/// <summary>
		/// Creates a string representation of the progress event.
		/// </summary>
		/// <returns>a listing of all event parameters and values</returns>
		public override string ToString()
		{
			string formatString =
				"{0}\n" +
				"\t CurrentFileName              = {1}\n" +
				"\t CurrentFileNumber            = {2}\n" +
				"\t TotalFiles                   = {3}\n" +
				"\t CurrentFileBytesProcessed    = {4}\n" +
				"\t CurrentFileTotalBytes        = {5}\n" +
				"\t CurrentArchiveName           = {6}\n" +
				"\t CurrentArchiveNumber         = {7}\n" +
				"\t TotalArchives                = {8}\n" +
				"\t CurrentArchiveBytesProcessed = {9}\n" +
				"\t CurrentArchiveTotalBytes     = {10}\n" +
				"\t FileBytesProcessed           = {11}\n" +
				"\t TotalFileBytes               = {12}\n";
			return String.Format(
				System.Globalization.CultureInfo.InvariantCulture,
				formatString,
				this.ProgressType,
				this.CurrentFileName,
				this.CurrentFileNumber,
				this.TotalFiles,
				this.CurrentFileBytesProcessed,
				this.CurrentFileTotalBytes,
				this.CurrentArchiveName,
				this.CurrentArchiveNumber,
				this.TotalArchives,
				this.CurrentArchiveBytesProcessed,
				this.CurrentArchiveTotalBytes,
				this.FileBytesProcessed,
				this.TotalFileBytes);
		}

	#endif
		}


		/// <summary>
		/// The type of progress event.
		/// </summary>
		/// <remarks>
		/// <p>PACKING EXAMPLE: The following sequence of events might be received when
		/// extracting a simple archive file with 2 files.</p>
		/// <list type="table">
		/// <listheader><term>Message Type</term><description>Description</description></listheader>
		/// <item><term>StartArchive</term> <description>Begin extracting archive</description></item>
		/// <item><term>StartFile</term>    <description>Begin extracting first file</description></item>
		/// <item><term>PartialFile</term>  <description>Extracting first file</description></item>
		/// <item><term>PartialFile</term>  <description>Extracting first file</description></item>
		/// <item><term>FinishFile</term>   <description>Finished extracting first file</description></item>
		/// <item><term>StartFile</term>    <description>Begin extracting second file</description></item>
		/// <item><term>PartialFile</term>  <description>Extracting second file</description></item>
		/// <item><term>FinishFile</term>   <description>Finished extracting second file</description></item>
		/// <item><term>FinishArchive</term><description>Finished extracting archive</description></item>
		/// </list>
		/// <p></p>
		/// <p>UNPACKING EXAMPLE:  Packing 3 files into 2 archive chunks, where the second file is
		///	continued to the second archive chunk.</p>
		/// <list type="table">
		/// <listheader><term>Message Type</term><description>Description</description></listheader>
		/// <item><term>StartFile</term>     <description>Begin compressing first file</description></item>
		/// <item><term>FinishFile</term>    <description>Finished compressing first file</description></item>
		/// <item><term>StartFile</term>     <description>Begin compressing second file</description></item>
		/// <item><term>PartialFile</term>   <description>Compressing second file</description></item>
		/// <item><term>PartialFile</term>   <description>Compressing second file</description></item>
		/// <item><term>FinishFile</term>    <description>Finished compressing second file</description></item>
		/// <item><term>StartArchive</term>  <description>Begin writing first archive</description></item>
		/// <item><term>PartialArchive</term><description>Writing first archive</description></item>
		/// <item><term>FinishArchive</term> <description>Finished writing first archive</description></item>
		/// <item><term>StartFile</term>     <description>Begin compressing third file</description></item>
		/// <item><term>PartialFile</term>   <description>Compressing third file</description></item>
		/// <item><term>FinishFile</term>    <description>Finished compressing third file</description></item>
		/// <item><term>StartArchive</term>  <description>Begin writing second archive</description></item>
		/// <item><term>PartialArchive</term><description>Writing second archive</description></item>
		/// <item><term>FinishArchive</term> <description>Finished writing second archive</description></item>
		/// </list>
		/// </remarks>
		public enum ArchiveProgressType : int
		{
			/// <summary>Status message before beginning the packing or unpacking an individual file.</summary>
			StartFile,

			/// <summary>Status message (possibly reported multiple times) during the process of packing or unpacking a file.</summary>
			PartialFile,

			/// <summary>Status message after completion of the packing or unpacking an individual file.</summary>
			FinishFile,

			/// <summary>Status message before beginning the packing or unpacking an archive.</summary>
			StartArchive,

			/// <summary>Status message (possibly reported multiple times) during the process of packing or unpacking an archiv.</summary>
			PartialArchive,

			/// <summary>Status message after completion of the packing or unpacking of an archive.</summary>
			FinishArchive,
		}

		/// <summary>
		/// Stream context used to extract a single file from an archive into a memory stream.
		/// </summary>
		public class BasicUnpackStreamContext : IUnpackStreamContext
		{
			private Stream archiveStream;
			private Stream fileStream;

			/// <summary>
			/// Creates a new BasicExtractStreamContext that reads from the specified archive stream.
			/// </summary>
			/// <param name="archiveStream">Archive stream to read from.</param>
			public BasicUnpackStreamContext(Stream archiveStream)
			{
				this.archiveStream = archiveStream;
			}

			/// <summary>
			/// Gets the stream for the extracted file, or null if no file was extracted.
			/// </summary>
			public Stream FileStream
			{
				get
				{
					return this.fileStream;
				}
			}

			/// <summary>
			/// Opens the archive stream for reading. Returns a DuplicateStream instance,
			/// so the stream may be virtually opened multiple times.
			/// </summary>
			/// <param name="archiveNumber">The archive number to open (ignored; 0 is assumed).</param>
			/// <param name="archiveName">The name of the archive being opened.</param>
			/// <param name="compressionEngine">Instance of the compression engine doing the operations.</param>
			/// <returns>A stream from which archive bytes are read.</returns>
			public Stream OpenArchiveReadStream(int archiveNumber, string archiveName, CompressionEngine compressionEngine)
			{
				return new DuplicateStream(this.archiveStream);
			}

			/// <summary>
			/// Does *not* close the stream. The archive stream should be managed by
			/// the code that invokes the archive extraction.
			/// </summary>
			/// <param name="archiveNumber">The archive number of the stream to close.</param>
			/// <param name="archiveName">The name of the archive being closed.</param>
			/// <param name="stream">The stream being closed.</param>
			public void CloseArchiveReadStream(int archiveNumber, string archiveName, Stream stream)
			{
				// Do nothing.
			}

			/// <summary>
			/// Opens a stream for writing extracted file bytes. The returned stream is a MemoryStream
			/// instance, so the file is extracted straight into memory.
			/// </summary>
			/// <param name="path">Path of the file within the archive.</param>
			/// <param name="fileSize">The uncompressed size of the file to be extracted.</param>
			/// <param name="lastWriteTime">The last write time of the file.</param>
			/// <returns>A stream where extracted file bytes are to be written.</returns>
			public Stream OpenFileWriteStream(string path, long fileSize, DateTime lastWriteTime)
			{
				this.fileStream = new MemoryStream(new byte[fileSize], 0, (int) fileSize, true, true);
				return this.fileStream;
			}

			/// <summary>
			/// Does *not* close the file stream. The file stream is saved in memory so it can
			/// be read later.
			/// </summary>
			/// <param name="path">Path of the file within the archive.</param>
			/// <param name="stream">The file stream to be closed.</param>
			/// <param name="attributes">The attributes of the extracted file.</param>
			/// <param name="lastWriteTime">The last write time of the file.</param>
			public void CloseFileWriteStream(string path, Stream stream, FileAttributes attributes, DateTime lastWriteTime)
			{
				// Do nothing.
			}
		}

		/// <summary>
		/// Engine capable of packing and unpacking archives in the cabinet format.
		/// </summary>
		public class CabEngine : CompressionEngine
		{
			private CabPacker packer;
			private CabUnpacker unpacker;

			/// <summary>
			/// Creates a new instance of the cabinet engine.
			/// </summary>
			public CabEngine()
				: base()
			{
			}

			/// <summary>
			/// Disposes of resources allocated by the cabinet engine.
			/// </summary>
			/// <param name="disposing">If true, the method has been called directly
			/// or indirectly by a user's code, so managed and unmanaged resources
			/// will be disposed. If false, the method has been called by the runtime
			/// from inside the finalizer, and only unmanaged resources will be
			/// disposed.</param>
			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					if (packer != null)
					{
						packer.Dispose();
						packer = null;
					}
					if (unpacker != null)
					{
						unpacker.Dispose();
						unpacker = null;
					}
				}

				base.Dispose(disposing);
			}

			private CabPacker Packer
			{
				get
				{
					if (this.packer == null)
					{
						this.packer = new CabPacker(this);
					}

					return this.packer;
				}
			}

			private CabUnpacker Unpacker
			{
				get
				{
					if (this.unpacker == null)
					{
						this.unpacker = new CabUnpacker(this);
					}

					return this.unpacker;
				}
			}

			/// <summary>
			/// Creates a cabinet or chain of cabinets.
			/// </summary>
			/// <param name="streamContext">A context interface to handle opening
			/// and closing of cabinet and file streams.</param>
			/// <param name="files">The paths of the files in the archive (not
			/// external file paths).</param>
			/// <param name="maxArchiveSize">The maximum number of bytes for one
			/// cabinet before the contents are chained to the next cabinet, or zero
			/// for unlimited cabinet size.</param>
			/// <exception cref="ArchiveException">The cabinet could not be
			/// created.</exception>
			/// <remarks>
			/// The stream context implementation may provide a mapping from the
			/// file paths within the cabinet to the external file paths.
			/// <para>Smaller folder sizes can make it more efficient to extract
			/// individual files out of large cabinet packages.</para>
			/// </remarks>
			public override void Pack(
				IPackStreamContext streamContext,
				IEnumerable<string> files,
				long maxArchiveSize)
			{
				this.Packer.CompressionLevel = this.CompressionLevel;
				this.Packer.UseTempFiles = this.UseTempFiles;
				this.Packer.Pack(streamContext, files, maxArchiveSize);
			}

			/// <summary>
			/// Checks whether a Stream begins with a header that indicates
			/// it is a valid cabinet file.
			/// </summary>
			/// <param name="stream">Stream for reading the cabinet file.</param>
			/// <returns>True if the stream is a valid cabinet file
			/// (with no offset); false otherwise.</returns>
			public override bool IsArchive(Stream stream)
			{
				return this.Unpacker.IsArchive(stream);
			}

			/// <summary>
			/// Gets information about files in a cabinet or cabinet chain.
			/// </summary>
			/// <param name="streamContext">A context interface to handle opening
			/// and closing of cabinet and file streams.</param>
			/// <param name="fileFilter">A predicate that can determine
			/// which files to process, optional.</param>
			/// <returns>Information about files in the cabinet stream.</returns>
			/// <exception cref="ArchiveException">The cabinet provided
			/// by the stream context is not valid.</exception>
			/// <remarks>
			/// The <paramref name="fileFilter"/> predicate takes an internal file
			/// path and returns true to include the file or false to exclude it.
			/// </remarks>
			public override IList<ArchiveFileInfo> GetFileInfo(
				IUnpackStreamContext streamContext,
				Predicate<string> fileFilter)
			{
				return this.Unpacker.GetFileInfo(streamContext, fileFilter);
			}

			/// <summary>
			/// Extracts files from a cabinet or cabinet chain.
			/// </summary>
			/// <param name="streamContext">A context interface to handle opening
			/// and closing of cabinet and file streams.</param>
			/// <param name="fileFilter">An optional predicate that can determine
			/// which files to process.</param>
			/// <exception cref="ArchiveException">The cabinet provided
			/// by the stream context is not valid.</exception>
			/// <remarks>
			/// The <paramref name="fileFilter"/> predicate takes an internal file
			/// path and returns true to include the file or false to exclude it.
			/// </remarks>
			public override void Unpack(
				IUnpackStreamContext streamContext,
				Predicate<string> fileFilter)
			{
				this.Unpacker.Unpack(streamContext, fileFilter);
			}

			internal void ReportProgress(ArchiveProgressEventArgs e)
			{
				base.OnProgress(e);
			}
		}

		internal static class CabErrorStrings
		{
			public static System.String GetErrorCode(System.Int32 ERF)
			{
				System.String[] Er1000 = { "Unknown error creating cabinet." , "Failure opening file to be stored in cabinet.",
				"Failure reading file to be stored in the cabinet." , "Could not allocate enough memory to create the cabinet." ,
				"Could not create a temporary file." , "Unknown compression type." , "Could not create cabinet file." ,
				"Client requested abort." , "Failure compressing data."};
				System.String[] Er2000 = { "Unknown error extracting cabinet." , "Cabinet not found." , 
					"Cabinet file does not have the correct format or is not a cabinet." , "Cabinet file has an unknown version number." ,
				"Cabinet file is corrupt." , "Could not allocate enough memory to extract cabinet." , "Unknown compression type in a cabinet folder." ,
				"Failure decompressing data from a cabinet file." , "Failure writing to target file." ,
				"Cabinets in a set do not have the same RESERVE sizes." , "Cabinet number returned on NEXT_CABINET is incorrect." ,
				"Client requested abort." };
				try 
				{
					if (ERF < 1999)
					{
						for (System.Int32 ITR = 0; (ITR + 800) < ERF; ITR++)
						{
							if (ITR + 1000 == ERF) { return Er1000[ITR]; }
						}
					}
					else
					{
						for (System.Int32 ITR = 0; (ITR + 800) < ERF; ITR++)
						{
							if (ITR + 2000 == ERF) { return Er2000[ITR]; }
						}
					}
				} catch (System.Exception) 
				{
					return "Generic Error: Could not find the error resource string provided.";
				}
				return "Generic Error: Could not find the error resource string provided.";
			}
		}

		/// <summary>
		/// Exception class for cabinet operations.
		/// </summary>
		[Serializable]
		public class CabException : ArchiveException
		{
			private int error;
			private int errorCode;

			/// <summary>
			/// Creates a new CabException with a specified error message and a reference to the
			/// inner exception that is the cause of this exception.
			/// </summary>
			/// <param name="message">The message that describes the error.</param>
			/// <param name="innerException">The exception that is the cause of the current exception. If the
			/// innerException parameter is not a null reference (Nothing in Visual Basic), the current exception
			/// is raised in a catch block that handles the inner exception.</param>
			public CabException(string message, Exception innerException)
				: this(0, 0, message, innerException) { }

			/// <summary>
			/// Creates a new CabException with a specified error message.
			/// </summary>
			/// <param name="message">The message that describes the error.</param>
			public CabException(string message)
				: this(0, 0, message, null) { }

			/// <summary>
			/// Creates a new CabException.
			/// </summary>
			public CabException()
				: this(0, 0, null, null) { }

			internal CabException(int error, int errorCode, string message, Exception innerException)
				: base(message, innerException)
			{
				this.error = error;
				this.errorCode = errorCode;
			}

			internal CabException(int error, int errorCode, string message)
				: this(error, errorCode, message, null) { }

			/// <summary>
			/// Initializes a new instance of the CabException class with serialized data.
			/// </summary>
			/// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
			/// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
			protected CabException(SerializationInfo info, StreamingContext context) : base(info, context)
			{
				if (info == null)
				{
					throw new ArgumentNullException("info");
				}

				this.error = info.GetInt32("cabError");
				this.errorCode = info.GetInt32("cabErrorCode");
			}

			/// <summary>
			/// Gets the FCI or FDI cabinet engine error number.
			/// </summary>
			/// <value>A cabinet engine error number, or 0 if the exception was
			/// not related to a cabinet engine error number.</value>
			public int Error
			{
				get
				{
					return this.error;
				}
			}

			/// <summary>
			/// Gets the Win32 error code.
			/// </summary>
			/// <value>A Win32 error code, or 0 if the exception was
			/// not related to a Win32 error.</value>
			public int ErrorCode
			{
				get
				{
					return this.errorCode;
				}
			}

			/// <summary>
			/// Sets the SerializationInfo with information about the exception.
			/// </summary>
			/// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
			/// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
			public override void GetObjectData(SerializationInfo info, StreamingContext context)
			{
				if (info == null)
				{
					throw new ArgumentNullException("info");
				}

				info.AddValue("cabError", this.error);
				info.AddValue("cabErrorCode", this.errorCode);
				base.GetObjectData(info, context);
			}

			internal static string GetErrorMessage(int error, int errorCode, bool extracting)
			{
				const int FCI_ERROR_RESOURCE_OFFSET = 1000;
				const int FDI_ERROR_RESOURCE_OFFSET = 2000;
				int resourceOffset = (extracting ? FDI_ERROR_RESOURCE_OFFSET : FCI_ERROR_RESOURCE_OFFSET);

				string msg = CabErrorStrings.GetErrorCode(resourceOffset + error);

				if (msg == null)
				{
					msg = CabErrorStrings.GetErrorCode(resourceOffset);
				}

				if (errorCode != 0)
				{ 
					msg = String.Format(CultureInfo.InvariantCulture, "{0} Error Code: {1}", msg, errorCode);
				}
				return msg;
			}
		}

		/// <summary>
		/// Object representing a compressed file within a cabinet package; provides operations for getting
		/// the file properties and extracting the file.
		/// </summary>
		[Serializable]
		public class CabFileInfo : ArchiveFileInfo
		{
			private int cabFolder;

			/// <summary>
			/// Creates a new CabinetFileInfo object representing a file within a cabinet in a specified path.
			/// </summary>
			/// <param name="cabinetInfo">An object representing the cabinet containing the file.</param>
			/// <param name="filePath">The path to the file within the cabinet. Usually, this is a simple file
			/// name, but if the cabinet contains a directory structure this may include the directory.</param>
			public CabFileInfo(CabInfo cabinetInfo, string filePath)
				: base(cabinetInfo, filePath)
			{
				if (cabinetInfo == null)
				{
					throw new ArgumentNullException("cabinetInfo");
				}

				this.cabFolder = -1;
			}

			/// <summary>
			/// Creates a new CabinetFileInfo object with all parameters specified,
			/// used internally when reading the metadata out of a cab.
			/// </summary>
			/// <param name="filePath">The internal path and name of the file in the cab.</param>
			/// <param name="cabFolder">The folder number containing the file.</param>
			/// <param name="cabNumber">The cabinet number where the file starts.</param>
			/// <param name="attributes">The stored attributes of the file.</param>
			/// <param name="lastWriteTime">The stored last write time of the file.</param>
			/// <param name="length">The uncompressed size of the file.</param>
			internal CabFileInfo(
				string filePath,
				int cabFolder,
				int cabNumber,
				FileAttributes attributes,
				DateTime lastWriteTime,
				long length)
				: base(filePath, cabNumber, attributes, lastWriteTime, length)
			{
				this.cabFolder = cabFolder;
			}

			/// <summary>
			/// Initializes a new instance of the CabinetFileInfo class with serialized data.
			/// </summary>
			/// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
			/// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
			protected CabFileInfo(SerializationInfo info, StreamingContext context)
				: base(info, context)
			{
				this.cabFolder = info.GetInt32("cabFolder");
			}

			/// <summary>
			/// Sets the SerializationInfo with information about the archive.
			/// </summary>
			/// <param name="info">The SerializationInfo that holds the serialized object data.</param>
			/// <param name="context">The StreamingContext that contains contextual information
			/// about the source or destination.</param>
			public override void GetObjectData(SerializationInfo info, StreamingContext context)
			{
				base.GetObjectData(info, context);
				info.AddValue("cabFolder", this.cabFolder);
			}

			/// <summary>
			/// Gets or sets the cabinet that contains this file.
			/// </summary>
			/// <value>
			/// The CabinetInfo instance that retrieved this file information -- this
			/// may be null if the CabinetFileInfo object was returned directly from a
			/// stream.
			/// </value>
			public CabInfo Cabinet
			{
				get
				{
					return (CabInfo) this.Archive;
				}
			}

			/// <summary>
			/// Gets the full path of the cabinet that contains this file.
			/// </summary>
			/// <value>The full path of the cabinet that contains this file.</value>
			public string CabinetName
			{
				get
				{
					return this.ArchiveName;
				}
			}

			/// <summary>
			/// Gets the number of the folder containing this file.
			/// </summary>
			/// <value>The number of the cabinet folder containing this file.</value>
			/// <remarks>A single folder or the first folder of a cabinet
			/// (or chain of cabinets) is numbered 0.</remarks>
			public int CabinetFolderNumber
			{
				get
				{
					if (this.cabFolder < 0)
					{
						this.Refresh();
					}
					return this.cabFolder;
				}
			}

			/// <summary>
			/// Refreshes the information in this object with new data retrieved
			/// from an archive.
			/// </summary>
			/// <param name="newFileInfo">Fresh instance for the same file just
			/// read from the archive.</param>
			/// <remarks>
			/// This implementation refreshes the <see cref="CabinetFolderNumber"/>.
			/// </remarks>
			protected override void Refresh(ArchiveFileInfo newFileInfo)
			{
				base.Refresh(newFileInfo);
				this.cabFolder = ((CabFileInfo) newFileInfo).cabFolder;
			}
		}

		/// <summary>
		/// Object representing a cabinet file on disk; provides access to
		/// file-based operations on the cabinet file.
		/// </summary>
		/// <remarks>
		/// Generally, the methods on this class are much easier to use than the
		/// stream-based interfaces provided by the <see cref="CabEngine"/> class.
		/// </remarks>
		[Serializable]
		public class CabInfo : ArchiveInfo
		{
			/// <summary>
			/// Creates a new CabinetInfo object representing a cabinet file in a specified path.
			/// </summary>
			/// <param name="path">The path to the cabinet file. When creating a cabinet file, this file does not
			/// necessarily exist yet.</param>
			public CabInfo(string path)
				: base(path)
			{
			}

			/// <summary>
			/// Initializes a new instance of the CabinetInfo class with serialized data.
			/// </summary>
			/// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
			/// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
			protected CabInfo(SerializationInfo info, StreamingContext context)
				: base(info, context)
			{
			}

			/// <summary>
			/// Creates a compression engine that does the low-level work for
			/// this object.
			/// </summary>
			/// <returns>A new <see cref="CabEngine"/> instance.</returns>
			/// <remarks>
			/// Each instance will be <see cref="CompressionEngine.Dispose()"/>d
			/// immediately after use.
			/// </remarks>
			protected override CompressionEngine CreateCompressionEngine()
			{
				return new CabEngine();
			}

			/// <summary>
			/// Gets information about the files contained in the archive.
			/// </summary>
			/// <returns>A list of <see cref="CabFileInfo"/> objects, each
			/// containing information about a file in the archive.</returns>
			public new IList<CabFileInfo> GetFiles()
			{
				IList<ArchiveFileInfo> files = base.GetFiles();
				List<CabFileInfo> cabFiles = new List<CabFileInfo>(files.Count);
				foreach (CabFileInfo cabFile in files) cabFiles.Add(cabFile);
				return cabFiles.AsReadOnly();
			}

			/// <summary>
			/// Gets information about the certain files contained in the archive file.
			/// </summary>
			/// <param name="searchPattern">The search string, such as
			/// &quot;*.txt&quot;.</param>
			/// <returns>A list of <see cref="CabFileInfo"/> objects, each containing
			/// information about a file in the archive.</returns>
			public new IList<CabFileInfo> GetFiles(string searchPattern)
			{
				IList<ArchiveFileInfo> files = base.GetFiles(searchPattern);
				List<CabFileInfo> cabFiles = new List<CabFileInfo>(files.Count);
				foreach (CabFileInfo cabFile in files) cabFiles.Add(cabFile);
				return cabFiles.AsReadOnly();
			}
		}

		internal class CabPacker : CabWorker
		{
			private const string TempStreamName = "%%TEMP%%";

			private NativeMethods.FCI.Handle fciHandle;

			// These delegates need to be saved as member variables
			// so that they don't get GC'd.
			private NativeMethods.FCI.PFNALLOC fciAllocMemHandler;
			private NativeMethods.FCI.PFNFREE fciFreeMemHandler;
			private NativeMethods.FCI.PFNOPEN fciOpenStreamHandler;
			private NativeMethods.FCI.PFNREAD fciReadStreamHandler;
			private NativeMethods.FCI.PFNWRITE fciWriteStreamHandler;
			private NativeMethods.FCI.PFNCLOSE fciCloseStreamHandler;
			private NativeMethods.FCI.PFNSEEK fciSeekStreamHandler;
			private NativeMethods.FCI.PFNFILEPLACED fciFilePlacedHandler;
			private NativeMethods.FCI.PFNDELETE fciDeleteFileHandler;
			private NativeMethods.FCI.PFNGETTEMPFILE fciGetTempFileHandler;

			private NativeMethods.FCI.PFNGETNEXTCABINET fciGetNextCabinet;
			private NativeMethods.FCI.PFNSTATUS fciCreateStatus;
			private NativeMethods.FCI.PFNGETOPENINFO fciGetOpenInfo;

			private IPackStreamContext context;

			private FileAttributes fileAttributes;
			private DateTime fileLastWriteTime;

			private int maxCabBytes;

			private long totalFolderBytesProcessedInCurrentCab;

			private CompressionLevel compressionLevel;
			private bool dontUseTempFiles;
			private IList<Stream> tempStreams;

			public CabPacker(CabEngine cabEngine)
				: base(cabEngine)
			{
				this.fciAllocMemHandler    = this.CabAllocMem;
				this.fciFreeMemHandler     = this.CabFreeMem;
				this.fciOpenStreamHandler  = this.CabOpenStreamEx;
				this.fciReadStreamHandler  = this.CabReadStreamEx;
				this.fciWriteStreamHandler = this.CabWriteStreamEx;
				this.fciCloseStreamHandler = this.CabCloseStreamEx;
				this.fciSeekStreamHandler  = this.CabSeekStreamEx;
				this.fciFilePlacedHandler  = this.CabFilePlaced;
				this.fciDeleteFileHandler  = this.CabDeleteFile;
				this.fciGetTempFileHandler = this.CabGetTempFile;
				this.fciGetNextCabinet     = this.CabGetNextCabinet;
				this.fciCreateStatus       = this.CabCreateStatus;
				this.fciGetOpenInfo        = this.CabGetOpenInfo;
				this.tempStreams = new List<Stream>();
				this.compressionLevel = CompressionLevel.Normal;
			}

			public bool UseTempFiles
			{
				get
				{
					return !this.dontUseTempFiles;
				}

				set
				{
					this.dontUseTempFiles = !value;
				}
			}

			public CompressionLevel CompressionLevel
			{
				get
				{
					return this.compressionLevel;
				}

				set
				{
					this.compressionLevel = value;
				}
			}

			private void CreateFci(long maxArchiveSize)
			{
				NativeMethods.FCI.CCAB ccab = new NativeMethods.FCI.CCAB();
				if (maxArchiveSize > 0 && maxArchiveSize < ccab.cb)
				{
					ccab.cb = Math.Max(
						NativeMethods.FCI.MIN_DISK, (int) maxArchiveSize);
				}

				object maxFolderSizeOption = this.context.GetOption(
					"maxFolderSize", null);
				if (maxFolderSizeOption != null)
				{
					long maxFolderSize = Convert.ToInt64(
						maxFolderSizeOption, CultureInfo.InvariantCulture);
					if (maxFolderSize > 0 && maxFolderSize < ccab.cbFolderThresh)
					{
						ccab.cbFolderThresh = (int) maxFolderSize;
					}
				}

				this.maxCabBytes = ccab.cb;
				ccab.szCab = this.context.GetArchiveName(0);
				if (ccab.szCab == null)
				{
					throw new FileNotFoundException(
						"Cabinet name not provided by stream context.");
				}
				ccab.setID = (short) new Random().Next(
					Int16.MinValue, Int16.MaxValue + 1);
				this.CabNumbers[ccab.szCab] = 0;
				this.currentArchiveName = ccab.szCab;
				this.totalArchives = 1;
				this.CabStream = null;

				this.Erf.Clear();
				this.fciHandle = NativeMethods.FCI.Create(
					this.ErfHandle.AddrOfPinnedObject(),
					this.fciFilePlacedHandler,
					this.fciAllocMemHandler,
					this.fciFreeMemHandler,
					this.fciOpenStreamHandler,
					this.fciReadStreamHandler,
					this.fciWriteStreamHandler,
					this.fciCloseStreamHandler,
					this.fciSeekStreamHandler,
					this.fciDeleteFileHandler,
					this.fciGetTempFileHandler,
					ccab,
					IntPtr.Zero);
				this.CheckError(false);
			}

			public void Pack(
				IPackStreamContext streamContext,
				IEnumerable<string> files,
				long maxArchiveSize)
			{
				if (streamContext == null)
				{
					throw new ArgumentNullException("streamContext");
				}

				if (files == null)
				{
					throw new ArgumentNullException("files");
				}

				lock (this)
				{
					try
					{
						this.context = streamContext;

						this.ResetProgressData();

						this.CreateFci(maxArchiveSize);

						foreach (string file in files)
						{
							FileAttributes attributes;
							DateTime lastWriteTime;
							Stream fileStream = this.context.OpenFileReadStream(
								file,
								out attributes,
								out lastWriteTime);
							if (fileStream != null)
							{
								this.totalFileBytes += fileStream.Length;
								this.totalFiles++;
								this.context.CloseFileReadStream(file, fileStream);
							}
						}

						long uncompressedBytesInFolder = 0;
						this.currentFileNumber = -1;

						foreach (string file in files)
						{
							FileAttributes attributes;
							DateTime lastWriteTime;
							Stream fileStream = this.context.OpenFileReadStream(
								file, out attributes, out lastWriteTime);
							if (fileStream == null)
							{
								continue;
							}

							if (fileStream.Length >= (long) NativeMethods.FCI.MAX_FOLDER)
							{
								throw new NotSupportedException(String.Format(
									CultureInfo.InvariantCulture,
									"File {0} exceeds maximum file size " +
									"for cabinet format.",
									file));
							}

							if (uncompressedBytesInFolder > 0)
							{
								// Automatically create a new folder if this file
								// won't fit in the current folder.
								bool nextFolder = uncompressedBytesInFolder
									+ fileStream.Length >= (long) NativeMethods.FCI.MAX_FOLDER;

								// Otherwise ask the client if it wants to
								// move to the next folder.
								if (!nextFolder)
								{
									object nextFolderOption = streamContext.GetOption(
										"nextFolder",
										new object[] { file, this.currentFolderNumber });
									nextFolder = Convert.ToBoolean(
										nextFolderOption, CultureInfo.InvariantCulture);
								}

								if (nextFolder)
								{
									this.FlushFolder();
									uncompressedBytesInFolder = 0;
								}
							}

							if (this.currentFolderTotalBytes > 0)
							{
								this.currentFolderTotalBytes = 0;
								this.currentFolderNumber++;
								uncompressedBytesInFolder = 0;
							}

							this.currentFileName = file;
							this.currentFileNumber++;

							this.currentFileTotalBytes = fileStream.Length;
							this.currentFileBytesProcessed = 0;
							this.OnProgress(ArchiveProgressType.StartFile);

							uncompressedBytesInFolder += fileStream.Length;

							this.AddFile(
								file,
								fileStream,
								attributes,
								lastWriteTime,
								false,
								this.CompressionLevel);
						}

						this.FlushFolder();
						this.FlushCabinet();
					}
					finally
					{
						if (this.CabStream != null)
						{
							this.context.CloseArchiveWriteStream(
								this.currentArchiveNumber,
								this.currentArchiveName,
								this.CabStream);
							this.CabStream = null;
						}

						if (this.FileStream != null)
						{
							this.context.CloseFileReadStream(
								this.currentFileName, this.FileStream);
							this.FileStream = null;
						}
						this.context = null;

						if (this.fciHandle != null)
						{
							this.fciHandle.Dispose();
							this.fciHandle = null;
						}
					}
				}
			}

			internal override int CabOpenStreamEx(string path, int openFlags, int shareMode, out int err, IntPtr pv)
			{
				if (this.CabNumbers.ContainsKey(path))
				{
					Stream stream = this.CabStream;
					if (stream == null)
					{
						short cabNumber = this.CabNumbers[path];

						this.currentFolderTotalBytes = 0;

						stream = this.context.OpenArchiveWriteStream(cabNumber, path, true, this.CabEngine);
						if (stream == null)
						{
							throw new FileNotFoundException(
								String.Format(CultureInfo.InvariantCulture, "Cabinet {0} not provided.", cabNumber));
						}
						this.currentArchiveName = path;

						this.currentArchiveTotalBytes = Math.Min(
							this.totalFolderBytesProcessedInCurrentCab, this.maxCabBytes);
						this.currentArchiveBytesProcessed = 0;

						this.OnProgress(ArchiveProgressType.StartArchive);
						this.CabStream = stream;
					}
					path = CabWorker.CabStreamName;
				}
				else if (path == CabPacker.TempStreamName)
				{
					// Opening memory stream for a temp file.
					Stream stream = new MemoryStream();
					this.tempStreams.Add(stream);
					int streamHandle = this.StreamHandles.AllocHandle(stream);
					err = 0;
					return streamHandle;
				}
				else if (path != CabWorker.CabStreamName)
				{
					// Opening a file on disk for a temp file.
					path = Path.Combine(Path.GetTempPath(), path);
					Stream stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite);
					this.tempStreams.Add(stream);
					stream = new DuplicateStream(stream);
					int streamHandle = this.StreamHandles.AllocHandle(stream);
					err = 0;
					return streamHandle;
				}
				return base.CabOpenStreamEx(path, openFlags, shareMode, out err, pv);
			}

			internal override int CabWriteStreamEx(int streamHandle, IntPtr memory, int cb, out int err, IntPtr pv)
			{
				int count = base.CabWriteStreamEx(streamHandle, memory, cb, out err, pv);
				if (count > 0 && err == 0)
				{
					Stream stream = this.StreamHandles[streamHandle];
					if (DuplicateStream.OriginalStream(stream) ==
						DuplicateStream.OriginalStream(this.CabStream))
					{
						this.currentArchiveBytesProcessed += cb;
						if (this.currentArchiveBytesProcessed > this.currentArchiveTotalBytes)
						{
							this.currentArchiveBytesProcessed = this.currentArchiveTotalBytes;
						}
					}
				}
				return count;
			}

			internal override int CabCloseStreamEx(int streamHandle, out int err, IntPtr pv)
			{
				Stream stream = DuplicateStream.OriginalStream(this.StreamHandles[streamHandle]);

				if (stream == DuplicateStream.OriginalStream(this.FileStream))
				{
					this.context.CloseFileReadStream(this.currentFileName, stream);
					this.FileStream = null;
					long remainder = this.currentFileTotalBytes - this.currentFileBytesProcessed;
					this.currentFileBytesProcessed += remainder;
					this.fileBytesProcessed += remainder;
					this.OnProgress(ArchiveProgressType.FinishFile);

					this.currentFileTotalBytes = 0;
					this.currentFileBytesProcessed = 0;
					this.currentFileName = null;
				}
				else if (stream == DuplicateStream.OriginalStream(this.CabStream))
				{
					if (stream.CanWrite)
					{
						stream.Flush();
					}

					this.currentArchiveBytesProcessed = this.currentArchiveTotalBytes;
					this.OnProgress(ArchiveProgressType.FinishArchive);
					this.currentArchiveNumber++;
					this.totalArchives++;

					this.context.CloseArchiveWriteStream(
						this.currentArchiveNumber,
						this.currentArchiveName,
						stream);

					this.currentArchiveName = this.NextCabinetName;
					this.currentArchiveBytesProcessed = this.currentArchiveTotalBytes = 0;
					this.totalFolderBytesProcessedInCurrentCab = 0;

					this.CabStream = null;
				}
				else  // Must be a temp stream
				{
					stream.Close();
					this.tempStreams.Remove(stream);
				}
				return base.CabCloseStreamEx(streamHandle, out err, pv);
			}

			/// <summary>
			/// Disposes of resources allocated by the cabinet engine.
			/// </summary>
			/// <param name="disposing">If true, the method has been called directly or indirectly by a user's code,
			/// so managed and unmanaged resources will be disposed. If false, the method has been called by the 
			/// runtime from inside the finalizer, and only unmanaged resources will be disposed.</param>
			protected override void Dispose(bool disposing) 
			{
				try
				{
					if (disposing)
					{
						if (this.fciHandle != null)
						{
							this.fciHandle.Dispose();
							this.fciHandle = null;
						}
					}
				}
				finally
				{
					base.Dispose(disposing);
				}
			}

			private static NativeMethods.FCI.TCOMP GetCompressionType(CompressionLevel compLevel)
			{
				if (compLevel < CompressionLevel.Min)
				{
					return NativeMethods.FCI.TCOMP.TYPE_NONE;
				}
				else
				{
					if (compLevel > CompressionLevel.Max)
					{
						compLevel = CompressionLevel.Max;
					}

					int lzxWindowMax =
						((int) NativeMethods.FCI.TCOMP.LZX_WINDOW_HI >> (int) NativeMethods.FCI.TCOMP.SHIFT_LZX_WINDOW) -
						((int) NativeMethods.FCI.TCOMP.LZX_WINDOW_LO >> (int) NativeMethods.FCI.TCOMP.SHIFT_LZX_WINDOW);
					int lzxWindow = lzxWindowMax *
						(compLevel - CompressionLevel.Min) / (CompressionLevel.Max - CompressionLevel.Min);

					return (NativeMethods.FCI.TCOMP) ((int) NativeMethods.FCI.TCOMP.TYPE_LZX |
						((int) NativeMethods.FCI.TCOMP.LZX_WINDOW_LO +
						(lzxWindow << (int) NativeMethods.FCI.TCOMP.SHIFT_LZX_WINDOW)));
				}
			}

			private void AddFile(
				string name,
				Stream stream,
				FileAttributes attributes,
				DateTime lastWriteTime,
				bool execute,
				CompressionLevel compLevel)
			{
				this.FileStream = stream;
				this.fileAttributes = attributes &
					(FileAttributes.Archive | FileAttributes.Hidden | FileAttributes.ReadOnly | FileAttributes.System);
				this.fileLastWriteTime = lastWriteTime;
				this.currentFileName = name;

				NativeMethods.FCI.TCOMP tcomp = CabPacker.GetCompressionType(compLevel);

				IntPtr namePtr = IntPtr.Zero;
				try
				{
					Encoding nameEncoding = Encoding.ASCII;
					if (Encoding.UTF8.GetByteCount(name) > name.Length)
					{
						nameEncoding = Encoding.UTF8;
						this.fileAttributes |= FileAttributes.Normal;  // _A_NAME_IS_UTF
					}

					byte[] nameBytes = nameEncoding.GetBytes(name);
					namePtr = Marshal.AllocHGlobal(nameBytes.Length + 1);
					Marshal.Copy(nameBytes, 0, namePtr, nameBytes.Length);
					Marshal.WriteByte(namePtr, nameBytes.Length, 0);

					this.Erf.Clear();
					NativeMethods.FCI.AddFile(
						this.fciHandle,
						String.Empty,
						namePtr,
						execute,
						this.fciGetNextCabinet,
						this.fciCreateStatus,
						this.fciGetOpenInfo,
						tcomp);
				}
				finally
				{
					if (namePtr != IntPtr.Zero)
					{
						Marshal.FreeHGlobal(namePtr);
					}
				}

				this.CheckError(false);
				this.FileStream = null;
				this.currentFileName = null;
			}

			private void FlushFolder()
			{
				this.Erf.Clear();
				NativeMethods.FCI.FlushFolder(this.fciHandle, this.fciGetNextCabinet, this.fciCreateStatus);
				this.CheckError(false);
			}

			private void FlushCabinet()
			{
				this.Erf.Clear();
				NativeMethods.FCI.FlushCabinet(this.fciHandle, false, this.fciGetNextCabinet, this.fciCreateStatus);
				this.CheckError(false);
			}

			private int CabGetOpenInfo(
				string path,
				out short date,
				out short time,
				out short attribs,
				out int err,
				IntPtr pv)
			{
				CompressionEngine.DateTimeToDosDateAndTime(this.fileLastWriteTime, out date, out time);
				attribs = (short) this.fileAttributes;

				Stream stream = this.FileStream;
				this.FileStream = new DuplicateStream(stream);
				int streamHandle = this.StreamHandles.AllocHandle(stream);
				err = 0;
				return streamHandle;
			}

			private int CabFilePlaced(
				IntPtr pccab,
				string filePath,
				long fileSize,
				int continuation,
				IntPtr pv)
			{
				return 0;
			}

			private int CabGetNextCabinet(IntPtr pccab, uint prevCabSize, IntPtr pv)
			{
				NativeMethods.FCI.CCAB nextCcab = new NativeMethods.FCI.CCAB();
				Marshal.PtrToStructure(pccab, nextCcab);

				nextCcab.szDisk = String.Empty;
				nextCcab.szCab = this.context.GetArchiveName(nextCcab.iCab);
				this.CabNumbers[nextCcab.szCab] = (short) nextCcab.iCab;
				this.NextCabinetName = nextCcab.szCab;

				Marshal.StructureToPtr(nextCcab, pccab, false);
				return 1;
			}

			private int CabCreateStatus(NativeMethods.FCI.STATUS typeStatus, uint cb1, uint cb2, IntPtr pv)
			{
				switch (typeStatus)
				{
					case NativeMethods.FCI.STATUS.FILE:
						if (cb2 > 0 && this.currentFileBytesProcessed < this.currentFileTotalBytes)
						{
							if (this.currentFileBytesProcessed + cb2 > this.currentFileTotalBytes)
							{
								cb2 = (uint) this.currentFileTotalBytes - (uint) this.currentFileBytesProcessed;
							}
							this.currentFileBytesProcessed += cb2;
							this.fileBytesProcessed += cb2;

							this.OnProgress(ArchiveProgressType.PartialFile);
						}
						break;

					case NativeMethods.FCI.STATUS.FOLDER:
						if (cb1 == 0)
						{
							this.currentFolderTotalBytes = cb2 - this.totalFolderBytesProcessedInCurrentCab;
							this.totalFolderBytesProcessedInCurrentCab = cb2;
						}
						else if (this.currentFolderTotalBytes == 0)
						{
							this.OnProgress(ArchiveProgressType.PartialArchive);
						}
						break;

					case NativeMethods.FCI.STATUS.CABINET:
						break;
				}
				return 0;
			}

			private int CabGetTempFile(IntPtr tempNamePtr, int tempNameSize, IntPtr pv)
			{
				string tempFileName;
				if (this.UseTempFiles)
				{
					tempFileName = Path.GetFileName(Path.GetTempFileName());
				}
				else
				{
					tempFileName = CabPacker.TempStreamName;
				}

				byte[] tempNameBytes = Encoding.ASCII.GetBytes(tempFileName);
				if (tempNameBytes.Length >= tempNameSize)
				{
					return -1;
				}

				Marshal.Copy(tempNameBytes, 0, tempNamePtr, tempNameBytes.Length);
				Marshal.WriteByte(tempNamePtr, tempNameBytes.Length, 0);  // null-terminator
				return 1;
			}

			private int CabDeleteFile(string path, out int err, IntPtr pv)
			{
				try
				{
					// Deleting a temp file - don't bother if it is only a memory stream.
					if (path != CabPacker.TempStreamName)
					{
						path = Path.Combine(Path.GetTempPath(), path);
						File.Delete(path);
					}
				}
				catch (IOException)
				{
					// Failure to delete a temp file is not fatal.
				}
				err = 0;
				return 1;
			}
		}

		internal class CabUnpacker : CabWorker
		{
			private NativeMethods.FDI.Handle fdiHandle;

			// These delegates need to be saved as member variables
			// so that they don't get GC'd.
			private NativeMethods.FDI.PFNALLOC fdiAllocMemHandler;
			private NativeMethods.FDI.PFNFREE fdiFreeMemHandler;
			private NativeMethods.FDI.PFNOPEN fdiOpenStreamHandler;
			private NativeMethods.FDI.PFNREAD fdiReadStreamHandler;
			private NativeMethods.FDI.PFNWRITE fdiWriteStreamHandler;
			private NativeMethods.FDI.PFNCLOSE fdiCloseStreamHandler;
			private NativeMethods.FDI.PFNSEEK fdiSeekStreamHandler;

			private IUnpackStreamContext context;

			private List<ArchiveFileInfo> fileList;

			private int folderId;

			private Predicate<string> filter;

			public CabUnpacker(CabEngine cabEngine)
				: base(cabEngine)
			{
				this.fdiAllocMemHandler = this.CabAllocMem;
				this.fdiFreeMemHandler = this.CabFreeMem;
				this.fdiOpenStreamHandler = this.CabOpenStream;
				this.fdiReadStreamHandler = this.CabReadStream;
				this.fdiWriteStreamHandler = this.CabWriteStream;
				this.fdiCloseStreamHandler = this.CabCloseStream;
				this.fdiSeekStreamHandler = this.CabSeekStream;

				this.fdiHandle = NativeMethods.FDI.Create(
					this.fdiAllocMemHandler,
					this.fdiFreeMemHandler,
					this.fdiOpenStreamHandler,
					this.fdiReadStreamHandler,
					this.fdiWriteStreamHandler,
					this.fdiCloseStreamHandler,
					this.fdiSeekStreamHandler,
					NativeMethods.FDI.CPU_80386,
					this.ErfHandle.AddrOfPinnedObject());
				if (this.Erf.Error)
				{
					int error = this.Erf.Oper;
					int errorCode = this.Erf.Type;
					this.ErfHandle.Free();
					throw new CabException(
						error,
						errorCode,
						CabException.GetErrorMessage(error, errorCode, true));
				}
			}

			public bool IsArchive(Stream stream)
			{
				if (stream == null)
				{
					throw new ArgumentNullException("stream");
				}

				lock (this)
				{
					short id;
					int folderCount, fileCount;
					return this.IsCabinet(stream, out id, out folderCount, out fileCount);
				}
			}

			public IList<ArchiveFileInfo> GetFileInfo(
				IUnpackStreamContext streamContext,
				Predicate<string> fileFilter)
			{
				if (streamContext == null)
				{
					throw new ArgumentNullException("streamContext");
				}

				lock (this)
				{
					this.context = streamContext;
					this.filter = fileFilter;
					this.NextCabinetName = String.Empty;
					this.fileList = new List<ArchiveFileInfo>();
					bool tmpSuppress = this.SuppressProgressEvents;
					this.SuppressProgressEvents = true;
					try
					{
						for (short cabNumber = 0;
							 this.NextCabinetName != null;
							 cabNumber++)
						{
							this.Erf.Clear();
							this.CabNumbers[this.NextCabinetName] = cabNumber;
							
							NativeMethods.FDI.Copy(
								this.fdiHandle,
								this.NextCabinetName,
								String.Empty,
								0,
								this.CabListNotify,
								IntPtr.Zero,
								IntPtr.Zero);
							this.CheckError(true);
						}

						List<ArchiveFileInfo> tmpFileList = this.fileList;
						this.fileList = null;
						return tmpFileList.AsReadOnly();
					}
					finally
					{
						this.SuppressProgressEvents = tmpSuppress;

						if (this.CabStream != null)
						{
							this.context.CloseArchiveReadStream(
								this.currentArchiveNumber,
								this.currentArchiveName,
								this.CabStream);
							this.CabStream = null;
						}

						this.context = null;
					}
				}
			}

			public void Unpack(
				IUnpackStreamContext streamContext,
				Predicate<string> fileFilter)
			{
				lock (this)
				{
					IList<ArchiveFileInfo> files =
						this.GetFileInfo(streamContext, fileFilter);

					this.ResetProgressData();

					if (files != null)
					{
						this.totalFiles = files.Count;

						for (int i = 0; i < files.Count; i++)
						{
							this.totalFileBytes += files[i].Length;
							if (files[i].ArchiveNumber >= this.totalArchives)
							{
								int totalArchives = files[i].ArchiveNumber + 1;
								this.totalArchives = (short) totalArchives;
							}
						}
					}

					this.context = streamContext;
					this.fileList = null;
					this.NextCabinetName = String.Empty;
					this.folderId = -1;
					this.currentFileNumber = -1;

					try
					{
						for (short cabNumber = 0;
							 this.NextCabinetName != null;
							 cabNumber++)
						{
							this.Erf.Clear();
							this.CabNumbers[this.NextCabinetName] = cabNumber;

							NativeMethods.FDI.Copy(
								this.fdiHandle,
								this.NextCabinetName,
								String.Empty,
								0,
								this.CabExtractNotify,
								IntPtr.Zero,
								IntPtr.Zero);
							this.CheckError(true);
						}
					}
					finally
					{
						if (this.CabStream != null)
						{
							this.context.CloseArchiveReadStream(
								this.currentArchiveNumber,
								this.currentArchiveName,
								this.CabStream);
							this.CabStream = null;
						}

						if (this.FileStream != null)
						{
							this.context.CloseFileWriteStream(this.currentFileName, this.FileStream, FileAttributes.Normal, DateTime.Now);
							this.FileStream = null;
						}

						this.context = null;
					}
				}
			}

			internal override int CabOpenStreamEx(string path, int openFlags, int shareMode, out int err, IntPtr pv)
			{
				if (this.CabNumbers.ContainsKey(path))
				{
					Stream stream = this.CabStream;
					if (stream == null)
					{
						short cabNumber = this.CabNumbers[path];

						stream = this.context.OpenArchiveReadStream(cabNumber, path, this.CabEngine);
						if (stream == null)
						{
							throw new FileNotFoundException(String.Format(CultureInfo.InvariantCulture, "Cabinet {0} not provided.", cabNumber));
						}
						this.currentArchiveName = path;
						this.currentArchiveNumber = cabNumber;
						if (this.totalArchives <= this.currentArchiveNumber)
						{
							int totalArchives = this.currentArchiveNumber + 1;
							this.totalArchives = (short) totalArchives;
						}
						this.currentArchiveTotalBytes = stream.Length;
						this.currentArchiveBytesProcessed = 0;

						if (this.folderId != -3)  // -3 is a special folderId that requires re-opening the same cab
						{
							this.OnProgress(ArchiveProgressType.StartArchive);
						}
						this.CabStream = stream;
					}
					path = CabWorker.CabStreamName;
				}
				return base.CabOpenStreamEx(path, openFlags, shareMode, out err, pv);
			}

			internal override int CabReadStreamEx(int streamHandle, IntPtr memory, int cb, out int err, IntPtr pv)
			{
				int count = base.CabReadStreamEx(streamHandle, memory, cb, out err, pv);
				if (err == 0 && this.CabStream != null)
				{
					if (this.fileList == null)
					{
						Stream stream = this.StreamHandles[streamHandle];
						if (DuplicateStream.OriginalStream(stream) ==
							DuplicateStream.OriginalStream(this.CabStream))
						{
							this.currentArchiveBytesProcessed += cb;
							if (this.currentArchiveBytesProcessed > this.currentArchiveTotalBytes)
							{
								this.currentArchiveBytesProcessed = this.currentArchiveTotalBytes;
							}
						}
					}
				}
				return count;
			}

			internal override int CabWriteStreamEx(int streamHandle, IntPtr memory, int cb, out int err, IntPtr pv)
			{
				int count = base.CabWriteStreamEx(streamHandle, memory, cb, out err, pv);
				if (count > 0 && err == 0)
				{
					this.currentFileBytesProcessed += cb;
					this.fileBytesProcessed += cb;
					this.OnProgress(ArchiveProgressType.PartialFile);
				}
				return count;
			}

			internal override int CabCloseStreamEx(int streamHandle, out int err, IntPtr pv)
			{
				Stream stream = DuplicateStream.OriginalStream(this.StreamHandles[streamHandle]);

				if (stream == DuplicateStream.OriginalStream(this.CabStream))
				{
					if (this.folderId != -3)  // -3 is a special folderId that requires re-opening the same cab
					{
						this.OnProgress(ArchiveProgressType.FinishArchive);
					}

					this.context.CloseArchiveReadStream(this.currentArchiveNumber, this.currentArchiveName, stream);

					this.currentArchiveName = this.NextCabinetName;
					this.currentArchiveBytesProcessed = this.currentArchiveTotalBytes = 0;

					this.CabStream = null;
				}
				return base.CabCloseStreamEx(streamHandle, out err, pv);
			}

			/// <summary>
			/// Disposes of resources allocated by the cabinet engine.
			/// </summary>
			/// <param name="disposing">If true, the method has been called directly or indirectly by a user's code,
			/// so managed and unmanaged resources will be disposed. If false, the method has been called by the 
			/// runtime from inside the finalizer, and only unmanaged resources will be disposed.</param>
			[SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
			protected override void Dispose(bool disposing)
			{
				try
				{
					if (disposing)
					{
						if (this.fdiHandle != null)
						{
							this.fdiHandle.Dispose();
							this.fdiHandle = null;
						}
					}
				}
				finally
				{
					base.Dispose(disposing);
				}
			}

			private static string GetFileName(NativeMethods.FDI.NOTIFICATION notification)
			{
				bool utf8Name = (notification.attribs & (ushort) FileAttributes.Normal) != 0;  // _A_NAME_IS_UTF

				// Non-utf8 names should be completely ASCII. But for compatibility with
				// legacy tools, interpret them using the current (Default) ANSI codepage.
				Encoding nameEncoding = utf8Name ? Encoding.UTF8 : Encoding.Default;

				// Find how many bytes are in the string.
				// Unfortunately there is no faster way.
				int nameBytesCount = 0;
				while (Marshal.ReadByte(notification.psz1, nameBytesCount) != 0)
				{
					nameBytesCount++;
				}

				byte[] nameBytes = new byte[nameBytesCount];
				Marshal.Copy(notification.psz1, nameBytes, 0, nameBytesCount);
				string name = nameEncoding.GetString(nameBytes);
				if (Path.IsPathRooted(name))
				{
					name = name.Replace("" + Path.VolumeSeparatorChar, "");
				}

				return name;
			}

			private bool IsCabinet(Stream cabStream, out short id, out int cabFolderCount, out int fileCount)
			{
				int streamHandle = this.StreamHandles.AllocHandle(cabStream);
				try
				{
					this.Erf.Clear();
					NativeMethods.FDI.CABINFO fdici;
					bool isCabinet = 0 != NativeMethods.FDI.IsCabinet(this.fdiHandle, streamHandle, out fdici);

					if (this.Erf.Error)
					{
						if (((NativeMethods.FDI.ERROR) this.Erf.Oper) == NativeMethods.FDI.ERROR.UNKNOWN_CABINET_VERSION)
						{
							isCabinet = false;
						}
						else
						{
							throw new CabException(
								this.Erf.Oper,
								this.Erf.Type,
								CabException.GetErrorMessage(this.Erf.Oper, this.Erf.Type, true));
						}
					}

					id = fdici.setID;
					cabFolderCount = (int) fdici.cFolders;
					fileCount = (int) fdici.cFiles;
					return isCabinet;
				}
				finally
				{
					this.StreamHandles.FreeHandle(streamHandle);
				}
			}

			private int CabListNotify(NativeMethods.FDI.NOTIFICATIONTYPE notificationType, NativeMethods.FDI.NOTIFICATION notification)
			{
				switch (notificationType)
				{
					case NativeMethods.FDI.NOTIFICATIONTYPE.CABINET_INFO:
						{
							string nextCab = Marshal.PtrToStringAnsi(notification.psz1);
							this.NextCabinetName = (nextCab.Length != 0 ? nextCab : null);
							return 0;  // Continue
						}
					case NativeMethods.FDI.NOTIFICATIONTYPE.PARTIAL_FILE:
						{
							// This notification can occur when examining the contents of a non-first cab file.
							return 0;  // Continue
						}
					case NativeMethods.FDI.NOTIFICATIONTYPE.COPY_FILE:
						{
							//bool execute = (notification.attribs & (ushort) FileAttributes.Device) != 0;  // _A_EXEC

							string name = CabUnpacker.GetFileName(notification);

							if (this.filter == null || this.filter(name))
							{
								if (this.fileList != null)
								{
									FileAttributes attributes = (FileAttributes) notification.attribs &
										(FileAttributes.Archive | FileAttributes.Hidden | FileAttributes.ReadOnly | FileAttributes.System);
									if (attributes == (FileAttributes) 0)
									{
										attributes = FileAttributes.Normal;
									}
									DateTime lastWriteTime;
									CompressionEngine.DosDateAndTimeToDateTime(notification.date, notification.time, out lastWriteTime);
									long length = notification.cb;

									CabFileInfo fileInfo = new CabFileInfo(
										name,
										notification.iFolder,
										notification.iCabinet,
										attributes,
										lastWriteTime,
										length);
									this.fileList.Add(fileInfo);
									this.currentFileNumber = this.fileList.Count - 1;
									this.fileBytesProcessed += notification.cb;
								}
							}

							this.totalFiles++;
							this.totalFileBytes += notification.cb;
							return 0;  // Continue
						}
				}
				return 0;
			}

			private int CabExtractNotify(NativeMethods.FDI.NOTIFICATIONTYPE notificationType, NativeMethods.FDI.NOTIFICATION notification)
			{
				switch (notificationType)
				{
					case NativeMethods.FDI.NOTIFICATIONTYPE.CABINET_INFO:
						{
							if (this.NextCabinetName != null && this.NextCabinetName.StartsWith("?", StringComparison.Ordinal))
							{
								// We are just continuing the copy of a file that spanned cabinets.
								// The next cabinet name needs to be preserved.
								this.NextCabinetName = this.NextCabinetName.Substring(1);
							}
							else
							{
								string nextCab = Marshal.PtrToStringAnsi(notification.psz1);
								this.NextCabinetName = (nextCab.Length != 0 ? nextCab : null);
							}
							return 0;  // Continue
						}
					case NativeMethods.FDI.NOTIFICATIONTYPE.NEXT_CABINET:
						{
							string nextCab = Marshal.PtrToStringAnsi(notification.psz1);
							this.CabNumbers[nextCab] = (short) notification.iCabinet;
							this.NextCabinetName = "?" + this.NextCabinetName;
							return 0;  // Continue
						}
					case NativeMethods.FDI.NOTIFICATIONTYPE.COPY_FILE:
						{
							return this.CabExtractCopyFile(notification);
						}
					case NativeMethods.FDI.NOTIFICATIONTYPE.CLOSE_FILE_INFO:
						{
							return this.CabExtractCloseFile(notification);
						}
				}
				return 0;
			}

			private int CabExtractCopyFile(NativeMethods.FDI.NOTIFICATION notification)
			{
				if (notification.iFolder != this.folderId)
				{
					if (notification.iFolder != -3)  // -3 is a special folderId used when continuing a folder from a previous cab
					{
						if (this.folderId != -1) // -1 means we just started the extraction sequence
						{
							this.currentFolderNumber++;
						}
					}
					this.folderId = notification.iFolder;
				}

				//bool execute = (notification.attribs & (ushort) FileAttributes.Device) != 0;  // _A_EXEC

				string name = CabUnpacker.GetFileName(notification);

				if (this.filter == null || this.filter(name))
				{
					this.currentFileNumber++;
					this.currentFileName = name;

					this.currentFileBytesProcessed = 0;
					this.currentFileTotalBytes = notification.cb;
					this.OnProgress(ArchiveProgressType.StartFile);

					DateTime lastWriteTime;
					CompressionEngine.DosDateAndTimeToDateTime(notification.date, notification.time, out lastWriteTime);

					Stream stream = this.context.OpenFileWriteStream(name, notification.cb, lastWriteTime);
					if (stream != null)
					{
						this.FileStream = stream;
						int streamHandle = this.StreamHandles.AllocHandle(stream);
						return streamHandle;
					}
					else
					{
						this.fileBytesProcessed += notification.cb;
						this.OnProgress(ArchiveProgressType.FinishFile);
						this.currentFileName = null;
					}
				}
				return 0;  // Continue
			}

			private int CabExtractCloseFile(NativeMethods.FDI.NOTIFICATION notification)
			{
				Stream stream = this.StreamHandles[notification.hf];
				this.StreamHandles.FreeHandle(notification.hf);

				//bool execute = (notification.attribs & (ushort) FileAttributes.Device) != 0;  // _A_EXEC

				string name = CabUnpacker.GetFileName(notification);

				FileAttributes attributes = (FileAttributes) notification.attribs &
					(FileAttributes.Archive | FileAttributes.Hidden | FileAttributes.ReadOnly | FileAttributes.System);
				if (attributes == (FileAttributes) 0)
				{
					attributes = FileAttributes.Normal;
				}
				DateTime lastWriteTime;
				CompressionEngine.DosDateAndTimeToDateTime(notification.date, notification.time, out lastWriteTime);

				stream.Flush();
				this.context.CloseFileWriteStream(name, stream, attributes, lastWriteTime);
				this.FileStream = null;

				long remainder = this.currentFileTotalBytes - this.currentFileBytesProcessed;
				this.currentFileBytesProcessed += remainder;
				this.fileBytesProcessed += remainder;
				this.OnProgress(ArchiveProgressType.FinishFile);
				this.currentFileName = null;

				return 1;  // Continue
			}
		}

		internal abstract class CabWorker : IDisposable
		{
			internal const string CabStreamName = "%%CAB%%";

			private CabEngine cabEngine;

			private HandleManager<Stream> streamHandles;
			private Stream cabStream;
			private Stream fileStream;

			private NativeMethods.ERF erf;
			private GCHandle erfHandle;

			private IDictionary<string, short> cabNumbers;
			private string nextCabinetName;

			private bool suppressProgressEvents;

			private byte[] buf;

			// Progress data
			protected string currentFileName;
			protected int    currentFileNumber;
			protected int    totalFiles;
			protected long   currentFileBytesProcessed;
			protected long   currentFileTotalBytes;
			protected short  currentFolderNumber;
			protected long   currentFolderTotalBytes;
			protected string currentArchiveName;
			protected short  currentArchiveNumber;
			protected short  totalArchives;
			protected long   currentArchiveBytesProcessed;
			protected long   currentArchiveTotalBytes;
			protected long   fileBytesProcessed;
			protected long   totalFileBytes;

			[SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
			protected CabWorker(CabEngine cabEngine)
			{
				this.cabEngine = cabEngine;
				this.streamHandles = new HandleManager<Stream>();
				this.erf = new NativeMethods.ERF();
				this.erfHandle = GCHandle.Alloc(this.erf, GCHandleType.Pinned);
				this.cabNumbers = new Dictionary<string, short>(1);

				// 32K seems to be the size of the largest chunks processed by cabinet.dll.
				// But just in case, this buffer will auto-enlarge.
				this.buf = new byte[32768];
			}

			~CabWorker()
			{
				this.Dispose(false);
			}

			public CabEngine CabEngine
			{
				get
				{
					return this.cabEngine;
				}
			}

			internal NativeMethods.ERF Erf
			{
				get
				{
					return this.erf;
				}
			}

			internal GCHandle ErfHandle
			{
				get
				{
					return this.erfHandle;
				}
			}

			internal HandleManager<Stream> StreamHandles
			{
				get
				{
					return this.streamHandles;
				}
			}

			internal bool SuppressProgressEvents
			{
				get
				{
					return this.suppressProgressEvents;
				}

				set
				{
					this.suppressProgressEvents = value;
				}
			}

			internal IDictionary<string, short> CabNumbers
			{
				get
				{
					return this.cabNumbers;
				}
			}

			internal string NextCabinetName
			{
				get
				{
					return this.nextCabinetName;
				}

				set
				{
					this.nextCabinetName = value;
				}
			}

			internal Stream CabStream
			{
				get
				{
					return this.cabStream;
				}

				set
				{
					this.cabStream = value;
				}
			}

			internal Stream FileStream
			{
				get
				{
					return this.fileStream;
				}

				set
				{
					this.fileStream = value;
				}
			}

			public void Dispose() 
			{
				this.Dispose(true);
				GC.SuppressFinalize(this); 
			}

			protected void ResetProgressData()
			{
				this.currentFileName = null;
				this.currentFileNumber = 0;
				this.totalFiles = 0;
				this.currentFileBytesProcessed = 0;
				this.currentFileTotalBytes = 0;
				this.currentFolderNumber = 0;
				this.currentFolderTotalBytes = 0;
				this.currentArchiveName = null;
				this.currentArchiveNumber = 0;
				this.totalArchives = 0;
				this.currentArchiveBytesProcessed = 0;
				this.currentArchiveTotalBytes = 0;
				this.fileBytesProcessed = 0;
				this.totalFileBytes = 0;
			}

			protected void OnProgress(ArchiveProgressType progressType)
			{
				if (!this.suppressProgressEvents)
				{
					ArchiveProgressEventArgs e = new ArchiveProgressEventArgs(
						progressType,
						this.currentFileName,
						this.currentFileNumber >= 0 ? this.currentFileNumber : 0,
						this.totalFiles,
						this.currentFileBytesProcessed,
						this.currentFileTotalBytes,
						this.currentArchiveName,
						this.currentArchiveNumber,
						this.totalArchives,
						this.currentArchiveBytesProcessed,
						this.currentArchiveTotalBytes,
						this.fileBytesProcessed,
						this.totalFileBytes);
					this.CabEngine.ReportProgress(e);
				}
			}

			internal IntPtr CabAllocMem(int byteCount)
			{
				IntPtr memPointer = Marshal.AllocHGlobal((IntPtr) byteCount);
				return memPointer;
			}

			internal void CabFreeMem(IntPtr memPointer)
			{
				Marshal.FreeHGlobal(memPointer);
			}

			internal int CabOpenStream(string path, int openFlags, int shareMode)
			{
				int err; return this.CabOpenStreamEx(path, openFlags, shareMode, out err, IntPtr.Zero);
			}

			internal virtual int CabOpenStreamEx(string path, int openFlags, int shareMode, out int err, IntPtr pv)
			{
				path = path.Trim();
				Stream stream = this.cabStream;
				this.cabStream = new DuplicateStream(stream);
				int streamHandle = this.streamHandles.AllocHandle(stream);
				err = 0;
				return streamHandle;
			}

			internal int CabReadStream(int streamHandle, IntPtr memory, int cb)
			{
				int err; return this.CabReadStreamEx(streamHandle, memory, cb, out err, IntPtr.Zero);
			}

			internal virtual int CabReadStreamEx(int streamHandle, IntPtr memory, int cb, out int err, IntPtr pv)
			{
				Stream stream = this.streamHandles[streamHandle];
				int count = (int) cb;
				if (count > this.buf.Length)
				{
					this.buf = new byte[count];
				}
				count = stream.Read(this.buf, 0, count);
				Marshal.Copy(this.buf, 0, memory, count);
				err = 0;
				return count;
			}

			internal int CabWriteStream(int streamHandle, IntPtr memory, int cb)
			{
				int err; return this.CabWriteStreamEx(streamHandle, memory, cb, out err, IntPtr.Zero);
			}

			internal virtual int CabWriteStreamEx(int streamHandle, IntPtr memory, int cb, out int err, IntPtr pv)
			{
				Stream stream = this.streamHandles[streamHandle];
				int count = (int) cb;
				if (count > this.buf.Length)
				{
					this.buf = new byte[count];
				}
				Marshal.Copy(memory, this.buf, 0, count);
				stream.Write(this.buf, 0, count);
				err = 0;
				return cb;
			}

			internal int CabCloseStream(int streamHandle)
			{
				int err; return this.CabCloseStreamEx(streamHandle, out err, IntPtr.Zero);
			}

			internal virtual int CabCloseStreamEx(int streamHandle, out int err, IntPtr pv)
			{
				this.streamHandles.FreeHandle(streamHandle);
				err = 0;
				return 0;
			}

			internal int CabSeekStream(int streamHandle, int offset, int seekOrigin)
			{
				int err; return this.CabSeekStreamEx(streamHandle, offset, seekOrigin, out err, IntPtr.Zero);
			}

			internal virtual int CabSeekStreamEx(int streamHandle, int offset, int seekOrigin, out int err, IntPtr pv)
			{
				Stream stream = this.streamHandles[streamHandle];
				offset = (int) stream.Seek(offset, (SeekOrigin) seekOrigin);
				err = 0;
				return offset;
			}

			/// <summary>
			/// Disposes of resources allocated by the cabinet engine.
			/// </summary>
			/// <param name="disposing">If true, the method has been called directly or indirectly by a user's code,
			/// so managed and unmanaged resources will be disposed. If false, the method has been called by the 
			/// runtime from inside the finalizer, and only unmanaged resources will be disposed.</param>
			[SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
			protected virtual void Dispose(bool disposing) 
			{
				if (disposing) 
				{
					if (this.cabStream != null)
					{
						this.cabStream.Close();
						this.cabStream = null;
					}

					if (this.fileStream != null)
					{
						this.fileStream.Close();
						this.fileStream = null;
					}
				}

				if (this.erfHandle.IsAllocated)
				{
					this.erfHandle.Free();
				}
			}

			protected void CheckError(bool extracting)
			{
				if (this.Erf.Error)
				{
					throw new CabException(
						this.Erf.Oper,
						this.Erf.Type,
						CabException.GetErrorMessage(this.Erf.Oper, this.Erf.Type, extracting));
				}
			}
		}

		/// <summary>
		/// Wraps a source stream and carries additional items that are disposed when the stream is closed.
		/// </summary>
		public class CargoStream : Stream
		{
			private Stream source;
			private List<IDisposable> cargo;

			/// <summary>
			/// Creates a new a cargo stream.
			/// </summary>
			/// <param name="source">source of the stream</param>
			/// <param name="cargo">List of additional items that are disposed when the stream is closed.
			/// The order of the list is the order in which the items are disposed.</param>
			public CargoStream(Stream source, params IDisposable[] cargo)
			{
				if (source == null)
				{
					throw new ArgumentNullException("source");
				}

				this.source = source;
				this.cargo = new List<IDisposable>(cargo);
			}

			/// <summary>
			/// Gets the source stream of the cargo stream.
			/// </summary>
			public Stream Source
			{
				get
				{
					return this.source;
				}
			}

			/// <summary>
			/// Gets the list of additional items that are disposed when the stream is closed.
			/// The order of the list is the order in which the items are disposed. The contents can be modified any time.
			/// </summary>
			public IList<IDisposable> Cargo
			{
				get
				{
					return this.cargo;
				}
			}

			/// <summary>
			/// Gets a value indicating whether the source stream supports reading.
			/// </summary>
			/// <value>true if the stream supports reading; otherwise, false.</value>
			public override bool CanRead
			{
				get
				{
					return this.source.CanRead;
				}
			}

			/// <summary>
			/// Gets a value indicating whether the source stream supports writing.
			/// </summary>
			/// <value>true if the stream supports writing; otherwise, false.</value>
			public override bool CanWrite
			{
				get
				{
					return this.source.CanWrite;
				}
			}

			/// <summary>
			/// Gets a value indicating whether the source stream supports seeking.
			/// </summary>
			/// <value>true if the stream supports seeking; otherwise, false.</value>
			public override bool CanSeek
			{
				get
				{
					return this.source.CanSeek;
				}
			}

			/// <summary>
			/// Gets the length of the source stream.
			/// </summary>
			public override long Length
			{
				get
				{
					return this.source.Length;
				}
			}

			/// <summary>
			/// Gets or sets the position of the source stream.
			/// </summary>
			public override long Position
			{
				get
				{
					return this.source.Position;
				}

				set
				{
					this.source.Position = value;
				}
			}

			/// <summary>
			/// Flushes the source stream.
			/// </summary>
			public override void Flush()
			{
				this.source.Flush();
			}

			/// <summary>
			/// Sets the length of the source stream.
			/// </summary>
			/// <param name="value">The desired length of the stream in bytes.</param>
			public override void SetLength(long value)
			{
				this.source.SetLength(value);
			}

			/// <summary>
			/// Closes the source stream and also closes the additional objects that are carried.
			/// </summary>
			public override void Close()
			{
				this.source.Close();

				foreach (IDisposable cargoObject in this.cargo)
				{
					cargoObject.Dispose();
				}
			}

			/// <summary>
			/// Reads from the source stream.
			/// </summary>
			/// <param name="buffer">An array of bytes. When this method returns, the buffer
			/// contains the specified byte array with the values between offset and
			/// (offset + count - 1) replaced by the bytes read from the source.</param>
			/// <param name="offset">The zero-based byte offset in buffer at which to begin
			/// storing the data read from the stream.</param>
			/// <param name="count">The maximum number of bytes to be read from the stream.</param>
			/// <returns>The total number of bytes read into the buffer. This can be less
			/// than the number of bytes requested if that many bytes are not currently available,
			/// or zero (0) if the end of the stream has been reached.</returns>
			public override int Read(byte[] buffer, int offset, int count)
			{
				return this.source.Read(buffer, offset, count);
			}

			/// <summary>
			/// Writes to the source stream.
			/// </summary>
			/// <param name="buffer">An array of bytes. This method copies count
			/// bytes from buffer to the stream.</param>
			/// <param name="offset">The zero-based byte offset in buffer at which
			/// to begin copying bytes to the stream.</param>
			/// <param name="count">The number of bytes to be written to the stream.</param>
			public override void Write(byte[] buffer, int offset, int count)
			{
				this.source.Write(buffer, offset, count);
			}

			/// <summary>
			/// Changes the position of the source stream.
			/// </summary>
			/// <param name="offset">A byte offset relative to the origin parameter.</param>
			/// <param name="origin">A value of type SeekOrigin indicating the reference
			/// point used to obtain the new position.</param>
			/// <returns>The new position within the stream.</returns>
			public override long Seek(long offset, SeekOrigin origin)
			{
				return this.source.Seek(offset, origin);
			}
		}

		/// <summary>
		/// Base class for an engine capable of packing and unpacking a particular
		/// compressed file format.
		/// </summary>
		public abstract class CompressionEngine : IDisposable
		{
			private CompressionLevel compressionLevel;
			private bool dontUseTempFiles;

			/// <summary>
			/// Creates a new instance of the compression engine base class.
			/// </summary>
			protected CompressionEngine()
			{
				this.compressionLevel = CompressionLevel.Normal;
			}

			/// <summary>
			/// Disposes the compression engine.
			/// </summary>
			~CompressionEngine()
			{
				this.Dispose(false);
			}

			/// <summary>
			/// Occurs when the compression engine reports progress in packing
			/// or unpacking an archive.
			/// </summary>
			/// <seealso cref="ArchiveProgressType"/>
			public event EventHandler<ArchiveProgressEventArgs> Progress;

			/// <summary>
			/// Gets or sets a flag indicating whether temporary files are created
			/// and used during compression.
			/// </summary>
			/// <value>True if temporary files are used; false if compression is done
			/// entirely in-memory.</value>
			/// <remarks>The value of this property is true by default. Using temporary
			/// files can greatly reduce the memory requirement of compression,
			/// especially when compressing large archives. However, setting this property
			/// to false may yield slightly better performance when creating small
			/// archives. Or it may be necessary if the process does not have sufficient
			/// privileges to create temporary files.</remarks>
			public bool UseTempFiles
			{
				get
				{
					return !this.dontUseTempFiles;
				}

				set
				{
					this.dontUseTempFiles = !value;
				}
			}

			/// <summary>
			/// Compression level to use when compressing files.
			/// </summary>
			/// <value>A compression level ranging from minimum to maximum compression,
			/// or no compression.</value>
			public CompressionLevel CompressionLevel
			{
				get
				{
					return this.compressionLevel;
				}

				set
				{
					this.compressionLevel = value;
				}
			}

			/// <summary>
			/// Disposes of resources allocated by the compression engine.
			/// </summary>
			public void Dispose()
			{
				this.Dispose(true);
				GC.SuppressFinalize(this);
			}

			/// <summary>
			/// Creates an archive.
			/// </summary>
			/// <param name="streamContext">A context interface to handle opening
			/// and closing of archive and file streams.</param>
			/// <param name="files">The paths of the files in the archive
			/// (not external file paths).</param>
			/// <exception cref="ArchiveException">The archive could not be
			/// created.</exception>
			/// <remarks>
			/// The stream context implementation may provide a mapping from the
			/// file paths within the archive to the external file paths.
			/// </remarks>
			public void Pack(IPackStreamContext streamContext, IEnumerable<string> files)
			{
				if (files == null)
				{
					throw new ArgumentNullException("files");
				}

				this.Pack(streamContext, files, 0);
			}

			/// <summary>
			/// Creates an archive or chain of archives.
			/// </summary>
			/// <param name="streamContext">A context interface to handle opening
			/// and closing of archive and file streams.</param>
			/// <param name="files">The paths of the files in the archive (not
			/// external file paths).</param>
			/// <param name="maxArchiveSize">The maximum number of bytes for one
			/// archive before the contents are chained to the next archive, or zero
			/// for unlimited archive size.</param>
			/// <exception cref="ArchiveException">The archive could not be
			/// created.</exception>
			/// <remarks>
			/// The stream context implementation may provide a mapping from the file
			/// paths within the archive to the external file paths.
			/// </remarks>
			public abstract void Pack(
				IPackStreamContext streamContext,
				IEnumerable<string> files,
				long maxArchiveSize);

			/// <summary>
			/// Checks whether a Stream begins with a header that indicates
			/// it is a valid archive.
			/// </summary>
			/// <param name="stream">Stream for reading the archive file.</param>
			/// <returns>True if the stream is a valid archive
			/// (with no offset); false otherwise.</returns>
			public abstract bool IsArchive(Stream stream);

			/// <summary>
			/// Gets the offset of an archive that is positioned 0 or more bytes
			/// from the start of the Stream.
			/// </summary>
			/// <param name="stream">A stream for reading the archive.</param>
			/// <returns>The offset in bytes of the archive,
			/// or -1 if no archive is found in the Stream.</returns>
			/// <remarks>The archive must begin on a 4-byte boundary.</remarks>
			public virtual long FindArchiveOffset(Stream stream)
			{
				if (stream == null)
				{
					throw new ArgumentNullException("stream");
				}

				long sectionSize = 4;
				long length = stream.Length;
				for (long offset = 0; offset <= length - sectionSize; offset += sectionSize)
				{
					stream.Seek(offset, SeekOrigin.Begin);
					if (this.IsArchive(stream))
					{
						return offset;
					}
				}

				return -1;
			}

			/// <summary>
			/// Gets information about all files in an archive stream.
			/// </summary>
			/// <param name="stream">A stream for reading the archive.</param>
			/// <returns>Information about all files in the archive stream.</returns>
			/// <exception cref="ArchiveException">The stream is not a valid
			/// archive.</exception>
			public IList<ArchiveFileInfo> GetFileInfo(Stream stream)
			{
				return this.GetFileInfo(new BasicUnpackStreamContext(stream), null);
			}

			/// <summary>
			/// Gets information about files in an archive or archive chain.
			/// </summary>
			/// <param name="streamContext">A context interface to handle opening
			/// and closing of archive and file streams.</param>
			/// <param name="fileFilter">A predicate that can determine
			/// which files to process, optional.</param>
			/// <returns>Information about files in the archive stream.</returns>
			/// <exception cref="ArchiveException">The archive provided
			/// by the stream context is not valid.</exception>
			/// <remarks>
			/// The <paramref name="fileFilter"/> predicate takes an internal file
			/// path and returns true to include the file or false to exclude it.
			/// </remarks>
			public abstract IList<ArchiveFileInfo> GetFileInfo(
				IUnpackStreamContext streamContext,
				Predicate<string> fileFilter);

			/// <summary>
			/// Gets the list of files in an archive Stream.
			/// </summary>
			/// <param name="stream">A stream for reading the archive.</param>
			/// <returns>A list of the paths of all files contained in the
			/// archive.</returns>
			/// <exception cref="ArchiveException">The stream is not a valid
			/// archive.</exception>
			public IList<string> GetFiles(Stream stream)
			{
				return this.GetFiles(new BasicUnpackStreamContext(stream), null);
			}

			/// <summary>
			/// Gets the list of files in an archive or archive chain.
			/// </summary>
			/// <param name="streamContext">A context interface to handle opening
			/// and closing of archive and file streams.</param>
			/// <param name="fileFilter">A predicate that can determine
			/// which files to process, optional.</param>
			/// <returns>An array containing the names of all files contained in
			/// the archive or archive chain.</returns>
			/// <exception cref="ArchiveException">The archive provided
			/// by the stream context is not valid.</exception>
			/// <remarks>
			/// The <paramref name="fileFilter"/> predicate takes an internal file
			/// path and returns true to include the file or false to exclude it.
			/// </remarks>
			public IList<string> GetFiles(
				IUnpackStreamContext streamContext,
				Predicate<string> fileFilter)
			{
				if (streamContext == null)
				{
					throw new ArgumentNullException("streamContext");
				}

				IList<ArchiveFileInfo> files =
					this.GetFileInfo(streamContext, fileFilter);
				IList<string> fileNames = new List<string>(files.Count);
				for (int i = 0; i < files.Count; i++)
				{
					fileNames.Add(files[i].Name);
				}

				return fileNames;
			}

			/// <summary>
			/// Reads a single file from an archive stream.
			/// </summary>
			/// <param name="stream">A stream for reading the archive.</param>
			/// <param name="path">The path of the file within the archive
			/// (not the external file path).</param>
			/// <returns>A stream for reading the extracted file, or null
			/// if the file does not exist in the archive.</returns>
			/// <exception cref="ArchiveException">The stream is not a valid
			/// archive.</exception>
			/// <remarks>The entire extracted file is cached in memory, so this
			/// method requires enough free memory to hold the file.</remarks>
			public Stream Unpack(Stream stream, string path)
			{
				if (stream == null)
				{
					throw new ArgumentNullException("stream");
				}

				if (path == null)
				{
					throw new ArgumentNullException("path");
				}

				BasicUnpackStreamContext streamContext =
					new BasicUnpackStreamContext(stream);
				this.Unpack(
					streamContext,
					delegate(string match)
					{
						return String.Compare(
							match, path, true, CultureInfo.InvariantCulture) == 0;
					});
				
				Stream extractStream = streamContext.FileStream;
				if (extractStream != null)
				{
					extractStream.Position = 0;
				}

				return extractStream;
			}

			/// <summary>
			/// Extracts files from an archive or archive chain.
			/// </summary>
			/// <param name="streamContext">A context interface to handle opening
			/// and closing of archive and file streams.</param>
			/// <param name="fileFilter">An optional predicate that can determine
			/// which files to process.</param>
			/// <exception cref="ArchiveException">The archive provided
			/// by the stream context is not valid.</exception>
			/// <remarks>
			/// The <paramref name="fileFilter"/> predicate takes an internal file
			/// path and returns true to include the file or false to exclude it.
			/// </remarks>
			public abstract void Unpack(
				IUnpackStreamContext streamContext,
				Predicate<string> fileFilter);

			/// <summary>
			/// Called by sublcasses to distribute a packing or unpacking progress
			/// event to listeners.
			/// </summary>
			/// <param name="e">Event details.</param>
			protected void OnProgress(ArchiveProgressEventArgs e)
			{
				if (this.Progress != null)
				{
					this.Progress(this, e);
				}
			}

			/// <summary>
			/// Disposes of resources allocated by the compression engine.
			/// </summary>
			/// <param name="disposing">If true, the method has been called
			/// directly or indirectly by a user's code, so managed and unmanaged
			/// resources will be disposed. If false, the method has been called by
			/// the runtime from inside the finalizer, and only unmanaged resources
			/// will be disposed.</param>
			protected virtual void Dispose(bool disposing)
			{
			}

			/// <summary>
			/// Compresion utility function for converting old-style
			/// date and time values to a DateTime structure.
			/// </summary>
			public static void DosDateAndTimeToDateTime(
				short dosDate, short dosTime, out DateTime dateTime)
			{
				if (dosDate == 0 && dosTime == 0)
				{
					dateTime = DateTime.MinValue;
				}
				else
				{
					long fileTime;
					SafeNativeMethods.DosDateTimeToFileTime(dosDate, dosTime, out fileTime);
					dateTime = DateTime.FromFileTimeUtc(fileTime);
					dateTime = new DateTime(dateTime.Ticks, DateTimeKind.Local);
				}
			}

			/// <summary>
			/// Compresion utility function for converting a DateTime structure
			/// to old-style date and time values.
			/// </summary>
			public static void DateTimeToDosDateAndTime(
				DateTime dateTime, out short dosDate, out short dosTime)
			{
				dateTime = new DateTime(dateTime.Ticks, DateTimeKind.Utc);
				long filetime = dateTime.ToFileTimeUtc();
				SafeNativeMethods.FileTimeToDosDateTime(ref filetime, out dosDate, out dosTime);
			}
		}

		/// <summary>
		/// Specifies the compression level ranging from minimum compresion to
		/// maximum compression, or no compression at all.
		/// </summary>
		/// <remarks>
		/// Although only four values are enumerated, any integral value between
		/// <see cref="CompressionLevel.Min"/> and <see cref="CompressionLevel.Max"/> can also be used.
		/// </remarks>
		public enum CompressionLevel
		{
			/// <summary>Do not compress files, only store.</summary>
			None = 0,

			/// <summary>Minimum compression; fastest.</summary>
			Min = 1,

			/// <summary>A compromize between speed and compression efficiency.</summary>
			Normal = 6,

			/// <summary>Maximum compression; slowest.</summary>
			Max = 10
		}

		/// <summary>
		/// Duplicates a source stream by maintaining a separate position.
		/// </summary>
		/// <remarks>
		/// WARNING: duplicate streams are not thread-safe with respect to each other or the original stream.
		/// If multiple threads use duplicate copies of the same stream, they must synchronize for any operations.
		/// </remarks>
		public class DuplicateStream : Stream
		{
			private Stream source;
			private long position;

			/// <summary>
			/// Creates a new duplicate of a stream.
			/// </summary>
			/// <param name="source">source of the duplicate</param>
			public DuplicateStream(Stream source)
			{
				if (source == null)
				{
					throw new ArgumentNullException("source");
				}

				this.source = DuplicateStream.OriginalStream(source);
			}

			/// <summary>
			/// Gets the original stream that was used to create the duplicate.
			/// </summary>
			public Stream Source
			{
				get
				{
					return this.source;
				}
			}

			/// <summary>
			/// Gets a value indicating whether the source stream supports reading.
			/// </summary>
			/// <value>true if the stream supports reading; otherwise, false.</value>
			public override bool CanRead
			{
				get
				{
					return this.source.CanRead;
				}
			}

			/// <summary>
			/// Gets a value indicating whether the source stream supports writing.
			/// </summary>
			/// <value>true if the stream supports writing; otherwise, false.</value>
			public override bool CanWrite
			{
				get
				{
					return this.source.CanWrite;
				}
			}

			/// <summary>
			/// Gets a value indicating whether the source stream supports seeking.
			/// </summary>
			/// <value>true if the stream supports seeking; otherwise, false.</value>
			public override bool CanSeek
			{
				get
				{
					return this.source.CanSeek;
				}
			}

			/// <summary>
			/// Gets the length of the source stream.
			/// </summary>
			public override long Length
			{
				get
				{
					return this.source.Length;
				}
			}

			/// <summary>
			/// Gets or sets the position of the current stream,
			/// ignoring the position of the source stream.
			/// </summary>
			public override long Position
			{
				get
				{
					return this.position;
				}

				set
				{
					this.position = value;
				}
			}

			/// <summary>
			/// Retrieves the original stream from a possible duplicate stream.
			/// </summary>
			/// <param name="stream">Possible duplicate stream.</param>
			/// <returns>If the stream is a DuplicateStream, returns
			/// the duplicate's source; otherwise returns the same stream.</returns>
			public static Stream OriginalStream(Stream stream)
			{
				DuplicateStream dupStream = stream as DuplicateStream;
				return dupStream != null ? dupStream.Source : stream;
			}

			/// <summary>
			/// Flushes the source stream.
			/// </summary>
			public override void Flush()
			{
				this.source.Flush();
			}

			/// <summary>
			/// Sets the length of the source stream.
			/// </summary>
			/// <param name="value">The desired length of the stream in bytes.</param>
			public override void SetLength(long value)
			{ 
				this.source.SetLength(value);
			}

			/// <summary>
			/// Closes the underlying stream, effectively closing ALL duplicates.
			/// </summary>
			public override void Close()
			{
				this.source.Close();
			}

			/// <summary>
			/// Reads from the source stream while maintaining a separate position
			/// and not impacting the source stream's position.
			/// </summary>
			/// <param name="buffer">An array of bytes. When this method returns, the buffer
			/// contains the specified byte array with the values between offset and
			/// (offset + count - 1) replaced by the bytes read from the current source.</param>
			/// <param name="offset">The zero-based byte offset in buffer at which to begin
			/// storing the data read from the current stream.</param>
			/// <param name="count">The maximum number of bytes to be read from the current stream.</param>
			/// <returns>The total number of bytes read into the buffer. This can be less
			/// than the number of bytes requested if that many bytes are not currently available,
			/// or zero (0) if the end of the stream has been reached.</returns>
			public override int Read(byte[] buffer, int offset, int count)
			{
				long saveSourcePosition = this.source.Position;
				this.source.Position = this.position;
				int read = this.source.Read(buffer, offset, count);
				this.position = this.source.Position;
				this.source.Position = saveSourcePosition;
				return read;
			}

			/// <summary>
			/// Writes to the source stream while maintaining a separate position
			/// and not impacting the source stream's position.
			/// </summary>
			/// <param name="buffer">An array of bytes. This method copies count
			/// bytes from buffer to the current stream.</param>
			/// <param name="offset">The zero-based byte offset in buffer at which
			/// to begin copying bytes to the current stream.</param>
			/// <param name="count">The number of bytes to be written to the
			/// current stream.</param>
			public override void Write(byte[] buffer, int offset, int count)
			{
				long saveSourcePosition = this.source.Position;
				this.source.Position = this.position;
				this.source.Write(buffer, offset, count);
				this.position = this.source.Position;
				this.source.Position = saveSourcePosition;
			}

			/// <summary>
			/// Changes the position of this stream without impacting the
			/// source stream's position.
			/// </summary>
			/// <param name="offset">A byte offset relative to the origin parameter.</param>
			/// <param name="origin">A value of type SeekOrigin indicating the reference
			/// point used to obtain the new position.</param>
			/// <returns>The new position within the current stream.</returns>
			public override long Seek(long offset, SeekOrigin origin)
			{
				long originPosition = 0;
				if (origin == SeekOrigin.Current)
				{
					originPosition = this.position;
				}
				else if (origin == SeekOrigin.End)
				{
					originPosition = this.Length;
				}

				this.position = originPosition + offset;
				return this.position;
			}
		}

		/// <summary>
		/// Generic class for managing allocations of integer handles
		/// for objects of a certain type.
		/// </summary>
		/// <typeparam name="T">The type of objects the handles refer to.</typeparam>
		internal sealed class HandleManager<T> where T : class
		{
			/// <summary>
			/// Auto-resizing list of objects for which handles have been allocated.
			/// Each handle is just an index into this list. When a handle is freed,
			/// the list item at that index is set to null.
			/// </summary>
			private List<T> handles;

			/// <summary>
			/// Creates a new HandleManager instance.
			/// </summary>
			public HandleManager()
			{
				this.handles = new List<T>();
			}

			/// <summary>
			/// Gets the object of a handle, or null if the handle is invalid.
			/// </summary>
			/// <param name="handle">The integer handle previously allocated
			/// for the desired object.</param>
			/// <returns>The object for which the handle was allocated.</returns>
			public T this[int handle]
			{
				get
				{
					if (handle > 0 && handle <= this.handles.Count)
					{
						return this.handles[handle - 1];
					}
					else
					{
						return null;
					}
				}
			}

			/// <summary>
			/// Allocates a new handle for an object.
			/// </summary>
			/// <param name="obj">Object that the handle will refer to.</param>
			/// <returns>New handle that can be later used to retrieve the object.</returns>
			public int AllocHandle(T obj)
			{
				this.handles.Add(obj);
				int handle = this.handles.Count;
				return handle;
			}

			/// <summary>
			/// Frees a handle that was previously allocated. Afterward the handle
			/// will be invalid and the object it referred to can no longer retrieved.
			/// </summary>
			/// <param name="handle">Handle to be freed.</param>
			public void FreeHandle(int handle)
			{
				if (handle > 0 && handle <= this.handles.Count)
				{
					this.handles[handle - 1] = null;
				}
			}
		}

		/// <summary>
		/// This interface provides the methods necessary for the
		/// <see cref="CompressionEngine"/> to open and close streams for archives
		/// and files. The implementor of this interface can use any kind of logic
		/// to determine what kind of streams to open and where.
		/// </summary>
		public interface IPackStreamContext
		{
			/// <summary>
			/// Gets the name of the archive with a specified number.
			/// </summary>
			/// <param name="archiveNumber">The 0-based index of the archive
			/// within the chain.</param>
			/// <returns>The name of the requested archive. May be an empty string
			/// for non-chained archives, but may never be null.</returns>
			/// <remarks>The archive name is the name stored within the archive, used for
			/// identification of the archive especially among archive chains. That
			/// name is often, but not necessarily the same as the filename of the
			/// archive package.</remarks>
			string GetArchiveName(int archiveNumber);

			/// <summary>
			/// Opens a stream for writing an archive package.
			/// </summary>
			/// <param name="archiveNumber">The 0-based index of the archive within
			/// the chain.</param>
			/// <param name="archiveName">The name of the archive that was returned
			/// by <see cref="GetArchiveName"/>.</param>
			/// <param name="truncate">True if the stream should be truncated when
			/// opened (if it already exists); false if an existing stream is being
			/// re-opened for writing additional data.</param>
			/// <param name="compressionEngine">Instance of the compression engine
			/// doing the operations.</param>
			/// <returns>A writable Stream where the compressed archive bytes will be
			/// written, or null to cancel the archive creation.</returns>
			/// <remarks>
			/// If this method returns null, the archive engine will throw a
			/// FileNotFoundException.
			/// </remarks>
			Stream OpenArchiveWriteStream(
				int archiveNumber,
				string archiveName,
				bool truncate,
				CompressionEngine compressionEngine);

			/// <summary>
			/// Closes a stream where an archive package was written.
			/// </summary>
			/// <param name="archiveNumber">The 0-based index of the archive within
			/// the chain.</param>
			/// <param name="archiveName">The name of the archive that was previously
			/// returned by
			/// <see cref="GetArchiveName"/>.</param>
			/// <param name="stream">A stream that was previously returned by
			/// <see cref="OpenArchiveWriteStream"/> and is now ready to be closed.</param>
			/// <remarks>
			/// If there is another archive package in the chain, then after this stream
			/// is closed a new stream will be opened.
			/// </remarks>
			void CloseArchiveWriteStream(int archiveNumber, string archiveName, Stream stream);

			/// <summary>
			/// Opens a stream to read a file that is to be included in an archive.
			/// </summary>
			/// <param name="path">The path of the file within the archive. This is often,
			/// but not necessarily, the same as the relative path of the file outside
			/// the archive.</param>
			/// <param name="attributes">Returned attributes of the opened file, to be
			/// stored in the archive.</param>
			/// <param name="lastWriteTime">Returned last-modified time of the opened file,
			/// to be stored in the archive.</param>
			/// <returns>A readable Stream where the file bytes will be read from before
			/// they are compressed, or null to skip inclusion of the file and continue to
			/// the next file.</returns>
			[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
			Stream OpenFileReadStream(
				string path,
				out FileAttributes attributes,
				out DateTime lastWriteTime);

			/// <summary>
			/// Closes a stream that has been used to read a file.
			/// </summary>
			/// <param name="path">The path of the file within the archive; the same as
			/// the path provided
			/// when the stream was opened.</param>
			/// <param name="stream">A stream that was previously returned by
			/// <see cref="OpenFileReadStream"/> and is now ready to be closed.</param>
			void CloseFileReadStream(string path, Stream stream);

			/// <summary>
			/// Gets extended parameter information specific to the compression
			/// format being used.
			/// </summary>
			/// <param name="optionName">Name of the option being requested.</param>
			/// <param name="parameters">Parameters for the option; for per-file options,
			/// the first parameter is typically the internal file path.</param>
			/// <returns>Option value, or null to use the default behavior.</returns>
			/// <remarks>
			/// This method provides a way to set uncommon options during packaging, or a
			/// way to handle aspects of compression formats not supported by the base library.
			/// <para>For example, this may be used by the zip compression library to
			/// specify different compression methods/levels on a per-file basis.</para>
			/// <para>The available option names, parameters, and expected return values
			/// should be documented by each compression library.</para>
			/// </remarks>
			object GetOption(string optionName, object[] parameters);
		}

		/// <summary>
		/// This interface provides the methods necessary for the <see cref="CompressionEngine"/> to open
		/// and close streams for archives and files. The implementor of this interface can use any
		/// kind of logic to determine what kind of streams to open and where 
		/// </summary>
		public interface IUnpackStreamContext
		{
			/// <summary>
			/// Opens the archive stream for reading.
			/// </summary>
			/// <param name="archiveNumber">The zero-based index of the archive to open.</param>
			/// <param name="archiveName">The name of the archive being opened.</param>
			/// <param name="compressionEngine">Instance of the compression engine doing the operations.</param>
			/// <returns>A stream from which archive bytes are read, or null to cancel extraction
			/// of the archive.</returns>
			/// <remarks>
			/// When the first archive in a chain is opened, the name is not yet known, so the
			/// provided value will be an empty string. When opening further archives, the
			/// provided value is the next-archive name stored in the previous archive. This
			/// name is often, but not necessarily, the same as the filename of the archive
			/// package to be opened.
			/// <para>If this method returns null, the archive engine will throw a
			/// FileNotFoundException.</para>
			/// </remarks>
			Stream OpenArchiveReadStream(int archiveNumber, string archiveName, CompressionEngine compressionEngine);

			/// <summary>
			/// Closes a stream where an archive package was read.
			/// </summary>
			/// <param name="archiveNumber">The archive number of the stream to close.</param>
			/// <param name="archiveName">The name of the archive being closed.</param>
			/// <param name="stream">The stream that was previously returned by
			/// <see cref="OpenArchiveReadStream"/> and is now ready to be closed.</param>
			void CloseArchiveReadStream(int archiveNumber, string archiveName, Stream stream);

			/// <summary>
			/// Opens a stream for writing extracted file bytes.
			/// </summary>
			/// <param name="path">The path of the file within the archive. This is often, but
			/// not necessarily, the same as the relative path of the file outside the archive.</param>
			/// <param name="fileSize">The uncompressed size of the file to be extracted.</param>
			/// <param name="lastWriteTime">The last write time of the file to be extracted.</param>
			/// <returns>A stream where extracted file bytes are to be written, or null to skip
			/// extraction of the file and continue to the next file.</returns>
			/// <remarks>
			/// The implementor may use the path, size and date information to dynamically
			/// decide whether or not the file should be extracted.
			/// </remarks>
			Stream OpenFileWriteStream(string path, long fileSize, DateTime lastWriteTime);

			/// <summary>
			/// Closes a stream where an extracted file was written.
			/// </summary>
			/// <param name="path">The path of the file within the archive.</param>
			/// <param name="stream">The stream that was previously returned by <see cref="OpenFileWriteStream"/>
			/// and is now ready to be closed.</param>
			/// <param name="attributes">The attributes of the extracted file.</param>
			/// <param name="lastWriteTime">The last write time of the file.</param>
			/// <remarks>
			/// The implementor may wish to apply the attributes and date to the newly-extracted file.
			/// </remarks>
			void CloseFileWriteStream(string path, Stream stream, FileAttributes attributes, DateTime lastWriteTime);
		}

		/// <summary>
		/// Native DllImport methods and related structures and constants used for
		/// cabinet creation and extraction via cabinet.dll.
		/// </summary>
		internal static class NativeMethods
		{
			/// <summary>
			/// A direct import of constants, enums, structures, delegates, and functions from fci.h.
			/// Refer to comments in fci.h for documentation.
			/// </summary>
			internal static class FCI
			{
				internal const int MIN_DISK = 32768;
				internal const int MAX_DISK = Int32.MaxValue;
				internal const int MAX_FOLDER = 0x7FFF8000;
				internal const int MAX_FILENAME = 256;
				internal const int MAX_CABINET_NAME = 256;
				internal const int MAX_CAB_PATH = 256;
				internal const int MAX_DISK_NAME = 256;

				internal const int CPU_80386 = 1;

				[UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate IntPtr PFNALLOC(int cb);
				[UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate void PFNFREE(IntPtr pv);

				[UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate int PFNOPEN(string path, int oflag, int pmode, out int err, IntPtr pv);
				[UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate int PFNREAD(int fileHandle, IntPtr memory, int cb, out int err, IntPtr pv);
				[UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate int PFNWRITE(int fileHandle, IntPtr memory, int cb, out int err, IntPtr pv);
				[UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate int PFNCLOSE(int fileHandle, out int err, IntPtr pv);
				[UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate int PFNSEEK(int fileHandle, int dist, int seekType, out int err, IntPtr pv);
				[UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate int PFNDELETE(string path, out int err, IntPtr pv);

				[UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate int PFNGETNEXTCABINET(IntPtr pccab, uint cbPrevCab, IntPtr pv);
				[UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate int PFNFILEPLACED(IntPtr pccab, string path, long fileSize, int continuation, IntPtr pv);
				[UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate int PFNGETOPENINFO(string path, out short date, out short time, out short pattribs, out int err, IntPtr pv);
				[UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate int PFNSTATUS(STATUS typeStatus, uint cb1, uint cb2, IntPtr pv);
				[UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate int PFNGETTEMPFILE(IntPtr tempNamePtr, int tempNameSize, IntPtr pv);

				/// <summary>
				/// Error codes that can be returned by FCI.
				/// </summary>
				internal enum ERROR : int
				{
					NONE,
					OPEN_SRC,
					READ_SRC,
					ALLOC_FAIL,
					TEMP_FILE,
					BAD_COMPR_TYPE,
					CAB_FILE,
					USER_ABORT,
					MCI_FAIL,
				}

				/// <summary>
				/// FCI compression algorithm types and parameters.
				/// </summary>
				internal enum TCOMP : ushort
				{
					MASK_TYPE           = 0x000F,
					TYPE_NONE           = 0x0000,
					TYPE_MSZIP          = 0x0001,
					TYPE_QUANTUM        = 0x0002,
					TYPE_LZX            = 0x0003,
					BAD                 = 0x000F,

					MASK_LZX_WINDOW     = 0x1F00,
					LZX_WINDOW_LO       = 0x0F00,
					LZX_WINDOW_HI       = 0x1500,
					SHIFT_LZX_WINDOW    = 0x0008,

					MASK_QUANTUM_LEVEL  = 0x00F0,
					QUANTUM_LEVEL_LO    = 0x0010,
					QUANTUM_LEVEL_HI    = 0x0070,
					SHIFT_QUANTUM_LEVEL = 0x0004,

					MASK_QUANTUM_MEM    = 0x1F00,
					QUANTUM_MEM_LO      = 0x0A00,
					QUANTUM_MEM_HI      = 0x1500,
					SHIFT_QUANTUM_MEM   = 0x0008,

					MASK_RESERVED       = 0xE000,
				}

				/// <summary>
				/// Reason for FCI status callback.
				/// </summary>
				internal enum STATUS : uint
				{
					FILE    = 0,
					FOLDER  = 1,
					CABINET = 2,
				}

				[SuppressMessage("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments")]
				[DllImport("cabinet.dll", EntryPoint = "FCICreate", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, CallingConvention = CallingConvention.Cdecl)]
				internal static extern Handle Create(IntPtr perf, PFNFILEPLACED pfnfcifp, PFNALLOC pfna, PFNFREE pfnf, PFNOPEN pfnopen, PFNREAD pfnread, PFNWRITE pfnwrite, PFNCLOSE pfnclose, PFNSEEK pfnseek, PFNDELETE pfndelete, PFNGETTEMPFILE pfnfcigtf, [MarshalAs(UnmanagedType.LPStruct)] CCAB pccab, IntPtr pv);

				[DllImport("cabinet.dll", EntryPoint = "FCIAddFile", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, CallingConvention = CallingConvention.Cdecl)]
				internal static extern int AddFile(Handle hfci, string pszSourceFile, IntPtr pszFileName, [MarshalAs(UnmanagedType.Bool)] bool fExecute, PFNGETNEXTCABINET pfnfcignc, PFNSTATUS pfnfcis, PFNGETOPENINFO pfnfcigoi, TCOMP typeCompress);

				[DllImport("cabinet.dll", EntryPoint = "FCIFlushCabinet", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, CallingConvention = CallingConvention.Cdecl)]
				internal static extern int FlushCabinet(Handle hfci, [MarshalAs(UnmanagedType.Bool)] bool fGetNextCab, PFNGETNEXTCABINET pfnfcignc, PFNSTATUS pfnfcis);

				[DllImport("cabinet.dll", EntryPoint = "FCIFlushFolder", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, CallingConvention = CallingConvention.Cdecl)]
				internal static extern int FlushFolder(Handle hfci, PFNGETNEXTCABINET pfnfcignc, PFNSTATUS pfnfcis);

				[SuppressUnmanagedCodeSecurity]
				[DllImport("cabinet.dll", EntryPoint = "FCIDestroy", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, CallingConvention = CallingConvention.Cdecl)]
				[return: MarshalAs(UnmanagedType.Bool)]
				internal static extern bool Destroy(IntPtr hfci);

				/// <summary>
				/// Cabinet information structure used for FCI initialization and GetNextCabinet callback.
				/// </summary>
				[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
				internal class CCAB
				{
					internal int cb             = MAX_DISK;
					internal int cbFolderThresh = MAX_FOLDER;
					internal int cbReserveCFHeader;
					internal int cbReserveCFFolder;
					internal int cbReserveCFData;
					internal int iCab;
					internal int iDisk;
					internal int fFailOnIncompressible;
					internal short setID;
					[MarshalAs(UnmanagedType.ByValTStr, SizeConst=MAX_DISK_NAME   )] internal string szDisk    = String.Empty;
					[MarshalAs(UnmanagedType.ByValTStr, SizeConst=MAX_CABINET_NAME)] internal string szCab     = String.Empty;
					[MarshalAs(UnmanagedType.ByValTStr, SizeConst=MAX_CAB_PATH    )] internal string szCabPath = String.Empty;
				}

				/// <summary>
				/// Ensures that the FCI handle is safely released.
				/// </summary>
				internal class Handle : SafeHandle
				{
					/// <summary>
					/// Creates a new unintialized handle. The handle will be initialized
					/// when it is marshalled back from native code.
					/// </summary>
					internal Handle()
						: base(IntPtr.Zero, true)
					{
					}

					/// <summary>
					/// Checks if the handle is invalid. An FCI handle is invalid when it is zero.
					/// </summary>
					public override bool IsInvalid
					{
						get
						{
							return this.handle == IntPtr.Zero;
						}
					}

					/// <summary>
					/// Releases the handle by calling FDIDestroy().
					/// </summary>
					/// <returns>True if the release succeeded.</returns>
					protected override bool ReleaseHandle()
					{
						return FCI.Destroy(this.handle);
					}
				}
			}

			/// <summary>
			/// A direct import of constants, enums, structures, delegates, and functions from fdi.h.
			/// Refer to comments in fdi.h for documentation.
			/// </summary>
			internal static class FDI
			{
				internal const int MAX_DISK         = Int32.MaxValue;
				internal const int MAX_FILENAME     = 256;
				internal const int MAX_CABINET_NAME = 256;
				internal const int MAX_CAB_PATH     = 256;
				internal const int MAX_DISK_NAME    = 256;

				internal const int CPU_80386 = 1;

				[UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate IntPtr PFNALLOC(int cb);
				[UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate void PFNFREE(IntPtr pv);

				[UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate int PFNOPEN(string path, int oflag, int pmode);
				[UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate int PFNREAD(int hf, IntPtr pv, int cb);
				[UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate int PFNWRITE(int hf, IntPtr pv, int cb);
				[UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate int PFNCLOSE(int hf);
				[UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate int PFNSEEK(int hf, int dist, int seektype);

				[UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate int PFNNOTIFY(NOTIFICATIONTYPE fdint, NOTIFICATION fdin);

				/// <summary>
				/// Error codes that can be returned by FDI.
				/// </summary>
				internal enum ERROR : int
				{
					NONE,
					CABINET_NOT_FOUND,
					NOT_A_CABINET,
					UNKNOWN_CABINET_VERSION,
					CORRUPT_CABINET,
					ALLOC_FAIL,
					BAD_COMPR_TYPE,
					MDI_FAIL,
					TARGET_FILE,
					RESERVE_MISMATCH,
					WRONG_CABINET,
					USER_ABORT,
				}

				/// <summary>
				/// Type of notification message for the FDI Notify callback.
				/// </summary>
				internal enum NOTIFICATIONTYPE : int
				{
					CABINET_INFO,
					PARTIAL_FILE,
					COPY_FILE,
					CLOSE_FILE_INFO,
					NEXT_CABINET,
					ENUMERATE,
				}

				[DllImport("cabinet.dll", EntryPoint = "FDICreate", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, CallingConvention = CallingConvention.Cdecl)]
				internal static extern Handle Create([MarshalAs(UnmanagedType.FunctionPtr)] PFNALLOC pfnalloc, [MarshalAs(UnmanagedType.FunctionPtr)] PFNFREE pfnfree, PFNOPEN pfnopen, PFNREAD pfnread, PFNWRITE pfnwrite, PFNCLOSE pfnclose, PFNSEEK pfnseek, int cpuType, IntPtr perf);

				[DllImport("cabinet.dll", EntryPoint = "FDICopy", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, CallingConvention = CallingConvention.Cdecl)]
				internal static extern int Copy(Handle hfdi, string pszCabinet, string pszCabPath, int flags, PFNNOTIFY pfnfdin, IntPtr pfnfdid, IntPtr pvUser);

				[SuppressUnmanagedCodeSecurity]
				[DllImport("cabinet.dll", EntryPoint = "FDIDestroy", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, CallingConvention = CallingConvention.Cdecl)]
				[return: MarshalAs(UnmanagedType.Bool)]
				internal static extern bool Destroy(IntPtr hfdi);

				[DllImport("cabinet.dll", EntryPoint = "FDIIsCabinet", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, CallingConvention = CallingConvention.Cdecl)]
				[SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable", Justification="FDI file handles definitely remain 4 bytes on 64bit platforms.")]
				internal static extern int IsCabinet(Handle hfdi, int hf, out CABINFO pfdici);

				/// <summary>
				/// Cabinet information structure filled in by FDI IsCabinet.
				/// </summary>
				[StructLayout(LayoutKind.Sequential)]
				internal struct CABINFO
				{
					internal int cbCabinet;
					internal short cFolders;
					internal short cFiles;
					internal short setID;
					internal short iCabinet;
					internal int fReserve;
					internal int hasprev;
					internal int hasnext;
				}

				/// <summary>
				/// Cabinet notification details passed to the FDI Notify callback.
				/// </summary>
				[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
				[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
				internal class NOTIFICATION
				{
					internal int cb;
					internal IntPtr psz1;
					internal IntPtr psz2;
					internal IntPtr psz3;
					internal IntPtr pv;

					internal IntPtr hf_ptr;

					internal short date;
					internal short time;
					internal short attribs;
					internal short setID;
					internal short iCabinet;
					internal short iFolder;
					internal int fdie;

					// Unlike all the other file handles in FCI/FDI, this one is
					// actually pointer-sized. Use a property to pretend it isn't.
					internal int hf
					{
						get { return (int) this.hf_ptr; }
					}
				}

				/// <summary>
				/// Ensures that the FDI handle is safely released.
				/// </summary>
				internal class Handle : SafeHandle
				{
					/// <summary>
					/// Creates a new unintialized handle. The handle will be initialized
					/// when it is marshalled back from native code.
					/// </summary>
					internal Handle()
						: base(IntPtr.Zero, true)
					{
					}

					/// <summary>
					/// Checks if the handle is invalid. An FDI handle is invalid when it is zero.
					/// </summary>
					public override bool IsInvalid
					{
						get
						{
							return this.handle == IntPtr.Zero;
						}
					}

					/// <summary>
					/// Releases the handle by calling FDIDestroy().
					/// </summary>
					/// <returns>True if the release succeeded.</returns>
					protected override bool ReleaseHandle()
					{
						return FDI.Destroy(this.handle);
					}
				}
			}

			/// <summary>
			/// Error info structure for FCI and FDI.
			/// </summary>
			/// <remarks>Before being passed to FCI or FDI, this structure is
			/// pinned in memory via a GCHandle. The pinning is necessary
			/// to be able to read the results, since the ERF structure doesn't
			/// get marshalled back out after an error.</remarks>
			[StructLayout(LayoutKind.Sequential)]
			internal class ERF
			{
				private int erfOper;
				private int erfType;
				private int fError;

				/// <summary>
				/// Gets or sets the cabinet error code.
				/// </summary>
				internal int Oper
				{
					get
					{
						return this.erfOper;
					}

					set
					{
						this.erfOper = value;
					}
				}

				/// <summary>
				/// Gets or sets the Win32 error code.
				/// </summary>
				internal int Type
				{
					get
					{
						return this.erfType;
					}

					set
					{
						this.erfType = value;
					}
				}

				/// <summary>
				/// GCHandle doesn't like the bool type, so use an int underneath.
				/// </summary>
				internal bool Error
				{
					get
					{
						return this.fError != 0;
					}

					set
					{
						this.fError = value ? 1 : 0;
					}
				}

				/// <summary>
				/// Clears the error information.
				/// </summary>
				internal void Clear()
				{
					this.Oper = 0;
					this.Type = 0;
					this.Error = false;
				}
			}
		}

		/// <summary>
		/// Wraps a source stream and offsets all read/write/seek calls by a given value.
		/// </summary>
		/// <remarks>
		/// This class is used to trick archive an packing or unpacking process
		/// into reading or writing at an offset into a file, primarily for
		/// self-extracting packages.
		/// </remarks>
		public class OffsetStream : Stream
		{
			private Stream source;
			private long sourceOffset;

			/// <summary>
			/// Creates a new OffsetStream instance from a source stream
			/// and using a specified offset.
			/// </summary>
			/// <param name="source">Underlying stream for which all calls will be offset.</param>
			/// <param name="offset">Positive or negative number of bytes to offset.</param>
			public OffsetStream(Stream source, long offset)
			{
				if (source == null)
				{
					throw new ArgumentNullException("source");
				}

				this.source = source;
				this.sourceOffset = offset;

				this.source.Seek(this.sourceOffset, SeekOrigin.Current);
			}

			/// <summary>
			/// Gets the underlying stream that this OffsetStream calls into.
			/// </summary>
			public Stream Source
			{
				get { return this.source; }
			}

			/// <summary>
			/// Gets the number of bytes to offset all calls before
			/// redirecting to the underlying stream.
			/// </summary>
			public long Offset
			{
				get { return this.sourceOffset; }
			}

			/// <summary>
			/// Gets a value indicating whether the source stream supports reading.
			/// </summary>
			/// <value>true if the stream supports reading; otherwise, false.</value>
			public override bool CanRead
			{
				get
				{
					return this.source.CanRead;
				}
			}

			/// <summary>
			/// Gets a value indicating whether the source stream supports writing.
			/// </summary>
			/// <value>true if the stream supports writing; otherwise, false.</value>
			public override bool CanWrite
			{
				get
				{
					return this.source.CanWrite;
				}
			}

			/// <summary>
			/// Gets a value indicating whether the source stream supports seeking.
			/// </summary>
			/// <value>true if the stream supports seeking; otherwise, false.</value>
			public override bool CanSeek
			{
				get
				{
					return this.source.CanSeek;
				}
			}

			/// <summary>
			/// Gets the effective length of the stream, which is equal to
			/// the length of the source stream minus the offset.
			/// </summary>
			public override long Length
			{
				get { return this.source.Length - this.sourceOffset; } 
			}

			/// <summary>
			/// Gets or sets the effective position of the stream, which
			/// is equal to the position of the source stream minus the offset.
			/// </summary>
			public override long Position
			{
				get { return this.source.Position - this.sourceOffset; }
				set { this.source.Position = value + this.sourceOffset; }
			}

			/// <summary>
			/// Reads a sequence of bytes from the source stream and advances
			/// the position within the stream by the number of bytes read.
			/// </summary>
			/// <param name="buffer">An array of bytes. When this method returns, the buffer
			/// contains the specified byte array with the values between offset and
			/// (offset + count - 1) replaced by the bytes read from the current source.</param>
			/// <param name="offset">The zero-based byte offset in buffer at which to begin
			/// storing the data read from the current stream.</param>
			/// <param name="count">The maximum number of bytes to be read from the current stream.</param>
			/// <returns>The total number of bytes read into the buffer. This can be less
			/// than the number of bytes requested if that many bytes are not currently available,
			/// or zero (0) if the end of the stream has been reached.</returns>
			public override int Read(byte[] buffer, int offset, int count)
			{
				return this.source.Read(buffer, offset, count);
			}

			/// <summary>
			/// Writes a sequence of bytes to the source stream and advances the
			/// current position within this stream by the number of bytes written.
			/// </summary>
			/// <param name="buffer">An array of bytes. This method copies count
			/// bytes from buffer to the current stream.</param>
			/// <param name="offset">The zero-based byte offset in buffer at which
			/// to begin copying bytes to the current stream.</param>
			/// <param name="count">The number of bytes to be written to the
			/// current stream.</param>
			public override void Write(byte[] buffer, int offset, int count)
			{
				this.source.Write(buffer, offset, count);
			}

			/// <summary>
			/// Reads a byte from the stream and advances the position within the
			/// source stream by one byte, or returns -1 if at the end of the stream.
			/// </summary>
			/// <returns>The unsigned byte cast to an Int32, or -1 if at the
			/// end of the stream.</returns>
			public override int ReadByte()
			{
				return this.source.ReadByte();
			}

			/// <summary>
			/// Writes a byte to the current position in the source stream and
			/// advances the position within the stream by one byte.
			/// </summary>
			/// <param name="value">The byte to write to the stream.</param>
			public override void WriteByte(byte value)
			{
				this.source.WriteByte(value);
			}

			/// <summary>
			/// Flushes the source stream.
			/// </summary>
			public override void Flush()
			{
				this.source.Flush();
			}

			/// <summary>
			/// Sets the position within the current stream, which is
			/// equal to the position within the source stream minus the offset.
			/// </summary>
			/// <param name="offset">A byte offset relative to the origin parameter.</param>
			/// <param name="origin">A value of type SeekOrigin indicating
			/// the reference point used to obtain the new position.</param>
			/// <returns>The new position within the current stream.</returns>
			public override long Seek(long offset, SeekOrigin origin)
			{
				return this.source.Seek(offset + (origin == SeekOrigin.Begin ? this.sourceOffset : 0), origin) - this.sourceOffset;
			}

			/// <summary>
			/// Sets the effective length of the stream, which is equal to
			/// the length of the source stream minus the offset.
			/// </summary>
			/// <param name="value">The desired length of the
			/// current stream in bytes.</param>
			public override void SetLength(long value)
			{
				this.source.SetLength(value + this.sourceOffset);
			}

			/// <summary>
			/// Closes the underlying stream.
			/// </summary>
			public override void Close()
			{
				this.source.Close();
			}
		}

		[SuppressUnmanagedCodeSecurity]
		internal static class SafeNativeMethods
		{
			[DllImport("kernel32.dll", SetLastError = true)]
			[return: MarshalAs(UnmanagedType.Bool)]
			internal static extern bool DosDateTimeToFileTime(
				short wFatDate, short wFatTime, out long fileTime);

			[DllImport("kernel32.dll", SetLastError = true)]
			[return: MarshalAs(UnmanagedType.Bool)]
			internal static extern bool FileTimeToDosDateTime(
				ref long fileTime, out short wFatDate, out short wFatTime);
		}
	}

	namespace Snappys
	{
        /// <summary>
        /// Routines for performing Snappy compression and decompression on raw data blocks using <see cref="Span{T}"/>.
        /// These routines do not read or write any Snappy framing.
        /// </summary>
        public static class Snappy
        {
            /// <summary>
            /// For a given amount of input data, calculate the maximum potential size of the compressed output.
            /// </summary>
            /// <param name="inputLength">Length of the input data, in bytes.</param>
            /// <returns>The maximum potential size of the compressed output.</returns>
            /// <remarks>
            /// This is useful for allocating a sufficient output buffer before calling <see cref="Compress"/>.
            /// </remarks>
            public static int GetMaxCompressedLength(int inputLength) =>
                SnappierInterop.Helpers.MaxCompressedLength(inputLength);

            /// <summary>
            /// Compress a block of Snappy data.
            /// </summary>
            /// <param name="input">Data to compress.</param>
            /// <param name="output">Buffer to receive the compressed data.</param>
            /// <returns>Number of bytes written to <paramref name="output"/>.</returns>
            /// <remarks>
            /// The output buffer must be large enough to contain the compressed output.
            /// </remarks>
            public static int Compress(ReadOnlySpan<byte> input, Span<byte> output)
            {
                using var compressor = new SnappierInterop.SnappyCompressor();

                return compressor.Compress(input, output);
            }

            /// <summary>
            /// Compress a block of Snappy data.
            /// </summary>
            /// <param name="input">Data to compress.</param>
            /// <returns>An <see cref="IMemoryOwner{T}"/> with the decompressed data. The caller is responsible for disposing this object.</returns>
            /// <remarks>
            /// Failing to dispose of the returned <see cref="IMemoryOwner{T}"/> may result in memory leaks.
            /// </remarks>
            public static IMemoryOwner<byte> CompressToMemory(ReadOnlySpan<byte> input)
            {
                byte[] buffer = ArrayPool<byte>.Shared.Rent(GetMaxCompressedLength(input.Length));

                try
                {
                    int length = Compress(input, buffer);

                    return new SnappierInterop.ByteArrayPoolMemoryOwner(buffer, length);
                }
                catch
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                    throw;
                }
            }

            /// <summary>
            /// Compress a block of Snappy data.
            /// </summary>
            /// <param name="input">Data to compress.</param>
            /// <remarks>
            /// The resulting byte array is allocated on the heap. If possible, <see cref="CompressToMemory"/> should
            /// be used instead since it uses a shared buffer pool.
            /// </remarks>
            public static byte[] CompressToArray(ReadOnlySpan<byte> input)
            {
                using var buffer = CompressToMemory(input);
                var bufferSpan = buffer.Memory.Span;

                var result = new byte[bufferSpan.Length];
                bufferSpan.CopyTo(result);
                return result;
            }

            /// <summary>
            /// Get the uncompressed data length from a compressed Snappy block.
            /// </summary>
            /// <param name="input">Compressed snappy block.</param>
            /// <returns>The length of the uncompressed data in the block.</returns>
            /// <exception cref="InvalidDataException">The data in <paramref name="input"/> has an invalid length.</exception>
            /// <remarks>
            /// This is useful for allocating a sufficient output buffer before calling <see cref="Decompress"/>.
            /// </remarks>
            public static int GetUncompressedLength(ReadOnlySpan<byte> input) =>
                SnappierInterop.SnappyDecompressor.ReadUncompressedLength(input);

            /// <summary>
            /// Decompress a block of Snappy data. This must be an entire block.
            /// </summary>
            /// <param name="input">Data to decompress.</param>
            /// <param name="output">Buffer to receive the decompressed data.</param>
            /// <returns>Number of bytes written to <paramref name="output"/>.</returns>
            /// <exception cref="InvalidDataException">Invalid Snappy block.</exception>
            /// <exception cref="ArgumentException">Output buffer is too small.</exception>
            public static int Decompress(ReadOnlySpan<byte> input, Span<byte> output)
            {
                using var decompressor = new SnappierInterop.SnappyDecompressor();

                decompressor.Decompress(input);

                if (!decompressor.AllDataDecompressed)
                {
                    SnappierInterop.ThrowHelper.ThrowInvalidDataException("Incomplete Snappy block.");
                }

                int read = decompressor.Read(output);

                if (!decompressor.EndOfFile)
                {
                    SnappierInterop.ThrowHelper.ThrowArgumentException("Output buffer is too small.", nameof(output));
                }

                return read;
            }

            /// <summary>
            /// Decompress a block of Snappy to a new memory buffer. This must be an entire block.
            /// </summary>
            /// <param name="input">Data to decompress.</param>
            /// <returns>An <see cref="IMemoryOwner{T}"/> with the decompressed data. The caller is responsible for disposing this object.</returns>
            /// <remarks>
            /// Failing to dispose of the returned <see cref="IMemoryOwner{T}"/> may result in memory leaks.
            /// </remarks>
            public static IMemoryOwner<byte> DecompressToMemory(ReadOnlySpan<byte> input)
            {
                using var decompressor = new SnappierInterop.SnappyDecompressor();

                decompressor.Decompress(input);

                if (!decompressor.AllDataDecompressed)
                {
                    SnappierInterop.ThrowHelper.ThrowInvalidDataException("Incomplete Snappy block.");
                }

                return decompressor.ExtractData();
            }

            /// <summary>
            /// Decompress a block of Snappy to a new byte array. This must be an entire block.
            /// </summary>
            /// <param name="input">Data to decompress.</param>
            /// <returns>The decompressed data.</returns>
            /// <remarks>
            /// The resulting byte array is allocated on the heap. If possible, <see cref="DecompressToMemory"/> should
            /// be used instead since it uses a shared buffer pool.
            /// </remarks>
            public static byte[] DecompressToArray(ReadOnlySpan<byte> input)
            {
                var length = GetUncompressedLength(input);

                var result = new byte[length];

                Decompress(input, result);

                return result;
            }
        }

		/// <summary>
		/// Stream which supports compressing or decompressing data using the Snappy compression algorithm.
		/// To decompress data, supply a stream to be read. To compress data, provide a stream to be written to.
		/// </summary>
		#nullable enable
		#pragma warning disable CS8602
		public sealed class SnappyStream : Stream
        {
            private const int DefaultBufferSize = 8192;

            private Stream _stream;
            private readonly CompressionMode _mode;
            private readonly bool _leaveOpen;
			private SnappierInterop.SnappyStreamDecompressor? _decompressor;
            private SnappierInterop.SnappyStreamCompressor? _compressor;

            private byte[]? _buffer = null;
            private bool _wroteBytes;

            /// <summary>
            /// Create a stream which supports compressing or decompressing data using the Snappy compression algorithm.
            /// To decompress data, supply a stream to be read. To compress data, provide a stream to be written to.
            /// </summary>
            /// <param name="stream">Source or destination stream.</param>
            /// <param name="mode">Compression or decompression mode.</param>
            /// <exception cref="ArgumentNullException"><paramref name="stream"/> is null.</exception>
            /// <exception cref="ArgumentException">Stream read/write capability doesn't match with <paramref name="mode"/>.</exception>
            /// <exception cref="ArgumentOutOfRangeException">Invalid <paramref name="mode"/>.</exception>
            /// <remarks>
            /// The stream will be closed when the SnappyStream is closed.
            /// </remarks>
            public SnappyStream(Stream stream, CompressionMode mode)
                : this(stream, mode, false)
            {
            }

            /// <summary>
            /// Create a stream which supports compressing or decompressing data using the Snappy compression algorithm.
            /// To decompress data, supply a stream to be read. To compress data, provide a stream to be written to.
            /// </summary>
            /// <param name="stream">Source or destination stream.</param>
            /// <param name="mode">Compression or decompression mode.</param>
            /// <param name="leaveOpen">If true, close the stream when the SnappyStream is closed.</param>
            /// <exception cref="ArgumentNullException"><paramref name="stream"/> is null.</exception>
            /// <exception cref="ArgumentException">Stream read/write capability doesn't match with <paramref name="mode"/>.</exception>
            /// <exception cref="ArgumentOutOfRangeException">Invalid <paramref name="mode"/>.</exception>
            public SnappyStream(Stream stream, CompressionMode mode, bool leaveOpen)
            {
                SnappierInterop.ThrowHelper.ThrowIfNull(stream);
                _stream = stream;
                _mode = mode;
                _leaveOpen = leaveOpen;

                switch (mode)
                {
                    case CompressionMode.Decompress:
                        if (!stream.CanRead)
                        {
                            SnappierInterop.ThrowHelper.ThrowArgumentException("Unreadable stream", nameof(stream));
                        }

                        _decompressor = new SnappierInterop.SnappyStreamDecompressor();

                        break;

                    case CompressionMode.Compress:
                        if (!stream.CanWrite)
                        {
                            SnappierInterop.ThrowHelper.ThrowArgumentException("Unwritable stream", nameof(stream));
                        }

                        _compressor = new SnappierInterop.SnappyStreamCompressor();
                        break;

                    default:
                        SnappierInterop.ThrowHelper.ThrowArgumentOutOfRangeException(nameof(mode), "Invalid mode");
                        break;
                }
            }

            /// <summary>
            /// The base stream being read from or written to.
            /// </summary>
            public Stream BaseStream => _stream;

            #region overrides

            /// <inheritdoc />
            public override bool CanRead => _mode == CompressionMode.Decompress && (_stream?.CanRead ?? false);

            /// <inheritdoc />
            public override bool CanWrite => _mode == CompressionMode.Compress && (_stream?.CanWrite ?? false);

            /// <inheritdoc />
            public override bool CanSeek => false;

            /// <inheritdoc />
            public override long Length
            {
                get
                {
                    SnappierInterop.ThrowHelper.ThrowNotSupportedException();
                    return 0;
                }
            }

            /// <inheritdoc />
            public override long Position
            {
                get
                {
                    SnappierInterop.ThrowHelper.ThrowNotSupportedException();
                    return 0;
                }
                // ReSharper disable once ValueParameterNotUsed
                set => SnappierInterop.ThrowHelper.ThrowNotSupportedException();
            }

            /// <inheritdoc />
            public override void Flush()
            {
                EnsureNotDisposed();

                if (_mode == CompressionMode.Compress && _wroteBytes)
                {
                    Debug.Assert(_compressor != null);
                    _compressor.Flush(_stream);
                }
            }

            /// <inheritdoc />
            public override Task FlushAsync(CancellationToken cancellationToken)
            {
                EnsureNoActiveAsyncOperation();
                EnsureNotDisposed();

                if (cancellationToken.IsCancellationRequested)
                {
                    return Task.FromCanceled(cancellationToken);
                }

                if (_mode == CompressionMode.Compress && _wroteBytes)
                {
                    Debug.Assert(_compressor != null);
                    return _compressor.FlushAsync(_stream, cancellationToken).AsTask();
                }

                return Task.CompletedTask;
            }

            /// <inheritdoc />
            public override long Seek(long offset, SeekOrigin origin)
            {
                SnappierInterop.ThrowHelper.ThrowNotSupportedException();
                return 0;
            }

            /// <inheritdoc />
            public override void SetLength(long value) => SnappierInterop.ThrowHelper.ThrowNotSupportedException();

            /// <inheritdoc />
            public override int Read(byte[] buffer, int offset, int count) => ReadCore(buffer.AsSpan(offset, count));

			#if ! (NETFRAMEWORK || NETSTANDARD2_0)
				/// <inheritdoc />
				public override int Read(Span<byte> buffer) => ReadCore(buffer);
			#endif 

            private int ReadCore(Span<byte> buffer)
            {
                EnsureDecompressionMode();
                EnsureNotDisposed();
                EnsureBufferInitialized();

                int totalRead = 0;

                Debug.Assert(_decompressor != null);
                while (true)
                {
                    int bytesRead = _decompressor.Decompress(buffer.Slice(totalRead));
                    totalRead += bytesRead;

                    if (totalRead == buffer.Length)
                    {
                        break;
                    }

                    Debug.Assert(_buffer != null);
					#if ! (NETFRAMEWORK || NETSTANDARD2_0)
						int bytes = _stream.Read(_buffer);
					#else
						int bytes = _stream.Read(_buffer, 0, _buffer.Length);
					#endif
                    if (bytes <= 0)
                    {
                        break;
                    }
                    else if (bytes > _buffer.Length)
                    {
                        SnappierInterop.ThrowHelper.ThrowInvalidDataException("Insufficient buffer");
                    }

                    _decompressor.SetInput(_buffer.AsMemory(0, bytes));
                }

                return totalRead;
            }

            /// <inheritdoc />
            public override Task<int> ReadAsync(byte[] buffer, int offset, int count,
                CancellationToken cancellationToken) =>
                ReadAsyncCore(buffer.AsMemory(offset, count), cancellationToken).AsTask();

			#if ! (NETFRAMEWORK || NETSTANDARD2_0)
				/// <inheritdoc />
				public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = new CancellationToken()) =>
				    ReadAsyncCore(buffer, cancellationToken);
			#endif

            private ValueTask<int> ReadAsyncCore(Memory<byte> buffer, CancellationToken cancellationToken = new CancellationToken())
            {
                EnsureDecompressionMode();
                EnsureNoActiveAsyncOperation();
                EnsureNotDisposed();

                if (cancellationToken.IsCancellationRequested)
                {
                    return new ValueTask<int>(Task.FromCanceled<int>(cancellationToken));
                }

                EnsureBufferInitialized();

                bool cleanup = true;
                AsyncOperationStarting();
                try
                {
                    Debug.Assert(_decompressor != null);

                    // Finish decompressing any bytes in the input buffer
                    int bytesRead = 0, bytesReadIteration = -1;
                    while (bytesRead < buffer.Length && bytesReadIteration != 0)
                    {
                        bytesReadIteration = _decompressor.Decompress(buffer.Span.Slice(bytesRead));
                        bytesRead += bytesReadIteration;
                    }

                    if (bytesRead != 0)
                    {
                        // If decompression output buffer is not empty, return immediately.
                        return new ValueTask<int>(bytesRead);
                    }

					#if ! (NETFRAMEWORK || NETSTANDARD2_0)
						ValueTask<int> readTask = _stream.ReadAsync(_buffer, cancellationToken);
					#else
						ValueTask<int> readTask = new(_stream.ReadAsync(_buffer, 0, _buffer.Length, cancellationToken));
					#endif
                    cleanup = false;
                    return FinishReadAsyncMemory(readTask, buffer, cancellationToken);
                }
                finally
                {
                    // if we haven't started any async work, decrement the counter to end the transaction
                    if (cleanup)
                    {
                        AsyncOperationCompleting();
                    }
                }
            }

            private async ValueTask<int> FinishReadAsyncMemory(
                ValueTask<int> readTask, Memory<byte> buffer, CancellationToken cancellationToken)
            {
                try
                {
                    Debug.Assert(_decompressor != null && _buffer != null);
                    while (true)
                    {
                        int bytesRead = await readTask.ConfigureAwait(false);
                        EnsureNotDisposed();

                        if (bytesRead <= 0)
                        {
                            // This indicates the base stream has received EOF
                            return 0;
                        }
                        else if (bytesRead > _buffer.Length)
                        {
                            // The stream is either malicious or poorly implemented and returned a number of
                            // bytes larger than the buffer supplied to it.
                            SnappierInterop.ThrowHelper.ThrowInvalidDataException("Insufficient buffer");
                        }

                        cancellationToken.ThrowIfCancellationRequested();

                        // Feed the data from base stream into decompression engine
                        _decompressor.SetInput(_buffer.AsMemory(0, bytesRead));

                        // Finish inflating any bytes in the input buffer
                        int inflatedBytes = 0, bytesReadIteration = -1;
                        while (inflatedBytes < buffer.Length && bytesReadIteration != 0)
                        {
                            bytesReadIteration = _decompressor.Decompress(buffer.Span.Slice(inflatedBytes));
                            inflatedBytes += bytesReadIteration;
                        }

                        if (inflatedBytes != 0)
                        {
                            // If decompression output buffer is not empty, return immediately.
                            return inflatedBytes;
                        }
                        else
                        {
                            // We could have read in head information and didn't get any data.
                            // Read from the base stream again.
							#if ! (NETFRAMEWORK || NETSTANDARD2_0)
							    readTask = _stream.ReadAsync(_buffer, cancellationToken);
							#else
								readTask = new ValueTask<int>(_stream.ReadAsync(_buffer, 0, _buffer.Length, cancellationToken));
							#endif
                        }
                    }
                }
                finally
                {
                    AsyncOperationCompleting();
                }
            }

            /// <inheritdoc />
            public override void Write(byte[] buffer, int offset, int count) =>
                WriteCore(buffer.AsSpan(offset, count));

			#if ! (NETFRAMEWORK || NETSTANDARD2_0)
				/// <inheritdoc />
				public override void Write(ReadOnlySpan<byte> buffer) => WriteCore(buffer);
			#endif 

            private void WriteCore(ReadOnlySpan<byte> buffer)
            {
                EnsureCompressionMode();
                EnsureNotDisposed();

                Debug.Assert(_compressor != null);
                _compressor.Write(buffer, _stream);

                _wroteBytes = true;
            }

            /// <inheritdoc />
            public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
                WriteAsyncCore(buffer.AsMemory(offset, count), cancellationToken).AsTask();

			#if ! (NETFRAMEWORK || NETSTANDARD2_0)
				/// <inheritdoc />
				public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer,
					CancellationToken cancellationToken = default) =>
					WriteAsyncCore(buffer, cancellationToken);
			#endif 

            private ValueTask WriteAsyncCore(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken)
            {
                EnsureCompressionMode();
                EnsureNoActiveAsyncOperation();
                EnsureNotDisposed();

                return cancellationToken.IsCancellationRequested
                    ? new ValueTask(Task.FromCanceled(cancellationToken))
                    : WriteAsyncMemoryCore(buffer, cancellationToken);
            }

            private async ValueTask WriteAsyncMemoryCore(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken)
            {
                AsyncOperationStarting();
                try
                {
                    Debug.Assert(_compressor != null);

                    await _compressor.WriteAsync(buffer, _stream, cancellationToken).ConfigureAwait(false);

                    _wroteBytes = true;
                }
                finally
                {
                    AsyncOperationCompleting();
                }
            }

            // This is called by Dispose:
            private void PurgeBuffers()
            {
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (_stream == null || _mode != CompressionMode.Compress)
                {
                    return;
                }

                Debug.Assert(_compressor != null);
                // Make sure to only "flush" when we actually had some input
                if (_wroteBytes)
                {
                    Flush();
                }
            }

            private ValueTask PurgeBuffersAsync()
            {
                // Same logic as PurgeBuffers, except with async counterparts.

                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (_stream == null || _mode != CompressionMode.Compress)
                {
                    return default;
                }

                Debug.Assert(_compressor != null);
                // Make sure to only "flush" when we actually had some input
                if (_wroteBytes)
                {
                    return new ValueTask(FlushAsync());
                }

                return default;
            }

            /// <inheritdoc />
            protected override void Dispose(bool disposing)
            {
                try
                {
                    PurgeBuffers();
                }
                finally
                {
                    // Stream.Close() may throw here (may or may not be due to the same error).
                    // In this case, we still need to clean up internal resources, hence the inner finally blocks.
                    try
                    {
                        if (disposing && !_leaveOpen)
                            _stream?.Dispose();
                    }
                    finally
                    {
                        _stream = null!;

                        try
                        {
                            _decompressor?.Dispose();
                            _compressor?.Dispose();
                        }
                        finally
                        {
                            _decompressor = null;
                            _compressor = null;

                            byte[]? buffer = _buffer;
                            if (buffer != null)
                            {
                                _buffer = null;
                                if (!AsyncOperationIsActive)
                                {
                                    ArrayPool<byte>.Shared.Return(buffer);
                                }
                            }

                            base.Dispose(disposing);
                        }
                    }
                }
            }

			#if ! (NETFRAMEWORK || NETSTANDARD2_0)
            /// <inheritdoc />
            public override async ValueTask DisposeAsync()
            {
                // Same logic as Dispose(true), except with async counterparts.

                try
                {
                    await PurgeBuffersAsync().ConfigureAwait(false);
                }
                finally
                {

                    // Stream.Close() may throw here (may or may not be due to the same error).
                    // In this case, we still need to clean up internal resources, hence the inner finally blocks.
                    Stream stream = _stream;
                    _stream = null!;
                    try
                    {
                        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                        if (!_leaveOpen && stream != null)
                        {
                            await stream.DisposeAsync().ConfigureAwait(false);
                        }
                    }
                    finally
                    {
                        try
                        {
                            _decompressor?.Dispose();
                        }
                        finally
                        {
                            _decompressor = null;

                            byte[]? buffer = _buffer;
                            if (buffer != null)
                            {
                                _buffer = null;
                                if (!AsyncOperationIsActive)
                                {
                                    ArrayPool<byte>.Shared.Return(buffer);
                                }
                            }
                        }
                    }
                }
            }
			#endif

            #endregion

            private void EnsureNotDisposed()
            {
                if (_stream == null)
                {
                    SnappierInterop.ThrowHelper.ThrowObjectDisposedException(nameof(SnappyStream));
                }
            }

            private void EnsureDecompressionMode()
            {
                if (_mode != CompressionMode.Decompress)
                {
                    SnappierInterop.ThrowHelper.ThrowNotSupportedException();
                }
            }

            private void EnsureCompressionMode()
            {
                if (_mode != CompressionMode.Compress)
                {
                    SnappierInterop.ThrowHelper.ThrowNotSupportedException();
                }
            }

            private void EnsureBufferInitialized()
            {
                _buffer ??= ArrayPool<byte>.Shared.Rent(DefaultBufferSize);
            }

            #region async controls

            private int _activeAsyncOperation;
            private bool AsyncOperationIsActive => _activeAsyncOperation != 0;

            private void EnsureNoActiveAsyncOperation()
            {
                if (AsyncOperationIsActive)
                {
                    SnappierInterop.ThrowHelper.ThrowInvalidOperationException("Invalid begin call");
                }
            }

            private void AsyncOperationStarting()
            {
                if (Interlocked.CompareExchange(ref _activeAsyncOperation, 1, 0) != 0)
                {
                    SnappierInterop.ThrowHelper.ThrowInvalidOperationException("Invalid begin call");
                }
            }

            private void AsyncOperationCompleting()
            {
                int oldValue = Interlocked.CompareExchange(ref _activeAsyncOperation, 0, 1);
                Debug.Assert(oldValue == 1, $"Expected {nameof(_activeAsyncOperation)} to be 1, got {oldValue}");
            }

            #endregion
        }
		#pragma warning restore CS8602
		#nullable disable
    }

	#nullable enable
    namespace SnappierInterop
    {
        /// <summary>
        /// Wraps an inner byte array from <see cref="ArrayPool{T}.Shared"/>"/> with a limited length.
        /// </summary>
        /// <remarks>
        /// We use this instead of the built-in <see cref="MemoryPool{T}"/> because we want to slice the array without
        /// allocating another wrapping class on the heap.
        /// </remarks>
        internal sealed class ByteArrayPoolMemoryOwner : IMemoryOwner<byte>
        {
            private byte[]? _innerArray;

            /// <inheritdoc />
            public Memory<byte> Memory { get; private set; }

            /// <summary>
            /// Create an empty ByteArrayPoolMemoryOwner.
            /// </summary>
            public ByteArrayPoolMemoryOwner()
            {
                // _innerArray will be null and Memory will be a default empty Memory<byte>
            }

            /// <summary>
            /// Given a byte array from <see cref="ArrayPool{T}.Shared"/>, create a ByteArrayPoolMemoryOwner
            /// which wraps it until disposed and slices it to <paramref name="length"/>.
            /// </summary>
            /// <param name="innerArray">An array from the <see cref="ArrayPool{T}.Shared"/>.</param>
            /// <param name="length">The length of the array to return from <see cref="Memory"/>.</param>
            public ByteArrayPoolMemoryOwner(byte[] innerArray, int length)
            {
                ThrowHelper.ThrowIfNull(innerArray);

                _innerArray = innerArray;
                Memory = innerArray.AsMemory(0, length); // Also validates length
            }

            /// <inheritdoc />
            public void Dispose()
            {
                byte[]? innerArray = _innerArray;
                if (innerArray is not null)
                {
                    _innerArray = null;
                    Memory = default;
                    ArrayPool<byte>.Shared.Return(innerArray);
                }
            }
        }

        internal static class Constants
        {
            public enum ChunkType : byte
            {
                CompressedData = 0x00,
                UncompressedData = 0x01,
                SkippableChunk = 0x80, // If this bit is set, we can safely skip the chunk if unknown
                Padding = 0xfe,
                StreamIdentifier = 0xff
            }

            public const byte Literal = 0b00;
            public const byte Copy1ByteOffset = 1; // 3 bit length + 3 bits of offset in opcode
            public const byte Copy2ByteOffset = 2;
            public const byte Copy4ByteOffset = 3;

            public const int MaximumTagLength = 5;

            public const int BlockLog = 16;
            public const long BlockSize = 1 << BlockLog;
            public const nint InputMarginBytes = 15;

            /// <summary>
            /// Data stored per entry in lookup table:
            ///      Range   Bits-used       Description
            ///      ------------------------------------
            ///      1..64   0..7            Literal/copy length encoded in opcode byte
            ///      0..7    8..10           Copy offset encoded in opcode byte / 256
            ///      0..4    11..13          Extra bytes after opcode
            ///
            /// We use eight bits for the length even though 7 would have sufficed
            /// because of efficiency reasons:
            ///      (1) Extracting a byte is faster than a bit-field
            ///      (2) It properly aligns copy offset so we do not need a &lt;&lt;8
            /// </summary>
            public static readonly ushort[] CharTable =
            {
            0x0001, 0x0804, 0x1001, 0x2001, 0x0002, 0x0805, 0x1002, 0x2002,
            0x0003, 0x0806, 0x1003, 0x2003, 0x0004, 0x0807, 0x1004, 0x2004,
            0x0005, 0x0808, 0x1005, 0x2005, 0x0006, 0x0809, 0x1006, 0x2006,
            0x0007, 0x080a, 0x1007, 0x2007, 0x0008, 0x080b, 0x1008, 0x2008,
            0x0009, 0x0904, 0x1009, 0x2009, 0x000a, 0x0905, 0x100a, 0x200a,
            0x000b, 0x0906, 0x100b, 0x200b, 0x000c, 0x0907, 0x100c, 0x200c,
            0x000d, 0x0908, 0x100d, 0x200d, 0x000e, 0x0909, 0x100e, 0x200e,
            0x000f, 0x090a, 0x100f, 0x200f, 0x0010, 0x090b, 0x1010, 0x2010,
            0x0011, 0x0a04, 0x1011, 0x2011, 0x0012, 0x0a05, 0x1012, 0x2012,
            0x0013, 0x0a06, 0x1013, 0x2013, 0x0014, 0x0a07, 0x1014, 0x2014,
            0x0015, 0x0a08, 0x1015, 0x2015, 0x0016, 0x0a09, 0x1016, 0x2016,
            0x0017, 0x0a0a, 0x1017, 0x2017, 0x0018, 0x0a0b, 0x1018, 0x2018,
            0x0019, 0x0b04, 0x1019, 0x2019, 0x001a, 0x0b05, 0x101a, 0x201a,
            0x001b, 0x0b06, 0x101b, 0x201b, 0x001c, 0x0b07, 0x101c, 0x201c,
            0x001d, 0x0b08, 0x101d, 0x201d, 0x001e, 0x0b09, 0x101e, 0x201e,
            0x001f, 0x0b0a, 0x101f, 0x201f, 0x0020, 0x0b0b, 0x1020, 0x2020,
            0x0021, 0x0c04, 0x1021, 0x2021, 0x0022, 0x0c05, 0x1022, 0x2022,
            0x0023, 0x0c06, 0x1023, 0x2023, 0x0024, 0x0c07, 0x1024, 0x2024,
            0x0025, 0x0c08, 0x1025, 0x2025, 0x0026, 0x0c09, 0x1026, 0x2026,
            0x0027, 0x0c0a, 0x1027, 0x2027, 0x0028, 0x0c0b, 0x1028, 0x2028,
            0x0029, 0x0d04, 0x1029, 0x2029, 0x002a, 0x0d05, 0x102a, 0x202a,
            0x002b, 0x0d06, 0x102b, 0x202b, 0x002c, 0x0d07, 0x102c, 0x202c,
            0x002d, 0x0d08, 0x102d, 0x202d, 0x002e, 0x0d09, 0x102e, 0x202e,
            0x002f, 0x0d0a, 0x102f, 0x202f, 0x0030, 0x0d0b, 0x1030, 0x2030,
            0x0031, 0x0e04, 0x1031, 0x2031, 0x0032, 0x0e05, 0x1032, 0x2032,
            0x0033, 0x0e06, 0x1033, 0x2033, 0x0034, 0x0e07, 0x1034, 0x2034,
            0x0035, 0x0e08, 0x1035, 0x2035, 0x0036, 0x0e09, 0x1036, 0x2036,
            0x0037, 0x0e0a, 0x1037, 0x2037, 0x0038, 0x0e0b, 0x1038, 0x2038,
            0x0039, 0x0f04, 0x1039, 0x2039, 0x003a, 0x0f05, 0x103a, 0x203a,
            0x003b, 0x0f06, 0x103b, 0x203b, 0x003c, 0x0f07, 0x103c, 0x203c,
            0x0801, 0x0f08, 0x103d, 0x203d, 0x1001, 0x0f09, 0x103e, 0x203e,
            0x1801, 0x0f0a, 0x103f, 0x203f, 0x2001, 0x0f0b, 0x1040, 0x2040
        };
        }

        internal class CopyHelpers
        {

		#if NET6_0_OR_GREATER

        // Raw bytes for PshufbFillPatterns. This syntax returns a ReadOnlySpan<byte> that references
        // directly to the static data within the DLL. This is only supported with bytes due to things
        // like byte-ordering on various architectures, so we can reference Vector128<byte> directly.
        // It is however safe to convert to Vector128<byte> so we'll do that below with some casts
        // that are elided by JIT.
        private static ReadOnlySpan<byte> PshufbFillPatternsAsBytes => new byte[] {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, // Never referenced, here for padding
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1,
            0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0,
            0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3,
            0, 1, 2, 3, 4, 0, 1, 2, 3, 4, 0, 1, 2, 3, 4, 0,
            0, 1, 2, 3, 4, 5, 0, 1, 2, 3, 4, 5, 0, 1, 2, 3,
            0, 1, 2, 3, 4, 5, 6, 0, 1, 2, 3, 4, 5, 6, 0, 1
        };

        /// <summary>
        /// This is a table of shuffle control masks that can be used as the source
        /// operand for PSHUFB to permute the contents of the destination XMM register
        /// into a repeating byte pattern.
        /// </summary>
        private static ReadOnlySpan<Vector128<byte>> PshufbFillPatterns
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => MemoryMarshal.CreateReadOnlySpan(
                reference: ref Unsafe.As<byte, Vector128<byte>>(ref MemoryMarshal.GetReference(PshufbFillPatternsAsBytes)),
                length: 8);
        }

        /// <summary>
        /// j * (16 / j) for all j from 0 to 7. 0 is not actually used.
        /// </summary>
        private static ReadOnlySpan<byte> PatternSizeTable => new byte[] {0, 16, 16, 15, 16, 15, 12, 14};


#endif

            /// <summary>
            /// Copy [src, src+(opEnd-op)) to [op, (opEnd-op)) but faster than
            /// IncrementalCopySlow. buf_limit is the address past the end of the writable
            /// region of the buffer. May write past opEnd, but won't write past bufferEnd.
            /// </summary>
            /// <param name="source">Pointer to the source point in the buffer.</param>
            /// <param name="op">Pointer to the destination point in the buffer.</param>
            /// <param name="opEnd">Pointer to the end of the area to write in the buffer.</param>
            /// <param name="bufferEnd">Pointer past the end of the buffer.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void IncrementalCopy(ref byte source, ref byte op, ref byte opEnd, ref byte bufferEnd)
            {
                Debug.Assert(Unsafe.IsAddressLessThan(ref source, ref op));
                Debug.Assert(!Unsafe.IsAddressGreaterThan(ref op, ref opEnd));
                Debug.Assert(!Unsafe.IsAddressGreaterThan(ref opEnd, ref bufferEnd));
                // NOTE: The copy tags use 3 or 6 bits to store the copy length, so len <= 64.
                Debug.Assert(Unsafe.ByteOffset(ref op, ref opEnd) <= (nint)64);
                // NOTE: In practice the compressor always emits len >= 4, so it is ok to
                // assume that to optimize this function, but this is not guaranteed by the
                // compression format, so we have to also handle len < 4 in case the input
                // does not satisfy these conditions.

                int patternSize = (int)Unsafe.ByteOffset(ref source, ref op);

                if (patternSize < 8)
                {
				#if NET6_0_OR_GREATER
                if (Ssse3.IsSupported) // SSSE3
                {
                    // Load the first eight bytes into an 128-bit XMM register, then use PSHUFB
                    // to permute the register's contents in-place into a repeating sequence of
                    // the first "pattern_size" bytes.
                    // For example, suppose:
                    //    src       == "abc"
                    //    op        == op + 3
                    // After _mm_shuffle_epi8(), "pattern" will have five copies of "abc"
                    // followed by one byte of slop: abcabcabcabcabca.
                    //
                    // The non-SSE fallback implementation suffers from store-forwarding stalls
                    // because its loads and stores partly overlap. By expanding the pattern
                    // in-place, we avoid the penalty.

                    if (!Unsafe.IsAddressGreaterThan(ref op, ref Unsafe.Subtract(ref bufferEnd, 16)))
                    {
                        Vector128<byte> shuffleMask = PshufbFillPatterns[patternSize];

						#if NET7_0_OR_GREATER
							Vector128<byte> srcPattern = Vector128.LoadUnsafe(ref source);
						#else
							Vector128<byte> srcPattern = Unsafe.ReadUnaligned<Vector128<byte>>(ref source);
						#endif

                        Vector128<byte> pattern = Shuffle(srcPattern, shuffleMask);

                        // Get the new pattern size now that we've repeated it
                        patternSize = PatternSizeTable[patternSize];

                        // If we're getting to the very end of the buffer, don't overrun
                        ref byte loopEnd = ref Unsafe.Subtract(ref bufferEnd, 15);
                        if (Unsafe.IsAddressGreaterThan(ref loopEnd, ref opEnd))
                        {
                            loopEnd = ref opEnd;
                        }

                        while (Unsafe.IsAddressLessThan(ref op, ref loopEnd))
                        {
                            pattern.StoreUnsafe(ref op);
                            op = ref Unsafe.Add(ref op, patternSize);
                        }

                        if (!Unsafe.IsAddressLessThan(ref op, ref opEnd))
                        {
                            return;
                        }
                    }

                    IncrementalCopySlow(ref source, ref op, ref opEnd);
                    return;
                }
                else
                {
				#endif
                    // No SSSE3 Fallback

                    // If plenty of buffer space remains, expand the pattern to at least 8
                    // bytes. The way the following loop is written, we need 8 bytes of buffer
                    // space if pattern_size >= 4, 11 bytes if pattern_size is 1 or 3, and 10
                    // bytes if pattern_size is 2.  Precisely encoding that is probably not
                    // worthwhile; instead, invoke the slow path if we cannot write 11 bytes
                    // (because 11 are required in the worst case).
                    if (!Unsafe.IsAddressGreaterThan(ref op, ref Unsafe.Subtract(ref bufferEnd, 11)))
                    {
                        while (patternSize < 8)
                        {
                            UnalignedCopy64(in source, ref op);
                            op = ref Unsafe.Add(ref op, patternSize);
                            patternSize *= 2;
                        }

                        if (!Unsafe.IsAddressLessThan(ref op, ref opEnd))
                        {
                            return;
                        }
                    }
                    else
                    {
                        IncrementalCopySlow(ref source, ref op, ref opEnd);
                        return;
                    }
					#if NET6_0_OR_GREATER
					}
					#endif
                }

                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                Debug.Assert(patternSize >= 8);

                // Copy 2x 8 bytes at a time. Because op - src can be < 16, a single
                // UnalignedCopy128 might overwrite data in op. UnalignedCopy64 is safe
                // because expanding the pattern to at least 8 bytes guarantees that
                // op - src >= 8.
                //
                // Typically, the op_limit is the gating factor so try to simplify the loop
                // based on that.
                if (!Unsafe.IsAddressGreaterThan(ref opEnd, ref Unsafe.Subtract(ref bufferEnd, 16)))
                {
                    UnalignedCopy64(in source, ref op);
                    UnalignedCopy64(in Unsafe.Add(ref source, 8), ref Unsafe.Add(ref op, 8));

                    if (Unsafe.IsAddressLessThan(ref op, ref Unsafe.Subtract(ref opEnd, 16)))
                    {
                        UnalignedCopy64(in Unsafe.Add(ref source, 16), ref Unsafe.Add(ref op, 16));
                        UnalignedCopy64(in Unsafe.Add(ref source, 24), ref Unsafe.Add(ref op, 24));
                    }
                    if (Unsafe.IsAddressLessThan(ref op, ref Unsafe.Subtract(ref opEnd, 32)))
                    {
                        UnalignedCopy64(in Unsafe.Add(ref source, 32), ref Unsafe.Add(ref op, 32));
                        UnalignedCopy64(in Unsafe.Add(ref source, 40), ref Unsafe.Add(ref op, 40));
                    }
                    if (Unsafe.IsAddressLessThan(ref op, ref Unsafe.Subtract(ref opEnd, 48)))
                    {
                        UnalignedCopy64(in Unsafe.Add(ref source, 48), ref Unsafe.Add(ref op, 48));
                        UnalignedCopy64(in Unsafe.Add(ref source, 56), ref Unsafe.Add(ref op, 56));
                    }

                    return;
                }

                // Fall back to doing as much as we can with the available slop in the
                // buffer.

                for (ref byte loopEnd = ref Unsafe.Subtract(ref bufferEnd, 16);
                     Unsafe.IsAddressLessThan(ref op, ref loopEnd);
                     op = ref Unsafe.Add(ref op, 16), source = ref Unsafe.Add(ref source, 16))
                {
                    UnalignedCopy64(in source, ref op);
                    UnalignedCopy64(in Unsafe.Add(ref source, 8), ref Unsafe.Add(ref op, 8));
                }

                if (!Unsafe.IsAddressLessThan(ref op, ref opEnd))
                {
                    return;
                }

                // We only take this branch if we didn't have enough slop and we can do a
                // single 8 byte copy.
                if (!Unsafe.IsAddressGreaterThan(ref op, ref Unsafe.Subtract(ref bufferEnd, 8)))
                {
                    UnalignedCopy64(in source, ref op);
                    source = ref Unsafe.Add(ref source, 8);
                    op = ref Unsafe.Add(ref op, 8);
                }

                IncrementalCopySlow(ref source, ref op, ref opEnd);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void IncrementalCopySlow(ref byte source, ref byte op, ref byte opEnd)
            {
                while (Unsafe.IsAddressLessThan(ref op, ref opEnd))
                {
                    op = source;
                    op = ref Unsafe.Add(ref op, 1);
                    source = ref Unsafe.Add(ref source, 1);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void UnalignedCopy64(in byte source, ref byte destination)
            {
                long tempStackVar = Unsafe.As<byte, long>(ref Unsafe.AsRef(source));
                Unsafe.As<byte, long>(ref destination) = tempStackVar;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void UnalignedCopy128(in byte source, ref byte destination)
            {
                Guid tempStackVar = Unsafe.As<byte, Guid>(ref Unsafe.AsRef(source));
                Unsafe.As<byte, Guid>(ref destination) = tempStackVar;
            }
        }

        internal static class Crc32CAlgorithm
        {
            #region static

            private const uint Poly = 0x82F63B78u;

            private static readonly uint[] Table;

            static Crc32CAlgorithm()
            {
                var table = new uint[16 * 256];
                for (uint i = 0; i < 256; i++)
                {
                    uint res = i;
                    for (int t = 0; t < 16; t++)
                    {
                        for (int k = 0; k < 8; k++) res = (res & 1) == 1 ? Poly ^ (res >> 1) : (res >> 1);
                        table[(t * 256) + i] = res;
                    }
                }

                Table = table;
            }

            #endregion

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static uint Compute(ReadOnlySpan<byte> source)
            {
                return Append(0, source);
            }

            public static uint Append(uint crc, ReadOnlySpan<byte> source)
            {
                uint crcLocal = uint.MaxValue ^ crc;

				#if NET6_0_OR_GREATER
            // If available on the current CPU, use ARM CRC32C intrinsic operations.
            // The if Crc32 statements are optimized out by the JIT compiler based on CPU support.
            if (Crc32.IsSupported)
            {
                if (Crc32.Arm64.IsSupported)
                {
                    while (source.Length >= 8)
                    {
                        crcLocal = Crc32.Arm64.ComputeCrc32C(crcLocal, MemoryMarshal.Read<ulong>(source));
                        source = source.Slice(8);
                    }
                }

                // Process in 4-byte chunks
                while (source.Length >= 4)
                {
                    crcLocal = Crc32.ComputeCrc32C(crcLocal, MemoryMarshal.Read<uint>(source));
                    source = source.Slice(4);
                }

                // Process the remainder
                int j = 0;
                while (j < source.Length)
                {
                    crcLocal = Crc32.ComputeCrc32C(crcLocal, source[j++]);
                }

                return crcLocal ^ uint.MaxValue;
            }

            // If available on the current CPU, use Intel CRC32C intrinsic operations.
            // The Sse42 if statements are optimized out by the JIT compiler based on CPU support.
            else if (Sse42.IsSupported)
            {
                // Process in 8-byte chunks first if 64-bit
                if (Sse42.X64.IsSupported)
                {
                    if (source.Length >= 8)
                    {
                        // work with a ulong local during the loop to reduce typecasts
                        ulong crcLocalLong = crcLocal;

                        while (source.Length >= 8)
                        {
                            crcLocalLong = Sse42.X64.Crc32(crcLocalLong, MemoryMarshal.Read<ulong>(source));
                            source = source.Slice(8);
                        }

                        crcLocal = (uint) crcLocalLong;
                    }
                }

                // Process in 4-byte chunks
                while (source.Length >= 4)
                {
                    crcLocal = Sse42.Crc32(crcLocal, MemoryMarshal.Read<uint>(source));
                    source = source.Slice(4);
                }

                // Process the remainder
                int j = 0;
                while (j < source.Length)
                {
                    crcLocal = Sse42.Crc32(crcLocal, source[j++]);
                }

                return crcLocal ^ uint.MaxValue;
            }
				#endif

                uint[] table = Table;
                while (source.Length >= 16)
                {
                    var a = table[(3 * 256) + source[12]]
                            ^ table[(2 * 256) + source[13]]
                            ^ table[(1 * 256) + source[14]]
                            ^ table[(0 * 256) + source[15]];

                    var b = table[(7 * 256) + source[8]]
                            ^ table[(6 * 256) + source[9]]
                            ^ table[(5 * 256) + source[10]]
                            ^ table[(4 * 256) + source[11]];

                    var c = table[(11 * 256) + source[4]]
                            ^ table[(10 * 256) + source[5]]
                            ^ table[(9 * 256) + source[6]]
                            ^ table[(8 * 256) + source[7]];

                    var d = table[(15 * 256) + ((byte)crcLocal ^ source[0])]
                            ^ table[(14 * 256) + ((byte)(crcLocal >> 8) ^ source[1])]
                            ^ table[(13 * 256) + ((byte)(crcLocal >> 16) ^ source[2])]
                            ^ table[(12 * 256) + ((crcLocal >> 24) ^ source[3])];

                    crcLocal = d ^ c ^ b ^ a;
                    source = source.Slice(16);
                }

                for (int offset = 0; offset < source.Length; offset++)
                {
                    crcLocal = table[(byte)(crcLocal ^ source[offset])] ^ crcLocal >> 8;
                }

                return crcLocal ^ uint.MaxValue;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static uint ApplyMask(uint x) =>
                unchecked(((x >> 15) | (x << 17)) + 0xa282ead8);
        }

        internal class HashTable : IDisposable
        {
            private const int MinHashTableBits = 8;
            private const int MinHashTableSize = 1 << MinHashTableBits;

            private const int MaxHashTableBits = 14;
            private const int MaxHashTableSize = 1 << MaxHashTableBits;

            private ushort[]? _buffer;

            public void EnsureCapacity(int inputSize)
            {
                var maxFragmentSize = Math.Min(inputSize, (int)Constants.BlockSize);
                var tableSize = CalculateTableSize(maxFragmentSize);

                if (_buffer is null || tableSize < _buffer.Length)
                {
                    if (_buffer is not null)
                    {
                        ArrayPool<ushort>.Shared.Return(_buffer);
                    }

                    _buffer = ArrayPool<ushort>.Shared.Rent(tableSize);
                }
            }

            public Span<ushort> GetHashTable(int fragmentSize)
            {
                if (_buffer is null)
                {
                    ThrowHelper.ThrowInvalidOperationException("Buffer not initialized");
                }

                int hashTableSize = CalculateTableSize(fragmentSize);
                if (hashTableSize > _buffer.Length)
                {
                    ThrowHelper.ThrowInvalidOperationException("Insufficient buffer size");
                }

                Span<ushort> hashTable = _buffer.AsSpan(0, hashTableSize);
                MemoryMarshal.AsBytes(hashTable).Fill(0);

                return hashTable;
            }

            private int CalculateTableSize(int inputSize)
            {
                if (inputSize > MaxHashTableSize)
                {
                    return MaxHashTableSize;
                }

                if (inputSize < MinHashTableSize)
                {
                    return MinHashTableSize;
                }

                return 2 << Helpers.Log2Floor((uint)(inputSize - 1));
            }

            public void Dispose()
            {
                if (_buffer is not null)
                {
                    ArrayPool<ushort>.Shared.Return(_buffer);
                    _buffer = null;
                }
            }

            /// <summary>
            /// Given a table of uint16_t whose size is mask / 2 + 1, return a pointer to the
            /// relevant entry, if any, for the given bytes.  Any hash function will do,
            /// but a good hash function reduces the number of collisions and thus yields
            /// better compression for compressible input.
            ///
            /// REQUIRES: mask is 2 * (table_size - 1), and table_size is a power of two.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ref ushort TableEntry(ref ushort table, uint bytes, uint mask)
            {
                // Our choice is quicker-and-dirtier than the typical hash function;
                // empirically, that seems beneficial.  The upper bits of kMagic * bytes are a
                // higher-quality hash than the lower bits, so when using kMagic * bytes we
                // also shift right to get a higher-quality end result.  There's no similar
                // issue with a CRC because all of the output bits of a CRC are equally good
                // "hashes." So, a CPU instruction for CRC, if available, tends to be a good
                // choice.

                uint hash;

				#if NET6_0_OR_GREATER
				// We use mask as the second arg to the CRC function, as it's about to
				// be used anyway; it'd be equally correct to use 0 or some constant.
				// Mathematically, _mm_crc32_u32 (or similar) is a function of the
				// xor of its arguments.

				if (System.Runtime.Intrinsics.X86.Sse42.IsSupported)
				{
					hash = Sse42.Crc32(bytes, mask);

				}
				else if (System.Runtime.Intrinsics.Arm.Crc32.IsSupported)
				{
					hash = Crc32.ComputeCrc32C(bytes, mask);
				}
				else
				#endif
                {
                    const uint kMagic = 0x1e35a7bd;
                    hash = (kMagic * bytes) >> (31 - MaxHashTableBits);
                }

                return ref Unsafe.AddByteOffset(ref table, hash & mask);
            }
        }

        internal static class Helpers
        {

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int MaxCompressedLength(int sourceBytes)
            {
                // Compressed data can be defined as:
                //    compressed := item* literal*
                //    item       := literal* copy
                //
                // The trailing literal sequence has a space blowup of at most 62/60
                // since a literal of length 60 needs one tag byte + one extra byte
                // for length information.
                //
                // We also add one extra byte to the blowup to account for the use of
                // "ref byte" pointers. The output index will be pushed one byte past
                // the end of the output data, but for safety we need to ensure that
                // it still points to an element in the buffer array.
                //
                // Item blowup is trickier to measure.  Suppose the "copy" op copies
                // 4 bytes of data.  Because of a special check in the encoding code,
                // we produce a 4-byte copy only if the offset is < 65536.  Therefore
                // the copy op takes 3 bytes to encode, and this type of item leads
                // to at most the 62/60 blowup for representing literals.
                //
                // Suppose the "copy" op copies 5 bytes of data.  If the offset is big
                // enough, it will take 5 bytes to encode the copy op.  Therefore the
                // worst case here is a one-byte literal followed by a five-byte copy.
                // I.e., 6 bytes of input turn into 7 bytes of "compressed" data.
                //
                // This last factor dominates the blowup, so the final estimate is:

                return 32 + sourceBytes + sourceBytes / 6 + 1;
            }

            private static ReadOnlySpan<byte> LeftShiftOverflowsMasks => new byte[]
            {
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x80, 0xc0, 0xe0, 0xf0, 0xf8, 0xfc, 0xfe
            };

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool LeftShiftOverflows(byte value, int shift) =>
                (value & LeftShiftOverflowsMasks[shift]) != 0;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static uint ExtractLowBytes(uint value, int numBytes)
            {
                Debug.Assert(numBytes >= 0);
                Debug.Assert(numBytes <= 4);

#if NET6_0_OR_GREATER
            if (Bmi2.IsSupported)
            {
                return Bmi2.ZeroHighBits(value, (uint)(numBytes * 8));
            }
            else
            {
                return value & ~(0xffffffff << (8 * numBytes));
            }
#else
                return value & ~(0xffffffff << (8 * numBytes));
#endif
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static uint UnsafeReadUInt32(ref byte ptr)
            {
                var result = Unsafe.ReadUnaligned<uint>(ref ptr);
                if (!BitConverter.IsLittleEndian)
                {
                    result = BinaryPrimitives.ReverseEndianness(result);
                }

                return result;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ulong UnsafeReadUInt64(ref byte ptr)
            {
                var result = Unsafe.ReadUnaligned<ulong>(ref ptr);
                if (!BitConverter.IsLittleEndian)
                {
                    result = BinaryPrimitives.ReverseEndianness(result);
                }

                return result;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void UnsafeWriteUInt32(ref byte ptr, uint value)
            {
                if (!BitConverter.IsLittleEndian)
                {
                    value = BinaryPrimitives.ReverseEndianness(value);
                }

                Unsafe.WriteUnaligned(ref ptr, value);
            }

#if NET6_0

        // Port of the method from .NET 7, but specific to bytes

        /// <summary>Stores a vector at the given destination.</summary>
        /// <param name="source">The vector that will be stored.</param>
        /// <param name="destination">The destination at which <paramref name="source" /> will be stored.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void StoreUnsafe(this Vector128<byte> source, ref byte destination)
        {
            Unsafe.WriteUnaligned(ref destination, source);
        }

#endif

#if !NET6_0_OR_GREATER

            // Port from .NET 7 BitOperations of a faster fallback algorithm for .NET Standard since we don't have intrinsics
            // or BitOperations. This is the same algorithm used by BitOperations.Log2 when hardware acceleration is unavailable.
            // https://github.com/dotnet/runtime/blob/bee217ffbdd6b3ad60b0e1e17c6370f4bb618779/src/libraries/System.Private.CoreLib/src/System/Numerics/BitOperations.cs#L404

            private static ReadOnlySpan<byte> Log2DeBruijn => new byte[32]
            {
            00, 09, 01, 10, 13, 21, 02, 29,
            11, 14, 16, 18, 22, 25, 03, 30,
            08, 12, 20, 28, 15, 17, 24, 07,
            19, 27, 23, 06, 26, 05, 04, 31
            };

            /// <summary>
            /// Returns the integer (floor) log of the specified value, base 2.
            /// Note that by convention, input value 0 returns 0 since Log(0) is undefined.
            /// Does not directly use any hardware intrinsics, nor does it incur branching.
            /// </summary>
            /// <param name="value">The value.</param>
            private static int Log2SoftwareFallback(uint value)
            {
                // No AggressiveInlining due to large method size
                // Has conventional contract 0->0 (Log(0) is undefined)

                // Fill trailing zeros with ones, eg 00010010 becomes 00011111
                value |= value >> 01;
                value |= value >> 02;
                value |= value >> 04;
                value |= value >> 08;
                value |= value >> 16;

                // uint.MaxValue >> 27 is always in range [0 - 31] so we use Unsafe.AddByteOffset to avoid bounds check
                return Unsafe.AddByteOffset(
                    // Using deBruijn sequence, k=2, n=5 (2^5=32) : 0b_0000_0111_1100_0100_1010_1100_1101_1101u
                    ref MemoryMarshal.GetReference(Log2DeBruijn),
                    // uint|long -> IntPtr cast on 32-bit platforms does expensive overflow checks not needed here
                    (IntPtr)(int)((value * 0x07C4ACDDu) >> 27));
            }

#endif

            /// <summary>
            /// Return floor(log2(n)) for positive integer n.  Returns -1 if n == 0.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int Log2Floor(uint n) =>
                n == 0 ? -1 : Log2FloorNonZero(n);


            /// <summary>
            /// Return floor(log2(n)) for positive integer n.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int Log2FloorNonZero(uint n)
            {
                Debug.Assert(n != 0);

#if NET6_0_OR_GREATER
            return BitOperations.Log2(n);
#else
                return Log2SoftwareFallback(n);
#endif
            }

            /// <summary>
            /// Finds the index of the least significant non-zero bit.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int FindLsbSetNonZero(uint n)
            {
                Debug.Assert(n != 0);

#if NET6_0_OR_GREATER
            return BitOperations.TrailingZeroCount(n);
#else
                int rc = 31;
                int shift = 1 << 4;

                for (int i = 4; i >= 0; --i)
                {
                    uint x = n << shift;
                    if (x != 0)
                    {
                        n = x;
                        rc -= shift;
                    }

                    shift >>= 1;
                }

                return rc;
#endif
            }

            /// <summary>
            /// Finds the index of the least significant non-zero bit.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int FindLsbSetNonZero(ulong n)
            {
                Debug.Assert(n != 0);

#if NET6_0_OR_GREATER
            return BitOperations.TrailingZeroCount(n);
#else
                uint bottomBits = unchecked((uint)n);
                if (bottomBits == 0)
                {
                    return 32 + FindLsbSetNonZero(unchecked((uint)(n >> 32)));
                }
                else
                {
                    return FindLsbSetNonZero(bottomBits);
                }
#endif
            }
        }

        /* ************************************************************
         *
         *    @author Couchbase <info@couchbase.com>
         *    @copyright 2021 Couchbase, Inc.
         *
         *    Licensed under the Apache License, Version 2.0 (the "License");
         *    you may not use this file except in compliance with the License.
         *    You may obtain a copy of the License at
         *
         *        http://www.apache.org/licenses/LICENSE-2.0
         *
         *    Unless required by applicable law or agreed to in writing, software
         *    distributed under the License is distributed on an "AS IS" BASIS,
         *    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
         *    See the License for the specific language governing permissions and
         *    limitations under the License.
         *
         * ************************************************************/

        internal class SnappyCompressor : IDisposable
        {
            private HashTable? _workingMemory = new();

            public int Compress(ReadOnlySpan<byte> input, Span<byte> output)
            {
                if (_workingMemory == null)
                {
                    ThrowHelper.ThrowObjectDisposedException(nameof(SnappyCompressor));
                }

                _workingMemory.EnsureCapacity(input.Length);

                int bytesWritten = WriteUncompressedLength(output, input.Length);
                output = output.Slice(bytesWritten);

                while (input.Length > 0)
                {
                    var fragment = input.Slice(0, Math.Min(input.Length, (int)Constants.BlockSize));

                    var hashTable = _workingMemory.GetHashTable(fragment.Length);

                    var maxOutput = Helpers.MaxCompressedLength(fragment.Length);

                    if (output.Length >= maxOutput)
                    {
                        var written = CompressFragment(fragment, output, hashTable);

                        output = output.Slice(written);
                        bytesWritten += written;
                    }
                    else
                    {
                        var scratch = ArrayPool<byte>.Shared.Rent(maxOutput);
                        try
                        {
                            int written = CompressFragment(fragment, scratch.AsSpan(), hashTable);
                            if (output.Length < written)
                            {
                                ThrowHelper.ThrowArgumentException("Insufficient output buffer", nameof(output));
                            }

                            scratch.AsSpan(0, written).CopyTo(output);
                            output = output.Slice(written);
                            bytesWritten += written;
                        }
                        finally
                        {
                            ArrayPool<byte>.Shared.Return(scratch);
                        }
                    }

                    input = input.Slice(fragment.Length);
                }

                return bytesWritten;
            }

            public void Dispose()
            {
                _workingMemory?.Dispose();
                _workingMemory = null;
            }

            private static int WriteUncompressedLength(Span<byte> output, int length)
            {
                const int b = 0b1000_0000;

                unchecked
                {
                    if (length < (1 << 7))
                    {
                        output[0] = (byte)length;
                        return 1;
                    }
                    else if (length < (1 << 14))
                    {
                        output[0] = (byte)(length | b);
                        output[1] = (byte)(length >> 7);
                        return 2;
                    }
                    else if (length < (1 << 21))
                    {
                        output[0] = (byte)(length | b);
                        output[1] = (byte)((length >> 7) | b);
                        output[2] = (byte)(length >> 14);
                        return 3;
                    }
                    else if (length < (1 << 28))
                    {
                        output[0] = (byte)(length | b);
                        output[1] = (byte)((length >> 7) | b);
                        output[2] = (byte)((length >> 14) | b);
                        output[3] = (byte)(length >> 21);
                        return 4;
                    }
                    else
                    {
                        output[0] = (byte)(length | b);
                        output[1] = (byte)((length >> 7) | b);
                        output[2] = (byte)((length >> 14) | b);
                        output[3] = (byte)((length >> 21) | b);
                        output[4] = (byte)(length >> 28);
                        return 5;
                    }
                }
            }

            #region CompressFragment

            private static int CompressFragment(ReadOnlySpan<byte> input, Span<byte> output, Span<ushort> tableSpan)
            {
                unchecked
                {
                    Debug.Assert(input.Length <= Constants.BlockSize);
                    Debug.Assert((tableSpan.Length & (tableSpan.Length - 1)) == 0); // table must be power of two

                    uint mask = (uint)(2 * (tableSpan.Length - 1));

                    ref byte inputStart = ref Unsafe.AsRef(in input[0]);
                    ref byte inputEnd = ref Unsafe.Add(ref inputStart, input.Length);
                    ref byte ip = ref inputStart;

                    ref byte op = ref output[0];
                    ref ushort table = ref tableSpan[0];

                    if (input.Length >= Constants.InputMarginBytes)
                    {
                        ref byte ipLimit = ref Unsafe.Subtract(ref inputEnd, Constants.InputMarginBytes);

                        for (uint preload = Helpers.UnsafeReadUInt32(ref Unsafe.Add(ref ip, 1)); ;)
                        {
                            // Bytes in [nextEmit, ip) will be emitted as literal bytes.  Or
                            // [nextEmit, ipEnd) after the main loop.
                            ref byte nextEmit = ref ip;
                            ip = ref Unsafe.Add(ref ip, 1);
                            ulong data = Helpers.UnsafeReadUInt64(ref ip);

                            // The body of this loop calls EmitLiteral once and then EmitCopy one or
                            // more times.  (The exception is that when we're close to exhausting
                            // the input we goto emit_remainder.)
                            //
                            // In the first iteration of this loop we're just starting, so
                            // there's nothing to copy, so calling EmitLiteral once is
                            // necessary.  And we only start a new iteration when the
                            // current iteration has determined that a call to EmitLiteral will
                            // precede the next call to EmitCopy (if any).
                            //
                            // Step 1: Scan forward in the input looking for a 4-byte-long match.
                            // If we get close to exhausting the input then goto emit_remainder.
                            //
                            // Heuristic match skipping: If 32 bytes are scanned with no matches
                            // found, start looking only at every other byte. If 32 more bytes are
                            // scanned (or skipped), look at every third byte, etc.. When a match is
                            // found, immediately go back to looking at every byte. This is a small
                            // loss (~5% performance, ~0.1% density) for compressible data due to more
                            // bookkeeping, but for non-compressible data (such as JPEG) it's a huge
                            // win since the compressor quickly "realizes" the data is incompressible
                            // and doesn't bother looking for matches everywhere.
                            //
                            // The "skip" variable keeps track of how many bytes there are since the
                            // last match; dividing it by 32 (ie. right-shifting by five) gives the
                            // number of bytes to move ahead for each iteration.
                            int skip = 32;

                            ref byte candidate = ref Unsafe.NullRef<byte>();
                            if (Unsafe.ByteOffset(ref ip, ref ipLimit) >= (nint)16)
                            {
                                nint delta = Unsafe.ByteOffset(ref inputStart, ref ip);
                                for (int j = 0; j < 16; j += 4)
                                {
                                    // Manually unroll this loop into chunks of 4

                                    uint dword = j == 0 ? preload : (uint)data;
                                    Debug.Assert(dword == Helpers.UnsafeReadUInt32(ref Unsafe.Add(ref ip, j)));
                                    ref ushort tableEntry = ref HashTable.TableEntry(ref table, dword, mask);
                                    candidate = ref Unsafe.Add(ref inputStart, tableEntry);
                                    Debug.Assert(!Unsafe.IsAddressLessThan(ref candidate, ref inputStart));
                                    Debug.Assert(Unsafe.IsAddressLessThan(ref candidate, ref Unsafe.Add(ref ip, j)));
                                    tableEntry = (ushort)(delta + j);

                                    if (Helpers.UnsafeReadUInt32(ref candidate) == dword)
                                    {
                                        op = (byte)(Constants.Literal | (j << 2));
                                        CopyHelpers.UnalignedCopy128(in nextEmit, ref Unsafe.Add(ref op, 1));
                                        ip = ref Unsafe.Add(ref ip, j);
                                        op = ref Unsafe.Add(ref op, j + 2);
                                        goto emit_match;
                                    }

                                    int i1 = j + 1;
                                    dword = (uint)(data >> 8);
                                    Debug.Assert(dword == Helpers.UnsafeReadUInt32(ref Unsafe.Add(ref ip, i1)));
                                    tableEntry = ref HashTable.TableEntry(ref table, dword, mask);
                                    candidate = ref Unsafe.Add(ref inputStart, tableEntry);
                                    Debug.Assert(!Unsafe.IsAddressLessThan(ref candidate, ref inputStart));
                                    Debug.Assert(Unsafe.IsAddressLessThan(ref candidate, ref Unsafe.Add(ref ip, i1)));
                                    tableEntry = (ushort)(delta + i1);

                                    if (Helpers.UnsafeReadUInt32(ref candidate) == dword)
                                    {
                                        op = (byte)(Constants.Literal | (i1 << 2));
                                        CopyHelpers.UnalignedCopy128(in nextEmit, ref Unsafe.Add(ref op, 1));
                                        ip = ref Unsafe.Add(ref ip, i1);
                                        op = ref Unsafe.Add(ref op, i1 + 2);
                                        goto emit_match;
                                    }

                                    int i2 = j + 2;
                                    dword = (uint)(data >> 16);
                                    Debug.Assert(dword == Helpers.UnsafeReadUInt32(ref Unsafe.Add(ref ip, i2)));
                                    tableEntry = ref HashTable.TableEntry(ref table, dword, mask);
                                    candidate = ref Unsafe.Add(ref inputStart, tableEntry);
                                    Debug.Assert(!Unsafe.IsAddressLessThan(ref candidate, ref inputStart));
                                    Debug.Assert(Unsafe.IsAddressLessThan(ref candidate, ref Unsafe.Add(ref ip, i2)));
                                    tableEntry = (ushort)(delta + i2);

                                    if (Helpers.UnsafeReadUInt32(ref candidate) == dword)
                                    {
                                        op = (byte)(Constants.Literal | (i2 << 2));
                                        CopyHelpers.UnalignedCopy128(in nextEmit, ref Unsafe.Add(ref op, 1));
                                        ip = ref Unsafe.Add(ref ip, i2);
                                        op = ref Unsafe.Add(ref op, i2 + 2);
                                        goto emit_match;
                                    }

                                    int i3 = j + 3;
                                    dword = (uint)(data >> 24);
                                    Debug.Assert(dword == Helpers.UnsafeReadUInt32(ref Unsafe.Add(ref ip, i3)));
                                    tableEntry = ref HashTable.TableEntry(ref table, dword, mask);
                                    candidate = ref Unsafe.Add(ref inputStart, tableEntry);
                                    Debug.Assert(!Unsafe.IsAddressLessThan(ref candidate, ref inputStart));
                                    Debug.Assert(Unsafe.IsAddressLessThan(ref candidate, ref Unsafe.Add(ref ip, i3)));
                                    tableEntry = (ushort)(delta + i3);

                                    if (Helpers.UnsafeReadUInt32(ref candidate) == dword)
                                    {
                                        op = (byte)(Constants.Literal | (i3 << 2));
                                        CopyHelpers.UnalignedCopy128(in nextEmit, ref Unsafe.Add(ref op, 1));
                                        ip = ref Unsafe.Add(ref ip, i3);
                                        op = ref Unsafe.Add(ref op, i3 + 2);
                                        goto emit_match;
                                    }

                                    data = Helpers.UnsafeReadUInt64(ref Unsafe.Add(ref ip, j + 4));
                                }

                                ip = ref Unsafe.Add(ref ip, 16);
                                skip += 16;
                            }

                            while (true)
                            {
                                Debug.Assert((uint)data == Helpers.UnsafeReadUInt32(ref ip));
                                ref ushort tableEntry = ref HashTable.TableEntry(ref table, (uint)data, mask);
                                int bytesBetweenHashLookups = skip >> 5;
                                skip += bytesBetweenHashLookups;

                                ref byte nextIp = ref Unsafe.Add(ref ip, bytesBetweenHashLookups);
                                if (Unsafe.IsAddressGreaterThan(ref nextIp, ref ipLimit))
                                {
                                    ip = ref nextEmit;
                                    goto emit_remainder;
                                }

                                candidate = ref Unsafe.Add(ref inputStart, tableEntry);
                                Debug.Assert(!Unsafe.IsAddressLessThan(ref candidate, ref inputStart));
                                Debug.Assert(Unsafe.IsAddressLessThan(ref candidate, ref ip));

                                tableEntry = (ushort)Unsafe.ByteOffset(ref inputStart, ref ip);
                                if ((uint)data == Helpers.UnsafeReadUInt32(ref candidate))
                                {
                                    break;
                                }

                                data = Helpers.UnsafeReadUInt32(ref nextIp);
                                ip = ref nextIp;
                            }

                            // Step 2: A 4-byte match has been found.  We'll later see if more
                            // than 4 bytes match.  But, prior to the match, input
                            // bytes [next_emit, ip) are unmatched.  Emit them as "literal bytes."
                            Debug.Assert(!Unsafe.IsAddressGreaterThan(ref Unsafe.Add(ref nextEmit, 16), ref inputEnd));
                            op = ref EmitLiteralFast(ref op, ref nextEmit, (uint)Unsafe.ByteOffset(ref nextEmit, ref ip));

                        // Step 3: Call EmitCopy, and then see if another EmitCopy could
                        // be our next move.  Repeat until we find no match for the
                        // input immediately after what was consumed by the last EmitCopy call.
                        //
                        // If we exit this loop normally then we need to call EmitLiteral next,
                        // though we don't yet know how big the literal will be.  We handle that
                        // by proceeding to the next iteration of the main loop.  We also can exit
                        // this loop via goto if we get close to exhausting the input.

                        emit_match:
                            do
                            {
                                // We have a 4-byte match at ip, and no need to emit any
                                // "literal bytes" prior to ip.
                                ref byte emitBase = ref ip;

                                var (matchLength, matchLengthLessThan8) =
                                    FindMatchLength(ref Unsafe.Add(ref candidate, 4), ref Unsafe.Add(ref ip, 4), ref inputEnd, ref data);

                                int matched = 4 + matchLength;
                                ip = ref Unsafe.Add(ref ip, matched);

                                nint offset = Unsafe.ByteOffset(ref candidate, ref emitBase);
                                if (matchLengthLessThan8)
                                {
                                    op = ref EmitCopyLenLessThan12(ref op, offset, matched);
                                }
                                else
                                {
                                    op = ref EmitCopyLenGreaterThanOrEqualTo12(ref op, offset, matched);
                                }

                                if (!Unsafe.IsAddressLessThan(ref ip, ref ipLimit))
                                {
                                    goto emit_remainder;
                                }

                                // Expect 5 bytes to match
                                Debug.Assert((data & 0xfffffffffful) ==
                                             (Helpers.UnsafeReadUInt64(ref ip) & 0xfffffffffful));

                                // We are now looking for a 4-byte match again.  We read
                                // table[Hash(ip, mask)] for that.  To improve compression,
                                // we also update table[Hash(ip - 1, mask)] and table[Hash(ip, mask)].
                                HashTable.TableEntry(ref table, Helpers.UnsafeReadUInt32(ref Unsafe.Subtract(ref ip, 1)), mask) =
                                    (ushort)(Unsafe.ByteOffset(ref inputStart, ref ip) - 1);
                                ref ushort tableEntry = ref HashTable.TableEntry(ref table, (uint)data, mask);
                                candidate = ref Unsafe.Add(ref inputStart, tableEntry);
                                tableEntry = (ushort)Unsafe.ByteOffset(ref inputStart, ref ip);
                            } while ((uint)data == Helpers.UnsafeReadUInt32(ref candidate));

                            // Because the least significant 5 bytes matched, we can utilize data
                            // for the next iteration.
                            preload = (uint)(data >> 8);
                        }
                    }

                emit_remainder:
                    // Emit the remaining bytes as a literal
                    if (Unsafe.IsAddressLessThan(ref ip, ref inputEnd))
                    {
                        op = ref EmitLiteralSlow(ref op, ref ip, (uint)Unsafe.ByteOffset(ref ip, ref inputEnd));
                    }

                    return (int)Unsafe.ByteOffset(ref output[0], ref op);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static ref byte EmitLiteralFast(ref byte op, ref byte literal, uint length)
            {
                Debug.Assert(length > 0);

                if (length <= 16)
                {
                    uint n = length - 1;
                    op = unchecked((byte)(Constants.Literal | (n << 2)));
                    op = ref Unsafe.Add(ref op, 1);

                    CopyHelpers.UnalignedCopy128(in literal, ref op);
                    return ref Unsafe.Add(ref op, length);
                }

                return ref EmitLiteralSlow(ref op, ref literal, length);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static ref byte EmitLiteralSlow(ref byte op, ref byte literal, uint length)
            {
                uint n = length - 1;
                if (n < 60)
                {
                    op = unchecked((byte)(Constants.Literal | (n << 2)));
                    op = ref Unsafe.Add(ref op, 1);
                }
                else
                {
                    int count = (Helpers.Log2Floor(n) >> 3) + 1;

                    Debug.Assert(count >= 1);
                    Debug.Assert(count <= 4);
                    op = unchecked((byte)(Constants.Literal | ((59 + count) << 2)));
                    op = ref Unsafe.Add(ref op, 1);

                    // Encode in upcoming bytes.
                    // Write 4 bytes, though we may care about only 1 of them. The output buffer
                    // is guaranteed to have at least 3 more spaces left as 'len >= 61' holds
                    // here and there is a std::memcpy() of size 'len' below.
                    Helpers.UnsafeWriteUInt32(ref op, n);
                    op = ref Unsafe.Add(ref op, count);
                }

                Unsafe.CopyBlockUnaligned(ref op, ref literal, length);
                return ref Unsafe.Add(ref op, length);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static ref byte EmitCopyAtMost64LenLessThan12(ref byte op, long offset, long length)
            {
                Debug.Assert(length <= 64);
                Debug.Assert(length >= 4);
                Debug.Assert(offset < 65536);
                Debug.Assert(length < 12);

                unchecked
                {
                    uint u = (uint)((length << 2) + (offset << 8));
                    uint copy1 = (uint)(Constants.Copy1ByteOffset - (4 << 2) + ((offset >> 3) & 0xe0));
                    uint copy2 = (uint)(Constants.Copy2ByteOffset - (1 << 2));

                    // It turns out that offset < 2048 is a difficult to predict branch.
                    // `perf record` shows this is the highest percentage of branch misses in
                    // benchmarks. This code produces branch free code, the data dependency
                    // chain that bottlenecks the throughput is so long that a few extra
                    // instructions are completely free (IPC << 6 because of data deps).
                    u += offset < 2048 ? copy1 : copy2;
                    Helpers.UnsafeWriteUInt32(ref op, u);
                }

                return ref Unsafe.Add(ref op, offset < 2048 ? 2 : 3);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static ref byte EmitCopyAtMost64LenGreaterThanOrEqualTo12(ref byte op, long offset, long length)
            {
                Debug.Assert(length <= 64);
                Debug.Assert(length >= 4);
                Debug.Assert(offset < 65536);
                Debug.Assert(length >= 12);

                // Write 4 bytes, though we only care about 3 of them.  The output buffer
                // is required to have some slack, so the extra byte won't overrun it.
                var u = unchecked((uint)(Constants.Copy2ByteOffset + ((length - 1) << 2) + (offset << 8)));
                Helpers.UnsafeWriteUInt32(ref op, u);
                return ref Unsafe.Add(ref op, 3);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static ref byte EmitCopyLenLessThan12(ref byte op, long offset, long length)
            {
                Debug.Assert(length < 12);

                return ref EmitCopyAtMost64LenLessThan12(ref op, offset, length);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static ref byte EmitCopyLenGreaterThanOrEqualTo12(ref byte op, long offset, long length)
            {
                Debug.Assert(length >= 12);

                // A special case for len <= 64 might help, but so far measurements suggest
                // it's in the noise.

                // Emit 64 byte copies but make sure to keep at least four bytes reserved.
                while (length >= 68)
                {
                    op = ref EmitCopyAtMost64LenGreaterThanOrEqualTo12(ref op, offset, 64);
                    length -= 64;
                }

                // One or two copies will now finish the job.
                if (length > 64)
                {
                    op = ref EmitCopyAtMost64LenGreaterThanOrEqualTo12(ref op, offset, 60);
                    length -= 60;
                }

                // Emit remainder.
                if (length < 12)
                {
                    op = ref EmitCopyAtMost64LenLessThan12(ref op, offset, length);
                }
                else
                {
                    op = ref EmitCopyAtMost64LenGreaterThanOrEqualTo12(ref op, offset, length);
                }
                return ref op;
            }

            /// <summary>
            /// Find the largest n such that
            ///
            ///   s1[0,n-1] == s2[0,n-1]
            ///   and n &lt;= (s2_limit - s2).
            ///
            /// Return (n, n &lt; 8).
            /// Reads up to and including *s2_limit but not beyond.
            /// Does not read *(s1 + (s2_limit - s2)) or beyond.
            /// Requires that s2_limit &gt;= s2.
            ///
            /// In addition populate *data with the next 5 bytes from the end of the match.
            /// This is only done if 8 bytes are available (s2_limit - s2 &gt;= 8). The point is
            /// that on some arch's this can be done faster in this routine than subsequent
            /// loading from s2 + n.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static (int matchLength, bool matchLengthLessThan8) FindMatchLength(
                ref byte s1, ref byte s2, ref byte s2Limit, ref ulong data)
            {
                Debug.Assert(!Unsafe.IsAddressLessThan(ref s2Limit, ref s2));

                if (BitConverter.IsLittleEndian && IntPtr.Size == 8)
                {
                    // Special implementation for 64-bit little endian processors (i.e. Intel/AMD x64)
                    return FindMatchLengthX64(ref s1, ref s2, ref s2Limit, ref data);
                }

                int matched = 0;

                while (Unsafe.ByteOffset(ref s2, ref s2Limit) >= (nint)4
                       && Helpers.UnsafeReadUInt32(ref s2) == Helpers.UnsafeReadUInt32(ref Unsafe.Add(ref s1, matched)))
                {
                    s2 = ref Unsafe.Add(ref s2, 4);
                    matched += 4;
                }

                if (BitConverter.IsLittleEndian && Unsafe.ByteOffset(ref s2, ref s2Limit) >= (nint)4)
                {
                    uint x = Helpers.UnsafeReadUInt32(ref s2) ^ Helpers.UnsafeReadUInt32(ref Unsafe.Add(ref s1, matched));
                    int matchingBits = Helpers.FindLsbSetNonZero(x);
                    matched += matchingBits >> 3;
                    s2 = ref Unsafe.Add(ref s2, matchingBits >> 3);
                }
                else
                {
                    while (Unsafe.IsAddressLessThan(ref s2, ref s2Limit) && Unsafe.Add(ref s1, matched) == s2)
                    {
                        s2 = ref Unsafe.Add(ref s2, 1);
                        ++matched;
                    }
                }

                if (Unsafe.ByteOffset(ref s2, ref s2Limit) >= (nint)8)
                {
                    data = Helpers.UnsafeReadUInt64(ref s2);
                }

                return (matched, matched < 8);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static (int matchLength, bool matchLengthLessThan8) FindMatchLengthX64(
                ref byte s1, ref byte s2, ref byte s2Limit, ref ulong data)
            {
                nint matched = 0;

                // This block isn't necessary for correctness; we could just start looping
                // immediately.  As an optimization though, it is useful.  It creates some not
                // uncommon code paths that determine, without extra effort, whether the match
                // length is less than 8.
                if (Unsafe.ByteOffset(ref s2, ref s2Limit) >= (nint)16)
                {
                    ulong a1 = Helpers.UnsafeReadUInt64(ref s1);
                    ulong a2 = Helpers.UnsafeReadUInt64(ref s2);

                    if (a1 != a2)
                    {
                        ulong xorval = a1 ^ a2;
                        int shift = Helpers.FindLsbSetNonZero(xorval);
                        int matchedBytes = shift >> 3;

                        ulong a3 = Helpers.UnsafeReadUInt64(ref Unsafe.Add(ref s2, 4));
                        a2 = unchecked((uint)xorval) == 0 ? a3 : a2;

                        data = a2 >> (shift & (3 * 8));
                        return (matchedBytes, true);
                    }
                    else
                    {
                        matched = 8;
                        s2 = ref Unsafe.Add(ref s2, 8);
                    }
                }

                // Find out how long the match is. We loop over the data 64 bits at a
                // time until we find a 64-bit block that doesn't match; then we find
                // the first non-matching bit and use that to calculate the total
                // length of the match.
                while (Unsafe.ByteOffset(ref s2, ref s2Limit) >= (nint)16)
                {
                    ulong a1 = Helpers.UnsafeReadUInt64(ref Unsafe.Add(ref s1, matched));
                    ulong a2 = Helpers.UnsafeReadUInt64(ref s2);
                    if (a1 == a2)
                    {
                        s2 = ref Unsafe.Add(ref s2, 8);
                        matched += 8;
                    }
                    else
                    {
                        ulong xorval = a1 ^ a2;
                        int shift = Helpers.FindLsbSetNonZero(xorval);
                        int matchedBytes = shift >> 3;

                        ulong a3 = Helpers.UnsafeReadUInt64(ref Unsafe.Add(ref s2, 4));
                        a2 = unchecked((uint)xorval) == 0 ? a3 : a2;

                        data = a2 >> (shift & (3 * 8));
                        matched += matchedBytes;
                        Debug.Assert(matched >= 8);
                        return ((int)matched, false);
                    }
                }

                while (Unsafe.IsAddressLessThan(ref s2, ref s2Limit))
                {
                    if (Unsafe.Add(ref s1, matched) == s2)
                    {
                        s2 = ref Unsafe.Add(ref s2, 1);
                        matched++;
                    }
                    else
                    {
                        if (Unsafe.ByteOffset(ref s2, ref s2Limit) >= (nint)8)
                        {
                            data = Helpers.UnsafeReadUInt64(ref s2);
                        }

                        return ((int)matched, matched < 8);
                    }
                }

                return ((int)matched, matched < 8);
            }

            #endregion
        }

        internal sealed class SnappyDecompressor : IDisposable
        {
            private byte[] _scratch = new byte[Constants.MaximumTagLength];
            private uint _scratchLength = 0;

            private int _remainingLiteral;

            private int _uncompressedLengthShift;
            private int _uncompressedLength;

            public bool NeedMoreData => !AllDataDecompressed && UnreadBytes == 0;

            /// <summary>
            /// Decompress a portion of the input.
            /// </summary>
            /// <param name="input">Input to process.</param>
            /// <returns>Number of bytes processed from the input.</returns>
            /// <remarks>
            /// The first call to this method after construction or after a call to <see cref="Reset"/> start at the
            /// beginning of a new Snappy block, leading with the encoded block size. It may be called multiple times
            /// as more data becomes available. <see cref="AllDataDecompressed"/> will be true once the entire block
            /// has been processed.
            /// </remarks>
            public void Decompress(ReadOnlySpan<byte> input)
            {
                if (!ExpectedLength.HasValue)
                {
                    var readLength = ReadUncompressedLength(ref input);
                    if (readLength.HasValue)
                    {
                        ExpectedLength = readLength.GetValueOrDefault();
                    }
                    else
                    {
                        // Not enough data yet to process the length
                        return;
                    }
                }

                // Process any input into the write buffer

                if (input.Length > 0)
                {
                    if (_remainingLiteral > 0)
                    {
                        int toWrite = Math.Min(_remainingLiteral, input.Length);

                        Append(input.Slice(0, toWrite));
                        input = input.Slice(toWrite);
                        _remainingLiteral -= toWrite;
                    }

                    if (!AllDataDecompressed && input.Length > 0)
                    {
                        DecompressAllTags(input);
                    }
                }
            }

            public void Reset()
            {
                _scratchLength = 0;
                _remainingLiteral = 0;

                _uncompressedLength = 0;
                _uncompressedLengthShift = 0;

                _lookbackPosition = 0;
                _readPosition = 0;
                ExpectedLength = null;
            }

            /// <summary>
            /// Read the uncompressed length stored at the start of the compressed data.
            /// </summary>
            /// <param name="input">Input data, which should begin with the varint encoded uncompressed length.</param>
            /// <returns>The length of the compressed data, or null if the length is not yet complete.</returns>
            /// <remarks>
            /// This variant is used when reading a stream, and will pause if there aren't enough bytes available
            /// in the input. Subsequent calls with more data will resume processing.
            /// </remarks>
            private int? ReadUncompressedLength(ref ReadOnlySpan<byte> input)
            {
                int result = _uncompressedLength;
                int shift = _uncompressedLengthShift;
                bool foundEnd = false;

                var i = 0;
                while (input.Length > i)
                {
                    byte c = input[i];
                    i += 1;

                    int val = c & 0x7f;
                    if (Helpers.LeftShiftOverflows((byte)val, shift))
                    {
                        ThrowHelper.ThrowInvalidOperationException("Invalid stream length");
                    }

                    result |= val << shift;

                    if (c < 128)
                    {
                        foundEnd = true;
                        break;
                    }

                    shift += 7;

                    if (shift >= 32)
                    {
                        ThrowHelper.ThrowInvalidOperationException("Invalid stream length");
                    }
                }

                input = input.Slice(i);
                _uncompressedLength = result;
                _uncompressedLengthShift = shift;

                return foundEnd ? (int?)result : null;
            }

            /// <summary>
            /// Read the uncompressed length stored at the start of the compressed data.
            /// </summary>
            /// <param name="input">Input data, which should begin with the varint encoded uncompressed length.</param>
            /// <returns>The length of the uncompressed data.</returns>
            /// <exception cref="InvalidDataException">Invalid stream length</exception>
            public static int ReadUncompressedLength(ReadOnlySpan<byte> input)
            {
                int result = 0;
                int shift = 0;
                bool foundEnd = false;

                var i = 0;
                while (input.Length > 0)
                {
                    byte c = input[i];
                    i += 1;

                    int val = c & 0x7f;
                    if (Helpers.LeftShiftOverflows((byte)val, shift))
                    {
                        ThrowHelper.ThrowInvalidDataException("Invalid stream length");
                    }

                    result |= val << shift;

                    if (c < 128)
                    {
                        foundEnd = true;
                        break;
                    }

                    shift += 7;

                    if (shift >= 32)
                    {
                        ThrowHelper.ThrowInvalidDataException("Invalid stream length");
                    }
                }

                if (!foundEnd)
                {
                    ThrowHelper.ThrowInvalidDataException("Invalid stream length");
                }

                return result;
            }

            internal void DecompressAllTags(ReadOnlySpan<byte> inputSpan)
            {
                // Put Constants.CharTable on the stack to simplify lookups within the loops below.
                // Slicing with length 256 here allows the JIT compiler to recognize the size is greater than
                // the size of the byte we're indexing with and optimize out range checks.
                ReadOnlySpan<ushort> charTable = Constants.CharTable.AsSpan(0, 256);

                unchecked
                {
                    ref byte input = ref Unsafe.AsRef(in inputSpan[0]);
                    ref byte inputEnd = ref Unsafe.Add(ref input, inputSpan.Length);

                    // Track the point in the input before which input is guaranteed to have at least Constants.MaxTagLength bytes left
                    ref byte inputLimitMinMaxTagLength = ref Unsafe.Subtract(ref inputEnd, Math.Min(inputSpan.Length, Constants.MaximumTagLength - 1));

                    ref byte buffer = ref _lookbackBuffer.Span[0];
                    ref byte bufferEnd = ref Unsafe.Add(ref buffer, _lookbackBuffer.Length);
                    ref byte op = ref Unsafe.Add(ref buffer, _lookbackPosition);

                    // Get a reference to the first byte in the scratch buffer, we'll reuse this so that we don't repeat range checks every time
                    ref byte scratch = ref _scratch[0];

                    if (_scratchLength > 0)
                    {
                        // Have partial tag remaining from a previous decompress run
                        // Get the combined tag in the scratch buffer, then run through
                        // special case processing that gets the tag from the scratch buffer
                        // and any literal data from the _input buffer

                        // This is not a hot path, so it's more efficient to process this as a separate method
                        // so that the stack size of this method is smaller and JIT can produce better results

                        (uint inputUsed, uint bytesWritten) =
                            DecompressTagFromScratch(ref input, ref inputEnd, ref op, ref buffer, ref bufferEnd, ref scratch);
                        if (inputUsed == 0)
                        {
                            // There was insufficient data to read an entire tag. Some data was moved to scratch
                            // but short circuit for another pass when we have more data.
                            return;
                        }

                        input = ref Unsafe.Add(ref input, inputUsed);
                        op = ref Unsafe.Add(ref op, bytesWritten);
                    }

                    if (!Unsafe.IsAddressLessThan(ref input, ref inputLimitMinMaxTagLength))
                    {
                        uint newScratchLength = RefillTag(ref input, ref inputEnd, ref scratch);
                        if (newScratchLength == uint.MaxValue)
                        {
                            goto exit;
                        }

                        if (newScratchLength > 0)
                        {
                            // Data has been moved to the scratch buffer
                            input = ref scratch;
                            inputEnd = ref Unsafe.Add(ref input, newScratchLength);
                            inputLimitMinMaxTagLength = ref Unsafe.Subtract(ref inputEnd,
                                Math.Min(newScratchLength, Constants.MaximumTagLength - 1));
                        }
                    }

                    uint preload = Helpers.UnsafeReadUInt32(ref input);

                    while (true)
                    {
                        byte c = (byte)preload;
                        input = ref Unsafe.Add(ref input, 1);

                        if ((c & 0x03) == Constants.Literal)
                        {
                            nint literalLength = unchecked((c >> 2) + 1);

                            if (TryFastAppend(ref op, ref bufferEnd, in input, Unsafe.ByteOffset(ref input, ref inputEnd), literalLength))
                            {
                                Debug.Assert(literalLength < 61);
                                op = ref Unsafe.Add(ref op, literalLength);
                                input = ref Unsafe.Add(ref input, literalLength);
                                // NOTE: There is no RefillTag here, as TryFastAppend()
                                // will not return true unless there's already at least five spare
                                // bytes in addition to the literal.
                                preload = Helpers.UnsafeReadUInt32(ref input);
                                continue;
                            }

                            if (literalLength >= 61)
                            {
                                // Long literal.
                                nint literalLengthLength = literalLength - 60;
                                uint literalLengthTemp = Helpers.UnsafeReadUInt32(ref input);

                                literalLength = (nint)Helpers.ExtractLowBytes(literalLengthTemp,
                                    (int)literalLengthLength) + 1;

                                input = ref Unsafe.Add(ref input, literalLengthLength);
                            }

                            nint inputRemaining = Unsafe.ByteOffset(ref input, ref inputEnd);
                            if (inputRemaining < literalLength)
                            {
                                Append(ref op, ref bufferEnd, in input, inputRemaining);
                                op = ref Unsafe.Add(ref op, inputRemaining);
                                _remainingLiteral = (int)(literalLength - inputRemaining);
                                goto exit;
                            }
                            else
                            {
                                Append(ref op, ref bufferEnd, in input, literalLength);
                                op = ref Unsafe.Add(ref op, literalLength);
                                input = ref Unsafe.Add(ref input, literalLength);

                                if (!Unsafe.IsAddressLessThan(ref input, ref inputLimitMinMaxTagLength))
                                {
                                    uint newScratchLength = RefillTag(ref input, ref inputEnd, ref scratch);
                                    if (newScratchLength == uint.MaxValue)
                                    {
                                        goto exit;
                                    }

                                    if (newScratchLength > 0)
                                    {
                                        // Data has been moved to the scratch buffer
                                        input = ref scratch;
                                        inputEnd = ref Unsafe.Add(ref input, newScratchLength);
                                        inputLimitMinMaxTagLength = ref Unsafe.Subtract(ref inputEnd,
                                            Math.Min(newScratchLength, Constants.MaximumTagLength - 1));

                                    }
                                }

                                preload = Helpers.UnsafeReadUInt32(ref input);
                            }
                        }
                        else
                        {
                            if ((c & 3) == Constants.Copy4ByteOffset)
                            {
                                uint copyOffset = Helpers.UnsafeReadUInt32(ref input);
                                input = ref Unsafe.Add(ref input, 4);

                                nint length = (c >> 2) + 1;
                                AppendFromSelf(ref op, ref buffer, ref bufferEnd, copyOffset, length);
                                op = ref Unsafe.Add(ref op, length);
                            }
                            else
                            {
                                ushort entry = charTable[c];

                                // We don't use BitConverter to read because we might be reading past the end of the span
                                // But we know that's safe because we'll be doing it in _scratch with extra data on the end.
                                // This reduces this step by several operations
                                preload = Helpers.UnsafeReadUInt32(ref input);

                                uint trailer = Helpers.ExtractLowBytes(preload, c & 3);
                                nint length = entry & 0xff;

                                // copy_offset/256 is encoded in bits 8..10.  By just fetching
                                // those bits, we get copy_offset (since the bit-field starts at
                                // bit 8).
                                uint copyOffset = (entry & 0x700u) + trailer;

                                AppendFromSelf(ref op, ref buffer, ref bufferEnd, copyOffset, length);
                                op = ref Unsafe.Add(ref op, length);

                                input = ref Unsafe.Add(ref input, c & 3);

                                // By using the result of the previous load we reduce the critical
                                // dependency chain of ip to 4 cycles.
                                preload >>= (c & 3) * 8;
                                if (Unsafe.IsAddressLessThan(ref input, ref inputLimitMinMaxTagLength)) continue;
                            }

                            if (!Unsafe.IsAddressLessThan(ref input, ref inputLimitMinMaxTagLength))
                            {
                                uint newScratchLength = RefillTag(ref input, ref inputEnd, ref scratch);
                                if (newScratchLength == uint.MaxValue)
                                {
                                    goto exit;
                                }

                                if (newScratchLength > 0)
                                {
                                    // Data has been moved to the scratch buffer
                                    input = ref scratch;
                                    inputEnd = ref Unsafe.Add(ref input, newScratchLength);
                                    inputLimitMinMaxTagLength = ref Unsafe.Subtract(ref inputEnd,
                                        Math.Min(newScratchLength, Constants.MaximumTagLength - 1));
                                }
                            }

                            preload = Helpers.UnsafeReadUInt32(ref input);
                        }
                    }

                exit:; // All input data is processed
                    _lookbackPosition = (int)Unsafe.ByteOffset(ref buffer, ref op);
                }
            }

            // Returns the amount of the input used, 0 indicates there was insufficient data.
            // Some of the input may have been used if 0 is returned, but it isn't relevant because
            // DecompressAllTags will short circuit.
            private (uint inputUsed, uint bytesWritten) DecompressTagFromScratch(ref byte input, ref byte inputEnd, ref byte op,
                ref byte buffer, ref byte bufferEnd, ref byte scratch)
            {
                // scratch will be the scratch buffer with only the tag if true is returned
                uint inputUsed = RefillTagFromScratch(ref input, ref inputEnd, ref scratch);
                if (inputUsed == 0)
                {
                    return (0, 0);
                }
                input = ref Unsafe.Add(ref input, inputUsed);

                // No more scratch for next cycle, we have a full buffer we're about to use
                _scratchLength = 0;

                byte c = scratch;
                scratch = ref Unsafe.Add(ref scratch, 1);

                if ((c & 0x03) == Constants.Literal)
                {
                    uint literalLength = (uint)((c >> 2) + 1);
                    if (literalLength >= 61)
                    {
                        // Long literal.
                        uint literalLengthLength = literalLength - 60;
                        uint literalLengthTemp = Helpers.UnsafeReadUInt32(ref scratch);

                        literalLength = Helpers.ExtractLowBytes(literalLengthTemp,
                            (int)literalLengthLength) + 1;
                    }

                    nint inputRemaining = Unsafe.ByteOffset(ref input, ref inputEnd);
                    if (inputRemaining < literalLength)
                    {
                        Append(ref op, ref bufferEnd, in input, inputRemaining);
                        _remainingLiteral = (int)(literalLength - inputRemaining);
                        _lookbackPosition += (int)Unsafe.ByteOffset(ref buffer, ref op);

                        // Insufficient data in this case as well, trigger a short circuit
                        return (0, 0);
                    }
                    else
                    {
                        Append(ref op, ref bufferEnd, in input, (nint)literalLength);

                        return (inputUsed + literalLength, literalLength);
                    }
                }
                else if ((c & 3) == Constants.Copy4ByteOffset)
                {
                    uint copyOffset = Helpers.UnsafeReadUInt32(ref scratch);

                    nint length = (c >> 2) + 1;

                    AppendFromSelf(ref op, ref buffer, ref bufferEnd, copyOffset, length);

                    return (inputUsed, (uint)length);
                }
                else
                {
                    ushort entry = Constants.CharTable[c];
                    uint data = Helpers.UnsafeReadUInt32(ref scratch);

                    uint trailer = Helpers.ExtractLowBytes(data, c & 3);
                    nint length = entry & 0xff;

                    // copy_offset/256 is encoded in bits 8..10.  By just fetching
                    // those bits, we get copy_offset (since the bit-field starts at
                    // bit 8).
                    uint copyOffset = (entry & 0x700u) + trailer;

                    AppendFromSelf(ref op, ref buffer, ref bufferEnd, copyOffset, length);

                    return (inputUsed, (uint)length);
                }
            }

            // Returns the amount of the input used, 0 indicates there was insufficient data.
            // Some of the input may have been used if 0 is returned, but it isn't relevant because
            // DecompressAllTags will short circuit.
            private uint RefillTagFromScratch(ref byte input, ref byte inputEnd, ref byte scratch)
            {
                Debug.Assert(_scratchLength > 0);

                if (!Unsafe.IsAddressLessThan(ref input, ref inputEnd))
                {
                    return 0;
                }

                // Read the tag character
                uint entry = Constants.CharTable[scratch];
                uint needed = (entry >> 11) + 1; // +1 byte for 'c'

                uint toCopy = Math.Min((uint)Unsafe.ByteOffset(ref input, ref inputEnd), needed - _scratchLength);
                Unsafe.CopyBlockUnaligned(ref Unsafe.Add(ref scratch, _scratchLength), ref input, toCopy);

                _scratchLength += toCopy;

                if (_scratchLength < needed)
                {
                    // Still insufficient
                    return 0;
                }

                return toCopy;
            }

            // Returns 0 if there is sufficient data available in the input buffer for the next tag AND enough extra padding to
            // safely read preload without overrunning the buffer.
            //
            // Returns uint.MaxValue if there is insufficient data and the decompression should stop until more data is available.
            // In this case any dangling unused bytes will be moved to scratch and _scratchLength for the next iteration.
            //
            // Returns a small number if we have enough data for this tag but not enough to safely load preload without a buffer
            // overrun. In this case, further reads should be from scratch with a length up to the returned number. Scratch will
            // always have some extra bytes on the end so we don't risk buffer overruns.
            private uint RefillTag(ref byte input, ref byte inputEnd, ref byte scratch)
            {
                if (!Unsafe.IsAddressLessThan(ref input, ref inputEnd))
                {
                    return uint.MaxValue;
                }

                // Read the tag character
                uint entry = Constants.CharTable[input];
                uint needed = (entry >> 11) + 1; // +1 byte for 'c'

                uint inputLength = (uint)Unsafe.ByteOffset(ref input, ref inputEnd);
                if (inputLength < needed)
                {
                    // Data is insufficient, copy to scratch
                    Unsafe.CopyBlockUnaligned(ref scratch, ref input, inputLength);

                    _scratchLength = inputLength;
                    return uint.MaxValue;
                }

                if (inputLength < Constants.MaximumTagLength)
                {
                    // Have enough bytes, but copy to scratch so that we do not
                    // read past end of input
                    Unsafe.CopyBlockUnaligned(ref scratch, ref input, inputLength);

                    return inputLength;
                }

                return 0;
            }

            #region Loopback Writer

            private byte[]? _lookbackBufferArray;
            private Memory<byte> _lookbackBuffer;
            private int _lookbackPosition = 0;
            private int _readPosition = 0;

            private int? _expectedLength;
            private int? ExpectedLength
            {
                get => _expectedLength;
                set
                {
                    _expectedLength = value;

                    if (value.HasValue && _lookbackBuffer.Length < value.GetValueOrDefault())
                    {
                        if (_lookbackBufferArray is not null)
                        {
                            ArrayPool<byte>.Shared.Return(_lookbackBufferArray);
                        }

                        _lookbackBufferArray = ArrayPool<byte>.Shared.Rent(value.GetValueOrDefault());
                        _lookbackBuffer = _lookbackBufferArray.AsMemory(0, _lookbackBufferArray.Length);
                    }
                }
            }

            public int UnreadBytes
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => (int)_lookbackPosition - _readPosition;
            }

            public bool EndOfFile
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ExpectedLength.HasValue && _readPosition >= ExpectedLength.GetValueOrDefault();
            }

            public bool AllDataDecompressed
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ExpectedLength.HasValue && _lookbackPosition >= ExpectedLength.GetValueOrDefault();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void Append(ReadOnlySpan<byte> input)
            {
                ref readonly byte inputPtr = ref input[0];

                var lookbackSpan = _lookbackBuffer.Span;
                ref byte op = ref lookbackSpan[_lookbackPosition];

                Append(ref op, ref Unsafe.Add(ref lookbackSpan[0], lookbackSpan.Length), in inputPtr, input.Length);
                _lookbackPosition += input.Length;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void Append(ref byte op, ref byte bufferEnd, in byte input, nint length)
            {
                if (length > Unsafe.ByteOffset(ref op, ref bufferEnd))
                {
                    ThrowHelper.ThrowInvalidDataException("Data too long");
                }

                Unsafe.CopyBlockUnaligned(ref op, ref Unsafe.AsRef(in input), (uint)length);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static bool TryFastAppend(ref byte op, ref byte bufferEnd, in byte input, nint available, nint length)
            {
                if (length <= 16 && available >= 16 + Constants.MaximumTagLength &&
                    Unsafe.ByteOffset(ref op, ref bufferEnd) >= (nint)16)
                {
                    CopyHelpers.UnalignedCopy128(in input, ref op);
                    return true;
                }

                return false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void AppendFromSelf(ref byte op, ref byte buffer, ref byte bufferEnd, uint copyOffset, nint length)
            {
                // ToInt64() ensures that this logic works correctly on x86 (with a slight perf hit on x86, though). This is because
                // nint is only 32-bit on x86, so casting uint copyOffset to an nint for the comparison can result in a negative number with some
                // forms of illegal data. This would then bypass the exception and cause unsafe memory access. Performing the comparison
                // as a long ensures we have enough bits to not lose data. On 64-bit platforms this is effectively a no-op.
                if (copyOffset == 0 || Unsafe.ByteOffset(ref buffer, ref op).ToInt64() < copyOffset)
                {
                    ThrowHelper.ThrowInvalidDataException("Invalid copy offset");
                }

                if (length > Unsafe.ByteOffset(ref op, ref bufferEnd))
                {
                    ThrowHelper.ThrowInvalidDataException("Data too long");
                }

                ref byte source = ref Unsafe.Subtract(ref op, copyOffset);
                CopyHelpers.IncrementalCopy(ref source, ref op,
                    ref Unsafe.Add(ref op, length), ref bufferEnd);
            }

            public int Read(Span<byte> destination)
            {
                var unreadBytes = UnreadBytes;
                if (unreadBytes == 0)
                {
                    return 0;
                }

                if (unreadBytes >= destination.Length)
                {
                    _lookbackBuffer.Span.Slice(_readPosition, destination.Length).CopyTo(destination);
                    _readPosition += destination.Length;
                    return destination.Length;
                }
                else
                {
                    _lookbackBuffer.Span.Slice(_readPosition, unreadBytes).CopyTo(destination);
                    _readPosition += unreadBytes;
                    return unreadBytes;
                }
            }

            /// <summary>
            /// Extracts the data from from the block, returning a block of memory and resetting the block.
            /// </summary>
            /// <returns>An block of memory. Caller is responsible for disposing.</returns>
            /// <remarks>
            /// This provides a more efficient way to decompress an entire block in scenarios where the caller
            /// wants an owned block of memory and isn't going to reuse the SnappyDecompressor. It avoids the
            /// need to copy a block of memory calling <see cref="Read"/>.
            /// </remarks>
            public IMemoryOwner<byte> ExtractData()
            {
                byte[]? data = _lookbackBufferArray;
                if (!ExpectedLength.HasValue)
                {
                    ThrowHelper.ThrowInvalidOperationException("No data present.");
                }
                else if (data is null || ExpectedLength.GetValueOrDefault() == 0)
                {
                    // Length was 0, so we've allocated nothing
                    return new ByteArrayPoolMemoryOwner();
                }

                if (!AllDataDecompressed)
                {
                    ThrowHelper.ThrowInvalidOperationException("Block is not fully decompressed.");
                }

                // Build the return before we reset and clear ExpectedLength
                var returnBuffer = new ByteArrayPoolMemoryOwner(data, ExpectedLength.GetValueOrDefault());

                // Clear the buffer so we don't return it
                _lookbackBufferArray = null;
                _lookbackBuffer = default;

                Reset();

                return returnBuffer;
            }

            #endregion

            #region Test Helpers

            /// <summary>
            /// Load some data into the output buffer, only used for testing.
            /// </summary>
            /// <param name="toWrite"></param>
            internal void WriteToBufferForTest(ReadOnlySpan<byte> toWrite)
            {
                Append(toWrite);
            }

            /// <summary>
            /// Load a byte array into _scratch, only used for testing.
            /// </summary>
            internal void LoadScratchForTest(byte[] newScratch, uint newScratchLength)
            {
                ThrowHelper.ThrowIfNull(newScratch);
                _scratch = newScratch;
                _scratchLength = newScratchLength;
            }

            /// <summary>
            /// Only used for testing.
            /// </summary>
            internal void SetExpectedLengthForTest(int expectedLength)
            {
                ExpectedLength = expectedLength;
            }

            #endregion

            public void Dispose()
            {
                if (_lookbackBufferArray is not null)
                {
                    ArrayPool<byte>.Shared.Return(_lookbackBufferArray);
                    _lookbackBufferArray = null;
                    _lookbackBuffer = default;
                }
            }
        }

        /// <summary>
        /// Emits the stream format used for Snappy streams.
        /// </summary>
        internal class SnappyStreamCompressor : IDisposable
        {
            private static ReadOnlySpan<byte> SnappyHeader => new byte[]
            {
            0xff, 0x06, 0x00, 0x00, 0x73, 0x4e, 0x61, 0x50, 0x70, 0x59
            };

            private SnappyCompressor? _compressor = new SnappyCompressor();

            private byte[]? _inputBuffer;
            private int _inputBufferSize;

            private byte[]? _outputBuffer;
            private int _outputBufferSize;

            private bool _streamHeaderWritten;

            /// <summary>
            /// Processes some input, potentially returning compressed data. Flush must be called when input is complete
            /// to get any remaining compressed data.
            /// </summary>
            /// <param name="input">Uncompressed data to emit.</param>
            /// <param name="stream">Output stream.</param>
            /// <returns>A block of memory with compressed data (if any). Must be used before any subsequent call to Write.</returns>
            public void Write(ReadOnlySpan<byte> input, Stream stream)
            {
                ThrowHelper.ThrowIfNull(stream);
                if (_compressor == null)
                {
                    ThrowHelper.ThrowObjectDisposedException(nameof(SnappyStreamCompressor));
                }

                EnsureBuffer();
                EnsureStreamHeaderWritten();

                while (input.Length > 0)
                {
                    var bytesRead = CompressInput(input);
                    input = input.Slice(bytesRead);

                    WriteOutputBuffer(stream);
                }
            }

            /// <summary>
            /// Processes some input, potentially returning compressed data. Flush must be called when input is complete
            /// to get any remaining compressed data.
            /// </summary>
            /// <param name="input">Uncompressed data to emit.</param>
            /// <param name="stream">Output stream.</param>
            /// <param name="cancellationToken">Cancellation token.</param>
            /// <returns>A block of memory with compressed data (if any). Must be used before any subsequent call to Write.</returns>
            public async ValueTask WriteAsync(ReadOnlyMemory<byte> input, Stream stream, CancellationToken cancellationToken = default)
            {
                ThrowHelper.ThrowIfNull(stream);
                if (_compressor == null)
                {
                    ThrowHelper.ThrowObjectDisposedException(nameof(SnappyStreamCompressor));
                }

                EnsureBuffer();
                EnsureStreamHeaderWritten();

                while (input.Length > 0)
                {
                    var bytesRead = CompressInput(input.Span);
                    input = input.Slice(bytesRead);

                    await WriteOutputBufferAsync(stream, cancellationToken).ConfigureAwait(false);
                }
            }

            public void Flush(Stream stream)
            {
                ThrowHelper.ThrowIfNull(stream);
                if (_compressor == null)
                {
                    ThrowHelper.ThrowObjectDisposedException(nameof(SnappyStreamCompressor));
                }

                EnsureBuffer();
                EnsureStreamHeaderWritten();

                if (_inputBufferSize > 0)
                {
                    CompressBlock(_inputBuffer.AsSpan(0, _inputBufferSize));
                    _inputBufferSize = 0;
                }

                WriteOutputBuffer(stream);
            }

            public async ValueTask FlushAsync(Stream stream, CancellationToken cancellationToken = default)
            {
                ThrowHelper.ThrowIfNull(stream);
                if (_compressor == null)
                {
                    ThrowHelper.ThrowObjectDisposedException(nameof(SnappyStreamCompressor));
                }

                EnsureBuffer();
                EnsureStreamHeaderWritten();

                if (_inputBufferSize > 0)
                {
                    CompressBlock(_inputBuffer.AsSpan(0, _inputBufferSize));
                    _inputBufferSize = 0;
                }

                await WriteOutputBufferAsync(stream, cancellationToken).ConfigureAwait(false);
            }

            private void WriteOutputBuffer(Stream stream)
            {
                if (_outputBufferSize <= 0)
                {
                    return;
                }

                stream.Write(_outputBuffer!, 0, _outputBufferSize);

                _outputBufferSize = 0;
            }

            private async ValueTask WriteOutputBufferAsync(Stream stream, CancellationToken cancellationToken = default)
            {
                if (_outputBufferSize <= 0)
                {
                    return;
                }

                await stream.WriteAsync(_outputBuffer!, 0, _outputBufferSize, cancellationToken).ConfigureAwait(false);

                _outputBufferSize = 0;
            }

            private void EnsureStreamHeaderWritten()
            {
                if (!_streamHeaderWritten)
                {
                    SnappyHeader.CopyTo(_outputBuffer.AsSpan());
                    _outputBufferSize += SnappyHeader.Length;

                    _streamHeaderWritten = true;
                }
            }

            /// <summary>
            /// Processes up to one entire block from the input, potentially combining with previous input blocks.
            /// Fills the compressed data to the output buffer. Will not process more than one output block at a time
            /// to avoid overflowing the output buffer.
            /// </summary>
            /// <param name="input">Input to compress.</param>
            /// <returns>Number of bytes consumed.</returns>
            private int CompressInput(ReadOnlySpan<byte> input)
            {
                Debug.Assert(input.Length > 0);

                if (_inputBufferSize == 0 && input.Length >= Constants.BlockSize)
                {
                    // Optimize to avoid copying

                    input = input.Slice(0, (int)Constants.BlockSize);
                    CompressBlock(input);
                    return input.Length;
                }

                // Append what we can to the input buffer

                var appendLength = Math.Min(input.Length, (int)Constants.BlockSize - _inputBufferSize);
                input.Slice(0, appendLength).CopyTo(_inputBuffer.AsSpan(_inputBufferSize));
                _inputBufferSize += appendLength;

                if (_inputBufferSize >= Constants.BlockSize)
                {
                    CompressBlock(_inputBuffer.AsSpan(0, _inputBufferSize));
                    _inputBufferSize = 0;
                }

                return appendLength;
            }

			#pragma warning disable CS8602
			private void CompressBlock(ReadOnlySpan<byte> input)
            {
                Debug.Assert(_compressor != null);
                Debug.Assert(input.Length <= Constants.BlockSize);

                var output = _outputBuffer.AsSpan(_outputBufferSize);

                // Make room for the header and CRC
                var compressionOutput = output.Slice(8);

                var bytesWritten = _compressor.Compress(input, compressionOutput);

                // Write the header

                WriteCompressedBlockHeader(input, output, bytesWritten);

                _outputBufferSize += bytesWritten + 8;
            }
			#pragma warning restore CS8602

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void WriteCompressedBlockHeader(ReadOnlySpan<byte> input, Span<byte> output, int compressedSize)
            {
                var blockSize = compressedSize + 4; // CRC

                BinaryPrimitives.WriteInt32LittleEndian(output.Slice(1), blockSize);
                output[0] = (byte)Constants.ChunkType.CompressedData;

                var crc = Crc32CAlgorithm.Compute(input);
                crc = Crc32CAlgorithm.ApplyMask(crc);
                BinaryPrimitives.WriteUInt32LittleEndian(output.Slice(4), crc);
            }

            private void EnsureBuffer()
            {
                if (_outputBuffer is null)
                {
                    // Allocate enough room for the stream header and block headers
                    _outputBuffer =
                        ArrayPool<byte>.Shared.Rent(Helpers.MaxCompressedLength((int)Constants.BlockSize) + 8 + SnappyHeader.Length);
                }

                if (_inputBuffer is null)
                {
                    // Allocate enough room for the stream header and block headers
                    _inputBuffer = ArrayPool<byte>.Shared.Rent((int)Constants.BlockSize);
                }
            }

            public void Dispose()
            {
                _compressor?.Dispose();
                _compressor = null;

                if (_outputBuffer is not null)
                {
                    ArrayPool<byte>.Shared.Return(_outputBuffer);
                    _outputBuffer = null;
                }
                if (_inputBuffer is not null)
                {
                    ArrayPool<byte>.Shared.Return(_inputBuffer);
                    _inputBuffer = null;
                }
            }
        }

        /// <summary>
        /// Parses the stream format used for Snappy streams.
        /// </summary>
        internal sealed class SnappyStreamDecompressor : IDisposable
        {
            private const int ScratchBufferSize = 4;

            private SnappyDecompressor? _decompressor = new();

            private ReadOnlyMemory<byte> _input;

            private readonly byte[] _scratch = new byte[ScratchBufferSize];
            private int _scratchLength;
            private Constants.ChunkType? _chunkType;
            private int _chunkSize;
            private int _chunkBytesProcessed;
            private uint _expectedChunkCrc;
            private uint _chunkCrc;

			#pragma warning disable CS8602
            public int Decompress(Span<byte> buffer)
            {
                Debug.Assert(_decompressor != null);

                ReadOnlySpan<byte> input = _input.Span;

                // Cache this to use later to calculate the total bytes written
                int originalBufferLength = buffer.Length;

                while (buffer.Length > 0
                       && (input.Length > 0 || (_chunkType == Constants.ChunkType.CompressedData && _decompressor.AllDataDecompressed)))
                {
                    // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                    switch (_chunkType)
                    {
                        case null:
                            // Not in a chunk, read the chunk type and size

                            uint rawChunkHeader = ReadChunkHeader(ref input);

                            if (rawChunkHeader == 0)
                            {
                                // Not enough data, get some more
                                goto exit;
                            }

                            _chunkType = (Constants.ChunkType)(rawChunkHeader & 0xff);
                            _chunkSize = unchecked((int)(rawChunkHeader >> 8));
                            _chunkBytesProcessed = 0;
                            _scratchLength = 0;
                            _chunkCrc = 0;
                            break;

                        case Constants.ChunkType.CompressedData:
                            {
                                if (_chunkBytesProcessed < 4)
                                {
                                    _decompressor.Reset();

                                    if (!ReadChunkCrc(ref input))
                                    {
                                        // Incomplete CRC
                                        goto exit;
                                    }

                                    if (input.Length == 0)
                                    {
                                        // No more data
                                        goto exit;
                                    }
                                }

                                while (buffer.Length > 0 && !_decompressor.EndOfFile)
                                {
                                    if (_decompressor.NeedMoreData)
                                    {
                                        if (input.Length == 0)
                                        {
                                            // No more data to give
                                            goto exit;
                                        }

                                        int availableChunkBytes = Math.Min(input.Length, _chunkSize - _chunkBytesProcessed);
                                        Debug.Assert(availableChunkBytes > 0);

                                        _decompressor.Decompress(input.Slice(0, availableChunkBytes));

                                        _chunkBytesProcessed += availableChunkBytes;
                                        input = input.Slice(availableChunkBytes);
                                    }

                                    int decompressedBytes = _decompressor.Read(buffer);

                                    _chunkCrc = Crc32CAlgorithm.Append(_chunkCrc, buffer.Slice(0, decompressedBytes));

                                    buffer = buffer.Slice(decompressedBytes);
                                }

                                if (_decompressor.EndOfFile)
                                {
                                    // Completed reading the chunk
                                    _chunkType = null;

                                    uint crc = Crc32CAlgorithm.ApplyMask(_chunkCrc);
                                    if (_expectedChunkCrc != crc)
                                    {
                                        ThrowHelper.ThrowInvalidDataException("Chunk CRC mismatch.");
                                    }
                                }

                                break;
                            }

                        case Constants.ChunkType.UncompressedData:
                            {
                                if (_chunkBytesProcessed < 4)
                                {
                                    if (!ReadChunkCrc(ref input))
                                    {
                                        // Incomplete CRC
                                        goto exit;
                                    }

                                    if (input.Length == 0)
                                    {
                                        // No more data
                                        goto exit;
                                    }
                                }

                                int chunkBytes = unchecked(Math.Min(Math.Min(buffer.Length, input.Length),
                                    _chunkSize - _chunkBytesProcessed));

                                input.Slice(0, chunkBytes).CopyTo(buffer);

                                _chunkCrc = Crc32CAlgorithm.Append(_chunkCrc, buffer.Slice(0, chunkBytes));

                                buffer = buffer.Slice(chunkBytes);
                                input = input.Slice(chunkBytes);
                                _chunkBytesProcessed += chunkBytes;

                                if (_chunkBytesProcessed >= _chunkSize)
                                {
                                    // Completed reading the chunk
                                    _chunkType = null;

                                    uint crc = Crc32CAlgorithm.ApplyMask(_chunkCrc);
                                    if (_expectedChunkCrc != crc)
                                    {
                                        ThrowHelper.ThrowInvalidDataException("Chunk CRC mismatch.");
                                    }
                                }

                                break;
                            }

                        default:
                            {
                                if (_chunkType < Constants.ChunkType.SkippableChunk)
                                {
                                    ThrowHelper.ThrowInvalidDataException($"Unknown chunk type {(int)_chunkType:x}");
                                }

                                int chunkBytes = Math.Min(input.Length, _chunkSize - _chunkBytesProcessed);

                                input = input.Slice(chunkBytes);
                                _chunkBytesProcessed += chunkBytes;

                                if (_chunkBytesProcessed >= _chunkSize)
                                {
                                    // Completed reading the chunk
                                    _chunkType = null;
                                }

                                break;
                            }
                    }
                }

            // We use a label and goto exit to avoid an unnecessary comparison on the while loop clause before
            // exiting the loop in cases where we know we're done processing data.
            exit:
                _input = _input.Slice(_input.Length - input.Length);
                return originalBufferLength - buffer.Length;
            }
			#pragma warning restore CS8602
            public void SetInput(ReadOnlyMemory<byte> input)
            {
                _input = input;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private uint ReadChunkHeader(ref ReadOnlySpan<byte> buffer)
            {
                if (_scratchLength > 0)
                {
                    var bytesToCopyToScratch = 4 - _scratchLength;

                    Span<byte> scratch = _scratch.AsSpan();
                    buffer.Slice(0, bytesToCopyToScratch).CopyTo(scratch.Slice(_scratchLength));

                    buffer = buffer.Slice(bytesToCopyToScratch);
                    _scratchLength += bytesToCopyToScratch;

                    if (_scratchLength < 4)
                    {
                        // Insufficient data
                        return 0;
                    }

                    _scratchLength = 0;
                    return BinaryPrimitives.ReadUInt32LittleEndian(scratch);
                }

                if (buffer.Length < 4)
                {
                    // Insufficient data

                    buffer.CopyTo(_scratch);

                    _scratchLength = buffer.Length;
                    buffer = Span<byte>.Empty;

                    return 0;
                }
                else
                {
                    uint result = BinaryPrimitives.ReadUInt32LittleEndian(buffer);
                    buffer = buffer.Slice(4);
                    return result;
                }
            }

            /// <summary>
            /// Assuming that we're at the beginning of a chunk, reads the CRC. If partially read, stores the value in
            /// _scratch for subsequent reads. Should not be called if chunkByteProcessed >= 4.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private bool ReadChunkCrc(ref ReadOnlySpan<byte> input)
            {
                Debug.Assert(_chunkBytesProcessed < 4);

                if (_chunkBytesProcessed == 0 && input.Length >= 4)
                {
                    // Common fast path

                    _expectedChunkCrc = BinaryPrimitives.ReadUInt32LittleEndian(input);
                    input = input.Slice(4);
                    _chunkBytesProcessed += 4;
                    return true;
                }

                // Copy to scratch
                int crcBytesAvailable = Math.Min(input.Length, 4 - _chunkBytesProcessed);
                input.Slice(0, crcBytesAvailable).CopyTo(_scratch.AsSpan(_scratchLength));
                _scratchLength += crcBytesAvailable;
                input = input.Slice(crcBytesAvailable);
                _chunkBytesProcessed += crcBytesAvailable;

                if (_scratchLength >= 4)
                {
                    _expectedChunkCrc = BinaryPrimitives.ReadUInt32LittleEndian(_scratch);
                    _scratchLength = 0;
                    return true;
                }

                return false;
            }

            public void Dispose()
            {
                _decompressor?.Dispose();
                _decompressor = null;
            }
        }

        internal static class ThrowHelper
        {
            [DoesNotReturn]
            public static void ThrowArgumentException(string? message, string? paramName) =>
                throw new ArgumentException(message, paramName);

            [DoesNotReturn]
            public static void ThrowArgumentOutOfRangeException(string? paramName, string? message) =>
                throw new ArgumentOutOfRangeException(paramName, message);

			#if NET6_0_OR_GREATER
				public static void ThrowIfNull([NotNull] object? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null) =>
					ArgumentNullException.ThrowIfNull(argument, paramName);
			#else
            [DoesNotReturn]
				private static void ThrowArgumentNullException(string? paramName) =>
					throw new ArgumentNullException(paramName);

				public static void ThrowIfNull([NotNull] object? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
				{
					if (argument is null)
					{
						ThrowArgumentNullException(paramName);
					}
				}
			#endif

            [DoesNotReturn]
            public static void ThrowInvalidDataException(string? message) =>
                throw new InvalidDataException(message);

            [DoesNotReturn]
            public static void ThrowInvalidOperationException(string? message) =>
                throw new InvalidOperationException(message);

            [DoesNotReturn]
            public static void ThrowNotSupportedException() =>
                throw new NotSupportedException();

            [DoesNotReturn]
            public static void ThrowObjectDisposedException(string? objectName) =>
                throw new ObjectDisposedException(objectName);
        }
    
	}
	#nullable disable

}
