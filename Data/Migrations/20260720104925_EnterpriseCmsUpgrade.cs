using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NedAcademyWebsite.Data.Migrations
{
    /// <inheritdoc />
    public partial class EnterpriseCmsUpgrade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "NewsItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "NewsItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "FacultyMembers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "FacultyMembers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "DegreePrograms",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(15)",
                oldMaxLength: 15);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "DegreePrograms",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CurriculumPdfPath",
                table: "DegreePrograms",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DegreeLevel",
                table: "DegreePrograms",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "DegreePrograms",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "DegreePrograms",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "DegreePrograms",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                table: "DegreePrograms",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublished",
                table: "DegreePrograms",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MetaTitle",
                table: "DegreePrograms",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Overview",
                table: "DegreePrograms",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "DegreePrograms",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "DegreePrograms",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntityName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: true),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PerformedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BackupRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    BackupType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BackupRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CmsNotifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    TargetUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CmsNotifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContentRevisions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    RevisionNumber = table.Column<int>(type: "int", nullable: false),
                    ContentJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ChangeNotes = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentRevisions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CustomForms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FormName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FormKey = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomForms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MediaAssets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FileType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    UploadDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsFeatured = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaAssets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NavMenuItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MenuName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsVisible = table.Column<bool>(type: "bit", nullable: false),
                    IsExternal = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NavMenuItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NavMenuItems_NavMenuItems_ParentId",
                        column: x => x.ParentId,
                        principalTable: "NavMenuItems",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PageSections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PageKey = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SectionKey = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SectionTitle = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    SectionSubtitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsVisible = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    BackgroundStyle = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ContainerWidth = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PaddingStyle = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AnimationType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageSections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SectionSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SectionKey = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsVisible = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    BackgroundStyle = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PaddingStyle = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SectionSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FormFields",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomFormId = table.Column<int>(type: "int", nullable: false),
                    FieldName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FieldLabel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FieldType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Placeholder = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OptionsCsv = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    ValidationRegex = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormFields_CustomForms_CustomFormId",
                        column: x => x.CustomFormId,
                        principalTable: "CustomForms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FormFields_CustomFormId",
                table: "FormFields",
                column: "CustomFormId");

            migrationBuilder.CreateIndex(
                name: "IX_NavMenuItems_ParentId",
                table: "NavMenuItems",
                column: "ParentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "BackupRecords");

            migrationBuilder.DropTable(
                name: "CmsNotifications");

            migrationBuilder.DropTable(
                name: "ContentRevisions");

            migrationBuilder.DropTable(
                name: "FormFields");

            migrationBuilder.DropTable(
                name: "MediaAssets");

            migrationBuilder.DropTable(
                name: "NavMenuItems");

            migrationBuilder.DropTable(
                name: "PageSections");

            migrationBuilder.DropTable(
                name: "SectionSettings");

            migrationBuilder.DropTable(
                name: "CustomForms");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "NewsItems");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "NewsItems");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "FacultyMembers");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "FacultyMembers");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "DegreePrograms");

            migrationBuilder.DropColumn(
                name: "CurriculumPdfPath",
                table: "DegreePrograms");

            migrationBuilder.DropColumn(
                name: "DegreeLevel",
                table: "DegreePrograms");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "DegreePrograms");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "DegreePrograms");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "DegreePrograms");

            migrationBuilder.DropColumn(
                name: "IsFeatured",
                table: "DegreePrograms");

            migrationBuilder.DropColumn(
                name: "IsPublished",
                table: "DegreePrograms");

            migrationBuilder.DropColumn(
                name: "MetaTitle",
                table: "DegreePrograms");

            migrationBuilder.DropColumn(
                name: "Overview",
                table: "DegreePrograms");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "DegreePrograms");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "DegreePrograms");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "DegreePrograms",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);
        }
    }
}
