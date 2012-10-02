using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Catel.Data;
using Catel.MVVM;
using CLP.Models;

namespace Classroom_Learning_Partner.ViewModels
{
    public class CLPAudioViewModel : ACLPPageObjectBaseViewModel
    {
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

        #endregion //Methods

    }
}