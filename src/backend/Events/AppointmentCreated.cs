namespace Clinic.Backend.Events;

public record AppointmentCreated(
    int AppointmentId,
    int TenantId,
    int BranchId,
    int PatientId,
    DateTime StartAt
);
