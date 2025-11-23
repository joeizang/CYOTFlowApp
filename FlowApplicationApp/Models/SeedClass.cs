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
        return new FlowRoles[6] {
            new() { RoleName = "Music Director", RoleDescription = "This is appointed leader of flow as carried out by the Church leadership", Id = Ulid.NewUlid(DateTimeOffset.Now).ToGuid()},
            new() {
                Id = Ulid.NewUlid(DateTimeOffset.Now).ToGuid(),
                RoleName = "Deputy Music Director",
                RoleDescription = "Assists the Director and shares the load with the director!"
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
}
