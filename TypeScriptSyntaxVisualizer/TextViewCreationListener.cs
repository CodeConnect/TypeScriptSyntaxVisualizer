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
      

        public class TypeScriptProcessor : IDisposable
        {
            [ComImport]
            [Guid("89747b18-17fb-405f-ba07-46a89c5e7be2")]
            [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
            internal interface IJavaScriptExecutionEngine
            {
                object AddScript(string name, string text, int textLength);
                void Close();
                void DumpHeapSnapshot(string snapshotFileName);
            }

            private readonly Thread initialThread;
            private IJavaScriptExecutionEngine engine;
            private dynamic shim;

            public TypeScriptProcessor()
            {
                initialThread = Thread.CurrentThread;
                var hr = CreateExecutionEngine("TSProcessor", enableDebugging: false, forceJScript9: false, engine: out engine);
                if (hr < 0)
                {
                    Marshal.ThrowExceptionForHR(hr);
                }
                var scriptText = CodeConnect.TypeScriptSyntaxVisualizer.Resources.convert;
                shim = engine.AddScript("test", scriptText, scriptText.Length);
            }

            public string Process(string sourceText)
            {
                VerifyThread();
                return shim(sourceText);
            }

            public void Dispose()
            {
                VerifyThread();
                if (engine != null)
                {
                    engine.Close();
                    Marshal.ReleaseComObject(engine);
                }
            }

            private void VerifyThread()
            {
                if (Thread.CurrentThread != initialThread)
                {
                    throw new InvalidOperationException("Attempt to access JS engine from the wrong thread");
                }
            }

            [DllImport("ScriptExecutionEnvironment.dll", CallingConvention = CallingConvention.Cdecl)]
            private static extern int CreateExecutionEngine([MarshalAs(UnmanagedType.LPWStr)] string name, [MarshalAs(UnmanagedType.Bool)] bool enableDebugging, [MarshalAs(UnmanagedType.Bool)] bool forceJScript9, out IJavaScriptExecutionEngine engine);
        }

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
                    var result = tsProcessor.Process(rawText);
                    var serializer = new JsonSerializer();
                    using (TextReader sr = new StringReader(result))
                    {
                        var root = (SyntaxNodeOrToken)serializer.Deserialize(sr, typeof(SyntaxNodeOrToken));
                        MyToolWindow.MyControl.UpdateWithSyntaxRoot(root, position);
                        System.Diagnostics.Debug.WriteLine(root.Kind);
                    }
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
