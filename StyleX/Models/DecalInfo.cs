using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StyleX.Models
{
    [Table("DecalInfo")]
    public class DecalInfo
    {
        [Key]
        public int DecalInfoID { get; set; }
        public string Image { get; set; } = null!;
        public string MeshUuid { get; set; } = null!;
        public double PositionX { get; set; }
        public double PositionY { get; set; }
        public double PositionZ { get; set; }
        public double OrientationX { get; set; }
        public double OrientationY { get; set; }
        public double OrientationZ { get; set; }

        public int RenderOrder { get; set; }
        public double Size { get; set; }


        public int CartItemID { get; set; }
        public CartItem CartItem { get; set; } = null!;
    }
}
