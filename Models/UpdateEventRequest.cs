namespace tgbotapi.Requests;

public class UpdateEventRequest
{
    public string Name { get; set; }
    public double Mark { get; set; }
    public Guid SubjectId { get; set; }
}