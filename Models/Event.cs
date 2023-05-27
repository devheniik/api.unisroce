namespace tgbotapi.Models;

public class Event
{
    public Guid Id { get; set; }
    public Guid SubjectId { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; }
    public double Mark { get; set; }
}