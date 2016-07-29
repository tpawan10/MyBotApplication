using System;
using System.Linq;
using Bot_Application1;
using Microsoft.Bot.Connector;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BotTests
{
    [TestClass]
    public class ToDoTableOperationTests
    {
        [TestMethod]
        public void InsertTableOperation()
        {
            string guid = Guid.NewGuid().ToString();
            TimeSpan timeSpan = TimeSpan.FromSeconds(3);
            ToDoItem item = new ToDoItem(guid, "This is a test.", timeSpan);
            item.SetCommunicationInformation(
                new CommunicationInfo()
                {
                    From = new ChannelAccount { Id = "Id", Name = "name" },
                    Recipient = new ChannelAccount { Id = "recipientId", Name = "recipientName" },
                    ServiceUri = "ARandomUri"
                });
            StorageManager.InsertItem(item);

            ToDoItem[] result = StorageManager.GetAllToDoItemsForUser(guid).ToArray();
            Assert.AreEqual(result.Length, 1);
        }
    }
}