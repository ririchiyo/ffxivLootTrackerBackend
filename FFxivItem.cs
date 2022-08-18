using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using GoogleSheetsWrapper;
using Microsoft.VisualBasic.FileIO;
using System;

namespace ffxivLootTrackerBackend
{
    public class FFxivItem
    {
        public string ItemName { get; set; }
        public int ItemId { get; set; }
        public string PlayerName { get; set; }
        public string LootEventTypeName { get; set; }
        public UInt64 Timestamp { get; set; }
    }

    public class ffxivLootMessage
    {
        public int itemID;
    }

    public class ffxivItem
    {
        public string timestamp;
        public string lootEventTypeName;
        public ffxivLootMessage lootMessage;
        public string itemName;
        public string playerName;
    }

    public class ffxivItemRecord
    {
        public ffxivItem ffxivitemrecord;
    }

    public class FFXIVRecord : BaseRecord
    {
        [SheetField(
            DisplayName = "Item Name",
            ColumnID = 1,
            FieldType = SheetFieldType.String)]
        public string Item_Name { get; set; }
        [SheetField(
            DisplayName = "Player Name",
            ColumnID = 2,
            FieldType = SheetFieldType.String)]
        public string Player_Name { get; set; }
        [SheetField(
            DisplayName = "LootEventTypeName",
            ColumnID = 3,
            FieldType = SheetFieldType.String)]
        public string LootEventTypeName { get; set; }
        [SheetField(
            DisplayName = "ItemId",
            ColumnID = 4,
            FieldType = SheetFieldType.Integer)]
        public int itemId { get; set; }
        [SheetField(
            DisplayName = "Timestamp",
            ColumnID = 5,
            FieldType = SheetFieldType.String)]
        public string timestamp { get; set; }
        [SheetField(
            DisplayName = "World",
            ColumnID = 6,
            FieldType = SheetFieldType.String)]
        public string world { get; set; }
    }

    public class FFXIVRepository : BaseRepository<FFXIVRecord>
    {
        public FFXIVRepository() { }

        public FFXIVRepository(SheetHelper<FFXIVRecord> sheetsHelper)
            : base(sheetsHelper) { }
    }

}
