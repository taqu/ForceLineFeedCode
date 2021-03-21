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

        public class FileSettings
        {
#if DEBUG
            private void Output(string message, ForceLineFeedCodePackage package)
            {
                Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
                EnvDTE.OutputWindow outputWindow = package.DTE.ToolWindows.OutputWindow;
                if (null == outputWindow) {
                    return;
                }
                foreach (EnvDTE.OutputWindowPane window in outputWindow.OutputWindowPanes) {
                    window.OutputString(message);
                }
            }
#endif
            public bool load(string solutionDirectory
#if DEBUG
                ,ForceLineFeedCodePackage package
#endif
                )
            {
                string path = solutionDirectory + '\\' + FileName;
                if (!System.IO.File.Exists(path)) {
#if DEBUG
                    Output("Cannot find " + path + "\n", package);
#endif
                    return false;
                }
                DateTime lastWriteTime = System.IO.File.GetLastWriteTime(path);
#if DEBUG
                Output(string.Format("Last write time {0}<={1}\n", lastWriteTime, lastWriteTime_), package);
#endif
                if (lastWriteTime <= lastWriteTime_) {
                    return true;
                }

                XmlReaderSettings settings = new XmlReaderSettings();
                settings.IgnoreWhitespace = true;
                settings.IgnoreComments = true;
                try {
                    TypeLineFeed[] lineFeeds = new TypeLineFeed[NumLanguages] { TypeLineFeed.LF, TypeLineFeed.LF, TypeLineFeed.LF };

                    using (XmlReader reader = XmlReader.Create(path, settings)) {
                        reader.ReadStartElement("General");
                        while (reader.Read()) {
                            if(!reader.IsStartElement() || "Code" != reader.Name) {
                                continue;
                            }
#if DEBUG
                            Output("Read Code\n", package);
#endif
                            if (!reader.HasAttributes) {
                                continue;
                            }
                            string lang = reader.GetAttribute("lang");
                            if(string.IsNullOrEmpty(lang)) {
                                continue;
                            }
                            lang = lang.Trim();
#if DEBUG
                            Output("  Language " + lang + "\n", package);
#endif
                            TypeLanguage typeLanguage = TypeLanguage.C_Cpp;
                            switch (lang) {
                            case "C/C++":
                                typeLanguage = TypeLanguage.C_Cpp;
                                break;
                            case "CSharp":
                                typeLanguage = TypeLanguage.CSharp;
                                break;
                            case "Others":
                                typeLanguage = TypeLanguage.Others;
                                break;
                            default:
                                continue;
                            }
                            string code = reader.ReadString().Trim();
#if DEBUG
                            Output("  code " + code + "\n", package);
#endif
                            TypeLineFeed typeLineFeed = TypeLineFeed.LF;
                            switch (code) {
                            case "LF":
                                typeLineFeed = TypeLineFeed.LF;
                                break;
                            case "CR":
                                typeLineFeed = TypeLineFeed.CR;
                                break;
                            case "CRLF":
                                typeLineFeed = TypeLineFeed.CRLF;
                                break;
                            default:
                                continue;
                            }

                            lineFeeds[(int)typeLanguage] = typeLineFeed;
                        }
                    } //using (XmlReader reader
                    lineFeeds_ = lineFeeds;
                    lastWriteTime_ = lastWriteTime;
                } catch(Exception exception){
#if DEBUG
                    Output(exception.ToString() + "\n", package);
#endif

                    return false;
                }

                return true;
            }

            public const string FileName = "_forcelinefeedcode.xml";
            public TypeLineFeed[] lineFeeds_ = new TypeLineFeed[NumLanguages] { TypeLineFeed.LF, TypeLineFeed.LF, TypeLineFeed.LF };
            public DateTime lastWriteTime_ = new DateTime();
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

        private TypeLineFeed[] lineFeeds_ = new TypeLineFeed[NumLanguages] { TypeLineFeed.LF, TypeLineFeed.LF, TypeLineFeed.LF};
        private bool loadSettingFile_;

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

        [Category("General")]
        [DisplayName("Load Setting File")]
        [Description("Load \"_forcelinefeedcode.xml\" in the solution directory.")]
        public bool LoadSettingFile 
        {
            get { return loadSettingFile_; }
            set { loadSettingFile_ = value; }
        }
    }
}
