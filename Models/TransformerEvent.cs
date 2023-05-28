namespace tgbotapi.Models;

public class TransformerEvent
{
    public Guid Id { get; set; }
    public Guid SubjectId { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; }
    public double Mark { get; set; }
    public string SubjectName { get; set; }
}