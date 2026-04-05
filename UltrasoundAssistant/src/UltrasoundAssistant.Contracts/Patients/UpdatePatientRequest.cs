namespace UltrasoundAssistant.Contracts.Patients;

public sealed class UpdatePatientRequest
{
    public int ExpectedVersion { get; set; }
    public string? FullName { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Gender { get; set; }
}
