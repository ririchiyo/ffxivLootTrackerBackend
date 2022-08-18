using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Numerics;
using System.Configuration;
using GoogleSheetsWrapper;
using System.Text.Json;
using System.Net;
using ffxivLootTrackerBackend;
using System.IO;

namespace ffxivLootTrackerBackend.Controllers
{

    [ApiController]
    [Route("api")]
    public class FFxivAPIController : ControllerBase
    {


        private string _spreadsheetId = ConfigurationManager.AppSettings["spreadsheetId"];
        private string connectionUrl = ConfigurationManager.AppSettings["connection_url"];

        private readonly ILogger<FFxivAPIController> _logger;

        public FFxivAPIController(ILogger<FFxivAPIController> logger)
        {
            _logger = logger;
        }

        [HttpGet("item")]
        public async IAsyncEnumerable<FFxivItem> Get([FromQuery]string player, [FromQuery]string world)
        {
            //System.Diagnostics.Debug.WriteLine($"Player = {player}, server = {server}");
            //if (playerName == null || playerName.Length == 0 || playerName.Length > 20) yield return new FFxivItem { };
            await using var connection = new MySqlConnection(connectionUrl);
            await connection.OpenAsync();

            using var command = new MySqlCommand($"SELECT timestamp, lootEventTypeName, itemId, itemName, playerName FROM ffxivItemInfo WHERE playerName = '{player}' AND world = '{world}';", connection);
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                yield return new FFxivItem { Timestamp = reader.GetUInt64(0), LootEventTypeName = reader.GetString(1), ItemId = reader.GetInt32(2), ItemName = reader.GetString(3), PlayerName = reader.GetString(4) };
            }
        }


        [HttpGet("players")]
        public async IAsyncEnumerable<FFxivPlayer> Get()
        {
            await using var connection = new MySqlConnection(connectionUrl);
            await connection.OpenAsync();

            using var command = new MySqlCommand("SELECT DISTINCT playerName, world FROM ffxivItemInfo;", connection);
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                yield return new FFxivPlayer { playerName = reader.GetString(0), world = reader.GetString(1) };
            }

        }

        [HttpGet("players/input")]
        public async IAsyncEnumerable<IActionResult> InsertData([FromBody]JsonElement data)
        {

            if (data.ToString() == null || data.GetArrayLength() > 8) yield return BadRequest();

            await using var connection = new MySqlConnection(connectionUrl);
            await connection.OpenAsync();

            foreach (JsonElement element in data.EnumerateArray())
            {
                FFxivPlayer player = new FFxivPlayer();
                player.playerName = element.GetProperty("playerName").GetString();
                player.world = element.GetProperty("world").GetString();
                FFxivItem item = new FFxivItem();
                item.Timestamp = element.GetProperty("timestamp").GetUInt64();
                item.ItemName = element.GetProperty("itemName").GetString();
                item.ItemId = element.GetProperty("lootMessage").GetProperty("itemId").GetInt32();
                item.LootEventTypeName = element.GetProperty("lootEventTypeName").GetString();
                item.PlayerName = player.playerName;

                using var command = new MySqlCommand($"INSERT INTO `ffxivItemInfo` (world, timestamp, lootEventTypeName, itemId, itemName, playerName) VALUES ('{player.world}','{item.Timestamp}','{item.LootEventTypeName}','{item.ItemId}','{item.ItemName}','{item.PlayerName}');", connection);
                await command.ExecuteNonQueryAsync();

                handleSheets(item, player.world);

                System.Diagnostics.Debug.WriteLine($"{player.playerName}{player.world}");
                System.Diagnostics.Debug.WriteLine($"{item.ItemId}{item.ItemName}{item.PlayerName}{item.LootEventTypeName}{item.Timestamp}");
            }

            yield return Ok();

        }

        public void handleSheets(FFxivItem item, string world)
        {
            var serviceAccount = ConfigurationManager.AppSettings["serviceAccount"];
            var jsonCredspath = "credentials.json";
            var jsonCredsContent = System.IO.File.ReadAllText(jsonCredspath);

            var sheetHelper = new SheetHelper<FFXIVRecord>(_spreadsheetId, serviceAccount, ConfigurationManager.AppSettings["sheetName"]);

            sheetHelper.Init(jsonCredsContent);

            var repository = new FFXIVRepository(sheetHelper);

            var s_result = repository.ValidateSchema();

            if (!s_result.IsValid)
            {
                throw new ArgumentException(s_result.ErrorMessage);
            }

            repository.AddRecord(new FFXIVRecord()
            {
                Item_Name = item.ItemName,
                itemId = item.ItemId,
                Player_Name = item.PlayerName,
                LootEventTypeName = item.LootEventTypeName,
                timestamp = item.Timestamp.ToString(),
                world = world
            });
        }

    }


}



/*
 *         public string ItemName { get; set; }
        public int ItemId { get; set; }
        public string PlayerName { get; set; }
        public string LootEventTypeName { get; set; }
        public UInt64 Timestamp { get; set; }
*/
