using Coursework.PollBuilder.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Coursework.PollBuilder.Data
{
    // Tên lớp phải kết thúc bởi DbContext
    public class PollBuilderDbContext : DbContext
    {
        // Constructor hỗ trợ cho cơ chế DI (Dependency Injection)
        public PollBuilderDbContext(DbContextOptions<PollBuilderDbContext> options)
            : base(options)
        {
        }

        // Khai báo các DbSet. Mỗi dòng khai báo sẽ sinh ra 1 Table trong Database ở dạng số nhiều
        public DbSet<Poll> Polls { get; set; }
        public DbSet<Vote> Votes { get; set; }

        // Hàm cấu hình các ràng buộc dữ liệu (Constraint) bằng Fluent API
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình bảng Polls
            modelBuilder.Entity<Poll>(entity =>
            {
                entity.ToTable("Polls");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Code).IsRequired().HasMaxLength(15);
                entity.Property(e => e.Question).IsRequired().HasMaxLength(500);
            });

            // Cấu hình bảng Votes
            modelBuilder.Entity<Vote>(entity =>
            {
                entity.ToTable("Votes");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.VoterToken).IsRequired().HasMaxLength(255);

                // Cấu hình khóa ngoại (Foreign Key) cho quan hệ 1-n
                entity.HasOne(v => v.Poll)
                      .WithMany(p => p.Votes)
                      .HasForeignKey(v => v.PollId)
                      .OnDelete(DeleteBehavior.Cascade); // Ràng buộc: Xóa Poll sẽ xóa luôn toàn bộ Votes của nó
            });
        }
    }
}