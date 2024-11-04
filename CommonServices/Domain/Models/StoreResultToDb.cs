using System;

public class StoreResultToDb
{
    // Input Parameters  
    public string Raw { get; set; }
    public DateTime Dt { get; set; }
    public int ValidationResult { get; set; }

    // Output Parameters
    public byte Result { get; set; }
    public string ResultText { get; set; }
}