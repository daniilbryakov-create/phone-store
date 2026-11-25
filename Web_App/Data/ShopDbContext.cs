using Microsoft.EntityFrameworkCore;
using Web_App.Models;

namespace Web_App.Data;

public class ShopDbContext : DbContext
{
    public ShopDbContext(DbContextOptions<ShopDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Seller> Sellers => Set<Seller>();
    public DbSet<Manager> Managers => Set<Manager>();
    public DbSet<Admin> Admins => Set<Admin>();

    public DbSet<Phone> Phones => Set<Phone>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();
    public DbSet<Return> Returns => Set<Return>();
    public DbSet<Supply> Supplies => Set<Supply>();
    public DbSet<SupplyItem> SupplyItems => Set<SupplyItem>();
    public DbSet<Report> Reports => Set<Report>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        // --- пользователи: одна таблица, колонка Discriminator ---
        b.Entity<User>()
            .HasDiscriminator<string>("Discriminator")
            .HasValue<Client>("Client")
            .HasValue<Seller>("Seller")
            .HasValue<Manager>("Manager")
            .HasValue<Admin>("Admin");

        // уникальность (индекс пропускает NULL: у офлайн-клиента логина нет)
        b.Entity<User>().HasIndex(u => u.Login).IsUnique();
        b.Entity<Client>().HasIndex(c => c.PhoneNumber).IsUnique();

        // --- удалять телефон/пользователя, на которого есть ссылки, нельзя ---
        b.Entity<OrderItem>().HasOne(i => i.Phone).WithMany().HasForeignKey(i => i.PhoneId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<SaleItem>().HasOne(i => i.Phone).WithMany().HasForeignKey(i => i.PhoneId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<SupplyItem>().HasOne(i => i.Phone).WithMany().HasForeignKey(i => i.PhoneId).OnDelete(DeleteBehavior.Restrict);

        b.Entity<Order>().HasOne(o => o.Client).WithMany(c => c.Orders).HasForeignKey(o => o.ClientId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Sale>().HasOne(s => s.Client).WithMany(c => c.Sales).HasForeignKey(s => s.ClientId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Sale>().HasOne(s => s.Seller).WithMany(s => s.Sales).HasForeignKey(s => s.SellerId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Return>().HasOne(r => r.Seller).WithMany(s => s.Returns).HasForeignKey(r => r.SellerId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Return>().HasOne(r => r.SaleItem).WithMany(i => i.Returns).HasForeignKey(r => r.SaleItemId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Supply>().HasOne(s => s.Manager).WithMany(m => m.Supplies).HasForeignKey(s => s.ManagerId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Supply>().HasOne(s => s.Supplier).WithMany().HasForeignKey(s => s.SupplierId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Report>().HasOne(r => r.Manager).WithMany(m => m.Reports).HasForeignKey(r => r.ManagerId).OnDelete(DeleteBehavior.Restrict);

        // --- позиции удаляются вместе с документом ---
        b.Entity<OrderItem>().HasOne(i => i.Order).WithMany(o => o.Items).HasForeignKey(i => i.OrderId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<OrderStatusChange>().HasOne(h => h.Order).WithMany(o => o.StatusHistory).HasForeignKey(h => h.OrderId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<SaleItem>().HasOne(i => i.Sale).WithMany(s => s.Items).HasForeignKey(i => i.SaleId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<SupplyItem>().HasOne(i => i.Supply).WithMany(s => s.Items).HasForeignKey(i => i.SupplyId).OnDelete(DeleteBehavior.Cascade);

        // --- строки отчёта хранятся как JSON в самой таблице Reports ---
        b.Entity<Report>().OwnsMany(r => r.SoldLines, o => o.ToJson());
        b.Entity<Report>().OwnsMany(r => r.StockLines, o => o.ToJson());
    }
}
