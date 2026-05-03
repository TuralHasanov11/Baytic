using Baytic.Domain.Veterinarians;

namespace Baytic.Domain.Tests;

public class VeterinarianProfileTests
{
    [Fact]
    public void VerifyCredential_ShouldMarkProfileAsVerified()
    {
        var profile = VeterinarianProfile.Register("vet-1", "Dr. Jane Doe", "Cape Town", 8);
        var credentialId = profile.AddCredential("Board Certification", "Veterinary Board", new DateOnly(2024, 1, 1));

        profile.VerifyCredential(credentialId, "admin-1");

        Assert.Equal(VeterinarianVerificationStatus.Verified, profile.VerificationStatus);
        Assert.True(profile.Credentials.Single().IsVerified);
        Assert.Single(profile.DomainEvents);
        Assert.IsType<VeterinarianCredentialVerifiedDomainEvent>(profile.DomainEvents.Single());
    }
}