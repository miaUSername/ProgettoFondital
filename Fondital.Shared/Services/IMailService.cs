﻿using Fondital.Shared.Dto;

namespace Fondital.Shared.Services
{
    public interface IMailService
    {
        void SendEmailAsync(MailRequestDto mailRequest);
    }
}