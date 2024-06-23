using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StyleX.Models
{
    //thiết lặp khi upload product
    [Table("ProductSetting")]
    public class ProductSetting
    {
        [Key]
        public int ProductSettingID { get; set; }
        //tên của bộ phận trên product
        public string ProductPartNameDefault { get; set; } = string.Empty;
        //tên hiển thị của bộ phận trên product
        public string ProductPartNameCustom { get; set; } = string.Empty;

        //mặc định sẽ đặt về màu vải trắng , false sẽ giữ màu từ model 
        //pbrMetallicRoughness.baseColorTexture.setTexture(null)
        public bool IsDefault { get; set; } = false;
        public string NameMaterialDefault { get; set; } = string.Empty; //tên loại chất liệu mặc định
        //public string Map { get; set; } = string.Empty;
        public string AoMap { get; set; } = string.Empty;
        public string NormalMap { get; set; } = string.Empty;
        public string RoughnessMap { get; set; } = string.Empty;
        public string MetalnessMap { get; set; } = string.Empty;
        //public string DisplacementMap { get; set; } = string.Empty;

        public int ProductID { get; set; }
        public Product Product { get; set; } = null!;

    }
}
