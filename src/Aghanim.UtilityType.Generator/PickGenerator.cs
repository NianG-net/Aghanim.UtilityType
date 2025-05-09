using Aghanim.UtilityType.Generator.Models;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Aghanim.UtilityType.Generator;

[Generator(LanguageNames.CSharp)]
public class PickGenerator : Parse, IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
#if DEBUG_SOURCE
            System.Diagnostics.Debugger.Launch();
#endif

        var pickDeclaration = context.SyntaxProvider.ForAttributeWithMetadataName(
            "Aghanim.UtilityType.PickAttribute`1",
            static (s, _) => s is TypeDeclarationSyntax,
            static (ctx, _) => (ctx.Attributes, ctx.TargetSymbol)
            )
            .Where(x => x.TargetSymbol is INamedTypeSymbol)
            .Select((x, _) => new UtilityTypeDeclaration(x.Attributes, (INamedTypeSymbol)x.TargetSymbol));


        context.RegisterSourceOutput(pickDeclaration, (sourceContext, utilityTypeDeclaration) =>
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
            if (constructorArguments.Contains(item.Name))
            {
                yield return item;
            }
        }
    }


}
