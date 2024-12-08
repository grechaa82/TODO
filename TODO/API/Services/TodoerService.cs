using API.Models;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

public class TodoerService : Todoer.TodoerBase
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<TodoerService> _logger;

    public TodoerService(
        AppDbContext dbContext,
        ILogger<TodoerService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public override Task<TodoListResponse> GetAllTodo(TodoFilterRequest request, ServerCallContext context)
    {
        var toDoItems = _dbContext.ToDoItems
            .AsNoTracking()
            .Where(x => x.IsComplited == request.IsCompleted)
            .Select(x => new TodoItemResponse
            {
                Id = x.Id,
                Title = x.Title,
                IsCompleted = x.IsComplited,
                CreatedAt = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(x.CreatedAt.ToUniversalTime()),
                CompletedAt = x.CompletedAt.HasValue
                    ? Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(x.CompletedAt.Value.ToUniversalTime())
                    : null
            })
            .ToList();

        return Task.FromResult(new TodoListResponse { Items = { toDoItems } });
    }

    public override async Task<TodoResponse> CreateTodo(TodoItemRequest request, ServerCallContext context)
    {
        var newToDoitem = new ToDoItem()
        {
            Title = request.Title,
            IsComplited = false,
            CreatedAt = DateTime.UtcNow,
        };

        await _dbContext.ToDoItems.AddAsync(newToDoitem);
        var created = await _dbContext.SaveChangesAsync();

        return await Task.FromResult(new TodoResponse
        {
            IsSuccess = created > 0
        });
    }

    public override async Task<TodoResponse> UpdateStatusTodo(UpdateStatusTodoRequest request, ServerCallContext context)
    {
        var toDoItem = await _dbContext.ToDoItems.FindAsync(request.Id);
        if (toDoItem == null)
        {
            return new TodoResponse { IsSuccess = false };
        }

        var newToDoItem = toDoItem.UpdateStatus(request.IsCompleted);

        _dbContext.ToDoItems.Update(toDoItem);
        var updated = await _dbContext.SaveChangesAsync();

        return await Task.FromResult(new TodoResponse
        {
            IsSuccess = updated > 0
        });
    }

    public override async Task<TodoResponse> DeleteTodo(TodoIdRequest request, ServerCallContext context)
    {
        var todoItem = await _dbContext.ToDoItems.FindAsync(request.Id);

        if (todoItem == null)
        {
            return new TodoResponse { IsSuccess = false };
        }

        _dbContext.ToDoItems.Remove(todoItem);
        var deleted = await _dbContext.SaveChangesAsync();

        return new TodoResponse { IsSuccess = deleted > 0 };
    }
}
