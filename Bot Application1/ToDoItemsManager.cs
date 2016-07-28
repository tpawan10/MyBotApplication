using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Bot.Connector;

namespace Bot_Application1
{
    public class ToDoItemsManager
    {
        public static Dictionary<string, List<ToDoItem>> toDoItemsCollection;

        public static Timer executeReminderTimer;

        static ToDoItemsManager()
        {
            toDoItemsCollection = new Dictionary<string, List<ToDoItem>>();
            executeReminderTimer = new Timer(ToDoItemsManager.TimerCallBackMethod, null, 10 * 1000, 30 * 1000);
        }

        public static void TimerCallBackMethod(object state)
        {
            DateTime timeNow = DateTime.Now;
            IEnumerable<ToDoItem> itemsToRemind = GetAllTasksToRemind(timeNow);
            ReminderItems(itemsToRemind);
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
            }
        }

        private static IEnumerable<ToDoItem> GetAllTasksToRemind(DateTime timeNow)
        {
            List<ToDoItem> itemsToRemind = new List<ToDoItem>();
            foreach (string user in toDoItemsCollection.Keys)
            {
                foreach (ToDoItem item in GetToDoItemsForUser(user).Where(todoItem => todoItem.NextRemind <= timeNow))
                {
                    itemsToRemind.Add(item);
                    item.SetNextRemind();
                }
            }

            return itemsToRemind;
        }

        public static void AddToDoItem(string userId, ToDoItem item)
        {
            List<ToDoItem> items;
            if (!toDoItemsCollection.TryGetValue(userId, out items) || items == null)
            {
                items = new List<ToDoItem>();
            }

            items.Add(item);
            toDoItemsCollection[userId] = items;
        }

        public static void RemoveItems(string userId, IEnumerable<ToDoItem> itemsToRemove)
        {
            List<ToDoItem> items;
            if (!toDoItemsCollection.TryGetValue(userId, out items) || items == null)
            {
                items = new List<ToDoItem>();
            }

            foreach (ToDoItem item in itemsToRemove)
            {
                items.Remove(item);
            }
        }

        public static IEnumerable<ToDoItem> GetToDoItemsForUser(string userId, ToDoItemStatus status = ToDoItemStatus.Pending)
        {
            List<ToDoItem> items;
            if (!toDoItemsCollection.TryGetValue(userId, out items))
            {
                items = new List<ToDoItem>();
            }
            return items.Where(item => item.Status.Equals(status));
        }
    }
}