using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Heritage_of_Turkey.Migrations
{
    public partial class AddUserIdToContactMessages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Mevcut tablolara yeni kolon ekleme kontrolleri
            migrationBuilder.Sql(@"
                IF COL_LENGTH('ContactMessages', 'UserId') IS NULL
                    ALTER TABLE ContactMessages ADD UserId nvarchar(450) NULL;

                IF COL_LENGTH('ContactMessages', 'AdminReply') IS NULL
                    ALTER TABLE ContactMessages ADD AdminReply nvarchar(1000) NULL;

                IF COL_LENGTH('ContactMessages', 'Status') IS NULL
                    ALTER TABLE ContactMessages ADD Status int NOT NULL DEFAULT 0;

                IF COL_LENGTH('ContactMessages', 'CreatedAt') IS NULL
                    ALTER TABLE ContactMessages ADD CreatedAt datetime2 NOT NULL DEFAULT GETDATE();

                IF COL_LENGTH('ContactMessages', 'RepliedAt') IS NULL
                    ALTER TABLE ContactMessages ADD RepliedAt datetime2 NULL;

                IF COL_LENGTH('Museums', 'GoogleMapsUrl') IS NULL
                    ALTER TABLE Museums ADD GoogleMapsUrl nvarchar(500) NULL;

                IF COL_LENGTH('Ruins', 'GoogleMapsUrl') IS NULL
                    ALTER TABLE Ruins ADD GoogleMapsUrl nvarchar(500) NULL;
            ");

            // 2. ContactMessages Index ve FK kontrolleri
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ContactMessages_UserId' AND object_id = OBJECT_ID('ContactMessages'))
                    CREATE INDEX IX_ContactMessages_UserId ON ContactMessages(UserId);

                IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ContactMessages_AspNetUsers_UserId')
                    ALTER TABLE ContactMessages
                    ADD CONSTRAINT FK_ContactMessages_AspNetUsers_UserId
                    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE SET NULL;
            ");

            // 3. Review Tablolarının ve Indexlerinin SQL Kontrollü Oluşturulması
            migrationBuilder.Sql(@"
                IF OBJECT_ID('MuseumReviews', 'U') IS NULL
                BEGIN
                    CREATE TABLE MuseumReviews (
                        MuseumReviewId int IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        MuseumId int NOT NULL,
                        UserId nvarchar(450) NOT NULL,
                        UserEmail nvarchar(256) NOT NULL,
                        Rating int NOT NULL,
                        CommentText nvarchar(1000) NOT NULL,
                        CreatedAt datetime2 NOT NULL,
                        CONSTRAINT FK_MuseumReviews_AspNetUsers_UserId
                            FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE NO ACTION,
                        CONSTRAINT FK_MuseumReviews_Museums_MuseumId
                            FOREIGN KEY (MuseumId) REFERENCES Museums(MuseumId) ON DELETE CASCADE
                    );
                END;

                IF OBJECT_ID('RuinReviews', 'U') IS NULL
                BEGIN
                    CREATE TABLE RuinReviews (
                        RuinReviewId int IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        RuinId int NOT NULL,
                        UserId nvarchar(450) NOT NULL,
                        UserEmail nvarchar(256) NOT NULL,
                        Rating int NOT NULL,
                        CommentText nvarchar(1000) NOT NULL,
                        CreatedAt datetime2 NOT NULL,
                        CONSTRAINT FK_RuinReviews_AspNetUsers_UserId
                            FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE NO ACTION,
                        CONSTRAINT FK_RuinReviews_Ruins_RuinId
                            FOREIGN KEY (RuinId) REFERENCES Ruins(RuinId) ON DELETE CASCADE
                    );
                END;

                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_MuseumReviews_MuseumId' AND object_id = OBJECT_ID('MuseumReviews'))
                    CREATE INDEX IX_MuseumReviews_MuseumId ON MuseumReviews(MuseumId);

                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_MuseumReviews_UserId' AND object_id = OBJECT_ID('MuseumReviews'))
                    CREATE INDEX IX_MuseumReviews_UserId ON MuseumReviews(UserId);

                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_RuinReviews_RuinId' AND object_id = OBJECT_ID('RuinReviews'))
                    CREATE INDEX IX_RuinReviews_RuinId ON RuinReviews(RuinId);

                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_RuinReviews_UserId' AND object_id = OBJECT_ID('RuinReviews'))
                    CREATE INDEX IX_RuinReviews_UserId ON RuinReviews(UserId);
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Down metodu için de aynı güvenlik önlemlerini içeren SQL
            migrationBuilder.Sql(@"
                IF OBJECT_ID('MuseumReviews', 'U') IS NOT NULL DROP TABLE MuseumReviews;
                IF OBJECT_ID('RuinReviews', 'U') IS NOT NULL DROP TABLE RuinReviews;

                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ContactMessages_AspNetUsers_UserId')
                    ALTER TABLE ContactMessages DROP CONSTRAINT FK_ContactMessages_AspNetUsers_UserId;

                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ContactMessages_UserId' AND object_id = OBJECT_ID('ContactMessages'))
                    DROP INDEX IX_ContactMessages_UserId ON ContactMessages;

                IF COL_LENGTH('ContactMessages', 'UserId') IS NOT NULL ALTER TABLE ContactMessages DROP COLUMN UserId;
                IF COL_LENGTH('ContactMessages', 'AdminReply') IS NOT NULL ALTER TABLE ContactMessages DROP COLUMN AdminReply;
                IF COL_LENGTH('ContactMessages', 'Status') IS NOT NULL ALTER TABLE ContactMessages DROP COLUMN Status;
                IF COL_LENGTH('ContactMessages', 'CreatedAt') IS NOT NULL ALTER TABLE ContactMessages DROP COLUMN CreatedAt;
                IF COL_LENGTH('ContactMessages', 'RepliedAt') IS NOT NULL ALTER TABLE ContactMessages DROP COLUMN RepliedAt;
                IF COL_LENGTH('Museums', 'GoogleMapsUrl') IS NOT NULL ALTER TABLE Museums DROP COLUMN GoogleMapsUrl;
                IF COL_LENGTH('Ruins', 'GoogleMapsUrl') IS NOT NULL ALTER TABLE Ruins DROP COLUMN GoogleMapsUrl;
            ");
        }
    }
}