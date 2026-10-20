using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AndDesk.Services;

public class ShellService : IShellService
{
    private readonly ConcurrentDictionary<string, ImageSource> _iconCache = new(StringComparer.OrdinalIgnoreCase);

    public (bool Success, string? ErrorMessage) OpenItem(string path)
    {
        try
        {
            if (!File.Exists(path) && !Directory.Exists(path))
            {
                return (false, "目标文件或文件夹不存在，可能已被移动或删除。");
            }

            var psi = new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true,
                WorkingDirectory = Directory.Exists(path) ? path : (Path.GetDirectoryName(path) ?? string.Empty)
            };

            Process.Start(psi);
            return (true, null);
        }
        catch (System.ComponentModel.Win32Exception winEx)
        {
            return (false, $"无法打开该项目：{winEx.Message} (Win32错误码: {winEx.NativeErrorCode})");
        }
        catch (Exception ex)
        {
            return (false, $"打开失败：{ex.Message}");
        }
    }

    public string GetFriendlyTypeName(string path)
    {
        try
        {
            var shinfo = new SHFILEINFO();
            uint flags = SHGFI_TYPENAME | SHGFI_USEFILEATTRIBUTES;
            uint attr = File.GetAttributes(path).HasFlag(FileAttributes.Directory) ? FILE_ATTRIBUTE_DIRECTORY : FILE_ATTRIBUTE_NORMAL;

            IntPtr res = SHGetFileInfo(path, attr, ref shinfo, (uint)Marshal.SizeOf(shinfo), flags);
            if (res != IntPtr.Zero && !string.IsNullOrWhiteSpace(shinfo.szTypeName))
            {
                return shinfo.szTypeName;
            }
        }
        catch
        {
            // Fall back to extension
        }

        string ext = Path.GetExtension(path);
        return string.IsNullOrEmpty(ext) ? "文件" : $"{ext.TrimStart('.').ToUpperInvariant()} 文件";
    }

    public ImageSource? GetIcon(string path, bool isDirectory)
    {
        // For cache key: files sharing extension (unless .exe or .lnk or .url) can share icons
        string ext = Path.GetExtension(path).ToLowerInvariant();
        string cacheKey = isDirectory
            ? "__folder__"
            : (ext is ".exe" or ".lnk" or ".url" or ".ico" ? path : ext);

        if (_iconCache.TryGetValue(cacheKey, out var cachedIcon))
        {
            return cachedIcon;
        }

        try
        {
            var shinfo = new SHFILEINFO();
            uint flags = SHGFI_ICON | SHGFI_LARGEICON;

            // If it's a generic file extension and not a link/exe, we can use attribute to avoid reading disk
            if (!isDirectory && ext is not ".exe" and not ".lnk" and not ".url" && !string.IsNullOrEmpty(ext))
            {
                flags |= SHGFI_USEFILEATTRIBUTES;
            }

            uint attr = isDirectory ? FILE_ATTRIBUTE_DIRECTORY : FILE_ATTRIBUTE_NORMAL;
            IntPtr hImg = SHGetFileInfo(path, attr, ref shinfo, (uint)Marshal.SizeOf(shinfo), flags);

            if (hImg != IntPtr.Zero && shinfo.hIcon != IntPtr.Zero)
            {
                try
                {
                    BitmapSource bs = Imaging.CreateBitmapSourceFromHIcon(
                        shinfo.hIcon,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions()
                    );

                    bs.Freeze(); // Make it thread-safe and reusable
                    _iconCache[cacheKey] = bs;
                    return bs;
                }
                finally
                {
                    DestroyIcon(shinfo.hIcon);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to get icon for {path}: {ex.Message}");
        }

        return null;
    }

    #region Win32 P/Invoke

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct SHFILEINFO
    {
        public IntPtr hIcon;
        public int iIcon;
        public uint dwAttributes;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szDisplayName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string szTypeName;
    }

    private const uint SHGFI_ICON = 0x000000100;
    private const uint SHGFI_LARGEICON = 0x000000000;
    private const uint SHGFI_TYPENAME = 0x000000400;
    private const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;

    private const uint FILE_ATTRIBUTE_NORMAL = 0x00000080;
    private const uint FILE_ATTRIBUTE_DIRECTORY = 0x00000010;

    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SHGetFileInfo(
        string pszPath,
        uint dwFileAttributes,
        ref SHFILEINFO psfi,
        uint cbFileInfo,
        uint uFlags
    );

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyIcon(IntPtr hIcon);

    #endregion
}
