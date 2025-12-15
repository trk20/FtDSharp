using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FtDSharp
{
    internal static class ForbiddenNamespaceValidator
    {
        private static readonly string[] ForbiddenNamespaces =
        {
            "System.Reflection",
            "System.IO",
            "System.Net",
            "System.Diagnostics.Process",
            "Microsoft.CodeAnalysis"
        };

        public static List<string> GetInvalidUsages(CSharpCompilation compilation, SyntaxTree syntaxTree)
        {
            var errors = new List<string>();

            var root = syntaxTree.GetRoot();

            // eg `using System.Reflection;`
            ValidateUsingDirectives(root, ref errors);

            // eg `using System; ... Reflection.X`
            ValidateSemanticUsage(compilation, syntaxTree, ref errors);

            return errors;
        }

        private static void ValidateUsingDirectives(SyntaxNode root, ref List<string> errors)
        {
            var usingDirectives = root.DescendantNodes().OfType<UsingDirectiveSyntax>();

            foreach (var usingDirective in usingDirectives)
            {
                var nameText = usingDirective.Name?.ToString();
                if (string.IsNullOrEmpty(nameText)) continue;

                foreach (var forbidden in ForbiddenNamespaces)
                {
                    // Namespace matches or is a child of forbidden namespace
                    // eg, "System.Reflection" or "System.Reflection.Emit"
                    if (nameText == forbidden || nameText.StartsWith(forbidden + "."))
                    {
                        var location = GetLocationString(usingDirective);
                        errors.Add($"{location}: Use of namespace '{nameText}' is not allowed.");
                    }
                }
            }
        }

        private static void ValidateSemanticUsage(CSharpCompilation compilation, SyntaxTree syntaxTree, ref List<string> errors)
        {
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var root = syntaxTree.GetRoot();

            // Check identifier names, qualified names, and member access expressions
            var nodesToCheck = root.DescendantNodes()
                .Where(n => n is IdentifierNameSyntax || n is QualifiedNameSyntax || n is MemberAccessExpressionSyntax);

            foreach (var node in nodesToCheck)
            {
                var symbolInfo = semanticModel.GetSymbolInfo(node);
                var symbol = symbolInfo.Symbol ?? symbolInfo.CandidateSymbols.FirstOrDefault();

                if (symbol == null) continue;

                // Get the containing namespace of the symbol
                var containingNamespace = GetFullNamespace(symbol);
                if (string.IsNullOrEmpty(containingNamespace)) continue;

                foreach (var forbidden in ForbiddenNamespaces)
                {
                    if (containingNamespace == forbidden || containingNamespace.StartsWith(forbidden + "."))
                    {
                        // Skip if this is a using directive (already checked in phase 1)
                        if (IsPartOfUsingDirective(node)) continue;

                        var location = GetLocationString(node);
                        errors.Add($"{location}: Use of '{symbol.Name}' from namespace '{containingNamespace}' is not allowed.");
                    }
                }
            }
        }

        private static string GetFullNamespace(ISymbol symbol)
        {
            // For namespace symbols, return the namespace itself
            if (symbol is INamespaceSymbol ns)
            {
                return ns.ToDisplayString();
            }

            // For type symbols, get the containing namespace
            if (symbol is ITypeSymbol typeSymbol)
            {
                return typeSymbol.ContainingNamespace?.ToDisplayString() ?? string.Empty;
            }

            // For members (methods, properties, etc.), get the containing type's namespace
            var containingType = symbol.ContainingType;
            if (containingType != null)
            {
                return containingType.ContainingNamespace?.ToDisplayString() ?? string.Empty;
            }

            // Fallback: try to get containing namespace directly
            return symbol.ContainingNamespace?.ToDisplayString() ?? string.Empty;
        }

        private static bool IsPartOfUsingDirective(SyntaxNode node)
        {
            var current = node.Parent;
            while (current != null)
            {
                if (current is UsingDirectiveSyntax)
                    return true;
                current = current.Parent;
            }
            return false;
        }

        private static string GetLocationString(SyntaxNode node)
        {
            var location = node.GetLocation();
            var lineSpan = location.GetLineSpan();
            var line = lineSpan.StartLinePosition.Line; // prelude adds a line
            var column = lineSpan.StartLinePosition.Character + 1; // +1 to convert to 1-based index
            return $"({line},{column})";
        }
    }
}
