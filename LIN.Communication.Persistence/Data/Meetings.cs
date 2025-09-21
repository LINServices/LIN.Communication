namespace LIN.Communication.Persistence.Data;

public class Meetings(Context context)
{

    /// <summary>
    /// Obtener los dispositivos usados en una reunión por un perfil.
    /// </summary>
    public async Task<ReadAllResponse<DeviceModel>> ReadDevices(int profileId, int conversationId)
    {

        var xx = from member in context.MeetingMembers
                 where
                 member.Meeting.EndTime == null // Reunión no haya finalizado.
                 && member.ProfileId == profileId // Perfil sea parte de la reunión.
                 && member.Meeting.ConversationId == conversationId // Conversación.
                 select member.Devices;

        return new(Responses.Success, await xx.SelectMany(t => t).ToListAsync());
    }

    public async Task<ReadOneResponse<MeetingModel>> FindMeet(int conversationId)
    {

        var xx = await (from meet in context.Meetings
                        where meet.ConversationId == conversationId
                        && meet.EndTime == null
                        select meet).FirstOrDefaultAsync();

        if (xx is null)
            return new(Responses.NotRows);

        return new(Responses.Success, xx);
    }

    public async Task<ReadOneResponse<MeetingMemberModel>> FindMeetMember(int conversationId, int profile)
    {

        var xx = await (from meetMember in context.MeetingMembers
                        where meetMember.ProfileId == profile
                        && meetMember.Meeting.ConversationId == conversationId
                        && meetMember.Meeting.EndTime == null
                        select meetMember).FirstOrDefaultAsync();

        if (xx is null)
            return new(Responses.NotRows);

        return new(Responses.Success, xx);
    }

    public async Task<CreateResponse> AddDevice(DeviceModel model)
    {
        try
        {
            model.MeetingMember = context.AttachOrUpdate(model.MeetingMember);

            context.Devices.Add(model);
            await context.SaveChangesAsync();
            return new(Responses.Success, model.Id);
        }
        catch
        {
            return new(Responses.Undefined);
        }
    }


    public async Task<CreateResponse> AddMember(MeetingMemberModel model)
    {
        try
        {
            model.ProfileModel = context.AttachOrUpdate(model.ProfileModel);
            model.Meeting = context.AttachOrUpdate(model.Meeting);

            context.MeetingMembers.Add(model);
            await context.SaveChangesAsync();
            return new(Responses.Success, model.Id);
        }
        catch
        {
            return new(Responses.Undefined);
        }

    }

    public async Task<ResponseBase> FinalizeCall(int meeting)
    {
        try
        {
            var up = await (from meet in context.Meetings
                            where meet.Id == meeting
                            select meet).ExecuteUpdateAsync(t => t.SetProperty(t => t.EndTime, DateTime.UtcNow));


            return new(Responses.Success);
        }
        catch
        {
            return new(Responses.Undefined);
        }
    }


    public async Task<CreateResponse> AddMeeting(MeetingModel model)
    {
        try
        {
            model.Conversation = context.AttachOrUpdate(model.Conversation);

            context.Meetings.Add(model);
            await context.SaveChangesAsync();
            return new(Responses.Success, model.Id);
        }
        catch
        {
            return new(Responses.Undefined);
        }
    }

    public async Task<ReadAllResponse<DeviceModel>> ReadDevices(int conversationId)
    {

        var xx = await (from meetMember in context.MeetingMembers
                        where meetMember.Meeting.ConversationId == conversationId
                        && meetMember.Meeting.EndTime == null
                        select meetMember.Devices).SelectMany(t => t).ToListAsync();

        if (xx is null)
            return new(Responses.NotRows);

        return new(Responses.Success, xx);
    }

    public async Task<ResponseBase> DeleteDevice(int deviceId)
    {

        var xx = await (from device in context.Devices
                        where device.Id == deviceId
                        select device).ExecuteDeleteAsync();

        return new(Responses.Success);
    }

    public async Task<ReadOneResponse<DeviceModel>> ReadDeviceBySignal(string id)
    {

        var xx = await (from device in context.Devices
                        where device.DeviceIdentifier == id
                        select new DeviceModel
                        {
                            DeviceIdentifier = device.DeviceIdentifier,
                            DeviceName = device.DeviceName,
                            Id = device.Id,
                            MeetingMemberId = device.MeetingMemberId,
                            MeetingMember = new MeetingMemberModel
                            {
                                Id = device.MeetingMember.Id,
                                ProfileId = device.MeetingMember.ProfileId,
                                MeetingId = device.MeetingMember.MeetingId,
                                Meeting = new MeetingModel
                                {
                                    Id = device.MeetingMember.Meeting.Id,
                                    ConversationId = device.MeetingMember.Meeting.ConversationId,
                                    StartTime = device.MeetingMember.Meeting.StartTime,
                                    EndTime = device.MeetingMember.Meeting.EndTime,
                                }
                            }
                        }).FirstOrDefaultAsync();

        if (xx is null)
            return new(Responses.NotRows);

        return new(Responses.Success, xx);
    }

}