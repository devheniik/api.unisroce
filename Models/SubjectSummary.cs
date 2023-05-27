namespace tgbotapi.Models;

public class SubjectSummary
{
    
    public string Name { get; set; }
    public double Total { get; set; }
    
    public SubjectSummary(string name, double total)
    {
        Name = name;
        Total = total;
    }
}