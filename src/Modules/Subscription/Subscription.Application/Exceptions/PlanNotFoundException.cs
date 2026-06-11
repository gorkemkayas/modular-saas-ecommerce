namespace Subscription.Application.Exceptions;

public sealed class PlanNotFoundException : ApplicationException
{
    public PlanNotFoundException(string planCode)
        : base($"Subscription plan '{planCode}' was not found.")
    {
        PlanCode = planCode;
    }

    public string PlanCode { get; }
}
