namespace Aghanim.UtilityType;
[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
public class MixinAttribute<T> : Attribute
{
    public MixinAttribute(params string[] args)
    {
        _ = args;
    }
   
}
