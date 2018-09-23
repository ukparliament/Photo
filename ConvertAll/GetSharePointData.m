let
    Site = SharePoint.Tables("https://hopuk.sharepoint.com/sites/datateam/datasources", [ApiVersion = 15]),
    PhotoList = Site{[Title = "Photo"]},
    PhotoItems = PhotoList[Items],
    Filtered = Table.SelectRows(PhotoItems , each ([Image in databases] = true)),
    Removed = Table.SelectColumns(Filtered, {"PhotoID", "ImageResourceUri"}),
    Json = Json.FromValue(Removed),
    Text = Text.FromBinary(Json)
in
    Text
