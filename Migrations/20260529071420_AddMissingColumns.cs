using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace wpf_projekt.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Kolumny TransferGroupId, Name (SharedAccounts, PersonalAccounts)
            // sa juz dodane przez migracj AddEventLogsTable (20260528).
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
