using Microsoft.EntityFrameworkCore;
using TireTraceabilityDemo.Models;

namespace TireTraceabilityDemo.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }


    // =========================================================
    // PRODUCTION DATA
    // =========================================================

    public DbSet<Tire> Tires => Set<Tire>();

    public DbSet<Curing> Curings => Set<Curing>();

    public DbSet<Inspection> Inspections => Set<Inspection>();


    // =========================================================
    // ADMIN
    // =========================================================

    public DbSet<Admin> Admins => Set<Admin>();


    // =========================================================
    // OPERATOR & MACHINE
    // =========================================================

    public DbSet<Operator> Operators => Set<Operator>();

    public DbSet<Machine> Machines => Set<Machine>();


    // =========================================================
    // MASTER DATA
    // =========================================================

    public DbSet<Dropdown> Dropdowns => Set<Dropdown>();


    // =========================================================
    // DAISHA
    // =========================================================
    //
    // Daisha = container/grouping untuk beberapa Tire.
    //
    // Contoh:
    //
    // Daisha
    // DS-20260830-001
    //        |
    //        +-- Tire 1
    //        +-- Tire 2
    //        +-- Tire 3
    //        +-- Tire 4
    //        +-- Tire 5
    //        +-- Tire 6
    //
    // DaishaTires digunakan sebagai tabel penghubung
    // antara Daisha dan Tire.
    //
    // =========================================================

    public DbSet<Daisha> Daishas => Set<Daisha>();

    public DbSet<DaishaTire> DaishaTires => Set<DaishaTire>();
}