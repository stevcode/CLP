﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight.Messaging;
using Classroom_Learning_Partner.ViewModels;
using System.Windows.Controls;

namespace Classroom_Learning_Partner
{
    public static class AppMessages
    {
        enum MessageTypes
        {
            AddPageToDisplay,
            ChangeInkMode
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
    }
}