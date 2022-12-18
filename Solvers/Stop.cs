namespace Solvers;

public record Stop(Customer Customer, int ServiceStartedAt)
{
    public int ServiceCompletedAt => ServiceStartedAt + Customer.ServiceTime;
}
