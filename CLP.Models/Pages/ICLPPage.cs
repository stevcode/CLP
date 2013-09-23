using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Ink;

namespace CLP.Models
{
    public enum GroupSubmitType
    {
        Deny,
        Allow,
        Force
    }

    public enum SubmissionType
    {
        None,
        Single,
        Group
    }

    public interface ICLPPage
    {
        DateTime CreationDate { get; }
        string UniqueID { get; set; }
        string ParentNotebookID { get; set; }

        SubmissionType SubmissionType { get; set; }
        DateTime SubmissionTime { get; set; }
        string SubmissionID { get; set; }
        Person Submitter { get; set; }
        Group GroupSubmitter { get; set; }

        int PageIndex { get; set; } //TODO: Remove as property in Model. Add as viewModel property and use static method to get index of page in property getter.
        int NumberOfSubmissions { get; set; }

        //TODO: rethink these properties. have submissions be attached to page instead of in dictionary on notebook. then use .Count() of submissions list?
        int NumberOfGroupSubmissions { get; set; }
        ObservableCollection<Tag> PageTags { get; }
        GroupSubmitType GroupSubmitType { get; set; }

        double PageHeight { get; set; }
        double PageWidth { get; set; }
        double InitialPageAspectRatio { get; set; }

        Dictionary<string, List<byte>> ImagePool { get; }
        ObservableCollection<StrokeDTO> SerializedStrokes { get; set; }
        StrokeCollection InkStrokes { get; set; }
        bool AddInkWithoutHistory { get; set; }
        ObservableCollection<ICLPPageObject> PageObjects { get; set; }

        CLPHistory PageHistory { get; }

        //NumberOfSubmissions, NumberOfGroupSubmissions

        ICLPPage DuplicatePage();
        ICLPPageObject GetPageObjectByUniqueID(string uniqueID);
        Stroke GetStrokeByStrokeID(string strokeID);
        void TrimPage();
    }
}