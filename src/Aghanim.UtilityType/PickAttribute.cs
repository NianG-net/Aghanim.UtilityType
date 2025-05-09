namespace Aghanim.UtilityType;
[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
public class PickAttribute<T> : Attribute 
{

    public PickAttribute(params string[] args)    
    {
        _ = args;
    }

}

