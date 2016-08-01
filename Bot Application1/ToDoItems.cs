// ---------------------------------------------------------------------------
// <copyright file="ToDoItems.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ---------------------------------------------------------------------------

using System;
using Microsoft.Bot.Connector;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace Bot_Application1
{
    public enum ToDoItemStatus
    {
        Pending,
        Done
    }

    public class CommunicationInfo
    {
        [JsonProperty(PropertyName = "uri")]
        public string ServiceUri { get; set; }

        [JsonProperty(PropertyName = "recipient")]
        public ChannelAccount Recipient { get; set; }

        [JsonProperty(PropertyName = "from")]
        public ChannelAccount From { get; set; }
    }

    public class ToDoItem : TableEntity
    {
        private ToDoItemStatus status;

        public string Title { get; set; }

        public string Status
        {
            get { return this.status.ToString(); }
            set
            {
                if (!Enum.TryParse(value, out this.status))
                {
                    throw new InvalidOperationException();
                }
            }
        }

        public int RemindIntervalInMins { get; set; }

        public DateTime NextRemind { get; set; }

        public CommunicationInfo CommunicationInformation
        {
            get { return this.commInfo; }
            set
            {
                this.commInfoJson = JsonConvert.SerializeObject(value);
                this.commInfo = value;
            }
        }

        private string commInfoJson;
        private CommunicationInfo commInfo;

        public string CommunicationInformationJson
        {
            get { return this.commInfoJson; }
            set
            {
                if (value != null)
                {
                    this.commInfo = JsonConvert.DeserializeObject<CommunicationInfo>(value);
                    this.commInfoJson = value;
                }
            }
        }

        public ToDoItem()
        {
        }

        public ToDoItem(string userId, string title) : this(userId, title, -1)
        {
        }

        public ToDoItem(string userId, string title, int remindIntervalInMins)
                : this(userId, title, remindIntervalInMins, ToDoItem.GetNextRemindTime(DateTime.Now, remindIntervalInMins))
        {
        }

        public ToDoItem(string userId, string title, int remindIntervalInMins, DateTime nextRemind, ToDoItemStatus status = ToDoItemStatus.Pending)
        : base(userId, DateTime.Now.ToBinary().ToString())
        {
            this.Title = title;
            this.status = status;
            this.RemindIntervalInMins = remindIntervalInMins;
            this.NextRemind = nextRemind;
        }

        public void UpdateStatus(ToDoItemStatus status)
        {
            this.status = status;
        }

        public void SetNextRemind()
        {
            this.NextRemind = ToDoItem.GetNextRemindTime(this.NextRemind, this.RemindIntervalInMins);
        }

        public static DateTime GetNextRemindTime(DateTime originalDateTime, int remindIntervalInMins)
        {
            return remindIntervalInMins == -1
                ? DateTime.MaxValue
                : originalDateTime.Add(TimeSpan.FromMinutes(remindIntervalInMins));
        }

        public void SetCommunicationInformation(string url, ChannelAccount from, ChannelAccount recipient)
        {
            this.SetCommunicationInformation(
                new CommunicationInfo()
                {
                    ServiceUri = url,
                    From = from,
                    Recipient = recipient
                });
        }

        public void SetCommunicationInformation(CommunicationInfo communicationInfo)
        {
            this.CommunicationInformation = communicationInfo;
        }
    }
}