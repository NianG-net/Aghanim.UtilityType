using Microsoft.CodeAnalysis;

using System.Collections.Immutable;

namespace Aghanim.UtilityType.Generator.Models;

public sealed class UtilityTypeDeclaration(ImmutableArray<AttributeData> attributes, INamedTypeSymbol namedTypeSymbol)
{
    public ImmutableArray<AttributeData> Attributes => attributes;
    public INamedTypeSymbol TargetSymbol => namedTypeSymbol;
}




