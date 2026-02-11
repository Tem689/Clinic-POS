using Clinic.Backend.Models;

namespace Clinic.Backend.Services;

public interface ICurrentTenant
{
    int? Id { get; }
    string? Name { get; }
    void SetTenant(int id, string name);
}

public class CurrentTenant : ICurrentTenant
{
    public int? Id { get; private set; }
    public string? Name { get; private set; }

    public void SetTenant(int id, string name)
    {
        Id = id;
        Name = name;
    }
}
