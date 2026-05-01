using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Notification.Infrastructure.Persistence;

#nullable disable

namespace Notification.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(NotificationDbContext))]
    partial class NotificationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "10.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Notification.Domain.Entities.NotificationAttempt", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("AttemptNumber")
                        .HasColumnType("integer");

                    b.Property<DateTime>("AttemptedAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("FailureCode")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("FailureMessage")
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)");

                    b.Property<Guid>("NotificationDispatchId")
                        .HasColumnType("uuid");

                    b.Property<string>("ProviderMessageId")
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<string>("ProviderName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("ProviderRequestReference")
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("NotificationDispatchId", "AttemptNumber")
                        .IsUnique();

                    b.HasIndex("NotificationDispatchId", "AttemptedAtUtc");

                    b.ToTable("NotificationAttempts", (string)null);
                });

            modelBuilder.Entity("Notification.Domain.Entities.NotificationDispatch", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("BusinessEntityId")
                        .HasColumnType("uuid");

                    b.Property<string>("BusinessEntityType")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<int>("Channel")
                        .HasColumnType("integer");

                    b.Property<DateTime>("CreatedAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Body")
                        .HasMaxLength(20000)
                        .HasColumnType("character varying(20000)");

                    b.Property<Guid?>("CustomerId")
                        .HasColumnType("uuid");

                    b.Property<string>("FailureCode")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("FailureMessage")
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)");

                    b.Property<DateTime?>("ClickedAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("ComplainedAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("LastAttemptAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("LastProviderEventAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("LastProviderEventType")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("ProviderMessageId")
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<string>("ProviderName")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("RecipientAddress")
                        .HasMaxLength(320)
                        .HasColumnType("character varying(320)");

                    b.Property<string>("RecipientName")
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<DateTime?>("BouncedAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("DeliveredAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("OpenedAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("SentAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("StoreId")
                        .HasColumnType("uuid");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<string>("Subject")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<string>("SuppressionReason")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<int>("Trigger")
                        .HasColumnType("integer");

                    b.Property<DateTime>("UpdatedAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("ProviderName", "ProviderMessageId");

                    b.HasIndex("StoreId", "Status", "CreatedAtUtc");

                    b.HasIndex("StoreId", "Trigger", "Channel", "BusinessEntityType", "BusinessEntityId")
                        .IsUnique();

                    b.ToTable("NotificationDispatches", (string)null);
                });

            modelBuilder.Entity("Notification.Domain.Entities.NotificationTemplate", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("BodyTemplate")
                        .IsRequired()
                        .HasMaxLength(12000)
                        .HasColumnType("character varying(12000)");

                    b.Property<int>("Channel")
                        .HasColumnType("integer");

                    b.Property<DateTime>("CreatedAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("IsActive")
                        .HasColumnType("boolean");

                    b.Property<string>("Locale")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<Guid>("StoreId")
                        .HasColumnType("uuid");

                    b.Property<string>("SubjectTemplate")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<int>("Trigger")
                        .HasColumnType("integer");

                    b.Property<DateTime>("UpdatedAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("StoreId", "IsActive", "UpdatedAtUtc");

                    b.HasIndex("StoreId", "Trigger", "Channel", "Locale")
                        .IsUnique();

                    b.ToTable("NotificationTemplates", (string)null);
                });

            modelBuilder.Entity("Notification.Domain.Entities.NotificationAttempt", b =>
                {
                    b.HasOne("Notification.Domain.Entities.NotificationDispatch", null)
                        .WithMany("Attempts")
                        .HasForeignKey("NotificationDispatchId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
