using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace FlightScannerApp.CheapFlights;

public partial class CheapFlightsContext : DbContext
{
    public CheapFlightsContext()
    {
    }

    public CheapFlightsContext(DbContextOptions<CheapFlightsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<FaresPair> FaresPairs { get; set; }

    public virtual DbSet<OneWayFare> OneWayFares { get; set; }

    public virtual DbSet<ViewCheapestFlightsInMonth> ViewCheapestFlightsInMonths { get; set; }

    public virtual DbSet<ViewFlightPrice> ViewFlightPrices { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySQL("server=mysql;database=CheapFlights;user=root;password=pass");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FaresPair>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable(tb => tb.HasComment("Stores all available pairs of flights. Date difference between departure and arrival is maximum 14 days."));

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Destination)
                .HasMaxLength(3)
                .HasComment("IATA airport code where arrival is");
            entity.Property(e => e.DestinationCurrency)
                .HasMaxLength(3)
                .HasComment("Currency in which price is from destination")
                .HasColumnName("Destination_Currency");
            entity.Property(e => e.DestinationDate)
                .HasComment("Date of the flight back")
                .HasColumnType("datetime")
                .HasColumnName("Destination_Date");
            entity.Property(e => e.DestinationPrice)
                .HasPrecision(10)
                .HasComment("Price of the flight from the destination point of view")
                .HasColumnName("Destination_Price");
            entity.Property(e => e.Origin)
                .HasMaxLength(3)
                .HasComment("IATA airport code from which flight going to happen");
            entity.Property(e => e.OriginCurrency)
                .HasMaxLength(3)
                .HasComment("Currency in which price is from origin")
                .HasColumnName("Origin_Currency");
            entity.Property(e => e.OriginDate)
                .HasComment("Date of the departure")
                .HasColumnType("datetime")
                .HasColumnName("Origin_Date");
            entity.Property(e => e.OriginPrice)
                .HasPrecision(10)
                .HasComment("Price of the flight from the origin point of view")
                .HasColumnName("Origin_Price");
            entity.Property(e => e.Timestamp)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("Represents date time when record was created")
                .HasColumnType("timestamp");
            entity.Property(e => e.UpdateNo).HasComment("Indicate batch number in which record was created");
        });

        modelBuilder.Entity<OneWayFare>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Content)
                .HasComment("Response from Ryanair API")
                .HasColumnType("json");
            entity.Property(e => e.Destination)
                .HasMaxLength(5)
                .HasComment("IATA airport code");
            entity.Property(e => e.Origin)
                .HasMaxLength(5)
                .HasComment("IATA airport code");
            entity.Property(e => e.Timestamp)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
        });

        modelBuilder.Entity<ViewCheapestFlightsInMonth>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("View_CheapestFlightsInMonth");

            entity.Property(e => e.CheapestPrice).HasPrecision(10);
            entity.Property(e => e.Currency).HasMaxLength(3);
            entity.Property(e => e.Date).HasColumnType("date");
            entity.Property(e => e.Destination)
                .HasMaxLength(5)
                .HasComment("IATA airport code");
            entity.Property(e => e.Origin)
                .HasMaxLength(5)
                .HasComment("IATA airport code");
            entity.Property(e => e.Year).HasColumnType("year");
        });

        modelBuilder.Entity<ViewFlightPrice>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("View_FlightPrices");

            entity.Property(e => e.Currency).HasMaxLength(3);
            entity.Property(e => e.Day).HasColumnType("date");
            entity.Property(e => e.Destination)
                .HasMaxLength(5)
                .HasComment("IATA airport code");
            entity.Property(e => e.Origin)
                .HasMaxLength(5)
                .HasComment("IATA airport code");
            entity.Property(e => e.Price).HasPrecision(10);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
