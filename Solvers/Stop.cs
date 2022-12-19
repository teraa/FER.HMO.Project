namespace Solvers;

public record Stop(Customer Customer, int ServiceStartedAt, float Distance)
{
    public int ServiceCompletedAt => ServiceStartedAt + Customer.ServiceTime;
}
