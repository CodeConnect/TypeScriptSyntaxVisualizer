using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;

namespace CodeConnect.TypeScriptSyntaxVisualizer
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(MyToolWindow))]
    [Guid(GuidList.guidTypeScriptSyntaxVisualizerPkgString)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists)]
    public sealed class TypeScriptSyntaxVisualizerPackage : Package
    {
        public TypeScriptSyntaxVisualizerPackage()
        {
        }

        private void ShowToolWindow(object sender, EventArgs e)
        {
            ToolWindowPane window = this.FindToolWindow(typeof(MyToolWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException("Window can't be created");
            }
            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }


        protected override void Initialize()
        {
            Debug.WriteLine (string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if ( null != mcs )
            {
                CommandID toolwndCommandID = new CommandID(GuidList.guidTypeScriptSyntaxVisualizerCmdSet, (int)PkgCmdIDList.cmdidMyTool);
                MenuCommand menuToolWin = new MenuCommand(ShowToolWindow, toolwndCommandID);
                mcs.AddCommand( menuToolWin );
            }

            VisualStudioServices.ServiceProvider = this;
            VisualStudioServices.OLEServiceProvider = (Microsoft.VisualStudio.OLE.Interop.IServiceProvider)VisualStudioServices.ServiceProvider.GetService(typeof(Microsoft.VisualStudio.OLE.Interop.IServiceProvider));
        }



    }
}
