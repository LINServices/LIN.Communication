﻿using LIN.Communication.Services.Models;

namespace LIN.Communication.Services.Interfaces;

public interface IMessageSender
{
    public Task<ResponseBase> Send(MessageModel message, string guid, ProfileModel remitente);
    public Task<ResponseBase> Send(MessageModel message, string guid, JwtModel remitente);
}