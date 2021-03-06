﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;

namespace Bot_Application1
{
    public class MessageParser
    {
        private static LuisService BingCortanaService;

        private static LuisService MyAppService;

        static MessageParser()
        {
            // https://api.projectoxford.ai/luis/v1/application?id=c413b2ef-382c-45bd-8ff0-f76d60e2a821&subscription-key=830b9b434d73481492b5dadc9be1f279&q=
            MessageParser.BingCortanaService = new LuisService(new LuisModelAttribute("c413b2ef-382c-45bd-8ff0-f76d60e2a821", "830b9b434d73481492b5dadc9be1f279"));

            // https://api.projectoxford.ai/luis/v1/application?id=72e18abc-39ba-4f8c-aeee-e5ea9719b88c&subscription-key=830b9b434d73481492b5dadc9be1f279
            MessageParser.MyAppService = new LuisService(new LuisModelAttribute("72e18abc-39ba-4f8c-aeee-e5ea9719b88c", "830b9b434d73481492b5dadc9be1f279"));
        }

        public async Task<IntentCommand> GetIntentCommand(
            string userId,
            string message,
            string serviceUri,
            ChannelAccount from,
            ChannelAccount recipient)
        {
            LuisResult result = await MessageParser.BingCortanaService.QueryAsync(message);
            IntentCommand command;
            if (TryGetCommand(userId, message, serviceUri, from, recipient, result, out command))
            {
                return command;
            }

            result = await MessageParser.MyAppService.QueryAsync(message);
            if (TryGetCommand(userId, message, serviceUri, from, recipient, result, out command))
            {
                return command;
            }

            return new NoOpCommand(result);
        }

        private static bool TryGetCommand(string userId,
            string message,
            string serviceUri,
            ChannelAccount from,
            ChannelAccount recipient,
            LuisResult result,
            out IntentCommand command)
        {
            command = null;
            bool foundMatchingIntent = false;
            switch (result.Intents[0].Intent)
            {
                case "builtin.intent.reminder.create_single_reminder":
                    foundMatchingIntent = CreateToDoCommand.TryGetCommand(userId, message, serviceUri, from, recipient, result, out command);
                    break;

                case "builtin.intent.weather.check_weather":
                    foundMatchingIntent = foundMatchingIntent = WeatherIntentCommand.TryGetCommand(message, result, out command);
                    break;

                case "TaskUpdate":
                    foundMatchingIntent = MessageParser.TryGetCommand(userId, message, result, out command);
                    break;

                case "TaskDisplay":
                    foundMatchingIntent = true;
                    command = new ShowToDoCommand(userId, ToDoItemStatus.Pending);
                    break;

                case "Greeting":
                    foundMatchingIntent = true;
                    command = new GreetingIntentCommand(from.Name);
                    break;

                case "AskingSimpleQuestion":
                    foundMatchingIntent = SimpleQuestionIntentCommand.TryGetCommand(result, out command);
                    break;

                case "None":
                    command = new NoOpCommand(result);
                    break;
            }

            return foundMatchingIntent;
        }

        private static bool TryGetCommand(string userId, string message, LuisResult result, out IntentCommand command)
        {
            IEnumerable<EntityRecommendation> updateTypeEntities =
                result.Entities.Where(e => e.Type.Equals("UpdateType"));
            if (updateTypeEntities.Any())
            {
                switch (updateTypeEntities.ElementAt(0).Entity)
                {
                    case "done with":
                    case "finished":
                    case "complete":
                    case "completed":
                    case "perform":
                    case "performed":
                        IEnumerable<int> elementsToComplete =
                            result.Entities.Where(e => e.Type.Equals("builtin.number")).Select(e => int.Parse(e.Entity));
                        command = new DoneToDoCommands(userId, elementsToComplete);
                        return true;

                    case "remove":
                    case "removed":
                    case "delete":
                    case "get rid of":
                        IEnumerable<int> elementsToRemove =
                             result.Entities.Where(e => e.Type.Equals("builtin.number")).Select(e => int.Parse(e.Entity));
                        command = new RemoveToDoCommands(userId, elementsToRemove);
                        return true;
                }
            }

            command = null;
            return false;
        }

        public static Task<T> GetAwaitable<T>(T itemToreturn)
        {
            Task<T> task = new Task<T>(() => itemToreturn);
            task.Start();
            return task;
        }
    }
}