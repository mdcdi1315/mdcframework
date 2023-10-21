

// An All-In-One framework abstracting the most important classes that are used in .NET
// that are more easily and more consistently to be used.
// The framework was designed to host many different operations , with the last goal 
// to be everything accessible for everyone.

// Global namespaces
using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO.Compression;
using System.Runtime.Versioning;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ROOT
{

    namespace CryptographicOperations
    {
        // A Collection Namespace of encrypting and decrypting files.
        // For now (At the time of writing this code) , only UTF-8 is supported.

        /// <summary>
        /// A storage class used to take values from the randomizer (Function <see cref="AESEncryption.MakeNewKeyAndInitVector"/>).
        /// </summary>
        public class KeyGenTable
        {
            private System.String ERC;
            private System.Byte[] _IV_;
            private System.Byte[] _EK_;
            private System.Int32 EMF_;

            /// <summary>
            /// Returns the <c>"Error"</c> <see cref="System.String"/> 
            /// in case of error; otherwise , the <see cref="System.String"/> <c>"OK"</c>.
            /// </summary>
            public System.String ErrorCode
            {
                get { return ERC; }
                set { ERC = value; }
            }

            /// <summary>
            /// Create a new <see cref="AESEncryption"/> class , if the required data were got.
            /// </summary>
            public AESEncryption Create
            {
                get
                {
                    if (CallerErroredOut == false)
                    {
                        AESEncryption EW = new();
                        EW.EncryptionKey = _EK_;
                        EW.IV = _IV_;
                        return EW;
                    }
                    else
                    {
                        throw new InvalidOperationException("The data were not got , so it is not possible to instantiate a new AES Encryption class.");
                    }
                }
            }

            /// <summary>
            /// Returns a <see cref="System.Boolean"/> indicating whether the function or any function that uses this has errored out.
            /// </summary>
            public System.Boolean CallerErroredOut
            {
                get { if (ERC == null) { return false; } else { if (ERC == "Error") { return true; } else { return false; } } }
            }

            /// <summary>
            /// The Encryption key. Recommended to be more than 32 bytes.
            /// </summary>
            public System.Byte[] Key
            {
                get { return _EK_; }
                set { _EK_ = value; }
            }

            /// <summary>
            /// The initialisation vector to use. Recommended to be more than 16 bytes.
            /// </summary>
            public System.Byte[] IV
            {
                get { return _IV_; }
                set { _IV_ = value; }
            }

            /// <summary>
            /// Returns the actual message key length as of <see cref="System.Int32"/> units.
            /// </summary>
            public System.Int32 KeyLengthInBits
            {
                get { return EMF_; }
                set { EMF_ = value; }
            }
        }

        /// <summary>
        /// AES Encryption class. It can also encrypt files.
        /// NOTE: You are NOT allowed to override this class.
        /// </summary>
        /// <remarks>Only files with UTF-8 encoding can be sucessfully encrypted and decrypted for now.</remarks>
        public sealed class AESEncryption : System.IDisposable
        {
            // Cryptographic Operations Class.
            private System.Byte[] _EncryptionKey_;
            private System.Byte[] _InitVec_;
            private System.Security.Cryptography.AesCng CNGBaseObject = new();

            /// <summary>
            /// The encryption key to set for encoding/decoding.
            /// </summary>
            public System.Byte[] EncryptionKey { set { _EncryptionKey_ = value; } }

            /// <summary>
            /// The Initialisation Vector to set for encoding/decoding.
            /// </summary>
            public System.Byte[] IV { set { _InitVec_ = value; } }

            /// <summary>
            /// Create a new random key and Initialisation vector to use.
            /// </summary>
            /// <returns>A new <see cref="KeyGenTable"/> containing the key and the IV.</returns>
            [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
            public static KeyGenTable MakeNewKeyAndInitVector()
            {
                System.Security.Cryptography.AesCng RETM;
                KeyGenTable RDM = new();
                try { RETM = new System.Security.Cryptography.AesCng(); }
                catch (System.Exception) { RDM.ErrorCode = "Error"; return RDM; }
                RDM.ErrorCode = "OK";
                RDM.IV = RETM.IV;
                RDM.Key = RETM.Key;
                RDM.KeyLengthInBits = RETM.KeySize;
                return RDM;
            }

            [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
            private System.Boolean _CheckPredefinedProperties()
            {
                if ((_EncryptionKey_ is null) || (_InitVec_ is null)) { return true; }
                if ((_EncryptionKey_.Length <= 0) || (_InitVec_.Length <= 0)) { return true; }
                return false;
            }

            /// <summary>
            /// Encrypts the specified <see cref="System.String"/> plain text as <see cref="System.Byte"/>[] units.
            /// </summary>
            /// <param name="PlainText">The text to encrypt.</param>
            /// <returns>The encrypted AES CNG message.</returns>
            [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
            public System.Byte[] EncryptSpecifiedData(System.String PlainText)
            {
                if (System.String.IsNullOrEmpty(PlainText))
                {
                    return null;
                }
                if (_CheckPredefinedProperties())
                {
                    return null;
                }
                System.Security.Cryptography.AesCng ENC_1 = CNGBaseObject;
                ENC_1.Key = _EncryptionKey_;
                ENC_1.IV = _InitVec_;
                ENC_1.Padding = System.Security.Cryptography.PaddingMode.PKCS7;
                ENC_1.Mode = System.Security.Cryptography.CipherMode.CBC;
                System.Security.Cryptography.ICryptoTransform ENC_2 = ENC_1.CreateEncryptor();
                System.Byte[] EncryptedArray;
                using (System.IO.MemoryStream MSSSR = new System.IO.MemoryStream())
                {
                    using (System.Security.Cryptography.CryptoStream CryptStrEnc = new System.Security.Cryptography.CryptoStream(MSSSR, ENC_2, System.Security.Cryptography.CryptoStreamMode.Write))
                    {
                        using (System.IO.StreamWriter SDM = new System.IO.StreamWriter(CryptStrEnc, System.Text.Encoding.UTF8))
                        {
                            SDM.Write(PlainText);
                        }
                        EncryptedArray = MSSSR.ToArray();
                    }
                }
                return EncryptedArray;
            }

            /// <summary>
            /// Encrypts the alive <see cref="System.IO.FileStream"/> with all of it's containing data as <see cref="System.Byte"/>[] units.
            /// </summary>
            /// <param name="UnderlyingStream">The <see cref="System.IO.Stream"/> object to get data from.</param>
            /// <returns>The encrypted AES CNG message.</returns>
            [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
            public System.Byte[] EncryptSpecifiedDataForFiles(System.IO.Stream UnderlyingStream)
            {
                if (!(UnderlyingStream is System.IO.FileStream) || (UnderlyingStream.CanRead == false)) { return null; }
                if (_CheckPredefinedProperties()) { return null; }
                System.Byte[] ByteArray = new System.Byte[UnderlyingStream.Length];
                UnderlyingStream.Read(ByteArray, 0, System.Convert.ToInt32(UnderlyingStream.Length));
                System.Security.Cryptography.AesCng ENC_1 = CNGBaseObject;
                ENC_1.Key = _EncryptionKey_;
                ENC_1.IV = _InitVec_;
                ENC_1.Padding = System.Security.Cryptography.PaddingMode.PKCS7;
                ENC_1.Mode = System.Security.Cryptography.CipherMode.CBC;
                System.Security.Cryptography.ICryptoTransform ENC_2 = ENC_1.CreateEncryptor();
                System.Byte[] EncryptedArray;
                using (System.IO.MemoryStream MSSSR = new System.IO.MemoryStream())
                {
                    using (System.Security.Cryptography.CryptoStream CryptStrEnc = new System.Security.Cryptography.CryptoStream(MSSSR, ENC_2, System.Security.Cryptography.CryptoStreamMode.Write))
                    {
                        using (System.IO.BinaryWriter SDM = new System.IO.BinaryWriter(CryptStrEnc, System.Text.Encoding.UTF8))
                        {
                            SDM.Write(ByteArray, 0, ByteArray.Length);
                        }
                        EncryptedArray = MSSSR.ToArray();
                    }
                }
                return EncryptedArray;
            }

            /// <summary>
            /// Decrypts the encdoed AES CNG message to <see cref="System.String"/> units.
            /// </summary>
            /// <param name="EncryptedArray">The encrypted AES CNG message.</param>
            /// <returns>The decoded message , as <see cref="System.String"/> code units.</returns>
            [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
            public System.String DecryptSpecifiedData(System.Byte[] EncryptedArray)
            {
                if ((EncryptedArray is null) || (EncryptedArray.Length <= 0))
                {
                    return null;
                }
                if (_CheckPredefinedProperties()) { return null; }
                System.Security.Cryptography.AesCng ENC_1 = CNGBaseObject;
                ENC_1.Key = _EncryptionKey_;
                ENC_1.IV = _InitVec_;
                ENC_1.Padding = System.Security.Cryptography.PaddingMode.PKCS7;
                ENC_1.Mode = System.Security.Cryptography.CipherMode.CBC;
                System.String StringToReturn = null;
                System.Security.Cryptography.ICryptoTransform ENC_2 = ENC_1.CreateDecryptor();
                using (System.IO.MemoryStream MSSSR = new System.IO.MemoryStream(EncryptedArray))
                {
                    using (System.Security.Cryptography.CryptoStream DCryptStrEnc = new System.Security.Cryptography.CryptoStream(MSSSR, ENC_2, System.Security.Cryptography.CryptoStreamMode.Read))
                    {
                        using (System.IO.StreamReader SDE = new System.IO.StreamReader(DCryptStrEnc, System.Text.Encoding.UTF8))
                        {
                            StringToReturn = SDE.ReadToEnd();
                        }
                    }
                }
                return StringToReturn;
            }

            /// <summary>
            /// Decrypts the encdoed AES CNG message from an alive <see cref="System.IO.Stream"/> 
            /// object to <see cref="System.String"/> units.
            /// </summary>
            /// <param name="EncasingStream">The <see cref="System.IO.Stream"/> object which contains the encoded AES CNG message.</param>
            /// <returns>The decoded message , as <see cref="System.String"/> code units.</returns>
            [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
            public System.String DecryptSpecifiedDataForFiles(System.IO.Stream EncasingStream)
            {
                if (EncasingStream.CanRead == false) { return null; }
                if (_CheckPredefinedProperties()) { return null; }
                System.Security.Cryptography.AesCng ENC_1 = CNGBaseObject;
                ENC_1.Key = _EncryptionKey_;
                ENC_1.IV = _InitVec_;
                ENC_1.Padding = System.Security.Cryptography.PaddingMode.PKCS7;
                ENC_1.Mode = System.Security.Cryptography.CipherMode.CBC;
                System.String FinalString = null;
                System.Security.Cryptography.ICryptoTransform ENC_2 = ENC_1.CreateDecryptor();
                using (System.Security.Cryptography.CryptoStream DCryptStrEnc = new System.Security.Cryptography.CryptoStream(EncasingStream, ENC_2, System.Security.Cryptography.CryptoStreamMode.Read))
                {
                    using (System.IO.StreamReader SDE = new(DCryptStrEnc, System.Text.Encoding.UTF8)) { FinalString = SDE.ReadToEnd(); }
                }
                return FinalString;
            }

            /// <summary>
            /// Converts either the key or Initialisation Vector to a safety-secure Base64 <see cref="System.String"/>.
            /// </summary>
            /// <param name="ByteValue">The Key or Initalisation Vector to convert.</param>
            /// <returns>A new Base64 <see cref="System.String"/> , which contains the encoded key or initialisation vector.</returns>
            [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
            public static System.String ConvertTextKeyOrIvToString(System.Byte[] ByteValue)
            {
                if ((ByteValue is null) || (ByteValue.Length <= 0)) { return null; }
                try
                {
                    ReadOnlySpan<System.Byte> In = ByteValue;
                    Span<System.Byte> Out = new();
                    if (System.Buffers.Text.Base64.EncodeToUtf8(In, Out, out System.Int32 BG,
                        out System.Int32 BW, true) == System.Buffers.OperationStatus.Done)
                    {
                        return System.Text.Encoding.UTF8.GetString(Out.ToArray());
                    }
                    else { return null; }
                }
                catch (System.Exception) { return null; }
            }

            /// <summary>
            /// Converts the converted key or Initialisation Vector from a Base64 <see cref="System.String"/> 
            /// back to a <see cref="System.Byte"/>[] array.
            /// </summary>
            /// <param name="StringValue">The Base64 encoded <see cref="System.String"/>.</param>
            /// <returns>The <see cref="System.Byte"/>[] before the conversion.</returns>
            [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
            public static System.Byte[] ConvertTextKeyOrIvFromStringToByteArray(System.String StringValue)
            {
                if (System.String.IsNullOrEmpty(StringValue)) { return null; }
                try
                {
                    ReadOnlySpan<System.Byte> In = System.Text.Encoding.UTF8.GetBytes(StringValue);
                    Span<System.Byte> Out = new();
                    if (System.Buffers.Text.Base64.DecodeFromUtf8(In, Out, out System.Int32 BG,
                        out System.Int32 ED, true) == System.Buffers.OperationStatus.Done)
                    {
                        return Out.ToArray();
                    }
                    else { return null; }
                }
                catch (System.Exception) { return null; }
            }

            /// <summary>
            /// Use the <see cref="Dispose"/> method to clear up the current key and Initialisation Vector so as to prepare
            /// the encryptor/decryptor for a new session or to invalidate it.
            /// </summary>
            public void Dispose() { DISPMETHOD(); }

            private void DISPMETHOD()
            {
                _EncryptionKey_ = null;
                _InitVec_ = null;
#pragma warning disable CS0219
                System.Object CNGBaseObject = null;
#pragma warning restore CS0219
            }
        }

        /// <summary>
        /// An example class demonstrating how you can encrypt and decrypt UTF-8 files.
        /// </summary>
        public class EDAFile
        {
            /// <summary>
            /// An storage class used in the example.
            /// </summary>
            public class EncryptionContext
            {
                private System.String _ERC_;
                private System.Byte[] _KEY_;
                private System.Byte[] _IV_;

                /// <summary>
                /// The error code of the <see cref="EncryptAFile(string, string)"/> function.
                /// </summary>
                public System.String ErrorCode
                {
                    get { return _ERC_; }
                    set { _ERC_ = value; }
                }

                /// <summary>
                /// The key that the function <see cref="EncryptAFile(string, string)"/> used.
                /// </summary>
                public System.Byte[] KeyUsed
                {
                    get { return _KEY_; }
                    set { _KEY_ = value; }
                }

                /// <summary>
                /// The Initialisation Vector that the function <see cref="EncryptAFile(string, string)"/> used.
                /// </summary>
                public System.Byte[] InitVectorUsed
                {
                    get { return _IV_; }
                    set { _IV_ = value; }
                }
            }

            /// <summary>
            /// Encrypts the specified file and puts the encrypted contents to a new file.
            /// </summary>
            /// <param name="FilePath">The file to encrypt.</param>
            /// <param name="FileOutputPath">The file path to put th encrypted file.</param>
            /// <returns>A new <see cref="EncryptionContext"/> class containing the key used and the Initialisation Vector</returns>
            public static EncryptionContext EncryptAFile(System.String FilePath, System.String FileOutputPath = "")
            {
                if (!(System.IO.File.Exists(FilePath))) { return null; }
                if (System.String.IsNullOrEmpty(FileOutputPath))
                {
                    FileOutputPath = (FilePath).Remove(FilePath.IndexOf(".")) + "_ENCRYPTED_" + (FilePath).Substring(FilePath.IndexOf("."));
                }
                EncryptionContext DMF = new EncryptionContext();
                AESEncryption MDA = new AESEncryption();
                KeyGenTable MAKER = AESEncryption.MakeNewKeyAndInitVector();
                if (MAKER.ErrorCode == "Error")
                {
                    DMF.ErrorCode = "Error";
                    return DMF;
                }
                MDA.EncryptionKey = MAKER.Key;
                MDA.IV = MAKER.IV;
                try
                {
                    using (System.IO.FileStream MDR = new System.IO.FileStream(FilePath, System.IO.FileMode.Open))
                    {
                        using (System.IO.FileStream MNH = System.IO.File.OpenWrite(FileOutputPath))
                        {
                            System.Byte[] FLL = MDA.EncryptSpecifiedDataForFiles(MDR);
                            MNH.Write(FLL, 0, System.Convert.ToInt32(FLL.Length));
                        }
                    }
                }
                catch (System.Exception EX)
                {
                    MAIN.WriteConsoleText(EX.Message);
                    DMF.ErrorCode = "Error";
                    return DMF;
                }
                MDA.Dispose();
                DMF.KeyUsed = MAKER.Key;
                DMF.InitVectorUsed = MAKER.IV;
                return DMF;
            }

            /// <summary>
            /// Decrypts the specified file and puts it's decrypted contents to the file path pointed out.
            /// </summary>
            /// <param name="FilePath">The encrypted file.</param>
            /// <param name="Key">The key that this file uses.</param>
            /// <param name="IV">The Initialisation Vector that this file uses.</param>
            /// <param name="FileOutputPath">The output from the decrypted file.</param>
            public static void DecryptAFile(System.String FilePath, System.Byte[] Key,
            System.Byte[] IV, System.String FileOutputPath = "")
            {
                if (!(System.IO.File.Exists(FilePath))) { return; }
                if ((Key is null) || (IV is null)) { return; }
                if ((Key.Length <= 0) || (IV.Length <= 0)) { return; }
                if (System.String.IsNullOrEmpty(FileOutputPath))
                {
                    FileOutputPath = (FilePath).Remove(FilePath.IndexOf(".")) + "_UNENCRYPTED_" + (FilePath).Substring(FilePath.IndexOf("."));
                }
                AESEncryption MDA = new AESEncryption();
                MDA.EncryptionKey = Key;
                MDA.IV = IV;
                try
                {
                    using (System.IO.FileStream MDR = new System.IO.FileStream(FilePath, System.IO.FileMode.Open))
                    {
                        using (System.IO.StreamWriter MNH = new System.IO.StreamWriter(System.IO.File.OpenWrite(FileOutputPath)))
                        {
                            MNH.WriteLine(MDA.DecryptSpecifiedDataForFiles(MDR));
                        }
                    }
                }
                catch (System.Exception EX)
                {
                    MAIN.WriteConsoleText(EX.Message);
                    return;
                }
                MDA.Dispose();
                return;
            }

            // Executable code examples for encrypting and unencrypting files:
            // Executable code starts here: <--
            // EDAFile.EncryptionContext RDF = EDAFile.EncryptAFile("E:\winrt\base.h" , "E:\IMAGES\winrtbase_h.Encrypted")
            // EDAFile.DecryptAFile("E:\IMAGES\winrtbase_h.Encrypted" , RDF.KeyUsed , RDF.InitVectorUsed , "E:\IMAGES\Unencrypted-4664.h")
            // --> Executable code ended.
            // This is the simpliest way to encrypt and decrypt the files , but you can make use of the original AES API and make the encryption/decryption as you like to.
            // To Access that API , use MDCFR.CryptographicOperations.AESEncryption .
            // More instructions on how to do such security conversions can be found in our Developing Website.
        }

    }

    namespace Archives
    {
        // A Collection Namespace for making and extracting archives.

        /// <summary>
        /// The Current Progress class represents an handler to check the archiving progress.
        /// </summary>
        public class CurrentProgress
        {
            private System.Int32 TFS = 0;
            private System.Int32 FPD = 0;
            private System.String WOF = null;
            private System.String WOFP = null;
            private CurrentOperation OP = CurrentOperation.None;

            /// <summary>
            /// Initialises a new instance of the <see cref="CurrentProgress"/> class. <br />
            /// When the initialisation completes , it fires up the <see cref="GlobalArchiveProgress.ProgressChanged"/>
            /// event.
            /// </summary>
            public CurrentProgress() { GlobalArchiveProgress.FireChanged(); }

            /// <summary>
            /// The number of the files that will be added to the archive.
            /// </summary>
            public System.Int32 TotalFiles { get { return TFS; } set { TFS = value; GlobalArchiveProgress.FireChanged(); } }
            /// <summary>
            /// The number of processed files that were added to or extracted from the archive.
            /// </summary>
            public System.Int32 FilesProcessed { get { return FPD; } set { FPD = value; GlobalArchiveProgress.FireChanged(); } }
            /// <summary>
            /// The name of the file that the operation is working on.
            /// </summary>
            public System.String WorkingOnFile { get { return WOF; } set { WOF = value; GlobalArchiveProgress.FireChanged(); } }
            /// <summary>
            /// The fully qualified path of the file that the operation is running on.
            /// </summary>
            public System.String WorkingOnFileFullPath { get { return WOFP; } set { WOFP = value; GlobalArchiveProgress.FireChanged(); } }
            /// <summary>
            /// The current operation undertaken. A default instance of this class auto-fills this value with <see cref="CurrentOperation.None"/> .
            /// </summary>
            public CurrentOperation Operation { get { return OP; } set { OP = value; GlobalArchiveProgress.FireChanged(); } }

            /// <summary>
            /// The remaining files to be processed. This is actually a subtraction of 
            /// <see cref="TotalFiles"/> and <see cref="FilesProcessed"/>
            /// properties.
            /// </summary>
            public System.Int32 FilesRemaining 
            { 
                get 
                { 
                    if (TotalFiles >= 0 && FilesProcessed >= 0) 
                    { return TotalFiles - FilesProcessed; } 
                    else { return 0; } 
                }
            }

            /// <summary>
            /// Gets a value whether this operation compresses a multiple of files.
            /// </summary>
            public System.Boolean IsMultipleFileOperation { get; internal set; }

            internal void SetOpFailed() { Operation = CurrentOperation.Failed; }

            internal void SetOpEnum() { Operation = CurrentOperation.Enumeration; }

            internal void SetOpCmp() { Operation = CurrentOperation.Compression; }

            internal void SetOpDcmp() { Operation = CurrentOperation.Extraction; }

            internal void SetOpTerm() { Operation = CurrentOperation.Termination; }

            internal void SetOpNone() { Operation = CurrentOperation.None; }
        }

        /// <summary>
        /// The progress class field that is globally used by all classes contained in this namespace.
        /// </summary>
        public static class GlobalArchiveProgress
        {
            /// <summary>
            /// The progress of any executed or executing operation.
            /// </summary>
            public static CurrentProgress Progress;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static System.Int32 FastAbs(System.Int32 Num)
            {
                System.Int32 Value = Num;
                if (Num < 0) { Value = Value * (-1); }
                return Value;
            }

            static GlobalArchiveProgress() 
            {
                ProgressChanged = new(NullEventHandler);
                Progress = new();
            }

            /// <summary>
            /// This event is fired up when one of the properties of the <see cref="CurrentProgress"/> class was changed.
            /// </summary>
            public static event System.EventHandler ProgressChanged;

            internal static void FireChanged() { ProgressChanged.Invoke(typeof(GlobalArchiveProgress) , EventArgs.Empty); }

            private static void NullEventHandler(System.Object sender , System.EventArgs e) { }

            internal static System.Boolean IsDecompOpForCab { get; set; }

            internal static void GetProgressFromCabinets(System.Object sender, ExternalArchivingMethods.Cabinets.ArchiveProgressEventArgs e)
            {
                if (Progress == null) { Progress = new CurrentProgress() { IsMultipleFileOperation = true }; }
                Progress.TotalFiles = e.TotalFiles;
                Progress.WorkingOnFile = e.CurrentFileName;
                Progress.WorkingOnFileFullPath = e.CurrentFileName;
                Progress.FilesProcessed = FastAbs(e.CurrentFileNumber);
                if (e.ProgressType == ExternalArchivingMethods.Cabinets.ArchiveProgressType.StartFile ||
                    e.ProgressType == ExternalArchivingMethods.Cabinets.ArchiveProgressType.StartArchive)
                {
                    if (IsDecompOpForCab) { Progress.SetOpDcmp(); }
                    else { Progress.Operation = CurrentOperation.Compression; }
                }
                else if (e.ProgressType == ExternalArchivingMethods.Cabinets.ArchiveProgressType.FinishArchive)
                {
                    Progress.Operation = CurrentOperation.Termination;
                }
            }
        }

        /// <summary>
        /// The Current Operation enumeration is used in the <see cref="CurrentProgress"/> class 
        /// and represents the current archive operation.
        /// </summary>
        public enum CurrentOperation
        {
            /// <summary>
            /// No outstanding operations are running.
            /// </summary>
            None = 0,
            /// <summary>
            /// The Enumeration operation enumerates the files to compress , or the files to extract.
            /// </summary>
            Enumeration = 1,
            /// <summary>
            /// The Extraction operation extracts files from an archive.
            /// </summary>
            Extraction = 2,
            /// <summary>
            /// The Compression operation compresses files to an archive.
            /// </summary>
            Compression = 3,
            /// <summary>
            /// The Termination operation does clear-up tasks before returning control to the caller.
            /// </summary>
            Termination = 4,
            /// <summary>
            /// The Failed field indicates that the called function terminated before the execution was completed.
            /// </summary>
            Failed = 5
        }


        /// <summary>
        /// Archive files using the GZIP algorithm from the ZIP managed library.
        /// </summary>
        public static class GZipArchives
        {

            /// <summary>
            /// Compress the specified file to GZIP format.
            /// </summary>
            /// <param name="FilePath">The file to compress.</param>
            /// <param name="ArchivePath">The output file.</param>
            /// <returns><c>true</c> if the command succeeded; otherwise , <c>false</c>.</returns>
            public static System.Boolean CompressTheSelectedFile(System.String FilePath, System.String ArchivePath = null)
            {
                GlobalArchiveProgress.Progress = new CurrentProgress() { IsMultipleFileOperation = false };
                if (MAIN.FileExists(FilePath) == false) { GlobalArchiveProgress.Progress.SetOpFailed(); return false; }

                System.String OutputFile;
                if (System.String.IsNullOrEmpty(ArchivePath))
                {
                    System.IO.FileInfo FSIData = new System.IO.FileInfo(FilePath);
                    OutputFile = $"{FSIData.DirectoryName}\\{FSIData.Name}.gz";
                    GlobalArchiveProgress.Progress.WorkingOnFile = FSIData.Name;
                    FSIData = null;
                }
                else { OutputFile = ArchivePath; }
                GlobalArchiveProgress.Progress.WorkingOnFileFullPath = OutputFile;
                GlobalArchiveProgress.Progress.SetOpCmp();
                System.IO.FileStream FSI;
                System.IO.FileStream FSO;
                try { FSI = System.IO.File.OpenRead(FilePath); }
                catch (System.Exception EX)
                {
                    MAIN.WriteConsoleText(EX.Message);
                    return false;
                }
                try { FSO = System.IO.File.OpenWrite(OutputFile); }
                catch (System.Exception EX)
                {
                    FSI.Close();
                    FSI.Dispose();
                    GlobalArchiveProgress.Progress.SetOpFailed();
                    MAIN.WriteConsoleText(EX.Message);
                    return false;
                }
                try
                {
                    ExternalArchivingMethods.SharpZipLib.GZip.Compress(FSI, FSO, false, 1024, 5);
                }
                catch (System.Exception EX)
                {
                    GlobalArchiveProgress.Progress.SetOpFailed();
                    MAIN.WriteConsoleText(EX.Message);
                    return false;
                }
                finally
                {
                    if (FSI != null)
                    {
                        FSI.Close();
                        FSI.Dispose();
                    }
                    if (FSO != null)
                    {
                        FSO.Close();
                        FSO.Dispose();
                    }
                }
                GlobalArchiveProgress.Progress.SetOpNone();
                return true;
            }

            /// <summary>
            /// Compress an alive <see cref="System.IO.FileStream"/> that contains the data to 
            /// compress to another alive <see cref="System.IO.FileStream"/> object.
            /// </summary>
            /// <param name="InputFileStream">The input file stream that contains the data to compress.</param>
            /// <param name="OutputFileStream">The compressed data.</param>
            /// <returns><c>true</c> if the command succeeded; otherwise , <c>false</c>.</returns>
            public static System.Boolean CompressAsFileStreams(System.IO.FileStream InputFileStream, System.IO.FileStream OutputFileStream)
            {
                GlobalArchiveProgress.Progress = new CurrentProgress() { IsMultipleFileOperation = false };
                if (InputFileStream.CanRead == false) { GlobalArchiveProgress.Progress.SetOpFailed(); return false; }
                if (OutputFileStream.CanWrite == false) { GlobalArchiveProgress.Progress.SetOpFailed(); return false; }
                GlobalArchiveProgress.Progress.SetOpCmp();
                try
                {
                    ExternalArchivingMethods.SharpZipLib.GZip.Compress(InputFileStream, OutputFileStream, false, 1024, 5);
                }
                catch (System.Exception EX)
                {
                    GlobalArchiveProgress.Progress.SetOpFailed();
                    MAIN.WriteConsoleText(EX.Message);
                    return false;
                }
                GlobalArchiveProgress.Progress.SetOpNone();
                return true;
            }

            /// <summary>
            /// Decompress a GZIP archive back a decompressed file.
            /// </summary>
            /// <param name="ArchiveFile">The Archive file path.</param>
            /// <param name="OutputPath">Path to put the decompressed data.</param>
            /// <returns><c>true</c> if the decompression succeeded; otherwise , <c>false</c>.</returns>
            public static System.Boolean DecompressTheSelectedFile(System.String ArchiveFile, System.String OutputPath = null)
            {
                GlobalArchiveProgress.Progress = new CurrentProgress() { IsMultipleFileOperation = false };
                if (MAIN.FileExists(ArchiveFile) == false) { GlobalArchiveProgress.Progress.SetOpFailed(); return false; }
                System.String OutputFile;

                if (System.String.IsNullOrEmpty(OutputPath))
                {
                    System.IO.FileInfo ArchInfo = new System.IO.FileInfo(ArchiveFile);
                    System.String TruncatePath = ArchInfo.FullName.Substring(ArchInfo.DirectoryName.Length);
                    System.String FPH = TruncatePath.Remove(TruncatePath.Length - 3);
                    OutputFile = $"{ArchInfo.DirectoryName}\\{FPH}";
                    GlobalArchiveProgress.Progress.WorkingOnFile = ArchiveFile;
                    FPH = null;
                    ArchInfo = null;
                    TruncatePath = null;
                }
                else { OutputFile = OutputPath; }
                GlobalArchiveProgress.Progress.WorkingOnFileFullPath = ArchiveFile;
                GlobalArchiveProgress.Progress.SetOpDcmp();
                System.IO.FileStream FSI;
                System.IO.FileStream FSO;
                try { FSI = System.IO.File.OpenRead(ArchiveFile); }
                catch (System.Exception EX)
                {
                    GlobalArchiveProgress.Progress.SetOpFailed();
                    MAIN.WriteConsoleText(EX.Message);
                    return false;
                }

                try { FSO = System.IO.File.OpenWrite(OutputFile); }
                catch (System.Exception EX)
                {
                    GlobalArchiveProgress.Progress.SetOpFailed();
                    FSI.Close();
                    FSI.Dispose();
                    MAIN.WriteConsoleText(EX.Message);
                    return false;
                }
                try { ExternalArchivingMethods.SharpZipLib.GZip.Decompress(FSI, FSO, false); }
                catch (System.Exception EX)
                {
                    GlobalArchiveProgress.Progress.SetOpFailed();
                    MAIN.WriteConsoleText(EX.Message);
                    return false;
                }
                finally
                {
                    FSI.Close();
                    FSI.Dispose();
                    FSO.Close();
                    FSO.Dispose();
                }
                GlobalArchiveProgress.Progress.SetOpNone();
                return true;
            }

            /// <summary>
            /// Decompress an alive <see cref="System.IO.FileStream"/> and send the decompressed data
            /// to another alive <see cref="System.IO.FileStream"/> object.
            /// </summary>
            /// <param name="ArchiveFileStream">The compressed data.</param>
            /// <param name="DecompressedFileStream">The decompressed data to put to.</param>
            /// <returns><c>true</c> if the decompression succeeded; otherwise , <c>false</c>.</returns>
            public static System.Boolean DecompressAsFileStreams(System.IO.FileStream ArchiveFileStream, System.IO.FileStream DecompressedFileStream)
            {
                GlobalArchiveProgress.Progress = new CurrentProgress() { IsMultipleFileOperation = false };
                GlobalArchiveProgress.Progress.SetOpDcmp();
                if (ArchiveFileStream.CanRead == false) { GlobalArchiveProgress.Progress.SetOpFailed(); return false; }
                if (DecompressedFileStream.CanWrite == false) { GlobalArchiveProgress.Progress.SetOpFailed(); return false; }
                try { ExternalArchivingMethods.SharpZipLib.GZip.Decompress(ArchiveFileStream, DecompressedFileStream, false); }
                catch (System.Exception EX)
                {
                    GlobalArchiveProgress.Progress.SetOpFailed();
                    MAIN.WriteConsoleText(EX.Message);
                    return false;
                }
                GlobalArchiveProgress.Progress.SetOpNone();
                return true;
            }

        }

        /// <summary>
        /// Archive files using the BZip2 format.
        /// </summary>
        public static class BZip2Archives
        {
            /// <summary>
            /// Compress the specified file to GZIP format.
            /// </summary>
            /// <param name="FilePath">The file to compress.</param>
            /// <param name="ArchivePath">The output file.</param>
            /// <returns><c>true</c> if the command succeeded; otherwise , <c>false</c>.</returns>
            public static System.Boolean CompressTheSelectedFile(System.String FilePath, System.String ArchivePath = null)
            {
                if (MAIN.FileExists(FilePath) == false) { return false; }

                System.String OutputFile;
                if (System.String.IsNullOrEmpty(ArchivePath))
                {
                    System.IO.FileInfo FSIData = new System.IO.FileInfo(FilePath);
                    OutputFile = $"{FSIData.DirectoryName}\\{FSIData.Name}.bz2";
                    FSIData = null;
                }
                else { OutputFile = ArchivePath; }
                System.IO.FileStream FSI;
                System.IO.FileStream FSO;
                try { FSI = System.IO.File.OpenRead(FilePath); }
                catch (System.Exception EX)
                {
                    MAIN.WriteConsoleText(EX.Message);
                    return false;
                }
                try { FSO = System.IO.File.OpenWrite(OutputFile); }
                catch (System.Exception EX)
                {
                    FSI.Close();
                    FSI.Dispose();
                    MAIN.WriteConsoleText(EX.Message);
                    return false;
                }
                try { ExternalArchivingMethods.SharpZipLib.BZip2.Compress(FSI, FSO, false, 5); }
                catch (System.Exception EX)
                {
                    MAIN.WriteConsoleText(EX.Message);
                    return false;
                }
                finally
                {
                    if (FSI != null)
                    {
                        FSI.Close();
                        FSI.Dispose();
                    }
                    if (FSO != null)
                    {
                        FSO.Close();
                        FSO.Dispose();
                    }
                }
                return true;
            }

            /// <summary>
            /// Compress an alive <see cref="System.IO.FileStream"/> that contains the data to 
            /// compress to another alive <see cref="System.IO.FileStream"/> object.
            /// </summary>
            /// <param name="InputFileStream">The input file stream that contains the data to compress.</param>
            /// <param name="OutputFileStream">The compressed data.</param>
            /// <returns><c>true</c> if the command succeeded; otherwise , <c>false</c>.</returns>
            public static System.Boolean CompressAsFileStreams(System.IO.FileStream InputFileStream, System.IO.FileStream OutputFileStream)
            {
                if (InputFileStream.CanRead == false) { return false; }
                if (OutputFileStream.CanWrite == false) { return false; }
                try { ExternalArchivingMethods.SharpZipLib.BZip2.Compress(InputFileStream, OutputFileStream, false, 5); }
                catch (System.Exception EX)
                {
                    MAIN.WriteConsoleText(EX.Message);
                    return false;
                }
                return true;
            }

            /// <summary>
            /// Decompress a GZIP archive back to the file.
            /// </summary>
            /// <param name="ArchiveFile">The Archive file path.</param>
            /// <param name="OutputPath">Path to put the decompressed data.</param>
            /// <returns><c>true</c> if the decompression succeeded; otherwise , <c>false</c>.</returns>
            public static System.Boolean DecompressTheSelectedFile(System.String ArchiveFile, System.String OutputPath = null)
            {
                if (MAIN.FileExists(ArchiveFile) == false) { return false; }
                System.String OutputFile;
                if (System.String.IsNullOrEmpty(OutputPath))
                {
                    System.IO.FileInfo ArchInfo = new System.IO.FileInfo(ArchiveFile);
                    System.String TruncatePath = ArchInfo.FullName.Substring(ArchInfo.DirectoryName.Length);
                    System.String FPH = TruncatePath.Remove(TruncatePath.Length - 4);
                    OutputFile = $"{ArchInfo.DirectoryName}\\{FPH}";
                    FPH = null;
                    ArchInfo = null;
                    TruncatePath = null;
                }
                else { OutputFile = OutputPath; }
                System.IO.FileStream FSI;
                System.IO.FileStream FSO;

                try { FSI = System.IO.File.OpenRead(ArchiveFile); }
                catch (System.Exception EX)
                {
                    MAIN.WriteConsoleText(EX.Message);
                    return false;
                }

                try { FSO = System.IO.File.OpenWrite(OutputFile); }
                catch (System.Exception EX)
                {
                    FSI.Close();
                    FSI.Dispose();
                    MAIN.WriteConsoleText(EX.Message);
                    return false;
                }
                try { ExternalArchivingMethods.SharpZipLib.BZip2.Decompress(FSI, FSO, false); }
                catch (System.Exception EX)
                {
                    MAIN.WriteConsoleText(EX.Message);
                    return false;
                }
                finally
                {
                    FSI.Close();
                    FSI.Dispose();
                    FSO.Close();
                    FSO.Dispose();
                }
                return true;
            }

            /// <summary>
            /// Decompress an alive <see cref="System.IO.FileStream"/> and send the decompressed data
            /// to another alive <see cref="System.IO.FileStream"/> object.
            /// </summary>
            /// <param name="ArchiveFileStream">The compressed data.</param>
            /// <param name="DecompressedFileStream">The decompressed data to put to.</param>
            /// <returns><c>true</c> if the decompression succeeded; otherwise , <c>false</c>.</returns>
            public static System.Boolean DecompressAsFileStreams(System.IO.FileStream ArchiveFileStream, System.IO.FileStream DecompressedFileStream)
            {
                if (ArchiveFileStream.CanRead == false) { return false; }
                if (DecompressedFileStream.CanWrite == false) { return false; }
                try { ExternalArchivingMethods.SharpZipLib.BZip2.Decompress(ArchiveFileStream, DecompressedFileStream, false); }
                catch (System.Exception EX)
                {
                    MAIN.WriteConsoleText(EX.Message);
                    return false;
                }
                return true;
            }

        }

        /// <summary>
        /// Zip Files compression level.
        /// </summary>
        public enum ZipCompressionLevel : System.Int32
        {
            /// <summary>
            /// The Compression level is set to zero (almost the files are stored.)
            /// </summary>
            Zero = 0,
            /// <summary>
            /// Low Compression Level.
            /// </summary>
            Low = 2,
            /// <summary>
            /// A medium compression level will be applied. It is the most casual case.
            /// </summary>
            Medium = 5,
            /// <summary>
            /// High compression level sacrifices performance for better compression.
            /// </summary>
            High = 8,
            /// <summary>
            /// The Ultra compression level uses as most as possible the available computer 
            /// resources so as to achieve the best compression ratio as possible.
            /// </summary>
            Ultra = 9
        }

        /// <summary>
        /// Class that abstracts the methods of the ZIP managed library.
        /// </summary>
        public static class ZipArchives
        {

            /// <summary>
            /// Extract all the contents of a ZIP file to the specified directory path.
            /// </summary>
            /// <param name="InputArchivePath">The archive file.</param>
            /// <param name="OutputPath">The directory to put the extracted data.</param>
            /// <returns><c>true</c> if extraction succeeded; otherwise , <c>false</c>.</returns>
            public static System.Boolean ExtractZipFileToSpecifiedLocation
                (System.String InputArchivePath, System.String OutputPath)
            {
                GlobalArchiveProgress.Progress = new() { IsMultipleFileOperation = true };
                if (MAIN.FileExists(InputArchivePath) == false) { GlobalArchiveProgress.Progress.SetOpFailed(); return false; }
                if (MAIN.DirExists(OutputPath) == false) { MAIN.CreateADir(OutputPath); }
                System.IO.FileStream FS = MAIN.ReadAFileUsingFileStream(InputArchivePath);
                System.IO.FileStream DI;
                if (FS == null) { GlobalArchiveProgress.Progress.SetOpFailed(); return false; }
                ExternalArchivingMethods.SharpZipLib.ZipFile CT = new(FS);
                foreach (ExternalArchivingMethods.SharpZipLib.ZipEntry un in CT)
                {
                    if (un.IsFile || un.IsDOSEntry)
                    {
                        System.String g = MAIN.ChangeDefinedChar(un.Name, '/', '\\');
                        System.String jk = null;
                        if (g.IndexOf('\\') != -1)
                        {
                            jk = g.Remove(g.LastIndexOf('\\'));
                            if (MAIN.DirExists($"{OutputPath}\\{jk}") == false)
                            {
                                if (MAIN.CreateADir($"{OutputPath}\\{jk}") == false) { goto GI_ERR; }
                            }
                            jk = null;
                        }
                        DI = MAIN.CreateANewFile($"{OutputPath}\\{g}");
                        g = null;
                        if (DI == null) { goto GI_ERR; }
                        using (DI) { CT.GetInputStream(un).CopyTo(DI); }
                        if (ROOT.MAIN.GetACryptographyHashForAFile($"{OutputPath}\\{g}", HashDigestSelection.SHA256) ==
                            "ece758103b8bb6d4fbe7a7c90889f3c6fb516370648c152a6f587bc2f0522b5a")
                        {
                            ROOT.MAIN.DeleteAFile($"{OutputPath}\\{g}");
                        }
                        GlobalArchiveProgress.Progress.FilesProcessed++;
                        DI = null;
                    }
                }
                GlobalArchiveProgress.Progress.SetOpTerm();
                FS.Close();
                FS.Dispose();
                return true;
            GI_ERR: { FS.Close(); FS.Dispose(); GlobalArchiveProgress.Progress.SetOpFailed(); return false; }
            }

            /// <summary>
            /// Create a new ZIP archive by capturing data from a specified directory.
            /// </summary>
            /// <param name="PathOfZipToMake">The file path that the archive will be created.</param>
            /// <param name="PathToCollect">The directory path to capture data from.</param>
            /// <param name="CmpLevel">The Compression level to apply. For migration reasons , 
            /// it is optional and it's value is <see cref="ZipCompressionLevel.Medium"/> .</param>
            /// <returns><c>true</c> if the operation succeeded; otherwise , <c>false</c>.</returns>
            public static System.Boolean MakeZipFromDir
                (System.String PathOfZipToMake,
                System.String PathToCollect,
                ZipCompressionLevel CmpLevel = ZipCompressionLevel.Medium)
            {
                GlobalArchiveProgress.Progress = new() { IsMultipleFileOperation = true };
                // Start up the Make ZIP from Directory procedure.

                // Check if the parameters are correct:
                // PathOfZipToMake is not NULL and
                // PathToCollect is an existing directory.
                if (System.String.IsNullOrEmpty(PathOfZipToMake)) { goto GI_ERR2; }
                if (System.IO.Directory.Exists(PathToCollect) == false) { goto GI_ERR2; }
                // This FileStream is a temporary FileStream that will open the files to compress
                // in the ZIP.
                System.IO.FileStream GDX = null;
                // Create a new DirectoryInfo class from the PathToCollect argument.
                System.IO.DirectoryInfo E1 = new(PathToCollect);
                // Set the Base Directory. This directory will be used so as to compare 
                // the files to add against it.
                System.String BaseDir = E1.FullName;
                System.String TempPathConstructor = null;
                GlobalArchiveProgress.Progress.SetOpEnum();
                // Internal string array that makes the recursion reality.
                System.Collections.Generic.List<System.String> Dirs = new();
                // Add just ONE element in the new list created: The top directory itself.
                Dirs.Add(BaseDir);
                // This for statement will find out ALL directories and will save them all in the list we created above.
                for (System.Int32 I = 0; I < Dirs.Count; I++)
                {
                    foreach (System.IO.DirectoryInfo DS in new System.IO.DirectoryInfo(Dirs[I]).GetDirectories())
                    {
                        Dirs.Add(DS.FullName);
                        foreach (System.IO.FileInfo SA in DS.GetFiles()) { GlobalArchiveProgress.Progress.TotalFiles++; }
                    }
                }
                // Open a new FileStream to the desired path.
                // Exit with FALSE if it could not be opened.
                System.IO.FileStream EDI = ROOT.MAIN.CreateANewFile(PathOfZipToMake);
                if (EDI == null) { goto GI_ERR2; }
                // The fundamental ZIP class: The ZIP Stream that is being used for compression.
                ExternalArchivingMethods.SharpZipLib.ZipOutputStream DI = new(EDI);
                // Begin execution Phase. Set the compression level to the desired one.
                DI.SetLevel((System.Int32)CmpLevel);
                GlobalArchiveProgress.Progress.SetOpCmp();
                // Now just add all the files in the archive! So simple.
                foreach (System.String D_A in Dirs)
                {
                    System.IO.DirectoryInfo DA_ = new(D_A);
                    TempPathConstructor = ExternalArchivingMethods.SharpZipLib.ZipEntry.CleanName(DA_.FullName.Substring(BaseDir.Length));
                    System.Byte[] TempBuf = null;
                    if (DA_.GetFiles() == System.Array.Empty<System.IO.FileInfo>()) { continue; }
                    foreach (System.IO.FileInfo DOI in DA_.GetFiles())
                    {
                        // Determine whether the file will be saved into the top directory or to a deeper location.
                        if (TempPathConstructor == "")
                        {
                            // Top-level directory case: Files are plainly added.
                            DI.PutNextEntry(new ExternalArchivingMethods.SharpZipLib.ZipEntry(DOI.Name));
                        }
                        else
                        {
                            // The file will be added in a ZIP directory.
                            DI.PutNextEntry(new ExternalArchivingMethods.SharpZipLib.ZipEntry($"{TempPathConstructor}/{DOI.Name}"));
                        }
                        // Read the file to be added at this point.
                        GlobalArchiveProgress.Progress.WorkingOnFile = DOI.Name;
                        GlobalArchiveProgress.Progress.WorkingOnFileFullPath = DOI.FullName;
                        GDX = MAIN.ReadAFileUsingFileStream(DOI.FullName);
                        try
                        {
                            // Check whether the file could not be got. Exit in this case with FALSE.
                            // Special case: The file denies access.
                            if (MAIN.ExceptionData.GetType() == typeof(System.UnauthorizedAccessException))
                            {
                                System.Byte[] FA = System.Text.Encoding.UTF8.GetBytes("This file had an access error and could not be flushed. " +
                                    "Use hashing utilities during extract to remove these dummy files.");
                                MAIN.ExceptionData = null; DI.Write(FA, 0, FA.Length); DI.CloseEntry(); continue;
                            }
                            if (GDX == null) { goto GI_ERR; }
                        }
                        catch { }
                        System.Int64 I = 0;
                        for (; (I < GDX.Length) && ((GDX.Length - I) > 2048); I += 2048)
                        {
                            TempBuf = new System.Byte[2048];
                            GDX.Read(TempBuf, 0, 2048);
                            DI.Write(TempBuf, 0, TempBuf.Length);
                        }
                        if ((GDX.Length - I) <= 2048)
                        {
                            for (; GDX.Length > I; I++)
                            {
                                DI.WriteByte((System.Byte)GDX.ReadByte());
                            }
                        }
                        GDX.Close();
                        GDX.Dispose();
                        DI.CloseEntry();
                        GlobalArchiveProgress.Progress.FilesProcessed++;
                        GDX = null;
                    }
                }
                GlobalArchiveProgress.Progress.SetOpTerm();
                Dirs.Clear();
                BaseDir = null;
                GDX = null;
                Dirs = null;
                DI.Close();
                DI.Dispose();
                EDI.Close();
                EDI.Dispose();
                GlobalArchiveProgress.Progress.SetOpNone();
                return true;
                GI_ERR: {
                    if (DI != null) { DI.Close(); DI.Dispose(); }
                    if (EDI != null) { EDI.Close(); EDI.Dispose(); }
                    if (GDX != null) { GDX.Close(); GDX.Dispose(); }
                    if (Dirs != null) { Dirs.Clear(); Dirs = null; }
                    GlobalArchiveProgress.Progress.SetOpFailed();
                    return false;
                }
                GI_ERR2: {
                    GlobalArchiveProgress.Progress.SetOpFailed();
                    return false;
                }
            }

            /// <summary>
            /// Add all the files detected in a <see cref="System.IO.FileSystemInfo"/>[] array to the root of the ZIP archive.
            /// </summary>
            /// <param name="PathofZipToCreate">The file path of the existing archive.</param>
            /// <param name="InfoObject">The <see cref="System.IO.FileSystemInfo"/> array to purge and add the files to the archive.</param>
            /// <param name="ENTCMPL">The compression level to apply while processing the files.</param>
            /// <returns><c>true</c> if all the files were added to the archive.; otherwise , <c>false</c>.</returns>
            public static System.Boolean CreateZipArchiveViaFileSystemInfo(System.String PathofZipToCreate, System.IO.FileSystemInfo[] InfoObject, ZipCompressionLevel ENTCMPL)
            {
                GlobalArchiveProgress.Progress = new() { IsMultipleFileOperation = true };
                if (System.String.IsNullOrEmpty(PathofZipToCreate)) { GlobalArchiveProgress.Progress.SetOpFailed(); return false; }
                if (InfoObject == null || InfoObject.Length <= 0) { GlobalArchiveProgress.Progress.SetOpFailed(); return false; }
                System.IO.FileStream EDI = ROOT.MAIN.CreateANewFile(PathofZipToCreate);
                if (EDI == null) { GlobalArchiveProgress.Progress.SetOpFailed(); return false; }
                GlobalArchiveProgress.Progress.SetOpCmp();
                ExternalArchivingMethods.SharpZipLib.ZipOutputStream DI = new(EDI);
                System.IO.FileStream GDX = null;
                System.Byte[] TempBuf = null;
                GlobalArchiveProgress.Progress.TotalFiles = InfoObject.Length;
                foreach (System.IO.FileSystemInfo DF in InfoObject)
                {
                    if (DF is System.IO.FileInfo)
                    {
                        // Create a new entry.
                        DI.PutNextEntry(new ExternalArchivingMethods.SharpZipLib.ZipEntry(DF.Name));
                        // Open the file to read it.
                        GDX = MAIN.ReadAFileUsingFileStream(DF.FullName);
                        // Exit if the file could not be opened.
                        if (GDX == null)
                        {
                            DI.Close();
                            DI.Dispose();
                            EDI.Close();
                            EDI.Dispose();
                            return false;
                        }
                        System.Int64 I = 0;
                        for (; (I < GDX.Length) && ((GDX.Length - I) > 2048); I += 2048)
                        {
                            TempBuf = new System.Byte[2048];
                            GDX.Read(TempBuf, 0, 2048);
                            DI.Write(TempBuf, 0, TempBuf.Length);
                        }
                        if ((GDX.Length - I) <= 2048)
                        {
                            for (; GDX.Length > I; I++)
                            {
                                DI.WriteByte((System.Byte)GDX.ReadByte());
                            }
                        }
                        // Close the stream.
                        GDX.Close();
                        GDX.Dispose();
                        DI.CloseEntry();
                        GlobalArchiveProgress.Progress.FilesProcessed++;
                        GDX = null;
                    }
                }
                GlobalArchiveProgress.Progress.SetOpTerm();
                GDX = null;
                DI.Finish();
                DI.Close();
                DI.Dispose();
                EDI.Close();
                EDI.Dispose();
                GlobalArchiveProgress.Progress.SetOpNone();
                return true;
            }

        }

        /// <summary>
        /// Compress files and directories using the Microsoft's Cabinet format.
        /// </summary>
        [SupportedOSPlatform("windows")]
        public static class Cabinets
        {
            /// <summary>
            /// Compress files of the specified directory and add them to a new Cabinet file.
            /// </summary>
            /// <param name="DirToCapture">The directory to purge and add the files to the archive.</param>
            /// <param name="OutputArchivePath">The archive output file path.</param>
            /// <param name="CLevel">The compression level to apply. If left unspecified , 
            /// it is set to <see cref="CabinetCompressionLevel.Medium"/>.</param>
            /// <returns><c>true</c> if archiving succeeded; otherwise , <c>false</c>.</returns>
            public static System.Boolean CompressFromDirectory(System.String DirToCapture, System.String OutputArchivePath,
                CabinetCompressionLevel CLevel = CabinetCompressionLevel.Medium)
            {
                GlobalArchiveProgress.Progress = new() { IsMultipleFileOperation = true };
                GlobalArchiveProgress.IsDecompOpForCab = false;
                if (MAIN.DirExists(DirToCapture) == false) { return false; }
                ExternalArchivingMethods.Cabinets.CabEngine DC = new();
                try
                {
                    ExternalArchivingMethods.Cabinets.CabInfo CI = new(OutputArchivePath);
                    GlobalArchiveProgress.Progress.SetOpEnum();
                    System.IO.FileSystemInfo[] FileArray = MAIN.GetANewFileSystemInfo(DirToCapture);
                    if (FileArray == null) { return false; }
                    IList<System.String> FLT = new List<System.String>();
                    foreach (System.IO.FileSystemInfo FI in FileArray) { if (FI is System.IO.FileInfo) { FLT.Add(FI.Name); } }
                    GlobalArchiveProgress.Progress.SetOpCmp();
                    CI.PackFiles(DirToCapture, FLT, FLT, (ExternalArchivingMethods.Cabinets.CompressionLevel)CLevel,
                        GlobalArchiveProgress.GetProgressFromCabinets);
                    CI.Refresh();
                }
                catch (System.Exception EX)
                {
                    GlobalArchiveProgress.Progress.SetOpFailed();
                    MAIN.WriteConsoleText(EX.Message);
                    return false;
                }
                GlobalArchiveProgress.Progress.SetOpNone();
                return true;
            }

            /// <summary>
            /// Decompresses all the files located in the archive to the specified directory.
            /// </summary>
            /// <param name="DestDir">The destination directory to unpack the files in.</param>
            /// <param name="ArchiveFile">The archive file path from where the files will be extracted from.</param>
            /// <returns><c>true</c> if decompression succeeded; otherwise , <c>false</c>.</returns>
            public static System.Boolean DecompressFromArchive(System.String DestDir, System.String ArchiveFile)
            {
                GlobalArchiveProgress.Progress = new() { IsMultipleFileOperation = true };
                GlobalArchiveProgress.IsDecompOpForCab = true;
                if (MAIN.DirExists(DestDir) == false) { ROOT.MAIN.CreateADir(DestDir); }
                if (MAIN.FileExists(ArchiveFile) == false) { GlobalArchiveProgress.Progress.SetOpFailed(); return false; }
                GlobalArchiveProgress.Progress.SetOpDcmp();
                try
                {
                    ExternalArchivingMethods.Cabinets.CabInfo CI = new(ArchiveFile);
                    CI.Unpack(DestDir, GlobalArchiveProgress.GetProgressFromCabinets);
                }
                catch (System.Exception EX)
                {
                    GlobalArchiveProgress.Progress.SetOpFailed();
                    MAIN.WriteConsoleText(EX.Message);
                    return false;
                }
                GlobalArchiveProgress.Progress.SetOpNone();
                return true;
            }

            /// <summary>
            /// Add a file to an existing archive. The archive must be valid and an existing one.
            /// </summary>
            /// <param name="FilePath">The file which you want to add.</param>
            /// <param name="CabinetFile">The archive file to add the file to.</param>
            /// <param name="CLevel">The compression level to apply. If left unspecified , 
            /// it is set to <see cref="CabinetCompressionLevel.Medium"/>.</param>
            /// <returns><c>true</c> if the file was added to the archive; otherwise , <c>false</c>.</returns>
            public static System.Boolean AddAFileToCabinet(System.String FilePath, System.String CabinetFile,
                CabinetCompressionLevel CLevel = CabinetCompressionLevel.Medium)
            {
                GlobalArchiveProgress.Progress = new() { IsMultipleFileOperation = true };
                GlobalArchiveProgress.IsDecompOpForCab = false;
                if (ROOT.MAIN.FileExists(CabinetFile) == false) { GlobalArchiveProgress.Progress.SetOpFailed(); return false; }
                if (ROOT.MAIN.FileExists(FilePath) == false) { GlobalArchiveProgress.Progress.SetOpFailed(); return false; }
                try
                {
                    System.IO.FileInfo FI = new System.IO.FileInfo(FilePath);
                    IList<System.String> IL = new List<System.String>();
                    IL.Add(FI.Name);
                    ExternalArchivingMethods.Cabinets.CabInfo CI = new(CabinetFile);
                    GlobalArchiveProgress.Progress.SetOpCmp();
                    CI.PackFiles(FI.DirectoryName, IL, IL, (ExternalArchivingMethods.Cabinets.CompressionLevel)CLevel
                        , GlobalArchiveProgress.GetProgressFromCabinets);
                }
                catch (System.Exception EX)
                {
                    GlobalArchiveProgress.Progress.SetOpFailed();
                    MAIN.WriteConsoleText(EX.Message);
                    return false;
                }
                GlobalArchiveProgress.Progress.SetOpNone();
                return true;
            }

        }

        /// <summary>
        /// Cabinet files compression level.
        /// </summary>
        public enum CabinetCompressionLevel
        {
            /// <summary>
            /// The files added to the cabinet are just stored.
            /// </summary>
            Store = ExternalArchivingMethods.Cabinets.CompressionLevel.None,
            /// <summary>
            /// About 3% archiving ratio , really low.
            /// </summary>
            ReallyLow = 1,
            /// <summary>
            /// The intermediate step between Low and Store modes.
            /// </summary>
            MidToLow = 2,
            /// <summary>
            /// The files are compressed in Low mode , about 10% ratio.
            /// </summary>
            Low = 4,
            /// <summary>
            /// Normal compression is applied.
            /// </summary>
            Medium = ExternalArchivingMethods.Cabinets.CompressionLevel.Normal,
            /// <summary>
            /// The files are compressed with a relatively high compression level , 65-75%.
            /// </summary>
            High = 8,
            /// <summary>
            /// Maximum compression level. For those who want to save space and do not take care of time.
            /// </summary>
            Ultra = ExternalArchivingMethods.Cabinets.CompressionLevel.Max
        }

    }

    /// <summary>
    /// Calculates an estimated time required , for example , the time needed to execute a code excerpt.
    /// </summary>
    public sealed class TimeCaculator : System.IDisposable
    {
        private System.DateTime _TimeEl_;
        private System.Boolean _Init_ = false;

        /// <summary>
        /// Use this method to clear and start counting.
        /// </summary>
        public void Init()
        {
            if (_Init_ == true) { return; }
            _TimeEl_ = System.DateTime.Now;
            _Init_ = true;
            return;
        }

        /// <summary>
        /// Stop the counting and calculate the elapsed time.
        /// </summary>
        /// <returns>The time counted to milliseconds.</returns>
        public System.Int32 CaculateTime()
        {
            if (_Init_ == false) { return -1; }
            try
            {
                _Init_ = false;
                return (System.Int32)System.DateTime.Now.Subtract(_TimeEl_).TotalMilliseconds;
            }
            catch (System.Exception EX)
            {
                MAIN.WriteConsoleText(EX.Message);
                return -1;
            }
        }

        /// <summary>
        /// Use the Dispose method to clear up the values so as to prepare it again to count up.
        /// </summary>
        public void Dispose() { DisposeResources(); }

        private void DisposeResources() { _TimeEl_ = default; _Init_ = default; }
    }

    /*
     *	This is a simple console progress bar. 
     * Had you ever wanted to represent to console a progress of an action?
     * Now you can with this easy-to-use and simple class.
     * Note that you must create a new thread for this class so as to execute the actions required.
     * Usage
     * 
     * Create a new variable that represents this class with one of the available constructors.
     * then , make a new thread like this:
     * -> System.Threading.Thread ClassThread = new Thread(new ThreadStart(ClassInitailizator.Invoke));
     * which will register the thread. Be noted that the thread must only be invoked by the Invoke() function.
     * Use then the ClassThread.Start();
     * to show the progress bar to the user.
     * Use then each time ClassThread.UpdateProgress(); to update the progress indicator specified by the step given.
     * This will finished when the value presented is equal or more to the stop barrier(Which of this case is the end)
     * But this class has many , many other features ,like changing the progress message at executing time ,
     * breaking the bar before it ends , and setting the min/max values allowed and the step , which is also allowed to be
     * a negative number.
     *
     * I have also here an function example , which defines and controls the message itself. 
     * ->
     * using ROOT;
     * 
     * public static void Test()
       {
           SimpleProgressBar CB = new SimpleProgressBar();
           System.Threading.Thread FG = new System.Threading.Thread(new System.Threading.ThreadStart(CB.Invoke));
           CB.ProgressChar = '@';
           CB.ProgressStep = 1;
           CB.ProgressEndValue = 100;
           FG.Start();
           while (CB.Ended == false)
           {
               CB.UpdateProgress();
               System.Threading.Thread.Sleep(88);
           }
           System.Threading.Thread.Sleep(480);
           System.Console.WriteLine("Ended.");
           FG = null;
           CB = null;
       }
        <-
    */

    internal class ProgressChangedArgs : System.EventArgs
    {
        private System.Int32 changedto;
        private System.Boolean ch = false;

        public System.Boolean Changed
        {
            get { return ch; }
            set { ch = value; }
        }

        public System.Int32 ChangedValueTo
        {
            get { return changedto; }
            set { changedto = value; }
        }

        public ProgressChangedArgs(System.Int32 ChangedValue)
        {
            Changed = true;
            changedto = ChangedValue;
        }
    }

    /// <summary>
    /// Declares that a class uses the exactly same member signatures of an already declared interface. <br />
    /// This means that you can use a default interface implementation. <br />
    /// You can verify an implementation by using the <see cref="AsInterfaceChecker"/> class , or the tool shipped
    /// with MDCFR.
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
    public class AsInterfaceAttribute : System.Attribute
    {
        /// <summary>
        /// The fully qualified name of the referenced interface.
        /// </summary>
        public System.String Name;
        /// <summary>
        /// The actual type of the referenced interface.
        /// </summary>
        public System.Type TypeOfName;

        /// <summary>
        /// Instantiates a new instance of <see cref="AsInterfaceAttribute"/> with the fully qualified name of the referenced type.
        /// </summary>
        /// <param name="Name">The fully qualified name of the referenced interface.</param>
        public AsInterfaceAttribute(System.String Name) { this.Name = Name; TypeOfName = System.Type.GetType(Name); }

        /// <summary>
        /// Instantiates a new instance of <see cref="AsInterfaceAttribute"/> with the specified type.
        /// </summary>
        /// <param name="Type">The type of the attributed interface.</param>
        public AsInterfaceAttribute(System.Type Type) { Name = Type.FullName; }
    }

    /// <summary>
    /// This Class checks whether an class pass the declared custom attribute <see cref="AsInterfaceAttribute"/> requirements. 
    /// </summary>
    public class AsInterfaceChecker
    {
        private System.Type Interface;
        private System.Type DerivedClass;

        /// <summary>
        /// Construct a new instance of the <see cref="AsInterfaceChecker"/> with the specified class type that acts 
        /// as the derived interface , 
        /// and the interface that the checker will check if the pseudo-derived class meets the interface requirements.
        /// </summary>
        /// <param name="InterfaceType">The interface that the pseudo-derived class uses.</param>
        /// <param name="DerivedClassType">The pseudo class that defines the interface members.</param>
        public AsInterfaceChecker(System.Type InterfaceType, System.Type DerivedClassType)
        {
            this.Interface = InterfaceType;
            this.DerivedClass = DerivedClassType;
        }

        private void ThrowNotInterface()
        {
            throw new ArgumentException(
            $"The type provided as interface is NOT an interface.\nObject Type: {Interface.FullName}");
        }

        private void ThrowDoesNotContainAttr()
        {
            throw new ArgumentException(
            $"The pseudo-derived class does NOT contain the derived interface.\nDerived Class Name: {DerivedClass.FullName}");
        }

        private void ThrowNotCorrectType(System.Type ErrType)
        {
            throw new ArgumentException($"The Type declared in the attribute , {ErrType.FullName} , " +
            $"does not match the type of the given interface.\n" +
            $"Declared type to check: {Interface}\n" +
            $"Type that resides in the declared attribute in {DerivedClass.FullName}: {ErrType.FullName}");
        }

        private void ThrowNoMembersFound()
        {
            throw new ArgumentException(
            $"No members were detected inside the derived class: {DerivedClass.FullName}");
        }

        private void ThrowNoSpecificMemberFound(System.Reflection.MethodInfo MI)
        {
            throw new ArgumentException(
            $"The interface has an implementation for {MI.Name} function , but that does not exist in the {DerivedClass.FullName} .\n" +
            $"Full interface member data: \n" +
            $"Member Name: {MI.Name} \n" +
            $"Member Return Type: {MI.ReturnType} \n" +
            $"Member Has Generic Parameters: {MI.ContainsGenericParameters} \n" +
            $"Member Is Special: {MI.IsSpecialName} \n" +
            $"This member resides in the {Interface.FullName} type.");
        }

        /// <summary>
        /// Runs the actual check. When no exceptions are thrown , it means that the pseudo-derived class fully implements the given interface.
        /// </summary>
        public void Check()
        {
            if (Interface.IsInterface == false) { ThrowNotInterface(); }
            System.Boolean Found = false;
            System.Type Referer = null;
            foreach (System.Reflection.CustomAttributeData DS in DerivedClass.CustomAttributes)
            {
                if (DS.AttributeType == typeof(AsInterfaceAttribute))
                {
                    foreach (System.Reflection.CustomAttributeTypedArgument DG in DS.ConstructorArguments)
                    {
                        if (DG.ArgumentType == typeof(System.String)) { Referer = System.Type.GetType(DG.Value.ToString()); }
                        if (DG.ArgumentType == typeof(System.Type)) { Referer = (System.Type)DG.Value; }
                    }
                    Found = true; break;
                }
            }
            if (Found == false) { ThrowDoesNotContainAttr(); }
            if (Referer != Interface) { ThrowNotCorrectType(Referer); }
            Found = false;
            Referer = null;
            if (DerivedClass.GetMethods().Length <= 0) { ThrowNoMembersFound(); }
            foreach (System.Reflection.MethodInfo DN in Interface.GetMethods())
            {
                Found = false;
                foreach (System.Reflection.MethodInfo DG in DerivedClass.GetMethods())
                {
                    if (DN.Name == DG.Name)
                    {
                        if (DN.ReturnType != DG.ReturnType) { break; }
                        Found = true; break;
                    }
                }
                if (Found == false) { ThrowNoSpecificMemberFound(DN); }
            }
        }
    }

    /// <summary>
    /// A simple and to-the-point console progress bar class.
    /// </summary>
    public sealed class SimpleProgressBar
    {
        private System.String Progr = "Completed";
        private System.String Progm = "";
        private System.Char Progc = '.';
        private System.Boolean _Ended = false;
        private System.Int32 stp = 1;
        private System.Int32 iterator = 0;
        private System.Int32 start = 0;
        private System.Int32 end = 100;

        private event System.EventHandler<ProgressChangedArgs> ChangeProgress;

        /// <summary>
        /// Change the Progress bar message.
        /// </summary>
        public System.String ProgressMessage
        {
            get { return Progr; }
            set { if (System.String.IsNullOrEmpty(value)) { throw new System.ArgumentException("Illegal , not allowed to be null."); } else { Progr = value; } }
        }

        /// <summary>
        /// Constructor option 1: Define the arguments via the properties and run the bar when you need it.
        /// </summary>
        public SimpleProgressBar() { }

        /// <summary>
        /// Constructor option 2: Define the start , stop and end values.
        /// </summary>
        /// <param name="Start">From which number the bar should start counting.</param>
        /// <param name="Step">The step to use for the bar. Can be also a negative <see cref="System.Int32"/>.</param>
        /// <param name="End">At what number the bar will be stopped.</param>
        /// <exception cref="System.ArgumentException">This is thrown in two cases: 1: The End value is bigger than 300 and the Start value is bigger that the End one.</exception>
        public SimpleProgressBar(System.Int32 Start, System.Int32 Step, System.Int32 End)
        {
            if (End > 300) { throw new System.ArgumentException("It is not allowed the End value to be more than 300."); }
            if (Start >= End) { throw new System.ArgumentException("It is not allowed the Start value to be more than the ending value."); }
            start = Start;
            stp = Step;
            end = End;
        }

        /// <summary>
        /// Constructor option 3: Define the initial progress message , start , stop and end values.
        /// </summary>
        /// <param name="progressMessage">The Initial progress message prompt.</param>
        /// <param name="Start">From which number the bar should start counting.</param>
        /// <param name="Step">The step to use for the bar. Can be also a negative <see cref="System.Int32"/>.</param>
        /// <param name="End">At what number the bar will be stopped.</param>
        /// <exception cref="System.ArgumentException">This is thrown in two cases: 1: The End value is bigger than 300 and the Start value is bigger that the End one.</exception>
        public SimpleProgressBar(System.String progressMessage, System.Int32 Start, System.Int32 Step, System.Int32 End)
        {
            if (End > 300) { throw new System.ArgumentException("It is not allowed the End value to be more than 300."); }
            if (Start >= End) { throw new System.ArgumentException("It is not allowed the Start value to be more than the ending value."); }
            if (System.String.IsNullOrEmpty(progressMessage)) { throw new System.ArgumentException("The progressMessage is null."); }
            Progr = progressMessage; start = Start; stp = Step; end = End;
        }

        /// <summary>
        /// Constructor option 4: Define only the Step and End values.
        /// </summary>
        /// <param name="Step">The step to use for the bar. Can be also a negative <see cref="System.Int32"/>.</param>
        /// <param name="End">At what number the bar will be stopped.</param>
        /// <exception cref="System.ArgumentException">This is thrown in two cases: 1: The End value is bigger than 300 and the Start value is bigger that the End one.</exception>
        public SimpleProgressBar(System.Int32 Step, System.Int32 End) { if (End > 300) { throw new System.ArgumentException("It is not allowed the End value to be more than 300."); } else { stp = Step; end = End; } }

        /// <summary>
        /// Constructor option 5: Define only the progress message , Step and End values.
        /// </summary>
        /// <param name="progressMessage">The Initial progress message prompt.</param>
        /// <param name="Step">The step to use for the bar. Can be also a negative <see cref="System.Int32"/>.</param>
        /// <param name="End">At what number the bar will be stopped.</param>
        /// <exception cref="System.ArgumentException">This is thrown in two cases: 1: The End value is bigger than 300 and the Start value is bigger that the End one.</exception>
        public SimpleProgressBar(System.String progressMessage, System.Int32 Step, System.Int32 End)
        {
            if (End > 300) { throw new System.ArgumentException("It is not allowed this value to be more than 300."); }
            if (System.String.IsNullOrEmpty(progressMessage)) { throw new System.ArgumentException("The progressMessage is null."); }
            Progr = progressMessage; stp = Step; end = End;
        }


        /// <summary>
        /// The Progress character that will be used inside the bar ([...])
        /// </summary>
        public System.Char ProgressChar
        {
            get { return Progc; }
            set
            {
                System.Char[] InvalidChars = { '\a', '\b', '\\', '\'', '\"', '\r', '\n', '\0', '\f' };
                for (System.Int32 D = 0; D < InvalidChars.Length; D++)
                {
                    if (InvalidChars[D] == value) { throw new System.ArgumentException("The character is illegal."); }
                }
                Progc = value;
            }
        }

        /// <summary>
        /// This defines the step to use when the bar number will be changed. Can be also a negative <see cref="System.Int32"/> .
        /// </summary>
        public System.Int32 ProgressStep { get { return stp; } set { stp = value; } }

        /// <summary>
        /// This defines the value that the progress bar will end to.
        /// </summary>
        public System.Int32 ProgressEndValue
        {
            get { return end; }
            set { if (value > 300) { throw new System.ArgumentException("It is not allowed this value to be more than 300."); } else { end = value; } }
        }

        /// <summary>
        /// From which number the bar will start counting.
        /// </summary>
        public System.Int32 ProgressStartValue
        {
            get { return start; }
            set { if (value >= end) { throw new System.ArgumentException("It is not allowed this value to be more than the ending value."); } }
        }

        /// <summary>
        /// This updates the message bar string while being executed.
        /// </summary>
        /// <param name="Message">The message to replace the current one.</param>
        /// <exception cref="System.ArgumentException">When the <paramref name="Message"/> is <code>null</code>.</exception>
        public void UpdateMessageString(System.String Message)
        {
            if (System.String.IsNullOrEmpty(Message)) { throw new System.ArgumentException("The Message is null."); }
            Progr = Message;
            ConsoleInterop.WriteToConsole($"{Progr}: {iterator}/{end} [{Progm}]\r");
        }

        /// <summary>
        /// Indicates if the bar was executed. Use it to close the running thread.
        /// </summary>
        public System.Boolean Ended { get { return _Ended; } set { _Ended = value; } }

        /// <summary>
        /// Update the progress by the defined step.
        /// </summary>
        public void UpdateProgress() { if (_Ended == false) { Progm += Progc; ChangeProgress.Invoke(null, new ProgressChangedArgs(iterator += stp)); } }

        private void ChangeBar(System.Object sender, ProgressChangedArgs e) { ConsoleInterop.WriteToConsole($"{Progr}: {e.ChangedValueTo}/{end} [{Progm}] \r"); }

        /// <summary>
        /// The function which starts up the Console Bar. This should only be used in a new <see cref="System.Threading.ThreadStart"/> delegate.
        /// </summary>
        public void Invoke()
        {
            ChangeProgress += ChangeBar;
            iterator = start;
            ConsoleInterop.WriteToConsole($"{Progr}: {iterator}/{end} [{Progm}]\r");
            do
            {
                if (iterator >= end) { _Ended = true; }
                System.Threading.Thread.Sleep(80);
            } while (_Ended == false);
            ChangeProgress -= ChangeBar;
            ChangeProgress = null;
            ROOT.MAIN.WriteConsoleText("\nCompleted.");
            return;
        }
    }

    namespace IntuitiveInteraction
    {
#pragma warning disable CS1591
        /// <summary>
        /// An enumeration of <see cref="System.Int32" /> that hold valid icon images allowed be shown when the class 
        /// <see cref="IntuitiveMessageBox"/> is invoked.
        /// </summary>
        public enum IconSelection : System.Int32
        {
            None = 0,
            Error = 1,
            Info = 2,
            Info2 = 3,
            Warning = 4,
            Notice = 5,
            InvalidOperation = 6,
            Question = 7
        }

        /// <summary>
        /// An enumeration of <see cref="System.Int32" /> that keeps valid button patterns for returning the button selected.
        /// </summary>
        /// <remarks>This is used only with the class <see cref="IntuitiveMessageBox"/>.</remarks>
        public enum ButtonSelection : System.Int32
        {
            OK = 0,
            YesNo = 1,
            OKCancel = 2,
            AbortRetry = 3,
            RetryCancel = 4,
            IgnoreCancel = 5,
            YesNoCancel = 6,
            YesNoRetry = 7,
            YesCancelAbort = 8
        }

        /// <summary>
        /// An enumeration of <see cref="System.Int32" /> values which indicates which button pressed or presents an error.
        /// </summary>
        public enum ButtonReturned : System.Int32
        {
            Error = 0,
            OK = 1,
            Cancel = 2,
            Yes = 3,
            No = 4,
            Retry = 5,
            Abort = 6,
            Ignore = 7,
            NotAnAnswer = Error | Cancel
        }
#pragma warning restore CS1591

        /// <summary>
        /// A class that extends the default Microsoft.VisualBasic.Interaction.InputBox() method.
        /// </summary>
        [SupportedOSPlatform("windows")]
        public class GetAStringFromTheUser : System.IDisposable
        {
            private Form Menu = new();
            private Button Button1 = new Button();
            private Button Button2 = new Button();
            private Label Label1 = new Label();
            private TextBox TextBox1 = new TextBox();
            private System.String Prompt_msg;
            private System.String Title_msg;
            private System.String Default_msg;
            private ButtonReturned _RET = 0;
            private System.String Value;

            private event System.EventHandler HANDLE;

            /// <summary>
            /// Constructor Option 1: Define the settings at any time you would like.
            /// </summary>
            /// <remarks>Do not forget to invoke the dialog using the <see cref="Invoke"/> function.</remarks>
            public GetAStringFromTheUser() { }

            /// <summary>
            /// Constructor Option 2: Define the arguments required at once , run the dialog and then dispose it.
            /// </summary>
            /// <param name="Prompt">A message prompting the User what he should type inside the input box.</param>
            /// <param name="Title">The dialog's title.</param>
            /// <param name="DefaultResponse">The default response or an example of the data to be provided by the User.</param>
            [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
            public GetAStringFromTheUser(System.String Prompt, System.String Title, System.String DefaultResponse)
            {
                Prompt_msg = Prompt;
                Title_msg = Title;
                Default_msg = DefaultResponse;
                HANDLE += Button_click;
                Initiate();
                HANDLE -= Button_click;
                this.Dispose();
            }

            /// <summary>
            /// Disposes all the <see cref="System.Windows.Forms.Form"/> members used to make this dialog.
            /// </summary>
            public void Dispose()
            {
                if (HANDLE != null) { HANDLE -= Button_click; HANDLE = null; }
                TextBox1.Dispose();
                Label1.Dispose();
                Button1.Dispose();
                Button2.Dispose();
                Menu.Dispose();
            }

            /// <summary>
            /// A message prompting the User what he should type inside the input box.
            /// </summary>
            public System.String Prompt
            {
                set { Prompt_msg = value; }
            }

            /// <summary>
            /// The window's title.
            /// </summary>
            public System.String Title
            {
                set { Title_msg = value; }
            }

            /// <summary>
            /// Invokes the User Input Box. Use it when you have used the parameterless constructor.
            /// </summary>
            [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
            public void Invoke()
            {
                HANDLE += Button_click;
                Initiate();
                HANDLE -= Button_click;
                this.Dispose();
            }

            /// <summary>
            /// The default response or an example of the data to be provided by the User.
            /// </summary>
            public System.String DefaultResponse
            {
                set { Default_msg = value; }
            }

            /// <summary>
            /// Returns the button pressed. 
            ///  -> 0 indicates an system error or the user used the 'X' (Close Window) button.
            ///  -> 2 indicates that the User supplied an option and then he pressed the 'OK' button.
            ///  -> 4 indicates that the User did or not gave an answer , but he canceled the action.
            /// </summary>
            public ButtonReturned ButtonClicked
            {
                get { return _RET; }
            }

            /// <summary>
            /// Returns a <see cref="System.Boolean" /> value indicating that the User has supplied a value and pressed the 'OK' button.
            /// </summary>
            public System.Boolean Success
            {
                get { if (_RET == ButtonReturned.NotAnAnswer) { return false; } else { return true; } }
            }

            /// <summary>
            /// Returns the value given by the User. It's type is a <see cref="System.String"/>.
            /// </summary>
            public System.String ValueReturned
            {
                get { if (_RET != ButtonReturned.Error) { return Value; } else { return null; } }
            }

            private protected void Button_click(System.Object sender, System.EventArgs e)
            {
                Menu.Close();
                if (sender == Button1)
                {
                    Value = TextBox1.Text;
                    _RET = ButtonReturned.OK;
                }
                if (sender == Button2)
                {
                    _RET = ButtonReturned.Cancel;
                }
                return;
            }

            [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
            private protected void Initiate()
            {
                Label1.SuspendLayout();
                Button1.SuspendLayout();
                Button2.SuspendLayout();
                TextBox1.SuspendLayout();
                Label1.BorderStyle = BorderStyle.None;
                Label1.UseMnemonic = true;
                Label1.AutoSize = true;
                Label1.Text = Prompt_msg;
                Label1.Location = new System.Drawing.Point(14, 25);
                Label1.Size = new System.Drawing.Size(180, 110);
                Button1.Location = new Point(260, 22);
                Button1.Size = new Size(65, 24);
                Button1.Text = "OK";
                Button2.Location = new Point(Button1.Location.X, Button1.Location.Y + Button1.Height + 9);
                Button2.Size = Button1.Size;
                Button2.Text = "Cancel";
                Button1.Click += HANDLE;
                Button2.Click += HANDLE;
                System.Char[] FindNL_S = Label1.Text.ToCharArray();
                // The last value for vertical padding.
                System.Int32 CH = 0;
                for (System.Int32 DI = 0; DI < FindNL_S.Length; DI++)
                {
                    // The required padding for Microsoft Sans Serif is now 16?.
                    if (FindNL_S[DI] == '\n') { CH += 16; }
                }
                FindNL_S = null;
                TextBox1.Location = new Point(11, (Label1.Height + CH) - 17);
                TextBox1.Size = new Size(330, 14);
                TextBox1.Text = Default_msg;
                TextBox1.BorderStyle = BorderStyle.Fixed3D;
                TextBox1.ReadOnly = false;
                TextBox1.BackColor = Color.LightGray;
                TextBox1.Multiline = false;
                TextBox1.ResumeLayout();
                TextBox1.Invalidate();
                Label1.ResumeLayout();
                Button1.ResumeLayout();
                Button2.ResumeLayout();
                Menu.FormBorderStyle = FormBorderStyle.FixedDialog;
                Menu.StartPosition = FormStartPosition.CenterScreen;
                Menu.Text = Title_msg;
                Menu.MinimizeBox = false;
                Menu.MaximizeBox = false;
                Menu.TopMost = true;
                Menu.ControlBox = false;
                Menu.ShowInTaskbar = false;
                // All the redrawings are only valid for Microsoft Sans Serif font!!!
                Menu.Font = new Font("Microsoft Sans Serif", (System.Single)9.10, FontStyle.Regular, GraphicsUnit.Point);
                Menu.Size = new System.Drawing.Size(TextBox1.Location.X + TextBox1.Size.Width + 28, TextBox1.Location.Y + TextBox1.Size.Height + 42);
                Menu.Controls.Add(TextBox1);
                Menu.Controls.Add(Label1);
                Menu.Controls.Add(Button1);
                Menu.Controls.Add(Button2);
                Menu.ShowDialog();
            }
        }
        
        /// <summary>
        /// A class that extends the <see cref="System.Windows.Forms.MessageBox"/> class by adding it new features.
        /// </summary>
        /// <remarks>Do not expect that it will be as fast as the <see cref="MessageBox"/>; This is made on managed code. </remarks>
        [SupportedOSPlatform("windows")]
        public class IntuitiveMessageBox : System.IDisposable
        {
            private System.String _MSG;
            private System.Windows.Forms.Label Label1 = new();
            private System.Windows.Forms.Panel Panel1 = new();
            private System.Drawing.Image Image1 = null;
            private System.Windows.Forms.PictureBox PictureBox1 = new();
            private Form Menu = new Form();
            private System.Windows.Forms.Button Button1 = new();
            private System.Windows.Forms.Button Button2 = new();
            private System.Windows.Forms.Button Button3 = new();
            private System.String _TITLE;
            private ButtonReturned BTR;
            private ButtonSelection BSL;
            private IconSelection SELI;

            private event System.EventHandler ButtonHandle;

            /// <summary>
            /// Constructor Option 1:  Define all the arguments at once , run the dialog and dispose the class.
            /// </summary>
            /// <param name="Message">The text of the information to show to the user.</param>
            /// <param name="Title">The message box title to show.</param>
            /// <param name="Buttons">The buttons that will be shown to the User.</param>
            /// <param name="Ic">The Icon that will be shown to the user. Can also be <see cref="IconSelection.None"/>.</param>
            [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
            public IntuitiveMessageBox(System.String Message, System.String Title, ButtonSelection Buttons, IconSelection Ic)
            {
                ButtonHandle += Button_Click;
                _MSG = Message;
                _TITLE = Title;
                BSL = Buttons;
                SELI = Ic;
                MakeAndInitDialog(Buttons, Ic);
                this.Dispose();
                ButtonHandle -= Button_Click;
            }

            /// <summary>
            /// Constructor Option 2: Define the properties instead and run the dialog whenever you want.
            /// </summary>
            /// <remarks>Do not forget to invoke the window using the <see cref="InvokeInstance"/> function.</remarks>
            public IntuitiveMessageBox() { ButtonHandle += Button_Click; }

            /// <summary>
            /// Constructor Option 3: Define the Message and the Title properties , do any other settings you want 
            /// and run the dialog whenever you want.
            /// </summary>
            /// <param name="Message">The text of the information to show to the user.</param>
            /// <param name="Title">The message box title to show.</param>
            /// <remarks>Do not forget to invoke the window using the <see cref="InvokeInstance"/> function.</remarks>
            public IntuitiveMessageBox(System.String Message, System.String Title)
            {
                ButtonHandle += Button_Click;
                _MSG = Message;
                _TITLE = Title;
            }

            /// <summary>
            /// The Dispose function clears up the resources used by the Message Box and then invalidates the class.
            /// </summary>
            /// <remarks>This function also implements the <see cref="System.IDisposable"/> interface.</remarks>
            [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
                if (ButtonHandle != null) { ButtonHandle -= Button_Click; }
                Label1.Dispose();
                Panel1.Dispose();
                if (Image1 != null) { Image1.Dispose(); }
                PictureBox1.Dispose();
                Button1.Dispose();
                Button2.Dispose();
                Button3.Dispose();
                Menu.Dispose();
                System.GC.Collect(3, System.GCCollectionMode.Forced, true);
            }

            /// <summary>
            /// The text of the information to show to the user.
            /// </summary>
            public System.String Message
            {
                get { return _MSG; }
                set { _MSG = value; }
            }

            /// <summary>
            /// The buttons that will be shown to the User.
            /// </summary>
            public ButtonSelection ButtonsToShow { get { return BSL; } set { BSL = value; } }

            /// <summary>
            /// The Icon that will be shown to the user. Can also be <see cref="IconSelection.None"/>.
            /// </summary>
            public IconSelection IconToShow { get { return SELI; } set { SELI = value; } }

            /// <summary>
            /// The message box title to show.
            /// </summary>
            public System.String Title { get { return _TITLE; } set { _TITLE = value; } }

            /// <summary>
            /// Returns the button selected by the User. It's value is one of the <see cref="ButtonReturned"/> <see cref="System.Enum"/> values.
            /// </summary>
            public ButtonReturned ButtonSelected { get { return BTR; } }

            private protected void Button_Click(System.Object sender, System.EventArgs e)
            {
                Menu.Close();
                if (BSL == 0)
                {
                    if (sender == Button1) { BTR = (ButtonReturned)1; }
                }
                if (BSL == (ButtonSelection)1)
                {
                    if (sender == Button1) { BTR = (ButtonReturned)4; }
                    if (sender == Button2) { BTR = (ButtonReturned)3; }
                }
                if (BSL == (ButtonSelection)2)
                {
                    if (sender == Button1) { BTR = (ButtonReturned)2; }
                    if (sender == Button2) { BTR = (ButtonReturned)1; }
                }
                if (BSL == (ButtonSelection)3)
                {
                    if (sender == Button1) { BTR = (ButtonReturned)5; }
                    if (sender == Button2) { BTR = (ButtonReturned)6; }
                }
                if (BSL == (ButtonSelection)4)
                {
                    if (sender == Button1) { BTR = (ButtonReturned)5; }
                    if (sender == Button2) { BTR = (ButtonReturned)2; }
                }
                if (BSL == (ButtonSelection)5)
                {
                    if (sender == Button1) { BTR = (ButtonReturned)7; }
                    if (sender == Button2) { BTR = (ButtonReturned)2; }
                }
                if (BSL == (ButtonSelection)6)
                {
                    if (sender == Button1) { BTR = (ButtonReturned)2; }
                    if (sender == Button2) { BTR = (ButtonReturned)4; }
                    if (sender == Button3) { BTR = (ButtonReturned)3; }
                }
                if (BSL == (ButtonSelection)7)
                {
                    if (sender == Button1) { BTR = (ButtonReturned)5; }
                    if (sender == Button2) { BTR = (ButtonReturned)4; }
                    if (sender == Button3) { BTR = (ButtonReturned)3; }
                }
                if (BSL == (ButtonSelection)8)
                {
                    if (sender == Button1) { BTR = (ButtonReturned)6; }
                    if (sender == Button2) { BTR = (ButtonReturned)2; }
                    if (sender == Button3) { BTR = (ButtonReturned)3; }
                }
                return;
            }

            /// <summary>
            /// Invokes the Message Box based on the current settings done by the User.
            /// </summary>
            /// <remarks>Do not forget to dispose the class by using the <see cref="Dispose"/> function.</remarks>
            [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
            public void InvokeInstance()
            {
                MakeAndInitDialog(BSL, SELI);
                ButtonHandle -= Button_Click;
                this.Dispose();
            }

            [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
            private protected void MakeAndInitDialog(ButtonSelection Butt, IconSelection Icon)
            {
                // The below statements select the appropriate image to be shown each time.
                // Although that these are icons , 
                if (Icon == (IconSelection)1) { Image1 = MDCFR.Properties.Resources.Error.ToBitmap(); }
                if (Icon == (IconSelection)2) { Image1 = MDCFR.Properties.Resources.Information.ToBitmap(); }
                if (Icon == (IconSelection)3) { Image1 = MDCFR.Properties.Resources.Information2.ToBitmap(); }
                if (Icon == (IconSelection)4) { Image1 = MDCFR.Properties.Resources.Warning.ToBitmap(); }
                if (Icon == (IconSelection)5) { Image1 = MDCFR.Properties.Resources.Information.ToBitmap(); }
                if (Icon == (IconSelection)6) { Image1 = MDCFR.Properties.Resources.InvalidOperation.ToBitmap(); }
                if (Icon == (IconSelection)7) { Image1 = MDCFR.Properties.Resources.Question.ToBitmap(); }
                if (Icon != 0)
                {
                    PictureBox1.SuspendLayout();
                    PictureBox1.AutoSize = true;
                    PictureBox1.Size = new System.Drawing.Size(38, 38);
                    PictureBox1.Image = Image1;
                    PictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                    PictureBox1.Location = new System.Drawing.Point(26, 22);
                    PictureBox1.ResumeLayout();
                }
                PictureBox1.PerformLayout();
                Label1.SuspendLayout();
                Label1.BorderStyle = BorderStyle.None;
                Label1.UseMnemonic = true;
                Label1.AutoSize = true;
                Label1.Text = _MSG;
                Label1.AutoEllipsis = false;
                if (Icon != 0)
                {
                    Label1.Location = new System.Drawing.Point((PictureBox1.Width + PictureBox1.Location.X) + 25, PictureBox1.Location.Y);
                }
                else { Label1.Location = new System.Drawing.Point(26, 22); }
                // By default , the Size value is not updated when the Label is even resized , as set by AutoSize.
                // To fix that , we will get the label's text , we will convert it to an array of System.Char , and then
                // will check out if there are any newline characters. If there are , the location of the last label and buttons will be padded up
                // accordingly. Additionally , neither the Width value is updated.
                // To Implement that , it will use a check mechanism while checking for paddings and will enable it to also
                //  resize that value too. <--
                System.Char[] FindNL_S = Label1.Text.ToCharArray();
                // The last value for vertical padding.
                System.Int32 CH = 0;
                // The Last value for horizontal padding.
                System.Int32 CW = 0;
                // Check flag for horizontal padding.
                System.Boolean CWC = false;
                // A temporary value that will compare if the temporary one is larger than the other.
                System.Int32 CWCM = 0;
                for (System.Int32 DI = 0; DI < FindNL_S.Length; DI++)
                {
                    // The required padding for Microsoft Sans Serif is 15.
                    if (FindNL_S[DI] == '\n') { CH += 15; CWC = true; } else { CWCM += 5; }
                    if (CWC) { if (CWCM > CW) { CW = CWCM; CWCM = 0; CWC = false; } }
                    // Recheck once more time in the last iteration if the box must be redrawn.
                    if (DI == FindNL_S.Length) { if (CWCM > CW) { CW = CWCM; } }
                }
                // Test if we don't have any '\n' s.
                System.Boolean CWH = true;
                for (System.Int32 DI = 0; DI < FindNL_S.Length; DI++) { if (FindNL_S[DI] == '\n') { CWH = false; } }
                // This special case is executed when the above statement does not have detected any \n s.
                // The numbers are really approximate; these were random math until the wanted situation was performed.
                if (CWH) { CH = 14; CW = (FindNL_S.Length + 10) * 4; }
                CWH = false;
                CWCM = 0;
                FindNL_S = null;
                // -->
                Label1.ResumeLayout();
                Label1.Refresh();
                Panel1.SuspendLayout();
                Panel1.BorderStyle = BorderStyle.None;
                Panel1.AutoSize = true;
                Panel1.BackColor = System.Drawing.Color.Gray;
                Panel1.Location = new System.Drawing.Point(0, Label1.Location.Y + CH + 40);
                CH = 0;
                Panel1.ResumeLayout();
                Button1.SuspendLayout();
                Button2.SuspendLayout();
                Button3.SuspendLayout();
                Button1.Location = new System.Drawing.Point(Label1.Location.X + CW - 18, Panel1.Location.Y + 10);
                Button1.Size = new System.Drawing.Size(65, 20);
                Button2.Location = new System.Drawing.Point(Button1.Left - 75, Button1.Top);
                Button2.Size = Button1.Size;
                Button3.Location = new System.Drawing.Point(Button2.Left - 75, Button1.Top);
                Button3.Size = Button1.Size;
                Panel1.Size = new System.Drawing.Size(PictureBox1.Size.Width + Label1.Location.X + CW + 35, 60);
                // Selection workflow; These statements select which buttons are shown , and determine the dialog.
                if (Butt == 0)
                {
                    Button1.Text = "OK";
                    Button1.Visible = true;
                    Button2.Visible = false;
                    Button3.Visible = false;
                    Menu.Controls.Add(Button1);
                    Button1.Click += ButtonHandle;
                }
                if (Butt == (ButtonSelection)1)
                {
                    Button1.Text = "No";
                    Button2.Text = "Yes";
                    Button3.Visible = false;
                    Menu.Controls.Add(Button2);
                    Menu.Controls.Add(Button1);
                    Button1.Click += ButtonHandle;
                    Button2.Click += ButtonHandle;
                }
                if (Butt == (ButtonSelection)2)
                {
                    Button1.Text = "Cancel";
                    Button2.Text = "OK";
                    Button3.Visible = false;
                    Menu.Controls.Add(Button1);
                    Menu.Controls.Add(Button2);
                    Button1.Click += ButtonHandle;
                    Button2.Click += ButtonHandle;
                }
                if (Butt == (ButtonSelection)3)
                {
                    Button1.Text = "Retry";
                    Button2.Text = "Abort";
                    Button3.Visible = false;
                    Menu.Controls.Add(Button1);
                    Menu.Controls.Add(Button2);
                    Button1.Click += ButtonHandle;
                    Button2.Click += ButtonHandle;
                }
                if (Butt == (ButtonSelection)4)
                {
                    Button1.Text = "Retry";
                    Button2.Text = "Cancel";
                    Button3.Visible = false;
                    Menu.Controls.Add(Button1);
                    Menu.Controls.Add(Button2);
                    Button1.Click += ButtonHandle;
                    Button2.Click += ButtonHandle;
                }
                if (Butt == (ButtonSelection)5)
                {
                    Button1.Text = "Ignore";
                    Button2.Text = "Cancel";
                    Button3.Visible = false;
                    Menu.Controls.Add(Button1);
                    Menu.Controls.Add(Button2);
                    Button1.Click += ButtonHandle;
                    Button2.Click += ButtonHandle;
                }
                if (Butt == (ButtonSelection)6)
                {
                    Button1.Text = "Cancel";
                    Button2.Text = "No";
                    Button3.Text = "Yes";
                    Menu.Controls.Add(Button3);
                    Menu.Controls.Add(Button2);
                    Menu.Controls.Add(Button1);
                    Button1.Click += ButtonHandle;
                    Button2.Click += ButtonHandle;
                    Button3.Click += ButtonHandle;
                }
                if (Butt == (ButtonSelection)7)
                {
                    Button1.Text = "Retry";
                    Button2.Text = "No";
                    Button3.Text = "Yes";
                    Menu.Controls.Add(Button3);
                    Menu.Controls.Add(Button2);
                    Menu.Controls.Add(Button1);
                    Button1.Click += ButtonHandle;
                    Button2.Click += ButtonHandle;
                    Button3.Click += ButtonHandle;
                }
                if (Butt == (ButtonSelection)8)
                {
                    Button1.Text = "Abort";
                    Button2.Text = "Cancel";
                    Button3.Text = "Yes";
                    Menu.Controls.Add(Button3);
                    Menu.Controls.Add(Button2);
                    Menu.Controls.Add(Button1);
                    Button1.Click += ButtonHandle;
                    Button2.Click += ButtonHandle;
                    Button3.Click += ButtonHandle;
                }
                Button1.ResumeLayout();
                Button2.ResumeLayout();
                Button3.ResumeLayout();
                Menu.FormBorderStyle = FormBorderStyle.FixedDialog;
                Menu.StartPosition = FormStartPosition.CenterScreen;
                Menu.Text = _TITLE;
                Menu.MinimizeBox = false;
                Menu.MaximizeBox = false;
                Menu.TopMost = true;
                Menu.ControlBox = false;
                Menu.ShowInTaskbar = false;
                // All the redrawings are only valid for Microsoft Sans Serif font!!!
                Menu.Font = new Font("Microsoft Sans Serif", (System.Single)8.25, FontStyle.Regular, GraphicsUnit.Point);
                Menu.Size = new System.Drawing.Size(Label1.Location.X + CW + 85, Panel1.Location.Y + Panel1.Height + 20);
                Menu.Controls.Add(Label1);
                Menu.Controls.Add(Panel1);
                if (Icon != 0) { Menu.Controls.Add(PictureBox1); }
                if (Icon == (IconSelection)1) { System.Media.SystemSounds.Hand.Play(); }
                if (Icon == (IconSelection)4) { System.Media.SystemSounds.Asterisk.Play(); }
                if (Icon == (IconSelection)5) { System.Media.SystemSounds.Question.Play(); }
                if (Icon == (IconSelection)6) { System.Media.SystemSounds.Hand.Play(); }
                if (Icon == (IconSelection)7) { System.Media.SystemSounds.Question.Play(); }
                Menu.ShowDialog();
            }

        }
    }

    namespace RandomNumbers
    {

        /* 
          * Written in 2018 by David Blackman and Sebastiano Vigna (vigna@acm.org)
          *
          * To the extent possible under law, the author has dedicated all copyright
          * and related and neighboring rights to this software to the public domain
          * worldwide. This software is distributed without any warranty.
          *
          *	See <http://creativecommons.org/publicdomain/zero/1.0/>. 
          */

        /// <summary>
        /// <see cref="IRandomBase"/> is an interface which allows the random number generators
        /// to easily implement the random number generation mechanism.
        /// </summary>
        public interface IRandomBase : System.IDisposable
        {
            /// <summary>
            /// Inherited from the <see cref="System.IDisposable"/> interface.
            /// The random instance must dispose the seed and the state instances.
            /// </summary>
            public new abstract void Dispose();

            /// <summary>
            /// Produces a new random number. Expressed as <see cref="System.UInt64"/> code points.
            /// </summary>
            /// <returns>A new random number.</returns>
            public abstract System.UInt64 Next();

            /// <summary>
            /// Gets or sets the Seed for the instance. Can be private for that class
            /// and just implement a constructor which would allow to set an seed.
            /// </summary>
            public abstract System.Int32 Seed { get; set; }

            /// <summary>
            /// Gets a value whether the algorithm is optimized for 32-Bit machines.
            /// If <see langword="false"/> , it indicates that the random 
            /// algorithm is optimized for 64-Bit machines only.
            /// </summary>
            public abstract System.Boolean Is32Bit { get; }
        }

        /// <summary>
        /// The <see cref="IRandomHelpers"/> interface assists the programmer in taking values returned from generators
        /// in smaller numeric values.
        /// </summary>
        public interface IRandomHelpers
        {
            /// <summary>
            /// Converts the random value given to a equivalent <see cref="System.Int32"/> instance.
            /// </summary>
            /// <param name="value">The random value to convert.</param>
            /// <returns>A new <see cref="System.Int32"/> instance that represents the <paramref name="value"/> ,
            /// but as <see cref="System.Int32"/> code points.</returns>
            public System.Int32 ToInt32(System.UInt64 value);

            /// <summary>
            /// Converts the random value given to a equivalent <see cref="System.Int64"/> instance.
            /// </summary>
            /// <param name="value">The random value to convert.</param>
            /// <returns>A new <see cref="System.Int64"/> instance that represents the <paramref name="value"/> ,
            /// but as <see cref="System.Int64"/> code points.</returns>
            public System.Int64 ToInt64(System.UInt64 value);

            /// <summary>
            /// Converts the random value given to a equivalent <see cref="System.UInt16"/> instance.
            /// </summary>
            /// <param name="value">The random value to convert.</param>
            /// <returns>A new <see cref="System.UInt16"/> instance that represents the <paramref name="value"/> ,
            /// but as <see cref="System.UInt16"/> code points.</returns>
            public System.UInt16 ToUInt16(System.UInt64 value);

            /// <summary>
            /// Converts the random value given to a equivalent <see cref="System.Int16"/> instance.
            /// </summary>
            /// <param name="value">The random value to convert.</param>
            /// <returns>A new <see cref="System.UInt16"/> instance that represents the <paramref name="value"/> ,
            /// but as <see cref="System.UInt16"/> code points.</returns>
            public System.Int16 ToInt16(System.UInt64 value);

            /// <summary>
            /// Extracts a Boolean value from a random number.
            /// </summary>
            /// <param name="value">The random value to convert.</param>
            /// <returns>A new <see cref="System.Boolean"/> value.</returns>
            public System.Boolean ToBool(System.UInt64 value);

            /// <summary>
            /// Converts the random value given to a equivalent <see cref="System.Byte"/> instance.
            /// </summary>
            /// <param name="value">The random value to convert.</param>
            /// <returns>A new <see cref="System.Byte"/> instance that represents the <paramref name="value"/> ,
            /// but as <see cref="System.Byte"/> code points.</returns>
            public System.Byte ToByte(System.UInt64 value);

            /// <summary>
            /// Converts the random value given to a equivalent <see cref="System.Char"/> instance.
            /// </summary>
            /// <param name="value">The random value to convert.</param>
            /// <returns>A new <see cref="System.Char"/> instance that represents the <paramref name="value"/> ,
            /// but as <see cref="System.Char"/> code points.</returns>
            public System.Char ToChar(System.UInt64 value);

            /// <summary>
            /// <para>Converts the random value given to a <see cref="System.Single"/> value using a special formula. </para>
            /// <para>This formula guarantees that one value returned from 10 random iterations might produce 
            /// a number that is fully convertible to an integer. </para>
            /// </summary>
            /// <param name="value">The random value to convert.</param>
            /// <returns>The random <see cref="System.Single"/> number produced by the formula.</returns>
            public System.Single ToFloat(System.UInt64 value);

            /// <summary>
            /// <para>Converts the random value given to a <see cref="System.Double"/> value using a special formula. </para>
            /// <para>This formula guarantees that one value returned from 10 random iterations might produce 
            /// a number that is fully convertible to an integer. </para>
            /// </summary>
            /// <param name="value">The random value to convert.</param>
            /// <returns>The random <see cref="System.Double"/> number produced by the formula.</returns>
            public System.Double ToDouble(System.UInt64 value);
        }

        /// <summary>
        /// Direct inheritance from the <see cref="IRandomHelpers"/> interface.
        /// This class acts as it's default interface implementation.
        /// </summary>
        [AsInterface(typeof(IRandomHelpers))]
        public abstract class IRandomHelpersBase : IRandomHelpers
        {
            /// <summary>
            /// Converts the random value given to a equivalent <see cref="System.Int32"/> instance.
            /// </summary>
            /// <param name="value">The random value to convert.</param>
            /// <returns>A new <see cref="System.Int32"/> instance that represents the <paramref name="value"/> ,
            /// but as <see cref="System.Int32"/> code points.</returns>
            public virtual System.Int32 ToInt32(System.UInt64 value) { return (System.Int32)(value >> 48); }

            /// <summary>
            /// Converts the random value given to a equivalent <see cref="System.Int64"/> instance.
            /// </summary>
            /// <param name="value">The random value to convert.</param>
            /// <returns>A new <see cref="System.Int64"/> instance that represents the <paramref name="value"/> ,
            /// but as <see cref="System.Int64"/> code points.</returns>
            public virtual System.Int64 ToInt64(System.UInt64 value) { return (System.Int32)(value >> 32); }

            /// <summary>
            /// Converts the random value given to a equivalent <see cref="System.UInt16"/> instance.
            /// </summary>
            /// <param name="value">The random value to convert.</param>
            /// <returns>A new <see cref="System.UInt16"/> instance that represents the <paramref name="value"/> ,
            /// but as <see cref="System.UInt16"/> code points.</returns>
            public virtual System.UInt16 ToUInt16(System.UInt64 value) { return (System.UInt16)((value >> 32) >> 16); }

            /// <summary>
            /// Converts the random value given to a equivalent <see cref="System.Int16"/> instance.
            /// </summary>
            /// <param name="value">The random value to convert.</param>
            /// <returns>A new <see cref="System.UInt16"/> instance that represents the <paramref name="value"/> ,
            /// but as <see cref="System.UInt16"/> code points.</returns>
            public virtual System.Int16 ToInt16(System.UInt64 value) { return (System.Int16)((value >> 32) >> 6 >> 11); }

            /// <summary>
            /// Converts the random value given to a equivalent <see cref="System.Byte"/> instance.
            /// </summary>
            /// <param name="value">The random value to convert.</param>
            /// <returns>A new <see cref="System.Byte"/> instance that represents the <paramref name="value"/> ,
            /// but as <see cref="System.Byte"/> code points.</returns>
            public virtual System.Byte ToByte(System.UInt64 value) { return (System.Byte)((value >> 32) >> 8 >> 16); }

            /// <summary>
            /// Converts the random value given to a equivalent <see cref="System.Char"/> instance.
            /// </summary>
            /// <param name="value">The random value to convert.</param>
            /// <returns>A new <see cref="System.Char"/> instance that represents the <paramref name="value"/> ,
            /// but as <see cref="System.Char"/> code points.</returns>
            public virtual System.Char ToChar(System.UInt64 value) { return (System.Char)ToUInt16(value); }

            /// <summary>
            /// <para>Converts the random value given to a <see cref="System.Single"/> value using a special formula. </para>
            /// <para>This formula guarantees that one value returned from 10 random iterations might produce 
            /// a number that is fully convertible to an integer. </para>
            /// </summary>
            /// <param name="value">The random value to convert.</param>
            /// <returns>The random <see cref="System.Single"/> number produced by the formula.</returns>
            public virtual System.Single ToFloat(System.UInt64 value) { return (System.Single)(((value >> 32) >> 16) / 5.3 / 2.2); }

            /// <summary>
            /// <para>Converts the random value given to a <see cref="System.Double"/> value using a special formula. </para>
            /// <para>This formula guarantees that one value returned from 10 random iterations might produce 
            /// a number that is fully convertible to an integer. </para>
            /// </summary>
            /// <param name="value">The random value to convert.</param>
            /// <returns>The random <see cref="System.Double"/> number produced by the formula.</returns>
            public virtual System.Double ToDouble(System.UInt64 value) { return (System.Double)(((value >> 32) >> 16) / 5.3 / 2.2); }

            /// <summary>
            /// Extracts a Boolean value from a random number.
            /// </summary>
            /// <param name="value">The random value to convert.</param>
            /// <returns>A new <see cref="System.Boolean"/> value.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public virtual System.Boolean ToBool(System.UInt64 value) { return (System.UInt16.MaxValue / 2) >= (ToUInt16(value)); }
        }

        /// <summary>
        /// <para>
        /// This is xoroshiro128++ 1.0, one of our all-purpose, rock-solid,
        /// small-state generators. It is extremely (sub-ns) fast and it passes all
        /// tests we are aware of, but its state space is large enough only for
        /// mild parallelism.
        /// </para>
        /// <para>
        /// For generating just floating-point numbers, xoroshiro128+ is even
        /// faster(but it has a very mild bias, see notes in the comments).
        /// </para>
        /// </summary>
        public sealed class Xoroshiro128PP : IRandomHelpersBase, IRandomBase
        {
            private System.UInt64[] _STATE = new System.UInt64[2];
            private static System.UInt64[] JUMP = { 0x2bd7a6a6e99c2ddc, 0x0992ccaf6a6fca05 };
            private static System.UInt64[] LONG_JUMP = { 0x360fd5f2cf8d5d99, 0x9c6e6877736c46e3 };
            private System.UInt64 _SEED = 0;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private System.UInt64 rotl(System.UInt64 x, int k) { return (x << k) | (x >> (64 - k)); }

            /// <inheritdoc />
            public System.Boolean Is32Bit { get { return true; } }

            /// <summary>
            /// Create a new instance of <see cref="Xoroshiro128PP"/> with a default seed value of 0.
            /// </summary>
            public Xoroshiro128PP() { _STATE[0] = 2; _STATE[1] = 0; _STATE[1] -= 2; }

            /// <summary>
            /// Create a new instance of <see cref="Xoroshiro128PP"/> with the seed value specified.
            /// </summary>
            /// <param name="Seed">A Seed value which will be used for consumption in the generator.</param>
            public Xoroshiro128PP(System.Int32 Seed)
            {
                _SEED = (System.UInt64)Seed;
                if ((Seed / 2) != 0) { _STATE[0] = (System.UInt64)Seed / 2; } else { _STATE[0] = _SEED * 2; }
                _STATE[1] = (_STATE[0] + (System.UInt64)Seed) ^ 2;
            }

            /// <summary>
            /// Sets or gets the SEED number for this instance.
            /// </summary>
            public System.Int32 Seed
            {
                get { return (System.Int32)_SEED; }
                set
                {
                    _SEED = (System.UInt64)value;
                    if ((_SEED / 2) != 0) { _STATE[0] = _SEED / 2; } else { _STATE[0] = _SEED * 2; }
                    _STATE[1] = (_STATE[0] + _SEED) ^ 2;
                }
            }

            /// <summary>
            /// Disposes the <see cref="Xoroshiro128PP"/> instance.
            /// </summary>
            public void Dispose() { _STATE = null; JUMP = null; LONG_JUMP = null; }

            /// <summary>
            /// Produce a new <see cref="Xoroshiro128PP"/> random number.
            /// </summary>
            /// <returns>A new <see cref="Xoroshiro128PP"/> random number.</returns>
            public System.UInt64 Next()
            {
                System.UInt64 s0 = _STATE[0];
                System.UInt64 s1 = _STATE[1];
                System.UInt64 result = rotl(s0 + s1, 17) + s0;

                s1 ^= s0;
                _STATE[0] = rotl(s0, 49) ^ s1 ^ (s1 << 21); // a, b
                _STATE[1] = rotl(s1, 28); // c

                return result;
            }

            /// <summary>
            /// This is the jump function for the generator. It is equivalent
            /// to 2^64 calls to Next(); it can be used to generate 2^64
            /// non-overlapping subsequences for parallel computations.
            /// </summary>
            public void Jump()
            {
                System.UInt64 s0 = 0;
                System.UInt64 s1 = 0;
                for (int i = 0; i < JUMP.Length; i++)
                {
                    for (int b = 0; b < 64; b++)
                    {
                        if ((JUMP[i] & (System.UInt64)(1 << b)) != 0)
                        {
                            s0 ^= _STATE[0];
                            s1 ^= _STATE[1];
                        }
                        Next();
                    }
                }

                _STATE[0] = s0;
                _STATE[1] = s1;
            }

            /// <summary>
            /// This is the long-jump function for the generator. It is equivalent to
            /// 2^96 calls to <see cref="Next"/>; it can be used to generate 2^32 starting points,
            /// from each of which <see cref="Jump"/> will generate 2^32 non-overlapping
            /// subsequences for parallel distributed computations.
            /// </summary>
            public void LongJump()
            {
                System.UInt64 s0 = 0;
                System.UInt64 s1 = 0;
                for (int i = 0; i < LONG_JUMP.Length; i++)
                {
                    for (int b = 0; b < 64; b++)
                    {
                        if ((LONG_JUMP[i] & (System.UInt64)(1 << b)) != 0)
                        {
                            s0 ^= _STATE[0];
                            s1 ^= _STATE[1];
                        }
                        Next();
                    }
                }
                _STATE[0] = s0;
                _STATE[1] = s1;
            }

            /// <inheritdoc />
            public override ushort ToUInt16(ulong value) { return (System.UInt16)(value >> 48); }

            /// <inheritdoc />
            public override int ToInt32(ulong value) { return (System.Int32)(value >> 32 >> 1); }

            /// <inheritdoc />
            public override long ToInt64(ulong value) { return (System.Int64)(value >> 4); }
        }

        /// <summary>
        /// <para>
        /// This is xoshiro128+ 1.0, our best and fastest 32-bit generator for 32-bit
        /// floating-point numbers.We suggest to use its upper bits for
        /// floating-point generation, as it is slightly faster than xoshiro128**.
        /// It passes all tests we are aware of except for
        /// linearity tests, as the lowest four bits have low linear complexity, so
        /// if low linear complexity is not considered an issue (as it is usually
        /// the case) it can be used to generate 32-bit outputs, too.
        /// </para>
        /// <para>
        /// We suggest to use a sign test to extract a random Boolean value, and right shifts to extract subsets of bits.
        /// </para>
        /// </summary>
        public sealed class Xoshiro128P : IRandomHelpersBase, IRandomBase
        {
            private static System.UInt32[] JUMP = { 0x8764000b, 0xf542d2d3, 0x6fa035c3, 0x77f2db5b };
            private static System.UInt32[] LONG_JUMP = { 0xb523952e, 0x0b6f099f, 0xccf5a0ef, 0x1c580662 };
            [MethodImpl(MethodImplOptions.AggressiveInlining)] private System.UInt32 rotl(System.UInt32 x, int k) { return (x << k) | (x >> (32 - k)); }
            private static System.UInt32[] STATE = new System.UInt32[4];
            private System.UInt32 _SEED = 0;

            /// <summary>
            /// Create a new instance of <see cref="Xoshiro128P"/> with a default seed value of 0.
            /// </summary>
            public Xoshiro128P() { STATE[0] = 2; STATE[1] = 0; STATE[1] -= 2; }

            /// <summary>
            /// Create a new instance of <see cref="Xoshiro128P"/> with the seed value specified.
            /// </summary>
            /// <param name="Seed">A Seed value which will be used for consumption in the generator.</param>
            public Xoshiro128P(System.Int32 Seed)
            {
                _SEED = (System.UInt32)Seed;
                if ((Seed / 2) != 0) { STATE[0] = (System.UInt32)Seed / 2; } else { STATE[0] = _SEED * 2; }
                STATE[1] = (STATE[0] + (System.UInt32)Seed) ^ 2;
            }

            /// <summary>
            /// Sets or gets the SEED number for this instance.
            /// </summary>
            public System.Int32 Seed
            {
                get { return (System.Int32)_SEED; }
                set
                {
                    _SEED = (System.UInt32)value;
                    if ((_SEED / 2) != 0) { STATE[0] = _SEED / 2; } else { STATE[0] = _SEED * 2; }
                    STATE[1] = (STATE[0] + _SEED) ^ 2;
                }
            }

            /// <summary>
            /// Produce a new <see cref="Xoshiro128P"/> random number.
            /// </summary>
            /// <returns>A new <see cref="Xoshiro128P"/> random number.</returns>
            public System.UInt64 Next()
            {
                System.UInt32 result = STATE[0] + STATE[3];
                System.UInt32 t = STATE[1] << 9;
                STATE[2] ^= STATE[0];
                STATE[3] ^= STATE[1];
                STATE[1] ^= STATE[2];
                STATE[0] ^= STATE[3];
                STATE[2] ^= t;
                STATE[3] = rotl(STATE[3], 11);
                return result;
            }

            /// <summary>
            /// This is the jump function for the generator. It is equivalent
            /// to 2^64 calls to Next(); it can be used to generate 2^64
            /// non-overlapping subsequences for parallel computations.
            /// </summary>
            public void Jump()
            {
                System.UInt32 s0 = 0;
                System.UInt32 s1 = 0;
                System.UInt32 s2 = 0;
                System.UInt32 s3 = 0;
                for (int i = 0; i < JUMP.Length; i++)
                {
                    for (int b = 0; b < 32; b++)
                    {
                        if ((JUMP[i] & 1 << b) != 0)
                        {
                            s0 ^= STATE[0];
                            s1 ^= STATE[1];
                            s2 ^= STATE[2];
                            s3 ^= STATE[3];
                        }
                        Next();
                    }
                }
                STATE[0] = s0;
                STATE[1] = s1;
                STATE[2] = s2;
                STATE[3] = s3;
            }

            /// <inheritdoc />
            public System.Boolean Is32Bit { get { return true; } }

            /// <summary>
            /// This is the long-jump function for the generator. It is equivalent to
            /// 2^96 calls to Next(); it can be used to generate 2^32 starting points,
            /// from each of which Jump() will generate 2^32 non-overlapping
            /// subsequences for parallel distributed computations.
            /// </summary>
            public void Long_Jump()
            {
                System.UInt32 s0 = 0;
                System.UInt32 s1 = 0;
                System.UInt32 s2 = 0;
                System.UInt32 s3 = 0;
                for (int i = 0; i < LONG_JUMP.Length; i++)
                {
                    for (int b = 0; b < 32; b++)
                    {
                        if ((LONG_JUMP[i] & 1 << b) != 0)
                        {
                            s0 ^= STATE[0];
                            s1 ^= STATE[1];
                            s2 ^= STATE[2];
                            s3 ^= STATE[3];
                        }
                        Next();
                    }
                }
                STATE[0] = s0;
                STATE[1] = s1;
                STATE[2] = s2;
                STATE[3] = s3;
            }

            /// <summary>
            /// Disposes the <see cref="Xoshiro128P"/> instance.
            /// </summary>
            public void Dispose() { STATE = null; JUMP = null; LONG_JUMP = null; }

            /// <inheritdoc />
            public override double ToDouble(ulong value) { return (System.Double)(((value >> 16) >> 8) / 5.3 / 2.2); }

            /// <inheritdoc />
            public override float ToFloat(ulong value) { return (System.Single)(((value >> 16) >> 8) / 5.3 / 2.2); }

            /// <inheritdoc />
            public override short ToInt16(ulong value) { return (System.Int16)(value >> 17); }
        }

        /// <summary>
        /// <para>
        /// This is xoroshiro128** 1.0, one of our all-purpose, rock-solid,
        /// small-state generators.It is extremely (sub-ns) fast and it passes all
        /// tests we are aware of, but its state space is large enough only for
        /// mild parallelism.
        /// </para>
        /// <para>
        /// For generating just floating-point numbers, xoroshiro128+ is even
        /// faster(but it has a very mild bias, see notes in the comments).
        /// </para>
        /// </summary>
        public sealed class Xoroshiro128SS : IRandomHelpersBase, IRandomBase
        {
            private System.UInt64[] STATE = new System.UInt64[2];
            private static System.UInt64[] JUMP = { 0xdf900294d8f554a5, 0x170865df4b3201fc };
            private static System.UInt64[] LONG_JUMP = { 0xd2a98b26625eee7b, 0xdddf9b1090aa7ac1 };
            private System.UInt64 _SEED = 0;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private System.UInt64 rotl(System.UInt64 x, int k) { return (x << k) | (x >> (64 - k)); }

            /// <summary>
            /// Create a new instance of <see cref="Xoroshiro128SS"/> with a default seed value of 0.
            /// </summary>
            public Xoroshiro128SS() { STATE[0] = 2; STATE[1] = 0; STATE[1] -= 2; }

            /// <summary>
            /// Create a new instance of <see cref="Xoroshiro128SS"/> with the seed value specified.
            /// </summary>
            /// <param name="Seed">A Seed value which will be used for consumption in the generator.</param>
            public Xoroshiro128SS(System.Int32 Seed)
            {
                _SEED = (System.UInt32)Seed;
                if ((Seed / 2) != 0) { STATE[0] = (System.UInt32)Seed / 2; } else { STATE[0] = _SEED * 2; }
                STATE[1] = (STATE[0] + (System.UInt32)Seed) ^ 2;
            }

            /// <inheritdoc />
            public System.Boolean Is32Bit { get { return true; } }

            /// <summary>
            /// Sets or gets the SEED number for this instance.
            /// </summary>
            public System.Int32 Seed
            {
                get { return (System.Int32)_SEED; }
                set
                {
                    _SEED = (System.UInt64)value;
                    if ((_SEED / 2) != 0) { STATE[0] = _SEED / 2; } else { STATE[0] = _SEED * 2; }
                    STATE[1] = (STATE[0] + _SEED) ^ 2;
                }
            }

            /// <summary>
            /// Produce a new <see cref="Xoroshiro128SS"/> random number.
            /// </summary>
            /// <returns>A new <see cref="Xoroshiro128SS"/> random number.</returns>
            public System.UInt64 Next()
            {
                System.UInt64 s0 = STATE[0];
                System.UInt64 s1 = STATE[1];
                System.UInt64 result = rotl(s0 * 5, 7) * 9;

                s1 ^= s0;
                STATE[0] = rotl(s0, 24) ^ s1 ^ (s1 << 16); // a, b
                STATE[1] = rotl(s1, 37); // c

                return result;
            }

            /// <summary>
            /// This is the jump function for the generator. It is equivalent
            /// to 2^64 calls to Next(); it can be used to generate 2^64
            /// non-overlapping subsequences for parallel computations.
            /// </summary>
            public void Jump()
            {
                System.UInt64 s0 = 0;
                System.UInt64 s1 = 0;
                for (int i = 0; i < JUMP.Length; i++)
                {
                    for (int b = 0; b < 64; b++)
                    {
                        if ((JUMP[i] & (System.UInt64)(1 << b)) != 0)
                        {
                            s0 ^= STATE[0];
                            s1 ^= STATE[1];
                        }
                        Next();
                    }
                }
                STATE[0] = s0;
                STATE[1] = s1;
            }

            /// <summary>
            /// This is the long-jump function for the generator. It is equivalent to
            /// 2^96 calls to Next(); it can be used to generate 2^32 starting points,
            /// from each of which Jump() will generate 2^32 non-overlapping
            /// subsequences for parallel distributed computations
            /// </summary>
            public void Long_Jump()
            {
                System.UInt64 s0 = 0;
                System.UInt64 s1 = 0;
                for (int i = 0; i < LONG_JUMP.Length; i++)
                {
                    for (int b = 0; b < 64; b++)
                    {
                        if ((LONG_JUMP[i] & (System.UInt64)(1 << b)) != 0)
                        {
                            s0 ^= STATE[0];
                            s1 ^= STATE[1];
                        }
                        Next();
                    }
                }
                STATE[0] = s0;
                STATE[1] = s1;
            }

            /// <summary>
            /// Disposes the <see cref="Xoroshiro128SS"/> instance.
            /// </summary>
            public void Dispose() { STATE = null; JUMP = null; LONG_JUMP = null; }

            /// <inheritdoc />
            public override System.UInt16 ToUInt16(System.UInt64 value) { return (System.UInt16)(value >> 48); }

            /// <inheritdoc />
            public override System.Int32 ToInt32(System.UInt64 value) { return (System.Int32)(value >> 33); }

            /// <inheritdoc />
            public override System.Int64 ToInt64(System.UInt64 value) { return (System.Int64)(value >> 9); }
        }

        /// <summary>
        /// <para>
        /// This is xoroshiro64* 1.0, our best and fastest 32-bit small-state
        /// generator for 32-bit floating-point numbers.We suggest to use its
        /// upper bits for floating-point generation, as it is slightly faster than
        /// xoroshiro64**. It passes all tests we are aware of except for linearity
        /// tests, as the lowest six bits have low linear complexity, so if low
        /// linear complexity is not considered an issue (as it is usually the
        /// case) it can be used to generate 32-bit outputs, too.
        /// </para>
        /// <para>
        /// We suggest to use a sign test to extract a random Boolean value, and right shifts to extract subsets of bits.
        /// </para>
        /// </summary>
        public sealed class Xoroshiro64S : IRandomHelpersBase, IRandomBase
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private System.UInt32 rotl(System.UInt32 x, int k) { return (x << k) | (x >> (32 - k)); }
            private static System.UInt32[] STATE = new System.UInt32[2];
            private System.UInt32 _SEED = 0;

            /// <summary>
            /// Create a new instance of <see cref="Xoroshiro64S"/> with a default seed value of 0.
            /// </summary>
            public Xoroshiro64S() { STATE[0] = 2; STATE[1] = 0; STATE[1] -= 2; }

            /// <summary>
            /// Produce a new <see cref="Xoroshiro64S"/> random number.
            /// </summary>
            /// <returns>A new <see cref="Xoroshiro64S"/> random number.</returns>
            public System.UInt64 Next()
            {
                System.UInt32 s0 = STATE[0];
                System.UInt32 s1 = STATE[1];
                System.UInt32 result = s0 * 0x9E3779BB;

                s1 ^= s0;
                STATE[0] = rotl(s0, 26) ^ s1 ^ (s1 << 9); // a, b
                STATE[1] = rotl(s1, 13); // c

                return result;
            }

            /// <summary>
            /// Sets or gets the SEED number for this instance.
            /// </summary>
            public System.Int32 Seed
            {
                get { return (System.Int32)_SEED; }
                set
                {
                    _SEED = (System.UInt32)value;
                    if ((_SEED / 2) != 0) { STATE[0] = _SEED / 2; } else { STATE[0] = _SEED * 2; }
                    STATE[1] = (STATE[0] + _SEED) ^ 2;
                }
            }

            /// <summary>
            /// Create a new instance of <see cref="Xoroshiro64S"/> with the seed value specified.
            /// </summary>
            /// <param name="Seed">A Seed value which will be used for consumption in the generator.</param>
            public Xoroshiro64S(System.Int32 Seed)
            {
                _SEED = (System.UInt32)Seed;
                if ((Seed / 2) != 0) { STATE[0] = (System.UInt32)Seed / 2; } else { STATE[0] = _SEED * 2; }
                STATE[1] = (STATE[0] + (System.UInt32)Seed) ^ 2;
            }

            /// <summary>
            /// Disposes the <see cref="Xoroshiro64S"/> instance.
            /// </summary>
            public void Dispose() { STATE = null; _SEED = 0; }

            /// <inheritdoc />
            public System.Boolean Is32Bit { get { return true; } }

            /// <inheritdoc />
            public override System.Int32 ToInt32(System.UInt64 Num) { return (System.Int32)(Num >> 2 >> 2); }

            /// <inheritdoc />
            public override System.Int16 ToInt16(System.UInt64 Num) { return (System.Int16)(Num >> 6 >> 11); }

            /// <inheritdoc />
            public override System.UInt16 ToUInt16(System.UInt64 Num) { return (System.UInt16)(Num >> 6 >> 9); }

            /// <inheritdoc />
            public override System.Int64 ToInt64(System.UInt64 Num) { return (System.Int64)Num; }

            /// <inheritdoc />
            public override System.Byte ToByte(System.UInt64 Num) { return (System.Byte)(Num >> 24); }
        }

        /// <summary>
        /// <para>
        /// This is xoroshiro64** 1.0, our 32-bit all-purpose, rock-solid,
        /// small-state generator.It is extremely fast and it passes all tests we
        /// are aware of, but its state space is not large enough for any parallel
        /// application.
        /// </para>
        /// <para>
        /// For generating just single-precision (i.e., 32-bit) floating-point numbers, xoroshiro64* is even faster.
        /// </para>
        /// </summary>
        public sealed class Xoroshiro64SS : IRandomBase
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)] private System.UInt32 rotl(System.UInt32 x, int k) { return (x << k) | (x >> (32 - k)); }
            private static System.UInt32[] STATE = new System.UInt32[2];
            private System.UInt32 _SEED = 0;

            /// <summary>
            /// Create a new instance of <see cref="Xoroshiro64SS"/> with a default seed value of 0.
            /// </summary>
            public Xoroshiro64SS() { STATE[0] = 2; STATE[1] = 0; STATE[1] -= 2; }

            /// <summary>
            /// Create a new instance of <see cref="Xoroshiro64SS"/> with the seed value specified.
            /// </summary>
            /// <param name="Seed">A Seed value which will be used for consumption in the generator.</param>
            public Xoroshiro64SS(System.Int32 Seed)
            {
                _SEED = (System.UInt32)Seed;
                if ((Seed / 2) != 0) { STATE[0] = (System.UInt32)Seed / 2; } else { STATE[0] = _SEED * 2; }
                STATE[1] = (STATE[0] + (System.UInt32)Seed) ^ 2;
            }

            /// <summary>
            /// Produce a new <see cref="Xoroshiro64SS"/> random number.
            /// </summary>
            /// <returns>A new <see cref="Xoroshiro64SS"/> random number.</returns>
            public System.UInt64 Next()
            {
                System.UInt32 s0 = STATE[0];
                System.UInt32 s1 = STATE[1];
                System.UInt32 result = rotl(s0 * 0x9E3779BB, 5) * 5;

                s1 ^= s0;
                STATE[0] = rotl(s0, 26) ^ s1 ^ (s1 << 9); // a, b
                STATE[1] = rotl(s1, 13); // c

                return result;
            }

            /// <summary>
            /// Sets or gets the SEED number for this instance.
            /// </summary>
            public System.Int32 Seed
            {
                get { return (System.Int32)_SEED; }
                set
                {
                    _SEED = (System.UInt32)value;
                    if ((_SEED / 2) != 0) { STATE[0] = _SEED / 2; } else { STATE[0] = _SEED * 2; }
                    STATE[1] = (STATE[0] + _SEED) ^ 2;
                }
            }

            /// <summary>
            /// Disposes the <see cref="Xoroshiro64SS"/> instance.
            /// </summary>
            public void Dispose() { STATE = null; _SEED = 0; }

            /// <inheritdoc />
            public System.Boolean Is32Bit { get { return true; } }
        }

        /// <summary>
        /// General purpose class to extract random values smaller than an <see cref="System.UInt64"/> number
        /// returned from the classes that implement the <see cref="IRandomBase"/> interface.
        /// </summary>
        internal static class RandomHelpers
        {

            /// <summary>
            /// Converts the random value given to a equivalent <see cref="System.Int32"/> instance.
            /// </summary>
            /// <param name="value">The random value to convert.</param>
            /// <returns>A new <see cref="System.Int32"/> instance that represents the <paramref name="value"/> ,
            /// but as <see cref="System.Int32"/> code points.</returns>
            public static System.Int32 ToInt32(System.UInt64 value) { return (System.Int32)(value >> 48); }

            /// <summary>
            /// Converts the random value given to a equivalent <see cref="System.Int64"/> instance.
            /// </summary>
            /// <param name="value">The random value to convert.</param>
            /// <returns>A new <see cref="System.Int64"/> instance that represents the <paramref name="value"/> ,
            /// but as <see cref="System.Int64"/> code points.</returns>
            public static System.Int64 ToInt64(System.UInt64 value) { return (System.Int32)(value >> 32); }

            /// <summary>
            /// Converts the random value given to a equivalent <see cref="System.UInt16"/> instance.
            /// </summary>
            /// <param name="value">The random value to convert.</param>
            /// <returns>A new <see cref="System.UInt16"/> instance that represents the <paramref name="value"/> ,
            /// but as <see cref="System.UInt16"/> code points.</returns>
            public static System.UInt16 ToUInt16(System.UInt64 value) { return (System.UInt16)((value >> 32) >> 16); }

            /// <summary>
            /// Converts the random value given to a equivalent <see cref="System.Int16"/> instance.
            /// </summary>
            /// <param name="value">The random value to convert.</param>
            /// <returns>A new <see cref="System.UInt16"/> instance that represents the <paramref name="value"/> ,
            /// but as <see cref="System.UInt16"/> code points.</returns>
            public static System.Int16 ToInt16(System.UInt64 value) { return (System.Int16)((value >> 32) >> 6 >> 11); }

            /// <summary>
            /// Converts the random value given to a equivalent <see cref="System.Byte"/> instance.
            /// </summary>
            /// <param name="value">The random value to convert.</param>
            /// <returns>A new <see cref="System.Byte"/> instance that represents the <paramref name="value"/> ,
            /// but as <see cref="System.Byte"/> code points.</returns>
            public static System.Byte ToByte(System.UInt64 value) { return (System.Byte)((value >> 32) >> 8 >> 16); }

            /// <summary>
            /// Converts the random value given to a equivalent <see cref="System.Char"/> instance.
            /// </summary>
            /// <param name="value">The random value to convert.</param>
            /// <returns>A new <see cref="System.Char"/> instance that represents the <paramref name="value"/> ,
            /// but as <see cref="System.Char"/> code points.</returns>
            public static System.Char ToChar(System.UInt64 value) { return (System.Char)ToUInt16(value); }

            /// <summary>
            /// <para>Converts the random value given to a <see cref="System.Single"/> value using a special formula. </para>
            /// <para>This formula guarantees that one value returned from 10 random iterations might produce 
            /// a number that is fully convertible to an integer. </para>
            /// </summary>
            /// <param name="value">The random value to convert.</param>
            /// <returns>The random <see cref="System.Single"/> number produced by the formula.</returns>
            public static System.Single ToFloat(System.UInt64 value) { return (System.Single)((value >> 32) >> 16 / 5 / 4); }

            /// <summary>
            /// <para>Converts the random value given to a <see cref="System.Double"/> value using a special formula. </para>
            /// <para>This formula guarantees that one value returned from 10 random iterations might produce 
            /// a number that is fully convertible to an integer. </para>
            /// </summary>
            /// <param name="value">The random value to convert.</param>
            /// <returns>The random <see cref="System.Double"/> number produced by the formula.</returns>
            public static System.Double ToDouble(System.UInt64 value) { return (System.Double)((value >> 32) >> 16 / 5 / 4); }

        }

    }

    /// <summary>
    /// This <see langword="struct" /> bears a resemblance to the <see cref="System.Collections.ArrayList"/> , 
    /// but this extends it's functionality and it's aim is to work only with <see cref="System.Byte"/> data.
    /// It is useful also for interplaying between an array and a generic <see cref="IList{T}"/>
    /// collection , which it combines the advantages that both provide.
    /// </summary>
    /// <remarks>
    /// <para> This <see langword="struct" /> implements the <see cref="IList{T}"/> interface. </para>
    /// <para> Note: &lt;T&gt; is <see cref="System.Byte"/>. </para>
    /// </remarks>
    public struct ModifidableBuffer : System.Collections.Generic.IList<System.Byte>
    {
        private readonly System.Collections.Specialized.OrderedDictionary _dict = new();
        private System.Int32 Iter = 0;

        /// <summary>
        /// Initialise a new modifidable buffer with no data in it.
        /// </summary>
        public ModifidableBuffer() { }

        /// <summary>
        /// Initialise a new modifidable buffer and populate it with data taken 
        /// from a instantiated <see cref="System.Byte"/>[] array. 
        /// </summary>
        /// <param name="Value">The <see cref="System.Byte"/>[] data.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ModifidableBuffer(System.Byte[] Value) { for (System.Int32 O = 0; O < Value.Length; O++) { Add(Value[O]); } }

        /// <summary>
        /// Initialise a new modifidable buffer and populate it with data taken 
        /// from a instantiated <see cref="System.Byte"/>[] array. 
        /// </summary>
        /// <param name="Value">The <see cref="System.Byte"/>[] data.</param>
        /// <param name="Index">The index that this instance will start 
        /// saving data from <paramref name="Value"/> parameter.</param>
        /// <param name="Count">How many elements to 
        /// copy from the <paramref name="Value"/> array.</param>
        /// <exception cref="InvalidOperationException">Index parameter is not allowed to be more than Count parameter.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ModifidableBuffer(System.Byte[] Value, System.Int32 Index, System.Int32 Count)
        {
            if (Index >= Count) { throw new InvalidOperationException("Index parameter is not allowed to be more than Count parameter."); }
            for (System.Int32 O = Index; O < Count; O++) { Add(Value[O]); }
        }

        /// <inheritdoc />
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public System.Int32 IndexOf(System.Byte Value)
        {
            foreach (System.Collections.Generic.KeyValuePair<System.Int32, System.Byte> DE in _dict)
            {
                if (DE.Value == Value) { return DE.Key; }
            }
            return -1;
        }

        /// <inheritdoc />
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public void Insert(System.Int32 Index, System.Byte Value)
        {
            try
            {
                System.Boolean Flag = false;
                foreach (System.Collections.Generic.KeyValuePair<System.Int32, System.Byte> DE in _dict)
                {
                    if (DE.Key == Index) { Flag = true; }
                }
                if (Flag) { _dict[Index] = Value; } else { Add(Value); }
            }
            catch (System.Exception EX)
            {
                throw new System.AggregateException("Could not add the specified value to the dictionary.", EX);
            }
        }

        /// <inheritdoc />
        /// <remarks>Be careful when removing entries , this will make the whole data
        /// array to shift by one and cover up the blank entry. Using this method
        /// can result to data corruption.</remarks>
        public void RemoveAt(System.Int32 Index) { _dict.Remove(Index); Iter--; }

        /// <summary>
        /// Adds a new entry to this instance.
        /// </summary>
        /// <param name="Value">The <see cref="System.Byte"/> value to add to the newly created entry.</param>
        /// <exception cref="System.AggregateException">Thrown when the adding was failed for a reason.</exception>
        public void Add(System.Byte Value)
        {
            try
            {
                _dict.Add(Iter, Value);
                Iter++;
            }
            catch (System.Exception EX)
            {
                throw new System.AggregateException($"Could not add the specified value to the dictionary.", EX);
            }
        }

        /// <inheritdoc />
        public System.Byte this[System.Int32 Index]
        {
            get { return (System.Byte)_dict[Index]; }
            set { Insert(Index, value); }
        }

        /// <summary>
        /// Adds empty entries specified by the <paramref name="Times"/> <see cref="System.Int32"/> .
        /// </summary>
        /// <param name="Times">The number of empty entries to add.</param>
        [System.Runtime.ConstrainedExecution.ReliabilityContract(
            System.Runtime.ConstrainedExecution.Consistency.MayCorruptAppDomain,
            System.Runtime.ConstrainedExecution.Cer.MayFail)]
        public void AddEntries(System.Int32 Times) { for (System.Int32 I = 0; I < Times; I++) { _dict.Add(Iter++, 0); } }

        /// <inheritdoc />
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(System.Byte[] Array, System.Int32 Index)
        {
            System.Int32 tmp = 0;
            for (System.Int32 I = Index; I < Iter; I++) { Array[tmp] = (System.Byte)_dict[I]; tmp++; }
        }

        /// <summary>
        /// The <see cref="ToArray()"/> method gets all the data representing the current buffer , and returns them
        /// as a one-dimensional and fixed <see cref="System.Byte"/>[] array.
        /// </summary>
        /// <returns>The data which this <see langword="struct"/> holds.</returns>
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public System.Byte[] ToArray()
        {
            System.Byte[] bytes = new System.Byte[Iter];
            CopyTo(bytes, 0);
            return bytes;
        }

        /// <inheritdoc />
        public System.Int32 Count { get { return Iter; } }

        /// <inheritdoc />
        public void Clear() { _dict.Clear(); Iter = 0; }

        /// <inheritdoc />
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public System.Boolean Contains(System.Byte item)
        {
            foreach (System.Collections.Generic.KeyValuePair<System.Int32, System.Byte> DE in _dict)
            {
                if (DE.Value == item) { return true; }
            }
            return false;
        }

        /// <inheritdoc />
        /// <remarks>Note: This property always returns <c>false</c>.</remarks>
        public System.Boolean IsReadOnly { get { return false; } }

        /// <inheritdoc />
        /// <remarks>Be careful when removing entries , this will make the whole data
        /// array to shift by one and cover up the blank entry. Using this method
        /// can result to data corruption.</remarks>
        public System.Boolean Remove(System.Byte item)
        {
            if (IndexOf(item) == -1) { return false; }
            try
            {
                _dict.Remove(IndexOf(item));
                Iter--;
            }
            catch { return false; }
            return true;
        }

        IEnumerator<System.Byte> IEnumerable<System.Byte>.GetEnumerator()
        {
            System.Collections.Generic.IList<System.Byte> result = new System.Byte[_dict.Count];
            for (System.Int32 I = 0; I < _dict.Count; I++) { result[I] = (System.Byte)_dict[I]; }
            return result.GetEnumerator();
        }

        /// <inheritdoc />
        public System.Collections.IEnumerator GetEnumerator() { return _dict.GetEnumerator(); }

        /// <summary>
        /// Returns the byte data , but as an hexadecimal <see cref="System.String"/> , if it fits to one.
        /// Otherwise , the <see cref="System.String"/> representation of this type.
        /// </summary>
        /// <returns>The <see cref="System.Byte"/> data kept by this instance as a 
        /// <see cref="System.String"/> , otherwise 
        /// the <see cref="System.String"/> representation of this type.</returns>
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public override System.String ToString()
        {
            System.String Thestring = null;
            try
            {
                for (System.Int32 I = 0; I < Iter; I++)
                {
                    Thestring += this[I].ToString("x2");
                }
                return Thestring;
            }
            catch { return GetType().ToString(); }
        }

        /// <summary>
        /// Convert immediately from an intitialised <see cref="ModifidableBuffer"/> structure as an new 
        /// <see cref="System.Byte"/>[] array.
        /// </summary>
        /// <param name="instance">The <see cref="ModifidableBuffer"/> instance to take the data from.</param>
        public static explicit operator System.Byte[](ModifidableBuffer instance) { return instance.ToArray(); }

        /// <summary>
        /// Convert the <see cref="System.Byte"/>[] data as an new <see cref="ModifidableBuffer"/>.
        /// </summary>
        /// <param name="Data">The <see cref="System.Byte"/>[] to take the data from.</param>
        public static explicit operator ModifidableBuffer(System.Byte[] Data) { return new ModifidableBuffer(Data); }
    }

    internal struct STDConstants
    {
        // Standard Text Definition constants and helper functions.
        // The below characters define how the STD format will be read and written.

        public const System.Char COMMENT = '#';

        public const System.Char SEPERATOR = '$';

        public const System.Char HDROPEN = '{';

        public const System.Char HDRCLOSE = '}';

        public const System.Char STRING = '\"';

        public const System.Int32 VERSION = 1;

        // The encoding table. 
        // This table will be used so as to return and save the colors as one value 
        // to the dictionary.
        // Example: Array is at index 0.
        // The Index 0 returns two values: 
        // The first one is the foreground color.
        // The second one is the background color.
        // So , { 0 , 0 } means that the foreground and background colors will be black.
        // Note: Consult the STDFrontColor and STDBackColor enums for more information about this
        // generated table.
        public static readonly System.Int32[,] EncodeTable = { { 0 , 0 } , { 0 , 1 } , { 0 , 2 } ,
            { 0 , 3 } , { 0 , 4 } , { 0 , 5 } , { 0 , 6 } , { 0 , 7 } , { 0 , 8 } , { 0 , 9 } , { 0 , 10 } ,
            { 0 , 11 } , { 0 , 12 } , { 0 , 13 } , { 0 , 14 } , { 0 , 15 } , { 1 , 0 } , { 1 , 1 } , { 1 , 2 } ,
            { 1 , 3 } , { 1 , 4 } , { 1 , 5 } , { 1 , 6 } , { 1 , 7 } , { 1 , 8 } , { 1 , 9 } , { 1 , 10 } ,
            { 1 , 11 } , { 1 , 12 } , { 1 , 13 } , { 1 , 14 } , { 1 , 15 } , { 2 , 0 } , { 2 , 1 } , { 2 , 2 } ,
            { 2 , 3 } , { 2 , 4 } , { 2 , 5 } , { 2 , 6 } , { 2 , 7 } , { 2 , 8 } , { 2 , 9 } , { 2 , 10 } ,
            { 2 , 11 } , { 2 , 12 } , { 2 , 13 } , { 2 , 14 } , { 2 , 15 } , { 3 , 0 } , { 3 , 1 } , { 3 , 2 } ,
            { 3 , 3 } , { 3 , 4 } , { 3 , 5 } , { 3 , 6 } , { 3 , 7 } , { 3 , 8 } , { 3 , 9 } , { 3 , 10 } ,
            { 3 , 11 } , { 3 , 12 } , { 3 , 13 } , { 3 , 14 } , { 3 , 15 } , { 4 , 0 } , { 4 , 1 } , { 4 , 2 } ,
            { 4 , 3 } , { 4 , 4 } , { 4 , 5 } , { 4 , 6 } , { 4 , 7 } , { 4 , 8 } , { 4 , 9 } , { 4 , 10 } ,
            { 4 , 11 } , { 4 , 12 } , { 4 , 13 } , { 4 , 14 } , { 4 , 15 } , { 5 , 0 } , { 5 , 1 } , { 5 , 2 } ,
            { 5 , 3 } , { 5 , 4 } , { 5 , 5 } , { 5 , 6 } , { 5 , 7 } , { 5 , 8 } , { 5 , 9 } , { 5 , 10 } ,
            { 5 , 11 } , { 5 , 12 } , { 5 , 13 } , { 5 , 14 } , { 5 , 15 } , { 6 , 0 } , { 6 , 1 } , { 6 , 2 } ,
            { 6 , 3 } , { 6 , 4 } , { 6 , 5 } , { 6 , 6 } , { 6 , 7 } , { 6 , 8 } , { 6 , 9 } , { 6 , 10 } ,
            { 6 , 11 } , { 6 , 12 } , { 6 , 13 } , { 6 , 14 } , { 6 , 15 } , { 7 , 0 } , { 7 , 1 } , { 7 , 2 } ,
            { 7 , 3 } , { 7 , 4 } , { 7 , 5 } , { 7 , 6 } , { 7 , 7 } , { 7 , 8 } , { 7 , 9 } , { 7 , 10 } ,
            { 7 , 11 } , { 7 , 12 } , { 7 , 13 } , { 7 , 14 } , { 7 , 15 } , { 8 , 0 } , { 8 , 1 } , { 8 , 2 } ,
            { 8 , 3 } , { 8 , 4 } , { 8 , 5 } , { 8 , 6 } , { 8 , 7 } , { 8 , 8 } , { 8 , 9 } , { 8 , 10 } ,
            { 8 , 11 } , { 8 , 12 } , { 8 , 13 } , { 8 , 14 } , { 8 , 15 } , { 9 , 0 } , { 9 , 1 } , { 9 , 2 } ,
            { 9 , 3 } , { 9 , 4 } , { 9 , 5 } , { 9 , 6 } , { 9 , 7 } , { 9 , 8 } , { 9 , 9 } , { 9 , 10 } ,
            { 9 , 11 } , { 9 , 12 } , { 9 , 13 } , { 9 , 14 } , { 9 , 15 } , { 10 , 0 } , { 10 , 1 } , { 10 , 2 } ,
            { 10 , 3 } , { 10 , 4 } , { 10 , 5 } , { 10 , 6 } , { 10 , 7 } , { 10 , 8 } , { 10 , 9 } , { 10 , 10 } ,
            { 10 , 11 } , { 10 , 12 } , { 10 , 13 } , { 10 , 14 } , { 10 , 15 } , { 11 , 0 } , { 11 , 1 } , { 11 , 2 } ,
            { 11 , 3 } , { 11 , 4 } , { 11 , 5 } , { 11 , 6 } , { 11 , 7 } , { 11 , 8 } , { 11 , 9 } , { 11 , 10 } ,
            { 11 , 11 } , { 11 , 12 } , { 11 , 13 } , { 11 , 14 } , { 11 , 15 } , { 12 , 0 } , { 12 , 1 } , { 12 , 2 } ,
            { 12 , 3 } , { 12 , 4 } , { 12 , 5 } , { 12 , 6 } , { 12 , 7 } , { 12 , 8 } , { 12 , 9 } , { 12 , 10 } ,
            { 12 , 11 } , { 12 , 12 } , { 12 , 13 } , { 12 , 14 } , { 12 , 15 } , { 13 , 0 } , { 13 , 1 } , { 13 , 2 } ,
            { 13 , 3 } , { 13 , 4 } , { 13 , 5 } , { 13 , 6 } , { 13 , 7 } , { 13 , 8 } , { 13 , 9 } , { 13 , 10 } ,
            { 13 , 11 } , { 13 , 12 } , { 13 , 13 } , { 13 , 14 } , { 13 , 15 } , { 14 , 0 } , { 14 , 1 } , { 14 , 2 } ,
            { 14 , 3 } , { 14 , 4 } , { 14 , 5 } , { 14 , 6 } , { 14 , 7 } , { 14 , 8 } , { 14 , 9 } , { 14 , 10 } ,
            { 14 , 11 } , { 14 , 12 } , { 14 , 13 } , { 14 , 14 } , { 14 , 15 } , { 15 , 0 } , { 15 , 1 } , { 15 , 2 } ,
            { 15 , 3 } , { 15 , 4 } , { 15 , 5 } , { 15 , 6 } , { 15 , 7 } , { 15 , 8 } , { 15 , 9 } , { 15 , 10 } ,
            { 15 , 11 } , { 15 , 12 } , { 15 , 13 } , { 15 , 14 } , { 15 , 15 } };

        // This function encodes the two enum values accepted and then converts them as a single value.
        // The value returned from this function is actually the index of the EncodeTable array that is equal
        // to the values supplied against this function.
        // Error handling: -1 suggests that the encoder failed for a reason.
        // 0 is the first index of the EncodeTable array.
        public static System.Int32 Encode(STDFrontColor Front, STDBackColor Back)
        {
            System.Int32 KeepFr = -1;
            System.Int32 KeepBk = -1;

            for (System.Int32 I = 0; I < EncodeTable.Length; I++)
            {
                if (EncodeTable[I, 0] == (System.Int32)Front) { KeepFr = I; }
                if (EncodeTable[I, 1] == (System.Int32)Back) { KeepBk = I; }

                if ((KeepFr != -1) && (KeepBk != -1))
                {
                    if (KeepFr == KeepBk) { return KeepFr; } else { continue; }
                }
            }
            return -1;
        }

        // This function decodes the single number given from the Encode function and then 
        // returns the two colors.
        public static STDColors Decode(System.Int32 Encoded)
        {
            return new STDColors(
                (STDFrontColor)EncodeTable[Encoded, 0],
                (STDBackColor)EncodeTable[Encoded, 1]);
        }

    }

    /// <summary>
    /// Represents a new STD (Standard Text with color Definition) context , which is a storage type
    /// which holds the STD data parsed , or the STD data to parse.
    /// </summary>
    public struct STDContext : System.IDisposable, IEnumerable<STDLine>
    {
        /// <summary>
        /// Initialise a new instance of the <see cref="STDContext"/> structure.
        /// </summary>
        public STDContext() { }

        // Dictionary that keeps the string data to display.
        private System.Collections.Generic.IDictionary<System.Int32, System.String> _dt1 =
            new System.Collections.Generic.SortedList<System.Int32, System.String>();
        // Dictionary that keeps which colors to display to the user.
        // The value set here is returned by the Encode function.
        private System.Collections.Generic.IDictionary<System.Int32, System.Int32> _dt2 =
            new System.Collections.Generic.SortedList<System.Int32, System.Int32>();
        // Dictionary that keeps data determining the type of data written to it , so :
        // 0 indicates a normal STD string , 
        // 1 indicates the STD version block , 
        // and 2 indicates a comment in the file.
        // These values are also exposed at ROOT.STDType enum.
        private System.Collections.Generic.IDictionary<System.Int32, System.Int32> _dt3 =
            new System.Collections.Generic.SortedList<System.Int32, System.Int32>();
        private System.Boolean addedheader = false;
        private System.Int32 Count = -1;
        private System.Boolean _disposed = false;

        /// <summary>
        /// Adds a new STD (Standard Text with color Definition) Line to store in the dictionary.
        /// </summary>
        /// <param name="Fr">The foreground color.</param>
        /// <param name="Bk">The background color.</param>
        /// <param name="Data">The string data to save.</param>
        /// <exception cref="InvalidOperationException" />
        public void Add(STDFrontColor Fr, STDBackColor Bk, System.String Data)
        {
            CheckIfDisposed();
            Count++;
            System.Int32 ER = STDConstants.Encode(Fr, Bk);
            if (ER == -1)
            {
                throw new InvalidOperationException("The Color values given were out of bounds.");
            }
            _dt1.Add(Count, Data);
            _dt2.Add(Count, ER);
            _dt3.Add(Count, 0);
        }

        /// <summary>
        /// Adds a new STD (Standard Text with color Definition) comment.
        /// </summary>
        /// <param name="Comment">The <see cref="System.String"/> comment data to pass.</param>
        public void AddComment(System.String Comment)
        {
            CheckIfDisposed();
            Count++;
            _dt1.Add(Count, Comment);
            _dt2.Add(Count, 0);
            _dt3.Add(Count, 2);
        }

        /// <summary>
        /// Adds a new STD (Standard Text with color Definition) version block.
        /// </summary>
        public void AddVersionBlock()
        {
            CheckIfDisposed();
            if (addedheader)
            {
                throw new InvalidOperationException("A version block has already been added." +
                "\nNo need to add the block two times.");
            }
            else { addedheader = true; }
            Count++;
            _dt1.Add(Count, $"{{Version${STDConstants.VERSION}}}");
            _dt2.Add(Count, 0);
            _dt3.Add(Count, 1);
        }

        /// <summary>
        /// Clears the saved STD (Standard Text with color Definition) entries , which can make this instance reusable.
        /// </summary>
        public void Clear()
        {
            CheckIfDisposed();
            Count = -1;
            addedheader = false;
            _dt1.Clear();
            _dt2.Clear();
            _dt3.Clear();
        }

        /// <summary>
        /// Disposes the current instance. This method is the same as <see cref="Clear()"/> , 
        /// but invalidates the internal dictionaries too.
        /// </summary>
        /// <remarks>
        /// It is not important to explicitly call <see cref="Dispose()"/> , because you can re-use this instance by calling
        /// the <see cref="Clear()"/> method and you have the instance as it is was created.
        /// </remarks>
        public void Dispose() { CheckIfDisposed(); Clear(); _dt1 = null; _dt2 = null; _dt3 = null; _disposed = true; }

        // Adds an invalid item to the dictionary.
        // This is added so as to detect parser errors or code mistakes.
        internal void AddInvalidItem(System.String Data)
        {
            CheckIfDisposed();
            Count++;
            _dt1.Add(Count, Data);
            _dt2.Add(Count, 0);
            _dt3.Add(Count, 3);
        }

        /// <summary>
        /// Counts the found STD (Standard Text with color Definition) entries that are existing on this instance.
        /// </summary>
        /// <remarks>This property includes ALL STD entries , including the Version Block ,
        /// Comments and STD entries.</remarks>
        public System.Int32 ItemsCount { get { CheckIfDisposed(); if (Count == -1) { return 0; } else { return Count + 1; } } }

        /// <summary>
        /// Gets the specified STD (Standard Text with color Definition) Line Entry at the specified index.
        /// </summary>
        /// <param name="Index">The index to get the entry from.</param>
        /// <returns>A new <see cref="STDLine"/> <see langword="struct"/> which contains the STD data.</returns>
        public STDLine Get(System.Int32 Index)
        {
            CheckIfDisposed();
            return new STDLine()
            {
                Colors = STDConstants.Decode(_dt2[Index]),
                Data = _dt1[Index],
                Type = (STDType)_dt3[Index]
            };
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { CheckIfDisposed(); return new STDENUM(this); }

        /// <inheritdoc />
        public IEnumerator<STDLine> GetEnumerator() { CheckIfDisposed(); return new STDENUM(this); }

        /// <summary>
        /// Return the data that an STD structure holds as a new <see cref="STDLine"/>[] array.
        /// </summary>
        /// <param name="context">The STD structure to get the data from.</param>
        public static explicit operator STDLine[](STDContext context)
        {
            STDLine[] output = new STDLine[context.ItemsCount];
            for (System.Int32 I = 0; I < output.Length; I++) { output[I] = context.Get(I); }
            return output;
        }

        private void CheckIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(this.GetType().FullName,
            "Disposed STD contexts are not allowed to be reused.\n If you want to reuse an STD context, " +
            $"then use the {this.GetType().FullName}.Clear() method.");
            }
        }
    }

    // The internal Enumerator implementation.
    // This structure is needed so as the GetEnumerator() function can be exported.
    // However , you might not need to use ever that function; On the other hand , 
    // this kind of implementation is used in an 'foreach' ('For Each' for Visual Basic)
    // context , which that one uses the GetEnumerator() function , which that keyword
    // is really a nice way to iterate through the STD Context without hassle.
    internal struct STDENUM : IEnumerator<STDLine>
    {
        System.Int32 POS = -1;
        STDContext _context;

        public STDENUM() { throw new AggregateException("This constructor cannot build the enumerator correctly."); }

        public STDENUM(STDContext context) { _context = context; }

        public STDLine Current { get { return _context.Get(POS); } }

        System.Object System.Collections.IEnumerator.Current { get { return _context.Get(POS); } }

        public System.Boolean MoveNext()
        {
            POS++;
            if (POS >= _context.ItemsCount) { return false; }
            return true;
        }

        public void Reset() { POS = -1; }

        public void Dispose() { _context.Dispose(); }
    }

    /// <summary>
    /// Represents an STD (Standard Text with color Definition) Line.
    /// </summary>
    [Serializable]
    public struct STDLine
    {
        /// <summary>
        /// The colors to use.
        /// </summary>
        public STDColors Colors;

        /// <summary>
        /// The <see cref="System.String"/> data to show.
        /// </summary>
        public System.String Data;

        /// <summary>
        /// The STD type that was got.
        /// </summary>
        public STDType Type;
    }

    /// <summary>
    /// This structure keeps a record of STD colors that each line uses.
    /// </summary>
    [Serializable]
    public readonly struct STDColors
    {
        /// <summary>
        /// The foreground color to specify.
        /// </summary>
        public readonly STDFrontColor FrontColor;

        /// <summary>
        /// The background color to specify.
        /// </summary>
        public readonly STDBackColor BackColor;

        /// <summary>
        /// Initialise a new <see cref="STDColors"/> structure with the specified STD colors.
        /// </summary>
        /// <param name="FC">The foreground color to specify.</param>
        /// <param name="BK">The background color to specify.</param>
        public STDColors(STDFrontColor FC, STDBackColor BK) { FrontColor = FC; BackColor = BK; }
    }

#pragma warning disable CS1591
    public enum STDFrontColor : System.Int32
    {
        INV = -2,
        Black = 0,
        White = 1,
        Blue = 2,
        Red = 3,
        Magenta = 4,
        Cyan = 5,
        Gray = 6,
        Green = 7,
        Yellow = 8,
        DarkBlue = 9,
        DarkCyan = 10,
        DarkGray = 11,
        DarkGreen = 12,
        DarkMagenta = 13,
        DarkRed = 14,
        DarkYellow = 15
    }

    /// <summary>
    /// The STD (Standard Text with color Definition) type that the entry you want to compare against is.
    /// </summary>
    public enum STDType : System.Int32
    {
        /// <summary>
        /// The Entry is a normal STD string definition.
        /// </summary>
        STDString = 0,
        /// <summary>
        /// The Entry is a valid STD version block.
        /// </summary>
        VersionBlock = 1,
        /// <summary>
        /// The Entry is a comment.
        /// </summary>
        Comment = 2,
        /// <summary>
        /// Invalid Entry which should be ignored.
        /// Normally , this occurs due to the parser incorrect readiness , but was not an error due to the following resons:
        /// </summary>
        /// <remarks>
        /// Unexpected new line , corrupt text , or unexpected characters returned.
        /// </remarks>
        Invalid = 3
    }

    public enum STDBackColor : System.Int32
    {
        INV = -2,
        Black = 0,
        White = 1,
        Blue = 2,
        Red = 3,
        Magenta = 4,
        Cyan = 5,
        Gray = 6,
        Green = 7,
        Yellow = 8,
        DarkBlue = 9,
        DarkCyan = 10,
        DarkGray = 11,
        DarkGreen = 12,
        DarkMagenta = 13,
        DarkRed = 14,
        DarkYellow = 15
    }

#pragma warning restore CS1591

    /// <summary>
    /// <para>
    /// The STD (Standard Text with color Definition) class gets data 
    /// from strings that contain messages colored as specified.
    /// </para>
    /// <para>
    /// This class contains static methods to do serialisation/deserialisation of this format.
    /// </para>
    /// </summary>
    /// <remarks>The STD format is an easy style outlining definition which can be used to a number of applications.
    /// Read more about it in the coding website.</remarks>
    public static class STD
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static STDFrontColor ParseFrontColor(System.String C)
        {
            switch (C.ToUpperInvariant())
            {
                case "BLACK": return STDFrontColor.Black;
                case "WHITE": return STDFrontColor.White;
                case "BLUE": return STDFrontColor.Blue;
                case "RED": return STDFrontColor.Red;
                case "GREEN": return STDFrontColor.Green;
                case "MAGENTA": return STDFrontColor.Magenta;
                case "YELLOW": return STDFrontColor.Yellow;
                case "CYAN": return STDFrontColor.Cyan;
                case "DARK BLUE": return STDFrontColor.DarkBlue;
                case "GRAY": return STDFrontColor.Gray;
                case "DARK CYAN": return STDFrontColor.DarkCyan;
                case "DARK GRAY": return STDFrontColor.DarkGray;
                case "DARK GREEN": return STDFrontColor.DarkGreen;
                case "DARK MAGENTA": return STDFrontColor.DarkMagenta;
                case "DARK YELLOW": return STDFrontColor.DarkYellow;
                case "DARK RED": return STDFrontColor.DarkRed;
                default: return (STDFrontColor)(-2);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static STDBackColor ParseBackColor(System.String C)
        {
            switch (C.ToUpperInvariant())
            {
                case "BLACK": return STDBackColor.Black;
                case "WHITE": return STDBackColor.White;
                case "BLUE": return STDBackColor.Blue;
                case "RED": return STDBackColor.Red;
                case "GREEN": return STDBackColor.Green;
                case "MAGENTA": return STDBackColor.Magenta;
                case "YELLOW": return STDBackColor.Yellow;
                case "CYAN": return STDBackColor.Cyan;
                case "DARK BLUE": return STDBackColor.DarkBlue;
                case "GRAY": return STDBackColor.Gray;
                case "DARK CYAN": return STDBackColor.DarkCyan;
                case "DARK GRAY": return STDBackColor.DarkGray;
                case "DARK GREEN": return STDBackColor.DarkGreen;
                case "DARK MAGENTA": return STDBackColor.DarkMagenta;
                case "DARK YELLOW": return STDBackColor.DarkYellow;
                case "DARK RED": return STDBackColor.DarkRed;
                default: return (STDBackColor)(-2);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static System.String ParseFrontColorS(STDFrontColor C)
        {
            return C switch
            {
                STDFrontColor.Black => "Black",
                STDFrontColor.White => "White",
                STDFrontColor.Cyan => "Cyan",
                STDFrontColor.Red => "Red",
                STDFrontColor.Blue => "Blue",
                STDFrontColor.Gray => "Gray",
                STDFrontColor.Magenta => "Magenta",
                STDFrontColor.Green => "Green",
                STDFrontColor.Yellow => "Yellow",
                STDFrontColor.DarkBlue => "Dark Blue",
                STDFrontColor.DarkGreen => "Dark Green",
                STDFrontColor.DarkRed => "Dark Red",
                STDFrontColor.DarkYellow => "Dark Yellow",
                STDFrontColor.DarkGray => "Dark Gray",
                STDFrontColor.DarkCyan => "Dark Cyan",
                STDFrontColor.DarkMagenta => "Dark Magenta",
                _ => ""
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static System.String ParseBackColorS(STDBackColor C)
        {
            return C switch
            {
                STDBackColor.Black => "Black",
                STDBackColor.White => "White",
                STDBackColor.Cyan => "Cyan",
                STDBackColor.Red => "Red",
                STDBackColor.Blue => "Blue",
                STDBackColor.Gray => "Gray",
                STDBackColor.Magenta => "Magenta",
                STDBackColor.Green => "Green",
                STDBackColor.Yellow => "Yellow",
                STDBackColor.DarkBlue => "Dark Blue",
                STDBackColor.DarkGreen => "Dark Green",
                STDBackColor.DarkRed => "Dark Red",
                STDBackColor.DarkYellow => "Dark Yellow",
                STDBackColor.DarkGray => "Dark Gray",
                STDBackColor.DarkCyan => "Dark Cyan",
                STDBackColor.DarkMagenta => "Dark Magenta",
                _ => ""
            };
        }

        /*
    #pragma warning disable CS8509
        private static System.ConsoleColor AsConsoleColorFrnt(STDFrontColor C)
        {
            return C switch
            {
                STDFrontColor.Black => System.ConsoleColor.Black,
                STDFrontColor.Magenta => System.ConsoleColor.Magenta,
                STDFrontColor.White => System.ConsoleColor.White,
                STDFrontColor.Blue => System.ConsoleColor.Blue,
                STDFrontColor.Red => System.ConsoleColor.Red,
                STDFrontColor.Cyan => System.ConsoleColor.Cyan,
                STDFrontColor.Gray => System.ConsoleColor.Gray,
                STDFrontColor.Green => System.ConsoleColor.Green,
                STDFrontColor.Yellow => System.ConsoleColor.Yellow,
                STDFrontColor.DarkBlue => System.ConsoleColor.DarkBlue,
                STDFrontColor.DarkCyan => System.ConsoleColor.DarkCyan,
                STDFrontColor.DarkGray => System.ConsoleColor.DarkGray,
                STDFrontColor.DarkMagenta => System.ConsoleColor.DarkMagenta,
                STDFrontColor.DarkGreen => System.ConsoleColor.DarkGreen,
                STDFrontColor.DarkRed => System.ConsoleColor.DarkRed,
                STDFrontColor.DarkYellow => System.ConsoleColor.DarkYellow
            };
        }

        private static System.ConsoleColor AsConsoleColorBack(STDBackColor C)
        {
            return C switch
            {
                STDBackColor.Black => System.ConsoleColor.Black,
                STDBackColor.Magenta => System.ConsoleColor.Magenta,
                STDBackColor.White => System.ConsoleColor.White,
                STDBackColor.Blue => System.ConsoleColor.Blue,
                STDBackColor.Red => System.ConsoleColor.Red,
                STDBackColor.Cyan => System.ConsoleColor.Cyan,
                STDBackColor.Gray => System.ConsoleColor.Gray,
                STDBackColor.Green => System.ConsoleColor.Green,
                STDBackColor.Yellow => System.ConsoleColor.Yellow,
                STDBackColor.DarkBlue => System.ConsoleColor.DarkBlue,
                STDBackColor.DarkCyan => System.ConsoleColor.DarkCyan,
                STDBackColor.DarkGray => System.ConsoleColor.DarkGray,
                STDBackColor.DarkMagenta => System.ConsoleColor.DarkMagenta,
                STDBackColor.DarkGreen => System.ConsoleColor.DarkGreen,
                STDBackColor.DarkRed => System.ConsoleColor.DarkRed,
                STDBackColor.DarkYellow => System.ConsoleColor.DarkYellow
            };
        }
    #pragma warning restore CS8509 */

        /// <summary>
        /// Deserialise the STD string data and convert them to a new STD Context.
        /// </summary>
        /// <param name="Data">The <see cref="System.String"/> data to parse.</param>
        /// <returns>A new STD Context.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static STDContext Deserialize(System.String Data)
        {
            System.String[] _tmp = Data.Split('\n');
            System.Boolean terminated = true;
            System.String slicer = null;
            System.Int32 Iterator0 = 0;
            System.Int32 Iterator1 = 0;
            System.String Temp = null;
            System.String Front = null;
            System.String Back = null;
            System.Boolean GivenVersion = false;
            STDContext STD = new();


            foreach (System.String Runner in _tmp)
            {
                try
                {
                    if (Runner.IndexOf(STDConstants.COMMENT, 0, 2) != -1)
                    {
                        // skip execution , it is a comment.
                        STD.AddComment(Runner.Substring(Runner.IndexOf(STDConstants.COMMENT) + 1));
                        terminated = true;
                        slicer = null;
                        continue;
                    }

                    if (Runner.Trim() == "") { break; }

                    // In normal practice , it would be expected that the parser should throw an error.
                    // However , this is done so as to parse as most as possible uncorrupted data.
                    // So , it instead adds an invalid item that can be examined by the programmer and detect the mistake or corruption.
                    if (((Runner.IndexOf(STDConstants.STRING) == -1) && (Runner.IndexOf(STDConstants.SEPERATOR) == -1)) &&
                        (Runner.IndexOf(STDConstants.COMMENT) == -1) && (Runner.IndexOf(STDConstants.HDRCLOSE) == -1) &&
                        (Runner.IndexOf(STDConstants.HDROPEN) == -1))
                    {
                        // Has none of the specified STD characters. In this case , add a new invalid item.
                        STD.AddInvalidItem(Runner);
                        continue;
                    }

                    if (Runner.IndexOf(STDConstants.HDROPEN, 0, 1) != -1)
                    {
                        // Start parsing the version block.
                        Temp = Runner.Substring(Runner.IndexOf(STDConstants.HDROPEN) + 1, Runner.IndexOf(STDConstants.SEPERATOR) - 1);
                        if (Temp.ToUpperInvariant() == "VERSION")
                        {
                            Temp = Runner.Substring(Runner.IndexOf(STDConstants.SEPERATOR) + 1, Runner.Length - Runner.IndexOf(STDConstants.HDRCLOSE));
                            if (System.Convert.ToInt32(Temp) == STDConstants.VERSION) { GivenVersion = true; STD.AddVersionBlock(); }
                        }
                        continue;
                    }

                    if ((Runner.IndexOf(STDConstants.STRING, 0, 1) != -1) && (terminated == true))
                    {
                        Temp = Runner.Substring(Runner.IndexOf(STDConstants.STRING) + 1);
                        slicer = Runner.Substring(Runner.IndexOf(STDConstants.STRING) + 1, Temp.IndexOf(STDConstants.STRING));
                        Iterator0 = Temp.IndexOf(STDConstants.STRING) + 1;
                        if (Temp.Substring(Iterator0, 1) != $"{STDConstants.SEPERATOR}") { terminated = false; continue; }
                        Temp = null;
                        Temp = Runner.Substring(Iterator0 + 3);
                        Iterator1 = Temp.IndexOf(STDConstants.STRING);
                        Front = Temp.Substring(0, Iterator1);
                        if (Temp.Substring(Iterator1 + 1, 1) != $"{STDConstants.SEPERATOR}") { terminated = false; continue; }
                        Back = MAIN.RemoveDefinedChars(Runner.Substring(Runner.LastIndexOf(STDConstants.SEPERATOR) + 1),
                            new[] { STDConstants.SEPERATOR, STDConstants.STRING });
                    }

                    if (terminated == false)
                    {
                        Front = null;
                        Back = null;
                        slicer = null;
                        throw new System.InvalidOperationException("Unterminated STD color string detected.");
                    }
                    else
                    {
                        STDFrontColor SS = ParseFrontColor(Front.Trim());
                        STDBackColor SD = ParseBackColor(Back.Trim());
                        if ((SS == STDFrontColor.INV) || (SD == STDBackColor.INV))
                        {
                            throw new System.InvalidOperationException(
                                $"The STD Color was not parsed. Invalid colors detected: [{Front}] [{Back}]");
                        }
                        STD.Add(SS, SD, slicer);
                        Front = null;
                        Back = null;
                        slicer = null;
                    }
                }
                catch (System.InvalidOperationException EX)
                {
                    // For fatal parser errors , rethrow the particular exception.
                    throw new InvalidOperationException("Could not deserialise the given data.\n", EX);
                }
                catch
                {
                    // Parser error. In this case , add a new invalid item.
                    STD.AddInvalidItem(Runner);
                    continue;
                }
            }

            if (GivenVersion == false)
            {
                throw new InvalidOperationException("The STD Version block was malformed. " +
                    "Please detect the error and fix it so as the data can be parsed again.");
            }
            _tmp = null;
            return STD;
        }

        /// <summary>
        /// Serialise from an STD Context the given data.
        /// </summary>
        /// <param name="Context">The STD Context to get data from.</param>
        /// <returns>The STD encoded data as a single <see cref="System.String"/>.</returns>
        /// <exception cref="AggregateException"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static System.String Serialize(STDContext Context)
        {
            System.String Result = null;
            System.String FR = null;
            System.String BK = null;
            foreach (STDLine STDL in Context)
            {
                if (STDL.Type == STDType.VersionBlock)
                {
                    Result += $"{STDL.Data}\n";
                    continue;
                }
                if (STDL.Type == STDType.Comment)
                {
                    Result += $"{STDConstants.COMMENT}{STDL.Data}\n";
                    continue;
                }
                if (STDL.Type == STDType.STDString)
                {
                    FR = ParseFrontColorS(STDL.Colors.FrontColor);
                    BK = ParseBackColorS(STDL.Colors.BackColor);
                    if (FR == "") { throw new AggregateException("The Front Color string could not be parsed."); }
                    if (BK == "") { throw new AggregateException("The Back Color string could not be parsed."); }
                    Result += $"{STDConstants.STRING}{STDL.Data}{STDConstants.STRING}" +
                        $"{STDConstants.SEPERATOR}{STDConstants.STRING}" +
                        $"{FR}{STDConstants.STRING}{STDConstants.SEPERATOR}{STDConstants.STRING}" +
                        $"{BK}{STDConstants.STRING}\n";
                    FR = null;
                    BK = null;
                    continue;
                }
            }
            return Result;
        }

        /*
        /// <summary>
        /// Displays the current data to the .NET running console.
        /// </summary>
        /// <param name="Context">The STD Context to display the data from.</param>
        public static void DisplayToConsole(STDContext Context)
        {
            STDLine STDL = new();
            for (System.Int32 I = 0; I < Context.ItemsCount - 1; I++)
            {
                if (STDL.Type == STDType.STDString)
                {
                    ROOT.MAIN.WriteCustomColoredText(STDL.Data , 
                        AsConsoleColorFrnt(STDL.colors.FrontColor) , AsConsoleColorBack(STDL.colors.BackColor));
                    continue;
                }
            }
        }
        */
    }

    /// <summary>
    /// The Write Mode to use when the <see cref="STDWriter"/> class is used.
    /// </summary>
    public enum STDWriteMode : System.Int32
    {
        /// <summary>
        /// Serialise and return the data as an new <see cref="STDContext"/> structure.
        /// </summary>
        AsContext = 2,
        /// <summary>
        /// Serialise and return the data as an new <see cref="System.String"/> class.
        /// </summary>
        AsString = 3,
        /// <summary>
        /// Serialise and emit the produced STD data to a new file.
        /// </summary>
        AsFile = 4,
        /// <summary>
        /// Serialise and return the data as a new <see cref="System.Text.StringBuilder"/> class.
        /// </summary>
        AsStringBuilder = 5
    }

    /// <summary>
    /// The STD Writer is another way to write directly either to a new STD context 
    /// or as a raw string.
    /// </summary>
    /// <remarks>This class cannot be inherited.</remarks>
    public sealed class STDWriter : System.IDisposable
    {
        private STDContext _context;
        private STDWriteMode _mode = STDWriteMode.AsContext;
        private System.IO.FileStream _stream = null;
        private System.Text.StringBuilder _sb = null;
        private STDLine _line = new();
        private System.Boolean _closed = true;

        /// <summary>
        /// Start a new instance of the <see cref="STDWriter"/> class. This constructor implies that the mode used is 
        /// the <see cref="STDWriteMode.AsContext"/> .
        /// </summary>
        public STDWriter() { _context = new STDContext(); _context.AddVersionBlock(); }

        /// <summary>
        /// Start a new instance of the <see cref="STDWriter"/> class. The mode that will be used is determined by the 
        /// <paramref name="mode"/> parameter.
        /// </summary>
        /// <param name="mode">The Write mode to use.</param>
        public STDWriter(STDWriteMode mode) { _mode = mode; InitWriter(); }

        private void InitWriter()
        {
            _context = new();
            if (_mode == STDWriteMode.AsFile) { _stream = default; }
            if (_mode == STDWriteMode.AsStringBuilder) { _sb = new(); }
            _context.AddVersionBlock();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _context.Dispose();
            if (_mode == STDWriteMode.AsFile) { _stream.Close(); _stream.Dispose(); }
            if (_mode == STDWriteMode.AsStringBuilder) { _sb.Clear(); _sb = null; }
            _line = default; //discard any data.
        }

        /// <summary>
        /// Returns the underlying STD Context that is being used by the writer.
        /// </summary>
        public STDContext Context { get { return _context; } }

        /// <summary>
        /// Adds a new STD entry.
        /// </summary>
        /// <param name="Data">The STD <see cref="System.String"/> text to add.</param>
        public void AddSTDEntry(System.String Data)
        {
            if (_closed == false)
            {
                throw new System.InvalidOperationException("Cannot add a new STD entry bacause it is not closed.\n" +
                    "To close an STD Entry , use the Colors property so as to close it.");
            }
            _line.Data = Data;
            _line.Type = STDType.STDString;
            _closed = false;
        }

        /// <summary>
        /// The colors to set for a new STD Entry.
        /// </summary>
        public STDColors Colors
        {
            set
            {
                if (_closed)
                {
                    throw new System.InvalidOperationException("A new STD Entry was not defined. Define one , then set it's colors.");
                }
                _line.Colors = value;
                _context.Add(_line.Colors.FrontColor, _line.Colors.BackColor, _line.Data);
                _closed = true;
            }
        }

        /// <summary>
        /// Append a new STD Comment.
        /// </summary>
        /// <param name="Comment">The Comment data to include too.</param>
        public void AddComment(System.String Comment) { _context.AddComment(Comment); }

        /// <summary>
        /// Gets a new instance of <see cref="System.Text.StringBuilder"/> with the serialised 
        /// STD data. Be noted , the class must be initialised with the <see cref="STDWriter(STDWriteMode)"/>
        /// constructor with a value of <see cref="STDWriteMode.AsStringBuilder"/> so that this can work.
        /// </summary>
        /// <exception cref="InvalidOperationException">The class was not initialised with the 
        /// <see cref="STDWriteMode.AsStringBuilder"/> value.</exception>
        /// <exception cref="AggregateException">Inherited from <see cref="STD.Serialize(STDContext)"/> . 
        /// See the documentation on that function so as to learn more about this exception. </exception>
        public System.Text.StringBuilder AsStringBuilder
        {
            get
            {
                if (_mode != STDWriteMode.AsStringBuilder)
                {
                    throw new InvalidOperationException("The instance was prepared for a different target. Use that method instead.");
                }
                _sb.AppendLine();
                _sb.Append(STD.Serialize(_context));
                return _sb;
            }
        }

        /// <summary>
        /// Gets the serialised data as a new <see cref="System.String"/>.
        /// Be noted , the class must be initialised with the <see cref="STDWriter(STDWriteMode)"/>
        /// constructor with a value of <see cref="STDWriteMode.AsString"/> so that this can work.
        /// </summary>
        /// <exception cref="InvalidOperationException">The class was not initialised with the 
        /// <see cref="STDWriteMode.AsString"/> value.</exception>
        /// <exception cref="AggregateException">Inherited from <see cref="STD.Serialize(STDContext)"/> . 
        /// See the documentation on that function so as to learn more about this exception. </exception>
        public System.String AsSingleString
        {
            get
            {
                if (_mode != STDWriteMode.AsString)
                {
                    throw new InvalidOperationException("The instance was prepared for a different target. Use that method instead.");
                }
                System.String DI = null;
                DI += " \n";
                DI += STD.Serialize(_context);
                return DI;
            }
        }

        /// <summary>
        /// <para>
        /// Writes the emitted STD data to a new file , or if the <paramref name="OverwriteIfExists"/> parameter is set to 
        /// <see langword="true"/> , and the file exists , it is overwrriten. </para>
        /// <para>
        /// Be noted , the class must be initialised with the <see cref="STDWriter(STDWriteMode)"/>
        /// constructor with a value of <see cref="STDWriteMode.AsFile"/> so that this can work.
        /// </para>
        /// </summary>
        /// <param name="PathToSave"></param>
        /// <param name="OverwriteIfExists"></param>
        /// <exception cref="System.IO.IOException"></exception>
        /// <exception cref="System.AggregateException">Inherited from <see cref="STD.Serialize(STDContext)"/> . 
        /// See the documentation on that function so as to learn more about this exception , or an file stream could
        /// not be opened to complete this operation.</exception>
        /// <exception cref="InvalidOperationException">The class was not initialised with the 
        /// <see cref="STDWriteMode.AsFile"/> value.</exception>
        public void AsANewFile(System.String PathToSave, System.Boolean OverwriteIfExists = false)
        {
            if (_mode != STDWriteMode.AsFile)
            {
                throw new InvalidOperationException("The instance was prepared for a different target. Use that method instead.");
            }
            if (MAIN.FileExists(PathToSave) && (OverwriteIfExists == false))
            {
                throw new System.IO.IOException($"Could not save the file {PathToSave} because the OverwriteIfExists was \n" +
                    $"not set or set to \'false\'. To allow file overwriting , set the OverwriteIfExists parameter to \'true\'.");
            }
            else if (MAIN.FileExists(PathToSave))
            {
                _stream = MAIN.ClearAndWriteAFile(PathToSave);
            }
            else { _stream = MAIN.CreateANewFile(PathToSave); }
            if (_stream == null)
            {
                throw new System.AggregateException("A new file stream could not be opened. An unexpected error occured.");
            }
            MAIN.AppendNewContentsToFile($"\n{STD.Serialize(_context)}", _stream);
            _stream.Flush();
        }

    }

    /// <summary>
    /// The STD Reader is another way to get the STD serialised data back to a new STD Context.
    /// </summary>
    /// <remarks>This class cannot be inherited.</remarks>
    public sealed class STDReader : System.IDisposable
    {
        private STDContext _context;
        private System.String _serialised = null;

        /// <summary>
        /// Get the data from an simple <see cref="System.String"/> .
        /// </summary>
        /// <param name="STDText">The <see cref="System.String"/> class that contains the STD data.</param>
        public STDReader(System.String STDText) { _serialised = STDText; }

        /// <summary>
        /// Get the data from an instantiated <see cref="System.Text.StringBuilder"/> class.
        /// </summary>
        /// <param name="builder">The <see cref="System.Text.StringBuilder"/> class that contains the STD data.</param>
        public STDReader(System.Text.StringBuilder builder) { _serialised = builder.ToString(); }

        /// <summary>
        /// Get the data from an intialised <see cref="System.IO.Stream"/> .
        /// </summary>
        /// <param name="Stream">The <see cref="System.IO.Stream"/> class that contains the STD data.</param>
        public STDReader(System.IO.Stream Stream)
        {
            System.IO.StreamReader dk = new(Stream);
            _serialised = dk.ReadToEnd();
            dk.Close();
            dk.Dispose();
        }

        /// <summary>
        /// Calling this constructor always throws an <see cref="System.InvalidOperationException"/> exception.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public STDReader()
        {
            throw new InvalidOperationException("The class must be initialised with at least one of the \n" +
            "parameterized constructor variants provided.");
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _context.Dispose();
            _serialised = null;
        }

        /// <summary>
        /// Returns the deserialised data given by the constructors.
        /// </summary>
        public STDContext Context
        {
            get
            {
                _context = STD.Deserialize(_serialised);
                return _context;
            }
        }
    }

}