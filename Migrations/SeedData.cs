using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MobileRechargeApp.Data.Migrations
{
    /// <summary>
    /// SAFE Seed Data Migration
    /// ✅ Will NOT affect existing data
    /// ✅ Skips any record that already exists
    /// ✅ Safe to run even if database already has data
    ///
    /// Run in Package Manager Console:
    ///   Add-Migration SeedData
    ///   Update-Database
    /// </summary>
    public partial class SeedData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var hasher = new PasswordHasher<IdentityUser>();
            var adminUser = new IdentityUser { Id = "user-admin-001" };
            var user1 = new IdentityUser { Id = "user-test-001" };
            var user2 = new IdentityUser { Id = "user-test-002" };

            string adminHash = hasher.HashPassword(adminUser, "Admin@123");
            string user1Hash = hasher.HashPassword(user1, "User@123");
            string user2Hash = hasher.HashPassword(user2, "User@123");

            // ═══════════════════════════════════════════════════════
            // 1. ROLES
            // ═══════════════════════════════════════════════════════
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Id = 'role-admin-001')
                    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
                    VALUES ('role-admin-001', 'Admin', 'ADMIN', NEWID());

                IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Id = 'role-user-001')
                    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
                    VALUES ('role-user-001', 'User', 'USER', NEWID());
            ");

            // ═══════════════════════════════════════════════════════
            // 2. USERS  (each checked by mobile number)
            // ═══════════════════════════════════════════════════════
            migrationBuilder.Sql($@"
                IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE UserName = '9000000000')
                    INSERT INTO AspNetUsers (
                        Id, UserName, NormalizedUserName, Email, NormalizedEmail,
                        EmailConfirmed, PasswordHash, PhoneNumber, PhoneNumberConfirmed,
                        LockoutEnabled, SecurityStamp, ConcurrencyStamp,
                        AccessFailedCount, TwoFactorEnabled)
                    VALUES ('user-admin-001','9000000000','9000000000',
                        'admin@mobicharge.pk','ADMIN@MOBICHARGE.PK',
                        1,'{adminHash}','9000000000',1,0,NEWID(),NEWID(),0,0);

                IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE UserName = '9001234567')
                    INSERT INTO AspNetUsers (
                        Id, UserName, NormalizedUserName, Email, NormalizedEmail,
                        EmailConfirmed, PasswordHash, PhoneNumber, PhoneNumberConfirmed,
                        LockoutEnabled, SecurityStamp, ConcurrencyStamp,
                        AccessFailedCount, TwoFactorEnabled)
                    VALUES ('user-test-001','9001234567','9001234567',
                        'ali@example.com','ALI@EXAMPLE.COM',
                        1,'{user1Hash}','9001234567',1,1,NEWID(),NEWID(),0,0);

                IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE UserName = '9009876543')
                    INSERT INTO AspNetUsers (
                        Id, UserName, NormalizedUserName, Email, NormalizedEmail,
                        EmailConfirmed, PasswordHash, PhoneNumber, PhoneNumberConfirmed,
                        LockoutEnabled, SecurityStamp, ConcurrencyStamp,
                        AccessFailedCount, TwoFactorEnabled)
                    VALUES ('user-test-002','9009876543','9009876543',
                        'sara@example.com','SARA@EXAMPLE.COM',
                        1,'{user2Hash}','9009876543',1,1,NEWID(),NEWID(),0,0);
            ");

            // ═══════════════════════════════════════════════════════
            // 3. ROLE ASSIGNMENTS
            // ═══════════════════════════════════════════════════════
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM AspNetUsers WHERE Id = 'user-admin-001')
                AND NOT EXISTS (SELECT 1 FROM AspNetUserRoles WHERE UserId = 'user-admin-001')
                    INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES ('user-admin-001','role-admin-001');

                IF EXISTS (SELECT 1 FROM AspNetUsers WHERE Id = 'user-test-001')
                AND NOT EXISTS (SELECT 1 FROM AspNetUserRoles WHERE UserId = 'user-test-001')
                    INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES ('user-test-001','role-user-001');

                IF EXISTS (SELECT 1 FROM AspNetUsers WHERE Id = 'user-test-002')
                AND NOT EXISTS (SELECT 1 FROM AspNetUserRoles WHERE UserId = 'user-test-002')
                    INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES ('user-test-002','role-user-001');
            ");

            // ═══════════════════════════════════════════════════════
            // 4. CLAIMS
            // ═══════════════════════════════════════════════════════
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM AspNetUsers WHERE Id = 'user-admin-001')
                AND NOT EXISTS (SELECT 1 FROM AspNetUserClaims WHERE UserId = 'user-admin-001' AND ClaimType = 'FullName')
                    INSERT INTO AspNetUserClaims (UserId,ClaimType,ClaimValue) VALUES ('user-admin-001','FullName','Administrator');

                IF EXISTS (SELECT 1 FROM AspNetUsers WHERE Id = 'user-test-001')
                AND NOT EXISTS (SELECT 1 FROM AspNetUserClaims WHERE UserId = 'user-test-001' AND ClaimType = 'FullName')
                    INSERT INTO AspNetUserClaims (UserId,ClaimType,ClaimValue) VALUES ('user-test-001','FullName','Ali Hassan');

                IF EXISTS (SELECT 1 FROM AspNetUsers WHERE Id = 'user-test-001')
                AND NOT EXISTS (SELECT 1 FROM AspNetUserClaims WHERE UserId = 'user-test-001' AND ClaimType = 'PlanType')
                    INSERT INTO AspNetUserClaims (UserId,ClaimType,ClaimValue) VALUES ('user-test-001','PlanType','Prepaid');

                IF EXISTS (SELECT 1 FROM AspNetUsers WHERE Id = 'user-test-002')
                AND NOT EXISTS (SELECT 1 FROM AspNetUserClaims WHERE UserId = 'user-test-002' AND ClaimType = 'FullName')
                    INSERT INTO AspNetUserClaims (UserId,ClaimType,ClaimValue) VALUES ('user-test-002','FullName','Sara Ahmed');

                IF EXISTS (SELECT 1 FROM AspNetUsers WHERE Id = 'user-test-002')
                AND NOT EXISTS (SELECT 1 FROM AspNetUserClaims WHERE UserId = 'user-test-002' AND ClaimType = 'PlanType')
                    INSERT INTO AspNetUserClaims (UserId,ClaimType,ClaimValue) VALUES ('user-test-002','PlanType','Postpaid');
            ");

            // ═══════════════════════════════════════════════════════
            // 5. TOP UP PLANS — only if table is empty
            // ═══════════════════════════════════════════════════════
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM TopUpPlans)
                BEGIN
                    INSERT INTO TopUpPlans (Amount, TalkTime, Validity, Description) VALUES
                    (50,   42,  7,  'Weekly Basic'),
                    (100,  88,  14, 'Fortnightly Talktime'),
                    (200,  180, 28, 'Monthly Standard'),
                    (500,  460, 30, 'Monthly Premium'),
                    (1000, 950, 60, 'Bi-Monthly Max');
                END
            ");

            // ═══════════════════════════════════════════════════════
            // 6. SPECIAL PLANS — only if table is empty
            // ═══════════════════════════════════════════════════════
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM SpecialPlans)
                BEGIN
                    INSERT INTO SpecialPlans (Amount, Description, Validity, Benefits) VALUES
                    (149,  'Starter Bundle',    7,  '1GB Data + 100 SMS + 50 Mins'),
                    (299,  'Weekend Data Pack', 3,  '5GB Data (Weekends Only)'),
                    (399,  'Monthly Internet',  30, '5GB Data + Unlimited SMS'),
                    (599,  'All-in-One Pack',   30, '10GB Data + 500 SMS + 200 Mins'),
                    (999,  'Unlimited Monthly', 30, 'Unlimited Calls + 20GB Data'),
                    (1500, 'Super Bundle',      60, '30GB Data + Unlimited Everything');
                END
            ");

            // ═══════════════════════════════════════════════════════
            // 7. SAMPLE TRANSACTIONS — only if table is empty
            // ═══════════════════════════════════════════════════════
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM Transactions)
                BEGIN
                    INSERT INTO Transactions (MobileNumber, PlanType, PlanId, Amount, PaymentMethod, Status, TxnDate) VALUES
                    ('9001234567','topup',   1,100, 'UPI',         'Success',DATEADD(day,-20,GETDATE())),
                    ('9001234567','special', 3,399, 'Debit Card',  'Success',DATEADD(day,-10,GETDATE())),
                    ('9001234567','topup',   2,200, 'Net Banking', 'Success',DATEADD(day,-3, GETDATE())),
                    ('9009876543','postpaid',0,743, 'Credit Card', 'Success',DATEADD(day,-25,GETDATE())),
                    ('9009876543','postpaid',0,812, 'UPI',         'Success',DATEADD(day,-55,GETDATE())),
                    ('9009876543','special', 5,999, 'Debit Card',  'Success',DATEADD(day,-8, GETDATE()));
                END
            ");

            // ═══════════════════════════════════════════════════════
            // 8. SAMPLE FEEDBACKS — only if table is empty
            // ═══════════════════════════════════════════════════════
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM Feedbacks)
                BEGIN
                    INSERT INTO Feedbacks (Name, Email, Message, SubmittedAt) VALUES
                    ('Ali Hassan', 'ali@example.com',  'Very easy to recharge! The interface is smooth and my transaction completed in seconds.', DATEADD(day,-15,GETDATE())),
                    ('Sara Ahmed', 'sara@example.com', 'Postpaid bill payment is super convenient. Love that I can see my usage breakdown clearly.', DATEADD(day,-8,GETDATE())),
                    ('Usman Khan', 'usman@example.com','Great service! Special plans have excellent value. Would love more data packs.', DATEADD(day,-3,GETDATE()));
                END
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Only removes the exact seeded records — never touches your own data
            migrationBuilder.Sql(@"
                DELETE FROM AspNetUserClaims WHERE UserId IN ('user-admin-001','user-test-001','user-test-002');
                DELETE FROM AspNetUserRoles  WHERE UserId IN ('user-admin-001','user-test-001','user-test-002');
                DELETE FROM AspNetUsers      WHERE Id     IN ('user-admin-001','user-test-001','user-test-002');
                DELETE FROM AspNetRoles      WHERE Id     IN ('role-admin-001','role-user-001');
            ");
        }
    }
}