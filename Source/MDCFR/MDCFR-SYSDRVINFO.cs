
using System;
using System.Text;
using System.Security;
using System.Runtime.InteropServices;

namespace Microsoft.IO
{
    internal static class Error
    {
        [SecuritySafeCritical]
        internal static Exception GetExceptionForLastWin32DriveError(string driveName)
        { return GetExceptionForWin32DriveError(Marshal.GetLastWin32Error(), driveName); }

        [SecurityCritical]
        internal static Exception GetExceptionForWin32DriveError(int errorCode, string driveName)
        {
            if (errorCode == 3 || errorCode == 15)
            {
                return new DriveNotFoundException(SR.Format(MDCFR.Properties.Resources.IO_DriveNotFound_Drive, driveName));
            }
            return System.IO.Win32Marshal.GetExceptionForWin32Error(errorCode, driveName);
        }
    }

    /// <summary>
    /// The exception that it is thrown when a drive was not found.
    /// </summary>
    public class DriveNotFoundException : System.IO.IOException
    {
        /// <summary>
        /// Creates a new instance of <see cref="DriveNotFoundException"/>.
        /// </summary>
        public DriveNotFoundException()
            : base(MDCFR.Properties.Resources.IO_DriveNotFound)
        { base.HResult = -2147024893; }

        /// <summary>
        /// Creates a new instance of <see cref="DriveNotFoundException"/> with the specified message.
        /// </summary>
        /// <param name="message">The message that the exception will use.</param>
        public DriveNotFoundException(string message)
            : base(message) { base.HResult = -2147024893; }

        /// <summary>
        /// Creates a new instance of <see cref="DriveNotFoundException"/> 
        /// with the specified message and the exception that caused this exception.
        /// </summary>
        /// <param name="message">The message that the exception will use.</param>
        /// <param name="innerException">The exception that caused this <see cref="DriveNotFoundException"/>.</param>
        public DriveNotFoundException(string message, Exception innerException)
            : base(message, innerException) { base.HResult = -2147024893; }
    }

    /// <summary>
    /// Specifies the drive type that a drive is.
    /// </summary>
    [Serializable]
    public enum DriveType
    {
        /// <summary>
        /// The drive type is unknown.
        /// </summary>
        Unknown,
        /// <summary>
        /// The drive has not been assigned a letter.
        /// </summary>
        NoRootDirectory,
        /// <summary>
        /// The drive is a removable USB drive.
        /// </summary>
        Removable,
        /// <summary>
        /// The drive has a fixed capacity size.
        /// </summary>
        Fixed,
        /// <summary>
        /// The drive is a NAS drive or any other means of network drive.
        /// </summary>
        Network,
        /// <summary>
        /// The drive is a Compact Disc Read Only Memory disc.
        /// </summary>
        CDRom,
        /// <summary>
        /// The drive is the computer's Random Access Memory (RAM).
        /// </summary>
        Ram
    }

    /// <summary>
    /// The Drive Information Class shows information about a logical drive.
    /// </summary>
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public sealed class DriveInfo
    {
        private readonly string _name;

        /// <summary>
        /// Gets the drive name that this instance represents.
        /// </summary>
        public string Name => _name;

        /// <summary>
        /// Gets a value whether the drive that this instance represents is ready-for-use.
        /// </summary>
        public bool IsReady => System.IO.Directory.Exists(Name);

        /// <summary>
        /// Gets the root directory of the drive that this instance represents.
        /// </summary>
        public System.IO.DirectoryInfo RootDirectory => new System.IO.DirectoryInfo(Name);

        /// <summary>
        /// Gets the drive type for this instance.
        /// </summary>
        public DriveType DriveType
        {
            [SecuritySafeCritical] 
            get { return (DriveType)Interop.Mincore.GetDriveType(Name); }
        }

        /// <summary>
        /// Gets the format of the drive. Some of the most known file formats for drive are NTFS , FAT32 , Ext4 , HFS , etc...
        /// </summary>
        public string DriveFormat
        {
            [SecuritySafeCritical]
            get
            {
                StringBuilder volumeName = new StringBuilder(50);
                StringBuilder stringBuilder = new StringBuilder(50);
                uint errorMode = Interop.Mincore.SetErrorMode(1u);
                try
                {
                    if (!Interop.Mincore.GetVolumeInformation(Name, volumeName, 50, out var _, out var _, out var _, stringBuilder, 50))
                    {
                        throw Error.GetExceptionForLastWin32DriveError(Name);
                    }
                }
                finally
                {
                    Interop.Mincore.SetErrorMode(errorMode);
                }
                return stringBuilder.ToString();
            }
        }

        /// <summary>
        /// Computes the available drive space.
        /// </summary>
        public long AvailableFreeSpace
        {
            [SecuritySafeCritical]
            get
            {
                uint errorMode = Interop.Mincore.SetErrorMode(1u);
                try
                {
                    if (!Interop.Mincore.GetDiskFreeSpaceEx(Name, out var freeBytesForUser, out var _, out var _))
                    {
                        throw Error.GetExceptionForLastWin32DriveError(Name);
                    }
                    return freeBytesForUser;
                }
                finally
                {
                    Interop.Mincore.SetErrorMode(errorMode);
                }
            }
        }

        /// <summary>
        /// Computes the absolute available drive space.
        /// </summary>
        public long TotalFreeSpace
        {
            [SecuritySafeCritical]
            get
            {
                uint errorMode = Interop.Mincore.SetErrorMode(1u);
                try
                {
                    if (!Interop.Mincore.GetDiskFreeSpaceEx(Name, out var _, out var _, out var freeBytes))
                    {
                        throw Error.GetExceptionForLastWin32DriveError(Name);
                    }
                    return freeBytes;
                }
                finally
                {
                    Interop.Mincore.SetErrorMode(errorMode);
                }
            }
        }

        /// <summary>
        /// Gets the drive's total size.
        /// </summary>
        public long TotalSize
        {
            [SecuritySafeCritical]
            get
            {
                uint errorMode = Interop.Mincore.SetErrorMode(1u);
                try
                {
                    if (!Interop.Mincore.GetDiskFreeSpaceEx(Name, out var _, out var totalBytes, out var _))
                    {
                        throw Error.GetExceptionForLastWin32DriveError(Name);
                    }
                    return totalBytes;
                }
                finally
                {
                    Interop.Mincore.SetErrorMode(errorMode);
                }
            }
        }

        /// <summary>
        /// Gets the drive friendly name (also known as the drive volume label).
        /// </summary>
        public string VolumeLabel
        {
            [SecuritySafeCritical]
            get
            {
                StringBuilder stringBuilder = new StringBuilder(50);
                StringBuilder fileSystemName = new StringBuilder(50);
                uint errorMode = Interop.Mincore.SetErrorMode(1u);
                try
                {
                    if (!Interop.Mincore.GetVolumeInformation(Name, stringBuilder, 50, out var _, out var _, out var _, fileSystemName, 50))
                    {
                        int num = Marshal.GetLastWin32Error();
                        if (num == 13)
                        {
                            num = 15;
                        }
                        throw Error.GetExceptionForWin32DriveError(num, Name);
                    }
                }
                finally
                {
                    Interop.Mincore.SetErrorMode(errorMode);
                }
                return stringBuilder.ToString();
            }
            [SecuritySafeCritical]
            set
            {
                uint errorMode = Interop.Mincore.SetErrorMode(1u);
                try
                {
                    if (!Interop.Mincore.SetVolumeLabel(Name, value))
                    {
                        int lastWin32Error = Marshal.GetLastWin32Error();
                        if (lastWin32Error == 5)
                        {
                            throw new UnauthorizedAccessException(MDCFR.Properties.Resources.InvalidOperation_SetVolumeLabelFailed);
                        }
                        throw Error.GetExceptionForWin32DriveError(lastWin32Error, Name);
                    }
                }
                finally
                {
                    Interop.Mincore.SetErrorMode(errorMode);
                }
            }
        }

        /// <summary>
        /// Creates a new instance of <see cref="DriveInfo"/> class 
        /// from a specified drive name.
        /// </summary>
        /// <param name="driveName">The drive name to create the instance for.</param>
        /// <exception cref="ArgumentNullException"><paramref name="driveName"/> is null.</exception>
        public DriveInfo(string driveName)
        {
            if (driveName == null)
            {
                throw new ArgumentNullException("driveName");
            }
            _name = NormalizeDriveName(driveName);
        }

        /// <summary>
        /// Returns the drive name that this instance represents.
        /// </summary>
        /// <returns>The drive name that this instance represents.</returns>
        public override string ToString() { return _name; }

        internal static bool HasIllegalCharacters(string path)
        {
            foreach (char c in path)
            {
                if (c <= '\u001f') { return true; }
                switch (c)
                {
                    case '"':
                    case '<':
                    case '>':
                    case '|':
                        return true;
                }
            }
            return false;
        }

        private static string NormalizeDriveName(string driveName)
        {
            string text;
            if (driveName.Length == 1) { text = driveName + ":\\"; }
            else
            {
                if (HasIllegalCharacters(driveName))
                {
                    throw new ArgumentException(SR.Format(MDCFR.Properties.Resources.Arg_InvalidDriveChars, driveName), "driveName");
                }
                text = System.IO.Path.GetPathRoot(driveName);
                if (text == null || text.Length == 0 || text.StartsWith("\\\\", StringComparison.Ordinal))
                {
                    throw new ArgumentException(MDCFR.Properties.Resources.Arg_MustBeDriveLetterOrRootDir);
                }
            }
            if (text.Length == 2 && text[1] == ':')
            {
                text += "\\";
            }
            char c = driveName[0];
            if ((c < 'A' || c > 'Z') && (c < 'a' || c > 'z'))
            {
                throw new ArgumentException(MDCFR.Properties.Resources.Arg_MustBeDriveLetterOrRootDir);
            }
            return text;
        }

        /// <summary>
        /// Get an array of all logical drives existing in the computer.
        /// </summary>
        /// <returns>A new <see cref="DriveInfo"/> array containing the logical drives found in the computer.</returns>
        public static DriveInfo[] GetDrives()
        {
            int logicalDrives = Interop.Mincore.GetLogicalDrives();
            if (logicalDrives == 0)
            {
                throw System.IO.Win32Marshal.GetExceptionForLastWin32Error();
            }
            uint num = (uint)logicalDrives;
            int num2 = 0;
            while (num != 0)
            {
                if ((num & (true ? 1u : 0u)) != 0)
                {
                    num2++;
                }
                num >>= 1;
            }
            DriveInfo[] array = new DriveInfo[num2];
            char[] array2 = new char[3] { 'A', ':', '\\' };
            num = (uint)logicalDrives;
            num2 = 0;
            while (num != 0)
            {
                if ((num & (true ? 1u : 0u)) != 0)
                {
                    array[num2++] = new DriveInfo(new string(array2));
                }
                num >>= 1;
                array2[0] += '\u0001';
            }
            return array;
        }
    }

}

internal partial class Interop
{
    internal class Mincore
    {
        internal const uint SEM_FAILCRITICALERRORS = 1u;

        private const int FORMAT_MESSAGE_IGNORE_INSERTS = 512;

        private const int FORMAT_MESSAGE_FROM_HMODULE = 2048;

        private const int FORMAT_MESSAGE_FROM_SYSTEM = 4096;

        private const int FORMAT_MESSAGE_ARGUMENT_ARRAY = 8192;

        private const int ERROR_INSUFFICIENT_BUFFER = 122;

        private const int InitialBufferSize = 256;

        private const int BufferSizeIncreaseFactor = 4;

        private const int MaxAllowedBufferSize = 66560;

        [DllImport(Libraries.CoreFile_L1, BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "GetDriveTypeW", SetLastError = true)]
        internal static extern int GetDriveType(string drive);

        [DllImport(Libraries.CoreFile_L1, BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "GetVolumeInformationW", SetLastError = true)]
        internal static extern bool GetVolumeInformation(string drive, [Out] StringBuilder volumeName, int volumeNameBufLen, out int volSerialNumber, out int maxFileNameLen, out int fileSystemFlags, [Out] StringBuilder fileSystemName, int fileSystemNameBufLen);

        [DllImport(Libraries.CoreFile_L1, SetLastError = true)]
        internal static extern int GetLogicalDrives();

        [DllImport(Libraries.CoreFile_L1, BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "GetDiskFreeSpaceExW", SetLastError = true)]
        internal static extern bool GetDiskFreeSpaceEx(string drive, out long freeBytesForUser, out long totalBytes, out long freeBytes);

        [DllImport(Libraries.Kernel32_L2, BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "SetVolumeLabelW", SetLastError = true)]
        internal static extern bool SetVolumeLabel(string driveLetter, string volumeName);

        [DllImport(Libraries.ErrorHandling, ExactSpelling = true)]
        internal static extern uint SetErrorMode(uint newMode);

        [DllImport(Libraries.Localization, BestFitMapping = true, CharSet = CharSet.Unicode, EntryPoint = "FormatMessageW", SetLastError = true)]
        private static extern int FormatMessage(int dwFlags, IntPtr lpSource, uint dwMessageId, int dwLanguageId, [Out] StringBuilder lpBuffer, int nSize, IntPtr[] arguments);

        internal static string GetMessage(int errorCode)
        {
            return GetMessage(IntPtr.Zero, errorCode);
        }

        internal static string GetMessage(IntPtr moduleHandle, int errorCode)
        {
            StringBuilder stringBuilder = new StringBuilder(256);
            do
            {
                if (TryGetErrorMessage(moduleHandle, errorCode, stringBuilder, out var errorMsg))
                {
                    return errorMsg;
                }
                stringBuilder.Capacity *= 4;
            }
            while (stringBuilder.Capacity < 66560);
            return $"Unknown error (0x{errorCode:x})";
        }

        private static bool TryGetErrorMessage(IntPtr moduleHandle, int errorCode, StringBuilder sb, out string errorMsg)
        {
            errorMsg = "";
            int num = 12800;
            if (moduleHandle != IntPtr.Zero)
            {
                num |= 0x800;
            }
            if (FormatMessage(num, moduleHandle, (uint)errorCode, 0, sb, sb.Capacity, null) != 0)
            {
                int num2;
                for (num2 = sb.Length; num2 > 0; num2--)
                {
                    char c = sb[num2 - 1];
                    if (c > ' ' && c != '.')
                    {
                        break;
                    }
                }
                errorMsg = sb.ToString(0, num2);
            }
            else
            {
                if (Marshal.GetLastWin32Error() == 122)
                {
                    return false;
                }
                errorMsg = $"Unknown error (0x{errorCode:x})";
            }
            return true;
        }
    }
}