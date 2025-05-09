namespace Aghanim.UtilityType;

[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
public class OmitAttribute<T> : Attribute
{

    public OmitAttribute(params string[] args)
    {
        _ = args;
    }

}
