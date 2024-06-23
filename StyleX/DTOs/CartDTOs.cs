namespace StyleX.DTOs
{
    public class AddToCartModel
    {
        public int productID {  get; set; }
        public string? size { get; set; } = "";
        public int? amount { get; set; } = 1;

    }
}
