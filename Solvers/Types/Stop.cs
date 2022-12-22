namespace Solvers.Types;

public record Stop(Customer Customer, int ServiceStartedAt, float Distance, int Demand)
{
    public int ServiceCompletedAt => ServiceStartedAt + Customer.ServiceTime;
}
