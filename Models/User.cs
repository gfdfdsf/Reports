namespace Proyecto_Integrador_DevOps.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Area { get; set; } = null!;
        public DateTime CreatedDate { get; set; }
        public bool IsAdmin { get; set; }
    }
}
