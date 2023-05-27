using Microsoft.EntityFrameworkCore;
using tgbotapi.Models;

namespace tgbotapi.Data;

public class Context: DbContext
{
    public Context(DbContextOptions<Context> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Subject> Subjects { get; set; } = null!;
    public DbSet<Event> Events { get; set; } = null!;
}