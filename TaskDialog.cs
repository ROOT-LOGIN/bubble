/*
RELEASE TO PUBLIC DOMAIN!
NO ANY WARRANTY!
USE ON YOUR OWN RISK!
*/

using System;
using System.Runtime.InteropServices;

namespace System.Windows.TaskDialog
{
    public static class NativeApi
    {
        /// <summary>
        /// The true
        /// </summary>
        public static readonly IntPtr TRUE = new IntPtr(1);
        /// <summary>
        /// The false
        /// </summary>
        public static readonly IntPtr FALSE = new IntPtr(0);
        /// <summary>
        /// The sizeof <seealso cref="TASKDIALOGCONFIG"/>
        /// </summary>
        public static readonly int SIZEOF_TASKDIALOGCONFIG = 4 + IntPtr.Size + IntPtr.Size + 4 + 4 + IntPtr.Size + IntPtr.Size + IntPtr.Size + IntPtr.Size + 4 + IntPtr.Size + 4 + 4 + IntPtr.Size + 4 + IntPtr.Size + IntPtr.Size + IntPtr.Size + IntPtr.Size + IntPtr.Size + IntPtr.Size + IntPtr.Size + IntPtr.Size + 4;

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="hwnd">The HWND.</param>
        /// <param name="msg">The MSG.</param>
        /// <param name="wp">The wp.</param>
        /// <param name="lp">The lp.</param>
        /// <returns></returns>
        [DllImport("User32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr SendMessage(IntPtr hwnd, int msg, IntPtr wp, IntPtr lp);

        /// <summary>
        /// Create task dialog.
        /// </summary>
        /// <param name="hwnd">The HWND.</param>
        /// <param name="hinstance">The hinstance.</param>
        /// <param name="title">The title.</param>
        /// <param name="instruction">The instruction.</param>
        /// <param name="content">The content.</param>
        /// <param name="commonButtons">The common buttons.</param>
        /// <param name="icon">The icon.</param>
        /// <param name="button">The clicked button.</param>
        /// <returns></returns>
        [DllImport("ComCtl32.dll", CharSet = CharSet.None)]
        public static extern IntPtr TaskDialog(IntPtr hwnd, IntPtr hinstance, [MarshalAs(UnmanagedType.LPWStr)]string title, [MarshalAs(UnmanagedType.LPWStr)]string instruction, [MarshalAs(UnmanagedType.LPWStr)]string content, TaskDialogCommonButton commonButtons, IntPtr icon, out int button);

        /// <summary>
        /// Create task dialog indirect.
        /// </summary>
        /// <param name="task">the task dialog config.</param>
        /// <param name="button">The clicked button.</param>
        /// <param name="radioButton">The checked radio button.</param>
        /// <param name="verificationChecked">Whether verification is checked.</param>
        /// <returns></returns>
        [DllImport("ComCtl32.dll", CharSet = CharSet.None)]
        public static extern IntPtr TaskDialogIndirect(ref TASKDIALOGCONFIG task, out int button, out int radioButton, out int verificationChecked);

#if DEBUG
        public static void TestTaskDialog(string caption, string[] argv)
        {
            int c1, c2, c3;
            TaskDialog(IntPtr.Zero, IntPtr.Zero, caption, "无效的命令行参数", string.Join(" ", argv), TaskDialogCommonButton.TDCBF_OK_BUTTON | TaskDialogCommonButton.TDCBF_CANCEL_BUTTON | TaskDialogCommonButton.TDCBF_RETRY_BUTTON, TaskDialogIcons.TD_WARNING_ICON, out c1);

            TASKDIALOGCONFIG task = new TASKDIALOGCONFIG();
            task.cbSize = SIZEOF_TASKDIALOGCONFIG;
            task.dwFlags = TaskDialogFlags.TDF_ENABLE_HYPERLINKS | TaskDialogFlags.TDF_CALLBACK_TIMER | TaskDialogFlags.TDF_CAN_BE_MINIMIZED
                | TaskDialogFlags.TDF_EXPANDED_BY_DEFAULT
                | TaskDialogFlags.TDF_USE_COMMAND_LINKS
                | TaskDialogFlags.TDF_VERIFICATION_FLAG_CHECKED
                | TaskDialogFlags.TDF_SHOW_MARQUEE_PROGRESS_BAR
                | TaskDialogFlags.TDF_EXPAND_FOOTER_AREA
                ;
            // TaskDialogFlags.TDF_USE_HICON_MAIN | TaskDialogFlags.TDF_USE_HICON_FOOTER  | 
            task.dwCommonButtons = TaskDialogCommonButton.TDCBF_OK_BUTTON | TaskDialogCommonButton.TDCBF_CANCEL_BUTTON | TaskDialogCommonButton.TDCBF_RETRY_BUTTON;
            task.pszWindowTitle = caption;
            task.hMainIcon = TaskDialogIcons.TD_ERROR_ICON;
            task.pszMainInstruction = "无效的命令行参数";
            task.pszContent = string.Join(" ", argv);
            task.cButtons = 2;
            task.nDefaultButton = DialogButtonIds.IDOK;
            task.pszVerificationText = "关闭后以默认参数运行程序";
            task.pszExpandedInformation = 
@"以下是有效的命令行参数：
    /designer
    /report
    /help, /?";
            task.pszExpandedControlText = "隐藏更多内容";
            task.pszCollapsedControlText = "显示更多内容";
            task.hFooterIcon = TaskDialogIcons.TD_INFORMATION_ICON;
            task.pszFooter = @"<A HREF=""http://localhost/"">点击这里</A>查看相关说明。";
            task.cxWidth = 300;
            task.pfCallback = TaskDialogCallback;

            task.pButtons = TASKDIALOGCONFIG.MarshalButtons(new[]
            {
                    new TaskDialogButton() {nButtonID = DialogButtonIds.IDOK, pszButtonText = "OKOKOKOKOKOK\n**HA-HA-HA-HA-HA" },
                    new TaskDialogButton() {nButtonID = DialogButtonIds.IDNO, pszButtonText = "NONONONONONO\n**YE-YE-YE-YE-YE" }
                });

            using (task)
            {
                var sz = Marshal.SizeOf(typeof(TASKDIALOGCONFIG));
                IntPtr v = TaskDialogIndirect(ref task, out c1, out c2, out c3);
            }
        }

        static int TaskDialogCallback(IntPtr hwnd, TaskDialogNotifications uNotification, IntPtr wp, IntPtr lp, IntPtr dwRefData)
        {
            switch (uNotification)
            {
                case TaskDialogNotifications.TDN_CREATED:
                    SendMessage(hwnd, (int)TaskDialogMessages.TDM_SET_PROGRESS_BAR_MARQUEE, TRUE, new IntPtr(20));
                    break;
                case TaskDialogNotifications.TDN_NAVIGATED:
                    break;
                case TaskDialogNotifications.TDN_BUTTON_CLICKED:
                    switch (wp.ToInt32())
                    {
                        case DialogButtonIds.IDCANCEL:
                            return HRESULT.S_OK;
                        default:
                            return HRESULT.S_FALSE;
                    }
                case TaskDialogNotifications.TDN_HYPERLINK_CLICKED:
                    int a = 0;
                    break;
                case TaskDialogNotifications.TDN_TIMER:
                    break;
                case TaskDialogNotifications.TDN_DESTROYED:
                    return HRESULT.S_OK;
                case TaskDialogNotifications.TDN_RADIO_BUTTON_CLICKED:
                    break;
                case TaskDialogNotifications.TDN_DIALOG_CONSTRUCTED:
                    break;
                case TaskDialogNotifications.TDN_VERIFICATION_CLICKED:
                    break;
                case TaskDialogNotifications.TDN_HELP:
                    break;
                case TaskDialogNotifications.TDN_EXPANDO_BUTTON_CLICKED:
                    break;
                default:
                    break;
            }
            return HRESULT.S_FALSE;
        }
#endif
    }

    /// <summary>
    /// Task dialog notification
    /// </summary>
    public enum TaskDialogNotifications
    {
        TDN_CREATED = 0,
        TDN_NAVIGATED = 1,
        TDN_BUTTON_CLICKED = 2,            // wParam = Button ID
        TDN_HYPERLINK_CLICKED = 3,            // lParam = (LPCWSTR)pszHREF
        TDN_TIMER = 4,            // wParam = Milliseconds since dialog created or timer reset
        TDN_DESTROYED = 5,
        TDN_RADIO_BUTTON_CLICKED = 6,            // wParam = Radio Button ID
        TDN_DIALOG_CONSTRUCTED = 7,
        TDN_VERIFICATION_CLICKED = 8,             // wParam = 1 if checkbox checked, 0 if not, lParam is unused and always 0
        TDN_HELP = 9,
        TDN_EXPANDO_BUTTON_CLICKED = 10            // wParam = 0 (dialog is now collapsed), wParam != 0 (dialog is now expanded)
    }

    /// <summary>
    /// The intrinsic task dialog icons
    /// </summary>
    public static class TaskDialogIcons
    {
        public static readonly IntPtr TD_WARNING_ICON = new IntPtr(-1 & 0xFFFF);
        public static readonly IntPtr TD_ERROR_ICON = new IntPtr(-2 & 0xFFFF);
        public static readonly IntPtr TD_INFORMATION_ICON = new IntPtr(-3 & 0xFFFF);
        public static readonly IntPtr TD_SHIELD_ICON = new IntPtr(-4 & 0xFFFF);
    }

    /// <summary>
    /// Task dialog messages
    /// </summary>
    public enum TaskDialogMessages
    {
        WM_USER = 0x0400,
        TDM_NAVIGATE_PAGE = WM_USER + 101,
        TDM_CLICK_BUTTON = WM_USER + 102, // wParam = Button ID
        TDM_SET_MARQUEE_PROGRESS_BAR = WM_USER + 103, // wParam = 0 (nonMarque) wParam != 0 (Marquee)
        TDM_SET_PROGRESS_BAR_STATE = WM_USER + 104, // wParam = new progress state
        TDM_SET_PROGRESS_BAR_RANGE = WM_USER + 105, // lParam = MAKELPARAM(nMinRange, nMaxRange)
        TDM_SET_PROGRESS_BAR_POS = WM_USER + 106, // wParam = new position
        TDM_SET_PROGRESS_BAR_MARQUEE = WM_USER + 107, // wParam = 0 (stop marquee), wParam != 0 (start marquee), lparam = speed (milliseconds between repaints)
        TDM_SET_ELEMENT_TEXT = WM_USER + 108, // wParam = element (TASKDIALOG_ELEMENTS), lParam = new element text (LPCWSTR)
        TDM_CLICK_RADIO_BUTTON = WM_USER + 110, // wParam = Radio Button ID
        TDM_ENABLE_BUTTON = WM_USER + 111, // lParam = 0 (disable), lParam != 0 (enable), wParam = Button ID
        TDM_ENABLE_RADIO_BUTTON = WM_USER + 112, // lParam = 0 (disable), lParam != 0 (enable), wParam = Radio Button ID
        TDM_CLICK_VERIFICATION = WM_USER + 113, // wParam = 0 (unchecked), 1 (checked), lParam = 1 (set key focus)
        TDM_UPDATE_ELEMENT_TEXT = WM_USER + 114, // wParam = element (TASKDIALOG_ELEMENTS), lParam = new element text (LPCWSTR)
        TDM_SET_BUTTON_ELEVATION_REQUIRED_STATE = WM_USER + 115, // wParam = Button ID, lParam = 0 (elevation not required), lParam != 0 (elevation required)
        TDM_UPDATE_ICON = WM_USER + 116  // wParam = icon element (TASKDIALOG_ICON_ELEMENTS), lParam = new icon (hIcon if TDF_USE_HICON_* was set, PCWSTR otherwise)
    }

    /// <summary>
    /// Task dialog common buttons
    /// </summary>
    public enum TaskDialogCommonButton
    {
        TDCBF_OK_BUTTON = 0x0001, // selected control return value IDOK
        TDCBF_YES_BUTTON = 0x0002, // selected control return value IDYES
        TDCBF_NO_BUTTON = 0x0004, // selected control return value IDNO
        TDCBF_CANCEL_BUTTON = 0x0008, // selected control return value IDCANCEL
        TDCBF_RETRY_BUTTON = 0x0010, // selected control return value IDRETRY
        TDCBF_CLOSE_BUTTON = 0x0020  // selected control return value IDCLOSE
    };

    /// <summary>
    /// The windows dialog button ids
    /// </summary>
    public sealed class DialogButtonIds
    {
        private DialogButtonIds() { throw new Exception(); }

        public const int IDOK = 1;
        public const int IDCANCEL = 2;
        public const int IDABORT = 3;
        public const int IDRETRY = 4;
        public const int IDIGNORE = 5;
        public const int IDYES = 6;
        public const int IDNO = 7;
        public const int IDCLOSE = 8;
        public const int IDHELP = 9;
        public const int IDTRYAGAIN = 10;
        public const int IDCONTINUE = 11;
        public const int IDTIMEOUT = 32000;
    }

    /// <summary>
    /// The task dialog flags
    /// </summary>
    public enum TaskDialogFlags
    {
        TDF_ENABLE_HYPERLINKS = 0x0001,
        TDF_USE_HICON_MAIN = 0x0002,
        TDF_USE_HICON_FOOTER = 0x0004,
        TDF_ALLOW_DIALOG_CANCELLATION = 0x0008,
        TDF_USE_COMMAND_LINKS = 0x0010,
        TDF_USE_COMMAND_LINKS_NO_ICON = 0x0020,
        TDF_EXPAND_FOOTER_AREA = 0x0040,
        TDF_EXPANDED_BY_DEFAULT = 0x0080,
        TDF_VERIFICATION_FLAG_CHECKED = 0x0100,
        TDF_SHOW_PROGRESS_BAR = 0x0200,
        TDF_SHOW_MARQUEE_PROGRESS_BAR = 0x0400,
        TDF_CALLBACK_TIMER = 0x0800,
        TDF_POSITION_RELATIVE_TO_WINDOW = 0x1000,
        TDF_RTL_LAYOUT = 0x2000,
        TDF_NO_DEFAULT_RADIO_BUTTON = 0x4000,
        TDF_CAN_BE_MINIMIZED = 0x8000,
        TDF_NO_SET_FOREGROUND = 0x00010000, // Don't call SetForegroundWindow() when activating the dialog
        TDF_SIZE_TO_CONTENT = 0x01000000  // used by ShellMessageBox to emulate MessageBox sizing behavior
    };

    /// <summary>
    /// HRESULT Constant
    /// </summary>
    public class HRESULT
    {
        public const int S_FALSE = 1;
        public const int S_OK = 0;
    }

    /// <summary>
    /// Task dialog comman link definition
    /// </summary>
    public struct TaskDialogButton
    {
        public int nButtonID;
        public string pszButtonText;
    }

    /// <summary>
    /// Task dialog callback delegate
    /// </summary>
    /// <param name="hwnd">The HWND.</param>
    /// <param name="uNotification">The notification.</param>
    /// <param name="wParam">The wp parameter.</param>
    /// <param name="lParam">The lp parameter.</param>
    /// <param name="dwRefData">The reference data.</param>
    /// <returns></returns>
    public delegate int TaskDialogCallbackProc(
        IntPtr hwnd,
        TaskDialogNotifications uNotification,
        IntPtr wParam,
        IntPtr lParam,
        IntPtr dwRefData
    );

    /// <summary>
    /// Task dialog config
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public struct TASKDIALOGCONFIG : IDisposable
    {
        public int cbSize;
        public IntPtr hwndParent;                             // incorrectly named, this is the owner window, not a parent.
        public IntPtr hInstance;                              // used for MAKEINTRESOURCE() strings
        public TaskDialogFlags dwFlags;            // TASKDIALOG_FLAGS (TDF_XXX) flags
        public TaskDialogCommonButton dwCommonButtons;    // TASKDIALOG_COMMON_BUTTON (TDCBF_XXX) flags
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pszWindowTitle;                         // string or MAKEINTRESOURCE()
        public IntPtr hMainIcon;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pszMainInstruction;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pszContent;
        public uint cButtons;
        public IntPtr pButtons;
        public int nDefaultButton;
        public uint cRadioButtons;
        public IntPtr pRadioButtons;
        public int nDefaultRadioButton;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pszVerificationText;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pszExpandedInformation;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pszExpandedControlText;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pszCollapsedControlText;
        public IntPtr hFooterIcon;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pszFooter;
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public TaskDialogCallbackProc pfCallback;               // TaskDialogCallbackProc
        public IntPtr lpCallbackData;
        public uint cxWidth;                                // width of the Task Dialog's client area in DLU's. If 0, Task Dialog will calculate the ideal width.

        /// <summary>
        /// Marshals the button definitions.
        /// </summary>
        /// <param name="buttons">The buttons.</param>
        /// <returns></returns>
        public static IntPtr MarshalButtons(TaskDialogButton[] buttons)
        {
            IntPtr retptr = Marshal.AllocHGlobal(buttons.Length * (4 + IntPtr.Size));
            IntPtr ptr = retptr;
            foreach (var btn in buttons)
            {
                Marshal.WriteInt32(ptr, btn.nButtonID);
                ptr += 4;
                Marshal.WriteIntPtr(ptr, Marshal.StringToHGlobalUni(btn.pszButtonText));
                ptr += IntPtr.Size;
            }
            return retptr;
        }

        void IDisposable.Dispose()
        {
            if (pButtons != IntPtr.Zero && cButtons != 0)
            {
                IntPtr ptr = pButtons;
                for (int i = 0; i < cButtons; i++)
                {
                    ptr += 4;
                    Marshal.FreeHGlobal(Marshal.ReadIntPtr(ptr));
                    ptr += 4;
                }
                Marshal.FreeHGlobal(pButtons);
                pButtons = IntPtr.Zero;
                cButtons = 0;
            }
            if (pRadioButtons != IntPtr.Zero && cRadioButtons != 0)
            {
                IntPtr ptr = pRadioButtons;
                for (int i = 0; i < cRadioButtons; i++)
                {
                    ptr += 4;
                    Marshal.FreeHGlobal(Marshal.ReadIntPtr(ptr));
                    ptr += 4;
                }
                Marshal.FreeHGlobal(pRadioButtons);
                pRadioButtons = IntPtr.Zero;
                cRadioButtons = 0;
            }
        }
    };
}
