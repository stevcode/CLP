using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Catel.Data;
using Catel.MVVM;
using CLP.Models;
using NAudio;
using NAudio.Wave;

namespace Classroom_Learning_Partner.ViewModels
{
    public class CLPAudioViewModel : ACLPPageObjectBaseViewModel
    {
        [DllImport("winmm.dll", EntryPoint = "mciSendStringA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        private static extern int mciSendString(string command, StringBuilder returnValue, int returnLength, IntPtr callback);

        //Declarations required for audio out and the MP3 stream
        IWavePlayer waveOutDevice;
        WaveStream mainOutputStream;
        WaveChannel32 volumeStream;

        /// <summary>
        /// Initializes a new instance of the CLPImageViewModel class.
        /// </summary>
        public CLPAudioViewModel(CLPAudio audio)
            : base()
        {
            PageObject = audio;
            waveOutDevice = new WaveOut();
        }

        public override string Title { get { return "AudioVM"; } }

        #region Bindings
        
        public double CurrentSliderValue
        {
            get { return GetValue<double>(CurrentSliderValueProperty); }
            set { SetValue(CurrentSliderValueProperty, value); }
        }

        public static readonly PropertyData CurrentSliderValueProperty = RegisterProperty("CurrentSliderValue", typeof(double));

        #endregion //Bindings

        #region Methods

        Timer audio_play_timer;
        double seconds;
        void audio_play_timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            CurrentSliderValue += (.5) / seconds * 10;
            if(CurrentSliderValue >= 10)
            {
                audio_play_timer.Stop();
                audio_play_timer.Dispose();
                //PlayingAudioVisibility = Visibility.Collapsed;
            }
        }

        private void Play()
        {
            WaveChannel32 inputStream;

            MemoryStream m_stream = new MemoryStream((PageObject as CLPAudio).ByteSource);
            WaveStream mp3Reader = new Mp3FileReader(m_stream);
            inputStream = new WaveChannel32(mp3Reader);
            volumeStream = inputStream;
            mainOutputStream = volumeStream;
            waveOutDevice.Init(mainOutputStream);
            waveOutDevice.Play();
        }

        #endregion //Methods

    }
}