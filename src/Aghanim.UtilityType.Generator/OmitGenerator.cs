using Microsoft.CodeAnalysis;

using System.Collections.Generic;
using System.Collections.Immutable;


namespace Aghanim.UtilityType.Generator;

[Generator(LanguageNames.CSharp)]
public class OmitGenerator : UtilityTypeGenerator
{
    public override string AttributeFullName => "Aghanim.UtilityType.OmitAttribute`1";

    public override string DefaultOptionKey => "DefaultOmitProperty";

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


