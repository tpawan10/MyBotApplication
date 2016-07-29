// ---------------------------------------------------------------------------
// <copyright file="ToDoItems.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ---------------------------------------------------------------------------

using System;
using System.Globalization;
using Microsoft.Bot.Connector;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace Bot_Application1
{
    public enum ToDoItemStatus
    {
        Pending,
        Done,
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
        public string Title { get; set; }

        public ToDoItemStatus Status { get; set; }

        public TimeSpan RemindInterval { get; set; }

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

        public ToDoItem(string userId, string title) : this(userId, title, TimeSpan.MaxValue)
        {
        }

        public ToDoItem(string userId, string title, TimeSpan remindInterval)
                : this(userId, title, remindInterval, DateTime.Now.Add(remindInterval))
        {
        }

        public ToDoItem(string userId, string title, DateTime nextRemind, TimeSpan remindInterval)
                : this(userId, title, remindInterval, nextRemind)
        {
        }

        private ToDoItem(string userId, string title, TimeSpan remindInterval, DateTime nextRemind, ToDoItemStatus status = ToDoItemStatus.Pending)
        : base(userId, DateTime.Now.ToString(CultureInfo.CurrentCulture))
        {
            this.Title = title;
            this.Status = status;
            this.RemindInterval = remindInterval;
            this.NextRemind = nextRemind;
        }

        public void UpdateStatus(ToDoItemStatus status)
        {
            this.Status = status;
        }

        public void SetNextRemind()
        {
            this.NextRemind = this.RemindInterval == TimeSpan.MaxValue ? DateTime.MaxValue : this.NextRemind.Add(this.RemindInterval);
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