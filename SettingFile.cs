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

        public OptionPageForceLineFeedCode.TypeLineFeed get(OptionPageForceLineFeedCode.TypeLanguage language)
        {
            return lineFeeds_[(int)language];
        }

        public bool load(EnvDTE80.DTE2 dte, string documentPath)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            string directoryPath = System.IO.Path.GetDirectoryName(documentPath);
            if (string.IsNullOrEmpty(directoryPath) || !System.IO.Directory.Exists(directoryPath)) {
                return false;
            }
            string filepath = string.Empty;
            if (!directoryToFile_.TryGetValue(directoryPath, out filepath) || !System.IO.File.Exists(filepath)) {
                if (null == dte) {
                    return false;
                }
                RunningDocTableEvents.output(string.Format("search from {0}\n", directoryPath), dte);
                filepath = findFile(directoryPath);
                if(string.IsNullOrEmpty(filepath) || !System.IO.File.Exists(filepath)) {
                    return false;
                }
                addToCache(directoryPath, filepath);
            }

            DateTime lastWriteTime = System.IO.File.GetLastWriteTime(filepath);
            RunningDocTableEvents.output(string.Format("{0}: last write time {1}<={2}\n", filepath, lastWriteTime, lastWriteTime_), dte);
            if (lastWriteTime <= lastWriteTime_) {
                return true;
            }

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = true;
            settings.IgnoreComments = true;
            try {
                TypeLineFeed[] lineFeeds = new TypeLineFeed[NumLanguages] { TypeLineFeed.LF, TypeLineFeed.LF, TypeLineFeed.LF };

                using (XmlReader reader = XmlReader.Create(filepath, settings)) {
                    reader.ReadStartElement("General");
                    do {
                        if (!reader.IsStartElement() || "Code" != reader.Name) {
                            continue;
                        }
                        RunningDocTableEvents.output("Read Code\n", dte);
                        if (!reader.HasAttributes) {
                            continue;
                        }
                        string lang = reader.GetAttribute("lang");
                        if (string.IsNullOrEmpty(lang)) {
                            continue;
                        }
                        lang = lang.Trim();
                        RunningDocTableEvents.output("  Language " + lang + "\n", dte);
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
                        RunningDocTableEvents.output("  code " + code + "\n", dte);
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
                    }while(reader.Read());
                } //using (XmlReader reader
                lineFeeds_ = lineFeeds;
                lastWriteTime_ = lastWriteTime;
#if DEBUG
                for(int i=0; i< NumLanguages; ++i) {
                    RunningDocTableEvents.output(string.Format(" lang:{0} code:{1}\n", (TypeLanguage)i, lineFeeds_[i]), dte);
                }
            } catch (Exception exception) {
                RunningDocTableEvents.output(exception.ToString() + "\n", dte);
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

        private void addToCache(string directory, string file)
        {
            if (directoryToFile_.ContainsKey(directory)) {
                directoryToFile_.Remove(directory);
            }
            if (MaxCaches <= directoryToFile_.Count) {
                System.Random random = new Random();
                for(int i=0; i<64; ++i) {
                    removeFromCache(random.Next() % directoryToFile_.Count);
                }
            }
            directoryToFile_.Add(directory, file);
        }

        private void removeFromCache(int index)
        {
            int count = 0;
            foreach (string key in directoryToFile_.Keys) {
                if(count == index) {
                    directoryToFile_.Remove(key);
                    return;
                }
                ++count;
            }
        }

        private static readonly string[] RootDirectories = new string[] {".git", ".svn"};
        private const int MaxCaches = 256;

        private TypeLineFeed[] lineFeeds_ = new TypeLineFeed[NumLanguages] { TypeLineFeed.LF, TypeLineFeed.LF, TypeLineFeed.LF };
        private System.Collections.Generic.Dictionary<string, string> directoryToFile_ = new System.Collections.Generic.Dictionary<string, string>(MaxCaches);
        private DateTime lastWriteTime_ = new DateTime();
    }
}

