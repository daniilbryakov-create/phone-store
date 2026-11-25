using Microsoft.EntityFrameworkCore;
using Web_App.Models;
using Web_App.Services;

namespace Web_App.Data;

/// <summary>Демо-данные при первом запуске (те же, что в консольной версии).</summary>
public static class DbSeeder
{
    public static void Seed(ShopDbContext db)
    {
        db.Database.Migrate();      // создаёт БД и применяет миграции
        if (db.Users.Any()) return; // уже заполнено

        db.Users.AddRange(
            new Admin   { FullName = "Администратор Системы",   Login = "admin",   PasswordHash = PasswordHasher.Hash("admin123") },
            new Manager { FullName = "Петров Пётр Петрович",    Login = "manager", PasswordHash = PasswordHasher.Hash("manager123") },
            new Seller  { FullName = "Сидорова Анна Ивановна",  Login = "seller",  PasswordHash = PasswordHasher.Hash("seller123") },
            new Client  { FullName = "Иванов Иван Иванович",    PhoneNumber = "+79001234567", Email = "ivanov@example.com",
                          Login = "client", PasswordHash = PasswordHasher.Hash("client123") });

        db.Suppliers.AddRange(
            new Supplier { OrganizationName = "ООО «МобайлОпт»",      Email = "sales@mobileopt.example.com" },
            new Supplier { OrganizationName = "АО «ТехноПоставка»",   Email = "info@technopostavka.example.com" });

        db.Phones.AddRange(
            new Phone { CompanyName = "Apple",   PhoneName = "iPhone 15 128GB", Price = 89990, StockAmount = 5,  Specification = "6.1\" OLED, A16 Bionic, 128 ГБ, 48 Мп" },
            new Phone { CompanyName = "Samsung", PhoneName = "Galaxy S24",      Price = 74990, StockAmount = 7,  Specification = "6.2\" AMOLED, Exynos 2400, 256 ГБ, 50 Мп" },
            new Phone { CompanyName = "Samsung", PhoneName = "Galaxy A55",      Price = 36990, StockAmount = 10, Specification = "6.6\" Super AMOLED, Exynos 1480, 128 ГБ, 50 Мп" },
            new Phone { CompanyName = "Xiaomi",  PhoneName = "Redmi Note 13",   Price = 19990, StockAmount = 15, Specification = "6.67\" AMOLED, Snapdragon 685, 128 ГБ, 108 Мп" },
            new Phone { CompanyName = "Xiaomi",  PhoneName = "14T",             Price = 44990, StockAmount = 4,  Specification = "6.67\" AMOLED, Dimensity 8300, 256 ГБ, 50 Мп" },
            new Phone { CompanyName = "Honor",   PhoneName = "90",              Price = 34990, StockAmount = 6,  Specification = "6.7\" OLED, Snapdragon 7 Gen 1, 256 ГБ, 200 Мп" },
            new Phone { CompanyName = "Google",  PhoneName = "Pixel 8",         Price = 62990, StockAmount = 0,  Specification = "6.2\" OLED, Tensor G3, 128 ГБ, 50 Мп" },
            new Phone { CompanyName = "Nokia",   PhoneName = "110",             Price = 3490,  StockAmount = 20, Specification = "1.77\" TFT, 2 SIM, аккумулятор 1000 мА·ч" });

        db.SaveChanges();
    }
}
