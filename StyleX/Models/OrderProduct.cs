using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StyleX.Models
{

    [Table("OrderProduct")]
    public class OrderProduct
    {
        [Key]
        public int CartItemID { get; set; }
        public int Amount { get; set; }
        public double Sale { get; set; } // khuyến mại tại thời điểm đặt đơn
		public double Price { get; set; } // giá tiền sp tại thời điểm đặt đơn

		public int OrderID { get; set; }
        public Order Order { get; set; } = null!;
        public int ProductID { get; set; }
        public Product Product { get; set; } = null!;
    }
}
