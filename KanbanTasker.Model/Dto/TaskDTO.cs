﻿using System;

namespace KanbanTasker.Model.Dto
{
    // A data transfer object for a task
    public class TaskDto
    {
        public int Id { get; set; }
        public int BoardId { get; set; }           // if every task is associate with a board than this should not be nullable.
        public string DateCreated { get; set; }
        public string DueDate { get; set; }
        public string StartDate { get; set; }
        public string FinishDate { get; set; }
        public string TimeDue { get; set; }
        public string ReminderTime { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public int ColumnIndex { get; set; }
        public string ColorKey { get; set; }
        public string Tags { get; set; }

        public virtual BoardDto Board { get; set; }
    }
}