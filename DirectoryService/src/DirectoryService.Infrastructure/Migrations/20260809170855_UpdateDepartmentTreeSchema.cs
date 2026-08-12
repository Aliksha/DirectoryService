using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectoryService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDepartmentTreeSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE public.departments ALTER COLUMN path TYPE ltree USING path::ltree;");

            migrationBuilder.Sql("UPDATE public.departments SET depth = nlevel(path);");

            migrationBuilder.Sql("ALTER TABLE public.departments ALTER COLUMN depth SET NOT NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE public.departments ALTER COLUMN path TYPE character varying(500);");
        }
    }
}
