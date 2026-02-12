namespace RadioFreeDAM.Api.Data.Entities;

public class UserEntity
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public string Email { get; set; }
    public DateTime? CreatedAt { get; set; }
}
