namespace CodeConnect.TypeScriptSyntaxVisualizer
{
    public static class VisualStudioServices
    {
        public static EnvDTE.DTE DTE
        {
            get;
            set;
        }

        public static Microsoft.VisualStudio.OLE.Interop.IServiceProvider OLEServiceProvider
        {
            get;
            set;
        }

        public static System.IServiceProvider ServiceProvider
        {
            get;
            set;
        }
    }
}
