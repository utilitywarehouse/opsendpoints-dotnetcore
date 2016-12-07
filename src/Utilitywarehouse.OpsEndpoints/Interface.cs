namespace Utilitywarehouse.OpsEndpoints
{
    public interface IApplicationHealthModel
    {
        bool Ready();
        HealthInfo Health();
        AboutInfo About();
    }

    public interface ICheck
    {
        CheckResult Run();
    }
}