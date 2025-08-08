namespace ApiWeb.Application.DTOs;

public enum SortBy
{
    Recent,
    Top
}

public class FeedQuery
{
    public SortBy SortBy { get; set; } = SortBy.Recent;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}