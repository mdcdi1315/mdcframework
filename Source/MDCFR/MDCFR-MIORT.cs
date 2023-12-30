
using System;
using System.IO;
using System.Text;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.Versioning;
using System.Collections.Generic;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;

#nullable enable

namespace Microsoft.IO
{
    using Microsoft.IO.Enumeration;
    using System.Runtime.Serialization;

    public static class Directory
    {
        public static DirectoryInfo? GetParent(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (path.Length == 0)
            {
                throw new ArgumentException(MDCFR.Properties.Resources.Argument_PathEmpty, "path");
            }
            string fullPath = Path.GetFullPath(path);
            string directoryName = Path.GetDirectoryName(fullPath);
            if (directoryName == null)
            {
                return null;
            }
            return new DirectoryInfo(directoryName);
        }

        public static DirectoryInfo CreateDirectory(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (path.Length == 0)
            {
                throw new ArgumentException(MDCFR.Properties.Resources.Argument_PathEmpty, "path");
            }
            string fullPath = Path.GetFullPath(path);
            FileSystem.CreateDirectory(fullPath);
            return new DirectoryInfo(path, fullPath, null, isNormalized: true);
        }

        public static bool Exists([NotNullWhen(true)] string? path)
        {
            try
            {
                if (path == null)
                {
                    return false;
                }
                if (path.Length == 0)
                {
                    return false;
                }
                string fullPath = Path.GetFullPath(path);
                return FileSystem.DirectoryExists(fullPath);
            }
            catch (ArgumentException)
            {
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
            return false;
        }

        public static void SetCreationTime(string path, DateTime creationTime)
        {
            string fullPath = Path.GetFullPath(path);
            FileSystem.SetCreationTime(fullPath, creationTime, asDirectory: true);
        }

        public static void SetCreationTimeUtc(string path, DateTime creationTimeUtc)
        {
            string fullPath = Path.GetFullPath(path);
            FileSystem.SetCreationTime(fullPath, File.GetUtcDateTimeOffset(creationTimeUtc), asDirectory: true);
        }

        public static DateTime GetCreationTime(string path)
        {
            return File.GetCreationTime(path);
        }

        public static DateTime GetCreationTimeUtc(string path)
        {
            return File.GetCreationTimeUtc(path);
        }

        public static void SetLastWriteTime(string path, DateTime lastWriteTime)
        {
            string fullPath = Path.GetFullPath(path);
            FileSystem.SetLastWriteTime(fullPath, lastWriteTime, asDirectory: true);
        }

        public static void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
        {
            string fullPath = Path.GetFullPath(path);
            FileSystem.SetLastWriteTime(fullPath, File.GetUtcDateTimeOffset(lastWriteTimeUtc), asDirectory: true);
        }

        public static DateTime GetLastWriteTime(string path)
        {
            return File.GetLastWriteTime(path);
        }

        public static DateTime GetLastWriteTimeUtc(string path)
        {
            return File.GetLastWriteTimeUtc(path);
        }

        public static void SetLastAccessTime(string path, DateTime lastAccessTime)
        {
            string fullPath = Path.GetFullPath(path);
            FileSystem.SetLastAccessTime(fullPath, lastAccessTime, asDirectory: true);
        }

        public static void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc)
        {
            string fullPath = Path.GetFullPath(path);
            FileSystem.SetLastAccessTime(fullPath, File.GetUtcDateTimeOffset(lastAccessTimeUtc), asDirectory: true);
        }

        public static DateTime GetLastAccessTime(string path)
        {
            return File.GetLastAccessTime(path);
        }

        public static DateTime GetLastAccessTimeUtc(string path)
        {
            return File.GetLastAccessTimeUtc(path);
        }

        public static string[] GetFiles(string path)
        {
            return GetFiles(path, "*", EnumerationOptions.Compatible);
        }

        public static string[] GetFiles(string path, string searchPattern)
        {
            return GetFiles(path, searchPattern, EnumerationOptions.Compatible);
        }

        public static string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
        {
            return GetFiles(path, searchPattern, EnumerationOptions.FromSearchOption(searchOption));
        }

        public static string[] GetFiles(string path, string searchPattern, EnumerationOptions enumerationOptions)
        {
            return new List<string>(InternalEnumeratePaths(path, searchPattern, SearchTarget.Files, enumerationOptions)).ToArray();
        }

        public static string[] GetDirectories(string path)
        {
            return GetDirectories(path, "*", EnumerationOptions.Compatible);
        }

        public static string[] GetDirectories(string path, string searchPattern)
        {
            return GetDirectories(path, searchPattern, EnumerationOptions.Compatible);
        }

        public static string[] GetDirectories(string path, string searchPattern, SearchOption searchOption)
        {
            return GetDirectories(path, searchPattern, EnumerationOptions.FromSearchOption(searchOption));
        }

        public static string[] GetDirectories(string path, string searchPattern, EnumerationOptions enumerationOptions)
        {
            return new List<string>(InternalEnumeratePaths(path, searchPattern, SearchTarget.Directories, enumerationOptions)).ToArray();
        }

        public static string[] GetFileSystemEntries(string path)
        {
            return GetFileSystemEntries(path, "*", EnumerationOptions.Compatible);
        }

        public static string[] GetFileSystemEntries(string path, string searchPattern)
        {
            return GetFileSystemEntries(path, searchPattern, EnumerationOptions.Compatible);
        }

        public static string[] GetFileSystemEntries(string path, string searchPattern, SearchOption searchOption)
        {
            return GetFileSystemEntries(path, searchPattern, EnumerationOptions.FromSearchOption(searchOption));
        }

        public static string[] GetFileSystemEntries(string path, string searchPattern, EnumerationOptions enumerationOptions)
        {
            return new List<string>(InternalEnumeratePaths(path, searchPattern, SearchTarget.Both, enumerationOptions)).ToArray();
        }

        internal static IEnumerable<string> InternalEnumeratePaths(string path, string searchPattern, SearchTarget searchTarget, EnumerationOptions options)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (searchPattern == null)
            {
                throw new ArgumentNullException("searchPattern");
            }
            FileSystemEnumerableFactory.NormalizeInputs(ref path, ref searchPattern, options.MatchType);
            return searchTarget switch
            {
                SearchTarget.Files => FileSystemEnumerableFactory.UserFiles(path, searchPattern, options),
                SearchTarget.Directories => FileSystemEnumerableFactory.UserDirectories(path, searchPattern, options),
                SearchTarget.Both => FileSystemEnumerableFactory.UserEntries(path, searchPattern, options),
                _ => throw new ArgumentOutOfRangeException("searchTarget"),
            };
        }

        public static IEnumerable<string> EnumerateDirectories(string path)
        {
            return EnumerateDirectories(path, "*", EnumerationOptions.Compatible);
        }

        public static IEnumerable<string> EnumerateDirectories(string path, string searchPattern)
        {
            return EnumerateDirectories(path, searchPattern, EnumerationOptions.Compatible);
        }

        public static IEnumerable<string> EnumerateDirectories(string path, string searchPattern, SearchOption searchOption)
        {
            return EnumerateDirectories(path, searchPattern, EnumerationOptions.FromSearchOption(searchOption));
        }

        public static IEnumerable<string> EnumerateDirectories(string path, string searchPattern, EnumerationOptions enumerationOptions)
        {
            return InternalEnumeratePaths(path, searchPattern, SearchTarget.Directories, enumerationOptions);
        }

        public static IEnumerable<string> EnumerateFiles(string path)
        {
            return EnumerateFiles(path, "*", EnumerationOptions.Compatible);
        }

        public static IEnumerable<string> EnumerateFiles(string path, string searchPattern)
        {
            return EnumerateFiles(path, searchPattern, EnumerationOptions.Compatible);
        }

        public static IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption)
        {
            return EnumerateFiles(path, searchPattern, EnumerationOptions.FromSearchOption(searchOption));
        }

        public static IEnumerable<string> EnumerateFiles(string path, string searchPattern, EnumerationOptions enumerationOptions)
        {
            return InternalEnumeratePaths(path, searchPattern, SearchTarget.Files, enumerationOptions);
        }

        public static IEnumerable<string> EnumerateFileSystemEntries(string path)
        {
            return EnumerateFileSystemEntries(path, "*", EnumerationOptions.Compatible);
        }

        public static IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern)
        {
            return EnumerateFileSystemEntries(path, searchPattern, EnumerationOptions.Compatible);
        }

        public static IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern, SearchOption searchOption)
        {
            return EnumerateFileSystemEntries(path, searchPattern, EnumerationOptions.FromSearchOption(searchOption));
        }

        public static IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern, EnumerationOptions enumerationOptions)
        {
            return InternalEnumeratePaths(path, searchPattern, SearchTarget.Both, enumerationOptions);
        }

        public static string GetDirectoryRoot(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            string fullPath = Path.GetFullPath(path);
            return Path.GetPathRoot(fullPath);
        }

        public static string GetCurrentDirectory()
        {
            return Environment.CurrentDirectory;
        }

        public static void SetCurrentDirectory(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (path.Length == 0)
            {
                throw new ArgumentException(MDCFR.Properties.Resources.Argument_PathEmpty, "path");
            }
            Environment.CurrentDirectory = Path.GetFullPath(path);
        }

        public static void Move(string sourceDirName, string destDirName)
        {
            if (sourceDirName == null)
            {
                throw new ArgumentNullException("sourceDirName");
            }
            if (sourceDirName.Length == 0)
            {
                throw new ArgumentException(MDCFR.Properties.Resources.Argument_EmptyFileName, "sourceDirName");
            }
            if (destDirName == null)
            {
                throw new ArgumentNullException("destDirName");
            }
            if (destDirName.Length == 0)
            {
                throw new ArgumentException(MDCFR.Properties.Resources.Argument_EmptyFileName, "destDirName");
            }
            string fullPath = Path.GetFullPath(sourceDirName);
            string text = System.IO.PathInternal.EnsureTrailingSeparator(fullPath);
            string fullPath2 = Path.GetFullPath(destDirName);
            string text2 = System.IO.PathInternal.EnsureTrailingSeparator(fullPath2);
            System.ReadOnlySpan<char> fileName = Path.GetFileName(MemoryExtensions.AsSpan(fullPath));
            System.ReadOnlySpan<char> fileName2 = Path.GetFileName(MemoryExtensions.AsSpan(fullPath2));
            StringComparison stringComparison = System.IO.PathInternal.StringComparison;
            bool flag = !MemoryExtensions.SequenceEqual<char>(fileName, fileName2) && MemoryExtensions.Equals(fileName, fileName2, StringComparison.OrdinalIgnoreCase) && MemoryExtensions.Equals(fileName2, fileName, stringComparison);
            if (!flag && string.Equals(text, text2, stringComparison))
            {
                throw new IOException(MDCFR.Properties.Resources.IO_SourceDestMustBeDifferent);
            }
            System.ReadOnlySpan<char> pathRoot = Path.GetPathRoot(MemoryExtensions.AsSpan(text));
            System.ReadOnlySpan<char> pathRoot2 = Path.GetPathRoot(MemoryExtensions.AsSpan(text2));
            if (!MemoryExtensions.Equals(pathRoot, pathRoot2, StringComparison.OrdinalIgnoreCase))
            {
                throw new IOException(MDCFR.Properties.Resources.IO_SourceDestMustHaveSameRoot);
            }
            if (!FileSystem.DirectoryExists(fullPath) && !FileSystem.FileExists(fullPath))
            {
                throw new DirectoryNotFoundException(System.SR.Format(MDCFR.Properties.Resources.IO_PathNotFound_Path, fullPath));
            }
            if (!flag && FileSystem.DirectoryExists(fullPath2))
            {
                throw new IOException(System.SR.Format(MDCFR.Properties.Resources.IO_AlreadyExists_Name, fullPath2));
            }
            if (!flag && Exists(fullPath2))
            {
                throw new IOException(System.SR.Format(MDCFR.Properties.Resources.IO_AlreadyExists_Name, fullPath2));
            }
            FileSystem.MoveDirectory(fullPath, fullPath2);
        }

        public static void Delete(string path)
        {
            string fullPath = Path.GetFullPath(path);
            FileSystem.RemoveDirectory(fullPath, recursive: false);
        }

        public static void Delete(string path, bool recursive)
        {
            string fullPath = Path.GetFullPath(path);
            FileSystem.RemoveDirectory(fullPath, recursive);
        }

        public static string[] GetLogicalDrives()
        {
            return FileSystem.GetLogicalDrives();
        }

        /// <summary>
        /// Creates a directory symbolic link identified by <paramref name="path" /> that points to <paramref name="pathToTarget" />.
        /// </summary>
        /// <param name="path">The absolute path where the symbolic link should be created.</param>
        /// <param name="pathToTarget">The target directory of the symbolic link.</param>
        /// <returns>A <see cref="T:Microsoft.IO.DirectoryInfo" /> instance that wraps the newly created directory symbolic link.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="path" /> or <paramref name="pathToTarget" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="path" /> or <paramref name="pathToTarget" /> is empty.
        /// -or-
        /// <paramref name="path" /> is not an absolute path.
        /// -or-
        /// <paramref name="path" /> or <paramref name="pathToTarget" /> contains invalid path characters.</exception>
        /// <exception cref="T:System.IO.IOException">A file or directory already exists in the location of <paramref name="path" />.
        /// -or-
        /// An I/O error occurred.</exception>
        public static FileSystemInfo CreateSymbolicLink(string path, string pathToTarget)
        {
            string fullPath = Path.GetFullPath(path);
            FileSystem.VerifyValidPath(pathToTarget, "pathToTarget");
            FileSystem.CreateSymbolicLink(path, pathToTarget, isDirectory: true);
            return new DirectoryInfo(path, fullPath, null, isNormalized: true);
        }

        /// <summary>
        /// Gets the target of the specified directory link.
        /// </summary>
        /// <param name="linkPath">The path of the directory link.</param>
        /// <param name="returnFinalTarget"><see langword="true" /> to follow links to the final target; <see langword="false" /> to return the immediate next link.</param>
        /// <returns>A <see cref="T:Microsoft.IO.DirectoryInfo" /> instance if <paramref name="linkPath" /> exists, independently if the target exists or not. <see langword="null" /> if <paramref name="linkPath" /> is not a link.</returns>
        /// <exception cref="T:System.IO.IOException">The directory on <paramref name="linkPath" /> does not exist.
        /// -or-
        /// The link's file system entry type is inconsistent with that of its target.
        /// -or-
        /// Too many levels of symbolic links.</exception>
        /// <remarks>When <paramref name="returnFinalTarget" /> is <see langword="true" />, the maximum number of symbolic links that are followed are 40 on Unix and 63 on Windows.</remarks>
        public static FileSystemInfo? ResolveLinkTarget(string linkPath, bool returnFinalTarget)
        {
            FileSystem.VerifyValidPath(linkPath, "linkPath");
            return FileSystem.ResolveLinkTarget(linkPath, returnFinalTarget, isDirectory: true);
        }
    }

    public sealed class DirectoryInfo : FileSystemInfo
    {
        private bool _isNormalized;

        public DirectoryInfo? Parent
        {
            get
            {
                string directoryName = Path.GetDirectoryName(System.IO.PathInternal.IsRoot(MemoryExtensions.AsSpan(FullPath)) ? FullPath : Path.TrimEndingDirectorySeparator(FullPath));
                if (directoryName == null)
                {
                    return null;
                }
                return new DirectoryInfo(directoryName, null, null, isNormalized: true);
            }
        }

        public DirectoryInfo Root => new DirectoryInfo(Path.GetPathRoot(FullPath));

        public DirectoryInfo(string path)
        {
            Init(path, Path.GetFullPath(path), null, isNormalized: true);
        }

        internal DirectoryInfo(string originalPath, string fullPath = null, string fileName = null, bool isNormalized = false)
        {
            Init(originalPath, fullPath, fileName, isNormalized);
        }

        private void Init(string originalPath, string fullPath = null, string fileName = null, bool isNormalized = false)
        {
            OriginalPath = originalPath ?? throw new ArgumentNullException("originalPath");
            fullPath = fullPath ?? originalPath;
            fullPath = (isNormalized ? fullPath : Path.GetFullPath(fullPath));
            _name = fileName ?? (System.IO.PathInternal.IsRoot(MemoryExtensions.AsSpan(fullPath)) ? MemoryExtensions.AsSpan(fullPath) : Path.GetFileName(Path.TrimEndingDirectorySeparator(MemoryExtensions.AsSpan(fullPath)))).ToString();
            FullPath = fullPath;
            _isNormalized = isNormalized;
        }

        public DirectoryInfo CreateSubdirectory(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (System.IO.PathInternal.IsEffectivelyEmpty(MemoryExtensions.AsSpan(path)))
            {
                throw new ArgumentException(MDCFR.Properties.Resources.Argument_PathEmpty, "path");
            }
            if (Path.IsPathRooted(path))
            {
                throw new ArgumentException(MDCFR.Properties.Resources.Arg_Path2IsRooted, "path");
            }
            string fullPath = Path.GetFullPath(Path.Combine(FullPath, path));
            System.ReadOnlySpan<char> readOnlySpan = Path.TrimEndingDirectorySeparator(MemoryExtensions.AsSpan(fullPath));
            System.ReadOnlySpan<char> readOnlySpan2 = Path.TrimEndingDirectorySeparator(MemoryExtensions.AsSpan(FullPath));
            if (MemoryExtensions.StartsWith(readOnlySpan, readOnlySpan2, System.IO.PathInternal.StringComparison) && (readOnlySpan.Length == readOnlySpan2.Length || System.IO.PathInternal.IsDirectorySeparator(fullPath[readOnlySpan2.Length])))
            {
                FileSystem.CreateDirectory(fullPath);
                return new DirectoryInfo(fullPath);
            }
            throw new ArgumentException(System.SR.Format(MDCFR.Properties.Resources.Argument_InvalidSubPath, path, FullPath), "path");
        }

        public void Create()
        {
            FileSystem.CreateDirectory(FullPath);
            Invalidate();
        }

        public FileInfo[] GetFiles()
        {
            return GetFiles("*", EnumerationOptions.Compatible);
        }

        public FileInfo[] GetFiles(string searchPattern)
        {
            return GetFiles(searchPattern, EnumerationOptions.Compatible);
        }

        public FileInfo[] GetFiles(string searchPattern, SearchOption searchOption)
        {
            return GetFiles(searchPattern, EnumerationOptions.FromSearchOption(searchOption));
        }

        public FileInfo[] GetFiles(string searchPattern, EnumerationOptions enumerationOptions)
        {
            return new List<FileInfo>((IEnumerable<FileInfo>)InternalEnumerateInfos(FullPath, searchPattern, SearchTarget.Files, enumerationOptions)).ToArray();
        }

        public FileSystemInfo[] GetFileSystemInfos()
        {
            return GetFileSystemInfos("*", EnumerationOptions.Compatible);
        }

        public FileSystemInfo[] GetFileSystemInfos(string searchPattern)
        {
            return GetFileSystemInfos(searchPattern, EnumerationOptions.Compatible);
        }

        public FileSystemInfo[] GetFileSystemInfos(string searchPattern, SearchOption searchOption)
        {
            return GetFileSystemInfos(searchPattern, EnumerationOptions.FromSearchOption(searchOption));
        }

        public FileSystemInfo[] GetFileSystemInfos(string searchPattern, EnumerationOptions enumerationOptions)
        {
            return new List<FileSystemInfo>(InternalEnumerateInfos(FullPath, searchPattern, SearchTarget.Both, enumerationOptions)).ToArray();
        }

        public DirectoryInfo[] GetDirectories()
        {
            return GetDirectories("*", EnumerationOptions.Compatible);
        }

        public DirectoryInfo[] GetDirectories(string searchPattern)
        {
            return GetDirectories(searchPattern, EnumerationOptions.Compatible);
        }

        public DirectoryInfo[] GetDirectories(string searchPattern, SearchOption searchOption)
        {
            return GetDirectories(searchPattern, EnumerationOptions.FromSearchOption(searchOption));
        }

        public DirectoryInfo[] GetDirectories(string searchPattern, EnumerationOptions enumerationOptions)
        {
            return new List<DirectoryInfo>((IEnumerable<DirectoryInfo>)InternalEnumerateInfos(FullPath, searchPattern, SearchTarget.Directories, enumerationOptions)).ToArray();
        }

        public IEnumerable<DirectoryInfo> EnumerateDirectories()
        {
            return EnumerateDirectories("*", EnumerationOptions.Compatible);
        }

        public IEnumerable<DirectoryInfo> EnumerateDirectories(string searchPattern)
        {
            return EnumerateDirectories(searchPattern, EnumerationOptions.Compatible);
        }

        public IEnumerable<DirectoryInfo> EnumerateDirectories(string searchPattern, SearchOption searchOption)
        {
            return EnumerateDirectories(searchPattern, EnumerationOptions.FromSearchOption(searchOption));
        }

        public IEnumerable<DirectoryInfo> EnumerateDirectories(string searchPattern, EnumerationOptions enumerationOptions)
        {
            return (IEnumerable<DirectoryInfo>)InternalEnumerateInfos(FullPath, searchPattern, SearchTarget.Directories, enumerationOptions);
        }

        public IEnumerable<FileInfo> EnumerateFiles()
        {
            return EnumerateFiles("*", EnumerationOptions.Compatible);
        }

        public IEnumerable<FileInfo> EnumerateFiles(string searchPattern)
        {
            return EnumerateFiles(searchPattern, EnumerationOptions.Compatible);
        }

        public IEnumerable<FileInfo> EnumerateFiles(string searchPattern, SearchOption searchOption)
        {
            return EnumerateFiles(searchPattern, EnumerationOptions.FromSearchOption(searchOption));
        }

        public IEnumerable<FileInfo> EnumerateFiles(string searchPattern, EnumerationOptions enumerationOptions)
        {
            return (IEnumerable<FileInfo>)InternalEnumerateInfos(FullPath, searchPattern, SearchTarget.Files, enumerationOptions);
        }

        public IEnumerable<FileSystemInfo> EnumerateFileSystemInfos()
        {
            return EnumerateFileSystemInfos("*", EnumerationOptions.Compatible);
        }

        public IEnumerable<FileSystemInfo> EnumerateFileSystemInfos(string searchPattern)
        {
            return EnumerateFileSystemInfos(searchPattern, EnumerationOptions.Compatible);
        }

        public IEnumerable<FileSystemInfo> EnumerateFileSystemInfos(string searchPattern, SearchOption searchOption)
        {
            return EnumerateFileSystemInfos(searchPattern, EnumerationOptions.FromSearchOption(searchOption));
        }

        public IEnumerable<FileSystemInfo> EnumerateFileSystemInfos(string searchPattern, EnumerationOptions enumerationOptions)
        {
            return InternalEnumerateInfos(FullPath, searchPattern, SearchTarget.Both, enumerationOptions);
        }

        private IEnumerable<FileSystemInfo> InternalEnumerateInfos(string path, string searchPattern, SearchTarget searchTarget, EnumerationOptions options)
        {
            if (searchPattern == null)
            {
                throw new ArgumentNullException("searchPattern");
            }
            _isNormalized &= FileSystemEnumerableFactory.NormalizeInputs(ref path, ref searchPattern, options.MatchType);
            return searchTarget switch
            {
                SearchTarget.Directories => FileSystemEnumerableFactory.DirectoryInfos(path, searchPattern, options, _isNormalized),
                SearchTarget.Files => FileSystemEnumerableFactory.FileInfos(path, searchPattern, options, _isNormalized),
                SearchTarget.Both => FileSystemEnumerableFactory.FileSystemInfos(path, searchPattern, options, _isNormalized),
                _ => throw new ArgumentException(MDCFR.Properties.Resources.ArgumentOutOfRange_Enum, "searchTarget"),
            };
        }

        public void MoveTo(string destDirName)
        {
            if (destDirName == null)
            {
                throw new ArgumentNullException("destDirName");
            }
            if (destDirName.Length == 0)
            {
                throw new ArgumentException(MDCFR.Properties.Resources.Argument_EmptyFileName, "destDirName");
            }
            string fullPath = Path.GetFullPath(destDirName);
            string text = System.IO.PathInternal.EnsureTrailingSeparator(fullPath);
            string text2 = System.IO.PathInternal.EnsureTrailingSeparator(FullPath);
            if (string.Equals(text2, text, System.IO.PathInternal.StringComparison))
            {
                throw new IOException(MDCFR.Properties.Resources.IO_SourceDestMustBeDifferent);
            }
            string pathRoot = Path.GetPathRoot(text2);
            string pathRoot2 = Path.GetPathRoot(text);
            if (!string.Equals(pathRoot, pathRoot2, System.IO.PathInternal.StringComparison))
            {
                throw new IOException(MDCFR.Properties.Resources.IO_SourceDestMustHaveSameRoot);
            }
            if (!Exists && !FileSystem.FileExists(FullPath))
            {
                throw new DirectoryNotFoundException(System.SR.Format(MDCFR.Properties.Resources.IO_PathNotFound_Path, FullPath));
            }
            if (FileSystem.DirectoryExists(fullPath))
            {
                throw new IOException(System.SR.Format(MDCFR.Properties.Resources.IO_AlreadyExists_Name, text));
            }
            FileSystem.MoveDirectory(FullPath, fullPath);
            Init(destDirName, text, null, isNormalized: true);
            Invalidate();
        }

        public override void Delete()
        {
            Delete(recursive: false);
        }

        public void Delete(bool recursive)
        {
            FileSystem.RemoveDirectory(FullPath, recursive);
            Invalidate();
        }
    }

    /// <summary>
    /// Simple wrapper to safely disable the normal media insertion prompt for
    /// removable media (floppies, cds, memory cards, etc.)
    /// </summary>
    /// <remarks>
    /// Note that removable media file systems lazily load. After starting the OS
    /// they won't be loaded until you have media in the drive- and as such the
    /// prompt won't happen. You have to have had media in at least once to get
    /// the file system to load and then have removed it.
    /// </remarks>
    internal struct DisableMediaInsertionPrompt : IDisposable
    {
        private bool _disableSuccess;

        private uint _oldMode;

        public static DisableMediaInsertionPrompt Create()
        {
            DisableMediaInsertionPrompt result = default(DisableMediaInsertionPrompt);
            result._disableSuccess = global::Interop.Kernel32.SetThreadErrorMode(1u, out result._oldMode);
            return result;
        }

        public void Dispose()
        {
            if (_disableSuccess)
            {
                global::Interop.Kernel32.SetThreadErrorMode(_oldMode, out var _);
            }
        }
    }

    /// <summary>Provides file and directory enumeration options.</summary>
    public class EnumerationOptions
    {
        private int _maxRecursionDepth;

        internal const int DefaultMaxRecursionDepth = int.MaxValue;

        /// <summary>
        /// For internal use. These are the options we want to use if calling the existing Directory/File APIs where you don't
        /// explicitly specify EnumerationOptions.
        /// </summary>
        internal static EnumerationOptions Compatible { get; } = new EnumerationOptions
        {
            MatchType = MatchType.Win32,
            AttributesToSkip = (FileAttributes)0,
            IgnoreInaccessible = false
        };


        private static EnumerationOptions CompatibleRecursive { get; } = new EnumerationOptions
        {
            RecurseSubdirectories = true,
            MatchType = MatchType.Win32,
            AttributesToSkip = (FileAttributes)0,
            IgnoreInaccessible = false
        };


        /// <summary>
        /// Internal singleton for default options.
        /// </summary>
        internal static EnumerationOptions Default { get; } = new EnumerationOptions();


        /// <summary>Gets or sets a value that indicates whether to recurse into subdirectories while enumerating. The default is <see langword="false" />.</summary>
        /// <value><see langword="true" /> to recurse into subdirectories; otherwise, <see langword="false" />.</value>
        public bool RecurseSubdirectories { get; set; }

        /// <summary>Gets or sets a value that indicates whether to skip files or directories when access is denied (for example, <see cref="T:System.UnauthorizedAccessException" /> or <see cref="T:System.Security.SecurityException" />). The default is <see langword="true" />.</summary>
        /// <value><see langword="true" /> to skip innacessible files or directories; otherwise, <see langword="false" />.</value>
        public bool IgnoreInaccessible { get; set; }

        /// <summary>Gets or sets the suggested buffer size, in bytes. The default is 0 (no suggestion).</summary>
        /// <value>The buffer size.</value>
        /// <remarks>Not all platforms use user allocated buffers, and some require either fixed buffers or a buffer that has enough space to return a full result.
        /// One scenario where this option is useful is with remote share enumeration on Windows. Having a large buffer may result in better performance as more results can be batched over the wire (for example, over a network share).
        /// A "large" buffer, for example, would be 16K. Typical is 4K.
        /// The suggested buffer size will not be used if it has no meaning for the native APIs on the current platform or if it would be too small for getting at least a single result.</remarks>
        public int BufferSize { get; set; }

        /// <summary>Gets or sets the attributes to skip. The default is <c>FileAttributes.Hidden | FileAttributes.System</c>.</summary>
        /// <value>The attributes to skip.</value>
        public FileAttributes AttributesToSkip { get; set; }

        /// <summary>Gets or sets the match type.</summary>
        /// <value>One of the enumeration values that indicates the match type.</value>
        /// <remarks>For APIs that allow specifying a match expression, this property allows you to specify how to interpret the match expression.
        /// The default is simple matching where '*' is always 0 or more characters and '?' is a single character.</remarks>
        public MatchType MatchType { get; set; }

        /// <summary>Gets or sets the case matching behavior.</summary>
        /// <value>One of the enumeration values that indicates the case matching behavior.</value>
        /// <remarks>For APIs that allow specifying a match expression, this property allows you to specify the case matching behavior.
        /// The default is to match platform defaults, which are gleaned from the case sensitivity of the temporary folder.</remarks>
        public MatchCasing MatchCasing { get; set; }

        /// <summary>Gets or sets a value that indicates the maximum directory depth to recurse while enumerating, when <see cref="P:Microsoft.IO.EnumerationOptions.RecurseSubdirectories" /> is set to <see langword="true" />.</summary>
        /// <value>A number that represents the maximum directory depth to recurse while enumerating. The default value is <see cref="F:System.Int32.MaxValue" />.</value>
        /// <remarks>If <see cref="P:Microsoft.IO.EnumerationOptions.MaxRecursionDepth" /> is set to a negative number, the default value <see cref="F:System.Int32.MaxValue" /> is used.
        /// If <see cref="P:Microsoft.IO.EnumerationOptions.MaxRecursionDepth" /> is set to zero, enumeration returns the contents of the initial directory.</remarks>
        public int MaxRecursionDepth
        {
            get
            {
                return _maxRecursionDepth;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", MDCFR.Properties.Resources.ArgumentOutOfRange_NeedNonNegNum);
                }
                _maxRecursionDepth = value;
            }
        }

        /// <summary>Gets or sets a value that indicates whether to return the special directory entries "." and "..".</summary>
        /// <value><see langword="true" /> to return the special directory entries "." and ".."; otherwise, <see langword="false" />.</value>
        public bool ReturnSpecialDirectories { get; set; }

        /// <summary>Initializes a new instance of the <see cref="T:Microsoft.IO.EnumerationOptions" /> class with the recommended default options.</summary>
        public EnumerationOptions()
        {
            IgnoreInaccessible = true;
            AttributesToSkip = FileAttributes.Hidden | FileAttributes.System;
            MaxRecursionDepth = int.MaxValue;
        }

        /// <summary>
        /// Converts SearchOptions to FindOptions. Throws if undefined SearchOption.
        /// </summary>
        internal static EnumerationOptions FromSearchOption(SearchOption searchOption)
        {
            if (searchOption != 0 && searchOption != SearchOption.AllDirectories)
            {
                throw new ArgumentOutOfRangeException("searchOption", MDCFR.Properties.Resources.ArgumentOutOfRange_Enum);
            }
            if (searchOption != SearchOption.AllDirectories)
            {
                return Compatible;
            }
            return CompatibleRecursive;
        }
    }

    public static class File
    {
        private const int MaxByteArrayLength = 2147483591;

        private static Encoding s_UTF8NoBOM;

        internal const int DefaultBufferSize = 4096;

        private static Encoding UTF8NoBOM => s_UTF8NoBOM ?? (s_UTF8NoBOM = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true));

        public static StreamReader OpenText(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            return new StreamReader(path);
        }

        public static StreamWriter CreateText(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            return new StreamWriter(path, append: false);
        }

        public static StreamWriter AppendText(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            return new StreamWriter(path, append: true);
        }

        /// <summary>
        /// Copies an existing file to a new file.
        /// An exception is raised if the destination file already exists.
        /// </summary>
        public static void Copy(string sourceFileName, string destFileName)
        {
            Copy(sourceFileName, destFileName, overwrite: false);
        }

        /// <summary>
        /// Copies an existing file to a new file.
        /// If <paramref name="overwrite" /> is false, an exception will be
        /// raised if the destination exists. Otherwise it will be overwritten.
        /// </summary>
        public static void Copy(string sourceFileName, string destFileName, bool overwrite)
        {
            if (sourceFileName == null)
            {
                throw new ArgumentNullException("sourceFileName", MDCFR.Properties.Resources.ArgumentNull_FileName);
            }
            if (destFileName == null)
            {
                throw new ArgumentNullException("destFileName", MDCFR.Properties.Resources.ArgumentNull_FileName);
            }
            if (sourceFileName.Length == 0)
            {
                throw new ArgumentException(MDCFR.Properties.Resources.Argument_EmptyFileName, "sourceFileName");
            }
            if (destFileName.Length == 0)
            {
                throw new ArgumentException(MDCFR.Properties.Resources.Argument_EmptyFileName, "destFileName");
            }
            FileSystem.CopyFile(Path.GetFullPath(sourceFileName), Path.GetFullPath(destFileName), overwrite);
        }

        public static FileStream Create(string path)
        {
            return Create(path, 4096);
        }

        public static FileStream Create(string path, int bufferSize)
        {
            return new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None, bufferSize);
        }

        public static FileStream Create(string path, int bufferSize, FileOptions options)
        {
            return new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None, bufferSize, options);
        }

        public static void Delete(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            FileSystem.DeleteFile(Path.GetFullPath(path));
        }

        public static bool Exists([NotNullWhen(true)] string? path)
        {
            try
            {
                if (path == null)
                {
                    return false;
                }
                if (path.Length == 0)
                {
                    return false;
                }
                path = Path.GetFullPath(path);
                if (path.Length > 0 && System.IO.PathInternal.IsDirectorySeparator(path[path.Length - 1]))
                {
                    return false;
                }
                return FileSystem.FileExists(path);
            }
            catch (ArgumentException)
            {
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
            return false;
        }

        public static FileStream Open(string path, FileMode mode)
        {
            return Open(path, mode, (mode == FileMode.Append) ? FileAccess.Write : FileAccess.ReadWrite, FileShare.None);
        }

        public static FileStream Open(string path, FileMode mode, FileAccess access)
        {
            return Open(path, mode, access, FileShare.None);
        }

        public static FileStream Open(string path, FileMode mode, FileAccess access, FileShare share)
        {
            return new FileStream(path, mode, access, share);
        }

        internal static DateTimeOffset GetUtcDateTimeOffset(DateTime dateTime)
        {
            if (dateTime.Kind == DateTimeKind.Unspecified)
            {
                return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            }
            return dateTime.ToUniversalTime();
        }

        public static void SetCreationTime(string path, DateTime creationTime)
        {
            string fullPath = Path.GetFullPath(path);
            FileSystem.SetCreationTime(fullPath, creationTime, asDirectory: false);
        }

        public static void SetCreationTimeUtc(string path, DateTime creationTimeUtc)
        {
            string fullPath = Path.GetFullPath(path);
            FileSystem.SetCreationTime(fullPath, GetUtcDateTimeOffset(creationTimeUtc), asDirectory: false);
        }

        public static DateTime GetCreationTime(string path)
        {
            string fullPath = Path.GetFullPath(path);
            return FileSystem.GetCreationTime(fullPath).LocalDateTime;
        }

        public static DateTime GetCreationTimeUtc(string path)
        {
            string fullPath = Path.GetFullPath(path);
            return FileSystem.GetCreationTime(fullPath).UtcDateTime;
        }

        public static void SetLastAccessTime(string path, DateTime lastAccessTime)
        {
            string fullPath = Path.GetFullPath(path);
            FileSystem.SetLastAccessTime(fullPath, lastAccessTime, asDirectory: false);
        }

        public static void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc)
        {
            string fullPath = Path.GetFullPath(path);
            FileSystem.SetLastAccessTime(fullPath, GetUtcDateTimeOffset(lastAccessTimeUtc), asDirectory: false);
        }

        public static DateTime GetLastAccessTime(string path)
        {
            string fullPath = Path.GetFullPath(path);
            return FileSystem.GetLastAccessTime(fullPath).LocalDateTime;
        }

        public static DateTime GetLastAccessTimeUtc(string path)
        {
            string fullPath = Path.GetFullPath(path);
            return FileSystem.GetLastAccessTime(fullPath).UtcDateTime;
        }

        public static void SetLastWriteTime(string path, DateTime lastWriteTime)
        {
            string fullPath = Path.GetFullPath(path);
            FileSystem.SetLastWriteTime(fullPath, lastWriteTime, asDirectory: false);
        }

        public static void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
        {
            string fullPath = Path.GetFullPath(path);
            FileSystem.SetLastWriteTime(fullPath, GetUtcDateTimeOffset(lastWriteTimeUtc), asDirectory: false);
        }

        public static DateTime GetLastWriteTime(string path)
        {
            string fullPath = Path.GetFullPath(path);
            return FileSystem.GetLastWriteTime(fullPath).LocalDateTime;
        }

        public static DateTime GetLastWriteTimeUtc(string path)
        {
            string fullPath = Path.GetFullPath(path);
            return FileSystem.GetLastWriteTime(fullPath).UtcDateTime;
        }

        public static FileAttributes GetAttributes(string path)
        {
            string fullPath = Path.GetFullPath(path);
            return FileSystem.GetAttributes(fullPath);
        }

        public static void SetAttributes(string path, FileAttributes fileAttributes)
        {
            string fullPath = Path.GetFullPath(path);
            FileSystem.SetAttributes(fullPath, fileAttributes);
        }

        public static FileStream OpenRead(string path)
        {
            return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public static FileStream OpenWrite(string path)
        {
            return new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
        }

        public static string ReadAllText(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (path.Length == 0)
            {
                throw new ArgumentException(MDCFR.Properties.Resources.Argument_EmptyPath, "path");
            }
            return InternalReadAllText(path, Encoding.UTF8);
        }

        public static string ReadAllText(string path, Encoding encoding)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }
            if (path.Length == 0)
            {
                throw new ArgumentException(MDCFR.Properties.Resources.Argument_EmptyPath, "path");
            }
            return InternalReadAllText(path, encoding);
        }

        private static string InternalReadAllText(string path, Encoding encoding)
        {
            using StreamReader streamReader = new StreamReader(path, encoding, detectEncodingFromByteOrderMarks: true);
            return streamReader.ReadToEnd();
        }

        public static void WriteAllText(string path, string? contents)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (path.Length == 0)
            {
                throw new ArgumentException(MDCFR.Properties.Resources.Argument_EmptyPath, "path");
            }
            using StreamWriter streamWriter = new StreamWriter(path);
            streamWriter.Write(contents);
        }

        public static void WriteAllText(string path, string? contents, Encoding encoding)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }
            if (path.Length == 0)
            {
                throw new ArgumentException(MDCFR.Properties.Resources.Argument_EmptyPath, "path");
            }
            using StreamWriter streamWriter = new StreamWriter(path, append: false, encoding);
            streamWriter.Write(contents);
        }

        public static byte[] ReadAllBytes(string path)
        {
            using FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 1, FileOptions.SequentialScan);
            long length = fileStream.Length;
            if (length > int.MaxValue)
            {
                throw new IOException(MDCFR.Properties.Resources.IO_FileTooLong2GB);
            }
            int num = 0;
            int num2 = (int)length;
            byte[] array = new byte[num2];
            while (num2 > 0)
            {
                int num3 = fileStream.Read(array, num, num2);
                if (num3 == 0)
                {
                    ThrowHelper.ThrowEndOfFileException();
                }
                num += num3;
                num2 -= num3;
            }
            return array;
        }

        public static void WriteAllBytes(string path, byte[] bytes)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path", MDCFR.Properties.Resources.ArgumentNull_Path);
            }
            if (path.Length == 0)
            {
                throw new ArgumentException(MDCFR.Properties.Resources.Argument_EmptyPath, "path");
            }
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }
            using FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read);
            fileStream.Write(bytes, 0, bytes.Length);
        }

        public static string[] ReadAllLines(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (path.Length == 0)
            {
                throw new ArgumentException(MDCFR.Properties.Resources.Argument_EmptyPath, "path");
            }
            return InternalReadAllLines(path, Encoding.UTF8);
        }

        public static string[] ReadAllLines(string path, Encoding encoding)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }
            if (path.Length == 0)
            {
                throw new ArgumentException(MDCFR.Properties.Resources.Argument_EmptyPath, "path");
            }
            return InternalReadAllLines(path, encoding);
        }

        private static string[] InternalReadAllLines(string path, Encoding encoding)
        {
            List<string> list = new List<string>();
            using (StreamReader streamReader = new StreamReader(path, encoding))
            {
                string item;
                while ((item = streamReader.ReadLine()) != null)
                {
                    list.Add(item);
                }
            }
            return list.ToArray();
        }

        public static IEnumerable<string> ReadLines(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (path.Length == 0)
            {
                throw new ArgumentException(MDCFR.Properties.Resources.Argument_EmptyPath, "path");
            }
            return System.IO.ReadLinesIterator.CreateIterator(path, Encoding.UTF8);
        }

        public static IEnumerable<string> ReadLines(string path, Encoding encoding)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }
            if (path.Length == 0)
            {
                throw new ArgumentException(MDCFR.Properties.Resources.Argument_EmptyPath, "path");
            }
            return System.IO.ReadLinesIterator.CreateIterator(path, encoding);
        }

        public static void WriteAllLines(string path, string[] contents)
        {
            WriteAllLines(path, (IEnumerable<string>)contents);
        }

        public static void WriteAllLines(string path, IEnumerable<string> contents)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (contents == null)
            {
                throw new ArgumentNullException("contents");
            }
            if (path.Length == 0)
            {
                throw new ArgumentException(MDCFR.Properties.Resources.Argument_EmptyPath, "path");
            }
            InternalWriteAllLines(new StreamWriter(path), contents);
        }

        public static void WriteAllLines(string path, string[] contents, Encoding encoding)
        {
            WriteAllLines(path, (IEnumerable<string>)contents, encoding);
        }

        public static void WriteAllLines(string path, IEnumerable<string> contents, Encoding encoding)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (contents == null)
            {
                throw new ArgumentNullException("contents");
            }
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }
            if (path.Length == 0)
            {
                throw new ArgumentException(MDCFR.Properties.Resources.Argument_EmptyPath, "path");
            }
            InternalWriteAllLines(new StreamWriter(path, append: false, encoding), contents);
        }

        private static void InternalWriteAllLines(TextWriter writer, IEnumerable<string> contents)
        {
            using (writer)
            {
                foreach (string content in contents)
                {
                    writer.WriteLine(content);
                }
            }
        }

        public static void AppendAllText(string path, string? contents)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (path.Length == 0)
            {
                throw new ArgumentException(MDCFR.Properties.Resources.Argument_EmptyPath, "path");
            }
            using StreamWriter streamWriter = new StreamWriter(path, append: true);
            streamWriter.Write(contents);
        }

        public static void AppendAllText(string path, string? contents, Encoding encoding)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }
            if (path.Length == 0)
            {
                throw new ArgumentException(MDCFR.Properties.Resources.Argument_EmptyPath, "path");
            }
            using StreamWriter streamWriter = new StreamWriter(path, append: true, encoding);
            streamWriter.Write(contents);
        }

        public static void AppendAllLines(string path, IEnumerable<string> contents)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (contents == null)
            {
                throw new ArgumentNullException("contents");
            }
            if (path.Length == 0)
            {
                throw new ArgumentException(MDCFR.Properties.Resources.Argument_EmptyPath, "path");
            }
            InternalWriteAllLines(new StreamWriter(path, append: true), contents);
        }

        public static void AppendAllLines(string path, IEnumerable<string> contents, Encoding encoding)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (contents == null)
            {
                throw new ArgumentNullException("contents");
            }
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }
            if (path.Length == 0)
            {
                throw new ArgumentException(MDCFR.Properties.Resources.Argument_EmptyPath, "path");
            }
            InternalWriteAllLines(new StreamWriter(path, append: true, encoding), contents);
        }

        public static void Replace(string sourceFileName, string destinationFileName, string? destinationBackupFileName)
        {
            Replace(sourceFileName, destinationFileName, destinationBackupFileName, ignoreMetadataErrors: false);
        }

        public static void Replace(string sourceFileName, string destinationFileName, string? destinationBackupFileName, bool ignoreMetadataErrors)
        {
            if (sourceFileName == null)
            {
                throw new ArgumentNullException("sourceFileName");
            }
            if (destinationFileName == null)
            {
                throw new ArgumentNullException("destinationFileName");
            }
            FileSystem.ReplaceFile(Path.GetFullPath(sourceFileName), Path.GetFullPath(destinationFileName), (destinationBackupFileName != null) ? Path.GetFullPath(destinationBackupFileName) : null, ignoreMetadataErrors);
        }

        public static void Move(string sourceFileName, string destFileName)
        {
            Move(sourceFileName, destFileName, overwrite: false);
        }

        public static void Move(string sourceFileName, string destFileName, bool overwrite)
        {
            if (sourceFileName == null)
            {
                throw new ArgumentNullException("sourceFileName", MDCFR.Properties.Resources.ArgumentNull_FileName);
            }
            if (destFileName == null)
            {
                throw new ArgumentNullException("destFileName", MDCFR.Properties.Resources.ArgumentNull_FileName);
            }
            if (sourceFileName.Length == 0)
            {
                throw new ArgumentException(MDCFR.Properties.Resources.Argument_EmptyFileName, "sourceFileName");
            }
            if (destFileName.Length == 0)
            {
                throw new ArgumentException(MDCFR.Properties.Resources.Argument_EmptyFileName, "destFileName");
            }
            string fullPath = Path.GetFullPath(sourceFileName);
            string fullPath2 = Path.GetFullPath(destFileName);
            if (!FileSystem.FileExists(fullPath))
            {
                throw new FileNotFoundException(System.SR.Format(MDCFR.Properties.Resources.IO_FileNotFound_FileName, fullPath), fullPath);
            }
            FileSystem.MoveFile(fullPath, fullPath2, overwrite);
        }

        [SupportedOSPlatform("windows")]
        public static void Encrypt(string path)
        {
            FileSystem.Encrypt(path ?? throw new ArgumentNullException("path"));
        }

        [SupportedOSPlatform("windows")]
        public static void Decrypt(string path)
        {
            FileSystem.Decrypt(path ?? throw new ArgumentNullException("path"));
        }

        private static StreamReader AsyncStreamReader(string path, Encoding encoding)
        {
            FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan);
            return new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks: true);
        }

        private static StreamWriter AsyncStreamWriter(string path, Encoding encoding, bool append)
        {
            FileStream stream = new FileStream(path, append ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.Read, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan);
            return new StreamWriter(stream, encoding);
        }

        public static Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ReadAllTextAsync(path, Encoding.UTF8, cancellationToken);
        }

        public static Task<string> ReadAllTextAsync(string path, Encoding encoding, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }
            if (path.Length == 0)
            {
                throw new ArgumentException(MDCFR.Properties.Resources.Argument_EmptyPath, "path");
            }
            if (!cancellationToken.IsCancellationRequested)
            {
                return InternalReadAllTextAsync(path, encoding, cancellationToken);
            }
            return Task.FromCanceled<string>(cancellationToken);
        }

        private static async Task<string> InternalReadAllTextAsync(string path, Encoding encoding, CancellationToken cancellationToken)
        {
            char[] buffer = null;
            StreamReader sr = AsyncStreamReader(path, encoding);
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                buffer = ArrayPool<char>.Shared.Rent(sr.CurrentEncoding.GetMaxCharCount(4096));
                StringBuilder sb = new StringBuilder();
                while (true)
                {
                    int num = await sr.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(continueOnCapturedContext: false);
                    if (num == 0)
                    {
                        break;
                    }
                    sb.Append(buffer, 0, num);
                }
                return sb.ToString();
            }
            finally
            {
                sr.Dispose();
                if (buffer != null)
                {
                    ArrayPool<char>.Shared.Return(buffer);
                }
            }
        }

        public static Task WriteAllTextAsync(string path, string? contents, CancellationToken cancellationToken = default(CancellationToken))
        {
            return WriteAllTextAsync(path, contents, UTF8NoBOM, cancellationToken);
        }

        public static Task WriteAllTextAsync(string path, string? contents, Encoding encoding, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }
            if (path.Length == 0)
            {
                throw new ArgumentException(MDCFR.Properties.Resources.Argument_EmptyPath, "path");
            }
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }
            if (string.IsNullOrEmpty(contents))
            {
                new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read).Dispose();
                return Task.CompletedTask;
            }
            return InternalWriteAllTextAsync(AsyncStreamWriter(path, encoding, append: false), contents, cancellationToken);
        }

        public static Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled<byte[]>(cancellationToken);
            }
            FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 1, FileOptions.Asynchronous | FileOptions.SequentialScan);
            bool flag = false;
            try
            {
                long length = fileStream.Length;
                if (length > int.MaxValue)
                {
                    IOException exception = new IOException(MDCFR.Properties.Resources.IO_FileTooLong2GB);
                    return Task.FromException<byte[]>(exception);
                }
                flag = true;
                return (length > 0) ? InternalReadAllBytesAsync(fileStream, (int)length, cancellationToken) : InternalReadAllBytesUnknownLengthAsync(fileStream, cancellationToken);
            }
            finally
            {
                if (!flag)
                {
                    fileStream.Dispose();
                }
            }
        }

        private static async Task<byte[]> InternalReadAllBytesAsync(FileStream fs, int count, CancellationToken cancellationToken)
        {
            using (fs)
            {
                int index = 0;
                byte[] bytes = new byte[count];
                do
                {
                    int num = await fs.ReadAsync(bytes, index, count - index, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                    if (num == 0)
                    {
                        ThrowHelper.ThrowEndOfFileException();
                    }
                    index += num;
                }
                while (index < count);
                return bytes;
            }
        }

        private static async Task<byte[]> InternalReadAllBytesUnknownLengthAsync(FileStream fs, CancellationToken cancellationToken)
        {
            byte[] rentedArray = ArrayPool<byte>.Shared.Rent(512);
            try
            {
                int bytesRead = 0;
                while (true)
                {
                    if (bytesRead == rentedArray.Length)
                    {
                        uint num = (uint)(rentedArray.Length * 2);
                        if (num > 2147483591)
                        {
                            num = (uint)Math.Max(2147483591, rentedArray.Length + 1);
                        }
                        byte[] array = ArrayPool<byte>.Shared.Rent((int)num);
                        Buffer.BlockCopy(rentedArray, 0, array, 0, bytesRead);
                        byte[] array2 = rentedArray;
                        rentedArray = array;
                        ArrayPool<byte>.Shared.Return(array2);
                    }
                    int num2 = await fs.ReadAsync(rentedArray, bytesRead, rentedArray.Length - bytesRead, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                    if (num2 == 0)
                    {
                        break;
                    }
                    bytesRead += num2;
                }
                return MemoryExtensions.AsSpan<byte>(rentedArray, 0, bytesRead).ToArray();
            }
            finally
            {
                fs.Dispose();
                ArrayPool<byte>.Shared.Return(rentedArray);
            }
        }

        public static Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (path == null)
            {
                throw new ArgumentNullException("path", MDCFR.Properties.Resources.ArgumentNull_Path);
            }
            if (path.Length == 0)
            {
                throw new ArgumentException(MDCFR.Properties.Resources.Argument_EmptyPath, "path");
            }
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }
            if (!cancellationToken.IsCancellationRequested)
            {
                return Core(path, bytes, cancellationToken);
            }
            return Task.FromCanceled(cancellationToken);
            static async Task Core(string path, byte[] bytes, CancellationToken cancellationToken)
            {
                using FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read, 1, FileOptions.Asynchronous | FileOptions.SequentialScan);
                await fs.WriteAsync(bytes, 0, bytes.Length, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
            }
        }

        public static Task<string[]> ReadAllLinesAsync(string path, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ReadAllLinesAsync(path, Encoding.UTF8, cancellationToken);
        }

        public static Task<string[]> ReadAllLinesAsync(string path, Encoding encoding, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }
            if (path.Length == 0)
            {
                throw new ArgumentException(MDCFR.Properties.Resources.Argument_EmptyPath, "path");
            }
            if (!cancellationToken.IsCancellationRequested)
            {
                return InternalReadAllLinesAsync(path, encoding, cancellationToken);
            }
            return Task.FromCanceled<string[]>(cancellationToken);
        }

        private static async Task<string[]> InternalReadAllLinesAsync(string path, Encoding encoding, CancellationToken cancellationToken)
        {
            using StreamReader sr = AsyncStreamReader(path, encoding);
            cancellationToken.ThrowIfCancellationRequested();
            List<string> lines = new List<string>();
            string item;
            while ((item = await sr.ReadLineAsync().ConfigureAwait(continueOnCapturedContext: false)) != null)
            {
                lines.Add(item);
                cancellationToken.ThrowIfCancellationRequested();
            }
            return lines.ToArray();
        }

        public static Task WriteAllLinesAsync(string path, IEnumerable<string> contents, CancellationToken cancellationToken = default(CancellationToken))
        {
            return WriteAllLinesAsync(path, contents, UTF8NoBOM, cancellationToken);
        }

        public static Task WriteAllLinesAsync(string path, IEnumerable<string> contents, Encoding encoding, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (contents == null)
            {
                throw new ArgumentNullException("contents");
            }
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }
            if (path.Length == 0)
            {
                throw new ArgumentException(MDCFR.Properties.Resources.Argument_EmptyPath, "path");
            }
            if (!cancellationToken.IsCancellationRequested)
            {
                return InternalWriteAllLinesAsync(AsyncStreamWriter(path, encoding, append: false), contents, cancellationToken);
            }
            return Task.FromCanceled(cancellationToken);
        }

        private static async Task InternalWriteAllLinesAsync(TextWriter writer, IEnumerable<string> contents, CancellationToken cancellationToken)
        {
            using (writer)
            {
                foreach (string content in contents)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await writer.WriteLineAsync(content).ConfigureAwait(continueOnCapturedContext: false);
                }
                cancellationToken.ThrowIfCancellationRequested();
                await writer.FlushAsync().ConfigureAwait(continueOnCapturedContext: false);
            }
        }

        private static async Task InternalWriteAllTextAsync(StreamWriter sw, string contents, CancellationToken cancellationToken)
        {
            char[] buffer = null;
            try
            {
                buffer = ArrayPool<char>.Shared.Rent(4096);
                int count = contents.Length;
                int batchSize;
                for (int index = 0; index < count; index += batchSize)
                {
                    batchSize = Math.Min(4096, count - index);
                    contents.CopyTo(index, buffer, 0, batchSize);
                    await sw.WriteAsync(buffer, 0, batchSize).ConfigureAwait(continueOnCapturedContext: false);
                }
                cancellationToken.ThrowIfCancellationRequested();
                await sw.FlushAsync().ConfigureAwait(continueOnCapturedContext: false);
            }
            finally
            {
                sw.Dispose();
                if (buffer != null)
                {
                    ArrayPool<char>.Shared.Return(buffer);
                }
            }
        }

        public static Task AppendAllTextAsync(string path, string? contents, CancellationToken cancellationToken = default(CancellationToken))
        {
            return AppendAllTextAsync(path, contents, UTF8NoBOM, cancellationToken);
        }

        public static Task AppendAllTextAsync(string path, string? contents, Encoding encoding, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }
            if (path.Length == 0)
            {
                throw new ArgumentException(MDCFR.Properties.Resources.Argument_EmptyPath, "path");
            }
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }
            if (string.IsNullOrEmpty(contents))
            {
                new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read).Dispose();
                return Task.CompletedTask;
            }
            return InternalWriteAllTextAsync(AsyncStreamWriter(path, encoding, append: true), contents, cancellationToken);
        }

        public static Task AppendAllLinesAsync(string path, IEnumerable<string> contents, CancellationToken cancellationToken = default(CancellationToken))
        {
            return AppendAllLinesAsync(path, contents, UTF8NoBOM, cancellationToken);
        }

        public static Task AppendAllLinesAsync(string path, IEnumerable<string> contents, Encoding encoding, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (contents == null)
            {
                throw new ArgumentNullException("contents");
            }
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }
            if (path.Length == 0)
            {
                throw new ArgumentException(MDCFR.Properties.Resources.Argument_EmptyPath, "path");
            }
            if (!cancellationToken.IsCancellationRequested)
            {
                return InternalWriteAllLinesAsync(AsyncStreamWriter(path, encoding, append: true), contents, cancellationToken);
            }
            return Task.FromCanceled(cancellationToken);
        }

        /// <summary>
        /// Creates a file symbolic link identified by <paramref name="path" /> that points to <paramref name="pathToTarget" />.
        /// </summary>
        /// <param name="path">The path where the symbolic link should be created.</param>
        /// <param name="pathToTarget">The path of the target to which the symbolic link points.</param>
        /// <returns>A <see cref="T:Microsoft.IO.FileInfo" /> instance that wraps the newly created file symbolic link.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="path" /> or <paramref name="pathToTarget" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="path" /> or <paramref name="pathToTarget" /> is empty.
        /// -or-
        /// <paramref name="path" /> or <paramref name="pathToTarget" /> contains a null character.</exception>
        /// <exception cref="T:System.IO.IOException">A file or directory already exists in the location of <paramref name="path" />.
        /// -or-
        /// An I/O error occurred.</exception>
        public static FileSystemInfo CreateSymbolicLink(string path, string pathToTarget)
        {
            string fullPath = Path.GetFullPath(path);
            FileSystem.VerifyValidPath(pathToTarget, "pathToTarget");
            FileSystem.CreateSymbolicLink(path, pathToTarget, isDirectory: false);
            return new FileInfo(path, fullPath, null, isNormalized: true);
        }

        /// <summary>
        /// Gets the target of the specified file link.
        /// </summary>
        /// <param name="linkPath">The path of the file link.</param>
        /// <param name="returnFinalTarget"><see langword="true" /> to follow links to the final target; <see langword="false" /> to return the immediate next link.</param>
        /// <returns>A <see cref="T:Microsoft.IO.FileInfo" /> instance if <paramref name="linkPath" /> exists, independently if the target exists or not. <see langword="null" /> if <paramref name="linkPath" /> is not a link.</returns>
        /// <exception cref="T:System.IO.IOException">The file on <paramref name="linkPath" /> does not exist.
        /// -or-
        /// The link's file system entry type is inconsistent with that of its target.
        /// -or-
        /// Too many levels of symbolic links.</exception>
        /// <remarks>When <paramref name="returnFinalTarget" /> is <see langword="true" />, the maximum number of symbolic links that are followed are 40 on Unix and 63 on Windows.</remarks>
        public static FileSystemInfo? ResolveLinkTarget(string linkPath, bool returnFinalTarget)
        {
            FileSystem.VerifyValidPath(linkPath, "linkPath");
            return FileSystem.ResolveLinkTarget(linkPath, returnFinalTarget, isDirectory: false);
        }
    }

    public sealed class FileInfo : FileSystemInfo
    {
        public long Length
        {
            get
            {
                if ((base.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    throw new FileNotFoundException(System.SR.Format(MDCFR.Properties.Resources.IO_FileNotFound_FileName, FullPath), FullPath);
                }
                return base.LengthCore;
            }
        }

        public string? DirectoryName => Path.GetDirectoryName(FullPath);

        public DirectoryInfo? Directory
        {
            get
            {
                string directoryName = DirectoryName;
                if (directoryName == null)
                {
                    return null;
                }
                return new DirectoryInfo(directoryName);
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return (base.Attributes & FileAttributes.ReadOnly) != 0;
            }
            set
            {
                if (value)
                {
                    base.Attributes |= FileAttributes.ReadOnly;
                }
                else
                {
                    base.Attributes &= ~FileAttributes.ReadOnly;
                }
            }
        }

        private FileInfo()
        {
        }

        public FileInfo(string fileName)
            : this(fileName, null, null, isNormalized: false)
        {
        }

        internal FileInfo(string originalPath, string fullPath = null, string fileName = null, bool isNormalized = false)
        {
            OriginalPath = originalPath ?? throw new ArgumentNullException("fileName");
            fullPath = fullPath ?? originalPath;
            FullPath = (isNormalized ? (fullPath ?? originalPath) : Path.GetFullPath(fullPath));
            _name = fileName ?? Path.GetFileName(originalPath);
        }

        public StreamReader OpenText()
        {
            return new StreamReader(base.NormalizedPath, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        }

        public StreamWriter CreateText()
        {
            return new StreamWriter(base.NormalizedPath, append: false);
        }

        public StreamWriter AppendText()
        {
            return new StreamWriter(base.NormalizedPath, append: true);
        }

        public FileInfo CopyTo(string destFileName)
        {
            return CopyTo(destFileName, overwrite: false);
        }

        public FileInfo CopyTo(string destFileName, bool overwrite)
        {
            if (destFileName == null)
            {
                throw new ArgumentNullException("destFileName", MDCFR.Properties.Resources.ArgumentNull_FileName);
            }
            if (destFileName.Length == 0)
            {
                throw new ArgumentException(MDCFR.Properties.Resources.Argument_EmptyFileName, "destFileName");
            }
            string fullPath = Path.GetFullPath(destFileName);
            FileSystem.CopyFile(FullPath, fullPath, overwrite);
            return new FileInfo(fullPath, null, null, isNormalized: true);
        }

        public FileStream Create()
        {
            FileStream result = File.Create(base.NormalizedPath);
            Invalidate();
            return result;
        }

        public override void Delete()
        {
            FileSystem.DeleteFile(FullPath);
            Invalidate();
        }

        public FileStream Open(FileMode mode)
        {
            return Open(mode, (mode == FileMode.Append) ? FileAccess.Write : FileAccess.ReadWrite, FileShare.None);
        }

        public FileStream Open(FileMode mode, FileAccess access)
        {
            return Open(mode, access, FileShare.None);
        }

        public FileStream Open(FileMode mode, FileAccess access, FileShare share)
        {
            return new FileStream(base.NormalizedPath, mode, access, share);
        }

        public FileStream OpenRead()
        {
            return new FileStream(base.NormalizedPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: false);
        }

        public FileStream OpenWrite()
        {
            return new FileStream(base.NormalizedPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
        }

        public void MoveTo(string destFileName)
        {
            MoveTo(destFileName, overwrite: false);
        }

        public void MoveTo(string destFileName, bool overwrite)
        {
            if (destFileName == null)
            {
                throw new ArgumentNullException("destFileName");
            }
            if (destFileName.Length == 0)
            {
                throw new ArgumentException(MDCFR.Properties.Resources.Argument_EmptyFileName, "destFileName");
            }
            string fullPath = Path.GetFullPath(destFileName);
            if (!new DirectoryInfo(Path.GetDirectoryName(FullName)).Exists)
            {
                throw new DirectoryNotFoundException(System.SR.Format(MDCFR.Properties.Resources.IO_PathNotFound_Path, FullName));
            }
            if (!Exists)
            {
                throw new FileNotFoundException(System.SR.Format(MDCFR.Properties.Resources.IO_FileNotFound_FileName, FullName), FullName);
            }
            FileSystem.MoveFile(FullPath, fullPath, overwrite);
            FullPath = fullPath;
            OriginalPath = destFileName;
            _name = Path.GetFileName(fullPath);
            Invalidate();
        }

        public FileInfo Replace(string destinationFileName, string? destinationBackupFileName)
        {
            return Replace(destinationFileName, destinationBackupFileName, ignoreMetadataErrors: false);
        }

        public FileInfo Replace(string destinationFileName, string? destinationBackupFileName, bool ignoreMetadataErrors)
        {
            if (destinationFileName == null)
            {
                throw new ArgumentNullException("destinationFileName");
            }
            FileSystem.ReplaceFile(FullPath, Path.GetFullPath(destinationFileName), (destinationBackupFileName != null) ? Path.GetFullPath(destinationBackupFileName) : null, ignoreMetadataErrors);
            return new FileInfo(destinationFileName);
        }

        [SupportedOSPlatform("windows")]
        public void Decrypt()
        {
            File.Decrypt(FullPath);
        }

        [SupportedOSPlatform("windows")]
        public void Encrypt()
        {
            File.Encrypt(FullPath);
        }
    }

    internal static class FileSystem
    {
        internal static void VerifyValidPath(string path, string argName)
        {
            if (path == null)
            {
                throw new ArgumentNullException(argName);
            }
            if (path.Length == 0)
            {
                throw new ArgumentException(MDCFR.Properties.Resources.Arg_PathEmpty, argName);
            }
            if (path.Contains('\0'))
            {
                throw new ArgumentException(MDCFR.Properties.Resources.Argument_InvalidPathChars, argName);
            }
        }

        public static void Encrypt(string path)
        {
            string fullPath = Path.GetFullPath(path);
            if (!global::Interop.Advapi32.EncryptFile(fullPath))
            {
                ThrowExceptionEncryptDecryptFail(fullPath);
            }
        }

        public static void Decrypt(string path)
        {
            string fullPath = Path.GetFullPath(path);
            if (!global::Interop.Advapi32.DecryptFile(fullPath))
            {
                ThrowExceptionEncryptDecryptFail(fullPath);
            }
        }

        private unsafe static void ThrowExceptionEncryptDecryptFail(string fullPath)
        {
            int lastWin32Error = Marshal.GetLastWin32Error();
            if (lastWin32Error == 5)
            {
                string text = DriveInfoInternal.NormalizeDriveName(Path.GetPathRoot(fullPath));
                using (DisableMediaInsertionPrompt.Create())
                {
                    if (!global::Interop.Kernel32.GetVolumeInformation(text, null, 0, null, null, out var fileSystemFlags, null, 0))
                    {
                        lastWin32Error = Marshal.GetLastWin32Error();
                        throw Win32Marshal.GetExceptionForWin32Error(lastWin32Error, text);
                    }
                    if (((ulong)fileSystemFlags & 0x20000uL) == 0L)
                    {
                        throw new NotSupportedException(MDCFR.Properties.Resources.PlatformNotSupported_FileEncryption);
                    }
                }
            }
            throw Win32Marshal.GetExceptionForWin32Error(lastWin32Error, fullPath);
        }

        public static void CopyFile(string sourceFullPath, string destFullPath, bool overwrite)
        {
            int num = global::Interop.Kernel32.CopyFile(sourceFullPath, destFullPath, !overwrite);
            if (num == 0)
            {
                return;
            }
            string path = destFullPath;
            if (num != 80)
            {
                using (SafeFileHandle safeFileHandle = global::Interop.Kernel32.CreateFile(sourceFullPath, int.MinValue, FileShare.Read, FileMode.Open, 0))
                {
                    if (safeFileHandle.IsInvalid)
                    {
                        path = sourceFullPath;
                    }
                }
                if (num == 5 && DirectoryExists(destFullPath))
                {
                    throw new IOException(System.SR.Format(MDCFR.Properties.Resources.Arg_FileIsDirectory_Name, destFullPath), 5);
                }
            }
            throw Win32Marshal.GetExceptionForWin32Error(num, path);
        }

        public static void ReplaceFile(string sourceFullPath, string destFullPath, string destBackupFullPath, bool ignoreMetadataErrors)
        {
            int dwReplaceFlags = (ignoreMetadataErrors ? 2 : 0);
            if (!global::Interop.Kernel32.ReplaceFile(destFullPath, sourceFullPath, destBackupFullPath, dwReplaceFlags, IntPtr.Zero, IntPtr.Zero))
            {
                throw Win32Marshal.GetExceptionForWin32Error(Marshal.GetLastWin32Error());
            }
        }

        public static void DeleteFile(string fullPath)
        {
            if (!global::Interop.Kernel32.DeleteFile(fullPath))
            {
                int lastWin32Error = Marshal.GetLastWin32Error();
                if (lastWin32Error != 2)
                {
                    throw Win32Marshal.GetExceptionForWin32Error(lastWin32Error, fullPath);
                }
            }
        }

        public static FileAttributes GetAttributes(string fullPath)
        {
            global::Interop.Kernel32.WIN32_FILE_ATTRIBUTE_DATA data = default(global::Interop.Kernel32.WIN32_FILE_ATTRIBUTE_DATA);
            int num = FillAttributeInfo(fullPath, ref data, returnErrorOnNotFound: true);
            if (num != 0)
            {
                throw Win32Marshal.GetExceptionForWin32Error(num, fullPath);
            }
            return (FileAttributes)data.dwFileAttributes;
        }

        public static DateTimeOffset GetCreationTime(string fullPath)
        {
            global::Interop.Kernel32.WIN32_FILE_ATTRIBUTE_DATA data = default(global::Interop.Kernel32.WIN32_FILE_ATTRIBUTE_DATA);
            int num = FillAttributeInfo(fullPath, ref data, returnErrorOnNotFound: false);
            if (num != 0)
            {
                throw Win32Marshal.GetExceptionForWin32Error(num, fullPath);
            }
            return data.ftCreationTime.ToDateTimeOffset();
        }

        public static FileSystemInfo GetFileSystemInfo(string fullPath, bool asDirectory)
        {
            if (!asDirectory)
            {
                return new FileInfo(fullPath, null, null, isNormalized: false);
            }
            return new DirectoryInfo(fullPath, null, null, isNormalized: false);
        }

        public static DateTimeOffset GetLastAccessTime(string fullPath)
        {
            global::Interop.Kernel32.WIN32_FILE_ATTRIBUTE_DATA data = default(global::Interop.Kernel32.WIN32_FILE_ATTRIBUTE_DATA);
            int num = FillAttributeInfo(fullPath, ref data, returnErrorOnNotFound: false);
            if (num != 0)
            {
                throw Win32Marshal.GetExceptionForWin32Error(num, fullPath);
            }
            return data.ftLastAccessTime.ToDateTimeOffset();
        }

        public static DateTimeOffset GetLastWriteTime(string fullPath)
        {
            global::Interop.Kernel32.WIN32_FILE_ATTRIBUTE_DATA data = default(global::Interop.Kernel32.WIN32_FILE_ATTRIBUTE_DATA);
            int num = FillAttributeInfo(fullPath, ref data, returnErrorOnNotFound: false);
            if (num != 0)
            {
                throw Win32Marshal.GetExceptionForWin32Error(num, fullPath);
            }
            return data.ftLastWriteTime.ToDateTimeOffset();
        }

        public static void MoveDirectory(string sourceFullPath, string destFullPath)
        {
            if (!global::Interop.Kernel32.MoveFile(sourceFullPath, destFullPath, overwrite: false))
            {
                int lastWin32Error = Marshal.GetLastWin32Error();
                switch (lastWin32Error)
                {
                    case 2:
                        throw Win32Marshal.GetExceptionForWin32Error(3, sourceFullPath);
                    case 183:
                        throw Win32Marshal.GetExceptionForWin32Error(183, destFullPath);
                    case 5:
                        throw new IOException(System.SR.Format(MDCFR.Properties.Resources.UnauthorizedAccess_IODenied_Path, sourceFullPath), Win32Marshal.MakeHRFromErrorCode(lastWin32Error));
                    default:
                        throw Win32Marshal.GetExceptionForWin32Error(lastWin32Error);
                }
            }
        }

        public static void MoveFile(string sourceFullPath, string destFullPath, bool overwrite)
        {
            if (!global::Interop.Kernel32.MoveFile(sourceFullPath, destFullPath, overwrite))
            {
                throw Win32Marshal.GetExceptionForLastWin32Error();
            }
        }

        private static SafeFileHandle OpenHandle(string fullPath, bool asDirectory)
        {
            string text = fullPath.Substring(0, System.IO.PathInternal.GetRootLength(MemoryExtensions.AsSpan(fullPath)));
            if (text == fullPath && text[1] == Path.VolumeSeparatorChar)
            {
                throw new ArgumentException(MDCFR.Properties.Resources.Arg_PathIsVolume, "path");
            }
            SafeFileHandle safeFileHandle = global::Interop.Kernel32.CreateFile(fullPath, 1073741824, FileShare.ReadWrite | FileShare.Delete, FileMode.Open, asDirectory ? 33554432 : 0);
            if (safeFileHandle.IsInvalid)
            {
                int num = Marshal.GetLastWin32Error();
                if (!asDirectory && num == 3 && fullPath.Equals(Directory.GetDirectoryRoot(fullPath)))
                {
                    num = 5;
                }
                throw Win32Marshal.GetExceptionForWin32Error(num, fullPath);
            }
            return safeFileHandle;
        }

        public static void RemoveDirectory(string fullPath, bool recursive)
        {
            if (!recursive)
            {
                RemoveDirectoryInternal(fullPath, topLevel: true);
                return;
            }
            global::Interop.Kernel32.WIN32_FIND_DATA findData = default(global::Interop.Kernel32.WIN32_FIND_DATA);
            GetFindData(fullPath, isDirectory: true, ignoreAccessDenied: true, ref findData);
            if (IsNameSurrogateReparsePoint(ref findData))
            {
                RemoveDirectoryInternal(fullPath, topLevel: true);
                return;
            }
            fullPath = System.IO.PathInternal.EnsureExtendedPrefix(fullPath);
            RemoveDirectoryRecursive(fullPath, ref findData, topLevel: true);
        }

        private static void GetFindData(string fullPath, bool isDirectory, bool ignoreAccessDenied, ref global::Interop.Kernel32.WIN32_FIND_DATA findData)
        {
            using Microsoft.Win32.SafeHandles.SafeFindHandle safeFindHandle = global::Interop.Kernel32.FindFirstFile(Path.TrimEndingDirectorySeparator(fullPath), ref findData);
            if (safeFindHandle.IsInvalid)
            {
                int num = Marshal.GetLastWin32Error();
                if (isDirectory && num == 2)
                {
                    num = 3;
                }
                if (!(isDirectory && num == 5 && ignoreAccessDenied))
                {
                    throw Win32Marshal.GetExceptionForWin32Error(num, fullPath);
                }
            }
        }

        private static bool IsNameSurrogateReparsePoint(ref global::Interop.Kernel32.WIN32_FIND_DATA data)
        {
            if ((data.dwFileAttributes & 0x400u) != 0)
            {
                return (data.dwReserved0 & 0x20000000) != 0;
            }
            return false;
        }

        private static void RemoveDirectoryRecursive(string fullPath, ref global::Interop.Kernel32.WIN32_FIND_DATA findData, bool topLevel)
        {
            Exception ex = null;
            using (Microsoft.Win32.SafeHandles.SafeFindHandle safeFindHandle = global::Interop.Kernel32.FindFirstFile(Path.Join(fullPath, "*"), ref findData))
            {
                if (safeFindHandle.IsInvalid)
                {
                    throw Win32Marshal.GetExceptionForLastWin32Error(fullPath);
                }
                int lastWin32Error;
                do
                {
                    if ((findData.dwFileAttributes & 0x10) == 0)
                    {
                        string stringFromFixedBuffer = findData.cFileName.GetStringFromFixedBuffer();
                        if (!global::Interop.Kernel32.DeleteFile(Path.Combine(fullPath, stringFromFixedBuffer)) && ex == null)
                        {
                            lastWin32Error = Marshal.GetLastWin32Error();
                            if (lastWin32Error != 2)
                            {
                                ex = Win32Marshal.GetExceptionForWin32Error(lastWin32Error, stringFromFixedBuffer);
                            }
                        }
                    }
                    else
                    {
                        if (findData.cFileName.FixedBufferEqualsString(".") || findData.cFileName.FixedBufferEqualsString(".."))
                        {
                            continue;
                        }
                        string stringFromFixedBuffer2 = findData.cFileName.GetStringFromFixedBuffer();
                        if (!IsNameSurrogateReparsePoint(ref findData))
                        {
                            try
                            {
                                RemoveDirectoryRecursive(Path.Combine(fullPath, stringFromFixedBuffer2), ref findData, topLevel: false);
                            }
                            catch (Exception ex2)
                            {
                                if (ex == null)
                                {
                                    ex = ex2;
                                }
                            }
                            continue;
                        }
                        if (findData.dwReserved0 == 2684354563u)
                        {
                            string mountPoint = Path.Join(fullPath, stringFromFixedBuffer2, "\\");
                            if (!global::Interop.Kernel32.DeleteVolumeMountPoint(mountPoint) && ex == null)
                            {
                                lastWin32Error = Marshal.GetLastWin32Error();
                                if (lastWin32Error != 0 && lastWin32Error != 3)
                                {
                                    ex = Win32Marshal.GetExceptionForWin32Error(lastWin32Error, stringFromFixedBuffer2);
                                }
                            }
                        }
                        if (!global::Interop.Kernel32.RemoveDirectory(Path.Combine(fullPath, stringFromFixedBuffer2)) && ex == null)
                        {
                            lastWin32Error = Marshal.GetLastWin32Error();
                            if (lastWin32Error != 3)
                            {
                                ex = Win32Marshal.GetExceptionForWin32Error(lastWin32Error, stringFromFixedBuffer2);
                            }
                        }
                    }
                }
                while (global::Interop.Kernel32.FindNextFile(safeFindHandle, ref findData));
                if (ex != null)
                {
                    throw ex;
                }
                lastWin32Error = Marshal.GetLastWin32Error();
                if (lastWin32Error != 0 && lastWin32Error != 18)
                {
                    throw Win32Marshal.GetExceptionForWin32Error(lastWin32Error, fullPath);
                }
            }
            RemoveDirectoryInternal(fullPath, topLevel, allowDirectoryNotEmpty: true);
        }

        private static void RemoveDirectoryInternal(string fullPath, bool topLevel, bool allowDirectoryNotEmpty = false)
        {
            if (global::Interop.Kernel32.RemoveDirectory(fullPath))
            {
                return;
            }
            int num = Marshal.GetLastWin32Error();
            switch (num)
            {
                case 2:
                    num = 3;
                    goto case 3;
                case 3:
                    if (!topLevel)
                    {
                        return;
                    }
                    break;
                case 145:
                    if (allowDirectoryNotEmpty)
                    {
                        return;
                    }
                    break;
                case 5:
                    throw new IOException(System.SR.Format(MDCFR.Properties.Resources.UnauthorizedAccess_IODenied_Path, fullPath));
            }
            throw Win32Marshal.GetExceptionForWin32Error(num, fullPath);
        }

        public static void SetAttributes(string fullPath, FileAttributes attributes)
        {
            if (!global::Interop.Kernel32.SetFileAttributes(fullPath, (int)attributes))
            {
                int lastWin32Error = Marshal.GetLastWin32Error();
                if (lastWin32Error == 87)
                {
                    throw new ArgumentException(MDCFR.Properties.Resources.Arg_InvalidFileAttrs, "attributes");
                }
                throw Win32Marshal.GetExceptionForWin32Error(lastWin32Error, fullPath);
            }
        }

        private unsafe static void SetFileTime(string fullPath, bool asDirectory, long creationTime = -1L, long lastAccessTime = -1L, long lastWriteTime = -1L, long changeTime = -1L, uint fileAttributes = 0u)
        {
            using SafeFileHandle hFile = OpenHandle(fullPath, asDirectory);
            global::Interop.Kernel32.FILE_BASIC_INFO fILE_BASIC_INFO = default(global::Interop.Kernel32.FILE_BASIC_INFO);
            fILE_BASIC_INFO.CreationTime = creationTime;
            fILE_BASIC_INFO.LastAccessTime = lastAccessTime;
            fILE_BASIC_INFO.LastWriteTime = lastWriteTime;
            fILE_BASIC_INFO.ChangeTime = changeTime;
            fILE_BASIC_INFO.FileAttributes = fileAttributes;
            global::Interop.Kernel32.FILE_BASIC_INFO fILE_BASIC_INFO2 = fILE_BASIC_INFO;
            if (!global::Interop.Kernel32.SetFileInformationByHandle(hFile, 0, &fILE_BASIC_INFO2, (uint)sizeof(global::Interop.Kernel32.FILE_BASIC_INFO)))
            {
                throw Win32Marshal.GetExceptionForLastWin32Error(fullPath);
            }
        }

        public static void SetCreationTime(string fullPath, DateTimeOffset time, bool asDirectory)
        {
            SetFileTime(fullPath, asDirectory, time.ToFileTime(), -1L, -1L, -1L);
        }

        public static void SetLastAccessTime(string fullPath, DateTimeOffset time, bool asDirectory)
        {
            SetFileTime(fullPath, asDirectory, -1L, time.ToFileTime(), -1L, -1L);
        }

        public static void SetLastWriteTime(string fullPath, DateTimeOffset time, bool asDirectory)
        {
            SetFileTime(fullPath, asDirectory, -1L, -1L, time.ToFileTime(), -1L);
        }

        public static string[] GetLogicalDrives()
        {
            return DriveInfoInternal.GetLogicalDrives();
        }

        internal static void CreateSymbolicLink(string path, string pathToTarget, bool isDirectory)
        {
            System.IO.PathInternal.GetLinkTargetFullPath(path, pathToTarget);
            global::Interop.Kernel32.CreateSymbolicLink(path, pathToTarget, isDirectory);
        }

        internal static FileSystemInfo ResolveLinkTarget(string linkPath, bool returnFinalTarget, bool isDirectory)
        {
            string text = (returnFinalTarget ? GetFinalLinkTarget(linkPath, isDirectory) : GetImmediateLinkTarget(linkPath, isDirectory, throwOnError: true, returnFullPath: true));
            if (text != null)
            {
                if (!isDirectory)
                {
                    return new FileInfo(text);
                }
                return new DirectoryInfo(text);
            }
            return null;
        }

        internal static string GetLinkTarget(string linkPath, bool isDirectory)
        {
            return GetImmediateLinkTarget(linkPath, isDirectory, throwOnError: false, returnFullPath: false);
        }

        /// <summary>
        /// Gets reparse point information associated to <paramref name="linkPath" />.
        /// </summary>
        /// <returns>The immediate link target, absolute or relative or null if the file is not a supported link.</returns>
        internal unsafe static string GetImmediateLinkTarget(string linkPath, bool isDirectory, bool throwOnError, bool returnFullPath)
        {
            using (SafeFileHandle safeFileHandle = OpenSafeFileHandle(linkPath, 35651584))
            {
                if (safeFileHandle.IsInvalid)
                {
                    if (!throwOnError)
                    {
                        return null;
                    }
                    int num = Marshal.GetLastWin32Error();
                    if (isDirectory && num == 2)
                    {
                        num = 3;
                    }
                    throw Win32Marshal.GetExceptionForWin32Error(num, linkPath);
                }
                byte[] array = ArrayPool<byte>.Shared.Rent(16384);
                try
                {
                    if (!global::Interop.Kernel32.DeviceIoControl(safeFileHandle, 589992u, IntPtr.Zero, 0u, array, 16384u, out var _, IntPtr.Zero))
                    {
                        if (!throwOnError)
                        {
                            return null;
                        }
                        int lastWin32Error = Marshal.GetLastWin32Error();
                        if (lastWin32Error == 4390)
                        {
                            return null;
                        }
                        throw Win32Marshal.GetExceptionForWin32Error(lastWin32Error, linkPath);
                    }
                    System.Span<byte> span = new System.Span<byte>(array);
                    global::Interop.Kernel32.SymbolicLinkReparseBuffer symbolicLinkReparseBuffer = default(global::Interop.Kernel32.SymbolicLinkReparseBuffer);
                    bool flag = MemoryMarshal.TryRead(span, out symbolicLinkReparseBuffer);
                    if (symbolicLinkReparseBuffer.ReparseTag == 2684354572u)
                    {
                        int num2 = sizeof(global::Interop.Kernel32.SymbolicLinkReparseBuffer) + symbolicLinkReparseBuffer.SubstituteNameOffset;
                        int substituteNameLength = symbolicLinkReparseBuffer.SubstituteNameLength;
                        System.Span<char> span2 = MemoryMarshal.Cast<byte, char>(span.Slice(num2, substituteNameLength));
                        if ((symbolicLinkReparseBuffer.Flags & 1) == 0)
                        {
                            if (MemoryExtensions.StartsWith<char>(span2, MemoryExtensions.AsSpan("\\??\\UNC\\")))
                            {
                                return Path.Join(MemoryExtensions.AsSpan("\\\\"), span2.Slice("\\??\\UNC\\".Length));
                            }
                            return GetTargetPathWithoutNTPrefix(span2);
                        }
                        if (returnFullPath)
                        {
                            return Path.Join(Path.GetDirectoryName(MemoryExtensions.AsSpan(linkPath)), span2);
                        }
                        return span2.ToString();
                    }
                    if (symbolicLinkReparseBuffer.ReparseTag == 2684354563u)
                    {
                        global::Interop.Kernel32.MountPointReparseBuffer mountPointReparseBuffer = default(global::Interop.Kernel32.MountPointReparseBuffer);
                        flag = MemoryMarshal.TryRead(span, out mountPointReparseBuffer);
                        int num3 = sizeof(global::Interop.Kernel32.MountPointReparseBuffer) + mountPointReparseBuffer.SubstituteNameOffset;
                        int substituteNameLength2 = mountPointReparseBuffer.SubstituteNameLength;
                        System.Span<char> span3 = MemoryMarshal.Cast<byte, char>(span.Slice(num3, substituteNameLength2));
                        return GetTargetPathWithoutNTPrefix(span3);
                    }
                    return null;
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(array);
                }
            }
            static string GetTargetPathWithoutNTPrefix(System.ReadOnlySpan<char> targetPath)
            {
                return targetPath.Slice("\\??\\".Length).ToString();
            }
        }

        private static string GetFinalLinkTarget(string linkPath, bool isDirectory)
        {
            global::Interop.Kernel32.WIN32_FIND_DATA findData = default(global::Interop.Kernel32.WIN32_FIND_DATA);
            GetFindData(linkPath, isDirectory, ignoreAccessDenied: false, ref findData);
            if ((findData.dwFileAttributes & 0x400) == 0 || (findData.dwReserved0 != 2684354572u && findData.dwReserved0 != 2684354563u))
            {
                return null;
            }
            using (SafeFileHandle safeFileHandle = OpenSafeFileHandle(linkPath, 33554435))
            {
                if (safeFileHandle.IsInvalid)
                {
                    int lastWin32Error = Marshal.GetLastWin32Error();
                    if (IsPathUnreachableError(lastWin32Error))
                    {
                        return GetFinalLinkTargetSlow(linkPath);
                    }
                    throw Win32Marshal.GetExceptionForWin32Error(lastWin32Error, linkPath);
                }
                char[] array = ArrayPool<char>.Shared.Rent(4096);
                try
                {
                    uint num = GetFinalPathNameByHandle(safeFileHandle, array);
                    if (num > array.Length)
                    {
                        char[] array2 = array;
                        array = ArrayPool<char>.Shared.Rent((int)num);
                        ArrayPool<char>.Shared.Return(array2);
                        num = GetFinalPathNameByHandle(safeFileHandle, array);
                    }
                    if (num == 0)
                    {
                        throw Win32Marshal.GetExceptionForLastWin32Error(linkPath);
                    }
                    int num2 = ((!System.IO.PathInternal.IsExtended(MemoryExtensions.AsSpan(linkPath))) ? 4 : 0);
                    return new string(array, num2, (int)num - num2);
                }
                finally
                {
                    ArrayPool<char>.Shared.Return(array);
                }
            }
            string GetFinalLinkTargetSlow(string linkPath)
            {
                string immediateLinkTarget = GetImmediateLinkTarget(linkPath, isDirectory, throwOnError: false, returnFullPath: true);
                string result = null;
                while (immediateLinkTarget != null)
                {
                    result = immediateLinkTarget;
                    immediateLinkTarget = GetImmediateLinkTarget(immediateLinkTarget, isDirectory, throwOnError: false, returnFullPath: true);
                }
                return result;
            }
            unsafe static uint GetFinalPathNameByHandle(SafeFileHandle handle, char[] buffer)
            {
                fixed (char* lpszFilePath = buffer)
                {
                    return global::Interop.Kernel32.GetFinalPathNameByHandle(handle, lpszFilePath, (uint)buffer.Length, 0u);
                }
            }
        }

        private unsafe static SafeFileHandle OpenSafeFileHandle(string path, int flags)
        {
            return global::Interop.Kernel32.CreateFile(path, 0, FileShare.ReadWrite | FileShare.Delete, (global::Interop.Kernel32.SECURITY_ATTRIBUTES*)(void*)IntPtr.Zero, FileMode.Open, flags, IntPtr.Zero);
        }

        public static bool DirectoryExists(string fullPath)
        {
            int lastError;
            return DirectoryExists(fullPath, out lastError);
        }

        private static bool DirectoryExists(string path, out int lastError)
        {
            global::Interop.Kernel32.WIN32_FILE_ATTRIBUTE_DATA data = default(global::Interop.Kernel32.WIN32_FILE_ATTRIBUTE_DATA);
            lastError = FillAttributeInfo(path, ref data, returnErrorOnNotFound: true);
            if (lastError == 0 && data.dwFileAttributes != -1)
            {
                return (data.dwFileAttributes & 0x10) != 0;
            }
            return false;
        }

        public static bool FileExists(string fullPath)
        {
            global::Interop.Kernel32.WIN32_FILE_ATTRIBUTE_DATA data = default(global::Interop.Kernel32.WIN32_FILE_ATTRIBUTE_DATA);
            if (FillAttributeInfo(fullPath, ref data, returnErrorOnNotFound: true) == 0 && data.dwFileAttributes != -1)
            {
                return (data.dwFileAttributes & 0x10) == 0;
            }
            return false;
        }

        /// <summary>
        /// Returns 0 on success, otherwise a Win32 error code.  Note that
        /// classes should use -1 as the uninitialized state for dataInitialized.
        /// </summary>
        /// <param name="path">The file path from which the file attribute information will be filled.</param>
        /// <param name="data">A struct that will contain the attribute information.</param>
        /// <param name="returnErrorOnNotFound">Return the error code for not found errors?</param>
        internal static int FillAttributeInfo(string path, ref global::Interop.Kernel32.WIN32_FILE_ATTRIBUTE_DATA data, bool returnErrorOnNotFound)
        {
            int num = 0;
            path = System.IO.PathInternal.TrimEndingDirectorySeparator(path);
            using (DisableMediaInsertionPrompt.Create())
            {
                if (!global::Interop.Kernel32.GetFileAttributesEx(path, global::Interop.Kernel32.GET_FILEEX_INFO_LEVELS.GetFileExInfoStandard, ref data))
                {
                    num = Marshal.GetLastWin32Error();
                    if (!IsPathUnreachableError(num))
                    {
                        global::Interop.Kernel32.WIN32_FIND_DATA data2 = default(global::Interop.Kernel32.WIN32_FIND_DATA);
                        using Microsoft.Win32.SafeHandles.SafeFindHandle safeFindHandle = global::Interop.Kernel32.FindFirstFile(path, ref data2);
                        if (safeFindHandle.IsInvalid)
                        {
                            num = Marshal.GetLastWin32Error();
                        }
                        else
                        {
                            num = 0;
                            data.PopulateFrom(ref data2);
                        }
                    }
                }
            }
            if (num != 0 && !returnErrorOnNotFound && ((uint)(num - 2) <= 1u || num == 21))
            {
                data.dwFileAttributes = -1;
                return 0;
            }
            return num;
        }

        internal static bool IsPathUnreachableError(int errorCode)
        {
            switch (errorCode)
            {
                case 2:
                case 3:
                case 6:
                case 21:
                case 53:
                case 65:
                case 67:
                case 87:
                case 123:
                case 161:
                case 206:
                case 1231:
                    return true;
                default:
                    return false;
            }
        }

        public unsafe static void CreateDirectory(string fullPath, byte[] securityDescriptor = null)
        {
            if (DirectoryExists(fullPath))
            {
                return;
            }
            List<string> list = new List<string>();
            bool flag = false;
            int num = fullPath.Length;
            if (num >= 2 && System.IO.PathInternal.EndsInDirectorySeparator(MemoryExtensions.AsSpan(fullPath)))
            {
                num--;
            }
            int rootLength = System.IO.PathInternal.GetRootLength(MemoryExtensions.AsSpan(fullPath));
            if (num > rootLength)
            {
                int num2 = num - 1;
                while (num2 >= rootLength && !flag)
                {
                    string text = fullPath.Substring(0, num2 + 1);
                    if (!DirectoryExists(text))
                    {
                        list.Add(text);
                    }
                    else
                    {
                        flag = true;
                    }
                    while (num2 > rootLength && !System.IO.PathInternal.IsDirectorySeparator(fullPath[num2]))
                    {
                        num2--;
                    }
                    num2--;
                }
            }
            int count = list.Count;
            bool flag2 = true;
            int num3 = 0;
            string path = fullPath;
            fixed (byte* ptr = securityDescriptor)
            {
                global::Interop.Kernel32.SECURITY_ATTRIBUTES sECURITY_ATTRIBUTES = default(global::Interop.Kernel32.SECURITY_ATTRIBUTES);
                sECURITY_ATTRIBUTES.nLength = (uint)sizeof(global::Interop.Kernel32.SECURITY_ATTRIBUTES);
                sECURITY_ATTRIBUTES.lpSecurityDescriptor = (IntPtr)ptr;
                global::Interop.Kernel32.SECURITY_ATTRIBUTES lpSecurityAttributes = sECURITY_ATTRIBUTES;
                while (list.Count > 0)
                {
                    string text2 = list[list.Count - 1];
                    list.RemoveAt(list.Count - 1);
                    flag2 = global::Interop.Kernel32.CreateDirectory(text2, ref lpSecurityAttributes);
                    if (!flag2 && num3 == 0)
                    {
                        int lastError = Marshal.GetLastWin32Error();
                        if (lastError != 183)
                        {
                            num3 = lastError;
                        }
                        else if (FileExists(text2) || (!DirectoryExists(text2, out lastError) && lastError == 5))
                        {
                            num3 = lastError;
                            path = text2;
                        }
                    }
                }
            }
            if (count == 0 && !flag)
            {
                string pathRoot = Path.GetPathRoot(fullPath);
                if (!DirectoryExists(pathRoot))
                {
                    throw Win32Marshal.GetExceptionForWin32Error(3, pathRoot);
                }
            }
            else if (!flag2 && num3 != 0)
            {
                throw Win32Marshal.GetExceptionForWin32Error(num3, path);
            }
        }
    }

    public abstract class FileSystemInfo : MarshalByRefObject, ISerializable
    {
        protected string FullPath;

        protected string OriginalPath;

        internal string _name;

        private string _linkTarget;

        private bool _linkTargetIsValid;

        private global::Interop.Kernel32.WIN32_FILE_ATTRIBUTE_DATA _data;

        private int _dataInitialized = -1;

        public virtual string FullName => FullPath;

        public string Extension
        {
            get
            {
                int length = FullPath.Length;
                int num = length;
                while (--num >= 0)
                {
                    char c = FullPath[num];
                    if (c == '.')
                    {
                        return FullPath.Substring(num, length - num);
                    }
                    if (System.IO.PathInternal.IsDirectorySeparator(c) || c == Path.VolumeSeparatorChar)
                    {
                        break;
                    }
                }
                return string.Empty;
            }
        }

        public virtual string Name => _name;

        public virtual bool Exists
        {
            get
            {
                try
                {
                    return ExistsCore;
                }
                catch
                {
                    return false;
                }
            }
        }

        public DateTime CreationTime
        {
            get
            {
                return CreationTimeUtc.ToLocalTime();
            }
            set
            {
                CreationTimeUtc = value.ToUniversalTime();
            }
        }

        public DateTime CreationTimeUtc
        {
            get
            {
                return CreationTimeCore.UtcDateTime;
            }
            set
            {
                CreationTimeCore = File.GetUtcDateTimeOffset(value);
            }
        }

        public DateTime LastAccessTime
        {
            get
            {
                return LastAccessTimeUtc.ToLocalTime();
            }
            set
            {
                LastAccessTimeUtc = value.ToUniversalTime();
            }
        }

        public DateTime LastAccessTimeUtc
        {
            get
            {
                return LastAccessTimeCore.UtcDateTime;
            }
            set
            {
                LastAccessTimeCore = File.GetUtcDateTimeOffset(value);
            }
        }

        public DateTime LastWriteTime
        {
            get
            {
                return LastWriteTimeUtc.ToLocalTime();
            }
            set
            {
                LastWriteTimeUtc = value.ToUniversalTime();
            }
        }

        public DateTime LastWriteTimeUtc
        {
            get
            {
                return LastWriteTimeCore.UtcDateTime;
            }
            set
            {
                LastWriteTimeCore = File.GetUtcDateTimeOffset(value);
            }
        }

        /// <summary>
        /// If this <see cref="T:Microsoft.IO.FileSystemInfo" /> instance represents a link, returns the link target's path.
        /// If a link does not exist in <see cref="P:Microsoft.IO.FileSystemInfo.FullName" />, or this instance does not represent a link, returns <see langword="null" />.
        /// </summary>
        public string? LinkTarget
        {
            get
            {
                if (_linkTargetIsValid)
                {
                    return _linkTarget;
                }
                _linkTarget = FileSystem.GetLinkTarget(FullPath, this is DirectoryInfo);
                _linkTargetIsValid = true;
                return _linkTarget;
            }
        }

        public FileAttributes Attributes
        {
            get
            {
                EnsureDataInitialized();
                return (FileAttributes)_data.dwFileAttributes;
            }
            set
            {
                FileSystem.SetAttributes(FullPath, value);
                _dataInitialized = -1;
            }
        }

        internal bool ExistsCore
        {
            get
            {
                if (_dataInitialized == -1)
                {
                    RefreshCore();
                }
                if (_dataInitialized != 0)
                {
                    return false;
                }
                if (_data.dwFileAttributes != -1)
                {
                    return this is DirectoryInfo == ((_data.dwFileAttributes & 0x10) == 16);
                }
                return false;
            }
        }

        internal DateTimeOffset CreationTimeCore
        {
            get
            {
                EnsureDataInitialized();
                return _data.ftCreationTime.ToDateTimeOffset();
            }
            set
            {
                FileSystem.SetCreationTime(FullPath, value, this is DirectoryInfo);
                _dataInitialized = -1;
            }
        }

        internal DateTimeOffset LastAccessTimeCore
        {
            get
            {
                EnsureDataInitialized();
                return _data.ftLastAccessTime.ToDateTimeOffset();
            }
            set
            {
                FileSystem.SetLastAccessTime(FullPath, value, this is DirectoryInfo);
                _dataInitialized = -1;
            }
        }

        internal DateTimeOffset LastWriteTimeCore
        {
            get
            {
                EnsureDataInitialized();
                return _data.ftLastWriteTime.ToDateTimeOffset();
            }
            set
            {
                FileSystem.SetLastWriteTime(FullPath, value, this is DirectoryInfo);
                _dataInitialized = -1;
            }
        }

        internal long LengthCore
        {
            get
            {
                EnsureDataInitialized();
                return (long)((ulong)_data.nFileSizeHigh << 32) | ((long)_data.nFileSizeLow & 0xFFFFFFFFL);
            }
        }

        internal string NormalizedPath
        {
            get
            {
                if (!System.IO.PathInternal.EndsWithPeriodOrSpace(FullPath))
                {
                    return FullPath;
                }
                return System.IO.PathInternal.EnsureExtendedPrefix(FullPath);
            }
        }

        protected FileSystemInfo(SerializationInfo info, StreamingContext context)
        {
            throw new PlatformNotSupportedException();
        }

        internal void Invalidate()
        {
            _linkTargetIsValid = false;
            InvalidateCore();
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new PlatformNotSupportedException();
        }

        public abstract void Delete();

        /// <summary>
        /// Creates a symbolic link located in <see cref="P:Microsoft.IO.FileSystemInfo.FullName" /> that points to the specified <paramref name="pathToTarget" />.
        /// </summary>
        /// <param name="pathToTarget">The path of the symbolic link target.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="pathToTarget" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="pathToTarget" /> is empty.
        /// -or-
        /// This instance was not created passing an absolute path.
        /// -or-
        /// <paramref name="pathToTarget" /> contains invalid path characters.</exception>
        /// <exception cref="T:System.IO.IOException">A file or directory already exists in the location of <see cref="P:Microsoft.IO.FileSystemInfo.FullName" />.
        /// -or-
        /// An I/O error occurred.</exception>
        public void CreateAsSymbolicLink(string pathToTarget)
        {
            FileSystem.VerifyValidPath(pathToTarget, "pathToTarget");
            FileSystem.CreateSymbolicLink(OriginalPath, pathToTarget, this is DirectoryInfo);
            Invalidate();
        }

        /// <summary>
        /// Gets the target of the specified link.
        /// </summary>
        /// <param name="returnFinalTarget"><see langword="true" /> to follow links to the final target; <see langword="false" /> to return the immediate next link.</param>
        /// <returns>A <see cref="T:Microsoft.IO.FileSystemInfo" /> instance if the link exists, independently if the target exists or not; <see langword="null" /> if this file or directory is not a link.</returns>
        /// <exception cref="T:System.IO.IOException">The file or directory does not exist.
        /// -or-
        /// The link's file system entry type is inconsistent with that of its target.
        /// -or-
        /// Too many levels of symbolic links.</exception>
        /// <remarks>When <paramref name="returnFinalTarget" /> is <see langword="true" />, the maximum number of symbolic links that are followed are 40 on Unix and 63 on Windows.</remarks>
        public FileSystemInfo? ResolveLinkTarget(bool returnFinalTarget)
        {
            return FileSystem.ResolveLinkTarget(FullPath, returnFinalTarget, this is DirectoryInfo);
        }

        /// <summary>
        /// Returns the original path. Use FullName or Name properties for the full path or file/directory name.
        /// </summary>
        public override string ToString()
        {
            return OriginalPath ?? string.Empty;
        }

        protected FileSystemInfo()
        {
        }

        internal unsafe static FileSystemInfo Create(string fullPath, ref FileSystemEntry findData)
        {
            FileSystemInfo fileSystemInfo = (findData.IsDirectory ? ((FileSystemInfo)new DirectoryInfo(fullPath, null, findData.FileName.ToString(), isNormalized: true)) : ((FileSystemInfo)new FileInfo(fullPath, null, findData.FileName.ToString(), isNormalized: true)));
            fileSystemInfo.Init(findData._info);
            return fileSystemInfo;
        }

        internal void InvalidateCore()
        {
            _dataInitialized = -1;
        }

        internal unsafe void Init(global::Interop.NtDll.FILE_FULL_DIR_INFORMATION* info)
        {
            _data.dwFileAttributes = (int)info->FileAttributes;
            _data.ftCreationTime = *(global::Interop.Kernel32.FILE_TIME*)(&info->CreationTime);
            _data.ftLastAccessTime = *(global::Interop.Kernel32.FILE_TIME*)(&info->LastAccessTime);
            _data.ftLastWriteTime = *(global::Interop.Kernel32.FILE_TIME*)(&info->LastWriteTime);
            _data.nFileSizeHigh = (uint)(info->EndOfFile >> 32);
            _data.nFileSizeLow = (uint)info->EndOfFile;
            _dataInitialized = 0;
        }

        private void EnsureDataInitialized()
        {
            if (_dataInitialized == -1)
            {
                _data = default(global::Interop.Kernel32.WIN32_FILE_ATTRIBUTE_DATA);
                RefreshCore();
            }
            if (_dataInitialized != 0)
            {
                throw Win32Marshal.GetExceptionForWin32Error(_dataInitialized, FullPath);
            }
        }

        public void Refresh()
        {
            _linkTargetIsValid = false;
            RefreshCore();
        }

        private void RefreshCore()
        {
            _dataInitialized = FileSystem.FillAttributeInfo(FullPath, ref _data, returnErrorOnNotFound: false);
        }
    }

    /// <summary>Specifies the type of character casing to match.</summary>
    public enum MatchCasing
    {
        /// <summary>Matches using the default casing for the given platform.</summary>
        PlatformDefault,
        /// <summary>Matches respecting character casing.</summary>
        CaseSensitive,
        /// <summary>Matches ignoring character casing.</summary>
        CaseInsensitive
    }

    /// <summary>Specifies the type of wildcard matching to use.</summary>
    public enum MatchType
    {
        /// <summary><para>Matches using '*' and '?' wildcards.</para><para>
        /// <c>*</c> matches from zero to any amount of characters. <c>?</c> matches exactly one character. <c>*.*</c> matches any name with a period in it (with <see cref="F:Microsoft.IO.MatchType.Win32" />, this would match all items).</para></summary>
        Simple,
        /// <summary><para>Match using Win32 DOS style matching semantics.</para><para>'*', '?', '&lt;', '&gt;', and '"' are all considered wildcards. Matches in a traditional DOS <c>/</c> Windows command prompt way. <c>*.*</c> matches all files. <c>?</c> matches collapse to periods. <c>file.??t</c> will match <c>file.t</c>, <c>file.at</c>, and <c>file.txt</c>.</para></summary>
        Win32
    }

    public static class Path
    {
        private readonly struct Join3Payload
        {
            public unsafe readonly char* First;

            public readonly int FirstLength;

            public unsafe readonly char* Second;

            public readonly int SecondLength;

            public unsafe readonly char* Third;

            public readonly int ThirdLength;

            public readonly byte Separators;

            public unsafe Join3Payload(char* first, int firstLength, char* second, int secondLength, char* third, int thirdLength, byte separators)
            {
                First = first;
                FirstLength = firstLength;
                Second = second;
                SecondLength = secondLength;
                Third = third;
                ThirdLength = thirdLength;
                Separators = separators;
            }
        }

        private readonly struct Join4Payload
        {
            public unsafe readonly char* First;

            public readonly int FirstLength;

            public unsafe readonly char* Second;

            public readonly int SecondLength;

            public unsafe readonly char* Third;

            public readonly int ThirdLength;

            public unsafe readonly char* Fourth;

            public readonly int FourthLength;

            public readonly byte Separators;

            public unsafe Join4Payload(char* first, int firstLength, char* second, int secondLength, char* third, int thirdLength, char* fourth, int fourthLength, byte separators)
            {
                First = first;
                FirstLength = firstLength;
                Second = second;
                SecondLength = secondLength;
                Third = third;
                ThirdLength = thirdLength;
                Fourth = fourth;
                FourthLength = fourthLength;
                Separators = separators;
            }
        }

        public static readonly char DirectorySeparatorChar = '\\';

        public static readonly char AltDirectorySeparatorChar = '/';

        public static readonly char VolumeSeparatorChar = ':';

        public static readonly char PathSeparator = ';';

        private const int KeyLength = 8;

        [Obsolete("Path.InvalidPathChars has been deprecated. Use GetInvalidPathChars or GetInvalidFileNameChars instead.")]
        public static readonly char[] InvalidPathChars = GetInvalidPathChars();

        private static System.ReadOnlySpan<byte> Base32Char => (System.ReadOnlySpan<byte>)new byte[32]
        {
            97, 98, 99, 100, 101, 102, 103, 104, 105, 106,
            107, 108, 109, 110, 111, 112, 113, 114, 115, 116,
            117, 118, 119, 120, 121, 122, 48, 49, 50, 51,
            52, 53
        };

        [return: NotNullIfNotNull("path")]
        public static string? ChangeExtension(string? path, string? extension)
        {
            if (path == null)
            {
                return null;
            }
            int num = path.Length;
            if (num == 0)
            {
                return string.Empty;
            }
            for (int num2 = path.Length - 1; num2 >= 0; num2--)
            {
                char c = path[num2];
                if (c == '.')
                {
                    num = num2;
                    break;
                }
                if (System.IO.PathInternal.IsDirectorySeparator(c))
                {
                    break;
                }
            }
            if (extension == null)
            {
                return path.Substring(0, num);
            }
            System.ReadOnlySpan<char> str = MemoryExtensions.AsSpan(path, 0, num);
            if (extension.Length == 0 || extension[0] != '.')
            {
                return StringExtensions.Concat(str, MemoryExtensions.AsSpan("."), MemoryExtensions.AsSpan(extension));
            }
            return StringExtensions.Concat(str, MemoryExtensions.AsSpan(extension));
        }

        /// <summary>
        /// Returns the directory portion of a file path. This method effectively
        /// removes the last segment of the given file path, i.e. it returns a
        /// string consisting of all characters up to but not including the last
        /// backslash ("\") in the file path. The returned value is null if the
        /// specified path is null, empty, or a root (such as "\", "C:", or
        /// "\\server\share").
        /// </summary>
        /// <remarks>
        /// Directory separators are normalized in the returned string.
        /// </remarks>
        public static string? GetDirectoryName(string? path)
        {
            if (path == null || System.IO.PathInternal.IsEffectivelyEmpty(MemoryExtensions.AsSpan(path)))
            {
                return null;
            }
            int directoryNameOffset = GetDirectoryNameOffset(MemoryExtensions.AsSpan(path));
            if (directoryNameOffset < 0)
            {
                return null;
            }
            return System.IO.PathInternal.NormalizeDirectorySeparators(path.Substring(0, directoryNameOffset));
        }

        /// <summary>
        /// Returns the directory portion of a file path. The returned value is empty
        /// if the specified path is null, empty, or a root (such as "\", "C:", or
        /// "\\server\share").
        /// </summary>
        /// <remarks>
        /// Unlike the string overload, this method will not normalize directory separators.
        /// </remarks>
        public static System.ReadOnlySpan<char> GetDirectoryName(System.ReadOnlySpan<char> path)
        {
            if (System.IO.PathInternal.IsEffectivelyEmpty(path))
            {
                return System.ReadOnlySpan<char>.Empty;
            }
            int directoryNameOffset = GetDirectoryNameOffset(path);
            if (directoryNameOffset < 0)
            {
                return System.ReadOnlySpan<char>.Empty;
            }
            return path.Slice(0, directoryNameOffset);
        }

        internal unsafe static int GetDirectoryNameOffset(System.ReadOnlySpan<char> path)
        {
            int rootLength = System.IO.PathInternal.GetRootLength(path);
            int num = path.Length;
            if (num <= rootLength)
            {
                return -1;
            }
            while (num > rootLength && !System.IO.PathInternal.IsDirectorySeparator(path[--num]))
            {
            }
            while (num > rootLength && System.IO.PathInternal.IsDirectorySeparator(path[num - 1]))
            {
                num--;
            }
            return num;
        }

        /// <summary>
        /// Returns the extension of the given path. The returned value includes the period (".") character of the
        /// extension except when you have a terminal period when you get string.Empty, such as ".exe" or ".cpp".
        /// The returned value is null if the given path is null or empty if the given path does not include an
        /// extension.
        /// </summary>
        [return: NotNullIfNotNull("path")]
        public static string? GetExtension(string? path)
        {
            if (path == null)
            {
                return null;
            }
            return GetExtension(MemoryExtensions.AsSpan(path)).ToString();
        }

        /// <summary>
        /// Returns the extension of the given path.
        /// </summary>
        /// <remarks>
        /// The returned value is an empty ReadOnlySpan if the given path does not include an extension.
        /// </remarks>
        public unsafe static System.ReadOnlySpan<char> GetExtension(System.ReadOnlySpan<char> path)
        {
            int length = path.Length;
            for (int num = length - 1; num >= 0; num--)
            {
                char c = path[num];
                if (c == '.')
                {
                    if (num != length - 1)
                    {
                        return path.Slice(num, length - num);
                    }
                    return System.ReadOnlySpan<char>.Empty;
                }
                if (System.IO.PathInternal.IsDirectorySeparator(c))
                {
                    break;
                }
            }
            return System.ReadOnlySpan<char>.Empty;
        }

        /// <summary>
        /// Returns the name and extension parts of the given path. The resulting string contains
        /// the characters of path that follow the last separator in path. The resulting string is
        /// null if path is null.
        /// </summary>
        [return: NotNullIfNotNull("path")]
        public static string? GetFileName(string? path)
        {
            if (path == null)
            {
                return null;
            }
            System.ReadOnlySpan<char> fileName = GetFileName(MemoryExtensions.AsSpan(path));
            if (path.Length == fileName.Length)
            {
                return path;
            }
            return fileName.ToString();
        }

        /// <summary>
        /// The returned ReadOnlySpan contains the characters of the path that follows the last separator in path.
        /// </summary>
        public unsafe static System.ReadOnlySpan<char> GetFileName(System.ReadOnlySpan<char> path)
        {
            int length = GetPathRoot(path).Length;
            int num = path.Length;
            while (--num >= 0)
            {
                if (num < length || System.IO.PathInternal.IsDirectorySeparator(path[num]))
                {
                    return path.Slice(num + 1, path.Length - num - 1);
                }
            }
            return path;
        }

        [return: NotNullIfNotNull("path")]
        public static string? GetFileNameWithoutExtension(string? path)
        {
            if (path == null)
            {
                return null;
            }
            System.ReadOnlySpan<char> fileNameWithoutExtension = GetFileNameWithoutExtension(MemoryExtensions.AsSpan(path));
            if (path.Length == fileNameWithoutExtension.Length)
            {
                return path;
            }
            return fileNameWithoutExtension.ToString();
        }

        /// <summary>
        /// Returns the characters between the last separator and last (.) in the path.
        /// </summary>
        public static System.ReadOnlySpan<char> GetFileNameWithoutExtension(System.ReadOnlySpan<char> path)
        {
            System.ReadOnlySpan<char> fileName = GetFileName(path);
            int num = MemoryExtensions.LastIndexOf<char>(fileName, '.');
            if (num != -1)
            {
                return fileName.Slice(0, num);
            }
            return fileName;
        }

        /// <summary>
        /// Returns a cryptographically strong random 8.3 string that can be
        /// used as either a folder name or a file name.
        /// </summary>
        public unsafe static string GetRandomFileName()
        {
            byte* ptr = stackalloc byte[8];
            global::Interop.GetRandomBytes(ptr, 8);
            return StringExtensions.Create(12, (IntPtr)ptr, delegate (System.Span<char> span, IntPtr key)
            {
                Populate83FileNameFromRandomBytes((byte*)(void*)key, 8, span);
            });
        }

        /// <summary>
        /// Returns true if the path is fixed to a specific drive or UNC path. This method does no
        /// validation of the path (URIs will be returned as relative as a result).
        /// Returns false if the path specified is relative to the current drive or working directory.
        /// </summary>
        /// <remarks>
        /// Handles paths that use the alternate directory separator.  It is a frequent mistake to
        /// assume that rooted paths <see cref="M:Microsoft.IO.Path.IsPathRooted(System.String)" /> are not relative.  This isn't the case.
        /// "C:a" is drive relative- meaning that it will be resolved against the current directory
        /// for C: (rooted, but relative). "C:\a" is rooted and not relative (the current directory
        /// will not be used to modify the path).
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        /// Thrown if <paramref name="path" /> is null.
        /// </exception>
        public static bool IsPathFullyQualified(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            return IsPathFullyQualified(MemoryExtensions.AsSpan(path));
        }

        public static bool IsPathFullyQualified(System.ReadOnlySpan<char> path)
        {
            return !System.IO.PathInternal.IsPartiallyQualified(path);
        }

        /// <summary>
        /// Tests if a path's file name includes a file extension. A trailing period
        /// is not considered an extension.
        /// </summary>
        public static bool HasExtension([NotNullWhen(true)] string? path)
        {
            if (path != null)
            {
                return HasExtension(MemoryExtensions.AsSpan(path));
            }
            return false;
        }

        public unsafe static bool HasExtension(System.ReadOnlySpan<char> path)
        {
            for (int num = path.Length - 1; num >= 0; num--)
            {
                char c = path[num];
                if (c == '.')
                {
                    return num != path.Length - 1;
                }
                if (System.IO.PathInternal.IsDirectorySeparator(c))
                {
                    break;
                }
            }
            return false;
        }

        public static string Combine(string path1, string path2)
        {
            if (path1 == null || path2 == null)
            {
                throw new ArgumentNullException((path1 == null) ? "path1" : "path2");
            }
            return CombineInternal(path1, path2);
        }

        public static string Combine(string path1, string path2, string path3)
        {
            if (path1 == null || path2 == null || path3 == null)
            {
                throw new ArgumentNullException((path1 == null) ? "path1" : ((path2 == null) ? "path2" : "path3"));
            }
            return CombineInternal(path1, path2, path3);
        }

        public static string Combine(string path1, string path2, string path3, string path4)
        {
            if (path1 == null || path2 == null || path3 == null || path4 == null)
            {
                throw new ArgumentNullException((path1 == null) ? "path1" : ((path2 == null) ? "path2" : ((path3 == null) ? "path3" : "path4")));
            }
            return CombineInternal(path1, path2, path3, path4);
        }

        public static string Combine(params string[] paths)
        {
            if (paths == null)
            {
                throw new ArgumentNullException("paths");
            }
            int num = 0;
            int num2 = 0;
            for (int i = 0; i < paths.Length; i++)
            {
                if (paths[i] == null)
                {
                    throw new ArgumentNullException("paths");
                }
                if (paths[i].Length != 0)
                {
                    if (IsPathRooted(paths[i]))
                    {
                        num2 = i;
                        num = paths[i].Length;
                    }
                    else
                    {
                        num += paths[i].Length;
                    }
                    char c = paths[i][paths[i].Length - 1];
                    if (!System.IO.PathInternal.IsDirectorySeparator(c))
                    {
                        num++;
                    }
                }
            }
            System.Span<char> initialBuffer = stackalloc char[260];
            ValueStringBuilder valueStringBuilder = new ValueStringBuilder(initialBuffer);
            valueStringBuilder.EnsureCapacity(num);
            for (int j = num2; j < paths.Length; j++)
            {
                if (paths[j].Length == 0)
                {
                    continue;
                }
                if (valueStringBuilder.Length == 0)
                {
                    valueStringBuilder.Append(paths[j]);
                    continue;
                }
                char c2 = valueStringBuilder[valueStringBuilder.Length - 1];
                if (!System.IO.PathInternal.IsDirectorySeparator(c2))
                {
                    valueStringBuilder.Append('\\');
                }
                valueStringBuilder.Append(paths[j]);
            }
            return valueStringBuilder.ToString();
        }

        public static string Join(System.ReadOnlySpan<char> path1, System.ReadOnlySpan<char> path2)
        {
            if (path1.Length == 0)
            {
                return path2.ToString();
            }
            if (path2.Length == 0)
            {
                return path1.ToString();
            }
            return JoinInternal(path1, path2);
        }

        public static string Join(System.ReadOnlySpan<char> path1, System.ReadOnlySpan<char> path2, System.ReadOnlySpan<char> path3)
        {
            if (path1.Length == 0)
            {
                return Join(path2, path3);
            }
            if (path2.Length == 0)
            {
                return Join(path1, path3);
            }
            if (path3.Length == 0)
            {
                return Join(path1, path2);
            }
            return JoinInternal(path1, path2, path3);
        }

        public static string Join(System.ReadOnlySpan<char> path1, System.ReadOnlySpan<char> path2, System.ReadOnlySpan<char> path3, System.ReadOnlySpan<char> path4)
        {
            if (path1.Length == 0)
            {
                return Join(path2, path3, path4);
            }
            if (path2.Length == 0)
            {
                return Join(path1, path3, path4);
            }
            if (path3.Length == 0)
            {
                return Join(path1, path2, path4);
            }
            if (path4.Length == 0)
            {
                return Join(path1, path2, path3);
            }
            return JoinInternal(path1, path2, path3, path4);
        }

        public static string Join(string? path1, string? path2)
        {
            return Join(MemoryExtensions.AsSpan(path1), MemoryExtensions.AsSpan(path2));
        }

        public static string Join(string? path1, string? path2, string? path3)
        {
            return Join(MemoryExtensions.AsSpan(path1), MemoryExtensions.AsSpan(path2), MemoryExtensions.AsSpan(path3));
        }

        public static string Join(string? path1, string? path2, string? path3, string? path4)
        {
            return Join(MemoryExtensions.AsSpan(path1), MemoryExtensions.AsSpan(path2), MemoryExtensions.AsSpan(path3), MemoryExtensions.AsSpan(path4));
        }

        public static string Join(params string?[] paths)
        {
            if (paths == null)
            {
                throw new ArgumentNullException("paths");
            }
            if (paths.Length == 0)
            {
                return string.Empty;
            }
            int num = 0;
            for (int i = 0; i < paths.Length; i++)
            {
                num += paths[i]?.Length ?? 0;
            }
            num += paths.Length - 1;
            System.Span<char> initialBuffer = stackalloc char[260];
            ValueStringBuilder valueStringBuilder = new ValueStringBuilder(initialBuffer);
            valueStringBuilder.EnsureCapacity(num);
            foreach (string text in paths)
            {
                if (string.IsNullOrEmpty(text))
                {
                    continue;
                }
                if (valueStringBuilder.Length == 0)
                {
                    valueStringBuilder.Append(text);
                    continue;
                }
                if (!System.IO.PathInternal.IsDirectorySeparator(valueStringBuilder[valueStringBuilder.Length - 1]) && !System.IO.PathInternal.IsDirectorySeparator(text[0]))
                {
                    valueStringBuilder.Append('\\');
                }
                valueStringBuilder.Append(text);
            }
            return valueStringBuilder.ToString();
        }

        public static bool TryJoin(System.ReadOnlySpan<char> path1, System.ReadOnlySpan<char> path2, System.Span<char> destination, out int charsWritten)
        {
            charsWritten = 0;
            if (path1.Length == 0 && path2.Length == 0)
            {
                return true;
            }
            if (path1.Length == 0 || path2.Length == 0)
            {
                ref System.ReadOnlySpan<char> reference = ref path1.Length == 0 ? ref path2 : ref path1;
                if (destination.Length < reference.Length)
                {
                    return false;
                }
                reference.CopyTo(destination);
                charsWritten = reference.Length;
                return true;
            }
            bool flag = !EndsInDirectorySeparator(path1) && !System.IO.PathInternal.StartsWithDirectorySeparator(path2);
            int num = path1.Length + path2.Length + (flag ? 1 : 0);
            if (destination.Length < num)
            {
                return false;
            }
            path1.CopyTo(destination);
            if (flag)
            {
                destination[path1.Length] = DirectorySeparatorChar;
            }
            path2.CopyTo(destination.Slice(path1.Length + (flag ? 1 : 0)));
            charsWritten = num;
            return true;
        }

        public static bool TryJoin(System.ReadOnlySpan<char> path1, System.ReadOnlySpan<char> path2, System.ReadOnlySpan<char> path3, System.Span<char> destination, out int charsWritten)
        {
            charsWritten = 0;
            if (path1.Length == 0 && path2.Length == 0 && path3.Length == 0)
            {
                return true;
            }
            if (path1.Length == 0)
            {
                return TryJoin(path2, path3, destination, out charsWritten);
            }
            if (path2.Length == 0)
            {
                return TryJoin(path1, path3, destination, out charsWritten);
            }
            if (path3.Length == 0)
            {
                return TryJoin(path1, path2, destination, out charsWritten);
            }
            int num = ((!EndsInDirectorySeparator(path1) && !System.IO.PathInternal.StartsWithDirectorySeparator(path2)) ? 1 : 0);
            bool flag = !EndsInDirectorySeparator(path2) && !System.IO.PathInternal.StartsWithDirectorySeparator(path3);
            if (flag)
            {
                num++;
            }
            int num2 = path1.Length + path2.Length + path3.Length + num;
            if (destination.Length < num2)
            {
                return false;
            }
            bool flag2 = TryJoin(path1, path2, destination, out charsWritten);
            if (flag)
            {
                destination[charsWritten++] = DirectorySeparatorChar;
            }
            path3.CopyTo(destination.Slice(charsWritten));
            charsWritten += path3.Length;
            return true;
        }

        private static string CombineInternal(string first, string second)
        {
            if (string.IsNullOrEmpty(first))
            {
                return second;
            }
            if (string.IsNullOrEmpty(second))
            {
                return first;
            }
            if (IsPathRooted(MemoryExtensions.AsSpan(second)))
            {
                return second;
            }
            return JoinInternal(MemoryExtensions.AsSpan(first), MemoryExtensions.AsSpan(second));
        }

        private static string CombineInternal(string first, string second, string third)
        {
            if (string.IsNullOrEmpty(first))
            {
                return CombineInternal(second, third);
            }
            if (string.IsNullOrEmpty(second))
            {
                return CombineInternal(first, third);
            }
            if (string.IsNullOrEmpty(third))
            {
                return CombineInternal(first, second);
            }
            if (IsPathRooted(MemoryExtensions.AsSpan(third)))
            {
                return third;
            }
            if (IsPathRooted(MemoryExtensions.AsSpan(second)))
            {
                return CombineInternal(second, third);
            }
            return JoinInternal(MemoryExtensions.AsSpan(first), MemoryExtensions.AsSpan(second), MemoryExtensions.AsSpan(third));
        }

        private static string CombineInternal(string first, string second, string third, string fourth)
        {
            if (string.IsNullOrEmpty(first))
            {
                return CombineInternal(second, third, fourth);
            }
            if (string.IsNullOrEmpty(second))
            {
                return CombineInternal(first, third, fourth);
            }
            if (string.IsNullOrEmpty(third))
            {
                return CombineInternal(first, second, fourth);
            }
            if (string.IsNullOrEmpty(fourth))
            {
                return CombineInternal(first, second, third);
            }
            if (IsPathRooted(MemoryExtensions.AsSpan(fourth)))
            {
                return fourth;
            }
            if (IsPathRooted(MemoryExtensions.AsSpan(third)))
            {
                return CombineInternal(third, fourth);
            }
            if (IsPathRooted(MemoryExtensions.AsSpan(second)))
            {
                return CombineInternal(second, third, fourth);
            }
            return JoinInternal(MemoryExtensions.AsSpan(first), MemoryExtensions.AsSpan(second), MemoryExtensions.AsSpan(third), MemoryExtensions.AsSpan(fourth));
        }

        private unsafe static string JoinInternal(System.ReadOnlySpan<char> first, System.ReadOnlySpan<char> second)
        {
            bool flag = System.IO.PathInternal.IsDirectorySeparator(first[first.Length - 1]) || System.IO.PathInternal.IsDirectorySeparator(second[0]);
            fixed (char* ptr = &MemoryMarshal.GetReference<char>(first))
            {
                fixed (char* ptr2 = &MemoryMarshal.GetReference<char>(second))
                {
                    return StringExtensions.Create(first.Length + second.Length + ((!flag) ? 1 : 0), ((IntPtr)ptr, first.Length, (IntPtr)ptr2, second.Length), delegate (System.Span<char> destination, (IntPtr First, int FirstLength, IntPtr Second, int SecondLength) state)
                    {
                        new System.Span<char>((void*)state.First, state.FirstLength).CopyTo(destination);
                        if (destination.Length != state.FirstLength + state.SecondLength)
                        {
                            destination[state.FirstLength] = '\\';
                        }
                        new System.Span<char>((void*)state.Second, state.SecondLength).CopyTo(destination.Slice(destination.Length - state.SecondLength));
                    });
                }
            }
        }

        private unsafe static string JoinInternal(System.ReadOnlySpan<char> first, System.ReadOnlySpan<char> second, System.ReadOnlySpan<char> third)
        {
            byte b = (byte)((!System.IO.PathInternal.IsDirectorySeparator(first[first.Length - 1]) && !System.IO.PathInternal.IsDirectorySeparator(second[0])) ? 1 : 0);
            byte b2 = (byte)((!System.IO.PathInternal.IsDirectorySeparator(second[second.Length - 1]) && !System.IO.PathInternal.IsDirectorySeparator(third[0])) ? 1 : 0);
            fixed (char* first2 = &MemoryMarshal.GetReference<char>(first))
            {
                fixed (char* second2 = &MemoryMarshal.GetReference<char>(second))
                {
                    fixed (char* third2 = &MemoryMarshal.GetReference<char>(third))
                    {
                        Join3Payload join3Payload = new Join3Payload(first2, first.Length, second2, second.Length, third2, third.Length, (byte)(b | (b2 << 1)));
                        return StringExtensions.Create(first.Length + second.Length + third.Length + b + b2, (IntPtr)(&join3Payload), delegate (System.Span<char> destination, IntPtr statePtr)
                        {
                            ref Join3Payload reference = ref *(Join3Payload*)(void*)statePtr;
                            new System.Span<char>((void*)reference.First, reference.FirstLength).CopyTo(destination);
                            if (((uint)reference.Separators & (true ? 1u : 0u)) != 0)
                            {
                                destination[reference.FirstLength] = '\\';
                            }
                            new System.Span<char>((void*)reference.Second, reference.SecondLength).CopyTo(destination.Slice(reference.FirstLength + (reference.Separators & 1)));
                            if ((reference.Separators & 2u) != 0)
                            {
                                destination[destination.Length - reference.ThirdLength - 1] = '\\';
                            }
                            new System.Span<char>((void*)reference.Third, reference.ThirdLength).CopyTo(destination.Slice(destination.Length - reference.ThirdLength));
                        });
                    }
                }
            }
        }

        private unsafe static string JoinInternal(System.ReadOnlySpan<char> first, System.ReadOnlySpan<char> second, System.ReadOnlySpan<char> third, System.ReadOnlySpan<char> fourth)
        {
            byte b = (byte)((!System.IO.PathInternal.IsDirectorySeparator(first[first.Length - 1]) && !System.IO.PathInternal.IsDirectorySeparator(second[0])) ? 1 : 0);
            byte b2 = (byte)((!System.IO.PathInternal.IsDirectorySeparator(second[second.Length - 1]) && !System.IO.PathInternal.IsDirectorySeparator(third[0])) ? 1 : 0);
            byte b3 = (byte)((!System.IO.PathInternal.IsDirectorySeparator(third[third.Length - 1]) && !System.IO.PathInternal.IsDirectorySeparator(fourth[0])) ? 1 : 0);
            fixed (char* first2 = &MemoryMarshal.GetReference<char>(first))
            {
                fixed (char* second2 = &MemoryMarshal.GetReference<char>(second))
                {
                    fixed (char* third2 = &MemoryMarshal.GetReference<char>(third))
                    {
                        fixed (char* fourth2 = &MemoryMarshal.GetReference<char>(fourth))
                        {
                            Join4Payload join4Payload = new Join4Payload(first2, first.Length, second2, second.Length, third2, third.Length, fourth2, fourth.Length, (byte)(b | (b2 << 1) | (b3 << 2)));
                            return StringExtensions.Create(first.Length + second.Length + third.Length + fourth.Length + b + b2 + b3, (IntPtr)(&join4Payload), delegate (System.Span<char> destination, IntPtr statePtr)
                            {
                                ref Join4Payload reference = ref *(Join4Payload*)(void*)statePtr;
                                new System.Span<char>((void*)reference.First, reference.FirstLength).CopyTo(destination);
                                int firstLength = reference.FirstLength;
                                if (((uint)reference.Separators & (true ? 1u : 0u)) != 0)
                                {
                                    destination[firstLength++] = '\\';
                                }
                                new System.Span<char>((void*)reference.Second, reference.SecondLength).CopyTo(destination.Slice(firstLength));
                                firstLength += reference.SecondLength;
                                if ((reference.Separators & 2u) != 0)
                                {
                                    destination[firstLength++] = '\\';
                                }
                                new System.Span<char>((void*)reference.Third, reference.ThirdLength).CopyTo(destination.Slice(firstLength));
                                firstLength += reference.ThirdLength;
                                if ((reference.Separators & 4u) != 0)
                                {
                                    destination[firstLength++] = '\\';
                                }
                                new System.Span<char>((void*)reference.Fourth, reference.FourthLength).CopyTo(destination.Slice(firstLength));
                            });
                        }
                    }
                }
            }
        }

        private unsafe static void Populate83FileNameFromRandomBytes(byte* bytes, int byteCount, System.Span<char> chars)
        {
            byte b = *bytes;
            byte b2 = bytes[1];
            byte b3 = bytes[2];
            byte b4 = bytes[3];
            byte b5 = bytes[4];
            chars[11] = (char)(*(byte*)Base32Char[bytes[7] & 0x1F]);
            chars[0] = (char)(*(byte*)Base32Char[b & 0x1F]);
            chars[1] = (char)(*(byte*)Base32Char[b2 & 0x1F]);
            chars[2] = (char)(*(byte*)Base32Char[b3 & 0x1F]);
            chars[3] = (char)(*(byte*)Base32Char[b4 & 0x1F]);
            chars[4] = (char)(*(byte*)Base32Char[b5 & 0x1F]);
            chars[5] = (char)(*(byte*)Base32Char[((b & 0xE0) >> 5) | ((b4 & 0x60) >> 2)]);
            chars[6] = (char)(*(byte*)Base32Char[((b2 & 0xE0) >> 5) | ((b5 & 0x60) >> 2)]);
            b3 = (byte)(b3 >> 5);
            if ((b4 & 0x80u) != 0)
            {
                b3 = (byte)(b3 | 8u);
            }
            if ((b5 & 0x80u) != 0)
            {
                b3 = (byte)(b3 | 0x10u);
            }
            chars[7] = (char)(*(byte*)Base32Char[(int)b3]);
            chars[8] = '.';
            chars[9] = (char)(*(byte*)Base32Char[bytes[5] & 0x1F]);
            chars[10] = (char)(*(byte*)Base32Char[bytes[6] & 0x1F]);
        }

        /// <summary>
        /// Create a relative path from one path to another. Paths will be resolved before calculating the difference.
        /// Default path comparison for the active platform will be used (OrdinalIgnoreCase for Windows or Mac, Ordinal for Unix).
        /// </summary>
        /// <param name="relativeTo">The source path the output should be relative to. This path is always considered to be a directory.</param>
        /// <param name="path">The destination path.</param>
        /// <returns>The relative path or <paramref name="path" /> if the paths don't share the same root.</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if <paramref name="relativeTo" /> or <paramref name="path" /> is <c>null</c> or an empty string.</exception>
        public static string GetRelativePath(string relativeTo, string path)
        {
            return GetRelativePath(relativeTo, path, System.IO.PathInternal.StringComparison);
        }

        private static string GetRelativePath(string relativeTo, string path, StringComparison comparisonType)
        {
            if (relativeTo == null)
            {
                throw new ArgumentNullException("relativeTo");
            }
            if (System.IO.PathInternal.IsEffectivelyEmpty(MemoryExtensions.AsSpan(relativeTo)))
            {
                throw new ArgumentException(MDCFR.Properties.Resources.Arg_PathEmpty, "relativeTo");
            }
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (System.IO.PathInternal.IsEffectivelyEmpty(MemoryExtensions.AsSpan(path)))
            {
                throw new ArgumentException(MDCFR.Properties.Resources.Arg_PathEmpty, "path");
            }
            relativeTo = GetFullPath(relativeTo);
            path = GetFullPath(path);
            if (!System.IO.PathInternal.AreRootsEqual(relativeTo, path, comparisonType))
            {
                return path;
            }
            int num = System.IO.PathInternal.GetCommonPathLength(relativeTo, path, comparisonType == StringComparison.OrdinalIgnoreCase);
            if (num == 0)
            {
                return path;
            }
            int num2 = relativeTo.Length;
            if (EndsInDirectorySeparator(MemoryExtensions.AsSpan(relativeTo)))
            {
                num2--;
            }
            bool flag = EndsInDirectorySeparator(MemoryExtensions.AsSpan(path));
            int num3 = path.Length;
            if (flag)
            {
                num3--;
            }
            if (num2 == num3 && num >= num2)
            {
                return ".";
            }
            System.Span<char> initialBuffer = stackalloc char[260];
            ValueStringBuilder valueStringBuilder = new ValueStringBuilder(initialBuffer);
            valueStringBuilder.EnsureCapacity(Math.Max(relativeTo.Length, path.Length));
            if (num < num2)
            {
                valueStringBuilder.Append("..");
                for (int i = num + 1; i < num2; i++)
                {
                    if (System.IO.PathInternal.IsDirectorySeparator(relativeTo[i]))
                    {
                        valueStringBuilder.Append(DirectorySeparatorChar);
                        valueStringBuilder.Append("..");
                    }
                }
            }
            else if (System.IO.PathInternal.IsDirectorySeparator(path[num]))
            {
                num++;
            }
            int num4 = num3 - num;
            if (flag)
            {
                num4++;
            }
            if (num4 > 0)
            {
                if (valueStringBuilder.Length > 0)
                {
                    valueStringBuilder.Append(DirectorySeparatorChar);
                }
                valueStringBuilder.Append(MemoryExtensions.AsSpan(path, num, num4));
            }
            return valueStringBuilder.ToString();
        }

        /// <summary>
        /// Trims one trailing directory separator beyond the root of the path.
        /// </summary>
        public static string TrimEndingDirectorySeparator(string path)
        {
            return System.IO.PathInternal.TrimEndingDirectorySeparator(path);
        }

        /// <summary>
        /// Trims one trailing directory separator beyond the root of the path.
        /// </summary>
        public static System.ReadOnlySpan<char> TrimEndingDirectorySeparator(System.ReadOnlySpan<char> path)
        {
            return System.IO.PathInternal.TrimEndingDirectorySeparator(path);
        }

        /// <summary>
        /// Returns true if the path ends in a directory separator.
        /// </summary>
        public static bool EndsInDirectorySeparator(System.ReadOnlySpan<char> path)
        {
            return System.IO.PathInternal.EndsInDirectorySeparator(path);
        }

        /// <summary>
        /// Returns true if the path ends in a directory separator.
        /// </summary>
        public static bool EndsInDirectorySeparator(string path)
        {
            return System.IO.PathInternal.EndsInDirectorySeparator(path);
        }

        public static char[] GetInvalidFileNameChars()
        {
            return new char[41]
            {
                '"', '<', '>', '|', '\0', '\u0001', '\u0002', '\u0003', '\u0004', '\u0005',
                '\u0006', '\a', '\b', '\t', '\n', '\v', '\f', '\r', '\u000e', '\u000f',
                '\u0010', '\u0011', '\u0012', '\u0013', '\u0014', '\u0015', '\u0016', '\u0017', '\u0018', '\u0019',
                '\u001a', '\u001b', '\u001c', '\u001d', '\u001e', '\u001f', ':', '*', '?', '\\',
                '/'
            };
        }

        public static char[] GetInvalidPathChars()
        {
            return new char[33]
            {
                '|', '\0', '\u0001', '\u0002', '\u0003', '\u0004', '\u0005', '\u0006', '\a', '\b',
                '\t', '\n', '\v', '\f', '\r', '\u000e', '\u000f', '\u0010', '\u0011', '\u0012',
                '\u0013', '\u0014', '\u0015', '\u0016', '\u0017', '\u0018', '\u0019', '\u001a', '\u001b', '\u001c',
                '\u001d', '\u001e', '\u001f'
            };
        }

        public static string GetFullPath(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (System.IO.PathInternal.IsEffectivelyEmpty(MemoryExtensions.AsSpan(path)))
            {
                throw new ArgumentException(MDCFR.Properties.Resources.Arg_PathEmpty, "path");
            }
            if (path.Contains('\0'))
            {
                throw new ArgumentException(MDCFR.Properties.Resources.Argument_InvalidPathChars, "path");
            }
            return GetFullPathInternal(path);
        }

        public static string GetFullPath(string path, string basePath)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (basePath == null)
            {
                throw new ArgumentNullException("basePath");
            }
            if (!IsPathFullyQualified(basePath))
            {
                throw new ArgumentException(MDCFR.Properties.Resources.Arg_BasePathNotFullyQualified, "basePath");
            }
            if (basePath.Contains('\0') || path.Contains('\0'))
            {
                throw new ArgumentException(MDCFR.Properties.Resources.Argument_InvalidPathChars);
            }
            if (IsPathFullyQualified(path))
            {
                return GetFullPathInternal(path);
            }
            if (System.IO.PathInternal.IsEffectivelyEmpty(MemoryExtensions.AsSpan(path)))
            {
                return basePath;
            }
            int length = path.Length;
            string text = ((length >= 1 && System.IO.PathInternal.IsDirectorySeparator(path[0])) ? Join(GetPathRoot(MemoryExtensions.AsSpan(basePath)), MemoryExtensions.AsSpan(path, 1)) : ((length < 2 || !System.IO.PathInternal.IsValidDriveChar(path[0]) || path[1] != ':') ? JoinInternal(MemoryExtensions.AsSpan(basePath), MemoryExtensions.AsSpan(path)) : ((!GetVolumeName(MemoryExtensions.AsSpan(path)).EqualsOrdinal(GetVolumeName(MemoryExtensions.AsSpan(basePath)))) ? ((!System.IO.PathInternal.IsDevice(MemoryExtensions.AsSpan(basePath))) ? path.Insert(2, "\\") : ((length == 2) ? JoinInternal(MemoryExtensions.AsSpan(basePath, 0, 4), MemoryExtensions.AsSpan(path), MemoryExtensions.AsSpan("\\")) : JoinInternal(MemoryExtensions.AsSpan(basePath, 0, 4), MemoryExtensions.AsSpan(path, 0, 2), MemoryExtensions.AsSpan("\\"), MemoryExtensions.AsSpan(path, 2)))) : Join(MemoryExtensions.AsSpan(basePath), MemoryExtensions.AsSpan(path, 2)))));
            if (!System.IO.PathInternal.IsDevice(MemoryExtensions.AsSpan(text)))
            {
                return GetFullPathInternal(text);
            }
            return System.IO.PathInternal.RemoveRelativeSegments(text, System.IO.PathInternal.GetRootLength(MemoryExtensions.AsSpan(text)));
        }

        private static string GetFullPathInternal(string path)
        {
            if (System.IO.PathInternal.IsExtended(MemoryExtensions.AsSpan(path)))
            {
                return path;
            }
            return System.IO.PathHelper.Normalize(path);
        }

        public static string GetTempPath()
        {
            System.Span<char> initialBuffer = stackalloc char[260];
            ValueStringBuilder builder = new ValueStringBuilder(initialBuffer);
            GetTempPath(ref builder);
            string result = System.IO.PathHelper.Normalize(ref builder);
            builder.Dispose();
            return result;
        }

        private static void GetTempPath(ref ValueStringBuilder builder)
        {
            uint tempPathW;
            while ((tempPathW = global::Interop.Kernel32.GetTempPathW(builder.Capacity, ref builder.GetPinnableReference())) > builder.Capacity)
            {
                builder.EnsureCapacity(checked((int)tempPathW));
            }
            if (tempPathW == 0)
            {
                throw Win32Marshal.GetExceptionForLastWin32Error();
            }
            builder.Length = (int)tempPathW;
        }

        public static string GetTempFileName()
        {
            System.Span<char> initialBuffer = stackalloc char[260];
            ValueStringBuilder builder = new ValueStringBuilder(initialBuffer);
            GetTempPath(ref builder);
            initialBuffer = stackalloc char[260];
            ValueStringBuilder path = new ValueStringBuilder(initialBuffer);
            uint tempFileNameW = global::Interop.Kernel32.GetTempFileNameW(ref builder.GetPinnableReference(), "tmp", 0u, ref path.GetPinnableReference());
            builder.Dispose();
            if (tempFileNameW == 0)
            {
                throw Win32Marshal.GetExceptionForLastWin32Error();
            }
            path.Length = MemoryExtensions.IndexOf<char>(path.RawChars, '\0');
            string result = System.IO.PathHelper.Normalize(ref path);
            path.Dispose();
            return result;
        }

        public static bool IsPathRooted([NotNullWhen(true)] string? path)
        {
            if (path != null)
            {
                return IsPathRooted(MemoryExtensions.AsSpan(path));
            }
            return false;
        }

        public unsafe static bool IsPathRooted(System.ReadOnlySpan<char> path)
        {
            int length = path.Length;
            if (length < 1 || !System.IO.PathInternal.IsDirectorySeparator(path[0]))
            {
                if (length >= 2 && System.IO.PathInternal.IsValidDriveChar(path[0]))
                {
                    return path[1] == 58;
                }
                return false;
            }
            return true;
        }

        public static string? GetPathRoot(string? path)
        {
            if (System.IO.PathInternal.IsEffectivelyEmpty(MemoryExtensions.AsSpan(path)))
            {
                return null;
            }
            System.ReadOnlySpan<char> pathRoot = GetPathRoot(MemoryExtensions.AsSpan(path));
            if (path.Length == pathRoot.Length)
            {
                return System.IO.PathInternal.NormalizeDirectorySeparators(path);
            }
            return System.IO.PathInternal.NormalizeDirectorySeparators(pathRoot.ToString());
        }

        /// <remarks>
        /// Unlike the string overload, this method will not normalize directory separators.
        /// </remarks>
        public static System.ReadOnlySpan<char> GetPathRoot(System.ReadOnlySpan<char> path)
        {
            if (System.IO.PathInternal.IsEffectivelyEmpty(path))
            {
                return System.ReadOnlySpan<char>.Empty;
            }
            int rootLength = System.IO.PathInternal.GetRootLength(path);
            if (rootLength > 0)
            {
                return path.Slice(0, rootLength);
            }
            return System.ReadOnlySpan<char>.Empty;
        }

        /// <summary>
        /// Returns the volume name for dos, UNC and device paths.
        /// </summary>
        internal static System.ReadOnlySpan<char> GetVolumeName(System.ReadOnlySpan<char> path)
        {
            System.ReadOnlySpan<char> pathRoot = GetPathRoot(path);
            if (pathRoot.Length == 0)
            {
                return pathRoot;
            }
            int num = GetUncRootLength(path);
            if (num == -1)
            {
                num = (System.IO.PathInternal.IsDevice(path) ? 4 : 0);
            }
            System.ReadOnlySpan<char> readOnlySpan = pathRoot.Slice(num);
            if (!EndsInDirectorySeparator(readOnlySpan))
            {
                return readOnlySpan;
            }
            return readOnlySpan.Slice(0, readOnlySpan.Length - 1);
        }

        /// <summary>
        /// Returns offset as -1 if the path is not in Unc format, otherwise returns the root length.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal static int GetUncRootLength(System.ReadOnlySpan<char> path)
        {
            bool flag = System.IO.PathInternal.IsDevice(path);
            if (!flag && path.Slice(0, 2).EqualsOrdinal(MemoryExtensions.AsSpan("\\\\")))
            {
                return 2;
            }
            if (flag && path.Length >= 8 && (path.Slice(0, 8).EqualsOrdinal(MemoryExtensions.AsSpan("\\\\?\\UNC\\")) || path.Slice(5, 4).EqualsOrdinal(MemoryExtensions.AsSpan("UNC\\"))))
            {
                return 8;
            }
            return -1;
        }
    }

    /// <devdoc>
    ///   Enum describing whether the search operation should
    ///   retrieve files/directories from the current directory alone
    ///   or should include all the subdirectories also.
    /// </devdoc>
    public enum SearchOption
    {
        /// <devdoc>
        ///   Include only the current directory in the search operation
        /// </devdoc>
        TopDirectoryOnly,
        /// <devdoc>
        ///   Include the current directory and all the sub-directories
        ///   underneath it including reparse points in the search operation.
        ///   This will traverse reparse points (i.e, mounted points and symbolic links)
        ///   recursively. If the directory structure searched contains a loop
        ///   because of hard links, the search operation will go on for ever.
        /// </devdoc>
        AllDirectories
    }

    internal enum SearchTarget { Files = 1, Directories, Both }

    public static class StringExtensions
    {
        public delegate void SpanAction<T, in TArg>(System.Span<T> span, TArg arg);

        public static bool Contains(this string s, char value)
        {
            return s.IndexOf(value) != -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool EqualsOrdinal(this System.ReadOnlySpan<char> span, System.ReadOnlySpan<char> value)
        {
            if (span.Length != value.Length)
            {
                return false;
            }
            if (value.Length == 0)
            {
                return true;
            }
            return MemoryExtensions.SequenceEqual<char>(span, value);
        }

        public unsafe static string Create<TState>(int length, TState state, SpanAction<char, TState> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }
            if (length <= 0)
            {
                if (length == 0)
                {
                    return string.Empty;
                }
                throw new ArgumentOutOfRangeException("length");
            }
            string text = new string('\0', length);
            fixed (char* ptr = text)
            {
                action(new System.Span<char>((void*)ptr, length), state);
            }
            return text;
        }

        internal unsafe static string Concat(System.ReadOnlySpan<char> str0, System.ReadOnlySpan<char> str1)
        {
            string text = new string('\0', checked(str0.Length + str1.Length));
            fixed (char* ptr = text)
            {
                System.Span<char> span = new System.Span<char>((void*)ptr, text.Length);
                str0.CopyTo(span);
                str1.CopyTo(span.Slice(str0.Length));
            }
            return text;
        }

        internal unsafe static string Concat(System.ReadOnlySpan<char> str0, System.ReadOnlySpan<char> str1, System.ReadOnlySpan<char> str2)
        {
            string text = new string('\0', checked(str0.Length + str1.Length + str2.Length));
            fixed (char* ptr = text)
            {
                System.Span<char> span = new System.Span<char>((void*)ptr, text.Length);
                str0.CopyTo(span);
                span = span.Slice(str0.Length);
                str1.CopyTo(span);
                span = span.Slice(str1.Length);
                str2.CopyTo(span);
            }
            return text;
        }
    }

    internal static class ThrowHelper
    {
        internal static void ThrowEndOfFileException()
        {
            throw new EndOfStreamException(MDCFR.Properties.Resources.IO_EOF_ReadBeyondEOF);
        }
    }

    namespace Enumeration
    {
        using System.Collections;
        using System.Runtime.ConstrainedExecution;

        /// <summary>Provides a lower level view of <see cref="T:System.IO.FileSystemInfo" /> to help process and filter find results.</summary>
        /// <summary>Provides a lower level view of <see cref="T:System.IO.FileSystemInfo" /> to help process and filter find results.</summary>
        public ref struct FileSystemEntry
        {
            internal unsafe global::Interop.NtDll.FILE_FULL_DIR_INFORMATION* _info;

            /// <summary>Gets the full path of the directory this entry resides in.</summary>
            /// <value>The full path of this entry's directory.</value>
            public System.ReadOnlySpan<char> Directory { get; private set; }

            /// <summary>Gets the full path of the root directory used for the enumeration.</summary>
            /// <value>The root directory.</value>
            public System.ReadOnlySpan<char> RootDirectory { get; private set; }

            /// <summary>Gets the root directory for the enumeration as specified in the constructor.</summary>
            /// <value>The original root directory.</value>
            public System.ReadOnlySpan<char> OriginalRootDirectory { get; private set; }

            /// <summary>Gets the file name for this entry.</summary>
            /// <value>This entry's file name.</value>
            public unsafe System.ReadOnlySpan<char> FileName => _info->FileName;

            /// <summary>Gets the attributes for this entry.</summary>
            /// <value>The attributes for this entry.</value>
            public unsafe FileAttributes Attributes => _info->FileAttributes;

            /// <summary>Gets the length of the file, in bytes.</summary>
            /// <value>The file length in bytes.</value>
            public unsafe long Length => _info->EndOfFile;

            /// <summary>Gets the creation time for the entry or the oldest available time stamp if the operating system does not support creation time stamps.</summary>
            /// <value>The creation time for the entry.</value>
            public unsafe DateTimeOffset CreationTimeUtc => _info->CreationTime.ToDateTimeOffset();

            /// <summary>Gets a datetime offset that represents the last access time in UTC.</summary>
            /// <value>The last access time in UTC.</value>
            public unsafe DateTimeOffset LastAccessTimeUtc => _info->LastAccessTime.ToDateTimeOffset();

            /// <summary>Gets a datetime offset that represents the last write time in UTC.</summary>
            /// <value>The last write time in UTC.</value>
            public unsafe DateTimeOffset LastWriteTimeUtc => _info->LastWriteTime.ToDateTimeOffset();

            /// <summary>Gets a value that indicates whether this entry is a directory.</summary>
            /// <value><see langword="true" /> if the entry is a directory; otherwise, <see langword="false" />.</value>
            public bool IsDirectory => (Attributes & FileAttributes.Directory) != 0;

            /// <summary>Gets a value that indicates whether the file has the hidden attribute.</summary>
            /// <value><see langword="true" /> if the file has the hidden attribute; otherwise, <see langword="false" />.</value>
            public bool IsHidden => (Attributes & FileAttributes.Hidden) != 0;

            /// <summary>Returns the full path for the find results, based on the initially provided path.</summary>
            /// <returns>A string representing the full path.</returns>
            public string ToSpecifiedFullPath()
            {
                System.ReadOnlySpan<char> readOnlySpan = Directory.Slice(RootDirectory.Length);
                if (Path.EndsInDirectorySeparator(OriginalRootDirectory) && System.IO.PathInternal.StartsWithDirectorySeparator(readOnlySpan))
                {
                    readOnlySpan = readOnlySpan.Slice(1);
                }
                return Path.Join(OriginalRootDirectory, readOnlySpan, FileName);
            }

            internal unsafe static void Initialize(ref FileSystemEntry entry, global::Interop.NtDll.FILE_FULL_DIR_INFORMATION* info, System.ReadOnlySpan<char> directory, System.ReadOnlySpan<char> rootDirectory, System.ReadOnlySpan<char> originalRootDirectory)
            {
                entry._info = info;
                entry.Directory = directory;
                entry.RootDirectory = rootDirectory;
                entry.OriginalRootDirectory = originalRootDirectory;
            }

            /// <summary>Converts the value of this instance to a <see cref="T:System.IO.FileSystemInfo" />.</summary>
            /// <returns>The value of this instance as a <see cref="T:System.IO.FileSystemInfo" />.</returns>
            public FileSystemInfo ToFileSystemInfo()
            {
                return FileSystemInfo.Create(Path.Join(Directory, FileName), ref this);
            }

            /// <summary>Returns the full path of the find result.</summary>
            /// <returns>A string representing the full path.</returns>
            public string ToFullPath()
            {
                return Path.Join(Directory, FileName);
            }
        }

        /// <summary>
        /// Enumerable that allows utilizing custom filter predicates and tranform delegates.
        /// </summary>
        public class FileSystemEnumerable<TResult> : IEnumerable<TResult>, IEnumerable
        {
            /// <summary>
            /// Delegate for filtering out find results.
            /// </summary>
            public delegate bool FindPredicate(ref FileSystemEntry entry);

            /// <summary>
            /// Delegate for transforming raw find data into a result.
            /// </summary>
            public delegate TResult FindTransform(ref FileSystemEntry entry);

            private sealed class DelegateEnumerator : FileSystemEnumerator<TResult>
            {
                private readonly FileSystemEnumerable<TResult> _enumerable;

                public DelegateEnumerator(FileSystemEnumerable<TResult> enumerable, bool isNormalized)
                    : base(enumerable._directory, isNormalized, enumerable._options)
                {
                    _enumerable = enumerable;
                }

                protected override TResult TransformEntry(ref FileSystemEntry entry)
                {
                    return _enumerable._transform(ref entry);
                }

                protected override bool ShouldRecurseIntoEntry(ref FileSystemEntry entry)
                {
                    return _enumerable.ShouldRecursePredicate?.Invoke(ref entry) ?? true;
                }

                protected override bool ShouldIncludeEntry(ref FileSystemEntry entry)
                {
                    return _enumerable.ShouldIncludePredicate?.Invoke(ref entry) ?? true;
                }
            }

            private DelegateEnumerator _enumerator;

            private readonly FindTransform _transform;

            private readonly EnumerationOptions _options;

            private readonly string _directory;

            public FindPredicate? ShouldIncludePredicate { get; set; }

            public FindPredicate? ShouldRecursePredicate { get; set; }

            public FileSystemEnumerable(string directory, FindTransform transform, EnumerationOptions? options = null)
                : this(directory, transform, options, isNormalized: false)
            {
            }

            internal FileSystemEnumerable(string directory, FindTransform transform, EnumerationOptions options, bool isNormalized)
            {
                _directory = directory ?? throw new ArgumentNullException("directory");
                _transform = transform ?? throw new ArgumentNullException("transform");
                _options = options ?? EnumerationOptions.Default;
                _enumerator = new DelegateEnumerator(this, isNormalized);
            }

            public IEnumerator<TResult> GetEnumerator()
            {
                return Interlocked.Exchange(ref _enumerator, null) ?? new DelegateEnumerator(this, isNormalized: false);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        internal static class FileSystemEnumerableFactory
        {
            private static readonly char[] s_unixEscapeChars = new char[4] { '\\', '"', '<', '>' };

            /// <summary>
            /// Validates the directory and expression strings to check that they have no invalid characters, any special DOS wildcard characters in Win32 in the expression get replaced with their proper escaped representation, and if the expression string begins with a directory name, the directory name is moved and appended at the end of the directory string.
            /// </summary>
            /// <param name="directory">A reference to a directory string that we will be checking for normalization.</param>
            /// <param name="expression">A reference to a expression string that we will be checking for normalization.</param>
            /// <param name="matchType">The kind of matching we want to check in the expression. If the value is Win32, we will replace special DOS wild characters to their safely escaped representation. This replacement does not affect the normalization status of the expression.</param>
            /// <returns><cref langword="false" /> if the directory reference string get modified inside this function due to the expression beginning with a directory name. <cref langword="true" /> if the directory reference string was not modified.</returns>
            /// <exception cref="T:System.ArgumentException">
            /// The expression is a rooted path.
            /// -or-
            /// The directory or the expression reference strings contain a null character.
            /// </exception>
            /// <exception cref="T:System.ArgumentOutOfRangeException">
            /// The match type is out of the range of the valid MatchType enum values.
            /// </exception>
            internal static bool NormalizeInputs(ref string directory, ref string expression, MatchType matchType)
            {
                if (Path.IsPathRooted(expression))
                {
                    throw new ArgumentException(MDCFR.Properties.Resources.Arg_Path2IsRooted, "expression");
                }
                if (expression.Contains('\0'))
                {
                    throw new ArgumentException(MDCFR.Properties.Resources.Argument_InvalidPathChars, expression);
                }
                if (directory.Contains('\0'))
                {
                    throw new ArgumentException(MDCFR.Properties.Resources.Argument_InvalidPathChars, directory);
                }
                System.ReadOnlySpan<char> directoryName = Path.GetDirectoryName(MemoryExtensions.AsSpan(expression));
                bool result = true;
                if (directoryName.Length != 0)
                {
                    directory = Path.Join(MemoryExtensions.AsSpan(directory), directoryName);
                    expression = expression.Substring(directoryName.Length + 1);
                    result = false;
                }
                switch (matchType)
                {
                    case MatchType.Win32:
                        if (expression == "*")
                        {
                            break;
                        }
                        if (string.IsNullOrEmpty(expression) || expression == "." || expression == "*.*")
                        {
                            expression = "*";
                            break;
                        }
                        if (Path.DirectorySeparatorChar != '\\' && expression.IndexOfAny(s_unixEscapeChars) != -1)
                        {
                            expression = expression.Replace("\\", "\\\\");
                            expression = expression.Replace("\"", "\\\"");
                            expression = expression.Replace(">", "\\>");
                            expression = expression.Replace("<", "\\<");
                        }
                        expression = FileSystemName.TranslateWin32Expression(expression);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("matchType");
                    case MatchType.Simple:
                        break;
                }
                return result;
            }

            private static bool MatchesPattern(string expression, System.ReadOnlySpan<char> name, EnumerationOptions options)
            {
                bool ignoreCase = (options.MatchCasing == MatchCasing.PlatformDefault && !System.IO.PathInternal.IsCaseSensitive) || options.MatchCasing == MatchCasing.CaseInsensitive;
                return options.MatchType switch
                {
                    MatchType.Simple => FileSystemName.MatchesSimpleExpression(MemoryExtensions.AsSpan(expression), name, ignoreCase),
                    MatchType.Win32 => FileSystemName.MatchesWin32Expression(MemoryExtensions.AsSpan(expression), name, ignoreCase),
                    _ => throw new ArgumentOutOfRangeException("options"),
                };
            }

            internal static IEnumerable<string> UserFiles(string directory, string expression, EnumerationOptions options)
            {
                return new FileSystemEnumerable<string>(directory, delegate (ref FileSystemEntry entry)
                {
                    return entry.ToSpecifiedFullPath();
                }, options)
                {
                    ShouldIncludePredicate = delegate (ref FileSystemEntry entry)
                    {
                        return !entry.IsDirectory && MatchesPattern(expression, entry.FileName, options);
                    }
                };
            }

            internal static IEnumerable<string> UserDirectories(string directory, string expression, EnumerationOptions options)
            {
                return new FileSystemEnumerable<string>(directory, delegate (ref FileSystemEntry entry)
                {
                    return entry.ToSpecifiedFullPath();
                }, options)
                {
                    ShouldIncludePredicate = delegate (ref FileSystemEntry entry)
                    {
                        return entry.IsDirectory && MatchesPattern(expression, entry.FileName, options);
                    }
                };
            }

            internal static IEnumerable<string> UserEntries(string directory, string expression, EnumerationOptions options)
            {
                return new FileSystemEnumerable<string>(directory, delegate (ref FileSystemEntry entry)
                {
                    return entry.ToSpecifiedFullPath();
                }, options)
                {
                    ShouldIncludePredicate = delegate (ref FileSystemEntry entry)
                    {
                        return MatchesPattern(expression, entry.FileName, options);
                    }
                };
            }

            internal static IEnumerable<FileInfo> FileInfos(string directory, string expression, EnumerationOptions options, bool isNormalized)
            {
                return new FileSystemEnumerable<FileInfo>(directory, delegate (ref FileSystemEntry entry)
                {
                    return (FileInfo)entry.ToFileSystemInfo();
                }, options, isNormalized)
                {
                    ShouldIncludePredicate = delegate (ref FileSystemEntry entry)
                    {
                        return !entry.IsDirectory && MatchesPattern(expression, entry.FileName, options);
                    }
                };
            }

            internal static IEnumerable<DirectoryInfo> DirectoryInfos(string directory, string expression, EnumerationOptions options, bool isNormalized)
            {
                return new FileSystemEnumerable<DirectoryInfo>(directory, delegate (ref FileSystemEntry entry)
                {
                    return (DirectoryInfo)entry.ToFileSystemInfo();
                }, options, isNormalized)
                {
                    ShouldIncludePredicate = delegate (ref FileSystemEntry entry)
                    {
                        return entry.IsDirectory && MatchesPattern(expression, entry.FileName, options);
                    }
                };
            }

            internal static IEnumerable<FileSystemInfo> FileSystemInfos(string directory, string expression, EnumerationOptions options, bool isNormalized)
            {
                return new FileSystemEnumerable<FileSystemInfo>(directory, delegate (ref FileSystemEntry entry)
                {
                    return entry.ToFileSystemInfo();
                }, options, isNormalized)
                {
                    ShouldIncludePredicate = delegate (ref FileSystemEntry entry)
                    {
                        return MatchesPattern(expression, entry.FileName, options);
                    }
                };
            }
        }

        /// <summary>Enumerates the file system elements of the provided type that are being searched and filtered by a <see cref="T:Microsoft.IO.Enumeration.FileSystemEnumerable`1" />.</summary>
        /// <typeparam name="TResult">The type of the result produced by this file system enumerator.</typeparam>
        /// <summary>Enumerates the file system elements of the provided type that are being searched and filtered by a <see cref="T:Microsoft.IO.Enumeration.FileSystemEnumerable`1" />.</summary>
        public abstract class FileSystemEnumerator<TResult> : CriticalFinalizerObject, IEnumerator<TResult>, IDisposable, IEnumerator
        {
            private int _remainingRecursionDepth;

            private const int StandardBufferSize = 4096;

            private const int MinimumBufferSize = 1024;

            private readonly string _originalRootDirectory;

            private readonly string _rootDirectory;

            private readonly EnumerationOptions _options;

            private readonly object _lock = new object();

            private unsafe global::Interop.NtDll.FILE_FULL_DIR_INFORMATION* _entry;

            private TResult _current;

            private IntPtr _buffer;

            private int _bufferLength;

            private IntPtr _directoryHandle;

            private string _currentPath;

            private bool _lastEntryFound;

            private Queue<(IntPtr Handle, string Path, int RemainingDepth)> _pending;

            /// <summary>Gets the currently visited element.</summary>
            /// <value>The currently visited element.</value>
            public TResult Current => _current;

            /// <summary>Gets the currently visited object.</summary>
            /// <value>The currently visited object.</value>
            /// <remarks>This member is an explicit interface member implementation. It can be used only when the <see cref="T:Microsoft.IO.Enumeration.FileSystemEnumerator`1" /> instance is cast to an <see cref="T:System.Collections.IEnumerator" /> interface.</remarks>
            object? IEnumerator.Current => Current;

            /// <summary>Encapsulates a find operation.</summary>
            /// <param name="directory">The directory to search in.</param>
            /// <param name="options">Enumeration options to use.</param>
            public FileSystemEnumerator(string directory, EnumerationOptions? options = null)
                : this(directory, isNormalized: false, options)
            {
            }

            /// <summary>
            /// Encapsulates a find operation.
            /// </summary>
            /// <param name="directory">The directory to search in.</param>
            /// <param name="isNormalized">Whether the directory path is already normalized or not.</param>
            /// <param name="options">Enumeration options to use.</param>
            internal FileSystemEnumerator(string directory, bool isNormalized, EnumerationOptions options = null)
            {
                _originalRootDirectory = directory ?? throw new ArgumentNullException("directory");
                _rootDirectory = Path.TrimEndingDirectorySeparator(isNormalized ? directory : Path.GetFullPath(directory));
                _options = options ?? EnumerationOptions.Default;
                _remainingRecursionDepth = _options.MaxRecursionDepth;
                Init();
            }

            /// <summary>When overridden in a derived class, determines whether the specified file system entry should be included in the results.</summary>
            /// <param name="entry">A file system entry reference.</param>
            /// <returns><see langword="true" /> if the specified file system entry should be included in the results; otherwise, <see langword="false" />.</returns>
            protected virtual bool ShouldIncludeEntry(ref FileSystemEntry entry)
            {
                return true;
            }

            /// <summary>When overridden in a derived class, determines whether the specified file system entry should be recursed.</summary>
            /// <param name="entry">A file system entry reference.</param>
            /// <returns><see langword="true" /> if the specified directory entry should be recursed into; otherwise, <see langword="false" />.</returns>
            protected virtual bool ShouldRecurseIntoEntry(ref FileSystemEntry entry)
            {
                return true;
            }

            /// <summary>When overridden in a derived class, generates the result type from the current entry.</summary>
            /// <param name="entry">A file system entry reference.</param>
            /// <returns>The result type from the current entry.</returns>
            protected abstract TResult TransformEntry(ref FileSystemEntry entry);

            /// <summary>When overridden in a derived class, this method is called whenever the end of a directory is reached.</summary>
            /// <param name="directory">The directory path as a read-only span.</param>
            protected virtual void OnDirectoryFinished(System.ReadOnlySpan<char> directory)
            {
            }

            /// <summary>When overridden in a derived class, returns a value that indicates whether to continue execution or throw the default exception.</summary>
            /// <param name="error">The native error code.</param>
            /// <returns><see langword="true" /> to continue; <see langword="false" /> to throw the default exception for the given error.</returns>
            protected virtual bool ContinueOnError(int error)
            {
                return false;
            }

            private unsafe void DirectoryFinished()
            {
                _entry = default(global::Interop.NtDll.FILE_FULL_DIR_INFORMATION*);
                CloseDirectoryHandle();
                OnDirectoryFinished(MemoryExtensions.AsSpan(_currentPath));
                if (!DequeueNextDirectory())
                {
                    _lastEntryFound = true;
                }
                else
                {
                    FindNextEntry();
                }
            }

            /// <summary>Always throws <see cref="T:System.NotSupportedException" />.</summary>
            public void Reset()
            {
                throw new NotSupportedException();
            }

            /// <summary>Releases the resources used by the current instance of the <see cref="T:Microsoft.IO.Enumeration.FileSystemEnumerator`1" /> class.</summary>
            public void Dispose()
            {
                InternalDispose(disposing: true);
                GC.SuppressFinalize(this);
            }

            /// <summary>When overridden in a derived class, releases the unmanaged resources used by the <see cref="T:Microsoft.IO.Enumeration.FileSystemEnumerator`1" /> class and optionally releases the managed resources.</summary>
            /// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
            protected virtual void Dispose(bool disposing)
            {
            }

            ~FileSystemEnumerator()
            {
                InternalDispose(disposing: false);
            }

            private void Init()
            {
                using (default(DisableMediaInsertionPrompt))
                {
                    _directoryHandle = CreateDirectoryHandle(_rootDirectory);
                    if (_directoryHandle == IntPtr.Zero)
                    {
                        _lastEntryFound = true;
                    }
                }
                _currentPath = _rootDirectory;
                int bufferSize = _options.BufferSize;
                _bufferLength = ((bufferSize <= 0) ? 4096 : Math.Max(1024, bufferSize));
                try
                {
                    _buffer = Marshal.AllocHGlobal(_bufferLength);
                }
                catch
                {
                    CloseDirectoryHandle();
                    throw;
                }
            }

            /// <summary>
            /// Fills the buffer with the next set of data.
            /// </summary>
            /// <returns>'true' if new data was found</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private unsafe bool GetData()
            {
                global::Interop.NtDll.IO_STATUS_BLOCK iO_STATUS_BLOCK = default(global::Interop.NtDll.IO_STATUS_BLOCK);
                int num = global::Interop.NtDll.NtQueryDirectoryFile(_directoryHandle, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, &iO_STATUS_BLOCK, _buffer, (uint)_bufferLength, global::Interop.NtDll.FILE_INFORMATION_CLASS.FileFullDirectoryInformation, global::Interop.BOOLEAN.FALSE, null, global::Interop.BOOLEAN.FALSE);
                switch ((uint)num)
                {
                    case 2147483654u:
                        DirectoryFinished();
                        return false;
                    case 0u:
                        return true;
                    case 3221225487u:
                        DirectoryFinished();
                        return false;
                    default:
                        {
                            int num2 = (int)global::Interop.NtDll.RtlNtStatusToDosError(num);
                            if ((num2 == 5 && _options.IgnoreInaccessible) || ContinueOnError(num2))
                            {
                                DirectoryFinished();
                                return false;
                            }
                            throw Win32Marshal.GetExceptionForWin32Error(num2, _currentPath);
                        }
                }
            }

            private unsafe IntPtr CreateRelativeDirectoryHandle(System.ReadOnlySpan<char> relativePath, string fullPath)
            {
                var (num, result) = global::Interop.NtDll.CreateFile(relativePath, _directoryHandle, global::Interop.NtDll.CreateDisposition.FILE_OPEN, global::Interop.NtDll.DesiredAccess.FILE_READ_DATA | global::Interop.NtDll.DesiredAccess.SYNCHRONIZE, FileShare.ReadWrite | FileShare.Delete, (FileAttributes)0, (global::Interop.NtDll.CreateOptions)16417u, global::Interop.ObjectAttributes.OBJ_CASE_INSENSITIVE, null, 0u, null, null);
                if (num == 0)
                {
                    return result;
                }
                int num2 = (int)global::Interop.NtDll.RtlNtStatusToDosError((int)num);
                if (ContinueOnDirectoryError(num2, ignoreNotFound: true))
                {
                    return IntPtr.Zero;
                }
                throw Win32Marshal.GetExceptionForWin32Error(num2, fullPath);
            }

            private void CloseDirectoryHandle()
            {
                IntPtr intPtr = Interlocked.Exchange(ref _directoryHandle, IntPtr.Zero);
                if (intPtr != IntPtr.Zero)
                {
                    global::Interop.Kernel32.CloseHandle(intPtr);
                }
            }

            /// <summary>
            /// Simple wrapper to allow creating a file handle for an existing directory.
            /// </summary>
            private IntPtr CreateDirectoryHandle(string path, bool ignoreNotFound = false)
            {
                IntPtr intPtr = global::Interop.Kernel32.CreateFile_IntPtr(path, 1, FileShare.ReadWrite | FileShare.Delete, FileMode.Open, 33554432);
                if (intPtr == IntPtr.Zero || intPtr == (IntPtr)(-1))
                {
                    int num = Marshal.GetLastWin32Error();
                    if (ContinueOnDirectoryError(num, ignoreNotFound))
                    {
                        return IntPtr.Zero;
                    }
                    if (num == 2)
                    {
                        num = 3;
                    }
                    throw Win32Marshal.GetExceptionForWin32Error(num, path);
                }
                return intPtr;
            }

            private bool ContinueOnDirectoryError(int error, bool ignoreNotFound)
            {
                if ((!ignoreNotFound || (error != 2 && error != 3 && error != 267)) && (error != 5 || !_options.IgnoreInaccessible))
                {
                    return ContinueOnError(error);
                }
                return true;
            }

            /// <summary>Advances the enumerator to the next item of the <see cref="T:Microsoft.IO.Enumeration.FileSystemEnumerator`1" />.</summary>
            /// <returns><see langword="true" /> if the enumerator successfully advanced to the next item; <see langword="false" /> if the end of the enumerator has been passed.</returns>
            public unsafe bool MoveNext()
            {
                if (_lastEntryFound)
                {
                    return false;
                }
                FileSystemEntry entry = default(FileSystemEntry);
                lock (_lock)
                {
                    if (_lastEntryFound)
                    {
                        return false;
                    }
                    while (true)
                    {
                        FindNextEntry();
                        if (_lastEntryFound)
                        {
                            return false;
                        }
                        FileSystemEntry.Initialize(ref entry, _entry, MemoryExtensions.AsSpan(_currentPath), MemoryExtensions.AsSpan(_rootDirectory), MemoryExtensions.AsSpan(_originalRootDirectory));
                        if ((_entry->FileAttributes & _options.AttributesToSkip) != 0)
                        {
                            continue;
                        }
                        if ((_entry->FileAttributes & FileAttributes.Directory) != 0)
                        {
                            if (_entry->FileName.Length <= 2 && _entry->FileName[0] == 46 && (_entry->FileName.Length != 2 || _entry->FileName[1] == 46))
                            {
                                if (!_options.ReturnSpecialDirectories)
                                {
                                    continue;
                                }
                            }
                            else if (_options.RecurseSubdirectories && _remainingRecursionDepth > 0 && ShouldRecurseIntoEntry(ref entry))
                            {
                                string text = Path.Join(MemoryExtensions.AsSpan(_currentPath), _entry->FileName);
                                IntPtr intPtr = CreateRelativeDirectoryHandle(_entry->FileName, text);
                                if (intPtr != IntPtr.Zero)
                                {
                                    try
                                    {
                                        if (_pending == null)
                                        {
                                            _pending = new Queue<(IntPtr, string, int)>();
                                        }
                                        _pending.Enqueue((intPtr, text, _remainingRecursionDepth - 1));
                                    }
                                    catch
                                    {
                                        global::Interop.Kernel32.CloseHandle(intPtr);
                                        throw;
                                    }
                                }
                            }
                        }
                        if (ShouldIncludeEntry(ref entry))
                        {
                            break;
                        }
                    }
                    _current = TransformEntry(ref entry);
                    return true;
                }
            }

            private unsafe void FindNextEntry()
            {
                _entry = global::Interop.NtDll.FILE_FULL_DIR_INFORMATION.GetNextInfo(_entry);
                if (_entry == null && GetData())
                {
                    _entry = (global::Interop.NtDll.FILE_FULL_DIR_INFORMATION*)(void*)_buffer;
                }
            }

            private bool DequeueNextDirectory()
            {
                if (_pending == null || _pending.Count == 0)
                {
                    return false;
                }
                (_directoryHandle, _currentPath, _remainingRecursionDepth) = _pending.Dequeue();
                return true;
            }

            private void InternalDispose(bool disposing)
            {
                if (_lock != null)
                {
                    lock (_lock)
                    {
                        _lastEntryFound = true;
                        CloseDirectoryHandle();
                        if (_pending != null)
                        {
                            while (_pending.Count > 0)
                            {
                                global::Interop.Kernel32.CloseHandle(_pending.Dequeue().Handle);
                            }
                            _pending = null;
                        }
                        if (_buffer != (IntPtr)0)
                        {
                            Marshal.FreeHGlobal(_buffer);
                        }
                        _buffer = default(IntPtr);
                    }
                }
                Dispose(disposing);
            }
        }

        /// <summary>Provides methods for matching file system names.</summary>
        public static class FileSystemName
        {
            private static readonly char[] s_wildcardChars = new char[5] { '"', '<', '>', '*', '?' };

            private static readonly char[] s_simpleWildcardChars = new char[2] { '*', '?' };

            /// <summary>Translates the given Win32 expression. Change '*' and '?' to '&lt;', '&gt;' and '"' to match Win32 behavior.</summary>
            /// <param name="expression">The expression to translate.</param>
            /// <returns>A string with the translated Win32 expression.</returns>
            /// <remarks>For compatibility, Windows changes some wildcards to provide a closer match to historical DOS 8.3 filename matching.</remarks>
            public static string TranslateWin32Expression(string? expression)
            {
                if (string.IsNullOrEmpty(expression) || expression == "*" || expression == "*.*")
                {
                    return "*";
                }
                bool flag = false;
                System.Span<char> initialBuffer = stackalloc char[32];
                ValueStringBuilder valueStringBuilder = new ValueStringBuilder(initialBuffer);
                int length = expression.Length;
                for (int i = 0; i < length; i++)
                {
                    char c = expression[i];
                    switch (c)
                    {
                        case '.':
                            flag = true;
                            if (i >= 1 && i == length - 1 && expression[i - 1] == '*')
                            {
                                valueStringBuilder[valueStringBuilder.Length - 1] = '<';
                            }
                            else if (i < length - 1 && (expression[i + 1] == '?' || expression[i + 1] == '*'))
                            {
                                valueStringBuilder.Append('"');
                            }
                            else
                            {
                                valueStringBuilder.Append('.');
                            }
                            break;
                        case '?':
                            flag = true;
                            valueStringBuilder.Append('>');
                            break;
                        default:
                            valueStringBuilder.Append(c);
                            break;
                    }
                }
                if (!flag)
                {
                    return expression;
                }
                return valueStringBuilder.ToString();
            }

            /// <summary>Verifies whether the given Win32 expression matches the given name. Supports the following wildcards: '*', '?', '&lt;', '&gt;', '"'. The backslash character '\' escapes.</summary>
            /// <param name="expression">The expression to match with, such as "*.foo".</param>
            /// <param name="name">The name to check against the expression.</param>
            /// <param name="ignoreCase"><see langword="true" /> to ignore case (default), <see langword="false" /> if the match should be case-sensitive.</param>
            /// <returns><see langword="true" /> if the given expression matches the given name; otherwise, <see langword="false" />.</returns>
            /// <remarks>The syntax of the <paramref name="expression" /> parameter is based on the syntax used by FileSystemWatcher, which is based on [RtlIsNameInExpression](/windows/win32/devnotes/rtlisnameinexpression), which defines the rules for matching DOS wildcards (`'*'`, `'?'`, `'&lt;'`, `'&gt;'`, `'"'`).
            /// Matching will not correspond to Win32 behavior unless you transform the expression using <see cref="M:Microsoft.IO.Enumeration.FileSystemName.TranslateWin32Expression(System.String)" />.</remarks>
            public static bool MatchesWin32Expression(System.ReadOnlySpan<char> expression, System.ReadOnlySpan<char> name, bool ignoreCase = true)
            {
                return MatchPattern(expression, name, ignoreCase, useExtendedWildcards: true);
            }

            /// <summary>Verifies whether the given expression matches the given name. Supports the following wildcards: '*' and '?'. The backslash character '\\' escapes.</summary>
            /// <param name="expression">The expression to match with.</param>
            /// <param name="name">The name to check against the expression.</param>
            /// <param name="ignoreCase"><see langword="true" /> to ignore case (default); <see langword="false" /> if the match should be case-sensitive.</param>
            /// <returns><see langword="true" /> if the given expression matches the given name; otherwise, <see langword="false" />.</returns>
            public static bool MatchesSimpleExpression(System.ReadOnlySpan<char> expression, System.ReadOnlySpan<char> name, bool ignoreCase = true)
            {
                return MatchPattern(expression, name, ignoreCase, useExtendedWildcards: false);
            }

            private unsafe static bool MatchPattern(System.ReadOnlySpan<char> expression, System.ReadOnlySpan<char> name, bool ignoreCase, bool useExtendedWildcards)
            {
                if (expression.Length == 0 || name.Length == 0)
                {
                    return false;
                }
                if (expression[0] == 42)
                {
                    if (expression.Length == 1)
                    {
                        return true;
                    }
                    System.ReadOnlySpan<char> readOnlySpan = expression.Slice(1);
                    if (MemoryExtensions.IndexOfAny<char>(readOnlySpan, (useExtendedWildcards ? s_wildcardChars : s_simpleWildcardChars)) == -1)
                    {
                        if (name.Length < readOnlySpan.Length)
                        {
                            return false;
                        }
                        return MemoryExtensions.EndsWith(name, readOnlySpan, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
                    }
                }
                int num = 0;
                int num2 = 1;
                char c = '\0';
                System.Span<int> span = default(System.Span<int>);
                System.Span<int> span2 = stackalloc int[16];
                System.Span<int> span3 = stackalloc int[16];
                span3[0] = 0;
                int num3 = expression.Length * 2;
                bool flag = false;
                int num6;
                while (!flag)
                {
                    if (num < name.Length)
                    {
                        c = name[num++];
                    }
                    else
                    {
                        if (span3[num2 - 1] == num3)
                        {
                            break;
                        }
                        flag = true;
                    }
                    int i = 0;
                    int num4 = 0;
                    int j = 0;
                    while (i < num2)
                    {
                        int num5 = (span3[i++] + 1) / 2;
                        while (num5 < expression.Length)
                        {
                            num6 = num5 * 2;
                            char c2 = expression[num5];
                            if (num4 >= span2.Length - 2)
                            {
                                int num7 = span2.Length * 2;
                                span = new int[num7];
                                span2.CopyTo(span);
                                span2 = span;
                                span = new int[num7];
                                span3.CopyTo(span);
                                span3 = span;
                            }
                            if (c2 != '*')
                            {
                                if (!useExtendedWildcards || c2 != '<')
                                {
                                    num6 += 2;
                                    if (useExtendedWildcards && c2 == '>')
                                    {
                                        if (!flag && c != '.')
                                        {
                                            span2[num4++] = num6;
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        if (!useExtendedWildcards || c2 != '"')
                                        {
                                            if (c2 == '\\')
                                            {
                                                if (++num5 == expression.Length)
                                                {
                                                    span2[num4++] = num3;
                                                    break;
                                                }
                                                num6 = num5 * 2 + 2;
                                                c2 = expression[num5];
                                            }
                                            if (!flag)
                                            {
                                                if (c2 == '?')
                                                {
                                                    span2[num4++] = num6;
                                                }
                                                else if (ignoreCase ? (char.ToUpperInvariant(c2) == char.ToUpperInvariant(c)) : (c2 == c))
                                                {
                                                    span2[num4++] = num6;
                                                }
                                            }
                                            break;
                                        }
                                        if (!flag)
                                        {
                                            if (c == '.')
                                            {
                                                span2[num4++] = num6;
                                            }
                                            break;
                                        }
                                    }
                                    goto IL_02e4;
                                }
                                bool flag2 = false;
                                if (!flag && c == '.')
                                {
                                    for (int k = num; k < name.Length; k++)
                                    {
                                        if (name[k] == 46)
                                        {
                                            flag2 = true;
                                            break;
                                        }
                                    }
                                }
                                if (!(flag || c != '.' || flag2))
                                {
                                    goto IL_02d3;
                                }
                            }
                            span2[num4++] = num6;
                            goto IL_02d3;
                        IL_02e4:
                            if (++num5 == expression.Length)
                            {
                                span2[num4++] = num3;
                            }
                            continue;
                        IL_02d3:
                            span2[num4++] = num6 + 1;
                            goto IL_02e4;
                        }
                        if (i >= num2 || j >= num4)
                        {
                            continue;
                        }
                        for (; j < num4; j++)
                        {
                            for (int length = span3.Length; i < length && span3[i] < span2[j]; i++)
                            {
                            }
                        }
                    }
                    if (num4 == 0)
                    {
                        return false;
                    }
                    span = span3;
                    span3 = span2;
                    span2 = span;
                    num2 = num4;
                }
                num6 = span3[num2 - 1];
                return num6 == num3;
            }
        }

        internal sealed class SafeFindHandle : SafeHandle
        {
            public override bool IsInvalid
            {
                get
                {
                    if (!(handle == IntPtr.Zero))
                    {
                        return handle == new IntPtr(-1);
                    }
                    return true;
                }
            }

            public SafeFindHandle()
                : base(IntPtr.Zero, ownsHandle: true)
            {
            }

            protected override bool ReleaseHandle()
            {
                return global::Interop.Kernel32.FindClose(handle);
            }
        }

        internal static class FixedBufferExtensions
        {
            /// <summary>
            /// Returns a string from the given span, terminating the string at null if present.
            /// </summary>
            internal unsafe static string GetStringFromFixedBuffer(this System.ReadOnlySpan<char> span)
            {
                fixed (char* value = &MemoryMarshal.GetReference<char>(span))
                {
                    return new string(value, 0, span.GetFixedBufferStringLength());
                }
            }

            /// <summary>
            /// Gets the null-terminated string length of the given span.
            /// </summary>
            internal static int GetFixedBufferStringLength(this System.ReadOnlySpan<char> span)
            {
                int num = MemoryExtensions.IndexOf<char>(span, '\0');
                if (num >= 0)
                {
                    return num;
                }
                return span.Length;
            }

            /// <summary>
            /// Returns true if the given string equals the given span.
            /// The span's logical length is to the first null if present.
            /// </summary>
            internal unsafe static bool FixedBufferEqualsString(this System.ReadOnlySpan<char> span, string value)
            {
                if (value == null || value.Length > span.Length)
                {
                    return false;
                }
                int i;
                for (i = 0; i < value.Length; i++)
                {
                    if (value[i] == '\0' || value[i] != span[i])
                    {
                        return false;
                    }
                }
                if (i != span.Length)
                {
                    return span[i] == 0;
                }
                return true;
            }
        }
    }

}
#nullable disable

internal static partial class Interop
{
    internal static class BCrypt
    {
        internal enum NTSTATUS : uint
        {
            STATUS_SUCCESS = 0u,
            STATUS_NOT_FOUND = 3221226021u,
            STATUS_INVALID_PARAMETER = 3221225485u,
            STATUS_NO_MEMORY = 3221225495u,
            STATUS_AUTH_TAG_MISMATCH = 3221266434u
        }

        internal const int BCRYPT_USE_SYSTEM_PREFERRED_RNG = 2;

        [DllImport(Libraries.BCrypt, CharSet = CharSet.Unicode)]
        internal unsafe static extern NTSTATUS BCryptGenRandom(IntPtr hAlgorithm, byte* pbBuffer, int cbBuffer, int dwFlags);
    }

    internal static partial class Kernel32
    {
        internal struct FILE_TIME
        {
            internal uint dwLowDateTime;

            internal uint dwHighDateTime;

            internal FILE_TIME(long fileTime)
            {
                dwLowDateTime = (uint)fileTime;
                dwHighDateTime = (uint)(fileTime >> 32);
            }

            internal long ToTicks()
            {
                return (long)(((ulong)dwHighDateTime << 32) + dwLowDateTime);
            }

            internal DateTime ToDateTimeUtc()
            {
                return DateTime.FromFileTimeUtc(ToTicks());
            }

            internal DateTimeOffset ToDateTimeOffset()
            {
                return DateTimeOffset.FromFileTime(ToTicks());
            }
        }

        internal struct FILE_BASIC_INFO
        {
            internal long CreationTime;

            internal long LastAccessTime;

            internal long LastWriteTime;

            internal long ChangeTime;

            internal uint FileAttributes;
        }

        internal static class FileAttributes
        {
            internal const int FILE_ATTRIBUTE_NORMAL = 128;

            internal const int FILE_ATTRIBUTE_READONLY = 1;

            internal const int FILE_ATTRIBUTE_DIRECTORY = 16;

            internal const int FILE_ATTRIBUTE_REPARSE_POINT = 1024;
        }

        internal enum FINDEX_INFO_LEVELS : uint { FindExInfoStandard, FindExInfoBasic, FindExInfoMaxInfoLevel }

        internal enum FINDEX_SEARCH_OPS : uint { FindExSearchNameMatch, FindExSearchLimitToDirectories, FindExSearchLimitToDevices, FindExSearchMaxSearchOp }

        internal static class GenericOperations
        {
            internal const int GENERIC_READ = int.MinValue;

            internal const int GENERIC_WRITE = 1073741824;
        }

        internal enum GET_FILEEX_INFO_LEVELS : uint { GetFileExInfoStandard, GetFileExMaxInfoLevel }

        internal struct SymbolicLinkReparseBuffer
        {
            internal uint ReparseTag;

            internal ushort ReparseDataLength;

            internal ushort Reserved;

            internal ushort SubstituteNameOffset;

            internal ushort SubstituteNameLength;

            internal ushort PrintNameOffset;

            internal ushort PrintNameLength;

            internal uint Flags;
        }

        internal struct MountPointReparseBuffer
        {
            public uint ReparseTag;

            public ushort ReparseDataLength;

            public ushort Reserved;

            public ushort SubstituteNameOffset;

            public ushort SubstituteNameLength;

            public ushort PrintNameOffset;

            public ushort PrintNameLength;
        }

        internal struct SECURITY_ATTRIBUTES
        {
            internal uint nLength;

            internal IntPtr lpSecurityDescriptor;

            internal BOOL bInheritHandle;
        }

        internal struct WIN32_FILE_ATTRIBUTE_DATA
        {
            internal int dwFileAttributes;

            internal FILE_TIME ftCreationTime;

            internal FILE_TIME ftLastAccessTime;

            internal FILE_TIME ftLastWriteTime;

            internal uint nFileSizeHigh;

            internal uint nFileSizeLow;

            internal void PopulateFrom(ref WIN32_FIND_DATA findData)
            {
                dwFileAttributes = (int)findData.dwFileAttributes;
                ftCreationTime = findData.ftCreationTime;
                ftLastAccessTime = findData.ftLastAccessTime;
                ftLastWriteTime = findData.ftLastWriteTime;
                nFileSizeHigh = findData.nFileSizeHigh;
                nFileSizeLow = findData.nFileSizeLow;
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct WIN32_FIND_DATA
        {
            internal uint dwFileAttributes;

            internal FILE_TIME ftCreationTime;

            internal FILE_TIME ftLastAccessTime;

            internal FILE_TIME ftLastWriteTime;

            internal uint nFileSizeHigh;

            internal uint nFileSizeLow;

            internal uint dwReserved0;

            internal uint dwReserved1;

            private unsafe fixed char _cFileName[260];

            private unsafe fixed char _cAlternateFileName[14];

            internal unsafe System.ReadOnlySpan<char> cFileName
            {
                get
                {
                    fixed (char* ptr = _cFileName)
                    {
                        return new System.ReadOnlySpan<char>((void*)ptr, 260);
                    }
                }
            }
        }

        internal static class IOReparseOptions
        {
            internal const uint IO_REPARSE_TAG_FILE_PLACEHOLDER = 2147483669u;

            internal const uint IO_REPARSE_TAG_MOUNT_POINT = 2684354563u;

            internal const uint IO_REPARSE_TAG_SYMLINK = 2684354572u;
        }

        internal static class FileOperations
        {
            internal const int OPEN_EXISTING = 3;

            internal const int COPY_FILE_FAIL_IF_EXISTS = 1;

            internal const int FILE_FLAG_BACKUP_SEMANTICS = 33554432;

            internal const int FILE_FLAG_FIRST_PIPE_INSTANCE = 524288;

            internal const int FILE_FLAG_OPEN_REPARSE_POINT = 2097152;

            internal const int FILE_FLAG_OVERLAPPED = 1073741824;

            internal const int FILE_LIST_DIRECTORY = 1;
        }

        /// <summary>
        /// The link target is a directory.
        /// </summary>
        internal const int SYMBOLIC_LINK_FLAG_DIRECTORY = 1;

        /// <summary>
        /// Allows creation of symbolic links from a process that is not elevated. Requires Windows 10 Insiders build 14972 or later.
        /// Developer Mode must first be enabled on the machine before this option will function.
        /// </summary>
        internal const int SYMBOLIC_LINK_FLAG_ALLOW_UNPRIVILEGED_CREATE = 2;

        internal const int FSCTL_GET_REPARSE_POINT = 589992;

        internal const int FileBasicInfo = 0;

        private const int FORMAT_MESSAGE_IGNORE_INSERTS = 512;

        private const int FORMAT_MESSAGE_FROM_HMODULE = 2048;

        private const int FORMAT_MESSAGE_FROM_SYSTEM = 4096;

        private const int FORMAT_MESSAGE_ARGUMENT_ARRAY = 8192;

        private const int FORMAT_MESSAGE_ALLOCATE_BUFFER = 256;

        private const int ERROR_INSUFFICIENT_BUFFER = 122;

        internal const uint FILE_NAME_NORMALIZED = 0u;

        internal const int MAXIMUM_REPARSE_DATA_BUFFER_SIZE = 16384;

        internal const uint SYMLINK_FLAG_RELATIVE = 1u;

        internal const uint SEM_FAILCRITICALERRORS = 1u;

        internal const uint FILE_SUPPORTS_ENCRYPTION = 131072u;

        private const uint MOVEFILE_REPLACE_EXISTING = 1u;

        private const uint MOVEFILE_COPY_ALLOWED = 2u;

        internal const int REPLACEFILE_IGNORE_MERGE_ERRORS = 2;

        [DllImport(Libraries.Kernel32, BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "CreateSymbolicLinkW", ExactSpelling = true, SetLastError = true)]
        private static extern bool CreateSymbolicLinkPrivate(string lpSymlinkFileName, string lpTargetFileName, int dwFlags);

        /// <summary>
        /// Creates a symbolic link.
        /// </summary>
        /// <param name="symlinkFileName">The symbolic link to be created.</param>
        /// <param name="targetFileName">The name of the target for the symbolic link to be created.
        /// If it has a device name associated with it, the link is treated as an absolute link; otherwise, the link is treated as a relative link.</param>
        /// <param name="isDirectory"><see langword="true" /> if the link target is a directory; <see langword="false" /> otherwise.</param>
        internal static void CreateSymbolicLink(string symlinkFileName, string targetFileName, bool isDirectory)
        {
            string path = symlinkFileName;
            symlinkFileName = System.IO.PathInternal.EnsureExtendedPrefixIfNeeded(symlinkFileName);
            targetFileName = System.IO.PathInternal.EnsureExtendedPrefixIfNeeded(targetFileName);
            int num = 0;
            bool flag = (Environment.OSVersion.Version.Major == 10 && Environment.OSVersion.Version.Build >= 14972) || Environment.OSVersion.Version.Major >= 11;
            if (flag)
            {
                num = 2;
            }
            if (isDirectory)
            {
                num |= 1;
            }
            if (!CreateSymbolicLinkPrivate(symlinkFileName, targetFileName, num))
            {
                throw Win32Marshal.GetExceptionForLastWin32Error(path);
            }
            int lastWin32Error;
            if (!flag && (lastWin32Error = Marshal.GetLastWin32Error()) != 0)
            {
                throw Win32Marshal.GetExceptionForWin32Error(lastWin32Error, path);
            }
        }

        [DllImport(Libraries.Kernel32, CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
        internal static extern bool DeviceIoControl(SafeHandle hDevice, uint dwIoControlCode, IntPtr lpInBuffer, uint nInBufferSize, byte[] lpOutBuffer, uint nOutBufferSize, out uint lpBytesReturned, IntPtr lpOverlapped);

        [DllImport(Libraries.Kernel32, SetLastError = true)]
        internal static extern bool FindClose(IntPtr hFindFile);

        /// <summary>
        /// WARNING: This method does not implicitly handle long paths. Use FindFirstFile.
        /// </summary>
        [DllImport(Libraries.Kernel32, CharSet = CharSet.Unicode, EntryPoint = "FindFirstFileExW", ExactSpelling = true, SetLastError = true)]
        private static extern SafeFindHandle FindFirstFileExPrivate(string lpFileName, FINDEX_INFO_LEVELS fInfoLevelId, ref WIN32_FIND_DATA lpFindFileData, FINDEX_SEARCH_OPS fSearchOp, IntPtr lpSearchFilter, int dwAdditionalFlags);

        internal static SafeFindHandle FindFirstFile(string fileName, ref WIN32_FIND_DATA data)
        {
            fileName = System.IO.PathInternal.EnsureExtendedPrefixIfNeeded(fileName);
            return FindFirstFileExPrivate(fileName, FINDEX_INFO_LEVELS.FindExInfoBasic, ref data, FINDEX_SEARCH_OPS.FindExSearchNameMatch, IntPtr.Zero, 0);
        }

        /// <summary>
        /// WARNING: This method does not implicitly handle long paths. Use GetFileAttributesEx.
        /// </summary>
        [DllImport(Libraries.Kernel32, CharSet = CharSet.Unicode, EntryPoint = "GetFileAttributesExW", ExactSpelling = true, SetLastError = true)]
        private static extern bool GetFileAttributesExPrivate(string name, GET_FILEEX_INFO_LEVELS fileInfoLevel, ref WIN32_FILE_ATTRIBUTE_DATA lpFileInformation);

        internal static bool GetFileAttributesEx(string name, GET_FILEEX_INFO_LEVELS fileInfoLevel, ref WIN32_FILE_ATTRIBUTE_DATA lpFileInformation)
        {
            name = System.IO.PathInternal.EnsureExtendedPrefixIfNeeded(name);
            return GetFileAttributesExPrivate(name, fileInfoLevel, ref lpFileInformation);
        }

        [DllImport(Libraries.Kernel32, CharSet = CharSet.Unicode, EntryPoint = "GetFinalPathNameByHandleW", ExactSpelling = true, SetLastError = true)]
        internal unsafe static extern uint GetFinalPathNameByHandle(SafeFileHandle hFile, char* lpszFilePath, uint cchFilePath, uint dwFlags);

        /// <summary>
        /// WARNING: This method does not implicitly handle long paths. Use GetFullPathName or PathHelper.
        /// </summary>
        [DllImport(Libraries.Kernel32, BestFitMapping = false, CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
        internal static extern uint GetFullPathNameW(ref char lpFileName, uint nBufferLength, ref char lpBuffer, IntPtr lpFilePart);

        [DllImport(Libraries.Kernel32, SetLastError = true)]
        internal static extern int GetLogicalDrives();

        /// <summary>
        /// WARNING: This method does not implicitly handle long paths. Use GetFullPath/PathHelper.
        /// </summary>
        [DllImport(Libraries.Kernel32, BestFitMapping = false, CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
        internal static extern uint GetLongPathNameW(ref char lpszShortPath, ref char lpszLongPath, uint cchBuffer);

        [DllImport(Libraries.Kernel32, BestFitMapping = false, CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
        internal static extern uint GetTempFileNameW(ref char lpPathName, string lpPrefixString, uint uUnique, ref char lpTempFileName);

        [DllImport(Libraries.Kernel32, BestFitMapping = false, CharSet = CharSet.Unicode, ExactSpelling = true)]
        internal static extern uint GetTempPathW(int bufferLen, ref char buffer);

        [DllImport(Libraries.Kernel32, ExactSpelling = true, SetLastError = true)]
        [SuppressGCTransition]
        internal static extern bool SetThreadErrorMode(uint dwNewMode, out uint lpOldMode);

        internal static int CopyFile(string src, string dst, bool failIfExists)
        {
            int flags = (failIfExists ? 1 : 0);
            int cancel = 0;
            if (!CopyFileEx(src, dst, IntPtr.Zero, IntPtr.Zero, ref cancel, flags))
            {
                return Marshal.GetLastWin32Error();
            }
            return 0;
        }

        /// <summary>
        /// WARNING: This method does not implicitly handle long paths. Use CopyFileEx.
        /// </summary>
        [DllImport(Libraries.Kernel32, BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "CopyFileExW", SetLastError = true)]
        private static extern bool CopyFileExPrivate(string src, string dst, IntPtr progressRoutine, IntPtr progressData, ref int cancel, int flags);

        internal static bool CopyFileEx(string src, string dst, IntPtr progressRoutine, IntPtr progressData, ref int cancel, int flags)
        {
            src = System.IO.PathInternal.EnsureExtendedPrefixIfNeeded(src);
            dst = System.IO.PathInternal.EnsureExtendedPrefixIfNeeded(dst);
            return CopyFileExPrivate(src, dst, progressRoutine, progressData, ref cancel, flags);
        }

        /// <summary>
        /// WARNING: This method does not implicitly handle long paths. Use CreateDirectory.
        /// </summary>
        [DllImport(Libraries.Kernel32, BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "CreateDirectoryW", SetLastError = true)]
        private static extern bool CreateDirectoryPrivate(string path, ref SECURITY_ATTRIBUTES lpSecurityAttributes);

        internal static bool CreateDirectory(string path, ref SECURITY_ATTRIBUTES lpSecurityAttributes)
        {
            path = System.IO.PathInternal.EnsureExtendedPrefix(path);
            return CreateDirectoryPrivate(path, ref lpSecurityAttributes);
        }

        /// <summary>
        /// WARNING: This method does not implicitly handle long paths. Use CreateFile.
        /// </summary>
        [DllImport(Libraries.Kernel32, BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "CreateFileW", ExactSpelling = true, SetLastError = true)]
        private unsafe static extern SafeFileHandle CreateFilePrivate(string lpFileName, int dwDesiredAccess, FileShare dwShareMode, SECURITY_ATTRIBUTES* lpSecurityAttributes, FileMode dwCreationDisposition, int dwFlagsAndAttributes, IntPtr hTemplateFile);

        internal unsafe static SafeFileHandle CreateFile(string lpFileName, int dwDesiredAccess, FileShare dwShareMode, SECURITY_ATTRIBUTES* lpSecurityAttributes, FileMode dwCreationDisposition, int dwFlagsAndAttributes, IntPtr hTemplateFile)
        {
            lpFileName = System.IO.PathInternal.EnsureExtendedPrefixIfNeeded(lpFileName);
            return CreateFilePrivate(lpFileName, dwDesiredAccess, dwShareMode, lpSecurityAttributes, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
        }

        internal unsafe static SafeFileHandle CreateFile(string lpFileName, int dwDesiredAccess, FileShare dwShareMode, FileMode dwCreationDisposition, int dwFlagsAndAttributes)
        {
            lpFileName = System.IO.PathInternal.EnsureExtendedPrefixIfNeeded(lpFileName);
            return CreateFilePrivate(lpFileName, dwDesiredAccess, dwShareMode, null, dwCreationDisposition, dwFlagsAndAttributes, IntPtr.Zero);
        }

        /// <summary>
        /// WARNING: This method does not implicitly handle long paths. Use CreateFile.
        /// </summary>
        [DllImport(Libraries.Kernel32, BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "CreateFileW", ExactSpelling = true, SetLastError = true)]
        private unsafe static extern IntPtr CreateFilePrivate_IntPtr(string lpFileName, int dwDesiredAccess, FileShare dwShareMode, SECURITY_ATTRIBUTES* lpSecurityAttributes, FileMode dwCreationDisposition, int dwFlagsAndAttributes, IntPtr hTemplateFile);

        internal unsafe static IntPtr CreateFile_IntPtr(string lpFileName, int dwDesiredAccess, FileShare dwShareMode, FileMode dwCreationDisposition, int dwFlagsAndAttributes)
        {
            lpFileName = System.IO.PathInternal.EnsureExtendedPrefixIfNeeded(lpFileName);
            return CreateFilePrivate_IntPtr(lpFileName, dwDesiredAccess, dwShareMode, null, dwCreationDisposition, dwFlagsAndAttributes, IntPtr.Zero);
        }

        /// <summary>
        /// WARNING: This method does not implicitly handle long paths. Use DeleteFile.
        /// </summary>
        [DllImport(Libraries.Kernel32, BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "DeleteFileW", SetLastError = true)]
        private static extern bool DeleteFilePrivate(string path);

        internal static bool DeleteFile(string path)
        {
            path = System.IO.PathInternal.EnsureExtendedPrefixIfNeeded(path);
            return DeleteFilePrivate(path);
        }

        /// <summary>
        /// WARNING: This method does not implicitly handle long paths. Use DeleteVolumeMountPoint.
        /// </summary>
        [DllImport(Libraries.Kernel32, BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "DeleteVolumeMountPointW", SetLastError = true)]
        internal static extern bool DeleteVolumeMountPointPrivate(string mountPoint);

        internal static bool DeleteVolumeMountPoint(string mountPoint)
        {
            mountPoint = System.IO.PathInternal.EnsureExtendedPrefixIfNeeded(mountPoint);
            return DeleteVolumeMountPointPrivate(mountPoint);
        }

        [DllImport(Libraries.Kernel32, BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "FindNextFileW", SetLastError = true)]
        internal static extern bool FindNextFile(Microsoft.Win32.SafeHandles.SafeFindHandle hndFindFile, ref WIN32_FIND_DATA lpFindFileData);

        [DllImport(Libraries.Kernel32, BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "GetVolumeInformationW", SetLastError = true)]
        internal unsafe static extern bool GetVolumeInformation(string drive, char* volumeName, int volumeNameBufLen, int* volSerialNumber, int* maxFileNameLen, out int fileSystemFlags, char* fileSystemName, int fileSystemNameBufLen);

        /// <summary>
        /// WARNING: This method does not implicitly handle long paths. Use MoveFile.
        /// </summary>
        [DllImport(Libraries.Kernel32, BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "MoveFileExW", SetLastError = true)]
        private static extern bool MoveFileExPrivate(string src, string dst, uint flags);

        /// <summary>
        /// Moves a file or directory, optionally overwriting existing destination file. NOTE: overwrite must be false for directories.
        /// </summary>
        /// <param name="src">Source file or directory</param>
        /// <param name="dst">Destination file or directory</param>
        /// <param name="overwrite">True to overwrite existing destination file. NOTE: must pass false for directories as overwrite of directories is not supported.</param>
        /// <returns></returns>
        internal static bool MoveFile(string src, string dst, bool overwrite)
        {
            src = System.IO.PathInternal.EnsureExtendedPrefixIfNeeded(src);
            dst = System.IO.PathInternal.EnsureExtendedPrefixIfNeeded(dst);
            uint num = 2u;
            if (overwrite)
            {
                num |= 1u;
            }
            return MoveFileExPrivate(src, dst, num);
        }

        /// <summary>
        /// WARNING: This method does not implicitly handle long paths. Use RemoveDirectory.
        /// </summary>
        [DllImport(Libraries.Kernel32, BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "RemoveDirectoryW", SetLastError = true)]
        private static extern bool RemoveDirectoryPrivate(string path);

        internal static bool RemoveDirectory(string path)
        {
            path = System.IO.PathInternal.EnsureExtendedPrefixIfNeeded(path);
            return RemoveDirectoryPrivate(path);
        }

        [DllImport(Libraries.Kernel32, BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "ReplaceFileW", SetLastError = true)]
        private static extern bool ReplaceFilePrivate(string replacedFileName, string replacementFileName, string backupFileName, int dwReplaceFlags, IntPtr lpExclude, IntPtr lpReserved);

        internal static bool ReplaceFile(string replacedFileName, string replacementFileName, string backupFileName, int dwReplaceFlags, IntPtr lpExclude, IntPtr lpReserved)
        {
            replacedFileName = System.IO.PathInternal.EnsureExtendedPrefixIfNeeded(replacedFileName);
            replacementFileName = System.IO.PathInternal.EnsureExtendedPrefixIfNeeded(replacementFileName);
            backupFileName = System.IO.PathInternal.EnsureExtendedPrefixIfNeeded(backupFileName);
            return ReplaceFilePrivate(replacedFileName, replacementFileName, backupFileName, dwReplaceFlags, lpExclude, lpReserved);
        }

        /// <summary>
        /// WARNING: This method does not implicitly handle long paths. Use SetFileAttributes.
        /// </summary>
        [DllImport(Libraries.Kernel32, BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "SetFileAttributesW", SetLastError = true)]
        private static extern bool SetFileAttributesPrivate(string name, int attr);

        internal static bool SetFileAttributes(string name, int attr)
        {
            name = System.IO.PathInternal.EnsureExtendedPrefixIfNeeded(name);
            return SetFileAttributesPrivate(name, attr);
        }

        [DllImport(Libraries.Kernel32, ExactSpelling = true, SetLastError = true)]
        internal unsafe static extern bool SetFileInformationByHandle(SafeFileHandle hFile, int FileInformationClass, void* lpFileInformation, uint dwBufferSize);
    }

    internal static class Advapi32
    {
        /// <summary>
        /// WARNING: This method does not implicitly handle long paths. Use EncryptFile.
        /// </summary>
        [DllImport(Libraries.Advapi32, CharSet = CharSet.Unicode, EntryPoint = "EncryptFileW", SetLastError = true)]
        private static extern bool EncryptFilePrivate(string lpFileName);

        internal static bool EncryptFile(string path)
        {
            path = System.IO.PathInternal.EnsureExtendedPrefixIfNeeded(path);
            return EncryptFilePrivate(path);
        }

        /// <summary>
        /// WARNING: This method does not implicitly handle long paths. Use DecryptFile.
        /// </summary>
        [DllImport(Libraries.Advapi32, CharSet = CharSet.Unicode, EntryPoint = "DecryptFileW", SetLastError = true)]
        private static extern bool DecryptFileFilePrivate(string lpFileName, int dwReserved);

        internal static bool DecryptFile(string path)
        {
            path = System.IO.PathInternal.EnsureExtendedPrefixIfNeeded(path);
            return DecryptFileFilePrivate(path, 0);
        }
    }

    internal static class Errors
    {
        internal const int ERROR_SUCCESS = 0;

        internal const int ERROR_INVALID_FUNCTION = 1;

        internal const int ERROR_FILE_NOT_FOUND = 2;

        internal const int ERROR_PATH_NOT_FOUND = 3;

        internal const int ERROR_ACCESS_DENIED = 5;

        internal const int ERROR_INVALID_HANDLE = 6;

        internal const int ERROR_NOT_ENOUGH_MEMORY = 8;

        internal const int ERROR_INVALID_DATA = 13;

        internal const int ERROR_INVALID_DRIVE = 15;

        internal const int ERROR_NO_MORE_FILES = 18;

        internal const int ERROR_NOT_READY = 21;

        internal const int ERROR_BAD_COMMAND = 22;

        internal const int ERROR_BAD_LENGTH = 24;

        internal const int ERROR_SHARING_VIOLATION = 32;

        internal const int ERROR_LOCK_VIOLATION = 33;

        internal const int ERROR_HANDLE_EOF = 38;

        internal const int ERROR_NOT_SUPPORTED = 50;

        internal const int ERROR_BAD_NETPATH = 53;

        internal const int ERROR_NETWORK_ACCESS_DENIED = 65;

        internal const int ERROR_BAD_NET_NAME = 67;

        internal const int ERROR_FILE_EXISTS = 80;

        internal const int ERROR_INVALID_PARAMETER = 87;

        internal const int ERROR_BROKEN_PIPE = 109;

        internal const int ERROR_DISK_FULL = 112;

        internal const int ERROR_SEM_TIMEOUT = 121;

        internal const int ERROR_CALL_NOT_IMPLEMENTED = 120;

        internal const int ERROR_INSUFFICIENT_BUFFER = 122;

        internal const int ERROR_INVALID_NAME = 123;

        internal const int ERROR_NEGATIVE_SEEK = 131;

        internal const int ERROR_DIR_NOT_EMPTY = 145;

        internal const int ERROR_BAD_PATHNAME = 161;

        internal const int ERROR_LOCK_FAILED = 167;

        internal const int ERROR_BUSY = 170;

        internal const int ERROR_ALREADY_EXISTS = 183;

        internal const int ERROR_BAD_EXE_FORMAT = 193;

        internal const int ERROR_ENVVAR_NOT_FOUND = 203;

        internal const int ERROR_FILENAME_EXCED_RANGE = 206;

        internal const int ERROR_EXE_MACHINE_TYPE_MISMATCH = 216;

        internal const int ERROR_FILE_TOO_LARGE = 223;

        internal const int ERROR_PIPE_BUSY = 231;

        internal const int ERROR_NO_DATA = 232;

        internal const int ERROR_PIPE_NOT_CONNECTED = 233;

        internal const int ERROR_MORE_DATA = 234;

        internal const int ERROR_NO_MORE_ITEMS = 259;

        internal const int ERROR_DIRECTORY = 267;

        internal const int ERROR_NOT_OWNER = 288;

        internal const int ERROR_TOO_MANY_POSTS = 298;

        internal const int ERROR_PARTIAL_COPY = 299;

        internal const int ERROR_ARITHMETIC_OVERFLOW = 534;

        internal const int ERROR_PIPE_CONNECTED = 535;

        internal const int ERROR_PIPE_LISTENING = 536;

        internal const int ERROR_MUTANT_LIMIT_EXCEEDED = 587;

        internal const int ERROR_OPERATION_ABORTED = 995;

        internal const int ERROR_IO_INCOMPLETE = 996;

        internal const int ERROR_IO_PENDING = 997;

        internal const int ERROR_NO_TOKEN = 1008;

        internal const int ERROR_SERVICE_DOES_NOT_EXIST = 1060;

        internal const int ERROR_NO_UNICODE_TRANSLATION = 1113;

        internal const int ERROR_DLL_INIT_FAILED = 1114;

        internal const int ERROR_COUNTER_TIMEOUT = 1121;

        internal const int ERROR_NO_ASSOCIATION = 1155;

        internal const int ERROR_DDE_FAIL = 1156;

        internal const int ERROR_DLL_NOT_FOUND = 1157;

        internal const int ERROR_NOT_FOUND = 1168;

        internal const int ERROR_NETWORK_UNREACHABLE = 1231;

        internal const int ERROR_NON_ACCOUNT_SID = 1257;

        internal const int ERROR_NOT_ALL_ASSIGNED = 1300;

        internal const int ERROR_UNKNOWN_REVISION = 1305;

        internal const int ERROR_INVALID_OWNER = 1307;

        internal const int ERROR_INVALID_PRIMARY_GROUP = 1308;

        internal const int ERROR_NO_SUCH_PRIVILEGE = 1313;

        internal const int ERROR_PRIVILEGE_NOT_HELD = 1314;

        internal const int ERROR_INVALID_ACL = 1336;

        internal const int ERROR_INVALID_SECURITY_DESCR = 1338;

        internal const int ERROR_INVALID_SID = 1337;

        internal const int ERROR_BAD_IMPERSONATION_LEVEL = 1346;

        internal const int ERROR_CANT_OPEN_ANONYMOUS = 1347;

        internal const int ERROR_NO_SECURITY_ON_OBJECT = 1350;

        internal const int ERROR_CANNOT_IMPERSONATE = 1368;

        internal const int ERROR_CLASS_ALREADY_EXISTS = 1410;

        internal const int ERROR_NO_SYSTEM_RESOURCES = 1450;

        internal const int ERROR_TIMEOUT = 1460;

        internal const int ERROR_EVENTLOG_FILE_CHANGED = 1503;

        internal const int ERROR_TRUSTED_RELATIONSHIP_FAILURE = 1789;

        internal const int ERROR_RESOURCE_LANG_NOT_FOUND = 1815;

        internal const int ERROR_NOT_A_REPARSE_POINT = 4390;
    }

    /// <summary>
    /// 100-nanosecond intervals (ticks) since January 1, 1601 (UTC).
    /// </summary>
    /// <remarks>
    /// For NT times that are defined as longs (LARGE_INTEGER, etc.).
    /// Do NOT use for FILETIME unless you are POSITIVE it will fall on an
    /// 8 byte boundary.
    /// </remarks>
    internal struct LongFileTime
    {
        /// <summary>
        /// 100-nanosecond intervals (ticks) since January 1, 1601 (UTC).
        /// </summary>
        internal long TicksSince1601;

        internal DateTimeOffset ToDateTimeOffset()
        {
            return new DateTimeOffset(DateTime.FromFileTimeUtc(TicksSince1601));
        }
    }

    /// <summary>
    /// <a href="https://msdn.microsoft.com/en-us/library/windows/hardware/ff557749.aspx">OBJECT_ATTRIBUTES</a> structure.
    /// The OBJECT_ATTRIBUTES structure specifies attributes that can be applied to objects or object handles by routines
    /// that create objects and/or return handles to objects.
    /// </summary>
    internal struct OBJECT_ATTRIBUTES
    {
        public uint Length;

        /// <summary>
        /// Optional handle to root object directory for the given ObjectName.
        /// Can be a file system directory or object manager directory.
        /// </summary>
        public IntPtr RootDirectory;

        /// <summary>
        /// Name of the object. Must be fully qualified if RootDirectory isn't set.
        /// Otherwise is relative to RootDirectory.
        /// </summary>
        public unsafe UNICODE_STRING* ObjectName;

        public ObjectAttributes Attributes;

        /// <summary>
        /// If null, object will receive default security settings.
        /// </summary>
        public unsafe void* SecurityDescriptor;

        /// <summary>
        /// Optional quality of service to be applied to the object. Used to indicate
        /// security impersonation level and context tracking mode (dynamic or static).
        /// </summary>
        public unsafe SECURITY_QUALITY_OF_SERVICE* SecurityQualityOfService;

        /// <summary>
        /// Equivalent of InitializeObjectAttributes macro with the exception that you can directly set SQOS.
        /// </summary>
        public unsafe OBJECT_ATTRIBUTES(UNICODE_STRING* objectName, ObjectAttributes attributes, IntPtr rootDirectory, SECURITY_QUALITY_OF_SERVICE* securityQualityOfService = null)
        {
            Length = (uint)sizeof(OBJECT_ATTRIBUTES);
            RootDirectory = rootDirectory;
            ObjectName = objectName;
            Attributes = attributes;
            SecurityDescriptor = null;
            SecurityQualityOfService = securityQualityOfService;
        }
    }

    [Flags]
    public enum ObjectAttributes : uint
    {
        /// <summary>
        /// This handle can be inherited by child processes of the current process.
        /// </summary>
        OBJ_INHERIT = 2u,
        /// <summary>
        /// This flag only applies to objects that are named within the object manager.
        /// By default, such objects are deleted when all open handles to them are closed.
        /// If this flag is specified, the object is not deleted when all open handles are closed.
        /// </summary>
        OBJ_PERMANENT = 0x10u,
        /// <summary>
        /// Only a single handle can be open for this object.
        /// </summary>
        OBJ_EXCLUSIVE = 0x20u,
        /// <summary>
        /// Lookups for this object should be case insensitive.
        /// </summary>
        OBJ_CASE_INSENSITIVE = 0x40u,
        /// <summary>
        /// Create on existing object should open, not fail with STATUS_OBJECT_NAME_COLLISION.
        /// </summary>
        OBJ_OPENIF = 0x80u,
        /// <summary>
        /// Open the symbolic link, not its target.
        /// </summary>
        OBJ_OPENLINK = 0x100u
    }

    internal struct UNICODE_STRING
    {
        /// <summary>
        /// Length in bytes, not including the null terminator, if any.
        /// </summary>
        internal ushort Length;

        /// <summary>
        /// Max size of the buffer in bytes
        /// </summary>
        internal ushort MaximumLength;

        internal IntPtr Buffer;
    }

    /// <summary>
    /// <a href="https://docs.microsoft.com/en-us/windows/win32/api/winnt/ns-winnt-security_quality_of_service">SECURITY_QUALITY_OF_SERVICE</a> structure.
    ///  Used to support client impersonation. Client specifies this to a server to allow
    ///  it to impersonate the client.
    /// </summary>
    internal struct SECURITY_QUALITY_OF_SERVICE
    {
        public uint Length;

        public ImpersonationLevel ImpersonationLevel;

        public ContextTrackingMode ContextTrackingMode;

        public BOOLEAN EffectiveOnly;

        public unsafe SECURITY_QUALITY_OF_SERVICE(ImpersonationLevel impersonationLevel, ContextTrackingMode contextTrackingMode, bool effectiveOnly)
        {
            Length = (uint)sizeof(SECURITY_QUALITY_OF_SERVICE);
            ImpersonationLevel = impersonationLevel;
            ContextTrackingMode = contextTrackingMode;
            EffectiveOnly = (effectiveOnly ? BOOLEAN.TRUE : BOOLEAN.FALSE);
        }
    }

    /// <summary>
    /// <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/aa379572.aspx">SECURITY_IMPERSONATION_LEVEL</a> enumeration values.
    ///  [SECURITY_IMPERSONATION_LEVEL]
    /// </summary>
    public enum ImpersonationLevel : uint
    {
        /// <summary>
        ///  The server process cannot obtain identification information about the client and cannot impersonate the client.
        ///  [SecurityAnonymous]
        /// </summary>
        Anonymous,
        /// <summary>
        ///  The server process can obtain identification information about the client, but cannot impersonate the client.
        ///  [SecurityIdentification]
        /// </summary>
        Identification,
        /// <summary>
        ///  The server process can impersonate the client's security context on it's local system.
        ///  [SecurityImpersonation]
        /// </summary>
        Impersonation,
        /// <summary>
        ///  The server process can impersonate the client's security context on remote systems.
        ///  [SecurityDelegation]
        /// </summary>
        Delegation
    }

    /// <summary>
    /// <a href="https://msdn.microsoft.com/en-us/library/cc234317.aspx">SECURITY_CONTEXT_TRACKING_MODE</a>
    /// </summary>
    public enum ContextTrackingMode : byte
    {
        /// <summary>
        ///  The server is given a snapshot of the client's security context.
        ///  [SECURITY_STATIC_TRACKING]
        /// </summary>
        Static,
        /// <summary>
        ///  The server is continually updated with changes.
        ///  [SECURITY_DYNAMIC_TRACKING]
        /// </summary>
        Dynamic
    }

    internal static class NtDll
    {
        /// <summary>
        /// <a href="https://msdn.microsoft.com/en-us/library/windows/hardware/ff540289.aspx">FILE_FULL_DIR_INFORMATION</a> structure.
        /// Used with GetFileInformationByHandleEx and FileIdBothDirectoryInfo/RestartInfo as well as NtQueryFileInformation.
        /// Equivalent to <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/hh447298.aspx">FILE_FULL_DIR_INFO</a> structure.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct FILE_FULL_DIR_INFORMATION
        {
            /// <summary>
            /// Offset in bytes of the next entry, if any.
            /// </summary>
            public uint NextEntryOffset;

            /// <summary>
            /// Byte offset within the parent directory, undefined for NTFS.
            /// </summary>
            public uint FileIndex;

            public LongFileTime CreationTime;

            public LongFileTime LastAccessTime;

            public LongFileTime LastWriteTime;

            public LongFileTime ChangeTime;

            public long EndOfFile;

            public long AllocationSize;

            /// <summary>
            /// File attributes.
            /// </summary>
            /// <remarks>
            /// Note that MSDN documentation isn't correct for this- it can return
            /// any FILE_ATTRIBUTE that is currently set on the file, not just the
            /// ones documented.
            /// </remarks>
            public FileAttributes FileAttributes;

            /// <summary>
            /// The length of the file name in bytes (without null).
            /// </summary>
            public uint FileNameLength;

            /// <summary>
            /// The extended attribute size OR the reparse tag if a reparse point.
            /// </summary>
            public uint EaSize;

            private char _fileName;

            public unsafe System.ReadOnlySpan<char> FileName
            {
                get
                {
                    fixed (char* ptr = &_fileName)
                    {
                        return new System.ReadOnlySpan<char>((void*)ptr, (int)FileNameLength / 2);
                    }
                }
            }

            /// <summary>
            /// Gets the next info pointer or null if there are no more.
            /// </summary>
            public unsafe static FILE_FULL_DIR_INFORMATION* GetNextInfo(FILE_FULL_DIR_INFORMATION* info)
            {
                if (info == null)
                {
                    return null;
                }
                uint nextEntryOffset = info->NextEntryOffset;
                if (nextEntryOffset == 0)
                {
                    return null;
                }
                return (FILE_FULL_DIR_INFORMATION*)((byte*)info + nextEntryOffset);
            }
        }

        public enum FILE_INFORMATION_CLASS : uint
        {
            FileDirectoryInformation = 1u,
            FileFullDirectoryInformation,
            FileBothDirectoryInformation,
            FileBasicInformation,
            FileStandardInformation,
            FileInternalInformation,
            FileEaInformation,
            FileAccessInformation,
            FileNameInformation,
            FileRenameInformation,
            FileLinkInformation,
            FileNamesInformation,
            FileDispositionInformation,
            FilePositionInformation,
            FileFullEaInformation,
            FileModeInformation,
            FileAlignmentInformation,
            FileAllInformation,
            FileAllocationInformation,
            FileEndOfFileInformation,
            FileAlternateNameInformation,
            FileStreamInformation,
            FilePipeInformation,
            FilePipeLocalInformation,
            FilePipeRemoteInformation,
            FileMailslotQueryInformation,
            FileMailslotSetInformation,
            FileCompressionInformation,
            FileObjectIdInformation,
            FileCompletionInformation,
            FileMoveClusterInformation,
            FileQuotaInformation,
            FileReparsePointInformation,
            FileNetworkOpenInformation,
            FileAttributeTagInformation,
            FileTrackingInformation,
            FileIdBothDirectoryInformation,
            FileIdFullDirectoryInformation,
            FileValidDataLengthInformation,
            FileShortNameInformation,
            FileIoCompletionNotificationInformation,
            FileIoStatusBlockRangeInformation,
            FileIoPriorityHintInformation,
            FileSfioReserveInformation,
            FileSfioVolumeInformation,
            FileHardLinkInformation,
            FileProcessIdsUsingFileInformation,
            FileNormalizedNameInformation,
            FileNetworkPhysicalNameInformation,
            FileIdGlobalTxDirectoryInformation,
            FileIsRemoteDeviceInformation,
            FileUnusedInformation,
            FileNumaNodeInformation,
            FileStandardLinkInformation,
            FileRemoteProtocolInformation,
            FileRenameInformationBypassAccessCheck,
            FileLinkInformationBypassAccessCheck,
            FileVolumeNameInformation,
            FileIdInformation,
            FileIdExtdDirectoryInformation,
            FileReplaceCompletionInformation,
            FileHardLinkFullIdInformation,
            FileIdExtdBothDirectoryInformation,
            FileDispositionInformationEx,
            FileRenameInformationEx,
            FileRenameInformationExBypassAccessCheck,
            FileDesiredStorageClassInformation,
            FileStatInformation
        }

        public struct IO_STATUS_BLOCK
        {
            [StructLayout(LayoutKind.Explicit)]
            public struct IO_STATUS
            {
                /// <summary>
                /// The completion status, either STATUS_SUCCESS if the operation was completed successfully or
                /// some other informational, warning, or error status.
                /// </summary>
                [FieldOffset(0)]
                public uint Status;

                /// <summary>
                /// Reserved for internal use.
                /// </summary>
                [FieldOffset(0)]
                public IntPtr Pointer;
            }

            /// <summary>
            /// Status
            /// </summary>
            public IO_STATUS Status;

            /// <summary>
            /// Request dependent value.
            /// </summary>
            public IntPtr Information;
        }

        /// <summary>
        /// File creation disposition when calling directly to NT APIs.
        /// </summary>
        public enum CreateDisposition : uint
        {
            /// <summary>
            /// Default. Replace or create. Deletes existing file instead of overwriting.
            /// </summary>
            /// <remarks>
            /// As this potentially deletes it requires that DesiredAccess must include Delete.
            /// This has no equivalent in CreateFile.
            /// </remarks>
            FILE_SUPERSEDE,
            /// <summary>
            /// Open if exists or fail if doesn't exist. Equivalent to OPEN_EXISTING or
            /// <see cref="F:System.IO.FileMode.Open" />.
            /// </summary>
            /// <remarks>
            /// TruncateExisting also uses Open and then manually truncates the file
            /// by calling NtSetInformationFile with FileAllocationInformation and an
            /// allocation size of 0.
            /// </remarks>
            FILE_OPEN,
            /// <summary>
            /// Create if doesn't exist or fail if does exist. Equivalent to CREATE_NEW
            /// or <see cref="F:System.IO.FileMode.CreateNew" />.
            /// </summary>
            FILE_CREATE,
            /// <summary>
            /// Open if exists or create if doesn't exist. Equivalent to OPEN_ALWAYS or
            /// <see cref="F:System.IO.FileMode.OpenOrCreate" />.
            /// </summary>
            FILE_OPEN_IF,
            /// <summary>
            /// Open and overwrite if exists or fail if doesn't exist. Equivalent to
            /// TRUNCATE_EXISTING or <see cref="F:System.IO.FileMode.Truncate" />.
            /// </summary>
            FILE_OVERWRITE,
            /// <summary>
            /// Open and overwrite if exists or create if doesn't exist. Equivalent to
            /// CREATE_ALWAYS or <see cref="F:System.IO.FileMode.Create" />.
            /// </summary>
            FILE_OVERWRITE_IF
        }

        /// <summary>
        /// Options for creating/opening files with NtCreateFile.
        /// </summary>
        public enum CreateOptions : uint
        {
            /// <summary>
            /// File being created or opened must be a directory file. Disposition must be FILE_CREATE, FILE_OPEN,
            /// or FILE_OPEN_IF.
            /// </summary>
            /// <remarks>
            /// Can only be used with FILE_SYNCHRONOUS_IO_ALERT/NONALERT, FILE_WRITE_THROUGH, FILE_OPEN_FOR_BACKUP_INTENT,
            /// and FILE_OPEN_BY_FILE_ID flags.
            /// </remarks>
            FILE_DIRECTORY_FILE = 1u,
            /// <summary>
            /// Applications that write data to the file must actually transfer the data into
            /// the file before any requested write operation is considered complete. This flag
            /// is set automatically if FILE_NO_INTERMEDIATE_BUFFERING is set.
            /// </summary>
            FILE_WRITE_THROUGH = 2u,
            /// <summary>
            /// All accesses to the file are sequential.
            /// </summary>
            FILE_SEQUENTIAL_ONLY = 4u,
            /// <summary>
            /// File cannot be cached in driver buffers. Cannot use with AppendData desired access.
            /// </summary>
            FILE_NO_INTERMEDIATE_BUFFERING = 8u,
            /// <summary>
            /// All operations are performed synchronously. Any wait on behalf of the caller is
            /// subject to premature termination from alerts.
            /// </summary>
            /// <remarks>
            /// Cannot be used with FILE_SYNCHRONOUS_IO_NONALERT.
            /// Synchronous DesiredAccess flag is required. I/O system will maintain file position context.
            /// </remarks>
            FILE_SYNCHRONOUS_IO_ALERT = 0x10u,
            /// <summary>
            /// All operations are performed synchronously. Waits in the system to synchronize I/O queuing
            /// and completion are not subject to alerts.
            /// </summary>
            /// <remarks>
            /// Cannot be used with FILE_SYNCHRONOUS_IO_ALERT.
            /// Synchronous DesiredAccess flag is required. I/O system will maintain file position context.
            /// </remarks>
            FILE_SYNCHRONOUS_IO_NONALERT = 0x20u,
            /// <summary>
            /// File being created or opened must not be a directory file. Can be a data file, device,
            /// or volume.
            /// </summary>
            FILE_NON_DIRECTORY_FILE = 0x40u,
            /// <summary>
            /// Create a tree connection for this file in order to open it over the network.
            /// </summary>
            /// <remarks>
            /// Not used by device and intermediate drivers.
            /// </remarks>
            FILE_CREATE_TREE_CONNECTION = 0x80u,
            /// <summary>
            /// Complete the operation immediately with a success code of STATUS_OPLOCK_BREAK_IN_PROGRESS if
            /// the target file is oplocked.
            /// </summary>
            /// <remarks>
            /// Not compatible with ReserveOpfilter or OpenRequiringOplock.
            /// Not used by device and intermediate drivers.
            /// </remarks>
            FILE_COMPLETE_IF_OPLOCKED = 0x100u,
            /// <summary>
            /// If the extended attributes on an existing file being opened indicate that the caller must
            /// understand extended attributes to properly interpret the file, fail the request.
            /// </summary>
            /// <remarks>
            /// Not used by device and intermediate drivers.
            /// </remarks>
            FILE_NO_EA_KNOWLEDGE = 0x200u,
            /// <summary>
            /// Accesses to the file can be random, so no sequential read-ahead operations should be performed
            /// on the file by FSDs or the system.
            /// </summary>
            FILE_RANDOM_ACCESS = 0x800u,
            /// <summary>
            /// Delete the file when the last handle to it is passed to NtClose. Requires Delete flag in
            /// DesiredAccess parameter.
            /// </summary>
            FILE_DELETE_ON_CLOSE = 0x1000u,
            /// <summary>
            /// Open the file by reference number or object ID. The file name that is specified by the ObjectAttributes
            /// name parameter includes the 8 or 16 byte file reference number or ID for the file in the ObjectAttributes
            /// name field. The device name can optionally be prefixed.
            /// </summary>
            /// <remarks>
            /// NTFS supports both reference numbers and object IDs. 16 byte reference numbers are 8 byte numbers padded
            /// with zeros. ReFS only supports reference numbers (not object IDs). 8 byte and 16 byte reference numbers
            /// are not related. Note that as the UNICODE_STRING will contain raw byte data, it may not be a "valid" string.
            /// Not used by device and intermediate drivers.
            /// </remarks>
            /// <example>
            /// \??\C:\{8 bytes of binary FileID}
            /// \device\HardDiskVolume1\{16 bytes of binary ObjectID}
            /// {8 bytes of binary FileID}
            /// </example>
            FILE_OPEN_BY_FILE_ID = 0x2000u,
            /// <summary>
            /// The file is being opened for backup intent. Therefore, the system should check for certain access rights
            /// and grant the caller the appropriate access to the file before checking the DesiredAccess parameter
            /// against the file's security descriptor.
            /// </summary>
            /// <remarks>
            /// Not used by device and intermediate drivers.
            /// </remarks>
            FILE_OPEN_FOR_BACKUP_INTENT = 0x4000u,
            /// <summary>
            /// When creating a file, specifies that it should not inherit the compression bit from the parent directory.
            /// </summary>
            FILE_NO_COMPRESSION = 0x8000u,
            /// <summary>
            /// The file is being opened and an opportunistic lock (oplock) on the file is being requested as a single atomic
            /// operation.
            /// </summary>
            /// <remarks>
            /// The file system checks for oplocks before it performs the create operation and will fail the create with a
            /// return code of STATUS_CANNOT_BREAK_OPLOCK if the result would be to break an existing oplock.
            /// Not compatible with CompleteIfOplocked or ReserveOpFilter. Windows 7 and up.
            /// </remarks>
            FILE_OPEN_REQUIRING_OPLOCK = 0x10000u,
            /// <summary>
            /// CreateFile2 uses this flag to prevent opening a file that you don't have access to without specifying
            /// FILE_SHARE_READ. (Preventing users that can only read a file from denying access to other readers.)
            /// </summary>
            /// <remarks>
            /// Windows 7 and up.
            /// </remarks>
            FILE_DISALLOW_EXCLUSIVE = 0x20000u,
            /// <summary>
            /// The client opening the file or device is session aware and per session access is validated if necessary.
            /// </summary>
            /// <remarks>
            /// Windows 8 and up.
            /// </remarks>
            FILE_SESSION_AWARE = 0x40000u,
            /// <summary>
            /// This flag allows an application to request a filter opportunistic lock (oplock) to prevent other applications
            /// from getting share violations.
            /// </summary>
            /// <remarks>
            /// Not compatible with CompleteIfOplocked or OpenRequiringOplock.
            /// If there are already open handles, the create request will fail with STATUS_OPLOCK_NOT_GRANTED.
            /// </remarks>
            FILE_RESERVE_OPFILTER = 0x100000u,
            /// <summary>
            /// Open a file with a reparse point attribute, bypassing the normal reparse point processing.
            /// </summary>
            FILE_OPEN_REPARSE_POINT = 0x200000u,
            /// <summary>
            /// Causes files that are marked with the Offline attribute not to be recalled from remote storage.
            /// </summary>
            /// <remarks>
            /// More details can be found in Remote Storage documentation (see Basic Concepts).
            /// https://technet.microsoft.com/en-us/library/cc938459.aspx
            /// </remarks>
            FILE_OPEN_NO_RECALL = 0x400000u
        }

        /// <summary>
        /// System.IO.FileAccess looks up these values when creating handles
        /// </summary>
        /// <remarks>
        /// File Security and Access Rights
        /// https://msdn.microsoft.com/en-us/library/windows/desktop/aa364399.aspx
        /// </remarks>
        [Flags]
        public enum DesiredAccess : uint
        {
            /// <summary>
            /// For a file, the right to read data from the file.
            /// </summary>
            /// <remarks>
            /// Directory version of this flag is <see cref="F:Interop.NtDll.DesiredAccess.FILE_LIST_DIRECTORY" />.
            /// </remarks>
            FILE_READ_DATA = 1u,
            /// <summary>
            /// For a directory, the right to list the contents.
            /// </summary>
            /// <remarks>
            /// File version of this flag is <see cref="F:Interop.NtDll.DesiredAccess.FILE_READ_DATA" />.
            /// </remarks>
            FILE_LIST_DIRECTORY = 1u,
            /// <summary>
            /// For a file, the right to write data to the file.
            /// </summary>
            /// <remarks>
            /// Directory version of this flag is <see cref="F:Interop.NtDll.DesiredAccess.FILE_ADD_FILE" />.
            /// </remarks>
            FILE_WRITE_DATA = 2u,
            /// <summary>
            /// For a directory, the right to create a file in a directory.
            /// </summary>
            /// <remarks>
            /// File version of this flag is <see cref="F:Interop.NtDll.DesiredAccess.FILE_WRITE_DATA" />.
            /// </remarks>
            FILE_ADD_FILE = 2u,
            /// <summary>
            /// For a file, the right to append data to a file. <see cref="F:Interop.NtDll.DesiredAccess.FILE_WRITE_DATA" /> is needed
            /// to overwrite existing data.
            /// </summary>
            /// <remarks>
            /// Directory version of this flag is <see cref="F:Interop.NtDll.DesiredAccess.FILE_ADD_SUBDIRECTORY" />.
            /// </remarks>
            FILE_APPEND_DATA = 4u,
            /// <summary>
            /// For a directory, the right to create a subdirectory.
            /// </summary>
            /// <remarks>
            /// File version of this flag is <see cref="F:Interop.NtDll.DesiredAccess.FILE_APPEND_DATA" />.
            /// </remarks>
            FILE_ADD_SUBDIRECTORY = 4u,
            /// <summary>
            /// For a named pipe, the right to create a pipe instance.
            /// </summary>
            FILE_CREATE_PIPE_INSTANCE = 4u,
            /// <summary>
            /// The right to read extended attributes.
            /// </summary>
            FILE_READ_EA = 8u,
            /// <summary>
            /// The right to write extended attributes.
            /// </summary>
            FILE_WRITE_EA = 0x10u,
            /// <summary>
            /// The right to execute the file.
            /// </summary>
            /// <remarks>
            /// Directory version of this flag is <see cref="F:Interop.NtDll.DesiredAccess.FILE_TRAVERSE" />.
            /// </remarks>
            FILE_EXECUTE = 0x20u,
            /// <summary>
            /// For a directory, the right to traverse the directory.
            /// </summary>
            /// <remarks>
            /// File version of this flag is <see cref="F:Interop.NtDll.DesiredAccess.FILE_EXECUTE" />.
            /// </remarks>
            FILE_TRAVERSE = 0x20u,
            /// <summary>
            /// For a directory, the right to delete a directory and all
            /// the files it contains, including read-only files.
            /// </summary>
            FILE_DELETE_CHILD = 0x40u,
            /// <summary>
            /// The right to read attributes.
            /// </summary>
            FILE_READ_ATTRIBUTES = 0x80u,
            /// <summary>
            /// The right to write attributes.
            /// </summary>
            FILE_WRITE_ATTRIBUTES = 0x100u,
            /// <summary>
            /// All standard and specific rights. [FILE_ALL_ACCESS]
            /// </summary>
            FILE_ALL_ACCESS = 0xF01FFu,
            /// <summary>
            /// The right to delete the object.
            /// </summary>
            DELETE = 0x10000u,
            /// <summary>
            /// The right to read the information in the object's security descriptor.
            /// Doesn't include system access control list info (SACL).
            /// </summary>
            READ_CONTROL = 0x20000u,
            /// <summary>
            /// The right to modify the discretionary access control list (DACL) in the
            /// object's security descriptor.
            /// </summary>
            WRITE_DAC = 0x40000u,
            /// <summary>
            /// The right to change the owner in the object's security descriptor.
            /// </summary>
            WRITE_OWNER = 0x80000u,
            /// <summary>
            /// The right to use the object for synchronization. Enables a thread to wait until the object
            /// is in the signaled state. This is required if opening a synchronous handle.
            /// </summary>
            SYNCHRONIZE = 0x100000u,
            /// <summary>
            /// Same as READ_CONTROL.
            /// </summary>
            STANDARD_RIGHTS_READ = 0x20000u,
            /// <summary>
            /// Same as READ_CONTROL.
            /// </summary>
            STANDARD_RIGHTS_WRITE = 0x20000u,
            /// <summary>
            /// Same as READ_CONTROL.
            /// </summary>
            STANDARD_RIGHTS_EXECUTE = 0x20000u,
            /// <summary>
            /// Maps internally to <see cref="F:Interop.NtDll.DesiredAccess.FILE_READ_ATTRIBUTES" /> | <see cref="F:Interop.NtDll.DesiredAccess.FILE_READ_DATA" /> | <see cref="F:Interop.NtDll.DesiredAccess.FILE_READ_EA" />
            /// | <see cref="F:Interop.NtDll.DesiredAccess.STANDARD_RIGHTS_READ" /> | <see cref="F:Interop.NtDll.DesiredAccess.SYNCHRONIZE" />.
            /// (For directories, <see cref="F:Interop.NtDll.DesiredAccess.FILE_READ_ATTRIBUTES" /> | <see cref="F:Interop.NtDll.DesiredAccess.FILE_LIST_DIRECTORY" /> | <see cref="F:Interop.NtDll.DesiredAccess.FILE_READ_EA" />
            /// | <see cref="F:Interop.NtDll.DesiredAccess.STANDARD_RIGHTS_READ" /> | <see cref="F:Interop.NtDll.DesiredAccess.SYNCHRONIZE" />.)
            /// </summary>
            FILE_GENERIC_READ = 0x80000000u,
            /// <summary>
            /// Maps internally to <see cref="F:Interop.NtDll.DesiredAccess.FILE_APPEND_DATA" /> | <see cref="F:Interop.NtDll.DesiredAccess.FILE_WRITE_ATTRIBUTES" /> | <see cref="F:Interop.NtDll.DesiredAccess.FILE_WRITE_DATA" />
            /// | <see cref="F:Interop.NtDll.DesiredAccess.FILE_WRITE_EA" /> | <see cref="F:Interop.NtDll.DesiredAccess.STANDARD_RIGHTS_READ" /> | <see cref="F:Interop.NtDll.DesiredAccess.SYNCHRONIZE" />.
            /// (For directories, <see cref="F:Interop.NtDll.DesiredAccess.FILE_ADD_SUBDIRECTORY" /> | <see cref="F:Interop.NtDll.DesiredAccess.FILE_WRITE_ATTRIBUTES" /> | <see cref="F:Interop.NtDll.DesiredAccess.FILE_ADD_FILE" /> AddFile
            /// | <see cref="F:Interop.NtDll.DesiredAccess.FILE_WRITE_EA" /> | <see cref="F:Interop.NtDll.DesiredAccess.STANDARD_RIGHTS_READ" /> | <see cref="F:Interop.NtDll.DesiredAccess.SYNCHRONIZE" />.)
            /// </summary>
            FILE_GENERIC_WRITE = 0x40000000u,
            /// <summary>
            /// Maps internally to <see cref="F:Interop.NtDll.DesiredAccess.FILE_EXECUTE" /> | <see cref="F:Interop.NtDll.DesiredAccess.FILE_READ_ATTRIBUTES" /> | <see cref="F:Interop.NtDll.DesiredAccess.STANDARD_RIGHTS_EXECUTE" />
            /// | <see cref="F:Interop.NtDll.DesiredAccess.SYNCHRONIZE" />.
            /// (For directories, <see cref="F:Interop.NtDll.DesiredAccess.FILE_DELETE_CHILD" /> | <see cref="F:Interop.NtDll.DesiredAccess.FILE_READ_ATTRIBUTES" /> | <see cref="F:Interop.NtDll.DesiredAccess.STANDARD_RIGHTS_EXECUTE" />
            /// | <see cref="F:Interop.NtDll.DesiredAccess.SYNCHRONIZE" />.)
            /// </summary>
            FILE_GENERIC_EXECUTE = 0x20000000u
        }

        [DllImport(Libraries.NtDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        private unsafe static extern uint NtCreateFile(IntPtr* FileHandle, DesiredAccess DesiredAccess, OBJECT_ATTRIBUTES* ObjectAttributes, IO_STATUS_BLOCK* IoStatusBlock, long* AllocationSize, FileAttributes FileAttributes, FileShare ShareAccess, CreateDisposition CreateDisposition, CreateOptions CreateOptions, void* EaBuffer, uint EaLength);

        internal unsafe static (uint status, IntPtr handle) CreateFile(System.ReadOnlySpan<char> path, IntPtr rootDirectory, CreateDisposition createDisposition, DesiredAccess desiredAccess = DesiredAccess.SYNCHRONIZE | DesiredAccess.FILE_GENERIC_READ, FileShare shareAccess = FileShare.ReadWrite | FileShare.Delete, FileAttributes fileAttributes = (FileAttributes)0, CreateOptions createOptions = CreateOptions.FILE_SYNCHRONOUS_IO_NONALERT, ObjectAttributes objectAttributes = ObjectAttributes.OBJ_CASE_INSENSITIVE, void* eaBuffer = null, uint eaLength = 0u, long* preallocationSize = null, SECURITY_QUALITY_OF_SERVICE* securityQualityOfService = null)
        {
            checked
            {
                fixed (char* ptr = &MemoryMarshal.GetReference<char>(path))
                {
                    UNICODE_STRING uNICODE_STRING = default(UNICODE_STRING);
                    uNICODE_STRING.Length = (ushort)(path.Length * 2);
                    uNICODE_STRING.MaximumLength = (ushort)(path.Length * 2);
                    uNICODE_STRING.Buffer = (IntPtr)ptr;
                    UNICODE_STRING uNICODE_STRING2 = uNICODE_STRING;
                    OBJECT_ATTRIBUTES oBJECT_ATTRIBUTES = new OBJECT_ATTRIBUTES(&uNICODE_STRING2, objectAttributes, rootDirectory, securityQualityOfService);
                    IntPtr item = default(IntPtr);
                    IO_STATUS_BLOCK iO_STATUS_BLOCK = default(IO_STATUS_BLOCK);
                    uint item2 = NtCreateFile(&item, desiredAccess, &oBJECT_ATTRIBUTES, &iO_STATUS_BLOCK, preallocationSize, fileAttributes, shareAccess, createDisposition, createOptions, eaBuffer, eaLength);
                    return (item2, item);
                }
            }
        }

        internal unsafe static (uint status, IntPtr handle) NtCreateFile(System.ReadOnlySpan<char> path, FileMode mode, FileAccess access, FileShare share, FileOptions options, long preallocationSize)
        {
            SECURITY_QUALITY_OF_SERVICE sECURITY_QUALITY_OF_SERVICE = new SECURITY_QUALITY_OF_SERVICE(ImpersonationLevel.Anonymous, ContextTrackingMode.Static, effectiveOnly: false);
            IntPtr zero = IntPtr.Zero;
            CreateDisposition createDisposition = GetCreateDisposition(mode);
            DesiredAccess desiredAccess = GetDesiredAccess(access, mode, options);
            FileShare shareAccess = GetShareAccess(share);
            FileAttributes fileAttributes = GetFileAttributes(options);
            CreateOptions createOptions = GetCreateOptions(options);
            ObjectAttributes objectAttributes = GetObjectAttributes(share);
            long* preallocationSize2 = &preallocationSize;
            SECURITY_QUALITY_OF_SERVICE* securityQualityOfService = &sECURITY_QUALITY_OF_SERVICE;
            return CreateFile(path, zero, createDisposition, desiredAccess, shareAccess, fileAttributes, createOptions, objectAttributes, null, 0u, preallocationSize2, securityQualityOfService);
        }

        private static CreateDisposition GetCreateDisposition(FileMode mode)
        {
            switch (mode)
            {
                case FileMode.CreateNew:
                    return CreateDisposition.FILE_CREATE;
                case FileMode.Create:
                    return CreateDisposition.FILE_SUPERSEDE;
                case FileMode.OpenOrCreate:
                case FileMode.Append:
                    return CreateDisposition.FILE_OPEN_IF;
                case FileMode.Truncate:
                    return CreateDisposition.FILE_OVERWRITE;
                default:
                    return CreateDisposition.FILE_OPEN;
            }
        }

        private static DesiredAccess GetDesiredAccess(FileAccess access, FileMode fileMode, FileOptions options)
        {
            DesiredAccess desiredAccess = DesiredAccess.FILE_READ_ATTRIBUTES | DesiredAccess.SYNCHRONIZE;
            if ((access & FileAccess.Read) != 0)
            {
                desiredAccess |= DesiredAccess.FILE_GENERIC_READ;
            }
            if ((access & FileAccess.Write) != 0)
            {
                desiredAccess |= DesiredAccess.FILE_GENERIC_WRITE;
            }
            if (fileMode == FileMode.Append)
            {
                desiredAccess |= DesiredAccess.FILE_APPEND_DATA;
            }
            if ((options & FileOptions.DeleteOnClose) != 0)
            {
                desiredAccess |= DesiredAccess.DELETE;
            }
            return desiredAccess;
        }

        private static FileShare GetShareAccess(FileShare share)
        {
            return share & ~FileShare.Inheritable;
        }

        private static FileAttributes GetFileAttributes(FileOptions options)
        {
            if ((options & FileOptions.Encrypted) == 0)
            {
                return (FileAttributes)0;
            }
            return FileAttributes.Encrypted;
        }

        private static CreateOptions GetCreateOptions(FileOptions options)
        {
            CreateOptions createOptions = CreateOptions.FILE_NON_DIRECTORY_FILE;
            if (((uint)options & 0x80000000u) != 0)
            {
                createOptions |= CreateOptions.FILE_WRITE_THROUGH;
            }
            if ((options & FileOptions.RandomAccess) != 0)
            {
                createOptions |= CreateOptions.FILE_RANDOM_ACCESS;
            }
            if ((options & FileOptions.SequentialScan) != 0)
            {
                createOptions |= CreateOptions.FILE_SEQUENTIAL_ONLY;
            }
            if ((options & FileOptions.DeleteOnClose) != 0)
            {
                createOptions |= CreateOptions.FILE_DELETE_ON_CLOSE;
            }
            if ((options & FileOptions.Asynchronous) == 0)
            {
                createOptions |= CreateOptions.FILE_SYNCHRONOUS_IO_NONALERT;
            }
            if ((options & (FileOptions)536870912) != 0)
            {
                createOptions |= CreateOptions.FILE_NO_INTERMEDIATE_BUFFERING;
            }
            return createOptions;
        }

        private static ObjectAttributes GetObjectAttributes(FileShare share)
        {
            return ObjectAttributes.OBJ_CASE_INSENSITIVE | (((share & FileShare.Inheritable) != 0) ? ObjectAttributes.OBJ_INHERIT : ((ObjectAttributes)0u));
        }

        [DllImport(Libraries.NtDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public unsafe static extern int NtQueryDirectoryFile(IntPtr FileHandle, IntPtr Event, IntPtr ApcRoutine, IntPtr ApcContext, IO_STATUS_BLOCK* IoStatusBlock, IntPtr FileInformation, uint Length, FILE_INFORMATION_CLASS FileInformationClass, BOOLEAN ReturnSingleEntry, UNICODE_STRING* FileName, BOOLEAN RestartScan);

        [DllImport(Libraries.NtDll, ExactSpelling = true)]
        public static extern uint RtlNtStatusToDosError(int Status);
    }

    internal static class StatusOptions
    {
        internal const uint STATUS_SUCCESS = 0u;

        internal const uint STATUS_SOME_NOT_MAPPED = 263u;

        internal const uint STATUS_NO_MORE_FILES = 2147483654u;

        internal const uint STATUS_INVALID_PARAMETER = 3221225485u;

        internal const uint STATUS_FILE_NOT_FOUND = 3221225487u;

        internal const uint STATUS_NO_MEMORY = 3221225495u;

        internal const uint STATUS_ACCESS_DENIED = 3221225506u;

        internal const uint STATUS_OBJECT_NAME_NOT_FOUND = 3221225524u;

        internal const uint STATUS_ACCOUNT_RESTRICTION = 3221225582u;

        internal const uint STATUS_NONE_MAPPED = 3221225587u;

        internal const uint STATUS_INSUFFICIENT_RESOURCES = 3221225626u;

        internal const uint STATUS_DISK_FULL = 3221225599u;

        internal const uint STATUS_FILE_TOO_LARGE = 3221227780u;
    }

    internal unsafe static void GetRandomBytes(byte* buffer, int length)
    {
        switch (BCrypt.BCryptGenRandom(IntPtr.Zero, buffer, length, 2))
        {
            case BCrypt.NTSTATUS.STATUS_NO_MEMORY:
                throw new OutOfMemoryException();
            default:
                throw new InvalidOperationException();
            case BCrypt.NTSTATUS.STATUS_SUCCESS:
                break;
        }
    }
}

