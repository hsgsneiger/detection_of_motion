using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge.Controls;
using AForge.Video;
using AForge.Video.DirectShow;
using AForge.Vision.Motion;
using Accord.Video.FFMPEG;
namespace XULiAnh
{
    public partial class Form1 : Form
    {
        private float motionLevel = (float)0.001;
        private FilterInfoCollection videoSource;
        private VideoFileSource videoSource2 ;
        private VideoCaptureDevice FinalFrame;
        private string filePath = null;
        MotionDetector Detector1;
        MotionDetector Detector2;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            backgroundBorder.Load("./red.jpg");
            Detector1 = new MotionDetector
                (
                new TwoFramesDifferenceDetector(),
                new MotionBorderHighlighting()
                );
            Detector2 = new MotionDetector
                (
                new TwoFramesDifferenceDetector(),
                new MotionBorderHighlighting()
                );
            videoSource = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach(FilterInfo source in videoSource)
            {
                cbListDevices.Items.Add(source.Name);
            }
            cbListDevices.Items.Add("Browser...");
            cbListDevices.SelectedIndex = 0;
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            if (((Button)sender).Text == "Start")
            {
                if (FinalFrame != null)
                {
                    videoSourcePlayer.SignalToStop();
                }
                startButton.Text = "Stop";
                
                if(cbListDevices.SelectedIndex == cbListDevices.Items.Count-1) // browse
                {
                    pictureBox1.BringToFront();
                    if (videoSourcePlayer.IsRunning)
                        videoSourcePlayer.Stop();
                    videoSource2 = new VideoFileSource(filePath);
                    videoSource2.NewFrame += new Accord.Video.NewFrameEventHandler(video_NewFrame);
                    // start the video source (local video)
                    videoSource2.Start();
                } else
                {
                    if (videoSource2 != null) videoSource2.SignalToStop();
                    videoSourcePlayer.BringToFront();
                    FinalFrame = new VideoCaptureDevice(videoSource[cbListDevices.SelectedIndex].MonikerString);
                    //videoSourcePlayer.NewFrame += new VideoSourcePlayer.NewFrameHandler(this.videoSourcePlayer_NewFrame);
                    videoSourcePlayer.VideoSource = FinalFrame;
                    videoSourcePlayer.Start();
                }
            } else // stop
            {
                videoSourcePlayer.SignalToStop();
                //videoSourcePlayer.SignalToStop();
                if(videoSource2 != null) videoSource2.SignalToStop();
                ((Button)sender).Text = "Start";
            }

        }
        private void video_NewFrame(object sender, Accord.Video.NewFrameEventArgs eventArgs)
        {
            if (Detector1.ProcessFrame(eventArgs.Frame) > motionLevel)
            {
                try
                {
                    Invoke(new Action(() => backgroundBorder.Visible = true));
                }
                catch (Exception ex)
                {
                    System.Environment.Exit(1);
                    //MessageBox.Show(ex.Message, "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                try
                {
                    Invoke(new Action(() => backgroundBorder.Visible = false));
                }
                catch (Exception ex)
                {
                    System.Environment.Exit(1);
                    //MessageBox.Show(ex.Message, "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
            pictureBox1.Image = (Image)eventArgs.Frame.Clone();
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //if (FinalFrame.IsRunning)
            //{
            //    FinalFrame.SignalToStop();
            //}
        }

        private void videoSourcePlayer_NewFrame(object sender, ref Bitmap image)
        {
            if (Detector2.ProcessFrame(image) > motionLevel)
            {
                try
                {
                    Invoke(new Action(() => backgroundBorder.Visible = true));
                }
                catch (Exception ex)
                {
                    System.Environment.Exit(1);
                    //MessageBox.Show(ex.Message, "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                try
                {
                    Invoke(new Action(() => backgroundBorder.Visible = false));
                }
                catch (Exception ex)
                {
                    System.Environment.Exit(1);
                    //MessageBox.Show(ex.Message, "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

        }

        private void btnBrowser_Click(object sender, EventArgs e)
        {
            //videoSourcePlayer.SignalToStop();
            //if (videoSource2 != null)  videoSource2.SignalToStop();
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "All Media Files | *.wav; *.aac; *.wma; *.wmv; *.avi; *.mpg; *.mpeg; *.m1v; *.mp2; *.mp3; *.mpa; *.mpe; *.m3u; *.mp4; *.mov; *.3g2; *.3gp2; *.3gp; *.3gpp; *.m4a; *.cda; *.aif; *.aifc; *.aiff; *.mid; *.midi; *.rmi; *.mkv; *.WAV; *.AAC; *.WMA; *.WMV; *.AVI; *.MPG; *.MPEG; *.M1V; *.MP2; *.MP3; *.MPA; *.MPE; *.M3U; *.MP4; *.MOV; *.3G2; *.3GP2; *.3GP; *.3GPP; *.M4A; *.CDA; *.AIF; *.AIFC; *.AIFF; *.MID; *.MIDI; *.RMI; *.MKV";
            dlg.ShowDialog();
        }

        private void cbListDevices_SelectedIndexChanged(object sender, EventArgs e)
        {
            videoSourcePlayer.SignalToStop();
            if (videoSource2 != null) 
            videoSource2.SignalToStop();
            startButton.Text = "Start";
            //MessageBox.Show(cbListDevices.SelectedIndex.ToString());
            if (cbListDevices.SelectedIndex == cbListDevices.Items.Count - 1) // item browse.. be selected
            {
                SelectVideoFromComputer();
            }
        }
        private void SelectVideoFromComputer()
        {
            try
            {
                using (OpenFileDialog ofd = new OpenFileDialog() { Filter = "MP4| *.mp4", ValidateNames = true, Multiselect = false })
                {
                    if (ofd.ShowDialog() == DialogResult.OK) ;
                    //cbListDevices.SelectedText = ofd.FileName.ToString();
                    //cbListDevices.Items[cbListDevices.SelectedIndex] = ofd.FileName.ToString();
                    if (!cbListDevices.IsHandleCreated)
                        cbListDevices.CreateControl();
                    Invoke(new Action(() => cbListDevices.Text = ofd.FileName.ToString()));
                    filePath = ofd.FileName.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            motionLevel = (float)trackBar1.Value / (float)4000;
            label2.Text =( motionLevel * 100).ToString() + "%";
        }
    }
}
