namespace ClassLibrary
{
    public enum NotificationType
    {
        Info,
        Warning,
        Error,
        System
    }

    public enum SortNotificationBy
    {
        Date,
        Priority,
        Title
    }

    public class Notification
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Content { get; set; }
        public NotificationType Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Priority { get; set; }
        public bool IsRead { get; set; }
    }

    public class NotificationFilterOptions
    {
        public bool? IsRead { get; set; }
        public NotificationType[]? Types { get; set; }
        public string? SearchText { get; set; }
        public int? MinPriority { get; set; }
        public SortNotificationBy? SortBy { get; set; }
        public bool Descending { get; set; }
    }

    public static class NotificationFilterService
    {
        public static IEnumerable<Notification> FilterAndSort(
            IEnumerable<Notification> notifications,
            NotificationFilterOptions options)
        {
            if (notifications == null)
                throw new ArgumentNullException(nameof(notifications));

            var query = notifications.AsQueryable();

            // Фильтрация по прочитанности
            if (options.IsRead.HasValue)
                query = query.Where(n => n.IsRead == options.IsRead.Value);

            // Фильтрация по типам
            if (options.Types != null && options.Types.Any())
                query = query.Where(n => options.Types.Contains(n.Type));

            // Фильтрация по минимальному приоритету
            if (options.MinPriority.HasValue)
                query = query.Where(n => n.Priority >= options.MinPriority.Value);

            // Поиск по тексту (в заголовке и содержимом)
            if (!string.IsNullOrWhiteSpace(options.SearchText))
                query = query.Where(n =>
                    n.Title.Contains(options.SearchText, StringComparison.OrdinalIgnoreCase) ||
                    (n.Content != null && n.Content.Contains(options.SearchText, StringComparison.OrdinalIgnoreCase)));

            // Сортировка
            if (options.SortBy.HasValue)
            {
                query = options.SortBy.Value switch
                {
                    SortNotificationBy.Date => options.Descending
                        ? query.OrderByDescending(n => n.CreatedAt)
                        : query.OrderBy(n => n.CreatedAt),
                    SortNotificationBy.Priority => options.Descending
                        ? query.OrderByDescending(n => n.Priority)
                        : query.OrderBy(n => n.Priority),
                    SortNotificationBy.Title => options.Descending
                        ? query.OrderByDescending(n => n.Title)
                        : query.OrderBy(n => n.Title),
                    _ => query
                };
            }

            return query.ToList();
        }
    }
}
