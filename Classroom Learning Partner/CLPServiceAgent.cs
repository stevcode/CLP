using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Catel.Data;
using Classroom_Learning_Partner.ViewModels;
using Classroom_Learning_Partner.Views.Modal_Windows;
using CLP.Entities;
using Catel.Collections;
using Catel.MVVM;
using Catel.MVVM.Views;
using Catel.IoC;
using Catel.Windows.Controls;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Security.Cryptography;
using System.Runtime.Serialization;

namespace Classroom_Learning_Partner
{
    //Sealed to allow the compiler to perform special optimizations during JIT
    public sealed class CLPServiceAgent
    {
        private CLPServiceAgent()
        {
            //_autoSaveTimer.Interval = 123000;
            //_autoSaveTimer.Elapsed += _autoSaveTimer_Elapsed;
        }

        //readonly allows thread-safety and means it can only be allocated once.
        private static readonly CLPServiceAgent _instance = new CLPServiceAgent();
        public static CLPServiceAgent Instance { get { return _instance; } }

        public void Initialize()
        {
        }

        public void Exit()
        {
            //ask to save notebooks, large window with checks for all notebooks (possibly also converter?)
            //sync with database
            //run network disconnect
            //_autoSaveTimer.Stop();
            
            Environment.Exit(0);
        }

        #region Network

        private Thread _networkThread;

        public void NetworkSetup()
        {
            _networkThread = new Thread(App.Network.Run) { IsBackground = true };
            _networkThread.Start();
        }

        public void NetworkReconnect()
        {
            App.Network.Stop();
            _networkThread.Join();
            _networkThread = null;

            App.Network.Dispose();
            App.Network = null;
            App.Network = new CLPNetwork();
            _networkThread = new Thread(App.Network.Run) { IsBackground = true };
            _networkThread.Start();
        }

        public void NetworkDisconnect()
        {
            App.Network.Stop();
            _networkThread.Join();
        }

        #endregion //Network
    }
}