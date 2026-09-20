namespace CeyPASS.Entities.Concrete
{
    /// <summary>Sistem log seviyesi (Serilog benzeri sıra).</summary>
    public enum IslemTuru:byte
    {
        Trace = 0, 
        Debug = 1, 
        Info = 2, 
        Warn = 3, 
        Error = 4, 
        Fatal = 5
    }
}
