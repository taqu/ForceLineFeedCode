using System;
using System.IO;
using System.Xml;
using Microsoft.VisualStudio.Shell;
using static ForceLineFeedCode.OptionPageForceLineFeedCode;

namespace ForceLineFeedCode
{
    public class SettingFile
    {
        public const string FileName = "_forcelinefeedcode.xml";
#if DEBUG
        private void output(string message, EnvDTE80.DTE2 dte)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            EnvDTE.OutputWindow outputWindow = dte.ToolWindows.OutputWindow;
            if (null == outputWindow) {
                return;
            }
            foreach (EnvDTE.OutputWindowPane window in outputWindow.OutputWindowPanes) {
                window.OutputString(message);
            }
        }
#endif

        public OptionPageForceLineFeedCode.TypeLineFeed get(OptionPageForceLineFeedCode.TypeLanguage language)
        {
            return lineFeeds_[(int)language];
        }

        public bool load(EnvDTE80.DTE2 dte)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (string.IsNullOrEmpty(filepath_)) {
                if (null == dte) {
                    return false;
                }
                if (null == dte.Solution) {
                    return false;
                }
                string solution = System.IO.Path.GetDirectoryName(dte.Solution.FullName);
                if (string.IsNullOrEmpty(solution)) {
                    return false;
                }
                filepath_ = findFile(solution);
                if(null == filepath_) {
                    return false;
                }
            }

            if (!System.IO.File.Exists(filepath_)) {
#if DEBUG
                output("Cannot find " + filepath_ + "\n", dte);
#endif
                return false;
            }
            DateTime lastWriteTime = System.IO.File.GetLastWriteTime(filepath_);
#if DEBUG
            output(string.Format("Last write time {0}<={1}\n", lastWriteTime, lastWriteTime_), dte);
#endif
            if (lastWriteTime <= lastWriteTime_) {
                return true;
            }

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = true;
            settings.IgnoreComments = true;
            try {
                TypeLineFeed[] lineFeeds = new TypeLineFeed[NumLanguages] { TypeLineFeed.LF, TypeLineFeed.LF, TypeLineFeed.LF };

                using (XmlReader reader = XmlReader.Create(filepath_, settings)) {
                    reader.ReadStartElement("General");
                    while (reader.Read()) {
                        if (!reader.IsStartElement() || "Code" != reader.Name) {
                            continue;
                        }
#if DEBUG
                        output("Read Code\n", dte);
#endif
                        if (!reader.HasAttributes) {
                            continue;
                        }
                        string lang = reader.GetAttribute("lang");
                        if (string.IsNullOrEmpty(lang)) {
                            continue;
                        }
                        lang = lang.Trim();
#if DEBUG
                        output("  Language " + lang + "\n", dte);
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
                        output("  code " + code + "\n", dte);
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
#if DEBUG
            } catch (Exception exception) {
                output(exception.ToString() + "\n", dte);
#else
            } catch {
#endif

                return false;
            }

            return true;
        }

        private string findFile(string directory)
        {
            if (!System.IO.Directory.Exists(directory)) {
                return null;
            }
            string filepath = directory + '\\' + FileName;
            if (System.IO.File.Exists(filepath)) {
                return filepath;
            }
            DirectoryInfo directoryInfo = new DirectoryInfo(directory);
            foreach(DirectoryInfo child in directoryInfo.GetDirectories()) {
                if(Array.Exists<string>(RootDirectories, element=> child.Name == element)) {
                    return null;
                }
            }
            DirectoryInfo parent = directoryInfo.Parent;
            if(null == parent) {
                return null;
            }
            return findFile(parent.FullName);
        }

        private static readonly string[] RootDirectories = new string[] {".git", ".svn"};

        private TypeLineFeed[] lineFeeds_ = new TypeLineFeed[NumLanguages] { TypeLineFeed.LF, TypeLineFeed.LF, TypeLineFeed.LF };
        private string filepath_;
        private DateTime lastWriteTime_ = new DateTime();
    }
}

