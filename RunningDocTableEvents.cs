using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Linq;

namespace ForceLineFeedCode
{
    internal class RunningDocTableEvents : IVsRunningDocTableEvents3
    {
        private ForceLineFeedCodePackage package_;

        private void Output(string message)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            EnvDTE.OutputWindow outputWindow = package_.DTE.ToolWindows.OutputWindow;
            if(null == outputWindow) {
                return;
            }
            foreach(EnvDTE.OutputWindowPane window in outputWindow.OutputWindowPanes) {
                window.OutputString(message);
            }
        }

        private void OutputLine(string message)
        {
            Output(message + "\n");
        }

        public RunningDocTableEvents(ForceLineFeedCodePackage package)
        {
            package_ = package;
            if(null != package_.RDT) {
                package_.RDT.Advise(this);
            }
        }

        public int OnAfterAttributeChange(uint docCookie, uint grfAttribs)
        {
            return VSConstants.S_OK;
        }


        public int OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterFirstDocumentLock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterSave(uint docCookie)
        {
#if false
            OutputLine("OnAfterSave");
            RunningDocumentInfo runningDocumentInfo = package_.RDT.GetDocumentInfo(docCookie);
            EnvDTE.Document document = package_.DTE.Documents.OfType<EnvDTE.Document>().SingleOrDefault(x => x.FullName == runningDocumentInfo.Moniker);
            if(null == document) {
                return VSConstants.S_OK;
            }
            if(document.Kind != "{8E7B96A8-E33D-11D0-A6D5-00C04FB67F6A}") {
                return VSConstants.S_OK;
            }

            OptionPageForceEnc optionPage = package_.Options;
            OptionPageForceEnc.TypeEncoding typeEncoding = OptionPageForceEnc.TypeEncoding.UTF_8;
            bool bom = true;
            if(null != optionPage){
                typeEncoding = optionPage.Encoding;
                bom = optionPage.BOM;
            }

            System.IO.FileStream stream = new System.IO.FileStream(document.FullName, System.IO.FileMode.Open);
            {
                System.Text.Encoding encoding;
                System.Text.Encoding writeEncoding;
                switch(typeEncoding) {
                case OptionPageForceEnc.TypeEncoding.UTF_16LE:
                    encoding = new System.Text.UnicodeEncoding(false, bom, true);
                    writeEncoding = new System.Text.UnicodeEncoding(false, bom);
                    break;
                case OptionPageForceEnc.TypeEncoding.UTF_16BE:
                    encoding = new System.Text.UnicodeEncoding(true, bom, true);
                    writeEncoding = new System.Text.UnicodeEncoding(true, bom);
                    break;
                case OptionPageForceEnc.TypeEncoding.UTF_32LE:
                    encoding = new System.Text.UTF32Encoding(false, bom, true);
                    writeEncoding = new System.Text.UTF32Encoding(false, bom);
                    break;
                case OptionPageForceEnc.TypeEncoding.UTF_32BE:
                    encoding = new System.Text.UTF32Encoding(true, bom, true);
                    writeEncoding = new System.Text.UTF32Encoding(true, bom);
                    break;
                case OptionPageForceEnc.TypeEncoding.UTF_8:
                    encoding = new System.Text.UTF8Encoding(bom, true);
                    writeEncoding = new System.Text.UTF8Encoding(bom);
                    break;
                default:
                    encoding = writeEncoding = System.Text.Encoding.GetEncoding((int)typeEncoding);
                    break;
                }

                try {
                    stream.Position = 0;
                    System.IO.StreamReader reader = new System.IO.StreamReader(stream, encoding);
                    while(!reader.EndOfStream) {
                        reader.Read();
                    }
                    stream.Close();
                    return VSConstants.S_OK;

                } catch(System.Text.DecoderFallbackException) {

                } catch {
                    stream.Close();
                    return VSConstants.S_OK;
                }

                OutputLine("Change Encoding");
                try {
                    stream.Position = 0;
                    System.IO.StreamReader reader = new System.IO.StreamReader(stream, System.Text.Encoding.Default);
                    string text = reader.ReadToEnd();
                    System.IO.File.WriteAllText(document.FullName, text, writeEncoding);

                } catch {
                }
                stream.Close();
            }
#endif
            return VSConstants.S_OK;
        }

        public int OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {
            return VSConstants.S_OK;
        }


        public int OnAfterAttributeChangeEx(uint docCookie, uint grfAttribs, IVsHierarchy pHierOld, uint itemidOld, string pszMkDocumentOld, IVsHierarchy pHierNew, uint itemidNew, string pszMkDocumentNew)
        {
            return VSConstants.S_OK;
        }

        private void loadSettings(out OptionPageForceLineFeedCode.TypeLineFeed linefeed, OptionPageForceLineFeedCode.TypeLanguage language)
        {
            linefeed = OptionPageForceLineFeedCode.TypeLineFeed.LF;

            if (package_.Options.LoadSettingFile) {
#if DEBUG
                OutputLine(package_.SolutionDirectory);
#endif
                OptionPageForceLineFeedCode.FileSettings fileSettings = package_.loadFileSettings();
                if(null != fileSettings) {
#if DEBUG
                    OutputLine(fileSettings.lastWriteTime_.ToShortDateString());
                    OutputLine(fileSettings.lineFeeds_[0].ToString());
                    OutputLine(fileSettings.lineFeeds_[1].ToString());
                    OutputLine(fileSettings.lineFeeds_[2].ToString());
#endif
                    linefeed = fileSettings.lineFeeds_[(int)language];
                    return;
                }
            }
            OptionPageForceLineFeedCode optionPage = package_.Options;
            if (null != optionPage) {
                switch (language) {
                case OptionPageForceLineFeedCode.TypeLanguage.C_Cpp:
                    linefeed = optionPage.LineFeedCpp;
                    break;
                case OptionPageForceLineFeedCode.TypeLanguage.CSharp:
                    linefeed = optionPage.LineFeedCSharp;
                    break;
                default:
                    linefeed = optionPage.LineFeedOthers;
                    break;
                }
            }
        }
        public int OnBeforeSave(uint docCookie)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
#if DEBUG
            OutputLine(string.Format("Load _forcelinefeedcode.xml: {0}", package_.Options.LoadSettingFile));
#endif
            RunningDocumentInfo runningDocumentInfo = package_.RDT.GetDocumentInfo(docCookie);
            EnvDTE.Document document = null;
            foreach(EnvDTE.Document doc in package_.DTE.Documents.OfType<EnvDTE.Document>())
            {
                if(doc.FullName == runningDocumentInfo.Moniker)
                {
                    document = doc;
                    break;
                }
            }
            if(null == document) {
                return VSConstants.S_OK;
            }
            if(document.Kind != "{8E7B96A8-E33D-11D0-A6D5-00C04FB67F6A}") {
                return VSConstants.S_OK;
            }
#if DEBUG
            OutputLine(document.Language);
#endif
            EnvDTE.TextDocument textDocument = document.Object("TextDocument") as EnvDTE.TextDocument;
            if(null == textDocument) {
                return VSConstants.S_OK;
            }


            OptionPageForceLineFeedCode.TypeLanguage language = OptionPageForceLineFeedCode.TypeLanguage.Others;
            switch (document.Language) {
            case "C/C++":
                language = OptionPageForceLineFeedCode.TypeLanguage.C_Cpp;
                break;
            case "CSharp":
                language = OptionPageForceLineFeedCode.TypeLanguage.CSharp;
                break;
            default:
                language = OptionPageForceLineFeedCode.TypeLanguage.Others;
                break;
            }

            OptionPageForceLineFeedCode.TypeLineFeed linefeed = OptionPageForceLineFeedCode.TypeLineFeed.LF;
            loadSettings(out linefeed, language);
#if DEBUG
            OutputLine(string.Format("Language {0}", language));
#endif
            string replaceLineFeed;
            switch(linefeed){
            case OptionPageForceLineFeedCode.TypeLineFeed.LF:
                replaceLineFeed = "\n";
                break;
            case OptionPageForceLineFeedCode.TypeLineFeed.CR:
                replaceLineFeed = "\r";
                break;
            //case OptionPageForceLineFeedCode.TypeLineFeed.CRLF:
            default:
                replaceLineFeed = "\r\n";
                break;
            }

            int count = 0;
            EnvDTE.EditPoint editPoint = textDocument.StartPoint.CreateEditPoint();
            if(OptionPageForceLineFeedCode.TypeLineFeed.CRLF == linefeed) {
                while(!editPoint.AtEndOfDocument) {
                    editPoint.EndOfLine();
                    string lineending = editPoint.GetText(1);
                    if(string.IsNullOrEmpty(lineending)) {
                        continue;
                    }

                    for(int i = 0; i < lineending.Length; ++i) {
                        switch(lineending[i]) {
                        case '\n':
                            editPoint.ReplaceText(1, replaceLineFeed, 0);
                            ++count;
                            break;
                        case '\r':
                            if((lineending.Length - 1)<=i) {
                                editPoint.ReplaceText(1, replaceLineFeed, 0);
                                ++count;
                            }else if('\n' != lineending[i+1]) {
                                editPoint.ReplaceText(1, replaceLineFeed, 0);
                                ++count;
                            }else {
                                ++i;
                            }
                            break;
                        default:
                            break;
                        }
                    }
                    editPoint.CharRight();
                }

            }else {
                while(!editPoint.AtEndOfDocument) {
                    editPoint.EndOfLine();
                    string lineending = editPoint.GetText(1);
                    if(string.IsNullOrEmpty(lineending)) {
                        continue;
                    }

                    for(int i = 0; i < lineending.Length; ++i) {
                        if(lineending[i] != replaceLineFeed[0]) {
                            editPoint.ReplaceText(1, replaceLineFeed, 0);
                            ++count;
                        }
                    }
                    editPoint.CharRight();
                }
            }
#if DEBUG
            OutputLine(string.Format("Replace {0} EOLs", count));
#endif
            return VSConstants.S_OK;
        }
    }
}
