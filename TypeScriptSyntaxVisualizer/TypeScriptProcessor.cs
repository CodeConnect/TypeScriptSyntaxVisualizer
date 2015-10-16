using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CodeConnect.TypeScriptSyntaxVisualizer
{
    /// <summary>
    /// In order to parse the TypeScript we have to run our Javascript
    /// parser. This sucks, but it seems to work and doesn't require 
    /// our users to have anything special installed.
    /// </summary>
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
            shim = engine.AddScript("TypeScriptParser", scriptText, scriptText.Length);
        }

        public SyntaxNodeOrToken ParseFileAndGetSyntaxRoot(string sourceText)
        {
            VerifyThread();
            var rawJson = shim(sourceText);
            var serializer = new JsonSerializer();
            using (TextReader sr = new StringReader(rawJson))
            {
                var root = (SyntaxNodeOrToken)serializer.Deserialize(sr, typeof(SyntaxNodeOrToken));
                return root;
            }
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
}
