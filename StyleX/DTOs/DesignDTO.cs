namespace StyleX.DTOs
{
    public class SaveDesignInfoModel
    {
        public int designInfoID { get; set; } // nếu = 0 thì tạo mới, khác 0 thì cập nhật
        public int cartItemID { get; set; }
        public IFormFile imageCartItem { get; set; } = null!;
        public string designName { get; set; } = string.Empty; // tên của bộ phận trên quần áo
        public string color { get; set; } = "#ffffff";
        public IFormFile? imageTexture { get; set; } //color image url
        public double textureScale { get; set; }
        public string nameMaterial { get; set; } = string.Empty;
        public IFormFile? aoMap { get; set; } //mô phỏng cách ánh sáng tương tác với môi trường xung quanh
        public IFormFile? normalMap { get; set; } //sử dụng để tạo ra chi tiết đồ họa
        public IFormFile? roughnessMap { get; set; } // xác định độ nhám
        public IFormFile? metalnessMap { get; set; } // xác định vị trí của các vùng kim loại trong vật liệu
    }

    public class SaveDecalInfoModel
    {
        public IFormFile image { get; set; } = null!;
        public string meshUuid { get; set; } = null!;
        public double positionX { get; set; }
        public double positionY { get; set; }
        public double positionZ { get; set; }
        public double orientationX { get; set; }
        public double orientationY { get; set; }
        public double orientationZ { get; set; }

        public int renderOrder { get; set; }
        public double size { get; set; }

        public int cartItemID { get; set; }
    }
}
