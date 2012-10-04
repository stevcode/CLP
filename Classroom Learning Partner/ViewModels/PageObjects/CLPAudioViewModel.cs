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
    public class CLPAudioViewModel : ACLPPageObjectBaseViewModel, IDisposable
    {
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
            waveOutDevice = new WaveOut();

            MemoryStream m_stream = new MemoryStream((PageObject as CLPAudio).ByteSource);
            WaveStream mp3Reader = new Mp3FileReader(m_stream);
            volumeStream = new WaveChannel32(mp3Reader);
            mainOutputStream = volumeStream;
            waveOutDevice.Init(mainOutputStream);
            waveOutDevice.Play();

            
        }

        #endregion //Methods


        public void Dispose()
        {
            CloseWaveOut();
        }

        private void CloseWaveOut()
        {
            if(waveOutDevice != null)
            {
                waveOutDevice.Stop();
            }
            if(mainOutputStream != null)
            {
                // this one really closes the file and ACM conversion
                volumeStream.Close();
                volumeStream = null;
                // this one does the metering stream
                mainOutputStream.Close();
                mainOutputStream = null;
            }
            if(waveOutDevice != null)
            {
                waveOutDevice.Dispose();
                waveOutDevice = null;
            }
        }
    }
}