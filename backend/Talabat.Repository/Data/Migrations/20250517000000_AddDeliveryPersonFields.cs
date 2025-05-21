using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Talabat.Repository.Data.Migrations
{
    public partial class AddDeliveryPersonFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "DeliveryMethods",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "Default Address");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "DeliveryMethods",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "default@delivery.com");

            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "DeliveryMethods",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "DefaultPass@123");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "DeliveryMethods",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "+1234567890");

            // Update existing records with proper values
            migrationBuilder.Sql(@"
                UPDATE DeliveryMethods 
                SET Email = CASE ShortName
                    WHEN 'UPS1' THEN 'ups1@delivery.com'
                    WHEN 'UPS2' THEN 'ups2@delivery.com'
                    WHEN 'UPS3' THEN 'ups3@delivery.com'
                    WHEN 'FREE' THEN 'free@delivery.com'
                    ELSE 'default@delivery.com'
                END,
                Password = CASE ShortName
                    WHEN 'UPS1' THEN 'Ups1@123'
                    WHEN 'UPS2' THEN 'Ups2@123'
                    WHEN 'UPS3' THEN 'Ups3@123'
                    WHEN 'FREE' THEN 'Free@123'
                    ELSE 'DefaultPass@123'
                END,
                Address = CASE ShortName
                    WHEN 'UPS1' THEN '123 UPS Street, City'
                    WHEN 'UPS2' THEN '456 UPS Avenue, City'
                    WHEN 'UPS3' THEN '789 UPS Road, City'
                    WHEN 'FREE' THEN '321 Free Delivery Lane, City'
                    ELSE 'Default Address'
                END,
                PhoneNumber = CASE ShortName
                    WHEN 'UPS1' THEN '+1234567890'
                    WHEN 'UPS2' THEN '+1234567891'
                    WHEN 'UPS3' THEN '+1234567892'
                    WHEN 'FREE' THEN '+1234567893'
                    ELSE '+1234567890'
                END");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "DeliveryMethods");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "DeliveryMethods");

            migrationBuilder.DropColumn(
                name: "Password",
                table: "DeliveryMethods");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "DeliveryMethods");
        }
    }
} 