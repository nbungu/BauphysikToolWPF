using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using BT.Logging;

namespace BauphysikToolWPF.Services.Application
{
    public static class PathService
    {

        public static readonly string DownloadsFolderPath = GetDownloadsFolderPath();
        public static readonly string LocalAppDataPath = GetLocalAppDataPath();
        public static readonly string LocalProgramDataPath = GetLocalProgramDataPath();
        
        #region Paths

        /// <summary>
        /// Path to: %appdata%
        /// </summary>
        /// <returns></returns>
        private static string GetLocalProgramDataPath()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        }

        /// <summary>
        /// Path to: %appdata%/BauphysikTool
        /// </summary>
        /// <returns></returns>
        private static string GetLocalAppDataPath()
        {
            string programDataPath = GetLocalProgramDataPath();
            string appFolder = Path.Combine(programDataPath, "BauphysikTool");
            return appFolder;
        }

        /// <summary>
        /// Retrieves the path to the user's Downloads folder.
        /// </summary>
        /// <returns>
        /// The full path to the Downloads folder. If the folder cannot be retrieved using the Windows API, 
        /// the method falls back to the default path: UserProfile + "Downloads".
        /// </returns>
        /// <remarks>
        /// This method first attempts to retrieve the Downloads folder using the Windows Shell API function
        /// SHGetKnownFolderPath, which accounts for custom folder locations. If this fails (e.g., due to
        /// relocation or API errors), it falls back to constructing the path manually based on the user's
        /// profile directory.
        /// </remarks>
        /// <exception cref="ExternalException">
        /// Thrown if the Windows API fails to retrieve the Downloads folder path.
        /// </exception>
        private static string GetDownloadsFolderPath()
        {
            try
            {
                // Attempt to retrieve the Downloads folder path
                Guid downloadsFolderGuid = new Guid("374DE290-123F-4565-9164-39C4925E467B");
                IntPtr outPath;

                int result = SHGetKnownFolderPath(downloadsFolderGuid, 0, IntPtr.Zero, out outPath);

                if (result >= 0) // Success
                {
                    string path = Marshal.PtrToStringUni(outPath) ?? String.Empty;
                    Marshal.FreeCoTaskMem(outPath);
                    return path;
                }
                throw new ExternalException("Failed to retrieve Downloads folder path.", result);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error retrieving Downloads folder: {ex.Message}");
                // Fallback to UserProfile + "Downloads"
                string fallbackPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

                Logger.LogInfo($"Falling back to default path: {fallbackPath}");
                return fallbackPath;
            }
        }

        
        
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern int SHGetKnownFolderPath(
            [MarshalAs(UnmanagedType.LPStruct)] Guid rfid,
            uint dwFlags,
            IntPtr hToken,
            out IntPtr pszPath);

        public static bool IsUncPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;
            return path.StartsWith(@"\\") || path.StartsWith("//");
        }

        public static bool IsPathUnderRoot(string fullPath, string root)
        {
            try
            {
                var normalizedRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
                var normalizedPath = Path.GetFullPath(fullPath);
                return normalizedPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        public static bool IsAllowedExtension(string path, string[] allowedExtensions)
        {
            string ext = Path.GetExtension(path);
            return allowedExtensions.Any(a => string.Equals(a, ext, StringComparison.OrdinalIgnoreCase));
        }

        public static bool IsReasonableFileSize(string path, long maxBytes)
        {
            try
            {
                var fi = new FileInfo(path);
                return fi.Length > 0 && fi.Length <= maxBytes;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Mask path for logs (keep filename but drop parent directories). Avoid printing full user/system paths to logs.
        /// </summary>
        public static string MaskPath(string fullPath)
        {
            try
            {
                string fileName = Path.GetFileName(fullPath);
                return $"...{Path.DirectorySeparatorChar}{fileName}";
            }
            catch
            {
                return "<invalid-path>";
            }
        }

        /// <summary>
        /// Generic small sanitizer for small user-visible strings (example).
        /// </summary>
        public static string MaskForLog(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            // Truncate long names to avoid leaking too much info
            return value.Length <= 60 ? value : value.Substring(0, 57) + "...";
        }

        #endregion
    }
}
