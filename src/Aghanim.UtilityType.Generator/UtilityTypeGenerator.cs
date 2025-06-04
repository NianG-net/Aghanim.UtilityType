using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Aghanim.UtilityType.Generator;

public abstract class UtilityTypeGenerator : IIncrementalGenerator
{
    public abstract IEnumerable<ISymbol> FilterSymbol(IEnumerable<ISymbol> symbols, ImmutableHashSet<string> constructorArguments);
    public IEnumerable<(string hitName, string source)> ParseSource(GeneratorAttributeSyntaxContext context)
    {
        var values =

        from attr in context.Attributes
        let TypeSymbol = attr.AttributeClass.TypeArguments[0]
        let ConstructorArguments = from typedConstant in attr.ConstructorArguments
                                   from innerTypedConstant in typedConstant.Values
                                   select innerTypedConstant.Value.ToString()
        select new { TypeSymbol, ConstructorArguments };


        var symbolDict = values.GroupBy(x => x.TypeSymbol, SymbolEqualityComparer.Default)

            .Select(x => new
            {
                TypeSymbol = Unsafe.As<ITypeSymbol>(x.Key),

                PropertySymbols = Unsafe.As<ITypeSymbol>(x.Key).GetMembers().OfType<IPropertySymbol>(),
                ConstructorArguments = x.SelectMany(x => x.ConstructorArguments).ToImmutableHashSet()
            });





        foreach (var symbolItem in symbolDict)
        {
            HashSet<MemberDeclarationSyntax> propertys = [];

            var symbols = FilterSymbol(symbolItem.PropertySymbols, [.. symbolItem.ConstructorArguments]);

            foreach (IPropertySymbol propertySymbol in symbols)
            {
                foreach (SyntaxReference syntaxReference in propertySymbol.DeclaringSyntaxReferences)
                {


                    var node = syntaxReference.GetSyntax();
                    if (node is ParameterSyntax)
                    {
                        var typeSyntax = ParseTypeName(propertySymbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
                        node = PropertyDeclaration(typeSyntax, propertySymbol.Name).WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                            .WithAccessorList(AccessorList(List([
                                AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                                AccessorDeclaration(SyntaxKind.InitAccessorDeclaration).WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
                            ])));
                    }
                    
                    var syntax = Unsafe.As<MemberDeclarationSyntax>(node);
                    if (symbolItem.TypeSymbol is { TypeKind: TypeKind.Interface } && !syntax.Modifiers.Any(SyntaxKind.PublicKeyword))
                    {
                        syntax = syntax.AddModifiers(Token(SyntaxKind.PublicKeyword));

                    }
                    propertys.Add(syntax);
                }


            }


            string hintName = $"{context.TargetSymbol.ToDisplayString()}.{symbolItem.TypeSymbol.ToDisplayString()}.g.cs";


            var typeDeclaration = Unsafe.As<TypeDeclarationSyntax>(context.TargetNode)
                .WithOpenBraceToken(Token(SyntaxKind.OpenBraceToken))
                .WithCloseBraceToken(Token(SyntaxKind.CloseBraceToken))
                .WithAttributeLists([])
                .WithMembers(List(propertys));

            var namespaceDeclaration = context.TargetNode.Ancestors().OfType<BaseNamespaceDeclarationSyntax>().FirstOrDefault();

            if (namespaceDeclaration is not null)
            {
                namespaceDeclaration = namespaceDeclaration.WithMembers([typeDeclaration]);
                var compilationUnit = context.SemanticModel.SyntaxTree.GetCompilationUnitRoot()
                                                                      .WithMembers([namespaceDeclaration]);
                yield return (hintName, compilationUnit.NormalizeWhitespace().ToFullString());
            }





        }
    }
    public abstract string AttributeFullName { get; }
    public virtual void Initialize(IncrementalGeneratorInitializationContext context)
    {
#if DEBUG_GEN
        System.Diagnostics.Debugger.Launch();
#endif
        var declaration = context.SyntaxProvider.ForAttributeWithMetadataName(
            AttributeFullName,
            static (s, _) => s is TypeDeclarationSyntax tds && tds.Modifiers.Any(SyntaxKind.PartialKeyword),
            static (ctx, _) => ctx)
            .Where(x => x.TargetSymbol is INamedTypeSymbol)
            .Select((x, _) => x);

        context.RegisterSourceOutput(declaration, (sourceContext, utilityTypeDeclaration) =>
        {
            var result = ParseSource(utilityTypeDeclaration);
            foreach (var (hitName, source) in result)
            {
                sourceContext.AddSource(hitName, source);
            }

        });
    }
}
