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
        private RunningDocumentTable runningDocumentTable_;

#if DEBUG
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
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            Output(message + "\n");
        }
#endif

        public RunningDocTableEvents(ForceLineFeedCodePackage package)
        {
            package_ = package;
            runningDocumentTable_ = new RunningDocumentTable(package);
            runningDocumentTable_.Advise(this);
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
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            linefeed = OptionPageForceLineFeedCode.TypeLineFeed.LF;

            if (package_.Options.LoadSettingFile) {
                SettingFile settingFile = package_.loadFileSettings();
                if(null != settingFile) {
                    linefeed = settingFile.get(language);
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
            RunningDocumentInfo runningDocumentInfo = runningDocumentTable_.GetDocumentInfo(docCookie);
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
