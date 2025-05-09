using Aghanim.UtilityType.Generator.Models;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Aghanim.UtilityType.Generator;

internal abstract class Parse
{
    public SyntaxKind ParseTypeKind(TypeKind typeKind, bool isRecord) => (typeKind, isRecord) switch
    {
        (TypeKind.Class, false) => SyntaxKind.ClassDeclaration,
        (TypeKind.Class, true) => SyntaxKind.RecordDeclaration,
        (TypeKind.Struct, false) => SyntaxKind.StructDeclaration,
        (TypeKind.Struct, true) => SyntaxKind.RecordStructDeclaration,
        (TypeKind.Interface, _) => SyntaxKind.InterfaceDeclaration,
        _ => throw new ArgumentException("Invalid type kind", nameof(typeKind))
    };
    public abstract IEnumerable<ISymbol> GetSymbols(IEnumerable<ISymbol> symbols, ImmutableHashSet<string> constructorArguments);
    public ICollection<(string hitName, string source)> ParseSource(UtilityTypeDeclaration declaration)
    {
        var containingNamespace = declaration.TargetSymbol.ContainingNamespace;
        var fileUsings = containingNamespace.DeclaringSyntaxReferences.SelectMany(x => x.SyntaxTree.GetRoot().DescendantNodes().OfType<UsingDirectiveSyntax>());
        var namespaceUsing = containingNamespace.DeclaringSyntaxReferences.SelectMany(x => x.GetSyntax().ChildNodes().OfType<UsingDirectiveSyntax>());
        var targetSymbol = declaration.TargetSymbol;


        var propertySymbolDict = new Dictionary<string, HashSet<MemberDeclarationSyntax>>();
        var attributeDict =

            from attr in declaration.Attributes
            let TypeSymbol = attr.AttributeClass.TypeArguments[0]
            let ConstructorArguments = from typedConstant in attr.ConstructorArguments
                                       from innerTypedConstant in typedConstant.Values
                                       select innerTypedConstant.Value.ToString()
            select new { TypeSymbol, ConstructorArguments } into temp
            group temp by temp.TypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) into g
            let TypeSymbol = g.First().TypeSymbol
            select new { TypeSymbol, PropertySymbols = TypeSymbol.GetMembers().OfType<IPropertySymbol>(), ConstructorArguments = g.SelectMany(x => x.ConstructorArguments).ToImmutableHashSet() };




        HashSet<(string hitName, string source)> result = [];
        foreach (var attributePair in attributeDict)
        {
            HashSet<MemberDeclarationSyntax> propertys = [];

            var symbols = GetSymbols(attributePair.PropertySymbols, attributePair.ConstructorArguments);

            foreach (ISymbol propertySymbol in symbols)
            {
                foreach (SyntaxReference syntaxReference in propertySymbol.DeclaringSyntaxReferences)
                {
                    var syntax = (MemberDeclarationSyntax)syntaxReference.GetSyntax();

                    propertys.Add(syntax);


                }


            }


            string hintName = $"{targetSymbol.ToDisplayString()}.{attributePair.TypeSymbol.ToDisplayString()}.g.cs";


            SyntaxKind typeKind = ParseTypeKind(targetSymbol.TypeKind, targetSymbol.IsRecord);
            var typeDeclaration = TypeDeclaration(typeKind, targetSymbol.Name)
                .WithModifiers(TokenList(
                    Token(SyntaxKind.PartialKeyword)
                    .WithLeadingTrivia(ParseLeadingTrivia("#nullable enable \r #pragma warning disable CS8618 "))
                    ))
                .WithMembers(List(propertys));

            var compilationUnit = CompilationUnit().WithUsings(List(fileUsings)); ;
            if (containingNamespace.IsGlobalNamespace)
            {
                compilationUnit = compilationUnit
                    .WithMembers(SingletonList<MemberDeclarationSyntax>(typeDeclaration));
            }
            else
            {
                compilationUnit = compilationUnit
                    .WithMembers(SingletonList<MemberDeclarationSyntax>(NamespaceDeclaration(ParseName(containingNamespace.ToDisplayString()))
                    .WithUsings(List(namespaceUsing))
                    .WithMembers(SingletonList<MemberDeclarationSyntax>(typeDeclaration))));
            }



            result.Add((hintName, compilationUnit.NormalizeWhitespace().ToFullString()));

        }
        return result;
    }
}
