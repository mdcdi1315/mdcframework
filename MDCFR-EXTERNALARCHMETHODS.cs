// This CS file contians external archiving methods.
// First one: (Version 1.5.4.6) Tar Archives.

// Global namespaces
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;


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
	
}
