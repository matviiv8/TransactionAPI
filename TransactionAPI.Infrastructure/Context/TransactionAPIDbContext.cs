using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionAPI.Domain.Enums;
using TransactionAPI.Domain.Models;
using Type = TransactionAPI.Domain.Enums.Type;

namespace TransactionAPI.Infrastructure.Context
{
    public class TransactionAPIDbContext : DbContext
    {
        public TransactionAPIDbContext(DbContextOptions<TransactionAPIDbContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }

        public virtual DbSet<Transaction> Transactions { get; set; }

        public virtual DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var transactionStatusConverter = new EnumToStringConverter<Status>();
            var transactionTypeConverter = new EnumToStringConverter<Type>();

            modelBuilder.Entity<Transaction>()
                .Property(transaction => transaction.Status)
                .HasConversion(transactionStatusConverter);

            modelBuilder.Entity<Transaction>()
                .Property(transaction => transaction.Type)
                .HasConversion(transactionTypeConverter);

            modelBuilder.Entity<User>()
                .HasKey(user => user.Username);

            base.OnModelCreating(modelBuilder);
        }
    }
}
