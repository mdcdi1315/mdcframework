
// An All-In-One framework abstracting the most important classes that are used in .NET
// that are more easily and more consistently to be used.
// The framework was designed to host many different operations , with the last goal 
// to be everything accessible for everyone.

// Global namespaces
using System;
using System.Runtime.Versioning;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

#if WPFExists == false
namespace System.Windows.Forms
{
    /// <summary>
    /// Specifies constants defining which buttons to display on a System.Windows.Forms.MessageBox.
    /// </summary>
    public enum MessageBoxButtons
    {
        /// <summary>
        /// The message box contains an OK button.
        /// </summary>
        OK,
        /// <summary>
        /// The message box contains OK and Cancel buttons.
        /// </summary>
        OKCancel,
        /// <summary>
        /// The message box contains Abort, Retry, and Ignore buttons.
        /// </summary>
        AbortRetryIgnore,
        /// <summary>
        /// The message box contains Yes, No, and Cancel buttons.
        /// </summary>
        YesNoCancel,
        /// <summary>
        /// The message box contains Yes and No buttons.
        /// </summary>
        YesNo,
        /// <summary>
        /// The message box contains Retry and Cancel buttons.
        /// </summary>
        RetryCancel
    }

    /// <summary>
    ///  Specifies constants defining which information to display.
    /// </summary>
    public enum MessageBoxIcon
    {
        /// <summary>
        /// The message box contains no symbols.
        /// </summary>
        None = 0,
        /// <summary>
        /// The message box contains a symbol consisting of a white X in a circle with a red background.
        /// </summary>
        Hand = 16,
        /// <summary>
        /// The message box contains a symbol consisting of a question mark in a circle.
        ///     The question mark message icon is no longer recommended because it does not clearly
        ///     represent a specific type of message and because the phrasing of a message as
        ///     a question could apply to any message type. In addition, users can confuse the
        ///     question mark symbol with a help information symbol. Therefore, do not use this
        ///     question mark symbol in your message boxes. The system continues to support its
        ///     inclusion only for backward compatibility.
        /// </summary>
        Question = 32,
        /// <summary>
        /// The message box contains a symbol consisting of an exclamation point in a triangle with a yellow background.
        /// </summary>
        Exclamation = 48,
        /// <summary>
        /// The message box contains a symbol consisting of a lowercase letter i in a circle.
        /// </summary>
        Asterisk = 64,
        /// <summary>
        /// The message box contains a symbol consisting of white X in a circle with a red background.
        /// </summary>
        Stop = 16,
        /// <summary>
        /// The message box contains a symbol consisting of white X in a circle with a red background.
        /// </summary>
        Error = 16,
        /// <summary>
        /// The message box contains a symbol consisting of an exclamation point in a triangle with a yellow background.
        /// </summary>
        Warning = 48,
        /// <summary>
        /// The message box contains a symbol consisting of a lowercase letter i in a circle.
        /// </summary>
        Information = 64
    }

    /// <summary>
    /// Provides an interface to expose Win32 HWND handles.
    /// </summary>
    [Guid("458AB8A2-A1EA-4d7b-8EBE-DEE5D3D9442C")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComVisible(true)]
    public interface IWin32Window
    {
        /// <summary>
        /// Gets the handle to the window represented by the implementer.
        /// </summary>
        /// <returns>A handle to the window represented by the implementer.</returns>
        IntPtr Handle { get; }
    }
}

namespace System.Drawing
{
    // Contains data-driven classes so as .NET 7.0 can sucessfully be built.
    // This is needed when WPF is unavailable , plus using System.Drawing.Common
    // is an temporary inconvenience , since itself does not allow to run on non-Windows machines.
    // Otherwise , the arbitrary resources that utilise these classes are not used , so everything is OK.

    internal class Icon { public Bitmap ToBitmap() { return null; } }

    internal class Bitmap { }
}
#endif

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
            [SupportedOSPlatform("windows")]
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
                CNGBaseObject.Dispose();
                CNGBaseObject = null;
#pragma warning restore CS0219
            }
        }

        /// <summary>
        /// An example class demonstrating how you can encrypt and decrypt UTF-8 files.
        /// </summary>
        [SupportedOSPlatform("Windows")]
        public static class EDAFile
        {
            /// <summary>
            /// An storage class used in the example.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Design",
                "CA1034:Nested types should not be visible",
                Justification = "It is just an example , not meant for the real world.")]
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
                [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance",
                    "CA1819:Properties should not return arrays",
                    Justification = "It is just an example , not meant for the real world.")]
                public System.Byte[] KeyUsed
                {
                    get { return _KEY_; }
                    set { _KEY_ = value; }
                }

                /// <summary>
                /// The Initialisation Vector that the function <see cref="EncryptAFile(string, string)"/> used.
                /// </summary>
                [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance",
                    "CA1819:Properties should not return arrays",
                    Justification = "It is just an example , not meant for the real world.")]
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
            private System.Int32 TFS;
            private System.Int32 FPD;
            private System.String WOF;
            private System.String WOFP;
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

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage",
                "CA2211:Non-constant fields should not be visible",
                Justification = "This field must be static , plus be visible " +
                "because it exposes the current progress of the archives." +
                "Finally , when this value is refreshed is done through the" +
                " ProgressChanged event. ")]
            public static CurrentProgress Progress;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static System.Int32 FastAbs(System.Int32 Num)
            {
                System.Int32 Value = Num;
                if (Num < 0) { Value = Value * (-1); }
                return Value;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance",
                "CA1810:Initialize reference type static fields inline",
                Justification = "Required so as to properly set the required values." +
                " Failure to do so ends up with an System.NullReferenceException.")]
            static GlobalArchiveProgress()
            {
                ProgressChanged = new(NullEventHandler);
                Progress = new();
            }

            /// <summary>
            /// This event is fired up when one of the properties of the <see cref="CurrentProgress"/> class was changed.
            /// </summary>
            public static event System.EventHandler ProgressChanged;

            internal static void FireChanged() { ProgressChanged.Invoke(typeof(GlobalArchiveProgress), EventArgs.Empty); }

            private static void NullEventHandler(System.Object sender, System.EventArgs e) { }

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
#if NET7_0_OR_GREATER
                    System.Console.WriteLine(EX.Message);
#else
                    MAIN.WriteConsoleText(EX.Message);
#endif
                    return false;
                }
                try { FSO = System.IO.File.OpenWrite(OutputFile); }
                catch (System.Exception EX)
                {
                    FSI.Close();
                    FSI.Dispose();
                    GlobalArchiveProgress.Progress.SetOpFailed();
#if NET7_0_OR_GREATER
                    System.Console.WriteLine(EX.Message);
#else
                    MAIN.WriteConsoleText(EX.Message);
#endif
                    return false;
                }
                try
                {
                    ExternalArchivingMethods.SharpZipLib.GZip.Compress(FSI, FSO, false, 1024, 5);
                }
                catch (System.Exception EX)
                {
                    GlobalArchiveProgress.Progress.SetOpFailed();
#if NET7_0_OR_GREATER
                    System.Console.WriteLine(EX.Message);
#else
                    MAIN.WriteConsoleText(EX.Message);
#endif
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
#if NET7_0_OR_GREATER
                    System.Console.WriteLine(EX.Message);
#else
                    MAIN.WriteConsoleText(EX.Message);
#endif
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
#if NET7_0_OR_GREATER
                    System.Console.WriteLine(EX.Message);
#else
                    MAIN.WriteConsoleText(EX.Message);
#endif
                    return false;
                }

                try { FSO = System.IO.File.OpenWrite(OutputFile); }
                catch (System.Exception EX)
                {
                    GlobalArchiveProgress.Progress.SetOpFailed();
                    FSI.Close();
                    FSI.Dispose();
#if NET7_0_OR_GREATER
                    System.Console.WriteLine(EX.Message);
#else
                    MAIN.WriteConsoleText(EX.Message);
#endif
                    return false;
                }
                try { ExternalArchivingMethods.SharpZipLib.GZip.Decompress(FSI, FSO, false); }
                catch (System.Exception EX)
                {
                    GlobalArchiveProgress.Progress.SetOpFailed();
#if NET7_0_OR_GREATER
                    System.Console.WriteLine(EX.Message);
#else
                    MAIN.WriteConsoleText(EX.Message);
#endif
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
#if NET7_0_OR_GREATER
                    System.Console.WriteLine(EX.Message);
#else
                    MAIN.WriteConsoleText(EX.Message);
#endif
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
#if NET7_0_OR_GREATER
                    System.Console.WriteLine(EX.Message);
#else
                    MAIN.WriteConsoleText(EX.Message);
#endif
                    return false;
                }
                try { FSO = System.IO.File.OpenWrite(OutputFile); }
                catch (System.Exception EX)
                {
                    FSI.Close();
                    FSI.Dispose();
#if NET7_0_OR_GREATER
                    System.Console.WriteLine(EX.Message);
#else
                    MAIN.WriteConsoleText(EX.Message);
#endif
                    return false;
                }
                try { ExternalArchivingMethods.SharpZipLib.BZip2.Compress(FSI, FSO, false, 5); }
                catch (System.Exception EX)
                {
#if NET7_0_OR_GREATER
                    System.Console.WriteLine(EX.Message);
#else
                    MAIN.WriteConsoleText(EX.Message);
#endif
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
#if NET7_0_OR_GREATER
                    System.Console.WriteLine(EX.Message);
#else
                    MAIN.WriteConsoleText(EX.Message);
#endif
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
#if NET7_0_OR_GREATER
                    System.Console.WriteLine(EX.Message);
#else
                    MAIN.WriteConsoleText(EX.Message);
#endif
                    return false;
                }

                try { FSO = System.IO.File.OpenWrite(OutputFile); }
                catch (System.Exception EX)
                {
                    FSI.Close();
                    FSI.Dispose();
#if NET7_0_OR_GREATER
                    System.Console.WriteLine(EX.Message);
#else
                    MAIN.WriteConsoleText(EX.Message);
#endif
                    return false;
                }
                try { ExternalArchivingMethods.SharpZipLib.BZip2.Decompress(FSI, FSO, false); }
                catch (System.Exception EX)
                {
#if NET7_0_OR_GREATER
                    System.Console.WriteLine(EX.Message);
#else
                    MAIN.WriteConsoleText(EX.Message);
#endif
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
#if NET7_0_OR_GREATER
                    System.Console.WriteLine(EX.Message);
#else
                    MAIN.WriteConsoleText(EX.Message);
#endif
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
            /// <param name="UsingCorruptCheck">If this parameter is set to <see langword="true"/> , then it will also perform a check
            /// in the extracted files whether the corrupted files that were not captured in the archive
            /// to be deleted.</param>
            /// <returns><c>true</c> if extraction succeeded; otherwise , <c>false</c>.</returns>
            public static System.Boolean ExtractZipFileToSpecifiedLocation
                (System.String InputArchivePath, System.String OutputPath, System.Boolean UsingCorruptCheck = false)
            {
                GlobalArchiveProgress.Progress = new() { IsMultipleFileOperation = true };
                if (MAIN.FileExists(InputArchivePath) == false) { GlobalArchiveProgress.Progress.SetOpFailed(); return false; }
                if (MAIN.DirExists(OutputPath) == false) { MAIN.CreateADir(OutputPath); }
                System.IO.FileStream FS = MAIN.ReadAFileUsingFileStream(InputArchivePath);
                System.IO.FileStream DI;
                if (FS == null) { GlobalArchiveProgress.Progress.SetOpFailed(); return false; }
                ExternalArchivingMethods.SharpZipLib.ZipFile CT = new(FS);
                GlobalArchiveProgress.Progress.SetOpEnum();
                if (CT.Count < System.Int32.MaxValue)
                {
                    GlobalArchiveProgress.Progress.TotalFiles = (System.Int32)CT.Count;
                }
                else { GlobalArchiveProgress.Progress.TotalFiles = 0; }
                GlobalArchiveProgress.Progress.SetOpDcmp();
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
                                if (MAIN.CreateADir($"{OutputPath}\\{jk}") == false)
                                {
                                    System.String[] Directs = jk.Split('\\');
                                    System.String Tmp = null;
                                    for (System.Int32 I = 0; I < Directs.Length; I++)
                                    {
                                        Tmp += Directs[I] + "\\";
                                        if ((MAIN.DirExists($"{OutputPath}\\{Tmp}") == false)) { if (MAIN.CreateADir($"{OutputPath}\\{Tmp}") == false) { goto GI_ERR; } }
                                    }
                                }
                            }
                            jk = null;
                        }
                        DI = MAIN.CreateANewFile($"{OutputPath}\\{g}");
                        GlobalArchiveProgress.Progress.WorkingOnFile = g;
                        GlobalArchiveProgress.Progress.WorkingOnFileFullPath = $"{OutputPath}\\{g}";
                        g = null;
                        if (DI == null) { goto GI_ERR; }
                        using (DI) { CT.GetInputStream(un).CopyTo(DI, 4096); }
                        if (UsingCorruptCheck && MAIN.GetACryptographyHashForAFile(
                            $"{OutputPath}\\{g}", HashDigestSelection.SHA256) ==
                            "ece758103b8bb6d4fbe7a7c90889f3c6fb516370648c152a6f587bc2f0522b5a")
                        { MAIN.DeleteAFile($"{OutputPath}\\{g}"); }
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
                List<System.IO.DirectoryInfo> Dirs = new(E1.GetDirectories("*", System.IO.SearchOption.AllDirectories)) { E1 };
                foreach (System.IO.DirectoryInfo FV in Dirs) { GlobalArchiveProgress.Progress.TotalFiles += FV.GetFiles("*").Length; }
                // Open a new FileStream to the desired path.
                // Exit with FALSE if it could not be opened.
                System.IO.FileStream EDI = MAIN.CreateANewFile(PathOfZipToMake);
                if (EDI == null) { goto GI_ERR2; }
                // The fundamental ZIP class: The ZIP Stream that is being used for compression.
                ExternalArchivingMethods.SharpZipLib.ZipOutputStream DI = new(EDI);
                // Begin execution Phase. Set the compression level to the desired one.
                DI.SetLevel((System.Int32)CmpLevel);
                GlobalArchiveProgress.Progress.SetOpCmp();
                // Now just add all the files in the archive! So simple.
                System.IO.FileInfo[] FI;
                // Declare also a Boolean value so as to detect whether a file was added plainly due to the excluded files list.
                System.Boolean AddedPlainly = false;
                foreach (System.IO.DirectoryInfo DA_ in Dirs)
                {
                    TempPathConstructor = ExternalArchivingMethods.SharpZipLib.ZipEntry.CleanName(DA_.FullName.Substring(BaseDir.Length));
                    FI = DA_.GetFiles();
                    if (FI == Array.Empty<System.IO.FileInfo>()) { continue; }
                    foreach (System.IO.FileInfo DOI in FI)
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
                        switch (DOI.Extension.ToLowerInvariant())
                        {
                            case ".zip":
                            case ".cab":
                            case ".gz":
                            case ".tgz":
                            case ".rar":
                            case ".7z":
                            case ".br":
                                AddedPlainly = true;
                                DI.SetLevel(0);
                                break;
                        };
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
                        if (GDX.Length > 0) { MAINInternal.BufferedCopyStream(GDX, DI, GDX.Length); }
                        GDX.Close();
                        GDX.Dispose();
                        DI.CloseEntry();
                        if (AddedPlainly) { DI.SetLevel((System.Int32)CmpLevel); AddedPlainly = false; }
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
            GI_ERR:
                {
                    if (DI != null) { DI.Close(); DI.Dispose(); }
                    if (EDI != null) { EDI.Close(); EDI.Dispose(); }
                    if (GDX != null) { GDX.Close(); GDX.Dispose(); }
                    if (Dirs != null) { Dirs.Clear(); Dirs = null; }
                    GlobalArchiveProgress.Progress.SetOpFailed();
                    return false;
                }
            GI_ERR2:
                {
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
                        if (GDX.Length > 0) { MAINInternal.BufferedCopyStream(GDX, DI, GDX.Length); }
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
                Dictionary<System.String, System.String> FN = null;
                try
                {
                    ExternalArchivingMethods.Cabinets.CabInfo CI = new(OutputArchivePath);
                    GlobalArchiveProgress.Progress.SetOpEnum();
                    System.IO.DirectoryInfo DI = new(DirToCapture);
                    System.IO.FileSystemInfo[] FileArray = MAIN.GetANewFileSystemInfo(DI.FullName);
                    if (FileArray == null) { return false; }
                    FN = new();
                    foreach (System.IO.FileSystemInfo FSI in FileArray)
                    {
                        if (FSI is System.IO.FileInfo FI) { FN.Add(FI.FullName.Substring(DI.FullName.Length + 1), FI.FullName); }
                    }
                    Array.Clear(FileArray, 0, FileArray.Length);
                    FileArray = null;
                    GlobalArchiveProgress.Progress.SetOpCmp();
                    CI.PackFileSet(DirToCapture, FN,
                        (ExternalArchivingMethods.Cabinets.CompressionLevel)CLevel,
                        GlobalArchiveProgress.GetProgressFromCabinets);
                    CI.Refresh();
                }
                catch (System.IO.IOException EX)
                {
                    GlobalArchiveProgress.Progress.SetOpFailed();
                    MAIN.ExceptionData = EX;
                    return false;
                }
                finally
                {
                    if (FN != null) { FN.Clear(); FN = null; }
                    DC.Dispose();
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
                catch (System.IO.IOException EX)
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
                catch (System.Security.SecurityException EX)
                {
                    GlobalArchiveProgress.Progress.SetOpFailed();
                    MAIN.WriteConsoleText(EX.Message);
                    return false;
                }
                catch (System.IO.IOException EX)
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design",
            "CA1027:Mark enums with FlagsAttribute",
            Justification = "Cannot apply bitwise combinations of constants" +
            " in an enumeration which holds a file compression level.")]
        [SupportedOSPlatform("windows")]
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

    internal readonly struct HW31Mapper
    {
        // Kept for backwards compatibility.
        /*
		public static readonly System.Object[,] Mapper = { { 0 , "AA" } , { 1 , "AB" } , { 2 , "AC" } , { 3 , "AD" } , { 4 , "AE" } , { 5 , "AF" } ,
		{ 6 , "AG" } , { 7 , "AH" } , { 8 , "AI" } , { 9 , "AJ" } , {10 , "AK" } , {11 , "AL" } , {12 , "AM" } , { 13 , "AN" } , { 14 , "AO" } ,
		{ 15 , "AP" } , { 16 , "AQ" } , { 17 , "AR" } , { 18 , "AS" } , { 19 , "AT" } , { 20 , "AU" } , { 21 , "AV" } , { 22 , "AW" } , { 23 , "AX" },
		{ 24 , "AY" } , { 25 , "AZ" } , { 26 , "Aa" } , { 27 , "Ab" } , { 28 , "Ac" } , { 29 , "Ad" } , { 30 , "Ae" } , { 31 , "Af" } , { 32 , "Ag" },
		{ 33 , "Ah" } , { 34 , "Ai" } , { 35 , "Aj" } , { 36 , "Ak" } , { 37  , "Al" } , { 38 , "Am" } , { 39 , "An" } , { 40 , "Ao" } , { 41 , "Ap" },
		{ 42 , "Aq" } , { 43 , "Ar" } , { 44 , "As" } , { 45 , "At" } , { 46 , "Au" } , { 47 , "Av" } , { 48 , "Aw" } , { 49 , "Ax" } , { 50 , "Ay" },
		{ 51 , "Az" } , { 52 , "aA" } , { 53 , "aB" } , { 54 , "aC" } , { 55 , "aD" } , { 56 , "aE" } , { 57 , "aF" } , { 58 , "aG" } , { 59 , "aH" },
		{ 60 , "aI" } , { 61 , "aJ" } , { 62 , "aK" } , { 63 , "aL" } , { 64 , "aM" } , { 65 , "aN" } , { 66 , "aO" } , { 67 , "aP" } , { 68 , "aQ" },
		{ 69 , "aR" } , { 70 , "aS" } , { 71 , "aT" } , { 72 , "aU" } , { 73 , "aV" } , { 74 , "aW" } , { 75 , "aX" } , { 76 , "aY" } , { 77 , "aZ" },
		{ 78 , "aa" } , { 79 , "ab" } , { 80 , "ac" } , { 81 , "ad" } , { 82 , "ae" } , { 83 , "af" } , { 84 , "ag" } , { 85 , "ah" } , { 86 , "ai" },
		{ 87 , "aj" } , { 88 , "ak" } , { 89 , "al" } , { 90 , "am" } , { 91 , "an" } , { 92 , "ao" } , { 93 , "ap" } , { 94 , "aq" } , { 95 , "ar" },
		{ 96 , "as" } , { 97 , "at" } , { 98 , "au" } , { 99 , "av" } , { 100 , "aw" } , { 101 , "ax" } , { 102 , "ay" } , { 103 , "az" } ,
		{ 104 , "BA" } , { 105 , "BB" } , { 106 , "BC" } , { 107 , "BD" } , { 108 , "BE" } , { 109 , "BF" } , { 110 , "BG" } , { 111 , "BH" },
		{ 112 , "BI" } , { 113 , "BJ" } , { 114 , "BK" } , { 115 , "BL" } , { 116 , "BM" } , { 117 , "BO" } , { 118 , "BP" },
		{ 119 , "BQ" } , { 120 , "BR" } , { 121 , "BS" } , { 122 , "BT" } , { 123 , "BU" } , { 124 , "BV" } , { 125 , "BW" } , { 126 , "BX" },
		{ 127 , "BY" } , { 128 , "BZ" } , { 129 , "Ba" } , { 130 , "Bb" } , { 131 , "Bc" } , { 132 , "Bd" } , { 133 , "Be" } , { 134 , "Bf" },
		{ 135 , "Bg" } , { 136 , "Bh" } , { 137 , "Bi" } , { 138 , "Bj" } , { 139 , "Bk" } , { 140 , "Bl" } , { 141 , "Bm" } , { 142 , "Bn" } ,
		{ 143 , "Bo" } , { 144 , "Bp" } , { 145 , "Bq" } , { 146 , "Br" } , { 147 , "Bs" } , { 148 , "Bt" } , { 149 , "Bu" } , { 150 , "Bv" },
		{ 151 , "Bw" } , { 152 , "Bx" } , { 153 , "By" } , { 154 , "Bz" } , { 155 , "bA" } , { 156 , "bB" } , { 157 , "bC" } , { 158 , "bD" },
		{ 159 , "bE" } , { 160 , "bF" } , { 161 , "bG" } , { 162 , "bH" } , { 163 , "bI" } , { 164 , "bJ" } , { 165 , "bK" } , { 166 , "bL" },
		{ 167 , "bM" } , { 168 , "bN" } , { 169 , "bP" } , { 170 , "bQ" } , { 171 , "bR" } , { 172 , "bS" } , { 173 , "bT" },
		{ 174 , "bU" } , { 175 , "bV" } , { 176 , "bW" } , { 177 , "bX" } , { 178 , "bY" } , { 179 , "bZ" } , { 180 , "ba" } , { 181 , "bb" },
		{ 182 , "bc" } , { 183 , "bd" } , { 184 , "be" } , { 185 , "bf" } , { 186 , "bg" } , { 187 , "bh" } , { 188 , "bi" } , { 189 , "bj" },
		{ 190 , "bk" } , { 191 , "bl" } , { 192 , "bm" } , { 193 , "bn" } , { 194 , "bo" } , { 195 , "bp" } , { 196 , "bq" } , { 197 , "br" },
		{ 198 , "bs" } , { 199 , "bt" } , { 200 , "bu" } , { 201 , "bv" } , { 202 , "bw" } , { 203 , "bx" } , { 204 , "by" } , { 205 , "bz" },
		{ 206 , "CA" } , { 207 , "CB" } , { 208 , "CC" } , { 209 , "CD" } , { 210 , "CE" } , { 211 , "CF" } , { 212 , "CG" } , { 213 , "CH" },
		{ 214 , "CI" } , { 215 , "CJ" } , { 216 , "CK" } , { 217 , "CL" } , { 218 , "CM" } , { 219 , "CN" } , { 220 , "CO" } , { 221 , "CP" },
		{ 222 , "CQ" } , { 223 , "CR" } , { 224 , "CS" } , { 225 , "CT" } , { 226 , "CU" } , { 227 , "CV" } , { 228 , "CW" } , { 229 , "CX" },
		{ 230 , "CY" } , { 231 , "CZ" } ,  { 232 , "Ca" } , { 233 , "Cb" } , { 234 , "Cc" } , { 235 , "Cd" } , { 236 , "Cf" } , { 237 , "Cg" },
		{ 238 , "Ch" } , { 239 , "Ci" } , { 240 , "Cj" } , { 241 , "Ck" } , { 242 , "Cl" } , { 243 , "Cm" } , { 244 , "Cn" } , { 245 , "Co" },
		{ 246 , "Cp" } , { 247 , "Cq" } , { 248 , "Cr" } , { 249 , "Cs" } , { 250 , "Ct" } , { 251 , "Cu" } , { 252 , "Cv" } , { 253 , "Cw" },
		{ 254 , "Cx" } , { 255 , "Cy" } };
		*/

        // This field keeps all the valid byte numbers , range 0..255 . ,256 bytes in total.
        public static readonly System.Int32[] nums = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22,
            23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54,
            55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86,
            87, 88, 89, 90, 91, 92, 93, 94, 95, 96, 97, 98, 99, 100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113,
            114, 115, 116, 117, 118, 119, 120, 121, 122, 123, 124, 125, 126, 127, 128, 129, 130, 131, 132, 133, 134, 135, 136, 137,
            138, 139, 140, 141, 142, 143, 144, 145, 146, 147, 148, 149, 150, 151, 152, 153, 154, 155, 156, 157, 158, 159, 160, 161,
            162, 163, 164, 165, 166, 167, 168, 169, 170, 171, 172, 173, 174, 175, 176, 177, 178, 179, 180, 181, 182, 183, 184, 185,
            186, 187, 188, 189, 190, 191, 192, 193, 194, 195, 196, 197, 198, 199, 200, 201, 202, 203, 204, 205, 206, 207, 208, 209,
            210, 211, 212, 213, 214, 215, 216, 217, 218, 219, 220, 221, 222, 223, 224 , 225 , 226 , 227 , 228 ,229, 230, 231, 232,
            233, 234, 235, 236, 237, 238, 239, 240, 241, 242, 243, 244, 245, 246, 247, 248, 249, 250, 251, 252, 253, 254 , 255 };

        // This field is the matching case of every single byte of the above field. Each single byte is assigned in a single
        // unique string.
        public static readonly System.String[] chars = { "AA", "AB", "AC", "AD", "AE", "AF", "AG", "AH", "AI", "AJ", "AK", "AL",
            "AM", "AN", "AO", "AP", "AQ", "AR", "AS", "AT", "AU", "AV", "AW", "AX", "AY", "AZ", "Aa", "Ab", "Ac", "Ad", "Ae", "Af", "Ag",
            "Ah", "Ai", "Aj", "Ak", "Al", "Am", "An", "Ao", "Ap", "Aq", "Ar", "As", "At", "Au", "Av", "Aw", "Ax", "Ay", "Az", "aA", "aB",
            "aC", "aD", "aE", "aF", "aG", "aH", "aI", "aJ", "aK", "aL", "aM", "aN", "aO", "aP", "aQ", "aR", "aS", "aT", "aU", "aV", "aW",
            "aX", "aY", "aZ", "aa", "ab", "ac", "ad", "ae", "af", "ag", "ah", "ai", "aj", "ak", "al", "am", "an", "ao", "ap", "aq", "ar",
            "as", "at", "au", "av", "aw", "ax", "ay", "az", "BA", "BB", "BC", "BD", "BE", "BF", "BG", "BH", "BI", "BJ", "BK", "BL", "BM",
            "BO", "BP", "BQ", "BR", "BS", "BT", "BU", "BV", "BW", "BX", "BY", "BZ", "Ba", "Bb", "Bc", "Bd", "Be", "Bf", "Bg", "Bh", "Bi",
            "Bj", "Bk", "Bl", "Bm", "Bn", "Bo", "Bp", "Bq", "Br", "Bs", "Bt", "Bu", "Bv", "Bw", "Bx", "By", "Bz", "bA", "bB", "bC", "bD",
            "bE", "bF", "bG", "bH", "bI", "bJ", "bK", "bL", "bM", "bN", "bP", "bQ", "bR", "bS", "bT", "bU", "bV", "bW", "bX", "bY", "bZ",
            "ba", "bb", "bc", "bd", "be", "bf", "bg", "bh", "bi", "bj", "bk", "bl", "bm", "bn", "bo", "bp", "bq", "br", "bs", "bt", "bu",
            "bv", "bw", "bx", "by", "bz", "CA", "CB", "CC", "CD", "CE", "CF", "CG", "CH", "CI", "CJ", "CK", "CL", "CM", "CN", "CO", "CP",
            "CQ", "CR", "CS", "CT", "CU", "CV", "CW", "CX", "CY", "CZ", "Ca", "Cb", "Cc", "Cd", "Cf", "Cg", "Ch", "Ci", "Cj", "Ck", "Cl",
            "Cm", "Cn", "Co", "Cp", "Cq", "Cr", "Cs", "Ct", "Cu", "Cv", "Cw", "Cx", "Cy" };
    }

    /// <summary>
    /// A static class which constructs HW31 strings from <see cref="System.Byte"/>[] arrays.
    /// </summary>
    public static class HW31Strings
    {
        /*
		 * Where this class is useful? 
		 * > The class , like Base64 , creates a string representation of the byte array given.
		 *    However , Base64 and HW31 have differences:
		 *    HW31 allocates two unique characters representing each byte; Base64 creates the next character based on the last one and the next byte value.
		 *    Base64 leaves null characters at the end of the string(Interpreted as '=') , while HW31 leaves always only a space in the end of the string.
		 * > HW31 will also always produce the same result , no matter how, except in the case of corrupt string , which even then will return zero's.
		 * > HW31 has a dictionary which allows it to pick the appropriate byte each time and translate it into a HW31.
		 * Is it reliable to save byte data on an HW31 string?
		 * It depends on what you will use it. For data encryption keys , better is the Base64;
		 * For small binary data , HW31 will do the work fine.
		 * HW31 could be also used in small data dictionaries , where data precision is required.
		 */

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        private static System.String ByteToCorrespondingChars(System.Byte Value)
        {
            for (System.Int32 I = 0; I < HW31Mapper.nums.Length; I++) { if (HW31Mapper.nums[I] == Value) { return $"{HW31Mapper.chars[I]}"; } }
            return "Error";
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        private static System.Byte CharsToCorrespondingByte(System.String Chars)
        {
            if (Chars.Length != 2) { return 0; }
            for (System.Int32 I = 0; I < HW31Mapper.chars.Length; I++) { if (HW31Mapper.chars[I] == Chars) { return (System.Byte)HW31Mapper.nums[I]; } }
            return 0;
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design",
            "CA1031:Do not catch general exception types",
            Justification = "Used to catch several exceptions at once.")]
        private static System.Boolean TestIfItIsAnHW31String(System.String HW31)
        {
            if (HW31 == null) { return false; }
            if (HW31.Length < 3) { return false; }
            System.Char[] HW31Arr = HW31.ToCharArray();
            if (HW31Arr[2] != ' ') { return false; }
            if (HW31Arr[HW31Arr.Length - 1] != ' ') { return false; }
            for (System.Int32 I = 0; I < HW31Arr.Length; I++)
            {
                try
                {
                    if (IsDigit(System.Convert.ToInt32(HW31Arr[I])) == true) { return false; }
                }
                catch { continue; }
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsDigit(System.Int32 I) { return (System.UInt32)(I - '0') <= ('9' - '0'); }

        /// <summary>
        /// Converts a <see cref="System.Byte"/>[] array to a new HW31 <see cref="System.String"/>. 
        /// </summary>
        /// <param name="Array">The Byte array to get the data from.</param>
        /// <returns>A new HW31 <see cref="System.String"/> . </returns>
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static HW31 ByteArrayToHW31String(System.Byte[] Array)
        {
            HW31 DC = new HW31();
            if (Array.Length < 1) { DC.SetOrGetError = true; return DC; }
            System.String Result = null;
            System.String tmp = "";
            for (System.Int32 I = 0; I < Array.Length; I++)
            {
                tmp = ByteToCorrespondingChars((System.Byte)HW31Mapper.nums[Array[I]]);
                if (tmp != "Error") { Result += (tmp + " "); }
                else { DC.SetOrGetError = true; return DC; }
            }
            DC = new HW31(Result);
            return DC;
        }

        /// <summary>
        /// Converts a <see cref="System.Byte"/>[] array to a new HW31 <see langword="struct"/>. 
        /// </summary>
        /// <param name="Array">The Byte array to get the data from.</param>
        /// <param name="Count">How many iterations will happen to the array.</param>
        /// <param name="Start">From which point the iterator will start calculating.</param>
        /// <returns>A new HW31 <see langword="struct"/> .</returns>
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static HW31 ByteArrayToHW31String(System.Byte[] Array, System.Int32 Start, System.Int32 Count)
        {
            HW31 DC = new HW31();
            if (Array.Length < 1) { DC.SetOrGetError = true; return DC; }
            if (Start < 0) { DC.SetOrGetError = true; return DC; }
            if (Start > Count) { DC.SetOrGetError = true; return DC; }
            if (Count < 1) { DC.SetOrGetError = true; return DC; }
            System.String Result = null;
            System.String tmp = "";
            for (System.Int32 I = Start; I < Count; I++)
            {
                tmp = ByteToCorrespondingChars((System.Byte)HW31Mapper.nums[Array[I]]);
                if (tmp != "Error") { Result += (tmp + " "); }
                else { DC.SetOrGetError = true; return DC; }
            }
            DC = new HW31(Result);
            return DC;
        }

        /// <summary>
        /// Calculates the length of the HW31 string before it is created.
        /// </summary>
        /// <param name="Array">The <see cref="System.Byte"/>[] to calculate the data from.</param>
        /// <returns>The estimated HW31 <see langword="struct"/> containing length.</returns>
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static System.Int64 EstimateHW31StringLength([System.Diagnostics.CodeAnalysis.DisallowNull] System.Byte[] Array)
        { if (Array == null) { return -1; } return Array.Length * 3; }

        /// <summary>
        /// Converts a created HW31 <see cref="System.String"/> back to a <see cref="System.Byte"/>[] array.
        /// </summary>
        /// <param name="HW31String">The already created HW31 <see cref="System.String"/>. </param>
        /// <returns>A new <see cref="System.Byte"/>[] containing the byte data kept by the HW31 string.</returns>
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static System.Byte[] HW31StringToByteArray(HW31 HW31String)
        {
            if (TestIfItIsAnHW31String(HW31String.ReturnHW31) == false) { return null; }
            System.Char[] HW31Arr = HW31String.ReturnHW31.ToCharArray();
            if (HW31Arr[HW31Arr.Length - 1] != ' ') { return null; }
            System.Int32 BlanksToRemove = 0;
            for (System.Int32 I = 0; I < HW31Arr.Length; I++) { if (HW31Arr[I] == ' ') { BlanksToRemove += 2; } }
            System.Byte[] Result = new System.Byte[HW31Arr.Length - BlanksToRemove];
            System.String _Tmp = null;
            System.Int32 Count = 0;
            System.Int32 ArrCount = 0;
            try
            {
                for (System.Int32 I = 0; I < HW31Arr.Length; I++)
                {
                    if (HW31Arr[I] != ' ') { _Tmp += HW31Arr[I]; Count++; }
                    if (Count >= 2) { Result[ArrCount] = CharsToCorrespondingByte(_Tmp); Count = 0; ArrCount++; _Tmp = null; }
                }
            }
            catch (System.Exception) { return null; }
            return Result;
        }

        /// <summary>
        /// Converts the HW31 string data back to an byte array.
        /// </summary>
        /// <param name="str">The HW31 structure to get data from.</param>
        /// <returns>A new <see cref="System.Byte"/>[] containing the original data.</returns>
        public static System.Byte[] ToArray(this HW31 str) { return HW31StringToByteArray(str);  }
    }

    /// <summary>
    /// The HW31 structure. HW31 is an intermediate storage to store binary data to <see cref="System.String"/>'s and the opposite.
    /// </summary>
#nullable enable
    [Serializable]
    public struct HW31 : IEquatable<HW31?>, IEquatable<HW31>
    {
        private System.String BackField;
        private System.Boolean Erro_r = false;

        internal System.String ReturnHW31 { get { return BackField; } }
        internal System.Boolean SetOrGetError { get { return Erro_r; } set { Erro_r = value; } }

        /// <summary>
        /// Returns a <see cref="System.Boolean"/> value , indicating that this HW31 is invalid and should be destroyed.
        /// </summary>
        /// <returns><c>true</c> if this structure is invalid; otherwise , <c>false</c> if it is usuable.</returns>
        public System.Boolean IsInvalid() { return Erro_r; }

        /// <summary>
        /// Create a new HW31 structure.
        /// </summary>
        /// <param name="HW31">The HW31 <see cref="System.String"/> to create from.</param>
        /// <exception cref="System.InvalidOperationException">
        /// The <see cref="System.String"/> attempted to set was not in the HW31 format.</exception>
        public HW31(System.String HW31)
        {
            if (TestIfItIsAnHW31String(HW31) == false)
            {
                throw new System.InvalidOperationException("Invalid attempt to set a string which is not an HW31 one.");
            }

            BackField = HW31;
        }

        /// <summary>Initialises a new instance of the <see cref="HW31"/> structure.</summary>
        public HW31() { BackField = ""; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design",
           "CA1031:Do not catch general exception types",
           Justification = "Used to catch several exceptions at once.")]
        private static System.Boolean TestIfItIsAnHW31String(System.String HW31)
        {
            if (HW31 == null) { return false; }
            if (HW31.Length < 3) { return false; }
            System.Char[] HW31Arr = HW31.ToCharArray();
            if (HW31Arr[2] != ' ') { return false; }
            if (HW31Arr[HW31Arr.Length - 1] != ' ') { return false; }
            for (System.Int32 I = 0; I < HW31Arr.Length; I++)
            {
                try
                {
                    if (IsDigit(System.Convert.ToInt32(HW31Arr[I])) == true) { return false; }
                }
                catch { continue; }
            }
            return true;
        }

        /// <summary>
        /// Detects if the specified <see cref="System.String"/> is an HW31 <see cref="System.String"/>.
        /// </summary>
        /// <param name="HW31">The HW31 <see cref="System.String"/> to test.</param>
        /// <returns><c>true</c> if the <paramref name="HW31"/> 
        /// can be an HW31 <see langword="struct"/>; otherwise , <c>false</c>.</returns>
        public static System.Boolean IsHW31(System.String HW31) { return TestIfItIsAnHW31String(HW31); }

        /// <summary>
        /// Test if an <see cref="HW31"/> structure that holds the HW31 <see cref="System.String"/> 
        /// is equal to a non-structured HW31 <see cref="System.String"/>.
        /// </summary>
        /// <param name="left">The <see cref="HW31"/> structure to take the <see cref="System.String"/> from.</param>
        /// <param name="right">The non-structured <see cref="System.String"/> to compare against.</param>
        /// <returns><c>true</c> if these two objects specified are equal; otherwise ,  <c>false</c>.</returns>
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static System.Boolean operator ==(HW31 left, System.String right) { return left.ReturnHW31.Equals(right); }

        /// <summary>
        /// Test if an <see cref="HW31"/> structure that holds the HW31 <see cref="System.String"/> 
        /// is NOT equal to a non-structured HW31 <see cref="System.String"/>.
        /// </summary>
        /// <param name="left">The <see cref="HW31"/> structure to take the <see cref="System.String"/> from.</param>
        /// <param name="right">The non-structured <see cref="System.String"/> to compare against.</param>
        /// <returns><c>true</c> if these two objects specified are NOT equal; otherwise ,  <c>false</c>.</returns>
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static System.Boolean operator !=(HW31 left, System.String right) { return left.ReturnHW31.Equals(right) == false; }

        /// <summary>
        /// Test if an <see cref="HW31"/> structure is equal to another.
        /// </summary>
        /// <param name="lhs">The first structure.</param>
        /// <param name="rhs">The second structure.</param>
        /// <returns><c>true</c> if the structures are equal; otherwise , <c>false</c>.</returns>
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static System.Boolean operator ==(HW31 lhs, HW31 rhs) { return lhs.Equals(rhs); }

        /// <summary>
        /// Test if an <see cref="HW31"/> structure is NOT equal to another.
        /// </summary>
        /// <param name="lhs">The first structure.</param>
        /// <param name="rhs">The second structure.</param>
        /// <returns><c>true</c> if the structures are NOT equal; otherwise , false.</returns>
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static System.Boolean operator !=(HW31 lhs, HW31 rhs) { return lhs.Equals(rhs) == false; }

        /// <summary>
        /// Test if an generic object is equal to this HW31 instance structure.
        /// </summary>
        /// <param name="obj">The generic object to compare.</param>
        /// <returns><c>true</c> if this structure is equal to the <paramref name="obj"/>;
        /// otherwise , <c>false</c>.</returns>
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public override System.Boolean Equals(object? obj) { return HW31.Equals(this, obj); }

        /// <summary>
        /// Test if another nullable HW31 construct is equal to this HW31 instance structure.
        /// </summary>
        /// <param name="other">The nullable HW31 construct to compare.</param>
        /// <returns><c>true</c> if this structure is equal to the <paramref name="other"/>;
        /// otherwise , <c>false</c>.</returns>
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public System.Boolean Equals(HW31? other) { if (other?.Equals(this) == true) { return true; } else { return false; } }

        /// <summary>
        /// Test if another HW31 construct is equal to this HW31 instance structure.
        /// </summary>
        /// <param name="other">The nullable HW31 construct to compare.</param>
        /// <returns><c>true</c> if this structure is equal to the <paramref name="other"/>;
        /// otherwise , <c>false</c>.</returns>
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public System.Boolean Equals(HW31 other) { if (other.Equals(this) == true) { return true; } else { return false; } }

        /// <summary>
        /// Direct conversion from an HW31 string to it's equivalent structure.
        /// </summary>
        /// <param name="input">The HW31 string to convert.</param>
        public static implicit operator HW31(System.String input) { return new HW31(input); }

        /// <summary>
        /// Direct conversion from an HW31 structure to it's equivalent string.
        /// </summary>
        /// <param name="input">The HW31 structure to convert.</param>
        public static implicit operator System.String(HW31 input) { return input.ToString(); }

        /// <inheritdoc />
        public override System.Int32 GetHashCode() { return BackField.GetHashCode(); }

        /// <summary>
        /// Gets the length of the HW31 <see cref="System.String"/>.
        /// </summary>
        /// <returns>The computed length.</returns>
        public System.Int32 Length() { return BackField.Length; }

        /// <summary>
        /// Gets the length of the HW31 <see cref="System.String"/> , but only the real interpreted characters.
        /// </summary>
        /// <returns>The computed length.</returns>
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public System.Int32 ClearLength()
        {
            System.Char[] Chars = BackField.ToCharArray();
            System.Int32 Remove = 0;
            for (System.Int32 I = 0; I < Chars.Length; I++) { if (Chars[I] == ' ') { Remove++; } }
            return Chars.Length - Remove;
        }

        // The below code is residing in the System.Buffers.Text namespace , which is a method for the internal mechanisms.
        // Here it is used for checking if the HW31 has digits , which is illegal.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsDigit(System.Int32 I) { return (System.UInt32)(I - '0') <= ('9' - '0'); }

        /// <summary>
        /// Returns the real HW31 <see cref="System.String"/> created.
        /// </summary>
        /// <returns>The HW31 <see cref="System.String"/> .</returns>
        public override System.String ToString()
        {
            if (Erro_r == true)
            {
                throw new InvalidOperationException("Cannot use this HW31 instance " +
                "because this structure is marked as invalid.");
            }
            return BackField.ToString();
        }

    }
#nullable disable

    /// <summary>
    /// The Registry Types that the user can use to set the data to a value.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming",
        "CA1720:Identifier contains type name",
        Justification = "String is also a Windows Registry type")]
    public enum RegTypes
    {
        /// <summary>
        /// Reserved property. Should not be used directly by your source code.
        /// </summary>
        ERROR = 0,
        /// <summary> RSVD </summary>
        RSVD0 = 1,
        /// <summary> RSVD </summary>
        RSVD1 = 2,
        /// <summary> RSVD </summary>
        RSVD2 = 3,
        /// <summary> RSVD </summary>
        RSVD3 = 4,
        /// <summary> RSVD </summary>
        RSVD4 = 5,
        /// <summary>
        /// The Registry type will be a string value.
        /// </summary>
        String = 6,
        /// <summary>
        /// The Registry type will be an environment variable string value.
        /// </summary>
        ExpandString = 7,
        /// <summary>
        /// The Registry type will be an quad-word byte array value.
        /// </summary>
        QuadWord = 8,
        /// <summary>
        /// The Registry type will be an double-word byte array value.
        /// </summary>
        DoubleWord = 9
    }

    /// <summary>
    /// The <see cref="RegEditor"/> instance class functions result after executing.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming",
        "CA1707:Identifiers should not contain underscores",
        Justification = "Removing underscores from a globally used field could be an breaking API change.")]
    public enum RegFunctionResult
    {
        /// <summary>
        /// Generic error.
        /// </summary>
        Error = 0,
        /// <summary> RSVD </summary>
        RSVD0 = 1,
        /// <summary> RSVD </summary>
        RSVD1 = 2,
        /// <summary>
        /// Incorrect Registry Path.
        /// </summary>
        /// <remarks>This is mostly returned when the path is incorrect or it is an registry error.</remarks>
        Misdefinition_Error = 3,
        /// <summary>
        /// The Root Key provided is invalid.
        /// </summary>
        InvalidRootKey = 4,
        /// <summary>
        /// Sucessfull execution.
        /// </summary>
        Success = 5
    }

    /// <summary>
    /// Valid root paths for the <see cref="RegEditor"/> to modify or create new values.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design",
        "CA1027:Mark enums with FlagsAttribute",
        Justification = "This enumeration represents fixed strings , not a set of bit values.")]
    public enum RegRootKeyValues
    {
        /// <summary>
        /// Reserved property for indicating a custom or unusuable root key.
        /// </summary>
        Inabsolute = 0,
        /// <summary> RSVD </summary>
        RSVD0 = 1,
        /// <summary> RSVD </summary>
        RSVD1 = 2,
        /// <summary> HKLM Path. </summary>
        HKLM = 3,
        /// <summary> HKCU Path. </summary>
        HKCU = 4,
        /// <summary> HKCC Path. </summary>
        HKCC = 5,
        /// <summary> HKPD Path. </summary>
        HKPD = 6,
        /// <summary> HKU Path. </summary>
        HKU = 7,
        /// <summary> HKCR Path. </summary>
        HKCR = 8,
        /// <summary>
        /// This provides the path to the Local Machine.
        /// </summary>
        LocalMachine = HKLM,
        /// <summary>
        /// This is the root path of the current user.
        /// </summary>
        CurrentUser = HKCU,
        /// <summary>
        /// This is the root path of the Current Config key.
        /// </summary>
        /// <remarks>This property is deprecated on the registry; use it with caution.</remarks>
        CurrentConfig = HKCC,
        /// <summary>
        /// This is the root path of the Performance Data key.
        /// </summary>
        /// <remarks>This property is deprecated on the registry; use it with caution.</remarks>
        PerfData = HKPD,
        /// <summary>
        /// This is the root path of the Users Data key.
        /// </summary>
        UsersStore = HKU,
        /// <summary>
        /// This is the root path of the Classes Data Root key.
        /// </summary>
        CurrentClassesRoot = HKCR
    }

    /// <summary>
    /// An easy to use Windows Registry Editor.
    /// </summary>
    [SupportedOSPlatform("windows")]
    public class RegEditor : System.IDisposable
    {
        private System.String _RootKey_;
        private System.String _SubKey_;
        private System.Boolean _DIAG_;

        /// <summary>
        /// The Registry Root Key. It accepts only specific values.
        /// </summary>
        public RegRootKeyValues RootKey
        {
            get
            {
                switch (_RootKey_)
                {
                    case "HKEY_LOCAL_MACHINE": return RegRootKeyValues.HKLM;
                    case "HKEY_CURRENT_USER": return RegRootKeyValues.HKCU;
                    case "HKEY_CURRENT_CONFIG": return RegRootKeyValues.HKCC;
                    case "HKEY_PERFORMANCE_DATA": return RegRootKeyValues.HKPD;
                    case "HKEY_USERS": return RegRootKeyValues.HKU;
                    case "HKEY_CLASSES_ROOT": return RegRootKeyValues.HKCR;
                    default: return RegRootKeyValues.Inabsolute;
                }
            }
            set
            {
                switch (value)
                {
                    case RegRootKeyValues.HKLM: _RootKey_ = "HKEY_LOCAL_MACHINE"; break;
                    case RegRootKeyValues.HKCU: _RootKey_ = "HKEY_CURRENT_USER"; break;
                    case RegRootKeyValues.HKCC: _RootKey_ = "HKEY_CURRENT_CONFIG"; break;
                    case RegRootKeyValues.HKPD: _RootKey_ = "HKEY_PERFORMANCE_DATA"; break;
                    case RegRootKeyValues.HKU: _RootKey_ = "HKEY_USERS"; break;
                    case RegRootKeyValues.HKCR: _RootKey_ = "HKEY_CLASSES_ROOT"; break;
                }
            }
        }

        /// <summary>
        /// The Registry sub-root key. Can be nested the one on the another.
        /// </summary>
        public System.String SubKey
        {
            get { return _SubKey_; }
            set { if (System.String.IsNullOrEmpty(value) == false) { _SubKey_ = value; } }
        }

        /// <summary>
        /// The default , classical and parameterless constructor.
        /// </summary>
        /// <remarks>You must set the required Registry Paths by the respective properties.</remarks>
        public RegEditor() { }

        /// <summary>
        /// Constructor which can be used to set the required Registry Paths on initialisation.
        /// </summary>
        /// <param name="KeyValue">One of the valid Root Keys. See the <see cref="RegRootKeyValues"/> <see cref="System.Enum"/> for more information. </param>
        /// <param name="SubKey">The Registry sub-root key. Can be nested the one on the another.</param>
        public RegEditor(RegRootKeyValues KeyValue, System.String SubKey)
        {
            switch (KeyValue)
            {
                case RegRootKeyValues.HKLM: _RootKey_ = "HKEY_LOCAL_MACHINE"; break;
                case RegRootKeyValues.HKCU: _RootKey_ = "HKEY_CURRENT_USER"; break;
                case RegRootKeyValues.HKCC: _RootKey_ = "HKEY_CURRENT_CONFIG"; break;
                case RegRootKeyValues.HKPD: _RootKey_ = "HKEY_PERFORMANCE_DATA"; break;
                case RegRootKeyValues.HKU: _RootKey_ = "HKEY_USERS"; break;
                case RegRootKeyValues.HKCR: _RootKey_ = "HKEY_CLASSES_ROOT"; break;
            }
            if (System.String.IsNullOrEmpty(SubKey) == false) { _SubKey_ = SubKey; }
        }

        /// <summary>
        /// Enable the Console Diagnostic debugging.
        /// </summary>
        public System.Boolean DiagnosticMessages { set { _DIAG_ = value; } }

        private System.Boolean _CheckPredefinedProperties()
        {
            if ((System.String.IsNullOrEmpty(_RootKey_) == false) &&
                (System.String.IsNullOrEmpty(_SubKey_) == false)) { return true; }
            else { return false; }
        }

        /// <summary>
        /// Gets the specified value from the key provided.
        /// </summary>
        /// <param name="VariableRegistryMember">The value name to retrieve the value data.</param>
        /// <returns>If it succeeded , a new <see cref="System.Object"/> instance containing the data; Otherwise , a <see cref="System.String"/> explaining the error.</returns>
        public System.Object GetEntry(System.String VariableRegistryMember)
        {
            if (System.String.IsNullOrEmpty(VariableRegistryMember)) { return "Error"; }
            if (!_CheckPredefinedProperties())
            {
                if (_DIAG_) { MAIN.WriteConsoleText("Error - Cannot initiate the Internal editor due to an error: Properties that point the searcher are undefined."); }
                return "UNDEF_ERR";
            }
            System.Object RegEntry = Microsoft.Win32.Registry.GetValue($"{_RootKey_}\\{_SubKey_}", VariableRegistryMember, "_ER_C_");
            if (System.Convert.ToString(RegEntry, default) == "_ER_C_") { return "Error"; }
            else
            {
                if (RegEntry is System.String[])
                {
                    return RegEntry;
                }
                else if (RegEntry is System.Byte[])
                {
                    return RegEntry;
                }
                else if (RegEntry is System.String) { return RegEntry; }
                else
                {
                    if (_DIAG_)
                    {
                        MAIN.WriteConsoleText("Error - Could not translate the object returned by the procedure.");
                        MAIN.WriteConsoleText("Please check that the entry is not broken , incorrect or in format that is not supported by this editor.");
                    }
                    return "Error";
                }
            }
        }

        /// <summary>
        /// Sets or creates the specified value.
        /// </summary>
        /// <param name="VariableRegistryMember">The value name whose data will be modified.</param>
        /// <param name="RegistryType">The value type that this value will have. Consult the <see cref="RegTypes"/> <see cref="System.Enum"/> for more information.</param>
        /// <param name="RegistryData">The new data that will be saved on the value; The type is depending upon the <paramref name="RegistryType"/> parameter.</param>
        /// <returns>A new <see cref="RegFunctionResult"/> <see cref="System.Enum"/> explaining if it succeeded.</returns>
        public RegFunctionResult SetEntry(System.String VariableRegistryMember, RegTypes RegistryType, System.Object RegistryData)
        {
            if (System.String.IsNullOrEmpty(VariableRegistryMember))
            {
                return RegFunctionResult.Error;
            }
            if (!_CheckPredefinedProperties())
            {
                if (_DIAG_) { MAIN.WriteConsoleText("Error - Cannot initiate the Internal editor due to an error: Properties that point the searcher are undefined."); }
                return RegFunctionResult.Misdefinition_Error;
            }
            if (RegistryData == null)
            {
                if (_DIAG_) { MAIN.WriteConsoleText("ERROR: 'null' value detected in RegistryData object. Maybe invalid definition?"); }
                return RegFunctionResult.Misdefinition_Error;
            }
            Microsoft.Win32.RegistryValueKind RegType_;
            if (RegistryType == RegTypes.String)
            {
                RegType_ = Microsoft.Win32.RegistryValueKind.String;
            }
            else if (RegistryType == RegTypes.ExpandString)
            {
                RegType_ = Microsoft.Win32.RegistryValueKind.ExpandString;
            }
            else if (RegistryType == RegTypes.QuadWord)
            {
                RegType_ = Microsoft.Win32.RegistryValueKind.QWord;
            }
            else if (RegistryType == RegTypes.DoubleWord)
            {
                RegType_ = Microsoft.Win32.RegistryValueKind.DWord;
            }
            else
            {
                if (_DIAG_)
                {
                    MAIN.WriteConsoleText($"ERROR: Unknown registry value type argument in the object creator was given: {RegistryType}");
                }
                return RegFunctionResult.InvalidRootKey;
            }
            try
            {
                Microsoft.Win32.Registry.SetValue($"{_RootKey_}\\{_SubKey_}", VariableRegistryMember, RegistryData, RegType_);
            }
            catch (System.Exception EX)
            {
                if (_DIAG_)
                {
                    MAIN.WriteConsoleText($"ERROR: Could not create key {VariableRegistryMember} . Invalid name maybe?");
                    MAIN.WriteConsoleText($"Error Raw Data: {EX}");
                }
                return RegFunctionResult.Error;
            }
            return RegFunctionResult.Success;
        }

        /// <summary>
        /// Deletes the specified value from the registry.
        /// </summary>
        /// <param name="VariableRegistryMember">The value which will be deleted.</param>
        /// <returns>A new <see cref="RegFunctionResult"/> <see cref="System.Enum"/> explaining if it succeeded.</returns>
        public RegFunctionResult DeleteEntry(System.String VariableRegistryMember)
        {
            if (System.String.IsNullOrEmpty(VariableRegistryMember)) { return RegFunctionResult.Error; }
            if (!_CheckPredefinedProperties())
            {
                if (_DIAG_) { MAIN.WriteConsoleText("Error - Cannot initiate the Internal editor due to an error: Properties that point the searcher are undefined."); }
                return RegFunctionResult.Misdefinition_Error;
            }
            Microsoft.Win32.RegistryKey ValueDelete;
            switch (_RootKey_)
            {
                case "HKEY_LOCAL_MACHINE":
                    ValueDelete = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(_SubKey_);
                    break;
                case "HKEY_CURRENT_USER":
                    ValueDelete = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(_SubKey_);
                    break;
                case "HKEY_CURRENT_CONFIG":
                    ValueDelete = Microsoft.Win32.Registry.CurrentConfig.OpenSubKey(_SubKey_);
                    break;
                case "HKEY_PERFORMANCE_DATA":
                    ValueDelete = Microsoft.Win32.Registry.PerformanceData.OpenSubKey(_SubKey_);
                    break;
                case "HKEY_USERS":
                    ValueDelete = Microsoft.Win32.Registry.Users.OpenSubKey(_SubKey_);
                    break;
                case "HKEY_CLASSES_ROOT":
                    ValueDelete = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(_SubKey_);
                    break;
                default:
                    if (_DIAG_)
                    {
                        MAIN.WriteConsoleText("Error - Registry root key could not be get. Incorrect Root Key Detected.");
                        MAIN.WriteConsoleText("Error while getting the root key: Root Key " + _RootKey_ + "Is invalid.");
                    }
                    return RegFunctionResult.Misdefinition_Error;
            }
            if (System.Convert.ToString(ValueDelete.GetValue(VariableRegistryMember, "_DNE_")) == "_DNE_")
            {
                ValueDelete.Close();
                return RegFunctionResult.Error;
            }
            ValueDelete.DeleteValue(VariableRegistryMember);
            ValueDelete.Close();
            return RegFunctionResult.Success;
        }

        /// <summary>
        /// Creates a new key inside a sub-key or the root key.
        /// </summary>
        /// <param name="KeyName">The sub-key to create. If this parameter is not defined , 
        /// then it will create the sub-key name defined in the <see cref="SubKey"/> property.</param>
        /// <returns>A new <see cref="RegFunctionResult"/> enumeration , which indicates success or not.</returns>
        public RegFunctionResult CreateNewKey(System.String KeyName = "")
        {
            if (System.String.IsNullOrEmpty(KeyName)) { KeyName = _SubKey_; }
            Microsoft.Win32.RegistryKey ValueCreate;
            try
            {
                switch (_RootKey_)
                {
                    case "HKEY_LOCAL_MACHINE":
                        ValueCreate = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(KeyName);
                        break;
                    case "HKEY_CURRENT_USER":
                        ValueCreate = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(KeyName);
                        break;
                    case "HKEY_CURRENT_CONFIG":
                        ValueCreate = Microsoft.Win32.Registry.CurrentConfig.CreateSubKey(KeyName);
                        break;
                    case "HKEY_PERFORMANCE_DATA":
                        ValueCreate = Microsoft.Win32.Registry.PerformanceData.CreateSubKey(KeyName);
                        break;
                    case "HKEY_USERS":
                        ValueCreate = Microsoft.Win32.Registry.Users.CreateSubKey(KeyName);
                        break;
                    case "HKEY_CLASSES_ROOT":
                        ValueCreate = Microsoft.Win32.Registry.ClassesRoot.CreateSubKey(KeyName);
                        break;
                    default:
                        if (_DIAG_)
                        {
                            MAIN.WriteConsoleText("Error - Registry root key could not be get. Incorrect Root Key Detected.");
                            MAIN.WriteConsoleText("Error while getting the root key: Root Key " + _RootKey_ + "Is invalid.");
                        }
                        return RegFunctionResult.Misdefinition_Error;
                }
                ValueCreate.Flush();
                ValueCreate.Close();
            }
            catch (Exception) { return RegFunctionResult.Error; }
            return RegFunctionResult.Success;
        }

        /// <summary>
        /// Determines whether a specified registry key exists , it's path depends by the <see cref="RootKey"/>
        /// property and the <paramref name="KeyName"/> parameter.
        /// </summary>
        /// <param name="KeyName">The sub-key to find. If this parameter is not defined , 
        /// then it will find the sub-key name defined in the <see cref="SubKey"/> property.</param>
        /// <returns><see langword="true"/> if the key exists , otherwise <see langword="false"/>. </returns>
        public System.Boolean KeyExists(System.String KeyName = "")
        {
            if (System.String.IsNullOrEmpty(KeyName)) { KeyName = _SubKey_; }
            Microsoft.Win32.RegistryKey ValueFind;
            try
            {
                switch (_RootKey_)
                {
                    case "HKEY_LOCAL_MACHINE":
                        ValueFind = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(KeyName);
                        break;
                    case "HKEY_CURRENT_USER":
                        ValueFind = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(KeyName);
                        break;
                    case "HKEY_CURRENT_CONFIG":
                        ValueFind = Microsoft.Win32.Registry.CurrentConfig.OpenSubKey(KeyName);
                        break;
                    case "HKEY_PERFORMANCE_DATA":
                        ValueFind = Microsoft.Win32.Registry.PerformanceData.OpenSubKey(KeyName);
                        break;
                    case "HKEY_USERS":
                        ValueFind = Microsoft.Win32.Registry.Users.OpenSubKey(KeyName);
                        break;
                    case "HKEY_CLASSES_ROOT":
                        ValueFind = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(KeyName);
                        break;
                    default:
                        return false;
                }
                ValueFind.Close();
            }
            catch (Exception) { return false; }
            return true;
        }

        /// <summary>
        /// Use this Dispose method to clear up the current key that the class is working on and make it possible to set a new path to work on.
        /// </summary>
        public void Dispose() { Dispose(true); GC.SuppressFinalize(this); }

        /// <summary>
        /// This Dispose method does the same thing as the parameterless <see cref="Dispose()"/> , 
        /// but has the option for a boolean to dispose all resources.
        /// Mainly provided for derivation reasons.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(System.Boolean disposing) { DisposeRes(); }

        private protected void DisposeRes()
        {
            // Delete any unused values.
            _RootKey_ = null;
            _SubKey_ = null;
            _DIAG_ = false;
        }
    }

    /// <summary>
    /// The exception that it is thrown when an unexpected result in an executing code block was found.
    /// </summary>
    public class ExecutionException : System.Exception
    {
        /// <summary>
        /// Creates a new instance of <see cref="ExecutionException"/> class.
        /// </summary>
        public ExecutionException() : base() { }

        /// <summary>
        /// Creates a new instance of <see cref="ExecutionException"/> class with the specified error message.
        /// </summary>
        /// <param name="message">The error message to show.</param>
        public ExecutionException(System.String message) : base(message) { }

        /// <summary>
        /// Creates a new instance of <see cref="ExecutionException"/> class with the specified error message
        /// and the <see cref="Exception"/> that caused this exception.
        /// </summary>
        /// <param name="message">The error message to show.</param>
        /// <param name="InnerException">The inner exception that it is the root cause of this exception.</param>
        public ExecutionException(System.String message , Exception InnerException) : base(message , InnerException) { }
    }

    /// <summary>
    /// The exception that is thrown when a native P/Invoke call failed to give correct results.
    /// </summary>
    public sealed class NativeCallErrorException : ExecutionException
    {
        /// <summary>
        /// Creates a new instance of <see cref="NativeCallErrorException"/> class.
        /// </summary>
        public NativeCallErrorException() : base() { }

        /// <summary>
        /// Creates a new instance of <see cref="NativeCallErrorException"/> 
        /// class with the specified error code.
        /// </summary>
        public NativeCallErrorException(System.Int64 code) : base() { ErrorCode = code; }

        /// <summary>
        /// Creates a new instance of <see cref="NativeCallErrorException"/> class with the specified error message.
        /// </summary>
        /// <param name="message">The error message to show.</param>
        public NativeCallErrorException(System.String message) : base(message) { }

        /// <summary>
        /// Creates a new instance of <see cref="NativeCallErrorException"/> class with the specified
        /// native error code and error message.
        /// </summary>
        /// <param name="message">The error message to show.</param>
        /// <param name="code">The error code that caused this exception.</param>
        public NativeCallErrorException(System.Int64 code , System.String message) : base(message + $"\nError Code: {code}") { ErrorCode = code; }

        /// <summary>
        /// Creates a new instance of <see cref="NativeCallErrorException"/> class with the specified error message
        /// and the <see cref="Exception"/> that caused this exception.
        /// </summary>
        /// <param name="message">The error message to show.</param>
        /// <param name="InnerException">The inner exception that it is the root cause of this exception.</param>
        public NativeCallErrorException(System.String message, Exception InnerException) : base(message, InnerException) { }

        /// <summary>
        /// Creates a new instance of <see cref="NativeCallErrorException"/> class with the specified error message
        /// the native error code , and the <see cref="Exception"/> that caused this exception.
        /// </summary>
        /// <param name="message">The error message to show.</param>
        /// <param name="code">The error code that caused this exception.</param>
        /// <param name="InnerException">The inner exception that it is the root cause of this exception.</param>
        public NativeCallErrorException(System.Int64 code, System.String message, Exception InnerException) : base(message + $"\nError Code: {code}", InnerException) { ErrorCode = code; }

        /// <summary>
        /// The error code of the native call , if it is available.
        /// </summary>
        public System.Int64 ErrorCode { get; private set; }
    }

    /// <summary>
    /// Contains static methods so as to get information on the last native Windows error code.
    /// </summary>
    [SupportedOSPlatform("windows")]
    public static class WindowsErrorCodes
    {
        [Flags]
        [Serializable]
        private enum FormatMsg_Flags : System.UInt32
        {
            FORMAT_MESSAGE_NO_RESTRICTIONS = 0,
            FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x00000100,
            FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x00002000,
            FORMAT_MESSAGE_FROM_HMODULE = 0x00000800,
            FORMAT_MESSAGE_FROM_STRING = 0x00000400,
            FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000,
            FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200,
            FORMAT_MESSAGE_MAX_WIDTH_MASK = 0x000000FF
        }

        [Flags]
        [Serializable]
        private enum FormatMsg_SourceType : System.UInt32
        {
            None = 0,
            FORMAT_MESSAGE_FROM_HMODULE = 0x00000800,
            FORMAT_MESSAGE_FROM_STRING = 0x00000400
        }

        [DllImport(Interop.Libraries.Kernel32, CallingConvention = CallingConvention.Winapi,
            CharSet = CharSet.Unicode, EntryPoint = "FormatMessageW")]
        private static extern System.UInt32 GetString(
            [In] FormatMsg_Flags Flags,
            [In] [Optional] FormatMsg_SourceType Source,
            [In] System.UInt32 MessageID,
            [In] System.UInt32 LanguageID,
            [Out] [MarshalAs(UnmanagedType.LPWStr)] out System.String Buffer,
            [In] System.UInt32 Input,
            [In] [Optional] System.String Arguments);

        [DllImport(Interop.Libraries.Kernel32 , CallingConvention = CallingConvention.Winapi ,
            CharSet = CharSet.Auto , EntryPoint = "GetLastError")]
        private static extern System.UInt32 LastWinErrorCode();

        /// <summary>
        /// Gets the last Windows native error code.
        /// </summary>
        /// <remarks>
        /// Note: This error code is only specific for the thread it was called on. <br />
        /// To get the error code for another thread , use this property on that thread.
        /// </remarks>
        public static System.UInt32 LastErrorCode 
        {
            [System.Security.SecurityCritical]
            [System.Security.SuppressUnmanagedCodeSecurity]
            get { return LastWinErrorCode(); } 
        }

        /// <summary>
        /// Throws an <see cref="NativeCallErrorException"/> from the given Win32 error code. <br />
        /// If the <paramref name="code"/> parameter is 0 , it just returns without throwing anything.
        /// </summary>
        /// <param name="code">The Win32 error code.</param>
        /// <param name="Msg">An optional message to provide. This parameter is optional.</param>
        [System.Security.SecurityCritical]
        [System.Security.SuppressUnmanagedCodeSecurity]
        public static void ThrowException(System.UInt32 code, System.String Msg = null)
        {
            if (code == 0) { return; }
            if (Msg != null) { Msg += "\n"; } else { Msg = System.String.Empty; }
            throw new NativeCallErrorException(code , Msg + GetErrorStringFromWin32Code(code));
        }

        /// <summary>
        /// Gets an error string from a given Win32 Error Code provided.
        /// </summary>
        /// <param name="code">The Win32 error code.</param>
        /// <returns>A <see cref="System.String"/> describing the error code.</returns>
        /// <exception cref="System.InvalidOperationException"><paramref name="code"/> was 0.</exception>
        [System.Security.SecurityCritical]
        [System.Security.SuppressUnmanagedCodeSecurity]
        public static System.String GetErrorStringFromWin32Code(System.UInt32 code)
        {
            if (code <= 0) { throw new System.InvalidOperationException("Error code 0 shows canonical operation , nothing to show."); }
            System.Char[] D = new System.Char[700];
            System.String retval = new(D);
            GetString(FormatMsg_Flags.FORMAT_MESSAGE_ALLOCATE_BUFFER |
                FormatMsg_Flags.FORMAT_MESSAGE_FROM_SYSTEM |
                FormatMsg_Flags.FORMAT_MESSAGE_IGNORE_INSERTS,
                FormatMsg_SourceType.None, code, 0, out retval, (System.UInt32)retval.Length);
            D = null;
            return retval;
        }

        /// <summary>
        /// Gets a string description from the last error code.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">
        /// No error was found , the property cannot return anything.
        /// </exception>
        public static System.String LastErrorString 
        {
            [System.Security.SecurityCritical]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            [System.Security.SuppressUnmanagedCodeSecurity]
            get { return GetErrorStringFromWin32Code(LastErrorCode); }
        }
    }

}

[Serializable]
internal enum ProcessInterop_Memory_Priority_Levels : System.UInt32
{
    [NonSerialized]
    None = 0,
    MEMORY_PRIORITY_VERY_LOW,
    MEMORY_PRIORITY_LOW,
    MEMORY_PRIORITY_MEDIUM,
    MEMORY_PRIORITY_BELOW_NORMAL,
    MEMORY_PRIORITY_NORMAL
}

[StructLayout(LayoutKind.Sequential, Size = 32)]
internal struct ProcessInterop_Memory_Priority_Info
{
    [MarshalAs(UnmanagedType.U4)]
    public ProcessInterop_Memory_Priority_Levels MemoryPriority;
}