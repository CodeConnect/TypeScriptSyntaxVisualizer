using System.Collections.Generic;
using System.ComponentModel;

namespace CodeConnect.TypeScriptSyntaxVisualizer
{
    /// <summary>
    /// Represents either a syntax node or syntax token from TypeScript.
    /// 
    /// NOTE:   If you change the properties here, you must change the corresponding
    ///         properties in the TypeScript tree builder or it won't deserialize correctly.
    /// </summary>
    public class SyntaxNodeOrToken
    {
        [Browsable(false)]
        public List<SyntaxNodeOrToken> Children { get; } = new List<SyntaxNodeOrToken>();
        public string Kind { get; }
        public int StartPosition { get; }
        public int End { get; }
        public string Text { get; }
        public bool IsToken { get; }

        public SyntaxNodeOrToken(string kind, int startPosition, int end, string text, bool isToken)
        {
            Kind = kind;
            StartPosition = startPosition;
            End = end;
            Text = text;
            IsToken = isToken;
        }
    }
}
