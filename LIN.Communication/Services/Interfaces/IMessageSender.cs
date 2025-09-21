namespace LIN.Communication.Services.Interfaces;

public interface IMessageSender
{
    public Task<ResponseBase> Send(MessageModel message, string guid, ProfileModel remitente, DateTime? timeToSend = null);
    public Task<ResponseBase> Send(MessageModel message, string guid, JwtModel remitente, DateTime? timeToSend = null);
    Task<ResponseBase> SendSystem(MessageModel message);
}