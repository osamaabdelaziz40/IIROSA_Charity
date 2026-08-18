// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Enums.cs" company="Usama Nada">
//   No Copyright .. Copy, Share, and Evolve.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Framework.Core
{
    /// <summary>
    /// The calendar type.
    /// </summary>
    public enum CalendarType
    {
        /// <summary>
        ///     The ummalqura.
        /// </summary>
        ummalqura,

        /// <summary>
        ///     The gregorian.
        /// </summary>
        gregorian
    }

    public enum ListViewMode
    {
        Grid,
        Cards,
        Calender
    }

    public enum ControlDirections
    {
        Left,
        Right,
        None
    }

    public enum ViewModes
    {
        New,
        Edit,
        View,
        Display,
        Review,
        Search,
        Returned
    }

    public enum SaveModes
    {
        Draft,
        Submit
    }

    /// <summary>
    ///     The number type.
    /// </summary>
    public enum NumberType
    {
        /// <summary>
        ///     The fixe d_ line.
        /// </summary>
        FIXED_LINE,

        /// <summary>
        ///     The mobile.
        /// </summary>
        MOBILE,

        /// <summary>
        ///     The fixe d_ lin e_ o r_ mobile.
        /// </summary>
        FIXED_LINE_OR_MOBILE,

        /// <summary>
        ///     The tol l_ free.
        /// </summary>
        TOLL_FREE,

        /// <summary>
        ///     The premiu m_ rate.
        /// </summary>
        PREMIUM_RATE,

        /// <summary>
        ///     The share d_ cost.
        /// </summary>
        SHARED_COST,

        /// <summary>
        ///     The voip.
        /// </summary>
        VOIP,

        /// <summary>
        ///     The persona l_ number.
        /// </summary>
        PERSONAL_NUMBER,

        /// <summary>
        ///     The pager.
        /// </summary>
        PAGER,

        /// <summary>
        ///     The uan.
        /// </summary>
        UAN,

        /// <summary>
        ///     The voicemail.
        /// </summary>
        VOICEMAIL,

        /// <summary>
        ///     The unknown.
        /// </summary>
        UNKNOWN
    }

    /// <summary>
    /// The check list item type.
    /// </summary>
    public enum CheckListItemType
    {
        /// <summary>
        /// The sector.
        /// </summary>
        Sector,

        /// <summary>
        /// The sub sector.
        /// </summary>
        SubSector,

        /// <summary>
        /// The exam.
        /// </summary>
        Exam,

        /// <summary>
        /// The course.
        /// </summary>
        Course,

        /// <summary>
        /// The competency.
        /// </summary>
        Competency,

        /// <summary>
        /// The competency element.
        /// </summary>
        CompetencyElement,

        /// <summary>
        /// The performance criteria.
        /// </summary>
        PerformanceCriteria,

        /// <summary>
        /// The assessment judgment.
        /// </summary>
        AssessmentJudgment,

        /// <summary>
        /// The expert.
        /// </summary>
        Expert,

        /// <summary>
        /// The folder.
        /// </summary>
        Folder,

        /// <summary>
        /// The sub folder.
        /// </summary>
        SubFolder,

        /// <summary>
        /// The period.
        /// </summary>
        Period,

        /// <summary>
        /// The organization.
        /// </summary>
        Organization,

        /// <summary>
        /// The event
        /// </summary>
        Event,

        /// <summary>
        /// The exam profile.
        /// </summary>
        ExamProfile,
    }

    public enum FormatEnum
    {
        Json = 1,
        xlsx = 2,
        Pdf = 3,
        Word = 4,
        Html = 5,
        Xml = 6
    }

    public enum FormatType
    {
        A4,
        A3,
        Letter
    }

    public enum OrientationEnum
    {
        Portrait = 0,
        Landscape = 1
    }

    public enum DateFormatType
    {
        yyyyMMdd,
        ddMMyyyy
    }
}