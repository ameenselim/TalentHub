using System;
using System.Collections.Generic;
using System.Text;

namespace TalentHub.Domain.Enums.Application
{
    public enum ApplicationStatus
    {
        Applied,
        UnderReview,
        HRInterview,
        TechnicalInterview,
        FinalInterview,
        OfferSent,
        Hired,
        Rejected
    }
}
