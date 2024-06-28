namespace StyleX.DTOs
{

    public class CreateOrder
    {
        public List<ItemOrder> itemOrders = new List<ItemOrder>();
    }

    public class SearhOrderModel
    {
        public string orderID { get; set; } = string.Empty;
        public string accountName { get; set;} = string.Empty;

    }
    public class ItemOrder
    {
        public int productID { get; set; }
        public int amount { get; set; }
    }

}
