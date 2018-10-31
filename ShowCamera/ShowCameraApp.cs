using System;

using System.Collections.Generic;
using System.Text;

using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace ShowCamera
{
    class ShowCameraApp: Control
    {
        [DllImport("coredll.dll")]
        private static extern IntPtr GetForegroundWindow();
        [DllImport("coredll.dll")]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder buf, int nMaxCount);
        [DllImport("coredll.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        Thread myThread=null;
#region fields
        private bool bRunning = false;
        string imageDir = @"\My Documents\My Pictures";
        public string _imageDir
        {
            get { return imageDir; }
        }
        string filePrefix = "img";
        List<string> newImages;
        public List<string> _newImages
        {
            get { return newImages; }
        }
        public bool _bRunning
        {
            get { return bRunning; }
        }
        OpenNETCF.IO.FileSystemWatcher fileWatcher;
#endregion
        public ShowCameraApp()
        {
            newImages = new List<string>();
            getCameraSettings(ref imageDir, ref filePrefix);

            fileWatcher = new OpenNETCF.IO.FileSystemWatcher(imageDir, "*.*");
            fileWatcher.EnableRaisingEvents = false;
            OpenNETCF.IO.NotifyFilters nf = OpenNETCF.IO.NotifyFilters.LastAccess | OpenNETCF.IO.NotifyFilters.LastWrite |
                                    OpenNETCF.IO.NotifyFilters.FileName | //OpenNETCF.IO.NotifyFilters.DirectoryName |
                                    OpenNETCF.IO.NotifyFilters.CreationTime;
            fileWatcher.NotifyFilter = nf;
            fileWatcher.IncludeSubdirectories = false;
            fileWatcher.Created += new OpenNETCF.IO.FileSystemEventHandler(fileWatcher_Changed);
            fileWatcher.EnableRaisingEvents = true;
        }

        ~ShowCameraApp()
        {
            this.Dispose();
        }
        public new void Dispose(){
            //need to stop thread to be able to exit the main proccess
            if (myThread != null)
            {
                myThread.Abort();
                bRunning = false;
            }
            base.Dispose();
        }
        void fileWatcher_Changed(object sender, OpenNETCF.IO.FileSystemEventArgs e)
        {
            if (!newImages.Contains(e.FullPath))    //avoid duplicates as event is fired two times for new file
            {
                newImages.Add(e.FullPath);
                System.Diagnostics.Debug.WriteLine("Got new image '"+e.FullPath+"'");
            }
        }
        public void startCamera()
        {
            if (!bRunning)
            {
                myThread = new Thread(new ThreadStart(showCamera));
                myThread.Start();
            }

        }
        public void stopCamera()
        {
            if (bRunning)
            {
                myThread.Abort();
            }
        }
        void getCameraSettings(ref string sImageDir, ref string sFilePrefix)
        {
            const string defaultDir = @"\My Documents\My Pictures";
            string sDir = @"\My Documents\My Pictures";
            string sPrefix = "img";
            try
            {
                Microsoft.Win32.RegistryKey rKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Pictures\Camera\USER");
                sDir = (string)rKey.GetValue("DefaultDir");
                if (sDir == "")
                    sDir = defaultDir;
                sPrefix = (string)rKey.GetValue("FilePrefix");
                rKey.Close();
            }
            catch (Exception)
            {
            }
            sImageDir = sDir;
            sFilePrefix = sPrefix;
            return;
        }

        //a thread
        void showCamera()
        {
            bRunning = true;
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            try
            {
                Lockdown.LockDown.SaveRegistryFullScreen();
                Lockdown.LockDown.SetRegistryFullScreen(true, false, false);
                //possibly you need to kill a running pimg.exe instance
                killPimg();
                System.Threading.Thread.Sleep(1000);
                process.StartInfo.FileName = @"\Windows\pimg.exe";
                process.StartInfo.Arguments = "-camerakey";
                if (process.Start())
                {
                    System.Diagnostics.Debug.WriteLine("Camera started...");
                    //OnRaiseCustomEvent(new CustomEventArgs("Camera started"));
                    OnRaiseCustomEvent(new CustomEventArgs(CameraStatus.started));
                    //process.WaitForExit();    //does not work here, so use the below code to wait for process
                    bool running=false;
                    do{
                        System.Threading.Thread.Sleep(500);
                        //look for window class "Camera View"
                        IntPtr hwndCam=GetForegroundWindow();
                        //string sClass = getClass(hwndCam);
                        string sWText=getWindowText(hwndCam);
                        //if (sClass == "Camera View") // this is only valid for the running preview before taking a picture
                        if(sWText== "Pictures & Videos")    // this is the Window Title for both, the Camera Preview and Camera ImageView
                            running = true;
                        else
                            running = false;
                    }while (running);
                    System.Diagnostics.Debug.WriteLine("Camera stopped...");
                    //OnRaiseCustomEvent(new CustomEventArgs("Camera stopped"));
                    OnRaiseCustomEvent(new CustomEventArgs(CameraStatus.stopped));
                }
                else
                    System.Diagnostics.Debug.WriteLine("Camera start failed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception: " + ex.Message);
            }
            Lockdown.LockDown.RestoreRegistryFullScreen();
            killPimg();
            System.Diagnostics.Debug.WriteLine("Thread stopped...");
            bRunning = false;
            return;// iRet;
        }

        static void killPimg()
        {
            System.Diagnostics.Debug.WriteLine("killing pimg.exe...");
            bool bFound = false;
            OpenNETCF.ToolHelp.ProcessEntry[] peList = OpenNETCF.ToolHelp.ProcessEntry.GetProcesses();
            foreach (OpenNETCF.ToolHelp.ProcessEntry pe in peList)
            {
                if (pe.ExeFile == "pimg.exe")
                {
                    pe.Kill();
                    bFound = true;
                    break;
                }
            }
            if (bFound)
                System.Diagnostics.Debug.WriteLine("KILLED");
            else
                System.Diagnostics.Debug.WriteLine("no pimg.exe running");
        }

        static string getClass(IntPtr hWnd)
        {
            string sRet="";
            StringBuilder sb = new StringBuilder(255);
            if (GetClassName(hWnd, sb, 255) != 0)
            {
                sRet = sb.ToString();
                return sRet;
            }
            else
                return sRet;
        }
        static string getWindowText(IntPtr hWnd)
        {
            string sRet="";
            StringBuilder sb = new StringBuilder(255);
            if (GetWindowText(hWnd, sb, 255) != 0)
            {
                sRet = sb.ToString();
                return sRet;
            }
            else
                return sRet;
        }
        #region event_stuff
        public delegate void CustomEventHandler(object sender, CustomEventArgs a);
        public event EventHandler<CustomEventArgs> RaiseCustomEvent;
        public enum CameraStatus
        {
            unknown = 0,
            started,
            stopped,
        }
        public class CustomEventArgs : EventArgs
        {
            public CustomEventArgs(string s)
            {
                msg = s;
                if (s.IndexOf("start") > 0)
                    _status = CameraStatus.started;
                else if (s.IndexOf("stop") > 0)
                    _status = CameraStatus.stopped;
                else
                    _status = CameraStatus.unknown;
            }
            public CustomEventArgs(CameraStatus status)
            {
                _status = status;
                msg = status.ToString();
            }
            public CameraStatus _status = CameraStatus.unknown;
            private string msg;
            public string Message
            {
                get { return msg; }
                set { msg = value; }
            }
        }
        // Wrap event invocations inside a protected virtual method
        // to allow derived classes to override the event invocation behavior
        delegate void setRaiseEvent(CustomEventArgs e);
        protected virtual void OnRaiseCustomEvent(CustomEventArgs e)
        {
            if (this.InvokeRequired)
            {
                setRaiseEvent d = new setRaiseEvent(OnRaiseCustomEvent);
                this.Invoke(d, new object[] { e });
            }
            else
            {
                if (this.RaiseCustomEvent != null)
                {
                    this.RaiseCustomEvent(this, e);
                }
            }
        }

        #endregion

    }
}
