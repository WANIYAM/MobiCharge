using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MobileRechargeApp.Models;

namespace MobileRechargeApp.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<TopUpPlan> TopUpPlans { get; set; }
        public DbSet<SpecialPlan> SpecialPlans { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
    }
}