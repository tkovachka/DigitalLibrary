namespace LibraryApplication.Domain.DTO
{
    public class ApiResponse
    {
        public int TotalItems { get; set; }
        public List<VolumeItem>? Items { get; set; }
    }
}
