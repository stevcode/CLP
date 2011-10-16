using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight.Messaging;

namespace Classroom_Learning_Partner
{
    public static class AppMessages
    {
        enum MessageTypes
        {
            SelectNotebook
        }

        public static class SelectNotebookMessage
        {
            public static void Send(string notebookName)
            {
                Messenger.Default.Send(notebookName, MessageTypes.SelectNotebook);
            }

            public static void Register(object recipient, Action<string> action)
            {
                Messenger.Default.Register(recipient, MessageTypes.SelectNotebook, action);
            }
        }


    }
}
