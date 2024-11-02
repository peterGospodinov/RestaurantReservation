using System;

public class InsertRequestResultModel
{
    // Input Parameters
    public string ClientName { get; set; }
    public string ClientPhone { get; set; }
    public int TableNumber { get; set; }
    public DateTime DateOfReservation { get; set; }
    public byte Status { get; set; }
    public string Raw { get; set; }

    // Output Parameters
    public byte Result { get; set; }
    public string ResultText { get; set; }
}