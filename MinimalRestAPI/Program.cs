using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

string jsonFilePath = "books.json";

List<Book> ReadBooksFromFile()
{
    if (!File.Exists(jsonFilePath)) return new List<Book>();
    var jsonData = File.ReadAllText(jsonFilePath);
    return JsonSerializer.Deserialize<List<Book>>(jsonData) ?? new List<Book>();
}

void WriteBooksToFile(List<Book> books)
{
    var jsonData = JsonSerializer.Serialize(books, new JsonSerializerOptions { WriteIndented = true });
    File.WriteAllText(jsonFilePath, jsonData);
}

if (!File.Exists(jsonFilePath))
{
    File.WriteAllText(jsonFilePath, "[]");
}

var app = builder.Build();

app.MapGet("/books", () =>
{
    var books = ReadBooksFromFile();
    return Results.Ok(books);
});

app.MapGet("/books/{id:int}", (int id) =>
{
    var books = ReadBooksFromFile();
    var book = books.FirstOrDefault(b => b.Id == id);
    return book is not null ? Results.Ok(book) : Results.NotFound();
});

app.MapPost("/books", (Book book) =>
{
    var books = ReadBooksFromFile();

    if (books.Any(b => b.Id == book.Id))
        return Results.BadRequest("A book with the same ID already exists.");

    books.Add(book);
    WriteBooksToFile(books);
    return Results.Created($"/books/{book.Id}", book);
});

app.MapPut("/books/{id:int}", (int id, Book updatedBook) =>
{
    var books = ReadBooksFromFile();
    var book = books.FirstOrDefault(b => b.Id == id);
    if (book is null) return Results.NotFound();

    book.Title = updatedBook.Title;
    book.Author = updatedBook.Author;
    book.Year = updatedBook.Year;

    WriteBooksToFile(books);
    return Results.NoContent();
});

app.MapDelete("/books/{id:int}", (int id) =>
{
    var books = ReadBooksFromFile();
    var book = books.FirstOrDefault(b => b.Id == id);
    if (book is null) return Results.NotFound();

    books.Remove(book);
    WriteBooksToFile(books);
    return Results.NoContent();
});

app.Run();

public class Book
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Author { get; set; }
    public int Year { get; set; }
}