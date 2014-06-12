﻿using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using Catel.Data;
using Catel.MVVM;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class ExitTicketCreationViewModel : ViewModelBase
    {

        public ExitTicketCreationViewModel()
        {
            GroupCreationViewModel = new GroupCreationViewModel();
            BasePage = new CLPPage();

            foreach (Group group in GroupCreationViewModel.Groups)
            {
                ExitTickets.Add(DifferentiatePage(BasePage, group.Label));
            }

            GroupCreationViewModel.Groups.CollectionChanged += Groups_CollectionChanged;
        }

        void Groups_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if(e.NewItems != null)
            {
                foreach(Group newGroup in e.NewItems)
                {
                    ExitTickets.Insert(e.NewStartingIndex, DifferentiatePage(BasePage, newGroup.Label));
                }
            }

            if(e.OldItems != null)
            {
                ExitTickets.RemoveAt(e.OldStartingIndex);
            }
        }

        public CLPPage BasePage
        {
            get { return GetValue<CLPPage>(BasePageProperty); }
            set { SetValue(BasePageProperty, value); }
        }

        public static readonly PropertyData BasePageProperty = RegisterProperty("BasePage", typeof(CLPPage), () => new CLPPage());

        public GroupCreationViewModel GroupCreationViewModel
        {
            get { return GetValue<GroupCreationViewModel>(GroupCreationViewModelProperty); }
            set { SetValue(GroupCreationViewModelProperty, value); }
        }

        public static readonly PropertyData GroupCreationViewModelProperty = RegisterProperty("GroupCreationViewModel", typeof(GroupCreationViewModel), () => new GroupCreationViewModel());

        public ObservableCollection<CLPPage> ExitTickets
        {
            get { return GetValue<ObservableCollection<CLPPage>>(ExitTicketsProperty); }
            set { SetValue(ExitTicketsProperty, value); }
        }

        public static readonly PropertyData ExitTicketsProperty = RegisterProperty("ExitTickets", typeof(ObservableCollection<CLPPage>), () => new ObservableCollection<CLPPage>());
        
        private static CLPPage DifferentiatePage(CLPPage original, string label)
        {
            CLPPage differentiatedPage = original.DuplicatePage();
            differentiatedPage.ID = original.ID;
            differentiatedPage.PageNumber = 999;
            differentiatedPage.DifferentiationLevel = label;
            foreach(var pageObject in differentiatedPage.PageObjects)
            {
                pageObject.DifferentiationLevel = differentiatedPage.DifferentiationLevel;
            }
            foreach(var historyItem in differentiatedPage.History.UndoItems)
            {
                historyItem.DifferentiationGroup = differentiatedPage.DifferentiationLevel;
            }
            foreach(var historyItem in differentiatedPage.History.RedoItems)
            {
                historyItem.DifferentiationGroup = differentiatedPage.DifferentiationLevel;
            }
            foreach(var stroke in differentiatedPage.InkStrokes)
            {
                stroke.SetStrokeDifferentiationGroup(differentiatedPage.DifferentiationLevel);
            }
            return differentiatedPage;
        }
    }
}