using Amazon.SQS;
using Amazon.SQS.Model;
using ApiAndUiProject.API.Models;
using Newtonsoft.Json;

namespace ApiAndUiProject.API.Services
{
    public class SqsService(IAmazonSQS sqsClient)
    {
        public async Task<Message?> GetMessageByCorrelationIdAsync(string queueUrl, string correlationId, int maxAttempts = 10, int delayMs = 2000)
        {
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                var receiveRequest = new ReceiveMessageRequest
                {
                    QueueUrl = queueUrl,
                    MessageAttributeNames = ["All"],
                    MaxNumberOfMessages = 10
                };
                var response = await sqsClient.ReceiveMessageAsync(receiveRequest);

                foreach (var msg in response.Messages)
                {
                    if (msg.MessageAttributes != null &&
                        msg.MessageAttributes.ContainsKey("CorrelationId") &&
                        msg.MessageAttributes["CorrelationId"].StringValue == correlationId)
                    {
                        return msg;
                    }
                }
                await Task.Delay(delayMs);
            }
            return null;
        }

        public async Task DeleteMessageAsync(string queueUrl, string receiptHandle)
        {
            await sqsClient.DeleteMessageAsync(queueUrl, receiptHandle);
        }

        public async Task DeleteMessagesByEmailAsync(string queueUrl, string email)
        {
            while (true)
            {
                var receiveRequest = new ReceiveMessageRequest
                {
                    QueueUrl = queueUrl,
                    MessageAttributeNames = ["All"],
                    MaxNumberOfMessages = 10
                };
                var response = await sqsClient.ReceiveMessageAsync(receiveRequest);

                if (response.Messages == null || response.Messages.Count == 0)
                    break;

                foreach (var msg in response.Messages)
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(msg.Body))
                            continue;

                        var user = JsonConvert.DeserializeObject<User>(msg.Body);

                        if (user != null &&
                            !string.IsNullOrWhiteSpace(user.Email) &&
                            user.Email.Equals(email, StringComparison.OrdinalIgnoreCase) &&
                            user.IsActive == false)
                        {
                            await sqsClient.DeleteMessageAsync(queueUrl, msg.ReceiptHandle);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[SQS CLEANUP] Failed to deserialize message: {ex.Message}");
                    }
                }
            }
        }

    }
}