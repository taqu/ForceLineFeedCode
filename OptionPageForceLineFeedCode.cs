using System;
using System.ComponentModel;

namespace ForceLineFeedCode
{
    public class OptionPageForceLineFeedCode : Microsoft.VisualStudio.Shell.DialogPage
    {

        public enum TypeLineFeed
        {
            LF =0,
            CR,
            CRLF,
        };

        public enum TypeLanguage
        {
            [Description("C/C++")]
            C_Cpp,
            [Description("CSharp")]
            CSharp,
            [Description("Others")]
            Others,
        };

        //public enum TypeEncoding
        //{
        //    [Description("UTF-16(LE)")]
        //    UTF_16LE=1200,
        //    [Description("UTF-16(BE)")]
        //    UTF_16BE=1201,
        //    [Description("UTF-32(LE)")]
        //    UTF_32LE=12000,
        //    [Description("UTF-32(BE)")]
        //    UTF_32BE=12001,
        //    [Description("UTF-8")]
        //    UTF_8=65001,
        //};

        private TypeLineFeed[] lineFeeds_ = new TypeLineFeed[3] { TypeLineFeed.LF, TypeLineFeed.LF, TypeLineFeed.LF};
        //[Category("General")]
        //[DisplayName("Line Feed")]
        //[Description("Line Feed")]
        //public TypeLineFeed LineFeed
        //{
        //    get { return lineFeed_; }
        //    set { lineFeed_ = value; }
        //}
        [Category("General")]
        [DisplayName("C/C++")]
        [Description("Line feed for C/C++")]
        public TypeLineFeed LineFeedCpp
        {
            get { return lineFeeds_[(int)TypeLanguage.C_Cpp]; }
            set { lineFeeds_[(int)TypeLanguage.C_Cpp] = value; }
        }

        [Category("General")]
        [DisplayName("CSharp")]
        [Description("Line feed for CSharp")]
        public TypeLineFeed LineFeedCSharp
        {
            get { return lineFeeds_[(int)TypeLanguage.CSharp]; }
            set { lineFeeds_[(int)TypeLanguage.CSharp] = value; }
        }

        [Category("General")]
        [DisplayName("Others")]
        [Description("Line feed for Others")]
        public TypeLineFeed LineFeedOthers
        {
            get { return lineFeeds_[(int)TypeLanguage.Others]; }
            set { lineFeeds_[(int)TypeLanguage.Others] = value; }
        }
    }
}
