using Microsoft.CodeAnalysis;

using System.Collections.Generic;
using System.Collections.Immutable;

namespace Aghanim.UtilityType.Generator;

internal class OmitParse : Parse
{
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
