﻿using System;

namespace Funtest.TransferObject.Email.Requests
{
    public class DataToInvitationLinkRequest
    {
        public string Email { get; set; }
        public string Role { get; set; }
        public Guid ProductId { get; set; }
    }
}
