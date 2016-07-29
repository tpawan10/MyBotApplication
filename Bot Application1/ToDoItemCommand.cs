// ---------------------------------------------------------------------------
// <copyright file="ToDoItemCommand.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ---------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;

namespace Bot_Application1
{
    // https://api.projectoxford.ai/luis/v1/application?id=e99f0d06-6f54-4376-bbde-3f5afaa8aa84&subscription-key=830b9b434d73481492b5dadc9be1f279

    public abstract class ToDoItemCommand : IntentCommand
    {
        protected string UserId;

        protected ToDoItemCommand(string userId)
        {
            this.UserId = userId;
        }
    }

    public class NoOpCommand : ToDoItemCommand
    {
        private string text;

        public NoOpCommand(string userId, string text) : base(userId)
        {
            this.text = text;
        }

        public override async Task<string> Execute()
        {
            return "I did not understand your command.";
        }
    }

    public class CreateToDoCommand : ToDoItemCommand
    {
        private ToDoItem itemToAdd;
        private static LuisService service;

        static CreateToDoCommand()
        {
            // https://api.projectoxford.ai/luis/v1/application?id=c413b2ef-382c-45bd-8ff0-f76d60e2a821&subscription-key=830b9b434d73481492b5dadc9be1f279&q=
            LuisModelAttribute attribute = new LuisModelAttribute(
               "c413b2ef-382c-45bd-8ff0-f76d60e2a821", "830b9b434d73481492b5dadc9be1f279");
            CreateToDoCommand.service = new LuisService(attribute);
        }

        public static bool TryGetCommand(string userId, string message, string url,
            ChannelAccount from,
            ChannelAccount recipient, LuisResult result, out IntentCommand command)
        {
            if (CreateToDoCommand.ValidateIntent(result.Intents))
            {
                ToDoItem expectedItem;
                if (CreateToDoCommand.TryCreateToDoItem(result.Entities, out expectedItem))
                {
                    expectedItem.SetCommunicationInformation(
                        new CommunicationInfo() { ServiceUri = url, From = from, Recipient = recipient });
                    command = new CreateToDoCommand(userId, expectedItem);
                    return true;
                }
            }

            command = null;
            return false;
        }

        public CreateToDoCommand(string userId, ToDoItem itemToAdd) : base(userId)
        {
            this.itemToAdd = itemToAdd;
        }

        public override async Task<string> Execute()
        {
            ToDoItemsManager.AddToDoItem(this.UserId, this.itemToAdd);
            return this.itemToAdd.Title + " was added";
        }

        private static bool TryCreateToDoItem(IList<EntityRecommendation> entities, out ToDoItem expectedItem)
        {
            expectedItem = null;
            if (entities != null && entities.Count != 0)
            {
                string text = string.Join(
                    " and ",
                    entities.Where(e => e.Type.Equals("builtin.reminder.reminder_text"))
                        .Select(e => e.Entity));

                IEnumerable<IDictionary<string, string>> resolutions =
                    entities.Where(
                        e =>
                            e.Type.Equals("builtin.reminder.start_date") || e.Type.Equals("builtin.reminder.start_time"))
                        .Select(e => e.Resolution);

                DateTime? finalDateTime = null;
                TimeSpan frequency = TimeSpan.MaxValue;

                var setDate = resolutions.Where(r => r["resolution_type"].Equals("builtin.datetime.set")).ToArray();
                if (setDate.Length != 0)
                {
                    int hours, mins;
                    if (setDate[0]["set"].Equals("xxxx-xx-xx", StringComparison.OrdinalIgnoreCase))
                    {
                        finalDateTime = DateTime.Today.AddHours(8);
                        frequency = TimeSpan.FromDays(1);
                        expectedItem = new ToDoItem(text, finalDateTime.Value, frequency);
                        return true;
                    }
                    if (setDate[0]["set"].StartsWith("xxxx-xx-xx", StringComparison.OrdinalIgnoreCase))
                    {
                        if (TryParseHourAndMinute(setDate[0]["set"], out hours, out mins))
                        {
                            finalDateTime = DateTime.Today.AddHours(hours).AddMinutes(mins);
                            frequency = TimeSpan.FromDays(1);
                            expectedItem = new ToDoItem(text, finalDateTime.Value, frequency);
                            return true;
                        }
                    }
                }

                var dateRes = resolutions.Where(r => r["resolution_type"].Equals("builtin.datetime.date")).ToArray();
                if (dateRes.Length != 0 && !finalDateTime.HasValue)
                {
                    finalDateTime = DateTime.Parse(dateRes[0]["date"]);
                }

                if (!finalDateTime.HasValue)
                {
                    finalDateTime = DateTime.Today;
                }

                var timeRes = resolutions.Where(r => r["resolution_type"].Equals("builtin.datetime.time")).ToArray();
                if (timeRes.Length != 0)
                {
                    int hours, mins;
                    if (TryParseHourAndMinute(timeRes[0]["time"], out hours, out mins))
                    {
                        finalDateTime = finalDateTime.Value.AddHours(hours).AddMinutes(mins);
                    }
                    else
                    {
                        return false;
                    }
                }

                expectedItem = new ToDoItem(title: text, nextRemind: finalDateTime.Value, remindInterval: frequency);
                return true;
            }

            return false;
        }

        private static bool TryParseHourAndMinute(string text, out int hours, out int mins)
        {
            hours = 0;
            mins = 0;
            Regex rex = new Regex(@".*T(\d+)[:]*(\d*).*");
            Match match = rex.Match(text);
            if (match.Success)
            {
                int.TryParse(match.Groups[1].Value, out hours);
                int.TryParse(match.Groups[2].Value, out mins);
                return true;
            }

            throw new Exception();
        }

        private static bool ValidateIntent(IList<IntentRecommendation> intents)
        {
            return !(intents == null
                     || intents.Count == 0
                     || !intents[0].Intent.Equals(
                         "builtin.intent.reminder.create_single_reminder",
                         StringComparison.InvariantCultureIgnoreCase)
                     || !intents[0].Intent.Equals(
                         "builtin.intent.weather.check_weather",
                         StringComparison.InvariantCultureIgnoreCase)
                         );
        }
    }

    public class ShowToDoCommand : ToDoItemCommand
    {
        private ToDoItemStatus taskStatusToShow;

        public ShowToDoCommand(string userId, ToDoItemStatus status) : base(userId)
        {
            this.taskStatusToShow = status;
        }

        public override async Task<string> Execute()
        {
            ToDoItem[] items = ToDoItemsManager.GetToDoItemsForUser(this.UserId, this.taskStatusToShow).ToArray();

            if (items != null && items.Length != 0)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < items.Length; i++)
                {
                    sb.Append("Item ");
                    sb.Append(i + 1);
                    sb.Append(": ");
                    sb.Append(items[i].Title);
                    sb.Append("\n\n");
                }
                return sb.ToString();
            }

            return "No task found";
        }
    }

    public class DoneToDoCommands : ToDoItemCommand
    {
        private readonly List<ToDoItem> itemsCompleted;

        public DoneToDoCommands(string userId, IEnumerable<int> itemsCompletedIndex) : base(userId)
        {
            ToDoItem[] items = ToDoItemsManager.GetToDoItemsForUser(this.UserId).ToArray();
            itemsCompleted = new List<ToDoItem>();
            foreach (int itemIndex in itemsCompletedIndex.Where(i => i >= 1 && i <= items.Length))
            {
                itemsCompleted.Add(items[itemIndex - 1]);
            }
        }

        public override async Task<string> Execute()
        {
            foreach (ToDoItem item in this.itemsCompleted)
            {
                item.UpdateStatus(ToDoItemStatus.Done);
            }

            return "Your task completion was marked successfully.";
        }
    }

    public class RemoveToDoCommands : ToDoItemCommand
    {
        private readonly List<ToDoItem> itemsToRemove;

        public RemoveToDoCommands(string userId, IEnumerable<int> itemsToRemoveIndex) : base(userId)
        {
            itemsToRemove = new List<ToDoItem>();
            ToDoItem[] items = ToDoItemsManager.GetToDoItemsForUser(this.UserId).ToArray();

            foreach (int itemIndex in itemsToRemoveIndex.Where(i => i >= 1 && i <= items.Length))
            {
                itemsToRemove.Add(items[itemIndex - 1]);
            }
        }

        public override async Task<string> Execute()
        {
            ToDoItemsManager.RemoveItems(this.UserId, this.itemsToRemove);

            return "Your task completion was marked successfully.";
        }
    }
}