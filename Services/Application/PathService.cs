using BT.Logging;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace BauphysikToolWPF.Services.Application
{
    public static class PathService
    {
        #region Path Repo
        
        public static readonly string UserDownloadsFolderPath = GetDownloadsFolderPath();

        /// <summary>
        /// Directory Path: %appdata%
        /// </summary>
        public static readonly string UserApplicationDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        /// <summary>
        /// Directory Path: %appdata%/BauphysikTool
        /// </summary>
        public static readonly string UserProgramDataPath = Path.Combine(UserApplicationDataPath, "BauphysikTool");

        /// <summary>
        /// Directory Path: C:/ProgramData
        /// </summary>
        public static readonly string CommonApplicationDataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);

        /// <summary>
        /// Initial file which is extracted to the installation/build directory. Serves as template and backup.
        /// File path: Build Directory/Repositories/recent_projects.json
        /// </summary>
        public static readonly string BuildDirRecentProjectsFilePath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".\\Repositories\\recent_projects.json"));

        /// <summary>
        /// User-Copy of the initial file which is stored in a user-writable location.
        /// File path: %appdata%/BauphysikTool/recent_projects.json
        /// </summary>
        public static readonly string UserRecentProjectsFilePath = Path.Combine(UserProgramDataPath, "recent_projects.json");

        /// <summary>
        /// Initial file which is extracted to the installation/build directory. Serves as template and backup.
        /// File path: Build Directory/Repositories/recent_projects.json
        /// </summary>
        public static readonly string BuildDirDemoProjectFilePath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".\\Repositories\\Demoprojekt.btk"));

        /// <summary>
        /// User-Copy of the initial file which is stored in a user-writable location.
        /// File path: %appdata%/BauphysikTool/recent_projects.json
        /// </summary>
        public static readonly string UserDemoProjectFilePath = Path.Combine(UserProgramDataPath, "Demoprojekt.btk");

        /// <summary>
        /// Initial file which is extracted to the installation/build directory. Serves as template and backup.
        /// File path: Build Directory/Repositories/updater.json
        /// </summary>
        public static readonly string BuildDirUpdaterFilePath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".\\Repositories\\updater.json"));

        /// <summary>
        /// User-Copy of the initial file which is stored in a user-writable location.
        /// File path: %appdata%/BauphysikTool/updater.json
        /// </summary>
        public static readonly string UserUpdaterFilePath = Path.Combine(UserProgramDataPath, "updater.json");

        // string literal can be const
        public const string ServerUpdaterQuery = "https://bauphysik-tool.de/strapi/api/downloads?sort=publishedAt:desc&fields[0]=semanticVersion&fields[1]=versionTag";

        /// <summary>
        /// Initial file which is extracted to the installation/build directory. Serves as template and backup.
        /// File path: Build Directory/Repositories/InitialDB.db
        /// </summary>
        public static readonly string BuildDirDatabaseFilePath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".\\Repositories\\InitialDB.db"));

        public static readonly string RootProjectDatabaseFile = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\Repositories\\InitialDB.db"));

        /// <summary>
        /// User-Copy of the initial file which is stored in a user-writable location.
        /// File path: %appdata%/BauphysikTool/BauphysikToolDB.db
        /// </summary>
        public static readonly string UserDatabaseFilePath = Path.Combine(UserProgramDataPath, "BauphysikToolDB.db");

        /// <summary>
        /// Initial file which is extracted to the installation/build directory. Serves as template and backup.
        /// File path: Build Directory/Resources/Fonts/segoeUI.fnt
        /// </summary>
        public static readonly string BuildDirFontFilePath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".\\Resources\\Fonts\\segoeUI.fnt"));

        /// <summary>
        /// Initial file which is extracted to the installation/build directory. Serves as template and backup.
        /// File path: Build Directory/Services/UI/OpenGL/layer.vert
        /// </summary>
        public static readonly string BuildDirVertexShaderFilePath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".\\Services\\UI\\OpenGL\\layer.vert"));

        /// <summary>
        /// Initial file which is extracted to the installation/build directory. Serves as template and backup.
        /// File path: Build Directory/Services/UI/OpenGL/layer.vert
        /// </summary>
        public static readonly string BuildDirFragmentShaderFilePath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".\\Services\\UI\\OpenGL\\layer.frag"));

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

        #endregion

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern int SHGetKnownFolderPath(
            [MarshalAs(UnmanagedType.LPStruct)] Guid rfid,
            uint dwFlags,
            IntPtr hToken,
            out IntPtr pszPath);

        #region Path Checks

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
