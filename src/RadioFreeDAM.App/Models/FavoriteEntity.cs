namespace RadioFreeDAM.App.Models;

public class FavoriteEntity
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string StationId { get; set; }

    public string Name { get; set; }

    public string StreamUrl { get; set; }

    public string ImageUrl { get; set; }

    public string Genre { get; set; }
}
