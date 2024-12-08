namespace API.Models;

public class ToDoItem
{
    public int Id { get; init; }
    public required string Title { get; init; }
    public bool IsComplited { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? CompletedAt { get; init; }

    public ToDoItem UpdateStatus(bool newStatus)
    {
        return new ToDoItem
        {
            Id = Id,
            Title = Title,
            IsComplited = newStatus,
            CreatedAt = CreatedAt,
            CompletedAt = newStatus ? DateTime.UtcNow : null
        };
    }
}
