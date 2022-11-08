using Microsoft.EntityFrameworkCore.Migrations;

namespace FakeApi.Migrations
{
    public partial class secondpostgressql : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "308660dc-ae51-480f-824d-7dca6714c3e2",
                column: "ConcurrencyStamp",
                value: "f485d776-6060-4cd2-b92c-07508466bf9e");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "90184155-dee0-40c9-bb1e-b5ed07afc04e",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "18e9c394-b6fa-46e9-a2ac-032b45b61b16", "AQAAAAEAACcQAAAAEJp1bgSEkBzrvEsR1I/jPQ31IslWLiA5SAYzpwiKhN0ALCOX+ix3+oppeqqydYCf1Q==", "f52cc436-55ca-4fb9-a072-33aa158f1b86" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "308660dc-ae51-480f-824d-7dca6714c3e2",
                column: "ConcurrencyStamp",
                value: "08631aab-c70a-44e1-8113-43f0b4752520");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "90184155-dee0-40c9-bb1e-b5ed07afc04e",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "54b91bb8-2551-4875-b617-b64037992894", "AQAAAAEAACcQAAAAEKNhE9W4IxF8LkKhF4v8Tp1C4BM+qpWIeVC8BA3zxcTrAw0WdaiRZ0UhxMbpKGwmzg==", "1d5eb83f-0b5d-45e9-b541-d5aac048d02f" });
        }
    }
}
