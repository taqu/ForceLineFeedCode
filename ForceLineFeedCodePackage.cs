using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;

namespace ForceLineFeedCode
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the information needed to show this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [Guid(GuidList.guidForceEncPkgString)]
    [ProvideOptionPage(typeof(OptionPageForceLineFeedCode), "ForceLineFeedCode", "General", 0, 0, true)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists)]
    public sealed class ForceLineFeedCodePackage : Package
    {
        public EnvDTE80.DTE2 DTE { get { return dte2_.Value;} }
        public RunningDocumentTable RDT { get { return runningDocumentTable_.Value;} }

        public OptionPageForceLineFeedCode Options
        {
            get
            {
                return GetDialogPage(typeof(OptionPageForceLineFeedCode)) as OptionPageForceLineFeedCode;
            }
        }

        private Lazy<EnvDTE80.DTE2> dte2_;
        private Lazy<RunningDocumentTable> runningDocumentTable_;
        private Lazy<Microsoft.VisualStudio.OLE.Interop.IServiceProvider> servicePorvider_;
        private RunningDocTableEvents runningDocTableEvents_;

        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public ForceLineFeedCodePackage()
        {
        }



        /////////////////////////////////////////////////////////////////////////////
        // Overridden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            dte2_ = new Lazy<EnvDTE80.DTE2>(()=> GetService(typeof(EnvDTE.DTE)) as EnvDTE80.DTE2);
            servicePorvider_ = new Lazy<Microsoft.VisualStudio.OLE.Interop.IServiceProvider>(() => Package.GetGlobalService(typeof(Microsoft.VisualStudio.OLE.Interop.IServiceProvider)) as Microsoft.VisualStudio.OLE.Interop.IServiceProvider);
            runningDocumentTable_ = new Lazy<RunningDocumentTable>(()=>new RunningDocumentTable(new ServiceProvider(servicePorvider_.Value)));
            runningDocTableEvents_ = new RunningDocTableEvents(this);
        }
        #endregion

    }
}
