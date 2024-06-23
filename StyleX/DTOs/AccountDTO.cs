namespace StyleX.DTOs
{
    public class SearchAccountModel
    {
        public string? accountName { get; set; } = string.Empty;
        public int isActive { get; set; } = 0;
    }
    public class UpdateAccountModel
    {
        public int accountID { get; set; }
        public string? fullName { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
        public string? phoneNumber { get; set; } = string.Empty;
        public string? address { get; set; } = string.Empty;
        public int numberPlayGame { get; set; } = 0;
    }
}
