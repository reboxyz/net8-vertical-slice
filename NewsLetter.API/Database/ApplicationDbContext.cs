using Microsoft.EntityFrameworkCore;
using NewsLetter.API.Entities;

namespace NewsLetter.API.Database;

public class ApplicationDbContext: DbContext
{
    public ApplicationDbContext(DbContextOptions options): base(options)
    {        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Article>(builder =>
            builder.OwnsOne(a => a.Tags, tagsBuilder => tagsBuilder.ToJson())
        );
    }

    public DbSet<Article> Articles { get; set; }

}
