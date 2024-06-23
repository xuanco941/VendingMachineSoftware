using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StyleX.Models
{
    [Table("ProductSettingMaterial")]
    public class ProductSettingMaterial
    {
        [Key]
        public int ProductSettingMaterialID { get; set; }
        public int ProductSettingID { get; set; }
        public ProductSetting ProductSetting { get; set; } = null!;
        public int MaterialID { get; set; }
        public Material Material { get; set; } = null!;
    }
}
