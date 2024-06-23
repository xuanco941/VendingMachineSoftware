using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StyleX.Models
{
    //chỉ số design của từng bộ phận trên product cart item
    [Table("DesignInfo")]
    public class DesignInfo
    {
        [Key]
        public int DesignInfoID { get; set; }
        public string DesignName { get; set; } = string.Empty; // tên của bộ phận trên quần áo

        //color
        public string Color { get; set; } = "#ffffff";

        public string? ImageTexture { get; set; } //color image url
        public double? TextureScale { get; set; }

        //material
        public string NameMaterial { get; set; } = string.Empty;
        public string? AoMap { get; set; } = string.Empty;//mô phỏng cách ánh sáng tương tác với môi trường xung quanh
        public string? NormalMap { get; set; } = string.Empty; //sử dụng để tạo ra chi tiết đồ họa
        public string? RoughnessMap { get; set; } = string.Empty;// xác định độ nhám
        public string? MetalnessMap { get; set; } = string.Empty;// xác định vị trí của các vùng kim loại trong vật liệu

        public int CartItemID { get; set; }
        public CartItem CartItem { get; set; } = null!;

    }
}
