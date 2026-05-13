using Amazon.SQS;
using Amazon.SQS.Model;

namespace ApiControllerProject.Services
{
    public class SqsMessageSender : IMessageSender
    {
        private readonly IAmazonSQS _sqsClient;

        public SqsMessageSender(IAmazonSQS sqsClient)
        {
            _sqsClient = sqsClient;
        }

        public async Task SendMessageAsync(string queueUrl, string messageBody, IDictionary<string, string> attributes = null)
        {
            var sendMessageRequest = new SendMessageRequest
            {
                QueueUrl = queueUrl,
                MessageBody = messageBody,
                MessageAttributes = attributes?.ToDictionary(
                    kvp => kvp.Key,
                    kvp => new MessageAttributeValue
                    {
                        DataType = "String",
                        StringValue = kvp.Value
                    }
                ) ?? new Dictionary<string, MessageAttributeValue>()
            };

            await _sqsClient.SendMessageAsync(sendMessageRequest);
        }
    }
}