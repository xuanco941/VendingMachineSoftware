namespace StyleX.DTOs
{
    public class AddMaterialModel
    {
        public string name { get; set; } = null!;
        public IFormFile file { get; set; } = null!;
        public IFormFile aoMap { get; set; } = null!;
        public IFormFile normalMap { get; set; } = null!;
        public IFormFile roughnessMap { get; set; } = null!;
        public IFormFile metalnessMap { get; set; } = null!;

        public bool status { get; set; }
        public bool isDecal { get; set; }

    }
    public class UpdateMaterialModel
    {
        public int materialID { get; set; }
        public string name { get; set; } = null!;
        public IFormFile file { get; set; } = null!;
        public IFormFile aoMap { get; set; } = null!;
        public IFormFile normalMap { get; set; } = null!;
        public IFormFile roughnessMap { get; set; } = null!;
        public IFormFile metalnessMap { get; set; } = null!;
        public bool status { get; set; }
        public bool isDecal { get; set; }

    }
}
