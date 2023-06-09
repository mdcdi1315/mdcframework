// This CS file contains external archiving methods.
// First one: (Version 1.5.4.6) Tar Archives.
// Second one: (Version 1.5.5.0) Cabinet Archives.
// Third one: (Version 1.5.5.0) Snappy Archives.

// Global namespaces
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Security;
using System.Threading;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

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
		public enum EntryType : byte
		{
			File = 0,
			FileObsolete = 0x30,
			HardLink = 0x31,
			SymLink = 0x32,
			CharDevice = 0x33,
			BlockDevice = 0x34,
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
		/// Extract contents of a tar file represented by a stream for the TarReader constructor
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
			/// 
			/// CAUTION! This method is not safe. It's not tar-bomb proof. 
			/// {see http://en.wikipedia.org/wiki/Tar_(file_format) }
			/// If you are not sure about the source of an archive you extracting,
			/// then use MoveNext and Read and handle paths like ".." and "../.." according
			/// to your business logic.
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
			/// 
			/// Example:
			/// while(MoveNext())
			/// { 
			///     Read(dataDestStream); 
			/// }
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
		public class TarWriter : LegacyTarWriter
		{

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
							throw new TarException("UsTar fileName can not be longer thatn 255 chars");
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

			[SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
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
			[SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
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

			[SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
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

        public static class Api
        {
            /// <summary>
            /// 	Constant max hash table bits.
            /// </summary>
            public const int MaxHashTableBits = 14;
            /// <summary>
            /// 	Constant max hash table size.
            /// </summary>
            public const int MaxHashTableSize = 1 << MaxHashTableBits;

            public enum Status
            {
                OK = 0,
                InvalidInput = 1,
                BufferTooSmall = 2,
            }

            public static int Compress(Source source, Sink sink)
            {
                return 0;
            }

            public static bool GetUncompressedLength(Source source, ref int result)
            {
                SnappyDecompressor decompressor = new SnappyDecompressor(source);
                return decompressor.ReadUncompressedLength(ref result);
            }

            public static int Compress(byte[] input, int inputLength, ref string output)
            {
                return 0;
            }

            public static bool Uncompress(byte[] compressed, int compressedLength, ref string uncompressed)
            {
                return true;
            }
        }

        /// <summary>
        /// 	Byte array source.
        /// </summary>
        public class ByteArraySource : Source
        {
            /// <summary>
            /// 	The left.
            /// </summary>
            private int left = 0;
            /// <summary>
            /// 	The pointer.
            /// </summary>
            private MemoryStream pointer = null;

            /// <summary>
            /// 	Initializes a new instance of the <see cref="SnappySharp.ByteArraySource"/> class.
            /// </summary>
            /// <param name='pointer'>
            /// 	Pointer.
            /// </param>
            /// <param name='n'>
            /// 	N.
            /// </param>
            public ByteArraySource(MemoryStream pointer, int n)
            {
                this.left = n;
                this.pointer = pointer;
            }

            /// <summary>
            /// 	Available this instance.
            /// </summary>
            public override int Available()
            {
                return this.left;
            }

            /// <summary>
            /// 	Peek the specified length.
            /// </summary>
            /// <param name='length'>
            /// 	Length.
            /// </param>
            public override MemoryStream Peek(ref int length)
            {
                length = this.left;
                return this.pointer;
            }

            /// <summary>
            /// 	Skip the specified n.
            /// </summary>
            /// <param name='n'>
            /// 	N.
            /// </param>
            public override void Skip(int n)
            {
                this.left -= n;
                this.pointer.Seek(n, SeekOrigin.Current);
            }
        }

        public class ByteBuffer
        {
            public ByteBuffer()
            {
            }
        }

        /// <summary>
        /// 	Sink.
        /// </summary>
        public abstract class Sink
        {
            /// <summary>
            /// 	Gets the append buffer.
            /// </summary>
            /// <returns>
            /// 	The append buffer.
            /// </returns>
            /// <param name='length'>
            /// 	Length.
            /// </param>
            /// <param name='scratch'>
            /// 	Scratch.
            /// </param>
            public virtual MemoryStream GetAppendBuffer(int length, MemoryStream scratch)
            {
                return scratch;
            }

            /// <summary>
            /// 	Append the specified bytes and n.
            /// </summary>
            /// <param name='bytes'>
            /// 	Bytes.
            /// </param>
            /// <param name='n'>
            /// 	N.
            /// </param>
            public abstract void Append(MemoryStream bytes, int n);
        }

        /// <summary>
        /// 	Snappy decompressor.
        /// </summary>
        internal class SnappyDecompressor
        {
            /// <summary>
            /// 	The reader.
            /// </summary>
            private Source reader = null;
            /// <summary>
            /// 	The EOF marker.
            /// </summary>
            private bool eof = false;
            /// <summary>
            /// 	The internal pointer.
            /// </summary>
            private long internalPointer = 0;
            /// <summary>
            /// 	The internal pointer limit.
            /// </summary>
            private long internalPointerLimit = 0;
            /// <summary>
            /// 	The peeked.
            /// </summary>
            private int peeked = 0;

            /// <summary>
            /// 	Initializes a new instance of the <see cref="SnappySharp.SnappyDecompressor"/> class.
            /// </summary>
            /// <param name='reader'>
            /// 	Reader.
            /// </param>
            public SnappyDecompressor(Source reader)
            {
                this.reader = reader;
            }

            /// <summary>
            /// 	Sets a value indicating whether this <see cref="SnappySharp.SnappyDecompressor"/> is EOF.
            /// </summary>
            /// <value>
            /// 	<c>true</c> if EOF; otherwise, <c>false</c>.
            /// </value>
            public bool Eof { get { return this.eof; } }

            /// <summary>
            /// 	Reads the length of the uncompressed.
            /// </summary>
            /// <returns>
            /// 	The uncompressed length.
            /// </returns>
            /// <param name='result'>
            /// 	If set to <c>true</c> result.
            /// </param>
            public bool ReadUncompressedLength(ref int result)
            {
                result = 0;
                uint shift = 0;

                while (true)
                {
                    if (shift >= 32)
                        return false;
                    int n = 0;
                    MemoryStream ip = reader.Peek(ref n);

                    if (n == 0)
                        return false;

                    uint c = (uint)ip.ReadByte();
                    reader.Skip(1);
                    result |= (int)(c & 0x7Fu) << (int)shift;
                    if (c < 128)
                        break;
                    shift += 7;
                }
                return true;
            }

            /// <summary>
            /// 	Step the specified writer.
            /// </summary>
            /// <param name='writer'>
            /// 	If set to <c>true</c> writer.
            /// </param>
            /// <typeparam name='Writer'>
            /// 	The 1st type parameter.
            /// </typeparam>
            public bool Step(Writer.IWriter writer)
            {
                return true;
            }

            /// <summary>
            /// 	Refills the tag.
            /// </summary>
            /// <returns>
            /// 	The tag.
            /// </returns>
            public bool RefillTag()
            {
                long ip = this.internalPointer;

                if (this.internalPointer == this.internalPointerLimit)
                {
                    int n = 0;
                    this.reader.Skip(this.peeked);
                    ip = this.reader.Peek(ref n).Position;
                    this.peeked = n;

                    if (n == 0)
                    {
                        this.eof = true;
                        return false;
                    }
                    this.internalPointerLimit = ip + n;
                }

                return true;
            }
        }

        /// <summary>
        /// 	Source.
        /// </summary>
        public abstract class Source
        {
            /// <summary>
            /// 	Initializes a new instance of the <see cref="SnappySharp.Source"/> class.
            /// </summary>
            public Source()
            {
            }

            /// <summary>
            /// 	Available this instance.
            /// </summary>
            public abstract int Available();
            /// <summary>
            /// 	Peek the specified length.
            /// </summary>
            /// <param name='length'>
            /// 	Length.
            /// </param>
            public abstract MemoryStream Peek(ref int length);
            /// <summary>
            /// 	Skip the specified n.
            /// </summary>
            /// <param name='n'>
            /// 	N.
            /// </param>
            public abstract void Skip(int n);
        }

        /// <summary>
        /// 	Unchecked byte array sink.
        /// </summary>
        public class UncheckedByteArraySink : Sink
        {
            /// <summary>
            /// 	The destination.
            /// </summary>
            MemoryStream destination = null;

            /// <summary>
            /// 	Initializes a new instance of the <see cref="SnappySharp.UncheckedByteArraySink"/> class.
            /// </summary>
            /// <param name='dest'>
            /// 	Destination.
            /// </param>
            public UncheckedByteArraySink(MemoryStream destination)
            {
                this.destination = destination;
            }

            /// <summary>
            /// 	Gets the current destination.
            /// </summary>
            /// <value>
            /// 	The current destination.
            /// </value>
            public MemoryStream CurrentDestination { get { return this.destination; } }

            /// <summary>
            /// 	Append the specified bytes and n.
            /// </summary>
            /// <param name='bytes'>
            /// 	Bytes.
            /// </param>
            /// <param name='n'>
            /// 	N.
            /// </param>
            public override void Append(MemoryStream bytes, int n)
            {
                if (bytes.Position != this.destination.Position)
                {
                    byte[] buffer = new byte[n];
                    bytes.Read(buffer, 0, n);
                    this.destination.Write(buffer, 0, n);
                }
                this.destination.Seek(n, SeekOrigin.Current);
            }

            /// <summary>
            /// 	Gets the append buffer.
            /// </summary>
            /// <returns>
            /// 	The append buffer.
            /// </returns>
            /// <param name='length'>
            /// 	Length.
            /// </param>
            /// <param name='scratch'>
            /// 	Scratch.
            /// </param>
            public override MemoryStream GetAppendBuffer(int length, MemoryStream scratch)
            {
                return this.destination;
            }
        }

        /// <summary>
        /// 	Working memory.
        /// </summary>
        public class WorkingMemory
        {
            /// <summary>
            /// 	The short table.
            /// </summary>
            ushort[] shortTable = new ushort[1 << 10];
            /// <summary>
            /// 	The large table.
            /// </summary>
            ushort[] largeTable = null;

            /// <summary>
            /// 	Gets the hash table.
            /// </summary>
            /// <returns>
            /// 	The hash table.
            /// </returns>
            /// <param name='inputSize'>
            /// 	Input size.
            /// </param>
            /// <param name='tableSize'>
            /// 	Table size.
            /// </param>
            public ushort[] GetHashTable(int inputSize, ref int tableSize)
            {
                int htSize = 256;
                while (htSize < inputSize)
                    htSize <<= 1;
                ushort[] table = null;
                if (htSize < Api.MaxHashTableSize && htSize <= (1 << 10))
                    table = this.shortTable;
                else
                {
                    if (this.largeTable == null)
                        this.largeTable = new ushort[Api.MaxHashTableSize];
                    table = this.largeTable;
                }
                tableSize = htSize;
                return table;
            }
        }

        namespace Util
        {
            /// <summary>
            /// 	Some bit-manipulation functions.
            /// </summary>
            public static class Bits
            {
                /// <summary>
                /// 	Lookuptable for fast computation of NumberOfTrailingZeros
                /// </summary>
                private static readonly int[] lookup = new int[]
                {
                32, 0, 1, 26, 2, 23, 27, 0, 3, 16, 24, 30, 28, 11, 0, 13, 4, 7, 17,
                0, 25, 22, 31, 15, 29, 10, 12, 6, 0, 21, 14, 9, 5, 20, 8, 19, 18
                };

                /// <summary>
                /// 	The word mask.
                /// </summary>
                internal static readonly uint[] wordMask = new uint[]
                {
                0u, 0xFFu, 0xFFFFu, 0xFFFFFFu, 0xFFFFFFFFu
                };

                /// <summary>
                /// 	Return floor(log2(n)) for positive integer n.  Returns -1 iff n == 0.
                /// </summary>
                /// <returns>
                /// 	floor(log2(n))
                /// </returns>
                /// <param name='n'>
                /// 	n
                /// </param>
                public static int Log2Floor(int n)
                {
                    return n == 0 ? -1 : 31 ^ NumberOfLeadingZeros(n);
                }

                /// <summary>
                /// Finds the LSB set non zero.
                /// </summary>
                /// <returns>
                /// The LSB set non zero.
                /// </returns>
                /// <param name='n'>
                /// N.
                /// </param>
                public static int FindLSBSetNonZero(int n)
                {
                    return NumberOfTrailingZeros(n);
                }

                /// <summary>
                /// Numbers the of trailing zeros.
                /// </summary>
                /// <returns>
                /// The of trailing zeros.
                /// </returns>
                /// <param name='i'>
                /// I.
                /// </param>
                private static int NumberOfTrailingZeros(int i)
                {
                    return lookup[(i & -i) % 37];
                }

                /// <summary>
                /// Numbers the of leading zeros.
                /// </summary>
                /// <returns>
                /// The of leading zeros.
                /// </returns>
                /// <param name='i'>
                /// I.
                /// </param>
                private static int NumberOfLeadingZeros(int i)
                {
                    // 32-bit word to reverse bit order
                    int v = i;
                    // swap odd and even bits
                    v = ((v >> 1) & 0x55555555) | ((v & 0x55555555) << 1);
                    // swap consecutive pairs
                    v = ((v >> 2) & 0x33333333) | ((v & 0x33333333) << 2);
                    // swap nibbles ... 
                    v = ((v >> 4) & 0x0F0F0F0F) | ((v & 0x0F0F0F0F) << 4);
                    // swap bytes
                    v = ((v >> 8) & 0x00FF00FF) | ((v & 0x00FF00FF) << 8);
                    // swap 2-byte long pairs
                    v = (v >> 16) | (v << 16);
                    return NumberOfTrailingZeros(v);
                }
            }
        }

        namespace Writer
        {
            /// <summary>
            /// 	Writer interface
            /// </summary>
            public interface IWriter
            {
                /// <summary>
                /// 	Sets the expected length.
                /// </summary>
                /// <param name='length'>
                /// 	Length.
                /// </param>
                void SetExpectedLength(int length);
                /// <summary>
                /// 	Checks the length.
                /// </summary>
                /// <returns>
                /// 	The length.
                /// </returns>
                bool CheckLength();
                /// <summary>
                /// 	Append the specified pointer, length and allowFastpath.
                /// </summary>
                /// <param name='pointer'>
                /// 	If set to <c>true</c> pointer.
                /// </param>
                /// <param name='length'>
                /// 	If set to <c>true</c> length.
                /// </param>
                /// <param name='allowFastpath'>
                /// 	If set to <c>true</c> allow fastpath.
                /// </param>
                bool Append(MemoryStream pointer, int length, bool allowFastpath);
                /// <summary>
                /// 	Appends from self.
                /// </summary>
                /// <returns>
                /// 	The from self.
                /// </returns>
                /// <param name='offset'>
                /// 	If set to <c>true</c> offset.
                /// </param>
                /// <param name='length'>
                /// 	If set to <c>true</c> length.
                /// </param>
                bool AppendFromSelf(int offset, int length);
            }

            /// <summary>
            /// 	Snappy array writer.
            /// </summary>
            public class SnappyArrayWriter : IWriter
            {
                /// <summary>
                /// 	The destination.
                /// </summary>
                private byte[] destination = null;
                /// <summary>
                /// 	The op.
                /// </summary>
                private int op = 0;
                /// <summary>
                /// 	The limit.
                /// </summary>
                private int limit = 0;

                /// <summary>
                /// 	Initializes a new instance of the <see cref="SnappySharp.Writer.SnappyArrayWriter"/> class.
                /// </summary>
                /// <param name='destination'>
                /// 	Destination array.
                /// </param>
                public SnappyArrayWriter(byte[] destination)
                {
                    this.destination = destination;
                }

                #region IWriter implementation
                public bool AppendFromSelf(int offset, int length)
                {
                    int spaceLeft = this.limit - this.op;
                    if (this.op <= offset - 1u)
                        return false;
                    if (spaceLeft < length)
                        return false;
                    for (int i = 0; i < length; ++i)
                        this.destination[this.op - offset + i] = this.destination[this.op + i];
                    this.op += length;
                    return true;
                }

                public bool Append(MemoryStream pointer, int length, bool allowFastpath)
                {
                    int spaceLeft = this.limit - this.op;
                    if (spaceLeft < length)
                        return false;
                    pointer.Read(this.destination, this.op, length);
                    this.op += length;
                    return true;
                }

                public bool CheckLength()
                {
                    return this.limit == this.op;
                }

                public void SetExpectedLength(int length)
                {
                    this.limit = this.op + length;
                }
                #endregion
            }

            /// <summary>
            /// 	Snappy decompression validator.
            /// </summary>
            public class SnappyDecompressionValidator : IWriter
            {
                /// <summary>
                /// 
                /// </summary>
                private int expected = 0;
                /// <summary>
                /// 	
                /// </summary>
                private int produced = 0;

                #region IWriter implementation
                public bool AppendFromSelf(int offset, int length)
                {
                    if (this.produced <= offset - length)
                        return false;
                    this.produced += length;
                    return this.produced <= this.expected;
                }

                public bool Append(MemoryStream pointer, int length, bool allowFastpath)
                {
                    this.produced += length;
                    return this.produced <= this.expected;
                }

                public bool CheckLength()
                {
                    return this.expected == this.produced;
                }

                public void SetExpectedLength(int length)
                {
                    this.expected = length;
                }
                #endregion
            }
        }
    
	}

}
