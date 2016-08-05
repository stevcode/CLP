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

        #region Utilities


        public byte[] UIElementToImageByteArray(UIElement source, double imageWidth = -1.0, double scale = 1.0, double dpi = 96, BitmapEncoder encoder = null)
        {
            const double SCREEN_DPI = 96.0;
            var actualWidth = imageWidth <= 0.0 ? source.RenderSize.Width : imageWidth;
            var actualHeight = actualWidth * source.RenderSize.Height / source.RenderSize.Width;

            var renderWidth = actualWidth * scale * dpi / SCREEN_DPI;
            var renderHeight = actualHeight * scale * dpi / SCREEN_DPI;

            var renderTarget = new RenderTargetBitmap((int)renderWidth, (int)renderHeight, dpi, dpi, PixelFormats.Pbgra32);
            var sourceBrush = new VisualBrush(source);

            var drawingVisual = new DrawingVisual();
            var drawingContext = drawingVisual.RenderOpen();

            using(drawingContext)
            {
                drawingContext.PushTransform(new ScaleTransform(scale, scale));
                drawingContext.DrawRectangle(sourceBrush, null, new Rect(new Point(0, 0), new Point(actualWidth, actualHeight)));
            }
            renderTarget.Render(drawingVisual);

            byte[] imageArray;
            if(encoder == null)
            {
                encoder = new PngBitmapEncoder();
            }

            encoder.Frames.Add(BitmapFrame.Create(renderTarget));
            using(var outputStream = new MemoryStream())
            {
                encoder.Save(outputStream);
                imageArray = outputStream.ToArray();
            }

            return imageArray;
        }

        
        

        /// <summary>
        /// Compresses byte array to new byte array.
        /// </summary>
        public byte[] Compress(byte[] raw)
        {
            using(var memory = new MemoryStream())
            {
                using(var gzip = new GZipStream(memory, CompressionMode.Compress, true))
                {
                    gzip.Write(raw, 0, raw.Length);
                }
                return memory.ToArray();
            }
        }

        public byte[] Decompress(byte[] gzip)
        {
            using(var stream = new GZipStream(new MemoryStream(gzip), CompressionMode.Decompress))
            {
                const int SIZE = 4096;
                var buffer = new byte[SIZE];
                using(var memory = new MemoryStream())
                {
                    int count;
                    do
                    {
                        count = stream.Read(buffer, 0, SIZE);
                        if(count > 0)
                        {
                            memory.Write(buffer, 0, count);
                        }
                    }
                    while(count > 0);
                    return memory.ToArray();
                }
            }
        }

        public string Zip(string text)
        {
            var buffer = System.Text.Encoding.Unicode.GetBytes(text);
            var ms = new MemoryStream();
            using(var zip = new GZipStream(ms, CompressionMode.Compress, true))
            {
                zip.Write(buffer, 0, buffer.Length);
            }

            ms.Position = 0;
            var outStream = new MemoryStream();

            var compressed = new byte[ms.Length];
            ms.Read(compressed, 0, compressed.Length);

            var gzBuffer = new byte[compressed.Length + 4];
            Buffer.BlockCopy(compressed, 0, gzBuffer, 4, compressed.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gzBuffer, 0, 4);
            return Convert.ToBase64String(gzBuffer);
        }

        public string UnZip(string compressedText)
        {
            var gzBuffer = Convert.FromBase64String(compressedText);
            using(var ms = new MemoryStream())
            {
                var msgLength = BitConverter.ToInt32(gzBuffer, 0);
                ms.Write(gzBuffer, 4, gzBuffer.Length - 4);

                var buffer = new byte[msgLength];

                ms.Position = 0;
                using(var zip = new GZipStream(ms, CompressionMode.Decompress))
                {
                    zip.Read(buffer, 0, buffer.Length);
                }

                return System.Text.Encoding.Unicode.GetString(buffer, 0, buffer.Length);
            }
        }

        public string Checksum(object obj)
        {
            // This may only work on objects with [Serializable]. -Casey
            DataContractSerializer serializer = new DataContractSerializer(obj.GetType());
            using (MemoryStream memoryStream = new MemoryStream())
            {
                serializer.WriteObject(memoryStream, obj);
                MD5CryptoServiceProvider hasher = new MD5CryptoServiceProvider();
                hasher.ComputeHash(memoryStream.ToArray());
                return Convert.ToBase64String(hasher.Hash);
            }
        }

        #endregion //Utilies

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