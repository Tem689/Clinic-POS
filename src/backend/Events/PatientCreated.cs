namespace Clinic.Backend.Events;

public record PatientCreated(int PatientId, int TenantId, string FullName);
