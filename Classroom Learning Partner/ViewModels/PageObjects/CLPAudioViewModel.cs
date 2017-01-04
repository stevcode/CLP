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
using CLP.Entities.Demo;
using NAudio;
using NAudio.Wave;

namespace Classroom_Learning_Partner.ViewModels
{
    public enum AudioState
    {
        Blank,
        Recording,
        HasAudio,
        Playing,
        Paused
    }

    public class CLPAudioViewModel : APageObjectBaseViewModel, IDisposable
    {
        //Declarations required for audio out and the MP3 stream
        private IWavePlayer waveOutDevice;
        private WaveStream mainOutputStream;
        private WaveChannel32 volumeStream;

        private AudioState currentAudioState;

        ///// <summary>
        ///// Initializes a new instance of the CLPImageViewModel class.
        ///// </summary>
        //public CLPAudioViewModel(CLPAudio audio)
        //    : base()
        //{
        //    AudioCommand = new Command(OnAudioCommandExecute);

        //    PageObject = audio;
        //    if(audio.ByteSource.Length == 0)
        //    {
        //        AudioImage = new BitmapImage(new Uri("..\\..\\Images\\mic_start.png", UriKind.Relative));
        //        currentAudioState = AudioState.Blank;
        //    }
        //    else
        //    {
        //        AudioImage = new BitmapImage(new Uri("..\\..\\Images\\play2.png", UriKind.Relative));
        //        currentAudioState = AudioState.HasAudio;
        //    }
        //}

        public override string Title { get { return "AudioVM"; } }

        #region Bindings

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public ImageSource AudioImage
        {
            get { return GetValue<ImageSource>(AudioImageProperty); }
            set { SetValue(AudioImageProperty, value); }
        }

        /// <summary>
        /// Register the AudioImage property so it is known in the class.
        /// </summary>
        public static readonly PropertyData AudioImageProperty = RegisterProperty("AudioImage", typeof(ImageSource), null);
        
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

        private void Record()
        {

        }

        private void Play()
        {
            //waveOutDevice = new WaveOut();

            //MemoryStream m_stream = new MemoryStream((PageObject as CLPAudio).ByteSource);
            //WaveStream mp3Reader = new Mp3FileReader(m_stream);
            //volumeStream = new WaveChannel32(mp3Reader);
            //mainOutputStream = volumeStream;
            //waveOutDevice.Init(mainOutputStream);
            //waveOutDevice.Play();
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

        #region Commands

        /// <summary>
        /// Gets the AudioCommand command.
        /// </summary>
        public Command AudioCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the AudioCommand command is executed.
        /// </summary>
        private void OnAudioCommandExecute()
        {
            switch(currentAudioState)
            {
                case AudioState.Blank:
                    Record();
                    break;
                case AudioState.Recording:
                    break;
                case AudioState.HasAudio:
                    break;
                case AudioState.Playing:
                    break;
                case AudioState.Paused:
                    break;
                default:
                    break;
            }
        }

        #endregion //Commands
    }
}