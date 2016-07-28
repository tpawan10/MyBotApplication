// ---------------------------------------------------------------------------
// <copyright file="IntentCommand.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ---------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Luis.Models;

namespace Bot_Application1
{
    public abstract class IntentCommand
    {
        public abstract Task<string> Execute();
    }

    public class GreetingIntentCommand : IntentCommand
    {
        private string userName;

        private static string[] Greetings = new string[]
        {
            "hello {0} :)",
            "Howdie {0}",
            "So {0}, what's up?",
            "Hey buddy, Welcome back {0}!!! ",
            "Great to see you {0}!!!!"
        };

        private static Random random = new Random();

        public GreetingIntentCommand(string userName)
        {
            this.userName = userName;
        }

        public override Task<string> Execute()
        {
            Task<string> task = new Task<string>(() => string.Format(Greetings[random.Next(0, Greetings.Length)], this.userName));
            task.Start();
            return task;
        }
    }

    public class SimpleQuestionIntentCommand : IntentCommand
    {
        private string question;
        private string who;
        private string noun;
        private string verb;

        public SimpleQuestionIntentCommand(LuisResult luisResult)
        {
            foreach (EntityRecommendation entity in luisResult.Entities)
            {
                switch (entity.Type)
                {
                    case "question":
                        this.question = entity.Entity;
                        break;

                    case "Who":
                        this.who = entity.Entity;
                        break;

                    case "Verb":
                        this.verb = entity.Entity;
                        break;

                    case "Noun":
                        this.noun = entity.Entity;
                        break;
                }
            }
        }

        public override Task<string> Execute()
        {
            string responseWho = SimpleQuestionIntentCommand.GetResponseWho(this.who);
            string responseVerbNoun;
            Task<string> replyTask;
            if (!SimpleQuestionIntentCommand.TryGetResponseVerbNoun(this.verb, this.noun, out responseVerbNoun))
            {
                replyTask = new Task<string>(() => "I did not understand your question: QuestionIntent");
                replyTask.Start();
                return replyTask;
            }

            Dictionary<string, Dictionary<string, List<string>>> possibleResponses = new Dictionary
                <string, Dictionary<string, List<string>>>()
            {
                {
                    "what",
                    new Dictionary<string, List<string>>()
                    {
                        {"doing",new List<string>{"reading a book","sleeping","chatting","day dreaming","talking to you"}},
                        {"reading",new List<string>{"a book","your messages","nothing","quora","Love Story by Eric sehgal"}},
                        {"read",new List<string>{"a book","your messages","nothing","quora","Love Story by Eric sehgal"}},
                        {"watching",new List<string>{"chat window","Gangam style","secret","harry potter","sea hawks game"}},
                        {"eating",new List<string>{"electricity","your time :P","CPU"}},
                    }
                },
                {
                    "how",
                    new Dictionary<string, List<string>>()
                    {
                        {"", new List<string>() {"good", "great", "fine", "awesome", "excellent" } },
                        {"doing", new List<string>() {"good", "great", "fine", "awesome", "excellent" } },
                        {"reading", new List<string>() {"by my eyes obviously", "okie dokie", "slow"} }
                    }
                }
            };

            string responseAction = SimpleQuestionIntentCommand.GetResponseAction(this.question, responseVerbNoun, possibleResponses);

            string reply = string.Format("{0} {1} {2}", responseWho, responseVerbNoun, responseAction); // who , verb, noun
            replyTask = new Task<string>(() => reply);
            replyTask.Start();
            return replyTask;
        }

        private static string GetResponseAction(string question, string responseVerbNoun, Dictionary<string, Dictionary<string, List<string>>> possibleResponses)
        {
            try
            {
                return
                    possibleResponses[question][responseVerbNoun][
                        new Random().Next(0, possibleResponses[question][responseVerbNoun].Count)];
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private static bool TryGetResponseVerbNoun(string verb, string noun, out string responseVerbNoun)
        {
            responseVerbNoun = null;
            if (string.IsNullOrEmpty(verb) && string.IsNullOrEmpty(noun))
            {
                return false;
            }

            responseVerbNoun = string.IsNullOrEmpty(noun) ? verb : noun;
            return true;
        }

        private static string GetResponseWho(string who)
        {
            if (who == string.Empty)
            {
                return "I am";
            }

            Dictionary<string, string> whoConverter = new Dictionary<string, string>()
            {
                {"your", "mine"},
                {"you", "I am"},
                {"", "I am"},
                {"my", "your"},
                {"mine", "your"}
            };

            if (whoConverter.ContainsKey(who))
            {
                return whoConverter[who];
            }

            return who;
        }
    }
}