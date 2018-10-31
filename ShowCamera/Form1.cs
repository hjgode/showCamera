using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ShowCamera
{
    public partial class Form1 : Form
    {
        ShowCameraApp showCameraApp;
        public Form1()
        {
            InitializeComponent();
            showCameraApp = new ShowCameraApp();
            showCameraApp.RaiseCustomEvent+=new EventHandler<ShowCameraApp.CustomEventArgs>(showCameraApp_RaiseCustomEvent);
        }

        void showCameraApp_RaiseCustomEvent(object sender, ShowCameraApp.CustomEventArgs a)
        {
            if (a._status == ShowCameraApp.CameraStatus.started)
            {
                btnStart.Enabled = false;
                btnStop.Enabled = true;
                listBox1.Items.Clear();
            }
            else if (a._status == ShowCameraApp.CameraStatus.stopped)
            {
                btnStart.Enabled = true;
                btnStop.Enabled = false;
                foreach (string s in showCameraApp._newImages)
                {
                    addItem(s);
                }
            }

        }
        delegate void SetListboxCallback(string text);
        public void addItem(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.listBox1.InvokeRequired)
            {
                SetListboxCallback d = new SetListboxCallback(addItem);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                listBox1.Items.Add(text);
            }
        }
        private void btnStart_Click(object sender, EventArgs e)
        {
            if (showCameraApp._bRunning == false)
            {
                showCameraApp.startCamera();
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (showCameraApp._bRunning)
            {
                showCameraApp.stopCamera();
            }

        }
    }
}