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

        /**
        @brief Print a message to the editor's output
        */
        [System.Diagnostics.Conditional("DEBUG")]
        public static void output(string message, EnvDTE80.DTE2 dte)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            EnvDTE.OutputWindow outputWindow = dte.ToolWindows.OutputWindow;
            if(null == outputWindow) {
                return;
            }
            foreach(EnvDTE.OutputWindowPane window in outputWindow.OutputWindowPanes) {
                window.OutputString(message);
            }
        }

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

        /**
        @brief Load a setting for the language
        */
        private void loadSettings(out OptionPageForceLineFeedCode.TypeLineFeed linefeed, OptionPageForceLineFeedCode.TypeLanguage language, string documentPath)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            linefeed = OptionPageForceLineFeedCode.TypeLineFeed.LF;

            //Load from a file
            if (package_.Options.LoadSettingFile) {
                SettingFile settingFile = package_.loadFileSettings(documentPath);
                if(null != settingFile) {
                    linefeed = settingFile.get(language);
                    return;
                }
            }
            //Otherwise, load from the option page
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
            output(string.Format("Load _forcelinefeedcode.xml: {0}\n", package_.Options.LoadSettingFile), package_.DTE);

            //Get the current document
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
            EnvDTE.TextDocument textDocument = document.Object("TextDocument") as EnvDTE.TextDocument;
            if(null == textDocument) {
                return VSConstants.S_OK;
            }

            //Get the current language
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

            //Specify a target line-feed code
            OptionPageForceLineFeedCode.TypeLineFeed linefeed = OptionPageForceLineFeedCode.TypeLineFeed.LF;
            loadSettings(out linefeed, language, document.Path);

            output(string.Format("doc:{0} => lang:{1} code:{2}\n", document.Language, language, linefeed), package_.DTE);
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

            //Convert different codes
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
            output(string.Format("Replace {0} EOLs\n", count), package_.DTE);
            return VSConstants.S_OK;
        }
    }
}
