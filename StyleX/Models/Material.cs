using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StyleX.Models
{
    //lấy ảnh chất liệu trên https://freepbr.com/c/cloth-fabric/
    [Table("Material")]
    public class Material
    {
        [Key]
        public int MaterialID { get; set; }
        public string Name { get; set; } = null!;
        public string Url { get; set; } = null!; //link ảnh preview vật liệu
        public string AoMap { get; set; } = string.Empty;//mô phỏng cách ánh sáng tương tác với môi trường xung quanh
        public string NormalMap { get; set; } = string.Empty; //sử dụng để tạo ra chi tiết đồ họa
        public string RoughnessMap { get; set; } = string.Empty;// xác định độ nhám
        public string MetalnessMap { get; set; } = string.Empty;// xác định vị trí của các vùng kim loại trong vật liệu
        //public string Map { get; set; } = string.Empty; //màu sắc cơ bản của vật liệu mà không có thông tin về ánh sáng hoặc bóng.
        //public string DisplacementMap { get; set; } = string.Empty;//thông tin về chiều cao của bề mặt vật liệu
        public bool Status { get; set; } //=false thì vật liệu này ngừng hđ
        public bool IsDecal { get; set; } = true; //=true thì có thể decal


    }
}
