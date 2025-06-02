using Aghanim.UtilityType;

namespace UtilityType.Sample
{
    public class One
    {
        public string? Name { get; set; }
        public int Id { get; set; }
    }

    [Pick<One>("Name")]
    public partial class Two
    {
        public long Code { get; set; }
    }


}
