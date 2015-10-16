using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Media;
using Microsoft.VisualStudio.Text;
using System.Collections.ObjectModel;

namespace CodeConnect.TypeScriptSyntaxVisualizer
{
    [Export(typeof(IWpfTextViewConnectionListener))]
    [ContentType("TypeScript")]
    [ContentType("JavaScript")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    public class TextViewCreationListener : IWpfTextViewConnectionListener
    {
        private void Caret_PositionChanged(object sender, CaretPositionChangedEventArgs args)
        {
            try
            {
                //Don't waste time computing anything if we can't show it.
                if (!MyToolWindow.MyControl.IsWindowVisible)
                    return;

                var caret = (ITextCaret)sender;
                int position = args.NewPosition.BufferPosition.Position;

                //We roll the dice on an OOM exception... :(
                string rawText = caret.ContainingTextViewLine.Snapshot.GetText();

                using (TypeScriptProcessor tsProcessor = new TypeScriptProcessor())
                {
                    var root = tsProcessor.ParseFileAndGetSyntaxRoot(rawText);
                    MyToolWindow.MyControl.UpdateWithSyntaxRoot(root, position);
                    System.Diagnostics.Debug.WriteLine(root.Kind);
                }
            }
            catch(Exception e)
            {
                var error = e.ToString();
                System.Diagnostics.Debug.WriteLine(error);
            }
        }

        public void SubjectBuffersConnected(IWpfTextView textView, ConnectionReason reason, Collection<ITextBuffer> subjectBuffers)
        {
            textView.Caret.PositionChanged += Caret_PositionChanged;
        }

        public void SubjectBuffersDisconnected(IWpfTextView textView, ConnectionReason reason, Collection<ITextBuffer> subjectBuffers)
        {
            textView.Caret.PositionChanged -= Caret_PositionChanged;
        }
    }
}
