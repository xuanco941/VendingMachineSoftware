namespace StyleX.DTOs
{
    public class AddCategoryModel
    {
        public string name { get; set; } = null!;
        public string description { get; set; } = string.Empty;
        public IFormFile file { get; set; } = null!;
    }
    public class UpdateCategoryModel
    {
        public int categoryID { get; set; }
        public string name { get; set; } = null!;
        public string description { get; set; } = string.Empty;
        public IFormFile file { get; set; } = null!;
    }
}
