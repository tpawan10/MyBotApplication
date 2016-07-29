using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Bot.Connector;

namespace Bot_Application1
{
    public class ToDoItemsManager
    {
        public static Timer executeReminderTimer;

        static ToDoItemsManager()
        {
            executeReminderTimer = new Timer(ToDoItemsManager.TimerCallBackMethod, null, 10 * 1000, 5 * 60 * 1000);
        }

        public static void TimerCallBackMethod(object state)
        {
            ReminderItems(GetAllTasksToRemind(DateTime.Now));
        }

        private static async void ReminderItems(IEnumerable<ToDoItem> itemsToRemind)
        {
            foreach (ToDoItem item in itemsToRemind)
            {
                var connector = new ConnectorClient(new Uri(item.CommunicationInformation.ServiceUri));
                var ConversationId =
                    await
                        connector.Conversations.CreateDirectConversationAsync(
                            item.CommunicationInformation.Recipient, item.CommunicationInformation.From);
                IMessageActivity message = Activity.CreateMessageActivity();
                message.From = item.CommunicationInformation.Recipient;
                message.Recipient = item.CommunicationInformation.From;
                message.Conversation = new ConversationAccount(id: ConversationId.Id);
                message.Text = "You need to: " + item.Title;
                message.Locale = "en-Us";
                connector.Conversations.SendToConversation((Activity)message);

                // Update item.
                StorageManager.UpdateToDoItem(item);
            }
        }

        private static IEnumerable<ToDoItem> GetAllTasksToRemind(DateTime timeNow)
        {
            List<ToDoItem> itemsToRemind = new List<ToDoItem>();
            foreach (ToDoItem item in StorageManager.GetAllToDoItemsToRemind(timeNow))
            {
                itemsToRemind.Add(item);
                item.SetNextRemind();
            }

            return itemsToRemind;
        }

        public static void AddToDoItem(string userId, ToDoItem item)
        {
            StorageManager.InsertItem(item);
        }

        public static void RemoveItems(string userId, IEnumerable<ToDoItem> itemsToRemove)
        {
            foreach (ToDoItem item in itemsToRemove)
            {
                StorageManager.RemoveItem(item);
            }
        }
    }
}