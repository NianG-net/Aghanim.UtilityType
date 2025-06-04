using Aghanim.UtilityType;

namespace UtilityType.Sample;

public class Entity
{
    public string? Name { get; set; }
    public int Id { get; set; }

    public DateTime CreateOn { get; set; }
}


[Omit<Entity>("CreateOn")]
[Omit<Entity>("Name")]
public partial class DtoDEMO;