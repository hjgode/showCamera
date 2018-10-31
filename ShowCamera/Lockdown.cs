
//TCO_Client_GUI
namespace Lockdown
{

    #region Namespace Import Declarations

    using Microsoft.Win32;
    using System.Drawing;
    using System;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    #endregion

    public class LockDown:IDisposable
    {

        #region Fields
        private static int hardwareDoneKeyEnabled;
        private static int hardwareStartKeyEnabled;
        private static string MENUBAR;
        private static int textModeEnabled;
        #endregion

        #region Nested Types

        public enum FullScreenStates
        {
            SHFS_SHOWTASKBAR = 1,
            SHFS_HIDETASKBAR = 2,
            SHFS_SHOWSIPBUTTON = 4,
            SHFS_HIDESIPBUTTON = 8,
            SHFS_SHOWSTARTICON = 16,
            SHFS_HIDESTARTICON = 32,
        }


        public enum GetWindowFlags
        {
            GW_HWNDFIRST = 0,
            GW_HWNDLAST = 1,
            GW_HWNDNEXT = 2,
            GW_HWNDPREV = 3,
            GW_OWNER = 4,
            GW_CHILD = 5,
            GW_MAX = 5,
        }


        [System.Runtime.InteropServices.StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }


        public enum ShowWindowFlags
        {
            SW_HIDE = 0,
            SW_SHOWNORMAL = 1,
            SW_SHOWNOACTIVATE = 4,
            SW_SHOW = 5,
            SW_MINIMIZE = 6,
            SW_SHOWNA = 8,
            SW_SHOWMAXIMIZED = 11,
            SW_MAXIMIZE = 12,
            SW_RESTORE = 13,
        }

        #endregion

        #region Constructors

        public LockDown()
        {
        }
        static LockDown()
        {
            MENUBAR = "SOFTWARE\\Microsoft\\Shell\\BubbleTiles";
            SaveRegistryFullScreen();
            textModeEnabled = -1;
            hardwareStartKeyEnabled = -1;
            hardwareDoneKeyEnabled = -1;
        }

        #endregion

        #region Methods
        public void Dispose()
        {
            RestoreRegistryFullScreen();
        }

        [DllImportAttribute("coredll.dll", EntryPoint = "AllKeys", SetLastError = true, PreserveSig = true, CallingConvention = CallingConvention.Winapi)]
        [PreserveSigAttribute()]
        public static extern bool AllKeys(bool bAllKeys);

        [DllImportAttribute("coredll.dll", EntryPoint = "FindWindow", SetLastError = true, PreserveSig = true, CallingConvention = CallingConvention.Winapi)]
        [PreserveSigAttribute()]
        private static extern IntPtr FindWindow(string className, string wndName);
        [DllImportAttribute("coredll.dll", EntryPoint = "GetCapture", SetLastError = false, PreserveSig = true, CallingConvention = CallingConvention.Winapi)]
        [PreserveSigAttribute()]
        private static extern IntPtr GetCapture();
        [DllImportAttribute("coredll.dll", EntryPoint = "GetWindow", SetLastError = false, PreserveSig = true, CallingConvention = CallingConvention.Winapi)]
        [PreserveSigAttribute()]
        private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);
        [DllImportAttribute("coredll.dll", EntryPoint = "MoveWindow", SetLastError = true, PreserveSig = true, CallingConvention = CallingConvention.Winapi)]
        [PreserveSigAttribute()]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
        public static bool RestoreRegistryFullScreen()
        {
            Exception exception1;
            RegistryKey registryKey1 = ((RegistryKey)null);
            try
            {
                registryKey1 = Registry.LocalMachine.OpenSubKey(LockDown.MENUBAR, true);
                if (LockDown.textModeEnabled >= 0)
                {
                    registryKey1.SetValue("TextmodeEnabled", LockDown.textModeEnabled, RegistryValueKind.DWord);
                }
                else
                {
                    registryKey1.DeleteValue("TextmodeEnabled", false);
                }
                if (LockDown.hardwareStartKeyEnabled >= 0)
                {
                    registryKey1.SetValue("HardwareStartKeyEnabled", LockDown.hardwareStartKeyEnabled, RegistryValueKind.DWord);
                }
                else
                {
                    registryKey1.DeleteValue("HardwareStartKeyEnabled", false);
                }
                if (LockDown.hardwareDoneKeyEnabled >= 0)
                {
                    registryKey1.SetValue("HardwareDoneKeyEnabled", LockDown.hardwareDoneKeyEnabled, RegistryValueKind.DWord);
                }
                else
                {
                    registryKey1.DeleteValue("HardwareDoneKeyEnabled", false);
                }
                registryKey1.Close();
            }
            catch (Exception exception2)
            {
                exception1 = exception2;
                if (registryKey1 != null)
                {
                    registryKey1.Close();
                }
                DialogResult dialogResult1 = MessageBox.Show(("Error in RestoreRegistryFullScreen: " + exception1.Message));
                return false;
            }
            return true;
        }

        public static bool SaveRegistryFullScreen()
        {
            RegistryKey registryKey1 = ((RegistryKey)null);
            try
            {
                registryKey1 = Registry.LocalMachine.OpenSubKey(LockDown.MENUBAR, true);
                try
                {
                    LockDown.textModeEnabled = ((int)registryKey1.GetValue("TextmodeEnabled", -1));
                }
                catch
                {
                    textModeEnabled = -1;
                }
                try
                {
                    LockDown.hardwareStartKeyEnabled = ((int)registryKey1.GetValue("HardwareStartKeyEnabled", -1));
                }
                catch
                {
                    LockDown.hardwareStartKeyEnabled = -1;
                }
                try
                {
                    LockDown.hardwareDoneKeyEnabled = ((int)registryKey1.GetValue("HardwareDoneKeyEnabled", -1));
                }
                catch
                {
                    LockDown.hardwareDoneKeyEnabled = -1;
                }
                registryKey1.Close();
            }
            catch
            {
                if (registryKey1 != null)
                {
                    registryKey1.Close();
                }
                return false;
            }
            return true;
        }

        /// <summary>
        /// set registry for hardware buttons present
        /// if a hardware button is marked as present
        /// there will be no on-screen icon
        /// </summary>
        /// <param name="bStartButtonPresent">hardware start-button present?</param>
        /// <param name="bDoneButtonPresent">hardware done-button present?</param>
        /// <param name="bTextMode">use text or graphic tile mode for menubar symbols and text</param>
        /// <returns></returns>
        public static bool SetRegistryFullScreen(bool bStartButtonPresent, bool bDoneButtonPresent, bool bTextMode)
        {
            Exception exception1;
            RegistryKey registryKey1 = ((RegistryKey)null);
            try
            {
                registryKey1 = Registry.LocalMachine.OpenSubKey(LockDown.MENUBAR, true);
                if (bTextMode)
                {
                    registryKey1.SetValue("TextmodeEnabled", 1, RegistryValueKind.DWord);
                }
                else
                {
                    registryKey1.DeleteValue("TextmodeEnabled", false);
                }
                if (bStartButtonPresent)
                {
                    registryKey1.SetValue("HardwareStartKeyEnabled", 1, RegistryValueKind.DWord);
                }
                else
                {
                    registryKey1.DeleteValue("HardwareStartKeyEnabled", false);
                }
                if (bDoneButtonPresent)
                {
                    registryKey1.SetValue("HardwareDoneKeyEnabled", 1, RegistryValueKind.DWord);
                }
                else
                {
                    registryKey1.DeleteValue("HardwareDoneKeyEnabled", false);
                }
                registryKey1.Close();
            }
            catch (Exception exception2)
            {
                exception1 = exception2;
                if (registryKey1 != null)
                {
                    registryKey1.Close();
                }
                DialogResult dialogResult1 = MessageBox.Show(("Error in GetRegistryDiagnostics: " + exception1.Message));
                return false;
            }
            return true;
        }

        [DllImportAttribute("aygshell.dll", EntryPoint = "SHFullScreen", SetLastError = false, PreserveSig = true, CallingConvention = CallingConvention.Winapi)]
        [PreserveSigAttribute()]
        private static extern uint SHFullScreen(IntPtr hwndRequester, uint dwState);
        public static void ShowFullScreen(Form form, bool bShow)
        {
            IntPtr intPtr1;
            form.Show();
            form.Capture = true;
            IntPtr intPtr2 = GetCapture();
            form.Capture = false;
            if (bShow)
            {
                bool b1 = AllKeys(false);
                uint uInt32_1 = SHFullScreen(intPtr2, (uint) (FullScreenStates.SHFS_SHOWSIPBUTTON | FullScreenStates.SHFS_SHOWSTARTICON | FullScreenStates.SHFS_SHOWTASKBAR)); //21
                intPtr1 = FindWindow("HHTaskBar", "");
                if (intPtr1 != IntPtr.Zero)
                {
                    bool b2 = ShowWindow(intPtr1, (int)(ShowWindowFlags.SW_SHOW));// 5);
                }
            }
            else
            {
                bool b3 = AllKeys(true);
                uint uInt32_2 = SHFullScreen(intPtr2, (uint)(FullScreenStates.SHFS_HIDESIPBUTTON | FullScreenStates.SHFS_HIDESTARTICON | FullScreenStates.SHFS_HIDETASKBAR));// 42);
                intPtr1 = FindWindow("HHTaskBar", "");
                if (intPtr1 != IntPtr.Zero)
                {
                    bool b5 = ShowWindow(intPtr1, (int)(ShowWindowFlags.SW_HIDE));// 0);
                }
            }
            ShowKeyboardSIP(bShow);
            bool b6 = ShowWindow(intPtr2, (int)(ShowWindowFlags.SW_SHOW));// 5);
            form.Show();
        }

        public static void ShowKeyboardSIP(bool bShow)
        {
            IntPtr intPtr1 = FindWindow("MS_SIPBUTTON", "MS_SIPBUTTON");
            if (intPtr1 == IntPtr.Zero)
            {
                return;
            }
            IntPtr intPtr2 = GetWindow(intPtr1, (uint)GetWindowFlags.GW_CHILD);
            if (intPtr2 != IntPtr.Zero)
            {
                bool b1 = ShowWindow(intPtr2, (bShow ? (int)(ShowWindowFlags.SW_SHOWNORMAL | ShowWindowFlags.SW_SHOWNOACTIVATE) : 0 ));// 5 : 0));
                bool b2 = ShowWindow(intPtr1, (bShow ? (int)(ShowWindowFlags.SW_SHOWNORMAL | ShowWindowFlags.SW_SHOWNOACTIVATE) : 0));// 5 : 0));5 : 0));
            }
        }

        [DllImportAttribute("coredll.dll", EntryPoint = "ShowWindow", SetLastError = false, PreserveSig = true, CallingConvention = CallingConvention.Winapi)]
        [PreserveSigAttribute()]
        public static extern bool ShowWindow(IntPtr hWnd, int cmdShow);
        [DllImportAttribute("coredll.dll", EntryPoint = "SystemParametersInfo", SetLastError = false, PreserveSig = true, CallingConvention = CallingConvention.Winapi)]
        [PreserveSigAttribute()]
        private static extern int SystemParametersInfo(int uiAction, int uiParam, ref RECT pvParam, int fWinIni);
        #endregion
    }

}

