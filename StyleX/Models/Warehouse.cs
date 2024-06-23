using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StyleX.Models
{
    [Table("Warehouse")]
    public class Warehouse
    {
        [Key]
        public int WarehouseID { get; set; }
        public int Amount { get; set; }
        public string Size { get; set; } = null!;
        public int ProductID { get; set; }
        public Product Product { get; set; } = null!;
    }
}
