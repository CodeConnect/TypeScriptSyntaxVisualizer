using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.ComponentModelHost;
using System.Windows.Forms;
using System.ComponentModel.Composition;
using System.Windows.Media;

namespace CodeConnect.TypeScriptSyntaxVisualizer
{
    [Guid("4a2b96fc-bf73-420e-ad92-dbc15aac6b39")]
    public class MyToolWindow : ToolWindowPane, IOleCommandTarget
    {
        public MyToolWindow() : base(null)
        {
            this.Caption = "TypeScript Syntax Visaulizer";
            this.BitmapResourceID = 301;
            this.BitmapIndex = 1;
       }

        public static MyControl MyControl { get; } = new MyControl();
        public override object Content
        {
            get
            {
                return MyControl;
            }
        }

        public override void OnToolWindowCreated()
        {
            //We need to set up the tool window to respond to key bindings
            //They're passed to the tool window and its buffers via Query() and Exec()
            var windowFrame = (IVsWindowFrame)Frame;
            var cmdUi = Microsoft.VisualStudio.VSConstants.GUID_TextEditorFactory;
            windowFrame.SetGuidProperty((int)__VSFPROPID.VSFPROPID_InheritKeyBindings, ref cmdUi);
            base.OnToolWindowCreated();
        }
    }
}
