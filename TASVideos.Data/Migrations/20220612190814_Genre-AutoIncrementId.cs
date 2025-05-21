﻿using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TASVideos.Data.Migrations;

public partial class GenreAutoIncrementId : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AlterColumn<int>(
				name: "id",
				table: "genres",
				type: "integer",
				nullable: false,
				oldClrType: typeof(int),
				oldType: "integer")
			.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
	}

	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AlterColumn<int>(
				name: "id",
				table: "genres",
				type: "integer",
				nullable: false,
				oldClrType: typeof(int),
				oldType: "integer")
			.OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
	}
}
