using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reciplex.Server.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table
                        .Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DisplayName = table.Column<string>(
                        type: "TEXT",
                        maxLength: 64,
                        nullable: false
                    ),
                    ConcurrencyTag = table.Column<string>(type: "TEXT", nullable: false),
                    Subject = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Authority = table.Column<string>(
                        type: "TEXT",
                        maxLength: 1024,
                        nullable: false
                    ),
                    Deleted = table.Column<long>(type: "INTEGER", nullable: true),
                    LastModified = table.Column<long>(type: "INTEGER", nullable: false),
                    Created = table.Column<long>(type: "INTEGER", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "RecipeBooks",
                columns: table => new
                {
                    Id = table
                        .Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    ShortDescription = table.Column<string>(
                        type: "TEXT",
                        maxLength: 256,
                        nullable: false
                    ),
                    LastModified = table.Column<long>(type: "INTEGER", nullable: false),
                    Created = table.Column<long>(type: "INTEGER", nullable: false),
                    ConcurrencyTag = table.Column<string>(type: "TEXT", nullable: false),
                    OwnerFk = table.Column<long>(type: "INTEGER", nullable: false),
                    ShareKey = table.Column<string>(type: "TEXT", nullable: false),
                    Deleted = table.Column<long>(type: "INTEGER", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeBooks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecipeBooks_Users_OwnerFk",
                        column: x => x.OwnerFk,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "RecipeBookAccessEntries",
                columns: table => new
                {
                    Id = table
                        .Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RecipeBookFk = table.Column<long>(type: "INTEGER", nullable: false),
                    UserFk = table.Column<long>(type: "INTEGER", nullable: false),
                    MayViewBook = table.Column<bool>(type: "INTEGER", nullable: false),
                    MayEditBook = table.Column<bool>(type: "INTEGER", nullable: false),
                    Reviewed = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastModified = table.Column<long>(type: "INTEGER", nullable: false),
                    Created = table.Column<long>(type: "INTEGER", nullable: false),
                    ConcurrencyTag = table.Column<string>(type: "TEXT", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeBookAccessEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecipeBookAccessEntries_RecipeBooks_RecipeBookFk",
                        column: x => x.RecipeBookFk,
                        principalTable: "RecipeBooks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_RecipeBookAccessEntries_Users_UserFk",
                        column: x => x.UserFk,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "Recipes",
                columns: table => new
                {
                    Id = table
                        .Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    ShortDescription = table.Column<string>(
                        type: "TEXT",
                        maxLength: 256,
                        nullable: false
                    ),
                    Details = table.Column<string>(
                        type: "TEXT",
                        maxLength: 1048576,
                        nullable: false
                    ),
                    LastModified = table.Column<long>(type: "INTEGER", nullable: false),
                    Created = table.Column<long>(type: "INTEGER", nullable: false),
                    ConcurrencyTag = table.Column<string>(type: "TEXT", nullable: false),
                    RecipeBookFk = table.Column<long>(type: "INTEGER", nullable: false),
                    Deleted = table.Column<long>(type: "INTEGER", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recipes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Recipes_RecipeBooks_RecipeBookFk",
                        column: x => x.RecipeBookFk,
                        principalTable: "RecipeBooks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_RecipeBookAccessEntries_RecipeBookFk",
                table: "RecipeBookAccessEntries",
                column: "RecipeBookFk"
            );

            migrationBuilder.CreateIndex(
                name: "IX_RecipeBookAccessEntries_UserFk",
                table: "RecipeBookAccessEntries",
                column: "UserFk"
            );

            migrationBuilder.CreateIndex(
                name: "IX_RecipeBookAccessEntries_UserFk_RecipeBookFk",
                table: "RecipeBookAccessEntries",
                columns: new[] { "UserFk", "RecipeBookFk" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_RecipeBooks_Deleted",
                table: "RecipeBooks",
                column: "Deleted"
            );

            migrationBuilder.CreateIndex(
                name: "IX_RecipeBooks_OwnerFk",
                table: "RecipeBooks",
                column: "OwnerFk"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Recipes_Deleted",
                table: "Recipes",
                column: "Deleted"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Recipes_RecipeBookFk",
                table: "Recipes",
                column: "RecipeBookFk"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Users_Authority_Subject",
                table: "Users",
                columns: new[] { "Authority", "Subject" }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "RecipeBookAccessEntries");

            migrationBuilder.DropTable(name: "Recipes");

            migrationBuilder.DropTable(name: "RecipeBooks");

            migrationBuilder.DropTable(name: "Users");
        }
    }
}
