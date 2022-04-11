using System;
using System.ComponentModel;
using System.Xml;

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
            C_Cpp =0,
            [Description("CSharp")]
            CSharp,
            [Description("Others")]
            Others,
        };
        public const int NumLanguages = 3;

        private TypeLineFeed[] lineFeeds_ = new TypeLineFeed[NumLanguages] { TypeLineFeed.LF, TypeLineFeed.LF, TypeLineFeed.LF};
        private bool loadSettingFile_;

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

        [Category("General")]
        [DisplayName("Load Setting File")]
        [Description("Load \"_forcelinefeedcode.xml\".")]
        public bool LoadSettingFile 
        {
            get { return loadSettingFile_; }
            set { loadSettingFile_ = value; }
        }
    }
}
