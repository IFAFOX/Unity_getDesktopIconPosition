using System;
using System.Text;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Drawing;
/// <summary>
/// 获得桌面图标名称和位置
/// </summary>
public static class DesktopUtil
{
    #region Api声明
    private const uint LVM_FIRST = 0x1000;
    private const uint LVM_GETITEMCOUNT = LVM_FIRST + 4;
    private const uint LVM_GETITEMW = LVM_FIRST + 75;
    private const uint LVM_GETITEMPOSITION = LVM_FIRST + 16;
    [DllImport("user32.dll")]
    private static extern int SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);
    [DllImport("user32.dll")]
    private static extern IntPtr FindWindow(string lpszClass, string lpszWindow);
    [DllImport("user32.dll")]
    private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);
    
    [DllImport("user32.dll")]
    private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint dwProcessId);
    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);
    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
    [DllImport("user32.dll")]
    private static extern IntPtr GetShellWindow();
    private const uint PROCESS_VM_OPERATION = 0x0008;
    private const uint PROCESS_VM_READ = 0x0010;
    private const uint PROCESS_VM_WRITE = 0x0020;
    [DllImport("kernel32.dll")]
    private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);
    private const uint MEM_COMMIT = 0x1000;
    private const uint MEM_RELEASE = 0x8000;
    private const uint MEM_RESERVE = 0x2000;
    private const uint PAGE_READWRITE = 4;
    [DllImport("kernel32.dll")]
    private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);
    [DllImport("kernel32.dll")]
    private static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint dwFreeType);
    [DllImport("kernel32.dll")]
    private static extern bool CloseHandle(IntPtr handle);
    [DllImport("kernel32.dll")]
    private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, IntPtr lpBuffer, int nSize, ref uint vNumberOfBytesRead);
    [DllImport("kernel32.dll")]
    private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress,
       IntPtr lpBuffer, int nSize, ref uint vNumberOfBytesRead);
    private struct LVITEM  //结构体
    {
        public int mask;
        public int iItem;
        public int iSubItem;
        public int state;
        public int stateMask;
        public IntPtr pszText; // string
        public int cchTextMax;
        public int iImage;
        public IntPtr lParam;
        public int iIndent;
        public int iGroupId;
        public int cColumns;
        public IntPtr puColumns;
    }
    private const int LVIF_TEXT = 0x0001;
    /// <summary>
    /// 节点个数,通过SendMessage 发送获取
    /// </summary>
    /// <param name="AHandle"></param>
    /// <returns></returns>
    private static int ListView_GetItemCount(IntPtr AHandle)
    {
        return SendMessage(AHandle, LVM_GETITEMCOUNT, 0, 0);
    }
    /// <summary>
    /// 图标位置
    /// </summary>
    /// <param name="AHandle"></param>
    /// <param name="AIndex"></param>
    /// <param name="APoint"></param>
    /// <returns></returns>
    private static bool ListView_GetItemPosition(IntPtr AHandle, int AIndex, IntPtr APoint)
    {
        return SendMessage(AHandle, LVM_GETITEMPOSITION, AIndex, APoint.ToInt32()) != 0;
    }
    #endregion
    public static int GetSysMajorVer()
    {
        return Environment.OSVersion.Version.Major;
    }

    //桌面上SysListView32的窗口句柄 
    public static IntPtr GetListViewHandle()
    {
        IntPtr _ProgMan = GetShellWindow();
        IntPtr _SHELLDLL_DefViewParent = _ProgMan;
        IntPtr _SHELLDLL_DefView = FindWindowEx(_ProgMan, IntPtr.Zero, "SHELLDLL_DefView", null);
        IntPtr _SysListView32 = FindWindowEx(_SHELLDLL_DefView, IntPtr.Zero, "SysListView32", "FolderView");
        
        if (_SHELLDLL_DefView == IntPtr.Zero)
        {
            const int maxChars = 256;
            StringBuilder clsNameSb = new StringBuilder(maxChars);
            EnumWindows((hwnd, lParam) =>
            {
                GetClassName(hwnd, clsNameSb, maxChars);
                string clsName = clsNameSb.ToString();
                Debug.Log($"Search: {clsName}");
                //if (clsName == "WorkerW")
                {
                    IntPtr child = FindWindowEx(hwnd, IntPtr.Zero, "SHELLDLL_DefView", null);
                    if (child != IntPtr.Zero)
                    {
                        _SHELLDLL_DefViewParent = hwnd;
                        _SHELLDLL_DefView = child;
                        _SysListView32 = FindWindowEx(child, IntPtr.Zero, "SysListView32", "FolderView"); ;
                        return false;
                    }
                }
                return true;
            }, IntPtr.Zero);
        }
        
        // switch (returnType)
        // {
        //     case GetHandleReturnType.ProgMan:
        //         return _ProgMan;
        //     case GetHandleReturnType.SHELLDLL_DefViewParent:
        //         return _SHELLDLL_DefViewParent;
        //     case GetHandleReturnType.SHELLDLL_DefView:
        //         return _SHELLDLL_DefView;
        //     case GetHandleReturnType.SysListView32:
        //         return _SysListView32;
        //     default:
        //         return IntPtr.Zero;
        // }

        //Debug.Log($"{_ProgMan}, {_SHELLDLL_DefViewParent}, {_SHELLDLL_DefView}, {_SysListView32}");

        return _SysListView32;
    }

    /// <summary>
    /// 获取桌面项目的名称
    /// </summary>
    /// <returns></returns>
    public static void GetIconNamesAndPoints(out string[] iconNames, out System.Drawing.Point[] iconPoints)
    {
        
        IntPtr vHandle = GetListViewHandle();
        int vItemCount = ListView_GetItemCount(vHandle);//个数
        uint vProcessId; //进程 pid
        GetWindowThreadProcessId(vHandle, out vProcessId);
        //打开并插入进程 
        IntPtr vProcess = OpenProcess(PROCESS_VM_OPERATION | PROCESS_VM_READ |
            PROCESS_VM_WRITE, false, vProcessId);
        IntPtr vPointer = VirtualAllocEx(vProcess, IntPtr.Zero, 4096,
            MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE);
        
        iconNames = new string[vItemCount];
        iconPoints = new System.Drawing.Point[vItemCount];

        try
        {
            for (int i = 0; i < vItemCount; i++)
            {
                byte[] vBuffer = new byte[256];
                LVITEM[] vItem = new LVITEM[1];
                vItem[0].mask = LVIF_TEXT;
                vItem[0].iItem = i;
                vItem[0].iSubItem = 0;
                vItem[0].cchTextMax = vBuffer.Length;
                vItem[0].pszText = (IntPtr)((int)vPointer + Marshal.SizeOf(typeof(LVITEM)));
                uint vNumberOfBytesRead = 0;
                /// 分配内存空间
                WriteProcessMemory(vProcess, vPointer,
                    Marshal.UnsafeAddrOfPinnedArrayElement(vItem, 0),
                    Marshal.SizeOf(typeof(LVITEM)), ref vNumberOfBytesRead);

                //发送信息 获取响应
                SendMessage(vHandle, LVM_GETITEMW, i, vPointer.ToInt32());
                ReadProcessMemory(vProcess,
                    (IntPtr)((int)vPointer + Marshal.SizeOf(typeof(LVITEM))),
                    Marshal.UnsafeAddrOfPinnedArrayElement(vBuffer, 0),
                    vBuffer.Length, ref vNumberOfBytesRead);
                string vText = Encoding.Unicode.GetString(vBuffer, 0,
                    (int)vNumberOfBytesRead).TrimEnd('\0');
                iconNames[i] = vText;
                
                ListView_GetItemPosition(vHandle, i, vPointer);
                ReadProcessMemory(vProcess,
                    vPointer,
                    Marshal.UnsafeAddrOfPinnedArrayElement(iconPoints, i), Marshal.SizeOf(typeof(System.Drawing.Point)), ref vNumberOfBytesRead);
            }
        }
        catch (Exception Ex)
        {
            throw Ex;
        }
        finally
        {
            VirtualFreeEx(vProcess, vPointer, 0, MEM_RELEASE);
            CloseHandle(vProcess);
        }

    }

    
}