using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio;

namespace CodeConnect.TypeScriptSyntaxVisualizer
{
    [Guid("4a2b96fc-bf73-420e-ad92-dbc15aac6b39")]
    public class MyToolWindow : ToolWindowPane, IOleCommandTarget, IVsWindowFrameNotify3
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
        
        public int OnClose(ref uint pgrfSaveOptions)
        {
            return (int)__FRAMECLOSE.FRAMECLOSE_PromptSave;
        }

        public int OnShow(int fShow)
        {
            //VS weirdness
            //OnShow() tells us when the window is hidden or shown
            switch((__FRAMESHOW)fShow)
            {
                case __FRAMESHOW.FRAMESHOW_WinShown:
                    MyControl.IsWindowVisible = true;
                    break;
                case __FRAMESHOW.FRAMESHOW_WinHidden:
                    MyControl.IsWindowVisible = false;
                    break;

            }
            return VSConstants.S_OK;
        }

        public int OnMove(int x, int y, int w, int h)
        {
            return VSConstants.S_OK;
        }

        public int OnSize(int x, int y, int w, int h)
        {
            return VSConstants.S_OK;
        }

        public int OnDockableChange(int fDockable, int x, int y, int w, int h)
        {
            return VSConstants.S_OK;
        }
    }
}
