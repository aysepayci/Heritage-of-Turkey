using Heritage_of_Turkey.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Heritage_of_Turkey.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet ekle
        // DbSet'ler, veritabanında karşılık gelen tabloları temsil eder. Her bir DbSet, uygulamanızdaki bir varlık türünü temsil eder.
        public DbSet<Category> Categories { get; set; }
        public DbSet<Museum> Museums { get; set; }
        public DbSet<Ruin> Ruins { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }
        public DbSet<MuseumReview> MuseumReviews { get; set; }
        public DbSet<RuinReview> RuinReviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)// OnModelCreating metodu, Entity Framework Core'un model oluşturma sürecinde çağrılır. Bu metodun içinde, veritabanı tablolarının ilişkilerini, kısıtlamalarını ve diğer yapılandırmalarını tanımlayabilirsiniz.
        {
            base.OnModelCreating(modelBuilder); // IdentityDbContext'in OnModelCreating metodunu çağırarak, Identity tablolarının yapılandırılmasını sağlıyoruz.

            // Relationships
            // Bir Category'nin birden fazla Museum ve Ruin'e sahip olabileceği ilişkileri tanımlanır. Ayrıca, bir Museum veya Ruin'in yalnızca bir Category'ye ait olabileceği belirtilir.
            modelBuilder.Entity<Museum>()
                .HasOne(m => m.Category)
                .WithMany(c => c.Museums)
                .HasForeignKey(m => m.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            // Bir Museum veya Ruin silindiğinde, ona ait Favorite kayıtlarının da silinmesini sağlamak için Cascade delete davranışı tanımlanır.
            modelBuilder.Entity<Ruin>()
                .HasOne(r => r.Category)
                .WithMany(c => c.Ruins)
                .HasForeignKey(r => r.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            // Bir User'ın birden fazla Favorite kaydı olabileceği, ancak her Favorite kaydının yalnızca bir User'a ait olabileceği ilişkisi tanımlanır. Benzer şekilde, bir Favorite kaydının ya bir Museum'a ya da bir Ruin'e ait olabileceği belirtilir.
            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.User)
                .WithMany(u => u.Favorites)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            // Bir Favorite kaydının ya bir Museum'a ya da bir Ruin'e ait olabileceği belirtilir. Bu, her iki ilişki için de Cascade delete davranışı tanımlanır, böylece bir Museum veya Ruin silindiğinde ona ait Favorite kayıtları da silinir.
            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.Museum)
                .WithMany(m => m.Favorites)
                .HasForeignKey(f => f.MuseumId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.Ruin)
                .WithMany(r => r.Favorites)
                .HasForeignKey(f => f.RuinId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MuseumReview>()
                .HasOne(r => r.Museum)
                .WithMany(m => m.Reviews)
                .HasForeignKey(r => r.MuseumId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MuseumReview>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RuinReview>()
                .HasOne(r => r.Ruin)
                .WithMany(m => m.Reviews)
                .HasForeignKey(r => r.RuinId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RuinReview>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ContactMessage>()
                .Property(m => m.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<ContactMessage>()
                .HasOne(m => m.User)
                .WithMany(u => u.ContactMessages)
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Seed Data
            // Veritabanına başlangıç verileri eklemek için HasData metodu kullanılır. Bu örnekte, Category tablosuna 6 farklı kategori eklenir. Her kategori, benzersiz bir CategoryId, bir CategoryName, bir Description, IsActive durumunu ve CreatedDate değerini içerir.
            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryId = 1, CategoryName = "Archaeology Museum", Description = "Museums featuring archaeological artifacts", IsActive = true, CreatedDate = new DateTime(2024, 1, 1) },
                new Category { CategoryId = 2, CategoryName = "Art Museum", Description = "Museums displaying paintings and sculptures", IsActive = true, CreatedDate = new DateTime(2024, 1, 1) },
                new Category { CategoryId = 3, CategoryName = "Ethnography Museum", Description = "Museums showcasing cultural heritage", IsActive = true, CreatedDate = new DateTime(2024, 1, 1) },
                new Category { CategoryId = 4, CategoryName = "Ancient City", Description = "Ruins of ancient cities", IsActive = true, CreatedDate = new DateTime(2024, 1, 1) },
                new Category { CategoryId = 5, CategoryName = "Ancient Theater", Description = "Ancient amphitheaters", IsActive = true, CreatedDate = new DateTime(2024, 1, 1) },
                new Category { CategoryId = 6, CategoryName = "Temple Ruins", Description = "Ancient temples and religious sites", IsActive = true, CreatedDate = new DateTime(2024, 1, 1) }
            );
            // Check Constraint
            // Favorite tablosunda, her kaydın ya bir Museum'a ya da bir Ruin'e ait olabileceği, ancak ikisine birden ait olamayacağı bir check constraint tanımlanır. Bu, veritabanı düzeyinde veri bütünlüğünü sağlamak için kullanılır.
            modelBuilder.Entity<Favorite>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_Favorite_MuseumOrRuin",
                    "([MuseumId] IS NOT NULL AND [RuinId] IS NULL) OR ([MuseumId] IS NULL AND [RuinId] IS NOT NULL)"
                ));
        }       
    }
}
