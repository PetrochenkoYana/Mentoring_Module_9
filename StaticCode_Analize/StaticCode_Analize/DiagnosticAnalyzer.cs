using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;


namespace StaticCode_Analize
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class StaticCode_AnalizeAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "StaticCode_Analize";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Naming";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        private static readonly LocalizableString ControllerRuleTitle = new LocalizableResourceString(nameof(Resources.ControllerRuleAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString ControllerRuleMessageFormat = new LocalizableResourceString(nameof(Resources.ControllerRuleAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString ControllerRuleDescription = new LocalizableResourceString(nameof(Resources.ControllerRuleAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string ControllerRuleCategory = "Naming";
        private static DiagnosticDescriptor ControllerRule = new DiagnosticDescriptor(DiagnosticId, ControllerRuleTitle, ControllerRuleMessageFormat, ControllerRuleCategory, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: ControllerRuleDescription);

        private static readonly LocalizableString AuthorizeControllerRuleTitle = new LocalizableResourceString(nameof(Resources.AuthorizeControllerRuleAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString AuthorizeControllerRuleMessageFormat = new LocalizableResourceString(nameof(Resources.AuthorizeControllerRuleAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString AuthorizeControllerRuleDescription = new LocalizableResourceString(nameof(Resources.AuthorizeControllerRuleAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string AuthorizeControllerRuleCategory = "Naming";
        private static DiagnosticDescriptor AuthorizeControllerRule = new DiagnosticDescriptor(DiagnosticId, AuthorizeControllerRuleTitle, AuthorizeControllerRuleMessageFormat, AuthorizeControllerRuleCategory, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: AuthorizeControllerRuleDescription);

        private static readonly LocalizableString NamespaceRuleTitle = new LocalizableResourceString(nameof(Resources.NamespaceRuleAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString NamespaceRuleMessageFormat = new LocalizableResourceString(nameof(Resources.NamespaceRuleAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString NamespaceRuleDescription = new LocalizableResourceString(nameof(Resources.NamespaceRuleAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string NamespaceRuleCategory = "Naming";
        private static DiagnosticDescriptor NamespaceRule = new DiagnosticDescriptor(DiagnosticId, NamespaceRuleTitle, NamespaceRuleMessageFormat, NamespaceRuleCategory, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: NamespaceRuleDescription);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            //context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
            //context.RegisterSymbolAction(FindWrongControllerName,SymbolKind.NamedType);
            //context.RegisterSymbolAction(FindUnauthorisedControllers, SymbolKind.NamedType);
            context.RegisterSymbolAction(FindWrongNamespaceTypes, SymbolKind.NamedType);
        }

        private static void FindWrongControllerName(SymbolAnalysisContext context)
        {
            var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;
            var controller = context.Compilation.GetTypeByMetadataName("System.Web.Mvc.Controller");
            var baseTypes = GetBaseClasses(namedTypeSymbol, context.Compilation.ObjectType);

            // Find just those named type symbols with names not containing "Controller".
            if (baseTypes.Contains(controller) && !namedTypeSymbol.Name.EndsWith("Controller"))
            {
                // For all such symbols, produce a diagnostic.
                var diagnostic = Diagnostic.Create(ControllerRule, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);

                context.ReportDiagnostic(diagnostic);
            }
        }

        public static ImmutableArray<INamedTypeSymbol> GetBaseClasses(INamedTypeSymbol type, INamedTypeSymbol objectType)
        {
            if (type == null || type.TypeKind == TypeKind.Error)
            {
                return ImmutableArray<INamedTypeSymbol>.Empty;
            }

            if (type.BaseType != null && type.BaseType.TypeKind != TypeKind.Error)
            {
                return GetBaseClasses(type.BaseType, objectType).Add(type.BaseType);
            }
            return ImmutableArray<INamedTypeSymbol>.Empty;
        }

        private static void FindUnauthorisedControllers(SymbolAnalysisContext context)
        {
            var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;
            var controller = context.Compilation.GetTypeByMetadataName("System.Web.Mvc.Controller");
            var authorizeAttribute = context.Compilation.GetTypeByMetadataName("System.Web.Mvc.AuthorizeAttribute");

            var baseTypes = GetBaseClasses(namedTypeSymbol, context.Compilation.ObjectType);

            // Find just those named type symbols that don't have [Authorize] attribute for class oe all its public methods.
            if (baseTypes.Contains(controller)
                && !(namedTypeSymbol.GetAttributes().Any(a => a.AttributeClass == authorizeAttribute))
                && !(namedTypeSymbol.GetMembers().Any(m =>
                        m.Kind == SymbolKind.Method &&
                        m.DeclaredAccessibility == Accessibility.Public &&
                        m.GetAttributes().Any(a => a.AttributeClass.MetadataName == authorizeAttribute.MetadataName))))
            {
                // For all such symbols, produce a diagnostic.
                var diagnostic = Diagnostic.Create(AuthorizeControllerRule, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);

                context.ReportDiagnostic(diagnostic);
            }
        }

        private static void FindWrongNamespaceTypes(SymbolAnalysisContext context)
        {
            var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;
            var namedTypeSymbolIsFromNamespace = context.Symbol.ContainingNamespace.MetadataName == "Entities";

            var dataContractAttribute = context.Compilation.GetTypeByMetadataName("System.Runtime.Serialization.DataContractAttribute");
            var dg = namedTypeSymbol.GetMembers();
            // Find just those named type symbols that don't have [DataContract] attribute , not public and don't contain 'Id' and 'Name' properties.
            if (namedTypeSymbolIsFromNamespace
                && (!(namedTypeSymbol.GetAttributes().Any(a => a.AttributeClass == dataContractAttribute))
                || namedTypeSymbol.DeclaredAccessibility != Accessibility.Public
                || !(namedTypeSymbol.GetMembers().Any(m => m.Kind == SymbolKind.Property && m.MetadataName == "Id"))
                || !namedTypeSymbol.GetMembers().Any(m => m.Kind == SymbolKind.Property && m.MetadataName == "Name")))
            {
                // For all such symbols, produce a diagnostic.
                var diagnostic = Diagnostic.Create(NamespaceRule, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);

                context.ReportDiagnostic(diagnostic);
            }
        }
        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            // TODO: Replace the following code with your own analysis, generating Diagnostic objects for any issues you find
            var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

            // Find just those named type symbols with names containing lowercase letters.
            if (namedTypeSymbol.Name.ToCharArray().Any(char.IsLower))
            {
                // For all such symbols, produce a diagnostic.
                var diagnostic = Diagnostic.Create(Rule, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);

                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
