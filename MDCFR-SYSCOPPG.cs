// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.IO;
using System.Threading;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;


namespace System.Text
{
    
    internal abstract class BaseCodePageEncoding : System.Text.EncodingNLS, ISerializable
    {
        [StructLayout(LayoutKind.Explicit)]
        internal struct CodePageDataFileHeader
        {
            [FieldOffset(0)]
            internal char TableName;

            [FieldOffset(32)]
            internal ushort Version;

            [FieldOffset(40)]
            internal short CodePageCount;

            [FieldOffset(42)]
            internal short unused1;
        }

        [StructLayout(LayoutKind.Explicit, Pack = 2)]
        internal struct CodePageIndex
        {
            [FieldOffset(0)]
            internal char CodePageName;

            [FieldOffset(32)]
            internal short CodePage;

            [FieldOffset(34)]
            internal short ByteCount;

            [FieldOffset(36)]
            internal int Offset;
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct CodePageHeader
        {
            [FieldOffset(0)]
            internal char CodePageName;

            [FieldOffset(32)]
            internal ushort VersionMajor;

            [FieldOffset(34)]
            internal ushort VersionMinor;

            [FieldOffset(36)]
            internal ushort VersionRevision;

            [FieldOffset(38)]
            internal ushort VersionBuild;

            [FieldOffset(40)]
            internal short CodePage;

            [FieldOffset(42)]
            internal short ByteCount;

            [FieldOffset(44)]
            internal char UnicodeReplace;

            [FieldOffset(46)]
            internal ushort ByteReplace;
        }

        internal const string CODE_PAGE_DATA_FILE_NAME = "codepages.nlp";

        protected int dataTableCodePage;

        protected int iExtraBytes;

        protected char[] arrayUnicodeBestFit;

        protected char[] arrayBytesBestFit;

        private const int CODEPAGE_DATA_FILE_HEADER_SIZE = 44;

        private const int CODEPAGE_HEADER_SIZE = 48;

        private static readonly byte[] s_codePagesDataHeader = new byte[44];

        protected static Stream s_codePagesEncodingDataStream = GetEncodingDataStream("codepages.nlp");

        protected static readonly object s_streamLock = new object();

        protected byte[] m_codePageHeader = new byte[48];

        protected int m_firstDataWordOffset;

        protected int m_dataSize;

        protected SafeAllocHHandle safeNativeMemoryHandle;

        internal BaseCodePageEncoding(int codepage)
            : this(codepage, codepage)
        {
        }

        internal BaseCodePageEncoding(int codepage, int dataCodePage)
            : base(codepage, new System.Text.InternalEncoderBestFitFallback(null), new System.Text.InternalDecoderBestFitFallback(null))
        {
            ((System.Text.InternalEncoderBestFitFallback)base.EncoderFallback).encoding = this;
            ((System.Text.InternalDecoderBestFitFallback)base.DecoderFallback).encoding = this;
            dataTableCodePage = dataCodePage;
            LoadCodePageTables();
        }

        internal BaseCodePageEncoding(int codepage, int dataCodePage, EncoderFallback enc, DecoderFallback dec)
            : base(codepage, enc, dec)
        {
            dataTableCodePage = dataCodePage;
            LoadCodePageTables();
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new PlatformNotSupportedException();
        }

        private unsafe static void ReadCodePageDataFileHeader(Stream stream, byte[] codePageDataFileHeader)
        {
            int num = stream.Read(codePageDataFileHeader, 0, codePageDataFileHeader.Length);
            if (BitConverter.IsLittleEndian)
            {
                return;
            }
            fixed (byte* ptr = &codePageDataFileHeader[0])
            {
                CodePageDataFileHeader* ptr2 = (CodePageDataFileHeader*)ptr;
                char* ptr3 = &ptr2->TableName;
                for (int i = 0; i < 16; i++)
                {
                    ptr3[i] = (char)BinaryPrimitives.ReverseEndianness(ptr3[i]);
                }
                ushort* ptr4 = &ptr2->Version;
                for (int j = 0; j < 4; j++)
                {
                    ptr4[j] = BinaryPrimitives.ReverseEndianness(ptr4[j]);
                }
                ptr2->CodePageCount = BinaryPrimitives.ReverseEndianness(ptr2->CodePageCount);
            }
        }

        private unsafe static void ReadCodePageIndex(Stream stream, byte[] codePageIndex)
        {
            int num = stream.Read(codePageIndex, 0, codePageIndex.Length);
            if (BitConverter.IsLittleEndian)
            {
                return;
            }
            fixed (byte* ptr = &codePageIndex[0])
            {
                CodePageIndex* ptr2 = (CodePageIndex*)ptr;
                char* ptr3 = &ptr2->CodePageName;
                for (int i = 0; i < 16; i++)
                {
                    ptr3[i] = (char)BinaryPrimitives.ReverseEndianness(ptr3[i]);
                }
                ptr2->CodePage = BinaryPrimitives.ReverseEndianness(ptr2->CodePage);
                ptr2->ByteCount = BinaryPrimitives.ReverseEndianness(ptr2->ByteCount);
                ptr2->Offset = BinaryPrimitives.ReverseEndianness(ptr2->Offset);
            }
        }

        private unsafe static void ReadCodePageHeader(Stream stream, byte[] codePageHeader)
        {
            int num = stream.Read(codePageHeader, 0, codePageHeader.Length);
            if (BitConverter.IsLittleEndian)
            {
                return;
            }
            fixed (byte* ptr = &codePageHeader[0])
            {
                CodePageHeader* ptr2 = (CodePageHeader*)ptr;
                char* ptr3 = &ptr2->CodePageName;
                for (int i = 0; i < 16; i++)
                {
                    ptr3[i] = (char)BinaryPrimitives.ReverseEndianness(ptr3[i]);
                }
                ptr2->VersionMajor = BinaryPrimitives.ReverseEndianness(ptr2->VersionMajor);
                ptr2->VersionMinor = BinaryPrimitives.ReverseEndianness(ptr2->VersionMinor);
                ptr2->VersionRevision = BinaryPrimitives.ReverseEndianness(ptr2->VersionRevision);
                ptr2->VersionBuild = BinaryPrimitives.ReverseEndianness(ptr2->VersionBuild);
                ptr2->CodePage = BinaryPrimitives.ReverseEndianness(ptr2->CodePage);
                ptr2->ByteCount = BinaryPrimitives.ReverseEndianness(ptr2->ByteCount);
                ptr2->UnicodeReplace = (char)BinaryPrimitives.ReverseEndianness(ptr2->UnicodeReplace);
                ptr2->ByteReplace = BinaryPrimitives.ReverseEndianness(ptr2->ByteReplace);
            }
        }

        internal static Stream GetEncodingDataStream(string tableName)
        {
            Stream manifestResourceStream = typeof(CodePagesEncodingProvider).Assembly.GetManifestResourceStream(tableName);
            if (manifestResourceStream == null)
            {
                throw new InvalidOperationException();
            }
            ReadCodePageDataFileHeader(manifestResourceStream, s_codePagesDataHeader);
            return manifestResourceStream;
        }

        private void LoadCodePageTables()
        {
            if (!FindCodePage(dataTableCodePage))
            {
                throw new NotSupportedException(System.SR.Format(MDCFR.Properties.Resources.NotSupported_NoCodepageData, CodePage));
            }
            LoadManagedCodePage();
        }

        private unsafe bool FindCodePage(int codePage)
        {
            byte[] array = new byte[sizeof(CodePageIndex)];
            lock (s_streamLock)
            {
                s_codePagesEncodingDataStream.Seek(44L, SeekOrigin.Begin);
                int codePageCount;
                fixed (byte* ptr = &s_codePagesDataHeader[0])
                {
                    CodePageDataFileHeader* ptr2 = (CodePageDataFileHeader*)ptr;
                    codePageCount = ptr2->CodePageCount;
                }
                fixed (byte* ptr3 = &array[0])
                {
                    CodePageIndex* ptr4 = (CodePageIndex*)ptr3;
                    for (int i = 0; i < codePageCount; i++)
                    {
                        ReadCodePageIndex(s_codePagesEncodingDataStream, array);
                        if (ptr4->CodePage == codePage)
                        {
                            long position = s_codePagesEncodingDataStream.Position;
                            s_codePagesEncodingDataStream.Seek(ptr4->Offset, SeekOrigin.Begin);
                            ReadCodePageHeader(s_codePagesEncodingDataStream, m_codePageHeader);
                            m_firstDataWordOffset = (int)s_codePagesEncodingDataStream.Position;
                            if (i == codePageCount - 1)
                            {
                                m_dataSize = (int)(s_codePagesEncodingDataStream.Length - ptr4->Offset - m_codePageHeader.Length);
                            }
                            else
                            {
                                s_codePagesEncodingDataStream.Seek(position, SeekOrigin.Begin);
                                int offset = ptr4->Offset;
                                ReadCodePageIndex(s_codePagesEncodingDataStream, array);
                                m_dataSize = ptr4->Offset - offset - m_codePageHeader.Length;
                            }
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        internal unsafe static int GetCodePageByteSize(int codePage)
        {
            byte[] array = new byte[sizeof(CodePageIndex)];
            lock (s_streamLock)
            {
                s_codePagesEncodingDataStream.Seek(44L, SeekOrigin.Begin);
                int codePageCount;
                fixed (byte* ptr = &s_codePagesDataHeader[0])
                {
                    CodePageDataFileHeader* ptr2 = (CodePageDataFileHeader*)ptr;
                    codePageCount = ptr2->CodePageCount;
                }
                fixed (byte* ptr3 = &array[0])
                {
                    CodePageIndex* ptr4 = (CodePageIndex*)ptr3;
                    for (int i = 0; i < codePageCount; i++)
                    {
                        ReadCodePageIndex(s_codePagesEncodingDataStream, array);
                        if (ptr4->CodePage == codePage)
                        {
                            return ptr4->ByteCount;
                        }
                    }
                }
            }
            return 0;
        }

        protected abstract void LoadManagedCodePage();

        protected unsafe byte* GetNativeMemory(int iSize)
        {
            if (safeNativeMemoryHandle == null)
            {
                byte* ptr = (byte*)(void*)Marshal.AllocHGlobal(iSize);
                safeNativeMemoryHandle = new SafeAllocHHandle((IntPtr)ptr);
            }
            return (byte*)(void*)safeNativeMemoryHandle.DangerousGetHandle();
        }

        protected abstract void ReadBestFitTable();

        internal char[] GetBestFitUnicodeToBytesData()
        {
            if (arrayUnicodeBestFit == null)
            {
                ReadBestFitTable();
            }
            return arrayUnicodeBestFit;
        }

        internal char[] GetBestFitBytesToUnicodeData()
        {
            if (arrayBytesBestFit == null)
            {
                ReadBestFitTable();
            }
            return arrayBytesBestFit;
        }

        internal void CheckMemorySection()
        {
            if (safeNativeMemoryHandle != null && safeNativeMemoryHandle.DangerousGetHandle() == IntPtr.Zero)
            {
                LoadManagedCodePage();
            }
        }
    }

    /// <summary>Provides access to an encoding provider for code pages that otherwise are available only in the desktop .NET Framework.</summary>
    public sealed class CodePagesEncodingProvider : EncodingProvider
    {
        private static readonly EncodingProvider s_singleton = new CodePagesEncodingProvider();

        private readonly Dictionary<int, Encoding> _encodings = new Dictionary<int, Encoding>();

        private readonly ReaderWriterLockSlim _cacheLock = new ReaderWriterLockSlim();

        private const int ISCIIAssemese = 57006;

        private const int ISCIIBengali = 57003;

        private const int ISCIIDevanagari = 57002;

        private const int ISCIIGujarathi = 57010;

        private const int ISCIIKannada = 57008;

        private const int ISCIIMalayalam = 57009;

        private const int ISCIIOriya = 57007;

        private const int ISCIIPanjabi = 57011;

        private const int ISCIITamil = 57004;

        private const int ISCIITelugu = 57005;

        private const int ISOKorean = 50225;

        private const int ChineseHZ = 52936;

        private const int ISO2022JP = 50220;

        private const int ISO2022JPESC = 50221;

        private const int ISO2022JPSISO = 50222;

        private const int ISOSimplifiedCN = 50227;

        private const int EUCJP = 51932;

        private const int CodePageMacGB2312 = 10008;

        private const int CodePageMacKorean = 10003;

        private const int CodePageGB2312 = 20936;

        private const int CodePageDLLKorean = 20949;

        private const int GB18030 = 54936;

        private const int DuplicateEUCCN = 51936;

        private const int EUCKR = 51949;

        private const int EUCCN = 936;

        private const int ISO_8859_8I = 38598;

        private const int ISO_8859_8_Visual = 28598;

        /// <summary>Gets an encoding provider for code pages supported in the desktop .NET Framework but not in the current .NET Framework platform.</summary>
        /// <returns>An encoding provider that allows access to encodings not supported on the current .NET Framework platform.</returns>
        public static EncodingProvider Instance => s_singleton;

        private static int SystemDefaultCodePage
        {
            get
            {
                if (global::Interop.Kernel32.TryGetACPCodePage(out var codePage) == false)
                {
                    return 0;
                }
                return codePage;
            }
        }

        internal CodePagesEncodingProvider()
        {
        }

        /// <summary>Returns the encoding associated with the specified code page identifier.</summary>
        /// <param name="codepage">The code page identifier of the preferred encoding which the encoding provider may support.</param>
        /// <returns>The encoding associated with the specified code page identifier, or <see langword="null" /> if the provider does not support the requested codepage encoding.</returns>
        #nullable enable
        public override Encoding? GetEncoding(int codepage)
        {
            if (codepage < 0 || codepage > 65535)
            {
                return null;
            }
            if (codepage == 0)
            {
                int systemDefaultCodePage = SystemDefaultCodePage;
                if (systemDefaultCodePage == 0)
                {
                    return null;
                }
                return GetEncoding(systemDefaultCodePage);
            }
#pragma warning disable CS8600
            Encoding value = null;
#pragma warning restore CS8600
            _cacheLock.EnterUpgradeableReadLock();
            try
            {
                if (_encodings.TryGetValue(codepage, out value))
                {
                    return value;
                }
                switch (System.Text.BaseCodePageEncoding.GetCodePageByteSize(codepage))
                {
                    case 1:
                        value = new System.Text.SBCSCodePageEncoding(codepage);
                        break;
                    case 2:
                        value = new System.Text.DBCSCodePageEncoding(codepage);
                        break;
                    default:
                        value = GetEncodingRare(codepage);
                        if (value == null)
                        {
                            return null;
                        }
                        break;
                }
                _cacheLock.EnterWriteLock();
                try
                {
                    if (_encodings.TryGetValue(codepage, out var value2))
                    {
                        return value2;
                    }
                    _encodings.Add(codepage, value);
                    return value;
                }
                finally
                {
                    _cacheLock.ExitWriteLock();
                }
            }
            finally
            {
                _cacheLock.ExitUpgradeableReadLock();
            }
        }
        
        /// <summary>Returns the encoding associated with the specified code page name.</summary>
        /// <param name="name">The code page name of the preferred encoding which the encoding provider may support.</param>
        /// <returns>The encoding associated with the specified code page, or <see langword="null" /> if the provider does not support the requested encoding.</returns>
        public override Encoding? GetEncoding(string name)
        {
            int codePageFromName = EncodingTable.GetCodePageFromName(name);
            if (codePageFromName == 0)
            {
                return null;
            }
            return GetEncoding(codePageFromName);
        }
        #nullable disable
        private static Encoding GetEncodingRare(int codepage)
        {
            Encoding result = null;
            switch (codepage)
            {
                case 57002:
                case 57003:
                case 57004:
                case 57005:
                case 57006:
                case 57007:
                case 57008:
                case 57009:
                case 57010:
                case 57011:
                    result = new System.Text.ISCIIEncoding(codepage);
                    break;
                case 10008:
                    result = new System.Text.DBCSCodePageEncoding(10008, 20936);
                    break;
                case 10003:
                    result = new System.Text.DBCSCodePageEncoding(10003, 20949);
                    break;
                case 54936:
                    result = new System.Text.GB18030Encoding();
                    break;
                case 50220:
                case 50221:
                case 50222:
                case 50225:
                case 52936:
                    result = new System.Text.ISO2022Encoding(codepage);
                    break;
                case 50227:
                case 51936:
                    result = new System.Text.DBCSCodePageEncoding(codepage, 936);
                    break;
                case 51932:
                    result = new System.Text.EUCJPEncoding();
                    break;
                case 51949:
                    result = new System.Text.DBCSCodePageEncoding(codepage, 20949);
                    break;
                case 38598:
                    result = new System.Text.SBCSCodePageEncoding(codepage, 28598);
                    break;
            }
            return result;
        }
    }

    internal class DBCSCodePageEncoding : System.Text.BaseCodePageEncoding
    {
        internal sealed class DBCSDecoder : System.Text.DecoderNLS
        {
            internal byte bLeftOver;

            internal override bool HasState => bLeftOver != 0;

            public DBCSDecoder(System.Text.DBCSCodePageEncoding encoding)
                : base(encoding)
            {
            }

            public override void Reset()
            {
                bLeftOver = 0;
                m_fallbackBuffer?.Reset();
            }
        }

        protected unsafe char* mapBytesToUnicode = null;

        protected unsafe ushort* mapUnicodeToBytes = null;

        protected const char UNKNOWN_CHAR_FLAG = '\0';

        protected const char UNICODE_REPLACEMENT_CHAR = '\ufffd';

        protected const char LEAD_BYTE_CHAR = '\ufffe';

        private ushort _bytesUnknown;

        private int _byteCountUnknown;

        protected char charUnknown;

        private static object s_InternalSyncObject;

        private static object InternalSyncObject
        {
            get
            {
                if (s_InternalSyncObject == null)
                {
                    object value = new object();
                    Interlocked.CompareExchange<object>(ref s_InternalSyncObject, value, (object)null);
                }
                return s_InternalSyncObject;
            }
        }

        public DBCSCodePageEncoding(int codePage)
            : this(codePage, codePage)
        {
        }

        internal unsafe DBCSCodePageEncoding(int codePage, int dataCodePage)
            : base(codePage, dataCodePage)
        {
        }

        internal unsafe DBCSCodePageEncoding(int codePage, int dataCodePage, EncoderFallback enc, DecoderFallback dec)
            : base(codePage, dataCodePage, enc, dec)
        {
        }

        internal unsafe static char ReadChar(char* pChar)
        {
            if (BitConverter.IsLittleEndian)
            {
                return *pChar;
            }
            return (char)BinaryPrimitives.ReverseEndianness(*pChar);
        }

        protected unsafe override void LoadManagedCodePage()
        {
            fixed (byte* ptr = &m_codePageHeader[0])
            {
                CodePageHeader* ptr2 = (CodePageHeader*)ptr;
                if (ptr2->ByteCount != 2)
                {
                    throw new NotSupportedException(System.SR.Format(MDCFR.Properties.Resources.NotSupported_NoCodepageData, CodePage));
                }
                _bytesUnknown = ptr2->ByteReplace;
                charUnknown = ptr2->UnicodeReplace;
                if (base.DecoderFallback is System.Text.InternalDecoderBestFitFallback)
                {
                    ((System.Text.InternalDecoderBestFitFallback)base.DecoderFallback).cReplacement = charUnknown;
                }
                _byteCountUnknown = 1;
                if (_bytesUnknown > 255)
                {
                    _byteCountUnknown++;
                }
                int num = 262148 + iExtraBytes;
                byte* nativeMemory = GetNativeMemory(num);
                Unsafe.InitBlockUnaligned(nativeMemory, 0, (uint)num);
                mapBytesToUnicode = (char*)nativeMemory;
                mapUnicodeToBytes = (ushort*)(nativeMemory + 131072);
                byte[] array = new byte[m_dataSize];
                lock (System.Text.BaseCodePageEncoding.s_streamLock)
                {
                    System.Text.BaseCodePageEncoding.s_codePagesEncodingDataStream.Seek(m_firstDataWordOffset, SeekOrigin.Begin);
                    int num2 = System.Text.BaseCodePageEncoding.s_codePagesEncodingDataStream.Read(array, 0, m_dataSize);
                }
                fixed (byte* ptr3 = array)
                {
                    char* ptr4 = (char*)ptr3;
                    int num3 = 0;
                    int num4 = 0;
                    while (num3 < 65536)
                    {
                        char c = ReadChar(ptr4);
                        ptr4++;
                        switch (c)
                        {
                            case '\u0001':
                                num3 = ReadChar(ptr4);
                                ptr4++;
                                continue;
                            case '\u0002':
                            case '\u0003':
                            case '\u0004':
                            case '\u0005':
                            case '\u0006':
                            case '\a':
                            case '\b':
                            case '\t':
                            case '\n':
                            case '\v':
                            case '\f':
                            case '\r':
                            case '\u000e':
                            case '\u000f':
                            case '\u0010':
                            case '\u0011':
                            case '\u0012':
                            case '\u0013':
                            case '\u0014':
                            case '\u0015':
                            case '\u0016':
                            case '\u0017':
                            case '\u0018':
                            case '\u0019':
                            case '\u001a':
                            case '\u001b':
                            case '\u001c':
                            case '\u001d':
                            case '\u001e':
                            case '\u001f':
                                num3 += c;
                                continue;
                        }
                        switch (c)
                        {
                            case '\uffff':
                                num4 = num3;
                                c = (char)num3;
                                break;
                            case '\ufffe':
                                num4 = num3;
                                break;
                            case '\ufffd':
                                num3++;
                                continue;
                            default:
                                num4 = num3;
                                break;
                        }
                        if (CleanUpBytes(ref num4))
                        {
                            if (c != '\ufffe')
                            {
                                mapUnicodeToBytes[(int)c] = (ushort)num4;
                            }
                            mapBytesToUnicode[num4] = c;
                        }
                        num3++;
                    }
                }
                CleanUpEndBytes(mapBytesToUnicode);
            }
        }

        protected virtual bool CleanUpBytes(ref int bytes)
        {
            return true;
        }

        protected unsafe virtual void CleanUpEndBytes(char* chars)
        {
        }

        protected unsafe override void ReadBestFitTable()
        {
            lock (InternalSyncObject)
            {
                if (arrayUnicodeBestFit != null)
                {
                    return;
                }
                byte[] array = new byte[m_dataSize];
                lock (System.Text.BaseCodePageEncoding.s_streamLock)
                {
                    System.Text.BaseCodePageEncoding.s_codePagesEncodingDataStream.Seek(m_firstDataWordOffset, SeekOrigin.Begin);
                    int num = System.Text.BaseCodePageEncoding.s_codePagesEncodingDataStream.Read(array, 0, m_dataSize);
                }
                fixed (byte* ptr = array)
                {
                    char* ptr2 = (char*)ptr;
                    int num2 = 0;
                    while (num2 < 65536)
                    {
                        char c = ReadChar(ptr2);
                        ptr2++;
                        switch (c)
                        {
                            case '\u0001':
                                num2 = ReadChar(ptr2);
                                ptr2++;
                                break;
                            case '\u0002':
                            case '\u0003':
                            case '\u0004':
                            case '\u0005':
                            case '\u0006':
                            case '\a':
                            case '\b':
                            case '\t':
                            case '\n':
                            case '\v':
                            case '\f':
                            case '\r':
                            case '\u000e':
                            case '\u000f':
                            case '\u0010':
                            case '\u0011':
                            case '\u0012':
                            case '\u0013':
                            case '\u0014':
                            case '\u0015':
                            case '\u0016':
                            case '\u0017':
                            case '\u0018':
                            case '\u0019':
                            case '\u001a':
                            case '\u001b':
                            case '\u001c':
                            case '\u001d':
                            case '\u001e':
                            case '\u001f':
                                num2 += c;
                                break;
                            default:
                                num2++;
                                break;
                        }
                    }
                    char* ptr3 = ptr2;
                    int num3 = 0;
                    num2 = ReadChar(ptr2);
                    ptr2++;
                    while (num2 < 65536)
                    {
                        char c2 = ReadChar(ptr2);
                        ptr2++;
                        switch (c2)
                        {
                            case '\u0001':
                                num2 = ReadChar(ptr2);
                                ptr2++;
                                continue;
                            case '\u0002':
                            case '\u0003':
                            case '\u0004':
                            case '\u0005':
                            case '\u0006':
                            case '\a':
                            case '\b':
                            case '\t':
                            case '\n':
                            case '\v':
                            case '\f':
                            case '\r':
                            case '\u000e':
                            case '\u000f':
                            case '\u0010':
                            case '\u0011':
                            case '\u0012':
                            case '\u0013':
                            case '\u0014':
                            case '\u0015':
                            case '\u0016':
                            case '\u0017':
                            case '\u0018':
                            case '\u0019':
                            case '\u001a':
                            case '\u001b':
                            case '\u001c':
                            case '\u001d':
                            case '\u001e':
                            case '\u001f':
                                num2 += c2;
                                continue;
                        }
                        if (c2 != '\ufffd')
                        {
                            int bytes = num2;
                            if (CleanUpBytes(ref bytes) && mapBytesToUnicode[bytes] != c2)
                            {
                                num3++;
                            }
                        }
                        num2++;
                    }
                    char[] array2 = new char[num3 * 2];
                    num3 = 0;
                    ptr2 = ptr3;
                    num2 = ReadChar(ptr2);
                    ptr2++;
                    bool flag = false;
                    while (num2 < 65536)
                    {
                        char c3 = ReadChar(ptr2);
                        ptr2++;
                        switch (c3)
                        {
                            case '\u0001':
                                num2 = ReadChar(ptr2);
                                ptr2++;
                                continue;
                            case '\u0002':
                            case '\u0003':
                            case '\u0004':
                            case '\u0005':
                            case '\u0006':
                            case '\a':
                            case '\b':
                            case '\t':
                            case '\n':
                            case '\v':
                            case '\f':
                            case '\r':
                            case '\u000e':
                            case '\u000f':
                            case '\u0010':
                            case '\u0011':
                            case '\u0012':
                            case '\u0013':
                            case '\u0014':
                            case '\u0015':
                            case '\u0016':
                            case '\u0017':
                            case '\u0018':
                            case '\u0019':
                            case '\u001a':
                            case '\u001b':
                            case '\u001c':
                            case '\u001d':
                            case '\u001e':
                            case '\u001f':
                                num2 += c3;
                                continue;
                        }
                        if (c3 != '\ufffd')
                        {
                            int bytes2 = num2;
                            if (CleanUpBytes(ref bytes2) && mapBytesToUnicode[bytes2] != c3)
                            {
                                if (bytes2 != num2)
                                {
                                    flag = true;
                                }
                                array2[num3++] = (char)bytes2;
                                array2[num3++] = c3;
                            }
                        }
                        num2++;
                    }
                    if (flag)
                    {
                        for (int i = 0; i < array2.Length - 2; i += 2)
                        {
                            int num4 = i;
                            char c4 = array2[i];
                            for (int j = i + 2; j < array2.Length; j += 2)
                            {
                                if (c4 > array2[j])
                                {
                                    c4 = array2[j];
                                    num4 = j;
                                }
                            }
                            if (num4 != i)
                            {
                                char c5 = array2[num4];
                                array2[num4] = array2[i];
                                array2[i] = c5;
                                c5 = array2[num4 + 1];
                                array2[num4 + 1] = array2[i + 1];
                                array2[i + 1] = c5;
                            }
                        }
                    }
                    arrayBytesBestFit = array2;
                    char* ptr4 = ptr2;
                    int num5 = ReadChar(ptr2++);
                    num3 = 0;
                    while (num5 < 65536)
                    {
                        char c6 = ReadChar(ptr2);
                        ptr2++;
                        switch (c6)
                        {
                            case '\u0001':
                                num5 = ReadChar(ptr2);
                                ptr2++;
                                continue;
                            case '\u0002':
                            case '\u0003':
                            case '\u0004':
                            case '\u0005':
                            case '\u0006':
                            case '\a':
                            case '\b':
                            case '\t':
                            case '\n':
                            case '\v':
                            case '\f':
                            case '\r':
                            case '\u000e':
                            case '\u000f':
                            case '\u0010':
                            case '\u0011':
                            case '\u0012':
                            case '\u0013':
                            case '\u0014':
                            case '\u0015':
                            case '\u0016':
                            case '\u0017':
                            case '\u0018':
                            case '\u0019':
                            case '\u001a':
                            case '\u001b':
                            case '\u001c':
                            case '\u001d':
                            case '\u001e':
                            case '\u001f':
                                num5 += c6;
                                continue;
                        }
                        if (c6 > '\0')
                        {
                            num3++;
                        }
                        num5++;
                    }
                    array2 = new char[num3 * 2];
                    ptr2 = ptr4;
                    num5 = ReadChar(ptr2++);
                    num3 = 0;
                    while (num5 < 65536)
                    {
                        char c7 = ReadChar(ptr2);
                        ptr2++;
                        switch (c7)
                        {
                            case '\u0001':
                                num5 = ReadChar(ptr2);
                                ptr2++;
                                continue;
                            case '\u0002':
                            case '\u0003':
                            case '\u0004':
                            case '\u0005':
                            case '\u0006':
                            case '\a':
                            case '\b':
                            case '\t':
                            case '\n':
                            case '\v':
                            case '\f':
                            case '\r':
                            case '\u000e':
                            case '\u000f':
                            case '\u0010':
                            case '\u0011':
                            case '\u0012':
                            case '\u0013':
                            case '\u0014':
                            case '\u0015':
                            case '\u0016':
                            case '\u0017':
                            case '\u0018':
                            case '\u0019':
                            case '\u001a':
                            case '\u001b':
                            case '\u001c':
                            case '\u001d':
                            case '\u001e':
                            case '\u001f':
                                num5 += c7;
                                continue;
                        }
                        if (c7 > '\0')
                        {
                            int bytes3 = c7;
                            if (CleanUpBytes(ref bytes3))
                            {
                                array2[num3++] = (char)num5;
                                array2[num3++] = mapBytesToUnicode[bytes3];
                            }
                        }
                        num5++;
                    }
                    arrayUnicodeBestFit = array2;
                }
            }
        }

        public unsafe override int GetByteCount(char* chars, int count, System.Text.EncoderNLS encoder)
        {
            CheckMemorySection();
            char c = '\0';
            if (encoder != null)
            {
                c = encoder.charLeftOver;
                if (encoder.InternalHasFallbackBuffer && encoder.FallbackBuffer.Remaining > 0)
                {
                    throw new ArgumentException(
                        System.SR.Format(
                            MDCFR.Properties.Resources.Argument_EncoderFallbackNotEmpty, 
                            EncodingName, 
                            encoder.Fallback.GetType()));
                }
            }
            int num = 0;
            char* ptr = chars + count;
            EncoderFallbackBuffer encoderFallbackBuffer = null;
            EncoderFallbackBufferHelper encoderFallbackBufferHelper = new EncoderFallbackBufferHelper(encoderFallbackBuffer);
            if (c > '\0')
            {
                encoderFallbackBuffer = encoder.FallbackBuffer;
                encoderFallbackBufferHelper = new EncoderFallbackBufferHelper(encoderFallbackBuffer);
                encoderFallbackBufferHelper.InternalInitialize(chars, ptr, encoder, _setEncoder: false);
                encoderFallbackBufferHelper.InternalFallback(c, ref chars);
            }
            char c2;
            while ((c2 = ((encoderFallbackBuffer != null) ? encoderFallbackBufferHelper.InternalGetNextChar() : '\0')) != 0 || chars < ptr)
            {
                if (c2 == '\0')
                {
                    c2 = *chars;
                    chars++;
                }
                ushort num2 = mapUnicodeToBytes[(int)c2];
                if (num2 == 0 && c2 != 0)
                {
                    if (encoderFallbackBuffer == null)
                    {
                        encoderFallbackBuffer = ((encoder != null) ? encoder.FallbackBuffer : base.EncoderFallback.CreateFallbackBuffer());
                        encoderFallbackBufferHelper = new EncoderFallbackBufferHelper(encoderFallbackBuffer);
                        encoderFallbackBufferHelper.InternalInitialize(ptr - count, ptr, encoder, _setEncoder: false);
                    }
                    encoderFallbackBufferHelper.InternalFallback(c2, ref chars);
                }
                else
                {
                    num++;
                    if (num2 >= 256)
                    {
                        num++;
                    }
                }
            }
            return num;
        }

        public unsafe override int GetBytes(char* chars, int charCount, byte* bytes, int byteCount, System.Text.EncoderNLS encoder)
        {
            CheckMemorySection();
            EncoderFallbackBuffer encoderFallbackBuffer = null;
            char* ptr = chars + charCount;
            char* ptr2 = chars;
            byte* ptr3 = bytes;
            byte* ptr4 = bytes + byteCount;
            EncoderFallbackBufferHelper encoderFallbackBufferHelper = new EncoderFallbackBufferHelper(encoderFallbackBuffer);
            char c = '\0';
            if (encoder != null)
            {
                c = encoder.charLeftOver;
                encoderFallbackBuffer = encoder.FallbackBuffer;
                encoderFallbackBufferHelper = new EncoderFallbackBufferHelper(encoderFallbackBuffer);
                encoderFallbackBufferHelper.InternalInitialize(chars, ptr, encoder, _setEncoder: true);
                if (encoder.m_throwOnOverflow && encoderFallbackBuffer.Remaining > 0)
                {
                    throw new ArgumentException(
                        System.SR.Format(
                            MDCFR.Properties.Resources.Argument_EncoderFallbackNotEmpty, 
                            EncodingName, 
                            encoder.Fallback.GetType()));
                }
                if (c > '\0')
                {
                    encoderFallbackBufferHelper.InternalFallback(c, ref chars);
                }
            }
            char c2;
            while ((c2 = ((encoderFallbackBuffer != null) ? encoderFallbackBufferHelper.InternalGetNextChar() : '\0')) != 0 || chars < ptr)
            {
                if (c2 == '\0')
                {
                    c2 = *chars;
                    chars++;
                }
                ushort num = mapUnicodeToBytes[(int)c2];
                if (num == 0 && c2 != 0)
                {
                    if (encoderFallbackBuffer == null)
                    {
                        encoderFallbackBuffer = base.EncoderFallback.CreateFallbackBuffer();
                        encoderFallbackBufferHelper = new EncoderFallbackBufferHelper(encoderFallbackBuffer);
                        encoderFallbackBufferHelper.InternalInitialize(ptr - charCount, ptr, encoder, _setEncoder: true);
                    }
                    encoderFallbackBufferHelper.InternalFallback(c2, ref chars);
                    continue;
                }
                if (num >= 256)
                {
                    if (bytes + 1 >= ptr4)
                    {
                        if (encoderFallbackBuffer == null || !encoderFallbackBufferHelper.bFallingBack)
                        {
                            chars--;
                        }
                        else
                        {
                            encoderFallbackBuffer.MovePrevious();
                        }
                        ThrowBytesOverflow(encoder, chars == ptr2);
                        break;
                    }
                    *bytes = (byte)(num >> 8);
                    bytes++;
                }
                else if (bytes >= ptr4)
                {
                    if (encoderFallbackBuffer == null || !encoderFallbackBufferHelper.bFallingBack)
                    {
                        chars--;
                    }
                    else
                    {
                        encoderFallbackBuffer.MovePrevious();
                    }
                    ThrowBytesOverflow(encoder, chars == ptr2);
                    break;
                }
                *bytes = (byte)(num & 0xFFu);
                bytes++;
            }
            if (encoder != null)
            {
                if (encoderFallbackBuffer != null && !encoderFallbackBufferHelper.bUsedEncoder)
                {
                    encoder.charLeftOver = '\0';
                }
                encoder.m_charsUsed = (int)(chars - ptr2);
            }
            return (int)(bytes - ptr3);
        }

        public unsafe override int GetCharCount(byte* bytes, int count, System.Text.DecoderNLS baseDecoder)
        {
            CheckMemorySection();
            DBCSDecoder dBCSDecoder = (DBCSDecoder)baseDecoder;
            DecoderFallbackBuffer decoderFallbackBuffer = null;
            byte* ptr = bytes + count;
            int num = count;
            DecoderFallbackBufferHelper decoderFallbackBufferHelper = new DecoderFallbackBufferHelper(decoderFallbackBuffer);
            if (dBCSDecoder != null && dBCSDecoder.bLeftOver > 0)
            {
                if (count == 0)
                {
                    if (!dBCSDecoder.MustFlush)
                    {
                        return 0;
                    }
                    decoderFallbackBuffer = dBCSDecoder.FallbackBuffer;
                    decoderFallbackBufferHelper = new DecoderFallbackBufferHelper(decoderFallbackBuffer);
                    decoderFallbackBufferHelper.InternalInitialize(bytes, null);
                    byte[] bytes2 = new byte[1] { dBCSDecoder.bLeftOver };
                    return decoderFallbackBufferHelper.InternalFallback(bytes2, bytes);
                }
                int num2 = dBCSDecoder.bLeftOver << 8;
                num2 |= *bytes;
                bytes++;
                if (mapBytesToUnicode[num2] == '\0' && num2 != 0)
                {
                    num--;
                    decoderFallbackBuffer = dBCSDecoder.FallbackBuffer;
                    decoderFallbackBufferHelper = new DecoderFallbackBufferHelper(decoderFallbackBuffer);
                    decoderFallbackBufferHelper.InternalInitialize(ptr - count, null);
                    byte[] bytes3 = new byte[2]
                    {
                        (byte)(num2 >> 8),
                        (byte)num2
                    };
                    num += decoderFallbackBufferHelper.InternalFallback(bytes3, bytes);
                }
            }
            while (bytes < ptr)
            {
                int num3 = *bytes;
                bytes++;
                char c = mapBytesToUnicode[num3];
                if (c == '\ufffe')
                {
                    num--;
                    if (bytes < ptr)
                    {
                        num3 <<= 8;
                        num3 |= *bytes;
                        bytes++;
                        c = mapBytesToUnicode[num3];
                    }
                    else
                    {
                        if (dBCSDecoder != null && !dBCSDecoder.MustFlush)
                        {
                            break;
                        }
                        num++;
                        c = '\0';
                    }
                }
                if (c == '\0' && num3 != 0)
                {
                    if (decoderFallbackBuffer == null)
                    {
                        decoderFallbackBuffer = ((dBCSDecoder != null) ? dBCSDecoder.FallbackBuffer : base.DecoderFallback.CreateFallbackBuffer());
                        decoderFallbackBufferHelper = new DecoderFallbackBufferHelper(decoderFallbackBuffer);
                        decoderFallbackBufferHelper.InternalInitialize(ptr - count, null);
                    }
                    num--;
                    byte[] bytes4 = ((num3 >= 256) ? new byte[2]
                    {
                        (byte)(num3 >> 8),
                        (byte)num3
                    } : new byte[1] { (byte)num3 });
                    num += decoderFallbackBufferHelper.InternalFallback(bytes4, bytes);
                }
            }
            return num;
        }

        public unsafe override int GetChars(byte* bytes, int byteCount, char* chars, int charCount, System.Text.DecoderNLS baseDecoder)
        {
            CheckMemorySection();
            DBCSDecoder dBCSDecoder = (DBCSDecoder)baseDecoder;
            byte* ptr = bytes;
            byte* ptr2 = bytes + byteCount;
            char* ptr3 = chars;
            char* ptr4 = chars + charCount;
            bool flag = false;
            DecoderFallbackBuffer decoderFallbackBuffer = null;
            DecoderFallbackBufferHelper decoderFallbackBufferHelper = new DecoderFallbackBufferHelper(decoderFallbackBuffer);
            if (dBCSDecoder != null && dBCSDecoder.bLeftOver > 0)
            {
                if (byteCount == 0)
                {
                    if (!dBCSDecoder.MustFlush)
                    {
                        return 0;
                    }
                    decoderFallbackBuffer = dBCSDecoder.FallbackBuffer;
                    decoderFallbackBufferHelper = new DecoderFallbackBufferHelper(decoderFallbackBuffer);
                    decoderFallbackBufferHelper.InternalInitialize(bytes, ptr4);
                    byte[] bytes2 = new byte[1] { dBCSDecoder.bLeftOver };
                    if (!decoderFallbackBufferHelper.InternalFallback(bytes2, bytes, ref chars))
                    {
                        ThrowCharsOverflow(dBCSDecoder, nothingDecoded: true);
                    }
                    dBCSDecoder.bLeftOver = 0;
                    return (int)(chars - ptr3);
                }
                int num = dBCSDecoder.bLeftOver << 8;
                num |= *bytes;
                bytes++;
                char c = mapBytesToUnicode[num];
                if (c == '\0' && num != 0)
                {
                    decoderFallbackBuffer = dBCSDecoder.FallbackBuffer;
                    decoderFallbackBufferHelper = new DecoderFallbackBufferHelper(decoderFallbackBuffer);
                    decoderFallbackBufferHelper.InternalInitialize(ptr2 - byteCount, ptr4);
                    byte[] bytes3 = new byte[2]
                    {
                        (byte)(num >> 8),
                        (byte)num
                    };
                    if (!decoderFallbackBufferHelper.InternalFallback(bytes3, bytes, ref chars))
                    {
                        ThrowCharsOverflow(dBCSDecoder, nothingDecoded: true);
                    }
                }
                else
                {
                    if (chars >= ptr4)
                    {
                        ThrowCharsOverflow(dBCSDecoder, nothingDecoded: true);
                    }
                    *(chars++) = c;
                }
            }
            while (bytes < ptr2)
            {
                int num2 = *bytes;
                bytes++;
                char c2 = mapBytesToUnicode[num2];
                if (c2 == '\ufffe')
                {
                    if (bytes < ptr2)
                    {
                        num2 <<= 8;
                        num2 |= *bytes;
                        bytes++;
                        c2 = mapBytesToUnicode[num2];
                    }
                    else
                    {
                        if (dBCSDecoder != null && !dBCSDecoder.MustFlush)
                        {
                            flag = true;
                            dBCSDecoder.bLeftOver = (byte)num2;
                            break;
                        }
                        c2 = '\0';
                    }
                }
                if (c2 == '\0' && num2 != 0)
                {
                    if (decoderFallbackBuffer == null)
                    {
                        decoderFallbackBuffer = ((dBCSDecoder != null) ? dBCSDecoder.FallbackBuffer : base.DecoderFallback.CreateFallbackBuffer());
                        decoderFallbackBufferHelper = new DecoderFallbackBufferHelper(decoderFallbackBuffer);
                        decoderFallbackBufferHelper.InternalInitialize(ptr2 - byteCount, ptr4);
                    }
                    byte[] array = ((num2 >= 256) ? new byte[2]
                    {
                        (byte)(num2 >> 8),
                        (byte)num2
                    } : new byte[1] { (byte)num2 });
                    if (!decoderFallbackBufferHelper.InternalFallback(array, bytes, ref chars))
                    {
                        bytes -= array.Length;
                        decoderFallbackBufferHelper.InternalReset();
                        ThrowCharsOverflow(dBCSDecoder, bytes == ptr);
                        break;
                    }
                    continue;
                }
                if (chars >= ptr4)
                {
                    bytes--;
                    if (num2 >= 256)
                    {
                        bytes--;
                    }
                    ThrowCharsOverflow(dBCSDecoder, bytes == ptr);
                    break;
                }
                *(chars++) = c2;
            }
            if (dBCSDecoder != null)
            {
                if (!flag)
                {
                    dBCSDecoder.bLeftOver = 0;
                }
                dBCSDecoder.m_bytesUsed = (int)(bytes - ptr);
            }
            return (int)(chars - ptr3);
        }

        public override int GetMaxByteCount(int charCount)
        {
            if (charCount < 0)
            {
                throw new ArgumentOutOfRangeException("charCount", MDCFR.Properties.Resources.ArgumentOutOfRange_NeedNonNegNum);
            }
            long num = (long)charCount + 1L;
            if (base.EncoderFallback.MaxCharCount > 1)
            {
                num *= base.EncoderFallback.MaxCharCount;
            }
            num *= 2;
            if (num > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException("charCount", MDCFR.Properties.Resources.ArgumentOutOfRange_GetByteCountOverflow);
            }
            return (int)num;
        }

        public override int GetMaxCharCount(int byteCount)
        {
            if (byteCount < 0)
            {
                throw new ArgumentOutOfRangeException("byteCount", MDCFR.Properties.Resources.ArgumentOutOfRange_NeedNonNegNum);
            }
            long num = (long)byteCount + 1L;
            if (base.DecoderFallback.MaxCharCount > 1)
            {
                num *= base.DecoderFallback.MaxCharCount;
            }
            if (num > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException("byteCount", MDCFR.Properties.Resources.ArgumentOutOfRange_GetCharCountOverflow);
            }
            return (int)num;
        }

        public override Decoder GetDecoder()
        {
            return new DBCSDecoder(this);
        }
    }

    internal struct DecoderFallbackBufferHelper
    {
        internal unsafe byte* byteStart;

        internal unsafe char* charEnd;

        private readonly DecoderFallbackBuffer _fallbackBuffer;

        public unsafe DecoderFallbackBufferHelper(DecoderFallbackBuffer fallbackBuffer)
        {
            _fallbackBuffer = fallbackBuffer;
            byteStart = null;
            charEnd = null;
        }

        internal unsafe void InternalReset()
        {
            byteStart = null;
            _fallbackBuffer.Reset();
        }

        internal unsafe void InternalInitialize(byte* _byteStart, char* _charEnd)
        {
            byteStart = _byteStart;
            charEnd = _charEnd;
        }

        internal unsafe bool InternalFallback(byte[] bytes, byte* pBytes, ref char* chars)
        {
            if (_fallbackBuffer.Fallback(bytes, (int)(pBytes - byteStart - bytes.Length)))
            {
                char* ptr = chars;
                bool flag = false;
                char nextChar;
                while ((nextChar = _fallbackBuffer.GetNextChar()) != 0)
                {
                    if (char.IsSurrogate(nextChar))
                    {
                        if (char.IsHighSurrogate(nextChar))
                        {
                            if (flag)
                            {
                                throw new ArgumentException(MDCFR.Properties.Resources.Argument_InvalidCharSequenceNoIndex);
                            }
                            flag = true;
                        }
                        else
                        {
                            if (!flag)
                            {
                                throw new ArgumentException(MDCFR.Properties.Resources.Argument_InvalidCharSequenceNoIndex);
                            }
                            flag = false;
                        }
                    }
                    if (ptr >= charEnd)
                    {
                        return false;
                    }
                    *(ptr++) = nextChar;
                }
                if (flag)
                {
                    throw new ArgumentException(MDCFR.Properties.Resources.Argument_InvalidCharSequenceNoIndex);
                }
                chars = ptr;
            }
            return true;
        }

        internal unsafe int InternalFallback(byte[] bytes, byte* pBytes)
        {
            if (_fallbackBuffer.Fallback(bytes, (int)(pBytes - byteStart - bytes.Length)))
            {
                int num = 0;
                bool flag = false;
                char nextChar;
                while ((nextChar = _fallbackBuffer.GetNextChar()) != 0)
                {
                    if (char.IsSurrogate(nextChar))
                    {
                        if (char.IsHighSurrogate(nextChar))
                        {
                            if (flag)
                            {
                                throw new ArgumentException(MDCFR.Properties.Resources.Argument_InvalidCharSequenceNoIndex);
                            }
                            flag = true;
                        }
                        else
                        {
                            if (!flag)
                            {
                                throw new ArgumentException(MDCFR.Properties.Resources.Argument_InvalidCharSequenceNoIndex);
                            }
                            flag = false;
                        }
                    }
                    num++;
                }
                if (flag)
                {
                    throw new ArgumentException(MDCFR.Properties.Resources.Argument_InvalidCharSequenceNoIndex);
                }
                return num;
            }
            return 0;
        }
    }

    internal class DecoderNLS : Decoder, ISerializable
    {
        protected System.Text.EncodingNLS m_encoding;

        protected bool m_mustFlush;

        internal bool m_throwOnOverflow;

        internal int m_bytesUsed;

        internal DecoderFallback m_fallback;

        internal DecoderFallbackBuffer m_fallbackBuffer;

        internal new DecoderFallback Fallback => m_fallback;

        internal bool InternalHasFallbackBuffer => m_fallbackBuffer != null;

        public new DecoderFallbackBuffer FallbackBuffer
        {
            get
            {
                if (m_fallbackBuffer == null)
                {
                    m_fallbackBuffer = ((m_fallback != null) ? m_fallback.CreateFallbackBuffer() : DecoderFallback.ReplacementFallback.CreateFallbackBuffer());
                }
                return m_fallbackBuffer;
            }
        }

        public bool MustFlush => m_mustFlush;

        internal virtual bool HasState => false;

        internal DecoderNLS(System.Text.EncodingNLS encoding)
        {
            m_encoding = encoding;
            m_fallback = m_encoding.DecoderFallback;
            Reset();
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new PlatformNotSupportedException();
        }

        public override void Reset()
        {
            m_fallbackBuffer?.Reset();
        }

        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            return GetCharCount(bytes, index, count, flush: false);
        }

        public unsafe override int GetCharCount(byte[] bytes, int index, int count, bool flush)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }
            if (index < 0 || count < 0)
            {
                throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", MDCFR.Properties.Resources.ArgumentOutOfRange_NeedNonNegNum);
            }
            if (bytes.Length - index < count)
            {
                throw new ArgumentOutOfRangeException("bytes", MDCFR.Properties.Resources.ArgumentOutOfRange_IndexCountBuffer);
            }
            if (bytes.Length == 0)
            {
                bytes = new byte[1];
            }
            fixed (byte* ptr = &bytes[0])
            {
                return GetCharCount(ptr + index, count, flush);
            }
        }

        public unsafe override int GetCharCount(byte* bytes, int count, bool flush)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", MDCFR.Properties.Resources.ArgumentOutOfRange_NeedNonNegNum);
            }
            m_mustFlush = flush;
            m_throwOnOverflow = true;
            return m_encoding.GetCharCount(bytes, count, this);
        }

        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            return GetChars(bytes, byteIndex, byteCount, chars, charIndex, flush: false);
        }

        public unsafe override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex, bool flush)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }
            if (chars == null)
            {
                throw new ArgumentNullException("chars");
            }
            if (byteIndex < 0 || byteCount < 0)
            {
                throw new ArgumentOutOfRangeException((byteIndex < 0) ? "byteIndex" : "byteCount", MDCFR.Properties.Resources.ArgumentOutOfRange_NeedNonNegNum);
            }
            if (bytes.Length - byteIndex < byteCount)
            {
                throw new ArgumentOutOfRangeException("bytes", MDCFR.Properties.Resources.ArgumentOutOfRange_IndexCountBuffer);
            }
            if (charIndex < 0 || charIndex > chars.Length)
            {
                throw new ArgumentOutOfRangeException("charIndex", MDCFR.Properties.Resources.ArgumentOutOfRange_IndexMustBeLessOrEqual);
            }
            if (bytes.Length == 0)
            {
                bytes = new byte[1];
            }
            int charCount = chars.Length - charIndex;
            if (chars.Length == 0)
            {
                chars = new char[1];
            }
            fixed (byte* ptr = &bytes[0])
            {
                fixed (char* ptr2 = &chars[0])
                {
                    return GetChars(ptr + byteIndex, byteCount, ptr2 + charIndex, charCount, flush);
                }
            }
        }

        public unsafe override int GetChars(byte* bytes, int byteCount, char* chars, int charCount, bool flush)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }
            if (chars == null)
            {
                throw new ArgumentNullException("chars");
            }
            if (byteCount < 0 || charCount < 0)
            {
                throw new ArgumentOutOfRangeException((byteCount < 0) ? "byteCount" : "charCount", MDCFR.Properties.Resources.ArgumentOutOfRange_NeedNonNegNum);
            }
            m_mustFlush = flush;
            m_throwOnOverflow = true;
            return m_encoding.GetChars(bytes, byteCount, chars, charCount, this);
        }

        public unsafe override void Convert(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex, int charCount, bool flush, out int bytesUsed, out int charsUsed, out bool completed)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }
            if (chars == null)
            {
                throw new ArgumentNullException("chars");
            }
            if (byteIndex < 0 || byteCount < 0)
            {
                throw new ArgumentOutOfRangeException((byteIndex < 0) ? "byteIndex" : "byteCount", MDCFR.Properties.Resources.ArgumentOutOfRange_NeedNonNegNum);
            }
            if (charIndex < 0 || charCount < 0)
            {
                throw new ArgumentOutOfRangeException((charIndex < 0) ? "charIndex" : "charCount", MDCFR.Properties.Resources.ArgumentOutOfRange_NeedNonNegNum);
            }
            if (bytes.Length - byteIndex < byteCount)
            {
                throw new ArgumentOutOfRangeException("bytes", MDCFR.Properties.Resources.ArgumentOutOfRange_IndexCountBuffer);
            }
            if (chars.Length - charIndex < charCount)
            {
                throw new ArgumentOutOfRangeException("chars", MDCFR.Properties.Resources.ArgumentOutOfRange_IndexCountBuffer);
            }
            if (bytes.Length == 0)
            {
                bytes = new byte[1];
            }
            if (chars.Length == 0)
            {
                chars = new char[1];
            }
            fixed (byte* ptr = &bytes[0])
            {
                fixed (char* ptr2 = &chars[0])
                {
                    Convert(ptr + byteIndex, byteCount, ptr2 + charIndex, charCount, flush, out bytesUsed, out charsUsed, out completed);
                }
            }
        }

        public unsafe override void Convert(byte* bytes, int byteCount, char* chars, int charCount, bool flush, out int bytesUsed, out int charsUsed, out bool completed)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }
            if (chars == null)
            {
                throw new ArgumentNullException("chars");
            }
            if (byteCount < 0 || charCount < 0)
            {
                throw new ArgumentOutOfRangeException((byteCount < 0) ? "byteCount" : "charCount", MDCFR.Properties.Resources.ArgumentOutOfRange_NeedNonNegNum);
            }
            m_mustFlush = flush;
            m_throwOnOverflow = false;
            m_bytesUsed = 0;
            charsUsed = m_encoding.GetChars(bytes, byteCount, chars, charCount, this);
            bytesUsed = m_bytesUsed;
            completed = bytesUsed == byteCount && (!flush || !HasState) && (m_fallbackBuffer == null || m_fallbackBuffer.Remaining == 0);
        }

        internal void ClearMustFlush()
        {
            m_mustFlush = false;
        }
    }

    internal struct EncoderFallbackBufferHelper
    {
        internal unsafe char* charStart;

        internal unsafe char* charEnd;

        internal System.Text.EncoderNLS encoder;

        internal bool setEncoder;

        internal bool bUsedEncoder;

        internal bool bFallingBack;

        internal int iRecursionCount;

        private const int iMaxRecursion = 250;

        private readonly EncoderFallbackBuffer _fallbackBuffer;

        public unsafe EncoderFallbackBufferHelper(EncoderFallbackBuffer fallbackBuffer)
        {
            _fallbackBuffer = fallbackBuffer;
            bFallingBack = (bUsedEncoder = (setEncoder = false));
            iRecursionCount = 0;
            charEnd = (charStart = null);
            encoder = null;
        }

        internal unsafe void InternalReset()
        {
            charStart = null;
            bFallingBack = false;
            iRecursionCount = 0;
            _fallbackBuffer.Reset();
        }

        internal unsafe void InternalInitialize(char* _charStart, char* _charEnd, System.Text.EncoderNLS _encoder, bool _setEncoder)
        {
            charStart = _charStart;
            charEnd = _charEnd;
            encoder = _encoder;
            setEncoder = _setEncoder;
            bUsedEncoder = false;
            bFallingBack = false;
            iRecursionCount = 0;
        }

        internal char InternalGetNextChar()
        {
            char nextChar = _fallbackBuffer.GetNextChar();
            bFallingBack = nextChar != '\0';
            if (nextChar == '\0')
            {
                iRecursionCount = 0;
            }
            return nextChar;
        }

        internal unsafe bool InternalFallback(char ch, ref char* chars)
        {
            int index = (int)(chars - charStart) - 1;
            if (char.IsHighSurrogate(ch))
            {
                if (chars >= charEnd)
                {
                    if (encoder != null && !encoder.MustFlush)
                    {
                        if (setEncoder)
                        {
                            bUsedEncoder = true;
                            encoder.charLeftOver = ch;
                        }
                        bFallingBack = false;
                        return false;
                    }
                }
                else
                {
                    char c = *chars;
                    if (char.IsLowSurrogate(c))
                    {
                        if (bFallingBack && iRecursionCount++ > 250)
                        {
                            ThrowLastCharRecursive(char.ConvertToUtf32(ch, c));
                        }
                        chars++;
                        bFallingBack = _fallbackBuffer.Fallback(ch, c, index);
                        return bFallingBack;
                    }
                }
            }
            if (bFallingBack && iRecursionCount++ > 250)
            {
                ThrowLastCharRecursive(ch);
            }
            bFallingBack = _fallbackBuffer.Fallback(ch, index);
            return bFallingBack;
        }

        internal static void ThrowLastCharRecursive(int charRecursive)
        {
            throw new ArgumentException(System.SR.Format(MDCFR.Properties.Resources.Argument_RecursiveFallback, charRecursive), "chars");
        }
    }

    internal class EncoderNLS : Encoder, ISerializable
    {
        internal char charLeftOver;

        protected System.Text.EncodingNLS m_encoding;

        protected bool m_mustFlush;

        internal bool m_throwOnOverflow;

        internal int m_charsUsed;

        internal EncoderFallback m_fallback;

        internal EncoderFallbackBuffer m_fallbackBuffer;

        internal new EncoderFallback Fallback => m_fallback;

        internal bool InternalHasFallbackBuffer => m_fallbackBuffer != null;

        public new EncoderFallbackBuffer FallbackBuffer
        {
            get
            {
                if (m_fallbackBuffer == null)
                {
                    m_fallbackBuffer = ((m_fallback != null) ? m_fallback.CreateFallbackBuffer() : EncoderFallback.ReplacementFallback.CreateFallbackBuffer());
                }
                return m_fallbackBuffer;
            }
        }

        public Encoding Encoding => m_encoding;

        public bool MustFlush => m_mustFlush;

        internal virtual bool HasState => charLeftOver != '\0';

        internal EncoderNLS(System.Text.EncodingNLS encoding)
        {
            m_encoding = encoding;
            m_fallback = m_encoding.EncoderFallback;
            Reset();
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new PlatformNotSupportedException();
        }

        public override void Reset()
        {
            charLeftOver = '\0';
            m_fallbackBuffer?.Reset();
        }

        public unsafe override int GetByteCount(char[] chars, int index, int count, bool flush)
        {
            if (chars == null)
            {
                throw new ArgumentNullException("chars");
            }
            if (index < 0 || count < 0)
            {
                throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", MDCFR.Properties.Resources.ArgumentOutOfRange_NeedNonNegNum);
            }
            if (chars.Length - index < count)
            {
                throw new ArgumentOutOfRangeException("chars", MDCFR.Properties.Resources.ArgumentOutOfRange_IndexCountBuffer);
            }
            if (chars.Length == 0)
            {
                chars = new char[1];
            }
            int num = -1;
            fixed (char* ptr = &chars[0])
            {
                num = GetByteCount(ptr + index, count, flush);
            }
            return num;
        }

        public unsafe override int GetByteCount(char* chars, int count, bool flush)
        {
            if (chars == null)
            {
                throw new ArgumentNullException("chars");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", MDCFR.Properties.Resources.ArgumentOutOfRange_NeedNonNegNum);
            }
            m_mustFlush = flush;
            m_throwOnOverflow = true;
            return m_encoding.GetByteCount(chars, count, this);
        }

        public unsafe override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex, bool flush)
        {
            if (chars == null)
            {
                throw new ArgumentNullException("chars");
            }
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }
            if (charIndex < 0 || charCount < 0)
            {
                throw new ArgumentOutOfRangeException((charIndex < 0) ? "charIndex" : "charCount", MDCFR.Properties.Resources.ArgumentOutOfRange_NeedNonNegNum);
            }
            if (chars.Length - charIndex < charCount)
            {
                throw new ArgumentOutOfRangeException("chars", MDCFR.Properties.Resources.ArgumentOutOfRange_IndexCountBuffer);
            }
            if (byteIndex < 0 || byteIndex > bytes.Length)
            {
                throw new ArgumentOutOfRangeException("byteIndex", MDCFR.Properties.Resources.ArgumentOutOfRange_IndexMustBeLessOrEqual);
            }
            if (chars.Length == 0)
            {
                chars = new char[1];
            }
            int byteCount = bytes.Length - byteIndex;
            if (bytes.Length == 0)
            {
                bytes = new byte[1];
            }
            fixed (char* ptr = &chars[0])
            {
                fixed (byte* ptr2 = &bytes[0])
                {
                    return GetBytes(ptr + charIndex, charCount, ptr2 + byteIndex, byteCount, flush);
                }
            }
        }

        public unsafe override int GetBytes(char* chars, int charCount, byte* bytes, int byteCount, bool flush)
        {
            if (chars == null)
            {
                throw new ArgumentNullException("chars");
            }
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }
            if (byteCount < 0 || charCount < 0)
            {
                throw new ArgumentOutOfRangeException((byteCount < 0) ? "byteCount" : "charCount", MDCFR.Properties.Resources.ArgumentOutOfRange_NeedNonNegNum);
            }
            m_mustFlush = flush;
            m_throwOnOverflow = true;
            return m_encoding.GetBytes(chars, charCount, bytes, byteCount, this);
        }

        public unsafe override void Convert(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex, int byteCount, bool flush, out int charsUsed, out int bytesUsed, out bool completed)
        {
            if (chars == null)
            {
                throw new ArgumentNullException("chars");
            }
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }
            if (charIndex < 0 || charCount < 0)
            {
                throw new ArgumentOutOfRangeException((charIndex < 0) ? "charIndex" : "charCount", 
                    MDCFR.Properties.Resources.ArgumentOutOfRange_NeedNonNegNum);
            }
            if (byteIndex < 0 || byteCount < 0)
            {
                throw new ArgumentOutOfRangeException((byteIndex < 0) ? "byteIndex" : "byteCount", 
                    MDCFR.Properties.Resources.ArgumentOutOfRange_NeedNonNegNum);
            }
            if (chars.Length - charIndex < charCount)
            {
                throw new ArgumentOutOfRangeException("chars", MDCFR.Properties.Resources.ArgumentOutOfRange_IndexCountBuffer);
            }
            if (bytes.Length - byteIndex < byteCount)
            {
                throw new ArgumentOutOfRangeException("bytes", MDCFR.Properties.Resources.ArgumentOutOfRange_IndexCountBuffer);
            }
            if (chars.Length == 0)
            {
                chars = new char[1];
            }
            if (bytes.Length == 0)
            {
                bytes = new byte[1];
            }
            fixed (char* ptr = &chars[0])
            {
                fixed (byte* ptr2 = &bytes[0])
                {
                    Convert(ptr + charIndex, charCount, ptr2 + byteIndex, byteCount, flush, out charsUsed, out bytesUsed, out completed);
                }
            }
        }

        public unsafe override void Convert(char* chars, int charCount, byte* bytes, int byteCount, bool flush, out int charsUsed, out int bytesUsed, out bool completed)
        {
            if (chars == null)
            {
                throw new ArgumentNullException("chars");
            }
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }
            if (charCount < 0 || byteCount < 0)
            {
                throw new ArgumentOutOfRangeException((charCount < 0) ? "charCount" : "byteCount", MDCFR.Properties.Resources.ArgumentOutOfRange_NeedNonNegNum);
            }
            m_mustFlush = flush;
            m_throwOnOverflow = false;
            m_charsUsed = 0;
            bytesUsed = m_encoding.GetBytes(chars, charCount, bytes, byteCount, this);
            charsUsed = m_charsUsed;
            completed = charsUsed == charCount && (!flush || !HasState) && (m_fallbackBuffer == null || m_fallbackBuffer.Remaining == 0);
        }

        internal void ClearMustFlush()
        {
            m_mustFlush = false;
        }
    }

    internal sealed class EncodingByteBuffer
    {
        private unsafe byte* _bytes;

        private unsafe readonly byte* _byteStart;

        private unsafe readonly byte* _byteEnd;

        private unsafe char* _chars;

        private unsafe readonly char* _charStart;

        private unsafe readonly char* _charEnd;

        private int _byteCountResult;

        private readonly System.Text.EncodingNLS _enc;

        private readonly System.Text.EncoderNLS _encoder;

        internal EncoderFallbackBuffer fallbackBuffer;

        internal EncoderFallbackBufferHelper fallbackBufferHelper;

        internal unsafe bool MoreData
        {
            get
            {
                if (fallbackBuffer.Remaining <= 0)
                {
                    return _chars < _charEnd;
                }
                return true;
            }
        }

        internal unsafe int CharsUsed => (int)(_chars - _charStart);

        internal int Count => _byteCountResult;

        internal unsafe EncodingByteBuffer(System.Text.EncodingNLS inEncoding, System.Text.EncoderNLS inEncoder, byte* inByteStart, int inByteCount, char* inCharStart, int inCharCount)
        {
            _enc = inEncoding;
            _encoder = inEncoder;
            _charStart = inCharStart;
            _chars = inCharStart;
            _charEnd = inCharStart + inCharCount;
            _bytes = inByteStart;
            _byteStart = inByteStart;
            _byteEnd = inByteStart + inByteCount;
            if (_encoder == null)
            {
                fallbackBuffer = _enc.EncoderFallback.CreateFallbackBuffer();
            }
            else
            {
                fallbackBuffer = _encoder.FallbackBuffer;
                if (_encoder.m_throwOnOverflow && _encoder.InternalHasFallbackBuffer && fallbackBuffer.Remaining > 0)
                {
                    throw new ArgumentException(
                        System.SR.Format(
                            MDCFR.Properties.Resources.Argument_EncoderFallbackNotEmpty, 
                            _encoder.Encoding.EncodingName, 
                            _encoder.Fallback.GetType()));
                }
            }
            fallbackBufferHelper = new EncoderFallbackBufferHelper(fallbackBuffer);
            fallbackBufferHelper.InternalInitialize(_chars, _charEnd, _encoder, _bytes != null);
        }

        internal unsafe bool AddByte(byte b, int moreBytesExpected)
        {
            if (_bytes != null)
            {
                if (_bytes >= _byteEnd - moreBytesExpected)
                {
                    MovePrevious(bThrow: true);
                    return false;
                }
                *(_bytes++) = b;
            }
            _byteCountResult++;
            return true;
        }

        internal bool AddByte(byte b1)
        {
            return AddByte(b1, 0);
        }

        internal bool AddByte(byte b1, byte b2)
        {
            return AddByte(b1, b2, 0);
        }

        internal bool AddByte(byte b1, byte b2, int moreBytesExpected)
        {
            if (AddByte(b1, 1 + moreBytesExpected))
            {
                return AddByte(b2, moreBytesExpected);
            }
            return false;
        }

        internal bool AddByte(byte b1, byte b2, byte b3)
        {
            return AddByte(b1, b2, b3, 0);
        }

        internal bool AddByte(byte b1, byte b2, byte b3, int moreBytesExpected)
        {
            if (AddByte(b1, 2 + moreBytesExpected) && AddByte(b2, 1 + moreBytesExpected))
            {
                return AddByte(b3, moreBytesExpected);
            }
            return false;
        }

        internal bool AddByte(byte b1, byte b2, byte b3, byte b4)
        {
            if (AddByte(b1, 3) && AddByte(b2, 2) && AddByte(b3, 1))
            {
                return AddByte(b4, 0);
            }
            return false;
        }

        internal unsafe void MovePrevious(bool bThrow)
        {
            if (fallbackBufferHelper.bFallingBack)
            {
                fallbackBuffer.MovePrevious();
            }
            else if (_chars > _charStart)
            {
                _chars--;
            }
            if (bThrow)
            {
                _enc.ThrowBytesOverflow(_encoder, _bytes == _byteStart);
            }
        }

        internal unsafe bool Fallback(char charFallback)
        {
            return fallbackBufferHelper.InternalFallback(charFallback, ref _chars);
        }

        internal unsafe char GetNextChar()
        {
            char c = fallbackBufferHelper.InternalGetNextChar();
            if (c == '\0' && _chars < _charEnd)
            {
                c = *(_chars++);
            }
            return c;
        }
    }

    internal sealed class EncodingCharBuffer
    {
        private unsafe char* _chars;

        private unsafe readonly char* _charStart;

        private unsafe readonly char* _charEnd;

        private int _charCountResult;

        private readonly System.Text.EncodingNLS _enc;

        private readonly System.Text.DecoderNLS _decoder;

        private unsafe readonly byte* _byteStart;

        private unsafe readonly byte* _byteEnd;

        private unsafe byte* _bytes;

        private readonly DecoderFallbackBuffer _fallbackBuffer;

        private DecoderFallbackBufferHelper _fallbackBufferHelper;

        internal unsafe bool MoreData => _bytes < _byteEnd;

        internal unsafe int BytesUsed => (int)(_bytes - _byteStart);

        internal int Count => _charCountResult;

        internal unsafe EncodingCharBuffer(System.Text.EncodingNLS enc, System.Text.DecoderNLS decoder, char* charStart, int charCount, byte* byteStart, int byteCount)
        {
            _enc = enc;
            _decoder = decoder;
            _chars = charStart;
            _charStart = charStart;
            _charEnd = charStart + charCount;
            _byteStart = byteStart;
            _bytes = byteStart;
            _byteEnd = byteStart + byteCount;
            if (_decoder == null)
            {
                _fallbackBuffer = enc.DecoderFallback.CreateFallbackBuffer();
            }
            else
            {
                _fallbackBuffer = _decoder.FallbackBuffer;
            }
            _fallbackBufferHelper = new DecoderFallbackBufferHelper(_fallbackBuffer);
            _fallbackBufferHelper.InternalInitialize(_bytes, _charEnd);
        }

        internal unsafe bool AddChar(char ch, int numBytes)
        {
            if (_chars != null)
            {
                if (_chars >= _charEnd)
                {
                    _bytes -= numBytes;
                    _enc.ThrowCharsOverflow(_decoder, _bytes <= _byteStart);
                    return false;
                }
                *(_chars++) = ch;
            }
            _charCountResult++;
            return true;
        }

        internal bool AddChar(char ch)
        {
            return AddChar(ch, 1);
        }

        internal unsafe bool AddChar(char ch1, char ch2, int numBytes)
        {
            if (_chars >= _charEnd - 1)
            {
                _bytes -= numBytes;
                _enc.ThrowCharsOverflow(_decoder, _bytes <= _byteStart);
                return false;
            }
            if (AddChar(ch1, numBytes))
            {
                return AddChar(ch2, numBytes);
            }
            return false;
        }

        internal unsafe void AdjustBytes(int count)
        {
            _bytes += count;
        }

        internal unsafe bool EvenMoreData(int count)
        {
            return _bytes <= _byteEnd - count;
        }

        internal unsafe byte GetNextByte()
        {
            if (_bytes >= _byteEnd)
            {
                return 0;
            }
            return *(_bytes++);
        }

        internal bool Fallback(byte fallbackByte)
        {
            byte[] byteBuffer = new byte[1] { fallbackByte };
            return Fallback(byteBuffer);
        }

        internal bool Fallback(byte byte1, byte byte2)
        {
            byte[] byteBuffer = new byte[2] { byte1, byte2 };
            return Fallback(byteBuffer);
        }

        internal bool Fallback(byte byte1, byte byte2, byte byte3, byte byte4)
        {
            byte[] byteBuffer = new byte[4] { byte1, byte2, byte3, byte4 };
            return Fallback(byteBuffer);
        }

        internal unsafe bool Fallback(byte[] byteBuffer)
        {
            if (_chars != null)
            {
                char* chars = _chars;
                if (!_fallbackBufferHelper.InternalFallback(byteBuffer, _bytes, ref _chars))
                {
                    _bytes -= byteBuffer.Length;
                    _fallbackBufferHelper.InternalReset();
                    _enc.ThrowCharsOverflow(_decoder, _chars == _charStart);
                    return false;
                }
                _charCountResult += (int)(_chars - chars);
            }
            else
            {
                _charCountResult += _fallbackBufferHelper.InternalFallback(byteBuffer, _bytes);
            }
            return true;
        }
    }

    internal abstract class EncodingNLS : Encoding
    {
        private string _encodingName;

        private string _webName;

        public override string EncodingName
        {
            get
            {
                if (_encodingName == null)
                {
                    _encodingName = GetLocalizedEncodingNameResource(CodePage);
                    if (_encodingName == null)
                    {
                        throw new NotSupportedException(System.SR.Format(MDCFR.Properties.Resources.MissingEncodingNameResource, WebName, CodePage));
                    }
                    if (_encodingName.StartsWith("Globalization_cp_", StringComparison.OrdinalIgnoreCase))
                    {
                        _encodingName = EncodingTable.GetEnglishNameFromCodePage(CodePage);
                        if (_encodingName == null)
                        {
                            throw new NotSupportedException(System.SR.Format(MDCFR.Properties.Resources.MissingEncodingNameResource, WebName, CodePage));
                        }
                    }
                }
                return _encodingName;
            }
        }

        public override string WebName
        {
            get
            {
                if (_webName == null)
                {
                    _webName = EncodingTable.GetWebNameFromCodePage(CodePage);
                    if (_webName == null)
                    {
                        throw new NotSupportedException(System.SR.Format(MDCFR.Properties.Resources.NotSupported_NoCodepageData, CodePage));
                    }
                }
                return _webName;
            }
        }

        public override string HeaderName => CodePage switch
        {
            932 => "iso-2022-jp",
            50221 => "iso-2022-jp",
            50225 => "euc-kr",
            _ => WebName,
        };

        public override string BodyName => CodePage switch
        {
            932 => "iso-2022-jp",
            1250 => "iso-8859-2",
            1251 => "koi8-r",
            1252 => "iso-8859-1",
            1253 => "iso-8859-7",
            1254 => "iso-8859-9",
            50221 => "iso-2022-jp",
            50225 => "iso-2022-kr",
            _ => WebName,
        };

        protected EncodingNLS(int codePage)
            : base(codePage)
        {
        }

        protected EncodingNLS(int codePage, EncoderFallback enc, DecoderFallback dec)
            : base(codePage, enc, dec)
        {
        }

        public unsafe abstract int GetByteCount(char* chars, int count, System.Text.EncoderNLS encoder);

        public unsafe abstract int GetBytes(char* chars, int charCount, byte* bytes, int byteCount, System.Text.EncoderNLS encoder);

        public unsafe abstract int GetCharCount(byte* bytes, int count, System.Text.DecoderNLS decoder);

        public unsafe abstract int GetChars(byte* bytes, int byteCount, char* chars, int charCount, System.Text.DecoderNLS decoder);

        public unsafe override int GetByteCount(char[] chars, int index, int count)
        {
            if (chars == null)
            {
                throw new ArgumentNullException("chars");
            }
            if (index < 0 || count < 0)
            {
                throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", MDCFR.Properties.Resources.ArgumentOutOfRange_NeedNonNegNum);
            }
            if (chars.Length - index < count)
            {
                throw new ArgumentOutOfRangeException("chars", MDCFR.Properties.Resources.ArgumentOutOfRange_IndexCountBuffer);
            }
            if (chars.Length == 0)
            {
                return 0;
            }
            fixed (char* ptr = &chars[0])
            {
                return GetByteCount(ptr + index, count, null);
            }
        }

        public unsafe override int GetByteCount(string s)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }
            fixed (char* chars = s)
            {
                return GetByteCount(chars, s.Length, null);
            }
        }

        public unsafe override int GetByteCount(char* chars, int count)
        {
            if (chars == null)
            {
                throw new ArgumentNullException("chars");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", MDCFR.Properties.Resources.ArgumentOutOfRange_NeedNonNegNum);
            }
            return GetByteCount(chars, count, null);
        }

        public unsafe override int GetBytes(string s, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }
            if (charIndex < 0 || charCount < 0)
            {
                throw new ArgumentOutOfRangeException((charIndex < 0) ? "charIndex" : "charCount", MDCFR.Properties.Resources.ArgumentOutOfRange_NeedNonNegNum);
            }
            if (s.Length - charIndex < charCount)
            {
                throw new ArgumentOutOfRangeException("s", MDCFR.Properties.Resources.ArgumentOutOfRange_IndexCount);
            }
            if (byteIndex < 0 || byteIndex > bytes.Length)
            {
                throw new ArgumentOutOfRangeException("byteIndex", MDCFR.Properties.Resources.ArgumentOutOfRange_IndexMustBeLessOrEqual);
            }
            int byteCount = bytes.Length - byteIndex;
            if (bytes.Length == 0)
            {
                bytes = new byte[1];
            }
            fixed (char* ptr = s)
            {
                fixed (byte* ptr2 = &bytes[0])
                {
                    return GetBytes(ptr + charIndex, charCount, ptr2 + byteIndex, byteCount, null);
                }
            }
        }

        public unsafe override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            if (chars == null)
            {
                throw new ArgumentNullException("chars");
            }
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }
            if (charIndex < 0 || charCount < 0)
            {
                throw new ArgumentOutOfRangeException((charIndex < 0) ? "charIndex" : "charCount",
                    MDCFR.Properties.Resources.ArgumentOutOfRange_NeedNonNegNum);
            }
            if (chars.Length - charIndex < charCount)
            {
                throw new ArgumentOutOfRangeException("chars", MDCFR.Properties.Resources.ArgumentOutOfRange_IndexCountBuffer);
            }
            if (byteIndex < 0 || byteIndex > bytes.Length)
            {
                throw new ArgumentOutOfRangeException("byteIndex", 
                    MDCFR.Properties.Resources.ArgumentOutOfRange_IndexMustBeLessOrEqual);
            }
            if (chars.Length == 0)
            {
                return 0;
            }
            int byteCount = bytes.Length - byteIndex;
            if (bytes.Length == 0)
            {
                bytes = new byte[1];
            }
            fixed (char* ptr = &chars[0])
            {
                fixed (byte* ptr2 = &bytes[0])
                {
                    return GetBytes(ptr + charIndex, charCount, ptr2 + byteIndex, byteCount, null);
                }
            }
        }

        public unsafe override int GetBytes(char* chars, int charCount, byte* bytes, int byteCount)
        {
            if (chars == null)
            {
                throw new ArgumentNullException("chars");
            }
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }
            if (charCount < 0 || byteCount < 0)
            {
                throw new ArgumentOutOfRangeException((charCount < 0) ? "charCount" : "byteCount", MDCFR.Properties.Resources.ArgumentOutOfRange_NeedNonNegNum);
            }
            return GetBytes(chars, charCount, bytes, byteCount, null);
        }

        public unsafe override int GetCharCount(byte[] bytes, int index, int count)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }
            if (index < 0 || count < 0)
            {
                throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", MDCFR.Properties.Resources.ArgumentOutOfRange_NeedNonNegNum);
            }
            if (bytes.Length - index < count)
            {
                throw new ArgumentOutOfRangeException("bytes", MDCFR.Properties.Resources.ArgumentOutOfRange_IndexCountBuffer);
            }
            if (bytes.Length == 0)
            {
                return 0;
            }
            fixed (byte* ptr = &bytes[0])
            {
                return GetCharCount(ptr + index, count, null);
            }
        }

        public unsafe override int GetCharCount(byte* bytes, int count)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", MDCFR.Properties.Resources.ArgumentOutOfRange_NeedNonNegNum);
            }
            return GetCharCount(bytes, count, null);
        }

        public unsafe override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }
            if (chars == null)
            {
                throw new ArgumentNullException("chars");
            }
            if (byteIndex < 0 || byteCount < 0)
            {
                throw new ArgumentOutOfRangeException((byteIndex < 0) ? "byteIndex" : "byteCount", MDCFR.Properties.Resources.ArgumentOutOfRange_NeedNonNegNum);
            }
            if (bytes.Length - byteIndex < byteCount)
            {
                throw new ArgumentOutOfRangeException("bytes", MDCFR.Properties.Resources.ArgumentOutOfRange_IndexCountBuffer);
            }
            if (charIndex < 0 || charIndex > chars.Length)
            {
                throw new ArgumentOutOfRangeException("charIndex", MDCFR.Properties.Resources.ArgumentOutOfRange_IndexMustBeLessOrEqual);
            }
            if (bytes.Length == 0)
            {
                return 0;
            }
            int charCount = chars.Length - charIndex;
            if (chars.Length == 0)
            {
                chars = new char[1];
            }
            fixed (byte* ptr = &bytes[0])
            {
                fixed (char* ptr2 = &chars[0])
                {
                    return GetChars(ptr + byteIndex, byteCount, ptr2 + charIndex, charCount, null);
                }
            }
        }

        public unsafe override int GetChars(byte* bytes, int byteCount, char* chars, int charCount)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }
            if (chars == null)
            {
                throw new ArgumentNullException("chars");
            }
            if (charCount < 0 || byteCount < 0)
            {
                throw new ArgumentOutOfRangeException((charCount < 0) ? "charCount" : "byteCount", MDCFR.Properties.Resources.ArgumentOutOfRange_NeedNonNegNum);
            }
            return GetChars(bytes, byteCount, chars, charCount, null);
        }

        public unsafe override string GetString(byte[] bytes, int index, int count)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }
            if (index < 0 || count < 0)
            {
                throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", MDCFR.Properties.Resources.ArgumentOutOfRange_NeedNonNegNum);
            }
            if (bytes.Length - index < count)
            {
                throw new ArgumentOutOfRangeException("bytes", MDCFR.Properties.Resources.ArgumentOutOfRange_IndexCountBuffer);
            }
            if (bytes.Length == 0)
            {
                return string.Empty;
            }
            fixed (byte* ptr = &bytes[0])
            {
                return GetString(ptr + index, count);
            }
        }

        public override Decoder GetDecoder()
        {
            return new System.Text.DecoderNLS(this);
        }

        public override Encoder GetEncoder()
        {
            return new System.Text.EncoderNLS(this);
        }

        internal void ThrowBytesOverflow(System.Text.EncoderNLS encoder, bool nothingEncoded)
        {
            if ((encoder?.m_throwOnOverflow ?? true) || nothingEncoded)
            {
                if (encoder != null && encoder.InternalHasFallbackBuffer)
                {
                    encoder.FallbackBuffer.Reset();
                }
                ThrowBytesOverflow();
            }
            encoder.ClearMustFlush();
        }

        internal void ThrowCharsOverflow(System.Text.DecoderNLS decoder, bool nothingDecoded)
        {
            if ((decoder?.m_throwOnOverflow ?? true) || nothingDecoded)
            {
                if (decoder != null && decoder.InternalHasFallbackBuffer)
                {
                    decoder.FallbackBuffer.Reset();
                }
                ThrowCharsOverflow();
            }
            decoder.ClearMustFlush();
        }

        [DoesNotReturn]
        internal void ThrowBytesOverflow()
        {
            throw new ArgumentException(
                System.SR.Format(MDCFR.Properties.Resources.Argument_EncodingConversionOverflowBytes, EncodingName, base.EncoderFallback.GetType()), "bytes");
        }

        [DoesNotReturn]
        internal void ThrowCharsOverflow()
        {
            throw new ArgumentException(
                System.SR.Format(
                    MDCFR.Properties.Resources.Argument_EncodingConversionOverflowChars, EncodingName, base.DecoderFallback.GetType()), "chars");
        }

        internal static string GetLocalizedEncodingNameResource(int codePage)
        {
            return codePage switch
            {
                37 => MDCFR.Properties.Resources.Globalization_cp_37,
                437 => MDCFR.Properties.Resources.Globalization_cp_437,
                500 => MDCFR.Properties.Resources.Globalization_cp_500,
                708 => MDCFR.Properties.Resources.Globalization_cp_708,
                720 => MDCFR.Properties.Resources.Globalization_cp_720,
                737 => MDCFR.Properties.Resources.Globalization_cp_737,
                775 => MDCFR.Properties.Resources.Globalization_cp_775,
                850 => MDCFR.Properties.Resources.Globalization_cp_850,
                852 => MDCFR.Properties.Resources.Globalization_cp_852,
                855 => MDCFR.Properties.Resources.Globalization_cp_855,
                857 => MDCFR.Properties.Resources.Globalization_cp_857,
                858 => MDCFR.Properties.Resources.Globalization_cp_858,
                860 => MDCFR.Properties.Resources.Globalization_cp_860,
                861 => MDCFR.Properties.Resources.Globalization_cp_861,
                862 => MDCFR.Properties.Resources.Globalization_cp_862,
                863 => MDCFR.Properties.Resources.Globalization_cp_863,
                864 => MDCFR.Properties.Resources.Globalization_cp_864,
                865 => MDCFR.Properties.Resources.Globalization_cp_865,
                866 => MDCFR.Properties.Resources.Globalization_cp_866,
                869 => MDCFR.Properties.Resources.Globalization_cp_869,
                870 => MDCFR.Properties.Resources.Globalization_cp_870,
                874 => MDCFR.Properties.Resources.Globalization_cp_874,
                875 => MDCFR.Properties.Resources.Globalization_cp_875,
                932 => MDCFR.Properties.Resources.Globalization_cp_932,
                936 => MDCFR.Properties.Resources.Globalization_cp_936,
                949 => MDCFR.Properties.Resources.Globalization_cp_949,
                950 => MDCFR.Properties.Resources.Globalization_cp_950,
                1026 => MDCFR.Properties.Resources.Globalization_cp_1026,
                1047 => MDCFR.Properties.Resources.Globalization_cp_1047,
                1140 => MDCFR.Properties.Resources.Globalization_cp_1140,
                1141 => MDCFR.Properties.Resources.Globalization_cp_1141,
                1142 => MDCFR.Properties.Resources.Globalization_cp_1142,
                1143 => MDCFR.Properties.Resources.Globalization_cp_1143,
                1144 => MDCFR.Properties.Resources.Globalization_cp_1144,
                1145 => MDCFR.Properties.Resources.Globalization_cp_1145,
                1146 => MDCFR.Properties.Resources.Globalization_cp_1146,
                1147 => MDCFR.Properties.Resources.Globalization_cp_1147,
                1148 => MDCFR.Properties.Resources.Globalization_cp_1148,
                1149 => MDCFR.Properties.Resources.Globalization_cp_1149,
                1250 => MDCFR.Properties.Resources.Globalization_cp_1250,
                1251 => MDCFR.Properties.Resources.Globalization_cp_1251,
                1252 => MDCFR.Properties.Resources.Globalization_cp_1252,
                1253 => MDCFR.Properties.Resources.Globalization_cp_1253,
                1254 => MDCFR.Properties.Resources.Globalization_cp_1254,
                1255 => MDCFR.Properties.Resources.Globalization_cp_1255,
                1256 => MDCFR.Properties.Resources.Globalization_cp_1256,
                1257 => MDCFR.Properties.Resources.Globalization_cp_1257,
                1258 => MDCFR.Properties.Resources.Globalization_cp_1258,
                1361 => MDCFR.Properties.Resources.Globalization_cp_1361,
                10000 => MDCFR.Properties.Resources.Globalization_cp_10000,
                10001 => MDCFR.Properties.Resources.Globalization_cp_10001,
                10002 => MDCFR.Properties.Resources.Globalization_cp_10002,
                10003 => MDCFR.Properties.Resources.Globalization_cp_10003,
                10004 => MDCFR.Properties.Resources.Globalization_cp_10004,
                10005 => MDCFR.Properties.Resources.Globalization_cp_10005,
                10006 => MDCFR.Properties.Resources.Globalization_cp_10006,
                10007 => MDCFR.Properties.Resources.Globalization_cp_10007,
                10008 => MDCFR.Properties.Resources.Globalization_cp_10008,
                10010 => MDCFR.Properties.Resources.Globalization_cp_10010,
                10017 => MDCFR.Properties.Resources.Globalization_cp_10017,
                10021 => MDCFR.Properties.Resources.Globalization_cp_10021,
                10029 => MDCFR.Properties.Resources.Globalization_cp_10029,
                10079 => MDCFR.Properties.Resources.Globalization_cp_10079,
                10081 => MDCFR.Properties.Resources.Globalization_cp_10081,
                10082 => MDCFR.Properties.Resources.Globalization_cp_10082,
                20000 => MDCFR.Properties.Resources.Globalization_cp_20000,
                20001 => MDCFR.Properties.Resources.Globalization_cp_20001,
                20002 => MDCFR.Properties.Resources.Globalization_cp_20002,
                20003 => MDCFR.Properties.Resources.Globalization_cp_20003,
                20004 => MDCFR.Properties.Resources.Globalization_cp_20004,
                20005 => MDCFR.Properties.Resources.Globalization_cp_20005,
                20105 => MDCFR.Properties.Resources.Globalization_cp_20105,
                20106 => MDCFR.Properties.Resources.Globalization_cp_20106,
                20107 => MDCFR.Properties.Resources.Globalization_cp_20107,
                20108 => MDCFR.Properties.Resources.Globalization_cp_20108,
                20261 => MDCFR.Properties.Resources.Globalization_cp_20261,
                20269 => MDCFR.Properties.Resources.Globalization_cp_20269,
                20273 => MDCFR.Properties.Resources.Globalization_cp_20273,
                20277 => MDCFR.Properties.Resources.Globalization_cp_20277,
                20278 => MDCFR.Properties.Resources.Globalization_cp_20278,
                20280 => MDCFR.Properties.Resources.Globalization_cp_20280,
                20284 => MDCFR.Properties.Resources.Globalization_cp_20284,
                20285 => MDCFR.Properties.Resources.Globalization_cp_20285,
                20290 => MDCFR.Properties.Resources.Globalization_cp_20290,
                20297 => MDCFR.Properties.Resources.Globalization_cp_20297,
                20420 => MDCFR.Properties.Resources.Globalization_cp_20420,
                20423 => MDCFR.Properties.Resources.Globalization_cp_20423,
                20424 => MDCFR.Properties.Resources.Globalization_cp_20424,
                20833 => MDCFR.Properties.Resources.Globalization_cp_20833,
                20838 => MDCFR.Properties.Resources.Globalization_cp_20838,
                20866 => MDCFR.Properties.Resources.Globalization_cp_20866,
                20871 => MDCFR.Properties.Resources.Globalization_cp_20871,
                20880 => MDCFR.Properties.Resources.Globalization_cp_20880,
                20905 => MDCFR.Properties.Resources.Globalization_cp_20905,
                20924 => MDCFR.Properties.Resources.Globalization_cp_20924,
                20932 => MDCFR.Properties.Resources.Globalization_cp_20932,
                20936 => MDCFR.Properties.Resources.Globalization_cp_20936,
                20949 => MDCFR.Properties.Resources.Globalization_cp_20949,
                21025 => MDCFR.Properties.Resources.Globalization_cp_21025,
                21027 => MDCFR.Properties.Resources.Globalization_cp_21027,
                21866 => MDCFR.Properties.Resources.Globalization_cp_21866,
                28592 => MDCFR.Properties.Resources.Globalization_cp_28592,
                28593 => MDCFR.Properties.Resources.Globalization_cp_28593,
                28594 => MDCFR.Properties.Resources.Globalization_cp_28594,
                28595 => MDCFR.Properties.Resources.Globalization_cp_28595,
                28596 => MDCFR.Properties.Resources.Globalization_cp_28596,
                28597 => MDCFR.Properties.Resources.Globalization_cp_28597,
                28598 => MDCFR.Properties.Resources.Globalization_cp_28598,
                28599 => MDCFR.Properties.Resources.Globalization_cp_28599,
                28603 => MDCFR.Properties.Resources.Globalization_cp_28603,
                28605 => MDCFR.Properties.Resources.Globalization_cp_28605,
                29001 => MDCFR.Properties.Resources.Globalization_cp_29001,
                38598 => MDCFR.Properties.Resources.Globalization_cp_38598,
                50000 => MDCFR.Properties.Resources.Globalization_cp_50000,
                50220 => MDCFR.Properties.Resources.Globalization_cp_50220,
                50221 => MDCFR.Properties.Resources.Globalization_cp_50221,
                50222 => MDCFR.Properties.Resources.Globalization_cp_50222,
                50225 => MDCFR.Properties.Resources.Globalization_cp_50225,
                50227 => MDCFR.Properties.Resources.Globalization_cp_50227,
                50229 => MDCFR.Properties.Resources.Globalization_cp_50229,
                50930 => MDCFR.Properties.Resources.Globalization_cp_50930,
                50931 => MDCFR.Properties.Resources.Globalization_cp_50931,
                50933 => MDCFR.Properties.Resources.Globalization_cp_50933,
                50935 => MDCFR.Properties.Resources.Globalization_cp_50935,
                50937 => MDCFR.Properties.Resources.Globalization_cp_50937,
                50939 => MDCFR.Properties.Resources.Globalization_cp_50939,
                51932 => MDCFR.Properties.Resources.Globalization_cp_51932,
                51936 => MDCFR.Properties.Resources.Globalization_cp_51936,
                51949 => MDCFR.Properties.Resources.Globalization_cp_51949,
                52936 => MDCFR.Properties.Resources.Globalization_cp_52936,
                54936 => MDCFR.Properties.Resources.Globalization_cp_54936,
                57002 => MDCFR.Properties.Resources.Globalization_cp_57002,
                57003 => MDCFR.Properties.Resources.Globalization_cp_57003,
                57004 => MDCFR.Properties.Resources.Globalization_cp_57004,
                57005 => MDCFR.Properties.Resources.Globalization_cp_57005,
                57006 => MDCFR.Properties.Resources.Globalization_cp_57006,
                57007 => MDCFR.Properties.Resources.Globalization_cp_57007,
                57008 => MDCFR.Properties.Resources.Globalization_cp_57008,
                57009 => MDCFR.Properties.Resources.Globalization_cp_57009,
                57010 => MDCFR.Properties.Resources.Globalization_cp_57010,
                57011 => MDCFR.Properties.Resources.Globalization_cp_57011,
                _ => null,
            };
        }
    }

    internal static class EncodingTable
    {
        private static readonly Dictionary<string, int> s_nameToCodePageCache = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        private static readonly Dictionary<int, string> s_codePageToWebNameCache = new Dictionary<int, string>();

        private static readonly Dictionary<int, string> s_codePageToEnglishNameCache = new Dictionary<int, string>();

        private static readonly ReaderWriterLockSlim s_cacheLock = new ReaderWriterLockSlim();

        private const string s_encodingNames = "437arabicasmo-708big5big5-hkscsccsid00858ccsid00924ccsid01140ccsid01141ccsid01142ccsid01143ccsid01144ccsid01145ccsid01146ccsid01147ccsid01148ccsid01149chinesecn-big5cn-gbcp00858cp00924cp01140cp01141cp01142cp01143cp01144cp01145cp01146cp01147cp01148cp01149cp037cp1025cp1026cp1252cp1256cp273cp278cp280cp284cp285cp290cp297cp420cp423cp424cp437cp500cp50227cp850cp852cp855cp857cp858cp860cp861cp862cp863cp864cp865cp866cp869cp870cp871cp875cp880cp905csbig5cseuckrcseucpkdfmtjapanesecsgb2312csgb231280csibm037csibm1026csibm273csibm277csibm278csibm280csibm284csibm285csibm290csibm297csibm420csibm423csibm424csibm500csibm870csibm871csibm880csibm905csibmthaicsiso2022jpcsiso2022krcsiso58gb231280csisolatin2csisolatin3csisolatin4csisolatin5csisolatin9csisolatinarabiccsisolatincyrilliccsisolatingreekcsisolatinhebrewcskoi8rcsksc56011987cspc8codepage437csshiftjiscswindows31jcyrillicdin_66003dos-720dos-862dos-874ebcdic-cp-ar1ebcdic-cp-beebcdic-cp-caebcdic-cp-chebcdic-cp-dkebcdic-cp-esebcdic-cp-fiebcdic-cp-frebcdic-cp-gbebcdic-cp-grebcdic-cp-heebcdic-cp-isebcdic-cp-itebcdic-cp-nlebcdic-cp-noebcdic-cp-roeceebcdic-cp-seebcdic-cp-trebcdic-cp-usebcdic-cp-wtebcdic-cp-yuebcdic-cyrillicebcdic-de-273+euroebcdic-dk-277+euroebcdic-es-284+euroebcdic-fi-278+euroebcdic-fr-297+euroebcdic-gb-285+euroebcdic-international-500+euroebcdic-is-871+euroebcdic-it-280+euroebcdic-jp-kanaebcdic-latin9--euroebcdic-no-277+euroebcdic-se-278+euroebcdic-us-37+euroecma-114ecma-118elot_928euc-cneuc-jpeuc-krextended_unix_code_packed_format_for_japanesegb18030gb2312gb2312-80gb231280gb_2312-80gbkgermangreekgreek8hebrewhz-gb-2312ibm-thaiibm00858ibm00924ibm01047ibm01140ibm01141ibm01142ibm01143ibm01144ibm01145ibm01146ibm01147ibm01148ibm01149ibm037ibm1026ibm273ibm277ibm278ibm280ibm284ibm285ibm290ibm297ibm420ibm423ibm424ibm437ibm500ibm737ibm775ibm850ibm852ibm855ibm857ibm860ibm861ibm862ibm863ibm864ibm865ibm866ibm869ibm870ibm871ibm880ibm905irviso-2022-jpiso-2022-jpeuciso-2022-kriso-2022-kr-7iso-2022-kr-7bitiso-2022-kr-8iso-2022-kr-8bitiso-8859-11iso-8859-13iso-8859-15iso-8859-2iso-8859-3iso-8859-4iso-8859-5iso-8859-6iso-8859-7iso-8859-8iso-8859-8 visualiso-8859-8-iiso-8859-9iso-ir-101iso-ir-109iso-ir-110iso-ir-126iso-ir-127iso-ir-138iso-ir-144iso-ir-148iso-ir-149iso-ir-58iso8859-2iso_8859-15iso_8859-2iso_8859-2:1987iso_8859-3iso_8859-3:1988iso_8859-4iso_8859-4:1988iso_8859-5iso_8859-5:1988iso_8859-6iso_8859-6:1987iso_8859-7iso_8859-7:1987iso_8859-8iso_8859-8:1988iso_8859-9iso_8859-9:1989johabkoikoi8koi8-rkoi8-rukoi8-ukoi8rkoreanks-c-5601ks-c5601ks_c_5601ks_c_5601-1987ks_c_5601-1989ks_c_5601_1987ksc5601ksc_5601l2l3l4l5l9latin2latin3latin4latin5latin9logicalmacintoshms_kanjinorwegianns_4551-1pc-multilingual-850+eurosen_850200_bshift-jisshift_jissjisswedishtis-620visualwindows-1250windows-1251windows-1252windows-1253windows-1254windows-1255windows-1256windows-1257windows-1258windows-874x-ansix-chinese-cnsx-chinese-etenx-cp1250x-cp1251x-cp20001x-cp20003x-cp20004x-cp20005x-cp20261x-cp20269x-cp20936x-cp20949x-cp50227x-ebcdic-koreanextendedx-eucx-euc-cnx-euc-jpx-europax-ia5x-ia5-germanx-ia5-norwegianx-ia5-swedishx-iscii-asx-iscii-bex-iscii-dex-iscii-gux-iscii-kax-iscii-max-iscii-orx-iscii-pax-iscii-tax-iscii-tex-mac-arabicx-mac-cex-mac-chinesesimpx-mac-chinesetradx-mac-croatianx-mac-cyrillicx-mac-greekx-mac-hebrewx-mac-icelandicx-mac-japanesex-mac-koreanx-mac-romanianx-mac-thaix-mac-turkishx-mac-ukrainianx-ms-cp932x-sjisx-x-big5";

        private static readonly int[] s_encodingNameIndices = new int[365]
        {
            0, 3, 9, 17, 21, 31, 41, 51, 61, 71,
            81, 91, 101, 111, 121, 131, 141, 151, 158, 165,
            170, 177, 184, 191, 198, 205, 212, 219, 226, 233,
            240, 247, 254, 259, 265, 271, 277, 283, 288, 293,
            298, 303, 308, 313, 318, 323, 328, 333, 338, 343,
            350, 355, 360, 365, 370, 375, 380, 385, 390, 395,
            400, 405, 410, 415, 420, 425, 430, 435, 440, 446,
            453, 472, 480, 490, 498, 507, 515, 523, 531, 539,
            547, 555, 563, 571, 579, 587, 595, 603, 611, 619,
            627, 635, 644, 655, 666, 681, 692, 703, 714, 725,
            736, 752, 770, 785, 801, 808, 821, 837, 847, 859,
            867, 876, 883, 890, 897, 910, 922, 934, 946, 958,
            970, 982, 994, 1006, 1018, 1030, 1042, 1054, 1066, 1078,
            1093, 1105, 1117, 1129, 1141, 1153, 1168, 1186, 1204, 1222,
            1240, 1258, 1276, 1305, 1323, 1341, 1355, 1374, 1392, 1410,
            1427, 1435, 1443, 1451, 1457, 1463, 1469, 1514, 1521, 1527,
            1536, 1544, 1554, 1557, 1563, 1568, 1574, 1580, 1590, 1598,
            1606, 1614, 1622, 1630, 1638, 1646, 1654, 1662, 1670, 1678,
            1686, 1694, 1702, 1708, 1715, 1721, 1727, 1733, 1739, 1745,
            1751, 1757, 1763, 1769, 1775, 1781, 1787, 1793, 1799, 1805,
            1811, 1817, 1823, 1829, 1835, 1841, 1847, 1853, 1859, 1865,
            1871, 1877, 1883, 1889, 1895, 1901, 1904, 1915, 1929, 1940,
            1953, 1969, 1982, 1998, 2009, 2020, 2031, 2041, 2051, 2061,
            2071, 2081, 2091, 2101, 2118, 2130, 2140, 2150, 2160, 2170,
            2180, 2190, 2200, 2210, 2220, 2230, 2239, 2248, 2259, 2269,
            2284, 2294, 2309, 2319, 2334, 2344, 2359, 2369, 2384, 2394,
            2409, 2419, 2434, 2444, 2459, 2464, 2467, 2471, 2477, 2484,
            2490, 2495, 2501, 2510, 2518, 2527, 2541, 2555, 2569, 2576,
            2584, 2586, 2588, 2590, 2592, 2594, 2600, 2606, 2612, 2618,
            2624, 2631, 2640, 2648, 2657, 2666, 2690, 2702, 2711, 2720,
            2724, 2731, 2738, 2744, 2756, 2768, 2780, 2792, 2804, 2816,
            2828, 2840, 2852, 2863, 2869, 2882, 2896, 2904, 2912, 2921,
            2930, 2939, 2948, 2957, 2966, 2975, 2984, 2993, 3016, 3021,
            3029, 3037, 3045, 3050, 3062, 3077, 3090, 3100, 3110, 3120,
            3130, 3140, 3150, 3160, 3170, 3180, 3190, 3202, 3210, 3227,
            3244, 3258, 3272, 3283, 3295, 3310, 3324, 3336, 3350, 3360,
            3373, 3388, 3398, 3404, 3412
        };

        private static readonly ushort[] s_codePagesByName = new ushort[364]
        {
            437, 28596, 708, 950, 950, 858, 20924, 1140, 1141, 1142,
            1143, 1144, 1145, 1146, 1147, 1148, 1149, 936, 950, 936,
            858, 20924, 1140, 1141, 1142, 1143, 1144, 1145, 1146, 1147,
            1148, 1149, 37, 21025, 1026, 1252, 1256, 20273, 20278, 20280,
            20284, 20285, 20290, 20297, 20420, 20423, 20424, 437, 500, 50227,
            850, 852, 855, 857, 858, 860, 861, 862, 863, 864,
            865, 866, 869, 870, 20871, 875, 20880, 20905, 950, 51949,
            51932, 936, 936, 37, 1026, 20273, 20277, 20278, 20280, 20284,
            20285, 20290, 20297, 20420, 20423, 20424, 500, 870, 20871, 20880,
            20905, 20838, 50221, 50225, 936, 28592, 28593, 28594, 28599, 28605,
            28596, 28595, 28597, 28598, 20866, 949, 437, 932, 932, 28595,
            20106, 720, 862, 874, 20420, 500, 37, 500, 20277, 20284,
            20278, 20297, 20285, 20423, 20424, 20871, 20280, 37, 20277, 870,
            20278, 20905, 37, 37, 870, 20880, 1141, 1142, 1145, 1143,
            1147, 1146, 1148, 1149, 1144, 20290, 20924, 1142, 1143, 1140,
            28596, 28597, 28597, 51936, 51932, 51949, 51932, 54936, 936, 936,
            936, 936, 936, 20106, 28597, 28597, 28598, 52936, 20838, 858,
            20924, 1047, 1140, 1141, 1142, 1143, 1144, 1145, 1146, 1147,
            1148, 1149, 37, 1026, 20273, 20277, 20278, 20280, 20284, 20285,
            20290, 20297, 20420, 20423, 20424, 437, 500, 737, 775, 850,
            852, 855, 857, 860, 861, 862, 863, 864, 865, 866,
            869, 870, 20871, 20880, 20905, 20105, 50220, 51932, 50225, 50225,
            50225, 51949, 51949, 874, 28603, 28605, 28592, 28593, 28594, 28595,
            28596, 28597, 28598, 28598, 38598, 28599, 28592, 28593, 28594, 28597,
            28596, 28598, 28595, 28599, 949, 936, 28592, 28605, 28592, 28592,
            28593, 28593, 28594, 28594, 28595, 28595, 28596, 28596, 28597, 28597,
            28598, 28598, 28599, 28599, 1361, 20866, 20866, 20866, 21866, 21866,
            20866, 949, 949, 949, 949, 949, 949, 949, 949, 949,
            28592, 28593, 28594, 28599, 28605, 28592, 28593, 28594, 28599, 28605,
            28598, 10000, 932, 20108, 20108, 858, 20107, 932, 932, 932,
            20107, 874, 28598, 1250, 1251, 1252, 1253, 1254, 1255, 1256,
            1257, 1258, 874, 1252, 20000, 20002, 1250, 1251, 20001, 20003,
            20004, 20005, 20261, 20269, 20936, 20949, 50227, 20833, 51932, 51936,
            51932, 29001, 20105, 20106, 20108, 20107, 57006, 57003, 57002, 57010,
            57008, 57009, 57007, 57011, 57004, 57005, 10004, 10029, 10008, 10002,
            10082, 10007, 10006, 10005, 10079, 10001, 10003, 10010, 10021, 10081,
            10017, 932, 932, 950
        };

        private static readonly ushort[] s_mappedCodePages = new ushort[132]
        {
            37, 437, 500, 708, 720, 737, 775, 850, 852, 855,
            857, 858, 860, 861, 862, 863, 864, 865, 866, 869,
            870, 874, 875, 932, 936, 949, 950, 1026, 1047, 1140,
            1141, 1142, 1143, 1144, 1145, 1146, 1147, 1148, 1149, 1250,
            1251, 1252, 1253, 1254, 1255, 1256, 1257, 1258, 1361, 10000,
            10001, 10002, 10003, 10004, 10005, 10006, 10007, 10008, 10010, 10017,
            10021, 10029, 10079, 10081, 10082, 20000, 20001, 20002, 20003, 20004,
            20005, 20105, 20106, 20107, 20108, 20261, 20269, 20273, 20277, 20278,
            20280, 20284, 20285, 20290, 20297, 20420, 20423, 20424, 20833, 20838,
            20866, 20871, 20880, 20905, 20924, 20932, 20936, 20949, 21025, 21866,
            28592, 28593, 28594, 28595, 28596, 28597, 28598, 28599, 28603, 28605,
            29001, 38598, 50220, 50221, 50222, 50225, 50227, 51932, 51936, 51949,
            52936, 54936, 57002, 57003, 57004, 57005, 57006, 57007, 57008, 57009,
            57010, 57011
        };

        private const string s_webNames = "ibm037ibm437ibm500asmo-708dos-720ibm737ibm775ibm850ibm852ibm855ibm857ibm00858ibm860ibm861dos-862ibm863ibm864ibm865cp866ibm869ibm870windows-874cp875shift_jisgb2312ks_c_5601-1987big5ibm1026ibm01047ibm01140ibm01141ibm01142ibm01143ibm01144ibm01145ibm01146ibm01147ibm01148ibm01149windows-1250windows-1251windows-1252windows-1253windows-1254windows-1255windows-1256windows-1257windows-1258johabmacintoshx-mac-japanesex-mac-chinesetradx-mac-koreanx-mac-arabicx-mac-hebrewx-mac-greekx-mac-cyrillicx-mac-chinesesimpx-mac-romanianx-mac-ukrainianx-mac-thaix-mac-cex-mac-icelandicx-mac-turkishx-mac-croatianx-chinese-cnsx-cp20001x-chinese-etenx-cp20003x-cp20004x-cp20005x-ia5x-ia5-germanx-ia5-swedishx-ia5-norwegianx-cp20261x-cp20269ibm273ibm277ibm278ibm280ibm284ibm285ibm290ibm297ibm420ibm423ibm424x-ebcdic-koreanextendedibm-thaikoi8-ribm871ibm880ibm905ibm00924euc-jpx-cp20936x-cp20949cp1025koi8-uiso-8859-2iso-8859-3iso-8859-4iso-8859-5iso-8859-6iso-8859-7iso-8859-8iso-8859-9iso-8859-13iso-8859-15x-europaiso-8859-8-iiso-2022-jpcsiso2022jpiso-2022-jpiso-2022-krx-cp50227euc-jpeuc-cneuc-krhz-gb-2312gb18030x-iscii-dex-iscii-bex-iscii-tax-iscii-tex-iscii-asx-iscii-orx-iscii-kax-iscii-max-iscii-gux-iscii-pa";

        private static readonly int[] s_webNameIndices = new int[133]
        {
            0, 6, 12, 18, 26, 33, 39, 45, 51, 57,
            63, 69, 77, 83, 89, 96, 102, 108, 114, 119,
            125, 131, 142, 147, 156, 162, 176, 180, 187, 195,
            203, 211, 219, 227, 235, 243, 251, 259, 267, 275,
            287, 299, 311, 323, 335, 347, 359, 371, 383, 388,
            397, 411, 428, 440, 452, 464, 475, 489, 506, 520,
            535, 545, 553, 568, 581, 595, 608, 617, 631, 640,
            649, 658, 663, 675, 688, 703, 712, 721, 727, 733,
            739, 745, 751, 757, 763, 769, 775, 781, 787, 810,
            818, 824, 830, 836, 842, 850, 856, 865, 874, 880,
            886, 896, 906, 916, 926, 936, 946, 956, 966, 977,
            988, 996, 1008, 1019, 1030, 1041, 1052, 1061, 1067, 1073,
            1079, 1089, 1096, 1106, 1116, 1126, 1136, 1146, 1156, 1166,
            1176, 1186, 1196
        };

        private const string s_englishNames = "IBM EBCDIC (US-Canada)OEM United StatesIBM EBCDIC (International)Arabic (ASMO 708)Arabic (DOS)Greek (DOS)Baltic (DOS)Western European (DOS)Central European (DOS)OEM CyrillicTurkish (DOS)OEM Multilingual Latin IPortuguese (DOS)Icelandic (DOS)Hebrew (DOS)French Canadian (DOS)Arabic (864)Nordic (DOS)Cyrillic (DOS)Greek, Modern (DOS)IBM EBCDIC (Multilingual Latin-2)Thai (Windows)IBM EBCDIC (Greek Modern)Japanese (Shift-JIS)Chinese Simplified (GB2312)KoreanChinese Traditional (Big5)IBM EBCDIC (Turkish Latin-5)IBM Latin-1IBM EBCDIC (US-Canada-Euro)IBM EBCDIC (Germany-Euro)IBM EBCDIC (Denmark-Norway-Euro)IBM EBCDIC (Finland-Sweden-Euro)IBM EBCDIC (Italy-Euro)IBM EBCDIC (Spain-Euro)IBM EBCDIC (UK-Euro)IBM EBCDIC (France-Euro)IBM EBCDIC (International-Euro)IBM EBCDIC (Icelandic-Euro)Central European (Windows)Cyrillic (Windows)Western European (Windows)Greek (Windows)Turkish (Windows)Hebrew (Windows)Arabic (Windows)Baltic (Windows)Vietnamese (Windows)Korean (Johab)Western European (Mac)Japanese (Mac)Chinese Traditional (Mac)Korean (Mac)Arabic (Mac)Hebrew (Mac)Greek (Mac)Cyrillic (Mac)Chinese Simplified (Mac)Romanian (Mac)Ukrainian (Mac)Thai (Mac)Central European (Mac)Icelandic (Mac)Turkish (Mac)Croatian (Mac)Chinese Traditional (CNS)TCA TaiwanChinese Traditional (Eten)IBM5550 TaiwanTeleText TaiwanWang TaiwanWestern European (IA5)German (IA5)Swedish (IA5)Norwegian (IA5)T.61ISO-6937IBM EBCDIC (Germany)IBM EBCDIC (Denmark-Norway)IBM EBCDIC (Finland-Sweden)IBM EBCDIC (Italy)IBM EBCDIC (Spain)IBM EBCDIC (UK)IBM EBCDIC (Japanese katakana)IBM EBCDIC (France)IBM EBCDIC (Arabic)IBM EBCDIC (Greek)IBM EBCDIC (Hebrew)IBM EBCDIC (Korean Extended)IBM EBCDIC (Thai)Cyrillic (KOI8-R)IBM EBCDIC (Icelandic)IBM EBCDIC (Cyrillic Russian)IBM EBCDIC (Turkish)IBM Latin-1Japanese (JIS 0208-1990 and 0212-1990)Chinese Simplified (GB2312-80)Korean WansungIBM EBCDIC (Cyrillic Serbian-Bulgarian)Cyrillic (KOI8-U)Central European (ISO)Latin 3 (ISO)Baltic (ISO)Cyrillic (ISO)Arabic (ISO)Greek (ISO)Hebrew (ISO-Visual)Turkish (ISO)Estonian (ISO)Latin 9 (ISO)EuropaHebrew (ISO-Logical)Japanese (JIS)Japanese (JIS-Allow 1 byte Kana)Japanese (JIS-Allow 1 byte Kana - SO/SI)Korean (ISO)Chinese Simplified (ISO-2022)Japanese (EUC)Chinese Simplified (EUC)Korean (EUC)Chinese Simplified (HZ)Chinese Simplified (GB18030)ISCII DevanagariISCII BengaliISCII TamilISCII TeluguISCII AssameseISCII OriyaISCII KannadaISCII MalayalamISCII GujaratiISCII Punjabi";

        private static readonly int[] s_englishNameIndices = new int[133]
        {
            0, 22, 39, 65, 82, 94, 105, 117, 139, 161,
            173, 186, 210, 226, 241, 253, 274, 286, 298, 312,
            331, 364, 378, 403, 423, 450, 456, 482, 510, 521,
            548, 573, 605, 637, 660, 683, 703, 727, 758, 785,
            811, 829, 855, 870, 887, 903, 919, 935, 955, 969,
            991, 1005, 1030, 1042, 1054, 1066, 1077, 1091, 1115, 1129,
            1144, 1154, 1176, 1191, 1204, 1218, 1243, 1253, 1279, 1293,
            1308, 1319, 1341, 1353, 1366, 1381, 1385, 1393, 1413, 1440,
            1467, 1485, 1503, 1518, 1548, 1567, 1586, 1604, 1623, 1651,
            1668, 1685, 1707, 1736, 1756, 1767, 1805, 1835, 1849, 1888,
            1905, 1927, 1940, 1952, 1966, 1978, 1989, 2008, 2021, 2035,
            2048, 2054, 2074, 2088, 2120, 2160, 2172, 2201, 2215, 2239,
            2251, 2274, 2302, 2318, 2331, 2342, 2354, 2368, 2379, 2392,
            2407, 2421, 2434
        };

        internal static int GetCodePageFromName(string name)
        {
            if (name == null)
            {
                return 0;
            }
            s_cacheLock.EnterUpgradeableReadLock();
            try
            {
                if (s_nameToCodePageCache.TryGetValue(name, out var value))
                {
                    return value;
                }
                value = InternalGetCodePageFromName(name);
                if (value == 0)
                {
                    return 0;
                }
                s_cacheLock.EnterWriteLock();
                try
                {
                    if (s_nameToCodePageCache.TryGetValue(name, out var value2))
                    {
                        return value2;
                    }
                    s_nameToCodePageCache.Add(name, value);
                    return value;
                }
                finally
                {
                    s_cacheLock.ExitWriteLock();
                }
            }
            finally
            {
                s_cacheLock.ExitUpgradeableReadLock();
            }
        }

        private static int InternalGetCodePageFromName(string name)
        {
            int i = 0;
            int num = s_encodingNameIndices.Length - 2;
            name = name.ToLowerInvariant();
            while (num - i > 3)
            {
                int num2 = (num - i) / 2 + i;
                int num3 = CompareOrdinal(name, "437arabicasmo-708big5big5-hkscsccsid00858ccsid00924ccsid01140ccsid01141ccsid01142ccsid01143ccsid01144ccsid01145ccsid01146ccsid01147ccsid01148ccsid01149chinesecn-big5cn-gbcp00858cp00924cp01140cp01141cp01142cp01143cp01144cp01145cp01146cp01147cp01148cp01149cp037cp1025cp1026cp1252cp1256cp273cp278cp280cp284cp285cp290cp297cp420cp423cp424cp437cp500cp50227cp850cp852cp855cp857cp858cp860cp861cp862cp863cp864cp865cp866cp869cp870cp871cp875cp880cp905csbig5cseuckrcseucpkdfmtjapanesecsgb2312csgb231280csibm037csibm1026csibm273csibm277csibm278csibm280csibm284csibm285csibm290csibm297csibm420csibm423csibm424csibm500csibm870csibm871csibm880csibm905csibmthaicsiso2022jpcsiso2022krcsiso58gb231280csisolatin2csisolatin3csisolatin4csisolatin5csisolatin9csisolatinarabiccsisolatincyrilliccsisolatingreekcsisolatinhebrewcskoi8rcsksc56011987cspc8codepage437csshiftjiscswindows31jcyrillicdin_66003dos-720dos-862dos-874ebcdic-cp-ar1ebcdic-cp-beebcdic-cp-caebcdic-cp-chebcdic-cp-dkebcdic-cp-esebcdic-cp-fiebcdic-cp-frebcdic-cp-gbebcdic-cp-grebcdic-cp-heebcdic-cp-isebcdic-cp-itebcdic-cp-nlebcdic-cp-noebcdic-cp-roeceebcdic-cp-seebcdic-cp-trebcdic-cp-usebcdic-cp-wtebcdic-cp-yuebcdic-cyrillicebcdic-de-273+euroebcdic-dk-277+euroebcdic-es-284+euroebcdic-fi-278+euroebcdic-fr-297+euroebcdic-gb-285+euroebcdic-international-500+euroebcdic-is-871+euroebcdic-it-280+euroebcdic-jp-kanaebcdic-latin9--euroebcdic-no-277+euroebcdic-se-278+euroebcdic-us-37+euroecma-114ecma-118elot_928euc-cneuc-jpeuc-krextended_unix_code_packed_format_for_japanesegb18030gb2312gb2312-80gb231280gb_2312-80gbkgermangreekgreek8hebrewhz-gb-2312ibm-thaiibm00858ibm00924ibm01047ibm01140ibm01141ibm01142ibm01143ibm01144ibm01145ibm01146ibm01147ibm01148ibm01149ibm037ibm1026ibm273ibm277ibm278ibm280ibm284ibm285ibm290ibm297ibm420ibm423ibm424ibm437ibm500ibm737ibm775ibm850ibm852ibm855ibm857ibm860ibm861ibm862ibm863ibm864ibm865ibm866ibm869ibm870ibm871ibm880ibm905irviso-2022-jpiso-2022-jpeuciso-2022-kriso-2022-kr-7iso-2022-kr-7bitiso-2022-kr-8iso-2022-kr-8bitiso-8859-11iso-8859-13iso-8859-15iso-8859-2iso-8859-3iso-8859-4iso-8859-5iso-8859-6iso-8859-7iso-8859-8iso-8859-8 visualiso-8859-8-iiso-8859-9iso-ir-101iso-ir-109iso-ir-110iso-ir-126iso-ir-127iso-ir-138iso-ir-144iso-ir-148iso-ir-149iso-ir-58iso8859-2iso_8859-15iso_8859-2iso_8859-2:1987iso_8859-3iso_8859-3:1988iso_8859-4iso_8859-4:1988iso_8859-5iso_8859-5:1988iso_8859-6iso_8859-6:1987iso_8859-7iso_8859-7:1987iso_8859-8iso_8859-8:1988iso_8859-9iso_8859-9:1989johabkoikoi8koi8-rkoi8-rukoi8-ukoi8rkoreanks-c-5601ks-c5601ks_c_5601ks_c_5601-1987ks_c_5601-1989ks_c_5601_1987ksc5601ksc_5601l2l3l4l5l9latin2latin3latin4latin5latin9logicalmacintoshms_kanjinorwegianns_4551-1pc-multilingual-850+eurosen_850200_bshift-jisshift_jissjisswedishtis-620visualwindows-1250windows-1251windows-1252windows-1253windows-1254windows-1255windows-1256windows-1257windows-1258windows-874x-ansix-chinese-cnsx-chinese-etenx-cp1250x-cp1251x-cp20001x-cp20003x-cp20004x-cp20005x-cp20261x-cp20269x-cp20936x-cp20949x-cp50227x-ebcdic-koreanextendedx-eucx-euc-cnx-euc-jpx-europax-ia5x-ia5-germanx-ia5-norwegianx-ia5-swedishx-iscii-asx-iscii-bex-iscii-dex-iscii-gux-iscii-kax-iscii-max-iscii-orx-iscii-pax-iscii-tax-iscii-tex-mac-arabicx-mac-cex-mac-chinesesimpx-mac-chinesetradx-mac-croatianx-mac-cyrillicx-mac-greekx-mac-hebrewx-mac-icelandicx-mac-japanesex-mac-koreanx-mac-romanianx-mac-thaix-mac-turkishx-mac-ukrainianx-ms-cp932x-sjisx-x-big5", s_encodingNameIndices[num2], s_encodingNameIndices[num2 + 1] - s_encodingNameIndices[num2]);
                if (num3 == 0)
                {
                    return s_codePagesByName[num2];
                }
                if (num3 < 0)
                {
                    num = num2;
                }
                else
                {
                    i = num2;
                }
            }
            for (; i <= num; i++)
            {
                if (CompareOrdinal(name, "437arabicasmo-708big5big5-hkscsccsid00858ccsid00924ccsid01140ccsid01141ccsid01142ccsid01143ccsid01144ccsid01145ccsid01146ccsid01147ccsid01148ccsid01149chinesecn-big5cn-gbcp00858cp00924cp01140cp01141cp01142cp01143cp01144cp01145cp01146cp01147cp01148cp01149cp037cp1025cp1026cp1252cp1256cp273cp278cp280cp284cp285cp290cp297cp420cp423cp424cp437cp500cp50227cp850cp852cp855cp857cp858cp860cp861cp862cp863cp864cp865cp866cp869cp870cp871cp875cp880cp905csbig5cseuckrcseucpkdfmtjapanesecsgb2312csgb231280csibm037csibm1026csibm273csibm277csibm278csibm280csibm284csibm285csibm290csibm297csibm420csibm423csibm424csibm500csibm870csibm871csibm880csibm905csibmthaicsiso2022jpcsiso2022krcsiso58gb231280csisolatin2csisolatin3csisolatin4csisolatin5csisolatin9csisolatinarabiccsisolatincyrilliccsisolatingreekcsisolatinhebrewcskoi8rcsksc56011987cspc8codepage437csshiftjiscswindows31jcyrillicdin_66003dos-720dos-862dos-874ebcdic-cp-ar1ebcdic-cp-beebcdic-cp-caebcdic-cp-chebcdic-cp-dkebcdic-cp-esebcdic-cp-fiebcdic-cp-frebcdic-cp-gbebcdic-cp-grebcdic-cp-heebcdic-cp-isebcdic-cp-itebcdic-cp-nlebcdic-cp-noebcdic-cp-roeceebcdic-cp-seebcdic-cp-trebcdic-cp-usebcdic-cp-wtebcdic-cp-yuebcdic-cyrillicebcdic-de-273+euroebcdic-dk-277+euroebcdic-es-284+euroebcdic-fi-278+euroebcdic-fr-297+euroebcdic-gb-285+euroebcdic-international-500+euroebcdic-is-871+euroebcdic-it-280+euroebcdic-jp-kanaebcdic-latin9--euroebcdic-no-277+euroebcdic-se-278+euroebcdic-us-37+euroecma-114ecma-118elot_928euc-cneuc-jpeuc-krextended_unix_code_packed_format_for_japanesegb18030gb2312gb2312-80gb231280gb_2312-80gbkgermangreekgreek8hebrewhz-gb-2312ibm-thaiibm00858ibm00924ibm01047ibm01140ibm01141ibm01142ibm01143ibm01144ibm01145ibm01146ibm01147ibm01148ibm01149ibm037ibm1026ibm273ibm277ibm278ibm280ibm284ibm285ibm290ibm297ibm420ibm423ibm424ibm437ibm500ibm737ibm775ibm850ibm852ibm855ibm857ibm860ibm861ibm862ibm863ibm864ibm865ibm866ibm869ibm870ibm871ibm880ibm905irviso-2022-jpiso-2022-jpeuciso-2022-kriso-2022-kr-7iso-2022-kr-7bitiso-2022-kr-8iso-2022-kr-8bitiso-8859-11iso-8859-13iso-8859-15iso-8859-2iso-8859-3iso-8859-4iso-8859-5iso-8859-6iso-8859-7iso-8859-8iso-8859-8 visualiso-8859-8-iiso-8859-9iso-ir-101iso-ir-109iso-ir-110iso-ir-126iso-ir-127iso-ir-138iso-ir-144iso-ir-148iso-ir-149iso-ir-58iso8859-2iso_8859-15iso_8859-2iso_8859-2:1987iso_8859-3iso_8859-3:1988iso_8859-4iso_8859-4:1988iso_8859-5iso_8859-5:1988iso_8859-6iso_8859-6:1987iso_8859-7iso_8859-7:1987iso_8859-8iso_8859-8:1988iso_8859-9iso_8859-9:1989johabkoikoi8koi8-rkoi8-rukoi8-ukoi8rkoreanks-c-5601ks-c5601ks_c_5601ks_c_5601-1987ks_c_5601-1989ks_c_5601_1987ksc5601ksc_5601l2l3l4l5l9latin2latin3latin4latin5latin9logicalmacintoshms_kanjinorwegianns_4551-1pc-multilingual-850+eurosen_850200_bshift-jisshift_jissjisswedishtis-620visualwindows-1250windows-1251windows-1252windows-1253windows-1254windows-1255windows-1256windows-1257windows-1258windows-874x-ansix-chinese-cnsx-chinese-etenx-cp1250x-cp1251x-cp20001x-cp20003x-cp20004x-cp20005x-cp20261x-cp20269x-cp20936x-cp20949x-cp50227x-ebcdic-koreanextendedx-eucx-euc-cnx-euc-jpx-europax-ia5x-ia5-germanx-ia5-norwegianx-ia5-swedishx-iscii-asx-iscii-bex-iscii-dex-iscii-gux-iscii-kax-iscii-max-iscii-orx-iscii-pax-iscii-tax-iscii-tex-mac-arabicx-mac-cex-mac-chinesesimpx-mac-chinesetradx-mac-croatianx-mac-cyrillicx-mac-greekx-mac-hebrewx-mac-icelandicx-mac-japanesex-mac-koreanx-mac-romanianx-mac-thaix-mac-turkishx-mac-ukrainianx-ms-cp932x-sjisx-x-big5", s_encodingNameIndices[i], s_encodingNameIndices[i + 1] - s_encodingNameIndices[i]) == 0)
                {
                    return s_codePagesByName[i];
                }
            }
            return 0;
        }

        private static int CompareOrdinal(string s1, string s2, int index, int length)
        {
            int num = s1.Length;
            if (num > length)
            {
                num = length;
            }
            int i;
            for (i = 0; i < num && s1[i] == s2[index + i]; i++)
            {
            }
            if (i < num)
            {
                return s1[i] - s2[index + i];
            }
            return s1.Length - length;
        }

        internal static string GetWebNameFromCodePage(int codePage)
        {
            return GetNameFromCodePage(codePage, "ibm037ibm437ibm500asmo-708dos-720ibm737ibm775ibm850ibm852ibm855ibm857ibm00858ibm860ibm861dos-862ibm863ibm864ibm865cp866ibm869ibm870windows-874cp875shift_jisgb2312ks_c_5601-1987big5ibm1026ibm01047ibm01140ibm01141ibm01142ibm01143ibm01144ibm01145ibm01146ibm01147ibm01148ibm01149windows-1250windows-1251windows-1252windows-1253windows-1254windows-1255windows-1256windows-1257windows-1258johabmacintoshx-mac-japanesex-mac-chinesetradx-mac-koreanx-mac-arabicx-mac-hebrewx-mac-greekx-mac-cyrillicx-mac-chinesesimpx-mac-romanianx-mac-ukrainianx-mac-thaix-mac-cex-mac-icelandicx-mac-turkishx-mac-croatianx-chinese-cnsx-cp20001x-chinese-etenx-cp20003x-cp20004x-cp20005x-ia5x-ia5-germanx-ia5-swedishx-ia5-norwegianx-cp20261x-cp20269ibm273ibm277ibm278ibm280ibm284ibm285ibm290ibm297ibm420ibm423ibm424x-ebcdic-koreanextendedibm-thaikoi8-ribm871ibm880ibm905ibm00924euc-jpx-cp20936x-cp20949cp1025koi8-uiso-8859-2iso-8859-3iso-8859-4iso-8859-5iso-8859-6iso-8859-7iso-8859-8iso-8859-9iso-8859-13iso-8859-15x-europaiso-8859-8-iiso-2022-jpcsiso2022jpiso-2022-jpiso-2022-krx-cp50227euc-jpeuc-cneuc-krhz-gb-2312gb18030x-iscii-dex-iscii-bex-iscii-tax-iscii-tex-iscii-asx-iscii-orx-iscii-kax-iscii-max-iscii-gux-iscii-pa", s_webNameIndices, s_codePageToWebNameCache);
        }

        internal static string GetEnglishNameFromCodePage(int codePage)
        {
            return GetNameFromCodePage(codePage, "IBM EBCDIC (US-Canada)OEM United StatesIBM EBCDIC (International)Arabic (ASMO 708)Arabic (DOS)Greek (DOS)Baltic (DOS)Western European (DOS)Central European (DOS)OEM CyrillicTurkish (DOS)OEM Multilingual Latin IPortuguese (DOS)Icelandic (DOS)Hebrew (DOS)French Canadian (DOS)Arabic (864)Nordic (DOS)Cyrillic (DOS)Greek, Modern (DOS)IBM EBCDIC (Multilingual Latin-2)Thai (Windows)IBM EBCDIC (Greek Modern)Japanese (Shift-JIS)Chinese Simplified (GB2312)KoreanChinese Traditional (Big5)IBM EBCDIC (Turkish Latin-5)IBM Latin-1IBM EBCDIC (US-Canada-Euro)IBM EBCDIC (Germany-Euro)IBM EBCDIC (Denmark-Norway-Euro)IBM EBCDIC (Finland-Sweden-Euro)IBM EBCDIC (Italy-Euro)IBM EBCDIC (Spain-Euro)IBM EBCDIC (UK-Euro)IBM EBCDIC (France-Euro)IBM EBCDIC (International-Euro)IBM EBCDIC (Icelandic-Euro)Central European (Windows)Cyrillic (Windows)Western European (Windows)Greek (Windows)Turkish (Windows)Hebrew (Windows)Arabic (Windows)Baltic (Windows)Vietnamese (Windows)Korean (Johab)Western European (Mac)Japanese (Mac)Chinese Traditional (Mac)Korean (Mac)Arabic (Mac)Hebrew (Mac)Greek (Mac)Cyrillic (Mac)Chinese Simplified (Mac)Romanian (Mac)Ukrainian (Mac)Thai (Mac)Central European (Mac)Icelandic (Mac)Turkish (Mac)Croatian (Mac)Chinese Traditional (CNS)TCA TaiwanChinese Traditional (Eten)IBM5550 TaiwanTeleText TaiwanWang TaiwanWestern European (IA5)German (IA5)Swedish (IA5)Norwegian (IA5)T.61ISO-6937IBM EBCDIC (Germany)IBM EBCDIC (Denmark-Norway)IBM EBCDIC (Finland-Sweden)IBM EBCDIC (Italy)IBM EBCDIC (Spain)IBM EBCDIC (UK)IBM EBCDIC (Japanese katakana)IBM EBCDIC (France)IBM EBCDIC (Arabic)IBM EBCDIC (Greek)IBM EBCDIC (Hebrew)IBM EBCDIC (Korean Extended)IBM EBCDIC (Thai)Cyrillic (KOI8-R)IBM EBCDIC (Icelandic)IBM EBCDIC (Cyrillic Russian)IBM EBCDIC (Turkish)IBM Latin-1Japanese (JIS 0208-1990 and 0212-1990)Chinese Simplified (GB2312-80)Korean WansungIBM EBCDIC (Cyrillic Serbian-Bulgarian)Cyrillic (KOI8-U)Central European (ISO)Latin 3 (ISO)Baltic (ISO)Cyrillic (ISO)Arabic (ISO)Greek (ISO)Hebrew (ISO-Visual)Turkish (ISO)Estonian (ISO)Latin 9 (ISO)EuropaHebrew (ISO-Logical)Japanese (JIS)Japanese (JIS-Allow 1 byte Kana)Japanese (JIS-Allow 1 byte Kana - SO/SI)Korean (ISO)Chinese Simplified (ISO-2022)Japanese (EUC)Chinese Simplified (EUC)Korean (EUC)Chinese Simplified (HZ)Chinese Simplified (GB18030)ISCII DevanagariISCII BengaliISCII TamilISCII TeluguISCII AssameseISCII OriyaISCII KannadaISCII MalayalamISCII GujaratiISCII Punjabi", s_englishNameIndices, s_codePageToEnglishNameCache);
        }

        private static string GetNameFromCodePage(int codePage, string names, int[] indices, Dictionary<int, string> cache)
        {
            if ((uint)codePage > 65535u)
            {
                return null;
            }
            int num = Array.IndexOf(s_mappedCodePages, (ushort)codePage);
            if (num < 0)
            {
                return null;
            }
            s_cacheLock.EnterUpgradeableReadLock();
            try
            {
                if (cache.TryGetValue(codePage, out var value))
                {
                    return value;
                }
                value = names.Substring(indices[num], indices[num + 1] - indices[num]);
                s_cacheLock.EnterWriteLock();
                try
                {
                    if (cache.TryGetValue(codePage, out var value2))
                    {
                        return value2;
                    }
                    cache.Add(codePage, value);
                    return value;
                }
                finally
                {
                    s_cacheLock.ExitWriteLock();
                }
            }
            finally
            {
                s_cacheLock.ExitUpgradeableReadLock();
            }
        }
    }

    internal sealed class EUCJPEncoding : System.Text.DBCSCodePageEncoding
    {
        public EUCJPEncoding()
            : base(51932, 932)
        {
        }

        protected override bool CleanUpBytes(ref int bytes)
        {
            if (bytes >= 256)
            {
                if (bytes >= 64064 && bytes <= 64587)
                {
                    if (bytes >= 64064 && bytes <= 64091)
                    {
                        if (bytes <= 64073)
                        {
                            bytes -= 2897;
                        }
                        else if (bytes >= 64074 && bytes <= 64083)
                        {
                            bytes -= 29430;
                        }
                        else if (bytes >= 64084 && bytes <= 64087)
                        {
                            bytes -= 2907;
                        }
                        else if (bytes == 64088)
                        {
                            bytes = 34698;
                        }
                        else if (bytes == 64089)
                        {
                            bytes = 34690;
                        }
                        else if (bytes == 64090)
                        {
                            bytes = 34692;
                        }
                        else if (bytes == 64091)
                        {
                            bytes = 34714;
                        }
                    }
                    else if (bytes >= 64092 && bytes <= 64587)
                    {
                        byte b = (byte)bytes;
                        if (b < 92)
                        {
                            bytes -= 3423;
                        }
                        else if (b >= 128 && b <= 155)
                        {
                            bytes -= 3357;
                        }
                        else
                        {
                            bytes -= 3356;
                        }
                    }
                }
                byte b2 = (byte)(bytes >> 8);
                byte b3 = (byte)bytes;
                b2 = (byte)(b2 - ((b2 > 159) ? 177 : 113));
                b2 = (byte)((b2 << 1) + 1);
                if (b3 > 158)
                {
                    b3 = (byte)(b3 - 126);
                    b2 = (byte)(b2 + 1);
                }
                else
                {
                    if (b3 > 126)
                    {
                        b3 = (byte)(b3 - 1);
                    }
                    b3 = (byte)(b3 - 31);
                }
                bytes = (b2 << 8) | b3 | 0x8080;
                if ((bytes & 0xFF00) < 41216 || (bytes & 0xFF00) > 65024 || (bytes & 0xFF) < 161 || (bytes & 0xFF) > 254)
                {
                    return false;
                }
            }
            else
            {
                if (bytes >= 161 && bytes <= 223)
                {
                    bytes |= 36352;
                    return true;
                }
                if (bytes >= 129 && bytes != 160 && bytes != 255)
                {
                    return false;
                }
            }
            return true;
        }

        protected unsafe override void CleanUpEndBytes(char* chars)
        {
            for (int i = 161; i <= 254; i++)
            {
                chars[i] = '\ufffe';
            }
            chars[142] = '\ufffe';
        }
    }

    internal sealed class GB18030Encoding : System.Text.DBCSCodePageEncoding
    {
        internal sealed class GB18030Decoder : System.Text.DecoderNLS
        {
            internal short bLeftOver1 = -1;

            internal short bLeftOver2 = -1;

            internal short bLeftOver3 = -1;

            internal short bLeftOver4 = -1;

            internal override bool HasState => bLeftOver1 >= 0;

            internal GB18030Decoder(System.Text.EncodingNLS encoding)
                : base(encoding)
            {
            }

            public override void Reset()
            {
                bLeftOver1 = -1;
                bLeftOver2 = -1;
                bLeftOver3 = -1;
                bLeftOver4 = -1;
                m_fallbackBuffer?.Reset();
            }
        }

        private const int GBLast4ByteCode = 39419;

        internal unsafe char* map4BytesToUnicode = null;

        internal unsafe byte* mapUnicodeTo4BytesFlags = null;

        private const int GB18030 = 54936;

        private const int GBSurrogateOffset = 189000;

        private const int GBLastSurrogateOffset = 1237575;

        private readonly ushort[] _tableUnicodeToGBDiffs = new ushort[439]
        {
            32896, 36, 32769, 2, 32770, 7, 32770, 5, 32769, 31,
            32769, 8, 32770, 6, 32771, 1, 32770, 4, 32770, 3,
            32769, 1, 32770, 1, 32769, 4, 32769, 17, 32769, 7,
            32769, 15, 32769, 24, 32769, 3, 32769, 4, 32769, 29,
            32769, 98, 32769, 1, 32769, 1, 32769, 1, 32769, 1,
            32769, 1, 32769, 1, 32769, 1, 32769, 28, 43199, 87,
            32769, 15, 32769, 101, 32769, 1, 32771, 13, 32769, 183,
            32785, 1, 32775, 7, 32785, 1, 32775, 55, 32769, 14,
            32832, 1, 32769, 7102, 32769, 2, 32772, 1, 32770, 2,
            32770, 7, 32770, 9, 32769, 1, 32770, 1, 32769, 5,
            32769, 112, 41699, 86, 32769, 1, 32769, 3, 32769, 12,
            32769, 10, 32769, 62, 32780, 4, 32778, 22, 32772, 2,
            32772, 110, 32769, 6, 32769, 1, 32769, 3, 32769, 4,
            32769, 2, 32772, 2, 32769, 1, 32769, 1, 32773, 2,
            32769, 5, 32772, 5, 32769, 10, 32769, 3, 32769, 5,
            32769, 13, 32770, 2, 32772, 6, 32770, 37, 32769, 3,
            32769, 11, 32769, 25, 32769, 82, 32769, 333, 32778, 10,
            32808, 100, 32844, 4, 32804, 13, 32783, 3, 32771, 10,
            32770, 16, 32770, 8, 32770, 8, 32770, 3, 32769, 2,
            32770, 18, 32772, 31, 32770, 2, 32769, 54, 32769, 1,
            32769, 2110, 65104, 2, 65108, 3, 65111, 2, 65112, 65117,
            10, 65118, 15, 65131, 2, 65134, 3, 65137, 4, 65139,
            2, 65140, 65141, 3, 65145, 14, 65156, 293, 43402, 43403,
            43404, 43405, 43406, 43407, 43408, 43409, 43410, 43411, 43412, 43413,
            4, 32772, 1, 32787, 5, 32770, 2, 32777, 20, 43401,
            2, 32851, 7, 32772, 2, 32854, 5, 32771, 6, 32805,
            246, 32778, 7, 32769, 113, 32769, 234, 32770, 12, 32771,
            2, 32769, 34, 32769, 9, 32769, 2, 32770, 2, 32769,
            113, 65110, 43, 65109, 298, 65114, 111, 65116, 11, 65115,
            765, 65120, 85, 65119, 96, 65122, 65125, 14, 65123, 147,
            65124, 218, 65128, 287, 65129, 113, 65130, 885, 65135, 264,
            65136, 471, 65138, 116, 65144, 4, 65143, 43, 65146, 248,
            65147, 373, 65149, 20, 65148, 193, 65152, 5, 65153, 82,
            65154, 16, 65155, 441, 65157, 50, 65158, 2, 65159, 4,
            65160, 65161, 1, 65162, 65163, 20, 65165, 3, 65164, 22,
            65167, 65166, 703, 65174, 39, 65171, 65172, 65173, 65175, 65170,
            111, 65176, 65177, 65178, 65179, 65180, 65181, 65182, 148, 65183,
            81, 53670, 14426, 36716, 1, 32859, 1, 32798, 13, 32801,
            1, 32771, 5, 32769, 7, 32769, 4, 32770, 4, 32770,
            8, 32769, 7, 32769, 16, 32770, 14, 32769, 4295, 32769,
            76, 32769, 27, 32769, 81, 32769, 9, 32769, 26, 32772,
            1, 32769, 1, 32770, 3, 32769, 6, 32771, 1, 32770,
            2, 32771, 1030, 32770, 1, 32786, 4, 32778, 1, 32772,
            1, 32782, 1, 32772, 149, 32862, 129, 32774, 26
        };

        internal unsafe GB18030Encoding()
            : base(54936, 936, EncoderFallback.ReplacementFallback, DecoderFallback.ReplacementFallback)
        {
        }

        protected unsafe override void LoadManagedCodePage()
        {
            iExtraBytes = 87032;
            base.LoadManagedCodePage();
            byte* ptr = (byte*)(void*)safeNativeMemoryHandle.DangerousGetHandle();
            mapUnicodeTo4BytesFlags = ptr + 262144;
            map4BytesToUnicode = (char*)(ptr + 262144 + 8192);
            char c = '\0';
            ushort num = 0;
            for (int i = 0; i < _tableUnicodeToGBDiffs.Length; i++)
            {
                ushort num2 = _tableUnicodeToGBDiffs[i];
                if ((num2 & 0x8000u) != 0)
                {
                    if (num2 > 36864 && num2 != 53670)
                    {
                        mapBytesToUnicode[(int)num2] = c;
                        mapUnicodeToBytes[(int)c] = num2;
                        c = (char)(c + 1);
                    }
                    else
                    {
                        c = (char)(c + (ushort)(num2 & 0x7FFF));
                    }
                    continue;
                }
                while (num2 > 0)
                {
                    map4BytesToUnicode[(int)num] = c;
                    mapUnicodeToBytes[(int)c] = num;
                    byte* num3 = mapUnicodeTo4BytesFlags + (int)c / 8;
                    *num3 = (byte)(*num3 | (byte)(1 << (int)c % 8));
                    c = (char)(c + 1);
                    num = (ushort)(num + 1);
                    num2 = (ushort)(num2 - 1);
                }
            }
        }

        internal unsafe bool Is4Byte(char charTest)
        {
            byte b = mapUnicodeTo4BytesFlags[(int)charTest / 8];
            if (b != 0)
            {
                return (b & (1 << (int)charTest % 8)) != 0;
            }
            return false;
        }

        public unsafe override int GetByteCount(char* chars, int count, System.Text.EncoderNLS encoder)
        {
            return GetBytes(chars, count, null, 0, encoder);
        }

        public unsafe override int GetBytes(char* chars, int charCount, byte* bytes, int byteCount, System.Text.EncoderNLS encoder)
        {
            char c = '\0';
            if (encoder != null)
            {
                c = encoder.charLeftOver;
            }
            EncodingByteBuffer encodingByteBuffer = new EncodingByteBuffer(this, encoder, bytes, byteCount, chars, charCount);
            while (true)
            {
                if (encodingByteBuffer.MoreData)
                {
                    char nextChar = encodingByteBuffer.GetNextChar();
                    if (c != 0)
                    {
                        if (!char.IsLowSurrogate(nextChar))
                        {
                            encodingByteBuffer.MovePrevious(bThrow: false);
                            if (encodingByteBuffer.Fallback(c))
                            {
                                c = '\0';
                                continue;
                            }
                            c = '\0';
                        }
                        else
                        {
                            int num = (c - 55296 << 10) + (nextChar - 56320);
                            byte b = (byte)(num % 10 + 48);
                            num /= 10;
                            byte b2 = (byte)(num % 126 + 129);
                            num /= 126;
                            byte b3 = (byte)(num % 10 + 48);
                            num /= 10;
                            c = '\0';
                            if (encodingByteBuffer.AddByte((byte)(num + 144), b3, b2, b))
                            {
                                c = '\0';
                                continue;
                            }
                            encodingByteBuffer.MovePrevious(bThrow: false);
                        }
                    }
                    else if (nextChar <= '\u007f')
                    {
                        if (encodingByteBuffer.AddByte((byte)nextChar))
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if (char.IsHighSurrogate(nextChar))
                        {
                            c = nextChar;
                            continue;
                        }
                        if (char.IsLowSurrogate(nextChar))
                        {
                            if (encodingByteBuffer.Fallback(nextChar))
                            {
                                continue;
                            }
                        }
                        else
                        {
                            ushort num2 = mapUnicodeToBytes[(int)nextChar];
                            if (Is4Byte(nextChar))
                            {
                                byte b4 = (byte)((int)num2 % 10 + 48);
                                num2 = (ushort)((int)num2 / 10);
                                byte b5 = (byte)((int)num2 % 126 + 129);
                                num2 = (ushort)((int)num2 / 126);
                                byte b6 = (byte)((int)num2 % 10 + 48);
                                num2 = (ushort)((int)num2 / 10);
                                if (encodingByteBuffer.AddByte((byte)(num2 + 129), b6, b5, b4))
                                {
                                    continue;
                                }
                            }
                            else if (encodingByteBuffer.AddByte((byte)(num2 >> 8), (byte)(num2 & 0xFFu)))
                            {
                                continue;
                            }
                        }
                    }
                }
                if ((encoder != null && !encoder.MustFlush) || c <= '\0')
                {
                    break;
                }
                encodingByteBuffer.Fallback(c);
                c = '\0';
            }
            if (encoder != null)
            {
                if (bytes != null)
                {
                    encoder.charLeftOver = c;
                }
                encoder.m_charsUsed = encodingByteBuffer.CharsUsed;
            }
            return encodingByteBuffer.Count;
        }

        internal static bool IsGBLeadByte(short ch)
        {
            if (ch >= 129)
            {
                return ch <= 254;
            }
            return false;
        }

        internal static bool IsGBTwoByteTrailing(short ch)
        {
            if (ch < 64 || ch > 126)
            {
                if (ch >= 128)
                {
                    return ch <= 254;
                }
                return false;
            }
            return true;
        }

        internal static bool IsGBFourByteTrailing(short ch)
        {
            if (ch >= 48)
            {
                return ch <= 57;
            }
            return false;
        }

        internal static int GetFourBytesOffset(short offset1, short offset2, short offset3, short offset4)
        {
            return (offset1 - 129) * 10 * 126 * 10 + (offset2 - 48) * 126 * 10 + (offset3 - 129) * 10 + offset4 - 48;
        }

        public unsafe override int GetCharCount(byte* bytes, int count, System.Text.DecoderNLS baseDecoder)
        {
            return GetChars(bytes, count, null, 0, baseDecoder);
        }

        public unsafe override int GetChars(byte* bytes, int byteCount, char* chars, int charCount, System.Text.DecoderNLS baseDecoder)
        {
            GB18030Decoder gB18030Decoder = (GB18030Decoder)baseDecoder;
            EncodingCharBuffer encodingCharBuffer = new EncodingCharBuffer(this, gB18030Decoder, chars, charCount, bytes, byteCount);
            short num = -1;
            short num2 = -1;
            short num3 = -1;
            short num4 = -1;
            if (gB18030Decoder != null && gB18030Decoder.bLeftOver1 != -1)
            {
                num = gB18030Decoder.bLeftOver1;
                num2 = gB18030Decoder.bLeftOver2;
                num3 = gB18030Decoder.bLeftOver3;
                num4 = gB18030Decoder.bLeftOver4;
                while (num != -1)
                {
                    if (!IsGBLeadByte(num))
                    {
                        if (num <= 127)
                        {
                            if (!encodingCharBuffer.AddChar((char)num))
                            {
                                break;
                            }
                        }
                        else if (!encodingCharBuffer.Fallback((byte)num))
                        {
                            break;
                        }
                        num = num2;
                        num2 = num3;
                        num3 = num4;
                        num4 = -1;
                        continue;
                    }
                    while (num2 == -1 || (IsGBFourByteTrailing(num2) && num4 == -1))
                    {
                        if (!encodingCharBuffer.MoreData)
                        {
                            if (gB18030Decoder.MustFlush)
                            {
                                break;
                            }
                            if (chars != null)
                            {
                                gB18030Decoder.bLeftOver1 = num;
                                gB18030Decoder.bLeftOver2 = num2;
                                gB18030Decoder.bLeftOver3 = num3;
                                gB18030Decoder.bLeftOver4 = num4;
                            }
                            gB18030Decoder.m_bytesUsed = encodingCharBuffer.BytesUsed;
                            return encodingCharBuffer.Count;
                        }
                        if (num2 == -1)
                        {
                            num2 = encodingCharBuffer.GetNextByte();
                        }
                        else if (num3 == -1)
                        {
                            num3 = encodingCharBuffer.GetNextByte();
                        }
                        else
                        {
                            num4 = encodingCharBuffer.GetNextByte();
                        }
                    }
                    if (IsGBTwoByteTrailing(num2))
                    {
                        int num5 = num << 8;
                        num5 |= (byte)num2;
                        if (!encodingCharBuffer.AddChar(mapBytesToUnicode[num5], 2))
                        {
                            break;
                        }
                        num = -1;
                        num2 = -1;
                    }
                    else if (IsGBFourByteTrailing(num2) && IsGBLeadByte(num3) && IsGBFourByteTrailing(num4))
                    {
                        int fourBytesOffset = GetFourBytesOffset(num, num2, num3, num4);
                        if (fourBytesOffset <= 39419)
                        {
                            if (!encodingCharBuffer.AddChar(map4BytesToUnicode[fourBytesOffset], 4))
                            {
                                break;
                            }
                        }
                        else if (fourBytesOffset >= 189000 && fourBytesOffset <= 1237575)
                        {
                            fourBytesOffset -= 189000;
                            if (!encodingCharBuffer.AddChar((char)(55296 + fourBytesOffset / 1024), (char)(56320 + fourBytesOffset % 1024), 4))
                            {
                                break;
                            }
                        }
                        else if (!encodingCharBuffer.Fallback((byte)num, (byte)num2, (byte)num3, (byte)num4))
                        {
                            break;
                        }
                        num = -1;
                        num2 = -1;
                        num3 = -1;
                        num4 = -1;
                    }
                    else
                    {
                        if (!encodingCharBuffer.Fallback((byte)num))
                        {
                            break;
                        }
                        num = num2;
                        num2 = num3;
                        num3 = num4;
                        num4 = -1;
                    }
                }
            }
            while (encodingCharBuffer.MoreData)
            {
                byte nextByte = encodingCharBuffer.GetNextByte();
                if (nextByte <= 127)
                {
                    if (!encodingCharBuffer.AddChar((char)nextByte))
                    {
                        break;
                    }
                }
                else if (IsGBLeadByte(nextByte))
                {
                    if (encodingCharBuffer.MoreData)
                    {
                        byte nextByte2 = encodingCharBuffer.GetNextByte();
                        if (IsGBTwoByteTrailing(nextByte2))
                        {
                            int num6 = nextByte << 8;
                            num6 |= nextByte2;
                            if (!encodingCharBuffer.AddChar(mapBytesToUnicode[num6], 2))
                            {
                                break;
                            }
                        }
                        else if (IsGBFourByteTrailing(nextByte2))
                        {
                            if (encodingCharBuffer.EvenMoreData(2))
                            {
                                byte nextByte3 = encodingCharBuffer.GetNextByte();
                                byte nextByte4 = encodingCharBuffer.GetNextByte();
                                if (IsGBLeadByte(nextByte3) && IsGBFourByteTrailing(nextByte4))
                                {
                                    int fourBytesOffset2 = GetFourBytesOffset(nextByte, nextByte2, nextByte3, nextByte4);
                                    if (fourBytesOffset2 <= 39419)
                                    {
                                        if (!encodingCharBuffer.AddChar(map4BytesToUnicode[fourBytesOffset2], 4))
                                        {
                                            break;
                                        }
                                    }
                                    else if (fourBytesOffset2 >= 189000 && fourBytesOffset2 <= 1237575)
                                    {
                                        fourBytesOffset2 -= 189000;
                                        if (!encodingCharBuffer.AddChar((char)(55296 + fourBytesOffset2 / 1024), (char)(56320 + fourBytesOffset2 % 1024), 4))
                                        {
                                            break;
                                        }
                                    }
                                    else if (!encodingCharBuffer.Fallback(nextByte, nextByte2, nextByte3, nextByte4))
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    encodingCharBuffer.AdjustBytes(-3);
                                    if (!encodingCharBuffer.Fallback(nextByte))
                                    {
                                        break;
                                    }
                                }
                                continue;
                            }
                            if (gB18030Decoder != null && !gB18030Decoder.MustFlush)
                            {
                                if (chars != null)
                                {
                                    num = nextByte;
                                    num2 = nextByte2;
                                    num3 = (short)((!encodingCharBuffer.MoreData) ? (-1) : encodingCharBuffer.GetNextByte());
                                    num4 = -1;
                                }
                                break;
                            }
                            if (!encodingCharBuffer.Fallback(nextByte, nextByte2))
                            {
                                break;
                            }
                        }
                        else
                        {
                            encodingCharBuffer.AdjustBytes(-1);
                            if (!encodingCharBuffer.Fallback(nextByte))
                            {
                                break;
                            }
                        }
                        continue;
                    }
                    if (gB18030Decoder != null && !gB18030Decoder.MustFlush)
                    {
                        if (chars != null)
                        {
                            num = nextByte;
                            num2 = -1;
                            num3 = -1;
                            num4 = -1;
                        }
                        break;
                    }
                    if (!encodingCharBuffer.Fallback(nextByte))
                    {
                        break;
                    }
                }
                else if (!encodingCharBuffer.Fallback(nextByte))
                {
                    break;
                }
            }
            if (gB18030Decoder != null)
            {
                if (chars != null)
                {
                    gB18030Decoder.bLeftOver1 = num;
                    gB18030Decoder.bLeftOver2 = num2;
                    gB18030Decoder.bLeftOver3 = num3;
                    gB18030Decoder.bLeftOver4 = num4;
                }
                gB18030Decoder.m_bytesUsed = encodingCharBuffer.BytesUsed;
            }
            return encodingCharBuffer.Count;
        }

        public override int GetMaxByteCount(int charCount)
        {
            if (charCount < 0)
            {
                throw new ArgumentOutOfRangeException("charCount", MDCFR.Properties.Resources.ArgumentOutOfRange_NeedNonNegNum);
            }
            long num = (long)charCount + 1L;
            if (base.EncoderFallback.MaxCharCount > 1)
            {
                num *= base.EncoderFallback.MaxCharCount;
            }
            num *= 4;
            if (num > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException("charCount", MDCFR.Properties.Resources.ArgumentOutOfRange_GetByteCountOverflow);
            }
            return (int)num;
        }

        public override int GetMaxCharCount(int byteCount)
        {
            if (byteCount < 0)
            {
                throw new ArgumentOutOfRangeException("byteCount", MDCFR.Properties.Resources.ArgumentOutOfRange_NeedNonNegNum);
            }
            long num = (long)byteCount + 3L;
            if (base.DecoderFallback.MaxCharCount > 1)
            {
                num *= base.DecoderFallback.MaxCharCount;
            }
            if (num > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException("byteCount", MDCFR.Properties.Resources.ArgumentOutOfRange_GetCharCountOverflow);
            }
            return (int)num;
        }

        public override Decoder GetDecoder()
        {
            return new GB18030Decoder(this);
        }
    }

    internal sealed class InternalDecoderBestFitFallback : DecoderFallback
    {
        internal System.Text.BaseCodePageEncoding encoding;

        internal char[] arrayBestFit;

        internal char cReplacement = '?';

        public override int MaxCharCount => 1;

        internal InternalDecoderBestFitFallback(System.Text.BaseCodePageEncoding _encoding)
        {
            encoding = _encoding;
        }

        public override DecoderFallbackBuffer CreateFallbackBuffer()
        {
            return new System.Text.InternalDecoderBestFitFallbackBuffer(this);
        }

        public override bool Equals([NotNullWhen(true)] object value)
        {
            if (value is System.Text.InternalDecoderBestFitFallback internalDecoderBestFitFallback)
            {
                return encoding.CodePage == internalDecoderBestFitFallback.encoding.CodePage;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return encoding.CodePage;
        }
    }

    internal sealed class InternalDecoderBestFitFallbackBuffer : DecoderFallbackBuffer
    {
        internal char cBestFit;

        internal int iCount = -1;

        internal int iSize;

        private readonly System.Text.InternalDecoderBestFitFallback _oFallback;

        private static object s_InternalSyncObject;

        private static object InternalSyncObject
        {
            get
            {
                if (s_InternalSyncObject == null)
                {
                    object value = new object();
                    Interlocked.CompareExchange<object>(ref s_InternalSyncObject, value, (object)null);
                }
                return s_InternalSyncObject;
            }
        }

        public override int Remaining
        {
            get
            {
                if (iCount <= 0)
                {
                    return 0;
                }
                return iCount;
            }
        }

        public InternalDecoderBestFitFallbackBuffer(System.Text.InternalDecoderBestFitFallback fallback)
        {
            _oFallback = fallback;
            if (_oFallback.arrayBestFit != null)
            {
                return;
            }
            lock (InternalSyncObject)
            {
                System.Text.InternalDecoderBestFitFallback oFallback = _oFallback;
                if (oFallback.arrayBestFit == null)
                {
                    oFallback.arrayBestFit = fallback.encoding.GetBestFitBytesToUnicodeData();
                }
            }
        }

        public override bool Fallback(byte[] bytesUnknown, int index)
        {
            cBestFit = TryBestFit(bytesUnknown);
            if (cBestFit == '\0')
            {
                cBestFit = _oFallback.cReplacement;
            }
            iCount = (iSize = 1);
            return true;
        }

        public override char GetNextChar()
        {
            iCount--;
            if (iCount < 0)
            {
                return '\0';
            }
            if (iCount == int.MaxValue)
            {
                iCount = -1;
                return '\0';
            }
            return cBestFit;
        }

        public override bool MovePrevious()
        {
            if (iCount >= 0)
            {
                iCount++;
            }
            if (iCount >= 0)
            {
                return iCount <= iSize;
            }
            return false;
        }

        public override void Reset()
        {
            iCount = -1;
        }

        internal unsafe static int InternalFallback(byte[] bytes, byte* pBytes)
        {
            return 1;
        }

        private char TryBestFit(byte[] bytesCheck)
        {
            int num = 0;
            int num2 = _oFallback.arrayBestFit.Length;
            if (num2 == 0)
            {
                return '\0';
            }
            if (bytesCheck.Length == 0 || bytesCheck.Length > 2)
            {
                return '\0';
            }
            char c = ((bytesCheck.Length != 1) ? ((char)((bytesCheck[0] << 8) + bytesCheck[1])) : ((char)bytesCheck[0]));
            if (c < _oFallback.arrayBestFit[0] || c > _oFallback.arrayBestFit[num2 - 2])
            {
                return '\0';
            }
            int num3;
            while ((num3 = num2 - num) > 6)
            {
                int num4 = (num3 / 2 + num) & 0xFFFE;
                char c2 = _oFallback.arrayBestFit[num4];
                if (c2 == c)
                {
                    return _oFallback.arrayBestFit[num4 + 1];
                }
                if (c2 < c)
                {
                    num = num4;
                }
                else
                {
                    num2 = num4;
                }
            }
            for (int num4 = num; num4 < num2; num4 += 2)
            {
                if (_oFallback.arrayBestFit[num4] == c)
                {
                    return _oFallback.arrayBestFit[num4 + 1];
                }
            }
            return '\0';
        }
    }

    internal sealed class InternalEncoderBestFitFallback : EncoderFallback
    {
        internal System.Text.BaseCodePageEncoding encoding;

        internal char[] arrayBestFit;

        public override int MaxCharCount => 1;

        internal InternalEncoderBestFitFallback(System.Text.BaseCodePageEncoding _encoding)
        {
            encoding = _encoding;
        }

        public override EncoderFallbackBuffer CreateFallbackBuffer()
        {
            return new System.Text.InternalEncoderBestFitFallbackBuffer(this);
        }

        public override bool Equals([NotNullWhen(true)] object value)
        {
            if (value is System.Text.InternalEncoderBestFitFallback internalEncoderBestFitFallback)
            {
                return encoding.CodePage == internalEncoderBestFitFallback.encoding.CodePage;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return encoding.CodePage;
        }
    }

    internal sealed class InternalEncoderBestFitFallbackBuffer : EncoderFallbackBuffer
    {
        private char _cBestFit;

        private readonly System.Text.InternalEncoderBestFitFallback _oFallback;

        private int _iCount = -1;

        private int _iSize;

        private static object s_InternalSyncObject;

        private static object InternalSyncObject
        {
            get
            {
                if (s_InternalSyncObject == null)
                {
                    object value = new object();
                    Interlocked.CompareExchange<object>(ref s_InternalSyncObject, value, (object)null);
                }
                return s_InternalSyncObject;
            }
        }

        public override int Remaining
        {
            get
            {
                if (_iCount <= 0)
                {
                    return 0;
                }
                return _iCount;
            }
        }

        public InternalEncoderBestFitFallbackBuffer(System.Text.InternalEncoderBestFitFallback fallback)
        {
            _oFallback = fallback;
            if (_oFallback.arrayBestFit != null)
            {
                return;
            }
            lock (InternalSyncObject)
            {
                System.Text.InternalEncoderBestFitFallback oFallback = _oFallback;
                if (oFallback.arrayBestFit == null)
                {
                    oFallback.arrayBestFit = fallback.encoding.GetBestFitUnicodeToBytesData();
                }
            }
        }

        public override bool Fallback(char charUnknown, int index)
        {
            _iCount = (_iSize = 1);
            _cBestFit = TryBestFit(charUnknown);
            if (_cBestFit == '\0')
            {
                _cBestFit = '?';
            }
            return true;
        }

        public override bool Fallback(char charUnknownHigh, char charUnknownLow, int index)
        {
            if (!char.IsHighSurrogate(charUnknownHigh))
            {
                throw new ArgumentOutOfRangeException("charUnknownHigh", 
                    System.SR.Format(MDCFR.Properties.Resources.ArgumentOutOfRange_Range, 55296, 56319));
            }
            if (!char.IsLowSurrogate(charUnknownLow))
            {
                throw new ArgumentOutOfRangeException("charUnknownLow", 
                    System.SR.Format(MDCFR.Properties.Resources.ArgumentOutOfRange_Range, 56320, 57343));
            }
            _cBestFit = '?';
            _iCount = (_iSize = 2);
            return true;
        }

        public override char GetNextChar()
        {
            _iCount--;
            if (_iCount < 0)
            {
                return '\0';
            }
            if (_iCount == int.MaxValue)
            {
                _iCount = -1;
                return '\0';
            }
            return _cBestFit;
        }

        public override bool MovePrevious()
        {
            if (_iCount >= 0)
            {
                _iCount++;
            }
            if (_iCount >= 0)
            {
                return _iCount <= _iSize;
            }
            return false;
        }

        public override void Reset()
        {
            _iCount = -1;
        }

        private char TryBestFit(char cUnknown)
        {
            int num = 0;
            int num2 = _oFallback.arrayBestFit.Length;
            int num3;
            while ((num3 = num2 - num) > 6)
            {
                int num4 = (num3 / 2 + num) & 0xFFFE;
                char c = _oFallback.arrayBestFit[num4];
                if (c == cUnknown)
                {
                    return _oFallback.arrayBestFit[num4 + 1];
                }
                if (c < cUnknown)
                {
                    num = num4;
                }
                else
                {
                    num2 = num4;
                }
            }
            for (int num4 = num; num4 < num2; num4 += 2)
            {
                if (_oFallback.arrayBestFit[num4] == cUnknown)
                {
                    return _oFallback.arrayBestFit[num4 + 1];
                }
            }
            return '\0';
        }
    }

    internal sealed class ISCIIEncoding : System.Text.EncodingNLS, ISerializable
    {
        internal sealed class ISCIIEncoder : System.Text.EncoderNLS
        {
            internal int defaultCodePage;

            internal int currentCodePage;

            internal bool bLastVirama;

            internal override bool HasState
            {
                get
                {
                    if (charLeftOver == '\0')
                    {
                        return currentCodePage != defaultCodePage;
                    }
                    return true;
                }
            }

            public ISCIIEncoder(System.Text.EncodingNLS encoding)
                : base(encoding)
            {
                currentCodePage = (defaultCodePage = encoding.CodePage - 57000);
            }

            public override void Reset()
            {
                bLastVirama = false;
                charLeftOver = '\0';
                m_fallbackBuffer?.Reset();
            }
        }

        internal sealed class ISCIIDecoder : System.Text.DecoderNLS
        {
            internal int currentCodePage;

            internal bool bLastATR;

            internal bool bLastVirama;

            internal bool bLastDevenagariStressAbbr;

            internal char cLastCharForNextNukta;

            internal char cLastCharForNoNextNukta;

            internal override bool HasState
            {
                get
                {
                    if (cLastCharForNextNukta == '\0' && cLastCharForNoNextNukta == '\0' && !bLastATR)
                    {
                        return bLastDevenagariStressAbbr;
                    }
                    return true;
                }
            }

            public ISCIIDecoder(System.Text.EncodingNLS encoding)
                : base(encoding)
            {
                currentCodePage = encoding.CodePage - 57000;
            }

            public override void Reset()
            {
                bLastATR = false;
                bLastVirama = false;
                bLastDevenagariStressAbbr = false;
                cLastCharForNextNukta = '\0';
                cLastCharForNoNextNukta = '\0';
                m_fallbackBuffer?.Reset();
            }
        }

        private const int CodeDevanagari = 2;

        private const int CodePunjabi = 11;

        private const int MultiByteBegin = 160;

        private const int IndicBegin = 2305;

        private const int IndicEnd = 3439;

        private const byte ControlATR = 239;

        private const byte ControlCodePageStart = 64;

        private const byte Virama = 232;

        private const byte Nukta = 233;

        private const byte DevenagariExt = 240;

        private const char ZWNJ = '\u200c';

        private const char ZWJ = '\u200d';

        private readonly int _defaultCodePage;

        private static readonly int[] s_UnicodeToIndicChar = new int[1135]
        {
            673, 674, 675, 0, 676, 677, 678, 679, 680, 681,
            682, 4774, 686, 683, 684, 685, 690, 687, 688, 689,
            691, 692, 693, 694, 695, 696, 697, 698, 699, 700,
            701, 702, 703, 704, 705, 706, 707, 708, 709, 710,
            711, 712, 713, 714, 715, 716, 717, 719, 720, 721,
            722, 723, 724, 725, 726, 727, 728, 0, 0, 745,
            4842, 730, 731, 732, 733, 734, 735, 4831, 739, 736,
            737, 738, 743, 740, 741, 742, 744, 0, 0, 4769,
            0, 8944, 0, 0, 0, 0, 0, 4787, 4788, 4789,
            4794, 4799, 4800, 4809, 718, 4778, 4775, 4827, 4828, 746,
            0, 753, 754, 755, 756, 757, 758, 759, 760, 761,
            762, 13040, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 929, 930,
            931, 0, 932, 933, 934, 935, 936, 937, 938, 5030,
            0, 0, 939, 941, 0, 0, 943, 945, 947, 948,
            949, 950, 951, 952, 953, 954, 955, 956, 957, 958,
            959, 960, 961, 962, 963, 964, 965, 966, 0, 968,
            969, 970, 971, 972, 973, 975, 0, 977, 0, 0,
            0, 981, 982, 983, 984, 0, 0, 1001, 0, 986,
            987, 988, 989, 990, 991, 5087, 0, 0, 992, 994,
            0, 0, 996, 998, 1000, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 5055,
            5056, 0, 974, 5034, 5031, 5083, 5084, 0, 0, 1009,
            1010, 1011, 1012, 1013, 1014, 1015, 1016, 1017, 1018, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 2978, 0, 0,
            2980, 2981, 2982, 2983, 2984, 2985, 0, 0, 0, 0,
            2987, 2989, 0, 0, 2992, 2993, 2995, 2996, 2997, 2998,
            2999, 3000, 3001, 3002, 3003, 3004, 3005, 3006, 3007, 3008,
            3009, 3010, 3011, 3012, 3013, 3014, 0, 3016, 3017, 3018,
            3019, 3020, 3021, 3023, 0, 3025, 3026, 0, 3028, 3029,
            0, 3031, 3032, 0, 0, 3049, 0, 3034, 3035, 3036,
            3037, 3038, 0, 0, 0, 0, 3040, 3042, 0, 0,
            3044, 3046, 3048, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 7092, 7093, 7098, 7104, 0, 7113,
            0, 0, 0, 0, 0, 0, 0, 3057, 3058, 3059,
            3060, 3061, 3062, 3063, 3064, 3065, 3066, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 2721, 2722, 2723, 0, 2724, 2725,
            2726, 2727, 2728, 2729, 2730, 0, 2734, 0, 2731, 2733,
            2738, 0, 2736, 2737, 2739, 2740, 2741, 2742, 2743, 2744,
            2745, 2746, 2747, 2748, 2749, 2750, 2751, 2752, 2753, 2754,
            2755, 2756, 2757, 2758, 0, 2760, 2761, 2762, 2763, 2764,
            2765, 2767, 0, 2769, 2770, 0, 2772, 2773, 2774, 2775,
            2776, 0, 0, 2793, 6890, 2778, 2779, 2780, 2781, 2782,
            2783, 6879, 2787, 0, 2784, 2786, 2791, 0, 2788, 2790,
            2792, 0, 0, 6817, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 6826,
            0, 0, 0, 0, 0, 2801, 2802, 2803, 2804, 2805,
            2806, 2807, 2808, 2809, 2810, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 1953, 1954, 1955, 0, 1956, 1957, 1958, 1959,
            1960, 1961, 1962, 6054, 0, 0, 1963, 1965, 0, 0,
            1968, 1969, 1971, 1972, 1973, 1974, 1975, 1976, 1977, 1978,
            1979, 1980, 1981, 1982, 1983, 1984, 1985, 1986, 1987, 1988,
            1989, 1990, 0, 1992, 1993, 1994, 1995, 1996, 1997, 1999,
            0, 2001, 2002, 0, 0, 2005, 2006, 2007, 2008, 0,
            0, 2025, 6122, 2010, 2011, 2012, 2013, 2014, 2015, 0,
            0, 0, 2016, 2018, 0, 0, 2020, 2022, 2024, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 6079, 6080, 0, 1998, 6058, 6055, 0,
            0, 0, 0, 2033, 2034, 2035, 2036, 2037, 2038, 2039,
            2040, 2041, 2042, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 1186, 1187, 0, 1188, 1189, 1190, 1191, 1192, 1193,
            0, 0, 0, 0, 1195, 1197, 0, 1199, 1200, 1201,
            1203, 0, 0, 0, 1207, 1208, 0, 1210, 0, 1212,
            1213, 0, 0, 0, 1217, 1218, 0, 0, 0, 1222,
            1223, 1224, 0, 0, 0, 1228, 1229, 1231, 1232, 1233,
            1234, 1235, 1236, 0, 1237, 1239, 1240, 0, 0, 0,
            0, 1242, 1243, 1244, 1245, 1246, 0, 0, 0, 1248,
            1249, 1250, 0, 1252, 1253, 1254, 1256, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 1266, 1267, 1268, 1269, 1270, 1271, 1272, 1273,
            1274, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 1441, 1442,
            1443, 0, 1444, 1445, 1446, 1447, 1448, 1449, 1450, 5542,
            0, 1451, 1452, 1453, 0, 1455, 1456, 1457, 1459, 1460,
            1461, 1462, 1463, 1464, 1465, 1466, 1467, 1468, 1469, 1470,
            1471, 1472, 1473, 1474, 1475, 1476, 1477, 1478, 0, 1480,
            1481, 1482, 1483, 1484, 1485, 1487, 1488, 1489, 1490, 0,
            1492, 1493, 1494, 1495, 1496, 0, 0, 0, 0, 1498,
            1499, 1500, 1501, 1502, 1503, 5599, 0, 1504, 1505, 1506,
            0, 1508, 1509, 1510, 1512, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 5546, 5543, 0, 0, 0, 0, 1521,
            1522, 1523, 1524, 1525, 1526, 1527, 1528, 1529, 1530, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 2210, 2211, 0,
            2212, 2213, 2214, 2215, 2216, 2217, 2218, 6310, 0, 2219,
            2220, 2221, 0, 2223, 2224, 2225, 2227, 2228, 2229, 2230,
            2231, 2232, 2233, 2234, 2235, 2236, 2237, 2238, 2239, 2240,
            2241, 2242, 2243, 2244, 2245, 2246, 0, 2248, 2249, 2250,
            2251, 2252, 2253, 2255, 2256, 2257, 2258, 0, 2260, 2261,
            2262, 2263, 2264, 0, 0, 0, 0, 2266, 2267, 2268,
            2269, 2270, 2271, 6367, 0, 2272, 2273, 2274, 0, 2276,
            2277, 2278, 2280, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 6345,
            0, 6314, 6311, 0, 0, 0, 0, 2289, 2290, 2291,
            2292, 2293, 2294, 2295, 2296, 2297, 2298, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 2466, 2467, 0, 2468, 2469,
            2470, 2471, 2472, 2473, 2474, 6566, 0, 2475, 2476, 2477,
            0, 2479, 2480, 2481, 2483, 2484, 2485, 2486, 2487, 2488,
            2489, 2490, 2491, 2492, 2493, 2494, 2495, 2496, 2497, 2498,
            2499, 2500, 2501, 2502, 0, 2504, 2505, 2506, 2507, 2508,
            2509, 2511, 2512, 2513, 2514, 2515, 2516, 2517, 2518, 2519,
            2520, 0, 0, 0, 0, 2522, 2523, 2524, 2525, 2526,
            2527, 0, 0, 2528, 2529, 2530, 0, 2532, 2533, 2534,
            2536, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 6570,
            6567, 0, 0, 0, 0, 2545, 2546, 2547, 2548, 2549,
            2550, 2551, 2552, 2553, 2554
        };

        private static readonly int[] s_IndicMappingIndex = new int[12]
        {
            -1, -1, 0, 1, 2, 3, 1, 4, 5, 6,
            7, 8
        };

        private static readonly char[,,] s_IndicMapping = new char[9, 2, 96]
        {
            {
                {
                    '\0', '\u0901', '\u0902', '\u0903', 'अ', 'आ', 'इ', 'ई', 'उ', 'ऊ',
                    'ऋ', 'ऎ', 'ए', 'ऐ', 'ऍ', 'ऒ', 'ओ', 'औ', 'ऑ', 'क',
                    'ख', 'ग', 'घ', 'ङ', 'च', 'छ', 'ज', 'झ', 'ञ', 'ट',
                    'ठ', 'ड', 'ढ', 'ण', 'त', 'थ', 'द', 'ध', 'न', 'ऩ',
                    'प', 'फ', 'ब', 'भ', 'म', 'य', 'य़', 'र', 'ऱ', 'ल',
                    'ळ', 'ऴ', 'व', 'श', 'ष', 'स', 'ह', '\0', '\u093e', '\u093f',
                    '\u0940', '\u0941', '\u0942', '\u0943', '\u0946', '\u0947', '\u0948', '\u0945', '\u094a', '\u094b',
                    '\u094c', '\u0949', '\u094d', '\u093c', '।', '\0', '\0', '\0', '\0', '\0',
                    '\0', '०', '१', '२', '३', '४', '५', '६', '७', '८',
                    '९', '\0', '\0', '\0', '\0', '\0'
                },
                {
                    '\0', 'ॐ', '\0', '\0', '\0', '\0', 'ऌ', 'ॡ', '\0', '\0',
                    'ॠ', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', 'क़',
                    'ख़', 'ग़', '\0', '\0', '\0', '\0', 'ज़', '\0', '\0', '\0',
                    '\0', 'ड़', 'ढ़', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', 'फ़', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\u0962',
                    '\u0963', '\0', '\0', '\u0944', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\u200c', '\u200d', 'ऽ', '\0', '\0', '\0', '\0', '\0',
                    '뢿', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\0', '\0', '\0'
                }
            },
            {
                {
                    '\0', '\u0981', '\u0982', '\u0983', 'অ', 'আ', 'ই', 'ঈ', 'উ', 'ঊ',
                    'ঋ', 'এ', 'এ', 'ঐ', 'ঐ', 'ও', 'ও', 'ঔ', 'ঔ', 'ক',
                    'খ', 'গ', 'ঘ', 'ঙ', 'চ', 'ছ', 'জ', 'ঝ', 'ঞ', 'ট',
                    'ঠ', 'ড', 'ঢ', 'ণ', 'ত', 'থ', 'দ', 'ধ', 'ন', 'ন',
                    'প', 'ফ', 'ব', 'ভ', 'ম', 'য', 'য়', 'র', 'র', 'ল',
                    'ল', 'ল', 'ব', 'শ', 'ষ', 'স', 'হ', '\0', '\u09be', '\u09bf',
                    '\u09c0', '\u09c1', '\u09c2', '\u09c3', '\u09c7', '\u09c7', '\u09c8', '\u09c8', '\u09cb', '\u09cb',
                    '\u09cc', '\u09cc', '\u09cd', '\u09bc', '.', '\0', '\0', '\0', '\0', '\0',
                    '\0', '০', '১', '২', '৩', '৪', '৫', '৬', '৭', '৮',
                    '৯', '\0', '\0', '\0', '\0', '\0'
                },
                {
                    '\0', '\0', '\0', '\0', '\0', '\0', 'ঌ', 'ৡ', '\0', '\0',
                    'ৠ', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', 'ড়', 'ঢ়', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\u09e2',
                    '\u09e3', '\0', '\0', '\u09c4', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\u200c', '\u200d', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\0', '\0', '\0'
                }
            },
            {
                {
                    '\0', '\0', '\u0b82', 'ஃ', 'அ', 'ஆ', 'இ', 'ஈ', 'உ', 'ஊ',
                    '\0', 'ஏ', 'ஏ', 'ஐ', 'ஐ', 'ஒ', 'ஓ', 'ஔ', 'ஔ', 'க',
                    'க', 'க', 'க', 'ங', 'ச', 'ச', 'ஜ', 'ஜ', 'ஞ', 'ட',
                    'ட', 'ட', 'ட', 'ண', 'த', 'த', 'த', 'த', 'ந', 'ன',
                    'ப', 'ப', 'ப', 'ப', 'ம', 'ய', 'ய', 'ர', 'ற', 'ல',
                    'ள', 'ழ', 'வ', 'ஷ', 'ஷ', 'ஸ', 'ஹ', '\0', '\u0bbe', '\u0bbf',
                    '\u0bc0', '\u0bc1', '\u0bc2', '\0', '\u0bc6', '\u0bc7', '\u0bc8', '\u0bc8', '\u0bca', '\u0bcb',
                    '\u0bcc', '\u0bcc', '\u0bcd', '\0', '.', '\0', '\0', '\0', '\0', '\0',
                    '\0', '0', '௧', '௨', '௩', '௪', '௫', '௬', '௭', '௮',
                    '௯', '\0', '\0', '\0', '\0', '\0'
                },
                {
                    '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\u200c', '\u200d', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\0', '\0', '\0'
                }
            },
            {
                {
                    '\0', '\u0c01', '\u0c02', '\u0c03', 'అ', 'ఆ', 'ఇ', 'ఈ', 'ఉ', 'ఊ',
                    'ఋ', 'ఎ', 'ఏ', 'ఐ', 'ఐ', 'ఒ', 'ఓ', 'ఔ', 'ఔ', 'క',
                    'ఖ', 'గ', 'ఘ', 'ఙ', 'చ', 'ఛ', 'జ', 'ఝ', 'ఞ', 'ట',
                    'ఠ', 'డ', 'ఢ', 'ణ', 'త', 'థ', 'ద', 'ధ', 'న', 'న',
                    'ప', 'ఫ', 'బ', 'భ', 'మ', 'య', 'య', 'ర', 'ఱ', 'ల',
                    'ళ', 'ళ', 'వ', 'శ', 'ష', 'స', 'హ', '\0', '\u0c3e', '\u0c3f',
                    '\u0c40', '\u0c41', '\u0c42', '\u0c43', '\u0c46', '\u0c47', '\u0c48', '\u0c48', '\u0c4a', '\u0c4b',
                    '\u0c4c', '\u0c4c', '\u0c4d', '\0', '.', '\0', '\0', '\0', '\0', '\0',
                    '\0', '౦', '౧', '౨', '౩', '౪', '౫', '౬', '౭', '౮',
                    '౯', '\0', '\0', '\0', '\0', '\0'
                },
                {
                    '\0', '\0', '\0', '\0', '\0', '\0', 'ఌ', 'ౡ', '\0', '\0',
                    'ౠ', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\u0c44', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\u200c', '\u200d', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\0', '\0', '\0'
                }
            },
            {
                {
                    '\0', '\u0b01', '\u0b02', '\u0b03', 'ଅ', 'ଆ', 'ଇ', 'ଈ', 'ଉ', 'ଊ',
                    'ଋ', 'ଏ', 'ଏ', 'ଐ', 'ଐ', 'ଐ', 'ଓ', 'ଔ', 'ଔ', 'କ',
                    'ଖ', 'ଗ', 'ଘ', 'ଙ', 'ଚ', 'ଛ', 'ଜ', 'ଝ', 'ଞ', 'ଟ',
                    'ଠ', 'ଡ', 'ଢ', 'ଣ', 'ତ', 'ଥ', 'ଦ', 'ଧ', 'ନ', 'ନ',
                    'ପ', 'ଫ', 'ବ', 'ଭ', 'ମ', 'ଯ', 'ୟ', 'ର', 'ର', 'ଲ',
                    'ଳ', 'ଳ', 'ବ', 'ଶ', 'ଷ', 'ସ', 'ହ', '\0', '\u0b3e', '\u0b3f',
                    '\u0b40', '\u0b41', '\u0b42', '\u0b43', '\u0b47', '\u0b47', '\u0b48', '\u0b48', '\u0b4b', '\u0b4b',
                    '\u0b4c', '\u0b4c', '\u0b4d', '\u0b3c', '.', '\0', '\0', '\0', '\0', '\0',
                    '\0', '୦', '୧', '୨', '୩', '୪', '୫', '୬', '୭', '୮',
                    '୯', '\0', '\0', '\0', '\0', '\0'
                },
                {
                    '\0', '\0', '\0', '\0', '\0', '\0', 'ఌ', 'ౡ', '\0', '\0',
                    'ౠ', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', 'ଡ଼', 'ଢ଼', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\u0c44', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\u200c', '\u200d', 'ଽ', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\0', '\0', '\0'
                }
            },
            {
                {
                    '\0', '\0', '\u0c82', '\u0c83', 'ಅ', 'ಆ', 'ಇ', 'ಈ', 'ಉ', 'ಊ',
                    'ಋ', 'ಎ', 'ಏ', 'ಐ', 'ಐ', 'ಒ', 'ಓ', 'ಔ', 'ಔ', 'ಕ',
                    'ಖ', 'ಗ', 'ಘ', 'ಙ', 'ಚ', 'ಛ', 'ಜ', 'ಝ', 'ಞ', 'ಟ',
                    'ಠ', 'ಡ', 'ಢ', 'ಣ', 'ತ', 'ಥ', 'ದ', 'ಧ', 'ನ', 'ನ',
                    'ಪ', 'ಫ', 'ಬ', 'ಭ', 'ಮ', 'ಯ', 'ಯ', 'ರ', 'ಱ', 'ಲ',
                    'ಳ', 'ಳ', 'ವ', 'ಶ', 'ಷ', 'ಸ', 'ಹ', '\0', '\u0cbe', '\u0cbf',
                    '\u0cc0', '\u0cc1', '\u0cc2', '\u0cc3', '\u0cc6', '\u0cc7', '\u0cc8', '\u0cc8', '\u0cca', '\u0ccb',
                    '\u0ccc', '\u0ccc', '\u0ccd', '\0', '.', '\0', '\0', '\0', '\0', '\0',
                    '\0', '೦', '೧', '೨', '೩', '೪', '೫', '೬', '೭', '೮',
                    '೯', '\0', '\0', '\0', '\0', '\0'
                },
                {
                    '\0', '\0', '\0', '\0', '\0', '\0', 'ಌ', 'ೡ', '\0', '\0',
                    'ೠ', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', 'ೞ', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\u0cc4', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\u200c', '\u200d', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\0', '\0', '\0'
                }
            },
            {
                {
                    '\0', '\0', '\u0d02', '\u0d03', 'അ', 'ആ', 'ഇ', 'ഈ', 'ഉ', 'ഊ',
                    'ഋ', 'എ', 'ഏ', 'ഐ', 'ഐ', 'ഒ', 'ഓ', 'ഔ', 'ഔ', 'ക',
                    'ഖ', 'ഗ', 'ഘ', 'ങ', 'ച', 'ഛ', 'ജ', 'ഝ', 'ഞ', 'ട',
                    'ഠ', 'ഡ', 'ഢ', 'ണ', 'ത', 'ഥ', 'ദ', 'ധ', 'ന', 'ന',
                    'പ', 'ഫ', 'ബ', 'ഭ', 'മ', 'യ', 'യ', 'ര', 'റ', 'ല',
                    'ള', 'ഴ', 'വ', 'ശ', 'ഷ', 'സ', 'ഹ', '\0', '\u0d3e', '\u0d3f',
                    '\u0d40', '\u0d41', '\u0d42', '\u0d43', '\u0d46', '\u0d47', '\u0d48', '\u0d48', '\u0d4a', '\u0d4b',
                    '\u0d4c', '\u0d4c', '\u0d4d', '\0', '.', '\0', '\0', '\0', '\0', '\0',
                    '\0', '൦', '൧', '൨', '൩', '൪', '൫', '൬', '൭', '൮',
                    '൯', '\0', '\0', '\0', '\0', '\0'
                },
                {
                    '\0', '\0', '\0', '\0', '\0', '\0', 'ഌ', 'ൡ', '\0', '\0',
                    'ൠ', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\u200c', '\u200d', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\0', '\0', '\0'
                }
            },
            {
                {
                    '\0', '\u0a81', '\u0a82', '\u0a83', 'અ', 'આ', 'ઇ', 'ઈ', 'ઉ', 'ઊ',
                    'ઋ', 'એ', 'એ', 'ઐ', 'ઍ', 'ઍ', 'ઓ', 'ઔ', 'ઑ', 'ક',
                    'ખ', 'ગ', 'ઘ', 'ઙ', 'ચ', 'છ', 'જ', 'ઝ', 'ઞ', 'ટ',
                    'ઠ', 'ડ', 'ઢ', 'ણ', 'ત', 'થ', 'દ', 'ધ', 'ન', 'ન',
                    'પ', 'ફ', 'બ', 'ભ', 'મ', 'ય', 'ય', 'ર', 'ર', 'લ',
                    'ળ', 'ળ', 'વ', 'શ', 'ષ', 'સ', 'હ', '\0', '\u0abe', '\u0abf',
                    '\u0ac0', '\u0ac1', '\u0ac2', '\u0ac3', '\u0ac7', '\u0ac7', '\u0ac8', '\u0ac5', '\u0acb', '\u0acb',
                    '\u0acc', '\u0ac9', '\u0acd', '\u0abc', '.', '\0', '\0', '\0', '\0', '\0',
                    '\0', '૦', '૧', '૨', '૩', '૪', '૫', '૬', '૭', '૮',
                    '૯', '\0', '\0', '\0', '\0', '\0'
                },
                {
                    '\0', 'ૐ', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    'ૠ', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\u0ac4', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\u200c', '\u200d', 'ઽ', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\0', '\0', '\0'
                }
            },
            {
                {
                    '\0', '\0', '\u0a02', '\0', 'ਅ', 'ਆ', 'ਇ', 'ਈ', 'ਉ', 'ਊ',
                    '\0', 'ਏ', 'ਏ', 'ਐ', 'ਐ', 'ਐ', 'ਓ', 'ਔ', 'ਔ', 'ਕ',
                    'ਖ', 'ਗ', 'ਘ', 'ਙ', 'ਚ', 'ਛ', 'ਜ', 'ਝ', 'ਞ', 'ਟ',
                    'ਠ', 'ਡ', 'ਢ', 'ਣ', 'ਤ', 'ਥ', 'ਦ', 'ਧ', 'ਨ', 'ਨ',
                    'ਪ', 'ਫ', 'ਬ', 'ਭ', 'ਮ', 'ਯ', 'ਯ', 'ਰ', 'ਰ', 'ਲ',
                    'ਲ਼', 'ਲ਼', 'ਵ', 'ਸ਼', 'ਸ਼', 'ਸ', 'ਹ', '\0', '\u0a3e', '\u0a3f',
                    '\u0a40', '\u0a41', '\u0a42', '\0', '\u0a47', '\u0a47', '\u0a48', '\u0a48', '\u0a4b', '\u0a4b',
                    '\u0a4c', '\u0a4c', '\u0a4d', '\u0a3c', '.', '\0', '\0', '\0', '\0', '\0',
                    '\0', '੦', '੧', '੨', '੩', '੪', '੫', '੬', '੭', '੮',
                    '੯', '\0', '\0', '\0', '\0', '\0'
                },
                {
                    '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    'ਖ਼', 'ਗ਼', '\0', '\0', '\0', '\0', 'ਜ਼', '\0', '\0', '\0',
                    '\0', '\0', 'ੜ', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', 'ਫ਼', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\u200c', '\u200d', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                    '\0', '\0', '\0', '\0', '\0', '\0'
                }
            }
        };

        private static ReadOnlySpan<byte> SecondIndicByte => new byte[4] { 0, 233, 184, 191 };

        public ISCIIEncoding(int codePage)
            : base(codePage)
        {
            _defaultCodePage = codePage - 57000;
            if (_defaultCodePage < 2 || _defaultCodePage > 11)
            {
                throw new ArgumentException(System.SR.Format(MDCFR.Properties.Resources.Argument_CodepageNotSupported, codePage), "codePage");
            }
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new PlatformNotSupportedException();
        }

        public override int GetMaxByteCount(int charCount)
        {
            if (charCount < 0)
            {
                throw new ArgumentOutOfRangeException("charCount", MDCFR.Properties.Resources.ArgumentOutOfRange_NeedNonNegNum);
            }
            long num = (long)charCount + 1L;
            if (base.EncoderFallback.MaxCharCount > 1)
            {
                num *= base.EncoderFallback.MaxCharCount;
            }
            num *= 4;
            if (num > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException("charCount", MDCFR.Properties.Resources.ArgumentOutOfRange_GetByteCountOverflow);
            }
            return (int)num;
        }

        public override int GetMaxCharCount(int byteCount)
        {
            if (byteCount < 0)
            {
                throw new ArgumentOutOfRangeException("byteCount", MDCFR.Properties.Resources.ArgumentOutOfRange_NeedNonNegNum);
            }
            long num = (long)byteCount + 1L;
            if (base.DecoderFallback.MaxCharCount > 1)
            {
                num *= base.DecoderFallback.MaxCharCount;
            }
            if (num > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException("byteCount", MDCFR.Properties.Resources.ArgumentOutOfRange_GetCharCountOverflow);
            }
            return (int)num;
        }

        public unsafe override int GetByteCount(char* chars, int count, System.Text.EncoderNLS baseEncoder)
        {
            return GetBytes(chars, count, null, 0, baseEncoder);
        }

        public unsafe override int GetBytes(char* chars, int charCount, byte* bytes, int byteCount, System.Text.EncoderNLS baseEncoder)
        {
            ISCIIEncoder iSCIIEncoder = (ISCIIEncoder)baseEncoder;
            EncodingByteBuffer encodingByteBuffer = new EncodingByteBuffer(this, iSCIIEncoder, bytes, byteCount, chars, charCount);
            int num = _defaultCodePage;
            bool flag = false;
            if (iSCIIEncoder != null)
            {
                num = iSCIIEncoder.currentCodePage;
                flag = iSCIIEncoder.bLastVirama;
                if (iSCIIEncoder.charLeftOver > '\0')
                {
                    encodingByteBuffer.Fallback(iSCIIEncoder.charLeftOver);
                    flag = false;
                }
            }
            while (encodingByteBuffer.MoreData)
            {
                char nextChar = encodingByteBuffer.GetNextChar();
                if (nextChar < '\u00a0')
                {
                    if (!encodingByteBuffer.AddByte((byte)nextChar))
                    {
                        break;
                    }
                    flag = false;
                    continue;
                }
                if (nextChar < '\u0901' || nextChar > '൯')
                {
                    if (flag && (nextChar == '\u200c' || nextChar == '\u200d'))
                    {
                        if (nextChar == '\u200c')
                        {
                            if (!encodingByteBuffer.AddByte(232))
                            {
                                break;
                            }
                        }
                        else if (!encodingByteBuffer.AddByte(233))
                        {
                            break;
                        }
                        flag = false;
                    }
                    else
                    {
                        encodingByteBuffer.Fallback(nextChar);
                        flag = false;
                    }
                    continue;
                }
                int num2 = s_UnicodeToIndicChar[nextChar - 2305];
                byte b = (byte)num2;
                int num3 = 0xF & (num2 >> 8);
                int num4 = 0xF000 & num2;
                if (num2 == 0)
                {
                    encodingByteBuffer.Fallback(nextChar);
                    flag = false;
                    continue;
                }
                if (num3 != num)
                {
                    if (!encodingByteBuffer.AddByte(239, (byte)((uint)num3 | 0x40u)))
                    {
                        break;
                    }
                    num = num3;
                }
                if (!encodingByteBuffer.AddByte(b, (num4 != 0) ? 1 : 0))
                {
                    break;
                }
                flag = b == 232;
                if (num4 != 0 && !encodingByteBuffer.AddByte(SecondIndicByte[num4 >> 12]))
                {
                    break;
                }
            }
            if (num != _defaultCodePage && (iSCIIEncoder == null || iSCIIEncoder.MustFlush))
            {
                if (encodingByteBuffer.AddByte(239, (byte)((uint)_defaultCodePage | 0x40u)))
                {
                    num = _defaultCodePage;
                }
                else
                {
                    encodingByteBuffer.GetNextChar();
                }
                flag = false;
            }
            if (iSCIIEncoder != null && bytes != null)
            {
                if (!encodingByteBuffer.fallbackBufferHelper.bUsedEncoder)
                {
                    iSCIIEncoder.charLeftOver = '\0';
                }
                iSCIIEncoder.currentCodePage = num;
                iSCIIEncoder.bLastVirama = flag;
                iSCIIEncoder.m_charsUsed = encodingByteBuffer.CharsUsed;
            }
            return encodingByteBuffer.Count;
        }

        public unsafe override int GetCharCount(byte* bytes, int count, System.Text.DecoderNLS baseDecoder)
        {
            return GetChars(bytes, count, null, 0, baseDecoder);
        }

        public unsafe override int GetChars(byte* bytes, int byteCount, char* chars, int charCount, System.Text.DecoderNLS baseDecoder)
        {
            ISCIIDecoder iSCIIDecoder = (ISCIIDecoder)baseDecoder;
            EncodingCharBuffer encodingCharBuffer = new EncodingCharBuffer(this, iSCIIDecoder, chars, charCount, bytes, byteCount);
            int num = _defaultCodePage;
            bool flag = false;
            bool flag2 = false;
            bool flag3 = false;
            char c = '\0';
            char c2 = '\0';
            if (iSCIIDecoder != null)
            {
                num = iSCIIDecoder.currentCodePage;
                flag = iSCIIDecoder.bLastATR;
                flag2 = iSCIIDecoder.bLastVirama;
                flag3 = iSCIIDecoder.bLastDevenagariStressAbbr;
                c = iSCIIDecoder.cLastCharForNextNukta;
                c2 = iSCIIDecoder.cLastCharForNoNextNukta;
            }
            bool flag4 = flag2 || flag || flag3 || c != '\0';
            int num2 = -1;
            if (num >= 2 && num <= 11)
            {
                num2 = s_IndicMappingIndex[num];
            }
            while (encodingCharBuffer.MoreData)
            {
                byte nextByte = encodingCharBuffer.GetNextByte();
                if (flag4)
                {
                    flag4 = false;
                    if (flag)
                    {
                        if (nextByte >= 66 && nextByte <= 75)
                        {
                            num = nextByte & 0xF;
                            num2 = s_IndicMappingIndex[num];
                            flag = false;
                            continue;
                        }
                        if (nextByte == 64)
                        {
                            num = _defaultCodePage;
                            num2 = -1;
                            if (num >= 2 && num <= 11)
                            {
                                num2 = s_IndicMappingIndex[num];
                            }
                            flag = false;
                            continue;
                        }
                        if (nextByte == 65)
                        {
                            num = _defaultCodePage;
                            num2 = -1;
                            if (num >= 2 && num <= 11)
                            {
                                num2 = s_IndicMappingIndex[num];
                            }
                            flag = false;
                            continue;
                        }
                        if (!encodingCharBuffer.Fallback(239))
                        {
                            break;
                        }
                        flag = false;
                    }
                    else if (flag2)
                    {
                        if (nextByte == 232)
                        {
                            if (!encodingCharBuffer.AddChar('\u200c'))
                            {
                                break;
                            }
                            flag2 = false;
                            continue;
                        }
                        if (nextByte == 233)
                        {
                            if (!encodingCharBuffer.AddChar('\u200d'))
                            {
                                break;
                            }
                            flag2 = false;
                            continue;
                        }
                        flag2 = false;
                    }
                    else if (flag3)
                    {
                        if (nextByte == 184)
                        {
                            if (!encodingCharBuffer.AddChar('\u0952'))
                            {
                                break;
                            }
                            flag3 = false;
                            continue;
                        }
                        if (nextByte == 191)
                        {
                            if (!encodingCharBuffer.AddChar('॰'))
                            {
                                break;
                            }
                            flag3 = false;
                            continue;
                        }
                        if (!encodingCharBuffer.Fallback(240))
                        {
                            break;
                        }
                        flag3 = false;
                    }
                    else
                    {
                        if (nextByte == 233)
                        {
                            if (!encodingCharBuffer.AddChar(c))
                            {
                                break;
                            }
                            c = (c2 = '\0');
                            continue;
                        }
                        if (!encodingCharBuffer.AddChar(c2))
                        {
                            break;
                        }
                        c = (c2 = '\0');
                    }
                }
                if (nextByte < 160)
                {
                    if (!encodingCharBuffer.AddChar((char)nextByte))
                    {
                        break;
                    }
                    continue;
                }
                if (nextByte == 239)
                {
                    flag = (flag4 = true);
                    continue;
                }
                char c3 = s_IndicMapping[num2, 0, nextByte - 160];
                char c4 = s_IndicMapping[num2, 1, nextByte - 160];
                if (c4 == '\0' || nextByte == 233)
                {
                    if (c3 == '\0')
                    {
                        if (!encodingCharBuffer.Fallback(nextByte))
                        {
                            break;
                        }
                    }
                    else if (!encodingCharBuffer.AddChar(c3))
                    {
                        break;
                    }
                }
                else if (nextByte == 232)
                {
                    if (!encodingCharBuffer.AddChar(c3))
                    {
                        break;
                    }
                    flag2 = (flag4 = true);
                }
                else if ((c4 & 0xF000) == 0)
                {
                    flag4 = true;
                    c = c4;
                    c2 = c3;
                }
                else
                {
                    flag3 = (flag4 = true);
                }
            }
            if (iSCIIDecoder == null || iSCIIDecoder.MustFlush)
            {
                if (flag)
                {
                    if (encodingCharBuffer.Fallback(239))
                    {
                        flag = false;
                    }
                    else
                    {
                        encodingCharBuffer.GetNextByte();
                    }
                }
                else if (flag3)
                {
                    if (encodingCharBuffer.Fallback(240))
                    {
                        flag3 = false;
                    }
                    else
                    {
                        encodingCharBuffer.GetNextByte();
                    }
                }
                else if (c2 != 0)
                {
                    if (encodingCharBuffer.AddChar(c2))
                    {
                        c2 = (c = '\0');
                    }
                    else
                    {
                        encodingCharBuffer.GetNextByte();
                    }
                }
            }
            if (iSCIIDecoder != null && chars != null)
            {
                if (!iSCIIDecoder.MustFlush || c2 != '\0' || flag || flag3)
                {
                    iSCIIDecoder.currentCodePage = num;
                    iSCIIDecoder.bLastVirama = flag2;
                    iSCIIDecoder.bLastATR = flag;
                    iSCIIDecoder.bLastDevenagariStressAbbr = flag3;
                    iSCIIDecoder.cLastCharForNextNukta = c;
                    iSCIIDecoder.cLastCharForNoNextNukta = c2;
                }
                else
                {
                    iSCIIDecoder.currentCodePage = _defaultCodePage;
                    iSCIIDecoder.bLastVirama = false;
                    iSCIIDecoder.bLastATR = false;
                    iSCIIDecoder.bLastDevenagariStressAbbr = false;
                    iSCIIDecoder.cLastCharForNextNukta = '\0';
                    iSCIIDecoder.cLastCharForNoNextNukta = '\0';
                }
                iSCIIDecoder.m_bytesUsed = encodingCharBuffer.BytesUsed;
            }
            return encodingCharBuffer.Count;
        }

        public override Decoder GetDecoder()
        {
            return new ISCIIDecoder(this);
        }

        public override Encoder GetEncoder()
        {
            return new ISCIIEncoder(this);
        }

        public override int GetHashCode()
        {
            return _defaultCodePage + base.EncoderFallback.GetHashCode() + base.DecoderFallback.GetHashCode();
        }
    }

    internal sealed class ISO2022Encoding : System.Text.DBCSCodePageEncoding
    {
        internal enum ISO2022Modes
        {
            ModeHalfwidthKatakana = 0,
            ModeJIS0208 = 1,
            ModeKR = 5,
            ModeHZ = 6,
            ModeGB2312 = 7,
            ModeCNS11643_1 = 9,
            ModeCNS11643_2 = 10,
            ModeASCII = 11,
            ModeIncompleteEscape = -1,
            ModeInvalidEscape = -2,
            ModeNOOP = -3
        }

        internal sealed class ISO2022Encoder : System.Text.EncoderNLS
        {
            internal ISO2022Modes currentMode;

            internal ISO2022Modes shiftInOutMode;

            internal override bool HasState
            {
                get
                {
                    if (charLeftOver == '\0')
                    {
                        return currentMode != ISO2022Modes.ModeASCII;
                    }
                    return true;
                }
            }

            internal ISO2022Encoder(System.Text.EncodingNLS encoding)
                : base(encoding)
            {
            }

            public override void Reset()
            {
                currentMode = ISO2022Modes.ModeASCII;
                shiftInOutMode = ISO2022Modes.ModeASCII;
                charLeftOver = '\0';
                m_fallbackBuffer?.Reset();
            }
        }

        internal sealed class ISO2022Decoder : System.Text.DecoderNLS
        {
            internal byte[] bytesLeftOver;

            internal int bytesLeftOverCount;

            internal ISO2022Modes currentMode;

            internal ISO2022Modes shiftInOutMode;

            internal override bool HasState
            {
                get
                {
                    if (bytesLeftOverCount == 0)
                    {
                        return currentMode != ISO2022Modes.ModeASCII;
                    }
                    return true;
                }
            }

            internal ISO2022Decoder(System.Text.EncodingNLS encoding)
                : base(encoding)
            {
            }

            public override void Reset()
            {
                bytesLeftOverCount = 0;
                bytesLeftOver = new byte[4];
                currentMode = ISO2022Modes.ModeASCII;
                shiftInOutMode = ISO2022Modes.ModeASCII;
                m_fallbackBuffer?.Reset();
            }
        }

        private const byte SHIFT_OUT = 14;

        private const byte SHIFT_IN = 15;

        private const byte ESCAPE = 27;

        private const byte LEADBYTE_HALFWIDTH = 16;

        private static readonly int[] s_tableBaseCodePages = new int[12]
        {
            932, 932, 932, 0, 0, 949, 936, 0, 0, 0,
            0, 0
        };

        private static readonly ushort[] s_HalfToFullWidthKanaTable = new ushort[63]
        {
            41379, 41430, 41431, 41378, 41382, 42482, 42401, 42403, 42405, 42407,
            42409, 42467, 42469, 42471, 42435, 41404, 42402, 42404, 42406, 42408,
            42410, 42411, 42413, 42415, 42417, 42419, 42421, 42423, 42425, 42427,
            42429, 42431, 42433, 42436, 42438, 42440, 42442, 42443, 42444, 42445,
            42446, 42447, 42450, 42453, 42456, 42459, 42462, 42463, 42464, 42465,
            42466, 42468, 42470, 42472, 42473, 42474, 42475, 42476, 42477, 42479,
            42483, 41387, 41388
        };

        internal ISO2022Encoding(int codePage)
            : base(codePage, s_tableBaseCodePages[codePage % 10])
        {
        }

        protected override bool CleanUpBytes(ref int bytes)
        {
            switch (CodePage)
            {
                case 50220:
                case 50221:
                case 50222:
                    if (bytes >= 256)
                    {
                        if (bytes >= 64064 && bytes <= 64587)
                        {
                            if (bytes >= 64064 && bytes <= 64091)
                            {
                                if (bytes <= 64073)
                                {
                                    bytes -= 2897;
                                }
                                else if (bytes >= 64074 && bytes <= 64083)
                                {
                                    bytes -= 29430;
                                }
                                else if (bytes >= 64084 && bytes <= 64087)
                                {
                                    bytes -= 2907;
                                }
                                else if (bytes == 64088)
                                {
                                    bytes = 34698;
                                }
                                else if (bytes == 64089)
                                {
                                    bytes = 34690;
                                }
                                else if (bytes == 64090)
                                {
                                    bytes = 34692;
                                }
                                else if (bytes == 64091)
                                {
                                    bytes = 34714;
                                }
                            }
                            else if (bytes >= 64092 && bytes <= 64587)
                            {
                                byte b = (byte)bytes;
                                if (b < 92)
                                {
                                    bytes -= 3423;
                                }
                                else if (b >= 128 && b <= 155)
                                {
                                    bytes -= 3357;
                                }
                                else
                                {
                                    bytes -= 3356;
                                }
                            }
                        }
                        byte b2 = (byte)(bytes >> 8);
                        byte b3 = (byte)bytes;
                        b2 = (byte)(b2 - ((b2 > 159) ? 177 : 113));
                        b2 = (byte)((b2 << 1) + 1);
                        if (b3 > 158)
                        {
                            b3 = (byte)(b3 - 126);
                            b2 = (byte)(b2 + 1);
                        }
                        else
                        {
                            if (b3 > 126)
                            {
                                b3 = (byte)(b3 - 1);
                            }
                            b3 = (byte)(b3 - 31);
                        }
                        bytes = (b2 << 8) | b3;
                    }
                    else
                    {
                        if (bytes >= 161 && bytes <= 223)
                        {
                            bytes += 3968;
                        }
                        if (bytes >= 129 && (bytes <= 159 || (bytes >= 224 && bytes <= 252)))
                        {
                            return false;
                        }
                    }
                    break;
                case 50225:
                    if (bytes >= 128 && bytes <= 255)
                    {
                        return false;
                    }
                    if (bytes >= 256 && ((bytes & 0xFF) < 161 || (bytes & 0xFF) == 255 || (bytes & 0xFF00) < 41216 || (bytes & 0xFF00) == 65280))
                    {
                        return false;
                    }
                    bytes &= 32639;
                    break;
                case 52936:
                    if (bytes >= 129 && bytes <= 254)
                    {
                        return false;
                    }
                    break;
            }
            return true;
        }

        public unsafe override int GetByteCount(char* chars, int count, System.Text.EncoderNLS baseEncoder)
        {
            return GetBytes(chars, count, null, 0, baseEncoder);
        }

        public unsafe override int GetBytes(char* chars, int charCount, byte* bytes, int byteCount, System.Text.EncoderNLS baseEncoder)
        {
            ISO2022Encoder encoder = (ISO2022Encoder)baseEncoder;
            int result = 0;
            switch (CodePage)
            {
                case 50220:
                case 50221:
                case 50222:
                    result = GetBytesCP5022xJP(chars, charCount, bytes, byteCount, encoder);
                    break;
                case 50225:
                    result = GetBytesCP50225KR(chars, charCount, bytes, byteCount, encoder);
                    break;
                case 52936:
                    result = GetBytesCP52936(chars, charCount, bytes, byteCount, encoder);
                    break;
            }
            return result;
        }

        public unsafe override int GetCharCount(byte* bytes, int count, System.Text.DecoderNLS baseDecoder)
        {
            return GetChars(bytes, count, null, 0, baseDecoder);
        }

        public unsafe override int GetChars(byte* bytes, int byteCount, char* chars, int charCount, System.Text.DecoderNLS baseDecoder)
        {
            ISO2022Decoder decoder = (ISO2022Decoder)baseDecoder;
            int result = 0;
            switch (CodePage)
            {
                case 50220:
                case 50221:
                case 50222:
                    result = GetCharsCP5022xJP(bytes, byteCount, chars, charCount, decoder);
                    break;
                case 50225:
                    result = GetCharsCP50225KR(bytes, byteCount, chars, charCount, decoder);
                    break;
                case 52936:
                    result = GetCharsCP52936(bytes, byteCount, chars, charCount, decoder);
                    break;
            }
            return result;
        }

        private unsafe int GetBytesCP5022xJP(char* chars, int charCount, byte* bytes, int byteCount, ISO2022Encoder encoder)
        {
            EncodingByteBuffer encodingByteBuffer = new EncodingByteBuffer(this, encoder, bytes, byteCount, chars, charCount);
            ISO2022Modes iSO2022Modes = ISO2022Modes.ModeASCII;
            ISO2022Modes iSO2022Modes2 = ISO2022Modes.ModeASCII;
            if (encoder != null)
            {
                char charLeftOver = encoder.charLeftOver;
                iSO2022Modes = encoder.currentMode;
                iSO2022Modes2 = encoder.shiftInOutMode;
                if (charLeftOver > '\0')
                {
                    encodingByteBuffer.Fallback(charLeftOver);
                }
            }
            while (encodingByteBuffer.MoreData)
            {
                char nextChar = encodingByteBuffer.GetNextChar();
                ushort num = mapUnicodeToBytes[(int)nextChar];
                byte b;
                byte b2;
                while (true)
                {
                    b = (byte)(num >> 8);
                    b2 = (byte)(num & 0xFFu);
                    if (b != 16)
                    {
                        break;
                    }
                    if (CodePage == 50220)
                    {
                        if (b2 >= 33 && b2 < 33 + s_HalfToFullWidthKanaTable.Length)
                        {
                            num = (ushort)(s_HalfToFullWidthKanaTable[b2 - 33] & 0x7F7Fu);
                            continue;
                        }
                        goto IL_009a;
                    }
                    goto IL_00be;
                }
                if (b != 0)
                {
                    if (CodePage == 50222 && iSO2022Modes == ISO2022Modes.ModeHalfwidthKatakana)
                    {
                        if (!encodingByteBuffer.AddByte(15))
                        {
                            break;
                        }
                        iSO2022Modes = iSO2022Modes2;
                    }
                    if (iSO2022Modes != ISO2022Modes.ModeJIS0208)
                    {
                        if (!encodingByteBuffer.AddByte((byte)27, (byte)36, (byte)66))
                        {
                            break;
                        }
                        iSO2022Modes = ISO2022Modes.ModeJIS0208;
                    }
                    if (!encodingByteBuffer.AddByte(b, b2))
                    {
                        break;
                    }
                }
                else if (num != 0 || nextChar == '\0')
                {
                    if (CodePage == 50222 && iSO2022Modes == ISO2022Modes.ModeHalfwidthKatakana)
                    {
                        if (!encodingByteBuffer.AddByte(15))
                        {
                            break;
                        }
                        iSO2022Modes = iSO2022Modes2;
                    }
                    if (iSO2022Modes != ISO2022Modes.ModeASCII)
                    {
                        if (!encodingByteBuffer.AddByte((byte)27, (byte)40, (byte)66))
                        {
                            break;
                        }
                        iSO2022Modes = ISO2022Modes.ModeASCII;
                    }
                    if (!encodingByteBuffer.AddByte(b2))
                    {
                        break;
                    }
                }
                else
                {
                    encodingByteBuffer.Fallback(nextChar);
                }
                continue;
            IL_009a:
                encodingByteBuffer.Fallback(nextChar);
                continue;
            IL_00be:
                if (iSO2022Modes != 0)
                {
                    if (CodePage == 50222)
                    {
                        if (!encodingByteBuffer.AddByte(14))
                        {
                            break;
                        }
                        iSO2022Modes2 = iSO2022Modes;
                        iSO2022Modes = ISO2022Modes.ModeHalfwidthKatakana;
                    }
                    else
                    {
                        if (!encodingByteBuffer.AddByte((byte)27, (byte)40, (byte)73))
                        {
                            break;
                        }
                        iSO2022Modes = ISO2022Modes.ModeHalfwidthKatakana;
                    }
                }
                if (!encodingByteBuffer.AddByte((byte)(b2 & 0x7Fu)))
                {
                    break;
                }
            }
            if (iSO2022Modes != ISO2022Modes.ModeASCII && (encoder == null || encoder.MustFlush))
            {
                if (CodePage == 50222 && iSO2022Modes == ISO2022Modes.ModeHalfwidthKatakana)
                {
                    if (encodingByteBuffer.AddByte(15))
                    {
                        iSO2022Modes = iSO2022Modes2;
                    }
                    else
                    {
                        encodingByteBuffer.GetNextChar();
                    }
                }
                if (iSO2022Modes != ISO2022Modes.ModeASCII && (CodePage != 50222 || iSO2022Modes != 0))
                {
                    if (encodingByteBuffer.AddByte((byte)27, (byte)40, (byte)66))
                    {
                        iSO2022Modes = ISO2022Modes.ModeASCII;
                    }
                    else
                    {
                        encodingByteBuffer.GetNextChar();
                    }
                }
            }
            if (bytes != null && encoder != null)
            {
                encoder.currentMode = iSO2022Modes;
                encoder.shiftInOutMode = iSO2022Modes2;
                if (!encodingByteBuffer.fallbackBufferHelper.bUsedEncoder)
                {
                    encoder.charLeftOver = '\0';
                }
                encoder.m_charsUsed = encodingByteBuffer.CharsUsed;
            }
            return encodingByteBuffer.Count;
        }

        private unsafe int GetBytesCP50225KR(char* chars, int charCount, byte* bytes, int byteCount, ISO2022Encoder encoder)
        {
            EncodingByteBuffer encodingByteBuffer = new EncodingByteBuffer(this, encoder, bytes, byteCount, chars, charCount);
            ISO2022Modes iSO2022Modes = ISO2022Modes.ModeASCII;
            ISO2022Modes iSO2022Modes2 = ISO2022Modes.ModeASCII;
            if (encoder != null)
            {
                char charLeftOver = encoder.charLeftOver;
                iSO2022Modes = encoder.currentMode;
                iSO2022Modes2 = encoder.shiftInOutMode;
                if (charLeftOver > '\0')
                {
                    encodingByteBuffer.Fallback(charLeftOver);
                }
            }
            while (encodingByteBuffer.MoreData)
            {
                char nextChar = encodingByteBuffer.GetNextChar();
                ushort num = mapUnicodeToBytes[(int)nextChar];
                byte b = (byte)(num >> 8);
                byte b2 = (byte)(num & 0xFFu);
                if (b != 0)
                {
                    if (iSO2022Modes2 != ISO2022Modes.ModeKR)
                    {
                        if (!encodingByteBuffer.AddByte((byte)27, (byte)36, (byte)41, (byte)67))
                        {
                            break;
                        }
                        iSO2022Modes2 = ISO2022Modes.ModeKR;
                    }
                    if (iSO2022Modes != ISO2022Modes.ModeKR)
                    {
                        if (!encodingByteBuffer.AddByte(14))
                        {
                            break;
                        }
                        iSO2022Modes = ISO2022Modes.ModeKR;
                    }
                    if (!encodingByteBuffer.AddByte(b, b2))
                    {
                        break;
                    }
                }
                else if (num != 0 || nextChar == '\0')
                {
                    if (iSO2022Modes != ISO2022Modes.ModeASCII)
                    {
                        if (!encodingByteBuffer.AddByte(15))
                        {
                            break;
                        }
                        iSO2022Modes = ISO2022Modes.ModeASCII;
                    }
                    if (!encodingByteBuffer.AddByte(b2))
                    {
                        break;
                    }
                }
                else
                {
                    encodingByteBuffer.Fallback(nextChar);
                }
            }
            if (iSO2022Modes != ISO2022Modes.ModeASCII && (encoder == null || encoder.MustFlush))
            {
                if (encodingByteBuffer.AddByte(15))
                {
                    iSO2022Modes = ISO2022Modes.ModeASCII;
                }
                else
                {
                    encodingByteBuffer.GetNextChar();
                }
            }
            if (bytes != null && encoder != null)
            {
                if (!encodingByteBuffer.fallbackBufferHelper.bUsedEncoder)
                {
                    encoder.charLeftOver = '\0';
                }
                encoder.currentMode = iSO2022Modes;
                if (!encoder.MustFlush || encoder.charLeftOver != 0)
                {
                    encoder.shiftInOutMode = iSO2022Modes2;
                }
                else
                {
                    encoder.shiftInOutMode = ISO2022Modes.ModeASCII;
                }
                encoder.m_charsUsed = encodingByteBuffer.CharsUsed;
            }
            return encodingByteBuffer.Count;
        }

        private unsafe int GetBytesCP52936(char* chars, int charCount, byte* bytes, int byteCount, ISO2022Encoder encoder)
        {
            EncodingByteBuffer encodingByteBuffer = new EncodingByteBuffer(this, encoder, bytes, byteCount, chars, charCount);
            ISO2022Modes iSO2022Modes = ISO2022Modes.ModeASCII;
            if (encoder != null)
            {
                char charLeftOver = encoder.charLeftOver;
                iSO2022Modes = encoder.currentMode;
                if (charLeftOver > '\0')
                {
                    encodingByteBuffer.Fallback(charLeftOver);
                }
            }
            while (encodingByteBuffer.MoreData)
            {
                char nextChar = encodingByteBuffer.GetNextChar();
                ushort num = mapUnicodeToBytes[(int)nextChar];
                if (num == 0 && nextChar != 0)
                {
                    encodingByteBuffer.Fallback(nextChar);
                    continue;
                }
                byte b = (byte)(num >> 8);
                byte b2 = (byte)(num & 0xFFu);
                if ((b != 0 && (b < 161 || b > 247 || b2 < 161 || b2 > 254)) || (b == 0 && b2 > 128 && b2 != byte.MaxValue))
                {
                    encodingByteBuffer.Fallback(nextChar);
                    continue;
                }
                if (b != 0)
                {
                    if (iSO2022Modes != ISO2022Modes.ModeHZ)
                    {
                        if (!encodingByteBuffer.AddByte(126, 123, 2))
                        {
                            break;
                        }
                        iSO2022Modes = ISO2022Modes.ModeHZ;
                    }
                    if (encodingByteBuffer.AddByte((byte)(b & 0x7Fu), (byte)(b2 & 0x7Fu)))
                    {
                        continue;
                    }
                    break;
                }
                if (iSO2022Modes != ISO2022Modes.ModeASCII)
                {
                    if (!encodingByteBuffer.AddByte(126, 125, (b2 != 126) ? 1 : 2))
                    {
                        break;
                    }
                    iSO2022Modes = ISO2022Modes.ModeASCII;
                }
                if ((b2 == 126 && !encodingByteBuffer.AddByte(126, 1)) || !encodingByteBuffer.AddByte(b2))
                {
                    break;
                }
            }
            if (iSO2022Modes != ISO2022Modes.ModeASCII && (encoder == null || encoder.MustFlush))
            {
                if (encodingByteBuffer.AddByte((byte)126, (byte)125))
                {
                    iSO2022Modes = ISO2022Modes.ModeASCII;
                }
                else
                {
                    encodingByteBuffer.GetNextChar();
                }
            }
            if (encoder != null && bytes != null)
            {
                encoder.currentMode = iSO2022Modes;
                if (!encodingByteBuffer.fallbackBufferHelper.bUsedEncoder)
                {
                    encoder.charLeftOver = '\0';
                }
                encoder.m_charsUsed = encodingByteBuffer.CharsUsed;
            }
            return encodingByteBuffer.Count;
        }

        private unsafe int GetCharsCP5022xJP(byte* bytes, int byteCount, char* chars, int charCount, ISO2022Decoder decoder)
        {
            EncodingCharBuffer encodingCharBuffer = new EncodingCharBuffer(this, decoder, chars, charCount, bytes, byteCount);
            ISO2022Modes iSO2022Modes = ISO2022Modes.ModeASCII;
            ISO2022Modes iSO2022Modes2 = ISO2022Modes.ModeASCII;
            byte[] bytes2 = new byte[4];
            int count = 0;
            if (decoder != null)
            {
                iSO2022Modes = decoder.currentMode;
                iSO2022Modes2 = decoder.shiftInOutMode;
                count = decoder.bytesLeftOverCount;
                for (int i = 0; i < count; i++)
                {
                    bytes2[i] = decoder.bytesLeftOver[i];
                }
            }
            while (encodingCharBuffer.MoreData || count > 0)
            {
                byte b;
                if (count > 0)
                {
                    if (bytes2[0] == 27)
                    {
                        if (!encodingCharBuffer.MoreData)
                        {
                            if (decoder != null && !decoder.MustFlush)
                            {
                                break;
                            }
                        }
                        else
                        {
                            bytes2[count++] = encodingCharBuffer.GetNextByte();
                            ISO2022Modes iSO2022Modes3 = CheckEscapeSequenceJP(bytes2, count);
                            switch (iSO2022Modes3)
                            {
                                default:
                                    count = 0;
                                    iSO2022Modes = (iSO2022Modes2 = iSO2022Modes3);
                                    continue;
                                case ISO2022Modes.ModeInvalidEscape:
                                    break;
                                case ISO2022Modes.ModeIncompleteEscape:
                                    continue;
                            }
                        }
                    }
                    b = DecrementEscapeBytes(ref bytes2, ref count);
                }
                else
                {
                    b = encodingCharBuffer.GetNextByte();
                    if (b == 27)
                    {
                        if (count == 0)
                        {
                            bytes2[0] = b;
                            count = 1;
                            continue;
                        }
                        encodingCharBuffer.AdjustBytes(-1);
                    }
                }
                switch (b)
                {
                    case 14:
                        iSO2022Modes2 = iSO2022Modes;
                        iSO2022Modes = ISO2022Modes.ModeHalfwidthKatakana;
                        continue;
                    case 15:
                        iSO2022Modes = iSO2022Modes2;
                        continue;
                }
                ushort num = b;
                bool flag = false;
                if (iSO2022Modes == ISO2022Modes.ModeJIS0208)
                {
                    if (count > 0)
                    {
                        if (bytes2[0] != 27)
                        {
                            num = (ushort)(num << 8);
                            num = (ushort)(num | DecrementEscapeBytes(ref bytes2, ref count));
                            flag = true;
                        }
                    }
                    else
                    {
                        if (!encodingCharBuffer.MoreData)
                        {
                            if (decoder == null || decoder.MustFlush)
                            {
                                encodingCharBuffer.Fallback(b);
                            }
                            else if (chars != null)
                            {
                                bytes2[0] = b;
                                count = 1;
                            }
                            break;
                        }
                        num = (ushort)(num << 8);
                        num = (ushort)(num | encodingCharBuffer.GetNextByte());
                        flag = true;
                    }
                    if (flag && (num & 0xFF00) == 10752)
                    {
                        num = (ushort)(num & 0xFFu);
                        num = (ushort)(num | 0x1000u);
                    }
                }
                else if (num >= 161 && num <= 223)
                {
                    num = (ushort)(num | 0x1000u);
                    num = (ushort)(num & 0xFF7Fu);
                }
                else if (iSO2022Modes == ISO2022Modes.ModeHalfwidthKatakana)
                {
                    num = (ushort)(num | 0x1000u);
                }
                char c = mapBytesToUnicode[(int)num];
                if (c == '\0' && num != 0)
                {
                    if (flag)
                    {
                        if (!encodingCharBuffer.Fallback((byte)(num >> 8), (byte)num))
                        {
                            break;
                        }
                    }
                    else if (!encodingCharBuffer.Fallback(b))
                    {
                        break;
                    }
                }
                else if (!encodingCharBuffer.AddChar(c, (!flag) ? 1 : 2))
                {
                    break;
                }
            }
            if (chars != null && decoder != null)
            {
                if (!decoder.MustFlush || count != 0)
                {
                    decoder.currentMode = iSO2022Modes;
                    decoder.shiftInOutMode = iSO2022Modes2;
                    decoder.bytesLeftOverCount = count;
                    decoder.bytesLeftOver = bytes2;
                }
                else
                {
                    decoder.currentMode = ISO2022Modes.ModeASCII;
                    decoder.shiftInOutMode = ISO2022Modes.ModeASCII;
                    decoder.bytesLeftOverCount = 0;
                }
                decoder.m_bytesUsed = encodingCharBuffer.BytesUsed;
            }
            return encodingCharBuffer.Count;
        }

        private static ISO2022Modes CheckEscapeSequenceJP(byte[] bytes, int escapeCount)
        {
            if (bytes[0] != 27)
            {
                return ISO2022Modes.ModeInvalidEscape;
            }
            if (escapeCount < 3)
            {
                return ISO2022Modes.ModeIncompleteEscape;
            }
            if (bytes[1] == 40)
            {
                if (bytes[2] == 66)
                {
                    return ISO2022Modes.ModeASCII;
                }
                if (bytes[2] == 72)
                {
                    return ISO2022Modes.ModeASCII;
                }
                if (bytes[2] == 74)
                {
                    return ISO2022Modes.ModeASCII;
                }
                if (bytes[2] == 73)
                {
                    return ISO2022Modes.ModeHalfwidthKatakana;
                }
            }
            else if (bytes[1] == 36)
            {
                if (bytes[2] == 64 || bytes[2] == 66)
                {
                    return ISO2022Modes.ModeJIS0208;
                }
                if (escapeCount < 4)
                {
                    return ISO2022Modes.ModeIncompleteEscape;
                }
                if (bytes[2] == 40 && bytes[3] == 68)
                {
                    return ISO2022Modes.ModeJIS0208;
                }
            }
            else if (bytes[1] == 38 && bytes[2] == 64)
            {
                return ISO2022Modes.ModeNOOP;
            }
            return ISO2022Modes.ModeInvalidEscape;
        }

        private static byte DecrementEscapeBytes(ref byte[] bytes, ref int count)
        {
            count--;
            byte result = bytes[0];
            for (int i = 0; i < count; i++)
            {
                bytes[i] = bytes[i + 1];
            }
            bytes[count] = 0;
            return result;
        }

        private unsafe int GetCharsCP50225KR(byte* bytes, int byteCount, char* chars, int charCount, ISO2022Decoder decoder)
        {
            EncodingCharBuffer encodingCharBuffer = new EncodingCharBuffer(this, decoder, chars, charCount, bytes, byteCount);
            ISO2022Modes iSO2022Modes = ISO2022Modes.ModeASCII;
            byte[] bytes2 = new byte[4];
            int count = 0;
            if (decoder != null)
            {
                iSO2022Modes = decoder.currentMode;
                count = decoder.bytesLeftOverCount;
                for (int i = 0; i < count; i++)
                {
                    bytes2[i] = decoder.bytesLeftOver[i];
                }
            }
            while (encodingCharBuffer.MoreData || count > 0)
            {
                byte b;
                if (count > 0)
                {
                    if (bytes2[0] == 27)
                    {
                        if (!encodingCharBuffer.MoreData)
                        {
                            if (decoder != null && !decoder.MustFlush)
                            {
                                break;
                            }
                        }
                        else
                        {
                            bytes2[count++] = encodingCharBuffer.GetNextByte();
                            switch (CheckEscapeSequenceKR(bytes2, count))
                            {
                                default:
                                    count = 0;
                                    continue;
                                case ISO2022Modes.ModeInvalidEscape:
                                    break;
                                case ISO2022Modes.ModeIncompleteEscape:
                                    continue;
                            }
                        }
                    }
                    b = DecrementEscapeBytes(ref bytes2, ref count);
                }
                else
                {
                    b = encodingCharBuffer.GetNextByte();
                    if (b == 27)
                    {
                        if (count == 0)
                        {
                            bytes2[0] = b;
                            count = 1;
                            continue;
                        }
                        encodingCharBuffer.AdjustBytes(-1);
                    }
                }
                switch (b)
                {
                    case 14:
                        iSO2022Modes = ISO2022Modes.ModeKR;
                        continue;
                    case 15:
                        iSO2022Modes = ISO2022Modes.ModeASCII;
                        continue;
                }
                ushort num = b;
                bool flag = false;
                if (iSO2022Modes == ISO2022Modes.ModeKR && b != 32 && b != 9 && b != 10)
                {
                    if (count > 0)
                    {
                        if (bytes2[0] != 27)
                        {
                            num = (ushort)(num << 8);
                            num = (ushort)(num | DecrementEscapeBytes(ref bytes2, ref count));
                            flag = true;
                        }
                    }
                    else
                    {
                        if (!encodingCharBuffer.MoreData)
                        {
                            if (decoder == null || decoder.MustFlush)
                            {
                                encodingCharBuffer.Fallback(b);
                            }
                            else if (chars != null)
                            {
                                bytes2[0] = b;
                                count = 1;
                            }
                            break;
                        }
                        num = (ushort)(num << 8);
                        num = (ushort)(num | encodingCharBuffer.GetNextByte());
                        flag = true;
                    }
                }
                char c = mapBytesToUnicode[(int)num];
                if (c == '\0' && num != 0)
                {
                    if (flag)
                    {
                        if (!encodingCharBuffer.Fallback((byte)(num >> 8), (byte)num))
                        {
                            break;
                        }
                    }
                    else if (!encodingCharBuffer.Fallback(b))
                    {
                        break;
                    }
                }
                else if (!encodingCharBuffer.AddChar(c, (!flag) ? 1 : 2))
                {
                    break;
                }
            }
            if (chars != null && decoder != null)
            {
                if (!decoder.MustFlush || count != 0)
                {
                    decoder.currentMode = iSO2022Modes;
                    decoder.bytesLeftOverCount = count;
                    decoder.bytesLeftOver = bytes2;
                }
                else
                {
                    decoder.currentMode = ISO2022Modes.ModeASCII;
                    decoder.shiftInOutMode = ISO2022Modes.ModeASCII;
                    decoder.bytesLeftOverCount = 0;
                }
                decoder.m_bytesUsed = encodingCharBuffer.BytesUsed;
            }
            return encodingCharBuffer.Count;
        }

        private static ISO2022Modes CheckEscapeSequenceKR(byte[] bytes, int escapeCount)
        {
            if (bytes[0] != 27)
            {
                return ISO2022Modes.ModeInvalidEscape;
            }
            if (escapeCount < 4)
            {
                return ISO2022Modes.ModeIncompleteEscape;
            }
            if (bytes[1] == 36 && bytes[2] == 41 && bytes[3] == 67)
            {
                return ISO2022Modes.ModeKR;
            }
            return ISO2022Modes.ModeInvalidEscape;
        }

        private unsafe int GetCharsCP52936(byte* bytes, int byteCount, char* chars, int charCount, ISO2022Decoder decoder)
        {
            EncodingCharBuffer encodingCharBuffer = new EncodingCharBuffer(this, decoder, chars, charCount, bytes, byteCount);
            ISO2022Modes iSO2022Modes = ISO2022Modes.ModeASCII;
            int num = -1;
            bool flag = false;
            if (decoder != null)
            {
                iSO2022Modes = decoder.currentMode;
                if (decoder.bytesLeftOverCount != 0)
                {
                    num = decoder.bytesLeftOver[0];
                }
            }
            while (encodingCharBuffer.MoreData || num >= 0)
            {
                byte b;
                if (num >= 0)
                {
                    b = (byte)num;
                    num = -1;
                }
                else
                {
                    b = encodingCharBuffer.GetNextByte();
                }
                if (b == 126)
                {
                    if (!encodingCharBuffer.MoreData)
                    {
                        if (decoder == null || decoder.MustFlush)
                        {
                            encodingCharBuffer.Fallback(b);
                            break;
                        }
                        decoder.ClearMustFlush();
                        if (chars != null)
                        {
                            decoder.bytesLeftOverCount = 1;
                            decoder.bytesLeftOver[0] = 126;
                            flag = true;
                        }
                        break;
                    }
                    b = encodingCharBuffer.GetNextByte();
                    if (b == 126 && iSO2022Modes == ISO2022Modes.ModeASCII)
                    {
                        if (!encodingCharBuffer.AddChar((char)b, 2))
                        {
                            break;
                        }
                        continue;
                    }
                    if (b == 123)
                    {
                        iSO2022Modes = ISO2022Modes.ModeHZ;
                        continue;
                    }
                    if (b == 125)
                    {
                        iSO2022Modes = ISO2022Modes.ModeASCII;
                        continue;
                    }
                    if (b == 10)
                    {
                        continue;
                    }
                    encodingCharBuffer.AdjustBytes(-1);
                    b = 126;
                }
                if (iSO2022Modes != ISO2022Modes.ModeASCII && b >= 32)
                {
                    if (!encodingCharBuffer.MoreData)
                    {
                        if (decoder == null || decoder.MustFlush)
                        {
                            encodingCharBuffer.Fallback(b);
                            break;
                        }
                        decoder.ClearMustFlush();
                        if (chars != null)
                        {
                            decoder.bytesLeftOverCount = 1;
                            decoder.bytesLeftOver[0] = b;
                            flag = true;
                        }
                        break;
                    }
                    byte nextByte = encodingCharBuffer.GetNextByte();
                    ushort num2 = (ushort)((b << 8) | nextByte);
                    char c;
                    if (b == 32 && nextByte != 0)
                    {
                        c = (char)nextByte;
                    }
                    else
                    {
                        if ((b < 33 || b > 119 || nextByte < 33 || nextByte > 126) && (b < 161 || b > 247 || nextByte < 161 || nextByte > 254))
                        {
                            if (nextByte != 32 || 33 > b || b > 125)
                            {
                                if (!encodingCharBuffer.Fallback((byte)(num2 >> 8), (byte)num2))
                                {
                                    break;
                                }
                                continue;
                            }
                            num2 = 8481;
                        }
                        num2 = (ushort)(num2 | 0x8080u);
                        c = mapBytesToUnicode[(int)num2];
                    }
                    if (c == '\0' && num2 != 0)
                    {
                        if (!encodingCharBuffer.Fallback((byte)(num2 >> 8), (byte)num2))
                        {
                            break;
                        }
                    }
                    else if (!encodingCharBuffer.AddChar(c, 2))
                    {
                        break;
                    }
                    continue;
                }
                char c2 = mapBytesToUnicode[(int)b];
                if ((c2 == '\0' || c2 == '\0') && b != 0)
                {
                    if (!encodingCharBuffer.Fallback(b))
                    {
                        break;
                    }
                }
                else if (!encodingCharBuffer.AddChar(c2))
                {
                    break;
                }
            }
            if (chars != null && decoder != null)
            {
                if (!flag)
                {
                    decoder.bytesLeftOverCount = 0;
                }
                if (decoder.MustFlush && decoder.bytesLeftOverCount == 0)
                {
                    decoder.currentMode = ISO2022Modes.ModeASCII;
                }
                else
                {
                    decoder.currentMode = iSO2022Modes;
                }
                decoder.m_bytesUsed = encodingCharBuffer.BytesUsed;
            }
            return encodingCharBuffer.Count;
        }

        public override int GetMaxByteCount(int charCount)
        {
            if (charCount < 0)
            {
                throw new ArgumentOutOfRangeException("charCount", MDCFR.Properties.Resources.ArgumentOutOfRange_NeedNonNegNum);
            }
            long num = (long)charCount + 1L;
            if (base.EncoderFallback.MaxCharCount > 1)
            {
                num *= base.EncoderFallback.MaxCharCount;
            }
            int num2 = 2;
            int num3 = 0;
            int num4 = 0;
            switch (CodePage)
            {
                case 50220:
                case 50221:
                    num2 = 5;
                    num4 = 3;
                    break;
                case 50222:
                    num2 = 5;
                    num4 = 4;
                    break;
                case 50225:
                    num2 = 3;
                    num3 = 4;
                    num4 = 1;
                    break;
                case 52936:
                    num2 = 4;
                    num4 = 2;
                    break;
            }
            num *= num2;
            num += num3 + num4;
            if (num > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException("charCount", MDCFR.Properties.Resources.ArgumentOutOfRange_GetByteCountOverflow);
            }
            return (int)num;
        }

        public override int GetMaxCharCount(int byteCount)
        {
            if (byteCount < 0)
            {
                throw new ArgumentOutOfRangeException("byteCount", MDCFR.Properties.Resources.ArgumentOutOfRange_NeedNonNegNum);
            }
            int num = 1;
            int num2 = 1;
            switch (CodePage)
            {
                case 50220:
                case 50221:
                case 50222:
                case 50225:
                    num = 1;
                    num2 = 3;
                    break;
                case 52936:
                    num = 1;
                    num2 = 1;
                    break;
            }
            long num3 = (long)byteCount * (long)num + num2;
            if (base.DecoderFallback.MaxCharCount > 1)
            {
                num3 *= base.DecoderFallback.MaxCharCount;
            }
            if (num3 > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException("byteCount", MDCFR.Properties.Resources.ArgumentOutOfRange_GetCharCountOverflow);
            }
            return (int)num3;
        }

        public override Encoder GetEncoder()
        {
            return new ISO2022Encoder(this);
        }

        public override Decoder GetDecoder()
        {
            return new ISO2022Decoder(this);
        }
    }

    internal sealed class SBCSCodePageEncoding : System.Text.BaseCodePageEncoding
    {
        private unsafe char* _mapBytesToUnicode = null;

        private unsafe byte* _mapUnicodeToBytes = null;

        private const char UNKNOWN_CHAR = '\ufffd';

        private byte _byteUnknown;

        private char _charUnknown;

        private static object s_InternalSyncObject;

        private static object InternalSyncObject
        {
            get
            {
                if (s_InternalSyncObject == null)
                {
                    object value = new object();
                    Interlocked.CompareExchange<object>(ref s_InternalSyncObject, value, (object)null);
                }
                return s_InternalSyncObject;
            }
        }

        public override bool IsSingleByte => true;

        public SBCSCodePageEncoding(int codePage)
            : this(codePage, codePage)
        {
        }

        public unsafe SBCSCodePageEncoding(int codePage, int dataCodePage)
            : base(codePage, dataCodePage)
        {
        }

        internal unsafe static ushort ReadUInt16(byte* pByte)
        {
            if (BitConverter.IsLittleEndian)
            {
                return *(ushort*)pByte;
            }
            return BinaryPrimitives.ReverseEndianness(*(ushort*)pByte);
        }

        protected unsafe override void LoadManagedCodePage()
        {
            fixed (byte* ptr = &m_codePageHeader[0])
            {
                CodePageHeader* ptr2 = (CodePageHeader*)ptr;
                if (ptr2->ByteCount != 1)
                {
                    throw new NotSupportedException(System.SR.Format(MDCFR.Properties.Resources.NotSupported_NoCodepageData, CodePage));
                }
                _byteUnknown = (byte)ptr2->ByteReplace;
                _charUnknown = ptr2->UnicodeReplace;
                int num = 66052 + iExtraBytes;
                byte* nativeMemory = GetNativeMemory(num);
                Unsafe.InitBlockUnaligned(nativeMemory, 0, (uint)num);
                char* ptr3 = (char*)nativeMemory;
                byte* ptr4 = nativeMemory + 512;
                byte[] array = new byte[512];
                lock (System.Text.BaseCodePageEncoding.s_streamLock)
                {
                    System.Text.BaseCodePageEncoding.s_codePagesEncodingDataStream.Seek(m_firstDataWordOffset, SeekOrigin.Begin);
                    int num2 = System.Text.BaseCodePageEncoding.s_codePagesEncodingDataStream.Read(array, 0, array.Length);
                }
                fixed (byte* ptr5 = &array[0])
                {
                    for (int i = 0; i < 256; i++)
                    {
                        char c = (char)ReadUInt16(ptr5 + 2 * i);
                        if (c != 0 || i == 0)
                        {
                            ptr3[i] = c;
                            if (c != '\ufffd')
                            {
                                ptr4[(int)c] = (byte)i;
                            }
                        }
                        else
                        {
                            ptr3[i] = '\ufffd';
                        }
                    }
                }
                _mapBytesToUnicode = ptr3;
                _mapUnicodeToBytes = ptr4;
            }
        }

        protected unsafe override void ReadBestFitTable()
        {
            lock (InternalSyncObject)
            {
                if (arrayUnicodeBestFit != null)
                {
                    return;
                }
                byte[] array = new byte[m_dataSize - 512];
                lock (System.Text.BaseCodePageEncoding.s_streamLock)
                {
                    System.Text.BaseCodePageEncoding.s_codePagesEncodingDataStream.Seek(m_firstDataWordOffset + 512, SeekOrigin.Begin);
                    int num = System.Text.BaseCodePageEncoding.s_codePagesEncodingDataStream.Read(array, 0, array.Length);
                }
                fixed (byte* ptr = array)
                {
                    byte* ptr2 = ptr;
                    char[] array2 = new char[256];
                    for (int i = 0; i < 256; i++)
                    {
                        array2[i] = _mapBytesToUnicode[i];
                    }
                    ushort num2;
                    while ((num2 = ReadUInt16(ptr2)) != 0)
                    {
                        ptr2 += 2;
                        array2[num2] = (char)ReadUInt16(ptr2);
                        ptr2 += 2;
                    }
                    arrayBytesBestFit = array2;
                    ptr2 += 2;
                    byte* ptr3 = ptr2;
                    int num3 = 0;
                    int num4 = ReadUInt16(ptr2);
                    ptr2 += 2;
                    while (num4 < 65536)
                    {
                        byte b = *ptr2;
                        ptr2++;
                        switch (b)
                        {
                            case 1:
                                num4 = ReadUInt16(ptr2);
                                ptr2 += 2;
                                continue;
                            case 2:
                            case 3:
                            case 4:
                            case 5:
                            case 6:
                            case 7:
                            case 8:
                            case 9:
                            case 10:
                            case 11:
                            case 12:
                            case 13:
                            case 14:
                            case 15:
                            case 16:
                            case 17:
                            case 18:
                            case 19:
                            case 20:
                            case 21:
                            case 22:
                            case 23:
                            case 24:
                            case 25:
                            case 26:
                            case 27:
                            case 28:
                            case 29:
                            case 31:
                                num4 += b;
                                continue;
                        }
                        if (b > 0)
                        {
                            num3++;
                        }
                        num4++;
                    }
                    array2 = new char[num3 * 2];
                    ptr2 = ptr3;
                    num4 = ReadUInt16(ptr2);
                    ptr2 += 2;
                    num3 = 0;
                    while (num4 < 65536)
                    {
                        byte b2 = *ptr2;
                        ptr2++;
                        switch (b2)
                        {
                            case 1:
                                num4 = ReadUInt16(ptr2);
                                ptr2 += 2;
                                continue;
                            case 2:
                            case 3:
                            case 4:
                            case 5:
                            case 6:
                            case 7:
                            case 8:
                            case 9:
                            case 10:
                            case 11:
                            case 12:
                            case 13:
                            case 14:
                            case 15:
                            case 16:
                            case 17:
                            case 18:
                            case 19:
                            case 20:
                            case 21:
                            case 22:
                            case 23:
                            case 24:
                            case 25:
                            case 26:
                            case 27:
                            case 28:
                            case 29:
                            case 31:
                                num4 += b2;
                                continue;
                        }
                        if (b2 == 30)
                        {
                            b2 = *ptr2;
                            ptr2++;
                        }
                        if (b2 > 0)
                        {
                            array2[num3++] = (char)num4;
                            array2[num3++] = _mapBytesToUnicode[(int)b2];
                        }
                        num4++;
                    }
                    arrayUnicodeBestFit = array2;
                }
            }
        }

        public unsafe override int GetByteCount(char* chars, int count, System.Text.EncoderNLS encoder)
        {
            CheckMemorySection();
            EncoderReplacementFallback encoderReplacementFallback = null;
            char c = '\0';
            if (encoder != null)
            {
                c = encoder.charLeftOver;
                encoderReplacementFallback = encoder.Fallback as EncoderReplacementFallback;
            }
            else
            {
                encoderReplacementFallback = base.EncoderFallback as EncoderReplacementFallback;
            }
            if (encoderReplacementFallback != null && encoderReplacementFallback.MaxCharCount == 1)
            {
                if (c > '\0')
                {
                    count++;
                }
                return count;
            }
            EncoderFallbackBuffer encoderFallbackBuffer = null;
            int num = 0;
            char* ptr = chars + count;
            EncoderFallbackBufferHelper encoderFallbackBufferHelper = new EncoderFallbackBufferHelper(encoderFallbackBuffer);
            if (c > '\0')
            {
                encoderFallbackBuffer = encoder.FallbackBuffer;
                encoderFallbackBufferHelper = new EncoderFallbackBufferHelper(encoderFallbackBuffer);
                encoderFallbackBufferHelper.InternalInitialize(chars, ptr, encoder, _setEncoder: false);
                encoderFallbackBufferHelper.InternalFallback(c, ref chars);
            }
            char c2;
            while ((c2 = ((encoderFallbackBuffer != null) ? encoderFallbackBufferHelper.InternalGetNextChar() : '\0')) != 0 || chars < ptr)
            {
                if (c2 == '\0')
                {
                    c2 = *chars;
                    chars++;
                }
                if (_mapUnicodeToBytes[(int)c2] == 0 && c2 != 0)
                {
                    if (encoderFallbackBuffer == null)
                    {
                        encoderFallbackBuffer = ((encoder != null) ? encoder.FallbackBuffer : base.EncoderFallback.CreateFallbackBuffer());
                        encoderFallbackBufferHelper = new EncoderFallbackBufferHelper(encoderFallbackBuffer);
                        encoderFallbackBufferHelper.InternalInitialize(ptr - count, ptr, encoder, _setEncoder: false);
                    }
                    encoderFallbackBufferHelper.InternalFallback(c2, ref chars);
                }
                else
                {
                    num++;
                }
            }
            return num;
        }

        public unsafe override int GetBytes(char* chars, int charCount, byte* bytes, int byteCount, System.Text.EncoderNLS encoder)
        {
            CheckMemorySection();
            EncoderReplacementFallback encoderReplacementFallback = null;
            char c = '\0';
            if (encoder != null)
            {
                c = encoder.charLeftOver;
                encoderReplacementFallback = encoder.Fallback as EncoderReplacementFallback;
            }
            else
            {
                encoderReplacementFallback = base.EncoderFallback as EncoderReplacementFallback;
            }
            char* ptr = chars + charCount;
            byte* ptr2 = bytes;
            char* ptr3 = chars;
            if (encoderReplacementFallback != null && encoderReplacementFallback.MaxCharCount == 1)
            {
                byte b = _mapUnicodeToBytes[(int)encoderReplacementFallback.DefaultString[0]];
                if (b != 0)
                {
                    if (c > '\0')
                    {
                        if (byteCount == 0)
                        {
                            ThrowBytesOverflow(encoder, nothingEncoded: true);
                        }
                        *(bytes++) = b;
                        byteCount--;
                    }
                    if (byteCount < charCount)
                    {
                        ThrowBytesOverflow(encoder, byteCount < 1);
                        ptr = chars + byteCount;
                    }
                    while (chars < ptr)
                    {
                        char c2 = *chars;
                        chars++;
                        byte b2 = _mapUnicodeToBytes[(int)c2];
                        if (b2 == 0 && c2 != 0)
                        {
                            *bytes = b;
                        }
                        else
                        {
                            *bytes = b2;
                        }
                        bytes++;
                    }
                    if (encoder != null)
                    {
                        encoder.charLeftOver = '\0';
                        encoder.m_charsUsed = (int)(chars - ptr3);
                    }
                    return (int)(bytes - ptr2);
                }
            }
            EncoderFallbackBuffer encoderFallbackBuffer = null;
            byte* ptr4 = bytes + byteCount;
            EncoderFallbackBufferHelper encoderFallbackBufferHelper = new EncoderFallbackBufferHelper(encoderFallbackBuffer);
            if (c > '\0')
            {
                encoderFallbackBuffer = encoder.FallbackBuffer;
                encoderFallbackBufferHelper = new EncoderFallbackBufferHelper(encoderFallbackBuffer);
                encoderFallbackBufferHelper.InternalInitialize(chars, ptr, encoder, _setEncoder: true);
                encoderFallbackBufferHelper.InternalFallback(c, ref chars);
                if (encoderFallbackBuffer.Remaining > ptr4 - bytes)
                {
                    ThrowBytesOverflow(encoder, nothingEncoded: true);
                }
            }
            char c3;
            while ((c3 = ((encoderFallbackBuffer != null) ? encoderFallbackBufferHelper.InternalGetNextChar() : '\0')) != 0 || chars < ptr)
            {
                if (c3 == '\0')
                {
                    c3 = *chars;
                    chars++;
                }
                byte b3 = _mapUnicodeToBytes[(int)c3];
                if (b3 == 0 && c3 != 0)
                {
                    if (encoderFallbackBuffer == null)
                    {
                        encoderFallbackBuffer = ((encoder != null) ? encoder.FallbackBuffer : base.EncoderFallback.CreateFallbackBuffer());
                        encoderFallbackBufferHelper = new EncoderFallbackBufferHelper(encoderFallbackBuffer);
                        encoderFallbackBufferHelper.InternalInitialize(ptr - charCount, ptr, encoder, _setEncoder: true);
                    }
                    encoderFallbackBufferHelper.InternalFallback(c3, ref chars);
                    if (encoderFallbackBuffer.Remaining > ptr4 - bytes)
                    {
                        chars--;
                        encoderFallbackBufferHelper.InternalReset();
                        ThrowBytesOverflow(encoder, chars == ptr3);
                        break;
                    }
                    continue;
                }
                if (bytes >= ptr4)
                {
                    if (encoderFallbackBuffer == null || !encoderFallbackBufferHelper.bFallingBack)
                    {
                        chars--;
                    }
                    ThrowBytesOverflow(encoder, chars == ptr3);
                    break;
                }
                *bytes = b3;
                bytes++;
            }
            if (encoder != null)
            {
                if (encoderFallbackBuffer != null && !encoderFallbackBufferHelper.bUsedEncoder)
                {
                    encoder.charLeftOver = '\0';
                }
                encoder.m_charsUsed = (int)(chars - ptr3);
            }
            return (int)(bytes - ptr2);
        }

        public unsafe override int GetCharCount(byte* bytes, int count, System.Text.DecoderNLS decoder)
        {
            CheckMemorySection();
            bool flag = false;
            DecoderReplacementFallback decoderReplacementFallback = null;
            if (decoder == null)
            {
                decoderReplacementFallback = base.DecoderFallback as DecoderReplacementFallback;
                flag = base.DecoderFallback is System.Text.InternalDecoderBestFitFallback;
            }
            else
            {
                decoderReplacementFallback = decoder.Fallback as DecoderReplacementFallback;
                flag = decoder.Fallback is System.Text.InternalDecoderBestFitFallback;
            }
            if (flag || (decoderReplacementFallback != null && decoderReplacementFallback.MaxCharCount == 1))
            {
                return count;
            }
            DecoderFallbackBuffer decoderFallbackBuffer = null;
            DecoderFallbackBufferHelper decoderFallbackBufferHelper = new DecoderFallbackBufferHelper(decoderFallbackBuffer);
            int num = count;
            byte[] array = new byte[1];
            byte* ptr = bytes + count;
            while (bytes < ptr)
            {
                char c = _mapBytesToUnicode[(int)(*bytes)];
                bytes++;
                if (c == '\ufffd')
                {
                    if (decoderFallbackBuffer == null)
                    {
                        decoderFallbackBuffer = ((decoder != null) ? decoder.FallbackBuffer : base.DecoderFallback.CreateFallbackBuffer());
                        decoderFallbackBufferHelper = new DecoderFallbackBufferHelper(decoderFallbackBuffer);
                        decoderFallbackBufferHelper.InternalInitialize(ptr - count, null);
                    }
                    array[0] = *(bytes - 1);
                    num--;
                    num += decoderFallbackBufferHelper.InternalFallback(array, bytes);
                }
            }
            return num;
        }

        public unsafe override int GetChars(byte* bytes, int byteCount, char* chars, int charCount, System.Text.DecoderNLS decoder)
        {
            CheckMemorySection();
            bool flag = false;
            byte* ptr = bytes + byteCount;
            byte* ptr2 = bytes;
            char* ptr3 = chars;
            DecoderReplacementFallback decoderReplacementFallback = null;
            if (decoder == null)
            {
                decoderReplacementFallback = base.DecoderFallback as DecoderReplacementFallback;
                flag = base.DecoderFallback is System.Text.InternalDecoderBestFitFallback;
            }
            else
            {
                decoderReplacementFallback = decoder.Fallback as DecoderReplacementFallback;
                flag = decoder.Fallback is System.Text.InternalDecoderBestFitFallback;
            }
            if (flag || (decoderReplacementFallback != null && decoderReplacementFallback.MaxCharCount == 1))
            {
                char c = decoderReplacementFallback?.DefaultString[0] ?? '?';
                if (charCount < byteCount)
                {
                    ThrowCharsOverflow(decoder, charCount < 1);
                    ptr = bytes + charCount;
                }
                while (bytes < ptr)
                {
                    char c2;
                    if (flag)
                    {
                        if (arrayBytesBestFit == null)
                        {
                            ReadBestFitTable();
                        }
                        c2 = arrayBytesBestFit[*bytes];
                    }
                    else
                    {
                        c2 = _mapBytesToUnicode[(int)(*bytes)];
                    }
                    bytes++;
                    if (c2 == '\ufffd')
                    {
                        *chars = c;
                    }
                    else
                    {
                        *chars = c2;
                    }
                    chars++;
                }
                if (decoder != null)
                {
                    decoder.m_bytesUsed = (int)(bytes - ptr2);
                }
                return (int)(chars - ptr3);
            }
            DecoderFallbackBuffer decoderFallbackBuffer = null;
            byte[] array = new byte[1];
            char* ptr4 = chars + charCount;
            DecoderFallbackBufferHelper decoderFallbackBufferHelper = new DecoderFallbackBufferHelper(null);
            while (bytes < ptr)
            {
                char c3 = _mapBytesToUnicode[(int)(*bytes)];
                bytes++;
                if (c3 == '\ufffd')
                {
                    if (decoderFallbackBuffer == null)
                    {
                        decoderFallbackBuffer = ((decoder != null) ? decoder.FallbackBuffer : base.DecoderFallback.CreateFallbackBuffer());
                        decoderFallbackBufferHelper = new DecoderFallbackBufferHelper(decoderFallbackBuffer);
                        decoderFallbackBufferHelper.InternalInitialize(ptr - byteCount, ptr4);
                    }
                    array[0] = *(bytes - 1);
                    if (!decoderFallbackBufferHelper.InternalFallback(array, bytes, ref chars))
                    {
                        bytes--;
                        decoderFallbackBufferHelper.InternalReset();
                        ThrowCharsOverflow(decoder, bytes == ptr2);
                        break;
                    }
                }
                else
                {
                    if (chars >= ptr4)
                    {
                        bytes--;
                        ThrowCharsOverflow(decoder, bytes == ptr2);
                        break;
                    }
                    *chars = c3;
                    chars++;
                }
            }
            if (decoder != null)
            {
                decoder.m_bytesUsed = (int)(bytes - ptr2);
            }
            return (int)(chars - ptr3);
        }

        public override int GetMaxByteCount(int charCount)
        {
            if (charCount < 0)
            {
                throw new ArgumentOutOfRangeException("charCount", MDCFR.Properties.Resources.ArgumentOutOfRange_NeedNonNegNum);
            }
            long num = (long)charCount + 1L;
            if (base.EncoderFallback.MaxCharCount > 1)
            {
                num *= base.EncoderFallback.MaxCharCount;
            }
            if (num > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException("charCount", MDCFR.Properties.Resources.ArgumentOutOfRange_GetByteCountOverflow);
            }
            return (int)num;
        }

        public override int GetMaxCharCount(int byteCount)
        {
            if (byteCount < 0)
            {
                throw new ArgumentOutOfRangeException("byteCount", MDCFR.Properties.Resources.ArgumentOutOfRange_NeedNonNegNum);
            }
            long num = byteCount;
            if (base.DecoderFallback.MaxCharCount > 1)
            {
                num *= base.DecoderFallback.MaxCharCount;
            }
            if (num > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException("byteCount", MDCFR.Properties.Resources.ArgumentOutOfRange_GetCharCountOverflow);
            }
            return (int)num;
        }
    }
}


