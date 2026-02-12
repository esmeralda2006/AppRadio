namespace RadioFreeDAM.Api.Data.Entities;

public class RadioStationEntity
{
public string Id { get; set; }
    public string Name { get; set; }
    public string Url { get; set; }
    public string ImageUrl { get; set; }
    public string Genre { get; set; }
    public string Country { get; set; }
    public string Tags { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }}
