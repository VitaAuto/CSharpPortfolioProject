using Amazon.SQS;
using Reqnroll;
using Reqnroll.BoDi;
using ApiAndUiProject.API.Context;

namespace ApiAndUiProject.Hooks
{
    [Binding]
    public class SqsQueueHook
    {
        private readonly ObjectContainer _container;
        private readonly IAmazonSQS _sqsClient;
        private readonly ApiContext _context;

        public SqsQueueHook(ObjectContainer container, IAmazonSQS sqsClient, ApiContext context)
        {
            _container = container;
            _sqsClient = sqsClient;
            _context = context;
        }

        [BeforeScenario]
        public async Task RegisterTestQueueUrl()
        {
            var queueName = $"test-queue-{Guid.NewGuid()}";
            var createResponse = await _sqsClient.CreateQueueAsync(queueName);
            var testQueueUrl = createResponse.QueueUrl;
            _context.Set("SqsQueueUrl", testQueueUrl);
        }

        [AfterScenario]
        public async Task DeleteTestQueue()
        {
            var queueUrl = _context.Get<string>("SqsQueueUrl");
            if (!string.IsNullOrEmpty(queueUrl))
            {
                await _sqsClient.DeleteQueueAsync(queueUrl);
            }
        }
    }
}