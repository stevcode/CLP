using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using Classroom_Learning_Partner.Model.CLPPageObjects;
using GalaSoft.MvvmLight.Command;
using Classroom_Learning_Partner.Model;

namespace Classroom_Learning_Partner.ViewModels.PageObjects
{
    class CLPAnimationViewModel : CLPPageObjectBaseViewModel
    {
        public CLPAnimationViewModel(CLPAnimation animation, CLPPageViewModel pageViewModel)
            : base(pageViewModel)
        {
            PageObject = animation;
            CLPService = new CLPServiceAgent();
        }
        private CLPServiceAgent CLPService;
        public const string PlaybackImagePropertyName = "PlaybackImage";
        private Uri _playbackImage = new Uri("..\\..\\Images\\play_green.png", UriKind.Relative);
        public Uri PlaybackImage
        {
            get
            {
                return _playbackImage;
            }
            set
            {
                _playbackImage = value;

                RaisePropertyChanged("PlaybackImage");
            }
        }
        public const string RecordImagePropertyName = "RecordImage";
        private Uri _recordImage = new Uri("..\\..\\Images\\record.png", UriKind.Relative);
        public Uri RecordImage
        {
            get
            {
                return _recordImage;
            }
            set
            {
                _recordImage = value;

                RaisePropertyChanged("RecordImage");
            }
        }
        private bool _recording = false;
        private bool Recording
        {
            get
            {
                return _recording;

            }
            set
            {
                _recording = value;

            }
        }
        private RelayCommand _startRecordCommand;
        public RelayCommand StartRecordCommand
        {
            get
            {
                return _startRecordCommand
                    ?? (_startRecordCommand = new RelayCommand(
                                          () =>
                                          {
                                              CLPHistoryItem item;
                                              Console.WriteLine("START Record COMMAND to create history item");
                                              if (!Recording)
                                              {
                                                   item = new CLPHistoryItem("START_RECORD");
                                                   CLPService.NewHistoryItem(item);
                                                   RecordImage = new Uri("..\\..\\Images\\stop_record.png", UriKind.Relative);
                                                   Recording = true;
                                              }
                                              else
                                              {
                                                   item = new CLPHistoryItem("STOP_RECORD");
                                                   CLPService.NewHistoryItem(item);
                                                   RecordImage = new Uri("..\\..\\Images\\record.png", UriKind.Relative);
                                                   Recording = false;
                                              }
                                              
 
                                          }));
            }
        }
        private delegate void NoArgDelegate();
        private CLPPageViewModel pageVM;
        private RelayCommand _startPlaybackCommand;
        public RelayCommand StartPlaybackCommand
        {
            get
            {
                return _startPlaybackCommand

                    ?? (_startPlaybackCommand = new RelayCommand(
                                          () =>
                                          {
                                              if (!Recording)
                                              {
                                                  PlaybackImage = new Uri("..\\..\\Images\\pause_blue.png", UriKind.Relative);
                                                  AppMessages.RequestCurrentDisplayedPage.Send((pageViewModel) =>
                                                  {
                                                      pageVM = pageViewModel;
                                                  });
                                                  pageVM.HistoryVM.Recording = true;
                                                  NoArgDelegate fetcher = new NoArgDelegate(this.pageVM.HistoryVM.startPlayback);
                                                  fetcher.BeginInvoke(null, null);
                                                  
                                                  //PlaybackImage = new Uri("..\\..\\Images\\play_green.png", UriKind.Relative);

                                              }

                                          }));
            }
        }

    }
}
