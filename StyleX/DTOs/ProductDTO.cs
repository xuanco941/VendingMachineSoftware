using StyleX.Models;

namespace StyleX.DTOs
{
    public class SearchProductModel
    {
        public int categoryID { get; set; } = 0;
        public int status { get; set; } = 0;
    }
    public class SearchProductModel2
    {
        public int categoryID { get; set; } = 0;
        public int sale { get; set; } = 0;
        public string nameProduct { get; set; } = string.Empty;

    }
    public class ProductSettingsWithMaterial
    {
        public int ProductSettingID { get; set; }
        public string ProductPartNameDefault { get; set; } = string.Empty;
        public string ProductPartNameCustom { get; set; } = string.Empty;
        public bool IsDefault { get; set; } = false;
        public string NameMaterialDefault { get; set; } = string.Empty;

    }
    public class AddProductModel
    {
        public string name { get; set; } = null!;
        public IFormFile fileModel { get; set; } = null!;
        public IFormFile file { get; set; } = null!;

		public string description { get; set; } = string.Empty;
        public double price { get; set; }
        public double sale { get; set; }
        public bool status { get; set; }
        public int numberAvailable { get; set; }


    }
    public class AddMatProductPart
    {
        public int productID { get; set; }
        public string name { get; set; } = string.Empty;
        public IFormFile? aoMap { get; set; }
        public IFormFile? normalMap { get; set; }
        public IFormFile? roughnessMap { get; set; }
        public IFormFile? metalnessMap { get; set; }
    }
    public class UpdateProductModel
    {
        public int productID { get; set; }
        public string name { get; set; } = null!;
        public IFormFile? file { get; set; }
		public string description { get; set; } = string.Empty;
        public double price { get; set; }
        public double sale { get; set; }
        public bool status { get; set; }
        public int numberAvailable { get; set; }

    }
    public class SettingProductModel
    {
        public int productSettingID { get; set; }

        public string productPartNameCustom { get; set; } = string.Empty;
        public bool isDefault { get; set; }
        public string nameMaterialDefault { get; set; } = string.Empty;

        public List<int> materials { get; set; } = new List<int>();
    }
}
