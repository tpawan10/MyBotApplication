// ---------------------------------------------------------------------------
// <copyright file="StorageManager.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ---------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Bot_Application1
{
    public class StorageManager
    {
        private static readonly CloudTable toDoTable;
        private static readonly CloudTable unhandledDataTable;

        static StorageManager()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=taskmanagerbot;AccountKey=HRoXZXAt23QT1Bb9KQOUvRMnsI9exDxBmRlinTUf6842dFNNryjWzhL+c/c+/PzfkCC/bDpmwOo8QTQ6O9Z4oA==");
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            toDoTable = tableClient.GetTableReference("ToDoItems");
            toDoTable.CreateIfNotExists();

            unhandledDataTable = tableClient.GetTableReference("UnhandledQueries");
            unhandledDataTable.CreateIfNotExists();
        }

        public static IEnumerable<ToDoItem> GetAllToDoItemsForUser(string userId)
        {
            TableQuery<ToDoItem> query = new TableQuery<ToDoItem>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, userId));

            return toDoTable.ExecuteQuery(query);
        }

        public static void UpdateToDoItem(ToDoItem itemToUpdate)
        {
            TableOperation updateOperation = TableOperation.Replace(itemToUpdate);
            toDoTable.ExecuteAsync(updateOperation);
        }

        public static void RemoveItem(ToDoItem itemToDelete)
        {
            TableOperation deleteOperation = TableOperation.Delete(itemToDelete);
            toDoTable.ExecuteAsync(deleteOperation);
        }

        public static void InsertItem(ToDoItem item)
        {
            TableOperation addOperation = TableOperation.Insert(item);
            toDoTable.ExecuteAsync(addOperation);
        }

        public static void InsertOrReplaceUnhandledQueryEntity(UnhandledCommandsEntity entity)
        {
            TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(entity);
            unhandledDataTable.ExecuteAsync(insertOrMergeOperation);
        }

        public static IEnumerable<ToDoItem> GetAllToDoItemsToRemind(DateTime timeNow)
        {
            TableQuery<ToDoItem> query = new TableQuery<ToDoItem>()
                .Where(TableQuery.GenerateFilterConditionForDate(
                    "NextRemind",
                    QueryComparisons.LessThanOrEqual,
                    timeNow.Add(Constants.RemindTimerInterval)));

            return toDoTable.ExecuteQuery(query);
        }
    }
}