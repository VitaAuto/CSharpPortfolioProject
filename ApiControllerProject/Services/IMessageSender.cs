namespace ApiControllerProject.Services
{
    public interface IMessageSender
    {
        Task SendMessageAsync(string queueUrl, string messageBody, IDictionary<string, string> attributes = null);
    }
}