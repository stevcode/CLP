﻿using System;
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
            UpdateFontSize,
            UpdateFontFamily
        }

        //
        public static class UpdateFontSize
        {
            public static void Send(double fontSize)
            {
                Messenger.Default.Send(fontSize, MessageTypes.UpdateFontSize);
            }

            public static void Register(object recipient, Action<double> action)
            {
                Messenger.Default.Register(recipient, MessageTypes.UpdateFontSize, action);
            }
        }

        //
        public static class UpdateFontFamily
        {
            public static void Send(FontFamily font)
            {
                Messenger.Default.Send(font, MessageTypes.UpdateFontFamily);
            }

            public static void Register(object recipient, Action<FontFamily> action)
            {
                Messenger.Default.Register(recipient, MessageTypes.UpdateFontFamily, action);
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

        public static class UpdateCLPHistory
        {
            public static void Send(CLPHistoryItem item)
            {
                Messenger.Default.Send(item, MessageTypes.UpdateCLPHistory);
            }

            public static void Register(object recipient, Action<CLPHistoryItem> action)
            {
                Messenger.Default.Register(recipient, MessageTypes.UpdateCLPHistory, action);
            }
        }

        public static class SetLaserPointerMode
        {
            //do we need to set boolean? when we click a diff pen input on the ribbon, what exactly happens?
            public static void Send(bool set)
            {
                Messenger.Default.Send(set, MessageTypes.SetLaserPointerMode);
            }

            //what exactly are these arguments?
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

            public static void Register(object recipient, Action<bool> action)
            {
                Messenger.Default.Register(recipient, MessageTypes.UpdateLaserPointerPosition, action);
            }
        }


    }
}
