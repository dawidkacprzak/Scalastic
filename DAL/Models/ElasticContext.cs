using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace DAL.Models
{
    public class ElasticContext : DbContext
    {
        public ElasticContext()
        {
        }

        public ElasticContext(DbContextOptions<ElasticContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Queries> Queries { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Data Source=51.254.205.149;Initial Catalog=Elastic;User ID=rekurencja;Password=Hermetyzacj4!");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.6-servicing-10079");

            modelBuilder.Entity<Queries>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.CountQuery)
                    .IsRequired()
                    .HasColumnType("text");

                entity.Property(e => e.IndexName)
                    .IsRequired()
                    .HasColumnType("text");

                entity.Property(e => e.MinutePeriod).HasDefaultValueSql("((10))");

                entity.Property(e => e.WhereQuery)
                    .IsRequired()
                    .HasColumnType("text");
            });

        }
    }
}
