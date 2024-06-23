namespace StyleX.DTOs
{
    public class WarehouseDTO
    {
        public class AddWarehouseModel
        {
            public int productID { get; set; }
            public int amount { get; set; }
            public string size { get; set; } = null!;
        }
        public class UpdateWarehouseModel
        {
            public int warehouseID { get; set; }
            public int amount { get; set; }
            public string size { get; set; } = null!;
        }
    }
}
