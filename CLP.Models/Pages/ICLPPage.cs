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

        ObservableCollection<Tag> PageTags { get; }
        GroupSubmitType GroupSubmitType { get; set; }

        double PageHeight { get; set; }
        double PageWidth { get; set; }
        double InitialPageAspectRatio { get; set; }

        Dictionary<string, List<byte>> ImagePool { get; }
        ObservableCollection<StrokeDTO> SerializedStrokes { get; set; }
        StrokeCollection InkStrokes { get; set; }
        ObservableCollection<ICLPPageObject> PageObjects { get; set; }

        CLPHistory PageHistory { get; }

        //PageIndex, NumberOfSubmissions, NumberOfGroupSubmissions?, PageTopics

        ICLPPage DuplicatePage();
        void TrimPage();
    }
}
