using Classroom_Learning_Partner.Model.CLPPageObjects;
using System.Windows.Media;
using System.IO;
using Catel.Data;
using Catel.MVVM;
using System;
using System.Windows;
using System.Timers;
using System.Runtime.InteropServices;
using Classroom_Learning_Partner.ViewModels.Workspaces;
using Classroom_Learning_Partner.Model;
using Classroom_Learning_Partner.ViewModels.Displays;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using System.Windows.Threading;

namespace Classroom_Learning_Partner.ViewModels.PageObjects
{
    public class CLPAudioViewModel : CLPPageObjectBaseViewModel
    {
        [DllImport("winmm.dll", EntryPoint = "mciSendStringA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        private static extern int mciSendString(string lpstrCommand, string lpstrReturnString, int uReturnLength, int hwndCallback);
            
            /// <summary>
            /// Initializes a new instance of the CLPImageViewModel class.
            /// </summary>
            public CLPAudioViewModel(CLPAudio audio)
                : base()
            {
                PageObject = audio;
                AudioPlayImage = new BitmapImage(new Uri("..\\..\\Images\\play2.png", UriKind.Relative));
               
                AudioRecordImage = new BitmapImage(new Uri("..\\..\\Images\\mic_start.png", UriKind.Relative));
               
                PlayingAudioVisibility = Visibility.Collapsed;

                RecordAudioCommand = new Command(OnRecordAudioCommandExecute);
                PlayAudioCommand = new Command(OnPlayAudioCommandExecute);

                PageHasAudioFile = false;
                this.path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Audio_Files\" + audio.ID + ".mp3";
                if(File.Exists(this.path) || (PageObject as CLPAudio).File.Length > 1)
                {
                    PageHasAudioFile = true;
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
              (DispatcherOperationCallback)delegate(object arg)
              {
                  AudioRecordImage = new BitmapImage(new Uri("..\\..\\Images\\play2.png", UriKind.Relative));
                  return null;
              }, null);
                   
                }
            }
            string path;
            public override string Title { get { return "AudioVM"; } }
            #region properties
            public static readonly PropertyData CurrentSliderValueProperty = RegisterProperty("CurrentSliderValue", typeof(double));
            public double CurrentSliderValue
            {
                get { return GetValue<double>(CurrentSliderValueProperty); }
                set { SetValue(CurrentSliderValueProperty, value); }
            }

            public static readonly PropertyData PlayingAudioVisibilityProperty = RegisterProperty("PlayingAudioVisibility", typeof(Visibility));
            public Visibility PlayingAudioVisibility
            {
                get { return GetValue<Visibility>(PlayingAudioVisibilityProperty); }
                set { SetValue(PlayingAudioVisibilityProperty, value); }
            }
            public bool PageHasAudioFile
            {
                get { return GetValue<bool>(PageHasAudioFileProperty); }
                set { SetValue(PageHasAudioFileProperty, value); }
            }
            public static readonly PropertyData PageHasAudioFileProperty = RegisterProperty("PageHasAudioFile", typeof(bool));

            #endregion
            #region Binding

            /// <summary>
            /// Gets or sets the property value.
            /// </summary>
            [ViewModelToModel("PageObject")]
            public string FilePath
            {
                get { return GetValue<string>(FilePathProperty); }
                set { SetValue(FilePathProperty, value); }
            }
            public static readonly PropertyData FilePathProperty = RegisterProperty("FilePath", typeof(string));

            public BitmapImage AudioRecordImage
            {
                get { return GetValue<BitmapImage>(AudioRecordImageProperty); }
                set { SetValue(AudioRecordImageProperty, value); }
            }
            public static readonly PropertyData AudioRecordImageProperty = RegisterProperty("AudioRecordImage", typeof(BitmapImage));

            public BitmapImage AudioPlayImage
            {
                get { return GetValue<BitmapImage>(AudioPlayImageProperty); }
                set { SetValue(AudioPlayImageProperty, value); }
            }
            public static readonly PropertyData AudioPlayImageProperty = RegisterProperty("AudioPlayImage", typeof(BitmapImage));

            #endregion //Binding

        #region methods
            string fullPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Audio_Files\tempAudio.wav";
            
            public void recordAudio()
            {

                try
                {
                    if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Audio_Files"))
                    {
                        DirectoryInfo worked = Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Audio_Files\");
                    }
                    if (File.Exists(fullPath))
                    {
                        File.Delete(fullPath);
                    }
                    mciSendString("open new Type waveaudio Alias recsound", "", 0, 0);
                    mciSendString("record recsound", "", 0, 0);

                    
                }
                catch (Exception e)
                {

                }
            }
            public void stopAudio()
            {
                try
                {

                    mciSendString("save recsound " + fullPath, "", 0, 0);
                    mciSendString("close recsound ", "", 0, 0);
                    
                    CLPAudio.ConvertWavMP3(fullPath, fullPath.Replace(".wav", ".mp3"));
                    //CLPPage page = ((App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).DisplayedPage.Page;
                    (this.PageObject as CLPAudio).File = CLPAudio.FileToByteArray(fullPath.Replace(".wav", ".mp3"));
                    
                }
                catch (Exception e)
                {
                }
                
            }
            //System.Media.SoundPlayer soundPlayer;
            System.Timers.Timer audio_play_timer;
            double seconds;
            void audio_play_timer_Elapsed(object sender, ElapsedEventArgs e)
            {
                CurrentSliderValue += (.5) / seconds * 10;
               // Console.WriteLine("Slider Position: " + CurrentSliderValue);
                if (CurrentSliderValue >= 10)
                {
                    audio_play_timer.Stop();
                    audio_play_timer.Dispose();
                    PlayingAudioVisibility = Visibility.Collapsed;
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
              (DispatcherOperationCallback)delegate(object arg)
              {
                  AudioRecordImage = new BitmapImage(new Uri("..\\..\\Images\\play2.png", UriKind.Relative));
                  return null;
              }, null);
                }
            }
            MediaPlayer wplayer;
            public void playAudio()
            {
                try
                {
                    if (!File.Exists(path))
                    {
                        System.IO.File.WriteAllBytes(path, (PageObject as CLPAudio).File);
                    }
                    //get the size if the file to calculate the duration so we can show a progress bar
                    FileInfo file = new FileInfo(path);
                    long sizeKb = file.Length / (long)1024.0;
                    //seconds = (Double)sizeKb / 11.0; //value for wav
                    seconds = (Double)sizeKb / 1.6; //value for mp3 has to be changed based on the bitrate we encode at
                    TimeSpan audioLength = new TimeSpan(0, 0, (int)seconds);
                    //initialize the timer
                    audio_play_timer = new System.Timers.Timer();
                    audio_play_timer.Interval = 500;
                    audio_play_timer.Elapsed += new ElapsedEventHandler(audio_play_timer_Elapsed);
                    audio_play_timer.Enabled = true;
                    
                    wplayer = new MediaPlayer();
                    wplayer.Open(new Uri(path));
                    wplayer.Play();
                    
                }
                catch (Exception e)
                {
                }

                ////\\\\ old way that works statically
                //string s;

                //// access media file
                //s = "open \"C:\\Users\\Claire\\"+path+" type waveaudio alias mysound";

                //mciSendString("open "+path+" type waveaudio alias mysound", null, 0, 0);

                //// play from start
                //s = "play mysound from 0 wait";     // append "wait" if you want blocking
                //mciSendString(s, null, 0, 0);
                ////System.Threading.Thread.Sleep(2000);
                //// stop playback
                //s = "stop mysound";         // if playing asynchronously
                //mciSendString(s, null, 0, 0);

                //// deallocate resources
                //s = "close mysound";
                //mciSendString(s, null, 0, 0);

            }

            public void stopAudioPlayback()
            {
                try
                {
                    if (wplayer != null)
                    {
                        wplayer.Stop();
                    }
                    audio_play_timer.Stop();
                    audio_play_timer.Dispose();
                    CurrentSliderValue = 0;
                    PlayingAudioVisibility = Visibility.Collapsed;
                }
                catch (Exception e)
                {

                }
            }

            public Command RecordAudioCommand { get; private set; }

            /// <summary>
            /// Method to invoke when the RecordAudioCommand command is executed.
            /// </summary>
            public bool isRecordingAudio = false;
            public Timer record_timer = null;
            public void OnRecordAudioCommandExecute()
            {

                CLPPage page = ((App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).DisplayedPage.Page;
                if (!isRecordingAudio && !PageHasAudioFile)
                {
                    //AudioRecordImage = new Uri("..\\Images\\mic_stop.png", UriKind.Relative);
                    PageHasAudioFile = true;
                    isRecordingAudio = true;
                    record_timer = new Timer();
                    record_timer.Elapsed += new ElapsedEventHandler(record_timer_Elapsed);
                    record_timer.Interval = 500;
                    record_timer.Enabled = true;
                    record_timer.Start();
                    recordAudio();
                }
                else if(isRecordingAudio)
                {
                    
                    stopAudio();
                    isRecordingAudio = false;
                    try
                    {
                        record_timer.Stop();
                        record_timer.Dispose();
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
              (DispatcherOperationCallback)delegate(object arg)
              {
                  AudioRecordImage = new BitmapImage(new Uri("..\\..\\Images\\play2.png", UriKind.Relative));
                  return null;
              }, null);
                    }
                    catch (Exception e)
                    { }
                }
                else if (PageHasAudioFile)
                {
                    PlayAudioCommand.Execute();
                }
            }
            bool flash = true;
            void record_timer_Elapsed(object sender, ElapsedEventArgs e)
            {
                if (flash)
                {
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (DispatcherOperationCallback)delegate(object arg)
                {
                    AudioRecordImage = new BitmapImage(new Uri("..\\..\\Images\\recordflash1.png", UriKind.Relative));
                 return null;}, null);
                }
                else
                {
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (DispatcherOperationCallback)delegate(object arg)
                {
                    AudioRecordImage = new BitmapImage(new Uri("..\\..\\Images\\recordflash2.png", UriKind.Relative));
                    return null;
                }, null);
                }

                flash = !flash;
            }
            /// <summary>
            /// Gets the PlayAudioCommand command.
            /// </summary>
            public Command PlayAudioCommand { get; private set; }

            /// <summary>
            /// Method to invoke when the PlayAudioCommand command is executed.
            /// </summary>
            bool playingAudio = false;

            private void OnPlayAudioCommandExecute()
            {
                CLPPage page = ((App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).DisplayedPage.Page;
                if (!playingAudio)
                {
                    try
                    {
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (DispatcherOperationCallback)delegate(object arg)
              {
                 // AudioPlayImage = new BitmapImage(new Uri("..\\..\\Images\\stop.png", UriKind.Relative));
                  AudioRecordImage = new BitmapImage(new Uri("..\\..\\Images\\stop.png", UriKind.Relative));
                  return null;
              }, null);
                        
                        //show the slider
                        PlayingAudioVisibility = Visibility.Visible;
                        //start the slider
                        CurrentSliderValue = 0;
                        playAudio();
                        playingAudio = true;
                    }
                    catch (Exception e) { }
                }
                else
                {
                    try
                    {
                        PlayingAudioVisibility = Visibility.Collapsed;
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (DispatcherOperationCallback)delegate(object arg)
                        {
                            //AudioPlayImage = new BitmapImage(new Uri("..\\..\\Images\\play2.png", UriKind.Relative));
                            AudioRecordImage = new BitmapImage(new Uri("..\\..\\Images\\play2.png", UriKind.Relative));
                            
                            return null;
                        }, null);
                        
                        stopAudioPlayback();
                        playingAudio = false;
                    }
                    catch (Exception e) { }
                }

            }
        #endregion

    }
    }



