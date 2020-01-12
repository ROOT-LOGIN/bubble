/// <summary>
/// Win32 platform dialogs
/// </summary>
public static class Win32Dialog
{
    [DllImport("Shell32.dll", CharSet = CharSet.Unicode)]
    static extern IntPtr SHBrowseForFolder(ref LPBROWSEINFO lpbi);

    [DllImport("Shell32.dll", CharSet = CharSet.Unicode)]
    static extern bool SHGetPathFromIDList(IntPtr pidl, [MarshalAs(UnmanagedType.LPWStr, SizeConst = 1024)]StringBuilder pszPath);

    delegate int HOOKPROC(IntPtr hwnd, uint uMsg, IntPtr lParam, IntPtr lpData);

    struct LPBROWSEINFO
    {
        public IntPtr hwndOwner;
        public IntPtr pidlRoot;
        [MarshalAs(UnmanagedType.LPWStr)] public string pszDisplayName;
        [MarshalAs(UnmanagedType.LPWStr)] public string lpszTitle;
        public uint ulFlags;
        public HOOKPROC lpfn;
        public IntPtr lParam;
        public int iImage;

    }

    [DllImport("Comdlg32.dll", CharSet = CharSet.Unicode)]
    static extern bool GetOpenFileNameW(ref LPOPENFILENAME lpofn);

    struct LPOPENFILENAME
    {
        public int lStructSize;
        public IntPtr hwndOwner;
        public IntPtr hInstance;
        [MarshalAs(UnmanagedType.LPWStr)] public string lpstrFilter;
        [MarshalAs(UnmanagedType.LPWStr)] public string lpstrCustomFilter;
        public uint nMaxCustFilter;
        public uint nFilterIndex;
        public IntPtr lpstrFile;
        public uint nMaxFile;
        [MarshalAs(UnmanagedType.LPWStr)] public string lpstrFileTitle;
        public uint nMaxFileTitle;
        public string lpstrInitialDir;
        [MarshalAs(UnmanagedType.LPWStr)] public string lpstrTitle;
        public int Flags;
        public ushort nFileOffset;
        public ushort nFileExtension;
        public string lpstrDefExt;
        public IntPtr lCustData;
        public HOOKPROC lpfnHook;
        public string lpTemplateName;
        public IntPtr pvReserved;
        public uint dwReserved;
        public uint FlagsEx;
    };

    static string[] ALL_FILES = new string[] { "All Files", "*.*" };

    public enum DlgStyle
    {
        Win95 = 0,
        Xp = 0x00000040 | 0x00080000,
        Vista = 0x00080000
    }

    /// <summary>
    /// Select file.
    /// </summary>
    /// <param name="owner">The owner.</param>
    /// <param name="multiSelect">can select multiple file</param>
    /// <param name="filterList">Filter list£¬odd is display name, even is comma seperated fileter list.</param>
    /// <param name="fileName">Default file name.</param>
    /// <param name="style">The style.</param>
    /// <returns></returns>
    public static string[] OpenFile(Window owner, bool multiSelect, string[] filterList, string fileName, DlgStyle style)
    {
        var info = new LPOPENFILENAME();
        if (owner == null) owner = ZHApp.FrameworkMainWindow;
        if (owner != null)
        {
            var wih = new WindowInteropHelper(owner);
            info.hwndOwner = wih.Handle;
        }
        if (filterList != null)
        {
            int r;
            Math.DivRem(filterList.Length, 2, out r);
            if (r != 0 || filterList.Length == 0)
            {
                filterList = ALL_FILES;
            }
        }
        if (filterList == null)
        {
            filterList = ALL_FILES;
        }
        info.lpstrFilter = string.Join("\0", filterList) + "\0\0";
        info.nFilterIndex = 1;
        info.lpstrFile = Marshal.StringToHGlobalUni((fileName ?? string.Empty).PadRight(1024, (char)0));
        info.nMaxFile = 1024;
        info.Flags = (multiSelect ? 0x200 : 0) | 0x00200000 | 0x00020000 | 0x00000800 | 0x00800000 | (int)style;
        info.lStructSize = Marshal.SizeOf(typeof(LPOPENFILENAME));

        try
        {
            if (GetOpenFileNameW(ref info))
            {
                if (style == DlgStyle.Win95)
                {
                    var bytes = new List<byte>(2000);
                    for (int i = 0; i < 1024 * 2; i += 2)
                    {
                        var l = Marshal.ReadByte(info.lpstrFile, i);
                        var h = Marshal.ReadByte(info.lpstrFile, i + 1);
                        if (l == 0 && h == 0) break;
                        bytes.Add(l);
                        bytes.Add(h);
                    }
                    var lpstrFile = Encoding.Unicode.GetString(bytes.ToArray());

                    if (multiSelect)
                    {
                        var ary = lpstrFile.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (ary.Length == 1) return ary;

                        var ret = new string[ary.Length - 1];
                        for (int i = 1; i < ary.Length; i++)
                        {
                            ret[i - 1] = System.IO.Path.Combine(ary[0], ary[i]);
                        }
                        return ret;
                    }
                    else
                    {
                        return new string[1] { lpstrFile };
                    }
                }
                else
                {
                    if (multiSelect)
                    {
                        var bytes = new List<byte>(2000);
                        int pl = -1, ph = -1;
                        for (int i = 0; i < 1024 * 2; i += 2)
                        {
                            var l = Marshal.ReadByte(info.lpstrFile, i);
                            var h = Marshal.ReadByte(info.lpstrFile, i + 1);
                            if (l == 0 && h == 0 && pl == 0 && ph == 0) break;
                            pl = l; ph = h;
                            bytes.Add(l);
                            bytes.Add(h);
                        }
                        var lpstrFile = Encoding.Unicode.GetString(bytes.ToArray());

                        var ary = lpstrFile.Split(new[] { (char)0 }, StringSplitOptions.RemoveEmptyEntries);
                        if (ary.Length == 1) return ary;

                        var ret = new string[ary.Length - 1];
                        for (int i = 1; i < ary.Length; i++)
                        {
                            ret[i - 1] = System.IO.Path.Combine(ary[0], ary[i]);
                        }
                        return ret;
                    }
                    else
                    {
                        var bytes = new List<byte>(2000);
                        for (int i = 0; i < 1024 * 2; i += 2)
                        {
                            var l = Marshal.ReadByte(info.lpstrFile, i);
                            var h = Marshal.ReadByte(info.lpstrFile, i + 1);
                            if (l == 0 && h == 0) break;
                            bytes.Add(l);
                            bytes.Add(h);
                        }
                        var lpstrFile = Encoding.Unicode.GetString(bytes.ToArray());
                        return new string[1] { lpstrFile };
                    }
                }
            }

            Marshal.FreeHGlobal(info.lpstrFile);
            return new string[0];
        }
        catch
        {
            Marshal.FreeHGlobal(info.lpstrFile);
            return new string[0];
        }
    }

    /// <summary>
    /// Select folder.
    /// </summary>
    /// <param name="owner">The owner.</param>
    /// <param name="remarkText">The remark text.</param>
    /// <returns></returns>
    public static string BrowserFolder(Window owner, string remarkText = "Select a folder")
    {
        var info = new LPBROWSEINFO();
        if (owner == null) owner = ZHApp.FrameworkMainWindow;
        if (owner != null)
        {
            var wih = new WindowInteropHelper(owner);
            info.hwndOwner = wih.Handle;
        }
        info.lpszTitle = remarkText;
        info.pszDisplayName = string.Join(string.Empty, Enumerable.Range(0, 300).Select(c => (char)0));
        info.ulFlags = 0x1 | 0x2 | 0x40;
        try
        {
            IntPtr pidl = SHBrowseForFolder(ref info);
            if (pidl == IntPtr.Zero) return null;
            StringBuilder buf = new StringBuilder(1024);
            SHGetPathFromIDList(pidl, buf);
            return buf.ToString();
        }
        catch
        {
            return null;
        }
    }
}
