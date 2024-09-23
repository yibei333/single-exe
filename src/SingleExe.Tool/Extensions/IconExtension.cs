using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SingleExe.Tool.Extensions;

public static class IconExtension
{
    public static void SaveIcon(string sourceFile, string iconPath)
    {
        nint? module = null;

        try
        {
            module = Kernel32.LoadLibraryEx(sourceFile, IntPtr.Zero, LOAD_LIBRARY.AS_DATAFILE);

            bool resDelegate(IntPtr _hModule, RT type, IntPtr lpszName, IntPtr lParam)
            {
                var iconResInfos = GetIconResourceInfo(_hModule, lpszName);
                using var stream = new FileStream(iconPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                WriteIconData(module.Value, iconResInfos, stream);
                stream.Flush();
                return false;
            }

            Kernel32.EnumResourceNames(module.Value, RT.GROUP_ICON, resDelegate, IntPtr.Zero);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.ToString());
        }

        if (module is not null)
        {
            Kernel32.FreeLibrary(module.Value);
        }
    }

    #region private
    static ICONRESINF[] GetIconResourceInfo(IntPtr module, IntPtr lpszName)
    {
        var hResInf = Kernel32.FindResource(module, lpszName, RT.GROUP_ICON);
        var hResource = Kernel32.LoadResource(module, hResInf);
        var ptrResource = Kernel32.LockResource(hResource);

        var iconResHead = (ICONRESHEAD)Marshal.PtrToStructure(ptrResource, typeof(ICONRESHEAD));

        var iconResInfos = Enumerable
            .Range(0, iconResHead.Count)
            .Select(i => (ICONRESINF)Marshal.PtrToStructure(ptrResource + Marshal.SizeOf(typeof(ICONRESHEAD)) + Marshal.SizeOf(typeof(ICONRESINF)) * i, typeof(ICONRESINF)))
            .ToArray();

        return iconResInfos;
    }

    static void WriteIconData(IntPtr hModule, ICONRESINF[] iconResInfos, Stream stream)
    {
        var address = Marshal.SizeOf(typeof(ICONFILEHEAD)) + Marshal.SizeOf(typeof(ICONFILEINF)) * iconResInfos.Length;

        var iconFiles = iconResInfos
            .Select(iconResInf =>
            {
                var iconBytes = GetResourceBytes(hModule, iconResInf.ID, RT.ICON);
                var iconFileInf = new ICONFILEINF
                {
                    Cx = iconResInf.Cx,
                    Cy = iconResInf.Cy,
                    ColorCount = iconResInf.ColorCount,
                    Planes = iconResInf.Planes,
                    BitCount = iconResInf.BitCount,
                    Size = iconResInf.Size,
                    Address = (uint)address
                };
                address += iconBytes.Length;
                return new { iconBytes, iconFileInf };
            }).ToList();

        var iconFileHead = new ICONFILEHEAD
        {
            Type = 1,
            Count = (ushort)iconResInfos.Length
        };
        var iconFileHeadBytes = StructureToBytes(iconFileHead);
        stream.Write(iconFileHeadBytes, 0, iconFileHeadBytes.Length);
        iconFiles.ForEach(iconFile =>
        {
            var bytes = StructureToBytes(iconFile.iconFileInf);
            stream.Write(bytes, 0, bytes.Length);
        });

        iconFiles.ForEach(iconFile =>
        {
            stream.Write(iconFile.iconBytes, 0, iconFile.iconBytes.Length);
        });
    }

    static byte[] GetResourceBytes(IntPtr hModule, IntPtr lpszName, RT type)
    {
        var hResInf = Kernel32.FindResource(hModule, lpszName, type);
        var hResource = Kernel32.LoadResource(hModule, hResInf);
        var ptrResource = Kernel32.LockResource(hResource);

        var size = Kernel32.SizeofResource(hModule, hResInf);
        var buff = new byte[size];
        Marshal.Copy(ptrResource, buff, 0, buff.Length);
        return buff;
    }

    static byte[] StructureToBytes(object obj)
    {
        var size = Marshal.SizeOf(obj);
        var bytes = new byte[size];
        var gch = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        Marshal.StructureToPtr(obj, gch.AddrOfPinnedObject(), false);
        gch.Free();
        return bytes;
    }

    [Flags]
    enum LOAD_LIBRARY : uint
    {
        DONT_RESOLVE_DLL_REFERENCES = 0x00000001,
        IGNORE_CODE_AUTHZ_LEVEL = 0x00000010,
        AS_DATAFILE = 0x00000002,
        AS_DATAFILE_EXCLUSIVE = 0x00000040,
        AS_IMAGE_RESOURCE = 0x00000020,
        WITH_ALTERED_SEARCH_PATH = 0x00000008
    }

    enum RT
    {
        CURSOR = 1,
        BITMAP = 2,
        ICON = 3,
        MENU = 4,
        DIALOG = 5,
        STRING = 6,
        FONTDIR = 7,
        FONT = 8,
        ACCELERATOR = 9,
        RCDATA = 10,
        MESSAGETABLE = 11,
        GROUP_CURSOR = 12,
        GROUP_ICON = 14,
        VERSION = 16,
        DLGINCLUDE = 17,
        PLUGPLAY = 19,
        VXD = 20,
        ANICURSOR = 21,
        ANIICON = 22,
        HTML = 23,
        MANIFEST = 24
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [DebuggerDisplay("{Cx} x {Cy}, {BitCount}bit, {Size}bytes")]
    struct ICONRESINF
    {
        public byte Cx;
        public byte Cy;
        public byte ColorCount;
        public byte Reserved;
        public ushort Planes;
        public ushort BitCount;
        public uint Size;
        public ushort ID;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct ICONRESHEAD
    {
        public ushort Reserved;
        public ushort Type;
        public ushort Count;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [DebuggerDisplay("{Cx} x {Cy}, {BitCount}bit, {Size}bytes")]
    struct ICONFILEINF
    {
        public byte Cx;
        public byte Cy;
        public byte ColorCount;
        public byte Reserved;
        public ushort Planes;
        public ushort BitCount;
        public uint Size;
        public uint Address;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct ICONFILEHEAD
    {
        public ushort Reserved;
        public ushort Type;
        public ushort Count;
    };

    delegate bool EnumResNameProcDelegate(IntPtr hModule, RT lpszType, IntPtr lpszName, IntPtr lParam);

    class Kernel32
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hReservedNull, LOAD_LIBRARY dwFlags);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool EnumResourceNames(IntPtr hModule, RT type, EnumResNameProcDelegate lpEnumFunc, IntPtr lParam);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr FindResource(IntPtr hModule, IntPtr lpszName, RT type);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr LoadResource(IntPtr hModule, IntPtr hResInfo);

        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern IntPtr LockResource(IntPtr hResource);

        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern uint SizeofResource(IntPtr hModule, IntPtr hResInfo);
    }
    #endregion
}
