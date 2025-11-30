using System;
using FlowApplicationApp.Data.DomainModels;
using FlowApplicationApp.ViewModels.Auditions;

namespace FlowApplicationApp.Infrastructure.Extensions;

public static class ViewModelExtensions
{
    extension(CreateAuditionerInputModel model)
    {
        public FlowAuditioner MapToDomainModel()
        {
            return new FlowAuditioner
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                WhatsAppNumber = model.PhoneNumber,
                DoB = model.DoB,
                Bio = model.Bio,
                BornAgainDate = model.BornAgainDate,
                WaterBaptismDate = model.WaterBaptismDate,
                HolySpiritBaptismDate = model.HolySpiritBaptismDate,
                HearsGod = model.HearsGod,
                HowTheyStartedHearingGod = model.HowTheyStartedHearingGod,
                CoverSpeech = model.CoverSpeech,
                ProfileImageUrl = model.GetProfileImageFileName()
            };
        }

        public string GetProfileImageFileName()
        {
            return $"{Ulid.NewUlid().ToGuid()}_{model.ProfileImage.FileName}";
        }
    }
}
