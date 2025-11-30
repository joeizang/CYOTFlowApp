using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FlowApplicationApp.Data.DomainModels;

namespace FlowApplicationApp.Models;

public static class SeedClass
{
    public static FlowRoles[] PrepareRolesSeed()
    {
        return new FlowRoles[7] {
            new() { RoleName = "Music Director", RoleDescription = "This is appointed leader of flow as carried out by the Church leadership", Id = Ulid.NewUlid(DateTimeOffset.Now).ToGuid()},
            new() {
                Id = Ulid.NewUlid(DateTimeOffset.Now).ToGuid(),
                RoleName = "Deputy Music Director",
                RoleDescription = "Assists the Director and shares the load with the director!"
            },
            new() {
                Id = Ulid.NewUlid(DateTimeOffset.Now).ToGuid(),
                RoleName = "Admin",
                RoleDescription = "Application Administrator with full access"
            },
            new() {
                Id = Ulid.NewUlid(DateTimeOffset.Now).ToGuid(),
                RoleName = "Member",
                RoleDescription = "Assists the Director and shares the load with the director!"
            },
            new() {
                Id = Ulid.NewUlid(DateTimeOffset.Now).ToGuid(),
                RoleName = "Vocalist",
                RoleDescription = "Assists the Director and shares the load with the director!"
            },
            new() {
                Id = Ulid.NewUlid(DateTimeOffset.Now).ToGuid(),
                RoleName = "Instrumentalist",
                RoleDescription = "Assists the Director and shares the load with the director!"
            },
            new()
            {
                Id = Ulid.NewUlid(DateTimeOffset.Now).ToGuid(),
                RoleName = "Spiritual Lead",
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
                UserName = "admin",
                PasswordHash = "ForWeCanDoNothing@gainstTheTruth-2026",
                DoB = new DateOnly(1990, 1, 1),
                WhatsAppNumber = "08189011535",
                Bio = "Courtyard of Truth Administrator Account",
                BornAgainDate = new DateOnly(1995, 7, 1),
                ProfileImageUrl = "/img/profilepix612x612.jpg",
                WaterBaptismDate = new DateOnly(2007, 1, 1),
                HolySpiritBaptismDate = new DateOnly(2007, 1, 1),
                HearsGod = true,
                HowTheyStartedHearingGod = "Admin account - N/A",
                CoverSpeech = "I am the administrator of this application.",
                CreatedOn = DateOnly.FromDateTime(DateTime.UtcNow),
                UpdatedOn = DateOnly.FromDateTime(DateTime.UtcNow),
                IsActive = true,
                Roles = [PrepareRolesSeed().Single(x => x.RoleName == "Admin")]
            },
            new()
            {
                Id = Ulid.NewUlid(DateTimeOffset.Now).ToGuid(),
                FirstName = "Sydney",
                LastName = "Udoh",
                Email = "sydney.udoh@gmail.com",
                UserName = "sydney",
                PasswordHash = "ForWeCanDoNothing@gainstTheTruth-2026",
                DoB = new DateOnly(1990, 1, 1),
                WhatsAppNumber = "08031234567",
                Bio = "Courtyard of Truth Flow Director Account",
                BornAgainDate = new DateOnly(1995, 7, 1),
                ProfileImageUrl = "/img/profilepix612x612.jpg",
                WaterBaptismDate = new DateOnly(2007, 1, 1),
                HolySpiritBaptismDate = new DateOnly(2007, 1, 1),
                HearsGod = true,
                HowTheyStartedHearingGod = "Admin account - N/A",
                CoverSpeech = "Director of Flow.",
                CreatedOn = DateOnly.FromDateTime(DateTime.UtcNow),
                UpdatedOn = DateOnly.FromDateTime(DateTime.UtcNow),
                IsActive = true,
                Roles = [PrepareRolesSeed().First(x => x.RoleName == "Admin")]
            },
            new()
            {
                Id = Ulid.NewUlid(DateTimeOffset.Now).ToGuid(),
                FirstName = "IfeOluwa",
                LastName = "Bamgboye",
                Email = "ifeoluwa.bamgboye@gmail.com",
                UserName = "ifeoluwa",
                PasswordHash = "ForWeCanDoNothing@gainstTheTruth-2026",
                DoB = new DateOnly(1975, 12, 1),
                WhatsAppNumber = "08039876543",
                Bio = "Courtyard of Truth Flow Spiritual Lead Account",
                BornAgainDate = new DateOnly(1995, 7, 1),
                ProfileImageUrl = "/img/profilepix612x612.jpg",
                WaterBaptismDate = new DateOnly(2007, 1, 1),
                HolySpiritBaptismDate = new DateOnly(2007, 1, 1),
                HearsGod = true,
                HowTheyStartedHearingGod = "Admin account - N/A",
                CoverSpeech = "I am the spiritual lead of Flow.",
                CreatedOn = DateOnly.FromDateTime(DateTime.UtcNow),
                UpdatedOn = DateOnly.FromDateTime(DateTime.UtcNow),
                IsActive = true,
                Roles = [PrepareRolesSeed().First(x => x.RoleName == "Admin")]
            }
        ];
    }
}
