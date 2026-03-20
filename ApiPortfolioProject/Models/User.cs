namespace ApiPortfolioProject.Models
{
    public class User
    {
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public bool IsActive { get; set; }
        public string? CreatedOn { get; set; }
        public string? ModifiedOn { get; set; } = string.Empty;
    }
}