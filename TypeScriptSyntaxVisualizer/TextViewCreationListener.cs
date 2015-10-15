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

namespace CodeConnect.TypeScriptSyntaxVisualizer
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    public class TextViewCreationListener : IWpfTextViewCreationListener
    {
        IWpfTextView _textView;
        public void TextViewCreated(IWpfTextView textView)
        {
            _textView = textView;
            textView.Caret.PositionChanged += Caret_PositionChanged;
        }

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

        public class CustomNode
        {
            [Browsable(false)]
            public List<CustomNode> Children { get; } = new List<CustomNode>();
            public string Kind { get; }
            public int Pos { get;  }
            public int End { get;  }
            public string Text { get; }
            public bool IsToken { get; }

            public CustomNode(string kind, int pos, int end, string text, bool isToken)
            {
                Kind = kind;
                Pos = pos;
                End = end;
                Text = text;
                IsToken = isToken;
            }
        }

        private async void Caret_PositionChanged(object sender, CaretPositionChangedEventArgs args)
        {
            try
            {
                //Get Text 
                int position = args.NewPosition.BufferPosition.Position;
                var rawText = _textView.TextBuffer.CurrentSnapshot.GetText();


                using (TypeScriptProcessor tsProcessor = new TypeScriptProcessor())
                {
                    var result = tsProcessor.Process(rawText);
                    var serializer = new JsonSerializer();
                    using (TextReader sr = new StringReader(result))
                    {
                        var root = (CustomNode)serializer.Deserialize(sr, typeof(CustomNode));
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
    }
}
