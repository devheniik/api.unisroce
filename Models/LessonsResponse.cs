namespace tgbotapi.Models;

public class Pair
{
    public string teacherName { get; set; }
    public string lecturerId { get; set; }
    public string type { get; set; }
    public string time { get; set; }
    public string name { get; set; }
    public string place { get; set; }
    public string tag { get; set; }
}

public class WeekData
{
    public string day { get; set; }
    public List<Pair> pairs { get; set; }
}

public class ScheduleData
{
    public string groupCode;
    public List<WeekData> scheduleFirstWeek { get; set; }
    public List<WeekData> scheduleSecondWeek { get; set; }
}

public class LessonsResponse
{
    public object paging { get; set; }
    public ScheduleData data { get; set; }
}