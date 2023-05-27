namespace tgbotapi.Models;

public class Paging
{
    public int pageCount { get; set; }
    public int totalItemCount { get; set; }
    public int pageNumber { get; set; }
    public int pageSize { get; set; }
    public bool hasPreviousPage { get; set; }
    public bool hasNextPage { get; set; }
    public bool isFirstPage { get; set; }
    public bool isLastPage { get; set; }
    public int firstItemOnPage { get; set; }
    public int lastItemOnPage { get; set; }
}

public class Item
{
    public string id { get; set; }
    public string name { get; set; }
    public string faculty { get; set; }
}

public class GroupResponse
{
    public Paging paging { get; set; }
    public Item[] data { get; set; }
}
