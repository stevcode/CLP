using System;
using System.Timers;
using System.Windows.Input;
using CLP.Models;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for CLPFuzzyFactorCardRemainder.xaml
    /// </summary>
    public partial class CLPFuzzyFactorCardRemainderView
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="CLPFuzzyFactorCardRemainderView"/> class.
        /// </summary>
        public CLPFuzzyFactorCardRemainderView()
        {
            InitializeComponent();
        }

        protected override Type GetViewModelType()
        {
            return typeof(CLPFuzzyFactorCardRemainderViewModel);
        }
    }
}
