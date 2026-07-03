using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserManagement.Api.Entities;

namespace UserManagement.Api.Data.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.Id)
            .ValueGeneratedNever();

        builder.Property(user => user.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(user => user.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(user => user.Email)
            .IsUnique();

        builder.Property(user => user.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(user => user.CreatedAt)
            .IsRequired();
    }
}
