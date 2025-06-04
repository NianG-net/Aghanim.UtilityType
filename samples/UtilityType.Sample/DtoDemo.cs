using Aghanim.UtilityType;

namespace UtilityType.Sample;


public class Entity(int C)
{
    public string? Name { get; set; }
    public int Id { get; set; }

    public DateTime CreateOn { get; set; }
}

public interface IEntity(int C)
{
    string? Name { get; set; }
    int Id { get; set; }

    public DateTime CreateOn { get; set; }
}



public record R1(string? Name, int Id);
//[Omit<R1>("CreateOn")]
[Omit<IEntity>("Name")]
public partial class DtoDEMO;