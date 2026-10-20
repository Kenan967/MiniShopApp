using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using OrderService.API.Data;

#nullable disable

namespace OrderService.API.Migrations
{
    [DbContext(typeof(OrderDbContext))]
    [Migration("20261020000000_AddOrderEmail")]
    partial class AddOrderEmail
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "8.0.31");

            modelBuilder.Entity("OrderService.API.Models.Order", b =>
            {
                b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");
                b.Property<DateTime>("CreatedAt").HasColumnType("TEXT");
                b.Property<string>("Email").IsRequired().HasMaxLength(254).HasColumnType("TEXT");
                b.Property<string>("ProductName").IsRequired().HasColumnType("TEXT");
                b.Property<int>("ProductId").HasColumnType("INTEGER");
                b.Property<decimal>("TotalPrice").HasColumnType("TEXT");
                b.Property<decimal>("UnitPrice").HasColumnType("TEXT");
                b.Property<int>("Quantity").HasColumnType("INTEGER");
                b.HasKey("Id");
                b.ToTable("Orders");
            });
        }
    }
}
