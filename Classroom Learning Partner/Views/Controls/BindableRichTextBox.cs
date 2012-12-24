using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Classroom_Learning_Partner.Views
{
    public class BindableRichTextBox : RichTextBox
    {
        private HashSet<Thread> _recursionProtection = new HashSet<Thread>();

        public BindableRichTextBox()
        {
            this.TextChanged += BindableRTB_TextChanged;
            BorderThickness = new Thickness(0.0);
        }

        void BindableRTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextRange text = new TextRange(this.Document.ContentStart, this.Document.ContentEnd);
            MemoryStream stream = new MemoryStream();
            text.Save(stream, DataFormats.Rtf);

            this.RTFText = Encoding.UTF8.GetString(stream.ToArray());
        }

        public string RTFText
        {
            get { return (string)GetValue(RTFTextProperty); }
            set
            {
                _recursionProtection.Add(Thread.CurrentThread);
                SetValue(RTFTextProperty, value);
                _recursionProtection.Remove(Thread.CurrentThread);
            }
        }

        public static readonly DependencyProperty RTFTextProperty = DependencyProperty.RegisterAttached(
            "RTFText",
            typeof(string),
            typeof(BindableRichTextBox),
            new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, CallBack)
        );

        private static void CallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BindableRichTextBox rtb = d as BindableRichTextBox;

            if(rtb._recursionProtection.Contains(Thread.CurrentThread))
                return;

            try
            {
                string rtfText = e.NewValue as string;
                var doc = new FlowDocument();

                if(rtfText == "")
                {
                    //create rtf version of ""
                    //RichTextBox richTextBox = new RichTextBox();
                    //richTextBox.Text = "Européen";
                    //string rtfFormattedString = richTextBox.Rtf;
                }

                var bytes = Encoding.UTF8.GetBytes(rtfText);
                var stream = new MemoryStream(bytes);

                var range = new TextRange(doc.ContentStart, doc.ContentEnd);
                range.Load(stream, DataFormats.Rtf);

                // Set the document
                rtb.Document = doc;
            }
            catch(Exception)
            {
                rtb.Document = new FlowDocument();
            }
        }
    }
}
