// ---------------------------------------------------------------------------
// <copyright file="ToDoItems.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ---------------------------------------------------------------------------

using System;
using Microsoft.Bot.Connector;

namespace Bot_Application1
{
    public enum ToDoItemStatus
    {
        Pending,
        Done,
    }

    public class CommunicationInfo
    {
        public string ServiceUri { get; set; }
        public ChannelAccount Recipient { get; set; }
        public ChannelAccount From { get; set; }
    }

    public class ToDoItem
    {
        public string Title { get; set; }
        public ToDoItemStatus Status { get; set; }

        public TimeSpan RemindInterval { get; set; }

        public DateTime NextRemind { get; set; }

        public CommunicationInfo CommunicationInformation { get; private set; }

        public ToDoItem()
        {
        }

        public ToDoItem(string title) : this(title, TimeSpan.MaxValue)
        {
        }

        public ToDoItem(string title, TimeSpan remindInterval)
                : this(title, remindInterval, DateTime.Now.Add(remindInterval))
        {
        }

        public ToDoItem(string title, DateTime nextRemind, TimeSpan remindInterval)
                : this(title, remindInterval, nextRemind)
        {
        }

        private ToDoItem(string title, TimeSpan remindInterval, DateTime nextRemind, ToDoItemStatus status = ToDoItemStatus.Pending)
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