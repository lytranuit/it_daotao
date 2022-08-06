using it.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using it.Areas.Admin.Models;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.ChangeTracking;
namespace it.Data
{
    public class ItContext : DbContext
    {
        public ItContext(DbContextOptions<ItContext> options) : base(options)
        {
        }

        public DbSet<UserModel> UserModel { get; set; }
        public DbSet<DocumentModel> DocumentModel { get; set; }
        public DbSet<EmployeeModel> EmployeeModel { get; set; }
        public DbSet<EmployeeJobPreviousModel> EmployeeJobPreviousModel { get; set; }
        public DbSet<EmployeeResponsibilitiesModel> EmployeeResponsibilitiesModel { get; set; }
        public DbSet<EmployeeReplaceModel> EmployeeReplaceModel { get; set; }
        public DbSet<EmployeeReportModel> EmployeeReportModel { get; set; }
        public DbSet<EmployeeWorkgroupModel> EmployeeWorkgroupModel { get; set; }
        public DbSet<RecordModel> RecordModel { get; set; }
        public DbSet<RecordTrainModel> RecordTrainModel { get; set; }
        public DbSet<WorkgroupModel> WorkgroupModel { get; set; }
        public DbSet<WorkgroupProcedureModel> WorkgroupProcedureModel { get; set; }
        public DbSet<ProcedureModel> ProcedureModel { get; set; }
        public DbSet<ProcedureVersionModel> ProcedureVersionModel { get; set; }
        public DbSet<DepartmentModel> DepartmentModel { get; set; }
        //public DbSet<User2Model> User2Model { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<IdentityUser>().ToTable("AspNetUsers");

            //modelBuilder.Entity<DocumentModel>().HasMany(l => l.Teams).WithOne().HasForeignKey("LeagueId");
            modelBuilder.Entity<DocumentModel>()
             .Property(b => b._data).HasColumnName("data");
        }
        protected override void ConfigureConventions(ModelConfigurationBuilder builder)
        {
        }
    }
}
