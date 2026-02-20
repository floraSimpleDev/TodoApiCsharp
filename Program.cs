var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(configure =>
{
  configure.DocumentName = "TodoApi";
  configure.Title = "Todo API v1";
  configure.Version = "v1";
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
  app.UseOpenApi();
  app.UseSwaggerUi();
}

var todos = new Dictionary<Guid, Todo>();

app.MapPost(
  "/todos",
  (CreateRequest request) =>
  {
    var todo = Todo.Create(request.Title);

    todos[todo.Id] = todo;

    return Results.Created($"/todos/{todo.Id}", Response.From(todo));
  }
);

app.MapPut(
  "/todos/{id:guid}",
  (Guid id, UpdateRequest request) =>
  {
    if (!todos.TryGetValue(id, out var todo))
    {
      return Results.NotFound("Todo not found.");
    }

    var updated = todo.Update(request.Title, request.IsCompleted);

    todos[id] = updated;

    return Results.Ok(Response.From(updated));
  }
);

app.MapGet(
  "/todos/{id}",
  (Guid? id) =>
  {
    if (!todos.TryGetValue(id.Value, out var todo))
    {
      return Results.NotFound("Todo not found.");
    }

    return Results.Ok(Response.From(todo));
  }
);

app.MapGet(
  "/todos",
  () =>
  {
    return Results.Ok(todos.Values.Select(Response.From));
  }
);

app.MapDelete(
  "/todos/{id:guid}",
  (Guid id) =>
  {
    if (!todos.Remove(id))
    {
      return Results.NotFound("Todo not found.");
    }

    return Results.NoContent();
  }
);

app.MapGet("/", () => "Hello World!");

app.Run();

public class Todo
{
  public Guid Id { get; init; }
  public string Title { get; private set; }
  public bool IsCompleted { get; private set; }
  public DateTime CreatedAt { get; init; }
  public DateTime? UpdatedAt { get; private set; }
  public DateTime? CompletedAt { get; private set; }

  public static Todo Create(string title)
  {
    return new Todo
    {
      Id = Guid.NewGuid(),
      Title = title,
      IsCompleted = false,
      CreatedAt = DateTime.UtcNow,
    };
  }

  public Todo Update(string title, bool isCompleted)
  {
    Title = title;
    UpdatedAt = DateTime.UtcNow;

    if (isCompleted && !IsCompleted)
    {
      IsCompleted = isCompleted;
      CompletedAt = DateTime.UtcNow;
    }
    else if (!isCompleted && IsCompleted)
    {
      IsCompleted = isCompleted;
      CompletedAt = null;
    }

    return this;
  }

  public void Complete()
  {
    if (IsCompleted)
    {
      return;
    }
    IsCompleted = true;
    CompletedAt = DateTime.UtcNow;
    UpdatedAt = DateTime.UtcNow;
  }
}

public record CreateRequest
{
  public required string Title { get; init; }
}

public record UpdateRequest
{
  public required string Title { get; init; }
  public bool IsCompleted { get; init; }
}

public record Response
{
  public Guid Id { get; init; }
  public required string Title { get; init; }
  public bool IsCompleted { get; init; }
  public DateTime? CreatedAt { get; init; }
  public DateTime? UpdatedAt { get; init; }
  public DateTime? CompletedAt { get; init; }

  public static Response From(Todo todo) =>
    new()
    {
      Id = todo.Id,
      Title = todo.Title,
      IsCompleted = todo.IsCompleted,
      CreatedAt = todo.CreatedAt,
      UpdatedAt = todo.UpdatedAt,
      CompletedAt = todo.CompletedAt,
    };
}
