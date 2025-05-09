using Aghanim.UtilityType.Generator.Models;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Collections.Generic;
using System.Collections.Immutable;


namespace Aghanim.UtilityType.Generator;

[Generator(LanguageNames.CSharp)]
public class OmitGenerator : Parse, IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
#if DEBUG_SOURCE
        System.Diagnostics.Debugger.Launch();
#endif

        
        var omitDeclaration = context.SyntaxProvider.ForAttributeWithMetadataName(
                "Aghanim.UtilityType.OmitAttribute`1",
                static (s, _) => s is TypeDeclarationSyntax,
                static (ctx, _) => (ctx.Attributes, ctx.TargetSymbol)

            )
            .Where(x => x.TargetSymbol is INamedTypeSymbol)
            .Select((x, _) => new UtilityTypeDeclaration(x.Attributes, (INamedTypeSymbol)x.TargetSymbol));


        context.RegisterSourceOutput(omitDeclaration, (sourceContext, utilityTypeDeclaration) =>
        {
            var result = ParseSource(utilityTypeDeclaration);
            foreach (var (hitName, source) in result)
            {
                sourceContext.AddSource(hitName, source);
            }

        });
    }
    public override IEnumerable<ISymbol> FilterSymbol(IEnumerable<ISymbol> symbols, ImmutableHashSet<string> constructorArguments)
    {
        foreach (var item in symbols)
        {
            if (!constructorArguments.Contains(item.Name))
            {
                yield return item;
            }
        }
    }

}


