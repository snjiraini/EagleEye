using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Telerik.WinControls.UI;
using Telerik.WinControls;
using AForge;
using AForge.Video;
using AForge.Video.DirectShow;
using System.Drawing.Imaging;
using System.Diagnostics;

namespace EagleEye
{
    public partial class frmMain : Telerik.WinControls.UI.RadForm
    {
        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoSource;
        private Bitmap img;

        // stop watch for measuring fps
        private Stopwatch stopWatch = null;

        public frmMain()
        {
            InitializeComponent();

            camera1FpsLabel.Text = string.Empty;
            camera2FpsLabel.Text = string.Empty;


            this.Width = 1000;
            this.Height = 620;
            ThemeResolutionService.ApplicationThemeName = "Office2010Black";
            backgroundSnapshots.WorkerReportsProgress = true;
            backgroundSnapshots.WorkerSupportsCancellation = true;

          

            // show device list
            try
            {
                // enumerate video devices
                videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

                if (videoDevices.Count == 0)
                {
                    throw new Exception();
                }

                for (int i = 1, n = videoDevices.Count; i <= n; i++)
                {
                    string cameraName = i + " : " + videoDevices[i - 1].Name;

                    camera1Combo.Items.Add(cameraName);
                    camera2Combo.Items.Add(cameraName);
                }

                // check cameras count
                if (videoDevices.Count == 1)
                {
                    camera2Combo.Items.Clear();

                    camera2Combo.Items.Add("Only one camera found");
                    camera2Combo.SelectedIndex = 0;
                    camera2Combo.Enabled = false;
                }
                else
                {
                    camera2Combo.SelectedIndex = 1;
                }
                camera1Combo.SelectedIndex = 0;
            }
            catch
            {
                btnStartVideo.Enabled = false;

                camera1Combo.Items.Add("No cameras found");
                camera2Combo.Items.Add("No cameras found");

                camera1Combo.SelectedIndex = 0;
                camera2Combo.SelectedIndex = 0;

                camera1Combo.Enabled = false;
                camera2Combo.Enabled = false;
            }
        }

        // Start cameras
        private void StartCameras()
        {
            // create first video source
            VideoCaptureDevice videoSource1 = new VideoCaptureDevice(videoDevices[camera1Combo.SelectedIndex].MonikerString);
            

            videoSourcePlayer1.VideoSource = videoSource1;
            videoSourcePlayer1.Start();

            // create second video source
            if (camera2Combo.Enabled == true)
            {
                System.Threading.Thread.Sleep(500);

                VideoCaptureDevice videoSource2 = new VideoCaptureDevice(videoDevices[camera2Combo.SelectedIndex].MonikerString);
                

                videoSourcePlayer2.VideoSource = videoSource2;
                videoSourcePlayer2.Start();
            }

            // reset stop watch
            stopWatch = null;
            // start timer
            timer.Start();
        }

        // Stop cameras
        private void StopCameras()
        {
            timer.Stop();

            videoSourcePlayer1.SignalToStop();
            videoSourcePlayer2.SignalToStop();

            videoSourcePlayer1.WaitForStop();
            videoSourcePlayer2.WaitForStop();
        }


        private void FrmMain_Load(object sender, EventArgs e)
        {
            //videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            //videoSource = new VideoCaptureDevice();


        }

        // On form closing
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopCameras();
        }

        private void BtnAlertsUP_Click(object sender, EventArgs e)
        {

        }

        private void RadGridView3_Click(object sender, EventArgs e)
        {

        }

        private void RadPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            img = (Bitmap)eventArgs.Frame.Clone();

        }

        private void BtnStartVideo_Click(object sender, EventArgs e)
        {
            StartCameras();

            btnStartVideo.Enabled = false;
            btnStopVideo.Enabled = true;

        }

        private void BtnStopVideo_Click(object sender, EventArgs e)
        {

            StopCameras();

            btnStartVideo.Enabled = true;
            btnStopVideo.Enabled = false;

            camera1FpsLabel.Text = string.Empty;
            camera2FpsLabel.Text = string.Empty;

        }

        // On times tick - collect statistics
        private void Timer_Tick_1(object sender, EventArgs e)
        {
            IVideoSource videoSource1 = videoSourcePlayer1.VideoSource;
            IVideoSource videoSource2 = videoSourcePlayer2.VideoSource;

            int framesReceived1 = 0;
            int framesReceived2 = 0;

            // get number of frames for the last second
            if (videoSource1 != null)
            {
                framesReceived1 = videoSource1.FramesReceived;

                //Take a snapshot of current frame and save photo
                videoSource1.NewFrame += new NewFrameEventHandler(VideoSource_NewFrame);
                save_snapshots();

            }

            if (videoSource2 != null)
            {
                framesReceived2 = videoSource2.FramesReceived;

                //Take a snapshot of current frame and save photo
                videoSource2.NewFrame += new NewFrameEventHandler(VideoSource_NewFrame);
                save_snapshots();
            }

            if (stopWatch == null)
            {
                stopWatch = new Stopwatch();
                stopWatch.Start();
            }
            else
            {
                stopWatch.Stop();

                float fps1 = 1000.0f * framesReceived1 / stopWatch.ElapsedMilliseconds;
                float fps2 = 1000.0f * framesReceived2 / stopWatch.ElapsedMilliseconds;

                camera1FpsLabel.Text = fps1.ToString("F2") + " fps";
                camera2FpsLabel.Text = fps2.ToString("F2") + " fps";

                stopWatch.Reset();
                stopWatch.Start();
            }
        }

        private void save_snapshots()
        {
            if (img != null)
            {
                //Save First
                DateTime datetime1 = DateTime.Now;
                String randomstring = datetime1.ToString("yyyyMMddHHmmss");
                Bitmap varBmp = new Bitmap(img);
                Bitmap newBitmap = new Bitmap(varBmp);
                varBmp.Save(Application.StartupPath + @"\snapshots\" + randomstring + ".png", ImageFormat.Png);
                //pictureBox1.Image = img;

                //Now Dispose to free the memory
                varBmp.Dispose();
                varBmp = null;

            }
        }



        //private void Timerbackgroundsnapshots_Tick(object sender, EventArgs e)
        //{

        //    // Perform a time consuming operation and report progress.
        //    for (int i = 0; i <= videoDevices.Count - 1; i++)
        //    {
        //        //ignore internal webcam
        //        if (videoDevices[i].Name != "HP TrueVision FHD RGB-IR")
        //        {
        //            videoSource = new VideoCaptureDevice(videoDevices[i].MonikerString);
        //            videoSource.NewFrame += new NewFrameEventHandler(VideoSource_NewFrame);

        //            if (videoSource.IsRunning == false)
        //            {
        //                videoSource.Start();
        //            }

        //        }


        //        if (img != null)
        //        {
        //            //Save First
        //            DateTime datetime1 = DateTime.Now;
        //            String randomstring = datetime1.ToString("yyyyMMddHHmmss");
        //            Bitmap varBmp = new Bitmap(img);
        //            Bitmap newBitmap = new Bitmap(varBmp);
        //            varBmp.Save(Application.StartupPath + @"\snapshots\" + randomstring + ".png", ImageFormat.Png);
        //            //pictureBox1.Image = img;

        //            //Now Dispose to free the memory
        //            varBmp.Dispose();
        //            varBmp = null;

        //        }

        //        //// stop video device
        //        //if (videoSource != null)
        //        //{
        //        //    videoSource.SignalToStop();
        //        //    videoSource.WaitForStop();
        //        //     videoSource = null;
        //        //}



        //        System.Threading.Thread.Sleep(250);


        //    }
        //}
    }
}
