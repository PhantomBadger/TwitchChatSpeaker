public class TTSEmote
{
    public TTSEmote(string id, string name, string imageUrl)
    {
        ID = id;
        Name = name;
        ImageUrl = imageUrl;
    }
    
    public string ID { get; set; }
    public string Name { get; set; }
    public string ImageUrl { get; set; }
}