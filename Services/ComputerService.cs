namespace TireTraceabilityDemo.Services;

public class ComputerService
{
    public string GetComputerName()
    {
        return Environment.MachineName;
    }
}