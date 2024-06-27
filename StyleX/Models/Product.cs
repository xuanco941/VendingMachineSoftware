using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StyleX.Models
{
    [Table("Product")]
    public class Product
    {
        [Key]
        public int ProductID { get; set; }
        public string ModelUrl { get; set; } = null!; // url model
        public string PosterUrl { get; set; } = null!; // ảnh hiển thị của product;  
		//public string? PosterDesignUrl1 { get; set; } // ảnh hiển thị của product;  
		//public string? PosterDesignUrl2 { get; set; } // ảnh hiển thị của product;  
        public int NumberAvailable { get; set; }
		public string Name { get; set; } = null!;
        public string? Description { get; set; } = string.Empty;
        public double Price { get; set; }
        public double Sale { get; set; } // % giảm giá
        public bool Status { get; set; } = true; //true = đang bán
        public DateTime CreateAt { get; set; } = DateTime.Now;

    }
}
