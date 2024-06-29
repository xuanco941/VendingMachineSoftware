using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StyleX.Models
{
    [Table("Order")]
    public class Order
    {
        [Key]
        public int OrderID { get; set; }
        public double BasePrice { get; set; } //tổng tiền
        public double NetPrice { get; set; } //số tiền phải trả
        public string? Description { get; set; } = string.Empty;
        public string? OrderDetail { get; set; } = string.Empty;
        public int Status { get; set; } = 0; //0.chờ thanh toán, 1 thành công, 2 thất bại, 3 hoàn trả
        public DateTime CreateAt { get; set; } = DateTime.Now;
        public DateTime UpdateAt { get; set; } = DateTime.Now;
    }
}
