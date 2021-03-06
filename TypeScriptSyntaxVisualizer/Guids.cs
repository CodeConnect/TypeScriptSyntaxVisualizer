﻿// Guids.cs
// MUST match guids.h
using System;

namespace CodeConnect.TypeScriptSyntaxVisualizer
{
    static class GuidList
    {
        public const string guidTypeScriptSyntaxVisualizerPkgString = "7de5d83a-17b7-4727-ba4a-340e40a43112";
        public const string guidTypeScriptSyntaxVisualizerCmdSetString = "ab9fce60-a426-4d8a-9947-02b391374ee9";
        public const string guidToolWindowPersistanceString = "4a2b96fc-bf73-420e-ad92-dbc15aac6b39";

        public static readonly Guid guidTypeScriptSyntaxVisualizerCmdSet = new Guid(guidTypeScriptSyntaxVisualizerCmdSetString);
    };
}