using Amazon.SQS;
using Amazon.SQS.Model;
using System.Linq;
using System.Threading.Tasks;

namespace ApiControllerProject.Services
{
    public class SqsInitializerService(IAmazonSQS sqsClient) : ISqsInitializerService
    {
        public async Task<string> EnsureQueueExistsAsync(string queueName)
        {
            const int maxAttempts = 10;
            const int delayMs = 2000;
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    var listResponse = await sqsClient.ListQueuesAsync(new ListQueuesRequest { QueueNamePrefix = queueName });
                    var queueUrl = listResponse.QueueUrls?.FirstOrDefault(url => url.EndsWith($"/{queueName}"));
                    if (queueUrl == null)
                    {
                        var createResponse = await sqsClient.CreateQueueAsync(new CreateQueueRequest { QueueName = queueName });
                        queueUrl = createResponse.QueueUrl;
                    }
                    return queueUrl;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Attempt {attempt} failed to connect to SQS: {ex.Message}");
                    if (attempt == maxAttempts)
                        throw;
                    await Task.Delay(delayMs);
                }
            }
            throw new Exception("Failed to connect to SQS after multiple attempts.");
        }
    }
}