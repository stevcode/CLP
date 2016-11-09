using System;
using System.Windows;

namespace Classroom_Learning_Partner
{
    public static class UIHelper
    {
        #region Static Methods

        public static void RunOnUI(Action action)
        {
            Application.Current.Dispatcher.BeginInvoke(action, null);
        }

        #endregion // Static Methods
    }
}