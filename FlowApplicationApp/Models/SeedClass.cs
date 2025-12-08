using FlowApplicationApp.Data.DomainModels;
using Microsoft.AspNetCore.Identity;

namespace FlowApplicationApp.Models;

public static class SeedClass
{
    public static FlowRoles[] PrepareRolesSeed()
    {
        return new FlowRoles[7] {
            new() { RoleName = "Music Director",
                Name = "Music-Director",
                RoleDescription = "This is appointed leader of flow as carried out by the Church leadership", Id = Ulid.NewUlid(DateTimeOffset.Now).ToGuid()},
            new() {
                Id = Ulid.NewUlid(DateTimeOffset.Now).ToGuid(),
                RoleName = "Deputy Music Director",
                Name = "Deputy-Music-Director",
                RoleDescription = "Assists the Director and shares the load with the director!"
            },
            new() {
                Id = Ulid.NewUlid(DateTimeOffset.Now).ToGuid(),
                RoleName = "Admin",
                Name = "Admin",
                RoleDescription = "Application Administrator with full access"
            },
            new() {
                Id = Ulid.NewUlid(DateTimeOffset.Now).ToGuid(),
                RoleName = "Member",
                Name = "Member",
                RoleDescription = "Assists the Director and shares the load with the director!"
            },
            new() {
                Id = Ulid.NewUlid(DateTimeOffset.Now).ToGuid(),
                RoleName = "Vocalist",
                Name = "Vocalist",
                RoleDescription = "Assists the Director and shares the load with the director!"
            },
            new() {
                Id = Ulid.NewUlid(DateTimeOffset.Now).ToGuid(),
                RoleName = "Instrumentalist",
                Name = "Instrumentalist",
                RoleDescription = "Assists the Director and shares the load with the director!"
            },
            new()
            {
                Id = Ulid.NewUlid(DateTimeOffset.Now).ToGuid(),
                RoleName = "Spiritual Lead",
                Name = "",
                RoleDescription = "Provides spiritual leadership and oversight to the flow team"
            }

        };
    }

    public static FlowMember[] PrepareAdminMemberSeed()
    {
        return
        [
            new()
            {
                Id = Ulid.NewUlid(DateTimeOffset.Now).ToGuid(),
                FirstName = "Joseph",
                LastName = "Izang",
                Email = "josephizang@gmail.com",
                UserName = "josephizang@gmail.com",
                DoB = new DateTime(1990, 1, 1),
                WhatsAppNumber = "08189011535",
                Bio = "Courtyard of Truth Administrator Account",
                BornAgainDate = new DateTime(1995, 7, 1),
                ProfileImageUrl = "/img/profilepix612x612.jpg",
                WaterBaptismDate = new DateTime(2007, 1, 1),
                HolySpiritBaptismDate = new DateTime(2007, 1, 1),
                HearsGod = true,
                HowTheyStartedHearingGod = "Admin account - N/A",
                CoverSpeech = "I am the administrator of this application.",
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow,
                IsActive = true,
                EmailConfirmed = true,
                Roles = [PrepareRolesSeed().Single(x => x.RoleName == "Admin")]
            },
            new()
            {
                Id = Ulid.NewUlid(DateTimeOffset.Now).ToGuid(),
                FirstName = "Sydney",
                LastName = "Udoh",
                Email = "sydney.udoh@gmail.com",
                UserName = "sydney.udoh@gmail.com",
                DoB = new DateTime(1990, 1, 1),
                WhatsAppNumber = "08031234567",
                Bio = "Courtyard of Truth Flow Director Account",
                BornAgainDate = new DateTime(1995, 7, 1),
                ProfileImageUrl = "/img/profilepix612x612.jpg",
                WaterBaptismDate = new DateTime(2007, 1, 1),
                HolySpiritBaptismDate = new DateTime(2007, 1, 1),
                HearsGod = true,
                HowTheyStartedHearingGod = "Admin account - N/A",
                CoverSpeech = "Director of Flow.",
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow,
                IsActive = true,
                EmailConfirmed = true,
                Roles = [PrepareRolesSeed().First(x => x.RoleName == "Admin")]
            },
            new()
            {
                Id = Ulid.NewUlid(DateTimeOffset.Now).ToGuid(),
                FirstName = "IfeOluwa",
                LastName = "Bamgboye",
                Email = "ifeoluwa.bamgboye@gmail.com",
                UserName = "ifeoluwa.bamgboye@gmail.com",
                DoB = new DateTime(1975, 12, 1),
                WhatsAppNumber = "08039876543",
                Bio = "Courtyard of Truth Flow Spiritual Lead Account",
                BornAgainDate = new DateTime(1995, 7, 1),
                ProfileImageUrl = "/img/profilepix612x612.jpg",
                WaterBaptismDate = new DateTime(2007, 1, 1),
                HolySpiritBaptismDate = new DateTime(2007, 1, 1),
                HearsGod = true,
                HowTheyStartedHearingGod = "Admin account - N/A",
                CoverSpeech = "I am the spiritual lead of Flow.",
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow,
                IsActive = true,
                EmailConfirmed = true,
                Roles = [PrepareRolesSeed().First(x => x.RoleName == "Admin")]
            }
        ];
    }
}
