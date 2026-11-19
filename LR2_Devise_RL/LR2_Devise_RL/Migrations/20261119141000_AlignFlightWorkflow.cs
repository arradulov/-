using LR2_Devise_RL.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LR2_Devise_RL.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20261119141000_AlignFlightWorkflow")]
    public partial class AlignFlightWorkflow : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("PRAGMA foreign_keys = 0;", suppressTransaction: true);

            migrationBuilder.Sql(
                """
                CREATE TABLE "__Devices_new" (
                    "Id" INTEGER NOT NULL CONSTRAINT "PK_Devices" PRIMARY KEY AUTOINCREMENT,
                    "Name" TEXT NOT NULL,
                    "BatteryLevel" INTEGER NOT NULL,
                    "MaxSpeed" REAL NOT NULL,
                    "MinSpeed" REAL NOT NULL
                );

                INSERT INTO "__Devices_new" ("Id", "Name", "BatteryLevel", "MaxSpeed", "MinSpeed")
                SELECT "Id", "Name", "BatteryLevel", "MaxSpeed", "MinSpeed"
                FROM "Devices";

                CREATE TABLE "__Missions_new" (
                    "Id" INTEGER NOT NULL CONSTRAINT "PK_Missions" PRIMARY KEY AUTOINCREMENT,
                    "Name" TEXT NOT NULL,
                    "Distance" REAL NOT NULL,
                    "TimeLimit" REAL NOT NULL
                );

                INSERT INTO "__Missions_new" ("Id", "Name", "Distance", "TimeLimit")
                SELECT "Id", "Name", "Distance",
                    CASE
                        WHEN "Speed" > 0 THEN "Distance" / "Speed"
                        ELSE 1
                    END
                FROM "Missions";

                CREATE TABLE "__FlightResults_new" (
                    "Id" INTEGER NOT NULL CONSTRAINT "PK_FlightResults" PRIMARY KEY AUTOINCREMENT,
                    "DeviceId" INTEGER NOT NULL,
                    "MissionId" INTEGER NOT NULL,
                    "FlightTime" REAL NOT NULL,
                    "ActualDistance" REAL NOT NULL,
                    "Status" INTEGER NOT NULL,
                    CONSTRAINT "FK_FlightResults_Devices_DeviceId"
                        FOREIGN KEY ("DeviceId") REFERENCES "Devices" ("Id") ON DELETE RESTRICT,
                    CONSTRAINT "FK_FlightResults_Missions_MissionId"
                        FOREIGN KEY ("MissionId") REFERENCES "Missions" ("Id") ON DELETE RESTRICT
                );

                INSERT INTO "__FlightResults_new"
                    ("Id", "DeviceId", "MissionId", "FlightTime", "ActualDistance", "Status")
                SELECT "Id", "DeviceId", "MissionId", "FlightTime", "ActualDistance", 1
                FROM "FlightResults";

                DROP TABLE "FlightResults";
                DROP TABLE "Devices";
                DROP TABLE "Missions";

                ALTER TABLE "__Devices_new" RENAME TO "Devices";
                ALTER TABLE "__Missions_new" RENAME TO "Missions";
                ALTER TABLE "__FlightResults_new" RENAME TO "FlightResults";

                CREATE INDEX "IX_FlightResults_DeviceId" ON "FlightResults" ("DeviceId");
                CREATE INDEX "IX_FlightResults_MissionId" ON "FlightResults" ("MissionId");
                """);

            migrationBuilder.Sql("PRAGMA foreign_keys = 1;", suppressTransaction: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("PRAGMA foreign_keys = 0;", suppressTransaction: true);

            migrationBuilder.Sql(
                """
                CREATE TABLE "__Devices_old" (
                    "Id" INTEGER NOT NULL CONSTRAINT "PK_Devices" PRIMARY KEY AUTOINCREMENT,
                    "Name" TEXT NOT NULL,
                    "BatteryLevel" INTEGER NOT NULL,
                    "MaxSpeed" REAL NOT NULL,
                    "MinSpeed" REAL NOT NULL,
                    "IsActive" INTEGER NOT NULL
                );

                INSERT INTO "__Devices_old"
                    ("Id", "Name", "BatteryLevel", "MaxSpeed", "MinSpeed", "IsActive")
                SELECT "Id", "Name", "BatteryLevel", "MaxSpeed", "MinSpeed", 1
                FROM "Devices";

                CREATE TABLE "__Missions_old" (
                    "Id" INTEGER NOT NULL CONSTRAINT "PK_Missions" PRIMARY KEY AUTOINCREMENT,
                    "Name" TEXT NOT NULL,
                    "Distance" REAL NOT NULL,
                    "Speed" REAL NOT NULL
                );

                INSERT INTO "__Missions_old" ("Id", "Name", "Distance", "Speed")
                SELECT "Id", "Name", "Distance",
                    CASE
                        WHEN "TimeLimit" > 0 THEN "Distance" / "TimeLimit"
                        ELSE 0
                    END
                FROM "Missions";

                CREATE TABLE "__FlightResults_old" (
                    "Id" INTEGER NOT NULL CONSTRAINT "PK_FlightResults" PRIMARY KEY AUTOINCREMENT,
                    "DeviceId" INTEGER NOT NULL,
                    "MissionId" INTEGER NOT NULL,
                    "FlightTime" REAL NOT NULL,
                    "ActualDistance" REAL NOT NULL,
                    "IsSuccessful" INTEGER NOT NULL
                );

                INSERT INTO "__FlightResults_old"
                    ("Id", "DeviceId", "MissionId", "FlightTime", "ActualDistance", "IsSuccessful")
                SELECT result."Id", result."DeviceId", result."MissionId",
                    result."FlightTime", result."ActualDistance",
                    CASE
                        WHEN result."Status" = 1
                            AND result."ActualDistance" >= mission."Distance"
                            AND result."FlightTime" > 0
                            AND result."FlightTime" <= mission."TimeLimit"
                        THEN 1
                        ELSE 0
                    END
                FROM "FlightResults" AS result
                INNER JOIN "Missions" AS mission ON result."MissionId" = mission."Id";

                DROP TABLE "FlightResults";
                DROP TABLE "Devices";
                DROP TABLE "Missions";

                ALTER TABLE "__Devices_old" RENAME TO "Devices";
                ALTER TABLE "__Missions_old" RENAME TO "Missions";
                ALTER TABLE "__FlightResults_old" RENAME TO "FlightResults";
                """);

            migrationBuilder.Sql("PRAGMA foreign_keys = 1;", suppressTransaction: true);
        }
    }
}
