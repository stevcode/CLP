using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight.Messaging;
using Classroom_Learning_Partner.ViewModels;
using Classroom_Learning_Partner.Model;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;


namespace Classroom_Learning_Partner
{
    public static class AppMessages
    {
        enum MessageTypes
        {
            AddPageToDisplay,
            ChangeInkMode,
            UpdateCLPHistory, 
	        SetLaserPointerMode,
	        UpdateLaserPointerPosition,
            TurnOffLaser,
            UpdateFont,
            ChangePlayback,
            SendPlaybackItem,
            Audio,
            SetSnapTileMode
        }

        public static class UpdateFont
        {
            public static void Send(double fontSize, FontFamily font, Brush fontColor)
            {
                var t = Tuple.Create<double, FontFamily, Brush>(fontSize, font, fontColor);
                Messenger.Default.Send(t, MessageTypes.UpdateFont);
            }

            public static void Register(object recipient, Action<Tuple<double, FontFamily, Brush>> action)
            {
                Messenger.Default.Register(recipient, MessageTypes.UpdateFont, action);
            }
        }

        public static class ChangeInkMode
        {
            public static void Send(InkCanvasEditingMode inkMode)
            {
                Messenger.Default.Send<InkCanvasEditingMode>(inkMode);
            }

            public static void Register(object recipient, Action<InkCanvasEditingMode> action)
            {
                Messenger.Default.Register<InkCanvasEditingMode>(recipient, action);
            }
        }

        public static class AddPageToDisplay
        {
            public static void Send(CLPPageViewModel pageVM)
            {
                Messenger.Default.Send(pageVM, MessageTypes.AddPageToDisplay);
            }

            public static void Register(object recipient, Action<CLPPageViewModel> action)
            {
                Messenger.Default.Register(recipient, MessageTypes.AddPageToDisplay, action);
            }
        }

        public static class RequestCurrentDisplayedPage
        {
            public static void Send(Action<CLPPageViewModel> callback)
            {
                var message = new NotificationMessageAction<CLPPageViewModel>("", callback);
                Messenger.Default.Send(message);
            }

            public static void Register(object recipient, Action<NotificationMessageAction<CLPPageViewModel>> action)
            {
                Messenger.Default.Register<NotificationMessageAction<CLPPageViewModel>>(recipient, action);
            }
        }
        //UpdateCLPHistory Messages are used to send a new HistoryItem to the HistoryVM
        public static class UpdateCLPHistory
        {
            public static void Send(List<object> item)
            {
                Messenger.Default.Send(item, MessageTypes.UpdateCLPHistory);
            }

            public static void Register(object recipient, Action<List<object>> action)
            {
                Messenger.Default.Register(recipient, MessageTypes.UpdateCLPHistory, action);
            }
        }
        
        public static class ChangePlayback
        {
            public static void Send(bool item)
            {
                Messenger.Default.Send(item, MessageTypes.ChangePlayback);
            }

            public static void Register(object recipient, Action<bool> action)
            {
                Messenger.Default.Register(recipient, MessageTypes.ChangePlayback, action);
            }
        }
        public static class SendPlaybackItem
        {
            public static void Send(CLPHistoryItem item)
            {
                Messenger.Default.Send(item, MessageTypes.SendPlaybackItem);
            }

            public static void Register(object recipient, Action<CLPHistoryItem> action)
            {
                Messenger.Default.Register(recipient, MessageTypes.SendPlaybackItem, action);
            }
        }
        public static class Audio
        {
            public static void Send(String item)
            {
                Messenger.Default.Send(item, MessageTypes.Audio);
            }

            public static void Register(object recipient, Action<String> action)
            {
                Messenger.Default.Register(recipient, MessageTypes.Audio, action);
            }
        }
        public static class SetLaserPointerMode
        {
            public static void Send(bool set)
            {
                Messenger.Default.Send(set, MessageTypes.SetLaserPointerMode);
            }

            public static void Register(object recipient, Action<bool> action)
            {
                Messenger.Default.Register(recipient, MessageTypes.SetLaserPointerMode, action);
            }
        }

        public static class UpdateLaserPointerPosition
        {
            public static void Send(Point pt)
            {
                Messenger.Default.Send(pt, MessageTypes.UpdateLaserPointerPosition);
            }

            public static void Register(object recipient, Action<Point> action)
            {
                Messenger.Default.Register(recipient, MessageTypes.UpdateLaserPointerPosition, action);
            }
        }

        public static class TurnOffLaser
        {
            public static void Send()
            {
                Messenger.Default.Send(MessageTypes.TurnOffLaser);
            }

            public static void Register(object recipient, Action<Point> action)
            {
                Messenger.Default.Register(recipient, MessageTypes.TurnOffLaser, action);
            }
        }

        public static class SetSnapTileMode
        {
            public static void Send(bool snapTileOnOff)
            {
                Messenger.Default.Send(snapTileOnOff, MessageTypes.SetSnapTileMode);
            }

            public static void Register(object recipient, Action<bool> action)
            {
                Messenger.Default.Register(recipient, MessageTypes.SetSnapTileMode, action);
            }
        }
    }
}
