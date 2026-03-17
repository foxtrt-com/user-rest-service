using UserService.Dtos;

namespace UserService.AsyncDataServices;

public interface IMessageBusClient
{
    void PublishNewUser(UserPublishedDto userPublishedDto);
}
