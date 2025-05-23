﻿using Microsoft.CodeAnalysis;
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
        let PropertySymbols = TypeSymbol.GetMembers().OfType<IPropertySymbol>()
        select new { PropertySymbols, TypeSymbol, ConstructorArguments };


        var attributeDict = values.ToDictionary(x => x.TypeSymbol, v => v, SymbolEqualityComparer.Default);
       

        foreach (var attributePair in attributeDict.Values)
        {
            HashSet<MemberDeclarationSyntax> propertys = [];

            var symbols = FilterSymbol(attributePair.PropertySymbols, [.. attributePair.ConstructorArguments]);

            foreach (ISymbol propertySymbol in symbols)
            {
                foreach (SyntaxReference syntaxReference in propertySymbol.DeclaringSyntaxReferences)
                {
                    var syntax = (MemberDeclarationSyntax)syntaxReference.GetSyntax();

                    propertys.Add(syntax);


                }


            }


            string hintName = $"{context.TargetSymbol.ToDisplayString()}.{attributePair.TypeSymbol.ToDisplayString()}.g.cs";



            var typeDeclaration = Unsafe.As<TypeDeclarationSyntax>(context.TargetNode)
                .WithMembers(List(propertys));
            var compilationUnit = context.SemanticModel.SyntaxTree.GetCompilationUnitRoot()
                                                                  .WithMembers(SingletonList<MemberDeclarationSyntax>(typeDeclaration));



            yield return (hintName, compilationUnit.NormalizeWhitespace().ToFullString());

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
