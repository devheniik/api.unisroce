namespace tgbotapi.Models;

public class User
{
    public Guid Id { get; set; }
    public string ChatId { get; set; }
    public string? CurrentAction { get; set; }
    public string? GroupId { get; set; }
    public string? SubjectId { get; set; }
    public string? TempEventName { get; set; }
}