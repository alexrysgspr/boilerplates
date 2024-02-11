namespace Si.IdCheck.Workers.Application.Helpers;
public class PagingHelpers
{
    public static int GetPageCount(int itemsCount, int pageSize)
    {
        return (itemsCount + pageSize - 1) / pageSize;
    }
}