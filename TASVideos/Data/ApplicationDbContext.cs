using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data.Entity;

namespace TASVideos.Data
{
	public class ApplicationDbContext : IdentityDbContext<User, Role, int>
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			: base(options)
		{
		}

		public DbSet<Permission> Permissions { get; set; }
		public DbSet<RolePermission> RolePermission { get; set; }

		protected override void OnModelCreating(ModelBuilder builder)
		{
			builder.Entity<User>(entity =>
			{
				entity.HasIndex(e => e.NormalizedEmail)
					.HasName("EmailIndex");

				entity.HasIndex(e => e.NormalizedUserName)
					.HasName("UserNameIndex")
					.IsUnique()
					.HasFilter("([NormalizedUserName] IS NOT NULL)");
			});

			builder.Entity<IdentityUserLogin<int>>(entity =>
			{
				entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });
				entity.HasIndex(e => e.UserId);
			});

			builder.Entity<IdentityRoleClaim<int>>(entity =>
			{
				entity.HasIndex(e => e.RoleId);
			});

			builder.Entity<IdentityUserClaim<int>>(entity =>
			{
				entity.HasIndex(e => e.UserId);
			});

			builder.Entity<IdentityUserRole<int>>(entity =>
			{
				entity.HasKey(e => new { e.UserId, e.RoleId });
				entity.HasIndex(e => e.RoleId);
			});

			builder.Entity<IdentityUserToken<int>>(entity =>
			{
				entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });
			});

			builder.Entity<RolePermission>()
				.HasKey(rp => new { rp.RoleId, rp.PermissionId });

			builder.Entity<RolePermission>()
				.HasOne(pt => pt.Role)
				.WithMany(p => p.RolePermission)
				.HasForeignKey(pt => pt.RoleId);

			builder.Entity<RolePermission>()
				.HasOne(pt => pt.Permission)
				.WithMany(t => t.RolePermission)
				.HasForeignKey(pt => pt.PermissionId);
		}
	}
}
