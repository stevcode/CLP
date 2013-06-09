using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Classroom_Learning_Partner.ViewModels;
using CLP.Models;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for CLPProofPageView.xaml
    /// </summary>
    public partial class CLPProofPageView
    {
        public CLPProofPageView()
        {
            InitializeComponent();
        }

        

        protected override Type GetViewModelType()
        {
            return typeof(CLPProofPageViewModel);
        }

        protected override void OnViewModelChanged()
        {
            if(ViewModel is CLPProofPageViewModel)
            {
                (ViewModel as CLPProofPageViewModel).TopCanvas = TopCanvas;
                (ViewModel as CLPProofPageViewModel).IsPagePreview = false;
            }
            
            base.OnViewModelChanged();
        }

    }
}
